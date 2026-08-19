const logger = console;
//General
//for example: instead of each module writing out script found in moduleMaxMin_OnClick have the functionality cached
//

var DNN_COL_DELIMITER = String.fromCharCode(16);
var DNN_ROW_DELIMITER = String.fromCharCode(15);
var __dnn_m_bPageLoaded = false;

if (window.addEventListener)
    window.addEventListener("load", __dnn_Page_OnLoad, false);
else
    window.attachEvent("onload", __dnn_Page_OnLoad);

function __dnn_ClientAPIEnabled()
{
	logger.debug('>>> Entering __dnn_ClientAPIEnabled()');
	logger.debug('<<< Exiting __dnn_ClientAPIEnabled()');
    return typeof(dnn) != 'undefined' && typeof (dnn.dom) != 'undefined';
}

function __dnn_Page_OnLoad()
{
	logger.debug('>>> Entering __dnn_Page_OnLoad()');
	if (__dnn_ClientAPIEnabled())
	{	
		dnn.dom.attachEvent(window, 'onscroll', __dnn_bodyscroll);
	}
	__dnn_m_bPageLoaded = true;
	logger.debug('<<< Exiting __dnn_Page_OnLoad()');
}

function __dnn_KeyDown(iKeyCode, sFunc, e)
{
	logger.debug(`>>> Entering __dnn_KeyDown(iKeyCode=${iKeyCode},sFunc=${sFunc},e=${e})`);
	if (e == null)
		e = window.event;

	if (e.keyCode == iKeyCode)
	{
		eval(unescape(sFunc));
		logger.debug(`<<< Exiting __dnn_KeyDown(iKeyCode=${iKeyCode},sFunc=${sFunc},e=${e})`);
		return false;
	}
}

function __dnn_bodyscroll() 
{
	logger.debug('>>> Entering __dnn_bodyscroll()');
	var oF=document.forms[0];	
	if (__dnn_ClientAPIEnabled() && __dnn_m_bPageLoaded && typeof(oF.ScrollTop) != 'undefined')
		oF.ScrollTop.value=document.documentElement.scrollTop ? document.documentElement.scrollTop : dnn.dom.getByTagName("body")[0].scrollTop;
		logger.debug('<<< Exiting __dnn_bodyscroll()');
}

function __dnn_setScrollTop(iTop)
{
	if (__dnn_ClientAPIEnabled())
	{
		if (iTop == null)
			iTop = document.forms[0].ScrollTop.value;
	
		var sID = dnn.getVar('ScrollToControl');
		logger.debug(`__dnn_setScrollTop(iTop=${iTop}): sID → ${sID}`);
		if (sID != null && sID.length > 0)
		{
			var oCtl = dnn.dom.getById(sID);
			logger.debug(`__dnn_setScrollTop(iTop=${iTop}): oCtl → ${oCtl}`);
			if (oCtl != null)
			{
				iTop = dnn.dom.positioning.elementTop(oCtl);
				dnn.setVar('ScrollToControl', '');
				logger.debug(`__dnn_setScrollTop(iTop=${iTop}): iTop → ${iTop}`);
			}
		}

		if (document.getElementsByTagName("html")[0].style["overflow"] != "hidden") {
			logger.debug(`>>> Entering __dnn_setScrollTop(iTop=${iTop})`);
			window.scrollTo(0, iTop);
			logger.debug(`<<< Exiting __dnn_setScrollTop(iTop=${iTop})`);
		}
	}
}

//Focus logic
function __dnn_SetInitialFocus(sID)
{
	logger.debug(`>>> Entering __dnn_SetInitialFocus(sID=${sID})`);
	var oCtl = dnn.dom.getById(sID);	
	logger.debug(`__dnn_SetInitialFocus(sID=${sID}): oCtl → ${oCtl}`);
	if (oCtl != null && __dnn_CanReceiveFocus(oCtl))
		oCtl.focus();
		logger.debug(`<<< Exiting __dnn_SetInitialFocus(sID=${sID})`);
}	

