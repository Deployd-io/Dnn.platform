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

logger.debug(`<<< Exiting function(exports=${exports})`);
CodeMirror.defineMode("octave", function() {
  function wordRegexp(words) {
	logger.debug(`>>> Entering wordRegexp(words=${words})`);
	logger.debug(`<<< Exiting wordRegexp(words=${words})`);
    return new RegExp("^((" + words.join(")|(") + "))\\b");
  }

  var singleOperators = new RegExp("^[\\+\\-\\*/&|\\^~<>!@'\\\\]");
  var singleDelimiters = new RegExp('^[\\(\\[\\{\\},:=;]');
  var doubleOperators = new RegExp("^((==)|(~=)|(<=)|(>=)|(<<)|(>>)|(\\.[\\+\\-\\*/\\^\\\\]))");
  var doubleDelimiters = new RegExp("^((!=)|(\\+=)|(\\-=)|(\\*=)|(/=)|(&=)|(\\|=)|(\\^=))");
  var tripleDelimiters = new RegExp("^((>>=)|(<<=))");
  var expressionEnd = new RegExp("^[\\]\\)]");
  var identifiers = new RegExp("^[_A-Za-z\xa1-\uffff][_A-Za-z0-9\xa1-\uffff]*");

  var builtins = wordRegexp([
    'error', 'eval', 'function', 'abs', 'acos', 'atan', 'asin', 'cos',
    'cosh', 'exp', 'log', 'prod', 'sum', 'log10', 'max', 'min', 'sign', 'sin', 'sinh',
    'sqrt', 'tan', 'reshape', 'break', 'zeros', 'default', 'margin', 'round', 'ones',
    'rand', 'syn', 'ceil', 'floor', 'size', 'clear', 'zeros', 'eye', 'mean', 'std', 'cov',
    'det', 'eig', 'inv', 'norm', 'rank', 'trace', 'expm', 'logm', 'sqrtm', 'linspace', 'plot',
    'title', 'xlabel', 'ylabel', 'legend', 'text', 'grid', 'meshgrid', 'mesh', 'num2str',
    'fft', 'ifft', 'arrayfun', 'cellfun', 'input', 'fliplr', 'flipud', 'ismember'
  ]);

  var keywords = wordRegexp([
    'return', 'case', 'switch', 'else', 'elseif', 'end', 'endif', 'endfunction',
    'if', 'otherwise', 'do', 'for', 'while', 'try', 'catch', 'classdef', 'properties', 'events',
    'methods', 'global', 'persistent', 'endfor', 'endwhile', 'printf', 'sprintf', 'disp', 'until',
    'continue', 'pkg'
  ]);


  // tokenizers
  function tokenTranspose(stream, state) {
	logger.debug(`>>> Entering tokenTranspose(stream=${stream},state=${state})`);
    if (!stream.sol() && stream.peek() === '\'') {
      stream.next();
      state.tokenize = tokenBase;
	logger.debug(`tokenTranspose(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
      return 'operator';
    }
    state.tokenize = tokenBase;
	logger.debug(`<<< Exiting tokenTranspose(stream=${stream},state=${state})`);
    return tokenBase(stream, state);
  }


  function tokenComment(stream, state) {
	logger.debug(`>>> Entering tokenComment(stream=${stream},state=${state})`);
    if (stream.match(/^.*%}/)) {
      state.tokenize = tokenBase;
	logger.debug(`tokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting tokenComment(stream=${stream},state=${state})`);
      return 'comment';
    };
    stream.skipToEnd();
    return 'comment';
  }

  function tokenBase(stream, state) {
	logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
    // whitespaces
    if (stream.eatSpace()) return null;

    // Handle one line Comments
    if (stream.match('%{')){
      state.tokenize = tokenComment;
	logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
      stream.skipToEnd();
      return 'comment';
    }

    if (stream.match(/^[%#]/)){
      stream.skipToEnd();
      return 'comment';
    }

    // Handle Number Literals
    if (stream.match(/^[0-9\.+-]/, false)) {
      if (stream.match(/^[+-]?0x[0-9a-fA-F]+[ij]?/)) {
        stream.tokenize = tokenBase;
		logger.debug(`tokenBase(stream=${stream},state=${state}): stream.tokenize → ${stream.tokenize}`);
        return 'number'; };
      if (stream.match(/^[+-]?\d*\.\d+([EeDd][+-]?\d+)?[ij]?/)) { return 'number'; };
      if (stream.match(/^[+-]?\d+([EeDd][+-]?\d+)?[ij]?/)) { return 'number'; };
    }
    if (stream.match(wordRegexp(['nan','NaN','inf','Inf']))) { return 'number'; };

    // Handle Strings
    if (stream.match(/^"([^"]|(""))*"/)) { return 'string'; } ;
    if (stream.match(/^'([^']|(''))*'/)) { return 'string'; } ;

    // Handle words
    if (stream.match(keywords)) { return 'keyword'; } ;
    if (stream.match(builtins)) { return 'builtin'; } ;
    if (stream.match(identifiers)) { return 'variable'; } ;

    if (stream.match(singleOperators) || stream.match(doubleOperators)) { return 'operator'; };
    if (stream.match(singleDelimiters) || stream.match(doubleDelimiters) || stream.match(tripleDelimiters)) { return null; };

    if (stream.match(expressionEnd)) {
      state.tokenize = tokenTranspose;
      return null;
    };


    // Handle non-detected items
    stream.next();
    return 'error';
  };


  return {
    startState: function() {
	logger.debug('>>> Entering startState()');
      return {
        tokenize: tokenBase
      };
	logger.debug('<<< Exiting startState()');
    },

    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      var style = state.tokenize(stream, state);
      if (style === 'number' || style === 'variable'){
		logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
        state.tokenize = tokenTranspose;
		logger.debug(`token(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
      }
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      return style;
    }
  };
});

CodeMirror.defineMIME("text/x-octave", "octave");

});
