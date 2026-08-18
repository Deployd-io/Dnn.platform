const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Slim Highlighting for CodeMirror copyright (c) HicknHack Software Gmbh

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

  CodeMirror.defineMode("slim", function(config) {
	logger.debug('>>> Entering defineMode()');
    var htmlMode = CodeMirror.getMode(config, {name: "htmlmixed"});
    var rubyMode = CodeMirror.getMode(config, "ruby");
	logger.debug(`defineMode(): htmlMode → ${htmlMode}`);
	logger.debug(`defineMode(): rubyMode → ${rubyMode}`);
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
	logger.debug(`<<< Exiting function(exports=${exports})`);
      return new RegExp("^("+arr.join('|')+"):");
    }(embedded);
	logger.debug('<<< Exiting defineMode()');

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
	logger.debug(`defineMode(): nameStartChar → ${nameStartChar}`);
    var nameChar = nameStartChar + "\\-0-9\xB7\u0300-\u036F\u203F-\u2040";
	logger.debug(`defineMode(): nameChar → ${nameChar}`);
    var nameRegexp = new RegExp("^[:"+nameStartChar+"](?::["+nameChar+"]|["+nameChar+"]*)");
	logger.debug(`defineMode(): nameRegexp → ${nameRegexp}`);
    var attributeNameRegexp = new RegExp("^[:"+nameStartChar+"][:\\."+nameChar+"]*(?=\\s*=)");
	logger.debug(`defineMode(): attributeNameRegexp → ${attributeNameRegexp}`);
    var wrappedAttributeNameRegexp = new RegExp("^[:"+nameStartChar+"][:\\."+nameChar+"]*");
	logger.debug(`defineMode(): wrappedAttributeNameRegexp → ${wrappedAttributeNameRegexp}`);
    var classNameRegexp = /^\.-?[_a-zA-Z]+[\w\-]*/;
    var classIdRegexp = /^#[_a-zA-Z]+[\w\-]*/;

    function backup(pos, tokenize, style) {
      var restore = function(stream, state) {
		logger.debug(`>>> Entering restore(stream=${stream},state=${state})`);
        state.tokenize = tokenize;
        if (stream.pos < pos) {
		logger.debug(`restore(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
          stream.pos = pos;
		logger.debug(`<<< Exiting restore(stream=${stream},state=${state})`);
          return style;
        }
        return state.tokenize(stream, state);
      };
      return function(stream, state) {
        state.tokenize = restore;
        return tokenize(stream, state);
      };
    }

    function maybeBackup(stream, state, pat, offset, style) {
	logger.debug(`>>> Entering maybeBackup(stream=${stream},state=${state},pat=${pat},offset=${offset},style=${style})`);
      var cur = stream.current();
      var idx = cur.search(pat);
	logger.debug(`maybeBackup(stream=${stream},state=${state},pat=${pat},offset=${offset},style=${style}): cur → ${cur}`);
      if (idx > -1) {
        state.tokenize = backup(stream.pos, state.tokenize, style);
        stream.backUp(cur.length - idx - offset);
      }
	logger.debug(`maybeBackup(stream=${stream},state=${state},pat=${pat},offset=${offset},style=${style}): idx → ${idx}`);
	logger.debug(`<<< Exiting maybeBackup(stream=${stream},state=${state},pat=${pat},offset=${offset},style=${style})`);
      return style;
    }

    function continueLine(state, column) {
	logger.debug(`>>> Entering continueLine(state=${state},column=${column})`);
      state.stack = {
        parent: state.stack,
        style: "continuation",
        indented: column,
        tokenize: state.line
      };
      state.line = state.tokenize;
	logger.debug(`continueLine(state=${state},column=${column}): state.line → ${state.line}`);
	logger.debug(`continueLine(state=${state},column=${column}): state.stack → ${state.stack}`);
	logger.debug(`<<< Exiting continueLine(state=${state},column=${column})`);
    }
    function finishContinue(state) {
      if (state.line == state.tokenize) {
		logger.debug(`>>> Entering finishContinue(state=${state})`);
		logger.debug(`finishContinue(state=${state}): state.stack → ${state.stack}`);
        state.line = state.stack.tokenize;
		logger.debug(`finishContinue(state=${state}): state.line → ${state.line}`);
		logger.debug(`<<< Exiting finishContinue(state=${state})`);
        state.stack = state.stack.parent;
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
		logger.debug(`>>> Entering runSplat(stream=${stream},state=${state})`);
        if (state.rubyState.tokenize.length == 1 && !state.rubyState.context.prev) {
          stream.backUp(1);
          if (stream.eatSpace()) {
            state.rubyState = rubyState;
            state.tokenize = tokenize;
			logger.debug(`runSplat(stream=${stream},state=${state}): state.rubyState → ${state.rubyState}`);
            return tokenize(stream, state);
          }
          stream.next();
		logger.debug(`runSplat(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        }
		logger.debug(`<<< Exiting runSplat(stream=${stream},state=${state})`);
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
	logger.debug(`>>> Entering ruby(stream=${stream},state=${state})`);
	logger.debug(`<<< Exiting ruby(stream=${stream},state=${state})`);
      return rubyMode.token(stream, state.rubyState);
    }

    function htmlLine(stream, state) {
	logger.debug(`>>> Entering htmlLine(stream=${stream},state=${state})`);
      if (stream.match(/^\\$/)) {
	logger.debug(`<<< Exiting htmlLine(stream=${stream},state=${state})`);
        return "lineContinuation";
      }
      return html(stream, state);
    }
    function html(stream, state) {
	logger.debug(`>>> Entering html(stream=${stream},state=${state})`);
      if (stream.match(/^#\{/)) {
        state.tokenize = rubyInQuote("}", state.tokenize);
        return null;
      }
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
	logger.debug(`>>> Entering startHtmlMode(stream=${stream},state=${state},offset=${offset})`);
      state.stack = {
        parent: state.stack,
        style: "html",
        indented: stream.column() + offset, // pipe + space
        tokenize: state.line
      };
      state.line = state.tokenize = html;
	logger.debug(`startHtmlMode(stream=${stream},state=${state},offset=${offset}): state.stack → ${state.stack}`);
	logger.debug(`startHtmlMode(stream=${stream},state=${state},offset=${offset}): state.line → ${state.line}`);
	logger.debug(`<<< Exiting startHtmlMode(stream=${stream},state=${state},offset=${offset})`);
      return null;
    }

    function comment(stream, state) {
	logger.debug(`>>> Entering comment(stream=${stream},state=${state})`);
      stream.skipToEnd();
	logger.debug(`<<< Exiting comment(stream=${stream},state=${state})`);
      return state.stack.style;
    }

    function commentMode(stream, state) {
	logger.debug(`>>> Entering commentMode(stream=${stream},state=${state})`);
      state.stack = {
        parent: state.stack,
        style: "comment",
        indented: state.indented + 1,
        tokenize: state.line
      };
      state.line = comment;
	logger.debug(`commentMode(stream=${stream},state=${state}): state.stack → ${state.stack}`);
	logger.debug(`commentMode(stream=${stream},state=${state}): state.line → ${state.line}`);
	logger.debug(`<<< Exiting commentMode(stream=${stream},state=${state})`);
      return comment(stream, state);
    }

    function attributeWrapper(stream, state) {
	logger.debug(`>>> Entering attributeWrapper(stream=${stream},state=${state})`);
      if (stream.eat(state.stack.endQuote)) {
        state.line = state.stack.line;
		logger.debug(`attributeWrapper(stream=${stream},state=${state}): state.line → ${state.line}`);
		logger.debug(`attributeWrapper(stream=${stream},state=${state}): state.stack → ${state.stack}`);
        state.tokenize = state.stack.tokenize;
		logger.debug(`attributeWrapper(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        state.stack = state.stack.parent;
        return null;
      }
      if (stream.match(wrappedAttributeNameRegexp)) {
        state.tokenize = attributeWrapperAssign;
        return "slimAttribute";
      }
      stream.next();
	logger.debug(`<<< Exiting attributeWrapper(stream=${stream},state=${state})`);
      return null;
    }
    function attributeWrapperAssign(stream, state) {
	logger.debug(`>>> Entering attributeWrapperAssign(stream=${stream},state=${state})`);
      if (stream.match(/^==?/)) {
        state.tokenize = attributeWrapperValue;
		logger.debug(`attributeWrapperAssign(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting attributeWrapperAssign(stream=${stream},state=${state})`);
        return null;
      }
      return attributeWrapper(stream, state);
    }
    function attributeWrapperValue(stream, state) {
	logger.debug(`>>> Entering attributeWrapperValue(stream=${stream},state=${state})`);
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
		logger.debug(`attributeWrapperValue(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`attributeWrapperValue(stream=${stream},state=${state}): ch → ${ch}`);
	logger.debug(`<<< Exiting attributeWrapperValue(stream=${stream},state=${state})`);
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
	logger.debug(`>>> Entering sub(stream=${stream},state=${state})`);
      if (stream.match(/^#\{/)) {
        state.tokenize = rubyInQuote("}", state.tokenize);
        return null;
      }
      var subStream = new CodeMirror.StringStream(stream.string.slice(state.stack.indented), stream.tabSize);
      subStream.pos = stream.pos - state.stack.indented;
      subStream.start = stream.start - state.stack.indented;
      subStream.lastColumnPos = stream.lastColumnPos - state.stack.indented;
      subStream.lastColumnValue = stream.lastColumnValue - state.stack.indented;
      var style = state.subMode.token(subStream, state.subState);
      stream.pos = subStream.pos + state.stack.indented;
	logger.debug(`sub(stream=${stream},state=${state}): subStream → ${subStream}`);
	logger.debug(`sub(stream=${stream},state=${state}): style → ${style}`);
	logger.debug(`sub(stream=${stream},state=${state}): subStream.pos → ${subStream.pos}`);
	logger.debug(`<<< Exiting sub(stream=${stream},state=${state})`);
      return style;
    }
    function firstSub(stream, state) {
      state.stack.indented = stream.column();
      state.line = state.tokenize = sub;
	logger.debug(`<<< Exiting firstSub(stream=${stream},state=${state})`);
      return state.tokenize(stream, state);
    }

    function createMode(mode) {
      var query = embedded[mode];
      var spec = CodeMirror.mimeModes[query];
      if (spec) {
		logger.debug(`>>> Entering createMode(mode=${mode})`);
		logger.debug(`<<< Exiting createMode(mode=${mode})`);
        return CodeMirror.getMode(config, spec);
      }
      var factory = CodeMirror.modes[query];
      if (factory) {
        return factory(config, {name: query});
      }
      return CodeMirror.getMode(config, "null");
    }

    function getMode(mode) {
	logger.debug(`>>> Entering getMode(mode=${mode})`);
      if (!modes.hasOwnProperty(mode)) {
	logger.debug(`<<< Exiting getMode(mode=${mode})`);
        return modes[mode] = createMode(mode);
      }
      return modes[mode];
    }

    function startSubMode(mode, state) {
	logger.debug(`>>> Entering startSubMode(mode=${mode},state=${state})`);
      var subMode = getMode(mode);
      var subState = subMode.startState && subMode.startState();
	logger.debug(`startSubMode(mode=${mode},state=${state}): subMode → ${subMode}`);
	logger.debug(`startSubMode(mode=${mode},state=${state}): subState → ${subState}`);

      state.subMode = subMode;
	logger.debug(`startSubMode(mode=${mode},state=${state}): state.subMode → ${state.subMode}`);
      state.subState = subState;

      state.stack = {
        parent: state.stack,
        style: "sub",
        indented: state.indented + 1,
        tokenize: state.line
      };
      state.line = state.tokenize = firstSub;
	logger.debug(`startSubMode(mode=${mode},state=${state}): state.subState → ${state.subState}`);
	logger.debug(`startSubMode(mode=${mode},state=${state}): state.stack → ${state.stack}`);
	logger.debug(`<<< Exiting startSubMode(mode=${mode},state=${state})`);
      return "slimSubmode";
    }

    function doctypeLine(stream, _state) {
	logger.debug(`>>> Entering doctypeLine(stream=${stream},_state=${_state})`);
      stream.skipToEnd();
	logger.debug(`<<< Exiting doctypeLine(stream=${stream},_state=${_state})`);
      return "slimDoctype";
    }

    function startLine(stream, state) {
	logger.debug(`>>> Entering startLine(stream=${stream},state=${state})`);
      var ch = stream.peek();
      if (ch == '<') {
		logger.debug(`startLine(stream=${stream},state=${state}): ch → ${ch}`);
        return (state.tokenize = startHtmlLine(state.tokenize))(stream, state);
      }
      if (stream.match(/^[|']/)) {
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
      if (m) {
        return startSubMode(m[1], state);
      }

      return slimTag(stream, state);
    }

    function slim(stream, state) {
	logger.debug(`>>> Entering slim(stream=${stream},state=${state})`);
      if (state.startOfLine) {
	logger.debug(`<<< Exiting slim(stream=${stream},state=${state})`);
        return startLine(stream, state);
      }
      return slimTag(stream, state);
    }

    function slimTag(stream, state) {
	logger.debug(`>>> Entering slimTag(stream=${stream},state=${state})`);
      if (stream.eat('*')) {
        state.tokenize = startRubySplat(slimTagExtras);
        return null;
      }
      if (stream.match(nameRegexp)) {
        state.tokenize = slimTagExtras;
		logger.debug(`slimTag(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting slimTag(stream=${stream},state=${state})`);
        return "slimTag";
      }
      return slimClass(stream, state);
    }
    function slimTagExtras(stream, state) {
	logger.debug(`>>> Entering slimTagExtras(stream=${stream},state=${state})`);
      if (stream.match(/^(<>?|><?)/)) {
        state.tokenize = slimClass;
		logger.debug(`slimTagExtras(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting slimTagExtras(stream=${stream},state=${state})`);
        return null;
      }
      return slimClass(stream, state);
    }
    function slimClass(stream, state) {
	logger.debug(`>>> Entering slimClass(stream=${stream},state=${state})`);
      if (stream.match(classIdRegexp)) {
        state.tokenize = slimClass;
		logger.debug(`slimClass(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        return "slimId";
      }
      if (stream.match(classNameRegexp)) {
        state.tokenize = slimClass;
	logger.debug(`<<< Exiting slimClass(stream=${stream},state=${state})`);
        return "slimClass";
      }
      return slimAttribute(stream, state);
    }
    function slimAttribute(stream, state) {
	logger.debug(`>>> Entering slimAttribute(stream=${stream},state=${state})`);
      if (stream.match(/^([\[\{\(])/)) {
        return startAttributeWrapperMode(state, closing[RegExp.$1], slimAttribute);
      }
      if (stream.match(attributeNameRegexp)) {
        state.tokenize = slimAttributeAssign;
		logger.debug(`slimAttribute(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        return "slimAttribute";
      }
      if (stream.peek() == '*') {
        stream.next();
        state.tokenize = startRubySplat(slimContent);
        return null;
      }
      return slimContent(stream, state);
    }
    function slimAttributeAssign(stream, state) {
	logger.debug(`>>> Entering slimAttributeAssign(stream=${stream},state=${state})`);
      if (stream.match(/^==?/)) {
        state.tokenize = slimAttributeValue;
		logger.debug(`slimAttributeAssign(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting slimAttributeAssign(stream=${stream},state=${state})`);
        return null;
      }
      // should never happen, because of forward lookup
      return slimAttribute(stream, state);
    }

    function slimAttributeValue(stream, state) {
	logger.debug(`>>> Entering slimAttributeValue(stream=${stream},state=${state})`);
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
		logger.debug(`slimAttributeValue(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`slimAttributeValue(stream=${stream},state=${state}): ch → ${ch}`);
	logger.debug(`<<< Exiting slimAttributeValue(stream=${stream},state=${state})`);
        return "keyword";
      }
      return startRubySplat(slimAttribute)(stream, state);
    }
    function slimAttributeSymbols(stream, state) {
	logger.debug(`>>> Entering slimAttributeSymbols(stream=${stream},state=${state})`);
      stream.backUp(1);
      if (stream.match(/^[^\s],(?=:)/)) {
        state.tokenize = startRubySplat(slimAttributeSymbols);
        return null;
      }
      stream.next();
	logger.debug(`<<< Exiting slimAttributeSymbols(stream=${stream},state=${state})`);
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
	logger.debug(`>>> Entering slimContent(stream=${stream},state=${state})`);
      if (stream.match(/^==?/)) {
        state.tokenize = ruby;
		logger.debug(`slimContent(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
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
	logger.debug(`<<< Exiting slimContent(stream=${stream},state=${state})`);
      return state.tokenize(stream, state);
    }

    var mode = {
      // default to html mode
      startState: function() {
		logger.debug('>>> Entering startState()');
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
		logger.debug('<<< Exiting startState()');
      },

      copyState: function(state) {
		logger.debug(`>>> Entering copyState(state=${state})`);
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
		logger.debug(`<<< Exiting copyState(state=${state})`);
      },

      token: function(stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
        if (stream.sol()) {
          state.indented = stream.indentation();
          state.startOfLine = true;
		logger.debug(`token(stream=${stream},state=${state}): state.startOfLine → ${state.startOfLine}`);
          state.tokenize = state.line;
		logger.debug(`token(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
          while (state.stack && state.stack.indented > state.indented && state.last != "slimSubmode") {
            state.line = state.tokenize = state.stack.tokenize;
			logger.debug(`token(stream=${stream},state=${state}): state.line → ${state.line}`);
			logger.debug(`token(stream=${stream},state=${state}): state.subMode → ${state.subMode}`);
			logger.debug(`token(stream=${stream},state=${state}): state.subState → ${state.subState}`);
            state.stack = state.stack.parent;
			logger.debug(`token(stream=${stream},state=${state}): state.stack → ${state.stack}`);
            state.subMode = null;
            state.subState = null;
          }
        }
        if (stream.eatSpace()) return null;
        var style = state.tokenize(stream, state);
		logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
        state.startOfLine = false;
        if (style) state.last = style;
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
        return styleMap.hasOwnProperty(style) ? styleMap[style] : style;
      },

      blankLine: function(state) {
		logger.debug(`>>> Entering blankLine(state=${state})`);
        if (state.subMode && state.subMode.blankLine) {
		logger.debug(`<<< Exiting blankLine(state=${state})`);
          return state.subMode.blankLine(state.subState);
        }
      },

      innerMode: function(state) {
		logger.debug(`>>> Entering innerMode(state=${state})`);
        if (state.subMode) return {state: state.subState, mode: state.subMode};
		logger.debug(`<<< Exiting innerMode(state=${state})`);
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
  logger.debug(`<<< Exiting indent(state=${state})`);
});
