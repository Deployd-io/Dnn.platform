var logger = (typeof logger !== 'undefined' && logger) || console;
Type.registerNamespace('dnn.controls');
dnn.controls.orient = function(){};
dnn.controls.orient.prototype = {
    horizontal: 0,
    vertical: 1
}
dnn.controls.orient.registerEnum("dnn.controls.orient");

dnn.controls.action = function(){};
dnn.controls.action.prototype = {
	    postback: 0,
	    expand: 1,
	    none: 2,
	    nav: 3
}
dnn.controls.action.registerEnum("dnn.controls.action");

dnn.extend(dnn.controls, {
    version: new Number('02.03'),
	pns: 'dnn',
	ns: 'controls',
	isLoaded: false,
	controls: [],
	toolbars: [],	//stores JSON toolbar objects		
	_behaviorIDs: [],
	
    find: function(behaviorID)
    {
		logger.error(`>>> Entering find(behaviorID=${behaviorID})`, { "event.action": "find", "logrite.behaviorID": behaviorID });
		logger.warn(`<<< Exiting find(behaviorID=${behaviorID})`, { "event.action": "find", "logrite.behaviorID": behaviorID });
        return this.controls[this._behaviorIDs[behaviorID]];
    }
		
});

dnn.controls.control = function(ctl)
{
    dnn.controls.control.initializeBase(this, [ctl]);
    dnn.controls.controls[ctl.id] = this;
    this.behaviorID = '';
    this.ns = ctl.id;
    this.container = ctl;
    this._props = null;
    this._childControls = [];
    this._childControlIDs = [];
    this._handlerControls = [];
}

