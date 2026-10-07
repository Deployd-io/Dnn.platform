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

  var Pos = CodeMirror.Pos;
  function cmp(a, b) { return a.line - b.line || a.ch - b.ch; }

  var nameStartChar = "A-Z_a-z\\u00C0-\\u00D6\\u00D8-\\u00F6\\u00F8-\\u02FF\\u0370-\\u037D\\u037F-\\u1FFF\\u200C-\\u200D\\u2070-\\u218F\\u2C00-\\u2FEF\\u3001-\\uD7FF\\uF900-\\uFDCF\\uFDF0-\\uFFFD";
  var nameChar = nameStartChar + "\-\:\.0-9\\u00B7\\u0300-\\u036F\\u203F-\\u2040";
  var xmlTagStart = new RegExp("<(/?)([" + nameStartChar + "][" + nameChar + "]*)", "g");

  function Iter(cm, line, ch, range) {
	logger.error(`>>> Entering Iter(cm=${cm},line=${line},ch=${ch},range=${range})`, { "event.action": "Iter", "logrite.cm": cm, "logrite.line": line, "logrite.ch": ch, "logrite.range": range });
    this.line = line; this.ch = ch;
    this.cm = cm; this.text = cm.getLine(line);
	logger.debug(`Iter(cm=${cm},line=${line},ch=${ch},range=${range}): line → ${line}`, { "event.action": "Iter", "logrite.cm": cm, "logrite.line": line, "logrite.ch": ch, "logrite.range": range });
	logger.debug(`Iter(cm=${cm},line=${line},ch=${ch},range=${range}): this.line → ${this.line}`, { "event.action": "Iter", "logrite.cm": cm, "logrite.line": line, "logrite.ch": ch, "logrite.range": range });
	logger.debug(`Iter(cm=${cm},line=${line},ch=${ch},range=${range}): cm → ${cm}`, { "event.action": "Iter", "logrite.cm": cm, "logrite.line": line, "logrite.ch": ch, "logrite.range": range });
    this.min = range ? range.from : cm.firstLine();
    this.max = range ? range.to - 1 : cm.lastLine();
  }

  function tagAt(iter, ch) {
	logger.error(`>>> Entering tagAt(iter=${iter},ch=${ch})`, { "event.action": "tagAt", "logrite.iter": iter, "logrite.ch": ch });
    var type = iter.cm.getTokenTypeAt(Pos(iter.line, ch));
	logger.error(`tagAt(iter=${iter},ch=${ch}): type → ${type}`, { "event.action": "tagAt", "logrite.iter": iter, "logrite.ch": ch });
    return type && /\btag\b/.test(type);
  }

  function nextLine(iter) {
	logger.error(`>>> Entering nextLine(iter=${iter})`, { "event.action": "nextLine", "logrite.iter": iter });
    if (iter.line >= iter.max) return;
    iter.ch = 0;
	logger.debug(`nextLine(iter=${iter}): iter.ch → ${iter.ch}`, { "event.action": "nextLine", "logrite.iter": iter });
    iter.text = iter.cm.getLine(++iter.line);
	logger.warn(`<<< Exiting nextLine(iter=${iter})`, { "event.action": "nextLine", "logrite.iter": iter });
    return true;
  }
  function prevLine(iter) {
	logger.error(`>>> Entering prevLine(iter=${iter})`, { "event.action": "prevLine", "logrite.iter": iter });
    if (iter.line <= iter.min) return;
    iter.text = iter.cm.getLine(--iter.line);
    iter.ch = iter.text.length;
	logger.debug(`prevLine(iter=${iter}): iter.ch → ${iter.ch}`, { "event.action": "prevLine", "logrite.iter": iter });
	logger.warn(`<<< Exiting prevLine(iter=${iter})`, { "event.action": "prevLine", "logrite.iter": iter });
    return true;
  }

  function toTagEnd(iter) {
	logger.error(`>>> Entering toTagEnd(iter=${iter})`, { "event.action": "toTagEnd", "logrite.iter": iter });
    for (;;) {
      var gt = iter.text.indexOf(">", iter.ch);
	logger.error(`toTagEnd(iter=${iter}): gt → ${gt}`, { "event.action": "toTagEnd", "logrite.iter": iter });
      if (gt == -1) { if (nextLine(iter)) continue; else return; }
      if (!tagAt(iter, gt + 1)) { iter.ch = gt + 1; continue; }
      var lastSlash = iter.text.lastIndexOf("/", gt);
	logger.error(`toTagEnd(iter=${iter}): lastSlash → ${lastSlash}`, { "event.action": "toTagEnd", "logrite.iter": iter });
      var selfClose = lastSlash > -1 && !/\S/.test(iter.text.slice(lastSlash + 1, gt));
	logger.error(`toTagEnd(iter=${iter}): selfClose → ${selfClose}`, { "event.action": "toTagEnd", "logrite.iter": iter });
      iter.ch = gt + 1;
	logger.warn(`<<< Exiting toTagEnd(iter=${iter})`, { "event.action": "toTagEnd", "logrite.iter": iter });
      return selfClose ? "selfClose" : "regular";
    }
  }
  function toTagStart(iter) {
	logger.error(`>>> Entering toTagStart(iter=${iter})`, { "event.action": "toTagStart", "logrite.iter": iter });
    for (;;) {
      var lt = iter.ch ? iter.text.lastIndexOf("<", iter.ch - 1) : -1;
	logger.error(`toTagStart(iter=${iter}): lt → ${lt}`, { "event.action": "toTagStart", "logrite.iter": iter });
      if (lt == -1) { if (prevLine(iter)) continue; else return; }
      if (!tagAt(iter, lt + 1)) { iter.ch = lt; continue; }
      xmlTagStart.lastIndex = lt;
      iter.ch = lt;
	logger.debug(`toTagStart(iter=${iter}): iter.ch → ${iter.ch}`, { "event.action": "toTagStart", "logrite.iter": iter });
      var match = xmlTagStart.exec(iter.text);
      if (match && match.index == lt) return match;
	logger.error(`toTagStart(iter=${iter}): match → ${match}`, { "event.action": "toTagStart", "logrite.iter": iter });
    }
  }

  function toNextTag(iter) {
	logger.error(`>>> Entering toNextTag(iter=${iter})`, { "event.action": "toNextTag", "logrite.iter": iter });
    for (;;) {
      xmlTagStart.lastIndex = iter.ch;
      var found = xmlTagStart.exec(iter.text);
	logger.error(`toNextTag(iter=${iter}): found → ${found}`, { "event.action": "toNextTag", "logrite.iter": iter });
      if (!found) { if (nextLine(iter)) continue; else return; }
      if (!tagAt(iter, found.index + 1)) { iter.ch = found.index + 1; continue; }
      iter.ch = found.index + found[0].length;
	logger.debug(`toNextTag(iter=${iter}): iter.ch → ${iter.ch}`, { "event.action": "toNextTag", "logrite.iter": iter });
	logger.warn(`<<< Exiting toNextTag(iter=${iter})`, { "event.action": "toNextTag", "logrite.iter": iter });
      return found;
    }
  }
  function toPrevTag(iter) {
	logger.error(`>>> Entering toPrevTag(iter=${iter})`, { "event.action": "toPrevTag", "logrite.iter": iter });
    for (;;) {
      var gt = iter.ch ? iter.text.lastIndexOf(">", iter.ch - 1) : -1;
	logger.error(`toPrevTag(iter=${iter}): gt → ${gt}`, { "event.action": "toPrevTag", "logrite.iter": iter });
      if (gt == -1) { if (prevLine(iter)) continue; else return; }
      if (!tagAt(iter, gt + 1)) { iter.ch = gt; continue; }
      var lastSlash = iter.text.lastIndexOf("/", gt);
	logger.error(`toPrevTag(iter=${iter}): lastSlash → ${lastSlash}`, { "event.action": "toPrevTag", "logrite.iter": iter });
      var selfClose = lastSlash > -1 && !/\S/.test(iter.text.slice(lastSlash + 1, gt));
	logger.error(`toPrevTag(iter=${iter}): selfClose → ${selfClose}`, { "event.action": "toPrevTag", "logrite.iter": iter });
      iter.ch = gt + 1;
	logger.warn(`<<< Exiting toPrevTag(iter=${iter})`, { "event.action": "toPrevTag", "logrite.iter": iter });
      return selfClose ? "selfClose" : "regular";
    }
  }

  function findMatchingClose(iter, tag) {
	logger.error(`>>> Entering findMatchingClose(iter=${iter},tag=${tag})`, { "event.action": "findMatchingClose", "logrite.iter": iter, "logrite.tag": tag });
    var stack = [];
    for (;;) {
	logger.debug(`findMatchingClose(iter=${iter},tag=${tag}): stack.length → ${stack.length}`, { "event.action": "findMatchingClose", "logrite.iter": iter, "logrite.tag": tag });
      var next = toNextTag(iter), end, startLine = iter.line, startCh = iter.ch - (next ? next[0].length : 0);
	logger.error(`findMatchingClose(iter=${iter},tag=${tag}): next → ${next}`, { "event.action": "findMatchingClose", "logrite.iter": iter, "logrite.tag": tag });
      if (!next || !(end = toTagEnd(iter))) return;
      if (end == "selfClose") continue;
      if (next[1]) { // closing tag
        for (var i = stack.length - 1; i >= 0; --i) if (stack[i] == next[2]) {
          stack.length = i;
          break;
        }
        if (i < 0 && (!tag || tag == next[2])) return {
          tag: next[2],
          from: Pos(startLine, startCh),
          to: Pos(iter.line, iter.ch)
        };
      } else { // opening tag
        stack.push(next[2]);
      }
    }
  }
  function findMatchingOpen(iter, tag) {
	logger.error(`>>> Entering findMatchingOpen(iter=${iter},tag=${tag})`, { "event.action": "findMatchingOpen", "logrite.iter": iter, "logrite.tag": tag });
    var stack = [];
    for (;;) {
	logger.debug(`findMatchingOpen(iter=${iter},tag=${tag}): stack.length → ${stack.length}`, { "event.action": "findMatchingOpen", "logrite.iter": iter, "logrite.tag": tag });
      var prev = toPrevTag(iter);
	logger.error(`findMatchingOpen(iter=${iter},tag=${tag}): prev → ${prev}`, { "event.action": "findMatchingOpen", "logrite.iter": iter, "logrite.tag": tag });
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
          break;
        }
        if (i < 0 && (!tag || tag == start[2])) return {
          tag: start[2],
          from: Pos(iter.line, iter.ch),
          to: Pos(endLine, endCh)
        };
		logger.warn(`<<< Exiting findMatchingOpen(iter=${iter},tag=${tag})`, { "event.action": "findMatchingOpen", "logrite.iter": iter, "logrite.tag": tag });
      }
    }
  }

  CodeMirror.registerHelper("fold", "xml", function(cm, start) {
    var iter = new Iter(cm, start.line, 0);
    for (;;) {
      var openTag = toNextTag(iter), end;
      if (!openTag || iter.line != start.line || !(end = toTagEnd(iter))) return;
      if (!openTag[1] && end != "selfClose") {
        var start = Pos(iter.line, iter.ch);
        var close = findMatchingClose(iter, openTag[2]);
        return close && {from: start, to: close.from};
      }
    }
  });
  CodeMirror.findMatchingTag = function(cm, pos, range) {
	logger.error(`>>> Entering findMatchingTag(cm=${cm},pos=${pos},range=${range})`, { "event.action": "findMatchingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.range": range });
    var iter = new Iter(cm, pos.line, pos.ch, range);
    if (iter.text.indexOf(">") == -1 && iter.text.indexOf("<") == -1) return;
    var end = toTagEnd(iter), to = end && Pos(iter.line, iter.ch);
	logger.error(`findMatchingTag(cm=${cm},pos=${pos},range=${range}): end → ${end}`, { "event.action": "findMatchingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.range": range });
    var start = end && toTagStart(iter);
    if (!end || !start || cmp(iter, pos) > 0) return;
    var here = {from: Pos(iter.line, iter.ch), to: to, tag: start[2]};
	logger.error(`findMatchingTag(cm=${cm},pos=${pos},range=${range}): here → ${here}`, { "event.action": "findMatchingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.range": range });
    if (end == "selfClose") return {open: here, close: null, at: "open"};

    if (start[1]) { // closing tag
      return {open: findMatchingOpen(iter, start[2]), close: here, at: "close"};
    } else { // opening tag
      iter = new Iter(cm, to.line, to.ch, range);
	logger.error(`findMatchingTag(cm=${cm},pos=${pos},range=${range}): iter → ${iter}`, { "event.action": "findMatchingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.range": range });
      return {open: here, close: findMatchingClose(iter, start[2]), at: "open"};
    }
  };

  CodeMirror.findEnclosingTag = function(cm, pos, range) {
	logger.error(`>>> Entering findEnclosingTag(cm=${cm},pos=${pos},range=${range})`, { "event.action": "findEnclosingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.range": range });
    var iter = new Iter(cm, pos.line, pos.ch, range);
    for (;;) {
	logger.error(`findEnclosingTag(cm=${cm},pos=${pos},range=${range}): iter → ${iter}`, { "event.action": "findEnclosingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.range": range });
      var open = findMatchingOpen(iter);
	logger.error(`findEnclosingTag(cm=${cm},pos=${pos},range=${range}): open → ${open}`, { "event.action": "findEnclosingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.range": range });
      if (!open) break;
      var forward = new Iter(cm, pos.line, pos.ch, range);
	logger.error(`findEnclosingTag(cm=${cm},pos=${pos},range=${range}): forward → ${forward}`, { "event.action": "findEnclosingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.range": range });
      var close = findMatchingClose(forward, open.tag);
      if (close) return {open: open, close: close};
	logger.error(`findEnclosingTag(cm=${cm},pos=${pos},range=${range}): close → ${close}`, { "event.action": "findEnclosingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.range": range });
    }
  };

  // Used by addon/edit/closetag.js
  CodeMirror.scanForClosingTag = function(cm, pos, name, end) {
	logger.error(`>>> Entering scanForClosingTag(cm=${cm},pos=${pos},name=${name},end=${end})`, { "event.action": "scanForClosingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.name": name, "logrite.end": end });
    var iter = new Iter(cm, pos.line, pos.ch, end ? {from: 0, to: end} : null);
	logger.error(`scanForClosingTag(cm=${cm},pos=${pos},name=${name},end=${end}): iter → ${iter}`, { "event.action": "scanForClosingTag", "logrite.cm": cm, "logrite.pos": pos, "logrite.name": name, "logrite.end": end });
    return findMatchingClose(iter, name);
  };
});
