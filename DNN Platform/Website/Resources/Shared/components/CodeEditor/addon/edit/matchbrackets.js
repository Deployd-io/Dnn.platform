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
  var ie_lt8 = /MSIE \d/.test(navigator.userAgent) &&
    (document.documentMode == null || document.documentMode < 8);

  var Pos = CodeMirror.Pos;

  var matching = {"(": ")>", ")": "(<", "[": "]>", "]": "[<", "{": "}>", "}": "{<"};

  function findMatchingBracket(cm, where, strict, config) {
	logger.error(`>>> Entering findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config})`, { "event.action": "findMatchingBracket", "logrite.cm": cm, "logrite.where": where, "logrite.strict": strict, "logrite.config": config });
    var line = cm.getLineHandle(where.line), pos = where.ch - 1;
    var match = (pos >= 0 && matching[line.text.charAt(pos)]) || matching[line.text.charAt(++pos)];
	logger.error(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): line → ${line}`, { "event.action": "findMatchingBracket", "logrite.cm": cm, "logrite.where": where, "logrite.strict": strict, "logrite.config": config });
	logger.error(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): match → ${match}`, { "event.action": "findMatchingBracket", "logrite.cm": cm, "logrite.where": where, "logrite.strict": strict, "logrite.config": config });
    if (!match) return null;
    var dir = match.charAt(1) == ">" ? 1 : -1;
	logger.error(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): dir → ${dir}`, { "event.action": "findMatchingBracket", "logrite.cm": cm, "logrite.where": where, "logrite.strict": strict, "logrite.config": config });
    if (strict && (dir > 0) != (pos == where.ch)) return null;
    var style = cm.getTokenTypeAt(Pos(where.line, pos + 1));
	logger.error(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): style → ${style}`, { "event.action": "findMatchingBracket", "logrite.cm": cm, "logrite.where": where, "logrite.strict": strict, "logrite.config": config });

    var found = scanForBracket(cm, Pos(where.line, pos + (dir > 0 ? 1 : 0)), dir, style || null, config);
	logger.error(`findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config}): found → ${found}`, { "event.action": "findMatchingBracket", "logrite.cm": cm, "logrite.where": where, "logrite.strict": strict, "logrite.config": config });
    if (found == null) return null;
	logger.warn(`<<< Exiting findMatchingBracket(cm=${cm},where=${where},strict=${strict},config=${config})`, { "event.action": "findMatchingBracket", "logrite.cm": cm, "logrite.where": where, "logrite.strict": strict, "logrite.config": config });
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
	logger.error(`>>> Entering scanForBracket(cm=${cm},where=${where},dir=${dir},style=${style},config=${config})`, { "event.action": "scanForBracket", "logrite.cm": cm, "logrite.where": where, "logrite.dir": dir, "logrite.style": style, "logrite.config": config });
    var maxScanLen = (config && config.maxScanLineLength) || 10000;
    var maxScanLines = (config && config.maxScanLines) || 1000;
	logger.error(`scanForBracket(cm=${cm},where=${where},dir=${dir},style=${style},config=${config}): maxScanLen → ${maxScanLen}`, { "event.action": "scanForBracket", "logrite.cm": cm, "logrite.where": where, "logrite.dir": dir, "logrite.style": style, "logrite.config": config });
	logger.error(`scanForBracket(cm=${cm},where=${where},dir=${dir},style=${style},config=${config}): maxScanLines → ${maxScanLines}`, { "event.action": "scanForBracket", "logrite.cm": cm, "logrite.where": where, "logrite.dir": dir, "logrite.style": style, "logrite.config": config });

    var stack = [];
    var re = config && config.bracketRegex ? config.bracketRegex : /[(){}[\]]/;
	logger.error(`scanForBracket(cm=${cm},where=${where},dir=${dir},style=${style},config=${config}): re → ${re}`, { "event.action": "scanForBracket", "logrite.cm": cm, "logrite.where": where, "logrite.dir": dir, "logrite.style": style, "logrite.config": config });
    var lineEnd = dir > 0 ? Math.min(where.line + maxScanLines, cm.lastLine() + 1)
                          : Math.max(cm.firstLine() - 1, where.line - maxScanLines);
    for (var lineNo = where.line; lineNo != lineEnd; lineNo += dir) {
	logger.error(`scanForBracket(cm=${cm},where=${where},dir=${dir},style=${style},config=${config}): lineEnd → ${lineEnd}`, { "event.action": "scanForBracket", "logrite.cm": cm, "logrite.where": where, "logrite.dir": dir, "logrite.style": style, "logrite.config": config });
      var line = cm.getLine(lineNo);
	logger.error(`scanForBracket(cm=${cm},where=${where},dir=${dir},style=${style},config=${config}): line → ${line}`, { "event.action": "scanForBracket", "logrite.cm": cm, "logrite.where": where, "logrite.dir": dir, "logrite.style": style, "logrite.config": config });
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
	logger.error(`scanForBracket(cm=${cm},where=${where},dir=${dir},style=${style},config=${config}): ch → ${ch}`, { "event.action": "scanForBracket", "logrite.cm": cm, "logrite.where": where, "logrite.dir": dir, "logrite.style": style, "logrite.config": config });
    return lineNo - dir == (dir > 0 ? cm.lastLine() : cm.firstLine()) ? false : null;
  }

  function matchBrackets(cm, autoclear, config) {
	logger.error(`>>> Entering matchBrackets(cm=${cm},autoclear=${autoclear},config=${config})`, { "event.action": "matchBrackets", "logrite.cm": cm, "logrite.autoclear": autoclear, "logrite.config": config });
    // Disable brace matching in long lines, since it'll cause hugely slow updates
    var maxHighlightLen = cm.state.matchBrackets.maxHighlightLineLength || 1000;
    var marks = [], ranges = cm.listSelections();
	logger.error(`matchBrackets(cm=${cm},autoclear=${autoclear},config=${config}): marks → ${marks}`, { "event.action": "matchBrackets", "logrite.cm": cm, "logrite.autoclear": autoclear, "logrite.config": config });
    for (var i = 0; i < ranges.length; i++) {
      var match = ranges[i].empty() && findMatchingBracket(cm, ranges[i].head, false, config);
	logger.error(`matchBrackets(cm=${cm},autoclear=${autoclear},config=${config}): match → ${match}`, { "event.action": "matchBrackets", "logrite.cm": cm, "logrite.autoclear": autoclear, "logrite.config": config });
      if (match && cm.getLine(match.from.line).length <= maxHighlightLen) {
        var style = match.match ? "CodeMirror-matchingbracket" : "CodeMirror-nonmatchingbracket";
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
		logger.error('>>> Entering clear()', { "event.action": "clear" });
        cm.operation(function() {
          for (var i = 0; i < marks.length; i++) marks[i].clear();
        });
		logger.warn('<<< Exiting clear()', { "event.action": "clear" });
      };
      if (autoclear) setTimeout(clear, 800);
      else return clear;
    }
	logger.warn(`<<< Exiting matchBrackets(cm=${cm},autoclear=${autoclear},config=${config})`, { "event.action": "matchBrackets", "logrite.cm": cm, "logrite.autoclear": autoclear, "logrite.config": config });
  }

  var currentlyHighlighted = null;
  function doMatchBrackets(cm) {
	logger.error(`>>> Entering doMatchBrackets(cm=${cm})`, { "event.action": "doMatchBrackets", "logrite.cm": cm });
    cm.operation(function() {
      if (currentlyHighlighted) {currentlyHighlighted(); currentlyHighlighted = null;}
      currentlyHighlighted = matchBrackets(cm, false, cm.state.matchBrackets);
    });
	logger.warn(`<<< Exiting doMatchBrackets(cm=${cm})`, { "event.action": "doMatchBrackets", "logrite.cm": cm });
  }

  CodeMirror.defineOption("matchBrackets", false, function(cm, val, old) {
    if (old && old != CodeMirror.Init)
      cm.off("cursorActivity", doMatchBrackets);
    if (val) {
      cm.state.matchBrackets = typeof val == "object" ? val : {};
      cm.on("cursorActivity", doMatchBrackets);
    }
  });

  CodeMirror.defineExtension("matchBrackets", function() {matchBrackets(this, true);});
  CodeMirror.defineExtension("findMatchingBracket", function(pos, strict, config){
    return findMatchingBracket(this, pos, strict, config);
  });
  CodeMirror.defineExtension("scanForBracket", function(pos, dir, style, config){
    return scanForBracket(this, pos, dir, style, config);
  });
});
