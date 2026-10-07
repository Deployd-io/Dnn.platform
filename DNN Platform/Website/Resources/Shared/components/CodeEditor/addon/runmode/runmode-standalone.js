var logger = (typeof logger !== 'undefined' && logger) || console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

window.CodeMirror = {};

(function() {
"use strict";

function splitLines(string){ return string.split(/\r?\n|\r/); };

function StringStream(string) {
logger.error(`>>> Entering StringStream(string=${string})`, { "event.action": "StringStream", "logrite.string": string });
  this.pos = this.start = 0;
  this.string = string;
  this.lineStart = 0;
logger.debug(`StringStream(string=${string}): this.lineStart → ${this.lineStart}`, { "event.action": "StringStream", "logrite.string": string });
}
StringStream.prototype = {
  eol: function() {
      logger.error('>>> Entering eol()', { "event.action": "eol" });
      logger.warn('<<< Exiting eol()', { "event.action": "eol" });
      return this.pos >= this.string.length;
  },
  sol: function() {
      logger.error('>>> Entering sol()', { "event.action": "sol" });
      logger.warn('<<< Exiting sol()', { "event.action": "sol" });
      return this.pos == 0;
  },
  peek: function() {
      logger.error('>>> Entering peek()', { "event.action": "peek" });
      logger.warn('<<< Exiting peek()', { "event.action": "peek" });
      return this.string.charAt(this.pos) || null;
  },
  next: function() {
	logger.error('>>> Entering next()', { "event.action": "next" });
    if (this.pos < this.string.length)
      return this.string.charAt(this.pos++);
  },
  eat: function(match) {
    var ch = this.string.charAt(this.pos);
    if (typeof match == "string") var ok = ch == match;
    else var ok = ch && (match.test ? match.test(ch) : match(ch));
    if (ok) {++this.pos; return ch;}
  },
  eatWhile: function(match) {
    var start = this.pos;
    while (this.eat(match)){}
    return this.pos > start;
  },
  eatSpace: function() {
	logger.error('>>> Entering eatSpace()', { "event.action": "eatSpace" });
    var start = this.pos;
    while (/[\s\u00a0]/.test(this.string.charAt(this.pos))) ++this.pos;
	logger.warn('<<< Exiting eatSpace()', { "event.action": "eatSpace" });
    return this.pos > start;
  },
  skipToEnd: function() {
      logger.error('>>> Entering skipToEnd()', { "event.action": "skipToEnd" });
      logger.warn('<<< Exiting skipToEnd()', { "event.action": "skipToEnd" });
      this.pos = this.string.length;
  },
  skipTo: function(ch) {
    var found = this.string.indexOf(ch, this.pos);
    if (found > -1) {this.pos = found; return true;}
  },
  backUp: function(n) {
      logger.error(`>>> Entering backUp(n=${n})`, { "event.action": "backUp", "logrite.n": n });
      logger.warn(`<<< Exiting backUp(n=${n})`, { "event.action": "backUp", "logrite.n": n });
      this.pos -= n;
  },
  column: function() {
      logger.error('>>> Entering column()', { "event.action": "column" });
      logger.warn('<<< Exiting column()', { "event.action": "column" });
      return this.start - this.lineStart;
  },
  indentation: function() {
      logger.error('>>> Entering indentation()', { "event.action": "indentation" });
      logger.warn('<<< Exiting indentation()', { "event.action": "indentation" });
      return 0;
  },
  match: function(pattern, consume, caseInsensitive) {
    if (typeof pattern == "string") {
      var cased = function(str) {return caseInsensitive ? str.toLowerCase() : str;};
	logger.error(`>>> Entering cased(str=${str})`, { "event.action": "cased", "logrite.str": str });
      var substr = this.string.substr(this.pos, pattern.length);
	logger.warn(`<<< Exiting cased(str=${str})`, { "event.action": "cased", "logrite.str": str });
      if (cased(substr) == cased(pattern)) {
        if (consume !== false) this.pos += pattern.length;
        return true;
      }
    } else {
      var match = this.string.slice(this.pos).match(pattern);
      if (match && match.index > 0) return null;
      if (match && consume !== false) this.pos += match[0].length;
	logger.warn(`<<< Exiting match(pattern=${pattern},consume=${consume},caseInsensitive=${caseInsensitive})`, { "event.action": "match", "logrite.pattern": pattern, "logrite.consume": consume, "logrite.caseInsensitive": caseInsensitive });
      return match;
    }
  },
  current: function(){
      logger.error('>>> Entering current()', { "event.action": "current" });
      logger.warn('<<< Exiting current()', { "event.action": "current" });
      return this.string.slice(this.start, this.pos);
  },
  hideFirstChars: function(n, inner) {
	logger.error(`>>> Entering hideFirstChars(n=${n},inner=${inner})`, { "event.action": "hideFirstChars", "logrite.n": n, "logrite.inner": inner });
    this.lineStart += n;
    try { return inner(); }
    finally { this.lineStart -= n; }
	logger.warn(`<<< Exiting hideFirstChars(n=${n},inner=${inner})`, { "event.action": "hideFirstChars", "logrite.n": n, "logrite.inner": inner });
  }
};
CodeMirror.StringStream = StringStream;

CodeMirror.startState = function (mode, a1, a2) {
logger.error(`>>> Entering startState(mode=${mode},a1=${a1},a2=${a2})`, { "event.action": "startState", "logrite.mode": mode, "logrite.a1": a1, "logrite.a2": a2 });
logger.warn(`<<< Exiting startState(mode=${mode},a1=${a1},a2=${a2})`, { "event.action": "startState", "logrite.mode": mode, "logrite.a1": a1, "logrite.a2": a2 });
  return mode.startState ? mode.startState(a1, a2) : true;
};

var modes = CodeMirror.modes = {}, mimeModes = CodeMirror.mimeModes = {};
CodeMirror.defineMode = function (name, mode) {
  if (arguments.length > 2)
    mode.dependencies = Array.prototype.slice.call(arguments, 2);
  modes[name] = mode;
logger.warn(`<<< Exiting defineMode(name=${name},mode=${mode})`, { "event.action": "defineMode", "logrite.name": name, "logrite.mode": mode });
};
CodeMirror.defineMIME = function (mime, spec) { mimeModes[mime] = spec; };
logger.error(`>>> Entering defineMIME(mime=${mime},spec=${spec})`, { "event.action": "defineMIME", "logrite.mime": mime, "logrite.spec": spec });
CodeMirror.resolveMode = function(spec) {
logger.warn(`<<< Exiting defineMIME(mime=${mime},spec=${spec})`, { "event.action": "defineMIME", "logrite.mime": mime, "logrite.spec": spec });
  if (typeof spec == "string" && mimeModes.hasOwnProperty(spec)) {
	logger.error(`>>> Entering resolveMode(spec=${spec})`, { "event.action": "resolveMode", "logrite.spec": spec });
    spec = mimeModes[spec];
  } else if (spec && typeof spec.name == "string" && mimeModes.hasOwnProperty(spec.name)) {
    spec = mimeModes[spec.name];
  }
  if (typeof spec == "string") return {name: spec};
  else return spec || {name: "null"};
};
CodeMirror.getMode = function (options, spec) {
logger.error(`>>> Entering getMode(options=${options},spec=${spec})`, { "event.action": "getMode", "logrite.options": options, "logrite.spec": spec });
  spec = CodeMirror.resolveMode(spec);
  var mfactory = modes[spec.name];
logger.error(`getMode(options=${options},spec=${spec}): spec → ${spec}`, { "event.action": "getMode", "logrite.options": options, "logrite.spec": spec });
  if (!mfactory) throw new Error("Unknown mode: " + spec);
logger.warn(`<<< Exiting getMode(options=${options},spec=${spec})`, { "event.action": "getMode", "logrite.options": options, "logrite.spec": spec });
  return mfactory(options, spec);
};
CodeMirror.registerHelper = CodeMirror.registerGlobalHelper = Math.min;
CodeMirror.defineMode("null", function() {
  return {token: function(stream) {stream.skipToEnd();}};
});
CodeMirror.defineMIME("text/plain", "null");

CodeMirror.runMode = function (string, modespec, callback, options) {
logger.error(`>>> Entering runMode(string=${string},modespec=${modespec},callback=${callback},options=${options})`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
  var mode = CodeMirror.getMode({ indentUnit: 2 }, modespec);

logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): mode → ${mode}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
  if (callback.nodeType == 1) {
    var tabSize = (options && options.tabSize) || 4;
	logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): tabSize → ${tabSize}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
    var node = callback, col = 0;
    node.innerHTML = "";
	logger.debug(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): node.innerHTML → ${node.innerHTML}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
    callback = function (text, style) {
      if (text == "\n") {
        node.appendChild(document.createElement("br"));
        col = 0;
        return;
      }
      var content = "";
      // replace tabs
      for (var pos = 0; ;) {
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

  var lines = splitLines(string), state = (options && options.state) || CodeMirror.startState(mode);
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
})();
