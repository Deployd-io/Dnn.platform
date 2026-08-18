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

CodeMirror.defineMode("properties", function() {
logger.debug('>>> Entering defineMode()');
  return {
    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      var sol = stream.sol() || state.afterSection;
	logger.debug(`<<< Exiting function(exports=${exports})`);
      var eol = stream.eol();
	logger.debug('<<< Exiting defineMode()');

      state.afterSection = false;
	logger.debug(`token(stream=${stream},state=${state}): sol → ${sol}`);
	logger.debug(`token(stream=${stream},state=${state}): eol → ${eol}`);

      if (sol) {
		logger.debug(`function(exports=${exports}): state.afterSection → ${state.afterSection}`);
        if (state.nextMultiline) {
		logger.debug(`defineMode(): state.afterSection → ${state.afterSection}`);
          state.inMultiline = true;
		logger.debug(`token(stream=${stream},state=${state}): state.afterSection → ${state.afterSection}`);
          state.nextMultiline = false;
		logger.debug(`function(exports=${exports}): state.inMultiline → ${state.inMultiline}`);
		logger.debug(`function(exports=${exports}): state.nextMultiline → ${state.nextMultiline}`);
        } else {
          state.position = "def";
		logger.debug(`defineMode(): state.inMultiline → ${state.inMultiline}`);
        }
      }

	logger.debug(`function(exports=${exports}): state.position → ${state.position}`);
      if (eol && ! state.nextMultiline) {
        state.inMultiline = false;
		logger.debug(`token(stream=${stream},state=${state}): state.inMultiline → ${state.inMultiline}`);
		logger.debug(`defineMode(): state.nextMultiline → ${state.nextMultiline}`);
        state.position = "def";
		logger.debug(`token(stream=${stream},state=${state}): state.nextMultiline → ${state.nextMultiline}`);
      }

	logger.debug(`defineMode(): state.position → ${state.position}`);
      if (sol) {
		logger.debug(`token(stream=${stream},state=${state}): state.position → ${state.position}`);
        while(stream.eatSpace());
      }

      var ch = stream.next();
	logger.debug(`token(stream=${stream},state=${state}): ch → ${ch}`);

      if (sol && (ch === "#" || ch === "!" || ch === ";")) {
        state.position = "comment";
        stream.skipToEnd();
        return "comment";
      } else if (sol && ch === "[") {
        state.afterSection = true;
        stream.skipTo("]"); stream.eat("]");
        return "header";
      } else if (ch === "=" || ch === ":") {
        state.position = "quote";
        return null;
      } else if (ch === "\\" && state.position === "quote") {
        if (stream.eol()) {  // end of line?
          // Multiline value
          state.nextMultiline = true;
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
        }
      }

      return state.position;
    },

    startState: function() {
	logger.debug('>>> Entering startState()');
      return {
        position : "def",       // Current position, "def", "quote" or "comment"
        nextMultiline : false,  // Is the next line multiline value
        inMultiline : false,    // Is the current line a multiline value
        afterSection : false    // Did we just open a section
      };
	logger.debug('<<< Exiting startState()');
    }

  };
});

CodeMirror.defineMIME("text/x-properties", "properties");
CodeMirror.defineMIME("text/x-ini", "properties");

});
