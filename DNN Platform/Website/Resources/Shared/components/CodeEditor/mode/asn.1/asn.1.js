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

  CodeMirror.defineMode("asn.1", function(config, parserConfig) {
	logger.debug('>>> Entering defineMode()');
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
	logger.debug('<<< Exiting defineMode()');

    function tokenBase(stream, state) {
	logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
      var ch = stream.next();
      if (ch == '"' || ch == "'") {
		logger.debug(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`);
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
	logger.debug(`tokenBase(stream=${stream},state=${state}): cur → ${cur}`);
      if (keywords.propertyIsEnumerable(cur)) return "keyword";
      if (cmipVerbs.propertyIsEnumerable(cur)) return "variable cmipVerbs";
      if (compareTypes.propertyIsEnumerable(cur)) return "atom compareTypes";
      if (status.propertyIsEnumerable(cur)) return "comment status";
      if (tags.propertyIsEnumerable(cur)) return "variable-3 tags";
      if (storage.propertyIsEnumerable(cur)) return "builtin storage";
      if (modifier.propertyIsEnumerable(cur)) return "string-2 modifier";
      if (accessTypes.propertyIsEnumerable(cur)) return "atom accessTypes";

	logger.debug(`<<< Exiting tokenBase(stream=${stream},state=${state})`);
      return "variable";
    }

    function tokenString(quote) {
	logger.debug(`>>> Entering tokenString(quote=${quote})`);
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
		logger.debug(`tokenString(quote=${quote}): state.tokenize → ${state.tokenize}`);
		logger.debug(`<<< Exiting tokenString(quote=${quote})`);
        return "string";
      };
    }

    function Context(indented, column, type, align, prev) {
	logger.debug(`>>> Entering Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev})`);
      this.indented = indented;
      this.column = column;
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): indented → ${indented}`);
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): this.indented → ${this.indented}`);
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): column → ${column}`);
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): align → ${align}`);
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): this.prev → ${this.prev}`);
      this.type = type;
      this.align = align;
      this.prev = prev;
	logger.debug(`Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev}): type → ${type}`);
	logger.debug(`<<< Exiting Context(indented=${indented},column=${column},type=${type},align=${align},prev=${prev})`);
    }
    function pushContext(state, col, type) {
      var indent = state.indented;
      if (state.context && state.context.type == "statement")
		logger.debug(`>>> Entering pushContext(state=${state},col=${col},type=${type})`);
		logger.debug(`<<< Exiting pushContext(state=${state},col=${col},type=${type})`);
        indent = state.context.indented;
      return state.context = new Context(indent, col, type, null, state.context);
    }
    function popContext(state) {
	logger.debug(`>>> Entering popContext(state=${state})`);
      var t = state.context.type;
      if (t == ")" || t == "]" || t == "}")
        state.indented = state.context.indented;
	logger.debug(`<<< Exiting popContext(state=${state})`);
      return state.context = state.context.prev;
    }

    //Interface
    return {
      startState: function(basecolumn) {
		logger.debug(`>>> Entering startState(basecolumn=${basecolumn})`);
        return {
          tokenize: null,
          context: new Context((basecolumn || 0) - indentUnit, 0, "top", false),
          indented: 0,
          startOfLine: true
        };
		logger.debug(`<<< Exiting startState(basecolumn=${basecolumn})`);
      },

      token: function(stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
        var ctx = state.context;
        if (stream.sol()) {
          if (ctx.align == null) ctx.align = false;
          state.indented = stream.indentation();
          state.startOfLine = true;
		logger.debug(`token(stream=${stream},state=${state}): state.startOfLine → ${state.startOfLine}`);
        }
        if (stream.eatSpace()) return null;
        curPunc = null;
        var style = (state.tokenize || tokenBase)(stream, state);
		logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
        if (style == "comment") return style;
        if (ctx.align == null) ctx.align = true;

        if ((curPunc == ";" || curPunc == ":" || curPunc == ",")
            && ctx.type == "statement"){
          popContext(state);
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
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
		logger.debug(`<<< Exiting function(exports=${exports})`);
        return style;
      },

      electricChars: "{}",
      lineComment: "--",
      fold: "brace"
    };
  });

  function words(str) {
	logger.debug(`>>> Entering words(str=${str})`);
    var obj = {}, words = str.split(" ");
    for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
	logger.debug(`words(str=${str}): obj → ${obj}`);
	logger.debug(`<<< Exiting words(str=${str})`);
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
