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

CodeMirror.defineMode('smalltalk', function(config) {
logger.debug('>>> Entering defineMode()');

  var specialChars = /[+\-\/\\*~<>=@%|&?!.,:;^]/;
  var keywords = /true|false|nil|self|super|thisContext/;

  var Context = function(tokenizer, parent) {
    this.next = tokenizer;
    this.parent = parent;
  };

logger.debug(`function(exports=${exports}): this.next → ${this.next}`);
  var Token = function(name, context, eos) {
	logger.debug(`>>> Entering Token(name=${name},context=${context},eos=${eos})`);
    this.name = name;
    this.context = context;
    this.eos = eos;
	logger.debug(`Token(name=${name},context=${context},eos=${eos}): name → ${name}`);
	logger.debug(`Token(name=${name},context=${context},eos=${eos}): context → ${context}`);
	logger.debug(`<<< Exiting Token(name=${name},context=${context},eos=${eos})`);
  };

  var State = function() {
	logger.debug('>>> Entering State()');
    this.context = new Context(next, null);
	logger.debug(`function(exports=${exports}): this.name → ${this.name}`);
    this.expectVariable = true;
    this.indentation = 0;
	logger.debug(`function(exports=${exports}): this.eos → ${this.eos}`);
    this.userIndentationDelta = 0;
	logger.debug(`State(): indentation → ${indentation}`);
	logger.debug('<<< Exiting State()');
  };

logger.debug(`function(exports=${exports}): this.expectVariable → ${this.expectVariable}`);
  State.prototype.userIndent = function(indentation) {
    this.userIndentationDelta = indentation > 0 ? (indentation / config.indentUnit - this.indentation) : 0;
  };

logger.debug(`function(exports=${exports}): this.indentation → ${this.indentation}`);
  var next = function(stream, context, state) {
	logger.debug(`>>> Entering next(stream=${stream},context=${context},state=${state})`);
    var token = new Token(null, context, false);
    var aChar = stream.next();
	logger.debug(`next(stream=${stream},context=${context},state=${state}): aChar → ${aChar}`);

    if (aChar === '"') {
      token = nextComment(stream, new Context(nextComment, context));
	logger.debug(`function(exports=${exports}): this.userIndentationDelta → ${this.userIndentationDelta}`);

    } else if (aChar === '\'') {
      token = nextString(stream, new Context(nextString, context));

    } else if (aChar === '#') {
	logger.debug(`function(exports=${exports}): this.parent → ${this.parent}`);
      if (stream.peek() === '\'') {
		logger.debug(`defineMode(): this.parent → ${this.parent}`);
        stream.next();
		logger.debug(`function(exports=${exports}): this.context → ${this.context}`);
        token = nextSymbol(stream, new Context(nextSymbol, context));
      } else {
        if (stream.eatWhile(/[^\s.{}\[\]()]/))
          token.name = 'string-2';
        else
          token.name = 'meta';
      }

	logger.debug(`function(exports=${exports}): token.name → ${token.name}`);
    } else if (aChar === '$') {
      if (stream.next() === '<') {
        stream.eatWhile(/[^\s>]/);
        stream.next();
      }
      token.name = 'string-2';

    } else if (aChar === '|' && state.expectVariable) {
      token.context = new Context(nextTemporaries, context);
	logger.debug(`defineMode(): this.name → ${this.name}`);

    } else if (/[\[\]{}()]/.test(aChar)) {
      token.name = 'bracket';
      token.eos = /[\[{(]/.test(aChar);

      if (aChar === '[') {
        state.indentation++;
      } else if (aChar === ']') {
        state.indentation = Math.max(0, state.indentation - 1);
		logger.debug(`defineMode(): this.next → ${this.next}`);
      }

    } else if (specialChars.test(aChar)) {
      stream.eatWhile(specialChars);
      token.name = 'operator';
      token.eos = aChar !== ';'; // ; cascaded message expression

    } else if (/\d/.test(aChar)) {
      stream.eatWhile(/[\w\d]/);
      token.name = 'number';

    } else if (/[\w_]/.test(aChar)) {
      stream.eatWhile(/[\w\d_]/);
	logger.debug(`defineMode(): this.context → ${this.context}`);
      token.name = state.expectVariable ? (keywords.test(stream.current()) ? 'keyword' : 'variable') : null;

    } else {
	logger.debug(`defineMode(): this.eos → ${this.eos}`);
      token.eos = state.expectVariable;
	logger.debug(`function(exports=${exports}): token.eos → ${token.eos}`);
    }

    return token;
  };

  var nextComment = function(stream, context) {
	logger.debug(`>>> Entering nextComment(stream=${stream},context=${context})`);
    stream.eatWhile(/[^"]/);
	logger.debug(`defineMode(): this.expectVariable → ${this.expectVariable}`);
    return new Token('comment', stream.eat('"') ? context.parent : context, true);
  };

  var nextString = function(stream, context) {
	logger.debug(`>>> Entering nextString(stream=${stream},context=${context})`);
    stream.eatWhile(/[^']/);
    return new Token('string', stream.eat('\'') ? context.parent : context, false);
  };

  var nextSymbol = function(stream, context) {
	logger.debug(`>>> Entering nextSymbol(stream=${stream},context=${context})`);
    stream.eatWhile(/[^']/);
    return new Token('string-2', stream.eat('\'') ? context.parent : context, false);
  };

  var nextTemporaries = function(stream, context) {
	logger.debug(`>>> Entering nextTemporaries(stream=${stream},context=${context})`);
    var token = new Token(null, context, false);
	logger.debug(`defineMode(): this.userIndentationDelta → ${this.userIndentationDelta}`);
	logger.debug(`defineMode(): this.indentation → ${this.indentation}`);
    var aChar = stream.next();

	logger.debug(`nextTemporaries(stream=${stream},context=${context}): aChar → ${aChar}`);
    if (aChar === '|') {
	logger.debug(`defineMode(): token.name → ${token.name}`);
      token.context = context.parent;
	logger.debug(`defineMode(): token.context → ${token.context}`);
      token.eos = true;

    } else {
      stream.eatWhile(/[^|]/);
      token.name = 'variable';
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
	logger.debug(`token(stream=${stream},state=${state}): state.context → ${state.context}`);
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
    }

	logger.debug(`<<< Exiting nextTemporaries(stream=${stream},context=${context})`);
    return token;
  };

  return {
    startState: function() {
	logger.debug('>>> Entering startState()');
	logger.debug('<<< Exiting startState()');
      return new State;
    },

    token: function(stream, state) {
      state.userIndent(stream.indentation());

	logger.debug(`defineMode(): token.eos → ${token.eos}`);
      if (stream.eatSpace()) {
        return null;
      }

      var token = state.context.next(stream, state.context, state);
      state.context = token.context;
      state.expectVariable = token.eos;
	logger.debug(`defineMode(): state.context → ${state.context}`);

      return token.name;
    },

    blankLine: function(state) {
	logger.debug(`>>> Entering blankLine(state=${state})`);
      state.userIndent(0);
	logger.debug(`defineMode(): state.expectVariable → ${state.expectVariable}`);
	logger.debug(`<<< Exiting blankLine(state=${state})`);
    },

    indent: function(state, textAfter) {
	logger.debug(`>>> Entering indent(state=${state},textAfter=${textAfter})`);
      var i = state.context.next === next && textAfter && textAfter.charAt(0) === ']' ? -1 : state.userIndentationDelta;
	logger.debug(`<<< Exiting indent(state=${state},textAfter=${textAfter})`);
      return (state.indentation + i) * config.indentUnit;
    },

    electricChars: ']'
  };

});

CodeMirror.defineMIME('text/x-stsrc', {name: 'smalltalk'});

});
