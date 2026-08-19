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

logger.debug(`<<< Exiting function(exports=${exports})`);
CodeMirror.defineMode("solr", function() {
logger.debug('>>> Entering defineMode()');
  "use strict";

  var isStringChar = /[^\s\|\!\+\-\*\?\~\^\&\:\(\)\[\]\{\}\^\"\\]/;
  var isOperatorChar = /[\|\!\+\-\*\?\~\^\&]/;
  var isOperatorString = /^(OR|AND|NOT|TO)$/i;

  function isNumber(word) {
	logger.debug(`>>> Entering isNumber(word=${word})`);
	logger.debug(`<<< Exiting isNumber(word=${word})`);
    return parseFloat(word, 10).toString() === word;
  }

  function tokenString(quote) {
	logger.debug(`>>> Entering tokenString(quote=${quote})`);
    return function(stream, state) {
      var escaped = false, next;
      while ((next = stream.next()) != null) {
        if (next == quote && !escaped) break;
        escaped = !escaped && next == "\\";
      }

      if (!escaped) state.tokenize = tokenBase;
	logger.debug(`<<< Exiting tokenString(quote=${quote})`);
      return "string";
    };
  }

  function tokenOperator(operator) {
	logger.debug(`>>> Entering tokenOperator(operator=${operator})`);
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
	logger.debug(`tokenOperator(operator=${operator}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting tokenOperator(operator=${operator})`);
      return style;
    };
  }

  function tokenWord(ch) {
	logger.debug(`>>> Entering tokenWord(ch=${ch})`);
    return function(stream, state) {
      var word = ch;
      while ((ch = stream.peek()) && ch.match(isStringChar) != null) {
        word += stream.next();
      }

      state.tokenize = tokenBase;
	logger.debug(`tokenWord(ch=${ch}): state.tokenize → ${state.tokenize}`);
      if (isOperatorString.test(word))
        return "operator";
      else if (isNumber(word))
        return "number";
      else if (stream.peek() == ":")
        return "field";
      else
		logger.debug(`<<< Exiting tokenWord(ch=${ch})`);
        return "string";
    };
  }

  function tokenBase(stream, state) {
	logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
    var ch = stream.next();
    if (ch == '"')
      state.tokenize = tokenString(ch);
    else if (isOperatorChar.test(ch))
      state.tokenize = tokenOperator(ch);
    else if (isStringChar.test(ch))
      state.tokenize = tokenWord(ch);

	logger.debug(`<<< Exiting tokenBase(stream=${stream},state=${state})`);
    return (state.tokenize != tokenBase) ? state.tokenize(stream, state) : null;
  }

  return {
    startState: function() {
	logger.debug('>>> Entering startState()');
      return {
        tokenize: tokenBase
      };
	logger.debug('<<< Exiting startState()');
    },

    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      if (stream.eatSpace()) return null;
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      return state.tokenize(stream, state);
    }
  };
});

CodeMirror.defineMIME("text/x-solr", "solr");

});
