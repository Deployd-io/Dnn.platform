const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../xml/xml"), require("../javascript/javascript"), require("../css/css"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../xml/xml", "../javascript/javascript", "../css/css"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMode("htmlmixed", function(config, parserConfig) {
  var htmlMode = CodeMirror.getMode(config, {name: "xml",
                                             htmlMode: true,
                                             multilineTagIndentFactor: parserConfig.multilineTagIndentFactor,
                                             multilineTagIndentPastTag: parserConfig.multilineTagIndentPastTag});
  var cssMode = CodeMirror.getMode(config, "css");

  var scriptTypes = [], scriptTypesConf = parserConfig && parserConfig.scriptTypes;
  scriptTypes.push({matches: /^(?:text|application)\/(?:x-)?(?:java|ecma)script$|^$/i,
                    mode: CodeMirror.getMode(config, "javascript")});
  if (scriptTypesConf) for (var i = 0; i < scriptTypesConf.length; ++i) {
    var conf = scriptTypesConf[i];
    scriptTypes.push({matches: conf.matches, mode: conf.mode && CodeMirror.getMode(config, conf.mode)});
  }
  scriptTypes.push({matches: /./,
                    mode: CodeMirror.getMode(config, "text/plain")});

  function html(stream, state) {
	logger.error(`>>> Entering html(stream=${stream},state=${state})`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
    var tagName = state.htmlState.tagName;
    if (tagName) tagName = tagName.toLowerCase();
    var style = htmlMode.token(stream, state.htmlState);
    if (tagName == "script" && /\btag\b/.test(style) && stream.current() == ">") {
      // Script block: mode to change to depends on type attribute
      var scriptType = stream.string.slice(Math.max(0, stream.pos - 100), stream.pos).match(/\btype\s*=\s*("[^"]+"|'[^']+'|\S+)[^<]*$/i);
      scriptType = scriptType ? scriptType[1] : "";
	logger.error(`html(stream=${stream},state=${state}): scriptType → ${scriptType}`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
      if (scriptType && /[\"\']/.test(scriptType.charAt(0))) scriptType = scriptType.slice(1, scriptType.length - 1);
      for (var i = 0; i < scriptTypes.length; ++i) {
        var tp = scriptTypes[i];
        if (typeof tp.matches == "string" ? scriptType == tp.matches : tp.matches.test(scriptType)) {
          if (tp.mode) {
            state.token = script;
			logger.debug(`html(stream=${stream},state=${state}): state.token → ${state.token}`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
            state.localMode = tp.mode;
			logger.debug(`html(stream=${stream},state=${state}): state.localMode → ${state.localMode}`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
            state.localState = tp.mode.startState && tp.mode.startState(htmlMode.indent(state.htmlState, ""));
          }
          break;
        }
      }
    } else if (tagName == "style" && /\btag\b/.test(style) && stream.current() == ">") {
      state.token = css;
      state.localMode = cssMode;
      state.localState = cssMode.startState(htmlMode.indent(state.htmlState, ""));
    }
	logger.error(`html(stream=${stream},state=${state}): style → ${style}`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
    return style;
  }
  function maybeBackup(stream, pat, style) {
	logger.error(`>>> Entering maybeBackup(stream=${stream},pat=${pat},style=${style})`, { "event.action": "maybeBackup", "logrite.stream": stream, "logrite.pat": pat, "logrite.style": style });
    var cur = stream.current();
    var close = cur.search(pat);
	logger.error(`maybeBackup(stream=${stream},pat=${pat},style=${style}): cur → ${cur}`, { "event.action": "maybeBackup", "logrite.stream": stream, "logrite.pat": pat, "logrite.style": style });
    if (close > -1) stream.backUp(cur.length - close);
    else if (cur.match(/<\/?$/)) {
	logger.error(`maybeBackup(stream=${stream},pat=${pat},style=${style}): close → ${close}`, { "event.action": "maybeBackup", "logrite.stream": stream, "logrite.pat": pat, "logrite.style": style });
      stream.backUp(cur.length);
      if (!stream.match(pat, false)) stream.match(cur);
    }
	logger.warn(`<<< Exiting maybeBackup(stream=${stream},pat=${pat},style=${style})`, { "event.action": "maybeBackup", "logrite.stream": stream, "logrite.pat": pat, "logrite.style": style });
    return style;
  }
  function script(stream, state) {
	logger.error(`>>> Entering script(stream=${stream},state=${state})`, { "event.action": "script", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^<\/\s*script\s*>/i, false)) {
      state.token = html;
	logger.debug(`script(stream=${stream},state=${state}): state.token → ${state.token}`, { "event.action": "script", "logrite.stream": stream, "logrite.state": state });
      state.localState = state.localMode = null;
	logger.debug(`script(stream=${stream},state=${state}): state.localState → ${state.localState}`, { "event.action": "script", "logrite.stream": stream, "logrite.state": state });
      return null;
    }
	logger.warn(`<<< Exiting script(stream=${stream},state=${state})`, { "event.action": "script", "logrite.stream": stream, "logrite.state": state });
    return maybeBackup(stream, /<\/\s*script\s*>/,
                       state.localMode.token(stream, state.localState));
  }
  function css(stream, state) {
	logger.error(`>>> Entering css(stream=${stream},state=${state})`, { "event.action": "css", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^<\/\s*style\s*>/i, false)) {
      state.token = html;
	logger.debug(`css(stream=${stream},state=${state}): state.token → ${state.token}`, { "event.action": "css", "logrite.stream": stream, "logrite.state": state });
      state.localState = state.localMode = null;
	logger.debug(`css(stream=${stream},state=${state}): state.localState → ${state.localState}`, { "event.action": "css", "logrite.stream": stream, "logrite.state": state });
      return null;
    }
	logger.warn(`<<< Exiting css(stream=${stream},state=${state})`, { "event.action": "css", "logrite.stream": stream, "logrite.state": state });
    return maybeBackup(stream, /<\/\s*style\s*>/,
                       cssMode.token(stream, state.localState));
  }

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      var state = htmlMode.startState();
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      return {token: html, localMode: null, localState: null, htmlState: state};
    },

    copyState: function(state) {
	logger.error(`>>> Entering copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
      if (state.localState)
        var local = CodeMirror.copyState(state.localMode, state.localState);
      return {token: state.token, localMode: state.localMode, localState: local,
              htmlState: CodeMirror.copyState(htmlMode, state.htmlState)};
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return state.token(stream, state);
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      if (!state.localMode || /^\s*<\//.test(textAfter))
        return htmlMode.indent(state.htmlState, textAfter);
      else if (state.localMode.indent)
        return state.localMode.indent(state.localState, textAfter);
      else
        return CodeMirror.Pass;
    },

    innerMode: function(state) {
	logger.error(`>>> Entering innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
	logger.warn(`<<< Exiting innerMode(state=${state})`, { "event.action": "innerMode", "logrite.state": state });
      return {state: state.localState || state.htmlState, mode: state.localMode || htmlMode};
    }
  };
}, "xml", "javascript", "css");

CodeMirror.defineMIME("text/html", "htmlmixed");

});
