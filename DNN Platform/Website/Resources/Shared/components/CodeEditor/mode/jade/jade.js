const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../javascript/javascript"), require("../css/css"), require("../htmlmixed/htmlmixed"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../javascript/javascript", "../css/css", "../htmlmixed/htmlmixed"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

logger.debug(`<<< Exiting function(exports=${exports})`);
CodeMirror.defineMode('jade', function (config) {
logger.debug('>>> Entering defineMode()');
  // token types
  var KEYWORD = 'keyword';
logger.debug(`defineMode(): KEYWORD → ${KEYWORD}`);
  var DOCTYPE = 'meta';
logger.debug(`defineMode(): DOCTYPE → ${DOCTYPE}`);
  var ID = 'builtin';
logger.debug(`defineMode(): ID → ${ID}`);
  var CLASS = 'qualifier';

  var ATTRS_NEST = {
    '{': '}',
    '(': ')',
    '[': ']'
  };

  var jsMode = CodeMirror.getMode(config, 'javascript');
logger.debug('<<< Exiting defineMode()');
logger.debug(`defineMode(): jsMode → ${jsMode}`);

  function State() {
	logger.debug('>>> Entering State()');
    this.javaScriptLine = false;
    this.javaScriptLineExcludesColon = false;

	logger.debug(`State(): this.javaScriptLine → ${this.javaScriptLine}`);
    this.javaScriptArguments = false;
    this.javaScriptArgumentsDepth = 0;
	logger.debug(`State(): this.javaScriptLineExcludesColon → ${this.javaScriptLineExcludesColon}`);

    this.isInterpolating = false;
	logger.debug(`State(): this.javaScriptArguments → ${this.javaScriptArguments}`);
    this.interpolationNesting = 0;

	logger.debug(`State(): this.javaScriptArgumentsDepth → ${this.javaScriptArgumentsDepth}`);
    this.jsState = jsMode.startState();

    this.restOfLine = '';
	logger.debug(`State(): this.isInterpolating → ${this.isInterpolating}`);

    this.isIncludeFiltered = false;
    this.isEach = false;
	logger.debug(`State(): this.interpolationNesting → ${this.interpolationNesting}`);

    this.lastTag = '';
    this.scriptType = '';

    // Attributes Mode
    this.isAttrs = false;
	logger.debug(`State(): this.restOfLine → ${this.restOfLine}`);
	logger.debug(`State(): this.isIncludeFiltered → ${this.isIncludeFiltered}`);
    this.attrsNest = [];
    this.inAttributeName = true;
    this.attributeIsType = false;
	logger.debug(`State(): this.isEach → ${this.isEach}`);
    this.attrValue = '';

    // Indented Mode
    this.indentOf = Infinity;
	logger.debug(`State(): this.lastTag → ${this.lastTag}`);
	logger.debug(`State(): this.scriptType → ${this.scriptType}`);
    this.indentToken = '';

    this.innerMode = null;
    this.innerState = null;
	logger.debug(`State(): this.isAttrs → ${this.isAttrs}`);

    this.innerModeForLine = false;
	logger.debug(`State(): this.innerModeForLine → ${this.innerModeForLine}`);
	logger.debug('<<< Exiting State()');
  }
  /**
   * Safely copy a state
   *
   * @return {State}
   */
  State.prototype.copy = function () {
    var res = new State();
    res.javaScriptLine = this.javaScriptLine;
	logger.debug(`function(): res → ${res}`);
    res.javaScriptLineExcludesColon = this.javaScriptLineExcludesColon;
    res.javaScriptArguments = this.javaScriptArguments;
    res.javaScriptArgumentsDepth = this.javaScriptArgumentsDepth;
    res.isInterpolating = this.isInterpolating;
    res.interpolationNesting = this.intpolationNesting;

    res.jsState = CodeMirror.copyState(jsMode, this.jsState);

    res.innerMode = this.innerMode;
	logger.debug(`function(): res.innerMode → ${res.innerMode}`);
	logger.debug(`function(): res.javaScriptLine → ${res.javaScriptLine}`);
	logger.debug(`function(): res.javaScriptLineExcludesColon → ${res.javaScriptLineExcludesColon}`);
    if (this.innerMode && this.innerState) {
      res.innerState = CodeMirror.copyState(this.innerMode, this.innerState);
    }

	logger.debug(`function(): res.javaScriptArguments → ${res.javaScriptArguments}`);
	logger.debug(`function(): res.javaScriptArgumentsDepth → ${res.javaScriptArgumentsDepth}`);
	logger.debug(`function(): res.isInterpolating → ${res.isInterpolating}`);
	logger.debug(`function(): res.isIncludeFiltered → ${res.isIncludeFiltered}`);
	logger.debug(`function(): res.isEach → ${res.isEach}`);
	logger.debug(`function(): res.lastTag → ${res.lastTag}`);
	logger.debug(`function(): res.scriptType → ${res.scriptType}`);
	logger.debug(`function(): res.isAttrs → ${res.isAttrs}`);
	logger.debug(`function(): res.interpolationNesting → ${res.interpolationNesting}`);
	logger.debug(`function(): res.inAttributeName → ${res.inAttributeName}`);
	logger.debug(`function(): res.attributeIsType → ${res.attributeIsType}`);
	logger.debug(`function(): res.attrValue → ${res.attrValue}`);
	logger.debug(`function(): res.indentOf → ${res.indentOf}`);
	logger.debug(`function(): res.indentToken → ${res.indentToken}`);
	logger.debug(`function(): res.innerModeForLine → ${res.innerModeForLine}`);
    res.restOfLine = this.restOfLine;
	logger.debug(`function(): res.restOfLine → ${res.restOfLine}`);

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

	logger.debug('<<< Exiting function()');
    return res;
  };

  function javaScript(stream, state) {
	logger.debug(`>>> Entering javaScript(stream=${stream},state=${state})`);
    if (stream.sol()) {
      // if javaScriptLine was set at end of line, ignore it
      state.javaScriptLine = false;
	logger.debug(`javaScript(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`);
      state.javaScriptLineExcludesColon = false;
	logger.debug(`javaScript(stream=${stream},state=${state}): state.javaScriptLineExcludesColon → ${state.javaScriptLineExcludesColon}`);
    }
    if (state.javaScriptLine) {
      if (state.javaScriptLineExcludesColon && stream.peek() === ':') {
        state.javaScriptLine = false;
        state.javaScriptLineExcludesColon = false;
        return;
      }
      var tok = jsMode.token(stream, state.jsState);
	logger.debug(`javaScript(stream=${stream},state=${state}): tok → ${tok}`);
      if (stream.eol()) state.javaScriptLine = false;
	logger.debug(`<<< Exiting javaScript(stream=${stream},state=${state})`);
      return tok || true;
    }
  }
  function javaScriptArguments(stream, state) {
	logger.debug(`>>> Entering javaScriptArguments(stream=${stream},state=${state})`);
    if (state.javaScriptArguments) {
      if (state.javaScriptArgumentsDepth === 0 && stream.peek() !== '(') {
        state.javaScriptArguments = false;
		logger.debug(`javaScriptArguments(stream=${stream},state=${state}): state.javaScriptArguments → ${state.javaScriptArguments}`);
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
	logger.debug(`javaScriptArguments(stream=${stream},state=${state}): tok → ${tok}`);
	logger.debug(`<<< Exiting javaScriptArguments(stream=${stream},state=${state})`);
      return tok || true;
    }
  }

  function yieldStatement(stream) {
	logger.debug(`>>> Entering yieldStatement(stream=${stream})`);
    if (stream.match(/^yield\b/)) {
		logger.debug(`<<< Exiting yieldStatement(stream=${stream})`);
        return 'keyword';
    }
  }

  function doctype(stream) {
	logger.debug(`>>> Entering doctype(stream=${stream})`);
    if (stream.match(/^(?:doctype) *([^\n]+)?/)) {
		logger.debug(`<<< Exiting doctype(stream=${stream})`);
        return DOCTYPE;
    }
  }

  function interpolation(stream, state) {
	logger.debug(`>>> Entering interpolation(stream=${stream},state=${state})`);
    if (stream.match('#{')) {
      state.isInterpolating = true;
	logger.debug(`interpolation(stream=${stream},state=${state}): state.isInterpolating → ${state.isInterpolating}`);
      state.interpolationNesting = 0;
	logger.debug(`interpolation(stream=${stream},state=${state}): state.interpolationNesting → ${state.interpolationNesting}`);
	logger.debug(`<<< Exiting interpolation(stream=${stream},state=${state})`);
      return 'punctuation';
    }
  }

  function interpolationContinued(stream, state) {
	logger.debug(`>>> Entering interpolationContinued(stream=${stream},state=${state})`);
    if (state.isInterpolating) {
      if (stream.peek() === '}') {
        state.interpolationNesting--;
        if (state.interpolationNesting < 0) {
          stream.next();
          state.isInterpolating = false;
		logger.debug(`interpolationContinued(stream=${stream},state=${state}): state.isInterpolating → ${state.isInterpolating}`);
          return 'puncutation';
        }
      } else if (stream.peek() === '{') {
        state.interpolationNesting++;
      }
	logger.debug(`<<< Exiting interpolationContinued(stream=${stream},state=${state})`);
      return jsMode.token(stream, state.jsState) || true;
    }
  }

  function caseStatement(stream, state) {
	logger.debug(`>>> Entering caseStatement(stream=${stream},state=${state})`);
    if (stream.match(/^case\b/)) {
      state.javaScriptLine = true;
	logger.debug(`caseStatement(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`);
	logger.debug(`<<< Exiting caseStatement(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }

  function when(stream, state) {
	logger.debug(`>>> Entering when(stream=${stream},state=${state})`);
    if (stream.match(/^when\b/)) {
      state.javaScriptLine = true;
	logger.debug(`when(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`);
      state.javaScriptLineExcludesColon = true;
	logger.debug(`when(stream=${stream},state=${state}): state.javaScriptLineExcludesColon → ${state.javaScriptLineExcludesColon}`);
	logger.debug(`<<< Exiting when(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }

  function defaultStatement(stream) {
	logger.debug(`>>> Entering defaultStatement(stream=${stream})`);
    if (stream.match(/^default\b/)) {
	logger.debug(`<<< Exiting defaultStatement(stream=${stream})`);
      return KEYWORD;
    }
  }

  function extendsStatement(stream, state) {
	logger.debug(`>>> Entering extendsStatement(stream=${stream},state=${state})`);
    if (stream.match(/^extends?\b/)) {
      state.restOfLine = 'string';
	logger.debug(`extendsStatement(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`);
	logger.debug(`<<< Exiting extendsStatement(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }

  function append(stream, state) {
	logger.debug(`>>> Entering append(stream=${stream},state=${state})`);
    if (stream.match(/^append\b/)) {
      state.restOfLine = 'variable';
	logger.debug(`append(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`);
	logger.debug(`<<< Exiting append(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }
  function prepend(stream, state) {
	logger.debug(`>>> Entering prepend(stream=${stream},state=${state})`);
    if (stream.match(/^prepend\b/)) {
      state.restOfLine = 'variable';
	logger.debug(`prepend(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`);
	logger.debug(`<<< Exiting prepend(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }
  function block(stream, state) {
	logger.debug(`>>> Entering block(stream=${stream},state=${state})`);
    if (stream.match(/^block\b *(?:(prepend|append)\b)?/)) {
      state.restOfLine = 'variable';
	logger.debug(`block(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`);
	logger.debug(`<<< Exiting block(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }

  function include(stream, state) {
	logger.debug(`>>> Entering include(stream=${stream},state=${state})`);
    if (stream.match(/^include\b/)) {
      state.restOfLine = 'string';
	logger.debug(`include(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`);
	logger.debug(`<<< Exiting include(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }

  function includeFiltered(stream, state) {
	logger.debug(`>>> Entering includeFiltered(stream=${stream},state=${state})`);
    if (stream.match(/^include:([a-zA-Z0-9\-]+)/, false) && stream.match('include')) {
      state.isIncludeFiltered = true;
	logger.debug(`includeFiltered(stream=${stream},state=${state}): state.isIncludeFiltered → ${state.isIncludeFiltered}`);
	logger.debug(`<<< Exiting includeFiltered(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }

  function includeFilteredContinued(stream, state) {
	logger.debug(`>>> Entering includeFilteredContinued(stream=${stream},state=${state})`);
    if (state.isIncludeFiltered) {
      var tok = filter(stream, state);
	logger.debug(`includeFilteredContinued(stream=${stream},state=${state}): tok → ${tok}`);
      state.isIncludeFiltered = false;
      state.restOfLine = 'string';
	logger.debug(`includeFilteredContinued(stream=${stream},state=${state}): state.isIncludeFiltered → ${state.isIncludeFiltered}`);
	logger.debug(`includeFilteredContinued(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`);
	logger.debug(`<<< Exiting includeFilteredContinued(stream=${stream},state=${state})`);
      return tok;
    }
  }

  function mixin(stream, state) {
	logger.debug(`>>> Entering mixin(stream=${stream},state=${state})`);
    if (stream.match(/^mixin\b/)) {
      state.javaScriptLine = true;
	logger.debug(`mixin(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`);
	logger.debug(`<<< Exiting mixin(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }

  function call(stream, state) {
	logger.debug(`>>> Entering call(stream=${stream},state=${state})`);
    if (stream.match(/^\+([-\w]+)/)) {
      if (!stream.match(/^\( *[-\w]+ *=/, false)) {
        state.javaScriptArguments = true;
		logger.debug(`call(stream=${stream},state=${state}): state.javaScriptArguments → ${state.javaScriptArguments}`);
        state.javaScriptArgumentsDepth = 0;
		logger.debug(`call(stream=${stream},state=${state}): state.javaScriptArgumentsDepth → ${state.javaScriptArgumentsDepth}`);
      }
      return 'variable';
    }
    if (stream.match(/^\+#{/, false)) {
      stream.next();
      state.mixinCallAfter = true;
	logger.debug(`call(stream=${stream},state=${state}): state.mixinCallAfter → ${state.mixinCallAfter}`);
	logger.debug(`<<< Exiting call(stream=${stream},state=${state})`);
      return interpolation(stream, state);
    }
  }
  function callArguments(stream, state) {
    if (state.mixinCallAfter) {
	logger.debug(`>>> Entering callArguments(stream=${stream},state=${state})`);
      state.mixinCallAfter = false;
	logger.debug(`callArguments(stream=${stream},state=${state}): state.mixinCallAfter → ${state.mixinCallAfter}`);
      if (!stream.match(/^\( *[-\w]+ *=/, false)) {
        state.javaScriptArguments = true;
		logger.debug(`callArguments(stream=${stream},state=${state}): state.javaScriptArguments → ${state.javaScriptArguments}`);
        state.javaScriptArgumentsDepth = 0;
		logger.debug(`callArguments(stream=${stream},state=${state}): state.javaScriptArgumentsDepth → ${state.javaScriptArgumentsDepth}`);
      }
	logger.debug(`<<< Exiting callArguments(stream=${stream},state=${state})`);
      return true;
    }
  }

  function conditional(stream, state) {
	logger.debug(`>>> Entering conditional(stream=${stream},state=${state})`);
    if (stream.match(/^(if|unless|else if|else)\b/)) {
      state.javaScriptLine = true;
	logger.debug(`conditional(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`);
	logger.debug(`<<< Exiting conditional(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }

  function each(stream, state) {
	logger.debug(`>>> Entering each(stream=${stream},state=${state})`);
    if (stream.match(/^(- *)?(each|for)\b/)) {
      state.isEach = true;
	logger.debug(`each(stream=${stream},state=${state}): state.isEach → ${state.isEach}`);
	logger.debug(`<<< Exiting each(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }
  function eachContinued(stream, state) {
	logger.debug(`>>> Entering eachContinued(stream=${stream},state=${state})`);
    if (state.isEach) {
      if (stream.match(/^ in\b/)) {
        state.javaScriptLine = true;
		logger.debug(`eachContinued(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`);
        state.isEach = false;
		logger.debug(`eachContinued(stream=${stream},state=${state}): state.isEach → ${state.isEach}`);
        return KEYWORD;
      } else if (stream.sol() || stream.eol()) {
        state.isEach = false;
      } else if (stream.next()) {
        while (!stream.match(/^ in\b/, false) && stream.next());
		logger.debug(`<<< Exiting eachContinued(stream=${stream},state=${state})`);
        return 'variable';
      }
    }
  }

  function whileStatement(stream, state) {
	logger.debug(`>>> Entering whileStatement(stream=${stream},state=${state})`);
    if (stream.match(/^while\b/)) {
      state.javaScriptLine = true;
	logger.debug(`whileStatement(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`);
	logger.debug(`<<< Exiting whileStatement(stream=${stream},state=${state})`);
      return KEYWORD;
    }
  }

  function tag(stream, state) {
	logger.debug(`>>> Entering tag(stream=${stream},state=${state})`);
    var captures;
    if (captures = stream.match(/^(\w(?:[-:\w]*\w)?)\/?/)) {
      state.lastTag = captures[1].toLowerCase();
      if (state.lastTag === 'script') {
        state.scriptType = 'application/javascript';
		logger.debug(`tag(stream=${stream},state=${state}): state.scriptType → ${state.scriptType}`);
      }
	logger.debug(`<<< Exiting tag(stream=${stream},state=${state})`);
      return 'tag';
    }
  }

  function filter(stream, state) {
	logger.debug(`>>> Entering filter(stream=${stream},state=${state})`);
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
	logger.debug(`<<< Exiting filter(stream=${stream},state=${state})`);
      return 'atom';
    }
  }

  function code(stream, state) {
	logger.debug(`>>> Entering code(stream=${stream},state=${state})`);
    if (stream.match(/^(!?=|-)/)) {
      state.javaScriptLine = true;
	logger.debug(`code(stream=${stream},state=${state}): state.javaScriptLine → ${state.javaScriptLine}`);
	logger.debug(`<<< Exiting code(stream=${stream},state=${state})`);
      return 'punctuation';
    }
  }

  function id(stream) {
	logger.debug(`>>> Entering id(stream=${stream})`);
    if (stream.match(/^#([\w-]+)/)) {
	logger.debug(`<<< Exiting id(stream=${stream})`);
      return ID;
    }
  }

  function className(stream) {
	logger.debug(`>>> Entering className(stream=${stream})`);
    if (stream.match(/^\.([\w-]+)/)) {
	logger.debug(`<<< Exiting className(stream=${stream})`);
      return CLASS;
    }
  }

  function attrs(stream, state) {
	logger.debug(`>>> Entering attrs(stream=${stream},state=${state})`);
    if (stream.peek() == '(') {
      stream.next();
      state.isAttrs = true;
	logger.debug(`attrs(stream=${stream},state=${state}): state.isAttrs → ${state.isAttrs}`);
	logger.debug(`attrs(stream=${stream},state=${state}): state.inAttributeName → ${state.inAttributeName}`);
	logger.debug(`attrs(stream=${stream},state=${state}): state.attrValue → ${state.attrValue}`);
	logger.debug(`attrs(stream=${stream},state=${state}): state.attributeIsType → ${state.attributeIsType}`);
      state.attrsNest = [];
	logger.debug(`attrs(stream=${stream},state=${state}): state.attrsNest → ${state.attrsNest}`);
      state.inAttributeName = true;
      state.attrValue = '';
      state.attributeIsType = false;
	logger.debug(`<<< Exiting attrs(stream=${stream},state=${state})`);
      return 'punctuation';
    }
  }

  function attrsContinued(stream, state) {
	logger.debug(`>>> Entering attrsContinued(stream=${stream},state=${state})`);
    if (state.isAttrs) {
      if (ATTRS_NEST[stream.peek()]) {
        state.attrsNest.push(ATTRS_NEST[stream.peek()]);
      }
      if (state.attrsNest[state.attrsNest.length - 1] === stream.peek()) {
        state.attrsNest.pop();
      } else  if (stream.eat(')')) {
        state.isAttrs = false;
		logger.debug(`attrsContinued(stream=${stream},state=${state}): state.isAttrs → ${state.isAttrs}`);
        return 'punctuation';
      }
      if (state.inAttributeName && stream.match(/^[^=,\)!]+/)) {
        if (stream.peek() === '=' || stream.peek() === '!') {
          state.inAttributeName = false;
		logger.debug(`attrsContinued(stream=${stream},state=${state}): state.inAttributeName → ${state.inAttributeName}`);
          state.jsState = jsMode.startState();
          if (state.lastTag === 'script' && stream.current().trim().toLowerCase() === 'type') {
            state.attributeIsType = true;
			logger.debug(`attrsContinued(stream=${stream},state=${state}): state.attributeIsType → ${state.attributeIsType}`);
          } else {
            state.attributeIsType = false;
          }
        }
        return 'attribute';
      }

      var tok = jsMode.token(stream, state.jsState);
	logger.debug(`attrsContinued(stream=${stream},state=${state}): tok → ${tok}`);
      if (state.attributeIsType && tok === 'string') {
        state.scriptType = stream.current().toString();
      }
      if (state.attrsNest.length === 0 && (tok === 'string' || tok === 'variable' || tok === 'keyword')) {
        try {
          Function('', 'var x ' + state.attrValue.replace(/,\s*$/, '').replace(/^!/, ''));
          state.inAttributeName = true;
          state.attrValue = '';
		logger.debug(`attrsContinued(stream=${stream},state=${state}): state.attrValue → ${state.attrValue}`);
          stream.backUp(stream.current().length);
          return attrsContinued(stream, state);
        } catch (ex) {
		logger.error(`Exception in attrsContinued(stream=${stream},state=${state}): ${ex.message}`, ex);
          //not the end of an attribute
        }
      }
      state.attrValue += stream.current();
	logger.debug(`<<< Exiting attrsContinued(stream=${stream},state=${state})`);
      return tok || true;
    }
  }

  function attributesBlock(stream, state) {
	logger.debug(`>>> Entering attributesBlock(stream=${stream},state=${state})`);
    if (stream.match(/^&attributes\b/)) {
      state.javaScriptArguments = true;
	logger.debug(`attributesBlock(stream=${stream},state=${state}): state.javaScriptArguments → ${state.javaScriptArguments}`);
      state.javaScriptArgumentsDepth = 0;
	logger.debug(`attributesBlock(stream=${stream},state=${state}): state.javaScriptArgumentsDepth → ${state.javaScriptArgumentsDepth}`);
	logger.debug(`<<< Exiting attributesBlock(stream=${stream},state=${state})`);
      return 'keyword';
    }
  }

  function indent(stream) {
	logger.debug(`>>> Entering indent(stream=${stream})`);
    if (stream.sol() && stream.eatSpace()) {
	logger.debug(`<<< Exiting indent(stream=${stream})`);
      return 'indent';
    }
  }

  function comment(stream, state) {
	logger.debug(`>>> Entering comment(stream=${stream},state=${state})`);
    if (stream.match(/^ *\/\/(-)?([^\n]*)/)) {
      state.indentOf = stream.indentation();
      state.indentToken = 'comment';
	logger.debug(`comment(stream=${stream},state=${state}): state.indentToken → ${state.indentToken}`);
	logger.debug(`<<< Exiting comment(stream=${stream},state=${state})`);
      return 'comment';
    }
  }

  function colon(stream) {
	logger.debug(`>>> Entering colon(stream=${stream})`);
    if (stream.match(/^: */)) {
	logger.debug(`<<< Exiting colon(stream=${stream})`);
      return 'colon';
    }
  }

  function text(stream, state) {
	logger.debug(`>>> Entering text(stream=${stream},state=${state})`);
    if (stream.match(/^(?:\| ?| )([^\n]+)/)) {
      return 'string';
    }
    if (stream.match(/^(<[^\n]*)/, false)) {
      // html string
      setInnerMode(stream, state, 'htmlmixed');
      state.innerModeForLine = true;
	logger.debug(`text(stream=${stream},state=${state}): state.innerModeForLine → ${state.innerModeForLine}`);
	logger.debug(`<<< Exiting text(stream=${stream},state=${state})`);
      return innerMode(stream, state, true);
    }
  }

  function dot(stream, state) {
	logger.debug(`>>> Entering dot(stream=${stream},state=${state})`);
    if (stream.eat('.')) {
      var innerMode = null;
      if (state.lastTag === 'script' && state.scriptType.toLowerCase().indexOf('javascript') != -1) {
        innerMode = state.scriptType.toLowerCase().replace(/"|'/g, '');
      } else if (state.lastTag === 'style') {
        innerMode = 'css';
      }
      setInnerMode(stream, state, innerMode);
	logger.debug(`<<< Exiting dot(stream=${stream},state=${state})`);
      return 'dot';
    }
  }

  function fail(stream) {
	logger.debug(`>>> Entering fail(stream=${stream})`);
    stream.next();
	logger.debug(`<<< Exiting fail(stream=${stream})`);
    return null;
  }


  function setInnerMode(stream, state, mode) {
	logger.debug(`>>> Entering setInnerMode(stream=${stream},state=${state},mode=${mode})`);
    mode = CodeMirror.mimeModes[mode] || mode;
    mode = config.innerModes ? config.innerModes(mode) || mode : mode;
    mode = CodeMirror.mimeModes[mode] || mode;
    mode = CodeMirror.getMode(config, mode);
    state.indentOf = stream.indentation();
	logger.debug(`setInnerMode(stream=${stream},state=${state},mode=${mode}): mode → ${mode}`);

    if (mode && mode.name !== 'null') {
      state.innerMode = mode;
	logger.debug(`setInnerMode(stream=${stream},state=${state},mode=${mode}): state.innerMode → ${state.innerMode}`);
    } else {
      state.indentToken = 'string';
	logger.debug(`setInnerMode(stream=${stream},state=${state},mode=${mode}): state.indentToken → ${state.indentToken}`);
    }
	logger.debug(`<<< Exiting setInnerMode(stream=${stream},state=${state},mode=${mode})`);
  }
  function innerMode(stream, state, force) {
	logger.debug(`>>> Entering innerMode(stream=${stream},state=${state},force=${force})`);
    if (stream.indentation() > state.indentOf || (state.innerModeForLine && !stream.sol()) || force) {
      if (state.innerMode) {
        if (!state.innerState) {
          state.innerState = state.innerMode.startState ? state.innerMode.startState(stream.indentation()) : {};
        }
        return stream.hideFirstChars(state.indentOf + 2, function () {
		logger.debug('>>> Entering hideFirstChars()');
		logger.debug('<<< Exiting hideFirstChars()');
          return state.innerMode.token(stream, state.innerState) || true;
        });
      } else {
        stream.skipToEnd();
        return state.indentToken;
      }
    } else if (stream.sol()) {
      state.indentOf = Infinity;
	logger.debug(`innerMode(stream=${stream},state=${state},force=${force}): state.indentOf → ${state.indentOf}`);
	logger.debug(`innerMode(stream=${stream},state=${state},force=${force}): state.innerState → ${state.innerState}`);
      state.indentToken = null;
	logger.debug(`innerMode(stream=${stream},state=${state},force=${force}): state.indentToken → ${state.indentToken}`);
      state.innerMode = null;
	logger.debug(`innerMode(stream=${stream},state=${state},force=${force}): state.innerMode → ${state.innerMode}`);
      state.innerState = null;
    }
	logger.debug(`<<< Exiting innerMode(stream=${stream},state=${state},force=${force})`);
  }
  function restOfLine(stream, state) {
	logger.debug(`>>> Entering restOfLine(stream=${stream},state=${state})`);
    if (stream.sol()) {
      // if restOfLine was set at end of line, ignore it
      state.restOfLine = '';
	logger.debug(`restOfLine(stream=${stream},state=${state}): state.restOfLine → ${state.restOfLine}`);
    }
    if (state.restOfLine) {
      stream.skipToEnd();
      var tok = state.restOfLine;
      state.restOfLine = '';
	logger.debug(`<<< Exiting restOfLine(stream=${stream},state=${state})`);
      return tok;
    }
  }


  function startState() {
	logger.debug('>>> Entering startState()');
	logger.debug('<<< Exiting startState()');
    return new State();
  }
  function copyState(state) {
	logger.debug(`>>> Entering copyState(state=${state})`);
	logger.debug(`<<< Exiting copyState(state=${state})`);
    return state.copy();
  }
  /**
   * Get the next token in the stream
   *
   * @param {Stream} stream
   * @param {State} state
   */
  function nextToken(stream, state) {
	logger.debug(`>>> Entering nextToken(stream=${stream},state=${state})`);
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

	logger.debug(`nextToken(stream=${stream},state=${state}): tok → ${tok}`);
	logger.debug(`<<< Exiting nextToken(stream=${stream},state=${state})`);
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