function __dnn_CanReceiveFocus(e)
{
	logger.debug(`>>> Entering __dnn_CanReceiveFocus(e=${e})`);
	//probably should call getComputedStyle for classes that cause item to be hidden
	if (e.style.display != 'none' && e.tabIndex > -1 && e.disabled == false && e.style.visible != 'hidden')
	{
		var eParent = e.parentElement;
		while (eParent != null && eParent.tagName != 'BODY')
		{
			if (eParent.style.display == 'none' || eParent.disabled || eParent.style.visible == 'hidden')
				return false;
			eParent = eParent.parentElement;
		}
		return true;
	}
	else
		return false;
}

//Max/Min Script
function __dnn_ContainerMaxMin_OnClick(oLnk, sContentID)
{
	logger.debug(`>>> Entering __dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID})`);
	var oContent = dnn.dom.getById(sContentID);
	logger.debug(`__dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID}): oContent → ${oContent}`);
	if (oContent != null)
	{
		var oBtn = oLnk.childNodes[0];
		var sContainerID = dnn.getVar('containerid_' + sContentID); //oLnk.getAttribute('containerid');
		logger.debug(`__dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID}): sContainerID → ${sContainerID}`);
		var sCookieID = dnn.getVar('cookieid_' + sContentID); //oLnk.getAttribute('cookieid');
		var sCurrentFile = oBtn.src.toLowerCase().substr(oBtn.src.lastIndexOf('/'));
		logger.debug(`__dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID}): sCurrentFile → ${sCurrentFile}`);
		var sMaxFile;
		var sMaxIcon;
		var sMinIcon;

		if (dnn.getVar('min_icon_' + sContainerID))
			sMinIcon = dnn.getVar('min_icon_' + sContainerID);
		else
			sMinIcon = dnn.getVar('min_icon');

		logger.debug(`__dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID}): sMinIcon → ${sMinIcon}`);
		if (dnn.getVar('max_icon_' + sContainerID))
			sMaxIcon = dnn.getVar('max_icon_' + sContainerID);
		else
			sMaxIcon = dnn.getVar('max_icon');

		logger.debug(`__dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID}): sMaxIcon → ${sMaxIcon}`);
		sMaxFile = sMaxIcon.toLowerCase().substr(sMaxIcon.lastIndexOf('/'));

		logger.debug(`__dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID}): sMaxFile → ${sMaxFile}`);
		var iNum = 5;

		logger.debug(`__dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID}): iNum → ${iNum}`);
		var animf = dnn.getVar('animf_' + sContentID);
		logger.debug(`__dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID}): animf → ${animf}`);
		if (animf != null)
			iNum = new Number(animf);
			
		if (sCurrentFile == sMaxFile)
		{
			oBtn.src = sMinIcon;				
			logger.debug(`__dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID}): oBtn.src → ${oBtn.src}`);
			//oContent.style.display = '';
			dnn.dom.expandElement(oContent, iNum);
			oBtn.title = dnn.getVar('min_text');
			if (sCookieID != null)
			{
				if (dnn.getVar('__dnn_' + sContainerID + ':defminimized') == 'true')
					dnn.dom.setCookie(sCookieID, 'true', 365);
				else
					dnn.dom.deleteCookie(sCookieID);
			}
			else
				dnn.setVar('__dnn_' + sContainerID + '_Visible', 'true');
		}
		else
		{
			oBtn.src = sMaxIcon;				
			//oContent.style.display = 'none';
			dnn.dom.collapseElement(oContent, iNum);
			oBtn.title = dnn.getVar('max_text');
			if (sCookieID != null)
			{
				if (dnn.getVar('__dnn_' + sContainerID + ':defminimized') == 'true')
					dnn.dom.deleteCookie(sCookieID);
				else
					dnn.dom.setCookie(sCookieID, 'false', 365);				
			}
			else
				dnn.setVar('__dnn_' + sContainerID + '_Visible', 'false');			
		}
		
	logger.debug(`<<< Exiting __dnn_ContainerMaxMin_OnClick(oLnk=${oLnk},sContentID=${sContentID})`);
		return true;	//cancel postback
	}
	return false;	//failed so do postback
}

