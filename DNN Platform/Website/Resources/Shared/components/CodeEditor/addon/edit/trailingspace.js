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
logger.debug(`<<< Exiting function(exports=${exports})`);
  CodeMirror.defineOption("showTrailingSpace", false, function(cm, val, prev) {
	logger.debug(`>>> Entering defineOption(false=${false})`);
    if (prev == CodeMirror.Init) prev = false;
    if (prev && !val)
      cm.removeOverlay("trailingspace");
    else if (!prev && val)
      cm.addOverlay({
        token: function(stream) {
		logger.debug(`>>> Entering token(stream=${stream})`);
          for (var l = stream.string.length, i = l; i && /\s/.test(stream.string.charAt(i - 1)); --i) {}
          if (i > stream.pos) { stream.pos = i; return null; }
		logger.debug(`token(stream=${stream}): l → ${l}`);
          stream.pos = l;
		logger.debug(`<<< Exiting token(stream=${stream})`);
		logger.debug(`function(exports=${exports}): stream.pos → ${stream.pos}`);
          return "trailingspace";
        },
        name: "trailingspace"
      });
  });
});
