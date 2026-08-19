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

  function Bar(cls, orientation, scroll) {
	logger.debug(`>>> Entering Bar(cls=${cls},orientation=${orientation},scroll=${scroll})`);
    this.orientation = orientation;
    this.scroll = scroll;
    this.screen = this.total = this.size = 1;
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): this.orientation → ${this.orientation}`);
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): scroll → ${scroll}`);
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): this.scroll → ${this.scroll}`);
    this.pos = 0;

    this.node = document.createElement("div");
    this.node.className = cls + "-" + orientation;
    this.inner = this.node.appendChild(document.createElement("div"));
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): this.screen → ${this.screen}`);
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): orientation → ${orientation}`);

    var self = this;
	logger.debug(`<<< Exiting Bar(cls=${cls},orientation=${orientation},scroll=${scroll})`);
    CodeMirror.on(this.inner, "mousedown", function(e) {
	logger.debug('>>> Entering on()');
      if (e.which != 1) return;
	logger.debug(`Bar(cls=${cls},orientation=${orientation},scroll=${scroll}): this.pos → ${this.pos}`);
      CodeMirror.e_preventDefault(e);
      var axis = self.orientation == "horizontal" ? "pageX" : "pageY";
	logger.debug('<<< Exiting on()');
      var start = e[axis], startpos = self.pos;
      function done() {
		logger.debug('>>> Entering done()');
        CodeMirror.off(document, "mousemove", move);
        CodeMirror.off(document, "mouseup", done);
		logger.debug('<<< Exiting done()');
      }
      function move(e) {
		logger.debug(`>>> Entering move(e=${e})`);
        if (e.which != 1) return done();
		logger.debug(`<<< Exiting move(e=${e})`);
        self.moveTo(startpos + (e[axis] - start) * (self.total / self.size));
      }
      CodeMirror.on(document, "mousemove", move);
      CodeMirror.on(document, "mouseup", done);
    });

    CodeMirror.on(this.node, "click", function(e) {
      CodeMirror.e_preventDefault(e);
      var innerBox = self.inner.getBoundingClientRect(), where;
	logger.debug(`on(innerBox=${innerBox}): innerBox → ${innerBox}`);
      if (self.orientation == "horizontal")
        where = e.clientX < innerBox.left ? -1 : e.clientX > innerBox.right ? 1 : 0;
      else
        where = e.clientY < innerBox.top ? -1 : e.clientY > innerBox.bottom ? 1 : 0;
      self.moveTo(self.pos + where * self.screen);
    });
	logger.debug(`on(innerBox=${innerBox}): where → ${where}`);

    function onWheel(e) {
	logger.debug(`>>> Entering onWheel(e=${e})`);
      var moved = CodeMirror.wheelEventPixels(e)[self.orientation == "horizontal" ? "x" : "y"];
      var oldPos = self.pos;
	logger.debug(`onWheel(e=${e}): moved → ${moved}`);
      self.moveTo(self.pos + moved);
      if (self.pos != oldPos) CodeMirror.e_preventDefault(e);
	logger.debug(`<<< Exiting onWheel(e=${e})`);
    }
    CodeMirror.on(this.node, "mousewheel", onWheel);
    CodeMirror.on(this.node, "DOMMouseScroll", onWheel);
  }

  Bar.prototype.moveTo = function(pos, update) {
    if (pos < 0) pos = 0;
    if (pos > this.total - this.screen) pos = this.total - this.screen;
    if (pos == this.pos) return;
    this.pos = pos;
    this.inner.style[this.orientation == "horizontal" ? "left" : "top"] =
      (pos * (this.size / this.total)) + "px";
	logger.debug(`function(pos=${pos},update=${update}): this.pos → ${this.pos}`);
    if (update !== false) this.scroll(pos, this.orientation);
  };

  var minButtonSize = 10;

  Bar.prototype.update = function(scrollSize, clientSize, barSize) {
    this.screen = clientSize;
    this.total = scrollSize;
    this.size = barSize;

	logger.debug(`function(scrollSize=${scrollSize},clientSize=${clientSize},barSize=${barSize}): this.screen → ${this.screen}`);
    var buttonSize = this.screen * (this.size / this.total);
    if (buttonSize < minButtonSize) {
	logger.debug(`function(scrollSize=${scrollSize},clientSize=${clientSize},barSize=${barSize}): this.total → ${this.total}`);
      this.size -= minButtonSize - buttonSize;
      buttonSize = minButtonSize;
	logger.debug(`function(scrollSize=${scrollSize},clientSize=${clientSize},barSize=${barSize}): this.size → ${this.size}`);
    }
    this.inner.style[this.orientation == "horizontal" ? "width" : "height"] =
      buttonSize + "px";
    this.inner.style[this.orientation == "horizontal" ? "left" : "top"] =
      this.pos * (this.size / this.total) + "px";
  };

  function SimpleScrollbars(cls, place, scroll) {
	logger.debug(`>>> Entering SimpleScrollbars(cls=${cls},place=${place},scroll=${scroll})`);
    this.addClass = cls;
    this.horiz = new Bar(cls, "horizontal", scroll);
    place(this.horiz.node);
    this.vert = new Bar(cls, "vertical", scroll);
	logger.debug(`SimpleScrollbars(cls=${cls},place=${place},scroll=${scroll}): this.addClass → ${this.addClass}`);
    place(this.vert.node);
    this.width = null;
	logger.debug(`SimpleScrollbars(cls=${cls},place=${place},scroll=${scroll}): this.width → ${this.width}`);
	logger.debug(`<<< Exiting SimpleScrollbars(cls=${cls},place=${place},scroll=${scroll})`);
  }

  SimpleScrollbars.prototype.update = function(measure) {
    if (this.width == null) {
      var style = window.getComputedStyle ? window.getComputedStyle(this.horiz.node) : this.horiz.node.currentStyle;
      if (style) this.width = parseInt(style.height);
    }
    var width = this.width || 0;

    var needsH = measure.scrollWidth > measure.clientWidth + 1;
	logger.debug(`function(measure=${measure}): style → ${style}`);
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

	logger.debug(`<<< Exiting function(measure=${measure})`);
    return {right: needsV ? width : 0, bottom: needsH ? width : 0};
  };

  SimpleScrollbars.prototype.setScrollTop = function(pos) {
    this.vert.moveTo(pos, false);
  };

  SimpleScrollbars.prototype.setScrollLeft = function(pos) {
    this.horiz.moveTo(pos, false);
  };

  SimpleScrollbars.prototype.clear = function() {
    var parent = this.horiz.node.parentNode;
    parent.removeChild(this.horiz.node);
    parent.removeChild(this.vert.node);
  };

  CodeMirror.scrollbarModel.simple = function(place, scroll) {
    return new SimpleScrollbars("CodeMirror-simplescroll", place, scroll);
  };
  CodeMirror.scrollbarModel.overlay = function(place, scroll) {
    return new SimpleScrollbars("CodeMirror-overlayscroll", place, scroll);
  };
});
