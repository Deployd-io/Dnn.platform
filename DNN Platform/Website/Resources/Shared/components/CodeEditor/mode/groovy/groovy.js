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

CodeMirror.defineMode("groovy", function(config) {
  function words(str) {
	logger.error(`>>> Entering words(str=${str})`, { "event.action": "words", "logrite.str": str });
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.error(`words(str=${str}): obj → ${obj}`, { "event.action": "words", "logrite.str": str });
	logger.warn(`<<< Exiting words(str=${str})`, { "event.action": "words", "logrite.str": str });
    return obj;
  }
  var keywords = words(
    "abstract as assert boolean break byte case catch char class const continue def default " +
    "do double else enum extends final finally float for goto if implements import in " +
    "instanceof int interface long native new package private protected public return " +
    "short static strictfp super switch synchronized threadsafe throw throws transient " +
    "try void volatile while");
  var blockKeywords = words("catch class do else finally for if switch try while enum interface def");
  var standaloneKeywords = words("return break continue");
  var atoms = words("null true false this");

  var curPunc;
  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
    if (ch == '"' || ch == "'") {
	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      return startString(ch, stream, state);
    }
    if (/[\[\]{}\(\),;\:\.]/.test(ch)) {
      curPunc = ch;
      return null;
    }
    if (/\d/.test(ch)) {
      stream.eatWhile(/[\w\.]/);
      if (stream.eat(/eE/)) { stream.eat(/\+\-/); stream.eatWhile(/\d/); }
      return "number";
    }
    if (ch == "/") {
      if (stream.eat("*")) {
        state.tokenize.push(tokenComment);
        return tokenComment(stream, state);
      }
      if (stream.eat("/")) {
        stream.skipToEnd();
        return "comment";
      }
      if (expectExpression(state.lastToken, false)) {
        return startString(ch, stream, state);
      }
    }
    if (ch == "-" && stream.eat(">")) {
      curPunc = "->";
      return null;
    }
    if (/[+\-*&%=<>!?|\/~]/.test(ch)) {
      stream.eatWhile(/[+\-*&%=<>|~]/);
      return "operator";
    }
    stream.eatWhile(/[\w\$_]/);
    if (ch == "@") { stream.eatWhile(/[\w\$_\.]/); return "meta"; }
    if (state.lastToken == ".") return "property";
    if (stream.eat(":")) { curPunc = "proplabel"; return "property"; }
    var cur = stream.current();
	logger.error(`tokenBase(stream=${stream},state=${state}): cur → ${cur}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    if (atoms.propertyIsEnumerable(cur)) { return "atom"; }
    if (keywords.propertyIsEnumerable(cur)) {
      if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
      else if (standaloneKeywords.propertyIsEnumerable(cur)) curPunc = "standalone";
      return "keyword";
    }
	logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    return "variable";
  }
  tokenBase.isBase = true;

  function startString(quote, stream, state) {
	logger.error(`>>> Entering startString(quote=${quote},stream=${stream},state=${state})`, { "event.action": "startString", "logrite.quote": quote, "logrite.stream": stream, "logrite.state": state });
    var tripleQuoted = false;
    if (quote != "/" && stream.eat(quote)) {
      if (stream.eat(quote)) tripleQuoted = true;
      else return "string";
    }
    function t(stream, state) {
	logger.error(`>>> Entering t(stream=${stream},state=${state})`, { "event.action": "t", "logrite.stream": stream, "logrite.state": state });
      var escaped = false, next, end = !tripleQuoted;
      while ((next = stream.next()) != null) {
        if (next == quote && !escaped) {
          if (!tripleQuoted) { break; }
          if (stream.match(quote + quote)) { end = true; break; }
        }
        if (quote == '"' && next == "$" && !escaped && stream.eat("{")) {
          state.tokenize.push(tokenBaseUntilBrace());
          return "string";
        }
        escaped = !escaped && next == "\\";
      }
      if (end) state.tokenize.pop();
      return "string";
    }
    state.tokenize.push(t);
	logger.warn(`<<< Exiting t(stream=${stream},state=${state})`, { "event.action": "t", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting startString(quote=${quote},stream=${stream},state=${state})`, { "event.action": "startString", "logrite.quote": quote, "logrite.stream": stream, "logrite.state": state });
    return t(stream, state);
  }

  function tokenBaseUntilBrace() {
	logger.error('>>> Entering tokenBaseUntilBrace()', { "event.action": "tokenBaseUntilBrace" });
    var depth = 1;
    function t(stream, state) {
      if (stream.peek() == "}") {
        depth--;
        if (depth == 0) {
          state.tokenize.pop();
          return state.tokenize[state.tokenize.length-1](stream, state);
        }
      } else if (stream.peek() == "{") {
        depth++;
      }
      return tokenBase(stream, state);
    }
    t.isBase = true;
	logger.warn('<<< Exiting tokenBaseUntilBrace()', { "event.action": "tokenBaseUntilBrace" });
    return t;
  }

  function tokenComment(stream, state) {
	logger.error(`>>> Entering tokenComment(stream=${stream},state=${state})`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
    var maybeEnd = false, ch;
    while (ch = stream.next()) {
      if (ch == "/" && maybeEnd) {
        state.tokenize.pop();
        break;
      }
      maybeEnd = (ch == "*");
    }
	logger.error(`tokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
    return "comment";
  }

  function expectExpression(last, newline) {
	logger.error(`>>> Entering expectExpression(last=${last},newline=${newline})`, { "event.action": "expectExpression", "logrite.last": last, "logrite.newline": newline });
    return !last || last == "operator" || last == "->" || /[\.\[\{\(,;:]/.test(last) ||
      last == "newstatement" || last == "keyword" || last == "proplabel" ||
      (last == "standalone" && !newline);
	logger.warn(`<<< Exiting expectExpression(last=${last},newline=${newline})`, { "event.action": "expectExpression", "logrite.last": last, "logrite.newline": newline });
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
        tokenize: [tokenBase],
        context: new Context((basecolumn || 0) - config.indentUnit, 0, "top", false),
        indented: 0,
        startOfLine: true,
        lastToken: null
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
        // Automatic semicolon insertion
        if (ctx.type == "statement" && !expectExpression(state.lastToken, true)) {
          popContext(state); ctx = state.context;
        }
      }
      if (stream.eatSpace()) return null;
      curPunc = null;
      var style = state.tokenize[state.tokenize.length-1](stream, state);
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (style == "comment") return style;
      if (ctx.align == null) ctx.align = true;

      if ((curPunc == ";" || curPunc == ":") && ctx.type == "statement") popContext(state);
      // Handle indentation for {x -> \n ... }
      else if (curPunc == "->" && ctx.type == "statement" && ctx.prev.type == "}") {
        popContext(state);
        state.context.align = false;
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
      else if (ctx.type == "}" || ctx.type == "top" || (ctx.type == "statement" && curPunc == "newstatement"))
        pushContext(state, stream.column(), "statement");
      state.startOfLine = false;
      state.lastToken = curPunc || style;
      return style;
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      if (!state.tokenize[state.tokenize.length-1].isBase) return 0;
      var firstChar = textAfter && textAfter.charAt(0), ctx = state.context;
	logger.error(`indent(state=${state},textAfter=${textAfter}): firstChar → ${firstChar}`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      if (ctx.type == "statement" && !expectExpression(state.lastToken, true)) ctx = ctx.prev;
      var closing = firstChar == ctx.type;
      if (ctx.type == "statement") return ctx.indented + (firstChar == "{" ? 0 : config.indentUnit);
      else if (ctx.align) return ctx.column + (closing ? 0 : 1);
      else return ctx.indented + (closing ? 0 : config.indentUnit);
    },

    electricChars: "{}",
    closeBrackets: {triples: "'\""},
    fold: "brace"
  };
});

CodeMirror.defineMIME("text/x-groovy", "groovy");

});
