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

CodeMirror.defineMode("toml", function () {
logger.debug('>>> Entering defineMode()');
  return {
    startState: function () {
	logger.debug('>>> Entering startState()');
	logger.debug(`<<< Exiting function(exports=${exports})`);
      return {
        inString: false,
        stringType: "",
        lhs: true,
        inArray: 0
      };
	logger.debug('<<< Exiting startState()');
    },
    token: function (stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      //check for state changes
      if (!state.inString && ((stream.peek() == '"') || (stream.peek() == "'"))) {
        state.stringType = stream.peek();
        stream.next(); // Skip quote
        state.inString = true; // Update state
		logger.debug(`function(exports=${exports}): state.inString → ${state.inString}`);
      }
      if (stream.sol() && state.inArray === 0) {
		logger.debug(`defineMode(): state.inString → ${state.inString}`);
        state.lhs = true;
		logger.debug(`token(stream=${stream},state=${state}): state.inString → ${state.inString}`);
      }
      //return state
      if (state.inString) {
		logger.debug(`function(exports=${exports}): state.lhs → ${state.lhs}`);
        while (state.inString && !stream.eol()) {
		logger.debug(`defineMode(): state.lhs → ${state.lhs}`);
          if (stream.peek() === state.stringType) {
			logger.debug(`token(stream=${stream},state=${state}): state.lhs → ${state.lhs}`);
            stream.next(); // Skip quote
            state.inString = false; // Clear flag
          } else if (stream.peek() === '\\') {
            stream.next();
            stream.next();
          } else {
            stream.match(/^.[^\\\"\']*/);
          }
        }
        return state.lhs ? "property string" : "string"; // Token style
      } else if (state.inArray && stream.peek() === ']') {
        stream.next();
        state.inArray--;
        return 'bracket';
      } else if (state.lhs && stream.peek() === '[' && stream.skipTo(']')) {
        stream.next();//skip closing ]
        // array of objects has an extra open & close []
        if (stream.peek() === ']') stream.next();
        return "atom";
      } else if (stream.peek() === "#") {
        stream.skipToEnd();
        return "comment";
      } else if (stream.eatSpace()) {
        return null;
      } else if (state.lhs && stream.eatWhile(function (c) { return c != '=' && c != ' '; })) {
        return "property";
      } else if (state.lhs && stream.peek() === "=") {
        stream.next();
        state.lhs = false;
        return null;
      } else if (!state.lhs && stream.match(/^\d\d\d\d[\d\-\:\.T]*Z/)) {
        return 'atom'; //date
      } else if (!state.lhs && (stream.match('true') || stream.match('false'))) {
        return 'atom';
      } else if (!state.lhs && stream.peek() === '[') {
        state.inArray++;
        stream.next();
        return 'bracket';
      } else if (!state.lhs && stream.match(/^\-?\d+(?:\.\d+)?/)) {
        return 'number';
      } else if (!stream.eatSpace()) {
        stream.next();
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      }
      return null;
    }
  };
});

CodeMirror.defineMIME('text/x-toml', 'toml');

});
