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
  CodeMirror.defineOption("selectionPointer", false, function(cm, val) {
	logger.debug('>>> Entering defineOption()');
    var data = cm.state.selectionPointer;
    if (data) {
	logger.debug(`defineOption(): data → ${data}`);
      CodeMirror.off(cm.getWrapperElement(), "mousemove", data.mousemove);
      CodeMirror.off(cm.getWrapperElement(), "mouseout", data.mouseout);
      CodeMirror.off(window, "scroll", data.windowScroll);
      cm.off("cursorActivity", reset);
      cm.off("scroll", reset);
      cm.state.selectionPointer = null;
      cm.display.lineDiv.style.cursor = "";
    }
    if (val) {
      data = cm.state.selectionPointer = {
        value: typeof val == "string" ? val : "default",
        mousemove: function(event) { mousemove(cm, event); },
        mouseout: function(event) { mouseout(cm, event); },
        windowScroll: function() { reset(cm); },
        rects: null,
        mouseX: null, mouseY: null,
        willUpdate: false
      };
      CodeMirror.on(cm.getWrapperElement(), "mousemove", data.mousemove);
      CodeMirror.on(cm.getWrapperElement(), "mouseout", data.mouseout);
      CodeMirror.on(window, "scroll", data.windowScroll);
      cm.on("cursorActivity", reset);
      cm.on("scroll", reset);
	logger.debug('<<< Exiting defineOption()');
    }
  });

  function mousemove(cm, event) {
	logger.debug(`>>> Entering mousemove(cm=${cm},event=${event})`);
    var data = cm.state.selectionPointer;
    if (event.buttons == null ? event.which : event.buttons) {
      data.mouseX = data.mouseY = null;
    } else {
      data.mouseX = event.clientX;
      data.mouseY = event.clientY;
	logger.debug(`mousemove(cm=${cm},event=${event}): data.mouseY → ${data.mouseY}`);
    }
    scheduleUpdate(cm);
	logger.debug(`mousemove(cm=${cm},event=${event}): data → ${data}`);
	logger.debug(`<<< Exiting mousemove(cm=${cm},event=${event})`);
  }

  function mouseout(cm, event) {
	logger.debug(`>>> Entering mouseout(cm=${cm},event=${event})`);
    if (!cm.getWrapperElement().contains(event.relatedTarget)) {
      var data = cm.state.selectionPointer;
	logger.debug(`mouseout(cm=${cm},event=${event}): data → ${data}`);
      data.mouseX = data.mouseY = null;
      scheduleUpdate(cm);
	logger.debug(`mouseout(cm=${cm},event=${event}): data.mouseX → ${data.mouseX}`);
    }
	logger.debug(`<<< Exiting mouseout(cm=${cm},event=${event})`);
  }

  function reset(cm) {
	logger.debug(`>>> Entering reset(cm=${cm})`);
    cm.state.selectionPointer.rects = null;
    scheduleUpdate(cm);
	logger.debug(`<<< Exiting reset(cm=${cm})`);
  }

  function scheduleUpdate(cm) {
	logger.debug(`>>> Entering scheduleUpdate(cm=${cm})`);
    if (!cm.state.selectionPointer.willUpdate) {
      cm.state.selectionPointer.willUpdate = true;
	logger.debug(`<<< Exiting scheduleUpdate(cm=${cm})`);
      setTimeout(function() {
		logger.debug('>>> Entering setTimeout()');
        update(cm);
		logger.debug('<<< Exiting setTimeout()');
        cm.state.selectionPointer.willUpdate = false;
      }, 50);
    }
  }

  function update(cm) {
	logger.debug(`>>> Entering update(cm=${cm})`);
    var data = cm.state.selectionPointer;
    if (!data) return;
	logger.debug(`update(cm=${cm}): data → ${data}`);
    if (data.rects == null && data.mouseX != null) {
      data.rects = [];
	logger.debug(`update(cm=${cm}): data.rects → ${data.rects}`);
      if (cm.somethingSelected()) {
        for (var sel = cm.display.selectionDiv.firstChild; sel; sel = sel.nextSibling)
          data.rects.push(sel.getBoundingClientRect());
      }
    }
    var inside = false;
    if (data.mouseX != null) for (var i = 0; i < data.rects.length; i++) {
      var rect = data.rects[i];
      if (rect.left <= data.mouseX && rect.right >= data.mouseX &&
          rect.top <= data.mouseY && rect.bottom >= data.mouseY)
        inside = true;
    }
    var cursor = inside ? data.value : "";
	logger.debug(`update(cm=${cm}): cursor → ${cursor}`);
    if (cm.display.lineDiv.style.cursor != cursor)
      cm.display.lineDiv.style.cursor = cursor;
  }
});
