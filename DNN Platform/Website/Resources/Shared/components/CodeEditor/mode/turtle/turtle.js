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

CodeMirror.defineMode("turtle", function(config) {
  var indentUnit = config.indentUnit;
  var curPunc;

  function wordRegexp(words) {
	logger.error(`>>> Entering wordRegexp(words=${words})`, { "event.action": "wordRegexp", "logrite.words": words });
	logger.warn(`<<< Exiting wordRegexp(words=${words})`, { "event.action": "wordRegexp", "logrite.words": words });
    return new RegExp("^(?:" + words.join("|") + ")$", "i");
  }
  var ops = wordRegexp([]);
  var keywords = wordRegexp(["@prefix", "@base", "a"]);
  var operatorChars = /[*+\-<>=&|]/;

  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
    curPunc = null;
	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    if (ch == "<" && !stream.match(/^[\s\u00a0=]/, false)) {
      stream.match(/^[^\s\u00a0>]*>?/);
      return "atom";
    }
    else if (ch == "\"" || ch == "'") {
      state.tokenize = tokenLiteral(ch);
      return state.tokenize(stream, state);
    }
    else if (/[{}\(\),\.;\[\]]/.test(ch)) {
      curPunc = ch;
      return null;
    }
    else if (ch == "#") {
      stream.skipToEnd();
      return "comment";
    }
    else if (operatorChars.test(ch)) {
      stream.eatWhile(operatorChars);
      return null;
    }
    else if (ch == ":") {
          return "operator";
        } else {
      stream.eatWhile(/[_\w\d]/);
      if(stream.peek() == ":") {
        return "variable-3";
      } else {
             var word = stream.current();

             if(keywords.test(word)) {
                        return "meta";
             }

             if(ch >= "A" && ch <= "Z") {
                    return "comment";
                 } else {
                        return "keyword";
                 }
      }
      var word = stream.current();
      if (ops.test(word))
        return null;
      else if (keywords.test(word))
        return "meta";
      else
        return "variable";
    }
  }

  function tokenLiteral(quote) {
	logger.error(`>>> Entering tokenLiteral(quote=${quote})`, { "event.action": "tokenLiteral", "logrite.quote": quote });
    return function(stream, state) {
      var escaped = false, ch;
      while ((ch = stream.next()) != null) {
        if (ch == quote && !escaped) {
          state.tokenize = tokenBase;
          break;
        }
        escaped = !escaped && ch == "\\";
      }
      return "string";
    };
  }

  function pushContext(state, type, col) {
	logger.error(`>>> Entering pushContext(state=${state},type=${type},col=${col})`, { "event.action": "pushContext", "logrite.state": state, "logrite.type": type, "logrite.col": col });
    state.context = {prev: state.context, indent: state.indent, col: col, type: type};
	logger.warn(`<<< Exiting pushContext(state=${state},type=${type},col=${col})`, { "event.action": "pushContext", "logrite.state": state, "logrite.type": type, "logrite.col": col });
  }
  function popContext(state) {
	logger.error(`>>> Entering popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
    state.indent = state.context.indent;
    state.context = state.context.prev;
	logger.warn(`<<< Exiting popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
  }

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      return {tokenize: tokenBase,
              context: null,
              indent: 0,
              col: 0};
			logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.sol()) {
        if (state.context && state.context.align == null) state.context.align = false;
        state.indent = stream.indentation();
      }
      if (stream.eatSpace()) return null;
      var style = state.tokenize(stream, state);
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

      if (style != "comment" && state.context && state.context.align == null && state.context.type != "pattern") {
        state.context.align = true;
      }

      if (curPunc == "(") pushContext(state, ")", stream.column());
      else if (curPunc == "[") pushContext(state, "]", stream.column());
      else if (curPunc == "{") pushContext(state, "}", stream.column());
      else if (/[\]\}\)]/.test(curPunc)) {
        while (state.context && state.context.type == "pattern") popContext(state);
        if (state.context && curPunc == state.context.type) popContext(state);
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      }
      else if (curPunc == "." && state.context && state.context.type == "pattern") popContext(state);
      else if (/atom|string|variable/.test(style) && state.context) {
        if (/[\}\]]/.test(state.context.type))
          pushContext(state, "pattern", stream.column());
        else if (state.context.type == "pattern" && !state.context.align) {
          state.context.align = true;
          state.context.col = stream.column();
        }
      }

      return style;
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      var firstChar = textAfter && textAfter.charAt(0);
      var context = state.context;
	logger.error(`indent(state=${state},textAfter=${textAfter}): firstChar → ${firstChar}`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      if (/[\]\}]/.test(firstChar))
        while (context && context.type == "pattern") context = context.prev;

      var closing = context && firstChar == context.type;
      if (!context)
        return 0;
      else if (context.type == "pattern")
        return context.col;
      else if (context.align)
        return context.col + (closing ? 0 : 1);
      else
        return context.indent + (closing ? 0 : indentUnit);
    },

    lineComment: "#"
  };
});

CodeMirror.defineMIME("text/turtle", "turtle");

});
