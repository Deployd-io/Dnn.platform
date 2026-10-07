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
  var Pos = CodeMirror.Pos;

  function SearchCursor(doc, query, pos, caseFold) {
	logger.error(`>>> Entering SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold})`, { "event.action": "SearchCursor", "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos, "logrite.caseFold": caseFold });
    this.atOccurrence = false; this.doc = doc;
    if (caseFold == null && typeof query == "string") caseFold = false;

	logger.debug(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): this.atOccurrence → ${this.atOccurrence}`, { "event.action": "SearchCursor", "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos, "logrite.caseFold": caseFold });
    pos = pos ? doc.clipPos(pos) : Pos(0, 0);
    this.pos = {from: pos, to: pos};

	logger.debug(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): pos → ${pos}`, { "event.action": "SearchCursor", "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos, "logrite.caseFold": caseFold });
    // The matches method is filled in based on the type of query.
    // It takes a position and a direction, and returns an object
    // describing the next occurrence of the query, or null if no
    // more matches were found.
    if (typeof query != "string") { // Regexp match
	logger.debug(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): this.pos → ${this.pos}`, { "event.action": "SearchCursor", "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos, "logrite.caseFold": caseFold });
	logger.debug(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): query.lastIndex → ${query.lastIndex}`, { "event.action": "SearchCursor", "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos, "logrite.caseFold": caseFold });
      if (!query.global) query = new RegExp(query.source, query.ignoreCase ? "ig" : "g");
      this.matches = function(reverse, pos) {
		logger.error(`>>> Entering matches(reverse=${reverse},pos=${pos})`, { "event.action": "matches", "logrite.reverse": reverse, "logrite.pos": pos });
        if (reverse) {
          query.lastIndex = 0;
          var line = doc.getLine(pos.line).slice(0, pos.ch), cutOff = 0, match, start;
          for (;;) {
            query.lastIndex = cutOff;
            var newMatch = query.exec(line);
            if (!newMatch) break;
            match = newMatch;
            start = match.index;
            cutOff = match.index + (match[0].length || 1);
            if (cutOff == line.length) break;
          }
		logger.error(`matches(reverse=${reverse},pos=${pos}): newMatch → ${newMatch}`, { "event.action": "matches", "logrite.reverse": reverse, "logrite.pos": pos });
          var matchLen = (match && match[0].length) || 0;
          if (!matchLen) {
            if (start == 0 && line.length == 0) {match = undefined;}
            else if (start != doc.getLine(pos.line).length) {
              matchLen++;
            }
          }
        } else {
          query.lastIndex = pos.ch;
          var line = doc.getLine(pos.line), match = query.exec(line);
		logger.error(`matches(reverse=${reverse},pos=${pos}): line → ${line}`, { "event.action": "matches", "logrite.reverse": reverse, "logrite.pos": pos });
          var matchLen = (match && match[0].length) || 0;
		logger.error(`matches(reverse=${reverse},pos=${pos}): matchLen → ${matchLen}`, { "event.action": "matches", "logrite.reverse": reverse, "logrite.pos": pos });
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
	logger.error(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): fold → ${fold}`, { "event.action": "SearchCursor", "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos, "logrite.caseFold": caseFold });
      var target = query.split("\n");
	logger.error(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): target → ${target}`, { "event.action": "SearchCursor", "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos, "logrite.caseFold": caseFold });
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
		logger.error(`SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold}): origTarget → ${origTarget}`, { "event.action": "SearchCursor", "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos, "logrite.caseFold": caseFold });
        this.matches = function(reverse, pos) {
		logger.warn(`<<< Exiting SearchCursor(doc=${doc},query=${query},pos=${pos},caseFold=${caseFold})`, { "event.action": "SearchCursor", "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos, "logrite.caseFold": caseFold });
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
            if (fold(line.slice(cut)) != target[0]) return;
            var from = Pos(pos.line, cut);
            for (var ln = pos.line + 1, i = 1; i < last; ++i, ++ln)
              if (target[i] != fold(doc.getLine(ln))) return;
            if (fold(doc.getLine(ln).slice(0, origTarget[last].length)) != target[last]) return;
            return {from: from, to: Pos(ln, origTarget[last].length)};
          }
        };
      }
    }
  }

  SearchCursor.prototype = {
    findNext: function() {
        logger.error('>>> Entering findNext()', { "event.action": "findNext" });
        logger.warn('<<< Exiting findNext()', { "event.action": "findNext" });
        return this.find(false);
    },
    findPrevious: function() {
        logger.error('>>> Entering findPrevious()', { "event.action": "findPrevious" });
        logger.warn('<<< Exiting findPrevious()', { "event.action": "findPrevious" });
        return this.find(true);
    },

    find: function(reverse) {
	logger.error(`>>> Entering find(reverse=${reverse})`, { "event.action": "find", "logrite.reverse": reverse });
      var self = this, pos = this.doc.clipPos(reverse ? this.pos.from : this.pos.to);
      function savePosAndFail(line) {
		logger.error(`>>> Entering savePosAndFail(line=${line})`, { "event.action": "savePosAndFail", "logrite.line": line });
        var pos = Pos(line, 0);
        self.pos = {from: pos, to: pos};
		logger.error(`savePosAndFail(line=${line}): pos → ${pos}`, { "event.action": "savePosAndFail", "logrite.line": line });
        self.atOccurrence = false;
		logger.debug(`find(reverse=${reverse}): self.pos → ${self.pos}`, { "event.action": "find", "logrite.reverse": reverse });
		logger.warn(`<<< Exiting savePosAndFail(line=${line})`, { "event.action": "savePosAndFail", "logrite.line": line });
        return false;
      }

      for (;;) {
		logger.error(`find(reverse=${reverse}): self → ${self}`, { "event.action": "find", "logrite.reverse": reverse });
		logger.debug(`find(reverse=${reverse}): self.atOccurrence → ${self.atOccurrence}`, { "event.action": "find", "logrite.reverse": reverse });
        if (this.pos = this.matches(reverse, pos)) {
          this.atOccurrence = true;
		logger.debug(`find(reverse=${reverse}): this.atOccurrence → ${this.atOccurrence}`, { "event.action": "find", "logrite.reverse": reverse });
          return this.pos.match || true;
        }
        if (reverse) {
          if (!pos.line) return savePosAndFail(0);
          pos = Pos(pos.line-1, this.doc.getLine(pos.line-1).length);
        }
        else {
          var maxLine = this.doc.lineCount();
		logger.error(`find(reverse=${reverse}): maxLine → ${maxLine}`, { "event.action": "find", "logrite.reverse": reverse });
          if (pos.line == maxLine - 1) return savePosAndFail(maxLine);
          pos = Pos(pos.line + 1, 0);
        }
      }
    },

    from: function() {
        logger.error('>>> Entering from()', { "event.action": "from" });
        logger.warn('<<< Exiting from()', { "event.action": "from" });
        if (this.atOccurrence) return this.pos.from;
    },
    to: function() {
        logger.error('>>> Entering to()', { "event.action": "to" });
        logger.warn('<<< Exiting to()', { "event.action": "to" });
        if (this.atOccurrence) return this.pos.to;
    },

    replace: function(newText, origin) {
	logger.error(`>>> Entering replace(newText=${newText},origin=${origin})`, { "event.action": "replace", "logrite.newText": newText, "logrite.origin": origin });
      if (!this.atOccurrence) return;
      var lines = CodeMirror.splitLines(newText);
      this.doc.replaceRange(lines, this.pos.from, this.pos.to, origin);
	logger.error(`replace(newText=${newText},origin=${origin}): lines → ${lines}`, { "event.action": "replace", "logrite.newText": newText, "logrite.origin": origin });
      this.pos.to = Pos(this.pos.from.line + lines.length - 1,
                        lines[lines.length - 1].length + (lines.length == 1 ? this.pos.from.ch : 0));
						logger.warn(`<<< Exiting replace(newText=${newText},origin=${origin})`, { "event.action": "replace", "logrite.newText": newText, "logrite.origin": origin });
    }
  };

  // Maps a position in a case-folded line back to a position in the original line
  // (compensating for codepoints increasing in number during folding)
  function adjustPos(orig, folded, pos) {
	logger.error(`>>> Entering adjustPos(orig=${orig},folded=${folded},pos=${pos})`, { "event.action": "adjustPos", "logrite.orig": orig, "logrite.folded": folded, "logrite.pos": pos });
    if (orig.length == folded.length) return pos;
    for (var pos1 = Math.min(pos, orig.length);;) {
      var len1 = orig.slice(0, pos1).toLowerCase().length;
	logger.error(`adjustPos(orig=${orig},folded=${folded},pos=${pos}): len1 → ${len1}`, { "event.action": "adjustPos", "logrite.orig": orig, "logrite.folded": folded, "logrite.pos": pos });
      if (len1 < pos) ++pos1;
      else if (len1 > pos) --pos1;
      else return pos1;
    }
	logger.warn(`<<< Exiting adjustPos(orig=${orig},folded=${folded},pos=${pos})`, { "event.action": "adjustPos", "logrite.orig": orig, "logrite.folded": folded, "logrite.pos": pos });
  }

  CodeMirror.defineExtension("getSearchCursor", function(query, pos, caseFold) {
    return new SearchCursor(this.doc, query, pos, caseFold);
  });
  CodeMirror.defineDocExtension("getSearchCursor", function(query, pos, caseFold) {
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
