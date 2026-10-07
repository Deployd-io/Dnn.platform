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

  CodeMirror.defineMode("asn.1", function(config, parserConfig) {
    var indentUnit = config.indentUnit,
        keywords = parserConfig.keywords || {},
        cmipVerbs = parserConfig.cmipVerbs || {},
        compareTypes = parserConfig.compareTypes || {},
        status = parserConfig.status || {},
        tags = parserConfig.tags || {},
        storage = parserConfig.storage || {},
        modifier = parserConfig.modifier || {},
        accessTypes = parserConfig.accessTypes|| {},
        multiLineStrings = parserConfig.multiLineStrings,
        indentStatements = parserConfig.indentStatements !== false;
    var isOperatorChar = /[\|\^]/;
    var curPunc;

    function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      var ch = stream.next();
      if (ch == '"' || ch == "'") {
		logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
        state.tokenize = tokenString(ch);
        return state.tokenize(stream, state);
      }
      if (/[\[\]\(\){}:=,;]/.test(ch)) {
        curPunc = ch;
        return "punctuation";
      }
      if (ch == "-"){
        if (stream.eat("-")) {
          stream.skipToEnd();
          return "comment";
        }
      }
      if (/\d/.test(ch)) {
        stream.eatWhile(/[\w\.]/);
        return "number";
      }
      if (isOperatorChar.test(ch)) {
        stream.eatWhile(isOperatorChar);
        return "operator";
      }

      stream.eatWhile(/[\w\-]/);
      var cur = stream.current();
	logger.error(`tokenBase(stream=${stream},state=${state}): cur → ${cur}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      if (keywords.propertyIsEnumerable(cur)) return "keyword";
      if (cmipVerbs.propertyIsEnumerable(cur)) return "variable cmipVerbs";
      if (compareTypes.propertyIsEnumerable(cur)) return "atom compareTypes";
      if (status.propertyIsEnumerable(cur)) return "comment status";
      if (tags.propertyIsEnumerable(cur)) return "variable-3 tags";
      if (storage.propertyIsEnumerable(cur)) return "builtin storage";
      if (modifier.propertyIsEnumerable(cur)) return "string-2 modifier";
      if (accessTypes.propertyIsEnumerable(cur)) return "atom accessTypes";

	logger.warn(`<<< Exiting tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      return "variable";
    }

    function tokenString(quote) {
	logger.error(`>>> Entering tokenString(quote=${quote})`, { "event.action": "tokenString", "logrite.quote": quote });
      return function(stream, state) {
        var escaped = false, next, end = false;
        while ((next = stream.next()) != null) {
          if (next == quote && !escaped){
            var afterNext = stream.peek();
            //look if the character if the quote is like the B in '10100010'B
            if (afterNext){
              afterNext = afterNext.toLowerCase();
              if(afterNext == "b" || afterNext == "h" || afterNext == "o")
                stream.next();
            }
            end = true; break;
          }
          escaped = !escaped && next == "\\";
        }
        if (end || !(escaped || multiLineStrings))
          state.tokenize = null;
        return "string";
      };
    }

    function Context(indented, column, type, align, prev) {
	logger.error(`>>> Entering Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev})`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
      this.indented = indented;
      this.column = column;
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): indented → ${indented}`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): this.indented → ${this.indented}`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): column → ${column}`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): this.prev → ${this.prev}`, { "event.action": "Context", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev });
      this.type = type;
      this.align = align;
      this.prev = prev;
    }
    function pushContext(state, col, type) {
	logger.error(`>>> Entering pushContext(state=${state},col=${col},type=${type})`, { "event.action": "pushContext", "logrite.state": state, "logrite.col": col, "logrite.type": type });
      var indent = state.indented;
      if (state.context && state.context.type == "statement")
        indent = state.context.indented;
	logger.warn(`<<< Exiting pushContext(state=${state},col=${col},type=${type})`, { "event.action": "pushContext", "logrite.state": state, "logrite.col": col, "logrite.type": type });
      return state.context = new Context(indent, col, type, null, state.context);
    }
    function popContext(state) {
	logger.error(`>>> Entering popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
      var t = state.context.type;
      if (t == ")" || t == "]" || t == "}")
        state.indented = state.context.indented;
	logger.warn(`<<< Exiting popContext(state=${state})`, { "event.action": "popContext", "logrite.state": state });
      return state.context = state.context.prev;
    }

    //Interface
    return {
      startState: function(basecolumn) {
		logger.error(`>>> Entering startState(basecolumn=${basecolumn})`, { "event.action": "startState", "logrite.basecolumn": basecolumn });
        return {
          tokenize: null,
          context: new Context((basecolumn || 0) - indentUnit, 0, "top", false),
          indented: 0,
          startOfLine: true
        };
		logger.warn(`<<< Exiting startState(basecolumn=${basecolumn})`, { "event.action": "startState", "logrite.basecolumn": basecolumn });
      },

      token: function(stream, state) {
		logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        var ctx = state.context;
        if (stream.sol()) {
          if (ctx.align == null) ctx.align = false;
          state.indented = stream.indentation();
          state.startOfLine = true;
		logger.debug(`token(stream=${stream},state=${state}): state.startOfLine → ${state.startOfLine}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        }
        if (stream.eatSpace()) return null;
        curPunc = null;
        var style = (state.tokenize || tokenBase)(stream, state);
		logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        if (style == "comment") return style;
        if (ctx.align == null) ctx.align = true;

        if ((curPunc == ";" || curPunc == ":" || curPunc == ",")
            && ctx.type == "statement"){
          popContext(state);
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        }
        else if (curPunc == "{") pushContext(state, stream.column(), "}");
        else if (curPunc == "[") pushContext(state, stream.column(), "]");
        else if (curPunc == "(") pushContext(state, stream.column(), ")");
        else if (curPunc == "}") {
          while (ctx.type == "statement") ctx = popContext(state);
          if (ctx.type == "}") ctx = popContext(state);
          while (ctx.type == "statement") ctx = popContext(state);
        }
        else if (curPunc == ctx.type) popContext(state);
        else if (indentStatements && (((ctx.type == "}" || ctx.type == "top")
            && curPunc != ';') || (ctx.type == "statement"
            && curPunc == "newstatement")))
          pushContext(state, stream.column(), "statement");

        state.startOfLine = false;
        return style;
      },

      electricChars: "{}",
      lineComment: "--",
      fold: "brace"
    };
  });

  function words(str) {
	logger.error(`>>> Entering words(str=${str})`, { "event.action": "words", "logrite.str": str });
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.error(`words(str=${str}): obj → ${obj}`, { "event.action": "words", "logrite.str": str });
	logger.warn(`<<< Exiting words(str=${str})`, { "event.action": "words", "logrite.str": str });
    return obj;
  }

  CodeMirror.defineMIME("text/x-ttcn-asn", {
    name: "asn.1",
    keywords: words("DEFINITIONS OBJECTS IF DERIVED INFORMATION ACTION" +
    " REPLY ANY NAMED CHARACTERIZED BEHAVIOUR REGISTERED" +
    " WITH AS IDENTIFIED CONSTRAINED BY PRESENT BEGIN" +
    " IMPORTS FROM UNITS SYNTAX MIN-ACCESS MAX-ACCESS" +
    " MINACCESS MAXACCESS REVISION STATUS DESCRIPTION" +
    " SEQUENCE SET COMPONENTS OF CHOICE DistinguishedName" +
    " ENUMERATED SIZE MODULE END INDEX AUGMENTS EXTENSIBILITY" +
    " IMPLIED EXPORTS"),
    cmipVerbs: words("ACTIONS ADD GET NOTIFICATIONS REPLACE REMOVE"),
    compareTypes: words("OPTIONAL DEFAULT MANAGED MODULE-TYPE MODULE_IDENTITY" +
    " MODULE-COMPLIANCE OBJECT-TYPE OBJECT-IDENTITY" +
    " OBJECT-COMPLIANCE MODE CONFIRMED CONDITIONAL" +
    " SUBORDINATE SUPERIOR CLASS TRUE FALSE NULL" +
    " TEXTUAL-CONVENTION"),
    status: words("current deprecated mandatory obsolete"),
    tags: words("APPLICATION AUTOMATIC EXPLICIT IMPLICIT PRIVATE TAGS" +
    " UNIVERSAL"),
    storage: words("BOOLEAN INTEGER OBJECT IDENTIFIER BIT OCTET STRING" +
    " UTCTime InterfaceIndex IANAifType CMIP-Attribute" +
    " REAL PACKAGE PACKAGES IpAddress PhysAddress" +
    " NetworkAddress BITS BMPString TimeStamp TimeTicks" +
    " TruthValue RowStatus DisplayString GeneralString" +
    " GraphicString IA5String NumericString" +
    " PrintableString SnmpAdminAtring TeletexString" +
    " UTF8String VideotexString VisibleString StringStore" +
    " ISO646String T61String UniversalString Unsigned32" +
    " Integer32 Gauge Gauge32 Counter Counter32 Counter64"),
    modifier: words("ATTRIBUTE ATTRIBUTES MANDATORY-GROUP MANDATORY-GROUPS" +
    " GROUP GROUPS ELEMENTS EQUALITY ORDERING SUBSTRINGS" +
    " DEFINED"),
    accessTypes: words("not-accessible accessible-for-notify read-only" +
    " read-create read-write"),
    multiLineStrings: true
  });
});
