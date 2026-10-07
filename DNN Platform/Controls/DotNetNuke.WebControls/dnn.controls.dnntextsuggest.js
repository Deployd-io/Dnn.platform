var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.dom.positioning.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.controls');

dnn.extend(dnn.controls, {
    initTextSuggest: function (ctl) 
    {    	
		logger.error(`>>> Entering initTextSuggest(ctl=${ctl})`, { "event.action": "initTextSuggest", "logrite.ctl": ctl });
	    if (ctl)
	    {
		    var ts = new dnn.controls.DNNTextSuggest(ctl);
		    ts.initialize();
			logger.error(`initTextSuggest(ctl=${ctl}): ts → ${ts}`, { "event.action": "initTextSuggest", "logrite.ctl": ctl });
			logger.warn(`<<< Exiting initTextSuggest(ctl=${ctl})`, { "event.action": "initTextSuggest", "logrite.ctl": ctl });
		    return ts;
	    }
    }
});

//------- Constructor -------//
dnn.controls.DNNTextSuggest = function(o)
{
	dnn.controls.DNNTextSuggest.initializeBase(this, [o]);	

	this.resultCtr = null;
	//this.DOM = null;

	//--- Appearance Properties ---//
	this.tscss = this.getProp('tscss', '');
	this.css = this.getProp('css', '');
	this.cssChild = this.getProp('csschild', '');
	this.cssHover = this.getProp('csshover', '');
	this.cssSel = this.getProp('csssel', '');

	this.sysImgPath = this.getProp('sysimgpath', '');
	this.workImg = 'dnnanim.gif';
		
	this.target = this.getProp('target', '');	
	this.defaultJS = this.getProp('js', '');	
	
	this.postBack = this.getProp('postback', '');
	this.callBack = this.getProp('callback', '');
	this.callBackStatFunc = this.getProp('callbackSF', '');
	if (this.callBackStatFunc.length > 0)
	    this.add_handler('callBackStatus', eval(this.callBackStatFunc));
		
	this.rootNode=null;
	this.selNode=null;  
	this.selIndex=-1;
	//this.delay = new Array();
	this.lookupDelay = this.getProp('ludelay', '500');
	this.lostFocusDelay = this.getProp('lfdelay', '500');

	//this.anim = dnn.dom.getAttr(o, 'anim', '');	//expand
	this.inAnimObj = null;
	this.inAnimType = null;
	this.prevText = '';	
	
    this.addHandlers(o, {'keyup': this.keyUp, 
                        'keypress': this.keyPress,
                        'blur': this.onblur,
                        'focus': this.onfocus}, this);	
	
	o.setAttribute('autocomplete', 'off');	
	this.delimiter = this.getProp('del', '');
	this.idtoken = this.getProp('idtok', '');
	this.maxRows = new Number(this.getProp('maxRows', '10'));
	
	if (this.maxRows == 0)
		this.maxRows = 9999;
	this.minChar = new Number(this.getProp('minChar', '1'));
	this.caseSensitive = this.getProp('casesens', '0') == '1';

	this.prevLookupText = '';
	this.prevLookupOffset = 0;	
	
	this._blurHideDelegate = dnn.createDelegate(this, this.blurHide);
	this._doLookupDelegate = dnn.createDelegate(this, this.doLookup);
}

