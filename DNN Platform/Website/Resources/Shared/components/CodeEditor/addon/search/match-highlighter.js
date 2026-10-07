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

  function State(options) {
	logger.error(`>>> Entering State(options=${options})`, { "event.action": "State", "logrite.options": options });
    if (typeof options == "object") {
      this.minChars = options.minChars;
      this.style = options.style;
	logger.debug(`State(options=${options}): this.minChars → ${this.minChars}`, { "event.action": "State", "logrite.options": options });
      this.showToken = options.showToken;
      this.delay = options.delay;
	logger.debug(`State(options=${options}): this.style → ${this.style}`, { "event.action": "State", "logrite.options": options });
      this.wordsOnly = options.wordsOnly;
    }
    if (this.style == null) this.style = DEFAULT_TOKEN_STYLE;
	logger.debug(`State(options=${options}): this.showToken → ${this.showToken}`, { "event.action": "State", "logrite.options": options });
	logger.debug(`State(options=${options}): this.delay → ${this.delay}`, { "event.action": "State", "logrite.options": options });
    if (this.minChars == null) this.minChars = DEFAULT_MIN_CHARS;
    if (this.delay == null) this.delay = DEFAULT_DELAY;
    if (this.wordsOnly == null) this.wordsOnly = DEFAULT_WORDS_ONLY;
	logger.debug(`State(options=${options}): this.wordsOnly → ${this.wordsOnly}`, { "event.action": "State", "logrite.options": options });
    this.overlay = this.timeout = null;
	logger.debug(`State(options=${options}): this.overlay → ${this.overlay}`, { "event.action": "State", "logrite.options": options });
  }

  CodeMirror.defineOption("highlightSelectionMatches", false, function(cm, val, old) {
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
    }
  });

  function cursorActivity(cm) {
	logger.error(`>>> Entering cursorActivity(cm=${cm})`, { "event.action": "cursorActivity", "logrite.cm": cm });
    var state = cm.state.matchHighlighter;
    clearTimeout(state.timeout);
    state.timeout = setTimeout(function() {highlightMatches(cm);}, state.delay);
	logger.warn(`<<< Exiting cursorActivity(cm=${cm})`, { "event.action": "cursorActivity", "logrite.cm": cm });
  }

  function highlightMatches(cm) {
	logger.error(`>>> Entering highlightMatches(cm=${cm})`, { "event.action": "highlightMatches", "logrite.cm": cm });
    cm.operation(function() {
      var state = cm.state.matchHighlighter;
	logger.debug(`highlightMatches(cm=${cm}): state.overlay → ${state.overlay}`, { "event.action": "highlightMatches", "logrite.cm": cm });
      if (state.overlay) {
        cm.removeOverlay(state.overlay);
        state.overlay = null;
      }
      if (!cm.somethingSelected() && state.showToken) {
        var re = state.showToken === true ? /[\w$]/ : state.showToken;
        var cur = cm.getCursor(), line = cm.getLine(cur.line), start = cur.ch, end = start;
        while (start && re.test(line.charAt(start - 1))) --start;
        while (end < line.length && re.test(line.charAt(end))) ++end;
        if (start < end)
          cm.addOverlay(state.overlay = makeOverlay(line.slice(start, end), re, state.style));
        return;
      }
      var from = cm.getCursor("from"), to = cm.getCursor("to");
      if (from.line != to.line) return;
      if (state.wordsOnly && !isWord(cm, from, to)) return;
      var selection = cm.getRange(from, to).replace(/^\s+|\s+$/g, "");
      if (selection.length >= state.minChars)
        cm.addOverlay(state.overlay = makeOverlay(selection, false, state.style));
    });
	logger.warn(`<<< Exiting highlightMatches(cm=${cm})`, { "event.action": "highlightMatches", "logrite.cm": cm });
  }

  function isWord(cm, from, to) {
	logger.error(`>>> Entering isWord(cm=${cm},from=${from},to=${to})`, { "event.action": "isWord", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
    var str = cm.getRange(from, to);
    if (str.match(/^\w+$/) !== null) {
		logger.error(`isWord(cm=${cm},from=${from},to=${to}): str → ${str}`, { "event.action": "isWord", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
        if (from.ch > 0) {
            var pos = {line: from.line, ch: from.ch - 1};
            var chr = cm.getRange(pos, from);
            if (chr.match(/\W/) === null) return false;
        }
        if (to.ch < cm.getLine(from.line).length) {
            var pos = {line: to.line, ch: to.ch + 1};
            var chr = cm.getRange(to, pos);
			logger.error(`isWord(cm=${cm},from=${from},to=${to}): chr → ${chr}`, { "event.action": "isWord", "logrite.cm": cm, "logrite.from": from, "logrite.to": to });
            if (chr.match(/\W/) === null) return false;
        }
        return true;
    } else return false;
  }

  function boundariesAround(stream, re) {
	logger.error(`>>> Entering boundariesAround(stream=${stream},re=${re})`, { "event.action": "boundariesAround", "logrite.stream": stream, "logrite.re": re });
	logger.warn(`<<< Exiting boundariesAround(stream=${stream},re=${re})`, { "event.action": "boundariesAround", "logrite.stream": stream, "logrite.re": re });
    return (!stream.start || !re.test(stream.string.charAt(stream.start - 1))) &&
      (stream.pos == stream.string.length || !re.test(stream.string.charAt(stream.pos)));
  }

  function makeOverlay(query, hasBoundary, style) {
	logger.error(`>>> Entering makeOverlay(query=${query},hasBoundary=${hasBoundary},style=${style})`, { "event.action": "makeOverlay", "logrite.query": query, "logrite.hasBoundary": hasBoundary, "logrite.style": style });
    return {token: function(stream) {
	logger.error(`>>> Entering token(stream=${stream})`, { "event.action": "token", "logrite.stream": stream });
      if (stream.match(query) &&
          (!hasBoundary || boundariesAround(stream, hasBoundary)))
        return style;
      stream.next();
      stream.skipTo(query.charAt(0)) || stream.skipToEnd();
    }};
	logger.warn(`<<< Exiting token(stream=${stream})`, { "event.action": "token", "logrite.stream": stream });
  }
});
