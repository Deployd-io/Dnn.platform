var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />

//dnn.xml Namespace 
Type.registerNamespace('dnn.xml');

dnn.extend(dnn.xml, {
	pns: 'dnn',
	ns: 'xml',
	parserName: null,

    get_parserName: function()
    {
		logger.error('>>> Entering get_parserName()', { "event.action": "get_parserName" });
        if (this.parserName == null)
            this.parserName = this._getParser();
		logger.warn('<<< Exiting get_parserName()', { "event.action": "get_parserName" });
        return this.parserName;
    },

    createDocument: function()
    {
		logger.error('>>> Entering createDocument()', { "event.action": "createDocument" });
        if (dnn.xml.get_parserName() == 'MSXML')
        {
	        var o = new ActiveXObject('MSXML.DOMDocument');
			logger.error(`createDocument(): o → ${o}`, { "event.action": "createDocument" });
	        o.async = false;
			logger.debug(`createDocument(): o.async → ${o.async}`, { "event.action": "createDocument" });
			logger.warn('<<< Exiting createDocument()', { "event.action": "createDocument" });
	        return new dnn.xml.documentObject(o); 
	    }
	    else if (dnn.xml.get_parserName() == 'DOMParser')
	    {
	        return new dnn.xml.documentObject(document.implementation.createDocument("", "", null)); 
	    }
	    else
	        return new dnn.xml.documentObject(new dnn.xml.JsDocument()); 
	},
	
    init: function()
    {
		logger.error('>>> Entering init()', { "event.action": "init" });
        if (this.get_parserName() == 'DOMParser')
	    {		        
		    function __dnn_getNodeXml() 
		    {
				logger.error('>>> Entering __dnn_getNodeXml()', { "event.action": "__dnn_getNodeXml" });
			    var oXmlSerializer = new XMLSerializer;  //create a new XMLSerializer    			    
			    var sXml = oXmlSerializer.serializeToString(this); //get the XML string
				logger.error(`__dnn_getNodeXml(): sXml → ${sXml}`, { "event.action": "__dnn_getNodeXml" });
			    return sXml; //return the XML string
		    }   //todo: move to inline function
		    Node.prototype.__defineGetter__("xml", __dnn_getNodeXml);
			logger.warn('<<< Exiting init()', { "event.action": "init" });
	    }
    },
    
    _getParser: function()
    {
		logger.error('>>> Entering _getParser()', { "event.action": "_getParser" });
	    if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer))
		    return 'MSXML';
	    else if (dnn.dom.browser.isType(dnn.dom.browser.Netscape,dnn.dom.browser.Mozilla))
		    return 'DOMParser';
	    else
		    return 'JS';
    }
    
});

//dnn.xml.documentObject Object
dnn.xml.documentObject = function(oDoc)
{
    this._doc = oDoc;
}

