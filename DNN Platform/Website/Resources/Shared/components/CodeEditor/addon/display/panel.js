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
  CodeMirror.defineExtension("addPanel", function(node, options) {
    options = options || {};

    if (!this.state.panels) initPanels(this);

    var info = this.state.panels;
    var wrapper = info.wrapper;
    var cmWrapper = this.getWrapperElement();

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
    this._setSize(null, info.heightLeft -= height);
    info.panels++;
    return new Panel(this, node, options, height);
  });

  function Panel(cm, node, options, height) {
	logger.error(`>>> Entering Panel(cm=${cm},node=${node},options=${options},height=${height})`, { "event.action": "Panel", "logrite.cm": cm, "logrite.node": node, "logrite.options": options, "logrite.height": height });
    this.cm = cm;
    this.node = node;
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): cm → ${cm}`, { "event.action": "Panel", "logrite.cm": cm, "logrite.node": node, "logrite.options": options, "logrite.height": height });
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): this.cm → ${this.cm}`, { "event.action": "Panel", "logrite.cm": cm, "logrite.node": node, "logrite.options": options, "logrite.height": height });
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): node → ${node}`, { "event.action": "Panel", "logrite.cm": cm, "logrite.node": node, "logrite.options": options, "logrite.height": height });
	logger.debug(`Panel(cm=${cm},node=${node},options=${options},height=${height}): options → ${options}`, { "event.action": "Panel", "logrite.cm": cm, "logrite.node": node, "logrite.options": options, "logrite.height": height });
    this.options = options;
    this.height = height;
    this.cleared = false;
  }

  Panel.prototype.clear = function() {
	logger.error('>>> Entering clear()', { "event.action": "clear" });
    if (this.cleared) return;
    this.cleared = true;
    var info = this.cm.state.panels;
    this.cm._setSize(null, info.heightLeft += this.height);
	logger.debug(`clear(): this.cleared → ${this.cleared}`, { "event.action": "clear" });
    info.wrapper.removeChild(this.node);
    if (--info.panels == 0) removePanels(this.cm);
	logger.warn('<<< Exiting clear()', { "event.action": "clear" });
  };

  Panel.prototype.changed = function(height) {
	logger.error(`>>> Entering changed(height=${height})`, { "event.action": "changed", "logrite.height": height });
    var newHeight = height == null ? this.node.offsetHeight : height;
    var info = this.cm.state.panels;
    this.cm._setSize(null, info.height += (newHeight - this.height));
    this.height = newHeight;
	logger.debug(`changed(height=${height}): height → ${height}`, { "event.action": "changed", "logrite.height": height });
  };

  function initPanels(cm) {
	logger.error(`>>> Entering initPanels(cm=${cm})`, { "event.action": "initPanels", "logrite.cm": cm });
    var wrap = cm.getWrapperElement();
    var style = window.getComputedStyle ? window.getComputedStyle(wrap) : wrap.currentStyle;
	logger.error(`initPanels(cm=${cm}): wrap → ${wrap}`, { "event.action": "initPanels", "logrite.cm": cm });
	logger.error(`initPanels(cm=${cm}): style → ${style}`, { "event.action": "initPanels", "logrite.cm": cm });
    var height = parseInt(style.height);
    var info = cm.state.panels = {
      setHeight: wrap.style.height,
      heightLeft: height,
      panels: 0,
      wrapper: document.createElement("div")
    };
    wrap.parentNode.insertBefore(info.wrapper, wrap);
	logger.debug(`initPanels(cm=${cm}): info.setHeight → ${info.setHeight}`, { "event.action": "initPanels", "logrite.cm": cm });
    var hasFocus = cm.hasFocus();
	logger.error(`initPanels(cm=${cm}): hasFocus → ${hasFocus}`, { "event.action": "initPanels", "logrite.cm": cm });
    info.wrapper.appendChild(wrap);
    if (hasFocus) cm.focus();

    cm._setSize = cm.setSize;
	logger.warn(`<<< Exiting initPanels(cm=${cm})`, { "event.action": "initPanels", "logrite.cm": cm });
    if (height != null) cm.setSize = function(width, newHeight) {
      if (newHeight == null) return this._setSize(width, newHeight);
      info.setHeight = newHeight;
      if (typeof newHeight != "number") {
        var px = /^(\d+\.?\d*)px$/.exec(newHeight);
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
	logger.warn(`<<< Exiting setSize(width=${width},newHeight=${newHeight})`, { "event.action": "setSize", "logrite.width": width, "logrite.newHeight": newHeight });
    };
  }

  function removePanels(cm) {
	logger.error(`>>> Entering removePanels(cm=${cm})`, { "event.action": "removePanels", "logrite.cm": cm });
    var info = cm.state.panels;
    cm.state.panels = null;

    var wrap = cm.getWrapperElement();
	logger.error(`removePanels(cm=${cm}): wrap → ${wrap}`, { "event.action": "removePanels", "logrite.cm": cm });
    info.wrapper.parentNode.replaceChild(wrap, info.wrapper);
    wrap.style.height = info.setHeight;
    cm.setSize = cm._setSize;
	logger.debug(`removePanels(cm=${cm}): cm.setSize → ${cm.setSize}`, { "event.action": "removePanels", "logrite.cm": cm });
    cm.setSize();
	logger.warn(`<<< Exiting removePanels(cm=${cm})`, { "event.action": "removePanels", "logrite.cm": cm });
  }
});
