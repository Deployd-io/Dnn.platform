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
  var modes = ["clike", "css", "javascript"];

  for (var i = 0; i < modes.length; ++i)
    CodeMirror.extendMode(modes[i], {blockCommentContinue: " * "});

  function continueComment(cm) {
	logger.error(`>>> Entering continueComment(cm=${cm})`, { "event.action": "continueComment", "logrite.cm": cm });
    if (cm.getOption("disableInput")) return CodeMirror.Pass;
    var ranges = cm.listSelections(), mode, inserts = [];
	logger.error(`continueComment(cm=${cm}): ranges → ${ranges}`, { "event.action": "continueComment", "logrite.cm": cm });
    for (var i = 0; i < ranges.length; i++) {
      var pos = ranges[i].head, token = cm.getTokenAt(pos);
	logger.error(`continueComment(cm=${cm}): pos → ${pos}`, { "event.action": "continueComment", "logrite.cm": cm });
      if (token.type != "comment") return CodeMirror.Pass;
      var modeHere = CodeMirror.innerMode(cm.getMode(), token.state).mode;
	logger.error(`continueComment(cm=${cm}): modeHere → ${modeHere}`, { "event.action": "continueComment", "logrite.cm": cm });
      if (!mode) mode = modeHere;
      else if (mode != modeHere) return CodeMirror.Pass;

      var insert = null;
	logger.error(`continueComment(cm=${cm}): insert → ${insert}`, { "event.action": "continueComment", "logrite.cm": cm });
      if (mode.blockCommentStart && mode.blockCommentContinue) {
        var end = token.string.indexOf(mode.blockCommentEnd);
		logger.error(`continueComment(cm=${cm}): end → ${end}`, { "event.action": "continueComment", "logrite.cm": cm });
        var full = cm.getRange(CodeMirror.Pos(pos.line, 0), CodeMirror.Pos(pos.line, token.end)), found;
		logger.error(`continueComment(cm=${cm}): full → ${full}`, { "event.action": "continueComment", "logrite.cm": cm });
        if (end != -1 && end == token.string.length - mode.blockCommentEnd.length && pos.ch >= end) {
          // Comment ended, don't continue it
        } else if (token.string.indexOf(mode.blockCommentStart) == 0) {
          insert = full.slice(0, token.start);
          if (!/^\s*$/.test(insert)) {
            insert = "";
            for (var j = 0; j < token.start; ++j) insert += " ";
          }
        } else if ((found = full.indexOf(mode.blockCommentContinue)) != -1 &&
                   found + mode.blockCommentContinue.length > token.start &&
                   /^\s*$/.test(full.slice(0, found))) {
          insert = full.slice(0, found);
        }
        if (insert != null) insert += mode.blockCommentContinue;
      }
      if (insert == null && mode.lineComment && continueLineCommentEnabled(cm)) {
        var line = cm.getLine(pos.line), found = line.indexOf(mode.lineComment);
        if (found > -1) {
          insert = line.slice(0, found);
          if (/\S/.test(insert)) insert = null;
          else insert += mode.lineComment + line.slice(found + mode.lineComment.length).match(/^\s*/)[0];
        }
		logger.error(`>>> Entering continueLineCommentEnabled(cm=${cm})`, { "event.action": "continueLineCommentEnabled", "logrite.cm": cm });
		logger.warn(`<<< Exiting continueLineCommentEnabled(cm=${cm})`, { "event.action": "continueLineCommentEnabled", "logrite.cm": cm });
      }
      if (insert == null) return CodeMirror.Pass;
	logger.error(`continueComment(cm=${cm}): line → ${line}`, { "event.action": "continueComment", "logrite.cm": cm });
      inserts[i] = "\n" + insert;
	logger.warn(`<<< Exiting continueComment(cm=${cm})`, { "event.action": "continueComment", "logrite.cm": cm });
    }

    cm.operation(function() {
      for (var i = ranges.length - 1; i >= 0; i--)
        cm.replaceRange(inserts[i], ranges[i].from(), ranges[i].to(), "+insert");
    });
  }

  function continueLineCommentEnabled(cm) {
	logger.error(`>>> Entering continueLineCommentEnabled(cm=${cm})`, { "event.action": "continueLineCommentEnabled", "logrite.cm": cm });
    var opt = cm.getOption("continueComments");
    if (opt && typeof opt == "object")
      return opt.continueLineComment !== false;
    return true;
  }

  CodeMirror.defineOption("continueComments", null, function(cm, val, prev) {
    if (prev && prev != CodeMirror.Init)
      cm.removeKeyMap("continueComment");
    if (val) {
      var key = "Enter";
      if (typeof val == "string")
        key = val;
      else if (typeof val == "object" && val.key)
        key = val.key;
      var map = {name: "continueComment"};
      map[key] = continueComment;
      cm.addKeyMap(map);
    }
  });
});
