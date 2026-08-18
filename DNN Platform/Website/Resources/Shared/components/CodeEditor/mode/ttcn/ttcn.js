const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineMode("ttcn", function(config, parserConfig) {
	logger.debug('>>> Entering defineMode()');
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
	logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
      var ch = stream.next();

	logger.debug(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`);
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
		logger.debug(`function(): state.tokenize → ${state.tokenize}`);
		logger.debug(`defineMode(): state.tokenize → ${state.tokenize}`);
          return tokenComment(stream, state);
        }
        if (stream.eat("/")) {
		logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
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
	logger.debug(`tokenBase(stream=${stream},state=${state}): cur → ${cur}`);

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

	logger.debug(`<<< Exiting tokenBase(stream=${stream},state=${state})`);
      return "variable";
    }

    function tokenString(quote) {
	logger.debug(`>>> Entering tokenString(quote=${quote})`);
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
		logger.debug(`<<< Exiting tokenString(quote=${quote})`);
        return "string";
      };
    }

    function tokenComment(stream, state) {
	logger.debug(`>>> Entering tokenComment(stream=${stream},state=${state})`);
      var maybeEnd = false, ch;
	logger.debug(`function(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
      while (ch = stream.next()) {
		logger.debug(`tokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
        if (ch == "/" && maybeEnd) {
          state.tokenize = null;
          break;
        }
        maybeEnd = (ch == "*");
      }
	logger.debug(`tokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting tokenComment(stream=${stream},state=${state})`);
      return "comment";
    }

    function Context(indented, column, type, align, prev) {
	logger.debug(`>>> Entering Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev})`);
      this.indented = indented;
      this.column = column;
	logger.debug(`function(): this.indented → ${this.indented}`);
      this.type = type;
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): indented → ${indented}`);
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): type → ${type}`);
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): this.indented → ${this.indented}`);
      this.align = align;
      this.prev = prev;
	logger.debug(`<<< Exiting Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev})`);
    }

    function pushContext(state, col, type) {
	logger.debug(`>>> Entering pushContext(state=${state},col=${col},type=${type})`);
      var indent = state.indented;
	logger.debug(`function(): this.column → ${this.column}`);
      if (state.context && state.context.type == "statement")
        indent = state.context.indented;
	logger.debug(`<<< Exiting pushContext(state=${state},col=${col},type=${type})`);
      return state.context = new Context(indent, col, type, null, state.context);
    }

    function popContext(state) {
	logger.debug(`>>> Entering popContext(state=${state})`);
      var t = state.context.type;
      if (t == ")" || t == "]" || t == "}")
        state.indented = state.context.indented;
	logger.debug(`<<< Exiting popContext(state=${state})`);
	logger.debug(`function(): this.prev → ${this.prev}`);
      return state.context = state.context.prev;
    }

    //Interface
    return {
      startState: function(basecolumn) {
		logger.debug(`>>> Entering startState(basecolumn=${basecolumn})`);
		logger.debug('<<< Exiting defineMode()');
        return {
          tokenize: null,
          context: new Context((basecolumn || 0) - indentUnit, 0, "top", false),
          indented: 0,
          startOfLine: true
        };
		logger.debug(`<<< Exiting startState(basecolumn=${basecolumn})`);
      },

      token: function(stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
        var ctx = state.context;
        if (stream.sol()) {
          if (ctx.align == null) ctx.align = false;
          state.indented = stream.indentation();
          state.startOfLine = true;
		logger.debug(`function(): state.indented → ${state.indented}`);
        }
        if (stream.eatSpace()) return null;
        curPunc = null;
		logger.debug(`defineMode(): this.column → ${this.column}`);
        var style = (state.tokenize || tokenBase)(stream, state);
		logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
        if (style == "comment") return style;
        if (ctx.align == null) ctx.align = true;
		logger.debug(`defineMode(): curPunc → ${curPunc}`);
		logger.debug(`defineMode(): this.indented → ${this.indented}`);

        if ((curPunc == ";" || curPunc == ":" || curPunc == ",")
            && ctx.type == "statement"){
          popContext(state);
		logger.debug(`function(): state.startOfLine → ${state.startOfLine}`);
        }
        else if (curPunc == "{") pushContext(state, stream.column(), "}");
        else if (curPunc == "[") pushContext(state, stream.column(), "]");
        else if (curPunc == "(") pushContext(state, stream.column(), ")");
        else if (curPunc == "}") {
		logger.debug(`function(): this.type → ${this.type}`);
          while (ctx.type == "statement") ctx = popContext(state);
		logger.debug(`defineMode(): this.type → ${this.type}`);
          if (ctx.type == "}") ctx = popContext(state);
		logger.debug(`function(): this.align → ${this.align}`);
          while (ctx.type == "statement") ctx = popContext(state);
		logger.debug(`defineMode(): this.align → ${this.align}`);
        }
        else if (curPunc == ctx.type) popContext(state);
        else if (indentStatements &&
            (((ctx.type == "}" || ctx.type == "top") && curPunc != ';') ||
            (ctx.type == "statement" && curPunc == "newstatement")))
          pushContext(state, stream.column(), "statement");

		logger.debug(`defineMode(): this.prev → ${this.prev}`);
        state.startOfLine = false;

		logger.debug(`defineMode(): state.indented → ${state.indented}`);
		logger.debug(`function(): mode.helperType → ${mode.helperType}`);
		logger.debug(`<<< Exiting function(exports=${exports})`);
        return style;
      },

      electricChars: "{}",
      blockCommentStart: "/*",
      blockCommentEnd: "*/",
      lineComment: "//",
      fold: "brace"
    };
  });
logger.debug(`defineMode(): state.startOfLine → ${state.startOfLine}`);

  function words(str) {
	logger.debug(`>>> Entering words(str=${str})`);
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.debug(`words(str=${str}): obj → ${obj}`);
	logger.debug(`<<< Exiting words(str=${str})`);
    return obj;
  }

  function def(mimes, mode) {
	logger.debug(`>>> Entering def(mimes=${mimes},mode=${mode})`);
    if (typeof mimes == "string") mimes = [mimes];
    var words = [];
    function add(obj) {
      if (obj) for (var prop in obj) if (obj.hasOwnProperty(prop))
        words.push(prop);
		logger.debug(`<<< Exiting add(obj=${obj})`);
    }

    add(mode.keywords);
    add(mode.builtin);
    add(mode.timerOps);
    add(mode.portOps);

    if (words.length) {
      mode.helperType = mimes[0];
      CodeMirror.registerHelper("hintWords", mimes[0], words);
	logger.debug(`def(mimes=${mimes},mode=${mode}): mode.helperType → ${mode.helperType}`);
    }

    for (var i = 0; i < mimes.length; ++i)
	logger.debug(`<<< Exiting def(mimes=${mimes},mode=${mode})`);
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
