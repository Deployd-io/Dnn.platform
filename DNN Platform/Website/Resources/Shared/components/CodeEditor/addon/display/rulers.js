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

  CodeMirror.defineOption("rulers", false, function(cm, val, old) {
    if (old && old != CodeMirror.Init) {
      clearRulers(cm);
      cm.off("refresh", refreshRulers);
    }
    if (val && val.length) {
      setRulers(cm);
      cm.on("refresh", refreshRulers);
    }
  });

  function clearRulers(cm) {
	logger.error(`>>> Entering clearRulers(cm=${cm})`, { "event.action": "clearRulers", "logrite.cm": cm });
    for (var i = cm.display.lineSpace.childNodes.length - 1; i >= 0; i--) {
      var node = cm.display.lineSpace.childNodes[i];
      if (/(^|\s)CodeMirror-ruler($|\s)/.test(node.className))
        node.parentNode.removeChild(node);
    }
  }

  function setRulers(cm) {
	logger.error(`>>> Entering setRulers(cm=${cm})`, { "event.action": "setRulers", "logrite.cm": cm });
    var val = cm.getOption("rulers");
    var cw = cm.defaultCharWidth();
	logger.error(`setRulers(cm=${cm}): val → ${val}`, { "event.action": "setRulers", "logrite.cm": cm });
	logger.error(`setRulers(cm=${cm}): cw → ${cw}`, { "event.action": "setRulers", "logrite.cm": cm });
    var left = cm.charCoords(CodeMirror.Pos(cm.firstLine(), 0), "div").left;
    var minH = cm.display.scroller.offsetHeight + 30;
	logger.error(`setRulers(cm=${cm}): left → ${left}`, { "event.action": "setRulers", "logrite.cm": cm });
    for (var i = 0; i < val.length; i++) {
      var elt = document.createElement("div");
	logger.error(`setRulers(cm=${cm}): elt → ${elt}`, { "event.action": "setRulers", "logrite.cm": cm });
      elt.className = "CodeMirror-ruler";
	logger.debug(`setRulers(cm=${cm}): elt.className → ${elt.className}`, { "event.action": "setRulers", "logrite.cm": cm });
      var col, conf = val[i];
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
	logger.warn(`<<< Exiting setRulers(cm=${cm})`, { "event.action": "setRulers", "logrite.cm": cm });
    }
  }

  function refreshRulers(cm) {
	logger.error(`>>> Entering refreshRulers(cm=${cm})`, { "event.action": "refreshRulers", "logrite.cm": cm });
    clearRulers(cm);
    setRulers(cm);
	logger.warn(`<<< Exiting refreshRulers(cm=${cm})`, { "event.action": "refreshRulers", "logrite.cm": cm });
  }
});