dnn.controls.control.prototype = 
{
    initialize: function(behaviorID)
    {
		logger.error(`>>> Entering initialize(behaviorID=${behaviorID})`, { "event.action": "initialize", "logrite.behaviorID": behaviorID });
         dnn.controls.control.callBaseMethod(this, 'initialize');
         if (behaviorID)
            this.behaviorID = behaviorID;
         else
            this.behaviorID = this.getProp('bid', '');
         if (this.behaviorID.length > 0)
            dnn.controls._behaviorIDs[this.behaviorID] = this.ns;
            
			logger.debug(`initialize(behaviorID=${behaviorID}): behaviorID → ${behaviorID}`, { "event.action": "initialize", "logrite.behaviorID": behaviorID });
    },
    
    getProp: function(name, defVal)
    {
		logger.error(`>>> Entering getProp(name=${name},defVal=${defVal})`, { "event.action": "getProp", "logrite.name": name, "logrite.defVal": defVal });
        if (this._props == null) 
        {
            this._props = {};
            var p = dnn.getVar(this.ns + '_p');
			logger.error(`getProp(name=${name},defVal=${defVal}): p → ${p}`, { "event.action": "getProp", "logrite.name": name, "logrite.defVal": defVal });
			logger.debug(`getProp(name=${name},defVal=${defVal}): this._props → ${this._props}`, { "event.action": "getProp", "logrite.name": name, "logrite.defVal": defVal });
            if (p)
            {
                this._props = dnn.evalJSON(p);
        	    if (dnn.dom.browser.isType(dnn.dom.browser.Mozilla) == false) //firefox cannot clear this value.  if F5 pressed, won't cause onsubmit.  Ctrl+F5 will...  
                    dnn.setVar(this.ns + '_p', ''); //clear out value when done
            }
        }
        var val = this._props[name];
        if (val == undefined || val == '')
            return defVal;
        else
            return val;
    },

    addHandlers: function(element, events, handlerOwner)
    {
		logger.error(`>>> Entering addHandlers(element=${element},events=${events},handlerOwner=${handlerOwner})`, { "event.action": "addHandlers", "logrite.element": element, "logrite.events": events, "logrite.handlerOwner": handlerOwner });
        this._handlerControls.push(element);    //store controls with handlers so we can clear them in dispose
        $addHandlers(element, events, handlerOwner);
		logger.warn(`<<< Exiting addHandlers(element=${element},events=${events},handlerOwner=${handlerOwner})`, { "event.action": "addHandlers", "logrite.element": element, "logrite.events": events, "logrite.handlerOwner": handlerOwner });
    },

    getChildControlId: function(id, prefix)
    {
		logger.error(`>>> Entering getChildControlId(id=${id},prefix=${prefix})`, { "event.action": "getChildControlId", "logrite.id": id, "logrite.prefix": prefix });
		logger.warn(`<<< Exiting getChildControlId(id=${id},prefix=${prefix})`, { "event.action": "getChildControlId", "logrite.id": id, "logrite.prefix": prefix });
        return this.ns + prefix + id;
    },
    
    createChildControl: function (tag, id, prefix)
    {
		logger.error(`>>> Entering createChildControl(tag=${tag},id=${id},prefix=${prefix})`, { "event.action": "createChildControl", "logrite.tag": tag, "logrite.id": id, "logrite.prefix": prefix });
	    var ctl = dnn.dom.createElement(tag);
	    ctl.ns = this.ns;
		logger.error(`createChildControl(tag=${tag},id=${id},prefix=${prefix}): ctl → ${ctl}`, { "event.action": "createChildControl", "logrite.tag": tag, "logrite.id": id, "logrite.prefix": prefix });
	    //ctl.id = this.ns + prefix + id;
	    ctl.id = this.getChildControlId(id, prefix);
		logger.debug(`createChildControl(tag=${tag},id=${id},prefix=${prefix}): ctl.ns → ${ctl.ns}`, { "event.action": "createChildControl", "logrite.tag": tag, "logrite.id": id, "logrite.prefix": prefix });
	    this.registerChildControl(ctl, id);
		logger.warn(`<<< Exiting createChildControl(tag=${tag},id=${id},prefix=${prefix})`, { "event.action": "createChildControl", "logrite.tag": tag, "logrite.id": id, "logrite.prefix": prefix });
	    return ctl;
    },
    
    registerChildControl: function(ctl, id)
    {
		logger.error(`>>> Entering registerChildControl(ctl=${ctl},id=${id})`, { "event.action": "registerChildControl", "logrite.ctl": ctl, "logrite.id": id });
	    this._childControlIDs[ctl.id] = id;
	    this._childControls[ctl.id] = ctl; 
		logger.warn(`<<< Exiting registerChildControl(ctl=${ctl},id=${id})`, { "event.action": "registerChildControl", "logrite.ctl": ctl, "logrite.id": id });
    }, 

    getChildControl: function (id, prefix)
    {
		logger.error(`>>> Entering getChildControl(id=${id},prefix=${prefix})`, { "event.action": "getChildControl", "logrite.id": id, "logrite.prefix": prefix });
	    var newId = this.ns + prefix + id;
	    if (this._childControls[newId] != null)	
		    return this._childControls[newId];
	    else
		    return $get(newId);
    },

    getChildControlBaseId: function(ctl)
    {
		logger.error(`>>> Entering getChildControlBaseId(ctl=${ctl})`, { "event.action": "getChildControlBaseId", "logrite.ctl": ctl });
        while (ctl.id.length == 0 && ctl.parentNode)
        {
            ctl = ctl.parentNode;
			logger.warn(`<<< Exiting getChildControlBaseId(ctl=${ctl})`, { "event.action": "getChildControlBaseId", "logrite.ctl": ctl });
        }
        return this._childControlIDs[ctl.id];
    },

    add_handler: function(name, handler)
    {
		logger.error(`>>> Entering add_handler(name=${name},handler=${handler})`, { "event.action": "add_handler", "logrite.handler": handler });
        this.get_events().addHandler(name, handler);
		logger.warn(`<<< Exiting add_handler(name=${name},handler=${handler})`, { "event.action": "add_handler", "logrite.handler": handler });
		logger.error(`>>> Entering add_handler(handler=${handler})`, { "event.action": "add_handler", "logrite.handler": handler });
		logger.warn(`<<< Exiting add_handler(handler=${handler})`, { "event.action": "add_handler", "logrite.handler": handler });
    },

    remove_handler: function(name, handler) 
    {
		logger.error(`>>> Entering remove_handler(name=${name},handler=${handler})`, { "event.action": "remove_handler", "logrite.handler": handler });
        this.get_events().removeHandler(name, handler);
		logger.warn(`<<< Exiting remove_handler(name=${name},handler=${handler})`, { "event.action": "remove_handler", "logrite.handler": handler });
		logger.error(`>>> Entering remove_handler(handler=${handler})`, { "event.action": "remove_handler", "logrite.handler": handler });
		logger.warn(`<<< Exiting remove_handler(handler=${handler})`, { "event.action": "remove_handler", "logrite.handler": handler });
    },
    
    invoke_handler: function(name, args)
    {
	logger.error(`>>> Entering invoke_handler(name=${name},args=${args})`, { "event.action": "invoke_handler", "logrite.name": name, "logrite.args": args });
    
        var h = this.get_events().getHandler(name);    
		logger.error(`invoke_handler(name=${name},args=${args}): h → ${h}`, { "event.action": "invoke_handler", "logrite.name": name, "logrite.args": args });
        if (args == null)
            args = Sys.EventArgs.Empty;
        
        if (h)
            h(this, args);
    },

    invoke_compatHandler: function(name)
    {
		logger.error(`>>> Entering invoke_compatHandler(name=${name})`, { "event.action": "invoke_compatHandler", "logrite.name": name });
        var ret =  true;
        var h;
        var evts = this.get_events()._getEvent(name);   //warning: accessing private member
		logger.error(`invoke_compatHandler(name=${name}): evts → ${evts}`, { "event.action": "invoke_compatHandler", "logrite.name": name });
        if (evts)
        {
            for (var i=0; i< evts.length; i++)
            {
                h = evts[i];
                ret = h.apply(undefined, arguments) != false;
                if (ret == false)
                    return ret;
            }
        }
        return true;
    },

    getDynamicEventObject: function(name, handler)
    {
		logger.error(`>>> Entering getDynamicEventObject(name=${name},handler=${handler})`, { "event.action": "getDynamicEventObject", "logrite.name": name, "logrite.handler": handler });
        var eh = {};
        eh[name] = handler;
		logger.warn(`<<< Exiting getDynamicEventObject(name=${name},handler=${handler})`, { "event.action": "getDynamicEventObject", "logrite.name": name, "logrite.handler": handler });
        return eh;
    },
    
    callBackFail: function (result, ctx, req) 
    {
		logger.error(`>>> Entering callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
	    this.invoke_handler('callBackFail', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
	    alert(result);
		logger.warn(`<<< Exiting callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },
    
    dispose: function()
    {
		logger.error('>>> Entering dispose()', { "event.action": "dispose" });
        this._childControls = null;
        this._childControlIDs = null;
		logger.debug(`dispose(): this._childControls → ${this._childControls}`, { "event.action": "dispose" });
		logger.debug(`dispose(): this._childControlIDs → ${this._childControlIDs}`, { "event.action": "dispose" });
        
        for (var i=0; i<this._handlerControls.length; i++)
        {
            $clearHandlers(this._handlerControls[i]);
            this._handlerControls[i] = null;
        }
        
        this.container = null;
        this._handlerControls = null;
		logger.debug(`dispose(): this.container → ${this.container}`, { "event.action": "dispose" });
		logger.debug(`dispose(): this._handlerControls → ${this._handlerControls}`, { "event.action": "dispose" });
        dnn.controls.control.callBaseMethod(this, 'dispose');
    },

    //strips prefix of on from event name
    _getEventName: function(s)
    {
		logger.error(`>>> Entering _getEventName(s=${s})`, { "event.action": "_getEventName", "logrite.s": s });
        if (s.indexOf('on') == 0)
            return s.substring(2);
        return s;
    }
    
}

dnn.controls.control.registerClass('dnn.controls.control', Sys.UI.Control);


dnn.controls.DNNNode = function(node)
{
    this._abbr = {target: 'tar', toolTip: 'tTip', imageIndex: 'imgIdx', image: 'img'};
	if (node != null)
	{
		this.node = node; 
		this.id = node.getAttribute('id', '');
		this.key = node.getAttribute('key', '');
		this.text = node.getAttribute('txt', '');
		this.url = node.getAttribute('url', '');
		this.js = node.getAttribute('js', '');
		this.target = node.getAttribute('tar', '');
		this.toolTip = node.getAttribute('tTip', '');
		this.enabled = node.getAttribute('enabled', '1') != '0';
		this.css = node.getAttribute('css', '');
		this.cssSel = node.getAttribute('cssSel', '');
		this.cssHover = node.getAttribute('cssHover', '');
		this.cssIcon = node.getAttribute('cssIcon', '');
		this.hasNodes = node.childNodeCount() > 0;	
		this.hasPendingNodes = (node.getAttribute('hasNodes', '0') == '1' && this.hasNodes == false);	
		this.imageIndex = new Number(node.getAttribute('imgIdx', '-1')); 
		this.image = node.getAttribute('img', '');
		this.level = this.getNodeLevel();	//cache
		this.isBreak = node.getAttribute('isBreak', '0') == '1' ? true : false;
	}
}

dnn.controls.DNNNode.prototype = 
{
    _getAbbr: function(name)
    {
		logger.error(`>>> Entering _getAbbr(name=${name})`, { "event.action": "_getAbbr", "logrite.name": name });
        if (this._abbr[name])
            return this._abbr[name];
		logger.warn(`<<< Exiting _getAbbr(name=${name})`, { "event.action": "_getAbbr", "logrite.name": name });
        return name;
    },
    _addAbbr: function(dict)
    {
		logger.error(`>>> Entering _addAbbr(dict=${dict})`, { "event.action": "_addAbbr", "logrite.dict": dict });
        for (var prop in dict)
            this._abbr[prop] = dict[prop];
    },
    childNodeCount: function()
    {
		logger.error('>>> Entering childNodeCount()', { "event.action": "childNodeCount" });
		logger.warn('<<< Exiting childNodeCount()', { "event.action": "childNodeCount" });
	    return this.node.childNodeCount();
    },
    getNodeLevel: function()
    {
		logger.error('>>> Entering getNodeLevel()', { "event.action": "getNodeLevel" });
		logger.warn('<<< Exiting getNodeLevel()', { "event.action": "getNodeLevel" });
        return this.getParentNodes().length;
    },
    getParentNodes: function()
    {
		logger.error('>>> Entering getParentNodes()', { "event.action": "getParentNodes" });
	    var nodes = [];
	    var node = this.node;
		logger.error(`getParentNodes(): node → ${node}`, { "event.action": "getParentNodes" });
	    while (node != null)
	    {
		    node = node.parentNode();
		    if (node == null || node.nodeName() == 'root')
			    break;
		    nodes.push(node);
			logger.warn('<<< Exiting getParentNodes()', { "event.action": "getParentNodes" });
	    }	
	    return nodes;
    },
    update: function(prop)
    {
		logger.error(`>>> Entering update(prop=${prop})`, { "event.action": "update", "logrite.prop": prop });
	    if (prop != null)
	    {
		    var type = typeof(this[prop]);
			logger.error(`update(prop=${prop}): type → ${type}`, { "event.action": "update", "logrite.prop": prop });
			var key = prop;
			logger.error(`update(prop=${prop}): key → ${key}`, { "event.action": "update", "logrite.prop": prop });
	        if (this._abbr[prop])
	            key = this._abbr[prop];
			
		    if (type == 'string' || type == 'number' || this[prop] == null)
			    this.node.setAttribute(prop, this[prop]);
		    else if (type == 'boolean')
			    this.node.setAttribute(prop, new Number(this[prop]));
	    }
	    else
	    {
		    for (prop in this)
                this.update(prop);
	    }
    }
}
dnn.controls.DNNNode.registerClass('dnn.controls.DNNNode');

dnn.controls.JSONNode = function (node, nodeName, nodeIndex, path)
{
    dnn.extend(this, node);
    this._nodeName = nodeName;
    this._nodeDictionary = null;
    this._nodeIndex = nodeIndex;
    this._nodePath = nodeIndex.toString(); //todo get rid of prefix
    if (path == null)
        this._nodePath = '';
    else if (path.length > 0)
        this._nodePath = path + '-' + nodeIndex;
    if (nodeName == 'root')
    {
        this._nodeDictionary = []; 
        this.setupJSONNodes(this, this, node.nodes);
    }
}

dnn.controls.JSONNode.prototype = 
{
    getAttribute: function(name, def) 
    {
		logger.error(`>>> Entering getAttribute(name=${name},def=${def})`, { "event.action": "getAttribute", "logrite.name": name, "logrite.def": def });
        def = (def) ? def : '';
		logger.warn(`<<< Exiting getAttribute(name=${name},def=${def})`, { "event.action": "getAttribute", "logrite.name": name, "logrite.def": def });
        return this[name] == null ? def : this[name];
    },
    setAttribute: function(name, val) 
    {
        this[name] = val;
		logger.warn(`<<< Exiting setAttribute(name=${name},val=${val})`, { "event.action": "setAttribute", "logrite.name": name, "logrite.val": val });
    },
    parentNode: function() 
    {
		logger.error('>>> Entering parentNode()', { "event.action": "parentNode" });
		logger.warn('<<< Exiting parentNode()', { "event.action": "parentNode" });
        return this._parentNode;
    },
    hasChildNodes: function() 
    {
		logger.error('>>> Entering hasChildNodes()', { "event.action": "hasChildNodes" });
		logger.warn('<<< Exiting hasChildNodes()', { "event.action": "hasChildNodes" });
        return this.nodes.length > 0;
    },
    getNodeIndex: function() 
    {
		logger.error('>>> Entering getNodeIndex()', { "event.action": "getNodeIndex" });
		logger.warn('<<< Exiting getNodeIndex()', { "event.action": "getNodeIndex" });
        return this._nodeIndex;
    },
    getNodePath: function() 
    {
		logger.error('>>> Entering getNodePath()', { "event.action": "getNodePath" });
		logger.warn('<<< Exiting getNodePath()', { "event.action": "getNodePath" });
        return this._nodePath;
    },
    childNodeCount: function() 
    {
        return this.nodes.length;
    },
    childNodes: function(idx) 
    {
		logger.error(`>>> Entering childNodes(idx=${idx})`, { "event.action": "childNodes", "logrite.idx": idx });
		logger.warn(`<<< Exiting childNodes(idx=${idx})`, { "event.action": "childNodes", "logrite.idx": idx });
        return this.nodes[idx];
    },
    nodeName: function() 
    {
		logger.error('>>> Entering nodeName()', { "event.action": "nodeName" });
		logger.warn('<<< Exiting nodeName()', { "event.action": "nodeName" });
        return this._nodeName;
    },
    rootNode: function() 
    {
		logger.error('>>> Entering rootNode()', { "event.action": "rootNode" });
		logger.warn('<<< Exiting rootNode()', { "event.action": "rootNode" });
        return this._parentNode == null ? this : this._parentNode.rootNode();
    },
    findNode: function(id) 
    {
		logger.error(`>>> Entering findNode(id=${id})`, { "event.action": "findNode", "logrite.id": id });
        if (arguments.length == 3)
            id = arguments[2];  //back compat 'n', 'id', val
		logger.error(`findNode(id=${id}): id → ${id}`, { "event.action": "findNode", "logrite.id": id });
        return this.rootNode()._nodeDictionary[id];
    },

    getJSON: function(node)
    {
		logger.error(`>>> Entering getJSON(node=${node})`, { "event.action": "getJSON", "logrite.node": node });
	    if (node == null)
		    node = this;

	    var json = '{';
    	
		json += this.getJSONAttributes(node, ':', ',') + ',nodes:[';
    	
	    for (var i=0; i<node.childNodeCount(); i++)
	    {
	        if (i > 0)
	            json += ',';
		    json += this.getJSON(node.childNodes(i));// + node.childNodes(i).text;
	    }	
    
        json += ']}';
		logger.warn(`<<< Exiting getJSON(node=${node})`, { "event.action": "getJSON", "logrite.node": node });
	    return json;
    },

    //only necessary for backwards compat  - becoming obsolete
    getXml: function(node)
    {
		logger.error(`>>> Entering getXml(node=${node})`, { "event.action": "getXml", "logrite.node": node });
	    if (node == null)
		    node = this;

	    var xml = '';
		logger.error(`getXml(node=${node}): xml → ${xml}`, { "event.action": "getXml", "logrite.node": node });
    	
	    //if (node.nodeName() != '__root')
		    xml =  '<' + node.nodeName() + this.getXmlAttributes(node) + '>';
    	
	    for (var i=0; i<node.childNodeCount(); i++)
	    {
		    xml += this.getXml(node.childNodes(i));// + node.childNodes(i).text;				
	    }	
	    //if (node.nodeName != '__root')
		    xml = xml + '</' + node.nodeName() + '>';
			logger.warn(`<<< Exiting getXml(node=${node})`, { "event.action": "getXml", "logrite.node": node });
	    return xml;
    },

    getJSONAttributes: function(node)
    {
		logger.error(`>>> Entering getJSONAttributes(node=${node})`, { "event.action": "getJSONAttributes", "logrite.node": node });
	    var ret = '';
		logger.error(`getJSONAttributes(node=${node}): ret → ${ret}`, { "event.action": "getJSONAttributes", "logrite.node": node });
	    for (var attr in node)
	    {
	        if (typeof(node[attr]) != 'function' && attr.substring(0, 1) != '_' && attr != 'nodes')
	        {
	            if (ret.length > 0)
	                ret += ',';
		        ret += ' ' + attr + ':"' + dnn.encodeJSON(node.getAttribute(attr).toString()) + '"';
				logger.warn(`<<< Exiting getJSONAttributes(node=${node})`, { "event.action": "getJSONAttributes", "logrite.node": node });
		    }
		}
	    return ret;
    },

    getXmlAttributes: function(node)
    {
		logger.error(`>>> Entering getXmlAttributes(node=${node})`, { "event.action": "getXmlAttributes", "logrite.node": node });
	    var ret = '';
		logger.error(`getXmlAttributes(node=${node}): ret → ${ret}`, { "event.action": "getXmlAttributes", "logrite.node": node });
	    for (var attr in node)
	    {
	        if (typeof(node[attr]) != 'function' && attr.substring(0, 1) != '_' && attr != 'nodes')
	        {
	            if (ret.length > 0)
	                ret += ' ';
		        ret += ' ' + attr + '="' + dnn.encodeHTML(node.getAttribute(attr)) + '"';					
				logger.warn(`<<< Exiting getXmlAttributes(node=${node})`, { "event.action": "getXmlAttributes", "logrite.node": node });
		    }
		}
	    return ret;
    },
    
    setupJSONNodes: function(root, parent, nodes)
    {
		logger.error(`>>> Entering setupJSONNodes(root=${root},parent=${parent},nodes=${nodes})`, { "event.action": "setupJSONNodes", "logrite.root": root, "logrite.parent": parent, "logrite.nodes": nodes });
        var jnode;
        
        for (var i=0;i<nodes.length; i++)
        {            
            jnode = new dnn.controls.JSONNode(nodes[i], 'n', i, parent.getNodePath());
            jnode._parentNode = parent;
			logger.error(`setupJSONNodes(root=${root},parent=${parent},nodes=${nodes}): jnode → ${jnode}`, { "event.action": "setupJSONNodes", "logrite.root": root, "logrite.parent": parent, "logrite.nodes": nodes });
			logger.debug(`setupJSONNodes(root=${root},parent=${parent},nodes=${nodes}): jnode → ${jnode}`, { "event.action": "setupJSONNodes", "logrite.root": root, "logrite.parent": parent, "logrite.nodes": nodes });
			logger.debug(`setupJSONNodes(root=${root},parent=${parent},nodes=${nodes}): jnode._parentNode → ${jnode._parentNode}`, { "event.action": "setupJSONNodes", "logrite.root": root, "logrite.parent": parent, "logrite.nodes": nodes });
            root._nodeDictionary[jnode.id] = jnode;
            nodes[i] = jnode;    //replace with JSONNode        
            this.setupJSONNodes(root, jnode, jnode.nodes);
			logger.warn(`<<< Exiting setupJSONNodes(root=${root},parent=${parent},nodes=${nodes})`, { "event.action": "setupJSONNodes", "logrite.root": root, "logrite.parent": parent, "logrite.nodes": nodes });
        }
    }
}
dnn.controls.JSONNode.registerClass('dnn.controls.JSONNode');

//Event Arguments
dnn.controls.DNNNodeEventArgs = function(node) 
{
    dnn.controls.DNNNodeEventArgs.initializeBase(this);
    this._node = node;
}

dnn.controls.DNNNodeEventArgs.prototype = 
{
    get_node: function()
    {
		logger.error('>>> Entering get_node()', { "event.action": "get_node" });
		logger.warn('<<< Exiting get_node()', { "event.action": "get_node" });
        return this._node;
    }
}
dnn.controls.DNNNodeEventArgs.registerClass('dnn.controls.DNNNodeEventArgs', Sys.CancelEventArgs);

dnn.controls.DNNTabStripEventArgs = function(tab) 
{
    dnn.controls.DNNTabStripEventArgs.initializeBase(this);
    this._tab = tab;
}

dnn.controls.DNNTabStripEventArgs.prototype = 
{
    get_tab: function()
    {
		logger.error('>>> Entering get_tab()', { "event.action": "get_tab" });
		logger.warn('<<< Exiting get_tab()', { "event.action": "get_tab" });
        return this._tab;
    }
}
dnn.controls.DNNTabStripEventArgs.registerClass('dnn.controls.DNNTabStripEventArgs', Sys.CancelEventArgs);

dnn.controls.DNNToolBarEventArgs = function(btn) 
{
    dnn.controls.DNNToolBarEventArgs.initializeBase(this);
    this._btn = btn;
}

dnn.controls.DNNToolBarEventArgs.prototype = 
{
    get_button: function()
    {
		logger.error('>>> Entering get_button()', { "event.action": "get_button" });
		logger.warn('<<< Exiting get_button()', { "event.action": "get_button" });
        return this._btn;
    }
}
dnn.controls.DNNToolBarEventArgs.registerClass('dnn.controls.DNNToolBarEventArgs', Sys.CancelEventArgs);

dnn.controls.DNNCallbackEventArgs = function(result, ctx, req) 
{
    dnn.controls.DNNCallbackEventArgs.initializeBase(this);
    this._result = result;
    this._context = ctx;
    this._req = req;
}

dnn.controls.DNNCallbackEventArgs.prototype = 
{
    get_result: function()
    {
		logger.error('>>> Entering get_result()', { "event.action": "get_result" });
		logger.warn('<<< Exiting get_result()', { "event.action": "get_result" });
        return this._result;
    },
    
    get_context: function()
    {
		logger.error('>>> Entering get_context()', { "event.action": "get_context" });
		logger.warn('<<< Exiting get_context()', { "event.action": "get_context" });
        return this._context;
    },
    
    get_request: function()
    {
		logger.error('>>> Entering get_request()', { "event.action": "get_request" });
		logger.warn('<<< Exiting get_request()', { "event.action": "get_request" });
        return this._req;
    }
}
dnn.controls.DNNCallbackEventArgs.registerClass('dnn.controls.DNNCallbackEventArgs', Sys.EventArgs);

//allows onsubmit to be easily trapped
dnn.controls._submitComponent = function()
{
    dnn.controls._submitComponent.initializeBase(this);
}
dnn.controls._submitComponent.prototype = 
{
    onsubmit: function()
    {
		logger.error('>>> Entering onsubmit()', { "event.action": "onsubmit" });
        var h = this.get_events().getHandler('submit');           
        if (h)
            h(this, Sys.EventArgs.Empty);
			logger.warn('<<< Exiting onsubmit()', { "event.action": "onsubmit" });
    },
    add_handler: function(handler)
    {
        this.get_events().addHandler('submit', handler);
    },
    remove_handler: function(handler) 
    {
        this.get_events().removeHandler('submit', handler);
    },
    
    dispose: function()
    {
        dnn.controls._submitComponent.callBaseMethod(this, 'dispose');
    }
}
dnn.controls._submitComponent.registerClass('dnn.controls._submitComponent', Sys.Component);
if (dnn.controls.submitComp == null)
    dnn.controls.submitComp = new dnn.controls._submitComponent();

