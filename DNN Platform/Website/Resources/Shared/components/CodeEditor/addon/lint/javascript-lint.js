const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
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

  var warnings = [ [ "Expected '{'",
                     "Statement body should be inside '{ }' braces." ] ];

  var errors = [ "Missing semicolon", "Extra comma", "Missing property name",
                 "Unmatched ", " and instead saw", " is not defined",
                 "Unclosed string", "Stopping, unable to continue" ];

  function validator(text, options) {
	logger.error(`>>> Entering validator(text=${text},options=${options})`, { "event.action": "validator", "logrite.text": text, "logrite.options": options });
    if (!window.JSHINT) return [];
    JSHINT(text, options, options.globals);
    var errors = JSHINT.data().errors, result = [];
	logger.error(`validator(text=${text},options=${options}): errors → ${errors}`, { "event.action": "validator", "logrite.text": text, "logrite.options": options });
    if (errors) parseErrors(errors, result);
	logger.warn(`<<< Exiting validator(text=${text},options=${options})`, { "event.action": "validator", "logrite.text": text, "logrite.options": options });
    return result;
  }

  CodeMirror.registerHelper("lint", "javascript", validator);

  function cleanup(error) {
	logger.error(`>>> Entering cleanup(error=${error})`, { "event.action": "cleanup", "logrite.error": error });
    // All problems are warnings by default
    fixWith(error, warnings, "warning", true);
    fixWith(error, errors, "error");

	logger.warn(`<<< Exiting cleanup(error=${error})`, { "event.action": "cleanup", "logrite.error": error });
    return isBogus(error) ? null : error;
  }

  function fixWith(error, fixes, severity, force) {
	logger.error(`>>> Entering fixWith(error=${error},fixes=${fixes},severity=${severity},force=${force})`, { "event.action": "fixWith", "logrite.error": error, "logrite.fixes": fixes, "logrite.severity": severity, "logrite.force": force });
    var description, fix, find, replace, found;

    description = error.description;
	logger.debug(`fixWith(error=${error},fixes=${fixes},severity=${severity},force=${force}): description → ${description}`, { "event.action": "fixWith", "logrite.error": error, "logrite.fixes": fixes, "logrite.severity": severity, "logrite.force": force });

    for ( var i = 0; i < fixes.length; i++) {
      fix = fixes[i];
      find = (typeof fix === "string" ? fix : fix[0]);
      replace = (typeof fix === "string" ? null : fix[1]);
      found = description.indexOf(find) !== -1;

      if (force || found) {
        error.severity = severity;
		logger.debug(`fixWith(error=${error},fixes=${fixes},severity=${severity},force=${force}): error.severity → ${error.severity}`, { "event.action": "fixWith", "logrite.error": error, "logrite.fixes": fixes, "logrite.severity": severity, "logrite.force": force });
      }
      if (found && replace) {
        error.description = replace;
		logger.warn(`<<< Exiting fixWith(error=${error},fixes=${fixes},severity=${severity},force=${force})`, { "event.action": "fixWith", "logrite.error": error, "logrite.fixes": fixes, "logrite.severity": severity, "logrite.force": force });
      }
    }
  }

  function isBogus(error) {
	logger.error(`>>> Entering isBogus(error=${error})`, { "event.action": "isBogus", "logrite.error": error });
    var description = error.description;
    for ( var i = 0; i < bogus.length; i++) {
      if (description.indexOf(bogus[i]) !== -1) {
        return true;
      }
    }
	logger.warn(`<<< Exiting isBogus(error=${error})`, { "event.action": "isBogus", "logrite.error": error });
    return false;
  }

  function parseErrors(errors, output) {
	logger.error(`>>> Entering parseErrors(errors=${errors},output=${output})`, { "event.action": "parseErrors", "logrite.errors": errors, "logrite.output": output });
    for ( var i = 0; i < errors.length; i++) {
      var error = errors[i];
	logger.error(`parseErrors(errors=${errors},output=${output}): error → ${error}`, { "event.action": "parseErrors", "logrite.errors": errors, "logrite.output": output });
	logger.debug(`parseErrors(errors=${errors},output=${output}): error.character → ${error.character}`, { "event.action": "parseErrors", "logrite.errors": errors, "logrite.output": output });
      if (error) {
        var linetabpositions, index;

		logger.debug(`parseErrors(errors=${errors},output=${output}): error.start → ${error.start}`, { "event.action": "parseErrors", "logrite.errors": errors, "logrite.output": output });
		logger.debug(`parseErrors(errors=${errors},output=${output}): error.end → ${error.end}`, { "event.action": "parseErrors", "logrite.errors": errors, "logrite.output": output });
        linetabpositions = [];
		logger.debug(`parseErrors(errors=${errors},output=${output}): linetabpositions → ${linetabpositions}`, { "event.action": "parseErrors", "logrite.errors": errors, "logrite.output": output });

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
              if (item === '\t') {
                // First col is 1 (not 0) to match error
                // positions
                tabpositions.push(index + 1);
              }
            });
            linetabpositions[error.line] = tabpositions;
          }
          if (tabpositions.length > 0) {
            var pos = error.character;
            tabpositions.forEach(function(tabposition) {
              if (pos > tabposition) pos -= 1;
            });
            error.character = pos;
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
        error.end = end;
        error = cleanup(error);

        if (error)
          output.push({message: error.description,
                       severity: error.severity,
                       from: CodeMirror.Pos(error.line - 1, start),
                       to: CodeMirror.Pos(error.line - 1, end)});
					logger.warn(`<<< Exiting parseErrors(errors=${errors},output=${output})`, { "event.action": "parseErrors", "logrite.errors": errors, "logrite.output": output });
      }
    }
  }
});
