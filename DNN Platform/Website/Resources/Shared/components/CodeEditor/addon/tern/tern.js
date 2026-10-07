const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Glue code between CodeMirror and Tern.
//
// Create a CodeMirror.TernServer to wrap an actual Tern server,
// register open documents (CodeMirror.Doc instances) with it, and
// call its methods to activate the assisting functions that Tern
// provides.
//
// Options supported (all optional):
// * defs: An array of JSON definition data structures.
// * plugins: An object mapping plugin names to configuration
//   options.
// * getFile: A function(name, c) that can be used to access files in
//   the project that haven't been loaded yet. Simply do c(null) to
//   indicate that a file is not available.
// * fileFilter: A function(value, docName, doc) that will be applied
//   to documents before passing them on to Tern.
// * switchToDoc: A function(name, doc) that should, when providing a
//   multi-file view, switch the view or focus to the named file.
// * showError: A function(editor, message) that can be used to
//   override the way errors are displayed.
// * completionTip: Customize the content in tooltips for completions.
//   Is passed a single argument—the completion's data as returned by
//   Tern—and may return a string, DOM node, or null to indicate that
//   no tip should be shown. By default the docstring is shown.
// * typeTip: Like completionTip, but for the tooltips shown for type
//   queries.
// * responseFilter: A function(doc, query, request, error, data) that
//   will be applied to the Tern responses before treating them
//
//
// It is possible to run the Tern server in a web worker by specifying
// these additional options:
// * useWorker: Set to true to enable web worker mode. You'll probably
//   want to feature detect the actual value you use here, for example
//   !!window.Worker.
// * workerScript: The main script of the worker. Point this to
//   wherever you are hosting worker.js from this directory.
// * workerDeps: An array of paths pointing (relative to workerScript)
//   to the Acorn and Tern libraries and any Tern plugins you want to
//   load. Or, if you minified those into a single script and included
//   them in the workerScript, simply leave this undefined.

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";
  // declare global: tern

  CodeMirror.TernServer = function(options) {
	logger.error(`>>> Entering TernServer(options=${options})`, { "event.action": "TernServer", "logrite.options": options });
    var self = this;
    this.options = options || {};
    var plugins = this.options.plugins || (this.options.plugins = {});
	logger.error(`TernServer(options=${options}): plugins → ${plugins}`, { "event.action": "TernServer", "logrite.options": options });
	logger.debug(`TernServer(options=${options}): options → ${options}`, { "event.action": "TernServer", "logrite.options": options });
	logger.debug(`TernServer(options=${options}): this.options → ${this.options}`, { "event.action": "TernServer", "logrite.options": options });
    if (!plugins.doc_comment) plugins.doc_comment = true;
    this.docs = Object.create(null);
    if (this.options.useWorker) {
      this.server = new WorkerServer(this);
    } else {
      this.server = new tern.Server({
        getFile: function(name, c) {
            logger.error(`>>> Entering getFile(name=${name},c=${c})`, { "event.action": "getFile", "logrite.ts": ts, "logrite.name": name, "logrite.c": c });
            logger.warn(`<<< Exiting getFile(name=${name},c=${c})`, { "event.action": "getFile", "logrite.ts": ts, "logrite.name": name, "logrite.c": c });
            return getFile(self, name, c);
        },
        async: true,
        defs: this.options.defs || [],
        plugins: plugins
      });
    }
    this.trackChange = function(doc, change) { trackChange(self, doc, change); };
	logger.error(`>>> Entering trackChange(doc=${doc},change=${change})`, { "event.action": "trackChange", "logrite.ts": ts, "logrite.doc": doc, "logrite.change": change });

	logger.warn(`<<< Exiting trackChange(doc=${doc},change=${change})`, { "event.action": "trackChange", "logrite.ts": ts, "logrite.doc": doc, "logrite.change": change });
    this.cachedArgHints = null;
    this.activeArgHints = null;
	logger.debug(`TernServer(options=${options}): this.cachedArgHints → ${this.cachedArgHints}`, { "event.action": "TernServer", "logrite.options": options });
    this.jumpStack = [];

	logger.debug(`TernServer(options=${options}): this.activeArgHints → ${this.activeArgHints}`, { "event.action": "TernServer", "logrite.options": options });
    this.getHint = function(cm, c) { return hint(self, cm, c); };
	logger.error(`>>> Entering getHint(cm=${cm},c=${c})`, { "event.action": "getHint", "logrite.cm": cm, "logrite.c": c });
    this.getHint.async = true;
	logger.warn(`<<< Exiting getHint(cm=${cm},c=${c})`, { "event.action": "getHint", "logrite.cm": cm, "logrite.c": c });
  };

  CodeMirror.TernServer.prototype = {
    addDoc: function(name, doc) {
      var data = {doc: doc, name: name, changed: null};
      this.server.addFile(name, docValue(this, data));
      CodeMirror.on(doc, "change", this.trackChange);
	logger.warn(`<<< Exiting addDoc(name=${name},doc=${doc})`, { "event.action": "addDoc", "logrite.name": name, "logrite.doc": doc });
      return this.docs[name] = data;
    },

    delDoc: function(id) {
	logger.error(`>>> Entering delDoc(id=${id})`, { "event.action": "delDoc", "logrite.id": id });
      var found = resolveDoc(this, id);
      if (!found) return;
	logger.error(`delDoc(id=${id}): found → ${found}`, { "event.action": "delDoc", "logrite.id": id });
      CodeMirror.off(found.doc, "change", this.trackChange);
      delete this.docs[found.name];
      this.server.delFile(found.name);
	logger.warn(`<<< Exiting delDoc(id=${id})`, { "event.action": "delDoc", "logrite.id": id });
    },

    hideDoc: function(id) {
	logger.error(`>>> Entering hideDoc(id=${id})`, { "event.action": "hideDoc", "logrite.id": id });
      closeArgHints(this);
      var found = resolveDoc(this, id);
      if (found && found.changed) sendDoc(this, found);
	logger.error(`hideDoc(id=${id}): found → ${found}`, { "event.action": "hideDoc", "logrite.id": id });
    },

    complete: function(cm) {
	logger.error(`>>> Entering complete(cm=${cm})`, { "event.action": "complete", "logrite.cm": cm });
      cm.showHint({hint: this.getHint});
	logger.warn(`<<< Exiting complete(cm=${cm})`, { "event.action": "complete", "logrite.cm": cm });
    },

    showType: function(cm, pos, c) {
        logger.error(`>>> Entering showType(cm=${cm},pos=${pos},c=${c})`, { "event.action": "showType", "logrite.cm": cm, "logrite.pos": pos, "logrite.c": c });
        logger.warn(`<<< Exiting showType(cm=${cm},pos=${pos},c=${c})`, { "event.action": "showType", "logrite.cm": cm, "logrite.pos": pos, "logrite.c": c });
        showContextInfo(this, cm, pos, "type", c);
    },

    showDocs: function(cm, pos, c) {
        logger.error(`>>> Entering showDocs(cm=${cm},pos=${pos},c=${c})`, { "event.action": "showDocs", "logrite.cm": cm, "logrite.pos": pos, "logrite.c": c });
        logger.warn(`<<< Exiting showDocs(cm=${cm},pos=${pos},c=${c})`, { "event.action": "showDocs", "logrite.cm": cm, "logrite.pos": pos, "logrite.c": c });
        showContextInfo(this, cm, pos, "documentation", c);
    },

    updateArgHints: function(cm) {
        logger.error(`>>> Entering updateArgHints(cm=${cm})`, { "event.action": "updateArgHints", "logrite.ts": ts, "logrite.cm": cm });
        logger.warn(`<<< Exiting updateArgHints(cm=${cm})`, { "event.action": "updateArgHints", "logrite.ts": ts, "logrite.cm": cm });
        updateArgHints(this, cm);
    },

    jumpToDef: function(cm) {
        logger.error(`>>> Entering jumpToDef(cm=${cm})`, { "event.action": "jumpToDef", "logrite.cm": cm });
        logger.warn(`<<< Exiting jumpToDef(cm=${cm})`, { "event.action": "jumpToDef", "logrite.cm": cm });
        jumpToDef(this, cm);
    },

    jumpBack: function(cm) {
        logger.error(`>>> Entering jumpBack(cm=${cm})`, { "event.action": "jumpBack", "logrite.ts": ts, "logrite.cm": cm });
        logger.warn(`<<< Exiting jumpBack(cm=${cm})`, { "event.action": "jumpBack", "logrite.ts": ts, "logrite.cm": cm });
        jumpBack(this, cm);
    },

    rename: function(cm) {
        logger.error(`>>> Entering rename(cm=${cm})`, { "event.action": "rename", "logrite.ts": ts, "logrite.cm": cm });
        logger.warn(`<<< Exiting rename(cm=${cm})`, { "event.action": "rename", "logrite.ts": ts, "logrite.cm": cm });
        rename(this, cm);
    },

    selectName: function(cm) {
        logger.error(`>>> Entering selectName(cm=${cm})`, { "event.action": "selectName", "logrite.ts": ts, "logrite.cm": cm });
        logger.warn(`<<< Exiting selectName(cm=${cm})`, { "event.action": "selectName", "logrite.ts": ts, "logrite.cm": cm });
        selectName(this, cm);
    },

    request: function (cm, query, c, pos) {
      var self = this;
	logger.warn(`<<< Exiting request(body=${body},c=${c})`, { "event.action": "request", "logrite.body": body, "logrite.c": c });
      var doc = findDoc(this, cm.getDoc());
      var request = buildRequest(this, doc, query, pos);
      var extraOptions = request.query && this.options.queryOptions && this.options.queryOptions[request.query.type]
      if (extraOptions) for (var prop in extraOptions) request.query[prop] = extraOptions[prop];

      this.server.request(request, function (error, data) {
        if (!error && self.options.responseFilter)
          data = self.options.responseFilter(doc, query, request, error, data);
        c(error, data);
      });
    },

    destroy: function () {
	logger.error('>>> Entering destroy()', { "event.action": "destroy" });
      if (this.worker) {
        this.worker.terminate();
        this.worker = null;
		logger.debug(`destroy(): this.worker → ${this.worker}`, { "event.action": "destroy" });
      }
    }
  };

  var Pos = CodeMirror.Pos;
  var cls = "CodeMirror-Tern-";
  var bigDoc = 250;

  function getFile(ts, name, c) {
	logger.error(`>>> Entering getFile(ts=${ts},name=${name},c=${c})`, { "event.action": "getFile", "logrite.ts": ts, "logrite.name": name, "logrite.c": c });
    var buf = ts.docs[name];
    if (buf)
      c(docValue(ts, buf));
    else if (ts.options.getFile)
      ts.options.getFile(name, c);
    else
      c(null);
  }

  function findDoc(ts, doc, name) {
	logger.error(`>>> Entering findDoc(ts=${ts},doc=${doc},name=${name})`, { "event.action": "findDoc", "logrite.ts": ts, "logrite.doc": doc, "logrite.name": name });
    for (var n in ts.docs) {
      var cur = ts.docs[n];
      if (cur.doc == doc) return cur;
    }
    if (!name) for (var i = 0;; ++i) {
      n = "[doc" + (i || "") + "]";
      if (!ts.docs[n]) { name = n; break; }
    }
	logger.warn(`<<< Exiting findDoc(ts=${ts},doc=${doc},name=${name})`, { "event.action": "findDoc", "logrite.ts": ts, "logrite.doc": doc, "logrite.name": name });
    return ts.addDoc(name, doc);
  }

  function resolveDoc(ts, id) {
	logger.error(`>>> Entering resolveDoc(ts=${ts},id=${id})`, { "event.action": "resolveDoc", "logrite.ts": ts, "logrite.id": id });
    if (typeof id == "string") return ts.docs[id];
    if (id instanceof CodeMirror) id = id.getDoc();
    if (id instanceof CodeMirror.Doc) return findDoc(ts, id);
	logger.warn(`<<< Exiting resolveDoc(ts=${ts},id=${id})`, { "event.action": "resolveDoc", "logrite.ts": ts, "logrite.id": id });
  }

  function trackChange(ts, doc, change) {
    var data = findDoc(ts, doc);

	logger.error(`trackChange(ts=${ts},doc=${doc},change=${change}): data → ${data}`, { "event.action": "trackChange", "logrite.ts": ts, "logrite.doc": doc, "logrite.change": change });
	logger.debug(`trackChange(ts=${ts},doc=${doc},change=${change}): data.changed → ${data.changed}`, { "event.action": "trackChange", "logrite.ts": ts, "logrite.doc": doc, "logrite.change": change });
    var argHints = ts.cachedArgHints;
    if (argHints && argHints.doc == doc && cmpPos(argHints.start, change.to) <= 0)
      ts.cachedArgHints = null;

	logger.debug(`trackChange(ts=${ts},doc=${doc},change=${change}): ts.cachedArgHints → ${ts.cachedArgHints}`, { "event.action": "trackChange", "logrite.ts": ts, "logrite.doc": doc, "logrite.change": change });
    var changed = data.changed;
    if (changed == null)
      data.changed = changed = {from: change.from.line, to: change.from.line};
    var end = change.from.line + (change.text.length - 1);
	logger.error(`trackChange(ts=${ts},doc=${doc},change=${change}): end → ${end}`, { "event.action": "trackChange", "logrite.ts": ts, "logrite.doc": doc, "logrite.change": change });
    if (change.from.line < changed.to) changed.to = changed.to - (change.to.line - end);
    if (end >= changed.to) changed.to = end + 1;
    if (changed.from > change.from.line) changed.from = change.from.line;

    if (doc.lineCount() > bigDoc && change.to - changed.from > 100) setTimeout(function() {
      if (data.changed && data.changed.to - data.changed.from > 100) sendDoc(ts, data);
    }, 200);
  }

  function sendDoc(ts, doc) {
	logger.error(`>>> Entering sendDoc(ts=${ts},doc=${doc})`, { "event.action": "sendDoc", "logrite.ts": ts, "logrite.doc": doc });
    ts.server.request({files: [{type: "full", name: doc.name, text: docValue(ts, doc)}]}, function(error) {
      if (error) window.console.error(error);
      else doc.changed = null;
    });
	logger.warn(`<<< Exiting sendDoc(ts=${ts},doc=${doc})`, { "event.action": "sendDoc", "logrite.ts": ts, "logrite.doc": doc });
  }

  // Completion

  function hint(ts, cm, c) {
	logger.error(`>>> Entering hint(ts=${ts},cm=${cm},c=${c})`, { "event.action": "hint", "logrite.ts": ts, "logrite.cm": cm, "logrite.c": c });
    ts.request(cm, {type: "completions", types: true, docs: true, urls: true}, function(error, data) {
      if (error) return showError(ts, cm, error);
      var completions = [], after = "";
      var from = data.start, to = data.end;
      if (cm.getRange(Pos(from.line, from.ch - 2), from) == "[\"" &&
          cm.getRange(to, Pos(to.line, to.ch + 2)) != "\"]")
        after = "\"]";

      for (var i = 0; i < data.completions.length; ++i) {
        var completion = data.completions[i], className = typeToIcon(completion.type);
        if (data.guess) className += " " + cls + "guess";
        completions.push({text: completion.name + after,
                          displayText: completion.displayName || completion.name,
                          className: className,
                          data: completion});
      }

      var obj = {from: from, to: to, list: completions};
      var tooltip = null;
      CodeMirror.on(obj, "close", function() { remove(tooltip); });
      CodeMirror.on(obj, "update", function() { remove(tooltip); });
      CodeMirror.on(obj, "select", function(cur, node) {
        remove(tooltip);
        var content = ts.options.completionTip ? ts.options.completionTip(cur.data) : cur.data.doc;
        if (content) {
          tooltip = makeTooltip(node.parentNode.getBoundingClientRect().right + window.pageXOffset,
                                node.getBoundingClientRect().top + window.pageYOffset, content);
          tooltip.className += " " + cls + "hint-doc";
        }
      });
      c(obj);
    });
	logger.warn(`<<< Exiting hint(ts=${ts},cm=${cm},c=${c})`, { "event.action": "hint", "logrite.ts": ts, "logrite.cm": cm, "logrite.c": c });
  }

  function typeToIcon(type) {
	logger.error(`>>> Entering typeToIcon(type=${type})`, { "event.action": "typeToIcon", "logrite.type": type });
    var suffix;
    if (type == "?") suffix = "unknown";
    else if (type == "number" || type == "string" || type == "bool") suffix = type;
    else if (/^fn\(/.test(type)) suffix = "fn";
    else if (/^\[/.test(type)) suffix = "array";
    else suffix = "object";
	logger.warn(`<<< Exiting typeToIcon(type=${type})`, { "event.action": "typeToIcon", "logrite.type": type });
    return cls + "completion " + cls + "completion-" + suffix;
  }

  // Type queries

  function showContextInfo(ts, cm, pos, queryName, c) {
	logger.error(`>>> Entering showContextInfo(ts=${ts},cm=${cm},pos=${pos},queryName=${queryName},c=${c})`, { "event.action": "showContextInfo", "logrite.ts": ts, "logrite.cm": cm, "logrite.pos": pos, "logrite.queryName": queryName, "logrite.c": c });
    ts.request(cm, queryName, function(error, data) {
      if (error) return showError(ts, cm, error);
      if (ts.options.typeTip) {
        var tip = ts.options.typeTip(data);
      } else {
        var tip = elt("span", null, elt("strong", null, data.type || "not found"));
        if (data.doc)
          tip.appendChild(document.createTextNode(" — " + data.doc));
        if (data.url) {
          tip.appendChild(document.createTextNode(" "));
          var child = tip.appendChild(elt("a", null, "[docs]"));
		logger.debug(`showContextInfo(ts=${ts},cm=${cm},pos=${pos},queryName=${queryName},c=${c}): child.target → ${child.target}`, { "event.action": "showContextInfo", "logrite.ts": ts, "logrite.cm": cm, "logrite.pos": pos, "logrite.queryName": queryName, "logrite.c": c });
          child.href = data.url;
		logger.debug(`showContextInfo(ts=${ts},cm=${cm},pos=${pos},queryName=${queryName},c=${c}): child.href → ${child.href}`, { "event.action": "showContextInfo", "logrite.ts": ts, "logrite.cm": cm, "logrite.pos": pos, "logrite.queryName": queryName, "logrite.c": c });
          child.target = "_blank";
        }
      }
      tempTooltip(cm, tip, ts);
      if (c) c();
    }, pos);
	logger.warn(`<<< Exiting showContextInfo(ts=${ts},cm=${cm},pos=${pos},queryName=${queryName},c=${c})`, { "event.action": "showContextInfo", "logrite.ts": ts, "logrite.cm": cm, "logrite.pos": pos, "logrite.queryName": queryName, "logrite.c": c });
  }

  // Maintaining argument hints

  function updateArgHints(ts, cm) {
    closeArgHints(ts);

    if (cm.somethingSelected()) return;
    var state = cm.getTokenAt(cm.getCursor()).state;
    var inner = CodeMirror.innerMode(cm.getMode(), state);
    if (inner.mode.name != "javascript") return;
    var lex = inner.state.lexical;
    if (lex.info != "call") return;

    var ch, argPos = lex.pos || 0, tabSize = cm.getOption("tabSize");
    for (var line = cm.getCursor().line, e = Math.max(0, line - 9), found = false; line >= e; --line) {
	logger.error(`updateArgHints(ts=${ts},cm=${cm}): line → ${line}`, { "event.action": "updateArgHints", "logrite.ts": ts, "logrite.cm": cm });
      var str = cm.getLine(line), extra = 0;
	logger.error(`updateArgHints(ts=${ts},cm=${cm}): str → ${str}`, { "event.action": "updateArgHints", "logrite.ts": ts, "logrite.cm": cm });
      for (var pos = 0;;) {
        var tab = str.indexOf("\t", pos);
		logger.error(`updateArgHints(ts=${ts},cm=${cm}): tab → ${tab}`, { "event.action": "updateArgHints", "logrite.ts": ts, "logrite.cm": cm });
        if (tab == -1) break;
        extra += tabSize - (tab + extra) % tabSize - 1;
        pos = tab + 1;
      }
      ch = lex.column - extra;
      if (str.charAt(ch) == "(") {found = true; break;}
    }
    if (!found) return;

    var start = Pos(line, ch);
    var cache = ts.cachedArgHints;
    if (cache && cache.doc == cm.getDoc() && cmpPos(start, cache.start) == 0)
      return showArgHints(ts, cm, argPos);

    ts.request(cm, {type: "type", preferFunction: true, end: start}, function(error, data) {
      if (error || !data.type || !(/^fn\(/).test(data.type)) return;
      ts.cachedArgHints = {
        start: pos,
        type: parseFnType(data.type),
        name: data.exprName || data.name || "fn",
        guess: data.guess,
        doc: cm.getDoc()
      };
      showArgHints(ts, cm, argPos);
    });
  }

  function showArgHints(ts, cm, pos) {
	logger.error(`>>> Entering showArgHints(ts=${ts},cm=${cm},pos=${pos})`, { "event.action": "showArgHints", "logrite.ts": ts, "logrite.cm": cm, "logrite.pos": pos });
    closeArgHints(ts);

    var cache = ts.cachedArgHints, tp = cache.type;
    var tip = elt("span", cache.guess ? cls + "fhint-guess" : null,
                  elt("span", cls + "fname", cache.name), "(");
    for (var i = 0; i < tp.args.length; ++i) {
      if (i) tip.appendChild(document.createTextNode(", "));
      var arg = tp.args[i];
      tip.appendChild(elt("span", cls + "farg" + (i == pos ? " " + cls + "farg-current" : ""), arg.name || "?"));
      if (arg.type != "?") {
        tip.appendChild(document.createTextNode(":\u00a0"));
        tip.appendChild(elt("span", cls + "type", arg.type));
      }
    }
    tip.appendChild(document.createTextNode(tp.rettype ? ") ->\u00a0" : ")"));
    if (tp.rettype) tip.appendChild(elt("span", cls + "type", tp.rettype));
    var place = cm.cursorCoords(null, "page");
    ts.activeArgHints = makeTooltip(place.right + 1, place.bottom, tip);
	logger.error(`showArgHints(ts=${ts},cm=${cm},pos=${pos}): place → ${place}`, { "event.action": "showArgHints", "logrite.ts": ts, "logrite.cm": cm, "logrite.pos": pos });
  }

  function parseFnType(text) {
	logger.error(`>>> Entering parseFnType(text=${text})`, { "event.action": "parseFnType", "logrite.text": text });
    var args = [], pos = 3;

    function skipMatching(upto) {
	logger.error(`>>> Entering skipMatching(upto=${upto})`, { "event.action": "skipMatching", "logrite.upto": upto });
      var depth = 0, start = pos;
      for (;;) {
        var next = text.charAt(pos);
        if (upto.test(next) && !depth) return text.slice(start, pos);
		logger.error(`skipMatching(upto=${upto}): next → ${next}`, { "event.action": "skipMatching", "logrite.upto": upto });
        if (/[{\[\(]/.test(next)) ++depth;
        else if (/[}\]\)]/.test(next)) --depth;
        ++pos;
      }
    }

    // Parse arguments
    if (text.charAt(pos) != ")") for (;;) {
      var name = text.slice(pos).match(/^([^, \(\[\{]+): /);
      if (name) {
        pos += name[0].length;
        name = name[1];
      }
      args.push({name: name, type: skipMatching(/[\),]/)});
      if (text.charAt(pos) == ")") break;
      pos += 2;
    }

	logger.error(`parseFnType(text=${text}): name → ${name}`, { "event.action": "parseFnType", "logrite.text": text });
    var rettype = text.slice(pos).match(/^\) -> (.*)$/);
	logger.error(`parseFnType(text=${text}): rettype → ${rettype}`, { "event.action": "parseFnType", "logrite.text": text });

	logger.warn(`<<< Exiting parseFnType(text=${text})`, { "event.action": "parseFnType", "logrite.text": text });
    return {args: args, rettype: rettype && rettype[1]};
  }

  // Moving to the definition of something

  function jumpToDef(ts, cm) {
    function inner(varName) {
	logger.error(`>>> Entering inner(varName=${varName})`, { "event.action": "inner", "logrite.varName": varName });
      var req = {type: "definition", variable: varName || null};
      var doc = findDoc(ts, cm.getDoc());
	logger.error(`inner(varName=${varName}): req → ${req}`, { "event.action": "inner", "logrite.varName": varName });
	logger.error(`inner(varName=${varName}): doc → ${doc}`, { "event.action": "inner", "logrite.varName": varName });
      ts.server.request(buildRequest(ts, doc, req), function(error, data) {
        if (error) return showError(ts, cm, error);
        if (!data.file && data.url) { window.open(data.url); return; }

        if (data.file) {
          var localDoc = ts.docs[data.file], found;
          if (localDoc && (found = findContext(localDoc.doc, data))) {
            ts.jumpStack.push({file: doc.name,
                               start: cm.getCursor("from"),
                               end: cm.getCursor("to")});
            moveTo(ts, doc, localDoc, found.start, found.end);
            return;
          }
        }
        showError(ts, cm, "Could not find a definition.");
      });
    }

    if (!atInterestingExpression(cm))
      dialog(cm, "Jump to variable", function(name) { if (name) inner(name); });
    else
      inner();
  }

  function jumpBack(ts, cm) {
    var pos = ts.jumpStack.pop(), doc = pos && ts.docs[pos.file];
    if (!doc) return;
	logger.error(`jumpBack(ts=${ts},cm=${cm}): pos → ${pos}`, { "event.action": "jumpBack", "logrite.ts": ts, "logrite.cm": cm });
    moveTo(ts, findDoc(ts, cm.getDoc()), doc, pos.start, pos.end);
  }

  function moveTo(ts, curDoc, doc, start, end) {
	logger.error(`>>> Entering moveTo(ts=${ts},curDoc=${curDoc},doc=${doc},start=${start},end=${end})`, { "event.action": "moveTo", "logrite.ts": ts, "logrite.curDoc": curDoc, "logrite.doc": doc, "logrite.start": start, "logrite.end": end });
    doc.doc.setSelection(start, end);
    if (curDoc != doc && ts.options.switchToDoc) {
      closeArgHints(ts);
      ts.options.switchToDoc(doc.name, doc.doc);
	logger.warn(`<<< Exiting moveTo(ts=${ts},curDoc=${curDoc},doc=${doc},start=${start},end=${end})`, { "event.action": "moveTo", "logrite.ts": ts, "logrite.curDoc": curDoc, "logrite.doc": doc, "logrite.start": start, "logrite.end": end });
    }
  }

  // The {line,ch} representation of positions makes this rather awkward.
  function findContext(doc, data) {
	logger.error(`>>> Entering findContext(doc=${doc},data=${data})`, { "event.action": "findContext", "logrite.doc": doc, "logrite.data": data });
    var before = data.context.slice(0, data.contextOffset).split("\n");
    var startLine = data.start.line - (before.length - 1);
	logger.error(`findContext(doc=${doc},data=${data}): before → ${before}`, { "event.action": "findContext", "logrite.doc": doc, "logrite.data": data });
	logger.error(`findContext(doc=${doc},data=${data}): startLine → ${startLine}`, { "event.action": "findContext", "logrite.doc": doc, "logrite.data": data });
    var start = Pos(startLine, (before.length == 1 ? data.start.ch : doc.getLine(startLine).length) - before[0].length);

    var text = doc.getLine(startLine).slice(start.ch);
	logger.error(`findContext(doc=${doc},data=${data}): text → ${text}`, { "event.action": "findContext", "logrite.doc": doc, "logrite.data": data });
    for (var cur = startLine + 1; cur < doc.lineCount() && text.length < data.context.length; ++cur)
      text += "\n" + doc.getLine(cur);
    if (text.slice(0, data.context.length) == data.context) return data;

    var cursor = doc.getSearchCursor(data.context, 0, false);
	logger.error(`findContext(doc=${doc},data=${data}): cursor → ${cursor}`, { "event.action": "findContext", "logrite.doc": doc, "logrite.data": data });
    var nearest, nearestDist = Infinity;
    while (cursor.findNext()) {
      var from = cursor.from(), dist = Math.abs(from.line - start.line) * 10000;
	logger.error(`findContext(doc=${doc},data=${data}): from → ${from}`, { "event.action": "findContext", "logrite.doc": doc, "logrite.data": data });
      if (!dist) dist = Math.abs(from.ch - start.ch);
      if (dist < nearestDist) { nearest = from; nearestDist = dist; }
	logger.error(`findContext(doc=${doc},data=${data}): nearest → ${nearest}`, { "event.action": "findContext", "logrite.doc": doc, "logrite.data": data });
    }
    if (!nearest) return null;

    if (before.length == 1)
      nearest.ch += before[0].length;
    else
      nearest = Pos(nearest.line + (before.length - 1), before[before.length - 1].length);
    if (data.start.line == data.end.line)
      var end = Pos(nearest.line, nearest.ch + (data.end.ch - data.start.ch));
    else
      var end = Pos(nearest.line + (data.end.line - data.start.line), data.end.ch);
	logger.error(`findContext(doc=${doc},data=${data}): end → ${end}`, { "event.action": "findContext", "logrite.doc": doc, "logrite.data": data });
    return {start: nearest, end: end};
  }

  function atInterestingExpression(cm) {
	logger.error(`>>> Entering atInterestingExpression(cm=${cm})`, { "event.action": "atInterestingExpression", "logrite.cm": cm });
    var pos = cm.getCursor("end"), tok = cm.getTokenAt(pos);
    if (tok.start < pos.ch && tok.type == "comment") return false;
	logger.error(`atInterestingExpression(cm=${cm}): pos → ${pos}`, { "event.action": "atInterestingExpression", "logrite.cm": cm });
	logger.warn(`<<< Exiting atInterestingExpression(cm=${cm})`, { "event.action": "atInterestingExpression", "logrite.cm": cm });
    return /[\w)\]]/.test(cm.getLine(pos.line).slice(Math.max(pos.ch - 1, 0), pos.ch + 1));
  }

  // Variable renaming

  function rename(ts, cm) {
	logger.error(`>>> Entering rename(ts=${ts},cm=${cm})`, { "event.action": "rename", "logrite.ts": ts, "logrite.cm": cm });
    var token = cm.getTokenAt(cm.getCursor());
    if (!/\w/.test(token.string)) return showError(ts, cm, "Not at a variable");
    dialog(cm, "New name for " + token.string, function(newName) {
	logger.warn(`<<< Exiting rename(ts=${ts},cm=${cm})`, { "event.action": "rename", "logrite.ts": ts, "logrite.cm": cm });
      ts.request(cm, {type: "rename", newName: newName, fullDocs: true}, function(error, data) {
        if (error) return showError(ts, cm, error);
        applyChanges(ts, data.changes);
      });
    });
  }

  function selectName(ts, cm) {
    var name = findDoc(ts, cm.doc).name;
    ts.request(cm, {type: "refs"}, function(error, data) {
	logger.error(`selectName(ts=${ts},cm=${cm}): name → ${name}`, { "event.action": "selectName", "logrite.ts": ts, "logrite.cm": cm });
      if (error) return showError(ts, cm, error);
      var ranges = [], cur = 0;
      var curPos = cm.getCursor();
      for (var i = 0; i < data.refs.length; i++) {
        var ref = data.refs[i];
        if (ref.file == name) {
          ranges.push({anchor: ref.start, head: ref.end});
          if (cmpPos(curPos, ref.start) >= 0 && cmpPos(curPos, ref.end) <= 0)
            cur = ranges.length - 1;
        }
      }
      cm.setSelections(ranges, cur);
    });
  }

  var nextChangeOrig = 0;
  function applyChanges(ts, changes) {
	logger.error(`>>> Entering applyChanges(ts=${ts},changes=${changes})`, { "event.action": "applyChanges", "logrite.ts": ts, "logrite.changes": changes });
    var perFile = Object.create(null);
    for (var i = 0; i < changes.length; ++i) {
	logger.error(`applyChanges(ts=${ts},changes=${changes}): perFile → ${perFile}`, { "event.action": "applyChanges", "logrite.ts": ts, "logrite.changes": changes });
      var ch = changes[i];
      (perFile[ch.file] || (perFile[ch.file] = [])).push(ch);
    }
    for (var file in perFile) {
      var known = ts.docs[file], chs = perFile[file];;
      if (!known) continue;
      chs.sort(function(a, b) { return cmpPos(b.start, a.start); });
      var origin = "*rename" + (++nextChangeOrig);
	logger.error(`applyChanges(ts=${ts},changes=${changes}): origin → ${origin}`, { "event.action": "applyChanges", "logrite.ts": ts, "logrite.changes": changes });
      for (var i = 0; i < chs.length; ++i) {
        var ch = chs[i];
        known.doc.replaceRange(ch.text, ch.start, ch.end, origin);
		logger.warn(`<<< Exiting applyChanges(ts=${ts},changes=${changes})`, { "event.action": "applyChanges", "logrite.ts": ts, "logrite.changes": changes });
      }
    }
  }

  // Generic request-building helper

  function buildRequest(ts, doc, query, pos) {
	logger.error(`>>> Entering buildRequest(ts=${ts},doc=${doc},query=${query},pos=${pos})`, { "event.action": "buildRequest", "logrite.ts": ts, "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos });
    var files = [], offsetLines = 0, allowFragments = !query.fullDocs;
    if (!allowFragments) delete query.fullDocs;
    if (typeof query == "string") query = {type: query};
    query.lineCharPositions = true;
	logger.debug(`buildRequest(ts=${ts},doc=${doc},query=${query},pos=${pos}): query.lineCharPositions → ${query.lineCharPositions}`, { "event.action": "buildRequest", "logrite.ts": ts, "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos });
    if (query.end == null) {
      query.end = pos || doc.doc.getCursor("end");
      if (doc.doc.somethingSelected())
        query.start = doc.doc.getCursor("start");
    }
    var startPos = query.start || query.end;

    if (doc.changed) {
      if (doc.doc.lineCount() > bigDoc && allowFragments !== false &&
          doc.changed.to - doc.changed.from < 100 &&
          doc.changed.from <= startPos.line && doc.changed.to > query.end.line) {
        files.push(getFragmentAround(doc, startPos, query.end));
        query.file = "#0";
		logger.debug(`buildRequest(ts=${ts},doc=${doc},query=${query},pos=${pos}): query.file → ${query.file}`, { "event.action": "buildRequest", "logrite.ts": ts, "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos });
        var offsetLines = files[0].offsetLines;
        if (query.start != null) query.start = Pos(query.start.line - -offsetLines, query.start.ch);
        query.end = Pos(query.end.line - offsetLines, query.end.ch);
      } else {
        files.push({type: "full",
                    name: doc.name,
                    text: docValue(ts, doc)});
        query.file = doc.name;
        doc.changed = null;
		logger.debug(`buildRequest(ts=${ts},doc=${doc},query=${query},pos=${pos}): doc.changed → ${doc.changed}`, { "event.action": "buildRequest", "logrite.ts": ts, "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos });
      }
    } else {
      query.file = doc.name;
    }
    for (var name in ts.docs) {
      var cur = ts.docs[name];
	logger.debug(`buildRequest(ts=${ts},doc=${doc},query=${query},pos=${pos}): cur.changed → ${cur.changed}`, { "event.action": "buildRequest", "logrite.ts": ts, "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos });
      if (cur.changed && cur != doc) {
        files.push({type: "full", name: cur.name, text: docValue(ts, cur)});
        cur.changed = null;
      }
    }

	logger.warn(`<<< Exiting buildRequest(ts=${ts},doc=${doc},query=${query},pos=${pos})`, { "event.action": "buildRequest", "logrite.ts": ts, "logrite.doc": doc, "logrite.query": query, "logrite.pos": pos });
    return {query: query, files: files};
  }

  function getFragmentAround(data, start, end) {
	logger.error(`>>> Entering getFragmentAround(data=${data},start=${start},end=${end})`, { "event.action": "getFragmentAround", "logrite.data": data, "logrite.start": start, "logrite.end": end });
    var doc = data.doc;
    var minIndent = null, minLine = null, endLine, tabSize = 4;
    for (var p = start.line - 1, min = Math.max(0, p - 50); p >= min; --p) {
      var line = doc.getLine(p), fn = line.search(/\bfunction\b/);
	logger.error(`getFragmentAround(data=${data},start=${start},end=${end}): line → ${line}`, { "event.action": "getFragmentAround", "logrite.data": data, "logrite.start": start, "logrite.end": end });
      if (fn < 0) continue;
      var indent = CodeMirror.countColumn(line, null, tabSize);
	logger.error(`getFragmentAround(data=${data},start=${start},end=${end}): indent → ${indent}`, { "event.action": "getFragmentAround", "logrite.data": data, "logrite.start": start, "logrite.end": end });
      if (minIndent != null && minIndent <= indent) continue;
      minIndent = indent;
      minLine = p;
    }
    if (minLine == null) minLine = min;
	logger.error(`getFragmentAround(data=${data},start=${start},end=${end}): p → ${p}`, { "event.action": "getFragmentAround", "logrite.data": data, "logrite.start": start, "logrite.end": end });
    var max = Math.min(doc.lastLine(), end.line + 20);
    if (minIndent == null || minIndent == CodeMirror.countColumn(doc.getLine(start.line), null, tabSize))
      endLine = max;
    else for (endLine = end.line + 1; endLine < max; ++endLine) {
      var indent = CodeMirror.countColumn(doc.getLine(endLine), null, tabSize);
      if (indent <= minIndent) break;
    }
    var from = Pos(minLine, 0);

	logger.warn(`<<< Exiting getFragmentAround(data=${data},start=${start},end=${end})`, { "event.action": "getFragmentAround", "logrite.data": data, "logrite.start": start, "logrite.end": end });
    return {type: "part",
            name: data.name,
            offsetLines: from.line,
            text: doc.getRange(from, Pos(endLine, 0))};
  }

  // Generic utilities

  var cmpPos = CodeMirror.cmpPos;

  function elt(tagname, cls /*, ... elts*/) {
	logger.error(`>>> Entering elt(tagname=${tagname})`, { "event.action": "elt", "logrite.tagname": tagname });
    var e = document.createElement(tagname);
    if (cls) e.className = cls;
	logger.error(`elt(tagname=${tagname}): e → ${e}`, { "event.action": "elt", "logrite.tagname": tagname });
    for (var i = 2; i < arguments.length; ++i) {
      var elt = arguments[i];
      if (typeof elt == "string") elt = document.createTextNode(elt);
      e.appendChild(elt);
    }
	logger.warn(`<<< Exiting elt(tagname=${tagname})`, { "event.action": "elt", "logrite.tagname": tagname });
    return e;
  }

  function dialog(cm, text, f) {
	logger.error(`>>> Entering dialog(cm=${cm},text=${text},f=${f})`, { "event.action": "dialog", "logrite.cm": cm, "logrite.text": text, "logrite.f": f });
    if (cm.openDialog)
      cm.openDialog(text + ": <input type=text>", f);
    else
      f(prompt(text, ""));
	logger.warn(`<<< Exiting dialog(cm=${cm},text=${text},f=${f})`, { "event.action": "dialog", "logrite.cm": cm, "logrite.text": text, "logrite.f": f });
  }

  // Tooltips

  function tempTooltip(cm, content, ts) {
	logger.error(`>>> Entering tempTooltip(cm=${cm},content=${content},ts=${ts})`, { "event.action": "tempTooltip", "logrite.cm": cm, "logrite.content": content, "logrite.ts": ts });
    if (cm.state.ternTooltip) remove(cm.state.ternTooltip);
    var where = cm.cursorCoords();
	logger.warn(`<<< Exiting tempTooltip(cm=${cm},content=${content},ts=${ts})`, { "event.action": "tempTooltip", "logrite.cm": cm, "logrite.content": content, "logrite.ts": ts });
	logger.error(`tempTooltip(cm=${cm},content=${content},ts=${ts}): where → ${where}`, { "event.action": "tempTooltip", "logrite.cm": cm, "logrite.content": content, "logrite.ts": ts });
    var tip = cm.state.ternTooltip = makeTooltip(where.right + 1, where.bottom, content);
    function maybeClear() {
	logger.error('>>> Entering maybeClear()', { "event.action": "maybeClear" });
      old = true;
      if (!mouseOnTip) clear();
	logger.warn('<<< Exiting maybeClear()', { "event.action": "maybeClear" });
    }
    function clear() {
	logger.error('>>> Entering clear()', { "event.action": "clear" });
      cm.state.ternTooltip = null;
      if (!tip.parentNode) return;
      cm.off("cursorActivity", clear);
      cm.off('blur', clear);
      cm.off('scroll', clear);
      fadeOut(tip);
	logger.warn('<<< Exiting clear()', { "event.action": "clear" });
    }
    var mouseOnTip = false, old = false;
    CodeMirror.on(tip, "mousemove", function() { mouseOnTip = true; });
    CodeMirror.on(tip, "mouseout", function(e) {
      if (!CodeMirror.contains(tip, e.relatedTarget || e.toElement)) {
        if (old) clear();
        else mouseOnTip = false;
      }
    });
    setTimeout(maybeClear, ts.options.hintDelay ? ts.options.hintDelay : 1700);
    cm.on("cursorActivity", clear);
    cm.on('blur', clear);
    cm.on('scroll', clear);
  }

  function makeTooltip(x, y, content) {
	logger.error(`>>> Entering makeTooltip(x=${x},y=${y},content=${content})`, { "event.action": "makeTooltip", "logrite.x": x, "logrite.y": y, "logrite.content": content });
    var node = elt("div", cls + "tooltip", content);
    node.style.left = x + "px";
	logger.error(`makeTooltip(x=${x},y=${y},content=${content}): node → ${node}`, { "event.action": "makeTooltip", "logrite.x": x, "logrite.y": y, "logrite.content": content });
    node.style.top = y + "px";
    document.body.appendChild(node);
	logger.warn(`<<< Exiting makeTooltip(x=${x},y=${y},content=${content})`, { "event.action": "makeTooltip", "logrite.x": x, "logrite.y": y, "logrite.content": content });
    return node;
  }

  function remove(node) {
	logger.error(`>>> Entering remove(node=${node})`, { "event.action": "remove", "logrite.node": node });
    var p = node && node.parentNode;
    if (p) p.removeChild(node);
	logger.warn(`<<< Exiting remove(node=${node})`, { "event.action": "remove", "logrite.node": node });
  }

  function fadeOut(tooltip) {
	logger.error(`>>> Entering fadeOut(tooltip=${tooltip})`, { "event.action": "fadeOut", "logrite.tooltip": tooltip });
    tooltip.style.opacity = "0";
    setTimeout(function() { remove(tooltip); }, 1100);
	logger.warn(`<<< Exiting fadeOut(tooltip=${tooltip})`, { "event.action": "fadeOut", "logrite.tooltip": tooltip });
  }

  function showError(ts, cm, msg) {
	logger.error(`>>> Entering showError(ts=${ts},cm=${cm},msg=${msg})`, { "event.action": "showError", "logrite.ts": ts, "logrite.cm": cm, "logrite.msg": msg });
    if (ts.options.showError)
      ts.options.showError(cm, msg);
    else
      tempTooltip(cm, String(msg), ts);
	logger.warn(`<<< Exiting showError(ts=${ts},cm=${cm},msg=${msg})`, { "event.action": "showError", "logrite.ts": ts, "logrite.cm": cm, "logrite.msg": msg });
  }

  function closeArgHints(ts) {
	logger.error(`>>> Entering closeArgHints(ts=${ts})`, { "event.action": "closeArgHints", "logrite.ts": ts });
    if (ts.activeArgHints) { remove(ts.activeArgHints); ts.activeArgHints = null; }
	logger.warn(`<<< Exiting closeArgHints(ts=${ts})`, { "event.action": "closeArgHints", "logrite.ts": ts });
  }

  function docValue(ts, doc) {
	logger.error(`>>> Entering docValue(ts=${ts},doc=${doc})`, { "event.action": "docValue", "logrite.ts": ts, "logrite.doc": doc });
    var val = doc.doc.getValue();
    if (ts.options.fileFilter) val = ts.options.fileFilter(val, doc.name, doc.doc);
	logger.error(`docValue(ts=${ts},doc=${doc}): val → ${val}`, { "event.action": "docValue", "logrite.ts": ts, "logrite.doc": doc });
    return val;
  }

  // Worker wrapper

  function WorkerServer(ts) {
	logger.error(`>>> Entering WorkerServer(ts=${ts})`, { "event.action": "WorkerServer", "logrite.ts": ts });
    var worker = ts.worker = new Worker(ts.options.workerScript);
    worker.postMessage({type: "init",
                        defs: ts.options.defs,
                        plugins: ts.options.plugins,
                        scripts: ts.options.workerDeps});
    var msgId = 0, pending = {};
	logger.error(`WorkerServer(ts=${ts}): worker → ${worker}`, { "event.action": "WorkerServer", "logrite.ts": ts });

	logger.warn(`<<< Exiting WorkerServer(ts=${ts})`, { "event.action": "WorkerServer", "logrite.ts": ts });
    function send(data, c) {
	logger.error(`>>> Entering send(data=${data},c=${c})`, { "event.action": "send", "logrite.data": data, "logrite.c": c });
      if (c) {
        data.id = ++msgId;
		logger.debug(`send(data=${data},c=${c}): data.id → ${data.id}`, { "event.action": "send", "logrite.data": data, "logrite.c": c });
        pending[msgId] = c;
      }
      worker.postMessage(data);
	logger.warn(`<<< Exiting send(data=${data},c=${c})`, { "event.action": "send", "logrite.data": data, "logrite.c": c });
    }
    worker.onmessage = function(e) {
	logger.error(`>>> Entering onmessage(e=${e})`, { "event.action": "onmessage", "logrite.e": e });
      var data = e.data;
      if (data.type == "getFile") {
        getFile(ts, data.name, function(err, text) {
          send({type: "getFile", err: String(err), text: text, id: data.id});
        });
      } else if (data.type == "debug") {
        window.console.log(data.message);
      } else if (data.id && pending[data.id]) {
        pending[data.id](data.err, data.body);
        delete pending[data.id];
		logger.warn(`<<< Exiting onmessage(e=${e})`, { "event.action": "onmessage", "logrite.e": e });
      }
    };
    worker.onerror = function(e) {
	logger.error(`>>> Entering onerror(e=${e})`, { "event.action": "onerror", "logrite.e": e });
      for (var id in pending) pending[id](e);
      pending = {};
	logger.warn(`<<< Exiting onerror(e=${e})`, { "event.action": "onerror", "logrite.e": e });
    };

    this.addFile = function(name, text) { send({type: "add", name: name, text: text}); };
	logger.error(`>>> Entering addFile(name=${name},text=${text})`, { "event.action": "addFile", "logrite.name": name, "logrite.text": text });
    this.delFile = function(name) { send({type: "del", name: name}); };
	logger.warn(`<<< Exiting addFile(name=${name},text=${text})`, { "event.action": "addFile", "logrite.name": name, "logrite.text": text });
    this.request = function(body, c) { send({type: "req", body: body}, c); };
	logger.error(`>>> Entering delFile(name=${name})`, { "event.action": "delFile", "logrite.name": name });
  }
});
