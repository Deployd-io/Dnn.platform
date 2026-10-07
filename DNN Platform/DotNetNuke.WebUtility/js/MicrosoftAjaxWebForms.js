var logger = (typeof logger !== 'undefined' && logger) || console;
//-----------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------
// MicrosoftAjaxWebForms.js
// Microsoft AJAX ASP.NET WebForms Framework.


Type.registerNamespace('Sys.WebForms');


Sys.WebForms.BeginRequestEventArgs = function Sys$WebForms$BeginRequestEventArgs(request, postBackElement) {
	logger.error(`>>> Entering BeginRequestEventArgs(request=${request},postBackElement=${postBackElement})`, { "event.action": "BeginRequestEventArgs", "logrite.request": request, "logrite.postBackElement": postBackElement });
    /// <param name="request" type="Sys.Net.WebRequest"></param>
    /// <param name="postBackElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "request", type: Sys.Net.WebRequest},
        {name: "postBackElement", domElement: true}
    ]);
    if (e) throw e;


    Sys.WebForms.BeginRequestEventArgs.initializeBase(this);
    this._request = request;
    this._postBackElement = postBackElement;
	logger.debug(`BeginRequestEventArgs(request=${request},postBackElement=${postBackElement}): this._postBackElement → ${this._postBackElement}`, { "event.action": "BeginRequestEventArgs", "logrite.request": request, "logrite.postBackElement": postBackElement });
}


    function Sys$WebForms$BeginRequestEventArgs$get_postBackElement() {
		logger.error('>>> Entering get_postBackElement()', { "event.action": "get_postBackElement" });
        /// <value domElement="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_postBackElement()', { "event.action": "get_postBackElement" });
        return this._postBackElement;
    }

    function Sys$WebForms$BeginRequestEventArgs$get_request() {
		logger.error('>>> Entering get_request()', { "event.action": "get_request" });
        /// <value type="Sys.Net.WebRequest"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_request()', { "event.action": "get_request" });
        return this._request;
    }
Sys.WebForms.BeginRequestEventArgs.prototype = {
    get_postBackElement: Sys$WebForms$BeginRequestEventArgs$get_postBackElement,

    get_request: Sys$WebForms$BeginRequestEventArgs$get_request
}

Sys.WebForms.BeginRequestEventArgs.registerClass('Sys.WebForms.BeginRequestEventArgs', Sys.EventArgs);

Sys.WebForms.EndRequestEventArgs = function Sys$WebForms$EndRequestEventArgs(error, dataItems, response) {
	logger.error(`>>> Entering EndRequestEventArgs(error=${error},dataItems=${dataItems},response=${response})`, { "event.action": "EndRequestEventArgs", "logrite.error": error, "logrite.dataItems": dataItems, "logrite.response": response });
    /// <param name="error" type="Error" mayBeNull="true"></param>
    /// <param name="dataItems" type="Object" mayBeNull="true"></param>
    /// <param name="response" type="Sys.Net.WebRequestExecutor"></param>
    var e = Function._validateParams(arguments, [
        {name: "error", type: Error, mayBeNull: true},
        {name: "dataItems", type: Object, mayBeNull: true},
        {name: "response", type: Sys.Net.WebRequestExecutor}
    ]);
    if (e) throw e;


    Sys.WebForms.EndRequestEventArgs.initializeBase(this);
    this._errorHandled = false;
    this._error = error;
		logger.debug(`EndRequestEventArgs(error=${error},dataItems=${dataItems},response=${response}): this._errorHandled → ${this._errorHandled}`, { "event.action": "EndRequestEventArgs", "logrite.error": error, "logrite.dataItems": dataItems, "logrite.response": response });
        this._dataItems = dataItems || new Object();
    this._response = response;
	logger.debug(`EndRequestEventArgs(error=${error},dataItems=${dataItems},response=${response}): this._response → ${this._response}`, { "event.action": "EndRequestEventArgs", "logrite.error": error, "logrite.dataItems": dataItems, "logrite.response": response });
}


    function Sys$WebForms$EndRequestEventArgs$get_dataItems() {
		logger.error('>>> Entering get_dataItems()', { "event.action": "get_dataItems" });
        /// <value type="Object"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_dataItems()', { "event.action": "get_dataItems" });
        return this._dataItems;
    }

    function Sys$WebForms$EndRequestEventArgs$get_error() {
		logger.error('>>> Entering get_error()', { "event.action": "get_error" });
        /// <value type="Error"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_error()', { "event.action": "get_error" });
        return this._error;
    }

    function Sys$WebForms$EndRequestEventArgs$get_errorHandled() {
		logger.error('>>> Entering get_errorHandled()', { "event.action": "get_errorHandled" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_errorHandled()', { "event.action": "get_errorHandled" });
        return this._errorHandled;
    }
    function Sys$WebForms$EndRequestEventArgs$set_errorHandled(value) {
		logger.error(`>>> Entering set_errorHandled(value=${value})`, { "event.action": "set_errorHandled", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;
		logger.error(`set_errorHandled(value=${value}): e → ${e}`, { "event.action": "set_errorHandled", "logrite.value": value });

        this._errorHandled = value;
		logger.debug(`set_errorHandled(value=${value}): this._errorHandled → ${this._errorHandled}`, { "event.action": "set_errorHandled", "logrite.value": value });
    }

    function Sys$WebForms$EndRequestEventArgs$get_response() {
		logger.error('>>> Entering get_response()', { "event.action": "get_response" });
        /// <value type="Sys.Net.WebRequestExecutor"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_response()', { "event.action": "get_response" });
        return this._response;
    }
Sys.WebForms.EndRequestEventArgs.prototype = {
    get_dataItems: Sys$WebForms$EndRequestEventArgs$get_dataItems,

    get_error: Sys$WebForms$EndRequestEventArgs$get_error,

    get_errorHandled: Sys$WebForms$EndRequestEventArgs$get_errorHandled,
    set_errorHandled: Sys$WebForms$EndRequestEventArgs$set_errorHandled,

    get_response: Sys$WebForms$EndRequestEventArgs$get_response
}

Sys.WebForms.EndRequestEventArgs.registerClass('Sys.WebForms.EndRequestEventArgs', Sys.EventArgs);

Sys.WebForms.InitializeRequestEventArgs = function Sys$WebForms$InitializeRequestEventArgs(request, postBackElement) {
	logger.error(`>>> Entering InitializeRequestEventArgs(request=${request},postBackElement=${postBackElement})`, { "event.action": "InitializeRequestEventArgs", "logrite.request": request, "logrite.postBackElement": postBackElement });
    /// <param name="request" type="Sys.Net.WebRequest"></param>
    /// <param name="postBackElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "request", type: Sys.Net.WebRequest},
        {name: "postBackElement", domElement: true}
    ]);
    if (e) throw e;


    Sys.WebForms.InitializeRequestEventArgs.initializeBase(this);
    this._request = request;
    this._postBackElement = postBackElement;
	logger.debug(`InitializeRequestEventArgs(request=${request},postBackElement=${postBackElement}): this._postBackElement → ${this._postBackElement}`, { "event.action": "InitializeRequestEventArgs", "logrite.request": request, "logrite.postBackElement": postBackElement });
}


    function Sys$WebForms$InitializeRequestEventArgs$get_postBackElement() {
        /// <value domElement="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._postBackElement;
    }

    function Sys$WebForms$InitializeRequestEventArgs$get_request() {
        /// <value type="Sys.Net.WebRequest"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._request;
    }
Sys.WebForms.InitializeRequestEventArgs.prototype = {
    get_postBackElement: Sys$WebForms$InitializeRequestEventArgs$get_postBackElement,

    get_request: Sys$WebForms$InitializeRequestEventArgs$get_request
}

Sys.WebForms.InitializeRequestEventArgs.registerClass('Sys.WebForms.InitializeRequestEventArgs', Sys.CancelEventArgs);

Sys.WebForms.PageLoadedEventArgs = function Sys$WebForms$PageLoadedEventArgs(panelsUpdated, panelsCreated, dataItems) {
	logger.error(`>>> Entering PageLoadedEventArgs(panelsUpdated=${panelsUpdated},panelsCreated=${panelsCreated},dataItems=${dataItems})`, { "event.action": "PageLoadedEventArgs", "logrite.panelsUpdated": panelsUpdated, "logrite.panelsCreated": panelsCreated, "logrite.dataItems": dataItems });
    /// <param name="panelsUpdated" type="Array"></param>
    /// <param name="panelsCreated" type="Array"></param>
    /// <param name="dataItems" type="Object" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "panelsUpdated", type: Array},
        {name: "panelsCreated", type: Array},
        {name: "dataItems", type: Object, mayBeNull: true}
    ]);
    if (e) throw e;

    Sys.WebForms.PageLoadedEventArgs.initializeBase(this);

    this._panelsUpdated = panelsUpdated;
    this._panelsCreated = panelsCreated;
		logger.debug(`PageLoadedEventArgs(panelsUpdated=${panelsUpdated},panelsCreated=${panelsCreated},dataItems=${dataItems}): this._panelsUpdated → ${this._panelsUpdated}`, { "event.action": "PageLoadedEventArgs", "logrite.panelsUpdated": panelsUpdated, "logrite.panelsCreated": panelsCreated, "logrite.dataItems": dataItems });
		logger.debug(`PageLoadedEventArgs(panelsUpdated=${panelsUpdated},panelsCreated=${panelsCreated},dataItems=${dataItems}): this._panelsCreated → ${this._panelsCreated}`, { "event.action": "PageLoadedEventArgs", "logrite.panelsUpdated": panelsUpdated, "logrite.panelsCreated": panelsCreated, "logrite.dataItems": dataItems });
        this._dataItems = dataItems || new Object();
}


    function Sys$WebForms$PageLoadedEventArgs$get_dataItems() {
        /// <value type="Object"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._dataItems;
    }

    function Sys$WebForms$PageLoadedEventArgs$get_panelsCreated() {
		logger.error('>>> Entering get_panelsCreated()', { "event.action": "get_panelsCreated" });
        /// <value type="Array"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_panelsCreated()', { "event.action": "get_panelsCreated" });
        return this._panelsCreated;
    }

    function Sys$WebForms$PageLoadedEventArgs$get_panelsUpdated() {
		logger.error('>>> Entering get_panelsUpdated()', { "event.action": "get_panelsUpdated" });
        /// <value type="Array"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_panelsUpdated()', { "event.action": "get_panelsUpdated" });
        return this._panelsUpdated;
    }
Sys.WebForms.PageLoadedEventArgs.prototype = {
    get_dataItems: Sys$WebForms$PageLoadedEventArgs$get_dataItems,

    get_panelsCreated: Sys$WebForms$PageLoadedEventArgs$get_panelsCreated,

    get_panelsUpdated: Sys$WebForms$PageLoadedEventArgs$get_panelsUpdated
}

Sys.WebForms.PageLoadedEventArgs.registerClass('Sys.WebForms.PageLoadedEventArgs', Sys.EventArgs);

