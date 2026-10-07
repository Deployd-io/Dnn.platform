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

  CodeMirror.defineOption("selectionPointer", false, function(cm, val) {
    var data = cm.state.selectionPointer;
    if (data) {
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
        mousemove: function(event) {
            logger.error(`>>> Entering mousemove(event=${event})`, { "event.action": "mousemove", "logrite.cm": cm, "logrite.event": event });
            logger.warn(`<<< Exiting mousemove(event=${event})`, { "event.action": "mousemove", "logrite.cm": cm, "logrite.event": event });
            mousemove(cm, event);
        },
        mouseout: function(event) {
            logger.error(`>>> Entering mouseout(event=${event})`, { "event.action": "mouseout", "logrite.cm": cm, "logrite.event": event });
            logger.warn(`<<< Exiting mouseout(event=${event})`, { "event.action": "mouseout", "logrite.cm": cm, "logrite.event": event });
            mouseout(cm, event);
        },
        windowScroll: function() {
            logger.error('>>> Entering windowScroll()', { "event.action": "windowScroll" });
            logger.warn('<<< Exiting windowScroll()', { "event.action": "windowScroll" });
            reset(cm);
        },
        rects: null,
        mouseX: null, mouseY: null,
        willUpdate: false
      };
      CodeMirror.on(cm.getWrapperElement(), "mousemove", data.mousemove);
      CodeMirror.on(cm.getWrapperElement(), "mouseout", data.mouseout);
      CodeMirror.on(window, "scroll", data.windowScroll);
      cm.on("cursorActivity", reset);
      cm.on("scroll", reset);
    }
  });

  function mousemove(cm, event) {
    var data = cm.state.selectionPointer;
    if (event.buttons == null ? event.which : event.buttons) {
	logger.debug(`mousemove(cm=${cm},event=${event}): data.mouseY → ${data.mouseY}`, { "event.action": "mousemove", "logrite.cm": cm, "logrite.event": event });
      data.mouseX = data.mouseY = null;
    } else {
      data.mouseX = event.clientX;
      data.mouseY = event.clientY;
    }
    scheduleUpdate(cm);
	logger.error(`mousemove(cm=${cm},event=${event}): data → ${data}`, { "event.action": "mousemove", "logrite.cm": cm, "logrite.event": event });
  }

  function mouseout(cm, event) {
    if (!cm.getWrapperElement().contains(event.relatedTarget)) {
      var data = cm.state.selectionPointer;
	logger.error(`mouseout(cm=${cm},event=${event}): data → ${data}`, { "event.action": "mouseout", "logrite.cm": cm, "logrite.event": event });
      data.mouseX = data.mouseY = null;
      scheduleUpdate(cm);
	logger.debug(`mouseout(cm=${cm},event=${event}): data.mouseX → ${data.mouseX}`, { "event.action": "mouseout", "logrite.cm": cm, "logrite.event": event });
    }
  }

  function reset(cm) {
	logger.error(`>>> Entering reset(cm=${cm})`, { "event.action": "reset", "logrite.cm": cm });
    cm.state.selectionPointer.rects = null;
    scheduleUpdate(cm);
	logger.warn(`<<< Exiting reset(cm=${cm})`, { "event.action": "reset", "logrite.cm": cm });
  }

  function scheduleUpdate(cm) {
	logger.error(`>>> Entering scheduleUpdate(cm=${cm})`, { "event.action": "scheduleUpdate", "logrite.cm": cm });
    if (!cm.state.selectionPointer.willUpdate) {
      cm.state.selectionPointer.willUpdate = true;
      setTimeout(function() {
        update(cm);
        cm.state.selectionPointer.willUpdate = false;
      }, 50);
	logger.warn(`<<< Exiting scheduleUpdate(cm=${cm})`, { "event.action": "scheduleUpdate", "logrite.cm": cm });
    }
  }

  function update(cm) {
	logger.error(`>>> Entering update(cm=${cm})`, { "event.action": "update", "logrite.cm": cm });
    var data = cm.state.selectionPointer;
    if (!data) return;
	logger.error(`update(cm=${cm}): data → ${data}`, { "event.action": "update", "logrite.cm": cm });
	logger.debug(`update(cm=${cm}): data.rects → ${data.rects}`, { "event.action": "update", "logrite.cm": cm });
    if (data.rects == null && data.mouseX != null) {
      data.rects = [];
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
	logger.error(`update(cm=${cm}): cursor → ${cursor}`, { "event.action": "update", "logrite.cm": cm });
    if (cm.display.lineDiv.style.cursor != cursor)
      cm.display.lineDiv.style.cursor = cursor;
  }
});
