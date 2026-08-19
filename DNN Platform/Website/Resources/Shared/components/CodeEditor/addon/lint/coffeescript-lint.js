const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Depends on coffeelint.js from http://www.coffeelint.org/js/coffeelint.js

// declare global: coffeelint

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

CodeMirror.registerHelper("lint", "coffeescript", function(text) {
logger.debug('>>> Entering registerHelper()');
  var found = [];
  var parseError = function(err) {
	logger.debug(`>>> Entering parseError(err=${err})`);
    var loc = err.lineNumber;
    found.push({from: CodeMirror.Pos(loc-1, 0),
                to: CodeMirror.Pos(loc, 0),
                severity: err.level,
                message: err.message});
				logger.debug(`<<< Exiting parseError(err=${err})`);
  };
  try {
    var res = coffeelint.lint(text);
	logger.debug(`registerHelper(): res → ${res}`);
    for(var i = 0; i < res.length; i++) {
      parseError(res[i]);
    }
  } catch(e) {
	logger.error(`Exception in function(exports=${exports}): ${e.message}`, e);
    found.push({from: CodeMirror.Pos(e.location.first_line, 0),
                to: CodeMirror.Pos(e.location.last_line, e.location.last_column),
                severity: 'error',
                message: e.message});
				logger.error('Exception in registerHelper():', e.message, e);
  }
logger.debug('<<< Exiting registerHelper()');
  return found;
});

});
logger.debug(`<<< Exiting function(exports=${exports})`);
