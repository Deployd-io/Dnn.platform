const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// declare global: diff_match_patch, DIFF_INSERT, DIFF_DELETE, DIFF_EQUAL

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("diff_match_patch"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "diff_match_patch"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";
  var Pos = CodeMirror.Pos;
  var svgNS = "http://www.w3.org/2000/svg";

  function DiffView(mv, type) {
	logger.error(`>>> Entering DiffView(mv=${mv},type=${type})`, { "event.action": "DiffView", "logrite.mv": mv, "logrite.type": type });
    this.mv = mv;
    this.type = type;
	logger.debug(`DiffView(mv=${mv},type=${type}): mv → ${mv}`, { "event.action": "DiffView", "logrite.mv": mv, "logrite.type": type });
    this.classes = type == "left"
      ? {chunk: "CodeMirror-merge-l-chunk",
         start: "CodeMirror-merge-l-chunk-start",
         end: "CodeMirror-merge-l-chunk-end",
         insert: "CodeMirror-merge-l-inserted",
         del: "CodeMirror-merge-l-deleted",
         connect: "CodeMirror-merge-l-connect"}
      : {chunk: "CodeMirror-merge-r-chunk",
         start: "CodeMirror-merge-r-chunk-start",
         end: "CodeMirror-merge-r-chunk-end",
         insert: "CodeMirror-merge-r-inserted",
         del: "CodeMirror-merge-r-deleted",
         connect: "CodeMirror-merge-r-connect"};
		logger.debug(`DiffView(mv=${mv},type=${type}): this.type → ${this.type}`, { "event.action": "DiffView", "logrite.mv": mv, "logrite.type": type });
  }

  DiffView.prototype = {
    constructor: DiffView,
    init: function(pane, orig, options) {
      this.edit = this.mv.edit;
      (this.edit.state.diffViews || (this.edit.state.diffViews = [])).push(this);
      this.orig = CodeMirror(pane, copyObj({value: orig, readOnly: !this.mv.options.allowEditingOriginals}, copyObj(options)));
	logger.debug(`init(pane=${pane},orig=${orig},options=${options}): this.edit → ${this.edit}`, { "event.action": "init", "logrite.pane": pane, "logrite.orig": orig, "logrite.options": options });
	logger.debug(`init(pane=${pane},orig=${orig},options=${options}): orig → ${orig}`, { "event.action": "init", "logrite.pane": pane, "logrite.orig": orig, "logrite.options": options });
      this.orig.state.diffViews = [this];

      this.diff = getDiff(asString(orig), asString(options.value));
      this.chunks = getChunks(this.diff);
      this.diffOutOfDate = this.dealigned = false;

	logger.debug(`init(pane=${pane},orig=${orig},options=${options}): this.diffOutOfDate → ${this.diffOutOfDate}`, { "event.action": "init", "logrite.pane": pane, "logrite.orig": orig, "logrite.options": options });
      this.showDifferences = options.showDifferences !== false;
      this.forceUpdate = registerUpdate(this);
      setScrollLock(this, true, false);
      registerScroll(this);
	logger.warn(`<<< Exiting init(pane=${pane},orig=${orig},options=${options})`, { "event.action": "init", "logrite.pane": pane, "logrite.orig": orig, "logrite.options": options });
    },
    setShowDifferences: function(val) {
	logger.error(`>>> Entering setShowDifferences(val=${val})`, { "event.action": "setShowDifferences", "logrite.val": val });
      val = val !== false;
      if (val != this.showDifferences) {
        this.showDifferences = val;
        this.forceUpdate("full");
		logger.warn(`<<< Exiting setShowDifferences(val=${val})`, { "event.action": "setShowDifferences", "logrite.val": val });
      }
    }
  };

  function ensureDiff(dv) {
	logger.error(`>>> Entering ensureDiff(dv=${dv})`, { "event.action": "ensureDiff", "logrite.dv": dv });
    if (dv.diffOutOfDate) {
      dv.diff = getDiff(dv.orig.getValue(), dv.edit.getValue());
      dv.chunks = getChunks(dv.diff);
      dv.diffOutOfDate = false;
	logger.debug(`ensureDiff(dv=${dv}): dv.diffOutOfDate → ${dv.diffOutOfDate}`, { "event.action": "ensureDiff", "logrite.dv": dv });
      CodeMirror.signal(dv.edit, "updateDiff", dv.diff);
	logger.warn(`<<< Exiting ensureDiff(dv=${dv})`, { "event.action": "ensureDiff", "logrite.dv": dv });
    }
  }

  var updating = false;
  function registerUpdate(dv) {
	logger.error(`>>> Entering registerUpdate(dv=${dv})`, { "event.action": "registerUpdate", "logrite.dv": dv });
    var edit = {from: 0, to: 0, marked: []};
    var orig = {from: 0, to: 0, marked: []};
    var debounceChange, updatingFast = false;
    function update(mode) {
	logger.error(`>>> Entering update(mode=${mode})`, { "event.action": "update", "logrite.mode": mode });
      updating = true;
      updatingFast = false;
      if (mode == "full") {
        if (dv.svg) clear(dv.svg);
        if (dv.copyButtons) clear(dv.copyButtons);
        clearMarks(dv.edit, edit.marked, dv.classes);
        clearMarks(dv.orig, orig.marked, dv.classes);
        edit.from = edit.to = orig.from = orig.to = 0;
      }
      ensureDiff(dv);
      if (dv.showDifferences) {
        updateMarks(dv.edit, dv.diff, edit, DIFF_INSERT, dv.classes);
        updateMarks(dv.orig, dv.diff, orig, DIFF_DELETE, dv.classes);
      }
      makeConnections(dv);

      if (dv.mv.options.connect == "align")
        alignChunks(dv);
      updating = false;
	logger.warn(`<<< Exiting update(mode=${mode})`, { "event.action": "update", "logrite.mode": mode });
    }
    function setDealign(fast) {
	logger.error(`>>> Entering setDealign(fast=${fast})`, { "event.action": "setDealign", "logrite.fast": fast });
      if (updating) return;
      dv.dealigned = true;
      set(fast);
	logger.warn(`<<< Exiting setDealign(fast=${fast})`, { "event.action": "setDealign", "logrite.fast": fast });
    }
    function set(fast) {
	logger.error(`>>> Entering set(fast=${fast})`, { "event.action": "set", "logrite.fast": fast });
      if (updating || updatingFast) return;
      clearTimeout(debounceChange);
      if (fast === true) updatingFast = true;
      debounceChange = setTimeout(update, fast === true ? 20 : 250);
	logger.warn(`<<< Exiting set(fast=${fast})`, { "event.action": "set", "logrite.fast": fast });
    }
    function change(_cm, change) {
	logger.error(`>>> Entering change(_cm=${_cm},change=${change})`, { "event.action": "change", "logrite._cm": _cm, "logrite.change": change });
      if (!dv.diffOutOfDate) {
        dv.diffOutOfDate = true;
        edit.from = edit.to = orig.from = orig.to = 0;
      }
      // Update faster when a line was added/removed
      setDealign(change.text.length - 1 != change.to.line - change.from.line);
	logger.warn(`<<< Exiting change(_cm=${_cm},change=${change})`, { "event.action": "change", "logrite._cm": _cm, "logrite.change": change });
    }
    dv.edit.on("change", change);
    dv.orig.on("change", change);
    dv.edit.on("markerAdded", setDealign);
    dv.edit.on("markerCleared", setDealign);
    dv.orig.on("markerAdded", setDealign);
    dv.orig.on("markerCleared", setDealign);
    dv.edit.on("viewportChange", function() { set(false); });
    dv.orig.on("viewportChange", function() { set(false); });
    update();
    return update;
  }

  function registerScroll(dv) {
	logger.error(`>>> Entering registerScroll(dv=${dv})`, { "event.action": "registerScroll", "logrite.dv": dv });
    dv.edit.on("scroll", function() {
      syncScroll(dv, DIFF_INSERT) && makeConnections(dv);
    });
    dv.orig.on("scroll", function() {
      syncScroll(dv, DIFF_DELETE) && makeConnections(dv);
    });
	logger.warn(`<<< Exiting registerScroll(dv=${dv})`, { "event.action": "registerScroll", "logrite.dv": dv });
  }

  function syncScroll(dv, type) {
	logger.error(`>>> Entering syncScroll(dv=${dv},type=${type})`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
    // Change handler will do a refresh after a timeout when diff is out of date
    if (dv.diffOutOfDate) return false;
    if (!dv.lockScroll) return true;
    var editor, other, now = +new Date;
    if (type == DIFF_INSERT) { editor = dv.edit; other = dv.orig; }
    else { editor = dv.orig; other = dv.edit; }
    // Don't take action if the position of this editor was recently set
    // (to prevent feedback loops)
    if (editor.state.scrollSetBy == dv && (editor.state.scrollSetAt || 0) + 50 > now) return false;

    var sInfo = editor.getScrollInfo();
	logger.error(`syncScroll(dv=${dv},type=${type}): sInfo → ${sInfo}`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
    if (dv.mv.options.connect == "align") {
      targetPos = sInfo.top;
    } else {
      var halfScreen = .5 * sInfo.clientHeight, midY = sInfo.top + halfScreen;
      var mid = editor.lineAtHeight(midY, "local");
	logger.error(`syncScroll(dv=${dv},type=${type}): mid → ${mid}`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
	logger.error(`syncScroll(dv=${dv},type=${type}): targetPos → ${targetPos}`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
      var around = chunkBoundariesAround(dv.chunks, mid, type == DIFF_INSERT);
	logger.error(`syncScroll(dv=${dv},type=${type}): around → ${around}`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
      var off = getOffsets(editor, type == DIFF_INSERT ? around.edit : around.orig);
	logger.error(`syncScroll(dv=${dv},type=${type}): off → ${off}`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
      var offOther = getOffsets(other, type == DIFF_INSERT ? around.orig : around.edit);
	logger.error(`syncScroll(dv=${dv},type=${type}): offOther → ${offOther}`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
      var ratio = (midY - off.top) / (off.bot - off.top);
	logger.error(`syncScroll(dv=${dv},type=${type}): ratio → ${ratio}`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
      var targetPos = (offOther.top - halfScreen) + ratio * (offOther.bot - offOther.top);

      var botDist, mix;
      // Some careful tweaking to make sure no space is left out of view
      // when scrolling to top or bottom.
      if (targetPos > sInfo.top && (mix = sInfo.top / halfScreen) < 1) {
        targetPos = targetPos * mix + sInfo.top * (1 - mix);
      } else if ((botDist = sInfo.height - sInfo.clientHeight - sInfo.top) < halfScreen) {
        var otherInfo = other.getScrollInfo();
		logger.error(`syncScroll(dv=${dv},type=${type}): otherInfo → ${otherInfo}`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
        var botDistOther = otherInfo.height - otherInfo.clientHeight - targetPos;
        if (botDistOther > botDist && (mix = botDist / halfScreen) < 1)
          targetPos = targetPos * mix + (otherInfo.height - otherInfo.clientHeight - botDist) * (1 - mix);
      }
    }

    other.scrollTo(sInfo.left, targetPos);
    other.state.scrollSetAt = now;
    other.state.scrollSetBy = dv;
	logger.warn(`<<< Exiting syncScroll(dv=${dv},type=${type})`, { "event.action": "syncScroll", "logrite.dv": dv, "logrite.type": type });
    return true;
  }

  function getOffsets(editor, around) {
	logger.error(`>>> Entering getOffsets(editor=${editor},around=${around})`, { "event.action": "getOffsets", "logrite.editor": editor, "logrite.around": around });
    var bot = around.after;
    if (bot == null) bot = editor.lastLine() + 1;
	logger.warn(`<<< Exiting getOffsets(editor=${editor},around=${around})`, { "event.action": "getOffsets", "logrite.editor": editor, "logrite.around": around });
    return {top: editor.heightAtLine(around.before || 0, "local"),
            bot: editor.heightAtLine(bot, "local")};
  }

  function setScrollLock(dv, val, action) {
	logger.error(`>>> Entering setScrollLock(dv=${dv},val=${val},action=${action})`, { "event.action": "setScrollLock", "logrite.dv": dv, "logrite.val": val, "logrite.action": action });
    dv.lockScroll = val;
    if (val && action != false) syncScroll(dv, DIFF_INSERT) && makeConnections(dv);
	logger.debug(`setScrollLock(dv=${dv},val=${val},action=${action}): dv.lockScroll → ${dv.lockScroll}`, { "event.action": "setScrollLock", "logrite.dv": dv, "logrite.val": val, "logrite.action": action });
    dv.lockButton.innerHTML = val ? "\u21db\u21da" : "\u21db&nbsp;&nbsp;\u21da";
	logger.warn(`<<< Exiting setScrollLock(dv=${dv},val=${val},action=${action})`, { "event.action": "setScrollLock", "logrite.dv": dv, "logrite.val": val, "logrite.action": action });
  }

  // Updating the marks for editor content

  function clearMarks(editor, arr, classes) {
	logger.error(`>>> Entering clearMarks(editor=${editor},arr=${arr},classes=${classes})`, { "event.action": "clearMarks", "logrite.editor": editor, "logrite.arr": arr, "logrite.classes": classes });
    for (var i = 0; i < arr.length; ++i) {
      var mark = arr[i];
      if (mark instanceof CodeMirror.TextMarker) {
        mark.clear();
      } else if (mark.parent) {
        editor.removeLineClass(mark, "background", classes.chunk);
        editor.removeLineClass(mark, "background", classes.start);
        editor.removeLineClass(mark, "background", classes.end);
      }
    }
    arr.length = 0;
	logger.warn(`<<< Exiting clearMarks(editor=${editor},arr=${arr},classes=${classes})`, { "event.action": "clearMarks", "logrite.editor": editor, "logrite.arr": arr, "logrite.classes": classes });
  }

  // FIXME maybe add a margin around viewport to prevent too many updates
  function updateMarks(editor, diff, state, type, classes) {
	logger.error(`>>> Entering updateMarks(editor=${editor},diff=${diff},state=${state},type=${type},classes=${classes})`, { "event.action": "updateMarks", "logrite.editor": editor, "logrite.diff": diff, "logrite.state": state, "logrite.type": type, "logrite.classes": classes });
    var vp = editor.getViewport();
    editor.operation(function() {
	logger.error(`updateMarks(editor=${editor},diff=${diff},state=${state},type=${type},classes=${classes}): vp → ${vp}`, { "event.action": "updateMarks", "logrite.editor": editor, "logrite.diff": diff, "logrite.state": state, "logrite.type": type, "logrite.classes": classes });
      if (state.from == state.to || vp.from - state.to > 20 || state.from - vp.to > 20) {
        clearMarks(editor, state.marked, classes);
        markChanges(editor, diff, type, state.marked, vp.from, vp.to, classes);
        state.from = vp.from; state.to = vp.to;
      } else {
        if (vp.from < state.from) {
          markChanges(editor, diff, type, state.marked, vp.from, state.from, classes);
          state.from = vp.from;
        }
        if (vp.to > state.to) {
          markChanges(editor, diff, type, state.marked, state.to, vp.to, classes);
          state.to = vp.to;
		logger.warn(`<<< Exiting updateMarks(editor=${editor},diff=${diff},state=${state},type=${type},classes=${classes})`, { "event.action": "updateMarks", "logrite.editor": editor, "logrite.diff": diff, "logrite.state": state, "logrite.type": type, "logrite.classes": classes });
        }
      }
    });
  }

  function markChanges(editor, diff, type, marks, from, to, classes) {
	logger.error(`>>> Entering markChanges(editor=${editor},diff=${diff},type=${type},marks=${marks},from=${from},to=${to},classes=${classes})`, { "event.action": "markChanges", "logrite.editor": editor, "logrite.diff": diff, "logrite.type": type, "logrite.marks": marks, "logrite.from": from, "logrite.to": to, "logrite.classes": classes });
    var pos = Pos(0, 0);
    var top = Pos(from, 0), bot = editor.clipPos(Pos(to - 1));
	logger.error(`markChanges(editor=${editor},diff=${diff},type=${type},marks=${marks},from=${from},to=${to},classes=${classes}): pos → ${pos}`, { "event.action": "markChanges", "logrite.editor": editor, "logrite.diff": diff, "logrite.type": type, "logrite.marks": marks, "logrite.from": from, "logrite.to": to, "logrite.classes": classes });
	logger.error(`markChanges(editor=${editor},diff=${diff},type=${type},marks=${marks},from=${from},to=${to},classes=${classes}): top → ${top}`, { "event.action": "markChanges", "logrite.editor": editor, "logrite.diff": diff, "logrite.type": type, "logrite.marks": marks, "logrite.from": from, "logrite.to": to, "logrite.classes": classes });
    var cls = type == DIFF_DELETE ? classes.del : classes.insert;
	logger.warn(`<<< Exiting markChanges(editor=${editor},diff=${diff},type=${type},marks=${marks},from=${from},to=${to},classes=${classes})`, { "event.action": "markChanges", "logrite.editor": editor, "logrite.diff": diff, "logrite.type": type, "logrite.marks": marks, "logrite.from": from, "logrite.to": to, "logrite.classes": classes });
    function markChunk(start, end) {
      var bfrom = Math.max(from, start), bto = Math.min(to, end);
	logger.error(`>>> Entering markChunk(start=${start},end=${end})`, { "event.action": "markChunk", "logrite.start": start, "logrite.end": end });
	logger.error(`markChunk(start=${start},end=${end}): line → ${line}`, { "event.action": "markChunk", "logrite.start": start, "logrite.end": end });
      for (var i = bfrom; i < bto; ++i) {
        var line = editor.addLineClass(i, "background", classes.chunk);
        if (i == start) editor.addLineClass(line, "background", classes.start);
        if (i == end - 1) editor.addLineClass(line, "background", classes.end);
        marks.push(line);
      }
      // When the chunk is empty, make sure a horizontal line shows up
      if (start == end && bfrom == end && bto == end) {
        if (bfrom)
          marks.push(editor.addLineClass(bfrom - 1, "background", classes.end));
        else
          marks.push(editor.addLineClass(bfrom, "background", classes.start));
      }
    }

    var chunkStart = 0;
    for (var i = 0; i < diff.length; ++i) {
      var part = diff[i], tp = part[0], str = part[1];
      if (tp == DIFF_EQUAL) {
        var cleanFrom = pos.line + (startOfLineClean(diff, i) ? 0 : 1);
        moveOver(pos, str);
        var cleanTo = pos.line + (endOfLineClean(diff, i) ? 1 : 0);
        if (cleanTo > cleanFrom) {
          if (i) markChunk(chunkStart, cleanFrom);
          chunkStart = cleanTo;
        }
      } else {
        if (tp == type) {
          var end = moveOver(pos, str, true);
          var a = posMax(top, pos), b = posMin(bot, end);
          if (!posEq(a, b))
            marks.push(editor.markText(a, b, {className: cls}));
          pos = end;
        }
      }
    }
    if (chunkStart <= pos.line) markChunk(chunkStart, pos.line + 1);
  }

  // Updating the gap between editor and original

  function makeConnections(dv) {
	logger.error(`>>> Entering makeConnections(dv=${dv})`, { "event.action": "makeConnections", "logrite.dv": dv });
    if (!dv.showDifferences) return;

    if (dv.svg) {
      clear(dv.svg);
      var w = dv.gap.offsetWidth;
      attrs(dv.svg, "width", w, "height", dv.gap.offsetHeight);
    }
    if (dv.copyButtons) clear(dv.copyButtons);

    var vpEdit = dv.edit.getViewport(), vpOrig = dv.orig.getViewport();
	logger.error(`makeConnections(dv=${dv}): vpEdit → ${vpEdit}`, { "event.action": "makeConnections", "logrite.dv": dv });
    var sTopEdit = dv.edit.getScrollInfo().top, sTopOrig = dv.orig.getScrollInfo().top;
	logger.error(`makeConnections(dv=${dv}): sTopEdit → ${sTopEdit}`, { "event.action": "makeConnections", "logrite.dv": dv });
    for (var i = 0; i < dv.chunks.length; i++) {
      var ch = dv.chunks[i];
      if (ch.editFrom <= vpEdit.to && ch.editTo >= vpEdit.from &&
          ch.origFrom <= vpOrig.to && ch.origTo >= vpOrig.from)
        drawConnectorsForChunk(dv, ch, sTopOrig, sTopEdit, w);
		logger.warn(`<<< Exiting makeConnections(dv=${dv})`, { "event.action": "makeConnections", "logrite.dv": dv });
    }
  }

  function getMatchingOrigLine(editLine, chunks) {
	logger.error(`>>> Entering getMatchingOrigLine(editLine=${editLine},chunks=${chunks})`, { "event.action": "getMatchingOrigLine", "logrite.editLine": editLine, "logrite.chunks": chunks });
    var editStart = 0, origStart = 0;
    for (var i = 0; i < chunks.length; i++) {
      var chunk = chunks[i];
      if (chunk.editTo > editLine && chunk.editFrom <= editLine) return null;
      if (chunk.editFrom > editLine) break;
      editStart = chunk.editTo;
      origStart = chunk.origTo;
    }
	logger.warn(`<<< Exiting getMatchingOrigLine(editLine=${editLine},chunks=${chunks})`, { "event.action": "getMatchingOrigLine", "logrite.editLine": editLine, "logrite.chunks": chunks });
    return origStart + (editLine - editStart);
  }

  function findAlignedLines(dv, other) {
	logger.error(`>>> Entering findAlignedLines(dv=${dv},other=${other})`, { "event.action": "findAlignedLines", "logrite.dv": dv, "logrite.other": other });
    var linesToAlign = [];
    for (var i = 0; i < dv.chunks.length; i++) {
      var chunk = dv.chunks[i];
      linesToAlign.push([chunk.origTo, chunk.editTo, other ? getMatchingOrigLine(chunk.editTo, other.chunks) : null]);
    }
    if (other) {
      for (var i = 0; i < other.chunks.length; i++) {
        var chunk = other.chunks[i];
        for (var j = 0; j < linesToAlign.length; j++) {
          var align = linesToAlign[j];
          if (align[1] == chunk.editTo) {
            j = -1;
            break;
          } else if (align[1] > chunk.editTo) {
            break;
          }
        }
        if (j > -1)
          linesToAlign.splice(j - 1, 0, [getMatchingOrigLine(chunk.editTo, dv.chunks), chunk.editTo, chunk.origTo]);
      }
    }
	logger.warn(`<<< Exiting findAlignedLines(dv=${dv},other=${other})`, { "event.action": "findAlignedLines", "logrite.dv": dv, "logrite.other": other });
    return linesToAlign;
  }

  function alignChunks(dv, force) {
	logger.error(`>>> Entering alignChunks(dv=${dv},force=${force})`, { "event.action": "alignChunks", "logrite.dv": dv, "logrite.force": force });
    if (!dv.dealigned && !force) return;
    if (!dv.orig.curOp) return dv.orig.operation(function() {
	logger.warn(`<<< Exiting alignChunks(dv=${dv},force=${force})`, { "event.action": "alignChunks", "logrite.dv": dv, "logrite.force": force });
      alignChunks(dv, force);
    });

    dv.dealigned = false;
	logger.debug(`alignChunks(dv=${dv},force=${force}): dv.dealigned → ${dv.dealigned}`, { "event.action": "alignChunks", "logrite.dv": dv, "logrite.force": force });
    var other = dv.mv.left == dv ? dv.mv.right : dv.mv.left;
	logger.debug(`alignChunks(dv=${dv},force=${force}): other.dealigned → ${other.dealigned}`, { "event.action": "alignChunks", "logrite.dv": dv, "logrite.force": force });
    if (other) {
      ensureDiff(other);
      other.dealigned = false;
    }
    var linesToAlign = findAlignedLines(dv, other);
	logger.error(`alignChunks(dv=${dv},force=${force}): linesToAlign → ${linesToAlign}`, { "event.action": "alignChunks", "logrite.dv": dv, "logrite.force": force });

    // Clear old aligners
    var aligners = dv.mv.aligners;
	logger.debug(`alignChunks(dv=${dv},force=${force}): aligners.length → ${aligners.length}`, { "event.action": "alignChunks", "logrite.dv": dv, "logrite.force": force });
    for (var i = 0; i < aligners.length; i++)
      aligners[i].clear();
    aligners.length = 0;

    var cm = [dv.orig, dv.edit], scroll = [];
    if (other) cm.push(other.orig);
    for (var i = 0; i < cm.length; i++)
      scroll.push(cm[i].getScrollInfo().top);

    for (var ln = 0; ln < linesToAlign.length; ln++)
      alignLines(cm, linesToAlign[ln], aligners);

    for (var i = 0; i < cm.length; i++)
      cm[i].scrollTo(null, scroll[i]);
  }

  function alignLines(cm, lines, aligners) {
	logger.error(`>>> Entering alignLines(cm=${cm},lines=${lines},aligners=${aligners})`, { "event.action": "alignLines", "logrite.cm": cm, "logrite.lines": lines, "logrite.aligners": aligners });
    var maxOffset = 0, offset = [];
    for (var i = 0; i < cm.length; i++) if (lines[i] != null) {
	logger.error(`alignLines(cm=${cm},lines=${lines},aligners=${aligners}): maxOffset → ${maxOffset}`, { "event.action": "alignLines", "logrite.cm": cm, "logrite.lines": lines, "logrite.aligners": aligners });
      var off = cm[i].heightAtLine(lines[i], "local");
	logger.error(`alignLines(cm=${cm},lines=${lines},aligners=${aligners}): off → ${off}`, { "event.action": "alignLines", "logrite.cm": cm, "logrite.lines": lines, "logrite.aligners": aligners });
      offset[i] = off;
      maxOffset = Math.max(maxOffset, off);
    }
    for (var i = 0; i < cm.length; i++) if (lines[i] != null) {
      var diff = maxOffset - offset[i];
      if (diff > 1)
        aligners.push(padAbove(cm[i], lines[i], diff));
    }
  }

  function padAbove(cm, line, size) {
	logger.error(`>>> Entering padAbove(cm=${cm},line=${line},size=${size})`, { "event.action": "padAbove", "logrite.cm": cm, "logrite.line": line, "logrite.size": size });
    var above = true;
    if (line > cm.lastLine()) {
      line--;
      above = false;
    }
    var elt = document.createElement("div");
    elt.className = "CodeMirror-merge-spacer";
    elt.style.height = size + "px"; elt.style.minWidth = "1px";
	logger.warn(`<<< Exiting padAbove(cm=${cm},line=${line},size=${size})`, { "event.action": "padAbove", "logrite.cm": cm, "logrite.line": line, "logrite.size": size });
    return cm.addLineWidget(line, elt, {height: size, above: above});
  }

  function drawConnectorsForChunk(dv, chunk, sTopOrig, sTopEdit, w) {
	logger.error(`>>> Entering drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w})`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
    var flip = dv.type == "left";
    var top = dv.orig.heightAtLine(chunk.origFrom, "local") - sTopOrig;
	logger.error(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): flip → ${flip}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
    if (dv.svg) {
      var topLpx = top;
      var topRpx = dv.edit.heightAtLine(chunk.editFrom, "local") - sTopEdit;
	logger.error(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): topRpx → ${topRpx}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
      if (flip) { var tmp = topLpx; topLpx = topRpx; topRpx = tmp; }
      var botLpx = dv.orig.heightAtLine(chunk.origTo, "local") - sTopOrig;
	logger.error(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): botLpx → ${botLpx}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
      var botRpx = dv.edit.heightAtLine(chunk.editTo, "local") - sTopEdit;
	logger.error(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): botRpx → ${botRpx}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
      if (flip) { var tmp = botLpx; botLpx = botRpx; botRpx = tmp; }
      var curveTop = " C " + w/2 + " " + topRpx + " " + w/2 + " " + topLpx + " " + (w + 2) + " " + topLpx;
	logger.error(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): curveTop → ${curveTop}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
      var curveBot = " C " + w/2 + " " + botLpx + " " + w/2 + " " + botRpx + " -1 " + botRpx;
	logger.error(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): curveBot → ${curveBot}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
      attrs(dv.svg.appendChild(document.createElementNS(svgNS, "path")),
            "d", "M -1 " + topRpx + curveTop + " L " + (w + 2) + " " + botLpx + curveBot + " z",
            "class", dv.classes.connect);
    }
    if (dv.copyButtons) {
      var copy = dv.copyButtons.appendChild(elt("div", dv.type == "left" ? "\u21dd" : "\u21dc",
                                                "CodeMirror-merge-copy"));
      var editOriginals = dv.mv.options.allowEditingOriginals;
	logger.debug(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): copy.chunk → ${copy.chunk}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
      copy.title = editOriginals ? "Push to left" : "Revert chunk";
	logger.debug(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): copy.title → ${copy.title}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
      copy.chunk = chunk;
      copy.style.top = top + "px";

      if (editOriginals) {
        var topReverse = dv.orig.heightAtLine(chunk.editFrom, "local") - sTopEdit;
		logger.error(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): topReverse → ${topReverse}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
        var copyReverse = dv.copyButtons.appendChild(elt("div", dv.type == "right" ? "\u21dd" : "\u21dc",
                                                         "CodeMirror-merge-copy-reverse"));
        copyReverse.title = "Push to right";
		logger.debug(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): copyReverse.title → ${copyReverse.title}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
        copyReverse.chunk = {editFrom: chunk.origFrom, editTo: chunk.origTo,
                             origFrom: chunk.editFrom, origTo: chunk.editTo};
        copyReverse.style.top = topReverse + "px";
        dv.type == "right" ? copyReverse.style.left = "2px" : copyReverse.style.right = "2px";
		logger.error(`drawConnectorsForChunk(dv=${dv},chunk=${chunk},sTopOrig=${sTopOrig},sTopEdit=${sTopEdit},w=${w}): top → ${top}`, { "event.action": "drawConnectorsForChunk", "logrite.dv": dv, "logrite.chunk": chunk, "logrite.sTopOrig": sTopOrig, "logrite.sTopEdit": sTopEdit, "logrite.w": w });
      }
    }
  }

  function copyChunk(dv, to, from, chunk) {
	logger.error(`>>> Entering copyChunk(dv=${dv},to=${to},from=${from},chunk=${chunk})`, { "event.action": "copyChunk", "logrite.dv": dv, "logrite.to": to, "logrite.from": from, "logrite.chunk": chunk });
    if (dv.diffOutOfDate) return;
    to.replaceRange(from.getRange(Pos(chunk.origFrom, 0), Pos(chunk.origTo, 0)),
                         Pos(chunk.editFrom, 0), Pos(chunk.editTo, 0));
						logger.warn(`<<< Exiting copyChunk(dv=${dv},to=${to},from=${from},chunk=${chunk})`, { "event.action": "copyChunk", "logrite.dv": dv, "logrite.to": to, "logrite.from": from, "logrite.chunk": chunk });
  }

  // Merge view, containing 0, 1, or 2 diff views.

  var MergeView = CodeMirror.MergeView = function(node, options) {
	logger.error(`>>> Entering MergeView(node=${node},options=${options})`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
    if (!(this instanceof MergeView)) return new MergeView(node, options);
	logger.warn(`<<< Exiting MergeView(node=${node},options=${options})`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });

    this.options = options;
    var origLeft = options.origLeft, origRight = options.origRight == null ? options.orig : options.origRight;
	logger.debug(`MergeView(node=${node},options=${options}): options → ${options}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
	logger.debug(`MergeView(node=${node},options=${options}): this.options → ${this.options}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });

    var hasLeft = origLeft != null, hasRight = origRight != null;
    var panes = 1 + (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);
	logger.error(`MergeView(node=${node},options=${options}): panes → ${panes}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
    var wrap = [], left = this.left = null, right = this.right = null;
	logger.error(`MergeView(node=${node},options=${options}): wrap → ${wrap}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
	logger.debug(`MergeView(node=${node},options=${options}): wrap → ${wrap}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
    var self = this;

    if (hasLeft) {
      left = this.left = new DiffView(this, "left");
      var leftPane = elt("div", null, "CodeMirror-merge-pane");
      wrap.push(leftPane);
	logger.error(`MergeView(node=${node},options=${options}): leftPane → ${leftPane}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
      wrap.push(buildGap(left));
    }

    var editPane = elt("div", null, "CodeMirror-merge-pane");
	logger.error(`MergeView(node=${node},options=${options}): editPane → ${editPane}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
    wrap.push(editPane);

    if (hasRight) {
      right = this.right = new DiffView(this, "right");
      wrap.push(buildGap(right));
      var rightPane = elt("div", null, "CodeMirror-merge-pane");
	logger.error(`MergeView(node=${node},options=${options}): rightPane → ${rightPane}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
      wrap.push(rightPane);
    }

    (hasRight ? rightPane : editPane).className += " CodeMirror-merge-pane-rightmost";

    wrap.push(elt("div", null, null, "height: 0; clear: both;"));

    var wrapElt = this.wrap = node.appendChild(elt("div", wrap, "CodeMirror-merge CodeMirror-merge-" + panes + "pane"));
	logger.error(`MergeView(node=${node},options=${options}): wrapElt → ${wrapElt}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
    this.edit = CodeMirror(editPane, copyObj(options));

    if (left) left.init(leftPane, origLeft, options);
    if (right) right.init(rightPane, origRight, options);

    if (options.collapseIdentical) {
      updating = true;
      this.editor().operation(function() {
        collapseIdenticalStretches(self, options.collapseIdentical);
      });
      updating = false;
    }
    if (options.connect == "align") {
      this.aligners = [];
      alignChunks(this.left || this.right, true);
	logger.debug(`MergeView(node=${node},options=${options}): this.aligners → ${this.aligners}`, { "event.action": "MergeView", "logrite.node": node, "logrite.options": options });
    }

    var onResize = function() {
	logger.error('>>> Entering onResize()', { "event.action": "onResize" });
      if (left) makeConnections(left);
      if (right) makeConnections(right);
	logger.warn('<<< Exiting onResize()', { "event.action": "onResize" });
    };
    CodeMirror.on(window, "resize", onResize);
    var resizeInterval = setInterval(function() {
      for (var p = wrapElt.parentNode; p && p != document.body; p = p.parentNode) {}
      if (!p) { clearInterval(resizeInterval); CodeMirror.off(window, "resize", onResize); }
    }, 5000);
  };

  function buildGap(dv) {
	logger.error(`>>> Entering buildGap(dv=${dv})`, { "event.action": "buildGap", "logrite.dv": dv });
    var lock = dv.lockButton = elt("div", null, "CodeMirror-merge-scrolllock");
    lock.title = "Toggle locked scrolling";
	logger.error(`buildGap(dv=${dv}): lock → ${lock}`, { "event.action": "buildGap", "logrite.dv": dv });
	logger.debug(`buildGap(dv=${dv}): lock.title → ${lock.title}`, { "event.action": "buildGap", "logrite.dv": dv });
    var lockWrap = elt("div", [lock], "CodeMirror-merge-scrolllock-wrap");
	logger.error(`buildGap(dv=${dv}): lockWrap → ${lockWrap}`, { "event.action": "buildGap", "logrite.dv": dv });
    CodeMirror.on(lock, "click", function() { setScrollLock(dv, !dv.lockScroll); });
    var gapElts = [lockWrap];
    if (dv.mv.options.revertButtons !== false) {
      dv.copyButtons = elt("div", null, "CodeMirror-merge-copybuttons-" + dv.type);
      CodeMirror.on(dv.copyButtons, "click", function(e) {
        var node = e.target || e.srcElement;
        if (!node.chunk) return;
        if (node.className == "CodeMirror-merge-copy-reverse") {
          copyChunk(dv, dv.orig, dv.edit, node.chunk);
          return;
        }
        copyChunk(dv, dv.edit, dv.orig, node.chunk);
      });
      gapElts.unshift(dv.copyButtons);
    }
    if (dv.mv.options.connect != "align") {
      var svg = document.createElementNS && document.createElementNS(svgNS, "svg");
	logger.error(`buildGap(dv=${dv}): svg → ${svg}`, { "event.action": "buildGap", "logrite.dv": dv });
      if (svg && !svg.createSVGRect) svg = null;
      dv.svg = svg;
      if (svg) gapElts.push(svg);
	logger.debug(`buildGap(dv=${dv}): dv.svg → ${dv.svg}`, { "event.action": "buildGap", "logrite.dv": dv });
    }

    return dv.gap = elt("div", gapElts, "CodeMirror-merge-gap");
  }

  MergeView.prototype = {
    constuctor: MergeView,
    editor: function() {
        logger.error('>>> Entering editor()', { "event.action": "editor" });
        logger.warn('<<< Exiting editor()', { "event.action": "editor" });
        return this.edit;
    },
    rightOriginal: function() {
        logger.error('>>> Entering rightOriginal()', { "event.action": "rightOriginal" });
        logger.warn('<<< Exiting rightOriginal()', { "event.action": "rightOriginal" });
        return this.right && this.right.orig;
    },
    leftOriginal: function() {
        logger.error('>>> Entering leftOriginal()', { "event.action": "leftOriginal" });
        logger.warn('<<< Exiting leftOriginal()', { "event.action": "leftOriginal" });
        return this.left && this.left.orig;
    },
    setShowDifferences: function(val) {
      if (this.right) this.right.setShowDifferences(val);
      if (this.left) this.left.setShowDifferences(val);
    },
    rightChunks: function() {
	logger.error('>>> Entering rightChunks()', { "event.action": "rightChunks" });
      if (this.right) { ensureDiff(this.right); return this.right.chunks; }
	logger.warn('<<< Exiting rightChunks()', { "event.action": "rightChunks" });
    },
    leftChunks: function() {
	logger.error('>>> Entering leftChunks()', { "event.action": "leftChunks" });
      if (this.left) { ensureDiff(this.left); return this.left.chunks; }
	logger.warn('<<< Exiting leftChunks()', { "event.action": "leftChunks" });
    }
  };

  function asString(obj) {
	logger.error(`>>> Entering asString(obj=${obj})`, { "event.action": "asString", "logrite.obj": obj });
    if (typeof obj == "string") return obj;
    else return obj.getValue();
  }

  // Operations on diffs

  var dmp = new diff_match_patch();
  function getDiff(a, b) {
	logger.error(`>>> Entering getDiff(a=${a},b=${b})`, { "event.action": "getDiff", "logrite.a": a, "logrite.b": b });
    var diff = dmp.diff_main(a, b);
    dmp.diff_cleanupSemantic(diff);
	logger.error(`getDiff(a=${a},b=${b}): diff → ${diff}`, { "event.action": "getDiff", "logrite.a": a, "logrite.b": b });
    // The library sometimes leaves in empty parts, which confuse the algorithm
    for (var i = 0; i < diff.length; ++i) {
      var part = diff[i];
      if (!part[1]) {
        diff.splice(i--, 1);
      } else if (i && diff[i - 1][0] == part[0]) {
        diff.splice(i--, 1);
        diff[i][1] += part[1];
      }
    }
	logger.warn(`<<< Exiting getDiff(a=${a},b=${b})`, { "event.action": "getDiff", "logrite.a": a, "logrite.b": b });
    return diff;
  }

  function getChunks(diff) {
	logger.error(`>>> Entering getChunks(diff=${diff})`, { "event.action": "getChunks", "logrite.diff": diff });
    var chunks = [];
    var startEdit = 0, startOrig = 0;
    var edit = Pos(0, 0), orig = Pos(0, 0);
	logger.error(`getChunks(diff=${diff}): edit → ${edit}`, { "event.action": "getChunks", "logrite.diff": diff });
    for (var i = 0; i < diff.length; ++i) {
      var part = diff[i], tp = part[0];
      if (tp == DIFF_EQUAL) {
        var startOff = startOfLineClean(diff, i) ? 0 : 1;
		logger.error(`getChunks(diff=${diff}): startOff → ${startOff}`, { "event.action": "getChunks", "logrite.diff": diff });
        var cleanFromEdit = edit.line + startOff, cleanFromOrig = orig.line + startOff;
        moveOver(edit, part[1], null, orig);
        var endOff = endOfLineClean(diff, i) ? 1 : 0;
		logger.error(`getChunks(diff=${diff}): endOff → ${endOff}`, { "event.action": "getChunks", "logrite.diff": diff });
        var cleanToEdit = edit.line + endOff, cleanToOrig = orig.line + endOff;
        if (cleanToEdit > cleanFromEdit) {
          if (i) chunks.push({origFrom: startOrig, origTo: cleanFromOrig,
                              editFrom: startEdit, editTo: cleanFromEdit});
          startEdit = cleanToEdit; startOrig = cleanToOrig;
        }
      } else {
        moveOver(tp == DIFF_INSERT ? edit : orig, part[1]);
      }
    }
    if (startEdit <= edit.line || startOrig <= orig.line)
      chunks.push({origFrom: startOrig, origTo: orig.line + 1,
                   editFrom: startEdit, editTo: edit.line + 1});
	logger.warn(`<<< Exiting getChunks(diff=${diff})`, { "event.action": "getChunks", "logrite.diff": diff });
    return chunks;
  }

  function endOfLineClean(diff, i) {
	logger.error(`>>> Entering endOfLineClean(diff=${diff},i=${i})`, { "event.action": "endOfLineClean", "logrite.diff": diff, "logrite.i": i });
    if (i == diff.length - 1) return true;
    var next = diff[i + 1][1];
    if (next.length == 1 || next.charCodeAt(0) != 10) return false;
    if (i == diff.length - 2) return true;
    next = diff[i + 2][1];
	logger.warn(`<<< Exiting endOfLineClean(diff=${diff},i=${i})`, { "event.action": "endOfLineClean", "logrite.diff": diff, "logrite.i": i });
    return next.length > 1 && next.charCodeAt(0) == 10;
  }

  function startOfLineClean(diff, i) {
	logger.error(`>>> Entering startOfLineClean(diff=${diff},i=${i})`, { "event.action": "startOfLineClean", "logrite.diff": diff, "logrite.i": i });
    if (i == 0) return true;
    var last = diff[i - 1][1];
    if (last.charCodeAt(last.length - 1) != 10) return false;
    if (i == 1) return true;
    last = diff[i - 2][1];
	logger.warn(`<<< Exiting startOfLineClean(diff=${diff},i=${i})`, { "event.action": "startOfLineClean", "logrite.diff": diff, "logrite.i": i });
    return last.charCodeAt(last.length - 1) == 10;
  }

  function chunkBoundariesAround(chunks, n, nInEdit) {
	logger.error(`>>> Entering chunkBoundariesAround(chunks=${chunks},n=${n},nInEdit=${nInEdit})`, { "event.action": "chunkBoundariesAround", "logrite.chunks": chunks, "logrite.n": n, "logrite.nInEdit": nInEdit });
    var beforeE, afterE, beforeO, afterO;
    for (var i = 0; i < chunks.length; i++) {
      var chunk = chunks[i];
      var fromLocal = nInEdit ? chunk.editFrom : chunk.origFrom;
      var toLocal = nInEdit ? chunk.editTo : chunk.origTo;
      if (afterE == null) {
        if (fromLocal > n) { afterE = chunk.editFrom; afterO = chunk.origFrom; }
        else if (toLocal > n) { afterE = chunk.editTo; afterO = chunk.origTo; }
      }
      if (toLocal <= n) { beforeE = chunk.editTo; beforeO = chunk.origTo; }
      else if (fromLocal <= n) { beforeE = chunk.editFrom; beforeO = chunk.origFrom; }
    }
	logger.warn(`<<< Exiting chunkBoundariesAround(chunks=${chunks},n=${n},nInEdit=${nInEdit})`, { "event.action": "chunkBoundariesAround", "logrite.chunks": chunks, "logrite.n": n, "logrite.nInEdit": nInEdit });
    return {edit: {before: beforeE, after: afterE}, orig: {before: beforeO, after: afterO}};
  }

  function collapseSingle(cm, from, to) {
	logger.error(`>>> Entering collapseSingle(cm=${cm},from=${from},to=${to})`, { "event.action": "collapseSingle", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
    cm.addLineClass(from, "wrap", "CodeMirror-merge-collapsed-line");
    var widget = document.createElement("span");
	logger.error(`collapseSingle(cm=${cm},from=${from},to=${to}): widget → ${widget}`, { "event.action": "collapseSingle", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
    widget.className = "CodeMirror-merge-collapsed-widget";
	logger.debug(`collapseSingle(cm=${cm},from=${from},to=${to}): widget.className → ${widget.className}`, { "event.action": "collapseSingle", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
    widget.title = "Identical text collapsed. Click to expand.";
    var mark = cm.markText(Pos(from, 0), Pos(to - 1), {
      inclusiveLeft: true,
      inclusiveRight: true,
      replacedWith: widget,
      clearOnEnter: true
    });
    function clear() {
      mark.clear();
      cm.removeLineClass(from, "wrap", "CodeMirror-merge-collapsed-line");
	logger.error(`clear(node=${node}): count → ${count}`, { "event.action": "clear", "logrite.node": node });
    }
    widget.addEventListener("click", clear);
    return {mark: mark, clear: clear};
  }

  function collapseStretch(size, editors) {
	logger.error(`>>> Entering collapseStretch(size=${size},editors=${editors})`, { "event.action": "collapseStretch", "logrite.size": size, "logrite.editors": editors });
    var marks = [];
    function clear() {
      for (var i = 0; i < marks.length; i++) marks[i].clear();
    }
    for (var i = 0; i < editors.length; i++) {
      var editor = editors[i];
      var mark = collapseSingle(editor.cm, editor.line, editor.line + size);
	logger.error(`collapseStretch(size=${size},editors=${editors}): mark → ${mark}`, { "event.action": "collapseStretch", "logrite.size": size, "logrite.editors": editors });
      marks.push(mark);
      mark.mark.on("clear", clear);
    }
    return marks[0].mark;
  }

  function unclearNearChunks(dv, margin, off, clear) {
	logger.error(`>>> Entering unclearNearChunks(dv=${dv},margin=${margin},off=${off},clear=${clear})`, { "event.action": "unclearNearChunks", "logrite.dv": dv, "logrite.margin": margin, "logrite.off": off, "logrite.clear": clear });
    for (var i = 0; i < dv.chunks.length; i++) {
      var chunk = dv.chunks[i];
      for (var l = chunk.editFrom - margin; l < chunk.editTo + margin; l++) {
        var pos = l + off;
        if (pos >= 0 && pos < clear.length) clear[pos] = false;
		logger.warn(`<<< Exiting unclearNearChunks(dv=${dv},margin=${margin},off=${off},clear=${clear})`, { "event.action": "unclearNearChunks", "logrite.dv": dv, "logrite.margin": margin, "logrite.off": off, "logrite.clear": clear });
      }
    }
  }

  function collapseIdenticalStretches(mv, margin) {
	logger.error(`>>> Entering collapseIdenticalStretches(mv=${mv},margin=${margin})`, { "event.action": "collapseIdenticalStretches", "logrite.mv": mv, "logrite.margin": margin });
    if (typeof margin != "number") margin = 2;
    var clear = [], edit = mv.editor(), off = edit.firstLine();
    for (var l = off, e = edit.lastLine(); l <= e; l++) clear.push(true);
	logger.error(`collapseIdenticalStretches(mv=${mv},margin=${margin}): l → ${l}`, { "event.action": "collapseIdenticalStretches", "logrite.mv": mv, "logrite.margin": margin });
    if (mv.left) unclearNearChunks(mv.left, margin, off, clear);
    if (mv.right) unclearNearChunks(mv.right, margin, off, clear);

    for (var i = 0; i < clear.length; i++) {
      if (clear[i]) {
        var line = i + off;
        for (var size = 1; i < clear.length - 1 && clear[i + 1]; i++, size++) {}
        if (size > margin) {
          var editors = [{line: line, cm: edit}];
          if (mv.left) editors.push({line: getMatchingOrigLine(line, mv.left.chunks), cm: mv.left.orig});
          if (mv.right) editors.push({line: getMatchingOrigLine(line, mv.right.chunks), cm: mv.right.orig});
          var mark = collapseStretch(size, editors);
          if (mv.options.onCollapse) mv.options.onCollapse(mv, line, size, mark);
		logger.error(`collapseIdenticalStretches(mv=${mv},margin=${margin}): mark → ${mark}`, { "event.action": "collapseIdenticalStretches", "logrite.mv": mv, "logrite.margin": margin });
        }
      }
    }
  }

  // General utilities

  function elt(tag, content, className, style) {
    var e = document.createElement(tag);
    if (className) e.className = className;
    if (style) e.style.cssText = style;
    if (typeof content == "string") e.appendChild(document.createTextNode(content));
    else if (content) for (var i = 0; i < content.length; ++i) e.appendChild(content[i]);
    return e;
  }

  function clear(node) {
	logger.error(`>>> Entering clear(node=${node})`, { "event.action": "clear", "logrite.node": node });
    for (var count = node.childNodes.length; count > 0; --count)
      node.removeChild(node.firstChild);
	logger.warn(`<<< Exiting clear(node=${node})`, { "event.action": "clear", "logrite.node": node });
  }

  function attrs(elt) {
	logger.error(`>>> Entering attrs(elt=${elt})`, { "event.action": "attrs", "logrite.elt": elt });
    for (var i = 1; i < arguments.length; i += 2)
      elt.setAttribute(arguments[i], arguments[i+1]);
	logger.warn(`<<< Exiting attrs(elt=${elt})`, { "event.action": "attrs", "logrite.elt": elt });
  }

  function copyObj(obj, target) {
	logger.error(`>>> Entering copyObj(obj=${obj},target=${target})`, { "event.action": "copyObj", "logrite.obj": obj, "logrite.target": target });
    if (!target) target = {};
    for (var prop in obj) if (obj.hasOwnProperty(prop)) target[prop] = obj[prop];
	logger.warn(`<<< Exiting copyObj(obj=${obj},target=${target})`, { "event.action": "copyObj", "logrite.obj": obj, "logrite.target": target });
    return target;
  }

  function moveOver(pos, str, copy, other) {
	logger.error(`>>> Entering moveOver(pos=${pos},str=${str},copy=${copy},other=${other})`, { "event.action": "moveOver", "logrite.pos": pos, "logrite.str": str, "logrite.copy": copy, "logrite.other": other });
    var out = copy ? Pos(pos.line, pos.ch) : pos, at = 0;
    for (;;) {
	logger.error(`moveOver(pos=${pos},str=${str},copy=${copy},other=${other}): out → ${out}`, { "event.action": "moveOver", "logrite.pos": pos, "logrite.str": str, "logrite.copy": copy, "logrite.other": other });
      var nl = str.indexOf("\n", at);
	logger.error(`moveOver(pos=${pos},str=${str},copy=${copy},other=${other}): nl → ${nl}`, { "event.action": "moveOver", "logrite.pos": pos, "logrite.str": str, "logrite.copy": copy, "logrite.other": other });
      if (nl == -1) break;
      ++out.line;
      if (other) ++other.line;
      at = nl + 1;
    }
    out.ch = (at ? 0 : out.ch) + (str.length - at);
    if (other) other.ch = (at ? 0 : other.ch) + (str.length - at);
	logger.warn(`<<< Exiting moveOver(pos=${pos},str=${str},copy=${copy},other=${other})`, { "event.action": "moveOver", "logrite.pos": pos, "logrite.str": str, "logrite.copy": copy, "logrite.other": other });
    return out;
  }

  function posMin(a, b) { return (a.line - b.line || a.ch - b.ch) < 0 ? a : b; }
  function posMax(a, b) {
      logger.error(`>>> Entering posMax(a=${a},b=${b})`, { "event.action": "posMax", "logrite.a": a, "logrite.b": b });
      logger.warn(`<<< Exiting posMax(a=${a},b=${b})`, { "event.action": "posMax", "logrite.a": a, "logrite.b": b });
      return (a.line - b.line || a.ch - b.ch) > 0 ? a : b;
  }
  function posEq(a, b) {
      logger.error(`>>> Entering posEq(a=${a},b=${b})`, { "event.action": "posEq", "logrite.a": a, "logrite.b": b });
      logger.warn(`<<< Exiting posEq(a=${a},b=${b})`, { "event.action": "posEq", "logrite.a": a, "logrite.b": b });
      return a.line == b.line && a.ch == b.ch;
  }

  function findPrevDiff(chunks, start, isOrig) {
	logger.error(`>>> Entering findPrevDiff(chunks=${chunks},start=${start},isOrig=${isOrig})`, { "event.action": "findPrevDiff", "logrite.chunks": chunks, "logrite.start": start, "logrite.isOrig": isOrig });
    for (var i = chunks.length - 1; i >= 0; i--) {
      var chunk = chunks[i];
      var to = (isOrig ? chunk.origTo : chunk.editTo) - 1;
      if (to < start) return to;
	logger.error(`findPrevDiff(chunks=${chunks},start=${start},isOrig=${isOrig}): to → ${to}`, { "event.action": "findPrevDiff", "logrite.chunks": chunks, "logrite.start": start, "logrite.isOrig": isOrig });
    }
  }

  function findNextDiff(chunks, start, isOrig) {
	logger.error(`>>> Entering findNextDiff(chunks=${chunks},start=${start},isOrig=${isOrig})`, { "event.action": "findNextDiff", "logrite.chunks": chunks, "logrite.start": start, "logrite.isOrig": isOrig });
    for (var i = 0; i < chunks.length; i++) {
      var chunk = chunks[i];
      var from = (isOrig ? chunk.origFrom : chunk.editFrom);
      if (from > start) return from;
	logger.error(`findNextDiff(chunks=${chunks},start=${start},isOrig=${isOrig}): from → ${from}`, { "event.action": "findNextDiff", "logrite.chunks": chunks, "logrite.start": start, "logrite.isOrig": isOrig });
    }
  }

  function goNearbyDiff(cm, dir) {
	logger.error(`>>> Entering goNearbyDiff(cm=${cm},dir=${dir})`, { "event.action": "goNearbyDiff", "logrite.cm": cm, "logrite.dir": dir });
    var found = null, views = cm.state.diffViews, line = cm.getCursor().line;
    if (views) for (var i = 0; i < views.length; i++) {
      var dv = views[i], isOrig = cm == dv.orig;
      ensureDiff(dv);
      var pos = dir < 0 ? findPrevDiff(dv.chunks, line, isOrig) : findNextDiff(dv.chunks, line, isOrig);
	logger.error(`goNearbyDiff(cm=${cm},dir=${dir}): pos → ${pos}`, { "event.action": "goNearbyDiff", "logrite.cm": cm, "logrite.dir": dir });
      if (pos != null && (found == null || (dir < 0 ? pos > found : pos < found)))
        found = pos;
    }
    if (found != null)
      cm.setCursor(found, 0);
    else
      return CodeMirror.Pass;
  }

  CodeMirror.commands.goNextDiff = function(cm) {
	logger.error(`>>> Entering goNextDiff(cm=${cm})`, { "event.action": "goNextDiff", "logrite.cm": cm });
	logger.warn(`<<< Exiting goNextDiff(cm=${cm})`, { "event.action": "goNextDiff", "logrite.cm": cm });
    return goNearbyDiff(cm, 1);
  };
  CodeMirror.commands.goPrevDiff = function(cm) {
	logger.error(`>>> Entering goPrevDiff(cm=${cm})`, { "event.action": "goPrevDiff", "logrite.cm": cm });
	logger.warn(`<<< Exiting goPrevDiff(cm=${cm})`, { "event.action": "goPrevDiff", "logrite.cm": cm });
    return goNearbyDiff(cm, -1);
  };
});
