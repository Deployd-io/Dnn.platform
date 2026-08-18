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
  var ie_lt8 = /MSIE \d/.test(navigator.userAgent) &&
    (document.documentMode == null || document.documentMode < 8);

logger.debug(`function(exports=${exports}): ie_lt8 → ${ie_lt8}`);
  var Pos = CodeMirror.Pos;

  var matching = {"(": ")>", ")": "(<", "[": "]>", "]": "[<", "{": "}>", "}": "{<"};
logger.debug(`<<< Exiting function(exports=${exports})`);
logger.debug(`function(exports=${exports}): matching → ${matching}`);

  function findMatchingBracket(cm, where, strict, config) {
	logger.debug(`>>> Entering findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config})`);
    var line = cm.getLineHandle(where.line), pos = where.ch - 1;
    var match = (pos >= 0 && matching[line.text.charAt(pos)]) || matching[line.text.charAt(++pos)];
	logger.debug(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): line → ${line}`);
	logger.debug(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): match → ${match}`);
    if (!match) return null;
    var dir = match.charAt(1) == ">" ? 1 : -1;
	logger.debug(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): dir → ${dir}`);
    if (strict && (dir > 0) != (pos == where.ch)) return null;
    var style = cm.getTokenTypeAt(Pos(where.line, pos + 1));
	logger.debug(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): style → ${style}`);

    var found = scanForBracket(cm, Pos(where.line, pos + (dir > 0 ? 1 : 0)), dir, style || null, config);
	logger.debug(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): found → ${found}`);
    if (found == null) return null;
	logger.debug(`<<< Exiting findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config})`);
    return {from: Pos(where.line, pos), to: found && found.pos,
            match: found && found.ch == match.charAt(0), forward: dir > 0};
  }

  // bracketRegex is used to specify which type of bracket to scan
  // should be a regexp, e.g. /[[\]]/
  //
  // Note: If "where" is on an open bracket, then this bracket is ignored.
  //
  // Returns false when no bracket was found, null when it reached
  // maxScanLines and gave up
  function scanForBracket(cm, where, dir, style, config) {
    var maxScanLen = (config && config.maxScanLineLength) || 10000;
    var maxScanLines = (config && config.maxScanLines) || 1000;

    var stack = [];
    var re = config && config.bracketRegex ? config.bracketRegex : /[(){}[\]]/;
    var lineEnd = dir > 0 ? Math.min(where.line + maxScanLines, cm.lastLine() + 1)
                          : Math.max(cm.firstLine() - 1, where.line - maxScanLines);
    for (var lineNo = where.line; lineNo != lineEnd; lineNo += dir) {
      var line = cm.getLine(lineNo);
      if (!line) continue;
      var pos = dir > 0 ? 0 : line.length - 1, end = dir > 0 ? line.length : -1;
      if (line.length > maxScanLen) continue;
      if (lineNo == where.line) pos = where.ch - (dir < 0 ? 1 : 0);
      for (; pos != end; pos += dir) {
        var ch = line.charAt(pos);
        if (re.test(ch) && (style === undefined || cm.getTokenTypeAt(Pos(lineNo, pos + 1)) == style)) {
          var match = matching[ch];
          if ((match.charAt(1) == ">") == (dir > 0)) stack.push(ch);
          else if (!stack.length) return {pos: Pos(lineNo, pos), ch: ch};
          else stack.pop();
        }
      }
    }
    return lineNo - dir == (dir > 0 ? cm.lastLine() : cm.firstLine()) ? false : null;
  }

  function matchBrackets(cm, autoclear, config) {
	logger.debug(`>>> Entering matchBrackets(cm=${cm},autoclear=${autoclear},config=${config})`);
    // Disable brace matching in long lines, since it'll cause hugely slow updates
    var maxHighlightLen = cm.state.matchBrackets.maxHighlightLineLength || 1000;
    var marks = [], ranges = cm.listSelections();
	logger.debug(`matchBrackets(cm=${cm},autoclear=${autoclear},config=${config}): marks → ${marks}`);
    for (var i = 0; i < ranges.length; i++) {
      var match = ranges[i].empty() && findMatchingBracket(cm, ranges[i].head, false, config);
	logger.debug(`matchBrackets(cm=${cm},autoclear=${autoclear},config=${config}): match → ${match}`);
      if (match && cm.getLine(match.from.line).length <= maxHighlightLen) {
        var style = match.match ? "CodeMirror-matchingbracket" : "CodeMirror-nonmatchingbracket";
		logger.debug(`matchBrackets(cm=${cm},autoclear=${autoclear},config=${config}): style → ${style}`);
        marks.push(cm.markText(match.from, Pos(match.from.line, match.from.ch + 1), {className: style}));
        if (match.to && cm.getLine(match.to.line).length <= maxHighlightLen)
          marks.push(cm.markText(match.to, Pos(match.to.line, match.to.ch + 1), {className: style}));
      }
    }

    if (marks.length) {
      // Kludge to work around the IE bug from issue #1193, where text
      // input stops going to the textare whever this fires.
      if (ie_lt8 && cm.state.focused) cm.focus();

      var clear = function() {
		logger.debug('>>> Entering clear()');
        cm.operation(function() {
		logger.debug(`>>> Entering operation(i=${i})`);
          for (var i = 0; i < marks.length; i++) marks[i].clear();
        });
		logger.debug(`<<< Exiting operation(i=${i})`);
		logger.debug('<<< Exiting clear()');
      };
      if (autoclear) setTimeout(clear, 800);
      else return clear;
	logger.debug(`<<< Exiting matchBrackets(cm=${cm},autoclear=${autoclear},config=${config})`);
    }
  }

  var currentlyHighlighted = null;
  function doMatchBrackets(cm) {
	logger.debug(`>>> Entering doMatchBrackets(cm=${cm})`);
    cm.operation(function() {
      if (currentlyHighlighted) {currentlyHighlighted(); currentlyHighlighted = null;}
      currentlyHighlighted = matchBrackets(cm, false, cm.state.matchBrackets);
    });
	logger.debug(`<<< Exiting doMatchBrackets(cm=${cm})`);
  }

  CodeMirror.defineOption("matchBrackets", false, function(cm, val, old) {
	logger.debug(`>>> Entering defineOption(false=${false})`);
    if (old && old != CodeMirror.Init)
      cm.off("cursorActivity", doMatchBrackets);
    if (val) {
      cm.state.matchBrackets = typeof val == "object" ? val : {};
      cm.on("cursorActivity", doMatchBrackets);
	logger.debug(`<<< Exiting defineOption(false=${false})`);
    }
  });

  CodeMirror.defineExtension("matchBrackets", function() {matchBrackets(this, true);});
  CodeMirror.defineExtension("findMatchingBracket", function(pos, strict, config){
	logger.debug('>>> Entering defineExtension()');
logger.debug('<<< Exiting defineExtension()');
    return findMatchingBracket(this, pos, strict, config);
  });
  CodeMirror.defineExtension("scanForBracket", function(pos, dir, style, config){
    return scanForBracket(this, pos, dir, style, config);
  });
});
