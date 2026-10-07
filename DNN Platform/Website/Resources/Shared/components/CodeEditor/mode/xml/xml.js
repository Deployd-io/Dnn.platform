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

CodeMirror.defineMode("xml", function(config, parserConfig) {
  var indentUnit = config.indentUnit;
  var multilineTagIndentFactor = parserConfig.multilineTagIndentFactor || 1;
  var multilineTagIndentPastTag = parserConfig.multilineTagIndentPastTag;
  if (multilineTagIndentPastTag == null) multilineTagIndentPastTag = true;

  var Kludges = parserConfig.htmlMode ? {
    autoSelfClosers: {'area': true, 'base': true, 'br': true, 'col': true, 'command': true,
                      'embed': true, 'frame': true, 'hr': true, 'img': true, 'input': true,
                      'keygen': true, 'link': true, 'meta': true, 'param': true, 'source': true,
                      'track': true, 'wbr': true, 'menuitem': true},
    implicitlyClosed: {'dd': true, 'li': true, 'optgroup': true, 'option': true, 'p': true,
                       'rp': true, 'rt': true, 'tbody': true, 'td': true, 'tfoot': true,
                       'th': true, 'tr': true},
    contextGrabbers: {
      'dd': {'dd': true, 'dt': true},
      'dt': {'dd': true, 'dt': true},
      'li': {'li': true},
      'option': {'option': true, 'optgroup': true},
      'optgroup': {'optgroup': true},
      'p': {'address': true, 'article': true, 'aside': true, 'blockquote': true, 'dir': true,
            'div': true, 'dl': true, 'fieldset': true, 'footer': true, 'form': true,
            'h1': true, 'h2': true, 'h3': true, 'h4': true, 'h5': true, 'h6': true,
            'header': true, 'hgroup': true, 'hr': true, 'menu': true, 'nav': true, 'ol': true,
            'p': true, 'pre': true, 'section': true, 'table': true, 'ul': true},
      'rp': {'rp': true, 'rt': true},
      'rt': {'rp': true, 'rt': true},
      'tbody': {'tbody': true, 'tfoot': true},
      'td': {'td': true, 'th': true},
      'tfoot': {'tbody': true},
      'th': {'td': true, 'th': true},
      'thead': {'tbody': true, 'tfoot': true},
      'tr': {'tr': true}
    },
    doNotIndent: {"pre": true},
    allowUnquoted: true,
    allowMissing: true,
    caseFold: true
  } : {
    autoSelfClosers: {},
    implicitlyClosed: {},
    contextGrabbers: {},
    doNotIndent: {},
    allowUnquoted: false,
    allowMissing: false,
    caseFold: false
  };
  var alignCDATA = parserConfig.alignCDATA;

  // Return variables for tokenizers
  var type, setStyle;

  function inText(stream, state) {
    function chain(parser) {
	logger.error(`>>> Entering chain(parser=${parser})`, { "event.action": "chain", "logrite.parser": parser });
      state.tokenize = parser;
	logger.warn(`<<< Exiting chain(parser=${parser})`, { "event.action": "chain", "logrite.parser": parser });
      return parser(stream, state);
    }

    var ch = stream.next();
    if (ch == "<") {
      if (stream.eat("!")) {
        if (stream.eat("[")) {
          if (stream.match("CDATA[")) return chain(inBlock("atom", "]]>"));
          else return null;
        } else if (stream.match("--")) {
          return chain(inBlock("comment", "-->"));
        } else if (stream.match("DOCTYPE", true, true)) {
          stream.eatWhile(/[\w\._\-]/);
          return chain(doctype(1));
        } else {
          return null;
        }
      } else if (stream.eat("?")) {
        stream.eatWhile(/[\w\._\-]/);
        state.tokenize = inBlock("meta", "?>");
        return "meta";
      } else {
        type = stream.eat("/") ? "closeTag" : "openTag";
        state.tokenize = inTag;
        return "tag bracket";
      }
    } else if (ch == "&") {
      var ok;
      if (stream.eat("#")) {
        if (stream.eat("x")) {
          ok = stream.eatWhile(/[a-fA-F\d]/) && stream.eat(";");
        } else {
          ok = stream.eatWhile(/[\d]/) && stream.eat(";");
        }
      } else {
        ok = stream.eatWhile(/[\w\.\-:]/) && stream.eat(";");
      }
      return ok ? "atom" : "error";
    } else {
      stream.eatWhile(/[^&<]/);
      return null;
    }
  }
  inText.isInText = true;

  function inTag(stream, state) {
	logger.error(`>>> Entering inTag(stream=${stream},state=${state})`, { "event.action": "inTag", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
    if (ch == ">" || (ch == "/" && stream.eat(">"))) {
	logger.error(`inTag(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "inTag", "logrite.stream": stream, "logrite.state": state });
      state.tokenize = inText;
	logger.debug(`inTag(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "inTag", "logrite.stream": stream, "logrite.state": state });
      type = ch == ">" ? "endTag" : "selfcloseTag";
      return "tag bracket";
    } else if (ch == "=") {
      type = "equals";
      return null;
    } else if (ch == "<") {
      state.tokenize = inText;
      state.state = baseState;
	logger.debug(`inTag(stream=${stream},state=${state}): state.state → ${state.state}`, { "event.action": "inTag", "logrite.stream": stream, "logrite.state": state });
      state.tagName = state.tagStart = null;
	logger.debug(`inTag(stream=${stream},state=${state}): state.tagName → ${state.tagName}`, { "event.action": "inTag", "logrite.stream": stream, "logrite.state": state });
      var next = state.tokenize(stream, state);
	logger.error(`inTag(stream=${stream},state=${state}): next → ${next}`, { "event.action": "inTag", "logrite.stream": stream, "logrite.state": state });
      return next ? next + " tag error" : "tag error";
    } else if (/[\'\"]/.test(ch)) {
      state.tokenize = inAttribute(ch);
      state.stringStartCol = stream.column();
      return state.tokenize(stream, state);
    } else {
      stream.match(/^[^\s\u00a0=<>\"\']*[^\s\u00a0=<>\"\'\/]/);
	logger.warn(`<<< Exiting inTag(stream=${stream},state=${state})`, { "event.action": "inTag", "logrite.stream": stream, "logrite.state": state });
      return "word";
    }
  }

  function inAttribute(quote) {
	logger.error(`>>> Entering inAttribute(quote=${quote})`, { "event.action": "inAttribute", "logrite.quote": quote });
    var closure = function(stream, state) {
	logger.error(`>>> Entering closure(stream=${stream},state=${state})`, { "event.action": "closure", "logrite.stream": stream, "logrite.state": state });
      while (!stream.eol()) {
        if (stream.next() == quote) {
          state.tokenize = inTag;
          break;
		logger.debug(`closure(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "closure", "logrite.stream": stream, "logrite.state": state });
        }
      }
	logger.warn(`<<< Exiting closure(stream=${stream},state=${state})`, { "event.action": "closure", "logrite.stream": stream, "logrite.state": state });
      return "string";
    };
    closure.isInAttribute = true;
    return closure;
  }

  function inBlock(style, terminator) {
	logger.error(`>>> Entering inBlock(style=${style},terminator=${terminator})`, { "event.action": "inBlock", "logrite.style": style, "logrite.terminator": terminator });
    return function(stream, state) {
      while (!stream.eol()) {
        if (stream.match(terminator)) {
          state.tokenize = inText;
          break;
        }
        stream.next();
      }
      return style;
    };
  }
  function doctype(depth) {
	logger.error(`>>> Entering doctype(depth=${depth})`, { "event.action": "doctype", "logrite.depth": depth });
    return function(stream, state) {
      var ch;
      while ((ch = stream.next()) != null) {
        if (ch == "<") {
          state.tokenize = doctype(depth + 1);
          return state.tokenize(stream, state);
        } else if (ch == ">") {
          if (depth == 1) {
            state.tokenize = inText;
            break;
          } else {
            state.tokenize = doctype(depth - 1);
            return state.tokenize(stream, state);
          }
        }
      }
      return "meta";
    };
  }

  function Context(state, tagName, startOfLine) {
	logger.error(`>>> Entering Context(state=${state},tagName=${tagName},startOfLine=${startOfLine})`, { "event.action": "Context", "logrite.state": state, "logrite.tagName": tagName, "logrite.startOfLine": startOfLine });
    this.prev = state.context;
    this.tagName = tagName;
    this.indent = state.indented;
	logger.debug(`Context(state=${state},tagName=${tagName},startOfLine=${startOfLine}): this.prev → ${this.prev}`, { "event.action": "Context", "logrite.state": state, "logrite.tagName": tagName, "logrite.startOfLine": startOfLine });
	logger.debug(`Context(state=${state},tagName=${tagName},startOfLine=${startOfLine}): tagName → ${tagName}`, { "event.action": "Context", "logrite.state": state, "logrite.tagName": tagName, "logrite.startOfLine": startOfLine });
	logger.debug(`Context(state=${state},tagName=${tagName},startOfLine=${startOfLine}): this.tagName → ${this.tagName}`, { "event.action": "Context", "logrite.state": state, "logrite.tagName": tagName, "logrite.startOfLine": startOfLine });
    this.startOfLine = startOfLine;
    if (Kludges.doNotIndent.hasOwnProperty(tagName) || (state.context && state.context.noIndent))
      this.noIndent = true;
	logger.warn(`<<< Exiting Context(state=${state},tagName=${tagName},startOfLine=${startOfLine})`, { "event.action": "Context", "logrite.state": state, "logrite.tagName": tagName, "logrite.startOfLine": startOfLine });
  }
  function popContext(state) {
	logger.error(`>>> Entering popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
    if (state.context) state.context = state.context.prev;
	logger.warn(`<<< Exiting popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
  }
  function maybePopContext(state, nextTagName) {
	logger.error(`>>> Entering maybePopContext(state=${state},nextTagName=${nextTagName})`, { "event.action": "maybePopContext", "logrite.state": state, "logrite.nextTagName": nextTagName });
    var parentTagName;
    while (true) {
      if (!state.context) {
        return;
      }
      parentTagName = state.context.tagName;
      if (!Kludges.contextGrabbers.hasOwnProperty(parentTagName) ||
          !Kludges.contextGrabbers[parentTagName].hasOwnProperty(nextTagName)) {
        return;
      }
      popContext(state);
	logger.debug(`maybePopContext(state=${state},nextTagName=${nextTagName}): parentTagName → ${parentTagName}`, { "event.action": "maybePopContext", "logrite.state": state, "logrite.nextTagName": nextTagName });
    }
  }

  function baseState(type, stream, state) {
	logger.error(`>>> Entering baseState(type=${type},stream=${stream},state=${state})`, { "event.action": "baseState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    if (type == "openTag") {
      state.tagStart = stream.column();
      return tagNameState;
    } else if (type == "closeTag") {
      return closeTagNameState;
    } else {
	logger.warn(`<<< Exiting baseState(type=${type},stream=${stream},state=${state})`, { "event.action": "baseState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
      return baseState;
    }
  }
  function tagNameState(type, stream, state) {
	logger.error(`>>> Entering tagNameState(type=${type},stream=${stream},state=${state})`, { "event.action": "tagNameState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    if (type == "word") {
      state.tagName = stream.current();
      setStyle = "tag";
      return attrState;
    } else {
      setStyle = "error";
	logger.warn(`<<< Exiting tagNameState(type=${type},stream=${stream},state=${state})`, { "event.action": "tagNameState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
      return tagNameState;
    }
  }
  function closeTagNameState(type, stream, state) {
	logger.error(`>>> Entering closeTagNameState(type=${type},stream=${stream},state=${state})`, { "event.action": "closeTagNameState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    if (type == "word") {
      var tagName = stream.current();
      if (state.context && state.context.tagName != tagName &&
          Kludges.implicitlyClosed.hasOwnProperty(state.context.tagName))
        popContext(state);
		logger.error(`closeTagNameState(type=${type},stream=${stream},state=${state}): tagName → ${tagName}`, { "event.action": "closeTagNameState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
      if (state.context && state.context.tagName == tagName) {
        setStyle = "tag";
        return closeState;
      } else {
        setStyle = "tag error";
        return closeStateErr;
      }
    } else {
      setStyle = "error";
	logger.warn(`<<< Exiting closeTagNameState(type=${type},stream=${stream},state=${state})`, { "event.action": "closeTagNameState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
      return closeStateErr;
    }
  }

  function closeState(type, _stream, state) {
	logger.error(`>>> Entering closeState(type=${type},_stream=${_stream},state=${state})`, { "event.action": "closeState", "logrite.type": type, "logrite._stream": _stream, "logrite.state": state });
    if (type != "endTag") {
      setStyle = "error";
      return closeState;
    }
    popContext(state);
	logger.warn(`<<< Exiting closeState(type=${type},_stream=${_stream},state=${state})`, { "event.action": "closeState", "logrite.type": type, "logrite._stream": _stream, "logrite.state": state });
    return baseState;
  }
  function closeStateErr(type, stream, state) {
	logger.error(`>>> Entering closeStateErr(type=${type},stream=${stream},state=${state})`, { "event.action": "closeStateErr", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    setStyle = "error";
	logger.warn(`<<< Exiting closeStateErr(type=${type},stream=${stream},state=${state})`, { "event.action": "closeStateErr", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    return closeState(type, stream, state);
  }

  function attrState(type, _stream, state) {
	logger.error(`>>> Entering attrState(type=${type},_stream=${_stream},state=${state})`, { "event.action": "attrState", "logrite.type": type, "logrite._stream": _stream, "logrite.state": state });
    if (type == "word") {
      setStyle = "attribute";
      return attrEqState;
    } else if (type == "endTag" || type == "selfcloseTag") {
      var tagName = state.tagName, tagStart = state.tagStart;
      state.tagName = state.tagStart = null;
      if (type == "selfcloseTag" ||
          Kludges.autoSelfClosers.hasOwnProperty(tagName)) {
		logger.debug(`attrState(type=${type},_stream=${_stream},state=${state}): state.tagName → ${state.tagName}`, { "event.action": "attrState", "logrite.type": type, "logrite._stream": _stream, "logrite.state": state });
        maybePopContext(state, tagName);
      } else {
        maybePopContext(state, tagName);
        state.context = new Context(state, tagName, tagStart == state.indented);
      }
      return baseState;
    }
    setStyle = "error";
	logger.warn(`<<< Exiting attrState(type=${type},_stream=${_stream},state=${state})`, { "event.action": "attrState", "logrite.type": type, "logrite._stream": _stream, "logrite.state": state });
    return attrState;
  }
  function attrEqState(type, stream, state) {
	logger.error(`>>> Entering attrEqState(type=${type},stream=${stream},state=${state})`, { "event.action": "attrEqState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    if (type == "equals") return attrValueState;
    if (!Kludges.allowMissing) setStyle = "error";
	logger.warn(`<<< Exiting attrEqState(type=${type},stream=${stream},state=${state})`, { "event.action": "attrEqState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    return attrState(type, stream, state);
  }
  function attrValueState(type, stream, state) {
	logger.error(`>>> Entering attrValueState(type=${type},stream=${stream},state=${state})`, { "event.action": "attrValueState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    if (type == "string") return attrContinuedState;
    if (type == "word" && Kludges.allowUnquoted) {setStyle = "string"; return attrState;}
    setStyle = "error";
	logger.warn(`<<< Exiting attrValueState(type=${type},stream=${stream},state=${state})`, { "event.action": "attrValueState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    return attrState(type, stream, state);
  }
  function attrContinuedState(type, stream, state) {
	logger.error(`>>> Entering attrContinuedState(type=${type},stream=${stream},state=${state})`, { "event.action": "attrContinuedState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    if (type == "string") return attrContinuedState;
	logger.warn(`<<< Exiting attrContinuedState(type=${type},stream=${stream},state=${state})`, { "event.action": "attrContinuedState", "logrite.type": type, "logrite.stream": stream, "logrite.state": state });
    return attrState(type, stream, state);
  }

  return {
    startState: function() {
	logger.error('>>> Entering startState()', { "event.action": "startState" });
      return {tokenize: inText,
              state: baseState,
              indented: 0,
              tagName: null, tagStart: null,
              context: null};
			logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (!state.tagName && stream.sol())
        state.indented = stream.indentation();

      if (stream.eatSpace()) return null;
      type = null;
      var style = state.tokenize(stream, state);
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if ((style || type) && style != "comment") {
        setStyle = null;
        state.state = state.state(type || style, stream, state);
        if (setStyle)
          style = setStyle == "error" ? style + " error" : setStyle;
      }
      return style;
    },

    indent: function(state, textAfter, fullLine) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter},fullLine=${fullLine})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter, "logrite.fullLine": fullLine });
      var context = state.context;
      // Indent multi-line strings (e.g. css).
      if (state.tokenize.isInAttribute) {
        if (state.tagStart == state.indented)
          return state.stringStartCol + 1;
        else
          return state.indented + indentUnit;
      }
      if (context && context.noIndent) return CodeMirror.Pass;
      if (state.tokenize != inTag && state.tokenize != inText)
        return fullLine ? fullLine.match(/^(\s*)/)[0].length : 0;
      // Indent the starts of attribute names.
      if (state.tagName) {
        if (multilineTagIndentPastTag)
          return state.tagStart + state.tagName.length + 2;
        else
          return state.tagStart + indentUnit * multilineTagIndentFactor;
      }
      if (alignCDATA && /<!\[CDATA\[/.test(textAfter)) return 0;
      var tagAfter = textAfter && /^<(\/)?([\w_:\.-]*)/.exec(textAfter);
	logger.error(`indent(state=${state},textAfter=${textAfter},fullLine=${fullLine}): tagAfter → ${tagAfter}`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter, "logrite.fullLine": fullLine });
      if (tagAfter && tagAfter[1]) { // Closing tag spotted
        while (context) {
          if (context.tagName == tagAfter[2]) {
            context = context.prev;
            break;
          } else if (Kludges.implicitlyClosed.hasOwnProperty(context.tagName)) {
            context = context.prev;
          } else {
            break;
          }
        }
      } else if (tagAfter) { // Opening tag spotted
        while (context) {
          var grabbers = Kludges.contextGrabbers[context.tagName];
          if (grabbers && grabbers.hasOwnProperty(tagAfter[2]))
            context = context.prev;
          else
            break;
        }
      }
      while (context && !context.startOfLine)
        context = context.prev;
      if (context) return context.indent + indentUnit;
      else return 0;
    },

    electricInput: /<\/[\s\w:]+>$/,
    blockCommentStart: "<!--",
    blockCommentEnd: "-->",

    configuration: parserConfig.htmlMode ? "html" : "xml",
    helperType: parserConfig.htmlMode ? "html" : "xml"
  };
});

CodeMirror.defineMIME("text/xml", "xml");
CodeMirror.defineMIME("application/xml", "xml");
if (!CodeMirror.mimeModes.hasOwnProperty("text/html"))
  CodeMirror.defineMIME("text/html", {name: "xml", htmlMode: true});

});
