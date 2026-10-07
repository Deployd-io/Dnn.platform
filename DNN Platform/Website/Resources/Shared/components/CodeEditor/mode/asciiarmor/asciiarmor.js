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

  function errorIfNotEmpty(stream) {
	logger.error(`>>> Entering errorIfNotEmpty(stream=${stream})`, { "event.action": "errorIfNotEmpty", "logrite.stream": stream });
    var nonWS = stream.match(/^\s*\S/);
    stream.skipToEnd();
	logger.error(`errorIfNotEmpty(stream=${stream}): nonWS → ${nonWS}`, { "event.action": "errorIfNotEmpty", "logrite.stream": stream });
	logger.warn(`<<< Exiting errorIfNotEmpty(stream=${stream})`, { "event.action": "errorIfNotEmpty", "logrite.stream": stream });
    return nonWS ? "error" : null;
  }

  CodeMirror.defineMode("asciiarmor", function() {
    return {
      token: function(stream, state) {
		logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        var m;
        if (state.state == "top") {
          if (stream.sol() && (m = stream.match(/^-----BEGIN (.*)?-----\s*$/))) {
            state.state = "headers";
			logger.debug(`token(stream=${stream},state=${state}): state.state → ${state.state}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
            state.type = m[1];
			logger.debug(`token(stream=${stream},state=${state}): state.type → ${state.type}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
            return "tag";
          }
          return errorIfNotEmpty(stream);
        } else if (state.state == "headers") {
          if (stream.sol() && stream.match(/^\w+:/)) {
            state.state = "header";
            return "atom";
          } else {
            var result = errorIfNotEmpty(stream);
			logger.error(`token(stream=${stream},state=${state}): result → ${result}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
            if (result) state.state = "body";
            return result;
          }
        } else if (state.state == "header") {
          stream.skipToEnd();
          state.state = "headers";
          return "string";
        } else if (state.state == "body") {
          if (stream.sol() && (m = stream.match(/^-----END (.*)?-----\s*$/))) {
            if (m[1] != state.type) return "error";
            state.state = "end";
            return "tag";
          } else {
            if (stream.eatWhile(/[A-Za-z0-9+\/=]/)) {
              return null;
            } else {
              stream.next();
              return "error";
            }
          }
        } else if (state.state == "end") {
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          return errorIfNotEmpty(stream);
        }
      },
      blankLine: function(state) {
		logger.error(`>>> Entering blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
        if (state.state == "headers") state.state = "body";
		logger.warn(`<<< Exiting blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
      },
      startState: function() {
		logger.error('>>> Entering startState()', { "event.action": "startState" });
		logger.warn('<<< Exiting startState()', { "event.action": "startState" });
        return {state: "top", type: null};
      }
    };
  });

  CodeMirror.defineMIME("application/pgp", "asciiarmor");
  CodeMirror.defineMIME("application/pgp-keys", "asciiarmor");
  CodeMirror.defineMIME("application/pgp-signature", "asciiarmor");
});
