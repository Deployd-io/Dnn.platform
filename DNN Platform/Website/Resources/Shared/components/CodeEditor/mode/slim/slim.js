const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Slim Highlighting for CodeMirror copyright (c) HicknHack Software Gmbh

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../htmlmixed/htmlmixed"), require("../ruby/ruby"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../htmlmixed/htmlmixed", "../ruby/ruby"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

  CodeMirror.defineMode("slim", function(config) {
    var htmlMode = CodeMirror.getMode(config, {name: "htmlmixed"});
    var rubyMode = CodeMirror.getMode(config, "ruby");
    var modes = { html: htmlMode, ruby: rubyMode };
    var embedded = {
      ruby: "ruby",
      javascript: "javascript",
      css: "text/css",
      sass: "text/x-sass",
      scss: "text/x-scss",
      less: "text/x-less",
      styl: "text/x-styl", // no highlighting so far
      coffee: "coffeescript",
      asciidoc: "text/x-asciidoc",
      markdown: "text/x-markdown",
      textile: "text/x-textile", // no highlighting so far
      creole: "text/x-creole", // no highlighting so far
      wiki: "text/x-wiki", // no highlighting so far
      mediawiki: "text/x-mediawiki", // no highlighting so far
      rdoc: "text/x-rdoc", // no highlighting so far
      builder: "text/x-builder", // no highlighting so far
      nokogiri: "text/x-nokogiri", // no highlighting so far
      erb: "application/x-erb"
    };
    var embeddedRegexp = function(map){
      var arr = [];
      for(var key in map) arr.push(key);
      return new RegExp("^("+arr.join('|')+"):");
    }(embedded);

    var styleMap = {
      "commentLine": "comment",
      "slimSwitch": "operator special",
      "slimTag": "tag",
      "slimId": "attribute def",
      "slimClass": "attribute qualifier",
      "slimAttribute": "attribute",
      "slimSubmode": "keyword special",
      "closeAttributeTag": null,
      "slimDoctype": null,
      "lineContinuation": null
    };
    var closing = {
      "{": "}",
      "[": "]",
      "(": ")"
    };

    var nameStartChar = "_a-zA-Z\xC0-\xD6\xD8-\xF6\xF8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD";
    var nameChar = nameStartChar + "\\-0-9\xB7\u0300-\u036F\u203F-\u2040";
    var nameRegexp = new RegExp("^[:"+nameStartChar+"](?::["+nameChar+"]|["+nameChar+"]*)");
    var attributeNameRegexp = new RegExp("^[:"+nameStartChar+"][:\\."+nameChar+"]*(?=\\s*=)");
    var wrappedAttributeNameRegexp = new RegExp("^[:"+nameStartChar+"][:\\."+nameChar+"]*");
    var classNameRegexp = /^\.-?[_a-zA-Z]+[\w\-]*/;
    var classIdRegexp = /^#[_a-zA-Z]+[\w\-]*/;

    function backup(pos, tokenize, style) {
      var restore = function(stream, state) {
		logger.error(`>>> Entering restore(stream=${stream},state=${state})`, { "event.action": "restore", "logrite.stream": stream, "logrite.state": state });
        state.tokenize = tokenize;
        if (stream.pos < pos) {
		logger.debug(`restore(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "restore", "logrite.stream": stream, "logrite.state": state });
          stream.pos = pos;
          return style;
        }
		logger.warn(`<<< Exiting restore(stream=${stream},state=${state})`, { "event.action": "restore", "logrite.stream": stream, "logrite.state": state });
        return state.tokenize(stream, state);
      };
      return function(stream, state) {
        state.tokenize = restore;
        return tokenize(stream, state);
      };
    }

    function maybeBackup(stream, state, pat, offset, style) {
	logger.error(`>>> Entering maybeBackup(stream=${stream},state=${state},pat=${pat},offset=${offset},style=${style})`, { "event.action": "maybeBackup", "logrite.stream": stream, "logrite.state": state, "logrite.pat": pat, "logrite.offset": offset, "logrite.style": style });
      var cur = stream.current();
      var idx = cur.search(pat);
	logger.error(`maybeBackup(stream=${stream},state=${state},pat=${pat},offset=${offset},style=${style}): cur → ${cur}`, { "event.action": "maybeBackup", "logrite.stream": stream, "logrite.state": state, "logrite.pat": pat, "logrite.offset": offset, "logrite.style": style });
      if (idx > -1) {
        state.tokenize = backup(stream.pos, state.tokenize, style);
        stream.backUp(cur.length - idx - offset);
      }
	logger.error(`maybeBackup(stream=${stream},state=${state},pat=${pat},offset=${offset},style=${style}): idx → ${idx}`, { "event.action": "maybeBackup", "logrite.stream": stream, "logrite.state": state, "logrite.pat": pat, "logrite.offset": offset, "logrite.style": style });
      return style;
    }

    function continueLine(state, column) {
	logger.error(`>>> Entering continueLine(state=${state},column=${column})`, { "event.action": "continueLine", "logrite.state": state, "logrite.column": column });
      state.stack = {
        parent: state.stack,
        style: "continuation",
        indented: column,
        tokenize: state.line
      };
      state.line = state.tokenize;
	logger.warn(`<<< Exiting continueLine(state=${state},column=${column})`, { "event.action": "continueLine", "logrite.state": state, "logrite.column": column });
    }
    function finishContinue(state) {
	logger.error(`>>> Entering finishContinue(state=${state})`, { "event.action": "finishContinue", "logrite.state": state });
      if (state.line == state.tokenize) {
        state.line = state.stack.tokenize;
		logger.debug(`finishContinue(state=${state}): state.line → ${state.line}`, { "event.action": "finishContinue", "logrite.state": state });
        state.stack = state.stack.parent;
		logger.warn(`<<< Exiting finishContinue(state=${state})`, { "event.action": "finishContinue", "logrite.state": state });
      }
    }

    function lineContinuable(column, tokenize) {
      return function(stream, state) {
        finishContinue(state);
        if (stream.match(/^\\$/)) {
          continueLine(state, column);
          return "lineContinuation";
        }
        var style = tokenize(stream, state);
        if (stream.eol() && stream.current().match(/(?:^|[^\\])(?:\\\\)*\\$/)) {
          stream.backUp(1);
        }
        return style;
      };
    }
    function commaContinuable(column, tokenize) {
      return function(stream, state) {
        finishContinue(state);
        var style = tokenize(stream, state);
        if (stream.eol() && stream.current().match(/,$/)) {
          continueLine(state, column);
        }
        return style;
      };
    }

    function rubyInQuote(endQuote, tokenize) {
      // TODO: add multi line support
      return function(stream, state) {
        var ch = stream.peek();
        if (ch == endQuote && state.rubyState.tokenize.length == 1) {
          // step out of ruby context as it seems to complete processing all the braces
          stream.next();
          state.tokenize = tokenize;
          return "closeAttributeTag";
        } else {
          return ruby(stream, state);
        }
      };
    }
    function startRubySplat(tokenize) {
      var rubyState;
      var runSplat = function(stream, state) {
		logger.error(`>>> Entering runSplat(stream=${stream},state=${state})`, { "event.action": "runSplat", "logrite.stream": stream, "logrite.state": state });
        if (state.rubyState.tokenize.length == 1 && !state.rubyState.context.prev) {
          stream.backUp(1);
          if (stream.eatSpace()) {
            state.rubyState = rubyState;
            state.tokenize = tokenize;
			logger.debug(`runSplat(stream=${stream},state=${state}): state.rubyState → ${state.rubyState}`, { "event.action": "runSplat", "logrite.stream": stream, "logrite.state": state });
            return tokenize(stream, state);
          }
          stream.next();
		logger.debug(`runSplat(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "runSplat", "logrite.stream": stream, "logrite.state": state });
        }
		logger.warn(`<<< Exiting runSplat(stream=${stream},state=${state})`, { "event.action": "runSplat", "logrite.stream": stream, "logrite.state": state });
        return ruby(stream, state);
      };
      return function(stream, state) {
        rubyState = state.rubyState;
        state.rubyState = rubyMode.startState();
        state.tokenize = runSplat;
        return ruby(stream, state);
      };
    }

    function ruby(stream, state) {
	logger.error(`>>> Entering ruby(stream=${stream},state=${state})`, { "event.action": "ruby", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting ruby(stream=${stream},state=${state})`, { "event.action": "ruby", "logrite.stream": stream, "logrite.state": state });
      return rubyMode.token(stream, state.rubyState);
    }

    function htmlLine(stream, state) {
	logger.error(`>>> Entering htmlLine(stream=${stream},state=${state})`, { "event.action": "htmlLine", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(/^\\$/)) {
        return "lineContinuation";
      }
	logger.warn(`<<< Exiting htmlLine(stream=${stream},state=${state})`, { "event.action": "htmlLine", "logrite.stream": stream, "logrite.state": state });
      return html(stream, state);
    }
    function html(stream, state) {
	logger.error(`>>> Entering html(stream=${stream},state=${state})`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(/^#\{/)) {
        state.tokenize = rubyInQuote("}", state.tokenize);
        return null;
      }
	logger.warn(`<<< Exiting html(stream=${stream},state=${state})`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
      return maybeBackup(stream, state, /[^\\]#\{/, 1, htmlMode.token(stream, state.htmlState));
    }

    function startHtmlLine(lastTokenize) {
      return function(stream, state) {
        var style = htmlLine(stream, state);
        if (stream.eol()) state.tokenize = lastTokenize;
        return style;
      };
    }

    function startHtmlMode(stream, state, offset) {
	logger.error(`>>> Entering startHtmlMode(stream=${stream},state=${state},offset=${offset})`, { "event.action": "startHtmlMode", "logrite.stream": stream, "logrite.state": state, "logrite.offset": offset });
      state.stack = {
        parent: state.stack,
        style: "html",
        indented: stream.column() + offset, // pipe + space
        tokenize: state.line
      };
      state.line = state.tokenize = html;
	logger.debug(`startHtmlMode(stream=${stream},state=${state},offset=${offset}): state.stack → ${state.stack}`, { "event.action": "startHtmlMode", "logrite.stream": stream, "logrite.state": state, "logrite.offset": offset });
	logger.debug(`startHtmlMode(stream=${stream},state=${state},offset=${offset}): state.line → ${state.line}`, { "event.action": "startHtmlMode", "logrite.stream": stream, "logrite.state": state, "logrite.offset": offset });
      return null;
    }

    function comment(stream, state) {
	logger.error(`>>> Entering comment(stream=${stream},state=${state})`, { "event.action": "comment", "logrite.stream": stream, "logrite.state": state });
      stream.skipToEnd();
	logger.warn(`<<< Exiting comment(stream=${stream},state=${state})`, { "event.action": "comment", "logrite.stream": stream, "logrite.state": state });
      return state.stack.style;
    }

    function commentMode(stream, state) {
	logger.error(`>>> Entering commentMode(stream=${stream},state=${state})`, { "event.action": "commentMode", "logrite.stream": stream, "logrite.state": state });
      state.stack = {
        parent: state.stack,
        style: "comment",
        indented: state.indented + 1,
        tokenize: state.line
      };
      state.line = comment;
	logger.debug(`commentMode(stream=${stream},state=${state}): state.stack → ${state.stack}`, { "event.action": "commentMode", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`commentMode(stream=${stream},state=${state}): state.line → ${state.line}`, { "event.action": "commentMode", "logrite.stream": stream, "logrite.state": state });
      return comment(stream, state);
    }

    function attributeWrapper(stream, state) {
	logger.error(`>>> Entering attributeWrapper(stream=${stream},state=${state})`, { "event.action": "attributeWrapper", "logrite.stream": stream, "logrite.state": state });
      if (stream.eat(state.stack.endQuote)) {
        state.line = state.stack.line;
		logger.debug(`attributeWrapper(stream=${stream},state=${state}): state.line → ${state.line}`, { "event.action": "attributeWrapper", "logrite.stream": stream, "logrite.state": state });
		logger.debug(`attributeWrapper(stream=${stream},state=${state}): state.stack → ${state.stack}`, { "event.action": "attributeWrapper", "logrite.stream": stream, "logrite.state": state });
        state.tokenize = state.stack.tokenize;
		logger.debug(`attributeWrapper(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "attributeWrapper", "logrite.stream": stream, "logrite.state": state });
        state.stack = state.stack.parent;
        return null;
      }
      if (stream.match(wrappedAttributeNameRegexp)) {
        state.tokenize = attributeWrapperAssign;
        return "slimAttribute";
      }
      stream.next();
	logger.warn(`<<< Exiting attributeWrapper(stream=${stream},state=${state})`, { "event.action": "attributeWrapper", "logrite.stream": stream, "logrite.state": state });
      return null;
    }
    function attributeWrapperAssign(stream, state) {
	logger.error(`>>> Entering attributeWrapperAssign(stream=${stream},state=${state})`, { "event.action": "attributeWrapperAssign", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(/^==?/)) {
        state.tokenize = attributeWrapperValue;
		logger.debug(`attributeWrapperAssign(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "attributeWrapperAssign", "logrite.stream": stream, "logrite.state": state });
        return null;
      }
	logger.warn(`<<< Exiting attributeWrapperAssign(stream=${stream},state=${state})`, { "event.action": "attributeWrapperAssign", "logrite.stream": stream, "logrite.state": state });
      return attributeWrapper(stream, state);
    }
    function attributeWrapperValue(stream, state) {
	logger.error(`>>> Entering attributeWrapperValue(stream=${stream},state=${state})`, { "event.action": "attributeWrapperValue", "logrite.stream": stream, "logrite.state": state });
      var ch = stream.peek();
      if (ch == '"' || ch == "\'") {
        state.tokenize = readQuoted(ch, "string", true, false, attributeWrapper);
        stream.next();
        return state.tokenize(stream, state);
      }
      if (ch == '[') {
        return startRubySplat(attributeWrapper)(stream, state);
      }
      if (stream.match(/^(true|false|nil)\b/)) {
        state.tokenize = attributeWrapper;
		logger.debug(`attributeWrapperValue(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "attributeWrapperValue", "logrite.stream": stream, "logrite.state": state });
	logger.error(`attributeWrapperValue(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "attributeWrapperValue", "logrite.stream": stream, "logrite.state": state });
        return "keyword";
      }
      return startRubySplat(attributeWrapper)(stream, state);
    }

    function startAttributeWrapperMode(state, endQuote, tokenize) {
      state.stack = {
        parent: state.stack,
        style: "wrapper",
        indented: state.indented + 1,
        tokenize: tokenize,
        line: state.line,
        endQuote: endQuote
      };
      state.line = state.tokenize = attributeWrapper;
      return null;
    }

    function sub(stream, state) {
	logger.error(`>>> Entering sub(stream=${stream},state=${state})`, { "event.action": "sub", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(/^#\{/)) {
        state.tokenize = rubyInQuote("}", state.tokenize);
        return null;
      }
      var subStream = new CodeMirror.StringStream(stream.string.slice(state.stack.indented), stream.tabSize);
	logger.error(`sub(stream=${stream},state=${state}): subStream → ${subStream}`, { "event.action": "sub", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`sub(stream=${stream},state=${state}): subStream.start → ${subStream.start}`, { "event.action": "sub", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`sub(stream=${stream},state=${state}): subStream.lastColumnPos → ${subStream.lastColumnPos}`, { "event.action": "sub", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`sub(stream=${stream},state=${state}): subStream.lastColumnValue → ${subStream.lastColumnValue}`, { "event.action": "sub", "logrite.stream": stream, "logrite.state": state });
      subStream.pos = stream.pos - state.stack.indented;
	logger.debug(`sub(stream=${stream},state=${state}): subStream.pos → ${subStream.pos}`, { "event.action": "sub", "logrite.stream": stream, "logrite.state": state });
      subStream.start = stream.start - state.stack.indented;
      subStream.lastColumnPos = stream.lastColumnPos - state.stack.indented;
      subStream.lastColumnValue = stream.lastColumnValue - state.stack.indented;
      var style = state.subMode.token(subStream, state.subState);
	logger.error(`sub(stream=${stream},state=${state}): style → ${style}`, { "event.action": "sub", "logrite.stream": stream, "logrite.state": state });
      stream.pos = subStream.pos + state.stack.indented;
	logger.warn(`<<< Exiting sub(stream=${stream},state=${state})`, { "event.action": "sub", "logrite.stream": stream, "logrite.state": state });
      return style;
    }
    function firstSub(stream, state) {
	logger.error(`>>> Entering firstSub(stream=${stream},state=${state})`, { "event.action": "firstSub", "logrite.stream": stream, "logrite.state": state });
      state.stack.indented = stream.column();
      state.line = state.tokenize = sub;
	logger.debug(`firstSub(stream=${stream},state=${state}): state.line → ${state.line}`, { "event.action": "firstSub", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting firstSub(stream=${stream},state=${state})`, { "event.action": "firstSub", "logrite.stream": stream, "logrite.state": state });
      return state.tokenize(stream, state);
    }

    function createMode(mode) {
	logger.error(`>>> Entering createMode(mode=${mode})`, { "event.action": "createMode", "logrite.mode": mode });
      var query = embedded[mode];
      var spec = CodeMirror.mimeModes[query];
      if (spec) {
        return CodeMirror.getMode(config, spec);
      }
      var factory = CodeMirror.modes[query];
      if (factory) {
        return factory(config, {name: query});
      }
	logger.warn(`<<< Exiting createMode(mode=${mode})`, { "event.action": "createMode", "logrite.mode": mode });
      return CodeMirror.getMode(config, "null");
    }

    function getMode(mode) {
	logger.error(`>>> Entering getMode(mode=${mode})`, { "event.action": "getMode", "logrite.mode": mode });
      if (!modes.hasOwnProperty(mode)) {
        return modes[mode] = createMode(mode);
      }
	logger.warn(`<<< Exiting getMode(mode=${mode})`, { "event.action": "getMode", "logrite.mode": mode });
      return modes[mode];
    }

    function startSubMode(mode, state) {
	logger.error(`>>> Entering startSubMode(mode=${mode},state=${state})`, { "event.action": "startSubMode", "logrite.mode": mode, "logrite.state": state });
      var subMode = getMode(mode);
      var subState = subMode.startState && subMode.startState();
	logger.error(`startSubMode(mode=${mode},state=${state}): subMode → ${subMode}`, { "event.action": "startSubMode", "logrite.mode": mode, "logrite.state": state });
	logger.error(`startSubMode(mode=${mode},state=${state}): subState → ${subState}`, { "event.action": "startSubMode", "logrite.mode": mode, "logrite.state": state });
	logger.debug(`startSubMode(mode=${mode},state=${state}): state.subState → ${state.subState}`, { "event.action": "startSubMode", "logrite.mode": mode, "logrite.state": state });

      state.subMode = subMode;
	logger.debug(`startSubMode(mode=${mode},state=${state}): state.subMode → ${state.subMode}`, { "event.action": "startSubMode", "logrite.mode": mode, "logrite.state": state });
      state.subState = subState;

      state.stack = {
        parent: state.stack,
        style: "sub",
        indented: state.indented + 1,
        tokenize: state.line
      };
      state.line = state.tokenize = firstSub;
	logger.debug(`startSubMode(mode=${mode},state=${state}): state.stack → ${state.stack}`, { "event.action": "startSubMode", "logrite.mode": mode, "logrite.state": state });
	logger.debug(`startSubMode(mode=${mode},state=${state}): state.line → ${state.line}`, { "event.action": "startSubMode", "logrite.mode": mode, "logrite.state": state });
      return "slimSubmode";
    }

    function doctypeLine(stream, _state) {
	logger.error(`>>> Entering doctypeLine(stream=${stream},_state=${_state})`, { "event.action": "doctypeLine", "logrite.stream": stream, "logrite._state": _state });
      stream.skipToEnd();
	logger.warn(`<<< Exiting doctypeLine(stream=${stream},_state=${_state})`, { "event.action": "doctypeLine", "logrite.stream": stream, "logrite._state": _state });
      return "slimDoctype";
    }

    function startLine(stream, state) {
	logger.error(`>>> Entering startLine(stream=${stream},state=${state})`, { "event.action": "startLine", "logrite.stream": stream, "logrite.state": state });
      var ch = stream.peek();
      if (ch == '<') {
		logger.error(`startLine(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "startLine", "logrite.stream": stream, "logrite.state": state });
        return (state.tokenize = startHtmlLine(state.tokenize))(stream, state);
      }
      if (stream.match(/^[|']/)) {
		logger.debug(`startLine(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "startLine", "logrite.stream": stream, "logrite.state": state });
        return startHtmlMode(stream, state, 1);
      }
      if (stream.match(/^\/(!|\[\w+])?/)) {
        return commentMode(stream, state);
      }
      if (stream.match(/^(-|==?[<>]?)/)) {
        state.tokenize = lineContinuable(stream.column(), commaContinuable(stream.column(), ruby));
        return "slimSwitch";
      }
      if (stream.match(/^doctype\b/)) {
        state.tokenize = doctypeLine;
        return "keyword";
      }

      var m = stream.match(embeddedRegexp);
	logger.error(`startLine(stream=${stream},state=${state}): m → ${m}`, { "event.action": "startLine", "logrite.stream": stream, "logrite.state": state });
      if (m) {
        return startSubMode(m[1], state);
      }

	logger.warn(`<<< Exiting startLine(stream=${stream},state=${state})`, { "event.action": "startLine", "logrite.stream": stream, "logrite.state": state });
      return slimTag(stream, state);
    }

    function slim(stream, state) {
	logger.error(`>>> Entering slim(stream=${stream},state=${state})`, { "event.action": "slim", "logrite.stream": stream, "logrite.state": state });
      if (state.startOfLine) {
        return startLine(stream, state);
      }
	logger.warn(`<<< Exiting slim(stream=${stream},state=${state})`, { "event.action": "slim", "logrite.stream": stream, "logrite.state": state });
      return slimTag(stream, state);
    }

    function slimTag(stream, state) {
	logger.error(`>>> Entering slimTag(stream=${stream},state=${state})`, { "event.action": "slimTag", "logrite.stream": stream, "logrite.state": state });
      if (stream.eat('*')) {
        state.tokenize = startRubySplat(slimTagExtras);
        return null;
      }
      if (stream.match(nameRegexp)) {
        state.tokenize = slimTagExtras;
		logger.debug(`slimTag(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "slimTag", "logrite.stream": stream, "logrite.state": state });
        return "slimTag";
      }
	logger.warn(`<<< Exiting slimTag(stream=${stream},state=${state})`, { "event.action": "slimTag", "logrite.stream": stream, "logrite.state": state });
      return slimClass(stream, state);
    }
    function slimTagExtras(stream, state) {
	logger.error(`>>> Entering slimTagExtras(stream=${stream},state=${state})`, { "event.action": "slimTagExtras", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(/^(<>?|><?)/)) {
        state.tokenize = slimClass;
		logger.debug(`slimTagExtras(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "slimTagExtras", "logrite.stream": stream, "logrite.state": state });
        return null;
      }
	logger.warn(`<<< Exiting slimTagExtras(stream=${stream},state=${state})`, { "event.action": "slimTagExtras", "logrite.stream": stream, "logrite.state": state });
      return slimClass(stream, state);
    }
    function slimClass(stream, state) {
	logger.error(`>>> Entering slimClass(stream=${stream},state=${state})`, { "event.action": "slimClass", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(classIdRegexp)) {
        state.tokenize = slimClass;
		logger.debug(`slimClass(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "slimClass", "logrite.stream": stream, "logrite.state": state });
        return "slimId";
      }
      if (stream.match(classNameRegexp)) {
        state.tokenize = slimClass;
        return "slimClass";
      }
	logger.warn(`<<< Exiting slimClass(stream=${stream},state=${state})`, { "event.action": "slimClass", "logrite.stream": stream, "logrite.state": state });
      return slimAttribute(stream, state);
    }
    function slimAttribute(stream, state) {
	logger.error(`>>> Entering slimAttribute(stream=${stream},state=${state})`, { "event.action": "slimAttribute", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(/^([\[\{\(])/)) {
        return startAttributeWrapperMode(state, closing[RegExp.$1], slimAttribute);
      }
      if (stream.match(attributeNameRegexp)) {
        state.tokenize = slimAttributeAssign;
		logger.debug(`slimAttribute(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "slimAttribute", "logrite.stream": stream, "logrite.state": state });
        return "slimAttribute";
      }
      if (stream.peek() == '*') {
        stream.next();
        state.tokenize = startRubySplat(slimContent);
        return null;
      }
	logger.warn(`<<< Exiting slimAttribute(stream=${stream},state=${state})`, { "event.action": "slimAttribute", "logrite.stream": stream, "logrite.state": state });
      return slimContent(stream, state);
    }
    function slimAttributeAssign(stream, state) {
	logger.error(`>>> Entering slimAttributeAssign(stream=${stream},state=${state})`, { "event.action": "slimAttributeAssign", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(/^==?/)) {
        state.tokenize = slimAttributeValue;
		logger.debug(`slimAttributeAssign(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "slimAttributeAssign", "logrite.stream": stream, "logrite.state": state });
        return null;
      }
      // should never happen, because of forward lookup
	logger.warn(`<<< Exiting slimAttributeAssign(stream=${stream},state=${state})`, { "event.action": "slimAttributeAssign", "logrite.stream": stream, "logrite.state": state });
      return slimAttribute(stream, state);
    }

    function slimAttributeValue(stream, state) {
	logger.error(`>>> Entering slimAttributeValue(stream=${stream},state=${state})`, { "event.action": "slimAttributeValue", "logrite.stream": stream, "logrite.state": state });
      var ch = stream.peek();
      if (ch == '"' || ch == "\'") {
        state.tokenize = readQuoted(ch, "string", true, false, slimAttribute);
        stream.next();
        return state.tokenize(stream, state);
      }
      if (ch == '[') {
        return startRubySplat(slimAttribute)(stream, state);
      }
      if (ch == ':') {
        return startRubySplat(slimAttributeSymbols)(stream, state);
      }
      if (stream.match(/^(true|false|nil)\b/)) {
        state.tokenize = slimAttribute;
		logger.debug(`slimAttributeValue(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "slimAttributeValue", "logrite.stream": stream, "logrite.state": state });
	logger.error(`slimAttributeValue(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "slimAttributeValue", "logrite.stream": stream, "logrite.state": state });
        return "keyword";
      }
      return startRubySplat(slimAttribute)(stream, state);
    }
    function slimAttributeSymbols(stream, state) {
	logger.error(`>>> Entering slimAttributeSymbols(stream=${stream},state=${state})`, { "event.action": "slimAttributeSymbols", "logrite.stream": stream, "logrite.state": state });
      stream.backUp(1);
      if (stream.match(/^[^\s],(?=:)/)) {
        state.tokenize = startRubySplat(slimAttributeSymbols);
        return null;
      }
      stream.next();
	logger.warn(`<<< Exiting slimAttributeSymbols(stream=${stream},state=${state})`, { "event.action": "slimAttributeSymbols", "logrite.stream": stream, "logrite.state": state });
      return slimAttribute(stream, state);
    }
    function readQuoted(quote, style, embed, unescaped, nextTokenize) {
      return function(stream, state) {
        finishContinue(state);
        var fresh = stream.current().length == 0;
        if (stream.match(/^\\$/, fresh)) {
          if (!fresh) return style;
          continueLine(state, state.indented);
          return "lineContinuation";
        }
        if (stream.match(/^#\{/, fresh)) {
          if (!fresh) return style;
          state.tokenize = rubyInQuote("}", state.tokenize);
          return null;
        }
        var escaped = false, ch;
        while ((ch = stream.next()) != null) {
          if (ch == quote && (unescaped || !escaped)) {
            state.tokenize = nextTokenize;
            break;
          }
          if (embed && ch == "#" && !escaped) {
            if (stream.eat("{")) {
              stream.backUp(2);
              break;
            }
          }
          escaped = !escaped && ch == "\\";
        }
        if (stream.eol() && escaped) {
          stream.backUp(1);
        }
        return style;
      };
    }
    function slimContent(stream, state) {
	logger.error(`>>> Entering slimContent(stream=${stream},state=${state})`, { "event.action": "slimContent", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(/^==?/)) {
        state.tokenize = ruby;
		logger.debug(`slimContent(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "slimContent", "logrite.stream": stream, "logrite.state": state });
        return "slimSwitch";
      }
      if (stream.match(/^\/$/)) { // tag close hint
        state.tokenize = slim;
        return null;
      }
      if (stream.match(/^:/)) { // inline tag
        state.tokenize = slimTag;
        return "slimSwitch";
      }
      startHtmlMode(stream, state, 0);
	logger.warn(`<<< Exiting slimContent(stream=${stream},state=${state})`, { "event.action": "slimContent", "logrite.stream": stream, "logrite.state": state });
      return state.tokenize(stream, state);
    }

    var mode = {
      // default to html mode
      startState: function() {
		logger.error('>>> Entering startState()', { "event.action": "startState" });
        var htmlState = htmlMode.startState();
        var rubyState = rubyMode.startState();
        return {
          htmlState: htmlState,
          rubyState: rubyState,
          stack: null,
          last: null,
          tokenize: slim,
          line: slim,
          indented: 0
        };
		logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      },

      copyState: function(state) {
		logger.error(`>>> Entering copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
        return {
          htmlState : CodeMirror.copyState(htmlMode, state.htmlState),
          rubyState: CodeMirror.copyState(rubyMode, state.rubyState),
          subMode: state.subMode,
          subState: state.subMode && CodeMirror.copyState(state.subMode, state.subState),
          stack: state.stack,
          last: state.last,
          tokenize: state.tokenize,
          line: state.line
        };
		logger.warn(`<<< Exiting copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
      },

      token: function(stream, state) {
		logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        if (stream.sol()) {
          state.indented = stream.indentation();
          state.startOfLine = true;
		logger.debug(`token(stream=${stream},state=${state}): state.startOfLine → ${state.startOfLine}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          state.tokenize = state.line;
		logger.debug(`token(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          while (state.stack && state.stack.indented > state.indented && state.last != "slimSubmode") {
            state.line = state.tokenize = state.stack.tokenize;
			logger.debug(`token(stream=${stream},state=${state}): state.line → ${state.line}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
			logger.debug(`token(stream=${stream},state=${state}): state.subMode → ${state.subMode}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
			logger.debug(`token(stream=${stream},state=${state}): state.subState → ${state.subState}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
            state.stack = state.stack.parent;
			logger.debug(`token(stream=${stream},state=${state}): state.stack → ${state.stack}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
            state.subMode = null;
            state.subState = null;
          }
        }
        if (stream.eatSpace()) return null;
        var style = state.tokenize(stream, state);
		logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        state.startOfLine = false;
        if (style) state.last = style;
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        return styleMap.hasOwnProperty(style) ? styleMap[style] : style;
      },

      blankLine: function(state) {
		logger.error(`>>> Entering blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
        if (state.subMode && state.subMode.blankLine) {
		logger.warn(`<<< Exiting blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
          return state.subMode.blankLine(state.subState);
        }
      },

      innerMode: function(state) {
		logger.error(`>>> Entering innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
        if (state.subMode) return {state: state.subState, mode: state.subMode};
		logger.warn(`<<< Exiting innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
        return {state: state, mode: mode};
      }

      //indent: function(state) {
      //  return state.indented;
      //}
    };
    return mode;
  }, "htmlmixed", "ruby");

  CodeMirror.defineMIME("text/x-slim", "slim");
  CodeMirror.defineMIME("application/x-slim", "slim");
});