Sys.WebForms.PageLoadingEventArgs = function Sys$WebForms$PageLoadingEventArgs(panelsUpdating, panelsDeleting, dataItems) {
	logger.error(`>>> Entering PageLoadingEventArgs(panelsUpdating=${panelsUpdating},panelsDeleting=${panelsDeleting},dataItems=${dataItems})`, { "event.action": "PageLoadingEventArgs", "logrite.panelsUpdating": panelsUpdating, "logrite.panelsDeleting": panelsDeleting, "logrite.dataItems": dataItems });
    /// <param name="panelsUpdating" type="Array"></param>
    /// <param name="panelsDeleting" type="Array"></param>
    /// <param name="dataItems" type="Object" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "panelsUpdating", type: Array},
        {name: "panelsDeleting", type: Array},
        {name: "dataItems", type: Object, mayBeNull: true}
    ]);
    if (e) throw e;

    Sys.WebForms.PageLoadingEventArgs.initializeBase(this);

    this._panelsUpdating = panelsUpdating;
    this._panelsDeleting = panelsDeleting;
		logger.debug(`PageLoadingEventArgs(panelsUpdating=${panelsUpdating},panelsDeleting=${panelsDeleting},dataItems=${dataItems}): this._panelsUpdating → ${this._panelsUpdating}`, { "event.action": "PageLoadingEventArgs", "logrite.panelsUpdating": panelsUpdating, "logrite.panelsDeleting": panelsDeleting, "logrite.dataItems": dataItems });
		logger.debug(`PageLoadingEventArgs(panelsUpdating=${panelsUpdating},panelsDeleting=${panelsDeleting},dataItems=${dataItems}): this._panelsDeleting → ${this._panelsDeleting}`, { "event.action": "PageLoadingEventArgs", "logrite.panelsUpdating": panelsUpdating, "logrite.panelsDeleting": panelsDeleting, "logrite.dataItems": dataItems });
        this._dataItems = dataItems || new Object();
}


    function Sys$WebForms$PageLoadingEventArgs$get_dataItems() {
        /// <value type="Object"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._dataItems;
    }

    function Sys$WebForms$PageLoadingEventArgs$get_panelsDeleting() {
		logger.error('>>> Entering get_panelsDeleting()', { "event.action": "get_panelsDeleting" });
        /// <value type="Array"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_panelsDeleting()', { "event.action": "get_panelsDeleting" });
        return this._panelsDeleting;
    }

    function Sys$WebForms$PageLoadingEventArgs$get_panelsUpdating() {
		logger.error('>>> Entering get_panelsUpdating()', { "event.action": "get_panelsUpdating" });
        /// <value type="Array"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_panelsUpdating()', { "event.action": "get_panelsUpdating" });
        return this._panelsUpdating;
    }
Sys.WebForms.PageLoadingEventArgs.prototype = {
    get_dataItems: Sys$WebForms$PageLoadingEventArgs$get_dataItems,

    get_panelsDeleting: Sys$WebForms$PageLoadingEventArgs$get_panelsDeleting,

    get_panelsUpdating: Sys$WebForms$PageLoadingEventArgs$get_panelsUpdating
}

