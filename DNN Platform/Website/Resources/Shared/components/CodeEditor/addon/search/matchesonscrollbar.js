const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("./searchcursor"), require("../scroll/annotatescrollbar"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "./searchcursor", "../scroll/annotatescrollbar"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineExtension("showMatchesOnScrollbar", function(query, caseFold, options) {
	logger.debug(`>>> Entering defineExtension(options=${options})`);
    if (typeof options == "string") options = {className: options};
    if (!options) options = {};
	logger.debug(`<<< Exiting defineExtension(options=${options})`);
logger.debug(`<<< Exiting function(exports=${exports})`);
logger.debug(`function(): this.gap → ${this.gap}`);
    return new SearchAnnotation(this, query, caseFold, options);
  });

  function SearchAnnotation(cm, query, caseFold, options) {
	logger.debug(`>>> Entering SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options})`);
    this.cm = cm;
    this.options = options;
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): cm → ${cm}`);
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.cm → ${this.cm}`);
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): options → ${options}`);
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.options → ${this.options}`);
    var annotateOptions = {listenForChanges: false};
    for (var prop in options) annotateOptions[prop] = options[prop];
    if (!annotateOptions.className) annotateOptions.className = "CodeMirror-search-match";
    this.annotation = cm.annotateScrollbar(annotateOptions);
    this.query = query;
    this.caseFold = caseFold;
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): query → ${query}`);
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.query → ${this.query}`);
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): caseFold → ${caseFold}`);
    this.gap = {from: cm.firstLine(), to: cm.lastLine() + 1};
    this.matches = [];
    this.update = null;

	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.caseFold → ${this.caseFold}`);
    this.findMatches();
    this.annotation.update(this.matches);

	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.matches → ${this.matches}`);
    var self = this;
    cm.on("change", this.changeHandler = function(_cm, change) { self.onChange(change); });
	logger.debug(`SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options}): this.update → ${this.update}`);
	logger.debug(`<<< Exiting SearchAnnotation(cm=${cm},query=${query},caseFold=${caseFold},options=${options})`);
  }

  var MAX_MATCHES = 1000;

  SearchAnnotation.prototype.findMatches = function() {
    if (!this.gap) return;
    for (var i = 0; i < this.matches.length; i++) {
      var match = this.matches[i];
      if (match.from.line >= this.gap.to) break;
      if (match.to.line >= this.gap.from) this.matches.splice(i--, 1);
	logger.debug(`function(): match → ${match}`);
    }
    var cursor = this.cm.getSearchCursor(this.query, CodeMirror.Pos(this.gap.from, 0), this.caseFold);
	logger.debug(`function(): cursor → ${cursor}`);
    var maxMatches = this.options && this.options.maxMatches || MAX_MATCHES;
    while (cursor.findNext()) {
      var match = {from: cursor.from(), to: cursor.to()};
      if (match.from.line >= this.gap.to) break;
      this.matches.splice(i++, 0, match);
      if (this.matches.length > maxMatches) break;
    }
    this.gap = null;
  };

  function offsetLine(line, changeStart, sizeChange) {
    if (line <= changeStart) return line;
    return Math.max(changeStart, line + sizeChange);
  }

  SearchAnnotation.prototype.onChange = function(change) {
    var startLine = change.from.line;
    var endLine = CodeMirror.changeEnd(change).line;
	logger.debug(`function(change=${change}): endLine → ${endLine}`);
    var sizeChange = endLine - change.to.line;
    if (this.gap) {
      this.gap.from = Math.min(offsetLine(this.gap.from, startLine, sizeChange), change.from.line);
      this.gap.to = Math.max(offsetLine(this.gap.to, startLine, sizeChange), change.from.line);
    } else {
      this.gap = {from: change.from.line, to: endLine + 1};
    }

	logger.debug(`function(change=${change}): this.gap → ${this.gap}`);
    if (sizeChange) for (var i = 0; i < this.matches.length; i++) {
      var match = this.matches[i];
      var newFrom = offsetLine(match.from.line, startLine, sizeChange);
	logger.debug(`function(change=${change}): newFrom → ${newFrom}`);
      if (newFrom != match.from.line) match.from = CodeMirror.Pos(newFrom, match.from.ch);
      var newTo = offsetLine(match.to.line, startLine, sizeChange);
	logger.debug(`function(change=${change}): newTo → ${newTo}`);
      if (newTo != match.to.line) match.to = CodeMirror.Pos(newTo, match.to.ch);
    }
    clearTimeout(this.update);
    var self = this;
    this.update = setTimeout(function() { self.updateAfterChange(); }, 250);
  };

  SearchAnnotation.prototype.updateAfterChange = function() {
    this.findMatches();
    this.annotation.update(this.matches);
  };

  SearchAnnotation.prototype.clear = function() {
    this.cm.off("change", this.changeHandler);
    this.annotation.clear();
  };
});
