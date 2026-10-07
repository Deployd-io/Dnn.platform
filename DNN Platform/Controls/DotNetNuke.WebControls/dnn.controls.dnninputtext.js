var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.controls.dnnlabeledit.js" assembly="DotNetNuke.WebControls" />

//DNNInputText is a dynamically loaded script used by the DNNLabelEdit control
dnn.controls.DNNInputText = function (multiLine)
{
	if (multiLine)
		this.control = document.createElement('textarea');	
	else
	{
		this.control = document.createElement('input');
		this.control.type = 'text';
	}
	this.container = this.control;
	this.initialized = true;
	this.supportsMultiLine = multiLine;
	this.isRichText = false;
	this.loaded = false;
}

dnn.controls.DNNInputText.prototype = 
{
focus: function ()
{
	logger.error('>>> Entering focus()', { "event.action": "focus" });
	this.control.focus();
	var len = this.getText().length;
	logger.error(`focus(): len → ${len}`, { "event.action": "focus" });
	if (this.control.createTextRange)
	{
		var range = this.control.createTextRange();
		logger.error(`focus(): range → ${range}`, { "event.action": "focus" });
		range.moveStart('character', len);
		range.moveEnd('character', len);
		range.collapse();
		range.select();
	}
	else
	{
		this.control.selectionStart = len;
		this.control.selectionEnd = len;
		logger.warn('<<< Exiting focus()', { "event.action": "focus" });
	}
	//this.control.select();
},

ltrim: function (s) 
{ 
	logger.error(`>>> Entering ltrim(s=${s})`, { "event.action": "ltrim", "logrite.s": s });
	logger.warn(`<<< Exiting ltrim(s=${s})`, { "event.action": "ltrim", "logrite.s": s });
	return s.replace(/^\s*/, "");
},

rtrim: function (s) 
{ 
	logger.error(`>>> Entering rtrim(s=${s})`, { "event.action": "rtrim", "logrite.s": s });
	logger.warn(`<<< Exiting rtrim(s=${s})`, { "event.action": "rtrim", "logrite.s": s });
	return s.replace(/\s*$/, "");
},

getText: function ()
{
	logger.error('>>> Entering getText()', { "event.action": "getText" });
	logger.warn('<<< Exiting getText()', { "event.action": "getText" });
	return this.control.value;
},

setText: function (s)
{
	logger.error(`>>> Entering setText(s=${s})`, { "event.action": "setText", "logrite.s": s });
	this.control.value = this.rtrim(this.ltrim(s));
	logger.warn(`<<< Exiting setText(s=${s})`, { "event.action": "setText", "logrite.s": s });
}
}
dnn.controls.DNNInputText.registerClass('dnn.controls.DNNInputText');
