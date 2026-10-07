var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xml.js" assembly="DotNetNuke.WebUtility" />

dnn.xml.parserName = 'JS';  //force JS version
//dnn.xml.JsDocument object ---------------------------------------------------------------------------------------------------------
dnn.xml.JsDocument = function()
{
	this.root = new dnn.xml.JsXmlNode(this, '__root');
	this.childNodes = this.root.childNodes;
	this.currentHashCode=0;
}

dnn.xml.JsDocument.prototype = 
{
    hasChildNodes: function ()
    {
		logger.error('>>> Entering hasChildNodes()', { "event.action": "hasChildNodes" });
		logger.warn('<<< Exiting hasChildNodes()', { "event.action": "hasChildNodes" });
	    return this.childNodes.length > 0;
    },

    loadXml: function (sXml)
    {
	    var oParser = new dnn.xml.JsParser();
	    oParser.parse(sXml, this.root);
		logger.warn(`<<< Exiting loadXml(sXml=${sXml})`, { "event.action": "loadXml", "logrite.sXml": sXml });
	    return true;
    },

    getXml: function()
    {
		logger.error('>>> Entering getXml()', { "event.action": "getXml", "logrite.oNode": oNode });
		logger.warn('<<< Exiting getXml()', { "event.action": "getXml", "logrite.oNode": oNode });
	    return this.root.getXml();
    },

    findNode: function(oParent, sNodeName, sAttr, sValue)
    {
	    //primitive for now...  
	    for (var i=0; i < oParent.childNodes.length; i++)
	    {
		    oNode = oParent.childNodes[i];

		    //if (oNode.nodeType != 3)  //exclude nodeType of Text (Netscape/Mozilla) issue!
		    if (oNode.nodeName == sNodeName)
		    {
			    if (sAttr == null)
				    return oNode;
			    else
			    {
				    if (oNode.getAttribute(sAttr) == sValue)
					    return oNode;
			    }
		    }
		    if (oNode.childNodes.length > 0)
		    {
			    var o = this.findNode(oNode, sNodeName, sAttr, sValue);
			    if (o != null)
				    return o;
		    }
	    }
		logger.warn(`<<< Exiting findNode(oParent=${oParent},sNodeName=${sNodeName},sAttr=${sAttr},sValue=${sValue})`, { "event.action": "findNode", "logrite.oParent": oParent, "logrite.sNodeName": sNodeName, "logrite.sAttr": sAttr, "logrite.sValue": sValue });
    },

    getNextHashCode: function()
    {
		logger.error('>>> Entering getNextHashCode()', { "event.action": "getNextHashCode" });
	    this.currentHashCode++;
		logger.warn('<<< Exiting getNextHashCode()', { "event.action": "getNextHashCode" });
	    return this.currentHashCode;
    }
}
dnn.xml.JsDocument.registerClass('dnn.xml.JsDocument');


//dnn.xml.JsXmlNode Object
dnn.xml.JsXmlNode = function(ownerDocument, name)
{
    this.ownerDocument = ownerDocument;
    this.nodeName = name;
    this.text = '';
    this.childNodes = new Array();
    this.attributes = new Array();
    this.parentNode = null;
    this.hashCode = this.ownerDocument.getNextHashCode();
    this.nodeType = 0;
    //this.xml = this.getXml;
}

