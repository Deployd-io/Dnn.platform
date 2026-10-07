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

  function doFold(cm, pos, options, force) {
	logger.error(`>>> Entering doFold(cm=${cm},pos=${pos},options=${options},force=${force})`, { "event.action": "doFold", "logrite.cm": cm, "logrite.pos": pos, "logrite.options": options, "logrite.force": force });
    if (options && options.call) {
      var finder = options;
      options = null;
    } else {
      var finder = getOption(cm, options, "rangeFinder");
	logger.error(`doFold(cm=${cm},pos=${pos},options=${options},force=${force}): finder → ${finder}`, { "event.action": "doFold", "logrite.cm": cm, "logrite.pos": pos, "logrite.options": options, "logrite.force": force });
    }
    if (typeof pos == "number") pos = CodeMirror.Pos(pos, 0);
    var minSize = getOption(cm, options, "minFoldSize");
	logger.error(`doFold(cm=${cm},pos=${pos},options=${options},force=${force}): minSize → ${minSize}`, { "event.action": "doFold", "logrite.cm": cm, "logrite.pos": pos, "logrite.options": options, "logrite.force": force });

    function getRange(allowFolded) {
	logger.error(`>>> Entering getRange(allowFolded=${allowFolded})`, { "event.action": "getRange", "logrite.allowFolded": allowFolded });
      var range = finder(cm, pos);
      if (!range || range.to.line - range.from.line < minSize) return null;
	logger.error(`getRange(allowFolded=${allowFolded}): range → ${range}`, { "event.action": "getRange", "logrite.allowFolded": allowFolded });
	logger.debug(`getRange(allowFolded=${allowFolded}): range.cleared → ${range.cleared}`, { "event.action": "getRange", "logrite.allowFolded": allowFolded });
      var marks = cm.findMarksAt(range.from);
      for (var i = 0; i < marks.length; ++i) {
        if (marks[i].__isFold && force !== "fold") {
          if (!allowFolded) return null;
          range.cleared = true;
          marks[i].clear();
        }
      }
	logger.error(`getRange(allowFolded=${allowFolded}): marks → ${marks}`, { "event.action": "getRange", "logrite.allowFolded": allowFolded });
      return range;
    }

    var range = getRange(true);
    if (getOption(cm, options, "scanUp")) while (!range && pos.line > cm.firstLine()) {
      pos = CodeMirror.Pos(pos.line - 1, 0);
      range = getRange(false);
    }
    if (!range || range.cleared || force === "unfold") return;

    var myWidget = makeWidget(cm, options);
    CodeMirror.on(myWidget, "mousedown", function(e) {
      myRange.clear();
      CodeMirror.e_preventDefault(e);
    });
    var myRange = cm.markText(range.from, range.to, {
      replacedWith: myWidget,
      clearOnEnter: true,
      __isFold: true
    });
    myRange.on("clear", function(from, to) {
      CodeMirror.signal(cm, "unfold", cm, from, to);
    });
    CodeMirror.signal(cm, "fold", cm, range.from, range.to);
	logger.warn(`<<< Exiting doFold(cm=${cm},pos=${pos},options=${options},force=${force})`, { "event.action": "doFold", "logrite.cm": cm, "logrite.pos": pos, "logrite.options": options, "logrite.force": force });
  }

  function makeWidget(cm, options) {
	logger.error(`>>> Entering makeWidget(cm=${cm},options=${options})`, { "event.action": "makeWidget", "logrite.cm": cm, "logrite.options": options });
    var widget = getOption(cm, options, "widget");
    if (typeof widget == "string") {
	logger.error(`makeWidget(cm=${cm},options=${options}): widget → ${widget}`, { "event.action": "makeWidget", "logrite.cm": cm, "logrite.options": options });
	logger.debug(`makeWidget(cm=${cm},options=${options}): widget.className → ${widget.className}`, { "event.action": "makeWidget", "logrite.cm": cm, "logrite.options": options });
      var text = document.createTextNode(widget);
	logger.error(`makeWidget(cm=${cm},options=${options}): text → ${text}`, { "event.action": "makeWidget", "logrite.cm": cm, "logrite.options": options });
      widget = document.createElement("span");
      widget.appendChild(text);
      widget.className = "CodeMirror-foldmarker";
    }
	logger.warn(`<<< Exiting makeWidget(cm=${cm},options=${options})`, { "event.action": "makeWidget", "logrite.cm": cm, "logrite.options": options });
    return widget;
  }

  // Clumsy backwards-compatible interface
  CodeMirror.newFoldFunction = function(rangeFinder, widget) {
	logger.error(`>>> Entering newFoldFunction(rangeFinder=${rangeFinder},widget=${widget})`, { "event.action": "newFoldFunction", "logrite.rangeFinder": rangeFinder, "logrite.widget": widget });
    return function(cm, pos) { doFold(cm, pos, {rangeFinder: rangeFinder, widget: widget}); };
  };

  // New-style interface
  CodeMirror.defineExtension("foldCode", function(pos, options, force) {
    doFold(this, pos, options, force);
  });

  CodeMirror.defineExtension("isFolded", function(pos) {
    var marks = this.findMarksAt(pos);
    for (var i = 0; i < marks.length; ++i)
      if (marks[i].__isFold) return true;
  });

  CodeMirror.commands.toggleFold = function(cm) {
	logger.error(`>>> Entering toggleFold(cm=${cm})`, { "event.action": "toggleFold", "logrite.cm": cm });
    cm.foldCode(cm.getCursor());
	logger.warn(`<<< Exiting toggleFold(cm=${cm})`, { "event.action": "toggleFold", "logrite.cm": cm });
  };
  CodeMirror.commands.fold = function(cm) {
	logger.error(`>>> Entering fold(cm=${cm})`, { "event.action": "fold", "logrite.cm": cm });
    cm.foldCode(cm.getCursor(), null, "fold");
	logger.warn(`<<< Exiting fold(cm=${cm})`, { "event.action": "fold", "logrite.cm": cm });
  };
  CodeMirror.commands.unfold = function(cm) {
	logger.error(`>>> Entering unfold(cm=${cm})`, { "event.action": "unfold", "logrite.cm": cm });
    cm.foldCode(cm.getCursor(), null, "unfold");
	logger.warn(`<<< Exiting unfold(cm=${cm})`, { "event.action": "unfold", "logrite.cm": cm });
  };
  CodeMirror.commands.foldAll = function(cm) {
	logger.error(`>>> Entering foldAll(cm=${cm})`, { "event.action": "foldAll", "logrite.cm": cm });
    cm.operation(function() {
      for (var i = cm.firstLine(), e = cm.lastLine(); i <= e; i++)
        cm.foldCode(CodeMirror.Pos(i, 0), null, "fold");
    });
  };
  CodeMirror.commands.unfoldAll = function(cm) {
	logger.error(`>>> Entering unfoldAll(cm=${cm})`, { "event.action": "unfoldAll", "logrite.cm": cm });
    cm.operation(function() {
      for (var i = cm.firstLine(), e = cm.lastLine(); i <= e; i++)
        cm.foldCode(CodeMirror.Pos(i, 0), null, "unfold");
    });
  };

  CodeMirror.registerHelper("fold", "combine", function() {
    var funcs = Array.prototype.slice.call(arguments, 0);
    return function(cm, start) {
      for (var i = 0; i < funcs.length; ++i) {
        var found = funcs[i](cm, start);
        if (found) return found;
      }
    };
  });

  CodeMirror.registerHelper("fold", "auto", function(cm, start) {
    var helpers = cm.getHelpers(start, "fold");
    for (var i = 0; i < helpers.length; i++) {
      var cur = helpers[i](cm, start);
      if (cur) return cur;
    }
  });

  var defaultOptions = {
    rangeFinder: CodeMirror.fold.auto,
    widget: "\u2194",
    minFoldSize: 0,
    scanUp: false
  };

  CodeMirror.defineOption("foldOptions", null);

  function getOption(cm, options, name) {
	logger.error(`>>> Entering getOption(cm=${cm},options=${options},name=${name})`, { "event.action": "getOption", "logrite.cm": cm, "logrite.options": options, "logrite.name": name });
    if (options && options[name] !== undefined)
      return options[name];
    var editorOptions = cm.options.foldOptions;
    if (editorOptions && editorOptions[name] !== undefined)
      return editorOptions[name];
    return defaultOptions[name];
  }

  CodeMirror.defineExtension("foldOption", function(options, name) {
    return getOption(this, options, name);
  });
});
