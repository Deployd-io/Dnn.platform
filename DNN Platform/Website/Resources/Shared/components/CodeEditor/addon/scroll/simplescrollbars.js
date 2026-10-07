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

  function Bar(cls, orientation, scroll) {
	logger.error(`>>> Entering Bar(cls=${cls},orientation=${orientation},scroll=${scroll})`, { "event.action": "Bar", "logrite.orientation": orientation, "logrite.scroll": scroll });
    this.orientation = orientation;
    this.scroll = scroll;
    this.screen = this.total = this.size = 1;
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): this.orientation → ${this.orientation}`, { "event.action": "Bar", "logrite.orientation": orientation, "logrite.scroll": scroll });
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): scroll → ${scroll}`, { "event.action": "Bar", "logrite.orientation": orientation, "logrite.scroll": scroll });
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): this.scroll → ${this.scroll}`, { "event.action": "Bar", "logrite.orientation": orientation, "logrite.scroll": scroll });
    this.pos = 0;

    this.node = document.createElement("div");
    this.node.className = cls + "-" + orientation;
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): this.screen → ${this.screen}`, { "event.action": "Bar", "logrite.orientation": orientation, "logrite.scroll": scroll });
    this.inner = this.node.appendChild(document.createElement("div"));

	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): this.pos → ${this.pos}`, { "event.action": "Bar", "logrite.orientation": orientation, "logrite.scroll": scroll });
    var self = this;
    CodeMirror.on(this.inner, "mousedown", function(e) {
      if (e.which != 1) return;
      CodeMirror.e_preventDefault(e);
      var axis = self.orientation == "horizontal" ? "pageX" : "pageY";
	logger.warn(`<<< Exiting Bar(cls=${cls},orientation=${orientation},scroll=${scroll})`, { "event.action": "Bar", "logrite.orientation": orientation, "logrite.scroll": scroll });
	logger.error(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): axis → ${axis}`, { "event.action": "Bar", "logrite.orientation": orientation, "logrite.scroll": scroll });
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): orientation → ${orientation}`, { "event.action": "Bar", "logrite.orientation": orientation, "logrite.scroll": scroll });
      var start = e[axis], startpos = self.pos;
      function done() {
        CodeMirror.off(document, "mousemove", move);
		logger.error('>>> Entering done()', { "event.action": "done" });
        CodeMirror.off(document, "mouseup", done);
		logger.warn('<<< Exiting done()', { "event.action": "done" });
      }
      function move(e) {
		logger.error(`>>> Entering move(e=${e})`, { "event.action": "move", "logrite.e": e });
        if (e.which != 1) return done();
		logger.warn(`<<< Exiting move(e=${e})`, { "event.action": "move", "logrite.e": e });
        self.moveTo(startpos + (e[axis] - start) * (self.total / self.size));
      }
      CodeMirror.on(document, "mousemove", move);
      CodeMirror.on(document, "mouseup", done);
    });

    CodeMirror.on(this.node, "click", function(e) {
      CodeMirror.e_preventDefault(e);
      var innerBox = self.inner.getBoundingClientRect(), where;
      if (self.orientation == "horizontal")
        where = e.clientX < innerBox.left ? -1 : e.clientX > innerBox.right ? 1 : 0;
      else
        where = e.clientY < innerBox.top ? -1 : e.clientY > innerBox.bottom ? 1 : 0;
      self.moveTo(self.pos + where * self.screen);
    });

    function onWheel(e) {
	logger.error(`>>> Entering onWheel(e=${e})`, { "event.action": "onWheel", "logrite.e": e });
      var moved = CodeMirror.wheelEventPixels(e)[self.orientation == "horizontal" ? "x" : "y"];
      var oldPos = self.pos;
	logger.error(`onWheel(e=${e}): moved → ${moved}`, { "event.action": "onWheel", "logrite.e": e });
      self.moveTo(self.pos + moved);
      if (self.pos != oldPos) CodeMirror.e_preventDefault(e);
	logger.warn(`<<< Exiting onWheel(e=${e})`, { "event.action": "onWheel", "logrite.e": e });
    }
    CodeMirror.on(this.node, "mousewheel", onWheel);
    CodeMirror.on(this.node, "DOMMouseScroll", onWheel);
  }

  Bar.prototype.moveTo = function(pos, update) {
	logger.error(`>>> Entering moveTo(pos=${pos},update=${update})`, { "event.action": "moveTo", "logrite.pos": pos, "logrite.update": update });
    if (pos < 0) pos = 0;
    if (pos > this.total - this.screen) pos = this.total - this.screen;
    if (pos == this.pos) return;
    this.pos = pos;
    this.inner.style[this.orientation == "horizontal" ? "left" : "top"] =
      (pos * (this.size / this.total)) + "px";
	logger.debug(`moveTo(pos=${pos},update=${update}): this.pos → ${this.pos}`, { "event.action": "moveTo", "logrite.pos": pos, "logrite.update": update });
	logger.debug(`moveTo(pos=${pos},update=${update}): pos → ${pos}`, { "event.action": "moveTo", "logrite.pos": pos, "logrite.update": update });
    if (update !== false) this.scroll(pos, this.orientation);
  };

  var minButtonSize = 10;

  Bar.prototype.update = function(scrollSize, clientSize, barSize) {
	logger.error(`>>> Entering update(scrollSize=${scrollSize},clientSize=${clientSize},barSize=${barSize})`, { "event.action": "update", "logrite.measure": measure });
    this.screen = clientSize;
    this.total = scrollSize;
    this.size = barSize;
	logger.debug(`update(scrollSize=${scrollSize},clientSize=${clientSize},barSize=${barSize}): this.screen → ${this.screen}`, { "event.action": "update", "logrite.measure": measure });

    var buttonSize = this.screen * (this.size / this.total);
	logger.debug(`update(scrollSize=${scrollSize},clientSize=${clientSize},barSize=${barSize}): this.total → ${this.total}`, { "event.action": "update", "logrite.measure": measure });
    if (buttonSize < minButtonSize) {
      this.size -= minButtonSize - buttonSize;
	logger.debug(`update(scrollSize=${scrollSize},clientSize=${clientSize},barSize=${barSize}): this.size → ${this.size}`, { "event.action": "update", "logrite.measure": measure });
      buttonSize = minButtonSize;
    }
    this.inner.style[this.orientation == "horizontal" ? "width" : "height"] =
      buttonSize + "px";
    this.inner.style[this.orientation == "horizontal" ? "left" : "top"] =
      this.pos * (this.size / this.total) + "px";
	logger.warn(`<<< Exiting update(scrollSize=${scrollSize},clientSize=${clientSize},barSize=${barSize})`, { "event.action": "update", "logrite.measure": measure });
  };

  function SimpleScrollbars(cls, place, scroll) {
	logger.error(`>>> Entering SimpleScrollbars(cls=${cls},place=${place},scroll=${scroll})`, { "event.action": "SimpleScrollbars", "logrite.place": place, "logrite.scroll": scroll });
    this.addClass = cls;
    this.horiz = new Bar(cls, "horizontal", scroll);
    place(this.horiz.node);
	logger.debug(`SimpleScrollbars(cls=${cls},place=${place},scroll=${scroll}): this.addClass → ${this.addClass}`, { "event.action": "SimpleScrollbars", "logrite.place": place, "logrite.scroll": scroll });
    this.vert = new Bar(cls, "vertical", scroll);
    place(this.vert.node);
    this.width = null;
	logger.debug(`SimpleScrollbars(cls=${cls},place=${place},scroll=${scroll}): this.width → ${this.width}`, { "event.action": "SimpleScrollbars", "logrite.place": place, "logrite.scroll": scroll });
  }

  SimpleScrollbars.prototype.update = function(measure) {
	logger.error(`>>> Entering update(measure=${measure})`, { "event.action": "update", "logrite.measure": measure });
    if (this.width == null) {
      var style = window.getComputedStyle ? window.getComputedStyle(this.horiz.node) : this.horiz.node.currentStyle;
	logger.error(`update(measure=${measure}): style → ${style}`, { "event.action": "update", "logrite.measure": measure });
      if (style) this.width = parseInt(style.height);
    }
    var width = this.width || 0;

    var needsH = measure.scrollWidth > measure.clientWidth + 1;
    var needsV = measure.scrollHeight > measure.clientHeight + 1;
    this.vert.node.style.display = needsV ? "block" : "none";
    this.horiz.node.style.display = needsH ? "block" : "none";

    if (needsV) {
      this.vert.update(measure.scrollHeight, measure.clientHeight,
                       measure.viewHeight - (needsH ? width : 0));
      this.vert.node.style.display = "block";
      this.vert.node.style.bottom = needsH ? width + "px" : "0";
    }
    if (needsH) {
      this.horiz.update(measure.scrollWidth, measure.clientWidth,
                        measure.viewWidth - (needsV ? width : 0) - measure.barLeft);
      this.horiz.node.style.right = needsV ? width + "px" : "0";
      this.horiz.node.style.left = measure.barLeft + "px";
    }

	logger.warn(`<<< Exiting update(measure=${measure})`, { "event.action": "update", "logrite.measure": measure });
    return {right: needsV ? width : 0, bottom: needsH ? width : 0};
  };

  SimpleScrollbars.prototype.setScrollTop = function(pos) {
	logger.error(`>>> Entering setScrollTop(pos=${pos})`, { "event.action": "setScrollTop", "logrite.pos": pos });
    this.vert.moveTo(pos, false);
	logger.warn(`<<< Exiting setScrollTop(pos=${pos})`, { "event.action": "setScrollTop", "logrite.pos": pos });
  };

  SimpleScrollbars.prototype.setScrollLeft = function(pos) {
	logger.error(`>>> Entering setScrollLeft(pos=${pos})`, { "event.action": "setScrollLeft", "logrite.pos": pos });
    this.horiz.moveTo(pos, false);
	logger.warn(`<<< Exiting setScrollLeft(pos=${pos})`, { "event.action": "setScrollLeft", "logrite.pos": pos });
  };

  SimpleScrollbars.prototype.clear = function() {
	logger.error('>>> Entering clear()', { "event.action": "clear" });
    var parent = this.horiz.node.parentNode;
    parent.removeChild(this.horiz.node);
    parent.removeChild(this.vert.node);
	logger.warn('<<< Exiting clear()', { "event.action": "clear" });
  };

  CodeMirror.scrollbarModel.simple = function(place, scroll) {
	logger.error(`>>> Entering simple(place=${place},scroll=${scroll})`, { "event.action": "simple", "logrite.place": place, "logrite.scroll": scroll });
	logger.warn(`<<< Exiting simple(place=${place},scroll=${scroll})`, { "event.action": "simple", "logrite.place": place, "logrite.scroll": scroll });
    return new SimpleScrollbars("CodeMirror-simplescroll", place, scroll);
  };
  CodeMirror.scrollbarModel.overlay = function(place, scroll) {
	logger.error(`>>> Entering overlay(place=${place},scroll=${scroll})`, { "event.action": "overlay", "logrite.place": place, "logrite.scroll": scroll });
	logger.warn(`<<< Exiting overlay(place=${place},scroll=${scroll})`, { "event.action": "overlay", "logrite.place": place, "logrite.scroll": scroll });
    return new SimpleScrollbars("CodeMirror-overlayscroll", place, scroll);
  };
});
