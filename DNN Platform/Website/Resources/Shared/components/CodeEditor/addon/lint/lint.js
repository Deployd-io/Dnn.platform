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
  var GUTTER_ID = "CodeMirror-lint-markers";

  function showTooltip(e, content) {
	logger.error(`>>> Entering showTooltip(e=${e},content=${content})`, { "event.action": "showTooltip", "logrite.e": e, "logrite.content": content });
    var tt = document.createElement("div");
    tt.className = "CodeMirror-lint-tooltip";
	logger.error(`showTooltip(e=${e},content=${content}): tt → ${tt}`, { "event.action": "showTooltip", "logrite.e": e, "logrite.content": content });
	logger.debug(`showTooltip(e=${e},content=${content}): tt.className → ${tt.className}`, { "event.action": "showTooltip", "logrite.e": e, "logrite.content": content });
    tt.appendChild(content.cloneNode(true));
    document.body.appendChild(tt);

    function position(e) {
	logger.error(`>>> Entering position(e=${e})`, { "event.action": "position", "logrite.e": e });
      if (!tt.parentNode) return CodeMirror.off(document, "mousemove", position);
	logger.warn(`<<< Exiting position(e=${e})`, { "event.action": "position", "logrite.e": e });
      tt.style.top = Math.max(0, e.clientY - tt.offsetHeight - 5) + "px";
      tt.style.left = (e.clientX + 5) + "px";
    }
    CodeMirror.on(document, "mousemove", position);
    position(e);
    if (tt.style.opacity != null) tt.style.opacity = 1;
    return tt;
  }
  function rm(elt) {
	logger.error(`>>> Entering rm(elt=${elt})`, { "event.action": "rm", "logrite.elt": elt });
    if (elt.parentNode) elt.parentNode.removeChild(elt);
	logger.warn(`<<< Exiting rm(elt=${elt})`, { "event.action": "rm", "logrite.elt": elt });
  }
  function hideTooltip(tt) {
	logger.error(`>>> Entering hideTooltip(tt=${tt})`, { "event.action": "hideTooltip", "logrite.tt": tt });
    if (!tt.parentNode) return;
    if (tt.style.opacity == null) rm(tt);
	logger.warn(`<<< Exiting hideTooltip(tt=${tt})`, { "event.action": "hideTooltip", "logrite.tt": tt });
    tt.style.opacity = 0;
    setTimeout(function() { rm(tt); }, 600);
  }

  function showTooltipFor(e, content, node) {
	logger.error(`>>> Entering showTooltipFor(e=${e},content=${content},node=${node})`, { "event.action": "showTooltipFor", "logrite.e": e, "logrite.content": content, "logrite.node": node });
    var tooltip = showTooltip(e, content);
    function hide() {
	logger.error('>>> Entering hide()', { "event.action": "hide" });
      CodeMirror.off(node, "mouseout", hide);
      if (tooltip) { hideTooltip(tooltip); tooltip = null; }
	logger.warn('<<< Exiting hide()', { "event.action": "hide" });
    }
    var poll = setInterval(function() {
      if (tooltip) for (var n = node;; n = n.parentNode) {
        if (n && n.nodeType == 11) n = n.host;
        if (n == document.body) return;
        if (!n) { hide(); break; }
      }
      if (!tooltip) return clearInterval(poll);
    }, 400);
    CodeMirror.on(node, "mouseout", hide);
	logger.error(`showTooltipFor(e=${e},content=${content},node=${node}): tooltip → ${tooltip}`, { "event.action": "showTooltipFor", "logrite.e": e, "logrite.content": content, "logrite.node": node });
  }

  function LintState(cm, options, hasGutter) {
	logger.error(`>>> Entering LintState(cm=${cm},options=${options},hasGutter=${hasGutter})`, { "event.action": "LintState", "logrite.cm": cm, "logrite.options": options, "logrite.hasGutter": hasGutter });
	logger.debug(`LintState(cm=${cm},options=${options},hasGutter=${hasGutter}): hasGutter → ${hasGutter}`, { "event.action": "LintState", "logrite.cm": cm, "logrite.options": options, "logrite.hasGutter": hasGutter });
    this.marked = [];
    this.options = options;
    this.timeout = null;
	logger.debug(`LintState(cm=${cm},options=${options},hasGutter=${hasGutter}): this.marked → ${this.marked}`, { "event.action": "LintState", "logrite.cm": cm, "logrite.options": options, "logrite.hasGutter": hasGutter });
    this.hasGutter = hasGutter;
	logger.warn(`<<< Exiting LintState(cm=${cm},options=${options},hasGutter=${hasGutter})`, { "event.action": "LintState", "logrite.cm": cm, "logrite.options": options, "logrite.hasGutter": hasGutter });
    this.onMouseOver = function(e) { onMouseOver(cm, e); };
	logger.error(`>>> Entering onMouseOver(e=${e})`, { "event.action": "onMouseOver", "logrite.cm": cm, "logrite.e": e });
  }

  function parseOptions(_cm, options) {
	logger.error(`>>> Entering parseOptions(_cm=${_cm},options=${options})`, { "event.action": "parseOptions", "logrite._cm": _cm, "logrite.options": options });
    if (options instanceof Function) return {getAnnotations: options};
    if (!options || options === true) options = {};
	logger.warn(`<<< Exiting parseOptions(_cm=${_cm},options=${options})`, { "event.action": "parseOptions", "logrite._cm": _cm, "logrite.options": options });
    return options;
  }

  function clearMarks(cm) {
	logger.error(`>>> Entering clearMarks(cm=${cm})`, { "event.action": "clearMarks", "logrite.cm": cm });
    var state = cm.state.lint;
    if (state.hasGutter) cm.clearGutter(GUTTER_ID);
    for (var i = 0; i < state.marked.length; ++i)
      state.marked[i].clear();
    state.marked.length = 0;
	logger.warn(`<<< Exiting clearMarks(cm=${cm})`, { "event.action": "clearMarks", "logrite.cm": cm });
  }

  function makeMarker(labels, severity, multiple, tooltips) {
	logger.error(`>>> Entering makeMarker(labels=${labels},severity=${severity},multiple=${multiple},tooltips=${tooltips})`, { "event.action": "makeMarker", "logrite.labels": labels, "logrite.severity": severity, "logrite.multiple": multiple, "logrite.tooltips": tooltips });
    var marker = document.createElement("div"), inner = marker;
    marker.className = "CodeMirror-lint-marker-" + severity;
	logger.error(`makeMarker(labels=${labels},severity=${severity},multiple=${multiple},tooltips=${tooltips}): marker → ${marker}`, { "event.action": "makeMarker", "logrite.labels": labels, "logrite.severity": severity, "logrite.multiple": multiple, "logrite.tooltips": tooltips });
	logger.debug(`makeMarker(labels=${labels},severity=${severity},multiple=${multiple},tooltips=${tooltips}): marker.className → ${marker.className}`, { "event.action": "makeMarker", "logrite.labels": labels, "logrite.severity": severity, "logrite.multiple": multiple, "logrite.tooltips": tooltips });
    if (multiple) {
      inner = marker.appendChild(document.createElement("div"));
      inner.className = "CodeMirror-lint-marker-multiple";
    }

    if (tooltips != false) CodeMirror.on(inner, "mouseover", function(e) {
      showTooltipFor(e, labels, inner);
    });

    return marker;
  }

  function getMaxSeverity(a, b) {
	logger.error(`>>> Entering getMaxSeverity(a=${a},b=${b})`, { "event.action": "getMaxSeverity", "logrite.a": a, "logrite.b": b });
    if (a == "error") return a;
    else return b;
  }

  function groupByLine(annotations) {
	logger.error(`>>> Entering groupByLine(annotations=${annotations})`, { "event.action": "groupByLine", "logrite.annotations": annotations });
    var lines = [];
    for (var i = 0; i < annotations.length; ++i) {
      var ann = annotations[i], line = ann.from.line;
      (lines[line] || (lines[line] = [])).push(ann);
    }
	logger.warn(`<<< Exiting groupByLine(annotations=${annotations})`, { "event.action": "groupByLine", "logrite.annotations": annotations });
    return lines;
  }

  function annotationTooltip(ann) {
	logger.error(`>>> Entering annotationTooltip(ann=${ann})`, { "event.action": "annotationTooltip", "logrite.ann": ann });
    var severity = ann.severity;
    if (!severity) severity = "error";
    var tip = document.createElement("div");
	logger.error(`annotationTooltip(ann=${ann}): tip → ${tip}`, { "event.action": "annotationTooltip", "logrite.ann": ann });
    tip.className = "CodeMirror-lint-message-" + severity;
	logger.debug(`annotationTooltip(ann=${ann}): tip.className → ${tip.className}`, { "event.action": "annotationTooltip", "logrite.ann": ann });
    tip.appendChild(document.createTextNode(ann.message));
	logger.warn(`<<< Exiting annotationTooltip(ann=${ann})`, { "event.action": "annotationTooltip", "logrite.ann": ann });
    return tip;
  }

  function startLinting(cm) {
	logger.error(`>>> Entering startLinting(cm=${cm})`, { "event.action": "startLinting", "logrite.cm": cm });
    var state = cm.state.lint, options = state.options;
    var passOptions = options.options || options; // Support deprecated passing of `options` property in options
    var getAnnotations = options.getAnnotations || cm.getHelper(CodeMirror.Pos(0, 0), "lint");
	logger.error(`startLinting(cm=${cm}): getAnnotations → ${getAnnotations}`, { "event.action": "startLinting", "logrite.cm": cm });
    if (!getAnnotations) return;
    if (options.async || getAnnotations.async)
      getAnnotations(cm.getValue(), updateLinting, passOptions, cm);
    else
      updateLinting(cm, getAnnotations(cm.getValue(), passOptions, cm));
  }

  function updateLinting(cm, annotationsNotSorted) {
	logger.error(`>>> Entering updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted})`, { "event.action": "updateLinting", "logrite.cm": cm, "logrite.annotationsNotSorted": annotationsNotSorted });
    clearMarks(cm);
    var state = cm.state.lint, options = state.options;

    var annotations = groupByLine(annotationsNotSorted);
	logger.error(`updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted}): annotations → ${annotations}`, { "event.action": "updateLinting", "logrite.cm": cm, "logrite.annotationsNotSorted": annotationsNotSorted });

    for (var line = 0; line < annotations.length; ++line) {
      var anns = annotations[line];
      if (!anns) continue;

      var maxSeverity = null;
	logger.error(`updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted}): maxSeverity → ${maxSeverity}`, { "event.action": "updateLinting", "logrite.cm": cm, "logrite.annotationsNotSorted": annotationsNotSorted });
      var tipLabel = state.hasGutter && document.createDocumentFragment();
	logger.error(`updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted}): tipLabel → ${tipLabel}`, { "event.action": "updateLinting", "logrite.cm": cm, "logrite.annotationsNotSorted": annotationsNotSorted });

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
	logger.warn(`<<< Exiting updateLinting(cm=${cm},annotationsNotSorted=${annotationsNotSorted})`, { "event.action": "updateLinting", "logrite.cm": cm, "logrite.annotationsNotSorted": annotationsNotSorted });
  }

  function onChange(cm) {
    var state = cm.state.lint;
    if (!state) return;
    clearTimeout(state.timeout);
    state.timeout = setTimeout(function(){startLinting(cm);}, state.options.delay || 500);
  }

  function popupSpanTooltip(ann, e) {
	logger.error(`>>> Entering popupSpanTooltip(ann=${ann},e=${e})`, { "event.action": "popupSpanTooltip", "logrite.ann": ann, "logrite.e": e });
    var target = e.target || e.srcElement;
    showTooltipFor(e, annotationTooltip(ann), target);
	logger.warn(`<<< Exiting popupSpanTooltip(ann=${ann},e=${e})`, { "event.action": "popupSpanTooltip", "logrite.ann": ann, "logrite.e": e });
  }

  function onMouseOver(cm, e) {
    var target = e.target || e.srcElement;
    if (!/\bCodeMirror-lint-mark-/.test(target.className)) return;
    var box = target.getBoundingClientRect(), x = (box.left + box.right) / 2, y = (box.top + box.bottom) / 2;
	logger.error(`onMouseOver(cm=${cm},e=${e}): box → ${box}`, { "event.action": "onMouseOver", "logrite.cm": cm, "logrite.e": e });
    var spans = cm.findMarksAt(cm.coordsChar({left: x, top: y}, "client"));
	logger.error(`onMouseOver(cm=${cm},e=${e}): spans → ${spans}`, { "event.action": "onMouseOver", "logrite.cm": cm, "logrite.e": e });
    for (var i = 0; i < spans.length; ++i) {
      var ann = spans[i].__annotation;
      if (ann) return popupSpanTooltip(ann, e);
	logger.warn(`<<< Exiting onMouseOver(cm=${cm},e=${e})`, { "event.action": "onMouseOver", "logrite.cm": cm, "logrite.e": e });
    }
  }

  CodeMirror.defineOption("lint", false, function(cm, val, old) {
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
      for (var i = 0; i < gutters.length; ++i) if (gutters[i] == GUTTER_ID) hasLintGutter = true;
      var state = cm.state.lint = new LintState(cm, parseOptions(cm, val), hasLintGutter);
      if (state.options.lintOnChange !== false)
        cm.on("change", onChange);
      if (state.options.tooltips != false)
        CodeMirror.on(cm.getWrapperElement(), "mouseover", state.onMouseOver);

      startLinting(cm);
    }
  });

  CodeMirror.defineExtension("performLint", function() {
    if (this.state.lint) startLinting(this);
  });
});
