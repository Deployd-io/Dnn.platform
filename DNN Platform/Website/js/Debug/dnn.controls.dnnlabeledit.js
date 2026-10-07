var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.dom.positioning.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.controls');

dnn.extend(dnn.controls, {
    initLabelEdit: function (ctl) 
    {
		logger.error(`>>> Entering initLabelEdit(ctl=${ctl})`, { "event.action": "initLabelEdit", "logrite.ctl": ctl });
        if (ctl && dnn.controls.controls[ctl.id] == null)
	    {
		    var lbl = new dnn.controls.DNNLabelEdit(ctl);
		    lbl.initialize();
			logger.error(`initLabelEdit(ctl=${ctl}): lbl → ${lbl}`, { "event.action": "initLabelEdit", "logrite.ctl": ctl });
			logger.warn(`<<< Exiting initLabelEdit(ctl=${ctl})`, { "event.action": "initLabelEdit", "logrite.ctl": ctl });
		    return lbl;
	    }
    }
});

//------- Constructor -------//
dnn.controls.DNNLabelEdit = function (o)
{
	dnn.controls.DNNLabelEdit.initializeBase(this, [o]);	

    this.control = this.container;//backwards compat
    
	this.editWrapper = null;	//stores dnn wrapper for abstracted edit control
	this.editContainer = null; //stores container of the control (necessary for iframe controls)
	this.editControl = null; //stores reference to underlying edit control (input, span, textarea)
	this.prevText = '';	
		
	this.onblurSave = (this.getProp('blursave', '1') == '1');
	
	this.toolbarId = this.getProp('tbId', '');
	this.nsPrefix = this.getProp('nsPrefix', '');
	this.toolbarEventName = this.getProp('tbEvent', 'onmousemove');
	this.toolbar = null;
	//this.scriptPath = dnn.dom.getScriptPath();
	//var oThisScript = dnn.dom.getScript('dnn.controls.dnnlabeledit.js');
	//if (oThisScript)
	//	this.scriptPath = oThisScript.src.replace('dnn.controls.dnnlabeledit.js', '');
		
	this.css = o.className;	
	this.cssEdit = this.getProp('cssEdit', '');
	this.cssWork = this.getProp('cssWork', '');
	this.cssOver = this.getProp('cssOver', '');
	this.sysImgPath = this.getProp('sysimgpath', '');
	this.callBack = this.getProp('callback', '');
	this.callBackStatFunc = this.getProp('callbackSF', '');
	if (this.callBackStatFunc.length > 0)
	    this.add_handler('callBackStatus', eval(this.callBackStatFunc));
	
	this.beforeSaveFunc = this.getProp('beforeSaveF', '');
	if (this.beforeSaveFunc.length > 0)
	    this.add_handler('beforeSave', eval(this.beforeSaveFunc));
	    
	this.eventName = this.getProp('eventName', 'onclick');
	//this.editEnabled = dnn.dom.getAttr(o, 'editEnabled', '1') == '1';
	this.multiLineEnabled = this.getProp('multiline', '0') == '1';
	this.saveOnEnter = this.getProp('saveonenter', '1') == '1';
	this.richTextEnabled = this.getProp('richtext', '0') == '1';
	this.supportsCE = (document.body.contentEditable != null);
	if (dnn.dom.browser.isType(dnn.dom.browser.Safari) || dnn.dom.browser.isType(dnn.dom.browser.Opera))		
		this.supportsCE = false;//Safari content editable still buggy...
	this.supportsRichText = (this.supportsCE || (dnn.dom.browser.isType(dnn.dom.browser.Mozilla) && navigator.productSub >= '20050111'));	//i belive firefox only works well with 1.5 or later, need a better way to detect this!
	
	if (this.eventName != 'none')
	    this.addHandlers(o, this.getDynamicEventObject(this._getEventName(this.eventName), this.performEdit), this);
	
	if (this.toolbarId.length > 0)
	    this.addHandlers(o, this.getDynamicEventObject(this._getEventName(this.toolbarEventName), this.showToolBar), this);

    this.addHandlers(o, {'mousemove': this.mouseMove, 
                        'mouseout': this.mouseOut}, this);

    this._toolbarActionDelegate = dnn.createDelegate(this, this.toolbarAction);                        
    this._initToolbarDelegate = dnn.createDelegate(this, this.initToolbar);
    this._performEditDelegate = dnn.createDelegate(this, this.performEdit);
}

