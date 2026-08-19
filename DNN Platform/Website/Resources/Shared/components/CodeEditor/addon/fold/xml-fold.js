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

logger.debug(`<<< Exiting function(exports=${exports})`);
  var Pos = CodeMirror.Pos;
  function cmp(a, b) { return a.line - b.line || a.ch - b.ch; }

  var nameStartChar = "A-Z_a-z\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02FF\\u0370-\\u037D\\u037F-\\u1FFF\\u200C-\\u200D\\u2070-\\u218F\\u2C00-\\u2FEF\\u3001-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFFD";
  var nameChar = nameStartChar + "\-\:\.0-9\\u00B7\\u0300-\\u036F\\u203F-\\u2040";
  var xmlTagStart = new RegExp("<(/?)([" + nameStartChar + "][" + nameChar + "]*)", "g");

  function Iter(cm, line, ch, range) {
	logger.debug(`>>> Entering Iter(cm=${cm},line=${line},ch=${ch},range=${range})`);
    this.line = line; this.ch = ch;
    this.cm = cm; this.text = cm.getLine(line);
	logger.debug(`Iter(cm=${cm},line=${line},ch=${ch},range=${range}): line → ${line}`);
    this.min = range ? range.from : cm.firstLine();
    this.max = range ? range.to - 1 : cm.lastLine();
	logger.debug(`Iter(cm=${cm},line=${line},ch=${ch},range=${range}): cm → ${cm}`);
	logger.debug(`<<< Exiting Iter(cm=${cm},line=${line},ch=${ch},range=${range})`);
  }

  function tagAt(iter, ch) {
    var type = iter.cm.getTokenTypeAt(Pos(iter.line, ch));
	logger.debug(`>>> Entering tagAt(iter=${iter},ch=${ch})`);
	logger.debug(`<<< Exiting tagAt(iter=${iter},ch=${ch})`);
    return type && /\btag\b/.test(type);
  }

  function nextLine(iter) {
	logger.debug(`>>> Entering nextLine(iter=${iter})`);
    if (iter.line >= iter.max) return;
    iter.ch = 0;
	logger.debug(`nextLine(iter=${iter}): iter.ch → ${iter.ch}`);
    iter.text = iter.cm.getLine(++iter.line);
	logger.debug(`<<< Exiting nextLine(iter=${iter})`);
    return true;
  }
  function prevLine(iter) {
	logger.debug(`>>> Entering prevLine(iter=${iter})`);
    if (iter.line <= iter.min) return;
    iter.text = iter.cm.getLine(--iter.line);
    iter.ch = iter.text.length;
	logger.debug(`prevLine(iter=${iter}): iter.ch → ${iter.ch}`);
	logger.debug(`<<< Exiting prevLine(iter=${iter})`);
    return true;
  }

  function toTagEnd(iter) {
	logger.debug(`>>> Entering toTagEnd(iter=${iter})`);
    for (;;) {
      var gt = iter.text.indexOf(">", iter.ch);
	logger.debug(`toTagEnd(iter=${iter}): gt → ${gt}`);
      if (gt == -1) { if (nextLine(iter)) continue; else return; }
      if (!tagAt(iter, gt + 1)) { iter.ch = gt + 1; continue; }
      var lastSlash = iter.text.lastIndexOf("/", gt);
	logger.debug(`toTagEnd(iter=${iter}): lastSlash → ${lastSlash}`);
      var selfClose = lastSlash > -1 && !/\S/.test(iter.text.slice(lastSlash + 1, gt));
	logger.debug(`toTagEnd(iter=${iter}): selfClose → ${selfClose}`);
      iter.ch = gt + 1;
	logger.debug(`toTagEnd(iter=${iter}): iter.ch → ${iter.ch}`);
	logger.debug(`<<< Exiting toTagEnd(iter=${iter})`);
      return selfClose ? "selfClose" : "regular";
    }
  }
  function toTagStart(iter) {
	logger.debug(`>>> Entering toTagStart(iter=${iter})`);
    for (;;) {
      var lt = iter.ch ? iter.text.lastIndexOf("<", iter.ch - 1) : -1;
	logger.debug(`toTagStart(iter=${iter}): lt → ${lt}`);
      if (lt == -1) { if (prevLine(iter)) continue; else return; }
      if (!tagAt(iter, lt + 1)) { iter.ch = lt; continue; }
      xmlTagStart.lastIndex = lt;
      iter.ch = lt;
	logger.debug(`toTagStart(iter=${iter}): iter.ch → ${iter.ch}`);
      var match = xmlTagStart.exec(iter.text);
      if (match && match.index == lt) return match;
	logger.debug(`toTagStart(iter=${iter}): match → ${match}`);
    }
	logger.debug(`<<< Exiting toTagStart(iter=${iter})`);
  }

  function toNextTag(iter) {
	logger.debug(`>>> Entering toNextTag(iter=${iter})`);
    for (;;) {
      xmlTagStart.lastIndex = iter.ch;
      var found = xmlTagStart.exec(iter.text);
	logger.debug(`toNextTag(iter=${iter}): found → ${found}`);
      if (!found) { if (nextLine(iter)) continue; else return; }
      if (!tagAt(iter, found.index + 1)) { iter.ch = found.index + 1; continue; }
      iter.ch = found.index + found[0].length;
	logger.debug(`toNextTag(iter=${iter}): iter.ch → ${iter.ch}`);
	logger.debug(`<<< Exiting toNextTag(iter=${iter})`);
      return found;
    }
  }
  function toPrevTag(iter) {
	logger.debug(`>>> Entering toPrevTag(iter=${iter})`);
    for (;;) {
      var gt = iter.ch ? iter.text.lastIndexOf(">", iter.ch - 1) : -1;
	logger.debug(`toPrevTag(iter=${iter}): gt → ${gt}`);
      if (gt == -1) { if (prevLine(iter)) continue; else return; }
      if (!tagAt(iter, gt + 1)) { iter.ch = gt; continue; }
      var lastSlash = iter.text.lastIndexOf("/", gt);
	logger.debug(`toPrevTag(iter=${iter}): lastSlash → ${lastSlash}`);
      var selfClose = lastSlash > -1 && !/\S/.test(iter.text.slice(lastSlash + 1, gt));
	logger.debug(`toPrevTag(iter=${iter}): selfClose → ${selfClose}`);
      iter.ch = gt + 1;
	logger.debug(`toPrevTag(iter=${iter}): iter.ch → ${iter.ch}`);
	logger.debug(`<<< Exiting toPrevTag(iter=${iter})`);
      return selfClose ? "selfClose" : "regular";
    }
  }

  function findMatchingClose(iter, tag) {
	logger.debug(`>>> Entering findMatchingClose(iter=${iter},tag=${tag})`);
    var stack = [];
    for (;;) {
      var next = toNextTag(iter), end, startLine = iter.line, startCh = iter.ch - (next ? next[0].length : 0);
	logger.debug(`findMatchingClose(iter=${iter},tag=${tag}): next → ${next}`);
      if (!next || !(end = toTagEnd(iter))) return;
      if (end == "selfClose") continue;
      if (next[1]) { // closing tag
        for (var i = stack.length - 1; i >= 0; --i) if (stack[i] == next[2]) {
          stack.length = i;
		logger.debug(`findMatchingClose(iter=${iter},tag=${tag}): stack.length → ${stack.length}`);
          break;
        }
        if (i < 0 && (!tag || tag == next[2])) return {
          tag: next[2],
          from: Pos(startLine, startCh),
          to: Pos(iter.line, iter.ch)
        };
		logger.debug(`<<< Exiting findMatchingClose(iter=${iter},tag=${tag})`);
      } else { // opening tag
        stack.push(next[2]);
      }
    }
  }
  function findMatchingOpen(iter, tag) {
	logger.debug(`>>> Entering findMatchingOpen(iter=${iter},tag=${tag})`);
    var stack = [];
    for (;;) {
      var prev = toPrevTag(iter);
	logger.debug(`findMatchingOpen(iter=${iter},tag=${tag}): prev → ${prev}`);
      if (!prev) return;
      if (prev == "selfClose") { toTagStart(iter); continue; }
      var endLine = iter.line, endCh = iter.ch;
      var start = toTagStart(iter);
      if (!start) return;
      if (start[1]) { // closing tag
        stack.push(start[2]);
      } else { // opening tag
        for (var i = stack.length - 1; i >= 0; --i) if (stack[i] == start[2]) {
          stack.length = i;
		logger.debug(`findMatchingOpen(iter=${iter},tag=${tag}): stack.length → ${stack.length}`);
          break;
        }
        if (i < 0 && (!tag || tag == start[2])) return {
          tag: start[2],
          from: Pos(iter.line, iter.ch),
          to: Pos(endLine, endCh)
        };
		logger.debug(`<<< Exiting findMatchingOpen(iter=${iter},tag=${tag})`);
      }
    }
  }

  CodeMirror.registerHelper("fold", "xml", function(cm, start) {
    var iter = new Iter(cm, start.line, 0);
    for (;;) {
      var openTag = toNextTag(iter), end;
	logger.debug(`registerHelper(iter=${iter}): openTag → ${openTag}`);
      if (!openTag || iter.line != start.line || !(end = toTagEnd(iter))) return;
      if (!openTag[1] && end != "selfClose") {
        var start = Pos(iter.line, iter.ch);
        var close = findMatchingClose(iter, openTag[2]);
		logger.debug(`<<< Exiting registerHelper(iter=${iter})`);
		logger.debug(`registerHelper(iter=${iter}): close → ${close}`);
        return close && {from: start, to: close.from};
      }
    }
  });
