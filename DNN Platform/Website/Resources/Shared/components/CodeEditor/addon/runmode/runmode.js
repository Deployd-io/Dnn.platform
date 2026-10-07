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

CodeMirror.runMode = function(string, modespec, callback, options) {
logger.error(`>>> Entering runMode(string=${string},modespec=${modespec},callback=${callback},options=${options})`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
  var mode = CodeMirror.getMode(CodeMirror.defaults, modespec);
  var ie = /MSIE \d/.test(navigator.userAgent);
logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): mode → ${mode}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): ie → ${ie}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
  var ie_lt9 = ie && (document.documentMode == null || document.documentMode < 9);

logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): ie_lt9 → ${ie_lt9}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
  if (callback.nodeType == 1) {
    var tabSize = (options && options.tabSize) || CodeMirror.defaults.tabSize;
	logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): tabSize → ${tabSize}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
    var node = callback, col = 0;
    node.innerHTML = "";
	logger.debug(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): node.innerHTML → ${node.innerHTML}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
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
        sp.className = "cm-" + style.replace(/ +/g, " cm-");
        sp.appendChild(document.createTextNode(content));
      } else {
        node.appendChild(document.createTextNode(content));
      }
	logger.warn(`<<< Exiting callback(text=${text},style=${style})`, { "event.action": "callback", "logrite.text": text, "logrite.style": style });
    };
  }

  var lines = CodeMirror.splitLines(string), state = (options && options.state) || CodeMirror.startState(mode);
logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): lines → ${lines}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
  for (var i = 0, e = lines.length; i < e; ++i) {
    if (i) callback("\n");
    var stream = new CodeMirror.StringStream(lines[i]);
	logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): stream → ${stream}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
    if (!stream.string && mode.blankLine) mode.blankLine(state);
    while (!stream.eol()) {
      var style = mode.token(stream, state);
	logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): style → ${style}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
      callback(stream.current(), style, i, stream.start, state);
      stream.start = stream.pos;
	logger.debug(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): stream.start → ${stream.start}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
    }
  }
};

});
