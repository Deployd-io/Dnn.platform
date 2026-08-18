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

CodeMirror.defineMode("yaml", function() {
logger.debug('>>> Entering defineMode()');

  var cons = ['true', 'false', 'on', 'off', 'yes', 'no'];
logger.debug(`defineMode(): cons → ${cons}`);
  var keywordRegex = new RegExp("\\b(("+cons.join(")|(")+"))$", 'i');

  return {
    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      var ch = stream.peek();
	logger.debug(`<<< Exiting function(exports=${exports})`);
      var esc = state.escaped;
	logger.debug(`defineMode(): keywordRegex → ${keywordRegex}`);
      state.escaped = false;
	logger.debug(`token(stream=${stream},state=${state}): ch → ${ch}`);
      /* comments */
      if (ch == "#" && (stream.pos == 0 || /\s/.test(stream.string.charAt(stream.pos - 1)))) {
		logger.debug(`function(exports=${exports}): state.escaped → ${state.escaped}`);
        stream.skipToEnd();
		logger.debug(`defineMode(): state.escaped → ${state.escaped}`);
		logger.debug(`token(stream=${stream},state=${state}): state.escaped → ${state.escaped}`);
        return "comment";
      }

      if (stream.match(/^('([^']|\\.)*'?|"([^"]|\\.)*"?)/))
        return "string";

      if (state.literal && stream.indentation() > state.keyCol) {
        stream.skipToEnd(); return "string";
      } else if (state.literal) { state.literal = false; }
      if (stream.sol()) {
        state.keyCol = 0;
		logger.debug(`function(exports=${exports}): state.keyCol → ${state.keyCol}`);
		logger.debug(`function(exports=${exports}): state.pairStart → ${state.pairStart}`);
        state.pair = false;
		logger.debug(`function(exports=${exports}): state.pair → ${state.pair}`);
        state.pairStart = false;
        /* document start */
        if(stream.match(/---/)) { return "def"; }
		logger.debug(`defineMode(): state.keyCol → ${state.keyCol}`);
        /* document end */
        if (stream.match(/\.\.\./)) { return "def"; }
		logger.debug(`token(stream=${stream},state=${state}): state.keyCol → ${state.keyCol}`);
        /* array list item */
        if (stream.match(/\s*-\s+/)) { return 'meta'; }
		logger.debug(`defineMode(): state.pair → ${state.pair}`);
      }
      /* inline pairs/lists */
      if (stream.match(/^(\{|\}|\[|\])/)) {
		logger.debug(`token(stream=${stream},state=${state}): state.pair → ${state.pair}`);
        if (ch == '{')
          state.inlinePairs++;
        else if (ch == '}')
          state.inlinePairs--;
        else if (ch == '[')
          state.inlineList++;
        else
          state.inlineList--;
        return 'meta';
      }

      /* list seperator */
      if (state.inlineList > 0 && !esc && ch == ',') {
        stream.next();
        return 'meta';
      }
      /* pairs seperator */
      if (state.inlinePairs > 0 && !esc && ch == ',') {
        state.keyCol = 0;
        state.pair = false;
        state.pairStart = false;
        stream.next();
        return 'meta';
      }

      /* start of value of a pair */
      if (state.pairStart) {
        /* block literals */
        if (stream.match(/^\s*(\||\>)\s*/)) { state.literal = true; return 'meta'; };
        /* references */
        if (stream.match(/^\s*(\&|\*)[a-z0-9\._-]+\b/i)) { return 'variable-2'; }
        /* numbers */
        if (state.inlinePairs == 0 && stream.match(/^\s*-?[0-9\.\,]+\s?$/)) { return 'number'; }
        if (state.inlinePairs > 0 && stream.match(/^\s*-?[0-9\.\,]+\s?(?=(,|}))/)) { return 'number'; }
        /* keywords */
        if (stream.match(keywordRegex)) { return 'keyword'; }
      }
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);

      /* pairs (associative arrays) -> key */
      if (!state.pair && stream.match(/^\s*(?:[,\[\]{}&*!|>'"%@`][^\s'":]|[^,\[\]{}#&*!|>'"%@`])[^#]*?(?=\s*:($|\s))/)) {
        state.pair = true;
        state.keyCol = stream.indentation();
        return "atom";
      }
      if (state.pair && stream.match(/^:\s*/)) { state.pairStart = true; return 'meta'; }

      /* nothing found, continue */
      state.pairStart = false;
      state.escaped = (ch == '\\');
      stream.next();
      return null;
    },
    startState: function() {
	logger.debug('>>> Entering startState()');
	logger.debug('<<< Exiting defineMode()');
      return {
        pair: false,
        pairStart: false,
        keyCol: 0,
        inlinePairs: 0,
        inlineList: 0,
        literal: false,
        escaped: false
      };
	logger.debug('<<< Exiting startState()');
    }
  };
});

CodeMirror.defineMIME("text/x-yaml", "yaml");

});
