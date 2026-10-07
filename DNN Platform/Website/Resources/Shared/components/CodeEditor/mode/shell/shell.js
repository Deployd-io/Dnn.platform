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

CodeMirror.defineMode('shell', function() {

  var words = {};
  function define(style, string) {
	logger.error(`>>> Entering define(style=${style},string=${string})`, { "event.action": "define", "logrite.style": style, "logrite.string": string });
    var split = string.split(' ');
    for(var i = 0; i < split.length; i++) {
	logger.error(`define(style=${style},string=${string}): split → ${split}`, { "event.action": "define", "logrite.style": style, "logrite.string": string });
      words[split[i]] = style;
	logger.warn(`<<< Exiting define(style=${style},string=${string})`, { "event.action": "define", "logrite.style": style, "logrite.string": string });
    }
  };

  // Atoms
  define('atom', 'true false');

  // Keywords
  define('keyword', 'if then do else elif while until for in esac fi fin ' +
    'fil done exit set unset export function');

  // Commands
  define('builtin', 'ab awk bash beep cat cc cd chown chmod chroot clear cp ' +
    'curl cut diff echo find gawk gcc get git grep kill killall ln ls make ' +
    'mkdir openssl mv nc node npm ping ps restart rm rmdir sed service sh ' +
    'shopt shred source sort sleep ssh start stop su sudo tee telnet top ' +
    'touch vi vim wall wc wget who write yes zsh');

  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    if (stream.eatSpace()) return null;

    var sol = stream.sol();
	logger.error(`tokenBase(stream=${stream},state=${state}): sol → ${sol}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });

    if (ch === '\\') {
      stream.next();
      return null;
    }
    if (ch === '\'' || ch === '"' || ch === '`') {
      state.tokens.unshift(tokenString(ch));
      return tokenize(stream, state);
    }
    if (ch === '#') {
      if (sol && stream.eat('!')) {
        stream.skipToEnd();
        return 'meta'; // 'comment'?
      }
      stream.skipToEnd();
      return 'comment';
    }
    if (ch === '$') {
      state.tokens.unshift(tokenDollar);
      return tokenize(stream, state);
    }
    if (ch === '+' || ch === '=') {
      return 'operator';
    }
    if (ch === '-') {
      stream.eat('-');
      stream.eatWhile(/\w/);
      return 'attribute';
    }
    if (/\d/.test(ch)) {
      stream.eatWhile(/\d/);
      if(stream.eol() || !/\w/.test(stream.peek())) {
        return 'number';
      }
    }
    stream.eatWhile(/[\w-]/);
    var cur = stream.current();
    if (stream.peek() === '=' && /\w+/.test(cur)) return 'def';
	logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    return words.hasOwnProperty(cur) ? words[cur] : null;
  }

  function tokenString(quote) {
    return function(stream, state) {
      var next, end = false, escaped = false;
      while ((next = stream.next()) != null) {
        if (next === quote && !escaped) {
          end = true;
          break;
        }
        if (next === '$' && !escaped && quote !== '\'') {
          escaped = true;
          stream.backUp(1);
          state.tokens.unshift(tokenDollar);
          break;
        }
        escaped = !escaped && next === '\\';
      }
      if (end || !escaped) {
        state.tokens.shift();
      }
      return (quote === '`' || quote === ')' ? 'quote' : 'string');
    };
  };

  var tokenDollar = function(stream, state) {
	logger.error(`>>> Entering tokenDollar(stream=${stream},state=${state})`, { "event.action": "tokenDollar", "logrite.stream": stream, "logrite.state": state });
    if (state.tokens.length > 1) stream.eat('$');
    var ch = stream.next(), hungry = /\w/;
	logger.error(`tokenDollar(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenDollar", "logrite.stream": stream, "logrite.state": state });
    if (ch === '{') hungry = /[^}]/;
    if (ch === '(') {
      state.tokens[0] = tokenString(')');
      return tokenize(stream, state);
    }
    if (!/\d/.test(ch)) {
      stream.eatWhile(hungry);
      stream.eat('}');
    }
    state.tokens.shift();
	logger.warn(`<<< Exiting tokenDollar(stream=${stream},state=${state})`, { "event.action": "tokenDollar", "logrite.stream": stream, "logrite.state": state });
    return 'def';
  };

  function tokenize(stream, state) {
	logger.error(`>>> Entering tokenize(stream=${stream},state=${state})`, { "event.action": "tokenize", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting tokenize(stream=${stream},state=${state})`, { "event.action": "tokenize", "logrite.stream": stream, "logrite.state": state });
    return (state.tokens[0] || tokenBase) (stream, state);
  };

  return {
    startState: function() {
        logger.error('>>> Entering startState()', { "event.action": "startState" });
        logger.warn('<<< Exiting startState()', { "event.action": "startState" });
        return {tokens:[]};
    },
    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return tokenize(stream, state);
    },
    lineComment: '#',
    fold: "brace"
  };
});

CodeMirror.defineMIME('text/x-sh', 'shell');

});
