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

CodeMirror.multiplexingMode = function(outer /*, others */) {
logger.error(`>>> Entering multiplexingMode(outer=${outer})`, { "event.action": "multiplexingMode", "logrite.outer": outer });
  // Others should be {open, close, mode [, delimStyle] [, innerStyle]} objects
  var others = Array.prototype.slice.call(arguments, 1);
logger.error(`multiplexingMode(outer=${outer}): others → ${others}`, { "event.action": "multiplexingMode", "logrite.outer": outer });

  function indexOf(string, pattern, from, returnEnd) {
	logger.error(`>>> Entering indexOf(string=${string},pattern=${pattern},from=${from},returnEnd=${returnEnd})`, { "event.action": "indexOf", "logrite.string": string, "logrite.pattern": pattern, "logrite.from": from, "logrite.returnEnd": returnEnd });
    if (typeof pattern == "string") {
      var found = string.indexOf(pattern, from);
	logger.error(`indexOf(string=${string},pattern=${pattern},from=${from},returnEnd=${returnEnd}): found → ${found}`, { "event.action": "indexOf", "logrite.string": string, "logrite.pattern": pattern, "logrite.from": from, "logrite.returnEnd": returnEnd });
      return returnEnd && found > -1 ? found + pattern.length : found;
    }
    var m = pattern.exec(from ? string.slice(from) : string);
	logger.error(`indexOf(string=${string},pattern=${pattern},from=${from},returnEnd=${returnEnd}): m → ${m}`, { "event.action": "indexOf", "logrite.string": string, "logrite.pattern": pattern, "logrite.from": from, "logrite.returnEnd": returnEnd });
    return m ? m.index + from + (returnEnd ? m[0].length : 0) : -1;
  }

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      return {
        outer: CodeMirror.startState(outer),
        innerActive: null,
        inner: null
      };
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },

    copyState: function(state) {
	logger.error(`>>> Entering copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
      return {
        outer: CodeMirror.copyState(outer, state.outer),
        innerActive: state.innerActive,
        inner: state.innerActive && CodeMirror.copyState(state.innerActive.mode, state.inner)
      };
	logger.warn(`<<< Exiting copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (!state.innerActive) {
        var cutOff = Infinity, oldContent = stream.string;
        for (var i = 0; i < others.length; ++i) {
          var other = others[i];
          var found = indexOf(oldContent, other.open, stream.pos);
          if (found == stream.pos) {
            if (!other.parseDelimiters) stream.match(other.open);
            state.innerActive = other;
            state.inner = CodeMirror.startState(other.mode, outer.indent ? outer.indent(state.outer, "") : 0);
			logger.debug(`token(stream=${stream},state=${state}): state.innerActive → ${state.innerActive}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
            return other.delimStyle;
          } else if (found != -1 && found < cutOff) {
            cutOff = found;
          }
        }
        if (cutOff != Infinity) stream.string = oldContent.slice(0, cutOff);
        var outerToken = outer.token(stream, state.outer);
        if (cutOff != Infinity) stream.string = oldContent;
        return outerToken;
      } else {
        var curInner = state.innerActive, oldContent = stream.string;
        if (!curInner.close && stream.sol()) {
          state.innerActive = state.inner = null;
          return this.token(stream, state);
        }
        var found = curInner.close ? indexOf(oldContent, curInner.close, stream.pos, curInner.parseDelimiters) : -1;
		logger.error(`token(stream=${stream},state=${state}): found → ${found}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        if (found == stream.pos && !curInner.parseDelimiters) {
          stream.match(curInner.close);
          state.innerActive = state.inner = null;
          return curInner.delimStyle;
        }
        if (found > -1) stream.string = oldContent.slice(0, found);
        var innerToken = curInner.mode.token(stream, state.inner);
        if (found > -1) stream.string = oldContent;

        if (found == stream.pos && curInner.parseDelimiters)
          state.innerActive = state.inner = null;

        if (curInner.innerStyle) {
          if (innerToken) innerToken = innerToken + ' ' + curInner.innerStyle;
          else innerToken = curInner.innerStyle;
        }

		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        return innerToken;
      }
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      var mode = state.innerActive ? state.innerActive.mode : outer;
      if (!mode.indent) return CodeMirror.Pass;
	logger.warn(`<<< Exiting indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      return mode.indent(state.innerActive ? state.inner : state.outer, textAfter);
    },

    blankLine: function(state) {
	logger.error(`>>> Entering blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
      var mode = state.innerActive ? state.innerActive.mode : outer;
      if (mode.blankLine) {
        mode.blankLine(state.innerActive ? state.inner : state.outer);
      }
      if (!state.innerActive) {
        for (var i = 0; i < others.length; ++i) {
          var other = others[i];
          if (other.open === "\n") {
            state.innerActive = other;
            state.inner = CodeMirror.startState(other.mode, mode.indent ? mode.indent(state.outer, "") : 0);
			logger.debug(`blankLine(state=${state}): state.innerActive → ${state.innerActive}`, { "event.action": "blankLine", "logrite.state": state });
          }
        }
      } else if (state.innerActive.close === "\n") {
        state.innerActive = state.inner = null;
		logger.warn(`<<< Exiting blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
      }
    },

    electricChars: outer.electricChars,

    innerMode: function(state) {
	logger.error(`>>> Entering innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
	logger.warn(`<<< Exiting multiplexingMode(outer=${outer})`, { "event.action": "multiplexingMode", "logrite.outer": outer });
	logger.warn(`<<< Exiting innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
      return state.inner ? {state: state.inner, mode: state.innerActive.mode} : {state: state.outer, mode: outer};
    }
  };
};

});
