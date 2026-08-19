const logger = console;
// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// Open simple dialogs on top of an editor. Relies on dialog.css.

(function(mod) {
logger.debug(`>>> Entering function(exports=${exports})`);
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
	logger.debug(`<<< Exiting function(exports=${exports})`);
})(function(CodeMirror) {
  function dialogDiv(cm, template, bottom) {
	logger.debug(`>>> Entering dialogDiv(cm=${cm},template=${template},bottom=${bottom})`);
    var wrap = cm.getWrapperElement();
    var dialog;
	logger.debug(`dialogDiv(cm=${cm},template=${template},bottom=${bottom}): wrap → ${wrap}`);
    dialog = wrap.appendChild(document.createElement("div"));
    if (bottom)
      dialog.className = "CodeMirror-dialog CodeMirror-dialog-bottom";
    else
      dialog.className = "CodeMirror-dialog CodeMirror-dialog-top";

    if (typeof template == "string") {
	logger.debug(`dialogDiv(cm=${cm},template=${template},bottom=${bottom}): dialog.className → ${dialog.className}`);
      dialog.innerHTML = template;
	logger.debug(`dialogDiv(cm=${cm},template=${template},bottom=${bottom}): dialog.innerHTML → ${dialog.innerHTML}`);
    } else { // Assuming it's a detached DOM element.
      dialog.appendChild(template);
    }
	logger.debug(`<<< Exiting dialogDiv(cm=${cm},template=${template},bottom=${bottom})`);
    return dialog;
  }

  function closeNotification(cm, newVal) {
	logger.debug(`>>> Entering closeNotification(cm=${cm},newVal=${newVal})`);
    if (cm.state.currentNotificationClose)
      cm.state.currentNotificationClose();
    cm.state.currentNotificationClose = newVal;
	logger.debug(`<<< Exiting closeNotification(cm=${cm},newVal=${newVal})`);
  }

  CodeMirror.defineExtension("openDialog", function(template, callback, options) {
	logger.debug('>>> Entering defineExtension()');
    if (!options) options = {};

    closeNotification(this, null);

    var dialog = dialogDiv(this, template, options.bottom);
	logger.debug('<<< Exiting defineExtension()');
    var closed = false, me = this;
    function close(newVal) {
	logger.debug(`>>> Entering close(newVal=${newVal})`);
      if (typeof newVal == 'string') {
        inp.value = newVal;
      } else {
        if (closed) return;
        closed = true;
        dialog.parentNode.removeChild(dialog);
        me.focus();

        if (options.onClose) options.onClose(dialog);
		logger.debug(`<<< Exiting close(newVal=${newVal})`);
      }
	logger.debug('>>> Entering close()');
	logger.debug('<<< Exiting close()');
    }

    var inp = dialog.getElementsByTagName("input")[0], button;
    if (inp) {
      if (options.value) {
        inp.value = options.value;
        if (options.selectValueOnOpen !== false) {
          inp.select();
        }
      }

      if (options.onInput)
        CodeMirror.on(inp, "input", function(e) { options.onInput(e, inp.value, close);});
      if (options.onKeyUp)
        CodeMirror.on(inp, "keyup", function(e) {options.onKeyUp(e, inp.value, close);});

      CodeMirror.on(inp, "keydown", function(e) {
		logger.debug(`>>> Entering on(inp=${inp})`);
        if (options && options.onKeyDown && options.onKeyDown(e, inp.value, close)) { return; }
        if (e.keyCode == 27 || (options.closeOnEnter !== false && e.keyCode == 13)) {
          inp.blur();
          CodeMirror.e_stop(e);
          close();
        }
		logger.debug(`<<< Exiting on(inp=${inp})`);
        if (e.keyCode == 13) callback(inp.value, e);
      });

      if (options.closeOnBlur !== false) CodeMirror.on(inp, "blur", close);

      inp.focus();
    } else if (button = dialog.getElementsByTagName("button")[0]) {
      CodeMirror.on(button, "click", function() {
        close();
        me.focus();
      });

      if (options.closeOnBlur !== false) CodeMirror.on(button, "blur", close);

      button.focus();
    }
    return close;
  });

  CodeMirror.defineExtension("openConfirm", function(template, callbacks, options) {
    closeNotification(this, null);
    var dialog = dialogDiv(this, template, options && options.bottom);
    var buttons = dialog.getElementsByTagName("button");
    var closed = false, me = this, blurring = 1;
	logger.debug(`defineExtension(): buttons → ${buttons}`);
    function close() {
      if (closed) return;
      closed = true;
      dialog.parentNode.removeChild(dialog);
      me.focus();
    }
    buttons[0].focus();
    for (var i = 0; i < buttons.length; ++i) {
      var b = buttons[i];
      (function(callback) {
        CodeMirror.on(b, "click", function(e) {
          CodeMirror.e_preventDefault(e);
          close();
          if (callback) callback(me);
        });
      })(callbacks[i]);
      CodeMirror.on(b, "blur", function() {
        --blurring;
        setTimeout(function() { if (blurring <= 0) close(); }, 200);
      });
      CodeMirror.on(b, "focus", function() { ++blurring; });
    }
  });

  /*
   * openNotification
   * Opens a notification, that can be closed with an optional timer
   * (default 5000ms timer) and always closes on click.
   *
   * If a notification is opened while another is opened, it will close the
   * currently opened one and open the new one immediately.
   */
  CodeMirror.defineExtension("openNotification", function(template, options) {
    closeNotification(this, close);
    var dialog = dialogDiv(this, template, options && options.bottom);
	logger.debug(`defineExtension(): dialog → ${dialog}`);
    var closed = false, doneTimer;
    var duration = options && typeof options.duration !== "undefined" ? options.duration : 5000;

	logger.debug(`defineExtension(): duration → ${duration}`);
    function close() {
      if (closed) return;
      closed = true;
      clearTimeout(doneTimer);
      dialog.parentNode.removeChild(dialog);
    }

    CodeMirror.on(dialog, 'click', function(e) {
      CodeMirror.e_preventDefault(e);
      close();
    });

    if (duration)
      doneTimer = setTimeout(close, duration);

    return close;
  });
});
