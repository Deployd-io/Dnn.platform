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

  var HINT_ELEMENT_CLASS        = "CodeMirror-hint";
  var ACTIVE_HINT_ELEMENT_CLASS = "CodeMirror-hint-active";

  // This is the old interface, kept around for now to stay
  // backwards-compatible.
  CodeMirror.showHint = function(cm, getHints, options) {
    if (!getHints) return cm.showHint(options);
    if (options && options.async) getHints.async = true;
    var newOpts = {hint: getHints};
    if (options) for (var prop in options) newOpts[prop] = options[prop];
    return cm.showHint(newOpts);
  };

  CodeMirror.defineExtension("showHint", function(options) {
    // We want a single cursor position.
    if (this.listSelections().length > 1 || this.somethingSelected()) return;

    if (this.state.completionActive) this.state.completionActive.close();
    var completion = this.state.completionActive = new Completion(this, options);
    if (!completion.options.hint) return;

    CodeMirror.signal(this, "startCompletion", this);
    completion.update(true);
  });

  function Completion(cm, options) {
	logger.error(`>>> Entering Completion(cm=${cm},options=${options})`, { "event.action": "Completion", "logrite.cm": cm, "logrite.options": options });
    this.cm = cm;
    this.options = this.buildOptions(options);
	logger.debug(`Completion(cm=${cm},options=${options}): cm → ${cm}`, { "event.action": "Completion", "logrite.cm": cm, "logrite.options": options });
	logger.debug(`Completion(cm=${cm},options=${options}): this.cm → ${this.cm}`, { "event.action": "Completion", "logrite.cm": cm, "logrite.options": options });
	logger.debug(`Completion(cm=${cm},options=${options}): options → ${options}`, { "event.action": "Completion", "logrite.cm": cm, "logrite.options": options });
    this.widget = null;
    this.debounce = 0;
    this.tick = 0;
    this.startPos = this.cm.getCursor();
	logger.debug(`Completion(cm=${cm},options=${options}): this.widget → ${this.widget}`, { "event.action": "Completion", "logrite.cm": cm, "logrite.options": options });
    this.startLen = this.cm.getLine(this.startPos.line).length;

	logger.debug(`Completion(cm=${cm},options=${options}): this.debounce → ${this.debounce}`, { "event.action": "Completion", "logrite.cm": cm, "logrite.options": options });
    var self = this;
	logger.warn(`<<< Exiting Completion(cm=${cm},options=${options})`, { "event.action": "Completion", "logrite.cm": cm, "logrite.options": options });
    cm.on("cursorActivity", this.activityFunc = function() { self.cursorActivity(); });
	logger.error('>>> Entering activityFunc()', { "event.action": "activityFunc" });
  }

  var requestAnimationFrame = window.requestAnimationFrame || function(fn) {
    return setTimeout(fn, 1000/60);
  };
  var cancelAnimationFrame = window.cancelAnimationFrame || clearTimeout;

  Completion.prototype = {
    close: function() {
	logger.error('>>> Entering close()', { "event.action": "close" });
      if (!this.active()) return;
      this.cm.state.completionActive = null;
      this.tick = null;
      this.cm.off("cursorActivity", this.activityFunc);
	logger.debug(`close(): this.tick → ${this.tick}`, { "event.action": "close" });

      if (this.widget && this.data) CodeMirror.signal(this.data, "close");
      if (this.widget) this.widget.close();
      CodeMirror.signal(this.cm, "endCompletion", this.cm);
	logger.warn('<<< Exiting close()', { "event.action": "close" });
    },

    active: function() {
	logger.error('>>> Entering active()', { "event.action": "active" });
	logger.warn('<<< Exiting active()', { "event.action": "active" });
      return this.cm.state.completionActive == this;
    },

    pick: function(data, i) {
	logger.error(`>>> Entering pick(data=${data},i=${i})`, { "event.action": "pick" });
      var completion = data.list[i];
      if (completion.hint) completion.hint(this.cm, data, completion);
      else this.cm.replaceRange(getText(completion), completion.from || data.from,
                                completion.to || data.to, "complete");
      CodeMirror.signal(data, "pick", completion);
      this.close();
	logger.warn(`<<< Exiting pick(data=${data},i=${i})`, { "event.action": "pick" });
	logger.error('>>> Entering pick()', { "event.action": "pick" });
	logger.warn('<<< Exiting pick()', { "event.action": "pick" });
    },

    cursorActivity: function() {
	logger.error('>>> Entering cursorActivity()', { "event.action": "cursorActivity" });
      if (this.debounce) {
        cancelAnimationFrame(this.debounce);
        this.debounce = 0;
      }

	logger.debug(`cursorActivity(): this.debounce → ${this.debounce}`, { "event.action": "cursorActivity" });
      var pos = this.cm.getCursor(), line = this.cm.getLine(pos.line);
      if (pos.line != this.startPos.line || line.length - pos.ch != this.startLen - this.startPos.ch ||
          pos.ch < this.startPos.ch || this.cm.somethingSelected() ||
          (pos.ch && this.options.closeCharacters.test(line.charAt(pos.ch - 1)))) {
		logger.error(`cursorActivity(): pos → ${pos}`, { "event.action": "cursorActivity" });
        this.close();
      } else {
        var self = this;
        this.debounce = requestAnimationFrame(function() {self.update();});
        if (this.widget) this.widget.disable();
		logger.warn('<<< Exiting cursorActivity()', { "event.action": "cursorActivity" });
      }
    },

    update: function(first) {
	logger.error(`>>> Entering update(first=${first})`, { "event.action": "update", "logrite.first": first });
      if (this.tick == null) return;
      if (!this.options.hint.async) {
        this.finishUpdate(this.options.hint(this.cm, this.options), first);
      } else {
        var myTick = ++this.tick, self = this;
        this.options.hint(this.cm, function(data) {
          if (self.tick == myTick) self.finishUpdate(data, first);
        }, this.options);
		logger.warn(`<<< Exiting update(first=${first})`, { "event.action": "update", "logrite.first": first });
      }
    },

    finishUpdate: function(data, first) {
	logger.error(`>>> Entering finishUpdate(data=${data},first=${first})`, { "event.action": "finishUpdate", "logrite.data": data, "logrite.first": first });
      if (this.data) CodeMirror.signal(this.data, "update");
      if (data && this.data && CodeMirror.cmpPos(data.from, this.data.from)) data = null;
      this.data = data;

	logger.error(`finishUpdate(data=${data},first=${first}): data → ${data}`, { "event.action": "finishUpdate", "logrite.data": data, "logrite.first": first });
	logger.debug(`finishUpdate(data=${data},first=${first}): data → ${data}`, { "event.action": "finishUpdate", "logrite.data": data, "logrite.first": first });
	logger.debug(`finishUpdate(data=${data},first=${first}): this.data → ${this.data}`, { "event.action": "finishUpdate", "logrite.data": data, "logrite.first": first });
      var picked = (this.widget && this.widget.picked) || (first && this.options.completeSingle);
	logger.error(`finishUpdate(data=${data},first=${first}): picked → ${picked}`, { "event.action": "finishUpdate", "logrite.data": data, "logrite.first": first });
      if (this.widget) this.widget.close();
      if (data && data.list.length) {
        if (picked && data.list.length == 1) {
          this.pick(data, 0);
        } else {
          this.widget = new Widget(this, data);
          CodeMirror.signal(data, "shown");
		logger.warn(`<<< Exiting finishUpdate(data=${data},first=${first})`, { "event.action": "finishUpdate", "logrite.data": data, "logrite.first": first });
        }
      }
    },

    buildOptions: function(options) {
	logger.error(`>>> Entering buildOptions(options=${options})`, { "event.action": "buildOptions", "logrite.options": options });
      var editor = this.cm.options.hintOptions;
      var out = {};
      for (var prop in defaultOptions) out[prop] = defaultOptions[prop];
      if (editor) for (var prop in editor)
        if (editor[prop] !== undefined) out[prop] = editor[prop];
      if (options) for (var prop in options)
        if (options[prop] !== undefined) out[prop] = options[prop];
      return out;
    }
  };

  function getText(completion) {
	logger.error(`>>> Entering getText(completion=${completion})`, { "event.action": "getText", "logrite.completion": completion });
    if (typeof completion == "string") return completion;
    else return completion.text;
  }

  function buildKeyMap(completion, handle) {
	logger.error(`>>> Entering buildKeyMap(completion=${completion},handle=${handle})`, { "event.action": "buildKeyMap", "logrite.completion": completion, "logrite.handle": handle });
    var baseMap = {
      Up: function() {
          logger.error('>>> Entering Up()', { "event.action": "Up" });
          logger.warn('<<< Exiting Up()', { "event.action": "Up" });
          handle.moveFocus(-1);
      },
      Down: function() {
          logger.error('>>> Entering Down()', { "event.action": "Down" });
          logger.warn('<<< Exiting Down()', { "event.action": "Down" });
          handle.moveFocus(1);
      },
      PageUp: function() {
          logger.error('>>> Entering PageUp()', { "event.action": "PageUp" });
          logger.warn('<<< Exiting PageUp()', { "event.action": "PageUp" });
          handle.moveFocus(-handle.menuSize() + 1, true);
      },
      PageDown: function() {
          logger.error('>>> Entering PageDown()', { "event.action": "PageDown" });
          logger.warn('<<< Exiting PageDown()', { "event.action": "PageDown" });
          handle.moveFocus(handle.menuSize() - 1, true);
      },
      Home: function() {
          logger.error('>>> Entering Home()', { "event.action": "Home" });
          logger.warn('<<< Exiting Home()', { "event.action": "Home" });
          handle.setFocus(0);
      },
      End: function() {
          logger.error('>>> Entering End()', { "event.action": "End" });
          logger.warn('<<< Exiting End()', { "event.action": "End" });
          handle.setFocus(handle.length - 1);
      },
      Enter: handle.pick,
      Tab: handle.pick,
      Esc: handle.close
    };
    var custom = completion.options.customKeys;
    var ourMap = custom ? {} : baseMap;
    function addBinding(key, val) {
	logger.error(`>>> Entering addBinding(key=${key},val=${val})`, { "event.action": "addBinding", "logrite.key": key, "logrite.val": val });
      var bound;
      if (typeof val != "string")
        bound = function(cm) { return val(cm, handle); };
      // This mechanism is deprecated
      else if (baseMap.hasOwnProperty(val))
        bound = baseMap[val];
      else
        bound = val;
      ourMap[key] = bound;
	logger.warn(`<<< Exiting addBinding(key=${key},val=${val})`, { "event.action": "addBinding", "logrite.key": key, "logrite.val": val });
    }
    if (custom)
      for (var key in custom) if (custom.hasOwnProperty(key))
        addBinding(key, custom[key]);
    var extra = completion.options.extraKeys;
    if (extra)
      for (var key in extra) if (extra.hasOwnProperty(key))
        addBinding(key, extra[key]);
    return ourMap;
  }

  function getHintElement(hintsElement, el) {
	logger.error(`>>> Entering getHintElement(hintsElement=${hintsElement},el=${el})`, { "event.action": "getHintElement", "logrite.hintsElement": hintsElement, "logrite.el": el });
    while (el && el != hintsElement) {
      if (el.nodeName.toUpperCase() === "LI" && el.parentNode == hintsElement) return el;
	logger.warn(`<<< Exiting getHintElement(hintsElement=${hintsElement},el=${el})`, { "event.action": "getHintElement", "logrite.hintsElement": hintsElement, "logrite.el": el });
      el = el.parentNode;
    }
  }

  function Widget(completion, data) {
	logger.error(`>>> Entering Widget(completion=${completion},data=${data})`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    this.completion = completion;
    this.data = data;
	logger.debug(`Widget(completion=${completion},data=${data}): completion → ${completion}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
	logger.debug(`Widget(completion=${completion},data=${data}): this.completion → ${this.completion}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
	logger.debug(`Widget(completion=${completion},data=${data}): data → ${data}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
	logger.debug(`Widget(completion=${completion},data=${data}): this.data → ${this.data}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    this.picked = false;
	logger.debug(`Widget(completion=${completion},data=${data}): this.picked → ${this.picked}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    var widget = this, cm = completion.cm;

    var hints = this.hints = document.createElement("ul");
	logger.error(`Widget(completion=${completion},data=${data}): hints → ${hints}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
	logger.debug(`Widget(completion=${completion},data=${data}): hints → ${hints}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    hints.className = "CodeMirror-hints";
    this.selectedHint = data.selectedHint || 0;

	logger.debug(`Widget(completion=${completion},data=${data}): hints.className → ${hints.className}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    var completions = data.list;
    for (var i = 0; i < completions.length; ++i) {
      var elt = hints.appendChild(document.createElement("li")), cur = completions[i];
	logger.error(`Widget(completion=${completion},data=${data}): elt → ${elt}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
	logger.debug(`Widget(completion=${completion},data=${data}): this.selectedHint → ${this.selectedHint}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
	logger.debug(`Widget(completion=${completion},data=${data}): elt.className → ${elt.className}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
      var className = HINT_ELEMENT_CLASS + (i != this.selectedHint ? "" : " " + ACTIVE_HINT_ELEMENT_CLASS);
      if (cur.className != null) className = cur.className + " " + className;
      elt.className = className;
      if (cur.render) cur.render(elt, data, cur);
      else elt.appendChild(document.createTextNode(cur.displayText || getText(cur)));
      elt.hintId = i;
	logger.debug(`Widget(completion=${completion},data=${data}): elt.hintId → ${elt.hintId}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    }

    var pos = cm.cursorCoords(completion.options.alignWithWord ? data.from : null);
	logger.error(`Widget(completion=${completion},data=${data}): pos → ${pos}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    var left = pos.left, top = pos.bottom, below = true;
    hints.style.left = left + "px";
    hints.style.top = top + "px";
    // If we're at the edge of the screen, then we want the menu to appear on the left of the cursor.
    var winW = window.innerWidth || Math.max(document.body.offsetWidth, document.documentElement.offsetWidth);
	logger.error(`Widget(completion=${completion},data=${data}): winW → ${winW}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    var winH = window.innerHeight || Math.max(document.body.offsetHeight, document.documentElement.offsetHeight);
	logger.error(`Widget(completion=${completion},data=${data}): winH → ${winH}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    (completion.options.container || document.body).appendChild(hints);
    var box = hints.getBoundingClientRect(), overlapY = box.bottom - winH;
	logger.error(`Widget(completion=${completion},data=${data}): box → ${box}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    if (overlapY > 0) {
      var height = box.bottom - box.top, curTop = pos.top - (pos.bottom - box.top);
	logger.error(`Widget(completion=${completion},data=${data}): height → ${height}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
      if (curTop - height > 0) { // Fits above cursor
        hints.style.top = (top = pos.top - height) + "px";
        below = false;
      } else if (height > winH) {
        hints.style.height = (winH - 5) + "px";
        hints.style.top = (top = pos.bottom - box.top) + "px";
        var cursor = cm.getCursor();
		logger.error(`Widget(completion=${completion},data=${data}): cursor → ${cursor}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
        if (data.from.ch != cursor.ch) {
          pos = cm.cursorCoords(cursor);
          hints.style.left = (left = pos.left) + "px";
          box = hints.getBoundingClientRect();
        }
      }
    }
    var overlapX = box.right - winW;
    if (overlapX > 0) {
      if (box.right - box.left > winW) {
        hints.style.width = (winW - 5) + "px";
        overlapX -= (box.right - box.left) - winW;
      }
      hints.style.left = (left = pos.left - overlapX) + "px";
    }

    cm.addKeyMap(this.keyMap = buildKeyMap(completion, {
      moveFocus: function(n, avoidWrap) {
          logger.error(`>>> Entering moveFocus(n=${n},avoidWrap=${avoidWrap})`, { "event.action": "moveFocus", "logrite.n": n, "logrite.avoidWrap": avoidWrap });
          logger.warn(`<<< Exiting moveFocus(n=${n},avoidWrap=${avoidWrap})`, { "event.action": "moveFocus", "logrite.n": n, "logrite.avoidWrap": avoidWrap });
          widget.changeActive(widget.selectedHint + n, avoidWrap);
      },
      setFocus: function(n) {
          logger.error(`>>> Entering setFocus(n=${n})`, { "event.action": "setFocus", "logrite.n": n });
          logger.warn(`<<< Exiting setFocus(n=${n})`, { "event.action": "setFocus", "logrite.n": n });
          widget.changeActive(n);
      },
      menuSize: function() {
          logger.error('>>> Entering menuSize()', { "event.action": "menuSize" });
          logger.warn('<<< Exiting menuSize()', { "event.action": "menuSize" });
          return widget.screenAmount();
      },
      length: completions.length,
      close: function() {
          logger.error('>>> Entering close()', { "event.action": "close" });
          logger.warn('<<< Exiting close()', { "event.action": "close" });
          completion.close();
      },
      pick: function() {
          logger.error('>>> Entering pick()', { "event.action": "pick" });
          logger.warn('<<< Exiting pick()', { "event.action": "pick" });
          widget.pick();
      },
      data: data
    }));

    if (completion.options.closeOnUnfocus) {
      var closingOnBlur;
      cm.on("blur", this.onBlur = function() { closingOnBlur = setTimeout(function() { completion.close(); }, 100); });
      cm.on("focus", this.onFocus = function() { clearTimeout(closingOnBlur); });
    }

    var startScroll = cm.getScrollInfo();
	logger.error(`Widget(completion=${completion},data=${data}): startScroll → ${startScroll}`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    cm.on("scroll", this.onScroll = function() {
	logger.error('>>> Entering onScroll()', { "event.action": "onScroll" });
      var curScroll = cm.getScrollInfo(), editor = cm.getWrapperElement().getBoundingClientRect();
      var newTop = top + startScroll.top - curScroll.top;
	logger.error(`onScroll(): curScroll → ${curScroll}`, { "event.action": "onScroll" });
      var point = newTop - (window.pageYOffset || (document.documentElement || document.body).scrollTop);
	logger.error(`onScroll(): point → ${point}`, { "event.action": "onScroll" });
      if (!below) point += hints.offsetHeight;
      if (point <= editor.top || point >= editor.bottom) return completion.close();
      hints.style.top = newTop + "px";
      hints.style.left = (left + startScroll.left - curScroll.left) + "px";
    });

    CodeMirror.on(hints, "dblclick", function(e) {
      var t = getHintElement(hints, e.target || e.srcElement);
      if (t && t.hintId != null) {widget.changeActive(t.hintId); widget.pick();}
    });

    CodeMirror.on(hints, "click", function(e) {
      var t = getHintElement(hints, e.target || e.srcElement);
      if (t && t.hintId != null) {
        widget.changeActive(t.hintId);
        if (completion.options.completeOnSingleClick) widget.pick();
      }
    });

	logger.warn(`<<< Exiting Widget(completion=${completion},data=${data})`, { "event.action": "Widget", "logrite.completion": completion, "logrite.data": data });
    CodeMirror.on(hints, "mousedown", function() {
      setTimeout(function(){cm.focus();}, 20);
    });

    CodeMirror.signal(data, "select", completions[0], hints.firstChild);
    return true;
  }

  Widget.prototype = {
    close: function() {
      if (this.completion.widget != this) return;
      this.completion.widget = null;
      this.hints.parentNode.removeChild(this.hints);
      this.completion.cm.removeKeyMap(this.keyMap);

      var cm = this.completion.cm;
      if (this.completion.options.closeOnUnfocus) {
        cm.off("blur", this.onBlur);
        cm.off("focus", this.onFocus);
      }
      cm.off("scroll", this.onScroll);
    },

    disable: function() {
	logger.error('>>> Entering disable()', { "event.action": "disable" });
      this.completion.cm.removeKeyMap(this.keyMap);
      var widget = this;
      this.keyMap = {Enter: function() { widget.picked = true; }};
      this.completion.cm.addKeyMap(this.keyMap);
	logger.warn('<<< Exiting disable()', { "event.action": "disable" });
    },

    pick: function() {
      this.completion.pick(this.data, this.selectedHint);
    },

    changeActive: function(i, avoidWrap) {
	logger.error(`>>> Entering changeActive(i=${i},avoidWrap=${avoidWrap})`, { "event.action": "changeActive", "logrite.i": i, "logrite.avoidWrap": avoidWrap });
      if (i >= this.data.list.length)
        i = avoidWrap ? this.data.list.length - 1 : 0;
      else if (i < 0)
        i = avoidWrap ? 0  : this.data.list.length - 1;
      if (this.selectedHint == i) return;
      var node = this.hints.childNodes[this.selectedHint];
      node.className = node.className.replace(" " + ACTIVE_HINT_ELEMENT_CLASS, "");
      node = this.hints.childNodes[this.selectedHint = i];
      node.className += " " + ACTIVE_HINT_ELEMENT_CLASS;
      if (node.offsetTop < this.hints.scrollTop)
        this.hints.scrollTop = node.offsetTop - 3;
      else if (node.offsetTop + node.offsetHeight > this.hints.scrollTop + this.hints.clientHeight)
        this.hints.scrollTop = node.offsetTop + node.offsetHeight - this.hints.clientHeight + 3;
      CodeMirror.signal(this.data, "select", this.data.list[this.selectedHint], node);
	logger.warn(`<<< Exiting changeActive(i=${i},avoidWrap=${avoidWrap})`, { "event.action": "changeActive", "logrite.i": i, "logrite.avoidWrap": avoidWrap });
    },

    screenAmount: function() {
	logger.error('>>> Entering screenAmount()', { "event.action": "screenAmount" });
	logger.warn('<<< Exiting screenAmount()', { "event.action": "screenAmount" });
      return Math.floor(this.hints.clientHeight / this.hints.firstChild.offsetHeight) || 1;
    }
  };

  CodeMirror.registerHelper("hint", "auto", function(cm, options) {
    var helpers = cm.getHelpers(cm.getCursor(), "hint"), words;
    if (helpers.length) {
      for (var i = 0; i < helpers.length; i++) {
        var cur = helpers[i](cm, options);
        if (cur && cur.list.length) return cur;
      }
    } else if (words = cm.getHelper(cm.getCursor(), "hintWords")) {
      if (words) return CodeMirror.hint.fromList(cm, {words: words});
    } else if (CodeMirror.hint.anyword) {
      return CodeMirror.hint.anyword(cm, options);
    }
  });

  CodeMirror.registerHelper("hint", "fromList", function(cm, options) {
    var cur = cm.getCursor(), token = cm.getTokenAt(cur);
    var to = CodeMirror.Pos(cur.line, token.end);
    if (token.string && /\w/.test(token.string[token.string.length - 1])) {
      var term = token.string, from = CodeMirror.Pos(cur.line, token.start);
    } else {
      var term = "", from = to;
    }
    var found = [];
    for (var i = 0; i < options.words.length; i++) {
      var word = options.words[i];
      if (word.slice(0, term.length) == term)
        found.push(word);
    }

    if (found.length) return {list: found, from: from, to: to};
  });

  CodeMirror.commands.autocomplete = CodeMirror.showHint;

  var defaultOptions = {
    hint: CodeMirror.hint.auto,
    completeSingle: true,
    alignWithWord: true,
    closeCharacters: /[\s()\[\]{};:>,]/,
    closeOnUnfocus: true,
    completeOnSingleClick: false,
    container: null,
    customKeys: null,
    extraKeys: null
  };

  CodeMirror.defineOption("hintOptions", null);
});
