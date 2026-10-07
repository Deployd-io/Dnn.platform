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

CodeMirror.defineMode('smalltalk', function(config) {

  var specialChars = /[+\-\/\\*~<>=@%|&?!.,:;^]/;
  var keywords = /true|false|nil|self|super|thisContext/;

  var Context = function(tokenizer, parent) {
    this.next = tokenizer;
    this.parent = parent;
  };

  var Token = function(name, context, eos) {
	logger.error(`>>> Entering Token(name=${name},context=${context},eos=${eos})`, { "event.action": "Token", "logrite.name": name, "logrite.context": context, "logrite.eos": eos });
    this.name = name;
    this.context = context;
	logger.debug(`Token(name=${name},context=${context},eos=${eos}): name → ${name}`, { "event.action": "Token", "logrite.name": name, "logrite.context": context, "logrite.eos": eos });
	logger.debug(`Token(name=${name},context=${context},eos=${eos}): this.name → ${this.name}`, { "event.action": "Token", "logrite.name": name, "logrite.context": context, "logrite.eos": eos });
    this.eos = eos;
  };

  var State = function() {
	logger.error('>>> Entering State()', { "event.action": "State" });
    this.context = new Context(next, null);
    this.expectVariable = true;
    this.indentation = 0;
    this.userIndentationDelta = 0;
	logger.debug(`State(): this.userIndentationDelta → ${this.userIndentationDelta}`, { "event.action": "State" });
  };

  State.prototype.userIndent = function(indentation) {
	logger.error(`>>> Entering userIndent(indentation=${indentation})`, { "event.action": "userIndent", "logrite.indentation": indentation });
    this.userIndentationDelta = indentation > 0 ? (indentation / config.indentUnit - this.indentation) : 0;
	logger.warn(`<<< Exiting userIndent(indentation=${indentation})`, { "event.action": "userIndent", "logrite.indentation": indentation });
  };

  var next = function(stream, context, state) {
	logger.error(`>>> Entering next(stream=${stream},context=${context},state=${state})`, { "event.action": "next", "logrite.stream": stream, "logrite.context": context, "logrite.state": state });
    var token = new Token(null, context, false);
    var aChar = stream.next();
	logger.error(`next(stream=${stream},context=${context},state=${state}): aChar → ${aChar}`, { "event.action": "next", "logrite.stream": stream, "logrite.context": context, "logrite.state": state });

    if (aChar === '"') {
      token = nextComment(stream, new Context(nextComment, context));

    } else if (aChar === '\'') {
      token = nextString(stream, new Context(nextString, context));

    } else if (aChar === '#') {
      if (stream.peek() === '\'') {
        stream.next();
        token = nextSymbol(stream, new Context(nextSymbol, context));
      } else {
        if (stream.eatWhile(/[^\s.{}\[\]()]/))
          token.name = 'string-2';
        else
          token.name = 'meta';
      }

    } else if (aChar === '$') {
      if (stream.next() === '<') {
        stream.eatWhile(/[^\s>]/);
        stream.next();
      }
      token.name = 'string-2';

    } else if (aChar === '|' && state.expectVariable) {
      token.context = new Context(nextTemporaries, context);

    } else if (/[\[\]{}()]/.test(aChar)) {
      token.name = 'bracket';
      token.eos = /[\[{(]/.test(aChar);

      if (aChar === '[') {
        state.indentation++;
      } else if (aChar === ']') {
        state.indentation = Math.max(0, state.indentation - 1);
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
      token.name = state.expectVariable ? (keywords.test(stream.current()) ? 'keyword' : 'variable') : null;

    } else {
      token.eos = state.expectVariable;
    }

	logger.warn(`<<< Exiting next(stream=${stream},context=${context},state=${state})`, { "event.action": "next", "logrite.stream": stream, "logrite.context": context, "logrite.state": state });
    return token;
  };

  var nextComment = function(stream, context) {
	logger.error(`>>> Entering nextComment(stream=${stream},context=${context})`, { "event.action": "nextComment", "logrite.stream": stream, "logrite.context": context });
    stream.eatWhile(/[^"]/);
	logger.warn(`<<< Exiting nextComment(stream=${stream},context=${context})`, { "event.action": "nextComment", "logrite.stream": stream, "logrite.context": context });
    return new Token('comment', stream.eat('"') ? context.parent : context, true);
  };

  var nextString = function(stream, context) {
	logger.error(`>>> Entering nextString(stream=${stream},context=${context})`, { "event.action": "nextString", "logrite.stream": stream, "logrite.context": context });
    stream.eatWhile(/[^']/);
	logger.warn(`<<< Exiting nextString(stream=${stream},context=${context})`, { "event.action": "nextString", "logrite.stream": stream, "logrite.context": context });
    return new Token('string', stream.eat('\'') ? context.parent : context, false);
  };

  var nextSymbol = function(stream, context) {
	logger.error(`>>> Entering nextSymbol(stream=${stream},context=${context})`, { "event.action": "nextSymbol", "logrite.stream": stream, "logrite.context": context });
    stream.eatWhile(/[^']/);
	logger.warn(`<<< Exiting nextSymbol(stream=${stream},context=${context})`, { "event.action": "nextSymbol", "logrite.stream": stream, "logrite.context": context });
    return new Token('string-2', stream.eat('\'') ? context.parent : context, false);
  };

  var nextTemporaries = function(stream, context) {
	logger.error(`>>> Entering nextTemporaries(stream=${stream},context=${context})`, { "event.action": "nextTemporaries", "logrite.stream": stream, "logrite.context": context });
    var token = new Token(null, context, false);
    var aChar = stream.next();
	logger.error(`nextTemporaries(stream=${stream},context=${context}): aChar → ${aChar}`, { "event.action": "nextTemporaries", "logrite.stream": stream, "logrite.context": context });

    if (aChar === '|') {
      token.context = context.parent;
      token.eos = true;

    } else {
      stream.eatWhile(/[^|]/);
      token.name = 'variable';
    }

	logger.warn(`<<< Exiting nextTemporaries(stream=${stream},context=${context})`, { "event.action": "nextTemporaries", "logrite.stream": stream, "logrite.context": context });
    return token;
  };

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      return new State;
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      state.userIndent(stream.indentation());

      if (stream.eatSpace()) {
        return null;
      }

      var token = state.context.next(stream, state.context, state);
      state.context = token.context;
	logger.debug(`token(stream=${stream},state=${state}): state.context → ${state.context}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      state.expectVariable = token.eos;
	logger.debug(`token(stream=${stream},state=${state}): state.expectVariable → ${state.expectVariable}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

      return token.name;
    },

    blankLine: function(state) {
	logger.error(`>>> Entering blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
      state.userIndent(0);
	logger.warn(`<<< Exiting blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      var i = state.context.next === next && textAfter && textAfter.charAt(0) === ']' ? -1 : state.userIndentationDelta;
	logger.warn(`<<< Exiting indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      return (state.indentation + i) * config.indentUnit;
    },

    electricChars: ']'
  };

});

CodeMirror.defineMIME('text/x-stsrc', {name: 'smalltalk'});

});
