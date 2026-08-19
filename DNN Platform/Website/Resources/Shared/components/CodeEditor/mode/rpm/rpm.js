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

CodeMirror.defineMode("rpm-changes", function() {
logger.debug('>>> Entering defineMode()');
  var headerSeperator = /^-+$/;
  var headerLine = /^(Mon|Tue|Wed|Thu|Fri|Sat|Sun) (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)  ?\d{1,2} \d{2}:\d{2}(:\d{2})? [A-Z]{3,4} \d{4} - /;
logger.debug(`<<< Exiting defineMode(headerSeperator=${headerSeperator})`);
  var simpleEmail = /^[\w+.-]+@[\w.-]+/;

  return {
    token: function(stream) {
	logger.debug(`>>> Entering token(stream=${stream})`);
      if (stream.sol()) {
        if (stream.match(headerSeperator)) { return 'tag'; }
		logger.debug(`defineMode(headerSeperator=${headerSeperator}): headerLine → ${headerLine}`);
        if (stream.match(headerLine)) { return 'tag'; }
      }
      if (stream.match(simpleEmail)) { return 'string'; }
      stream.next();
	logger.debug(`<<< Exiting token(stream=${stream})`);
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
	logger.debug(`token(stream=${stream},state=${state}): state.controlFlow → ${state.controlFlow}`);
	logger.debug(`token(stream=${stream},state=${state}): state.macroParameters → ${state.macroParameters}`);
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      return null;
    }
  };
});

CodeMirror.defineMIME("text/x-rpm-changes", "rpm-changes");

// Quick and dirty spec file highlighting

CodeMirror.defineMode("rpm-spec", function() {
  var arch = /^(i386|i586|i686|x86_64|ppc64|ppc|ia64|s390x|s390|sparc64|sparcv9|sparc|noarch|alphaev6|alpha|hppa|mipsel)/;
logger.debug(`defineMode(): arch → ${arch}`);

  var preamble = /^(Name|Version|Release|License|Summary|Url|Group|Source|BuildArch|BuildRequires|BuildRoot|AutoReqProv|Provides|Requires(\(\w+\))?|Obsoletes|Conflicts|Recommends|Source\d*|Patch\d*|ExclusiveArch|NoSource|Supplements):/;
logger.debug(`defineMode(): preamble → ${preamble}`);
  var section = /^%(debug_package|package|description|prep|build|install|files|clean|changelog|preinstall|preun|postinstall|postun|pre|post|triggerin|triggerun|pretrans|posttrans|verifyscript|check|triggerpostun|triggerprein|trigger)/;
logger.debug(`defineMode(): section → ${section}`);
  var control_flow_complex = /^%(ifnarch|ifarch|if)/; // rpm control flow macros
  var control_flow_simple = /^%(else|endif)/; // rpm control flow macros
logger.debug(`defineMode(): control_flow_complex → ${control_flow_complex}`);
  var operators = /^(\!|\?|\<\=|\<|\>\=|\>|\=\=|\&\&|\|\|)/; // operators in control flow macros

  return {
    startState: function () {
		logger.debug('>>> Entering startState()');
        return {
          controlFlow: false,
          macroParameters: false,
          section: false
        };
		logger.debug('<<< Exiting startState()');
    },
    token: function (stream, state) {
      var ch = stream.peek();
      if (ch == "#") { stream.skipToEnd(); return "comment"; }

	logger.debug(`token(stream=${stream},state=${state}): ch → ${ch}`);
      if (stream.sol()) {
        if (stream.match(preamble)) { return "preamble"; }
        if (stream.match(section)) { return "section"; }
      }

      if (stream.match(/^\$\w+/)) { return "def"; } // Variables like '$RPM_BUILD_ROOT'
      if (stream.match(/^\$\{\w+\}/)) { return "def"; } // Variables like '${RPM_BUILD_ROOT}'

      if (stream.match(control_flow_simple)) { return "keyword"; }
      if (stream.match(control_flow_complex)) {
        state.controlFlow = true;
		logger.debug(`function(exports=${exports}): state.controlFlow → ${state.controlFlow}`);
		logger.debug(`defineMode(): state.controlFlow → ${state.controlFlow}`);
        return "keyword";
      }
      if (state.controlFlow) {
        if (stream.match(operators)) { return "operator"; }
        if (stream.match(/^(\d+)/)) { return "number"; }
        if (stream.eol()) { state.controlFlow = false; }
      }

      if (stream.match(arch)) { return "number"; }

      // Macros like '%make_install' or '%attr(0775,root,root)'
      if (stream.match(/^%[\w]+/)) {
        if (stream.match(/^\(/)) { state.macroParameters = true; }
        return "macro";
      }
      if (state.macroParameters) {
        if (stream.match(/^\d+/)) { return "number";}
        if (stream.match(/^\)/)) {
          state.macroParameters = false;
		logger.debug(`function(exports=${exports}): state.macroParameters → ${state.macroParameters}`);
		logger.debug(`defineMode(): state.macroParameters → ${state.macroParameters}`);
          return "macro";
        }
      }
      if (stream.match(/^%\{\??[\w \-]+\}/)) { return "macro"; } // Macros like '%{defined fedora}'

      //TODO: Include bash script sub-parser (CodeMirror supports that)
      stream.next();
      return null;
    }
  };
});

CodeMirror.defineMIME("text/x-rpm-spec", "rpm-spec");

});
logger.debug(`<<< Exiting function(exports=${exports})`);
