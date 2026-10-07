const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("./foldcode"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "./foldcode"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineOption("foldGutter", false, function(cm, val, old) {
    if (old && old != CodeMirror.Init) {
      cm.clearGutter(cm.state.foldGutter.options.gutter);
      cm.state.foldGutter = null;
      cm.off("gutterClick", onGutterClick);
      cm.off("change", onChange);
      cm.off("viewportChange", onViewportChange);
      cm.off("fold", onFold);
      cm.off("unfold", onFold);
      cm.off("swapDoc", updateInViewport);
    }
    if (val) {
      cm.state.foldGutter = new State(parseOptions(val));
      updateInViewport(cm);
      cm.on("gutterClick", onGutterClick);
      cm.on("change", onChange);
      cm.on("viewportChange", onViewportChange);
      cm.on("fold", onFold);
      cm.on("unfold", onFold);
      cm.on("swapDoc", updateInViewport);
    }
  });

  var Pos = CodeMirror.Pos;

  function State(options) {
	logger.error(`>>> Entering State(options=${options})`, { "event.action": "State", "logrite.options": options });
    this.options = options;
    this.from = this.to = 0;
	logger.debug(`State(options=${options}): this.options → ${this.options}`, { "event.action": "State", "logrite.options": options });
  }

  function parseOptions(opts) {
	logger.error(`>>> Entering parseOptions(opts=${opts})`, { "event.action": "parseOptions", "logrite.opts": opts });
    if (opts === true) opts = {};
    if (opts.gutter == null) opts.gutter = "CodeMirror-foldgutter";
    if (opts.indicatorOpen == null) opts.indicatorOpen = "CodeMirror-foldgutter-open";
    if (opts.indicatorFolded == null) opts.indicatorFolded = "CodeMirror-foldgutter-folded";
	logger.warn(`<<< Exiting parseOptions(opts=${opts})`, { "event.action": "parseOptions", "logrite.opts": opts });
    return opts;
  }

  function isFolded(cm, line) {
	logger.error(`>>> Entering isFolded(cm=${cm},line=${line})`, { "event.action": "isFolded", "logrite.cm": cm, "logrite.line": line });
    var marks = cm.findMarksAt(Pos(line));
    for (var i = 0; i < marks.length; ++i)
      if (marks[i].__isFold && marks[i].find().from.line == line) return marks[i];
	logger.warn(`<<< Exiting isFolded(cm=${cm},line=${line})`, { "event.action": "isFolded", "logrite.cm": cm, "logrite.line": line });
  }

  function marker(spec) {
	logger.error(`>>> Entering marker(spec=${spec})`, { "event.action": "marker", "logrite.spec": spec });
    if (typeof spec == "string") {
      var elt = document.createElement("div");
	logger.error(`marker(spec=${spec}): elt → ${elt}`, { "event.action": "marker", "logrite.spec": spec });
      elt.className = spec + " CodeMirror-guttermarker-subtle";
	logger.debug(`marker(spec=${spec}): elt.className → ${elt.className}`, { "event.action": "marker", "logrite.spec": spec });
      return elt;
    } else {
	logger.warn(`<<< Exiting marker(spec=${spec})`, { "event.action": "marker", "logrite.spec": spec });
      return spec.cloneNode(true);
    }
  }

  function updateFoldInfo(cm, from, to) {
	logger.error(`>>> Entering updateFoldInfo(cm=${cm},from=${from},to=${to})`, { "event.action": "updateFoldInfo", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
    var opts = cm.state.foldGutter.options, cur = from;
    var minSize = cm.foldOption(opts, "minFoldSize");
	logger.error(`updateFoldInfo(cm=${cm},from=${from},to=${to}): minSize → ${minSize}`, { "event.action": "updateFoldInfo", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
    var func = cm.foldOption(opts, "rangeFinder");
	logger.error(`updateFoldInfo(cm=${cm},from=${from},to=${to}): func → ${func}`, { "event.action": "updateFoldInfo", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
    cm.eachLine(from, to, function(line) {
      var mark = null;
      if (isFolded(cm, cur)) {
        mark = marker(opts.indicatorFolded);
      } else {
        var pos = Pos(cur, 0);
        var range = func && func(cm, pos);
        if (range && range.to.line - range.from.line >= minSize)
          mark = marker(opts.indicatorOpen);
      }
      cm.setGutterMarker(line, opts.gutter, mark);
      ++cur;
    });
	logger.warn(`<<< Exiting updateFoldInfo(cm=${cm},from=${from},to=${to})`, { "event.action": "updateFoldInfo", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
  }

  function updateInViewport(cm) {
	logger.error(`>>> Entering updateInViewport(cm=${cm})`, { "event.action": "updateInViewport", "logrite.cm": cm });
    var vp = cm.getViewport(), state = cm.state.foldGutter;
    if (!state) return;
	logger.error(`updateInViewport(cm=${cm}): vp → ${vp}`, { "event.action": "updateInViewport", "logrite.cm": cm });
    cm.operation(function() {
      updateFoldInfo(cm, vp.from, vp.to);
    });
    state.from = vp.from; state.to = vp.to;
	logger.warn(`<<< Exiting updateInViewport(cm=${cm})`, { "event.action": "updateInViewport", "logrite.cm": cm });
  }

  function onGutterClick(cm, line, gutter) {
	logger.error(`>>> Entering onGutterClick(cm=${cm},line=${line},gutter=${gutter})`, { "event.action": "onGutterClick", "logrite.cm": cm, "logrite.line": line, "logrite.gutter": gutter });
    var state = cm.state.foldGutter;
    if (!state) return;
    var opts = state.options;
    if (gutter != opts.gutter) return;
    var folded = isFolded(cm, line);
	logger.error(`onGutterClick(cm=${cm},line=${line},gutter=${gutter}): folded → ${folded}`, { "event.action": "onGutterClick", "logrite.cm": cm, "logrite.line": line, "logrite.gutter": gutter });
    if (folded) folded.clear();
    else cm.foldCode(Pos(line, 0), opts.rangeFinder);
	logger.warn(`<<< Exiting onGutterClick(cm=${cm},line=${line},gutter=${gutter})`, { "event.action": "onGutterClick", "logrite.cm": cm, "logrite.line": line, "logrite.gutter": gutter });
  }

  function onChange(cm) {
    var state = cm.state.foldGutter;
    if (!state) return;
    var opts = state.options;
    state.from = state.to = 0;
    clearTimeout(state.changeUpdate);
    state.changeUpdate = setTimeout(function() { updateInViewport(cm); }, opts.foldOnChangeTimeSpan || 600);
  }

  function onViewportChange(cm) {
	logger.error(`>>> Entering onViewportChange(cm=${cm})`, { "event.action": "onViewportChange", "logrite.cm": cm });
    var state = cm.state.foldGutter;
    if (!state) return;
	logger.debug(`onViewportChange(cm=${cm}): state.from → ${state.from}`, { "event.action": "onViewportChange", "logrite.cm": cm });
	logger.debug(`onViewportChange(cm=${cm}): state.to → ${state.to}`, { "event.action": "onViewportChange", "logrite.cm": cm });
    var opts = state.options;
    clearTimeout(state.changeUpdate);
    state.changeUpdate = setTimeout(function() {
      var vp = cm.getViewport();
      if (state.from == state.to || vp.from - state.to > 20 || state.from - vp.to > 20) {
        updateInViewport(cm);
      } else {
        cm.operation(function() {
          if (vp.from < state.from) {
            updateFoldInfo(cm, vp.from, state.from);
            state.from = vp.from;
          }
          if (vp.to > state.to) {
            updateFoldInfo(cm, state.to, vp.to);
            state.to = vp.to;
          }
        });
      }
    }, opts.updateViewportTimeSpan || 400);
	logger.warn(`<<< Exiting onViewportChange(cm=${cm})`, { "event.action": "onViewportChange", "logrite.cm": cm });
  }

  function onFold(cm, from) {
	logger.error(`>>> Entering onFold(cm=${cm},from=${from})`, { "event.action": "onFold", "logrite.cm": cm, "logrite.from": from });
    var state = cm.state.foldGutter;
    if (!state) return;
    var line = from.line;
    if (line >= state.from && line < state.to)
      updateFoldInfo(cm, line, line + 1);
  }
});
