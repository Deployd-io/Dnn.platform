const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("./searchcursor"), require("../scroll/annotatescrollbar"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "./searchcursor", "../scroll/annotatescrollbar"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineExtension("showMatchesOnScrollbar", function(query, caseFold, options) {
    if (typeof options == "string") options = {className: options};
    if (!options) options = {};
    return new SearchAnnotation(this, query, caseFold, options);
  });

  function SearchAnnotation(cm, query, caseFold, options) {
	logger.error(`>>> Entering SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options})`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
    this.cm = cm;
    this.options = options;
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): cm → ${cm}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.cm → ${this.cm}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): options → ${options}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.options → ${this.options}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
    var annotateOptions = {listenForChanges: false};
    for (var prop in options) annotateOptions[prop] = options[prop];
    if (!annotateOptions.className) annotateOptions.className = "CodeMirror-search-match";
    this.annotation = cm.annotateScrollbar(annotateOptions);
    this.query = query;
    this.caseFold = caseFold;
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): query → ${query}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.query → ${this.query}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): caseFold → ${caseFold}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.caseFold → ${this.caseFold}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
    this.gap = {from: cm.firstLine(), to: cm.lastLine() + 1};
    this.matches = [];
    this.update = null;

    this.findMatches();
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.matches → ${this.matches}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
    this.annotation.update(this.matches);

	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.update → ${this.update}`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
    var self = this;
	logger.warn(`<<< Exiting SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options})`, { "event.action": "SearchAnnotation", "logrite.cm": cm, "logrite.query": query, "logrite.caseFold": caseFold, "logrite.options": options });
    cm.on("change", this.changeHandler = function(_cm, change) { self.onChange(change); });
	logger.error(`>>> Entering changeHandler(_cm=${_cm},change=${change})`, { "event.action": "changeHandler", "logrite._cm": _cm, "logrite.change": change });
  }

  var MAX_MATCHES = 1000;

  SearchAnnotation.prototype.findMatches = function() {
	logger.error('>>> Entering findMatches()', { "event.action": "findMatches" });
    if (!this.gap) return;
    for (var i = 0; i < this.matches.length; i++) {
      var match = this.matches[i];
      if (match.from.line >= this.gap.to) break;
      if (match.to.line >= this.gap.from) this.matches.splice(i--, 1);
    }
    var cursor = this.cm.getSearchCursor(this.query, CodeMirror.Pos(this.gap.from, 0), this.caseFold);
	logger.error(`findMatches(): cursor → ${cursor}`, { "event.action": "findMatches" });
    var maxMatches = this.options && this.options.maxMatches || MAX_MATCHES;
    while (cursor.findNext()) {
      var match = {from: cursor.from(), to: cursor.to()};
	logger.error(`findMatches(): match → ${match}`, { "event.action": "findMatches" });
      if (match.from.line >= this.gap.to) break;
      this.matches.splice(i++, 0, match);
      if (this.matches.length > maxMatches) break;
    }
    this.gap = null;
	logger.debug(`findMatches(): this.gap → ${this.gap}`, { "event.action": "findMatches" });
  };

  function offsetLine(line, changeStart, sizeChange) {
	logger.error(`>>> Entering offsetLine(line=${line},changeStart=${changeStart},sizeChange=${sizeChange})`, { "event.action": "offsetLine", "logrite.line": line, "logrite.changeStart": changeStart, "logrite.sizeChange": sizeChange });
    if (line <= changeStart) return line;
	logger.warn(`<<< Exiting offsetLine(line=${line},changeStart=${changeStart},sizeChange=${sizeChange})`, { "event.action": "offsetLine", "logrite.line": line, "logrite.changeStart": changeStart, "logrite.sizeChange": sizeChange });
    return Math.max(changeStart, line + sizeChange);
  }

  SearchAnnotation.prototype.onChange = function(change) {
    var startLine = change.from.line;
    var endLine = CodeMirror.changeEnd(change).line;
    var sizeChange = endLine - change.to.line;
    if (this.gap) {
      this.gap.from = Math.min(offsetLine(this.gap.from, startLine, sizeChange), change.from.line);
      this.gap.to = Math.max(offsetLine(this.gap.to, startLine, sizeChange), change.from.line);
    } else {
      this.gap = {from: change.from.line, to: endLine + 1};
    }

    if (sizeChange) for (var i = 0; i < this.matches.length; i++) {
      var match = this.matches[i];
      var newFrom = offsetLine(match.from.line, startLine, sizeChange);
      if (newFrom != match.from.line) match.from = CodeMirror.Pos(newFrom, match.from.ch);
      var newTo = offsetLine(match.to.line, startLine, sizeChange);
      if (newTo != match.to.line) match.to = CodeMirror.Pos(newTo, match.to.ch);
    }
    clearTimeout(this.update);
    var self = this;
    this.update = setTimeout(function() { self.updateAfterChange(); }, 250);
  };

  SearchAnnotation.prototype.updateAfterChange = function() {
	logger.error('>>> Entering updateAfterChange()', { "event.action": "updateAfterChange" });
    this.findMatches();
    this.annotation.update(this.matches);
	logger.warn('<<< Exiting updateAfterChange()', { "event.action": "updateAfterChange" });
  };

  SearchAnnotation.prototype.clear = function() {
	logger.error('>>> Entering clear()', { "event.action": "clear" });
    this.cm.off("change", this.changeHandler);
    this.annotation.clear();
	logger.warn('<<< Exiting clear()', { "event.action": "clear" });
  };
});
