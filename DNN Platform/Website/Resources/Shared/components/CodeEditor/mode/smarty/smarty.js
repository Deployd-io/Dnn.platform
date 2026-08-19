const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/**
 * Smarty 2 and 3 mode.
 */

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
  CodeMirror.defineMode("smarty", function(config, parserConf) {
	logger.debug('>>> Entering defineMode()');
    var rightDelimiter = parserConf.rightDelimiter || "}";
    var leftDelimiter = parserConf.leftDelimiter || "{";
	logger.debug(`defineMode(): rightDelimiter → ${rightDelimiter}`);
	logger.debug(`defineMode(): leftDelimiter → ${leftDelimiter}`);
    var version = parserConf.version || 2;
    var baseMode = CodeMirror.getMode(config, parserConf.baseMode || "null");
	logger.debug(`defineMode(): baseMode → ${baseMode}`);

    var keyFunctions = ["debug", "extends", "function", "include", "literal"];
    var regs = {
      operatorChars: /[+\-*&%=<>!?]/,
      validIdentifier: /[a-zA-Z0-9_]/,
      stringChar: /['"]/
    };

    var last;
    function cont(style, lastType) {
	logger.debug(`>>> Entering cont(style=${style},lastType=${lastType})`);
      last = lastType;
	logger.debug(`cont(style=${style},lastType=${lastType}): last → ${last}`);
	logger.debug(`<<< Exiting cont(style=${style},lastType=${lastType})`);
      return style;
    }

    function chain(stream, state, parser) {
	logger.debug(`>>> Entering chain(stream=${stream},state=${state},parser=${parser})`);
      state.tokenize = parser;
	logger.debug(`chain(stream=${stream},state=${state},parser=${parser}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting chain(stream=${stream},state=${state},parser=${parser})`);
      return parser(stream, state);
    }

    // Smarty 3 allows { and } surrounded by whitespace to NOT slip into Smarty mode
    function doesNotCount(stream, pos) {
	logger.debug(`>>> Entering doesNotCount(stream=${stream},pos=${pos})`);
      if (pos == null) pos = stream.pos;
      return version === 3 && leftDelimiter == "{" &&
        (pos == stream.string.length || /\s/.test(stream.string.charAt(pos)));
		logger.debug(`<<< Exiting doesNotCount(stream=${stream},pos=${pos})`);
    }

    function tokenTop(stream, state) {
	logger.debug(`>>> Entering tokenTop(stream=${stream},state=${state})`);
      var string = stream.string;
      for (var scan = stream.pos;;) {
        var nextMatch = string.indexOf(leftDelimiter, scan);
		logger.debug(`tokenTop(stream=${stream},state=${state}): nextMatch → ${nextMatch}`);
        scan = nextMatch + leftDelimiter.length;
        if (nextMatch == -1 || !doesNotCount(stream, nextMatch + leftDelimiter.length)) break;
      }
      if (nextMatch == stream.pos) {
        stream.match(leftDelimiter);
        if (stream.eat("*")) {
          return chain(stream, state, tokenBlock("comment", "*" + rightDelimiter));
        } else {
          state.depth++;
          state.tokenize = tokenSmarty;
		logger.debug(`tokenTop(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
          last = "startTag";
          return "tag";
        }
      }

      if (nextMatch > -1) stream.string = string.slice(0, nextMatch);
      var token = baseMode.token(stream, state.base);
      if (nextMatch > -1) stream.string = string;
	logger.debug(`<<< Exiting tokenTop(stream=${stream},state=${state})`);
      return token;
    }

    // parsing Smarty content
    function tokenSmarty(stream, state) {
	logger.debug(`>>> Entering tokenSmarty(stream=${stream},state=${state})`);
      if (stream.match(rightDelimiter, true)) {
        if (version === 3) {
          state.depth--;
          if (state.depth <= 0) {
            state.tokenize = tokenTop;
			logger.debug(`tokenSmarty(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
          }
        } else {
          state.tokenize = tokenTop;
        }
        return cont("tag", null);
      }

      if (stream.match(leftDelimiter, true)) {
        state.depth++;
        return cont("tag", "startTag");
      }

      var ch = stream.next();
	logger.debug(`tokenSmarty(stream=${stream},state=${state}): ch → ${ch}`);
      if (ch == "$") {
        stream.eatWhile(regs.validIdentifier);
        return cont("variable-2", "variable");
      } else if (ch == "|") {
        return cont("operator", "pipe");
      } else if (ch == ".") {
        return cont("operator", "property");
      } else if (regs.stringChar.test(ch)) {
        state.tokenize = tokenAttribute(ch);
        return cont("string", "string");
      } else if (regs.operatorChars.test(ch)) {
        stream.eatWhile(regs.operatorChars);
        return cont("operator", "operator");
      } else if (ch == "[" || ch == "]") {
        return cont("bracket", "bracket");
      } else if (ch == "(" || ch == ")") {
        return cont("bracket", "operator");
      } else if (/\d/.test(ch)) {
        stream.eatWhile(/\d/);
        return cont("number", "number");
      } else {

        if (state.last == "variable") {
          if (ch == "@") {
            stream.eatWhile(regs.validIdentifier);
            return cont("property", "property");
          } else if (ch == "|") {
            stream.eatWhile(regs.validIdentifier);
            return cont("qualifier", "modifier");
          }
        } else if (state.last == "pipe") {
          stream.eatWhile(regs.validIdentifier);
          return cont("qualifier", "modifier");
        } else if (state.last == "whitespace") {
          stream.eatWhile(regs.validIdentifier);
          return cont("attribute", "modifier");
        } if (state.last == "property") {
          stream.eatWhile(regs.validIdentifier);
          return cont("property", null);
        } else if (/\s/.test(ch)) {
          last = "whitespace";
          return null;
        }

        var str = "";
		logger.debug(`tokenSmarty(stream=${stream},state=${state}): str → ${str}`);
        if (ch != "/") {
          str += ch;
        }
        var c = null;
        while (c = stream.eat(regs.validIdentifier)) {
          str += c;
        }
        for (var i=0, j=keyFunctions.length; i<j; i++) {
          if (keyFunctions[i] == str) {
            return cont("keyword", "keyword");
          }
        }
        if (/\s/.test(ch)) {
		logger.debug(`<<< Exiting tokenSmarty(stream=${stream},state=${state})`);
          return null;
        }
        return cont("tag", "tag");
      }
    }

    function tokenAttribute(quote) {
	logger.debug(`>>> Entering tokenAttribute(quote=${quote})`);
      return function(stream, state) {
        var prevChar = null;
        var currChar = null;
        while (!stream.eol()) {
          currChar = stream.peek();
          if (stream.next() == quote && prevChar !== '\\') {
            state.tokenize = tokenSmarty;
			logger.debug(`tokenAttribute(quote=${quote}): state.tokenize → ${state.tokenize}`);
			logger.debug(`function(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
            break;
          }
          prevChar = currChar;
        }
		logger.debug(`<<< Exiting tokenAttribute(quote=${quote})`);
        return "string";
      };
    }

    function tokenBlock(style, terminator) {
	logger.debug(`>>> Entering tokenBlock(style=${style},terminator=${terminator})`);
      return function(stream, state) {
        while (!stream.eol()) {
          if (stream.match(terminator)) {
            state.tokenize = tokenTop;
			logger.debug(`tokenBlock(style=${style},terminator=${terminator}): state.tokenize → ${state.tokenize}`);
            break;
          }
          stream.next();
        }
		logger.debug(`<<< Exiting tokenBlock(style=${style},terminator=${terminator})`);
        return style;
      };
    }

    return {
      startState: function() {
		logger.debug('>>> Entering startState()');
        return {
          base: CodeMirror.startState(baseMode),
          tokenize: tokenTop,
          last: null,
          depth: 0
        };
		logger.debug('<<< Exiting startState()');
      },
      copyState: function(state) {
		logger.debug(`>>> Entering copyState(state=${state})`);
        return {
          base: CodeMirror.copyState(baseMode, state.base),
          tokenize: state.tokenize,
          last: state.last,
          depth: state.depth
        };
		logger.debug(`<<< Exiting copyState(state=${state})`);
      },
      innerMode: function(state) {
		logger.debug(`>>> Entering innerMode(state=${state})`);
        if (state.tokenize == tokenTop)
          return {mode: baseMode, state: state.base};
      },
      token: function(stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
        var style = state.tokenize(stream, state);
        state.last = last;
		logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
        return style;
      },
      indent: function(state, text) {
		logger.debug(`>>> Entering indent(state=${state},text=${text})`);
        if (state.tokenize == tokenTop && baseMode.indent)
          return baseMode.indent(state.base, text);
        else
          return CodeMirror.Pass;
      },
      blockCommentStart: leftDelimiter + "*",
      blockCommentEnd: "*" + rightDelimiter
    };
  });

  CodeMirror.defineMIME("text/x-smarty", "smarty");
});
