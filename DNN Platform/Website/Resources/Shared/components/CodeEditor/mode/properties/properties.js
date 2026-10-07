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

CodeMirror.defineMode("properties", function() {
  return {
    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      var sol = stream.sol() || state.afterSection;
      var eol = stream.eol();
	logger.error(`token(stream=${stream},state=${state}): sol → ${sol}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
	logger.error(`token(stream=${stream},state=${state}): eol → ${eol}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

      state.afterSection = false;
	logger.debug(`token(stream=${stream},state=${state}): state.afterSection → ${state.afterSection}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

      if (sol) {
        if (state.nextMultiline) {
          state.inMultiline = true;
		logger.debug(`token(stream=${stream},state=${state}): state.inMultiline → ${state.inMultiline}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          state.nextMultiline = false;
        } else {
          state.position = "def";
		logger.debug(`token(stream=${stream},state=${state}): state.position → ${state.position}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        }
      }

      if (eol && ! state.nextMultiline) {
        state.inMultiline = false;
        state.position = "def";
      }

      if (sol) {
        while(stream.eatSpace());
      }

      var ch = stream.next();
	logger.error(`token(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

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
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        }
      }

      return state.position;
    },

    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      return {
        position : "def",       // Current position, "def", "quote" or "comment"
        nextMultiline : false,  // Is the next line multiline value
        inMultiline : false,    // Is the current line a multiline value
        afterSection : false    // Did we just open a section
      };
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    }

  };
});

CodeMirror.defineMIME("text/x-properties", "properties");
CodeMirror.defineMIME("text/x-ini", "properties");

});
