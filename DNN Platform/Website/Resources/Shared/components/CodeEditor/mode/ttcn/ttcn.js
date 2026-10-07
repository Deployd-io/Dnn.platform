const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineMode("ttcn", function(config, parserConfig) {
    var indentUnit = config.indentUnit,
        keywords = parserConfig.keywords || {},
        builtin = parserConfig.builtin || {},
        timerOps = parserConfig.timerOps || {},
        portOps  = parserConfig.portOps || {},
        configOps = parserConfig.configOps || {},
        verdictOps = parserConfig.verdictOps || {},
        sutOps = parserConfig.sutOps || {},
        functionOps = parserConfig.functionOps || {},

        verdictConsts = parserConfig.verdictConsts || {},
        booleanConsts = parserConfig.booleanConsts || {},
        otherConsts   = parserConfig.otherConsts || {},

        types = parserConfig.types || {},
        visibilityModifiers = parserConfig.visibilityModifiers || {},
        templateMatch = parserConfig.templateMatch || {},
        multiLineStrings = parserConfig.multiLineStrings,
        indentStatements = parserConfig.indentStatements !== false;
    var isOperatorChar = /[+\-*&@=<>!\/]/;
    var curPunc;

    function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      var ch = stream.next();

	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      if (ch == '"' || ch == "'") {
        state.tokenize = tokenString(ch);
        return state.tokenize(stream, state);
      }
      if (/[\[\]{}\(\),;\\:\?\.]/.test(ch)) {
        curPunc = ch;
        return "punctuation";
      }
      if (ch == "#"){
        stream.skipToEnd();
        return "atom preprocessor";
      }
      if (ch == "%"){
        stream.eatWhile(/\b/);
        return "atom ttcn3Macros";
      }
      if (/\d/.test(ch)) {
        stream.eatWhile(/[\w\.]/);
        return "number";
      }
      if (ch == "/") {
        if (stream.eat("*")) {
          state.tokenize = tokenComment;
		logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
          return tokenComment(stream, state);
        }
        if (stream.eat("/")) {
          stream.skipToEnd();
          return "comment";
        }
      }
      if (isOperatorChar.test(ch)) {
        if(ch == "@"){
          if(stream.match("try") || stream.match("catch")
              || stream.match("lazy")){
            return "keyword";
          }
        }
        stream.eatWhile(isOperatorChar);
        return "operator";
      }
      stream.eatWhile(/[\w\$_\xa1-\uffff]/);
      var cur = stream.current();
	logger.error(`tokenBase(stream=${stream},state=${state}): cur → ${cur}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });

      if (keywords.propertyIsEnumerable(cur)) return "keyword";
      if (builtin.propertyIsEnumerable(cur)) return "builtin";

      if (timerOps.propertyIsEnumerable(cur)) return "def timerOps";
      if (configOps.propertyIsEnumerable(cur)) return "def configOps";
      if (verdictOps.propertyIsEnumerable(cur)) return "def verdictOps";
      if (portOps.propertyIsEnumerable(cur)) return "def portOps";
      if (sutOps.propertyIsEnumerable(cur)) return "def sutOps";
      if (functionOps.propertyIsEnumerable(cur)) return "def functionOps";

      if (verdictConsts.propertyIsEnumerable(cur)) return "string verdictConsts";
      if (booleanConsts.propertyIsEnumerable(cur)) return "string booleanConsts";
      if (otherConsts.propertyIsEnumerable(cur)) return "string otherConsts";

      if (types.propertyIsEnumerable(cur)) return "builtin types";
      if (visibilityModifiers.propertyIsEnumerable(cur))
        return "builtin visibilityModifiers";
      if (templateMatch.propertyIsEnumerable(cur)) return "atom templateMatch";

	logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      return "variable";
    }

    function tokenString(quote) {
	logger.error(`>>> Entering tokenString(quote=${quote})`, { "event.action": "tokenString", "logrite.quote": quote });
      return function(stream, state) {
        var escaped = false, next, end = false;
        while ((next = stream.next()) != null) {
          if (next == quote && !escaped){
            var afterQuote = stream.peek();
            //look if the character after the quote is like the B in '10100010'B
            if (afterQuote){
              afterQuote = afterQuote.toLowerCase();
              if(afterQuote == "b" || afterQuote == "h" || afterQuote == "o")
                stream.next();
            }
            end = true; break;
          }
          escaped = !escaped && next == "\\";
        }
        if (end || !(escaped || multiLineStrings))
          state.tokenize = null;
        return "string";
      };
    }

    function tokenComment(stream, state) {
	logger.error(`>>> Entering tokenComment(stream=${stream},state=${state})`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
      var maybeEnd = false, ch;
      while (ch = stream.next()) {
        if (ch == "/" && maybeEnd) {
          state.tokenize = null;
		logger.debug(`tokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
          break;
        }
        maybeEnd = (ch == "*");
      }
	logger.error(`tokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
      return "comment";
    }

    function Context(indented, column, type, align, prev) {
	logger.error(`>>> Entering Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev})`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
      this.indented = indented;
      this.column = column;
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): indented → ${indented}`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): this.indented → ${this.indented}`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): column → ${column}`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): type → ${type}`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
      this.type = type;
      this.align = align;
      this.prev = prev;
    }

    function pushContext(state, col, type) {
	logger.error(`>>> Entering pushContext(state=${state},col=${col},type=${type})`, { "event.action": "pushContext", "logrite.state": state, "logrite.col": col, "logrite.type": type });
      var indent = state.indented;
      if (state.context && state.context.type == "statement")
        indent = state.context.indented;
	logger.warn(`<<< Exiting pushContext(state=${state},col=${col},type=${type})`, { "event.action": "pushContext", "logrite.state": state, "logrite.col": col, "logrite.type": type });
      return state.context = new Context(indent, col, type, null, state.context);
    }

    function popContext(state) {
	logger.error(`>>> Entering popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
      var t = state.context.type;
      if (t == ")" || t == "]" || t == "}")
        state.indented = state.context.indented;
	logger.warn(`<<< Exiting popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
      return state.context = state.context.prev;
    }

    //Interface
    return {
      startState: function(basecolumn) {
		logger.error(`>>> Entering startState(basecolumn=${basecolumn})`, { "event.action": "startState", "logrite.basecolumn": basecolumn });
        return {
          tokenize: null,
          context: new Context((basecolumn || 0) - indentUnit, 0, "top", false),
          indented: 0,
          startOfLine: true
        };
		logger.warn(`<<< Exiting startState(basecolumn=${basecolumn})`, { "event.action": "startState", "logrite.basecolumn": basecolumn });
      },

      token: function(stream, state) {
		logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        var ctx = state.context;
        if (stream.sol()) {
          if (ctx.align == null) ctx.align = false;
          state.indented = stream.indentation();
          state.startOfLine = true;
		logger.debug(`token(stream=${stream},state=${state}): state.startOfLine → ${state.startOfLine}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        }
        if (stream.eatSpace()) return null;
        curPunc = null;
        var style = (state.tokenize || tokenBase)(stream, state);
		logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        if (style == "comment") return style;
        if (ctx.align == null) ctx.align = true;

        if ((curPunc == ";" || curPunc == ":" || curPunc == ",")
            && ctx.type == "statement"){
          popContext(state);
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        }
        else if (curPunc == "{") pushContext(state, stream.column(), "}");
        else if (curPunc == "[") pushContext(state, stream.column(), "]");
        else if (curPunc == "(") pushContext(state, stream.column(), ")");
        else if (curPunc == "}") {
          while (ctx.type == "statement") ctx = popContext(state);
          if (ctx.type == "}") ctx = popContext(state);
          while (ctx.type == "statement") ctx = popContext(state);
        }
        else if (curPunc == ctx.type) popContext(state);
        else if (indentStatements &&
            (((ctx.type == "}" || ctx.type == "top") && curPunc != ';') ||
            (ctx.type == "statement" && curPunc == "newstatement")))
          pushContext(state, stream.column(), "statement");

        state.startOfLine = false;

        return style;
      },

      electricChars: "{}",
      blockCommentStart: "/*",
      blockCommentEnd: "*/",
      lineComment: "//",
      fold: "brace"
    };
  });

  function words(str) {
	logger.error(`>>> Entering words(str=${str})`, { "event.action": "words", "logrite.str": str });
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.error(`words(str=${str}): obj → ${obj}`, { "event.action": "words", "logrite.str": str });
	logger.warn(`<<< Exiting words(str=${str})`, { "event.action": "words", "logrite.str": str });
    return obj;
  }

  function def(mimes, mode) {
	logger.error(`>>> Entering def(mimes=${mimes},mode=${mode})`, { "event.action": "def", "logrite.mimes": mimes, "logrite.mode": mode });
    if (typeof mimes == "string") mimes = [mimes];
    var words = [];
    function add(obj) {
	logger.error(`>>> Entering add(obj=${obj})`, { "event.action": "add", "logrite.obj": obj });
      if (obj) for (var prop in obj) if (obj.hasOwnProperty(prop))
        words.push(prop);
		logger.warn(`<<< Exiting add(obj=${obj})`, { "event.action": "add", "logrite.obj": obj });
    }

    add(mode.keywords);
    add(mode.builtin);
    add(mode.timerOps);
    add(mode.portOps);

    if (words.length) {
      mode.helperType = mimes[0];
	logger.debug(`def(mimes=${mimes},mode=${mode}): mode.helperType → ${mode.helperType}`, { "event.action": "def", "logrite.mimes": mimes, "logrite.mode": mode });
      CodeMirror.registerHelper("hintWords", mimes[0], words);
    }

    for (var i = 0; i < mimes.length; ++i)
      CodeMirror.defineMIME(mimes[i], mode);
  }

  def(["text/x-ttcn", "text/x-ttcn3", "text/x-ttcnpp"], {
    name: "ttcn",
    keywords: words("activate address alive all alt altstep and and4b any" +
    " break case component const continue control deactivate" +
    " display do else encode enumerated except exception" +
    " execute extends extension external for from function" +
    " goto group if import in infinity inout interleave" +
    " label language length log match message mixed mod" +
    " modifies module modulepar mtc noblock not not4b nowait" +
    " of on optional or or4b out override param pattern port" +
    " procedure record recursive rem repeat return runs select" +
    " self sender set signature system template testcase to" +
    " type union value valueof var variant while with xor xor4b"),
    builtin: words("bit2hex bit2int bit2oct bit2str char2int char2oct encvalue" +
    " decomp decvalue float2int float2str hex2bit hex2int" +
    " hex2oct hex2str int2bit int2char int2float int2hex" +
    " int2oct int2str int2unichar isbound ischosen ispresent" +
    " isvalue lengthof log2str oct2bit oct2char oct2hex oct2int" +
    " oct2str regexp replace rnd sizeof str2bit str2float" +
    " str2hex str2int str2oct substr unichar2int unichar2char" +
    " enum2int"),
    types: words("anytype bitstring boolean char charstring default float" +
    " hexstring integer objid octetstring universal verdicttype timer"),
    timerOps: words("read running start stop timeout"),
    portOps: words("call catch check clear getcall getreply halt raise receive" +
    " reply send trigger"),
    configOps: words("create connect disconnect done kill killed map unmap"),
    verdictOps: words("getverdict setverdict"),
    sutOps: words("action"),
    functionOps: words("apply derefers refers"),

    verdictConsts: words("error fail inconc none pass"),
    booleanConsts: words("true false"),
    otherConsts: words("null NULL omit"),

    visibilityModifiers: words("private public friend"),
    templateMatch: words("complement ifpresent subset superset permutation"),
    multiLineStrings: true
  });
});