Sys.WebForms.PageLoadingEventArgs.registerClass('Sys.WebForms.PageLoadingEventArgs', Sys.EventArgs);
Sys.WebForms.PageRequestManager = function Sys$WebForms$PageRequestManager() {
	logger.error('>>> Entering PageRequestManager()', { "event.action": "PageRequestManager" });
    this._form = null;
    this._updatePanelIDs = null;
    this._updatePanelClientIDs = null;
	logger.debug(`PageRequestManager(): this._form → ${this._form}`, { "event.action": "PageRequestManager" });
    this._oldUpdatePanelIDs = null;
    this._childUpdatePanelIDs = null;
	logger.debug(`PageRequestManager(): this._updatePanelIDs → ${this._updatePanelIDs}`, { "event.action": "PageRequestManager" });
    this._panelsToRefreshIDs = null;
    this._updatePanelHasChildrenAsTriggers = null;
	logger.debug(`PageRequestManager(): this._updatePanelClientIDs → ${this._updatePanelClientIDs}`, { "event.action": "PageRequestManager" });
    this._asyncPostBackControlIDs = null;
    this._asyncPostBackControlClientIDs = null;
	logger.debug(`PageRequestManager(): this._oldUpdatePanelIDs → ${this._oldUpdatePanelIDs}`, { "event.action": "PageRequestManager" });
    this._postBackControlIDs = null;
    this._postBackControlClientIDs = null;
	logger.debug(`PageRequestManager(): this._childUpdatePanelIDs → ${this._childUpdatePanelIDs}`, { "event.action": "PageRequestManager" });
    this._scriptManagerID = null;
    this._pageLoadedHandler = null;
	logger.debug(`PageRequestManager(): this._panelsToRefreshIDs → ${this._panelsToRefreshIDs}`, { "event.action": "PageRequestManager" });

    this._additionalInput = null;
	logger.debug(`PageRequestManager(): this._updatePanelHasChildrenAsTriggers → ${this._updatePanelHasChildrenAsTriggers}`, { "event.action": "PageRequestManager" });
    this._onsubmit = null;
    this._onSubmitStatements = [];
	logger.debug(`PageRequestManager(): this._asyncPostBackControlIDs → ${this._asyncPostBackControlIDs}`, { "event.action": "PageRequestManager" });
    this._originalDoPostBack = null;
    this._postBackSettings = null;
	logger.debug(`PageRequestManager(): this._asyncPostBackControlClientIDs → ${this._asyncPostBackControlClientIDs}`, { "event.action": "PageRequestManager" });
    this._request = null;
    this._onFormSubmitHandler = null;
	logger.debug(`PageRequestManager(): this._postBackControlIDs → ${this._postBackControlIDs}`, { "event.action": "PageRequestManager" });
    this._onFormElementClickHandler = null;
    this._onWindowUnloadHandler = null;
	logger.debug(`PageRequestManager(): this._postBackControlClientIDs → ${this._postBackControlClientIDs}`, { "event.action": "PageRequestManager" });
    this._asyncPostBackTimeout = null;

	logger.debug(`PageRequestManager(): this._scriptManagerID → ${this._scriptManagerID}`, { "event.action": "PageRequestManager" });
    this._controlIDToFocus = null;
    this._scrollPosition = null;
	logger.debug(`PageRequestManager(): this._pageLoadedHandler → ${this._pageLoadedHandler}`, { "event.action": "PageRequestManager" });
    this._dataItems = null;
    this._response = null;
	logger.debug(`PageRequestManager(): this._additionalInput → ${this._additionalInput}`, { "event.action": "PageRequestManager" });
    this._processingRequest = false;
    this._scriptDisposes = {};
	logger.debug(`PageRequestManager(): this._onsubmit → ${this._onsubmit}`, { "event.action": "PageRequestManager" });
}



    function Sys$WebForms$PageRequestManager$_get_eventHandlerList() {
		logger.error('>>> Entering _get_eventHandlerList()', { "event.action": "_get_eventHandlerList" });
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
		logger.warn('<<< Exiting _get_eventHandlerList()', { "event.action": "_get_eventHandlerList" });
        return this._events;
    }

    function Sys$WebForms$PageRequestManager$get_isInAsyncPostBack() {
		logger.error('>>> Entering get_isInAsyncPostBack()', { "event.action": "get_isInAsyncPostBack" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_isInAsyncPostBack()', { "event.action": "get_isInAsyncPostBack" });
        return this._request !== null;
    }


    function Sys$WebForms$PageRequestManager$add_beginRequest(handler) {
		logger.error(`>>> Entering add_beginRequest(handler=${handler})`, { "event.action": "add_beginRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_beginRequest(handler=${handler}): e → ${e}`, { "event.action": "add_beginRequest", "logrite.handler": handler });

        this._get_eventHandlerList().addHandler("beginRequest", handler);
		logger.warn(`<<< Exiting add_beginRequest(handler=${handler})`, { "event.action": "add_beginRequest", "logrite.handler": handler });
    }
    function Sys$WebForms$PageRequestManager$remove_beginRequest(handler) {
		logger.error(`>>> Entering remove_beginRequest(handler=${handler})`, { "event.action": "remove_beginRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_beginRequest(handler=${handler}): e → ${e}`, { "event.action": "remove_beginRequest", "logrite.handler": handler });

        this._get_eventHandlerList().removeHandler("beginRequest", handler);
		logger.warn(`<<< Exiting remove_beginRequest(handler=${handler})`, { "event.action": "remove_beginRequest", "logrite.handler": handler });
    }

    function Sys$WebForms$PageRequestManager$add_endRequest(handler) {
		logger.error(`>>> Entering add_endRequest(handler=${handler})`, { "event.action": "add_endRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_endRequest(handler=${handler}): e → ${e}`, { "event.action": "add_endRequest", "logrite.handler": handler });

        this._get_eventHandlerList().addHandler("endRequest", handler);
		logger.warn(`<<< Exiting add_endRequest(handler=${handler})`, { "event.action": "add_endRequest", "logrite.handler": handler });
    }
    function Sys$WebForms$PageRequestManager$remove_endRequest(handler) {
		logger.error(`>>> Entering remove_endRequest(handler=${handler})`, { "event.action": "remove_endRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_endRequest(handler=${handler}): e → ${e}`, { "event.action": "remove_endRequest", "logrite.handler": handler });

        this._get_eventHandlerList().removeHandler("endRequest", handler);
		logger.warn(`<<< Exiting remove_endRequest(handler=${handler})`, { "event.action": "remove_endRequest", "logrite.handler": handler });
    }

    function Sys$WebForms$PageRequestManager$add_initializeRequest(handler) {
		logger.error(`>>> Entering add_initializeRequest(handler=${handler})`, { "event.action": "add_initializeRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_initializeRequest(handler=${handler}): e → ${e}`, { "event.action": "add_initializeRequest", "logrite.handler": handler });

        this._get_eventHandlerList().addHandler("initializeRequest", handler);
		logger.warn(`<<< Exiting add_initializeRequest(handler=${handler})`, { "event.action": "add_initializeRequest", "logrite.handler": handler });
    }
    function Sys$WebForms$PageRequestManager$remove_initializeRequest(handler) {
		logger.error(`>>> Entering remove_initializeRequest(handler=${handler})`, { "event.action": "remove_initializeRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_initializeRequest(handler=${handler}): e → ${e}`, { "event.action": "remove_initializeRequest", "logrite.handler": handler });

        this._get_eventHandlerList().removeHandler("initializeRequest", handler);
		logger.warn(`<<< Exiting remove_initializeRequest(handler=${handler})`, { "event.action": "remove_initializeRequest", "logrite.handler": handler });
    }

    function Sys$WebForms$PageRequestManager$add_pageLoaded(handler) {
		logger.error(`>>> Entering add_pageLoaded(handler=${handler})`, { "event.action": "add_pageLoaded", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_pageLoaded(handler=${handler}): e → ${e}`, { "event.action": "add_pageLoaded", "logrite.handler": handler });

        this._get_eventHandlerList().addHandler("pageLoaded", handler);
		logger.warn(`<<< Exiting add_pageLoaded(handler=${handler})`, { "event.action": "add_pageLoaded", "logrite.handler": handler });
    }
    function Sys$WebForms$PageRequestManager$remove_pageLoaded(handler) {
		logger.error(`>>> Entering remove_pageLoaded(handler=${handler})`, { "event.action": "remove_pageLoaded", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_pageLoaded(handler=${handler}): e → ${e}`, { "event.action": "remove_pageLoaded", "logrite.handler": handler });

        this._get_eventHandlerList().removeHandler("pageLoaded", handler);
		logger.warn(`<<< Exiting remove_pageLoaded(handler=${handler})`, { "event.action": "remove_pageLoaded", "logrite.handler": handler });
    }

    function Sys$WebForms$PageRequestManager$add_pageLoading(handler) {
		logger.error(`>>> Entering add_pageLoading(handler=${handler})`, { "event.action": "add_pageLoading", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_pageLoading(handler=${handler}): e → ${e}`, { "event.action": "add_pageLoading", "logrite.handler": handler });

        this._get_eventHandlerList().addHandler("pageLoading", handler);
		logger.warn(`<<< Exiting add_pageLoading(handler=${handler})`, { "event.action": "add_pageLoading", "logrite.handler": handler });
    }
    function Sys$WebForms$PageRequestManager$remove_pageLoading(handler) {
		logger.error(`>>> Entering remove_pageLoading(handler=${handler})`, { "event.action": "remove_pageLoading", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_pageLoading(handler=${handler}): e → ${e}`, { "event.action": "remove_pageLoading", "logrite.handler": handler });

        this._get_eventHandlerList().removeHandler("pageLoading", handler);
		logger.warn(`<<< Exiting remove_pageLoading(handler=${handler})`, { "event.action": "remove_pageLoading", "logrite.handler": handler });
    }

    function Sys$WebForms$PageRequestManager$abortPostBack() {
		logger.error('>>> Entering abortPostBack()', { "event.action": "abortPostBack" });
        if (!this._processingRequest && this._request) {
            this._request.get_executor().abort();
            this._request = null;
			logger.warn('<<< Exiting abortPostBack()', { "event.action": "abortPostBack" });
			logger.debug(`abortPostBack(): this._request → ${this._request}`, { "event.action": "abortPostBack" });
        }
    }

    function Sys$WebForms$PageRequestManager$_createPageRequestManagerTimeoutError() {
				logger.error('>>> Entering _createPageRequestManagerTimeoutError()', { "event.action": "_createPageRequestManagerTimeoutError" });
				logger.warn('<<< Exiting _createPageRequestManagerTimeoutError()', { "event.action": "_createPageRequestManagerTimeoutError" });
                var displayMessage = "Sys.WebForms.PageRequestManagerTimeoutException: " + Sys.WebForms.Res.PRM_TimeoutError;
        var e = Error.create(displayMessage, {name: 'Sys.WebForms.PageRequestManagerTimeoutException'});
		logger.error(`_createPageRequestManagerTimeoutError(): displayMessage → ${displayMessage}`, { "event.action": "_createPageRequestManagerTimeoutError" });
		logger.error(`_createPageRequestManagerTimeoutError(): e → ${e}`, { "event.action": "_createPageRequestManagerTimeoutError" });
        e.popStackFrame();
        return e;
    }

    function Sys$WebForms$PageRequestManager$_createPageRequestManagerServerError(httpStatusCode, message) {
				logger.error(`>>> Entering _createPageRequestManagerServerError(httpStatusCode=${httpStatusCode},message=${message})`, { "event.action": "_createPageRequestManagerServerError", "logrite.httpStatusCode": httpStatusCode, "logrite.message": message });
				logger.warn(`<<< Exiting _createPageRequestManagerServerError(httpStatusCode=${httpStatusCode},message=${message})`, { "event.action": "_createPageRequestManagerServerError", "logrite.httpStatusCode": httpStatusCode, "logrite.message": message });
                var displayMessage = message || ("Sys.WebForms.PageRequestManagerServerErrorException: " + String.format(Sys.WebForms.Res.PRM_ServerError, httpStatusCode));
        var e = Error.create(displayMessage, {
            name: 'Sys.WebForms.PageRequestManagerServerErrorException',
            httpStatusCode: httpStatusCode
        });
        e.popStackFrame();
		logger.error(`_createPageRequestManagerServerError(httpStatusCode=${httpStatusCode},message=${message}): displayMessage → ${displayMessage}`, { "event.action": "_createPageRequestManagerServerError", "logrite.httpStatusCode": httpStatusCode, "logrite.message": message });
        return e;
    }

    function Sys$WebForms$PageRequestManager$_createPageRequestManagerParserError(parserErrorMessage) {
				logger.error(`>>> Entering _createPageRequestManagerParserError(parserErrorMessage=${parserErrorMessage})`, { "event.action": "_createPageRequestManagerParserError", "logrite.parserErrorMessage": parserErrorMessage });
				logger.warn(`<<< Exiting _createPageRequestManagerParserError(parserErrorMessage=${parserErrorMessage})`, { "event.action": "_createPageRequestManagerParserError", "logrite.parserErrorMessage": parserErrorMessage });
                var displayMessage = "Sys.WebForms.PageRequestManagerParserErrorException: " + String.format(Sys.WebForms.Res.PRM_ParserError, parserErrorMessage);
        var e = Error.create(displayMessage, {name: 'Sys.WebForms.PageRequestManagerParserErrorException'});
		logger.error(`_createPageRequestManagerParserError(parserErrorMessage=${parserErrorMessage}): displayMessage → ${displayMessage}`, { "event.action": "_createPageRequestManagerParserError", "logrite.parserErrorMessage": parserErrorMessage });
		logger.error(`_createPageRequestManagerParserError(parserErrorMessage=${parserErrorMessage}): e → ${e}`, { "event.action": "_createPageRequestManagerParserError", "logrite.parserErrorMessage": parserErrorMessage });
        e.popStackFrame();
        return e;
    }

    function Sys$WebForms$PageRequestManager$_createPostBackSettings(async, panelID, sourceElement) {
	logger.error(`>>> Entering _createPostBackSettings(async=${async},panelID=${panelID},sourceElement=${sourceElement})`, { "event.action": "_createPostBackSettings", "logrite.async": async, "logrite.panelID": panelID, "logrite.sourceElement": sourceElement });

		logger.warn(`<<< Exiting _createPostBackSettings(async=${async},panelID=${panelID},sourceElement=${sourceElement})`, { "event.action": "_createPostBackSettings", "logrite.async": async, "logrite.panelID": panelID, "logrite.sourceElement": sourceElement });
        return { async:async, panelID:panelID, sourceElement:sourceElement };
    }

    function Sys$WebForms$PageRequestManager$_convertToClientIDs(source, destinationIDs, destinationClientIDs) {
		logger.error(`>>> Entering _convertToClientIDs(source=${source},destinationIDs=${destinationIDs},destinationClientIDs=${destinationClientIDs})`, { "event.action": "_convertToClientIDs", "logrite.source": source, "logrite.destinationIDs": destinationIDs, "logrite.destinationClientIDs": destinationClientIDs });
        if (source) {
            for (var i = 0; i < source.length; i++) {
                Array.add(destinationIDs, source[i]);
                Array.add(destinationClientIDs, this._uniqueIDToClientID(source[i]));
				logger.warn(`<<< Exiting _convertToClientIDs(source=${source},destinationIDs=${destinationIDs},destinationClientIDs=${destinationClientIDs})`, { "event.action": "_convertToClientIDs", "logrite.source": source, "logrite.destinationIDs": destinationIDs, "logrite.destinationClientIDs": destinationClientIDs });
            }
        }
    }

    function Sys$WebForms$PageRequestManager$_decodeString(encodedValue) {
		logger.error(`>>> Entering _decodeString(encodedValue=${encodedValue})`, { "event.action": "_decodeString", "logrite.encodedValue": encodedValue });
		logger.warn(`<<< Exiting _decodeString(encodedValue=${encodedValue})`, { "event.action": "_decodeString", "logrite.encodedValue": encodedValue });
        return encodedValue.replace(/\\\u00FF\\/g, "\u0000").replace(/\u00FF\u00FF/g, "\u00FF");
    }

    function Sys$WebForms$PageRequestManager$_destroyTree(element) {
				logger.error(`>>> Entering _destroyTree(element=${element})`, { "event.action": "_destroyTree", "logrite.element": element });
                if (element.nodeType === 1) {
                                    var childNodes = element.childNodes;
            for (var i = childNodes.length - 1; i >= 0; i--) {
                var node = childNodes[i];
                if (node.nodeType === 1) {
                    if (node.dispose && typeof(node.dispose) === "function") {
                        node.dispose();
                    }
                    else if (node.control && typeof(node.control.dispose) === "function") {
                        node.control.dispose();
                    }
                    var behaviors = Sys.UI.Behavior.getBehaviors(node);
                    for (var j = behaviors.length - 1; j >= 0; j--) {
                        behaviors[j].dispose();
                    }
                    this._destroyTree(node);
                }
            }
        }
    }

    function Sys$WebForms$PageRequestManager$dispose() {
		logger.error('>>> Entering dispose()', { "event.action": "dispose" });
        if (this._form) {
            Sys.UI.DomEvent.removeHandler(this._form, 'submit', this._onFormSubmitHandler);
            Sys.UI.DomEvent.removeHandler(this._form, 'click', this._onFormElementClickHandler);
            Sys.UI.DomEvent.removeHandler(window, 'unload', this._onWindowUnloadHandler);
            Sys.UI.DomEvent.removeHandler(window, 'load', this._pageLoadedHandler);
        }

        if (this._originalDoPostBack) {
            window.__doPostBack = this._originalDoPostBack;
            this._originalDoPostBack = null;
        }

		logger.debug(`dispose(): this._originalDoPostBack → ${this._originalDoPostBack}`, { "event.action": "dispose" });
        this._form = null;
        this._updatePanelIDs = null;
		logger.debug(`dispose(): this._form → ${this._form}`, { "event.action": "dispose" });
        this._oldUpdatePanelIDs = null;
        this._childUpdatePanelIDs = null;
		logger.debug(`dispose(): this._updatePanelIDs → ${this._updatePanelIDs}`, { "event.action": "dispose" });
        this._updatePanelClientIDs = null;
        this._asyncPostBackControlIDs = null;
		logger.debug(`dispose(): this._oldUpdatePanelIDs → ${this._oldUpdatePanelIDs}`, { "event.action": "dispose" });
        this._asyncPostBackControlClientIDs = null;
        this._postBackControlIDs = null;
		logger.debug(`dispose(): this._childUpdatePanelIDs → ${this._childUpdatePanelIDs}`, { "event.action": "dispose" });
        this._postBackControlClientIDs = null;
        this._asyncPostBackTimeout = null;
		logger.debug(`dispose(): this._updatePanelClientIDs → ${this._updatePanelClientIDs}`, { "event.action": "dispose" });
        this._scrollPosition = null;
        this._dataItems = null;
		logger.debug(`dispose(): this._dataItems → ${this._dataItems}`, { "event.action": "dispose" });
    }


    function Sys$WebForms$PageRequestManager$_doPostBack(eventTarget, eventArgument) {
		logger.error(`>>> Entering _doPostBack(eventTarget=${eventTarget},eventArgument=${eventArgument})`, { "event.action": "_doPostBack", "logrite.eventTarget": eventTarget, "logrite.eventArgument": eventArgument });
        this._additionalInput = null;

        var form = this._form;
        if (form.action !== form._initialAction) {
									logger.debug(`_doPostBack(eventTarget=${eventTarget},eventArgument=${eventArgument}): form.onsubmit → ${form.onsubmit}`, { "event.action": "_doPostBack", "logrite.eventTarget": eventTarget, "logrite.eventArgument": eventArgument });
                                    this._postBackSettings = this._createPostBackSettings(false, null, null);
        }
        else {
                        var clientID = this._uniqueIDToClientID(eventTarget);
						logger.error(`_doPostBack(eventTarget=${eventTarget},eventArgument=${eventArgument}): clientID → ${clientID}`, { "event.action": "_doPostBack", "logrite.eventTarget": eventTarget, "logrite.eventArgument": eventArgument });
            var postBackElement = document.getElementById(clientID);
			logger.error(`_doPostBack(eventTarget=${eventTarget},eventArgument=${eventArgument}): postBackElement → ${postBackElement}`, { "event.action": "_doPostBack", "logrite.eventTarget": eventTarget, "logrite.eventArgument": eventArgument });
            if (!postBackElement) {
                                                                                if (Array.contains(this._asyncPostBackControlIDs, eventTarget)) {
                                        this._postBackSettings = this._createPostBackSettings(true, this._scriptManagerID + '|' + eventTarget, null);
                }
                else {
					logger.debug(`_doPostBack(eventTarget=${eventTarget},eventArgument=${eventArgument}): this._additionalInput → ${this._additionalInput}`, { "event.action": "_doPostBack", "logrite.eventTarget": eventTarget, "logrite.eventArgument": eventArgument });
                    if (Array.contains(this._postBackControlIDs, eventTarget)) {
                                                this._postBackSettings = this._createPostBackSettings(false, null, null);
                    }
                    else {
                                                                                                var nearestUniqueIDMatch = this._findNearestElement(eventTarget);
																								logger.error(`_doPostBack(eventTarget=${eventTarget},eventArgument=${eventArgument}): nearestUniqueIDMatch → ${nearestUniqueIDMatch}`, { "event.action": "_doPostBack", "logrite.eventTarget": eventTarget, "logrite.eventArgument": eventArgument });
                        if (nearestUniqueIDMatch) {
                                                                                    this._postBackSettings = this._getPostBackSettings(nearestUniqueIDMatch, eventTarget);
                        }
                        else {
                                                                                    this._postBackSettings = this._createPostBackSettings(false, null, null);
                        }
                    }
                }
            }
            else {
                                                this._postBackSettings = this._getPostBackSettings(postBackElement, eventTarget);
            }
        }

        if (!this._postBackSettings.async) {
                                    form.onsubmit = this._onsubmit;
            this._originalDoPostBack(eventTarget, eventArgument);
            form.onsubmit = null;
            return;
        }

        form.__EVENTTARGET.value = eventTarget;
        form.__EVENTARGUMENT.value = eventArgument;
        this._onFormSubmit();
    }

    function Sys$WebForms$PageRequestManager$_elementContains(container, element) {
		logger.error(`>>> Entering _elementContains(container=${container},element=${element})`, { "event.action": "_elementContains", "logrite.container": container, "logrite.element": element });
        while (element) {
            if (element === container) {
                return true;
            }
            element = element.parentNode;
        }
		logger.warn(`<<< Exiting _elementContains(container=${container},element=${element})`, { "event.action": "_elementContains", "logrite.container": container, "logrite.element": element });
        return false;
    }

    function Sys$WebForms$PageRequestManager$_endPostBack(error, response) {
		logger.error(`>>> Entering _endPostBack(error=${error},response=${response})`, { "event.action": "_endPostBack", "logrite.error": error, "logrite.response": response });
        this._processingRequest = false;

        this._request = null;
		logger.debug(`_endPostBack(error=${error},response=${response}): this._processingRequest → ${this._processingRequest}`, { "event.action": "_endPostBack", "logrite.error": error, "logrite.response": response });
        this._additionalInput = null;

		logger.debug(`_endPostBack(error=${error},response=${response}): this._request → ${this._request}`, { "event.action": "_endPostBack", "logrite.error": error, "logrite.response": response });
        var handler = this._get_eventHandlerList().getHandler("endRequest");
		logger.error(`_endPostBack(error=${error},response=${response}): handler → ${handler}`, { "event.action": "_endPostBack", "logrite.error": error, "logrite.response": response });
        var errorHandled = false;
		logger.error(`_endPostBack(error=${error},response=${response}): errorHandled → ${errorHandled}`, { "event.action": "_endPostBack", "logrite.error": error, "logrite.response": response });
        if (handler) {
            var eventArgs = new Sys.WebForms.EndRequestEventArgs(error, this._dataItems, response);
			logger.error(`_endPostBack(error=${error},response=${response}): eventArgs → ${eventArgs}`, { "event.action": "_endPostBack", "logrite.error": error, "logrite.response": response });
			logger.debug(`_endPostBack(error=${error},response=${response}): this._additionalInput → ${this._additionalInput}`, { "event.action": "_endPostBack", "logrite.error": error, "logrite.response": response });
            handler(this, eventArgs);
            errorHandled = eventArgs.get_errorHandled();
        }
        this._dataItems = null;
        if (error && !errorHandled) {
			logger.debug(`_endPostBack(error=${error},response=${response}): this._dataItems → ${this._dataItems}`, { "event.action": "_endPostBack", "logrite.error": error, "logrite.response": response });
            alert(error.message);
			logger.warn(`<<< Exiting _endPostBack(error=${error},response=${response})`, { "event.action": "_endPostBack", "logrite.error": error, "logrite.response": response });
        }
    }



    function Sys$WebForms$PageRequestManager$_findNearestElement(uniqueID) {
		logger.error(`>>> Entering _findNearestElement(uniqueID=${uniqueID})`, { "event.action": "_findNearestElement", "logrite.uniqueID": uniqueID });
        while (uniqueID.length > 0) {
            var clientID = this._uniqueIDToClientID(uniqueID);
			logger.error(`_findNearestElement(uniqueID=${uniqueID}): clientID → ${clientID}`, { "event.action": "_findNearestElement", "logrite.uniqueID": uniqueID });
            var element = document.getElementById(clientID);
			logger.error(`_findNearestElement(uniqueID=${uniqueID}): element → ${element}`, { "event.action": "_findNearestElement", "logrite.uniqueID": uniqueID });
            if (element) {
                return element;
            }
            var indexOfLastDollar = uniqueID.lastIndexOf('$');
			logger.error(`_findNearestElement(uniqueID=${uniqueID}): indexOfLastDollar → ${indexOfLastDollar}`, { "event.action": "_findNearestElement", "logrite.uniqueID": uniqueID });
            if (indexOfLastDollar === -1) {
                return null;
            }
            uniqueID = uniqueID.substring(0, indexOfLastDollar);
        }
		logger.warn(`<<< Exiting _findNearestElement(uniqueID=${uniqueID})`, { "event.action": "_findNearestElement", "logrite.uniqueID": uniqueID });
        return null;
    }

    function Sys$WebForms$PageRequestManager$_findText(text, location) {
		logger.error(`>>> Entering _findText(text=${text},location=${location})`, { "event.action": "_findText", "logrite.text": text, "logrite.location": location });
        var startIndex = Math.max(0, location - 20);
        var endIndex = Math.min(text.length, location + 20);
		logger.error(`_findText(text=${text},location=${location}): startIndex → ${startIndex}`, { "event.action": "_findText", "logrite.text": text, "logrite.location": location });
		logger.error(`_findText(text=${text},location=${location}): endIndex → ${endIndex}`, { "event.action": "_findText", "logrite.text": text, "logrite.location": location });
        return text.substring(startIndex, endIndex);
    }

    function Sys$WebForms$PageRequestManager$_getPageLoadedEventArgs(initialLoad) {
																																		logger.error(`>>> Entering _getPageLoadedEventArgs(initialLoad=${initialLoad})`, { "event.action": "_getPageLoadedEventArgs", "logrite.initialLoad": initialLoad });
                                                                                                                                        
        var updated = [];
        var created = [];

                var oldIDs = this._oldUpdatePanelIDs || [];         var newIDs = this._updatePanelIDs;         var childIDs = this._childUpdatePanelIDs || [];         var refreshedIDs = this._panelsToRefreshIDs || []; 
                for (var i = 0; i < refreshedIDs.length; i++) {
            Array.add(updated, document.getElementById(this._uniqueIDToClientID(refreshedIDs[i])));
        }

                        for (var i = 0; i < newIDs.length; i++) {
            if (initialLoad || Array.indexOf(childIDs, newIDs[i]) !== -1) {
                Array.add(created, document.getElementById(this._uniqueIDToClientID(newIDs[i])));
            }
        }

		logger.warn(`<<< Exiting _getPageLoadedEventArgs(initialLoad=${initialLoad})`, { "event.action": "_getPageLoadedEventArgs", "logrite.initialLoad": initialLoad });
        return new Sys.WebForms.PageLoadedEventArgs(updated, created, this._dataItems);
    }

    function Sys$WebForms$PageRequestManager$_getPageLoadingEventArgs() {
																																		logger.error('>>> Entering _getPageLoadingEventArgs()', { "event.action": "_getPageLoadingEventArgs" });
                                                                                                                                        
        var updated = [];
        var deleted = [];

        var oldIDs = this._oldUpdatePanelIDs;
        var newIDs = this._updatePanelIDs;
        var childIDs = this._childUpdatePanelIDs;
        var refreshedIDs = this._panelsToRefreshIDs;

                for (var i = 0; i < refreshedIDs.length; i++) {
            Array.add(updated, document.getElementById(this._uniqueIDToClientID(refreshedIDs[i])));
        }

                for (var i = 0; i < oldIDs.length; i++) {
            if (Array.indexOf(refreshedIDs, oldIDs[i]) === -1 &&
                (Array.indexOf(newIDs, oldIDs[i]) === -1 || Array.indexOf(childIDs, oldIDs[i]) > -1)) {
                Array.add(deleted, document.getElementById(this._uniqueIDToClientID(oldIDs[i])));
            }
        }

		logger.warn('<<< Exiting _getPageLoadingEventArgs()', { "event.action": "_getPageLoadingEventArgs" });
        return new Sys.WebForms.PageLoadingEventArgs(updated, deleted, this._dataItems);
    }

    function Sys$WebForms$PageRequestManager$_getPostBackSettings(element, elementUniqueID) {
	logger.error(`>>> Entering _getPostBackSettings(element=${element},elementUniqueID=${elementUniqueID})`, { "event.action": "_getPostBackSettings", "logrite.element": element, "logrite.elementUniqueID": elementUniqueID });


        var originalElement = element;

                        var proposedSettings = null;
						logger.error(`_getPostBackSettings(element=${element},elementUniqueID=${elementUniqueID}): proposedSettings → ${proposedSettings}`, { "event.action": "_getPostBackSettings", "logrite.element": element, "logrite.elementUniqueID": elementUniqueID });

                        while (element) {
            if (element.id) {
                                if (!proposedSettings && Array.contains(this._asyncPostBackControlClientIDs, element.id)) {
                                        proposedSettings = this._createPostBackSettings(true, this._scriptManagerID + '|' + elementUniqueID, originalElement);
                }
                else {
                    if (!proposedSettings && Array.contains(this._postBackControlClientIDs, element.id)) {
                                                return this._createPostBackSettings(false, null, null);
                    }
                    else {
                        var indexOfPanel = Array.indexOf(this._updatePanelClientIDs, element.id);
						logger.error(`_getPostBackSettings(element=${element},elementUniqueID=${elementUniqueID}): indexOfPanel → ${indexOfPanel}`, { "event.action": "_getPostBackSettings", "logrite.element": element, "logrite.elementUniqueID": elementUniqueID });
                        if (indexOfPanel !== -1) {
                                                        if (this._updatePanelHasChildrenAsTriggers[indexOfPanel]) {
                                                                
                                                                                                                                return this._createPostBackSettings(true, this._updatePanelIDs[indexOfPanel] + '|' + elementUniqueID, originalElement);
                            }
                            else {
                                                                                                return this._createPostBackSettings(true, this._scriptManagerID + '|' + elementUniqueID, originalElement);
                            }
                        }
                    }
                }

                                if (!proposedSettings && this._matchesParentIDInList(element.id, this._asyncPostBackControlClientIDs)) {
                                        proposedSettings = this._createPostBackSettings(true, this._scriptManagerID + '|' + elementUniqueID, originalElement);
                }
                else {
                    if (!proposedSettings && this._matchesParentIDInList(element.id, this._postBackControlClientIDs)) {
                                                return this._createPostBackSettings(false, null, null);
                    }
                }
            }

            element = element.parentNode;
        }

                                                if (!proposedSettings) {
                        return this._createPostBackSettings(false, null, null);
        }
        else {
			logger.warn(`<<< Exiting _getPostBackSettings(element=${element},elementUniqueID=${elementUniqueID})`, { "event.action": "_getPostBackSettings", "logrite.element": element, "logrite.elementUniqueID": elementUniqueID });
            return proposedSettings;
        }
    }

    function Sys$WebForms$PageRequestManager$_getScrollPosition() {
		logger.error('>>> Entering _getScrollPosition()', { "event.action": "_getScrollPosition" });
        var d = document.documentElement;
        if (d && (this._validPosition(d.scrollLeft) || this._validPosition(d.scrollTop))) {
            return {
                x: d.scrollLeft,
                y: d.scrollTop
            };
        }
        else {
            d = document.body;
            if (d && (this._validPosition(d.scrollLeft) || this._validPosition(d.scrollTop))) {
                return {
                    x: d.scrollLeft,
                    y: d.scrollTop
                };
            }
            else {
                if (this._validPosition(window.pageXOffset) || this._validPosition(window.pageYOffset)) {
                    return {
                        x: window.pageXOffset,
                        y: window.pageYOffset
                    };
                }
                else {
					logger.warn('<<< Exiting _getScrollPosition()', { "event.action": "_getScrollPosition" });
                    return {
                        x: 0,
                        y: 0
                    };
                }
            }
        }
    }

    function Sys$WebForms$PageRequestManager$_initializeInternal(scriptManagerID, formElement) {
		logger.error(`>>> Entering _initializeInternal(scriptManagerID=${scriptManagerID},formElement=${formElement})`, { "event.action": "_initializeInternal", "logrite.scriptManagerID": scriptManagerID, "logrite.formElement": formElement });
        this._scriptManagerID = scriptManagerID;

        this._form = formElement;
		logger.debug(`_initializeInternal(scriptManagerID=${scriptManagerID},formElement=${formElement}): this._scriptManagerID → ${this._scriptManagerID}`, { "event.action": "_initializeInternal", "logrite.scriptManagerID": scriptManagerID, "logrite.formElement": formElement });

        
				logger.debug(`_initializeInternal(scriptManagerID=${scriptManagerID},formElement=${formElement}): this._form → ${this._form}`, { "event.action": "_initializeInternal", "logrite.scriptManagerID": scriptManagerID, "logrite.formElement": formElement });
                this._form._initialAction = this._form.action;

        this._onsubmit = this._form.onsubmit;
        this._form.onsubmit = null;
		logger.debug(`_initializeInternal(scriptManagerID=${scriptManagerID},formElement=${formElement}): this._onsubmit → ${this._onsubmit}`, { "event.action": "_initializeInternal", "logrite.scriptManagerID": scriptManagerID, "logrite.formElement": formElement });
        this._onFormSubmitHandler = Function.createDelegate(this, this._onFormSubmit);
        this._onFormElementClickHandler = Function.createDelegate(this, this._onFormElementClick);
        this._onWindowUnloadHandler = Function.createDelegate(this, this._onWindowUnload);
        Sys.UI.DomEvent.addHandler(this._form, 'submit', this._onFormSubmitHandler);
        Sys.UI.DomEvent.addHandler(this._form, 'click', this._onFormElementClickHandler);
        Sys.UI.DomEvent.addHandler(window, 'unload', this._onWindowUnloadHandler);

        this._originalDoPostBack = window.__doPostBack;
                if (this._originalDoPostBack) {
				logger.debug(`_initializeInternal(scriptManagerID=${scriptManagerID},formElement=${formElement}): this._originalDoPostBack → ${this._originalDoPostBack}`, { "event.action": "_initializeInternal", "logrite.scriptManagerID": scriptManagerID, "logrite.formElement": formElement });
            window.__doPostBack = Function.createDelegate(this, this._doPostBack);
        }

        this._pageLoadedHandler = Function.createDelegate(this, this._pageLoadedInitialLoad);
        Sys.UI.DomEvent.addHandler(window, 'load', this._pageLoadedHandler);
		logger.warn(`<<< Exiting _initializeInternal(scriptManagerID=${scriptManagerID},formElement=${formElement})`, { "event.action": "_initializeInternal", "logrite.scriptManagerID": scriptManagerID, "logrite.formElement": formElement });
    }

    function Sys$WebForms$PageRequestManager$_matchesParentIDInList(clientID, parentIDList) {
		logger.error(`>>> Entering _matchesParentIDInList(clientID=${clientID},parentIDList=${parentIDList})`, { "event.action": "_matchesParentIDInList", "logrite.clientID": clientID, "logrite.parentIDList": parentIDList });
        for (var i = 0; i < parentIDList.length; i++) {
            if (clientID.startsWith(parentIDList[i] + "_")) {
                return true;
            }
        }
		logger.warn(`<<< Exiting _matchesParentIDInList(clientID=${clientID},parentIDList=${parentIDList})`, { "event.action": "_matchesParentIDInList", "logrite.clientID": clientID, "logrite.parentIDList": parentIDList });
        return false;
    }

    function Sys$WebForms$PageRequestManager$_onFormElementClick(evt) {
		logger.error(`>>> Entering _onFormElementClick(evt=${evt})`, { "event.action": "_onFormElementClick", "logrite.evt": evt });
        var element = evt.target;
        if (element.disabled) {
            return;
        }

                this._postBackSettings = this._getPostBackSettings(element, element.name);


        if (element.name) {
            if (element.tagName === 'INPUT') {
                var type = element.type;
                if (type === 'submit') {
                    this._additionalInput = element.name + '=' + encodeURIComponent(element.value);
                }
                else if (type === 'image') {
                    var x = evt.offsetX;
                    var y = evt.offsetY;
                    this._additionalInput = element.name + '.x=' + x + '&' + element.name + '.y=' + y;
                }
            }
            else if ((element.tagName === 'BUTTON') && (element.name.length !== 0) && (element.type === 'submit')) {
                this._additionalInput = element.name + '=' + encodeURIComponent(element.value);
            }
        }
    }

    function Sys$WebForms$PageRequestManager$_onFormSubmit(evt) {
		logger.error(`>>> Entering _onFormSubmit(evt=${evt})`, { "event.action": "_onFormSubmit", "logrite.evt": evt });
        var continueSubmit = true;

                if (this._onsubmit) {
            continueSubmit = this._onsubmit();
        }

                if (continueSubmit) {
            for (var i = 0; i < this._onSubmitStatements.length; i++) {
                if (!this._onSubmitStatements[i]()) {
                    continueSubmit = false;
                    break;
                }
            }
        }

        if (!continueSubmit) {
            if (evt) {
                evt.preventDefault();
            }
            return;
        }

        var form = this._form;
        if (form.action !== form._initialAction) {
                                    return;
        }

                        if (!this._postBackSettings.async) {
            return;
        }

                var formBody = new Sys.StringBuilder();
				logger.error(`_onFormSubmit(evt=${evt}): formBody → ${formBody}`, { "event.action": "_onFormSubmit", "logrite.evt": evt });
        formBody.append(this._scriptManagerID + '=' + this._postBackSettings.panelID + '&');

        var count = form.elements.length;
		logger.error(`_onFormSubmit(evt=${evt}): count → ${count}`, { "event.action": "_onFormSubmit", "logrite.evt": evt });
        for (var i = 0; i < count; i++) {
            var element = form.elements[i];
            var name = element.name;
			logger.error(`_onFormSubmit(evt=${evt}): name → ${name}`, { "event.action": "_onFormSubmit", "logrite.evt": evt });
            if (typeof(name) === "undefined" || (name === null) || (name.length === 0)) {
                continue;
            }

            var tagName = element.tagName;

            if (tagName === 'INPUT') {
                var type = element.type;
                if ((type === 'text') ||
                    (type === 'password') ||
                    (type === 'hidden') ||
                    (((type === 'checkbox') || (type === 'radio')) && element.checked)) {
                    formBody.append(name);
                    formBody.append('=');
                    formBody.append(encodeURIComponent(element.value));
                    formBody.append('&');
                }
            }
            else if (tagName === 'SELECT') {
                var optionCount = element.options.length;
                for (var j = 0; j < optionCount; j++) {
                    var option = element.options[j];
                    if (option.selected) {
                        formBody.append(name);
                        formBody.append('=');
                        formBody.append(encodeURIComponent(option.value));
                        formBody.append('&');
                    }
                }
            }
            else if (tagName === 'TEXTAREA') {
                formBody.append(name);
                formBody.append('=');
                formBody.append(encodeURIComponent(element.value));
                formBody.append('&');
            }
        }

        if (this._additionalInput) {
            formBody.append(this._additionalInput);
            this._additionalInput = null;
        }

        var request = new Sys.Net.WebRequest();
        request.set_url(form.action);
        request.get_headers()['X-MicrosoftAjax'] = 'Delta=true';
        request.get_headers()['Cache-Control'] = 'no-cache';
        request.set_timeout(this._asyncPostBackTimeout);
        request.add_completed(Function.createDelegate(this, this._onFormSubmitCompleted));
        request.set_body(formBody.toString());

        var handler = this._get_eventHandlerList().getHandler("initializeRequest");
        if (handler) {
            var eventArgs = new Sys.WebForms.InitializeRequestEventArgs(request, this._postBackSettings.sourceElement);
            handler(this, eventArgs);
            continueSubmit = !eventArgs.get_cancel();
        }

        if (!continueSubmit) {
            if (evt) {
                evt.preventDefault();
            }
            return;
        }

                this._scrollPosition = this._getScrollPosition();


                                this.abortPostBack();

        handler = this._get_eventHandlerList().getHandler("beginRequest");
        if (handler) {
            var eventArgs = new Sys.WebForms.BeginRequestEventArgs(request, this._postBackSettings.sourceElement);
            handler(this, eventArgs);
        }

        this._request = request;
        request.invoke();

                if (evt) {
            evt.preventDefault();
        }
    }

    function Sys$WebForms$PageRequestManager$_onFormSubmitCompleted(sender, eventArgs) {
		logger.error(`>>> Entering _onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs})`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
        this._processingRequest = true;

        var delimitByLengthDelimiter = '|';
		logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): delimitByLengthDelimiter → ${delimitByLengthDelimiter}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
		logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): this._processingRequest → ${this._processingRequest}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
        
        if (sender.get_timedOut()) {
            this._endPostBack(this._createPageRequestManagerTimeoutError(), sender);
            return;
        }

        if (sender.get_aborted()) {
            this._endPostBack(null, sender);
            return;
        }

                if (!this._request || sender.get_webRequest() !== this._request) {
            return;
        }

        var errorMessage;
        var delta = [];

                if (sender.get_statusCode() !== 200) {
            this._endPostBack(this._createPageRequestManagerServerError(sender.get_statusCode()), sender);
            return;
        }

                        var reply = sender.get_responseData();
						logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): reply → ${reply}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
        var delimiterIndex, len, type, id, content;
        var replyIndex = 0;
        var parserErrorDetails = null;
		logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): parserErrorDetails → ${parserErrorDetails}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });

        while (replyIndex < reply.length) {
                        delimiterIndex = reply.indexOf(delimitByLengthDelimiter, replyIndex);
						logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): delimiterIndex → ${delimiterIndex}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            if (delimiterIndex === -1) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }
            len = parseInt(reply.substring(replyIndex, delimiterIndex), 10);
            if ((len % 1) !== 0) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }
            replyIndex = delimiterIndex + 1;

                        delimiterIndex = reply.indexOf(delimitByLengthDelimiter, replyIndex);
            if (delimiterIndex === -1) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }
            type = reply.substring(replyIndex, delimiterIndex);
            replyIndex = delimiterIndex + 1;

                        delimiterIndex = reply.indexOf(delimitByLengthDelimiter, replyIndex);
            if (delimiterIndex === -1) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }
            id = reply.substring(replyIndex, delimiterIndex);
            replyIndex = delimiterIndex + 1;

                        if ((replyIndex + len) >= reply.length) {
                parserErrorDetails = this._findText(reply, reply.length);
                break;
            }
            content = this._decodeString(reply.substr(replyIndex, len));
            replyIndex += len;

                        if (reply.charAt(replyIndex) !== delimitByLengthDelimiter) {
                parserErrorDetails = this._findText(reply, replyIndex);
                break;
            }

            replyIndex++;

            Array.add(delta, {type: type, id: id, content: content});
        }

                if (parserErrorDetails) {
            this._endPostBack(this._createPageRequestManagerParserError(String.format(Sys.WebForms.Res.PRM_ParserErrorDetails, parserErrorDetails)), sender);
            return;
        }


        var updatePanelNodes = [];
        var hiddenFieldNodes = [];
        var arrayDeclarationNodes = [];
        var scriptBlockNodes = [];
        var expandoNodes = [];
        var onSubmitNodes = [];
        var dataItemNodes = [];
		logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): dataItemNodes → ${dataItemNodes}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
        var dataItemJsonNodes = [];
		logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): dataItemJsonNodes → ${dataItemJsonNodes}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
        var scriptDisposeNodes = [];
        var asyncPostBackControlIDsNode, postBackControlIDsNode,
            updatePanelIDsNode, asyncPostBackTimeoutNode,
            childUpdatePanelIDsNode, panelsToRefreshNode, formActionNode;

                for (var i = 0; i < delta.length; i++) {
            var deltaNode = delta[i];
            switch (deltaNode.type) {
                case "updatePanel":
                    Array.add(updatePanelNodes, deltaNode);
                    break;
                case "hiddenField":
                    Array.add(hiddenFieldNodes, deltaNode);
                    break;
                case "arrayDeclaration":
                    Array.add(arrayDeclarationNodes, deltaNode);
                    break;
                case "scriptBlock":
                    Array.add(scriptBlockNodes, deltaNode);
                    break;
                case "expando":
                    Array.add(expandoNodes, deltaNode);
                    break;
                case "onSubmit":
                    Array.add(onSubmitNodes, deltaNode);
                    break;
                case "asyncPostBackControlIDs":
                    asyncPostBackControlIDsNode = deltaNode;
					logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): asyncPostBackControlIDsNode → ${asyncPostBackControlIDsNode}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
                    break;
                case "postBackControlIDs":
                    postBackControlIDsNode = deltaNode;
                    break;
                case "updatePanelIDs":
                    updatePanelIDsNode = deltaNode;
                    break;
                case "asyncPostBackTimeout":
                    asyncPostBackTimeoutNode = deltaNode;
                    break;
                case "childUpdatePanelIDs":
                    childUpdatePanelIDsNode = deltaNode;
                    break;
                case "panelsToRefreshIDs":
                    panelsToRefreshNode = deltaNode;
                    break;
                case "formAction":
                    formActionNode = deltaNode;
                    break;
                case "dataItem":
                    Array.add(dataItemNodes, deltaNode);
                    break;
                case "dataItemJson":
                    Array.add(dataItemJsonNodes, deltaNode);
                    break;
                case "scriptDispose":
                    Array.add(scriptDisposeNodes, deltaNode);
                    break;
                case "pageRedirect":
                    window.location.href = deltaNode.content;
                    return;
                case "error":
                                        this._endPostBack(this._createPageRequestManagerServerError(Number.parseInvariant(deltaNode.id), deltaNode.content), sender);
                    return;
                case "pageTitle":
                    document.title = deltaNode.content;
                    break;
                case "focus":
                    this._controlIDToFocus = deltaNode.content;
                    break;
                default:
										logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): this._controlIDToFocus → ${this._controlIDToFocus}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
                                        this._endPostBack(this._createPageRequestManagerParserError(String.format(Sys.WebForms.Res.PRM_UnknownToken, deltaNode.type)), sender);
                    return;
            }
        }

        var i;

                if (asyncPostBackControlIDsNode && postBackControlIDsNode &&
            updatePanelIDsNode && panelsToRefreshNode &&
            asyncPostBackTimeoutNode && childUpdatePanelIDsNode) {

            this._oldUpdatePanelIDs = this._updatePanelIDs;
            var childUpdatePanelIDsString = childUpdatePanelIDsNode.content;
			logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): this._oldUpdatePanelIDs → ${this._oldUpdatePanelIDs}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            this._childUpdatePanelIDs = childUpdatePanelIDsString.length ? childUpdatePanelIDsString.split(',') : [];

            var asyncPostBackControlIDsArray = this._splitNodeIntoArray(asyncPostBackControlIDsNode);
			logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): asyncPostBackControlIDsArray → ${asyncPostBackControlIDsArray}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            var postBackControlIDsArray = this._splitNodeIntoArray(postBackControlIDsNode);
			logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): postBackControlIDsArray → ${postBackControlIDsArray}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            var updatePanelIDsArray = this._splitNodeIntoArray(updatePanelIDsNode);
			logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): updatePanelIDsArray → ${updatePanelIDsArray}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            this._panelsToRefreshIDs = this._splitNodeIntoArray(panelsToRefreshNode);

                                                for (i = 0; i < this._panelsToRefreshIDs.length; i++) {
                var panelClientID = this._uniqueIDToClientID(this._panelsToRefreshIDs[i]);
				logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): panelClientID → ${panelClientID}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
                if (!document.getElementById(panelClientID)) {
                    this._endPostBack(Error.invalidOperation(String.format(Sys.WebForms.Res.PRM_MissingPanel, panelClientID)), sender);
                    return;
                }
            }

            var asyncPostBackTimeout = asyncPostBackTimeoutNode.content;
            this._updateControls(updatePanelIDsArray, asyncPostBackControlIDsArray, postBackControlIDsArray, asyncPostBackTimeout);
        }

                this._dataItems = {};
        for (i = 0; i < dataItemNodes.length; i++) {
			logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): this._dataItems → ${this._dataItems}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            var dataItemNode = dataItemNodes[i];
			logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): dataItemNode → ${dataItemNode}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            this._dataItems[dataItemNode.id] = dataItemNode.content;
        }
        for (i = 0; i < dataItemJsonNodes.length; i++) {
            var dataItemJsonNode = dataItemJsonNodes[i];
			logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): dataItemJsonNode → ${dataItemJsonNode}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            this._dataItems[dataItemJsonNode.id] = eval(dataItemJsonNode.content);
        }


        var handler = this._get_eventHandlerList().getHandler("pageLoading");
		logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): handler → ${handler}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
        if (handler) {
            handler(this, this._getPageLoadingEventArgs());
        }


                if (formActionNode) {
            this._form.action = formActionNode.content;
                                                this._form._initialAction = this._form.action;
        }

                                for (i = 0; i < updatePanelNodes.length; i++) {
            var deltaUpdatePanel = updatePanelNodes[i];

            var deltaPanelID = deltaUpdatePanel.id;
            var deltaPanelRendering = deltaUpdatePanel.content;

            var updatePanelElement = document.getElementById(deltaPanelID);
			logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): updatePanelElement → ${updatePanelElement}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });

            if (!updatePanelElement) {
                this._endPostBack(Error.invalidOperation(String.format(Sys.WebForms.Res.PRM_MissingPanel, deltaPanelID)), sender);
                return;
            }

            this._updatePanel(updatePanelElement, deltaPanelRendering);
        }

                                for (i = 0; i < scriptDisposeNodes.length; i++) {
            var disposePanelId = scriptDisposeNodes[i].id;
            var disposeScript = scriptDisposeNodes[i].content;
            this._registerDisposeScript(disposePanelId, disposeScript);
        }

                for (i = 0; i < hiddenFieldNodes.length; i++) {
            var id = hiddenFieldNodes[i].id;
			logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): id → ${id}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            var value = hiddenFieldNodes[i].content;

            var hiddenFieldElement = document.getElementById(id);
			logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): hiddenFieldElement → ${hiddenFieldElement}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
			logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): hiddenFieldElement.id → ${hiddenFieldElement.id}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
			logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): hiddenFieldElement.name → ${hiddenFieldElement.name}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
			logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): hiddenFieldElement.type → ${hiddenFieldElement.type}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
			logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): hiddenFieldElement.value → ${hiddenFieldElement.value}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
            if (!hiddenFieldElement) {
                                hiddenFieldElement = document.createElement('input');
                hiddenFieldElement.id = id;
                hiddenFieldElement.name = id;
                hiddenFieldElement.type = 'hidden';
                this._form.appendChild(hiddenFieldElement);
            }
            hiddenFieldElement.value = value;
        }

                var arrayScript = '';
				logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): arrayScript → ${arrayScript}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
        for (i = 0; i < arrayDeclarationNodes.length; i++) {
            arrayScript += "Sys.WebForms.PageRequestManager._addArrayElement('" + arrayDeclarationNodes[i].id + "', " + arrayDeclarationNodes[i].content + ");\r\n";
        }

                var expandoScript = '';
				logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): expandoScript → ${expandoScript}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
        for (i = 0; i < expandoNodes.length; i++) {
            var propertyReference = expandoNodes[i].id;
            var propertyValue = expandoNodes[i].content;
            expandoScript += propertyReference + " = " + propertyValue + "\r\n";
        }

                        Sys._ScriptLoader.readLoadedScripts();

                                Sys.Application.beginCreateComponents();

        var scriptLoader = Sys._ScriptLoader.getInstance();
		logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): scriptLoader → ${scriptLoader}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });

                                        if (arrayScript.length) {
            scriptLoader.queueScriptBlock(arrayScript);
        }
        if (expandoScript.length) {
            scriptLoader.queueScriptBlock(expandoScript);
        }

        for (i = 0; i < scriptBlockNodes.length; i++) {
            var scriptBlockType = scriptBlockNodes[i].id;
            switch (scriptBlockType) {
                case "ScriptContentNoTags":
                                        scriptLoader.queueScriptBlock(scriptBlockNodes[i].content);
                    break;
                case "ScriptContentWithTags":
                                        var scriptTagAttributes;
                    eval("scriptTagAttributes = " + scriptBlockNodes[i].content);

                                        if (scriptTagAttributes.src && Sys._ScriptLoader.isScriptLoaded(scriptTagAttributes.src)) {
                        continue;
                    }

                    scriptLoader.queueCustomScriptTag(scriptTagAttributes);
                    break;
                case "ScriptPath":
                                        if (Sys._ScriptLoader.isScriptLoaded(scriptBlockNodes[i].content)) {
                        continue;
                    }

                                        scriptLoader.queueScriptReference(scriptBlockNodes[i].content);
                    break;
            }
        }

                        var onSubmitStatementScript = '';
						logger.error(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): onSubmitStatementScript → ${onSubmitStatementScript}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
        for (var i = 0; i < onSubmitNodes.length; i++) {
            if (i === 0) {
                onSubmitStatementScript = 'Array.add(Sys.WebForms.PageRequestManager.getInstance()._onSubmitStatements, function() {\r\n';
            }

            onSubmitStatementScript += onSubmitNodes[i].content + "\r\n";
        }
        if (onSubmitStatementScript.length) {
            onSubmitStatementScript += "\r\nreturn true;\r\n});\r\n";
            scriptLoader.queueScriptBlock(onSubmitStatementScript);
        }

                this._response = sender;

						logger.debug(`_onFormSubmitCompleted(sender=${sender},eventArgs=${eventArgs}): this._response → ${this._response}`, { "event.action": "_onFormSubmitCompleted", "logrite.sender": sender, "logrite.eventArgs": eventArgs });
                        scriptLoader.loadScripts(0, Function.createDelegate(this, this._scriptsLoadComplete), null, null);

                    }

    function Sys$WebForms$PageRequestManager$_onWindowUnload(evt) {
		logger.error(`>>> Entering _onWindowUnload(evt=${evt})`, { "event.action": "_onWindowUnload", "logrite.evt": evt });
        this.dispose();
		logger.warn(`<<< Exiting _onWindowUnload(evt=${evt})`, { "event.action": "_onWindowUnload", "logrite.evt": evt });
    }

    function Sys$WebForms$PageRequestManager$_pageLoaded(initialLoad) {
		logger.error(`>>> Entering _pageLoaded(initialLoad=${initialLoad})`, { "event.action": "_pageLoaded", "logrite.initialLoad": initialLoad });
        var handler = this._get_eventHandlerList().getHandler("pageLoaded");
        if (handler) {
			logger.error(`_pageLoaded(initialLoad=${initialLoad}): handler → ${handler}`, { "event.action": "_pageLoaded", "logrite.initialLoad": initialLoad });
            handler(this, this._getPageLoadedEventArgs(initialLoad));
        }
        if (!initialLoad) {
                                    Sys.Application.raiseLoad();
									logger.warn(`<<< Exiting _pageLoaded(initialLoad=${initialLoad})`, { "event.action": "_pageLoaded", "logrite.initialLoad": initialLoad });
        }
    }

    function Sys$WebForms$PageRequestManager$_pageLoadedInitialLoad(evt) {
		logger.error(`>>> Entering _pageLoadedInitialLoad(evt=${evt})`, { "event.action": "_pageLoadedInitialLoad", "logrite.evt": evt });
        this._pageLoaded(true);
		logger.warn(`<<< Exiting _pageLoadedInitialLoad(evt=${evt})`, { "event.action": "_pageLoadedInitialLoad", "logrite.evt": evt });
    }

    function Sys$WebForms$PageRequestManager$_registerDisposeScript(panelID, disposeScript) {
		logger.error(`>>> Entering _registerDisposeScript(panelID=${panelID},disposeScript=${disposeScript})`, { "event.action": "_registerDisposeScript", "logrite.panelID": panelID, "logrite.disposeScript": disposeScript });
        if (!this._scriptDisposes[panelID]) {
            this._scriptDisposes[panelID] = [disposeScript];
        }
        else {
            Array.add(this._scriptDisposes[panelID], disposeScript);
			logger.warn(`<<< Exiting _registerDisposeScript(panelID=${panelID},disposeScript=${disposeScript})`, { "event.action": "_registerDisposeScript", "logrite.panelID": panelID, "logrite.disposeScript": disposeScript });
        }
    }

    function Sys$WebForms$PageRequestManager$_scriptsLoadComplete() {
						logger.error('>>> Entering _scriptsLoadComplete()', { "event.action": "_scriptsLoadComplete" });
                        
						logger.warn('<<< Exiting _scriptsLoadComplete()', { "event.action": "_scriptsLoadComplete" });
                                                if (window.__theFormPostData) {
            window.__theFormPostData = "";
        }
        if (window.__theFormPostCollection) {
            window.__theFormPostCollection = [];
        }
        if (window.WebForm_InitCallback) {
            window.WebForm_InitCallback();
        }

                if (this._scrollPosition) {
                        if (window.scrollTo) {
                window.scrollTo(this._scrollPosition.x, this._scrollPosition.y);
            }
            this._scrollPosition = null;
        }

		logger.debug(`_scriptsLoadComplete(): this._scrollPosition → ${this._scrollPosition}`, { "event.action": "_scriptsLoadComplete" });
        Sys.Application.endCreateComponents();

                this._pageLoaded(false);

        this._endPostBack(null, this._response);
        this._response = null;

				logger.debug(`_scriptsLoadComplete(): this._response → ${this._response}`, { "event.action": "_scriptsLoadComplete" });
                if (this._controlIDToFocus) {
            var focusTarget;
            var oldContentEditableSetting;
            if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
                                                                                var targetControl = $get(this._controlIDToFocus);
																				logger.error(`_scriptsLoadComplete(): targetControl → ${targetControl}`, { "event.action": "_scriptsLoadComplete" });

                var focusTarget = targetControl;
								logger.debug(`_scriptsLoadComplete(): focusTarget.contentEditable → ${focusTarget.contentEditable}`, { "event.action": "_scriptsLoadComplete" });
                                if (targetControl && (!WebForm_CanFocus(targetControl))) {
                    focusTarget = WebForm_FindFirstFocusableChild(targetControl);
                }
                                                if (focusTarget && (typeof(focusTarget.contentEditable) !== "undefined")) {
                    oldContentEditableSetting = focusTarget.contentEditable;
                    focusTarget.contentEditable = false;
					logger.debug(`_scriptsLoadComplete(): oldContentEditableSetting → ${oldContentEditableSetting}`, { "event.action": "_scriptsLoadComplete" });
                }
                else {
                    focusTarget = null;
                }
            }
            WebForm_AutoFocus(this._controlIDToFocus);
            if (focusTarget) {
                                focusTarget.contentEditable = oldContentEditableSetting;
            }
            this._controlIDToFocus = null;
			logger.debug(`_scriptsLoadComplete(): this._controlIDToFocus → ${this._controlIDToFocus}`, { "event.action": "_scriptsLoadComplete" });
        }
    }

    function Sys$WebForms$PageRequestManager$_splitNodeIntoArray(node) {
		logger.error(`>>> Entering _splitNodeIntoArray(node=${node})`, { "event.action": "_splitNodeIntoArray", "logrite.node": node });
        var str = node.content;
        var arr = str.length ? str.split(',') : [];
		logger.error(`_splitNodeIntoArray(node=${node}): arr → ${arr}`, { "event.action": "_splitNodeIntoArray", "logrite.node": node });
        return arr;
    }

    function Sys$WebForms$PageRequestManager$_uniqueIDToClientID(uniqueID) {
				logger.error(`>>> Entering _uniqueIDToClientID(uniqueID=${uniqueID})`, { "event.action": "_uniqueIDToClientID", "logrite.uniqueID": uniqueID });
				logger.warn(`<<< Exiting _uniqueIDToClientID(uniqueID=${uniqueID})`, { "event.action": "_uniqueIDToClientID", "logrite.uniqueID": uniqueID });
                return uniqueID.replace(/\$/g, '_');
    }

    function Sys$WebForms$PageRequestManager$_updateControls(updatePanelIDs, asyncPostBackControlIDs, postBackControlIDs, asyncPostBackTimeout) {
		logger.error(`>>> Entering _updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout})`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });
        if (updatePanelIDs) {
                                                this._updatePanelIDs = new Array(updatePanelIDs.length);
            this._updatePanelClientIDs = new Array(updatePanelIDs.length);
            this._updatePanelHasChildrenAsTriggers = new Array(updatePanelIDs.length);
            for (var i = 0; i < updatePanelIDs.length; i++) {
                var realPanelID = updatePanelIDs[i].substr(1);
				logger.error(`_updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout}): realPanelID → ${realPanelID}`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });
                var childrenAsTriggers = (updatePanelIDs[i].charAt(0) === 't');
				logger.error(`_updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout}): childrenAsTriggers → ${childrenAsTriggers}`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });

                                this._updatePanelHasChildrenAsTriggers[i] = childrenAsTriggers;
                this._updatePanelIDs[i] = realPanelID;
                this._updatePanelClientIDs[i] = this._uniqueIDToClientID(realPanelID);
            }
            this._asyncPostBackTimeout = asyncPostBackTimeout * 1000;
        }
        else {
			logger.debug(`_updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout}): this._asyncPostBackTimeout → ${this._asyncPostBackTimeout}`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });
            this._updatePanelIDs = [];
            this._updatePanelClientIDs = [];
			logger.debug(`_updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout}): this._updatePanelIDs → ${this._updatePanelIDs}`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });
            this._updatePanelHasChildrenAsTriggers = [];
            this._asyncPostBackTimeout = 0;
			logger.debug(`_updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout}): this._updatePanelClientIDs → ${this._updatePanelClientIDs}`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });
        }

		logger.debug(`_updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout}): this._updatePanelHasChildrenAsTriggers → ${this._updatePanelHasChildrenAsTriggers}`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });
        this._asyncPostBackControlIDs = [];
        this._asyncPostBackControlClientIDs = [];
        this._convertToClientIDs(asyncPostBackControlIDs, this._asyncPostBackControlIDs, this._asyncPostBackControlClientIDs);

		logger.debug(`_updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout}): this._asyncPostBackControlIDs → ${this._asyncPostBackControlIDs}`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });
        this._postBackControlIDs = [];
        this._postBackControlClientIDs = [];
		logger.debug(`_updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout}): this._asyncPostBackControlClientIDs → ${this._asyncPostBackControlClientIDs}`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });
		logger.debug(`_updateControls(updatePanelIDs=${updatePanelIDs},asyncPostBackControlIDs=${asyncPostBackControlIDs},postBackControlIDs=${postBackControlIDs},asyncPostBackTimeout=${asyncPostBackTimeout}): this._postBackControlIDs → ${this._postBackControlIDs}`, { "event.action": "_updateControls", "logrite.updatePanelIDs": updatePanelIDs, "logrite.asyncPostBackControlIDs": asyncPostBackControlIDs, "logrite.postBackControlIDs": postBackControlIDs, "logrite.asyncPostBackTimeout": asyncPostBackTimeout });
        this._convertToClientIDs(postBackControlIDs, this._postBackControlIDs, this._postBackControlClientIDs);
    }

    function Sys$WebForms$PageRequestManager$_updatePanel(updatePanelElement, rendering) {
	logger.error(`>>> Entering _updatePanel(updatePanelElement=${updatePanelElement},rendering=${rendering})`, { "event.action": "_updatePanel", "logrite.updatePanelElement": updatePanelElement, "logrite.rendering": rendering });

        for (var updatePanelID in this._scriptDisposes) {
            if (this._elementContains(updatePanelElement, document.getElementById(updatePanelID))) {
                                var disposeScripts = this._scriptDisposes[updatePanelID];
                for (var i = 0; i < disposeScripts.length; i++) {
                    eval(disposeScripts[i]);
                }

                                delete this._scriptDisposes[updatePanelID];
            }
        }

        this._destroyTree(updatePanelElement);

                updatePanelElement.innerHTML = rendering;
				logger.warn(`<<< Exiting _updatePanel(updatePanelElement=${updatePanelElement},rendering=${rendering})`, { "event.action": "_updatePanel", "logrite.updatePanelElement": updatePanelElement, "logrite.rendering": rendering });
    }

    function Sys$WebForms$PageRequestManager$_validPosition(position) {
		logger.error(`>>> Entering _validPosition(position=${position})`, { "event.action": "_validPosition", "logrite.position": position });
		logger.warn(`<<< Exiting _validPosition(position=${position})`, { "event.action": "_validPosition", "logrite.position": position });
        return (typeof(position) !== "undefined") && (position !== null) && (position !== 0);
    }
