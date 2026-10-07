var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

dnn.xmlhttp.parserName = 'JS';  //force JS version

dnn.xmlhttp.JsXmlHttpRequest = function ()
{
	dnn.xmlhttp.contextId += 1;
	this.contextId = dnn.xmlhttp.contextId;
	this.method = null;
	this.url = null;
	this.async = true;
	this.doc = null;
	
	this.iframe = document.createElement('IFRAME');
	this.iframe.name = 'dnniframe' + this.contextId;
	this.iframe.id = 'dnniframe' + this.contextId;
	this.iframe.src = '';	//TODO:  FIX FOR SSL!!!
	this.iframe.height = 0;
	this.iframe.width = 0;
	this.iframe.style.visibility = 'hidden';
	document.body.appendChild(this.iframe);	
}

dnn.xmlhttp.JsXmlHttpRequest.prototype = 
{
    open: function(sMethod, sURL, bAsync)
    {
		logger.error(`>>> Entering open(sMethod=${sMethod},sURL=${sURL},bAsync=${bAsync})`, { "event.action": "open", "logrite.sMethod": sMethod, "logrite.sURL": sURL, "logrite.bAsync": bAsync });
	    this.method = sMethod;
	    this.url = sURL;
		logger.debug(`open(sMethod=${sMethod},sURL=${sURL},bAsync=${bAsync}): this.method → ${this.method}`, { "event.action": "open", "logrite.sMethod": sMethod, "logrite.sURL": sURL, "logrite.bAsync": bAsync });
	    this.async = bAsync;
		logger.debug(`open(sMethod=${sMethod},sURL=${sURL},bAsync=${bAsync}): this.async → ${this.async}`, { "event.action": "open", "logrite.sMethod": sMethod, "logrite.sURL": sURL, "logrite.bAsync": bAsync });
    },

    send: function(postData)
    {
		logger.error(`>>> Entering send(postData=${postData})`, { "event.action": "send", "logrite.postData": postData });
	    this.assignIFrameDoc();

	    if (this.doc == null)	//opera does not allow access to iframe right away
	    {
		    window.setTimeout(dnn.dom.getObjMethRef(this, 'send'), 1000);
		    return;
	    }
	    this.doc.open();
	    this.doc.write('<html><body>');
	    this.doc.write('<form name="TheForm" method="post" target="" ');
	    var sSep = '?';
		logger.error(`send(postData=${postData}): sSep → ${sSep}`, { "event.action": "send", "logrite.postData": postData });
	    if (this.url.indexOf('?') > -1)
		    sSep = '&';
    		
	    this.doc.write(' action="' + this.url + sSep + '__U=' + this.getUnique() + '">');
	    this.doc.write('<input type="hidden" name="ctx" value="' + this.contextId + '">');
	    if (postData && postData.length > 0)
	    {
		    var aryData = postData.split('&');
			logger.error(`send(postData=${postData}): aryData → ${aryData}`, { "event.action": "send", "logrite.postData": postData });
		    for (var i=0; i<aryData.length; i++)
			    this.doc.write('<input type="hidden" name="' + aryData[i].split('=')[0] + '" value="' + aryData[i].split('=')[1] + '">');
	    }
	    this.doc.write('</form></body></html>');
	    this.doc.close();

	    this.assignIFrameDoc();	//opera needs this reassigned after we wrote to it
    	
	    this.doc.forms[0].submit();
    	
		logger.warn(`<<< Exiting send(postData=${postData})`, { "event.action": "send", "logrite.postData": postData });
    },

    assignIFrameDoc: function()
    {
		logger.error('>>> Entering assignIFrameDoc()', { "event.action": "assignIFrameDoc" });
	    if (this.iframe.contentDocument) 
		    this.doc = this.iframe.contentDocument; 
	    else if (this.iframe.contentWindow) 
		    this.doc = this.iframe.contentWindow.document; 
	    else if (window.frames[this.iframe.name]) 
		    this.doc = window.frames[this.iframe.name].document; 
    },

    getResponseHeader: function(sKey)
    {
		logger.error(`>>> Entering getResponseHeader(sKey=${sKey})`, { "event.action": "getResponseHeader", "logrite.sKey": sKey });
	    this.assignIFrameDoc();
	    var oCtl = dnn.dom.getById(sKey, this.doc);
	    if (oCtl != null)
		    return oCtl.value;
	    else
		    return 'WARNING:  response header not found';
    },

    getUnique: function ()
    {
		logger.error('>>> Entering getUnique()', { "event.action": "getUnique" });
		logger.warn('<<< Exiting getUnique()', { "event.action": "getUnique" });
	    return new Date().getTime();
    }
}

dnn.xmlhttp.JsXmlHttpRequest.registerClass('dnn.xmlhttp.JsXmlHttpRequest');