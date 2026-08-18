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
CodeMirror.defineMode("xquery", function() {
logger.debug('>>> Entering defineMode()');

  // The keywords object is set to the result of this self executing
  // function. Each keyword is a property of the keywords object whose
  // value is {type: atype, style: astyle}
  var keywords = function(){
	logger.debug('<<< Exiting defineMode()');
    // conveinence functions used to build keywords object
    function kw(type) {return {type: type, style: "keyword"};}
    var A = kw("keyword a")
      , B = kw("keyword b")
      , C = kw("keyword c")
      , operator = kw("operator")
      , atom = {type: "atom", style: "atom"}
      , punctuation = {type: "punctuation", style: null}
      , qualifier = {type: "axis_specifier", style: "qualifier"};

    // kwObj is what is return from this function at the end
    var kwObj = {
      'if': A, 'switch': A, 'while': A, 'for': A,
      'else': B, 'then': B, 'try': B, 'finally': B, 'catch': B,
      'element': C, 'attribute': C, 'let': C, 'implements': C, 'import': C, 'module': C, 'namespace': C,
      'return': C, 'super': C, 'this': C, 'throws': C, 'where': C, 'private': C,
      ',': punctuation,
      'null': atom, 'fn:false()': atom, 'fn:true()': atom
    };

    // a list of 'basic' keywords. For each add a property to kwObj with the value of
    // {type: basic[i], style: "keyword"} e.g. 'after' --> {type: "after", style: "keyword"}
    var basic = ['after','ancestor','ancestor-or-self','and','as','ascending','assert','attribute','before',
    'by','case','cast','child','comment','declare','default','define','descendant','descendant-or-self',
    'descending','document','document-node','element','else','eq','every','except','external','following',
    'following-sibling','follows','for','function','if','import','in','instance','intersect','item',
    'let','module','namespace','node','node','of','only','or','order','parent','precedes','preceding',
    'preceding-sibling','processing-instruction','ref','return','returns','satisfies','schema','schema-element',
    'self','some','sortby','stable','text','then','to','treat','typeswitch','union','variable','version','where',
    'xquery', 'empty-sequence'];
    for(var i=0, l=basic.length; i < l; i++) { kwObj[basic[i]] = kw(basic[i]);};
	logger.debug(`function(): basic → ${basic}`);

    // a list of types. For each add a property to kwObj with the value of
    // {type: "atom", style: "atom"}
    var types = ['xs:string', 'xs:float', 'xs:decimal', 'xs:double', 'xs:integer', 'xs:boolean', 'xs:date', 'xs:dateTime',
    'xs:time', 'xs:duration', 'xs:dayTimeDuration', 'xs:time', 'xs:yearMonthDuration', 'numeric', 'xs:hexBinary',
    'xs:base64Binary', 'xs:anyURI', 'xs:QName', 'xs:byte','xs:boolean','xs:anyURI','xf:yearMonthDuration'];
    for(var i=0, l=types.length; i < l; i++) { kwObj[types[i]] = atom;};
	logger.debug(`function(): types → ${types}`);

    // each operator will add a property to kwObj with value of {type: "operator", style: "keyword"}
    var operators = ['eq', 'ne', 'lt', 'le', 'gt', 'ge', ':=', '=', '>', '>=', '<', '<=', '.', '|', '?', 'and', 'or', 'div', 'idiv', 'mod', '*', '/', '+', '-'];
	logger.debug(`function(): operators → ${operators}`);
    for(var i=0, l=operators.length; i < l; i++) { kwObj[operators[i]] = operator;};

    // each axis_specifiers will add a property to kwObj with value of {type: "axis_specifier", style: "qualifier"}
    var axis_specifiers = ["self::", "attribute::", "child::", "descendant::", "descendant-or-self::", "parent::",
    "ancestor::", "ancestor-or-self::", "following::", "preceding::", "following-sibling::", "preceding-sibling::"];
    for(var i=0, l=axis_specifiers.length; i < l; i++) { kwObj[axis_specifiers[i]] = qualifier; };
	logger.debug(`function(): axis_specifiers → ${axis_specifiers}`);

    return kwObj;
  }();

  function chain(stream, state, f) {
	logger.debug(`>>> Entering chain(stream=${stream},state=${state},f=${f})`);
    state.tokenize = f;
	logger.debug(`<<< Exiting chain(stream=${stream},state=${state},f=${f})`);
    return f(stream, state);
  }

  // the primary mode tokenizer
  function tokenBase(stream, state) {
	logger.debug(`>>> Entering tokenBase(stream=${stream},state=${state})`);
    var ch = stream.next(),
        mightBeFunction = false,
        isEQName = isEQNameAhead(stream);
		logger.debug(`tokenBase(stream=${stream},state=${state}): ch → ${ch}`);

    // an XML tag (if not in some sub, chained tokenizer)
    if (ch == "<") {
      if(stream.match("!--", true))
        return chain(stream, state, tokenXMLComment);

      if(stream.match("![CDATA", false)) {
        state.tokenize = tokenCDATA;
		logger.debug(`tokenBase(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        return "tag";
      }

      if(stream.match("?", false)) {
        return chain(stream, state, tokenPreProcessing);
      }

      var isclose = stream.eat("/");
	logger.debug(`tokenBase(stream=${stream},state=${state}): isclose → ${isclose}`);
      stream.eatSpace();
      var tagName = "", c;
	logger.debug(`tokenBase(stream=${stream},state=${state}): tagName → ${tagName}`);
      while ((c = stream.eat(/[^\s\u00a0=<>\"\'\/?]/))) tagName += c;

      return chain(stream, state, tokenTag(tagName, isclose));
    }
    // start code block
    else if(ch == "{") {
      pushStateStack(state,{ type: "codeblock"});
      return null;
    }
    // end code block
    else if(ch == "}") {
      popStateStack(state);
      return null;
    }
    // if we're in an XML block
    else if(isInXmlBlock(state)) {
      if(ch == ">")
        return "tag";
      else if(ch == "/" && stream.eat(">")) {
        popStateStack(state);
        return "tag";
      }
      else
        return "variable";
    }
    // if a number
    else if (/\d/.test(ch)) {
      stream.match(/^\d*(?:\.\d*)?(?:E[+\-]?\d+)?/);
      return "atom";
    }
    // comment start
    else if (ch === "(" && stream.eat(":")) {
      pushStateStack(state, { type: "comment"});
      return chain(stream, state, tokenComment);
    }
    // quoted string
    else if (  !isEQName && (ch === '"' || ch === "'"))
      return chain(stream, state, tokenString(ch));
    // variable
    else if(ch === "$") {
      return chain(stream, state, tokenVariable);
    }
    // assignment
    else if(ch ===":" && stream.eat("=")) {
      return "keyword";
    }
    // open paren
    else if(ch === "(") {
      pushStateStack(state, { type: "paren"});
      return null;
    }
    // close paren
    else if(ch === ")") {
      popStateStack(state);
      return null;
    }
    // open paren
    else if(ch === "[") {
      pushStateStack(state, { type: "bracket"});
      return null;
    }
    // close paren
    else if(ch === "]") {
      popStateStack(state);
      return null;
    }
    else {
      var known = keywords.propertyIsEnumerable(ch) && keywords[ch];

      // if there's a EQName ahead, consume the rest of the string portion, it's likely a function
      if(isEQName && ch === '\"') while(stream.next() !== '"'){}
      if(isEQName && ch === '\'') while(stream.next() !== '\''){}

      // gobble up a word if the character is not known
      if(!known) stream.eatWhile(/[\w\$_-]/);

      // gobble a colon in the case that is a lib func type call fn:doc
      var foundColon = stream.eat(":");

      // if there's not a second colon, gobble another word. Otherwise, it's probably an axis specifier
      // which should get matched as a keyword
      if(!stream.eat(":") && foundColon) {
        stream.eatWhile(/[\w\$_-]/);
      }
      // if the next non whitespace character is an open paren, this is probably a function (if not a keyword of other sort)
      if(stream.match(/^[ \t]*\(/, false)) {
        mightBeFunction = true;
      }
      // is the word a keyword?
      var word = stream.current();
      known = keywords.propertyIsEnumerable(word) && keywords[word];

      // if we think it's a function call but not yet known,
      // set style to variable for now for lack of something better
      if(mightBeFunction && !known) known = {type: "function_call", style: "variable def"};

      // if the previous word was element, attribute, axis specifier, this word should be the name of that
      if(isInXmlConstructor(state)) {
        popStateStack(state);
        return "variable";
      }
      // as previously checked, if the word is element,attribute, axis specifier, call it an "xmlconstructor" and
      // push the stack so we know to look for it on the next word
      if(word == "element" || word == "attribute" || known.type == "axis_specifier") pushStateStack(state, {type: "xmlconstructor"});

      // if the word is known, return the details of that else just call this a generic 'word'
      return known ? known.style : "variable";
    }
  }

  // handle comments, including nested
  function tokenComment(stream, state) {
	logger.debug(`>>> Entering tokenComment(stream=${stream},state=${state})`);
    var maybeEnd = false, maybeNested = false, nestedCount = 0, ch;
    while (ch = stream.next()) {
      if (ch == ")" && maybeEnd) {
        if(nestedCount > 0)
          nestedCount--;
        else {
          popStateStack(state);
          break;
        }
      }
      else if(ch == ":" && maybeNested) {
        nestedCount++;
      }
      maybeEnd = (ch == ":");
      maybeNested = (ch == "(");
    }

	logger.debug(`<<< Exiting tokenComment(stream=${stream},state=${state})`);
    return "comment";
  }

  // tokenizer for string literals
  // optionally pass a tokenizer function to set state.tokenize back to when finished
  function tokenString(quote, f) {
	logger.debug(`>>> Entering tokenString(quote=${quote},f=${f})`);
    return function(stream, state) {
      var ch;
	logger.debug(`<<< Exiting tokenString(quote=${quote},f=${f})`);

      if(isInString(state) && stream.current() == quote) {
        popStateStack(state);
        if(f) state.tokenize = f;
        return "string";
      }

      pushStateStack(state, { type: "string", name: quote, tokenize: tokenString(quote, f) });

      // if we're in a string and in an XML block, allow an embedded code block
      if(stream.match("{", false) && isInXmlAttributeBlock(state)) {
        state.tokenize = tokenBase;
		logger.debug(`tokenString(quote=${quote},f=${f}): state.tokenize → ${state.tokenize}`);
		logger.debug(`function(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        return "string";
      }


      while (ch = stream.next()) {
        if (ch ==  quote) {
          popStateStack(state);
          if(f) state.tokenize = f;
          break;
        }
        else {
          // if we're in a string and in an XML block, allow an embedded code block in an attribute
          if(stream.match("{", false) && isInXmlAttributeBlock(state)) {
            state.tokenize = tokenBase;
            return "string";
          }

        }
      }

      return "string";
    };
  }

  // tokenizer for variables
  function tokenVariable(stream, state) {
	logger.debug(`>>> Entering tokenVariable(stream=${stream},state=${state})`);
    var isVariableChar = /[\w\$_-]/;

    // a variable may start with a quoted EQName so if the next character is quote, consume to the next quote
    if(stream.eat("\"")) {
      while(stream.next() !== '\"'){};
      stream.eat(":");
    } else {
      stream.eatWhile(isVariableChar);
      if(!stream.match(":=", false)) stream.eat(":");
    }
    stream.eatWhile(isVariableChar);
    state.tokenize = tokenBase;
	logger.debug(`tokenVariable(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting tokenVariable(stream=${stream},state=${state})`);
    return "variable";
  }

  // tokenizer for XML tags
  function tokenTag(name, isclose) {
    return function(stream, state) {
      stream.eatSpace();
      if(isclose && stream.eat(">")) {
        popStateStack(state);
        state.tokenize = tokenBase;
        return "tag";
      }
      // self closing tag without attributes?
      if(!stream.eat("/"))
        pushStateStack(state, { type: "tag", name: name, tokenize: tokenBase});
      if(!stream.eat(">")) {
        state.tokenize = tokenAttribute;
        return "tag";
      }
      else {
        state.tokenize = tokenBase;
      }
      return "tag";
    };
  }

  // tokenizer for XML attributes
  function tokenAttribute(stream, state) {
	logger.debug(`>>> Entering tokenAttribute(stream=${stream},state=${state})`);
    var ch = stream.next();

	logger.debug(`tokenAttribute(stream=${stream},state=${state}): ch → ${ch}`);
    if(ch == "/" && stream.eat(">")) {
      if(isInXmlAttributeBlock(state)) popStateStack(state);
      if(isInXmlBlock(state)) popStateStack(state);
      return "tag";
    }
    if(ch == ">") {
      if(isInXmlAttributeBlock(state)) popStateStack(state);
      return "tag";
    }
    if(ch == "=")
      return null;
    // quoted string
    if (ch == '"' || ch == "'")
      return chain(stream, state, tokenString(ch, tokenAttribute));

    if(!isInXmlAttributeBlock(state))
      pushStateStack(state, { type: "attribute", tokenize: tokenAttribute});

    stream.eat(/[a-zA-Z_:]/);
    stream.eatWhile(/[-a-zA-Z0-9_:.]/);
    stream.eatSpace();

    // the case where the attribute has not value and the tag was closed
    if(stream.match(">", false) || stream.match("/", false)) {
      popStateStack(state);
      state.tokenize = tokenBase;
	logger.debug(`tokenAttribute(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
    }

	logger.debug(`<<< Exiting tokenAttribute(stream=${stream},state=${state})`);
    return "attribute";
  }

  // handle comments, including nested
  function tokenXMLComment(stream, state) {
	logger.debug(`>>> Entering tokenXMLComment(stream=${stream},state=${state})`);
    var ch;
    while (ch = stream.next()) {
      if (ch == "-" && stream.match("->", true)) {
        state.tokenize = tokenBase;
		logger.debug(`tokenXMLComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
		logger.debug(`<<< Exiting tokenXMLComment(stream=${stream},state=${state})`);
        return "comment";
      }
    }
  }


  // handle CDATA
  function tokenCDATA(stream, state) {
	logger.debug(`>>> Entering tokenCDATA(stream=${stream},state=${state})`);
    var ch;
    while (ch = stream.next()) {
      if (ch == "]" && stream.match("]", true)) {
        state.tokenize = tokenBase;
		logger.debug(`tokenCDATA(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
		logger.debug(`<<< Exiting tokenCDATA(stream=${stream},state=${state})`);
        return "comment";
      }
    }
  }

  // handle preprocessing instructions
  function tokenPreProcessing(stream, state) {
	logger.debug(`>>> Entering tokenPreProcessing(stream=${stream},state=${state})`);
    var ch;
    while (ch = stream.next()) {
      if (ch == "?" && stream.match(">", true)) {
        state.tokenize = tokenBase;
		logger.debug(`tokenPreProcessing(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
		logger.debug(`<<< Exiting tokenPreProcessing(stream=${stream},state=${state})`);
        return "comment meta";
      }
    }
  }


  // functions to test the current context of the state
  function isInXmlBlock(state) { return isIn(state, "tag"); }
  function isInXmlAttributeBlock(state) { return isIn(state, "attribute"); }
  function isInXmlConstructor(state) { return isIn(state, "xmlconstructor"); }
  function isInString(state) { return isIn(state, "string"); }

  function isEQNameAhead(stream) {
	logger.debug(`>>> Entering isEQNameAhead(stream=${stream})`);
    // assume we've already eaten a quote (")
    if(stream.current() === '"')
      return stream.match(/^[^\"]+\"\:/, false);
    else if(stream.current() === '\'')
      return stream.match(/^[^\"]+\'\:/, false);
    else
	logger.debug(`>>> Entering isIn(state=${state},type=${type})`);
	logger.debug(`<<< Exiting isIn(state=${state},type=${type})`);
      return false;
  }

  function isIn(state, type) {
    return (state.stack.length && state.stack[state.stack.length - 1].type == type);
  }

  function pushStateStack(state, newState) {
	logger.debug(`>>> Entering pushStateStack(state=${state},newState=${newState})`);
    state.stack.push(newState);
	logger.debug(`<<< Exiting pushStateStack(state=${state},newState=${newState})`);
  }

  function popStateStack(state) {
	logger.debug(`>>> Entering popStateStack(state=${state})`);
    state.stack.pop();
    var reinstateTokenize = state.stack.length && state.stack[state.stack.length-1].tokenize;
    state.tokenize = reinstateTokenize || tokenBase;
	logger.debug(`popStateStack(state=${state}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting popStateStack(state=${state})`);
  }

  // the interface for the mode API
  return {
    startState: function() {
	logger.debug('>>> Entering startState()');
      return {
        tokenize: tokenBase,
        cc: [],
        stack: []
      };
	logger.debug('<<< Exiting startState()');
    },

    token: function(stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      if (stream.eatSpace()) return null;
      var style = state.tokenize(stream, state);
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
	logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
      return style;
    },

    blockCommentStart: "(:",
    blockCommentEnd: ":)"

  };

});

CodeMirror.defineMIME("application/xquery", "xquery");

});