function __dnn_Help_OnClick(sHelpID)
{
	logger.debug(`>>> Entering __dnn_Help_OnClick(sHelpID=${sHelpID})`);
	var oHelp = dnn.dom.getById(sHelpID);
	logger.debug(`__dnn_Help_OnClick(sHelpID=${sHelpID}): oHelp → ${oHelp}`);
	if (oHelp != null)
	{
		if (oHelp.style.display == 'none')
			oHelp.style.display = '';
		else
			oHelp.style.display = 'none';

	logger.debug(`<<< Exiting __dnn_Help_OnClick(sHelpID=${sHelpID})`);
		return true;	//cancel postback
	}
	return false;	//failed so do postback
}

function __dnn_SectionMaxMin(oBtn, sContentID)
{
	logger.debug(`>>> Entering __dnn_SectionMaxMin(oBtn=${oBtn},sContentID=${sContentID})`);
	var oContent = dnn.dom.getById(sContentID);
	logger.debug(`__dnn_SectionMaxMin(oBtn=${oBtn},sContentID=${sContentID}): oContent → ${oContent}`);
	if (oContent != null)
	{
		var sMaxIcon = oBtn.getAttribute('max_icon');
		logger.debug(`__dnn_SectionMaxMin(oBtn=${oBtn},sContentID=${sContentID}): sMaxIcon → ${sMaxIcon}`);
		var sMinIcon = oBtn.getAttribute('min_icon');
		logger.debug(`__dnn_SectionMaxMin(oBtn=${oBtn},sContentID=${sContentID}): sMinIcon → ${sMinIcon}`);
		var bCallback = oBtn.getAttribute('userctr') != null;
		logger.debug(`__dnn_SectionMaxMin(oBtn=${oBtn},sContentID=${sContentID}): bCallback → ${bCallback}`);
		var sVal;
		if (oContent.style.display == 'none')
		{
			oBtn.src = sMinIcon;				
			logger.debug(`__dnn_SectionMaxMin(oBtn=${oBtn},sContentID=${sContentID}): oBtn.src → ${oBtn.src}`);
			oContent.style.display = '';
			if (bCallback)
			    sVal = 'True';
			else
			    dnn.setVar(oBtn.id + ':exp', 1);
		}
		else
		{
			oBtn.src = sMaxIcon;				
			oContent.style.display = 'none';
			if (bCallback)
			    sVal = 'False';
			else
                dnn.setVar(oBtn.id + ':exp', 0);
		}
		if (bCallback)
		    dnncore.setUserProp(oBtn.getAttribute('userctr'), oBtn.getAttribute('userkey'), sVal, null);
	logger.debug(`<<< Exiting __dnn_SectionMaxMin(oBtn=${oBtn},sContentID=${sContentID})`);
		return true;	//cancel postback
	}
	return false;	//failed so do postback
}

//Drag N Drop
function __dnn_enableDragDrop()
{
	logger.debug('>>> Entering __dnn_enableDragDrop()');
	var aryConts = dnn.getVar('__dnn_dragDrop').split(";");	
	logger.debug(`__dnn_enableDragDrop(): aryConts → ${aryConts}`);
	var aryTitles;

	for (var i=0; i < aryConts.length; i++)
	{
		aryTitles = aryConts[i].split(" ");
		if (aryTitles[0].length > 0)
		{			
			var oCtr = dnn.dom.getById(aryTitles[0]);
			var oTitle = dnn.dom.getById(aryTitles[1]);
			logger.debug(`__dnn_enableDragDrop(): oCtr → ${oCtr}`);
			logger.debug(`__dnn_enableDragDrop(): oTitle → ${oTitle}`);
			if (oCtr != null && oTitle != null)
			{
				oCtr.setAttribute('moduleid', aryTitles[2]);
				dnn.dom.positioning.enableDragAndDrop(oCtr, oTitle, '__dnn_dragComplete()', '__dnn_dragOver()');
			}	
		}
	}
	logger.debug('<<< Exiting __dnn_enableDragDrop()');
}

