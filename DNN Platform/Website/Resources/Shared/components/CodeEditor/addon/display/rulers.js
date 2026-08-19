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
  CodeMirror.defineOption("rulers", false, function(cm, val, old) {
	logger.debug(`>>> Entering defineOption(false=${false})`);
    if (old && old != CodeMirror.Init) {
      clearRulers(cm);
      cm.off("refresh", refreshRulers);
    }
    if (val && val.length) {
      setRulers(cm);
      cm.on("refresh", refreshRulers);
	logger.debug(`<<< Exiting defineOption(false=${false})`);
    }
  });

  function clearRulers(cm) {
	logger.debug(`>>> Entering clearRulers(cm=${cm})`);
    for (var i = cm.display.lineSpace.childNodes.length - 1; i >= 0; i--) {
      var node = cm.display.lineSpace.childNodes[i];
      if (/(^|\s)CodeMirror-ruler($|\s)/.test(node.className))
		logger.debug(`<<< Exiting clearRulers(cm=${cm})`);
        node.parentNode.removeChild(node);
    }
  }

  function setRulers(cm) {
	logger.debug(`>>> Entering setRulers(cm=${cm})`);
    var val = cm.getOption("rulers");
    var cw = cm.defaultCharWidth();
	logger.debug(`setRulers(cm=${cm}): val → ${val}`);
	logger.debug(`setRulers(cm=${cm}): cw → ${cw}`);
    var left = cm.charCoords(CodeMirror.Pos(cm.firstLine(), 0), "div").left;
    var minH = cm.display.scroller.offsetHeight + 30;
	logger.debug(`setRulers(cm=${cm}): left → ${left}`);
    for (var i = 0; i < val.length; i++) {
      var elt = document.createElement("div");
	logger.debug(`setRulers(cm=${cm}): elt → ${elt}`);
      elt.className = "CodeMirror-ruler";
      var col, conf = val[i];
	logger.debug(`setRulers(cm=${cm}): elt.className → ${elt.className}`);
      if (typeof conf == "number") {
        col = conf;
      } else {
        col = conf.column;
        if (conf.className) elt.className += " " + conf.className;
        if (conf.color) elt.style.borderColor = conf.color;
        if (conf.lineStyle) elt.style.borderLeftStyle = conf.lineStyle;
        if (conf.width) elt.style.borderLeftWidth = conf.width;
      }
      elt.style.left = (left + col * cw) + "px";
      elt.style.top = "-50px";
      elt.style.bottom = "-20px";
      elt.style.minHeight = minH + "px";
      cm.display.lineSpace.insertBefore(elt, cm.display.cursorDiv);
	logger.debug(`<<< Exiting setRulers(cm=${cm})`);
    }
  }

  function refreshRulers(cm) {
	logger.debug(`>>> Entering refreshRulers(cm=${cm})`);
    clearRulers(cm);
    setRulers(cm);
	logger.debug(`<<< Exiting refreshRulers(cm=${cm})`);
  }
});
