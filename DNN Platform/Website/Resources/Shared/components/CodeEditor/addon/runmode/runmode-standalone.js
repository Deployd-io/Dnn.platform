const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

window.CodeMirror = {};

(function() {
logger.debug('>>> Entering function()');
"use strict";

function splitLines(string){ return string.split(/\r?\n|\r/); };

logger.debug('<<< Exiting function()');
function StringStream(string) {
logger.debug(`>>> Entering StringStream(string=${string})`);
  this.pos = this.start = 0;
  this.string = string;
logger.debug(`function(): this.pos → ${this.pos}`);
  this.lineStart = 0;
logger.debug(`StringStream(string=${string}): string → ${string}`);
logger.debug(`<<< Exiting StringStream(string=${string})`);
}
StringStream.prototype = {
  eol: function() {return this.pos >= this.string.length;},
  sol: function() {return this.pos == 0;},
  peek: function() {return this.string.charAt(this.pos) || null;},
  next: function() {
	logger.debug('>>> Entering next()');
    if (this.pos < this.string.length)
	logger.debug('<<< Exiting next()');
      return this.string.charAt(this.pos++);
  },
  eat: function(match) {
    var ch = this.string.charAt(this.pos);
    if (typeof match == "string") var ok = ch == match;
    else var ok = ch && (match.test ? match.test(ch) : match(ch));
    if (ok) {++this.pos; return ch;}
	logger.debug(`eat(match=${match}): ok → ${ok}`);
	logger.debug(`<<< Exiting eat(match=${match})`);
  },
  eatWhile: function(match) {
    var start = this.pos;
    while (this.eat(match)){}
	logger.debug(`<<< Exiting eatWhile(match=${match})`);
    return this.pos > start;
  },
  eatSpace: function() {
	logger.debug('>>> Entering eatSpace()');
    var start = this.pos;
    while (/[\s\u00a0]/.test(this.string.charAt(this.pos))) ++this.pos;
	logger.debug('<<< Exiting eatSpace()');
    return this.pos > start;
  },
  skipToEnd: function() {this.pos = this.string.length;},
  skipTo: function(ch) {
	logger.debug(`>>> Entering skipTo(ch=${ch})`);
    var found = this.string.indexOf(ch, this.pos);
    if (found > -1) {this.pos = found; return true;}
	logger.debug(`skipTo(ch=${ch}): found → ${found}`);
	logger.debug(`<<< Exiting skipTo(ch=${ch})`);
  },
  backUp: function(n) {this.pos -= n;},
  column: function() {return this.start - this.lineStart;},
  indentation: function() {return 0;},
  match: function(pattern, consume, caseInsensitive) {
	logger.debug(`>>> Entering match(pattern=${pattern},consume=${consume},caseInsensitive=${caseInsensitive})`);
    if (typeof pattern == "string") {
      var cased = function(str) {return caseInsensitive ? str.toLowerCase() : str;};
      var substr = this.string.substr(this.pos, pattern.length);
      if (cased(substr) == cased(pattern)) {
        if (consume !== false) this.pos += pattern.length;
		logger.debug(`match(pattern=${pattern},consume=${consume},caseInsensitive=${caseInsensitive}): substr → ${substr}`);
        return true;
      }
    } else {
      var match = this.string.slice(this.pos).match(pattern);
      if (match && match.index > 0) return null;
      if (match && consume !== false) this.pos += match[0].length;
	logger.debug(`<<< Exiting match(pattern=${pattern},consume=${consume},caseInsensitive=${caseInsensitive})`);
      return match;
    }
  },
  current: function(){return this.string.slice(this.start, this.pos);},
  hideFirstChars: function(n, inner) {
	logger.debug(`>>> Entering hideFirstChars(n=${n},inner=${inner})`);
    this.lineStart += n;
    try { return inner(); }
    finally { this.lineStart -= n; }
	logger.debug(`<<< Exiting hideFirstChars(n=${n},inner=${inner})`);
  }
};
CodeMirror.StringStream = StringStream;

CodeMirror.startState = function (mode, a1, a2) {
  return mode.startState ? mode.startState(a1, a2) : true;
};

var modes = CodeMirror.modes = {}, mimeModes = CodeMirror.mimeModes = {};
CodeMirror.defineMode = function (name, mode) {
  if (arguments.length > 2)
    mode.dependencies = Array.prototype.slice.call(arguments, 2);
  modes[name] = mode;
logger.debug('>>> Entering defineMode()');
logger.debug('<<< Exiting defineMode()');
};
CodeMirror.defineMIME = function (mime, spec) { mimeModes[mime] = spec; };
CodeMirror.resolveMode = function(spec) {
  if (typeof spec == "string" && mimeModes.hasOwnProperty(spec)) {
    spec = mimeModes[spec];
  } else if (spec && typeof spec.name == "string" && mimeModes.hasOwnProperty(spec.name)) {
    spec = mimeModes[spec.name];
  }
  if (typeof spec == "string") return {name: spec};
  else return spec || {name: "null"};
};
CodeMirror.getMode = function (options, spec) {
  spec = CodeMirror.resolveMode(spec);
  var mfactory = modes[spec.name];
logger.debug(`function(options=${options},spec=${spec}): spec → ${spec}`);
  if (!mfactory) throw new Error("Unknown mode: " + spec);
  return mfactory(options, spec);
};
CodeMirror.registerHelper = CodeMirror.registerGlobalHelper = Math.min;
CodeMirror.defineMode("null", function() {
  return {token: function(stream) {stream.skipToEnd();}};
});
CodeMirror.defineMIME("text/plain", "null");

CodeMirror.runMode = function (string, modespec, callback, options) {
  var mode = CodeMirror.getMode({ indentUnit: 2 }, modespec);
logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): mode → ${mode}`);

  if (callback.nodeType == 1) {
    var tabSize = (options && options.tabSize) || 4;
	logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): tabSize → ${tabSize}`);
    var node = callback, col = 0;
    node.innerHTML = "";
    callback = function (text, style) {
      if (text == "\n") {
        node.appendChild(document.createElement("br"));
		logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): node.innerHTML → ${node.innerHTML}`);
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

	logger.debug(`function(): pos → ${pos}`);
      if (style) {
        var sp = node.appendChild(document.createElement("span"));
        sp.className = "cm-" + style.replace(/ +/g, " cm-");
        sp.appendChild(document.createTextNode(content));
      } else {
        node.appendChild(document.createTextNode(content));
      }
    };
  }

  var lines = splitLines(string), state = (options && options.state) || CodeMirror.startState(mode);
logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): lines → ${lines}`);
  for (var i = 0, e = lines.length; i < e; ++i) {
    if (i) callback("\n");
    var stream = new CodeMirror.StringStream(lines[i]);
	logger.debug(`function(string=${string},modespec=${modespec},callback=${callback},options=${options}): stream → ${stream}`);
    if (!stream.string && mode.blankLine) mode.blankLine(state);
    while (!stream.eol()) {
      var style = mode.token(stream, state);
      callback(stream.current(), style, i, stream.start, state);
      stream.start = stream.pos;
    }
  }
};
})();
