const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/* Just enough of CodeMirror to run runMode under node.js */

function splitLines(string){return string.split(/\r\n?|\n/);};

// Counts the column offset in a string, taking tabs into account.
// Used mostly to find indentation.
var countColumn = function(string, end, tabSize, startIndex, startValue) {
logger.debug(`>>> Entering countColumn(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue})`);
  if (end == null) {
    end = string.search(/[^\s\u00a0]/);
    if (end == -1) end = string.length;
  }
  for (var i = startIndex || 0, n = startValue || 0;;) {
    var nextTab = string.indexOf("\t", i);
	logger.debug(`countColumn(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue}): nextTab → ${nextTab}`);
    if (nextTab < 0 || nextTab >= end)
      return n + (end - i);
    n += nextTab - i;
    n += tabSize - (n % tabSize);
    i = nextTab + 1;
  }
logger.debug(`countColumn(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue}): end → ${end}`);
logger.debug(`<<< Exiting countColumn(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue})`);
};

function StringStream(string, tabSize) {
logger.debug(`>>> Entering StringStream(string=${string},tabSize=${tabSize})`);
  this.pos = this.start = 0;
  this.string = string;
  this.tabSize = tabSize || 8;
logger.debug(`StringStream(string=${string},tabSize=${tabSize}): this.pos → ${this.pos}`);
logger.debug(`StringStream(string=${string},tabSize=${tabSize}): string → ${string}`);
logger.debug(`StringStream(string=${string},tabSize=${tabSize}): this.lineStart → ${this.lineStart}`);
logger.debug(`StringStream(string=${string},tabSize=${tabSize}): tabSize → ${tabSize}`);
  this.lastColumnPos = this.lastColumnValue = 0;
  this.lineStart = 0;
logger.debug(`<<< Exiting StringStream(string=${string},tabSize=${tabSize})`);
};

