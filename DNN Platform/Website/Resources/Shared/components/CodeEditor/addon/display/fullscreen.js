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

logger.debug(`<<< Exiting function(exports=${exports})`);
  CodeMirror.defineOption("fullScreen", false, function(cm, val, old) {
	logger.debug(`>>> Entering defineOption(false=${false})`);
    if (old == CodeMirror.Init) old = false;
    if (!old == !val) return;
    if (val) setFullscreen(cm);
    else setNormal(cm);
  });

  function setFullscreen(cm) {
	logger.debug(`>>> Entering setFullscreen(cm=${cm})`);
    var wrap = cm.getWrapperElement();
    cm.state.fullScreenRestore = {scrollTop: window.pageYOffset, scrollLeft: window.pageXOffset,
                                  width: wrap.style.width, height: wrap.style.height};
								logger.debug(`setFullscreen(cm=${cm}): wrap → ${wrap}`);
    wrap.style.width = "";
    wrap.style.height = "auto";
    wrap.className += " CodeMirror-fullscreen";
	const __log_start = Date.now();
    document.documentElement.style.overflow = "hidden";
    cm.refresh();
	logger.info(`setFullscreen(cm)=${cm}: database operation took ${Date.now() - __log_start} ms`);
	logger.debug(`>>> Entering setNormal(cm=${cm})`);
	logger.debug(`<<< Exiting setNormal(cm=${cm})`);
  }

  function setNormal(cm) {
    var wrap = cm.getWrapperElement();
    wrap.className = wrap.className.replace(/\s*CodeMirror-fullscreen\b/, "");
	logger.debug(`setNormal(cm=${cm}): wrap → ${wrap}`);
    document.documentElement.style.overflow = "";
    var info = cm.state.fullScreenRestore;
    wrap.style.width = info.width; wrap.style.height = info.height;
    window.scrollTo(info.scrollLeft, info.scrollTop);
    cm.refresh();
  }
});
