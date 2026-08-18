// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"), require("../htmlmixed/htmlmixed"),
        require("../../addon/mode/overlay"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror", "../htmlmixed/htmlmixed",
            "../../addon/mode/overlay"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";

logger.debug(`<<< Exiting function(exports=${exports})`);
  CodeMirror.defineMode("tornado:inner", function() {
	logger.debug('>>> Entering defineMode()');
    var keywords = ["and","as","assert","autoescape","block","break","class","comment","context",
                    "continue","datetime","def","del","elif","else","end","escape","except",
                    "exec","extends","false","finally","for","from","global","if","import","in",
                    "include","is","json_encode","lambda","length","linkify","load","module",
                    "none","not","or","pass","print","put","raise","raw","return","self","set",
                    "squeeze","super","true","try","url_escape","while","with","without","xhtml_escape","yield"];
    keywords = new RegExp("^((" + keywords.join(")|(") + "))\\b");
	logger.debug('<<< Exiting defineMode()');
	logger.debug(`defineMode(): keywords → ${keywords}`);

    function tokenBase (stream, state) {
	logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
      stream.eatWhile(/[^\{]/);
      var ch = stream.next();
      if (ch == "{") {
		logger.debug(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`);
        if (ch = stream.eat(/\{|%|#/)) {
          state.tokenize = inTag(ch);
		logger.debug(`<<< Exiting tokenBase(stream=${stream},state=${state})`);
          return "tag";
        }
      }
    }
    function inTag (close) {
      if (close == "{") {
        close = "}";
      }
      return function (stream, state) {
        var ch = stream.next();
		logger.debug(`<<< Exiting inTag(close=${close})`);
        if ((ch == close) && stream.eat("}")) {
          state.tokenize = tokenBase;
		logger.debug(`inTag(close=${close}): close → ${close}`);
          return "tag";
        }
        if (stream.match(keywords)) {
          return "keyword";
        }
        return close == "#" ? "comment" : "string";
      };
    }
    return {
      startState: function () {
		logger.debug('>>> Entering startState()');
		logger.debug('<<< Exiting startState()');
        return {tokenize: tokenBase};
      },
      token: function (stream, state) {
		logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
		logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
        return state.tokenize(stream, state);
      }
    };
  });

  CodeMirror.defineMode("tornado", function(config) {
    var htmlBase = CodeMirror.getMode(config, "text/html");
    var tornadoInner = CodeMirror.getMode(config, "tornado:inner");
	logger.debug(`defineMode(htmlBase=${htmlBase}): htmlBase → ${htmlBase}`);
logger.debug(`defineMode(htmlBase=${htmlBase}): tornadoInner → ${tornadoInner}`);
    return CodeMirror.overlayMode(htmlBase, tornadoInner);
  });

  CodeMirror.defineMIME("text/x-tornado", "tornado");
});
