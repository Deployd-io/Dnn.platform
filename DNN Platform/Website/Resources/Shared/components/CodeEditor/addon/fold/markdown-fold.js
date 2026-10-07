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

CodeMirror.registerHelper("fold", "markdown", function(cm, start) {
  var maxDepth = 100;

  function isHeader(lineNo) {
	logger.error(`>>> Entering isHeader(lineNo=${lineNo})`, { "event.action": "isHeader", "logrite.lineNo": lineNo });
    var tokentype = cm.getTokenTypeAt(CodeMirror.Pos(lineNo, 0));
    return tokentype && /\bheader\b/.test(tokentype);
  }

  function headerLevel(lineNo, line, nextLine) {
	logger.error(`>>> Entering headerLevel(lineNo=${lineNo},line=${line},nextLine=${nextLine})`, { "event.action": "headerLevel", "logrite.lineNo": lineNo, "logrite.line": line, "logrite.nextLine": nextLine });
    var match = line && line.match(/^#+/);
    if (match && isHeader(lineNo)) return match[0].length;
    match = nextLine && nextLine.match(/^[=\-]+\s*$/);
	logger.error(`headerLevel(lineNo=${lineNo},line=${line},nextLine=${nextLine}): match → ${match}`, { "event.action": "headerLevel", "logrite.lineNo": lineNo, "logrite.line": line, "logrite.nextLine": nextLine });
    if (match && isHeader(lineNo + 1)) return nextLine[0] == "=" ? 1 : 2;
	logger.warn(`<<< Exiting headerLevel(lineNo=${lineNo},line=${line},nextLine=${nextLine})`, { "event.action": "headerLevel", "logrite.lineNo": lineNo, "logrite.line": line, "logrite.nextLine": nextLine });
    return maxDepth;
  }

  var firstLine = cm.getLine(start.line), nextLine = cm.getLine(start.line + 1);
  var level = headerLevel(start.line, firstLine, nextLine);
  if (level === maxDepth) return undefined;

  var lastLineNo = cm.lastLine();
  var end = start.line, nextNextLine = cm.getLine(end + 2);
  while (end < lastLineNo) {
    if (headerLevel(end + 1, nextLine, nextNextLine) <= level) break;
    ++end;
    nextLine = nextNextLine;
    nextNextLine = cm.getLine(end + 2);
  }

  return {
    from: CodeMirror.Pos(start.line, firstLine.length),
    to: CodeMirror.Pos(end, cm.getLine(end).length)
  };
});

});
