const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object")
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd)
    define(["../../lib/codemirror"], mod);
  else
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMode("cmake", function () {
  var variable_regex = /({)?[a-zA-Z0-9_]+(})?/;

  function tokenString(stream, state) {
	logger.error(`>>> Entering tokenString(stream=${stream},state=${state})`, { "event.action": "tokenString", "logrite.stream": stream, "logrite.state": state });
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
	logger.warn(`<<< Exiting tokenString(stream=${stream},state=${state})`, { "event.action": "tokenString", "logrite.stream": stream, "logrite.state": state });
    return "string";
  }

  function tokenize(stream, state) {
	logger.error(`>>> Entering tokenize(stream=${stream},state=${state})`, { "event.action": "tokenize", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();

	logger.error(`tokenize(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenize", "logrite.stream": stream, "logrite.state": state });
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
	logger.debug(`tokenize(stream=${stream},state=${state}): state.pending → ${state.pending}`, { "event.action": "tokenize", "logrite.stream": stream, "logrite.state": state });
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
	logger.warn(`<<< Exiting tokenize(stream=${stream},state=${state})`, { "event.action": "tokenize", "logrite.stream": stream, "logrite.state": state });
    return null;
  }
  return {
    startState: function () {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      var state = {};
      state.inDefinition = false;
      state.inInclude = false;
	logger.debug(`startState(): state.inDefinition → ${state.inDefinition}`, { "event.action": "startState" });
	logger.debug(`startState(): state.inInclude → ${state.inInclude}`, { "event.action": "startState" });
	logger.debug(`startState(): state.pending → ${state.pending}`, { "event.action": "startState" });
      state.continueString = false;
      state.pending = false;
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      return state;
    },
    token: function (stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.eatSpace()) return null;
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return tokenize(stream, state);
    }
  };
});

CodeMirror.defineMIME("text/x-cmake", "cmake");

});
