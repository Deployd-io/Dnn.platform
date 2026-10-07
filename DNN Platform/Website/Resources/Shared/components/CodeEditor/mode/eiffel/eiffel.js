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

CodeMirror.defineMode("eiffel", function() {
  function wordObj(words) {
	logger.error(`>>> Entering wordObj(words=${words})`, { "event.action": "wordObj", "logrite.words": words });
    var o = {};
    for (var i = 0, e = words.length; i < e; ++i) o[words[i]] = true;
	logger.warn(`<<< Exiting wordObj(words=${words})`, { "event.action": "wordObj", "logrite.words": words });
    return o;
  }
  var keywords = wordObj([
    'note',
    'across',
    'when',
    'variant',
    'until',
    'unique',
    'undefine',
    'then',
    'strip',
    'select',
    'retry',
    'rescue',
    'require',
    'rename',
    'reference',
    'redefine',
    'prefix',
    'once',
    'old',
    'obsolete',
    'loop',
    'local',
    'like',
    'is',
    'inspect',
    'infix',
    'include',
    'if',
    'frozen',
    'from',
    'external',
    'export',
    'ensure',
    'end',
    'elseif',
    'else',
    'do',
    'creation',
    'create',
    'check',
    'alias',
    'agent',
    'separate',
    'invariant',
    'inherit',
    'indexing',
    'feature',
    'expanded',
    'deferred',
    'class',
    'Void',
    'True',
    'Result',
    'Precursor',
    'False',
    'Current',
    'create',
    'attached',
    'detachable',
    'as',
    'and',
    'implies',
    'not',
    'or'
  ]);
  var operators = wordObj([":=", "and then","and", "or","<<",">>"]);

  function chain(newtok, stream, state) {
	logger.error(`>>> Entering chain(newtok=${newtok},stream=${stream},state=${state})`, { "event.action": "chain", "logrite.newtok": newtok, "logrite.stream": stream, "logrite.state": state });
    state.tokenize.push(newtok);
	logger.warn(`<<< Exiting chain(newtok=${newtok},stream=${stream},state=${state})`, { "event.action": "chain", "logrite.newtok": newtok, "logrite.stream": stream, "logrite.state": state });
    return newtok(stream, state);
  }

  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    if (stream.eatSpace()) return null;
    var ch = stream.next();
	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    if (ch == '"'||ch == "'") {
      return chain(readQuoted(ch, "string"), stream, state);
    } else if (ch == "-"&&stream.eat("-")) {
      stream.skipToEnd();
      return "comment";
    } else if (ch == ":"&&stream.eat("=")) {
      return "operator";
    } else if (/[0-9]/.test(ch)) {
      stream.eatWhile(/[xXbBCc0-9\.]/);
      stream.eat(/[\?\!]/);
      return "ident";
    } else if (/[a-zA-Z_0-9]/.test(ch)) {
      stream.eatWhile(/[a-zA-Z_0-9]/);
      stream.eat(/[\?\!]/);
      return "ident";
    } else if (/[=+\-\/*^%<>~]/.test(ch)) {
      stream.eatWhile(/[=+\-\/*^%<>~]/);
      return "operator";
    } else {
	logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      return null;
    }
  }

  function readQuoted(quote, style,  unescaped) {
	logger.error(`>>> Entering readQuoted(quote=${quote},style=${style},unescaped=${unescaped})`, { "event.action": "readQuoted", "logrite.quote": quote, "logrite.style": style, "logrite.unescaped": unescaped });
    return function(stream, state) {
      var escaped = false, ch;
      while ((ch = stream.next()) != null) {
        if (ch == quote && (unescaped || !escaped)) {
          state.tokenize.pop();
          break;
        }
        escaped = !escaped && ch == "%";
      }
      return style;
    };
  }

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      return {tokenize: [tokenBase]};
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      var style = state.tokenize[state.tokenize.length-1](stream, state);
      if (style == "ident") {
		logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        var word = stream.current();
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        style = keywords.propertyIsEnumerable(stream.current()) ? "keyword"
          : operators.propertyIsEnumerable(stream.current()) ? "operator"
          : /^[A-Z][A-Z_0-9]*$/g.test(word) ? "tag"
          : /^0[bB][0-1]+$/g.test(word) ? "number"
          : /^0[cC][0-7]+$/g.test(word) ? "number"
          : /^0[xX][a-fA-F0-9]+$/g.test(word) ? "number"
          : /^([0-9]+\.[0-9]*)|([0-9]*\.[0-9]+)$/g.test(word) ? "number"
          : /^[0-9]+$/g.test(word) ? "number"
          : "variable";
      }
	logger.error(`token(stream=${stream},state=${state}): word → ${word}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return style;
    },
    lineComment: "--"
  };
});

CodeMirror.defineMIME("text/x-eiffel", "eiffel");

});
