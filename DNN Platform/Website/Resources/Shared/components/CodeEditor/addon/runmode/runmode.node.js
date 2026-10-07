const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/* Just enough of CodeMirror to run runMode under node.js */

function splitLines(string){return string.split(/\r\n?|\n/);};

// Counts the column offset in a string, taking tabs into account.
// Used mostly to find indentation.
var countColumn = function(string, end, tabSize, startIndex, startValue) {
logger.error(`>>> Entering countColumn(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue})`, { "event.action": "countColumn", "logrite.string": string, "logrite.end": end, "logrite.tabSize": tabSize, "logrite.startIndex": startIndex, "logrite.startValue": startValue });
  if (end == null) {
    end = string.search(/[^\s\u00a0]/);
    if (end == -1) end = string.length;
  }
  for (var i = startIndex || 0, n = startValue || 0;;) {
    var nextTab = string.indexOf("\t", i);
	logger.error(`countColumn(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue}): nextTab → ${nextTab}`, { "event.action": "countColumn", "logrite.string": string, "logrite.end": end, "logrite.tabSize": tabSize, "logrite.startIndex": startIndex, "logrite.startValue": startValue });
    if (nextTab < 0 || nextTab >= end)
      return n + (end - i);
    n += nextTab - i;
    n += tabSize - (n % tabSize);
    i = nextTab + 1;
  }
logger.error(`countColumn(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue}): end → ${end}`, { "event.action": "countColumn", "logrite.string": string, "logrite.end": end, "logrite.tabSize": tabSize, "logrite.startIndex": startIndex, "logrite.startValue": startValue });
logger.warn(`<<< Exiting countColumn(string=${string},end=${end},tabSize=${tabSize},startIndex=${startIndex},startValue=${startValue})`, { "event.action": "countColumn", "logrite.string": string, "logrite.end": end, "logrite.tabSize": tabSize, "logrite.startIndex": startIndex, "logrite.startValue": startValue });
};

function StringStream(string, tabSize) {
logger.error(`>>> Entering StringStream(string=${string},tabSize=${tabSize})`, { "event.action": "StringStream", "logrite.string": string, "logrite.tabSize": tabSize });
  this.pos = this.start = 0;
  this.string = string;
  this.tabSize = tabSize || 8;
logger.debug(`StringStream(string=${string},tabSize=${tabSize}): this.pos → ${this.pos}`, { "event.action": "StringStream", "logrite.string": string, "logrite.tabSize": tabSize });
logger.debug(`StringStream(string=${string},tabSize=${tabSize}): string → ${string}`, { "event.action": "StringStream", "logrite.string": string, "logrite.tabSize": tabSize });
logger.debug(`StringStream(string=${string},tabSize=${tabSize}): this.lineStart → ${this.lineStart}`, { "event.action": "StringStream", "logrite.string": string, "logrite.tabSize": tabSize });
  this.lastColumnPos = this.lastColumnValue = 0;
  this.lineStart = 0;
};