dnn.xml.JsXmlNode.prototype = 
{
    appendChild: function(oNode)
    {
		logger.error(`>>> Entering appendChild(oNode=${oNode})`, { "event.action": "appendChild", "logrite.oNode": oNode });
	    this.childNodes[this.childNodes.length] = oNode;
	    oNode.parentNode = this;
		logger.warn(`<<< Exiting appendChild(oNode=${oNode})`, { "event.action": "appendChild", "logrite.oNode": oNode });
    },

    removeChild: function(oNode)
    {
	    var oParent = this;
	    var iHash = oNode.hashCode;
	    var bFound = false;
	    for (var i=0; i<oParent.childNodes.length; i++)
	    {
		    if (bFound == false)
		    {
			    if (oParent.childNodes[i].hashCode == iHash)
				    bFound = true;
		    }
		    if (bFound)
			    oParent.childNodes[i] = oParent.childNodes[i+1];
	    }
	    if (bFound)
		    oParent.childNodes.length = oParent.childNodes.length - 1; //remove last node
		logger.warn(`<<< Exiting removeChild(oNode=${oNode})`, { "event.action": "removeChild", "logrite.oNode": oNode });
	    return oNode;
    },

    hasChildNodes: function()
    {
	    return this.childNodes.length > 0;
    },

    getXml: function(oNode)
    {
		logger.error(`>>> Entering getXml(oNode=${oNode})`, { "event.action": "getXml", "logrite.oNode": oNode });
	    if (oNode == null)
		    oNode = this;

	    var sXml = '';
		logger.error(`getXml(oNode=${oNode}): sXml → ${sXml}`, { "event.action": "getXml", "logrite.oNode": oNode });
    	
	    if (oNode.nodeName != '__root')
		    sXml =  '<' + oNode.nodeName + this.getAttributes(oNode) + '>';
    	
	    for (var i=0; i<oNode.childNodes.length; i++)
	    {
		    sXml += this.getXml(oNode.childNodes[i]) + oNode.childNodes[i].text;				
	    }	
	    if (oNode.nodeName != '__root')
		    sXml = sXml + '</' + oNode.nodeName + '>';
	    return sXml;
    },

    getAttributes: function(oNode)
    {
		logger.error(`>>> Entering getAttributes(oNode=${oNode})`, { "event.action": "getAttributes", "logrite.oNode": oNode });
	    var sRet = '';
	    for (var sAttr in oNode.attributes)
		    sRet += ' ' + sAttr + '="' + dnn.encodeHTML(oNode.attributes[sAttr]) + '"';					
			logger.warn(`<<< Exiting getAttributes(oNode=${oNode})`, { "event.action": "getAttributes", "logrite.oNode": oNode });
	    return sRet;
    },

    getAttribute: function(sAttr)
    {
		logger.error(`>>> Entering getAttribute(sAttr=${sAttr})`, { "event.action": "getAttribute", "logrite.sAttr": sAttr });
		logger.warn(`<<< Exiting getAttribute(sAttr=${sAttr})`, { "event.action": "getAttribute", "logrite.sAttr": sAttr });
	    return this.attributes[sAttr];
    },

    setAttribute: function(sAttr, sVal)
    {
		logger.error(`>>> Entering setAttribute(sAttr=${sAttr},sVal=${sVal})`, { "event.action": "setAttribute", "logrite.sAttr": sAttr, "logrite.sVal": sVal });
	    this.attributes[sAttr] = sVal;
		logger.warn(`<<< Exiting setAttribute(sAttr=${sAttr},sVal=${sVal})`, { "event.action": "setAttribute", "logrite.sAttr": sAttr, "logrite.sVal": sVal });
    },

    removeAttribute: function(sAttr)
    {
		logger.error(`>>> Entering removeAttribute(sAttr=${sAttr})`, { "event.action": "removeAttribute", "logrite.sAttr": sAttr });
	    delete this.attributes[sAttr];
		logger.warn(`<<< Exiting removeAttribute(sAttr=${sAttr})`, { "event.action": "removeAttribute", "logrite.sAttr": sAttr });
    }
}
dnn.xml.JsXmlNode.registerClass('dnn.xml.JsXmlNode');    

//primitive Js Xml Parser 
//sure a regular expression guru could make better
dnn.xml.JsParser = function()
{
    this.pos = null;
    this.xmlArray = null;
    this.root = null;
}

