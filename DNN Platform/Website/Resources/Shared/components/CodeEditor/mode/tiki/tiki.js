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

CodeMirror.defineMode('tiki', function(config) {
  function inBlock(style, terminator, returnTokenizer) {
    return function(stream, state) {
      while (!stream.eol()) {
        if (stream.match(terminator)) {
          state.tokenize = inText;
          break;
        }
        stream.next();
      }

      if (returnTokenizer) state.tokenize = returnTokenizer;

      return style;
    };
  }

  function inLine(style) {
	logger.error(`>>> Entering inLine(style=${style})`, { "event.action": "inLine", "logrite.style": style });
    return function(stream, state) {
      while(!stream.eol()) {
        stream.next();
      }
      state.tokenize = inText;
      return style;
    };
  }

  function inText(stream, state) {
    function chain(parser) {
	logger.error(`>>> Entering chain(parser=${parser})`, { "event.action": "chain", "logrite.parser": parser });
      state.tokenize = parser;
	logger.warn(`<<< Exiting chain(parser=${parser})`, { "event.action": "chain", "logrite.parser": parser });
      return parser(stream, state);
    }

    var sol = stream.sol();
    var ch = stream.next();

    //non start of line
    switch (ch) { //switch is generally much faster than if, so it is used here
    case "{": //plugin
      stream.eat("/");
      stream.eatSpace();
      stream.eatWhile(/[^\s\u00a0=\"\'\/?(}]/);
      state.tokenize = inPlugin;
      return "tag";
    case "_": //bold
      if (stream.eat("_"))
        return chain(inBlock("strong", "__", inText));
      break;
    case "'": //italics
      if (stream.eat("'"))
        return chain(inBlock("em", "''", inText));
      break;
    case "(":// Wiki Link
      if (stream.eat("("))
        return chain(inBlock("variable-2", "))", inText));
      break;
    case "[":// Weblink
      return chain(inBlock("variable-3", "]", inText));
      break;
    case "|": //table
      if (stream.eat("|"))
        return chain(inBlock("comment", "||"));
      break;
    case "-":
      if (stream.eat("=")) {//titleBar
        return chain(inBlock("header string", "=-", inText));
      } else if (stream.eat("-")) {//deleted
        return chain(inBlock("error tw-deleted", "--", inText));
      }
      break;
    case "=": //underline
      if (stream.match("=="))
        return chain(inBlock("tw-underline", "===", inText));
      break;
    case ":":
      if (stream.eat(":"))
        return chain(inBlock("comment", "::"));
      break;
    case "^": //box
      return chain(inBlock("tw-box", "^"));
      break;
    case "~": //np
      if (stream.match("np~"))
        return chain(inBlock("meta", "~/np~"));
      break;
    }

    //start of line types
    if (sol) {
      switch (ch) {
      case "!": //header at start of line
        if (stream.match('!!!!!')) {
          return chain(inLine("header string"));
        } else if (stream.match('!!!!')) {
          return chain(inLine("header string"));
        } else if (stream.match('!!!')) {
          return chain(inLine("header string"));
        } else if (stream.match('!!')) {
          return chain(inLine("header string"));
        } else {
          return chain(inLine("header string"));
        }
        break;
      case "*": //unordered list line item, or <li /> at start of line
      case "#": //ordered list line item, or <li /> at start of line
      case "+": //ordered list line item, or <li /> at start of line
        return chain(inLine("tw-listitem bracket"));
        break;
      }
    }

    //stream.eatWhile(/[&{]/); was eating up plugins, turned off to act less like html and more like tiki
    return null;
  }

  var indentUnit = config.indentUnit;

  // Return variables for tokenizers
  var pluginName, type;
  function inPlugin(stream, state) {
	logger.error(`>>> Entering inPlugin(stream=${stream},state=${state})`, { "event.action": "inPlugin", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
    var peek = stream.peek();
	logger.error(`inPlugin(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "inPlugin", "logrite.stream": stream, "logrite.state": state });
	logger.error(`inPlugin(stream=${stream},state=${state}): peek → ${peek}`, { "event.action": "inPlugin", "logrite.stream": stream, "logrite.state": state });

    if (ch == "}") {
      state.tokenize = inText;
	logger.debug(`inPlugin(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "inPlugin", "logrite.stream": stream, "logrite.state": state });
      //type = ch == ")" ? "endPlugin" : "selfclosePlugin"; inPlugin
      return "tag";
    } else if (ch == "(" || ch == ")") {
      return "bracket";
    } else if (ch == "=") {
      type = "equals";

      if (peek == ">") {
        ch = stream.next();
        peek = stream.peek();
      }

      //here we detect values directly after equal character with no quotes
      if (!/[\'\"]/.test(peek)) {
        state.tokenize = inAttributeNoQuote();
      }
      //end detect values

      return "operator";
    } else if (/[\'\"]/.test(ch)) {
      state.tokenize = inAttribute(ch);
      return state.tokenize(stream, state);
    } else {
      stream.eatWhile(/[^\s\u00a0=\"\'\/?]/);
	logger.warn(`<<< Exiting inPlugin(stream=${stream},state=${state})`, { "event.action": "inPlugin", "logrite.stream": stream, "logrite.state": state });
      return "keyword";
    }
  }

  function inAttribute(quote) {
	logger.error(`>>> Entering inAttribute(quote=${quote})`, { "event.action": "inAttribute", "logrite.quote": quote });
    return function(stream, state) {
      while (!stream.eol()) {
        if (stream.next() == quote) {
          state.tokenize = inPlugin;
          break;
        }
      }
      return "string";
    };
  }

  function inAttributeNoQuote() {
	logger.error('>>> Entering inAttributeNoQuote()', { "event.action": "inAttributeNoQuote" });
    return function(stream, state) {
      while (!stream.eol()) {
        var ch = stream.next();
        var peek = stream.peek();
        if (ch == " " || ch == "," || /[ )}]/.test(peek)) {
      state.tokenize = inPlugin;
      break;
	logger.warn('<<< Exiting inAttributeNoQuote()', { "event.action": "inAttributeNoQuote" });
    }
  }
  return "string";
};
                     }

var curState, setStyle;
function pass() {
logger.error('>>> Entering pass()', { "event.action": "pass" });
  for (var i = arguments.length - 1; i >= 0; i--) curState.cc.push(arguments[i]);
logger.warn('<<< Exiting pass()', { "event.action": "pass" });
}

function cont() {
logger.error('>>> Entering cont()', { "event.action": "cont" });
  pass.apply(null, arguments);
logger.warn('<<< Exiting cont()', { "event.action": "cont" });
  return true;
}

function pushContext(pluginName, startOfLine) {
logger.error(`>>> Entering pushContext(pluginName=${pluginName},startOfLine=${startOfLine})`, { "event.action": "pushContext", "logrite.pluginName": pluginName, "logrite.startOfLine": startOfLine });
  var noIndent = curState.context && curState.context.noIndent;
  curState.context = {
    prev: curState.context,
    pluginName: pluginName,
    indent: curState.indented,
    startOfLine: startOfLine,
    noIndent: noIndent
  };
logger.warn(`<<< Exiting pushContext(pluginName=${pluginName},startOfLine=${startOfLine})`, { "event.action": "pushContext", "logrite.pluginName": pluginName, "logrite.startOfLine": startOfLine });
}

function popContext() {
logger.error('>>> Entering popContext()', { "event.action": "popContext" });
  if (curState.context) curState.context = curState.context.prev;
logger.warn('<<< Exiting popContext()', { "event.action": "popContext" });
}

function element(type) {
logger.error(`>>> Entering element(type=${type})`, { "event.action": "element", "logrite.type": type });
  if (type == "openPlugin") {curState.pluginName = pluginName; return cont(attributes, endplugin(curState.startOfLine));}
  else if (type == "closePlugin") {
    var err = false;
    if (curState.context) {
      err = curState.context.pluginName != pluginName;
      popContext();
    } else {
      err = true;
    }
    if (err) setStyle = "error";
    return cont(endcloseplugin(err));
  }
  else if (type == "string") {
    if (!curState.context || curState.context.name != "!cdata") pushContext("!cdata");
    if (curState.tokenize == inText) popContext();
    return cont();
  }
  else return cont();
}

function endplugin(startOfLine) {
logger.error(`>>> Entering endplugin(startOfLine=${startOfLine})`, { "event.action": "endplugin", "logrite.startOfLine": startOfLine });
  return function(type) {
    if (
      type == "selfclosePlugin" ||
        type == "endPlugin"
    )
      return cont();
    if (type == "endPlugin") {pushContext(curState.pluginName, startOfLine); return cont();}
    return cont();
  };
}

function endcloseplugin(err) {
logger.error(`>>> Entering endcloseplugin(err=${err})`, { "event.action": "endcloseplugin", "logrite.err": err });
  return function(type) {
    if (err) setStyle = "error";
    if (type == "endPlugin") return cont();
    return pass();
  };
}

function attributes(type) {
logger.error(`>>> Entering attributes(type=${type})`, { "event.action": "attributes", "logrite.type": type });
  if (type == "keyword") {setStyle = "attribute"; return cont(attributes);}
  if (type == "equals") return cont(attvalue, attributes);
logger.warn(`<<< Exiting attributes(type=${type})`, { "event.action": "attributes", "logrite.type": type });
  return pass();
}
function attvalue(type) {
logger.error(`>>> Entering attvalue(type=${type})`, { "event.action": "attvalue", "logrite.type": type });
  if (type == "keyword") {setStyle = "string"; return cont();}
  if (type == "string") return cont(attvaluemaybe);
logger.warn(`<<< Exiting attvalue(type=${type})`, { "event.action": "attvalue", "logrite.type": type });
  return pass();
}
function attvaluemaybe(type) {
logger.error(`>>> Entering attvaluemaybe(type=${type})`, { "event.action": "attvaluemaybe", "logrite.type": type });
  if (type == "string") return cont(attvaluemaybe);
  else return pass();
}
return {
  startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    return {tokenize: inText, cc: [], indented: 0, startOfLine: true, pluginName: null, context: null};
  },
  token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
    if (stream.sol()) {
      state.startOfLine = true;
      state.indented = stream.indentation();
    }
    if (stream.eatSpace()) return null;
	logger.debug(`token(stream=${stream},state=${state}): state.startOfLine → ${state.startOfLine}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

    setStyle = type = pluginName = null;
    var style = state.tokenize(stream, state);
    if ((style || type) && style != "comment") {
      curState = state;
      while (true) {
        var comb = state.cc.pop() || element;
		logger.error(`token(stream=${stream},state=${state}): comb → ${comb}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        if (comb(type || style)) break;
      }
    }
    state.startOfLine = false;
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
    return setStyle || style;
  },
  indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
    var context = state.context;
    if (context && context.noIndent) return 0;
    if (context && /^{\//.test(textAfter))
        context = context.prev;
        while (context && !context.startOfLine)
          context = context.prev;
		logger.warn(`<<< Exiting indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
        if (context) return context.indent + indentUnit;
        else return 0;
       },
    electricChars: "/"
  };
});

CodeMirror.defineMIME("text/tiki", "tiki");

});