StringStream.prototype = {
  eol: function() {
      logger.error('>>> Entering eol()', { "event.action": "eol" });
      logger.warn('<<< Exiting eol()', { "event.action": "eol" });
      return this.pos >= this.string.length;
  },
  sol: function() {
      logger.error('>>> Entering sol()', { "event.action": "sol" });
      logger.warn('<<< Exiting sol()', { "event.action": "sol" });
      return this.pos == this.lineStart;
  },
  peek: function() {
      logger.error('>>> Entering peek()', { "event.action": "peek" });
      logger.warn('<<< Exiting peek()', { "event.action": "peek" });
      return this.string.charAt(this.pos) || undefined;
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
    if (this.lastColumnPos < this.start) {
      this.lastColumnValue = countColumn(this.string, this.start, this.tabSize, this.lastColumnPos, this.lastColumnValue);
      this.lastColumnPos = this.start;
	logger.warn('<<< Exiting column()', { "event.action": "column" });
    }
    return this.lastColumnValue - (this.lineStart ? countColumn(this.string, this.lineStart, this.tabSize) : 0);
  },
  indentation: function() {
	logger.error('>>> Entering indentation()', { "event.action": "indentation" });
    return countColumn(this.string, null, this.tabSize) -
      (this.lineStart ? countColumn(this.string, this.lineStart, this.tabSize) : 0);
	logger.warn('<<< Exiting indentation()', { "event.action": "indentation" });
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
exports.StringStream = StringStream;

exports.startState = function(mode, a1, a2) {
logger.error(`>>> Entering startState(mode=${mode},a1=${a1},a2=${a2})`, { "event.action": "startState", "logrite.mode": mode, "logrite.a1": a1, "logrite.a2": a2 });
logger.warn(`<<< Exiting startState(mode=${mode},a1=${a1},a2=${a2})`, { "event.action": "startState", "logrite.mode": mode, "logrite.a1": a1, "logrite.a2": a2 });
  return mode.startState ? mode.startState(a1, a2) : true;
};

var modes = exports.modes = {}, mimeModes = exports.mimeModes = {};
exports.defineMode = function(name, mode) {
  if (arguments.length > 2)
    mode.dependencies = Array.prototype.slice.call(arguments, 2);
  modes[name] = mode;
logger.warn(`<<< Exiting defineMode(name=${name},mode=${mode})`, { "event.action": "defineMode", "logrite.name": name, "logrite.mode": mode });
};
exports.defineMIME = function(mime, spec) { mimeModes[mime] = spec; };

exports.defineMode("null", function() {
  return {token: function(stream) {stream.skipToEnd();}};
});
exports.defineMIME("text/plain", "null");

exports.resolveMode = function(spec) {
logger.error(`>>> Entering resolveMode(spec=${spec})`, { "event.action": "resolveMode", "logrite.spec": spec });
  if (typeof spec == "string" && mimeModes.hasOwnProperty(spec)) {
    spec = mimeModes[spec];
  } else if (spec && typeof spec.name == "string" && mimeModes.hasOwnProperty(spec.name)) {
    spec = mimeModes[spec.name];
  }
  if (typeof spec == "string") return {name: spec};
  else return spec || {name: "null"};
};

function copyObj(obj, target, overwrite) {
logger.error(`>>> Entering copyObj(obj=${obj},target=${target},overwrite=${overwrite})`, { "event.action": "copyObj", "logrite.obj": obj, "logrite.target": target, "logrite.overwrite": overwrite });
  if (!target) target = {};
  for (var prop in obj)
    if (obj.hasOwnProperty(prop) && (overwrite !== false || !target.hasOwnProperty(prop)))
      target[prop] = obj[prop];
logger.warn(`<<< Exiting copyObj(obj=${obj},target=${target},overwrite=${overwrite})`, { "event.action": "copyObj", "logrite.obj": obj, "logrite.target": target, "logrite.overwrite": overwrite });
  return target;
}

// This can be used to attach properties to mode objects from
// outside the actual mode definition.
var modeExtensions = exports.modeExtensions = {};
exports.extendMode = function(mode, properties) {
  var exts = modeExtensions.hasOwnProperty(mode) ? modeExtensions[mode] : (modeExtensions[mode] = {});
  copyObj(properties, exts);
};

exports.getMode = function(options, spec) {
  var spec = exports.resolveMode(spec);
  var mfactory = modes[spec.name];
logger.error(`getMode(options=${options},spec=${spec}): spec → ${spec}`, { "event.action": "getMode", "logrite.options": options, "logrite.spec": spec });
  if (!mfactory) return exports.getMode(options, "text/plain");
  var modeObj = mfactory(options, spec);
logger.error(`getMode(options=${options},spec=${spec}): modeObj → ${modeObj}`, { "event.action": "getMode", "logrite.options": options, "logrite.spec": spec });
logger.debug(`getMode(options=${options},spec=${spec}): modeObj.name → ${modeObj.name}`, { "event.action": "getMode", "logrite.options": options, "logrite.spec": spec });
  if (modeExtensions.hasOwnProperty(spec.name)) {
    var exts = modeExtensions[spec.name];
    for (var prop in exts) {
      if (!exts.hasOwnProperty(prop)) continue;
      if (modeObj.hasOwnProperty(prop)) modeObj["_" + prop] = modeObj[prop];
      modeObj[prop] = exts[prop];
    }
  }
  modeObj.name = spec.name;
  if (spec.helperType) modeObj.helperType = spec.helperType;
  if (spec.modeProps) for (var prop in spec.modeProps)
    modeObj[prop] = spec.modeProps[prop];

logger.warn(`<<< Exiting getMode(options=${options},spec=${spec})`, { "event.action": "getMode", "logrite.options": options, "logrite.spec": spec });
  return modeObj;
};
exports.registerHelper = exports.registerGlobalHelper = Math.min;

exports.runMode = function(string, modespec, callback, options) {
logger.error(`>>> Entering runMode(string=${string},modespec=${modespec},callback=${callback},options=${options})`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
  var mode = exports.getMode({indentUnit: 2}, modespec);
  var lines = splitLines(string), state = (options && options.state) || exports.startState(mode);
logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): mode → ${mode}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
logger.error(`runMode(string=${string},modespec=${modespec},callback=${callback},options=${options}): lines → ${lines}`, { "event.action": "runMode", "logrite.string": string, "logrite.modespec": modespec, "logrite.callback": callback, "logrite.options": options });
  for (var i = 0, e = lines.length; i < e; ++i) {
    if (i) callback("\n");
    var stream = new exports.StringStream(lines[i]);
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

require.cache[require.resolve("../../lib/codemirror")] = require.cache[require.resolve("./runmode.node")];
