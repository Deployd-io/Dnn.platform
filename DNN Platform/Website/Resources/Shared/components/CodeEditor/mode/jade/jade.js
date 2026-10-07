const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../javascript/javascript"), require("../css/css"), require("../htmlmixed/htmlmixed"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../javascript/javascript", "../css/css", "../htmlmixed/htmlmixed"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMode('jade', function (config) {
  // token types
  var KEYWORD = 'keyword';
  var DOCTYPE = 'meta';
  var ID = 'builtin';
  var CLASS = 'qualifier';

  var ATTRS_NEST = {
    '{': '}',
    '(': ')',
    '[': ']'
  };

  var jsMode = CodeMirror.getMode(config, 'javascript');

  function State() {
	logger.error('>>> Entering State()', { "event.action": "State" });
    this.javaScriptLine = false;
    this.javaScriptLineExcludesColon = false;

	logger.debug(`State(): this.javaScriptLine → ${this.javaScriptLine}`, { "event.action": "State" });
    this.javaScriptArguments = false;
    this.javaScriptArgumentsDepth = 0;
	logger.debug(`State(): this.javaScriptLineExcludesColon → ${this.javaScriptLineExcludesColon}`, { "event.action": "State" });

    this.isInterpolating = false;
	logger.debug(`State(): this.javaScriptArguments → ${this.javaScriptArguments}`, { "event.action": "State" });
    this.interpolationNesting = 0;

	logger.debug(`State(): this.javaScriptArgumentsDepth → ${this.javaScriptArgumentsDepth}`, { "event.action": "State" });
    this.jsState = jsMode.startState();

	logger.debug(`State(): this.isInterpolating → ${this.isInterpolating}`, { "event.action": "State" });
    this.restOfLine = '';

	logger.debug(`State(): this.interpolationNesting → ${this.interpolationNesting}`, { "event.action": "State" });
    this.isIncludeFiltered = false;
    this.isEach = false;

	logger.debug(`State(): this.restOfLine → ${this.restOfLine}`, { "event.action": "State" });
    this.lastTag = '';
    this.scriptType = '';
	logger.debug(`State(): this.isIncludeFiltered → ${this.isIncludeFiltered}`, { "event.action": "State" });

    // Attributes Mode
    this.isAttrs = false;
	logger.debug(`State(): this.isEach → ${this.isEach}`, { "event.action": "State" });
	logger.debug(`State(): this.lastTag → ${this.lastTag}`, { "event.action": "State" });
    this.attrsNest = [];
    this.inAttributeName = true;
    this.attributeIsType = false;
	logger.debug(`State(): this.scriptType → ${this.scriptType}`, { "event.action": "State" });
    this.attrValue = '';

	logger.debug(`State(): this.isAttrs → ${this.isAttrs}`, { "event.action": "State" });
    // Indented Mode
    this.indentOf = Infinity;
	logger.debug(`State(): this.attrsNest → ${this.attrsNest}`, { "event.action": "State" });
    this.indentToken = '';

	logger.debug(`State(): this.inAttributeName → ${this.inAttributeName}`, { "event.action": "State" });
    this.innerMode = null;
    this.innerState = null;
	logger.debug(`State(): this.attributeIsType → ${this.attributeIsType}`, { "event.action": "State" });

    this.innerModeForLine = false;
	logger.debug(`State(): this.innerModeForLine → ${this.innerModeForLine}`, { "event.action": "State" });
  }
  /**
   * Safely copy a state
   *
   * @return {State}
   */
  State.prototype.copy = function () {
	logger.error('>>> Entering copy()', { "event.action": "copy" });
    var res = new State();
    res.javaScriptLine = this.javaScriptLine;
	logger.error(`copy(): res → ${res}`, { "event.action": "copy" });
    res.javaScriptLineExcludesColon = this.javaScriptLineExcludesColon;
    res.javaScriptArguments = this.javaScriptArguments;
    res.javaScriptArgumentsDepth = this.javaScriptArgumentsDepth;
    res.isInterpolating = this.isInterpolating;
	logger.debug(`copy(): res.javaScriptLine → ${res.javaScriptLine}`, { "event.action": "copy" });
	logger.debug(`copy(): res.javaScriptLineExcludesColon → ${res.javaScriptLineExcludesColon}`, { "event.action": "copy" });
	logger.debug(`copy(): res.javaScriptArguments → ${res.javaScriptArguments}`, { "event.action": "copy" });
	logger.debug(`copy(): res.javaScriptArgumentsDepth → ${res.javaScriptArgumentsDepth}`, { "event.action": "copy" });
	logger.debug(`copy(): res.isInterpolating → ${res.isInterpolating}`, { "event.action": "copy" });
	logger.debug(`copy(): res.innerMode → ${res.innerMode}`, { "event.action": "copy" });
    res.interpolationNesting = this.intpolationNesting;
	logger.debug(`copy(): res.interpolationNesting → ${res.interpolationNesting}`, { "event.action": "copy" });

    res.jsState = CodeMirror.copyState(jsMode, this.jsState);

    res.innerMode = this.innerMode;
    if (this.innerMode && this.innerState) {
      res.innerState = CodeMirror.copyState(this.innerMode, this.innerState);
    }

	logger.debug(`copy(): res.isIncludeFiltered → ${res.isIncludeFiltered}`, { "event.action": "copy" });
	logger.debug(`copy(): res.isEach → ${res.isEach}`, { "event.action": "copy" });
	logger.debug(`copy(): res.lastTag → ${res.lastTag}`, { "event.action": "copy" });
	logger.debug(`copy(): res.scriptType → ${res.scriptType}`, { "event.action": "copy" });
	logger.debug(`copy(): res.isAttrs → ${res.isAttrs}`, { "event.action": "copy" });
	logger.debug(`copy(): res.inAttributeName → ${res.inAttributeName}`, { "event.action": "copy" });
	logger.debug(`copy(): res.attributeIsType → ${res.attributeIsType}`, { "event.action": "copy" });
	logger.debug(`copy(): res.attrValue → ${res.attrValue}`, { "event.action": "copy" });
	logger.debug(`copy(): res.indentOf → ${res.indentOf}`, { "event.action": "copy" });
	logger.debug(`copy(): res.indentToken → ${res.indentToken}`, { "event.action": "copy" });
	logger.debug(`copy(): res.innerModeForLine → ${res.innerModeForLine}`, { "event.action": "copy" });
    res.restOfLine = this.restOfLine;
	logger.debug(`copy(): res.restOfLine → ${res.restOfLine}`, { "event.action": "copy" });

    res.isIncludeFiltered = this.isIncludeFiltered;
    res.isEach = this.isEach;
    res.lastTag = this.lastTag;
    res.scriptType = this.scriptType;
    res.isAttrs = this.isAttrs;
    res.attrsNest = this.attrsNest.slice();
    res.inAttributeName = this.inAttributeName;
    res.attributeIsType = this.attributeIsType;
    res.attrValue = this.attrValue;
    res.indentOf = this.indentOf;
    res.indentToken = this.indentToken;

    res.innerModeForLine = this.innerModeForLine;

	logger.warn('<<< Exiting copy()', { "event.action": "copy" });
    return res;
  };

  function javaScript(stream, state) {
	logger.error(`>>> Entering javaScript(stream=${stream},state=${state})`, { "event.action": "javaScript", "logrite.stream": stream, "logrite.state": state });
    if (stream.sol()) {
      // if javaScriptLine was set at end of line, ignore it
      state.javaScriptLine = false;
	logger.debug(`javaScript(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`, { "event.action": "javaScript", "logrite.stream": stream, "logrite.state": state });
      state.javaScriptLineExcludesColon = false;
	logger.debug(`javaScript(stream=${stream},state=${state}): state.javaScriptLineExcludesColon → ${state.javaScriptLineExcludesColon}`, { "event.action": "javaScript", "logrite.stream": stream, "logrite.state": state });
    }
    if (state.javaScriptLine) {
      if (state.javaScriptLineExcludesColon && stream.peek() === ':') {
        state.javaScriptLine = false;
        state.javaScriptLineExcludesColon = false;
        return;
      }
      var tok = jsMode.token(stream, state.jsState);
	logger.error(`javaScript(stream=${stream},state=${state}): tok → ${tok}`, { "event.action": "javaScript", "logrite.stream": stream, "logrite.state": state });
      if (stream.eol()) state.javaScriptLine = false;
	logger.warn(`<<< Exiting javaScript(stream=${stream},state=${state})`, { "event.action": "javaScript", "logrite.stream": stream, "logrite.state": state });
      return tok || true;
    }
  }
  function javaScriptArguments(stream, state) {
	logger.error(`>>> Entering javaScriptArguments(stream=${stream},state=${state})`, { "event.action": "javaScriptArguments", "logrite.stream": stream, "logrite.state": state });
    if (state.javaScriptArguments) {
      if (state.javaScriptArgumentsDepth === 0 && stream.peek() !== '(') {
        state.javaScriptArguments = false;
		logger.debug(`javaScriptArguments(stream=${stream},state=${state}): state.javaScriptArguments → ${state.javaScriptArguments}`, { "event.action": "javaScriptArguments", "logrite.stream": stream, "logrite.state": state });
        return;
      }
      if (stream.peek() === '(') {
        state.javaScriptArgumentsDepth++;
      } else if (stream.peek() === ')') {
        state.javaScriptArgumentsDepth--;
      }
      if (state.javaScriptArgumentsDepth === 0) {
        state.javaScriptArguments = false;
        return;
      }

      var tok = jsMode.token(stream, state.jsState);
	logger.error(`javaScriptArguments(stream=${stream},state=${state}): tok → ${tok}`, { "event.action": "javaScriptArguments", "logrite.stream": stream, "logrite.state": state });
      return tok || true;
    }
  }

  function yieldStatement(stream) {
	logger.error(`>>> Entering yieldStatement(stream=${stream})`, { "event.action": "yieldStatement", "logrite.stream": stream });
    if (stream.match(/^yield\b/)) {
		logger.warn(`<<< Exiting yieldStatement(stream=${stream})`, { "event.action": "yieldStatement", "logrite.stream": stream });
        return 'keyword';
    }
  }

  function doctype(stream) {
	logger.error(`>>> Entering doctype(stream=${stream})`, { "event.action": "doctype", "logrite.stream": stream });
    if (stream.match(/^(?:doctype) *([^\n]+)?/)) {
		logger.warn(`<<< Exiting doctype(stream=${stream})`, { "event.action": "doctype", "logrite.stream": stream });
        return DOCTYPE;
    }
  }

  function interpolation(stream, state) {
	logger.error(`>>> Entering interpolation(stream=${stream},state=${state})`, { "event.action": "interpolation", "logrite.stream": stream, "logrite.state": state });
    if (stream.match('#{')) {
      state.isInterpolating = true;
	logger.debug(`interpolation(stream=${stream},state=${state}): state.isInterpolating → ${state.isInterpolating}`, { "event.action": "interpolation", "logrite.stream": stream, "logrite.state": state });
      state.interpolationNesting = 0;
	logger.debug(`interpolation(stream=${stream},state=${state}): state.interpolationNesting → ${state.interpolationNesting}`, { "event.action": "interpolation", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting interpolation(stream=${stream},state=${state})`, { "event.action": "interpolation", "logrite.stream": stream, "logrite.state": state });
      return 'punctuation';
    }
  }

  function interpolationContinued(stream, state) {
	logger.error(`>>> Entering interpolationContinued(stream=${stream},state=${state})`, { "event.action": "interpolationContinued", "logrite.stream": stream, "logrite.state": state });
    if (state.isInterpolating) {
      if (stream.peek() === '}') {
        state.interpolationNesting--;
        if (state.interpolationNesting < 0) {
          stream.next();
          state.isInterpolating = false;
		logger.debug(`interpolationContinued(stream=${stream},state=${state}): state.isInterpolating → ${state.isInterpolating}`, { "event.action": "interpolationContinued", "logrite.stream": stream, "logrite.state": state });
          return 'puncutation';
        }
      } else if (stream.peek() === '{') {
        state.interpolationNesting++;
      }
	logger.warn(`<<< Exiting interpolationContinued(stream=${stream},state=${state})`, { "event.action": "interpolationContinued", "logrite.stream": stream, "logrite.state": state });
      return jsMode.token(stream, state.jsState) || true;
    }
  }

  function caseStatement(stream, state) {
	logger.error(`>>> Entering caseStatement(stream=${stream},state=${state})`, { "event.action": "caseStatement", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^case\b/)) {
      state.javaScriptLine = true;
	logger.debug(`caseStatement(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`, { "event.action": "caseStatement", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting caseStatement(stream=${stream},state=${state})`, { "event.action": "caseStatement", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }

  function when(stream, state) {
	logger.error(`>>> Entering when(stream=${stream},state=${state})`, { "event.action": "when", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^when\b/)) {
      state.javaScriptLine = true;
	logger.debug(`when(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`, { "event.action": "when", "logrite.stream": stream, "logrite.state": state });
      state.javaScriptLineExcludesColon = true;
	logger.debug(`when(stream=${stream},state=${state}): state.javaScriptLineExcludesColon → ${state.javaScriptLineExcludesColon}`, { "event.action": "when", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting when(stream=${stream},state=${state})`, { "event.action": "when", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }

  function defaultStatement(stream) {
	logger.error(`>>> Entering defaultStatement(stream=${stream})`, { "event.action": "defaultStatement", "logrite.stream": stream });
    if (stream.match(/^default\b/)) {
	logger.warn(`<<< Exiting defaultStatement(stream=${stream})`, { "event.action": "defaultStatement", "logrite.stream": stream });
      return KEYWORD;
    }
  }

  function extendsStatement(stream, state) {
	logger.error(`>>> Entering extendsStatement(stream=${stream},state=${state})`, { "event.action": "extendsStatement", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^extends?\b/)) {
      state.restOfLine = 'string';
	logger.debug(`extendsStatement(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`, { "event.action": "extendsStatement", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting extendsStatement(stream=${stream},state=${state})`, { "event.action": "extendsStatement", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }

  function append(stream, state) {
	logger.error(`>>> Entering append(stream=${stream},state=${state})`, { "event.action": "append", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^append\b/)) {
      state.restOfLine = 'variable';
	logger.debug(`append(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`, { "event.action": "append", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting append(stream=${stream},state=${state})`, { "event.action": "append", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }
  function prepend(stream, state) {
	logger.error(`>>> Entering prepend(stream=${stream},state=${state})`, { "event.action": "prepend", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^prepend\b/)) {
      state.restOfLine = 'variable';
	logger.debug(`prepend(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`, { "event.action": "prepend", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting prepend(stream=${stream},state=${state})`, { "event.action": "prepend", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }
  function block(stream, state) {
	logger.error(`>>> Entering block(stream=${stream},state=${state})`, { "event.action": "block", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^block\b *(?:(prepend|append)\b)?/)) {
      state.restOfLine = 'variable';
	logger.debug(`block(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`, { "event.action": "block", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting block(stream=${stream},state=${state})`, { "event.action": "block", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }

  function include(stream, state) {
	logger.error(`>>> Entering include(stream=${stream},state=${state})`, { "event.action": "include", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^include\b/)) {
      state.restOfLine = 'string';
	logger.debug(`include(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`, { "event.action": "include", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting include(stream=${stream},state=${state})`, { "event.action": "include", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }

  function includeFiltered(stream, state) {
	logger.error(`>>> Entering includeFiltered(stream=${stream},state=${state})`, { "event.action": "includeFiltered", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^include:([a-zA-Z0-9\-]+)/, false) && stream.match('include')) {
      state.isIncludeFiltered = true;
	logger.debug(`includeFiltered(stream=${stream},state=${state}): state.isIncludeFiltered → ${state.isIncludeFiltered}`, { "event.action": "includeFiltered", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting includeFiltered(stream=${stream},state=${state})`, { "event.action": "includeFiltered", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }

  function includeFilteredContinued(stream, state) {
	logger.error(`>>> Entering includeFilteredContinued(stream=${stream},state=${state})`, { "event.action": "includeFilteredContinued", "logrite.stream": stream, "logrite.state": state });
    if (state.isIncludeFiltered) {
      var tok = filter(stream, state);
	logger.error(`includeFilteredContinued(stream=${stream},state=${state}): tok → ${tok}`, { "event.action": "includeFilteredContinued", "logrite.stream": stream, "logrite.state": state });
      state.isIncludeFiltered = false;
      state.restOfLine = 'string';
	logger.debug(`includeFilteredContinued(stream=${stream},state=${state}): state.isIncludeFiltered → ${state.isIncludeFiltered}`, { "event.action": "includeFilteredContinued", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting includeFilteredContinued(stream=${stream},state=${state})`, { "event.action": "includeFilteredContinued", "logrite.stream": stream, "logrite.state": state });
      return tok;
    }
  }

  function mixin(stream, state) {
	logger.error(`>>> Entering mixin(stream=${stream},state=${state})`, { "event.action": "mixin", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^mixin\b/)) {
      state.javaScriptLine = true;
	logger.debug(`mixin(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`, { "event.action": "mixin", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting mixin(stream=${stream},state=${state})`, { "event.action": "mixin", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }

  function call(stream, state) {
	logger.error(`>>> Entering call(stream=${stream},state=${state})`, { "event.action": "call", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^\+([-\w]+)/)) {
      if (!stream.match(/^\( *[-\w]+ *=/, false)) {
        state.javaScriptArguments = true;
		logger.debug(`call(stream=${stream},state=${state}): state.javaScriptArguments → ${state.javaScriptArguments}`, { "event.action": "call", "logrite.stream": stream, "logrite.state": state });
        state.javaScriptArgumentsDepth = 0;
		logger.debug(`call(stream=${stream},state=${state}): state.javaScriptArgumentsDepth → ${state.javaScriptArgumentsDepth}`, { "event.action": "call", "logrite.stream": stream, "logrite.state": state });
      }
      return 'variable';
    }
    if (stream.match(/^\+#{/, false)) {
      stream.next();
      state.mixinCallAfter = true;
	logger.debug(`call(stream=${stream},state=${state}): state.mixinCallAfter → ${state.mixinCallAfter}`, { "event.action": "call", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting call(stream=${stream},state=${state})`, { "event.action": "call", "logrite.stream": stream, "logrite.state": state });
      return interpolation(stream, state);
    }
  }
  function callArguments(stream, state) {
	logger.error(`>>> Entering callArguments(stream=${stream},state=${state})`, { "event.action": "callArguments", "logrite.stream": stream, "logrite.state": state });
    if (state.mixinCallAfter) {
      state.mixinCallAfter = false;
	logger.debug(`callArguments(stream=${stream},state=${state}): state.mixinCallAfter → ${state.mixinCallAfter}`, { "event.action": "callArguments", "logrite.stream": stream, "logrite.state": state });
      if (!stream.match(/^\( *[-\w]+ *=/, false)) {
        state.javaScriptArguments = true;
		logger.debug(`callArguments(stream=${stream},state=${state}): state.javaScriptArguments → ${state.javaScriptArguments}`, { "event.action": "callArguments", "logrite.stream": stream, "logrite.state": state });
        state.javaScriptArgumentsDepth = 0;
		logger.debug(`callArguments(stream=${stream},state=${state}): state.javaScriptArgumentsDepth → ${state.javaScriptArgumentsDepth}`, { "event.action": "callArguments", "logrite.stream": stream, "logrite.state": state });
      }
	logger.warn(`<<< Exiting callArguments(stream=${stream},state=${state})`, { "event.action": "callArguments", "logrite.stream": stream, "logrite.state": state });
      return true;
    }
  }

  function conditional(stream, state) {
	logger.error(`>>> Entering conditional(stream=${stream},state=${state})`, { "event.action": "conditional", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^(if|unless|else if|else)\b/)) {
      state.javaScriptLine = true;
	logger.debug(`conditional(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`, { "event.action": "conditional", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting conditional(stream=${stream},state=${state})`, { "event.action": "conditional", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }

  function each(stream, state) {
	logger.error(`>>> Entering each(stream=${stream},state=${state})`, { "event.action": "each", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^(- *)?(each|for)\b/)) {
      state.isEach = true;
	logger.debug(`each(stream=${stream},state=${state}): state.isEach → ${state.isEach}`, { "event.action": "each", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting each(stream=${stream},state=${state})`, { "event.action": "each", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }
  function eachContinued(stream, state) {
	logger.error(`>>> Entering eachContinued(stream=${stream},state=${state})`, { "event.action": "eachContinued", "logrite.stream": stream, "logrite.state": state });
    if (state.isEach) {
      if (stream.match(/^ in\b/)) {
        state.javaScriptLine = true;
		logger.debug(`eachContinued(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`, { "event.action": "eachContinued", "logrite.stream": stream, "logrite.state": state });
        state.isEach = false;
		logger.debug(`eachContinued(stream=${stream},state=${state}): state.isEach → ${state.isEach}`, { "event.action": "eachContinued", "logrite.stream": stream, "logrite.state": state });
        return KEYWORD;
      } else if (stream.sol() || stream.eol()) {
        state.isEach = false;
      } else if (stream.next()) {
        while (!stream.match(/^ in\b/, false) && stream.next());
		logger.warn(`<<< Exiting eachContinued(stream=${stream},state=${state})`, { "event.action": "eachContinued", "logrite.stream": stream, "logrite.state": state });
        return 'variable';
      }
    }
  }

  function whileStatement(stream, state) {
	logger.error(`>>> Entering whileStatement(stream=${stream},state=${state})`, { "event.action": "whileStatement", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^while\b/)) {
      state.javaScriptLine = true;
	logger.debug(`whileStatement(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`, { "event.action": "whileStatement", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting whileStatement(stream=${stream},state=${state})`, { "event.action": "whileStatement", "logrite.stream": stream, "logrite.state": state });
      return KEYWORD;
    }
  }

  function tag(stream, state) {
	logger.error(`>>> Entering tag(stream=${stream},state=${state})`, { "event.action": "tag", "logrite.stream": stream, "logrite.state": state });
    var captures;
    if (captures = stream.match(/^(\w(?:[-:\w]*\w)?)\/?/)) {
      state.lastTag = captures[1].toLowerCase();
      if (state.lastTag === 'script') {
        state.scriptType = 'application/javascript';
		logger.debug(`tag(stream=${stream},state=${state}): state.scriptType → ${state.scriptType}`, { "event.action": "tag", "logrite.stream": stream, "logrite.state": state });
      }
	logger.warn(`<<< Exiting tag(stream=${stream},state=${state})`, { "event.action": "tag", "logrite.stream": stream, "logrite.state": state });
      return 'tag';
    }
  }

  function filter(stream, state) {
	logger.error(`>>> Entering filter(stream=${stream},state=${state})`, { "event.action": "filter", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^:([\w\-]+)/)) {
      var innerMode;
      if (config && config.innerModes) {
        innerMode = config.innerModes(stream.current().substring(1));
      }
      if (!innerMode) {
        innerMode = stream.current().substring(1);
      }
      if (typeof innerMode === 'string') {
        innerMode = CodeMirror.getMode(config, innerMode);
      }
      setInnerMode(stream, state, innerMode);
	logger.warn(`<<< Exiting filter(stream=${stream},state=${state})`, { "event.action": "filter", "logrite.stream": stream, "logrite.state": state });
      return 'atom';
    }
  }

  function code(stream, state) {
	logger.error(`>>> Entering code(stream=${stream},state=${state})`, { "event.action": "code", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^(!?=|-)/)) {
      state.javaScriptLine = true;
	logger.debug(`code(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`, { "event.action": "code", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting code(stream=${stream},state=${state})`, { "event.action": "code", "logrite.stream": stream, "logrite.state": state });
      return 'punctuation';
    }
  }

  function id(stream) {
	logger.error(`>>> Entering id(stream=${stream})`, { "event.action": "id", "logrite.stream": stream });
    if (stream.match(/^#([\w-]+)/)) {
	logger.warn(`<<< Exiting id(stream=${stream})`, { "event.action": "id", "logrite.stream": stream });
      return ID;
    }
  }

  function className(stream) {
	logger.error(`>>> Entering className(stream=${stream})`, { "event.action": "className", "logrite.stream": stream });
    if (stream.match(/^\.([\w-]+)/)) {
	logger.warn(`<<< Exiting className(stream=${stream})`, { "event.action": "className", "logrite.stream": stream });
      return CLASS;
    }
  }

  function attrs(stream, state) {
	logger.error(`>>> Entering attrs(stream=${stream},state=${state})`, { "event.action": "attrs", "logrite.stream": stream, "logrite.state": state });
    if (stream.peek() == '(') {
      stream.next();
      state.isAttrs = true;
	logger.debug(`attrs(stream=${stream},state=${state}): state.isAttrs → ${state.isAttrs}`, { "event.action": "attrs", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`attrs(stream=${stream},state=${state}): state.inAttributeName → ${state.inAttributeName}`, { "event.action": "attrs", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`attrs(stream=${stream},state=${state}): state.attrValue → ${state.attrValue}`, { "event.action": "attrs", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`attrs(stream=${stream},state=${state}): state.attributeIsType → ${state.attributeIsType}`, { "event.action": "attrs", "logrite.stream": stream, "logrite.state": state });
      state.attrsNest = [];
	logger.debug(`attrs(stream=${stream},state=${state}): state.attrsNest → ${state.attrsNest}`, { "event.action": "attrs", "logrite.stream": stream, "logrite.state": state });
      state.inAttributeName = true;
      state.attrValue = '';
      state.attributeIsType = false;
	logger.warn(`<<< Exiting attrs(stream=${stream},state=${state})`, { "event.action": "attrs", "logrite.stream": stream, "logrite.state": state });
      return 'punctuation';
    }
  }

  function attrsContinued(stream, state) {
	logger.error(`>>> Entering attrsContinued(stream=${stream},state=${state})`, { "event.action": "attrsContinued", "logrite.stream": stream, "logrite.state": state });
    if (state.isAttrs) {
      if (ATTRS_NEST[stream.peek()]) {
        state.attrsNest.push(ATTRS_NEST[stream.peek()]);
      }
      if (state.attrsNest[state.attrsNest.length - 1] === stream.peek()) {
        state.attrsNest.pop();
      } else  if (stream.eat(')')) {
        state.isAttrs = false;
		logger.debug(`attrsContinued(stream=${stream},state=${state}): state.isAttrs → ${state.isAttrs}`, { "event.action": "attrsContinued", "logrite.stream": stream, "logrite.state": state });
        return 'punctuation';
      }
      if (state.inAttributeName && stream.match(/^[^=,\)!]+/)) {
        if (stream.peek() === '=' || stream.peek() === '!') {
          state.inAttributeName = false;
		logger.debug(`attrsContinued(stream=${stream},state=${state}): state.inAttributeName → ${state.inAttributeName}`, { "event.action": "attrsContinued", "logrite.stream": stream, "logrite.state": state });
          state.jsState = jsMode.startState();
          if (state.lastTag === 'script' && stream.current().trim().toLowerCase() === 'type') {
            state.attributeIsType = true;
          } else {
            state.attributeIsType = false;
          }
        }
        return 'attribute';
      }

      var tok = jsMode.token(stream, state.jsState);
	logger.error(`attrsContinued(stream=${stream},state=${state}): tok → ${tok}`, { "event.action": "attrsContinued", "logrite.stream": stream, "logrite.state": state });
      if (state.attributeIsType && tok === 'string') {
        state.scriptType = stream.current().toString();
      }
      if (state.attrsNest.length === 0 && (tok === 'string' || tok === 'variable' || tok === 'keyword')) {
        try {
          Function('', 'var x ' + state.attrValue.replace(/,\s*$/, '').replace(/^!/, ''));
          state.inAttributeName = true;
          state.attrValue = '';
		logger.debug(`attrsContinued(stream=${stream},state=${state}): state.attrValue → ${state.attrValue}`, { "event.action": "attrsContinued", "logrite.stream": stream, "logrite.state": state });
          stream.backUp(stream.current().length);
          return attrsContinued(stream, state);
        } catch (ex) {
		logger.error(`Exception in attrsContinued(stream=${stream},state=${state}): ${ex.message}`, ex, { "event.action": "attrsContinued", "event.category": "process", "logrite.stream": stream, "logrite.state": state });
          //not the end of an attribute
        }
      }
      state.attrValue += stream.current();
	logger.warn(`<<< Exiting attrsContinued(stream=${stream},state=${state})`, { "event.action": "attrsContinued", "logrite.stream": stream, "logrite.state": state });
      return tok || true;
    }
  }

  function attributesBlock(stream, state) {
	logger.error(`>>> Entering attributesBlock(stream=${stream},state=${state})`, { "event.action": "attributesBlock", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^&attributes\b/)) {
      state.javaScriptArguments = true;
	logger.debug(`attributesBlock(stream=${stream},state=${state}): state.javaScriptArguments → ${state.javaScriptArguments}`, { "event.action": "attributesBlock", "logrite.stream": stream, "logrite.state": state });
      state.javaScriptArgumentsDepth = 0;
	logger.debug(`attributesBlock(stream=${stream},state=${state}): state.javaScriptArgumentsDepth → ${state.javaScriptArgumentsDepth}`, { "event.action": "attributesBlock", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting attributesBlock(stream=${stream},state=${state})`, { "event.action": "attributesBlock", "logrite.stream": stream, "logrite.state": state });
      return 'keyword';
    }
  }

  function indent(stream) {
	logger.error(`>>> Entering indent(stream=${stream})`, { "event.action": "indent", "logrite.stream": stream });
    if (stream.sol() && stream.eatSpace()) {
	logger.warn(`<<< Exiting indent(stream=${stream})`, { "event.action": "indent", "logrite.stream": stream });
      return 'indent';
    }
  }

  function comment(stream, state) {
	logger.error(`>>> Entering comment(stream=${stream},state=${state})`, { "event.action": "comment", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^ *\/\/(-)?([^\n]*)/)) {
      state.indentOf = stream.indentation();
      state.indentToken = 'comment';
	logger.debug(`comment(stream=${stream},state=${state}): state.indentToken → ${state.indentToken}`, { "event.action": "comment", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting comment(stream=${stream},state=${state})`, { "event.action": "comment", "logrite.stream": stream, "logrite.state": state });
      return 'comment';
    }
  }

  function colon(stream) {
	logger.error(`>>> Entering colon(stream=${stream})`, { "event.action": "colon", "logrite.stream": stream });
    if (stream.match(/^: */)) {
	logger.warn(`<<< Exiting colon(stream=${stream})`, { "event.action": "colon", "logrite.stream": stream });
      return 'colon';
    }
  }

  function text(stream, state) {
	logger.error(`>>> Entering text(stream=${stream},state=${state})`, { "event.action": "text", "logrite.stream": stream, "logrite.state": state });
    if (stream.match(/^(?:\| ?| )([^\n]+)/)) {
      return 'string';
    }
    if (stream.match(/^(<[^\n]*)/, false)) {
      // html string
      setInnerMode(stream, state, 'htmlmixed');
      state.innerModeForLine = true;
	logger.debug(`text(stream=${stream},state=${state}): state.innerModeForLine → ${state.innerModeForLine}`, { "event.action": "text", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting text(stream=${stream},state=${state})`, { "event.action": "text", "logrite.stream": stream, "logrite.state": state });
      return innerMode(stream, state, true);
    }
  }

  function dot(stream, state) {
	logger.error(`>>> Entering dot(stream=${stream},state=${state})`, { "event.action": "dot", "logrite.stream": stream, "logrite.state": state });
    if (stream.eat('.')) {
      var innerMode = null;
      if (state.lastTag === 'script' && state.scriptType.toLowerCase().indexOf('javascript') != -1) {
        innerMode = state.scriptType.toLowerCase().replace(/"|'/g, '');
      } else if (state.lastTag === 'style') {
        innerMode = 'css';
      }
      setInnerMode(stream, state, innerMode);
	logger.warn(`<<< Exiting dot(stream=${stream},state=${state})`, { "event.action": "dot", "logrite.stream": stream, "logrite.state": state });
      return 'dot';
    }
  }

  function fail(stream) {
	logger.error(`>>> Entering fail(stream=${stream})`, { "event.action": "fail", "logrite.stream": stream });
    stream.next();
	logger.warn(`<<< Exiting fail(stream=${stream})`, { "event.action": "fail", "logrite.stream": stream });
    return null;
  }


  function setInnerMode(stream, state, mode) {
	logger.error(`>>> Entering setInnerMode(stream=${stream},state=${state},mode=${mode})`, { "event.action": "setInnerMode", "logrite.stream": stream, "logrite.state": state, "logrite.mode": mode });
    mode = CodeMirror.mimeModes[mode] || mode;
    mode = config.innerModes ? config.innerModes(mode) || mode : mode;
    mode = CodeMirror.mimeModes[mode] || mode;
    mode = CodeMirror.getMode(config, mode);
    state.indentOf = stream.indentation();
	logger.error(`setInnerMode(stream=${stream},state=${state},mode=${mode}): mode → ${mode}`, { "event.action": "setInnerMode", "logrite.stream": stream, "logrite.state": state, "logrite.mode": mode });

    if (mode && mode.name !== 'null') {
      state.innerMode = mode;
    } else {
      state.indentToken = 'string';
	logger.warn(`<<< Exiting setInnerMode(stream=${stream},state=${state},mode=${mode})`, { "event.action": "setInnerMode", "logrite.stream": stream, "logrite.state": state, "logrite.mode": mode });
    }
  }
  function innerMode(stream, state, force) {
	logger.error(`>>> Entering innerMode(stream=${stream},state=${state},force=${force})`, { "event.action": "innerMode", "logrite.stream": stream, "logrite.state": state, "logrite.force": force });
    if (stream.indentation() > state.indentOf || (state.innerModeForLine && !stream.sol()) || force) {
      if (state.innerMode) {
        if (!state.innerState) {
          state.innerState = state.innerMode.startState ? state.innerMode.startState(stream.indentation()) : {};
        }
        return stream.hideFirstChars(state.indentOf + 2, function () {
          return state.innerMode.token(stream, state.innerState) || true;
        });
      } else {
        stream.skipToEnd();
		logger.warn(`<<< Exiting innerMode(stream=${stream},state=${state},force=${force})`, { "event.action": "innerMode", "logrite.stream": stream, "logrite.state": state, "logrite.force": force });
        return state.indentToken;
      }
    } else if (stream.sol()) {
      state.indentOf = Infinity;
	logger.debug(`innerMode(stream=${stream},state=${state},force=${force}): state.indentOf → ${state.indentOf}`, { "event.action": "innerMode", "logrite.stream": stream, "logrite.state": state, "logrite.force": force });
	logger.debug(`innerMode(stream=${stream},state=${state},force=${force}): state.innerMode → ${state.innerMode}`, { "event.action": "innerMode", "logrite.stream": stream, "logrite.state": state, "logrite.force": force });
	logger.debug(`innerMode(stream=${stream},state=${state},force=${force}): state.innerState → ${state.innerState}`, { "event.action": "innerMode", "logrite.stream": stream, "logrite.state": state, "logrite.force": force });
      state.indentToken = null;
	logger.debug(`innerMode(stream=${stream},state=${state},force=${force}): state.indentToken → ${state.indentToken}`, { "event.action": "innerMode", "logrite.stream": stream, "logrite.state": state, "logrite.force": force });
      state.innerMode = null;
      state.innerState = null;
    }
  }
  function restOfLine(stream, state) {
	logger.error(`>>> Entering restOfLine(stream=${stream},state=${state})`, { "event.action": "restOfLine", "logrite.stream": stream, "logrite.state": state });
    if (stream.sol()) {
      // if restOfLine was set at end of line, ignore it
      state.restOfLine = '';
	logger.debug(`restOfLine(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`, { "event.action": "restOfLine", "logrite.stream": stream, "logrite.state": state });
    }
    if (state.restOfLine) {
      stream.skipToEnd();
      var tok = state.restOfLine;
      state.restOfLine = '';
	logger.warn(`<<< Exiting restOfLine(stream=${stream},state=${state})`, { "event.action": "restOfLine", "logrite.stream": stream, "logrite.state": state });
      return tok;
    }
  }


  function startState() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    return new State();
  }
  function copyState(state) {
	logger.error(`>>> Entering copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
	logger.warn(`<<< Exiting copyState(state=${state})`, { "event.action": "copyState", "logrite.state": state });
    return state.copy();
  }
  /**
   * Get the next token in the stream
   *
   * @param {Stream} stream
   * @param {State} state
   */
  function nextToken(stream, state) {
	logger.error(`>>> Entering nextToken(stream=${stream},state=${state})`, { "event.action": "nextToken", "logrite.stream": stream, "logrite.state": state });
    var tok = innerMode(stream, state)
      || restOfLine(stream, state)
      || interpolationContinued(stream, state)
      || includeFilteredContinued(stream, state)
      || eachContinued(stream, state)
      || attrsContinued(stream, state)
      || javaScript(stream, state)
      || javaScriptArguments(stream, state)
      || callArguments(stream, state)

      || yieldStatement(stream, state)
      || doctype(stream, state)
      || interpolation(stream, state)
      || caseStatement(stream, state)
      || when(stream, state)
      || defaultStatement(stream, state)
      || extendsStatement(stream, state)
      || append(stream, state)
      || prepend(stream, state)
      || block(stream, state)
      || include(stream, state)
      || includeFiltered(stream, state)
      || mixin(stream, state)
      || call(stream, state)
      || conditional(stream, state)
      || each(stream, state)
      || whileStatement(stream, state)
      || tag(stream, state)
      || filter(stream, state)
      || code(stream, state)
      || id(stream, state)
      || className(stream, state)
      || attrs(stream, state)
      || attributesBlock(stream, state)
      || indent(stream, state)
      || text(stream, state)
      || comment(stream, state)
      || colon(stream, state)
      || dot(stream, state)
      || fail(stream, state);

	logger.error(`nextToken(stream=${stream},state=${state}): tok → ${tok}`, { "event.action": "nextToken", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting nextToken(stream=${stream},state=${state})`, { "event.action": "nextToken", "logrite.stream": stream, "logrite.state": state });
    return tok === true ? null : tok;
  }
  return {
    startState: startState,
    copyState: copyState,
    token: nextToken
  };
});

CodeMirror.defineMIME('text/x-jade', 'jade');

});
