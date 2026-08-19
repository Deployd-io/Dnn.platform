const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// TODO actually recognize syntax of TypeScript constructs

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
CodeMirror.defineMode("javascript", function(config, parserConfig) {
logger.debug('>>> Entering defineMode()');
  var indentUnit = config.indentUnit;
  var statementIndent = parserConfig.statementIndent;
  var jsonldMode = parserConfig.jsonld;
  var jsonMode = parserConfig.json || jsonldMode;
  var isTS = parserConfig.typescript;
  var wordRE = parserConfig.wordCharacters || /[\w$\xa1-\uffff]/;

  // Tokenizer

logger.debug('<<< Exiting defineMode()');
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
	logger.debug(`>>> Entering readRegexp(stream=${stream})`);
    var escaped = false, next, inSet = false;
    while ((next = stream.next()) != null) {
	logger.debug(`readRegexp(stream=${stream}): escaped → ${escaped}`);
      if (!escaped) {
        if (next == "/" && !inSet) return;
        if (next == "[") inSet = true;
        else if (inSet && next == "]") inSet = false;
      }
      escaped = !escaped && next == "\\";
	logger.debug(`<<< Exiting readRegexp(stream=${stream})`);
    }
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
  function tokenBase(stream, state) {
	logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
    var ch = stream.next();
    if (ch == '"' || ch == "'") {
	logger.debug(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`);
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
		logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
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
        state.tokenize = tokenBase;
        break;
      }
      escaped = !escaped && next == "\\";
    }
	logger.debug(`tokenQuasi(stream=${stream},state=${state}): escaped → ${escaped}`);
	logger.debug(`<<< Exiting tokenQuasi(stream=${stream},state=${state})`);
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
	logger.debug(`>>> Entering findFatArrow(stream=${stream},state=${state})`);
    if (state.fatArrowAt) state.fatArrowAt = null;
    var arrow = stream.string.indexOf("=>", stream.start);
	logger.debug(`findFatArrow(stream=${stream},state=${state}): arrow → ${arrow}`);
    if (arrow < 0) return;

    var depth = 0, sawSomething = false;
    for (var pos = arrow - 1; pos >= 0; --pos) {
      var ch = stream.string.charAt(pos);
	logger.debug(`findFatArrow(stream=${stream},state=${state}): ch → ${ch}`);
      var bracket = brackets.indexOf(ch);
	logger.debug(`findFatArrow(stream=${stream},state=${state}): bracket → ${bracket}`);
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
		logger.debug(`<<< Exiting findFatArrow(stream=${stream},state=${state})`);
        break;
      }
    }
    if (sawSomething && !depth) state.fatArrowAt = pos;
  }

  // Parser

  var atomicTypes = {"atom": true, "number": true, "variable": true, "string": true, "regexp": true, "this": true, "jsonld-keyword": true};

  function JSLexical(indented, column, type, align, prev, info) {
	logger.debug(`>>> Entering JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info})`);
    this.indented = indented;
    this.column = column;
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): indented → ${indented}`);
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): this.indented → ${this.indented}`);
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): column → ${column}`);
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): this.column → ${this.column}`);
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): info → ${info}`);
    this.type = type;
    this.prev = prev;
    this.info = info;
	logger.debug(`JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info}): type → ${type}`);
    if (align != null) this.align = align;
	logger.debug(`<<< Exiting JSLexical(indented=${indented},column=${column},type=${type},align=${align},prev=${prev},info=${info})`);
  }

  function inScope(state, varname) {
	logger.debug(`>>> Entering inScope(state=${state},varname=${varname})`);
    for (var v = state.localVars; v; v = v.next)
      if (v.name == varname) return true;
    for (var cx = state.context; cx; cx = cx.prev) {
      for (var v = cx.vars; v; v = v.next)
        if (v.name == varname) return true;
		logger.debug(`<<< Exiting inScope(state=${state},varname=${varname})`);
    }
  }

  function parseJS(state, style, type, content, stream) {
	logger.debug(`>>> Entering parseJS(state=${state},style=${style},type=${type},content=${content},stream=${stream})`);
    var cc = state.cc;
    // Communicate our context to the combinators.
    // (Less wasteful than consing up a hundred closures on every call.)
    cx.state = state; cx.stream = stream; cx.marked = null, cx.cc = cc; cx.style = style;

    if (!state.lexical.hasOwnProperty("align"))
      state.lexical.align = true;

    while(true) {
      var combinator = cc.length ? cc.pop() : jsonMode ? expression : statement;
	logger.debug(`parseJS(state=${state},style=${style},type=${type},content=${content},stream=${stream}): combinator → ${combinator}`);
      if (combinator(type, content)) {
        while(cc.length && cc[cc.length - 1].lex)
          cc.pop()();
        if (cx.marked) return cx.marked;
        if (type == "variable" && inScope(state, content)) return "variable-2";
		logger.debug(`<<< Exiting parseJS(state=${state},style=${style},type=${type},content=${content},stream=${stream})`);
        return style;
      }
    }
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
    function inList(list) {
	logger.debug(`>>> Entering inList(list=${list})`);
      for (var v = list; v; v = v.next)
        if (v.name == varname) return true;
	logger.debug(`<<< Exiting inList(list=${list})`);
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
	logger.debug('>>> Entering pushcontext()');
    cx.state.context = {prev: cx.state.context, vars: cx.state.localVars};
    cx.state.localVars = defaultVars;
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
      var state = cx.state, indent = state.indented;
      if (state.lexical.type == "stat") indent = state.lexical.indented;
      else for (var outer = state.lexical; outer && outer.type == ")" && outer.align; outer = outer.prev)
		logger.debug(`result(): outer → ${outer}`);
        indent = outer.indented;
      state.lexical = new JSLexical(indent, cx.stream.column(), type, null, state.lexical, info);
	logger.debug('<<< Exiting result()');
    };
    result.lex = true;
	logger.debug(`pushlex(type=${type},info=${info}): result.lex → ${result.lex}`);
	logger.debug(`<<< Exiting pushlex(type=${type},info=${info})`);
    return result;
  }
  function poplex() {
	logger.debug('>>> Entering poplex()');
    var state = cx.state;
    if (state.lexical.prev) {
      if (state.lexical.type == ")")
        state.indented = state.lexical.indented;
      state.lexical = state.lexical.prev;
	logger.debug(`poplex(): state.lexical → ${state.lexical}`);
	logger.debug(`poplex(): state.indented → ${state.indented}`);
    }
	logger.debug('<<< Exiting poplex()');
  }
  poplex.lex = true;

  function expect(wanted) {
    function exp(type) {
	logger.debug(`>>> Entering exp(type=${type})`);
      if (type == wanted) return cont();
      else if (wanted == ";") return pass();
      else return cont(exp);
	logger.debug(`<<< Exiting exp(type=${type})`);
    };
    return exp;
  }

  function statement(type, value) {
	logger.debug(`>>> Entering statement(type=${type},value=${value})`);
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
	logger.debug(`<<< Exiting statement(type=${type},value=${value})`);
    return pass(pushlex("stat"), expression, expect(";"), poplex);
  }
  function expression(type) {
	logger.debug(`>>> Entering expression(type=${type})`);
	logger.debug(`<<< Exiting expression(type=${type})`);
    return expressionInner(type, false);
  }
  function expressionNoComma(type) {
	logger.debug(`>>> Entering expressionNoComma(type=${type})`);
	logger.debug(`<<< Exiting expressionNoComma(type=${type})`);
    return expressionInner(type, true);
  }
  function expressionInner(type, noComma) {
	logger.debug(`>>> Entering expressionInner(type=${type},noComma=${noComma})`);
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
	logger.debug(`<<< Exiting expressionInner(type=${type},noComma=${noComma})`);
    return cont();
  }
  function maybeexpression(type) {
	logger.debug(`>>> Entering maybeexpression(type=${type})`);
    if (type.match(/[;\}\)\],]/)) return pass();
	logger.debug(`<<< Exiting maybeexpression(type=${type})`);
    return pass(expression);
  }
  function maybeexpressionNoComma(type) {
	logger.debug(`>>> Entering maybeexpressionNoComma(type=${type})`);
    if (type.match(/[;\}\)\],]/)) return pass();
	logger.debug(`<<< Exiting maybeexpressionNoComma(type=${type})`);
    return pass(expressionNoComma);
  }

  function maybeoperatorComma(type, value) {
	logger.debug(`>>> Entering maybeoperatorComma(type=${type},value=${value})`);
    if (type == ",") return cont(expression);
	logger.debug(`<<< Exiting maybeoperatorComma(type=${type},value=${value})`);
    return maybeoperatorNoComma(type, value, false);
  }
  function maybeoperatorNoComma(type, value, noComma) {
	logger.debug(`>>> Entering maybeoperatorNoComma(type=${type},value=${value},noComma=${noComma})`);
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
	logger.debug(`<<< Exiting maybeoperatorNoComma(type=${type},value=${value},noComma=${noComma})`);
  }
  function quasi(type, value) {
	logger.debug(`>>> Entering quasi(type=${type},value=${value})`);
    if (type != "quasi") return pass();
    if (value.slice(value.length - 2) != "${") return cont(quasi);
	logger.debug(`<<< Exiting quasi(type=${type},value=${value})`);
    return cont(expression, continueQuasi);
  }
  function continueQuasi(type) {
	logger.debug(`>>> Entering continueQuasi(type=${type})`);
    if (type == "}") {
      cx.marked = "string-2";
      cx.state.tokenize = tokenQuasi;
	logger.debug(`<<< Exiting continueQuasi(type=${type})`);
      return cont(quasi);
    }
  }
  function arrowBody(type) {
	logger.debug(`>>> Entering arrowBody(type=${type})`);
    findFatArrow(cx.stream, cx.state);
	logger.debug(`<<< Exiting arrowBody(type=${type})`);
    return pass(type == "{" ? statement : expression);
  }
  function arrowBodyNoComma(type) {
	logger.debug(`>>> Entering arrowBodyNoComma(type=${type})`);
    findFatArrow(cx.stream, cx.state);
	logger.debug(`<<< Exiting arrowBodyNoComma(type=${type})`);
    return pass(type == "{" ? statement : expressionNoComma);
  }
  function maybelabel(type) {
	logger.debug(`>>> Entering maybelabel(type=${type})`);
    if (type == ":") return cont(poplex, statement);
	logger.debug(`<<< Exiting maybelabel(type=${type})`);
    return pass(maybeoperatorComma, expect(";"), poplex);
  }
  function property(type) {
	logger.debug(`>>> Entering property(type=${type})`);
    if (type == "variable") {cx.marked = "property"; return cont();}
	logger.debug(`<<< Exiting property(type=${type})`);
  }
  function objprop(type, value) {
	logger.debug(`>>> Entering objprop(type=${type},value=${value})`);
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
	logger.debug(`<<< Exiting objprop(type=${type},value=${value})`);
      return cont(expression, expect("]"), afterprop);
    }
  }
  function getterSetter(type) {
	logger.debug(`>>> Entering getterSetter(type=${type})`);
    if (type != "variable") return pass(afterprop);
    cx.marked = "property";
	logger.debug(`<<< Exiting getterSetter(type=${type})`);
    return cont(functiondef);
  }
  function afterprop(type) {
	logger.debug(`>>> Entering afterprop(type=${type})`);
    if (type == ":") return cont(expressionNoComma);
    if (type == "(") return pass(functiondef);
	logger.debug(`<<< Exiting afterprop(type=${type})`);
  }
  function commasep(what, end) {
    function proceed(type) {
	logger.debug(`>>> Entering proceed(type=${type})`);
      if (type == ",") {
        var lex = cx.state.lexical;
        if (lex.info == "call") lex.pos = (lex.pos || 0) + 1;
        return cont(what, proceed);
      }
      if (type == end) return cont();
	logger.debug(`<<< Exiting proceed(type=${type})`);
      return cont(expect(end));
    }
    return function(type) {
      if (type == end) return cont();
      return pass(what, proceed);
    };
  }
  function contCommasep(what, end, info) {
	logger.debug(`>>> Entering contCommasep(what=${what},end=${end},info=${info})`);
    for (var i = 3; i < arguments.length; i++)
      cx.cc.push(arguments[i]);
	logger.debug(`<<< Exiting contCommasep(what=${what},end=${end},info=${info})`);
    return cont(pushlex(end, info), commasep(what, end), poplex);
  }
  function block(type) {
	logger.debug(`>>> Entering block(type=${type})`);
    if (type == "}") return cont();
	logger.debug(`<<< Exiting block(type=${type})`);
    return pass(statement, block);
  }
  function maybetype(type) {
	logger.debug(`>>> Entering maybetype(type=${type})`);
    if (isTS && type == ":") return cont(typedef);
	logger.debug(`<<< Exiting maybetype(type=${type})`);
  }
  function maybedefault(_, value) {
	logger.debug(`>>> Entering maybedefault(_=${_},value=${value})`);
    if (value == "=") return cont(expressionNoComma);
	logger.debug(`<<< Exiting maybedefault(_=${_},value=${value})`);
  }
  function typedef(type) {
	logger.debug(`>>> Entering typedef(type=${type})`);
    if (type == "variable") {cx.marked = "variable-3"; return cont();}
	logger.debug(`<<< Exiting typedef(type=${type})`);
  }
  function vardef() {
	logger.debug('>>> Entering vardef()');
	logger.debug('<<< Exiting vardef()');
    return pass(pattern, maybetype, maybeAssign, vardefCont);
  }
  function pattern(type, value) {
	logger.debug(`>>> Entering pattern(type=${type},value=${value})`);
    if (type == "variable") { register(value); return cont(); }
    if (type == "[") return contCommasep(pattern, "]");
    if (type == "{") return contCommasep(proppattern, "}");
	logger.debug(`<<< Exiting pattern(type=${type},value=${value})`);
  }
  function proppattern(type, value) {
	logger.debug(`>>> Entering proppattern(type=${type},value=${value})`);
    if (type == "variable" && !cx.stream.match(/^\s*:/, false)) {
      register(value);
      return cont(maybeAssign);
    }
    if (type == "variable") cx.marked = "property";
	logger.debug(`<<< Exiting proppattern(type=${type},value=${value})`);
    return cont(expect(":"), pattern, maybeAssign);
  }
  function maybeAssign(_type, value) {
	logger.debug(`>>> Entering maybeAssign(_type=${_type},value=${value})`);
    if (value == "=") return cont(expressionNoComma);
	logger.debug(`<<< Exiting maybeAssign(_type=${_type},value=${value})`);
  }
  function vardefCont(type) {
	logger.debug(`>>> Entering vardefCont(type=${type})`);
    if (type == ",") return cont(vardef);
	logger.debug(`<<< Exiting vardefCont(type=${type})`);
  }
  function maybeelse(type, value) {
	logger.debug(`>>> Entering maybeelse(type=${type},value=${value})`);
    if (type == "keyword b" && value == "else") return cont(pushlex("form", "else"), statement, poplex);
	logger.debug(`<<< Exiting maybeelse(type=${type},value=${value})`);
  }
  function forspec(type) {
	logger.debug(`>>> Entering forspec(type=${type})`);
    if (type == "(") return cont(pushlex(")"), forspec1, expect(")"), poplex);
	logger.debug(`<<< Exiting forspec(type=${type})`);
  }
  function forspec1(type) {
	logger.debug(`>>> Entering forspec1(type=${type})`);
    if (type == "var") return cont(vardef, expect(";"), forspec2);
    if (type == ";") return cont(forspec2);
    if (type == "variable") return cont(formaybeinof);
	logger.debug(`<<< Exiting forspec1(type=${type})`);
    return pass(expression, expect(";"), forspec2);
  }
  function formaybeinof(_type, value) {
	logger.debug(`>>> Entering formaybeinof(_type=${_type},value=${value})`);
    if (value == "in" || value == "of") { cx.marked = "keyword"; return cont(expression); }
	logger.debug(`<<< Exiting formaybeinof(_type=${_type},value=${value})`);
    return cont(maybeoperatorComma, forspec2);
  }
  function forspec2(type, value) {
	logger.debug(`>>> Entering forspec2(type=${type},value=${value})`);
    if (type == ";") return cont(forspec3);
    if (value == "in" || value == "of") { cx.marked = "keyword"; return cont(expression); }
	logger.debug(`<<< Exiting forspec2(type=${type},value=${value})`);
    return pass(expression, expect(";"), forspec3);
  }
  function forspec3(type) {
	logger.debug(`>>> Entering forspec3(type=${type})`);
    if (type != ")") cont(expression);
	logger.debug(`<<< Exiting forspec3(type=${type})`);
  }
  function functiondef(type, value) {
	logger.debug(`>>> Entering functiondef(type=${type},value=${value})`);
    if (value == "*") {cx.marked = "keyword"; return cont(functiondef);}
    if (type == "variable") {register(value); return cont(functiondef);}
    if (type == "(") return cont(pushcontext, pushlex(")"), commasep(funarg, ")"), poplex, statement, popcontext);
	logger.debug(`<<< Exiting functiondef(type=${type},value=${value})`);
  }
  function funarg(type) {
	logger.debug(`>>> Entering funarg(type=${type})`);
    if (type == "spread") return cont(funarg);
	logger.debug(`<<< Exiting funarg(type=${type})`);
    return pass(pattern, maybetype, maybedefault);
  }
  function className(type, value) {
	logger.debug(`>>> Entering className(type=${type},value=${value})`);
    if (type == "variable") {register(value); return cont(classNameAfter);}
	logger.debug(`<<< Exiting className(type=${type},value=${value})`);
  }
  function classNameAfter(type, value) {
	logger.debug(`>>> Entering classNameAfter(type=${type},value=${value})`);
    if (value == "extends") return cont(expression, classNameAfter);
    if (type == "{") return cont(pushlex("}"), classBody, poplex);
	logger.debug(`<<< Exiting classNameAfter(type=${type},value=${value})`);
  }
  function classBody(type, value) {
	logger.debug(`>>> Entering classBody(type=${type},value=${value})`);
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
	logger.debug(`<<< Exiting classBody(type=${type},value=${value})`);
  }
  function classGetterSetter(type) {
	logger.debug(`>>> Entering classGetterSetter(type=${type})`);
    if (type != "variable") return pass();
    cx.marked = "property";
	logger.debug(`<<< Exiting classGetterSetter(type=${type})`);
    return cont();
  }
  function afterExport(_type, value) {
	logger.debug(`>>> Entering afterExport(_type=${_type},value=${value})`);
    if (value == "*") { cx.marked = "keyword"; return cont(maybeFrom, expect(";")); }
    if (value == "default") { cx.marked = "keyword"; return cont(expression, expect(";")); }
	logger.debug(`<<< Exiting afterExport(_type=${_type},value=${value})`);
    return pass(statement);
  }
  function afterImport(type) {
	logger.debug(`>>> Entering afterImport(type=${type})`);
    if (type == "string") return cont();
	logger.debug(`<<< Exiting afterImport(type=${type})`);
    return pass(importSpec, maybeFrom);
  }
  function importSpec(type, value) {
	logger.debug(`>>> Entering importSpec(type=${type},value=${value})`);
    if (type == "{") return contCommasep(importSpec, "}");
    if (type == "variable") register(value);
    if (value == "*") cx.marked = "keyword";
	logger.debug(`<<< Exiting importSpec(type=${type},value=${value})`);
    return cont(maybeAs);
  }
  function maybeAs(_type, value) {
	logger.debug(`>>> Entering maybeAs(_type=${_type},value=${value})`);
    if (value == "as") { cx.marked = "keyword"; return cont(importSpec); }
	logger.debug(`<<< Exiting maybeAs(_type=${_type},value=${value})`);
  }
  function maybeFrom(_type, value) {
	logger.debug(`>>> Entering maybeFrom(_type=${_type},value=${value})`);
    if (value == "from") { cx.marked = "keyword"; return cont(expression); }
	logger.debug(`<<< Exiting maybeFrom(_type=${_type},value=${value})`);
  }
  function arrayLiteral(type) {
	logger.debug(`>>> Entering arrayLiteral(type=${type})`);
    if (type == "]") return cont();
	logger.debug(`<<< Exiting arrayLiteral(type=${type})`);
    return pass(expressionNoComma, maybeArrayComprehension);
  }
  function maybeArrayComprehension(type) {
	logger.debug(`>>> Entering maybeArrayComprehension(type=${type})`);
    if (type == "for") return pass(comprehension, expect("]"));
    if (type == ",") return cont(commasep(maybeexpressionNoComma, "]"));
	logger.debug(`<<< Exiting maybeArrayComprehension(type=${type})`);
    return pass(commasep(expressionNoComma, "]"));
  }
  function comprehension(type) {
	logger.debug(`>>> Entering comprehension(type=${type})`);
    if (type == "for") return cont(forspec, comprehension);
    if (type == "if") return cont(expression, comprehension);
	logger.debug(`<<< Exiting comprehension(type=${type})`);
  }

  function isContinuedStatement(state, textAfter) {
	logger.debug(`>>> Entering isContinuedStatement(state=${state},textAfter=${textAfter})`);
	logger.debug(`<<< Exiting isContinuedStatement(state=${state},textAfter=${textAfter})`);
    return state.lastType == "operator" || state.lastType == "," ||
      isOperatorChar.test(textAfter.charAt(0)) ||
      /[,.]/.test(textAfter.charAt(0));
  }

  // Interface

  return {
    startState: function(basecolumn) {
	logger.debug(`>>> Entering startState(basecolumn=${basecolumn})`);
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
		logger.debug(`<<< Exiting startState(basecolumn=${basecolumn})`);
        state.globalVars = parserConfig.globalVars;
	logger.debug(`startState(basecolumn=${basecolumn}): state.globalVars → ${state.globalVars}`);
      return state;
    },

    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      if (stream.sol()) {
        if (!state.lexical.hasOwnProperty("align"))
          state.lexical.align = false;
        state.indented = stream.indentation();
        findFatArrow(stream, state);
      }
      if (state.tokenize != tokenComment && stream.eatSpace()) return null;
      var style = state.tokenize(stream, state);
	logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
      if (type == "comment") return style;
      state.lastType = type == "operator" && (content == "++" || content == "--") ? "incdec" : type;
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
      return parseJS(state, style, type, content, stream);
    },

    indent: function(state, textAfter) {
	logger.debug(`>>> Entering indent(state=${state},textAfter=${textAfter})`);
      if (state.tokenize == tokenComment) return CodeMirror.Pass;
      if (state.tokenize != tokenBase) return 0;
      var firstChar = textAfter && textAfter.charAt(0), lexical = state.lexical;
	logger.debug(`indent(state=${state},textAfter=${textAfter}): firstChar → ${firstChar}`);
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
	logger.debug(`<<< Exiting indent(state=${state},textAfter=${textAfter})`);
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
