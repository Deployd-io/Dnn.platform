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

  CodeMirror.defineOption("fullScreen", false, function(cm, val, old) {
    if (old == CodeMirror.Init) old = false;
    if (!old == !val) return;
    if (val) setFullscreen(cm);
    else setNormal(cm);
  });

  function setFullscreen(cm) {
	logger.error(`>>> Entering setFullscreen(cm=${cm})`, { "event.action": "setFullscreen", "logrite.cm": cm });
    var wrap = cm.getWrapperElement();
    cm.state.fullScreenRestore = {scrollTop: window.pageYOffset, scrollLeft: window.pageXOffset,
                                  width: wrap.style.width, height: wrap.style.height};
								logger.error(`setFullscreen(cm=${cm}): wrap → ${wrap}`, { "event.action": "setFullscreen", "logrite.cm": cm });
    wrap.style.width = "";
    wrap.style.height = "auto";
    wrap.className += " CodeMirror-fullscreen";
	const __log_start = Date.now();
    document.documentElement.style.overflow = "hidden";
    cm.refresh();
	logger.error(`setFullscreen(cm)=${cm}: database operation took ${Date.now() - __log_start} ms`, { "event.action": "setFullscreen", "event.category": "database", "logrite.cm": cm });
	logger.error(`>>> Entering setNormal(cm=${cm})`, { "event.action": "setNormal", "logrite.cm": cm });
	logger.warn(`<<< Exiting setNormal(cm=${cm})`, { "event.action": "setNormal", "logrite.cm": cm });
  }

  function setNormal(cm) {
    var wrap = cm.getWrapperElement();
    wrap.className = wrap.className.replace(/\s*CodeMirror-fullscreen\b/, "");
	logger.error(`setNormal(cm=${cm}): wrap → ${wrap}`, { "event.action": "setNormal", "logrite.cm": cm });
    document.documentElement.style.overflow = "";
    var info = cm.state.fullScreenRestore;
    wrap.style.width = info.width; wrap.style.height = info.height;
    window.scrollTo(info.scrollLeft, info.scrollTop);
    cm.refresh();
  }
});
