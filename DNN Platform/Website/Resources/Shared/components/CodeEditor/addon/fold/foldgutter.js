const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("./foldcode"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "./foldcode"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineOption("foldGutter", false, function(cm, val, old) {
	logger.debug('>>> Entering defineOption()');
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
	logger.debug('<<< Exiting defineOption()');
    }
  });

  var Pos = CodeMirror.Pos;
logger.debug(`<<< Exiting function(exports=${exports})`);

  function State(options) {
	logger.debug(`>>> Entering State(options=${options})`);
    this.options = options;
    this.from = this.to = 0;
	logger.debug(`State(options=${options}): options → ${options}`);
	logger.debug(`State(options=${options}): this.from → ${this.from}`);
	logger.debug(`State(options=${options}): this.options → ${this.options}`);
	logger.debug(`<<< Exiting State(options=${options})`);
  }

  function parseOptions(opts) {
    if (opts === true) opts = {};
	logger.debug(`>>> Entering parseOptions(opts=${opts})`);
    if (opts.gutter == null) opts.gutter = "CodeMirror-foldgutter";
    if (opts.indicatorOpen == null) opts.indicatorOpen = "CodeMirror-foldgutter-open";
    if (opts.indicatorFolded == null) opts.indicatorFolded = "CodeMirror-foldgutter-folded";
	logger.debug(`<<< Exiting parseOptions(opts=${opts})`);
    return opts;
  }

  function isFolded(cm, line) {
	logger.debug(`>>> Entering isFolded(cm=${cm},line=${line})`);
    var marks = cm.findMarksAt(Pos(line));
    for (var i = 0; i < marks.length; ++i)
      if (marks[i].__isFold && marks[i].find().from.line == line) return marks[i];
	logger.debug(`<<< Exiting isFolded(cm=${cm},line=${line})`);
  }

  function marker(spec) {
	logger.debug(`>>> Entering marker(spec=${spec})`);
    if (typeof spec == "string") {
      var elt = document.createElement("div");
	logger.debug(`marker(spec=${spec}): elt → ${elt}`);
      elt.className = spec + " CodeMirror-guttermarker-subtle";
	logger.debug(`marker(spec=${spec}): elt.className → ${elt.className}`);
      return elt;
    } else {
	logger.debug(`<<< Exiting marker(spec=${spec})`);
      return spec.cloneNode(true);
    }
  }

  function updateFoldInfo(cm, from, to) {
	logger.debug(`>>> Entering updateFoldInfo(cm=${cm},from=${from},to=${to})`);
    var opts = cm.state.foldGutter.options, cur = from;
    var minSize = cm.foldOption(opts, "minFoldSize");
	logger.debug(`updateFoldInfo(cm=${cm},from=${from},to=${to}): minSize → ${minSize}`);
    var func = cm.foldOption(opts, "rangeFinder");
    cm.eachLine(from, to, function(line) {
	logger.debug(`<<< Exiting updateFoldInfo(cm=${cm},from=${from},to=${to})`);
      var mark = null;
	logger.debug(`>>> Entering eachLine(from=${from},to=${to},mark=${mark})`);
	logger.debug(`eachLine(from=${from},to=${to},mark=${mark}): mark → ${mark}`);
      if (isFolded(cm, cur)) {
        mark = marker(opts.indicatorFolded);
      } else {
        var pos = Pos(cur, 0);
		logger.debug(`eachLine(from=${from},to=${to},mark=${mark}): pos → ${pos}`);
        var range = func && func(cm, pos);
		logger.debug(`eachLine(from=${from},to=${to},mark=${mark}): range → ${range}`);
        if (range && range.to.line - range.from.line >= minSize)
          mark = marker(opts.indicatorOpen);
      }
      cm.setGutterMarker(line, opts.gutter, mark);
      ++cur;
    });
  }

  function updateInViewport(cm) {
	logger.debug(`>>> Entering updateInViewport(cm=${cm})`);
    var vp = cm.getViewport(), state = cm.state.foldGutter;
    if (!state) return;
	logger.debug(`updateInViewport(cm=${cm}): vp → ${vp}`);
    cm.operation(function() {
	logger.debug('>>> Entering operation()');
      updateFoldInfo(cm, vp.from, vp.to);
    });
	logger.debug('<<< Exiting operation()');
    state.from = vp.from; state.to = vp.to;
	logger.debug(`updateInViewport(cm=${cm}): state.from → ${state.from}`);
	logger.debug(`<<< Exiting updateInViewport(cm=${cm})`);
  }

  function onGutterClick(cm, line, gutter) {
	logger.debug(`>>> Entering onGutterClick(cm=${cm},line=${line},gutter=${gutter})`);
    var state = cm.state.foldGutter;
    if (!state) return;
    var opts = state.options;
    if (gutter != opts.gutter) return;
    var folded = isFolded(cm, line);
	logger.debug(`onGutterClick(cm=${cm},line=${line},gutter=${gutter}): folded → ${folded}`);
    if (folded) folded.clear();
    else cm.foldCode(Pos(line, 0), opts.rangeFinder);
	logger.debug(`<<< Exiting onGutterClick(cm=${cm},line=${line},gutter=${gutter})`);
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
	logger.debug(`>>> Entering onViewportChange(cm=${cm})`);
    var state = cm.state.foldGutter;
    if (!state) return;
    var opts = state.options;
    clearTimeout(state.changeUpdate);
	logger.debug(`<<< Exiting onViewportChange(cm=${cm})`);
    state.changeUpdate = setTimeout(function() {
      var vp = cm.getViewport();
      if (state.from == state.to || vp.from - state.to > 20 || state.from - vp.to > 20) {
		logger.debug(`setTimeout(vp=${vp}): vp → ${vp}`);
        updateInViewport(cm);
      } else {
		logger.debug(`<<< Exiting setTimeout(vp=${vp})`);
        cm.operation(function() {
          if (vp.from < state.from) {
            updateFoldInfo(cm, vp.from, state.from);
            state.from = vp.from;
			logger.debug(`onViewportChange(cm=${cm}): state.from → ${state.from}`);
          }
          if (vp.to > state.to) {
            updateFoldInfo(cm, state.to, vp.to);
            state.to = vp.to;
          }
        });
      }
    }, opts.updateViewportTimeSpan || 400);
	logger.debug(`onViewportChange(cm=${cm}): state.to → ${state.to}`);
  }

  function onFold(cm, from) {
	logger.debug(`>>> Entering onFold(cm=${cm},from=${from})`);
    var state = cm.state.foldGutter;
    if (!state) return;
    var line = from.line;
    if (line >= state.from && line < state.to)
      updateFoldInfo(cm, line, line + 1);
  }
});
