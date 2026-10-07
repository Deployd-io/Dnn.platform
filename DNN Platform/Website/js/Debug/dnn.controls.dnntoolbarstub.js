var logger = (typeof logger !== 'undefined' && logger) || console;
//this script enables any element on the page to have a toolbar associated.
//the RegisterToolBar method is used on the server-side to do the association.

function __dnn_toolbarHandler(toolBarId, controlId, behaviorId, namespacePrefix, loadHandler, eventName, hideEventName)
{
	logger.error(`>>> Entering __dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName})`, { "event.action": "__dnn_toolbarHandler", "logrite.toolBarId": toolBarId, "logrite.controlId": controlId, "logrite.behaviorId": behaviorId, "logrite.namespacePrefix": namespacePrefix, "logrite.loadHandler": loadHandler, "logrite.eventName": eventName, "logrite.hideEventName": hideEventName });
	var status = dnn.dom.scriptStatus('dnn.controls.dnntoolbar.js');
	logger.error(`__dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName}): status → ${status}`, { "event.action": "__dnn_toolbarHandler", "logrite.toolBarId": toolBarId, "logrite.controlId": controlId, "logrite.behaviorId": behaviorId, "logrite.namespacePrefix": namespacePrefix, "logrite.loadHandler": loadHandler, "logrite.eventName": eventName, "logrite.hideEventName": hideEventName });
	if (status == 'complete')
	{
		var tb = new dnn.controls.DNNToolBar(controlId, behaviorId);
		logger.error(`__dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName}): tb → ${tb}`, { "event.action": "__dnn_toolbarHandler", "logrite.toolBarId": toolBarId, "logrite.controlId": controlId, "logrite.behaviorId": behaviorId, "logrite.namespacePrefix": namespacePrefix, "logrite.loadHandler": loadHandler, "logrite.eventName": eventName, "logrite.hideEventName": hideEventName });
		//dnn.controls.controls[toolBarId] = tb;    //WHY??
		var ctl = $get(controlId);
		logger.error(`__dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName}): ctl → ${ctl}`, { "event.action": "__dnn_toolbarHandler", "logrite.toolBarId": toolBarId, "logrite.controlId": controlId, "logrite.behaviorId": behaviorId, "logrite.namespacePrefix": namespacePrefix, "logrite.loadHandler": loadHandler, "logrite.eventName": eventName, "logrite.hideEventName": hideEventName });
		tb.loadDefinition(toolBarId, namespacePrefix, ctl, ctl.parentNode, ctl, loadHandler);
		dnn.dom.addSafeHandler(ctl, eventName, tb, 'show');
		dnn.dom.addSafeHandler(ctl, hideEventName, tb, 'beginHide');
		tb.show();
	}
	else if (status == '')	//not loaded
		dnn.dom.loadScript(dnn.dom.getScriptPath() + 'dnn.controls.dnntoolbar.js', '', function () {__dnn_toolbarHandler(toolBarId, controlId, behaviorId, namespacePrefix, loadHandler, eventName, hideEventName)});
		logger.warn(`<<< Exiting __dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName})`, { "event.action": "__dnn_toolbarHandler", "logrite.toolBarId": toolBarId, "logrite.controlId": controlId, "logrite.behaviorId": behaviorId, "logrite.namespacePrefix": namespacePrefix, "logrite.loadHandler": loadHandler, "logrite.eventName": eventName, "logrite.hideEventName": hideEventName });
}

