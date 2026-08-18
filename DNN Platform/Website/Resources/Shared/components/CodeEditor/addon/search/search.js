const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Define search commands. Depends on dialog.js or another
// implementation of the openDialog method.

// Replace works a little oddly -- it will do the replace on the next
// Ctrl-G (or whatever is bound to findNext) press. You prevent a
// replace by making sure the match is no longer selected when hitting
// Ctrl-G.

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("./searchcursor"), require("../dialog/dialog"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "./searchcursor", "../dialog/dialog"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  function searchOverlay(query, caseInsensitive) {
    if (typeof query == "string")
      query = new RegExp(query.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&"), caseInsensitive ? "gi" : "g");
    else if (!query.global)
      query = new RegExp(query.source, query.ignoreCase ? "gi" : "g");

    return {token: function(stream) {
	logger.debug(`>>> Entering token(stream=${stream})`);
      query.lastIndex = stream.pos;
	logger.debug(`<<< Exiting token(stream=${stream})`);
      var match = query.exec(stream.string);
      if (match && match.index == stream.pos) {
        stream.pos += match[0].length;
        return "searching";
      } else if (match) {
        stream.pos = match.index;
      } else {
        stream.skipToEnd();
      }
    }};
  }

  function SearchState() {
	logger.debug('>>> Entering SearchState()');
    this.posFrom = this.posTo = this.lastQuery = this.query = null;
    this.overlay = null;
	logger.debug('<<< Exiting SearchState()');
  }

  function getSearchState(cm) {
	logger.debug(`>>> Entering getSearchState(cm=${cm})`);
	logger.debug(`<<< Exiting getSearchState(cm=${cm})`);
    return cm.state.search || (cm.state.search = new SearchState());
  }

  function queryCaseInsensitive(query) {
    return typeof query == "string" && query == query.toLowerCase();
  }

  function getSearchCursor(cm, query, pos) {
    // Heuristic: if the query string is all lowercase, do a case insensitive search.
    return cm.getSearchCursor(query, pos, queryCaseInsensitive(query));
  }

  function persistentDialog(cm, text, deflt, f) {
	logger.debug(`>>> Entering persistentDialog(cm=${cm},text=${text},deflt=${deflt},f=${f})`);
    cm.openDialog(text, f, {
      value: deflt,
      selectValueOnOpen: true,
      closeOnEnter: false,
      onClose: function() { clearSearch(cm); }
    });
	logger.debug(`<<< Exiting persistentDialog(cm=${cm},text=${text},deflt=${deflt},f=${f})`);
  }

  function dialog(cm, text, shortText, deflt, f) {
	logger.debug(`>>> Entering dialog(cm=${cm},text=${text},shortText=${shortText},deflt=${deflt},f=${f})`);
    if (cm.openDialog) cm.openDialog(text, f, {value: deflt, selectValueOnOpen: true});
    else f(prompt(shortText, deflt));
	logger.debug(`<<< Exiting dialog(cm=${cm},text=${text},shortText=${shortText},deflt=${deflt},f=${f})`);
  }

  function confirmDialog(cm, text, shortText, fs) {
	logger.debug(`>>> Entering confirmDialog(cm=${cm},text=${text},shortText=${shortText},fs=${fs})`);
    if (cm.openConfirm) cm.openConfirm(text, fs);
    else if (confirm(shortText)) fs[0]();
	logger.debug(`<<< Exiting confirmDialog(cm=${cm},text=${text},shortText=${shortText},fs=${fs})`);
  }

  function parseString(string) {
	logger.debug(`>>> Entering parseString(string=${string})`);
    return string.replace(/\\(.)/g, function(_, ch) {
	logger.debug('>>> Entering function()');
      if (ch == "n") return "\n"
      if (ch == "r") return "\r"
	logger.debug('<<< Exiting function()');
	logger.debug(`<<< Exiting parseString(string=${string})`);
      return ch
    })
  }

  function parseQuery(query) {
    var isRE = query.match(/^\/(.*)\/([a-z]*)$/);
    if (isRE) {
      try { query = new RegExp(isRE[1], isRE[2].indexOf("i") == -1 ? "" : "i"); }
      catch(e) {} // Not a regular expression after all, do a string search
    } else {
      query = parseString(query)
    }
    if (typeof query == "string" ? query == "" : query.test(""))
      query = /x^/;
	logger.debug(`<<< Exiting parseQuery(query=${query})`);
    return query;
  }

  var queryDialog =
    'Search: <input type="text" style="width: 10em" class="CodeMirror-search-field"/> <span style="color: #888" class="CodeMirror-search-hint">(Use /re/ syntax for regexp search)</span>';

  function startSearch(cm, state, query) {
    state.queryText = query;
    state.query = parseQuery(query);
    cm.removeOverlay(state.overlay, queryCaseInsensitive(state.query));
    state.overlay = searchOverlay(state.query, queryCaseInsensitive(state.query));
    cm.addOverlay(state.overlay);
    if (cm.showMatchesOnScrollbar) {
      if (state.annotate) { state.annotate.clear(); state.annotate = null; }
      state.annotate = cm.showMatchesOnScrollbar(state.query, queryCaseInsensitive(state.query));
    }
	logger.debug(`<<< Exiting startSearch(cm=${cm},state=${state},query=${query})`);
  }

  function doSearch(cm, rev, persistent) {
	logger.debug(`>>> Entering doSearch(cm=${cm},rev=${rev},persistent=${persistent})`);
    var state = getSearchState(cm);
    if (state.query) return findNext(cm, rev);
    var q = cm.getSelection() || state.lastQuery;
	logger.debug(`<<< Exiting doSearch(cm=${cm},rev=${rev},persistent=${persistent})`);
    if (persistent && cm.openDialog) {
      persistentDialog(cm, queryDialog, q, function(query, event) {
        CodeMirror.e_stop(event);
        if (!query) return;
        if (query != state.queryText) startSearch(cm, state, query);
        findNext(cm, event.shiftKey);
      });
    } else {
      dialog(cm, queryDialog, "Search for:", q, function(query) {
		logger.debug(`>>> Entering dialog(cm=${cm},queryDialog=${queryDialog},q=${q})`);
        if (query && !state.query) cm.operation(function() {
		logger.debug('>>> Entering operation()');
          startSearch(cm, state, query);
          state.posFrom = state.posTo = cm.getCursor();
          findNext(cm, rev);
		logger.debug('<<< Exiting operation()');
        });
      });
    }
  }

  function findNext(cm, rev) {cm.operation(function() {
	logger.debug(`>>> Entering findNext(cm=${cm},rev=${rev})`);
    var state = getSearchState(cm);
    var cursor = getSearchCursor(cm, state.query, rev ? state.posFrom : state.posTo);
    if (!cursor.find(rev)) {
      cursor = getSearchCursor(cm, state.query, rev ? CodeMirror.Pos(cm.lastLine()) : CodeMirror.Pos(cm.firstLine(), 0));
      if (!cursor.find(rev)) return;
    }
    cm.setSelection(cursor.from(), cursor.to());
    cm.scrollIntoView({from: cursor.from(), to: cursor.to()}, 20);
	logger.debug(`<<< Exiting findNext(cm=${cm},rev=${rev})`);
    state.posFrom = cursor.from(); state.posTo = cursor.to();
  });}

  function clearSearch(cm) {cm.operation(function() {
	logger.debug(`>>> Entering clearSearch(cm=${cm})`);
    var state = getSearchState(cm);
    state.lastQuery = state.query;
    if (!state.query) return;
    state.query = state.queryText = null;
    cm.removeOverlay(state.overlay);
	logger.debug(`<<< Exiting clearSearch(cm=${cm})`);
    if (state.annotate) { state.annotate.clear(); state.annotate = null; }
  });}

  var replaceQueryDialog =
    'Replace: <input type="text" style="width: 10em" class="CodeMirror-search-field"/> <span style="color: #888" class="CodeMirror-search-hint">(Use /re/ syntax for regexp search)</span>';
  var replacementQueryDialog = 'With: <input type="text" style="width: 10em" class="CodeMirror-search-field"/>';
  var doReplaceConfirm = "Replace? <button>Yes</button> <button>No</button> <button>Stop</button>";

  function replace(cm, all) {
    if (cm.getOption("readOnly")) return;
    var query = cm.getSelection() || getSearchState(cm).lastQuery;
    dialog(cm, replaceQueryDialog, "Replace:", query, function(query) {
      if (!query) return;
      query = parseQuery(query);
      dialog(cm, replacementQueryDialog, "Replace with:", "", function(text) {
        text = parseString(text)
        if (all) {
          cm.operation(function() {
            for (var cursor = getSearchCursor(cm, query); cursor.findNext();) {
              if (typeof query != "string") {
                var match = cm.getRange(cursor.from(), cursor.to()).match(query);
                cursor.replace(text.replace(/\$(\d)/g, function(_, i) {return match[i];}));
              } else cursor.replace(text);
            }
          });
        } else {
          clearSearch(cm);
          var cursor = getSearchCursor(cm, query, cm.getCursor());
          var advance = function() {
            var start = cursor.from(), match;
            if (!(match = cursor.findNext())) {
              cursor = getSearchCursor(cm, query);
              if (!(match = cursor.findNext()) ||
                  (start && cursor.from().line == start.line && cursor.from().ch == start.ch)) return;
            }
            cm.setSelection(cursor.from(), cursor.to());
            cm.scrollIntoView({from: cursor.from(), to: cursor.to()});
            confirmDialog(cm, doReplaceConfirm, "Replace?",
                          [function() {doReplace(match);}, advance]);
          };
          var doReplace = function(match) {
            cursor.replace(typeof query == "string" ? text :
                           text.replace(/\$(\d)/g, function(_, i) {return match[i];}));
            advance();
          };
          advance();
        }
      });
    });
  }

  CodeMirror.commands.find = function(cm) {clearSearch(cm); doSearch(cm);};
  CodeMirror.commands.findPersistent = function(cm) {clearSearch(cm); doSearch(cm, false, true);};
  CodeMirror.commands.findNext = doSearch;
  CodeMirror.commands.findPrev = function(cm) {doSearch(cm, true);};
  CodeMirror.commands.clearSearch = clearSearch;
  CodeMirror.commands.replace = replace;
  CodeMirror.commands.replaceAll = function(cm) {replace(cm, true);};
});
