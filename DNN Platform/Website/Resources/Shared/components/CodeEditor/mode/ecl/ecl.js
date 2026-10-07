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

CodeMirror.defineMode("ecl", function(config) {

  function words(str) {
	logger.error(`>>> Entering words(str=${str})`, { "event.action": "words", "logrite.str": str });
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.error(`words(str=${str}): obj → ${obj}`, { "event.action": "words", "logrite.str": str });
	logger.warn(`<<< Exiting words(str=${str})`, { "event.action": "words", "logrite.str": str });
    return obj;
  }

  function metaHook(stream, state) {
	logger.error(`>>> Entering metaHook(stream=${stream},state=${state})`, { "event.action": "metaHook", "logrite.stream": stream, "logrite.state": state });
    if (!state.startOfLine) return false;
    stream.skipToEnd();
	logger.warn(`<<< Exiting metaHook(stream=${stream},state=${state})`, { "event.action": "metaHook", "logrite.stream": stream, "logrite.state": state });
    return "meta";
  }

  var indentUnit = config.indentUnit;
  var keyword = words("abs acos allnodes ascii asin asstring atan atan2 ave case choose choosen choosesets clustersize combine correlation cos cosh count covariance cron dataset dedup define denormalize distribute distributed distribution ebcdic enth error evaluate event eventextra eventname exists exp failcode failmessage fetch fromunicode getisvalid global graph group hash hash32 hash64 hashcrc hashmd5 having if index intformat isvalid iterate join keyunicode length library limit ln local log loop map matched matchlength matchposition matchtext matchunicode max merge mergejoin min nolocal nonempty normalize parse pipe power preload process project pull random range rank ranked realformat recordof regexfind regexreplace regroup rejected rollup round roundup row rowdiff sample set sin sinh sizeof soapcall sort sorted sqrt stepped stored sum table tan tanh thisnode topn tounicode transfer trim truncate typeof ungroup unicodeorder variance which workunit xmldecode xmlencode xmltext xmlunicode");
  var variable = words("apply assert build buildindex evaluate fail keydiff keypatch loadxml nothor notify output parallel sequential soapcall wait");
  var variable_2 = words("__compressed__ all and any as atmost before beginc++ best between case const counter csv descend encrypt end endc++ endmacro except exclusive expire export extend false few first flat from full function group header heading hole ifblock import in interface joined keep keyed last left limit load local locale lookup macro many maxcount maxlength min skew module named nocase noroot noscan nosort not of only opt or outer overwrite packed partition penalty physicallength pipe quote record relationship repeat return right scan self separator service shared skew skip sql store terminator thor threshold token transform trim true type unicodeorder unsorted validate virtual whole wild within xml xpath");
  var variable_3 = words("ascii big_endian boolean data decimal ebcdic integer pattern qstring real record rule set of string token udecimal unicode unsigned varstring varunicode");
  var builtin = words("checkpoint deprecated failcode failmessage failure global independent onwarning persist priority recovery stored success wait when");
  var blockKeywords = words("catch class do else finally for if switch try while");
  var atoms = words("true false null");
  var hooks = {"#": metaHook};
  var isOperatorChar = /[+\-*&%=<>!?|\/]/;

  var curPunc;

  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
    if (hooks[ch]) {
	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      var result = hooks[ch](stream, state);
	logger.error(`tokenBase(stream=${stream},state=${state}): result → ${result}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      if (result !== false) return result;
    }
    if (ch == '"' || ch == "'") {
      state.tokenize = tokenString(ch);
      return state.tokenize(stream, state);
    }
    if (/[\[\]{}\(\),;\:\.]/.test(ch)) {
      curPunc = ch;
      return null;
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
      stream.eatWhile(isOperatorChar);
      return "operator";
    }
    stream.eatWhile(/[\w\$_]/);
    var cur = stream.current().toLowerCase();
	logger.error(`tokenBase(stream=${stream},state=${state}): cur → ${cur}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    if (keyword.propertyIsEnumerable(cur)) {
      if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
      return "keyword";
    } else if (variable.propertyIsEnumerable(cur)) {
      if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
      return "variable";
    } else if (variable_2.propertyIsEnumerable(cur)) {
      if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
      return "variable-2";
    } else if (variable_3.propertyIsEnumerable(cur)) {
      if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
      return "variable-3";
    } else if (builtin.propertyIsEnumerable(cur)) {
      if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
      return "builtin";
    } else { //Data types are of from KEYWORD##
                var i = cur.length - 1;
                while(i >= 0 && (!isNaN(cur[i]) || cur[i] == '_'))
                        --i;

                if (i > 0) {
                        var cur2 = cur.substr(0, i + 1);
						logger.error(`tokenBase(stream=${stream},state=${state}): cur2 → ${cur2}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
                if (variable_3.propertyIsEnumerable(cur2)) {
                        if (blockKeywords.propertyIsEnumerable(cur2)) curPunc = "newstatement";
                        return "variable-3";
                }
            }
    }
    if (atoms.propertyIsEnumerable(cur)) return "atom";
	logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    return null;
  }

  function tokenString(quote) {
	logger.error(`>>> Entering tokenString(quote=${quote})`, { "event.action": "tokenString", "logrite.quote": quote });
    return function(stream, state) {
      var escaped = false, next, end = false;
      while ((next = stream.next()) != null) {
        if (next == quote && !escaped) {end = true; break;}
        escaped = !escaped && next == "\\";
      }
      if (end || !escaped)
        state.tokenize = tokenBase;
      return "string";
    };
  }

  function tokenComment(stream, state) {
	logger.error(`>>> Entering tokenComment(stream=${stream},state=${state})`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
    var maybeEnd = false, ch;
    while (ch = stream.next()) {
      if (ch == "/" && maybeEnd) {
        state.tokenize = tokenBase;
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
	logger.warn(`<<< Exiting pushContext(state=${state},col=${col},type=${type})`, { "event.action": "pushContext", "logrite.state": state, "logrite.col": col, "logrite.type": type });
    return state.context = new Context(state.indented, col, type, null, state.context);
  }
  function popContext(state) {
	logger.error(`>>> Entering popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
    var t = state.context.type;
    if (t == ")" || t == "]" || t == "}")
      state.indented = state.context.indented;
	logger.warn(`<<< Exiting popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
    return state.context = state.context.prev;
  }

  // Interface

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
      if (style == "comment" || style == "meta") return style;
      if (ctx.align == null) ctx.align = true;

	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if ((curPunc == ";" || curPunc == ":") && ctx.type == "statement") popContext(state);
      else if (curPunc == "{") pushContext(state, stream.column(), "}");
      else if (curPunc == "[") pushContext(state, stream.column(), "]");
      else if (curPunc == "(") pushContext(state, stream.column(), ")");
      else if (curPunc == "}") {
        while (ctx.type == "statement") ctx = popContext(state);
        if (ctx.type == "}") ctx = popContext(state);
        while (ctx.type == "statement") ctx = popContext(state);
      }
      else if (curPunc == ctx.type) popContext(state);
      else if (ctx.type == "}" || ctx.type == "top" || (ctx.type == "statement" && curPunc == "newstatement"))
        pushContext(state, stream.column(), "statement");
      state.startOfLine = false;
      return style;
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      if (state.tokenize != tokenBase && state.tokenize != null) return 0;
      var ctx = state.context, firstChar = textAfter && textAfter.charAt(0);
	logger.error(`indent(state=${state},textAfter=${textAfter}): ctx → ${ctx}`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      if (ctx.type == "statement" && firstChar == "}") ctx = ctx.prev;
      var closing = firstChar == ctx.type;
      if (ctx.type == "statement") return ctx.indented + (firstChar == "{" ? 0 : indentUnit);
      else if (ctx.align) return ctx.column + (closing ? 0 : 1);
      else return ctx.indented + (closing ? 0 : indentUnit);
    },

    electricChars: "{}"
  };
});

CodeMirror.defineMIME("text/x-ecl", "ecl");

});
