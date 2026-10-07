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

CodeMirror.defineMode("pascal", function() {
  function words(str) {
	logger.error(`>>> Entering words(str=${str})`, { "event.action": "words", "logrite.str": str });
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.error(`words(str=${str}): obj → ${obj}`, { "event.action": "words", "logrite.str": str });
	logger.warn(`<<< Exiting words(str=${str})`, { "event.action": "words", "logrite.str": str });
    return obj;
  }
  var keywords = words("and array begin case const div do downto else end file for forward integer " +
                       "boolean char function goto if in label mod nil not of or packed procedure " +
                       "program record repeat set string then to type until var while with");
  var atoms = {"null": true};

  var isOperatorChar = /[+\-*&%=<>!?|\/]/;

  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
    if (ch == "#" && state.startOfLine) {
	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      stream.skipToEnd();
      return "meta";
    }
    if (ch == '"' || ch == "'") {
      state.tokenize = tokenString(ch);
      return state.tokenize(stream, state);
    }
    if (ch == "(" && stream.eat("*")) {
      state.tokenize = tokenComment;
	logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      return tokenComment(stream, state);
    }
    if (/[\[\]{}\(\),;\:\.]/.test(ch)) {
      return null;
    }
    if (/\d/.test(ch)) {
      stream.eatWhile(/[\w\.]/);
      return "number";
    }
    if (ch == "/") {
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
    var cur = stream.current();
	logger.error(`tokenBase(stream=${stream},state=${state}): cur → ${cur}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    if (keywords.propertyIsEnumerable(cur)) return "keyword";
    if (atoms.propertyIsEnumerable(cur)) return "atom";
	logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    return "variable";
  }

  function tokenString(quote) {
	logger.error(`>>> Entering tokenString(quote=${quote})`, { "event.action": "tokenString", "logrite.quote": quote });
    return function(stream, state) {
      var escaped = false, next, end = false;
      while ((next = stream.next()) != null) {
        if (next == quote && !escaped) {end = true; break;}
        escaped = !escaped && next == "\\";
      }
      if (end || !escaped) state.tokenize = null;
      return "string";
    };
  }

  function tokenComment(stream, state) {
	logger.error(`>>> Entering tokenComment(stream=${stream},state=${state})`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
    var maybeEnd = false, ch;
    while (ch = stream.next()) {
      if (ch == ")" && maybeEnd) {
        state.tokenize = null;
		logger.debug(`tokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
        break;
      }
      maybeEnd = (ch == "*");
    }
	logger.error(`tokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
    return "comment";
  }

  // Interface

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      return {tokenize: null};
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.eatSpace()) return null;
      var style = (state.tokenize || tokenBase)(stream, state);
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (style == "comment" || style == "meta") return style;
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return style;
    },

    electricChars: "{}"
  };
});

CodeMirror.defineMIME("text/x-pascal", "pascal");

});