Sys.WebForms.PageRequestManager.prototype = {

    _get_eventHandlerList: Sys$WebForms$PageRequestManager$_get_eventHandlerList,

    get_isInAsyncPostBack: Sys$WebForms$PageRequestManager$get_isInAsyncPostBack,

        add_beginRequest: Sys$WebForms$PageRequestManager$add_beginRequest,
    remove_beginRequest: Sys$WebForms$PageRequestManager$remove_beginRequest,

    add_endRequest: Sys$WebForms$PageRequestManager$add_endRequest,
    remove_endRequest: Sys$WebForms$PageRequestManager$remove_endRequest,

    add_initializeRequest: Sys$WebForms$PageRequestManager$add_initializeRequest,
    remove_initializeRequest: Sys$WebForms$PageRequestManager$remove_initializeRequest,

    add_pageLoaded: Sys$WebForms$PageRequestManager$add_pageLoaded,
    remove_pageLoaded: Sys$WebForms$PageRequestManager$remove_pageLoaded,

    add_pageLoading: Sys$WebForms$PageRequestManager$add_pageLoading,
    remove_pageLoading: Sys$WebForms$PageRequestManager$remove_pageLoading,

    abortPostBack: Sys$WebForms$PageRequestManager$abortPostBack,

    _createPageRequestManagerTimeoutError: Sys$WebForms$PageRequestManager$_createPageRequestManagerTimeoutError,

    _createPageRequestManagerServerError: Sys$WebForms$PageRequestManager$_createPageRequestManagerServerError,

    _createPageRequestManagerParserError: Sys$WebForms$PageRequestManager$_createPageRequestManagerParserError,

    _createPostBackSettings: Sys$WebForms$PageRequestManager$_createPostBackSettings,

    _convertToClientIDs: Sys$WebForms$PageRequestManager$_convertToClientIDs,

    _decodeString: Sys$WebForms$PageRequestManager$_decodeString,

    _destroyTree: Sys$WebForms$PageRequestManager$_destroyTree,

    dispose: Sys$WebForms$PageRequestManager$dispose,

        _doPostBack: Sys$WebForms$PageRequestManager$_doPostBack,

    _elementContains: Sys$WebForms$PageRequestManager$_elementContains,

    _endPostBack: Sys$WebForms$PageRequestManager$_endPostBack,

            _findNearestElement: Sys$WebForms$PageRequestManager$_findNearestElement,

    _findText: Sys$WebForms$PageRequestManager$_findText,

    _getPageLoadedEventArgs: Sys$WebForms$PageRequestManager$_getPageLoadedEventArgs,

    _getPageLoadingEventArgs: Sys$WebForms$PageRequestManager$_getPageLoadingEventArgs,

    _getPostBackSettings: Sys$WebForms$PageRequestManager$_getPostBackSettings,

    _getScrollPosition: Sys$WebForms$PageRequestManager$_getScrollPosition,

    _initializeInternal: Sys$WebForms$PageRequestManager$_initializeInternal,

    _matchesParentIDInList: Sys$WebForms$PageRequestManager$_matchesParentIDInList,

    _onFormElementClick: Sys$WebForms$PageRequestManager$_onFormElementClick,

    _onFormSubmit: Sys$WebForms$PageRequestManager$_onFormSubmit,

    _onFormSubmitCompleted: Sys$WebForms$PageRequestManager$_onFormSubmitCompleted,

    _onWindowUnload: Sys$WebForms$PageRequestManager$_onWindowUnload,

    _pageLoaded: Sys$WebForms$PageRequestManager$_pageLoaded,

    _pageLoadedInitialLoad: Sys$WebForms$PageRequestManager$_pageLoadedInitialLoad,

    _registerDisposeScript: Sys$WebForms$PageRequestManager$_registerDisposeScript,

    _scriptsLoadComplete: Sys$WebForms$PageRequestManager$_scriptsLoadComplete,

    _splitNodeIntoArray: Sys$WebForms$PageRequestManager$_splitNodeIntoArray,

    _uniqueIDToClientID: Sys$WebForms$PageRequestManager$_uniqueIDToClientID,

    _updateControls: Sys$WebForms$PageRequestManager$_updateControls,

    _updatePanel: Sys$WebForms$PageRequestManager$_updatePanel,

    _validPosition: Sys$WebForms$PageRequestManager$_validPosition
}

