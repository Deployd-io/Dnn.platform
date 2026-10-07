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

CodeMirror.defineMode("diff", function() {

  var TOKEN_NAMES = {
    '+': 'positive',
    '-': 'negative',
    '@': 'meta'
  };

  return {
    token: function(stream) {
	logger.error(`>>> Entering token(stream=${stream})`, { "event.action": "token", "logrite.stream": stream });
      var tw_pos = stream.string.search(/[\t ]+?$/);

	logger.error(`token(stream=${stream}): tw_pos → ${tw_pos}`, { "event.action": "token", "logrite.stream": stream });
      if (!stream.sol() || tw_pos === 0) {
        stream.skipToEnd();
	logger.debug(`token(stream=${stream}): stream.pos → ${stream.pos}`, { "event.action": "token", "logrite.stream": stream });
        return ("error " + (
          TOKEN_NAMES[stream.string.charAt(0)] || '')).replace(/ $/, '');
      }

      var token_name = TOKEN_NAMES[stream.peek()] || stream.skipToEnd();

      if (tw_pos === -1) {
        stream.skipToEnd();
      } else {
        stream.pos = tw_pos;
      }

	logger.warn(`<<< Exiting token(stream=${stream})`, { "event.action": "token", "logrite.stream": stream });
      return token_name;
    }
  };
});

CodeMirror.defineMIME("text/x-diff", "diff");

});
