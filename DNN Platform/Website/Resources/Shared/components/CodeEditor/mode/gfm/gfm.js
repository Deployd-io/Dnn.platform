const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../markdown/markdown"), require("../../addon/mode/overlay"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../markdown/markdown", "../../addon/mode/overlay"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

logger.debug(`<<< Exiting function(exports=${exports})`);
CodeMirror.defineMode("gfm", function(config, modeConfig) {
logger.debug('>>> Entering defineMode()');
  var codeDepth = 0;
logger.debug('<<< Exiting defineMode()');
  function blankLine(state) {
	logger.debug(`>>> Entering blankLine(state=${state})`);
    state.code = false;
	logger.debug(`<<< Exiting blankLine(state=${state})`);
    return null;
  }
  var gfmOverlay = {
    startState: function() {
	logger.debug('>>> Entering startState()');
      return {
        code: false,
        codeBlock: false,
        ateSpace: false
      };
	logger.debug('<<< Exiting startState()');
    },
    copyState: function(s) {
	logger.debug(`>>> Entering copyState(s=${s})`);
      return {
        code: s.code,
        codeBlock: s.codeBlock,
        ateSpace: s.ateSpace
      };
	logger.debug(`<<< Exiting copyState(s=${s})`);
    },
    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      state.combineTokens = null;

      // Hack to prevent formatting override inside code blocks (block and inline)
      if (state.codeBlock) {
        if (stream.match(/^```/)) {
          state.codeBlock = false;
          return null;
        }
        stream.skipToEnd();
        return null;
      }
      if (stream.sol()) {
        state.code = false;
      }
      if (stream.sol() && stream.match(/^```/)) {
        stream.skipToEnd();
        state.codeBlock = true;
        return null;
      }
      // If this block is changed, it may need to be updated in Markdown mode
      if (stream.peek() === '`') {
        stream.next();
        var before = stream.pos;
        stream.eatWhile('`');
		logger.debug(`token(stream=${stream},state=${state}): state.code → ${state.code}`);
		logger.debug(`token(stream=${stream},state=${state}): state.combineTokens → ${state.combineTokens}`);
		logger.debug(`token(stream=${stream},state=${state}): state.codeBlock → ${state.codeBlock}`);
        var difference = 1 + stream.pos - before;
        if (!state.code) {
          codeDepth = difference;
          state.code = true;
        } else {
          if (difference === codeDepth) { // Must be exact
            state.code = false;
          }
        }
        return null;
      } else if (state.code) {
        stream.next();
        return null;
      }
      // Check if space. If so, links can be formatted later on
      if (stream.eatSpace()) {
        state.ateSpace = true;
		logger.debug(`token(stream=${stream},state=${state}): state.ateSpace → ${state.ateSpace}`);
        return null;
      }
      if (stream.sol() || state.ateSpace) {
        state.ateSpace = false;
        if(stream.match(/^(?:[a-zA-Z0-9\-_]+\/)?(?:[a-zA-Z0-9\-_]+@)?(?:[a-f0-9]{7,40}\b)/)) {
          // User/Project@SHA
          // User@SHA
          // SHA
          state.combineTokens = true;
          return "link";
        } else if (stream.match(/^(?:[a-zA-Z0-9\-_]+\/)?(?:[a-zA-Z0-9\-_]+)?#[0-9]+\b/)) {
          // User/Project#Num
          // User#Num
          // #Num
          state.combineTokens = true;
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
          return "link";
        }
      }
      if (stream.match(/^((?:[a-z][\w-]+:(?:\/{1,3}|[a-z0-9%])|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}\/)(?:[^\s()<>]|\([^\s()<>]*\))+(?:\([^\s()<>]*\)|[^\s`*!()\[\]{};:'".,<>?«»“”‘’]))/i) &&
         stream.string.slice(stream.start - 2, stream.start) != "](") {
        // URLs
        // Taken from http://daringfireball.net/2010/07/improved_regex_for_matching_urls
        // And then (issue #1160) simplified to make it not crash the Chrome Regexp engine
        state.combineTokens = true;
        return "link";
      }
      stream.next();
      return null;
    },
    blankLine: blankLine
  };

  var markdownConfig = {
    underscoresBreakWords: false,
    taskLists: true,
    fencedCodeBlocks: true,
    strikethrough: true
  };
  for (var attr in modeConfig) {
    markdownConfig[attr] = modeConfig[attr];
  }
  markdownConfig.name = "markdown";
  return CodeMirror.overlayMode(CodeMirror.getMode(config, markdownConfig), gfmOverlay);

}, "markdown");

  CodeMirror.defineMIME("text/x-gfm", "gfm");
});
