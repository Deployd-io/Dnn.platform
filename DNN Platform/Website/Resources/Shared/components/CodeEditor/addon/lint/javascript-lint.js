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
  // declare global: JSHINT

  var bogus = [ "Dangerous comment" ];
logger.debug(`function(exports=${exports}): bogus → ${bogus}`);

  var warnings = [ [ "Expected '{'",
                     "Statement body should be inside '{ }' braces." ] ];
					logger.debug(`function(exports=${exports}): warnings → ${warnings}`);

  var errors = [ "Missing semicolon", "Extra comma", "Missing property name",
                 "Unmatched ", " and instead saw", " is not defined",
                 "Unclosed string", "Stopping, unable to continue" ];
				logger.debug(`<<< Exiting function(exports=${exports})`);
				logger.debug(`function(exports=${exports}): errors → ${errors}`);

  function validator(text, options) {
	logger.debug(`>>> Entering validator(text=${text},options=${options})`);
    if (!window.JSHINT) return [];
    JSHINT(text, options, options.globals);
    var errors = JSHINT.data().errors, result = [];
	logger.debug(`validator(text=${text},options=${options}): errors → ${errors}`);
    if (errors) parseErrors(errors, result);
	logger.debug(`<<< Exiting validator(text=${text},options=${options})`);
    return result;
  }

  CodeMirror.registerHelper("lint", "javascript", validator);

  function cleanup(error) {
	logger.debug(`>>> Entering cleanup(error=${error})`);
    // All problems are warnings by default
    fixWith(error, warnings, "warning", true);
    fixWith(error, errors, "error");

	logger.debug(`<<< Exiting cleanup(error=${error})`);
    return isBogus(error) ? null : error;
  }

  function fixWith(error, fixes, severity, force) {
	logger.debug(`>>> Entering fixWith(error=${error},fixes=${fixes},severity=${severity},force=${force})`);
    var description, fix, find, replace, found;

    description = error.description;

    for ( var i = 0; i < fixes.length; i++) {
      fix = fixes[i];
      find = (typeof fix === "string" ? fix : fix[0]);
      replace = (typeof fix === "string" ? null : fix[1]);
      found = description.indexOf(find) !== -1;

      if (force || found) {
        error.severity = severity;
		logger.debug(`fixWith(error=${error},fixes=${fixes},severity=${severity},force=${force}): error.severity → ${error.severity}`);
      }
      if (found && replace) {
        error.description = replace;
		logger.debug(`fixWith(error=${error},fixes=${fixes},severity=${severity},force=${force}): error.description → ${error.description}`);
      }
	logger.debug(`<<< Exiting fixWith(error=${error},fixes=${fixes},severity=${severity},force=${force})`);
    }
  }

  function isBogus(error) {
	logger.debug(`>>> Entering isBogus(error=${error})`);
    var description = error.description;
    for ( var i = 0; i < bogus.length; i++) {
      if (description.indexOf(bogus[i]) !== -1) {
        return true;
      }
    }
	logger.debug(`<<< Exiting isBogus(error=${error})`);
    return false;
  }

  function parseErrors(errors, output) {
	logger.debug(`>>> Entering parseErrors(errors=${errors},output=${output})`);
    for ( var i = 0; i < errors.length; i++) {
      var error = errors[i];
	logger.debug(`parseErrors(errors=${errors},output=${output}): error → ${error}`);
      if (error) {
        var linetabpositions, index;

        linetabpositions = [];

        // This next block is to fix a problem in jshint. Jshint
        // replaces
        // all tabs with spaces then performs some checks. The error
        // positions (character/space) are then reported incorrectly,
        // not taking the replacement step into account. Here we look
        // at the evidence line and try to adjust the character position
        // to the correct value.
        if (error.evidence) {
          // Tab positions are computed once per line and cached
          var tabpositions = linetabpositions[error.line];
          if (!tabpositions) {
            var evidence = error.evidence;
            tabpositions = [];
            // ugggh phantomjs does not like this
            // forEachChar(evidence, function(item, index) {
            Array.prototype.forEach.call(evidence, function(item,
                                                            index) {
															logger.debug('>>> Entering forEachChar()');
              if (item === '\t') {
                // First col is 1 (not 0) to match error
                // positions
                tabpositions.push(index + 1);
				logger.debug('<<< Exiting forEachChar()');
              }
            });
            linetabpositions[error.line] = tabpositions;
          }
          if (tabpositions.length > 0) {
            var pos = error.character;
            tabpositions.forEach(function(tabposition) {
			logger.debug('>>> Entering forEach()');
              if (pos > tabposition) pos -= 1;
            });
			logger.debug('<<< Exiting forEach()');
            error.character = pos;
			logger.debug(`parseErrors(errors=${errors},output=${output}): error.character → ${error.character}`);
          }
        }

        var start = error.character - 1, end = start + 1;
        if (error.evidence) {
          index = error.evidence.substring(start).search(/.\b/);
          if (index > -1) {
            end += index;
          }
        }

        // Convert to format expected by validation service
        error.description = error.reason;// + "(jshint)";
        error.start = error.character;
		logger.debug(`parseErrors(errors=${errors},output=${output}): error.start → ${error.start}`);
        error.end = end;
		logger.debug(`parseErrors(errors=${errors},output=${output}): error.end → ${error.end}`);
        error = cleanup(error);

        if (error)
          output.push({message: error.description,
                       severity: error.severity,
                       from: CodeMirror.Pos(error.line - 1, start),
                       to: CodeMirror.Pos(error.line - 1, end)});
					logger.debug(`<<< Exiting parseErrors(errors=${errors},output=${output})`);
      }
    }
  }
});
