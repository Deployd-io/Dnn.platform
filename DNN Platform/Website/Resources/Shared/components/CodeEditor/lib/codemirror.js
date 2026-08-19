const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// This is CodeMirror (http://codemirror.net), a code editor
// implemented in JavaScript on top of the browser's DOM.
//
// You can find some technical background for some of the code below
// at http://marijnhaverbeke.nl/blog/#cm-internals .

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    module.exports = mod();
  else if (typeof define == "function" && define.amd) // AMD
    return define([], mod);
  else // Plain browser env
    this.CodeMirror = mod();
})(function() {
  "use strict";

  // BROWSER SNIFFING

  // Kludges for bugs and behavior differences that can't be feature
  // detected are enabled based on userAgent etc sniffing.

  var gecko = /gecko\/\d/i.test(navigator.userAgent);
  var ie_upto10 = /MSIE \d/.test(navigator.userAgent);
  var ie_11up = /Trident\/(?:[7-9]|\d{2,})\..*rv:(\d+)/.exec(navigator.userAgent);
  var ie = ie_upto10 || ie_11up;
  var ie_version = ie && (ie_upto10 ? document.documentMode || 6 : ie_11up[1]);
  var webkit = /WebKit\//.test(navigator.userAgent);
  var qtwebkit = webkit && /Qt\/\d+\.\d+/.test(navigator.userAgent);
  var chrome = /Chrome\//.test(navigator.userAgent);
  var presto = /Opera\//.test(navigator.userAgent);
  var safari = /Apple Computer/.test(navigator.vendor);
  var mac_geMountainLion = /Mac OS X 1\d\D([8-9]|\d\d)\D/.test(navigator.userAgent);
  var phantom = /PhantomJS/.test(navigator.userAgent);

  var ios = /AppleWebKit/.test(navigator.userAgent) && /Mobile\/\w+/.test(navigator.userAgent);
  // This is woefully incomplete. Suggestions for alternative methods welcome.
  var mobile = ios || /Android|webOS|BlackBerry|Opera Mini|Opera Mobi|IEMobile/i.test(navigator.userAgent);
  var mac = ios || /Mac/.test(navigator.platform);
  var windows = /win/i.test(navigator.platform);

  var presto_version = presto && navigator.userAgent.match(/Version\/(\d*\.\d*)/);
  if (presto_version) presto_version = Number(presto_version[1]);
  if (presto_version && presto_version >= 15) { presto = false; webkit = true; }
  // Some browsers use the wrong event properties to signal cmd/ctrl on OS X
  var flipCtrlCmd = mac && (qtwebkit || presto && (presto_version == null || presto_version < 12.11));
  var captureRightClick = gecko || (ie && ie_version >= 9);

  // Optimize some code when these features are not used.
  var sawReadOnlySpans = false, sawCollapsedSpans = false;

  // EDITOR CONSTRUCTOR

  // A CodeMirror instance represents an editor. This is the object
  // that user code is usually dealing with.

logger.debug(`function(string=${string},tabSize=${tabSize}): this.string → ${this.string}`);
logger.debug(`function(doc=${doc},node=${node},options=${options}): this.doc → ${this.doc}`);
  function CodeMirror(place, options) {
    if (!(this instanceof CodeMirror)) return new CodeMirror(place, options);
	logger.debug(`>>> Entering CodeMirror(place=${place},options=${options})`);

	logger.debug(`<<< Exiting CodeMirror(place=${place},options=${options})`);
    this.options = options = options ? copyObj(options) : {};
    // Determine effective options based on given values and defaults.
    copyObj(defaults, options, false);
	logger.debug(`CodeMirror(place=${place},options=${options}): options → ${options}`);
    setGuttersForLineNumbers(options);

    var doc = options.value;
    if (typeof doc == "string") doc = new Doc(doc, options.mode, null, options.lineSeparator);
    this.doc = doc;

	logger.debug(`CodeMirror(place=${place},options=${options}): this.doc → ${this.doc}`);
    var input = new CodeMirror.inputStyles[options.inputStyle](this);
	logger.debug(`CodeMirror(place=${place},options=${options}): input → ${input}`);
    var display = this.display = new Display(place, doc, input);
	logger.debug(`CodeMirror(place=${place},options=${options}): display → ${display}`);
    display.wrapper.CodeMirror = this;
    updateGutters(this);
    themeChanged(this);
    if (options.lineWrapping)
      this.display.wrapper.className += " CodeMirror-wrap";
    if (options.autofocus && !mobile) display.input.focus();
    initScrollbars(this);

    this.state = {
      keyMaps: [],  // stores maps added by addKeyMap
      overlays: [], // highlighting overlays, as added by addOverlay
      modeGen: 0,   // bumped when mode/overlay changes, used to invalidate highlighting info
      overwrite: false,
      delayingBlurEvent: false,
      focused: false,
      suppressEdits: false, // used to disable editing during key handlers when in readOnly mode
      pasteIncoming: false, cutIncoming: false, // help recognize paste/cut edits in input.poll
      selectingText: false,
      draggingText: false,
      highlight: new Delayed(), // stores highlight worker timeout
      keySeq: null,  // Unfinished key sequence
      specialChars: null
    };

	logger.debug(`CodeMirror(place=${place},options=${options}): this.state → ${this.state}`);
    var cm = this;

    // Override magic textarea content restore that IE sometimes does
    // on our hidden textarea on reload
    if (ie && ie_version < 11) setTimeout(function() { cm.display.input.reset(true); }, 20);

    registerEventHandlers(this);
    ensureGlobalHandlers();

    startOperation(this);
    this.curOp.forceUpdate = true;
    attachDoc(this, doc);

    if ((options.autofocus && !mobile) || cm.hasFocus())
      setTimeout(bind(onFocus, this), 20);
    else
      onBlur(this);

    for (var opt in optionHandlers) if (optionHandlers.hasOwnProperty(opt))
      optionHandlers[opt](this, options[opt], Init);
    maybeUpdateLineNumberWidth(this);
    if (options.finishInit) options.finishInit(this);
    for (var i = 0; i < initHooks.length; ++i) initHooks[i](this);
    endOperation(this);
    // Suppress optimizelegibility in Webkit, since it breaks text
    // measuring on line wrapping boundaries.
    if (webkit && options.lineWrapping &&
        getComputedStyle(display.lineDiv).textRendering == "optimizelegibility")
      display.lineDiv.style.textRendering = "auto";
  }

  // DISPLAY CONSTRUCTOR

  // The display handles the DOM integration, both for input reading
  // and content drawing. It holds references to DOM nodes and
  // display-related state.

  function Display(place, doc, input) {
	logger.debug(`>>> Entering Display(place=${place},doc=${doc},input=${input})`);
    var d = this;
    this.input = input;

	logger.debug(`Display(place=${place},doc=${doc},input=${input}): input → ${input}`);
    // Covers bottom-right square when both scrollbars are present.
    d.scrollbarFiller = elt("div", null, "CodeMirror-scrollbar-filler");
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): this.input → ${this.input}`);
    d.scrollbarFiller.setAttribute("cm-not-content", "true");
    // Covers bottom of gutter when coverGutterNextToScrollbar is on
    // and h scrollbar is present.
    d.gutterFiller = elt("div", null, "CodeMirror-gutter-filler");
    d.gutterFiller.setAttribute("cm-not-content", "true");
    // Will contain the actual code, positioned to cover the viewport.
    d.lineDiv = elt("div", null, "CodeMirror-code");
    // Elements are added to these to represent selection and cursors.
    d.selectionDiv = elt("div", null, null, "position: relative; z-index: 1");
    d.cursorDiv = elt("div", null, "CodeMirror-cursors");
    // A visibility: hidden element used to find the size of things.
    d.measure = elt("div", null, "CodeMirror-measure");
    // When lines outside of the viewport are measured, they are drawn in this.
    d.lineMeasure = elt("div", null, "CodeMirror-measure");
    // Wraps everything that needs to exist inside the vertically-padded coordinate system
    d.lineSpace = elt("div", [d.measure, d.lineMeasure, d.selectionDiv, d.cursorDiv, d.lineDiv],
                      null, "position: relative; outline: none");
    // Moved around its parent to cover visible view.
    d.mover = elt("div", [elt("div", [d.lineSpace], "CodeMirror-lines")], null, "position: relative");
    // Set to the height of the document, allowing scrolling.
    d.sizer = elt("div", [d.mover], "CodeMirror-sizer");
    d.sizerWidth = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.sizerWidth → ${d.sizerWidth}`);
    // Behavior of elts with overflow: auto and padding is
    // inconsistent across browsers. This is used to ensure the
    // scrollable area is big enough.
    d.heightForcer = elt("div", null, null, "position: absolute; height: " + scrollerGap + "px; width: 1px;");
    // Will contain the gutters, if any.
    d.gutters = elt("div", null, "CodeMirror-gutters");
    d.lineGutter = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.lineGutter → ${d.lineGutter}`);
    // Actual scrollable element.
    d.scroller = elt("div", [d.sizer, d.heightForcer, d.gutters], "CodeMirror-scroll");
    d.scroller.setAttribute("tabIndex", "-1");
    // The element in which the editor lives.
    d.wrapper = elt("div", [d.scrollbarFiller, d.gutterFiller, d.scroller], "CodeMirror");

    // Work around IE7 z-index bug (not perfect, hence IE7 not really being supported)
    if (ie && ie_version < 8) { d.gutters.style.zIndex = -1; d.scroller.style.paddingRight = 0; }
    if (!webkit && !(gecko && mobile)) d.scroller.draggable = true;

    if (place) {
      if (place.appendChild) place.appendChild(d.wrapper);
      else place(d.wrapper);
    }

    // Current rendered range (may be bigger than the view window).
    d.viewFrom = d.viewTo = doc.first;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.viewFrom → ${d.viewFrom}`);
    d.reportedViewFrom = d.reportedViewTo = doc.first;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.reportedViewFrom → ${d.reportedViewFrom}`);
    // Information about the rendered lines.
    d.view = [];
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.view → ${d.view}`);
    d.renderedView = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.renderedView → ${d.renderedView}`);
    // Holds info about a single rendered line when it was rendered
    // for measurement, while not in view.
    d.externalMeasured = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.externalMeasured → ${d.externalMeasured}`);
    // Empty space (in pixels) above the view
    d.viewOffset = 0;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.viewOffset → ${d.viewOffset}`);
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.updateLineNumbers → ${d.updateLineNumbers}`);
    d.lastWrapHeight = d.lastWrapWidth = 0;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.lastWrapHeight → ${d.lastWrapHeight}`);
    d.updateLineNumbers = null;

    d.nativeBarWidth = d.barHeight = d.barWidth = 0;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.nativeBarWidth → ${d.nativeBarWidth}`);
    d.scrollbarsClipped = false;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.scrollbarsClipped → ${d.scrollbarsClipped}`);

    // Used to only resize the line number gutter when necessary (when
    // the amount of lines crosses a boundary that makes its width change)
    d.lineNumWidth = d.lineNumInnerWidth = d.lineNumChars = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.lineNumWidth → ${d.lineNumWidth}`);
    // Set to true when a non-horizontal-scrolling line widget is
    // added. As an optimization, line widget aligning is skipped when
    // this is false.
    d.alignWidgets = false;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.alignWidgets → ${d.alignWidgets}`);

    d.cachedCharWidth = d.cachedTextHeight = d.cachedPaddingH = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.cachedCharWidth → ${d.cachedCharWidth}`);

    // Tracks the maximum line length so that the horizontal scrollbar
    // can be kept static when scrolling.
    d.maxLine = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.maxLine → ${d.maxLine}`);
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.maxLineChanged → ${d.maxLineChanged}`);
    d.maxLineLength = 0;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.maxLineLength → ${d.maxLineLength}`);
    d.maxLineChanged = false;

    // Used for measuring wheel scrolling granularity
    d.wheelDX = d.wheelDY = d.wheelStartX = d.wheelStartY = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.wheelDX → ${d.wheelDX}`);

    // True when shift is held down.
    d.shift = false;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.shift → ${d.shift}`);

    // Used to track whether anything happened since the context menu
    // was opened.
    d.selForContextMenu = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.selForContextMenu → ${d.selForContextMenu}`);

    d.activeTouch = null;
	logger.debug(`Display(place=${place},doc=${doc},input=${input}): d.activeTouch → ${d.activeTouch}`);

    input.init(d);
	logger.debug(`<<< Exiting Display(place=${place},doc=${doc},input=${input})`);
  }

  // STATE UPDATES

  // Used to get the editor into a consistent state again when options change.

  function loadMode(cm) {
	logger.debug(`>>> Entering loadMode(cm=${cm})`);
    cm.doc.mode = CodeMirror.getMode(cm.options, cm.doc.modeOption);
    resetModeState(cm);
	logger.debug(`<<< Exiting loadMode(cm=${cm})`);
  }

  function resetModeState(cm) {
	logger.debug(`>>> Entering resetModeState(cm=${cm})`);
    cm.doc.iter(function(line) {
	logger.debug('>>> Entering iter()');
      if (line.stateAfter) line.stateAfter = null;
      if (line.styles) line.styles = null;
    });
    cm.doc.frontier = cm.doc.first;
	logger.debug('<<< Exiting iter()');
    startWorker(cm, 100);
    cm.state.modeGen++;
    if (cm.curOp) regChange(cm);
	logger.debug(`<<< Exiting resetModeState(cm=${cm})`);
  }

  function wrappingChanged(cm) {
	logger.debug(`>>> Entering wrappingChanged(cm=${cm})`);
    if (cm.options.lineWrapping) {
      addClass(cm.display.wrapper, "CodeMirror-wrap");
      cm.display.sizer.style.minWidth = "";
      cm.display.sizerWidth = null;
    } else {
      rmClass(cm.display.wrapper, "CodeMirror-wrap");
      findMaxLine(cm);
    }
    estimateLineHeights(cm);
    regChange(cm);
	logger.debug(`<<< Exiting wrappingChanged(cm=${cm})`);
    clearCaches(cm);
    setTimeout(function(){updateScrollbars(cm);}, 100);
  }

  // Returns a function that estimates the height of a line, to use as
  // first approximation until the line becomes visible (and is thus
  // properly measurable).
  function estimateHeight(cm) {
	logger.debug(`>>> Entering estimateHeight(cm=${cm})`);
    var th = textHeight(cm.display), wrapping = cm.options.lineWrapping;
    var perLine = wrapping && Math.max(5, cm.display.scroller.clientWidth / charWidth(cm.display) - 3);
	logger.debug(`estimateHeight(cm=${cm}): th → ${th}`);
    return function(line) {
      if (lineIsHidden(cm.doc, line)) return 0;

      var widgetsHeight = 0;
      if (line.widgets) for (var i = 0; i < line.widgets.length; i++) {
        if (line.widgets[i].height) widgetsHeight += line.widgets[i].height;
      }

      if (wrapping)
        return widgetsHeight + (Math.ceil(line.text.length / perLine) || 1) * th;
      else
		logger.debug(`<<< Exiting estimateHeight(cm=${cm})`);
        return widgetsHeight + th;
    };
  }

  function estimateLineHeights(cm) {
	logger.debug(`>>> Entering estimateLineHeights(cm=${cm})`);
    var doc = cm.doc, est = estimateHeight(cm);
    doc.iter(function(line) {
      var estHeight = est(line);
	logger.debug(`estimateLineHeights(cm=${cm}): doc → ${doc}`);
      if (estHeight != line.height) updateLineHeight(line, estHeight);
    });
	logger.debug(`<<< Exiting estimateLineHeights(cm=${cm})`);
  }

  function themeChanged(cm) {
	logger.debug(`>>> Entering themeChanged(cm=${cm})`);
    cm.display.wrapper.className = cm.display.wrapper.className.replace(/\s*cm-s-\S+/g, "") +
      cm.options.theme.replace(/(^|\s)\s*/g, " cm-s-");
    clearCaches(cm);
	logger.debug(`<<< Exiting themeChanged(cm=${cm})`);
  }

  function guttersChanged(cm) {
	logger.debug(`>>> Entering guttersChanged(cm=${cm})`);
    updateGutters(cm);
    regChange(cm);
    setTimeout(function(){alignHorizontally(cm);}, 20);
	logger.debug(`<<< Exiting guttersChanged(cm=${cm})`);
  }

  // Rebuild the gutter elements, ensure the margin to the left of the
  // code matches their width.
  function updateGutters(cm) {
	logger.debug(`>>> Entering updateGutters(cm=${cm})`);
    var gutters = cm.display.gutters, specs = cm.options.gutters;
    removeChildren(gutters);
    for (var i = 0; i < specs.length; ++i) {
      var gutterClass = specs[i];
      var gElt = gutters.appendChild(elt("div", null, "CodeMirror-gutter " + gutterClass));
	logger.debug(`updateGutters(cm=${cm}): gElt → ${gElt}`);
      if (gutterClass == "CodeMirror-linenumbers") {
        cm.display.lineGutter = gElt;
        gElt.style.width = (cm.display.lineNumWidth || 1) + "px";
      }
    }
    gutters.style.display = i ? "" : "none";
    updateGutterSpace(cm);
	logger.debug(`<<< Exiting updateGutters(cm=${cm})`);
  }

  function updateGutterSpace(cm) {
	logger.debug(`>>> Entering updateGutterSpace(cm=${cm})`);
    var width = cm.display.gutters.offsetWidth;
    cm.display.sizer.style.marginLeft = width + "px";
	logger.debug(`<<< Exiting updateGutterSpace(cm=${cm})`);
  }

  // Compute the character length of a line, taking into account
  // collapsed ranges (see markText) that might hide parts, and join
  // other lines onto it.
  function lineLength(line) {
	logger.debug(`>>> Entering lineLength(line=${line})`);
    if (line.height == 0) return 0;
    var len = line.text.length, merged, cur = line;
    while (merged = collapsedSpanAtStart(cur)) {
      var found = merged.find(0, true);
      cur = found.from.line;
      len += found.from.ch - found.to.ch;
    }
    cur = line;
    while (merged = collapsedSpanAtEnd(cur)) {
      var found = merged.find(0, true);
	logger.debug(`lineLength(line=${line}): found → ${found}`);
      len -= cur.text.length - found.from.ch;
      cur = found.to.line;
      len += cur.text.length - found.to.ch;
    }
	logger.debug(`<<< Exiting lineLength(line=${line})`);
    return len;
  }

  // Find the longest line in the document.
  function findMaxLine(cm) {
	logger.debug(`>>> Entering findMaxLine(cm=${cm})`);
    var d = cm.display, doc = cm.doc;
    d.maxLine = getLine(doc, doc.first);
    d.maxLineLength = lineLength(d.maxLine);
    d.maxLineChanged = true;
	logger.debug(`findMaxLine(cm=${cm}): d.maxLineChanged → ${d.maxLineChanged}`);
    doc.iter(function(line) {
	logger.debug(`<<< Exiting findMaxLine(cm=${cm})`);
      var len = lineLength(line);
	logger.debug(`iter(len=${len}): len → ${len}`);
      if (len > d.maxLineLength) {
        d.maxLineLength = len;
		logger.debug(`findMaxLine(cm=${cm}): d.maxLineLength → ${d.maxLineLength}`);
        d.maxLine = line;
      }
    });
	logger.debug(`findMaxLine(cm=${cm}): d.maxLine → ${d.maxLine}`);
  }

  // Make sure the gutters options contains the element
  // "CodeMirror-linenumbers" when the lineNumbers option is true.
  function setGuttersForLineNumbers(options) {
	logger.debug(`>>> Entering setGuttersForLineNumbers(options=${options})`);
    var found = indexOf(options.gutters, "CodeMirror-linenumbers");
    if (found == -1 && options.lineNumbers) {
	logger.debug(`setGuttersForLineNumbers(options=${options}): found → ${found}`);
      options.gutters = options.gutters.concat(["CodeMirror-linenumbers"]);
    } else if (found > -1 && !options.lineNumbers) {
      options.gutters = options.gutters.slice(0);
      options.gutters.splice(found, 1);
	logger.debug(`<<< Exiting setGuttersForLineNumbers(options=${options})`);
    }
  }

  // SCROLLBARS

  // Prepare DOM reads needed to update the scrollbars. Done in one
  // shot to minimize update/measure roundtrips.
  function measureForScrollbars(cm) {
	logger.debug(`>>> Entering measureForScrollbars(cm=${cm})`);
    var d = cm.display, gutterW = d.gutters.offsetWidth;
    var docH = Math.round(cm.doc.height + paddingVert(cm.display));
	logger.debug(`<<< Exiting measureForScrollbars(cm=${cm})`);
    return {
      clientHeight: d.scroller.clientHeight,
      viewHeight: d.wrapper.clientHeight,
      scrollWidth: d.scroller.scrollWidth, clientWidth: d.scroller.clientWidth,
      viewWidth: d.wrapper.clientWidth,
      barLeft: cm.options.fixedGutter ? gutterW : 0,
      docHeight: docH,
      scrollHeight: docH + scrollGap(cm) + d.barHeight,
      nativeBarWidth: d.nativeBarWidth,
      gutterWidth: gutterW
    };
  }

  function NativeScrollbars(place, scroll, cm) {
	logger.debug(`>>> Entering NativeScrollbars(place=${place},scroll=${scroll},cm=${cm})`);
    this.cm = cm;
    var vert = this.vert = elt("div", [elt("div", null, null, "min-width: 1px")], "CodeMirror-vscrollbar");
	logger.debug(`NativeScrollbars(place=${place},scroll=${scroll},cm=${cm}): cm → ${cm}`);
	logger.debug(`NativeScrollbars(place=${place},scroll=${scroll},cm=${cm}): vert → ${vert}`);
	logger.debug(`NativeScrollbars(place=${place},scroll=${scroll},cm=${cm}): this.cm → ${this.cm}`);
    var horiz = this.horiz = elt("div", [elt("div", null, null, "height: 100%; min-height: 1px")], "CodeMirror-hscrollbar");
    place(vert); place(horiz);
	logger.debug(`NativeScrollbars(place=${place},scroll=${scroll},cm=${cm}): horiz → ${horiz}`);

	logger.debug(`<<< Exiting NativeScrollbars(place=${place},scroll=${scroll},cm=${cm})`);
    on(vert, "scroll", function() {
      if (vert.clientHeight) scroll(vert.scrollTop, "vertical");
	logger.debug(`>>> Entering on(vert=${vert})`);
	logger.debug(`>>> Entering on(horiz=${horiz})`);
	logger.debug(`on(horiz=${horiz}): this.checkedOverlay → ${this.checkedOverlay}`);
    });
    on(horiz, "scroll", function() {
      if (horiz.clientWidth) scroll(horiz.scrollLeft, "horizontal");
    });

	logger.debug(`<<< Exiting on(vert=${vert})`);
    this.checkedOverlay = false;
	logger.debug(`on(vert=${vert}): this.checkedOverlay → ${this.checkedOverlay}`);
	logger.debug(`<<< Exiting on(horiz=${horiz})`);
    // Need to set a minimum width to see the scrollbar on IE7 (but must not set it on IE8).
    if (ie && ie_version < 8) this.horiz.style.minHeight = this.vert.style.minWidth = "18px";
  }

  NativeScrollbars.prototype = copyObj({
    update: function(measure) {
	logger.debug(`>>> Entering update(measure=${measure})`);
      var needsH = measure.scrollWidth > measure.clientWidth + 1;
      var needsV = measure.scrollHeight > measure.clientHeight + 1;
      var sWidth = measure.nativeBarWidth;

      if (needsV) {
        this.vert.style.display = "block";
        this.vert.style.bottom = needsH ? sWidth + "px" : "0";
        var totalHeight = measure.viewHeight - (needsH ? sWidth : 0);
		logger.debug(`update(measure=${measure}): totalHeight → ${totalHeight}`);
        // A bug in IE8 can cause this value to be negative, so guard it.
        this.vert.firstChild.style.height =
          Math.max(0, measure.scrollHeight - measure.clientHeight + totalHeight) + "px";
      } else {
        this.vert.style.display = "";
        this.vert.firstChild.style.height = "0";
      }

      if (needsH) {
        this.horiz.style.display = "block";
        this.horiz.style.right = needsV ? sWidth + "px" : "0";
        this.horiz.style.left = measure.barLeft + "px";
        var totalWidth = measure.viewWidth - measure.barLeft - (needsV ? sWidth : 0);
		logger.debug(`update(measure=${measure}): totalWidth → ${totalWidth}`);
        this.horiz.firstChild.style.width =
          (measure.scrollWidth - measure.clientWidth + totalWidth) + "px";
      } else {
        this.horiz.style.display = "";
        this.horiz.firstChild.style.width = "0";
      }

      if (!this.checkedOverlay && measure.clientHeight > 0) {
        if (sWidth == 0) this.overlayHack();
        this.checkedOverlay = true;
      }

	logger.debug(`<<< Exiting update(measure=${measure})`);
      return {right: needsV ? sWidth : 0, bottom: needsH ? sWidth : 0};
    },
    setScrollLeft: function(pos) {
	logger.debug(`>>> Entering setScrollLeft(pos=${pos})`);
      if (this.horiz.scrollLeft != pos) this.horiz.scrollLeft = pos;
	logger.debug(`<<< Exiting setScrollLeft(pos=${pos})`);
	logger.debug(`>>> Entering setScrollLeft(cm=${cm},val=${val},isScroller=${isScroller})`);
	logger.debug(`setScrollLeft(cm=${cm},val=${val},isScroller=${isScroller}): val → ${val}`);
	logger.debug(`<<< Exiting setScrollLeft(cm=${cm},val=${val},isScroller=${isScroller})`);
    },
    setScrollTop: function(pos) {
	logger.debug(`>>> Entering setScrollTop(pos=${pos})`);
      if (this.vert.scrollTop != pos) this.vert.scrollTop = pos;
	logger.debug(`<<< Exiting setScrollTop(pos=${pos})`);
	logger.debug(`>>> Entering setScrollTop(cm=${cm},val=${val})`);
	logger.debug(`<<< Exiting setScrollTop(cm=${cm},val=${val})`);
    },
    overlayHack: function() {
	logger.debug('>>> Entering overlayHack()');
      var w = mac && !mac_geMountainLion ? "12px" : "18px";
      this.horiz.style.minHeight = this.vert.style.minWidth = w;
	logger.debug(`overlayHack(): w → ${w}`);
      var self = this;
      var barMouseDown = function(e) {
		logger.debug(`>>> Entering barMouseDown(e=${e})`);
        if (e_target(e) != self.vert && e_target(e) != self.horiz)
          operation(self.cm, onMouseDown)(e);
		logger.debug(`<<< Exiting barMouseDown(e=${e})`);
      };
      on(this.vert, "mousedown", barMouseDown);
      on(this.horiz, "mousedown", barMouseDown);
	logger.debug('<<< Exiting overlayHack()');
    },
    clear: function() {
	logger.debug('>>> Entering clear()');
      var parent = this.horiz.parentNode;
      parent.removeChild(this.horiz);
      parent.removeChild(this.vert);
	logger.debug('<<< Exiting clear()');
    }
  }, NativeScrollbars.prototype);

  function NullScrollbars() {}

  NullScrollbars.prototype = copyObj({
    update: function() { return {bottom: 0, right: 0}; },
    setScrollLeft: function() {},
    setScrollTop: function() {},
    clear: function() {}
  }, NullScrollbars.prototype);

  CodeMirror.scrollbarModel = {"native": NativeScrollbars, "null": NullScrollbars};

  function initScrollbars(cm) {
	logger.debug(`>>> Entering initScrollbars(cm=${cm})`);
    if (cm.display.scrollbars) {
      cm.display.scrollbars.clear();
      if (cm.display.scrollbars.addClass)
        rmClass(cm.display.wrapper, cm.display.scrollbars.addClass);
    }

    cm.display.scrollbars = new CodeMirror.scrollbarModel[cm.options.scrollbarStyle](function(node) {
      cm.display.wrapper.insertBefore(node, cm.display.scrollbarFiller);
	logger.debug(`<<< Exiting initScrollbars(cm=${cm})`);
      // Prevent clicks in the scrollbars from killing focus
      on(node, "mousedown", function() {
		logger.debug(`>>> Entering on(node=${node})`);
        if (cm.state.focused) setTimeout(function() { cm.display.input.focus(); }, 0);
      });
      node.setAttribute("cm-not-content", "true");
	logger.debug(`<<< Exiting on(node=${node})`);
    }, function(pos, axis) {
      if (axis == "horizontal") setScrollLeft(cm, pos);
      else setScrollTop(cm, pos);
    }, cm);
    if (cm.display.scrollbars.addClass)
      addClass(cm.display.wrapper, cm.display.scrollbars.addClass);
  }

  function updateScrollbars(cm, measure) {
	logger.debug(`>>> Entering updateScrollbars(cm=${cm},measure=${measure})`);
    if (!measure) measure = measureForScrollbars(cm);
    var startWidth = cm.display.barWidth, startHeight = cm.display.barHeight;
    updateScrollbarsInner(cm, measure);
    for (var i = 0; i < 4 && startWidth != cm.display.barWidth || startHeight != cm.display.barHeight; i++) {
      if (startWidth != cm.display.barWidth && cm.options.lineWrapping)
        updateHeightsInViewport(cm);
      updateScrollbarsInner(cm, measureForScrollbars(cm));
      startWidth = cm.display.barWidth; startHeight = cm.display.barHeight;
	logger.debug(`<<< Exiting updateScrollbars(cm=${cm},measure=${measure})`);
    }
  }

  // Re-synchronize the fake scrollbars with the actual size of the
  // content.
  function updateScrollbarsInner(cm, measure) {
	logger.debug(`>>> Entering updateScrollbarsInner(cm=${cm},measure=${measure})`);
    var d = cm.display;
    var sizes = d.scrollbars.update(measure);
	logger.debug(`updateScrollbarsInner(cm=${cm},measure=${measure}): sizes → ${sizes}`);

    d.sizer.style.paddingRight = (d.barWidth = sizes.right) + "px";
    d.sizer.style.paddingBottom = (d.barHeight = sizes.bottom) + "px";

    if (sizes.right && sizes.bottom) {
      d.scrollbarFiller.style.display = "block";
      d.scrollbarFiller.style.height = sizes.bottom + "px";
      d.scrollbarFiller.style.width = sizes.right + "px";
    } else d.scrollbarFiller.style.display = "";
    if (sizes.bottom && cm.options.coverGutterNextToScrollbar && cm.options.fixedGutter) {
      d.gutterFiller.style.display = "block";
      d.gutterFiller.style.height = sizes.bottom + "px";
      d.gutterFiller.style.width = measure.gutterWidth + "px";
    } else d.gutterFiller.style.display = "";
	logger.debug(`<<< Exiting updateScrollbarsInner(cm=${cm},measure=${measure})`);
  }

  // Compute the lines that are visible in a given viewport (defaults
  // the the current scroll position). viewport may contain top,
  // height, and ensure (see op.scrollToPos) properties.
  function visibleLines(display, doc, viewport) {
	logger.debug(`>>> Entering visibleLines(display=${display},doc=${doc},viewport=${viewport})`);
    var top = viewport && viewport.top != null ? Math.max(0, viewport.top) : display.scroller.scrollTop;
    top = Math.floor(top - paddingTop(display));
    var bottom = viewport && viewport.bottom != null ? viewport.bottom : top + display.wrapper.clientHeight;
	logger.debug(`visibleLines(display=${display},doc=${doc},viewport=${viewport}): top → ${top}`);

    var from = lineAtHeight(doc, top), to = lineAtHeight(doc, bottom);
    // Ensure is a {from: {line, ch}, to: {line, ch}} object, and
    // forces those lines into the viewport (if possible).
    if (viewport && viewport.ensure) {
      var ensureFrom = viewport.ensure.from.line, ensureTo = viewport.ensure.to.line;
      if (ensureFrom < from) {
        from = ensureFrom;
        to = lineAtHeight(doc, heightAtLine(getLine(doc, ensureFrom)) + display.wrapper.clientHeight);
      } else if (Math.min(ensureTo, doc.lastLine()) >= to) {
        from = lineAtHeight(doc, heightAtLine(getLine(doc, ensureTo)) - display.wrapper.clientHeight);
        to = ensureTo;
      }
    }
	logger.debug(`<<< Exiting visibleLines(display=${display},doc=${doc},viewport=${viewport})`);
    return {from: from, to: Math.max(to, from + 1)};
  }

  // LINE NUMBERS

  // Re-align line numbers and gutter marks to compensate for
  // horizontal scrolling.
  function alignHorizontally(cm) {
	logger.debug(`>>> Entering alignHorizontally(cm=${cm})`);
    var display = cm.display, view = display.view;
    if (!display.alignWidgets && (!display.gutters.firstChild || !cm.options.fixedGutter)) return;
    var comp = compensateForHScroll(display) - display.scroller.scrollLeft + cm.doc.scrollLeft;
	logger.debug(`alignHorizontally(cm=${cm}): comp → ${comp}`);
    var gutterW = display.gutters.offsetWidth, left = comp + "px";
    for (var i = 0; i < view.length; i++) if (!view[i].hidden) {
      if (cm.options.fixedGutter && view[i].gutter)
        view[i].gutter.style.left = left;
      var align = view[i].alignable;
      if (align) for (var j = 0; j < align.length; j++)
        align[j].style.left = left;
    }
    if (cm.options.fixedGutter)
	logger.debug(`alignHorizontally(cm=${cm}): gutterW → ${gutterW}`);
      display.gutters.style.left = (comp + gutterW) + "px";
	logger.debug(`<<< Exiting alignHorizontally(cm=${cm})`);
  }

  // Used to ensure that the line number gutter is still the right
  // size for the current document size. Returns true when an update
  // is needed.
  function maybeUpdateLineNumberWidth(cm) {
	logger.debug(`>>> Entering maybeUpdateLineNumberWidth(cm=${cm})`);
    if (!cm.options.lineNumbers) return false;
    var doc = cm.doc, last = lineNumberFor(cm.options, doc.first + doc.size - 1), display = cm.display;
    if (last.length != display.lineNumChars) {
      var test = display.measure.appendChild(elt("div", [elt("div", last)],
                                                 "CodeMirror-linenumber CodeMirror-gutter-elt"));
      var innerW = test.firstChild.offsetWidth, padding = test.offsetWidth - innerW;
	logger.debug(`maybeUpdateLineNumberWidth(cm=${cm}): test → ${test}`);
      display.lineGutter.style.width = "";
      display.lineNumInnerWidth = Math.max(innerW, display.lineGutter.offsetWidth - padding) + 1;
      display.lineNumWidth = display.lineNumInnerWidth + padding;
	logger.debug(`maybeUpdateLineNumberWidth(cm=${cm}): display.lineNumWidth → ${display.lineNumWidth}`);
      display.lineNumChars = display.lineNumInnerWidth ? last.length : -1;
	logger.debug(`maybeUpdateLineNumberWidth(cm=${cm}): display.lineNumChars → ${display.lineNumChars}`);
      display.lineGutter.style.width = display.lineNumWidth + "px";
      updateGutterSpace(cm);
	logger.debug(`maybeUpdateLineNumberWidth(cm=${cm}): doc → ${doc}`);
	logger.debug(`<<< Exiting maybeUpdateLineNumberWidth(cm=${cm})`);
      return true;
    }
    return false;
  }

  function lineNumberFor(options, i) {
	logger.debug(`>>> Entering lineNumberFor(options=${options},i=${i})`);
	logger.debug(`<<< Exiting lineNumberFor(options=${options},i=${i})`);
    return String(options.lineNumberFormatter(i + options.firstLineNumber));
  }

  // Computes display.scroller.scrollLeft + display.gutters.offsetWidth,
  // but using getBoundingClientRect to get a sub-pixel-accurate
  // result.
  function compensateForHScroll(display) {
	logger.debug(`>>> Entering compensateForHScroll(display=${display})`);
	logger.debug(`<<< Exiting compensateForHScroll(display=${display})`);
    return display.scroller.getBoundingClientRect().left - display.sizer.getBoundingClientRect().left;
  }

  // DISPLAY DRAWING

  function DisplayUpdate(cm, viewport, force) {
	logger.debug(`>>> Entering DisplayUpdate(cm=${cm},viewport=${viewport},force=${force})`);
    var display = cm.display;

    this.viewport = viewport;
    // Store some values that we'll need later (but don't want to force a relayout for)
    this.visible = visibleLines(display, cm.doc, viewport);
	logger.debug(`DisplayUpdate(cm=${cm},viewport=${viewport},force=${force}): this.viewport → ${this.viewport}`);
	logger.debug(`DisplayUpdate(cm=${cm},viewport=${viewport},force=${force}): viewport → ${viewport}`);
    this.editorIsHidden = !display.wrapper.offsetWidth;
    this.wrapperHeight = display.wrapper.clientHeight;
    this.wrapperWidth = display.wrapper.clientWidth;
	logger.debug(`DisplayUpdate(cm=${cm},viewport=${viewport},force=${force}): this.editorIsHidden → ${this.editorIsHidden}`);
    this.oldDisplayWidth = displayWidth(cm);
    this.force = force;
    this.dims = getDimensions(cm);
	logger.debug(`DisplayUpdate(cm=${cm},viewport=${viewport},force=${force}): this.wrapperHeight → ${this.wrapperHeight}`);
	logger.debug(`DisplayUpdate(cm=${cm},viewport=${viewport},force=${force}): force → ${force}`);
	logger.debug(`DisplayUpdate(cm=${cm},viewport=${viewport},force=${force}): dims → ${dims}`);
    this.events = [];
	logger.debug(`DisplayUpdate(cm=${cm},viewport=${viewport},force=${force}): this.events → ${this.events}`);
	logger.debug(`<<< Exiting DisplayUpdate(cm=${cm},viewport=${viewport},force=${force})`);
  }

  DisplayUpdate.prototype.signal = function(emitter, type) {
    if (hasHandler(emitter, type))
      this.events.push(arguments);
  };
  DisplayUpdate.prototype.finish = function() {
    for (var i = 0; i < this.events.length; i++)
      signal.apply(null, this.events[i]);
  };

  function maybeClipScrollbars(cm) {
	logger.debug(`>>> Entering maybeClipScrollbars(cm=${cm})`);
    var display = cm.display;
    if (!display.scrollbarsClipped && display.scroller.offsetWidth) {
      display.nativeBarWidth = display.scroller.offsetWidth - display.scroller.clientWidth;
	logger.debug(`maybeClipScrollbars(cm=${cm}): display.nativeBarWidth → ${display.nativeBarWidth}`);
      display.heightForcer.style.height = scrollGap(cm) + "px";
      display.sizer.style.marginBottom = -display.nativeBarWidth + "px";
      display.sizer.style.borderRightWidth = scrollGap(cm) + "px";
      display.scrollbarsClipped = true;
	logger.debug(`maybeClipScrollbars(cm=${cm}): display.scrollbarsClipped → ${display.scrollbarsClipped}`);
    }
	logger.debug(`<<< Exiting maybeClipScrollbars(cm=${cm})`);
  }

  // Does the actual updating of the line display. Bails out
  // (returning false) when there is nothing to be done and forced is
  // false.
  function updateDisplayIfNeeded(cm, update) {
	logger.debug(`>>> Entering updateDisplayIfNeeded(cm=${cm},update=${update})`);
    var display = cm.display, doc = cm.doc;

    if (update.editorIsHidden) {
      resetView(cm);
      return false;
    }

    // Bail out if the visible area is already rendered and nothing changed.
    if (!update.force &&
        update.visible.from >= display.viewFrom && update.visible.to <= display.viewTo &&
        (display.updateLineNumbers == null || display.updateLineNumbers >= display.viewTo) &&
        display.renderedView == display.view && countDirtyView(cm) == 0)
      return false;

    if (maybeUpdateLineNumberWidth(cm)) {
      resetView(cm);
      update.dims = getDimensions(cm);
	logger.debug(`>>> Entering countDirtyView(cm=${cm})`);
	logger.debug(`<<< Exiting countDirtyView(cm=${cm})`);
    }

    // Compute a suitable new viewport (from & to)
    var end = doc.first + doc.size;
    var from = Math.max(update.visible.from - cm.options.viewportMargin, doc.first);
    var to = Math.min(end, update.visible.to + cm.options.viewportMargin);
    if (display.viewFrom < from && from - display.viewFrom < 20) from = Math.max(doc.first, display.viewFrom);
    if (display.viewTo > to && display.viewTo - to < 20) to = Math.min(end, display.viewTo);
    if (sawCollapsedSpans) {
      from = visualLineNo(cm.doc, from);
      to = visualLineEndNo(cm.doc, to);
    }

    var different = from != display.viewFrom || to != display.viewTo ||
      display.lastWrapHeight != update.wrapperHeight || display.lastWrapWidth != update.wrapperWidth;
    adjustView(cm, from, to);

    display.viewOffset = heightAtLine(getLine(cm.doc, display.viewFrom));
    // Position the mover div to align with the current scroll position
    cm.display.mover.style.top = display.viewOffset + "px";

    var toUpdate = countDirtyView(cm);
    if (!different && toUpdate == 0 && !update.force && display.renderedView == display.view &&
        (display.updateLineNumbers == null || display.updateLineNumbers >= display.viewTo))
      return false;

    // For big changes, we hide the enclosing element during the
    // update, since that speeds up the operations on most browsers.
    var focused = activeElt();
    if (toUpdate > 4) display.lineDiv.style.display = "none";
    patchDisplay(cm, display.updateLineNumbers, update.dims);
    if (toUpdate > 4) display.lineDiv.style.display = "";
    display.renderedView = display.view;
	logger.debug(`updateDisplayIfNeeded(cm=${cm},update=${update}): display.renderedView → ${display.renderedView}`);
    // There might have been a widget with a focused element that got
    // hidden or updated, if so re-focus it.
    if (focused && activeElt() != focused && focused.offsetHeight) focused.focus();

    // Prevent selection and cursors from interfering with the scroll
    // width and height.
    removeChildren(display.cursorDiv);
    removeChildren(display.selectionDiv);
    display.gutters.style.height = display.sizer.style.minHeight = 0;

    if (different) {
      display.lastWrapHeight = update.wrapperHeight;
	logger.debug(`updateDisplayIfNeeded(cm=${cm},update=${update}): display.lastWrapHeight → ${display.lastWrapHeight}`);
      display.lastWrapWidth = update.wrapperWidth;
	logger.debug(`updateDisplayIfNeeded(cm=${cm},update=${update}): display.lastWrapWidth → ${display.lastWrapWidth}`);
      startWorker(cm, 400);
    }

    display.updateLineNumbers = null;
	logger.debug(`updateDisplayIfNeeded(cm=${cm},update=${update}): display.updateLineNumbers → ${display.updateLineNumbers}`);

	logger.debug(`<<< Exiting updateDisplayIfNeeded(cm=${cm},update=${update})`);
    return true;
  }

  function postUpdateDisplay(cm, update) {
	logger.debug(`>>> Entering postUpdateDisplay(cm=${cm},update=${update})`);
    var viewport = update.viewport;
    for (var first = true;; first = false) {
	logger.debug(`postUpdateDisplay(cm=${cm},update=${update}): viewport → ${viewport}`);
      if (!first || !cm.options.lineWrapping || update.oldDisplayWidth == displayWidth(cm)) {
        // Clip forced viewport to actual scrollable area.
        if (viewport && viewport.top != null)
          viewport = {top: Math.min(cm.doc.height + paddingVert(cm.display) - displayHeight(cm), viewport.top)};
        // Updated line heights might result in the drawn area not
        // actually covering the viewport. Keep looping until it does.
        update.visible = visibleLines(cm.display, cm.doc, viewport);
        if (update.visible.from >= cm.display.viewFrom && update.visible.to <= cm.display.viewTo)
          break;
      }
      if (!updateDisplayIfNeeded(cm, update)) break;
      updateHeightsInViewport(cm);
      var barMeasure = measureForScrollbars(cm);
	logger.debug(`postUpdateDisplay(cm=${cm},update=${update}): barMeasure → ${barMeasure}`);
      updateSelection(cm);
      setDocumentHeight(cm, barMeasure);
      updateScrollbars(cm, barMeasure);
    }

    update.signal(cm, "update", cm);
    if (cm.display.viewFrom != cm.display.reportedViewFrom || cm.display.viewTo != cm.display.reportedViewTo) {
      update.signal(cm, "viewportChange", cm, cm.display.viewFrom, cm.display.viewTo);
      cm.display.reportedViewFrom = cm.display.viewFrom; cm.display.reportedViewTo = cm.display.viewTo;
	logger.debug(`<<< Exiting postUpdateDisplay(cm=${cm},update=${update})`);
    }
  }

  function updateDisplaySimple(cm, viewport) {
	logger.debug(`>>> Entering updateDisplaySimple(cm=${cm},viewport=${viewport})`);
    var update = new DisplayUpdate(cm, viewport);
    if (updateDisplayIfNeeded(cm, update)) {
      updateHeightsInViewport(cm);
      postUpdateDisplay(cm, update);
      var barMeasure = measureForScrollbars(cm);
	logger.debug(`updateDisplaySimple(cm=${cm},viewport=${viewport}): barMeasure → ${barMeasure}`);
      updateSelection(cm);
      setDocumentHeight(cm, barMeasure);
      updateScrollbars(cm, barMeasure);
      update.finish();
	logger.debug(`<<< Exiting updateDisplaySimple(cm=${cm},viewport=${viewport})`);
    }
  }

  function setDocumentHeight(cm, measure) {
	logger.debug(`>>> Entering setDocumentHeight(cm=${cm},measure=${measure})`);
    cm.display.sizer.style.minHeight = measure.docHeight + "px";
    var total = measure.docHeight + cm.display.barHeight;
	logger.debug(`setDocumentHeight(cm=${cm},measure=${measure}): total → ${total}`);
    cm.display.heightForcer.style.top = total + "px";
    cm.display.gutters.style.height = Math.max(total + scrollGap(cm), measure.clientHeight) + "px";
	logger.debug(`<<< Exiting setDocumentHeight(cm=${cm},measure=${measure})`);
  }

  // Read the actual heights of the rendered lines, and update their
  // stored heights to match.
  function updateHeightsInViewport(cm) {
	logger.debug(`>>> Entering updateHeightsInViewport(cm=${cm})`);
    var display = cm.display;
    var prevBottom = display.lineDiv.offsetTop;
    for (var i = 0; i < display.view.length; i++) {
      var cur = display.view[i], height;
      if (cur.hidden) continue;
      if (ie && ie_version < 8) {
        var bot = cur.node.offsetTop + cur.node.offsetHeight;
        height = bot - prevBottom;
        prevBottom = bot;
      } else {
        var box = cur.node.getBoundingClientRect();
		logger.debug(`updateHeightsInViewport(cm=${cm}): box → ${box}`);
        height = box.bottom - box.top;
      }
      var diff = cur.line.height - height;
      if (height < 2) height = textHeight(display);
      if (diff > .001 || diff < -.001) {
        updateLineHeight(cur.line, height);
        updateWidgetHeight(cur.line);
        if (cur.rest) for (var j = 0; j < cur.rest.length; j++)
		logger.debug(`<<< Exiting updateHeightsInViewport(cm=${cm})`);
          updateWidgetHeight(cur.rest[j]);
      }
    }
  }

  // Read and store the height of line widgets associated with the
  // given line.
  function updateWidgetHeight(line) {
	logger.debug(`>>> Entering updateWidgetHeight(line=${line})`);
    if (line.widgets) for (var i = 0; i < line.widgets.length; ++i)
      line.widgets[i].height = line.widgets[i].node.offsetHeight;
	logger.debug(`<<< Exiting updateWidgetHeight(line=${line})`);
  }

  // Do a bulk-read of the DOM positions and sizes needed to draw the
  // view, so that we don't interleave reading and writing to the DOM.
  function getDimensions(cm) {
	logger.debug(`>>> Entering getDimensions(cm=${cm})`);
    var d = cm.display, left = {}, width = {};
    var gutterLeft = d.gutters.clientLeft;
    for (var n = d.gutters.firstChild, i = 0; n; n = n.nextSibling, ++i) {
      left[cm.options.gutters[i]] = n.offsetLeft + n.clientLeft + gutterLeft;
      width[cm.options.gutters[i]] = n.clientWidth;
    }
	logger.debug(`<<< Exiting getDimensions(cm=${cm})`);
    return {fixedPos: compensateForHScroll(d),
            gutterTotalWidth: d.gutters.offsetWidth,
            gutterLeft: left,
            gutterWidth: width,
            wrapperWidth: d.wrapper.clientWidth};
  }

  // Sync the actual display DOM structure with display.view, removing
  // nodes for lines that are no longer in view, and creating the ones
  // that are not there yet, and updating the ones that are out of
  // date.
  function patchDisplay(cm, updateNumbersFrom, dims) {
	logger.debug(`>>> Entering patchDisplay(cm=${cm},updateNumbersFrom=${updateNumbersFrom},dims=${dims})`);
    var display = cm.display, lineNumbers = cm.options.lineNumbers;
    var container = display.lineDiv, cur = container.firstChild;

    function rm(node) {
	logger.debug(`>>> Entering rm(node=${node})`);
      var next = node.nextSibling;
      // Works around a throw-scroll bug in OS X Webkit
      if (webkit && mac && cm.display.currentWheelTarget == node)
        node.style.display = "none";
      else
        node.parentNode.removeChild(node);
	logger.debug(`<<< Exiting rm(node=${node})`);
      return next;
    }

    var view = display.view, lineN = display.viewFrom;
    // Loop over the elements in the view, syncing cur (the DOM nodes
    // in display.lineDiv) with the view as we go.
    for (var i = 0; i < view.length; i++) {
      var lineView = view[i];
      if (lineView.hidden) {
      } else if (!lineView.node || lineView.node.parentNode != container) { // Not drawn yet
        var node = buildLineElement(cm, lineView, lineN, dims);
        container.insertBefore(node, cur);
      } else { // Already drawn
        while (cur != lineView.node) cur = rm(cur);
        var updateNumber = lineNumbers && updateNumbersFrom != null &&
          updateNumbersFrom <= lineN && lineView.lineNumber;
        if (lineView.changes) {
          if (indexOf(lineView.changes, "gutter") > -1) updateNumber = false;
          updateLineForChanges(cm, lineView, lineN, dims);
        }
        if (updateNumber) {
          removeChildren(lineView.lineNumber);
          lineView.lineNumber.appendChild(document.createTextNode(lineNumberFor(cm.options, lineN)));
        }
        cur = lineView.node.nextSibling;
      }
      lineN += lineView.size;
    }
    while (cur) cur = rm(cur);
	logger.debug(`<<< Exiting patchDisplay(cm=${cm},updateNumbersFrom=${updateNumbersFrom},dims=${dims})`);
  }

  // When an aspect of a line changes, a string is added to
  // lineView.changes. This updates the relevant part of the line's
  // DOM structure.
  function updateLineForChanges(cm, lineView, lineN, dims) {
	logger.debug(`>>> Entering updateLineForChanges(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims})`);
    for (var j = 0; j < lineView.changes.length; j++) {
      var type = lineView.changes[j];
      if (type == "text") updateLineText(cm, lineView);
      else if (type == "gutter") updateLineGutter(cm, lineView, lineN, dims);
      else if (type == "class") updateLineClasses(lineView);
      else if (type == "widget") updateLineWidgets(cm, lineView, dims);
    }
    lineView.changes = null;
	logger.debug(`updateLineForChanges(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims}): lineView.changes → ${lineView.changes}`);
	logger.debug(`<<< Exiting updateLineForChanges(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims})`);
  }

  // Lines with gutter elements, widgets or a background class need to
  // be wrapped, and have the extra elements added to the wrapper div
  function ensureLineWrapped(lineView) {
	logger.debug(`>>> Entering ensureLineWrapped(lineView=${lineView})`);
    if (lineView.node == lineView.text) {
      lineView.node = elt("div", null, null, "position: relative");
      if (lineView.text.parentNode)
        lineView.text.parentNode.replaceChild(lineView.node, lineView.text);
      lineView.node.appendChild(lineView.text);
      if (ie && ie_version < 8) lineView.node.style.zIndex = 2;
    }
	logger.debug(`<<< Exiting ensureLineWrapped(lineView=${lineView})`);
    return lineView.node;
  }

  function updateLineBackground(lineView) {
	logger.debug(`>>> Entering updateLineBackground(lineView=${lineView})`);
    var cls = lineView.bgClass ? lineView.bgClass + " " + (lineView.line.bgClass || "") : lineView.line.bgClass;
    if (cls) cls += " CodeMirror-linebackground";
	logger.debug(`updateLineBackground(lineView=${lineView}): cls → ${cls}`);
    if (lineView.background) {
      if (cls) lineView.background.className = cls;
      else { lineView.background.parentNode.removeChild(lineView.background); lineView.background = null; }
    } else if (cls) {
      var wrap = ensureLineWrapped(lineView);
      lineView.background = wrap.insertBefore(elt("div", null, cls), wrap.firstChild);
	logger.debug(`updateLineBackground(lineView=${lineView}): wrap → ${wrap}`);
    }
	logger.debug(`<<< Exiting updateLineBackground(lineView=${lineView})`);
  }

  // Wrapper around buildLineContent which will reuse the structure
  // in display.externalMeasured when possible.
  function getLineContent(cm, lineView) {
	logger.debug(`>>> Entering getLineContent(cm=${cm},lineView=${lineView})`);
    var ext = cm.display.externalMeasured;
    if (ext && ext.line == lineView.line) {
      cm.display.externalMeasured = null;
      lineView.measure = ext.measure;
	logger.debug(`getLineContent(cm=${cm},lineView=${lineView}): lineView.measure → ${lineView.measure}`);
	logger.debug(`<<< Exiting getLineContent(cm=${cm},lineView=${lineView})`);
      return ext.built;
    }
    return buildLineContent(cm, lineView);
  }

  // Redraw the line's text. Interacts with the background and text
  // classes because the mode may output tokens that influence these
  // classes.
  function updateLineText(cm, lineView) {
	logger.debug(`>>> Entering updateLineText(cm=${cm},lineView=${lineView})`);
    var cls = lineView.text.className;
    var built = getLineContent(cm, lineView);
	logger.debug(`updateLineText(cm=${cm},lineView=${lineView}): built → ${built}`);
    if (lineView.text == lineView.node) lineView.node = built.pre;
    lineView.text.parentNode.replaceChild(built.pre, lineView.text);
    lineView.text = built.pre;
	logger.debug(`updateLineText(cm=${cm},lineView=${lineView}): lineView.text → ${lineView.text}`);
    if (built.bgClass != lineView.bgClass || built.textClass != lineView.textClass) {
      lineView.bgClass = built.bgClass;
	logger.debug(`updateLineText(cm=${cm},lineView=${lineView}): lineView.bgClass → ${lineView.bgClass}`);
      lineView.textClass = built.textClass;
	logger.debug(`updateLineText(cm=${cm},lineView=${lineView}): lineView.textClass → ${lineView.textClass}`);
      updateLineClasses(lineView);
    } else if (cls) {
      lineView.text.className = cls;
	logger.debug(`<<< Exiting updateLineText(cm=${cm},lineView=${lineView})`);
    }
  }

  function updateLineClasses(lineView) {
	logger.debug(`>>> Entering updateLineClasses(lineView=${lineView})`);
    updateLineBackground(lineView);
    if (lineView.line.wrapClass)
      ensureLineWrapped(lineView).className = lineView.line.wrapClass;
    else if (lineView.node != lineView.text)
      lineView.node.className = "";
    var textClass = lineView.textClass ? lineView.textClass + " " + (lineView.line.textClass || "") : lineView.line.textClass;
    lineView.text.className = textClass || "";
	logger.debug(`updateLineClasses(lineView=${lineView}): textClass → ${textClass}`);
	logger.debug(`<<< Exiting updateLineClasses(lineView=${lineView})`);
  }

  function updateLineGutter(cm, lineView, lineN, dims) {
	logger.debug(`>>> Entering updateLineGutter(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims})`);
    if (lineView.gutter) {
      lineView.node.removeChild(lineView.gutter);
      lineView.gutter = null;
	logger.debug(`updateLineGutter(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims}): lineView.gutter → ${lineView.gutter}`);
    }
    if (lineView.gutterBackground) {
      lineView.node.removeChild(lineView.gutterBackground);
      lineView.gutterBackground = null;
	logger.debug(`updateLineGutter(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims}): lineView.gutterBackground → ${lineView.gutterBackground}`);
    }
    if (lineView.line.gutterClass) {
      var wrap = ensureLineWrapped(lineView);
      lineView.gutterBackground = elt("div", null, "CodeMirror-gutter-background " + lineView.line.gutterClass,
                                      "left: " + (cm.options.fixedGutter ? dims.fixedPos : -dims.gutterTotalWidth) +
                                      "px; width: " + dims.gutterTotalWidth + "px");
      wrap.insertBefore(lineView.gutterBackground, lineView.text);
    }
    var markers = lineView.line.gutterMarkers;
    if (cm.options.lineNumbers || markers) {
      var wrap = ensureLineWrapped(lineView);
	logger.debug(`updateLineGutter(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims}): wrap → ${wrap}`);
      var gutterWrap = lineView.gutter = elt("div", null, "CodeMirror-gutter-wrapper", "left: " +
                                             (cm.options.fixedGutter ? dims.fixedPos : -dims.gutterTotalWidth) + "px");
      cm.display.input.setUneditable(gutterWrap);
      wrap.insertBefore(gutterWrap, lineView.text);
      if (lineView.line.gutterClass)
        gutterWrap.className += " " + lineView.line.gutterClass;
      if (cm.options.lineNumbers && (!markers || !markers["CodeMirror-linenumbers"]))
        lineView.lineNumber = gutterWrap.appendChild(
          elt("div", lineNumberFor(cm.options, lineN),
              "CodeMirror-linenumber CodeMirror-gutter-elt",
              "left: " + dims.gutterLeft["CodeMirror-linenumbers"] + "px; width: "
              + cm.display.lineNumInnerWidth + "px"));
      if (markers) for (var k = 0; k < cm.options.gutters.length; ++k) {
        var id = cm.options.gutters[k], found = markers.hasOwnProperty(id) && markers[id];
		logger.debug(`updateLineGutter(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims}): id → ${id}`);
        if (found)
          gutterWrap.appendChild(elt("div", [found], "CodeMirror-gutter-elt", "left: " +
                                     dims.gutterLeft[id] + "px; width: " + dims.gutterWidth[id] + "px"));
									logger.debug(`<<< Exiting updateLineGutter(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims})`);
      }
    }
  }

  function updateLineWidgets(cm, lineView, dims) {
	logger.debug(`>>> Entering updateLineWidgets(cm=${cm},lineView=${lineView},dims=${dims})`);
    if (lineView.alignable) lineView.alignable = null;
    for (var node = lineView.node.firstChild, next; node; node = next) {
      var next = node.nextSibling;
      if (node.className == "CodeMirror-linewidget")
        lineView.node.removeChild(node);
    }
    insertLineWidgets(cm, lineView, dims);
	logger.debug(`<<< Exiting updateLineWidgets(cm=${cm},lineView=${lineView},dims=${dims})`);
  }

  // Build a line's DOM representation from scratch
  function buildLineElement(cm, lineView, lineN, dims) {
	logger.debug(`>>> Entering buildLineElement(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims})`);
    var built = getLineContent(cm, lineView);
    lineView.text = lineView.node = built.pre;
	logger.debug(`buildLineElement(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims}): built → ${built}`);
	logger.debug(`buildLineElement(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims}): lineView.text → ${lineView.text}`);
    if (built.bgClass) lineView.bgClass = built.bgClass;
    if (built.textClass) lineView.textClass = built.textClass;

    updateLineClasses(lineView);
    updateLineGutter(cm, lineView, lineN, dims);
    insertLineWidgets(cm, lineView, dims);
	logger.debug(`<<< Exiting buildLineElement(cm=${cm},lineView=${lineView},lineN=${lineN},dims=${dims})`);
    return lineView.node;
  }

  // A lineView may contain multiple logical lines (when merged by
  // collapsed spans). The widgets for all of them need to be drawn.
  function insertLineWidgets(cm, lineView, dims) {
	logger.debug(`>>> Entering insertLineWidgets(cm=${cm},lineView=${lineView},dims=${dims})`);
    insertLineWidgetsFor(cm, lineView.line, lineView, dims, true);
    if (lineView.rest) for (var i = 0; i < lineView.rest.length; i++)
	logger.debug(`<<< Exiting insertLineWidgets(cm=${cm},lineView=${lineView},dims=${dims})`);
      insertLineWidgetsFor(cm, lineView.rest[i], lineView, dims, false);
  }

  function insertLineWidgetsFor(cm, line, lineView, dims, allowAbove) {
	logger.debug(`>>> Entering insertLineWidgetsFor(cm=${cm},line=${line},lineView=${lineView},dims=${dims},allowAbove=${allowAbove})`);
    if (!line.widgets) return;
    var wrap = ensureLineWrapped(lineView);
	logger.debug(`insertLineWidgetsFor(cm=${cm},line=${line},lineView=${lineView},dims=${dims},allowAbove=${allowAbove}): wrap → ${wrap}`);
    for (var i = 0, ws = line.widgets; i < ws.length; ++i) {
      var widget = ws[i], node = elt("div", [widget.node], "CodeMirror-linewidget");
	logger.debug(`insertLineWidgetsFor(cm=${cm},line=${line},lineView=${lineView},dims=${dims},allowAbove=${allowAbove}): widget → ${widget}`);
      if (!widget.handleMouseEvents) node.setAttribute("cm-ignore-events", "true");
      positionLineWidget(widget, node, lineView, dims);
      cm.display.input.setUneditable(node);
      if (allowAbove && widget.above)
        wrap.insertBefore(node, lineView.gutter || lineView.text);
      else
        wrap.appendChild(node);
      signalLater(widget, "redraw");
	logger.debug(`<<< Exiting insertLineWidgetsFor(cm=${cm},line=${line},lineView=${lineView},dims=${dims},allowAbove=${allowAbove})`);
    }
  }

  function positionLineWidget(widget, node, lineView, dims) {
	logger.debug(`>>> Entering positionLineWidget(widget=${widget},node=${node},lineView=${lineView},dims=${dims})`);
    if (widget.noHScroll) {
      (lineView.alignable || (lineView.alignable = [])).push(node);
      var width = dims.wrapperWidth;
      node.style.left = dims.fixedPos + "px";
      if (!widget.coverGutter) {
        width -= dims.gutterTotalWidth;
        node.style.paddingLeft = dims.gutterTotalWidth + "px";
      }
      node.style.width = width + "px";
    }
    if (widget.coverGutter) {
      node.style.zIndex = 5;
      node.style.position = "relative";
      if (!widget.noHScroll) node.style.marginLeft = -dims.gutterTotalWidth + "px";
	logger.debug(`<<< Exiting positionLineWidget(widget=${widget},node=${node},lineView=${lineView},dims=${dims})`);
    }
  }

  // POSITION OBJECT

  // A Pos instance represents a position within the text.
  var Pos = CodeMirror.Pos = function(line, ch) {
	logger.debug(`function(line=${line},ch=${ch}): this.line → ${this.line}`);
    if (!(this instanceof Pos)) return new Pos(line, ch);
    this.line = line; this.ch = ch;
  };

  // Compare two positions, return 0 if they are the same, a negative
  // number when a is less, and a positive number otherwise.
  var cmp = CodeMirror.cmpPos = function(a, b) { return a.line - b.line || a.ch - b.ch; };

  function copyPos(x) {return Pos(x.line, x.ch);}
  function maxPos(a, b) { return cmp(a, b) < 0 ? b : a; }
  function minPos(a, b) { return cmp(a, b) < 0 ? a : b; }

  // INPUT HANDLING

  function ensureFocus(cm) {
	logger.debug(`>>> Entering ensureFocus(cm=${cm})`);
    if (!cm.state.focused) { cm.display.input.focus(); onFocus(cm); }
	logger.debug(`<<< Exiting ensureFocus(cm=${cm})`);
  }

  function isReadOnly(cm) {
	logger.debug(`>>> Entering isReadOnly(cm=${cm})`);
	logger.debug(`<<< Exiting isReadOnly(cm=${cm})`);
    return cm.options.readOnly || cm.doc.cantEdit;
  }

  // This will be set to an array of strings when copying, so that,
  // when pasting, we know what kind of selections the copied text
  // was made out of.
  var lastCopied = null;

  function applyTextInput(cm, inserted, deleted, sel, origin) {
	logger.debug(`>>> Entering applyTextInput(cm=${cm},inserted=${inserted},deleted=${deleted},sel=${sel},origin=${origin})`);
    var doc = cm.doc;
    cm.display.shift = false;
    if (!sel) sel = doc.sel;

    var paste = cm.state.pasteIncoming || origin == "paste";
	logger.debug(`applyTextInput(cm=${cm},inserted=${inserted},deleted=${deleted},sel=${sel},origin=${origin}): paste → ${paste}`);
    var textLines = doc.splitLines(inserted), multiPaste = null;
	logger.debug(`applyTextInput(cm=${cm},inserted=${inserted},deleted=${deleted},sel=${sel},origin=${origin}): textLines → ${textLines}`);
    // When pasing N lines into N selections, insert one line per selection
    if (paste && sel.ranges.length > 1) {
      if (lastCopied && lastCopied.join("\n") == inserted) {
        if (sel.ranges.length % lastCopied.length == 0) {
          multiPaste = [];
          for (var i = 0; i < lastCopied.length; i++)
            multiPaste.push(doc.splitLines(lastCopied[i]));
        }
      } else if (textLines.length == sel.ranges.length) {
        multiPaste = map(textLines, function(l) { return [l]; });
      }
    }

    // Normal behavior is to insert the new text into every selection
    for (var i = sel.ranges.length - 1; i >= 0; i--) {
      var range = sel.ranges[i];
      var from = range.from(), to = range.to();
      if (range.empty()) {
        if (deleted && deleted > 0) // Handle deletion
          from = Pos(from.line, from.ch - deleted);
        else if (cm.state.overwrite && !paste) // Handle overwrite
          to = Pos(to.line, Math.min(getLine(doc, to.line).text.length, to.ch + lst(textLines).length));
      }
      var updateInput = cm.curOp.updateInput;
      var changeEvent = {from: from, to: to, text: multiPaste ? multiPaste[i % multiPaste.length] : textLines,
                         origin: origin || (paste ? "paste" : cm.state.cutIncoming ? "cut" : "+input")};
      makeChange(cm.doc, changeEvent);
      signalLater(cm, "inputRead", cm, changeEvent);
    }
    if (inserted && !paste)
      triggerElectric(cm, inserted);

    ensureCursorVisible(cm);
    cm.curOp.updateInput = updateInput;
    cm.curOp.typing = true;
    cm.state.pasteIncoming = cm.state.cutIncoming = false;
	logger.debug(`<<< Exiting applyTextInput(cm=${cm},inserted=${inserted},deleted=${deleted},sel=${sel},origin=${origin})`);
  }

  function handlePaste(e, cm) {
	logger.debug(`>>> Entering handlePaste(e=${e},cm=${cm})`);
    var pasted = e.clipboardData && e.clipboardData.getData("text/plain");
    if (pasted) {
	logger.debug(`handlePaste(e=${e},cm=${cm}): pasted → ${pasted}`);
      e.preventDefault();
      if (!isReadOnly(cm) && !cm.options.disableInput)
        runInOp(cm, function() { applyTextInput(cm, pasted, 0, null, "paste"); });
      return true;
    }
  }

  function triggerElectric(cm, inserted) {
	logger.debug(`>>> Entering triggerElectric(cm=${cm},inserted=${inserted})`);
    // When an 'electric' character is inserted, immediately trigger a reindent
    if (!cm.options.electricChars || !cm.options.smartIndent) return;
    var sel = cm.doc.sel;

    for (var i = sel.ranges.length - 1; i >= 0; i--) {
      var range = sel.ranges[i];
      if (range.head.ch > 100 || (i && sel.ranges[i - 1].head.line == range.head.line)) continue;
      var mode = cm.getModeAt(range.head);
	logger.debug(`triggerElectric(cm=${cm},inserted=${inserted}): mode → ${mode}`);
      var indented = false;
      if (mode.electricChars) {
        for (var j = 0; j < mode.electricChars.length; j++)
          if (inserted.indexOf(mode.electricChars.charAt(j)) > -1) {
            indented = indentLine(cm, range.head.line, "smart");
            break;
          }
      } else if (mode.electricInput) {
        if (mode.electricInput.test(getLine(cm.doc, range.head.line).text.slice(0, range.head.ch)))
          indented = indentLine(cm, range.head.line, "smart");
      }
      if (indented) signalLater(cm, "electricInput", cm, range.head.line);
	logger.debug(`triggerElectric(cm=${cm},inserted=${inserted}): indented → ${indented}`);
    }
	logger.debug(`<<< Exiting triggerElectric(cm=${cm},inserted=${inserted})`);
  }

  function copyableRanges(cm) {
	logger.debug(`>>> Entering copyableRanges(cm=${cm})`);
    var text = [], ranges = [];
    for (var i = 0; i < cm.doc.sel.ranges.length; i++) {
      var line = cm.doc.sel.ranges[i].head.line;
      var lineRange = {anchor: Pos(line, 0), head: Pos(line + 1, 0)};
	logger.debug(`copyableRanges(cm=${cm}): lineRange → ${lineRange}`);
      ranges.push(lineRange);
      text.push(cm.getRange(lineRange.anchor, lineRange.head));
    }
	logger.debug(`<<< Exiting copyableRanges(cm=${cm})`);
    return {text: text, ranges: ranges};
  }

  function disableBrowserMagic(field) {
	logger.debug(`>>> Entering disableBrowserMagic(field=${field})`);
    field.setAttribute("autocorrect", "off");
    field.setAttribute("autocapitalize", "off");
    field.setAttribute("spellcheck", "false");
	logger.debug(`<<< Exiting disableBrowserMagic(field=${field})`);
  }

  // TEXTAREA INPUT STYLE

  function TextareaInput(cm) {
	logger.debug(`>>> Entering TextareaInput(cm=${cm})`);
    this.cm = cm;
    // See input.poll and input.reset
    this.prevInput = "";
	logger.debug(`TextareaInput(cm=${cm}): cm → ${cm}`);
	logger.debug(`TextareaInput(cm=${cm}): this.cm → ${this.cm}`);

    // Flag that indicates whether we expect input to appear real soon
    // now (after some event like 'keypress' or 'input') and are
    // polling intensively.
    this.pollingFast = false;
	logger.debug(`TextareaInput(cm=${cm}): this.prevInput → ${this.prevInput}`);
    // Self-resetting timeout for the poller
    this.polling = new Delayed();
	logger.debug(`TextareaInput(cm=${cm}): this.pollingFast → ${this.pollingFast}`);
    // Tracks when input.reset has punted to just putting a short
    // string into the textarea instead of the full selection.
    this.inaccurateSelection = false;
    // Used to work around IE issue with selection being forgotten when focus moves away from textarea
    this.hasSelection = false;
	logger.debug(`TextareaInput(cm=${cm}): this.inaccurateSelection → ${this.inaccurateSelection}`);
	logger.debug(`TextareaInput(cm=${cm}): this.hasSelection → ${this.hasSelection}`);
    this.composing = null;
	logger.debug(`TextareaInput(cm=${cm}): this.composing → ${this.composing}`);
	logger.debug(`<<< Exiting TextareaInput(cm=${cm})`);
  };

  function hiddenTextarea() {
    var te = elt("textarea", null, null, "position: absolute; padding: 0; width: 1px; height: 1em; outline: none");
	logger.debug('>>> Entering hiddenTextarea()');
    var div = elt("div", [te], null, "overflow: hidden; position: relative; width: 3px; height: 0px;");
    // The textarea is kept positioned near the cursor to prevent the
    // fact that it'll be scrolled into view on input from scrolling
    // our fake cursor out of view. On webkit, when wrap=off, paste is
    // very slow. So make the area wide instead.
    if (webkit) te.style.width = "1000px";
    else te.setAttribute("wrap", "off");
	logger.debug(`hiddenTextarea(): div → ${div}`);
    // If border: 0; -- iOS fails to open keyboard (issue #1287)
    if (ios) te.style.border = "1px solid black";
    disableBrowserMagic(te);
	logger.debug('<<< Exiting hiddenTextarea()');
    return div;
  }

  TextareaInput.prototype = copyObj({
    init: function(display) {
	logger.debug(`>>> Entering init(display=${display})`);
      var input = this, cm = this.cm;

      // Wraps and hides input textarea
      var div = this.wrapper = hiddenTextarea();
      // The semihidden textarea that is focused when the editor is
      // focused, and receives input.
      var te = this.textarea = div.firstChild;
      display.wrapper.insertBefore(div, display.wrapper.firstChild);

      // Needed to hide big blue blinking cursor on Mobile Safari (doesn't seem to work in iOS 8 anymore)
      if (ios) te.style.width = "0px";
	logger.debug(`<<< Exiting init(display=${display})`);

      on(te, "input", function() {
        if (ie && ie_version >= 9 && input.hasSelection) input.hasSelection = null;
        input.poll();
      });

      on(te, "paste", function(e) {
        if (handlePaste(e, cm)) return true;

        cm.state.pasteIncoming = true;
        input.fastPoll();
      });

      function prepareCopyCut(e) {
		logger.debug(`>>> Entering prepareCopyCut(e=${e})`);
        if (cm.somethingSelected()) {
          lastCopied = cm.getSelections();
          if (input.inaccurateSelection) {
            input.prevInput = "";
            input.inaccurateSelection = false;
            te.value = lastCopied.join("\n");
            selectInput(te);
          }
        } else if (!cm.options.lineWiseCopyCut) {
          return;
        } else {
          var ranges = copyableRanges(cm);
		logger.debug(`prepareCopyCut(e=${e}): ranges → ${ranges}`);
          lastCopied = ranges.text;
          if (e.type == "cut") {
            cm.setSelections(ranges.ranges, null, sel_dontScroll);
          } else {
            input.prevInput = "";
            te.value = ranges.text.join("\n");
            selectInput(te);
          }
        }
        if (e.type == "cut") cm.state.cutIncoming = true;
		logger.debug(`<<< Exiting prepareCopyCut(e=${e})`);
      }
      on(te, "cut", prepareCopyCut);
      on(te, "copy", prepareCopyCut);

      on(display.scroller, "paste", function(e) {
        if (eventInWidget(display, e)) return;
        cm.state.pasteIncoming = true;
        input.focus();
      });

      // Prevent normal selection in the editor (we handle our own)
      on(display.lineSpace, "selectstart", function(e) {
        if (!eventInWidget(display, e)) e_preventDefault(e);
      });
	logger.debug(`on(): input.composing → ${input.composing}`);
	logger.debug(`on(te=${te}): input.composing → ${input.composing}`);

      on(te, "compositionstart", function() {
        var start = cm.getCursor("from");
        input.composing = {
          start: start,
          range: cm.markText(start, cm.getCursor("to"), {className: "CodeMirror-composing"})
        };
      });
      on(te, "compositionend", function() {
        if (input.composing) {
          input.poll();
          input.composing.range.clear();
          input.composing = null;
        }
      });
	logger.debug(`init(display=${display}): div → ${div}`);
	logger.debug(`init(display=${display}): div.contentEditable → ${div.contentEditable}`);
    },

    prepareSelection: function() {
	logger.debug('>>> Entering prepareSelection()');
      // Redraw the selection and/or cursor
      var cm = this.cm, display = cm.display, doc = cm.doc;
      var result = prepareSelection(cm);

      // Move the hidden textarea near the cursor to prevent scrolling artifacts
      if (cm.options.moveInputWithCursor) {
        var headPos = cursorCoords(cm, doc.sel.primary().head, "div");
        var wrapOff = display.wrapper.getBoundingClientRect(), lineOff = display.lineDiv.getBoundingClientRect();
		logger.debug(`prepareSelection(): headPos → ${headPos}`);
        result.teTop = Math.max(0, Math.min(display.wrapper.clientHeight - 10,
                                            headPos.top + lineOff.top - wrapOff.top));
											logger.debug(`prepareSelection(): wrapOff → ${wrapOff}`);
        result.teLeft = Math.max(0, Math.min(display.wrapper.clientWidth - 10,
                                             headPos.left + lineOff.left - wrapOff.left));
      }

	logger.debug(`prepareSelection(): result.focus → ${result.focus}`);
	logger.debug('<<< Exiting prepareSelection()');
      return result;
    },

    showSelection: function(drawn) {
	logger.debug(`>>> Entering showSelection(drawn=${drawn})`);
      var cm = this.cm, display = cm.display;
      removeChildrenAndAdd(display.cursorDiv, drawn.cursors);
      removeChildrenAndAdd(display.selectionDiv, drawn.selection);
      if (drawn.teTop != null) {
        this.wrapper.style.top = drawn.teTop + "px";
        this.wrapper.style.left = drawn.teLeft + "px";
		logger.debug(`<<< Exiting showSelection(drawn=${drawn})`);
      }
    },

    // Reset the input to correspond to the selection (or to be empty,
    // when not typing and nothing is selected)
    reset: function(typing) {
      if (this.contextMenuPending) return;
      var minimal, selected, cm = this.cm, doc = cm.doc;
      if (cm.somethingSelected()) {
        this.prevInput = "";
        var range = doc.sel.primary();
        minimal = hasCopyEvent &&
          (range.to().line - range.from().line > 100 || (selected = cm.getSelection()).length > 1000);
        var content = minimal ? "-" : selected || cm.getSelection();
        this.textarea.value = content;
        if (cm.state.focused) selectInput(this.textarea);
        if (ie && ie_version >= 9) this.hasSelection = content;
      } else if (!typing) {
        this.prevInput = this.textarea.value = "";
        if (ie && ie_version >= 9) this.hasSelection = null;
      }
      this.inaccurateSelection = minimal;
	logger.debug('>>> Entering reset()');
	logger.debug('<<< Exiting reset()');
    },

    getField: function() { return this.textarea; },

    supportsTouch: function() { return false; },

    focus: function() {
	logger.debug('>>> Entering focus()');
      if (this.cm.options.readOnly != "nocursor" && (!mobile || activeElt() != this.textarea)) {
        try { this.textarea.focus(); }
        catch (e) {} // IE8 will throw if the textarea is display: none or not in DOM
		logger.debug('<<< Exiting focus()');
      }
    },

    blur: function() { this.textarea.blur(); },

    resetPosition: function() {
	logger.debug('>>> Entering resetPosition()');
      this.wrapper.style.top = this.wrapper.style.left = 0;
	logger.debug('<<< Exiting resetPosition()');
    },

    receivedFocus: function() { this.slowPoll(); },

    // Poll for input changes, using the normal rate of polling. This
    // runs as long as the editor is focused.
    slowPoll: function() {
	logger.debug('>>> Entering slowPoll()');
      var input = this;
      if (input.pollingFast) return;
	logger.debug('<<< Exiting slowPoll()');
      input.polling.set(this.cm.options.pollInterval, function() {
		logger.debug('>>> Entering set()');
        input.poll();
		logger.debug('<<< Exiting set()');
        if (input.cm.state.focused) input.slowPoll();
      });
	logger.debug('>>> Entering receivedFocus()');
	logger.debug('<<< Exiting receivedFocus()');
    },

    // When an event has just come in that is likely to add or change
    // something in the input textarea, we poll faster, to ensure that
    // the change appears on the screen quickly.
    fastPoll: function() {
	logger.debug('>>> Entering fastPoll()');
      var missed = false, input = this;
      input.pollingFast = true;
	logger.debug(`fastPoll(): input.pollingFast → ${input.pollingFast}`);
      function p() {
		logger.debug('>>> Entering p()');
        var changed = input.poll();
        if (!changed && !missed) {missed = true; input.polling.set(60, p);}
        else {input.pollingFast = false; input.slowPoll();}
		logger.debug('<<< Exiting p()');
      }
      input.polling.set(20, p);
	logger.debug('<<< Exiting fastPoll()');
    },

    // Read input from the textarea, and update the document to match.
    // When something is selected, it is present in the textarea, and
    // selected (unless it is huge, in which case a placeholder is
    // used). When nothing is selected, the cursor sits after previously
    // seen text (can be empty), which is stored in prevInput (we must
    // not reset the textarea when typing, because that breaks IME).
    poll: function() {
	logger.debug('>>> Entering poll()');
      var cm = this.cm, input = this.textarea, prevInput = this.prevInput;
      // Since this is called a *lot*, try to bail out as cheaply as
      // possible when it is clear that nothing happened. hasSelection
      // will be the case when there is a lot of text in the textarea,
      // in which case reading its value would be expensive.
      if (this.contextMenuPending || !cm.state.focused ||
          (hasSelection(input) && !prevInput && !this.composing) ||
          isReadOnly(cm) || cm.options.disableInput || cm.state.keySeq)
        return false;

      var text = input.value;
      // If nothing changed, bail.
      if (text == prevInput && !cm.somethingSelected()) return false;
      // Work around nonsensical selection resetting in IE9/10, and
      // inexplicable appearance of private area unicode characters on
      // some key combos in Mac (#2689).
      if (ie && ie_version >= 9 && this.hasSelection === text ||
          mac && /[\uf700-\uf7ff]/.test(text)) {
        cm.display.input.reset();
        return false;
      }

      if (cm.doc.sel == cm.display.selForContextMenu) {
        var first = text.charCodeAt(0);
        if (first == 0x200b && !prevInput) prevInput = "\u200b";
        if (first == 0x21da) { this.reset(); return this.cm.execCommand("undo"); }
      }
      // Find the part of the input that is actually new
      var same = 0, l = Math.min(prevInput.length, text.length);
      while (same < l && prevInput.charCodeAt(same) == text.charCodeAt(same)) ++same;

	logger.debug('<<< Exiting poll()');
      var self = this;
      runInOp(cm, function() {
		logger.debug(`>>> Entering runInOp(cm=${cm})`);
        applyTextInput(cm, text.slice(same), prevInput.length - same,
                       null, self.composing ? "*compose" : null);

        // Don't leave long text in the textarea, since it makes further polling slow
        if (text.length > 1000 || text.indexOf("\n") > -1) input.value = self.prevInput = "";
        else self.prevInput = text;

        if (self.composing) {
          self.composing.range.clear();
          self.composing.range = cm.markText(self.composing.start, cm.getCursor("to"),
                                             {className: "CodeMirror-composing"});
		logger.debug(`<<< Exiting runInOp(cm=${cm})`);
		logger.debug('<<< Exiting runInOp()');
		logger.debug(`<<< Exiting runInOp(cm=${cm},f=${f})`);
        }
      });
      return true;
    },

    ensurePolled: function() {
	logger.debug('>>> Entering ensurePolled()');
      if (this.pollingFast && this.poll()) this.pollingFast = false;
	logger.debug('<<< Exiting ensurePolled()');
    },

    onKeyPress: function() {
      if (ie && ie_version >= 9) this.hasSelection = null;
      this.fastPoll();
    },

    onContextMenu: function(e) {
	logger.debug(`>>> Entering onContextMenu(e=${e})`);
      var input = this, cm = input.cm, display = cm.display, te = input.textarea;
      var pos = posFromMouse(cm, e), scrollPos = display.scroller.scrollTop;
	logger.debug(`onContextMenu(e=${e}): pos → ${pos}`);
      if (!pos || presto) return; // Opera is difficult.

      // Reset the current text selection only if the click is done outside of the selection
      // and 'resetSelectionOnContextMenu' option is true.
      var reset = cm.options.resetSelectionOnContextMenu;
      if (reset && cm.doc.sel.contains(pos) == -1)
        operation(cm, setSelection)(cm.doc, simpleSelection(pos), sel_dontScroll);

      var oldCSS = te.style.cssText;
      input.wrapper.style.position = "absolute";
      te.style.cssText = "position: fixed; width: 30px; height: 30px; top: " + (e.clientY - 5) +
        "px; left: " + (e.clientX - 5) + "px; z-index: 1000; background: " +
        (ie ? "rgba(255, 255, 255, .05)" : "transparent") +
        "; outline: none; border-width: 0; outline: none; overflow: hidden; opacity: .05; filter: alpha(opacity=5);";
      if (webkit) var oldScrollY = window.scrollY; // Work around Chrome issue (#2712)
	logger.debug(`onContextMenu(e=${e}): oldScrollY → ${oldScrollY}`);
      display.input.focus();
      if (webkit) window.scrollTo(null, oldScrollY);
      display.input.reset();
      // Adds "Select all" to context menu in FF
      if (!cm.somethingSelected()) te.value = input.prevInput = " ";
      input.contextMenuPending = true;
	logger.debug(`onContextMenu(e=${e}): input.contextMenuPending → ${input.contextMenuPending}`);
      display.selForContextMenu = cm.doc.sel;
	logger.debug(`onContextMenu(e=${e}): display.selForContextMenu → ${display.selForContextMenu}`);
      clearTimeout(display.detectingSelectAll);

	logger.debug(`<<< Exiting onContextMenu(e=${e})`);
      // Select-all will be greyed out if there's nothing to select, so
      // this adds a zero-width space so that we can later check whether
      // it got selected.
      function prepareSelectAllHack() {
		logger.debug('>>> Entering prepareSelectAllHack()');
        if (te.selectionStart != null) {
          var selected = cm.somethingSelected();
		logger.debug(`prepareSelectAllHack(): selected → ${selected}`);
          var extval = "\u200b" + (selected ? te.value : "");
		logger.debug(`prepareSelectAllHack(): extval → ${extval}`);
          te.value = "\u21da"; // Used to catch context-menu undo
          te.value = extval;
          input.prevInput = selected ? "" : "\u200b";
          te.selectionStart = 1; te.selectionEnd = extval.length;
          // Re-set this, in case some other handler touched the
          // selection in the meantime.
          display.selForContextMenu = cm.doc.sel;
        }
		logger.debug('<<< Exiting prepareSelectAllHack()');
      }
      function rehide() {
		logger.debug('>>> Entering rehide()');
        input.contextMenuPending = false;
        input.wrapper.style.position = "relative";
        te.style.cssText = oldCSS;
        if (ie && ie_version < 9) display.scrollbars.setScrollTop(display.scroller.scrollTop = scrollPos);

        // Try to detect the user choosing select-all
        if (te.selectionStart != null) {
          if (!ie || (ie && ie_version < 9)) prepareSelectAllHack();
          var i = 0, poll = function() {
            if (display.selForContextMenu == cm.doc.sel && te.selectionStart == 0 &&
                te.selectionEnd > 0 && input.prevInput == "\u200b")
              operation(cm, commands.selectAll)(cm);
            else if (i++ < 10) display.detectingSelectAll = setTimeout(poll, 500);
            else display.input.reset();
          };
          display.detectingSelectAll = setTimeout(poll, 200);
		logger.debug('<<< Exiting rehide()');
        }
      }

      if (ie && ie_version >= 9) prepareSelectAllHack();
      if (captureRightClick) {
        e_stop(e);
        var mouseup = function() {
		logger.debug('>>> Entering mouseup()');
          off(window, "mouseup", mouseup);
          setTimeout(rehide, 20);
		logger.debug('<<< Exiting mouseup()');
        };
        on(window, "mouseup", mouseup);
      } else {
        setTimeout(rehide, 50);
      }
	logger.debug(`>>> Entering onContextMenu(cm=${cm},e=${e})`);
	logger.debug(`<<< Exiting onContextMenu(cm=${cm},e=${e})`);
    },

    setUneditable: nothing,

    needsContentAttribute: false
  }, TextareaInput.prototype);

  // CONTENTEDITABLE INPUT STYLE

  function ContentEditableInput(cm) {
	logger.debug(`>>> Entering ContentEditableInput(cm=${cm})`);
    this.cm = cm;
    this.lastAnchorNode = this.lastAnchorOffset = this.lastFocusNode = this.lastFocusOffset = null;
	logger.debug(`ContentEditableInput(cm=${cm}): cm → ${cm}`);
	logger.debug(`ContentEditableInput(cm=${cm}): this.cm → ${this.cm}`);
	logger.debug(`ContentEditableInput(cm=${cm}): this.gracePeriod → ${this.gracePeriod}`);
    this.polling = new Delayed();
    this.gracePeriod = false;
	logger.debug(`<<< Exiting ContentEditableInput(cm=${cm})`);
  }

  ContentEditableInput.prototype = copyObj({
    init: function(display) {
      var input = this, cm = input.cm;
      var div = input.div = display.lineDiv;
      div.contentEditable = "true";
      disableBrowserMagic(div);

      on(div, "paste", function(e) { handlePaste(e, cm); })

      on(div, "compositionstart", function(e) {
        var data = e.data;
        input.composing = {sel: cm.doc.sel, data: data, startData: data};
        if (!data) return;
        var prim = cm.doc.sel.primary();
        var line = cm.getLine(prim.head.line);
		logger.debug(`on(div=${div},data=${data}): data → ${data}`);
		logger.debug(`on(div=${div},data=${data}): prim → ${prim}`);
		logger.debug(`on(div=${div},data=${data}): line → ${line}`);
        var found = line.indexOf(data, Math.max(0, prim.head.ch - data.length));
        if (found > -1 && found <= prim.head.ch)
          input.composing.sel = simpleSelection(Pos(prim.head.line, found),
                                                Pos(prim.head.line, found + data.length));
      });
      on(div, "compositionupdate", function(e) {
        input.composing.data = e.data;
      });
	logger.debug(`on(div=${div}): input.composing → ${input.composing}`);
      on(div, "compositionend", function(e) {
        var ours = input.composing;
        if (!ours) return;
        if (e.data != ours.startData && !/\u200b/.test(e.data))
          ours.data = e.data;
        // Need a small delay to prevent other code (input event,
        // selection polling) from doing damage when fired right after
        // compositionend.
        setTimeout(function() {
		logger.debug('>>> Entering setTimeout()');
          if (!ours.handled)
            input.applyComposition(ours);
          if (input.composing == ours)
			logger.debug('<<< Exiting setTimeout()');
            input.composing = null;
        }, 50);
      });

      on(div, "touchstart", function() {
        input.forceCompositionEnd();
      });

      on(div, "input", function() {
        if (input.composing) return;
        if (!input.pollContent())
          runInOp(input.cm, function() {regChange(cm);});
      });

      function onCopyCut(e) {
		logger.debug(`>>> Entering onCopyCut(e=${e})`);
        if (cm.somethingSelected()) {
          lastCopied = cm.getSelections();
          if (e.type == "cut") cm.replaceSelection("", null, "cut");
        } else if (!cm.options.lineWiseCopyCut) {
          return;
        } else {
          var ranges = copyableRanges(cm);
		logger.debug(`onCopyCut(e=${e}): ranges → ${ranges}`);
          lastCopied = ranges.text;
          if (e.type == "cut") {
            cm.operation(function() {
			logger.debug('>>> Entering operation()');
              cm.setSelections(ranges.ranges, 0, sel_dontScroll);
              cm.replaceSelection("", null, "cut");
            });
			logger.debug('<<< Exiting operation()');
          }
        }
        // iOS exposes the clipboard API, but seems to discard content inserted into it
        if (e.clipboardData && !ios) {
          e.preventDefault();
          e.clipboardData.clearData();
          e.clipboardData.setData("text/plain", lastCopied.join("\n"));
        } else {
          // Old-fashioned briefly-focus-a-textarea hack
          var kludge = hiddenTextarea(), te = kludge.firstChild;
		logger.debug(`onCopyCut(e=${e}): kludge → ${kludge}`);
          cm.display.lineSpace.insertBefore(kludge, cm.display.lineSpace.firstChild);
          te.value = lastCopied.join("\n");
          var hadFocus = document.activeElement;
          selectInput(te);
          setTimeout(function() {
            cm.display.lineSpace.removeChild(kludge);
            hadFocus.focus();
          }, 50);
        }
		logger.debug(`<<< Exiting onCopyCut(e=${e})`);
      }
      on(div, "copy", onCopyCut);
      on(div, "cut", onCopyCut);
    },

    prepareSelection: function() {
      var result = prepareSelection(this.cm, false);
      result.focus = this.cm.state.focused;
	logger.debug(`>>> Entering prepareSelection(cm=${cm},primary=${primary})`);
	logger.debug(`<<< Exiting prepareSelection(cm=${cm},primary=${primary})`);
      return result;
    },

    showSelection: function(info) {
      if (!info || !this.cm.display.view.length) return;
      if (info.focus) this.showPrimarySelection();
      this.showMultipleSelections(info);
    },

    showPrimarySelection: function() {
	logger.debug('>>> Entering showPrimarySelection()');
      var sel = window.getSelection(), prim = this.cm.doc.sel.primary();
      var curAnchor = domToPos(this.cm, sel.anchorNode, sel.anchorOffset);
	logger.debug(`showPrimarySelection(): sel → ${sel}`);
      var curFocus = domToPos(this.cm, sel.focusNode, sel.focusOffset);
      if (curAnchor && !curAnchor.bad && curFocus && !curFocus.bad &&
          cmp(minPos(curAnchor, curFocus), prim.from()) == 0 &&
          cmp(maxPos(curAnchor, curFocus), prim.to()) == 0)
		logger.debug(`showPrimarySelection(): curAnchor → ${curAnchor}`);
	logger.debug(`showPrimarySelection(): curFocus → ${curFocus}`);
        return;

      var start = posToDOM(this.cm, prim.from());
      var end = posToDOM(this.cm, prim.to());
      if (!start && !end) return;

      var view = this.cm.display.view;
      var old = sel.rangeCount && sel.getRangeAt(0);
      if (!start) {
        start = {node: view[0].measure.map[2], offset: 0};
      } else if (!end) { // FIXME dangerously hacky
        var measure = view[view.length - 1].measure;
        var map = measure.maps ? measure.maps[measure.maps.length - 1] : measure.map;
        end = {node: map[map.length - 1], offset: map[map.length - 2] - map[map.length - 3]};
      }

      try { var rng = range(start.node, start.offset, end.offset, end.node); }
      catch(e) {} // Our model of the DOM might be outdated, in which case the range we try to set can be impossible
	logger.error('Exception in showPrimarySelection():', e.message, e);
      if (rng) {
        sel.removeAllRanges();
        sel.addRange(rng);
        if (old && sel.anchorNode == null) sel.addRange(old);
        else if (gecko) this.startGracePeriod();
      }
      this.rememberSelection();
	logger.debug('<<< Exiting showPrimarySelection()');
    },

    startGracePeriod: function() {
	logger.debug('>>> Entering startGracePeriod()');
      var input = this;
      clearTimeout(this.gracePeriod);
	logger.debug('<<< Exiting startGracePeriod()');
      this.gracePeriod = setTimeout(function() {
        input.gracePeriod = false;
        if (input.selectionChanged())
          input.cm.operation(function() { input.cm.curOp.selectionChanged = true; });
      }, 20);
	logger.debug(`startGracePeriod(): input.gracePeriod → ${input.gracePeriod}`);
    },

    showMultipleSelections: function(info) {
	logger.debug(`>>> Entering showMultipleSelections(info=${info})`);
      removeChildrenAndAdd(this.cm.display.cursorDiv, info.cursors);
      removeChildrenAndAdd(this.cm.display.selectionDiv, info.selection);
	logger.debug(`<<< Exiting showMultipleSelections(info=${info})`);
    },

    rememberSelection: function() {
	logger.debug('>>> Entering rememberSelection()');
      var sel = window.getSelection();
      this.lastAnchorNode = sel.anchorNode; this.lastAnchorOffset = sel.anchorOffset;
	logger.debug(`rememberSelection(): sel → ${sel}`);
	logger.debug(`rememberSelection(): this.lastAnchorNode → ${this.lastAnchorNode}`);
      this.lastFocusNode = sel.focusNode; this.lastFocusOffset = sel.focusOffset;
	logger.debug(`rememberSelection(): this.lastFocusNode → ${this.lastFocusNode}`);
	logger.debug('<<< Exiting rememberSelection()');
    },

    selectionInEditor: function() {
      var sel = window.getSelection();
	logger.debug('>>> Entering selectionInEditor()');
      if (!sel.rangeCount) return false;
      var node = sel.getRangeAt(0).commonAncestorContainer;
	logger.debug('<<< Exiting selectionInEditor()');
	logger.debug(`selectionInEditor(): node → ${node}`);
      return contains(this.div, node);
    },

    focus: function() {
      if (this.cm.options.readOnly != "nocursor") this.div.focus();
    },
    blur: function() { this.div.blur(); },
    getField: function() { return this.div; },

    supportsTouch: function() { return true; },

    receivedFocus: function() {
      var input = this;
      if (this.selectionInEditor())
        this.pollSelection();
      else
        runInOp(this.cm, function() { input.cm.curOp.selectionChanged = true; });

      function poll() {
        if (input.cm.state.focused) {
          input.pollSelection();
          input.polling.set(input.cm.options.pollInterval, poll);
        }
      }
      this.polling.set(this.cm.options.pollInterval, poll);
    },

    selectionChanged: function() {
	logger.debug('>>> Entering selectionChanged()');
      var sel = window.getSelection();
      return sel.anchorNode != this.lastAnchorNode || sel.anchorOffset != this.lastAnchorOffset ||
        sel.focusNode != this.lastFocusNode || sel.focusOffset != this.lastFocusOffset;
		logger.debug('<<< Exiting selectionChanged()');
    },

    pollSelection: function() {
	logger.debug('>>> Entering pollSelection()');
      if (!this.composing && !this.gracePeriod && this.selectionChanged()) {
        var sel = window.getSelection(), cm = this.cm;
		logger.debug(`pollSelection(): sel → ${sel}`);
        this.rememberSelection();
        var anchor = domToPos(cm, sel.anchorNode, sel.anchorOffset);
		logger.debug(`pollSelection(): anchor → ${anchor}`);
        var head = domToPos(cm, sel.focusNode, sel.focusOffset);
        if (anchor && head) runInOp(cm, function() {
		logger.debug('<<< Exiting pollSelection()');
          setSelection(cm.doc, simpleSelection(anchor, head), sel_dontScroll);
          if (anchor.bad || head.bad) cm.curOp.selectionChanged = true;
        });
      }
    },

    pollContent: function() {
	logger.debug('>>> Entering pollContent()');
      var cm = this.cm, display = cm.display, sel = cm.doc.sel.primary();
      var from = sel.from(), to = sel.to();
	logger.debug(`pollContent(): cm → ${cm}`);
      if (from.line < display.viewFrom || to.line > display.viewTo - 1) return false;

      var fromIndex;
      if (from.line == display.viewFrom || (fromIndex = findViewIndex(cm, from.line)) == 0) {
        var fromLine = lineNo(display.view[0].line);
        var fromNode = display.view[0].node;
      } else {
        var fromLine = lineNo(display.view[fromIndex].line);
		logger.debug(`pollContent(): fromLine → ${fromLine}`);
        var fromNode = display.view[fromIndex - 1].node.nextSibling;
      }
      var toIndex = findViewIndex(cm, to.line);
	logger.debug(`pollContent(): toIndex → ${toIndex}`);
      if (toIndex == display.view.length - 1) {
        var toLine = display.viewTo - 1;
        var toNode = display.lineDiv.lastChild;
      } else {
        var toLine = lineNo(display.view[toIndex + 1].line) - 1;
		logger.debug(`pollContent(): toLine → ${toLine}`);
        var toNode = display.view[toIndex + 1].node.previousSibling;
      }

      var newText = cm.doc.splitLines(domTextBetween(cm, fromNode, toNode, fromLine, toLine));
	logger.debug(`pollContent(): newText → ${newText}`);
      var oldText = getBetween(cm.doc, Pos(fromLine, 0), Pos(toLine, getLine(cm.doc, toLine).text.length));
      while (newText.length > 1 && oldText.length > 1) {
		logger.debug(`pollContent(): oldText → ${oldText}`);
        if (lst(newText) == lst(oldText)) { newText.pop(); oldText.pop(); toLine--; }
        else if (newText[0] == oldText[0]) { newText.shift(); oldText.shift(); fromLine++; }
        else break;
      }

      var cutFront = 0, cutEnd = 0;
      var newTop = newText[0], oldTop = oldText[0], maxCutFront = Math.min(newTop.length, oldTop.length);
      while (cutFront < maxCutFront && newTop.charCodeAt(cutFront) == oldTop.charCodeAt(cutFront))
        ++cutFront;
      var newBot = lst(newText), oldBot = lst(oldText);
      var maxCutEnd = Math.min(newBot.length - (newText.length == 1 ? cutFront : 0),
                               oldBot.length - (oldText.length == 1 ? cutFront : 0));
      while (cutEnd < maxCutEnd &&
             newBot.charCodeAt(newBot.length - cutEnd - 1) == oldBot.charCodeAt(oldBot.length - cutEnd - 1))
        ++cutEnd;

      newText[newText.length - 1] = newBot.slice(0, newBot.length - cutEnd);
      newText[0] = newText[0].slice(cutFront);

      var chFrom = Pos(fromLine, cutFront);
      var chTo = Pos(toLine, oldText.length ? lst(oldText).length - cutEnd : 0);
      if (newText.length > 1 || newText[0] || cmp(chFrom, chTo)) {
        replaceRange(cm.doc, newText, chFrom, chTo, "+input");
        return true;
      }
    },

    ensurePolled: function() {
      this.forceCompositionEnd();
    },
    reset: function() {
      this.forceCompositionEnd();
    },
    forceCompositionEnd: function() {
	logger.debug('>>> Entering forceCompositionEnd()');
      if (!this.composing || this.composing.handled) return;
      this.applyComposition(this.composing);
      this.composing.handled = true;
      this.div.blur();
      this.div.focus();
	logger.debug('<<< Exiting forceCompositionEnd()');
    },
    applyComposition: function(composing) {
	logger.debug(`>>> Entering applyComposition(composing=${composing})`);
      if (composing.data && composing.data != composing.startData)
        operation(this.cm, applyTextInput)(this.cm, composing.data, 0, composing.sel);
		logger.debug(`<<< Exiting applyComposition(composing=${composing})`);
    },

    setUneditable: function(node) {
	logger.debug(`>>> Entering setUneditable(node=${node})`);
      node.setAttribute("contenteditable", "false");
	logger.debug(`<<< Exiting setUneditable(node=${node})`);
    },

    onKeyPress: function(e) {
      e.preventDefault();
      operation(this.cm, applyTextInput)(this.cm, String.fromCharCode(e.charCode == null ? e.keyCode : e.charCode), 0);
    },

    onContextMenu: nothing,
    resetPosition: nothing,

    needsContentAttribute: true
  }, ContentEditableInput.prototype);

  function posToDOM(cm, pos) {
	logger.debug(`>>> Entering posToDOM(cm=${cm},pos=${pos})`);
    var view = findViewForLine(cm, pos.line);
    if (!view || view.hidden) return null;
	logger.debug(`posToDOM(cm=${cm},pos=${pos}): view → ${view}`);
    var line = getLine(cm.doc, pos.line);
	logger.debug(`posToDOM(cm=${cm},pos=${pos}): line → ${line}`);
    var info = mapFromLineView(view, line, pos.line);
	logger.debug(`posToDOM(cm=${cm},pos=${pos}): info → ${info}`);

    var order = getOrder(line), side = "left";
	logger.debug(`posToDOM(cm=${cm},pos=${pos}): order → ${order}`);
    if (order) {
      var partPos = getBidiPartAt(order, pos.ch);
	logger.debug(`posToDOM(cm=${cm},pos=${pos}): partPos → ${partPos}`);
      side = partPos % 2 ? "right" : "left";
    }
    var result = nodeAndOffsetInLineMap(info.map, pos.ch, side);
    result.offset = result.collapse == "right" ? result.end : result.start;
	logger.debug(`posToDOM(cm=${cm},pos=${pos}): result → ${result}`);
	logger.debug(`<<< Exiting posToDOM(cm=${cm},pos=${pos})`);
    return result;
  }

  function badPos(pos, bad) { if (bad) pos.bad = true; return pos; }

  function domToPos(cm, node, offset) {
	logger.debug(`>>> Entering domToPos(cm=${cm},node=${node},offset=${offset})`);
    var lineNode;
    if (node == cm.display.lineDiv) {
      lineNode = cm.display.lineDiv.childNodes[offset];
      if (!lineNode) return badPos(cm.clipPos(Pos(cm.display.viewTo - 1)), true);
	logger.debug(`domToPos(cm=${cm},node=${node},offset=${offset}): lineNode → ${lineNode}`);
      node = null; offset = 0;
    } else {
      for (lineNode = node;; lineNode = lineNode.parentNode) {
        if (!lineNode || lineNode == cm.display.lineDiv) return null;
        if (lineNode.parentNode && lineNode.parentNode == cm.display.lineDiv) break;
      }
    }
    for (var i = 0; i < cm.display.view.length; i++) {
      var lineView = cm.display.view[i];
      if (lineView.node == lineNode)
		logger.debug(`<<< Exiting domToPos(cm=${cm},node=${node},offset=${offset})`);
        return locateNodeInLineView(lineView, node, offset);
    }
  }

  function locateNodeInLineView(lineView, node, offset) {
	logger.debug(`>>> Entering locateNodeInLineView(lineView=${lineView},node=${node},offset=${offset})`);
    var wrapper = lineView.text.firstChild, bad = false;
    if (!node || !contains(wrapper, node)) return badPos(Pos(lineNo(lineView.line), 0), true);
    if (node == wrapper) {
      bad = true;
      node = wrapper.childNodes[offset];
      offset = 0;
      if (!node) {
        var line = lineView.rest ? lst(lineView.rest) : lineView.line;
		logger.debug(`locateNodeInLineView(lineView=${lineView},node=${node},offset=${offset}): line → ${line}`);
        return badPos(Pos(lineNo(line), line.text.length), bad);
      }
    }

    var textNode = node.nodeType == 3 ? node : null, topNode = node;
    if (!textNode && node.childNodes.length == 1 && node.firstChild.nodeType == 3) {
      textNode = node.firstChild;
      if (offset) offset = textNode.nodeValue.length;
    }
    while (topNode.parentNode != wrapper) topNode = topNode.parentNode;
    var measure = lineView.measure, maps = measure.maps;

    function find(textNode, topNode, offset) {
	logger.debug(`>>> Entering find(textNode=${textNode},topNode=${topNode},offset=${offset})`);
      for (var i = -1; i < (maps ? maps.length : 0); i++) {
        var map = i < 0 ? measure.map : maps[i];
        for (var j = 0; j < map.length; j += 3) {
          var curNode = map[j + 2];
          if (curNode == textNode || curNode == topNode) {
            var line = lineNo(i < 0 ? lineView.line : lineView.rest[i]);
			logger.debug(`find(textNode=${textNode},topNode=${topNode},offset=${offset}): line → ${line}`);
            var ch = map[j] + offset;
            if (offset < 0 || curNode != textNode) ch = map[j + (offset ? 1 : 0)];
			logger.debug(`<<< Exiting find(textNode=${textNode},topNode=${topNode},offset=${offset})`);
            return Pos(line, ch);
          }
        }
      }
    }
    var found = find(textNode, topNode, offset);
    if (found) return badPos(found, bad);

    // FIXME this is all really shaky. might handle the few cases it needs to handle, but likely to cause problems
    for (var after = topNode.nextSibling, dist = textNode ? textNode.nodeValue.length - offset : 0; after; after = after.nextSibling) {
      found = find(after, after.firstChild, 0);
      if (found)
        return badPos(Pos(found.line, found.ch - dist), bad);
      else
        dist += after.textContent.length;
    }
    for (var before = topNode.previousSibling, dist = offset; before; before = before.previousSibling) {
      found = find(before, before.firstChild, -1);
      if (found)
        return badPos(Pos(found.line, found.ch + dist), bad);
      else
        dist += after.textContent.length;
    }
	logger.debug(`<<< Exiting locateNodeInLineView(lineView=${lineView},node=${node},offset=${offset})`);
  }

  function domTextBetween(cm, from, to, fromLine, toLine) {
	logger.debug(`>>> Entering domTextBetween(cm=${cm},from=${from},to=${to},fromLine=${fromLine},toLine=${toLine})`);
    var text = "", closing = false, lineSep = cm.doc.lineSeparator();
    function recognizeMarker(id) { return function(marker) { return marker.id == id; }; }
    function walk(node) {
	logger.debug(`>>> Entering walk(node=${node})`);
      if (node.nodeType == 1) {
        var cmText = node.getAttribute("cm-text");
		logger.debug(`walk(node=${node}): cmText → ${cmText}`);
        if (cmText != null) {
          if (cmText == "") cmText = node.textContent.replace(/\u200b/g, "");
          text += cmText;
          return;
        }
        var markerID = node.getAttribute("cm-marker"), range;
		logger.debug(`walk(node=${node}): markerID → ${markerID}`);
        if (markerID) {
          var found = cm.findMarks(Pos(fromLine, 0), Pos(toLine + 1, 0), recognizeMarker(+markerID));
		logger.debug(`walk(node=${node}): found → ${found}`);
          if (found.length && (range = found[0].find()))
            text += getBetween(cm.doc, range.from, range.to).join(lineSep);
          return;
        }
        if (node.getAttribute("contenteditable") == "false") return;
        for (var i = 0; i < node.childNodes.length; i++)
          walk(node.childNodes[i]);
        if (/^(pre|div|p)$/i.test(node.nodeName))
          closing = true;
      } else if (node.nodeType == 3) {
        var val = node.nodeValue;
        if (!val) return;
        if (closing) {
          text += lineSep;
          closing = false;
        }
        text += val;
		logger.debug(`walk(node=${node}): closing → ${closing}`);
      }
	logger.debug(`<<< Exiting walk(node=${node})`);
    }
    for (;;) {
      walk(from);
      if (from == to) break;
      from = from.nextSibling;
    }
	logger.debug(`<<< Exiting domTextBetween(cm=${cm},from=${from},to=${to},fromLine=${fromLine},toLine=${toLine})`);
    return text;
  }

  CodeMirror.inputStyles = {"textarea": TextareaInput, "contenteditable": ContentEditableInput};

  // SELECTION / CURSOR

  // Selection objects are immutable. A new one is created every time
  // the selection changes. A selection is one or more non-overlapping
  // (and non-touching) ranges, sorted, and an integer that indicates
  // which one is the primary selection (the one that's scrolled into
  // view, that getCursor returns, etc).
  function Selection(ranges, primIndex) {
	logger.debug(`>>> Entering Selection(ranges=${ranges},primIndex=${primIndex})`);
    this.ranges = ranges;
    this.primIndex = primIndex;
	logger.debug(`Selection(ranges=${ranges},primIndex=${primIndex}): this.ranges → ${this.ranges}`);
	logger.debug(`Selection(ranges=${ranges},primIndex=${primIndex}): this.primIndex → ${this.primIndex}`);
	logger.debug(`<<< Exiting Selection(ranges=${ranges},primIndex=${primIndex})`);
  }

  Selection.prototype = {
    primary: function() { return this.ranges[this.primIndex]; },
    equals: function(other) {
      if (other == this) return true;
      if (other.primIndex != this.primIndex || other.ranges.length != this.ranges.length) return false;
      for (var i = 0; i < this.ranges.length; i++) {
        var here = this.ranges[i], there = other.ranges[i];
        if (cmp(here.anchor, there.anchor) != 0 || cmp(here.head, there.head) != 0) return false;
		logger.debug(`<<< Exiting equals(other=${other})`);
      }
      return true;
    },
    deepCopy: function() {
	logger.debug('>>> Entering deepCopy()');
      for (var out = [], i = 0; i < this.ranges.length; i++)
        out[i] = new Range(copyPos(this.ranges[i].anchor), copyPos(this.ranges[i].head));
	logger.debug('<<< Exiting deepCopy()');
      return new Selection(out, this.primIndex);
    },
    somethingSelected: function() {
	logger.debug('>>> Entering somethingSelected()');
      for (var i = 0; i < this.ranges.length; i++)
        if (!this.ranges[i].empty()) return true;
	logger.debug('<<< Exiting somethingSelected()');
      return false;
    },
    contains: function(pos, end) {
      if (!end) end = pos;
      for (var i = 0; i < this.ranges.length; i++) {
        var range = this.ranges[i];
        if (cmp(end, range.from()) >= 0 && cmp(pos, range.to()) <= 0)
          return i;
      }
	logger.debug(`<<< Exiting contains(pos=${pos},end=${end})`);
      return -1;
    }
  };

  function Range(anchor, head) {
	logger.debug(`>>> Entering Range(anchor=${anchor},head=${head})`);
    this.anchor = anchor; this.head = head;
	logger.debug(`Range(anchor=${anchor},head=${head}): anchor → ${anchor}`);
	logger.debug(`<<< Exiting Range(anchor=${anchor},head=${head})`);
  }

  Range.prototype = {
    from: function() { return minPos(this.anchor, this.head); },
    to: function() { return maxPos(this.anchor, this.head); },
    empty: function() {
	logger.debug('>>> Entering empty()');
	logger.debug('<<< Exiting empty()');
      return this.head.line == this.anchor.line && this.head.ch == this.anchor.ch;
    }
  };

  // Take an unsorted, potentially overlapping set of ranges, and
  // build a selection out of it. 'Consumes' ranges array (modifying
  // it).
  function normalizeSelection(ranges, primIndex) {
	logger.debug(`>>> Entering normalizeSelection(ranges=${ranges},primIndex=${primIndex})`);
    var prim = ranges[primIndex];
    ranges.sort(function(a, b) { return cmp(a.from(), b.from()); });
    primIndex = indexOf(ranges, prim);
    for (var i = 1; i < ranges.length; i++) {
      var cur = ranges[i], prev = ranges[i - 1];
      if (cmp(prev.to(), cur.from()) >= 0) {
        var from = minPos(prev.from(), cur.from()), to = maxPos(prev.to(), cur.to());
        var inv = prev.empty() ? cur.from() == cur.head : prev.from() == prev.head;
		logger.debug(`normalizeSelection(ranges=${ranges},primIndex=${primIndex}): inv → ${inv}`);
        if (i <= primIndex) --primIndex;
        ranges.splice(--i, 2, new Range(inv ? to : from, inv ? from : to));
      }
    }
	logger.debug(`normalizeSelection(ranges=${ranges},primIndex=${primIndex}): primIndex → ${primIndex}`);
	logger.debug(`<<< Exiting normalizeSelection(ranges=${ranges},primIndex=${primIndex})`);
    return new Selection(ranges, primIndex);
  }

  function simpleSelection(anchor, head) {
	logger.debug(`>>> Entering simpleSelection(anchor=${anchor},head=${head})`);
	logger.debug(`<<< Exiting simpleSelection(anchor=${anchor},head=${head})`);
    return new Selection([new Range(anchor, head || anchor)], 0);
  }

  // Most of the external API clips given positions to make sure they
  // actually exist within the document.
  function clipLine(doc, n) {return Math.max(doc.first, Math.min(n, doc.first + doc.size - 1));}
  function clipPos(doc, pos) {
	logger.debug(`>>> Entering clipPos(doc=${doc},pos=${pos})`);
    if (pos.line < doc.first) return Pos(doc.first, 0);
    var last = doc.first + doc.size - 1;
    if (pos.line > last) return Pos(last, getLine(doc, last).text.length);
	logger.debug(`<<< Exiting clipPos(doc=${doc},pos=${pos})`);
    return clipToLen(pos, getLine(doc, pos.line).text.length);
  }
  function clipToLen(pos, linelen) {
	logger.debug(`>>> Entering clipToLen(pos=${pos},linelen=${linelen})`);
    var ch = pos.ch;
    if (ch == null || ch > linelen) return Pos(pos.line, linelen);
    else if (ch < 0) return Pos(pos.line, 0);
    else return pos;
	logger.debug(`<<< Exiting clipToLen(pos=${pos},linelen=${linelen})`);
  }
  function isLine(doc, l) {return l >= doc.first && l < doc.first + doc.size;}
  function clipPosArray(doc, array) {
	logger.debug(`>>> Entering clipPosArray(doc=${doc},array=${array})`);
    for (var out = [], i = 0; i < array.length; i++) out[i] = clipPos(doc, array[i]);
	logger.debug(`clipPosArray(doc=${doc},array=${array}): out → ${out}`);
	logger.debug(`<<< Exiting clipPosArray(doc=${doc},array=${array})`);
    return out;
  }

  // SELECTION UPDATES

  // The 'scroll' parameter given to many of these indicated whether
  // the new cursor position should be scrolled into view after
  // modifying the selection.

  // If shift is held or the extend flag is set, extends a range to
  // include a given position (and optionally a second position).
  // Otherwise, simply returns the range between the given positions.
  // Used for cursor motion and such.
  function extendRange(doc, range, head, other) {
	logger.debug(`>>> Entering extendRange(doc=${doc},range=${range},head=${head},other=${other})`);
    if (doc.cm && doc.cm.display.shift || doc.extend) {
      var anchor = range.anchor;
      if (other) {
        var posBefore = cmp(head, anchor) < 0;
		logger.debug(`extendRange(doc=${doc},range=${range},head=${head},other=${other}): posBefore → ${posBefore}`);
        if (posBefore != (cmp(other, anchor) < 0)) {
          anchor = head;
          head = other;
        } else if (posBefore != (cmp(head, other) < 0)) {
          head = other;
        }
      }
      return new Range(anchor, head);
    } else {
	logger.debug(`<<< Exiting extendRange(doc=${doc},range=${range},head=${head},other=${other})`);
      return new Range(other || head, head);
    }
  }

  // Extend the primary selection range, discard the rest.
  function extendSelection(doc, head, other, options) {
	logger.debug(`>>> Entering extendSelection(doc=${doc},head=${head},other=${other},options=${options})`);
    setSelection(doc, new Selection([extendRange(doc, doc.sel.primary(), head, other)], 0), options);
	logger.debug(`<<< Exiting extendSelection(doc=${doc},head=${head},other=${other},options=${options})`);
  }

  // Extend all selections (pos is an array of selections with length
  // equal the number of selections)
  function extendSelections(doc, heads, options) {
	logger.debug(`>>> Entering extendSelections(doc=${doc},heads=${heads},options=${options})`);
    for (var out = [], i = 0; i < doc.sel.ranges.length; i++)
      out[i] = extendRange(doc, doc.sel.ranges[i], heads[i], null);
    var newSel = normalizeSelection(out, doc.sel.primIndex);
    setSelection(doc, newSel, options);
	logger.debug(`extendSelections(doc=${doc},heads=${heads},options=${options}): newSel → ${newSel}`);
	logger.debug(`<<< Exiting extendSelections(doc=${doc},heads=${heads},options=${options})`);
  }

  // Updates a single range in the selection.
  function replaceOneSelection(doc, i, range, options) {
	logger.debug(`>>> Entering replaceOneSelection(doc=${doc},i=${i},range=${range},options=${options})`);
    var ranges = doc.sel.ranges.slice(0);
    ranges[i] = range;
	logger.debug(`replaceOneSelection(doc=${doc},i=${i},range=${range},options=${options}): ranges → ${ranges}`);
    setSelection(doc, normalizeSelection(ranges, doc.sel.primIndex), options);
	logger.debug(`<<< Exiting replaceOneSelection(doc=${doc},i=${i},range=${range},options=${options})`);
  }

  // Reset the selection to a single range.
  function setSimpleSelection(doc, anchor, head, options) {
	logger.debug(`>>> Entering setSimpleSelection(doc=${doc},anchor=${anchor},head=${head},options=${options})`);
    setSelection(doc, simpleSelection(anchor, head), options);
	logger.debug(`<<< Exiting setSimpleSelection(doc=${doc},anchor=${anchor},head=${head},options=${options})`);
  }

  // Give beforeSelectionChange handlers a change to influence a
  // selection update.
  function filterSelectionChange(doc, sel) {
	logger.debug(`>>> Entering filterSelectionChange(doc=${doc},sel=${sel})`);
    var obj = {
      ranges: sel.ranges,
      update: function(ranges) {
		logger.debug(`>>> Entering update(ranges=${ranges})`);
        this.ranges = [];
        for (var i = 0; i < ranges.length; i++)
		logger.debug(`filterSelectionChange(doc=${doc},sel=${sel}): this.ranges → ${this.ranges}`);
          this.ranges[i] = new Range(clipPos(doc, ranges[i].anchor),
                                     clipPos(doc, ranges[i].head));
									logger.debug(`<<< Exiting update(ranges=${ranges})`);
      }
    };
    signal(doc, "beforeSelectionChange", doc, obj);
    if (doc.cm) signal(doc.cm, "beforeSelectionChange", doc.cm, obj);
    if (obj.ranges != sel.ranges) return normalizeSelection(obj.ranges, obj.ranges.length - 1);
    else return sel;
	logger.debug(`<<< Exiting filterSelectionChange(doc=${doc},sel=${sel})`);
  }

  function setSelectionReplaceHistory(doc, sel, options) {
	logger.debug(`>>> Entering setSelectionReplaceHistory(doc=${doc},sel=${sel},options=${options})`);
    var done = doc.history.done, last = lst(done);
    if (last && last.ranges) {
      done[done.length - 1] = sel;
      setSelectionNoUndo(doc, sel, options);
    } else {
      setSelection(doc, sel, options);
	logger.debug(`<<< Exiting setSelectionReplaceHistory(doc=${doc},sel=${sel},options=${options})`);
	logger.debug(`>>> Entering done(e=${e})`);
	logger.debug(`<<< Exiting done(e=${e})`);
    }
  }

  // Set a new selection.
  function setSelection(doc, sel, options) {
	logger.debug(`>>> Entering setSelection(doc=${doc},sel=${sel},options=${options})`);
    setSelectionNoUndo(doc, sel, options);
    addSelectionToHistory(doc, doc.sel, doc.cm ? doc.cm.curOp.id : NaN, options);
	logger.debug(`<<< Exiting setSelection(doc=${doc},sel=${sel},options=${options})`);
  }

  function setSelectionNoUndo(doc, sel, options) {
	logger.debug(`>>> Entering setSelectionNoUndo(doc=${doc},sel=${sel},options=${options})`);
    if (hasHandler(doc, "beforeSelectionChange") || doc.cm && hasHandler(doc.cm, "beforeSelectionChange"))
      sel = filterSelectionChange(doc, sel);

    var bias = options && options.bias ||
      (cmp(sel.primary().head, doc.sel.primary().head) < 0 ? -1 : 1);
	logger.debug(`setSelectionNoUndo(doc=${doc},sel=${sel},options=${options}): sel → ${sel}`);
    setSelectionInner(doc, skipAtomicInSelection(doc, sel, bias, true));

    if (!(options && options.scroll === false) && doc.cm)
	logger.debug(`<<< Exiting setSelectionNoUndo(doc=${doc},sel=${sel},options=${options})`);
      ensureCursorVisible(doc.cm);
  }

  function setSelectionInner(doc, sel) {
	logger.debug(`>>> Entering setSelectionInner(doc=${doc},sel=${sel})`);
    if (sel.equals(doc.sel)) return;

    doc.sel = sel;
	logger.debug(`setSelectionInner(doc=${doc},sel=${sel}): doc.sel → ${doc.sel}`);

    if (doc.cm) {
      doc.cm.curOp.updateInput = doc.cm.curOp.selectionChanged = true;
      signalCursorActivity(doc.cm);
    }
    signalLater(doc, "cursorActivity", doc);
	logger.debug(`<<< Exiting setSelectionInner(doc=${doc},sel=${sel})`);
  }

  // Verify that the selection does not partially select any atomic
  // marked ranges.
  function reCheckSelection(doc) {
	logger.debug(`>>> Entering reCheckSelection(doc=${doc})`);
    setSelectionInner(doc, skipAtomicInSelection(doc, doc.sel, null, false), sel_dontScroll);
	logger.debug(`<<< Exiting reCheckSelection(doc=${doc})`);
  }

  // Return a selection that does not partially select any atomic
  // ranges.
  function skipAtomicInSelection(doc, sel, bias, mayClear) {
	logger.debug(`>>> Entering skipAtomicInSelection(doc=${doc},sel=${sel},bias=${bias},mayClear=${mayClear})`);
    var out;
    for (var i = 0; i < sel.ranges.length; i++) {
      var range = sel.ranges[i];
      var newAnchor = skipAtomic(doc, range.anchor, bias, mayClear);
	logger.debug(`skipAtomicInSelection(doc=${doc},sel=${sel},bias=${bias},mayClear=${mayClear}): newAnchor → ${newAnchor}`);
      var newHead = skipAtomic(doc, range.head, bias, mayClear);
      if (out || newAnchor != range.anchor || newHead != range.head) {
        if (!out) out = sel.ranges.slice(0, i);
        out[i] = new Range(newAnchor, newHead);
      }
    }
	logger.debug(`skipAtomicInSelection(doc=${doc},sel=${sel},bias=${bias},mayClear=${mayClear}): newHead → ${newHead}`);
	logger.debug(`<<< Exiting skipAtomicInSelection(doc=${doc},sel=${sel},bias=${bias},mayClear=${mayClear})`);
    return out ? normalizeSelection(out, sel.primIndex) : sel;
  }

  // Ensure a given position is not inside an atomic range.
  function skipAtomic(doc, pos, bias, mayClear) {
	logger.debug(`>>> Entering skipAtomic(doc=${doc},pos=${pos},bias=${bias},mayClear=${mayClear})`);
    var flipped = false, curPos = pos;
    var dir = bias || 1;
    doc.cantEdit = false;
	logger.debug(`skipAtomic(doc=${doc},pos=${pos},bias=${bias},mayClear=${mayClear}): doc.cantEdit → ${doc.cantEdit}`);
    search: for (;;) {
      var line = getLine(doc, curPos.line);
      if (line.markedSpans) {
        for (var i = 0; i < line.markedSpans.length; ++i) {
          var sp = line.markedSpans[i], m = sp.marker;
          if ((sp.from == null || (m.inclusiveLeft ? sp.from <= curPos.ch : sp.from < curPos.ch)) &&
              (sp.to == null || (m.inclusiveRight ? sp.to >= curPos.ch : sp.to > curPos.ch))) {
            if (mayClear) {
              signal(m, "beforeCursorEnter");
              if (m.explicitlyCleared) {
                if (!line.markedSpans) break;
                else {--i; continue;}
              }
            }
            if (!m.atomic) continue;
            var newPos = m.find(dir < 0 ? -1 : 1);
			logger.debug(`skipAtomic(doc=${doc},pos=${pos},bias=${bias},mayClear=${mayClear}): newPos → ${newPos}`);
            if (cmp(newPos, curPos) == 0) {
              newPos.ch += dir;
              if (newPos.ch < 0) {
                if (newPos.line > doc.first) newPos = clipPos(doc, Pos(newPos.line - 1));
                else newPos = null;
              } else if (newPos.ch > line.text.length) {
                if (newPos.line < doc.first + doc.size - 1) newPos = Pos(newPos.line + 1, 0);
                else newPos = null;
              }
              if (!newPos) {
                if (flipped) {
                  // Driven in a corner -- no valid cursor position found at all
                  // -- try again *with* clearing, if we didn't already
                  if (!mayClear) return skipAtomic(doc, pos, bias, true);
                  // Otherwise, turn off editing until further notice, and return the start of the doc
                  doc.cantEdit = true;
                  return Pos(doc.first, 0);
                }
                flipped = true; newPos = pos; dir = -dir;
              }
            }
            curPos = newPos;
            continue search;
          }
        }
      }
	logger.debug(`skipAtomic(doc=${doc},pos=${pos},bias=${bias},mayClear=${mayClear}): line → ${line}`);
	logger.debug(`<<< Exiting skipAtomic(doc=${doc},pos=${pos},bias=${bias},mayClear=${mayClear})`);
      return curPos;
    }
  }

  // SELECTION DRAWING

  function updateSelection(cm) {
	logger.debug(`>>> Entering updateSelection(cm=${cm})`);
    cm.display.input.showSelection(cm.display.input.prepareSelection());
	logger.debug(`<<< Exiting updateSelection(cm=${cm})`);
  }

  function prepareSelection(cm, primary) {
    var doc = cm.doc, result = {};
    var curFragment = result.cursors = document.createDocumentFragment();
    var selFragment = result.selection = document.createDocumentFragment();
	logger.debug(`prepareSelection(cm=${cm},primary=${primary}): curFragment → ${curFragment}`);
	logger.debug(`prepareSelection(cm=${cm},primary=${primary}): selFragment → ${selFragment}`);

    for (var i = 0; i < doc.sel.ranges.length; i++) {
      if (primary === false && i == doc.sel.primIndex) continue;
      var range = doc.sel.ranges[i];
      var collapsed = range.empty();
	logger.debug(`prepareSelection(cm=${cm},primary=${primary}): collapsed → ${collapsed}`);
      if (collapsed || cm.options.showCursorWhenSelecting)
        drawSelectionCursor(cm, range.head, curFragment);
      if (!collapsed)
        drawSelectionRange(cm, range, selFragment);
    }
    return result;
  }

  // Draws a cursor for the given range
  function drawSelectionCursor(cm, head, output) {
	logger.debug(`>>> Entering drawSelectionCursor(cm=${cm},head=${head},output=${output})`);
    var pos = cursorCoords(cm, head, "div", null, null, !cm.options.singleCursorHeightPerLine);

	logger.debug(`drawSelectionCursor(cm=${cm},head=${head},output=${output}): pos → ${pos}`);
    var cursor = output.appendChild(elt("div", "\u00a0", "CodeMirror-cursor"));
	logger.debug(`drawSelectionCursor(cm=${cm},head=${head},output=${output}): cursor → ${cursor}`);
    cursor.style.left = pos.left + "px";
    cursor.style.top = pos.top + "px";
    cursor.style.height = Math.max(0, pos.bottom - pos.top) * cm.options.cursorHeight + "px";

    if (pos.other) {
      // Secondary cursor, shown when on a 'jump' in bi-directional text
      var otherCursor = output.appendChild(elt("div", "\u00a0", "CodeMirror-cursor CodeMirror-secondarycursor"));
	logger.debug(`drawSelectionCursor(cm=${cm},head=${head},output=${output}): otherCursor → ${otherCursor}`);
      otherCursor.style.display = "";
      otherCursor.style.left = pos.other.left + "px";
      otherCursor.style.top = pos.other.top + "px";
      otherCursor.style.height = (pos.other.bottom - pos.other.top) * .85 + "px";
	logger.debug(`<<< Exiting drawSelectionCursor(cm=${cm},head=${head},output=${output})`);
    }
  }

  // Draws the given range as a highlighted selection
  function drawSelectionRange(cm, range, output) {
	logger.debug(`>>> Entering drawSelectionRange(cm=${cm},range=${range},output=${output})`);
    var display = cm.display, doc = cm.doc;
    var fragment = document.createDocumentFragment();
	logger.debug(`drawSelectionRange(cm=${cm},range=${range},output=${output}): fragment → ${fragment}`);
    var padding = paddingH(cm.display), leftSide = padding.left;
    var rightSide = Math.max(display.sizerWidth, displayWidth(cm) - display.sizer.offsetLeft) - padding.right;
	logger.debug(`drawSelectionRange(cm=${cm},range=${range},output=${output}): padding → ${padding}`);
	logger.debug(`drawSelectionRange(cm=${cm},range=${range},output=${output}): rightSide → ${rightSide}`);

    function add(left, top, width, bottom) {
      if (top < 0) top = 0;
	logger.debug(`>>> Entering add(left=${left},top=${top},width=${width},bottom=${bottom})`);
      top = Math.round(top);
      bottom = Math.round(bottom);
	logger.debug(`add(left=${left},top=${top},width=${width},bottom=${bottom}): top → ${top}`);
      fragment.appendChild(elt("div", null, "CodeMirror-selected", "position: absolute; left: " + left +
                               "px; top: " + top + "px; width: " + (width == null ? rightSide - left : width) +
                               "px; height: " + (bottom - top) + "px"));
							logger.debug(`add(left=${left},top=${top},width=${width},bottom=${bottom}): bottom → ${bottom}`);
							logger.debug(`<<< Exiting add(left=${left},top=${top},width=${width},bottom=${bottom})`);
    }

    function drawForLine(line, fromArg, toArg) {
	logger.debug(`>>> Entering drawForLine(line=${line},fromArg=${fromArg},toArg=${toArg})`);
      var lineObj = getLine(doc, line);
      var lineLen = lineObj.text.length;
	logger.debug(`drawForLine(line=${line},fromArg=${fromArg},toArg=${toArg}): lineObj → ${lineObj}`);
      var start, end;
	logger.debug(`<<< Exiting drawForLine(line=${line},fromArg=${fromArg},toArg=${toArg})`);
      function coords(ch, bias) {
		logger.debug(`>>> Entering coords(ch=${ch},bias=${bias})`);
		logger.debug(`<<< Exiting coords(ch=${ch},bias=${bias})`);
        return charCoords(cm, Pos(line, ch), "div", lineObj, bias);
      }

      iterateBidiSections(getOrder(lineObj), fromArg || 0, toArg == null ? lineLen : toArg, function(from, to, dir) {
        var leftPos = coords(from, "left"), rightPos, left, right;
        if (from == to) {
		logger.debug(`function(toArg=${toArg},leftPos=${leftPos}): leftPos → ${leftPos}`);
          rightPos = leftPos;
          left = right = leftPos.left;
        } else {
          rightPos = coords(to - 1, "right");
		logger.debug(`function(toArg=${toArg},leftPos=${leftPos}): rightPos → ${rightPos}`);
          if (dir == "rtl") { var tmp = leftPos; leftPos = rightPos; rightPos = tmp; }
          left = leftPos.left;
          right = rightPos.right;
        }
        if (fromArg == null && from == 0) left = leftSide;
        if (rightPos.top - leftPos.top > 3) { // Different lines, draw top part
          add(left, leftPos.top, null, leftPos.bottom);
          left = leftSide;
          if (leftPos.bottom < rightPos.top) add(left, leftPos.bottom, null, rightPos.top);
        }
        if (toArg == null && to == lineLen) right = rightSide;
        if (!start || leftPos.top < start.top || leftPos.top == start.top && leftPos.left < start.left)
          start = leftPos;
        if (!end || rightPos.bottom > end.bottom || rightPos.bottom == end.bottom && rightPos.right > end.right)
          end = rightPos;
        if (left < leftSide + 1) left = leftSide;
        add(left, rightPos.top, right - left, rightPos.bottom);
      });
	logger.debug(`>>> Entering getOrder(line=${line})`);
	logger.debug(`<<< Exiting getOrder(line=${line})`);
      return {start: start, end: end};
    }

    var sFrom = range.from(), sTo = range.to();
    if (sFrom.line == sTo.line) {
      drawForLine(sFrom.line, sFrom.ch, sTo.ch);
    } else {
      var fromLine = getLine(doc, sFrom.line), toLine = getLine(doc, sTo.line);
      var singleVLine = visualLine(fromLine) == visualLine(toLine);
      var leftEnd = drawForLine(sFrom.line, sFrom.ch, singleVLine ? fromLine.text.length + 1 : null).end;
      var rightStart = drawForLine(sTo.line, singleVLine ? 0 : null, sTo.ch).start;
      if (singleVLine) {
        if (leftEnd.top < rightStart.top - 2) {
          add(leftEnd.right, leftEnd.top, null, leftEnd.bottom);
          add(leftSide, rightStart.top, rightStart.left, rightStart.bottom);
        } else {
          add(leftEnd.right, leftEnd.top, rightStart.left - leftEnd.right, leftEnd.bottom);
        }
      }
      if (leftEnd.bottom < rightStart.top)
        add(leftSide, leftEnd.bottom, null, rightStart.top);
    }

    output.appendChild(fragment);
	logger.debug(`<<< Exiting drawSelectionRange(cm=${cm},range=${range},output=${output})`);
  }

  // Cursor-blinking
  function restartBlink(cm) {
	logger.debug(`>>> Entering restartBlink(cm=${cm})`);
    if (!cm.state.focused) return;
    var display = cm.display;
    clearInterval(display.blinker);
    var on = true;
    display.cursorDiv.style.visibility = "";
    if (cm.options.cursorBlinkRate > 0)
      display.blinker = setInterval(function() {
		logger.debug('>>> Entering setInterval()');
		logger.debug('<<< Exiting setInterval()');
        display.cursorDiv.style.visibility = (on = !on) ? "" : "hidden";
      }, cm.options.cursorBlinkRate);
    else if (cm.options.cursorBlinkRate < 0)
      display.cursorDiv.style.visibility = "hidden";
	logger.debug(`<<< Exiting restartBlink(cm=${cm})`);
  }

  // HIGHLIGHT WORKER

  function startWorker(cm, time) {
	logger.debug(`>>> Entering startWorker(cm=${cm},time=${time})`);
    if (cm.doc.mode.startState && cm.doc.frontier < cm.display.viewTo)
      cm.state.highlight.set(time, bind(highlightWorker, cm));
	logger.debug(`<<< Exiting startWorker(cm=${cm},time=${time})`);
  }

  function highlightWorker(cm) {
	logger.debug(`>>> Entering highlightWorker(cm=${cm})`);
    var doc = cm.doc;
    if (doc.frontier < doc.first) doc.frontier = doc.first;
    if (doc.frontier >= cm.display.viewTo) return;
    var end = +new Date + cm.options.workTime;
    var state = copyState(doc.mode, getStateBefore(cm, doc.frontier));
    var changedLines = [];

    doc.iter(doc.frontier, Math.min(doc.first + doc.size, cm.display.viewTo + 500), function(line) {
      if (doc.frontier >= cm.display.viewFrom) { // Visible
        var oldStyles = line.styles, tooLong = line.text.length > cm.options.maxHighlightLength;
        var highlighted = highlightLine(cm, line, tooLong ? copyState(doc.mode, state) : state, true);
        line.styles = highlighted.styles;
		logger.debug(`function(newCls=${newCls}): highlighted → ${highlighted}`);
        var oldCls = line.styleClasses, newCls = highlighted.classes;
        if (newCls) line.styleClasses = newCls;
        else if (oldCls) line.styleClasses = null;
        var ischange = !oldStyles || oldStyles.length != line.styles.length ||
          oldCls != newCls && (!oldCls || !newCls || oldCls.bgClass != newCls.bgClass || oldCls.textClass != newCls.textClass);
		logger.debug(`function(newCls=${newCls}): line.styles → ${line.styles}`);
        for (var i = 0; !ischange && i < oldStyles.length; ++i) ischange = oldStyles[i] != line.styles[i];
        if (ischange) changedLines.push(doc.frontier);
        line.stateAfter = tooLong ? state : copyState(doc.mode, state);
      } else {
        if (line.text.length <= cm.options.maxHighlightLength)
          processLine(cm, line.text, state);
        line.stateAfter = doc.frontier % 5 == 0 ? copyState(doc.mode, state) : null;
      }
      ++doc.frontier;
      if (+new Date > end) {
        startWorker(cm, cm.options.workDelay);
        return true;
      }
    });
    if (changedLines.length) runInOp(cm, function() {
      for (var i = 0; i < changedLines.length; i++)
        regLineChange(cm, changedLines[i], "text");
    });
	logger.debug(`<<< Exiting highlightWorker(cm=${cm})`);
  }

  // Finds the line to start with when starting a parse. Tries to
  // find a line with a stateAfter, so that it can start with a
  // valid state. If that fails, it returns the line with the
  // smallest indentation, which tends to need the least context to
  // parse correctly.
  function findStartLine(cm, n, precise) {
	logger.debug(`>>> Entering findStartLine(cm=${cm},n=${n},precise=${precise})`);
    var minindent, minline, doc = cm.doc;
    var lim = precise ? -1 : n - (cm.doc.mode.innerMode ? 1000 : 100);
    for (var search = n; search > lim; --search) {
      if (search <= doc.first) return doc.first;
      var line = getLine(doc, search - 1);
	logger.debug(`findStartLine(cm=${cm},n=${n},precise=${precise}): line → ${line}`);
      if (line.stateAfter && (!precise || search <= doc.frontier)) return search;
      var indented = countColumn(line.text, null, cm.options.tabSize);
      if (minline == null || minindent > indented) {
        minline = search - 1;
        minindent = indented;
      }
    }
	logger.debug(`findStartLine(cm=${cm},n=${n},precise=${precise}): lim → ${lim}`);
	logger.debug(`findStartLine(cm=${cm},n=${n},precise=${precise}): indented → ${indented}`);
	logger.debug(`findStartLine(cm=${cm},n=${n},precise=${precise}): minindent → ${minindent}`);
	logger.debug(`<<< Exiting findStartLine(cm=${cm},n=${n},precise=${precise})`);
    return minline;
  }

  function getStateBefore(cm, n, precise) {
    var doc = cm.doc, display = cm.display;
	logger.debug(`>>> Entering getStateBefore(cm=${cm},n=${n},precise=${precise})`);
    if (!doc.mode.startState) return true;
    var pos = findStartLine(cm, n, precise), state = pos > doc.first && getLine(doc, pos-1).stateAfter;
	logger.debug(`getStateBefore(cm=${cm},n=${n},precise=${precise}): pos → ${pos}`);
    if (!state) state = startState(doc.mode);
    else state = copyState(doc.mode, state);
    doc.iter(pos, n, function(line) {
      processLine(cm, line.text, state);
      var save = pos == n - 1 || pos % 5 == 0 || pos >= display.viewFrom && pos < display.viewTo;
      line.stateAfter = save ? copyState(doc.mode, state) : null;
      ++pos;
    });
    if (precise) doc.frontier = pos;
	logger.debug(`<<< Exiting getStateBefore(cm=${cm},n=${n},precise=${precise})`);
    return state;
  }

  // POSITION MEASUREMENT

  function paddingTop(display) {return display.lineSpace.offsetTop;}
  function paddingVert(display) {return display.mover.offsetHeight - display.lineSpace.offsetHeight;}
  function paddingH(display) {
	logger.debug(`>>> Entering paddingH(display=${display})`);
    if (display.cachedPaddingH) return display.cachedPaddingH;
    var e = removeChildrenAndAdd(display.measure, elt("pre", "x"));
	logger.debug(`paddingH(display=${display}): e → ${e}`);
    var style = window.getComputedStyle ? window.getComputedStyle(e) : e.currentStyle;
	logger.debug(`paddingH(display=${display}): style → ${style}`);
    var data = {left: parseInt(style.paddingLeft), right: parseInt(style.paddingRight)};
	logger.debug(`paddingH(display=${display}): data → ${data}`);
    if (!isNaN(data.left) && !isNaN(data.right)) display.cachedPaddingH = data;
	logger.debug(`<<< Exiting paddingH(display=${display})`);
    return data;
  }

  function scrollGap(cm) { return scrollerGap - cm.display.nativeBarWidth; }
  function displayWidth(cm) {
	logger.debug(`>>> Entering displayWidth(cm=${cm})`);
	logger.debug(`<<< Exiting displayWidth(cm=${cm})`);
    return cm.display.scroller.clientWidth - scrollGap(cm) - cm.display.barWidth;
  }
  function displayHeight(cm) {
	logger.debug(`>>> Entering displayHeight(cm=${cm})`);
	logger.debug(`<<< Exiting displayHeight(cm=${cm})`);
    return cm.display.scroller.clientHeight - scrollGap(cm) - cm.display.barHeight;
  }

  // Ensure the lineView.wrapping.heights array is populated. This is
  // an array of bottom offsets for the lines that make up a drawn
  // line. When lineWrapping is on, there might be more than one
  // height.
  function ensureLineHeights(cm, lineView, rect) {
	logger.debug(`>>> Entering ensureLineHeights(cm=${cm},lineView=${lineView},rect=${rect})`);
    var wrapping = cm.options.lineWrapping;
    var curWidth = wrapping && displayWidth(cm);
	logger.debug(`ensureLineHeights(cm=${cm},lineView=${lineView},rect=${rect}): curWidth → ${curWidth}`);
    if (!lineView.measure.heights || wrapping && lineView.measure.width != curWidth) {
      var heights = lineView.measure.heights = [];
      if (wrapping) {
        lineView.measure.width = curWidth;
        var rects = lineView.text.firstChild.getClientRects();
        for (var i = 0; i < rects.length - 1; i++) {
          var cur = rects[i], next = rects[i + 1];
          if (Math.abs(cur.bottom - next.bottom) > 2)
            heights.push((cur.bottom + next.top) / 2 - rect.top);
        }
      }
      heights.push(rect.bottom - rect.top);
	logger.debug(`ensureLineHeights(cm=${cm},lineView=${lineView},rect=${rect}): rects → ${rects}`);
    }
	logger.debug(`<<< Exiting ensureLineHeights(cm=${cm},lineView=${lineView},rect=${rect})`);
  }

  // Find a line map (mapping character offsets to text nodes) and a
  // measurement cache for the given line number. (A line view might
  // contain multiple lines when collapsed ranges are present.)
  function mapFromLineView(lineView, line, lineN) {
	logger.debug(`>>> Entering mapFromLineView(lineView=${lineView},line=${line},lineN=${lineN})`);
    if (lineView.line == line)
      return {map: lineView.measure.map, cache: lineView.measure.cache};
    for (var i = 0; i < lineView.rest.length; i++)
      if (lineView.rest[i] == line)
        return {map: lineView.measure.maps[i], cache: lineView.measure.caches[i]};
    for (var i = 0; i < lineView.rest.length; i++)
      if (lineNo(lineView.rest[i]) > lineN)
		logger.debug(`<<< Exiting mapFromLineView(lineView=${lineView},line=${line},lineN=${lineN})`);
        return {map: lineView.measure.maps[i], cache: lineView.measure.caches[i], before: true};
  }

  // Render a line into the hidden node display.externalMeasured. Used
  // when measurement is needed for a line that's not in the viewport.
  function updateExternalMeasurement(cm, line) {
	logger.debug(`>>> Entering updateExternalMeasurement(cm=${cm},line=${line})`);
    line = visualLine(line);
    var lineN = lineNo(line);
	logger.debug(`updateExternalMeasurement(cm=${cm},line=${line}): line → ${line}`);
	logger.debug(`updateExternalMeasurement(cm=${cm},line=${line}): lineN → ${lineN}`);
    var view = cm.display.externalMeasured = new LineView(cm.doc, line, lineN);
    view.lineN = lineN;
	logger.debug(`updateExternalMeasurement(cm=${cm},line=${line}): view → ${view}`);
    var built = view.built = buildLineContent(cm, view);
	logger.debug(`updateExternalMeasurement(cm=${cm},line=${line}): built → ${built}`);
    view.text = built.pre;
    removeChildrenAndAdd(cm.display.lineMeasure, built.pre);
	logger.debug(`updateExternalMeasurement(cm=${cm},line=${line}): view.lineN → ${view.lineN}`);
	logger.debug(`updateExternalMeasurement(cm=${cm},line=${line}): view.text → ${view.text}`);
	logger.debug(`<<< Exiting updateExternalMeasurement(cm=${cm},line=${line})`);
    return view;
  }

  // Get a {top, bottom, left, right} box (in line-local coordinates)
  // for a given character.
  function measureChar(cm, line, ch, bias) {
	logger.debug(`>>> Entering measureChar(cm=${cm},line=${line},ch=${ch},bias=${bias})`);
	logger.debug(`<<< Exiting measureChar(cm=${cm},line=${line},ch=${ch},bias=${bias})`);
    return measureCharPrepared(cm, prepareMeasureForLine(cm, line), ch, bias);
  }

  // Find a line view that corresponds to the given line number.
  function findViewForLine(cm, lineN) {
	logger.debug(`>>> Entering findViewForLine(cm=${cm},lineN=${lineN})`);
    if (lineN >= cm.display.viewFrom && lineN < cm.display.viewTo)
      return cm.display.view[findViewIndex(cm, lineN)];
    var ext = cm.display.externalMeasured;
    if (ext && lineN >= ext.lineN && lineN < ext.lineN + ext.size)
	logger.debug(`<<< Exiting findViewForLine(cm=${cm},lineN=${lineN})`);
      return ext;
  }

  // Measurement can be split in two steps, the set-up work that
  // applies to the whole line, and the measurement of the actual
  // character. Functions like coordsChar, that need to do a lot of
  // measurements in a row, can thus ensure that the set-up work is
  // only done once.
  function prepareMeasureForLine(cm, line) {
	logger.debug(`>>> Entering prepareMeasureForLine(cm=${cm},line=${line})`);
    var lineN = lineNo(line);
    var view = findViewForLine(cm, lineN);
	logger.debug(`prepareMeasureForLine(cm=${cm},line=${line}): lineN → ${lineN}`);
	logger.debug(`prepareMeasureForLine(cm=${cm},line=${line}): view → ${view}`);
    if (view && !view.text) {
      view = null;
    } else if (view && view.changes) {
      updateLineForChanges(cm, view, lineN, getDimensions(cm));
      cm.curOp.forceUpdate = true;
    }
    if (!view)
      view = updateExternalMeasurement(cm, line);

    var info = mapFromLineView(view, line, lineN);
	logger.debug(`<<< Exiting prepareMeasureForLine(cm=${cm},line=${line})`);
    return {
      line: line, view: view, rect: null,
      map: info.map, cache: info.cache, before: info.before,
      hasHeights: false
    };
  }

  // Given a prepared measurement object, measures the position of an
  // actual character (or fetches it from the cache).
  function measureCharPrepared(cm, prepared, ch, bias, varHeight) {
	logger.debug(`>>> Entering measureCharPrepared(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias},varHeight=${varHeight})`);
    if (prepared.before) ch = -1;
    var key = ch + (bias || ""), found;
	logger.warn(`measureCharPrepared(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias},varHeight=${varHeight}): key → ${key}`);
    if (prepared.cache.hasOwnProperty(key)) {
      found = prepared.cache[key];
    } else {
      if (!prepared.rect)
        prepared.rect = prepared.view.text.getBoundingClientRect();
      if (!prepared.hasHeights) {
        ensureLineHeights(cm, prepared.view, prepared.rect);
        prepared.hasHeights = true;
		logger.debug(`measureCharPrepared(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias},varHeight=${varHeight}): prepared.hasHeights → ${prepared.hasHeights}`);
      }
      found = measureCharInner(cm, prepared, ch, bias);
      if (!found.bogus) prepared.cache[key] = found;
    }
	logger.debug(`<<< Exiting measureCharPrepared(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias},varHeight=${varHeight})`);
    return {left: found.left, right: found.right,
            top: varHeight ? found.rtop : found.top,
            bottom: varHeight ? found.rbottom : found.bottom};
  }

  var nullRect = {left: 0, right: 0, top: 0, bottom: 0};

  function nodeAndOffsetInLineMap(map, ch, bias) {
	logger.debug(`>>> Entering nodeAndOffsetInLineMap(map=${map},ch=${ch},bias=${bias})`);
    var node, start, end, collapse;
    // First, search the line map for the text node corresponding to,
    // or closest to, the target character.
    for (var i = 0; i < map.length; i += 3) {
      var mStart = map[i], mEnd = map[i + 1];
      if (ch < mStart) {
        start = 0; end = 1;
        collapse = "left";
      } else if (ch < mEnd) {
        start = ch - mStart;
        end = start + 1;
      } else if (i == map.length - 3 || ch == mEnd && map[i + 3] > ch) {
        end = mEnd - mStart;
        start = end - 1;
        if (ch >= mEnd) collapse = "right";
      }
      if (start != null) {
        node = map[i + 2];
        if (mStart == mEnd && bias == (node.insertLeft ? "left" : "right"))
          collapse = bias;
        if (bias == "left" && start == 0)
          while (i && map[i - 2] == map[i - 3] && map[i - 1].insertLeft) {
            node = map[(i -= 3) + 2];
            collapse = "left";
          }
        if (bias == "right" && start == mEnd - mStart)
          while (i < map.length - 3 && map[i + 3] == map[i + 4] && !map[i + 5].insertLeft) {
            node = map[(i += 3) + 2];
            collapse = "right";
          }
        break;
      }
    }
	logger.debug(`<<< Exiting nodeAndOffsetInLineMap(map=${map},ch=${ch},bias=${bias})`);
    return {node: node, start: start, end: end, collapse: collapse, coverStart: mStart, coverEnd: mEnd};
  }

  function measureCharInner(cm, prepared, ch, bias) {
	logger.debug(`>>> Entering measureCharInner(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias})`);
    var place = nodeAndOffsetInLineMap(prepared.map, ch, bias);
    var node = place.node, start = place.start, end = place.end, collapse = place.collapse;
	logger.debug(`measureCharInner(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias}): place → ${place}`);

    var rect;
    if (node.nodeType == 3) { // If it is a text node, use a range to retrieve the coordinates.
      for (var i = 0; i < 4; i++) { // Retry a maximum of 4 times when nonsense rectangles are returned
        while (start && isExtendingChar(prepared.line.text.charAt(place.coverStart + start))) --start;
        while (place.coverStart + end < place.coverEnd && isExtendingChar(prepared.line.text.charAt(place.coverStart + end))) ++end;
        if (ie && ie_version < 9 && start == 0 && end == place.coverEnd - place.coverStart) {
          rect = node.parentNode.getBoundingClientRect();
        } else if (ie && cm.options.lineWrapping) {
          var rects = range(node, start, end).getClientRects();
		logger.debug(`measureCharInner(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias}): rects → ${rects}`);
          if (rects.length)
            rect = rects[bias == "right" ? rects.length - 1 : 0];
          else
            rect = nullRect;
        } else {
          rect = range(node, start, end).getBoundingClientRect() || nullRect;
        }
        if (rect.left || rect.right || start == 0) break;
        end = start;
        start = start - 1;
        collapse = "right";
      }
      if (ie && ie_version < 11) rect = maybeUpdateRectForZooming(cm.display.measure, rect);
    } else { // If it is a widget, simply get the box for the whole widget.
      if (start > 0) collapse = bias = "right";
      var rects;
      if (cm.options.lineWrapping && (rects = node.getClientRects()).length > 1)
        rect = rects[bias == "right" ? rects.length - 1 : 0];
      else
        rect = node.getBoundingClientRect();
    }
    if (ie && ie_version < 9 && !start && (!rect || !rect.left && !rect.right)) {
      var rSpan = node.parentNode.getClientRects()[0];
	logger.debug(`measureCharInner(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias}): rSpan → ${rSpan}`);
      if (rSpan)
        rect = {left: rSpan.left, right: rSpan.left + charWidth(cm.display), top: rSpan.top, bottom: rSpan.bottom};
      else
        rect = nullRect;
    }

    var rtop = rect.top - prepared.rect.top, rbot = rect.bottom - prepared.rect.top;
    var mid = (rtop + rbot) / 2;
	logger.debug(`measureCharInner(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias}): mid → ${mid}`);
    var heights = prepared.view.measure.heights;
    for (var i = 0; i < heights.length - 1; i++)
      if (mid < heights[i]) break;
    var top = i ? heights[i - 1] : 0, bot = heights[i];
    var result = {left: (collapse == "right" ? rect.right : rect.left) - prepared.rect.left,
                  right: (collapse == "left" ? rect.left : rect.right) - prepared.rect.left,
                  top: top, bottom: bot};
    if (!rect.left && !rect.right) result.bogus = true;
	logger.debug(`measureCharInner(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias}): result → ${result}`);
    if (!cm.options.singleCursorHeightPerLine) { result.rtop = rtop; result.rbottom = rbot; }

	logger.debug(`<<< Exiting measureCharInner(cm=${cm},prepared=${prepared},ch=${ch},bias=${bias})`);
    return result;
  }

  // Work around problem with bounding client rects on ranges being
  // returned incorrectly when zoomed on IE10 and below.
  function maybeUpdateRectForZooming(measure, rect) {
	logger.debug(`>>> Entering maybeUpdateRectForZooming(measure=${measure},rect=${rect})`);
    if (!window.screen || screen.logicalXDPI == null ||
        screen.logicalXDPI == screen.deviceXDPI || !hasBadZoomedRects(measure))
      return rect;
    var scaleX = screen.logicalXDPI / screen.deviceXDPI;
    var scaleY = screen.logicalYDPI / screen.deviceYDPI;
	logger.debug(`<<< Exiting maybeUpdateRectForZooming(measure=${measure},rect=${rect})`);
    return {left: rect.left * scaleX, right: rect.right * scaleX,
            top: rect.top * scaleY, bottom: rect.bottom * scaleY};
  }

  function clearLineMeasurementCacheFor(lineView) {
	logger.debug(`>>> Entering clearLineMeasurementCacheFor(lineView=${lineView})`);
    if (lineView.measure) {
      lineView.measure.cache = {};
      lineView.measure.heights = null;
      if (lineView.rest) for (var i = 0; i < lineView.rest.length; i++)
		logger.debug(`<<< Exiting clearLineMeasurementCacheFor(lineView=${lineView})`);
        lineView.measure.caches[i] = {};
    }
  }

  function clearLineMeasurementCache(cm) {
	logger.debug(`>>> Entering clearLineMeasurementCache(cm=${cm})`);
    cm.display.externalMeasure = null;
    removeChildren(cm.display.lineMeasure);
    for (var i = 0; i < cm.display.view.length; i++)
	logger.debug(`<<< Exiting clearLineMeasurementCache(cm=${cm})`);
      clearLineMeasurementCacheFor(cm.display.view[i]);
  }

  function clearCaches(cm) {
	logger.debug(`>>> Entering clearCaches(cm=${cm})`);
    clearLineMeasurementCache(cm);
    cm.display.cachedCharWidth = cm.display.cachedTextHeight = cm.display.cachedPaddingH = null;
    if (!cm.options.lineWrapping) cm.display.maxLineChanged = true;
    cm.display.lineNumChars = null;
	logger.debug(`<<< Exiting clearCaches(cm=${cm})`);
  }

  function pageScrollX() { return window.pageXOffset || (document.documentElement || document.body).scrollLeft; }
  function pageScrollY() { return window.pageYOffset || (document.documentElement || document.body).scrollTop; }

  // Converts a {top, bottom, left, right} box from line-local
  // coordinates into another coordinate system. Context may be one of
  // "line", "div" (display.lineDiv), "local"/null (editor), "window",
  // or "page".
  function intoCoordSystem(cm, lineObj, rect, context) {
	logger.debug(`>>> Entering intoCoordSystem(cm=${cm},lineObj=${lineObj},rect=${rect},context=${context})`);
    if (lineObj.widgets) for (var i = 0; i < lineObj.widgets.length; ++i) if (lineObj.widgets[i].above) {
      var size = widgetHeight(lineObj.widgets[i]);
	logger.debug(`intoCoordSystem(cm=${cm},lineObj=${lineObj},rect=${rect},context=${context}): size → ${size}`);
      rect.top += size; rect.bottom += size;
    }
    if (context == "line") return rect;
    if (!context) context = "local";
    var yOff = heightAtLine(lineObj);
	logger.debug(`intoCoordSystem(cm=${cm},lineObj=${lineObj},rect=${rect},context=${context}): yOff → ${yOff}`);
    if (context == "local") yOff += paddingTop(cm.display);
    else yOff -= cm.display.viewOffset;
    if (context == "page" || context == "window") {
      var lOff = cm.display.lineSpace.getBoundingClientRect();
	logger.debug(`intoCoordSystem(cm=${cm},lineObj=${lineObj},rect=${rect},context=${context}): lOff → ${lOff}`);
      yOff += lOff.top + (context == "window" ? 0 : pageScrollY());
      var xOff = lOff.left + (context == "window" ? 0 : pageScrollX());
	logger.debug(`intoCoordSystem(cm=${cm},lineObj=${lineObj},rect=${rect},context=${context}): xOff → ${xOff}`);
      rect.left += xOff; rect.right += xOff;
    }
    rect.top += yOff; rect.bottom += yOff;
	logger.debug(`<<< Exiting intoCoordSystem(cm=${cm},lineObj=${lineObj},rect=${rect},context=${context})`);
    return rect;
  }

  // Coverts a box from "div" coords to another coordinate system.
  // Context may be "window", "page", "div", or "local"/null.
  function fromCoordSystem(cm, coords, context) {
	logger.debug(`>>> Entering fromCoordSystem(cm=${cm},coords=${coords},context=${context})`);
    if (context == "div") return coords;
    var left = coords.left, top = coords.top;
    // First move into "page" coordinate system
    if (context == "page") {
      left -= pageScrollX();
      top -= pageScrollY();
    } else if (context == "local" || !context) {
      var localBox = cm.display.sizer.getBoundingClientRect();
	logger.debug(`fromCoordSystem(cm=${cm},coords=${coords},context=${context}): localBox → ${localBox}`);
      left += localBox.left;
      top += localBox.top;
    }

    var lineSpaceBox = cm.display.lineSpace.getBoundingClientRect();
	logger.debug(`fromCoordSystem(cm=${cm},coords=${coords},context=${context}): lineSpaceBox → ${lineSpaceBox}`);
	logger.debug(`<<< Exiting fromCoordSystem(cm=${cm},coords=${coords},context=${context})`);
    return {left: left - lineSpaceBox.left, top: top - lineSpaceBox.top};
  }

  function charCoords(cm, pos, context, lineObj, bias) {
	logger.debug(`>>> Entering charCoords(cm=${cm},pos=${pos},context=${context},lineObj=${lineObj},bias=${bias})`);
    if (!lineObj) lineObj = getLine(cm.doc, pos.line);
	logger.debug(`<<< Exiting charCoords(cm=${cm},pos=${pos},context=${context},lineObj=${lineObj},bias=${bias})`);
    return intoCoordSystem(cm, lineObj, measureChar(cm, lineObj, pos.ch, bias), context);
  }

  // Returns a box for a given cursor position, which may have an
  // 'other' property containing the position of the secondary cursor
  // on a bidi boundary.
  function cursorCoords(cm, pos, context, lineObj, preparedMeasure, varHeight) {
	logger.debug(`>>> Entering cursorCoords(cm=${cm},pos=${pos},context=${context},lineObj=${lineObj},preparedMeasure=${preparedMeasure},varHeight=${varHeight})`);
    lineObj = lineObj || getLine(cm.doc, pos.line);
    if (!preparedMeasure) preparedMeasure = prepareMeasureForLine(cm, lineObj);
	logger.debug(`cursorCoords(cm=${cm},pos=${pos},context=${context},lineObj=${lineObj},preparedMeasure=${preparedMeasure},varHeight=${varHeight}): lineObj → ${lineObj}`);
    function get(ch, right) {
	logger.debug(`>>> Entering get(ch=${ch},right=${right})`);
      var m = measureCharPrepared(cm, preparedMeasure, ch, right ? "right" : "left", varHeight);
      if (right) m.left = m.right; else m.right = m.left;
	logger.debug(`get(ch=${ch},right=${right}): m → ${m}`);
	logger.debug(`<<< Exiting get(ch=${ch},right=${right})`);
      return intoCoordSystem(cm, lineObj, m, context);
    }
    function getBidi(ch, partPos) {
	logger.debug(`>>> Entering getBidi(ch=${ch},partPos=${partPos})`);
      var part = order[partPos], right = part.level % 2;
      if (ch == bidiLeft(part) && partPos && part.level < order[partPos - 1].level) {
        part = order[--partPos];
        ch = bidiRight(part) - (part.level % 2 ? 0 : 1);
        right = true;
      } else if (ch == bidiRight(part) && partPos < order.length - 1 && part.level < order[partPos + 1].level) {
        part = order[++partPos];
        ch = bidiLeft(part) - part.level % 2;
        right = false;
      }
      if (right && ch == part.to && ch > part.from) return get(ch - 1);
	logger.debug(`getBidi(ch=${ch},partPos=${partPos}): ch → ${ch}`);
	logger.debug(`<<< Exiting getBidi(ch=${ch},partPos=${partPos})`);
      return get(ch, right);
    }
    var order = getOrder(lineObj), ch = pos.ch;
	logger.debug(`cursorCoords(cm=${cm},pos=${pos},context=${context},lineObj=${lineObj},preparedMeasure=${preparedMeasure},varHeight=${varHeight}): order → ${order}`);
    if (!order) return get(ch);
    var partPos = getBidiPartAt(order, ch);
	logger.debug(`cursorCoords(cm=${cm},pos=${pos},context=${context},lineObj=${lineObj},preparedMeasure=${preparedMeasure},varHeight=${varHeight}): partPos → ${partPos}`);
    var val = getBidi(ch, partPos);
	logger.debug(`cursorCoords(cm=${cm},pos=${pos},context=${context},lineObj=${lineObj},preparedMeasure=${preparedMeasure},varHeight=${varHeight}): val → ${val}`);
    if (bidiOther != null) val.other = getBidi(ch, bidiOther);
	logger.debug(`<<< Exiting cursorCoords(cm=${cm},pos=${pos},context=${context},lineObj=${lineObj},preparedMeasure=${preparedMeasure},varHeight=${varHeight})`);
    return val;
  }

  // Used to cheaply estimate the coordinates for a position. Used for
  // intermediate scroll updates.
  function estimateCoords(cm, pos) {
	logger.debug(`>>> Entering estimateCoords(cm=${cm},pos=${pos})`);
    var left = 0, pos = clipPos(cm.doc, pos);
    if (!cm.options.lineWrapping) left = charWidth(cm.display) * pos.ch;
	logger.debug(`estimateCoords(cm=${cm},pos=${pos}): left → ${left}`);
    var lineObj = getLine(cm.doc, pos.line);
	logger.debug(`estimateCoords(cm=${cm},pos=${pos}): lineObj → ${lineObj}`);
    var top = heightAtLine(lineObj) + paddingTop(cm.display);
	logger.debug(`estimateCoords(cm=${cm},pos=${pos}): top → ${top}`);
	logger.debug(`<<< Exiting estimateCoords(cm=${cm},pos=${pos})`);
    return {left: left, right: left, top: top, bottom: top + lineObj.height};
  }

  // Positions returned by coordsChar contain some extra information.
  // xRel is the relative x position of the input coordinates compared
  // to the found position (so xRel > 0 means the coordinates are to
  // the right of the character position, for example). When outside
  // is true, that means the coordinates lie outside the line's
  // vertical range.
  function PosWithInfo(line, ch, outside, xRel) {
	logger.debug(`>>> Entering PosWithInfo(line=${line},ch=${ch},outside=${outside},xRel=${xRel})`);
    var pos = Pos(line, ch);
    pos.xRel = xRel;
	logger.debug(`PosWithInfo(line=${line},ch=${ch},outside=${outside},xRel=${xRel}): pos → ${pos}`);
	logger.debug(`PosWithInfo(line=${line},ch=${ch},outside=${outside},xRel=${xRel}): pos.xRel → ${pos.xRel}`);
    if (outside) pos.outside = true;
	logger.debug(`<<< Exiting PosWithInfo(line=${line},ch=${ch},outside=${outside},xRel=${xRel})`);
    return pos;
  }

  // Compute the character position closest to the given coordinates.
  // Input must be lineSpace-local ("div" coordinate system).
  function coordsChar(cm, x, y) {
	logger.debug(`>>> Entering coordsChar(cm=${cm},x=${x},y=${y})`);
    var doc = cm.doc;
    y += cm.display.viewOffset;
    if (y < 0) return PosWithInfo(doc.first, 0, true, -1);
    var lineN = lineAtHeight(doc, y), last = doc.first + doc.size - 1;
	logger.debug(`coordsChar(cm=${cm},x=${x},y=${y}): lineN → ${lineN}`);
    if (lineN > last)
      return PosWithInfo(doc.first + doc.size - 1, getLine(doc, last).text.length, true, 1);
    if (x < 0) x = 0;

    var lineObj = getLine(doc, lineN);
	logger.debug(`coordsChar(cm=${cm},x=${x},y=${y}): lineObj → ${lineObj}`);
    for (;;) {
      var found = coordsCharInner(cm, lineObj, lineN, x, y);
	logger.debug(`coordsChar(cm=${cm},x=${x},y=${y}): found → ${found}`);
      var merged = collapsedSpanAtEnd(lineObj);
	logger.debug(`coordsChar(cm=${cm},x=${x},y=${y}): merged → ${merged}`);
      var mergedPos = merged && merged.find(0, true);
	logger.debug(`coordsChar(cm=${cm},x=${x},y=${y}): mergedPos → ${mergedPos}`);
      if (merged && (found.ch > mergedPos.from.ch || found.ch == mergedPos.from.ch && found.xRel > 0))
        lineN = lineNo(lineObj = mergedPos.to.line);
      else
		logger.debug(`<<< Exiting coordsChar(cm=${cm},x=${x},y=${y})`);
        return found;
    }
  }

  function coordsCharInner(cm, lineObj, lineNo, x, y) {
	logger.debug(`>>> Entering coordsCharInner(cm=${cm},lineObj=${lineObj},lineNo=${lineNo},x=${x},y=${y})`);
    var innerOff = y - heightAtLine(lineObj);
    var wrongLine = false, adjust = 2 * cm.display.wrapper.clientWidth;
	logger.debug(`coordsCharInner(cm=${cm},lineObj=${lineObj},lineNo=${lineNo},x=${x},y=${y}): innerOff → ${innerOff}`);
    var preparedMeasure = prepareMeasureForLine(cm, lineObj);
	logger.debug(`coordsCharInner(cm=${cm},lineObj=${lineObj},lineNo=${lineNo},x=${x},y=${y}): preparedMeasure → ${preparedMeasure}`);

    function getX(ch) {
	logger.debug(`>>> Entering getX(ch=${ch})`);
      var sp = cursorCoords(cm, Pos(lineNo, ch), "line", lineObj, preparedMeasure);
      wrongLine = true;
	logger.debug(`getX(ch=${ch}): sp → ${sp}`);
      if (innerOff > sp.bottom) return sp.left - adjust;
      else if (innerOff < sp.top) return sp.left + adjust;
      else wrongLine = false;
	logger.debug(`<<< Exiting getX(ch=${ch})`);
      return sp.left;
    }

    var bidi = getOrder(lineObj), dist = lineObj.text.length;
    var from = lineLeft(lineObj), to = lineRight(lineObj);
    var fromX = getX(from), fromOutside = wrongLine, toX = getX(to), toOutside = wrongLine;

    if (x > toX) return PosWithInfo(lineNo, to, toOutside, 1);
    // Do a binary search between these bounds.
    for (;;) {
      if (bidi ? to == from || to == moveVisually(lineObj, from, 1) : to - from <= 1) {
        var ch = x < fromX || x - fromX <= toX - x ? from : to;
        var xDiff = x - (ch == from ? fromX : toX);
        while (isExtendingChar(lineObj.text.charAt(ch))) ++ch;
        var pos = PosWithInfo(lineNo, ch, ch == from ? fromOutside : toOutside,
                              xDiff < -1 ? -1 : xDiff > 1 ? 1 : 0);
        return pos;
      }
      var step = Math.ceil(dist / 2), middle = from + step;
      if (bidi) {
        middle = from;
        for (var i = 0; i < step; ++i) middle = moveVisually(lineObj, middle, 1);
      }
      var middleX = getX(middle);
      if (middleX > x) {to = middle; toX = middleX; if (toOutside = wrongLine) toX += 1000; dist = step;}
      else {from = middle; fromX = middleX; fromOutside = wrongLine; dist -= step;}
    }
	logger.debug(`<<< Exiting coordsCharInner(cm=${cm},lineObj=${lineObj},lineNo=${lineNo},x=${x},y=${y})`);
  }

  var measureText;
  // Compute the default text height.
  function textHeight(display) {
	logger.debug(`>>> Entering textHeight(display=${display})`);
    if (display.cachedTextHeight != null) return display.cachedTextHeight;
    if (measureText == null) {
      measureText = elt("pre");
      // Measure a bunch of lines, for browsers that compute
      // fractional heights.
      for (var i = 0; i < 49; ++i) {
        measureText.appendChild(document.createTextNode("x"));
        measureText.appendChild(elt("br"));
      }
      measureText.appendChild(document.createTextNode("x"));
    }
    removeChildrenAndAdd(display.measure, measureText);
    var height = measureText.offsetHeight / 50;
    if (height > 3) display.cachedTextHeight = height;
    removeChildren(display.measure);
	logger.debug(`<<< Exiting textHeight(display=${display})`);
    return height || 1;
  }

  // Compute the default character width.
  function charWidth(display) {
	logger.debug(`>>> Entering charWidth(display=${display})`);
    if (display.cachedCharWidth != null) return display.cachedCharWidth;
    var anchor = elt("span", "xxxxxxxxxx");
	logger.debug(`charWidth(display=${display}): anchor → ${anchor}`);
    var pre = elt("pre", [anchor]);
	logger.debug(`charWidth(display=${display}): pre → ${pre}`);
    removeChildrenAndAdd(display.measure, pre);
    var rect = anchor.getBoundingClientRect(), width = (rect.right - rect.left) / 10;
	logger.debug(`charWidth(display=${display}): rect → ${rect}`);
    if (width > 2) display.cachedCharWidth = width;
    return width || 10;
  }

  // OPERATIONS

  // Operations are used to wrap a series of changes to the editor
  // state in such a way that each change won't have to update the
  // cursor and display (which would be awkward, slow, and
  // error-prone). Instead, display updates are batched and then all
  // combined and executed at once.

  var operationGroup = null;

  var nextOpId = 0;
  // Start a new operation.
  function startOperation(cm) {
	logger.debug(`>>> Entering startOperation(cm=${cm})`);
    cm.curOp = {
      cm: cm,
      viewChanged: false,      // Flag that indicates that lines might need to be redrawn
      startHeight: cm.doc.height, // Used to detect need to update scrollbar
      forceUpdate: false,      // Used to force a redraw
      updateInput: null,       // Whether to reset the input textarea
      typing: false,           // Whether this reset should be careful to leave existing text (for compositing)
      changeObjs: null,        // Accumulated changes, for firing change events
      cursorActivityHandlers: null, // Set of handlers to fire cursorActivity on
      cursorActivityCalled: 0, // Tracks which cursorActivity handlers have been called already
      selectionChanged: false, // Whether the selection needs to be redrawn
      updateMaxLine: false,    // Set when the widest line needs to be determined anew
      scrollLeft: null, scrollTop: null, // Intermediate scroll position, not pushed to DOM yet
      scrollToPos: null,       // Used to scroll to a specific position
      focus: false,
      id: ++nextOpId           // Unique ID
    };
    if (operationGroup) {
	logger.debug(`startOperation(cm=${cm}): cm.curOp → ${cm.curOp}`);
      operationGroup.ops.push(cm.curOp);
    } else {
	logger.debug(`<<< Exiting startOperation(cm=${cm})`);
      cm.curOp.ownsGroup = operationGroup = {
        ops: [cm.curOp],
        delayedCallbacks: []
      };
    }
  }

  function fireCallbacksForOps(group) {
	logger.debug(`>>> Entering fireCallbacksForOps(group=${group})`);
    // Calls delayed callbacks and cursorActivity handlers until no
    // new ones appear
    var callbacks = group.delayedCallbacks, i = 0;
    do {
      for (; i < callbacks.length; i++)
        callbacks[i].call(null);
      for (var j = 0; j < group.ops.length; j++) {
        var op = group.ops[j];
        if (op.cursorActivityHandlers)
          while (op.cursorActivityCalled < op.cursorActivityHandlers.length)
            op.cursorActivityHandlers[op.cursorActivityCalled++].call(null, op.cm);
      }
    } while (i < callbacks.length);
	logger.debug(`<<< Exiting fireCallbacksForOps(group=${group})`);
  }

  // Finish an operation, updating the display and signalling delayed events
  function endOperation(cm) {
	logger.debug(`>>> Entering endOperation(cm=${cm})`);
    var op = cm.curOp, group = op.ownsGroup;
    if (!group) return;

    try { fireCallbacksForOps(group); }
    finally {
      operationGroup = null;
      for (var i = 0; i < group.ops.length; i++)
        group.ops[i].cm.curOp = null;
      endOperations(group);
	logger.debug(`<<< Exiting endOperation(cm=${cm})`);
    }
  }

  // The DOM updates done when an operation finishes are batched so
  // that the minimum number of relayouts are required.
  function endOperations(group) {
	logger.debug(`>>> Entering endOperations(group=${group})`);
    var ops = group.ops;
    for (var i = 0; i < ops.length; i++) // Read DOM
      endOperation_R1(ops[i]);
    for (var i = 0; i < ops.length; i++) // Write DOM (maybe)
      endOperation_W1(ops[i]);
    for (var i = 0; i < ops.length; i++) // Read DOM
      endOperation_R2(ops[i]);
    for (var i = 0; i < ops.length; i++) // Write DOM (maybe)
      endOperation_W2(ops[i]);
    for (var i = 0; i < ops.length; i++) // Read DOM
      endOperation_finish(ops[i]);
	logger.debug(`<<< Exiting endOperations(group=${group})`);
  }

  function endOperation_R1(op) {
	logger.debug(`>>> Entering endOperation_R1(op=${op})`);
    var cm = op.cm, display = cm.display;
    maybeClipScrollbars(cm);
    if (op.updateMaxLine) findMaxLine(cm);

    op.mustUpdate = op.viewChanged || op.forceUpdate || op.scrollTop != null ||
      op.scrollToPos && (op.scrollToPos.from.line < display.viewFrom ||
                         op.scrollToPos.to.line >= display.viewTo) ||
      display.maxLineChanged && cm.options.lineWrapping;
    op.update = op.mustUpdate &&
      new DisplayUpdate(cm, op.mustUpdate && {top: op.scrollTop, ensure: op.scrollToPos}, op.forceUpdate);
	logger.debug(`<<< Exiting endOperation_R1(op=${op})`);
	logger.debug(`endOperation_R1(op=${op}): op.update → ${op.update}`);
  }

  function endOperation_W1(op) {
	logger.debug(`>>> Entering endOperation_W1(op=${op})`);
    op.updatedDisplay = op.mustUpdate && updateDisplayIfNeeded(op.cm, op.update);
	logger.debug(`<<< Exiting endOperation_W1(op=${op})`);
  }

  function endOperation_R2(op) {
	logger.debug(`>>> Entering endOperation_R2(op=${op})`);
    var cm = op.cm, display = cm.display;
    if (op.updatedDisplay) updateHeightsInViewport(cm);

    op.barMeasure = measureForScrollbars(cm);

    // If the max line changed since it was last measured, measure it,
    // and ensure the document's width matches it.
    // updateDisplay_W2 will use these properties to do the actual resizing
    if (display.maxLineChanged && !cm.options.lineWrapping) {
      op.adjustWidthTo = measureChar(cm, display.maxLine, display.maxLine.text.length).left + 3;
      cm.display.sizerWidth = op.adjustWidthTo;
      op.barMeasure.scrollWidth =
        Math.max(display.scroller.clientWidth, display.sizer.offsetLeft + op.adjustWidthTo + scrollGap(cm) + cm.display.barWidth);
      op.maxScrollLeft = Math.max(0, display.sizer.offsetLeft + op.adjustWidthTo - displayWidth(cm));
    }

    if (op.updatedDisplay || op.selectionChanged)
	logger.debug(`<<< Exiting endOperation_R2(op=${op})`);
      op.preparedSelection = display.input.prepareSelection();
  }

  function endOperation_W2(op) {
	logger.debug(`>>> Entering endOperation_W2(op=${op})`);
    var cm = op.cm;

    if (op.adjustWidthTo != null) {
      cm.display.sizer.style.minWidth = op.adjustWidthTo + "px";
      if (op.maxScrollLeft < cm.doc.scrollLeft)
        setScrollLeft(cm, Math.min(cm.display.scroller.scrollLeft, op.maxScrollLeft), true);
      cm.display.maxLineChanged = false;
    }

    if (op.preparedSelection)
      cm.display.input.showSelection(op.preparedSelection);
    if (op.updatedDisplay)
      setDocumentHeight(cm, op.barMeasure);
    if (op.updatedDisplay || op.startHeight != cm.doc.height)
      updateScrollbars(cm, op.barMeasure);

    if (op.selectionChanged) restartBlink(cm);

    if (cm.state.focused && op.updateInput)
      cm.display.input.reset(op.typing);
    if (op.focus && op.focus == activeElt()) ensureFocus(op.cm);
	logger.debug(`<<< Exiting endOperation_W2(op=${op})`);
  }

  function endOperation_finish(op) {
	logger.debug(`>>> Entering endOperation_finish(op=${op})`);
    var cm = op.cm, display = cm.display, doc = cm.doc;

    if (op.updatedDisplay) postUpdateDisplay(cm, op.update);

    // Abort mouse wheel delta measurement, when scrolling explicitly
    if (display.wheelStartX != null && (op.scrollTop != null || op.scrollLeft != null || op.scrollToPos))
      display.wheelStartX = display.wheelStartY = null;

	logger.debug(`endOperation_finish(op=${op}): display.wheelStartX → ${display.wheelStartX}`);
    // Propagate the scroll position to the actual DOM scroller
    if (op.scrollTop != null && (display.scroller.scrollTop != op.scrollTop || op.forceScroll)) {
      doc.scrollTop = Math.max(0, Math.min(display.scroller.scrollHeight - display.scroller.clientHeight, op.scrollTop));
      display.scrollbars.setScrollTop(doc.scrollTop);
      display.scroller.scrollTop = doc.scrollTop;
    }
    if (op.scrollLeft != null && (display.scroller.scrollLeft != op.scrollLeft || op.forceScroll)) {
      doc.scrollLeft = Math.max(0, Math.min(display.scroller.scrollWidth - displayWidth(cm), op.scrollLeft));
      display.scrollbars.setScrollLeft(doc.scrollLeft);
      display.scroller.scrollLeft = doc.scrollLeft;
      alignHorizontally(cm);
    }
    // If we need to scroll a specific position into view, do so.
    if (op.scrollToPos) {
      var coords = scrollPosIntoView(cm, clipPos(doc, op.scrollToPos.from),
                                     clipPos(doc, op.scrollToPos.to), op.scrollToPos.margin);
      if (op.scrollToPos.isCursor && cm.state.focused) maybeScrollWindow(cm, coords);
    }

    // Fire events for markers that are hidden/unidden by editing or
    // undoing
    var hidden = op.maybeHiddenMarkers, unhidden = op.maybeUnhiddenMarkers;
    if (hidden) for (var i = 0; i < hidden.length; ++i)
      if (!hidden[i].lines.length) signal(hidden[i], "hide");
    if (unhidden) for (var i = 0; i < unhidden.length; ++i)
      if (unhidden[i].lines.length) signal(unhidden[i], "unhide");

    if (display.wrapper.offsetHeight)
      doc.scrollTop = cm.display.scroller.scrollTop;

	logger.debug(`endOperation_finish(op=${op}): doc.scrollTop → ${doc.scrollTop}`);
    // Fire change events, and delayed event handlers
    if (op.changeObjs)
      signal(cm, "changes", cm, op.changeObjs);
    if (op.update)
	logger.debug(`<<< Exiting endOperation_finish(op=${op})`);
      op.update.finish();
  }

  // Run the given function in an operation
  function runInOp(cm, f) {
    if (cm.curOp) return f();
    startOperation(cm);
    try { return f(); }
    finally { endOperation(cm); }
  }
  // Wraps a function in an operation. Returns the wrapped function.
  function operation(cm, f) {
    return function() {
      if (cm.curOp) return f.apply(cm, arguments);
      startOperation(cm);
      try { return f.apply(cm, arguments); }
      finally { endOperation(cm); }
    };
  }
  // Used to add methods to editor and doc instances, wrapping them in
  // operations.
  function methodOp(f) {
	logger.debug(`>>> Entering methodOp(f=${f})`);
    return function() {
      if (this.curOp) return f.apply(this, arguments);
      startOperation(this);
      try { return f.apply(this, arguments); }
      finally { endOperation(this); }
    };
	logger.debug(`methodOp(): range.margin → ${range.margin}`);
	logger.debug(`<<< Exiting methodOp(f=${f})`);
  }
  function docMethodOp(f) {
	logger.debug(`>>> Entering docMethodOp(f=${f})`);
    return function() {
      var cm = this.cm;
      if (!cm || cm.curOp) return f.apply(this, arguments);
      startOperation(cm);
      try { return f.apply(this, arguments); }
      finally { endOperation(cm); }
    };
	logger.debug(`docMethodOp(ranges=${ranges}): ranges → ${ranges}`);
	logger.debug(`<<< Exiting docMethodOp(f=${f})`);
  }

  // VIEW TRACKING

  // These objects are used to represent the visible (currently drawn)
  // part of the document. A LineView may correspond to multiple
  // logical lines, if those are connected by collapsed ranges.
  function LineView(doc, line, lineN) {
	logger.debug(`>>> Entering LineView(doc=${doc},line=${line},lineN=${lineN})`);
    // The starting line
    this.line = line;
    // Continuing lines, if any
    this.rest = visualLineContinued(line);
	logger.debug(`LineView(doc=${doc},line=${line},lineN=${lineN}): this.line → ${this.line}`);
    // Number of logical lines in this visual line
    this.size = this.rest ? lineNo(lst(this.rest)) - lineN + 1 : 1;
	logger.debug(`LineView(doc=${doc},line=${line},lineN=${lineN}): line → ${line}`);
    this.node = this.text = null;
    this.hidden = lineIsHidden(doc, line);
	logger.debug(`LineView(doc=${doc},line=${line},lineN=${lineN}): node → ${node}`);
	logger.debug(`<<< Exiting LineView(doc=${doc},line=${line},lineN=${lineN})`);
  }

  // Create a range of LineView objects for the given lines.
  function buildViewArray(cm, from, to) {
	logger.debug(`>>> Entering buildViewArray(cm=${cm},from=${from},to=${to})`);
    var array = [], nextPos;
    for (var pos = from; pos < to; pos = nextPos) {
      var view = new LineView(cm.doc, getLine(cm.doc, pos), pos);
	logger.debug(`buildViewArray(cm=${cm},from=${from},to=${to}): view → ${view}`);
      nextPos = pos + view.size;
      array.push(view);
    }
	logger.debug(`<<< Exiting buildViewArray(cm=${cm},from=${from},to=${to})`);
	logger.debug(`>>> Entering lineNo(line=${line})`);
	logger.debug(`<<< Exiting lineNo(line=${line})`);
    return array;
  }

  // Updates the display.view data structure for a given change to the
  // document. From and to are in pre-change coordinates. Lendiff is
  // the amount of lines added or subtracted by the change. This is
  // used for changes that span multiple lines, or change the way
  // lines are divided into visual lines. regLineChange (below)
  // registers single-line changes.
  function regChange(cm, from, to, lendiff) {
	logger.debug(`>>> Entering regChange(cm=${cm},from=${from},to=${to},lendiff=${lendiff})`);
    if (from == null) from = cm.doc.first;
    if (to == null) to = cm.doc.first + cm.doc.size;
    if (!lendiff) lendiff = 0;

    var display = cm.display;
    if (lendiff && to < display.viewTo &&
        (display.updateLineNumbers == null || display.updateLineNumbers > from))
      display.updateLineNumbers = from;
	logger.debug(`regChange(cm=${cm},from=${from},to=${to},lendiff=${lendiff}): display.updateLineNumbers → ${display.updateLineNumbers}`);

    cm.curOp.viewChanged = true;

    if (from >= display.viewTo) { // Change after
      if (sawCollapsedSpans && visualLineNo(cm.doc, from) < display.viewTo)
        resetView(cm);
    } else if (to <= display.viewFrom) { // Change before
      if (sawCollapsedSpans && visualLineEndNo(cm.doc, to + lendiff) > display.viewFrom) {
        resetView(cm);
      } else {
        display.viewFrom += lendiff;
        display.viewTo += lendiff;
		logger.debug(`>>> Entering visualLineEndNo(doc=${doc},lineN=${lineN})`);
		logger.debug(`<<< Exiting visualLineEndNo(doc=${doc},lineN=${lineN})`);
      }
    } else if (from <= display.viewFrom && to >= display.viewTo) { // Full overlap
      resetView(cm);
    } else if (from <= display.viewFrom) { // Top overlap
      var cut = viewCuttingPoint(cm, to, to + lendiff, 1);
      if (cut) {
        display.view = display.view.slice(cut.index);
        display.viewFrom = cut.lineN;
		logger.debug(`regChange(cm=${cm},from=${from},to=${to},lendiff=${lendiff}): display.viewFrom → ${display.viewFrom}`);
        display.viewTo += lendiff;
      } else {
        resetView(cm);
      }
    } else if (to >= display.viewTo) { // Bottom overlap
      var cut = viewCuttingPoint(cm, from, from, -1);
	logger.debug(`regChange(cm=${cm},from=${from},to=${to},lendiff=${lendiff}): cut → ${cut}`);
      if (cut) {
        display.view = display.view.slice(0, cut.index);
        display.viewTo = cut.lineN;
		logger.debug(`regChange(cm=${cm},from=${from},to=${to},lendiff=${lendiff}): display.viewTo → ${display.viewTo}`);
      } else {
        resetView(cm);
      }
    } else { // Gap in the middle
      var cutTop = viewCuttingPoint(cm, from, from, -1);
	logger.debug(`regChange(cm=${cm},from=${from},to=${to},lendiff=${lendiff}): cutTop → ${cutTop}`);
      var cutBot = viewCuttingPoint(cm, to, to + lendiff, 1);
	logger.debug(`regChange(cm=${cm},from=${from},to=${to},lendiff=${lendiff}): cutBot → ${cutBot}`);
      if (cutTop && cutBot) {
        display.view = display.view.slice(0, cutTop.index)
          .concat(buildViewArray(cm, cutTop.lineN, cutBot.lineN))
          .concat(display.view.slice(cutBot.index));
        display.viewTo += lendiff;
      } else {
        resetView(cm);
      }
	logger.debug(`>>> Entering resetView(cm=${cm})`);
	logger.debug(`<<< Exiting resetView(cm=${cm})`);
    }

    var ext = display.externalMeasured;
    if (ext) {
      if (to < ext.lineN)
        ext.lineN += lendiff;
      else if (from < ext.lineN + ext.size)
		logger.debug(`<<< Exiting regChange(cm=${cm},from=${from},to=${to},lendiff=${lendiff})`);
        display.externalMeasured = null;
    }
	logger.debug(`regChange(cm=${cm},from=${from},to=${to},lendiff=${lendiff}): display.externalMeasured → ${display.externalMeasured}`);
  }

  // Register a change to a single line. Type must be one of "text",
  // "gutter", "class", "widget"
  function regLineChange(cm, line, type) {
	logger.debug(`>>> Entering regLineChange(cm=${cm},line=${line},type=${type})`);
    cm.curOp.viewChanged = true;
    var display = cm.display, ext = cm.display.externalMeasured;
    if (ext && line >= ext.lineN && line < ext.lineN + ext.size)
      display.externalMeasured = null;

	logger.debug(`regLineChange(cm=${cm},line=${line},type=${type}): display.externalMeasured → ${display.externalMeasured}`);
    if (line < display.viewFrom || line >= display.viewTo) return;
    var lineView = display.view[findViewIndex(cm, line)];
	logger.debug(`regLineChange(cm=${cm},line=${line},type=${type}): lineView → ${lineView}`);
    if (lineView.node == null) return;
    var arr = lineView.changes || (lineView.changes = []);
    if (indexOf(arr, type) == -1) arr.push(type);
	logger.debug(`regLineChange(cm=${cm},line=${line},type=${type}): arr → ${arr}`);
	logger.debug(`<<< Exiting regLineChange(cm=${cm},line=${line},type=${type})`);
  }

  // Clear the view.
  function resetView(cm) {
    cm.display.viewFrom = cm.display.viewTo = cm.doc.first;
    cm.display.view = [];
    cm.display.viewOffset = 0;
  }

  // Find the view element corresponding to a given line. Return null
  // when the line isn't visible.
  function findViewIndex(cm, n) {
	logger.debug(`>>> Entering findViewIndex(cm=${cm},n=${n})`);
    if (n >= cm.display.viewTo) return null;
    n -= cm.display.viewFrom;
    if (n < 0) return null;
    var view = cm.display.view;
    for (var i = 0; i < view.length; i++) {
      n -= view[i].size;
      if (n < 0) return i;
	logger.debug(`<<< Exiting findViewIndex(cm=${cm},n=${n})`);
    }
  }

  function viewCuttingPoint(cm, oldN, newN, dir) {
	logger.debug(`>>> Entering viewCuttingPoint(cm=${cm},oldN=${oldN},newN=${newN},dir=${dir})`);
    var index = findViewIndex(cm, oldN), diff, view = cm.display.view;
    if (!sawCollapsedSpans || newN == cm.doc.first + cm.doc.size)
      return {index: index, lineN: newN};
    for (var i = 0, n = cm.display.viewFrom; i < index; i++)
      n += view[i].size;
    if (n != oldN) {
      if (dir > 0) {
        if (index == view.length - 1) return null;
        diff = (n + view[index].size) - oldN;
        index++;
      } else {
        diff = n - oldN;
      }
      oldN += diff; newN += diff;
    }
    while (visualLineNo(cm.doc, newN) != newN) {
      if (index == (dir < 0 ? 0 : view.length - 1)) return null;
      newN += dir * view[index - (dir < 0 ? 1 : 0)].size;
      index += dir;
    }
	logger.debug(`<<< Exiting viewCuttingPoint(cm=${cm},oldN=${oldN},newN=${newN},dir=${dir})`);
    return {index: index, lineN: newN};
  }

  // Force the view to cover a given range, adding empty view element
  // or clipping off existing ones as needed.
  function adjustView(cm, from, to) {
	logger.debug(`>>> Entering adjustView(cm=${cm},from=${from},to=${to})`);
    var display = cm.display, view = display.view;
    if (view.length == 0 || from >= display.viewTo || to <= display.viewFrom) {
      display.view = buildViewArray(cm, from, to);
      display.viewFrom = from;
	logger.debug(`adjustView(cm=${cm},from=${from},to=${to}): display.viewFrom → ${display.viewFrom}`);
    } else {
      if (display.viewFrom > from)
        display.view = buildViewArray(cm, from, display.viewFrom).concat(display.view);
      else if (display.viewFrom < from)
        display.view = display.view.slice(findViewIndex(cm, from));
      display.viewFrom = from;
      if (display.viewTo < to)
        display.view = display.view.concat(buildViewArray(cm, display.viewTo, to));
      else if (display.viewTo > to)
        display.view = display.view.slice(0, findViewIndex(cm, to));
    }
    display.viewTo = to;
	logger.debug(`adjustView(cm=${cm},from=${from},to=${to}): display.viewTo → ${display.viewTo}`);
	logger.debug(`<<< Exiting adjustView(cm=${cm},from=${from},to=${to})`);
  }

  // Count the number of lines in the view whose DOM representation is
  // out of date (or nonexistent).
  function countDirtyView(cm) {
    var view = cm.display.view, dirty = 0;
    for (var i = 0; i < view.length; i++) {
      var lineView = view[i];
      if (!lineView.hidden && (!lineView.node || lineView.changes)) ++dirty;
    }
    return dirty;
  }

  // EVENT HANDLERS

  // Attach the necessary event handlers when initializing the editor
  function registerEventHandlers(cm) {
	logger.debug(`>>> Entering registerEventHandlers(cm=${cm})`);
    var d = cm.display;
    on(d.scroller, "mousedown", operation(cm, onMouseDown));
    // Older IE's will not fire a second mousedown for a double click
    if (ie && ie_version < 11)
      on(d.scroller, "dblclick", operation(cm, function(e) {
        if (signalDOMEvent(cm, e)) return;
        var pos = posFromMouse(cm, e);
		logger.debug(`on(pos=${pos}): pos → ${pos}`);
        if (!pos || clickInGutter(cm, e) || eventInWidget(cm.display, e)) return;
        e_preventDefault(e);
        var word = cm.findWordAt(pos);
		logger.debug(`on(pos=${pos}): word → ${word}`);
        extendSelection(cm.doc, word.anchor, word.head);
      }));
    else
      on(d.scroller, "dblclick", function(e) { signalDOMEvent(cm, e) || e_preventDefault(e); });
    // Some browsers fire contextmenu *after* opening the menu, at
    // which point we can't mess with it anymore. Context menu is
    // handled in onMouseDown for these browsers.
    if (!captureRightClick) on(d.scroller, "contextmenu", function(e) {onContextMenu(cm, e);});

    // Used to suppress mouse event handling when a touch happens
    var touchFinished, prevTouch = {end: 0};
    function finishTouch() {
	logger.debug('>>> Entering finishTouch()');
      if (d.activeTouch) {
        touchFinished = setTimeout(function() {d.activeTouch = null;}, 1000);
        prevTouch = d.activeTouch;
        prevTouch.end = +new Date;
		logger.debug(`finishTouch(): prevTouch → ${prevTouch}`);
		logger.debug(`finishTouch(): prevTouch.end → ${prevTouch.end}`);
      }
	logger.debug('<<< Exiting finishTouch()');
    };
    function isMouseLikeTouchEvent(e) {
	logger.debug(`>>> Entering isMouseLikeTouchEvent(e=${e})`);
      if (e.touches.length != 1) return false;
      var touch = e.touches[0];
	logger.debug(`<<< Exiting isMouseLikeTouchEvent(e=${e})`);
      return touch.radiusX <= 1 && touch.radiusY <= 1;
    }
    function farAway(touch, other) {
	logger.debug(`>>> Entering farAway(touch=${touch},other=${other})`);
      if (other.left == null) return true;
      var dx = other.left - touch.left, dy = other.top - touch.top;
	logger.debug(`<<< Exiting farAway(touch=${touch},other=${other})`);
      return dx * dx + dy * dy > 20 * 20;
    }
    on(d.scroller, "touchstart", function(e) {
      if (!isMouseLikeTouchEvent(e)) {
        clearTimeout(touchFinished);
        var now = +new Date;
        d.activeTouch = {start: now, moved: false,
                         prev: now - prevTouch.end <= 300 ? prevTouch : null};
						logger.debug(`on(now=${now}): d.activeTouch → ${d.activeTouch}`);
        if (e.touches.length == 1) {
          d.activeTouch.left = e.touches[0].pageX;
          d.activeTouch.top = e.touches[0].pageY;
        }
      }
    });
    on(d.scroller, "touchmove", function() {
      if (d.activeTouch) d.activeTouch.moved = true;
    });
    on(d.scroller, "touchend", function(e) {
      var touch = d.activeTouch;
      if (touch && !eventInWidget(d, e) && touch.left != null &&
          !touch.moved && new Date - touch.start < 300) {
        var pos = cm.coordsChar(d.activeTouch, "page"), range;
		logger.debug(`on(): pos → ${pos}`);
        if (!touch.prev || farAway(touch, touch.prev)) // Single tap
          range = new Range(pos, pos);
        else if (!touch.prev.prev || farAway(touch, touch.prev.prev)) // Double tap
          range = cm.findWordAt(pos);
        else // Triple tap
          range = new Range(Pos(pos.line, 0), clipPos(cm.doc, Pos(pos.line + 1, 0)));
        cm.setSelection(range.anchor, range.head);
        cm.focus();
        e_preventDefault(e);
      }
      finishTouch();
    });
    on(d.scroller, "touchcancel", finishTouch);

    // Sync scrolling between fake scrollbars and real scrollable
    // area, ensure viewport is updated when scrolling.
    on(d.scroller, "scroll", function() {
      if (d.scroller.clientHeight) {
        setScrollTop(cm, d.scroller.scrollTop);
        setScrollLeft(cm, d.scroller.scrollLeft, true);
        signal(cm, "scroll", cm);
      }
    });

    // Listen to wheel events in order to try and update the viewport on time.
    on(d.scroller, "mousewheel", function(e){onScrollWheel(cm, e);});
    on(d.scroller, "DOMMouseScroll", function(e){onScrollWheel(cm, e);});

    // Prevent wrapper from ever scrolling
    on(d.wrapper, "scroll", function() { d.wrapper.scrollTop = d.wrapper.scrollLeft = 0; });

    d.dragFunctions = {
      enter: function(e) {if (!signalDOMEvent(cm, e)) e_stop(e);},
      over: function(e) {if (!signalDOMEvent(cm, e)) { onDragOver(cm, e); e_stop(e); }},
      start: function(e){onDragStart(cm, e);},
      drop: operation(cm, onDrop),
      leave: function() {clearDragCursor(cm);}
    };

    var inp = d.input.getField();
    on(inp, "keyup", function(e) { onKeyUp.call(cm, e); });
    on(inp, "keydown", operation(cm, onKeyDown));
    on(inp, "keypress", operation(cm, onKeyPress));
    on(inp, "focus", bind(onFocus, cm));
    on(inp, "blur", bind(onBlur, cm));
	logger.debug(`<<< Exiting registerEventHandlers(cm=${cm})`);
  }

  function dragDropChanged(cm, value, old) {
	logger.debug(`>>> Entering dragDropChanged(cm=${cm},value=${value},old=${old})`);
    var wasOn = old && old != CodeMirror.Init;
    if (!value != !wasOn) {
      var funcs = cm.display.dragFunctions;
      var toggle = value ? on : off;
      toggle(cm.display.scroller, "dragstart", funcs.start);
      toggle(cm.display.scroller, "dragenter", funcs.enter);
      toggle(cm.display.scroller, "dragover", funcs.over);
      toggle(cm.display.scroller, "dragleave", funcs.leave);
      toggle(cm.display.scroller, "drop", funcs.drop);
	logger.debug(`<<< Exiting dragDropChanged(cm=${cm},value=${value},old=${old})`);
    }
  }

  // Called when the window resizes
  function onResize(cm) {
	logger.debug(`>>> Entering onResize(cm=${cm})`);
    var d = cm.display;
    if (d.lastWrapHeight == d.wrapper.clientHeight && d.lastWrapWidth == d.wrapper.clientWidth)
      return;
    // Might be a text scaling operation, clear size caches.
    d.cachedCharWidth = d.cachedTextHeight = d.cachedPaddingH = null;
	logger.debug(`onResize(cm=${cm}): d.cachedCharWidth → ${d.cachedCharWidth}`);
    d.scrollbarsClipped = false;
	logger.debug(`onResize(cm=${cm}): d.scrollbarsClipped → ${d.scrollbarsClipped}`);
    cm.setSize();
	logger.debug(`<<< Exiting onResize(cm=${cm})`);
  }

  // MOUSE EVENTS

  // Return true when the given mouse event happened in a widget
  function eventInWidget(display, e) {
	logger.debug(`>>> Entering eventInWidget(display=${display},e=${e})`);
    for (var n = e_target(e); n != display.wrapper; n = n.parentNode) {
      if (!n || (n.nodeType == 1 && n.getAttribute("cm-ignore-events") == "true") ||
          (n.parentNode == display.sizer && n != display.mover))
		logger.debug(`<<< Exiting eventInWidget(display=${display},e=${e})`);
        return true;
    }
  }

  // Given a mouse event, find the corresponding position. If liberal
  // is false, it checks whether a gutter or scrollbar was clicked,
  // and returns null if it was. forRect is used by rectangular
  // selections, and tries to estimate a character position even for
  // coordinates beyond the right of the text.
  function posFromMouse(cm, e, liberal, forRect) {
	logger.debug(`>>> Entering posFromMouse(cm=${cm},e=${e},liberal=${liberal},forRect=${forRect})`);
    var display = cm.display;
    if (!liberal && e_target(e).getAttribute("cm-not-content") == "true") return null;

    var x, y, space = display.lineSpace.getBoundingClientRect();
    // Fails unpredictably on IE[67] when mouse is dragged around quickly.
    try { x = e.clientX - space.left; y = e.clientY - space.top; }
    catch (e) { return null; }
	logger.error(`Exception in posFromMouse(cm=${cm},e=${e},liberal=${liberal},forRect=${forRect}): ${e.message}`, e);
    var coords = coordsChar(cm, x, y), line;
    if (forRect && coords.xRel == 1 && (line = getLine(cm.doc, coords.line).text).length == coords.ch) {
      var colDiff = countColumn(line, line.length, cm.options.tabSize) - line.length;
	logger.debug(`posFromMouse(cm=${cm},e=${e},liberal=${liberal},forRect=${forRect}): colDiff → ${colDiff}`);
      coords = Pos(coords.line, Math.max(0, Math.round((x - paddingH(cm.display).left) / charWidth(cm.display)) - colDiff));
    }
	logger.debug(`<<< Exiting posFromMouse(cm=${cm},e=${e},liberal=${liberal},forRect=${forRect})`);
    return coords;
  }

  // A mouse down can be a single click, double click, triple click,
  // start of selection drag, start of text drag, new cursor
  // (ctrl-click), rectangle drag (alt-drag), or xwin
  // middle-click-paste. Or it might be a click on something we should
  // not interfere with, such as a scrollbar or widget.
  function onMouseDown(e) {
	logger.debug(`>>> Entering onMouseDown(e=${e})`);
    var cm = this, display = cm.display;
    if (display.activeTouch && display.input.supportsTouch() || signalDOMEvent(cm, e)) return;
    display.shift = e.shiftKey;
	logger.debug(`onMouseDown(e=${e}): display.shift → ${display.shift}`);

    if (eventInWidget(display, e)) {
      if (!webkit) {
        // Briefly turn off draggability, to allow widgets to do
        // normal dragging things.
        display.scroller.draggable = false;
        setTimeout(function(){display.scroller.draggable = true;}, 100);
      }
      return;
    }
    if (clickInGutter(cm, e)) return;
    var start = posFromMouse(cm, e);
    window.focus();

    switch (e_button(e)) {
    case 1:
      // #3261: make sure, that we're not starting a second selection
      if (cm.state.selectingText)
        cm.state.selectingText(e);
      else if (start)
        leftButtonDown(cm, e, start);
      else if (e_target(e) == display.scroller)
        e_preventDefault(e);
      break;
    case 2:
      if (webkit) cm.state.lastMiddleDown = +new Date;
      if (start) extendSelection(cm.doc, start);
      setTimeout(function() {display.input.focus();}, 20);
      e_preventDefault(e);
      break;
    case 3:
      if (captureRightClick) onContextMenu(cm, e);
      else delayBlurEvent(cm);
      break;
    }
	logger.debug(`<<< Exiting onMouseDown(e=${e})`);
  }

  var lastClick, lastDoubleClick;
  function leftButtonDown(cm, e, start) {
	logger.debug(`>>> Entering leftButtonDown(cm=${cm},e=${e},start=${start})`);
    if (ie) setTimeout(bind(ensureFocus, cm), 0);
    else cm.curOp.focus = activeElt();

    var now = +new Date, type;
    if (lastDoubleClick && lastDoubleClick.time > now - 400 && cmp(lastDoubleClick.pos, start) == 0) {
      type = "triple";
    } else if (lastClick && lastClick.time > now - 400 && cmp(lastClick.pos, start) == 0) {
      type = "double";
      lastDoubleClick = {time: now, pos: start};
    } else {
      type = "single";
      lastClick = {time: now, pos: start};
    }

    var sel = cm.doc.sel, modifier = mac ? e.metaKey : e.ctrlKey, contained;
    if (cm.options.dragDrop && dragAndDrop && !isReadOnly(cm) &&
        type == "single" && (contained = sel.contains(start)) > -1 &&
        (cmp((contained = sel.ranges[contained]).from(), start) < 0 || start.xRel > 0) &&
        (cmp(contained.to(), start) > 0 || start.xRel < 0))
      leftButtonStartDrag(cm, e, start, modifier);
    else
      leftButtonSelect(cm, e, start, type, modifier);
	logger.debug(`>>> Entering delayBlurEvent(cm=${cm})`);
	logger.debug(`<<< Exiting delayBlurEvent(cm=${cm})`);
  }

  // Start a text drag. When it ends, see if any dragging actually
  // happen, and treat as a click if it didn't.
  function leftButtonStartDrag(cm, e, start, modifier) {
	logger.debug(`>>> Entering leftButtonStartDrag(cm=${cm},e=${e},start=${start},modifier=${modifier})`);
    var display = cm.display, startTime = +new Date;
    var dragEnd = operation(cm, function(e2) {
      if (webkit) display.scroller.draggable = false;
      cm.state.draggingText = false;
      off(document, "mouseup", dragEnd);
      off(display.scroller, "drop", dragEnd);
      if (Math.abs(e.clientX - e2.clientX) + Math.abs(e.clientY - e2.clientY) < 10) {
        e_preventDefault(e2);
        if (!modifier && +new Date - 200 < startTime)
          extendSelection(cm.doc, start);
        // Work around unexplainable focus problem in IE9 (#2127) and Chrome (#3081)
		logger.debug(`<<< Exiting leftButtonStartDrag(cm=${cm},e=${e},start=${start},modifier=${modifier})`);
        if (webkit || ie && ie_version == 9)
          setTimeout(function() {document.body.focus(); display.input.focus();}, 20);
        else
          display.input.focus();
      }
    });
    // Let the drag handler handle this.
    if (webkit) display.scroller.draggable = true;
    cm.state.draggingText = dragEnd;
    // IE's approach to draggable
    if (display.scroller.dragDrop) display.scroller.dragDrop();
    on(document, "mouseup", dragEnd);
    on(display.scroller, "drop", dragEnd);
  }

  // Normal selection, as opposed to text dragging.
  function leftButtonSelect(cm, e, start, type, addNew) {
	logger.debug(`>>> Entering leftButtonSelect(cm=${cm},e=${e},start=${start},type=${type},addNew=${addNew})`);
    var display = cm.display, doc = cm.doc;
    e_preventDefault(e);

    var ourRange, ourIndex, startSel = doc.sel, ranges = startSel.ranges;
    if (addNew && !e.shiftKey) {
      ourIndex = doc.sel.contains(start);
      if (ourIndex > -1)
        ourRange = ranges[ourIndex];
      else
        ourRange = new Range(start, start);
    } else {
      ourRange = doc.sel.primary();
      ourIndex = doc.sel.primIndex;
    }

    if (e.altKey) {
      type = "rect";
      if (!addNew) ourRange = new Range(start, start);
	logger.debug(`leftButtonSelect(cm=${cm},e=${e},start=${start},type=${type},addNew=${addNew}): type → ${type}`);
      start = posFromMouse(cm, e, true, true);
      ourIndex = -1;
    } else if (type == "double") {
      var word = cm.findWordAt(start);
	logger.debug(`leftButtonSelect(cm=${cm},e=${e},start=${start},type=${type},addNew=${addNew}): word → ${word}`);
      if (cm.display.shift || doc.extend)
        ourRange = extendRange(doc, ourRange, word.anchor, word.head);
      else
        ourRange = word;
    } else if (type == "triple") {
      var line = new Range(Pos(start.line, 0), clipPos(doc, Pos(start.line + 1, 0)));
	logger.debug(`leftButtonSelect(cm=${cm},e=${e},start=${start},type=${type},addNew=${addNew}): line → ${line}`);
      if (cm.display.shift || doc.extend)
        ourRange = extendRange(doc, ourRange, line.anchor, line.head);
      else
        ourRange = line;
    } else {
      ourRange = extendRange(doc, ourRange, start);
    }

    if (!addNew) {
      ourIndex = 0;
      setSelection(doc, new Selection([ourRange], 0), sel_mouse);
      startSel = doc.sel;
    } else if (ourIndex == -1) {
      ourIndex = ranges.length;
      setSelection(doc, normalizeSelection(ranges.concat([ourRange]), ourIndex),
                   {scroll: false, origin: "*mouse"});
    } else if (ranges.length > 1 && ranges[ourIndex].empty() && type == "single" && !e.shiftKey) {
      setSelection(doc, normalizeSelection(ranges.slice(0, ourIndex).concat(ranges.slice(ourIndex + 1)), 0),
                   {scroll: false, origin: "*mouse"});
      startSel = doc.sel;
    } else {
      replaceOneSelection(doc, ourIndex, ourRange, sel_mouse);
    }

	logger.debug(`<<< Exiting leftButtonSelect(cm=${cm},e=${e},start=${start},type=${type},addNew=${addNew})`);
    var lastPos = start;
    function extendTo(pos) {
	logger.debug(`>>> Entering extendTo(pos=${pos})`);
      if (cmp(lastPos, pos) == 0) return;
      lastPos = pos;

      if (type == "rect") {
        var ranges = [], tabSize = cm.options.tabSize;
        var startCol = countColumn(getLine(doc, start.line).text, start.ch, tabSize);
		logger.debug(`extendTo(pos=${pos}): startCol → ${startCol}`);
        var posCol = countColumn(getLine(doc, pos.line).text, pos.ch, tabSize);
		logger.debug(`extendTo(pos=${pos}): posCol → ${posCol}`);
        var left = Math.min(startCol, posCol), right = Math.max(startCol, posCol);
		logger.debug(`extendTo(pos=${pos}): left → ${left}`);
        for (var line = Math.min(start.line, pos.line), end = Math.min(cm.lastLine(), Math.max(start.line, pos.line));
             line <= end; line++) {
          var text = getLine(doc, line).text, leftPos = findColumn(text, left, tabSize);
		logger.debug(`extendTo(pos=${pos}): text → ${text}`);
          if (left == right)
            ranges.push(new Range(Pos(line, leftPos), Pos(line, leftPos)));
          else if (text.length > leftPos)
            ranges.push(new Range(Pos(line, leftPos), Pos(line, findColumn(text, right, tabSize))));
        }
        if (!ranges.length) ranges.push(new Range(start, start));
		logger.debug(`extendTo(pos=${pos}): line → ${line}`);
        setSelection(doc, normalizeSelection(startSel.ranges.slice(0, ourIndex).concat(ranges), ourIndex),
                     {origin: "*mouse", scroll: false});
        cm.scrollIntoView(pos);
      } else {
        var oldRange = ourRange;
        var anchor = oldRange.anchor, head = pos;
		logger.debug(`extendTo(pos=${pos}): anchor → ${anchor}`);
        if (type != "single") {
          if (type == "double")
            var range = cm.findWordAt(pos);
          else
            var range = new Range(Pos(pos.line, 0), clipPos(doc, Pos(pos.line + 1, 0)));
          if (cmp(range.anchor, anchor) > 0) {
			logger.debug(`extendTo(pos=${pos}): range → ${range}`);
            head = range.head;
            anchor = minPos(oldRange.from(), range.anchor);
          } else {
            head = range.anchor;
            anchor = maxPos(oldRange.to(), range.head);
          }
        }
        var ranges = startSel.ranges.slice(0);
		logger.debug(`extendTo(pos=${pos}): ranges → ${ranges}`);
        ranges[ourIndex] = new Range(clipPos(doc, anchor), head);
        setSelection(doc, normalizeSelection(ranges, ourIndex), sel_mouse);
		logger.debug(`<<< Exiting extendTo(pos=${pos})`);
      }
    }

    var editorSize = display.wrapper.getBoundingClientRect();
    // Used to ensure timeout re-tries don't fire when another extend
    // happened in the meantime (clearTimeout isn't reliable -- at
    // least on Chrome, the timeouts still happen even when cleared,
    // if the clear happens after their scheduled firing time).
    var counter = 0;

    function extend(e) {
	logger.debug(`>>> Entering extend(e=${e})`);
      var curCount = ++counter;
      var cur = posFromMouse(cm, e, true, type == "rect");
	logger.debug(`extend(e=${e}): cur → ${cur}`);
      if (!cur) return;
      if (cmp(cur, lastPos) != 0) {
        cm.curOp.focus = activeElt();
        extendTo(cur);
        var visible = visibleLines(display, doc);
		logger.debug(`<<< Exiting extend(e=${e})`);
		logger.debug(`extend(e=${e}): visible → ${visible}`);
        if (cur.line >= visible.to || cur.line < visible.from)
          setTimeout(operation(cm, function(){if (counter == curCount) extend(e);}), 150);
      } else {
        var outside = e.clientY < editorSize.top ? -20 : e.clientY > editorSize.bottom ? 20 : 0;
        if (outside) setTimeout(operation(cm, function() {
          if (counter != curCount) return;
          display.scroller.scrollTop += outside;
          extend(e);
        }), 50);
      }
    }

    function done(e) {
      cm.state.selectingText = false;
      counter = Infinity;
      e_preventDefault(e);
      display.input.focus();
      off(document, "mousemove", move);
      off(document, "mouseup", up);
      doc.history.lastSelOrigin = null;
    }

    var move = operation(cm, function(e) {
      if (!e_button(e)) done(e);
      else extend(e);
    });
    var up = operation(cm, done);
    cm.state.selectingText = up;
    on(document, "mousemove", move);
    on(document, "mouseup", up);
  }

  // Determines whether an event happened in the gutter, and fires the
  // handlers for the corresponding event.
  function gutterEvent(cm, e, type, prevent, signalfn) {
	logger.debug(`>>> Entering gutterEvent(cm=${cm},e=${e},type=${type},prevent=${prevent},signalfn=${signalfn})`);
    try { var mX = e.clientX, mY = e.clientY; }
    catch(e) { return false; }
	logger.error(`Exception in gutterEvent(cm=${cm},e=${e},type=${type},prevent=${prevent},signalfn=${signalfn}): ${e.message}`, e);
    if (mX >= Math.floor(cm.display.gutters.getBoundingClientRect().right)) return false;
    if (prevent) e_preventDefault(e);

    var display = cm.display;
    var lineBox = display.lineDiv.getBoundingClientRect();
	logger.debug(`gutterEvent(cm=${cm},e=${e},type=${type},prevent=${prevent},signalfn=${signalfn}): lineBox → ${lineBox}`);

    if (mY > lineBox.bottom || !hasHandler(cm, type)) return e_defaultPrevented(e);
    mY -= lineBox.top - display.viewOffset;

    for (var i = 0; i < cm.options.gutters.length; ++i) {
      var g = display.gutters.childNodes[i];
      if (g && g.getBoundingClientRect().right >= mX) {
        var line = lineAtHeight(cm.doc, mY);
		logger.debug(`gutterEvent(cm=${cm},e=${e},type=${type},prevent=${prevent},signalfn=${signalfn}): line → ${line}`);
        var gutter = cm.options.gutters[i];
        signalfn(cm, type, cm, line, gutter, e);
		logger.debug(`<<< Exiting gutterEvent(cm=${cm},e=${e},type=${type},prevent=${prevent},signalfn=${signalfn})`);
        return e_defaultPrevented(e);
      }
    }
	logger.debug(`>>> Entering up(e=${e})`);
	logger.debug(`<<< Exiting up(e=${e})`);
  }

  function clickInGutter(cm, e) {
	logger.debug(`>>> Entering clickInGutter(cm=${cm},e=${e})`);
	logger.debug(`<<< Exiting clickInGutter(cm=${cm},e=${e})`);
    return gutterEvent(cm, e, "gutterClick", true, signalLater);
  }

  // Kludge to work around strange IE behavior where it'll sometimes
  // re-fire a series of drag-related events right after the drop (#1551)
  var lastDrop = 0;

  function onDrop(e) {
	logger.debug(`>>> Entering onDrop(e=${e})`);
    var cm = this;
    clearDragCursor(cm);
    if (signalDOMEvent(cm, e) || eventInWidget(cm.display, e))
      return;
    e_preventDefault(e);
    if (ie) lastDrop = +new Date;
    var pos = posFromMouse(cm, e, true), files = e.dataTransfer.files;
    if (!pos || isReadOnly(cm)) return;
    // Might be a file drop, in which case we simply extract the text
    // and insert it.
    if (files && files.length && window.FileReader && window.File) {
      var n = files.length, text = Array(n), read = 0;
	logger.debug(`onDrop(e=${e}): n → ${n}`);
      var loadFile = function(file, i) {
		logger.debug(`>>> Entering loadFile(file=${file},i=${i})`);
        var reader = new FileReader;
        reader.onload = operation(cm, function() {
          text[i] = reader.result;
          if (++read == n) {
            pos = clipPos(cm.doc, pos);
            var change = {from: pos, to: pos,
                          text: cm.doc.splitLines(text.join(cm.doc.lineSeparator())),
                          origin: "paste"};
            makeChange(cm.doc, change);
            setSelectionReplaceHistory(cm.doc, simpleSelection(pos, changeEnd(change)));
          }
        });
        reader.readAsText(file);
		logger.debug(`<<< Exiting loadFile(file=${file},i=${i})`);
      };
      for (var i = 0; i < n; ++i) loadFile(files[i], i);
    } else { // Normal drop
      // Don't do a replace if the drop happened inside of the selected text.
      if (cm.state.draggingText && cm.doc.sel.contains(pos) > -1) {
        cm.state.draggingText(e);
        // Ensure the editor is re-focused
        setTimeout(function() {cm.display.input.focus();}, 20);
        return;
      }
      try {
        var text = e.dataTransfer.getData("Text");
        if (text) {
          if (cm.state.draggingText && !(mac ? e.altKey : e.ctrlKey))
            var selected = cm.listSelections();
          setSelectionNoUndo(cm.doc, simpleSelection(pos, pos));
          if (selected) for (var i = 0; i < selected.length; ++i)
            replaceRange(cm.doc, "", selected[i].anchor, selected[i].head, "drag");
          cm.replaceSelection(text, "around", "paste");
          cm.display.input.focus();
        }
      }
      catch(e){}
    }
	logger.debug(`<<< Exiting onDrop(e=${e})`);
  }

  function onDragStart(cm, e) {
	logger.debug(`>>> Entering onDragStart(cm=${cm},e=${e})`);
    if (ie && (!cm.state.draggingText || +new Date - lastDrop < 100)) { e_stop(e); return; }
    if (signalDOMEvent(cm, e) || eventInWidget(cm.display, e)) return;

    e.dataTransfer.setData("Text", cm.getSelection());

    // Use dummy image instead of default browsers image.
    // Recent Safari (~6.0.2) have a tendency to segfault when this happens, so we don't do it there.
    if (e.dataTransfer.setDragImage && !safari) {
      var img = elt("img", null, null, "position: fixed; left: 0; top: 0;");
	logger.debug(`onDragStart(cm=${cm},e=${e}): img → ${img}`);
      img.src = "data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
      if (presto) {
        img.width = img.height = 1;
		logger.debug(`onDragStart(cm=${cm},e=${e}): img.width → ${img.width}`);
        cm.display.wrapper.appendChild(img);
        // Force a relayout, or Opera won't use our image for some obscure reason
        img._top = img.offsetTop;
		logger.debug(`onDragStart(cm=${cm},e=${e}): img._top → ${img._top}`);
      }
      e.dataTransfer.setDragImage(img, 0, 0);
      if (presto) img.parentNode.removeChild(img);
	logger.debug(`<<< Exiting onDragStart(cm=${cm},e=${e})`);
    }
  }

  function onDragOver(cm, e) {
	logger.debug(`>>> Entering onDragOver(cm=${cm},e=${e})`);
    var pos = posFromMouse(cm, e);
    if (!pos) return;
	logger.debug(`onDragOver(cm=${cm},e=${e}): pos → ${pos}`);
    var frag = document.createDocumentFragment();
	logger.debug(`onDragOver(cm=${cm},e=${e}): frag → ${frag}`);
    drawSelectionCursor(cm, pos, frag);
    if (!cm.display.dragCursor) {
      cm.display.dragCursor = elt("div", null, "CodeMirror-cursors CodeMirror-dragcursors");
      cm.display.lineSpace.insertBefore(cm.display.dragCursor, cm.display.cursorDiv);
    }
    removeChildrenAndAdd(cm.display.dragCursor, frag);
	logger.debug(`<<< Exiting onDragOver(cm=${cm},e=${e})`);
  }

  function clearDragCursor(cm) {
	logger.debug(`>>> Entering clearDragCursor(cm=${cm})`);
    if (cm.display.dragCursor) {
      cm.display.lineSpace.removeChild(cm.display.dragCursor);
      cm.display.dragCursor = null;
	logger.debug(`<<< Exiting clearDragCursor(cm=${cm})`);
    }
  }

  // SCROLL EVENTS

  // Sync the scrollable area and scrollbars, ensure the viewport
  // covers the visible area.
  function setScrollTop(cm, val) {
    if (Math.abs(cm.doc.scrollTop - val) < 2) return;
    cm.doc.scrollTop = val;
    if (!gecko) updateDisplaySimple(cm, {top: val});
    if (cm.display.scroller.scrollTop != val) cm.display.scroller.scrollTop = val;
    cm.display.scrollbars.setScrollTop(val);
    if (gecko) updateDisplaySimple(cm);
    startWorker(cm, 100);
  }
  // Sync scroller and scrollbar, ensure the gutter elements are
  // aligned.
  function setScrollLeft(cm, val, isScroller) {
    if (isScroller ? val == cm.doc.scrollLeft : Math.abs(cm.doc.scrollLeft - val) < 2) return;
    val = Math.min(val, cm.display.scroller.scrollWidth - cm.display.scroller.clientWidth);
    cm.doc.scrollLeft = val;
    alignHorizontally(cm);
    if (cm.display.scroller.scrollLeft != val) cm.display.scroller.scrollLeft = val;
    cm.display.scrollbars.setScrollLeft(val);
  }

  // Since the delta values reported on mouse wheel events are
  // unstandardized between browsers and even browser versions, and
  // generally horribly unpredictable, this code starts by measuring
  // the scroll effect that the first few mouse wheel events have,
  // and, from that, detects the way it can convert deltas to pixel
  // offsets afterwards.
  //
  // The reason we want to know the amount a wheel event will scroll
  // is that it gives us a chance to update the display before the
  // actual scrolling happens, reducing flickering.

  var wheelSamples = 0, wheelPixelsPerUnit = null;
  // Fill in a browser-detected starting value on browsers where we
  // know one. These don't have to be accurate -- the result of them
  // being wrong would just be a slight flicker on the first wheel
  // scroll (if it is large enough).
  if (ie) wheelPixelsPerUnit = -.53;
  else if (gecko) wheelPixelsPerUnit = 15;
  else if (chrome) wheelPixelsPerUnit = -.7;
  else if (safari) wheelPixelsPerUnit = -1/3;

  var wheelEventDelta = function(e) {
	logger.debug(`>>> Entering wheelEventDelta(e=${e})`);
    var dx = e.wheelDeltaX, dy = e.wheelDeltaY;
    if (dx == null && e.detail && e.axis == e.HORIZONTAL_AXIS) dx = e.detail;
    if (dy == null && e.detail && e.axis == e.VERTICAL_AXIS) dy = e.detail;
    else if (dy == null) dy = e.wheelDelta;
	logger.debug(`<<< Exiting wheelEventDelta(e=${e})`);
    return {x: dx, y: dy};
  };
  CodeMirror.wheelEventPixels = function(e) {
    var delta = wheelEventDelta(e);
    delta.x *= wheelPixelsPerUnit;
	logger.debug(`function(e=${e}): delta → ${delta}`);
    delta.y *= wheelPixelsPerUnit;
	logger.debug(`<<< Exiting function(e=${e})`);
    return delta;
  };

  function onScrollWheel(cm, e) {
	logger.debug(`>>> Entering onScrollWheel(cm=${cm},e=${e})`);
    var delta = wheelEventDelta(e), dx = delta.x, dy = delta.y;

	logger.debug(`onScrollWheel(cm=${cm},e=${e}): delta → ${delta}`);
    var display = cm.display, scroll = display.scroller;
    // Quit if there's nothing to scroll here
    if (!(dx && scroll.scrollWidth > scroll.clientWidth ||
          dy && scroll.scrollHeight > scroll.clientHeight)) return;

    // Webkit browsers on OS X abort momentum scrolls when the target
    // of the scroll event is removed from the scrollable element.
    // This hack (see related code in patchDisplay) makes sure the
    // element is kept around.
    if (dy && mac && webkit) {
      outer: for (var cur = e.target, view = display.view; cur != scroll; cur = cur.parentNode) {
        for (var i = 0; i < view.length; i++) {
          if (view[i].node == cur) {
            cm.display.currentWheelTarget = cur;
            break outer;
          }
        }
      }
    }

    // On some browsers, horizontal scrolling will cause redraws to
    // happen before the gutter has been realigned, causing it to
    // wriggle around in a most unseemly way. When we have an
    // estimated pixels/delta value, we just handle horizontal
    // scrolling entirely here. It'll be slightly off from native, but
    // better than glitching out.
    if (dx && !gecko && !presto && wheelPixelsPerUnit != null) {
      if (dy)
        setScrollTop(cm, Math.max(0, Math.min(scroll.scrollTop + dy * wheelPixelsPerUnit, scroll.scrollHeight - scroll.clientHeight)));
      setScrollLeft(cm, Math.max(0, Math.min(scroll.scrollLeft + dx * wheelPixelsPerUnit, scroll.scrollWidth - scroll.clientWidth)));
      e_preventDefault(e);
      display.wheelStartX = null; // Abort measurement, if in progress
	logger.debug(`onScrollWheel(cm=${cm},e=${e}): display.wheelStartX → ${display.wheelStartX}`);
      return;
    }

    // 'Project' the visible viewport to cover the area that is being
    // scrolled into view (if we know enough to estimate it).
    if (dy && wheelPixelsPerUnit != null) {
      var pixels = dy * wheelPixelsPerUnit;
      var top = cm.doc.scrollTop, bot = top + display.wrapper.clientHeight;
      if (pixels < 0) top = Math.max(0, top + pixels - 50);
      else bot = Math.min(cm.doc.height, bot + pixels + 50);
      updateDisplaySimple(cm, {top: top, bottom: bot});
    }

    if (wheelSamples < 20) {
      if (display.wheelStartX == null) {
        display.wheelStartX = scroll.scrollLeft; display.wheelStartY = scroll.scrollTop;
        display.wheelDX = dx; display.wheelDY = dy;
		logger.debug(`onScrollWheel(cm=${cm},e=${e}): display.wheelDX → ${display.wheelDX}`);
        setTimeout(function() {
          if (display.wheelStartX == null) return;
          var movedX = scroll.scrollLeft - display.wheelStartX;
          var movedY = scroll.scrollTop - display.wheelStartY;
          var sample = (movedY && display.wheelDY && movedY / display.wheelDY) ||
            (movedX && display.wheelDX && movedX / display.wheelDX);
          display.wheelStartX = display.wheelStartY = null;
		logger.debug(`setTimeout(): sample → ${sample}`);
          if (!sample) return;
          wheelPixelsPerUnit = (wheelPixelsPerUnit * wheelSamples + sample) / (wheelSamples + 1);
          ++wheelSamples;
        }, 200);
      } else {
        display.wheelDX += dx; display.wheelDY += dy;
      }
    }
	logger.debug(`<<< Exiting onScrollWheel(cm=${cm},e=${e})`);
  }

  // KEY EVENTS

  // Run a handler that was bound to a key.
  function doHandleBinding(cm, bound, dropShift) {
	logger.debug(`>>> Entering doHandleBinding(cm=${cm},bound=${bound},dropShift=${dropShift})`);
    if (typeof bound == "string") {
      bound = commands[bound];
      if (!bound) return false;
    }
    // Ensure previous input has been read, so that the handler sees a
    // consistent view of the document
    cm.display.input.ensurePolled();
    var prevShift = cm.display.shift, done = false;
    try {
      if (isReadOnly(cm)) cm.state.suppressEdits = true;
      if (dropShift) cm.display.shift = false;
      done = bound(cm) != Pass;
    } finally {
      cm.display.shift = prevShift;
      cm.state.suppressEdits = false;
    }
	logger.debug(`<<< Exiting doHandleBinding(cm=${cm},bound=${bound},dropShift=${dropShift})`);
    return done;
  }

  function lookupKeyForEditor(cm, name, handle) {
	logger.debug(`>>> Entering lookupKeyForEditor(cm=${cm},name=${name},handle=${handle})`);
    for (var i = 0; i < cm.state.keyMaps.length; i++) {
      var result = lookupKey(name, cm.state.keyMaps[i], handle, cm);
	logger.debug(`lookupKeyForEditor(cm=${cm},name=${name},handle=${handle}): result → ${result}`);
      if (result) return result;
    }
	logger.debug(`<<< Exiting lookupKeyForEditor(cm=${cm},name=${name},handle=${handle})`);
    return (cm.options.extraKeys && lookupKey(name, cm.options.extraKeys, handle, cm))
      || lookupKey(name, cm.options.keyMap, handle, cm);
  }

  var stopSeq = new Delayed;
  function dispatchKey(cm, name, e, handle) {
	logger.debug(`>>> Entering dispatchKey(cm=${cm},name=${name},e=${e},handle=${handle})`);
    var seq = cm.state.keySeq;
    if (seq) {
      if (isModifierKey(name)) return "handled";
      stopSeq.set(50, function() {
        if (cm.state.keySeq == seq) {
          cm.state.keySeq = null;
          cm.display.input.reset();
        }
      });
      name = seq + " " + name;
    }
    var result = lookupKeyForEditor(cm, name, handle);
	logger.debug(`dispatchKey(cm=${cm},name=${name},e=${e},handle=${handle}): name → ${name}`);
	logger.debug(`dispatchKey(cm=${cm},name=${name},e=${e},handle=${handle}): result → ${result}`);

    if (result == "multi")
      cm.state.keySeq = name;
    if (result == "handled")
      signalLater(cm, "keyHandled", cm, name, e);

    if (result == "handled" || result == "multi") {
      e_preventDefault(e);
      restartBlink(cm);
    }

    if (seq && !result && /\'$/.test(name)) {
      e_preventDefault(e);
	logger.debug(`<<< Exiting dispatchKey(cm=${cm},name=${name},e=${e},handle=${handle})`);
      return true;
    }
    return !!result;
  }

  // Handle a key from the keydown event.
  function handleKeyBinding(cm, e) {
	logger.debug(`>>> Entering handleKeyBinding(cm=${cm},e=${e})`);
    var name = keyName(e, true);
    if (!name) return false;
	logger.debug(`handleKeyBinding(cm=${cm},e=${e}): name → ${name}`);

    if (e.shiftKey && !cm.state.keySeq) {
      // First try to resolve full name (including 'Shift-'). Failing
      // that, see if there is a cursor-motion command (starting with
      // 'go') bound to the keyname without 'Shift-'.
      return dispatchKey(cm, "Shift-" + name, e, function(b) {return doHandleBinding(cm, b, true);})
          || dispatchKey(cm, name, e, function(b) {
               if (typeof b == "string" ? /^go[A-Z]/.test(b) : b.motion)
                 return doHandleBinding(cm, b);
             });
    } else {
	logger.debug(`<<< Exiting handleKeyBinding(cm=${cm},e=${e})`);
      return dispatchKey(cm, name, e, function(b) { return doHandleBinding(cm, b); });
    }
  }

  // Handle a key from the keypress event
  function handleCharBinding(cm, e, ch) {
	logger.debug(`>>> Entering handleCharBinding(cm=${cm},e=${e},ch=${ch})`);
	logger.debug(`<<< Exiting handleCharBinding(cm=${cm},e=${e},ch=${ch})`);
    return dispatchKey(cm, "'" + ch + "'", e,
                       function(b) { return doHandleBinding(cm, b, true); });
  }

  var lastStoppedKey = null;
  function onKeyDown(e) {
    var cm = this;
    cm.curOp.focus = activeElt();
    if (signalDOMEvent(cm, e)) return;
    // IE does strange things with escape.
    if (ie && ie_version < 11 && e.keyCode == 27) e.returnValue = false;
    var code = e.keyCode;
    cm.display.shift = code == 16 || e.shiftKey;
    var handled = handleKeyBinding(cm, e);
    if (presto) {
      lastStoppedKey = handled ? code : null;
      // Opera has no cut event... we try to at least catch the key combo
      if (!handled && code == 88 && !hasCopyEvent && (mac ? e.metaKey : e.ctrlKey))
        cm.replaceSelection("", null, "cut");
    }

    // Turn mouse into crosshair when Alt is held on Mac.
    if (code == 18 && !/\bCodeMirror-crosshair\b/.test(cm.display.lineDiv.className))
      showCrossHair(cm);
  }

  function showCrossHair(cm) {
	logger.debug(`>>> Entering showCrossHair(cm=${cm})`);
    var lineDiv = cm.display.lineDiv;
    addClass(lineDiv, "CodeMirror-crosshair");
	logger.debug(`<<< Exiting showCrossHair(cm=${cm})`);

    function up(e) {
      if (e.keyCode == 18 || !e.altKey) {
        rmClass(lineDiv, "CodeMirror-crosshair");
        off(document, "keyup", up);
        off(document, "mouseover", up);
      }
    }
    on(document, "keyup", up);
    on(document, "mouseover", up);
  }

  function onKeyUp(e) {
    if (e.keyCode == 16) this.doc.sel.shift = false;
    signalDOMEvent(this, e);
  }

  function onKeyPress(e) {
    var cm = this;
    if (eventInWidget(cm.display, e) || signalDOMEvent(cm, e) || e.ctrlKey && !e.altKey || mac && e.metaKey) return;
    var keyCode = e.keyCode, charCode = e.charCode;
    if (presto && keyCode == lastStoppedKey) {lastStoppedKey = null; e_preventDefault(e); return;}
    if ((presto && (!e.which || e.which < 10)) && handleKeyBinding(cm, e)) return;
    var ch = String.fromCharCode(charCode == null ? keyCode : charCode);
    if (handleCharBinding(cm, e, ch)) return;
    cm.display.input.onKeyPress(e);
  }

  // FOCUS/BLUR EVENTS

  function delayBlurEvent(cm) {
    cm.state.delayingBlurEvent = true;
    setTimeout(function() {
      if (cm.state.delayingBlurEvent) {
        cm.state.delayingBlurEvent = false;
        onBlur(cm);
		logger.debug(`>>> Entering setTimeout(resizeTimer=${resizeTimer})`);
      }
    }, 100);
  }

  function onFocus(cm) {
    if (cm.state.delayingBlurEvent) cm.state.delayingBlurEvent = false;

    if (cm.options.readOnly == "nocursor") return;
    if (!cm.state.focused) {
      signal(cm, "focus", cm);
      cm.state.focused = true;
      addClass(cm.display.wrapper, "CodeMirror-focused");
      // This test prevents this from firing when a context
      // menu is closed (since the input reset would kill the
      // select-all detection hack)
      if (!cm.curOp && cm.display.selForContextMenu != cm.doc.sel) {
        cm.display.input.reset();
        if (webkit) setTimeout(function() { cm.display.input.reset(true); }, 20); // Issue #1730
      }
      cm.display.input.receivedFocus();
    }
    restartBlink(cm);
  }
  function onBlur(cm) {
    if (cm.state.delayingBlurEvent) return;

    if (cm.state.focused) {
      signal(cm, "blur", cm);
      cm.state.focused = false;
      rmClass(cm.display.wrapper, "CodeMirror-focused");
    }
    clearInterval(cm.display.blinker);
    setTimeout(function() {if (!cm.state.focused) cm.display.shift = false;}, 150);
  }

  // CONTEXT MENU HANDLING

  // To make the context menu work, we need to briefly unhide the
  // textarea (making it as unobtrusive as possible) to let the
  // right-click take effect on it.
  function onContextMenu(cm, e) {
    if (eventInWidget(cm.display, e) || contextMenuInGutter(cm, e)) return;
    cm.display.input.onContextMenu(e);
  }

  function contextMenuInGutter(cm, e) {
	logger.debug(`>>> Entering contextMenuInGutter(cm=${cm},e=${e})`);
    if (!hasHandler(cm, "gutterContextMenu")) return false;
	logger.debug(`<<< Exiting contextMenuInGutter(cm=${cm},e=${e})`);
    return gutterEvent(cm, e, "gutterContextMenu", false, signal);
  }

  // UPDATING

  // Compute the position of the end of a change (its 'to' property
  // refers to the pre-change end).
  var changeEnd = CodeMirror.changeEnd = function(change) {
    if (!change.text) return change.to;
    return Pos(change.from.line + change.text.length - 1,
               lst(change.text).length + (change.text.length == 1 ? change.from.ch : 0));
  };

  // Adjust a position to refer to the post-change position of the
  // same text, or the end of the change if the change covers it.
  function adjustForChange(pos, change) {
	logger.debug(`>>> Entering adjustForChange(pos=${pos},change=${change})`);
    if (cmp(pos, change.from) < 0) return pos;
    if (cmp(pos, change.to) <= 0) return changeEnd(change);

    var line = pos.line + change.text.length - (change.to.line - change.from.line) - 1, ch = pos.ch;
    if (pos.line == change.to.line) ch += changeEnd(change).ch - change.to.ch;
	logger.debug(`adjustForChange(pos=${pos},change=${change}): line → ${line}`);
	logger.debug(`<<< Exiting adjustForChange(pos=${pos},change=${change})`);
    return Pos(line, ch);
  }

  function computeSelAfterChange(doc, change) {
	logger.debug(`>>> Entering computeSelAfterChange(doc=${doc},change=${change})`);
    var out = [];
    for (var i = 0; i < doc.sel.ranges.length; i++) {
      var range = doc.sel.ranges[i];
      out.push(new Range(adjustForChange(range.anchor, change),
                         adjustForChange(range.head, change)));
    }
	logger.debug(`<<< Exiting computeSelAfterChange(doc=${doc},change=${change})`);
    return normalizeSelection(out, doc.sel.primIndex);
  }

  function offsetPos(pos, old, nw) {
	logger.debug(`>>> Entering offsetPos(pos=${pos},old=${old},nw=${nw})`);
    if (pos.line == old.line)
      return Pos(nw.line, pos.ch - old.ch + nw.ch);
    else
	logger.debug(`<<< Exiting offsetPos(pos=${pos},old=${old},nw=${nw})`);
      return Pos(nw.line + (pos.line - old.line), pos.ch);
  }

  // Used by replaceSelections to allow moving the selection to the
  // start or around the replaced test. Hint may be "start" or "around".
  function computeReplacedSel(doc, changes, hint) {
	logger.debug(`>>> Entering computeReplacedSel(doc=${doc},changes=${changes},hint=${hint})`);
    var out = [];
    var oldPrev = Pos(doc.first, 0), newPrev = oldPrev;
    for (var i = 0; i < changes.length; i++) {
      var change = changes[i];
      var from = offsetPos(change.from, oldPrev, newPrev);
      var to = offsetPos(changeEnd(change), oldPrev, newPrev);
      oldPrev = change.to;
      newPrev = to;
      if (hint == "around") {
        var range = doc.sel.ranges[i], inv = cmp(range.head, range.anchor) < 0;
		logger.debug(`computeReplacedSel(doc=${doc},changes=${changes},hint=${hint}): range → ${range}`);
        out[i] = new Range(inv ? to : from, inv ? from : to);
      } else {
        out[i] = new Range(from, from);
      }
    }
	logger.debug(`computeReplacedSel(doc=${doc},changes=${changes},hint=${hint}): newPrev → ${newPrev}`);
	logger.debug(`<<< Exiting computeReplacedSel(doc=${doc},changes=${changes},hint=${hint})`);
    return new Selection(out, doc.sel.primIndex);
  }

  // Allow "beforeChange" event handlers to influence a change
  function filterChange(doc, change, update) {
	logger.debug(`>>> Entering filterChange(doc=${doc},change=${change},update=${update})`);
    var obj = {
      canceled: false,
      from: change.from,
      to: change.to,
      text: change.text,
      origin: change.origin,
      cancel: function() { this.canceled = true; }
    };
    if (update) obj.update = function(from, to, text, origin) {
      if (from) this.from = clipPos(doc, from);
      if (to) this.to = clipPos(doc, to);
      if (text) this.text = text;
      if (origin !== undefined) this.origin = origin;
	logger.debug(`>>> Entering update(line=${line},text=${text},spans=${spans})`);
	logger.debug(`<<< Exiting update(line=${line},text=${text},spans=${spans})`);
    };
    signal(doc, "beforeChange", doc, obj);
    if (doc.cm) signal(doc.cm, "beforeChange", doc.cm, obj);

    if (obj.canceled) return null;
	logger.debug(`<<< Exiting filterChange(doc=${doc},change=${change},update=${update})`);
    return {from: obj.from, to: obj.to, text: obj.text, origin: obj.origin};
  }

  // Apply a change to a document, and add it to the document's
  // history, and propagating it to all linked documents.
  function makeChange(doc, change, ignoreReadOnly) {
	logger.debug(`>>> Entering makeChange(doc=${doc},change=${change},ignoreReadOnly=${ignoreReadOnly})`);
    if (doc.cm) {
      if (!doc.cm.curOp) return operation(doc.cm, makeChange)(doc, change, ignoreReadOnly);
	logger.debug(`<<< Exiting makeChange(doc=${doc},change=${change},ignoreReadOnly=${ignoreReadOnly})`);
      if (doc.cm.state.suppressEdits) return;
    }

    if (hasHandler(doc, "beforeChange") || doc.cm && hasHandler(doc.cm, "beforeChange")) {
      change = filterChange(doc, change, true);
      if (!change) return;
	logger.debug(`makeChange(doc=${doc},change=${change},ignoreReadOnly=${ignoreReadOnly}): change → ${change}`);
    }

    // Possibly split or suppress the update based on the presence
    // of read-only spans in its range.
    var split = sawReadOnlySpans && !ignoreReadOnly && removeReadOnlyRanges(doc, change.from, change.to);
	logger.debug(`makeChange(doc=${doc},change=${change},ignoreReadOnly=${ignoreReadOnly}): split → ${split}`);
    if (split) {
      for (var i = split.length - 1; i >= 0; --i)
        makeChangeInner(doc, {from: split[i].from, to: split[i].to, text: i ? [""] : change.text});
    } else {
      makeChangeInner(doc, change);
	logger.debug(`>>> Entering removeReadOnlyRanges(doc=${doc},from=${from},to=${to})`);
	logger.debug(`<<< Exiting removeReadOnlyRanges(doc=${doc},from=${from},to=${to})`);
    }
  }

  function makeChangeInner(doc, change) {
	logger.debug(`>>> Entering makeChangeInner(doc=${doc},change=${change})`);
    if (change.text.length == 1 && change.text[0] == "" && cmp(change.from, change.to) == 0) return;
    var selAfter = computeSelAfterChange(doc, change);
	logger.debug(`makeChangeInner(doc=${doc},change=${change}): selAfter → ${selAfter}`);
    addChangeToHistory(doc, change, selAfter, doc.cm ? doc.cm.curOp.id : NaN);

    makeChangeSingleDoc(doc, change, selAfter, stretchSpansOverChange(doc, change));
    var rebased = [];
	logger.debug(`<<< Exiting makeChangeInner(doc=${doc},change=${change})`);

    linkedDocs(doc, function(doc, sharedHist) {
	logger.debug(`>>> Entering linkedDocs(doc=${doc})`);
      if (!sharedHist && indexOf(rebased, doc.history) == -1) {
        rebaseHist(doc.history, change);
        rebased.push(doc.history);
      }
      makeChangeSingleDoc(doc, change, null, stretchSpansOverChange(doc, change));
    });
	logger.debug(`<<< Exiting linkedDocs(doc=${doc})`);
  }

  // Revert a change stored in a document's history.
  function makeChangeFromHistory(doc, type, allowSelectionOnly) {
	logger.debug(`>>> Entering makeChangeFromHistory(doc=${doc},type=${type},allowSelectionOnly=${allowSelectionOnly})`);
    if (doc.cm && doc.cm.state.suppressEdits) return;

    var hist = doc.history, event, selAfter = doc.sel;
    var source = type == "undo" ? hist.done : hist.undone, dest = type == "undo" ? hist.undone : hist.done;
	logger.debug(`makeChangeFromHistory(doc=${doc},type=${type},allowSelectionOnly=${allowSelectionOnly}): source → ${source}`);

    // Verify that there is a useable event (so that ctrl-z won't
    // needlessly clear selection events)
    for (var i = 0; i < source.length; i++) {
      event = source[i];
      if (allowSelectionOnly ? event.ranges && !event.equals(doc.sel) : !event.ranges)
        break;
    }
    if (i == source.length) return;
    hist.lastOrigin = hist.lastSelOrigin = null;
	logger.debug(`makeChangeFromHistory(doc=${doc},type=${type},allowSelectionOnly=${allowSelectionOnly}): hist.lastOrigin → ${hist.lastOrigin}`);

    for (;;) {
      event = source.pop();
      if (event.ranges) {
        pushSelectionToHistory(event, dest);
        if (allowSelectionOnly && !event.equals(doc.sel)) {
          setSelection(doc, event, {clearRedo: false});
          return;
        }
        selAfter = event;
      }
      else break;
    }

    // Build up a reverse change object to add to the opposite history
    // stack (redo when undoing, and vice versa).
    var antiChanges = [];
    pushSelectionToHistory(selAfter, dest);
    dest.push({changes: antiChanges, generation: hist.generation});
    hist.generation = event.generation || ++hist.maxGeneration;
	logger.debug(`makeChangeFromHistory(doc=${doc},type=${type},allowSelectionOnly=${allowSelectionOnly}): hist.generation → ${hist.generation}`);

    var filter = hasHandler(doc, "beforeChange") || doc.cm && hasHandler(doc.cm, "beforeChange");
	logger.debug(`makeChangeFromHistory(doc=${doc},type=${type},allowSelectionOnly=${allowSelectionOnly}): filter → ${filter}`);

    for (var i = event.changes.length - 1; i >= 0; --i) {
      var change = event.changes[i];
      change.origin = type;
	logger.debug(`makeChangeFromHistory(doc=${doc},type=${type},allowSelectionOnly=${allowSelectionOnly}): change.origin → ${change.origin}`);
      if (filter && !filterChange(doc, change, false)) {
        source.length = 0;
		logger.debug(`makeChangeFromHistory(doc=${doc},type=${type},allowSelectionOnly=${allowSelectionOnly}): source.length → ${source.length}`);
        return;
      }

      antiChanges.push(historyChangeFromChange(doc, change));

      var after = i ? computeSelAfterChange(doc, change) : lst(source);
	logger.debug(`makeChangeFromHistory(doc=${doc},type=${type},allowSelectionOnly=${allowSelectionOnly}): after → ${after}`);
      makeChangeSingleDoc(doc, change, after, mergeOldSpans(doc, change));
      if (!i && doc.cm) doc.cm.scrollIntoView({from: change.from, to: changeEnd(change)});
      var rebased = [];

      // Propagate to the linked documents
      linkedDocs(doc, function(doc, sharedHist) {
        if (!sharedHist && indexOf(rebased, doc.history) == -1) {
          rebaseHist(doc.history, change);
          rebased.push(doc.history);
        }
        makeChangeSingleDoc(doc, change, null, mergeOldSpans(doc, change));
      });
	logger.debug(`>>> Entering hasHandler(emitter=${emitter},type=${type})`);
    }
	logger.debug(`<<< Exiting hasHandler(emitter=${emitter},type=${type})`);
	logger.debug(`<<< Exiting makeChangeFromHistory(doc=${doc},type=${type},allowSelectionOnly=${allowSelectionOnly})`);
  }

  // Sub-views need their line numbers shifted when text is added
  // above or below them in the parent document.
  function shiftDoc(doc, distance) {
	logger.debug(`>>> Entering shiftDoc(doc=${doc},distance=${distance})`);
    if (distance == 0) return;
    doc.first += distance;
    doc.sel = new Selection(map(doc.sel.ranges, function(range) {
      return new Range(Pos(range.anchor.line + distance, range.anchor.ch),
                       Pos(range.head.line + distance, range.head.ch));
    }), doc.sel.primIndex);
    if (doc.cm) {
      regChange(doc.cm, doc.first, doc.first - distance, distance);
      for (var d = doc.cm.display, l = d.viewFrom; l < d.viewTo; l++)
        regLineChange(doc.cm, l, "gutter");
    }
	logger.debug(`<<< Exiting shiftDoc(doc=${doc},distance=${distance})`);
  }

  // More lower-level change function, handling only a single document
  // (not linked ones).
  function makeChangeSingleDoc(doc, change, selAfter, spans) {
	logger.debug(`>>> Entering makeChangeSingleDoc(doc=${doc},change=${change},selAfter=${selAfter},spans=${spans})`);
    if (doc.cm && !doc.cm.curOp)
      return operation(doc.cm, makeChangeSingleDoc)(doc, change, selAfter, spans);

    if (change.to.line < doc.first) {
      shiftDoc(doc, change.text.length - 1 - (change.to.line - change.from.line));
      return;
    }
    if (change.from.line > doc.lastLine()) return;

    // Clip the change to the size of this doc
    if (change.from.line < doc.first) {
      var shift = change.text.length - 1 - (doc.first - change.from.line);
	logger.debug(`makeChangeSingleDoc(doc=${doc},change=${change},selAfter=${selAfter},spans=${spans}): shift → ${shift}`);
      shiftDoc(doc, shift);
      change = {from: Pos(doc.first, 0), to: Pos(change.to.line + shift, change.to.ch),
                text: [lst(change.text)], origin: change.origin};
    }
    var last = doc.lastLine();
	logger.debug(`makeChangeSingleDoc(doc=${doc},change=${change},selAfter=${selAfter},spans=${spans}): last → ${last}`);
    if (change.to.line > last) {
      change = {from: change.from, to: Pos(last, getLine(doc, last).text.length),
                text: [change.text[0]], origin: change.origin};
    }

    change.removed = getBetween(doc, change.from, change.to);

    if (!selAfter) selAfter = computeSelAfterChange(doc, change);
    if (doc.cm) makeChangeSingleDocInEditor(doc.cm, change, spans);
    else updateDoc(doc, change, spans);
    setSelectionNoUndo(doc, selAfter, sel_dontScroll);
	logger.debug(`<<< Exiting makeChangeSingleDoc(doc=${doc},change=${change},selAfter=${selAfter},spans=${spans})`);
  }

  // Handle the interaction of a change to a document with the editor
  // that this document is part of.
  function makeChangeSingleDocInEditor(cm, change, spans) {
	logger.debug(`>>> Entering makeChangeSingleDocInEditor(cm=${cm},change=${change},spans=${spans})`);
    var doc = cm.doc, display = cm.display, from = change.from, to = change.to;

    var recomputeMaxLength = false, checkWidthStart = from.line;
    if (!cm.options.lineWrapping) {
      checkWidthStart = lineNo(visualLine(getLine(doc, from.line)));
      doc.iter(checkWidthStart, to.line + 1, function(line) {
        if (line == display.maxLine) {
          recomputeMaxLength = true;
          return true;
        }
      });
    }

    if (doc.sel.contains(change.from, change.to) > -1)
      signalCursorActivity(cm);

    updateDoc(doc, change, spans, estimateHeight(cm));

    if (!cm.options.lineWrapping) {
      doc.iter(checkWidthStart, from.line + change.text.length, function(line) {
        var len = lineLength(line);
		logger.debug(`iter(checkWidthStart=${checkWidthStart},len=${len}): len → ${len}`);
        if (len > display.maxLineLength) {
          display.maxLine = line;
		logger.debug(`makeChangeSingleDocInEditor(cm=${cm},change=${change},spans=${spans}): display.maxLine → ${display.maxLine}`);
		logger.debug(`makeChangeSingleDocInEditor(cm=${cm},change=${change},spans=${spans}): display.maxLineChanged → ${display.maxLineChanged}`);
          display.maxLineLength = len;
		logger.debug(`makeChangeSingleDocInEditor(cm=${cm},change=${change},spans=${spans}): display.maxLineLength → ${display.maxLineLength}`);
          display.maxLineChanged = true;
          recomputeMaxLength = false;
        }
      });
      if (recomputeMaxLength) cm.curOp.updateMaxLine = true;
    }

    // Adjust frontier, schedule worker
    doc.frontier = Math.min(doc.frontier, from.line);
    startWorker(cm, 400);

    var lendiff = change.text.length - (to.line - from.line) - 1;
	logger.debug(`makeChangeSingleDocInEditor(cm=${cm},change=${change},spans=${spans}): lendiff → ${lendiff}`);
    // Remember that these lines changed, for updating the display
    if (change.full)
      regChange(cm);
    else if (from.line == to.line && change.text.length == 1 && !isWholeLineUpdate(cm.doc, change))
      regLineChange(cm, from.line, "text");
    else
      regChange(cm, from.line, to.line + 1, lendiff);

    var changesHandler = hasHandler(cm, "changes"), changeHandler = hasHandler(cm, "change");
    if (changeHandler || changesHandler) {
      var obj = {
        from: from, to: to,
        text: change.text,
        removed: change.removed,
        origin: change.origin
      };
      if (changeHandler) signalLater(cm, "change", cm, obj);
      if (changesHandler) (cm.curOp.changeObjs || (cm.curOp.changeObjs = [])).push(obj);
    }
    cm.display.selForContextMenu = null;
	logger.debug(`makeChangeSingleDocInEditor(cm=${cm},change=${change},spans=${spans}): changesHandler → ${changesHandler}`);
	logger.debug(`<<< Exiting makeChangeSingleDocInEditor(cm=${cm},change=${change},spans=${spans})`);
  }

  function replaceRange(doc, code, from, to, origin) {
	logger.debug(`>>> Entering replaceRange(doc=${doc},code=${code},from=${from},to=${to},origin=${origin})`);
    if (!to) to = from;
    if (cmp(to, from) < 0) { var tmp = to; to = from; from = tmp; }
    if (typeof code == "string") code = doc.splitLines(code);
    makeChange(doc, {from: from, to: to, text: code, origin: origin});
	logger.debug(`<<< Exiting replaceRange(doc=${doc},code=${code},from=${from},to=${to},origin=${origin})`);
	logger.debug(`>>> Entering replaceRange(code=${code},from=${from},to=${to},origin=${origin})`);
	logger.debug(`<<< Exiting replaceRange(code=${code},from=${from},to=${to},origin=${origin})`);
  }

  // SCROLLING THINGS INTO VIEW

  // If an editor sits on the top or bottom of the window, partially
  // scrolled out of view, this ensures that the cursor is visible.
  function maybeScrollWindow(cm, coords) {
	logger.debug(`>>> Entering maybeScrollWindow(cm=${cm},coords=${coords})`);
    if (signalDOMEvent(cm, "scrollCursorIntoView")) return;

    var display = cm.display, box = display.sizer.getBoundingClientRect(), doScroll = null;
	logger.debug(`maybeScrollWindow(cm=${cm},coords=${coords}): display → ${display}`);
    if (coords.top + box.top < 0) doScroll = true;
    else if (coords.bottom + box.top > (window.innerHeight || document.documentElement.clientHeight)) doScroll = false;
    if (doScroll != null && !phantom) {
      var scrollNode = elt("div", "\u200b", null, "position: absolute; top: " +
                           (coords.top - display.viewOffset - paddingTop(cm.display)) + "px; height: " +
                           (coords.bottom - coords.top + scrollGap(cm) + display.barHeight) + "px; left: " +
                           coords.left + "px; width: 2px;");
      cm.display.lineSpace.appendChild(scrollNode);
      scrollNode.scrollIntoView(doScroll);
      cm.display.lineSpace.removeChild(scrollNode);
	logger.debug(`<<< Exiting maybeScrollWindow(cm=${cm},coords=${coords})`);
    }
  }

  // Scroll a given position into view (immediately), verifying that
  // it actually became visible (as line heights are accurately
  // measured, the position of something may 'drift' during drawing).
  function scrollPosIntoView(cm, pos, end, margin) {
	logger.debug(`>>> Entering scrollPosIntoView(cm=${cm},pos=${pos},end=${end},margin=${margin})`);
    if (margin == null) margin = 0;
    for (var limit = 0; limit < 5; limit++) {
      var changed = false, coords = cursorCoords(cm, pos);
	logger.debug(`scrollPosIntoView(cm=${cm},pos=${pos},end=${end},margin=${margin}): changed → ${changed}`);
      var endCoords = !end || end == pos ? coords : cursorCoords(cm, end);
	logger.debug(`scrollPosIntoView(cm=${cm},pos=${pos},end=${end},margin=${margin}): endCoords → ${endCoords}`);
      var scrollPos = calculateScrollPos(cm, Math.min(coords.left, endCoords.left),
                                         Math.min(coords.top, endCoords.top) - margin,
                                         Math.max(coords.left, endCoords.left),
                                         Math.max(coords.bottom, endCoords.bottom) + margin);
      var startTop = cm.doc.scrollTop, startLeft = cm.doc.scrollLeft;
	logger.debug(`scrollPosIntoView(cm=${cm},pos=${pos},end=${end},margin=${margin}): scrollPos → ${scrollPos}`);
      if (scrollPos.scrollTop != null) {
        setScrollTop(cm, scrollPos.scrollTop);
        if (Math.abs(cm.doc.scrollTop - startTop) > 1) changed = true;
      }
      if (scrollPos.scrollLeft != null) {
        setScrollLeft(cm, scrollPos.scrollLeft);
        if (Math.abs(cm.doc.scrollLeft - startLeft) > 1) changed = true;
      }
      if (!changed) break;
    }
	logger.debug(`<<< Exiting scrollPosIntoView(cm=${cm},pos=${pos},end=${end},margin=${margin})`);
    return coords;
  }

  // Scroll a given set of coordinates into view (immediately).
  function scrollIntoView(cm, x1, y1, x2, y2) {
	logger.debug(`>>> Entering scrollIntoView(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2})`);
    var scrollPos = calculateScrollPos(cm, x1, y1, x2, y2);
    if (scrollPos.scrollTop != null) setScrollTop(cm, scrollPos.scrollTop);
	logger.debug(`scrollIntoView(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2}): scrollPos → ${scrollPos}`);
    if (scrollPos.scrollLeft != null) setScrollLeft(cm, scrollPos.scrollLeft);
	logger.debug(`<<< Exiting scrollIntoView(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2})`);
  }

  // Calculate a new scroll position needed to scroll the given
  // rectangle into view. Returns an object with scrollTop and
  // scrollLeft properties. When these are undefined, the
  // vertical/horizontal position does not need to be adjusted.
  function calculateScrollPos(cm, x1, y1, x2, y2) {
	logger.debug(`>>> Entering calculateScrollPos(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2})`);
    var display = cm.display, snapMargin = textHeight(cm.display);
    if (y1 < 0) y1 = 0;
	logger.debug(`calculateScrollPos(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2}): display → ${display}`);
    var screentop = cm.curOp && cm.curOp.scrollTop != null ? cm.curOp.scrollTop : display.scroller.scrollTop;
    var screen = displayHeight(cm), result = {};
	logger.debug(`calculateScrollPos(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2}): screen → ${screen}`);
    if (y2 - y1 > screen) y2 = y1 + screen;
    var docBottom = cm.doc.height + paddingVert(display);
	logger.debug(`calculateScrollPos(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2}): docBottom → ${docBottom}`);
    var atTop = y1 < snapMargin, atBottom = y2 > docBottom - snapMargin;
    if (y1 < screentop) {
      result.scrollTop = atTop ? 0 : y1;
	logger.debug(`calculateScrollPos(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2}): result.scrollTop → ${result.scrollTop}`);
    } else if (y2 > screentop + screen) {
      var newTop = Math.min(y1, (atBottom ? docBottom : y2) - screen);
	logger.debug(`calculateScrollPos(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2}): newTop → ${newTop}`);
      if (newTop != screentop) result.scrollTop = newTop;
    }

    var screenleft = cm.curOp && cm.curOp.scrollLeft != null ? cm.curOp.scrollLeft : display.scroller.scrollLeft;
    var screenw = displayWidth(cm) - (cm.options.fixedGutter ? display.gutters.offsetWidth : 0);
	logger.debug(`calculateScrollPos(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2}): screenw → ${screenw}`);
    var tooWide = x2 - x1 > screenw;
    if (tooWide) x2 = x1 + screenw;
    if (x1 < 10)
      result.scrollLeft = 0;
    else if (x1 < screenleft)
      result.scrollLeft = Math.max(0, x1 - (tooWide ? 0 : 10));
    else if (x2 > screenw + screenleft - 3)
      result.scrollLeft = x2 + (tooWide ? 0 : 10) - screenw;
	logger.debug(`<<< Exiting calculateScrollPos(cm=${cm},x1=${x1},y1=${y1},x2=${x2},y2=${y2})`);
    return result;
  }

  // Store a relative adjustment to the scroll position in the current
  // operation (to be applied when the operation finishes).
  function addToScrollPos(cm, left, top) {
	logger.debug(`>>> Entering addToScrollPos(cm=${cm},left=${left},top=${top})`);
    if (left != null || top != null) resolveScrollToPos(cm);
    if (left != null)
      cm.curOp.scrollLeft = (cm.curOp.scrollLeft == null ? cm.doc.scrollLeft : cm.curOp.scrollLeft) + left;
    if (top != null)
	logger.debug(`<<< Exiting addToScrollPos(cm=${cm},left=${left},top=${top})`);
      cm.curOp.scrollTop = (cm.curOp.scrollTop == null ? cm.doc.scrollTop : cm.curOp.scrollTop) + top;
  }

  // Make sure that at the end of the operation the current cursor is
  // shown.
  function ensureCursorVisible(cm) {
	logger.debug(`>>> Entering ensureCursorVisible(cm=${cm})`);
    resolveScrollToPos(cm);
    var cur = cm.getCursor(), from = cur, to = cur;
    if (!cm.options.lineWrapping) {
      from = cur.ch ? Pos(cur.line, cur.ch - 1) : cur;
      to = Pos(cur.line, cur.ch + 1);
    }
    cm.curOp.scrollToPos = {from: from, to: to, margin: cm.options.cursorScrollMargin, isCursor: true};
	logger.debug(`ensureCursorVisible(cm=${cm}): cur → ${cur}`);
	logger.debug(`<<< Exiting ensureCursorVisible(cm=${cm})`);
  }

  // When an operation has its scrollToPos property set, and another
  // scroll action is applied before the end of the operation, this
  // 'simulates' scrolling that position into view in a cheap way, so
  // that the effect of intermediate scroll commands is not ignored.
  function resolveScrollToPos(cm) {
	logger.debug(`>>> Entering resolveScrollToPos(cm=${cm})`);
    var range = cm.curOp.scrollToPos;
    if (range) {
      cm.curOp.scrollToPos = null;
      var from = estimateCoords(cm, range.from), to = estimateCoords(cm, range.to);
      var sPos = calculateScrollPos(cm, Math.min(from.left, to.left),
                                    Math.min(from.top, to.top) - range.margin,
                                    Math.max(from.right, to.right),
                                    Math.max(from.bottom, to.bottom) + range.margin);
      cm.scrollTo(sPos.scrollLeft, sPos.scrollTop);
	logger.debug(`resolveScrollToPos(cm=${cm}): sPos → ${sPos}`);
    }
	logger.debug(`<<< Exiting resolveScrollToPos(cm=${cm})`);
  }

  // API UTILITIES

  // Indent the given line. The how parameter can be "smart",
  // "add"/null, "subtract", or "prev". When aggressive is false
  // (typically set to true for forced single-line indents), empty
  // lines are not indented, and places where the mode returns Pass
  // are left alone.
  function indentLine(cm, n, how, aggressive) {
	logger.debug(`>>> Entering indentLine(cm=${cm},n=${n},how=${how},aggressive=${aggressive})`);
    var doc = cm.doc, state;
    if (how == null) how = "add";
    if (how == "smart") {
      // Fall back to "prev" when the mode doesn't have an indentation
      // method.
      if (!doc.mode.indent) how = "prev";
      else state = getStateBefore(cm, n);
    }

    var tabSize = cm.options.tabSize;
    var line = getLine(doc, n), curSpace = countColumn(line.text, null, tabSize);
	logger.debug(`indentLine(cm=${cm},n=${n},how=${how},aggressive=${aggressive}): line → ${line}`);
    if (line.stateAfter) line.stateAfter = null;
    var curSpaceString = line.text.match(/^\s*/)[0], indentation;
	logger.debug(`indentLine(cm=${cm},n=${n},how=${how},aggressive=${aggressive}): curSpaceString → ${curSpaceString}`);
    if (!aggressive && !/\S/.test(line.text)) {
      indentation = 0;
      how = "not";
    } else if (how == "smart") {
      indentation = doc.mode.indent(state, line.text.slice(curSpaceString.length), line.text);
      if (indentation == Pass || indentation > 150) {
        if (!aggressive) return;
        how = "prev";
      }
    }
    if (how == "prev") {
      if (n > doc.first) indentation = countColumn(getLine(doc, n-1).text, null, tabSize);
      else indentation = 0;
    } else if (how == "add") {
      indentation = curSpace + cm.options.indentUnit;
    } else if (how == "subtract") {
      indentation = curSpace - cm.options.indentUnit;
    } else if (typeof how == "number") {
      indentation = curSpace + how;
	logger.debug(`indentLine(cm=${cm},n=${n},how=${how},aggressive=${aggressive}): how → ${how}`);
    }
    indentation = Math.max(0, indentation);

    var indentString = "", pos = 0;
	logger.debug(`indentLine(cm=${cm},n=${n},how=${how},aggressive=${aggressive}): indentString → ${indentString}`);
    if (cm.options.indentWithTabs)
      for (var i = Math.floor(indentation / tabSize); i; --i) {pos += tabSize; indentString += "\t";}
    if (pos < indentation) indentString += spaceStr(indentation - pos);

    if (indentString != curSpaceString) {
      replaceRange(doc, indentString, Pos(n, 0), Pos(n, curSpaceString.length), "+input");
      line.stateAfter = null;
	logger.debug(`indentLine(cm=${cm},n=${n},how=${how},aggressive=${aggressive}): line.stateAfter → ${line.stateAfter}`);
      return true;
    } else {
      // Ensure that, if the cursor was in the whitespace at the start
      // of the line, it is moved to the end of that space.
      for (var i = 0; i < doc.sel.ranges.length; i++) {
        var range = doc.sel.ranges[i];
        if (range.head.line == n && range.head.ch < curSpaceString.length) {
          var pos = Pos(n, curSpaceString.length);
		logger.debug(`indentLine(cm=${cm},n=${n},how=${how},aggressive=${aggressive}): pos → ${pos}`);
          replaceOneSelection(doc, i, new Range(pos, pos));
          break;
        }
      }
    }
	logger.debug(`<<< Exiting indentLine(cm=${cm},n=${n},how=${how},aggressive=${aggressive})`);
  }

  // Utility for applying a change to a line by handle or number,
  // returning the number and optionally registering the line as
  // changed.
  function changeLine(doc, handle, changeType, op) {
	logger.debug(`>>> Entering changeLine(doc=${doc},handle=${handle},changeType=${changeType},op=${op})`);
    var no = handle, line = handle;
    if (typeof handle == "number") line = getLine(doc, clipLine(doc, handle));
    else no = lineNo(handle);
    if (no == null) return null;
    if (op(line, no) && doc.cm) regLineChange(doc.cm, no, changeType);
	logger.debug(`<<< Exiting changeLine(doc=${doc},handle=${handle},changeType=${changeType},op=${op})`);
    return line;
  }

  // Helper for deleting text near the selection(s), used to implement
  // backspace, delete, and similar functionality.
  function deleteNearSelection(cm, compute) {
	logger.debug(`>>> Entering deleteNearSelection(cm=${cm},compute=${compute})`);
    var ranges = cm.doc.sel.ranges, kill = [];
    // Build up a set of ranges to kill first, merging overlapping
    // ranges.
    for (var i = 0; i < ranges.length; i++) {
      var toKill = compute(ranges[i]);
	logger.debug(`deleteNearSelection(cm=${cm},compute=${compute}): toKill → ${toKill}`);
      while (kill.length && cmp(toKill.from, lst(kill).to) <= 0) {
        var replaced = kill.pop();
		logger.debug(`deleteNearSelection(cm=${cm},compute=${compute}): replaced → ${replaced}`);
        if (cmp(replaced.from, toKill.from) < 0) {
          toKill.from = replaced.from;
		logger.debug(`deleteNearSelection(cm=${cm},compute=${compute}): toKill.from → ${toKill.from}`);
          break;
        }
      }
      kill.push(toKill);
	logger.debug(`<<< Exiting deleteNearSelection(cm=${cm},compute=${compute})`);
    }
    // Next, remove those actual ranges.
    runInOp(cm, function() {
      for (var i = kill.length - 1; i >= 0; i--)
        replaceRange(cm.doc, "", kill[i].from, kill[i].to, "+delete");
      ensureCursorVisible(cm);
    });
	logger.debug(`>>> Entering deleteNearSelection(cm=${cm})`);
  }

  // Used for horizontal relative motion. Dir is -1 or 1 (left or
  // right), unit can be "char", "column" (like char, but doesn't
  // cross line boundaries), "word" (across next word), or "group" (to
  // the start of next group of word or non-word-non-whitespace
  // chars). The visually param controls whether, in right-to-left
  // text, direction 1 means to move towards the next index in the
  // string, or towards the character to the right of the current
  // position. The resulting position will have a hitSide=true
  // property if it reached the end of the document.
  function findPosH(doc, pos, dir, unit, visually) {
	logger.debug(`>>> Entering findPosH(doc=${doc},pos=${pos},dir=${dir},unit=${unit},visually=${visually})`);
    var line = pos.line, ch = pos.ch, origDir = dir;
    var lineObj = getLine(doc, line);
    var possible = true;
	logger.debug(`findPosH(doc=${doc},pos=${pos},dir=${dir},unit=${unit},visually=${visually}): lineObj → ${lineObj}`);
    function findNextLine() {
	logger.debug('>>> Entering findNextLine()');
      var l = line + dir;
      if (l < doc.first || l >= doc.first + doc.size) return (possible = false);
      line = l;
	logger.debug(`findNextLine(): line → ${line}`);
	logger.debug('<<< Exiting findNextLine()');
      return lineObj = getLine(doc, l);
    }
    function moveOnce(boundToLine) {
	logger.debug(`>>> Entering moveOnce(boundToLine=${boundToLine})`);
      var next = (visually ? moveVisually : moveLogically)(lineObj, ch, dir, true);
      if (next == null) {
        if (!boundToLine && findNextLine()) {
          if (visually) ch = (dir < 0 ? lineRight : lineLeft)(lineObj);
          else ch = dir < 0 ? lineObj.text.length : 0;
        } else return (possible = false);
      } else ch = next;
	logger.debug(`<<< Exiting moveOnce(boundToLine=${boundToLine})`);
	logger.debug('>>> Entering next()');
      return true;
    }

    if (unit == "char") moveOnce();
    else if (unit == "column") moveOnce(true);
    else if (unit == "word" || unit == "group") {
      var sawType = null, group = unit == "group";
      var helper = doc.cm && doc.cm.getHelper(pos, "wordChars");
      for (var first = true;; first = false) {
        if (dir < 0 && !moveOnce(!first)) break;
        var cur = lineObj.text.charAt(ch) || "\n";
        var type = isWordChar(cur, helper) ? "w"
          : group && cur == "\n" ? "n"
          : !group || /\s/.test(cur) ? null
          : "p";
        if (group && !first && !type) type = "s";
        if (sawType && sawType != type) {
          if (dir < 0) {dir = 1; moveOnce();}
          break;
        }

        if (type) sawType = type;
        if (dir > 0 && !moveOnce(!first)) break;
      }
    }
    var result = skipAtomic(doc, Pos(line, ch), origDir, true);
    if (!possible) result.hitSide = true;
	logger.debug(`<<< Exiting findPosH(doc=${doc},pos=${pos},dir=${dir},unit=${unit},visually=${visually})`);
	logger.debug('<<< Exiting next()');
    return result;
  }

  // For relative vertical movement. Dir may be -1 or 1. Unit can be
  // "page" or "line". The resulting position will have a hitSide=true
  // property if it reached the end of the document.
  function findPosV(cm, pos, dir, unit) {
	logger.debug(`>>> Entering findPosV(cm=${cm},pos=${pos},dir=${dir},unit=${unit})`);
    var doc = cm.doc, x = pos.left, y;
    if (unit == "page") {
      var pageSize = Math.min(cm.display.wrapper.clientHeight, window.innerHeight || document.documentElement.clientHeight);
	logger.debug(`findPosV(cm=${cm},pos=${pos},dir=${dir},unit=${unit}): pageSize → ${pageSize}`);
      y = pos.top + dir * (pageSize - (dir < 0 ? 1.5 : .5) * textHeight(cm.display));
    } else if (unit == "line") {
      y = dir > 0 ? pos.bottom + 3 : pos.top - 3;
    }
    for (;;) {
      var target = coordsChar(cm, x, y);
	logger.debug(`findPosV(cm=${cm},pos=${pos},dir=${dir},unit=${unit}): target → ${target}`);
      if (!target.outside) break;
      if (dir < 0 ? y <= 0 : y >= doc.height) { target.hitSide = true; break; }
      y += dir * 5;
    }
	logger.debug(`<<< Exiting findPosV(cm=${cm},pos=${pos},dir=${dir},unit=${unit})`);
    return target;
  }

  // EDITOR METHODS

  // The publicly visible API. Note that methodOp(f) means
  // 'wrap f in an operation, performed on its `this` parameter'.

  // This is not the complete set of editor methods. Most of the
  // methods defined on the Doc type are also injected into
  // CodeMirror.prototype, for backwards compatibility and
  // convenience.

  CodeMirror.prototype = {
    constructor: CodeMirror,
    focus: function(){window.focus(); this.display.input.focus();},

    setOption: function(option, value) {
      var options = this.options, old = options[option];
      if (options[option] == value && option != "mode") return;
      options[option] = value;
      if (optionHandlers.hasOwnProperty(option))
        operation(this, optionHandlers[option])(this, value, old);
		logger.debug(`<<< Exiting setOption(option=${option},value=${value})`);
    },

    getOption: function(option) {return this.options[option];},
    getDoc: function() {return this.doc;},

    addKeyMap: function(map, bottom) {
	logger.debug(`>>> Entering addKeyMap(map=${map},bottom=${bottom})`);
      this.state.keyMaps[bottom ? "push" : "unshift"](getKeyMap(map));
	logger.debug(`<<< Exiting addKeyMap(map=${map},bottom=${bottom})`);
    },
    removeKeyMap: function(map) {
	logger.debug(`>>> Entering removeKeyMap(map=${map})`);
      var maps = this.state.keyMaps;
      for (var i = 0; i < maps.length; ++i)
        if (maps[i] == map || maps[i].name == map) {
          maps.splice(i, 1);
		logger.debug(`<<< Exiting removeKeyMap(map=${map})`);
          return true;
        }
    },

    addOverlay: methodOp(function(spec, options) {
      var mode = spec.token ? spec : CodeMirror.getMode(this.options, spec);
      if (mode.startState) throw new Error("Overlays may not be stateful.");
      this.state.overlays.push({mode: mode, modeSpec: spec, opaque: options && options.opaque});
      this.state.modeGen++;
      regChange(this);
    }),
    removeOverlay: methodOp(function(spec) {
      var overlays = this.state.overlays;
      for (var i = 0; i < overlays.length; ++i) {
        var cur = overlays[i].modeSpec;
        if (cur == spec || typeof spec == "string" && cur.name == spec) {
          overlays.splice(i, 1);
          this.state.modeGen++;
          regChange(this);
          return;
        }
      }
    }),

    indentLine: methodOp(function(n, dir, aggressive) {
      if (typeof dir != "string" && typeof dir != "number") {
        if (dir == null) dir = this.options.smartIndent ? "smart" : "prev";
        else dir = dir ? "add" : "subtract";
      }
      if (isLine(this.doc, n)) indentLine(this, n, dir, aggressive);
    }),
    indentSelection: methodOp(function(how) {
      var ranges = this.doc.sel.ranges, end = -1;
      for (var i = 0; i < ranges.length; i++) {
        var range = ranges[i];
        if (!range.empty()) {
          var from = range.from(), to = range.to();
          var start = Math.max(end, from.line);
          end = Math.min(this.lastLine(), to.line - (to.ch ? 0 : 1)) + 1;
          for (var j = start; j < end; ++j)
            indentLine(this, j, how);
          var newRanges = this.doc.sel.ranges;
          if (from.ch == 0 && ranges.length == newRanges.length && newRanges[i].from().ch > 0)
            replaceOneSelection(this.doc, i, new Range(from, newRanges[i].to()), sel_dontScroll);
        } else if (range.head.line > end) {
          indentLine(this, range.head.line, how, true);
          end = range.head.line;
          if (i == this.doc.sel.primIndex) ensureCursorVisible(this);
        }
      }
    }),

    // Fetch the parser token for a given character. Useful for hacks
    // that want to inspect the mode state (say, for completion).
    getTokenAt: function(pos, precise) {
	logger.debug(`>>> Entering getTokenAt(pos=${pos},precise=${precise})`);
	logger.debug(`<<< Exiting getTokenAt(pos=${pos},precise=${precise})`);
      return takeToken(this, pos, precise);
    },

    getLineTokens: function(line, precise) {
	logger.debug(`>>> Entering getLineTokens(line=${line},precise=${precise})`);
	logger.debug(`<<< Exiting getLineTokens(line=${line},precise=${precise})`);
      return takeToken(this, Pos(line), precise, true);
    },

    getTokenTypeAt: function(pos) {
	logger.debug(`>>> Entering getTokenTypeAt(pos=${pos})`);
      pos = clipPos(this.doc, pos);
      var styles = getLineStyles(this, getLine(this.doc, pos.line));
	logger.debug(`getTokenTypeAt(pos=${pos}): pos → ${pos}`);
	logger.debug(`getTokenTypeAt(pos=${pos}): styles → ${styles}`);
      var before = 0, after = (styles.length - 1) / 2, ch = pos.ch;
      var type;
	logger.debug(`getTokenTypeAt(pos=${pos}): before → ${before}`);
      if (ch == 0) type = styles[2];
      else for (;;) {
        var mid = (before + after) >> 1;
        if ((mid ? styles[mid * 2 - 1] : 0) >= ch) after = mid;
        else if (styles[mid * 2 + 1] < ch) before = mid + 1;
        else { type = styles[mid * 2 + 2]; break; }
      }
      var cut = type ? type.indexOf("cm-overlay ") : -1;
      return cut < 0 ? type : cut == 0 ? null : type.slice(0, cut - 1);
    },

    getModeAt: function(pos) {
	logger.debug(`>>> Entering getModeAt(pos=${pos})`);
      var mode = this.doc.mode;
      if (!mode.innerMode) return mode;
	logger.debug(`<<< Exiting getModeAt(pos=${pos})`);
      return CodeMirror.innerMode(mode, this.getTokenAt(pos).state).mode;
    },

    getHelper: function(pos, type) {
	logger.debug(`>>> Entering getHelper(pos=${pos},type=${type})`);
	logger.debug(`<<< Exiting getHelper(pos=${pos},type=${type})`);
      return this.getHelpers(pos, type)[0];
    },

    getHelpers: function(pos, type) {
      var found = [];
      if (!helpers.hasOwnProperty(type)) return found;
      var help = helpers[type], mode = this.getModeAt(pos);
      if (typeof mode[type] == "string") {
        if (help[mode[type]]) found.push(help[mode[type]]);
      } else if (mode[type]) {
        for (var i = 0; i < mode[type].length; i++) {
          var val = help[mode[type][i]];
          if (val) found.push(val);
        }
      } else if (mode.helperType && help[mode.helperType]) {
        found.push(help[mode.helperType]);
      } else if (help[mode.name]) {
        found.push(help[mode.name]);
      }
      for (var i = 0; i < help._global.length; i++) {
        var cur = help._global[i];
        if (cur.pred(mode, this) && indexOf(found, cur.val) == -1)
          found.push(cur.val);
      }
	logger.debug(`<<< Exiting getHelpers(pos=${pos},type=${type})`);
      return found;
    },

    getStateAfter: function(line, precise) {
	logger.debug(`>>> Entering getStateAfter(line=${line},precise=${precise})`);
      var doc = this.doc;
      line = clipLine(doc, line == null ? doc.first + doc.size - 1: line);
	logger.debug(`<<< Exiting getStateAfter(line=${line},precise=${precise})`);
	logger.debug(`getStateAfter(line=${line},precise=${precise}): line → ${line}`);
	logger.debug(`>>> Entering indexOf(array=${array},elt=${elt})`);
	logger.debug(`<<< Exiting indexOf(array=${array},elt=${elt})`);
      return getStateBefore(this, line + 1, precise);
    },

    cursorCoords: function(start, mode) {
      var pos, range = this.doc.sel.primary();
      if (start == null) pos = range.head;
      else if (typeof start == "object") pos = clipPos(this.doc, start);
      else pos = start ? range.from() : range.to();
      return cursorCoords(this, pos, mode || "page");
    },

    charCoords: function(pos, mode) {
      return charCoords(this, clipPos(this.doc, pos), mode || "page");
    },

    coordsChar: function(coords, mode) {
      coords = fromCoordSystem(this, coords, mode || "page");
      return coordsChar(this, coords.left, coords.top);
    },

    lineAtHeight: function(height, mode) {
      height = fromCoordSystem(this, {top: height, left: 0}, mode || "page").top;
	logger.debug(`>>> Entering lineAtHeight(chunk=${chunk},h=${h})`);
	logger.debug(`<<< Exiting lineAtHeight(chunk=${chunk},h=${h})`);
      return lineAtHeight(this.doc, height + this.display.viewOffset);
    },
    heightAtLine: function(line, mode) {
	logger.debug(`>>> Entering heightAtLine(line=${line},mode=${mode})`);
      var end = false, lineObj;
      if (typeof line == "number") {
        var last = this.doc.first + this.doc.size - 1;
        if (line < this.doc.first) line = this.doc.first;
        else if (line > last) { line = last; end = true; }
        lineObj = getLine(this.doc, line);
      } else {
        lineObj = line;
      }
      return intoCoordSystem(this, lineObj, {top: 0, left: 0}, mode || "page").top +
        (end ? this.doc.height - heightAtLine(lineObj) : 0);
		logger.debug(`>>> Entering heightAtLine(lineObj=${lineObj})`);
		logger.debug(`heightAtLine(lineObj=${lineObj}): lineObj → ${lineObj}`);
		logger.debug(`<<< Exiting heightAtLine(lineObj=${lineObj})`);
    },

    defaultTextHeight: function() { return textHeight(this.display); },
    defaultCharWidth: function() { return charWidth(this.display); },

    setGutterMarker: methodOp(function(line, gutterID, value) {
      return changeLine(this.doc, line, "gutter", function(line) {
        var markers = line.gutterMarkers || (line.gutterMarkers = {});
        markers[gutterID] = value;
        if (!value && isEmpty(markers)) line.gutterMarkers = null;
        return true;
      });
    }),

    clearGutter: methodOp(function(gutterID) {
      var cm = this, doc = cm.doc, i = doc.first;
      doc.iter(function(line) {
        if (line.gutterMarkers && line.gutterMarkers[gutterID]) {
          line.gutterMarkers[gutterID] = null;
          regLineChange(cm, i, "gutter");
          if (isEmpty(line.gutterMarkers)) line.gutterMarkers = null;
        }
        ++i;
      });
    }),

    lineInfo: function(line) {
	logger.debug(`>>> Entering lineInfo(line=${line})`);
      if (typeof line == "number") {
        if (!isLine(this.doc, line)) return null;
        var n = line;
        line = getLine(this.doc, line);
        if (!line) return null;
		logger.debug(`lineInfo(line=${line}): line → ${line}`);
      } else {
        var n = lineNo(line);
		logger.debug(`lineInfo(line=${line}): n → ${n}`);
        if (n == null) return null;
      }
      return {line: n, handle: line, text: line.text, gutterMarkers: line.gutterMarkers,
              textClass: line.textClass, bgClass: line.bgClass, wrapClass: line.wrapClass,
              widgets: line.widgets};
			logger.debug(`<<< Exiting lineInfo(line=${line})`);
    },

    getViewport: function() { return {from: this.display.viewFrom, to: this.display.viewTo};},

    addWidget: function(pos, node, scroll, vert, horiz) {
	logger.debug(`>>> Entering addWidget(pos=${pos},node=${node},scroll=${scroll},vert=${vert},horiz=${horiz})`);
      var display = this.display;
      pos = cursorCoords(this, clipPos(this.doc, pos));
      var top = pos.bottom, left = pos.left;
	logger.debug(`addWidget(pos=${pos},node=${node},scroll=${scroll},vert=${vert},horiz=${horiz}): pos → ${pos}`);
      node.style.position = "absolute";
      node.setAttribute("cm-ignore-events", "true");
      this.display.input.setUneditable(node);
      display.sizer.appendChild(node);
      if (vert == "over") {
        top = pos.top;
      } else if (vert == "above" || vert == "near") {
        var vspace = Math.max(display.wrapper.clientHeight, this.doc.height),
        hspace = Math.max(display.sizer.clientWidth, display.lineSpace.clientWidth);
		logger.debug(`addWidget(pos=${pos},node=${node},scroll=${scroll},vert=${vert},horiz=${horiz}): vspace → ${vspace}`);
        // Default to positioning above (if specified and possible); otherwise default to positioning below
        if ((vert == 'above' || pos.bottom + node.offsetHeight > vspace) && pos.top > node.offsetHeight)
          top = pos.top - node.offsetHeight;
        else if (pos.bottom + node.offsetHeight <= vspace)
          top = pos.bottom;
        if (left + node.offsetWidth > hspace)
          left = hspace - node.offsetWidth;
      }
      node.style.top = top + "px";
      node.style.left = node.style.right = "";
      if (horiz == "right") {
        left = display.sizer.clientWidth - node.offsetWidth;
        node.style.right = "0px";
      } else {
        if (horiz == "left") left = 0;
        else if (horiz == "middle") left = (display.sizer.clientWidth - node.offsetWidth) / 2;
        node.style.left = left + "px";
      }
      if (scroll)
		logger.debug(`addWidget(pos=${pos},node=${node},scroll=${scroll},vert=${vert},horiz=${horiz}): left → ${left}`);
        scrollIntoView(this, left, top, left + node.offsetWidth, top + node.offsetHeight);
		logger.debug(`<<< Exiting addWidget(pos=${pos},node=${node},scroll=${scroll},vert=${vert},horiz=${horiz})`);
    },

    triggerOnKeyDown: methodOp(onKeyDown),
    triggerOnKeyPress: methodOp(onKeyPress),
    triggerOnKeyUp: onKeyUp,

    execCommand: function(cmd) {
	logger.debug(`>>> Entering execCommand(cmd=${cmd})`);
      if (commands.hasOwnProperty(cmd))
		logger.debug(`<<< Exiting execCommand(cmd=${cmd})`);
        return commands[cmd].call(null, this);
    },

    triggerElectric: methodOp(function(text) { triggerElectric(this, text); }),

    findPosH: function(from, amount, unit, visually) {
      var dir = 1;
      if (amount < 0) { dir = -1; amount = -amount; }
      for (var i = 0, cur = clipPos(this.doc, from); i < amount; ++i) {
        cur = findPosH(this.doc, cur, dir, unit, visually);
        if (cur.hitSide) break;
      }
      return cur;
    },

    moveH: methodOp(function(dir, unit) {
      var cm = this;
      cm.extendSelectionsBy(function(range) {
		logger.debug('>>> Entering extendSelectionsBy()');
        if (cm.display.shift || cm.doc.extend || range.empty())
          return findPosH(cm.doc, range.head, dir, unit, cm.options.rtlMoveVisually);
        else
          return dir < 0 ? range.from() : range.to();
      }, sel_move);
    }),

    deleteH: methodOp(function(dir, unit) {
      var sel = this.doc.sel, doc = this.doc;
      if (sel.somethingSelected())
        doc.replaceSelection("", null, "+delete");
      else
        deleteNearSelection(this, function(range) {
          var other = findPosH(doc, range.head, dir, unit, false);
		logger.debug(`<<< Exiting deleteNearSelection(this=${this},other=${other})`);
		logger.debug(`deleteNearSelection(this=${this},other=${other}): other → ${other}`);
          return dir < 0 ? {from: other, to: range.head} : {from: range.head, to: other};
        });
    }),

    findPosV: function(from, amount, unit, goalColumn) {
      var dir = 1, x = goalColumn;
      if (amount < 0) { dir = -1; amount = -amount; }
      for (var i = 0, cur = clipPos(this.doc, from); i < amount; ++i) {
        var coords = cursorCoords(this, cur, "div");
        if (x == null) x = coords.left;
        else coords.left = x;
        cur = findPosV(this, coords, dir, unit);
        if (cur.hitSide) break;
      }
      return cur;
    },

    moveV: methodOp(function(dir, unit) {
      var cm = this, doc = this.doc, goals = [];
      var collapse = !cm.display.shift && !doc.extend && doc.sel.somethingSelected();
      doc.extendSelectionsBy(function(range) {
        if (collapse)
          return dir < 0 ? range.from() : range.to();
        var headPos = cursorCoords(cm, range.head, "div");
		logger.debug(`extendSelectionsBy(): headPos → ${headPos}`);
        if (range.goalColumn != null) headPos.left = range.goalColumn;
        goals.push(headPos.left);
        var pos = findPosV(cm, headPos, dir, unit);
        if (unit == "page" && range == doc.sel.primary())
          addToScrollPos(cm, null, charCoords(cm, pos, "div").top - headPos.top);
        return pos;
      }, sel_move);
      if (goals.length) for (var i = 0; i < doc.sel.ranges.length; i++)
        doc.sel.ranges[i].goalColumn = goals[i];
    }),

    // Find the word at the given position (as returned by coordsChar).
    findWordAt: function(pos) {
	logger.debug(`>>> Entering findWordAt(pos=${pos})`);
      var doc = this.doc, line = getLine(doc, pos.line).text;
      var start = pos.ch, end = pos.ch;
	logger.debug(`findWordAt(pos=${pos}): doc → ${doc}`);
      if (line) {
        var helper = this.getHelper(pos, "wordChars");
		logger.debug(`findWordAt(pos=${pos}): helper → ${helper}`);
        if ((pos.xRel < 0 || end == line.length) && start) --start; else ++end;
        var startChar = line.charAt(start);
		logger.debug(`findWordAt(pos=${pos}): startChar → ${startChar}`);
        var check = isWordChar(startChar, helper)
          ? function(ch) { return isWordChar(ch, helper); }
          : /\s/.test(startChar) ? function(ch) {return /\s/.test(ch);}
          : function(ch) {return !/\s/.test(ch) && !isWordChar(ch);};
        while (start > 0 && check(line.charAt(start - 1))) --start;
		logger.debug(`findWordAt(pos=${pos}): check → ${check}`);
        while (end < line.length && check(line.charAt(end))) ++end;
		logger.debug(`<<< Exiting findWordAt(pos=${pos})`);
		logger.debug(`function(spec=${spec}): spec → ${spec}`);
		logger.debug(`function(spec=${spec}): spec.name → ${spec.name}`);
		logger.debug(`function(options=${options},spec=${spec}): spec → ${spec}`);
		logger.debug(`function(mode=${mode},state=${state}): info → ${info}`);
		logger.debug(`function(keymap=${keymap}): val → ${val}`);
		logger.debug(`function(keymap=${keymap}): name → ${name}`);
		logger.debug(`function(value=${value}): name → ${name}`);
		logger.debug(`function(textarea=${textarea},options=${options}): options → ${options}`);
		logger.debug(`function(textarea=${textarea},options=${options}): options.value → ${options.value}`);
		logger.debug(`function(textarea=${textarea},options=${options}): options.tabindex → ${options.tabindex}`);
		logger.debug(`function(textarea=${textarea},options=${options}): options.placeholder → ${options.placeholder}`);
		logger.debug(`function(cm=${cm}): cm.save → ${cm.save}`);
		logger.debug(`function(cm=${cm}): cm.toTextArea → ${cm.toTextArea}`);
		logger.debug(`function(): cm.toTextArea → ${cm.toTextArea}`);
		logger.debug(`function(string=${string},tabSize=${tabSize}): this.pos → ${this.pos}`);
		logger.debug(`function(doc=${doc},type=${type}): this.lines → ${this.lines}`);
		logger.debug(`function(doc=${doc},type=${type}): this.doc → ${this.doc}`);
		logger.debug(`function(doc=${doc},type=${type}): this.id → ${this.id}`);
		logger.debug(`function(): widget.height → ${widget.height}`);
		logger.debug(`function(markers=${markers},primary=${primary}): this.markers → ${this.markers}`);
		logger.debug(`function(): this.explicitlyCleared → ${this.explicitlyCleared}`);
		logger.debug(`function(): cm → ${cm}`);
		logger.debug(`function(): height → ${height}`);
		logger.debug(`function(): this.height → ${this.height}`);
		logger.debug(`function(text=${text},mode=${mode},firstLine=${firstLine},lineSep=${lineSep}): this.first → ${this.first}`);
		logger.debug(`function(text=${text},mode=${mode},firstLine=${firstLine},lineSep=${lineSep}): this.scrollTop → ${this.scrollTop}`);
		logger.debug(`function(text=${text},mode=${mode},firstLine=${firstLine},lineSep=${lineSep}): this.cantEdit → ${this.cantEdit}`);
		logger.debug(`function(text=${text},mode=${mode},firstLine=${firstLine},lineSep=${lineSep}): this.cleanGeneration → ${this.cleanGeneration}`);
		logger.debug(`function(text=${text},mode=${mode},firstLine=${firstLine},lineSep=${lineSep}): this.frontier → ${this.frontier}`);
		logger.debug(`function(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue}): end → ${end}`);
		logger.error(`Exception in function(node=${node},start=${start},end=${end}): ${e.message}`, e);
		logger.debug(`function(): line → ${line}`);
		logger.debug(`function(): rt → ${rt}`);
		logger.error(`Exception in function(te=${te}): ${e.message}`, e);
		logger.debug(`function(te=${te}): range → ${range}`);
		logger.debug(`function(e=${e}): e → ${e}`);
      }
      return new Range(Pos(pos.line, start), Pos(pos.line, end));
    },

    toggleOverwrite: function(value) {
      if (value != null && value == this.state.overwrite) return;
      if (this.state.overwrite = !this.state.overwrite)
        addClass(this.display.cursorDiv, "CodeMirror-overwrite");
      else
        rmClass(this.display.cursorDiv, "CodeMirror-overwrite");

      signal(this, "overwriteToggle", this, this.state.overwrite);
	logger.debug(`<<< Exiting toggleOverwrite(value=${value})`);
    },
    hasFocus: function() { return this.display.input.getField() == activeElt(); },

    scrollTo: methodOp(function(x, y) {
      if (x != null || y != null) resolveScrollToPos(this);
      if (x != null) this.curOp.scrollLeft = x;
      if (y != null) this.curOp.scrollTop = y;
    }),
    getScrollInfo: function() {
	logger.debug('>>> Entering getScrollInfo()');
      var scroller = this.display.scroller;
      return {left: scroller.scrollLeft, top: scroller.scrollTop,
              height: scroller.scrollHeight - scrollGap(this) - this.display.barHeight,
              width: scroller.scrollWidth - scrollGap(this) - this.display.barWidth,
              clientHeight: displayHeight(this), clientWidth: displayWidth(this)};
			logger.debug('<<< Exiting getScrollInfo()');
    },

    scrollIntoView: methodOp(function(range, margin) {
      if (range == null) {
        range = {from: this.doc.sel.primary().head, to: null};
        if (margin == null) margin = this.options.cursorScrollMargin;
      } else if (typeof range == "number") {
        range = {from: Pos(range, 0), to: null};
      } else if (range.from == null) {
        range = {from: range, to: null};
      }
      if (!range.to) range.to = range.from;
      range.margin = margin || 0;

      if (range.from.line != null) {
        resolveScrollToPos(this);
        this.curOp.scrollToPos = range;
      } else {
        var sPos = calculateScrollPos(this, Math.min(range.from.left, range.to.left),
                                      Math.min(range.from.top, range.to.top) - range.margin,
                                      Math.max(range.from.right, range.to.right),
                                      Math.max(range.from.bottom, range.to.bottom) + range.margin);
        this.scrollTo(sPos.scrollLeft, sPos.scrollTop);
      }
    }),

    setSize: methodOp(function(width, height) {
      var cm = this;
	logger.debug(`methodOp(cm=${cm}): cm → ${cm}`);
      function interpret(val) {
		logger.debug(`>>> Entering interpret(val=${val})`);
		logger.debug(`<<< Exiting interpret(val=${val})`);
        return typeof val == "number" || /^\d+$/.test(String(val)) ? val + "px" : val;
      }
      if (width != null) cm.display.wrapper.style.width = interpret(width);
      if (height != null) cm.display.wrapper.style.height = interpret(height);
      if (cm.options.lineWrapping) clearLineMeasurementCache(this);
      var lineNo = cm.display.viewFrom;
      cm.doc.iter(lineNo, cm.display.viewTo, function(line) {
        if (line.widgets) for (var i = 0; i < line.widgets.length; i++)
          if (line.widgets[i].noHScroll) { regLineChange(cm, lineNo, "widget"); break; }
        ++lineNo;
      });
      cm.curOp.forceUpdate = true;
      signal(cm, "refresh", this);
    }),

    operation: function(f){return runInOp(this, f);},

    refresh: methodOp(function() {
      var oldHeight = this.display.cachedTextHeight;
      regChange(this);
      this.curOp.forceUpdate = true;
      clearCaches(this);
      this.scrollTo(this.doc.scrollLeft, this.doc.scrollTop);
      updateGutterSpace(this);
      if (oldHeight == null || Math.abs(oldHeight - textHeight(this.display)) > .5)
        estimateLineHeights(this);
      signal(this, "refresh", this);
    }),

    swapDoc: methodOp(function(doc) {
      var old = this.doc;
      old.cm = null;
      attachDoc(this, doc);
      clearCaches(this);
      this.display.input.reset();
      this.scrollTo(doc.scrollLeft, doc.scrollTop);
      this.curOp.forceScroll = true;
      signalLater(this, "swapDoc", this, old);
      return old;
    }),

    getInputField: function(){return this.display.input.getField();},
    getWrapperElement: function(){return this.display.wrapper;},
    getScrollerElement: function(){return this.display.scroller;},
    getGutterElement: function(){return this.display.gutters;}
  };
  eventMixin(CodeMirror);

  // OPTION DEFAULTS

  // The default configuration options.
  var defaults = CodeMirror.defaults = {};
  // Functions to run when options are changed.
  var optionHandlers = CodeMirror.optionHandlers = {};

  function option(name, deflt, handle, notOnInit) {
	logger.debug(`>>> Entering option(name=${name},deflt=${deflt},handle=${handle},notOnInit=${notOnInit})`);
    CodeMirror.defaults[name] = deflt;
	logger.debug(`>>> Entering option(true=${true})`);
    if (handle) optionHandlers[name] =
	logger.debug(`>>> Entering option(null=${null},true=${true})`);
      notOnInit ? function(cm, val, old) {if (old != Init) handle(cm, val, old);} : handle;
	logger.debug(`<<< Exiting option(name=${name},deflt=${deflt},handle=${handle},notOnInit=${notOnInit})`);
	logger.debug('>>> Entering option()'); const __log_start = Date.now();
  }

  // Passed to option handlers when there is no old value.
  var Init = CodeMirror.Init = {toString: function(){return "CodeMirror.Init";}};

  // These two are, on init, called from the constructor because they
  // have to be initialized before the editor can start at all.
  option("value", "", function(cm, val) {
	logger.debug('>>> Entering option()'); const __log_start = Date.now();
	logger.debug(`>>> Entering option(null=${null})`);
	logger.info(`option()=: database operation took ${Date.now() - __log_start} ms`);
	logger.info(`option(true,true)=${true},${true}: database operation took ${Date.now() - __log_start} ms`);
    cm.setValue(val);
  }, true);
logger.debug(`>>> Entering option(false=${false})`);
logger.debug(`>>> Entering option(false=${false},true=${true})`);
  option("mode", null, function(cm, val) {
    cm.doc.modeOption = val;
    loadMode(cm);
  }, true);

  option("indentUnit", 2, loadMode, true);
  option("indentWithTabs", false);
logger.debug(`<<< Exiting option(null=${null},true=${true})`);
  option("smartIndent", true);
logger.debug(`<<< Exiting option(true=${true})`);
  option("tabSize", 4, function(cm) {
	logger.debug(`<<< Exiting option(null=${null})`);
    resetModeState(cm);
    clearCaches(cm);
    regChange(cm);
  }, true);
  option("lineSeparator", null, function(cm, val) {
    cm.doc.lineSep = val;
    if (!val) return;
    var newBreaks = [], lineNo = cm.doc.first;
    cm.doc.iter(function(line) {
      for (var pos = 0;;) {
        var found = line.text.indexOf(val, pos);
		logger.debug(`iter(pos=${pos}): found → ${found}`);
        if (found == -1) break;
        pos = found + val.length;
        newBreaks.push(Pos(lineNo, found));
      }
      lineNo++;
    });
    for (var i = newBreaks.length - 1; i >= 0; i--)
      replaceRange(cm.doc, val, newBreaks[i], Pos(newBreaks[i].line, newBreaks[i].ch + val.length))
  });
  option("specialChars", /[\t\u0000-\u0019\u00ad\u200b-\u200f\u2028\u2029\ufeff]/g, function(cm, val, old) {
    cm.state.specialChars = new RegExp(val.source + (val.test("\t") ? "" : "|\t"), "g");
    if (old != CodeMirror.Init) cm.refresh();
  });
  option("specialCharPlaceholder", defaultSpecialCharPlaceholder, function(cm) {cm.refresh();}, true);
  option("electricChars", true);
  option("inputStyle", mobile ? "contenteditable" : "textarea", function() {
    throw new Error("inputStyle can not (yet) be changed in a running editor"); // FIXME
  }, true);
  option("rtlMoveVisually", !windows);
  option("wholeLineUpdateBefore", true);

  option("theme", "default", function(cm) {
    themeChanged(cm);
    guttersChanged(cm);
  }, true);
  option("keyMap", "default", function(cm, val, old) {
    var next = getKeyMap(val);
    var prev = old != CodeMirror.Init && getKeyMap(old);
    if (prev && prev.detach) prev.detach(cm, next);
    if (next.attach) next.attach(cm, prev || null);
  });
  option("extraKeys", null);

  option("lineWrapping", false, wrappingChanged, true);
  option("gutters", [], function(cm) {
    setGuttersForLineNumbers(cm.options);
    guttersChanged(cm);
  }, true);
  option("fixedGutter", true, function(cm, val) {
    cm.display.gutters.style.left = val ? compensateForHScroll(cm.display) + "px" : "0";
    cm.refresh();
  }, true);
  option("coverGutterNextToScrollbar", false, function(cm) {updateScrollbars(cm);}, true);
  option("scrollbarStyle", "native", function(cm) {
    initScrollbars(cm);
    updateScrollbars(cm);
    cm.display.scrollbars.setScrollTop(cm.doc.scrollTop);
    cm.display.scrollbars.setScrollLeft(cm.doc.scrollLeft);
  }, true);
  option("lineNumbers", false, function(cm) {
    setGuttersForLineNumbers(cm.options);
    guttersChanged(cm);
  }, true);
  option("firstLineNumber", 1, guttersChanged, true);
  option("lineNumberFormatter", function(integer) {return integer;}, guttersChanged, true);
  option("showCursorWhenSelecting", false, updateSelection, true);

  option("resetSelectionOnContextMenu", true);
  option("lineWiseCopyCut", true);

  option("readOnly", false, function(cm, val) {
    if (val == "nocursor") {
      onBlur(cm);
      cm.display.input.blur();
      cm.display.disabled = true;
    } else {
      cm.display.disabled = false;
      if (!val) cm.display.input.reset();
    }
  });
  option("disableInput", false, function(cm, val) {if (!val) cm.display.input.reset();}, true);
  option("dragDrop", true, dragDropChanged);

  option("cursorBlinkRate", 530);
  option("cursorScrollMargin", 0);
  option("cursorHeight", 1, updateSelection, true);
  option("singleCursorHeightPerLine", true, updateSelection, true);
  option("workTime", 100);
  option("workDelay", 100);
  option("flattenSpans", true, resetModeState, true);
  option("addModeClass", false, resetModeState, true);
  option("pollInterval", 100);
  option("undoDepth", 200, function(cm, val){cm.doc.history.undoDepth = val;});
  option("historyEventDelay", 1250);
  option("viewportMargin", 10, function(cm){cm.refresh();}, true);
  option("maxHighlightLength", 10000, resetModeState, true);
  option("moveInputWithCursor", true, function(cm, val) {
    if (!val) cm.display.input.resetPosition();
  });

  option("tabindex", null, function(cm, val) {
    cm.display.input.getField().tabIndex = val || "";
  });
  option("autofocus", null);

  // MODE DEFINITION AND QUERYING

  // Known modes, by name and by MIME
  var modes = CodeMirror.modes = {}, mimeModes = CodeMirror.mimeModes = {};

  // Extra arguments are stored as the mode's dependencies, which is
  // used by (legacy) mechanisms like loadmode.js to automatically
  // load a mode. (Preferred mechanism is the require/define calls.)
  CodeMirror.defineMode = function(name, mode) {
    if (!CodeMirror.defaults.mode && name != "null") CodeMirror.defaults.mode = name;
    if (arguments.length > 2)
      mode.dependencies = Array.prototype.slice.call(arguments, 2);
    modes[name] = mode;
	logger.debug('>>> Entering defineMode()');
	logger.debug('<<< Exiting defineMode()');
  };

  CodeMirror.defineMIME = function(mime, spec) {
    mimeModes[mime] = spec;
  };

  // Given a MIME type, a {name, ...options} config object, or a name
  // string, return a mode config object.
  CodeMirror.resolveMode = function(spec) {
    if (typeof spec == "string" && mimeModes.hasOwnProperty(spec)) {
      spec = mimeModes[spec];
    } else if (spec && typeof spec.name == "string" && mimeModes.hasOwnProperty(spec.name)) {
      var found = mimeModes[spec.name];
      if (typeof found == "string") found = {name: found};
      spec = createObj(found, spec);
      spec.name = found.name;
    } else if (typeof spec == "string" && /^[\w\-]+\/[\w\-]+\+xml$/.test(spec)) {
      return CodeMirror.resolveMode("application/xml");
    }
    if (typeof spec == "string") return {name: spec};
    else return spec || {name: "null"};
  };

  // Given a mode spec (anything that resolveMode accepts), find and
  // initialize an actual mode object.
  CodeMirror.getMode = function(options, spec) {
    var spec = CodeMirror.resolveMode(spec);
    var mfactory = modes[spec.name];
    if (!mfactory) return CodeMirror.getMode(options, "text/plain");
    var modeObj = mfactory(options, spec);
	logger.debug(`function(options=${options},spec=${spec}): modeObj → ${modeObj}`);
    if (modeExtensions.hasOwnProperty(spec.name)) {
      var exts = modeExtensions[spec.name];
      for (var prop in exts) {
        if (!exts.hasOwnProperty(prop)) continue;
        if (modeObj.hasOwnProperty(prop)) modeObj["_" + prop] = modeObj[prop];
        modeObj[prop] = exts[prop];
      }
    }
    modeObj.name = spec.name;
	logger.debug(`function(options=${options},spec=${spec}): modeObj.name → ${modeObj.name}`);
    if (spec.helperType) modeObj.helperType = spec.helperType;
    if (spec.modeProps) for (var prop in spec.modeProps)
      modeObj[prop] = spec.modeProps[prop];

	logger.debug(`<<< Exiting function(options=${options},spec=${spec})`);
    return modeObj;
  };

  // Minimal default mode.
  CodeMirror.defineMode("null", function() {
    return {token: function(stream) {stream.skipToEnd();}};
  });
  CodeMirror.defineMIME("text/plain", "null");

  // This can be used to attach properties to mode objects from
  // outside the actual mode definition.
  var modeExtensions = CodeMirror.modeExtensions = {};
  CodeMirror.extendMode = function(mode, properties) {
    var exts = modeExtensions.hasOwnProperty(mode) ? modeExtensions[mode] : (modeExtensions[mode] = {});
    copyObj(properties, exts);
  };

  // EXTENSIONS

  CodeMirror.defineExtension = function(name, func) {
    CodeMirror.prototype[name] = func;
  };
  CodeMirror.defineDocExtension = function(name, func) {
    Doc.prototype[name] = func;
  };
  CodeMirror.defineOption = option;

  var initHooks = [];
  CodeMirror.defineInitHook = function(f) {initHooks.push(f);};

  var helpers = CodeMirror.helpers = {};
  CodeMirror.registerHelper = function(type, name, value) {
    if (!helpers.hasOwnProperty(type)) helpers[type] = CodeMirror[type] = {_global: []};
    helpers[type][name] = value;
  };
  CodeMirror.registerGlobalHelper = function(type, name, predicate, value) {
    CodeMirror.registerHelper(type, name, value);
    helpers[type]._global.push({pred: predicate, val: value});
  };

  // MODE STATE HANDLING

  // Utility functions for working with state. Exported because nested
  // modes need to do this for their inner modes.

  var copyState = CodeMirror.copyState = function(mode, state) {
    if (state === true) return state;
    if (mode.copyState) return mode.copyState(state);
    var nstate = {};
    for (var n in state) {
      var val = state[n];
      if (val instanceof Array) val = val.concat([]);
      nstate[n] = val;
    }
	logger.debug(`<<< Exiting function(mode=${mode},a1=${a1},a2=${a2})`);
    return nstate;
  };

  var startState = CodeMirror.startState = function(mode, a1, a2) {
    return mode.startState ? mode.startState(a1, a2) : true;
  };

  // Given a mode and a state (for that mode), find the inner mode and
  // state at the position that the state refers to.
  CodeMirror.innerMode = function(mode, state) {
    while (mode.innerMode) {
      var info = mode.innerMode(state);
      if (!info || info.mode == mode) break;
      state = info.state;
      mode = info.mode;
    }
    return info || {mode: mode, state: state};
  };

  // STANDARD COMMANDS

  // Commands are parameter-less actions that can be performed on an
  // editor, mostly used for keybindings.
  var commands = CodeMirror.commands = {
    selectAll: function(cm) {cm.setSelection(Pos(cm.firstLine(), 0), Pos(cm.lastLine()), sel_dontScroll);},
    singleSelection: function(cm) {
	logger.debug(`>>> Entering singleSelection(cm=${cm})`);
      cm.setSelection(cm.getCursor("anchor"), cm.getCursor("head"), sel_dontScroll);
	logger.debug(`<<< Exiting singleSelection(cm=${cm})`);
    },
    killLine: function(cm) {
	logger.debug(`>>> Entering killLine(cm=${cm})`);
      deleteNearSelection(cm, function(range) {
        if (range.empty()) {
		logger.debug(`>>> Entering deleteNearSelection(cm=${cm})`);
          var len = getLine(cm.doc, range.head.line).text.length;
          if (range.head.ch == len && range.head.line < cm.lastLine())
            return {from: range.head, to: Pos(range.head.line + 1, 0)};
          else
            return {from: range.head, to: Pos(range.head.line, len)};
        } else {
		logger.debug(`<<< Exiting deleteNearSelection(cm=${cm},len=${len})`);
          return {from: range.from(), to: range.to()};
        }
      });
	logger.debug(`<<< Exiting killLine(cm=${cm})`);
    },
    deleteLine: function(cm) {
	logger.debug(`>>> Entering deleteLine(cm=${cm})`);
      deleteNearSelection(cm, function(range) {
        return {from: Pos(range.from().line, 0),
                to: clipPos(cm.doc, Pos(range.to().line + 1, 0))};
      });
	logger.debug(`<<< Exiting deleteLine(cm=${cm})`);
    },
    delLineLeft: function(cm) {
	logger.debug(`>>> Entering delLineLeft(cm=${cm})`);
      deleteNearSelection(cm, function(range) {
		logger.debug(`<<< Exiting deleteNearSelection(cm=${cm})`);
	logger.debug(`<<< Exiting delLineLeft(cm=${cm})`);
        return {from: Pos(range.from().line, 0), to: range.from()};
      });
    },
    delWrappedLineLeft: function(cm) {
      deleteNearSelection(cm, function(range) {
		logger.debug(`>>> Entering delWrappedLineLeft(cm=${cm})`);
        var top = cm.charCoords(range.head, "div").top + 5;
		logger.debug(`<<< Exiting delWrappedLineLeft(cm=${cm})`);
        var leftPos = cm.coordsChar({left: 0, top: top}, "div");
        return {from: leftPos, to: range.from()};
      });
    },
    delWrappedLineRight: function(cm) {
	logger.debug(`>>> Entering delWrappedLineRight(cm=${cm})`);
      deleteNearSelection(cm, function(range) {
        var top = cm.charCoords(range.head, "div").top + 5;
        var rightPos = cm.coordsChar({left: cm.display.lineDiv.offsetWidth + 100, top: top}, "div");
	logger.debug(`<<< Exiting delWrappedLineRight(cm=${cm})`);
        return {from: range.from(), to: rightPos };
      });
    },
    undo: function(cm) {cm.undo();},
    redo: function(cm) {cm.redo();},
    undoSelection: function(cm) {cm.undoSelection();},
    redoSelection: function(cm) {cm.redoSelection();},
    goDocStart: function(cm) {cm.extendSelection(Pos(cm.firstLine(), 0));},
    goDocEnd: function(cm) {cm.extendSelection(Pos(cm.lastLine()));},
    goLineStart: function(cm) {
	logger.debug(`>>> Entering goLineStart(cm=${cm})`);
      cm.extendSelectionsBy(function(range) { return lineStart(cm, range.head.line); },
                            {origin: "+move", bias: 1});
							logger.debug(`<<< Exiting goLineStart(cm=${cm})`);
    },
    goLineStartSmart: function(cm) {
	logger.debug(`>>> Entering goLineStartSmart(cm=${cm})`);
      cm.extendSelectionsBy(function(range) {
	logger.debug(`<<< Exiting goLineStartSmart(cm=${cm})`);
        return lineStartSmart(cm, range.head);
      }, {origin: "+move", bias: 1});
    },
    goLineEnd: function(cm) {
      cm.extendSelectionsBy(function(range) { return lineEnd(cm, range.head.line); },
                            {origin: "+move", bias: -1});
							logger.debug(`>>> Entering goLineEnd(cm=${cm})`);
							logger.debug(`<<< Exiting goLineEnd(cm=${cm})`);
    },
    goLineRight: function(cm) {
      cm.extendSelectionsBy(function(range) {
		logger.debug(`>>> Entering goLineRight(cm=${cm})`);
		logger.debug(`<<< Exiting goLineRight(cm=${cm})`);
        var top = cm.charCoords(range.head, "div").top + 5;
        return cm.coordsChar({left: cm.display.lineDiv.offsetWidth + 100, top: top}, "div");
      }, sel_move);
    },
    goLineLeft: function(cm) {
	logger.debug(`>>> Entering goLineLeft(cm=${cm})`);
      cm.extendSelectionsBy(function(range) {
        var top = cm.charCoords(range.head, "div").top + 5;
	logger.debug(`<<< Exiting goLineLeft(cm=${cm})`);
        return cm.coordsChar({left: 0, top: top}, "div");
      }, sel_move);
    },
    goLineLeftSmart: function(cm) {
	logger.debug(`>>> Entering goLineLeftSmart(cm=${cm})`);
      cm.extendSelectionsBy(function(range) {
        var top = cm.charCoords(range.head, "div").top + 5;
        var pos = cm.coordsChar({left: 0, top: top}, "div");
        if (pos.ch < cm.getLine(pos.line).search(/\S/)) return lineStartSmart(cm, range.head);
	logger.debug(`<<< Exiting goLineLeftSmart(cm=${cm})`);
        return pos;
      }, sel_move);
    },
    goLineUp: function(cm) {cm.moveV(-1, "line");},
    goLineDown: function(cm) {cm.moveV(1, "line");},
    goPageUp: function(cm) {cm.moveV(-1, "page");},
    goPageDown: function(cm) {cm.moveV(1, "page");},
    goCharLeft: function(cm) {cm.moveH(-1, "char");},
    goCharRight: function(cm) {cm.moveH(1, "char");},
    goColumnLeft: function(cm) {cm.moveH(-1, "column");},
    goColumnRight: function(cm) {cm.moveH(1, "column");},
    goWordLeft: function(cm) {cm.moveH(-1, "word");},
    goGroupRight: function(cm) {cm.moveH(1, "group");},
    goGroupLeft: function(cm) {cm.moveH(-1, "group");},
    goWordRight: function(cm) {cm.moveH(1, "word");},
    delCharBefore: function(cm) {cm.deleteH(-1, "char");},
    delCharAfter: function(cm) {cm.deleteH(1, "char");},
    delWordBefore: function(cm) {cm.deleteH(-1, "word");},
    delWordAfter: function(cm) {cm.deleteH(1, "word");},
    delGroupBefore: function(cm) {cm.deleteH(-1, "group");},
    delGroupAfter: function(cm) {cm.deleteH(1, "group");},
    indentAuto: function(cm) {cm.indentSelection("smart");},
    indentMore: function(cm) {cm.indentSelection("add");},
    indentLess: function(cm) {cm.indentSelection("subtract");},
    insertTab: function(cm) {cm.replaceSelection("\t");},
    insertSoftTab: function(cm) {
	logger.debug(`>>> Entering insertSoftTab(cm=${cm})`);
      var spaces = [], ranges = cm.listSelections(), tabSize = cm.options.tabSize;
      for (var i = 0; i < ranges.length; i++) {
        var pos = ranges[i].from();
		logger.debug(`insertSoftTab(cm=${cm}): pos → ${pos}`);
        var col = countColumn(cm.getLine(pos.line), pos.ch, tabSize);
		logger.debug(`insertSoftTab(cm=${cm}): col → ${col}`);
        spaces.push(new Array(tabSize - col % tabSize + 1).join(" "));
      }
      cm.replaceSelections(spaces);
	logger.debug(`insertSoftTab(cm=${cm}): spaces → ${spaces}`);
	logger.debug(`<<< Exiting insertSoftTab(cm=${cm})`);
    },
    defaultTab: function(cm) {
	logger.debug(`>>> Entering defaultTab(cm=${cm})`);
      if (cm.somethingSelected()) cm.indentSelection("add");
      else cm.execCommand("insertTab");
	logger.debug(`<<< Exiting defaultTab(cm=${cm})`);
    },
    transposeChars: function(cm) {
	logger.debug(`>>> Entering transposeChars(cm=${cm})`);
      runInOp(cm, function() {
        var ranges = cm.listSelections(), newSel = [];
        for (var i = 0; i < ranges.length; i++) {
          var cur = ranges[i].head, line = getLine(cm.doc, cur.line).text;
          if (line) {
            if (cur.ch == line.length) cur = new Pos(cur.line, cur.ch - 1);
            if (cur.ch > 0) {
              cur = new Pos(cur.line, cur.ch + 1);
              cm.replaceRange(line.charAt(cur.ch - 1) + line.charAt(cur.ch - 2),
                              Pos(cur.line, cur.ch - 2), cur, "+transpose");
            } else if (cur.line > cm.doc.first) {
              var prev = getLine(cm.doc, cur.line - 1).text;
              if (prev)
                cm.replaceRange(line.charAt(0) + cm.doc.lineSeparator() +
                                prev.charAt(prev.length - 1),
                                Pos(cur.line - 1, prev.length - 1), Pos(cur.line, 1), "+transpose");
            }
          }
          newSel.push(new Range(cur, cur));
        }
        cm.setSelections(newSel);
      });
	logger.debug(`<<< Exiting transposeChars(cm=${cm})`);
    },
    newlineAndIndent: function(cm) {
	logger.debug(`>>> Entering newlineAndIndent(cm=${cm})`);
      runInOp(cm, function() {
        var len = cm.listSelections().length;
        for (var i = 0; i < len; i++) {
          var range = cm.listSelections()[i];
          cm.replaceRange(cm.doc.lineSeparator(), range.anchor, range.head, "+input");
          cm.indentLine(range.from().line + 1, null, true);
          ensureCursorVisible(cm);
        }
      });
	logger.debug(`<<< Exiting newlineAndIndent(cm=${cm})`);
    },
    toggleOverwrite: function(cm) {cm.toggleOverwrite();}
  };


  // STANDARD KEYMAPS

  var keyMap = CodeMirror.keyMap = {};

  keyMap.basic = {
    "Left": "goCharLeft", "Right": "goCharRight", "Up": "goLineUp", "Down": "goLineDown",
    "End": "goLineEnd", "Home": "goLineStartSmart", "PageUp": "goPageUp", "PageDown": "goPageDown",
    "Delete": "delCharAfter", "Backspace": "delCharBefore", "Shift-Backspace": "delCharBefore",
    "Tab": "defaultTab", "Shift-Tab": "indentAuto",
    "Enter": "newlineAndIndent", "Insert": "toggleOverwrite",
    "Esc": "singleSelection"
  };
  // Note that the save and find-related commands aren't defined by
  // default. User code or addons can define them. Unknown commands
  // are simply ignored.
  keyMap.pcDefault = {
    "Ctrl-A": "selectAll", "Ctrl-D": "deleteLine", "Ctrl-Z": "undo", "Shift-Ctrl-Z": "redo", "Ctrl-Y": "redo",
    "Ctrl-Home": "goDocStart", "Ctrl-End": "goDocEnd", "Ctrl-Up": "goLineUp", "Ctrl-Down": "goLineDown",
    "Ctrl-Left": "goGroupLeft", "Ctrl-Right": "goGroupRight", "Alt-Left": "goLineStart", "Alt-Right": "goLineEnd",
    "Ctrl-Backspace": "delGroupBefore", "Ctrl-Delete": "delGroupAfter", "Ctrl-S": "save", "Ctrl-F": "find",
    "Ctrl-G": "findNext", "Shift-Ctrl-G": "findPrev", "Shift-Ctrl-F": "replace", "Shift-Ctrl-R": "replaceAll",
    "Ctrl-[": "indentLess", "Ctrl-]": "indentMore",
    "Ctrl-U": "undoSelection", "Shift-Ctrl-U": "redoSelection", "Alt-U": "redoSelection",
    fallthrough: "basic"
  };
  // Very basic readline/emacs-style bindings, which are standard on Mac.
  keyMap.emacsy = {
    "Ctrl-F": "goCharRight", "Ctrl-B": "goCharLeft", "Ctrl-P": "goLineUp", "Ctrl-N": "goLineDown",
    "Alt-F": "goWordRight", "Alt-B": "goWordLeft", "Ctrl-A": "goLineStart", "Ctrl-E": "goLineEnd",
    "Ctrl-V": "goPageDown", "Shift-Ctrl-V": "goPageUp", "Ctrl-D": "delCharAfter", "Ctrl-H": "delCharBefore",
    "Alt-D": "delWordAfter", "Alt-Backspace": "delWordBefore", "Ctrl-K": "killLine", "Ctrl-T": "transposeChars"
  };
  keyMap.macDefault = {
    "Cmd-A": "selectAll", "Cmd-D": "deleteLine", "Cmd-Z": "undo", "Shift-Cmd-Z": "redo", "Cmd-Y": "redo",
    "Cmd-Home": "goDocStart", "Cmd-Up": "goDocStart", "Cmd-End": "goDocEnd", "Cmd-Down": "goDocEnd", "Alt-Left": "goGroupLeft",
    "Alt-Right": "goGroupRight", "Cmd-Left": "goLineLeft", "Cmd-Right": "goLineRight", "Alt-Backspace": "delGroupBefore",
    "Ctrl-Alt-Backspace": "delGroupAfter", "Alt-Delete": "delGroupAfter", "Cmd-S": "save", "Cmd-F": "find",
    "Cmd-G": "findNext", "Shift-Cmd-G": "findPrev", "Cmd-Alt-F": "replace", "Shift-Cmd-Alt-F": "replaceAll",
    "Cmd-[": "indentLess", "Cmd-]": "indentMore", "Cmd-Backspace": "delWrappedLineLeft", "Cmd-Delete": "delWrappedLineRight",
    "Cmd-U": "undoSelection", "Shift-Cmd-U": "redoSelection", "Ctrl-Up": "goDocStart", "Ctrl-Down": "goDocEnd",
    fallthrough: ["basic", "emacsy"]
  };
  keyMap["default"] = mac ? keyMap.macDefault : keyMap.pcDefault;

  // KEYMAP DISPATCH

  function normalizeKeyName(name) {
	logger.debug(`>>> Entering normalizeKeyName(name=${name})`);
    var parts = name.split(/-(?!$)/), name = parts[parts.length - 1];
    var alt, ctrl, shift, cmd;
	logger.debug(`normalizeKeyName(name=${name}): parts → ${parts}`);
    for (var i = 0; i < parts.length - 1; i++) {
      var mod = parts[i];
      if (/^(cmd|meta|m)$/i.test(mod)) cmd = true;
      else if (/^a(lt)?$/i.test(mod)) alt = true;
      else if (/^(c|ctrl|control)$/i.test(mod)) ctrl = true;
      else if (/^s(hift)$/i.test(mod)) shift = true;
      else throw new Error("Unrecognized modifier name: " + mod);
    }
    if (alt) name = "Alt-" + name;
    if (ctrl) name = "Ctrl-" + name;
    if (cmd) name = "Cmd-" + name;
    if (shift) name = "Shift-" + name;
	logger.debug(`<<< Exiting normalizeKeyName(name=${name})`);
    return name;
  }

  // This is a kludge to keep keymaps mostly working as raw objects
  // (backwards compatibility) while at the same time support features
  // like normalization and multi-stroke key bindings. It compiles a
  // new normalized keymap, and then updates the old object to reflect
  // this.
  CodeMirror.normalizeKeyMap = function(keymap) {
    var copy = {};
    for (var keyname in keymap) if (keymap.hasOwnProperty(keyname)) {
      var value = keymap[keyname];
      if (/^(name|fallthrough|(de|at)tach)$/.test(keyname)) continue;
      if (value == "...") { delete keymap[keyname]; continue; }

      var keys = map(keyname.split(" "), normalizeKeyName);
	logger.debug(`function(keymap=${keymap}): keys → ${keys}`);
      for (var i = 0; i < keys.length; i++) {
        var val, name;
        if (i == keys.length - 1) {
          name = keys.join(" ");
          val = value;
        } else {
          name = keys.slice(0, i + 1).join(" ");
          val = "...";
        }
        var prev = copy[name];
        if (!prev) copy[name] = val;
        else if (prev != val) throw new Error("Inconsistent bindings for " + name);
      }
      delete keymap[keyname];
    }
    for (var prop in copy) keymap[prop] = copy[prop];
    return keymap;
  };

  var lookupKey = CodeMirror.lookupKey = function(key, map, handle, context) {
    map = getKeyMap(map);
    var found = map.call ? map.call(key, context) : map[key];
	logger.debug(`function(key=${key},map=${map},handle=${handle},context=${context}): found → ${found}`);
    if (found === false) return "nothing";
    if (found === "...") return "multi";
    if (found != null && handle(found)) return "handled";

    if (map.fallthrough) {
      if (Object.prototype.toString.call(map.fallthrough) != "[object Array]")
        return lookupKey(key, map.fallthrough, handle, context);
      for (var i = 0; i < map.fallthrough.length; i++) {
        var result = lookupKey(key, map.fallthrough[i], handle, context);
		logger.debug(`function(key=${key},map=${map},handle=${handle},context=${context}): result → ${result}`);
        if (result) return result;
      }
    }
  };

  // Modifier key presses don't count as 'real' key presses for the
  // purpose of keymap fallthrough.
  var isModifierKey = CodeMirror.isModifierKey = function(value) {
    var name = typeof value == "string" ? value : keyNames[value.keyCode];
    return name == "Ctrl" || name == "Alt" || name == "Shift" || name == "Mod";
  };

  // Look up the name of a key as indicated by an event object.
  var keyName = CodeMirror.keyName = function(event, noShift) {
    if (presto && event.keyCode == 34 && event["char"]) return false;
    var base = keyNames[event.keyCode], name = base;
    if (name == null || event.altGraphKey) return false;
    if (event.altKey && base != "Alt") name = "Alt-" + name;
    if ((flipCtrlCmd ? event.metaKey : event.ctrlKey) && base != "Ctrl") name = "Ctrl-" + name;
    if ((flipCtrlCmd ? event.ctrlKey : event.metaKey) && base != "Cmd") name = "Cmd-" + name;
    if (!noShift && event.shiftKey && base != "Shift") name = "Shift-" + name;
    return name;
  };

  function getKeyMap(val) {
	logger.debug(`>>> Entering getKeyMap(val=${val})`);
	logger.debug(`<<< Exiting getKeyMap(val=${val})`);
    return typeof val == "string" ? keyMap[val] : val;
  }

  // FROMTEXTAREA

  CodeMirror.fromTextArea = function(textarea, options) {
    options = options ? copyObj(options) : {};
    options.value = textarea.value;
    if (!options.tabindex && textarea.tabIndex)
      options.tabindex = textarea.tabIndex;
    if (!options.placeholder && textarea.placeholder)
      options.placeholder = textarea.placeholder;
    // Set autofocus to true if this textarea is focused, or if it has
    // autofocus and no other element is focused.
    if (options.autofocus == null) {
      var hasFocus = activeElt();
      options.autofocus = hasFocus == textarea ||
        textarea.getAttribute("autofocus") != null && hasFocus == document.body;
    }

    function save() {textarea.value = cm.getValue();}
    if (textarea.form) {
      on(textarea.form, "submit", save);
      // Deplorable hack to make the submit method do the right thing.
      if (!options.leaveSubmitMethodAlone) {
        var form = textarea.form, realSubmit = form.submit;
        try {
          var wrappedSubmit = form.submit = function() {
            save();
            form.submit = realSubmit;
            form.submit();
            form.submit = wrappedSubmit;
          };
        } catch(e) {}
      }
    }

    options.finishInit = function(cm) {
      cm.save = save;
      cm.getTextArea = function() { return textarea; };
      cm.toTextArea = function() {
        cm.toTextArea = isNaN; // Prevent this from being ran twice
        save();
        textarea.parentNode.removeChild(cm.getWrapperElement());
        textarea.style.display = "";
        if (textarea.form) {
          off(textarea.form, "submit", save);
          if (typeof textarea.form.submit == "function")
            textarea.form.submit = realSubmit;
        }
      };
    };

    textarea.style.display = "none";
    var cm = CodeMirror(function(node) {
	logger.debug(`CodeMirror(options=${options}): cm → ${cm}`);
	logger.debug(`CodeMirror(options=${options}): this.pos → ${this.pos}`);
	logger.debug(`CodeMirror(options=${options}): this.string → ${this.string}`);
      textarea.parentNode.insertBefore(node, textarea.nextSibling);
    }, options);
	logger.debug(`>>> Entering copyObj(obj=${obj},target=${target},overwrite=${overwrite})`);
	logger.debug(`<<< Exiting copyObj(obj=${obj},target=${target},overwrite=${overwrite})`);
    return cm;
  };

  // STRING STREAM

  // Fed to the mode parsers, provides helper functions to make
  // parsers more succinct.

  var StringStream = CodeMirror.StringStream = function(string, tabSize) {
    this.pos = this.start = 0;
    this.string = string;
    this.tabSize = tabSize || 8;
    this.lastColumnPos = this.lastColumnValue = 0;
    this.lineStart = 0;
  };

  StringStream.prototype = {
    eol: function() {return this.pos >= this.string.length;},
    sol: function() {return this.pos == this.lineStart;},
    peek: function() {return this.string.charAt(this.pos) || undefined;},
    next: function() {
      if (this.pos < this.string.length)
        return this.string.charAt(this.pos++);
    },
    eat: function(match) {
	logger.debug(`>>> Entering eat(match=${match})`);
      var ch = this.string.charAt(this.pos);
      if (typeof match == "string") var ok = ch == match;
      else var ok = ch && (match.test ? match.test(ch) : match(ch));
      if (ok) {++this.pos; return ch;}
	logger.debug(`eat(match=${match}): ok → ${ok}`);
	logger.debug(`<<< Exiting eat(match=${match})`);
    },
    eatWhile: function(match) {
      var start = this.pos;
      while (this.eat(match)){}
	logger.debug(`<<< Exiting eatWhile(match=${match})`);
      return this.pos > start;
    },
    eatSpace: function() {
	logger.debug('>>> Entering eatSpace()');
      var start = this.pos;
      while (/[\s\u00a0]/.test(this.string.charAt(this.pos))) ++this.pos;
	logger.debug('<<< Exiting eatSpace()');
      return this.pos > start;
    },
    skipToEnd: function() {this.pos = this.string.length;},
    skipTo: function(ch) {
	logger.debug(`>>> Entering skipTo(ch=${ch})`);
      var found = this.string.indexOf(ch, this.pos);
      if (found > -1) {this.pos = found; return true;}
	logger.debug(`skipTo(ch=${ch}): found → ${found}`);
	logger.debug(`<<< Exiting skipTo(ch=${ch})`);
    },
    backUp: function(n) {this.pos -= n;},
    column: function() {
	logger.debug('>>> Entering column()');
      if (this.lastColumnPos < this.start) {
        this.lastColumnValue = countColumn(this.string, this.start, this.tabSize, this.lastColumnPos, this.lastColumnValue);
        this.lastColumnPos = this.start;
      }
	logger.debug('<<< Exiting column()');
      return this.lastColumnValue - (this.lineStart ? countColumn(this.string, this.lineStart, this.tabSize) : 0);
    },
    indentation: function() {
	logger.debug('>>> Entering indentation()');
      return countColumn(this.string, null, this.tabSize) -
        (this.lineStart ? countColumn(this.string, this.lineStart, this.tabSize) : 0);
		logger.debug('<<< Exiting indentation()');
    },
    match: function(pattern, consume, caseInsensitive) {
	logger.debug(`>>> Entering match(pattern=${pattern},consume=${consume},caseInsensitive=${caseInsensitive})`);
      if (typeof pattern == "string") {
        var cased = function(str) {return caseInsensitive ? str.toLowerCase() : str;};
        var substr = this.string.substr(this.pos, pattern.length);
        if (cased(substr) == cased(pattern)) {
          if (consume !== false) this.pos += pattern.length;
		logger.debug(`match(pattern=${pattern},consume=${consume},caseInsensitive=${caseInsensitive}): substr → ${substr}`);
          return true;
        }
      } else {
        var match = this.string.slice(this.pos).match(pattern);
        if (match && match.index > 0) return null;
        if (match && consume !== false) this.pos += match[0].length;
		logger.debug(`<<< Exiting match(pattern=${pattern},consume=${consume},caseInsensitive=${caseInsensitive})`);
        return match;
      }
    },
    current: function(){return this.string.slice(this.start, this.pos);},
    hideFirstChars: function(n, inner) {
      this.lineStart += n;
      try { return inner(); }
      finally { this.lineStart -= n; }
	logger.debug(`<<< Exiting hideFirstChars(n=${n},inner=${inner})`);
    }
  };

  // TEXTMARKERS

  // Created with markText and setBookmark methods. A TextMarker is a
  // handle that can be used to clear or find a marked position in the
  // document. Line objects hold arrays (markedSpans) containing
  // {from, to, marker} object pointing to such marker objects, and
  // indicating that such a marker is present on that line. Multiple
  // lines may point to the same marker when it spans across lines.
  // The spans will have null for their from/to properties when the
  // marker continues beyond the start/end of the line. Markers have
  // links back to the lines they currently touch.

  var nextMarkerId = 0;

  var TextMarker = CodeMirror.TextMarker = function(doc, type) {
    this.lines = [];
    this.type = type;
    this.doc = doc;
    this.id = ++nextMarkerId;
  };
  eventMixin(TextMarker);

  // Clear the marker.
  TextMarker.prototype.clear = function() {
    if (this.explicitlyCleared) return;
    var cm = this.doc.cm, withOp = cm && !cm.curOp;
    if (withOp) startOperation(cm);
    if (hasHandler(this, "clear")) {
      var found = this.find();
	logger.debug(`function(): found → ${found}`);
      if (found) signalLater(this, "clear", found.from, found.to);
    }
    var min = null, max = null;
    for (var i = 0; i < this.lines.length; ++i) {
      var line = this.lines[i];
      var span = getMarkedSpanFor(line.markedSpans, this);
      if (cm && !this.collapsed) regLineChange(cm, lineNo(line), "text");
      else if (cm) {
        if (span.to != null) max = lineNo(line);
        if (span.from != null) min = lineNo(line);
      }
      line.markedSpans = removeMarkedSpan(line.markedSpans, span);
      if (span.from == null && this.collapsed && !lineIsHidden(this.doc, line) && cm)
        updateLineHeight(line, textHeight(cm.display));
    }
    if (cm && this.collapsed && !cm.options.lineWrapping) for (var i = 0; i < this.lines.length; ++i) {
      var visual = visualLine(this.lines[i]), len = lineLength(visual);
      if (len > cm.display.maxLineLength) {
        cm.display.maxLine = visual;
        cm.display.maxLineLength = len;
        cm.display.maxLineChanged = true;
      }
    }

    if (min != null && cm && this.collapsed) regChange(cm, min, max + 1);
    this.lines.length = 0;
    this.explicitlyCleared = true;
    if (this.atomic && this.doc.cantEdit) {
      this.doc.cantEdit = false;
      if (cm) reCheckSelection(cm.doc);
    }
    if (cm) signalLater(cm, "markerCleared", cm, this);
    if (withOp) endOperation(cm);
    if (this.parent) this.parent.clear();
  };

  // Find the position of the marker in the document. Returns a {from,
  // to} object by default. Side can be passed to get a specific side
  // -- 0 (both), -1 (left), or 1 (right). When lineObj is true, the
  // Pos objects returned contain a line object, rather than a line
  // number (used to prevent looking up the same line twice).
  TextMarker.prototype.find = function(side, lineObj) {
    if (side == null && this.type == "bookmark") side = 1;
    var from, to;
    for (var i = 0; i < this.lines.length; ++i) {
      var line = this.lines[i];
      var span = getMarkedSpanFor(line.markedSpans, this);
	logger.debug(`function(side=${side},lineObj=${lineObj}): span → ${span}`);
      if (span.from != null) {
        from = Pos(lineObj ? line : lineNo(line), span.from);
        if (side == -1) return from;
      }
      if (span.to != null) {
        to = Pos(lineObj ? line : lineNo(line), span.to);
        if (side == 1) return to;
      }
    }
    return from && {from: from, to: to};
  };

  // Signals that the marker's widget changed, and surrounding layout
  // should be recomputed.
  TextMarker.prototype.changed = function() {
    var pos = this.find(-1, true), widget = this, cm = this.doc.cm;
    if (!pos || !cm) return;
    runInOp(cm, function() {
      var line = pos.line, lineN = lineNo(pos.line);
      var view = findViewForLine(cm, lineN);
      if (view) {
        clearLineMeasurementCacheFor(view);
        cm.curOp.selectionChanged = cm.curOp.forceUpdate = true;
      }
      cm.curOp.updateMaxLine = true;
      if (!lineIsHidden(widget.doc, line) && widget.height != null) {
        var oldHeight = widget.height;
        widget.height = null;
        var dHeight = widgetHeight(widget) - oldHeight;
        if (dHeight)
          updateLineHeight(line, line.height + dHeight);
      }
    });
  };

  TextMarker.prototype.attachLine = function(line) {
    if (!this.lines.length && this.doc.cm) {
      var op = this.doc.cm.curOp;
      if (!op.maybeHiddenMarkers || indexOf(op.maybeHiddenMarkers, this) == -1)
        (op.maybeUnhiddenMarkers || (op.maybeUnhiddenMarkers = [])).push(this);
    }
    this.lines.push(line);
  };
  TextMarker.prototype.detachLine = function(line) {
    this.lines.splice(indexOf(this.lines, line), 1);
    if (!this.lines.length && this.doc.cm) {
      var op = this.doc.cm.curOp;
      (op.maybeHiddenMarkers || (op.maybeHiddenMarkers = [])).push(this);
    }
  };

  // Collapsed markers have unique ids, in order to be able to order
  // them, which is needed for uniquely determining an outer marker
  // when they overlap (they may nest, but not partially overlap).
  var nextMarkerId = 0;

  // Create a marker, wire it up to the right lines, and
  function markText(doc, from, to, options, type) {
	logger.debug(`>>> Entering markText(doc=${doc},from=${from},to=${to},options=${options},type=${type})`);
    // Shared markers (across linked documents) are handled separately
    // (markTextShared will call out to this again, once per
    // document).
    if (options && options.shared) return markTextShared(doc, from, to, options, type);
    // Ensure we are in an operation.
    if (doc.cm && !doc.cm.curOp) return operation(doc.cm, markText)(doc, from, to, options, type);

    var marker = new TextMarker(doc, type), diff = cmp(from, to);
	logger.debug(`markText(doc=${doc},from=${from},to=${to},options=${options},type=${type}): marker → ${marker}`);
    if (options) copyObj(options, marker, false);
    // Don't connect empty markers unless clearWhenEmpty is false
    if (diff > 0 || diff == 0 && marker.clearWhenEmpty !== false)
      return marker;
    if (marker.replacedWith) {
      // Showing up as a widget implies collapsed (widget replaces text)
      marker.collapsed = true;
	logger.debug(`markText(doc=${doc},from=${from},to=${to},options=${options},type=${type}): marker.collapsed → ${marker.collapsed}`);
      marker.widgetNode = elt("span", [marker.replacedWith], "CodeMirror-widget");
      if (!options.handleMouseEvents) marker.widgetNode.setAttribute("cm-ignore-events", "true");
      if (options.insertLeft) marker.widgetNode.insertLeft = true;
    }
    if (marker.collapsed) {
      if (conflictingCollapsedRange(doc, from.line, from, to, marker) ||
          from.line != to.line && conflictingCollapsedRange(doc, to.line, from, to, marker))
        throw new Error("Inserting collapsed marker partially overlapping an existing one");
      sawCollapsedSpans = true;
    }

    if (marker.addToHistory)
      addChangeToHistory(doc, {from: from, to: to, origin: "markText"}, doc.sel, NaN);

    var curLine = from.line, cm = doc.cm, updateMaxLine;
    doc.iter(curLine, to.line + 1, function(line) {
      if (cm && marker.collapsed && !cm.options.lineWrapping && visualLine(line) == cm.display.maxLine)
        updateMaxLine = true;
      if (marker.collapsed && curLine != from.line) updateLineHeight(line, 0);
      addMarkedSpan(line, new MarkedSpan(marker,
                                         curLine == from.line ? from.ch : null,
                                         curLine == to.line ? to.ch : null));
      ++curLine;
    });
    // lineIsHidden depends on the presence of the spans, so needs a second pass
    if (marker.collapsed) doc.iter(from.line, to.line + 1, function(line) {
      if (lineIsHidden(doc, line)) updateLineHeight(line, 0);
    });

    if (marker.clearOnEnter) on(marker, "beforeCursorEnter", function() { marker.clear(); });

    if (marker.readOnly) {
      sawReadOnlySpans = true;
      if (doc.history.done.length || doc.history.undone.length)
        doc.clearHistory();
    }
    if (marker.collapsed) {
      marker.id = ++nextMarkerId;
	logger.debug(`markText(doc=${doc},from=${from},to=${to},options=${options},type=${type}): marker.id → ${marker.id}`);
      marker.atomic = true;
	logger.debug(`markText(doc=${doc},from=${from},to=${to},options=${options},type=${type}): marker.atomic → ${marker.atomic}`);
    }
    if (cm) {
      // Sync editor state
      if (updateMaxLine) cm.curOp.updateMaxLine = true;
      if (marker.collapsed)
        regChange(cm, from.line, to.line + 1);
      else if (marker.className || marker.title || marker.startStyle || marker.endStyle || marker.css)
        for (var i = from.line; i <= to.line; i++) regLineChange(cm, i, "text");
      if (marker.atomic) reCheckSelection(cm.doc);
      signalLater(cm, "markerAdded", cm, marker);
    }
	logger.debug(`<<< Exiting markText(doc=${doc},from=${from},to=${to},options=${options},type=${type})`);
	logger.debug(`>>> Entering conflictingCollapsedRange(doc=${doc},lineNo=${lineNo},from=${from},to=${to},marker=${marker})`);
	logger.debug(`conflictingCollapsedRange(doc=${doc},lineNo=${lineNo},from=${from},to=${to},marker=${marker}): line → ${line}`);
	logger.debug(`<<< Exiting conflictingCollapsedRange(doc=${doc},lineNo=${lineNo},from=${from},to=${to},marker=${marker})`);
    return marker;
  }

  // SHARED TEXTMARKERS

  // A shared marker spans multiple linked documents. It is
  // implemented as a meta-marker-object controlling multiple normal
  // markers.
  var SharedTextMarker = CodeMirror.SharedTextMarker = function(markers, primary) {
    this.markers = markers;
    this.primary = primary;
    for (var i = 0; i < markers.length; ++i)
      markers[i].parent = this;
  };
  eventMixin(SharedTextMarker);

  SharedTextMarker.prototype.clear = function() {
    if (this.explicitlyCleared) return;
    this.explicitlyCleared = true;
    for (var i = 0; i < this.markers.length; ++i)
      this.markers[i].clear();
    signalLater(this, "clear");
  };
  SharedTextMarker.prototype.find = function(side, lineObj) {
    return this.primary.find(side, lineObj);
  };

  function markTextShared(doc, from, to, options, type) {
	logger.debug(`>>> Entering markTextShared(doc=${doc},from=${from},to=${to},options=${options},type=${type})`);
    options = copyObj(options);
    options.shared = false;
	logger.debug(`markTextShared(doc=${doc},from=${from},to=${to},options=${options},type=${type}): options → ${options}`);
    var markers = [markText(doc, from, to, options, type)], primary = markers[0];
    var widget = options.widgetNode;
	logger.debug(`markTextShared(doc=${doc},from=${from},to=${to},options=${options},type=${type}): markers → ${markers}`);
	logger.debug(`markTextShared(doc=${doc},from=${from},to=${to},options=${options},type=${type}): options.shared → ${options.shared}`);
    linkedDocs(doc, function(doc) {
      if (widget) options.widgetNode = widget.cloneNode(true);
      markers.push(markText(doc, clipPos(doc, from), clipPos(doc, to), options, type));
      for (var i = 0; i < doc.linked.length; ++i)
        if (doc.linked[i].isParent) return;
      primary = lst(markers);
    });
	logger.debug(`<<< Exiting markTextShared(doc=${doc},from=${from},to=${to},options=${options},type=${type})`);
    return new SharedTextMarker(markers, primary);
  }

  function findSharedMarkers(doc) {
	logger.debug(`>>> Entering findSharedMarkers(doc=${doc})`);
	logger.debug(`<<< Exiting findSharedMarkers(doc=${doc})`);
    return doc.findMarks(Pos(doc.first, 0), doc.clipPos(Pos(doc.lastLine())),
                         function(m) { return m.parent; });
  }

  function copySharedMarkers(doc, markers) {
	logger.debug(`>>> Entering copySharedMarkers(doc=${doc},markers=${markers})`);
    for (var i = 0; i < markers.length; i++) {
      var marker = markers[i], pos = marker.find();
	logger.debug(`copySharedMarkers(doc=${doc},markers=${markers}): marker → ${marker}`);
      var mFrom = doc.clipPos(pos.from), mTo = doc.clipPos(pos.to);
	logger.debug(`copySharedMarkers(doc=${doc},markers=${markers}): mFrom → ${mFrom}`);
      if (cmp(mFrom, mTo)) {
        var subMark = markText(doc, mFrom, mTo, marker.primary, marker.primary.type);
		logger.debug(`copySharedMarkers(doc=${doc},markers=${markers}): subMark → ${subMark}`);
        marker.markers.push(subMark);
        subMark.parent = marker;
		logger.debug(`copySharedMarkers(doc=${doc},markers=${markers}): subMark.parent → ${subMark.parent}`);
      }
	logger.debug(`<<< Exiting copySharedMarkers(doc=${doc},markers=${markers})`);
    }
  }

  function detachSharedMarkers(markers) {
	logger.debug(`>>> Entering detachSharedMarkers(markers=${markers})`);
    for (var i = 0; i < markers.length; i++) {
      var marker = markers[i], linked = [marker.primary.doc];;
      linkedDocs(marker.primary.doc, function(d) { linked.push(d); });
      for (var j = 0; j < marker.markers.length; j++) {
        var subMarker = marker.markers[j];
        if (indexOf(linked, subMarker.doc) == -1) {
          subMarker.parent = null;
		logger.debug(`detachSharedMarkers(markers=${markers}): subMarker.parent → ${subMarker.parent}`);
          marker.markers.splice(j--, 1);
		logger.debug(`<<< Exiting detachSharedMarkers(markers=${markers})`);
        }
      }
    }
  }

  // TEXTMARKER SPANS

  function MarkedSpan(marker, from, to) {
	logger.debug(`>>> Entering MarkedSpan(marker=${marker},from=${from},to=${to})`);
    this.marker = marker;
    this.from = from; this.to = to;
	logger.debug(`MarkedSpan(marker=${marker},from=${from},to=${to}): this.marker → ${this.marker}`);
	logger.debug(`MarkedSpan(marker=${marker},from=${from},to=${to}): marker → ${marker}`);
	logger.debug(`MarkedSpan(marker=${marker},from=${from},to=${to}): this.from → ${this.from}`);
	logger.debug(`<<< Exiting MarkedSpan(marker=${marker},from=${from},to=${to})`);
  }

  // Search an array of spans for a span matching the given marker.
  function getMarkedSpanFor(spans, marker) {
	logger.debug(`>>> Entering getMarkedSpanFor(spans=${spans},marker=${marker})`);
    if (spans) for (var i = 0; i < spans.length; ++i) {
      var span = spans[i];
      if (span.marker == marker) return span;
	logger.debug(`<<< Exiting getMarkedSpanFor(spans=${spans},marker=${marker})`);
    }
  }
  // Remove a span from an array, returning undefined if no spans are
  // left (we don't store arrays for lines without spans).
  function removeMarkedSpan(spans, span) {
	logger.debug(`>>> Entering removeMarkedSpan(spans=${spans},span=${span})`);
    for (var r, i = 0; i < spans.length; ++i)
      if (spans[i] != span) (r || (r = [])).push(spans[i]);
	logger.debug(`<<< Exiting removeMarkedSpan(spans=${spans},span=${span})`);
    return r;
  }
  // Add a span to a line.
  function addMarkedSpan(line, span) {
	logger.debug(`>>> Entering addMarkedSpan(line=${line},span=${span})`);
    line.markedSpans = line.markedSpans ? line.markedSpans.concat([span]) : [span];
    span.marker.attachLine(line);
	logger.debug(`<<< Exiting addMarkedSpan(line=${line},span=${span})`);
  }

  // Used for the algorithm that adjusts markers for a change in the
  // document. These functions cut an array of spans at a given
  // character position, returning an array of remaining chunks (or
  // undefined if nothing remains).
  function markedSpansBefore(old, startCh, isInsert) {
	logger.debug(`>>> Entering markedSpansBefore(old=${old},startCh=${startCh},isInsert=${isInsert})`);
    if (old) for (var i = 0, nw; i < old.length; ++i) {
      var span = old[i], marker = span.marker;
      var startsBefore = span.from == null || (marker.inclusiveLeft ? span.from <= startCh : span.from < startCh);
      if (startsBefore || span.from == startCh && marker.type == "bookmark" && (!isInsert || !span.marker.insertLeft)) {
        var endsAfter = span.to == null || (marker.inclusiveRight ? span.to >= startCh : span.to > startCh);
		logger.debug(`markedSpansBefore(old=${old},startCh=${startCh},isInsert=${isInsert}): endsAfter → ${endsAfter}`);
        (nw || (nw = [])).push(new MarkedSpan(marker, span.from, endsAfter ? null : span.to));
      }
    }
	logger.debug(`markedSpansBefore(old=${old},startCh=${startCh},isInsert=${isInsert}): startsBefore → ${startsBefore}`);
	logger.debug(`<<< Exiting markedSpansBefore(old=${old},startCh=${startCh},isInsert=${isInsert})`);
    return nw;
  }
  function markedSpansAfter(old, endCh, isInsert) {
	logger.debug(`>>> Entering markedSpansAfter(old=${old},endCh=${endCh},isInsert=${isInsert})`);
    if (old) for (var i = 0, nw; i < old.length; ++i) {
      var span = old[i], marker = span.marker;
      var endsAfter = span.to == null || (marker.inclusiveRight ? span.to >= endCh : span.to > endCh);
      if (endsAfter || span.from == endCh && marker.type == "bookmark" && (!isInsert || span.marker.insertLeft)) {
        var startsBefore = span.from == null || (marker.inclusiveLeft ? span.from <= endCh : span.from < endCh);
        (nw || (nw = [])).push(new MarkedSpan(marker, startsBefore ? null : span.from - endCh,
                                              span.to == null ? null : span.to - endCh));
      }
    }
	logger.debug(`markedSpansAfter(old=${old},endCh=${endCh},isInsert=${isInsert}): endsAfter → ${endsAfter}`);
	logger.debug(`markedSpansAfter(old=${old},endCh=${endCh},isInsert=${isInsert}): startsBefore → ${startsBefore}`);
	logger.debug(`<<< Exiting markedSpansAfter(old=${old},endCh=${endCh},isInsert=${isInsert})`);
    return nw;
  }

  // Given a change object, compute the new set of marker spans that
  // cover the line in which the change took place. Removes spans
  // entirely within the change, reconnects spans belonging to the
  // same marker that appear on both sides of the change, and cuts off
  // spans partially within the change. Returns an array of span
  // arrays with one element for each line in (after) the change.
  function stretchSpansOverChange(doc, change) {
	logger.debug(`>>> Entering stretchSpansOverChange(doc=${doc},change=${change})`);
    if (change.full) return null;
    var oldFirst = isLine(doc, change.from.line) && getLine(doc, change.from.line).markedSpans;
	logger.debug(`stretchSpansOverChange(doc=${doc},change=${change}): oldFirst → ${oldFirst}`);
    var oldLast = isLine(doc, change.to.line) && getLine(doc, change.to.line).markedSpans;
	logger.debug(`stretchSpansOverChange(doc=${doc},change=${change}): oldLast → ${oldLast}`);
    if (!oldFirst && !oldLast) return null;

    var startCh = change.from.ch, endCh = change.to.ch, isInsert = cmp(change.from, change.to) == 0;
	logger.debug(`stretchSpansOverChange(doc=${doc},change=${change}): startCh → ${startCh}`);
    // Get the spans that 'stick out' on both sides
    var first = markedSpansBefore(oldFirst, startCh, isInsert);
	logger.debug(`stretchSpansOverChange(doc=${doc},change=${change}): first → ${first}`);
    var last = markedSpansAfter(oldLast, endCh, isInsert);
	logger.debug(`stretchSpansOverChange(doc=${doc},change=${change}): last → ${last}`);

    // Next, merge those two ends
    var sameLine = change.text.length == 1, offset = lst(change.text).length + (sameLine ? startCh : 0);
	logger.debug(`stretchSpansOverChange(doc=${doc},change=${change}): sameLine → ${sameLine}`);
    if (first) {
      // Fix up .to properties of first
      for (var i = 0; i < first.length; ++i) {
        var span = first[i];
        if (span.to == null) {
          var found = getMarkedSpanFor(last, span.marker);
          if (!found) span.to = startCh;
          else if (sameLine) span.to = found.to == null ? null : found.to + offset;
        }
      }
    }
    if (last) {
      // Fix up .from in last (or move them into first in case of sameLine)
      for (var i = 0; i < last.length; ++i) {
        var span = last[i];
        if (span.to != null) span.to += offset;
        if (span.from == null) {
          var found = getMarkedSpanFor(first, span.marker);
		logger.debug(`stretchSpansOverChange(doc=${doc},change=${change}): found → ${found}`);
          if (!found) {
            span.from = offset;
			logger.debug(`stretchSpansOverChange(doc=${doc},change=${change}): span.from → ${span.from}`);
            if (sameLine) (first || (first = [])).push(span);
          }
        } else {
          span.from += offset;
          if (sameLine) (first || (first = [])).push(span);
        }
      }
    }
    // Make sure we didn't create any zero-length spans
    if (first) first = clearEmptySpans(first);
    if (last && last != first) last = clearEmptySpans(last);

    var newMarkers = [first];
    if (!sameLine) {
      // Fill gap with whole-line-spans
      var gap = change.text.length - 2, gapMarkers;
      if (gap > 0 && first)
        for (var i = 0; i < first.length; ++i)
          if (first[i].to == null)
            (gapMarkers || (gapMarkers = [])).push(new MarkedSpan(first[i].marker, null, null));
      for (var i = 0; i < gap; ++i)
        newMarkers.push(gapMarkers);
      newMarkers.push(last);
    }
	logger.debug(`<<< Exiting stretchSpansOverChange(doc=${doc},change=${change})`);
    return newMarkers;
  }

  // Remove spans that are empty and don't have a clearWhenEmpty
  // option of false.
  function clearEmptySpans(spans) {
	logger.debug(`>>> Entering clearEmptySpans(spans=${spans})`);
    for (var i = 0; i < spans.length; ++i) {
      var span = spans[i];
      if (span.from != null && span.from == span.to && span.marker.clearWhenEmpty !== false)
        spans.splice(i--, 1);
    }
    if (!spans.length) return null;
	logger.debug(`<<< Exiting clearEmptySpans(spans=${spans})`);
    return spans;
  }

  // Used for un/re-doing changes from the history. Combines the
  // result of computing the existing spans with the set of spans that
  // existed in the history (so that deleting around a span and then
  // undoing brings back the span).
  function mergeOldSpans(doc, change) {
	logger.debug(`>>> Entering mergeOldSpans(doc=${doc},change=${change})`);
    var old = getOldSpans(doc, change);
    var stretched = stretchSpansOverChange(doc, change);
	logger.debug(`mergeOldSpans(doc=${doc},change=${change}): old → ${old}`);
	logger.debug(`mergeOldSpans(doc=${doc},change=${change}): stretched → ${stretched}`);
    if (!old) return stretched;
    if (!stretched) return old;

    for (var i = 0; i < old.length; ++i) {
      var oldCur = old[i], stretchCur = stretched[i];
      if (oldCur && stretchCur) {
        spans: for (var j = 0; j < stretchCur.length; ++j) {
          var span = stretchCur[j];
          for (var k = 0; k < oldCur.length; ++k)
            if (oldCur[k].marker == span.marker) continue spans;
          oldCur.push(span);
        }
      } else if (stretchCur) {
        old[i] = stretchCur;
      }
    }
	logger.debug(`<<< Exiting mergeOldSpans(doc=${doc},change=${change})`);
    return old;
  }

  // Used to 'clip' out readOnly ranges when making a change.
  function removeReadOnlyRanges(doc, from, to) {
    var markers = null;
    doc.iter(from.line, to.line + 1, function(line) {
      if (line.markedSpans) for (var i = 0; i < line.markedSpans.length; ++i) {
        var mark = line.markedSpans[i].marker;
        if (mark.readOnly && (!markers || indexOf(markers, mark) == -1))
          (markers || (markers = [])).push(mark);
      }
    });
    if (!markers) return null;
    var parts = [{from: from, to: to}];
    for (var i = 0; i < markers.length; ++i) {
      var mk = markers[i], m = mk.find(0);
      for (var j = 0; j < parts.length; ++j) {
        var p = parts[j];
        if (cmp(p.to, m.from) < 0 || cmp(p.from, m.to) > 0) continue;
        var newParts = [j, 1], dfrom = cmp(p.from, m.from), dto = cmp(p.to, m.to);
		logger.debug(`removeReadOnlyRanges(doc=${doc},from=${from},to=${to}): newParts → ${newParts}`);
        if (dfrom < 0 || !mk.inclusiveLeft && !dfrom)
          newParts.push({from: p.from, to: m.from});
        if (dto > 0 || !mk.inclusiveRight && !dto)
          newParts.push({from: m.to, to: p.to});
        parts.splice.apply(parts, newParts);
        j += newParts.length - 1;
      }
    }
	logger.debug(`removeReadOnlyRanges(doc=${doc},from=${from},to=${to}): mk → ${mk}`);
    return parts;
  }

  // Connect or disconnect spans from a line.
  function detachMarkedSpans(line) {
	logger.debug(`>>> Entering detachMarkedSpans(line=${line})`);
    var spans = line.markedSpans;
    if (!spans) return;
    for (var i = 0; i < spans.length; ++i)
      spans[i].marker.detachLine(line);
    line.markedSpans = null;
	logger.debug(`detachMarkedSpans(line=${line}): line.markedSpans → ${line.markedSpans}`);
	logger.debug(`<<< Exiting detachMarkedSpans(line=${line})`);
  }
  function attachMarkedSpans(line, spans) {
	logger.debug(`>>> Entering attachMarkedSpans(line=${line},spans=${spans})`);
    if (!spans) return;
    for (var i = 0; i < spans.length; ++i)
      spans[i].marker.attachLine(line);
    line.markedSpans = spans;
	logger.debug(`attachMarkedSpans(line=${line},spans=${spans}): line.markedSpans → ${line.markedSpans}`);
	logger.debug(`<<< Exiting attachMarkedSpans(line=${line},spans=${spans})`);
  }

  // Helpers used when computing which overlapping collapsed span
  // counts as the larger one.
  function extraLeft(marker) { return marker.inclusiveLeft ? -1 : 0; }
  function extraRight(marker) { return marker.inclusiveRight ? 1 : 0; }

  // Returns a number indicating which of two overlapping collapsed
  // spans is larger (and thus includes the other). Falls back to
  // comparing ids when the spans cover exactly the same range.
  function compareCollapsedMarkers(a, b) {
	logger.debug(`>>> Entering compareCollapsedMarkers(a=${a},b=${b})`);
    var lenDiff = a.lines.length - b.lines.length;
    if (lenDiff != 0) return lenDiff;
    var aPos = a.find(), bPos = b.find();
	logger.debug(`compareCollapsedMarkers(a=${a},b=${b}): aPos → ${aPos}`);
    var fromCmp = cmp(aPos.from, bPos.from) || extraLeft(a) - extraLeft(b);
	logger.debug(`compareCollapsedMarkers(a=${a},b=${b}): fromCmp → ${fromCmp}`);
    if (fromCmp) return -fromCmp;
    var toCmp = cmp(aPos.to, bPos.to) || extraRight(a) - extraRight(b);
	logger.debug(`compareCollapsedMarkers(a=${a},b=${b}): toCmp → ${toCmp}`);
    if (toCmp) return toCmp;
	logger.debug(`<<< Exiting compareCollapsedMarkers(a=${a},b=${b})`);
    return b.id - a.id;
  }

  // Find out whether a line ends or starts in a collapsed span. If
  // so, return the marker for that span.
  function collapsedSpanAtSide(line, start) {
	logger.debug(`>>> Entering collapsedSpanAtSide(line=${line},start=${start})`);
    var sps = sawCollapsedSpans && line.markedSpans, found;
    if (sps) for (var sp, i = 0; i < sps.length; ++i) {
      sp = sps[i];
      if (sp.marker.collapsed && (start ? sp.from : sp.to) == null &&
          (!found || compareCollapsedMarkers(found, sp.marker) < 0))
        found = sp.marker;
    }
	logger.debug(`<<< Exiting collapsedSpanAtSide(line=${line},start=${start})`);
    return found;
  }
  function collapsedSpanAtStart(line) { return collapsedSpanAtSide(line, true); }
  function collapsedSpanAtEnd(line) { return collapsedSpanAtSide(line, false); }

  // Test whether there exists a collapsed span that partially
  // overlaps (covers the start or end, but not both) of a new span.
  // Such overlap is not allowed.
  function conflictingCollapsedRange(doc, lineNo, from, to, marker) {
    var line = getLine(doc, lineNo);
    var sps = sawCollapsedSpans && line.markedSpans;
    if (sps) for (var i = 0; i < sps.length; ++i) {
      var sp = sps[i];
      if (!sp.marker.collapsed) continue;
      var found = sp.marker.find(0);
	logger.debug(`conflictingCollapsedRange(doc=${doc},lineNo=${lineNo},from=${from},to=${to},marker=${marker}): found → ${found}`);
      var fromCmp = cmp(found.from, from) || extraLeft(sp.marker) - extraLeft(marker);
	logger.debug(`conflictingCollapsedRange(doc=${doc},lineNo=${lineNo},from=${from},to=${to},marker=${marker}): fromCmp → ${fromCmp}`);
      var toCmp = cmp(found.to, to) || extraRight(sp.marker) - extraRight(marker);
	logger.debug(`conflictingCollapsedRange(doc=${doc},lineNo=${lineNo},from=${from},to=${to},marker=${marker}): toCmp → ${toCmp}`);
      if (fromCmp >= 0 && toCmp <= 0 || fromCmp <= 0 && toCmp >= 0) continue;
      if (fromCmp <= 0 && (cmp(found.to, from) > 0 || (sp.marker.inclusiveRight && marker.inclusiveLeft)) ||
          fromCmp >= 0 && (cmp(found.from, to) < 0 || (sp.marker.inclusiveLeft && marker.inclusiveRight)))
        return true;
    }
  }

  // A visual line is a line as drawn on the screen. Folding, for
  // example, can cause multiple logical lines to appear on the same
  // visual line. This finds the start of the visual line that the
  // given line is part of (usually that is the line itself).
  function visualLine(line) {
	logger.debug(`>>> Entering visualLine(line=${line})`);
    var merged;
    while (merged = collapsedSpanAtStart(line))
      line = merged.find(-1, true).line;
	logger.debug(`visualLine(line=${line}): line → ${line}`);
	logger.debug(`<<< Exiting visualLine(line=${line})`);
    return line;
  }

  // Returns an array of logical lines that continue the visual line
  // started by the argument, or undefined if there are no such lines.
  function visualLineContinued(line) {
	logger.debug(`>>> Entering visualLineContinued(line=${line})`);
    var merged, lines;
    while (merged = collapsedSpanAtEnd(line)) {
      line = merged.find(1, true).line;
      (lines || (lines = [])).push(line);
	logger.debug(`visualLineContinued(line=${line}): line → ${line}`);
    }
	logger.debug(`<<< Exiting visualLineContinued(line=${line})`);
    return lines;
  }

  // Get the line number of the start of the visual line that the
  // given line number is part of.
  function visualLineNo(doc, lineN) {
	logger.debug(`>>> Entering visualLineNo(doc=${doc},lineN=${lineN})`);
    var line = getLine(doc, lineN), vis = visualLine(line);
    if (line == vis) return lineN;
	logger.debug(`visualLineNo(doc=${doc},lineN=${lineN}): line → ${line}`);
	logger.debug(`<<< Exiting visualLineNo(doc=${doc},lineN=${lineN})`);
    return lineNo(vis);
  }
  // Get the line number of the start of the next visual line after
  // the given line.
  function visualLineEndNo(doc, lineN) {
	logger.debug(`>>> Entering visualLineEndNo(doc=${doc},lineN=${lineN})`);
    if (lineN > doc.lastLine()) return lineN;
    var line = getLine(doc, lineN), merged;
    if (!lineIsHidden(doc, line)) return lineN;
    while (merged = collapsedSpanAtEnd(line))
      line = merged.find(1, true).line;
	logger.debug(`visualLineEndNo(doc=${doc},lineN=${lineN}): line → ${line}`);
	logger.debug(`<<< Exiting visualLineEndNo(doc=${doc},lineN=${lineN})`);
    return lineNo(line) + 1;
  }

  // Compute whether a line is hidden. Lines count as hidden when they
  // are part of a visual line that starts with another line, or when
  // they are entirely covered by collapsed, non-widget span.
  function lineIsHidden(doc, line) {
	logger.debug(`>>> Entering lineIsHidden(doc=${doc},line=${line})`);
    var sps = sawCollapsedSpans && line.markedSpans;
    if (sps) for (var sp, i = 0; i < sps.length; ++i) {
      sp = sps[i];
      if (!sp.marker.collapsed) continue;
      if (sp.from == null) return true;
      if (sp.marker.widgetNode) continue;
      if (sp.from == 0 && sp.marker.inclusiveLeft && lineIsHiddenInner(doc, line, sp))
		logger.debug(`<<< Exiting lineIsHidden(doc=${doc},line=${line})`);
        return true;
    }
  }
  function lineIsHiddenInner(doc, line, span) {
	logger.debug(`>>> Entering lineIsHiddenInner(doc=${doc},line=${line},span=${span})`);
    if (span.to == null) {
      var end = span.marker.find(1, true);
	logger.debug(`lineIsHiddenInner(doc=${doc},line=${line},span=${span}): end → ${end}`);
      return lineIsHiddenInner(doc, end.line, getMarkedSpanFor(end.line.markedSpans, span.marker));
    }
    if (span.marker.inclusiveRight && span.to == line.text.length)
      return true;
    for (var sp, i = 0; i < line.markedSpans.length; ++i) {
      sp = line.markedSpans[i];
      if (sp.marker.collapsed && !sp.marker.widgetNode && sp.from == span.to &&
          (sp.to == null || sp.to != span.from) &&
          (sp.marker.inclusiveLeft || span.marker.inclusiveRight) &&
          lineIsHiddenInner(doc, line, sp)) return true;
    }
	logger.debug(`<<< Exiting lineIsHiddenInner(doc=${doc},line=${line},span=${span})`);
  }

  // LINE WIDGETS

  // Line widgets are block elements displayed above or below a line.

  var LineWidget = CodeMirror.LineWidget = function(doc, node, options) {
    if (options) for (var opt in options) if (options.hasOwnProperty(opt))
      this[opt] = options[opt];
    this.doc = doc;
    this.node = node;
  };
  eventMixin(LineWidget);

  function adjustScrollWhenAboveVisible(cm, line, diff) {
	logger.debug(`>>> Entering adjustScrollWhenAboveVisible(cm=${cm},line=${line},diff=${diff})`);
    if (heightAtLine(line) < ((cm.curOp && cm.curOp.scrollTop) || cm.doc.scrollTop))
      addToScrollPos(cm, null, diff);
	logger.debug(`<<< Exiting adjustScrollWhenAboveVisible(cm=${cm},line=${line},diff=${diff})`);
  }

  LineWidget.prototype.clear = function() {
    var cm = this.doc.cm, ws = this.line.widgets, line = this.line, no = lineNo(line);
    if (no == null || !ws) return;
    for (var i = 0; i < ws.length; ++i) if (ws[i] == this) ws.splice(i--, 1);
    if (!ws.length) line.widgets = null;
    var height = widgetHeight(this);
    updateLineHeight(line, Math.max(0, line.height - height));
    if (cm) runInOp(cm, function() {
      adjustScrollWhenAboveVisible(cm, line, -height);
      regLineChange(cm, no, "widget");
    });
  };
  LineWidget.prototype.changed = function() {
    var oldH = this.height, cm = this.doc.cm, line = this.line;
    this.height = null;
    var diff = widgetHeight(this) - oldH;
	logger.debug(`function(): diff → ${diff}`);
    if (!diff) return;
    updateLineHeight(line, line.height + diff);
    if (cm) runInOp(cm, function() {
      cm.curOp.forceUpdate = true;
      adjustScrollWhenAboveVisible(cm, line, diff);
    });
  };

  function widgetHeight(widget) {
	logger.debug(`>>> Entering widgetHeight(widget=${widget})`);
    if (widget.height != null) return widget.height;
    var cm = widget.doc.cm;
    if (!cm) return 0;
    if (!contains(document.body, widget.node)) {
      var parentStyle = "position: relative;";
	logger.debug(`widgetHeight(widget=${widget}): parentStyle → ${parentStyle}`);
      if (widget.coverGutter)
        parentStyle += "margin-left: -" + cm.display.gutters.offsetWidth + "px;";
      if (widget.noHScroll)
        parentStyle += "width: " + cm.display.wrapper.clientWidth + "px;";
      removeChildrenAndAdd(cm.display.measure, elt("div", [widget.node], null, parentStyle));
    }
	logger.debug(`<<< Exiting widgetHeight(widget=${widget})`);
    return widget.height = widget.node.offsetHeight;
  }

  function addLineWidget(doc, handle, node, options) {
	logger.debug(`>>> Entering addLineWidget(doc=${doc},handle=${handle},node=${node},options=${options})`);
    var widget = new LineWidget(doc, node, options);
    var cm = doc.cm;
	logger.debug(`addLineWidget(doc=${doc},handle=${handle},node=${node},options=${options}): widget → ${widget}`);
    if (cm && widget.noHScroll) cm.display.alignWidgets = true;
    changeLine(doc, handle, "widget", function(line) {
      var widgets = line.widgets || (line.widgets = []);
      if (widget.insertAt == null) widgets.push(widget);
      else widgets.splice(Math.min(widgets.length - 1, Math.max(0, widget.insertAt)), 0, widget);
      widget.line = line;
      if (cm && !lineIsHidden(doc, line)) {
        var aboveVisible = heightAtLine(line) < doc.scrollTop;
        updateLineHeight(line, line.height + widgetHeight(widget));
        if (aboveVisible) addToScrollPos(cm, null, widget.height);
        cm.curOp.forceUpdate = true;
      }
	logger.debug(`<<< Exiting addLineWidget(doc=${doc},handle=${handle},node=${node},options=${options})`);
      return true;
    });
    return widget;
  }

  // LINE DATA STRUCTURE

  // Line objects. These hold state related to a line, including
  // highlighting info (the styles array).
  var Line = CodeMirror.Line = function(text, markedSpans, estimateHeight) {
    this.text = text;
    attachMarkedSpans(this, markedSpans);
    this.height = estimateHeight ? estimateHeight(this) : 1;
  };
  eventMixin(Line);
  Line.prototype.lineNo = function() { return lineNo(this); };

  // Change the content (text, markers) of a line. Automatically
  // invalidates cached information and tries to re-estimate the
  // line's height.
  function updateLine(line, text, markedSpans, estimateHeight) {
	logger.debug(`>>> Entering updateLine(line=${line},text=${text},markedSpans=${markedSpans},estimateHeight=${estimateHeight})`);
    line.text = text;
    if (line.stateAfter) line.stateAfter = null;
	logger.debug(`updateLine(line=${line},text=${text},markedSpans=${markedSpans},estimateHeight=${estimateHeight}): line.text → ${line.text}`);
    if (line.styles) line.styles = null;
    if (line.order != null) line.order = null;
    detachMarkedSpans(line);
    attachMarkedSpans(line, markedSpans);
    var estHeight = estimateHeight ? estimateHeight(line) : 1;
    if (estHeight != line.height) updateLineHeight(line, estHeight);
	logger.debug(`updateLine(line=${line},text=${text},markedSpans=${markedSpans},estimateHeight=${estimateHeight}): estHeight → ${estHeight}`);
	logger.debug(`<<< Exiting updateLine(line=${line},text=${text},markedSpans=${markedSpans},estimateHeight=${estimateHeight})`);
  }

  // Detach a line from the document tree and its markers.
  function cleanUpLine(line) {
	logger.debug(`>>> Entering cleanUpLine(line=${line})`);
    line.parent = null;
    detachMarkedSpans(line);
	logger.debug(`cleanUpLine(line=${line}): line.parent → ${line.parent}`);
	logger.debug(`<<< Exiting cleanUpLine(line=${line})`);
  }

  function extractLineClasses(type, output) {
	logger.debug(`>>> Entering extractLineClasses(type=${type},output=${output})`);
    if (type) for (;;) {
      var lineClass = type.match(/(?:^|\s+)line-(background-)?(\S+)/);
	logger.debug(`extractLineClasses(type=${type},output=${output}): lineClass → ${lineClass}`);
      if (!lineClass) break;
      type = type.slice(0, lineClass.index) + type.slice(lineClass.index + lineClass[0].length);
      var prop = lineClass[1] ? "bgClass" : "textClass";
	logger.debug(`extractLineClasses(type=${type},output=${output}): type → ${type}`);
	logger.debug(`extractLineClasses(type=${type},output=${output}): prop → ${prop}`);
      if (output[prop] == null)
        output[prop] = lineClass[2];
      else if (!(new RegExp("(?:^|\s)" + lineClass[2] + "(?:$|\s)")).test(output[prop]))
        output[prop] += " " + lineClass[2];
    }
	logger.debug(`<<< Exiting extractLineClasses(type=${type},output=${output})`);
    return type;
  }

  function callBlankLine(mode, state) {
	logger.debug(`>>> Entering callBlankLine(mode=${mode},state=${state})`);
    if (mode.blankLine) return mode.blankLine(state);
    if (!mode.innerMode) return;
    var inner = CodeMirror.innerMode(mode, state);
    if (inner.mode.blankLine) return inner.mode.blankLine(inner.state);
	logger.debug(`callBlankLine(mode=${mode},state=${state}): inner → ${inner}`);
	logger.debug(`<<< Exiting callBlankLine(mode=${mode},state=${state})`);
  }

  function readToken(mode, stream, state, inner) {
	logger.debug(`>>> Entering readToken(mode=${mode},stream=${stream},state=${state},inner=${inner})`);
    for (var i = 0; i < 10; i++) {
      if (inner) inner[0] = CodeMirror.innerMode(mode, state).mode;
      var style = mode.token(stream, state);
      if (stream.pos > stream.start) return style;
	logger.debug(`readToken(mode=${mode},stream=${stream},state=${state},inner=${inner}): style → ${style}`);
    }
	logger.debug(`<<< Exiting readToken(mode=${mode},stream=${stream},state=${state},inner=${inner})`);
    throw new Error("Mode " + mode.name + " failed to advance stream.");
  }

  // Utility for getTokenAt and getLineTokens
  function takeToken(cm, pos, precise, asArray) {
    function getObj(copy) {
	logger.debug(`>>> Entering getObj(copy=${copy})`);
	logger.debug(`<<< Exiting getObj(copy=${copy})`);
      return {start: stream.start, end: stream.pos,
              string: stream.current(),
              type: style || null,
              state: copy ? copyState(doc.mode, state) : state};
    }

    var doc = cm.doc, mode = doc.mode, style;
    pos = clipPos(doc, pos);
    var line = getLine(doc, pos.line), state = getStateBefore(cm, pos.line, precise);
    var stream = new StringStream(line.text, cm.options.tabSize), tokens;
    if (asArray) tokens = [];
    while ((asArray || stream.pos < pos.ch) && !stream.eol()) {
      stream.start = stream.pos;
      style = readToken(mode, stream, state);
      if (asArray) tokens.push(getObj(true));
    }
    return asArray ? tokens : getObj();
  }

  // Run the given mode's parser over a line, calling f for each token.
  function runMode(cm, text, mode, state, f, lineClasses, forceToEnd) {
	logger.debug(`>>> Entering runMode(cm=${cm},text=${text},mode=${mode},state=${state},f=${f},lineClasses=${lineClasses},forceToEnd=${forceToEnd})`);
    var flattenSpans = mode.flattenSpans;
    if (flattenSpans == null) flattenSpans = cm.options.flattenSpans;
    var curStart = 0, curStyle = null;
    var stream = new StringStream(text, cm.options.tabSize), style;
	logger.debug(`runMode(cm=${cm},text=${text},mode=${mode},state=${state},f=${f},lineClasses=${lineClasses},forceToEnd=${forceToEnd}): stream → ${stream}`);
    var inner = cm.options.addModeClass && [null];
    if (text == "") extractLineClasses(callBlankLine(mode, state), lineClasses);
    while (!stream.eol()) {
      if (stream.pos > cm.options.maxHighlightLength) {
        flattenSpans = false;
        if (forceToEnd) processLine(cm, text, state, stream.pos);
        stream.pos = text.length;
		logger.debug(`runMode(cm=${cm},text=${text},mode=${mode},state=${state},f=${f},lineClasses=${lineClasses},forceToEnd=${forceToEnd}): stream.pos → ${stream.pos}`);
        style = null;
      } else {
        style = extractLineClasses(readToken(mode, stream, state, inner), lineClasses);
		logger.debug(`runMode(cm=${cm},text=${text},mode=${mode},state=${state},f=${f},lineClasses=${lineClasses},forceToEnd=${forceToEnd}): style → ${style}`);
      }
      if (inner) {
        var mName = inner[0].name;
        if (mName) style = "m-" + (style ? mName + " " + style : mName);
      }
      if (!flattenSpans || curStyle != style) {
        while (curStart < stream.start) {
          curStart = Math.min(stream.start, curStart + 50000);
          f(curStart, curStyle);
        }
        curStyle = style;
      }
      stream.start = stream.pos;
	logger.debug(`runMode(cm=${cm},text=${text},mode=${mode},state=${state},f=${f},lineClasses=${lineClasses},forceToEnd=${forceToEnd}): stream.start → ${stream.start}`);
    }
    while (curStart < stream.pos) {
      // Webkit seems to refuse to render text nodes longer than 57444 characters
      var pos = Math.min(stream.pos, curStart + 50000);
	logger.debug(`runMode(cm=${cm},text=${text},mode=${mode},state=${state},f=${f},lineClasses=${lineClasses},forceToEnd=${forceToEnd}): pos → ${pos}`);
      f(pos, curStyle);
      curStart = pos;
	logger.debug(`<<< Exiting runMode(cm=${cm},text=${text},mode=${mode},state=${state},f=${f},lineClasses=${lineClasses},forceToEnd=${forceToEnd})`);
    }
  }

  // Compute a style array (an array starting with a mode generation
  // -- for invalidation -- followed by pairs of end positions and
  // style strings), which is used to highlight the tokens on the
  // line.
  function highlightLine(cm, line, state, forceToEnd) {
	logger.debug(`>>> Entering highlightLine(cm=${cm},line=${line},state=${state},forceToEnd=${forceToEnd})`);
    // A styles array always starts with a number identifying the
    // mode/overlays that it is based on (for easy invalidation).
    var st = [cm.state.modeGen], lineClasses = {};
    // Compute the base array of styles
    runMode(cm, line.text, cm.doc.mode, state, function(end, style) {
	logger.debug(`>>> Entering runMode(cm=${cm},state=${state},lineClasses=${lineClasses},forceToEnd=${forceToEnd})`);
	logger.debug(`<<< Exiting runMode(cm=${cm},state=${state},lineClasses=${lineClasses},forceToEnd=${forceToEnd})`);
      st.push(end, style);
    }, lineClasses, forceToEnd);

    // Run overlays, adjust style array.
    for (var o = 0; o < cm.state.overlays.length; ++o) {
      var overlay = cm.state.overlays[o], i = 1, at = 0;
      runMode(cm, line.text, overlay.mode, true, function(end, style) {
        var start = i;
        // Ensure there's a token end at the current position, and that i points at it
        while (at < end) {
          var i_end = st[i];
          if (i_end > end)
            st.splice(i, 1, end, st[i+1], i_end);
          i += 2;
          at = Math.min(end, i_end);
        }
        if (!style) return;
        if (overlay.opaque) {
          st.splice(start, i - start, end, "cm-overlay " + style);
          i = start + 2;
        } else {
          for (; start < i; start += 2) {
            var cur = st[start+1];
            st[start+1] = (cur ? cur + " " : "") + "cm-overlay " + style;
          }
        }
      }, lineClasses);
    }

	logger.debug(`<<< Exiting highlightLine(cm=${cm},line=${line},state=${state},forceToEnd=${forceToEnd})`);
    return {styles: st, classes: lineClasses.bgClass || lineClasses.textClass ? lineClasses : null};
  }

  function getLineStyles(cm, line, updateFrontier) {
	logger.debug(`>>> Entering getLineStyles(cm=${cm},line=${line},updateFrontier=${updateFrontier})`);
    if (!line.styles || line.styles[0] != cm.state.modeGen) {
      var state = getStateBefore(cm, lineNo(line));
      var result = highlightLine(cm, line, line.text.length > cm.options.maxHighlightLength ? copyState(cm.doc.mode, state) : state);
	logger.debug(`getLineStyles(cm=${cm},line=${line},updateFrontier=${updateFrontier}): result → ${result}`);
      line.stateAfter = state;
      line.styles = result.styles;
	logger.debug(`getLineStyles(cm=${cm},line=${line},updateFrontier=${updateFrontier}): line.stateAfter → ${line.stateAfter}`);
      if (result.classes) line.styleClasses = result.classes;
      else if (line.styleClasses) line.styleClasses = null;
	logger.debug(`getLineStyles(cm=${cm},line=${line},updateFrontier=${updateFrontier}): line.styles → ${line.styles}`);
      if (updateFrontier === cm.doc.frontier) cm.doc.frontier++;
    }
	logger.debug(`<<< Exiting getLineStyles(cm=${cm},line=${line},updateFrontier=${updateFrontier})`);
    return line.styles;
  }

  // Lightweight form of highlight -- proceed over this line and
  // update state, but don't save a style array. Used for lines that
  // aren't currently visible.
  function processLine(cm, text, state, startAt) {
	logger.debug(`>>> Entering processLine(cm=${cm},text=${text},state=${state},startAt=${startAt})`);
    var mode = cm.doc.mode;
    var stream = new StringStream(text, cm.options.tabSize);
	logger.debug(`processLine(cm=${cm},text=${text},state=${state},startAt=${startAt}): stream → ${stream}`);
    stream.start = stream.pos = startAt || 0;
    if (text == "") callBlankLine(mode, state);
	logger.debug(`processLine(cm=${cm},text=${text},state=${state},startAt=${startAt}): stream.start → ${stream.start}`);
    while (!stream.eol()) {
      readToken(mode, stream, state);
      stream.start = stream.pos;
	logger.debug(`<<< Exiting processLine(cm=${cm},text=${text},state=${state},startAt=${startAt})`);
    }
  }

  // Convert a style as returned by a mode (either null, or a string
  // containing one or more styles) to a CSS style. This is cached,
  // and also looks for line-wide styles.
  var styleToClassCache = {}, styleToClassCacheWithMode = {};
  function interpretTokenStyle(style, options) {
	logger.debug(`>>> Entering interpretTokenStyle(style=${style},options=${options})`);
    if (!style || /^\s*$/.test(style)) return null;
    var cache = options.addModeClass ? styleToClassCacheWithMode : styleToClassCache;
	logger.debug(`<<< Exiting interpretTokenStyle(style=${style},options=${options})`);
    return cache[style] ||
      (cache[style] = style.replace(/\S+/g, "cm-$&"));
  }

  // Render the DOM representation of the text of a line. Also builds
  // up a 'line map', which points at the DOM nodes that represent
  // specific stretches of text, and is used by the measuring code.
  // The returned object contains the DOM node, this map, and
  // information about line-wide styles that were set by the mode.
  function buildLineContent(cm, lineView) {
	logger.debug(`>>> Entering buildLineContent(cm=${cm},lineView=${lineView})`);
    // The padding-right forces the element to have a 'border', which
    // is needed on Webkit to be able to get line-level bounding
    // rectangles for it (in measureChar).
    var content = elt("span", null, null, webkit ? "padding-right: .1px" : null);
	logger.debug(`buildLineContent(cm=${cm},lineView=${lineView}): content → ${content}`);
    var builder = {pre: elt("pre", [content], "CodeMirror-line"), content: content,
                   col: 0, pos: 0, cm: cm,
                   splitSpaces: (ie || webkit) && cm.getOption("lineWrapping")};
    lineView.measure = {};
	logger.debug(`buildLineContent(cm=${cm},lineView=${lineView}): builder → ${builder}`);
	logger.debug(`buildLineContent(cm=${cm},lineView=${lineView}): lineView.measure → ${lineView.measure}`);

    // Iterate over the logical lines that make up this visual line.
    for (var i = 0; i <= (lineView.rest ? lineView.rest.length : 0); i++) {
      var line = i ? lineView.rest[i - 1] : lineView.line, order;
      builder.pos = 0;
	logger.debug(`buildLineContent(cm=${cm},lineView=${lineView}): builder.pos → ${builder.pos}`);
      builder.addToken = buildToken;
	logger.debug(`buildLineContent(cm=${cm},lineView=${lineView}): builder.addToken → ${builder.addToken}`);
      // Optionally wire in some hacks into the token-rendering
      // algorithm, to deal with browser quirks.
      if (hasBadBidiRects(cm.display.measure) && (order = getOrder(line)))
        builder.addToken = buildTokenBadBidi(builder.addToken, order);
      builder.map = [];
	logger.debug(`buildLineContent(cm=${cm},lineView=${lineView}): builder.map → ${builder.map}`);
      var allowFrontierUpdate = lineView != cm.display.externalMeasured && lineNo(line);
	logger.debug(`buildLineContent(cm=${cm},lineView=${lineView}): allowFrontierUpdate → ${allowFrontierUpdate}`);
      insertLineContent(line, builder, getLineStyles(cm, line, allowFrontierUpdate));
      if (line.styleClasses) {
        if (line.styleClasses.bgClass)
          builder.bgClass = joinClasses(line.styleClasses.bgClass, builder.bgClass || "");
        if (line.styleClasses.textClass)
          builder.textClass = joinClasses(line.styleClasses.textClass, builder.textClass || "");
      }

      // Ensure at least a single node is present, for measuring.
      if (builder.map.length == 0)
        builder.map.push(0, 0, builder.content.appendChild(zeroWidthElement(cm.display.measure)));

      // Store the map and a cache object for the current logical line
      if (i == 0) {
        lineView.measure.map = builder.map;
        lineView.measure.cache = {};
      } else {
        (lineView.measure.maps || (lineView.measure.maps = [])).push(builder.map);
        (lineView.measure.caches || (lineView.measure.caches = [])).push({});
      }
    }

    // See issue #2901
    if (webkit && /\bcm-tab\b/.test(builder.content.lastChild.className))
      builder.content.className = "cm-tab-wrap-hack";

    signal(cm, "renderLine", cm, lineView.line, builder.pre);
    if (builder.pre.className)
      builder.textClass = joinClasses(builder.pre.className, builder.textClass || "");

	logger.debug(`<<< Exiting buildLineContent(cm=${cm},lineView=${lineView})`);
    return builder;
  }

  function defaultSpecialCharPlaceholder(ch) {
	logger.debug(`>>> Entering defaultSpecialCharPlaceholder(ch=${ch})`);
    var token = elt("span", "\u2022", "cm-invalidchar");
    token.title = "\\u" + ch.charCodeAt(0).toString(16);
    token.setAttribute("aria-label", token.title);
	logger.debug(`<<< Exiting defaultSpecialCharPlaceholder(ch=${ch})`);
    return token;
  }

  // Build up the DOM representation for a single token, and add it to
  // the line map. Takes care to render special characters separately.
  function buildToken(builder, text, style, startStyle, endStyle, title, css) {
	logger.debug(`>>> Entering buildToken(builder=${builder},text=${text},style=${style},startStyle=${startStyle},endStyle=${endStyle},title=${title},css=${css})`);
    if (!text) return;
    var displayText = builder.splitSpaces ? text.replace(/ {3,}/g, splitSpaces) : text;
	logger.debug(`buildToken(builder=${builder},text=${text},style=${style},startStyle=${startStyle},endStyle=${endStyle},title=${title},css=${css}): displayText → ${displayText}`);
    var special = builder.cm.state.specialChars, mustWrap = false;
    if (!special.test(text)) {
      builder.col += text.length;
      var content = document.createTextNode(displayText);
      builder.map.push(builder.pos, builder.pos + text.length, content);
      if (ie && ie_version < 9) mustWrap = true;
      builder.pos += text.length;
    } else {
      var content = document.createDocumentFragment(), pos = 0;
	logger.debug(`buildToken(builder=${builder},text=${text},style=${style},startStyle=${startStyle},endStyle=${endStyle},title=${title},css=${css}): content → ${content}`);
      while (true) {
        special.lastIndex = pos;
		logger.debug(`buildToken(builder=${builder},text=${text},style=${style},startStyle=${startStyle},endStyle=${endStyle},title=${title},css=${css}): special.lastIndex → ${special.lastIndex}`);
        var m = special.exec(text);
		logger.debug(`buildToken(builder=${builder},text=${text},style=${style},startStyle=${startStyle},endStyle=${endStyle},title=${title},css=${css}): m → ${m}`);
        var skipped = m ? m.index - pos : text.length - pos;
        if (skipped) {
          var txt = document.createTextNode(displayText.slice(pos, pos + skipped));
          if (ie && ie_version < 9) content.appendChild(elt("span", [txt]));
          else content.appendChild(txt);
          builder.map.push(builder.pos, builder.pos + skipped, txt);
          builder.col += skipped;
          builder.pos += skipped;
        }
        if (!m) break;
        pos += skipped + 1;
        if (m[0] == "\t") {
          var tabSize = builder.cm.options.tabSize, tabWidth = tabSize - builder.col % tabSize;
          var txt = content.appendChild(elt("span", spaceStr(tabWidth), "cm-tab"));
          txt.setAttribute("role", "presentation");
          txt.setAttribute("cm-text", "\t");
          builder.col += tabWidth;
        } else if (m[0] == "\r" || m[0] == "\n") {
          var txt = content.appendChild(elt("span", m[0] == "\r" ? "␍" : "␤", "cm-invalidchar"));
          txt.setAttribute("cm-text", m[0]);
          builder.col += 1;
        } else {
          var txt = builder.cm.options.specialCharPlaceholder(m[0]);
		logger.debug(`buildToken(builder=${builder},text=${text},style=${style},startStyle=${startStyle},endStyle=${endStyle},title=${title},css=${css}): txt → ${txt}`);
          txt.setAttribute("cm-text", m[0]);
          if (ie && ie_version < 9) content.appendChild(elt("span", [txt]));
          else content.appendChild(txt);
          builder.col += 1;
        }
        builder.map.push(builder.pos, builder.pos + 1, txt);
        builder.pos++;
      }
    }
    if (style || startStyle || endStyle || mustWrap || css) {
      var fullStyle = style || "";
	logger.debug(`buildToken(builder=${builder},text=${text},style=${style},startStyle=${startStyle},endStyle=${endStyle},title=${title},css=${css}): fullStyle → ${fullStyle}`);
      if (startStyle) fullStyle += startStyle;
      if (endStyle) fullStyle += endStyle;
      var token = elt("span", [content], fullStyle, css);
      if (title) token.title = title;
      return builder.content.appendChild(token);
    }
    builder.content.appendChild(content);
	logger.debug(`<<< Exiting buildToken(builder=${builder},text=${text},style=${style},startStyle=${startStyle},endStyle=${endStyle},title=${title},css=${css})`);
  }

  function splitSpaces(old) {
	logger.debug(`>>> Entering splitSpaces(old=${old})`);
    var out = " ";
    for (var i = 0; i < old.length - 2; ++i) out += i % 2 ? " " : "\u00a0";
	logger.debug(`splitSpaces(old=${old}): out → ${out}`);
    out += " ";
	logger.debug(`<<< Exiting splitSpaces(old=${old})`);
    return out;
  }

  // Work around nonsense dimensions being reported for stretches of
  // right-to-left text.
  function buildTokenBadBidi(inner, order) {
	logger.debug(`>>> Entering buildTokenBadBidi(inner=${inner},order=${order})`);
    return function(builder, text, style, startStyle, endStyle, title, css) {
      style = style ? style + " cm-force-border" : "cm-force-border";
      var start = builder.pos, end = start + text.length;
      for (;;) {
        // Find the part that overlaps with the start of this text
        for (var i = 0; i < order.length; i++) {
          var part = order[i];
          if (part.to > start && part.from <= start) break;
        }
        if (part.to >= end) return inner(builder, text, style, startStyle, endStyle, title, css);
        inner(builder, text.slice(0, part.to - start), style, startStyle, null, title, css);
        startStyle = null;
        text = text.slice(part.to - start);
        start = part.to;
      }
    };
	logger.debug(`<<< Exiting buildTokenBadBidi(inner=${inner},order=${order})`);
  }

  function buildCollapsedSpan(builder, size, marker, ignoreWidget) {
	logger.debug(`>>> Entering buildCollapsedSpan(builder=${builder},size=${size},marker=${marker},ignoreWidget=${ignoreWidget})`);
    var widget = !ignoreWidget && marker.widgetNode;
    if (widget) builder.map.push(builder.pos, builder.pos + size, widget);
	logger.debug(`buildCollapsedSpan(builder=${builder},size=${size},marker=${marker},ignoreWidget=${ignoreWidget}): widget → ${widget}`);
    if (!ignoreWidget && builder.cm.display.input.needsContentAttribute) {
      if (!widget)
        widget = builder.content.appendChild(document.createElement("span"));
      widget.setAttribute("cm-marker", marker.id);
    }
    if (widget) {
      builder.cm.display.input.setUneditable(widget);
      builder.content.appendChild(widget);
    }
    builder.pos += size;
	logger.debug(`<<< Exiting buildCollapsedSpan(builder=${builder},size=${size},marker=${marker},ignoreWidget=${ignoreWidget})`);
  }

  // Outputs a number of spans to make up a line, taking highlighting
  // and marked text into account.
  function insertLineContent(line, builder, styles) {
	logger.debug(`>>> Entering insertLineContent(line=${line},builder=${builder},styles=${styles})`);
    var spans = line.markedSpans, allText = line.text, at = 0;
    if (!spans) {
      for (var i = 1; i < styles.length; i+=2)
        builder.addToken(builder, allText.slice(at, at = styles[i]), interpretTokenStyle(styles[i+1], builder.cm.options));
      return;
    }

    var len = allText.length, pos = 0, i = 1, text = "", style, css;
	logger.debug(`insertLineContent(line=${line},builder=${builder},styles=${styles}): len → ${len}`);
    var nextChange = 0, spanStyle, spanEndStyle, spanStartStyle, title, collapsed;
    for (;;) {
      if (nextChange == pos) { // Update current marker set
        spanStyle = spanEndStyle = spanStartStyle = title = css = "";
        collapsed = null; nextChange = Infinity;
        var foundBookmarks = [];
        for (var j = 0; j < spans.length; ++j) {
          var sp = spans[j], m = sp.marker;
          if (m.type == "bookmark" && sp.from == pos && m.widgetNode) {
            foundBookmarks.push(m);
          } else if (sp.from <= pos && (sp.to == null || sp.to > pos || m.collapsed && sp.to == pos && sp.from == pos)) {
            if (sp.to != null && sp.to != pos && nextChange > sp.to) {
              nextChange = sp.to;
              spanEndStyle = "";
            }
            if (m.className) spanStyle += " " + m.className;
            if (m.css) css = m.css;
            if (m.startStyle && sp.from == pos) spanStartStyle += " " + m.startStyle;
            if (m.endStyle && sp.to == nextChange) spanEndStyle += " " + m.endStyle;
            if (m.title && !title) title = m.title;
            if (m.collapsed && (!collapsed || compareCollapsedMarkers(collapsed.marker, m) < 0))
              collapsed = sp;
          } else if (sp.from > pos && nextChange > sp.from) {
            nextChange = sp.from;
          }
        }
        if (collapsed && (collapsed.from || 0) == pos) {
          buildCollapsedSpan(builder, (collapsed.to == null ? len + 1 : collapsed.to) - pos,
                             collapsed.marker, collapsed.from == null);
          if (collapsed.to == null) return;
          if (collapsed.to == pos) collapsed = false;
        }
        if (!collapsed && foundBookmarks.length) for (var j = 0; j < foundBookmarks.length; ++j)
          buildCollapsedSpan(builder, 0, foundBookmarks[j]);
      }
      if (pos >= len) break;

      var upto = Math.min(len, nextChange);
	logger.debug(`insertLineContent(line=${line},builder=${builder},styles=${styles}): upto → ${upto}`);
      while (true) {
        if (text) {
          var end = pos + text.length;
          if (!collapsed) {
            var tokenText = end > upto ? text.slice(0, upto - pos) : text;
            builder.addToken(builder, tokenText, style ? style + spanStyle : spanStyle,
                             spanStartStyle, pos + tokenText.length == nextChange ? spanEndStyle : "", title, css);
          }
          if (end >= upto) {text = text.slice(upto - pos); pos = upto; break;}
          pos = end;
          spanStartStyle = "";
        }
        text = allText.slice(at, at = styles[i++]);
        style = interpretTokenStyle(styles[i++], builder.cm.options);
		logger.debug(`insertLineContent(line=${line},builder=${builder},styles=${styles}): text → ${text}`);
      }
    }
	logger.debug(`<<< Exiting insertLineContent(line=${line},builder=${builder},styles=${styles})`);
  }

  // DOCUMENT DATA STRUCTURE

  // By default, updates that start and end at the beginning of a line
  // are treated specially, in order to make the association of line
  // widgets and marker elements with the text behave more intuitive.
  function isWholeLineUpdate(doc, change) {
	logger.debug(`>>> Entering isWholeLineUpdate(doc=${doc},change=${change})`);
	logger.debug(`<<< Exiting isWholeLineUpdate(doc=${doc},change=${change})`);
    return change.from.ch == 0 && change.to.ch == 0 && lst(change.text) == "" &&
      (!doc.cm || doc.cm.options.wholeLineUpdateBefore);
  }

  // Perform a change on the document data structure.
  function updateDoc(doc, change, markedSpans, estimateHeight) {
    function spansFor(n) {return markedSpans ? markedSpans[n] : null;}
    function update(line, text, spans) {
      updateLine(line, text, spans, estimateHeight);
      signalLater(line, "change", line, change);
    }
    function linesFor(start, end) {
	logger.debug(`>>> Entering linesFor(start=${start},end=${end})`);
      for (var i = start, result = []; i < end; ++i)
        result.push(new Line(text[i], spansFor(i), estimateHeight));
	logger.debug(`<<< Exiting linesFor(start=${start},end=${end})`);
      return result;
    }

    var from = change.from, to = change.to, text = change.text;
    var firstLine = getLine(doc, from.line), lastLine = getLine(doc, to.line);
    var lastText = lst(text), lastSpans = spansFor(text.length - 1), nlines = to.line - from.line;

    // Adjust the line structure
    if (change.full) {
      doc.insert(0, linesFor(0, text.length));
      doc.remove(text.length, doc.size - text.length);
    } else if (isWholeLineUpdate(doc, change)) {
      // This is a whole-line replace. Treated specially to make
      // sure line objects move the way they are supposed to.
      var added = linesFor(0, text.length - 1);
      update(lastLine, lastLine.text, lastSpans);
      if (nlines) doc.remove(from.line, nlines);
      if (added.length) doc.insert(from.line, added);
    } else if (firstLine == lastLine) {
      if (text.length == 1) {
        update(firstLine, firstLine.text.slice(0, from.ch) + lastText + firstLine.text.slice(to.ch), lastSpans);
      } else {
        var added = linesFor(1, text.length - 1);
        added.push(new Line(lastText + firstLine.text.slice(to.ch), lastSpans, estimateHeight));
        update(firstLine, firstLine.text.slice(0, from.ch) + text[0], spansFor(0));
        doc.insert(from.line + 1, added);
      }
    } else if (text.length == 1) {
      update(firstLine, firstLine.text.slice(0, from.ch) + text[0] + lastLine.text.slice(to.ch), spansFor(0));
      doc.remove(from.line + 1, nlines);
    } else {
      update(firstLine, firstLine.text.slice(0, from.ch) + text[0], spansFor(0));
      update(lastLine, lastText + lastLine.text.slice(to.ch), lastSpans);
      var added = linesFor(1, text.length - 1);
      if (nlines > 1) doc.remove(from.line + 1, nlines - 1);
      doc.insert(from.line + 1, added);
    }

    signalLater(doc, "change", doc, change);
  }

  // The document is represented as a BTree consisting of leaves, with
  // chunk of lines in them, and branches, with up to ten leaves or
  // other branch nodes below them. The top node is always a branch
  // node, and is the document object itself (meaning it has
  // additional methods and properties).
  //
  // All nodes have parent links. The tree is used both to go from
  // line numbers to line objects, and to go from objects to numbers.
  // It also indexes by height, and is used to convert between height
  // and line object, and to find the total height of the document.
  //
  // See also http://marijnhaverbeke.nl/blog/codemirror-line-tree.html

  function LeafChunk(lines) {
	logger.debug(`>>> Entering LeafChunk(lines=${lines})`);
    this.lines = lines;
    this.parent = null;
	logger.debug(`LeafChunk(lines=${lines}): lines → ${lines}`);
	logger.debug(`LeafChunk(lines=${lines}): this.lines → ${this.lines}`);
    for (var i = 0, height = 0; i < lines.length; ++i) {
      lines[i].parent = this;
      height += lines[i].height;
	logger.debug(`LeafChunk(lines=${lines}): this.parent → ${this.parent}`);
    }
    this.height = height;
	logger.debug(`LeafChunk(lines=${lines}): this.height → ${this.height}`);
	logger.debug(`<<< Exiting LeafChunk(lines=${lines})`);
  }

  LeafChunk.prototype = {
    chunkSize: function() { return this.lines.length; },
    // Remove the n lines at offset 'at'.
    removeInner: function(at, n) {
	logger.debug(`>>> Entering removeInner(at=${at},n=${n})`);
      for (var i = at, e = at + n; i < e; ++i) {
        var line = this.lines[i];
        this.height -= line.height;
        cleanUpLine(line);
        signalLater(line, "delete");
      }
      this.lines.splice(at, n);
	logger.debug(`<<< Exiting removeInner(at=${at},n=${n})`);
    },
    // Helper used to collapse a small branch into a single leaf.
    collapse: function(lines) {
	logger.debug(`>>> Entering collapse(lines=${lines})`);
      lines.push.apply(lines, this.lines);
	logger.debug(`<<< Exiting collapse(lines=${lines})`);
    },
    // Insert the given array of lines at offset 'at', count them as
    // having the given height.
    insertInner: function(at, lines, height) {
	logger.debug(`>>> Entering insertInner(at=${at},lines=${lines},height=${height})`);
      this.height += height;
      this.lines = this.lines.slice(0, at).concat(lines).concat(this.lines.slice(at));
      for (var i = 0; i < lines.length; ++i) lines[i].parent = this;
	logger.debug(`insertInner(at=${at},lines=${lines},height=${height}): lines → ${lines}`);
	logger.debug(`<<< Exiting insertInner(at=${at},lines=${lines},height=${height})`);
    },
    // Used to iterate over a part of the tree.
    iterN: function(at, n, op) {
	logger.debug(`>>> Entering iterN(at=${at},n=${n},op=${op})`);
      for (var e = at + n; at < e; ++at)
		logger.debug(`<<< Exiting iterN(at=${at},n=${n},op=${op})`);
        if (op(this.lines[at])) return true;
		logger.debug(`iterN(at=${at},n=${n},op=${op}): used → ${used}`);
    }
  };

  function BranchChunk(children) {
	logger.debug(`>>> Entering BranchChunk(children=${children})`);
    this.children = children;
    var size = 0, height = 0;
	logger.debug(`BranchChunk(children=${children}): children → ${children}`);
	logger.debug(`BranchChunk(children=${children}): this.children → ${this.children}`);
	logger.debug(`BranchChunk(children=${children}): size → ${size}`);
    for (var i = 0; i < children.length; ++i) {
      var ch = children[i];
      size += ch.chunkSize(); height += ch.height;
      ch.parent = this;
	logger.debug(`BranchChunk(children=${children}): ch.parent → ${ch.parent}`);
    }
    this.size = size;
    this.height = height;
	logger.debug(`BranchChunk(children=${children}): this.size → ${this.size}`);
	logger.debug(`BranchChunk(children=${children}): this.height → ${this.height}`);
	logger.debug(`BranchChunk(children=${children}): height → ${height}`);
    this.parent = null;
	logger.debug(`BranchChunk(children=${children}): this.parent → ${this.parent}`);
	logger.debug(`<<< Exiting BranchChunk(children=${children})`);
  }

  BranchChunk.prototype = {
    chunkSize: function() { return this.size; },
    removeInner: function(at, n) {
      this.size -= n;
      for (var i = 0; i < this.children.length; ++i) {
        var child = this.children[i], sz = child.chunkSize();
		logger.debug(`removeInner(at=${at},n=${n}): child → ${child}`);
        if (at < sz) {
          var rm = Math.min(n, sz - at), oldHeight = child.height;
          child.removeInner(at, rm);
          this.height -= oldHeight - child.height;
          if (sz == rm) { this.children.splice(i--, 1); child.parent = null; }
          if ((n -= rm) == 0) break;
          at = 0;
        } else at -= sz;
      }
      // If the result is smaller than 25 lines, ensure that it is a
      // single leaf node.
      if (this.size - n < 25 &&
          (this.children.length > 1 || !(this.children[0] instanceof LeafChunk))) {
        var lines = [];
        this.collapse(lines);
        this.children = [new LeafChunk(lines)];
        this.children[0].parent = this;
		logger.debug(`removeInner(at=${at},n=${n}): children → ${children}`);
      }
    },
    collapse: function(lines) {
      for (var i = 0; i < this.children.length; ++i) this.children[i].collapse(lines);
    },
    insertInner: function(at, lines, height) {
      this.size += lines.length;
      this.height += height;
      for (var i = 0; i < this.children.length; ++i) {
        var child = this.children[i], sz = child.chunkSize();
        if (at <= sz) {
          child.insertInner(at, lines, height);
          if (child.lines && child.lines.length > 50) {
            while (child.lines.length > 50) {
              var spilled = child.lines.splice(child.lines.length - 25, 25);
			logger.debug(`insertInner(at=${at},lines=${lines},height=${height}): spilled → ${spilled}`);
              var newleaf = new LeafChunk(spilled);
			logger.debug(`insertInner(at=${at},lines=${lines},height=${height}): newleaf → ${newleaf}`);
              child.height -= newleaf.height;
              this.children.splice(i + 1, 0, newleaf);
              newleaf.parent = this;
			logger.debug(`insertInner(at=${at},lines=${lines},height=${height}): newleaf.parent → ${newleaf.parent}`);
            }
            this.maybeSpill();
          }
          break;
        }
        at -= sz;
      }
    },
    // When a node has grown, check whether it should be split.
    maybeSpill: function() {
	logger.debug('>>> Entering maybeSpill()');
      if (this.children.length <= 10) return;
      var me = this;
      do {
        var spilled = me.children.splice(me.children.length - 5, 5);
		logger.debug(`maybeSpill(): spilled → ${spilled}`);
        var sibling = new BranchChunk(spilled);
		logger.debug(`maybeSpill(): sibling → ${sibling}`);
        if (!me.parent) { // Become the parent node
          var copy = new BranchChunk(me.children);
          copy.parent = me;
          me.children = [copy, sibling];
		logger.debug(`maybeSpill(): copy.parent → ${copy.parent}`);
		logger.debug(`maybeSpill(): me.children → ${me.children}`);
          me = copy;
        } else {
          me.size -= sibling.size;
          me.height -= sibling.height;
          var myIndex = indexOf(me.parent.children, me);
		logger.debug(`maybeSpill(): myIndex → ${myIndex}`);
          me.parent.children.splice(myIndex + 1, 0, sibling);
		logger.debug(`>>> Entering copy(copyHistory=${copyHistory})`);
        }
        sibling.parent = me.parent;
		logger.debug(`maybeSpill(): sibling.parent → ${sibling.parent}`);
      } while (me.children.length > 10);
      me.parent.maybeSpill();
	logger.debug('<<< Exiting maybeSpill()');
    },
    iterN: function(at, n, op) {
      for (var i = 0; i < this.children.length; ++i) {
        var child = this.children[i], sz = child.chunkSize();
        if (at < sz) {
          var used = Math.min(n, sz - at);
          if (child.iterN(at, used, op)) return true;
          if ((n -= used) == 0) break;
          at = 0;
        } else at -= sz;
      }
    }
  };

  var nextDocId = 0;
  var Doc = CodeMirror.Doc = function(text, mode, firstLine, lineSep) {
    if (!(this instanceof Doc)) return new Doc(text, mode, firstLine, lineSep);
    if (firstLine == null) firstLine = 0;

    BranchChunk.call(this, [new LeafChunk([new Line("", null)])]);
    this.first = firstLine;
    this.scrollTop = this.scrollLeft = 0;
    this.cantEdit = false;
    this.cleanGeneration = 1;
    this.frontier = firstLine;
    var start = Pos(firstLine, 0);
    this.sel = simpleSelection(start);
    this.history = new History(null);
    this.id = ++nextDocId;
    this.modeOption = mode;
    this.lineSep = lineSep;

    if (typeof text == "string") text = this.splitLines(text);
    updateDoc(this, {from: start, to: start, text: text});
    setSelection(this, simpleSelection(start), sel_dontScroll);
  };

  Doc.prototype = createObj(BranchChunk.prototype, {
    constructor: Doc,
    // Iterate over the document. Supports two forms -- with only one
    // argument, it calls that for each line in the document. With
    // three, it iterates over the range given by the first two (with
    // the second being non-inclusive).
    iter: function(from, to, op) {
      if (op) this.iterN(from - this.first, to - from, op);
      else this.iterN(this.first, this.first + this.size, from);
	logger.debug(`iter(): doc.sel → ${doc.sel}`);
	logger.debug(`iter(): doc.extend → ${doc.extend}`);
	logger.debug(`iter(): doc.scrollTop → ${doc.scrollTop}`);
    },

    // Non-public interface for adding and removing lines.
    insert: function(at, lines) {
	logger.debug(`>>> Entering insert(at=${at},lines=${lines})`);
      var height = 0;
      for (var i = 0; i < lines.length; ++i) height += lines[i].height;
      this.insertInner(at - this.first, lines, height);
	logger.debug(`<<< Exiting insert(at=${at},lines=${lines})`);
    },
    remove: function(at, n) { this.removeInner(at - this.first, n); },

    // From here, the methods are part of the public interface. Most
    // are also available from CodeMirror (editor) instances.

    getValue: function(lineSep) {
	logger.debug(`>>> Entering getValue(lineSep=${lineSep})`);
      var lines = getLines(this, this.first, this.first + this.size);
      if (lineSep === false) return lines;
	logger.debug(`getValue(lineSep=${lineSep}): lines → ${lines}`);
	logger.debug(`<<< Exiting getValue(lineSep=${lineSep})`);
      return lines.join(lineSep || this.lineSeparator());
    },
    setValue: docMethodOp(function(code) {
      var top = Pos(this.first, 0), last = this.first + this.size - 1;
      makeChange(this, {from: top, to: Pos(last, getLine(this, last).text.length),
                        text: this.splitLines(code), origin: "setValue", full: true}, true);
      setSelection(this, simpleSelection(top));
    }),
    replaceRange: function(code, from, to, origin) {
      from = clipPos(this, from);
      to = to ? clipPos(this, to) : from;
      replaceRange(this, code, from, to, origin);
    },
    getRange: function(from, to, lineSep) {
	logger.debug(`>>> Entering getRange(from=${from},to=${to},lineSep=${lineSep})`);
      var lines = getBetween(this, clipPos(this, from), clipPos(this, to));
      if (lineSep === false) return lines;
	logger.debug(`getRange(from=${from},to=${to},lineSep=${lineSep}): lines → ${lines}`);
	logger.debug(`<<< Exiting getRange(from=${from},to=${to},lineSep=${lineSep})`);
      return lines.join(lineSep || this.lineSeparator());
    },

    getLine: function(line) {var l = this.getLineHandle(line); return l && l.text;},

    getLineHandle: function(line) {if (isLine(this, line)) return getLine(this, line);},
    getLineNumber: function(line) {return lineNo(line);},

    getLineHandleVisualStart: function(line) {
	logger.debug(`>>> Entering getLineHandleVisualStart(line=${line})`);
      if (typeof line == "number") line = getLine(this, line);
	logger.debug(`<<< Exiting getLineHandleVisualStart(line=${line})`);
      return visualLine(line);
    },

    lineCount: function() {return this.size;},
    firstLine: function() {return this.first;},
    lastLine: function() {return this.first + this.size - 1;},

    clipPos: function(pos) {return clipPos(this, pos);},

    getCursor: function(start) {
	logger.debug(`>>> Entering getCursor(start=${start})`);
      var range = this.sel.primary(), pos;
      if (start == null || start == "head") pos = range.head;
      else if (start == "anchor") pos = range.anchor;
      else if (start == "end" || start == "to" || start === false) pos = range.to();
      else pos = range.from();
	logger.debug(`<<< Exiting getCursor(start=${start})`);
	logger.debug(`getCursor(start=${start}): range → ${range}`);
      return pos;
    },
    listSelections: function() { return this.sel.ranges; },
    somethingSelected: function() {return this.sel.somethingSelected();},

    setCursor: docMethodOp(function(line, ch, options) {
      setSimpleSelection(this, clipPos(this, typeof line == "number" ? Pos(line, ch || 0) : line), null, options);
    }),
    setSelection: docMethodOp(function(anchor, head, options) {
      setSimpleSelection(this, clipPos(this, anchor), clipPos(this, head || anchor), options);
    }),
    extendSelection: docMethodOp(function(head, other, options) {
      extendSelection(this, clipPos(this, head), other && clipPos(this, other), options);
    }),
    extendSelections: docMethodOp(function(heads, options) {
      extendSelections(this, clipPosArray(this, heads, options));
    }),
    extendSelectionsBy: docMethodOp(function(f, options) {
      extendSelections(this, map(this.sel.ranges, f), options);
    }),
    setSelections: docMethodOp(function(ranges, primary, options) {
      if (!ranges.length) return;
      for (var i = 0, out = []; i < ranges.length; i++)
        out[i] = new Range(clipPos(this, ranges[i].anchor),
                           clipPos(this, ranges[i].head));
      if (primary == null) primary = Math.min(ranges.length - 1, this.sel.primIndex);
      setSelection(this, normalizeSelection(out, primary), options);
    }),
    addSelection: docMethodOp(function(anchor, head, options) {
      var ranges = this.sel.ranges.slice(0);
      ranges.push(new Range(clipPos(this, anchor), clipPos(this, head || anchor)));
      setSelection(this, normalizeSelection(ranges, ranges.length - 1), options);
    }),

    getSelection: function(lineSep) {
	logger.debug(`>>> Entering getSelection(lineSep=${lineSep})`);
      var ranges = this.sel.ranges, lines;
      for (var i = 0; i < ranges.length; i++) {
        var sel = getBetween(this, ranges[i].from(), ranges[i].to());
        lines = lines ? lines.concat(sel) : sel;
		logger.debug(`getSelection(lineSep=${lineSep}): sel → ${sel}`);
      }
      if (lineSep === false) return lines;
      else return lines.join(lineSep || this.lineSeparator());
	logger.debug(`<<< Exiting getSelection(lineSep=${lineSep})`);
    },
    getSelections: function(lineSep) {
	logger.debug(`>>> Entering getSelections(lineSep=${lineSep})`);
      var parts = [], ranges = this.sel.ranges;
      for (var i = 0; i < ranges.length; i++) {
        var sel = getBetween(this, ranges[i].from(), ranges[i].to());
        if (lineSep !== false) sel = sel.join(lineSep || this.lineSeparator());
        parts[i] = sel;
		logger.debug(`getSelections(lineSep=${lineSep}): sel → ${sel}`);
      }
	logger.debug(`<<< Exiting getSelections(lineSep=${lineSep})`);
      return parts;
    },
    replaceSelection: function(code, collapse, origin) {
	logger.debug(`>>> Entering replaceSelection(code=${code},collapse=${collapse},origin=${origin})`);
      var dup = [];
      for (var i = 0; i < this.sel.ranges.length; i++)
        dup[i] = code;
      this.replaceSelections(dup, collapse, origin || "+input");
	logger.debug(`<<< Exiting replaceSelection(code=${code},collapse=${collapse},origin=${origin})`);
    },
    replaceSelections: docMethodOp(function(code, collapse, origin) {
      var changes = [], sel = this.sel;
      for (var i = 0; i < sel.ranges.length; i++) {
        var range = sel.ranges[i];
        changes[i] = {from: range.from(), to: range.to(), text: this.splitLines(code[i]), origin: origin};
      }
      var newSel = collapse && collapse != "end" && computeReplacedSel(this, changes, collapse);
	logger.debug(`docMethodOp(changes=${changes}): newSel → ${newSel}`);
      for (var i = changes.length - 1; i >= 0; i--)
        makeChange(this, changes[i]);
      if (newSel) setSelectionReplaceHistory(this, newSel);
      else if (this.cm) ensureCursorVisible(this.cm);
    }),
    undo: docMethodOp(function() {makeChangeFromHistory(this, "undo");}),
    redo: docMethodOp(function() {makeChangeFromHistory(this, "redo");}),
    undoSelection: docMethodOp(function() {makeChangeFromHistory(this, "undo", true);}),
    redoSelection: docMethodOp(function() {makeChangeFromHistory(this, "redo", true);}),

    setExtending: function(val) {this.extend = val;},
    getExtending: function() {return this.extend;},

    historySize: function() {
	logger.debug('>>> Entering historySize()');
      var hist = this.history, done = 0, undone = 0;
      for (var i = 0; i < hist.done.length; i++) if (!hist.done[i].ranges) ++done;
      for (var i = 0; i < hist.undone.length; i++) if (!hist.undone[i].ranges) ++undone;
	logger.debug('<<< Exiting historySize()');
      return {undo: done, redo: undone};
    },
    clearHistory: function() {this.history = new History(this.history.maxGeneration);},

    markClean: function() {
	logger.debug('>>> Entering markClean()');
      this.cleanGeneration = this.changeGeneration(true);
	logger.debug('<<< Exiting markClean()');
    },
    changeGeneration: function(forceSplit) {
	logger.debug(`>>> Entering changeGeneration(forceSplit=${forceSplit})`);
      if (forceSplit)
        this.history.lastOp = this.history.lastSelOp = this.history.lastOrigin = null;
	logger.debug(`<<< Exiting changeGeneration(forceSplit=${forceSplit})`);
      return this.history.generation;
    },
    isClean: function (gen) {
	logger.debug(`>>> Entering isClean(gen=${gen})`);
	logger.debug(`<<< Exiting isClean(gen=${gen})`);
      return this.history.generation == (gen || this.cleanGeneration);
    },

    getHistory: function() {
	logger.debug('>>> Entering getHistory()');
      return {done: copyHistoryArray(this.history.done),
              undone: copyHistoryArray(this.history.undone)};
			logger.debug('<<< Exiting getHistory()');
    },
    setHistory: function(histData) {
	logger.debug(`>>> Entering setHistory(histData=${histData})`);
      var hist = this.history = new History(this.history.maxGeneration);
      hist.done = copyHistoryArray(histData.done.slice(0), null, true);
      hist.undone = copyHistoryArray(histData.undone.slice(0), null, true);
	logger.debug(`<<< Exiting setHistory(histData=${histData})`);
    },

    addLineClass: docMethodOp(function(handle, where, cls) {
      return changeLine(this, handle, where == "gutter" ? "gutter" : "class", function(line) {
        var prop = where == "text" ? "textClass"
                 : where == "background" ? "bgClass"
                 : where == "gutter" ? "gutterClass" : "wrapClass";
        if (!line[prop]) line[prop] = cls;
        else if (classTest(cls).test(line[prop])) return false;
        else line[prop] += " " + cls;
        return true;
      });
    }),
    removeLineClass: docMethodOp(function(handle, where, cls) {
      return changeLine(this, handle, where == "gutter" ? "gutter" : "class", function(line) {
        var prop = where == "text" ? "textClass"
                 : where == "background" ? "bgClass"
                 : where == "gutter" ? "gutterClass" : "wrapClass";
        var cur = line[prop];
        if (!cur) return false;
        else if (cls == null) line[prop] = null;
        else {
          var found = cur.match(classTest(cls));
          if (!found) return false;
          var end = found.index + found[0].length;
          line[prop] = cur.slice(0, found.index) + (!found.index || end == cur.length ? "" : " ") + cur.slice(end) || null;
        }
        return true;
      });
    }),

    addLineWidget: docMethodOp(function(handle, node, options) {
      return addLineWidget(this, handle, node, options);
    }),
    removeLineWidget: function(widget) { widget.clear(); },

    markText: function(from, to, options) {
      return markText(this, clipPos(this, from), clipPos(this, to), options, "range");
    },
    setBookmark: function(pos, options) {
	logger.debug(`>>> Entering setBookmark(pos=${pos},options=${options})`);
      var realOpts = {replacedWith: options && (options.nodeType == null ? options.widget : options),
                      insertLeft: options && options.insertLeft,
                      clearWhenEmpty: false, shared: options && options.shared,
                      handleMouseEvents: options && options.handleMouseEvents};
      pos = clipPos(this, pos);
	logger.debug(`setBookmark(pos=${pos},options=${options}): realOpts → ${realOpts}`);
	logger.debug(`setBookmark(pos=${pos},options=${options}): pos → ${pos}`);
	logger.debug(`<<< Exiting setBookmark(pos=${pos},options=${options})`);
      return markText(this, pos, pos, realOpts, "bookmark");
    },
    findMarksAt: function(pos) {
	logger.debug(`>>> Entering findMarksAt(pos=${pos})`);
      pos = clipPos(this, pos);
      var markers = [], spans = getLine(this, pos.line).markedSpans;
	logger.debug(`findMarksAt(pos=${pos}): pos → ${pos}`);
	logger.debug(`findMarksAt(pos=${pos}): markers → ${markers}`);
      if (spans) for (var i = 0; i < spans.length; ++i) {
        var span = spans[i];
        if ((span.from == null || span.from <= pos.ch) &&
            (span.to == null || span.to >= pos.ch))
          markers.push(span.marker.parent || span.marker);
		logger.debug(`<<< Exiting findMarksAt(pos=${pos})`);
      }
      return markers;
    },
    findMarks: function(from, to, filter) {
      from = clipPos(this, from); to = clipPos(this, to);
      var found = [], lineNo = from.line;
      this.iter(from.line, to.line + 1, function(line) {
        var spans = line.markedSpans;
        if (spans) for (var i = 0; i < spans.length; i++) {
          var span = spans[i];
          if (!(lineNo == from.line && from.ch > span.to ||
                span.from == null && lineNo != from.line||
                lineNo == to.line && span.from > to.ch) &&
              (!filter || filter(span.marker)))
            found.push(span.marker.parent || span.marker);
        }
        ++lineNo;
      });
      return found;
    },
    getAllMarks: function() {
	logger.debug('>>> Entering getAllMarks()');
      var markers = [];
      this.iter(function(line) {
        var sps = line.markedSpans;
        if (sps) for (var i = 0; i < sps.length; ++i)
          if (sps[i].from != null) markers.push(sps[i].marker);
      });
	logger.debug('<<< Exiting getAllMarks()');
      return markers;
    },

    posFromIndex: function(off) {
      var ch, lineNo = this.first;
      this.iter(function(line) {
        var sz = line.text.length + 1;
        if (sz > off) { ch = off; return true; }
        off -= sz;
        ++lineNo;
      });
	logger.debug(`<<< Exiting posFromIndex(off=${off})`);
      return clipPos(this, Pos(lineNo, ch));
    },
    indexFromPos: function (coords) {
	logger.debug(`>>> Entering indexFromPos(coords=${coords})`);
      coords = clipPos(this, coords);
      var index = coords.ch;
      if (coords.line < this.first || coords.ch < 0) return 0;
      this.iter(this.first, coords.line, function (line) {
        index += line.text.length + 1;
      });
	logger.debug(`<<< Exiting indexFromPos(coords=${coords})`);
      return index;
    },

    copy: function(copyHistory) {
	logger.debug(`>>> Entering copy(copyHistory=${copyHistory})`);
      var doc = new Doc(getLines(this, this.first, this.first + this.size),
                        this.modeOption, this.first, this.lineSep);
      doc.scrollTop = this.scrollTop; doc.scrollLeft = this.scrollLeft;
	logger.debug(`copy(copyHistory=${copyHistory}): doc → ${doc}`);
      doc.sel = this.sel;
      doc.extend = false;
      if (copyHistory) {
        doc.history.undoDepth = this.history.undoDepth;
        doc.setHistory(this.getHistory());
      }
	logger.debug(`copy(copyHistory=${copyHistory}): doc.scrollTop → ${doc.scrollTop}`);
	logger.debug(`<<< Exiting copy(copyHistory=${copyHistory})`);
      return doc;
    },

    linkedDoc: function(options) {
	logger.debug(`>>> Entering linkedDoc(options=${options})`);
      if (!options) options = {};
      var from = this.first, to = this.first + this.size;
      if (options.from != null && options.from > from) from = options.from;
      if (options.to != null && options.to < to) to = options.to;
      var copy = new Doc(getLines(this, from, to), options.mode || this.modeOption, from, this.lineSep);
      if (options.sharedHist) copy.history = this.history;
      (this.linked || (this.linked = [])).push({doc: copy, sharedHist: options.sharedHist});
      copy.linked = [{doc: this, isParent: true, sharedHist: options.sharedHist}];
      copySharedMarkers(copy, findSharedMarkers(this));
	logger.debug(`linkedDoc(options=${options}): copy.linked → ${copy.linked}`);
	logger.debug(`<<< Exiting linkedDoc(options=${options})`);
      return copy;
    },
    unlinkDoc: function(other) {
	logger.debug(`>>> Entering unlinkDoc(other=${other})`);
      if (other instanceof CodeMirror) other = other.doc;
      if (this.linked) for (var i = 0; i < this.linked.length; ++i) {
        var link = this.linked[i];
        if (link.doc != other) continue;
        this.linked.splice(i, 1);
        other.unlinkDoc(this);
        detachSharedMarkers(findSharedMarkers(this));
        break;
      }
      // If the histories were shared, split them again
      if (other.history == this.history) {
        var splitIds = [other.id];
        linkedDocs(other, function(doc) {splitIds.push(doc.id);}, true);
        other.history = new History(null);
        other.history.done = copyHistoryArray(this.history.done, splitIds);
        other.history.undone = copyHistoryArray(this.history.undone, splitIds);
      }
	logger.debug(`<<< Exiting unlinkDoc(other=${other})`);
    },
    iterLinkedDocs: function(f) {linkedDocs(this, f);},

    getMode: function() {return this.mode;},
    getEditor: function() {return this.cm;},

    splitLines: function(str) {
	logger.debug(`>>> Entering splitLines(str=${str})`);
      if (this.lineSep) return str.split(this.lineSep);
	logger.debug(`<<< Exiting splitLines(str=${str})`);
      return splitLinesAuto(str);
    },
    lineSeparator: function() { return this.lineSep || "\n"; }
  });

  // Public alias.
  Doc.prototype.eachLine = Doc.prototype.iter;

  // Set up methods on CodeMirror's prototype to redirect to the editor's document.
  var dontDelegate = "iter insert remove copy getEditor constructor".split(" ");
  for (var prop in Doc.prototype) if (Doc.prototype.hasOwnProperty(prop) && indexOf(dontDelegate, prop) < 0)
    CodeMirror.prototype[prop] = (function(method) {
      return function() {return method.apply(this.doc, arguments);};
    })(Doc.prototype[prop]);

  eventMixin(Doc);

  // Call f for all linked documents.
  function linkedDocs(doc, f, sharedHistOnly) {
    function propagate(doc, skip, sharedHist) {
	logger.debug(`>>> Entering propagate(doc=${doc},skip=${skip},sharedHist=${sharedHist})`);
      if (doc.linked) for (var i = 0; i < doc.linked.length; ++i) {
        var rel = doc.linked[i];
        if (rel.doc == skip) continue;
        var shared = sharedHist && rel.sharedHist;
        if (sharedHistOnly && !shared) continue;
        f(rel.doc, shared);
        propagate(rel.doc, doc, shared);
		logger.debug(`<<< Exiting propagate(doc=${doc},skip=${skip},sharedHist=${sharedHist})`);
      }
    }
    propagate(doc, null, true);
  }

  // Attach a document to an editor.
  function attachDoc(cm, doc) {
	logger.debug(`>>> Entering attachDoc(cm=${cm},doc=${doc})`);
    if (doc.cm) throw new Error("This document is already in use.");
    cm.doc = doc;
	logger.debug(`attachDoc(cm=${cm},doc=${doc}): cm.doc → ${cm.doc}`);
    doc.cm = cm;
	logger.debug(`attachDoc(cm=${cm},doc=${doc}): doc.cm → ${doc.cm}`);
    estimateLineHeights(cm);
    loadMode(cm);
    if (!cm.options.lineWrapping) findMaxLine(cm);
    cm.options.mode = doc.modeOption;
    regChange(cm);
	logger.debug(`<<< Exiting attachDoc(cm=${cm},doc=${doc})`);
  }

  // LINE UTILITIES

  // Find the line object corresponding to the given line number.
  function getLine(doc, n) {
	logger.debug(`>>> Entering getLine(doc=${doc},n=${n})`);
    n -= doc.first;
    if (n < 0 || n >= doc.size) throw new Error("There is no line " + (n + doc.first) + " in the document.");
    for (var chunk = doc; !chunk.lines;) {
      for (var i = 0;; ++i) {
        var child = chunk.children[i], sz = child.chunkSize();
		logger.debug(`getLine(doc=${doc},n=${n}): child → ${child}`);
        if (n < sz) { chunk = child; break; }
        n -= sz;
      }
    }
	logger.debug(`<<< Exiting getLine(doc=${doc},n=${n})`);
    return chunk.lines[n];
  }

  // Get the part of a document between two positions, as an array of
  // strings.
  function getBetween(doc, start, end) {
	logger.debug(`>>> Entering getBetween(doc=${doc},start=${start},end=${end})`);
    var out = [], n = start.line;
    doc.iter(start.line, end.line + 1, function(line) {
      var text = line.text;
      if (n == end.line) text = text.slice(0, end.ch);
      if (n == start.line) text = text.slice(start.ch);
      out.push(text);
      ++n;
    });
	logger.debug(`<<< Exiting getBetween(doc=${doc},start=${start},end=${end})`);
    return out;
  }
  // Get the lines between from and to, as array of strings.
  function getLines(doc, from, to) {
	logger.debug(`>>> Entering getLines(doc=${doc},from=${from},to=${to})`);
    var out = [];
    doc.iter(from, to, function(line) { out.push(line.text); });
	logger.debug(`<<< Exiting getLines(doc=${doc},from=${from},to=${to})`);
    return out;
  }

  // Update the height of a line, propagating the height change
  // upwards to parent nodes.
  function updateLineHeight(line, height) {
	logger.debug(`>>> Entering updateLineHeight(line=${line},height=${height})`);
    var diff = height - line.height;
    if (diff) for (var n = line; n; n = n.parent) n.height += diff;
	logger.debug(`<<< Exiting updateLineHeight(line=${line},height=${height})`);
  }

  // Given a line object, find its line number by walking up through
  // its parent links.
  function lineNo(line) {
    if (line.parent == null) return null;
    var cur = line.parent, no = indexOf(cur.lines, line);
    for (var chunk = cur.parent; chunk; cur = chunk, chunk = chunk.parent) {
      for (var i = 0;; ++i) {
        if (chunk.children[i] == cur) break;
        no += chunk.children[i].chunkSize();
      }
    }
    return no + cur.first;
  }

  // Find the line at the given vertical position, using the height
  // information in the document tree.
  function lineAtHeight(chunk, h) {
    var n = chunk.first;
    outer: do {
      for (var i = 0; i < chunk.children.length; ++i) {
        var child = chunk.children[i], ch = child.height;
        if (h < ch) { chunk = child; continue outer; }
        h -= ch;
        n += child.chunkSize();
      }
      return n;
    } while (!chunk.lines);
    for (var i = 0; i < chunk.lines.length; ++i) {
      var line = chunk.lines[i], lh = line.height;
      if (h < lh) break;
      h -= lh;
    }
    return n + i;
  }


  // Find the height above the given line.
  function heightAtLine(lineObj) {
    lineObj = visualLine(lineObj);

    var h = 0, chunk = lineObj.parent;
    for (var i = 0; i < chunk.lines.length; ++i) {
      var line = chunk.lines[i];
      if (line == lineObj) break;
      else h += line.height;
    }
    for (var p = chunk.parent; p; chunk = p, p = chunk.parent) {
      for (var i = 0; i < p.children.length; ++i) {
        var cur = p.children[i];
        if (cur == chunk) break;
        else h += cur.height;
      }
    }
	logger.debug(`<<< Exiting heightAtLine(lineObj=${lineObj})`);
    return h;
  }

  // Get the bidi ordering for the given line (and cache it). Returns
  // false for lines that are fully left-to-right, and an array of
  // BidiSpan objects otherwise.
  function getOrder(line) {
    var order = line.order;
    if (order == null) order = line.order = bidiOrdering(line.text);
    return order;
  }

  // HISTORY

  function History(startGen) {
	logger.debug(`>>> Entering History(startGen=${startGen})`);
    // Arrays of change events and selections. Doing something adds an
    // event to done and clears undo. Undoing moves events from done
    // to undone, redoing moves them in the other direction.
    this.done = []; this.undone = [];
    this.undoDepth = Infinity;
	logger.debug(`History(startGen=${startGen}): this.done → ${this.done}`);
    // Used to track when changes can be merged into a single undo
    // event
    this.lastModTime = this.lastSelTime = 0;
	logger.debug(`History(startGen=${startGen}): this.undoDepth → ${this.undoDepth}`);
	logger.debug(`History(startGen=${startGen}): this.lastModTime → ${this.lastModTime}`);
    this.lastOp = this.lastSelOp = null;
    this.lastOrigin = this.lastSelOrigin = null;
    // Used by the isClean() method
    this.generation = this.maxGeneration = startGen || 1;
	logger.debug(`History(startGen=${startGen}): this.lastOp → ${this.lastOp}`);
	logger.debug(`History(startGen=${startGen}): this.generation → ${this.generation}`);
	logger.debug(`<<< Exiting History(startGen=${startGen})`);
  }

  // Create a history change event from an updateDoc-style change
  // object.
  function historyChangeFromChange(doc, change) {
	logger.debug(`>>> Entering historyChangeFromChange(doc=${doc},change=${change})`);
    var histChange = {from: copyPos(change.from), to: changeEnd(change), text: getBetween(doc, change.from, change.to)};
    attachLocalSpans(doc, histChange, change.from.line, change.to.line + 1);
	logger.debug(`historyChangeFromChange(doc=${doc},change=${change}): histChange → ${histChange}`);
    linkedDocs(doc, function(doc) {attachLocalSpans(doc, histChange, change.from.line, change.to.line + 1);}, true);
	logger.debug(`<<< Exiting historyChangeFromChange(doc=${doc},change=${change})`);
    return histChange;
  }

  // Pop all selection events off the end of a history array. Stop at
  // a change event.
  function clearSelectionEvents(array) {
	logger.debug(`>>> Entering clearSelectionEvents(array=${array})`);
    while (array.length) {
      var last = lst(array);
	logger.debug(`clearSelectionEvents(array=${array}): last → ${last}`);
      if (last.ranges) array.pop();
      else break;
	logger.debug(`<<< Exiting clearSelectionEvents(array=${array})`);
    }
  }

  // Find the top change event in the history. Pop off selection
  // events that are in the way.
  function lastChangeEvent(hist, force) {
	logger.debug(`>>> Entering lastChangeEvent(hist=${hist},force=${force})`);
    if (force) {
      clearSelectionEvents(hist.done);
      return lst(hist.done);
    } else if (hist.done.length && !lst(hist.done).ranges) {
      return lst(hist.done);
    } else if (hist.done.length > 1 && !hist.done[hist.done.length - 2].ranges) {
      hist.done.pop();
	logger.debug(`<<< Exiting lastChangeEvent(hist=${hist},force=${force})`);
      return lst(hist.done);
    }
  }

  // Register a change in the history. Merges changes that are within
  // a single operation, ore are close together with an origin that
  // allows merging (starting with "+") into a single event.
  function addChangeToHistory(doc, change, selAfter, opId) {
	logger.debug(`>>> Entering addChangeToHistory(doc=${doc},change=${change},selAfter=${selAfter},opId=${opId})`);
    var hist = doc.history;
    hist.undone.length = 0;
    var time = +new Date, cur;

    if ((hist.lastOp == opId ||
         hist.lastOrigin == change.origin && change.origin &&
         ((change.origin.charAt(0) == "+" && doc.cm && hist.lastModTime > time - doc.cm.options.historyEventDelay) ||
          change.origin.charAt(0) == "*")) &&
        (cur = lastChangeEvent(hist, hist.lastOp == opId))) {
      // Merge this change into the last event
      var last = lst(cur.changes);
	logger.debug(`addChangeToHistory(doc=${doc},change=${change},selAfter=${selAfter},opId=${opId}): last → ${last}`);
      if (cmp(change.from, change.to) == 0 && cmp(change.from, last.to) == 0) {
        // Optimized case for simple insertion -- don't want to add
        // new changesets for every character typed
        last.to = changeEnd(change);
      } else {
        // Add new sub-event
        cur.changes.push(historyChangeFromChange(doc, change));
      }
    } else {
      // Can not be merged, start a new event.
      var before = lst(hist.done);
	logger.debug(`addChangeToHistory(doc=${doc},change=${change},selAfter=${selAfter},opId=${opId}): before → ${before}`);
      if (!before || !before.ranges)
        pushSelectionToHistory(doc.sel, hist.done);
      cur = {changes: [historyChangeFromChange(doc, change)],
             generation: hist.generation};
      hist.done.push(cur);
      while (hist.done.length > hist.undoDepth) {
        hist.done.shift();
        if (!hist.done[0].ranges) hist.done.shift();
      }
    }
    hist.done.push(selAfter);
    hist.generation = ++hist.maxGeneration;
	logger.debug(`addChangeToHistory(doc=${doc},change=${change},selAfter=${selAfter},opId=${opId}): hist.generation → ${hist.generation}`);
	logger.debug(`addChangeToHistory(doc=${doc},change=${change},selAfter=${selAfter},opId=${opId}): hist.lastOp → ${hist.lastOp}`);
	logger.debug(`addChangeToHistory(doc=${doc},change=${change},selAfter=${selAfter},opId=${opId}): hist.lastOrigin → ${hist.lastOrigin}`);
    hist.lastModTime = hist.lastSelTime = time;
	logger.debug(`addChangeToHistory(doc=${doc},change=${change},selAfter=${selAfter},opId=${opId}): hist.lastModTime → ${hist.lastModTime}`);
    hist.lastOp = hist.lastSelOp = opId;
    hist.lastOrigin = hist.lastSelOrigin = change.origin;

    if (!last) signal(doc, "historyAdded");
	logger.debug(`<<< Exiting addChangeToHistory(doc=${doc},change=${change},selAfter=${selAfter},opId=${opId})`);
  }

  function selectionEventCanBeMerged(doc, origin, prev, sel) {
	logger.debug(`>>> Entering selectionEventCanBeMerged(doc=${doc},origin=${origin},prev=${prev},sel=${sel})`);
    var ch = origin.charAt(0);
	logger.debug(`selectionEventCanBeMerged(doc=${doc},origin=${origin},prev=${prev},sel=${sel}): ch → ${ch}`);
	logger.debug(`<<< Exiting selectionEventCanBeMerged(doc=${doc},origin=${origin},prev=${prev},sel=${sel})`);
    return ch == "*" ||
      ch == "+" &&
      prev.ranges.length == sel.ranges.length &&
      prev.somethingSelected() == sel.somethingSelected() &&
      new Date - doc.history.lastSelTime <= (doc.cm ? doc.cm.options.historyEventDelay : 500);
  }

  // Called whenever the selection changes, sets the new selection as
  // the pending selection in the history, and pushes the old pending
  // selection into the 'done' array when it was significantly
  // different (in number of selected ranges, emptiness, or time).
  function addSelectionToHistory(doc, sel, opId, options) {
	logger.debug(`>>> Entering addSelectionToHistory(doc=${doc},sel=${sel},opId=${opId},options=${options})`);
    var hist = doc.history, origin = options && options.origin;

    // A new event is started when the previous origin does not match
    // the current, or the origins don't allow matching. Origins
    // starting with * are always merged, those starting with + are
    // merged when similar and close together in time.
    if (opId == hist.lastSelOp ||
        (origin && hist.lastSelOrigin == origin &&
         (hist.lastModTime == hist.lastSelTime && hist.lastOrigin == origin ||
          selectionEventCanBeMerged(doc, origin, lst(hist.done), sel))))
      hist.done[hist.done.length - 1] = sel;
    else
      pushSelectionToHistory(sel, hist.done);

    hist.lastSelTime = +new Date;
	logger.debug(`addSelectionToHistory(doc=${doc},sel=${sel},opId=${opId},options=${options}): hist.lastSelTime → ${hist.lastSelTime}`);
	logger.debug(`addSelectionToHistory(doc=${doc},sel=${sel},opId=${opId},options=${options}): hist.lastSelOp → ${hist.lastSelOp}`);
    hist.lastSelOrigin = origin;
	logger.debug(`addSelectionToHistory(doc=${doc},sel=${sel},opId=${opId},options=${options}): hist.lastSelOrigin → ${hist.lastSelOrigin}`);
    hist.lastSelOp = opId;
    if (options && options.clearRedo !== false)
	logger.debug(`<<< Exiting addSelectionToHistory(doc=${doc},sel=${sel},opId=${opId},options=${options})`);
      clearSelectionEvents(hist.undone);
  }

  function pushSelectionToHistory(sel, dest) {
	logger.debug(`>>> Entering pushSelectionToHistory(sel=${sel},dest=${dest})`);
    var top = lst(dest);
    if (!(top && top.ranges && top.equals(sel)))
	logger.debug(`pushSelectionToHistory(sel=${sel},dest=${dest}): top → ${top}`);
      dest.push(sel);
	logger.debug(`<<< Exiting pushSelectionToHistory(sel=${sel},dest=${dest})`);
  }

  // Used to store marked span information in the history.
  function attachLocalSpans(doc, change, from, to) {
	logger.debug(`>>> Entering attachLocalSpans(doc=${doc},change=${change},from=${from},to=${to})`);
    var existing = change["spans_" + doc.id], n = 0;
    doc.iter(Math.max(doc.first, from), Math.min(doc.first + doc.size, to), function(line) {
      if (line.markedSpans)
        (existing || (existing = change["spans_" + doc.id] = {}))[n] = line.markedSpans;
      ++n;
    });
	logger.debug(`<<< Exiting attachLocalSpans(doc=${doc},change=${change},from=${from},to=${to})`);
  }

  // When un/re-doing restores text containing marked spans, those
  // that have been explicitly cleared should not be restored.
  function removeClearedSpans(spans) {
	logger.debug(`>>> Entering removeClearedSpans(spans=${spans})`);
    if (!spans) return null;
    for (var i = 0, out; i < spans.length; ++i) {
      if (spans[i].marker.explicitlyCleared) { if (!out) out = spans.slice(0, i); }
      else if (out) out.push(spans[i]);
    }
	logger.debug(`<<< Exiting removeClearedSpans(spans=${spans})`);
    return !out ? spans : out.length ? out : null;
  }

  // Retrieve and filter the old marked spans stored in a change event.
  function getOldSpans(doc, change) {
	logger.debug(`>>> Entering getOldSpans(doc=${doc},change=${change})`);
    var found = change["spans_" + doc.id];
    if (!found) return null;
	logger.debug(`getOldSpans(doc=${doc},change=${change}): found → ${found}`);
    for (var i = 0, nw = []; i < change.text.length; ++i)
      nw.push(removeClearedSpans(found[i]));
	logger.debug(`<<< Exiting getOldSpans(doc=${doc},change=${change})`);
    return nw;
  }

  // Used both to provide a JSON-safe object in .getHistory, and, when
  // detaching a document, to split the history in two
  function copyHistoryArray(events, newGroup, instantiateSel) {
	logger.debug(`>>> Entering copyHistoryArray(events=${events},newGroup=${newGroup},instantiateSel=${instantiateSel})`);
    for (var i = 0, copy = []; i < events.length; ++i) {
      var event = events[i];
      if (event.ranges) {
        copy.push(instantiateSel ? Selection.prototype.deepCopy.call(event) : event);
        continue;
      }
      var changes = event.changes, newChanges = [];
      copy.push({changes: newChanges});
      for (var j = 0; j < changes.length; ++j) {
        var change = changes[j], m;
        newChanges.push({from: change.from, to: change.to, text: change.text});
        if (newGroup) for (var prop in change) if (m = prop.match(/^spans_(\d+)$/)) {
          if (indexOf(newGroup, Number(m[1])) > -1) {
            lst(newChanges)[prop] = change[prop];
            delete change[prop];
          }
        }
      }
    }
	logger.debug(`<<< Exiting copyHistoryArray(events=${events},newGroup=${newGroup},instantiateSel=${instantiateSel})`);
    return copy;
  }

  // Rebasing/resetting history to deal with externally-sourced changes

  function rebaseHistSelSingle(pos, from, to, diff) {
	logger.debug(`>>> Entering rebaseHistSelSingle(pos=${pos},from=${from},to=${to},diff=${diff})`);
    if (to < pos.line) {
      pos.line += diff;
    } else if (from < pos.line) {
      pos.line = from;
	logger.debug(`rebaseHistSelSingle(pos=${pos},from=${from},to=${to},diff=${diff}): pos.line → ${pos.line}`);
      pos.ch = 0;
	logger.debug(`rebaseHistSelSingle(pos=${pos},from=${from},to=${to},diff=${diff}): pos.ch → ${pos.ch}`);
    }
	logger.debug(`<<< Exiting rebaseHistSelSingle(pos=${pos},from=${from},to=${to},diff=${diff})`);
  }

  // Tries to rebase an array of history events given a change in the
  // document. If the change touches the same lines as the event, the
  // event, and everything 'behind' it, is discarded. If the change is
  // before the event, the event's positions are updated. Uses a
  // copy-on-write scheme for the positions, to avoid having to
  // reallocate them all on every rebase, but also avoid problems with
  // shared position objects being unsafely updated.
  function rebaseHistArray(array, from, to, diff) {
	logger.debug(`>>> Entering rebaseHistArray(array=${array},from=${from},to=${to},diff=${diff})`);
    for (var i = 0; i < array.length; ++i) {
      var sub = array[i], ok = true;
      if (sub.ranges) {
        if (!sub.copied) { sub = array[i] = sub.deepCopy(); sub.copied = true; }
        for (var j = 0; j < sub.ranges.length; j++) {
          rebaseHistSelSingle(sub.ranges[j].anchor, from, to, diff);
          rebaseHistSelSingle(sub.ranges[j].head, from, to, diff);
        }
        continue;
      }
      for (var j = 0; j < sub.changes.length; ++j) {
        var cur = sub.changes[j];
        if (to < cur.from.line) {
          cur.from = Pos(cur.from.line + diff, cur.from.ch);
          cur.to = Pos(cur.to.line + diff, cur.to.ch);
        } else if (from <= cur.to.line) {
          ok = false;
          break;
        }
      }
      if (!ok) {
        array.splice(0, i + 1);
        i = 0;
		logger.debug(`<<< Exiting rebaseHistArray(array=${array},from=${from},to=${to},diff=${diff})`);
      }
    }
  }

  function rebaseHist(hist, change) {
	logger.debug(`>>> Entering rebaseHist(hist=${hist},change=${change})`);
    var from = change.from.line, to = change.to.line, diff = change.text.length - (to - from) - 1;
    rebaseHistArray(hist.done, from, to, diff);
    rebaseHistArray(hist.undone, from, to, diff);
	logger.debug(`<<< Exiting rebaseHist(hist=${hist},change=${change})`);
  }

  // EVENT UTILITIES

  // Due to the fact that we still support jurassic IE versions, some
  // compatibility wrappers are needed.

  var e_preventDefault = CodeMirror.e_preventDefault = function(e) {
    if (e.preventDefault) e.preventDefault();
    else e.returnValue = false;
  };
  var e_stopPropagation = CodeMirror.e_stopPropagation = function(e) {
    if (e.stopPropagation) e.stopPropagation();
    else e.cancelBubble = true;
  };
  function e_defaultPrevented(e) {
	logger.debug(`>>> Entering e_defaultPrevented(e=${e})`);
	logger.debug(`<<< Exiting e_defaultPrevented(e=${e})`);
    return e.defaultPrevented != null ? e.defaultPrevented : e.returnValue == false;
  }
  var e_stop = CodeMirror.e_stop = function(e) {e_preventDefault(e); e_stopPropagation(e);};

  function e_target(e) {return e.target || e.srcElement;}
  function e_button(e) {
	logger.debug(`>>> Entering e_button(e=${e})`);
    var b = e.which;
    if (b == null) {
      if (e.button & 1) b = 1;
      else if (e.button & 2) b = 3;
      else if (e.button & 4) b = 2;
    }
    if (mac && e.ctrlKey && b == 1) b = 3;
	logger.debug(`<<< Exiting e_button(e=${e})`);
    return b;
  }

  // EVENT HANDLING

  // Lightweight event framework. on/off also work on DOM nodes,
  // registering native DOM handlers.

  var on = CodeMirror.on = function(emitter, type, f) {
    if (emitter.addEventListener)
      emitter.addEventListener(type, f, false);
    else if (emitter.attachEvent)
      emitter.attachEvent("on" + type, f);
    else {
      var map = emitter._handlers || (emitter._handlers = {});
      var arr = map[type] || (map[type] = []);
	logger.debug(`function(emitter=${emitter},type=${type},f=${f}): arr → ${arr}`);
      arr.push(f);
	logger.debug(`>>> Entering map(array=${array},f=${f})`);
	logger.debug(`<<< Exiting map(array=${array},f=${f})`);
    }
  };

  var off = CodeMirror.off = function(emitter, type, f) {
    if (emitter.removeEventListener)
      emitter.removeEventListener(type, f, false);
    else if (emitter.detachEvent)
      emitter.detachEvent("on" + type, f);
    else {
      var arr = emitter._handlers && emitter._handlers[type];
      if (!arr) return;
      for (var i = 0; i < arr.length; ++i)
        if (arr[i] == f) { arr.splice(i, 1); break; }
    }
  };

  var signal = CodeMirror.signal = function(emitter, type /*, values...*/) {
    var arr = emitter._handlers && emitter._handlers[type];
    if (!arr) return;
    var args = Array.prototype.slice.call(arguments, 2);
	logger.debug(`function(emitter=${emitter}): args → ${args}`);
    for (var i = 0; i < arr.length; ++i) arr[i].apply(null, args);
  };

  var orphanDelayedCallbacks = null;

  // Often, we want to signal events at a point where we are in the
  // middle of some work, but don't want the handler to start calling
  // other methods on the editor, which might be in an inconsistent
  // state or simply not expect any other events to happen.
  // signalLater looks whether there are any handlers, and schedules
  // them to be executed when the last operation ends, or, if no
  // operation is active, when a timeout fires.
  function signalLater(emitter, type /*, values...*/) {
	logger.debug(`>>> Entering signalLater(emitter=${emitter})`);
    var arr = emitter._handlers && emitter._handlers[type];
    if (!arr) return;
    var args = Array.prototype.slice.call(arguments, 2), list;
	logger.debug(`signalLater(emitter=${emitter}): args → ${args}`);
    if (operationGroup) {
      list = operationGroup.delayedCallbacks;
    } else if (orphanDelayedCallbacks) {
      list = orphanDelayedCallbacks;
    } else {
      list = orphanDelayedCallbacks = [];
      setTimeout(fireOrphanDelayed, 0);
	logger.debug(`signalLater(emitter=${emitter}): list → ${list}`);
    }
	logger.debug(`<<< Exiting signalLater(emitter=${emitter})`);
    function bnd(f) {return function(){f.apply(null, args);};};
    for (var i = 0; i < arr.length; ++i)
      list.push(bnd(arr[i]));
  }

  function fireOrphanDelayed() {
	logger.debug('>>> Entering fireOrphanDelayed()');
    var delayed = orphanDelayedCallbacks;
    orphanDelayedCallbacks = null;
    for (var i = 0; i < delayed.length; ++i) delayed[i]();
	logger.debug(`fireOrphanDelayed(): orphanDelayedCallbacks → ${orphanDelayedCallbacks}`);
	logger.debug('<<< Exiting fireOrphanDelayed()');
  }

  // The DOM events that CodeMirror handles can be overridden by
  // registering a (non-DOM) handler on the editor for the event name,
  // and preventDefault-ing the event in that handler.
  function signalDOMEvent(cm, e, override) {
	logger.debug(`>>> Entering signalDOMEvent(cm=${cm},e=${e},override=${override})`);
    if (typeof e == "string")
      e = {type: e, preventDefault: function() { this.defaultPrevented = true; }};
    signal(cm, override || e.type, cm, e);
	logger.debug(`signalDOMEvent(cm=${cm},e=${e},override=${override}): e → ${e}`);
	logger.debug(`<<< Exiting signalDOMEvent(cm=${cm},e=${e},override=${override})`);
    return e_defaultPrevented(e) || e.codemirrorIgnore;
  }

  function signalCursorActivity(cm) {
	logger.debug(`>>> Entering signalCursorActivity(cm=${cm})`);
    var arr = cm._handlers && cm._handlers.cursorActivity;
    if (!arr) return;
    var set = cm.curOp.cursorActivityHandlers || (cm.curOp.cursorActivityHandlers = []);
    for (var i = 0; i < arr.length; ++i) if (indexOf(set, arr[i]) == -1)
	logger.debug(`<<< Exiting signalCursorActivity(cm=${cm})`);
      set.push(arr[i]);
  }

  function hasHandler(emitter, type) {
    var arr = emitter._handlers && emitter._handlers[type];
    return arr && arr.length > 0;
  }

  // Add on and off methods to a constructor's prototype, to make
  // registering events on such objects more convenient.
  function eventMixin(ctor) {
	logger.debug(`>>> Entering eventMixin(ctor=${ctor})`);
    ctor.prototype.on = function(type, f) {on(this, type, f);};
    ctor.prototype.off = function(type, f) {off(this, type, f);};
	logger.debug(`<<< Exiting eventMixin(ctor=${ctor})`);
  }

  // MISC UTILITIES

  // Number of pixels added to scroller and sizer to hide scrollbar
  var scrollerGap = 30;

  // Returned or thrown by various protocols to signal 'I'm not
  // handling this'.
  var Pass = CodeMirror.Pass = {toString: function(){return "CodeMirror.Pass";}};

  // Reused option objects for setSelection & friends
  var sel_dontScroll = {scroll: false}, sel_mouse = {origin: "*mouse"}, sel_move = {origin: "+move"};

  function Delayed() {this.id = null;}
  Delayed.prototype.set = function(ms, f) {
    clearTimeout(this.id);
    this.id = setTimeout(f, ms);
  };

  // Counts the column offset in a string, taking tabs into account.
  // Used mostly to find indentation.
  var countColumn = CodeMirror.countColumn = function(string, end, tabSize, startIndex, startValue) {
    if (end == null) {
      end = string.search(/[^\s\u00a0]/);
      if (end == -1) end = string.length;
    }
    for (var i = startIndex || 0, n = startValue || 0;;) {
      var nextTab = string.indexOf("\t", i);
	logger.debug(`function(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue}): nextTab → ${nextTab}`);
      if (nextTab < 0 || nextTab >= end)
        return n + (end - i);
      n += nextTab - i;
      n += tabSize - (n % tabSize);
      i = nextTab + 1;
    }
  };

  // The inverse of countColumn -- find the offset that corresponds to
  // a particular column.
  function findColumn(string, goal, tabSize) {
	logger.debug(`>>> Entering findColumn(string=${string},goal=${goal},tabSize=${tabSize})`);
    for (var pos = 0, col = 0;;) {
      var nextTab = string.indexOf("\t", pos);
	logger.debug(`findColumn(string=${string},goal=${goal},tabSize=${tabSize}): nextTab → ${nextTab}`);
      if (nextTab == -1) nextTab = string.length;
      var skipped = nextTab - pos;
      if (nextTab == string.length || col + skipped >= goal)
        return pos + Math.min(skipped, goal - col);
      col += nextTab - pos;
      col += tabSize - (col % tabSize);
      pos = nextTab + 1;
      if (col >= goal) return pos;
    }
	logger.debug(`<<< Exiting findColumn(string=${string},goal=${goal},tabSize=${tabSize})`);
  }

  var spaceStrs = [""];
  function spaceStr(n) {
	logger.debug(`>>> Entering spaceStr(n=${n})`);
    while (spaceStrs.length <= n)
      spaceStrs.push(lst(spaceStrs) + " ");
	logger.debug(`<<< Exiting spaceStr(n=${n})`);
    return spaceStrs[n];
  }

  function lst(arr) { return arr[arr.length-1]; }

  var selectInput = function(node) { node.select(); };
  if (ios) // Mobile Safari apparently has a bug where select() is broken.
    selectInput = function(node) { node.selectionStart = 0; node.selectionEnd = node.value.length; };
  else if (ie) // Suppress mysterious IE10 errors
    selectInput = function(node) { try { node.select(); } catch(_e) {} };

  function indexOf(array, elt) {
    for (var i = 0; i < array.length; ++i)
      if (array[i] == elt) return i;
    return -1;
  }
  function map(array, f) {
    var out = [];
    for (var i = 0; i < array.length; i++) out[i] = f(array[i], i);
    return out;
  }

  function nothing() {}

  function createObj(base, props) {
	logger.debug(`>>> Entering createObj(base=${base},props=${props})`);
    var inst;
    if (Object.create) {
      inst = Object.create(base);
    } else {
      nothing.prototype = base;
	logger.debug(`createObj(base=${base},props=${props}): nothing.prototype → ${nothing.prototype}`);
      inst = new nothing();
    }
    if (props) copyObj(props, inst);
	logger.debug(`createObj(base=${base},props=${props}): inst → ${inst}`);
	logger.debug(`<<< Exiting createObj(base=${base},props=${props})`);
    return inst;
  };

  function copyObj(obj, target, overwrite) {
    if (!target) target = {};
    for (var prop in obj)
      if (obj.hasOwnProperty(prop) && (overwrite !== false || !target.hasOwnProperty(prop)))
        target[prop] = obj[prop];
    return target;
  }

  function bind(f) {
	logger.debug(`>>> Entering bind(f=${f})`);
    var args = Array.prototype.slice.call(arguments, 1);
	logger.debug(`bind(f=${f}): args → ${args}`);
	logger.debug(`<<< Exiting bind(f=${f})`);
    return function(){return f.apply(null, args);};
  }

  var nonASCIISingleCaseWordChar = /[\u00df\u0587\u0590-\u05f4\u0600-\u06ff\u3040-\u309f\u30a0-\u30ff\u3400-\u4db5\u4e00-\u9fcc\uac00-\ud7af]/;
  var isWordCharBasic = CodeMirror.isWordChar = function(ch) {
	logger.debug(`>>> Entering isWordChar(ch=${ch},helper=${helper})`);
	logger.debug(`<<< Exiting isWordChar(ch=${ch},helper=${helper})`);
    return /\w/.test(ch) || ch > "\x80" &&
      (ch.toUpperCase() != ch.toLowerCase() || nonASCIISingleCaseWordChar.test(ch));
  };
  function isWordChar(ch, helper) {
    if (!helper) return isWordCharBasic(ch);
    if (helper.source.indexOf("\\w") > -1 && isWordCharBasic(ch)) return true;
    return helper.test(ch);
  }

  function isEmpty(obj) {
    for (var n in obj) if (obj.hasOwnProperty(n) && obj[n]) return false;
    return true;
  }

  // Extending unicode characters. A series of a non-extending char +
  // any number of extending chars is treated as a single unit as far
  // as editing and measuring is concerned. This is not fully correct,
  // since some scripts/fonts/browsers also treat other configurations
  // of code points as a group.
  var extendingChars = /[\u0300-\u036f\u0483-\u0489\u0591-\u05bd\u05bf\u05c1\u05c2\u05c4\u05c5\u05c7\u0610-\u061a\u064b-\u065e\u0670\u06d6-\u06dc\u06de-\u06e4\u06e7\u06e8\u06ea-\u06ed\u0711\u0730-\u074a\u07a6-\u07b0\u07eb-\u07f3\u0816-\u0819\u081b-\u0823\u0825-\u0827\u0829-\u082d\u0900-\u0902\u093c\u0941-\u0948\u094d\u0951-\u0955\u0962\u0963\u0981\u09bc\u09be\u09c1-\u09c4\u09cd\u09d7\u09e2\u09e3\u0a01\u0a02\u0a3c\u0a41\u0a42\u0a47\u0a48\u0a4b-\u0a4d\u0a51\u0a70\u0a71\u0a75\u0a81\u0a82\u0abc\u0ac1-\u0ac5\u0ac7\u0ac8\u0acd\u0ae2\u0ae3\u0b01\u0b3c\u0b3e\u0b3f\u0b41-\u0b44\u0b4d\u0b56\u0b57\u0b62\u0b63\u0b82\u0bbe\u0bc0\u0bcd\u0bd7\u0c3e-\u0c40\u0c46-\u0c48\u0c4a-\u0c4d\u0c55\u0c56\u0c62\u0c63\u0cbc\u0cbf\u0cc2\u0cc6\u0ccc\u0ccd\u0cd5\u0cd6\u0ce2\u0ce3\u0d3e\u0d41-\u0d44\u0d4d\u0d57\u0d62\u0d63\u0dca\u0dcf\u0dd2-\u0dd4\u0dd6\u0ddf\u0e31\u0e34-\u0e3a\u0e47-\u0e4e\u0eb1\u0eb4-\u0eb9\u0ebb\u0ebc\u0ec8-\u0ecd\u0f18\u0f19\u0f35\u0f37\u0f39\u0f71-\u0f7e\u0f80-\u0f84\u0f86\u0f87\u0f90-\u0f97\u0f99-\u0fbc\u0fc6\u102d-\u1030\u1032-\u1037\u1039\u103a\u103d\u103e\u1058\u1059\u105e-\u1060\u1071-\u1074\u1082\u1085\u1086\u108d\u109d\u135f\u1712-\u1714\u1732-\u1734\u1752\u1753\u1772\u1773\u17b7-\u17bd\u17c6\u17c9-\u17d3\u17dd\u180b-\u180d\u18a9\u1920-\u1922\u1927\u1928\u1932\u1939-\u193b\u1a17\u1a18\u1a56\u1a58-\u1a5e\u1a60\u1a62\u1a65-\u1a6c\u1a73-\u1a7c\u1a7f\u1b00-\u1b03\u1b34\u1b36-\u1b3a\u1b3c\u1b42\u1b6b-\u1b73\u1b80\u1b81\u1ba2-\u1ba5\u1ba8\u1ba9\u1c2c-\u1c33\u1c36\u1c37\u1cd0-\u1cd2\u1cd4-\u1ce0\u1ce2-\u1ce8\u1ced\u1dc0-\u1de6\u1dfd-\u1dff\u200c\u200d\u20d0-\u20f0\u2cef-\u2cf1\u2de0-\u2dff\u302a-\u302f\u3099\u309a\ua66f-\ua672\ua67c\ua67d\ua6f0\ua6f1\ua802\ua806\ua80b\ua825\ua826\ua8c4\ua8e0-\ua8f1\ua926-\ua92d\ua947-\ua951\ua980-\ua982\ua9b3\ua9b6-\ua9b9\ua9bc\uaa29-\uaa2e\uaa31\uaa32\uaa35\uaa36\uaa43\uaa4c\uaab0\uaab2-\uaab4\uaab7\uaab8\uaabe\uaabf\uaac1\uabe5\uabe8\uabed\udc00-\udfff\ufb1e\ufe00-\ufe0f\ufe20-\ufe26\uff9e\uff9f]/;
  function isExtendingChar(ch) { return ch.charCodeAt(0) >= 768 && extendingChars.test(ch); }

  // DOM UTILITIES

  function elt(tag, content, className, style) {
    var e = document.createElement(tag);
    if (className) e.className = className;
    if (style) e.style.cssText = style;
    if (typeof content == "string") e.appendChild(document.createTextNode(content));
    else if (content) for (var i = 0; i < content.length; ++i) e.appendChild(content[i]);
    return e;
  }

  var range;
  if (document.createRange) range = function(node, start, end, endNode) {
    var r = document.createRange();
    r.setEnd(endNode || node, end);
    r.setStart(node, start);
	logger.debug(`<<< Exiting function(node=${node},start=${start},end=${end})`);
    return r;
  };
  else range = function(node, start, end) {
    var r = document.body.createTextRange();
    try { r.moveToElementText(node.parentNode); }
    catch(e) { return r; }
	logger.error(`Exception in function(node=${node},start=${start},end=${end}): ${e.message}`, e);
    r.collapse(true);
    r.moveEnd("character", end);
    r.moveStart("character", start);
    return r;
  };

  function removeChildren(e) {
	logger.debug(`>>> Entering removeChildren(e=${e})`);
    for (var count = e.childNodes.length; count > 0; --count)
      e.removeChild(e.firstChild);
	logger.debug(`<<< Exiting removeChildren(e=${e})`);
    return e;
  }

  function removeChildrenAndAdd(parent, e) {
	logger.debug(`>>> Entering removeChildrenAndAdd(parent=${parent},e=${e})`);
	logger.debug(`<<< Exiting removeChildrenAndAdd(parent=${parent},e=${e})`);
    return removeChildren(parent).appendChild(e);
  }

  var contains = CodeMirror.contains = function(parent, child) {
    if (child.nodeType == 3) // Android browser always returns false when child is a textnode
      child = child.parentNode;
    if (parent.contains)
      return parent.contains(child);
    do {
      if (child.nodeType == 11) child = child.host;
      if (child == parent) return true;
    } while (child = child.parentNode);
  };

  function activeElt() {
	logger.debug('>>> Entering activeElt()');
    var activeElement = document.activeElement;
    while (activeElement && activeElement.root && activeElement.root.activeElement)
      activeElement = activeElement.root.activeElement;
	logger.debug('<<< Exiting activeElt()');
    return activeElement;
  }
  // Older versions of IE throws unspecified error when touching
  // document.activeElement in some cases (during loading, in iframe)
  if (ie && ie_version < 11) activeElt = function() {
    try { return document.activeElement; }
    catch(e) { return document.body; }
  };

  function classTest(cls) { return new RegExp("(^|\\s)" + cls + "(?:$|\\s)\\s*"); }
  var rmClass = CodeMirror.rmClass = function(node, cls) {
    var current = node.className;
    var match = classTest(cls).exec(current);
    if (match) {
      var after = current.slice(match.index + match[0].length);
	logger.debug(`function(node=${node},cls=${cls}): after → ${after}`);
      node.className = current.slice(0, match.index) + (after ? match[1] + after : "");
    }
  };
  var addClass = CodeMirror.addClass = function(node, cls) {
    var current = node.className;
    if (!classTest(cls).test(current)) node.className += (current ? " " : "") + cls;
  };
  function joinClasses(a, b) {
	logger.debug(`>>> Entering joinClasses(a=${a},b=${b})`);
    var as = a.split(" ");
    for (var i = 0; i < as.length; i++)
	logger.debug(`joinClasses(a=${a},b=${b}): as → ${as}`);
      if (as[i] && !classTest(as[i]).test(b)) b += " " + as[i];
	logger.debug(`<<< Exiting joinClasses(a=${a},b=${b})`);
    return b;
  }

  // WINDOW-WIDE EVENTS

  // These must be handled carefully, because naively registering a
  // handler for each editor will cause the editors to never be
  // garbage collected.

  function forEachCodeMirror(f) {
	logger.debug(`>>> Entering forEachCodeMirror(f=${f})`);
    if (!document.body.getElementsByClassName) return;
    var byClass = document.body.getElementsByClassName("CodeMirror");
	logger.debug(`forEachCodeMirror(f=${f}): byClass → ${byClass}`);
    for (var i = 0; i < byClass.length; i++) {
      var cm = byClass[i].CodeMirror;
      if (cm) f(cm);
	logger.debug(`<<< Exiting forEachCodeMirror(f=${f})`);
    }
  }

  var globalsRegistered = false;
  function ensureGlobalHandlers() {
	logger.debug('>>> Entering ensureGlobalHandlers()');
    if (globalsRegistered) return;
    registerGlobalHandlers();
    globalsRegistered = true;
	logger.debug('<<< Exiting ensureGlobalHandlers()');
  }
  function registerGlobalHandlers() {
	logger.debug('>>> Entering registerGlobalHandlers()');
    // When the window resizes, we need to refresh active editors.
    var resizeTimer;
	logger.debug(`registerGlobalHandlers(): resizeTimer → ${resizeTimer}`);
    on(window, "resize", function() {
      if (resizeTimer == null) resizeTimer = setTimeout(function() {
        resizeTimer = null;
        forEachCodeMirror(onResize);
      }, 100);
    });
    // When the window loses focus, we want to show the editor as blurred
    on(window, "blur", function() {
      forEachCodeMirror(onBlur);
    });
	logger.debug('<<< Exiting registerGlobalHandlers()');
  }

  // FEATURE DETECTION

  // Detect drag-and-drop
  var dragAndDrop = function() {
    // There is *some* kind of drag-and-drop support in IE6-8, but I
    // couldn't get it to work yet.
    if (ie && ie_version < 9) return false;
    var div = elt('div');
	logger.debug(`function(): div → ${div}`);
    return "draggable" in div || "dragDrop" in div;
  }();

  var zwspSupported;
  function zeroWidthElement(measure) {
	logger.debug(`>>> Entering zeroWidthElement(measure=${measure})`);
    if (zwspSupported == null) {
      var test = elt("span", "\u200b");
	logger.debug(`zeroWidthElement(measure=${measure}): test → ${test}`);
      removeChildrenAndAdd(measure, elt("span", [test, document.createTextNode("x")]));
      if (measure.firstChild.offsetHeight != 0)
        zwspSupported = test.offsetWidth <= 1 && test.offsetHeight > 2 && !(ie && ie_version < 8);
    }
    var node = zwspSupported ? elt("span", "\u200b") :
      elt("span", "\u00a0", null, "display: inline-block; width: 1px; margin-right: -1px");
    node.setAttribute("cm-text", "");
	logger.debug(`<<< Exiting zeroWidthElement(measure=${measure})`);
    return node;
  }

  // Feature-detect IE's crummy client rect reporting for bidi text
  var badBidiRects;
  function hasBadBidiRects(measure) {
	logger.debug(`>>> Entering hasBadBidiRects(measure=${measure})`);
    if (badBidiRects != null) return badBidiRects;
    var txt = removeChildrenAndAdd(measure, document.createTextNode("A\u062eA"));
	logger.debug(`hasBadBidiRects(measure=${measure}): txt → ${txt}`);
    var r0 = range(txt, 0, 1).getBoundingClientRect();
	logger.debug(`hasBadBidiRects(measure=${measure}): r0 → ${r0}`);
    if (!r0 || r0.left == r0.right) return false; // Safari returns null in some cases (#2780)
    var r1 = range(txt, 1, 2).getBoundingClientRect();
	logger.debug(`hasBadBidiRects(measure=${measure}): r1 → ${r1}`);
	logger.debug(`<<< Exiting hasBadBidiRects(measure=${measure})`);
    return badBidiRects = (r1.right - r0.right < 3);
  }

  // See if "".split is the broken IE version, if so, provide an
  // alternative way to split lines.
  var splitLinesAuto = CodeMirror.splitLines = "\n\nb".split(/\n/).length != 3 ? function(string) {
    var pos = 0, result = [], l = string.length;
    while (pos <= l) {
      var nl = string.indexOf("\n", pos);
      if (nl == -1) nl = string.length;
	logger.debug(`function(): nl → ${nl}`);
      var line = string.slice(pos, string.charAt(nl - 1) == "\r" ? nl - 1 : nl);
      var rt = line.indexOf("\r");
      if (rt != -1) {
        result.push(line.slice(0, rt));
        pos += rt + 1;
      } else {
        result.push(line);
        pos = nl + 1;
      }
    }
	logger.debug(`<<< Exiting function(te=${te})`);
    return result;
  } : function(string){return string.split(/\r\n?|\n/);};

  var hasSelection = window.getSelection ? function(te) {
    try { return te.selectionStart != te.selectionEnd; }
    catch(e) { return false; }
	logger.error(`Exception in function(te=${te}): ${e.message}`, e);
  } : function(te) {
    try {var range = te.ownerDocument.selection.createRange();}
    catch(e) {}
    if (!range || range.parentElement() != te) return false;
    return range.compareEndPoints("StartToEnd", range) != 0;
  };

  var hasCopyEvent = (function() {
    var e = elt("div");
    if ("oncopy" in e) return true;
    e.setAttribute("oncopy", "return;");
    return typeof e.oncopy == "function";
  })();

  var badZoomedRects = null;
  function hasBadZoomedRects(measure) {
	logger.debug(`>>> Entering hasBadZoomedRects(measure=${measure})`);
    if (badZoomedRects != null) return badZoomedRects;
    var node = removeChildrenAndAdd(measure, elt("span", "x"));
	logger.debug(`hasBadZoomedRects(measure=${measure}): node → ${node}`);
    var normal = node.getBoundingClientRect();
	logger.debug(`hasBadZoomedRects(measure=${measure}): normal → ${normal}`);
    var fromRange = range(node, 0, 1).getBoundingClientRect();
	logger.debug(`hasBadZoomedRects(measure=${measure}): fromRange → ${fromRange}`);
	logger.debug(`<<< Exiting hasBadZoomedRects(measure=${measure})`);
    return badZoomedRects = Math.abs(normal.left - fromRange.left) > 1;
  }

  // KEY NAMES

  var keyNames = {3: "Enter", 8: "Backspace", 9: "Tab", 13: "Enter", 16: "Shift", 17: "Ctrl", 18: "Alt",
                  19: "Pause", 20: "CapsLock", 27: "Esc", 32: "Space", 33: "PageUp", 34: "PageDown", 35: "End",
                  36: "Home", 37: "Left", 38: "Up", 39: "Right", 40: "Down", 44: "PrintScrn", 45: "Insert",
                  46: "Delete", 59: ";", 61: "=", 91: "Mod", 92: "Mod", 93: "Mod", 107: "=", 109: "-", 127: "Delete",
                  173: "-", 186: ";", 187: "=", 188: ",", 189: "-", 190: ".", 191: "/", 192: "`", 219: "[", 220: "\\",
                  221: "]", 222: "'", 63232: "Up", 63233: "Down", 63234: "Left", 63235: "Right", 63272: "Delete",
                  63273: "Home", 63275: "End", 63276: "PageUp", 63277: "PageDown", 63302: "Insert"};
  CodeMirror.keyNames = keyNames;
  (function() {
    // Number keys
    for (var i = 0; i < 10; i++) keyNames[i + 48] = keyNames[i + 96] = String(i);
    // Alphabetic keys
    for (var i = 65; i <= 90; i++) keyNames[i] = String.fromCharCode(i);
    // Function keys
    for (var i = 1; i <= 12; i++) keyNames[i + 111] = keyNames[i + 63235] = "F" + i;
  })();

  // BIDI HELPERS

  function iterateBidiSections(order, from, to, f) {
    if (!order) return f(from, to, "ltr");
    var found = false;
    for (var i = 0; i < order.length; ++i) {
      var part = order[i];
      if (part.from < to && part.to > from || from == to && part.to == from) {
        f(Math.max(part.from, from), Math.min(part.to, to), part.level == 1 ? "rtl" : "ltr");
        found = true;
      }
    }
    if (!found) f(from, to, "ltr");
  }

  function bidiLeft(part) { return part.level % 2 ? part.to : part.from; }
  function bidiRight(part) { return part.level % 2 ? part.from : part.to; }

  function lineLeft(line) { var order = getOrder(line); return order ? bidiLeft(order[0]) : 0; }
  function lineRight(line) {
    var order = getOrder(line);
    if (!order) return line.text.length;
    return bidiRight(lst(order));
  }

  function lineStart(cm, lineN) {
    var line = getLine(cm.doc, lineN);
    var visual = visualLine(line);
    if (visual != line) lineN = lineNo(visual);
    var order = getOrder(visual);
    var ch = !order ? 0 : order[0].level % 2 ? lineRight(visual) : lineLeft(visual);
    return Pos(lineN, ch);
  }
  function lineEnd(cm, lineN) {
    var merged, line = getLine(cm.doc, lineN);
    while (merged = collapsedSpanAtEnd(line)) {
      line = merged.find(1, true).line;
      lineN = null;
    }
    var order = getOrder(line);
    var ch = !order ? line.text.length : order[0].level % 2 ? lineLeft(line) : lineRight(line);
    return Pos(lineN == null ? lineNo(line) : lineN, ch);
  }
  function lineStartSmart(cm, pos) {
    var start = lineStart(cm, pos.line);
    var line = getLine(cm.doc, start.line);
    var order = getOrder(line);
    if (!order || order[0].level == 0) {
      var firstNonWS = Math.max(0, line.text.search(/\S/));
      var inWS = pos.line == start.line && pos.ch <= firstNonWS && pos.ch;
      return Pos(start.line, inWS ? 0 : firstNonWS);
    }
    return start;
  }

  function compareBidiLevel(order, a, b) {
    var linedir = order[0].level;
    if (a == linedir) return true;
    if (b == linedir) return false;
    return a < b;
  }
  var bidiOther;
  function getBidiPartAt(order, pos) {
    bidiOther = null;
    for (var i = 0, found; i < order.length; ++i) {
      var cur = order[i];
      if (cur.from < pos && cur.to > pos) return i;
      if ((cur.from == pos || cur.to == pos)) {
        if (found == null) {
          found = i;
        } else if (compareBidiLevel(order, cur.level, order[found].level)) {
          if (cur.from != cur.to) bidiOther = found;
          return i;
        } else {
          if (cur.from != cur.to) bidiOther = i;
          return found;
        }
      }
    }
    return found;
  }

  function moveInLine(line, pos, dir, byUnit) {
    if (!byUnit) return pos + dir;
    do pos += dir;
    while (pos > 0 && isExtendingChar(line.text.charAt(pos)));
    return pos;
  }

  // This is needed in order to move 'visually' through bi-directional
  // text -- i.e., pressing left should make the cursor go left, even
  // when in RTL text. The tricky part is the 'jumps', where RTL and
  // LTR text touch each other. This often requires the cursor offset
  // to move more than one unit, in order to visually move one unit.
  function moveVisually(line, start, dir, byUnit) {
    var bidi = getOrder(line);
    if (!bidi) return moveLogically(line, start, dir, byUnit);
    var pos = getBidiPartAt(bidi, start), part = bidi[pos];
    var target = moveInLine(line, start, part.level % 2 ? -dir : dir, byUnit);

    for (;;) {
      if (target > part.from && target < part.to) return target;
      if (target == part.from || target == part.to) {
        if (getBidiPartAt(bidi, target) == pos) return target;
        part = bidi[pos += dir];
        return (dir > 0) == part.level % 2 ? part.to : part.from;
      } else {
        part = bidi[pos += dir];
        if (!part) return null;
        if ((dir > 0) == part.level % 2)
          target = moveInLine(line, part.to, -1, byUnit);
        else
          target = moveInLine(line, part.from, 1, byUnit);
      }
    }
  }

  function moveLogically(line, start, dir, byUnit) {
    var target = start + dir;
    if (byUnit) while (target > 0 && isExtendingChar(line.text.charAt(target))) target += dir;
    return target < 0 || target > line.text.length ? null : target;
  }

  // Bidirectional ordering algorithm
  // See http://unicode.org/reports/tr9/tr9-13.html for the algorithm
  // that this (partially) implements.

  // One-char codes used for character types:
  // L (L):   Left-to-Right
  // R (R):   Right-to-Left
  // r (AL):  Right-to-Left Arabic
  // 1 (EN):  European Number
  // + (ES):  European Number Separator
  // % (ET):  European Number Terminator
  // n (AN):  Arabic Number
  // , (CS):  Common Number Separator
  // m (NSM): Non-Spacing Mark
  // b (BN):  Boundary Neutral
  // s (B):   Paragraph Separator
  // t (S):   Segment Separator
  // w (WS):  Whitespace
  // N (ON):  Other Neutrals

  // Returns null if characters are ordered as they appear
  // (left-to-right), or an array of sections ({from, to, level}
  // objects) in the order in which they occur visually.
  var bidiOrdering = (function() {
    // Character types for codepoints 0 to 0xff
    var lowTypes = "bbbbbbbbbtstwsbbbbbbbbbbbbbbssstwNN%%%NNNNNN,N,N1111111111NNNNNNNLLLLLLLLLLLLLLLLLLLLLLLLLLNNNNNNLLLLLLLLLLLLLLLLLLLLLLLLLLNNNNbbbbbbsbbbbbbbbbbbbbbbbbbbbbbbbbb,N%%%%NNNNLNNNNN%%11NLNNN1LNNNNNLLLLLLLLLLLLLLLLLLLLLLLNLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLN";
    // Character types for codepoints 0x600 to 0x6ff
    var arabicTypes = "rrrrrrrrrrrr,rNNmmmmmmrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrmmmmmmmmmmmmmmrrrrrrrnnnnnnnnnn%nnrrrmrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrmmmmmmmmmmmmmmmmmmmNmmmm";
    function charType(code) {
      if (code <= 0xf7) return lowTypes.charAt(code);
      else if (0x590 <= code && code <= 0x5f4) return "R";
      else if (0x600 <= code && code <= 0x6ed) return arabicTypes.charAt(code - 0x600);
      else if (0x6ee <= code && code <= 0x8ac) return "r";
      else if (0x2000 <= code && code <= 0x200b) return "w";
      else if (code == 0x200c) return "b";
      else return "L";
    }

    var bidiRE = /[\u0590-\u05f4\u0600-\u06ff\u0700-\u08ac]/;
    var isNeutral = /[stwN]/, isStrong = /[LRr]/, countsAsLeft = /[Lb1n]/, countsAsNum = /[1n]/;
    // Browsers seem to always treat the boundaries of block elements as being L.
    var outerType = "L";

    function BidiSpan(level, from, to) {
      this.level = level;
      this.from = from; this.to = to;
    }

    return function(str) {
      if (!bidiRE.test(str)) return false;
      var len = str.length, types = [];
      for (var i = 0, type; i < len; ++i)
        types.push(type = charType(str.charCodeAt(i)));

      // W1. Examine each non-spacing mark (NSM) in the level run, and
      // change the type of the NSM to the type of the previous
      // character. If the NSM is at the start of the level run, it will
      // get the type of sor.
      for (var i = 0, prev = outerType; i < len; ++i) {
        var type = types[i];
        if (type == "m") types[i] = prev;
        else prev = type;
      }

      // W2. Search backwards from each instance of a European number
      // until the first strong type (R, L, AL, or sor) is found. If an
      // AL is found, change the type of the European number to Arabic
      // number.
      // W3. Change all ALs to R.
      for (var i = 0, cur = outerType; i < len; ++i) {
        var type = types[i];
        if (type == "1" && cur == "r") types[i] = "n";
        else if (isStrong.test(type)) { cur = type; if (type == "r") types[i] = "R"; }
      }

      // W4. A single European separator between two European numbers
      // changes to a European number. A single common separator between
      // two numbers of the same type changes to that type.
      for (var i = 1, prev = types[0]; i < len - 1; ++i) {
        var type = types[i];
        if (type == "+" && prev == "1" && types[i+1] == "1") types[i] = "1";
        else if (type == "," && prev == types[i+1] &&
                 (prev == "1" || prev == "n")) types[i] = prev;
        prev = type;
      }

      // W5. A sequence of European terminators adjacent to European
      // numbers changes to all European numbers.
      // W6. Otherwise, separators and terminators change to Other
      // Neutral.
      for (var i = 0; i < len; ++i) {
        var type = types[i];
        if (type == ",") types[i] = "N";
        else if (type == "%") {
          for (var end = i + 1; end < len && types[end] == "%"; ++end) {}
          var replace = (i && types[i-1] == "!") || (end < len && types[end] == "1") ? "1" : "N";
          for (var j = i; j < end; ++j) types[j] = replace;
          i = end - 1;
        }
      }

      // W7. Search backwards from each instance of a European number
      // until the first strong type (R, L, or sor) is found. If an L is
      // found, then change the type of the European number to L.
      for (var i = 0, cur = outerType; i < len; ++i) {
        var type = types[i];
        if (cur == "L" && type == "1") types[i] = "L";
        else if (isStrong.test(type)) cur = type;
      }

      // N1. A sequence of neutrals takes the direction of the
      // surrounding strong text if the text on both sides has the same
      // direction. European and Arabic numbers act as if they were R in
      // terms of their influence on neutrals. Start-of-level-run (sor)
      // and end-of-level-run (eor) are used at level run boundaries.
      // N2. Any remaining neutrals take the embedding direction.
      for (var i = 0; i < len; ++i) {
        if (isNeutral.test(types[i])) {
          for (var end = i + 1; end < len && isNeutral.test(types[end]); ++end) {}
          var before = (i ? types[i-1] : outerType) == "L";
          var after = (end < len ? types[end] : outerType) == "L";
          var replace = before || after ? "L" : "R";
          for (var j = i; j < end; ++j) types[j] = replace;
          i = end - 1;
        }
      }

      // Here we depart from the documented algorithm, in order to avoid
      // building up an actual levels array. Since there are only three
      // levels (0, 1, 2) in an implementation that doesn't take
      // explicit embedding into account, we can build up the order on
      // the fly, without following the level-based algorithm.
      var order = [], m;
      for (var i = 0; i < len;) {
        if (countsAsLeft.test(types[i])) {
          var start = i;
          for (++i; i < len && countsAsLeft.test(types[i]); ++i) {}
          order.push(new BidiSpan(0, start, i));
        } else {
          var pos = i, at = order.length;
          for (++i; i < len && types[i] != "L"; ++i) {}
          for (var j = pos; j < i;) {
            if (countsAsNum.test(types[j])) {
              if (pos < j) order.splice(at, 0, new BidiSpan(1, pos, j));
              var nstart = j;
              for (++j; j < i && countsAsNum.test(types[j]); ++j) {}
              order.splice(at, 0, new BidiSpan(2, nstart, j));
              pos = j;
            } else ++j;
          }
          if (pos < i) order.splice(at, 0, new BidiSpan(1, pos, i));
        }
      }
      if (order[0].level == 1 && (m = str.match(/^\s+/))) {
        order[0].from = m[0].length;
        order.unshift(new BidiSpan(0, 0, m[0].length));
      }
      if (lst(order).level == 1 && (m = str.match(/\s+$/))) {
        lst(order).to -= m[0].length;
        order.push(new BidiSpan(0, len - m[0].length, len));
      }
      if (order[0].level == 2)
        order.unshift(new BidiSpan(1, order[0].to, order[0].to));
      if (order[0].level != lst(order).level)
        order.push(new BidiSpan(order[0].level, len, len));

      return order;
    };
  })();

  // THE END

  CodeMirror.version = "5.6.0";

  return CodeMirror;
});
