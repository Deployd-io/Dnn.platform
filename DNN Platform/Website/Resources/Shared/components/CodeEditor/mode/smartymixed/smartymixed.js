const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/**
* @file smartymixed.js
* @brief Smarty Mixed Codemirror mode (Smarty + Mixed HTML)
* @author Ruslan Osmanov <rrosmanov at gmail dot com>
* @version 3.0
* @date 05.07.2013
*/

// Warning: Don't base other modes on this one. This here is a
// terrible way to write a mixed mode.

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../htmlmixed/htmlmixed"), require("../smarty/smarty"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../htmlmixed/htmlmixed", "../smarty/smarty"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMode("smartymixed", function(config) {
  var htmlMixedMode = CodeMirror.getMode(config, "htmlmixed");
  var smartyMode = CodeMirror.getMode(config, "smarty");

  var settings = {
    rightDelimiter: '}',
    leftDelimiter: '{'
  };

  if (config.hasOwnProperty("leftDelimiter")) {
    settings.leftDelimiter = config.leftDelimiter;
  }
  if (config.hasOwnProperty("rightDelimiter")) {
    settings.rightDelimiter = config.rightDelimiter;
  }

  function reEsc(str) { return str.replace(/[^\s\w]/g, "\\$&"); }

  var reLeft = reEsc(settings.leftDelimiter), reRight = reEsc(settings.rightDelimiter);
  var regs = {
    smartyComment: new RegExp("^" + reRight + "\\*"),
    literalOpen: new RegExp(reLeft + "literal" + reRight),
    literalClose: new RegExp(reLeft + "\/literal" + reRight),
    hasLeftDelimeter: new RegExp(".*" + reLeft),
    htmlHasLeftDelimeter: new RegExp("[^<>]*" + reLeft)
  };

  var helpers = {
    chain: function(stream, state, parser) {
      state.tokenize = parser;
	logger.warn(`<<< Exiting chain(stream=${stream},state=${state},parser=${parser})`, { "event.action": "chain", "logrite.stream": stream, "logrite.state": state, "logrite.parser": parser });
      return parser(stream, state);
    },

    cleanChain: function(stream, state, parser) {
	logger.error(`>>> Entering cleanChain(stream=${stream},state=${state},parser=${parser})`, { "event.action": "cleanChain", "logrite.stream": stream, "logrite.state": state, "logrite.parser": parser });
      state.tokenize = null;
      state.localState = null;
	logger.debug(`cleanChain(stream=${stream},state=${state},parser=${parser}): state.tokenize → ${state.tokenize}`, { "event.action": "cleanChain", "logrite.stream": stream, "logrite.state": state, "logrite.parser": parser });
      state.localMode = null;
	logger.warn(`<<< Exiting cleanChain(stream=${stream},state=${state},parser=${parser})`, { "event.action": "cleanChain", "logrite.stream": stream, "logrite.state": state, "logrite.parser": parser });
      return (typeof parser == "string") ? (parser ? parser : null) : parser(stream, state);
    },

    maybeBackup: function(stream, pat, style) {
      var cur = stream.current();
      var close = cur.search(pat),
      m;
      if (close > - 1) stream.backUp(cur.length - close);
      else if (m = cur.match(/<\/?$/)) {
        stream.backUp(cur.length);
        if (!stream.match(pat, false)) stream.match(cur[0]);
      }
	logger.warn(`<<< Exiting maybeBackup(stream=${stream},pat=${pat},style=${style})`, { "event.action": "maybeBackup", "logrite.stream": stream, "logrite.pat": pat, "logrite.style": style });
      return style;
    }
  };

  var parsers = {
    html: function(stream, state) {
      var htmlTagName = state.htmlMixedState.htmlState.context && state.htmlMixedState.htmlState.context.tagName
        ? state.htmlMixedState.htmlState.context.tagName
        : null;

      if (!state.inLiteral && stream.match(regs.htmlHasLeftDelimeter, false) && htmlTagName === null) {
        state.tokenize = parsers.smarty;
        state.localMode = smartyMode;
        state.localState = smartyMode.startState(htmlMixedMode.indent(state.htmlMixedState, ""));
        return helpers.maybeBackup(stream, settings.leftDelimiter, smartyMode.token(stream, state.localState));
      } else if (!state.inLiteral && stream.match(settings.leftDelimiter, false)) {
        state.tokenize = parsers.smarty;
        state.localMode = smartyMode;
        state.localState = smartyMode.startState(htmlMixedMode.indent(state.htmlMixedState, ""));
        return helpers.maybeBackup(stream, settings.leftDelimiter, smartyMode.token(stream, state.localState));
      }
	logger.warn(`<<< Exiting html(stream=${stream},state=${state})`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
      return htmlMixedMode.token(stream, state.htmlMixedState);
    },

    smarty: function(stream, state) {
      if (stream.match(settings.leftDelimiter, false)) {
        if (stream.match(regs.smartyComment, false)) {
          return helpers.chain(stream, state, parsers.inBlock("comment", "*" + settings.rightDelimiter));
        }
      } else if (stream.match(settings.rightDelimiter, false)) {
        stream.eat(settings.rightDelimiter);
        state.tokenize = parsers.html;
        state.localMode = htmlMixedMode;
        state.localState = state.htmlMixedState;
        return "tag";
      }

	logger.warn(`<<< Exiting smarty(stream=${stream},state=${state})`, { "event.action": "smarty", "logrite.stream": stream, "logrite.state": state });
      return helpers.maybeBackup(stream, settings.rightDelimiter, smartyMode.token(stream, state.localState));
    },

    inBlock: function(style, terminator) {
      return function(stream, state) {
        while (!stream.eol()) {
          if (stream.match(terminator)) {
            helpers.cleanChain(stream, state, "");
            break;
          }
          stream.next();
        }
	logger.warn(`<<< Exiting inBlock(style=${style},terminator=${terminator})`, { "event.action": "inBlock", "logrite.style": style, "logrite.terminator": terminator });
        return style;
      };
    }
  };

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      var state = htmlMixedMode.startState();
      return {
        token: parsers.html,
        localMode: null,
        localState: null,
        htmlMixedState: state,
        tokenize: null,
        inLiteral: false
      };
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },

    copyState: function(state) {
	logger.error(`>>> Entering copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
      var local = null, tok = (state.tokenize || state.token);
      if (state.localState) {
        local = CodeMirror.copyState((tok != parsers.html ? smartyMode : htmlMixedMode), state.localState);
		logger.error(`copyState(state=${state}): local → ${local}`, { "event.action": "copyState", "logrite.state": state });
		logger.warn(`<<< Exiting copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
      }
      return {
        token: state.token,
        tokenize: state.tokenize,
        localMode: state.localMode,
        localState: local,
        htmlMixedState: CodeMirror.copyState(htmlMixedMode, state.htmlMixedState),
        inLiteral: state.inLiteral
      };
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(settings.leftDelimiter, false)) {
        if (!state.inLiteral && stream.match(regs.literalOpen, true)) {
          state.inLiteral = true;
		logger.debug(`token(stream=${stream},state=${state}): state.inLiteral → ${state.inLiteral}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          return "keyword";
        } else if (state.inLiteral && stream.match(regs.literalClose, true)) {
          state.inLiteral = false;
          return "keyword";
        }
      }
      if (state.inLiteral && state.localState != state.htmlMixedState) {
        state.tokenize = parsers.html;
		logger.debug(`token(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
		logger.debug(`token(stream=${stream},state=${state}): state.localState → ${state.localState}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        state.localMode = htmlMixedMode;
		logger.debug(`token(stream=${stream},state=${state}): state.localMode → ${state.localMode}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        state.localState = state.htmlMixedState;
      }

      var style = (state.tokenize || state.token)(stream, state);
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return style;
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      if (state.localMode == smartyMode
          || (state.inLiteral && !state.localMode)
         || regs.hasLeftDelimeter.test(textAfter)) {
        return CodeMirror.Pass;
      }
	logger.warn(`<<< Exiting indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      return htmlMixedMode.indent(state.htmlMixedState, textAfter);
    },

    innerMode: function(state) {
	logger.error(`>>> Entering innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
      return {
        state: state.localState || state.htmlMixedState,
        mode: state.localMode || htmlMixedMode
      };
	logger.warn(`<<< Exiting innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
    }
  };
}, "htmlmixed", "smarty");

CodeMirror.defineMIME("text/x-smarty", "smartymixed");
// vim: et ts=2 sts=2 sw=2

});
