const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

//tcl mode by Ford_Lawnmower :: Based on Velocity mode by Steve O'Hara

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMode("tcl", function() {
logger.debug('>>> Entering defineMode()');
  function parseWords(str) {
	logger.debug('>>> Entering parseWords()');
    var obj = {}, words = str.split(" ");
	logger.debug('<<< Exiting defineMode()');
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.debug(`parseWords(): obj → ${obj}`);
	logger.debug('<<< Exiting parseWords()');
    return obj;
  }
  var keywords = parseWords("Tcl safe after append array auto_execok auto_import auto_load " +
        "auto_mkindex auto_mkindex_old auto_qualify auto_reset bgerror " +
        "binary break catch cd close concat continue dde eof encoding error " +
        "eval exec exit expr fblocked fconfigure fcopy file fileevent filename " +
        "filename flush for foreach format gets glob global history http if " +
        "incr info interp join lappend lindex linsert list llength load lrange " +
        "lreplace lsearch lset lsort memory msgcat namespace open package parray " +
        "pid pkg::create pkg_mkIndex proc puts pwd re_syntax read regex regexp " +
        "registry regsub rename resource return scan seek set socket source split " +
        "string subst switch tcl_endOfWord tcl_findLibrary tcl_startOfNextWord " +
        "tcl_wordBreakAfter tcl_startOfPreviousWord tcl_wordBreakBefore tcltest " +
        "tclvars tell time trace unknown unset update uplevel upvar variable " +
    "vwait");
    var functions = parseWords("if elseif else and not or eq ne in ni for foreach while switch");
    var isOperatorChar = /[+\-*&%=<>!?^\/\|]/;
    function chain(stream, state, f) {
	logger.debug('>>> Entering chain()');
      state.tokenize = f;
	logger.debug(`chain(): state.tokenize → ${state.tokenize}`);
	logger.debug('<<< Exiting chain()');
      return f(stream, state);
    }
    function tokenBase(stream, state) {
	logger.debug('>>> Entering tokenBase()');
      var beforeParams = state.beforeParams;
      state.beforeParams = false;
	logger.debug(`tokenBase(): state.beforeParams → ${state.beforeParams}`);
      var ch = stream.next();
	logger.debug(`tokenBase(): ch → ${ch}`);
      if ((ch == '"' || ch == "'") && state.inParams)
        return chain(stream, state, tokenString(ch));
      else if (/[\[\]{}\(\),;\.]/.test(ch)) {
        if (ch == "(" && beforeParams) state.inParams = true;
        else if (ch == ")") state.inParams = false;
          return null;
      }
      else if (/\d/.test(ch)) {
        stream.eatWhile(/[\w\.]/);
        return "number";
      }
      else if (ch == "#" && stream.eat("*")) {
        return chain(stream, state, tokenComment);
      }
      else if (ch == "#" && stream.match(/ *\[ *\[/)) {
        return chain(stream, state, tokenUnparsed);
      }
      else if (ch == "#" && stream.eat("#")) {
        stream.skipToEnd();
        return "comment";
      }
      else if (ch == '"') {
        stream.skipTo(/"/);
        return "comment";
      }
      else if (ch == "$") {
        stream.eatWhile(/[$_a-z0-9A-Z\.{:]/);
        stream.eatWhile(/}/);
        state.beforeParams = true;
        return "builtin";
      }
      else if (isOperatorChar.test(ch)) {
        stream.eatWhile(isOperatorChar);
        return "comment";
      }
      else {
        stream.eatWhile(/[\w\$_{}\xa1-\uffff]/);
        var word = stream.current().toLowerCase();
		logger.debug(`tokenBase(): word → ${word}`);
        if (keywords && keywords.propertyIsEnumerable(word))
          return "keyword";
        if (functions && functions.propertyIsEnumerable(word)) {
          state.beforeParams = true;
		logger.debug('<<< Exiting tokenBase()');
          return "keyword";
        }
        return null;
      }
    }
    function tokenString(quote) {
	logger.debug('>>> Entering tokenString()');
      return function(stream, state) {
	logger.debug('>>> Entering function()');
      var escaped = false, next, end = false;
      while ((next = stream.next()) != null) {
        if (next == quote && !escaped) {
          end = true;
          break;
        }
        escaped = !escaped && next == "\\";
		logger.debug('<<< Exiting function()');
      }
      if (end) state.tokenize = tokenBase;
		logger.debug('<<< Exiting tokenString()');
        return "string";
      };
    }
    function tokenComment(stream, state) {
	logger.debug('>>> Entering tokenComment()');
      var maybeEnd = false, ch;
      while (ch = stream.next()) {
        if (ch == "#" && maybeEnd) {
          state.tokenize = tokenBase;
		logger.debug(`tokenComment(): state.tokenize → ${state.tokenize}`);
          break;
        }
        maybeEnd = (ch == "*");
      }
	logger.debug(`tokenComment(): maybeEnd → ${maybeEnd}`);
	logger.debug('<<< Exiting tokenComment()');
      return "comment";
    }
    function tokenUnparsed(stream, state) {
	logger.debug('>>> Entering tokenUnparsed()');
      var maybeEnd = 0, ch;
      while (ch = stream.next()) {
        if (ch == "#" && maybeEnd == 2) {
          state.tokenize = tokenBase;
		logger.debug(`tokenUnparsed(): state.tokenize → ${state.tokenize}`);
          break;
        }
        if (ch == "]")
          maybeEnd++;
        else if (ch != " ")
          maybeEnd = 0;
      }
	logger.debug('<<< Exiting tokenUnparsed()');
      return "meta";
    }
    return {
      startState: function() {
		logger.debug('>>> Entering startState()');
        return {
          tokenize: tokenBase,
          beforeParams: false,
          inParams: false
        };
		logger.debug('<<< Exiting startState()');
      },
      token: function(stream, state) {
		logger.debug('>>> Entering token()');
        if (stream.eatSpace()) return null;
		logger.debug('<<< Exiting token()');
        return state.tokenize(stream, state);
      }
    };
});
CodeMirror.defineMIME("text/x-tcl", "tcl");

});
