const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

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

// Depends on js-yaml.js from https://github.com/nodeca/js-yaml

// declare global: jsyaml

CodeMirror.registerHelper("lint", "yaml", function(text) {
  var found = [];
  try { jsyaml.load(text); }
  catch(e) {
	logger.error(`Exception in function(exports=${exports}): ${e.message}`, e);
      var loc = e.mark;
	logger.error(`Exception in registerHelper(found=${found}): ${e.message}`, e);
      found.push({ from: CodeMirror.Pos(loc.line, loc.column), to: CodeMirror.Pos(loc.line, loc.column), message: e.message });
	logger.debug(`<<< Exiting registerHelper(found=${found})`);
  }
  return found;
});

});
logger.debug(`<<< Exiting function(exports=${exports})`);
