const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), "cjs");
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], function(CM) { mod(CM, "amd"); });
  else // Plain browser env
    mod(CodeMirror, "plain");
})(function(CodeMirror, env) {
  if (!CodeMirror.modeURL) CodeMirror.modeURL = "../mode/%N/%N.js";

  var loading = {};
  function splitCallback(cont, n) {
	logger.debug(`>>> Entering splitCallback(cont=${cont},n=${n})`);
    var countDown = n;
	logger.debug(`<<< Exiting splitCallback(cont=${cont},n=${n})`);
    return function() { if (--countDown == 0) cont(); };
  }
  function ensureDeps(mode, cont) {
	logger.debug(`>>> Entering ensureDeps(mode=${mode},cont=${cont})`);
    var deps = CodeMirror.modes[mode].dependencies;
    if (!deps) return cont();
    var missing = [];
    for (var i = 0; i < deps.length; ++i) {
      if (!CodeMirror.modes.hasOwnProperty(deps[i]))
        missing.push(deps[i]);
    }
    if (!missing.length) return cont();
	logger.debug(`<<< Exiting ensureDeps(mode=${mode},cont=${cont})`);
    var split = splitCallback(cont, missing.length);
	logger.debug(`ensureDeps(mode=${mode},cont=${cont}): split → ${split}`);
    for (var i = 0; i < missing.length; ++i)
      CodeMirror.requireMode(missing[i], split);
  }

  CodeMirror.requireMode = function(mode, cont) {
    if (typeof mode != "string") mode = mode.name;
    if (CodeMirror.modes.hasOwnProperty(mode)) return ensureDeps(mode, cont);
    if (loading.hasOwnProperty(mode)) return loading[mode].push(cont);

    var file = CodeMirror.modeURL.replace(/%N/g, mode);
	logger.debug(`function(mode=${mode},cont=${cont}): file → ${file}`);
    if (env == "plain") {
      var script = document.createElement("script");
	logger.debug(`function(mode=${mode},cont=${cont}): script → ${script}`);
      script.src = file;
      var others = document.getElementsByTagName("script")[0];
	logger.debug(`function(mode=${mode},cont=${cont}): others → ${others}`);
      var list = loading[mode] = [cont];
      CodeMirror.on(script, "load", function() {
		logger.debug(`>>> Entering on(script=${script},i=${i})`);
        ensureDeps(mode, function() {
          for (var i = 0; i < list.length; ++i) list[i]();
		logger.debug(`>>> Entering ensureDeps(mode=${mode},i=${i})`);
		logger.debug(`<<< Exiting on(script=${script},i=${i})`);
        });
      });
	logger.debug(`<<< Exiting ensureDeps(mode=${mode},i=${i})`);
      others.parentNode.insertBefore(script, others);
    } else if (env == "cjs") {
      require(file);
      cont();
    } else if (env == "amd") {
      requirejs([file], cont);
    }
	logger.debug(`>>> Entering requireMode(mode=${mode})`);
	logger.debug(`<<< Exiting requireMode(mode=${mode})`);
  };

  CodeMirror.autoLoadMode = function(instance, mode) {
    if (!CodeMirror.modes.hasOwnProperty(mode))
      CodeMirror.requireMode(mode, function() {
        instance.setOption("mode", instance.getOption("mode"));
        logger.debug(`<<< Exiting function(instance=${instance},mode=${mode})`);
      });
  };
});
