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

  var HINT_ELEMENT_CLASS        = "CodeMirror-hint";
  var ACTIVE_HINT_ELEMENT_CLASS = "CodeMirror-hint-active";
logger.debug(`function(exports=${exports}): ACTIVE_HINT_ELEMENT_CLASS → ${ACTIVE_HINT_ELEMENT_CLASS}`);

  // This is the old interface, kept around for now to stay
  // backwards-compatible.
  CodeMirror.showHint = function(cm, getHints, options) {
    if (!getHints) return cm.showHint(options);
    if (options && options.async) getHints.async = true;
    var newOpts = {hint: getHints};
    if (options) for (var prop in options) newOpts[prop] = options[prop];
	logger.debug(`<<< Exiting function(exports=${exports})`);
    return cm.showHint(newOpts);
  };

  CodeMirror.defineExtension("showHint", function(options) {
    // We want a single cursor position.
    if (this.listSelections().length > 1 || this.somethingSelected()) return;

    if (this.state.completionActive) this.state.completionActive.close();
    var completion = this.state.completionActive = new Completion(this, options);
	logger.debug(`defineExtension(completion=${completion}): completion → ${completion}`);
    if (!completion.options.hint) return;

    CodeMirror.signal(this, "startCompletion", this);
    completion.update(true);
  });
