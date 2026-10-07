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

CodeMirror.defineMode("sass", function(config) {
  function tokenRegexp(words) {
	logger.error(`>>> Entering tokenRegexp(words=${words})`, { "event.action": "tokenRegexp", "logrite.words": words });
	logger.warn(`<<< Exiting tokenRegexp(words=${words})`, { "event.action": "tokenRegexp", "logrite.words": words });
    return new RegExp("^" + words.join("|"));
  }

  var keywords = ["true", "false", "null", "auto"];
  var keywordsRegexp = new RegExp("^" + keywords.join("|"));

  var operators = ["\\(", "\\)", "=", ">", "<", "==", ">=", "<=", "\\+", "-",
                   "\\!=", "/", "\\*", "%", "and", "or", "not", ";","\\{","\\}",":"];
  var opRegexp = tokenRegexp(operators);

  var pseudoElementsRegexp = /^::?[a-zA-Z_][\w\-]*/;

  function urlTokens(stream, state) {
	logger.error(`>>> Entering urlTokens(stream=${stream},state=${state})`, { "event.action": "urlTokens", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.peek();

	logger.error(`urlTokens(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "urlTokens", "logrite.stream": stream, "logrite.state": state });
    if (ch === ")") {
      stream.next();
      state.tokenizer = tokenBase;
	logger.debug(`urlTokens(stream=${stream},state=${state}): state.tokenizer → ${state.tokenizer}`, { "event.action": "urlTokens", "logrite.stream": stream, "logrite.state": state });
      return "operator";
    } else if (ch === "(") {
      stream.next();
      stream.eatSpace();

      return "operator";
    } else if (ch === "'" || ch === '"') {
      state.tokenizer = buildStringTokenizer(stream.next());
      return "string";
    } else {
      state.tokenizer = buildStringTokenizer(")", false);
	logger.warn(`<<< Exiting urlTokens(stream=${stream},state=${state})`, { "event.action": "urlTokens", "logrite.stream": stream, "logrite.state": state });
      return "string";
    }
  }
  function comment(indentation, multiLine) {
	logger.error(`>>> Entering comment(indentation=${indentation},multiLine=${multiLine})`, { "event.action": "comment", "logrite.indentation": indentation, "logrite.multiLine": multiLine });
    return function(stream, state) {
      if (stream.sol() && stream.indentation() <= indentation) {
        state.tokenizer = tokenBase;
        return tokenBase(stream, state);
      }

      if (multiLine && stream.skipTo("*/")) {
        stream.next();
        stream.next();
        state.tokenizer = tokenBase;
      } else {
        stream.skipToEnd();
      }

      return "comment";
    };
  }

  function buildStringTokenizer(quote, greedy) {
	logger.error(`>>> Entering buildStringTokenizer(quote=${quote},greedy=${greedy})`, { "event.action": "buildStringTokenizer", "logrite.quote": quote, "logrite.greedy": greedy });
    if (greedy == null) { greedy = true; }

    function stringTokenizer(stream, state) {
	logger.error(`>>> Entering stringTokenizer(stream=${stream},state=${state})`, { "event.action": "stringTokenizer", "logrite.stream": stream, "logrite.state": state });
      var nextChar = stream.next();
      var peekChar = stream.peek();
	logger.error(`stringTokenizer(stream=${stream},state=${state}): nextChar → ${nextChar}`, { "event.action": "stringTokenizer", "logrite.stream": stream, "logrite.state": state });
	logger.error(`stringTokenizer(stream=${stream},state=${state}): peekChar → ${peekChar}`, { "event.action": "stringTokenizer", "logrite.stream": stream, "logrite.state": state });
      var previousChar = stream.string.charAt(stream.pos-2);

	logger.error(`stringTokenizer(stream=${stream},state=${state}): previousChar → ${previousChar}`, { "event.action": "stringTokenizer", "logrite.stream": stream, "logrite.state": state });
      var endingString = ((nextChar !== "\\" && peekChar === quote) || (nextChar === quote && previousChar !== "\\"));
	logger.error(`stringTokenizer(stream=${stream},state=${state}): endingString → ${endingString}`, { "event.action": "stringTokenizer", "logrite.stream": stream, "logrite.state": state });

      if (endingString) {
        if (nextChar !== quote && greedy) { stream.next(); }
        state.tokenizer = tokenBase;
		logger.debug(`stringTokenizer(stream=${stream},state=${state}): state.tokenizer → ${state.tokenizer}`, { "event.action": "stringTokenizer", "logrite.stream": stream, "logrite.state": state });
        return "string";
      } else if (nextChar === "#" && peekChar === "{") {
        state.tokenizer = buildInterpolationTokenizer(stringTokenizer);
        stream.next();
        return "operator";
      } else {
	logger.warn(`<<< Exiting stringTokenizer(stream=${stream},state=${state})`, { "event.action": "stringTokenizer", "logrite.stream": stream, "logrite.state": state });
        return "string";
      }
    }

	logger.warn(`<<< Exiting buildStringTokenizer(quote=${quote},greedy=${greedy})`, { "event.action": "buildStringTokenizer", "logrite.quote": quote, "logrite.greedy": greedy });
    return stringTokenizer;
  }

  function buildInterpolationTokenizer(currentTokenizer) {
    return function(stream, state) {
      if (stream.peek() === "}") {
        stream.next();
        state.tokenizer = currentTokenizer;
        return "operator";
      } else {
        return tokenBase(stream, state);
      }
    };
  }

  function indent(state) {
	logger.error(`>>> Entering indent(state=${state})`, { "event.action": "indent", "logrite.state": state });
    if (state.indentCount == 0) {
      state.indentCount++;
      var lastScopeOffset = state.scopes[0].offset;
      var currentOffset = lastScopeOffset + config.indentUnit;
      state.scopes.unshift({ offset:currentOffset });
	logger.warn(`<<< Exiting indent(state=${state})`, { "event.action": "indent", "logrite.state": state });
    }
  }

  function dedent(state) {
	logger.error(`>>> Entering dedent(state=${state})`, { "event.action": "dedent", "logrite.state": state });
    if (state.scopes.length == 1) return;

    state.scopes.shift();
	logger.warn(`<<< Exiting dedent(state=${state})`, { "event.action": "dedent", "logrite.state": state });
  }

  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.peek();

	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    // Comment
    if (stream.match("/*")) {
      state.tokenizer = comment(stream.indentation(), true);
      return state.tokenizer(stream, state);
    }
    if (stream.match("//")) {
      state.tokenizer = comment(stream.indentation(), false);
      return state.tokenizer(stream, state);
    }

    // Interpolation
    if (stream.match("#{")) {
      state.tokenizer = buildInterpolationTokenizer(tokenBase);
      return "operator";
    }

    // Strings
    if (ch === '"' || ch === "'") {
      stream.next();
      state.tokenizer = buildStringTokenizer(ch);
      return "string";
    }

    if(!state.cursorHalf){// state.cursorHalf === 0
    // first half i.e. before : for key-value pairs
    // including selectors

      if (ch === ".") {
        stream.next();
        if (stream.match(/^[\w-]+/)) {
          indent(state);
          return "atom";
        } else if (stream.peek() === "#") {
          indent(state);
          return "atom";
        }
      }

      if (ch === "#") {
        stream.next();
        // ID selectors
        if (stream.match(/^[\w-]+/)) {
          indent(state);
          return "atom";
        }
        if (stream.peek() === "#") {
          indent(state);
          return "atom";
        }
      }

      // Variables
      if (ch === "$") {
        stream.next();
        stream.eatWhile(/[\w-]/);
        return "variable-2";
      }

      // Numbers
      if (stream.match(/^-?[0-9\.]+/))
        return "number";

      // Units
      if (stream.match(/^(px|em|in)\b/))
        return "unit";

      if (stream.match(keywordsRegexp))
        return "keyword";

      if (stream.match(/^url/) && stream.peek() === "(") {
        state.tokenizer = urlTokens;
		logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenizer → ${state.tokenizer}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
        return "atom";
      }

      if (ch === "=") {
        // Match shortcut mixin definition
        if (stream.match(/^=[\w-]+/)) {
          indent(state);
          return "meta";
        }
      }

      if (ch === "+") {
        // Match shortcut mixin definition
        if (stream.match(/^\+[\w-]+/)){
          return "variable-3";
        }
      }

      if(ch === "@"){
        if(stream.match(/@extend/)){
          if(!stream.match(/\s*[\w]/))
            dedent(state);
        }
      }


      // Indent Directives
      if (stream.match(/^@(else if|if|media|else|for|each|while|mixin|function)/)) {
        indent(state);
        return "meta";
      }

      // Other Directives
      if (ch === "@") {
        stream.next();
        stream.eatWhile(/[\w-]/);
        return "meta";
      }

      if (stream.eatWhile(/[\w-]/)){
        if(stream.match(/ *: *[\w-\+\$#!\("']/,false)){
          return "property";
        }
        else if(stream.match(/ *:/,false)){
          indent(state);
          state.cursorHalf = 1;
		logger.debug(`tokenBase(stream=${stream},state=${state}): state.cursorHalf → ${state.cursorHalf}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
          return "atom";
        }
        else if(stream.match(/ *,/,false)){
          return "atom";
        }
        else{
          indent(state);
          return "atom";
        }
      }

      if(ch === ":"){
        if (stream.match(pseudoElementsRegexp)){ // could be a pseudo-element
          return "keyword";
        }
        stream.next();
        state.cursorHalf=1;
        return "operator";
      }

    } // cursorHalf===0 ends here
    else{

      if (ch === "#") {
        stream.next();
        // Hex numbers
        if (stream.match(/[0-9a-fA-F]{6}|[0-9a-fA-F]{3}/)){
          if(!stream.peek()){
            state.cursorHalf = 0;
          }
          return "number";
        }
      }

      // Numbers
      if (stream.match(/^-?[0-9\.]+/)){
        if(!stream.peek()){
          state.cursorHalf = 0;
        }
        return "number";
      }

      // Units
      if (stream.match(/^(px|em|in)\b/)){
        if(!stream.peek()){
          state.cursorHalf = 0;
        }
        return "unit";
      }

      if (stream.match(keywordsRegexp)){
        if(!stream.peek()){
          state.cursorHalf = 0;
        }
        return "keyword";
      }

      if (stream.match(/^url/) && stream.peek() === "(") {
        state.tokenizer = urlTokens;
        if(!stream.peek()){
          state.cursorHalf = 0;
        }
        return "atom";
      }

      // Variables
      if (ch === "$") {
        stream.next();
        stream.eatWhile(/[\w-]/);
        if(!stream.peek()){
          state.cursorHalf = 0;
        }
        return "variable-3";
      }

      // bang character for !important, !default, etc.
      if (ch === "!") {
        stream.next();
        if(!stream.peek()){
          state.cursorHalf = 0;
        }
        return stream.match(/^[\w]+/) ? "keyword": "operator";
      }

      if (stream.match(opRegexp)){
        if(!stream.peek()){
          state.cursorHalf = 0;
        }
        return "operator";
      }

      // attributes
      if (stream.eatWhile(/[\w-]/)) {
        if(!stream.peek()){
          state.cursorHalf = 0;
        }
        return "attribute";
      }

      //stream.eatSpace();
      if(!stream.peek()){
        state.cursorHalf = 0;
        return null;
      }

    } // else ends here

    if (stream.match(opRegexp))
      return "operator";

    // If we haven't returned by now, we move 1 character
    // and return an error
    stream.next();
	logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    return null;
  }

  function tokenLexer(stream, state) {
	logger.error(`>>> Entering tokenLexer(stream=${stream},state=${state})`, { "event.action": "tokenLexer", "logrite.stream": stream, "logrite.state": state });
    if (stream.sol()) state.indentCount = 0;
    var style = state.tokenizer(stream, state);
	logger.error(`tokenLexer(stream=${stream},state=${state}): style → ${style}`, { "event.action": "tokenLexer", "logrite.stream": stream, "logrite.state": state });
    var current = stream.current();
	logger.error(`tokenLexer(stream=${stream},state=${state}): current → ${current}`, { "event.action": "tokenLexer", "logrite.stream": stream, "logrite.state": state });

    if (current === "@return" || current === "}"){
      dedent(state);
    }

    if (style !== null) {
      var startOfToken = stream.pos - current.length;

      var withCurrentIndent = startOfToken + (config.indentUnit * state.indentCount);
	logger.error(`tokenLexer(stream=${stream},state=${state}): withCurrentIndent → ${withCurrentIndent}`, { "event.action": "tokenLexer", "logrite.stream": stream, "logrite.state": state });

      var newScopes = [];

      for (var i = 0; i < state.scopes.length; i++) {
        var scope = state.scopes[i];

        if (scope.offset <= withCurrentIndent)
          newScopes.push(scope);
      }

      state.scopes = newScopes;
	logger.debug(`tokenLexer(stream=${stream},state=${state}): state.scopes → ${state.scopes}`, { "event.action": "tokenLexer", "logrite.stream": stream, "logrite.state": state });
    }


	logger.warn(`<<< Exiting tokenLexer(stream=${stream},state=${state})`, { "event.action": "tokenLexer", "logrite.stream": stream, "logrite.state": state });
    return style;
  }

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      return {
        tokenizer: tokenBase,
        scopes: [{offset: 0, type: "sass"}],
        indentCount: 0,
        cursorHalf: 0,  // cursor half tells us if cursor lies after (1)
                        // or before (0) colon (well... more or less)
        definedVars: [],
        definedMixins: []
      };
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },
    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      var style = tokenLexer(stream, state);

      state.lastToken = { style: style, content: stream.current() };
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return style;
    },

    indent: function(state) {
      return state.scopes[0].offset;
    }
  };
});

CodeMirror.defineMIME("text/x-sass", "sass");

});
