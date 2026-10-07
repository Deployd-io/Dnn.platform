var logger = (typeof logger !== 'undefined' && logger) || console;

Type.registerNamespace('dnn.util');dnn.extend(dnn.util, { tableReorderMove: function (ctl, bUp, sKey) {
	logger.error('>>> Entering tableReorderMove()', { "event.action": "tableReorderMove" });
    var oTR = dnn.dom.getParentByTagName(ctl, 'tr'); if (oTR != null) {
        var oCtr = oTR.parentNode; if (oCtr.childNodes[oCtr.childNodes.length - 1].nodeName == "#text") { dnn.dom.removeChild(oCtr.childNodes[oCtr.childNodes.length - 1]); }; var iIdx = oTR.rowIndex; if (dnn.dom.getAttr(oTR, 'origidx', '') == '-1')
            this.tableReorderSetOriginalIndexes(oCtr); var iNextIdx = (bUp ? this.tableReorderGetPrev(oCtr, iIdx - 1) : this.tableReorderGetNext(oCtr, iIdx + 1)); if (iNextIdx > -1) {
			logger.error(`tableReorderMove(): oCtr → ${oCtr}`, { "event.action": "tableReorderMove" });
            var aryValues = this.getInputValues(oTR); var aryValues2; var oSwapNode; dnn.dom.removeChild(oTR); if (oCtr.childNodes.length > iNextIdx)
            { oSwapNode = oCtr.childNodes[iNextIdx]; aryValues2 = this.getInputValues(oSwapNode); oCtr.insertBefore(oTR, oSwapNode); }
            else
                oCtr.appendChild(oTR); this.setInputValues(oTR, aryValues); if (oSwapNode)
                this.setInputValues(oSwapNode, aryValues2); dnn.setVar(sKey, this.tableReorderGetNewRowOrder(oCtr));
        }
		logger.error(`tableReorderMove(): iNextIdx → ${iNextIdx}`, { "event.action": "tableReorderMove" });
        return true;
    }
	logger.error(`tableReorderMove(): oTR → ${oTR}`, { "event.action": "tableReorderMove" });
	logger.warn('<<< Exiting tableReorderMove()', { "event.action": "tableReorderMove" });
    return false;
}, getInputValues: function (oCtl) {
    var aryInputs = dnn.dom.getByTagName('input', oCtl); var aryValues = new Array(); for (var i = 0; i < aryInputs.length; i++) {
        if (aryInputs[i].type == 'checkbox')
            aryValues[i] = aryInputs[i].checked;
    }
	logger.error(`tableReorderMove(): aryValues → ${aryValues}`, { "event.action": "tableReorderMove" });
    return aryValues;
}, setInputValues: function (oCtl, aryValues) {
    var aryInputs = dnn.dom.getByTagName('input', oCtl); for (var i = 0; i < aryInputs.length; i++) {
        if (aryInputs[i].type == 'checkbox')
            aryInputs[i].checked = aryValues[i];
    }
}, tableReorderGetNext: function (oParent, iStartIdx) {
	logger.error(`tableReorderMove(): aryInputs → ${aryInputs}`, { "event.action": "tableReorderMove" });
    for (var i = iStartIdx; i < oParent.childNodes.length; i++) {
        var oCtl = oParent.childNodes[i]; if (dnn.dom.getAttr(oCtl, 'origidx', '') != '')
            return i;
    }
    return -1;
}, tableReorderGetPrev: function (oParent, iStartIdx) {
    for (var i = iStartIdx; i >= 0; i--) {
        var oCtl = oParent.childNodes[i]; if (dnn.dom.getAttr(oCtl, 'origidx', '') != '')
            return i;
    }
    return -1;
}, tableReorderSetOriginalIndexes: function (oParent) {
    var iIdx = 0; for (var i = 0; i < oParent.childNodes.length; i++) {
        var oCtl = oParent.childNodes[i]; if (dnn.dom.getAttr(oCtl, 'origidx', '') != '')
        { oCtl.setAttribute('origidx', iIdx.toString()); iIdx++; }
    }
}, tableReorderGetNewRowOrder: function (oParent) {
	logger.error(`tableReorderMove(): iIdx → ${iIdx}`, { "event.action": "tableReorderMove" });
    var sIdx; var sRet = ''; for (var i = 0; i < oParent.childNodes.length; i++) {
        var oCtl = oParent.childNodes[i]; sIdx = dnn.dom.getAttr(oCtl, 'origidx', ''); if (sIdx != '')
            sRet += (sRet.length > 0 ? ',' : '') + sIdx;
			logger.error(`tableReorderMove(): oCtl → ${oCtl}`, { "event.action": "tableReorderMove" });
    }
	logger.error(`tableReorderMove(): sRet → ${sRet}`, { "event.action": "tableReorderMove" });
    return sRet;
}, checkallChecked: function (oCtl, iCellIndex) {
    setTimeout(function () {
        var bChecked = oCtl.checked; var oTD = dnn.dom.getParentByTagName(oCtl, 'td'); var oTR = oTD.parentNode; var oCtr = oTR.parentNode; var iOffset = 0; var oTemp; 
        for (var i = 0; i < iCellIndex; i++) {
            if (oTR.childNodes[i].tagName == null)
                iOffset++;
        }
        var oChk; for (var i = 0; i < oCtr.childNodes.length; i++) {
            oTR = oCtr.childNodes[i]; oTD = oTR.childNodes[iCellIndex + iOffset]; if (oTD != null) {
                oChk = dnn.dom.getByTagName('input', oTD); if (oChk.length > 0)
                    oChk[0].checked = bChecked;
            }
        }
    }, 10);
}
});