Sys.WebForms.PageRequestManager.getInstance = function Sys$WebForms$PageRequestManager$getInstance() {
	logger.error('>>> Entering getInstance()', { "event.action": "getInstance" });
    /// <returns type="Sys.WebForms.PageRequestManager"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting getInstance()', { "event.action": "getInstance" });
    return Sys.WebForms.PageRequestManager._instance || null;
}

Sys.WebForms.PageRequestManager._addArrayElement = function Sys$WebForms$PageRequestManager$_addArrayElement(arrayName, arrayValue) {
	logger.error(`>>> Entering _addArrayElement(arrayName=${arrayName},arrayValue=${arrayValue})`, { "event.action": "_addArrayElement", "logrite.arrayName": arrayName, "logrite.arrayValue": arrayValue });
    if (typeof(window[arrayName]) === "undefined") {
                window[arrayName] = [ arrayValue ];
    }
    else {
                Array.add(window[arrayName], arrayValue);
				logger.warn(`<<< Exiting _addArrayElement(arrayName=${arrayName},arrayValue=${arrayValue})`, { "event.action": "_addArrayElement", "logrite.arrayName": arrayName, "logrite.arrayValue": arrayValue });
    }
}

Sys.WebForms.PageRequestManager._initialize = function Sys$WebForms$PageRequestManager$_initialize(scriptManagerID, formElement) {
	logger.error(`>>> Entering _initialize(scriptManagerID=${scriptManagerID},formElement=${formElement})`, { "event.action": "_initialize", "logrite.scriptManagerID": scriptManagerID, "logrite.formElement": formElement });
    if (Sys.WebForms.PageRequestManager.getInstance()) {
        throw Error.invalidOperation(Sys.WebForms.Res.PRM_CannotRegisterTwice);
    }
    Sys.WebForms.PageRequestManager._instance = new Sys.WebForms.PageRequestManager();
    Sys.WebForms.PageRequestManager.getInstance()._initializeInternal(scriptManagerID, formElement);
	logger.warn(`<<< Exiting _initialize(scriptManagerID=${scriptManagerID},formElement=${formElement})`, { "event.action": "_initialize", "logrite.scriptManagerID": scriptManagerID, "logrite.formElement": formElement });
}

