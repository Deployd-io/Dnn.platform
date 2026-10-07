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

CodeMirror.defineMode("haxe", function(config, parserConfig) {
  var indentUnit = config.indentUnit;

  // Tokenizer

  var keywords = function(){
    function kw(type) {return {type: type, style: "keyword"};}
    var A = kw("keyword a"), B = kw("keyword b"), C = kw("keyword c");
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
	logger.error(`>>> Entering chain(stream=${stream},state=${state},f=${f})`, { "event.action": "chain", "logrite.stream": stream, "logrite.state": state, "logrite.f": f });
    state.tokenize = f;
	logger.warn(`<<< Exiting chain(stream=${stream},state=${state},f=${f})`, { "event.action": "chain", "logrite.stream": stream, "logrite.state": state, "logrite.f": f });
    return f(stream, state);
  }

  function nextUntilUnescaped(stream, end) {
	logger.error(`>>> Entering nextUntilUnescaped(stream=${stream},end=${end})`, { "event.action": "nextUntilUnescaped", "logrite.stream": stream, "logrite.end": end });
    var escaped = false, next;
    while ((next = stream.next()) != null) {
      if (next == end && !escaped)
        return false;
      escaped = !escaped && next == "\\";
    }
	logger.error(`nextUntilUnescaped(stream=${stream},end=${end}): escaped → ${escaped}`, { "event.action": "nextUntilUnescaped", "logrite.stream": stream, "logrite.end": end });
    return escaped;
  }

  // Used as scratch variables to communicate multiple values without
  // consing up tons of objects.
  var type, content;
  function ret(tp, style, cont) {
	logger.error(`>>> Entering ret(tp=${tp},style=${style},cont=${cont})`, { "event.action": "ret", "logrite.tp": tp, "logrite.style": style, "logrite.cont": cont });
    type = tp; content = cont;
	logger.warn(`<<< Exiting ret(tp=${tp},style=${style},cont=${cont})`, { "event.action": "ret", "logrite.tp": tp, "logrite.style": style, "logrite.cont": cont });
    return style;
  }

  function haxeTokenBase(stream, state) {
	logger.error(`>>> Entering haxeTokenBase(stream=${stream},state=${state})`, { "event.action": "haxeTokenBase", "logrite.stream": stream, "logrite.state": state });
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
	logger.warn(`<<< Exiting haxeTokenBase(stream=${stream},state=${state})`, { "event.action": "haxeTokenBase", "logrite.stream": stream, "logrite.state": state });
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
	logger.error(`>>> Entering haxeTokenString(quote=${quote})`, { "event.action": "haxeTokenString", "logrite.quote": quote });
    return function(stream, state) {
      if (!nextUntilUnescaped(stream, quote))
        state.tokenize = haxeTokenBase;
      return ret("string", "string");
    };
  }

  function haxeTokenComment(stream, state) {
	logger.error(`>>> Entering haxeTokenComment(stream=${stream},state=${state})`, { "event.action": "haxeTokenComment", "logrite.stream": stream, "logrite.state": state });
    var maybeEnd = false, ch;
    while (ch = stream.next()) {
      if (ch == "/" && maybeEnd) {
        state.tokenize = haxeTokenBase;
		logger.debug(`haxeTokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "haxeTokenComment", "logrite.stream": stream, "logrite.state": state });
        break;
      }
      maybeEnd = (ch == "*");
    }
	logger.error(`haxeTokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`, { "event.action": "haxeTokenComment", "logrite.stream": stream, "logrite.state": state });
    return ret("comment", "comment");
  }

  // Parser

  var atomicTypes = {"atom": true, "number": true, "variable": true, "string": true, "regexp": true};

  function HaxeLexical(indented, column, type, align, prev, info) {
	logger.error(`>>> Entering HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info})`, { "event.action": "HaxeLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
    this.indented = indented;
    this.column = column;
	logger.debug(`HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): indented → ${indented}`, { "event.action": "HaxeLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
	logger.debug(`HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): this.indented → ${this.indented}`, { "event.action": "HaxeLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
	logger.debug(`HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): column → ${column}`, { "event.action": "HaxeLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
	logger.debug(`HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): type → ${type}`, { "event.action": "HaxeLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
	logger.debug(`HaxeLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): this.column → ${this.column}`, { "event.action": "HaxeLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
    this.type = type;
    this.prev = prev;
    this.info = info;
    if (align != null) this.align = align;
  }

  function inScope(state, varname) {
	logger.error(`>>> Entering inScope(state=${state},varname=${varname})`, { "event.action": "inScope", "logrite.state": state, "logrite.varname": varname });
    for (var v = state.localVars; v; v = v.next)
      if (v.name == varname) return true;
	logger.warn(`<<< Exiting inScope(state=${state},varname=${varname})`, { "event.action": "inScope", "logrite.state": state, "logrite.varname": varname });
  }

  function parseHaxe(state, style, type, content, stream) {
	logger.error(`>>> Entering parseHaxe(state=${state},style=${style},type=${type},content=${content},stream=${stream})`, { "event.action": "parseHaxe", "logrite.state": state, "logrite.style": style, "logrite.type": type, "logrite.content": content, "logrite.stream": stream });
    var cc = state.cc;
    // Communicate our context to the combinators.
    // (Less wasteful than consing up a hundred closures on every call.)
    cx.state = state; cx.stream = stream; cx.marked = null, cx.cc = cc;

    if (!state.lexical.hasOwnProperty("align"))
      state.lexical.align = true;

    while(true) {
      var combinator = cc.length ? cc.pop() : statement;
	logger.error(`parseHaxe(state=${state},style=${style},type=${type},content=${content},stream=${stream}): combinator → ${combinator}`, { "event.action": "parseHaxe", "logrite.state": state, "logrite.style": style, "logrite.type": type, "logrite.content": content, "logrite.stream": stream });
      if (combinator(type, content)) {
        while(cc.length && cc[cc.length - 1].lex)
          cc.pop()();
        if (cx.marked) return cx.marked;
        if (type == "variable" && inScope(state, content)) return "variable-2";
    if (type == "variable" && imported(state, content)) return "variable-3";
		logger.warn(`<<< Exiting parseHaxe(state=${state},style=${style},type=${type},content=${content},stream=${stream})`, { "event.action": "parseHaxe", "logrite.state": state, "logrite.style": style, "logrite.type": type, "logrite.content": content, "logrite.stream": stream });
        return style;
      }
    }
  }

  function imported(state, typename)
  {
logger.error(`>>> Entering imported(state=${state},typename=${typename})`, { "event.action": "imported", "logrite.state": state, "logrite.typename": typename });
  if (/[a-z]/.test(typename.charAt(0)))
    return false;
  var len = state.importedtypes.length;
  for (var i = 0; i<len; i++)
    if(state.importedtypes[i]==typename) return true;
	logger.warn(`<<< Exiting imported(state=${state},typename=${typename})`, { "event.action": "imported", "logrite.state": state, "logrite.typename": typename });
  }


  function registerimport(importname) {
logger.error(`>>> Entering registerimport(importname=${importname})`, { "event.action": "registerimport", "logrite.importname": importname });
  var state = cx.state;
  for (var t = state.importedtypes; t; t = t.next)
    if(t.name == importname) return;
  state.importedtypes = { name: importname, next: state.importedtypes };
logger.debug(`registerimport(importname=${importname}): state.importedtypes → ${state.importedtypes}`, { "event.action": "registerimport", "logrite.importname": importname });
  }
  // Combinator utils

  var cx = {state: null, column: null, marked: null, cc: null};
  function pass() {
	logger.error('>>> Entering pass()', { "event.action": "pass" });
    for (var i = arguments.length - 1; i >= 0; i--) cx.cc.push(arguments[i]);
	logger.warn('<<< Exiting pass()', { "event.action": "pass" });
  }
  function cont() {
	logger.error('>>> Entering cont()', { "event.action": "cont" });
    pass.apply(null, arguments);
	logger.warn('<<< Exiting cont()', { "event.action": "cont" });
    return true;
  }
  function register(varname) {
	logger.error(`>>> Entering register(varname=${varname})`, { "event.action": "register", "logrite.varname": varname });
    var state = cx.state;
    if (state.context) {
      cx.marked = "def";
      for (var v = state.localVars; v; v = v.next)
        if (v.name == varname) return;
      state.localVars = {name: varname, next: state.localVars};
	logger.debug(`register(varname=${varname}): state.localVars → ${state.localVars}`, { "event.action": "register", "logrite.varname": varname });
    }
  }

  // Combinators

  var defaultVars = {name: "this", next: null};
  function pushcontext() {
	logger.error('>>> Entering pushcontext()', { "event.action": "pushcontext" });
    if (!cx.state.context) cx.state.localVars = defaultVars;
    cx.state.context = {prev: cx.state.context, vars: cx.state.localVars};
	logger.warn('<<< Exiting pushcontext()', { "event.action": "pushcontext" });
  }
  function popcontext() {
	logger.error('>>> Entering popcontext()', { "event.action": "popcontext" });
    cx.state.localVars = cx.state.context.vars;
    cx.state.context = cx.state.context.prev;
	logger.warn('<<< Exiting popcontext()', { "event.action": "popcontext" });
  }
  function pushlex(type, info) {
	logger.error(`>>> Entering pushlex(type=${type},info=${info})`, { "event.action": "pushlex", "logrite.type": type, "logrite.info": info });
    var result = function() {
	logger.error('>>> Entering result()', { "event.action": "result" });
      var state = cx.state;
      state.lexical = new HaxeLexical(state.indented, cx.stream.column(), type, null, state.lexical, info);
	logger.warn('<<< Exiting result()', { "event.action": "result" });
    };
    result.lex = true;
    return result;
  }
  function poplex() {
	logger.error('>>> Entering poplex()', { "event.action": "poplex" });
    var state = cx.state;
    if (state.lexical.prev) {
	logger.debug(`poplex(): state.indented → ${state.indented}`, { "event.action": "poplex" });
      if (state.lexical.type == ")")
        state.indented = state.lexical.indented;
      state.lexical = state.lexical.prev;
	logger.debug(`poplex(): state.lexical → ${state.lexical}`, { "event.action": "poplex" });
    }
  }
  poplex.lex = true;

  function expect(wanted) {
    function f(type) {
	logger.error(`>>> Entering f(type=${type})`, { "event.action": "f", "logrite.type": type });
      if (type == wanted) return cont();
      else if (wanted == ";") return pass();
      else return cont(f);
    };
    return f;
  }

  function statement(type) {
	logger.error(`>>> Entering statement(type=${type})`, { "event.action": "statement", "logrite.type": type });
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
    if (type == "variable") return cont(pushlex("stat"), maybelabel);
    if (type == "switch") return cont(pushlex("form"), expression, pushlex("}", "switch"), expect("{"),
                                         block, poplex, poplex);
    if (type == "case") return cont(expression, expect(":"));
    if (type == "default") return cont(expect(":"));
    if (type == "catch") return cont(pushlex("form"), pushcontext, expect("("), funarg, expect(")"),
                                        statement, poplex, popcontext);
    if (type == "import") return cont(importdef, expect(";"));
    if (type == "typedef") return cont(typedef);
	logger.warn(`<<< Exiting statement(type=${type})`, { "event.action": "statement", "logrite.type": type });
    return pass(pushlex("stat"), expression, expect(";"), poplex);
  }
  function expression(type) {
	logger.error(`>>> Entering expression(type=${type})`, { "event.action": "expression", "logrite.type": type });
    if (atomicTypes.hasOwnProperty(type)) return cont(maybeoperator);
    if (type == "function") return cont(functiondef);
    if (type == "keyword c") return cont(maybeexpression);
    if (type == "(") return cont(pushlex(")"), maybeexpression, expect(")"), poplex, maybeoperator);
    if (type == "operator") return cont(expression);
    if (type == "[") return cont(pushlex("]"), commasep(expression, "]"), poplex, maybeoperator);
    if (type == "{") return cont(pushlex("}"), commasep(objprop, "}"), poplex, maybeoperator);
	logger.warn(`<<< Exiting expression(type=${type})`, { "event.action": "expression", "logrite.type": type });
    return cont();
  }
  function maybeexpression(type) {
	logger.error(`>>> Entering maybeexpression(type=${type})`, { "event.action": "maybeexpression", "logrite.type": type });
    if (type.match(/[;\}\)\],]/)) return pass();
	logger.warn(`<<< Exiting maybeexpression(type=${type})`, { "event.action": "maybeexpression", "logrite.type": type });
    return pass(expression);
  }

  function maybeoperator(type, value) {
	logger.error(`>>> Entering maybeoperator(type=${type},value=${value})`, { "event.action": "maybeoperator", "logrite.type": type, "logrite.value": value });
    if (type == "operator" && /\+\+|--/.test(value)) return cont(maybeoperator);
    if (type == "operator" || type == ":") return cont(expression);
    if (type == ";") return;
    if (type == "(") return cont(pushlex(")"), commasep(expression, ")"), poplex, maybeoperator);
    if (type == ".") return cont(property, maybeoperator);
    if (type == "[") return cont(pushlex("]"), expression, expect("]"), poplex, maybeoperator);
	logger.warn(`<<< Exiting maybeoperator(type=${type},value=${value})`, { "event.action": "maybeoperator", "logrite.type": type, "logrite.value": value });
  }

  function maybeattribute(type) {
	logger.error(`>>> Entering maybeattribute(type=${type})`, { "event.action": "maybeattribute", "logrite.type": type });
    if (type == "attribute") return cont(maybeattribute);
    if (type == "function") return cont(functiondef);
    if (type == "var") return cont(vardef1);
	logger.warn(`<<< Exiting maybeattribute(type=${type})`, { "event.action": "maybeattribute", "logrite.type": type });
  }

  function metadef(type) {
	logger.error(`>>> Entering metadef(type=${type})`, { "event.action": "metadef", "logrite.type": type });
    if(type == ":") return cont(metadef);
    if(type == "variable") return cont(metadef);
    if(type == "(") return cont(pushlex(")"), commasep(metaargs, ")"), poplex, statement);
	logger.warn(`<<< Exiting metadef(type=${type})`, { "event.action": "metadef", "logrite.type": type });
  }
  function metaargs(type) {
	logger.error(`>>> Entering metaargs(type=${type})`, { "event.action": "metaargs", "logrite.type": type });
    if(type == "variable") return cont();
	logger.warn(`<<< Exiting metaargs(type=${type})`, { "event.action": "metaargs", "logrite.type": type });
  }

  function importdef (type, value) {
logger.error(`>>> Entering importdef(type=${type},value=${value})`, { "event.action": "importdef", "logrite.type": type, "logrite.value": value });
  if(type == "variable" && /[A-Z]/.test(value.charAt(0))) { registerimport(value); return cont(); }
  else if(type == "variable" || type == "property" || type == "." || value == "*") return cont(importdef);
logger.warn(`<<< Exiting importdef(type=${type},value=${value})`, { "event.action": "importdef", "logrite.type": type, "logrite.value": value });
  }

  function typedef (type, value)
  {
logger.error(`>>> Entering typedef(type=${type},value=${value})`, { "event.action": "typedef", "logrite.type": type, "logrite.value": value });
  if(type == "variable" && /[A-Z]/.test(value.charAt(0))) { registerimport(value); return cont(); }
  else if (type == "type" && /[A-Z]/.test(value.charAt(0))) { return cont(); }
logger.warn(`<<< Exiting typedef(type=${type},value=${value})`, { "event.action": "typedef", "logrite.type": type, "logrite.value": value });
  }

  function maybelabel(type) {
	logger.error(`>>> Entering maybelabel(type=${type})`, { "event.action": "maybelabel", "logrite.type": type });
    if (type == ":") return cont(poplex, statement);
	logger.warn(`<<< Exiting maybelabel(type=${type})`, { "event.action": "maybelabel", "logrite.type": type });
    return pass(maybeoperator, expect(";"), poplex);
  }
  function property(type) {
	logger.error(`>>> Entering property(type=${type})`, { "event.action": "property", "logrite.type": type });
    if (type == "variable") {cx.marked = "property"; return cont();}
	logger.warn(`<<< Exiting property(type=${type})`, { "event.action": "property", "logrite.type": type });
  }
  function objprop(type) {
	logger.error(`>>> Entering objprop(type=${type})`, { "event.action": "objprop", "logrite.type": type });
    if (type == "variable") cx.marked = "property";
    if (atomicTypes.hasOwnProperty(type)) return cont(expect(":"), expression);
	logger.warn(`<<< Exiting objprop(type=${type})`, { "event.action": "objprop", "logrite.type": type });
  }
  function commasep(what, end) {
    function proceed(type) {
	logger.error(`>>> Entering proceed(type=${type})`, { "event.action": "proceed", "logrite.type": type });
      if (type == ",") return cont(what, proceed);
      if (type == end) return cont();
	logger.warn(`<<< Exiting proceed(type=${type})`, { "event.action": "proceed", "logrite.type": type });
      return cont(expect(end));
    }
    return function(type) {
      if (type == end) return cont();
      else return pass(what, proceed);
    };
  }
  function block(type) {
	logger.error(`>>> Entering block(type=${type})`, { "event.action": "block", "logrite.type": type });
    if (type == "}") return cont();
	logger.warn(`<<< Exiting block(type=${type})`, { "event.action": "block", "logrite.type": type });
    return pass(statement, block);
  }
  function vardef1(type, value) {
	logger.error(`>>> Entering vardef1(type=${type},value=${value})`, { "event.action": "vardef1", "logrite.type": type, "logrite.value": value });
    if (type == "variable"){register(value); return cont(typeuse, vardef2);}
	logger.warn(`<<< Exiting vardef1(type=${type},value=${value})`, { "event.action": "vardef1", "logrite.type": type, "logrite.value": value });
    return cont();
  }
  function vardef2(type, value) {
	logger.error(`>>> Entering vardef2(type=${type},value=${value})`, { "event.action": "vardef2", "logrite.type": type, "logrite.value": value });
    if (value == "=") return cont(expression, vardef2);
    if (type == ",") return cont(vardef1);
	logger.warn(`<<< Exiting vardef2(type=${type},value=${value})`, { "event.action": "vardef2", "logrite.type": type, "logrite.value": value });
  }
  function forspec1(type, value) {
logger.error(`>>> Entering forspec1(type=${type},value=${value})`, { "event.action": "forspec1", "logrite.type": type, "logrite.value": value });
  if (type == "variable") {
    register(value);
  }
logger.warn(`<<< Exiting forspec1(type=${type},value=${value})`, { "event.action": "forspec1", "logrite.type": type, "logrite.value": value });
  return cont(pushlex(")"), pushcontext, forin, expression, poplex, statement, popcontext);
  }
  function forin(_type, value) {
	logger.error(`>>> Entering forin(_type=${_type},value=${value})`, { "event.action": "forin", "logrite._type": _type, "logrite.value": value });
    if (value == "in") return cont();
	logger.warn(`<<< Exiting forin(_type=${_type},value=${value})`, { "event.action": "forin", "logrite._type": _type, "logrite.value": value });
  }
  function functiondef(type, value) {
	logger.error(`>>> Entering functiondef(type=${type},value=${value})`, { "event.action": "functiondef", "logrite.type": type, "logrite.value": value });
    if (type == "variable") {register(value); return cont(functiondef);}
    if (value == "new") return cont(functiondef);
    if (type == "(") return cont(pushlex(")"), pushcontext, commasep(funarg, ")"), poplex, typeuse, statement, popcontext);
	logger.warn(`<<< Exiting functiondef(type=${type},value=${value})`, { "event.action": "functiondef", "logrite.type": type, "logrite.value": value });
  }
  function typeuse(type) {
	logger.error(`>>> Entering typeuse(type=${type})`, { "event.action": "typeuse", "logrite.type": type });
    if(type == ":") return cont(typestring);
	logger.warn(`<<< Exiting typeuse(type=${type})`, { "event.action": "typeuse", "logrite.type": type });
  }
  function typestring(type) {
	logger.error(`>>> Entering typestring(type=${type})`, { "event.action": "typestring", "logrite.type": type });
    if(type == "type") return cont();
    if(type == "variable") return cont();
    if(type == "{") return cont(pushlex("}"), commasep(typeprop, "}"), poplex);
	logger.warn(`<<< Exiting typestring(type=${type})`, { "event.action": "typestring", "logrite.type": type });
  }
  function typeprop(type) {
	logger.error(`>>> Entering typeprop(type=${type})`, { "event.action": "typeprop", "logrite.type": type });
    if(type == "variable") return cont(typeuse);
	logger.warn(`<<< Exiting typeprop(type=${type})`, { "event.action": "typeprop", "logrite.type": type });
  }
  function funarg(type, value) {
	logger.error(`>>> Entering funarg(type=${type},value=${value})`, { "event.action": "funarg", "logrite.type": type, "logrite.value": value });
    if (type == "variable") {register(value); return cont(typeuse);}
	logger.warn(`<<< Exiting funarg(type=${type},value=${value})`, { "event.action": "funarg", "logrite.type": type, "logrite.value": value });
  }

  // Interface

  return {
    startState: function(basecolumn) {
	logger.error(`>>> Entering startState(basecolumn=${basecolumn})`, { "event.action": "startState" });
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
	logger.error(`startState(basecolumn=${basecolumn}): defaulttypes → ${defaulttypes}`, { "event.action": "startState" });
	logger.error('>>> Entering startState()', { "event.action": "startState" });
	logger.warn('<<< Exiting startState()', { "event.action": "startState" });
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.sol()) {
        if (!state.lexical.hasOwnProperty("align"))
          state.lexical.align = false;
        state.indented = stream.indentation();
      }
      if (stream.eatSpace()) return null;
      var style = state.tokenize(stream, state);
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (type == "comment") return style;
      state.reAllowed = !!(type == "operator" || type == "keyword c" || type.match(/^[\[{}\(,;:]$/));
      state.kwAllowed = type != '.';
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`token(stream=${stream},state=${state}): state.define → ${state.define}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`token(stream=${stream},state=${state}): state.inString → ${state.inString}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return parseHaxe(state, style, type, content, stream);
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      if (state.tokenize != haxeTokenBase) return 0;
      var firstChar = textAfter && textAfter.charAt(0), lexical = state.lexical;
	logger.error(`indent(state=${state},textAfter=${textAfter}): firstChar → ${firstChar}`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
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
        return style;
      }

      var ch = stream.peek();

      if (state.inString == false && ch == "'") {
        state.inString = true;
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
