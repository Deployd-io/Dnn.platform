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

  CodeMirror.defineExtension("annotateScrollbar", function(options) {
    if (typeof options == "string") options = {className: options};
    return new Annotation(this, options);
  });

  CodeMirror.defineOption("scrollButtonHeight", 0);

  function Annotation(cm, options) {
	logger.error(`>>> Entering Annotation(cm=${cm},options=${options})`, { "event.action": "Annotation", "logrite.cm": cm, "logrite.options": options });
    this.cm = cm;
    this.options = options;
	logger.debug(`Annotation(cm=${cm},options=${options}): cm → ${cm}`, { "event.action": "Annotation", "logrite.cm": cm, "logrite.options": options });
	logger.debug(`Annotation(cm=${cm},options=${options}): this.cm → ${this.cm}`, { "event.action": "Annotation", "logrite.cm": cm, "logrite.options": options });
	logger.debug(`Annotation(cm=${cm},options=${options}): options → ${options}`, { "event.action": "Annotation", "logrite.cm": cm, "logrite.options": options });
	logger.debug(`Annotation(cm=${cm},options=${options}): this.options → ${this.options}`, { "event.action": "Annotation", "logrite.cm": cm, "logrite.options": options });
    this.buttonHeight = options.scrollButtonHeight || cm.getOption("scrollButtonHeight");
    this.annotations = [];
    this.doRedraw = this.doUpdate = null;
    this.div = cm.getWrapperElement().appendChild(document.createElement("div"));
    this.div.style.cssText = "position: absolute; right: 0; top: 0; z-index: 7; pointer-events: none";
    this.computeScale();
	logger.debug(`Annotation(cm=${cm},options=${options}): this.annotations → ${this.annotations}`, { "event.action": "Annotation", "logrite.cm": cm, "logrite.options": options });

	logger.warn(`<<< Exiting Annotation(cm=${cm},options=${options})`, { "event.action": "Annotation", "logrite.cm": cm, "logrite.options": options });
    function scheduleRedraw(delay) {
	logger.error(`>>> Entering scheduleRedraw(delay=${delay})`, { "event.action": "scheduleRedraw", "logrite.delay": delay });
      clearTimeout(self.doRedraw);
      self.doRedraw = setTimeout(function() { self.redraw(); }, delay);
	logger.warn(`<<< Exiting scheduleRedraw(delay=${delay})`, { "event.action": "scheduleRedraw", "logrite.delay": delay });
    }

    var self = this;
    cm.on("refresh", this.resizeHandler = function() {
	logger.error('>>> Entering resizeHandler()', { "event.action": "resizeHandler" });
      clearTimeout(self.doUpdate);
      self.doUpdate = setTimeout(function() {
        if (self.computeScale()) scheduleRedraw(20);
      }, 100);
    });
    cm.on("markerAdded", this.resizeHandler);
    cm.on("markerCleared", this.resizeHandler);
    if (options.listenForChanges !== false)
      cm.on("change", this.changeHandler = function() {
		logger.error('>>> Entering changeHandler()', { "event.action": "changeHandler" });
        scheduleRedraw(250);
      });
  }

  Annotation.prototype.computeScale = function() {
	logger.error('>>> Entering computeScale()', { "event.action": "computeScale" });
    var cm = this.cm;
    var hScale = (cm.getWrapperElement().clientHeight - cm.display.barHeight - this.buttonHeight * 2) /
      cm.heightAtLine(cm.lastLine() + 1, "local");
    if (hScale != this.hScale) {
      this.hScale = hScale;
	logger.debug(`computeScale(): hScale → ${hScale}`, { "event.action": "computeScale" });
      return true;
    }
  };

  Annotation.prototype.update = function(annotations) {
	logger.error(`>>> Entering update(annotations=${annotations})`, { "event.action": "update", "logrite.annotations": annotations });
    this.annotations = annotations;
    this.redraw();
	logger.warn(`<<< Exiting update(annotations=${annotations})`, { "event.action": "update", "logrite.annotations": annotations });
  };

  Annotation.prototype.redraw = function(compute) {
	logger.error(`>>> Entering redraw(compute=${compute})`, { "event.action": "redraw", "logrite.compute": compute });
    if (compute !== false) this.computeScale();
    var cm = this.cm, hScale = this.hScale;

    var frag = document.createDocumentFragment(), anns = this.annotations;
	logger.error(`changeHandler(): frag → ${frag}`, { "event.action": "changeHandler" });

	logger.error(`redraw(compute=${compute}): frag → ${frag}`, { "event.action": "redraw", "logrite.compute": compute });
    var wrapping = cm.getOption("lineWrapping");
    var singleLineH = wrapping && cm.defaultTextHeight() * 1.5;
	logger.error(`redraw(compute=${compute}): singleLineH → ${singleLineH}`, { "event.action": "redraw", "logrite.compute": compute });
    var curLine = null, curLineObj = null;
    function getY(pos, top) {
	logger.error(`changeHandler(): singleLineH → ${singleLineH}`, { "event.action": "changeHandler" });
      if (curLine != pos.line) {
		logger.error(`>>> Entering getY(pos=${pos},top=${top})`, { "event.action": "getY", "logrite.pos": pos, "logrite.top": top });
        curLine = pos.line;
        curLineObj = cm.getLineHandle(curLine);
      }
      if (wrapping && curLineObj.height > singleLineH)
        return cm.charCoords(pos, "local")[top ? "top" : "bottom"];
      var topY = cm.heightAtLine(curLineObj, "local");
	logger.error(`getY(pos=${pos},top=${top}): topY → ${topY}`, { "event.action": "getY", "logrite.pos": pos, "logrite.top": top });
      return topY + (top ? 0 : curLineObj.height);
    }

    if (cm.display.barWidth) for (var i = 0, nextTop; i < anns.length; i++) {
      var ann = anns[i];
      var top = nextTop || getY(ann.from, true) * hScale;
	logger.error(`redraw(compute=${compute}): top → ${top}`, { "event.action": "redraw", "logrite.compute": compute });
      var bottom = getY(ann.to, false) * hScale;
	logger.error(`redraw(compute=${compute}): bottom → ${bottom}`, { "event.action": "redraw", "logrite.compute": compute });
      while (i < anns.length - 1) {
        nextTop = getY(anns[i + 1].from, true) * hScale;
        if (nextTop > bottom + .9) break;
        ann = anns[++i];
        bottom = getY(ann.to, false) * hScale;
      }
      if (bottom == top) continue;
      var height = Math.max(bottom - top, 3);
	logger.error(`redraw(compute=${compute}): height → ${height}`, { "event.action": "redraw", "logrite.compute": compute });

      var elt = frag.appendChild(document.createElement("div"));
	logger.error(`redraw(compute=${compute}): elt → ${elt}`, { "event.action": "redraw", "logrite.compute": compute });
      elt.style.cssText = "position: absolute; right: 0px; width: " + Math.max(cm.display.barWidth - 1, 2) + "px; top: "
        + (top + this.buttonHeight) + "px; height: " + height + "px";
      elt.className = this.options.className;
		logger.debug(`redraw(compute=${compute}): elt.className → ${elt.className}`, { "event.action": "redraw", "logrite.compute": compute });
    }
    this.div.textContent = "";
    this.div.appendChild(frag);
	logger.warn(`<<< Exiting redraw(compute=${compute})`, { "event.action": "redraw", "logrite.compute": compute });
  };

  Annotation.prototype.clear = function() {
	logger.error('>>> Entering clear()', { "event.action": "clear" });
    this.cm.off("refresh", this.resizeHandler);
    this.cm.off("markerAdded", this.resizeHandler);
    this.cm.off("markerCleared", this.resizeHandler);
    if (this.changeHandler) this.cm.off("change", this.changeHandler);
    this.div.parentNode.removeChild(this.div);
	logger.warn('<<< Exiting clear()', { "event.action": "clear" });
  };
});