Sys.WebForms.PageRequestManager.registerClass('Sys.WebForms.PageRequestManager');
Sys.UI._UpdateProgress = function Sys$UI$_UpdateProgress(element) {
	logger.error(`>>> Entering _UpdateProgress(element=${element})`, { "event.action": "_UpdateProgress", "logrite.element": element });
    Sys.UI._UpdateProgress.initializeBase(this,[element]);
    this._displayAfter = 500;
    this._dynamicLayout = true;
	logger.debug(`_UpdateProgress(element=${element}): this._displayAfter → ${this._displayAfter}`, { "event.action": "_UpdateProgress", "logrite.element": element });
    this._associatedUpdatePanelId = null;
    this._beginRequestHandlerDelegate = null;
	logger.debug(`_UpdateProgress(element=${element}): this._dynamicLayout → ${this._dynamicLayout}`, { "event.action": "_UpdateProgress", "logrite.element": element });
    this._startDelegate = null;
    this._endRequestHandlerDelegate = null;
	logger.debug(`_UpdateProgress(element=${element}): this._associatedUpdatePanelId → ${this._associatedUpdatePanelId}`, { "event.action": "_UpdateProgress", "logrite.element": element });
    this._pageRequestManager = null;
    this._timerCookie = null;
	logger.debug(`_UpdateProgress(element=${element}): this._timerCookie → ${this._timerCookie}`, { "event.action": "_UpdateProgress", "logrite.element": element });
}

    function Sys$UI$_UpdateProgress$get_displayAfter() {
		logger.error('>>> Entering get_displayAfter()', { "event.action": "get_displayAfter" });
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_displayAfter()', { "event.action": "get_displayAfter" });
        return this._displayAfter;
    }
    function Sys$UI$_UpdateProgress$set_displayAfter(value) {
		logger.error(`>>> Entering set_displayAfter(value=${value})`, { "event.action": "set_displayAfter", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;
		logger.error(`set_displayAfter(value=${value}): e → ${e}`, { "event.action": "set_displayAfter", "logrite.value": value });

        this._displayAfter = value;
		logger.debug(`set_displayAfter(value=${value}): this._displayAfter → ${this._displayAfter}`, { "event.action": "set_displayAfter", "logrite.value": value });
    }
    function Sys$UI$_UpdateProgress$get_dynamicLayout() {
		logger.error('>>> Entering get_dynamicLayout()', { "event.action": "get_dynamicLayout" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_dynamicLayout()', { "event.action": "get_dynamicLayout" });
        return this._dynamicLayout;
    }
    function Sys$UI$_UpdateProgress$set_dynamicLayout(value) {
		logger.error(`>>> Entering set_dynamicLayout(value=${value})`, { "event.action": "set_dynamicLayout", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;
		logger.error(`set_dynamicLayout(value=${value}): e → ${e}`, { "event.action": "set_dynamicLayout", "logrite.value": value });

        this._dynamicLayout = value;
		logger.debug(`set_dynamicLayout(value=${value}): this._dynamicLayout → ${this._dynamicLayout}`, { "event.action": "set_dynamicLayout", "logrite.value": value });
    }
    function Sys$UI$_UpdateProgress$get_associatedUpdatePanelId() {
		logger.error('>>> Entering get_associatedUpdatePanelId()', { "event.action": "get_associatedUpdatePanelId" });
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_associatedUpdatePanelId()', { "event.action": "get_associatedUpdatePanelId" });
        return this._associatedUpdatePanelId;
    }
    function Sys$UI$_UpdateProgress$set_associatedUpdatePanelId(value) {
		logger.error(`>>> Entering set_associatedUpdatePanelId(value=${value})`, { "event.action": "set_associatedUpdatePanelId", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;
		logger.error(`set_associatedUpdatePanelId(value=${value}): e → ${e}`, { "event.action": "set_associatedUpdatePanelId", "logrite.value": value });

        this._associatedUpdatePanelId = value;
		logger.debug(`set_associatedUpdatePanelId(value=${value}): this._associatedUpdatePanelId → ${this._associatedUpdatePanelId}`, { "event.action": "set_associatedUpdatePanelId", "logrite.value": value });
    }
    function Sys$UI$_UpdateProgress$_handleBeginRequest(sender, arg) {
		logger.error(`>>> Entering _handleBeginRequest(sender=${sender},arg=${arg})`, { "event.action": "_handleBeginRequest", "logrite.sender": sender, "logrite.arg": arg });
        var curElem = arg.get_postBackElement();
        var showProgress = !this._associatedUpdatePanelId; 
        while (!showProgress && curElem) {
            if (curElem.id && this._associatedUpdatePanelId === curElem.id) {
                showProgress = true; 
            }
            curElem = curElem.parentNode; 
        } 
        if (showProgress) {
            this._timerCookie = window.setTimeout(this._startDelegate, this._displayAfter);
			logger.warn(`<<< Exiting _handleBeginRequest(sender=${sender},arg=${arg})`, { "event.action": "_handleBeginRequest", "logrite.sender": sender, "logrite.arg": arg });
        }
    }
    function Sys$UI$_UpdateProgress$_startRequest() {
        if (this._pageRequestManager.get_isInAsyncPostBack()) {
            if (this._dynamicLayout) this.get_element().style.display = 'block';
            else this.get_element().style.visibility = 'visible';
        }
        this._timerCookie = null;
		logger.debug(`_startRequest(): this._timerCookie → ${this._timerCookie}`, { "event.action": "_startRequest" });
    }
    function Sys$UI$_UpdateProgress$_handleEndRequest(sender, arg) {
		logger.error(`>>> Entering _handleEndRequest(sender=${sender},arg=${arg})`, { "event.action": "_handleEndRequest", "logrite.sender": sender, "logrite.arg": arg });
        if (this._dynamicLayout) this.get_element().style.display = 'none';
        else this.get_element().style.visibility = 'hidden';
        if (this._timerCookie) {
            window.clearTimeout(this._timerCookie);
            this._timerCookie = null;
			logger.warn(`<<< Exiting _handleEndRequest(sender=${sender},arg=${arg})`, { "event.action": "_handleEndRequest", "logrite.sender": sender, "logrite.arg": arg });
			logger.debug(`_handleEndRequest(sender=${sender},arg=${arg}): this._timerCookie → ${this._timerCookie}`, { "event.action": "_handleEndRequest", "logrite.sender": sender, "logrite.arg": arg });
        }
    }
    function Sys$UI$_UpdateProgress$dispose() {
       if (this._pageRequestManager !== null) {
           this._pageRequestManager.remove_beginRequest(this._beginRequestHandlerDelegate);
           this._pageRequestManager.remove_endRequest(this._endRequestHandlerDelegate);
       }
       Sys.UI._UpdateProgress.callBaseMethod(this,"dispose");
    }
    function Sys$UI$_UpdateProgress$initialize() {
		logger.error('>>> Entering initialize()', { "event.action": "initialize" });
        Sys.UI._UpdateProgress.callBaseMethod(this, 'initialize');
    	this._beginRequestHandlerDelegate = Function.createDelegate(this, this._handleBeginRequest);
    	this._endRequestHandlerDelegate = Function.createDelegate(this, this._handleEndRequest);
    	this._startDelegate = Function.createDelegate(this, this._startRequest);
    	if (Sys.WebForms && Sys.WebForms.PageRequestManager) {
           this._pageRequestManager = Sys.WebForms.PageRequestManager.getInstance();
    	}
    	if (this._pageRequestManager !== null ) {
               	    this._pageRequestManager.add_beginRequest(this._beginRequestHandlerDelegate);
    	    this._pageRequestManager.add_endRequest(this._endRequestHandlerDelegate);
			logger.warn('<<< Exiting initialize()', { "event.action": "initialize" });
    	}
    }
Sys.UI._UpdateProgress.prototype = {
    get_displayAfter: Sys$UI$_UpdateProgress$get_displayAfter,
    set_displayAfter: Sys$UI$_UpdateProgress$set_displayAfter,
    get_dynamicLayout: Sys$UI$_UpdateProgress$get_dynamicLayout,
    set_dynamicLayout: Sys$UI$_UpdateProgress$set_dynamicLayout,
    get_associatedUpdatePanelId: Sys$UI$_UpdateProgress$get_associatedUpdatePanelId,
    set_associatedUpdatePanelId: Sys$UI$_UpdateProgress$set_associatedUpdatePanelId,
    _handleBeginRequest: Sys$UI$_UpdateProgress$_handleBeginRequest,
    _startRequest: Sys$UI$_UpdateProgress$_startRequest,
    _handleEndRequest: Sys$UI$_UpdateProgress$_handleEndRequest,
    dispose: Sys$UI$_UpdateProgress$dispose,
    initialize: Sys$UI$_UpdateProgress$initialize
}
Sys.UI._UpdateProgress.registerClass('Sys.UI._UpdateProgress', Sys.UI.Control);

Type.registerNamespace('Sys.WebForms');
Sys.WebForms.Res={
"PRM_MissingPanel":"Could not find UpdatePanel with ID \u0027{0}\u0027. If it is being updated dynamically then it must be inside another UpdatePanel.",
"PRM_ServerError":"An unknown error occurred while processing the request on the server. The status code returned from the server was: {0}",
"PRM_ParserError":"The message received from the server could not be parsed.",
"PRM_TimeoutError":"The server request timed out.",
"PRM_CannotRegisterTwice":"The PageRequestManager cannot be initialized more than once.",
"PRM_UnknownToken":"Unknown token: \u0027{0}\u0027.",
"PRM_MissingPanel":"Could not find UpdatePanel with ID \u0027{0}\u0027. If it is being updated dynamically then it must be inside another UpdatePanel.",
"PRM_ServerError":"An unknown error occurred while processing the request on the server. The status code returned from the server was: {0}",
"PRM_ParserError":"The message received from the server could not be parsed. Common causes for this error are when the response is modified by calls to Response.Write(), response filters, HttpModules, or server trace is enabled.\r\nDetails: {0}",
"PRM_TimeoutError":"The server request timed out.",
"PRM_ParserErrorDetails":"Error parsing near \u0027{0}\u0027.",
"PRM_CannotRegisterTwice":"The PageRequestManager cannot be initialized more than once."
};

if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();