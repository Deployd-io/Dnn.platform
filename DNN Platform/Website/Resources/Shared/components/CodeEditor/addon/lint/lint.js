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
  var GUTTER_ID = "CodeMirror-lint-markers";
logger.debug(`<<< Exiting function(exports=${exports})`);
logger.debug(`function(exports=${exports}): GUTTER_ID → ${GUTTER_ID}`);

  function showTooltip(e, content) {
	logger.debug(`>>> Entering showTooltip(e=${e},content=${content})`);
    var tt = document.createElement("div");
    tt.className = "CodeMirror-lint-tooltip";
	logger.debug(`showTooltip(e=${e},content=${content}): tt → ${tt}`);
	logger.debug(`showTooltip(e=${e},content=${content}): tt.className → ${tt.className}`);
    tt.appendChild(content.cloneNode(true));
    document.body.appendChild(tt);

    function position(e) {
	logger.debug(`>>> Entering position(e=${e})`);
      if (!tt.parentNode) return CodeMirror.off(document, "mousemove", position);
	logger.debug(`<<< Exiting position(e=${e})`);
      tt.style.top = Math.max(0, e.clientY - tt.offsetHeight - 5) + "px";
      tt.style.left = (e.clientX + 5) + "px";
    }
    CodeMirror.on(document, "mousemove", position);
    position(e);
    if (tt.style.opacity != null) tt.style.opacity = 1;
	logger.debug(`<<< Exiting showTooltip(e=${e},content=${content})`);
    return tt;
  }
  function rm(elt) {
	logger.debug(`>>> Entering rm(elt=${elt})`);
    if (elt.parentNode) elt.parentNode.removeChild(elt);
	logger.debug(`<<< Exiting rm(elt=${elt})`);
  }
  function hideTooltip(tt) {
	logger.debug(`>>> Entering hideTooltip(tt=${tt})`);
    if (!tt.parentNode) return;
    if (tt.style.opacity == null) rm(tt);
	logger.debug(`<<< Exiting hideTooltip(tt=${tt})`);
    tt.style.opacity = 0;
    setTimeout(function() { rm(tt); }, 600);
  }

  function showTooltipFor(e, content, node) {
	logger.debug(`>>> Entering showTooltipFor(e=${e},content=${content},node=${node})`);
    var tooltip = showTooltip(e, content);
    function hide() {
	logger.debug('>>> Entering hide()');
      CodeMirror.off(node, "mouseout", hide);
      if (tooltip) { hideTooltip(tooltip); tooltip = null; }
	logger.debug('<<< Exiting hide()');
    }
    var poll = setInterval(function() {
	logger.debug(`>>> Entering setInterval(n=${n})`);
      if (tooltip) for (var n = node;; n = n.parentNode) {
        if (n && n.nodeType == 11) n = n.host;
        if (n == document.body) return;
        if (!n) { hide(); break; }
      }
      if (!tooltip) return clearInterval(poll);
	logger.debug(`<<< Exiting setInterval(n=${n})`);
    }, 400);
    CodeMirror.on(node, "mouseout", hide);
	logger.debug(`showTooltipFor(e=${e},content=${content},node=${node}): tooltip → ${tooltip}`);
	logger.debug(`<<< Exiting showTooltipFor(e=${e},content=${content},node=${node})`);
  }

  function LintState(cm, options, hasGutter) {
	logger.debug(`>>> Entering LintState(cm=${cm},options=${options},hasGutter=${hasGutter})`);
    this.marked = [];
    this.options = options;
    this.timeout = null;
	logger.debug(`LintState(cm=${cm},options=${options},hasGutter=${hasGutter}): this.marked → ${this.marked}`);
	logger.debug(`LintState(cm=${cm},options=${options},hasGutter=${hasGutter}): options → ${options}`);
	logger.debug(`LintState(cm=${cm},options=${options},hasGutter=${hasGutter}): this.options → ${this.options}`);
    this.hasGutter = hasGutter;
    this.onMouseOver = function(e) { onMouseOver(cm, e); };
	logger.debug(`LintState(cm=${cm},options=${options},hasGutter=${hasGutter}): hasGutter → ${hasGutter}`);
	logger.debug(`<<< Exiting LintState(cm=${cm},options=${options},hasGutter=${hasGutter})`);
  }

  function parseOptions(_cm, options) {
	logger.debug(`>>> Entering parseOptions(_cm=${_cm},options=${options})`);
    if (options instanceof Function) return {getAnnotations: options};
    if (!options || options === true) options = {};
	logger.debug(`<<< Exiting parseOptions(_cm=${_cm},options=${options})`);
    return options;
  }

  function clearMarks(cm) {
	logger.debug(`>>> Entering clearMarks(cm=${cm})`);
    var state = cm.state.lint;
    if (state.hasGutter) cm.clearGutter(GUTTER_ID);
    for (var i = 0; i < state.marked.length; ++i)
      state.marked[i].clear();
    state.marked.length = 0;
	logger.debug(`<<< Exiting clearMarks(cm=${cm})`);
  }

  function makeMarker(labels, severity, multiple, tooltips) {
	logger.debug(`>>> Entering makeMarker(labels=${labels},severity=${severity},multiple=${multiple},tooltips=${tooltips})`);
    var marker = document.createElement("div"), inner = marker;
    marker.className = "CodeMirror-lint-marker-" + severity;
	logger.debug(`makeMarker(labels=${labels},severity=${severity},multiple=${multiple},tooltips=${tooltips}): marker → ${marker}`);
    if (multiple) {
      inner = marker.appendChild(document.createElement("div"));
	logger.debug(`makeMarker(labels=${labels},severity=${severity},multiple=${multiple},tooltips=${tooltips}): marker.className → ${marker.className}`);
      inner.className = "CodeMirror-lint-marker-multiple";
    }

	logger.debug(`makeMarker(labels=${labels},severity=${severity},multiple=${multiple},tooltips=${tooltips}): inner.className → ${inner.className}`);
    if (tooltips != false) CodeMirror.on(inner, "mouseover", function(e) {
	logger.debug('>>> Entering on()');
      showTooltipFor(e, labels, inner);
    });
	logger.debug('<<< Exiting on()');

	logger.debug(`<<< Exiting makeMarker(labels=${labels},severity=${severity},multiple=${multiple},tooltips=${tooltips})`);
    return marker;
  }

  function getMaxSeverity(a, b) {
	logger.debug(`>>> Entering getMaxSeverity(a=${a},b=${b})`);
    if (a == "error") return a;
    else return b;
	logger.debug(`<<< Exiting getMaxSeverity(a=${a},b=${b})`);
  }

  function groupByLine(annotations) {
	logger.debug(`>>> Entering groupByLine(annotations=${annotations})`);
    var lines = [];
    for (var i = 0; i < annotations.length; ++i) {
      var ann = annotations[i], line = ann.from.line;
      (lines[line] || (lines[line] = [])).push(ann);
    }
	logger.debug(`<<< Exiting groupByLine(annotations=${annotations})`);
    return lines;
  }

  function annotationTooltip(ann) {
	logger.debug(`>>> Entering annotationTooltip(ann=${ann})`);
    var severity = ann.severity;
    if (!severity) severity = "error";
    var tip = document.createElement("div");
	logger.debug(`annotationTooltip(ann=${ann}): tip → ${tip}`);
    tip.className = "CodeMirror-lint-message-" + severity;
    tip.appendChild(document.createTextNode(ann.message));
	logger.debug(`annotationTooltip(ann=${ann}): tip.className → ${tip.className}`);
	logger.debug(`<<< Exiting annotationTooltip(ann=${ann})`);
    return tip;
  }

  function startLinting(cm) {
	logger.debug(`>>> Entering startLinting(cm=${cm})`);
    var state = cm.state.lint, options = state.options;
    var passOptions = options.options || options; // Support deprecated passing of `options` property in options
    var getAnnotations = options.getAnnotations || cm.getHelper(CodeMirror.Pos(0, 0), "lint");
	logger.debug(`startLinting(cm=${cm}): getAnnotations → ${getAnnotations}`);
    if (!getAnnotations) return;
    if (options.async || getAnnotations.async)
      getAnnotations(cm.getValue(), updateLinting, passOptions, cm);
    else
      updateLinting(cm, getAnnotations(cm.getValue(), passOptions, cm));
  }

  function updateLinting(cm, annotationsNotSorted) {
	logger.debug(`>>> Entering updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted})`);
    clearMarks(cm);
    var state = cm.state.lint, options = state.options;

    var annotations = groupByLine(annotationsNotSorted);
	logger.debug(`updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted}): annotations → ${annotations}`);

    for (var line = 0; line < annotations.length; ++line) {
      var anns = annotations[line];
      if (!anns) continue;

      var maxSeverity = null;
	logger.debug(`updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted}): maxSeverity → ${maxSeverity}`);
      var tipLabel = state.hasGutter && document.createDocumentFragment();
	logger.debug(`updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted}): tipLabel → ${tipLabel}`);

      for (var i = 0; i < anns.length; ++i) {
        var ann = anns[i];
        var severity = ann.severity;
        if (!severity) severity = "error";
        maxSeverity = getMaxSeverity(maxSeverity, severity);

        if (options.formatAnnotation) ann = options.formatAnnotation(ann);
        if (state.hasGutter) tipLabel.appendChild(annotationTooltip(ann));

        if (ann.to) state.marked.push(cm.markText(ann.from, ann.to, {
          className: "CodeMirror-lint-mark-" + severity,
          __annotation: ann
        }));
      }

      if (state.hasGutter)
        cm.setGutterMarker(line, GUTTER_ID, makeMarker(tipLabel, maxSeverity, anns.length > 1,
                                                       state.options.tooltips));
    }
    if (options.onUpdateLinting) options.onUpdateLinting(annotationsNotSorted, annotations, cm);
	logger.debug(`<<< Exiting updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted})`);
  }

  function onChange(cm) {
    var state = cm.state.lint;
    if (!state) return;
    clearTimeout(state.timeout);
    state.timeout = setTimeout(function(){startLinting(cm);}, state.options.delay || 500);
  }

  function popupSpanTooltip(ann, e) {
	logger.debug(`>>> Entering popupSpanTooltip(ann=${ann},e=${e})`);
    var target = e.target || e.srcElement;
    showTooltipFor(e, annotationTooltip(ann), target);
	logger.debug(`<<< Exiting popupSpanTooltip(ann=${ann},e=${e})`);
  }

  function onMouseOver(cm, e) {
	logger.debug(`>>> Entering onMouseOver(cm=${cm},e=${e})`);
    var target = e.target || e.srcElement;
    if (!/\bCodeMirror-lint-mark-/.test(target.className)) return;
    var box = target.getBoundingClientRect(), x = (box.left + box.right) / 2, y = (box.top + box.bottom) / 2;
	logger.debug(`onMouseOver(cm=${cm},e=${e}): box → ${box}`);
    var spans = cm.findMarksAt(cm.coordsChar({left: x, top: y}, "client"));
	logger.debug(`onMouseOver(cm=${cm},e=${e}): spans → ${spans}`);
    for (var i = 0; i < spans.length; ++i) {
      var ann = spans[i].__annotation;
      if (ann) return popupSpanTooltip(ann, e);
	logger.debug(`<<< Exiting onMouseOver(cm=${cm},e=${e})`);
    }
  }

  CodeMirror.defineOption("lint", false, function(cm, val, old) {
	logger.debug('>>> Entering defineOption()');
    if (old && old != CodeMirror.Init) {
      clearMarks(cm);
      if (cm.state.lint.options.lintOnChange !== false)
        cm.off("change", onChange);
      CodeMirror.off(cm.getWrapperElement(), "mouseover", cm.state.lint.onMouseOver);
      clearTimeout(cm.state.lint.timeout);
      delete cm.state.lint;
    }

    if (val) {
      var gutters = cm.getOption("gutters"), hasLintGutter = false;
	logger.debug(`defineOption(): gutters → ${gutters}`);
      for (var i = 0; i < gutters.length; ++i) if (gutters[i] == GUTTER_ID) hasLintGutter = true;
      var state = cm.state.lint = new LintState(cm, parseOptions(cm, val), hasLintGutter);
      if (state.options.lintOnChange !== false)
        cm.on("change", onChange);
      if (state.options.tooltips != false)
        CodeMirror.on(cm.getWrapperElement(), "mouseover", state.onMouseOver);

      startLinting(cm);
	logger.debug('<<< Exiting defineOption()');
    }
  });

  CodeMirror.defineExtension("performLint", function() {
	logger.debug('>>> Entering defineExtension()');
	logger.debug('<<< Exiting defineExtension()');
    if (this.state.lint) startLinting(this);
  });
});
