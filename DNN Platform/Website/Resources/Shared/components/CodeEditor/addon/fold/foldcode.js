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

  function doFold(cm, pos, options, force) {
	logger.debug(`>>> Entering doFold(cm=${cm},pos=${pos},options=${options},force=${force})`);
    if (options && options.call) {
      var finder = options;
      options = null;
    } else {
      var finder = getOption(cm, options, "rangeFinder");
	logger.debug(`doFold(cm=${cm},pos=${pos},options=${options},force=${force}): finder → ${finder}`);
    }
    if (typeof pos == "number") pos = CodeMirror.Pos(pos, 0);
    var minSize = getOption(cm, options, "minFoldSize");
	logger.debug(`doFold(cm=${cm},pos=${pos},options=${options},force=${force}): minSize → ${minSize}`);

    function getRange(allowFolded) {
	logger.debug(`>>> Entering getRange(allowFolded=${allowFolded})`);
      var range = finder(cm, pos);
      if (!range || range.to.line - range.from.line < minSize) return null;
	logger.debug(`getRange(allowFolded=${allowFolded}): range → ${range}`);
      var marks = cm.findMarksAt(range.from);
      for (var i = 0; i < marks.length; ++i) {
        if (marks[i].__isFold && force !== "fold") {
          if (!allowFolded) return null;
          range.cleared = true;
		logger.debug(`getRange(allowFolded=${allowFolded}): range.cleared → ${range.cleared}`);
          marks[i].clear();
        }
      }
	logger.debug(`getRange(allowFolded=${allowFolded}): marks → ${marks}`);
	logger.debug(`<<< Exiting getRange(allowFolded=${allowFolded})`);
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
	logger.debug(`>>> Entering on(myWidget=${myWidget})`);
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
	logger.debug('<<< Exiting on()');
	logger.debug(`<<< Exiting doFold(cm=${cm},pos=${pos},options=${options},force=${force})`);
  }

  function makeWidget(cm, options) {
    var widget = getOption(cm, options, "widget");
	logger.debug(`>>> Entering makeWidget(cm=${cm},options=${options})`);
    if (typeof widget == "string") {
      var text = document.createTextNode(widget);
      widget = document.createElement("span");
	logger.debug(`makeWidget(cm=${cm},options=${options}): widget → ${widget}`);
      widget.appendChild(text);
	logger.debug(`makeWidget(cm=${cm},options=${options}): text → ${text}`);
      widget.className = "CodeMirror-foldmarker";
    }
	logger.debug(`makeWidget(cm=${cm},options=${options}): widget.className → ${widget.className}`);
	logger.debug(`<<< Exiting makeWidget(cm=${cm},options=${options})`);
    return widget;
  }

  // Clumsy backwards-compatible interface
  CodeMirror.newFoldFunction = function(rangeFinder, widget) {
	logger.debug(`<<< Exiting function(rangeFinder=${rangeFinder},widget=${widget})`);
    return function(cm, pos) { doFold(cm, pos, {rangeFinder: rangeFinder, widget: widget}); };
  };

  // New-style interface
  CodeMirror.defineExtension("foldCode", function(pos, options, force) {
	logger.debug('>>> Entering defineExtension()');
	logger.debug('<<< Exiting defineExtension()');
    doFold(this, pos, options, force);
  });

  CodeMirror.defineExtension("isFolded", function(pos) {
    var marks = this.findMarksAt(pos);
    for (var i = 0; i < marks.length; ++i)
      if (marks[i].__isFold) return true;
  });

  CodeMirror.commands.toggleFold = function(cm) {
    cm.foldCode(cm.getCursor());
  };
  CodeMirror.commands.fold = function(cm) {
    cm.foldCode(cm.getCursor(), null, "fold");
  };
  CodeMirror.commands.unfold = function(cm) {
    cm.foldCode(cm.getCursor(), null, "unfold");
  };
  CodeMirror.commands.foldAll = function(cm) {
    cm.operation(function() {
	logger.debug(`>>> Entering operation(i=${i})`);
      for (var i = cm.firstLine(), e = cm.lastLine(); i <= e; i++)
        cm.foldCode(CodeMirror.Pos(i, 0), null, "fold");
    });
	logger.debug(`<<< Exiting operation(i=${i})`);
  };
  CodeMirror.commands.unfoldAll = function(cm) {
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
	logger.debug(`<<< Exiting registerHelper(funcs=${funcs})`);
    };
  });

  CodeMirror.registerHelper("fold", "auto", function(cm, start) {
    var helpers = cm.getHelpers(start, "fold");
	logger.debug(`registerHelper(helpers=${helpers}): helpers → ${helpers}`);
    for (var i = 0; i < helpers.length; i++) {
      var cur = helpers[i](cm, start);
      if (cur) return cur;
	logger.debug(`registerHelper(helpers=${helpers}): cur → ${cur}`);
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
	logger.debug(`>>> Entering getOption(cm=${cm},options=${options},name=${name})`);
    if (options && options[name] !== undefined)
      return options[name];
    var editorOptions = cm.options.foldOptions;
    if (editorOptions && editorOptions[name] !== undefined)
	logger.debug(`<<< Exiting getOption(cm=${cm},options=${options},name=${name})`);
      return editorOptions[name];
    return defaultOptions[name];
  }

  CodeMirror.defineExtension("foldOption", function(options, name) {
    return getOption(this, options, name);
  });
});
