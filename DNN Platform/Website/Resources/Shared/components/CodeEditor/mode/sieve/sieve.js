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
CodeMirror.defineMode("sieve", function(config) {
  function words(str) {
	logger.debug(`>>> Entering words(str=${str})`);
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.debug(`words(str=${str}): obj → ${obj}`);
	logger.debug(`<<< Exiting words(str=${str})`);
    return obj;
  }

  var keywords = words("if elsif else stop require");
  var atoms = words("true false not");
  var indentUnit = config.indentUnit;

  function tokenBase(stream, state) {
logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);

    var ch = stream.next();
	logger.debug(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`);
    if (ch == "/" && stream.eat("*")) {
      state.tokenize = tokenCComment;
	logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
      return tokenCComment(stream, state);
    }

    if (ch === '#') {
      stream.skipToEnd();
      return "comment";
    }

    if (ch == "\"") {
      state.tokenize = tokenString(ch);
      return state.tokenize(stream, state);
    }

    if (ch == "(") {
      state._indent.push("(");
      // add virtual angel wings so that editor behaves...
      // ...more sane incase of broken brackets
      state._indent.push("{");
      return null;
    }

    if (ch === "{") {
      state._indent.push("{");
      return null;
    }

    if (ch == ")")  {
      state._indent.pop();
      state._indent.pop();
    }

    if (ch === "}") {
      state._indent.pop();
      return null;
    }

    if (ch == ",")
      return null;

    if (ch == ";")
      return null;


    if (/[{}\(\),;]/.test(ch))
      return null;

    // 1*DIGIT "K" / "M" / "G"
    if (/\d/.test(ch)) {
      stream.eatWhile(/[\d]/);
      stream.eat(/[KkMmGg]/);
      return "number";
    }

    // ":" (ALPHA / "_") *(ALPHA / DIGIT / "_")
    if (ch == ":") {
      stream.eatWhile(/[a-zA-Z_]/);
      stream.eatWhile(/[a-zA-Z0-9_]/);

      return "operator";
    }

    stream.eatWhile(/\w/);
    var cur = stream.current();
	logger.debug(`tokenBase(stream=${stream},state=${state}): cur → ${cur}`);

    // "text:" *(SP / HTAB) (hash-comment / CRLF)
    // *(multiline-literal / multiline-dotstart)
    // "." CRLF
    if ((cur == "text") && stream.eat(":"))
    {
      state.tokenize = tokenMultiLineString;
      return "string";
    }

    if (keywords.propertyIsEnumerable(cur))
      return "keyword";

    if (atoms.propertyIsEnumerable(cur))
      return "atom";

	logger.debug(`<<< Exiting tokenBase(stream=${stream},state=${state})`);
    return null;
  }

  function tokenMultiLineString(stream, state)
  {
    state._multiLineString = true;
	logger.debug(`tokenMultiLineString(stream=${stream},state=${state}): state._multiLineString → ${state._multiLineString}`);
    // the first line is special it may contain a comment
    if (!stream.sol()) {
	logger.debug(`>>> Entering tokenMultiLineString(stream=${stream},state=${state})`);
      stream.eatSpace();

      if (stream.peek() == "#") {
        stream.skipToEnd();
        return "comment";
      }

      stream.skipToEnd();
      return "string";
    }

    if ((stream.next() == ".")  && (stream.eol()))
    {
      state._multiLineString = false;
      state.tokenize = tokenBase;
	logger.debug(`tokenMultiLineString(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
    }

	logger.debug(`<<< Exiting tokenMultiLineString(stream=${stream},state=${state})`);
    return "string";
  }

  function tokenCComment(stream, state) {
	logger.debug(`>>> Entering tokenCComment(stream=${stream},state=${state})`);
    var maybeEnd = false, ch;
    while ((ch = stream.next()) != null) {
      if (maybeEnd && ch == "/") {
        state.tokenize = tokenBase;
		logger.debug(`tokenCComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        break;
      }
      maybeEnd = (ch == "*");
    }
	logger.debug(`tokenCComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
	logger.debug(`<<< Exiting tokenCComment(stream=${stream},state=${state})`);
    return "comment";
  }

  function tokenString(quote) {
	logger.debug(`>>> Entering tokenString(quote=${quote})`);
    return function(stream, state) {
      var escaped = false, ch;
      while ((ch = stream.next()) != null) {
        if (ch == quote && !escaped)
          break;
        escaped = !escaped && ch == "\\";
      }
      if (!escaped) state.tokenize = tokenBase;
	logger.debug(`<<< Exiting tokenString(quote=${quote})`);
      return "string";
    };
  }

  return {
    startState: function(base) {
	logger.debug(`>>> Entering startState(base=${base})`);
      return {tokenize: tokenBase,
              baseIndent: base || 0,
              _indent: []};
			logger.debug(`<<< Exiting startState(base=${base})`);
    },

    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      if (stream.eatSpace())
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
        return null;

      return (state.tokenize || tokenBase)(stream, state);;
    },

    indent: function(state, _textAfter) {
	logger.debug(`>>> Entering indent(state=${state},_textAfter=${_textAfter})`);
      var length = state._indent.length;
      if (_textAfter && (_textAfter[0] == "}"))
        length--;

      if (length <0)
        length = 0;

	logger.debug(`<<< Exiting indent(state=${state},_textAfter=${_textAfter})`);
      return length * indentUnit;
    },

    electricChars: "}"
  };
});

CodeMirror.defineMIME("application/sieve", "sieve");

});
