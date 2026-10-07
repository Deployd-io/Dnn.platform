const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("./runmode"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "./runmode"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

  var isBlock = /^(p|li|div|h\\d|pre|blockquote|td)$/;

  function textContent(node, out) {
	logger.error(`>>> Entering textContent(node=${node},out=${out})`, { "event.action": "textContent", "logrite.node": node, "logrite.out": out });
    if (node.nodeType == 3) return out.push(node.nodeValue);
	logger.warn(`<<< Exiting textContent(node=${node},out=${out})`, { "event.action": "textContent", "logrite.node": node, "logrite.out": out });
    for (var ch = node.firstChild; ch; ch = ch.nextSibling) {
      textContent(ch, out);
      if (isBlock.test(node.nodeType)) out.push("\n");
    }
  }

  CodeMirror.colorize = function(collection, defaultMode) {
	logger.error(`>>> Entering colorize(collection=${collection},defaultMode=${defaultMode})`, { "event.action": "colorize", "logrite.collection": collection, "logrite.defaultMode": defaultMode });
    if (!collection) collection = document.body.getElementsByTagName("pre");

    for (var i = 0; i < collection.length; ++i) {
      var node = collection[i];
	logger.debug(`colorize(collection=${collection},defaultMode=${defaultMode}): node.innerHTML → ${node.innerHTML}`, { "event.action": "colorize", "logrite.collection": collection, "logrite.defaultMode": defaultMode });
      var mode = node.getAttribute("data-lang") || defaultMode;
	logger.error(`colorize(collection=${collection},defaultMode=${defaultMode}): mode → ${mode}`, { "event.action": "colorize", "logrite.collection": collection, "logrite.defaultMode": defaultMode });
      if (!mode) continue;

      var text = [];
      textContent(node, text);
      node.innerHTML = "";
      CodeMirror.runMode(text.join(""), mode, node);

      node.className += " cm-s-default";
	logger.warn(`<<< Exiting colorize(collection=${collection},defaultMode=${defaultMode})`, { "event.action": "colorize", "logrite.collection": collection, "logrite.defaultMode": defaultMode });
    }
  };
});