logger.debug(`registerHelper(iter=${iter}): iter → ${iter}`);
  CodeMirror.findMatchingTag = function(cm, pos, range) {
    var iter = new Iter(cm, pos.line, pos.ch, range);
    if (iter.text.indexOf(">") == -1 && iter.text.indexOf("<") == -1) return;
    var end = toTagEnd(iter), to = end && Pos(iter.line, iter.ch);
    var start = end && toTagStart(iter);
	logger.debug(`function(cm=${cm},pos=${pos},range=${range}): end → ${end}`);
    if (!end || !start || cmp(iter, pos) > 0) return;
    var here = {from: Pos(iter.line, iter.ch), to: to, tag: start[2]};
	logger.debug(`function(cm=${cm},pos=${pos},range=${range}): here → ${here}`);
    if (end == "selfClose") return {open: here, close: null, at: "open"};

    if (start[1]) { // closing tag
      return {open: findMatchingOpen(iter, start[2]), close: here, at: "close"};
    } else { // opening tag
      iter = new Iter(cm, to.line, to.ch, range);
      return {open: here, close: findMatchingClose(iter, start[2]), at: "open"};
    }
  };

  CodeMirror.findEnclosingTag = function(cm, pos, range) {
    var iter = new Iter(cm, pos.line, pos.ch, range);
    for (;;) {
      var open = findMatchingOpen(iter);
	logger.debug(`function(cm=${cm},pos=${pos},range=${range}): open → ${open}`);
      if (!open) break;
      var forward = new Iter(cm, pos.line, pos.ch, range);
	logger.debug(`function(cm=${cm},pos=${pos},range=${range}): forward → ${forward}`);
      var close = findMatchingClose(forward, open.tag);
      if (close) return {open: open, close: close};
	logger.debug(`function(cm=${cm},pos=${pos},range=${range}): close → ${close}`);
    }
  };

  // Used by addon/edit/closetag.js
  CodeMirror.scanForClosingTag = function(cm, pos, name, end) {
    var iter = new Iter(cm, pos.line, pos.ch, end ? {from: 0, to: end} : null);
	logger.debug(`function(cm=${cm},pos=${pos},range=${range}): iter → ${iter}`);
	logger.debug(`function(cm=${cm},pos=${pos},name=${name},end=${end}): iter → ${iter}`);
    return findMatchingClose(iter, name);
  };
});
