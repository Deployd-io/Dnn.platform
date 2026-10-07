const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") { // CommonJS
    mod(require("../../lib/codemirror"));
  } else if (typeof define == "function" && define.amd) { // AMD
    define(["../../lib/codemirror"], mod);
  } else { // Plain browser env
    mod(CodeMirror);
  }
})(function(CodeMirror) {
  "use strict";

  var TOKEN_STYLES = {
    addition: "positive",
    attributes: "attribute",
    bold: "strong",
    cite: "keyword",
    code: "atom",
    definitionList: "number",
    deletion: "negative",
    div: "punctuation",
    em: "em",
    footnote: "variable",
    footCite: "qualifier",
    header: "header",
    html: "comment",
    image: "string",
    italic: "em",
    link: "link",
    linkDefinition: "link",
    list1: "variable-2",
    list2: "variable-3",
    list3: "keyword",
    notextile: "string-2",
    pre: "operator",
    p: "property",
    quote: "bracket",
    span: "quote",
    specialChar: "tag",
    strong: "strong",
    sub: "builtin",
    sup: "builtin",
    table: "variable-3",
    tableHeading: "operator"
  };

  function startNewLine(stream, state) {
	logger.error(`>>> Entering startNewLine(stream=${stream},state=${state})`, { "event.action": "startNewLine", "logrite.stream": stream, "logrite.state": state });
    state.mode = Modes.newLayout;
    state.tableHeading = false;
	logger.debug(`startNewLine(stream=${stream},state=${state}): state.mode → ${state.mode}`, { "event.action": "startNewLine", "logrite.stream": stream, "logrite.state": state });

    if (state.layoutType === "definitionList" && state.spanningLayout &&
        stream.match(RE("definitionListEnd"), false))
      state.spanningLayout = false;
		logger.debug(`startNewLine(stream=${stream},state=${state}): state.tableHeading → ${state.tableHeading}`, { "event.action": "startNewLine", "logrite.stream": stream, "logrite.state": state });
	logger.warn(`<<< Exiting startNewLine(stream=${stream},state=${state})`, { "event.action": "startNewLine", "logrite.stream": stream, "logrite.state": state });
  }

  function handlePhraseModifier(stream, state, ch) {
	logger.error(`>>> Entering handlePhraseModifier(stream=${stream},state=${state},ch=${ch})`, { "event.action": "handlePhraseModifier", "logrite.stream": stream, "logrite.state": state, "logrite.ch": ch });
    if (ch === "_") {
      if (stream.eat("_"))
        return togglePhraseModifier(stream, state, "italic", /__/, 2);
      else
        return togglePhraseModifier(stream, state, "em", /_/, 1);
    }

    if (ch === "*") {
      if (stream.eat("*")) {
        return togglePhraseModifier(stream, state, "bold", /\*\*/, 2);
      }
      return togglePhraseModifier(stream, state, "strong", /\*/, 1);
    }

    if (ch === "[") {
      if (stream.match(/\d+\]/)) state.footCite = true;
      return tokenStyles(state);
    }

    if (ch === "(") {
      var spec = stream.match(/^(r|tm|c)\)/);
	logger.error(`handlePhraseModifier(stream=${stream},state=${state},ch=${ch}): spec → ${spec}`, { "event.action": "handlePhraseModifier", "logrite.stream": stream, "logrite.state": state, "logrite.ch": ch });
      if (spec)
        return tokenStylesWith(state, TOKEN_STYLES.specialChar);
    }

    if (ch === "<" && stream.match(/(\w+)[^>]+>[^<]+<\/\1>/))
      return tokenStylesWith(state, TOKEN_STYLES.html);

    if (ch === "?" && stream.eat("?"))
      return togglePhraseModifier(stream, state, "cite", /\?\?/, 2);

    if (ch === "=" && stream.eat("="))
      return togglePhraseModifier(stream, state, "notextile", /==/, 2);

    if (ch === "-" && !stream.eat("-"))
      return togglePhraseModifier(stream, state, "deletion", /-/, 1);

    if (ch === "+")
      return togglePhraseModifier(stream, state, "addition", /\+/, 1);

    if (ch === "~")
      return togglePhraseModifier(stream, state, "sub", /~/, 1);

    if (ch === "^")
      return togglePhraseModifier(stream, state, "sup", /\^/, 1);

    if (ch === "%")
      return togglePhraseModifier(stream, state, "span", /%/, 1);

    if (ch === "@")
      return togglePhraseModifier(stream, state, "code", /@/, 1);

    if (ch === "!") {
      var type = togglePhraseModifier(stream, state, "image", /(?:\([^\)]+\))?!/, 1);
	logger.error(`handlePhraseModifier(stream=${stream},state=${state},ch=${ch}): type → ${type}`, { "event.action": "handlePhraseModifier", "logrite.stream": stream, "logrite.state": state, "logrite.ch": ch });
      stream.match(/^:\S+/); // optional Url portion
      return type;
    }
	logger.warn(`<<< Exiting handlePhraseModifier(stream=${stream},state=${state},ch=${ch})`, { "event.action": "handlePhraseModifier", "logrite.stream": stream, "logrite.state": state, "logrite.ch": ch });
    return tokenStyles(state);
  }

  function togglePhraseModifier(stream, state, phraseModifier, closeRE, openSize) {
	logger.error(`>>> Entering togglePhraseModifier(stream=${stream},state=${state},phraseModifier=${phraseModifier},closeRE=${closeRE},openSize=${openSize})`, { "event.action": "togglePhraseModifier", "logrite.stream": stream, "logrite.state": state, "logrite.phraseModifier": phraseModifier, "logrite.closeRE": closeRE, "logrite.openSize": openSize });
    var charBefore = stream.pos > openSize ? stream.string.charAt(stream.pos - openSize - 1) : null;
    var charAfter = stream.peek();
	logger.error(`togglePhraseModifier(stream=${stream},state=${state},phraseModifier=${phraseModifier},closeRE=${closeRE},openSize=${openSize}): charBefore → ${charBefore}`, { "event.action": "togglePhraseModifier", "logrite.stream": stream, "logrite.state": state, "logrite.phraseModifier": phraseModifier, "logrite.closeRE": closeRE, "logrite.openSize": openSize });
    if (state[phraseModifier]) {
      if ((!charAfter || /\W/.test(charAfter)) && charBefore && /\S/.test(charBefore)) {
        var type = tokenStyles(state);
		logger.error(`togglePhraseModifier(stream=${stream},state=${state},phraseModifier=${phraseModifier},closeRE=${closeRE},openSize=${openSize}): type → ${type}`, { "event.action": "togglePhraseModifier", "logrite.stream": stream, "logrite.state": state, "logrite.phraseModifier": phraseModifier, "logrite.closeRE": closeRE, "logrite.openSize": openSize });
        state[phraseModifier] = false;
        return type;
      }
    } else if ((!charBefore || /\W/.test(charBefore)) && charAfter && /\S/.test(charAfter) &&
               stream.match(new RegExp("^.*\\S" + closeRE.source + "(?:\\W|$)"), false)) {
      state[phraseModifier] = true;
      state.mode = Modes.attributes;
	logger.debug(`togglePhraseModifier(stream=${stream},state=${state},phraseModifier=${phraseModifier},closeRE=${closeRE},openSize=${openSize}): state.mode → ${state.mode}`, { "event.action": "togglePhraseModifier", "logrite.stream": stream, "logrite.state": state, "logrite.phraseModifier": phraseModifier, "logrite.closeRE": closeRE, "logrite.openSize": openSize });
    }
	logger.error(`togglePhraseModifier(stream=${stream},state=${state},phraseModifier=${phraseModifier},closeRE=${closeRE},openSize=${openSize}): charAfter → ${charAfter}`, { "event.action": "togglePhraseModifier", "logrite.stream": stream, "logrite.state": state, "logrite.phraseModifier": phraseModifier, "logrite.closeRE": closeRE, "logrite.openSize": openSize });
    return tokenStyles(state);
  };

  function tokenStyles(state) {
	logger.error(`>>> Entering tokenStyles(state=${state})`, { "event.action": "tokenStyles", "logrite.state": state });
    var disabled = textileDisabled(state);
    if (disabled) return disabled;
	logger.error(`tokenStyles(state=${state}): disabled → ${disabled}`, { "event.action": "tokenStyles", "logrite.state": state });

    var styles = [];
    if (state.layoutType) styles.push(TOKEN_STYLES[state.layoutType]);

    styles = styles.concat(activeStyles(
      state, "addition", "bold", "cite", "code", "deletion", "em", "footCite",
      "image", "italic", "link", "span", "strong", "sub", "sup", "table", "tableHeading"));

    if (state.layoutType === "header")
      styles.push(TOKEN_STYLES.header + "-" + state.header);

	logger.warn(`<<< Exiting tokenStyles(state=${state})`, { "event.action": "tokenStyles", "logrite.state": state });
    return styles.length ? styles.join(" ") : null;
  }

  function textileDisabled(state) {
	logger.error(`>>> Entering textileDisabled(state=${state})`, { "event.action": "textileDisabled", "logrite.state": state });
    var type = state.layoutType;

    switch(type) {
    case "notextile":
    case "code":
    case "pre":
      return TOKEN_STYLES[type];
    default:
      if (state.notextile)
        return TOKEN_STYLES.notextile + (type ? (" " + TOKEN_STYLES[type]) : "");
      return null;
    }
  }

  function tokenStylesWith(state, extraStyles) {
	logger.error(`>>> Entering tokenStylesWith(state=${state},extraStyles=${extraStyles})`, { "event.action": "tokenStylesWith", "logrite.state": state, "logrite.extraStyles": extraStyles });
    var disabled = textileDisabled(state);
    if (disabled) return disabled;
	logger.error(`tokenStylesWith(state=${state},extraStyles=${extraStyles}): disabled → ${disabled}`, { "event.action": "tokenStylesWith", "logrite.state": state, "logrite.extraStyles": extraStyles });

    var type = tokenStyles(state);
	logger.error(`tokenStylesWith(state=${state},extraStyles=${extraStyles}): type → ${type}`, { "event.action": "tokenStylesWith", "logrite.state": state, "logrite.extraStyles": extraStyles });
    if (extraStyles)
      return type ? (type + " " + extraStyles) : extraStyles;
    else
      return type;
  }

  function activeStyles(state) {
	logger.error(`>>> Entering activeStyles(state=${state})`, { "event.action": "activeStyles", "logrite.state": state });
    var styles = [];
    for (var i = 1; i < arguments.length; ++i) {
      if (state[arguments[i]])
        styles.push(TOKEN_STYLES[arguments[i]]);
    }
	logger.warn(`<<< Exiting activeStyles(state=${state})`, { "event.action": "activeStyles", "logrite.state": state });
    return styles;
  }

  function blankLine(state) {
	logger.error(`>>> Entering blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
    var spanningLayout = state.spanningLayout, type = state.layoutType;

    for (var key in state) if (state.hasOwnProperty(key))
      delete state[key];

    state.mode = Modes.newLayout;
	logger.debug(`blankLine(state=${state}): state.mode → ${state.mode}`, { "event.action": "blankLine", "logrite.state": state });
    if (spanningLayout) {
      state.layoutType = type;
	logger.debug(`blankLine(state=${state}): state.layoutType → ${state.layoutType}`, { "event.action": "blankLine", "logrite.state": state });
      state.spanningLayout = true;
	logger.warn(`<<< Exiting blankLine(state=${state})`, { "event.action": "blankLine", "logrite.state": state });
    }
  }

  var REs = {
    cache: {},
    single: {
      bc: "bc",
      bq: "bq",
      definitionList: /- [^(?::=)]+:=+/,
      definitionListEnd: /.*=:\s*$/,
      div: "div",
      drawTable: /\|.*\|/,
      foot: /fn\d+/,
      header: /h[1-6]/,
      html: /\s*<(?:\/)?(\w+)(?:[^>]+)?>(?:[^<]+<\/\1>)?/,
      link: /[^"]+":\S/,
      linkDefinition: /\[[^\s\]]+\]\S+/,
      list: /(?:#+|\*+)/,
      notextile: "notextile",
      para: "p",
      pre: "pre",
      table: "table",
      tableCellAttributes: /[\/\\]\d+/,
      tableHeading: /\|_\./,
      tableText: /[^"_\*\[\(\?\+~\^%@|-]+/,
      text: /[^!"_=\*\[\(<\?\+~\^%@-]+/
    },
    attributes: {
      align: /(?:<>|<|>|=)/,
      selector: /\([^\(][^\)]+\)/,
      lang: /\[[^\[\]]+\]/,
      pad: /(?:\(+|\)+){1,2}/,
      css: /\{[^\}]+\}/
    },
    createRe: function(name) {
	logger.error(`>>> Entering createRe(name=${name})`, { "event.action": "createRe", "logrite.name": name });
      switch (name) {
      case "drawTable":
        return REs.makeRe("^", REs.single.drawTable, "$");
      case "html":
        return REs.makeRe("^", REs.single.html, "(?:", REs.single.html, ")*", "$");
      case "linkDefinition":
        return REs.makeRe("^", REs.single.linkDefinition, "$");
      case "listLayout":
        return REs.makeRe("^", REs.single.list, RE("allAttributes"), "*\\s+");
      case "tableCellAttributes":
        return REs.makeRe("^", REs.choiceRe(REs.single.tableCellAttributes,
                                            RE("allAttributes")), "+\\.");
      case "type":
        return REs.makeRe("^", RE("allTypes"));
      case "typeLayout":
        return REs.makeRe("^", RE("allTypes"), RE("allAttributes"),
                          "*\\.\\.?", "(\\s+|$)");
      case "attributes":
        return REs.makeRe("^", RE("allAttributes"), "+");

      case "allTypes":
        return REs.choiceRe(REs.single.div, REs.single.foot,
                            REs.single.header, REs.single.bc, REs.single.bq,
                            REs.single.notextile, REs.single.pre, REs.single.table,
                            REs.single.para);

      case "allAttributes":
        return REs.choiceRe(REs.attributes.selector, REs.attributes.css,
                            REs.attributes.lang, REs.attributes.align, REs.attributes.pad);

      default:
	logger.warn(`<<< Exiting createRe(name=${name})`, { "event.action": "createRe", "logrite.name": name });
        return REs.makeRe("^", REs.single[name]);
      }
    },
    makeRe: function() {
	logger.error('>>> Entering makeRe()', { "event.action": "makeRe" });
      var pattern = "";
      for (var i = 0; i < arguments.length; ++i) {
		logger.error(`makeRe(): pattern → ${pattern}`, { "event.action": "makeRe" });
        var arg = arguments[i];
        pattern += (typeof arg === "string") ? arg : arg.source;
		logger.warn('<<< Exiting makeRe()', { "event.action": "makeRe" });
      }
      return new RegExp(pattern);
    },
    choiceRe: function() {
	logger.error('>>> Entering choiceRe()', { "event.action": "choiceRe" });
      var parts = [arguments[0]];
      for (var i = 1; i < arguments.length; ++i) {
        parts[i * 2 - 1] = "|";
        parts[i * 2] = arguments[i];
      }

      parts.unshift("(?:");
      parts.push(")");
	logger.warn('<<< Exiting choiceRe()', { "event.action": "choiceRe" });
      return REs.makeRe.apply(null, parts);
    }
  };

  function RE(name) {
	logger.error(`>>> Entering RE(name=${name})`, { "event.action": "RE", "logrite.name": name });
	logger.warn(`<<< Exiting RE(name=${name})`, { "event.action": "RE", "logrite.name": name });
    return (REs.cache[name] || (REs.cache[name] = REs.createRe(name)));
  }

  var Modes = {
    newLayout: function(stream, state) {
	logger.error(`>>> Entering newLayout(stream=${stream},state=${state})`, { "event.action": "newLayout", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(RE("typeLayout"), false)) {
        state.spanningLayout = false;
		logger.debug(`newLayout(stream=${stream},state=${state}): state.spanningLayout → ${state.spanningLayout}`, { "event.action": "newLayout", "logrite.stream": stream, "logrite.state": state });
        return (state.mode = Modes.blockType)(stream, state);
      }
      var newMode;
      if (!textileDisabled(state)) {
        if (stream.match(RE("listLayout"), false))
          newMode = Modes.list;
        else if (stream.match(RE("drawTable"), false))
          newMode = Modes.table;
        else if (stream.match(RE("linkDefinition"), false))
          newMode = Modes.linkDefinition;
        else if (stream.match(RE("definitionList")))
          newMode = Modes.definitionList;
        else if (stream.match(RE("html"), false))
          newMode = Modes.html;
		logger.debug(`newLayout(stream=${stream},state=${state}): newMode → ${newMode}`, { "event.action": "newLayout", "logrite.stream": stream, "logrite.state": state });
      }
      return (state.mode = (newMode || Modes.text))(stream, state);
    },

    blockType: function(stream, state) {
	logger.error(`>>> Entering blockType(stream=${stream},state=${state})`, { "event.action": "blockType", "logrite.stream": stream, "logrite.state": state });
      var match, type;
      state.layoutType = null;
	logger.debug(`blockType(stream=${stream},state=${state}): state.layoutType → ${state.layoutType}`, { "event.action": "blockType", "logrite.stream": stream, "logrite.state": state });

      if (match = stream.match(RE("type")))
        type = match[0];
      else
        return (state.mode = Modes.text)(stream, state);

      if (match = type.match(RE("header"))) {
        state.layoutType = "header";
        state.header = parseInt(match[0][1]);
      } else if (type.match(RE("bq"))) {
        state.layoutType = "quote";
      } else if (type.match(RE("bc"))) {
        state.layoutType = "code";
      } else if (type.match(RE("foot"))) {
        state.layoutType = "footnote";
      } else if (type.match(RE("notextile"))) {
        state.layoutType = "notextile";
      } else if (type.match(RE("pre"))) {
        state.layoutType = "pre";
      } else if (type.match(RE("div"))) {
        state.layoutType = "div";
      } else if (type.match(RE("table"))) {
        state.layoutType = "table";
      }

      state.mode = Modes.attributes;
	logger.warn(`<<< Exiting blockType(stream=${stream},state=${state})`, { "event.action": "blockType", "logrite.stream": stream, "logrite.state": state });
      return tokenStyles(state);
    },

    text: function(stream, state) {
	logger.error(`>>> Entering text(stream=${stream},state=${state})`, { "event.action": "text", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(RE("text"))) return tokenStyles(state);

      var ch = stream.next();
      if (ch === '"')
        return (state.mode = Modes.link)(stream, state);
      return handlePhraseModifier(stream, state, ch);
    },

    attributes: function(stream, state) {
	logger.error(`>>> Entering attributes(stream=${stream},state=${state})`, { "event.action": "attributes", "logrite.stream": stream, "logrite.state": state });
      state.mode = Modes.layoutLength;

	logger.debug(`attributes(stream=${stream},state=${state}): state.mode → ${state.mode}`, { "event.action": "attributes", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(RE("attributes")))
        return tokenStylesWith(state, TOKEN_STYLES.attributes);
      else
        return tokenStyles(state);
    },

    layoutLength: function(stream, state) {
	logger.error(`>>> Entering layoutLength(stream=${stream},state=${state})`, { "event.action": "layoutLength", "logrite.stream": stream, "logrite.state": state });
      if (stream.eat(".") && stream.eat("."))
        state.spanningLayout = true;

	logger.debug(`layoutLength(stream=${stream},state=${state}): state.spanningLayout → ${state.spanningLayout}`, { "event.action": "layoutLength", "logrite.stream": stream, "logrite.state": state });
      state.mode = Modes.text;
	logger.warn(`<<< Exiting layoutLength(stream=${stream},state=${state})`, { "event.action": "layoutLength", "logrite.stream": stream, "logrite.state": state });
      return tokenStyles(state);
    },

    list: function(stream, state) {
	logger.error(`>>> Entering list(stream=${stream},state=${state})`, { "event.action": "list", "logrite.stream": stream, "logrite.state": state });
      var match = stream.match(RE("list"));
      state.listDepth = match[0].length;
	logger.error(`list(stream=${stream},state=${state}): match → ${match}`, { "event.action": "list", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`list(stream=${stream},state=${state}): state.listDepth → ${state.listDepth}`, { "event.action": "list", "logrite.stream": stream, "logrite.state": state });
      var listMod = (state.listDepth - 1) % 3;
	logger.error(`list(stream=${stream},state=${state}): listMod → ${listMod}`, { "event.action": "list", "logrite.stream": stream, "logrite.state": state });
      if (!listMod)
        state.layoutType = "list1";
      else if (listMod === 1)
        state.layoutType = "list2";
      else
        state.layoutType = "list3";

      state.mode = Modes.attributes;
	logger.warn(`<<< Exiting list(stream=${stream},state=${state})`, { "event.action": "list", "logrite.stream": stream, "logrite.state": state });
      return tokenStyles(state);
    },

    link: function(stream, state) {
	logger.error(`>>> Entering link(stream=${stream},state=${state})`, { "event.action": "link", "logrite.stream": stream, "logrite.state": state });
      state.mode = Modes.text;
      if (stream.match(RE("link"))) {
		logger.debug(`link(stream=${stream},state=${state}): state.mode → ${state.mode}`, { "event.action": "link", "logrite.stream": stream, "logrite.state": state });
        stream.match(/\S+/);
		logger.warn(`<<< Exiting link(stream=${stream},state=${state})`, { "event.action": "link", "logrite.stream": stream, "logrite.state": state });
        return tokenStylesWith(state, TOKEN_STYLES.link);
      }
      return tokenStyles(state);
    },

    linkDefinition: function(stream, state) {
	logger.error(`>>> Entering linkDefinition(stream=${stream},state=${state})`, { "event.action": "linkDefinition", "logrite.stream": stream, "logrite.state": state });
      stream.skipToEnd();
	logger.warn(`<<< Exiting linkDefinition(stream=${stream},state=${state})`, { "event.action": "linkDefinition", "logrite.stream": stream, "logrite.state": state });
      return tokenStylesWith(state, TOKEN_STYLES.linkDefinition);
    },

    definitionList: function(stream, state) {
	logger.error(`>>> Entering definitionList(stream=${stream},state=${state})`, { "event.action": "definitionList", "logrite.stream": stream, "logrite.state": state });
      stream.match(RE("definitionList"));

      state.layoutType = "definitionList";
	logger.debug(`definitionList(stream=${stream},state=${state}): state.layoutType → ${state.layoutType}`, { "event.action": "definitionList", "logrite.stream": stream, "logrite.state": state });

      if (stream.match(/\s*$/))
        state.spanningLayout = true;
      else
        state.mode = Modes.attributes;

	logger.warn(`<<< Exiting definitionList(stream=${stream},state=${state})`, { "event.action": "definitionList", "logrite.stream": stream, "logrite.state": state });
	logger.debug(`definitionList(stream=${stream},state=${state}): state.spanningLayout → ${state.spanningLayout}`, { "event.action": "definitionList", "logrite.stream": stream, "logrite.state": state });
      return tokenStyles(state);
    },

    html: function(stream, state) {
	logger.error(`>>> Entering html(stream=${stream},state=${state})`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
      stream.skipToEnd();
	logger.warn(`<<< Exiting html(stream=${stream},state=${state})`, { "event.action": "html", "logrite.stream": stream, "logrite.state": state });
      return tokenStylesWith(state, TOKEN_STYLES.html);
    },

    table: function(stream, state) {
	logger.error(`>>> Entering table(stream=${stream},state=${state})`, { "event.action": "table", "logrite.stream": stream, "logrite.state": state });
      state.layoutType = "table";
	logger.warn(`<<< Exiting table(stream=${stream},state=${state})`, { "event.action": "table", "logrite.stream": stream, "logrite.state": state });
      return (state.mode = Modes.tableCell)(stream, state);
    },

    tableCell: function(stream, state) {
	logger.error(`>>> Entering tableCell(stream=${stream},state=${state})`, { "event.action": "tableCell", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(RE("tableHeading")))
        state.tableHeading = true;
      else
        stream.eat("|");

	logger.debug(`tableCell(stream=${stream},state=${state}): state.tableHeading → ${state.tableHeading}`, { "event.action": "tableCell", "logrite.stream": stream, "logrite.state": state });
      state.mode = Modes.tableCellAttributes;
	logger.warn(`<<< Exiting tableCell(stream=${stream},state=${state})`, { "event.action": "tableCell", "logrite.stream": stream, "logrite.state": state });
      return tokenStyles(state);
    },

    tableCellAttributes: function(stream, state) {
	logger.error(`>>> Entering tableCellAttributes(stream=${stream},state=${state})`, { "event.action": "tableCellAttributes", "logrite.stream": stream, "logrite.state": state });
      state.mode = Modes.tableText;

	logger.debug(`tableCellAttributes(stream=${stream},state=${state}): state.mode → ${state.mode}`, { "event.action": "tableCellAttributes", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(RE("tableCellAttributes")))
        return tokenStylesWith(state, TOKEN_STYLES.attributes);
      else
        return tokenStyles(state);
    },

    tableText: function(stream, state) {
	logger.error(`>>> Entering tableText(stream=${stream},state=${state})`, { "event.action": "tableText", "logrite.stream": stream, "logrite.state": state });
      if (stream.match(RE("tableText")))
        return tokenStyles(state);

      if (stream.peek() === "|") { // end of cell
        state.mode = Modes.tableCell;
		logger.warn(`<<< Exiting tableText(stream=${stream},state=${state})`, { "event.action": "tableText", "logrite.stream": stream, "logrite.state": state });
        return tokenStyles(state);
      }
      return handlePhraseModifier(stream, state, stream.next());
    }
  };

  CodeMirror.defineMode("textile", function() {
    return {
      startState: function() {
		logger.error('>>> Entering startState()', { "event.action": "startState" });
		logger.warn('<<< Exiting startState()', { "event.action": "startState" });
        return { mode: Modes.newLayout };
      },
      token: function(stream, state) {
		logger.error(`>>> Entering token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        if (stream.sol()) startNewLine(stream, state);
		logger.warn(`<<< Exiting token(stream=${stream},state=${state})`, { "event.action": "token", "logrite.stream": stream, "logrite.state": state });
        return state.mode(stream, state);
      },
      blankLine: blankLine
    };
  });

  CodeMirror.defineMIME("text/x-textile", "textile");
});
