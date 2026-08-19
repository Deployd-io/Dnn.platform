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

CodeMirror.defineMode("haxe", function(config, parserConfig) {
logger.debug('>>> Entering defineMode()');
  var indentUnit = config.indentUnit;

  // Tokenizer

  var keywords = function(){
    function kw(type) {return {type: type, style: "keyword"};}
    var A = kw("keyword a"), B = kw("keyword b"), C = kw("keyword c");
	logger.debug(`function(): A → ${A}`);
    var operator = kw("operator"), atom = {type: "atom", style: "atom"}, attribute = {type:"attribute", style: "attribute"};
  var type = kw("typedef");
    return {
      "if": A, "while": A, "else": B, "do": B, "try": B,
      "return": C, "break": C, "continue": C, "new": C, "throw": C,
      "var": kw("var"), "inline":attribute, "static": attribute, "using":kw("import"),
    "public": attribute, "private": attribute, "cast": kw("cast"), "import": kw("import"), "macro": kw("macro"),
      "function": kw("function"), "catch": kw("catch"), "untyped": kw("untyped"), "callback": kw("cb"),
      "for": kw("for"), "switch": kw("switch"), "case": kw("case"), "default": kw("default"),
      "in": operator, "never": kw("property_access"), "trace":kw("trace"),
    "class": type, "abstract":type, "enum":type, "interface":type, "typedef":type, "extends":type, "implements":type, "dynamic":type,
      "true": atom, "false": atom, "null": atom
    };
  }();

  var isOperatorChar = /[+\-*&%=<>!?|]/;

  function chain(stream, state, f) {
	logger.debug(`>>> Entering chain(stream=${stream},state=${state},f=${f})`);
    state.tokenize = f;
	logger.debug(`chain(stream=${stream},state=${state},f=${f}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting chain(stream=${stream},state=${state},f=${f})`);
    return f(stream, state);
  }

  function nextUntilUnescaped(stream, end) {
	logger.debug(`>>> Entering nextUntilUnescaped(stream=${stream},end=${end})`);
    var escaped = false, next;
    while ((next = stream.next()) != null) {
      if (next == end && !escaped)
        return false;
      escaped = !escaped && next == "\\";
    }
	logger.debug(`nextUntilUnescaped(stream=${stream},end=${end}): escaped → ${escaped}`);
	logger.debug(`<<< Exiting nextUntilUnescaped(stream=${stream},end=${end})`);
    return escaped;
  }

  // Used as scratch variables to communicate multiple values without
  // consing up tons of objects.
  var type, content;
  function ret(tp, style, cont) {
	logger.debug(`>>> Entering ret(tp=${tp},style=${style},cont=${cont})`);
    type = tp; content = cont;
	logger.debug(`<<< Exiting ret(tp=${tp},style=${style},cont=${cont})`);
    return style;
  }

  function haxeTokenBase(stream, state) {
	logger.debug(`>>> Entering haxeTokenBase(stream=${stream},state=${state})`);
    var ch = stream.next();
    if (ch == '"' || ch == "'")
      return chain(stream, state, haxeTokenString(ch));
    else if (/[\[\]{}\(\),;\:\.]/.test(ch))
      return ret(ch);
    else if (ch == "0" && stream.eat(/x/i)) {
      stream.eatWhile(/[\da-f]/i);
      return ret("number", "number");
    }
    else if (/\d/.test(ch) || ch == "-" && stream.eat(/\d/)) {
      stream.match(/^\d*(?:\.\d*)?(?:[eE][+\-]?\d+)?/);
      return ret("number", "number");
    }
    else if (state.reAllowed && (ch == "~" && stream.eat(/\//))) {
      nextUntilUnescaped(stream, "/");
      stream.eatWhile(/[gimsu]/);
	logger.debug(`<<< Exiting haxeTokenBase(stream=${stream},state=${state})`);
      return ret("regexp", "string-2");
    }
    else if (ch == "/") {
      if (stream.eat("*")) {
        return chain(stream, state, haxeTokenComment);
      }
      else if (stream.eat("/")) {
        stream.skipToEnd();
        return ret("comment", "comment");
      }
      else {
        stream.eatWhile(isOperatorChar);
        return ret("operator", null, stream.current());
      }
    }
    else if (ch == "#") {
        stream.skipToEnd();
        return ret("conditional", "meta");
    }
    else if (ch == "@") {
      stream.eat(/:/);
      stream.eatWhile(/[\w_]/);
      return ret ("metadata", "meta");
    }
    else if (isOperatorChar.test(ch)) {
      stream.eatWhile(isOperatorChar);
      return ret("operator", null, stream.current());
    }
    else {
    var word;
    if(/[A-Z]/.test(ch))
    {
      stream.eatWhile(/[\w_<>]/);
      word = stream.current();
      return ret("type", "variable-3", word);
    }
    else
    {
        stream.eatWhile(/[\w_]/);
        var word = stream.current(), known = keywords.propertyIsEnumerable(word) && keywords[word];
        return (known && state.kwAllowed) ? ret(known.type, known.style, word) :
                       ret("variable", "variable", word);
    }
    }
  }

  function haxeTokenString(quote) {
	logger.debug(`>>> Entering haxeTokenString(quote=${quote})`);
    return function(stream, state) {
      if (!nextUntilUnescaped(stream, quote))
        state.tokenize = haxeTokenBase;
	logger.debug(`<<< Exiting haxeTokenString(quote=${quote})`);
      return ret("string", "string");
    };
  }

  function haxeTokenComment(stream, state) {
	logger.debug(`>>> Entering haxeTokenComment(stream=${stream},state=${state})`);
    var maybeEnd = false, ch;
    while (ch = stream.next()) {
      if (ch == "/" && maybeEnd) {
        state.tokenize = haxeTokenBase;
        break;
      }
      maybeEnd = (ch == "*");
	logger.debug(`haxeTokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
    }
	logger.debug(`haxeTokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
	logger.debug(`<<< Exiting haxeTokenComment(stream=${stream},state=${state})`);
    return ret("comment", "comment");
  }

  // Parser

  var atomicTypes = {"atom": true, "number": true, "variable": true, "string": true, "regexp": true};

  function HaxeLexical(indented, column, type, align, prev, info) {
	logger.debug(`>>> Entering HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info})`);
    this.indented = indented;
	logger.debug(`defineMode(): atomicTypes → ${atomicTypes}`);
	logger.debug(`defineMode(): this.indented → ${this.indented}`);
    this.column = column;
    this.type = type;
	logger.debug(`HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): indented → ${indented}`);
	logger.debug(`HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): this.indented → ${this.indented}`);
	logger.debug(`HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): column → ${column}`);
    this.prev = prev;
    this.info = info;
	logger.debug(`HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): prev → ${prev}`);
    if (align != null) this.align = align;
	logger.debug(`<<< Exiting HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info})`);
  }

  function inScope(state, varname) {
	logger.debug(`>>> Entering inScope(state=${state},varname=${varname})`);
    for (var v = state.localVars; v; v = v.next)
      if (v.name == varname) return true;
	logger.debug(`<<< Exiting inScope(state=${state},varname=${varname})`);
  }

  function parseHaxe(state, style, type, content, stream) {
	logger.debug(`>>> Entering parseHaxe(state=${state},style=${style},type=${type},content=${content},stream=${stream})`);
    var cc = state.cc;
    // Communicate our context to the combinators.
    // (Less wasteful than consing up a hundred closures on every call.)
    cx.state = state; cx.stream = stream; cx.marked = null, cx.cc = cc;

    if (!state.lexical.hasOwnProperty("align"))
      state.lexical.align = true;

    while(true) {
      var combinator = cc.length ? cc.pop() : statement;
	logger.debug(`parseHaxe(state=${state},style=${style},type=${type},content=${content},stream=${stream}): combinator → ${combinator}`);
      if (combinator(type, content)) {
		logger.debug(`defineMode(): this.type → ${this.type}`);
		logger.debug(`defineMode(): this.prev → ${this.prev}`);
        while(cc.length && cc[cc.length - 1].lex)
          cc.pop()();
        if (cx.marked) return cx.marked;
        if (type == "variable" && inScope(state, content)) return "variable-2";
    if (type == "variable" && imported(state, content)) return "variable-3";
		logger.debug(`<<< Exiting parseHaxe(state=${state},style=${style},type=${type},content=${content},stream=${stream})`);
        return style;
      }
    }
  }

  function imported(state, typename)
  {
  if (/[a-z]/.test(typename.charAt(0)))
    return false;
  var len = state.importedtypes.length;
  for (var i = 0; i<len; i++)
    if(state.importedtypes[i]==typename) return true;
  }


  function registerimport(importname) {
  var state = cx.state;
  for (var t = state.importedtypes; t; t = t.next)
    if(t.name == importname) return;
  state.importedtypes = { name: importname, next: state.importedtypes };
logger.debug(`defineMode(): state.importedtypes → ${state.importedtypes}`);
  }
  // Combinator utils

  var cx = {state: null, column: null, marked: null, cc: null};
  function pass() {
	logger.debug('>>> Entering pass()');
    for (var i = arguments.length - 1; i >= 0; i--) cx.cc.push(arguments[i]);
	logger.debug('<<< Exiting pass()');
  }
  function cont() {
	logger.debug('>>> Entering cont()');
    pass.apply(null, arguments);
	logger.debug('<<< Exiting cont()');
    return true;
  }
  function register(varname) {
	logger.debug(`>>> Entering register(varname=${varname})`);
    var state = cx.state;
    if (state.context) {
      cx.marked = "def";
	logger.debug(`defineMode(): cx.marked → ${cx.marked}`);
      for (var v = state.localVars; v; v = v.next)
        if (v.name == varname) return;
      state.localVars = {name: varname, next: state.localVars};
	logger.debug(`<<< Exiting register(varname=${varname})`);
    }
  }

  // Combinators

  var defaultVars = {name: "this", next: null};
  function pushcontext() {
	logger.debug('>>> Entering pushcontext()');
    if (!cx.state.context) cx.state.localVars = defaultVars;
	logger.debug(`defineMode(): defaultVars → ${defaultVars}`);
	logger.debug(`defineMode(): state.localVars → ${state.localVars}`);
    cx.state.context = {prev: cx.state.context, vars: cx.state.localVars};
	logger.debug('<<< Exiting pushcontext()');
  }
  function popcontext() {
	logger.debug('>>> Entering popcontext()');
    cx.state.localVars = cx.state.context.vars;
    cx.state.context = cx.state.context.prev;
	logger.debug('<<< Exiting popcontext()');
  }
  function pushlex(type, info) {
	logger.debug(`>>> Entering pushlex(type=${type},info=${info})`);
    var result = function() {
	logger.debug('>>> Entering result()');
      var state = cx.state;
      state.lexical = new HaxeLexical(state.indented, cx.stream.column(), type, null, state.lexical, info);
	logger.debug('<<< Exiting result()');
    };
    result.lex = true;
	logger.debug(`<<< Exiting pushlex(type=${type},info=${info})`);
    return result;
  }
  function poplex() {
	logger.debug('>>> Entering poplex()');
    var state = cx.state;
	logger.debug(`defineMode(): result.lex → ${result.lex}`);
    if (state.lexical.prev) {
      if (state.lexical.type == ")")
        state.indented = state.lexical.indented;
      state.lexical = state.lexical.prev;
	logger.debug('<<< Exiting poplex()');
    }
  }
  poplex.lex = true;

  function expect(wanted) {
	logger.debug(`>>> Entering expect(wanted=${wanted})`);
    function f(type) {
	logger.debug(`defineMode(): state.indented → ${state.indented}`);
	logger.debug(`defineMode(): state.lexical → ${state.lexical}`);
      if (type == wanted) return cont();
      else if (wanted == ";") return pass();
      else return cont(f);
	logger.debug(`<<< Exiting f(type=${type})`);
    };
	logger.debug(`<<< Exiting expect(wanted=${wanted})`);
    return f;
  }

  function statement(type) {
	logger.debug(`>>> Entering statement(type=${type})`);
    if (type == "@") return cont(metadef);
    if (type == "var") return cont(pushlex("vardef"), vardef1, expect(";"), poplex);
    if (type == "keyword a") return cont(pushlex("form"), expression, statement, poplex);
    if (type == "keyword b") return cont(pushlex("form"), statement, poplex);
    if (type == "{") return cont(pushlex("}"), pushcontext, block, poplex, popcontext);
    if (type == ";") return cont();
    if (type == "attribute") return cont(maybeattribute);
    if (type == "function") return cont(functiondef);
    if (type == "for") return cont(pushlex("form"), expect("("), pushlex(")"), forspec1, expect(")"),
                                      poplex, statement, poplex);
									logger.debug(`defineMode(): poplex.lex → ${poplex.lex}`);
    if (type == "variable") return cont(pushlex("stat"), maybelabel);
    if (type == "switch") return cont(pushlex("form"), expression, pushlex("}", "switch"), expect("{"),
                                         block, poplex, poplex);
    if (type == "case") return cont(expression, expect(":"));
    if (type == "default") return cont(expect(":"));
    if (type == "catch") return cont(pushlex("form"), pushcontext, expect("("), funarg, expect(")"),
                                        statement, poplex, popcontext);
    if (type == "import") return cont(importdef, expect(";"));
    if (type == "typedef") return cont(typedef);
	logger.debug(`<<< Exiting statement(type=${type})`);
    return pass(pushlex("stat"), expression, expect(";"), poplex);
  }
  function expression(type) {
	logger.debug(`>>> Entering expression(type=${type})`);
    if (atomicTypes.hasOwnProperty(type)) return cont(maybeoperator);
    if (type == "function") return cont(functiondef);
    if (type == "keyword c") return cont(maybeexpression);
    if (type == "(") return cont(pushlex(")"), maybeexpression, expect(")"), poplex, maybeoperator);
    if (type == "operator") return cont(expression);
    if (type == "[") return cont(pushlex("]"), commasep(expression, "]"), poplex, maybeoperator);
    if (type == "{") return cont(pushlex("}"), commasep(objprop, "}"), poplex, maybeoperator);
	logger.debug(`<<< Exiting expression(type=${type})`);
    return cont();
  }
  function maybeexpression(type) {
	logger.debug(`>>> Entering maybeexpression(type=${type})`);
    if (type.match(/[;\}\)\],]/)) return pass();
	logger.debug(`<<< Exiting maybeexpression(type=${type})`);
    return pass(expression);
  }

  function maybeoperator(type, value) {
	logger.debug(`>>> Entering maybeoperator(type=${type},value=${value})`);
    if (type == "operator" && /\+\+|--/.test(value)) return cont(maybeoperator);
    if (type == "operator" || type == ":") return cont(expression);
    if (type == ";") return;
    if (type == "(") return cont(pushlex(")"), commasep(expression, ")"), poplex, maybeoperator);
    if (type == ".") return cont(property, maybeoperator);
	logger.debug(`<<< Exiting maybeoperator(type=${type},value=${value})`);
    if (type == "[") return cont(pushlex("]"), expression, expect("]"), poplex, maybeoperator);
  }

  function maybeattribute(type) {
	logger.debug(`>>> Entering maybeattribute(type=${type})`);
    if (type == "attribute") return cont(maybeattribute);
    if (type == "function") return cont(functiondef);
	logger.debug(`<<< Exiting maybeattribute(type=${type})`);
    if (type == "var") return cont(vardef1);
  }

  function metadef(type) {
	logger.debug(`>>> Entering metadef(type=${type})`);
    if(type == ":") return cont(metadef);
    if(type == "variable") return cont(metadef);
	logger.debug(`<<< Exiting metadef(type=${type})`);
    if(type == "(") return cont(pushlex(")"), commasep(metaargs, ")"), poplex, statement);
  }
  function metaargs(type) {
	logger.debug(`>>> Entering metaargs(type=${type})`);
    if(type == "variable") return cont();
	logger.debug(`<<< Exiting metaargs(type=${type})`);
  }

  function importdef (type, value) {
  if(type == "variable" && /[A-Z]/.test(value.charAt(0))) { registerimport(value); return cont(); }
  else if(type == "variable" || type == "property" || type == "." || value == "*") return cont(importdef);
  }

  function typedef (type, value)
  {
  if(type == "variable" && /[A-Z]/.test(value.charAt(0))) { registerimport(value); return cont(); }
  else if (type == "type" && /[A-Z]/.test(value.charAt(0))) { return cont(); }
  }

  function maybelabel(type) {
	logger.debug(`>>> Entering maybelabel(type=${type})`);
    if (type == ":") return cont(poplex, statement);
	logger.debug(`<<< Exiting maybelabel(type=${type})`);
    return pass(maybeoperator, expect(";"), poplex);
  }
  function property(type) {
	logger.debug(`>>> Entering property(type=${type})`);
    if (type == "variable") {cx.marked = "property"; return cont();}
	logger.debug(`<<< Exiting property(type=${type})`);
  }
  function objprop(type) {
	logger.debug(`>>> Entering objprop(type=${type})`);
    if (type == "variable") cx.marked = "property";
    if (atomicTypes.hasOwnProperty(type)) return cont(expect(":"), expression);
	logger.debug(`<<< Exiting objprop(type=${type})`);
  }
  function commasep(what, end) {
	logger.debug(`>>> Entering commasep(what=${what},end=${end})`);
    function proceed(type) {
	logger.debug(`>>> Entering proceed(type=${type})`);
      if (type == ",") return cont(what, proceed);
      if (type == end) return cont();
	logger.debug(`<<< Exiting proceed(type=${type})`);
      return cont(expect(end));
    }
    return function(type) {
      if (type == end) return cont();
      else return pass(what, proceed);
    };
	logger.debug(`<<< Exiting commasep(what=${what},end=${end})`);
  }
  function block(type) {
	logger.debug(`>>> Entering block(type=${type})`);
    if (type == "}") return cont();
	logger.debug(`<<< Exiting block(type=${type})`);
    return pass(statement, block);
  }
  function vardef1(type, value) {
	logger.debug(`>>> Entering vardef1(type=${type},value=${value})`);
    if (type == "variable"){register(value); return cont(typeuse, vardef2);}
	logger.debug(`<<< Exiting vardef1(type=${type},value=${value})`);
    return cont();
  }
  function vardef2(type, value) {
	logger.debug(`>>> Entering vardef2(type=${type},value=${value})`);
    if (value == "=") return cont(expression, vardef2);
    if (type == ",") return cont(vardef1);
	logger.debug(`<<< Exiting vardef2(type=${type},value=${value})`);
  }
  function forspec1(type, value) {
  if (type == "variable") {
    register(value);
  }
  return cont(pushlex(")"), pushcontext, forin, expression, poplex, statement, popcontext);
  }
  function forin(_type, value) {
	logger.debug(`>>> Entering forin(_type=${_type},value=${value})`);
    if (value == "in") return cont();
	logger.debug(`<<< Exiting forin(_type=${_type},value=${value})`);
  }
  function functiondef(type, value) {
	logger.debug(`>>> Entering functiondef(type=${type},value=${value})`);
    if (type == "variable") {register(value); return cont(functiondef);}
    if (value == "new") return cont(functiondef);
	logger.debug(`<<< Exiting functiondef(type=${type},value=${value})`);
    if (type == "(") return cont(pushlex(")"), pushcontext, commasep(funarg, ")"), poplex, typeuse, statement, popcontext);
  }
  function typeuse(type) {
	logger.debug(`>>> Entering typeuse(type=${type})`);
    if(type == ":") return cont(typestring);
	logger.debug(`<<< Exiting typeuse(type=${type})`);
  }
  function typestring(type) {
	logger.debug(`>>> Entering typestring(type=${type})`);
    if(type == "type") return cont();
    if(type == "variable") return cont();
	logger.debug(`<<< Exiting typestring(type=${type})`);
    if(type == "{") return cont(pushlex("}"), commasep(typeprop, "}"), poplex);
  }
  function typeprop(type) {
	logger.debug(`>>> Entering typeprop(type=${type})`);
    if(type == "variable") return cont(typeuse);
	logger.debug(`<<< Exiting typeprop(type=${type})`);
  }
  function funarg(type, value) {
	logger.debug(`>>> Entering funarg(type=${type},value=${value})`);
    if (type == "variable") {register(value); return cont(typeuse);}
	logger.debug(`<<< Exiting funarg(type=${type},value=${value})`);
  }

  // Interface

  return {
    startState: function(basecolumn) {
	logger.debug('<<< Exiting defineMode()');
    var defaulttypes = ["Int", "Float", "String", "Void", "Std", "Bool", "Dynamic", "Array"];
      return {
        tokenize: haxeTokenBase,
        reAllowed: true,
        kwAllowed: true,
        cc: [],
        lexical: new HaxeLexical((basecolumn || 0) - indentUnit, 0, "block", false),
        localVars: parserConfig.localVars,
    importedtypes: defaulttypes,
        context: parserConfig.localVars && {vars: parserConfig.localVars},
        indented: 0
      };
	logger.debug('>>> Entering startState()');
	logger.debug('<<< Exiting startState()');
    },

    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      if (stream.sol()) {
        if (!state.lexical.hasOwnProperty("align"))
          state.lexical.align = false;
        state.indented = stream.indentation();
      }
      if (stream.eatSpace()) return null;
      var style = state.tokenize(stream, state);
      if (type == "comment") return style;
      state.reAllowed = !!(type == "operator" || type == "keyword c" || type.match(/^[\[{}\(,;:]$/));
      state.kwAllowed = type != '.';
	logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
	logger.debug(`token(stream=${stream},state=${state}): state.define → ${state.define}`);
	logger.debug(`token(stream=${stream},state=${state}): state.inString → ${state.inString}`);
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      return parseHaxe(state, style, type, content, stream);
    },

    indent: function(state, textAfter) {
	logger.debug(`>>> Entering indent(state=${state},textAfter=${textAfter})`);
      if (state.tokenize != haxeTokenBase) return 0;
      var firstChar = textAfter && textAfter.charAt(0), lexical = state.lexical;
	logger.debug(`indent(state=${state},textAfter=${textAfter}): firstChar → ${firstChar}`);
      if (lexical.type == "stat" && firstChar == "}") lexical = lexical.prev;
      var type = lexical.type, closing = firstChar == type;
      if (type == "vardef") return lexical.indented + 4;
      else if (type == "form" && firstChar == "{") return lexical.indented;
      else if (type == "stat" || type == "form") return lexical.indented + indentUnit;
      else if (lexical.info == "switch" && !closing)
        return lexical.indented + (/^(?:case|default)\b/.test(textAfter) ? indentUnit : 2 * indentUnit);
      else if (lexical.align) return lexical.column + (closing ? 0 : 1);
      else return lexical.indented + (closing ? 0 : indentUnit);
    },

    electricChars: "{}",
    blockCommentStart: "/*",
    blockCommentEnd: "*/",
    lineComment: "//"
  };
});

CodeMirror.defineMIME("text/x-haxe", "haxe");

CodeMirror.defineMode("hxml", function () {

  return {
    startState: function () {
      return {
        define: false,
        inString: false
      };
    },
    token: function (stream, state) {
      var ch = stream.peek();
      var sol = stream.sol();

      ///* comments */
      if (ch == "#") {
        stream.skipToEnd();
        return "comment";
      }
      if (sol && ch == "-") {
        var style = "variable-2";

        stream.eat(/-/);

        if (stream.peek() == "-") {
          stream.eat(/-/);
          style = "keyword a";
        }

        if (stream.peek() == "D") {
          stream.eat(/[D]/);
          style = "keyword c";
          state.define = true;
        }

        stream.eatWhile(/[A-Z]/i);
		logger.debug(`defineMode(): state.define → ${state.define}`);
        return style;
      }

      var ch = stream.peek();

      if (state.inString == false && ch == "'") {
        state.inString = true;
		logger.debug(`defineMode(): state.inString → ${state.inString}`);
        ch = stream.next();
      }

      if (state.inString == true) {
        if (stream.skipTo("'")) {

        } else {
          stream.skipToEnd();
        }

        if (stream.peek() == "'") {
          stream.next();
          state.inString = false;
        }

        return "string";
      }

      stream.next();
      return null;
    },
    lineComment: "#"
  };
});

CodeMirror.defineMIME("text/x-hxml", "hxml");

});
