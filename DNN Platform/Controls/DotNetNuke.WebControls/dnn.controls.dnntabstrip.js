var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.controls');

dnn.extend(dnn.controls, {
    initTabStrip: function (ctl) 
    {	
		logger.error(`>>> Entering initTabStrip(ctl=${ctl})`, { "event.action": "initTabStrip", "logrite.ctl": ctl });
	    if (ctl && dnn.controls.controls[ctl.id] == null)
	    {
	        var ts = new dnn.controls.DNNTabStrip(ctl);
		    ts.initialize();
			logger.error(`initTabStrip(ctl=${ctl}): ts → ${ts}`, { "event.action": "initTabStrip", "logrite.ctl": ctl });
			logger.warn(`<<< Exiting initTabStrip(ctl=${ctl})`, { "event.action": "initTabStrip", "logrite.ctl": ctl });
		    return ts;
	    }
    }
});

//------- Constructor -------//
dnn.controls.DNNTabStrip = function(o)
{
	dnn.controls.DNNTabStrip.initializeBase(this, [o]);	
	
	this.contentContainer = $get(o.id + '_c');
	this.resultCtr = null;
	//--- Appearance Properties ---//

	this.workImg = this.getProp('workImage', '');;	
	this.tabRenderMode = new Number(this.getProp('trm', '0'));		
	this.callBack = this.getProp('callback', '');
	this.callBackStatFunc = this.getProp('callbackSF', '');
	if (this.callBackStatFunc.length > 0)
	    this.add_handler('callBackStatus', eval(this.callBackStatFunc));
	
	this.callbackType = this.getProp('cbtype', '0');
	this.tabClickFunc = this.getProp('tabClickF', '');
	if (this.tabClickFunc.length > 0)
	    this.add_handler('tabClick', eval(this.tabClickFunc));
	
	this.selectedIndexFunc = this.getProp('selIdxF', '');
	if (this.selectedIndexFunc.length > 0)
	    this.add_handler('selectedIndexChanged', eval(this.selectedIndexFunc));
			
	this.lblCss = this.getProp('css', '');
	this.lblCssSel = this.getProp('csssel', this.lblCss);
	this.lblCssHover = this.getProp('csshover', '');
	this.lblCssDisabled = this.getProp('cssdisabled', '');
	
	this.pendingLookupId = null;

    var json = dnn.evalJSON(dnn.getVar(this.ns + '_json'));
    if (json)
    {
        this.tabData = json.tabs;
        this.tabLabelData = json.tablabels;
    }

	this.tabs = [];
	this.tabIds = [];
	//this.tabLookup = [];
	this.selectedIndex = -1;
	this.selectedTab = null;

	var clientId;
	var tabId;
	var tab;
	for (var i=0; i<this.tabData.length; i++)
	{
	    clientId = this.tabData[i].tcid;
	    tabId = this.tabData[i].tid;
		tab = new dnn.controls.DNNTab(this, i, tabId, clientId);
		tab.selected = (tab.container != null && tab.container.style.display != 'none');  //initial assignment of selected if container exists, otherwise showTab handles this property
		this.tabIds.push(tab.tabId);
		this.tabs[tabId] = tab;
		//this.tabLookup[tab.tab.id] = tab.tabId;
        this.addHandlers(tab.tab, {'click': this.tabClick, 
                                'mouseover': this.tabMouseOver,
                                'mouseout': this.tabMouseOut}, tab);
				
		if (tab.selected)
		{
			this.selectedIndex = i;
			this.selectedTab = tab;
		}
		
	}
	        
	this.update();
}

