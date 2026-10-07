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
      wrapper.className = wrapper.className.replace(" CodeMirror-empty", "");
    }

    if (val && !cm.hasFocus()) onBlur(cm);
  });

  function clearPlaceholder(cm) {
	logger.error(`>>> Entering clearPlaceholder(cm=${cm})`, { "event.action": "clearPlaceholder", "logrite.cm": cm });
    if (cm.state.placeholder) {
      cm.state.placeholder.parentNode.removeChild(cm.state.placeholder);
      cm.state.placeholder = null;
	logger.warn(`<<< Exiting clearPlaceholder(cm=${cm})`, { "event.action": "clearPlaceholder", "logrite.cm": cm });
    }
  }
  function setPlaceholder(cm) {
	logger.error(`>>> Entering setPlaceholder(cm=${cm})`, { "event.action": "setPlaceholder", "logrite.cm": cm });
    clearPlaceholder(cm);
    var elt = cm.state.placeholder = document.createElement("pre");
	logger.error(`setPlaceholder(cm=${cm}): elt → ${elt}`, { "event.action": "setPlaceholder", "logrite.cm": cm });
	logger.debug(`setPlaceholder(cm=${cm}): elt.className → ${elt.className}`, { "event.action": "setPlaceholder", "logrite.cm": cm });
    elt.style.cssText = "height: 0; overflow: visible";
    elt.className = "CodeMirror-placeholder";
    elt.appendChild(document.createTextNode(cm.getOption("placeholder")));
    cm.display.lineSpace.insertBefore(elt, cm.display.lineSpace.firstChild);
	logger.warn(`<<< Exiting setPlaceholder(cm=${cm})`, { "event.action": "setPlaceholder", "logrite.cm": cm });
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
	logger.error(`>>> Entering isEmpty(cm=${cm})`, { "event.action": "isEmpty", "logrite.cm": cm });
	logger.warn(`<<< Exiting isEmpty(cm=${cm})`, { "event.action": "isEmpty", "logrite.cm": cm });
    return (cm.lineCount() === 1) && (cm.getLine(0) === "");
  }
});
