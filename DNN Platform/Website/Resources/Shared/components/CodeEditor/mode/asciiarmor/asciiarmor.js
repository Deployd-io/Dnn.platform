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

  function errorIfNotEmpty(stream) {
	logger.debug(`>>> Entering errorIfNotEmpty(stream=${stream})`);
    var nonWS = stream.match(/^\s*\S/);
    stream.skipToEnd();
	logger.debug(`errorIfNotEmpty(stream=${stream}): nonWS → ${nonWS}`);
	logger.debug(`<<< Exiting errorIfNotEmpty(stream=${stream})`);
    return nonWS ? "error" : null;
  }

  CodeMirror.defineMode("asciiarmor", function() {
	logger.debug('>>> Entering defineMode()');
    return {
      token: function(stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
        var m;
        if (state.state == "top") {
          if (stream.sol() && (m = stream.match(/^-----BEGIN (.*)?-----\s*$/))) {
            state.state = "headers";
			logger.debug(`defineMode(): state.state → ${state.state}`);
            state.type = m[1];
			logger.debug(`defineMode(): state.type → ${state.type}`);
			logger.debug(`token(stream=${stream},state=${state}): state.state → ${state.state}`);
            return "tag";
          }
		logger.debug(`token(stream=${stream},state=${state}): state.type → ${state.type}`);
          return errorIfNotEmpty(stream);
        } else if (state.state == "headers") {
          if (stream.sol() && stream.match(/^\w+:/)) {
            state.state = "header";
            return "atom";
          } else {
            var result = errorIfNotEmpty(stream);
			logger.debug(`token(stream=${stream},state=${state}): result → ${result}`);
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
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
          return errorIfNotEmpty(stream);
        }
      },
      blankLine: function(state) {
		logger.debug(`>>> Entering blankLine(state=${state})`);
        if (state.state == "headers") state.state = "body";
		logger.debug(`<<< Exiting blankLine(state=${state})`);
      },
      startState: function() {
		logger.debug('>>> Entering startState()');
		logger.debug('<<< Exiting startState()');
        return {state: "top", type: null};
      }
    };
  });

  CodeMirror.defineMIME("application/pgp", "asciiarmor");
  CodeMirror.defineMIME("application/pgp-keys", "asciiarmor");
  CodeMirror.defineMIME("application/pgp-signature", "asciiarmor");
});
