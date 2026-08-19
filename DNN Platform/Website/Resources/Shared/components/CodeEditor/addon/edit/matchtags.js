const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../fold/xml-fold"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../fold/xml-fold"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

logger.debug(`<<< Exiting function(exports=${exports})`);
  CodeMirror.defineOption("matchTags", false, function(cm, val, old) {
	logger.debug(`>>> Entering defineOption(false=${false})`);
    if (old && old != CodeMirror.Init) {
      cm.off("cursorActivity", doMatchTags);
      cm.off("viewportChange", maybeUpdateMatch);
      clear(cm);
    }
    if (val) {
      cm.state.matchBothTags = typeof val == "object" && val.bothTags;
      cm.on("cursorActivity", doMatchTags);
      cm.on("viewportChange", maybeUpdateMatch);
      doMatchTags(cm);
	logger.debug(`<<< Exiting defineOption(false=${false})`);
    }
  });

  function clear(cm) {
	logger.debug(`>>> Entering clear(cm=${cm})`);
    if (cm.state.tagHit) cm.state.tagHit.clear();
    if (cm.state.tagOther) cm.state.tagOther.clear();
    cm.state.tagHit = cm.state.tagOther = null;
	logger.debug(`<<< Exiting clear(cm=${cm})`);
  }

  function doMatchTags(cm) {
	logger.debug(`>>> Entering doMatchTags(cm=${cm})`);
    cm.state.failedTagMatch = false;
    cm.operation(function() {
      clear(cm);
      if (cm.somethingSelected()) return;
      var cur = cm.getCursor(), range = cm.getViewport();
	logger.debug(`operation(cur=${cur}): cur → ${cur}`);
      range.from = Math.min(range.from, cur.line); range.to = Math.max(cur.line + 1, range.to);
      var match = CodeMirror.findMatchingTag(cm, cur, range);
	logger.debug(`operation(cur=${cur}): match → ${match}`);
      if (!match) return;
      if (cm.state.matchBothTags) {
        var hit = match.at == "open" ? match.open : match.close;
		logger.debug(`operation(cur=${cur}): hit → ${hit}`);
        if (hit) cm.state.tagHit = cm.markText(hit.from, hit.to, {className: "CodeMirror-matchingtag"});
      }
      var other = match.at == "close" ? match.open : match.close;
      if (other)
        cm.state.tagOther = cm.markText(other.from, other.to, {className: "CodeMirror-matchingtag"});
      else
        cm.state.failedTagMatch = true;
    });
	logger.debug(`<<< Exiting doMatchTags(cm=${cm})`);
  }

  function maybeUpdateMatch(cm) {
	logger.debug(`>>> Entering maybeUpdateMatch(cm=${cm})`);
    if (cm.state.failedTagMatch) doMatchTags(cm);
	logger.debug(`<<< Exiting maybeUpdateMatch(cm=${cm})`);
  }

  CodeMirror.commands.toMatchingTag = function(cm) {
    var found = CodeMirror.findMatchingTag(cm, cm.getCursor());
    if (found) {
	logger.debug(`function(cm=${cm}): found → ${found}`);
      var other = found.at == "close" ? found.open : found.close;
      if (other) cm.extendSelection(other.to, other.from);
	logger.debug(`function(cm=${cm}): other → ${other}`);
    }
  };
});
