const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// TODO actually recognize syntax of TypeScript constructs

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
"use strict";

CodeMirror.defineMode("javascript", function(config, parserConfig) {
  var indentUnit = config.indentUnit;
  var statementIndent = parserConfig.statementIndent;
  var jsonldMode = parserConfig.jsonld;
  var jsonMode = parserConfig.json || jsonldMode;
  var isTS = parserConfig.typescript;
  var wordRE = parserConfig.wordCharacters || /[\w$\xa1-\uffff]/;

  // Tokenizer

  var keywords = function(){
    function kw(type) {return {type: type, style: "keyword"};}
    var A = kw("keyword a"), B = kw("keyword b"), C = kw("keyword c");
    var operator = kw("operator"), atom = {type: "atom", style: "atom"};

    var jsKeywords = {
      "if": kw("if"), "while": A, "with": A, "else": B, "do": B, "try": B, "finally": B,
      "return": C, "break": C, "continue": C, "new": C, "delete": C, "throw": C, "debugger": C,
      "var": kw("var"), "const": kw("var"), "let": kw("var"),
      "function": kw("function"), "catch": kw("catch"),
      "for": kw("for"), "switch": kw("switch"), "case": kw("case"), "default": kw("default"),
      "in": operator, "typeof": operator, "instanceof": operator,
      "true": atom, "false": atom, "null": atom, "undefined": atom, "NaN": atom, "Infinity": atom,
      "this": kw("this"), "class": kw("class"), "super": kw("atom"),
      "yield": C, "export": kw("export"), "import": kw("import"), "extends": C
    };

    // Extend the 'normal' keywords with the TypeScript language extensions
    if (isTS) {
      var type = {type: "variable", style: "variable-3"};
      var tsKeywords = {
        // object-like things
        "interface": kw("interface"),
        "extends": kw("extends"),
        "constructor": kw("constructor"),

        // scope modifiers
        "public": kw("public"),
        "private": kw("private"),
        "protected": kw("protected"),
        "static": kw("static"),

        // types
        "string": type, "number": type, "bool": type, "any": type
      };

      for (var attr in tsKeywords) {
        jsKeywords[attr] = tsKeywords[attr];
      }
    }

    return jsKeywords;
  }();

  var isOperatorChar = /[+\-*&%=<>!?|~^]/;
  var isJsonldKeyword = /^@(context|id|value|language|type|container|list|set|reverse|index|base|vocab|graph)"/;

  function readRegexp(stream) {
	logger.error(`>>> Entering readRegexp(stream=${stream})`, { "event.action": "readRegexp", "logrite.stream": stream });
    var escaped = false, next, inSet = false;
    while ((next = stream.next()) != null) {
	logger.error(`readRegexp(stream=${stream}): escaped → ${escaped}`, { "event.action": "readRegexp", "logrite.stream": stream });
      if (!escaped) {
        if (next == "/" && !inSet) return;
        if (next == "[") inSet = true;
        else if (inSet && next == "]") inSet = false;
      }
      escaped = !escaped && next == "\\";
	logger.warn(`<<< Exiting readRegexp(stream=${stream})`, { "event.action": "readRegexp", "logrite.stream": stream });
    }
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
  function tokenBase(stream, state) {
	logger.error(`>>> Entering tokenBase(stream=${stream},state=${state})`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
    var ch = stream.next();
    if (ch == '"' || ch == "'") {
	logger.error(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
      state.tokenize = tokenString(ch);
      return state.tokenize(stream, state);
    } else if (ch == "." && stream.match(/^\d+(?:[eE][+\-]?\d+)?/)) {
      return ret("number", "number");
    } else if (ch == "." && stream.match("..")) {
      return ret("spread", "meta");
    } else if (/[\[\]{}\(\),;\:\.]/.test(ch)) {
      return ret(ch);
    } else if (ch == "=" && stream.eat(">")) {
      return ret("=>", "operator");
    } else if (ch == "0" && stream.eat(/x/i)) {
      stream.eatWhile(/[\da-f]/i);
      return ret("number", "number");
    } else if (/\d/.test(ch)) {
      stream.match(/^\d*(?:\.\d*)?(?:[eE][+\-]?\d+)?/);
      return ret("number", "number");
    } else if (ch == "/") {
      if (stream.eat("*")) {
        state.tokenize = tokenComment;
		logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenBase", "logrite.stream": stream, "logrite.state": state });
        return tokenComment(stream, state);
      } else if (stream.eat("/")) {
        stream.skipToEnd();
        return ret("comment", "comment");
      } else if (state.lastType == "operator" || state.lastType == "keyword c" ||
               state.lastType == "sof" || /^[\[{}\(,;:]$/.test(state.lastType)) {
        readRegexp(stream);
        stream.match(/^\b(([gimyu])(?![gimyu]*\2))+\b/);
        return ret("regexp", "string-2");
      } else {
        stream.eatWhile(isOperatorChar);
        return ret("operator", "operator", stream.current());
      }
    } else if (ch == "`") {
      state.tokenize = tokenQuasi;
      return tokenQuasi(stream, state);
    } else if (ch == "#") {
      stream.skipToEnd();
      return ret("error", "error");
    } else if (isOperatorChar.test(ch)) {
      stream.eatWhile(isOperatorChar);
      return ret("operator", "operator", stream.current());
    } else if (wordRE.test(ch)) {
      stream.eatWhile(wordRE);
      var word = stream.current(), known = keywords.propertyIsEnumerable(word) && keywords[word];
      return (known && state.lastType != ".") ? ret(known.type, known.style, word) :
                     ret("variable", "variable", word);
    }
  }

  function tokenString(quote) {
    return function(stream, state) {
      var escaped = false, next;
      if (jsonldMode && stream.peek() == "@" && stream.match(isJsonldKeyword)){
        state.tokenize = tokenBase;
        return ret("jsonld-keyword", "meta");
      }
      while ((next = stream.next()) != null) {
        if (next == quote && !escaped) break;
        escaped = !escaped && next == "\\";
      }
      if (!escaped) state.tokenize = tokenBase;
      return ret("string", "string");
    };
  }

  function tokenComment(stream, state) {
    var maybeEnd = false, ch;
    while (ch = stream.next()) {
      if (ch == "/" && maybeEnd) {
        state.tokenize = tokenBase;
        break;
      }
      maybeEnd = (ch == "*");
    }
    return ret("comment", "comment");
  }

  function tokenQuasi(stream, state) {
    var escaped = false, next;
    while ((next = stream.next()) != null) {
      if (!escaped && (next == "`" || next == "$" && stream.eat("{"))) {
		logger.error(`>>> Entering tokenQuasi(stream=${stream},state=${state})`, { "event.action": "tokenQuasi", "logrite.stream": stream, "logrite.state": state });
        state.tokenize = tokenBase;
        break;
      }
      escaped = !escaped && next == "\\";
	logger.debug(`tokenQuasi(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`, { "event.action": "tokenQuasi", "logrite.stream": stream, "logrite.state": state });
    }
    return ret("quasi", "string-2", stream.current());
  }

  var brackets = "([{}])";
  // This is a crude lookahead trick to try and notice that we're
  // parsing the argument patterns for a fat-arrow function before we
  // actually hit the arrow token. It only works if the arrow is on
  // the same line as the arguments and there's no strange noise
  // (comments) in between. Fallback is to only notice when we hit the
  // arrow, and not declare the arguments as locals for the arrow
  // body.
  function findFatArrow(stream, state) {
	logger.error(`>>> Entering findFatArrow(stream=${stream},state=${state})`, { "event.action": "findFatArrow", "logrite.stream": stream, "logrite.state": state });
    if (state.fatArrowAt) state.fatArrowAt = null;
    var arrow = stream.string.indexOf("=>", stream.start);
	logger.error(`findFatArrow(stream=${stream},state=${state}): arrow → ${arrow}`, { "event.action": "findFatArrow", "logrite.stream": stream, "logrite.state": state });
    if (arrow < 0) return;

    var depth = 0, sawSomething = false;
    for (var pos = arrow - 1; pos >= 0; --pos) {
      var ch = stream.string.charAt(pos);
	logger.error(`findFatArrow(stream=${stream},state=${state}): ch → ${ch}`, { "event.action": "findFatArrow", "logrite.stream": stream, "logrite.state": state });
      var bracket = brackets.indexOf(ch);
	logger.error(`findFatArrow(stream=${stream},state=${state}): bracket → ${bracket}`, { "event.action": "findFatArrow", "logrite.stream": stream, "logrite.state": state });
      if (bracket >= 0 && bracket < 3) {
        if (!depth) { ++pos; break; }
        if (--depth == 0) break;
      } else if (bracket >= 3 && bracket < 6) {
        ++depth;
      } else if (wordRE.test(ch)) {
        sawSomething = true;
      } else if (/["'\/]/.test(ch)) {
        return;
      } else if (sawSomething && !depth) {
        ++pos;
		logger.warn(`<<< Exiting findFatArrow(stream=${stream},state=${state})`, { "event.action": "findFatArrow", "logrite.stream": stream, "logrite.state": state });
        break;
      }
    }
    if (sawSomething && !depth) state.fatArrowAt = pos;
  }

  // Parser

  var atomicTypes = {"atom": true, "number": true, "variable": true, "string": true, "regexp": true, "this": true, "jsonld-keyword": true};

  function JSLexical(indented, column, type, align, prev, info) {
	logger.error(`>>> Entering JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info})`, { "event.action": "JSLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
    this.indented = indented;
    this.column = column;
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): indented → ${indented}`, { "event.action": "JSLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): this.indented → ${this.indented}`, { "event.action": "JSLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): column → ${column}`, { "event.action": "JSLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): type → ${type}`, { "event.action": "JSLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): this.column → ${this.column}`, { "event.action": "JSLexical", "logrite.indented": indented, "logrite.column": column, "logrite.type": type, "logrite.align": align, "logrite.prev": prev, "logrite.info": info });
    this.type = type;
    this.prev = prev;
    this.info = info;
    if (align != null) this.align = align;
  }

  function inScope(state, varname) {
	logger.error(`>>> Entering inScope(state=${state},varname=${varname})`, { "event.action": "inScope", "logrite.state": state, "logrite.varname": varname });
    for (var v = state.localVars; v; v = v.next)
      if (v.name == varname) return true;
    for (var cx = state.context; cx; cx = cx.prev) {
      for (var v = cx.vars; v; v = v.next)
        if (v.name == varname) return true;
		logger.warn(`<<< Exiting inScope(state=${state},varname=${varname})`, { "event.action": "inScope", "logrite.state": state, "logrite.varname": varname });
    }
  }

  function parseJS(state, style, type, content, stream) {
	logger.error(`>>> Entering parseJS(state=${state},style=${style},type=${type},content=${content},stream=${stream})`, { "event.action": "parseJS", "logrite.state": state, "logrite.style": style, "logrite.type": type, "logrite.content": content, "logrite.stream": stream });
    var cc = state.cc;
    // Communicate our context to the combinators.
    // (Less wasteful than consing up a hundred closures on every call.)
    cx.state = state; cx.stream = stream; cx.marked = null, cx.cc = cc; cx.style = style;

    if (!state.lexical.hasOwnProperty("align"))
      state.lexical.align = true;

    while(true) {
      var combinator = cc.length ? cc.pop() : jsonMode ? expression : statement;
	logger.error(`parseJS(state=${state},style=${style},type=${type},content=${content},stream=${stream}): combinator → ${combinator}`, { "event.action": "parseJS", "logrite.state": state, "logrite.style": style, "logrite.type": type, "logrite.content": content, "logrite.stream": stream });
      if (combinator(type, content)) {
        while(cc.length && cc[cc.length - 1].lex)
          cc.pop()();
        if (cx.marked) return cx.marked;
        if (type == "variable" && inScope(state, content)) return "variable-2";
		logger.warn(`<<< Exiting parseJS(state=${state},style=${style},type=${type},content=${content},stream=${stream})`, { "event.action": "parseJS", "logrite.state": state, "logrite.style": style, "logrite.type": type, "logrite.content": content, "logrite.stream": stream });
        return style;
      }
    }
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
    function inList(list) {
	logger.error(`>>> Entering inList(list=${list})`, { "event.action": "inList", "logrite.list": list });
      for (var v = list; v; v = v.next)
        if (v.name == varname) return true;
	logger.warn(`<<< Exiting inList(list=${list})`, { "event.action": "inList", "logrite.list": list });
      return false;
    }
    var state = cx.state;
    if (state.context) {
      cx.marked = "def";
      if (inList(state.localVars)) return;
      state.localVars = {name: varname, next: state.localVars};
    } else {
      if (inList(state.globalVars)) return;
      if (parserConfig.globalVars)
        state.globalVars = {name: varname, next: state.globalVars};
    }
  }

  // Combinators

  var defaultVars = {name: "this", next: {name: "arguments"}};
  function pushcontext() {
	logger.error('>>> Entering pushcontext()', { "event.action": "pushcontext" });
    cx.state.context = {prev: cx.state.context, vars: cx.state.localVars};
    cx.state.localVars = defaultVars;
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
      var state = cx.state, indent = state.indented;
      if (state.lexical.type == "stat") indent = state.lexical.indented;
      else for (var outer = state.lexical; outer && outer.type == ")" && outer.align; outer = outer.prev)
        indent = outer.indented;
      state.lexical = new JSLexical(indent, cx.stream.column(), type, null, state.lexical, info);
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
    function exp(type) {
	logger.error(`>>> Entering exp(type=${type})`, { "event.action": "exp", "logrite.type": type });
      if (type == wanted) return cont();
      else if (wanted == ";") return pass();
      else return cont(exp);
    };
    return exp;
  }

  function statement(type, value) {
	logger.error(`>>> Entering statement(type=${type},value=${value})`, { "event.action": "statement", "logrite.type": type, "logrite.value": value });
    if (type == "var") return cont(pushlex("vardef", value.length), vardef, expect(";"), poplex);
    if (type == "keyword a") return cont(pushlex("form"), expression, statement, poplex);
    if (type == "keyword b") return cont(pushlex("form"), statement, poplex);
    if (type == "{") return cont(pushlex("}"), block, poplex);
    if (type == ";") return cont();
    if (type == "if") {
      if (cx.state.lexical.info == "else" && cx.state.cc[cx.state.cc.length - 1] == poplex)
        cx.state.cc.pop()();
      return cont(pushlex("form"), expression, statement, poplex, maybeelse);
    }
    if (type == "function") return cont(functiondef);
    if (type == "for") return cont(pushlex("form"), forspec, statement, poplex);
    if (type == "variable") return cont(pushlex("stat"), maybelabel);
    if (type == "switch") return cont(pushlex("form"), expression, pushlex("}", "switch"), expect("{"),
                                      block, poplex, poplex);
    if (type == "case") return cont(expression, expect(":"));
    if (type == "default") return cont(expect(":"));
    if (type == "catch") return cont(pushlex("form"), pushcontext, expect("("), funarg, expect(")"),
                                     statement, poplex, popcontext);
    if (type == "class") return cont(pushlex("form"), className, poplex);
    if (type == "export") return cont(pushlex("form"), afterExport, poplex);
    if (type == "import") return cont(pushlex("form"), afterImport, poplex);
	logger.warn(`<<< Exiting statement(type=${type},value=${value})`, { "event.action": "statement", "logrite.type": type, "logrite.value": value });
    return pass(pushlex("stat"), expression, expect(";"), poplex);
  }
  function expression(type) {
	logger.error(`>>> Entering expression(type=${type})`, { "event.action": "expression", "logrite.type": type });
	logger.warn(`<<< Exiting expression(type=${type})`, { "event.action": "expression", "logrite.type": type });
    return expressionInner(type, false);
  }
  function expressionNoComma(type) {
	logger.error(`>>> Entering expressionNoComma(type=${type})`, { "event.action": "expressionNoComma", "logrite.type": type });
	logger.warn(`<<< Exiting expressionNoComma(type=${type})`, { "event.action": "expressionNoComma", "logrite.type": type });
    return expressionInner(type, true);
  }
  function expressionInner(type, noComma) {
	logger.error(`>>> Entering expressionInner(type=${type},noComma=${noComma})`, { "event.action": "expressionInner", "logrite.type": type, "logrite.noComma": noComma });
    if (cx.state.fatArrowAt == cx.stream.start) {
      var body = noComma ? arrowBodyNoComma : arrowBody;
      if (type == "(") return cont(pushcontext, pushlex(")"), commasep(pattern, ")"), poplex, expect("=>"), body, popcontext);
      else if (type == "variable") return pass(pushcontext, pattern, expect("=>"), body, popcontext);
    }

    var maybeop = noComma ? maybeoperatorNoComma : maybeoperatorComma;
    if (atomicTypes.hasOwnProperty(type)) return cont(maybeop);
    if (type == "function") return cont(functiondef, maybeop);
    if (type == "keyword c") return cont(noComma ? maybeexpressionNoComma : maybeexpression);
    if (type == "(") return cont(pushlex(")"), maybeexpression, comprehension, expect(")"), poplex, maybeop);
    if (type == "operator" || type == "spread") return cont(noComma ? expressionNoComma : expression);
    if (type == "[") return cont(pushlex("]"), arrayLiteral, poplex, maybeop);
    if (type == "{") return contCommasep(objprop, "}", null, maybeop);
    if (type == "quasi") { return pass(quasi, maybeop); }
	logger.warn(`<<< Exiting expressionInner(type=${type},noComma=${noComma})`, { "event.action": "expressionInner", "logrite.type": type, "logrite.noComma": noComma });
    return cont();
  }
  function maybeexpression(type) {
	logger.error(`>>> Entering maybeexpression(type=${type})`, { "event.action": "maybeexpression", "logrite.type": type });
    if (type.match(/[;\}\)\],]/)) return pass();
	logger.warn(`<<< Exiting maybeexpression(type=${type})`, { "event.action": "maybeexpression", "logrite.type": type });
    return pass(expression);
  }
  function maybeexpressionNoComma(type) {
	logger.error(`>>> Entering maybeexpressionNoComma(type=${type})`, { "event.action": "maybeexpressionNoComma", "logrite.type": type });
    if (type.match(/[;\}\)\],]/)) return pass();
	logger.warn(`<<< Exiting maybeexpressionNoComma(type=${type})`, { "event.action": "maybeexpressionNoComma", "logrite.type": type });
    return pass(expressionNoComma);
  }

  function maybeoperatorComma(type, value) {
	logger.error(`>>> Entering maybeoperatorComma(type=${type},value=${value})`, { "event.action": "maybeoperatorComma", "logrite.type": type, "logrite.value": value });
    if (type == ",") return cont(expression);
	logger.warn(`<<< Exiting maybeoperatorComma(type=${type},value=${value})`, { "event.action": "maybeoperatorComma", "logrite.type": type, "logrite.value": value });
    return maybeoperatorNoComma(type, value, false);
  }
  function maybeoperatorNoComma(type, value, noComma) {
	logger.error(`>>> Entering maybeoperatorNoComma(type=${type},value=${value},noComma=${noComma})`, { "event.action": "maybeoperatorNoComma", "logrite.type": type, "logrite.value": value, "logrite.noComma": noComma });
    var me = noComma == false ? maybeoperatorComma : maybeoperatorNoComma;
    var expr = noComma == false ? expression : expressionNoComma;
    if (type == "=>") return cont(pushcontext, noComma ? arrowBodyNoComma : arrowBody, popcontext);
    if (type == "operator") {
      if (/\+\+|--/.test(value)) return cont(me);
      if (value == "?") return cont(expression, expect(":"), expr);
      return cont(expr);
    }
    if (type == "quasi") { return pass(quasi, me); }
    if (type == ";") return;
    if (type == "(") return contCommasep(expressionNoComma, ")", "call", me);
    if (type == ".") return cont(property, me);
    if (type == "[") return cont(pushlex("]"), maybeexpression, expect("]"), poplex, me);
	logger.warn(`<<< Exiting maybeoperatorNoComma(type=${type},value=${value},noComma=${noComma})`, { "event.action": "maybeoperatorNoComma", "logrite.type": type, "logrite.value": value, "logrite.noComma": noComma });
  }
  function quasi(type, value) {
	logger.error(`>>> Entering quasi(type=${type},value=${value})`, { "event.action": "quasi", "logrite.type": type, "logrite.value": value });
    if (type != "quasi") return pass();
    if (value.slice(value.length - 2) != "${") return cont(quasi);
	logger.warn(`<<< Exiting quasi(type=${type},value=${value})`, { "event.action": "quasi", "logrite.type": type, "logrite.value": value });
    return cont(expression, continueQuasi);
  }
  function continueQuasi(type) {
	logger.error(`>>> Entering continueQuasi(type=${type})`, { "event.action": "continueQuasi", "logrite.type": type });
    if (type == "}") {
      cx.marked = "string-2";
      cx.state.tokenize = tokenQuasi;
	logger.warn(`<<< Exiting continueQuasi(type=${type})`, { "event.action": "continueQuasi", "logrite.type": type });
      return cont(quasi);
    }
  }
  function arrowBody(type) {
	logger.error(`>>> Entering arrowBody(type=${type})`, { "event.action": "arrowBody", "logrite.type": type });
    findFatArrow(cx.stream, cx.state);
	logger.warn(`<<< Exiting arrowBody(type=${type})`, { "event.action": "arrowBody", "logrite.type": type });
    return pass(type == "{" ? statement : expression);
  }
  function arrowBodyNoComma(type) {
	logger.error(`>>> Entering arrowBodyNoComma(type=${type})`, { "event.action": "arrowBodyNoComma", "logrite.type": type });
    findFatArrow(cx.stream, cx.state);
	logger.warn(`<<< Exiting arrowBodyNoComma(type=${type})`, { "event.action": "arrowBodyNoComma", "logrite.type": type });
    return pass(type == "{" ? statement : expressionNoComma);
  }
  function maybelabel(type) {
	logger.error(`>>> Entering maybelabel(type=${type})`, { "event.action": "maybelabel", "logrite.type": type });
    if (type == ":") return cont(poplex, statement);
	logger.warn(`<<< Exiting maybelabel(type=${type})`, { "event.action": "maybelabel", "logrite.type": type });
    return pass(maybeoperatorComma, expect(";"), poplex);
  }
  function property(type) {
	logger.error(`>>> Entering property(type=${type})`, { "event.action": "property", "logrite.type": type });
    if (type == "variable") {cx.marked = "property"; return cont();}
	logger.warn(`<<< Exiting property(type=${type})`, { "event.action": "property", "logrite.type": type });
  }
  function objprop(type, value) {
	logger.error(`>>> Entering objprop(type=${type},value=${value})`, { "event.action": "objprop", "logrite.type": type, "logrite.value": value });
    if (type == "variable" || cx.style == "keyword") {
      cx.marked = "property";
      if (value == "get" || value == "set") return cont(getterSetter);
      return cont(afterprop);
    } else if (type == "number" || type == "string") {
      cx.marked = jsonldMode ? "property" : (cx.style + " property");
      return cont(afterprop);
    } else if (type == "jsonld-keyword") {
      return cont(afterprop);
    } else if (type == "[") {
	logger.warn(`<<< Exiting objprop(type=${type},value=${value})`, { "event.action": "objprop", "logrite.type": type, "logrite.value": value });
      return cont(expression, expect("]"), afterprop);
    }
  }
  function getterSetter(type) {
	logger.error(`>>> Entering getterSetter(type=${type})`, { "event.action": "getterSetter", "logrite.type": type });
    if (type != "variable") return pass(afterprop);
    cx.marked = "property";
	logger.warn(`<<< Exiting getterSetter(type=${type})`, { "event.action": "getterSetter", "logrite.type": type });
    return cont(functiondef);
  }
  function afterprop(type) {
	logger.error(`>>> Entering afterprop(type=${type})`, { "event.action": "afterprop", "logrite.type": type });
    if (type == ":") return cont(expressionNoComma);
    if (type == "(") return pass(functiondef);
	logger.warn(`<<< Exiting afterprop(type=${type})`, { "event.action": "afterprop", "logrite.type": type });
  }
  function commasep(what, end) {
    function proceed(type) {
	logger.error(`>>> Entering proceed(type=${type})`, { "event.action": "proceed", "logrite.type": type });
      if (type == ",") {
        var lex = cx.state.lexical;
        if (lex.info == "call") lex.pos = (lex.pos || 0) + 1;
        return cont(what, proceed);
      }
      if (type == end) return cont();
	logger.warn(`<<< Exiting proceed(type=${type})`, { "event.action": "proceed", "logrite.type": type });
      return cont(expect(end));
    }
    return function(type) {
      if (type == end) return cont();
      return pass(what, proceed);
    };
  }
  function contCommasep(what, end, info) {
	logger.error(`>>> Entering contCommasep(what=${what},end=${end},info=${info})`, { "event.action": "contCommasep", "logrite.what": what, "logrite.end": end, "logrite.info": info });
    for (var i = 3; i < arguments.length; i++)
      cx.cc.push(arguments[i]);
	logger.warn(`<<< Exiting contCommasep(what=${what},end=${end},info=${info})`, { "event.action": "contCommasep", "logrite.what": what, "logrite.end": end, "logrite.info": info });
    return cont(pushlex(end, info), commasep(what, end), poplex);
  }
  function block(type) {
	logger.error(`>>> Entering block(type=${type})`, { "event.action": "block", "logrite.type": type });
    if (type == "}") return cont();
	logger.warn(`<<< Exiting block(type=${type})`, { "event.action": "block", "logrite.type": type });
    return pass(statement, block);
  }
  function maybetype(type) {
	logger.error(`>>> Entering maybetype(type=${type})`, { "event.action": "maybetype", "logrite.type": type });
    if (isTS && type == ":") return cont(typedef);
	logger.warn(`<<< Exiting maybetype(type=${type})`, { "event.action": "maybetype", "logrite.type": type });
  }
  function maybedefault(_, value) {
	logger.error(`>>> Entering maybedefault(_=${_},value=${value})`, { "event.action": "maybedefault", "logrite._": _, "logrite.value": value });
    if (value == "=") return cont(expressionNoComma);
	logger.warn(`<<< Exiting maybedefault(_=${_},value=${value})`, { "event.action": "maybedefault", "logrite._": _, "logrite.value": value });
  }
  function typedef(type) {
	logger.error(`>>> Entering typedef(type=${type})`, { "event.action": "typedef", "logrite.type": type });
    if (type == "variable") {cx.marked = "variable-3"; return cont();}
	logger.warn(`<<< Exiting typedef(type=${type})`, { "event.action": "typedef", "logrite.type": type });
  }
  function vardef() {
	logger.error('>>> Entering vardef()', { "event.action": "vardef" });
	logger.warn('<<< Exiting vardef()', { "event.action": "vardef" });
    return pass(pattern, maybetype, maybeAssign, vardefCont);
  }
  function pattern(type, value) {
	logger.error(`>>> Entering pattern(type=${type},value=${value})`, { "event.action": "pattern", "logrite.type": type, "logrite.value": value });
    if (type == "variable") { register(value); return cont(); }
    if (type == "[") return contCommasep(pattern, "]");
    if (type == "{") return contCommasep(proppattern, "}");
	logger.warn(`<<< Exiting pattern(type=${type},value=${value})`, { "event.action": "pattern", "logrite.type": type, "logrite.value": value });
  }
  function proppattern(type, value) {
	logger.error(`>>> Entering proppattern(type=${type},value=${value})`, { "event.action": "proppattern", "logrite.type": type, "logrite.value": value });
    if (type == "variable" && !cx.stream.match(/^\s*:/, false)) {
      register(value);
      return cont(maybeAssign);
    }
    if (type == "variable") cx.marked = "property";
	logger.warn(`<<< Exiting proppattern(type=${type},value=${value})`, { "event.action": "proppattern", "logrite.type": type, "logrite.value": value });
    return cont(expect(":"), pattern, maybeAssign);
  }
  function maybeAssign(_type, value) {
	logger.error(`>>> Entering maybeAssign(_type=${_type},value=${value})`, { "event.action": "maybeAssign", "logrite._type": _type, "logrite.value": value });
    if (value == "=") return cont(expressionNoComma);
	logger.warn(`<<< Exiting maybeAssign(_type=${_type},value=${value})`, { "event.action": "maybeAssign", "logrite._type": _type, "logrite.value": value });
  }
  function vardefCont(type) {
	logger.error(`>>> Entering vardefCont(type=${type})`, { "event.action": "vardefCont", "logrite.type": type });
    if (type == ",") return cont(vardef);
	logger.warn(`<<< Exiting vardefCont(type=${type})`, { "event.action": "vardefCont", "logrite.type": type });
  }
  function maybeelse(type, value) {
	logger.error(`>>> Entering maybeelse(type=${type},value=${value})`, { "event.action": "maybeelse", "logrite.type": type, "logrite.value": value });
    if (type == "keyword b" && value == "else") return cont(pushlex("form", "else"), statement, poplex);
	logger.warn(`<<< Exiting maybeelse(type=${type},value=${value})`, { "event.action": "maybeelse", "logrite.type": type, "logrite.value": value });
  }
  function forspec(type) {
	logger.error(`>>> Entering forspec(type=${type})`, { "event.action": "forspec", "logrite.type": type });
    if (type == "(") return cont(pushlex(")"), forspec1, expect(")"), poplex);
	logger.warn(`<<< Exiting forspec(type=${type})`, { "event.action": "forspec", "logrite.type": type });
  }
  function forspec1(type) {
	logger.error(`>>> Entering forspec1(type=${type})`, { "event.action": "forspec1", "logrite.type": type });
    if (type == "var") return cont(vardef, expect(";"), forspec2);
    if (type == ";") return cont(forspec2);
    if (type == "variable") return cont(formaybeinof);
	logger.warn(`<<< Exiting forspec1(type=${type})`, { "event.action": "forspec1", "logrite.type": type });
    return pass(expression, expect(";"), forspec2);
  }
  function formaybeinof(_type, value) {
	logger.error(`>>> Entering formaybeinof(_type=${_type},value=${value})`, { "event.action": "formaybeinof", "logrite._type": _type, "logrite.value": value });
    if (value == "in" || value == "of") { cx.marked = "keyword"; return cont(expression); }
	logger.warn(`<<< Exiting formaybeinof(_type=${_type},value=${value})`, { "event.action": "formaybeinof", "logrite._type": _type, "logrite.value": value });
    return cont(maybeoperatorComma, forspec2);
  }
  function forspec2(type, value) {
	logger.error(`>>> Entering forspec2(type=${type},value=${value})`, { "event.action": "forspec2", "logrite.type": type, "logrite.value": value });
    if (type == ";") return cont(forspec3);
    if (value == "in" || value == "of") { cx.marked = "keyword"; return cont(expression); }
	logger.warn(`<<< Exiting forspec2(type=${type},value=${value})`, { "event.action": "forspec2", "logrite.type": type, "logrite.value": value });
    return pass(expression, expect(";"), forspec3);
  }
  function forspec3(type) {
	logger.error(`>>> Entering forspec3(type=${type})`, { "event.action": "forspec3", "logrite.type": type });
    if (type != ")") cont(expression);
	logger.warn(`<<< Exiting forspec3(type=${type})`, { "event.action": "forspec3", "logrite.type": type });
  }
  function functiondef(type, value) {
	logger.error(`>>> Entering functiondef(type=${type},value=${value})`, { "event.action": "functiondef", "logrite.type": type, "logrite.value": value });
    if (value == "*") {cx.marked = "keyword"; return cont(functiondef);}
    if (type == "variable") {register(value); return cont(functiondef);}
    if (type == "(") return cont(pushcontext, pushlex(")"), commasep(funarg, ")"), poplex, statement, popcontext);
	logger.warn(`<<< Exiting functiondef(type=${type},value=${value})`, { "event.action": "functiondef", "logrite.type": type, "logrite.value": value });
  }
  function funarg(type) {
	logger.error(`>>> Entering funarg(type=${type})`, { "event.action": "funarg", "logrite.type": type });
    if (type == "spread") return cont(funarg);
	logger.warn(`<<< Exiting funarg(type=${type})`, { "event.action": "funarg", "logrite.type": type });
    return pass(pattern, maybetype, maybedefault);
  }
  function className(type, value) {
	logger.error(`>>> Entering className(type=${type},value=${value})`, { "event.action": "className", "logrite.type": type, "logrite.value": value });
    if (type == "variable") {register(value); return cont(classNameAfter);}
	logger.warn(`<<< Exiting className(type=${type},value=${value})`, { "event.action": "className", "logrite.type": type, "logrite.value": value });
  }
  function classNameAfter(type, value) {
	logger.error(`>>> Entering classNameAfter(type=${type},value=${value})`, { "event.action": "classNameAfter", "logrite.type": type, "logrite.value": value });
    if (value == "extends") return cont(expression, classNameAfter);
    if (type == "{") return cont(pushlex("}"), classBody, poplex);
	logger.warn(`<<< Exiting classNameAfter(type=${type},value=${value})`, { "event.action": "classNameAfter", "logrite.type": type, "logrite.value": value });
  }
  function classBody(type, value) {
	logger.error(`>>> Entering classBody(type=${type},value=${value})`, { "event.action": "classBody", "logrite.type": type, "logrite.value": value });
    if (type == "variable" || cx.style == "keyword") {
      if (value == "static") {
        cx.marked = "keyword";
        return cont(classBody);
      }
      cx.marked = "property";
      if (value == "get" || value == "set") return cont(classGetterSetter, functiondef, classBody);
      return cont(functiondef, classBody);
    }
    if (value == "*") {
      cx.marked = "keyword";
      return cont(classBody);
    }
    if (type == ";") return cont(classBody);
    if (type == "}") return cont();
	logger.warn(`<<< Exiting classBody(type=${type},value=${value})`, { "event.action": "classBody", "logrite.type": type, "logrite.value": value });
  }
  function classGetterSetter(type) {
	logger.error(`>>> Entering classGetterSetter(type=${type})`, { "event.action": "classGetterSetter", "logrite.type": type });
    if (type != "variable") return pass();
    cx.marked = "property";
	logger.warn(`<<< Exiting classGetterSetter(type=${type})`, { "event.action": "classGetterSetter", "logrite.type": type });
    return cont();
  }
  function afterExport(_type, value) {
	logger.error(`>>> Entering afterExport(_type=${_type},value=${value})`, { "event.action": "afterExport", "logrite._type": _type, "logrite.value": value });
    if (value == "*") { cx.marked = "keyword"; return cont(maybeFrom, expect(";")); }
    if (value == "default") { cx.marked = "keyword"; return cont(expression, expect(";")); }
	logger.warn(`<<< Exiting afterExport(_type=${_type},value=${value})`, { "event.action": "afterExport", "logrite._type": _type, "logrite.value": value });
    return pass(statement);
  }
  function afterImport(type) {
	logger.error(`>>> Entering afterImport(type=${type})`, { "event.action": "afterImport", "logrite.type": type });
    if (type == "string") return cont();
	logger.warn(`<<< Exiting afterImport(type=${type})`, { "event.action": "afterImport", "logrite.type": type });
    return pass(importSpec, maybeFrom);
  }
  function importSpec(type, value) {
	logger.error(`>>> Entering importSpec(type=${type},value=${value})`, { "event.action": "importSpec", "logrite.type": type, "logrite.value": value });
    if (type == "{") return contCommasep(importSpec, "}");
    if (type == "variable") register(value);
    if (value == "*") cx.marked = "keyword";
	logger.warn(`<<< Exiting importSpec(type=${type},value=${value})`, { "event.action": "importSpec", "logrite.type": type, "logrite.value": value });
    return cont(maybeAs);
  }
  function maybeAs(_type, value) {
	logger.error(`>>> Entering maybeAs(_type=${_type},value=${value})`, { "event.action": "maybeAs", "logrite._type": _type, "logrite.value": value });
    if (value == "as") { cx.marked = "keyword"; return cont(importSpec); }
	logger.warn(`<<< Exiting maybeAs(_type=${_type},value=${value})`, { "event.action": "maybeAs", "logrite._type": _type, "logrite.value": value });
  }
  function maybeFrom(_type, value) {
	logger.error(`>>> Entering maybeFrom(_type=${_type},value=${value})`, { "event.action": "maybeFrom", "logrite._type": _type, "logrite.value": value });
    if (value == "from") { cx.marked = "keyword"; return cont(expression); }
	logger.warn(`<<< Exiting maybeFrom(_type=${_type},value=${value})`, { "event.action": "maybeFrom", "logrite._type": _type, "logrite.value": value });
  }
  function arrayLiteral(type) {
	logger.error(`>>> Entering arrayLiteral(type=${type})`, { "event.action": "arrayLiteral", "logrite.type": type });
    if (type == "]") return cont();
	logger.warn(`<<< Exiting arrayLiteral(type=${type})`, { "event.action": "arrayLiteral", "logrite.type": type });
    return pass(expressionNoComma, maybeArrayComprehension);
  }
  function maybeArrayComprehension(type) {
	logger.error(`>>> Entering maybeArrayComprehension(type=${type})`, { "event.action": "maybeArrayComprehension", "logrite.type": type });
    if (type == "for") return pass(comprehension, expect("]"));
    if (type == ",") return cont(commasep(maybeexpressionNoComma, "]"));
	logger.warn(`<<< Exiting maybeArrayComprehension(type=${type})`, { "event.action": "maybeArrayComprehension", "logrite.type": type });
    return pass(commasep(expressionNoComma, "]"));
  }
  function comprehension(type) {
	logger.error(`>>> Entering comprehension(type=${type})`, { "event.action": "comprehension", "logrite.type": type });
    if (type == "for") return cont(forspec, comprehension);
    if (type == "if") return cont(expression, comprehension);
	logger.warn(`<<< Exiting comprehension(type=${type})`, { "event.action": "comprehension", "logrite.type": type });
  }

  function isContinuedStatement(state, textAfter) {
	logger.error(`>>> Entering isContinuedStatement(state=${state},textAfter=${textAfter})`, { "event.action": "isContinuedStatement", "logrite.state": state, "logrite.textAfter": textAfter });
	logger.warn(`<<< Exiting isContinuedStatement(state=${state},textAfter=${textAfter})`, { "event.action": "isContinuedStatement", "logrite.state": state, "logrite.textAfter": textAfter });
    return state.lastType == "operator" || state.lastType == "," ||
      isOperatorChar.test(textAfter.charAt(0)) ||
      /[,.]/.test(textAfter.charAt(0));
  }

  // Interface

  return {
    startState: function(basecolumn) {
	logger.error(`>>> Entering startState(basecolumn=${basecolumn})`, { "event.action": "startState", "logrite.basecolumn": basecolumn });
      var state = {
        tokenize: tokenBase,
        lastType: "sof",
        cc: [],
        lexical: new JSLexical((basecolumn || 0) - indentUnit, 0, "block", false),
        localVars: parserConfig.localVars,
        context: parserConfig.localVars && {vars: parserConfig.localVars},
        indented: 0
      };
      if (parserConfig.globalVars && typeof parserConfig.globalVars == "object")
        state.globalVars = parserConfig.globalVars;
      return state;
    },

    token: function(stream, state) {
	logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (stream.sol()) {
        if (!state.lexical.hasOwnProperty("align"))
          state.lexical.align = false;
        state.indented = stream.indentation();
        findFatArrow(stream, state);
      }
      if (state.tokenize != tokenComment && stream.eatSpace()) return null;
      var style = state.tokenize(stream, state);
	logger.error(`token(stream=${stream},state=${state}): style → ${style}`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      if (type == "comment") return style;
      state.lastType = type == "operator" && (content == "++" || content == "--") ? "incdec" : type;
	logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
      return parseJS(state, style, type, content, stream);
    },

    indent: function(state, textAfter) {
	logger.error(`>>> Entering indent(state=${state},textAfter=${textAfter})`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      if (state.tokenize == tokenComment) return CodeMirror.Pass;
      if (state.tokenize != tokenBase) return 0;
      var firstChar = textAfter && textAfter.charAt(0), lexical = state.lexical;
	logger.error(`indent(state=${state},textAfter=${textAfter}): firstChar → ${firstChar}`, { "event.action": "indent", "logrite.state": state, "logrite.textAfter": textAfter });
      // Kludge to prevent 'maybelse' from blocking lexical scope pops
      if (!/^\s*else\b/.test(textAfter)) for (var i = state.cc.length - 1; i >= 0; --i) {
        var c = state.cc[i];
        if (c == poplex) lexical = lexical.prev;
        else if (c != maybeelse) break;
      }
      if (lexical.type == "stat" && firstChar == "}") lexical = lexical.prev;
      if (statementIndent && lexical.type == ")" && lexical.prev.type == "stat")
        lexical = lexical.prev;
      var type = lexical.type, closing = firstChar == type;

      if (type == "vardef") return lexical.indented + (state.lastType == "operator" || state.lastType == "," ? lexical.info + 1 : 0);
      else if (type == "form" && firstChar == "{") return lexical.indented;
      else if (type == "form") return lexical.indented + indentUnit;
      else if (type == "stat")
        return lexical.indented + (isContinuedStatement(state, textAfter) ? statementIndent || indentUnit : 0);
      else if (lexical.info == "switch" && !closing && parserConfig.doubleIndentSwitch != false)
        return lexical.indented + (/^(?:case|default)\b/.test(textAfter) ? indentUnit : 2 * indentUnit);
      else if (lexical.align) return lexical.column + (closing ? 0 : 1);
      else return lexical.indented + (closing ? 0 : indentUnit);
    },

    electricInput: /^\s*(?:case .*?:|default:|\{|\})$/,
    blockCommentStart: jsonMode ? null : "/*",
    blockCommentEnd: jsonMode ? null : "*/",
    lineComment: jsonMode ? null : "//",
    fold: "brace",
    closeBrackets: "()[]{}''\"\"``",

    helperType: jsonMode ? "json" : "javascript",
    jsonldMode: jsonldMode,
    jsonMode: jsonMode
  };
});

CodeMirror.registerHelper("wordChars", "javascript", /[\w$]/);

CodeMirror.defineMIME("text/javascript", "javascript");
CodeMirror.defineMIME("text/ecmascript", "javascript");
CodeMirror.defineMIME("application/javascript", "javascript");
CodeMirror.defineMIME("application/x-javascript", "javascript");
CodeMirror.defineMIME("application/ecmascript", "javascript");
CodeMirror.defineMIME("application/json", {name: "javascript", json: true});
CodeMirror.defineMIME("application/x-json", {name: "javascript", json: true});
CodeMirror.defineMIME("application/ld+json", {name: "javascript", jsonld: true});
CodeMirror.defineMIME("text/typescript", { name: "javascript", typescript: true });
CodeMirror.defineMIME("application/typescript", { name: "javascript", typescript: true });

});
