const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../htmlmixed/htmlmixed"), require("../ruby/ruby"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../htmlmixed/htmlmixed", "../ruby/ruby"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

logger.debug(`<<< Exiting function(exports=${exports})`);
  // full haml mode. This handled embeded ruby and html fragments too
  CodeMirror.defineMode("haml", function(config) {
	logger.debug('>>> Entering defineMode()');
    var htmlMode = CodeMirror.getMode(config, {name: "htmlmixed"});
    var rubyMode = CodeMirror.getMode(config, "ruby");
	logger.debug(`defineMode(): htmlMode → ${htmlMode}`);
	logger.debug(`defineMode(): rubyMode → ${rubyMode}`);

	logger.debug('<<< Exiting defineMode()');
    function rubyInQuote(endQuote) {
      return function(stream, state) {
	logger.debug(`>>> Entering rubyInQuote(endQuote=${endQuote})`);
        var ch = stream.peek();
        if (ch == endQuote && state.rubyState.tokenize.length == 1) {
          // step out of ruby context as it seems to complete processing all the braces
          stream.next();
          state.tokenize = html;
		logger.debug(`rubyInQuote(endQuote=${endQuote}): state.tokenize → ${state.tokenize}`);
          return "closeAttributeTag";
        } else {
		logger.debug(`<<< Exiting rubyInQuote(endQuote=${endQuote})`);
          return ruby(stream, state);
        }
      };
    }

    function ruby(stream, state) {
	logger.debug(`>>> Entering ruby(stream=${stream},state=${state})`);
      if (stream.match("-#")) {
        stream.skipToEnd();
	logger.debug(`<<< Exiting ruby(stream=${stream},state=${state})`);
        return "comment";
      }
      return rubyMode.token(stream, state.rubyState);
    }

    function html(stream, state) {
	logger.debug(`>>> Entering html(stream=${stream},state=${state})`);
      var ch = stream.peek();

	logger.debug(`html(stream=${stream},state=${state}): ch → ${ch}`);
      // handle haml declarations. All declarations that cant be handled here
      // will be passed to html mode
      if (state.previousToken.style == "comment" ) {
        if (state.indented > state.previousToken.indented) {
          stream.skipToEnd();
          return "commentLine";
        }
      }

      if (state.startOfLine) {
        if (ch == "!" && stream.match("!!")) {
          stream.skipToEnd();
          return "tag";
        } else if (stream.match(/^%[\w:#\.]+=/)) {
          state.tokenize = ruby;
          return "hamlTag";
        } else if (stream.match(/^%[\w:]+/)) {
		logger.debug(`html(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
          return "hamlTag";
        } else if (ch == "/" ) {
          stream.skipToEnd();
          return "comment";
        }
      }

      if (state.startOfLine || state.previousToken.style == "hamlTag") {
        if ( ch == "#" || ch == ".") {
          stream.match(/[\w-#\.]*/);
          return "hamlAttribute";
        }
      }

      // donot handle --> as valid ruby, make it HTML close comment instead
      if (state.startOfLine && !stream.match("-->", false) && (ch == "=" || ch == "-" )) {
        state.tokenize = ruby;
        return state.tokenize(stream, state);
      }

      if (state.previousToken.style == "hamlTag" ||
          state.previousToken.style == "closeAttributeTag" ||
          state.previousToken.style == "hamlAttribute") {
        if (ch == "(") {
          state.tokenize = rubyInQuote(")");
          return state.tokenize(stream, state);
        } else if (ch == "{") {
          state.tokenize = rubyInQuote("}");
          return state.tokenize(stream, state);
        }
      }

	logger.debug(`<<< Exiting html(stream=${stream},state=${state})`);
      return htmlMode.token(stream, state.htmlState);
    }

    return {
      // default to html mode
      startState: function() {
		logger.debug('>>> Entering startState()');
        var htmlState = htmlMode.startState();
        var rubyState = rubyMode.startState();
        return {
          htmlState: htmlState,
          rubyState: rubyState,
          indented: 0,
          previousToken: { style: null, indented: 0},
          tokenize: html
        };
		logger.debug('<<< Exiting startState()');
      },

      copyState: function(state) {
		logger.debug(`>>> Entering copyState(state=${state})`);
        return {
          htmlState : CodeMirror.copyState(htmlMode, state.htmlState),
          rubyState: CodeMirror.copyState(rubyMode, state.rubyState),
          indented: state.indented,
          previousToken: state.previousToken,
          tokenize: state.tokenize
        };
		logger.debug(`<<< Exiting copyState(state=${state})`);
      },

      token: function(stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
        if (stream.sol()) {
          state.indented = stream.indentation();
          state.startOfLine = true;
		logger.debug(`token(stream=${stream},state=${state}): state.startOfLine → ${state.startOfLine}`);
        }
        if (stream.eatSpace()) return null;
        var style = state.tokenize(stream, state);
        state.startOfLine = false;
        // dont record comment line as we only want to measure comment line with
        // the opening comment block
        if (style && style != "commentLine") {
          state.previousToken = { style: style, indented: state.indented };
		logger.debug(`token(stream=${stream},state=${state}): state.previousToken → ${state.previousToken}`);
        }
        // if current state is ruby and the previous token is not `,` reset the
        // tokenize to html
        if (stream.eol() && state.tokenize == ruby) {
          stream.backUp(1);
          var ch = stream.peek();
		logger.debug(`token(stream=${stream},state=${state}): ch → ${ch}`);
          stream.next();
          if (ch && ch != ",") {
            state.tokenize = html;
			logger.debug(`token(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
          }
        }
        // reprocess some of the specific style tag when finish setting previousToken
        if (style == "hamlTag") {
          style = "tag";
        } else if (style == "commentLine") {
          style = "comment";
        } else if (style == "hamlAttribute") {
          style = "attribute";
        } else if (style == "closeAttributeTag") {
          style = null;
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
        }
        return style;
      }
    };
  }, "htmlmixed", "ruby");

  CodeMirror.defineMIME("text/x-haml", "haml");
});