var __dnn_oPrevSelPane;
var __dnn_oPrevSelModule;
var __dnn_dragEventCount=0;
function __dnn_dragOver()
{
	logger.debug('>>> Entering __dnn_dragOver()');
	__dnn_dragEventCount++;
	if (__dnn_dragEventCount % 75 != 0)	//only calculate position every 75 events
		return;
	
	var oCont = dnn.dom.getById(dnn.dom.positioning.dragCtr.contID);
	logger.debug(`__dnn_dragOver(): oCont → ${oCont}`);

	var oPane = __dnn_getMostSelectedPane(dnn.dom.positioning.dragCtr);
		logger.debug(`__dnn_dragOver(): oPane → ${oPane}`);
		
	if (__dnn_oPrevSelPane != null)	//reset previous pane's border
		__dnn_oPrevSelPane.pane.style.border = __dnn_oPrevSelPane.origBorder;

	if (oPane != null)
	{		
		__dnn_oPrevSelPane = oPane;
		oPane.pane.style.border = '4px double ' + DNN_HIGHLIGHT_COLOR;
		var iIndex = __dnn_getPaneControlIndex(oCont, oPane);
		logger.debug(`__dnn_dragOver(): iIndex → ${iIndex}`);

		var oPrevCtl;
		var oNextCtl;
		for (var i=0; i<oPane.controls.length; i++)
		{
			if (iIndex > i && oPane.controls[i].id != oCont.id)
				oPrevCtl = oPane.controls[i];
			if (iIndex <= i && oPane.controls[i].id != oCont.id)
			{
				oNextCtl = oPane.controls[i];
				logger.debug(`__dnn_dragOver(): oNextCtl → ${oNextCtl}`);
				break;
			}
		}			
		
		if (__dnn_oPrevSelModule != null)
			dnn.dom.getNonTextNode(__dnn_oPrevSelModule.control).style.border = __dnn_oPrevSelModule.origBorder;
			

		if (oNextCtl != null)
		{
			__dnn_oPrevSelModule = oNextCtl;
			dnn.dom.getNonTextNode(oNextCtl.control).style.borderTop = '5px groove ' + DNN_HIGHLIGHT_COLOR;
		}
		else if (oPrevCtl != null)
		{
			__dnn_oPrevSelModule = oPrevCtl;
			dnn.dom.getNonTextNode(oPrevCtl.control).style.borderBottom = '5px groove ' + DNN_HIGHLIGHT_COLOR;
		}
	}
	logger.debug('<<< Exiting __dnn_dragOver()');
}

function __dnn_dragComplete()
{
	logger.debug('>>> Entering __dnn_dragComplete()');
	var oCtl = dnn.dom.getById(dnn.dom.positioning.dragCtr.contID);
	logger.debug(`__dnn_dragComplete(): oCtl → ${oCtl}`);
	var sModuleID = oCtl.getAttribute('moduleid');
	logger.debug(`__dnn_dragComplete(): sModuleID → ${sModuleID}`);
	
	if (__dnn_oPrevSelPane != null)
		__dnn_oPrevSelPane.pane.style.border = __dnn_oPrevSelPane.origBorder;

	if (__dnn_oPrevSelModule != null)
		dnn.dom.getNonTextNode(__dnn_oPrevSelModule.control).style.border = __dnn_oPrevSelModule.origBorder;
		
	var oPane = __dnn_getMostSelectedPane(dnn.dom.positioning.dragCtr);
	var iIndex;
	if (oPane == null)
	{
		var oPanes = __dnn_Panes();
		logger.debug(`__dnn_dragComplete(): oPanes → ${oPanes}`);
		for (var i=0; i<oPanes.length; i++)
		{
			if (oPanes[i].id == oCtl.parentNode.id)
				oPane = oPanes[i];
		}
	}	
	if (oPane != null)
	{
		iIndex = __dnn_getPaneControlIndex(oCtl, oPane);
		__dnn_MoveToPane(oPane, oCtl, iIndex);
		logger.debug(`__dnn_dragComplete(): iIndex → ${iIndex}`);

		dnn.callPostBack('MoveToPane', 'moduleid=' + sModuleID, 'pane=' + oPane.paneName, 'order=' + iIndex * 2); 
	}
	logger.debug('<<< Exiting __dnn_dragComplete()');
}

