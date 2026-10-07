var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.util');

dnn.extend(dnn.util, {

tableReorderMove: function(ctl, bUp, sKey)
{
	logger.error(`>>> Entering tableReorderMove(ctl=${ctl},bUp=${bUp},sKey=${sKey})`, { "event.action": "tableReorderMove", "logrite.ctl": ctl, "logrite.bUp": bUp, "logrite.sKey": sKey });
	var oTR = dnn.dom.getParentByTagName(ctl, 'tr');

	logger.error(`tableReorderMove(ctl=${ctl},bUp=${bUp},sKey=${sKey}): oTR → ${oTR}`, { "event.action": "tableReorderMove", "logrite.ctl": ctl, "logrite.bUp": bUp, "logrite.sKey": sKey });
	if (oTR != null)
	{
		var oCtr = oTR.parentNode;
		var iIdx = oTR.rowIndex;
		if (dnn.dom.getAttr(oTR, 'origidx', '') == '-1')
			this.tableReorderSetOriginalIndexes(oCtr);

		var iNextIdx = (bUp ? this.tableReorderGetPrev(oCtr, iIdx-1) : this.tableReorderGetNext(oCtr, iIdx+1));
		logger.error(`tableReorderMove(ctl=${ctl},bUp=${bUp},sKey=${sKey}): iNextIdx → ${iNextIdx}`, { "event.action": "tableReorderMove", "logrite.ctl": ctl, "logrite.bUp": bUp, "logrite.sKey": sKey });
		if (iNextIdx > -1)
		{
			var aryValues = this.getInputValues(oTR);
			logger.error(`tableReorderMove(ctl=${ctl},bUp=${bUp},sKey=${sKey}): aryValues → ${aryValues}`, { "event.action": "tableReorderMove", "logrite.ctl": ctl, "logrite.bUp": bUp, "logrite.sKey": sKey });
			var aryValues2;
			var oSwapNode;
			dnn.dom.removeChild(oTR);
			if (oCtr.childNodes.length > iNextIdx)
			{
				oSwapNode = oCtr.childNodes[iNextIdx];
				aryValues2 = this.getInputValues(oSwapNode);
				logger.debug(`tableReorderMove(ctl=${ctl},bUp=${bUp},sKey=${sKey}): oSwapNode → ${oSwapNode}`, { "event.action": "tableReorderMove", "logrite.ctl": ctl, "logrite.bUp": bUp, "logrite.sKey": sKey });
				logger.error(`tableReorderMove(ctl=${ctl},bUp=${bUp},sKey=${sKey}): aryValues2 → ${aryValues2}`, { "event.action": "tableReorderMove", "logrite.ctl": ctl, "logrite.bUp": bUp, "logrite.sKey": sKey });
				oCtr.insertBefore(oTR, oSwapNode);
			}
			else
				oCtr.appendChild(oTR);
			this.setInputValues(oTR, aryValues);
			logger.debug(`tableReorderMove(ctl=${ctl},bUp=${bUp},sKey=${sKey}): aryValues2 → ${aryValues2}`, { "event.action": "tableReorderMove", "logrite.ctl": ctl, "logrite.bUp": bUp, "logrite.sKey": sKey });
			if (oSwapNode)
				this.setInputValues(oSwapNode, aryValues2);
			
			dnn.setVar(sKey,this.tableReorderGetNewRowOrder(oCtr));
			logger.warn(`<<< Exiting tableReorderMove(ctl=${ctl},bUp=${bUp},sKey=${sKey})`, { "event.action": "tableReorderMove", "logrite.ctl": ctl, "logrite.bUp": bUp, "logrite.sKey": sKey });
		}
		return true;//handled client-side
	}
	return false;	//cause postback
},

getInputValues: function(oCtl)
{
	var aryInputs = dnn.dom.getByTagName('input', oCtl);
	var aryValues = new Array();
	for (var i=0; i<aryInputs.length; i++)
	{
		if (aryInputs[i].type == 'checkbox')
			aryValues[i] = aryInputs[i].checked;
	}
	logger.warn(`<<< Exiting getInputValues(oCtl=${oCtl})`, { "event.action": "getInputValues", "logrite.oCtl": oCtl });
	return aryValues;
},

setInputValues: function(oCtl, aryValues)
{
	var aryInputs = dnn.dom.getByTagName('input', oCtl);
	for (var i=0; i<aryInputs.length; i++)
	{
		if (aryInputs[i].type == 'checkbox')
			aryInputs[i].checked = aryValues[i];
	}
	logger.warn(`<<< Exiting setInputValues(oCtl=${oCtl},aryValues=${aryValues})`, { "event.action": "setInputValues", "logrite.oCtl": oCtl, "logrite.aryValues": aryValues });
},

tableReorderGetNext: function (oParent, iStartIdx)
{
	logger.error(`>>> Entering tableReorderGetNext(oParent=${oParent},iStartIdx=${iStartIdx})`, { "event.action": "tableReorderGetNext", "logrite.oParent": oParent, "logrite.iStartIdx": iStartIdx });
	for (var i=iStartIdx; i<oParent.childNodes.length; i++)
	{
		var oCtl = oParent.childNodes[i];
		if (dnn.dom.getAttr(oCtl, 'origidx', '') != '')
			return i;
	}
	return -1;
},

tableReorderGetPrev: function(oParent, iStartIdx)
{
	logger.error(`>>> Entering tableReorderGetPrev(oParent=${oParent},iStartIdx=${iStartIdx})`, { "event.action": "tableReorderGetPrev", "logrite.oParent": oParent, "logrite.iStartIdx": iStartIdx });
	for (var i=iStartIdx; i>=0; i--)
	{
		var oCtl = oParent.childNodes[i];
		if (dnn.dom.getAttr(oCtl, 'origidx', '') != '')
			return i;
	}
	return -1;
},

tableReorderSetOriginalIndexes: function(oParent)
{
	logger.error(`>>> Entering tableReorderSetOriginalIndexes(oParent=${oParent})`, { "event.action": "tableReorderSetOriginalIndexes", "logrite.oParent": oParent });
	var iIdx=0;
	for (var i=0; i<oParent.childNodes.length; i++)
	{
		var oCtl = oParent.childNodes[i];
		logger.error(`tableReorderSetOriginalIndexes(oParent=${oParent}): oCtl → ${oCtl}`, { "event.action": "tableReorderSetOriginalIndexes", "logrite.oParent": oParent });
		if (dnn.dom.getAttr(oCtl, 'origidx', '') != '')
		{
			oCtl.setAttribute('origidx', iIdx.toString());
			iIdx++;
			logger.warn(`<<< Exiting tableReorderSetOriginalIndexes(oParent=${oParent})`, { "event.action": "tableReorderSetOriginalIndexes", "logrite.oParent": oParent });
		}
	}
},

tableReorderGetNewRowOrder: function(oParent)
{
	logger.error(`>>> Entering tableReorderGetNewRowOrder(oParent=${oParent})`, { "event.action": "tableReorderGetNewRowOrder", "logrite.oParent": oParent });
	var sIdx;
	var sRet='';
	logger.error(`tableReorderGetNewRowOrder(oParent=${oParent}): sRet → ${sRet}`, { "event.action": "tableReorderGetNewRowOrder", "logrite.oParent": oParent });
	for (var i=0; i<oParent.childNodes.length; i++)
	{
		var oCtl = oParent.childNodes[i];
		sIdx = dnn.dom.getAttr(oCtl, 'origidx', '');
		if (sIdx != '')
			sRet+= (sRet.length>0 ? ',':'') + sIdx;
			logger.warn(`<<< Exiting tableReorderGetNewRowOrder(oParent=${oParent})`, { "event.action": "tableReorderGetNewRowOrder", "logrite.oParent": oParent });
	}
	return sRet;
},

//doesn't belong here...   Don't use outside core, it will be moved
checkallChecked: function(oCtl, iCellIndex)
{
	logger.error(`>>> Entering checkallChecked(oCtl=${oCtl},iCellIndex=${iCellIndex})`, { "event.action": "checkallChecked", "logrite.oCtl": oCtl, "logrite.iCellIndex": iCellIndex });
	var bChecked = oCtl.checked;
	var oTD = dnn.dom.getParentByTagName(oCtl, 'td');
	var oTR = oTD.parentNode;
	var oCtr = oTR.parentNode;
	var iOffset=0;
	
	var oTemp;
	for (var i=0; i<iCellIndex;i++) //firefox has text nodes
	{
		if (oTR.childNodes[i].tagName == null)	
			iOffset++;	
	}

	var oChk;
	for (var i=0; i<oCtr.childNodes.length; i++)
	{
		oTR = oCtr.childNodes[i];
		oTD = oTR.childNodes[iCellIndex+iOffset];
		if (oTD != null)
		{
			oChk = dnn.dom.getByTagName('input', oTD);
			if (oChk.length > 0)
				oChk[0].checked = bChecked;		
		}
	}
	//return true;
	
	logger.warn(`<<< Exiting checkallChecked(oCtl=${oCtl},iCellIndex=${iCellIndex})`, { "event.action": "checkallChecked", "logrite.oCtl": oCtl, "logrite.iCellIndex": iCellIndex });
}
});
