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
  CodeMirror.defineExtension("addPanel", function(node, options) {
	logger.debug('>>> Entering defineExtension()');
    options = options || {};

    if (!this.state.panels) initPanels(this);

    var info = this.state.panels;
    var wrapper = info.wrapper;
    var cmWrapper = this.getWrapperElement();
	logger.debug(`defineExtension(): cmWrapper → ${cmWrapper}`);

    if (options.after instanceof Panel && !options.after.cleared) {
      wrapper.insertBefore(node, options.before.node.nextSibling);
    } else if (options.before instanceof Panel && !options.before.cleared) {
      wrapper.insertBefore(node, options.before.node);
    } else if (options.replace instanceof Panel && !options.replace.cleared) {
      wrapper.insertBefore(node, options.replace.node);
      options.replace.clear();
    } else if (options.position == "bottom") {
      wrapper.appendChild(node);
    } else if (options.position == "before-bottom") {
      wrapper.insertBefore(node, cmWrapper.nextSibling);
    } else if (options.position == "after-top") {
      wrapper.insertBefore(node, cmWrapper);
    } else {
      wrapper.insertBefore(node, wrapper.firstChild);
    }

    var height = (options && options.height) || node.offsetHeight;
	logger.debug(`defineExtension(): height → ${height}`);
    this._setSize(null, info.heightLeft -= height);
    info.panels++;
	logger.debug('<<< Exiting defineExtension()');
logger.debug(`<<< Exiting function(exports=${exports})`);
    return new Panel(this, node, options, height);
  });

  function Panel(cm, node, options, height) {
	logger.debug(`>>> Entering Panel(cm=${cm},node=${node},options=${options},height=${height})`);
    this.cm = cm;
    this.node = node;
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): cm → ${cm}`);
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): this.cm → ${this.cm}`);
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): node → ${node}`);
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): this.cleared → ${this.cleared}`);
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): height → ${height}`);
    this.options = options;
    this.height = height;
    this.cleared = false;
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): options → ${options}`);
	logger.debug(`<<< Exiting Panel(cm=${cm},node=${node},options=${options},height=${height})`);
  }

  Panel.prototype.clear = function() {
    if (this.cleared) return;
    this.cleared = true;
    var info = this.cm.state.panels;
	logger.debug(`function(): this.cleared → ${this.cleared}`);
    this.cm._setSize(null, info.heightLeft += this.height);
    info.wrapper.removeChild(this.node);
    if (--info.panels == 0) removePanels(this.cm);
  };

  Panel.prototype.changed = function(height) {
    var newHeight = height == null ? this.node.offsetHeight : height;
    var info = this.cm.state.panels;
    this.cm._setSize(null, info.height += (newHeight - this.height));
    this.height = newHeight;
	logger.debug(`function(height=${height}): this.height → ${this.height}`);
  };

  function initPanels(cm) {
	logger.debug(`>>> Entering initPanels(cm=${cm})`);
    var wrap = cm.getWrapperElement();
    var style = window.getComputedStyle ? window.getComputedStyle(wrap) : wrap.currentStyle;
	logger.debug(`initPanels(cm=${cm}): wrap → ${wrap}`);
	logger.debug(`initPanels(cm=${cm}): style → ${style}`);
    var height = parseInt(style.height);
    var info = cm.state.panels = {
      setHeight: wrap.style.height,
      heightLeft: height,
      panels: 0,
      wrapper: document.createElement("div")
    };
    wrap.parentNode.insertBefore(info.wrapper, wrap);
    var hasFocus = cm.hasFocus();
	logger.debug(`initPanels(cm=${cm}): hasFocus → ${hasFocus}`);
    info.wrapper.appendChild(wrap);
    if (hasFocus) cm.focus();

    cm._setSize = cm.setSize;
	logger.debug(`initPanels(cm=${cm}): cm._setSize → ${cm._setSize}`);
    if (height != null) cm.setSize = function(width, newHeight) {
	logger.debug(`<<< Exiting initPanels(cm=${cm})`);
      if (newHeight == null) return this._setSize(width, newHeight);
      info.setHeight = newHeight;
	logger.debug(`initPanels(cm=${cm}): info.setHeight → ${info.setHeight}`);
      if (typeof newHeight != "number") {
        var px = /^(\d+\.?\d*)px$/.exec(newHeight);
		logger.debug(`function(): px → ${px}`);
        if (px) {
          newHeight = Number(px[1]);
        } else {
          info.wrapper.style.height = newHeight;
          newHeight = info.wrapper.offsetHeight;
          info.wrapper.style.height = "";
        }
      }
      cm._setSize(width, info.heightLeft += (newHeight - height));
      height = newHeight;
    };
  }

  function removePanels(cm) {
	logger.debug(`>>> Entering removePanels(cm=${cm})`);
    var info = cm.state.panels;
    cm.state.panels = null;

    var wrap = cm.getWrapperElement();
	logger.debug(`removePanels(cm=${cm}): wrap → ${wrap}`);
    info.wrapper.parentNode.replaceChild(wrap, info.wrapper);
    wrap.style.height = info.setHeight;
    cm.setSize = cm._setSize;
	logger.debug(`removePanels(cm=${cm}): cm.setSize → ${cm.setSize}`);
    cm.setSize();
	logger.debug(`<<< Exiting removePanels(cm=${cm})`);
  }
});
