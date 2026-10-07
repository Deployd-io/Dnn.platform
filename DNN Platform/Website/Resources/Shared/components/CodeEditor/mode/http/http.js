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

CodeMirror.defineMode("http", function() {
  function failFirstLine(stream, state) {
	logger.error(`>>> Entering failFirstLine(stream=${stream},state=${state})`, { "event.action": "failFirstLine", "logrite.stream": stream, "logrite.state": state });
    stream.skipToEnd();
    state.cur = header;
	logger.debug(`failFirstLine(stream=${stream},state=${state}): state.cur → ${state.cur}`, { "event.action": "failFirstLine", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting failFirstLine(stream=${stream},state=${state})`, { "event.action": "failFirstLine", "logrite.stream": stream, "logrite.state": state });
    return "error";
  }

  function start(stream, state) {
	logger.error(`>>> Entering start(stream=${stream},state=${state})`, { "event.action": "start", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^HTTP\/\d\.\d/)) {
      state.cur = responseStatusCode;
	logger.debug(`start(stream=${stream},state=${state}): state.cur → ${state.cur}`, { "event.action": "start", "logrite.stream": stream, "logrite.state": state });
      return "keyword";
    } else if (stream.match(/^[A-Z]+/) && /[ \t]/.test(stream.peek())) {
      state.cur = requestPath;
      return "keyword";
    } else {
	logger.warn(`<<< Exiting start(stream=${stream},state=${state})`, { "event.action": "start", "logrite.stream": stream, "logrite.state": state });
      return failFirstLine(stream, state);
    }
  }

  function responseStatusCode(stream, state) {
	logger.error(`>>> Entering responseStatusCode(stream=${stream},state=${state})`, { "event.action": "responseStatusCode", "logrite.stream": stream, "logrite.state": state });
    var code = stream.match(/^\d+/);
    if (!code) return failFirstLine(stream, state);
	logger.error(`responseStatusCode(stream=${stream},state=${state}): code → ${code}`, { "event.action": "responseStatusCode", "logrite.stream": stream, "logrite.state": state });

    state.cur = responseStatusText;
	logger.debug(`responseStatusCode(stream=${stream},state=${state}): state.cur → ${state.cur}`, { "event.action": "responseStatusCode", "logrite.stream": stream, "logrite.state": state });
    var status = Number(code[0]);
	logger.error(`responseStatusCode(stream=${stream},state=${state}): status → ${status}`, { "event.action": "responseStatusCode", "logrite.stream": stream, "logrite.state": state });
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
	logger.warn(`<<< Exiting responseStatusCode(stream=${stream},state=${state})`, { "event.action": "responseStatusCode", "logrite.stream": stream, "logrite.state": state });
      return "error";
    }
  }

  function responseStatusText(stream, state) {
	logger.error(`>>> Entering responseStatusText(stream=${stream},state=${state})`, { "event.action": "responseStatusText", "logrite.stream": stream, "logrite.state": state });
    stream.skipToEnd();
    state.cur = header;
	logger.debug(`responseStatusText(stream=${stream},state=${state}): state.cur → ${state.cur}`, { "event.action": "responseStatusText", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting responseStatusText(stream=${stream},state=${state})`, { "event.action": "responseStatusText", "logrite.stream": stream, "logrite.state": state });
    return null;
  }

  function requestPath(stream, state) {
	logger.error(`>>> Entering requestPath(stream=${stream},state=${state})`, { "event.action": "requestPath", "logrite.stream": stream, "logrite.state": state });
    stream.eatWhile(/\S/);
    state.cur = requestProtocol;
	logger.debug(`requestPath(stream=${stream},state=${state}): state.cur → ${state.cur}`, { "event.action": "requestPath", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting requestPath(stream=${stream},state=${state})`, { "event.action": "requestPath", "logrite.stream": stream, "logrite.state": state });
    return "string-2";
  }

  function requestProtocol(stream, state) {
	logger.error(`>>> Entering requestProtocol(stream=${stream},state=${state})`, { "event.action": "requestProtocol", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^HTTP\/\d\.\d$/)) {
      state.cur = header;
	logger.debug(`requestProtocol(stream=${stream},state=${state}): state.cur → ${state.cur}`, { "event.action": "requestProtocol", "logrite.stream": stream, "logrite.state": state });
      return "keyword";
    } else {
	logger.warn(`<<< Exiting requestProtocol(stream=${stream},state=${state})`, { "event.action": "requestProtocol", "logrite.stream": stream, "logrite.state": state });
      return failFirstLine(stream, state);
    }
  }

  function header(stream) {
	logger.error(`>>> Entering header(stream=${stream})`, { "event.action": "header", "logrite.stream": stream });
    if (stream.sol() && !stream.eat(/[ \t]/)) {
      if (stream.match(/^.*?:/)) {
        return "atom";
      } else {
        stream.skipToEnd();
        return "error";
      }
    } else {
      stream.skipToEnd();
	logger.warn(`<<< Exiting header(stream=${stream})`, { "event.action": "header", "logrite.stream": stream });
      return "string";
    }
  }

  function body(stream) {
	logger.error(`>>> Entering body(stream=${stream})`, { "event.action": "body", "logrite.stream": stream });
    stream.skipToEnd();
	logger.warn(`<<< Exiting body(stream=${stream})`, { "event.action": "body", "logrite.stream": stream });
    return null;
  }

  return {
    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      var cur = state.cur;
      if (cur != header && cur != body && stream.eatSpace()) return null;
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return cur(stream, state);
    },

    blankLine: function(state) {
	logger.error(`>>> Entering blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
      state.cur = body;
	logger.warn(`<<< Exiting blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
    },

    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      return {cur: start};
    }
  };
});

CodeMirror.defineMIME("message/http", "http");

});