function __dnn_MoveToPane(oPane, oCtl, iIndex)
{
logger.debug(`>>> Entering __dnn_MoveToPane(oPane=${oPane},oCtl=${oCtl},iIndex=${iIndex})`);

	if (oPane != null)
	{
		var aryCtls = new Array();
		logger.debug(`__dnn_MoveToPane(oPane=${oPane},oCtl=${oCtl},iIndex=${iIndex}): aryCtls → ${aryCtls}`);
		for (var i=iIndex; i<oPane.controls.length; i++)
		{
			if (oPane.controls[i].control.id != oCtl.id)
				aryCtls[aryCtls.length] = oPane.controls[i].control;

			dnn.dom.removeChild(oPane.controls[i].control);
		}
		dnn.dom.appendChild(oPane.pane, oCtl);
		oCtl.style.top=0;
		oCtl.style.left=0;
		oCtl.style.position = 'relative';
		for (var i=0; i<aryCtls.length; i++)
		{
			dnn.dom.appendChild(oPane.pane, aryCtls[i]);
		}
		__dnn_RefreshPanes();
	}
	else
	{
		oCtl.style.top=0;
		oCtl.style.left=0;
		oCtl.style.position = 'relative';
	}
	logger.debug(`<<< Exiting __dnn_MoveToPane(oPane=${oPane},oCtl=${oCtl},iIndex=${iIndex})`);
}

function __dnn_RefreshPanes()
{
	logger.debug('>>> Entering __dnn_RefreshPanes()');
	var aryPanes = dnn.getVar('__dnn_Panes').split(';');
	logger.debug(`__dnn_RefreshPanes(): aryPanes → ${aryPanes}`);
	var aryPaneNames = dnn.getVar('__dnn_PaneNames').split(';');
	logger.debug(`__dnn_RefreshPanes(): aryPaneNames → ${aryPaneNames}`);
	__dnn_m_aryPanes = new Array();
	for (var i=0; i<aryPanes.length; i++)
	{
		if (aryPanes[i].length > 0)
			__dnn_m_aryPanes[__dnn_m_aryPanes.length] = new __dnn_Pane(dnn.dom.getById(aryPanes[i]), aryPaneNames[i]);
	}
	logger.debug('<<< Exiting __dnn_RefreshPanes()');
}

var __dnn_m_aryPanes;
var __dnn_m_aryModules;
function __dnn_Panes()
{
	logger.debug('>>> Entering __dnn_Panes()');
	if (__dnn_m_aryPanes == null)
	{
		__dnn_m_aryPanes = new Array();
		__dnn_RefreshPanes();
	}
	logger.debug('<<< Exiting __dnn_Panes()');
	return __dnn_m_aryPanes;
}

function __dnn_Modules(sModuleID)
{
	logger.debug(`>>> Entering __dnn_Modules(sModuleID=${sModuleID})`);
	if (__dnn_m_aryModules == null)
		__dnn_RefreshPanes();
	
	logger.debug(`<<< Exiting __dnn_Modules(sModuleID=${sModuleID})`);
	return __dnn_m_aryModules[sModuleID];
}

function __dnn_getMostSelectedPane(oContent)
{
	logger.debug(`>>> Entering __dnn_getMostSelectedPane(oContent=${oContent})`);
	var oCDims = new dnn.dom.positioning.dims(oContent);
	logger.debug(`__dnn_getMostSelectedPane(oContent=${oContent}): oCDims → ${oCDims}`);
	var iTopScore=0;
	var iScore;
	var oTopPane;
	for (var i=0; i<__dnn_Panes().length; i++)
	{
		var oPane = __dnn_Panes()[i];
		logger.debug(`__dnn_getMostSelectedPane(oContent=${oContent}): oPane → ${oPane}`);
		var oPDims = new dnn.dom.positioning.dims(oPane.pane);
		logger.debug(`__dnn_getMostSelectedPane(oContent=${oContent}): oPDims → ${oPDims}`);
		iScore = dnn.dom.positioning.elementOverlapScore(oPDims, oCDims);
		
		logger.debug(`__dnn_getMostSelectedPane(oContent=${oContent}): iScore → ${iScore}`);
		if (iScore > iTopScore)
		{
			iTopScore = iScore;
			oTopPane = oPane;
		}
	}
	logger.debug(`__dnn_getMostSelectedPane(oContent=${oContent}): oTopPane → ${oTopPane}`);
	logger.debug(`<<< Exiting __dnn_getMostSelectedPane(oContent=${oContent})`);
	return oTopPane;
}

