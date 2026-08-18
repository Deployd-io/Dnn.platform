const logger = console;
//this script enables any element on the page to have a toolbar associated.
//the RegisterToolBar method is used on the server-side to do the association.

function __dnn_toolbarHandler(toolBarId, controlId, behaviorId, namespacePrefix, loadHandler, eventName, hideEventName)
{
	logger.debug(`>>> Entering __dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName})`);
	var status = dnn.dom.scriptStatus('dnn.controls.dnntoolbar.js');
	logger.debug(`__dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName}): status → ${status}`);
	if (status == 'complete')
	{
		var tb = new dnn.controls.DNNToolBar(controlId, behaviorId);
		logger.debug(`__dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName}): tb → ${tb}`);
		//dnn.controls.controls[toolBarId] = tb;    //WHY??
		var ctl = $get(controlId);
		logger.debug(`__dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName}): ctl → ${ctl}`);
		tb.loadDefinition(toolBarId, namespacePrefix, ctl, ctl.parentNode, ctl, loadHandler);
		dnn.dom.addSafeHandler(ctl, eventName, tb, 'show');
		dnn.dom.addSafeHandler(ctl, hideEventName, tb, 'beginHide');
		tb.show();
	}
	else if (status == '')	//not loaded
		dnn.dom.loadScript(dnn.dom.getScriptPath() + 'dnn.controls.dnntoolbar.js', '', function () {__dnn_toolbarHandler(toolBarId, controlId, behaviorId, namespacePrefix, loadHandler, eventName, hideEventName)});
		logger.debug(`<<< Exiting __dnn_toolbarHandler(toolBarId=${toolBarId},controlId=${controlId},behaviorId=${behaviorId},namespacePrefix=${namespacePrefix},loadHandler=${loadHandler},eventName=${eventName},hideEventName=${hideEventName})`);
}

