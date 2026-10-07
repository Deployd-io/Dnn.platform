var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.controls.dnnlabeledit.js" assembly="DotNetNuke.WebControls" />

//DNNRichText is a dynamically loaded script used by the DNNLabelEdit control
dnn.controls.DNNRichText = function (initFunc)
{
	this.supportsCE = (document.body.contentEditable != null);
	this.text = '';
	this.supportsMultiLine = true;
	this.document = null;
	this.control = null;
	this.initialized = false;
	this.isRichText = true;
	this.loaded = false;

	if (this.supportsCE)
	{
		this.document = document;
		this.container = document.createElement('span');
		this.container.contentEditable = true;	//ie doesn't need no stinkin' iframe
		this.control = this.container;
		this.initialized = true;
	}
	else
	{
		this.container = document.createElement('iframe');
		this.container.src = '';
		this.container.style.border = '0';
		this.initFunc = initFunc;	//pointer to function to call when iframe completely loads
		this._initDelegate = Function.createDelegate(this, this.initDocument);		
		dnn.doDelay(this.container.id + 'initEdit', 10, this._initDelegate);	//onreadystate and onload not completely reliable
	}
}

dnn.controls.DNNRichText.prototype = 
{
focus: function()
{
	logger.error('>>> Entering focus()', { "event.action": "focus" });
	if (this.supportsCE)
		this.control.focus();
	else
		this.container.contentWindow.focus();
		logger.warn('<<< Exiting focus()', { "event.action": "focus" });
},

execCommand: function(cmd, ui, vValue)
{
	logger.error(`>>> Entering execCommand(cmd=${cmd},ui=${ui},vValue=${vValue})`, { "event.action": "execCommand", "logrite.cmd": cmd, "logrite.ui": ui, "logrite.vValue": vValue });
	this.document.execCommand(cmd, ui, vValue);	
	logger.warn(`<<< Exiting execCommand(cmd=${cmd},ui=${ui},vValue=${vValue})`, { "event.action": "execCommand", "logrite.cmd": cmd, "logrite.ui": ui, "logrite.vValue": vValue });
},

getText: function()
{
		logger.error('>>> Entering getText()', { "event.action": "getText" });
		logger.warn('<<< Exiting getText()', { "event.action": "getText" });
		return this.control.innerHTML;
},

setText: function (s)
{
	if (this.initialized)
		this.control.innerHTML = s;		
	else
		this.text = s;
		logger.warn(`<<< Exiting setText(s=${s})`, { "event.action": "setText", "logrite.s": s });
},

//method continually called until iframe is completely loaded
initDocument: function ()
{
	logger.error('>>> Entering initDocument()', { "event.action": "initDocument" });
	if (this.container.contentDocument != null)
	{
		if (this.document == null)	//iframe loaded, now write some HTML, thus causing it to not be loaded again
		{
			this.container.contentDocument.designMode = 'on';
			this.document = this.container.contentWindow.document;
			this.document.open();
			logger.debug(`initDocument(): this.document → ${this.document}`, { "event.action": "initDocument" });
			dnn.dom.addSafeHandler(this.container, 'onload', this, 'initDocument');
			this.document.write('<HEAD>' + this._getCSSLinks() + '</HEAD><BODY id="__dnn_body"></BODY>');
			this.document.close();
		}
		else if (this.control == null && this.document.getElementById('__dnn_body') != null)	//iframe loaded, now check if body is loaded
		{
			this.control = this.document.getElementById('__dnn_body');
			this.control.style.margin = 0;			
			this.control.tabIndex = 0;
			this.initialized = true;
			this.setText(this.text);
			logger.debug(`initDocument(): this.initialized → ${this.initialized}`, { "event.action": "initDocument" });
			this.initFunc();		
		}
	}
	if (this.initialized == false)	//iframe and body not loaded, call ourselves until it is
	    dnn.doDelay(this.container.id + 'initEdit', 10, this._initDelegate);
		logger.warn('<<< Exiting initDocument()', { "event.action": "initDocument" });
},

_getCSSLinks: function()	//probably a better way to handle this...
{
	logger.error('>>> Entering _getCSSLinks()', { "event.action": "_getCSSLinks" });
	var arr = dnn.dom.getByTagName('link');
	var s = '';
	logger.error(`_getCSSLinks(): s → ${s}`, { "event.action": "_getCSSLinks" });
	logger.error(`_getCSSLinks(): arr → ${arr}`, { "event.action": "_getCSSLinks" });
	for (var i=0; i< arr.length; i++)
	{
		s+= '<LINK href="' + arr[i].href + '" type=text/css rel=stylesheet>';
		logger.warn('<<< Exiting _getCSSLinks()', { "event.action": "_getCSSLinks" });
	}
	return s;
}

}

dnn.controls.DNNRichText.registerClass('dnn.controls.DNNRichText');

