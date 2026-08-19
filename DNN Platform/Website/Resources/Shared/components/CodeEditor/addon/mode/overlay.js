const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Utility function that allows modes to be combined. The mode given
// as the base argument takes care of most of the normal mode
// functionality, but a second (typically simple) mode is used, which
// can override the style of text. Both modes get to parse all of the
// text, but when both assign a non-null style to a piece of code, the
// overlay wins, unless the combine argument was true and not overridden,
// or state.overlay.combineTokens was true, in which case the styles are
// combined.

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

CodeMirror.overlayMode = function(base, overlay, combine) {
  return {
    startState: function() {
	logger.debug('>>> Entering startState()');
      return {
        base: CodeMirror.startState(base),
        overlay: CodeMirror.startState(overlay),
        basePos: 0, baseCur: null,
        overlayPos: 0, overlayCur: null,
        streamSeen: null
      };
	logger.debug('<<< Exiting startState()');
    },
    copyState: function(state) {
	logger.debug(`>>> Entering copyState(state=${state})`);
      return {
        base: CodeMirror.copyState(base, state.base),
        overlay: CodeMirror.copyState(overlay, state.overlay),
        basePos: state.basePos, baseCur: null,
        overlayPos: state.overlayPos, overlayCur: null
      };
	logger.debug(`<<< Exiting copyState(state=${state})`);
    },

    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      if (stream != state.streamSeen ||
          Math.min(state.basePos, state.overlayPos) < stream.start) {
        state.streamSeen = stream;
        state.basePos = state.overlayPos = stream.start;
      }

	logger.debug(`function(exports=${exports}): state.streamSeen → ${state.streamSeen}`);
	logger.debug(`function(base=${base},overlay=${overlay},combine=${combine}): state.streamSeen → ${state.streamSeen}`);
	logger.debug(`function(exports=${exports}): state.basePos → ${state.basePos}`);
	logger.debug(`function(base=${base},overlay=${overlay},combine=${combine}): state.basePos → ${state.basePos}`);
      if (stream.start == state.basePos) {
        state.baseCur = base.token(stream, state.base);
        state.basePos = stream.pos;
      }
      if (stream.start == state.overlayPos) {
		logger.debug(`token(stream=${stream},state=${state}): state.streamSeen → ${state.streamSeen}`);
		logger.debug(`token(stream=${stream},state=${state}): state.basePos → ${state.basePos}`);
        stream.pos = stream.start;
        state.overlayCur = overlay.token(stream, state.overlay);
		logger.debug(`function(exports=${exports}): stream.pos → ${stream.pos}`);
		logger.debug(`function(base=${base},overlay=${overlay},combine=${combine}): stream.pos → ${stream.pos}`);
        state.overlayPos = stream.pos;
      }
      stream.pos = Math.min(state.basePos, state.overlayPos);
	logger.debug(`function(exports=${exports}): state.overlayPos → ${state.overlayPos}`);
	logger.debug(`function(base=${base},overlay=${overlay},combine=${combine}): state.overlayPos → ${state.overlayPos}`);

      // state.overlay.combineTokens always takes precedence over combine,
      // unless set to null
      if (state.overlayCur == null) return state.baseCur;
      else if (state.baseCur != null &&
               state.overlay.combineTokens ||
               combine && state.overlay.combineTokens == null)
        return state.baseCur + " " + state.overlayCur;
      else return state.overlayCur;
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
    },

    indent: base.indent && function(state, textAfter) {
	logger.debug(`<<< Exiting function(state=${state},textAfter=${textAfter})`);
      return base.indent(state.base, textAfter);
    },
    electricChars: base.electricChars,

    innerMode: function(state) { return {state: state.base, mode: base}; },

    blankLine: function(state) {
	logger.debug(`>>> Entering blankLine(state=${state})`);
      if (base.blankLine) base.blankLine(state.base);
      if (overlay.blankLine) overlay.blankLine(state.overlay);
	logger.debug(`<<< Exiting blankLine(state=${state})`);
	logger.debug(`<<< Exiting function(exports=${exports})`);
    }
  };
};

});
