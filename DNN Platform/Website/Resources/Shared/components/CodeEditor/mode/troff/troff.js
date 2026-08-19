const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object")
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd)
    define(["../../lib/codemirror"], mod);
  else
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

logger.debug(`<<< Exiting function(exports=${exports})`);
CodeMirror.defineMode('troff', function() {
logger.debug('>>> Entering defineMode()');

  var words = {};
logger.debug('<<< Exiting defineMode()');

  function tokenBase(stream) {
	logger.debug(`>>> Entering tokenBase(stream=${stream})`);
    if (stream.eatSpace()) return null;

    var sol = stream.sol();
	logger.debug(`tokenBase(stream=${stream}): sol → ${sol}`);
    var ch = stream.next();
	logger.debug(`tokenBase(stream=${stream}): ch → ${ch}`);

    if (ch === '\\') {
      if (stream.match('fB') || stream.match('fR') || stream.match('fI') ||
          stream.match('u')  || stream.match('d')  ||
          stream.match('%')  || stream.match('&')) {
        return 'string';
      }
      if (stream.match('m[')) {
        stream.skipTo(']');
        stream.next();
        return 'string';
      }
      if (stream.match('s+') || stream.match('s-')) {
        stream.eatWhile(/[\d-]/);
        return 'string';
      }
      if (stream.match('\(') || stream.match('*\(')) {
        stream.eatWhile(/[\w-]/);
        return 'string';
      }
      return 'string';
    }
    if (sol && (ch === '.' || ch === '\'')) {
      if (stream.eat('\\') && stream.eat('\"')) {
        stream.skipToEnd();
        return 'comment';
      }
    }
    if (sol && ch === '.') {
      if (stream.match('B ') || stream.match('I ') || stream.match('R ')) {
        return 'attribute';
      }
      if (stream.match('TH ') || stream.match('SH ') || stream.match('SS ') || stream.match('HP ')) {
        stream.skipToEnd();
        return 'quote';
      }
      if ((stream.match(/[A-Z]/) && stream.match(/[A-Z]/)) || (stream.match(/[a-z]/) && stream.match(/[a-z]/))) {
        return 'attribute';
      }
    }
    stream.eatWhile(/[\w-]/);
    var cur = stream.current();
	logger.debug(`<<< Exiting tokenBase(stream=${stream})`);
    return words.hasOwnProperty(cur) ? words[cur] : null;
  }

  function tokenize(stream, state) {
	logger.debug(`>>> Entering tokenize(stream=${stream},state=${state})`);
	logger.debug(`<<< Exiting tokenize(stream=${stream},state=${state})`);
    return (state.tokens[0] || tokenBase) (stream, state);
  };

  return {
    startState: function() {return {tokens:[]};},
    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      return tokenize(stream, state);
    }
  };
});

CodeMirror.defineMIME('troff', 'troff');

});