dnn.controls.DNNTextSuggest.prototype = 
{
//--- Event Handlers ---//
keyPress: function (e) 
{
	logger.error(`>>> Entering keyPress(e=${e})`, { "event.action": "keyPress", "logrite.e": e });
	if(e.charCode == KEY_RETURN)	//stop from posting
	{
	    e.stopPropagation();
	    e.preventDefault();
		logger.warn(`<<< Exiting keyPress(e=${e})`, { "event.action": "keyPress", "logrite.e": e });
		return false;
	}
},

onblur: function (e) 
{
	dnn.doDelay(this.ns + 'ob', this.lostFocusDelay, this._blurHideDelegate);
},

onfocus: function (e) 
{
	dnn.cancelDelay(this.ns + 'ob');
},

blurHide: function(e)
{
	logger.error(`>>> Entering blurHide(e=${e})`, { "event.action": "blurHide", "logrite.e": e });
	this.clearResults(true);
	logger.warn(`<<< Exiting blurHide(e=${e})`, { "event.action": "blurHide", "logrite.e": e });
},

keyUp: function (e) 
{
	logger.error(`>>> Entering keyUp(e=${e})`, { "event.action": "keyUp", "logrite.e": e });
	this.prevText = this.container.value;
	if (e.keyCode == KEY_UP_ARROW)
		this.setNodeIndex(this.selIndex - 1);
	else if(e.keyCode == KEY_DOWN_ARROW)
		this.setNodeIndex(this.selIndex + 1);
	else if(e.keyCode == KEY_RETURN)
	{
		if (this.selIndex > -1)
		{
			this.selectNode(this.getNodeByIndex(this.selIndex));
			this.clearResults(true);
		}
	}
	else if(e.keyCode == KEY_ESCAPE)
		this.clearResults(true);
	else
	{
		dnn.cancelDelay(this.ns + 'kd');
		dnn.doDelay(this.ns + 'kd', this.lookupDelay, this._doLookupDelegate);	
	}
		
		logger.warn(`<<< Exiting keyUp(e=${e})`, { "event.action": "keyUp", "logrite.e": e });
},

nodeMOver: function(evt)
{
	logger.error(`>>> Entering nodeMOver(evt=${evt})`, { "event.action": "nodeMOver", "logrite.evt": evt });
	var node = this._findEventNode(evt);
	logger.error(`nodeMOver(evt=${evt}): node → ${node}`, { "event.action": "nodeMOver", "logrite.evt": evt });
	if (node != null)
	{
		var tsNode = new dnn.controls.DNNTextSuggestNode(node);
		logger.error(`nodeMOver(evt=${evt}): tsNode → ${tsNode}`, { "event.action": "nodeMOver", "logrite.evt": evt });
		tsNode.hover = true;
		logger.debug(`nodeMOver(evt=${evt}): tsNode.hover → ${tsNode.hover}`, { "event.action": "nodeMOver", "logrite.evt": evt });
		this.assignCss(tsNode);
		logger.warn(`<<< Exiting nodeMOver(evt=${evt})`, { "event.action": "nodeMOver", "logrite.evt": evt });
	}
},

nodeMOut: function(evt)
{
	logger.error(`>>> Entering nodeMOut(evt=${evt})`, { "event.action": "nodeMOut", "logrite.evt": evt });
	var node = this._findEventNode(evt);
	logger.error(`nodeMOut(evt=${evt}): node → ${node}`, { "event.action": "nodeMOut", "logrite.evt": evt });
	if (node != null)
	{
		var tsNode = new dnn.controls.DNNTextSuggestNode(node);
		logger.error(`nodeMOut(evt=${evt}): tsNode → ${tsNode}`, { "event.action": "nodeMOut", "logrite.evt": evt });
		tsNode.hover = false;
		logger.debug(`nodeMOut(evt=${evt}): tsNode.hover → ${tsNode.hover}`, { "event.action": "nodeMOut", "logrite.evt": evt });
		this.assignCss(tsNode);
		logger.warn(`<<< Exiting nodeMOut(evt=${evt})`, { "event.action": "nodeMOut", "logrite.evt": evt });
	}
},

nodeClick: function(evt)
{
	logger.error(`>>> Entering nodeClick(evt=${evt})`, { "event.action": "nodeClick", "logrite.evt": evt });
	var node = this._findEventNode(evt);
	logger.error(`nodeClick(evt=${evt}): node → ${node}`, { "event.action": "nodeClick", "logrite.evt": evt });
	if (node != null)
	{
		var tsNode = new dnn.controls.DNNTextSuggestNode(node);
		logger.error(`nodeClick(evt=${evt}): tsNode → ${tsNode}`, { "event.action": "nodeClick", "logrite.evt": evt });
		this.selectNode(tsNode);
		this.clearResults(true);
		logger.warn(`<<< Exiting nodeClick(evt=${evt})`, { "event.action": "nodeClick", "logrite.evt": evt });
	}
},

// Methods
getTextOffset: function () 
{
	logger.error('>>> Entering getTextOffset()', { "event.action": "getTextOffset" });
	var offset = 0;
	if (this.delimiter.length > 0)
	{
		var ary = this.container.value.split(this.delimiter);
		logger.error(`getTextOffset(): ary → ${ary}`, { "event.action": "getTextOffset" });
		var pos = dnn.dom.cursorPos(this.container);
		logger.error(`getTextOffset(): pos → ${pos}`, { "event.action": "getTextOffset" });
		var len = 0;
		for (offset=0; offset<ary.length-1; offset++)
		{
			len += ary[offset].length + 1;
			if (len > pos)
				break;
		}
	}
	return offset;
},

setText: function (s, id) 
{
	logger.error(`>>> Entering setText(s=${s},id=${id})`, { "event.action": "setText", "logrite.s": s, "logrite.id": id });
	if (this.idtoken.length > 0)
		s += ' ' + this.idtoken.replace('~', id);
		
	if (this.delimiter.length > 0)
	{
		var ary = this.container.value.split(this.delimiter);
		logger.error(`setText(s=${s},id=${id}): ary → ${ary}`, { "event.action": "setText", "logrite.s": s, "logrite.id": id });
		ary[this.getTextOffset()] = s;

		this.container.value = ary.join(this.delimiter);
		if (this.container.value.lastIndexOf(this.delimiter) != this.container.value.length - 1)
			this.container.value += this.delimiter;
		
	}
	else
		this.container.value = s;

	this.prevText = this.container.value;
	logger.debug(`setText(s=${s},id=${id}): this.prevText → ${this.prevText}`, { "event.action": "setText", "logrite.s": s, "logrite.id": id });
},

getText: function () 
{
	logger.error('>>> Entering getText()', { "event.action": "getText" });
	if (this.delimiter.length > 0 && this.container.value.indexOf(this.delimiter) > -1)
	{
		var ary = this.container.value.split(this.delimiter);
		logger.warn('<<< Exiting getText()', { "event.action": "getText" });
		logger.error(`getText(): ary → ${ary}`, { "event.action": "getText" });
		return ary[this.getTextOffset()];
	}
	else
		return this.container.value;
},

formatText: function (s) 
{
	logger.error(`>>> Entering formatText(s=${s})`, { "event.action": "formatText", "logrite.s": s });
	if (this.caseSensitive)
		return s;
	else
		return s.toLowerCase();
},

highlightNode: function(index, bHighlight)
{
	logger.error(`>>> Entering highlightNode(index=${index},bHighlight=${bHighlight})`, { "event.action": "highlightNode", "logrite.index": index, "logrite.bHighlight": bHighlight });
	if (index > -1)
	{
		var tsNode = this.getNodeByIndex(index);
		logger.error(`highlightNode(index=${index},bHighlight=${bHighlight}): tsNode → ${tsNode}`, { "event.action": "highlightNode", "logrite.index": index, "logrite.bHighlight": bHighlight });
		tsNode.hover = bHighlight;
		logger.debug(`highlightNode(index=${index},bHighlight=${bHighlight}): tsNode.hover → ${tsNode.hover}`, { "event.action": "highlightNode", "logrite.index": index, "logrite.bHighlight": bHighlight });
		this.assignCss(tsNode);				
		logger.warn(`<<< Exiting highlightNode(index=${index},bHighlight=${bHighlight})`, { "event.action": "highlightNode", "logrite.index": index, "logrite.bHighlight": bHighlight });
	}
},

getNodeByIndex: function(index)
{
	logger.error(`>>> Entering getNodeByIndex(index=${index})`, { "event.action": "getNodeByIndex", "logrite.index": index });
	var element = this.resultCtr.childNodes[index];
	if (element)
	{
		var node = this.rootNode.findNode(this.getChildControlBaseId(element));
		if (node)
			return new dnn.controls.DNNTextSuggestNode(node);
	}
},

setNodeIndex: function(index)
{
	logger.error(`>>> Entering setNodeIndex(index=${index})`, { "event.action": "setNodeIndex", "logrite.index": index });
	if (index > -1 && index < this.resultCtr.childNodes.length)
	{
		this.highlightNode(this.selIndex, false);
		this.selIndex = index;
		this.highlightNode(this.selIndex, true);
		logger.warn(`<<< Exiting setNodeIndex(index=${index})`, { "event.action": "setNodeIndex", "logrite.index": index });
		logger.debug(`setNodeIndex(index=${index}): this.selIndex → ${this.selIndex}`, { "event.action": "setNodeIndex", "logrite.index": index });
	}
},

selectNode: function (tsNode) 
{		
	logger.error(`>>> Entering selectNode(tsNode=${tsNode})`, { "event.action": "selectNode", "logrite.tsNode": tsNode });
    var arg = new dnn.controls.DNNNodeEventArgs(tsNode);
    this.invoke_handler('click', arg);
	logger.error(`selectNode(tsNode=${tsNode}): arg → ${arg}`, { "event.action": "selectNode", "logrite.tsNode": tsNode });
    if (arg.get_cancel())
        return;

	if (this.selNode != null)
	{
		this.selNode.selected = null;
		this.assignCss(this.selNode);
	}		
	
	if (tsNode.selected)
	{
		tsNode.selected = null;
		logger.debug(`selectNode(tsNode=${tsNode}): tsNode.selected → ${tsNode.selected}`, { "event.action": "selectNode", "logrite.tsNode": tsNode });
		this.assignCss(tsNode);
	}
	else
	{
		tsNode.selected = true;
		this.assignCss(tsNode);
	}
	
	this.selNode = tsNode;

	logger.debug(`selectNode(tsNode=${tsNode}): this.selNode → ${this.selNode}`, { "event.action": "selectNode", "logrite.tsNode": tsNode });
	if (tsNode.selected)
	{
		this.setText(tsNode.text, tsNode.id);
		var js = '';
		if (this.defaultJS.length > 0)
			js = this.defaultJS;
		if (tsNode.js.length > 0)
			js = tsNode.js;
		
		if (js.length > 0)
		{
			if (eval(js) == false)
				return;	//don't do postback if returns false
		}
		
		if (tsNode.clickAction == null || tsNode.clickAction == dnn.controls.action.postback)
			eval(this.postBack.replace('[TEXT]', this.getText()));
		else if (tsNode.clickAction == dnn.controls.action.nav)
			dnn.dom.navigate(tsNode.url, tsNode.target.length > 0 ? tsNode.target : this.target);
	}
	return true;		
},

positionMenu: function ()
{
	logger.error('>>> Entering positionMenu()', { "event.action": "positionMenu" });
	var dims = new dnn.dom.positioning.dims(this.container);
	this.resultCtr.style.left = dims.l - dnn.dom.positioning.bodyScrollLeft();			
	logger.error(`positionMenu(): dims → ${dims}`, { "event.action": "positionMenu" });
	this.resultCtr.style.top = dims.t + dims.h;
	
	logger.warn('<<< Exiting positionMenu()', { "event.action": "positionMenu" });
},

showResults: function()
{
	logger.error('>>> Entering showResults()', { "event.action": "showResults" });
	if (this.resultCtr)
		this.resultCtr.style.display = '';
},

hideResults: function()
{
	logger.error('>>> Entering hideResults()', { "event.action": "hideResults" });
	if (this.resultCtr)
		this.resultCtr.style.display = 'none';
    dnn.dom.positioning.placeOnTop(this.resultCtr, false, this.sysImgPath + 'spacer.gif');  
	logger.warn('<<< Exiting hideResults()', { "event.action": "hideResults" });
},

clearResults: function (hide) 
{
	logger.error(`>>> Entering clearResults(hide=${hide})`, { "event.action": "clearResults", "logrite.hide": hide });
	if (this.resultCtr != null)
		this.resultCtr.innerHTML = '';
	this.selIndex = -1;
	this.selNode = null;
	logger.debug(`clearResults(hide=${hide}): this.selIndex → ${this.selIndex}`, { "event.action": "clearResults", "logrite.hide": hide });
	logger.debug(`clearResults(hide=${hide}): this.selNode → ${this.selNode}`, { "event.action": "clearResults", "logrite.hide": hide });
	if (hide)
		this.hideResults();
},

clear: function () 
{
	logger.error('>>> Entering clear()', { "event.action": "clear" });
	this.clearResults();
	this.setText('', '');
	logger.warn('<<< Exiting clear()', { "event.action": "clear" });
},

doLookup: function () 
{
	logger.error('>>> Entering doLookup()', { "event.action": "doLookup" });
	if (this.getText().length >= this.minChar)
	{
		if (this.needsLookup())
		{
			this.prevLookupOffset = this.getTextOffset();
			this.prevLookupText = this.formatText(this.getText());
			eval(this.callBack.replace('[TEXT]', this.prevLookupText));
			logger.warn('<<< Exiting doLookup()', { "event.action": "doLookup" });
		}
		else
			this.renderResults(null);
	}
	else
		this.clearResults();
},

needsLookup: function () 
{
	logger.error('>>> Entering needsLookup()', { "event.action": "needsLookup" });
	if (this.rootNode == null)
		return true;

	if (this.prevLookupOffset != this.getTextOffset() || this.rootNode == null)
		return true;

	if (this.formatText(this.getText()).indexOf(this.prevLookupText) == 0)	//if starts with previous lookup
	{
		if (this.rootNode.childNodeCount() < this.maxRows)	//if rows are less than max, don't need lookup
			return false;
		
		var node;
		var oneMatch = false;
		var text = this.getText();
		logger.error(`needsLookup(): text → ${text}`, { "event.action": "needsLookup" });
		for (var i=0; i<this.maxRows; i++)
		{
			node = new dnn.controls.DNNTextSuggestNode(this.rootNode.childNodes(i));
			logger.error(`needsLookup(): node → ${node}`, { "event.action": "needsLookup" });
			if (this.formatText(node.text).indexOf(text) == 0)	
			{
				if (i==0)	//if first shown node hasn't changed
					return false;
				else 
					oneMatch = true;
			}
			else if (oneMatch)	//if found match and a row following has no match then we dont need lookup
				return false;
		}		
	logger.warn('<<< Exiting needsLookup()', { "event.action": "needsLookup" });
	}

	return true;

},

renderResults: function (result) 
{
	logger.error(`>>> Entering renderResults(result=${result})`, { "event.action": "renderResults", "logrite.result": result });
	if (result != null)
	{
	    var json = dnn.evalJSON("{" + result + "}");
		logger.error(`renderResults(result=${result}): json → ${json}`, { "event.action": "renderResults", "logrite.result": result });
        this.nodes = json.nodes;
        this.rootNode = {};
        this.rootNode.nodes = this.nodes;
		logger.debug(`renderResults(result=${result}): this.nodes → ${this.nodes}`, { "event.action": "renderResults", "logrite.result": result });
        this.rootNode.id = this.ns;
        this.rootNode = new dnn.controls.JSONNode(this.rootNode, 'root', 0);
		logger.debug(`renderResults(result=${result}): this.rootNode → ${this.rootNode}`, { "event.action": "renderResults", "logrite.result": result });
		
	}
	if (this.rootNode != null)
	{
		if (this.resultCtr == null)
			this.renderContainer();
			
		this.clearResults();
		for (var i=0; i<this.rootNode.childNodeCount(); i++)
			this.renderNode(this.rootNode.childNodes(i), this.resultCtr);
		this.showResults();
	}
	logger.warn(`<<< Exiting renderResults(result=${result})`, { "event.action": "renderResults", "logrite.result": result });
    dnn.dom.positioning.placeOnTop(this.resultCtr, true, this.sysImgPath + 'spacer.gif');  

},

renderContainer: function () 
{
	logger.error('>>> Entering renderContainer()', { "event.action": "renderContainer" });
	this.resultCtr = document.createElement('DIV');
	this.container.parentNode.appendChild(this.resultCtr);

	this.resultCtr.className = this.tscss;
	this.resultCtr.style.position = 'absolute';
	this.positionMenu();
	logger.warn('<<< Exiting renderContainer()', { "event.action": "renderContainer" });
},

renderNode: function (node, ctr) 
{
	var tsNode;		
	tsNode = new dnn.controls.DNNTextSuggestNode(node);
	//text must be prefixed by value we are looking for and we must be under the maxRows
	if (this.formatText(tsNode.text).indexOf(this.formatText(this.getText())) == 0 && ctr.childNodes.length < this.maxRows)
	{
		var newCtr = this.createChildControl('div', tsNode.id, 'ctr'); //container for Node
		newCtr.appendChild(this.renderText(tsNode));	//render text

		if (tsNode.enabled)
		{
			this.addHandlers(newCtr, {'click': this.nodeClick,
			                          'mouseover': this.nodeMOver,
			                          'mouseout': this.nodeMOut}, this);
		}

		if (tsNode.toolTip.length > 0)
			newCtr.title = tsNode.toolTip;
			
		ctr.appendChild(newCtr);
		this.assignCss(tsNode);
	}
	logger.warn(`<<< Exiting renderNode(node=${node},ctr=${ctr})`, { "event.action": "renderNode", "logrite.node": node, "logrite.ctr": ctr });
},

renderText: function (tsNode) 
{
	logger.error(`>>> Entering renderText(tsNode=${tsNode})`, { "event.action": "renderText", "logrite.tsNode": tsNode });
	var span = this.createChildControl('SPAN', tsNode.id, 't'); //document.createElement('SPAN');
	span.innerHTML = tsNode.text;	
	logger.error(`renderText(tsNode=${tsNode}): span → ${span}`, { "event.action": "renderText", "logrite.tsNode": tsNode });
	logger.debug(`renderText(tsNode=${tsNode}): span.innerHTML → ${span.innerHTML}`, { "event.action": "renderText", "logrite.tsNode": tsNode });
	span.style.cursor = 'pointer';
	
	logger.warn(`<<< Exiting renderText(tsNode=${tsNode})`, { "event.action": "renderText", "logrite.tsNode": tsNode });
	return span;
},

assignCss: function (tsNode)
{
	logger.error(`>>> Entering assignCss(tsNode=${tsNode})`, { "event.action": "assignCss", "logrite.tsNode": tsNode });
	var oCtr = this.getChildControl(tsNode.id, 'ctr'); //dnn.dom.getById(__dm_getControlID(this.ns, tsNode.id, 'ctr'));//, this.container);
	var nodeCss = this.css;
	logger.error(`assignCss(tsNode=${tsNode}): oCtr → ${oCtr}`, { "event.action": "assignCss", "logrite.tsNode": tsNode });

	if (tsNode.css.length > 0)
		nodeCss = tsNode.css;

	if (tsNode.hover)
		nodeCss += ' ' + (tsNode.cssHover.length > 0 ? tsNode.cssHover : this.cssHover);
	if (tsNode.selected)
		nodeCss += ' ' + (tsNode.cssSel.length > 0 ? tsNode.cssSel : this.cssSel);
	
	oCtr.className = nodeCss;
	logger.debug(`assignCss(tsNode=${tsNode}): oCtr.className → ${oCtr.className}`, { "event.action": "assignCss", "logrite.tsNode": tsNode });
},

callBackStatus: function (result, ctx, req) 
{
	logger.error(`>>> Entering callBackStatus(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackStatus", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	var ts = ctx;
	ts.invoke_compatHandler('callBackStatus', result, ctx, req);	
	logger.warn(`<<< Exiting callBackStatus(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackStatus", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
},

callBackSuccess: function (result, ctx, req) 
{
	logger.error(`>>> Entering callBackSuccess(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	var ts = ctx;
	ts.invoke_compatHandler('callBackStatus', result, ctx, req);	
	ts.invoke_handler('callBackSuccess', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));	
	logger.warn(`<<< Exiting callBackSuccess(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	ts.renderResults(result);

},

callBackFail: function (result, ctx, req) 
{
	logger.error(`>>> Entering callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	var ts = ctx;
	ts.invoke_handler('callBackFail', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));	
	logger.warn(`<<< Exiting callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	//alert(result);
},

_findEventNode: function(evt)
{
	logger.error(`>>> Entering _findEventNode(evt=${evt})`, { "event.action": "_findEventNode", "logrite.evt": evt });
	logger.warn(`<<< Exiting _findEventNode(evt=${evt})`, { "event.action": "_findEventNode", "logrite.evt": evt });
    return this.rootNode.findNode(this.getChildControlBaseId(evt.target));
},

dispose: function()
{
	logger.error('>>> Entering dispose()', { "event.action": "dispose" });
    this._blurHideDelegate = null;
    this._doLookupDelegate = null;
	logger.debug(`dispose(): this._blurHideDelegate → ${this._blurHideDelegate}`, { "event.action": "dispose" });
	logger.debug(`dispose(): this._doLookupDelegate → ${this._doLookupDelegate}`, { "event.action": "dispose" });
    dnn.controls.DNNTextSuggest.callBaseMethod(this, 'dispose');
}

}

dnn.controls.DNNTextSuggest.registerClass('dnn.controls.DNNTextSuggest', dnn.controls.control);



dnn.controls.DNNTextSuggestNode = function(node)
{
    dnn.controls.DNNTextSuggestNode.initializeBase(this, [node]);
    
	//textsuggest specific attributes
	this.hover = false;
	this.selected = node.getAttribute('selected', '0') == '1' ? true : null;
	this.clickAction = node.getAttribute('ca', dnn.controls.action.none);
}

//DNNTextSuggestNode specific methods
dnn.controls.DNNTextSuggestNode.prototype = 
{
childNodes: function (index)
{
	logger.error(`>>> Entering childNodes(index=${index})`, { "event.action": "childNodes", "logrite.index": index });
	if (this.node.childNodes[index] != null)
		return new dnn.controls.DNNTextSuggestNode(this.node.childNodes[index]);
}
}
dnn.controls.DNNTextSuggestNode.registerClass('dnn.controls.DNNTextSuggestNode', dnn.controls.DNNNode);
