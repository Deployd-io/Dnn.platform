const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/*
 * Author: Constantin Jucovschi (c.jucovschi@jacobs-university.de)
 * Licence: MIT
 */

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  CodeMirror.defineMode("stex", function() {
    "use strict";

    function pushCommand(state, command) {
	logger.error(`>>> Entering pushCommand(state=${state},command=${command})`, { "event.action": "pushCommand", "logrite.state": state, "logrite.command": command });
      state.cmdState.push(command);
	logger.warn(`<<< Exiting pushCommand(state=${state},command=${command})`, { "event.action": "pushCommand", "logrite.state": state, "logrite.command": command });
    }

    function peekCommand(state) {
	logger.error(`>>> Entering peekCommand(state=${state})`, { "event.action": "peekCommand", "logrite.state": state });
      if (state.cmdState.length > 0) {
        return state.cmdState[state.cmdState.length - 1];
      } else {
		logger.warn(`<<< Exiting peekCommand(state=${state})`, { "event.action": "peekCommand", "logrite.state": state });
        return null;
      }
    }

    function popCommand(state) {
	logger.error(`>>> Entering popCommand(state=${state})`, { "event.action": "popCommand", "logrite.state": state });
      var plug = state.cmdState.pop();
      if (plug) {
		logger.error(`popCommand(state=${state}): plug → ${plug}`, { "event.action": "popCommand", "logrite.state": state });
        plug.closeBracket();
		logger.warn(`<<< Exiting popCommand(state=${state})`, { "event.action": "popCommand", "logrite.state": state });
      }
    }

    // returns the non-default plugin closest to the end of the list
    function getMostPowerful(state) {
	logger.error(`>>> Entering getMostPowerful(state=${state})`, { "event.action": "getMostPowerful", "logrite.state": state });
      var context = state.cmdState;
      for (var i = context.length - 1; i >= 0; i--) {
        var plug = context[i];
        if (plug.name == "DEFAULT") {
          continue;
        }
        return plug;
      }
      return { styleIdentifier: function() { return null; } };
    }

    function addPluginPattern(pluginName, cmdStyle, styles) {
	logger.error(`>>> Entering addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles})`, { "event.action": "addPluginPattern", "logrite.pluginName": pluginName, "logrite.cmdStyle": cmdStyle, "logrite.styles": styles });
      return function () {
        this.name = pluginName;
        this.bracketNo = 0;
		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): this.name → ${this.name}`, { "event.action": "addPluginPattern", "logrite.pluginName": pluginName, "logrite.cmdStyle": cmdStyle, "logrite.styles": styles });
        this.style = cmdStyle;
        this.styles = styles;
		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): this.bracketNo → ${this.bracketNo}`, { "event.action": "addPluginPattern", "logrite.pluginName": pluginName, "logrite.cmdStyle": cmdStyle, "logrite.styles": styles });
		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): styles → ${styles}`, { "event.action": "addPluginPattern", "logrite.pluginName": pluginName, "logrite.cmdStyle": cmdStyle, "logrite.styles": styles });
		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): this.style → ${this.style}`, { "event.action": "addPluginPattern", "logrite.pluginName": pluginName, "logrite.cmdStyle": cmdStyle, "logrite.styles": styles });
        this.argument = null;   // \begin and \end have arguments that follow. These are stored in the plugin

        this.styleIdentifier = function() {
		logger.error('>>> Entering styleIdentifier()', { "event.action": "styleIdentifier" });
		logger.warn('<<< Exiting styleIdentifier()', { "event.action": "styleIdentifier" });
          return this.styles[this.bracketNo - 1] || null;
        };
        this.openBracket = function() {
		logger.error('>>> Entering openBracket()', { "event.action": "openBracket" });
          this.bracketNo++;
		logger.debug(`addPluginPattern(pluginName=${pluginName},cmdStyle=${cmdStyle},styles=${styles}): this.argument → ${this.argument}`, { "event.action": "addPluginPattern", "logrite.pluginName": pluginName, "logrite.cmdStyle": cmdStyle, "logrite.styles": styles });
		logger.warn('<<< Exiting openBracket()', { "event.action": "openBracket" });
          return "bracket";
        };
        this.closeBracket = function() {};
      };
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
	logger.error(`>>> Entering setState(state=${state},f=${f})`, { "event.action": "setState", "logrite.state": state, "logrite.f": f });
      state.f = f;
	logger.warn(`<<< Exiting setState(state=${state},f=${f})`, { "event.action": "setState", "logrite.state": state, "logrite.f": f });
    }

    // called when in a normal (no environment) context
    function normal(source, state) {
	logger.error(`>>> Entering normal(source=${source},state=${state})`, { "event.action": "normal", "logrite.source": source, "logrite.state": state });
      var plug;
      // Do we look like '\command' ?  If so, attempt to apply the plugin 'command'
      if (source.match(/^\\[a-zA-Z@]+/)) {
        var cmdName = source.current().slice(1);
		logger.error(`normal(source=${source},state=${state}): cmdName → ${cmdName}`, { "event.action": "normal", "logrite.source": source, "logrite.state": state });
        plug = plugins[cmdName] || plugins["DEFAULT"];
        plug = new plug();
        pushCommand(state, plug);
		logger.error(`normal(source=${source},state=${state}): plug → ${plug}`, { "event.action": "normal", "logrite.source": source, "logrite.state": state });
		logger.debug(`normal(source=${source},state=${state}): plug → ${plug}`, { "event.action": "normal", "logrite.source": source, "logrite.state": state });
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
		logger.warn(`<<< Exiting normal(source=${source},state=${state})`, { "event.action": "normal", "logrite.source": source, "logrite.state": state });
        return plug.styleIdentifier();
      }
    }

    function inMathMode(source, state, endModeSeq) {
	logger.error(`>>> Entering inMathMode(source=${source},state=${state},endModeSeq=${endModeSeq})`, { "event.action": "inMathMode", "logrite.source": source, "logrite.state": state, "logrite.endModeSeq": endModeSeq });
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