dnn.controls.DNNTabStrip.prototype = 
{
//--- Event Handlers ---//

/*
keyPress: function (e, element) 
{
	var KEY_RETURN = 13;
	if(e.keyCode == KEY_RETURN)	//stop from posting
		return false;
},

keyUp: function (e, element) 
{
	var KEY_UP_ARROW = 38;
	var KEY_DOWN_ARROW = 40;
	var KEY_RETURN = 13;
	var KEY_ESCAPE = 27;		
},
*/


//compatibility change:  No longer passes element, but instead tab
tabClick: function(evt, element)
{
	logger.error(`>>> Entering tabClick(evt=${evt},element=${element})`, { "event.action": "tabClick", "logrite.evt": evt, "logrite.element": element });
    var tab = this;
    var ts = tab.strip;
	if (tab.enabled)
	{
    	var arg = new dnn.controls.DNNTabStripEventArgs(tab);
		logger.error(`tabClick(evt=${evt},element=${element}): arg → ${arg}`, { "event.action": "tabClick", "logrite.evt": evt, "logrite.element": element });
    	ts.invoke_handler('tabClick', arg)
    	if (arg.get_cancel() == false)
			ts.showTab(tab.tabId);			
	}
},

tabMouseOver: function(evt, element)
{
	logger.error(`>>> Entering tabMouseOver(evt=${evt},element=${element})`, { "event.action": "tabMouseOver", "logrite.evt": evt, "logrite.element": element });
	if (typeof(dnn) != 'undefined')
	{
		this.hovered = true;
		this.assignCss();
		logger.warn(`<<< Exiting tabMouseOver(evt=${evt},element=${element})`, { "event.action": "tabMouseOver", "logrite.evt": evt, "logrite.element": element });
		logger.debug(`tabMouseOver(evt=${evt},element=${element}): this.hovered → ${this.hovered}`, { "event.action": "tabMouseOver", "logrite.evt": evt, "logrite.element": element });
	}
},

tabMouseOut: function(evt, element)
{
	logger.error(`>>> Entering tabMouseOut(evt=${evt},element=${element})`, { "event.action": "tabMouseOut", "logrite.evt": evt, "logrite.element": element });
	if (typeof(dnn) != 'undefined')
	{
		this.hovered = false;
		this.assignCss();
		logger.warn(`<<< Exiting tabMouseOut(evt=${evt},element=${element})`, { "event.action": "tabMouseOut", "logrite.evt": evt, "logrite.element": element });
		logger.debug(`tabMouseOut(evt=${evt},element=${element}): this.hovered → ${this.hovered}`, { "event.action": "tabMouseOut", "logrite.evt": evt, "logrite.element": element });
	}
},

// Methods

setSelectedIndex: function (index)
{
	logger.error(`>>> Entering setSelectedIndex(index=${index})`, { "event.action": "setSelectedIndex", "logrite.index": index });
	logger.warn(`<<< Exiting setSelectedIndex(index=${index})`, { "event.action": "setSelectedIndex", "logrite.index": index });
	return this.showTab(this.tabIds[index]);	
},

showTab: function (tabId) 
{
	logger.error(`>>> Entering showTab(tabId=${tabId})`, { "event.action": "showTab", "logrite.tabId": tabId });
    var tab;
	if (this.needsLookup(tabId))
	{
	    if (this.pendingLookupId == null)
	    {
			tab = this.tabs[tabId];
			tab.showWork(true);
	        this.pendingLookupId = tabId;
	        
			logger.debug(`showTab(tabId=${tabId}): this.pendingLookupId → ${this.pendingLookupId}`, { "event.action": "showTab", "logrite.tabId": tabId });
	        if (this.tabRenderMode == '1') //postback
	        {
				//for (var id in this.tabs)
				for (var i=0; i<this.tabIds.length;i++)
				{
					tab = this.tabs[this.tabIds[i]];
					if (tab.tabId == tabId)
					{
						tab.selected = true;
						this.selectedIndex = i;
						this.selectedTab = tab;
						logger.debug(`showTab(tabId=${tabId}): this.selectedIndex → ${this.selectedIndex}`, { "event.action": "showTab", "logrite.tabId": tabId });
						//this.raiseEvent('selectedIndexFunc', null, this);
						this.invoke_compatHandler('selectedIndexChanged', null, this);
						logger.debug(`showTab(tabId=${tabId}): this.selectedTab → ${this.selectedTab}`, { "event.action": "showTab", "logrite.tabId": tabId });
					}
					else
						tab.selected = false;
				}
				this.update();
	        }
		    eval(this.callBack.replace('[TABID]', tabId).replace("'[POST]'", tab.postMode).replace("[CBTYPE]", tab.callbackType));
		}
	}
	else
	{
		for (var i=0; i<this.tabIds.length;i++)
		{
			tab = this.tabs[this.tabIds[i]];
	        if (tab.tabId == tabId)
	        {
				tab.showWork(false);
				logger.debug(`showTab(tabId=${tabId}): tab → ${tab}`, { "event.action": "showTab", "logrite.tabId": tabId });
				logger.debug(`showTab(tabId=${tabId}): tab.rendered → ${tab.rendered}`, { "event.action": "showTab", "logrite.tabId": tabId });
				logger.debug(`showTab(tabId=${tabId}): tab.hovered → ${tab.hovered}`, { "event.action": "showTab", "logrite.tabId": tabId });
	            tab.selected = true;
				logger.debug(`showTab(tabId=${tabId}): tab.selected → ${tab.selected}`, { "event.action": "showTab", "logrite.tabId": tabId });
	            tab.rendered = true;
	            tab.hovered = false;
	            tab.container.style.display = '';
	            tab.assignCss();
	            this.selectedIndex = i;
	            this.selectedTab = tab;
	            //this.raiseEvent('selectedIndexFunc', null, this);
	            this.invoke_compatHandler('selectedIndexChanged', null, this);
	        }
	        else 
	        {
	            if (tab.container != null)
					tab.container.style.display = 'none';
	            tab.selected = false;
	            tab.assignCss();
	        }
	    }
		
        this.update();		
		logger.warn(`<<< Exiting showTab(tabId=${tabId})`, { "event.action": "showTab", "logrite.tabId": tabId });
	}
	return tab;
},

raiseEvent: function(sFunc, evt, element)
{
	logger.error(`>>> Entering raiseEvent(sFunc=${sFunc},evt=${evt},element=${element})`, { "event.action": "raiseEvent", "logrite.sFunc": sFunc, "logrite.evt": evt, "logrite.element": element });
	if (this[sFunc].length > 0)
	{
		var oPointerFunc = eval(this[sFunc]);
		logger.warn(`<<< Exiting raiseEvent(sFunc=${sFunc},evt=${evt},element=${element})`, { "event.action": "raiseEvent", "logrite.sFunc": sFunc, "logrite.evt": evt, "logrite.element": element });
		logger.error(`raiseEvent(sFunc=${sFunc},evt=${evt},element=${element}): oPointerFunc → ${oPointerFunc}`, { "event.action": "raiseEvent", "logrite.sFunc": sFunc, "logrite.evt": evt, "logrite.element": element });
		return oPointerFunc(evt, element) != false;
	}
	return true;
},

update: function()
{
	logger.error('>>> Entering update()', { "event.action": "update" });
    var ary = [];
    for (var i=0; i<this.tabIds.length;i++)
        ary[ary.length] = this.tabs[this.tabIds[i]].serialize();
    dnn.setVar(this.ns + '_tabs', ary.join(','));
	logger.warn('<<< Exiting update()', { "event.action": "update" });
},

createTab: function (html, id) 
{
	logger.error(`>>> Entering createTab(html=${html},id=${id})`, { "event.action": "createTab", "logrite.html": html, "logrite.id": id });
    var span = dnn.dom.createElement('span');
    span.innerHTML = html;
	logger.error(`createTab(html=${html},id=${id}): span → ${span}`, { "event.action": "createTab", "logrite.html": html, "logrite.id": id });
	logger.debug(`createTab(html=${html},id=${id}): span.innerHTML → ${span.innerHTML}`, { "event.action": "createTab", "logrite.html": html, "logrite.id": id });
    
    var ctr = span.childNodes[0];
	logger.error(`createTab(html=${html},id=${id}): ctr → ${ctr}`, { "event.action": "createTab", "logrite.html": html, "logrite.id": id });
    if (ctr.id != this.tabs[id].clientId)
		ctr = dnn.dom.getById(this.tabs[id].clientId, span); 
    
    //ctr.parentNode.removeNode(ctr);
    this.tabs[id].container = ctr;
    //ctr.style.display = 'none';    //?????
    this.contentContainer.appendChild(ctr);
	logger.warn(`<<< Exiting createTab(html=${html},id=${id})`, { "event.action": "createTab", "logrite.html": html, "logrite.id": id });
    return this.tabs[id];
},

resetTab: function (id) 
{
	logger.error(`>>> Entering resetTab(id=${id})`, { "event.action": "resetTab", "logrite.id": id });
	var tab = this.tabs[id];
	if (tab.container != null)
	{
		//tab.container.style.display = 'none';
		dnn.dom.removeChild(tab.container);
		tab.container = null;
		logger.debug(`resetTab(id=${id}): tab.container → ${tab.container}`, { "event.action": "resetTab", "logrite.id": id });
	}
},

needsLookup: function (tabId) 
{
	logger.error(`>>> Entering needsLookup(tabId=${tabId})`, { "event.action": "needsLookup", "logrite.tabId": tabId });
	logger.warn(`<<< Exiting needsLookup(tabId=${tabId})`, { "event.action": "needsLookup", "logrite.tabId": tabId });
    return this.tabs[tabId].container == null; 
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
	ctx.invoke_handler('callBackSuccess', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
	
	/*if (ts.callBackStatFunc != null && ts.callBackStatFunc.length > 0)
	{
		var oPointerFunc = eval(ts.callBackStatFunc);
		oPointerFunc(result, ctx, req);	
	}*/
	var ret = dnn.evalJSON(result);
	logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): oPointerFunc → ${oPointerFunc}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): ret → ${ret}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	var tab = ts.createTab(ret.text, ts.pendingLookupId);
	logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): tab → ${tab}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	var vars = dnn.evalJSON(ret.vars);
	logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): vars → ${vars}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	for (var key in vars)
	    dnn.setVar(key, vars[key]);
	
	if (ret.scripts != null)
	{
		var scripts = dnn.evalJSON(ret.scripts);
		logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): scripts → ${scripts}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
		var inlineScripts = [];
		var refScripts = [];
		for (var s in scripts)
		{
			if (scripts[s].src && scripts[s].src.length > 0)
			{
				if (dnn.dom.scriptStatus(scripts[s].src) == '')
					refScripts.push(scripts[s].src);
			}
			else
				inlineScripts.push(scripts[s].__text);
		}
		dnn.dom.loadScripts(refScripts, inlineScripts, dnn.createDelegate(ts, ts.callBackScriptComplete));
	}	
	else
		ts.callBackScriptComplete();

},

