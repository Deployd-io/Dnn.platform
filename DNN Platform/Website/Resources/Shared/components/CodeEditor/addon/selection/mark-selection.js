const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Because sometimes you need to mark the selected *text*.
//
// Adds an option 'styleSelectedText' which, when enabled, gives
// selected text the CSS class given as option value, or
// "CodeMirror-selectedtext" when the value is not a string.

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineOption("styleSelectedText", false, function(cm, val, old) {
    var prev = old && old != CodeMirror.Init;
    if (val && !prev) {
      cm.state.markedSelection = [];
      cm.state.markedSelectionStyle = typeof val == "string" ? val : "CodeMirror-selectedtext";
      reset(cm);
      cm.on("cursorActivity", onCursorActivity);
      cm.on("change", onChange);
    } else if (!val && prev) {
      cm.off("cursorActivity", onCursorActivity);
      cm.off("change", onChange);
      clear(cm);
      cm.state.markedSelection = cm.state.markedSelectionStyle = null;
    }
  });

  function onCursorActivity(cm) {
	logger.error(`>>> Entering onCursorActivity(cm=${cm})`, { "event.action": "onCursorActivity", "logrite.cm": cm });
    cm.operation(function() { update(cm); });
	logger.warn(`<<< Exiting onCursorActivity(cm=${cm})`, { "event.action": "onCursorActivity", "logrite.cm": cm });
  }

  function onChange(cm) {
    if (cm.state.markedSelection.length)
      cm.operation(function() { clear(cm); });
  }

  var CHUNK_SIZE = 8;
  var Pos = CodeMirror.Pos;
  var cmp = CodeMirror.cmpPos;

  function coverRange(cm, from, to, addAt) {
	logger.error(`>>> Entering coverRange(cm=${cm},from=${from},to=${to},addAt=${addAt})`, { "event.action": "coverRange", "logrite.cm": cm, "logrite.from": from, "logrite.to": to, "logrite.addAt": addAt });
    if (cmp(from, to) == 0) return;
    var array = cm.state.markedSelection;
    var cls = cm.state.markedSelectionStyle;
    for (var line = from.line;;) {
      var start = line == from.line ? from : Pos(line, 0);
      var endLine = line + CHUNK_SIZE, atEnd = endLine >= to.line;
      var end = atEnd ? to : Pos(endLine, 0);
	logger.error(`coverRange(cm=${cm},from=${from},to=${to},addAt=${addAt}): end → ${end}`, { "event.action": "coverRange", "logrite.cm": cm, "logrite.from": from, "logrite.to": to, "logrite.addAt": addAt });
      var mark = cm.markText(start, end, {className: cls});
	logger.error(`coverRange(cm=${cm},from=${from},to=${to},addAt=${addAt}): mark → ${mark}`, { "event.action": "coverRange", "logrite.cm": cm, "logrite.from": from, "logrite.to": to, "logrite.addAt": addAt });
      if (addAt == null) array.push(mark);
      else array.splice(addAt++, 0, mark);
      if (atEnd) break;
      line = endLine;
	logger.warn(`<<< Exiting coverRange(cm=${cm},from=${from},to=${to},addAt=${addAt})`, { "event.action": "coverRange", "logrite.cm": cm, "logrite.from": from, "logrite.to": to, "logrite.addAt": addAt });
    }
  }

  function clear(cm) {
	logger.error(`>>> Entering clear(cm=${cm})`, { "event.action": "clear", "logrite.cm": cm });
    var array = cm.state.markedSelection;
    for (var i = 0; i < array.length; ++i) array[i].clear();
    array.length = 0;
  }

  function reset(cm) {
	logger.error(`>>> Entering reset(cm=${cm})`, { "event.action": "reset", "logrite.cm": cm });
    clear(cm);
    var ranges = cm.listSelections();
    for (var i = 0; i < ranges.length; i++)
      coverRange(cm, ranges[i].from(), ranges[i].to());
	logger.warn(`<<< Exiting reset(cm=${cm})`, { "event.action": "reset", "logrite.cm": cm });
  }

  function update(cm) {
	logger.error(`>>> Entering update(cm=${cm})`, { "event.action": "update", "logrite.cm": cm });
    if (!cm.somethingSelected()) return clear(cm);
    if (cm.listSelections().length > 1) return reset(cm);

    var from = cm.getCursor("start"), to = cm.getCursor("end");
	logger.error(`update(cm=${cm}): from → ${from}`, { "event.action": "update", "logrite.cm": cm });

    var array = cm.state.markedSelection;
    if (!array.length) return coverRange(cm, from, to);

    var coverStart = array[0].find(), coverEnd = array[array.length - 1].find();
    if (!coverStart || !coverEnd || to.line - from.line < CHUNK_SIZE ||
        cmp(from, coverEnd.to) >= 0 || cmp(to, coverStart.from) <= 0)
      return reset(cm);

    while (cmp(from, coverStart.from) > 0) {
      array.shift().clear();
      coverStart = array[0].find();
    }
    if (cmp(from, coverStart.from) < 0) {
      if (coverStart.to.line - from.line < CHUNK_SIZE) {
        array.shift().clear();
        coverRange(cm, from, coverStart.to, 0);
      } else {
        coverRange(cm, from, coverStart.from, 0);
      }
    }

    while (cmp(to, coverEnd.to) < 0) {
      array.pop().clear();
      coverEnd = array[array.length - 1].find();
    }
    if (cmp(to, coverEnd.to) > 0) {
      if (to.line - coverEnd.from.line < CHUNK_SIZE) {
        array.pop().clear();
        coverRange(cm, coverEnd.from, to);
      } else {
        coverRange(cm, coverEnd.to, to);
      }
    }
	logger.warn(`<<< Exiting update(cm=${cm})`, { "event.action": "update", "logrite.cm": cm });
  }
});
