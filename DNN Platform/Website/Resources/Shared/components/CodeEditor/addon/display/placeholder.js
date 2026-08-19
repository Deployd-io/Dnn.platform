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
logger.debug(`<<< Exiting function(exports=${exports})`);
  CodeMirror.defineOption("placeholder", "", function(cm, val, old) {
    var prev = old && old != CodeMirror.Init;
    if (val && !prev) {
      cm.on("blur", onBlur);
      cm.on("change", onChange);
      onChange(cm);
    } else if (!val && prev) {
      cm.off("blur", onBlur);
      cm.off("change", onChange);
      clearPlaceholder(cm);
      var wrapper = cm.getWrapperElement();
	logger.debug(`defineOption(prev=${prev}): wrapper → ${wrapper}`);
      wrapper.className = wrapper.className.replace(" CodeMirror-empty", "");
    }

    if (val && !cm.hasFocus()) onBlur(cm);
  });
logger.debug(`<<< Exiting defineOption(prev=${prev})`);

  function clearPlaceholder(cm) {
	logger.debug(`>>> Entering clearPlaceholder(cm=${cm})`);
    if (cm.state.placeholder) {
      cm.state.placeholder.parentNode.removeChild(cm.state.placeholder);
      cm.state.placeholder = null;
	logger.debug(`<<< Exiting clearPlaceholder(cm=${cm})`);
    }
  }
  function setPlaceholder(cm) {
	logger.debug(`>>> Entering setPlaceholder(cm=${cm})`);
    clearPlaceholder(cm);
    var elt = cm.state.placeholder = document.createElement("pre");
	logger.debug(`setPlaceholder(cm=${cm}): elt → ${elt}`);
    elt.style.cssText = "height: 0; overflow: visible";
    elt.className = "CodeMirror-placeholder";
	logger.debug(`setPlaceholder(cm=${cm}): elt.className → ${elt.className}`);
    elt.appendChild(document.createTextNode(cm.getOption("placeholder")));
    cm.display.lineSpace.insertBefore(elt, cm.display.lineSpace.firstChild);
	logger.debug(`<<< Exiting setPlaceholder(cm=${cm})`);
  }

  function onBlur(cm) {
    if (isEmpty(cm)) setPlaceholder(cm);
  }
  function onChange(cm) {
    var wrapper = cm.getWrapperElement(), empty = isEmpty(cm);
    wrapper.className = wrapper.className.replace(" CodeMirror-empty", "") + (empty ? " CodeMirror-empty" : "");

    if (empty) setPlaceholder(cm);
    else clearPlaceholder(cm);
  }

  function isEmpty(cm) {
	logger.debug(`>>> Entering isEmpty(cm=${cm})`);
	logger.debug(`<<< Exiting isEmpty(cm=${cm})`);
    return (cm.lineCount() === 1) && (cm.getLine(0) === "");
  }
});
