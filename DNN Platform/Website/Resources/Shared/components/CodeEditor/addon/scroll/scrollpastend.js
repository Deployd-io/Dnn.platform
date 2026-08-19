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
  CodeMirror.defineOption("scrollPastEnd", false, function(cm, val, old) {
	logger.debug(`>>> Entering defineOption(false=${false})`);
    if (old && old != CodeMirror.Init) {
      cm.off("change", onChange);
      cm.off("refresh", updateBottomMargin);
      cm.display.lineSpace.parentNode.style.paddingBottom = "";
      cm.state.scrollPastEndPadding = null;
    }
    if (val) {
      cm.on("change", onChange);
      cm.on("refresh", updateBottomMargin);
      updateBottomMargin(cm);
	logger.debug(`<<< Exiting defineOption(false=${false})`);
    }
  });

  function onChange(cm, change) {
    if (CodeMirror.changeEnd(change).line == cm.lastLine())
      updateBottomMargin(cm);
  }

  function updateBottomMargin(cm) {
	logger.debug(`>>> Entering updateBottomMargin(cm=${cm})`);
    var padding = "";
    if (cm.lineCount() > 1) {
	logger.debug(`updateBottomMargin(cm=${cm}): padding → ${padding}`);
      var totalH = cm.display.scroller.clientHeight - 30,
          lastLineH = cm.getLineHandle(cm.lastLine()).height;
      padding = (totalH - lastLineH) + "px";
    }
    if (cm.state.scrollPastEndPadding != padding) {
      cm.state.scrollPastEndPadding = padding;
      cm.display.lineSpace.parentNode.style.paddingBottom = padding;
      cm.setSize();
	logger.debug(`<<< Exiting updateBottomMargin(cm=${cm})`);
    }
  }
});
