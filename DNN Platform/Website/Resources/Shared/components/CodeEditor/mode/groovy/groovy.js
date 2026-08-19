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

CodeMirror.defineMode("groovy", function(config) {
logger.debug('>>> Entering defineMode()');
  function words(str) {
	logger.debug(`>>> Entering words(str=${str})`);
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.debug(`words(str=${str}): obj → ${obj}`);
	logger.debug(`<<< Exiting words(str=${str})`);
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
logger.debug(`defineMode(): standaloneKeywords → ${standaloneKeywords}`);
  var atoms = words("null true false this");
logger.debug(`defineMode(): atoms → ${atoms}`);

  var curPunc;
  function tokenBase(stream, state) {
	logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
    var ch = stream.next();
    if (ch == '"' || ch == "'") {
	logger.debug(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`);
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
	logger.debug(`tokenBase(stream=${stream},state=${state}): cur → ${cur}`);
    if (atoms.propertyIsEnumerable(cur)) { return "atom"; }
    if (keywords.propertyIsEnumerable(cur)) {
      if (blockKeywords.propertyIsEnumerable(cur)) curPunc = "newstatement";
      else if (standaloneKeywords.propertyIsEnumerable(cur)) curPunc = "standalone";
	logger.debug(`<<< Exiting tokenBase(stream=${stream},state=${state})`);
      return "keyword";
    }
    return "variable";
  }
  tokenBase.isBase = true;
logger.debug(`defineMode(): tokenBase.isBase → ${tokenBase.isBase}`);

  function startString(quote, stream, state) {
	logger.debug(`>>> Entering startString(quote=${quote},stream=${stream},state=${state})`);
    var tripleQuoted = false;
    if (quote != "/" && stream.eat(quote)) {
      if (stream.eat(quote)) tripleQuoted = true;
      else return "string";
    }
    function t(stream, state) {
	logger.debug(`>>> Entering t(stream=${stream},state=${state})`);
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
	logger.debug(`<<< Exiting t(stream=${stream},state=${state})`);
	logger.debug(`<<< Exiting startString(quote=${quote},stream=${stream},state=${state})`);
    return t(stream, state);
  }

  function tokenBaseUntilBrace() {
	logger.debug('>>> Entering tokenBaseUntilBrace()');
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
	logger.debug('<<< Exiting tokenBaseUntilBrace()');
    return t;
  }

  function tokenComment(stream, state) {
	logger.debug(`>>> Entering tokenComment(stream=${stream},state=${state})`);
    var maybeEnd = false, ch;
    while (ch = stream.next()) {
	logger.debug(`tokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
      if (ch == "/" && maybeEnd) {
        state.tokenize.pop();
        break;
      }
      maybeEnd = (ch == "*");
    }
	logger.debug(`<<< Exiting tokenComment(stream=${stream},state=${state})`);
    return "comment";
  }

  function expectExpression(last, newline) {
	logger.debug(`>>> Entering expectExpression(last=${last},newline=${newline})`);
    return !last || last == "operator" || last == "->" || /[\.\[\{\(,;:]/.test(last) ||
      last == "newstatement" || last == "keyword" || last == "proplabel" ||
      (last == "standalone" && !newline);
	logger.debug(`<<< Exiting expectExpression(last=${last},newline=${newline})`);
  }

  function Context(indented, column, type, align, prev) {
	logger.debug(`>>> Entering Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev})`);
    this.indented = indented;
    this.column = column;
	logger.debug(`defineMode(): this.indented → ${this.indented}`);
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
	logger.debug(`<<< Exiting pushContext(state=${state},col=${col},type=${type})`);
    return state.context = new Context(state.indented, col, type, null, state.context);
  }
  function popContext(state) {
	logger.debug(`>>> Entering popContext(state=${state})`);
    var t = state.context.type;
    if (t == ")" || t == "]" || t == "}")
      state.indented = state.context.indented;
	logger.debug(`<<< Exiting popContext(state=${state})`);
    return state.context = state.context.prev;
  }

  // Interface

  return {
    startState: function(basecolumn) {
	logger.debug(`>>> Entering startState(basecolumn=${basecolumn})`);
	logger.debug(`defineMode(): this.column → ${this.column}`);
      return {
        tokenize: [tokenBase],
        context: new Context((basecolumn || 0) - config.indentUnit, 0, "top", false),
        indented: 0,
        startOfLine: true,
        lastToken: null
      };
	logger.debug(`<<< Exiting startState(basecolumn=${basecolumn})`);
    },

    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      var ctx = state.context;
      if (stream.sol()) {
        if (ctx.align == null) ctx.align = false;
        state.indented = stream.indentation();
		logger.debug(`defineMode(): this.prev → ${this.prev}`);
		logger.debug(`defineMode(): state.indented → ${state.indented}`);
        state.startOfLine = true;
        // Automatic semicolon insertion
        if (ctx.type == "statement" && !expectExpression(state.lastToken, true)) {
          popContext(state); ctx = state.context;
		logger.debug(`defineMode(): this.align → ${this.align}`);
        }
      }
      if (stream.eatSpace()) return null;
      curPunc = null;
	logger.debug(`defineMode(): this.type → ${this.type}`);
      var style = state.tokenize[state.tokenize.length-1](stream, state);
	logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
      if (style == "comment") return style;
	logger.debug(`defineMode(): curPunc → ${curPunc}`);
      if (ctx.align == null) ctx.align = true;

      if ((curPunc == ";" || curPunc == ":") && ctx.type == "statement") popContext(state);
      // Handle indentation for {x -> \n ... }
      else if (curPunc == "->" && ctx.type == "statement" && ctx.prev.type == "}") {
		logger.debug(`token(stream=${stream},state=${state}): state.startOfLine → ${state.startOfLine}`);
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
	logger.debug(`defineMode(): state.lastToken → ${state.lastToken}`);
	logger.debug('<<< Exiting defineMode()');
      return style;
    },

    indent: function(state, textAfter) {
	logger.debug(`>>> Entering indent(state=${state},textAfter=${textAfter})`);
      if (!state.tokenize[state.tokenize.length-1].isBase) return 0;
      var firstChar = textAfter && textAfter.charAt(0), ctx = state.context;
	logger.debug(`indent(state=${state},textAfter=${textAfter}): firstChar → ${firstChar}`);
      if (ctx.type == "statement" && !expectExpression(state.lastToken, true)) ctx = ctx.prev;
      var closing = firstChar == ctx.type;
	logger.debug(`<<< Exiting indent(state=${state},textAfter=${textAfter})`);
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
