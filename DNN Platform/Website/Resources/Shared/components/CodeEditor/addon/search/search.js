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
	logger.error(`>>> Entering searchOverlay(query=${query},caseInsensitive=${caseInsensitive})`, { "event.action": "searchOverlay", "logrite.query": query, "logrite.caseInsensitive": caseInsensitive });
    if (typeof query == "string")
      query = new RegExp(query.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&"), caseInsensitive ? "gi" : "g");
    else if (!query.global)
      query = new RegExp(query.source, query.ignoreCase ? "gi" : "g");

	logger.error(`searchOverlay(query=${query},caseInsensitive=${caseInsensitive}): query → ${query}`, { "event.action": "searchOverlay", "logrite.query": query, "logrite.caseInsensitive": caseInsensitive });
    return {token: function(stream) {
	logger.error(`>>> Entering token(stream=${stream})`, { "event.action": "token", "logrite.stream": stream });
      query.lastIndex = stream.pos;
      var match = query.exec(stream.string);
	logger.warn(`<<< Exiting token(stream=${stream})`, { "event.action": "token", "logrite.stream": stream });
	logger.error(`token(stream=${stream}): match → ${match}`, { "event.action": "token", "logrite.stream": stream });
      if (match && match.index == stream.pos) {
        stream.pos += match[0].length;
        return "searching";
      } else if (match) {
        stream.pos = match.index;
      } else {
		logger.debug(`searchOverlay(query=${query},caseInsensitive=${caseInsensitive}): query.lastIndex → ${query.lastIndex}`, { "event.action": "searchOverlay", "logrite.query": query, "logrite.caseInsensitive": caseInsensitive });
        stream.skipToEnd();
		logger.debug(`token(stream=${stream}): stream.pos → ${stream.pos}`, { "event.action": "token", "logrite.stream": stream });
      }
    }};
  }

  function SearchState() {
	logger.error('>>> Entering SearchState()', { "event.action": "SearchState" });
    this.posFrom = this.posTo = this.lastQuery = this.query = null;
    this.overlay = null;
	logger.debug(`SearchState(): this.posFrom → ${this.posFrom}`, { "event.action": "SearchState" });
  }

  function getSearchState(cm) {
	logger.error(`>>> Entering getSearchState(cm=${cm})`, { "event.action": "getSearchState", "logrite.cm": cm });
	logger.warn(`<<< Exiting getSearchState(cm=${cm})`, { "event.action": "getSearchState", "logrite.cm": cm });
    return cm.state.search || (cm.state.search = new SearchState());
  }

  function queryCaseInsensitive(query) {
	logger.error(`>>> Entering queryCaseInsensitive(query=${query})`, { "event.action": "queryCaseInsensitive", "logrite.query": query });
	logger.warn(`<<< Exiting queryCaseInsensitive(query=${query})`, { "event.action": "queryCaseInsensitive", "logrite.query": query });
    return typeof query == "string" && query == query.toLowerCase();
  }

  function getSearchCursor(cm, query, pos) {
	logger.error(`>>> Entering getSearchCursor(cm=${cm},query=${query},pos=${pos})`, { "event.action": "getSearchCursor", "logrite.cm": cm, "logrite.query": query, "logrite.pos": pos });
    // Heuristic: if the query string is all lowercase, do a case insensitive search.
	logger.warn(`<<< Exiting getSearchCursor(cm=${cm},query=${query},pos=${pos})`, { "event.action": "getSearchCursor", "logrite.cm": cm, "logrite.query": query, "logrite.pos": pos });
    return cm.getSearchCursor(query, pos, queryCaseInsensitive(query));
  }

  function persistentDialog(cm, text, deflt, f) {
	logger.error(`>>> Entering persistentDialog(cm=${cm},text=${text},deflt=${deflt},f=${f})`, { "event.action": "persistentDialog", "logrite.cm": cm, "logrite.text": text, "logrite.deflt": deflt, "logrite.f": f });
    cm.openDialog(text, f, {
      value: deflt,
      selectValueOnOpen: true,
      closeOnEnter: false,
      onClose: function() {
          logger.error('>>> Entering onClose()', { "event.action": "onClose" });
          logger.warn('<<< Exiting onClose()', { "event.action": "onClose" });
          clearSearch(cm);
      }
    });
	logger.warn(`<<< Exiting persistentDialog(cm=${cm},text=${text},deflt=${deflt},f=${f})`, { "event.action": "persistentDialog", "logrite.cm": cm, "logrite.text": text, "logrite.deflt": deflt, "logrite.f": f });
  }

  function dialog(cm, text, shortText, deflt, f) {
	logger.error(`>>> Entering dialog(cm=${cm},text=${text},shortText=${shortText},deflt=${deflt},f=${f})`, { "event.action": "dialog", "logrite.cm": cm, "logrite.text": text, "logrite.shortText": shortText, "logrite.deflt": deflt, "logrite.f": f });
    if (cm.openDialog) cm.openDialog(text, f, {value: deflt, selectValueOnOpen: true});
    else f(prompt(shortText, deflt));
	logger.warn(`<<< Exiting dialog(cm=${cm},text=${text},shortText=${shortText},deflt=${deflt},f=${f})`, { "event.action": "dialog", "logrite.cm": cm, "logrite.text": text, "logrite.shortText": shortText, "logrite.deflt": deflt, "logrite.f": f });
  }

  function confirmDialog(cm, text, shortText, fs) {
	logger.error(`>>> Entering confirmDialog(cm=${cm},text=${text},shortText=${shortText},fs=${fs})`, { "event.action": "confirmDialog", "logrite.cm": cm, "logrite.text": text, "logrite.shortText": shortText, "logrite.fs": fs });
    if (cm.openConfirm) cm.openConfirm(text, fs);
    else if (confirm(shortText)) fs[0]();
	logger.warn(`<<< Exiting confirmDialog(cm=${cm},text=${text},shortText=${shortText},fs=${fs})`, { "event.action": "confirmDialog", "logrite.cm": cm, "logrite.text": text, "logrite.shortText": shortText, "logrite.fs": fs });
  }

  function parseString(string) {
	logger.error(`>>> Entering parseString(string=${string})`, { "event.action": "parseString", "logrite.string": string });
    return string.replace(/\\(.)/g, function(_, ch) {
      if (ch == "n") return "\n"
      if (ch == "r") return "\r"
	logger.warn(`<<< Exiting parseString(string=${string})`, { "event.action": "parseString", "logrite.string": string });
      return ch
    })
  }

  function parseQuery(query) {
	logger.error(`>>> Entering parseQuery(query=${query})`, { "event.action": "parseQuery", "logrite.query": query });
    var isRE = query.match(/^\/(.*)\/([a-z]*)$/);
    if (isRE) {
	logger.error(`parseQuery(query=${query}): isRE → ${isRE}`, { "event.action": "parseQuery", "logrite.query": query });
      try { query = new RegExp(isRE[1], isRE[2].indexOf("i") == -1 ? "" : "i"); }
      catch(e) {} // Not a regular expression after all, do a string search
    } else {
      query = parseString(query)
    }
    if (typeof query == "string" ? query == "" : query.test(""))
      query = /x^/;
	logger.warn(`<<< Exiting parseQuery(query=${query})`, { "event.action": "parseQuery", "logrite.query": query });
    return query;
  }

  var queryDialog =
    'Search: <input type="text" style="width: 10em" class="CodeMirror-search-field"/> <span style="color: #888" class="CodeMirror-search-hint">(Use /re/ syntax for regexp search)</span>';

  function startSearch(cm, state, query) {
	logger.error(`>>> Entering startSearch(cm=${cm},state=${state},query=${query})`, { "event.action": "startSearch", "logrite.cm": cm, "logrite.state": state, "logrite.query": query });
    state.queryText = query;
    state.query = parseQuery(query);
	logger.debug(`startSearch(cm=${cm},state=${state},query=${query}): state.queryText → ${state.queryText}`, { "event.action": "startSearch", "logrite.cm": cm, "logrite.state": state, "logrite.query": query });
    cm.removeOverlay(state.overlay, queryCaseInsensitive(state.query));
    state.overlay = searchOverlay(state.query, queryCaseInsensitive(state.query));
    cm.addOverlay(state.overlay);
    if (cm.showMatchesOnScrollbar) {
      if (state.annotate) { state.annotate.clear(); state.annotate = null; }
      state.annotate = cm.showMatchesOnScrollbar(state.query, queryCaseInsensitive(state.query));
	logger.warn(`<<< Exiting startSearch(cm=${cm},state=${state},query=${query})`, { "event.action": "startSearch", "logrite.cm": cm, "logrite.state": state, "logrite.query": query });
    }
  }

  function doSearch(cm, rev, persistent) {
	logger.error(`>>> Entering doSearch(cm=${cm},rev=${rev},persistent=${persistent})`, { "event.action": "doSearch", "logrite.cm": cm, "logrite.rev": rev, "logrite.persistent": persistent });
    var state = getSearchState(cm);
    if (state.query) return findNext(cm, rev);
    var q = cm.getSelection() || state.lastQuery;
	logger.warn(`<<< Exiting doSearch(cm=${cm},rev=${rev},persistent=${persistent})`, { "event.action": "doSearch", "logrite.cm": cm, "logrite.rev": rev, "logrite.persistent": persistent });
	logger.error(`doSearch(cm=${cm},rev=${rev},persistent=${persistent}): q → ${q}`, { "event.action": "doSearch", "logrite.cm": cm, "logrite.rev": rev, "logrite.persistent": persistent });
    if (persistent && cm.openDialog) {
      persistentDialog(cm, queryDialog, q, function(query, event) {
        CodeMirror.e_stop(event);
        if (!query) return;
        if (query != state.queryText) startSearch(cm, state, query);
        findNext(cm, event.shiftKey);
      });
    } else {
      dialog(cm, queryDialog, "Search for:", q, function(query) {
        if (query && !state.query) cm.operation(function() {
          startSearch(cm, state, query);
          state.posFrom = state.posTo = cm.getCursor();
          findNext(cm, rev);
        });
      });
    }
  }

  function findNext(cm, rev) {cm.operation(function() {
	logger.error(`>>> Entering findNext(cm=${cm},rev=${rev})`, { "event.action": "findNext", "logrite.cm": cm, "logrite.rev": rev });
    var state = getSearchState(cm);
    var cursor = getSearchCursor(cm, state.query, rev ? state.posFrom : state.posTo);
    if (!cursor.find(rev)) {
      cursor = getSearchCursor(cm, state.query, rev ? CodeMirror.Pos(cm.lastLine()) : CodeMirror.Pos(cm.firstLine(), 0));
      if (!cursor.find(rev)) return;
    }
    cm.setSelection(cursor.from(), cursor.to());
    cm.scrollIntoView({from: cursor.from(), to: cursor.to()}, 20);
	logger.warn(`<<< Exiting findNext(cm=${cm},rev=${rev})`, { "event.action": "findNext", "logrite.cm": cm, "logrite.rev": rev });
    state.posFrom = cursor.from(); state.posTo = cursor.to();
  });}

  function clearSearch(cm) {cm.operation(function() {
	logger.error(`>>> Entering clearSearch(cm=${cm})`, { "event.action": "clearSearch", "logrite.cm": cm });
    var state = getSearchState(cm);
    state.lastQuery = state.query;
	logger.debug(`clearSearch(cm=${cm}): state.lastQuery → ${state.lastQuery}`, { "event.action": "clearSearch", "logrite.cm": cm });
	logger.debug(`clearSearch(cm=${cm}): state.query → ${state.query}`, { "event.action": "clearSearch", "logrite.cm": cm });
    if (!state.query) return;
    state.query = state.queryText = null;
    cm.removeOverlay(state.overlay);
	logger.warn(`<<< Exiting clearSearch(cm=${cm})`, { "event.action": "clearSearch", "logrite.cm": cm });
    if (state.annotate) { state.annotate.clear(); state.annotate = null; }
  });}

  var replaceQueryDialog =
    'Replace: <input type="text" style="width: 10em" class="CodeMirror-search-field"/> <span style="color: #888" class="CodeMirror-search-hint">(Use /re/ syntax for regexp search)</span>';
  var replacementQueryDialog = 'With: <input type="text" style="width: 10em" class="CodeMirror-search-field"/>';
  var doReplaceConfirm = "Replace? <button>Yes</button> <button>No</button> <button>Stop</button>";

  function replace(cm, all) {
	logger.error(`>>> Entering replace(cm=${cm},all=${all})`, { "event.action": "replace", "logrite.cm": cm, "logrite.all": all });
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
			logger.error('>>> Entering advance()', { "event.action": "advance" });
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
						logger.warn('<<< Exiting advance()', { "event.action": "advance" });
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
	logger.warn(`<<< Exiting replace(cm=${cm},all=${all})`, { "event.action": "replace", "logrite.cm": cm, "logrite.all": all });
  }

  CodeMirror.commands.find = function(cm) {clearSearch(cm); doSearch(cm);};
