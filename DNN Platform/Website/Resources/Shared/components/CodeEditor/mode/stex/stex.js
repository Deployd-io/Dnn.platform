const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/*
 * Author: Constantin Jucovschi (c.jucovschi@jacobs-university.de)
 * Licence: MIT
 */

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
  CodeMirror.defineMode("stex", function() {
	logger.debug('>>> Entering defineMode()');
    "use strict";
	logger.debug('<<< Exiting defineMode()');

    function pushCommand(state, command) {
	logger.debug(`>>> Entering pushCommand(state=${state},command=${command})`);
      state.cmdState.push(command);
	logger.debug(`<<< Exiting pushCommand(state=${state},command=${command})`);
    }

    function peekCommand(state) {
	logger.debug(`>>> Entering peekCommand(state=${state})`);
      if (state.cmdState.length > 0) {
        return state.cmdState[state.cmdState.length - 1];
      } else {
		logger.debug(`<<< Exiting peekCommand(state=${state})`);
        return null;
      }
    }

    function popCommand(state) {
	logger.debug(`>>> Entering popCommand(state=${state})`);
      var plug = state.cmdState.pop();
      if (plug) {
		logger.debug(`popCommand(state=${state}): plug → ${plug}`);
        plug.closeBracket();
		logger.debug(`<<< Exiting popCommand(state=${state})`);
      }
    }

    // returns the non-default plugin closest to the end of the list
    function getMostPowerful(state) {
	logger.debug(`>>> Entering getMostPowerful(state=${state})`);
      var context = state.cmdState;
      for (var i = context.length - 1; i >= 0; i--) {
        var plug = context[i];
        if (plug.name == "DEFAULT") {
          continue;
        }
	logger.debug(`<<< Exiting getMostPowerful(state=${state})`);
        return plug;
      }
      return { styleIdentifier: function() { return null; } };
    }

    function addPluginPattern(pluginName, cmdStyle, styles) {
	logger.debug(`>>> Entering addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles})`);
      return function () {
        this.name = pluginName;
        this.bracketNo = 0;
        this.style = cmdStyle;
		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): this.name → ${this.name}`);
		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): this.bracketNo → ${this.bracketNo}`);
        this.styles = styles;
        this.argument = null;   // \begin and \end have arguments that follow. These are stored in the plugin

		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): this.style → ${this.style}`);
        this.styleIdentifier = function() {
		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): this.styles → ${this.styles}`);
          return this.styles[this.bracketNo - 1] || null;
        };
        this.openBracket = function() {
		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): this.argument → ${this.argument}`);
          this.bracketNo++;
          return "bracket";
        };
        this.closeBracket = function() {};
      };
	logger.debug(`<<< Exiting addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles})`);
    }

    var plugins = {};

    plugins["importmodule"] = addPluginPattern("importmodule", "tag", ["string", "builtin"]);
    plugins["documentclass"] = addPluginPattern("documentclass", "tag", ["", "atom"]);
    plugins["usepackage"] = addPluginPattern("usepackage", "tag", ["atom"]);
    plugins["begin"] = addPluginPattern("begin", "tag", ["atom"]);
    plugins["end"] = addPluginPattern("end", "tag", ["atom"]);

    plugins["DEFAULT"] = function () {
      this.name = "DEFAULT";
      this.style = "tag";

      this.styleIdentifier = this.openBracket = this.closeBracket = function() {};
    };

    function setState(state, f) {
	logger.debug(`>>> Entering setState(state=${state},f=${f})`);
      state.f = f;
	logger.debug(`<<< Exiting setState(state=${state},f=${f})`);
    }

    // called when in a normal (no environment) context
    function normal(source, state) {
	logger.debug(`>>> Entering normal(source=${source},state=${state})`);
      var plug;
      // Do we look like '\command' ?  If so, attempt to apply the plugin 'command'
      if (source.match(/^\\[a-zA-Z@]+/)) {
        var cmdName = source.current().slice(1);
		logger.debug(`normal(source=${source},state=${state}): cmdName → ${cmdName}`);
        plug = plugins[cmdName] || plugins["DEFAULT"];
        plug = new plug();
        pushCommand(state, plug);
		logger.debug(`normal(source=${source},state=${state}): plug → ${plug}`);
        setState(state, beginParams);
        return plug.style;
      }

      // escape characters
      if (source.match(/^\\[$&%#{}_]/)) {
        return "tag";
      }

      // white space control characters
      if (source.match(/^\\[,;!\/\\]/)) {
        return "tag";
      }

      // find if we're starting various math modes
      if (source.match("\\[")) {
        setState(state, function(source, state){ return inMathMode(source, state, "\\]"); });
        return "keyword";
      }
      if (source.match("$$")) {
        setState(state, function(source, state){ return inMathMode(source, state, "$$"); });
        return "keyword";
      }
      if (source.match("$")) {
        setState(state, function(source, state){ return inMathMode(source, state, "$"); });
        return "keyword";
      }

      var ch = source.next();
      if (ch == "%") {
        source.skipToEnd();
        return "comment";
      } else if (ch == '}' || ch == ']') {
        plug = peekCommand(state);
        if (plug) {
          plug.closeBracket(ch);
          setState(state, beginParams);
        } else {
          return "error";
        }
        return "bracket";
      } else if (ch == '{' || ch == '[') {
        plug = plugins["DEFAULT"];
        plug = new plug();
        pushCommand(state, plug);
        return "bracket";
      } else if (/\d/.test(ch)) {
        source.eatWhile(/[\w.%]/);
        return "atom";
      } else {
        source.eatWhile(/[\w\-_]/);
        plug = getMostPowerful(state);
        if (plug.name == 'begin') {
          plug.argument = source.current();
        }
		logger.debug(`<<< Exiting normal(source=${source},state=${state})`);
        return plug.styleIdentifier();
      }
    }

    function inMathMode(source, state, endModeSeq) {
	logger.debug(`>>> Entering inMathMode(source=${source},state=${state},endModeSeq=${endModeSeq})`);
      if (source.eatSpace()) {
        return null;
      }
      if (source.match(endModeSeq)) {
        setState(state, normal);
        return "keyword";
      }
      if (source.match(/^\\[a-zA-Z@]+/)) {
        return "tag";
      }
      if (source.match(/^[a-zA-Z]+/)) {
        return "variable-2";
      }
      // escape characters
      if (source.match(/^\\[$&%#{}_]/)) {
        return "tag";
      }
      // white space control characters
      if (source.match(/^\\[,;!\/]/)) {
        return "tag";
      }
      // special math-mode characters
      if (source.match(/^[\^_&]/)) {
        return "tag";
      }
      // non-special characters
      if (source.match(/^[+\-<>|=,\/@!*:;'"`~#?]/)) {
        return null;
      }
      if (source.match(/^(\d+\.\d*|\d*\.\d+|\d+)/)) {
        return "number";
      }
      var ch = source.next();
      if (ch == "{" || ch == "}" || ch == "[" || ch == "]" || ch == "(" || ch == ")") {
        return "bracket";
      }

      if (ch == "%") {
        source.skipToEnd();
        return "comment";
      }
      return "error";
    }

    function beginParams(source, state) {
      var ch = source.peek(), lastPlug;
      if (ch == '{' || ch == '[') {
        lastPlug = peekCommand(state);
        lastPlug.openBracket(ch);
        source.eat(ch);
        setState(state, normal);
        return "bracket";
      }
      if (/[ \t\r]/.test(ch)) {
        source.eat(ch);
        return null;
      }
      setState(state, normal);
      popCommand(state);

      return normal(source, state);
    }

    return {
      startState: function() {
        return {
          cmdState: [],
          f: normal
        };
      },
      copyState: function(s) {
        return {
          cmdState: s.cmdState.slice(),
          f: s.f
        };
      },
      token: function(stream, state) {
        return state.f(stream, state);
      },
      blankLine: function(state) {
        state.f = normal;
        state.cmdState.length = 0;
      },
      lineComment: "%"
    };
  });

  CodeMirror.defineMIME("text/x-stex", "stex");
  CodeMirror.defineMIME("text/x-latex", "stex");

});
