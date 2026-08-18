const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Because sometimes you need to style the cursor's line.
//
// Adds an option 'styleActiveLine' which, when enabled, gives the
// active line's wrapping <div> the CSS class "CodeMirror-activeline",
// and gives its background <div> the class "CodeMirror-activeline-background".

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
  var WRAP_CLASS = "CodeMirror-activeline";
logger.debug(`function(exports=${exports}): WRAP_CLASS → ${WRAP_CLASS}`);
  var BACK_CLASS = "CodeMirror-activeline-background";

logger.debug(`function(exports=${exports}): BACK_CLASS → ${BACK_CLASS}`);
  CodeMirror.defineOption("styleActiveLine", false, function(cm, val, old) {
	logger.debug(`<<< Exiting function(exports=${exports})`);
    var prev = old && old != CodeMirror.Init;
	logger.debug(`>>> Entering defineOption(false=${false},prev=${prev})`);
    if (val && !prev) {
      cm.state.activeLines = [];
      updateActiveLines(cm, cm.listSelections());
      cm.on("beforeSelectionChange", selectionChange);
    } else if (!val && prev) {
      cm.off("beforeSelectionChange", selectionChange);
      clearActiveLines(cm);
      delete cm.state.activeLines;
	logger.debug(`<<< Exiting defineOption(false=${false},prev=${prev})`);
    }
  });

  function clearActiveLines(cm) {
	logger.debug(`>>> Entering clearActiveLines(cm=${cm})`);
    for (var i = 0; i < cm.state.activeLines.length; i++) {
      cm.removeLineClass(cm.state.activeLines[i], "wrap", WRAP_CLASS);
      cm.removeLineClass(cm.state.activeLines[i], "background", BACK_CLASS);
	logger.debug(`<<< Exiting clearActiveLines(cm=${cm})`);
    }
  }

  function sameArray(a, b) {
	logger.debug(`>>> Entering sameArray(a=${a},b=${b})`);
    if (a.length != b.length) return false;
    for (var i = 0; i < a.length; i++)
      if (a[i] != b[i]) return false;
	logger.debug(`<<< Exiting sameArray(a=${a},b=${b})`);
    return true;
  }

  function updateActiveLines(cm, ranges) {
	logger.debug(`>>> Entering updateActiveLines(cm=${cm},ranges=${ranges})`);
    var active = [];
    for (var i = 0; i < ranges.length; i++) {
      var range = ranges[i];
      if (!range.empty()) continue;
      var line = cm.getLineHandleVisualStart(range.head.line);
	logger.debug(`updateActiveLines(cm=${cm},ranges=${ranges}): line → ${line}`);
      if (active[active.length - 1] != line) active.push(line);
    }
    if (sameArray(cm.state.activeLines, active)) return;
	logger.debug(`<<< Exiting updateActiveLines(cm=${cm},ranges=${ranges})`);
    cm.operation(function() {
	logger.debug(`>>> Entering operation(i=${i})`);
      clearActiveLines(cm);
      for (var i = 0; i < active.length; i++) {
        cm.addLineClass(active[i], "wrap", WRAP_CLASS);
        cm.addLineClass(active[i], "background", BACK_CLASS);
      }
      cm.state.activeLines = active;
    });
	logger.debug(`<<< Exiting operation(i=${i})`);
  }

  function selectionChange(cm, sel) {
	logger.debug(`>>> Entering selectionChange(cm=${cm},sel=${sel})`);
    updateActiveLines(cm, sel.ranges);
	logger.debug(`<<< Exiting selectionChange(cm=${cm},sel=${sel})`);
  }
});