logger.debug(`<<< Exiting defineExtension(completion=${completion})`);

  function Completion(cm, options) {
	logger.debug(`>>> Entering Completion(cm=${cm},options=${options})`);
    this.cm = cm;
    this.options = this.buildOptions(options);
	logger.debug(`Completion(cm=${cm},options=${options}): cm → ${cm}`);
    this.widget = null;
    this.debounce = 0;
	logger.debug(`Completion(cm=${cm},options=${options}): options → ${options}`);
	logger.debug(`Completion(cm=${cm},options=${options}): this.cm → ${this.cm}`);
    this.tick = 0;
    this.startPos = this.cm.getCursor();
    this.startLen = this.cm.getLine(this.startPos.line).length;

    var self = this;
	logger.debug(`Completion(cm=${cm},options=${options}): this.widget → ${this.widget}`);
    cm.on("cursorActivity", this.activityFunc = function() { self.cursorActivity(); });
	logger.debug(`<<< Exiting Completion(cm=${cm},options=${options})`);
  }

  var requestAnimationFrame = window.requestAnimationFrame || function(fn) {
	logger.debug(`<<< Exiting function(fn=${fn})`);
    return setTimeout(fn, 1000/60);
  };
  var cancelAnimationFrame = window.cancelAnimationFrame || clearTimeout;

  Completion.prototype = {
    close: function() {
	logger.debug('>>> Entering close()');
      if (!this.active()) return;
      this.cm.state.completionActive = null;
      this.tick = null;
      this.cm.off("cursorActivity", this.activityFunc);
	logger.debug(`close(): this.tick → ${this.tick}`);

      if (this.widget && this.data) CodeMirror.signal(this.data, "close");
      if (this.widget) this.widget.close();
      CodeMirror.signal(this.cm, "endCompletion", this.cm);
	logger.debug('<<< Exiting close()');
    },

    active: function() {
	logger.debug('>>> Entering active()');
	logger.debug('<<< Exiting active()');
      return this.cm.state.completionActive == this;
    },

    pick: function(data, i) {
	logger.debug(`>>> Entering pick(data=${data},i=${i})`);
      var completion = data.list[i];
      if (completion.hint) completion.hint(this.cm, data, completion);
      else this.cm.replaceRange(getText(completion), completion.from || data.from,
                                completion.to || data.to, "complete");
      CodeMirror.signal(data, "pick", completion);
      this.close();
	logger.debug(`<<< Exiting pick(data=${data},i=${i})`);
	logger.debug('>>> Entering pick()');
	logger.debug('<<< Exiting pick()');
    },

    cursorActivity: function() {
	logger.debug('>>> Entering cursorActivity()');
      if (this.debounce) {
        cancelAnimationFrame(this.debounce);
        this.debounce = 0;
      }

	logger.debug(`cursorActivity(): this.debounce → ${this.debounce}`);
      var pos = this.cm.getCursor(), line = this.cm.getLine(pos.line);
      if (pos.line != this.startPos.line || line.length - pos.ch != this.startLen - this.startPos.ch ||
          pos.ch < this.startPos.ch || this.cm.somethingSelected() ||
          (pos.ch && this.options.closeCharacters.test(line.charAt(pos.ch - 1)))) {
		logger.debug(`cursorActivity(): pos → ${pos}`);
        this.close();
      } else {
        var self = this;
        this.debounce = requestAnimationFrame(function() {self.update();});
        if (this.widget) this.widget.disable();
      }
	logger.debug('<<< Exiting cursorActivity()');
    },

    update: function(first) {
	logger.debug(`>>> Entering update(first=${first})`);
      if (this.tick == null) return;
      if (!this.options.hint.async) {
        this.finishUpdate(this.options.hint(this.cm, this.options), first);
      } else {
        var myTick = ++this.tick, self = this;
        this.options.hint(this.cm, function(data) {
		logger.debug('>>> Entering hint()');
		logger.debug(`hint(): this.data → ${this.data}`);
          if (self.tick == myTick) self.finishUpdate(data, first);
        }, this.options);
      }
	logger.debug(`<<< Exiting update(first=${first})`);
    },

    finishUpdate: function(data, first) {
	logger.debug(`>>> Entering finishUpdate(data=${data},first=${first})`);
      if (this.data) CodeMirror.signal(this.data, "update");
      if (data && this.data && CodeMirror.cmpPos(data.from, this.data.from)) data = null;
      this.data = data;

      var picked = (this.widget && this.widget.picked) || (first && this.options.completeSingle);
      if (this.widget) this.widget.close();
	logger.debug(`finishUpdate(data=${data},first=${first}): picked → ${picked}`);
	logger.debug(`finishUpdate(data=${data},first=${first}): data → ${data}`);
      if (data && data.list.length) {
        if (picked && data.list.length == 1) {
          this.pick(data, 0);
        } else {
          this.widget = new Widget(this, data);
          CodeMirror.signal(data, "shown");
        }
      }
	logger.debug(`<<< Exiting finishUpdate(data=${data},first=${first})`);
    },

    buildOptions: function(options) {
	logger.debug(`>>> Entering buildOptions(options=${options})`);
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
	logger.debug(`>>> Entering getText(completion=${completion})`);
    if (typeof completion == "string") return completion;
    else return completion.text;
	logger.debug(`<<< Exiting getText(completion=${completion})`);
  }

  function buildKeyMap(completion, handle) {
	logger.debug(`>>> Entering buildKeyMap(completion=${completion},handle=${handle})`);
    var baseMap = {
      Up: function() {handle.moveFocus(-1);},
      Down: function() {handle.moveFocus(1);},
      PageUp: function() {handle.moveFocus(-handle.menuSize() + 1, true);},
      PageDown: function() {handle.moveFocus(handle.menuSize() - 1, true);},
      Home: function() {handle.setFocus(0);},
      End: function() {handle.setFocus(handle.length - 1);},
      Enter: handle.pick,
      Tab: handle.pick,
      Esc: handle.close
    };
    var custom = completion.options.customKeys;
    var ourMap = custom ? {} : baseMap;
    function addBinding(key, val) {
	logger.debug(`>>> Entering addBinding(key=${key},val=${val})`);
      var bound;
      if (typeof val != "string")
        bound = function(cm) { return val(cm, handle); };
      // This mechanism is deprecated
      else if (baseMap.hasOwnProperty(val))
        bound = baseMap[val];
      else
        bound = val;
      ourMap[key] = bound;
	logger.debug(`addBinding(key=${key},val=${val}): bound → ${bound}`);
	logger.debug(`<<< Exiting addBinding(key=${key},val=${val})`);
    }
    if (custom)
      for (var key in custom) if (custom.hasOwnProperty(key))
        addBinding(key, custom[key]);
    var extra = completion.options.extraKeys;
    if (extra)
      for (var key in extra) if (extra.hasOwnProperty(key))
        addBinding(key, extra[key]);
	logger.debug(`<<< Exiting buildKeyMap(completion=${completion},handle=${handle})`);
    return ourMap;
  }

  function getHintElement(hintsElement, el) {
	logger.debug(`>>> Entering getHintElement(hintsElement=${hintsElement},el=${el})`);
    while (el && el != hintsElement) {
      if (el.nodeName.toUpperCase() === "LI" && el.parentNode == hintsElement) return el;
	logger.debug(`<<< Exiting getHintElement(hintsElement=${hintsElement},el=${el})`);
      el = el.parentNode;
    }
  }

  function Widget(completion, data) {
	logger.debug(`>>> Entering Widget(completion=${completion},data=${data})`);
    this.completion = completion;
    this.data = data;
	logger.debug(`Widget(completion=${completion},data=${data}): completion → ${completion}`);
    this.picked = false;
    var widget = this, cm = completion.cm;
	logger.debug(`Widget(completion=${completion},data=${data}): data → ${data}`);
	logger.debug(`Widget(completion=${completion},data=${data}): this.completion → ${this.completion}`);
	logger.debug(`Widget(completion=${completion},data=${data}): this.data → ${this.data}`);

    var hints = this.hints = document.createElement("ul");
    hints.className = "CodeMirror-hints";
    this.selectedHint = data.selectedHint || 0;
	logger.debug(`Widget(completion=${completion},data=${data}): hints → ${hints}`);

    var completions = data.list;
	logger.debug(`Widget(completion=${completion},data=${data}): this.picked → ${this.picked}`);
    for (var i = 0; i < completions.length; ++i) {
      var elt = hints.appendChild(document.createElement("li")), cur = completions[i];
	logger.debug(`Widget(completion=${completion},data=${data}): elt → ${elt}`);
	logger.debug(`Widget(completion=${completion},data=${data}): hints.className → ${hints.className}`);
      var className = HINT_ELEMENT_CLASS + (i != this.selectedHint ? "" : " " + ACTIVE_HINT_ELEMENT_CLASS);
      if (cur.className != null) className = cur.className + " " + className;
      elt.className = className;
      if (cur.render) cur.render(elt, data, cur);
      else elt.appendChild(document.createTextNode(cur.displayText || getText(cur)));
      elt.hintId = i;
	logger.debug(`Widget(completion=${completion},data=${data}): this.selectedHint → ${this.selectedHint}`);
    }

	logger.debug(`Widget(completion=${completion},data=${data}): elt.className → ${elt.className}`);
	logger.debug(`Widget(completion=${completion},data=${data}): elt.hintId → ${elt.hintId}`);
    var pos = cm.cursorCoords(completion.options.alignWithWord ? data.from : null);
	logger.debug(`Widget(completion=${completion},data=${data}): pos → ${pos}`);
    var left = pos.left, top = pos.bottom, below = true;
    hints.style.left = left + "px";
    hints.style.top = top + "px";
    // If we're at the edge of the screen, then we want the menu to appear on the left of the cursor.
    var winW = window.innerWidth || Math.max(document.body.offsetWidth, document.documentElement.offsetWidth);
	logger.debug(`Widget(completion=${completion},data=${data}): winW → ${winW}`);
    var winH = window.innerHeight || Math.max(document.body.offsetHeight, document.documentElement.offsetHeight);
	logger.debug(`Widget(completion=${completion},data=${data}): winH → ${winH}`);
    (completion.options.container || document.body).appendChild(hints);
    var box = hints.getBoundingClientRect(), overlapY = box.bottom - winH;
	logger.debug(`Widget(completion=${completion},data=${data}): box → ${box}`);
    if (overlapY > 0) {
      var height = box.bottom - box.top, curTop = pos.top - (pos.bottom - box.top);
	logger.debug(`Widget(completion=${completion},data=${data}): height → ${height}`);
      if (curTop - height > 0) { // Fits above cursor
        hints.style.top = (top = pos.top - height) + "px";
        below = false;
      } else if (height > winH) {
        hints.style.height = (winH - 5) + "px";
        hints.style.top = (top = pos.bottom - box.top) + "px";
        var cursor = cm.getCursor();
		logger.debug(`Widget(completion=${completion},data=${data}): cursor → ${cursor}`);
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
      moveFocus: function(n, avoidWrap) { widget.changeActive(widget.selectedHint + n, avoidWrap); },
      setFocus: function(n) { widget.changeActive(n); },
      menuSize: function() { return widget.screenAmount(); },
      length: completions.length,
      close: function() { completion.close(); },
      pick: function() { widget.pick(); },
      data: data
    }));

    if (completion.options.closeOnUnfocus) {
      var closingOnBlur;
      cm.on("blur", this.onBlur = function() { closingOnBlur = setTimeout(function() { completion.close(); }, 100); });
      cm.on("focus", this.onFocus = function() { clearTimeout(closingOnBlur); });
    }

    var startScroll = cm.getScrollInfo();
	logger.debug(`Widget(completion=${completion},data=${data}): startScroll → ${startScroll}`);
    cm.on("scroll", this.onScroll = function() {
	logger.debug('>>> Entering on()');
      var curScroll = cm.getScrollInfo(), editor = cm.getWrapperElement().getBoundingClientRect();
      var newTop = top + startScroll.top - curScroll.top;
      var point = newTop - (window.pageYOffset || (document.documentElement || document.body).scrollTop);
      if (!below) point += hints.offsetHeight;
      if (point <= editor.top || point >= editor.bottom) return completion.close();
	logger.debug(`on(): curScroll → ${curScroll}`);
	logger.debug(`on(): point → ${point}`);
      hints.style.top = newTop + "px";
      hints.style.left = (left + startScroll.left - curScroll.left) + "px";
    });
	logger.debug('<<< Exiting on()');

    CodeMirror.on(hints, "dblclick", function(e) {
      var t = getHintElement(hints, e.target || e.srcElement);
	logger.debug(`on(hints=${hints},t=${t}): t → ${t}`);
      if (t && t.hintId != null) {widget.changeActive(t.hintId); widget.pick();}
    });

    CodeMirror.on(hints, "click", function(e) {
      var t = getHintElement(hints, e.target || e.srcElement);
      if (t && t.hintId != null) {
        widget.changeActive(t.hintId);
        if (completion.options.completeOnSingleClick) widget.pick();
      }
    });

    CodeMirror.on(hints, "mousedown", function() {
      setTimeout(function(){cm.focus();}, 20);
    });
	logger.debug(`on(hints=${hints}): Widget.prototype → ${Widget.prototype}`);

    CodeMirror.signal(data, "select", completions[0], hints.firstChild);
	logger.debug(`<<< Exiting Widget(completion=${completion},data=${data})`);
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
	logger.debug('>>> Entering disable()');
      this.completion.cm.removeKeyMap(this.keyMap);
      var widget = this;
      this.keyMap = {Enter: function() { widget.picked = true; }};
      this.completion.cm.addKeyMap(this.keyMap);
	logger.debug('<<< Exiting disable()');
    },

    pick: function() {
      this.completion.pick(this.data, this.selectedHint);
    },

    changeActive: function(i, avoidWrap) {
	logger.debug(`>>> Entering changeActive(i=${i},avoidWrap=${avoidWrap})`);
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
	logger.debug(`<<< Exiting changeActive(i=${i},avoidWrap=${avoidWrap})`);
    },

    screenAmount: function() {
	logger.debug('>>> Entering screenAmount()');
	logger.debug('<<< Exiting screenAmount()');
      return Math.floor(this.hints.clientHeight / this.hints.firstChild.offsetHeight) || 1;
    }
  };

  CodeMirror.registerHelper("hint", "auto", function(cm, options) {
    var helpers = cm.getHelpers(cm.getCursor(), "hint"), words;
	logger.debug(`>>> Entering registerHelper(cur=${cur})`);
    if (helpers.length) {
      for (var i = 0; i < helpers.length; i++) {
        var cur = helpers[i](cm, options);
        if (cur && cur.list.length) return cur;
      }
    } else if (words = cm.getHelper(cm.getCursor(), "hintWords")) {
      if (words) return CodeMirror.hint.fromList(cm, {words: words});
    } else if (CodeMirror.hint.anyword) {
	logger.debug(`<<< Exiting registerHelper(cur=${cur})`);
      return CodeMirror.hint.anyword(cm, options);
    }
  });

  CodeMirror.registerHelper("hint", "fromList", function(cm, options) {
    var cur = cm.getCursor(), token = cm.getTokenAt(cur);
	logger.debug(`registerHelper(cur=${cur}): cur → ${cur}`);
    var to = CodeMirror.Pos(cur.line, token.end);
    if (token.string && /\w/.test(token.string[token.string.length - 1])) {
	logger.debug(`registerHelper(cur=${cur}): to → ${to}`);
      var term = token.string, from = CodeMirror.Pos(cur.line, token.start);
    } else {
      var term = "", from = to;
	logger.debug(`registerHelper(cur=${cur}): term → ${term}`);
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
