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
  function parseWords(str) {
	logger.error(`>>> Entering parseWords(str=${str})`, { "event.action": "parseWords", "logrite.str": str });
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.error(`parseWords(str=${str}): obj → ${obj}`, { "event.action": "parseWords", "logrite.str": str });
	logger.warn(`<<< Exiting parseWords(str=${str})`, { "event.action": "parseWords", "logrite.str": str });
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
	logger.error(`>>> Entering chain(stream=${stream},state=${state},f=${f})`, { "event.action": "chain", "logrite.stream": stream, "logrite.state": state, "logrite.f": f });
      state.tokenize = f;
	logger.warn(`<<< Exiting chain(stream=${stream},state=${state},f=${f})`, { "event.action": "chain", "logrite.stream": stream, "logrite.state": state, "logrite.f": f });
      return f(stream, state);
    }
    function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      var beforeParams = state.beforeParams;
      state.beforeParams = false;
	logger.debug(`tokenBase(stream=${stream},state=${state}): state.beforeParams → ${state.beforeParams}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      var ch = stream.next();
	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
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
        if (keywords && keywords.propertyIsEnumerable(word))
          return "keyword";
        if (functions && functions.propertyIsEnumerable(word)) {
          state.beforeParams = true;
          return "keyword";
        }
		logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
        return null;
      }
    }
    function tokenString(quote) {
	logger.error(`>>> Entering tokenString(quote=${quote})`, { "event.action": "tokenString", "logrite.quote": quote });
      return function(stream, state) {
      var escaped = false, next, end = false;
      while ((next = stream.next()) != null) {
        if (next == quote && !escaped) {
          end = true;
          break;
        }
        escaped = !escaped && next == "\\";
      }
      if (end) state.tokenize = tokenBase;
        return "string";
      };
    }
    function tokenComment(stream, state) {
	logger.error(`>>> Entering tokenComment(stream=${stream},state=${state})`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
      var maybeEnd = false, ch;
      while (ch = stream.next()) {
        if (ch == "#" && maybeEnd) {
          state.tokenize = tokenBase;
		logger.debug(`tokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
          break;
        }
        maybeEnd = (ch == "*");
      }
	logger.error(`tokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
      return "comment";
    }
    function tokenUnparsed(stream, state) {
	logger.error(`>>> Entering tokenUnparsed(stream=${stream},state=${state})`, { "event.action": "tokenUnparsed", "logrite.stream": stream, "logrite.state": state });
      var maybeEnd = 0, ch;
      while (ch = stream.next()) {
        if (ch == "#" && maybeEnd == 2) {
          state.tokenize = tokenBase;
		logger.debug(`tokenUnparsed(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenUnparsed", "logrite.stream": stream, "logrite.state": state });
          break;
        }
        if (ch == "]")
          maybeEnd++;
        else if (ch != " ")
          maybeEnd = 0;
      }
	logger.warn(`<<< Exiting tokenUnparsed(stream=${stream},state=${state})`, { "event.action": "tokenUnparsed", "logrite.stream": stream, "logrite.state": state });
      return "meta";
    }
    return {
      startState: function() {
		logger.error('>>> Entering startState()', { "event.action": "startState" });
        return {
          tokenize: tokenBase,
          beforeParams: false,
          inParams: false
        };
		logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      },
      token: function(stream, state) {
		logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        if (stream.eatSpace()) return null;
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        return state.tokenize(stream, state);
      }
    };
});
CodeMirror.defineMIME("text/x-tcl", "tcl");

});
