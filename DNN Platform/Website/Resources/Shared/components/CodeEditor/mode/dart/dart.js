const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../clike/clike"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../clike/clike"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  var keywords = ("this super static final const abstract class extends external factory " +
    "implements get native operator set typedef with enum throw rethrow " +
    "assert break case continue default in return new deferred async await " +
    "try catch finally do else for if switch while import library export " +
    "part of show hide is").split(" ");
  var blockKeywords = "try catch finally do else for if switch while".split(" ");
logger.debug(`function(): blockKeywords → ${blockKeywords}`);
  var atoms = "true false null".split(" ");
logger.debug(`function(): atoms → ${atoms}`);
  var builtins = "void bool num int double dynamic var String".split(" ");
logger.debug(`function(): builtins → ${builtins}`);

  function set(words) {
	logger.debug(`>>> Entering set(words=${words})`);
    var obj = {};
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.debug(`<<< Exiting set(words=${words})`);
    return obj;
  }

  CodeMirror.defineMIME("application/dart", {
    name: "clike",
    keywords: set(keywords),
    multiLineStrings: true,
    blockKeywords: set(blockKeywords),
    builtin: set(builtins),
    atoms: set(atoms),
    hooks: {
      "@": function(stream) {
        stream.eatWhile(/[\w\$_]/);
        return "meta";
      }
    }
  });

  CodeMirror.registerHelper("hintWords", "application/dart", keywords.concat(atoms).concat(builtins));

  // This is needed to make loading through meta.js work.
  CodeMirror.defineMode("dart", function(conf) {
	logger.debug('>>> Entering defineMode()');
	logger.debug('<<< Exiting function()');
	logger.debug('<<< Exiting defineMode()');
    return CodeMirror.getMode(conf, "application/dart");
  }, "clike");
});
