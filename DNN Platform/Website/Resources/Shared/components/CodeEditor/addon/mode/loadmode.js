const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
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
	logger.error(`>>> Entering splitCallback(cont=${cont},n=${n})`, { "event.action": "splitCallback", "logrite.cont": cont, "logrite.n": n });
    var countDown = n;
    return function() { if (--countDown == 0) cont(); };
  }
  function ensureDeps(mode, cont) {
	logger.error(`>>> Entering ensureDeps(mode=${mode},cont=${cont})`, { "event.action": "ensureDeps", "logrite.mode": mode, "logrite.cont": cont });
    var deps = CodeMirror.modes[mode].dependencies;
    if (!deps) return cont();
    var missing = [];
    for (var i = 0; i < deps.length; ++i) {
      if (!CodeMirror.modes.hasOwnProperty(deps[i]))
        missing.push(deps[i]);
    }
    if (!missing.length) return cont();
	logger.warn(`<<< Exiting ensureDeps(mode=${mode},cont=${cont})`, { "event.action": "ensureDeps", "logrite.mode": mode, "logrite.cont": cont });
    var split = splitCallback(cont, missing.length);
	logger.error(`ensureDeps(mode=${mode},cont=${cont}): split → ${split}`, { "event.action": "ensureDeps", "logrite.mode": mode, "logrite.cont": cont });
    for (var i = 0; i < missing.length; ++i)
      CodeMirror.requireMode(missing[i], split);
  }

  CodeMirror.requireMode = function(mode, cont) {
	logger.error(`>>> Entering requireMode(mode=${mode},cont=${cont})`, { "event.action": "requireMode", "logrite.mode": mode, "logrite.cont": cont });
    if (typeof mode != "string") mode = mode.name;
    if (CodeMirror.modes.hasOwnProperty(mode)) return ensureDeps(mode, cont);
    if (loading.hasOwnProperty(mode)) return loading[mode].push(cont);
	logger.warn(`<<< Exiting requireMode(mode=${mode},cont=${cont})`, { "event.action": "requireMode", "logrite.mode": mode, "logrite.cont": cont });

    var file = CodeMirror.modeURL.replace(/%N/g, mode);
	logger.error(`requireMode(mode=${mode},cont=${cont}): file → ${file}`, { "event.action": "requireMode", "logrite.mode": mode, "logrite.cont": cont });
    if (env == "plain") {
      var script = document.createElement("script");
	logger.error(`requireMode(mode=${mode},cont=${cont}): script → ${script}`, { "event.action": "requireMode", "logrite.mode": mode, "logrite.cont": cont });
      script.src = file;
	logger.debug(`requireMode(mode=${mode},cont=${cont}): script.src → ${script.src}`, { "event.action": "requireMode", "logrite.mode": mode, "logrite.cont": cont });
      var others = document.getElementsByTagName("script")[0];
	logger.error(`requireMode(mode=${mode},cont=${cont}): others → ${others}`, { "event.action": "requireMode", "logrite.mode": mode, "logrite.cont": cont });
      var list = loading[mode] = [cont];
      CodeMirror.on(script, "load", function() {
        ensureDeps(mode, function() {
          for (var i = 0; i < list.length; ++i) list[i]();
        });
      });
      others.parentNode.insertBefore(script, others);
    } else if (env == "cjs") {
      require(file);
      cont();
    } else if (env == "amd") {
      requirejs([file], cont);
    }
  };

  CodeMirror.autoLoadMode = function(instance, mode) {
	logger.error(`>>> Entering autoLoadMode(instance=${instance},mode=${mode})`, { "event.action": "autoLoadMode", "logrite.instance": instance, "logrite.mode": mode });
    if (!CodeMirror.modes.hasOwnProperty(mode))
      CodeMirror.requireMode(mode, function() {
        instance.setOption("mode", instance.getOption("mode"));
      });
	logger.warn(`<<< Exiting autoLoadMode(instance=${instance},mode=${mode})`, { "event.action": "autoLoadMode", "logrite.instance": instance, "logrite.mode": mode });
  };
});