dnn.xml.documentObject.prototype = 
{
    getXml: function()
    {
		logger.error('>>> Entering getXml()', { "event.action": "getXml" });
        //todo: use switch here
        if (dnn.xml.get_parserName() == 'MSXML')
            return this._doc.xml;
	    else if (dnn.xml.get_parserName() == 'DOMParser')
	        return this._doc.xml;
	    else
	        return this._doc.getXml();	//wish other browsers supported getters/setters	
    
    },

    loadXml: function(sXml)
    {
		logger.error(`>>> Entering loadXml(sXml=${sXml})`, { "event.action": "loadXml", "logrite.sXml": sXml });
        if (dnn.xml.get_parserName() == 'MSXML')
            return this._doc.loadXML(sXml);
	    else if (dnn.xml.get_parserName() == 'DOMParser')
	    {
	        // parse the string to a new doc
	        var oDoc = (new DOMParser()).parseFromString(sXml, "text/xml");
				logger.error(`loadXml(sXml=${sXml}): oDoc → ${oDoc}`, { "event.action": "loadXml", "logrite.sXml": sXml });
			    
	        // remove all initial children
	        while (this._doc.hasChildNodes())
		        this._doc.removeChild(this._doc.lastChild);

	        // insert and import nodes
	        for (var i = 0; i < oDoc.childNodes.length; i++) 
		        this._doc.appendChild(this._doc.importNode(oDoc.childNodes[i], true));
	    }
	    else
	        return this._doc.loadXml(sXml);
    },

    childNodes: function(iIndex)
    {
		logger.error(`>>> Entering childNodes(iIndex=${iIndex})`, { "event.action": "childNodes", "logrite.iIndex": iIndex });
        if (this._doc.childNodes[iIndex] != null)
	        return new dnn.xml.XmlNode(this._doc.childNodes[iIndex]);
    },

    findNode: function(sNodeName, sAttr, sValue)
    {
		logger.error(`>>> Entering findNode(sNodeName=${sNodeName},sAttr=${sAttr},sValue=${sValue})`, { "event.action": "findNode", "logrite.sNodeName": sNodeName, "logrite.sAttr": sAttr, "logrite.sValue": sValue });
		logger.warn(`<<< Exiting findNode(sNodeName=${sNodeName},sAttr=${sAttr},sValue=${sValue})`, { "event.action": "findNode", "logrite.sNodeName": sNodeName, "logrite.sAttr": sAttr, "logrite.sValue": sValue });
		logger.error(`findNode(sNodeName=${sNodeName},sAttr=${sAttr},sValue=${sValue}): oNode → ${oNode}`, { "event.action": "findNode", "logrite.sNodeName": sNodeName, "logrite.sAttr": sAttr, "logrite.sValue": sValue });
        return this.childNodes(0).findNode(sNodeName, sAttr, sValue);
    },

    childNodeCount: function()
    {
		logger.error('>>> Entering childNodeCount()', { "event.action": "childNodeCount" });
		logger.warn('<<< Exiting childNodeCount()', { "event.action": "childNodeCount" });
        return this._doc.childNodes.length;
    },

    rootNode: function()
    {
		logger.error('>>> Entering rootNode()', { "event.action": "rootNode" });
        var oNode;
        for (var i=0; i<this.childNodeCount(); i++)
        {
	        if (this.childNodes(i).nodeType != 7)
	        {
		        oNode = this.childNodes(i);
				logger.error(`rootNode(): oNode → ${oNode}`, { "event.action": "rootNode" });
		        break;
	        }
        }
        return oNode;
    }
}
dnn.xml.documentObject.registerClass('dnn.xml.documentObject');

//dnn.xml.XmlNode ---------------------------------------------------------------------------------------------------------
dnn.xml.XmlNode = function(oNode)
{
	this.node = oNode;
	this.ownerDocument = this.node.ownerDocument;
	this.nodeType = this.node.nodeType;
}
	
