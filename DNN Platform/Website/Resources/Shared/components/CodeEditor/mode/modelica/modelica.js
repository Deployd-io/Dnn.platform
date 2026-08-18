const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Modelica support for CodeMirror, copyright (c) by Lennart Ochel

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})

(function(CodeMirror) {
  "use strict";
logger.debug(`<<< Exiting function(exports=${exports})`);

  CodeMirror.defineMode("modelica", function(config, parserConfig) {
logger.debug('>>> Entering defineMode()');

    var indentUnit = config.indentUnit;
    var keywords = parserConfig.keywords || {};
    var builtin = parserConfig.builtin || {};
    var atoms = parserConfig.atoms || {};

    var isSingleOperatorChar = /[;=\(:\),{}.*<>+\-\/^\[\]]/;
	logger.debug(`defineMode(): isSingleOperatorChar → ${isSingleOperatorChar}`);
    var isDoubleOperatorChar = /(:=|<=|>=|==|<>|\.\+|\.\-|\.\*|\.\/|\.\^)/;
	logger.debug(`defineMode(): isDoubleOperatorChar → ${isDoubleOperatorChar}`);
    var isDigit = /[0-9]/;
    var isNonDigit = /[_a-zA-Z]/;
	logger.debug('<<< Exiting defineMode()');

    function tokenLineComment(stream, state) {
	logger.debug(`>>> Entering tokenLineComment(stream=${stream},state=${state})`);
      stream.skipToEnd();
      state.tokenize = null;
	logger.debug(`tokenLineComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting tokenLineComment(stream=${stream},state=${state})`);
      return "comment";
    }

    function tokenBlockComment(stream, state) {
	logger.debug(`>>> Entering tokenBlockComment(stream=${stream},state=${state})`);
      var maybeEnd = false, ch;
      while (ch = stream.next()) {
        if (maybeEnd && ch == "/") {
          state.tokenize = null;
		logger.debug(`tokenBlockComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
          break;
        }
        maybeEnd = (ch == "*");
      }
	logger.debug(`tokenBlockComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
	logger.debug(`<<< Exiting tokenBlockComment(stream=${stream},state=${state})`);
      return "comment";
    }

    function tokenString(stream, state) {
	logger.debug(`>>> Entering tokenString(stream=${stream},state=${state})`);
      var escaped = false, ch;
      while ((ch = stream.next()) != null) {
		logger.debug(`tokenString(stream=${stream},state=${state}): escaped → ${escaped}`);
        if (ch == '"' && !escaped) {
          state.tokenize = null;
		logger.debug(`tokenString(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
          state.sol = false;
		logger.debug(`tokenString(stream=${stream},state=${state}): state.sol → ${state.sol}`);
          break;
        }
        escaped = !escaped && ch == "\\";
      }

	logger.debug(`<<< Exiting tokenString(stream=${stream},state=${state})`);
      return "string";
    }

    function tokenIdent(stream, state) {
	logger.debug(`>>> Entering tokenIdent(stream=${stream},state=${state})`);
      stream.eatWhile(isDigit);
      while (stream.eat(isDigit) || stream.eat(isNonDigit)) { }


      var cur = stream.current();
	logger.debug(`tokenIdent(stream=${stream},state=${state}): cur → ${cur}`);

      if(state.sol && (cur == "package" || cur == "model" || cur == "when" || cur == "connector")) state.level++;
      else if(state.sol && cur == "end" && state.level > 0) state.level--;

      state.tokenize = null;
	logger.debug(`tokenIdent(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
      state.sol = false;
	logger.debug(`tokenIdent(stream=${stream},state=${state}): state.sol → ${state.sol}`);

      if (keywords.propertyIsEnumerable(cur)) return "keyword";
      else if (builtin.propertyIsEnumerable(cur)) return "builtin";
      else if (atoms.propertyIsEnumerable(cur)) return "atom";
      else return "variable";
	logger.debug(`<<< Exiting tokenIdent(stream=${stream},state=${state})`);
    }

    function tokenQIdent(stream, state) {
	logger.debug(`>>> Entering tokenQIdent(stream=${stream},state=${state})`);
      while (stream.eat(/[^']/)) { }

      state.tokenize = null;
      state.sol = false;

      if(stream.eat("'"))
        return "variable";
      else
        return "error";
    }

    function tokenUnsignedNuber(stream, state) {
	logger.debug(`>>> Entering tokenUnsignedNuber(stream=${stream},state=${state})`);
      stream.eatWhile(isDigit);
      if (stream.eat('.')) {
        stream.eatWhile(isDigit);
      }
      if (stream.eat('e') || stream.eat('E')) {
        if (!stream.eat('-'))
          stream.eat('+');
        stream.eatWhile(isDigit);
      }

      state.tokenize = null;
	logger.debug(`tokenUnsignedNuber(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
      state.sol = false;
	logger.debug(`tokenUnsignedNuber(stream=${stream},state=${state}): state.sol → ${state.sol}`);
	logger.debug(`<<< Exiting tokenUnsignedNuber(stream=${stream},state=${state})`);
      return "number";
    }

    // Interface
    return {
      startState: function() {
		logger.debug('>>> Entering startState()');
        return {
          tokenize: null,
          level: 0,
          sol: true
        };
		logger.debug('<<< Exiting startState()');
      },

      token: function(stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
        if(state.tokenize != null) {
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
          return state.tokenize(stream, state);
        }

        if(stream.sol()) {
          state.sol = true;
		logger.debug(`token(stream=${stream},state=${state}): state.sol → ${state.sol}`);
        }

        // WHITESPACE
        if(stream.eatSpace()) {
          state.tokenize = null;
		logger.debug(`token(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
          return null;
        }

        var ch = stream.next();
		logger.debug(`token(stream=${stream},state=${state}): ch → ${ch}`);

        // LINECOMMENT
        if(ch == '/' && stream.eat('/')) {
          state.tokenize = tokenLineComment;
        }
        // BLOCKCOMMENT
        else if(ch == '/' && stream.eat('*')) {
          state.tokenize = tokenBlockComment;
        }
        // TWO SYMBOL TOKENS
        else if(isDoubleOperatorChar.test(ch+stream.peek())) {
          stream.next();
          state.tokenize = null;
          return "operator";
        }
        // SINGLE SYMBOL TOKENS
        else if(isSingleOperatorChar.test(ch)) {
          state.tokenize = null;
          return "operator";
        }
        // IDENT
        else if(isNonDigit.test(ch)) {
          state.tokenize = tokenIdent;
        }
        // Q-IDENT
        else if(ch == "'" && stream.peek() && stream.peek() != "'") {
          state.tokenize = tokenQIdent;
        }
        // STRING
        else if(ch == '"') {
          state.tokenize = tokenString;
        }
        // UNSIGNED_NUBER
        else if(isDigit.test(ch)) {
          state.tokenize = tokenUnsignedNuber;
        }
        // ERROR
        else {
          state.tokenize = null;
          return "error";
        }

        return state.tokenize(stream, state);
      },

      indent: function(state, textAfter) {
		logger.debug(`>>> Entering indent(state=${state},textAfter=${textAfter})`);
        if (state.tokenize != null) return CodeMirror.Pass;

        var level = state.level;
        if(/(algorithm)/.test(textAfter)) level--;
        if(/(equation)/.test(textAfter)) level--;
        if(/(initial algorithm)/.test(textAfter)) level--;
        if(/(initial equation)/.test(textAfter)) level--;
        if(/(end)/.test(textAfter)) level--;

        if(level > 0)
          return indentUnit*level;
        else
          return 0;
      },

      blockCommentStart: "/*",
      blockCommentEnd: "*/",
      lineComment: "//"
    };
  });

  function words(str) {
	logger.debug(`>>> Entering words(str=${str})`);
    var obj = {}, words = str.split(" ");
    for (var i=0; i<words.length; ++i)
	logger.debug(`words(str=${str}): obj → ${obj}`);
      obj[words[i]] = true;
	logger.debug(`<<< Exiting words(str=${str})`);
    return obj;
  }

  var modelicaKeywords = "algorithm and annotation assert block break class connect connector constant constrainedby der discrete each else elseif elsewhen encapsulated end enumeration equation expandable extends external false final flow for function if import impure in initial inner input loop model not operator or outer output package parameter partial protected public pure record redeclare replaceable return stream then true type when while within";
  var modelicaBuiltin = "abs acos actualStream asin atan atan2 cardinality ceil cos cosh delay div edge exp floor getInstanceName homotopy inStream integer log log10 mod pre reinit rem semiLinear sign sin sinh spatialDistribution sqrt tan tanh";
  var modelicaAtoms = "Real Boolean Integer String";

  function def(mimes, mode) {
	logger.debug(`>>> Entering def(mimes=${mimes},mode=${mode})`);
    if (typeof mimes == "string")
      mimes = [mimes];

    var words = [];
	logger.debug(`<<< Exiting def(mimes=${mimes},mode=${mode})`);

    function add(obj) {
      if (obj)
        for (var prop in obj)
          if (obj.hasOwnProperty(prop))
            words.push(prop);
			logger.debug(`<<< Exiting add(obj=${obj})`);
    }

    add(mode.keywords);
    add(mode.builtin);
    add(mode.atoms);

    if (words.length) {
      mode.helperType = mimes[0];
      CodeMirror.registerHelper("hintWords", mimes[0], words);
    }

    for (var i=0; i<mimes.length; ++i)
      CodeMirror.defineMIME(mimes[i], mode);
  }

  def(["text/x-modelica"], {
    name: "modelica",
    keywords: words(modelicaKeywords),
    builtin: words(modelicaBuiltin),
    atoms: words(modelicaAtoms)
  });
});
