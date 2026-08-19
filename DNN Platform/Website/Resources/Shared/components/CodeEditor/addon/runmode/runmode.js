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

CodeMirror.runMode = function(string, modespec, callback, options) {
  var mode = CodeMirror.getMode(CodeMirror.defaults, modespec);
logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): mode → ${mode}`);
  var ie = /MSIE \d/.test(navigator.userAgent);
logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): ie → ${ie}`);
  var ie_lt9 = ie && (document.documentMode == null || document.documentMode < 9);
logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): ie_lt9 → ${ie_lt9}`);

  if (callback.nodeType == 1) {
    var tabSize = (options && options.tabSize) || CodeMirror.defaults.tabSize;
	logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): tabSize → ${tabSize}`);
    var node = callback, col = 0;
    node.innerHTML = "";
	logger.debug(`function(exports=${exports}): node.innerHTML → ${node.innerHTML}`);
	logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): node.innerHTML → ${node.innerHTML}`);
    callback = function(text, style) {
      if (text == "\n") {
        // Emitting LF or CRLF on IE8 or earlier results in an incorrect display.
        // Emitting a carriage return makes everything ok.
        node.appendChild(document.createTextNode(ie_lt9 ? '\r' : text));
        col = 0;
        return;
      }
      var content = "";
      // replace tabs
      for (var pos = 0;;) {
        var idx = text.indexOf("\t", pos);
        if (idx == -1) {
		logger.debug(`function(text=${text},style=${style}): content → ${content}`);
		logger.debug(`function(text=${text},style=${style}): idx → ${idx}`);
          content += text.slice(pos);
          col += text.length - pos;
          break;
        } else {
          col += idx - pos;
          content += text.slice(pos, idx);
          var size = tabSize - col % tabSize;
          col += size;
          for (var i = 0; i < size; ++i) content += " ";
          pos = idx + 1;
        }
      }

      if (style) {
        var sp = node.appendChild(document.createElement("span"));
		logger.debug(`function(text=${text},style=${style}): sp → ${sp}`);
        sp.className = "cm-" + style.replace(/ +/g, " cm-");
        sp.appendChild(document.createTextNode(content));
      } else {
        node.appendChild(document.createTextNode(content));
		logger.debug(`<<< Exiting function(text=${text},style=${style})`);
      }
    };
  }

  var lines = CodeMirror.splitLines(string), state = (options && options.state) || CodeMirror.startState(mode);
logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): lines → ${lines}`);
  for (var i = 0, e = lines.length; i < e; ++i) {
    if (i) callback("\n");
    var stream = new CodeMirror.StringStream(lines[i]);
	logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): stream → ${stream}`);
    if (!stream.string && mode.blankLine) mode.blankLine(state);
    while (!stream.eol()) {
      var style = mode.token(stream, state);
	logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): style → ${style}`);
      callback(stream.current(), style, i, stream.start, state);
      stream.start = stream.pos;
	logger.debug(`function(exports=${exports}): stream.start → ${stream.start}`);
	logger.debug(`<<< Exiting function(exports=${exports})`);
	logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): stream.start → ${stream.start}`);
    }
  }
};

});
