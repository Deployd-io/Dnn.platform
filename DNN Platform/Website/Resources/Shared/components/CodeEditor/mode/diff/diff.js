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

CodeMirror.defineMode("diff", function() {
logger.debug('>>> Entering defineMode()');

  var TOKEN_NAMES = {
    '+': 'positive',
    '-': 'negative',
    '@': 'meta'
  };

  return {
    token: function(stream) {
	logger.debug(`>>> Entering token(stream=${stream})`);
      var tw_pos = stream.string.search(/[\t ]+?$/);

      if (!stream.sol() || tw_pos === 0) {
		logger.debug(`token(stream=${stream}): tw_pos → ${tw_pos}`);
        stream.skipToEnd();
        return ("error " + (
          TOKEN_NAMES[stream.string.charAt(0)] || '')).replace(/ $/, '');
      }

      var token_name = TOKEN_NAMES[stream.peek()] || stream.skipToEnd();

      if (tw_pos === -1) {
        stream.skipToEnd();
      } else {
        stream.pos = tw_pos;
		logger.debug(`function(exports=${exports}): stream.pos → ${stream.pos}`);
      }

	logger.debug(`token(stream=${stream}): stream.pos → ${stream.pos}`);
	logger.debug(`<<< Exiting token(stream=${stream})`);
      return token_name;
    }
  };
});
logger.debug('<<< Exiting defineMode()');

CodeMirror.defineMIME("text/x-diff", "diff");

});
logger.debug(`<<< Exiting function(exports=${exports})`);
