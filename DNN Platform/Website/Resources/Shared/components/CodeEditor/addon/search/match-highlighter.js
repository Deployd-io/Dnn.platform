const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Highlighting text that matches the selection
//
// Defines an option highlightSelectionMatches, which, when enabled,
// will style strings that match the selection throughout the
// document.
//
// The option can be set to true to simply enable it, or to a
// {minChars, style, wordsOnly, showToken, delay} object to explicitly
// configure it. minChars is the minimum amount of characters that should be
// selected for the behavior to occur, and style is the token style to
// apply to the matches. This will be prefixed by "cm-" to create an
// actual CSS class name. If wordsOnly is enabled, the matches will be
// highlighted only if the selected text is a word. showToken, when enabled,
// will cause the current token to be highlighted when nothing is selected.
// delay is used to specify how much time to wait, in milliseconds, before
// highlighting the matches.

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

  var DEFAULT_MIN_CHARS = 2;
  var DEFAULT_TOKEN_STYLE = "matchhighlight";
  var DEFAULT_DELAY = 100;
  var DEFAULT_WORDS_ONLY = false;
logger.debug(`<<< Exiting function(exports=${exports})`);

  function State(options) {
	logger.debug(`>>> Entering State(options=${options})`);
    if (typeof options == "object") {
      this.minChars = options.minChars;
      this.style = options.style;
	logger.debug(`State(options=${options}): this.minChars → ${this.minChars}`);
      this.showToken = options.showToken;
      this.delay = options.delay;
	logger.debug(`State(options=${options}): this.style → ${this.style}`);
      this.wordsOnly = options.wordsOnly;
    }
    if (this.style == null) this.style = DEFAULT_TOKEN_STYLE;
	logger.debug(`State(options=${options}): this.showToken → ${this.showToken}`);
	logger.debug(`State(options=${options}): this.delay → ${this.delay}`);
    if (this.minChars == null) this.minChars = DEFAULT_MIN_CHARS;
    if (this.delay == null) this.delay = DEFAULT_DELAY;
    if (this.wordsOnly == null) this.wordsOnly = DEFAULT_WORDS_ONLY;
	logger.debug(`State(options=${options}): this.wordsOnly → ${this.wordsOnly}`);
    this.overlay = this.timeout = null;
	logger.debug(`State(options=${options}): this.overlay → ${this.overlay}`);
	logger.debug(`<<< Exiting State(options=${options})`);
  }

  CodeMirror.defineOption("highlightSelectionMatches", false, function(cm, val, old) {
	logger.debug(`>>> Entering defineOption(false=${false})`);
    if (old && old != CodeMirror.Init) {
      var over = cm.state.matchHighlighter.overlay;
      if (over) cm.removeOverlay(over);
      clearTimeout(cm.state.matchHighlighter.timeout);
      cm.state.matchHighlighter = null;
      cm.off("cursorActivity", cursorActivity);
    }
    if (val) {
      cm.state.matchHighlighter = new State(val);
      highlightMatches(cm);
      cm.on("cursorActivity", cursorActivity);
	logger.debug(`<<< Exiting defineOption(false=${false})`);
    }
  });

  function cursorActivity(cm) {
	logger.debug(`>>> Entering cursorActivity(cm=${cm})`);
    var state = cm.state.matchHighlighter;
    clearTimeout(state.timeout);
    state.timeout = setTimeout(function() {highlightMatches(cm);}, state.delay);
	logger.debug(`<<< Exiting cursorActivity(cm=${cm})`);
  }

  function highlightMatches(cm) {
	logger.debug(`>>> Entering highlightMatches(cm=${cm})`);
    cm.operation(function() {
      var state = cm.state.matchHighlighter;
      if (state.overlay) {
        cm.removeOverlay(state.overlay);
        state.overlay = null;
		logger.debug(`highlightMatches(cm=${cm}): state.overlay → ${state.overlay}`);
      }
      if (!cm.somethingSelected() && state.showToken) {
		logger.debug(`operation(state=${state}): state.overlay → ${state.overlay}`);
        var re = state.showToken === true ? /[\w$]/ : state.showToken;
        var cur = cm.getCursor(), line = cm.getLine(cur.line), start = cur.ch, end = start;
		logger.debug(`operation(state=${state}): cur → ${cur}`);
        while (start && re.test(line.charAt(start - 1))) --start;
        while (end < line.length && re.test(line.charAt(end))) ++end;
        if (start < end)
          cm.addOverlay(state.overlay = makeOverlay(line.slice(start, end), re, state.style));
        return;
      }
      var from = cm.getCursor("from"), to = cm.getCursor("to");
	logger.debug(`operation(state=${state}): from → ${from}`);
      if (from.line != to.line) return;
      if (state.wordsOnly && !isWord(cm, from, to)) return;
      var selection = cm.getRange(from, to).replace(/^\s+|\s+$/g, "");
      if (selection.length >= state.minChars)
		logger.debug(`operation(state=${state}): selection → ${selection}`);
        cm.addOverlay(state.overlay = makeOverlay(selection, false, state.style));
    });
	logger.debug(`<<< Exiting highlightMatches(cm=${cm})`);
  }

  function isWord(cm, from, to) {
	logger.debug(`>>> Entering isWord(cm=${cm},from=${from},to=${to})`);
    var str = cm.getRange(from, to);
    if (str.match(/^\w+$/) !== null) {
		logger.debug(`isWord(cm=${cm},from=${from},to=${to}): str → ${str}`);
        if (from.ch > 0) {
            var pos = {line: from.line, ch: from.ch - 1};
            var chr = cm.getRange(pos, from);
            if (chr.match(/\W/) === null) return false;
        }
        if (to.ch < cm.getLine(from.line).length) {
            var pos = {line: to.line, ch: to.ch + 1};
            var chr = cm.getRange(to, pos);
			logger.debug(`isWord(cm=${cm},from=${from},to=${to}): chr → ${chr}`);
            if (chr.match(/\W/) === null) return false;
        }
        return true;
    } else return false;
	logger.debug(`<<< Exiting isWord(cm=${cm},from=${from},to=${to})`);
  }

  function boundariesAround(stream, re) {
	logger.debug(`>>> Entering boundariesAround(stream=${stream},re=${re})`);
	logger.debug(`<<< Exiting boundariesAround(stream=${stream},re=${re})`);
    return (!stream.start || !re.test(stream.string.charAt(stream.start - 1))) &&
      (stream.pos == stream.string.length || !re.test(stream.string.charAt(stream.pos)));
  }

  function makeOverlay(query, hasBoundary, style) {
	logger.debug(`>>> Entering makeOverlay(query=${query},hasBoundary=${hasBoundary},style=${style})`);
    return {token: function(stream) {
	logger.debug(`>>> Entering token(stream=${stream})`);
      if (stream.match(query) &&
          (!hasBoundary || boundariesAround(stream, hasBoundary)))
		logger.debug(`<<< Exiting token(stream=${stream})`);
        return style;
      stream.next();
      stream.skipTo(query.charAt(0)) || stream.skipToEnd();
    }};
  }
});