dnn.xml.JsParser.prototype = 
{
    parse: function(sXml, oRoot)
    {
		logger.error(`>>> Entering parse(sXml=${sXml},oRoot=${oRoot})`, { "event.action": "parse", "logrite.sXml": sXml, "logrite.oRoot": oRoot });
	    this.pos = 0;
	    this.xmlArray = sXml.split('>');
		logger.debug(`parse(sXml=${sXml},oRoot=${oRoot}): this.pos → ${this.pos}`, { "event.action": "parse", "logrite.sXml": sXml, "logrite.oRoot": oRoot });
	    this.processXml(oRoot);
		logger.warn(`<<< Exiting parse(sXml=${sXml},oRoot=${oRoot})`, { "event.action": "parse", "logrite.sXml": sXml, "logrite.oRoot": oRoot });
    },

    getProcessString: function ()
    {
		logger.error('>>> Entering getProcessString()', { "event.action": "getProcessString" });
	    var s = this.xmlArray[this.pos];
	    if (s == null)
		    s = '';
		logger.error(`getProcessString(): s → ${s}`, { "event.action": "getProcessString" });
	    return s.replace(/^\s*/, "").replace(/\s*$/, ""); //trim off spaces on both sides
    },

    processXml: function(oParent)
    {
	    var oNewParent = oParent;
	    var bClose = this.isCloseTag();
	    var bOpen = this.isOpenTag();
	    while ((bClose == false || (bClose && bOpen)) && this.getProcessString().length > 0)
	    {
		    if (bClose)
		    {
			    this.processOpenTag(oParent);
			    this.pos +=1;
		    }
		    else
		    {
			    oNewParent = this.processOpenTag(oParent);
			    this.pos +=1;
			    this.processXml(oNewParent);
		    }					

		    bClose = this.isCloseTag();
		    bOpen = this.isOpenTag();
	    }
	    var s = this.getProcessString();		
	    if (bClose && s.substr(0,1) != '<')
		    oParent.text = s.substr(0,s.indexOf('<'));
	    this.pos +=1;
		logger.warn(`<<< Exiting processXml(oParent=${oParent})`, { "event.action": "processXml", "logrite.oParent": oParent });
    },

    isCloseTag: function()
    {
		logger.error('>>> Entering isCloseTag()', { "event.action": "isCloseTag" });
	    var s = this.getProcessString();
	    if (s.substr(0, 1) == '/' || s.indexOf('</') > -1 || s.substr(s.length-1) == '/')
		    return true;
	    else
		    return false;
    },

    isOpenTag: function()
    {
		logger.error('>>> Entering isOpenTag()', { "event.action": "isOpenTag" });
	    var s = this.getProcessString();
	    if (s.substr(0, 1) == '<' && s.substr(0, 2) != '</' && s.substr(0,2) != '<?')
		    return true;
	    else
		    return false;
    },

    processOpenTag: function(oParent)
    {		
		logger.error(`>>> Entering processOpenTag(oParent=${oParent})`, { "event.action": "processOpenTag", "logrite.oParent": oParent });
	    if (this.isOpenTag(this.getProcessString()))
	    {
		    var sArr = this.getProcessString().split(' ');
			logger.error(`processOpenTag(oParent=${oParent}): sArr → ${sArr}`, { "event.action": "processOpenTag", "logrite.oParent": oParent });
		    var oNode = new dnn.xml.JsXmlNode(oParent.ownerDocument);
			logger.error(`processOpenTag(oParent=${oParent}): oNode → ${oNode}`, { "event.action": "processOpenTag", "logrite.oParent": oParent });
			logger.debug(`processOpenTag(oParent=${oParent}): oNode.parentNode → ${oNode.parentNode}`, { "event.action": "processOpenTag", "logrite.oParent": oParent });
    		
		    oNode.nodeName = sArr[0].substr(1).replace('/', '');
		    oNode.parentNode = oParent;
		    this.processAttributes(oNode);
		    oParent.appendChild(oNode);
		    oParent = oNode;
			logger.warn(`<<< Exiting processOpenTag(oParent=${oParent})`, { "event.action": "processOpenTag", "logrite.oParent": oParent });
	    }
	    return oParent
    },

    processAttributes: function(oNode)
    {
		logger.error(`>>> Entering processAttributes(oNode=${oNode})`, { "event.action": "processAttributes", "logrite.oNode": oNode });
	    var s = this.getProcessString();
	    if (s.indexOf(' ') > -1)
		    s = s.substr(s.indexOf(' ') + 1);
			logger.error(`processAttributes(oNode=${oNode}): s → ${s}`, { "event.action": "processAttributes", "logrite.oNode": oNode });
    	
	    if (s.indexOf('=') > -1)
	    {
		    var bValue=false;
		    var sName='';
			logger.error(`processAttributes(oNode=${oNode}): sName → ${sName}`, { "event.action": "processAttributes", "logrite.oNode": oNode });
		    var sValue='';
			logger.error(`processAttributes(oNode=${oNode}): sValue → ${sValue}`, { "event.action": "processAttributes", "logrite.oNode": oNode });
		    var sChar;
		    for (var i=0; i<s.length; i++)
		    {
			    sChar = s.substr(i, 1);
				logger.error(`processAttributes(oNode=${oNode}): sChar → ${sChar}`, { "event.action": "processAttributes", "logrite.oNode": oNode });
			    if (sChar == '"')
			    {
					logger.debug(`processAttributes(oNode=${oNode}): sChar → ${sChar}`, { "event.action": "processAttributes", "logrite.oNode": oNode });
				    if (bValue)
				    {
					    //need to escape out special characters
					    oNode.attributes[sName] = dnn.decodeHTML(sValue);
					    sName = '';
					    sValue = '';
					    i++; //skip space
				    }						
				    bValue = !bValue;
			    }
			    else if (sChar != '=' || bValue==true)	//if inside value then allow =
			    {
				    if (bValue)
					    sValue += sChar;
				    else
					    sName += sChar;
			    }
		    }
	    }
    }
}
dnn.xml.JsParser.registerClass('dnn.xml.JsParser');    
