var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />

//--- dnn.diagnostics
Type.registerNamespace('dnn.diagnostics');

dnn.extend(dnn.diagnostics, {
	pns: 'dnn',
	ns: 'diagnostics',
	parserName: null,
	debugCtl: null,
	debugWait: (document.all != null),
	debugArray: [],

    clearDebug: function()
    {
		logger.error('>>> Entering clearDebug()', { "event.action": "clearDebug" });
	    if (this.debugCtl != null)
	    {
		    this.debugCtl.value = '';
			logger.warn('<<< Exiting clearDebug()', { "event.action": "clearDebug" });
		    return true;
	    }
	    return false;
    },

    displayDebug: function(sText)
    {
		logger.error(`>>> Entering displayDebug(sText=${sText})`, { "event.action": "displayDebug", "logrite.sText": sText });
	    if (this.debugCtl == null)
	    {
		    if (dnn.dom.browser.type == dnn.dom.browser.InternetExplorer)
		    {
			    var oBody = dnn.dom.getByTagName("body")[0];
				logger.error(`displayDebug(sText=${sText}): oBody → ${oBody}`, { "event.action": "displayDebug", "logrite.sText": sText });
			    if (this.debugWait && oBody.readyState != 'complete')
			    {
				    dnn.debugWait = true;
				    this.debugArray[this.debugArray.length] = sText;
				    //document.attachEvent('onreadystate', __dnn_documentLoaded);
				    if (oBody.onload == null || oBody.onload.toString().indexOf('__dnn_documentLoaded') == -1)
					    oBody.onload = dnn.dom.appendFunction(oBody.onload, '__dnn_documentLoaded()');
				    return;
			    }
		    }
		    this.debugCtl = dnn.dom.getById('__dnnDebugOutput');
		    if (this.debugCtl == null)
		    {
			    this.debugCtl = dnn.dom.createElement('TEXTAREA');
			    this.debugCtl.id = '__dnnDebugOutput';
			    this.debugCtl.rows=10;
			    this.debugCtl.cols=100;
			    dnn.dom.appendChild(oBody, this.debugCtl);
		    }
		    this.debugCtl.style.display = 'block';
	    }
    	
	    if (dnn.diagnostics.debugCtl == null)
		    alert(sText);
	    else
		    dnn.diagnostics.debugCtl.value += sText + '\n';
    	
		logger.warn(`<<< Exiting displayDebug(sText=${sText})`, { "event.action": "displayDebug", "logrite.sText": sText });
	    return true;
    },

    assertCheck: function(sCom, bVal, sMsg)
    {
		logger.error(`>>> Entering assertCheck(sCom=${sCom},bVal=${bVal},sMsg=${sMsg})`, { "event.action": "assertCheck", "logrite.sCom": sCom, "logrite.bVal": bVal, "logrite.sMsg": sMsg });
	    if (!bVal)
		    this.displayDebug(sCom + ' - FAILED (' + sMsg + ')');
	    else if (this.verbose)
		    this.displayDebug(sCom + ' - PASSED');
    },

    assert: function(sCom, bVal) 
    {
	logger.error(`>>> Entering assert(sCom=${sCom},bVal=${bVal})`, { "event.action": "assert", "logrite.sCom": sCom, "logrite.bVal": bVal });
      this.assertCheck(sCom, bVal == true, 'Testing assert(boolean) for true');
	logger.warn(`<<< Exiting assert(sCom=${sCom},bVal=${bVal})`, { "event.action": "assert", "logrite.sCom": sCom, "logrite.bVal": bVal });
    },

    assertTrue: function(sCom, bVal)
    {
	logger.error(`>>> Entering assertTrue(sCom=${sCom},bVal=${bVal})`, { "event.action": "assertTrue", "logrite.sCom": sCom, "logrite.bVal": bVal });
      this.assertCheck(sCom, bVal == true, 'Testing assert(boolean) for true');
	logger.warn(`<<< Exiting assertTrue(sCom=${sCom},bVal=${bVal})`, { "event.action": "assertTrue", "logrite.sCom": sCom, "logrite.bVal": bVal });
    },

    assertFalse: function(sCom, bVal)
    {
	logger.error(`>>> Entering assertFalse(sCom=${sCom},bVal=${bVal})`, { "event.action": "assertFalse", "logrite.sCom": sCom, "logrite.bVal": bVal });
      this.assertCheck(sCom, bVal == false, 'Testing assert(boolean) for false');
	logger.warn(`<<< Exiting assertFalse(sCom=${sCom},bVal=${bVal})`, { "event.action": "assertFalse", "logrite.sCom": sCom, "logrite.bVal": bVal });
    },

    assertEquals: function(sCom, sVal1, sVal2)
    {
	logger.error(`>>> Entering assertEquals(sCom=${sCom},sVal1=${sVal1},sVal2=${sVal2})`, { "event.action": "assertEquals", "logrite.sCom": sCom, "logrite.sVal1": sVal1, "logrite.sVal2": sVal2 });
      this.assertCheck(sCom, sVal1 == sVal2, 'Testing Equals: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') != ' + this._safeString(sVal2) + ' (' + typeof(sVal2) + ')');
	logger.warn(`<<< Exiting assertEquals(sCom=${sCom},sVal1=${sVal1},sVal2=${sVal2})`, { "event.action": "assertEquals", "logrite.sCom": sCom, "logrite.sVal1": sVal1, "logrite.sVal2": sVal2 });
    },

    assertNotEquals: function(sCom, sVal1, sVal2)
    {
	logger.error(`>>> Entering assertNotEquals(sCom=${sCom},sVal1=${sVal1},sVal2=${sVal2})`, { "event.action": "assertNotEquals", "logrite.sCom": sCom, "logrite.sVal1": sVal1, "logrite.sVal2": sVal2 });
      this.assertCheck(sCom, sVal1 != sVal2, 'Testing NotEquals: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') == ' + this._safeString(sVal2) + ' (' + typeof(sVal2) + ')');
	logger.warn(`<<< Exiting assertNotEquals(sCom=${sCom},sVal1=${sVal1},sVal2=${sVal2})`, { "event.action": "assertNotEquals", "logrite.sCom": sCom, "logrite.sVal1": sVal1, "logrite.sVal2": sVal2 });
    },

    assertNull: function (sCom, sVal1)
    {
		logger.error(`>>> Entering assertNull(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertNull", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
	    this.assertCheck(sCom, sVal1 == null, 'Testing null: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') != null');
		logger.warn(`<<< Exiting assertNull(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertNull", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
    },

    assertNotNull: function (sCom, sVal1)
    {
		logger.error(`>>> Entering assertNotNull(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertNotNull", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
	    this.assertCheck(sCom, sVal1 != null, 'Testing for null: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') == null');
		logger.warn(`<<< Exiting assertNotNull(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertNotNull", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
    },

    assertStringLength: function (sCom, sVal1)
    {
		logger.error(`>>> Entering assertStringLength(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertStringLength", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
	    this.assertCheck(sCom, ((sVal1 == null) ? false : sVal1.length > 0), 'Testing for string length: ' + this._safeString(sVal1) + ' (' + ((sVal1 == null) ? 'null' : sVal1.length) + ')');
		logger.warn(`<<< Exiting assertStringLength(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertStringLength", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
    },

    assertNaN: function (sCom, sVal1)
    {
		logger.error(`>>> Entering assertNaN(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertNaN", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
	    this.assertCheck(sCom, isNaN(sVal1), 'Testing for NaN: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') is a number');
		logger.warn(`<<< Exiting assertNaN(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertNaN", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
    },

    assertNotNaN: function (sCom, sVal1)
    {
		logger.error(`>>> Entering assertNotNaN(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertNotNaN", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
	    this.assertCheck(sCom, isNaN(sVal1) == false, 'Testing for NotNaN: ' + this._safeString(sVal1) + ' (' + typeof(sVal1) + ') is NOT a number');
		logger.warn(`<<< Exiting assertNotNaN(sCom=${sCom},sVal1=${sVal1})`, { "event.action": "assertNotNaN", "logrite.sCom": sCom, "logrite.sVal1": sVal1 });
    },
    
    _safeString: function(s)
    {
		logger.error(`>>> Entering _safeString(s=${s})`, { "event.action": "_safeString", "logrite.s": s });
	    if (typeof(s) == 'string' || typeof(s) == 'number')
		    return s;
	    else
		    return typeof(s);
    }
});


//--- End dnn.diagnostics
//dnn_diagnosticTests(dnn);

var __dnn_m_aryHandled=new Array();
function dnn_diagnosticTests(oParent)
{
	logger.error(`>>> Entering dnn_diagnosticTests(oParent=${oParent})`, { "event.action": "dnn_diagnosticTests", "logrite.oParent": oParent });
	if (oParent.ns == 'dnn')
		dnn.diagnostics.clearDebug();
	if (typeof(oParent.UnitTests) == 'function')
	{
		dnn.diagnostics.displayDebug('------- Starting ' + oParent.pns + '.' + oParent.ns + ' tests (v.' + (oParent.apiversion ? oParent.apiversion : dnn.apiversion) + ') ' + new Date().toString() + ' -------');
		oParent.UnitTests();
	}
	
	for (var obj in oParent)
	{
		if (oParent[obj] != null && typeof(oParent[obj]) == 'object' && __dnn_m_aryHandled[obj] == null)
		{
			//if (obj != 'debugCtl')	//what is this IE object???
			if (oParent[obj].pns != null)
				dnn_diagnosticTests(oParent[obj]);
		}
		//__dnn_m_aryHandled[obj] = true;
	}
}

function __dnn_documentLoaded()
{
	logger.error('>>> Entering __dnn_documentLoaded()', { "event.action": "__dnn_documentLoaded" });
	dnn.diagnostics.debugWait = false;
	dnn.diagnostics.displayDebug('document loaded... avoiding Operation Aborted IE bug');
	dnn.diagnostics.displayDebug(dnn.diagnostics.debugArray.join('\n'));
	
	logger.warn('<<< Exiting __dnn_documentLoaded()', { "event.action": "__dnn_documentLoaded" });
}