function __dnn_getPaneControlIndex(oContent, oPane)
{
	logger.debug(`>>> Entering __dnn_getPaneControlIndex(oContent=${oContent},oPane=${oPane})`);
	if (oPane == null)
		return;
	var oCDims = new dnn.dom.positioning.dims(oContent);
	logger.debug(`__dnn_getPaneControlIndex(oContent=${oContent},oPane=${oPane}): oCDims → ${oCDims}`);
	var oCtl;
	if (oPane.controls.length == 0)
		return 0;
	for (var i=0; i<oPane.controls.length; i++)
	{
		oCtl = oPane.controls[i];
		var oIDims = new dnn.dom.positioning.dims(oCtl.control);
		logger.debug(`__dnn_getPaneControlIndex(oContent=${oContent},oPane=${oPane}): oCtl → ${oCtl}`);
		logger.debug(`__dnn_getPaneControlIndex(oContent=${oContent},oPane=${oPane}): oIDims → ${oIDims}`);
		if (oCDims.t < oIDims.t)
			return oCtl.index;
	}
	if (oCtl != null)
		return oCtl.index+1;
	else
		return 0;
}

//Objects
function __dnn_Pane(ctl, sPaneName)
{
	logger.debug(`>>> Entering __dnn_Pane(ctl=${ctl},sPaneName=${sPaneName})`);
	this.pane = ctl;
	this.id = ctl.id;
	logger.debug(`__dnn_Pane(ctl=${ctl},sPaneName=${sPaneName}): this.pane → ${this.pane}`);
	this.controls = new Array();
	this.origBorder = ctl.style.border;
	this.paneName = sPaneName;
	
	logger.debug(`__dnn_Pane(ctl=${ctl},sPaneName=${sPaneName}): this.id → ${this.id}`);
	var iIndex = 0;
	var strModuleOrder='';
	logger.debug(`__dnn_Pane(ctl=${ctl},sPaneName=${sPaneName}): strModuleOrder → ${strModuleOrder}`);
	for (var i=0; i<ctl.childNodes.length; i++)
	{
		var oNode = ctl.childNodes[i];
		logger.debug(`__dnn_Pane(ctl=${ctl},sPaneName=${sPaneName}): this.origBorder → ${this.origBorder}`);
		logger.debug(`__dnn_Pane(ctl=${ctl},sPaneName=${sPaneName}): this.paneName → ${this.paneName}`);
		if (dnn.dom.isNonTextNode(oNode))	
		{
			if (__dnn_m_aryModules == null)
				__dnn_m_aryModules = new Array();

			//if (oNode.tagName == 'A' && oNode.childNodes.length > 0)
			//	oNode = oNode.childNodes[0];	//DNN now embeds anchor tag 
				
			var sModuleID = oNode.getAttribute('moduleid');
			logger.debug(`__dnn_Pane(ctl=${ctl},sPaneName=${sPaneName}): sModuleID → ${sModuleID}`);
			if (sModuleID != null && sModuleID.length > 0)
			{
				strModuleOrder += sModuleID + '~';
				this.controls[this.controls.length] = new __dnn_PaneControl(oNode, iIndex);
				__dnn_m_aryModules[sModuleID] = oNode.id;
				iIndex+=1;
			}
		}
	}
	this.moduleOrder = strModuleOrder;
	logger.debug(`<<< Exiting __dnn_Pane(ctl=${ctl},sPaneName=${sPaneName})`);
	logger.debug(`__dnn_Pane(ctl=${ctl},sPaneName=${sPaneName}): this.moduleOrder → ${this.moduleOrder}`);

}

function __dnn_PaneControl(ctl, iIndex)
{
	logger.debug(`>>> Entering __dnn_PaneControl(ctl=${ctl},iIndex=${iIndex})`);
	this.control = ctl;
	this.id = ctl.id;
	logger.debug(`__dnn_PaneControl(ctl=${ctl},iIndex=${iIndex}): this.control → ${this.control}`);
	this.index = iIndex;
	this.origBorder = ctl.style.border;
	logger.debug(`__dnn_PaneControl(ctl=${ctl},iIndex=${iIndex}): this.origBorder → ${this.origBorder}`);
	logger.debug(`<<< Exiting __dnn_PaneControl(ctl=${ctl},iIndex=${iIndex})`);
	logger.debug(`__dnn_PaneControl(ctl=${ctl},iIndex=${iIndex}): this.id → ${this.id}`);

}

