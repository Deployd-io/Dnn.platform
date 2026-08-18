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

  var Pos = CodeMirror.Pos;
logger.debug(`<<< Exiting function(exports=${exports})`);

  function findParagraph(cm, pos, options) {
	logger.debug(`>>> Entering findParagraph(cm=${cm},pos=${pos},options=${options})`);
    var startRE = options.paragraphStart || cm.getHelper(pos, "paragraphStart");
    for (var start = pos.line, first = cm.firstLine(); start > first; --start) {
	logger.debug(`findParagraph(cm=${cm},pos=${pos},options=${options}): startRE → ${startRE}`);
      var line = cm.getLine(start);
      if (startRE && startRE.test(line)) break;
      if (!/\S/.test(line)) { ++start; break; }
    }
    var endRE = options.paragraphEnd || cm.getHelper(pos, "paragraphEnd");
    for (var end = pos.line + 1, last = cm.lastLine(); end <= last; ++end) {
      var line = cm.getLine(end);
	logger.debug(`findParagraph(cm=${cm},pos=${pos},options=${options}): line → ${line}`);
      if (endRE && endRE.test(line)) { ++end; break; }
      if (!/\S/.test(line)) break;
    }
	logger.debug(`findParagraph(cm=${cm},pos=${pos},options=${options}): end → ${end}`);
	logger.debug(`findParagraph(cm=${cm},pos=${pos},options=${options}): endRE → ${endRE}`);
	logger.debug(`<<< Exiting findParagraph(cm=${cm},pos=${pos},options=${options})`);
    return {from: start, to: end};
  }

  function findBreakPoint(text, column, wrapOn, killTrailingSpace) {
	logger.debug(`>>> Entering findBreakPoint(text=${text},column=${column},wrapOn=${wrapOn},killTrailingSpace=${killTrailingSpace})`);
    for (var at = column; at > 0; --at)
      if (wrapOn.test(text.slice(at - 1, at + 1))) break;
    if (at == 0) at = column;
    var endOfText = at;
    if (killTrailingSpace)
      while (text.charAt(endOfText - 1) == " ") --endOfText;
	logger.debug(`<<< Exiting findBreakPoint(text=${text},column=${column},wrapOn=${wrapOn},killTrailingSpace=${killTrailingSpace})`);
    return {from: endOfText, to: at};
  }

  function wrapRange(cm, from, to, options) {
	logger.debug(`>>> Entering wrapRange(cm=${cm},from=${from},to=${to},options=${options})`);
    from = cm.clipPos(from); to = cm.clipPos(to);
    var column = options.column || 80;
	logger.debug(`wrapRange(cm=${cm},from=${from},to=${to},options=${options}): from → ${from}`);
    var wrapOn = options.wrapOn || /\s\S|-[^\.\d]/;
    var killTrailing = options.killTrailingSpace !== false;
    var changes = [], curLine = "", curNo = from.line;
	logger.debug(`wrapRange(cm=${cm},from=${from},to=${to},options=${options}): changes → ${changes}`);
    var lines = cm.getRange(from, to, false);
	logger.debug(`wrapRange(cm=${cm},from=${from},to=${to},options=${options}): lines → ${lines}`);
    if (!lines.length) return null;
    var leadingSpace = lines[0].match(/^[ \t]*/)[0];
	logger.debug(`wrapRange(cm=${cm},from=${from},to=${to},options=${options}): leadingSpace → ${leadingSpace}`);

    for (var i = 0; i < lines.length; ++i) {
      var text = lines[i], oldLen = curLine.length, spaceInserted = 0;
	logger.debug(`wrapRange(cm=${cm},from=${from},to=${to},options=${options}): text → ${text}`);
      if (curLine && text && !wrapOn.test(curLine.charAt(curLine.length - 1) + text.charAt(0))) {
        curLine += " ";
        spaceInserted = 1;
      }
      var spaceTrimmed = "";
      if (i) {
        spaceTrimmed = text.match(/^\s*/)[0];
		logger.debug(`wrapRange(cm=${cm},from=${from},to=${to},options=${options}): spaceTrimmed → ${spaceTrimmed}`);
        text = text.slice(spaceTrimmed.length);
      }
      curLine += text;
      if (i) {
        var firstBreak = curLine.length > column && leadingSpace == spaceTrimmed &&
          findBreakPoint(curLine, column, wrapOn, killTrailing);
        // If this isn't broken, or is broken at a different point, remove old break
        if (!firstBreak || firstBreak.from != oldLen || firstBreak.to != oldLen + spaceInserted) {
          changes.push({text: [spaceInserted ? " " : ""],
                        from: Pos(curNo, oldLen),
                        to: Pos(curNo + 1, spaceTrimmed.length)});
        } else {
          curLine = leadingSpace + text;
          ++curNo;
        }
      }
      while (curLine.length > column) {
        var bp = findBreakPoint(curLine, column, wrapOn, killTrailing);
		logger.debug(`wrapRange(cm=${cm},from=${from},to=${to},options=${options}): bp → ${bp}`);
        changes.push({text: ["", leadingSpace],
                      from: Pos(curNo, bp.from),
                      to: Pos(curNo, bp.to)});
        curLine = leadingSpace + curLine.slice(bp.to);
        ++curNo;
      }
    }
    if (changes.length) cm.operation(function() {
	logger.debug('>>> Entering operation()');
      for (var i = 0; i < changes.length; ++i) {
        var change = changes[i];
        cm.replaceRange(change.text, change.from, change.to);
		logger.debug('<<< Exiting operation()');
      }
    });
	logger.debug(`<<< Exiting wrapRange(cm=${cm},from=${from},to=${to},options=${options})`);
    return changes.length ? {from: changes[0].from, to: CodeMirror.changeEnd(changes[changes.length - 1])} : null;
  }

  CodeMirror.defineExtension("wrapParagraph", function(pos, options) {
	logger.debug(`>>> Entering defineExtension(options=${options})`);
    options = options || {};
    if (!pos) pos = this.getCursor();
    var para = findParagraph(this, pos, options);
	logger.debug(`<<< Exiting defineExtension(options=${options})`);
    return wrapRange(this, Pos(para.from, 0), Pos(para.to - 1), options);
  });

  CodeMirror.commands.wrapLines = function(cm) {
    cm.operation(function() {
      var ranges = cm.listSelections(), at = cm.lastLine() + 1;
      for (var i = ranges.length - 1; i >= 0; i--) {
		logger.debug(`operation(ranges=${ranges}): ranges → ${ranges}`);
        var range = ranges[i], span;
        if (range.empty()) {
          var para = findParagraph(cm, range.head, {});
		logger.debug(`operation(ranges=${ranges}): para → ${para}`);
          span = {from: Pos(para.from, 0), to: Pos(para.to - 1)};
        } else {
          span = {from: range.from(), to: range.to()};
		logger.debug(`operation(ranges=${ranges}): span → ${span}`);
        }
        if (span.to.line >= at) continue;
        at = span.from.line;
        wrapRange(cm, span.from, span.to, {});
      }
    });
  };

  CodeMirror.defineExtension("wrapRange", function(from, to, options) {
    return wrapRange(this, from, to, options || {});
  });

  CodeMirror.defineExtension("wrapParagraphsInRange", function(from, to, options) {
    options = options || {};
    var cm = this, paras = [];
    for (var line = from.line; line <= to.line;) {
      var para = findParagraph(cm, Pos(line, 0), options);
	logger.debug(`defineExtension(options=${options}): para → ${para}`);
      paras.push(para);
      line = para.to;
    }
    var madeChange = false;
    if (paras.length) cm.operation(function() {
      for (var i = paras.length - 1; i >= 0; --i)
        madeChange = madeChange || wrapRange(cm, Pos(paras[i].from, 0), Pos(paras[i].to - 1), options);
    });
    return madeChange;
  });
});