dnn.xml.XmlNode.prototype = 
{
    parentNode: function()
	{
		logger.error('>>> Entering parentNode()', { "event.action": "parentNode" });
		if (this.node.parentNode != null)
			return new dnn.xml.XmlNode(this.node.parentNode);
		else
			return null;
	},
	
	childNodes: function(iIndex)
	{
		if (this.node.childNodes[iIndex] != null)
			return new dnn.xml.XmlNode(this.node.childNodes[iIndex]);
	},

	childNodeCount: function()
	{
		return this.node.childNodes.length;
	},

	nodeName: function()
	{
		logger.error('>>> Entering nodeName()', { "event.action": "nodeName" });
		logger.warn('<<< Exiting nodeName()', { "event.action": "nodeName" });
		return this.node.nodeName;
	},
	
	getAttribute: function(sAttr, sDef)
	{
		logger.error(`>>> Entering getAttribute(sAttr=${sAttr},sDef=${sDef})`, { "event.action": "getAttribute", "logrite.sAttr": sAttr, "logrite.sDef": sDef });
		var sRet = this.node.getAttribute(sAttr);
		if (sRet == null)
			sRet = sDef;
		return sRet;
	},

	setAttribute: function (sAttr, sVal)
	{
		logger.error(`>>> Entering setAttribute(sAttr=${sAttr},sVal=${sVal})`, { "event.action": "setAttribute", "logrite.sAttr": sAttr, "logrite.sVal": sVal });
		if (sVal == null)
			return this.node.removeAttribute(sAttr);
		else
			return this.node.setAttribute(sAttr, sVal);
	},
	
	getXml: function()
	{
		if (this.node.xml != null)
			return this.node.xml;
		else
			return this.node.getXml();	
	},

	getDocumentXml: function()
	{
		logger.error('>>> Entering getDocumentXml()', { "event.action": "getDocumentXml" });
		if (this.node.ownerDocument.xml != null)
			return this.node.ownerDocument.xml;
		else
			return this.node.ownerDocument.getXml();
	},

	appendXml: function(sXml)
	{
		logger.error(`>>> Entering appendXml(sXml=${sXml})`, { "event.action": "appendXml", "logrite.sXml": sXml });
		var oDoc = dnn.xml.createDocument();
		oDoc.loadXml('<___temp>' + sXml + '</___temp>');	//need to guarantee a single root
		logger.error(`appendXml(sXml=${sXml}): oDoc → ${oDoc}`, { "event.action": "appendXml", "logrite.sXml": sXml });
		var aNodes = new Array();
		logger.error(`appendXml(sXml=${sXml}): aNodes → ${aNodes}`, { "event.action": "appendXml", "logrite.sXml": sXml });

		for (var i=0; i<oDoc.childNodes(0).childNodeCount(); i++)	//appending child actually removes node from document, so get references then do append
			aNodes[aNodes.length] = oDoc.childNodes(0).childNodes(i).node;	//use real underlying node object
		for (var i=0; i<aNodes.length; i++)
			this.node.appendChild(aNodes[i]);	//surprised I don't need importNode
		
		logger.warn(`<<< Exiting appendXml(sXml=${sXml})`, { "event.action": "appendXml", "logrite.sXml": sXml });
		return true;
	},

	getNodeIndex: function(sIDName)
	{
		logger.error(`>>> Entering getNodeIndex(sIDName=${sIDName})`, { "event.action": "getNodeIndex", "logrite.sIDName": sIDName });
		var oParent = this.parentNode();
		var sID = this.getAttribute(sIDName);
		logger.error(`getNodeIndex(sIDName=${sIDName}): sID → ${sID}`, { "event.action": "getNodeIndex", "logrite.sIDName": sIDName });
		logger.error(`getNodeIndex(sIDName=${sIDName}): oParent → ${oParent}`, { "event.action": "getNodeIndex", "logrite.sIDName": sIDName });
		for (var i=0; i<oParent.childNodeCount(); i++)
		{
			if (oParent.childNodes(i).getAttribute(sIDName) == sID)
				return i;
		}
		return -1;
	},

	findNode: function(sNodeName, sAttr, sValue)
	{
		var sXPath = "//" + sNodeName + "[@" + sAttr + "='" + sValue + "']";
		var oNode;
		logger.error(`findNode(sNodeName=${sNodeName},sAttr=${sAttr},sValue=${sValue}): sXPath → ${sXPath}`, { "event.action": "findNode", "logrite.sNodeName": sNodeName, "logrite.sAttr": sAttr, "logrite.sValue": sValue });
		if (typeof(this.node.selectSingleNode) != 'undefined')
			oNode = this.node.selectSingleNode(sXPath);
		else if (typeof(this.node.ownerDocument.evaluate) != 'undefined')
		{
			var oNodeList = (this.node.ownerDocument.evaluate(sXPath, this.node, null, 0, null));
			logger.error(`findNode(sNodeName=${sNodeName},sAttr=${sAttr},sValue=${sValue}): oNodeList → ${oNodeList}`, { "event.action": "findNode", "logrite.sNodeName": sNodeName, "logrite.sAttr": sAttr, "logrite.sValue": sValue });
			if (oNodeList != null)
				oNode = oNodeList.iterateNext();
		}
		else
			oNode = this.node.ownerDocument.findNode(this.node, sNodeName, sAttr, sValue);
		
		if (oNode != null)
			return new dnn.xml.XmlNode(oNode);
	},

	removeChild: function(oNode)
	{
		logger.error(`>>> Entering removeChild(oNode=${oNode})`, { "event.action": "removeChild", "logrite.oNode": oNode });
		logger.warn(`<<< Exiting removeChild(oNode=${oNode})`, { "event.action": "removeChild", "logrite.oNode": oNode });
		return this.node.removeChild(oNode.node);
	}
}
dnn.xml.XmlNode.registerClass('dnn.xml.XmlNode');

dnn.xml.init();