dnn.controls.DNNLabelEdit.prototype =
{

    isEditMode: function()
    {
		logger.error('>>> Entering isEditMode()', { "event.action": "isEditMode" });
		logger.warn('<<< Exiting isEditMode()', { "event.action": "isEditMode" });
        return (this.container.style.display != '')
    },

    initToolbar: function()
    {
		logger.error('>>> Entering initToolbar()', { "event.action": "initToolbar" });
        if (this.toolbar == null)
        {
            var status = dnn.dom.scriptStatus('dnn.controls.dnntoolbar.js');
			logger.error(`initToolbar(): status → ${status}`, { "event.action": "initToolbar" });
            if (status == 'complete')
            {
                this.toolbar = new dnn.controls.DNNToolBar(this.ns);
                this.toolbar.loadDefinition(this.toolbarId, this.nsPrefix, this.container, this.container.parentNode, this.container, this._toolbarActionDelegate);
                this.handleToolbarDisplay();
            }
            else if (status == '')	//not loaded
                dnn.dom.loadScript(dnn.dom.getScriptPath() + 'dnn.controls.dnntoolbar.js', '', this._initToolbarDelegate);
		logger.warn('<<< Exiting initToolbar()', { "event.action": "initToolbar" });
        }

    },

    toolbarAction: function(btn, src)
    {
		logger.error(`>>> Entering toolbarAction(btn=${btn},src=${src})`, { "event.action": "toolbarAction", "logrite.btn": btn, "logrite.src": src });
        var action = btn.clickAction;
        if (action == 'edit')
            this.performEdit();
        else if (action == 'save')
        {
            this.persistEdit();
            this.toolbar.hide();
        }
        else if (action == 'cancel')
        {
            this.cancelEdit();
            this.toolbar.hide();
        }
        else if (this.isFormatButton(action))
        {
            if (this.editWrapper)
            {
                var link;
                if (action == 'createlink' && dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer) == false)
                    link = prompt(btn.tooltip);

				logger.error(`toolbarAction(btn=${btn},src=${src}): link → ${link}`, { "event.action": "toolbarAction", "logrite.btn": btn, "logrite.src": src });
				logger.debug(`toolbarAction(btn=${btn},src=${src}): link → ${link}`, { "event.action": "toolbarAction", "logrite.btn": btn, "logrite.src": src });
                this.editWrapper.focus();
                this.editWrapper.execCommand(action, null, link);
            }
		logger.warn(`<<< Exiting toolbarAction(btn=${btn},src=${src})`, { "event.action": "toolbarAction", "logrite.btn": btn, "logrite.src": src });
        }

    },

    performEdit: function()
    {
		logger.error('>>> Entering performEdit()', { "event.action": "performEdit" });
        if (this.toolbar)
            this.toolbar.hide();

        this.initEditWrapper();
        if (this.editContainer != null)
        {
            //this.editContainer.style.height = (dnn.dom.positioning.elementHeight(this.container) + 4) + 'px';
            //var dims = new dnn.dom.positioning.dims(this.container.parentNode);
            //this.editContainer.style.width = (dims.w - dims.l) + 'px';
            //this.editContainer.style.width = dnn.dom.positioning.elementWidth(this.container.parentNode) + 'px' //'100%';
            this.editContainer.style.width = '100%';
            this.editContainer.style.display = '';
            //this.editContainer.style.visibility = '';	//firefox workaround... can't do display none
            this.editContainer.style.overflow = 'auto';
            this.editContainer.style.overflowX = 'hidden';

            this.prevText = this.container.innerHTML;
            if (dnn.dom.browser.isType(dnn.dom.browser.Safari) && this.container.innerText)		//safari gets strange chars... use innerText
                this.prevText = this.container.innerText;
				logger.debug(`performEdit(): this.prevText → ${this.prevText}`, { "event.action": "performEdit" });
            this.editWrapper.setText(this.prevText);
            this.initEditControl();
            this.container.style.display = 'none';
            this.handleToolbarDisplay();

            var arg = new Sys.CancelEventArgs();
			logger.error(`performEdit(): arg → ${arg}`, { "event.action": "performEdit" });
            this.invoke_handler('beforeEdit', arg);
            if (arg.get_cancel())
            {
                this.cancelEdit();
                return;
            }

		logger.warn('<<< Exiting performEdit()', { "event.action": "performEdit" });
        }
    },

    showToolBar: function()
    {
		logger.error('>>> Entering showToolBar()', { "event.action": "showToolBar" });
        this.initToolbar();
        if (this.toolbar)
            this.toolbar.show(false);
    },

    mouseMove: function(e)
    {
		logger.error(`>>> Entering mouseMove(e=${e})`, { "event.action": "mouseMove", "logrite.e": e });
        if (this.toolbarId.length > 0 && this.toolbarEventName == 'onmousemove')
            this.showToolBar();
        this.container.className = this.css + ' ' + this.cssOver;
		logger.warn(`<<< Exiting mouseMove(e=${e})`, { "event.action": "mouseMove", "logrite.e": e });
    },

    mouseOut: function()
    {
		logger.error('>>> Entering mouseOut()', { "event.action": "mouseOut" });
        if (this.toolbar)
            this.toolbar.beginHide();
        this.container.className = this.css;
		logger.warn('<<< Exiting mouseOut()', { "event.action": "mouseOut" });
    },

    initEditWrapper: function()
    {
		logger.error('>>> Entering initEditWrapper()', { "event.action": "initEditWrapper" });
        if (this.editWrapper == null)
        {
            var richText = (this.richTextEnabled && this.supportsRichText);
			logger.error(`initEditWrapper(): richText → ${richText}`, { "event.action": "initEditWrapper" });
            var script = (richText ? 'dnn.controls.dnnrichtext.js' : 'dnn.controls.dnninputtext.js');
			logger.error(`initEditWrapper(): script → ${script}`, { "event.action": "initEditWrapper" });

            var status = dnn.dom.scriptStatus(script);
			logger.error(`initEditWrapper(): status → ${status}`, { "event.action": "initEditWrapper" });
            if (status == 'complete')
            {
                var txt;
                if (this.richTextEnabled && this.supportsRichText)
                {
                    var func = dnn.dom.getObjMethRef(this, 'initEditControl');
					logger.error(`initEditWrapper(): func → ${func}`, { "event.action": "initEditWrapper" });
                    txt = new dnn.controls.DNNRichText(func);
                }
                else
                    txt = new dnn.controls.DNNInputText(this.multiLineEnabled);

				logger.error(`initEditWrapper(): txt → ${txt}`, { "event.action": "initEditWrapper" });
				logger.debug(`initEditWrapper(): txt → ${txt}`, { "event.action": "initEditWrapper" });
                this.editWrapper = txt;
				logger.debug(`initEditWrapper(): this.editWrapper → ${this.editWrapper}`, { "event.action": "initEditWrapper" });
                this.editContainer = this.editWrapper.container;
                this.container.parentNode.insertBefore(this.editContainer, this.container);
                if (this.richTextEnabled && this.supportsCE)	//control is instantly available (not an iframe)
                    this.initEditControl();
            }
            else if (status == '') //not loaded
                dnn.dom.loadScript(dnn.dom.getScriptPath() + script, '', this._performEditDelegate);
				logger.warn('<<< Exiting initEditWrapper()', { "event.action": "initEditWrapper" });
        }
    },

    initEditControl: function()
    {
		logger.error('>>> Entering initEditControl()', { "event.action": "initEditControl" });
        if (this.editWrapper.initialized)
        {
            this.editControl = this.editWrapper.control;
            this.editControl.className = this.container.className + ' ' + this.cssEdit;
			logger.debug(`initEditControl(): this.editControl → ${this.editControl}`, { "event.action": "initEditControl" });
            this.editWrapper.focus();
            if (this.editWrapper.loaded == false)
            {
                var eventObj = { keypress: this.handleKeyPress,
                    mousemove: this.mouseMove,
                    mouseout: this.mouseOut
                };

				logger.debug(`initEditControl(): eventObj.blur → ${eventObj.blur}`, { "event.action": "initEditControl" });
                if (this.onblurSave)
                    eventObj.blur = this.persistEdit;

                if (this.editWrapper.supportsCE || this.editWrapper.isRichText == false)	//if browser supports contentEditable or is a simple INPUT control
                    this.addHandlers(this.editControl, eventObj, this);
                else	//IFRAME event handlers
                    this.addHandlers(this.editContainer.contentWindow.document, eventObj, this);

                this.editWrapper.loaded = true;
				logger.warn('<<< Exiting initEditControl()', { "event.action": "initEditControl" });
            }
        }
    },

    persistEdit: function()
    {
		logger.error('>>> Entering persistEdit()', { "event.action": "persistEdit" });
        if (this.editWrapper.getText() != this.prevText)
        {
            if (this.invoke_compatHandler('beforeSave', null, this))
            {
                this.editControl.className = this.container.className + ' ' + this.cssWork;
                eval(this.callBack.replace('[TEXT]', dnn.escapeForEval(this.editWrapper.getText())));
				logger.warn('<<< Exiting persistEdit()', { "event.action": "persistEdit" });
            }
        }
        else
            this.showLabel();
    },

    cancelEdit: function()
    {
		logger.error('>>> Entering cancelEdit()', { "event.action": "cancelEdit" });
        this.editWrapper.setText(this.prevText);
        this.showLabel();
		logger.warn('<<< Exiting cancelEdit()', { "event.action": "cancelEdit" });
    },

    callBackStatus: function(result, ctx, req)
    {
		logger.error(`>>> Entering callBackStatus(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackStatus", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        var lbl = ctx;
        lbl.invoke_compatHandler('callBackStatus', result, ctx, req);
		logger.warn(`<<< Exiting callBackStatus(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackStatus", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },

    callBackSuccess: function(result, ctx, req)
    {
		logger.error(`>>> Entering callBackSuccess(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        ctx.callBackStatus(result, ctx);
        ctx.invoke_handler('callBackSuccess', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
        ctx.showLabel();
	    ctx.flashLabel();
		logger.warn(`<<< Exiting callBackSuccess(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },

    //obsolete
    raiseEvent: function(sFunc, evt, element)
    {
		logger.error(`>>> Entering raiseEvent(sFunc=${sFunc},evt=${evt},element=${element})`, { "event.action": "raiseEvent", "logrite.sFunc": sFunc, "logrite.evt": evt, "logrite.element": element });
        if (this[sFunc].length > 0)
        {
            var ptr = eval(this[sFunc]);
			logger.warn(`<<< Exiting raiseEvent(sFunc=${sFunc},evt=${evt},element=${element})`, { "event.action": "raiseEvent", "logrite.sFunc": sFunc, "logrite.evt": evt, "logrite.element": element });
			logger.error(`raiseEvent(sFunc=${sFunc},evt=${evt},element=${element}): ptr → ${ptr}`, { "event.action": "raiseEvent", "logrite.sFunc": sFunc, "logrite.evt": evt, "logrite.element": element });
            return ptr(evt, element) != false;
        }
        return true;
    },


    handleToolbarDisplay: function()
    {
		logger.error('>>> Entering handleToolbarDisplay()', { "event.action": "handleToolbarDisplay" });
        if (this.toolbar)
        {
            var inEdit = this.isEditMode();
			logger.error(`handleToolbarDisplay(): inEdit → ${inEdit}`, { "event.action": "handleToolbarDisplay" });
            var btn;
            for (var key in this.toolbar.buttons)
            {
                btn = this.toolbar.buttons[key];
                if (typeof btn == 'function')	//safeguard against other js frameworks
                    continue;

	            if (key == 'edit') {
	            	btn.visible = !inEdit;
		            if (btn.visible) {
			            this.toolbar.css = this.toolbar.css.replace(" editMode", "");
		            } else {
			            this.toolbar.css = this.toolbar.css + " editMode";
		            }
	            } else if (this.isFormatButton(key))
		            btn.visible = (inEdit && this.editWrapper && this.editWrapper.isRichText);
	            else
		            btn.visible = inEdit;

            }
            this.toolbar.refresh();
			logger.warn('<<< Exiting handleToolbarDisplay()', { "event.action": "handleToolbarDisplay" });
        }
    },

    isFormatButton: function(key)
    {
		logger.error(`>>> Entering isFormatButton(key=${key})`, { "event.action": "isFormatButton", "logrite.key": key });
		logger.warn(`<<< Exiting isFormatButton(key=${key})`, { "event.action": "isFormatButton", "logrite.key": key });
        return '~bold~italic~underline~justifyleft~justifycenter~justifyright~insertorderedlist~insertunorderedlist~outdent~indent~createlink~'.indexOf('~' + key + '~') > -1;
    },

    showLabel: function()
    {
		logger.error('>>> Entering showLabel()', { "event.action": "showLabel" });
        this.container.innerHTML = this.editWrapper.getText();
        this.container.style.display = '';
        this.container.className = this.css;
        //this.editContainer.style.width = 0; //firefox workaround
        //this.editContainer.style.visibility = 'hidden';	//firefox workaround
        this.editContainer.style.display = 'none';
        this.handleToolbarDisplay();
		logger.warn('<<< Exiting showLabel()', { "event.action": "showLabel" });
    },
    
    flashLabel: function () {
		logger.error('>>> Entering flashLabel()', { "event.action": "flashLabel" });
    	var handler = this;
	    this.container.style.backgroundColor = "#fffacd";
	    setTimeout(function () {
		    handler.container.style.backgroundColor = "#fffff0";
		    setTimeout(function () {
			    handler.container.style.backgroundColor = "transparent";
            }, 300);
        }, 2500);
		logger.warn('<<< Exiting flashLabel()', { "event.action": "flashLabel" });
	},

    callBackFail: function(result, ctx, req)
    {
		logger.error(`>>> Entering callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        ctx.invoke_handler('callBackFail', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
        ctx.cancelEdit();
		logger.warn(`<<< Exiting callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },

    handleKeyPress: function(e)
    {
        if (e.charCode == KEY_RETURN && this.editWrapper.supportsMultiLine == false)
        {
            if (this.saveOnEnter)
                this.persistEdit();
            e.preventDefault();
        }
        else if (e.charCode == KEY_ESCAPE)
        {
            this.cancelEdit();
            e.preventDefault();
        }
    },

    dispose: function()
    {
		logger.error('>>> Entering dispose()', { "event.action": "dispose" });
        this._toolbarActionDelegate = null;
        this._initToolbarDelegate = null;
		logger.debug(`dispose(): this._toolbarActionDelegate → ${this._toolbarActionDelegate}`, { "event.action": "dispose" });
        this._performEditDelegate = null;
        dnn.controls.DNNLabelEdit.callBaseMethod(this, 'dispose');
		logger.debug(`dispose(): this._initToolbarDelegate → ${this._initToolbarDelegate}`, { "event.action": "dispose" });
    }

}

dnn.controls.DNNLabelEdit.registerClass('dnn.controls.DNNLabelEdit', dnn.controls.control);