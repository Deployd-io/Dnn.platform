const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object")
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd)
    define(["../../lib/codemirror"], mod);
  else
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

logger.debug(`<<< Exiting function(exports=${exports})`);
CodeMirror.defineMode("cmake", function () {
logger.debug('>>> Entering defineMode()');
  var variable_regex = /({)?[a-zA-Z0-9_]+(})?/;
logger.debug('<<< Exiting defineMode()');
logger.debug(`defineMode(): variable_regex → ${variable_regex}`);

  function tokenString(stream, state) {
	logger.debug(`>>> Entering tokenString(stream=${stream},state=${state})`);
    var current, prev, found_var = false;
    while (!stream.eol() && (current = stream.next()) != state.pending) {
      if (current === '$' && prev != '\\' && state.pending == '"') {
        found_var = true;
        break;
      }
      prev = current;
    }
    if (found_var) {
      stream.backUp(1);
    }
    if (current == state.pending) {
      state.continueString = false;
    } else {
      state.continueString = true;
    }
	logger.debug(`<<< Exiting tokenString(stream=${stream},state=${state})`);
    return "string";
  }

  function tokenize(stream, state) {
	logger.debug(`>>> Entering tokenize(stream=${stream},state=${state})`);
    var ch = stream.next();

	logger.debug(`tokenize(stream=${stream},state=${state}): ch → ${ch}`);
    // Have we found a variable?
    if (ch === '$') {
      if (stream.match(variable_regex)) {
        return 'variable-2';
      }
      return 'variable';
    }
    // Should we still be looking for the end of a string?
    if (state.continueString) {
      // If so, go through the loop again
      stream.backUp(1);
      return tokenString(stream, state);
    }
    // Do we just have a function on our hands?
    // In 'cmake_minimum_required (VERSION 2.8.8)', 'cmake_minimum_required' is matched
    if (stream.match(/(\s+)?\w+\(/) || stream.match(/(\s+)?\w+\ \(/)) {
      stream.backUp(1);
      return 'def';
    }
    if (ch == "#") {
      stream.skipToEnd();
      return "comment";
    }
    // Have we found a string?
    if (ch == "'" || ch == '"') {
      // Store the type (single or double)
      state.pending = ch;
	logger.debug(`tokenize(stream=${stream},state=${state}): state.pending → ${state.pending}`);
      // Perform the looping function to find the end
      return tokenString(stream, state);
    }
    if (ch == '(' || ch == ')') {
      return 'bracket';
    }
    if (ch.match(/[0-9]/)) {
      return 'number';
    }
    stream.eatWhile(/[\w-]/);
    return null;
  }
  return {
    startState: function () {
	logger.debug('>>> Entering startState()');
      var state = {};
      state.inDefinition = false;
      state.inInclude = false;
	logger.debug(`startState(): state.inDefinition → ${state.inDefinition}`);
	logger.debug(`startState(): state.inInclude → ${state.inInclude}`);
	logger.debug(`startState(): state.pending → ${state.pending}`);
      state.continueString = false;
	logger.debug(`startState(): state.continueString → ${state.continueString}`);
      state.pending = false;
	logger.debug('<<< Exiting startState()');
      return state;
    },
    token: function (stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      if (stream.eatSpace()) return null;
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      return tokenize(stream, state);
    }
  };
});

CodeMirror.defineMIME("text/x-cmake", "cmake");

});
