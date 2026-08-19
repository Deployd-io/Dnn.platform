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

  var WORD = /[\w$]+/, RANGE = 500;

logger.debug(`<<< Exiting function(exports=${exports})`);
  CodeMirror.registerHelper("hint", "anyword", function(editor, options) {
	logger.debug('>>> Entering registerHelper()');
    var word = options && options.word || WORD;
    var range = options && options.range || RANGE;
    var cur = editor.getCursor(), curLine = editor.getLine(cur.line);
	logger.debug(`registerHelper(): cur → ${cur}`);
    var end = cur.ch, start = end;
    while (start && word.test(curLine.charAt(start - 1))) --start;
    var curWord = start != end && curLine.slice(start, end);
	logger.debug(`registerHelper(): curWord → ${curWord}`);

    var list = options && options.list || [], seen = {};
    var re = new RegExp(word.source, "g");
	logger.debug(`registerHelper(): re → ${re}`);
    for (var dir = -1; dir <= 1; dir += 2) {
      var line = cur.line, endLine = Math.min(Math.max(line + dir * range, editor.firstLine()), editor.lastLine()) + dir;
	logger.debug(`registerHelper(): line → ${line}`);
      for (; line != endLine; line += dir) {
        var text = editor.getLine(line), m;
		logger.debug(`registerHelper(): text → ${text}`);
        while (m = re.exec(text)) {
          if (line == cur.line && m[0] === curWord) continue;
          if ((!curWord || m[0].lastIndexOf(curWord, 0) == 0) && !Object.prototype.hasOwnProperty.call(seen, m[0])) {
            seen[m[0]] = true;
            list.push(m[0]);
			logger.debug('<<< Exiting registerHelper()');
          }
        }
      }
    }
    return {list: list, from: CodeMirror.Pos(cur.line, start), to: CodeMirror.Pos(cur.line, end)};
  });
});