callBackScriptComplete: function()
{
	logger.error('>>> Entering callBackScriptComplete()', { "event.action": "callBackScriptComplete" });
	var tabId = this.pendingLookupId;
	this.pendingLookupId = null;
	this.showTab(tabId);
	logger.debug(`callBackScriptComplete(): this.pendingLookupId → ${this.pendingLookupId}`, { "event.action": "callBackScriptComplete" });
},

callBackFail: function (result, ctx, req) 
{
	logger.error(`>>> Entering callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	ctx.invoke_handler('callBackFail', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
	logger.warn(`<<< Exiting callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
},

_getTabFromTabElement: function(element)
{
	logger.error(`>>> Entering _getTabFromTabElement(element=${element})`, { "event.action": "_getTabFromTabElement", "logrite.element": element });
    
	logger.warn(`<<< Exiting _getTabFromTabElement(element=${element})`, { "event.action": "_getTabFromTabElement", "logrite.element": element });
    //return this.tabs[this.tabLookup[element.id]]; 
},

_getLabelData: function(index, prop, def)
{
	logger.error(`>>> Entering _getLabelData(index=${index},prop=${prop},def=${def})`, { "event.action": "_getLabelData", "logrite.index": index, "logrite.prop": prop, "logrite.def": def });
    var data = this.tabLabelData[index];
    if (data[prop] != null && data[prop].length > 0)
        return data[prop];
    else
        return def;
},

_getTabData: function(index, prop, def)
{
	logger.error(`>>> Entering _getTabData(index=${index},prop=${prop},def=${def})`, { "event.action": "_getTabData", "logrite.index": index, "logrite.prop": prop, "logrite.def": def });
    var data = this.tabData[index];
    if (data[prop] != null && data[prop].length > 0)
        return data[prop];
    else
        return def;
}

}
dnn.controls.DNNTabStrip.registerClass('dnn.controls.DNNTabStrip', dnn.controls.control);

