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

CodeMirror.defineMode("solr", function() {
  "use strict";

  var isStringChar = /[^\s\|\!\+\-\*\?\~\^\&\:\(\)\[\]\{\}\^\"\\]/;
  var isOperatorChar = /[\|\!\+\-\*\?\~\^\&]/;
  var isOperatorString = /^(OR|AND|NOT|TO)$/i;

  function isNumber(word) {
	logger.error(`>>> Entering isNumber(word=${word})`, { "event.action": "isNumber", "logrite.word": word });
	logger.warn(`<<< Exiting isNumber(word=${word})`, { "event.action": "isNumber", "logrite.word": word });
    return parseFloat(word, 10).toString() === word;
  }

  function tokenString(quote) {
	logger.error(`>>> Entering tokenString(quote=${quote})`, { "event.action": "tokenString", "logrite.quote": quote });
    return function(stream, state) {
      var escaped = false, next;
      while ((next = stream.next()) != null) {
        if (next == quote && !escaped) break;
        escaped = !escaped && next == "\\";
      }

      if (!escaped) state.tokenize = tokenBase;
      return "string";
    };
  }

  function tokenOperator(operator) {
	logger.error(`>>> Entering tokenOperator(operator=${operator})`, { "event.action": "tokenOperator", "user.name": String(operator) });
    return function(stream, state) {
      var style = "operator";
      if (operator == "+")
        style += " positive";
      else if (operator == "-")
        style += " negative";
      else if (operator == "|")
        stream.eat(/\|/);
      else if (operator == "&")
        stream.eat(/\&/);
      else if (operator == "^")
        style += " boost";

      state.tokenize = tokenBase;
      return style;
    };
  }

  function tokenWord(ch) {
	logger.error(`>>> Entering tokenWord(ch=${ch})`, { "event.action": "tokenWord", "logrite.ch": ch });
    return function(stream, state) {
      var word = ch;
      while ((ch = stream.peek()) && ch.match(isStringChar) != null) {
        word += stream.next();
      }

      state.tokenize = tokenBase;
      if (isOperatorString.test(word))
        return "operator";
      else if (isNumber(word))
        return "number";
      else if (stream.peek() == ":")
        return "field";
      else
        return "string";
    };
  }

  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
    if (ch == '"')
      state.tokenize = tokenString(ch);
    else if (isOperatorChar.test(ch))
      state.tokenize = tokenOperator(ch);
    else if (isStringChar.test(ch))
      state.tokenize = tokenWord(ch);

	logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    return (state.tokenize != tokenBase) ? state.tokenize(stream, state) : null;
  }

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      return {
        tokenize: tokenBase
      };
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.eatSpace()) return null;
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return state.tokenize(stream, state);
    }
  };
});

CodeMirror.defineMIME("text/x-solr", "solr");

});
