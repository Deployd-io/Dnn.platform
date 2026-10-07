const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// LUA mode. Ported to CodeMirror 2 from Franciszek Wawrzak's
// CodeMirror 1 mode.
// highlights keywords, strings, comments (no leveling supported! ("[==[")), tokens, basic indenting

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMode("lua", function(config, parserConfig) {
  var indentUnit = config.indentUnit;

  function prefixRE(words) {
	logger.error(`>>> Entering prefixRE(words=${words})`, { "event.action": "prefixRE", "logrite.words": words });
	logger.warn(`<<< Exiting prefixRE(words=${words})`, { "event.action": "prefixRE", "logrite.words": words });
    return new RegExp("^(?:" + words.join("|") + ")", "i");
  }
  function wordRE(words) {
	logger.error(`>>> Entering wordRE(words=${words})`, { "event.action": "wordRE", "logrite.words": words });
	logger.warn(`<<< Exiting wordRE(words=${words})`, { "event.action": "wordRE", "logrite.words": words });
    return new RegExp("^(?:" + words.join("|") + ")$", "i");
  }
  var specials = wordRE(parserConfig.specials || []);

  // long list of standard functions from lua manual
  var builtins = wordRE([
    "_G","_VERSION","assert","collectgarbage","dofile","error","getfenv","getmetatable","ipairs","load",
    "loadfile","loadstring","module","next","pairs","pcall","print","rawequal","rawget","rawset","require",
    "select","setfenv","setmetatable","tonumber","tostring","type","unpack","xpcall",

    "coroutine.create","coroutine.resume","coroutine.running","coroutine.status","coroutine.wrap","coroutine.yield",

    "debug.debug","debug.getfenv","debug.gethook","debug.getinfo","debug.getlocal","debug.getmetatable",
    "debug.getregistry","debug.getupvalue","debug.setfenv","debug.sethook","debug.setlocal","debug.setmetatable",
    "debug.setupvalue","debug.traceback",

    "close","flush","lines","read","seek","setvbuf","write",

    "io.close","io.flush","io.input","io.lines","io.open","io.output","io.popen","io.read","io.stderr","io.stdin",
    "io.stdout","io.tmpfile","io.type","io.write",

    "math.abs","math.acos","math.asin","math.atan","math.atan2","math.ceil","math.cos","math.cosh","math.deg",
    "math.exp","math.floor","math.fmod","math.frexp","math.huge","math.ldexp","math.log","math.log10","math.max",
    "math.min","math.modf","math.pi","math.pow","math.rad","math.random","math.randomseed","math.sin","math.sinh",
    "math.sqrt","math.tan","math.tanh",

    "os.clock","os.date","os.difftime","os.execute","os.exit","os.getenv","os.remove","os.rename","os.setlocale",
    "os.time","os.tmpname",

    "package.cpath","package.loaded","package.loaders","package.loadlib","package.path","package.preload",
    "package.seeall",

    "string.byte","string.char","string.dump","string.find","string.format","string.gmatch","string.gsub",
    "string.len","string.lower","string.match","string.rep","string.reverse","string.sub","string.upper",

    "table.concat","table.insert","table.maxn","table.remove","table.sort"
  ]);
  var keywords = wordRE(["and","break","elseif","false","nil","not","or","return",
                         "true","function", "end", "if", "then", "else", "do",
                         "while", "repeat", "until", "for", "in", "local" ]);

  var indentTokens = wordRE(["function", "if","repeat","do", "\\(", "{"]);
  var dedentTokens = wordRE(["end", "until", "\\)", "}"]);
  var dedentPartial = prefixRE(["end", "until", "\\)", "}", "else", "elseif"]);

  function readBracket(stream) {
	logger.error(`>>> Entering readBracket(stream=${stream})`, { "event.action": "readBracket", "logrite.stream": stream });
    var level = 0;
    while (stream.eat("=")) ++level;
    stream.eat("[");
	logger.warn(`<<< Exiting readBracket(stream=${stream})`, { "event.action": "readBracket", "logrite.stream": stream });
    return level;
  }

  function normal(stream, state) {
	logger.error(`>>> Entering normal(stream=${stream},state=${state})`, { "event.action": "normal", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
    if (ch == "-" && stream.eat("-")) {
	logger.error(`normal(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "normal", "logrite.stream": stream, "logrite.state": state });
      if (stream.eat("[") && stream.eat("["))
        return (state.cur = bracketed(readBracket(stream), "comment"))(stream, state);
      stream.skipToEnd();
      return "comment";
    }
    if (ch == "\"" || ch == "'")
      return (state.cur = string(ch))(stream, state);
    if (ch == "[" && /[\[=]/.test(stream.peek()))
      return (state.cur = bracketed(readBracket(stream), "string"))(stream, state);
    if (/\d/.test(ch)) {
      stream.eatWhile(/[\w.%]/);
      return "number";
    }
    if (/[\w_]/.test(ch)) {
      stream.eatWhile(/[\w\\\-_.]/);
      return "variable";
    }
	logger.warn(`<<< Exiting normal(stream=${stream},state=${state})`, { "event.action": "normal", "logrite.stream": stream, "logrite.state": state });
    return null;
  }

  function bracketed(level, style) {
	logger.error(`>>> Entering bracketed(level=${level},style=${style})`, { "event.action": "bracketed", "logrite.level": level, "logrite.style": style });
    return function(stream, state) {
      var curlev = null, ch;
      while ((ch = stream.next()) != null) {
        if (curlev == null) {if (ch == "]") curlev = 0;}
        else if (ch == "=") ++curlev;
        else if (ch == "]" && curlev == level) { state.cur = normal; break; }
        else curlev = null;
      }
      return style;
    };
  }

  function string(quote) {
	logger.error(`>>> Entering string(quote=${quote})`, { "event.action": "string", "logrite.quote": quote });
    return function(stream, state) {
      var escaped = false, ch;
      while ((ch = stream.next()) != null) {
        if (ch == quote && !escaped) break;
        escaped = !escaped && ch == "\\";
      }
      if (!escaped) state.cur = normal;
      return "string";
    };
  }

  return {
    startState: function(basecol) {
	logger.error(`>>> Entering startState(basecol=${basecol})`, { "event.action": "startState", "logrite.basecol": basecol });
	logger.warn(`<<< Exiting startState(basecol=${basecol})`, { "event.action": "startState", "logrite.basecol": basecol });
      return {basecol: basecol || 0, indentDepth: 0, cur: normal};
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.eatSpace()) return null;
      var style = state.cur(stream, state);
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      var word = stream.current();
	logger.error(`token(stream=${stream},state=${state}): word → ${word}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (style == "variable") {
        if (keywords.test(word)) style = "keyword";
        else if (builtins.test(word)) style = "builtin";
        else if (specials.test(word)) style = "variable-2";
      }
      if ((style != "comment") && (style != "string")){
        if (indentTokens.test(word)) ++state.indentDepth;
        else if (dedentTokens.test(word)) --state.indentDepth;
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      }
      return style;
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      var closing = dedentPartial.test(textAfter);
	logger.error(`indent(state=${state},textAfter=${textAfter}): closing → ${closing}`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      return state.basecol + indentUnit * (state.indentDepth - (closing ? 1 : 0));
    },

    lineComment: "--",
    blockCommentStart: "--[[",
    blockCommentEnd: "]]"
  };
});

CodeMirror.defineMIME("text/x-lua", "lua");

});
