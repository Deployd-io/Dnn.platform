const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Modelica support for CodeMirror, copyright (c) by Lennart Ochel

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})

(function(CodeMirror) {
  "use strict";

  CodeMirror.defineMode("modelica", function(config, parserConfig) {

    var indentUnit = config.indentUnit;
    var keywords = parserConfig.keywords || {};
    var builtin = parserConfig.builtin || {};
    var atoms = parserConfig.atoms || {};

    var isSingleOperatorChar = /[;=\(:\),{}.*<>+\-\/^\[\]]/;
    var isDoubleOperatorChar = /(:=|<=|>=|==|<>|\.\+|\.\-|\.\*|\.\/|\.\^)/;
    var isDigit = /[0-9]/;
    var isNonDigit = /[_a-zA-Z]/;

    function tokenLineComment(stream, state) {
	logger.error(`>>> Entering tokenLineComment(stream=${stream},state=${state})`, { "event.action": "tokenLineComment", "logrite.stream": stream, "logrite.state": state });
      stream.skipToEnd();
      state.tokenize = null;
	logger.debug(`tokenLineComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenLineComment", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting tokenLineComment(stream=${stream},state=${state})`, { "event.action": "tokenLineComment", "logrite.stream": stream, "logrite.state": state });
      return "comment";
    }

    function tokenBlockComment(stream, state) {
	logger.error(`>>> Entering tokenBlockComment(stream=${stream},state=${state})`, { "event.action": "tokenBlockComment", "logrite.stream": stream, "logrite.state": state });
      var maybeEnd = false, ch;
      while (ch = stream.next()) {
        if (maybeEnd && ch == "/") {
          state.tokenize = null;
		logger.debug(`tokenBlockComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenBlockComment", "logrite.stream": stream, "logrite.state": state });
          break;
        }
        maybeEnd = (ch == "*");
      }
	logger.error(`tokenBlockComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`, { "event.action": "tokenBlockComment", "logrite.stream": stream, "logrite.state": state });
      return "comment";
    }

    function tokenString(stream, state) {
	logger.error(`>>> Entering tokenString(stream=${stream},state=${state})`, { "event.action": "tokenString", "logrite.stream": stream, "logrite.state": state });
      var escaped = false, ch;
      while ((ch = stream.next()) != null) {
		logger.error(`tokenString(stream=${stream},state=${state}): escaped → ${escaped}`, { "event.action": "tokenString", "logrite.stream": stream, "logrite.state": state });
        if (ch == '"' && !escaped) {
          state.tokenize = null;
		logger.debug(`tokenString(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenString", "logrite.stream": stream, "logrite.state": state });
          state.sol = false;
		logger.debug(`tokenString(stream=${stream},state=${state}): state.sol → ${state.sol}`, { "event.action": "tokenString", "logrite.stream": stream, "logrite.state": state });
          break;
        }
        escaped = !escaped && ch == "\\";
      }

	logger.warn(`<<< Exiting tokenString(stream=${stream},state=${state})`, { "event.action": "tokenString", "logrite.stream": stream, "logrite.state": state });
      return "string";
    }

    function tokenIdent(stream, state) {
	logger.error(`>>> Entering tokenIdent(stream=${stream},state=${state})`, { "event.action": "tokenIdent", "logrite.stream": stream, "logrite.state": state });
      stream.eatWhile(isDigit);
      while (stream.eat(isDigit) || stream.eat(isNonDigit)) { }


      var cur = stream.current();
	logger.error(`tokenIdent(stream=${stream},state=${state}): cur → ${cur}`, { "event.action": "tokenIdent", "logrite.stream": stream, "logrite.state": state });

      if(state.sol && (cur == "package" || cur == "model" || cur == "when" || cur == "connector")) state.level++;
      else if(state.sol && cur == "end" && state.level > 0) state.level--;

      state.tokenize = null;
	logger.debug(`tokenIdent(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenIdent", "logrite.stream": stream, "logrite.state": state });
      state.sol = false;
	logger.debug(`tokenIdent(stream=${stream},state=${state}): state.sol → ${state.sol}`, { "event.action": "tokenIdent", "logrite.stream": stream, "logrite.state": state });

      if (keywords.propertyIsEnumerable(cur)) return "keyword";
      else if (builtin.propertyIsEnumerable(cur)) return "builtin";
      else if (atoms.propertyIsEnumerable(cur)) return "atom";
      else return "variable";
    }

    function tokenQIdent(stream, state) {
	logger.error(`>>> Entering tokenQIdent(stream=${stream},state=${state})`, { "event.action": "tokenQIdent", "logrite.stream": stream, "logrite.state": state });
      while (stream.eat(/[^']/)) { }

      state.tokenize = null;
	logger.debug(`tokenQIdent(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenQIdent", "logrite.stream": stream, "logrite.state": state });
      state.sol = false;
	logger.debug(`tokenQIdent(stream=${stream},state=${state}): state.sol → ${state.sol}`, { "event.action": "tokenQIdent", "logrite.stream": stream, "logrite.state": state });

      if(stream.eat("'"))
        return "variable";
      else
        return "error";
    }

    function tokenUnsignedNuber(stream, state) {
	logger.error(`>>> Entering tokenUnsignedNuber(stream=${stream},state=${state})`, { "event.action": "tokenUnsignedNuber", "logrite.stream": stream, "logrite.state": state });
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
	logger.debug(`tokenUnsignedNuber(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenUnsignedNuber", "logrite.stream": stream, "logrite.state": state });
      state.sol = false;
	logger.debug(`tokenUnsignedNuber(stream=${stream},state=${state}): state.sol → ${state.sol}`, { "event.action": "tokenUnsignedNuber", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting tokenUnsignedNuber(stream=${stream},state=${state})`, { "event.action": "tokenUnsignedNuber", "logrite.stream": stream, "logrite.state": state });
      return "number";
    }

    // Interface
    return {
      startState: function() {
		logger.error('>>> Entering startState()', { "event.action": "startState" });
        return {
          tokenize: null,
          level: 0,
          sol: true
        };
		logger.warn('<<< Exiting startState()', { "event.action": "startState" });
      },

      token: function(stream, state) {
		logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        if(state.tokenize != null) {
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          return state.tokenize(stream, state);
        }

        if(stream.sol()) {
          state.sol = true;
		logger.debug(`token(stream=${stream},state=${state}): state.sol → ${state.sol}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        }

        // WHITESPACE
        if(stream.eatSpace()) {
          state.tokenize = null;
		logger.debug(`token(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
          return null;
        }

        var ch = stream.next();
		logger.error(`token(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });

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
		logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
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
	logger.error(`>>> Entering words(str=${str})`, { "event.action": "words", "logrite.str": str });
    var obj = {}, words = str.split(" ");
    for (var i=0; i<words.length; ++i)
      obj[words[i]] = true;
	logger.warn(`<<< Exiting words(str=${str})`, { "event.action": "words", "logrite.str": str });
    return obj;
  }

  var modelicaKeywords = "algorithm and annotation assert block break class connect connector constant constrainedby der discrete each else elseif elsewhen encapsulated end enumeration equation expandable extends external false final flow for function if import impure in initial inner input loop model not operator or outer output package parameter partial protected public pure record redeclare replaceable return stream then true type when while within";
  var modelicaBuiltin = "abs acos actualStream asin atan atan2 cardinality ceil cos cosh delay div edge exp floor getInstanceName homotopy inStream integer log log10 mod pre reinit rem semiLinear sign sin sinh spatialDistribution sqrt tan tanh";
  var modelicaAtoms = "Real Boolean Integer String";

  function def(mimes, mode) {
	logger.error(`>>> Entering def(mimes=${mimes},mode=${mode})`, { "event.action": "def", "logrite.mimes": mimes, "logrite.mode": mode });
    if (typeof mimes == "string")
      mimes = [mimes];

    var words = [];
	logger.warn(`<<< Exiting def(mimes=${mimes},mode=${mode})`, { "event.action": "def", "logrite.mimes": mimes, "logrite.mode": mode });

    function add(obj) {
	logger.error(`>>> Entering add(obj=${obj})`, { "event.action": "add", "logrite.obj": obj });
      if (obj)
        for (var prop in obj)
          if (obj.hasOwnProperty(prop))
            words.push(prop);
			logger.warn(`<<< Exiting add(obj=${obj})`, { "event.action": "add", "logrite.obj": obj });
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