logger.error(`>>> Entering find(cm=${cm})`, { "event.action": "find", "logrite.cm": cm });
  CodeMirror.commands.findPersistent = function(cm) {clearSearch(cm); doSearch(cm, false, true);};
logger.warn(`<<< Exiting find(cm=${cm})`, { "event.action": "find", "logrite.cm": cm });
  CodeMirror.commands.findNext = doSearch;
logger.error(`>>> Entering findPersistent(cm=${cm})`, { "event.action": "findPersistent", "logrite.cm": cm });
  CodeMirror.commands.findPrev = function(cm) {doSearch(cm, true);};
logger.warn(`<<< Exiting findPersistent(cm=${cm})`, { "event.action": "findPersistent", "logrite.cm": cm });
  CodeMirror.commands.clearSearch = clearSearch;
logger.error(`>>> Entering findPrev(cm=${cm})`, { "event.action": "findPrev", "logrite.cm": cm });
  CodeMirror.commands.replace = replace;
logger.warn(`<<< Exiting findPrev(cm=${cm})`, { "event.action": "findPrev", "logrite.cm": cm });
  CodeMirror.commands.replaceAll = function(cm) {replace(cm, true);};
logger.error(`>>> Entering replaceAll(cm=${cm})`, { "event.action": "replaceAll", "logrite.cm": cm });
});