dnn.controls.DNNTab = function(strip, index, id, clientId)
{
    var tab = $get(clientId + '_l');
	if (tab != null)
	{
	    this.tab = tab;
		this.container = $get(clientId); 
		this.icon = $get(clientId + '_i');
		this.work = $get(clientId + '_w');
	    this.rendered = (this.container != null);
	    this.selected = false;
	    this.hovered = false;
	    this.tabIndex = index;
	    this.strip = strip;
	    this.text = this.tab.innerHTML;
		this.clientId = clientId;
		this.tabId = id;
		this.css = this.strip._getLabelData(index, 'css', strip.lblCss);
		this.cssSel = this.strip._getLabelData(index, 'csssel', strip.lblCssSel); 
		this.cssHover = this.strip._getLabelData(index, 'csshover', strip.lblCssHover);  
		this.cssDisabled = this.strip._getLabelData(index, 'cssdisabled', strip.lblCssDisabled);
		this.postMode = this.strip._getTabData(index, 'postmode', null);
		this.enabled = (this.strip._getTabData(index, 'enabled', "1") == "1");
		this.callbackType = this.strip._getTabData(index, 'cbtype', strip.callbackType);
		if (this.postMode)
			this.postMode = '\'' + this.postMode + '\'';
	}
}

dnn.controls.DNNTab.prototype = 
{
serialize: function ()
{
	logger.error('>>> Entering serialize()', { "event.action": "serialize" });
    //bitvalues - 1=rendered, 2=selected, 4=enabled (so 7 means rendered, selected, and enabled)
	logger.warn('<<< Exiting serialize()', { "event.action": "serialize" });
    return this.tabId + '=' + ((this.rendered ? 1 : 0) + (this.selected ? 2 : 0) + (this.enabled ? 4 : 0));
},
showWork: function (show)
{
logger.error(`>>> Entering showWork(show=${show})`, { "event.action": "showWork", "logrite.show": show });

	if (this.work != null)
	{
		if (show)
		{
			this.work.style.display = '';
			if (this.icon != null)
				this.icon.style.display = 'none';
		}
		else
		{
			this.work.style.display = 'none';
			if (this.icon != null)
				this.icon.style.display = '';
		}
	}
},
enable: function()
{
	logger.error('>>> Entering enable()', { "event.action": "enable" });
	this.enabled = true;
	
	logger.debug(`enable(): this.enabled → ${this.enabled}`, { "event.action": "enable" });
},
disable: function()
{
	logger.error('>>> Entering disable()', { "event.action": "disable" });
	this.enabled = false;
	logger.warn('<<< Exiting disable()', { "event.action": "disable" });
},
assignCss: function()
{
	logger.error('>>> Entering assignCss()', { "event.action": "assignCss" });
	var sCss = '';
	if (this.enabled == false && this.cssDisabled.length > 0)
		sCss = this.cssDisabled;
	else
	{
	    if (this.hovered && this.cssHover.length > 0)
	        sCss = this.cssHover;
	    else
			sCss = (this.selected ? this.cssSel : this.css);
	}
	this.tab.className = sCss;
	logger.error(`assignCss(): sCss → ${sCss}`, { "event.action": "assignCss" });
}

}

dnn.controls.DNNTab.registerClass('dnn.controls.DNNTab');