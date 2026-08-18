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

logger.debug(`<<< Exiting function(exports=${exports})`);
CodeMirror.defineMode("velocity", function() {
    function parseWords(str) {
		logger.debug(`>>> Entering parseWords(str=${str})`);
        var obj = {}, words = str.split(" ");
        for (var i = 0; i < words.length; ++i) obj[words[i]] = true;
		logger.debug(`parseWords(str=${str}): obj → ${obj}`);
		logger.debug(`<<< Exiting parseWords(str=${str})`);
        return obj;
    }

    var keywords = parseWords("#end #else #break #stop #[[ #]] " +
                              "#{end} #{else} #{break} #{stop}");
    var functions = parseWords("#if #elseif #foreach #set #include #parse #macro #define #evaluate " +
                               "#{if} #{elseif} #{foreach} #{set} #{include} #{parse} #{macro} #{define} #{evaluate}");
    var specials = parseWords("$foreach.count $foreach.hasNext $foreach.first $foreach.last $foreach.topmost $foreach.parent.count $foreach.parent.hasNext $foreach.parent.first $foreach.parent.last $foreach.parent $velocityCount $!bodyContent $bodyContent");
    var isOperatorChar = /[+\-*&%=<>!?:\/|]/;

    function chain(stream, state, f) {
		logger.debug(`>>> Entering chain(stream=${stream},state=${state},f=${f})`);
        state.tokenize = f;
		logger.debug(`chain(stream=${stream},state=${state},f=${f}): state.tokenize → ${state.tokenize}`);
		logger.debug(`<<< Exiting chain(stream=${stream},state=${state},f=${f})`);
        return f(stream, state);
    }
    function tokenBase(stream, state) {
		logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
        var beforeParams = state.beforeParams;
        state.beforeParams = false;
		logger.debug(`tokenBase(stream=${stream},state=${state}): state.beforeParams → ${state.beforeParams}`);
        var ch = stream.next();
		logger.debug(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`);
        // start of unparsed string?
        if ((ch == "'") && state.inParams) {
            state.lastTokenWasBuiltin = false;
			logger.debug(`tokenBase(stream=${stream},state=${state}): state.lastTokenWasBuiltin → ${state.lastTokenWasBuiltin}`);
            return chain(stream, state, tokenString(ch));
        }
        // start of parsed string?
        else if ((ch == '"')) {
            state.lastTokenWasBuiltin = false;
            if (state.inString) {
                state.inString = false;
				logger.debug(`tokenBase(stream=${stream},state=${state}): state.inString → ${state.inString}`);
                return "string";
            }
            else if (state.inParams)
                return chain(stream, state, tokenString(ch));
        }
        // is it one of the special signs []{}().,;? Seperator?
        else if (/[\[\]{}\(\),;\.]/.test(ch)) {
            if (ch == "(" && beforeParams)
                state.inParams = true;
            else if (ch == ")") {
                state.inParams = false;
				logger.debug(`tokenBase(stream=${stream},state=${state}): state.inParams → ${state.inParams}`);
                state.lastTokenWasBuiltin = true;
            }
            return null;
        }
        // start of a number value?
        else if (/\d/.test(ch)) {
            state.lastTokenWasBuiltin = false;
            stream.eatWhile(/[\w\.]/);
            return "number";
        }
        // multi line comment?
        else if (ch == "#" && stream.eat("*")) {
            state.lastTokenWasBuiltin = false;
            return chain(stream, state, tokenComment);
        }
        // unparsed content?
        else if (ch == "#" && stream.match(/ *\[ *\[/)) {
            state.lastTokenWasBuiltin = false;
            return chain(stream, state, tokenUnparsed);
        }
        // single line comment?
        else if (ch == "#" && stream.eat("#")) {
            state.lastTokenWasBuiltin = false;
            stream.skipToEnd();
            return "comment";
        }
        // variable?
        else if (ch == "$") {
            stream.eatWhile(/[\w\d\$_\.{}]/);
            // is it one of the specials?
            if (specials && specials.propertyIsEnumerable(stream.current())) {
                return "keyword";
            }
            else {
                state.lastTokenWasBuiltin = true;
                state.beforeParams = true;
                return "builtin";
            }
        }
        // is it a operator?
        else if (isOperatorChar.test(ch)) {
            state.lastTokenWasBuiltin = false;
            stream.eatWhile(isOperatorChar);
            return "operator";
        }
        else {
            // get the whole word
            stream.eatWhile(/[\w\$_{}@]/);
            var word = stream.current();
            // is it one of the listed keywords?
            if (keywords && keywords.propertyIsEnumerable(word))
                return "keyword";
            // is it one of the listed functions?
            if (functions && functions.propertyIsEnumerable(word) ||
                    (stream.current().match(/^#@?[a-z0-9_]+ *$/i) && stream.peek()=="(") &&
                     !(functions && functions.propertyIsEnumerable(word.toLowerCase()))) {
                state.beforeParams = true;
                state.lastTokenWasBuiltin = false;
                return "keyword";
            }
            if (state.inString) {
                state.lastTokenWasBuiltin = false;
                return "string";
            }
            if (stream.pos > word.length && stream.string.charAt(stream.pos-word.length-1)=="." && state.lastTokenWasBuiltin)
                return "builtin";
            // default: just a "word"
            state.lastTokenWasBuiltin = false;
			logger.debug(`<<< Exiting tokenBase(stream=${stream},state=${state})`);
            return null;
        }
    }

    function tokenString(quote) {
		logger.debug(`>>> Entering tokenString(quote=${quote})`);
        return function(stream, state) {
            var escaped = false, next, end = false;
            while ((next = stream.next()) != null) {
                if ((next == quote) && !escaped) {
                    end = true;
                    break;
                }
                if (quote=='"' && stream.peek() == '$' && !escaped) {
                    state.inString = true;
					logger.debug(`tokenString(quote=${quote}): state.inString → ${state.inString}`);
                    end = true;
					logger.debug(`function(stream=${stream},state=${state}): state.inString → ${state.inString}`);
                    break;
                }
                escaped = !escaped && next == "\\";
            }
            if (end) state.tokenize = tokenBase;
			logger.debug(`<<< Exiting tokenString(quote=${quote})`);
            return "string";
        };
    }

    function tokenComment(stream, state) {
		logger.debug(`>>> Entering tokenComment(stream=${stream},state=${state})`);
        var maybeEnd = false, ch;
        while (ch = stream.next()) {
            if (ch == "#" && maybeEnd) {
                state.tokenize = tokenBase;
				logger.debug(`tokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
                break;
            }
            maybeEnd = (ch == "*");
        }
		logger.debug(`tokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
		logger.debug(`<<< Exiting tokenComment(stream=${stream},state=${state})`);
        return "comment";
    }

    function tokenUnparsed(stream, state) {
		logger.debug(`>>> Entering tokenUnparsed(stream=${stream},state=${state})`);
        var maybeEnd = 0, ch;
        while (ch = stream.next()) {
            if (ch == "#" && maybeEnd == 2) {
                state.tokenize = tokenBase;
				logger.debug(`tokenUnparsed(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
                break;
            }
            if (ch == "]")
                maybeEnd++;
            else if (ch != " ")
                maybeEnd = 0;
        }
		logger.debug(`<<< Exiting tokenUnparsed(stream=${stream},state=${state})`);
        return "meta";
    }
    // Interface

    return {
        startState: function() {
			logger.debug('>>> Entering startState()');
            return {
                tokenize: tokenBase,
                beforeParams: false,
                inParams: false,
                inString: false,
                lastTokenWasBuiltin: false
            };
			logger.debug('<<< Exiting startState()');
        },

        token: function(stream, state) {
			logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
            if (stream.eatSpace()) return null;
			logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
            return state.tokenize(stream, state);
        },
        blockCommentStart: "#*",
        blockCommentEnd: "*#",
        lineComment: "##",
        fold: "velocity"
    };
});

CodeMirror.defineMIME("text/velocity", "velocity");

});
