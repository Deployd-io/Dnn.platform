const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// By the Neo4j Team and contributors.
// https://github.com/neo4j-contrib/CodeMirror

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
  var wordRegexp = function(words) {
	logger.debug(`>>> Entering wordRegexp(words=${words})`);
	logger.debug(`<<< Exiting wordRegexp(words=${words})`);
    return new RegExp("^(?:" + words.join("|") + ")$", "i");
  };

  CodeMirror.defineMode("cypher", function(config) {
	logger.debug('>>> Entering defineMode()'); const __log_start = Date.now();
    var tokenBase = function(stream/*, state*/) {
	logger.debug(`>>> Entering tokenBase(stream=${stream})`);
      var ch = stream.next();
      if (ch === "\"" || ch === "'") {
		logger.debug(`tokenBase(stream=${stream}): ch → ${ch}`);
        stream.match(/.+?["']/);
        return "string";
      }
      if (/[{}\(\),\.;\[\]]/.test(ch)) {
        curPunc = ch;
        return "node";
      } else if (ch === "/" && stream.eat("/")) {
        stream.skipToEnd();
        return "comment";
      } else if (operatorChars.test(ch)) {
        stream.eatWhile(operatorChars);
        return null;
      } else {
        stream.eatWhile(/[_\w\d]/);
        if (stream.eat(":")) {
          stream.eatWhile(/[\w\d_\-]/);
          return "atom";
        }
        var word = stream.current();
		logger.debug(`tokenBase(stream=${stream}): word → ${word}`);
        if (funcs.test(word)) return "builtin";
        if (preds.test(word)) return "def";
        if (keywords.test(word)) return "keyword";
		logger.debug(`<<< Exiting tokenBase(stream=${stream})`);
        return "variable";
      }
    };
    var pushContext = function(state, type, col) {
	logger.debug(`>>> Entering pushContext(state=${state},type=${type},col=${col})`);
	logger.debug(`<<< Exiting pushContext(state=${state},type=${type},col=${col})`);
      return state.context = {
        prev: state.context,
        indent: state.indent,
        col: col,
        type: type
      };
    };
    var popContext = function(state) {
	logger.debug(`>>> Entering popContext(state=${state})`);
      state.indent = state.context.indent;
	logger.debug(`popContext(state=${state}): state.indent → ${state.indent}`);
	logger.debug(`<<< Exiting popContext(state=${state})`);
      return state.context = state.context.prev;
    };
    var indentUnit = config.indentUnit;
    var curPunc;
	logger.debug(`defineMode(): curPunc → ${curPunc}`);
    var funcs = wordRegexp(["abs", "acos", "allShortestPaths", "asin", "atan", "atan2", "avg", "ceil", "coalesce", "collect", "cos", "cot", "count", "degrees", "e", "endnode", "exp", "extract", "filter", "floor", "haversin", "head", "id", "keys", "labels", "last", "left", "length", "log", "log10", "lower", "ltrim", "max", "min", "node", "nodes", "percentileCont", "percentileDisc", "pi", "radians", "rand", "range", "reduce", "rel", "relationship", "relationships", "replace", "right", "round", "rtrim", "shortestPath", "sign", "sin", "split", "sqrt", "startnode", "stdev", "stdevp", "str", "substring", "sum", "tail", "tan", "timestamp", "toFloat", "toInt", "trim", "type", "upper"]);
	logger.debug(`defineMode(): funcs → ${funcs}`);
    var preds = wordRegexp(["all", "and", "any", "has", "in", "none", "not", "or", "single", "xor", "like", "ilike", "exists"]);
	logger.debug(`defineMode(): preds → ${preds}`);
    var keywords = wordRegexp(["as", "asc", "ascending", "assert", "by", "case", "commit", "constraint", "create", "csv", "cypher", "delete", "desc", "descending", "distinct", "drop", "else", "end", "explain", "false", "fieldterminator", "foreach", "from", "headers", "in", "index", "is", "join", "limit", "load", "match", "merge", "null", "on", "optional", "order", "periodic", "profile", "remove", "return", "scan", "set", "skip", "start", "then", "true", "union", "unique", "unwind", "using", "when", "where", "with"]);
	logger.debug(`defineMode(): keywords → ${keywords}`);
    var operatorChars = /[*+\-<>=&|~%^]/;

    return {
      startState: function(/*base*/) {
		logger.debug('>>> Entering startState()');
        return {
          tokenize: tokenBase,
          context: null,
          indent: 0,
          col: 0
        };
		logger.debug('<<< Exiting startState()');
      },
      token: function(stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
        if (stream.sol()) {
          if (state.context && (state.context.align == null)) {
            state.context.align = false;
          }
          state.indent = stream.indentation();
        }
        if (stream.eatSpace()) {
          return null;
        }
        var style = state.tokenize(stream, state);
		logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
        if (style !== "comment" && state.context && (state.context.align == null) && state.context.type !== "pattern") {
		logger.debug(`function(exports=${exports}): state.indent → ${state.indent}`);
          state.context.align = true;
		logger.debug(`defineMode(): state.indent → ${state.indent}`);
        }
        if (curPunc === "(") {
          pushContext(state, ")", stream.column());
        } else if (curPunc === "[") {
          pushContext(state, "]", stream.column());
        } else if (curPunc === "{") {
          pushContext(state, "}", stream.column());
        } else if (/[\]\}\)]/.test(curPunc)) {
          while (state.context && state.context.type === "pattern") {
            popContext(state);
          }
          if (state.context && curPunc === state.context.type) {
            popContext(state);
          }
        } else if (curPunc === "." && state.context && state.context.type === "pattern") {
          popContext(state);
        } else if (/atom|string|variable/.test(style) && state.context) {
          if (/[\}\]]/.test(state.context.type)) {
            pushContext(state, "pattern", stream.column());
          } else if (state.context.type === "pattern" && !state.context.align) {
            state.context.align = true;
            state.context.col = stream.column();
			logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
          }
		logger.info(`defineMode()=: delete operation took ${Date.now() - __log_start} ms`);
        }
		logger.debug('<<< Exiting defineMode()');
        return style;
      },
      indent: function(state, textAfter) {
		logger.debug(`>>> Entering indent(state=${state},textAfter=${textAfter})`);
        var firstChar = textAfter && textAfter.charAt(0);
        var context = state.context;
		logger.debug(`indent(state=${state},textAfter=${textAfter}): firstChar → ${firstChar}`);
        if (/[\]\}]/.test(firstChar)) {
          while (context && context.type === "pattern") {
            context = context.prev;
			logger.debug(`<<< Exiting indent(state=${state},textAfter=${textAfter})`);
          }
        }
        var closing = context && firstChar === context.type;
        if (!context) return 0;
        if (context.type === "keywords") return CodeMirror.commands.newlineAndIndent;
        if (context.align) return context.col + (closing ? 0 : 1);
		logger.debug(`<<< Exiting function(exports=${exports})`);
        return context.indent + (closing ? 0 : indentUnit);
      }
    };
  });

  CodeMirror.modeExtensions["cypher"] = {
    autoFormatLineBreaks: function(text) {
	logger.debug(`>>> Entering autoFormatLineBreaks(text=${text})`);
      var i, lines, reProcessedPortion;
      var lines = text.split("\n");
	logger.debug(`autoFormatLineBreaks(text=${text}): lines → ${lines}`);
      var reProcessedPortion = /\s+\b(return|where|order by|match|with|skip|limit|create|delete|set)\b\s/g;
      for (var i = 0; i < lines.length; i++)
		logger.debug(`autoFormatLineBreaks(text=${text}): reProcessedPortion → ${reProcessedPortion}`);
        lines[i] = lines[i].replace(reProcessedPortion, " \n$1 ").trim();
		logger.debug(`<<< Exiting autoFormatLineBreaks(text=${text})`);
      return lines.join("\n");
    }
  };

  CodeMirror.defineMIME("application/x-cypher-query", "cypher");

});
