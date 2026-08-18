const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/*
  DTD mode
  Ported to CodeMirror by Peter Kroon <plakroon@gmail.com>
  Report bugs/issues here: https://github.com/codemirror/CodeMirror/issues
  GitHub: @peterkroon
*/

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

logger.debug(`<<< Exiting function(exports=${exports})`);
CodeMirror.defineMode("dtd", function(config) {
logger.debug('>>> Entering defineMode()');
  var indentUnit = config.indentUnit, type;
logger.debug('<<< Exiting defineMode()');
  function ret(style, tp) {type = tp; return style;}

  function tokenBase(stream, state) {
	logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
    var ch = stream.next();

	logger.debug(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`);
    if (ch == "<" && stream.eat("!") ) {
      if (stream.eatWhile(/[\-]/)) {
        state.tokenize = tokenSGMLComment;
		logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        return tokenSGMLComment(stream, state);
      } else if (stream.eatWhile(/[\w]/)) return ret("keyword", "doindent");
    } else if (ch == "<" && stream.eat("?")) { //xml declaration
      state.tokenize = inBlock("meta", "?>");
	logger.debug(`<<< Exiting tokenBase(stream=${stream},state=${state})`);
      return ret("meta", ch);
    } else if (ch == "#" && stream.eatWhile(/[\w]/)) return ret("atom", "tag");
    else if (ch == "|") return ret("keyword", "seperator");
    else if (ch.match(/[\(\)\[\]\-\.,\+\?>]/)) return ret(null, ch);//if(ch === ">") return ret(null, "endtag"); else
    else if (ch.match(/[\[\]]/)) return ret("rule", ch);
    else if (ch == "\"" || ch == "'") {
      state.tokenize = tokenString(ch);
      return state.tokenize(stream, state);
    } else if (stream.eatWhile(/[a-zA-Z\?\+\d]/)) {
      var sc = stream.current();
      if( sc.substr(sc.length-1,sc.length).match(/\?|\+/) !== null )stream.backUp(1);
      return ret("tag", "tag");
    } else if (ch == "%" || ch == "*" ) return ret("number", "number");
    else {
      stream.eatWhile(/[\w\\\-_%.{,]/);
      return ret(null, null);
    }
  }

  function tokenSGMLComment(stream, state) {
	logger.debug(`>>> Entering tokenSGMLComment(stream=${stream},state=${state})`);
    var dashes = 0, ch;
    while ((ch = stream.next()) != null) {
      if (dashes >= 2 && ch == ">") {
        state.tokenize = tokenBase;
		logger.debug(`tokenSGMLComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        break;
      }
      dashes = (ch == "-") ? dashes + 1 : 0;
    }
	logger.debug(`tokenSGMLComment(stream=${stream},state=${state}): dashes → ${dashes}`);
	logger.debug(`<<< Exiting tokenSGMLComment(stream=${stream},state=${state})`);
    return ret("comment", "comment");
  }

  function tokenString(quote) {
	logger.debug(`>>> Entering tokenString(quote=${quote})`);
    return function(stream, state) {
      var escaped = false, ch;
      while ((ch = stream.next()) != null) {
        if (ch == quote && !escaped) {
          state.tokenize = tokenBase;
          break;
        }
        escaped = !escaped && ch == "\\";
      }
	logger.debug(`tokenString(quote=${quote}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting tokenString(quote=${quote})`);
      return ret("string", "tag");
    };
  }

  function inBlock(style, terminator) {
    return function(stream, state) {
      while (!stream.eol()) {
        if (stream.match(terminator)) {
          state.tokenize = tokenBase;
          break;
        }
        stream.next();
      }
      return style;
    };
  }

  return {
    startState: function(base) {
	logger.debug(`>>> Entering startState(base=${base})`);
      return {tokenize: tokenBase,
              baseIndent: base || 0,
              stack: []};
			logger.debug(`<<< Exiting startState(base=${base})`);
    },

    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      if (stream.eatSpace()) return null;
      var style = state.tokenize(stream, state);
	logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);

      var context = state.stack[state.stack.length-1];
      if (stream.current() == "[" || type === "doindent" || type == "[") state.stack.push("rule");
      else if (type === "endtag") state.stack[state.stack.length-1] = "endtag";
      else if (stream.current() == "]" || type == "]" || (type == ">" && context == "rule")) state.stack.pop();
      else if (type == "[") state.stack.push("[");
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      return style;
    },

    indent: function(state, textAfter) {
	logger.debug(`>>> Entering indent(state=${state},textAfter=${textAfter})`);
      var n = state.stack.length;

	logger.debug(`<<< Exiting indent(state=${state},textAfter=${textAfter})`);
      if( textAfter.match(/\]\s+|\]/) )n=n-1;
      else if(textAfter.substr(textAfter.length-1, textAfter.length) === ">"){
        if(textAfter.substr(0,1) === "<")n;
        else if( type == "doindent" && textAfter.length > 1 )n;
        else if( type == "doindent")n--;
        else if( type == ">" && textAfter.length > 1)n;
        else if( type == "tag" && textAfter !== ">")n;
        else if( type == "tag" && state.stack[state.stack.length-1] == "rule")n--;
        else if( type == "tag")n++;
        else if( textAfter === ">" && state.stack[state.stack.length-1] == "rule" && type === ">")n--;
        else if( textAfter === ">" && state.stack[state.stack.length-1] == "rule")n;
        else if( textAfter.substr(0,1) !== "<" && textAfter.substr(0,1) === ">" )n=n-1;
        else if( textAfter === ">")n;
        else n=n-1;
        //over rule them all
        if(type == null || type == "]")n--;
      }

      return state.baseIndent + n * indentUnit;
    },

    electricChars: "]>"
  };
});

CodeMirror.defineMIME("application/xml-dtd", "dtd");

});
