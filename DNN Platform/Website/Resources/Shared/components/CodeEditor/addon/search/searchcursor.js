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
  var Pos = CodeMirror.Pos;
logger.debug(`<<< Exiting function(exports=${exports})`);

  function SearchCursor(doc, query, pos, caseFold) {
	logger.debug(`>>> Entering SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold})`);
    this.atOccurrence = false; this.doc = doc;
    if (caseFold == null && typeof query == "string") caseFold = false;

	logger.debug(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): this.atOccurrence → ${this.atOccurrence}`);
    pos = pos ? doc.clipPos(pos) : Pos(0, 0);
    this.pos = {from: pos, to: pos};

	logger.debug(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): pos → ${pos}`);
    // The matches method is filled in based on the type of query.
    // It takes a position and a direction, and returns an object
    // describing the next occurrence of the query, or null if no
    // more matches were found.
    if (typeof query != "string") { // Regexp match
	logger.debug(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): this.pos → ${this.pos}`);
      if (!query.global) query = new RegExp(query.source, query.ignoreCase ? "ig" : "g");
      this.matches = function(reverse, pos) {
        if (reverse) {
          query.lastIndex = 0;
		logger.debug(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): query.lastIndex → ${query.lastIndex}`);
          var line = doc.getLine(pos.line).slice(0, pos.ch), cutOff = 0, match, start;
          for (;;) {
            query.lastIndex = cutOff;
            var newMatch = query.exec(line);
			logger.debug(`function(reverse=${reverse},pos=${pos}): newMatch → ${newMatch}`);
            if (!newMatch) break;
            match = newMatch;
            start = match.index;
			logger.debug(`function(reverse=${reverse},pos=${pos}): query.lastIndex → ${query.lastIndex}`);
            cutOff = match.index + (match[0].length || 1);
			logger.debug(`function(reverse=${reverse},pos=${pos}): cutOff → ${cutOff}`);
            if (cutOff == line.length) break;
          }
          var matchLen = (match && match[0].length) || 0;
		logger.debug(`function(reverse=${reverse},pos=${pos}): matchLen → ${matchLen}`);
          if (!matchLen) {
            if (start == 0 && line.length == 0) {match = undefined;}
            else if (start != doc.getLine(pos.line).length) {
              matchLen++;
            }
          }
        } else {
          query.lastIndex = pos.ch;
          var line = doc.getLine(pos.line), match = query.exec(line);
          var matchLen = (match && match[0].length) || 0;
          var start = match && match.index;
          if (start + matchLen != line.length && !matchLen) matchLen = 1;
        }
        if (match && matchLen)
          return {from: Pos(pos.line, start),
                  to: Pos(pos.line, start + matchLen),
                  match: match};
      };
    } else { // String query
      var origQuery = query;
      if (caseFold) query = query.toLowerCase();
      var fold = caseFold ? function(str){return str.toLowerCase();} : function(str){return str;};
      var target = query.split("\n");
	logger.debug(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): target → ${target}`);
      // Different methods for single-line and multi-line queries
      if (target.length == 1) {
        if (!query.length) {
          // Empty string would match anything and never progress, so
          // we define it to match nothing instead.
          this.matches = function() {};
        } else {
          this.matches = function(reverse, pos) {
            if (reverse) {
              var orig = doc.getLine(pos.line).slice(0, pos.ch), line = fold(orig);
              var match = line.lastIndexOf(query);
			logger.debug(`function(reverse=${reverse},pos=${pos}): orig → ${orig}`);
			logger.debug(`function(reverse=${reverse},pos=${pos}): match → ${match}`);
              if (match > -1) {
                match = adjustPos(orig, line, match);
                return {from: Pos(pos.line, match), to: Pos(pos.line, match + origQuery.length)};
              }
             } else {
               var orig = doc.getLine(pos.line).slice(pos.ch), line = fold(orig);
               var match = line.indexOf(query);
               if (match > -1) {
                 match = adjustPos(orig, line, match) + pos.ch;
                 return {from: Pos(pos.line, match), to: Pos(pos.line, match + origQuery.length)};
               }
            }
          };
        }
      } else {
        var origTarget = origQuery.split("\n");
        this.matches = function(reverse, pos) {
          var last = target.length - 1;
          if (reverse) {
            if (pos.line - (target.length - 1) < doc.firstLine()) return;
            if (fold(doc.getLine(pos.line).slice(0, origTarget[last].length)) != target[target.length - 1]) return;
            var to = Pos(pos.line, origTarget[last].length);
            for (var ln = pos.line - 1, i = last - 1; i >= 1; --i, --ln)
              if (target[i] != fold(doc.getLine(ln))) return;
            var line = doc.getLine(ln), cut = line.length - origTarget[0].length;
            if (fold(line.slice(cut)) != target[0]) return;
            return {from: Pos(ln, cut), to: to};
          } else {
            if (pos.line + (target.length - 1) > doc.lastLine()) return;
            var line = doc.getLine(pos.line), cut = line.length - origTarget[0].length;
			logger.debug(`function(reverse=${reverse},pos=${pos}): line → ${line}`);
            if (fold(line.slice(cut)) != target[0]) return;
            var from = Pos(pos.line, cut);
            for (var ln = pos.line + 1, i = 1; i < last; ++i, ++ln)
              if (target[i] != fold(doc.getLine(ln))) return;
            if (fold(doc.getLine(ln).slice(0, origTarget[last].length)) != target[last]) return;
			logger.debug(`<<< Exiting SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold})`);
            return {from: from, to: Pos(ln, origTarget[last].length)};
          }
        };
      }
    }
  }

  SearchCursor.prototype = {
    findNext: function() {return this.find(false);},
    findPrevious: function() {return this.find(true);},

    find: function(reverse) {
	logger.debug(`>>> Entering find(reverse=${reverse})`);
      var self = this, pos = this.doc.clipPos(reverse ? this.pos.from : this.pos.to);
      function savePosAndFail(line) {
		logger.debug(`>>> Entering savePosAndFail(line=${line})`);
        var pos = Pos(line, 0);
		logger.debug(`find(reverse=${reverse}): self → ${self}`);
        self.pos = {from: pos, to: pos};
		logger.debug(`savePosAndFail(line=${line}): pos → ${pos}`);
        self.atOccurrence = false;
		logger.debug(`<<< Exiting savePosAndFail(line=${line})`);
	logger.debug(`find(reverse=${reverse}): self.pos → ${self.pos}`);
        return false;
      }

      for (;;) {
        if (this.pos = this.matches(reverse, pos)) {
          this.atOccurrence = true;
		logger.debug(`find(reverse=${reverse}): this.atOccurrence → ${this.atOccurrence}`);
          return this.pos.match || true;
        }
        if (reverse) {
		logger.debug(`find(reverse=${reverse}): self.atOccurrence → ${self.atOccurrence}`);
          if (!pos.line) return savePosAndFail(0);
          pos = Pos(pos.line-1, this.doc.getLine(pos.line-1).length);
        }
        else {
          var maxLine = this.doc.lineCount();
		logger.debug(`find(reverse=${reverse}): maxLine → ${maxLine}`);
          if (pos.line == maxLine - 1) return savePosAndFail(maxLine);
          pos = Pos(pos.line + 1, 0);
        }
      }
	logger.debug(`<<< Exiting find(reverse=${reverse})`);
    },

    from: function() {if (this.atOccurrence) return this.pos.from;},
    to: function() {if (this.atOccurrence) return this.pos.to;},

    replace: function(newText, origin) {
	logger.debug(`>>> Entering replace(newText=${newText},origin=${origin})`);
      if (!this.atOccurrence) return;
      var lines = CodeMirror.splitLines(newText);
	logger.debug(`replace(newText=${newText},origin=${origin}): lines → ${lines}`);
      this.doc.replaceRange(lines, this.pos.from, this.pos.to, origin);
      this.pos.to = Pos(this.pos.from.line + lines.length - 1,
                        lines[lines.length - 1].length + (lines.length == 1 ? this.pos.from.ch : 0));
						logger.debug(`<<< Exiting replace(newText=${newText},origin=${origin})`);
    }
  };

  // Maps a position in a case-folded line back to a position in the original line
  // (compensating for codepoints increasing in number during folding)
  function adjustPos(orig, folded, pos) {
	logger.debug(`>>> Entering adjustPos(orig=${orig},folded=${folded},pos=${pos})`);
    if (orig.length == folded.length) return pos;
    for (var pos1 = Math.min(pos, orig.length);;) {
      var len1 = orig.slice(0, pos1).toLowerCase().length;
	logger.debug(`adjustPos(orig=${orig},folded=${folded},pos=${pos}): len1 → ${len1}`);
      if (len1 < pos) ++pos1;
      else if (len1 > pos) --pos1;
      else return pos1;
	logger.debug(`<<< Exiting adjustPos(orig=${orig},folded=${folded},pos=${pos})`);
    }
  }

  CodeMirror.defineExtension("getSearchCursor", function(query, pos, caseFold) {
	logger.debug('>>> Entering defineExtension()');
	logger.debug('<<< Exiting defineExtension()');
	logger.debug(`<<< Exiting defineExtension(ranges=${ranges})`);
    return new SearchCursor(this.doc, query, pos, caseFold);
  });
  CodeMirror.defineDocExtension("getSearchCursor", function(query, pos, caseFold) {
	logger.debug('>>> Entering defineDocExtension()');
logger.debug('<<< Exiting defineDocExtension()');
    return new SearchCursor(this, query, pos, caseFold);
  });

  CodeMirror.defineExtension("selectMatches", function(query, caseFold) {
    var ranges = [];
    var cur = this.getSearchCursor(query, this.getCursor("from"), caseFold);
    while (cur.findNext()) {
      if (CodeMirror.cmpPos(cur.to(), this.getCursor("to")) > 0) break;
      ranges.push({anchor: cur.from(), head: cur.to()});
    }
    if (ranges.length)
      this.setSelections(ranges, 0);
  });
});