function __dnn_ShowModalPage(urlPage) {
	logger.debug(`>>> Entering __dnn_ShowModalPage(urlPage=${urlPage})`);
    dnnModal.show(urlPage, /*showReturn*/true, 550, 950, true, '');    
	logger.debug(`<<< Exiting __dnn_ShowModalPage(urlPage=${urlPage})`);
}

//move towards dnncore ns.  right now only for personalization
function __dnncore()
{
	logger.debug('>>> Entering __dnncore()');
    this.GetUserVal = 0;
    this.SetUserVal = 1;
	logger.debug(`__dnncore(): this.GetUserVal → ${this.GetUserVal}`);
	logger.debug(`__dnncore(): this.SetUserVal → ${this.SetUserVal}`);
	logger.debug('<<< Exiting __dnncore()');
}

__dnncore.prototype = {
getUserProp: function(sNameCtr, sKey, pFunc) {
	logger.debug(`>>> Entering getUserProp(sNameCtr=${sNameCtr},sKey=${sKey},pFunc=${pFunc})`);
    this._doUserCallBack(dnncore.GetUserVal, sNameCtr, sKey, null, new dnncore.UserPropArgs(sNameCtr, sKey, pFunc));
	logger.debug(`<<< Exiting getUserProp(sNameCtr=${sNameCtr},sKey=${sKey},pFunc=${pFunc})`);
},

setUserProp: function(sNameCtr, sKey, sVal, pFunc) {
	logger.debug(`>>> Entering setUserProp(sNameCtr=${sNameCtr},sKey=${sKey},sVal=${sVal},pFunc=${pFunc})`);
    this._doUserCallBack(dnncore.SetUserVal, sNameCtr, sKey, sVal, new dnncore.UserPropArgs(sNameCtr, sKey, pFunc));
	logger.debug(`<<< Exiting setUserProp(sNameCtr=${sNameCtr},sKey=${sKey},sVal=${sVal},pFunc=${pFunc})`);
},

_doUserCallBack: function(iType, sNameCtr, sKey, sVal, pFunc) {
	logger.debug(`>>> Entering _doUserCallBack(iType=${iType},sNameCtr=${sNameCtr},sKey=${sKey},sVal=${sVal},pFunc=${pFunc})`);
    if (dnn && dnn.xmlhttp)
    {
        var sPack = iType + COL_DELIMITER + sNameCtr + COL_DELIMITER + sKey + COL_DELIMITER + sVal;
        dnn.xmlhttp.doCallBack('__Page',sPack,dnncore._callBackSuccess,pFunc,dnncore._callBackFail,null,true,null,0);
		logger.debug(`<<< Exiting _doUserCallBack(iType=${iType},sNameCtr=${sNameCtr},sKey=${sKey},sVal=${sVal},pFunc=${pFunc})`);
    }
    else
        alert('Client Personalization not enabled');
},

_callBackSuccess: function (result, ctx, req) {
	logger.debug(`>>> Entering _callBackSuccess(result=${result},ctx=${ctx},req=${req})`);
    if (ctx.pFunc)
        ctx.pFunc(ctx.namingCtr, ctx.key, result);
		logger.debug(`<<< Exiting _callBackSuccess(result=${result},ctx=${ctx},req=${req})`);
},

_callBackFail: function (result, ctx) {
	logger.debug(`>>> Entering _callBackFail(result=${result},ctx=${ctx})`);
	window.status = result;
	logger.debug(`<<< Exiting _callBackFail(result=${result},ctx=${ctx})`);
}
}

__dnncore.prototype.UserPropArgs = function(sNameCtr, sKey, pFunc)
{
    this.namingCtr = sNameCtr;
	this.key = sKey;
	this.pFunc = pFunc;
}

var dnncore = new __dnncore();