const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

/***
    |''Name''|tiddlywiki.js|
    |''Description''|Enables TiddlyWikiy syntax highlighting using CodeMirror|
    |''Author''|PMario|
    |''Version''|0.1.7|
    |''Status''|''stable''|
    |''Source''|[[GitHub|https://github.com/pmario/CodeMirror2/blob/tw-syntax/mode/tiddlywiki]]|
    |''Documentation''|http://codemirror.tiddlyspace.com/|
    |''License''|[[MIT License|http://www.opensource.org/licenses/mit-license.php]]|
    |''CoreVersion''|2.5.0|
    |''Requires''|codemirror.js|
    |''Keywords''|syntax highlighting color code mirror codemirror|
    ! Info
    CoreVersion parameter is needed for TiddlyWiki only!
***/
//{{{

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
CodeMirror.defineMode("tiddlywiki", function () {
logger.debug('>>> Entering defineMode()');
  // Tokenizer
  var textwords = {};

  var keywords = function () {
    function kw(type) {
	logger.debug(`>>> Entering kw(type=${type})`);
	logger.debug(`<<< Exiting kw(type=${type})`);
      return { type: type, style: "macro"};
    }
    return {
      "allTags": kw('allTags'), "closeAll": kw('closeAll'), "list": kw('list'),
      "newJournal": kw('newJournal'), "newTiddler": kw('newTiddler'),
      "permaview": kw('permaview'), "saveChanges": kw('saveChanges'),
      "search": kw('search'), "slider": kw('slider'),   "tabs": kw('tabs'),
      "tag": kw('tag'), "tagging": kw('tagging'),       "tags": kw('tags'),
      "tiddler": kw('tiddler'), "timeline": kw('timeline'),
      "today": kw('today'), "version": kw('version'),   "option": kw('option'),

      "with": kw('with'),
      "filter": kw('filter')
    };
  }();

  var isSpaceName = /[\w_\-]/i,
  reHR = /^\-\-\-\-+$/,                                 // <hr>
  reWikiCommentStart = /^\/\*\*\*$/,            // /***
  reWikiCommentStop = /^\*\*\*\/$/,             // ***/
  reBlockQuote = /^<<<$/,

  reJsCodeStart = /^\/\/\{\{\{$/,                       // //{{{ js block start
  reJsCodeStop = /^\/\/\}\}\}$/,                        // //}}} js stop
  reXmlCodeStart = /^<!--\{\{\{-->$/,           // xml block start
  reXmlCodeStop = /^<!--\}\}\}-->$/,            // xml stop

  reCodeBlockStart = /^\{\{\{$/,                        // {{{ TW text div block start
  reCodeBlockStop = /^\}\}\}$/,                 // }}} TW text stop

  reUntilCodeStop = /.*?\}\}\}/;

  function chain(stream, state, f) {
	logger.debug(`>>> Entering chain(stream=${stream},state=${state},f=${f})`);
    state.tokenize = f;
	logger.debug(`chain(stream=${stream},state=${state},f=${f}): state.tokenize → ${state.tokenize}`);
	logger.debug(`<<< Exiting chain(stream=${stream},state=${state},f=${f})`);
    return f(stream, state);
  }

  function jsTokenBase(stream, state) {
	logger.debug(`>>> Entering jsTokenBase(stream=${stream},state=${state})`);
    var sol = stream.sol(), ch;

	logger.debug(`jsTokenBase(stream=${stream},state=${state}): sol → ${sol}`);
    state.block = false;        // indicates the start of a code block.
	logger.debug(`jsTokenBase(stream=${stream},state=${state}): state.block → ${state.block}`);

    ch = stream.peek();         // don't eat, to make matching simpler

    // check start of  blocks
    if (sol && /[<\/\*{}\-]/.test(ch)) {
      if (stream.match(reCodeBlockStart)) {
        state.block = true;
        return chain(stream, state, twTokenCode);
      }
      if (stream.match(reBlockQuote)) {
        return 'quote';
      }
      if (stream.match(reWikiCommentStart) || stream.match(reWikiCommentStop)) {
        return 'comment';
      }
      if (stream.match(reJsCodeStart) || stream.match(reJsCodeStop) || stream.match(reXmlCodeStart) || stream.match(reXmlCodeStop)) {
        return 'comment';
      }
      if (stream.match(reHR)) {
        return 'hr';
      }
    } // sol
    ch = stream.next();

    if (sol && /[\/\*!#;:>|]/.test(ch)) {
      if (ch == "!") { // tw header
        stream.skipToEnd();
        return "header";
      }
      if (ch == "*") { // tw list
        stream.eatWhile('*');
        return "comment";
      }
      if (ch == "#") { // tw numbered list
        stream.eatWhile('#');
        return "comment";
      }
      if (ch == ";") { // definition list, term
        stream.eatWhile(';');
        return "comment";
      }
      if (ch == ":") { // definition list, description
        stream.eatWhile(':');
        return "comment";
      }
      if (ch == ">") { // single line quote
        stream.eatWhile(">");
        return "quote";
      }
      if (ch == '|') {
        return 'header';
      }
    }

    if (ch == '{' && stream.match(/\{\{/)) {
      return chain(stream, state, twTokenCode);
    }

    // rudimentary html:// file:// link matching. TW knows much more ...
    if (/[hf]/i.test(ch)) {
      if (/[ti]/i.test(stream.peek()) && stream.match(/\b(ttps?|tp|ile):\/\/[\-A-Z0-9+&@#\/%?=~_|$!:,.;]*[A-Z0-9+&@#\/%=~_|$]/i)) {
        return "link";
      }
    }
    // just a little string indicator, don't want to have the whole string covered
    if (ch == '"') {
      return 'string';
    }
    if (ch == '~') {    // _no_ CamelCase indicator should be bold
      return 'brace';
    }
    if (/[\[\]]/.test(ch)) { // check for [[..]]
      if (stream.peek() == ch) {
        stream.next();
        return 'brace';
      }
    }
    if (ch == "@") {    // check for space link. TODO fix @@...@@ highlighting
      stream.eatWhile(isSpaceName);
      return "link";
    }
    if (/\d/.test(ch)) {        // numbers
      stream.eatWhile(/\d/);
      return "number";
    }
    if (ch == "/") { // tw invisible comment
      if (stream.eat("%")) {
        return chain(stream, state, twTokenComment);
      }
      else if (stream.eat("/")) { //
        return chain(stream, state, twTokenEm);
      }
    }
    if (ch == "_") { // tw underline
      if (stream.eat("_")) {
        return chain(stream, state, twTokenUnderline);
      }
    }
    // strikethrough and mdash handling
    if (ch == "-") {
      if (stream.eat("-")) {
        // if strikethrough looks ugly, change CSS.
        if (stream.peek() != ' ')
          return chain(stream, state, twTokenStrike);
        // mdash
        if (stream.peek() == ' ')
          return 'brace';
      }
    }
    if (ch == "'") { // tw bold
      if (stream.eat("'")) {
        return chain(stream, state, twTokenStrong);
      }
    }
    if (ch == "<") { // tw macro
      if (stream.eat("<")) {
        return chain(stream, state, twTokenMacro);
      }
    }
    else {
      return null;
    }

    // core macro handling
    stream.eatWhile(/[\w\$_]/);
    var word = stream.current(),
    known = textwords.propertyIsEnumerable(word) && textwords[word];

	logger.debug(`jsTokenBase(stream=${stream},state=${state}): word → ${word}`);
	logger.debug(`<<< Exiting jsTokenBase(stream=${stream},state=${state})`);
    return known ? known.style : null;
  } // jsTokenBase()

  // tw invisible comment
  function twTokenComment(stream, state) {
	logger.debug(`>>> Entering twTokenComment(stream=${stream},state=${state})`);
    var maybeEnd = false,
    ch;
    while (ch = stream.next()) {
      if (ch == "/" && maybeEnd) {
        state.tokenize = jsTokenBase;
		logger.debug(`twTokenComment(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        break;
      }
      maybeEnd = (ch == "%");
    }
	logger.debug(`twTokenComment(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
	logger.debug(`<<< Exiting twTokenComment(stream=${stream},state=${state})`);
    return "comment";
  }

  // tw strong / bold
  function twTokenStrong(stream, state) {
	logger.debug(`>>> Entering twTokenStrong(stream=${stream},state=${state})`);
    var maybeEnd = false,
    ch;
    while (ch = stream.next()) {
      if (ch == "'" && maybeEnd) {
        state.tokenize = jsTokenBase;
		logger.debug(`twTokenStrong(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        break;
      }
      maybeEnd = (ch == "'");
    }
	logger.debug(`twTokenStrong(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
	logger.debug(`<<< Exiting twTokenStrong(stream=${stream},state=${state})`);
    return "strong";
  }

  // tw code
  function twTokenCode(stream, state) {
	logger.debug(`>>> Entering twTokenCode(stream=${stream},state=${state})`);
    var sb = state.block;

    if (sb && stream.current()) {
      return "comment";
    }

    if (!sb && stream.match(reUntilCodeStop)) {
      state.tokenize = jsTokenBase;
	logger.debug(`twTokenCode(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
      return "comment";
    }

    if (sb && stream.sol() && stream.match(reCodeBlockStop)) {
      state.tokenize = jsTokenBase;
      return "comment";
    }

    stream.next();
	logger.debug(`<<< Exiting twTokenCode(stream=${stream},state=${state})`);
    return "comment";
  }

  // tw em / italic
  function twTokenEm(stream, state) {
	logger.debug(`>>> Entering twTokenEm(stream=${stream},state=${state})`);
    var maybeEnd = false,
    ch;
    while (ch = stream.next()) {
      if (ch == "/" && maybeEnd) {
        state.tokenize = jsTokenBase;
		logger.debug(`twTokenEm(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        break;
      }
      maybeEnd = (ch == "/");
    }
	logger.debug(`twTokenEm(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
	logger.debug(`<<< Exiting twTokenEm(stream=${stream},state=${state})`);
    return "em";
  }

  // tw underlined text
  function twTokenUnderline(stream, state) {
	logger.debug(`>>> Entering twTokenUnderline(stream=${stream},state=${state})`);
    var maybeEnd = false,
    ch;
    while (ch = stream.next()) {
      if (ch == "_" && maybeEnd) {
        state.tokenize = jsTokenBase;
		logger.debug(`twTokenUnderline(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        break;
      }
      maybeEnd = (ch == "_");
    }
	logger.debug(`twTokenUnderline(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
	logger.debug(`<<< Exiting twTokenUnderline(stream=${stream},state=${state})`);
    return "underlined";
  }

  // tw strike through text looks ugly
  // change CSS if needed
  function twTokenStrike(stream, state) {
	logger.debug(`>>> Entering twTokenStrike(stream=${stream},state=${state})`);
    var maybeEnd = false, ch;

    while (ch = stream.next()) {
      if (ch == "-" && maybeEnd) {
        state.tokenize = jsTokenBase;
		logger.debug(`twTokenStrike(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
        break;
      }
      maybeEnd = (ch == "-");
    }
	logger.debug(`twTokenStrike(stream=${stream},state=${state}): maybeEnd → ${maybeEnd}`);
	logger.debug(`<<< Exiting twTokenStrike(stream=${stream},state=${state})`);
    return "strikethrough";
  }

  // macro
  function twTokenMacro(stream, state) {
	logger.debug(`>>> Entering twTokenMacro(stream=${stream},state=${state})`);
    var ch, word, known;

	logger.debug(`twTokenMacro(stream=${stream},state=${state}): ch → ${ch}`);
    if (stream.current() == '<<') {
      return 'macro';
    }

    ch = stream.next();
    if (!ch) {
      state.tokenize = jsTokenBase;
	logger.debug(`twTokenMacro(stream=${stream},state=${state}): state.tokenize → ${state.tokenize}`);
      return null;
    }
    if (ch == ">") {
      if (stream.peek() == '>') {
        stream.next();
        state.tokenize = jsTokenBase;
        return "macro";
      }
    }

    stream.eatWhile(/[\w\$_]/);
    word = stream.current();
    known = keywords.propertyIsEnumerable(word) && keywords[word];

    if (known) {
      return known.style, word;
    }
    else {
	logger.debug(`<<< Exiting twTokenMacro(stream=${stream},state=${state})`);
      return null, word;
    }
  }

  // Interface
  return {
    startState: function () {
	logger.debug('>>> Entering startState()');
      return {
        tokenize: jsTokenBase,
        indented: 0,
        level: 0
      };
	logger.debug('<<< Exiting startState()');
    },

    token: function (stream, state) {
	logger.debug(`>>> Entering token(stream=${stream},state=${state})`);
      if (stream.eatSpace()) return null;
      var style = state.tokenize(stream, state);
	logger.debug(`<<< Exiting token(stream=${stream},state=${state})`);
	logger.debug(`token(stream=${stream},state=${state}): style → ${style}`);
      return style;
    },

    electricChars: ""
  };
});

CodeMirror.defineMIME("text/x-tiddlywiki", "tiddlywiki");
});

//}}}
