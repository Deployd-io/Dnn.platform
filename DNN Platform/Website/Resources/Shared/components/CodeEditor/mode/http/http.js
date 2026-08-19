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
CodeMirror.defineMode("http", function() {
  function failFirstLine(stream, state) {
	logger.debug(`>>> Entering failFirstLine(stream=${stream},state=${state})`);
    stream.skipToEnd();
    state.cur = header;
	logger.debug(`failFirstLine(stream=${stream},state=${state}): state.cur → ${state.cur}`);
	logger.debug(`<<< Exiting failFirstLine(stream=${stream},state=${state})`);
    return "error";
  }

  function start(stream, state) {
	logger.debug(`>>> Entering start(stream=${stream},state=${state})`);
    if (stream.match(/^HTTP\/\d\.\d/)) {
      state.cur = responseStatusCode;
	logger.debug(`start(stream=${stream},state=${state}): state.cur → ${state.cur}`);
      return "keyword";
    } else if (stream.match(/^[A-Z]+/) && /[ \t]/.test(stream.peek())) {
      state.cur = requestPath;
      return "keyword";
    } else {
	logger.debug(`<<< Exiting start(stream=${stream},state=${state})`);
      return failFirstLine(stream, state);
    }
  }

  function responseStatusCode(stream, state) {
	logger.debug(`>>> Entering responseStatusCode(stream=${stream},state=${state})`);
    var code = stream.match(/^\d+/);
    if (!code) return failFirstLine(stream, state);
	logger.debug(`responseStatusCode(stream=${stream},state=${state}): code → ${code}`);

    state.cur = responseStatusText;
	logger.debug(`responseStatusCode(stream=${stream},state=${state}): state.cur → ${state.cur}`);
    var status = Number(code[0]);
	logger.debug(`responseStatusCode(stream=${stream},state=${state}): status → ${status}`);
    if (status >= 100 && status < 200) {
      return "positive informational";
    } else if (status >= 200 && status < 300) {
      return "positive success";
    } else if (status >= 300 && status < 400) {
      return "positive redirect";
    } else if (status >= 400 && status < 500) {
      return "negative client-error";
    } else if (status >= 500 && status < 600) {
      return "negative server-error";
    } else {
	logger.debug(`<<< Exiting responseStatusCode(stream=${stream},state=${state})`);
      return "error";
    }
  }

  function responseStatusText(stream, state) {
	logger.debug(`>>> Entering responseStatusText(stream=${stream},state=${state})`);
    stream.skipToEnd();
    state.cur = header;
	logger.debug(`responseStatusText(stream=${stream},state=${state}): state.cur → ${state.cur}`);
	logger.debug(`<<< Exiting responseStatusText(stream=${stream},state=${state})`);
    return null;
  }

  function requestPath(stream, state) {
	logger.debug(`>>> Entering requestPath(stream=${stream},state=${state})`);
    stream.eatWhile(/\S/);
    state.cur = requestProtocol;
	logger.debug(`requestPath(stream=${stream},state=${state}): state.cur → ${state.cur}`);
	logger.debug(`<<< Exiting requestPath(stream=${stream},state=${state})`);
    return "string-2";
  }

  function requestProtocol(stream, state) {
	logger.debug(`>>> Entering requestProtocol(stream=${stream},state=${state})`);
    if (stream.match(/^HTTP\/\d\.\d$/)) {
      state.cur = header;
	logger.debug(`requestProtocol(stream=${stream},state=${state}): state.cur → ${state.cur}`);
      return "keyword";
    } else {
	logger.debug(`<<< Exiting requestProtocol(stream=${stream},state=${state})`);
      return failFirstLine(stream, state);
    }
  }

  function header(stream) {
	logger.debug(`>>> Entering header(stream=${stream})`);
    if (stream.sol() && !stream.eat(/[ \t]/)) {
      if (stream.match(/^.*?:/)) {
        return "atom";
      } else {
        stream.skipToEnd();
        return "error";
      }
    } else {
      stream.skipToEnd();
	logger.debug(`<<< Exiting header(stream=${stream})`);
      return "string";
    }
  }

  function body(stream) {
	logger.debug(`>>> Entering body(stream=${stream})`);
    stream.skipToEnd();
	logger.debug(`<<< Exiting body(stream=${stream})`);
    return null;
  }

  return {
    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      var cur = state.cur;
      if (cur != header && cur != body && stream.eatSpace()) return null;
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      return cur(stream, state);
    },

    blankLine: function(state) {
	logger.debug(`>>> Entering blankLine(state=${state})`);
      state.cur = body;
	logger.debug(`blankLine(state=${state}): state.cur → ${state.cur}`);
	logger.debug(`<<< Exiting blankLine(state=${state})`);
    },

    startState: function() {
	logger.debug('>>> Entering startState()');
	logger.debug('<<< Exiting startState()');
      return {cur: start};
    }
  };
});

CodeMirror.defineMIME("message/http", "http");

});
