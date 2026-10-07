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

CodeMirror.defineMode("dylan", function(_config) {
  // Words
  var words = {
    // Words that introduce unnamed definitions like "define interface"
    unnamedDefinition: ["interface"],

    // Words that introduce simple named definitions like "define library"
    namedDefinition: ["module", "library", "macro",
                      "C-struct", "C-union",
                      "C-function", "C-callable-wrapper"
                     ],

    // Words that introduce type definitions like "define class".
    // These are also parameterized like "define method" and are
    // appended to otherParameterizedDefinitionWords
    typeParameterizedDefinition: ["class", "C-subtype", "C-mapped-subtype"],

    // Words that introduce trickier definitions like "define method".
    // These require special definitions to be added to startExpressions
    otherParameterizedDefinition: ["method", "function",
                                   "C-variable", "C-address"
                                  ],

    // Words that introduce module constant definitions.
    // These must also be simple definitions and are
    // appended to otherSimpleDefinitionWords
    constantSimpleDefinition: ["constant"],

    // Words that introduce module variable definitions.
    // These must also be simple definitions and are
    // appended to otherSimpleDefinitionWords
    variableSimpleDefinition: ["variable"],

    // Other words that introduce simple definitions
    // (without implicit bodies).
    otherSimpleDefinition: ["generic", "domain",
                            "C-pointer-type",
                            "table"
                           ],

    // Words that begin statements with implicit bodies.
    statement: ["if", "block", "begin", "method", "case",
                "for", "select", "when", "unless", "until",
                "while", "iterate", "profiling", "dynamic-bind"
               ],

    // Patterns that act as separators in compound statements.
    // This may include any general pattern that must be indented
    // specially.
    separator: ["finally", "exception", "cleanup", "else",
                "elseif", "afterwards"
               ],

    // Keywords that do not require special indentation handling,
    // but which should be highlighted
    other: ["above", "below", "by", "from", "handler", "in",
            "instance", "let", "local", "otherwise", "slot",
            "subclass", "then", "to", "keyed-by", "virtual"
           ],

    // Condition signaling function calls
    signalingCalls: ["signal", "error", "cerror",
                     "break", "check-type", "abort"
                    ]
  };

  words["otherDefinition"] =
    words["unnamedDefinition"]
    .concat(words["namedDefinition"])
    .concat(words["otherParameterizedDefinition"]);

  words["definition"] =
    words["typeParameterizedDefinition"]
    .concat(words["otherDefinition"]);

  words["parameterizedDefinition"] =
    words["typeParameterizedDefinition"]
    .concat(words["otherParameterizedDefinition"]);

  words["simpleDefinition"] =
    words["constantSimpleDefinition"]
    .concat(words["variableSimpleDefinition"])
    .concat(words["otherSimpleDefinition"]);

  words["keyword"] =
    words["statement"]
    .concat(words["separator"])
    .concat(words["other"]);

  // Patterns
  var symbolPattern = "[-_a-zA-Z?!*@<>$%]+";
  var symbol = new RegExp("^" + symbolPattern);
  var patterns = {
    // Symbols with special syntax
    symbolKeyword: symbolPattern + ":",
    symbolClass: "<" + symbolPattern + ">",
    symbolGlobal: "\\*" + symbolPattern + "\\*",
    symbolConstant: "\\$" + symbolPattern
  };
  var patternStyles = {
    symbolKeyword: "atom",
    symbolClass: "tag",
    symbolGlobal: "variable-2",
    symbolConstant: "variable-3"
  };

  // Compile all patterns to regular expressions
  for (var patternName in patterns)
    if (patterns.hasOwnProperty(patternName))
      patterns[patternName] = new RegExp("^" + patterns[patternName]);

  // Names beginning "with-" and "without-" are commonly
  // used as statement macro
  patterns["keyword"] = [/^with(?:out)?-[-_a-zA-Z?!*@<>$%]+/];

  var styles = {};
  styles["keyword"] = "keyword";
  styles["definition"] = "def";
  styles["simpleDefinition"] = "def";
  styles["signalingCalls"] = "builtin";

  // protected words lookup table
  var wordLookup = {};
  var styleLookup = {};

  [
    "keyword",
    "definition",
    "simpleDefinition",
    "signalingCalls"
  ].forEach(function(type) {
    words[type].forEach(function(word) {
      wordLookup[word] = type;
      styleLookup[word] = styles[type];
    });
  });


  function chain(stream, state, f) {
	logger.error(`>>> Entering chain(stream=${stream},state=${state},f=${f})`, { "event.action": "chain", "logrite.stream": stream, "logrite.state": state, "logrite.f": f });
    state.tokenize = f;
	logger.warn(`<<< Exiting chain(stream=${stream},state=${state},f=${f})`, { "event.action": "chain", "logrite.stream": stream, "logrite.state": state, "logrite.f": f });
    return f(stream, state);
  }

  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    // String
    var ch = stream.peek();
	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    if (ch == "'" || ch == '"') {
      stream.next();
      return chain(stream, state, tokenString(ch, "string"));
    }
    // Comment
    else if (ch == "/") {
      stream.next();
      if (stream.eat("*")) {
        return chain(stream, state, tokenComment);
      } else if (stream.eat("/")) {
        stream.skipToEnd();
        return "comment";
      } else {
        stream.skipTo(" ");
        return "operator";
      }
    }
    // Decimal
    else if (/\d/.test(ch)) {
      stream.match(/^\d*(?:\.\d*)?(?:e[+\-]?\d+)?/);
      return "number";
    }
    // Hash
    else if (ch == "#") {
      stream.next();
      // Symbol with string syntax
      ch = stream.peek();
      if (ch == '"') {
        stream.next();
        return chain(stream, state, tokenString('"', "string-2"));
      }
      // Binary number
      else if (ch == "b") {
        stream.next();
        stream.eatWhile(/[01]/);
        return "number";
      }
      // Hex number
      else if (ch == "x") {
        stream.next();
        stream.eatWhile(/[\da-f]/i);
        return "number";
      }
      // Octal number
      else if (ch == "o") {
        stream.next();
        stream.eatWhile(/[0-7]/);
        return "number";
      }
      // Hash symbol
      else {
        stream.eatWhile(/[-a-zA-Z]/);
        return "keyword";
      }
    } else if (stream.match("end")) {
      return "keyword";
    }
    for (var name in patterns) {
      if (patterns.hasOwnProperty(name)) {
        var pattern = patterns[name];
        if ((pattern instanceof Array && pattern.some(function(p) {
          return stream.match(p);
        })) || stream.match(pattern))
          return patternStyles[name];
      }
    }
    if (stream.match("define")) {
      return "def";
    } else {
      stream.eatWhile(/[\w\-]/);
      // Keyword
      if (wordLookup[stream.current()]) {
        return styleLookup[stream.current()];
      } else if (stream.current().match(symbol)) {
        return "variable";
      } else {
        stream.next();
        return "variable-2";
      }
    }
  }

  function tokenComment(stream, state) {
	logger.error(`>>> Entering tokenComment(stream=${stream},state=${state})`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
    var maybeEnd = false,
    ch;
    while ((ch = stream.next())) {
      if (ch == "/" && maybeEnd) {
        state.tokenize = tokenBase;
		logger.debug(`tokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
        break;
      }
      maybeEnd = (ch == "*");
    }
	logger.error(`tokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`, { "event.action": "tokenComment", "logrite.stream": stream, "logrite.state": state });
    return "comment";
  }

  function tokenString(quote, style) {
	logger.error(`>>> Entering tokenString(quote=${quote},style=${style})`, { "event.action": "tokenString", "logrite.quote": quote, "logrite.style": style });
    return function(stream, state) {
      var next, end = false;
      while ((next = stream.next()) != null) {
        if (next == quote) {
          end = true;
          break;
        }
      }
      if (end)
        state.tokenize = tokenBase;
      return style;
    };
  }

  // Interface
  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      return {
        tokenize: tokenBase,
        currentIndent: 0
      };
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },
    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.eatSpace())
        return null;
      var style = state.tokenize(stream, state);
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return style;
    },
    blockCommentStart: "/*",
    blockCommentEnd: "*/"
  };
});

CodeMirror.defineMIME("text/x-dylan", "dylan");

});
