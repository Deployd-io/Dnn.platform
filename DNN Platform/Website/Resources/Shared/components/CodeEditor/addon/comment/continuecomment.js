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
  var modes = ["clike", "css", "javascript"];
logger.debug(`function(exports=${exports}): modes → ${modes}`);

  for (var i = 0; i < modes.length; ++i)
    CodeMirror.extendMode(modes[i], {blockCommentContinue: " * "});

  function continueComment(cm) {
	logger.debug(`>>> Entering continueComment(cm=${cm})`);
    if (cm.getOption("disableInput")) return CodeMirror.Pass;
    var ranges = cm.listSelections(), mode, inserts = [];
	logger.debug(`continueComment(cm=${cm}): ranges → ${ranges}`);
    for (var i = 0; i < ranges.length; i++) {
      var pos = ranges[i].head, token = cm.getTokenAt(pos);
	logger.debug(`continueComment(cm=${cm}): pos → ${pos}`);
      if (token.type != "comment") return CodeMirror.Pass;
      var modeHere = CodeMirror.innerMode(cm.getMode(), token.state).mode;
	logger.debug(`continueComment(cm=${cm}): modeHere → ${modeHere}`);
      if (!mode) mode = modeHere;
      else if (mode != modeHere) return CodeMirror.Pass;

      var insert = null;
	logger.debug(`continueComment(cm=${cm}): insert → ${insert}`);
      if (mode.blockCommentStart && mode.blockCommentContinue) {
        var end = token.string.indexOf(mode.blockCommentEnd);
		logger.debug(`continueComment(cm=${cm}): end → ${end}`);
        var full = cm.getRange(CodeMirror.Pos(pos.line, 0), CodeMirror.Pos(pos.line, token.end)), found;
		logger.debug(`continueComment(cm=${cm}): full → ${full}`);
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
		logger.debug(`>>> Entering continueLineCommentEnabled(cm=${cm})`);
		logger.debug(`<<< Exiting continueLineCommentEnabled(cm=${cm})`);
      }
      if (insert == null) return CodeMirror.Pass;
	logger.debug(`continueComment(cm=${cm}): line → ${line}`);
      inserts[i] = "\n" + insert;
	logger.debug(`<<< Exiting continueComment(cm=${cm})`);
    }

    cm.operation(function() {
	logger.debug(`>>> Entering operation(i=${i})`);
      for (var i = ranges.length - 1; i >= 0; i--)
        cm.replaceRange(inserts[i], ranges[i].from(), ranges[i].to(), "+insert");
    });
	logger.debug(`<<< Exiting operation(i=${i})`);
  }

  function continueLineCommentEnabled(cm) {
	logger.debug(`>>> Entering continueLineCommentEnabled(cm=${cm})`);
    var opt = cm.getOption("continueComments");
    if (opt && typeof opt == "object")
      return opt.continueLineComment !== false;
    return true;
  }

  CodeMirror.defineOption("continueComments", null, function(cm, val, prev) {
	logger.debug(`>>> Entering defineOption(null=${null})`);
    if (prev && prev != CodeMirror.Init)
      cm.removeKeyMap("continueComment");
    if (val) {
      var key = "Enter";
	logger.warn(`defineOption(null=${null}): key → ${key}`);
      if (typeof val == "string")
        key = val;
      else if (typeof val == "object" && val.key)
        key = val.key;
      var map = {name: "continueComment"};
	logger.debug(`defineOption(null=${null}): map → ${map}`);
      map[key] = continueComment;
      cm.addKeyMap(map);
	logger.debug(`<<< Exiting defineOption(null=${null})`);
    }
  });
});
