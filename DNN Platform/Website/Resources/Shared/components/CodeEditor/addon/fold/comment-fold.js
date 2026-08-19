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

CodeMirror.registerGlobalHelper("fold", "comment", function(mode) {
logger.debug('>>> Entering registerGlobalHelper()');
logger.debug('<<< Exiting registerGlobalHelper()');
  return mode.blockCommentStart && mode.blockCommentEnd;
}, function(cm, start) {
  var mode = cm.getModeAt(start), startToken = mode.blockCommentStart, endToken = mode.blockCommentEnd;
logger.debug(`function(exports=${exports}): mode → ${mode}`);
  if (!startToken || !endToken) return;
  var line = start.line, lineText = cm.getLine(line);
logger.debug(`function(exports=${exports}): line → ${line}`);

  var startCh;
  for (var at = start.ch, pass = 0;;) {
    var found = at <= 0 ? -1 : lineText.lastIndexOf(startToken, at - 1);
	logger.debug(`function(exports=${exports}): found → ${found}`);
    if (found == -1) {
      if (pass == 1) return;
      pass = 1;
      at = lineText.length;
      continue;
    }
    if (pass == 1 && found < start.ch) return;
    if (/comment/.test(cm.getTokenTypeAt(CodeMirror.Pos(line, found + 1)))) {
      startCh = found + startToken.length;
      break;
	logger.debug(`function(exports=${exports}): startCh → ${startCh}`);
    }
    at = found - 1;
  }

  var depth = 1, lastLine = cm.lastLine(), end, endCh;
logger.debug(`function(exports=${exports}): depth → ${depth}`);
  outer: for (var i = line; i <= lastLine; ++i) {
    var text = cm.getLine(i), pos = i == line ? startCh : 0;
	logger.debug(`function(exports=${exports}): text → ${text}`);
    for (;;) {
      var nextOpen = text.indexOf(startToken, pos), nextClose = text.indexOf(endToken, pos);
	logger.debug(`function(exports=${exports}): nextOpen → ${nextOpen}`);
      if (nextOpen < 0) nextOpen = text.length;
      if (nextClose < 0) nextClose = text.length;
      pos = Math.min(nextOpen, nextClose);
      if (pos == text.length) break;
	logger.debug(`<<< Exiting function(exports=${exports})`);
      if (pos == nextOpen) ++depth;
      else if (!--depth) { end = i; endCh = pos; break outer; }
      ++pos;
    }
  }
  if (end == null || line == end && endCh == startCh) return;
  return {from: CodeMirror.Pos(line, startCh),
          to: CodeMirror.Pos(end, endCh)};
});

});
