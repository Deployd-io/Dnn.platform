var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.dom.positioning.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.controls');

dnn.extend(dnn.controls, {
    initMultiStateBox: function (ctl) 
    {    	
		logger.error(`>>> Entering initMultiStateBox(ctl=${ctl})`, { "event.action": "initMultiStateBox", "logrite.ctl": ctl });
	    if (ctl)
	    {
		    var ts = new dnn.controls.DNNMultiStateBox(ctl);
		    ts.initialize();
			logger.error(`initMultiStateBox(ctl=${ctl}): ts → ${ts}`, { "event.action": "initMultiStateBox", "logrite.ctl": ctl });
			logger.warn(`<<< Exiting initMultiStateBox(ctl=${ctl})`, { "event.action": "initMultiStateBox", "logrite.ctl": ctl });
		    return ts;
	    }
    }
});

//------- Constructor -------//
dnn.controls.DNNMultiStateBox = function(o)
{
	dnn.controls.DNNMultiStateBox.initializeBase(this, [o]);	

	//--- Appearance Properties ---//
	this.css = this.getProp('css', '');

    this.enabled = (this.getProp('enabled', '1') == '1');
	this.imgPath = this.getProp('imgpath', 'images/');
	this.states = this.getProp('states', '[]');
	this.states = Sys.Serialization.JavaScriptSerializer.deserialize(this.states);
	this.stateIndex = []
	
	for (var i=0; i<this.states.length; i++)
	    this.stateIndex[this.states[i].Key] = i;
	    
	this.js = this.getProp('js', '');	
	
    this._img = document.createElement('img');
    //this._img.style.position = 'absolute';
    this.container.parentNode.insertBefore(this._img, this.container);
    this.container.checked = true;
    //this.container.style.display = 'none';
    this.container.style.position = 'absolute';
    this.container.style.top = '-999px';
    this._label = dnn.dom.getByTagName('label', this.container.parentNode);
    if (this._label)
    {
        for (var i=0; i<this._label.length; i++)
        {
            if (this._label[i].htmlFor == this.container.id)
            {
                this._label = this._label[i];
                break;
            }
        }
    }
    else
        this._img.tabIndex = 0;
        
    this.set_Value(this.get_Value());
    
    this.addHandlers(this.container, {'click': this.click}, this);	
    this.addHandlers(this._img, {'click': this.click, 'keypress': this.keypress}, this);	

}

dnn.controls.DNNMultiStateBox.prototype = 
{
get_Value: function() {
    logger.error('>>> Entering get_Value()', { "event.action": "get_Value" });
    logger.warn('<<< Exiting get_Value()', { "event.action": "get_Value" });
    return this.container.value;
},
set_Value: function(value) 
{
	logger.error(`>>> Entering set_Value(value=${value})`, { "event.action": "set_Value", "logrite.value": value });
    var index = this.stateIndex[value];
    var state = this.states[index];

    if (this.enabled)
        this._img.src = this.imgPath + state.ImageUrl;
    else    
        this._img.src = this.imgPath + state.DisabledImageUrl;
    this._img.alt = state.ToolTip;
    if (this._label)
        this._label.innerHTML = state.Text;
    
    this.container.value = state.Key;
	logger.warn(`<<< Exiting set_Value(value=${value})`, { "event.action": "set_Value", "logrite.value": value });
},

//--- Event Handlers ---//
click: function(sender, args)
{
	logger.error(`>>> Entering click(sender=${sender},args=${args})`, { "event.action": "click", "logrite.sender": sender, "logrite.args": args });
    if (this.enabled)
    {
        var index = this.stateIndex[this.get_Value()] + 1;
        if (index > this.states.length - 1)
            index = 0;
        this.set_Value(this.states[index].Key);
    }
    this.container.checked = true;
	logger.warn(`<<< Exiting click(sender=${sender},args=${args})`, { "event.action": "click", "logrite.sender": sender, "logrite.args": args });
},

keypress: function(sender, args)
{
logger.error(`>>> Entering keypress(sender=${sender},args=${args})`, { "event.action": "keypress", "logrite.sender": sender, "logrite.args": args });
debugger;
logger.warn(`<<< Exiting keypress(sender=${sender},args=${args})`, { "event.action": "keypress", "logrite.sender": sender, "logrite.args": args });
},

// Methods
dispose: function()
{
	logger.error('>>> Entering dispose()', { "event.action": "dispose" });
    dnn.controls.DNNMultiStateBox.callBaseMethod(this, 'dispose');
	logger.warn('<<< Exiting dispose()', { "event.action": "dispose" });
}

}

dnn.controls.DNNMultiStateBox.registerClass('dnn.controls.DNNMultiStateBox', dnn.controls.control);
