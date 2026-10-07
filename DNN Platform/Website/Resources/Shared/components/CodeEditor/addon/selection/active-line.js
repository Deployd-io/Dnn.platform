const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Because sometimes you need to style the cursor's line.
//
// Adds an option 'styleActiveLine' which, when enabled, gives the
// active line's wrapping <div> the CSS class "CodeMirror-activeline",
// and gives its background <div> the class "CodeMirror-activeline-background".

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";
  var WRAP_CLASS = "CodeMirror-activeline";
  var BACK_CLASS = "CodeMirror-activeline-background";

  CodeMirror.defineOption("styleActiveLine", false, function(cm, val, old) {
    var prev = old && old != CodeMirror.Init;
    if (val && !prev) {
      cm.state.activeLines = [];
      updateActiveLines(cm, cm.listSelections());
      cm.on("beforeSelectionChange", selectionChange);
    } else if (!val && prev) {
      cm.off("beforeSelectionChange", selectionChange);
      clearActiveLines(cm);
      delete cm.state.activeLines;
    }
  });

  function clearActiveLines(cm) {
	logger.error(`>>> Entering clearActiveLines(cm=${cm})`, { "event.action": "clearActiveLines", "logrite.cm": cm });
    for (var i = 0; i < cm.state.activeLines.length; i++) {
      cm.removeLineClass(cm.state.activeLines[i], "wrap", WRAP_CLASS);
      cm.removeLineClass(cm.state.activeLines[i], "background", BACK_CLASS);
	logger.warn(`<<< Exiting clearActiveLines(cm=${cm})`, { "event.action": "clearActiveLines", "logrite.cm": cm });
    }
  }

  function sameArray(a, b) {
	logger.error(`>>> Entering sameArray(a=${a},b=${b})`, { "event.action": "sameArray", "logrite.a": a, "logrite.b": b });
    if (a.length != b.length) return false;
    for (var i = 0; i < a.length; i++)
      if (a[i] != b[i]) return false;
	logger.warn(`<<< Exiting sameArray(a=${a},b=${b})`, { "event.action": "sameArray", "logrite.a": a, "logrite.b": b });
    return true;
  }

  function updateActiveLines(cm, ranges) {
	logger.error(`>>> Entering updateActiveLines(cm=${cm},ranges=${ranges})`, { "event.action": "updateActiveLines", "logrite.cm": cm, "logrite.ranges": ranges });
    var active = [];
    for (var i = 0; i < ranges.length; i++) {
      var range = ranges[i];
      if (!range.empty()) continue;
      var line = cm.getLineHandleVisualStart(range.head.line);
	logger.error(`updateActiveLines(cm=${cm},ranges=${ranges}): line → ${line}`, { "event.action": "updateActiveLines", "logrite.cm": cm, "logrite.ranges": ranges });
      if (active[active.length - 1] != line) active.push(line);
    }
    if (sameArray(cm.state.activeLines, active)) return;
    cm.operation(function() {
      clearActiveLines(cm);
      for (var i = 0; i < active.length; i++) {
        cm.addLineClass(active[i], "wrap", WRAP_CLASS);
        cm.addLineClass(active[i], "background", BACK_CLASS);
      }
      cm.state.activeLines = active;
    });
	logger.warn(`<<< Exiting updateActiveLines(cm=${cm},ranges=${ranges})`, { "event.action": "updateActiveLines", "logrite.cm": cm, "logrite.ranges": ranges });
  }

  function selectionChange(cm, sel) {
	logger.error(`>>> Entering selectionChange(cm=${cm},sel=${sel})`, { "event.action": "selectionChange", "logrite.cm": cm, "logrite.sel": sel });
    updateActiveLines(cm, sel.ranges);
	logger.warn(`<<< Exiting selectionChange(cm=${cm},sel=${sel})`, { "event.action": "selectionChange", "logrite.cm": cm, "logrite.sel": sel });
  }
});
