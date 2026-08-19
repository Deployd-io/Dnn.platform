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

  function forEach(arr, f) {
	logger.debug(`>>> Entering forEach(arr=${arr},f=${f})`);
    for (var i = 0, e = arr.length; i < e; ++i) f(arr[i]);
	logger.debug(`<<< Exiting forEach(arr=${arr},f=${f})`);
  }

  function arrayContains(arr, item) {
	logger.debug(`>>> Entering arrayContains(arr=${arr},item=${item})`);
    if (!Array.prototype.indexOf) {
      var i = arr.length;
      while (i--) {
        if (arr[i] === item) {
          return true;
        }
      }
	logger.debug(`<<< Exiting arrayContains(arr=${arr},item=${item})`);
      return false;
    }
    return arr.indexOf(item) != -1;
  }

  function scriptHint(editor, _keywords, getToken) {
    // Find the token at the cursor
    var cur = editor.getCursor(), token = getToken(editor, cur), tprop = token;
    // If it's not a 'word-style' token, ignore the token.

    if (!/^[\w$_]*$/.test(token.string)) {
        token = tprop = {start: cur.ch, end: cur.ch, string: "", state: token.state,
                         className: token.string == ":" ? "python-type" : null};
    }

    if (!context) var context = [];
    context.push(tprop);

    var completionList = getCompletions(token, context);
    completionList = completionList.sort();

    return {list: completionList,
            from: CodeMirror.Pos(cur.line, token.start),
            to: CodeMirror.Pos(cur.line, token.end)};
  }

  function pythonHint(editor) {
	logger.debug(`>>> Entering pythonHint(editor=${editor})`);
	logger.debug(`<<< Exiting pythonHint(editor=${editor})`);
    return scriptHint(editor, pythonKeywordsU, function (e, cur) {return e.getTokenAt(cur);});
  }
  CodeMirror.registerHelper("hint", "python", pythonHint);

  var pythonKeywords = "and del from not while as elif global or with assert else if pass yield"
+ "break except import print class exec in raise continue finally is return def for lambda try";
  var pythonKeywordsL = pythonKeywords.split(" ");
  var pythonKeywordsU = pythonKeywords.toUpperCase().split(" ");

  var pythonBuiltins = "abs divmod input open staticmethod all enumerate int ord str "
+ "any eval isinstance pow sum basestring execfile issubclass print super"
+ "bin file iter property tuple bool filter len range type"
+ "bytearray float list raw_input unichr callable format locals reduce unicode"
+ "chr frozenset long reload vars classmethod getattr map repr xrange"
+ "cmp globals max reversed zip compile hasattr memoryview round __import__"
+ "complex hash min set apply delattr help next setattr buffer"
+ "dict hex object slice coerce dir id oct sorted intern ";
  var pythonBuiltinsL = pythonBuiltins.split(" ").join("() ").split(" ");
  var pythonBuiltinsU = pythonBuiltins.toUpperCase().split(" ").join("() ").split(" ");

  function getCompletions(token, context) {
    var found = [], start = token.string;
    function maybeAdd(str) {
	logger.debug(`>>> Entering maybeAdd(str=${str})`);
      if (str.lastIndexOf(start, 0) == 0 && !arrayContains(found, str)) found.push(str);
	logger.debug(`<<< Exiting maybeAdd(str=${str})`);
    }

    function gatherCompletions(_obj) {
		logger.debug(`>>> Entering gatherCompletions(_obj=${_obj})`);
        forEach(pythonBuiltinsL, maybeAdd);
        forEach(pythonBuiltinsU, maybeAdd);
        forEach(pythonKeywordsL, maybeAdd);
        forEach(pythonKeywordsU, maybeAdd);
		logger.debug(`<<< Exiting gatherCompletions(_obj=${_obj})`);
    }

    if (context) {
      // If this is a property, see if it belongs to some object we can
      // find in the current environment.
      var obj = context.pop(), base;

      if (obj.type == "variable")
          base = obj.string;
      else if(obj.type == "variable-3")
          base = ":" + obj.string;

      while (base != null && context.length)
        base = base[context.pop().string];
      if (base != null) gatherCompletions(base);
    }
    return found;
  }
});