StringStream.prototype = {
  eol: function() {return this.pos >= this.string.length;},
  sol: function() {return this.pos == this.lineStart;},
  peek: function() {return this.string.charAt(this.pos) || undefined;},
  next: function() {
	logger.debug('>>> Entering next()');
    if (this.pos < this.string.length)
      return this.string.charAt(this.pos++);
  },
  eat: function(match) {
	logger.debug(`>>> Entering eat(match=${match})`);
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
  column: function() {
	logger.debug('>>> Entering column()');
    if (this.lastColumnPos < this.start) {
      this.lastColumnValue = countColumn(this.string, this.start, this.tabSize, this.lastColumnPos, this.lastColumnValue);
      this.lastColumnPos = this.start;
    }
	logger.debug('<<< Exiting column()');
    return this.lastColumnValue - (this.lineStart ? countColumn(this.string, this.lineStart, this.tabSize) : 0);
  },
  indentation: function() {
	logger.debug('>>> Entering indentation()');
    return countColumn(this.string, null, this.tabSize) -
      (this.lineStart ? countColumn(this.string, this.lineStart, this.tabSize) : 0);
	logger.debug('<<< Exiting indentation()');
  },
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
exports.StringStream = StringStream;

exports.startState = function(mode, a1, a2) {
logger.debug(`>>> Entering startState(mode=${mode},a1=${a1},a2=${a2})`);
logger.debug(`<<< Exiting startState(mode=${mode},a1=${a1},a2=${a2})`);
  return mode.startState ? mode.startState(a1, a2) : true;
};

var modes = exports.modes = {}, mimeModes = exports.mimeModes = {};
exports.defineMode = function(name, mode) {
  if (arguments.length > 2)
    mode.dependencies = Array.prototype.slice.call(arguments, 2);
	logger.debug('>>> Entering defineMode()');
  modes[name] = mode;
logger.debug('<<< Exiting defineMode()');
};
exports.defineMIME = function(mime, spec) { mimeModes[mime] = spec; };

exports.defineMode("null", function() {
  return {token: function(stream) {stream.skipToEnd();}};
});
exports.defineMIME("text/plain", "null");

exports.resolveMode = function(spec) {
logger.debug(`>>> Entering resolveMode(spec=${spec})`);
  if (typeof spec == "string" && mimeModes.hasOwnProperty(spec)) {
    spec = mimeModes[spec];
  } else if (spec && typeof spec.name == "string" && mimeModes.hasOwnProperty(spec.name)) {
    spec = mimeModes[spec.name];
  }
  if (typeof spec == "string") return {name: spec};
  else return spec || {name: "null"};
logger.debug(`<<< Exiting resolveMode(spec=${spec})`);
};

function copyObj(obj, target, overwrite) {
logger.debug(`>>> Entering copyObj(obj=${obj},target=${target},overwrite=${overwrite})`);
  if (!target) target = {};
  for (var prop in obj)
    if (obj.hasOwnProperty(prop) && (overwrite !== false || !target.hasOwnProperty(prop)))
      target[prop] = obj[prop];
logger.debug(`<<< Exiting copyObj(obj=${obj},target=${target},overwrite=${overwrite})`);
  return target;
}

// This can be used to attach properties to mode objects from
// outside the actual mode definition.
var modeExtensions = exports.modeExtensions = {};
exports.extendMode = function(mode, properties) {
  var exts = modeExtensions.hasOwnProperty(mode) ? modeExtensions[mode] : (modeExtensions[mode] = {});
  copyObj(properties, exts);
logger.debug(`<<< Exiting extendMode(mode=${mode},properties=${properties})`);
};

exports.getMode = function(options, spec) {
  var spec = exports.resolveMode(spec);
  var mfactory = modes[spec.name];
logger.debug(`getMode(options=${options},spec=${spec}): spec → ${spec}`);
  if (!mfactory) return exports.getMode(options, "text/plain");
  var modeObj = mfactory(options, spec);
logger.debug(`getMode(options=${options},spec=${spec}): modeObj → ${modeObj}`);
  if (modeExtensions.hasOwnProperty(spec.name)) {
    var exts = modeExtensions[spec.name];
    for (var prop in exts) {
      if (!exts.hasOwnProperty(prop)) continue;
      if (modeObj.hasOwnProperty(prop)) modeObj["_" + prop] = modeObj[prop];
      modeObj[prop] = exts[prop];
    }
  }
  modeObj.name = spec.name;
logger.debug(`getMode(options=${options},spec=${spec}): modeObj.name → ${modeObj.name}`);
  if (spec.helperType) modeObj.helperType = spec.helperType;
  if (spec.modeProps) for (var prop in spec.modeProps)
    modeObj[prop] = spec.modeProps[prop];

logger.debug(`<<< Exiting getMode(options=${options},spec=${spec})`);
  return modeObj;
};
exports.registerHelper = exports.registerGlobalHelper = Math.min;

exports.runMode = function(string, modespec, callback, options) {
logger.debug(`>>> Entering runMode(string=${string},modespec=${modespec},callback=${callback},options=${options})`);
  var mode = exports.getMode({indentUnit: 2}, modespec);
  var lines = splitLines(string), state = (options && options.state) || exports.startState(mode);
logger.debug(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): mode → ${mode}`);
logger.debug(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): lines → ${lines}`);
  for (var i = 0, e = lines.length; i < e; ++i) {
    if (i) callback("\n");
    var stream = new exports.StringStream(lines[i]);
	logger.debug(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): stream → ${stream}`);
    if (!stream.string && mode.blankLine) mode.blankLine(state);
    while (!stream.eol()) {
      var style = mode.token(stream, state);
	logger.debug(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): style → ${style}`);
      callback(stream.current(), style, i, stream.start, state);
      stream.start = stream.pos;
	logger.debug(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): stream.start → ${stream.start}`);
    }
	logger.debug(`<<< Exiting runMode(string=${string},modespec=${modespec},callback=${callback},options=${options})`);
  }
};

require.cache[require.resolve("../../lib/codemirror")] = require.cache[require.resolve("./runmode.node")];
