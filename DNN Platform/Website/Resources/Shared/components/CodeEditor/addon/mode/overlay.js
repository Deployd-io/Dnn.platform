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
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.overlayMode = function(base, overlay, combine) {
logger.error(`>>> Entering overlayMode(base=${base},overlay=${overlay},combine=${combine})`, { "event.action": "overlayMode", "logrite.base": base, "logrite.overlay": overlay, "logrite.combine": combine });
  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      return {
        base: CodeMirror.startState(base),
        overlay: CodeMirror.startState(overlay),
        basePos: 0, baseCur: null,
        overlayPos: 0, overlayCur: null,
        streamSeen: null
      };
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },
    copyState: function(state) {
	logger.error(`>>> Entering copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
      return {
        base: CodeMirror.copyState(base, state.base),
        overlay: CodeMirror.copyState(overlay, state.overlay),
        basePos: state.basePos, baseCur: null,
        overlayPos: state.overlayPos, overlayCur: null
      };
	logger.warn(`<<< Exiting copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream != state.streamSeen ||
          Math.min(state.basePos, state.overlayPos) < stream.start) {
        state.streamSeen = stream;
        state.basePos = state.overlayPos = stream.start;
      }

	logger.debug(`token(stream=${stream},state=${state}): state.streamSeen → ${state.streamSeen}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`token(stream=${stream},state=${state}): state.basePos → ${state.basePos}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.start == state.basePos) {
        state.baseCur = base.token(stream, state.base);
        state.basePos = stream.pos;
      }
      if (stream.start == state.overlayPos) {
        stream.pos = stream.start;
        state.overlayCur = overlay.token(stream, state.overlay);
		logger.debug(`token(stream=${stream},state=${state}): stream.pos → ${stream.pos}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        state.overlayPos = stream.pos;
      }
      stream.pos = Math.min(state.basePos, state.overlayPos);
	logger.debug(`token(stream=${stream},state=${state}): state.overlayPos → ${state.overlayPos}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

      // state.overlay.combineTokens always takes precedence over combine,
      // unless set to null
      if (state.overlayCur == null) return state.baseCur;
      else if (state.baseCur != null &&
               state.overlay.combineTokens ||
               combine && state.overlay.combineTokens == null)
        return state.baseCur + " " + state.overlayCur;
      else return state.overlayCur;
    },

    indent: base.indent && function(state, textAfter) {
      return base.indent(state.base, textAfter);
    },
    electricChars: base.electricChars,

    innerMode: function(state) { return {state: state.base, mode: base}; },

    blankLine: function(state) {
	logger.error(`>>> Entering blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
      if (base.blankLine) base.blankLine(state.base);
      if (overlay.blankLine) overlay.blankLine(state.overlay);
	logger.warn(`<<< Exiting blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
    }
  };
logger.warn(`<<< Exiting overlayMode(base=${base},overlay=${overlay},combine=${combine})`, { "event.action": "overlayMode", "logrite.base": base, "logrite.overlay": overlay, "logrite.combine": combine });
};

});
