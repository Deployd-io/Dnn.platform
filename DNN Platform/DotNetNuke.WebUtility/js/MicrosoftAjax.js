var logger = (typeof logger !== 'undefined' && logger) || console;
//-----------------------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------
// MicrosoftAjax.js
// Microsoft AJAX Framework.


Function.__typeName = 'Function';
Function.__class = true;

Function.createCallback = function Function$createCallback(method, context) {
	logger.error(`>>> Entering createCallback(method=${method},context=${context})`, { "event.action": "createCallback", "logrite.method": method, "logrite.context": context });
    /// <param name="method" type="Function"></param>
    /// <param name="context" mayBeNull="true"></param>
    /// <returns type="Function"></returns>
    var e = Function._validateParams(arguments, [
        {name: "method", type: Function},
        {name: "context", mayBeNull: true}
    ]);
    if (e) throw e;


        
    return function() {
        var l = arguments.length;
        if (l > 0) {
                        var args = [];
            for (var i = 0; i < l; i++) {
                args[i] = arguments[i];
            }
            args[l] = context;
            return method.apply(this, args);
        }
        return method.call(this, context);
    }
}

Function.createDelegate = function Function$createDelegate(instance, method) {
	logger.error(`>>> Entering createDelegate(instance=${instance},method=${method})`, { "event.action": "createDelegate", "logrite.instance": instance, "logrite.method": method });
    /// <param name="instance" mayBeNull="true"></param>
    /// <param name="method" type="Function"></param>
    /// <returns type="Function"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", mayBeNull: true},
        {name: "method", type: Function}
    ]);
    if (e) throw e;


        
    return function() {
        return method.apply(instance, arguments);
    }
}

Function.emptyFunction = Function.emptyMethod = function Function$emptyMethod() {
	logger.error('>>> Entering emptyMethod()', { "event.action": "emptyMethod" });
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting emptyMethod()', { "event.action": "emptyMethod" });
}

Function._validateParams = function Function$_validateParams(params, expectedParams) {
																																	logger.error(`>>> Entering _validateParams(params=${params},expectedParams=${expectedParams})`, { "event.action": "_validateParams", "logrite.params": params, "logrite.expectedParams": expectedParams });
                                                                                                                                    
    var e;

    e = Function._validateParameterCount(params, expectedParams);
    if (e) {
        e.popStackFrame();
        return e;
    }

    for (var i=0; i < params.length; i++) {
                                var expectedParam = expectedParams[Math.min(i, expectedParams.length - 1)];
								logger.error(`_validateParams(params=${params},expectedParams=${expectedParams}): expectedParam → ${expectedParam}`, { "event.action": "_validateParams", "logrite.params": params, "logrite.expectedParams": expectedParams });

        var paramName = expectedParam.name;
        if (expectedParam.parameterArray) {
                        paramName += "[" + (i - expectedParams.length + 1) + "]";
        }

        e = Function._validateParameter(params[i], expectedParam, paramName);
        if (e) {
			logger.error(`_validateParams(params=${params},expectedParams=${expectedParams}): e → ${e}`, { "event.action": "_validateParams", "logrite.params": params, "logrite.expectedParams": expectedParams });
            e.popStackFrame();
            return e;
        }
    }


	logger.warn(`<<< Exiting _validateParams(params=${params},expectedParams=${expectedParams})`, { "event.action": "_validateParams", "logrite.params": params, "logrite.expectedParams": expectedParams });
    return null;
}

Function._validateParameterCount = function Function$_validateParameterCount(params, expectedParams) {
	logger.error(`>>> Entering _validateParameterCount(params=${params},expectedParams=${expectedParams})`, { "event.action": "_validateParameterCount", "logrite.params": params, "logrite.expectedParams": expectedParams });
    var maxParams = expectedParams.length;
    var minParams = 0;
    for (var i=0; i < expectedParams.length; i++) {
        if (expectedParams[i].parameterArray) {
            maxParams = Number.MAX_VALUE;
        }
        else if (!expectedParams[i].optional) {
            minParams++;
        }
    }

    if (params.length < minParams || params.length > maxParams) {
        var e = Error.parameterCount();
        e.popStackFrame();
        return e;
    }

	logger.warn(`<<< Exiting _validateParameterCount(params=${params},expectedParams=${expectedParams})`, { "event.action": "_validateParameterCount", "logrite.params": params, "logrite.expectedParams": expectedParams });
    return null;
}

Function._validateParameter = function Function$_validateParameter(param, expectedParam, paramName) {
	logger.error(`>>> Entering _validateParameter(param=${param},expectedParam=${expectedParam},paramName=${paramName})`, { "event.action": "_validateParameter", "logrite.param": param, "logrite.expectedParam": expectedParam, "logrite.paramName": paramName });
    var e;

    var expectedType = expectedParam.type;
    var expectedInteger = !!expectedParam.integer;
    var expectedDomElement = !!expectedParam.domElement;
    var mayBeNull = !!expectedParam.mayBeNull;

    e = Function._validateParameterType(param, expectedType, expectedInteger, expectedDomElement, mayBeNull, paramName);
    if (e) {
        e.popStackFrame();
        return e;
    }

        var expectedElementType = expectedParam.elementType;
    var elementMayBeNull = !!expectedParam.elementMayBeNull;
    if (expectedType === Array && typeof(param) !== "undefined" && param !== null &&
        (expectedElementType || !elementMayBeNull)) {
        var expectedElementInteger = !!expectedParam.elementInteger;
        var expectedElementDomElement = !!expectedParam.elementDomElement;
        for (var i=0; i < param.length; i++) {
            var elem = param[i];
            e = Function._validateParameterType(elem, expectedElementType,
                expectedElementInteger, expectedElementDomElement, elementMayBeNull,
                paramName + "[" + i + "]");
				logger.error(`_validateParameter(param=${param},expectedParam=${expectedParam},paramName=${paramName}): e → ${e}`, { "event.action": "_validateParameter", "logrite.param": param, "logrite.expectedParam": expectedParam, "logrite.paramName": paramName });
            if (e) {
                e.popStackFrame();
                return e;
            }
        }
    }

	logger.warn(`<<< Exiting _validateParameter(param=${param},expectedParam=${expectedParam},paramName=${paramName})`, { "event.action": "_validateParameter", "logrite.param": param, "logrite.expectedParam": expectedParam, "logrite.paramName": paramName });
    return null;
}

Function._validateParameterType = function Function$_validateParameterType(param, expectedType, expectedInteger, expectedDomElement, mayBeNull, paramName) {
	logger.error(`>>> Entering _validateParameterType(param=${param},expectedType=${expectedType},expectedInteger=${expectedInteger},expectedDomElement=${expectedDomElement},mayBeNull=${mayBeNull},paramName=${paramName})`, { "event.action": "_validateParameterType", "logrite.param": param, "logrite.expectedType": expectedType, "logrite.expectedInteger": expectedInteger, "logrite.expectedDomElement": expectedDomElement, "logrite.mayBeNull": mayBeNull, "logrite.paramName": paramName });
    var e;

    if (typeof(param) === "undefined") {
        if (mayBeNull) {
            return null;
        }
        else {
            e = Error.argumentUndefined(paramName);
            e.popStackFrame();
            return e;
        }
    }

    if (param === null) {
        if (mayBeNull) {
            return null;
        }
        else {
            e = Error.argumentNull(paramName);
            e.popStackFrame();
            return e;
        }
    }

    if (expectedType && expectedType.__enum) {
        if (typeof(param) !== 'number') {
            e = Error.argumentType(paramName, Object.getType(param), expectedType);
            e.popStackFrame();
            return e;
        }
        if ((param % 1) === 0) {
            var values = expectedType.prototype;
            if (!expectedType.__flags || (param === 0)) {
                for (var i in values) {
                    if (values[i] === param) return null;
                }
            }
            else {
                var v = param;
                for (var i in values) {
                    var vali = values[i];
                    if (vali === 0) continue;
                    if ((vali & param) === vali) {
                        v -= vali;
                    }
                    if (v === 0) return null;
                }
            }
        }
        e = Error.argumentOutOfRange(paramName, param, String.format(Sys.Res.enumInvalidValue, param, expectedType.getName()));
        e.popStackFrame();
        return e;
    }

                    if (expectedDomElement && (param !== window) && (param !== document) &&
        !(window.HTMLElement && (param instanceof HTMLElement)) &&
        (typeof(param.nodeName) !== 'string')) {

        e = Error.argument(paramName, Sys.Res.argumentDomElement);
        e.popStackFrame();
        return e;
    }

        if (expectedType && !expectedType.isInstanceOfType(param)) {
        e = Error.argumentType(paramName, Object.getType(param), expectedType);
        e.popStackFrame();
        return e;
    }

    if (expectedType === Number && expectedInteger) {
                        if ((param % 1) !== 0) {
            e = Error.argumentOutOfRange(paramName, param, Sys.Res.argumentInteger);
            e.popStackFrame();
			logger.error(`_validateParameterType(param=${param},expectedType=${expectedType},expectedInteger=${expectedInteger},expectedDomElement=${expectedDomElement},mayBeNull=${mayBeNull},paramName=${paramName}): e → ${e}`, { "event.action": "_validateParameterType", "logrite.param": param, "logrite.expectedType": expectedType, "logrite.expectedInteger": expectedInteger, "logrite.expectedDomElement": expectedDomElement, "logrite.mayBeNull": mayBeNull, "logrite.paramName": paramName });
            return e;
        }
    }

	logger.warn(`<<< Exiting _validateParameterType(param=${param},expectedType=${expectedType},expectedInteger=${expectedInteger},expectedDomElement=${expectedDomElement},mayBeNull=${mayBeNull},paramName=${paramName})`, { "event.action": "_validateParameterType", "logrite.param": param, "logrite.expectedType": expectedType, "logrite.expectedInteger": expectedInteger, "logrite.expectedDomElement": expectedDomElement, "logrite.mayBeNull": mayBeNull, "logrite.paramName": paramName });
    return null;
}
Error.__typeName = 'Error';
Error.__class = true;

Error.create = function Error$create(message, errorInfo) {
	logger.error(`>>> Entering create(message=${message},errorInfo=${errorInfo})`, { "event.action": "create", "logrite.type": type, "logrite.properties": properties, "logrite.events": events, "logrite.references": references, "logrite.element": element });
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="errorInfo" optional="true" mayBeNull="true"></param>
    /// <returns type="Error"></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true},
        {name: "errorInfo", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


            var e = new Error(message);
			logger.error(`create(message=${message},errorInfo=${errorInfo}): e → ${e}`, { "event.action": "create", "logrite.type": type, "logrite.properties": properties, "logrite.events": events, "logrite.references": references, "logrite.element": element });
    e.message = message;
			logger.debug(`create(message=${message},errorInfo=${errorInfo}): e.message → ${e.message}`, { "event.action": "create", "logrite.type": type, "logrite.properties": properties, "logrite.events": events, "logrite.references": references, "logrite.element": element });

    if (errorInfo) {
        for (var v in errorInfo) {
            e[v] = errorInfo[v];
        }
    }

    e.popStackFrame();
	logger.warn(`<<< Exiting create(message=${message},errorInfo=${errorInfo})`, { "event.action": "create", "logrite.type": type, "logrite.properties": properties, "logrite.events": events, "logrite.references": references, "logrite.element": element });
    return e;
}

Error.argument = function Error$argument(paramName, message) {
	logger.error(`>>> Entering argument(paramName=${paramName},message=${message})`, { "event.action": "argument", "logrite.paramName": paramName, "logrite.message": message });
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentException: " + (message ? message : Sys.Res.argument);
	logger.error(`argument(paramName=${paramName},message=${message}): displayMessage → ${displayMessage}`, { "event.action": "argument", "logrite.paramName": paramName, "logrite.message": message });
    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

    var e = Error.create(displayMessage, { name: "Sys.ArgumentException", paramName: paramName });
	logger.error(`argument(paramName=${paramName},message=${message}): e → ${e}`, { "event.action": "argument", "logrite.paramName": paramName, "logrite.message": message });
    e.popStackFrame();
	logger.warn(`<<< Exiting argument(paramName=${paramName},message=${message})`, { "event.action": "argument", "logrite.paramName": paramName, "logrite.message": message });
    return e;
}

Error.argumentNull = function Error$argumentNull(paramName, message) {
	logger.error(`>>> Entering argumentNull(paramName=${paramName},message=${message})`, { "event.action": "argumentNull", "logrite.paramName": paramName, "logrite.message": message });
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentNullException: " + (message ? message : Sys.Res.argumentNull);
	logger.error(`argumentNull(paramName=${paramName},message=${message}): displayMessage → ${displayMessage}`, { "event.action": "argumentNull", "logrite.paramName": paramName, "logrite.message": message });
    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

    var e = Error.create(displayMessage, { name: "Sys.ArgumentNullException", paramName: paramName });
	logger.error(`argumentNull(paramName=${paramName},message=${message}): e → ${e}`, { "event.action": "argumentNull", "logrite.paramName": paramName, "logrite.message": message });
    e.popStackFrame();
	logger.warn(`<<< Exiting argumentNull(paramName=${paramName},message=${message})`, { "event.action": "argumentNull", "logrite.paramName": paramName, "logrite.message": message });
    return e;
}

Error.argumentOutOfRange = function Error$argumentOutOfRange(paramName, actualValue, message) {
	logger.error(`>>> Entering argumentOutOfRange(paramName=${paramName},actualValue=${actualValue},message=${message})`, { "event.action": "argumentOutOfRange", "logrite.paramName": paramName, "logrite.actualValue": actualValue, "logrite.message": message });
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="actualValue" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "actualValue", mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentOutOfRangeException: " + (message ? message : Sys.Res.argumentOutOfRange);
	logger.error(`argumentOutOfRange(paramName=${paramName},actualValue=${actualValue},message=${message}): displayMessage → ${displayMessage}`, { "event.action": "argumentOutOfRange", "logrite.paramName": paramName, "logrite.actualValue": actualValue, "logrite.message": message });
    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

                if (typeof(actualValue) !== "undefined" && actualValue !== null) {
        displayMessage += "\n" + String.format(Sys.Res.actualValue, actualValue);
    }

    var e = Error.create(displayMessage, {
        name: "Sys.ArgumentOutOfRangeException",
        paramName: paramName,
        actualValue: actualValue
    });
    e.popStackFrame();
	logger.warn(`<<< Exiting argumentOutOfRange(paramName=${paramName},actualValue=${actualValue},message=${message})`, { "event.action": "argumentOutOfRange", "logrite.paramName": paramName, "logrite.actualValue": actualValue, "logrite.message": message });
    return e;
}

Error.argumentType = function Error$argumentType(paramName, actualType, expectedType, message) {
	logger.error(`>>> Entering argumentType(paramName=${paramName},actualType=${actualType},expectedType=${expectedType},message=${message})`, { "event.action": "argumentType", "logrite.paramName": paramName, "logrite.actualType": actualType, "logrite.expectedType": expectedType, "logrite.message": message });
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="actualType" type="Type" optional="true" mayBeNull="true"></param>
    /// <param name="expectedType" type="Type" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "actualType", type: Type, mayBeNull: true, optional: true},
        {name: "expectedType", type: Type, mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentTypeException: ";
	logger.error(`argumentType(paramName=${paramName},actualType=${actualType},expectedType=${expectedType},message=${message}): displayMessage → ${displayMessage}`, { "event.action": "argumentType", "logrite.paramName": paramName, "logrite.actualType": actualType, "logrite.expectedType": expectedType, "logrite.message": message });
    if (message) {
        displayMessage += message;
    }
    else if (actualType && expectedType) {
        displayMessage +=
            String.format(Sys.Res.argumentTypeWithTypes, actualType.getName(), expectedType.getName());
    }
    else {
        displayMessage += Sys.Res.argumentType;
    }

    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

    var e = Error.create(displayMessage, {
        name: "Sys.ArgumentTypeException",
        paramName: paramName,
        actualType: actualType,
        expectedType: expectedType
    });
    e.popStackFrame();
	logger.warn(`<<< Exiting argumentType(paramName=${paramName},actualType=${actualType},expectedType=${expectedType},message=${message})`, { "event.action": "argumentType", "logrite.paramName": paramName, "logrite.actualType": actualType, "logrite.expectedType": expectedType, "logrite.message": message });
    return e;
}

Error.argumentUndefined = function Error$argumentUndefined(paramName, message) {
	logger.error(`>>> Entering argumentUndefined(paramName=${paramName},message=${message})`, { "event.action": "argumentUndefined", "logrite.paramName": paramName, "logrite.message": message });
    /// <param name="paramName" type="String" optional="true" mayBeNull="true"></param>
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "paramName", type: String, mayBeNull: true, optional: true},
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ArgumentUndefinedException: " + (message ? message : Sys.Res.argumentUndefined);
	logger.error(`argumentUndefined(paramName=${paramName},message=${message}): displayMessage → ${displayMessage}`, { "event.action": "argumentUndefined", "logrite.paramName": paramName, "logrite.message": message });
    if (paramName) {
        displayMessage += "\n" + String.format(Sys.Res.paramName, paramName);
    }

    var e = Error.create(displayMessage, { name: "Sys.ArgumentUndefinedException", paramName: paramName });
	logger.error(`argumentUndefined(paramName=${paramName},message=${message}): e → ${e}`, { "event.action": "argumentUndefined", "logrite.paramName": paramName, "logrite.message": message });
    e.popStackFrame();
	logger.warn(`<<< Exiting argumentUndefined(paramName=${paramName},message=${message})`, { "event.action": "argumentUndefined", "logrite.paramName": paramName, "logrite.message": message });
    return e;
}

Error.format = function Error$format(message) {
	logger.error(`>>> Entering format(message=${message})`, { "event.action": "format", "logrite.format": format, "logrite.args": args });
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    var displayMessage = "Sys.FormatException: " + (message ? message : Sys.Res.format);
	logger.error(`format(message=${message}): displayMessage → ${displayMessage}`, { "event.action": "format", "logrite.format": format, "logrite.args": args });
    var e = Error.create(displayMessage, {name: 'Sys.FormatException'});
	logger.error(`format(message=${message}): e → ${e}`, { "event.action": "format", "logrite.format": format, "logrite.args": args });
    e.popStackFrame();
	logger.warn(`<<< Exiting format(message=${message})`, { "event.action": "format", "logrite.format": format, "logrite.args": args });
	logger.warn(`<<< Exiting format(format=${format})`, { "event.action": "format", "logrite.format": format, "logrite.args": args });
    return e;
}

Error.invalidOperation = function Error$invalidOperation(message) {
	logger.error(`>>> Entering invalidOperation(message=${message})`, { "event.action": "invalidOperation", "logrite.message": message });
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    var displayMessage = "Sys.InvalidOperationException: " + (message ? message : Sys.Res.invalidOperation);
	logger.error(`invalidOperation(message=${message}): displayMessage → ${displayMessage}`, { "event.action": "invalidOperation", "logrite.message": message });

    var e = Error.create(displayMessage, {name: 'Sys.InvalidOperationException'});
	logger.error(`invalidOperation(message=${message}): e → ${e}`, { "event.action": "invalidOperation", "logrite.message": message });
    e.popStackFrame();
	logger.warn(`<<< Exiting invalidOperation(message=${message})`, { "event.action": "invalidOperation", "logrite.message": message });
    return e;
}

Error.notImplemented = function Error$notImplemented(message) {
	logger.error(`>>> Entering notImplemented(message=${message})`, { "event.action": "notImplemented", "logrite.message": message });
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    var displayMessage = "Sys.NotImplementedException: " + (message ? message : Sys.Res.notImplemented);
	logger.error(`notImplemented(message=${message}): displayMessage → ${displayMessage}`, { "event.action": "notImplemented", "logrite.message": message });

    var e = Error.create(displayMessage, {name: 'Sys.NotImplementedException'});
	logger.error(`notImplemented(message=${message}): e → ${e}`, { "event.action": "notImplemented", "logrite.message": message });
    e.popStackFrame();
	logger.warn(`<<< Exiting notImplemented(message=${message})`, { "event.action": "notImplemented", "logrite.message": message });
    return e;
}

Error.parameterCount = function Error$parameterCount(message) {
	logger.error(`>>> Entering parameterCount(message=${message})`, { "event.action": "parameterCount", "logrite.message": message });
    /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "message", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;


    var displayMessage = "Sys.ParameterCountException: " + (message ? message : Sys.Res.parameterCount);
	logger.error(`parameterCount(message=${message}): displayMessage → ${displayMessage}`, { "event.action": "parameterCount", "logrite.message": message });
    var e = Error.create(displayMessage, {name: 'Sys.ParameterCountException'});
	logger.error(`parameterCount(message=${message}): e → ${e}`, { "event.action": "parameterCount", "logrite.message": message });
    e.popStackFrame();
	logger.warn(`<<< Exiting parameterCount(message=${message})`, { "event.action": "parameterCount", "logrite.message": message });
    return e;
}

Error.prototype.popStackFrame = function Error$popStackFrame() {
	logger.error('>>> Entering popStackFrame()', { "event.action": "popStackFrame" });
    if (arguments.length !== 0) throw Error.parameterCount();

                            
    if (typeof(this.stack) === "undefined" || this.stack === null ||
        typeof(this.fileName) === "undefined" || this.fileName === null ||
        typeof(this.lineNumber) === "undefined" || this.lineNumber === null) {
        return;
    }

    var stackFrames = this.stack.split("\n");
	logger.error(`popStackFrame(): stackFrames → ${stackFrames}`, { "event.action": "popStackFrame" });

                var currentFrame = stackFrames[0];
    var pattern = this.fileName + ":" + this.lineNumber;
    while(typeof(currentFrame) !== "undefined" &&
          currentFrame !== null &&
          currentFrame.indexOf(pattern) === -1) {
		logger.error(`popStackFrame(): pattern → ${pattern}`, { "event.action": "popStackFrame" });
        stackFrames.shift();
        currentFrame = stackFrames[0];
    }

    var nextFrame = stackFrames[1];

        if (typeof(nextFrame) === "undefined" || nextFrame === null) {
        return;
    }

        var nextFrameParts = nextFrame.match(/@(.*):(\d+)$/);
		logger.error(`popStackFrame(): nextFrameParts → ${nextFrameParts}`, { "event.action": "popStackFrame" });
    if (typeof(nextFrameParts) === "undefined" || nextFrameParts === null) {
        return;
    }

    this.fileName = nextFrameParts[1];

		logger.debug(`popStackFrame(): this.fileName → ${this.fileName}`, { "event.action": "popStackFrame" });
        this.lineNumber = parseInt(nextFrameParts[2]);

    stackFrames.shift();
    this.stack = stackFrames.join("\n");
	logger.warn('<<< Exiting popStackFrame()', { "event.action": "popStackFrame" });
}
if (!window) this.window = this;

window.Type = Function;

window.__rootNamespaces = [];
window.__registeredTypes = {};

Type.__fullyQualifiedIdentifierRegExp = new RegExp("^[^.0-9 \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\]([^ \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\]*[^. \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\])?$", "i");
Type.__identifierRegExp = new RegExp("^[^.0-9 \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\][^. \\s|,;:&*=+\\-()\\[\\]{}^%#@!~\\n\\r\\t\\f\\\\]*$", "i");

Type.prototype.callBaseMethod = function Type$callBaseMethod(instance, name, baseArguments) {
	logger.error(`>>> Entering callBaseMethod(instance=${instance},name=${name},baseArguments=${baseArguments})`, { "event.action": "callBaseMethod", "logrite.instance": instance, "logrite.name": name, "logrite.baseArguments": baseArguments });
    /// <param name="instance"></param>
    /// <param name="name" type="String"></param>
    /// <param name="baseArguments" type="Array" optional="true" mayBeNull="true" elementMayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"},
        {name: "name", type: String},
        {name: "baseArguments", type: Array, mayBeNull: true, optional: true, elementMayBeNull: true}
    ]);
    if (e) throw e;

    var baseMethod = this.getBaseMethod(instance, name);
	logger.error(`callBaseMethod(instance=${instance},name=${name},baseArguments=${baseArguments}): baseMethod → ${baseMethod}`, { "event.action": "callBaseMethod", "logrite.instance": instance, "logrite.name": name, "logrite.baseArguments": baseArguments });
    if (!baseMethod) throw Error.invalidOperation(String.format(Sys.Res.methodNotFound, name));
    if (!baseArguments) {
        return baseMethod.apply(instance);
    }
    else {
		logger.warn(`<<< Exiting callBaseMethod(instance=${instance},name=${name},baseArguments=${baseArguments})`, { "event.action": "callBaseMethod", "logrite.instance": instance, "logrite.name": name, "logrite.baseArguments": baseArguments });
        return baseMethod.apply(instance, baseArguments);
    }
}

Type.prototype.getBaseMethod = function Type$getBaseMethod(instance, name) {
	logger.error(`>>> Entering getBaseMethod(instance=${instance},name=${name})`, { "event.action": "getBaseMethod", "logrite.instance": instance, "logrite.name": name });
    /// <param name="instance"></param>
    /// <param name="name" type="String"></param>
    /// <returns type="Function" mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"},
        {name: "name", type: String}
    ]);
    if (e) throw e;

    if (!this.isInstanceOfType(instance)) throw Error.argumentType('instance', Object.getType(instance), this);
    var baseType = this.getBaseType();
	logger.error(`getBaseMethod(instance=${instance},name=${name}): baseType → ${baseType}`, { "event.action": "getBaseMethod", "logrite.instance": instance, "logrite.name": name });
    if (baseType) {
        var baseMethod = baseType.prototype[name];
        return (baseMethod instanceof Function) ? baseMethod : null;
    }

	logger.warn(`<<< Exiting getBaseMethod(instance=${instance},name=${name})`, { "event.action": "getBaseMethod", "logrite.instance": instance, "logrite.name": name });
    return null;
}

Type.prototype.getBaseType = function Type$getBaseType() {
	logger.error('>>> Entering getBaseType()', { "event.action": "getBaseType" });
    /// <returns type="Type" mayBeNull="true"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting getBaseType()', { "event.action": "getBaseType" });
    return (typeof(this.__baseType) === "undefined") ? null : this.__baseType;
}

Type.prototype.getInterfaces = function Type$getInterfaces() {
	logger.error('>>> Entering getInterfaces()', { "event.action": "getInterfaces" });
    /// <returns type="Array" elementType="Type" mayBeNull="false" elementMayBeNull="false"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    var result = [];
	logger.error(`getInterfaces(): result → ${result}`, { "event.action": "getInterfaces" });
    var type = this;
    while(type) {
        var interfaces = type.__interfaces;
        if (interfaces) {
            for (var i = 0, l = interfaces.length; i < l; i++) {
                var interfaceType = interfaces[i];
                if (!Array.contains(result, interfaceType)) {
                    result[result.length] = interfaceType;
                }
            }
        }
        type = type.__baseType;
    }
	logger.warn('<<< Exiting getInterfaces()', { "event.action": "getInterfaces" });
    return result;
}

Type.prototype.getName = function Type$getName() {
	logger.error('>>> Entering getName()', { "event.action": "getName" });
    /// <returns type="String"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting getName()', { "event.action": "getName" });
    return (typeof(this.__typeName) === "undefined") ? "" : this.__typeName;
}

Type.prototype.implementsInterface = function Type$implementsInterface(interfaceType) {
	logger.error(`>>> Entering implementsInterface(interfaceType=${interfaceType})`, { "event.action": "implementsInterface", "logrite.interfaceType": interfaceType });
    /// <param name="interfaceType" type="Type"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "interfaceType", type: Type}
    ]);
    if (e) throw e;

    this.resolveInheritance();

    var interfaceName = interfaceType.getName();
	logger.error(`implementsInterface(interfaceType=${interfaceType}): interfaceName → ${interfaceName}`, { "event.action": "implementsInterface", "logrite.interfaceType": interfaceType });
    var cache = this.__interfaceCache;
    if (cache) {
        var cacheEntry = cache[interfaceName];
        if (typeof(cacheEntry) !== 'undefined') return cacheEntry;
    }
    else {
        cache = this.__interfaceCache = {};
    }

    var baseType = this;
    while (baseType) {
        var interfaces = baseType.__interfaces;
        if (interfaces) {
            if (Array.indexOf(interfaces, interfaceType) !== -1) {
                return cache[interfaceName] = true;
            }
        }

        baseType = baseType.__baseType;
    }

	logger.warn(`<<< Exiting implementsInterface(interfaceType=${interfaceType})`, { "event.action": "implementsInterface", "logrite.interfaceType": interfaceType });
    return cache[interfaceName] = false;
}

Type.prototype.inheritsFrom = function Type$inheritsFrom(parentType) {
	logger.error(`>>> Entering inheritsFrom(parentType=${parentType})`, { "event.action": "inheritsFrom", "logrite.parentType": parentType });
    /// <param name="parentType" type="Type"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "parentType", type: Type}
    ]);
    if (e) throw e;

    this.resolveInheritance();
    var baseType = this.__baseType;
    while (baseType) {
        if (baseType === parentType) {
            return true;
        }
        baseType = baseType.__baseType;
    }

	logger.warn(`<<< Exiting inheritsFrom(parentType=${parentType})`, { "event.action": "inheritsFrom", "logrite.parentType": parentType });
    return false;
}

Type.prototype.initializeBase = function Type$initializeBase(instance, baseArguments) {
	logger.error(`>>> Entering initializeBase(instance=${instance},baseArguments=${baseArguments})`, { "event.action": "initializeBase", "logrite.instance": instance, "logrite.baseArguments": baseArguments });
    /// <param name="instance"></param>
    /// <param name="baseArguments" type="Array" optional="true" mayBeNull="true" elementMayBeNull="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"},
        {name: "baseArguments", type: Array, mayBeNull: true, optional: true, elementMayBeNull: true}
    ]);
    if (e) throw e;

    if (!this.isInstanceOfType(instance)) throw Error.argumentType('instance', Object.getType(instance), this);

    this.resolveInheritance();
    if (this.__baseType) {
        if (!baseArguments) {
            this.__baseType.apply(instance);
        }
        else {
            this.__baseType.apply(instance, baseArguments);
        }
    }

	logger.warn(`<<< Exiting initializeBase(instance=${instance},baseArguments=${baseArguments})`, { "event.action": "initializeBase", "logrite.instance": instance, "logrite.baseArguments": baseArguments });
    return instance;
}

Type.prototype.isImplementedBy = function Type$isImplementedBy(instance) {
	logger.error(`>>> Entering isImplementedBy(instance=${instance})`, { "event.action": "isImplementedBy", "logrite.instance": instance });
    /// <param name="instance" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", mayBeNull: true}
    ]);
    if (e) throw e;

    if (typeof(instance) === "undefined" || instance === null) return false;

    var instanceType = Object.getType(instance);
	logger.error(`isImplementedBy(instance=${instance}): instanceType → ${instanceType}`, { "event.action": "isImplementedBy", "logrite.instance": instance });
    return !!(instanceType.implementsInterface && instanceType.implementsInterface(this));
}

Type.prototype.isInstanceOfType = function Type$isInstanceOfType(instance) {
	logger.error(`>>> Entering isInstanceOfType(instance=${instance})`, { "event.action": "isInstanceOfType", "logrite.instance": instance });
    /// <param name="instance" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", mayBeNull: true}
    ]);
    if (e) throw e;

    if (typeof(instance) === "undefined" || instance === null) return false;

    if (instance instanceof this) return true;

    var instanceType = Object.getType(instance);
	logger.error(`isInstanceOfType(instance=${instance}): instanceType → ${instanceType}`, { "event.action": "isInstanceOfType", "logrite.instance": instance });
    return !!(instanceType === this) ||
           (instanceType.inheritsFrom && instanceType.inheritsFrom(this)) ||
           (instanceType.implementsInterface && instanceType.implementsInterface(this));
}

Type.prototype.registerClass = function Type$registerClass(typeName, baseType, interfaceTypes) {
	logger.error(`>>> Entering registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes})`, { "event.action": "registerClass", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
    /// <param name="typeName" type="String"></param>
    /// <param name="baseType" type="Type" optional="true" mayBeNull="true"></param>
    /// <param name="interfaceTypes" parameterArray="true" type="Type"></param>
    /// <returns type="Type"></returns>
    var e = Function._validateParams(arguments, [
        {name: "typeName", type: String},
        {name: "baseType", type: Type, mayBeNull: true, optional: true},
        {name: "interfaceTypes", type: Type, parameterArray: true}
    ]);
    if (e) throw e;

    if (!Type.__fullyQualifiedIdentifierRegExp.test(typeName)) throw Error.argument('typeName', Sys.Res.notATypeName);
        var parsedName;
    try {
        parsedName = eval(typeName);
    }
    catch(e) {
		logger.error(`Exception in registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes}): ${e.message}`, e, { "event.action": "registerClass", "event.category": "process", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
		logger.error(`registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes}): parsedName → ${parsedName}`, { "event.action": "registerClass", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
        throw Error.argument('typeName', Sys.Res.argumentTypeName);
    }
    if (parsedName !== this) throw Error.argument('typeName', Sys.Res.badTypeName);
		logger.debug(`registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes}): parsedName → ${parsedName}`, { "event.action": "registerClass", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
        if (window.__registeredTypes[typeName]) throw Error.invalidOperation(String.format(Sys.Res.typeRegisteredTwice, typeName));

            if ((arguments.length > 1) && (typeof(baseType) === 'undefined')) throw Error.argumentUndefined('baseType');
    if (baseType && !baseType.__class) throw Error.argument('baseType', Sys.Res.baseNotAClass);

    this.prototype.constructor = this;
    this.__typeName = typeName;
    this.__class = true;
	logger.debug(`registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes}): this.__typeName → ${this.__typeName}`, { "event.action": "registerClass", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
    if (baseType) {
        this.__baseType = baseType;
		logger.debug(`registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes}): this.__class → ${this.__class}`, { "event.action": "registerClass", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
        this.__basePrototypePending = true;
    }
        if (!window.__classes) window.__classes = {};
		logger.debug(`registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes}): this.__baseType → ${this.__baseType}`, { "event.action": "registerClass", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
		logger.debug(`registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes}): this.__basePrototypePending → ${this.__basePrototypePending}`, { "event.action": "registerClass", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
    window.__classes[typeName.toUpperCase()] = this;

                if (interfaceTypes) {
        this.__interfaces = [];
        for (var i = 2; i < arguments.length; i++) {
			logger.debug(`registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes}): this.__interfaces → ${this.__interfaces}`, { "event.action": "registerClass", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
            var interfaceType = arguments[i];
            if (!interfaceType.__interface) throw Error.argument('interfaceTypes[' + (i - 2) + ']', Sys.Res.notAnInterface);
            this.resolveInheritance();
            for (var methodName in interfaceType.prototype) {
                var method = interfaceType.prototype[methodName];
                if (!this.prototype[methodName]) {
                    this.prototype[methodName] = method;
                }
            }
            this.__interfaces.push(interfaceType);
        }
    }
    window.__registeredTypes[typeName] = true;

	logger.warn(`<<< Exiting registerClass(typeName=${typeName},baseType=${baseType},interfaceTypes=${interfaceTypes})`, { "event.action": "registerClass", "logrite.typeName": typeName, "logrite.baseType": baseType, "logrite.interfaceTypes": interfaceTypes });
    return this;
}

Type.prototype.registerInterface = function Type$registerInterface(typeName) {
	logger.error(`>>> Entering registerInterface(typeName=${typeName})`, { "event.action": "registerInterface", "logrite.typeName": typeName });
    /// <param name="typeName" type="String"></param>
    /// <returns type="Type"></returns>
    var e = Function._validateParams(arguments, [
        {name: "typeName", type: String}
    ]);
    if (e) throw e;

    if (!Type.__fullyQualifiedIdentifierRegExp.test(typeName)) throw Error.argument('typeName', Sys.Res.notATypeName);
        var parsedName;
    try {
        parsedName = eval(typeName);
    }
    catch(e) {
		logger.error(`Exception in registerInterface(typeName=${typeName}): ${e.message}`, e, { "event.action": "registerInterface", "event.category": "process", "logrite.typeName": typeName });
		logger.error(`registerInterface(typeName=${typeName}): parsedName → ${parsedName}`, { "event.action": "registerInterface", "logrite.typeName": typeName });
        throw Error.argument('typeName', Sys.Res.argumentTypeName);
    }
    if (parsedName !== this) throw Error.argument('typeName', Sys.Res.badTypeName);
		logger.debug(`registerInterface(typeName=${typeName}): parsedName → ${parsedName}`, { "event.action": "registerInterface", "logrite.typeName": typeName });
        if (window.__registeredTypes[typeName]) throw Error.invalidOperation(String.format(Sys.Res.typeRegisteredTwice, typeName));
    this.prototype.constructor = this;
    this.__typeName = typeName;
    this.__interface = true;
	logger.debug(`registerInterface(typeName=${typeName}): this.__typeName → ${this.__typeName}`, { "event.action": "registerInterface", "logrite.typeName": typeName });
    window.__registeredTypes[typeName] = true;

	logger.debug(`registerInterface(typeName=${typeName}): this.__interface → ${this.__interface}`, { "event.action": "registerInterface", "logrite.typeName": typeName });
	logger.warn(`<<< Exiting registerInterface(typeName=${typeName})`, { "event.action": "registerInterface", "logrite.typeName": typeName });
    return this;
}

Type.prototype.resolveInheritance = function Type$resolveInheritance() {
	logger.error('>>> Entering resolveInheritance()', { "event.action": "resolveInheritance" });
    if (arguments.length !== 0) throw Error.parameterCount();

    if (this.__basePrototypePending) {
        var baseType = this.__baseType;

        baseType.resolveInheritance();

        for (var memberName in baseType.prototype) {
            var memberValue = baseType.prototype[memberName];
            if (!this.prototype[memberName]) {
                this.prototype[memberName] = memberValue;
            }
        }
        delete this.__basePrototypePending;
		logger.warn('<<< Exiting resolveInheritance()', { "event.action": "resolveInheritance" });
    }
}

Type.getRootNamespaces = function Type$getRootNamespaces() {
	logger.error('>>> Entering getRootNamespaces()', { "event.action": "getRootNamespaces" });
    /// <returns type="Array"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting getRootNamespaces()', { "event.action": "getRootNamespaces" });
    return Array.clone(window.__rootNamespaces);
}

Type.isClass = function Type$isClass(type) {
	logger.error(`>>> Entering isClass(type=${type})`, { "event.action": "isClass", "logrite.type": type });
    /// <param name="type" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(type) === 'undefined') || (type === null)) return false;
	logger.warn(`<<< Exiting isClass(type=${type})`, { "event.action": "isClass", "logrite.type": type });
    return !!type.__class;
}

Type.isInterface = function Type$isInterface(type) {
	logger.error(`>>> Entering isInterface(type=${type})`, { "event.action": "isInterface", "logrite.type": type });
    /// <param name="type" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(type) === 'undefined') || (type === null)) return false;
	logger.warn(`<<< Exiting isInterface(type=${type})`, { "event.action": "isInterface", "logrite.type": type });
    return !!type.__interface;
}

Type.isNamespace = function Type$isNamespace(object) {
	logger.error(`>>> Entering isNamespace(object=${object})`, { "event.action": "isNamespace", "logrite.object": object });
    /// <param name="object" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "object", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(object) === 'undefined') || (object === null)) return false;
	logger.warn(`<<< Exiting isNamespace(object=${object})`, { "event.action": "isNamespace", "logrite.object": object });
    return !!object.__namespace;
}

Type.parse = function Type$parse(typeName, ns) {
	logger.error(`>>> Entering parse(typeName=${typeName},ns=${ns})`, { "event.action": "parse", "logrite.value": value, "logrite.ignoreCase": ignoreCase });
    /// <param name="typeName" type="String" mayBeNull="true"></param>
    /// <param name="ns" optional="true" mayBeNull="true"></param>
    /// <returns type="Type" mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "typeName", type: String, mayBeNull: true},
        {name: "ns", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    var fn;
    if (ns) {
        if (!window.__classes) return null;
        fn = window.__classes[ns.getName().toUpperCase() + '.' + typeName.toUpperCase()];
        return fn || null;
    }
    if (!typeName) return null;
    if (!Type.__htClasses) {
        Type.__htClasses = {};
    }
    fn = Type.__htClasses[typeName];
    if (!fn) {
        fn = eval(typeName);
        if (typeof(fn) !== 'function') throw Error.argument('typeName', Sys.Res.notATypeName);
		logger.error(`parse(typeName=${typeName},ns=${ns}): fn → ${fn}`, { "event.action": "parse", "logrite.value": value, "logrite.ignoreCase": ignoreCase });
        Type.__htClasses[typeName] = fn;
    }
	logger.warn(`<<< Exiting parse(typeName=${typeName},ns=${ns})`, { "event.action": "parse", "logrite.value": value, "logrite.ignoreCase": ignoreCase });
    return fn;
}

Type.registerNamespace = function Type$registerNamespace(namespacePath) {
	logger.error(`>>> Entering registerNamespace(namespacePath=${namespacePath})`, { "event.action": "registerNamespace", "logrite.namespacePath": namespacePath });
    /// <param name="namespacePath" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "namespacePath", type: String}
    ]);
    if (e) throw e;

    if (!Type.__fullyQualifiedIdentifierRegExp.test(namespacePath)) throw Error.argument('namespacePath', Sys.Res.invalidNameSpace);
    var rootObject = window;
    var namespaceParts = namespacePath.split('.');
	logger.error(`registerNamespace(namespacePath=${namespacePath}): namespaceParts → ${namespaceParts}`, { "event.action": "registerNamespace", "logrite.namespacePath": namespacePath });

    for (var i = 0; i < namespaceParts.length; i++) {
        var currentPart = namespaceParts[i];
        var ns = rootObject[currentPart];
		logger.debug(`registerNamespace(namespacePath=${namespacePath}): ns.__namespace → ${ns.__namespace}`, { "event.action": "registerNamespace", "logrite.namespacePath": namespacePath });
        if (ns && !ns.__namespace) {
            throw Error.invalidOperation(String.format(Sys.Res.namespaceContainsObject, namespaceParts.splice(0, i + 1).join('.')));
        }
        if (!ns) {
            ns = rootObject[currentPart] = {};
            if (i === 0) {
                window.__rootNamespaces[window.__rootNamespaces.length] = ns;
            }
            ns.__namespace = true;
            ns.__typeName = namespaceParts.slice(0, i + 1).join('.');
            var parsedName;
            try {
                parsedName = eval(ns.__typeName);
            }
            catch(e) {
				logger.error(`Exception in registerNamespace(namespacePath=${namespacePath}): ${e.message}`, e, { "event.action": "registerNamespace", "event.category": "process", "logrite.namespacePath": namespacePath });
                parsedName = null;
            }
            if (parsedName !== ns) {
				logger.debug(`registerNamespace(namespacePath=${namespacePath}): parsedName → ${parsedName}`, { "event.action": "registerNamespace", "logrite.namespacePath": namespacePath });
                delete rootObject[currentPart];
                throw Error.argument('namespacePath', Sys.Res.invalidNameSpace);
            }
            ns.getName = function ns$getName() {return this.__typeName;}
        }
        rootObject = ns;
		logger.warn(`<<< Exiting registerNamespace(namespacePath=${namespacePath})`, { "event.action": "registerNamespace", "logrite.namespacePath": namespacePath });
    }
}
Object.__typeName = 'Object';
Object.__class = true;

Object.getType = function Object$getType(instance) {
	logger.error(`>>> Entering getType(instance=${instance})`, { "event.action": "getType", "logrite.instance": instance });
    /// <param name="instance"></param>
    /// <returns type="Type"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"}
    ]);
    if (e) throw e;

    var ctor = instance.constructor;
    if (!ctor || (typeof(ctor) !== "function") || !ctor.__typeName || (ctor.__typeName === 'Object')) {
        return Object;
    }
	logger.warn(`<<< Exiting getType(instance=${instance})`, { "event.action": "getType", "logrite.instance": instance });
    return ctor;
}

Object.getTypeName = function Object$getTypeName(instance) {
	logger.error(`>>> Entering getTypeName(instance=${instance})`, { "event.action": "getTypeName", "logrite.instance": instance });
    /// <param name="instance"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance"}
    ]);
    if (e) throw e;

	logger.warn(`<<< Exiting getTypeName(instance=${instance})`, { "event.action": "getTypeName", "logrite.instance": instance });
    return Object.getType(instance).getName();
}
Boolean.__typeName = 'Boolean';
Boolean.__class = true;

Boolean.parse = function Boolean$parse(value) {
    /// <param name="value" type="String"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String}
    ]);
    if (e) throw e;

    var v = value.trim().toLowerCase();
    if (v === 'false') return false;
    if (v === 'true') return true;
    throw Error.argumentOutOfRange('value', value, Sys.Res.boolTrueOrFalse);
}
Date.__typeName = 'Date';
Date.__class = true;

Date._appendPreOrPostMatch = function Date$_appendPreOrPostMatch(preMatch, strBuilder) {
				logger.error(`>>> Entering _appendPreOrPostMatch(preMatch=${preMatch},strBuilder=${strBuilder})`, { "event.action": "_appendPreOrPostMatch", "logrite.preMatch": preMatch, "logrite.strBuilder": strBuilder });
                var quoteCount = 0;
    var escaped = false;
    for (var i = 0, il = preMatch.length; i < il; i++) {
        var c = preMatch.charAt(i);
        switch (c) {
        case '\'':
            if (escaped) strBuilder.append("'");
            else quoteCount++;
            escaped = false;
            break;
        case '\\':
            if (escaped) strBuilder.append("\\");
            escaped = !escaped;
            break;
        default:
            strBuilder.append(c);
            escaped = false;
            break;
        }
    }
	logger.error(`_appendPreOrPostMatch(preMatch=${preMatch},strBuilder=${strBuilder}): c → ${c}`, { "event.action": "_appendPreOrPostMatch", "logrite.preMatch": preMatch, "logrite.strBuilder": strBuilder });
    return quoteCount;
}

Date._expandFormat = function Date$_expandFormat(dtf, format) {
		logger.error(`>>> Entering _expandFormat(dtf=${dtf},format=${format})`, { "event.action": "_expandFormat", "logrite.dtf": dtf, "logrite.format": format });
        if (!format) {
		logger.warn(`<<< Exiting _expandFormat(dtf=${dtf},format=${format})`, { "event.action": "_expandFormat", "logrite.dtf": dtf, "logrite.format": format });
        format = "F";
    }
    if (format.length === 1) {
        switch (format) {
        case "d":
            return dtf.ShortDatePattern;
        case "D":
            return dtf.LongDatePattern;
        case "t":
            return dtf.ShortTimePattern;
        case "T":
            return dtf.LongTimePattern;
        case "F":
            return dtf.FullDateTimePattern;
        case "M": case "m":
            return dtf.MonthDayPattern;
        case "s":
            return dtf.SortableDateTimePattern;
        case "Y": case "y":
            return dtf.YearMonthPattern;
        default:
            throw Error.format(Sys.Res.formatInvalidString);
        }
    }
    return format;
}

Date._expandYear = function Date$_expandYear(dtf, year) {
		logger.error(`>>> Entering _expandYear(dtf=${dtf},year=${year})`, { "event.action": "_expandYear", "logrite.dtf": dtf, "logrite.year": year });
        if (year < 100) {
        var curr = new Date().getFullYear();
		logger.error(`_expandYear(dtf=${dtf},year=${year}): curr → ${curr}`, { "event.action": "_expandYear", "logrite.dtf": dtf, "logrite.year": year });
        year += curr - (curr % 100);
        if (year > dtf.Calendar.TwoDigitYearMax) {
		logger.warn(`<<< Exiting _expandYear(dtf=${dtf},year=${year})`, { "event.action": "_expandYear", "logrite.dtf": dtf, "logrite.year": year });
            return year - 100;
        }
    }
    return year;
}

Date._getParseRegExp = function Date$_getParseRegExp(dtf, format) {
				logger.error(`>>> Entering _getParseRegExp(dtf=${dtf},format=${format})`, { "event.action": "_getParseRegExp", "logrite.dtf": dtf, "logrite.format": format });
				logger.warn(`<<< Exiting _getParseRegExp(dtf=${dtf},format=${format})`, { "event.action": "_getParseRegExp", "logrite.dtf": dtf, "logrite.format": format });
                if (!dtf._parseRegExp) {
        dtf._parseRegExp = {};
    }
    else if (dtf._parseRegExp[format]) {
        return dtf._parseRegExp[format];
    }

        var expFormat = Date._expandFormat(dtf, format);
    expFormat = expFormat.replace(/([\^\$\.\*\+\?\|\[\]\(\)\{\}])/g, "\\\\$1");

    var regexp = new Sys.StringBuilder("^");
    var groups = [];
    var index = 0;
    var quoteCount = 0;
    var tokenRegExp = Date._getTokenRegExp();
    var match;

        while ((match = tokenRegExp.exec(expFormat)) !== null) {
        var preMatch = expFormat.slice(index, match.index);
        index = tokenRegExp.lastIndex;

                quoteCount += Date._appendPreOrPostMatch(preMatch, regexp);
        if ((quoteCount%2) === 1) {
            regexp.append(match[0]);
            continue;
        }

                switch (match[0]) {
            case 'dddd': case 'ddd':
            case 'MMMM': case 'MMM':
                regexp.append("(\\D+)");
                break;
            case 'tt': case 't':
                regexp.append("(\\D*)");
                break;
            case 'yyyy':
                regexp.append("(\\d{4})");
                break;
            case 'fff':
                regexp.append("(\\d{3})");
                break;
            case 'ff':
                regexp.append("(\\d{2})");
                break;
            case 'f':
                regexp.append("(\\d)");
                break;
            case 'dd': case 'd':
            case 'MM': case 'M':
            case 'yy': case 'y':
            case 'HH': case 'H':
            case 'hh': case 'h':
            case 'mm': case 'm':
            case 'ss': case 's':
                regexp.append("(\\d\\d?)");
                break;
            case 'zzz':
                regexp.append("([+-]?\\d\\d?:\\d{2})");
                break;
            case 'zz': case 'z':
                regexp.append("([+-]?\\d\\d?)");
                break;
        }
        Array.add(groups, match[0]);
    }
    Date._appendPreOrPostMatch(expFormat.slice(index), regexp);
    regexp.append("$");
        var regexpStr = regexp.toString().replace(/\s+/g, "\\s+");
    var parseRegExp = {'regExp': regexpStr, 'groups': groups};
        dtf._parseRegExp[format] = parseRegExp;
    return parseRegExp;
}

Date._getTokenRegExp = function Date$_getTokenRegExp() {
		logger.error('>>> Entering _getTokenRegExp()', { "event.action": "_getTokenRegExp" });
		logger.warn('<<< Exiting _getTokenRegExp()', { "event.action": "_getTokenRegExp" });
        return /dddd|ddd|dd|d|MMMM|MMM|MM|M|yyyy|yy|y|hh|h|HH|H|mm|m|ss|s|tt|t|fff|ff|f|zzz|zz|z/g;
}

Date.parseLocale = function Date$parseLocale(value, formats) {
	logger.error(`>>> Entering parseLocale(value=${value},formats=${formats})`, { "event.action": "parseLocale", "logrite.value": value });
    /// <param name="value" type="String"></param>
    /// <param name="formats" parameterArray="true" optional="true" mayBeNull="true"></param>
    /// <returns type="Date"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String},
        {name: "formats", mayBeNull: true, optional: true, parameterArray: true}
    ]);
    if (e) throw e;

	logger.warn(`<<< Exiting parseLocale(value=${value},formats=${formats})`, { "event.action": "parseLocale", "logrite.value": value });
	logger.warn(`<<< Exiting parseLocale(value=${value})`, { "event.action": "parseLocale", "logrite.value": value });
    return Date._parse(value, Sys.CultureInfo.CurrentCulture, arguments);
}

Date.parseInvariant = function Date$parseInvariant(value, formats) {
	logger.error(`>>> Entering parseInvariant(value=${value},formats=${formats})`, { "event.action": "parseInvariant", "logrite.value": value });
    /// <param name="value" type="String"></param>
    /// <param name="formats" parameterArray="true" optional="true" mayBeNull="true"></param>
    /// <returns type="Date"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String},
        {name: "formats", mayBeNull: true, optional: true, parameterArray: true}
    ]);
    if (e) throw e;

	logger.warn(`<<< Exiting parseInvariant(value=${value},formats=${formats})`, { "event.action": "parseInvariant", "logrite.value": value });
	logger.warn(`<<< Exiting parseInvariant(value=${value})`, { "event.action": "parseInvariant", "logrite.value": value });
    return Date._parse(value, Sys.CultureInfo.InvariantCulture, arguments);
}

Date._parse = function Date$_parse(value, cultureInfo, args) {
			logger.error(`>>> Entering _parse(value=${value},cultureInfo=${cultureInfo},args=${args})`, { "event.action": "_parse", "logrite.value": value });
			logger.warn(`<<< Exiting _parse(value=${value},cultureInfo=${cultureInfo})`, { "event.action": "_parse", "logrite.value": value });
            var custom = false;
    for (var i = 1, il = args.length; i < il; i++) {
        var format = args[i];
        if (format) {
            custom = true;
            var date = Date._parseExact(value, format, cultureInfo);
            if (date) return date;
        }
    }
        if (! custom) {
        var formats = cultureInfo._getDateTimeFormats();
        for (var i = 0, il = formats.length; i < il; i++) {
            var date = Date._parseExact(value, formats[i], cultureInfo);
			logger.error(`_parse(value=${value},cultureInfo=${cultureInfo},args=${args}): date → ${date}`, { "event.action": "_parse", "logrite.value": value });
            if (date) return date;
        }
    }
	logger.error(`_parse(value=${value},cultureInfo=${cultureInfo},args=${args}): formats → ${formats}`, { "event.action": "_parse", "logrite.value": value });
    return null;
}

Date._parseExact = function Date$_parseExact(value, format, cultureInfo) {
			logger.error(`>>> Entering _parseExact(value=${value},format=${format},cultureInfo=${cultureInfo})`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
			logger.warn(`<<< Exiting _parseExact(value=${value},format=${format},cultureInfo=${cultureInfo})`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
            value = value.trim();
    var dtf = cultureInfo.dateTimeFormat;
	logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): value → ${value}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });

            var parseInfo = Date._getParseRegExp(dtf, format);
			logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): parseInfo → ${parseInfo}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
    var match = new RegExp(parseInfo.regExp).exec(value);
		logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): match → ${match}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
        if (match !== null) {
        var groups = parseInfo.groups;
        var year = null, month = null, date = null, weekDay = null;
        var hour = 0, min = 0, sec = 0, msec = 0, tzMinOffset = null;
        var pmHour = false;
                for (var j = 0, jl = groups.length; j < jl; j++) {
            var matchGroup = match[j+1];
            if (matchGroup) {
                switch (groups[j]) {
                    case 'dd': case 'd':
                                                date = Date._parseInt(matchGroup);
                                                if ((date < 1) || (date > 31)) return null;
                        break;
                    case 'MMMM':
                                                month = cultureInfo._getMonthIndex(matchGroup);
                        if ((month < 0) || (month > 11)) return null;
                        break;
                    case 'MMM':
                                                month = cultureInfo._getAbbrMonthIndex(matchGroup);
                        if ((month < 0) || (month > 11)) return null;
                        break;
                    case 'M': case 'MM':
                                                var month = Date._parseInt(matchGroup) - 1;
												logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): month → ${month}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
                        if ((month < 0) || (month > 11)) return null;
                        break;
                    case 'y': case 'yy':
                                                year = Date._expandYear(dtf,Date._parseInt(matchGroup));
												logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): year → ${year}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
                        if ((year < 0) || (year > 9999)) return null;
                        break;
                    case 'yyyy':
                                                year = Date._parseInt(matchGroup);
                        if ((year < 0) || (year > 9999)) return null;
                        break;
                    case 'h': case 'hh':
                                                hour = Date._parseInt(matchGroup);
												logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): hour → ${hour}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
                        if (hour === 12) hour = 0;
                        if ((hour < 0) || (hour > 11)) return null;
                        break;
                    case 'H': case 'HH':
                                                hour = Date._parseInt(matchGroup);
                        if ((hour < 0) || (hour > 23)) return null;
                        break;
                    case 'm': case 'mm':
                                                min = Date._parseInt(matchGroup);
                        if ((min < 0) || (min > 59)) return null;
                        break;
                    case 's': case 'ss':
                                                sec = Date._parseInt(matchGroup);
                        if ((sec < 0) || (sec > 59)) return null;
                        break;
                    case 'tt': case 't':
                                                var upperToken = matchGroup.toUpperCase();
                        pmHour = (upperToken === dtf.PMDesignator.toUpperCase());
												logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): pmHour → ${pmHour}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
                        if (!pmHour && (upperToken !== dtf.AMDesignator.toUpperCase())) return null;
                        break;
                    case 'f':
                                                msec = Date._parseInt(matchGroup) * 100;
                        if ((msec < 0) || (msec > 999)) return null;
                        break;
                    case 'ff':
                                                msec = Date._parseInt(matchGroup) * 10;
                        if ((msec < 0) || (msec > 999)) return null;
                        break;
                    case 'fff':
                                                msec = Date._parseInt(matchGroup);
                        if ((msec < 0) || (msec > 999)) return null;
                        break;
                    case 'dddd':
                                                weekDay = cultureInfo._getDayIndex(matchGroup);
                        if ((weekDay < 0) || (weekDay > 6)) return null;
                        break;
                    case 'ddd':
                                                weekDay = cultureInfo._getAbbrDayIndex(matchGroup);
                        if ((weekDay < 0) || (weekDay > 6)) return null;
                        break;
                    case 'zzz':
                                                var offsets = matchGroup.split(/:/);
												logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): offsets → ${offsets}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
                        if (offsets.length !== 2) return null;
                        var hourOffset = Date._parseInt(offsets[0]);
                        if ((hourOffset < -12) || (hourOffset > 13)) return null;
                        var minOffset = Date._parseInt(offsets[1]);
						logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): minOffset → ${minOffset}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
                        if ((minOffset < 0) || (minOffset > 59)) return null;
                        tzMinOffset = (hourOffset * 60) + (matchGroup.startsWith('-')? -minOffset : minOffset);
                        break;
                    case 'z': case 'zz':
                                                var hourOffset = Date._parseInt(matchGroup);
												logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): hourOffset → ${hourOffset}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
                        if ((hourOffset < -12) || (hourOffset > 13)) return null;
                        tzMinOffset = hourOffset * 60;
                        break;
                }
            }
        }
        var result = new Date();
		logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): result → ${result}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
        if (year === null) {
            year = result.getFullYear();
        }
        if (month === null) {
            month = result.getMonth();
        }
        if (date === null) {
            date = result.getDate();
        }
                result.setFullYear(year, month, date);
                if (result.getDate() !== date) return null;
                if ((weekDay !== null) && (result.getDay() !== weekDay)) {
            return null;
        }
                if (pmHour && (hour < 12)) {
            hour += 12;
        }
        result.setHours(hour, min, sec, msec);
        if (tzMinOffset !== null) {
                        var adjustedMin = result.getMinutes() - (tzMinOffset + result.getTimezoneOffset());
												logger.error(`_parseExact(value=${value},format=${format},cultureInfo=${cultureInfo}): adjustedMin → ${adjustedMin}`, { "event.action": "_parseExact", "logrite.value": value, "logrite.format": format, "logrite.cultureInfo": cultureInfo });
                                                result.setHours(result.getHours() + parseInt(adjustedMin/60), adjustedMin%60);
        }
        return result;
    }
}

Date._parseInt = function Date$_parseInt(value) {
		logger.error(`>>> Entering _parseInt(value=${value})`, { "event.action": "_parseInt", "logrite.value": value });
		logger.warn(`<<< Exiting _parseInt(value=${value})`, { "event.action": "_parseInt", "logrite.value": value });
        return parseInt(value.replace(/^[\s0]+(\d+)$/,"$1"));
}

Date.prototype.format = function Date$format(format) {
    /// <param name="format" type="String"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String}
    ]);
    if (e) throw e;

    return this._toFormattedString(format, Sys.CultureInfo.InvariantCulture);
}

Date.prototype.localeFormat = function Date$localeFormat(format) {
	logger.error(`>>> Entering localeFormat(format=${format})`, { "event.action": "localeFormat", "logrite.format": format, "logrite.args": args });
    /// <param name="format" type="String"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String}
    ]);
    if (e) throw e;

	logger.warn(`<<< Exiting localeFormat(format=${format})`, { "event.action": "localeFormat", "logrite.format": format, "logrite.args": args });
	logger.warn(`<<< Exiting localeFormat(format=${format},args=${args})`, { "event.action": "localeFormat", "logrite.format": format, "logrite.args": args });
    return this._toFormattedString(format, Sys.CultureInfo.CurrentCulture);
}

Date.prototype._toFormattedString = function Date$_toFormattedString(format, cultureInfo) {
	logger.error(`>>> Entering _toFormattedString(format=${format},cultureInfo=${cultureInfo})`, { "event.action": "_toFormattedString", "logrite.useLocale": useLocale, "logrite.args": args });
    if (!format || (format.length === 0) || (format === 'i')) {
        if (cultureInfo && (cultureInfo.name.length > 0)) {
            return this.toLocaleString();
        }
        else {
            return this.toString();
        }
    }

    var dtf = cultureInfo.dateTimeFormat;
    format = Date._expandFormat(dtf, format);

        var ret = new Sys.StringBuilder();
		logger.error(`_toFormattedString(format=${format},cultureInfo=${cultureInfo}): ret → ${ret}`, { "event.action": "_toFormattedString", "logrite.useLocale": useLocale, "logrite.args": args });
    var hour;

    function addLeadingZero(num) {
		logger.error(`>>> Entering addLeadingZero(num=${num})`, { "event.action": "addLeadingZero", "logrite.num": num });
        if (num < 10) {
            return '0' + num;
        }
		logger.warn(`<<< Exiting addLeadingZero(num=${num})`, { "event.action": "addLeadingZero", "logrite.num": num });
        return num.toString();
    }

    function addLeadingZeros(num) {
		logger.error(`>>> Entering addLeadingZeros(num=${num})`, { "event.action": "addLeadingZeros", "logrite.num": num });
        if (num < 10) {
            return '00' + num;
        }
        if (num < 100) {
            return '0' + num;
        }
		logger.warn(`<<< Exiting addLeadingZeros(num=${num})`, { "event.action": "addLeadingZeros", "logrite.num": num });
        return num.toString();
    }

    var quoteCount = 0;
    var tokenRegExp = Date._getTokenRegExp();
    for (;;) {

                var index = tokenRegExp.lastIndex;

                var ar = tokenRegExp.exec(format);

                var preMatch = format.slice(index, ar ? ar.index : format.length);
        quoteCount += Date._appendPreOrPostMatch(preMatch, ret);

        if (!ar) break;

                if ((quoteCount%2) === 1) {
            ret.append(ar[0]);
            continue;
        }

        switch (ar[0]) {
        case "dddd":
                        ret.append(dtf.DayNames[this.getDay()]);
            break;
        case "ddd":
                        ret.append(dtf.AbbreviatedDayNames[this.getDay()]);
            break;
        case "dd":
                        ret.append(addLeadingZero(this.getDate()));
            break;
        case "d":
                        ret.append(this.getDate());
            break;
        case "MMMM":
                        ret.append(dtf.MonthNames[this.getMonth()]);
            break;
        case "MMM":
                        ret.append(dtf.AbbreviatedMonthNames[this.getMonth()]);
            break;
        case "MM":
                        ret.append(addLeadingZero(this.getMonth() + 1));
            break;
        case "M":
                        ret.append(this.getMonth() + 1);
            break;
        case "yyyy":
                        ret.append(this.getFullYear());
            break;
        case "yy":
                        ret.append(addLeadingZero(this.getFullYear() % 100));
            break;
        case "y":
                        ret.append(this.getFullYear() % 100);
            break;
        case "hh":
                        hour = this.getHours() % 12;
            if (hour === 0) hour = 12;
            ret.append(addLeadingZero(hour));
            break;
        case "h":
                        hour = this.getHours() % 12;
            if (hour === 0) hour = 12;
            ret.append(hour);
            break;
        case "HH":
                        ret.append(addLeadingZero(this.getHours()));
            break;
        case "H":
                        ret.append(this.getHours());
            break;
        case "mm":
                        ret.append(addLeadingZero(this.getMinutes()));
            break;
        case "m":
                        ret.append(this.getMinutes());
            break;
        case "ss":
                        ret.append(addLeadingZero(this.getSeconds()));
            break;
        case "s":
                        ret.append(this.getSeconds());
            break;
        case "tt":
                        ret.append((this.getHours() < 12) ? dtf.AMDesignator : dtf.PMDesignator);
            break;
        case "t":
                        ret.append(((this.getHours() < 12) ? dtf.AMDesignator : dtf.PMDesignator).charAt(0));
            break;
        case "f":
                        ret.append(addLeadingZeros(this.getMilliseconds()).charAt(0));
            break;
        case "ff":
                        ret.append(addLeadingZeros(this.getMilliseconds()).substr(0, 2));
            break;
        case "fff":
                        ret.append(addLeadingZeros(this.getMilliseconds()));
            break;
        case "z":
                        hour = this.getTimezoneOffset() / 60;
            ret.append(((hour >= 0) ? '+' : '-') + Math.floor(Math.abs(hour)));
            break;
        case "zz":
                        hour = this.getTimezoneOffset() / 60;
            ret.append(((hour >= 0) ? '+' : '-') + addLeadingZero(Math.floor(Math.abs(hour))));
            break;
        case "zzz":
                        hour = this.getTimezoneOffset() / 60;
            ret.append(((hour >= 0) ? '+' : '-') + addLeadingZero(Math.floor(Math.abs(hour))) +
                dtf.TimeSeparator + addLeadingZero(Math.abs(this.getTimezoneOffset() % 60)));
            break;
        }
    }
	logger.warn(`<<< Exiting _toFormattedString(format=${format},cultureInfo=${cultureInfo})`, { "event.action": "_toFormattedString", "logrite.useLocale": useLocale, "logrite.args": args });
    return ret.toString();
}
Number.__typeName = 'Number';
Number.__class = true;

Number.parseLocale = function Number$parseLocale(value) {
    /// <param name="value" type="String"></param>
    /// <returns type="Number"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String}
    ]);
    if (e) throw e;

    return Number._parse(value, Sys.CultureInfo.CurrentCulture);
}
Number.parseInvariant = function Number$parseInvariant(value) {
    /// <param name="value" type="String"></param>
    /// <returns type="Number"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String}
    ]);
    if (e) throw e;

    return Number._parse(value, Sys.CultureInfo.InvariantCulture);
}
Number._parse = function Number$_parse(value, cultureInfo) {
            var valueStr = value.trim();
    if (valueStr.match(/infinity/i) !== null) {
        return parseFloat(valueStr);
    }
    if (valueStr.match(/^0x[a-f0-9]+$/i) !== null) {
        return parseInt(valueStr);
    }
    var numFormat = cultureInfo.numberFormat;
    var decSeparator = numFormat.NumberDecimalSeparator;
    var grpSeparator = numFormat.NumberGroupSeparator;

            var numberFormatRegex = new RegExp("^[+-]?[\\d\\" + grpSeparator + "]*\\" + decSeparator + "?\\d*([eE][+-]?\\d+)?$");
    if (!valueStr.match(numberFormatRegex)) {
        return Number.NaN;
    }

        valueStr = valueStr.split(grpSeparator).join("");

        valueStr = valueStr.replace(decSeparator, ".");

    return parseFloat(valueStr);
}

Number.prototype.format = function Number$format(format) {
    /// <param name="format" type="String"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String}
    ]);
    if (e) throw e;

    return this._toFormattedString(format, Sys.CultureInfo.InvariantCulture);
}
Number.prototype.localeFormat = function Number$localeFormat(format) {
    /// <param name="format" type="String"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String}
    ]);
    if (e) throw e;

    return this._toFormattedString(format, Sys.CultureInfo.CurrentCulture);
}
Number.prototype._toFormattedString = function Number$_toFormattedString(format, cultureInfo) {
    if (!format || (format.length === 0) || (format === 'i')) {
        if (cultureInfo && (cultureInfo.name.length > 0)) {
            return this.toLocaleString();
        }
        else {
            return this.toString();
        }
    }

        var _percentPositivePattern = ["n %", "n%", "%n" ];
    var _percentNegativePattern = ["-n %", "-n%", "-%n"];
    var _numberNegativePattern = ["(n)","-n","- n","n-","n -"];
    var _currencyPositivePattern = ["$n","n$","$ n","n $"];
    var _currencyNegativePattern = ["($n)","-$n","$-n","$n-","(n$)","-n$","n-$","n$-","-n $","-$ n","n $-","$ n-","$ -n","n- $","($ n)","(n $)"];

        function expandNumber(number, precision, groupSizes, sep, decimalChar) {
		logger.error(`>>> Entering expandNumber(number=${number},precision=${precision},groupSizes=${groupSizes},sep=${sep},decimalChar=${decimalChar})`, { "event.action": "expandNumber", "logrite.number": number, "logrite.precision": precision, "logrite.groupSizes": groupSizes, "logrite.sep": sep, "logrite.decimalChar": decimalChar });
        
        var curSize = groupSizes[0];
        var curGroupIndex = 1;

                var numberString = number.toString();
        var right = "";
        var exponent = "";
				logger.error(`expandNumber(number=${number},precision=${precision},groupSizes=${groupSizes},sep=${sep},decimalChar=${decimalChar}): exponent → ${exponent}`, { "event.action": "expandNumber", "logrite.number": number, "logrite.precision": precision, "logrite.groupSizes": groupSizes, "logrite.sep": sep, "logrite.decimalChar": decimalChar });
                var decimalSplit = numberString.split('.');
				logger.error(`expandNumber(number=${number},precision=${precision},groupSizes=${groupSizes},sep=${sep},decimalChar=${decimalChar}): decimalSplit → ${decimalSplit}`, { "event.action": "expandNumber", "logrite.number": number, "logrite.precision": precision, "logrite.groupSizes": groupSizes, "logrite.sep": sep, "logrite.decimalChar": decimalChar });
        if (decimalSplit.length > 1) {
            numberString = decimalSplit[0];
            right = decimalSplit[1];
                        var exponentSplit = right.split(/e/i);
						logger.error(`expandNumber(number=${number},precision=${precision},groupSizes=${groupSizes},sep=${sep},decimalChar=${decimalChar}): exponentSplit → ${exponentSplit}`, { "event.action": "expandNumber", "logrite.number": number, "logrite.precision": precision, "logrite.groupSizes": groupSizes, "logrite.sep": sep, "logrite.decimalChar": decimalChar });
            if (exponentSplit.length > 1) {
                right = exponentSplit[0];
                exponent = "e" + exponentSplit[1];
            }
        }

                if (precision > 0) {
                        var rightDifference = right.length - precision;
            if (rightDifference > 0) {
                right = right.slice(0, precision);
            } else if (rightDifference < 0) {
                for (var i=0; i<Math.abs(rightDifference); i++) {
                    right += '0';
                }
            }

                        right = decimalChar + right;
        }
        else {             right = "";
        }
        right += exponent;

        var stringIndex = numberString.length-1;
        var ret = "";
		logger.error(`expandNumber(number=${number},precision=${precision},groupSizes=${groupSizes},sep=${sep},decimalChar=${decimalChar}): ret → ${ret}`, { "event.action": "expandNumber", "logrite.number": number, "logrite.precision": precision, "logrite.groupSizes": groupSizes, "logrite.sep": sep, "logrite.decimalChar": decimalChar });
        while (stringIndex >= 0) {

                        if (curSize === 0 || curSize > stringIndex) {
                if (ret.length > 0)
                    return numberString.slice(0, stringIndex + 1) + sep + ret + right;
                else
                    return numberString.slice(0, stringIndex + 1) + right;
            }

            if (ret.length > 0)
                ret = numberString.slice(stringIndex - curSize + 1, stringIndex+1) + sep + ret;
            else
                ret = numberString.slice(stringIndex - curSize + 1, stringIndex+1);

            stringIndex -= curSize;

            if (curGroupIndex < groupSizes.length) {
                curSize = groupSizes[curGroupIndex];
                curGroupIndex++;
            }
        }
		logger.warn(`<<< Exiting expandNumber(number=${number},precision=${precision},groupSizes=${groupSizes},sep=${sep},decimalChar=${decimalChar})`, { "event.action": "expandNumber", "logrite.number": number, "logrite.precision": precision, "logrite.groupSizes": groupSizes, "logrite.sep": sep, "logrite.decimalChar": decimalChar });
        return numberString.slice(0, stringIndex + 1) + sep + ret + right;
    }
    var nf = cultureInfo.numberFormat;

        var number = Math.abs(this);

        if (!format)
        format = "D";

    var precision = -1;
    if (format.length > 1) precision = parseInt(format.slice(1));

    var pattern;
    switch (format.charAt(0)) {
    case "d":
    case "D":
        pattern = 'n';

                if (precision !== -1) {
            var numberStr = ""+number;
            var zerosToAdd = precision - numberStr.length;
            if (zerosToAdd > 0) {
                for (var i=0; i<zerosToAdd; i++) {
                    numberStr = '0'+numberStr;
                }
            }
            number = numberStr;
        }

                if (this < 0) number = -number;
        break;
    case "c":
    case "C":
        if (this < 0) pattern = _currencyNegativePattern[nf.CurrencyNegativePattern];
        else pattern = _currencyPositivePattern[nf.CurrencyPositivePattern];
        if (precision === -1) precision = nf.CurrencyDecimalDigits;
        number = expandNumber(Math.abs(this), precision, nf.CurrencyGroupSizes, nf.CurrencyGroupSeparator, nf.CurrencyDecimalSeparator);
        break;
    case "n":
    case "N":
        if (this < 0) pattern = _numberNegativePattern[nf.NumberNegativePattern];
        else pattern = 'n';
        if (precision === -1) precision = nf.NumberDecimalDigits;
        number = expandNumber(Math.abs(this), precision, nf.NumberGroupSizes, nf.NumberGroupSeparator, nf.NumberDecimalSeparator);
        break;
    case "p":
    case "P":
        if (this < 0) pattern = _percentNegativePattern[nf.PercentNegativePattern];
        else pattern = _percentPositivePattern[nf.PercentPositivePattern];
        if (precision === -1) precision = nf.PercentDecimalDigits;
        number = expandNumber(Math.abs(this), precision, nf.PercentGroupSizes, nf.PercentGroupSeparator, nf.PercentDecimalSeparator);
        break;
    default:
        throw Error.format(Sys.Res.formatBadFormatSpecifier);
    }

    var regex = /n|\$|-|%/g;

        var ret = "";

    for (;;) {

                var index = regex.lastIndex;

                var ar = regex.exec(pattern);

                ret += pattern.slice(index, ar ? ar.index : pattern.length);

        if (!ar)
            break;

        switch (ar[0]) {
        case "n":
            ret += number;
            break;
        case "$":
            ret += nf.CurrencySymbol;
            break;
        case "-":
            ret += nf.NegativeSign;
            break;
        case "%":
            ret += nf.PercentSymbol;
            break;
        }
    }

    return ret;
}
RegExp.__typeName = 'RegExp';
RegExp.__class = true;
Array.__typeName = 'Array';
Array.__class = true;

Array.add = Array.enqueue = function Array$enqueue(array, item) {
	logger.error(`>>> Entering enqueue(array=${array},item=${item})`, { "event.action": "enqueue", "logrite.array": array, "logrite.item": item });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="item" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "item", mayBeNull: true}
    ]);
    if (e) throw e;


        array[array.length] = item;
		logger.warn(`<<< Exiting enqueue(array=${array},item=${item})`, { "event.action": "enqueue", "logrite.array": array, "logrite.item": item });
}

Array.addRange = function Array$addRange(array, items) {
	logger.error(`>>> Entering addRange(array=${array},items=${items})`, { "event.action": "addRange", "logrite.array": array, "logrite.items": items });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="items" type="Array" elementMayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "items", type: Array, elementMayBeNull: true}
    ]);
    if (e) throw e;


        array.push.apply(array, items);
		logger.warn(`<<< Exiting addRange(array=${array},items=${items})`, { "event.action": "addRange", "logrite.array": array, "logrite.items": items });
}

Array.clear = function Array$clear(array) {
	logger.error(`>>> Entering clear(array=${array})`, { "event.action": "clear" });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true}
    ]);
    if (e) throw e;

    array.length = 0;
	logger.debug(`clear(): this._value → ${this._value}`, { "event.action": "clear" });
	logger.debug(`clear(): this._parts → ${this._parts}`, { "event.action": "clear" });
	logger.debug(`clear(): this._len → ${this._len}`, { "event.action": "clear" });
}

Array.clone = function Array$clone(array) {
	logger.error(`>>> Entering clone(array=${array})`, { "event.action": "clone", "logrite.array": array });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <returns type="Array" elementMayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true}
    ]);
    if (e) throw e;

    if (array.length === 1) {
        return [array[0]];
    }
    else {
						logger.warn(`<<< Exiting clone(array=${array})`, { "event.action": "clone", "logrite.array": array });
                        return Array.apply(null, array);
    }
}

Array.contains = function Array$contains(array, item) {
	logger.error(`>>> Entering contains(array=${array},item=${item})`, { "event.action": "contains", "logrite.array": array, "logrite.item": item });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="item" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "item", mayBeNull: true}
    ]);
    if (e) throw e;

	logger.warn(`<<< Exiting contains(array=${array},item=${item})`, { "event.action": "contains", "logrite.array": array, "logrite.item": item });
    return (Array.indexOf(array, item) >= 0);
}

Array.dequeue = function Array$dequeue(array) {
	logger.error(`>>> Entering dequeue(array=${array})`, { "event.action": "dequeue", "logrite.array": array });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <returns mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true}
    ]);
    if (e) throw e;

	logger.warn(`<<< Exiting dequeue(array=${array})`, { "event.action": "dequeue", "logrite.array": array });
    return array.shift();
}

Array.forEach = function Array$forEach(array, method, instance) {
	logger.error(`>>> Entering forEach(array=${array},method=${method},instance=${instance})`, { "event.action": "forEach", "logrite.array": array, "logrite.method": method, "logrite.instance": instance });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="method" type="Function"></param>
    /// <param name="instance" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "method", type: Function},
        {name: "instance", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    for (var i = 0, l = array.length; i < l; i++) {
        var elt = array[i];
        if (typeof(elt) !== 'undefined') method.call(instance, elt, i, array);
		logger.warn(`<<< Exiting forEach(array=${array},method=${method},instance=${instance})`, { "event.action": "forEach", "logrite.array": array, "logrite.method": method, "logrite.instance": instance });
    }
}

Array.indexOf = function Array$indexOf(array, item, start) {
	logger.error(`>>> Entering indexOf(array=${array},item=${item},start=${start})`, { "event.action": "indexOf", "logrite.array": array, "logrite.item": item, "logrite.start": start });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="item" optional="true" mayBeNull="true"></param>
    /// <param name="start" optional="true" mayBeNull="true"></param>
    /// <returns type="Number"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "item", mayBeNull: true, optional: true},
        {name: "start", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    if (typeof(item) === "undefined") return -1;
    var length = array.length;
    if (length !== 0) {
                start = start - 0;
                if (isNaN(start)) {
            start = 0;
        }
        else {
                                    if (isFinite(start)) {
                                start = start - (start % 1);
            }
                        if (start < 0) {
                start = Math.max(0, length + start);
            }
        }

                for (var i = start; i < length; i++) {
            if ((typeof(array[i]) !== "undefined") && (array[i] === item)) {
                return i;
            }
        }
    }
	logger.warn(`<<< Exiting indexOf(array=${array},item=${item},start=${start})`, { "event.action": "indexOf", "logrite.array": array, "logrite.item": item, "logrite.start": start });
    return -1;
}

Array.insert = function Array$insert(array, index, item) {
	logger.error(`>>> Entering insert(array=${array},index=${index},item=${item})`, { "event.action": "insert", "logrite.array": array, "logrite.index": index, "logrite.item": item });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="index" mayBeNull="true"></param>
    /// <param name="item" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "index", mayBeNull: true},
        {name: "item", mayBeNull: true}
    ]);
    if (e) throw e;

    array.splice(index, 0, item);
	logger.warn(`<<< Exiting insert(array=${array},index=${index},item=${item})`, { "event.action": "insert", "logrite.array": array, "logrite.index": index, "logrite.item": item });
}

Array.parse = function Array$parse(value) {
    /// <param name="value" type="String" mayBeNull="true"></param>
    /// <returns type="Array" elementMayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String, mayBeNull: true}
    ]);
    if (e) throw e;

    if (!value) return [];
    var v = eval(value);
    if (!Array.isInstanceOfType(v)) throw Error.argument('value', Sys.Res.arrayParseBadFormat);
    return v;
}

Array.remove = function Array$remove(array, item) {
	logger.error(`>>> Entering remove(array=${array},item=${item})`, { "event.action": "remove", "logrite.array": array, "logrite.item": item });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="item" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "item", mayBeNull: true}
    ]);
    if (e) throw e;

    var index = Array.indexOf(array, item);
    if (index >= 0) {
        array.splice(index, 1);
    }
	logger.warn(`<<< Exiting remove(array=${array},item=${item})`, { "event.action": "remove", "logrite.array": array, "logrite.item": item });
    return (index >= 0);
}

Array.removeAt = function Array$removeAt(array, index) {
	logger.error(`>>> Entering removeAt(array=${array},index=${index})`, { "event.action": "removeAt", "logrite.array": array, "logrite.index": index });
    /// <param name="array" type="Array" elementMayBeNull="true"></param>
    /// <param name="index" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "array", type: Array, elementMayBeNull: true},
        {name: "index", mayBeNull: true}
    ]);
    if (e) throw e;

    array.splice(index, 1);
	logger.warn(`<<< Exiting removeAt(array=${array},index=${index})`, { "event.action": "removeAt", "logrite.array": array, "logrite.index": index });
}
String.__typeName = 'String';
String.__class = true;

String.prototype.endsWith = function String$endsWith(suffix) {
	logger.error(`>>> Entering endsWith(suffix=${suffix})`, { "event.action": "endsWith", "logrite.suffix": suffix });
    /// <param name="suffix" type="String"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "suffix", type: String}
    ]);
    if (e) throw e;

	logger.warn(`<<< Exiting endsWith(suffix=${suffix})`, { "event.action": "endsWith", "logrite.suffix": suffix });
    return (this.substr(this.length - suffix.length) === suffix);
}

String.prototype.startsWith = function String$startsWith(prefix) {
	logger.error(`>>> Entering startsWith(prefix=${prefix})`, { "event.action": "startsWith", "logrite.prefix": prefix });
    /// <param name="prefix" type="String"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "prefix", type: String}
    ]);
    if (e) throw e;

	logger.warn(`<<< Exiting startsWith(prefix=${prefix})`, { "event.action": "startsWith", "logrite.prefix": prefix });
    return (this.substr(0, prefix.length) === prefix);
}

String.prototype.trim = function String$trim() {
	logger.error('>>> Entering trim()', { "event.action": "trim" });
    /// <returns type="String"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting trim()', { "event.action": "trim" });
    return this.replace(/^\s+|\s+$/g, '');
}

String.prototype.trimEnd = function String$trimEnd() {
	logger.error('>>> Entering trimEnd()', { "event.action": "trimEnd" });
    /// <returns type="String"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting trimEnd()', { "event.action": "trimEnd" });
    return this.replace(/\s+$/, '');
}

String.prototype.trimStart = function String$trimStart() {
	logger.error('>>> Entering trimStart()', { "event.action": "trimStart" });
    /// <returns type="String"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting trimStart()', { "event.action": "trimStart" });
    return this.replace(/^\s+/, '');
}

String.format = function String$format(format, args) {
    /// <param name="format" type="String"></param>
    /// <param name="args" parameterArray="true" mayBeNull="true"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String},
        {name: "args", mayBeNull: true, parameterArray: true}
    ]);
    if (e) throw e;

    return String._toFormattedString(false, arguments);
}

String.localeFormat = function String$localeFormat(format, args) {
    /// <param name="format" type="String"></param>
    /// <param name="args" parameterArray="true" mayBeNull="true"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "format", type: String},
        {name: "args", mayBeNull: true, parameterArray: true}
    ]);
    if (e) throw e;

    return String._toFormattedString(true, arguments);
}

String._toFormattedString = function String$_toFormattedString(useLocale, args) {
    var result = '';
    var format = args[0];

    for (var i=0;;) {
                var open = format.indexOf('{', i);
        var close = format.indexOf('}', i);
        if ((open < 0) && (close < 0)) {
                        result += format.slice(i);
            break;
        }
        if ((close > 0) && ((close < open) || (open < 0))) {
                        if (format.charAt(close + 1) !== '}') {
                throw Error.argument('format', Sys.Res.stringFormatBraceMismatch);
            }
            result += format.slice(i, close + 1);
            i = close + 2;
            continue;
        }

                result += format.slice(i, open);
        i = open + 1;

                if (format.charAt(i) === '{') {
            result += '{';
            i++;
            continue;
        }

                if (close < 0) throw Error.argument('format', Sys.Res.stringFormatBraceMismatch);

        
                var brace = format.substring(i, close);
        var colonIndex = brace.indexOf(':');
        var argNumber = parseInt((colonIndex < 0)? brace : brace.substring(0, colonIndex)) + 1;
        if (isNaN(argNumber)) throw Error.argument('format', Sys.Res.stringFormatInvalid);
        var argFormat = (colonIndex < 0)? '' : brace.substring(colonIndex + 1);

        var arg = args[argNumber];
        if (typeof(arg) === "undefined" || arg === null) {
            arg = '';
        }

                if (arg.toFormattedString) {
            result += arg.toFormattedString(argFormat);
        }
        else if (useLocale && arg.localeFormat) {
            result += arg.localeFormat(argFormat);
        }
        else if (arg.format) {
            result += arg.format(argFormat);
        }
        else
            result += arg.toString();

        i = close + 1;
    }

    return result;
}

Type.registerNamespace('Sys');
Sys.IDisposable = function Sys$IDisposable() {
	logger.error('>>> Entering IDisposable()', { "event.action": "IDisposable" });
	logger.warn('<<< Exiting IDisposable()', { "event.action": "IDisposable" });
    throw Error.notImplemented();
}

    function Sys$IDisposable$dispose() {
		logger.error('>>> Entering dispose()', { "event.action": "dispose" });
		logger.warn('<<< Exiting dispose()', { "event.action": "dispose" });
        throw Error.notImplemented();
    }
Sys.IDisposable.prototype = {
    dispose: Sys$IDisposable$dispose
}
Sys.IDisposable.registerInterface('Sys.IDisposable');
Sys.StringBuilder = function Sys$StringBuilder(initialText) {
	logger.error(`>>> Entering StringBuilder(initialText=${initialText})`, { "event.action": "StringBuilder", "logrite.initialText": initialText });
    /// <param name="initialText" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "initialText", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    this._parts = (typeof(initialText) !== 'undefined' && initialText !== null && initialText !== '') ?
        [initialText.toString()] : [];
    this._value = {};
    this._len = 0;
	logger.debug(`StringBuilder(initialText=${initialText}): this._value → ${this._value}`, { "event.action": "StringBuilder", "logrite.initialText": initialText });
}


    function Sys$StringBuilder$append(text) {
		logger.error(`>>> Entering append(text=${text})`, { "event.action": "append", "logrite.text": text });
        /// <param name="text" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "text", mayBeNull: true}
        ]);
        if (e) throw e;

        this._parts[this._parts.length] = text;
		logger.warn(`<<< Exiting append(text=${text})`, { "event.action": "append", "logrite.text": text });
    }

    function Sys$StringBuilder$appendLine(text) {
		logger.error(`>>> Entering appendLine(text=${text})`, { "event.action": "appendLine", "logrite.text": text });
        /// <param name="text" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "text", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        this._parts[this._parts.length] =
            ((typeof(text) === 'undefined') || (text === null) || (text === '')) ?
            '\r\n' : text + '\r\n';
			logger.warn(`<<< Exiting appendLine(text=${text})`, { "event.action": "appendLine", "logrite.text": text });
    }

    function Sys$StringBuilder$clear() {
        if (arguments.length !== 0) throw Error.parameterCount();
        this._parts = [];
        this._value = {};
        this._len = 0;
    }

    function Sys$StringBuilder$isEmpty() {
		logger.error('>>> Entering isEmpty()', { "event.action": "isEmpty" });
        /// <returns type="Boolean"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._parts.length === 0) return true;
		logger.warn('<<< Exiting isEmpty()', { "event.action": "isEmpty" });
        return this.toString() === '';
    }



    function Sys$StringBuilder$toString(separator) {
		logger.error(`>>> Entering toString(separator=${separator})`, { "event.action": "toString", "logrite.value": value });
        /// <param name="separator" type="String" optional="true" mayBeNull="true"></param>
        /// <returns type="String"></returns>
        var e = Function._validateParams(arguments, [
            {name: "separator", type: String, mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        separator = separator || '';
        var parts = this._parts;
		logger.error(`toString(separator=${separator}): separator → ${separator}`, { "event.action": "toString", "logrite.value": value });
        if (this._len !== parts.length) {
            this._value = {};
            this._len = parts.length;
			logger.debug(`toString(separator=${separator}): this._value → ${this._value}`, { "event.action": "toString", "logrite.value": value });
        }
        var val = this._value;
		logger.debug(`toString(separator=${separator}): this._len → ${this._len}`, { "event.action": "toString", "logrite.value": value });
        if (typeof(val[separator]) === 'undefined') {
                        if (separator !== '') {
                for (var i = 0; i < parts.length;) {
                    if ((typeof(parts[i]) === 'undefined') || (parts[i] === '') || (parts[i] === null)) {
                        parts.splice(i, 1);
                    }
                    else {
                        i++;
                    }
                }
            }
            val[separator] = this._parts.join(separator);
        }
        return val[separator];
    }
Sys.StringBuilder.prototype = {
    append: Sys$StringBuilder$append,

    appendLine: Sys$StringBuilder$appendLine,

    clear: Sys$StringBuilder$clear,

    isEmpty: Sys$StringBuilder$isEmpty,

            toString: Sys$StringBuilder$toString
}
Sys.StringBuilder.registerClass('Sys.StringBuilder');
if (!window.XMLHttpRequest) {
    window.XMLHttpRequest = function window$XMLHttpRequest() {
		logger.error('>>> Entering XMLHttpRequest()', { "event.action": "XMLHttpRequest" });
        var progIDs = [ 'Msxml2.XMLHTTP', 'Microsoft.XMLHTTP' ];
	    
		logger.error(`XMLHttpRequest(): progIDs → ${progIDs}`, { "event.action": "XMLHttpRequest" });
        for (var i = 0; i < progIDs.length; i++) {
            try {
                var xmlHttp = new ActiveXObject(progIDs[i]);
				logger.error(`XMLHttpRequest(): xmlHttp → ${xmlHttp}`, { "event.action": "XMLHttpRequest" });
                return xmlHttp;
            }
            catch (ex) {
			logger.error('Exception in XMLHttpRequest():', ex.message, ex, { "event.action": "XMLHttpRequest", "event.category": "process" });
            }
        }
	    
		logger.warn('<<< Exiting XMLHttpRequest()', { "event.action": "XMLHttpRequest" });
        return null;
    }
}

Sys.Browser = {};

Sys.Browser.InternetExplorer = {};
Sys.Browser.Firefox = {};
Sys.Browser.Safari = {};
Sys.Browser.Opera = {};

Sys.Browser.agent = null;
Sys.Browser.hasDebuggerStatement = false;
Sys.Browser.name = navigator.appName;
Sys.Browser.version = parseFloat(navigator.appVersion);

if (navigator.userAgent.indexOf(' MSIE ') > -1) {
    Sys.Browser.agent = Sys.Browser.InternetExplorer;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/MSIE (\d+\.\d+)/)[1]);
    Sys.Browser.hasDebuggerStatement = true;
}
else if (navigator.userAgent.indexOf(' Firefox/') > -1) {
    Sys.Browser.agent = Sys.Browser.Firefox;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/ Firefox\/(\d+\.\d+)/)[1]);
    Sys.Browser.name = 'Firefox';
    Sys.Browser.hasDebuggerStatement = true;
}
else if (navigator.userAgent.indexOf(' Safari/') > -1) {
    Sys.Browser.agent = Sys.Browser.Safari;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/ Safari\/(\d+\.\d+)/)[1]);
    Sys.Browser.name = 'Safari';
}
else if (navigator.userAgent.indexOf('Opera/') > -1) {
    Sys.Browser.agent = Sys.Browser.Opera;
}


Type.registerNamespace('Sys.UI');

Sys._Debug = function Sys$_Debug() {
	logger.error('>>> Entering _Debug()', { "event.action": "_Debug" });
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting _Debug()', { "event.action": "_Debug" });
}


    function Sys$_Debug$_appendConsole(text) {
				logger.error(`>>> Entering _appendConsole(text=${text})`, { "event.action": "_appendConsole", "logrite.text": text });
				logger.warn(`<<< Exiting _appendConsole(text=${text})`, { "event.action": "_appendConsole", "logrite.text": text });
                if ((typeof(Debug) !== 'undefined') && Debug.writeln) {
            Debug.writeln(text);
        }
                if (window.console && window.console.log) {
            window.console.log(text);
        }
                if (window.opera) {
            window.opera.postError(text);
        }
                if (window.debugService) {
            window.debugService.trace(text);
        }
    }

    function Sys$_Debug$_appendTrace(text) {
		logger.error(`>>> Entering _appendTrace(text=${text})`, { "event.action": "_appendTrace", "logrite.text": text });
        var traceElement = document.getElementById('TraceConsole');
        if (traceElement && (traceElement.tagName.toUpperCase() === 'TEXTAREA')) {
			logger.error(`_appendTrace(text=${text}): traceElement → ${traceElement}`, { "event.action": "_appendTrace", "logrite.text": text });
            traceElement.value += text + '\n';
			logger.warn(`<<< Exiting _appendTrace(text=${text})`, { "event.action": "_appendTrace", "logrite.text": text });
        }
    }

    function Sys$_Debug$assert(condition, message, displayCaller) {
		logger.error(`>>> Entering assert(condition=${condition},message=${message},displayCaller=${displayCaller})`, { "event.action": "assert", "logrite.condition": condition, "logrite.message": message, "logrite.displayCaller": displayCaller });
        /// <param name="condition" type="Boolean"></param>
        /// <param name="message" type="String" optional="true" mayBeNull="true"></param>
        /// <param name="displayCaller" type="Boolean" optional="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "condition", type: Boolean},
            {name: "message", type: String, mayBeNull: true, optional: true},
            {name: "displayCaller", type: Boolean, optional: true}
        ]);
        if (e) throw e;

        if (!condition) {
            message = (displayCaller && this.assert.caller) ?
                String.format(Sys.Res.assertFailedCaller, message, this.assert.caller) :
                String.format(Sys.Res.assertFailed, message);

            if (confirm(String.format(Sys.Res.breakIntoDebugger, message))) {
                this.fail(message);
				logger.warn(`<<< Exiting assert(condition=${condition},message=${message},displayCaller=${displayCaller})`, { "event.action": "assert", "logrite.condition": condition, "logrite.message": message, "logrite.displayCaller": displayCaller });
            }
        }
    }

    function Sys$_Debug$clearTrace() {
		logger.error('>>> Entering clearTrace()', { "event.action": "clearTrace" });
        var traceElement = document.getElementById('TraceConsole');
        if (traceElement && (traceElement.tagName.toUpperCase() === 'TEXTAREA')) {
			logger.error(`clearTrace(): traceElement → ${traceElement}`, { "event.action": "clearTrace" });
            traceElement.value = '';
			logger.debug(`clearTrace(): traceElement.value → ${traceElement.value}`, { "event.action": "clearTrace" });
        }
    }

    function Sys$_Debug$fail(message) {
		logger.error(`>>> Entering fail(message=${message})`, { "event.action": "fail", "logrite.message": message });
        /// <param name="message" type="String" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "message", type: String, mayBeNull: true}
        ]);
        if (e) throw e;

        this._appendConsole(message);

                if (Sys.Browser.hasDebuggerStatement) {
            eval('debugger');
			logger.warn(`<<< Exiting fail(message=${message})`, { "event.action": "fail", "logrite.message": message });
        }
    }

    function Sys$_Debug$trace(text) {
		logger.error(`>>> Entering trace(text=${text})`, { "event.action": "trace", "logrite.text": text });
        /// <param name="text"></param>
        var e = Function._validateParams(arguments, [
            {name: "text"}
        ]);
        if (e) throw e;

        this._appendConsole(text);
        this._appendTrace(text);
		logger.warn(`<<< Exiting trace(text=${text})`, { "event.action": "trace", "logrite.text": text });
    }

    function Sys$_Debug$traceDump(object, name) {
		logger.error(`>>> Entering traceDump(object=${object},name=${name})`, { "event.action": "traceDump", "logrite.object": object, "logrite.name": name });
        /// <param name="object" mayBeNull="true"></param>
        /// <param name="name" type="String" mayBeNull="true" optional="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "object", mayBeNull: true},
            {name: "name", type: String, mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        var text = this._traceDump(object, name, true);
		logger.warn(`<<< Exiting traceDump(object=${object},name=${name})`, { "event.action": "traceDump", "logrite.object": object, "logrite.name": name });
    }

    function Sys$_Debug$_traceDump(object, name, recursive, indentationPadding, loopArray) {
		logger.error(`>>> Entering _traceDump(object=${object},name=${name},recursive=${recursive},indentationPadding=${indentationPadding},loopArray=${loopArray})`, { "event.action": "_traceDump", "logrite.object": object, "logrite.name": name, "logrite.recursive": recursive, "logrite.indentationPadding": indentationPadding, "logrite.loopArray": loopArray });
        name = name? name : 'traceDump';
        indentationPadding = indentationPadding? indentationPadding : '';
		logger.error(`_traceDump(object=${object},name=${name},recursive=${recursive},indentationPadding=${indentationPadding},loopArray=${loopArray}): name → ${name}`, { "event.action": "_traceDump", "logrite.object": object, "logrite.name": name, "logrite.recursive": recursive, "logrite.indentationPadding": indentationPadding, "logrite.loopArray": loopArray });
        if (object === null) {
            this.trace(indentationPadding + name + ': null');
            return;
        }
        switch(typeof(object)) {
            case 'undefined':
                this.trace(indentationPadding + name + ': Undefined');
                break;
            case 'number': case 'string': case 'boolean':
                this.trace(indentationPadding + name + ': ' + object);
                break;
            default:
                if (Date.isInstanceOfType(object) || RegExp.isInstanceOfType(object)) {
                    this.trace(indentationPadding + name + ': ' + object.toString());
                    break;
                }
                if (!loopArray) {
                    loopArray = [];
                }
                else if (Array.contains(loopArray, object)) {
                    this.trace(indentationPadding + name + ': ...');
                    return;
                }
                Array.add(loopArray, object);

                                                                if ((object == window) || (object === document) ||
                    (window.HTMLElement && (object instanceof HTMLElement)) ||
                    (typeof(object.nodeName) === 'string')) {
                    var tag = object.tagName? object.tagName : 'DomElement';
					logger.error(`_traceDump(object=${object},name=${name},recursive=${recursive},indentationPadding=${indentationPadding},loopArray=${loopArray}): tag → ${tag}`, { "event.action": "_traceDump", "logrite.object": object, "logrite.name": name, "logrite.recursive": recursive, "logrite.indentationPadding": indentationPadding, "logrite.loopArray": loopArray });
                    if (object.id) {
                        tag += ' - ' + object.id;
                    }
                    this.trace(indentationPadding + name + ' {' +  tag + '}');
                }
                                else {
                    var typeName = Object.getTypeName(object);
					logger.error(`_traceDump(object=${object},name=${name},recursive=${recursive},indentationPadding=${indentationPadding},loopArray=${loopArray}): typeName → ${typeName}`, { "event.action": "_traceDump", "logrite.object": object, "logrite.name": name, "logrite.recursive": recursive, "logrite.indentationPadding": indentationPadding, "logrite.loopArray": loopArray });
                    this.trace(indentationPadding + name + (typeof(typeName) === 'string' ? ' {' + typeName + '}' : ''));
                    if ((indentationPadding === '') || recursive) {
                        indentationPadding += "    ";
                        var i, length, properties, p, v;
                        if (Array.isInstanceOfType(object)) {
                            length = object.length;
                            for (i = 0; i < length; i++) {
                                this._traceDump(object[i], '[' + i + ']', recursive, indentationPadding, loopArray);
                            }
                        }
                        else {
                            for (p in object) {
                                v = object[p];
                                if (!Function.isInstanceOfType(v)) {
                                    this._traceDump(v, p, recursive, indentationPadding, loopArray);
                                }
                            }
                        }
                    }
                }
                Array.remove(loopArray, object);
        }
    }
Sys._Debug.prototype = {

    _appendConsole: Sys$_Debug$_appendConsole,

    _appendTrace: Sys$_Debug$_appendTrace,

    assert: Sys$_Debug$assert,

    clearTrace: Sys$_Debug$clearTrace,

    fail: Sys$_Debug$fail,

    trace: Sys$_Debug$trace,

    traceDump: Sys$_Debug$traceDump,

    _traceDump: Sys$_Debug$_traceDump
}
Sys._Debug.registerClass('Sys._Debug');

Sys.Debug = new Sys._Debug();
    Sys.Debug.isDebug = true;
function Sys$Enum$parse(value, ignoreCase) {
    /// <param name="value" type="String"></param>
    /// <param name="ignoreCase" type="Boolean" optional="true"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String},
        {name: "ignoreCase", type: Boolean, optional: true}
    ]);
    if (e) throw e;

    var values, parsed, val;
    if (ignoreCase) {
        values = this.__lowerCaseValues;
        if (!values) {
            this.__lowerCaseValues = values = {};
            var prototype = this.prototype;
            for (var name in prototype) {
                values[name.toLowerCase()] = prototype[name];
            }
        }
    }
    else {
        values = this.prototype;
    }
    if (!this.__flags) {
        val = (ignoreCase ? value.toLowerCase() : value);
        parsed = values[val.trim()];
        if (typeof(parsed) !== 'number') throw Error.argument('value', String.format(Sys.Res.enumInvalidValue, value, this.__typeName));
        return parsed;
    }
    else {
        var parts = (ignoreCase ? value.toLowerCase() : value).split(',');
        var v = 0;

        for (var i = parts.length - 1; i >= 0; i--) {
            var part = parts[i].trim();
            parsed = values[part];
            if (typeof(parsed) !== 'number') throw Error.argument('value', String.format(Sys.Res.enumInvalidValue, value.split(',')[i].trim(), this.__typeName));
            v |= parsed;
        }
        return v;
    }
}

function Sys$Enum$toString(value) {
    /// <param name="value" optional="true" mayBeNull="true"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

            if ((typeof(value) === 'undefined') || (value === null)) return this.__string;
    if ((typeof(value) != 'number') || ((value % 1) !== 0)) throw Error.argumentType('value', Object.getType(value), this);
    var values = this.prototype;
    var i;
    if (!this.__flags || (value === 0)) {
        for (i in values) {
            if (values[i] === value) {
                return i;
            }
        }
    }
    else {
        var sorted = this.__sortedValues;
        if (!sorted) {
            sorted = [];
            for (i in values) {
                sorted[sorted.length] = {key: i, value: values[i]};
            }
            sorted.sort(function(a, b) {
                return a.value - b.value;
            });
            this.__sortedValues = sorted;
        }
        var parts = [];
        var v = value;
        for (i = sorted.length - 1; i >= 0; i--) {
            var kvp = sorted[i];
            var vali = kvp.value;
            if (vali === 0) continue;
            if ((vali & value) === vali) {
                parts[parts.length] = kvp.key;
                v -= vali;
                if (v === 0) break;
            }
        }
        if (parts.length && v === 0) return parts.reverse().join(', ');
    }
    throw Error.argumentOutOfRange('value', value, String.format(Sys.Res.enumInvalidValue, value, this.__typeName));
}

Type.prototype.registerEnum = function Type$registerEnum(name, flags) {
	logger.error(`>>> Entering registerEnum(name=${name},flags=${flags})`, { "event.action": "registerEnum", "logrite.name": name, "logrite.flags": flags });
    /// <param name="name" type="String"></param>
    /// <param name="flags" type="Boolean" optional="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "name", type: String},
        {name: "flags", type: Boolean, optional: true}
    ]);
    if (e) throw e;

    if (!Type.__fullyQualifiedIdentifierRegExp.test(name)) throw Error.argument('name', Sys.Res.notATypeName);
        var parsedName;
    try {
        parsedName = eval(name);
    }
    catch(e) {
		logger.error(`Exception in registerEnum(name=${name},flags=${flags}): ${e.message}`, e, { "event.action": "registerEnum", "event.category": "process", "logrite.name": name, "logrite.flags": flags });
		logger.error(`registerEnum(name=${name},flags=${flags}): parsedName → ${parsedName}`, { "event.action": "registerEnum", "logrite.name": name, "logrite.flags": flags });
        throw Error.argument('name', Sys.Res.argumentTypeName);
    }
    if (parsedName !== this) throw Error.argument('name', Sys.Res.badTypeName);
	logger.debug(`registerEnum(name=${name},flags=${flags}): parsedName → ${parsedName}`, { "event.action": "registerEnum", "logrite.name": name, "logrite.flags": flags });
    if (window.__registeredTypes[name]) throw Error.invalidOperation(String.format(Sys.Res.typeRegisteredTwice, name));
    for (var i in this.prototype) {
        var val = this.prototype[i];
        if (!Type.__identifierRegExp.test(i)) throw Error.invalidOperation(String.format(Sys.Res.enumInvalidValueName, i));
        if (typeof(val) !== 'number' || (val % 1) !== 0) throw Error.invalidOperation(Sys.Res.enumValueNotInteger);
        if (typeof(this[i]) !== 'undefined') throw Error.invalidOperation(String.format(Sys.Res.enumReservedName, i));
    }
    for (var i in this.prototype) {
        this[i] = this.prototype[i];
    }
    this.__typeName = name;
    this.parse = Sys$Enum$parse;
	logger.debug(`registerEnum(name=${name},flags=${flags}): this.__typeName → ${this.__typeName}`, { "event.action": "registerEnum", "logrite.name": name, "logrite.flags": flags });
    this.__string = this.toString();
    this.toString = Sys$Enum$toString;
	logger.debug(`registerEnum(name=${name},flags=${flags}): this.parse → ${this.parse}`, { "event.action": "registerEnum", "logrite.name": name, "logrite.flags": flags });
    this.__flags = flags;
    this.__enum = true;
    window.__registeredTypes[name] = true;
	logger.debug(`registerEnum(name=${name},flags=${flags}): this.toString → ${this.toString}`, { "event.action": "registerEnum", "logrite.name": name, "logrite.flags": flags });
}

Type.isEnum = function Type$isEnum(type) {
	logger.error(`>>> Entering isEnum(type=${type})`, { "event.action": "isEnum", "logrite.type": type });
    /// <param name="type" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(type) === 'undefined') || (type === null)) return false;
	logger.warn(`<<< Exiting isEnum(type=${type})`, { "event.action": "isEnum", "logrite.type": type });
    return !!type.__enum;
}

Type.isFlags = function Type$isFlags(type) {
	logger.error(`>>> Entering isFlags(type=${type})`, { "event.action": "isFlags", "logrite.type": type });
    /// <param name="type" mayBeNull="true"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", mayBeNull: true}
    ]);
    if (e) throw e;

    if ((typeof(type) === 'undefined') || (type === null)) return false;
	logger.warn(`<<< Exiting isFlags(type=${type})`, { "event.action": "isFlags", "logrite.type": type });
    return !!type.__flags;
}
Sys.EventHandlerList = function Sys$EventHandlerList() {
	logger.error('>>> Entering EventHandlerList()', { "event.action": "EventHandlerList" });
    if (arguments.length !== 0) throw Error.parameterCount();
    this._list = {};
	logger.debug(`EventHandlerList(): this._list → ${this._list}`, { "event.action": "EventHandlerList" });
}


    function Sys$EventHandlerList$addHandler(id, handler) {
		logger.error(`>>> Entering addHandler(id=${id},handler=${handler})`, { "event.action": "addHandler", "logrite.element": element, "logrite.eventName": eventName, "logrite.handler": handler });
        /// <param name="id" type="String"></param>
        /// <param name="handler" type="Function"></param>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String},
            {name: "handler", type: Function}
        ]);
        if (e) throw e;

        Array.add(this._getEvent(id, true), handler);
		logger.warn(`<<< Exiting addHandler(id=${id},handler=${handler})`, { "event.action": "addHandler", "logrite.element": element, "logrite.eventName": eventName, "logrite.handler": handler });
    }
    function Sys$EventHandlerList$removeHandler(id, handler) {
		logger.error(`>>> Entering removeHandler(id=${id},handler=${handler})`, { "event.action": "removeHandler", "logrite.element": element, "logrite.eventName": eventName, "logrite.handler": handler });
        /// <param name="id" type="String"></param>
        /// <param name="handler" type="Function"></param>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String},
            {name: "handler", type: Function}
        ]);
        if (e) throw e;

        var evt = this._getEvent(id);
		logger.error(`removeHandler(id=${id},handler=${handler}): evt → ${evt}`, { "event.action": "removeHandler", "logrite.element": element, "logrite.eventName": eventName, "logrite.handler": handler });
        if (!evt) return;
        Array.remove(evt, handler);
		logger.warn(`<<< Exiting removeHandler(id=${id},handler=${handler})`, { "event.action": "removeHandler", "logrite.element": element, "logrite.eventName": eventName, "logrite.handler": handler });
    }
    function Sys$EventHandlerList$getHandler(id) {
		logger.error(`>>> Entering getHandler(id=${id})`, { "event.action": "getHandler", "logrite.id": id });
        /// <param name="id" type="String"></param>
        /// <returns type="Function"></returns>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String}
        ]);
        if (e) throw e;

        var evt = this._getEvent(id);
        if (!evt || (evt.length === 0)) return null;
        evt = Array.clone(evt);
        if (!evt._handler) {
            evt._handler = function(source, args) {
				logger.error(`>>> Entering _handler(source=${source},args=${args})`, { "event.action": "_handler", "logrite.source": source, "logrite.args": args });
                for (var i = 0, l = evt.length; i < l; i++) {
                    evt[i](source, args);
					logger.warn(`<<< Exiting _handler(source=${source},args=${args})`, { "event.action": "_handler", "logrite.source": source, "logrite.args": args });
                }
            };
        }
        return evt._handler;
    }

    function Sys$EventHandlerList$_getEvent(id, create) {
		logger.error(`>>> Entering _getEvent(id=${id},create=${create})`, { "event.action": "_getEvent", "logrite.id": id, "logrite.create": create });
        if (!this._list[id]) {
            if (!create) return null;
            this._list[id] = [];
        }
		logger.warn(`<<< Exiting _getEvent(id=${id},create=${create})`, { "event.action": "_getEvent", "logrite.id": id, "logrite.create": create });
        return this._list[id];
    }
Sys.EventHandlerList.prototype = {
    addHandler: Sys$EventHandlerList$addHandler,
    removeHandler: Sys$EventHandlerList$removeHandler,
    getHandler: Sys$EventHandlerList$getHandler,

    _getEvent: Sys$EventHandlerList$_getEvent
}
Sys.EventHandlerList.registerClass('Sys.EventHandlerList');
Sys.EventArgs = function Sys$EventArgs() {
	logger.error('>>> Entering EventArgs()', { "event.action": "EventArgs" });
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting EventArgs()', { "event.action": "EventArgs" });
}
Sys.EventArgs.registerClass('Sys.EventArgs');

Sys.EventArgs.Empty = new Sys.EventArgs();
Sys.CancelEventArgs = function Sys$CancelEventArgs() {
	logger.error('>>> Entering CancelEventArgs()', { "event.action": "CancelEventArgs" });
    if (arguments.length !== 0) throw Error.parameterCount();
    Sys.CancelEventArgs.initializeBase(this);

    this._cancel = false;
	logger.debug(`CancelEventArgs(): this._cancel → ${this._cancel}`, { "event.action": "CancelEventArgs" });
}


    function Sys$CancelEventArgs$get_cancel() {
		logger.error('>>> Entering get_cancel()', { "event.action": "get_cancel" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_cancel()', { "event.action": "get_cancel" });
        return this._cancel;
    }
    function Sys$CancelEventArgs$set_cancel(value) {
		logger.error(`>>> Entering set_cancel(value=${value})`, { "event.action": "set_cancel", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;
		logger.error(`set_cancel(value=${value}): e → ${e}`, { "event.action": "set_cancel", "logrite.value": value });

        this._cancel = value;
		logger.debug(`set_cancel(value=${value}): this._cancel → ${this._cancel}`, { "event.action": "set_cancel", "logrite.value": value });
    }
Sys.CancelEventArgs.prototype = {
    get_cancel: Sys$CancelEventArgs$get_cancel,
    set_cancel: Sys$CancelEventArgs$set_cancel
}

Sys.CancelEventArgs.registerClass('Sys.CancelEventArgs', Sys.EventArgs);
Sys.INotifyPropertyChange = function Sys$INotifyPropertyChange() {
	logger.error('>>> Entering INotifyPropertyChange()', { "event.action": "INotifyPropertyChange" });
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting INotifyPropertyChange()', { "event.action": "INotifyPropertyChange" });
    throw Error.notImplemented();
}

    function Sys$INotifyPropertyChange$add_propertyChanged(handler) {
	logger.error(`>>> Entering add_propertyChanged(handler=${handler})`, { "event.action": "add_propertyChanged", "logrite.handler": handler });
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;
	logger.error(`add_propertyChanged(handler=${handler}): e → ${e}`, { "event.action": "add_propertyChanged", "logrite.handler": handler });

		logger.warn(`<<< Exiting add_propertyChanged(handler=${handler})`, { "event.action": "add_propertyChanged", "logrite.handler": handler });
        throw Error.notImplemented();
    }
    function Sys$INotifyPropertyChange$remove_propertyChanged(handler) {
	logger.error(`>>> Entering remove_propertyChanged(handler=${handler})`, { "event.action": "remove_propertyChanged", "logrite.handler": handler });
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;
	logger.error(`remove_propertyChanged(handler=${handler}): e → ${e}`, { "event.action": "remove_propertyChanged", "logrite.handler": handler });

		logger.warn(`<<< Exiting remove_propertyChanged(handler=${handler})`, { "event.action": "remove_propertyChanged", "logrite.handler": handler });
        throw Error.notImplemented();
    }
Sys.INotifyPropertyChange.prototype = {
    add_propertyChanged: Sys$INotifyPropertyChange$add_propertyChanged,
    remove_propertyChanged: Sys$INotifyPropertyChange$remove_propertyChanged
}
Sys.INotifyPropertyChange.registerInterface('Sys.INotifyPropertyChange');
Sys.PropertyChangedEventArgs = function Sys$PropertyChangedEventArgs(propertyName) {
	logger.error(`>>> Entering PropertyChangedEventArgs(propertyName=${propertyName})`, { "event.action": "PropertyChangedEventArgs", "logrite.propertyName": propertyName });
    /// <param name="propertyName" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "propertyName", type: String}
    ]);
    if (e) throw e;

    Sys.PropertyChangedEventArgs.initializeBase(this);
    this._propertyName = propertyName;
	logger.debug(`PropertyChangedEventArgs(propertyName=${propertyName}): this._propertyName → ${this._propertyName}`, { "event.action": "PropertyChangedEventArgs", "logrite.propertyName": propertyName });
}
 
    function Sys$PropertyChangedEventArgs$get_propertyName() {
		logger.error('>>> Entering get_propertyName()', { "event.action": "get_propertyName" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_propertyName()', { "event.action": "get_propertyName" });
        return this._propertyName;
    }
Sys.PropertyChangedEventArgs.prototype = {
    get_propertyName: Sys$PropertyChangedEventArgs$get_propertyName
}
Sys.PropertyChangedEventArgs.registerClass('Sys.PropertyChangedEventArgs', Sys.EventArgs);
Sys.INotifyDisposing = function Sys$INotifyDisposing() {
	logger.error('>>> Entering INotifyDisposing()', { "event.action": "INotifyDisposing" });
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting INotifyDisposing()', { "event.action": "INotifyDisposing" });
    throw Error.notImplemented();
}

    function Sys$INotifyDisposing$add_disposing(handler) {
	logger.error(`>>> Entering add_disposing(handler=${handler})`, { "event.action": "add_disposing", "logrite.handler": handler });
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;
	logger.error(`add_disposing(handler=${handler}): e → ${e}`, { "event.action": "add_disposing", "logrite.handler": handler });

		logger.warn(`<<< Exiting add_disposing(handler=${handler})`, { "event.action": "add_disposing", "logrite.handler": handler });
        throw Error.notImplemented();
    }
    function Sys$INotifyDisposing$remove_disposing(handler) {
	logger.error(`>>> Entering remove_disposing(handler=${handler})`, { "event.action": "remove_disposing", "logrite.handler": handler });
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;
	logger.error(`remove_disposing(handler=${handler}): e → ${e}`, { "event.action": "remove_disposing", "logrite.handler": handler });

		logger.warn(`<<< Exiting remove_disposing(handler=${handler})`, { "event.action": "remove_disposing", "logrite.handler": handler });
        throw Error.notImplemented();
    }
Sys.INotifyDisposing.prototype = {
    add_disposing: Sys$INotifyDisposing$add_disposing,
    remove_disposing: Sys$INotifyDisposing$remove_disposing
}
Sys.INotifyDisposing.registerInterface("Sys.INotifyDisposing");
Sys.Component = function Sys$Component() {
	logger.error('>>> Entering Component()', { "event.action": "Component" });
    if (arguments.length !== 0) throw Error.parameterCount();
    if (Sys.Application) Sys.Application.registerDisposableObject(this);
	logger.warn('<<< Exiting Component()', { "event.action": "Component" });
}





    function Sys$Component$get_events() {
		logger.error('>>> Entering get_events()', { "event.action": "get_events" });
        /// <value type="Sys.EventHandlerList"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
		logger.warn('<<< Exiting get_events()', { "event.action": "get_events" });
        return this._events;
    }
    function Sys$Component$get_id() {
		logger.error('>>> Entering get_id()', { "event.action": "get_id" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_id()', { "event.action": "get_id" });
        return this._id;
    }
    function Sys$Component$set_id(value) {
		logger.error(`>>> Entering set_id(value=${value})`, { "event.action": "set_id", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;
		logger.error(`set_id(value=${value}): e → ${e}`, { "event.action": "set_id", "logrite.value": value });

        if (this._idSet) throw Error.invalidOperation(Sys.Res.componentCantSetIdTwice);
        this._idSet = true;
        var oldId = this.get_id();
		logger.error(`set_id(value=${value}): oldId → ${oldId}`, { "event.action": "set_id", "logrite.value": value });
		logger.debug(`set_id(value=${value}): this._idSet → ${this._idSet}`, { "event.action": "set_id", "logrite.value": value });
		logger.debug(`set_id(value=${value}): this._id → ${this._id}`, { "event.action": "set_id", "logrite.value": value });
        if (oldId && Sys.Application.findComponent(oldId)) throw Error.invalidOperation(Sys.Res.componentCantSetIdAfterAddedToApp);
        this._id = value;
    }
    function Sys$Component$get_isInitialized() {
		logger.error('>>> Entering get_isInitialized()', { "event.action": "get_isInitialized" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_isInitialized()', { "event.action": "get_isInitialized" });
        return this._initialized;
    }
    function Sys$Component$get_isUpdating() {
		logger.error('>>> Entering get_isUpdating()', { "event.action": "get_isUpdating" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_isUpdating()', { "event.action": "get_isUpdating" });
        return this._updating;
    }
    function Sys$Component$add_disposing(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("disposing", handler);
    }
    function Sys$Component$remove_disposing(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("disposing", handler);
    }
    function Sys$Component$add_propertyChanged(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("propertyChanged", handler);
    }
    function Sys$Component$remove_propertyChanged(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("propertyChanged", handler);
    }
    function Sys$Component$beginUpdate() {
		logger.error('>>> Entering beginUpdate()', { "event.action": "beginUpdate" });
        this._updating = true;
		logger.debug(`beginUpdate(): this._updating → ${this._updating}`, { "event.action": "beginUpdate" });
    }
    function Sys$Component$dispose() {
        if (this._events) {
            var handler = this._events.getHandler("disposing");
			logger.error(`dispose(): handler → ${handler}`, { "event.action": "dispose" });
            if (handler) {
                handler(this, Sys.EventArgs.Empty);
            }
        }
        delete this._events;
        Sys.Application.unregisterDisposableObject(this);
        Sys.Application.removeComponent(this);
    }
    function Sys$Component$endUpdate() {
		logger.error('>>> Entering endUpdate()', { "event.action": "endUpdate" });
        this._updating = false;
        if (!this._initialized) this.initialize();
        this.updated();
		logger.debug(`endUpdate(): this._updating → ${this._updating}`, { "event.action": "endUpdate" });
    }
    function Sys$Component$initialize() {
		logger.error('>>> Entering initialize()', { "event.action": "initialize" });
        this._initialized = true;
		logger.debug(`initialize(): this._initialized → ${this._initialized}`, { "event.action": "initialize" });
    }
    function Sys$Component$raisePropertyChanged(propertyName) {
		logger.error(`>>> Entering raisePropertyChanged(propertyName=${propertyName})`, { "event.action": "raisePropertyChanged", "logrite.propertyName": propertyName });
        /// <param name="propertyName" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "propertyName", type: String}
        ]);
        if (e) throw e;

        if (!this._events) return;
        var handler = this._events.getHandler("propertyChanged");
		logger.error(`raisePropertyChanged(propertyName=${propertyName}): handler → ${handler}`, { "event.action": "raisePropertyChanged", "logrite.propertyName": propertyName });
        if (handler) {
            handler(this, new Sys.PropertyChangedEventArgs(propertyName));
			logger.warn(`<<< Exiting raisePropertyChanged(propertyName=${propertyName})`, { "event.action": "raisePropertyChanged", "logrite.propertyName": propertyName });
        }
    }
    function Sys$Component$updated() {
	logger.error('>>> Entering updated()', { "event.action": "updated" });
    }
Sys.Component.prototype = {
    _id: null,
    _idSet: false,
    _initialized: false,
    _updating: false,
    get_events: Sys$Component$get_events,
    get_id: Sys$Component$get_id,
    set_id: Sys$Component$set_id,
    get_isInitialized: Sys$Component$get_isInitialized,
    get_isUpdating: Sys$Component$get_isUpdating,
    add_disposing: Sys$Component$add_disposing,
    remove_disposing: Sys$Component$remove_disposing,
    add_propertyChanged: Sys$Component$add_propertyChanged,
    remove_propertyChanged: Sys$Component$remove_propertyChanged,
    beginUpdate: Sys$Component$beginUpdate,
    dispose: Sys$Component$dispose,
    endUpdate: Sys$Component$endUpdate,
    initialize: Sys$Component$initialize,
    raisePropertyChanged: Sys$Component$raisePropertyChanged,
    updated: Sys$Component$updated
}
Sys.Component.registerClass('Sys.Component', null, Sys.IDisposable, Sys.INotifyPropertyChange, Sys.INotifyDisposing);

function Sys$Component$_setProperties(target, properties) {
	logger.error(`>>> Entering _setProperties(target=${target},properties=${properties})`, { "event.action": "_setProperties", "logrite.target": target, "logrite.properties": properties });
    /// <param name="target"></param>
    /// <param name="properties"></param>
    var e = Function._validateParams(arguments, [
        {name: "target"},
        {name: "properties"}
    ]);
    if (e) throw e;

    var current;
    var targetType = Object.getType(target);
	logger.error(`_setProperties(target=${target},properties=${properties}): targetType → ${targetType}`, { "event.action": "_setProperties", "logrite.target": target, "logrite.properties": properties });
    var isObject = (targetType === Object) || (targetType === Sys.UI.DomElement);
	logger.error(`_setProperties(target=${target},properties=${properties}): isObject → ${isObject}`, { "event.action": "_setProperties", "logrite.target": target, "logrite.properties": properties });
    var isComponent = Sys.Component.isInstanceOfType(target) && !target.get_isUpdating();
	logger.error(`_setProperties(target=${target},properties=${properties}): isComponent → ${isComponent}`, { "event.action": "_setProperties", "logrite.target": target, "logrite.properties": properties });
    if (isComponent) target.beginUpdate();
    for (var name in properties) {
        var val = properties[name];
        var getter = isObject ? null : target["get_" + name];
		logger.error(`_setProperties(target=${target},properties=${properties}): getter → ${getter}`, { "event.action": "_setProperties", "logrite.target": target, "logrite.properties": properties });
        if (isObject || typeof(getter) !== 'function') {
                        var targetVal = target[name];
            if (!isObject && typeof(targetVal) === 'undefined') throw Error.invalidOperation(String.format(Sys.Res.propertyUndefined, name));
            if (!val || (typeof(val) !== 'object') || (isObject && !targetVal)) {
                target[name] = val;
            }
            else {
                Sys$Component$_setProperties(targetVal, val);
            }
        }
        else {
            var setter = target["set_" + name];
			logger.error(`_setProperties(target=${target},properties=${properties}): setter → ${setter}`, { "event.action": "_setProperties", "logrite.target": target, "logrite.properties": properties });
            if (typeof(setter) === 'function') {
                                setter.apply(target, [val]);
								logger.warn(`<<< Exiting _setProperties(target=${target},properties=${properties})`, { "event.action": "_setProperties", "logrite.target": target, "logrite.properties": properties });
            }
            else if (val instanceof Array) {
                                current = getter.apply(target);
                if (!(current instanceof Array)) throw new Error.invalidOperation(String.format(Sys.Res.propertyNotAnArray, name));
                for (var i = 0, j = current.length, l= val.length; i < l; i++, j++) {
                    current[j] = val[i];
                }
            }
            else if ((typeof(val) === 'object') && (Object.getType(val) === Object)) {
                                current = getter.apply(target);
                if ((typeof(current) === 'undefined') || (current === null)) throw new Error.invalidOperation(String.format(Sys.Res.propertyNullOrUndefined, name));
                Sys$Component$_setProperties(current, val);
            }
            else {
                                throw new Error.invalidOperation(String.format(Sys.Res.propertyNotWritable, name));
            }
        }
    }
    if (isComponent) target.endUpdate();
}

function Sys$Component$_setReferences(component, references) {
	logger.error(`>>> Entering _setReferences(component=${component},references=${references})`, { "event.action": "_setReferences", "logrite.component": component, "logrite.references": references });
    for (var name in references) {
        var setter = component["set_" + name];
		logger.error(`_setReferences(component=${component},references=${references}): setter → ${setter}`, { "event.action": "_setReferences", "logrite.component": component, "logrite.references": references });
        var reference = $find(references[name]);
		logger.error(`_setReferences(component=${component},references=${references}): reference → ${reference}`, { "event.action": "_setReferences", "logrite.component": component, "logrite.references": references });
        if (typeof(setter) !== 'function') throw new Error.invalidOperation(String.format(Sys.Res.propertyNotWritable, name));
        if (!reference) throw Error.invalidOperation(String.format(Sys.Res.referenceNotFound, references[name]));
        setter.apply(component, [reference]);
		logger.warn(`<<< Exiting _setReferences(component=${component},references=${references})`, { "event.action": "_setReferences", "logrite.component": component, "logrite.references": references });
    }
}

var $create = Sys.Component.create = function Sys$Component$create(type, properties, events, references, element) {
    /// <param name="type" type="Type"></param>
    /// <param name="properties" optional="true" mayBeNull="true"></param>
    /// <param name="events" optional="true" mayBeNull="true"></param>
    /// <param name="references" optional="true" mayBeNull="true"></param>
    /// <param name="element" domElement="true" optional="true" mayBeNull="true"></param>
    /// <returns type="Sys.UI.Component"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", type: Type},
        {name: "properties", mayBeNull: true, optional: true},
        {name: "events", mayBeNull: true, optional: true},
        {name: "references", mayBeNull: true, optional: true},
        {name: "element", mayBeNull: true, domElement: true, optional: true}
    ]);
    if (e) throw e;

    if (!type.inheritsFrom(Sys.Component)) {
        throw Error.argument('type', String.format(Sys.Res.createNotComponent, type.getName()));
    }
    if (type.inheritsFrom(Sys.UI.Behavior) || type.inheritsFrom(Sys.UI.Control)) {
        if (!element) throw Error.argument('element', Sys.Res.createNoDom);
    }
    else if (element) throw Error.argument('element', Sys.Res.createComponentOnDom);
    var component = (element ? new type(element): new type());
    var app = Sys.Application;
    var creatingComponents = app.get_isCreatingComponents();

    component.beginUpdate();
    if (properties) {
        Sys$Component$_setProperties(component, properties);
    }
    if (events) {
        for (var name in events) {
            if (!(component["add_" + name] instanceof Function)) throw new Error.invalidOperation(String.format(Sys.Res.undefinedEvent, name));
            if (!(events[name] instanceof Function)) throw new Error.invalidOperation(Sys.Res.eventHandlerNotFunction);
            component["add_" + name](events[name]);
        }
    }

    app._createdComponents[app._createdComponents.length] = component;
    if (component.get_id()) {
        app.addComponent(component);
    }
    if (creatingComponents) {
        if (references) {
            app._addComponentToSecondPass(component, references);
        }
        else {
            component.endUpdate();
        }
    }
    else {
        if (references) {
            Sys$Component$_setReferences(component, references);
        }
        component.endUpdate();
    }

    return component;
}
Sys.UI.MouseButton = function Sys$UI$MouseButton() {
	logger.error('>>> Entering MouseButton()', { "event.action": "MouseButton" });
    /// <field name="leftButton" type="Number" integer="true" static="true"></field>
    /// <field name="middleButton" type="Number" integer="true" static="true"></field>
    /// <field name="rightButton" type="Number" integer="true" static="true"></field>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting MouseButton()', { "event.action": "MouseButton" });
    throw Error.notImplemented();
}




Sys.UI.MouseButton.prototype = {
    leftButton: 0,
    middleButton: 1,
    rightButton: 2
}
Sys.UI.MouseButton.registerEnum("Sys.UI.MouseButton");
Sys.UI.Key = function Sys$UI$Key() {
	logger.error('>>> Entering Key()', { "event.action": "Key" });
    /// <field name="backspace" type="Number" integer="true" static="true"></field>
    /// <field name="tab" type="Number" integer="true" static="true"></field>
    /// <field name="enter" type="Number" integer="true" static="true"></field>
    /// <field name="esc" type="Number" integer="true" static="true"></field>
    /// <field name="space" type="Number" integer="true" static="true"></field>
    /// <field name="pageUp" type="Number" integer="true" static="true"></field>
    /// <field name="pageDown" type="Number" integer="true" static="true"></field>
    /// <field name="end" type="Number" integer="true" static="true"></field>
    /// <field name="home" type="Number" integer="true" static="true"></field>
    /// <field name="left" type="Number" integer="true" static="true"></field>
    /// <field name="up" type="Number" integer="true" static="true"></field>
    /// <field name="right" type="Number" integer="true" static="true"></field>
    /// <field name="down" type="Number" integer="true" static="true"></field>
    /// <field name="del" type="Number" integer="true" static="true"></field>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting Key()', { "event.action": "Key" });
    throw Error.notImplemented();
}















Sys.UI.Key.prototype = {
    backspace: 8,
    tab: 9,
    enter: 13,
    esc: 27,
    space: 32,
    pageUp: 33,
    pageDown: 34,
    end: 35,
    home: 36,
    left: 37,
    up: 38,
    right: 39,
    down: 40,
    del: 127
}
Sys.UI.Key.registerEnum("Sys.UI.Key");
Sys.UI.DomEvent = function Sys$UI$DomEvent(eventObject) {
	logger.error(`>>> Entering DomEvent(eventObject=${eventObject})`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
    /// <param name="eventObject"></param>
    /// <field name="altKey" type="Boolean"></field>
    /// <field name="button" type="Sys.UI.MouseButton"></field>
    /// <field name="charCode" type="Number" integer="true"></field>
    /// <field name="clientX" type="Number" integer="true"></field>
    /// <field name="clientY" type="Number" integer="true"></field>
    /// <field name="ctrlKey" type="Boolean"></field>
    /// <field name="keyCode" type="Number" integer="true"></field>
    /// <field name="offsetX" type="Number" integer="true"></field>
    /// <field name="offsetY" type="Number" integer="true"></field>
    /// <field name="screenX" type="Number" integer="true"></field>
    /// <field name="screenY" type="Number" integer="true"></field>
    /// <field name="shiftKey" type="Boolean"></field>
    /// <field name="target" domElement="true"></field>
    /// <field name="type" type="String"></field>
    var e = Function._validateParams(arguments, [
        {name: "eventObject"}
    ]);
    if (e) throw e;

    var e = eventObject;
    this.rawEvent = e;
    this.altKey = e.altKey;
	logger.debug(`DomEvent(eventObject=${eventObject}): this.rawEvent → ${this.rawEvent}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
    if (typeof(e.button) !== 'undefined') {
        this.button = (typeof(e.which) !== 'undefined') ? e.button :
			logger.debug(`DomEvent(eventObject=${eventObject}): this.altKey → ${this.altKey}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
            (e.button === 4) ? Sys.UI.MouseButton.middleButton :
            (e.button === 2) ? Sys.UI.MouseButton.rightButton :
            Sys.UI.MouseButton.leftButton;
    }
    if (e.type === 'keypress') {
        this.charCode = e.charCode || e.keyCode;
    }
    else if (e.keyCode && (e.keyCode === 46)) {
		logger.debug(`DomEvent(eventObject=${eventObject}): this.charCode → ${this.charCode}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
        this.keyCode = 127;
    }
    else {
        this.keyCode = e.keyCode;
		logger.debug(`DomEvent(eventObject=${eventObject}): this.keyCode → ${this.keyCode}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
    }
    this.clientX = e.clientX;
    this.clientY = e.clientY;
	logger.debug(`DomEvent(eventObject=${eventObject}): this.clientX → ${this.clientX}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
    this.ctrlKey = e.ctrlKey;
    this.target = e.target ? e.target : e.srcElement;
	logger.debug(`DomEvent(eventObject=${eventObject}): this.clientY → ${this.clientY}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
    if (this.target) {
        var loc = Sys.UI.DomElement.getLocation(this.target);
		logger.error(`DomEvent(eventObject=${eventObject}): loc → ${loc}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
		logger.debug(`DomEvent(eventObject=${eventObject}): this.ctrlKey → ${this.ctrlKey}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
        this.offsetX = (typeof(e.offsetX) !== 'undefined') ? e.offsetX : window.pageXOffset + (e.clientX || 0) - loc.x;
        this.offsetY = (typeof(e.offsetY) !== 'undefined') ? e.offsetY : window.pageYOffset + (e.clientY || 0) - loc.y;
    }
    this.screenX = e.screenX;
	logger.debug(`DomEvent(eventObject=${eventObject}): this.target → ${this.target}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
	logger.debug(`DomEvent(eventObject=${eventObject}): this.screenX → ${this.screenX}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
    this.screenY = e.screenY;
    this.shiftKey = e.shiftKey;
    this.type = e.type;
	logger.debug(`DomEvent(eventObject=${eventObject}): this.type → ${this.type}`, { "event.action": "DomEvent", "logrite.eventObject": eventObject });
}

    function Sys$UI$DomEvent$preventDefault() {
		logger.error('>>> Entering preventDefault()', { "event.action": "preventDefault" });
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this.rawEvent.preventDefault) {
            this.rawEvent.preventDefault();
			logger.warn('<<< Exiting preventDefault()', { "event.action": "preventDefault" });
        }
        else if (window.event) {
            window.event.returnValue = false;
        }
    }
    function Sys$UI$DomEvent$stopPropagation() {
		logger.error('>>> Entering stopPropagation()', { "event.action": "stopPropagation" });
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this.rawEvent.stopPropagation) {
            this.rawEvent.stopPropagation();
			logger.warn('<<< Exiting stopPropagation()', { "event.action": "stopPropagation" });
        }
        else if (window.event) {
            window.event.cancelBubble = true;
        }
    }
Sys.UI.DomEvent.prototype = {
    preventDefault: Sys$UI$DomEvent$preventDefault,
    stopPropagation: Sys$UI$DomEvent$stopPropagation
}
Sys.UI.DomEvent.registerClass('Sys.UI.DomEvent');

var $addHandler = Sys.UI.DomEvent.addHandler = function Sys$UI$DomEvent$addHandler(element, eventName, handler) {
    /// <param name="element" domElement="true"></param>
    /// <param name="eventName" type="String"></param>
    /// <param name="handler" type="Function"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "eventName", type: String},
        {name: "handler", type: Function}
    ]);
    if (e) throw e;

    if (!element._events) {
        element._events = {};
		logger.debug(`addHandler(element=${element},eventName=${eventName},handler=${handler}): element._events → ${element._events}`, { "event.action": "addHandler", "logrite.element": element, "logrite.eventName": eventName, "logrite.handler": handler });
    }
    var eventCache = element._events[eventName];
    if (!eventCache) {
        element._events[eventName] = eventCache = [];
    }
    var browserHandler;
    if (element.addEventListener) {
        browserHandler = function(e) {
			logger.error(`>>> Entering browserHandler(e=${e})`, { "event.action": "browserHandler" });
			logger.warn(`<<< Exiting browserHandler(e=${e})`, { "event.action": "browserHandler" });
            return handler.call(element, new Sys.UI.DomEvent(e));
        }
        element.addEventListener(eventName, browserHandler, false);
    }
    else if (element.attachEvent) {
        browserHandler = function() {
            return handler.call(element, new Sys.UI.DomEvent(window.event));
        }
        element.attachEvent('on' + eventName, browserHandler);
    }
    eventCache[eventCache.length] = {handler: handler, browserHandler: browserHandler};
}

var $addHandlers = Sys.UI.DomEvent.addHandlers = function Sys$UI$DomEvent$addHandlers(element, events, handlerOwner) {
	logger.error(`>>> Entering addHandlers(element=${element},events=${events},handlerOwner=${handlerOwner})`, { "event.action": "addHandlers", "logrite.element": element, "logrite.events": events, "logrite.handlerOwner": handlerOwner });
    /// <param name="element" domElement="true"></param>
    /// <param name="events" type="Object"></param>
    /// <param name="handlerOwner" optional="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "events", type: Object},
        {name: "handlerOwner", optional: true}
    ]);
    if (e) throw e;

    for (var name in events) {
        var handler = events[name];
        if (typeof(handler) !== 'function') throw Error.invalidOperation(Sys.Res.cantAddNonFunctionhandler);
        if (handlerOwner) {
            handler = Function.createDelegate(handlerOwner, handler);
        }
        $addHandler(element, name, handler);
		logger.error(`addHandlers(element=${element},events=${events},handlerOwner=${handlerOwner}): handler → ${handler}`, { "event.action": "addHandlers", "logrite.element": element, "logrite.events": events, "logrite.handlerOwner": handlerOwner });
    }
}

var $clearHandlers = Sys.UI.DomEvent.clearHandlers = function Sys$UI$DomEvent$clearHandlers(element) {
	logger.error(`>>> Entering clearHandlers(element=${element})`, { "event.action": "clearHandlers", "logrite.element": element });
    /// <param name="element" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    if (element._events) {
        var cache = element._events;
        for (var name in cache) {
            var handlers = cache[name];
            for (var i = handlers.length - 1; i >= 0; i--) {
                $removeHandler(element, name, handlers[i].handler);
            }
        }
        element._events = null;
		logger.warn(`<<< Exiting clearHandlers(element=${element})`, { "event.action": "clearHandlers", "logrite.element": element });
    }
}

var $removeHandler = Sys.UI.DomEvent.removeHandler = function Sys$UI$DomEvent$removeHandler(element, eventName, handler) {
    /// <param name="element" domElement="true"></param>
    /// <param name="eventName" type="String"></param>
    /// <param name="handler" type="Function"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "eventName", type: String},
        {name: "handler", type: Function}
    ]);
    if (e) throw e;

    var browserHandler = null;
    if ((typeof(element._events) !== 'object') || (element._events == null)) throw Error.invalidOperation(Sys.Res.eventHandlerInvalid);
    var cache = element._events[eventName];
    if (!(cache instanceof Array)) throw Error.invalidOperation(Sys.Res.eventHandlerInvalid);
    var browserHandler = null;
    for (var i = 0, l = cache.length; i < l; i++) {
        if (cache[i].handler === handler) {
            browserHandler = cache[i].browserHandler;
            break;
        }
    }
    if (typeof(browserHandler) !== 'function') throw Error.invalidOperation(Sys.Res.eventHandlerInvalid);
    if (element.removeEventListener) {
        element.removeEventListener(eventName, browserHandler, false);
    }
    else if (element.detachEvent) {
        element.detachEvent('on' + eventName, browserHandler);
    }
    cache.splice(i, 1);
}
Sys.IContainer = function Sys$IContainer() {
	logger.error('>>> Entering IContainer()', { "event.action": "IContainer" });
	logger.warn('<<< Exiting IContainer()', { "event.action": "IContainer" });
    throw Error.notImplemented();
}

    function Sys$IContainer$addComponent(component) {
		logger.error(`>>> Entering addComponent(component=${component})`, { "event.action": "addComponent", "logrite.component": component });
        /// <param name="component" type="Sys.Component"></param>
        var e = Function._validateParams(arguments, [
            {name: "component", type: Sys.Component}
        ]);
        if (e) throw e;

		logger.warn(`<<< Exiting addComponent(component=${component})`, { "event.action": "addComponent", "logrite.component": component });
        throw Error.notImplemented();
    }
    function Sys$IContainer$removeComponent(component) {
		logger.error(`>>> Entering removeComponent(component=${component})`, { "event.action": "removeComponent", "logrite.component": component });
        /// <param name="component" type="Sys.Component"></param>
        var e = Function._validateParams(arguments, [
            {name: "component", type: Sys.Component}
        ]);
        if (e) throw e;

		logger.warn(`<<< Exiting removeComponent(component=${component})`, { "event.action": "removeComponent", "logrite.component": component });
        throw Error.notImplemented();
    }
    function Sys$IContainer$findComponent(id) {
		logger.error(`>>> Entering findComponent(id=${id})`, { "event.action": "findComponent", "logrite.id": id, "logrite.parent": parent });
        /// <param name="id" type="String"></param>
        /// <returns type="Sys.Component"></returns>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String}
        ]);
        if (e) throw e;

		logger.warn(`<<< Exiting findComponent(id=${id})`, { "event.action": "findComponent", "logrite.id": id, "logrite.parent": parent });
        throw Error.notImplemented();
    }
    function Sys$IContainer$getComponents() {
		logger.error('>>> Entering getComponents()', { "event.action": "getComponents" });
        /// <returns type="Array" elementType="Sys.Component"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting getComponents()', { "event.action": "getComponents" });
        throw Error.notImplemented();
    }
Sys.IContainer.prototype = {
    addComponent: Sys$IContainer$addComponent,
    removeComponent: Sys$IContainer$removeComponent,
    findComponent: Sys$IContainer$findComponent,
    getComponents: Sys$IContainer$getComponents
}
Sys.IContainer.registerInterface("Sys.IContainer");


Sys._ScriptLoader = function Sys$_ScriptLoader() {
	logger.error('>>> Entering _ScriptLoader()', { "event.action": "_ScriptLoader" });
    this._scriptsToLoad = null;
    this._scriptLoadedDelegate = Function.createDelegate(this, this._scriptLoadedHandler);
	logger.debug(`_ScriptLoader(): this._scriptsToLoad → ${this._scriptsToLoad}`, { "event.action": "_ScriptLoader" });
}

    function Sys$_ScriptLoader$dispose() {
        this._stopLoading();
        if(this._events) {
            delete this._events;
        }
        this._scriptLoadedDelegate = null;        
		logger.debug(`dispose(): this._scriptLoadedDelegate → ${this._scriptLoadedDelegate}`, { "event.action": "dispose" });
    }

    function Sys$_ScriptLoader$loadScripts(scriptTimeout, allScriptsLoadedCallback, scriptLoadFailedCallback, scriptLoadTimeoutCallback) {
		logger.error(`>>> Entering loadScripts(scriptTimeout=${scriptTimeout},allScriptsLoadedCallback=${allScriptsLoadedCallback},scriptLoadFailedCallback=${scriptLoadFailedCallback},scriptLoadTimeoutCallback=${scriptLoadTimeoutCallback})`, { "event.action": "loadScripts", "logrite.scriptTimeout": scriptTimeout, "logrite.allScriptsLoadedCallback": allScriptsLoadedCallback, "logrite.scriptLoadFailedCallback": scriptLoadFailedCallback, "logrite.scriptLoadTimeoutCallback": scriptLoadTimeoutCallback });
        /// <param name="scriptTimeout" type="Number" integer="true"></param>
        /// <param name="allScriptsLoadedCallback" type="Function" mayBeNull="true"></param>
        /// <param name="scriptLoadFailedCallback" type="Function" mayBeNull="true"></param>
        /// <param name="scriptLoadTimeoutCallback" type="Function" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "scriptTimeout", type: Number, integer: true},
            {name: "allScriptsLoadedCallback", type: Function, mayBeNull: true},
            {name: "scriptLoadFailedCallback", type: Function, mayBeNull: true},
            {name: "scriptLoadTimeoutCallback", type: Function, mayBeNull: true}
        ]);
        if (e) throw e;

        if(this._loading) {
            throw Error.invalidOperation(Sys.Res.scriptLoaderAlreadyLoading);
        }
        this._loading = true;
        this._allScriptsLoadedCallback = allScriptsLoadedCallback;
		logger.debug(`loadScripts(scriptTimeout=${scriptTimeout},allScriptsLoadedCallback=${allScriptsLoadedCallback},scriptLoadFailedCallback=${scriptLoadFailedCallback},scriptLoadTimeoutCallback=${scriptLoadTimeoutCallback}): this._loading → ${this._loading}`, { "event.action": "loadScripts", "logrite.scriptTimeout": scriptTimeout, "logrite.allScriptsLoadedCallback": allScriptsLoadedCallback, "logrite.scriptLoadFailedCallback": scriptLoadFailedCallback, "logrite.scriptLoadTimeoutCallback": scriptLoadTimeoutCallback });
        this._scriptLoadFailedCallback = scriptLoadFailedCallback;
        this._scriptLoadTimeoutCallback = scriptLoadTimeoutCallback;
		logger.debug(`loadScripts(scriptTimeout=${scriptTimeout},allScriptsLoadedCallback=${allScriptsLoadedCallback},scriptLoadFailedCallback=${scriptLoadFailedCallback},scriptLoadTimeoutCallback=${scriptLoadTimeoutCallback}): this._allScriptsLoadedCallback → ${this._allScriptsLoadedCallback}`, { "event.action": "loadScripts", "logrite.scriptTimeout": scriptTimeout, "logrite.allScriptsLoadedCallback": allScriptsLoadedCallback, "logrite.scriptLoadFailedCallback": scriptLoadFailedCallback, "logrite.scriptLoadTimeoutCallback": scriptLoadTimeoutCallback });
        
        this._loadScriptsInternal();
		logger.debug(`loadScripts(scriptTimeout=${scriptTimeout},allScriptsLoadedCallback=${allScriptsLoadedCallback},scriptLoadFailedCallback=${scriptLoadFailedCallback},scriptLoadTimeoutCallback=${scriptLoadTimeoutCallback}): this._scriptLoadFailedCallback → ${this._scriptLoadFailedCallback}`, { "event.action": "loadScripts", "logrite.scriptTimeout": scriptTimeout, "logrite.allScriptsLoadedCallback": allScriptsLoadedCallback, "logrite.scriptLoadFailedCallback": scriptLoadFailedCallback, "logrite.scriptLoadTimeoutCallback": scriptLoadTimeoutCallback });
    }

    function Sys$_ScriptLoader$notifyScriptLoaded() {
		logger.error('>>> Entering notifyScriptLoaded()', { "event.action": "notifyScriptLoaded" });
        if (arguments.length !== 0) throw Error.parameterCount();
        
                        if(!this._loading) {
                                    return;
        }

        this._currentTask._notified++;
        
        if(Sys.Browser.agent === Sys.Browser.Safari) {           
            if(this._currentTask._notified === 1) {
                                                                                                                window.setTimeout(Function.createDelegate(this, function() {
                    this._scriptLoadedHandler(this._currentTask.get_scriptElement(), true);
                }), 0);
            }
        }

                            }

    function Sys$_ScriptLoader$queueCustomScriptTag(scriptAttributes) {
		logger.error(`>>> Entering queueCustomScriptTag(scriptAttributes=${scriptAttributes})`, { "event.action": "queueCustomScriptTag", "logrite.scriptAttributes": scriptAttributes });
        /// <param name="scriptAttributes" mayBeNull="false"></param>
        var e = Function._validateParams(arguments, [
            {name: "scriptAttributes"}
        ]);
        if (e) throw e;

        if(!this._scriptsToLoad) {
            this._scriptsToLoad = [];
        }
        Array.add(this._scriptsToLoad, scriptAttributes);
		logger.debug(`queueCustomScriptTag(scriptAttributes=${scriptAttributes}): this._scriptsToLoad → ${this._scriptsToLoad}`, { "event.action": "queueCustomScriptTag", "logrite.scriptAttributes": scriptAttributes });
    }

    function Sys$_ScriptLoader$queueScriptBlock(scriptContent) {
		logger.error(`>>> Entering queueScriptBlock(scriptContent=${scriptContent})`, { "event.action": "queueScriptBlock", "logrite.scriptContent": scriptContent });
        /// <param name="scriptContent" type="String" mayBeNull="false"></param>
        var e = Function._validateParams(arguments, [
            {name: "scriptContent", type: String}
        ]);
        if (e) throw e;

        if(!this._scriptsToLoad) {
            this._scriptsToLoad = [];
        }
        Array.add(this._scriptsToLoad, {text: scriptContent});
		logger.debug(`queueScriptBlock(scriptContent=${scriptContent}): this._scriptsToLoad → ${this._scriptsToLoad}`, { "event.action": "queueScriptBlock", "logrite.scriptContent": scriptContent });
    }

    function Sys$_ScriptLoader$queueScriptReference(scriptUrl) {
		logger.error(`>>> Entering queueScriptReference(scriptUrl=${scriptUrl})`, { "event.action": "queueScriptReference", "logrite.scriptUrl": scriptUrl });
        /// <param name="scriptUrl" type="String" mayBeNull="false"></param>
        var e = Function._validateParams(arguments, [
            {name: "scriptUrl", type: String}
        ]);
        if (e) throw e;

        if(!this._scriptsToLoad) {
            this._scriptsToLoad = [];
        }
        Array.add(this._scriptsToLoad, {src: scriptUrl});
		logger.debug(`queueScriptReference(scriptUrl=${scriptUrl}): this._scriptsToLoad → ${this._scriptsToLoad}`, { "event.action": "queueScriptReference", "logrite.scriptUrl": scriptUrl });
    }

    function Sys$_ScriptLoader$_createScriptElement(queuedScript) {
		logger.error(`>>> Entering _createScriptElement(queuedScript=${queuedScript})`, { "event.action": "_createScriptElement", "logrite.queuedScript": queuedScript });
        var scriptElement = document.createElement('SCRIPT');

						logger.error(`_createScriptElement(queuedScript=${queuedScript}): scriptElement → ${scriptElement}`, { "event.action": "_createScriptElement", "logrite.queuedScript": queuedScript });
                        scriptElement.type = 'text/javascript';
						logger.debug(`_createScriptElement(queuedScript=${queuedScript}): scriptElement.type → ${scriptElement.type}`, { "event.action": "_createScriptElement", "logrite.queuedScript": queuedScript });

                for (var attr in queuedScript) {
            scriptElement[attr] = queuedScript[attr];
        }
        
		logger.warn(`<<< Exiting _createScriptElement(queuedScript=${queuedScript})`, { "event.action": "_createScriptElement", "logrite.queuedScript": queuedScript });
        return scriptElement;
    }

    function Sys$_ScriptLoader$_loadScriptsInternal() {
				logger.error('>>> Entering _loadScriptsInternal()', { "event.action": "_loadScriptsInternal" });
				logger.warn('<<< Exiting _loadScriptsInternal()', { "event.action": "_loadScriptsInternal" });
                if (this._scriptsToLoad && this._scriptsToLoad.length > 0) {
            var nextScript = Array.dequeue(this._scriptsToLoad);
						logger.error(`_loadScriptsInternal(): nextScript → ${nextScript}`, { "event.action": "_loadScriptsInternal" });
                        var scriptElement = this._createScriptElement(nextScript);
						logger.error(`_loadScriptsInternal(): scriptElement → ${scriptElement}`, { "event.action": "_loadScriptsInternal" });
						logger.debug(`_loadScriptsInternal(): scriptElement.innerHTML → ${scriptElement.innerHTML}`, { "event.action": "_loadScriptsInternal" });
            
            if (scriptElement.text && Sys.Browser.agent === Sys.Browser.Safari) {
                                scriptElement.innerHTML = scriptElement.text;
                delete scriptElement.text;
            }            

                                                            if (typeof(nextScript.src) === "string") {
                                this._currentTask = new Sys._ScriptLoaderTask(scriptElement, this._scriptLoadedDelegate);
                                                                                this._currentTask.execute();
            }
            else {
                                                document.getElementsByTagName('HEAD')[0].appendChild(scriptElement);
                
                                Sys._ScriptLoader._clearScript(scriptElement);

                                                                this._loadScriptsInternal();
            }
        }
        else {
                        var callback = this._allScriptsLoadedCallback;
            this._stopLoading();
            if(callback) {
                callback(this);
            }
        }
    }

    function Sys$_ScriptLoader$_raiseError(multipleCallbacks) {
				logger.error(`>>> Entering _raiseError(multipleCallbacks=${multipleCallbacks})`, { "event.action": "_raiseError", "logrite.multipleCallbacks": multipleCallbacks });
				logger.warn(`<<< Exiting _raiseError(multipleCallbacks=${multipleCallbacks})`, { "event.action": "_raiseError", "logrite.multipleCallbacks": multipleCallbacks });
                var callback = this._scriptLoadFailedCallback;
        var scriptElement = this._currentTask.get_scriptElement();
		logger.error(`_raiseError(multipleCallbacks=${multipleCallbacks}): scriptElement → ${scriptElement}`, { "event.action": "_raiseError", "logrite.multipleCallbacks": multipleCallbacks });
        this._stopLoading();
        
        if(callback) {
            callback(this, scriptElement, multipleCallbacks);
        }
        else {
            throw Sys._ScriptLoader._errorScriptLoadFailed(scriptElement.src, multipleCallbacks);
        }
    }

    function Sys$_ScriptLoader$_scriptLoadedHandler(scriptElement, loaded) {
								logger.error(`>>> Entering _scriptLoadedHandler(scriptElement=${scriptElement},loaded=${loaded})`, { "event.action": "_scriptLoadedHandler", "logrite.scriptElement": scriptElement, "logrite.loaded": loaded });
								logger.warn(`<<< Exiting _scriptLoadedHandler(scriptElement=${scriptElement},loaded=${loaded})`, { "event.action": "_scriptLoadedHandler", "logrite.scriptElement": scriptElement, "logrite.loaded": loaded });
                                if(loaded && this._currentTask._notified) {
            if(this._currentTask._notified > 1) {
                                this._raiseError(true);
            }
            else {
                                Array.add(Sys._ScriptLoader._getLoadedScripts(), scriptElement.src);
                this._currentTask.dispose();
                this._currentTask = null;
                this._loadScriptsInternal();
				logger.debug(`_scriptLoadedHandler(scriptElement=${scriptElement},loaded=${loaded}): this._currentTask → ${this._currentTask}`, { "event.action": "_scriptLoadedHandler", "logrite.scriptElement": scriptElement, "logrite.loaded": loaded });
            }
        }
        else {
                        this._raiseError(false);
        }
    }

    function Sys$_ScriptLoader$_scriptLoadTimeoutHandler() {
		logger.error('>>> Entering _scriptLoadTimeoutHandler()', { "event.action": "_scriptLoadTimeoutHandler" });
        var callback = this._scriptLoadTimeoutCallback;
        this._stopLoading();

        if(callback) {
            callback(this);
			logger.warn('<<< Exiting _scriptLoadTimeoutHandler()', { "event.action": "_scriptLoadTimeoutHandler" });
        }
    }

    function Sys$_ScriptLoader$_stopLoading() {
		logger.error('>>> Entering _stopLoading()', { "event.action": "_stopLoading" });
        if(this._timeoutCookie) {
            window.clearTimeout(this._timeoutCookie);
            this._timeoutCookie = null;
        }

		logger.debug(`_stopLoading(): this._timeoutCookie → ${this._timeoutCookie}`, { "event.action": "_stopLoading" });
        if(this._currentTask) {
            this._currentTask.dispose();
            this._currentTask = null;
        }

		logger.debug(`_stopLoading(): this._currentTask → ${this._currentTask}`, { "event.action": "_stopLoading" });
        this._scriptsToLoad = null;
        this._loading = null;
		logger.debug(`_stopLoading(): this._scriptsToLoad → ${this._scriptsToLoad}`, { "event.action": "_stopLoading" });
        
        this._allScriptsLoadedCallback = null;
		logger.debug(`_stopLoading(): this._loading → ${this._loading}`, { "event.action": "_stopLoading" });
        this._scriptLoadFailedCallback = null;
        this._scriptLoadTimeoutCallback = null;
		logger.debug(`_stopLoading(): this._scriptLoadTimeoutCallback → ${this._scriptLoadTimeoutCallback}`, { "event.action": "_stopLoading" });
    }
Sys._ScriptLoader.prototype = {
    dispose: Sys$_ScriptLoader$dispose,
    
    loadScripts: Sys$_ScriptLoader$loadScripts,
    
    notifyScriptLoaded: Sys$_ScriptLoader$notifyScriptLoaded,
    
    queueCustomScriptTag: Sys$_ScriptLoader$queueCustomScriptTag,

    queueScriptBlock: Sys$_ScriptLoader$queueScriptBlock,

    queueScriptReference: Sys$_ScriptLoader$queueScriptReference,
    
    _createScriptElement: Sys$_ScriptLoader$_createScriptElement,   

    _loadScriptsInternal: Sys$_ScriptLoader$_loadScriptsInternal,
    
    _raiseError: Sys$_ScriptLoader$_raiseError,
    
    _scriptLoadedHandler: Sys$_ScriptLoader$_scriptLoadedHandler,
    
    _scriptLoadTimeoutHandler: Sys$_ScriptLoader$_scriptLoadTimeoutHandler,
    
    _stopLoading: Sys$_ScriptLoader$_stopLoading    
}
Sys._ScriptLoader.registerClass('Sys._ScriptLoader', null, Sys.IDisposable);

Sys._ScriptLoader.getInstance = function Sys$_ScriptLoader$getInstance() {
	logger.error('>>> Entering getInstance()', { "event.action": "getInstance" });
    var sl = Sys._ScriptLoader._activeInstance;
    if(!sl) {
        sl = Sys._ScriptLoader._activeInstance = new Sys._ScriptLoader();
    }
	logger.error(`getInstance(): sl → ${sl}`, { "event.action": "getInstance" });
    return sl;
}

Sys._ScriptLoader.isScriptLoaded = function Sys$_ScriptLoader$isScriptLoaded(scriptSrc) {
	logger.error(`>>> Entering isScriptLoaded(scriptSrc=${scriptSrc})`, { "event.action": "isScriptLoaded", "logrite.scriptSrc": scriptSrc });
    var scriptElement = document.createElement('script');
    scriptElement.src = scriptSrc;
	logger.error(`isScriptLoaded(scriptSrc=${scriptSrc}): scriptElement → ${scriptElement}`, { "event.action": "isScriptLoaded", "logrite.scriptSrc": scriptSrc });
	logger.warn(`<<< Exiting isScriptLoaded(scriptSrc=${scriptSrc})`, { "event.action": "isScriptLoaded", "logrite.scriptSrc": scriptSrc });
    return Array.contains(Sys._ScriptLoader._getLoadedScripts(), scriptElement.src);
}

Sys._ScriptLoader.readLoadedScripts = function Sys$_ScriptLoader$readLoadedScripts() {
		logger.error('>>> Entering readLoadedScripts()', { "event.action": "readLoadedScripts" });
        if(!Sys._ScriptLoader._referencedScripts) {
        var referencedScripts = Sys._ScriptLoader._referencedScripts = [];

        var existingScripts = document.getElementsByTagName('SCRIPT');
		logger.error(`readLoadedScripts(): existingScripts → ${existingScripts}`, { "event.action": "readLoadedScripts" });
        for (i = existingScripts.length - 1; i >= 0; i--) {
            var scriptNode = existingScripts[i];
            var scriptSrc = scriptNode.src;
            if (scriptSrc.length) {
                if (!Array.contains(referencedScripts, scriptSrc)) {
                    Array.add(referencedScripts, scriptSrc);
					logger.warn('<<< Exiting readLoadedScripts()', { "event.action": "readLoadedScripts" });
                }
            }
        }
    }
}

Sys._ScriptLoader._clearScript = function Sys$_ScriptLoader$_clearScript(scriptElement) {
	logger.error(`>>> Entering _clearScript(scriptElement=${scriptElement})`, { "event.action": "_clearScript", "logrite.scriptElement": scriptElement });
    if (!Sys.Debug.isDebug) {
                        scriptElement.parentNode.removeChild(scriptElement);
						logger.warn(`<<< Exiting _clearScript(scriptElement=${scriptElement})`, { "event.action": "_clearScript", "logrite.scriptElement": scriptElement });
    }
}

Sys._ScriptLoader._errorScriptLoadFailed = function Sys$_ScriptLoader$_errorScriptLoadFailed(scriptUrl, multipleCallbacks) {
	logger.error(`>>> Entering _errorScriptLoadFailed(scriptUrl=${scriptUrl},multipleCallbacks=${multipleCallbacks})`, { "event.action": "_errorScriptLoadFailed", "logrite.scriptUrl": scriptUrl, "logrite.multipleCallbacks": multipleCallbacks });
    var errorMessage;
    if(multipleCallbacks) {
        errorMessage = Sys.Res.scriptLoadMultipleCallbacks;
    }
    else {
                errorMessage = Sys.Res.scriptLoadFailedDebug;
    }

	logger.debug(`_errorScriptLoadFailed(scriptUrl=${scriptUrl},multipleCallbacks=${multipleCallbacks}): errorMessage → ${errorMessage}`, { "event.action": "_errorScriptLoadFailed", "logrite.scriptUrl": scriptUrl, "logrite.multipleCallbacks": multipleCallbacks });
    var displayMessage = "Sys.ScriptLoadFailedException: " + String.format(errorMessage, scriptUrl);
	logger.error(`_errorScriptLoadFailed(scriptUrl=${scriptUrl},multipleCallbacks=${multipleCallbacks}): displayMessage → ${displayMessage}`, { "event.action": "_errorScriptLoadFailed", "logrite.scriptUrl": scriptUrl, "logrite.multipleCallbacks": multipleCallbacks });
    var e = Error.create(displayMessage, {name: 'Sys.ScriptLoadFailedException', 'scriptUrl': scriptUrl });
	logger.error(`_errorScriptLoadFailed(scriptUrl=${scriptUrl},multipleCallbacks=${multipleCallbacks}): e → ${e}`, { "event.action": "_errorScriptLoadFailed", "logrite.scriptUrl": scriptUrl, "logrite.multipleCallbacks": multipleCallbacks });
    e.popStackFrame();
	logger.warn(`<<< Exiting _errorScriptLoadFailed(scriptUrl=${scriptUrl},multipleCallbacks=${multipleCallbacks})`, { "event.action": "_errorScriptLoadFailed", "logrite.scriptUrl": scriptUrl, "logrite.multipleCallbacks": multipleCallbacks });
    return e;
}

Sys._ScriptLoader._getLoadedScripts = function Sys$_ScriptLoader$_getLoadedScripts() {
	logger.error('>>> Entering _getLoadedScripts()', { "event.action": "_getLoadedScripts" });
    if(!Sys._ScriptLoader._referencedScripts) {
        Sys._ScriptLoader._referencedScripts = [];
        Sys._ScriptLoader.readLoadedScripts();
    }
	logger.warn('<<< Exiting _getLoadedScripts()', { "event.action": "_getLoadedScripts" });
    return Sys._ScriptLoader._referencedScripts;
}


Sys._ScriptLoaderTask = function Sys$_ScriptLoaderTask(scriptElement, completedCallback) {
	logger.error(`>>> Entering _ScriptLoaderTask(scriptElement=${scriptElement},completedCallback=${completedCallback})`, { "event.action": "_ScriptLoaderTask", "logrite.scriptElement": scriptElement, "logrite.completedCallback": completedCallback });
    /// <param name="scriptElement" domElement="true"></param>
    /// <param name="completedCallback" type="Function"></param>
    var e = Function._validateParams(arguments, [
        {name: "scriptElement", domElement: true},
        {name: "completedCallback", type: Function}
    ]);
    if (e) throw e;

    this._scriptElement = scriptElement;
    this._completedCallback = completedCallback;
	logger.debug(`_ScriptLoaderTask(scriptElement=${scriptElement},completedCallback=${completedCallback}): this._scriptElement → ${this._scriptElement}`, { "event.action": "_ScriptLoaderTask", "logrite.scriptElement": scriptElement, "logrite.completedCallback": completedCallback });
    this._notified = 0;
	logger.debug(`_ScriptLoaderTask(scriptElement=${scriptElement},completedCallback=${completedCallback}): this._notified → ${this._notified}`, { "event.action": "_ScriptLoaderTask", "logrite.scriptElement": scriptElement, "logrite.completedCallback": completedCallback });
}

    function Sys$_ScriptLoaderTask$get_scriptElement() {
		logger.error('>>> Entering get_scriptElement()', { "event.action": "get_scriptElement" });
        /// <value domElement="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_scriptElement()', { "event.action": "get_scriptElement" });
        return this._scriptElement;
    }

    function Sys$_ScriptLoaderTask$dispose() {
                if(this._disposed) {
                        return;
        }
        this._disposed = true;
        this._removeScriptElementHandlers();
				logger.debug(`dispose(): this._disposed → ${this._disposed}`, { "event.action": "dispose" });
                Sys._ScriptLoader._clearScript(this._scriptElement);
        this._scriptElement = null;
		logger.debug(`dispose(): this._scriptElement → ${this._scriptElement}`, { "event.action": "dispose" });
    }

    function Sys$_ScriptLoaderTask$execute() {
		logger.error('>>> Entering execute()', { "event.action": "execute" });
        if (arguments.length !== 0) throw Error.parameterCount();
        this._addScriptElementHandlers();
        document.getElementsByTagName('HEAD')[0].appendChild(this._scriptElement);
		logger.warn('<<< Exiting execute()', { "event.action": "execute" });
    }

    function Sys$_ScriptLoaderTask$_addScriptElementHandlers() {
				logger.error('>>> Entering _addScriptElementHandlers()', { "event.action": "_addScriptElementHandlers" });
				logger.warn('<<< Exiting _addScriptElementHandlers()', { "event.action": "_addScriptElementHandlers" });
                this._scriptLoadDelegate = Function.createDelegate(this, this._scriptLoadHandler);
        
        if (Sys.Browser.agent !== Sys.Browser.InternetExplorer) {
            this._scriptElement.readyState = 'loaded';
            $addHandler(this._scriptElement, 'load', this._scriptLoadDelegate);
        }
        else {
            $addHandler(this._scriptElement, 'readystatechange', this._scriptLoadDelegate);
        }    
                        this._scriptErrorDelegate = Function.createDelegate(this, this._scriptErrorHandler);
        $addHandler(this._scriptElement, 'error', this._scriptErrorDelegate);
    }

    function Sys$_ScriptLoaderTask$_removeScriptElementHandlers() {
				logger.error('>>> Entering _removeScriptElementHandlers()', { "event.action": "_removeScriptElementHandlers" });
                if(this._scriptLoadDelegate) {
            var scriptElement = this.get_scriptElement();
			logger.error(`_removeScriptElementHandlers(): scriptElement → ${scriptElement}`, { "event.action": "_removeScriptElementHandlers" });
            if (Sys.Browser.agent !== Sys.Browser.InternetExplorer) {
                $removeHandler(scriptElement, 'load', this._scriptLoadDelegate);
            }
            else {
                $removeHandler(scriptElement, 'readystatechange', this._scriptLoadDelegate);
            }
            $removeHandler(scriptElement, 'error', this._scriptErrorDelegate);
            this._scriptErrorDelegate = null;
            this._scriptLoadDelegate = null;
			logger.debug(`_removeScriptElementHandlers(): this._scriptErrorDelegate → ${this._scriptErrorDelegate}`, { "event.action": "_removeScriptElementHandlers" });
			logger.debug(`_removeScriptElementHandlers(): this._scriptLoadDelegate → ${this._scriptLoadDelegate}`, { "event.action": "_removeScriptElementHandlers" });
        }
    }

    function Sys$_ScriptLoaderTask$_scriptErrorHandler() {
				logger.error('>>> Entering _scriptErrorHandler()', { "event.action": "_scriptErrorHandler" });
                if(this._disposed) {
            return;
        }
        
                this._completedCallback(this.get_scriptElement(), false);
    }

    function Sys$_ScriptLoaderTask$_scriptLoadHandler() {
				logger.error('>>> Entering _scriptLoadHandler()', { "event.action": "_scriptLoadHandler" });
                if(this._disposed) {
            return;
        }

        var scriptElement = this.get_scriptElement();
        if ((scriptElement.readyState !== 'loaded') &&
            (scriptElement.readyState !== 'complete')) {
			logger.error(`_scriptLoadHandler(): scriptElement → ${scriptElement}`, { "event.action": "_scriptLoadHandler" });
            return;
        }
        
                                        var _this = this;
        window.setTimeout(function() {
            _this._completedCallback(scriptElement, true);
        }, 0);
    }
Sys._ScriptLoaderTask.prototype = {
    get_scriptElement: Sys$_ScriptLoaderTask$get_scriptElement,
    
    dispose: Sys$_ScriptLoaderTask$dispose,
        
    execute: Sys$_ScriptLoaderTask$execute,
       
    _addScriptElementHandlers: Sys$_ScriptLoaderTask$_addScriptElementHandlers,    
    
    _removeScriptElementHandlers: Sys$_ScriptLoaderTask$_removeScriptElementHandlers,    

    _scriptErrorHandler: Sys$_ScriptLoaderTask$_scriptErrorHandler,
           
    _scriptLoadHandler: Sys$_ScriptLoaderTask$_scriptLoadHandler  
}
Sys._ScriptLoaderTask.registerClass("Sys._ScriptLoaderTask", null, Sys.IDisposable);
Sys.ApplicationLoadEventArgs = function Sys$ApplicationLoadEventArgs(components, isPartialLoad) {
	logger.error(`>>> Entering ApplicationLoadEventArgs(components=${components},isPartialLoad=${isPartialLoad})`, { "event.action": "ApplicationLoadEventArgs", "logrite.components": components, "logrite.isPartialLoad": isPartialLoad });
    /// <param name="components" type="Array" elementType="Sys.Component"></param>
    /// <param name="isPartialLoad" type="Boolean"></param>
    var e = Function._validateParams(arguments, [
        {name: "components", type: Array, elementType: Sys.Component},
        {name: "isPartialLoad", type: Boolean}
    ]);
    if (e) throw e;

    Sys.ApplicationLoadEventArgs.initializeBase(this);
    this._components = components;
    this._isPartialLoad = isPartialLoad;
	logger.debug(`ApplicationLoadEventArgs(components=${components},isPartialLoad=${isPartialLoad}): this._isPartialLoad → ${this._isPartialLoad}`, { "event.action": "ApplicationLoadEventArgs", "logrite.components": components, "logrite.isPartialLoad": isPartialLoad });
}
 
    function Sys$ApplicationLoadEventArgs$get_components() {
		logger.error('>>> Entering get_components()', { "event.action": "get_components" });
        /// <value type="Array" elementType="Sys.Component"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_components()', { "event.action": "get_components" });
        return this._components;
    }
    function Sys$ApplicationLoadEventArgs$get_isPartialLoad() {
		logger.error('>>> Entering get_isPartialLoad()', { "event.action": "get_isPartialLoad" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_isPartialLoad()', { "event.action": "get_isPartialLoad" });
        return this._isPartialLoad;
    }
Sys.ApplicationLoadEventArgs.prototype = {
    get_components: Sys$ApplicationLoadEventArgs$get_components,
    get_isPartialLoad: Sys$ApplicationLoadEventArgs$get_isPartialLoad
}
Sys.ApplicationLoadEventArgs.registerClass('Sys.ApplicationLoadEventArgs', Sys.EventArgs);
Sys._Application = function Sys$_Application() {
	logger.error('>>> Entering _Application()', { "event.action": "_Application" });
    Sys._Application.initializeBase(this);

    this._disposableObjects = [];
    this._components = {};
	logger.debug(`_Application(): this._disposableObjects → ${this._disposableObjects}`, { "event.action": "_Application" });
    this._createdComponents = [];
    this._secondPassComponents = [];
	logger.debug(`_Application(): this._components → ${this._components}`, { "event.action": "_Application" });

    this._unloadHandlerDelegate = Function.createDelegate(this, this._unloadHandler);
	logger.debug(`_Application(): this._createdComponents → ${this._createdComponents}`, { "event.action": "_Application" });
    this._loadHandlerDelegate = Function.createDelegate(this, this._loadHandler);

	logger.debug(`_Application(): this._secondPassComponents → ${this._secondPassComponents}`, { "event.action": "_Application" });
    Sys.UI.DomEvent.addHandler(window, "unload", this._unloadHandlerDelegate);
    Sys.UI.DomEvent.addHandler(window, "load", this._loadHandlerDelegate);
	logger.warn('<<< Exiting _Application()', { "event.action": "_Application" });
}




    function Sys$_Application$get_isCreatingComponents() {
		logger.error('>>> Entering get_isCreatingComponents()', { "event.action": "get_isCreatingComponents" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_isCreatingComponents()', { "event.action": "get_isCreatingComponents" });
        return this._creatingComponents;
    }
    function Sys$_Application$add_load(handler) {
		logger.error(`>>> Entering add_load(handler=${handler})`, { "event.action": "add_load", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_load(handler=${handler}): e → ${e}`, { "event.action": "add_load", "logrite.handler": handler });

        this.get_events().addHandler("load", handler);
		logger.warn(`<<< Exiting add_load(handler=${handler})`, { "event.action": "add_load", "logrite.handler": handler });
    }
    function Sys$_Application$remove_load(handler) {
		logger.error(`>>> Entering remove_load(handler=${handler})`, { "event.action": "remove_load", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_load(handler=${handler}): e → ${e}`, { "event.action": "remove_load", "logrite.handler": handler });

        this.get_events().removeHandler("load", handler);
		logger.warn(`<<< Exiting remove_load(handler=${handler})`, { "event.action": "remove_load", "logrite.handler": handler });
    }
    function Sys$_Application$add_init(handler) {
		logger.error(`>>> Entering add_init(handler=${handler})`, { "event.action": "add_init", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_init(handler=${handler}): e → ${e}`, { "event.action": "add_init", "logrite.handler": handler });

        if (this._initialized) {
            handler(this, Sys.EventArgs.Empty);
        }
        else {
            this.get_events().addHandler("init", handler);
			logger.warn(`<<< Exiting add_init(handler=${handler})`, { "event.action": "add_init", "logrite.handler": handler });
        }
    }
    function Sys$_Application$remove_init(handler) {
		logger.error(`>>> Entering remove_init(handler=${handler})`, { "event.action": "remove_init", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_init(handler=${handler}): e → ${e}`, { "event.action": "remove_init", "logrite.handler": handler });

        this.get_events().removeHandler("init", handler);
		logger.warn(`<<< Exiting remove_init(handler=${handler})`, { "event.action": "remove_init", "logrite.handler": handler });
    }
    function Sys$_Application$add_unload(handler) {
		logger.error(`>>> Entering add_unload(handler=${handler})`, { "event.action": "add_unload", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_unload(handler=${handler}): e → ${e}`, { "event.action": "add_unload", "logrite.handler": handler });

        this.get_events().addHandler("unload", handler);
		logger.warn(`<<< Exiting add_unload(handler=${handler})`, { "event.action": "add_unload", "logrite.handler": handler });
    }
    function Sys$_Application$remove_unload(handler) {
		logger.error(`>>> Entering remove_unload(handler=${handler})`, { "event.action": "remove_unload", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_unload(handler=${handler}): e → ${e}`, { "event.action": "remove_unload", "logrite.handler": handler });

        this.get_events().removeHandler("unload", handler);
		logger.warn(`<<< Exiting remove_unload(handler=${handler})`, { "event.action": "remove_unload", "logrite.handler": handler });
    }
    function Sys$_Application$addComponent(component) {
        /// <param name="component" type="Sys.Component"></param>
        var e = Function._validateParams(arguments, [
            {name: "component", type: Sys.Component}
        ]);
        if (e) throw e;

        var id = component.get_id();
		logger.error(`addComponent(component=${component}): id → ${id}`, { "event.action": "addComponent", "logrite.component": component });
        if (!id) throw Error.invalidOperation(Sys.Res.cantAddWithoutId);
        if (typeof(this._components[id]) !== 'undefined') throw Error.invalidOperation(String.format(Sys.Res.appDuplicateComponent, id));
        this._components[id] = component;
    }
    function Sys$_Application$beginCreateComponents() {
		logger.error('>>> Entering beginCreateComponents()', { "event.action": "beginCreateComponents" });
        this._creatingComponents = true;
		logger.debug(`beginCreateComponents(): this._creatingComponents → ${this._creatingComponents}`, { "event.action": "beginCreateComponents" });
    }
    function Sys$_Application$dispose() {
        if (!this._disposing) {
            this._disposing = true;
            if (window.pageUnload) {
				logger.debug(`dispose(): this._disposing → ${this._disposing}`, { "event.action": "dispose" });
                window.pageUnload(this, Sys.EventArgs.Empty);
            }
            var unloadHandler = this.get_events().getHandler("unload");
			logger.error(`dispose(): unloadHandler → ${unloadHandler}`, { "event.action": "dispose" });
            if (unloadHandler) {
                unloadHandler(this, Sys.EventArgs.Empty);
            }
            var disposableObjects = Array.clone(this._disposableObjects);
			logger.error(`dispose(): disposableObjects → ${disposableObjects}`, { "event.action": "dispose" });
            for (var i = 0, l = disposableObjects.length; i < l; i++) {
                disposableObjects[i].dispose();
            }
            Array.clear(this._disposableObjects);

            Sys.UI.DomEvent.removeHandler(window, "unload", this._unloadHandlerDelegate);
            if(this._loadHandlerDelegate) {
                Sys.UI.DomEvent.removeHandler(window, "load", this._loadHandlerDelegate);
                this._loadHandlerDelegate = null;
            }

			logger.debug(`dispose(): this._loadHandlerDelegate → ${this._loadHandlerDelegate}`, { "event.action": "dispose" });
            var sl = Sys._ScriptLoader.getInstance();
			logger.error(`dispose(): sl → ${sl}`, { "event.action": "dispose" });
            if(sl) {
                sl.dispose();
            }

            Sys._Application.callBaseMethod(this, 'dispose');
        }
    }
    function Sys$_Application$endCreateComponents() {
		logger.error('>>> Entering endCreateComponents()', { "event.action": "endCreateComponents" });
        var components = this._secondPassComponents;
        for (var i = 0, l = components.length; i < l; i++) {
            var component = components[i].component;
            Sys$Component$_setReferences(component, components[i].references);
            component.endUpdate();
        }
        this._secondPassComponents = [];
        this._creatingComponents = false;
		logger.debug(`endCreateComponents(): this._creatingComponents → ${this._creatingComponents}`, { "event.action": "endCreateComponents" });
    }
    function Sys$_Application$findComponent(id, parent) {
        /// <param name="id" type="String"></param>
        /// <param name="parent" optional="true" mayBeNull="true"></param>
        /// <returns type="Sys.Component" mayBeNull="true"></returns>
        var e = Function._validateParams(arguments, [
            {name: "id", type: String},
            {name: "parent", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

                        return (parent ?
            ((Sys.IContainer.isInstanceOfType(parent)) ?
                parent.findComponent(id) :
                parent[id] || null) :
            Sys.Application._components[id] || null);
    }
    function Sys$_Application$getComponents() {
        /// <returns type="Array" elementType="Sys.Component"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        var res = [];
        var components = this._components;
        for (var name in components) {
            res[res.length] = components[name];
        }
        return res;
    }
    function Sys$_Application$initialize() {
        if(!this._initialized && !this._initializing) {
            this._initializing = true;
                                                window.setTimeout(Function.createDelegate(this, this._doInitialize), 0);
												logger.debug(`initialize(): this._initializing → ${this._initializing}`, { "event.action": "initialize" });
        }
    }
    function Sys$_Application$notifyScriptLoaded() {
        if (arguments.length !== 0) throw Error.parameterCount();
        var sl = Sys._ScriptLoader.getInstance();
		logger.error(`notifyScriptLoaded(): sl → ${sl}`, { "event.action": "notifyScriptLoaded" });
        if(sl) {
            sl.notifyScriptLoaded();
        }
    }
    function Sys$_Application$registerDisposableObject(object) {
		logger.error(`>>> Entering registerDisposableObject(object=${object})`, { "event.action": "registerDisposableObject", "logrite.object": object });
        /// <param name="object" type="Sys.IDisposable"></param>
        var e = Function._validateParams(arguments, [
            {name: "object", type: Sys.IDisposable}
        ]);
        if (e) throw e;

        if (!this._disposing) {
            this._disposableObjects[this._disposableObjects.length] = object;
			logger.warn(`<<< Exiting registerDisposableObject(object=${object})`, { "event.action": "registerDisposableObject", "logrite.object": object });
        }
    }
    function Sys$_Application$raiseLoad() {
		logger.error('>>> Entering raiseLoad()', { "event.action": "raiseLoad" });
        var h = this.get_events().getHandler("load");
        var args = new Sys.ApplicationLoadEventArgs(Array.clone(this._createdComponents), !this._initializing);
		logger.error(`raiseLoad(): h → ${h}`, { "event.action": "raiseLoad" });
		logger.error(`raiseLoad(): args → ${args}`, { "event.action": "raiseLoad" });
        if (h) {
            h(this, args);
        }

        if (window.pageLoad) {
            window.pageLoad(this, args);
        }
        this._createdComponents = [];
		logger.debug(`raiseLoad(): this._createdComponents → ${this._createdComponents}`, { "event.action": "raiseLoad" });
    }
    function Sys$_Application$removeComponent(component) {
        /// <param name="component" type="Sys.Component"></param>
        var e = Function._validateParams(arguments, [
            {name: "component", type: Sys.Component}
        ]);
        if (e) throw e;

        var id = component.get_id();
        if (id) delete this._components[id];
		logger.error(`removeComponent(component=${component}): id → ${id}`, { "event.action": "removeComponent", "logrite.component": component });
    }
    function Sys$_Application$unregisterDisposableObject(object) {
		logger.error(`>>> Entering unregisterDisposableObject(object=${object})`, { "event.action": "unregisterDisposableObject", "logrite.object": object });
        /// <param name="object" type="Sys.IDisposable"></param>
        var e = Function._validateParams(arguments, [
            {name: "object", type: Sys.IDisposable}
        ]);
        if (e) throw e;

        if (!this._disposing) {
            Array.remove(this._disposableObjects, object);
			logger.warn(`<<< Exiting unregisterDisposableObject(object=${object})`, { "event.action": "unregisterDisposableObject", "logrite.object": object });
        }
    }
    function Sys$_Application$_addComponentToSecondPass(component, references) {
		logger.error(`>>> Entering _addComponentToSecondPass(component=${component},references=${references})`, { "event.action": "_addComponentToSecondPass", "logrite.component": component, "logrite.references": references });
        this._secondPassComponents[this._secondPassComponents.length] = {component: component, references: references};
		logger.warn(`<<< Exiting _addComponentToSecondPass(component=${component},references=${references})`, { "event.action": "_addComponentToSecondPass", "logrite.component": component, "logrite.references": references });
    }
    function Sys$_Application$_doInitialize() {
		logger.error('>>> Entering _doInitialize()', { "event.action": "_doInitialize" });
        Sys._Application.callBaseMethod(this, 'initialize');

        var handler = this.get_events().getHandler("init");
		logger.error(`_doInitialize(): handler → ${handler}`, { "event.action": "_doInitialize" });
        if (handler) {
            this.beginCreateComponents();
            handler(this, Sys.EventArgs.Empty);
            this.endCreateComponents();
        }
        this.raiseLoad();
        this._initializing = false;
		logger.debug(`_doInitialize(): this._initializing → ${this._initializing}`, { "event.action": "_doInitialize" });
    }
    function Sys$_Application$_loadHandler() {
						logger.error('>>> Entering _loadHandler()', { "event.action": "_loadHandler" });
                        if(this._loadHandlerDelegate) {
            Sys.UI.DomEvent.removeHandler(window, "load", this._loadHandlerDelegate);
            this._loadHandlerDelegate = null;
        }
        this.initialize();
		logger.debug(`_loadHandler(): this._loadHandlerDelegate → ${this._loadHandlerDelegate}`, { "event.action": "_loadHandler" });
    }
    function Sys$_Application$_unloadHandler(event) {
		logger.error(`>>> Entering _unloadHandler(event=${event})`, { "event.action": "_unloadHandler", "logrite.event": event });
        this.dispose();
		logger.warn(`<<< Exiting _unloadHandler(event=${event})`, { "event.action": "_unloadHandler", "logrite.event": event });
    }
Sys._Application.prototype = {
    _creatingComponents: false,
    _disposing: false,

    get_isCreatingComponents: Sys$_Application$get_isCreatingComponents,
    add_load: Sys$_Application$add_load,
    remove_load: Sys$_Application$remove_load,
    add_init: Sys$_Application$add_init,
    remove_init: Sys$_Application$remove_init,
    add_unload: Sys$_Application$add_unload,
    remove_unload: Sys$_Application$remove_unload,
    addComponent: Sys$_Application$addComponent,
    beginCreateComponents: Sys$_Application$beginCreateComponents,
    dispose: Sys$_Application$dispose,
    endCreateComponents: Sys$_Application$endCreateComponents,
    findComponent: Sys$_Application$findComponent,
    getComponents: Sys$_Application$getComponents,
    initialize: Sys$_Application$initialize,
    notifyScriptLoaded: Sys$_Application$notifyScriptLoaded,
    registerDisposableObject: Sys$_Application$registerDisposableObject,
    raiseLoad: Sys$_Application$raiseLoad,
    removeComponent: Sys$_Application$removeComponent,
    unregisterDisposableObject: Sys$_Application$unregisterDisposableObject,
    _addComponentToSecondPass: Sys$_Application$_addComponentToSecondPass,
    _doInitialize: Sys$_Application$_doInitialize,    
    _loadHandler: Sys$_Application$_loadHandler,
    _unloadHandler: Sys$_Application$_unloadHandler
}
Sys._Application.registerClass('Sys._Application', Sys.Component, Sys.IContainer);

Sys.Application = new Sys._Application();

var $find = Sys.Application.findComponent;

Type.registerNamespace('Sys.Net');

Sys.Net.WebRequestExecutor = function Sys$Net$WebRequestExecutor() {
	logger.error('>>> Entering WebRequestExecutor()', { "event.action": "WebRequestExecutor" });
    if (arguments.length !== 0) throw Error.parameterCount();
    this._webRequest = null;
    this._resultObject = null;
	logger.debug(`WebRequestExecutor(): this._webRequest → ${this._webRequest}`, { "event.action": "WebRequestExecutor" });
}


    function Sys$Net$WebRequestExecutor$get_webRequest() {
		logger.error('>>> Entering get_webRequest()', { "event.action": "get_webRequest" });
        /// <value type="Sys.Net.WebRequest"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_webRequest()', { "event.action": "get_webRequest" });
        return this._webRequest;
    }

    function Sys$Net$WebRequestExecutor$_set_webRequest(value) {
		logger.error(`>>> Entering _set_webRequest(value=${value})`, { "event.action": "_set_webRequest", "logrite.value": value });
        if (this.get_started()) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOnceStarted, 'set_webRequest'));
        }

        this._webRequest = value;
		logger.debug(`_set_webRequest(value=${value}): this._webRequest → ${this._webRequest}`, { "event.action": "_set_webRequest", "logrite.value": value });
    }


    function Sys$Net$WebRequestExecutor$get_started() {
		logger.error('>>> Entering get_started()', { "event.action": "get_started" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_started()', { "event.action": "get_started" });
        throw Error.notImplemented();
    }

    function Sys$Net$WebRequestExecutor$get_responseAvailable() {
		logger.error('>>> Entering get_responseAvailable()', { "event.action": "get_responseAvailable" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_responseAvailable()', { "event.action": "get_responseAvailable" });
        throw Error.notImplemented();
    }

    function Sys$Net$WebRequestExecutor$get_timedOut() {
		logger.error('>>> Entering get_timedOut()', { "event.action": "get_timedOut" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_timedOut()', { "event.action": "get_timedOut" });
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_aborted() {
		logger.error('>>> Entering get_aborted()', { "event.action": "get_aborted" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_aborted()', { "event.action": "get_aborted" });
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_responseData() {
		logger.error('>>> Entering get_responseData()', { "event.action": "get_responseData" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_responseData()', { "event.action": "get_responseData" });
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_statusCode() {
		logger.error('>>> Entering get_statusCode()', { "event.action": "get_statusCode" });
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_statusCode()', { "event.action": "get_statusCode" });
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_statusText() {
		logger.error('>>> Entering get_statusText()', { "event.action": "get_statusText" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_statusText()', { "event.action": "get_statusText" });
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_xml() {
		logger.error('>>> Entering get_xml()', { "event.action": "get_xml" });
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_xml()', { "event.action": "get_xml" });
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$get_object() {
		logger.error('>>> Entering get_object()', { "event.action": "get_object" });
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._resultObject) {
            this._resultObject = Sys.Serialization.JavaScriptSerializer.deserialize(this.get_responseData());
        }
		logger.warn('<<< Exiting get_object()', { "event.action": "get_object" });
        return this._resultObject;
    }


    function Sys$Net$WebRequestExecutor$executeRequest() {
		logger.error('>>> Entering executeRequest()', { "event.action": "executeRequest", "logrite.webRequest": webRequest });
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting executeRequest()', { "event.action": "executeRequest", "logrite.webRequest": webRequest });
		logger.error(`Exception in executeRequest(webRequest=${webRequest}): ${e.message}`, e, { "event.action": "executeRequest", "event.category": "process", "logrite.webRequest": webRequest });
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$abort() {
		logger.error('>>> Entering abort()', { "event.action": "abort" });
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting abort()', { "event.action": "abort" });
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$getResponseHeader(header) {
		logger.error(`>>> Entering getResponseHeader(header=${header})`, { "event.action": "getResponseHeader", "logrite.header": header });
        /// <param name="header" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "header", type: String}
        ]);
        if (e) throw e;

		logger.warn(`<<< Exiting getResponseHeader(header=${header})`, { "event.action": "getResponseHeader", "logrite.header": header });
        throw Error.notImplemented();
    }
    function Sys$Net$WebRequestExecutor$getAllResponseHeaders() {
		logger.error('>>> Entering getAllResponseHeaders()', { "event.action": "getAllResponseHeaders" });
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting getAllResponseHeaders()', { "event.action": "getAllResponseHeaders" });
        throw Error.notImplemented();
    }
Sys.Net.WebRequestExecutor.prototype = {
    get_webRequest: Sys$Net$WebRequestExecutor$get_webRequest,

    _set_webRequest: Sys$Net$WebRequestExecutor$_set_webRequest,

        get_started: Sys$Net$WebRequestExecutor$get_started,

    get_responseAvailable: Sys$Net$WebRequestExecutor$get_responseAvailable,

    get_timedOut: Sys$Net$WebRequestExecutor$get_timedOut,
    get_aborted: Sys$Net$WebRequestExecutor$get_aborted,
    get_responseData: Sys$Net$WebRequestExecutor$get_responseData,
    get_statusCode: Sys$Net$WebRequestExecutor$get_statusCode,
    get_statusText: Sys$Net$WebRequestExecutor$get_statusText,
    get_xml: Sys$Net$WebRequestExecutor$get_xml,
    get_object: Sys$Net$WebRequestExecutor$get_object,

        executeRequest: Sys$Net$WebRequestExecutor$executeRequest,
    abort: Sys$Net$WebRequestExecutor$abort,
    getResponseHeader: Sys$Net$WebRequestExecutor$getResponseHeader,
    getAllResponseHeaders: Sys$Net$WebRequestExecutor$getAllResponseHeaders
}
Sys.Net.WebRequestExecutor.registerClass('Sys.Net.WebRequestExecutor');
window.XMLDOM = function window$XMLDOM(markup) {
	logger.error(`>>> Entering XMLDOM(markup=${markup})`, { "event.action": "XMLDOM", "logrite.markup": markup });
    if (!window.DOMParser) {
        var progIDs = [ 'Msxml2.DOMDocument.3.0', 'Msxml2.DOMDocument' ];
		logger.error(`XMLDOM(markup=${markup}): progIDs → ${progIDs}`, { "event.action": "XMLDOM", "logrite.markup": markup });
        for (var i = 0; i < progIDs.length; i++) {
            try {
                var xmlDOM = new ActiveXObject(progIDs[i]);
				logger.error(`XMLDOM(markup=${markup}): xmlDOM → ${xmlDOM}`, { "event.action": "XMLDOM", "logrite.markup": markup });
                xmlDOM.async = false;
				logger.debug(`XMLDOM(markup=${markup}): xmlDOM.async → ${xmlDOM.async}`, { "event.action": "XMLDOM", "logrite.markup": markup });
                xmlDOM.loadXML(markup);
                xmlDOM.setProperty('SelectionLanguage', 'XPath');
                return xmlDOM;
            }
            catch (ex) {
			logger.error(`Exception in XMLDOM(markup=${markup}): ${ex.message}`, ex, { "event.action": "XMLDOM", "event.category": "process", "logrite.markup": markup });
            }
        }
        return null;
    }
        else {
        try {
            var domParser = new window.DOMParser();
			logger.error(`XMLDOM(markup=${markup}): domParser → ${domParser}`, { "event.action": "XMLDOM", "logrite.markup": markup });
            return domParser.parseFromString(markup, 'text/xml');
        } catch (ex) {
            return null;
        }
    }
	logger.warn(`<<< Exiting XMLDOM(markup=${markup})`, { "event.action": "XMLDOM", "logrite.markup": markup });
    return null;
    }

Sys.Net.XMLHttpExecutor = function Sys$Net$XMLHttpExecutor() {
	logger.error('>>> Entering XMLHttpExecutor()', { "event.action": "XMLHttpExecutor" });
    if (arguments.length !== 0) throw Error.parameterCount();

    Sys.Net.XMLHttpExecutor.initializeBase(this);

    var _this = this;
	logger.debug(`XMLHttpExecutor(): _this._responseAvailable → ${_this._responseAvailable}`, { "event.action": "XMLHttpExecutor" });
	logger.debug(`XMLHttpExecutor(): _this._xmlHttpRequest → ${_this._xmlHttpRequest}`, { "event.action": "XMLHttpExecutor" });
	logger.debug(`XMLHttpExecutor(): _this._timer → ${_this._timer}`, { "event.action": "XMLHttpExecutor" });
	logger.debug(`XMLHttpExecutor(): this._webRequest → ${this._webRequest}`, { "event.action": "XMLHttpExecutor" });
	logger.debug(`XMLHttpExecutor(): _this._timedOut → ${_this._timedOut}`, { "event.action": "XMLHttpExecutor" });
	logger.debug(`XMLHttpExecutor(): this._responseAvailable → ${this._responseAvailable}`, { "event.action": "XMLHttpExecutor" });
    this._xmlHttpRequest = null;
	logger.debug(`XMLHttpExecutor(): this._xmlHttpRequest → ${this._xmlHttpRequest}`, { "event.action": "XMLHttpExecutor" });
    this._webRequest = null;
    this._responseAvailable = false;
    this._timedOut = false;
    this._timer = null;
    this._aborted = false;
    this._started = false;

	logger.debug(`XMLHttpExecutor(): this._timedOut → ${this._timedOut}`, { "event.action": "XMLHttpExecutor" });
    this._onReadyStateChange = function () {
		logger.error('>>> Entering _onReadyStateChange()', { "event.action": "_onReadyStateChange" });
        
        if (_this._xmlHttpRequest.readyState === 4 ) {

			logger.debug(`XMLHttpExecutor(): this._aborted → ${this._aborted}`, { "event.action": "XMLHttpExecutor" });
            _this._clearTimer();
            _this._responseAvailable = true;
			logger.debug(`XMLHttpExecutor(): this._started → ${this._started}`, { "event.action": "XMLHttpExecutor" });
            _this._webRequest.completed(Sys.EventArgs.Empty);
            if (_this._xmlHttpRequest != null) {
                _this._xmlHttpRequest.onreadystatechange = Function.emptyMethod;
                _this._xmlHttpRequest = null;
				logger.warn('<<< Exiting _onReadyStateChange()', { "event.action": "_onReadyStateChange" });
            }
        }
    }

	logger.debug(`XMLHttpExecutor(): this._timer → ${this._timer}`, { "event.action": "XMLHttpExecutor" });
    this._clearTimer = function this$_clearTimer() {
		logger.error('>>> Entering _clearTimer()', { "event.action": "_clearTimer" });
        if (_this._timer != null) {
            window.clearTimeout(_this._timer);
            _this._timer = null;
			logger.warn('<<< Exiting _clearTimer()', { "event.action": "_clearTimer" });
        }
    }

    this._onTimeout = function this$_onTimeout() {
		logger.error('>>> Entering _onTimeout()', { "event.action": "_onTimeout" });
        if (!_this._responseAvailable) {
            _this._clearTimer();
            _this._timedOut = true;
            _this._xmlHttpRequest.onreadystatechange = Function.emptyMethod;
            _this._xmlHttpRequest.abort();
            _this._webRequest.completed(Sys.EventArgs.Empty);
            _this._xmlHttpRequest = null;
			logger.warn('<<< Exiting _onTimeout()', { "event.action": "_onTimeout" });
        }
	logger.warn('<<< Exiting XMLHttpExecutor()', { "event.action": "XMLHttpExecutor" });
    }

}



    function Sys$Net$XMLHttpExecutor$get_timedOut() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timedOut;
    }

    function Sys$Net$XMLHttpExecutor$get_started() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._started;
    }

    function Sys$Net$XMLHttpExecutor$get_responseAvailable() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
    return this._responseAvailable;
    }

    function Sys$Net$XMLHttpExecutor$get_aborted() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._aborted;
    }

    function Sys$Net$XMLHttpExecutor$executeRequest() {
        if (arguments.length !== 0) throw Error.parameterCount();
        this._webRequest = this.get_webRequest();
		const _start_executeRequest_4063 = Date.now();

        if (this._started) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOnceStarted, 'executeRequest'));
        }
        if (this._webRequest === null) {
            throw Error.invalidOperation(Sys.Res.nullWebRequest);
        }

        var body = this._webRequest.get_body();
		logger.error(`executeRequest(): body → ${body}`, { "event.action": "executeRequest", "logrite.webRequest": webRequest });
        var headers = this._webRequest.get_headers();
		logger.error(`executeRequest(): headers → ${headers}`, { "event.action": "executeRequest", "logrite.webRequest": webRequest });
        this._xmlHttpRequest = new XMLHttpRequest();
        this._xmlHttpRequest.onreadystatechange = this._onReadyStateChange;
		logger.error(`executeRequest(): API call took ${Date.now() - _start_executeRequest_4063} ms`, { "event.action": "executeRequest", "event.category": "network", "logrite.webRequest": webRequest });
        var verb = this._webRequest.get_httpVerb();
		logger.error(`executeRequest(): verb → ${verb}`, { "event.action": "executeRequest", "logrite.webRequest": webRequest });
        this._xmlHttpRequest.open(verb, this._webRequest.getResolvedUrl(), true );
        if (headers) {
            for (var header in headers) {
                var val = headers[header];
                if (typeof(val) !== "function")
                    this._xmlHttpRequest.setRequestHeader(header, val);
            }
        }

        if (verb.toLowerCase() === "post") {
                        if ((headers === null) || !headers['Content-Type']) {
                this._xmlHttpRequest.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            }

                        if (!body) {
                body = "";
            }
        }

        var timeout = this._webRequest.get_timeout();
		logger.error(`executeRequest(): timeout → ${timeout}`, { "event.action": "executeRequest", "logrite.webRequest": webRequest });
        if (timeout > 0) {
            this._timer = window.setTimeout(Function.createDelegate(this, this._onTimeout), timeout);
        }
        this._xmlHttpRequest.send(body);
        this._started = true;
		logger.debug(`executeRequest(): this._started → ${this._started}`, { "event.action": "executeRequest", "logrite.webRequest": webRequest });
    }

    function Sys$Net$XMLHttpExecutor$getResponseHeader(header) {
        /// <param name="header" type="String"></param>
        /// <returns type="String"></returns>
        var e = Function._validateParams(arguments, [
            {name: "header", type: String}
        ]);
        if (e) throw e;

        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'getResponseHeader'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'getResponseHeader'));
        }

        var result;
        try {
            result = this._xmlHttpRequest.getResponseHeader(header);
        } catch (e) {
        }
        if (!result) result = "";
		logger.error(`getResponseHeader(header=${header}): result → ${result}`, { "event.action": "getResponseHeader", "logrite.header": header });
        return result;
    }

    function Sys$Net$XMLHttpExecutor$getAllResponseHeaders() {
        /// <returns type="String"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'getAllResponseHeaders'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'getAllResponseHeaders'));
        }

        return this._xmlHttpRequest.getAllResponseHeaders();
    }

    function Sys$Net$XMLHttpExecutor$get_responseData() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'get_responseData'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'get_responseData'));
        }

        return this._xmlHttpRequest.responseText;
    }

    function Sys$Net$XMLHttpExecutor$get_statusCode() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'get_statusCode'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'get_statusCode'));
        }

        return this._xmlHttpRequest.status;
    }

    function Sys$Net$XMLHttpExecutor$get_statusText() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'get_statusText'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'get_statusText'));
        }

        return this._xmlHttpRequest.statusText;
    }

    function Sys$Net$XMLHttpExecutor$get_xml() {
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._responseAvailable) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallBeforeResponse, 'get_xml'));
        }
        if (!this._xmlHttpRequest) {
            throw Error.invalidOperation(String.format(Sys.Res.cannotCallOutsideHandler, 'get_xml'));
        }

        var xml = this._xmlHttpRequest.responseXML;
		logger.error(`get_xml(): xml → ${xml}`, { "event.action": "get_xml" });
        if (!xml || !xml.documentElement) {

                        xml = new XMLDOM(this._xmlHttpRequest.responseText);

                        if (!xml || !xml.documentElement)
                return null;
        }
                else if (navigator.userAgent.indexOf('MSIE') !== -1) {
            xml.setProperty('SelectionLanguage', 'XPath');
        }

                if (xml.documentElement.namespaceURI === "http://www.mozilla.org/newlayout/xml/parsererror.xml" &&
            xml.documentElement.tagName === "parsererror") {
            return null;
        }
        
                if (xml.documentElement.firstChild && xml.documentElement.firstChild.tagName === "parsererror") {
            return null;
        }
        
        return xml;
    }

    function Sys$Net$XMLHttpExecutor$abort() {
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._started) {
            throw Error.invalidOperation(Sys.Res.cannotAbortBeforeStart);
        }

                if (this._aborted || this._responseAvailable || this._timedOut)
            return;

        this._aborted = true;

		logger.debug(`abort(): this._aborted → ${this._aborted}`, { "event.action": "abort" });
        this._clearTimer();

        if (this._xmlHttpRequest && !this._responseAvailable) {

                        this._xmlHttpRequest.onreadystatechange = Function.emptyMethod;
            this._xmlHttpRequest.abort();

            this._xmlHttpRequest = null;
            var handler = this._webRequest._get_eventHandlerList().getHandler("completed");
			logger.error(`abort(): handler → ${handler}`, { "event.action": "abort" });
			logger.debug(`abort(): this._xmlHttpRequest → ${this._xmlHttpRequest}`, { "event.action": "abort" });
            if (handler) {
                handler(this, Sys.EventArgs.Empty);
            }
        }
    }
Sys.Net.XMLHttpExecutor.prototype = {

    get_timedOut: Sys$Net$XMLHttpExecutor$get_timedOut,

    get_started: Sys$Net$XMLHttpExecutor$get_started,

    get_responseAvailable: Sys$Net$XMLHttpExecutor$get_responseAvailable,

    get_aborted: Sys$Net$XMLHttpExecutor$get_aborted,

    executeRequest: Sys$Net$XMLHttpExecutor$executeRequest,

    getResponseHeader: Sys$Net$XMLHttpExecutor$getResponseHeader,

    getAllResponseHeaders: Sys$Net$XMLHttpExecutor$getAllResponseHeaders,

    get_responseData: Sys$Net$XMLHttpExecutor$get_responseData,

    get_statusCode: Sys$Net$XMLHttpExecutor$get_statusCode,

    get_statusText: Sys$Net$XMLHttpExecutor$get_statusText,

    get_xml: Sys$Net$XMLHttpExecutor$get_xml,

    abort: Sys$Net$XMLHttpExecutor$abort
}
Sys.Net.XMLHttpExecutor.registerClass('Sys.Net.XMLHttpExecutor', Sys.Net.WebRequestExecutor);
Sys.Net._WebRequestManager = function Sys$Net$_WebRequestManager() {
	logger.error('>>> Entering _WebRequestManager()', { "event.action": "_WebRequestManager" });
    this._this = this;
    this._defaultTimeout = 0;
    this._defaultExecutorType = "Sys.Net.XMLHttpExecutor";
	logger.debug(`_WebRequestManager(): this._defaultExecutorType → ${this._defaultExecutorType}`, { "event.action": "_WebRequestManager" });
}


    function Sys$Net$_WebRequestManager$add_invokingRequest(handler) {
		logger.error(`>>> Entering add_invokingRequest(handler=${handler})`, { "event.action": "add_invokingRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_invokingRequest(handler=${handler}): e → ${e}`, { "event.action": "add_invokingRequest", "logrite.handler": handler });

        this._get_eventHandlerList().addHandler("invokingRequest", handler);
		logger.warn(`<<< Exiting add_invokingRequest(handler=${handler})`, { "event.action": "add_invokingRequest", "logrite.handler": handler });
    }
    function Sys$Net$_WebRequestManager$remove_invokingRequest(handler) {
		logger.error(`>>> Entering remove_invokingRequest(handler=${handler})`, { "event.action": "remove_invokingRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_invokingRequest(handler=${handler}): e → ${e}`, { "event.action": "remove_invokingRequest", "logrite.handler": handler });

        this._get_eventHandlerList().removeHandler("invokingRequest", handler);
		logger.warn(`<<< Exiting remove_invokingRequest(handler=${handler})`, { "event.action": "remove_invokingRequest", "logrite.handler": handler });
    }

    function Sys$Net$_WebRequestManager$add_completedRequest(handler) {
		logger.error(`>>> Entering add_completedRequest(handler=${handler})`, { "event.action": "add_completedRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`add_completedRequest(handler=${handler}): e → ${e}`, { "event.action": "add_completedRequest", "logrite.handler": handler });

        this._get_eventHandlerList().addHandler("completedRequest", handler);
		logger.warn(`<<< Exiting add_completedRequest(handler=${handler})`, { "event.action": "add_completedRequest", "logrite.handler": handler });
    }
    function Sys$Net$_WebRequestManager$remove_completedRequest(handler) {
		logger.error(`>>> Entering remove_completedRequest(handler=${handler})`, { "event.action": "remove_completedRequest", "logrite.handler": handler });
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;
		logger.error(`remove_completedRequest(handler=${handler}): e → ${e}`, { "event.action": "remove_completedRequest", "logrite.handler": handler });

        this._get_eventHandlerList().removeHandler("completedRequest", handler);
		logger.warn(`<<< Exiting remove_completedRequest(handler=${handler})`, { "event.action": "remove_completedRequest", "logrite.handler": handler });
    }

    function Sys$Net$_WebRequestManager$_get_eventHandlerList() {
		logger.error('>>> Entering _get_eventHandlerList()', { "event.action": "_get_eventHandlerList" });
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
		logger.warn('<<< Exiting _get_eventHandlerList()', { "event.action": "_get_eventHandlerList" });
        return this._events;
    }

    function Sys$Net$_WebRequestManager$get_defaultTimeout() {
		logger.error('>>> Entering get_defaultTimeout()', { "event.action": "get_defaultTimeout" });
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_defaultTimeout()', { "event.action": "get_defaultTimeout" });
        return this._defaultTimeout;
    }
    function Sys$Net$_WebRequestManager$set_defaultTimeout(value) {
		logger.error(`>>> Entering set_defaultTimeout(value=${value})`, { "event.action": "set_defaultTimeout", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;
		logger.error(`set_defaultTimeout(value=${value}): e → ${e}`, { "event.action": "set_defaultTimeout", "logrite.value": value });

        if (value < 0) {
            throw Error.argumentOutOfRange("value", value, Sys.Res.invalidTimeout);
        }

        this._defaultTimeout = value;
		logger.debug(`set_defaultTimeout(value=${value}): this._defaultTimeout → ${this._defaultTimeout}`, { "event.action": "set_defaultTimeout", "logrite.value": value });
    }

    function Sys$Net$_WebRequestManager$get_defaultExecutorType() {
		logger.error('>>> Entering get_defaultExecutorType()', { "event.action": "get_defaultExecutorType" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_defaultExecutorType()', { "event.action": "get_defaultExecutorType" });
        return this._defaultExecutorType;
    }
    function Sys$Net$_WebRequestManager$set_defaultExecutorType(value) {
		logger.error(`>>> Entering set_defaultExecutorType(value=${value})`, { "event.action": "set_defaultExecutorType", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;
		logger.error(`set_defaultExecutorType(value=${value}): e → ${e}`, { "event.action": "set_defaultExecutorType", "logrite.value": value });

        this._defaultExecutorType = value;
		logger.debug(`set_defaultExecutorType(value=${value}): this._defaultExecutorType → ${this._defaultExecutorType}`, { "event.action": "set_defaultExecutorType", "logrite.value": value });
    }

    function Sys$Net$_WebRequestManager$executeRequest(webRequest) {
        /// <param name="webRequest" type="Sys.Net.WebRequest"></param>
        var e = Function._validateParams(arguments, [
            {name: "webRequest", type: Sys.Net.WebRequest}
        ]);
        if (e) throw e;

        var executor = webRequest.get_executor();
				logger.error(`executeRequest(webRequest=${webRequest}): executor → ${executor}`, { "event.action": "executeRequest", "logrite.webRequest": webRequest });
                if (!executor) {
            
            var failed = false;
            try {
                var executorType = eval(this._defaultExecutorType);
				logger.error(`executeRequest(webRequest=${webRequest}): executorType → ${executorType}`, { "event.action": "executeRequest", "logrite.webRequest": webRequest });
                executor = new executorType();
            } catch (e) {
                failed = true;
            }

            if (failed  || !Sys.Net.WebRequestExecutor.isInstanceOfType(executor) || !executor) {
                throw Error.argument("defaultExecutorType", String.format(Sys.Res.invalidExecutorType, this._defaultExecutorType));
            }

            webRequest.set_executor(executor);
        }

                if (executor.get_aborted()) {
            return;
        }

        var evArgs = new Sys.Net.NetworkRequestEventArgs(webRequest);
		logger.error(`executeRequest(webRequest=${webRequest}): evArgs → ${evArgs}`, { "event.action": "executeRequest", "logrite.webRequest": webRequest });
        var handler = this._get_eventHandlerList().getHandler("invokingRequest");
		logger.error(`executeRequest(webRequest=${webRequest}): handler → ${handler}`, { "event.action": "executeRequest", "logrite.webRequest": webRequest });
        if (handler) {
            handler(this, evArgs);
        }

        if (!evArgs.get_cancel()) {
            executor.executeRequest();
        }
    }
Sys.Net._WebRequestManager.prototype = {
    add_invokingRequest: Sys$Net$_WebRequestManager$add_invokingRequest,
    remove_invokingRequest: Sys$Net$_WebRequestManager$remove_invokingRequest,

    add_completedRequest: Sys$Net$_WebRequestManager$add_completedRequest,
    remove_completedRequest: Sys$Net$_WebRequestManager$remove_completedRequest,

    _get_eventHandlerList: Sys$Net$_WebRequestManager$_get_eventHandlerList,

    get_defaultTimeout: Sys$Net$_WebRequestManager$get_defaultTimeout,
    set_defaultTimeout: Sys$Net$_WebRequestManager$set_defaultTimeout,

    get_defaultExecutorType: Sys$Net$_WebRequestManager$get_defaultExecutorType,
    set_defaultExecutorType: Sys$Net$_WebRequestManager$set_defaultExecutorType,

    executeRequest: Sys$Net$_WebRequestManager$executeRequest
}

Sys.Net._WebRequestManager.registerClass('Sys.Net._WebRequestManager');

Sys.Net.WebRequestManager = new Sys.Net._WebRequestManager();
Sys.Net.NetworkRequestEventArgs = function Sys$Net$NetworkRequestEventArgs(webRequest) {
	logger.error(`>>> Entering NetworkRequestEventArgs(webRequest=${webRequest})`, { "event.action": "NetworkRequestEventArgs", "logrite.webRequest": webRequest });
    /// <param name="webRequest" type="Sys.Net.WebRequest"></param>
    var e = Function._validateParams(arguments, [
        {name: "webRequest", type: Sys.Net.WebRequest}
    ]);
    if (e) throw e;

    Sys.Net.NetworkRequestEventArgs.initializeBase(this);
    this._webRequest = webRequest;
	logger.debug(`NetworkRequestEventArgs(webRequest=${webRequest}): this._webRequest → ${this._webRequest}`, { "event.action": "NetworkRequestEventArgs", "logrite.webRequest": webRequest });
}


    function Sys$Net$NetworkRequestEventArgs$get_webRequest() {
        /// <value type="Sys.Net.WebRequest"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._webRequest;
    }
Sys.Net.NetworkRequestEventArgs.prototype = {
    get_webRequest: Sys$Net$NetworkRequestEventArgs$get_webRequest
}

Sys.Net.NetworkRequestEventArgs.registerClass('Sys.Net.NetworkRequestEventArgs', Sys.CancelEventArgs);
Sys.Net.WebRequest = function Sys$Net$WebRequest() {
	logger.error('>>> Entering WebRequest()', { "event.action": "WebRequest" });
    if (arguments.length !== 0) throw Error.parameterCount();
    this._url = "";
    this._headers = { };
	logger.debug(`WebRequest(): this._url → ${this._url}`, { "event.action": "WebRequest" });
    this._body = null;
    this._userContext = null;
	logger.debug(`WebRequest(): this._headers → ${this._headers}`, { "event.action": "WebRequest" });
    this._httpVerb = null;
    this._executor = null;
	logger.debug(`WebRequest(): this._body → ${this._body}`, { "event.action": "WebRequest" });
    this._invokeCalled = false;
    this._timeout = 0;
	logger.debug(`WebRequest(): this._timeout → ${this._timeout}`, { "event.action": "WebRequest" });
}


    function Sys$Net$WebRequest$add_completed(handler) {
	logger.error(`>>> Entering add_completed(handler=${handler})`, { "event.action": "add_completed", "logrite.handler": handler });
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;
	logger.error(`add_completed(handler=${handler}): e → ${e}`, { "event.action": "add_completed", "logrite.handler": handler });

        this._get_eventHandlerList().addHandler("completed", handler);
		logger.warn(`<<< Exiting add_completed(handler=${handler})`, { "event.action": "add_completed", "logrite.handler": handler });
    }
    function Sys$Net$WebRequest$remove_completed(handler) {
	logger.error(`>>> Entering remove_completed(handler=${handler})`, { "event.action": "remove_completed", "logrite.handler": handler });
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;
	logger.error(`remove_completed(handler=${handler}): e → ${e}`, { "event.action": "remove_completed", "logrite.handler": handler });

        this._get_eventHandlerList().removeHandler("completed", handler);
		logger.warn(`<<< Exiting remove_completed(handler=${handler})`, { "event.action": "remove_completed", "logrite.handler": handler });
    }

    function Sys$Net$WebRequest$completed(eventArgs) {
		logger.error(`>>> Entering completed(eventArgs=${eventArgs})`, { "event.action": "completed", "logrite.eventArgs": eventArgs });
        /// <param name="eventArgs" type="Sys.EventArgs"></param>
        var e = Function._validateParams(arguments, [
            {name: "eventArgs", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        var handler = Sys.Net.WebRequestManager._get_eventHandlerList().getHandler("completedRequest");
		logger.error(`completed(eventArgs=${eventArgs}): handler → ${handler}`, { "event.action": "completed", "logrite.eventArgs": eventArgs });
        if (handler) {
            handler(this._executor, eventArgs);
        }

        handler = this._get_eventHandlerList().getHandler("completed");
        if (handler) {
            handler(this._executor, eventArgs);
			logger.warn(`<<< Exiting completed(eventArgs=${eventArgs})`, { "event.action": "completed", "logrite.eventArgs": eventArgs });
        }
    }

    function Sys$Net$WebRequest$_get_eventHandlerList() {
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }

    function Sys$Net$WebRequest$get_url() {
		logger.error('>>> Entering get_url()', { "event.action": "get_url" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_url()', { "event.action": "get_url" });
        return this._url;
    }
    function Sys$Net$WebRequest$set_url(value) {
		logger.error(`>>> Entering set_url(value=${value})`, { "event.action": "set_url", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;
		logger.error(`set_url(value=${value}): e → ${e}`, { "event.action": "set_url", "logrite.value": value });

        this._url = value;
		logger.debug(`set_url(value=${value}): this._url → ${this._url}`, { "event.action": "set_url", "logrite.value": value });
    }

    function Sys$Net$WebRequest$get_headers() {
		logger.error('>>> Entering get_headers()', { "event.action": "get_headers" });
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_headers()', { "event.action": "get_headers" });
        return this._headers;
    }

    function Sys$Net$WebRequest$get_httpVerb() {
		logger.error('>>> Entering get_httpVerb()', { "event.action": "get_httpVerb" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
                if (this._httpVerb === null) {
            if (this._body === null) {
                return "GET";
            }
            return "POST";
        }
		logger.warn('<<< Exiting get_httpVerb()', { "event.action": "get_httpVerb" });
        return this._httpVerb;
    }
    function Sys$Net$WebRequest$set_httpVerb(value) {
		logger.error(`>>> Entering set_httpVerb(value=${value})`, { "event.action": "set_httpVerb", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;
		logger.error(`set_httpVerb(value=${value}): e → ${e}`, { "event.action": "set_httpVerb", "logrite.value": value });

        if (value.length === 0) {
            throw Error.argument('value', Sys.Res.invalidHttpVerb);
        }

        this._httpVerb = value;
		logger.debug(`set_httpVerb(value=${value}): this._httpVerb → ${this._httpVerb}`, { "event.action": "set_httpVerb", "logrite.value": value });
    }

    function Sys$Net$WebRequest$get_body() {
		logger.error('>>> Entering get_body()', { "event.action": "get_body" });
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_body()', { "event.action": "get_body" });
        return this._body;
    }
    function Sys$Net$WebRequest$set_body(value) {
		logger.error(`>>> Entering set_body(value=${value})`, { "event.action": "set_body", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;
		logger.error(`set_body(value=${value}): e → ${e}`, { "event.action": "set_body", "logrite.value": value });

        this._body = value;
		logger.debug(`set_body(value=${value}): this._body → ${this._body}`, { "event.action": "set_body", "logrite.value": value });
    }

    function Sys$Net$WebRequest$get_userContext() {
		logger.error('>>> Entering get_userContext()', { "event.action": "get_userContext" });
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_userContext()', { "event.action": "get_userContext" });
        return this._userContext;
    }
    function Sys$Net$WebRequest$set_userContext(value) {
		logger.error(`>>> Entering set_userContext(value=${value})`, { "event.action": "set_userContext", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;
		logger.error(`set_userContext(value=${value}): e → ${e}`, { "event.action": "set_userContext", "logrite.value": value });

        this._userContext = value;
		logger.debug(`set_userContext(value=${value}): this._userContext → ${this._userContext}`, { "event.action": "set_userContext", "logrite.value": value });
    }

    function Sys$Net$WebRequest$get_executor() {
		logger.error('>>> Entering get_executor()', { "event.action": "get_executor" });
        /// <value type="Sys.Net.WebRequestExecutor"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_executor()', { "event.action": "get_executor" });
        return this._executor;
    }
    function Sys$Net$WebRequest$set_executor(value) {
		logger.error(`>>> Entering set_executor(value=${value})`, { "event.action": "set_executor", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Sys.Net.WebRequestExecutor}]);
        if (e) throw e;
		logger.error(`set_executor(value=${value}): e → ${e}`, { "event.action": "set_executor", "logrite.value": value });

        if (this._executor !== null && this._executor.get_started()) {
            throw Error.invalidOperation(Sys.Res.setExecutorAfterActive);
        }

        this._executor = value;
        this._executor._set_webRequest(this);
		logger.debug(`set_executor(value=${value}): this._executor → ${this._executor}`, { "event.action": "set_executor", "logrite.value": value });
    }

    function Sys$Net$WebRequest$get_timeout() {
		logger.error('>>> Entering get_timeout()', { "event.action": "get_timeout" });
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._timeout === 0) {
            return Sys.Net.WebRequestManager.get_defaultTimeout();
        }
		logger.warn('<<< Exiting get_timeout()', { "event.action": "get_timeout" });
        return this._timeout;
    }
    function Sys$Net$WebRequest$set_timeout(value) {
		logger.error(`>>> Entering set_timeout(value=${value})`, { "event.action": "set_timeout", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;
		logger.error(`set_timeout(value=${value}): e → ${e}`, { "event.action": "set_timeout", "logrite.value": value });

        if (value < 0) {
            throw Error.argumentOutOfRange("value", value, Sys.Res.invalidTimeout);
        }

        this._timeout = value;
		logger.debug(`set_timeout(value=${value}): this._timeout → ${this._timeout}`, { "event.action": "set_timeout", "logrite.value": value });
    }

    function Sys$Net$WebRequest$getResolvedUrl() {
		logger.error('>>> Entering getResolvedUrl()', { "event.action": "getResolvedUrl" });
        /// <returns type="String"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting getResolvedUrl()', { "event.action": "getResolvedUrl" });
        return Sys.Net.WebRequest._resolveUrl(this._url);
    }

    function Sys$Net$WebRequest$invoke() {
		logger.error('>>> Entering invoke()', { "event.action": "invoke", "logrite.servicePath": servicePath, "logrite.methodName": methodName, "logrite.useGet": useGet, "logrite.params": params, "logrite.onSuccess": onSuccess, "logrite.onFailure": onFailure, "logrite.userContext": userContext, "logrite.timeout": timeout });
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._invokeCalled) {
            throw Error.invalidOperation(Sys.Res.invokeCalledTwice);
        }

        Sys.Net.WebRequestManager.executeRequest(this);
        this._invokeCalled = true;
		logger.warn(`<<< Exiting invoke(servicePath=${servicePath},methodName=${methodName},useGet=${useGet},params=${params},onSuccess=${onSuccess},onFailure=${onFailure},userContext=${userContext},timeout=${timeout})`, { "event.action": "invoke", "logrite.servicePath": servicePath, "logrite.methodName": methodName, "logrite.useGet": useGet, "logrite.params": params, "logrite.onSuccess": onSuccess, "logrite.onFailure": onFailure, "logrite.userContext": userContext, "logrite.timeout": timeout });
		logger.debug(`invoke(): this._invokeCalled → ${this._invokeCalled}`, { "event.action": "invoke", "logrite.servicePath": servicePath, "logrite.methodName": methodName, "logrite.useGet": useGet, "logrite.params": params, "logrite.onSuccess": onSuccess, "logrite.onFailure": onFailure, "logrite.userContext": userContext, "logrite.timeout": timeout });
    }
Sys.Net.WebRequest.prototype = {
    add_completed: Sys$Net$WebRequest$add_completed,
    remove_completed: Sys$Net$WebRequest$remove_completed,

    completed: Sys$Net$WebRequest$completed,

    _get_eventHandlerList: Sys$Net$WebRequest$_get_eventHandlerList,

    get_url: Sys$Net$WebRequest$get_url,
    set_url: Sys$Net$WebRequest$set_url,

    get_headers: Sys$Net$WebRequest$get_headers,

    get_httpVerb: Sys$Net$WebRequest$get_httpVerb,
    set_httpVerb: Sys$Net$WebRequest$set_httpVerb,

    get_body: Sys$Net$WebRequest$get_body,
    set_body: Sys$Net$WebRequest$set_body,

    get_userContext: Sys$Net$WebRequest$get_userContext,
    set_userContext: Sys$Net$WebRequest$set_userContext,

    get_executor: Sys$Net$WebRequest$get_executor,
    set_executor: Sys$Net$WebRequest$set_executor,

    get_timeout: Sys$Net$WebRequest$get_timeout,
    set_timeout: Sys$Net$WebRequest$set_timeout,

    getResolvedUrl: Sys$Net$WebRequest$getResolvedUrl,

    invoke: Sys$Net$WebRequest$invoke
}

Sys.Net.WebRequest._resolveUrl = function Sys$Net$WebRequest$_resolveUrl(url, baseUrl) {
		logger.error(`>>> Entering _resolveUrl(url=${url},baseUrl=${baseUrl})`, { "event.action": "_resolveUrl", "logrite.url": url, "logrite.baseUrl": baseUrl });
        if (url && url.indexOf('://') !== -1) {
		logger.warn(`<<< Exiting _resolveUrl(url=${url},baseUrl=${baseUrl})`, { "event.action": "_resolveUrl", "logrite.url": url, "logrite.baseUrl": baseUrl });
        return url;
    }

        if (!baseUrl || baseUrl.length === 0) {
        var baseElement = document.getElementsByTagName('base')[0];
		logger.error(`_resolveUrl(url=${url},baseUrl=${baseUrl}): baseElement → ${baseElement}`, { "event.action": "_resolveUrl", "logrite.url": url, "logrite.baseUrl": baseUrl });
        if (baseElement && baseElement.href && baseElement.href.length > 0) {
            baseUrl = baseElement.href;
        }
        else {
            baseUrl = document.URL;
        }
    }

        var qsStart = baseUrl.indexOf('?');
		logger.error(`_resolveUrl(url=${url},baseUrl=${baseUrl}): qsStart → ${qsStart}`, { "event.action": "_resolveUrl", "logrite.url": url, "logrite.baseUrl": baseUrl });
    if (qsStart !== -1) {
        baseUrl = baseUrl.substr(0, qsStart);
    }
    baseUrl = baseUrl.substr(0, baseUrl.lastIndexOf('/') + 1);

		logger.error(`_resolveUrl(url=${url},baseUrl=${baseUrl}): baseUrl → ${baseUrl}`, { "event.action": "_resolveUrl", "logrite.url": url, "logrite.baseUrl": baseUrl });
        if (!url || url.length === 0) {
        return baseUrl;
    }

        if (url.charAt(0) === '/') {
        var slashslash = baseUrl.indexOf('://');
		logger.error(`_resolveUrl(url=${url},baseUrl=${baseUrl}): slashslash → ${slashslash}`, { "event.action": "_resolveUrl", "logrite.url": url, "logrite.baseUrl": baseUrl });
        if (slashslash === -1) {
            throw Error.argument("baseUrl", Sys.Res.badBaseUrl1);
        }

        var nextSlash = baseUrl.indexOf('/', slashslash + 3);
		logger.error(`_resolveUrl(url=${url},baseUrl=${baseUrl}): nextSlash → ${nextSlash}`, { "event.action": "_resolveUrl", "logrite.url": url, "logrite.baseUrl": baseUrl });
        if (nextSlash === -1) {
            throw Error.argument("baseUrl", Sys.Res.badBaseUrl2);
        }

        return baseUrl.substr(0, nextSlash) + url;
    }
            else {
        var lastSlash = baseUrl.lastIndexOf('/');
		logger.error(`_resolveUrl(url=${url},baseUrl=${baseUrl}): lastSlash → ${lastSlash}`, { "event.action": "_resolveUrl", "logrite.url": url, "logrite.baseUrl": baseUrl });
        if (lastSlash === -1) {
            throw Error.argument("baseUrl", Sys.Res.badBaseUrl3);
        }

        return baseUrl.substr(0, lastSlash+1) + url;
    }
}

Sys.Net.WebRequest._createQueryString = function Sys$Net$WebRequest$_createQueryString(queryString, encodeMethod) {
		logger.error(`>>> Entering _createQueryString(queryString=${queryString},encodeMethod=${encodeMethod})`, { "event.action": "_createQueryString", "logrite.queryString": queryString, "logrite.encodeMethod": encodeMethod });
        if (!encodeMethod)
        encodeMethod = encodeURIComponent;

    var sb = new Sys.StringBuilder();
	logger.error(`_createQueryString(queryString=${queryString},encodeMethod=${encodeMethod}): sb → ${sb}`, { "event.action": "_createQueryString", "logrite.queryString": queryString, "logrite.encodeMethod": encodeMethod });

    var i = 0;
    for (var arg in queryString) {
        var obj = queryString[arg];
        if (typeof(obj) === "function") continue;
        var val = Sys.Serialization.JavaScriptSerializer.serialize(obj);
		logger.error(`_createQueryString(queryString=${queryString},encodeMethod=${encodeMethod}): val → ${val}`, { "event.action": "_createQueryString", "logrite.queryString": queryString, "logrite.encodeMethod": encodeMethod });
        if (i !== 0) {
            sb.append('&');
        }

        sb.append(arg);
        sb.append('=');
        sb.append(encodeMethod(val));

        i++;
    }

    return sb.toString();
}

Sys.Net.WebRequest._createUrl = function Sys$Net$WebRequest$_createUrl(url, queryString) {
	logger.error(`>>> Entering _createUrl(url=${url},queryString=${queryString})`, { "event.action": "_createUrl", "logrite.url": url, "logrite.queryString": queryString });
    if (!queryString) {
        return url;
    }

    var qs = Sys.Net.WebRequest._createQueryString(queryString);
	logger.error(`_createUrl(url=${url},queryString=${queryString}): qs → ${qs}`, { "event.action": "_createUrl", "logrite.url": url, "logrite.queryString": queryString });
    if (qs.length > 0) {
        var sep = '?';
		logger.error(`_createUrl(url=${url},queryString=${queryString}): sep → ${sep}`, { "event.action": "_createUrl", "logrite.url": url, "logrite.queryString": queryString });
        if (url && url.indexOf('?') !== -1)
            sep = '&';
        return url + sep + qs;
    } else {
		logger.warn(`<<< Exiting _createUrl(url=${url},queryString=${queryString})`, { "event.action": "_createUrl", "logrite.url": url, "logrite.queryString": queryString });
        return url;
    }
}

Sys.Net.WebRequest.registerClass('Sys.Net.WebRequest');
Sys.Net.WebServiceProxy = function Sys$Net$WebServiceProxy() {
logger.error('>>> Entering WebServiceProxy()', { "event.action": "WebServiceProxy" });
}


    function Sys$Net$WebServiceProxy$set_timeout(value) {
        this._timeout = value;
    }
    function Sys$Net$WebServiceProxy$get_timeout() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timeout;
    }
    function Sys$Net$WebServiceProxy$set_defaultUserContext(value) {
		logger.error(`>>> Entering set_defaultUserContext(value=${value})`, { "event.action": "set_defaultUserContext", "logrite.value": value });
        this._userContext = value;
		logger.debug(`set_defaultUserContext(value=${value}): this._userContext → ${this._userContext}`, { "event.action": "set_defaultUserContext", "logrite.value": value });
    }
    function Sys$Net$WebServiceProxy$get_defaultUserContext() {
		logger.error('>>> Entering get_defaultUserContext()', { "event.action": "get_defaultUserContext" });
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_defaultUserContext()', { "event.action": "get_defaultUserContext" });
        return this._userContext;
    }
    function Sys$Net$WebServiceProxy$set_defaultSucceededCallback(value) {
		logger.error(`>>> Entering set_defaultSucceededCallback(value=${value})`, { "event.action": "set_defaultSucceededCallback", "logrite.value": value });
        this._succeeded = value;
		logger.debug(`set_defaultSucceededCallback(value=${value}): this._succeeded → ${this._succeeded}`, { "event.action": "set_defaultSucceededCallback", "logrite.value": value });
    }
    function Sys$Net$WebServiceProxy$get_defaultSucceededCallback() {
		logger.error('>>> Entering get_defaultSucceededCallback()', { "event.action": "get_defaultSucceededCallback" });
        /// <value type="Function"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_defaultSucceededCallback()', { "event.action": "get_defaultSucceededCallback" });
        return this._succeeded;
    }
    function Sys$Net$WebServiceProxy$set_defaultFailedCallback(value) {
		logger.error(`>>> Entering set_defaultFailedCallback(value=${value})`, { "event.action": "set_defaultFailedCallback", "logrite.value": value });
        this._failed = value;
		logger.debug(`set_defaultFailedCallback(value=${value}): this._failed → ${this._failed}`, { "event.action": "set_defaultFailedCallback", "logrite.value": value });
    }
    function Sys$Net$WebServiceProxy$get_defaultFailedCallback() {
		logger.error('>>> Entering get_defaultFailedCallback()', { "event.action": "get_defaultFailedCallback" });
        /// <value type="Function"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_defaultFailedCallback()', { "event.action": "get_defaultFailedCallback" });
        return this._failed;
    }
    function Sys$Net$WebServiceProxy$set_path(value) {
		logger.error(`>>> Entering set_path(value=${value})`, { "event.action": "set_path", "logrite.value": value });
        this._path = value;
		logger.debug(`set_path(value=${value}): this._path → ${this._path}`, { "event.action": "set_path", "logrite.value": value });
    }
    function Sys$Net$WebServiceProxy$get_path() {
		logger.error('>>> Entering get_path()', { "event.action": "get_path" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_path()', { "event.action": "get_path" });
        return this._path;
    }

    function Sys$Net$WebServiceProxy$_invoke(servicePath, methodName, useGet, params, onSuccess, onFailure, userContext) {
		logger.error(`>>> Entering _invoke(servicePath=${servicePath},methodName=${methodName},useGet=${useGet},params=${params},onSuccess=${onSuccess},onFailure=${onFailure},userContext=${userContext})`, { "event.action": "_invoke", "logrite.servicePath": servicePath, "logrite.methodName": methodName, "logrite.useGet": useGet, "logrite.params": params, "logrite.onSuccess": onSuccess, "logrite.onFailure": onFailure, "logrite.userContext": userContext });
        /// <param name="servicePath" type="String"></param>
        /// <param name="methodName" type="String"></param>
        /// <param name="useGet" type="Boolean"></param>
        /// <param name="params"></param>
        /// <param name="onSuccess" type="Function" mayBeNull="true" optional="true"></param>
        /// <param name="onFailure" type="Function" mayBeNull="true" optional="true"></param>
        /// <param name="userContext" mayBeNull="true" optional="true"></param>
        /// <returns type="Sys.Net.WebRequest"></returns>
        var e = Function._validateParams(arguments, [
            {name: "servicePath", type: String},
            {name: "methodName", type: String},
            {name: "useGet", type: Boolean},
            {name: "params"},
            {name: "onSuccess", type: Function, mayBeNull: true, optional: true},
            {name: "onFailure", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;


                if (onSuccess === null || typeof onSuccess === 'undefined') onSuccess = this.get_defaultSucceededCallback();
        if (onFailure === null || typeof onFailure === 'undefined') onFailure = this.get_defaultFailedCallback();
        if (userContext === null || typeof userContext === 'undefined') userContext = this.get_defaultUserContext();
        
		logger.warn(`<<< Exiting _invoke(servicePath=${servicePath},methodName=${methodName},useGet=${useGet},params=${params},onSuccess=${onSuccess},onFailure=${onFailure},userContext=${userContext})`, { "event.action": "_invoke", "logrite.servicePath": servicePath, "logrite.methodName": methodName, "logrite.useGet": useGet, "logrite.params": params, "logrite.onSuccess": onSuccess, "logrite.onFailure": onFailure, "logrite.userContext": userContext });
        return Sys.Net.WebServiceProxy.invoke(servicePath, methodName, useGet, params, onSuccess, onFailure, userContext, this.get_timeout());
    }
Sys.Net.WebServiceProxy.prototype = {
    set_timeout: Sys$Net$WebServiceProxy$set_timeout,
    get_timeout: Sys$Net$WebServiceProxy$get_timeout,
    set_defaultUserContext: Sys$Net$WebServiceProxy$set_defaultUserContext,
    get_defaultUserContext: Sys$Net$WebServiceProxy$get_defaultUserContext,
    set_defaultSucceededCallback: Sys$Net$WebServiceProxy$set_defaultSucceededCallback,
    get_defaultSucceededCallback: Sys$Net$WebServiceProxy$get_defaultSucceededCallback,
    set_defaultFailedCallback: Sys$Net$WebServiceProxy$set_defaultFailedCallback,
    get_defaultFailedCallback: Sys$Net$WebServiceProxy$get_defaultFailedCallback,
    set_path: Sys$Net$WebServiceProxy$set_path,
    get_path: Sys$Net$WebServiceProxy$get_path,

    _invoke: Sys$Net$WebServiceProxy$_invoke
}
Sys.Net.WebServiceProxy.registerClass('Sys.Net.WebServiceProxy');

Sys.Net.WebServiceProxy.invoke = function Sys$Net$WebServiceProxy$invoke(servicePath, methodName, useGet, params, onSuccess, onFailure, userContext, timeout) {
    /// <param name="servicePath" type="String"></param>
    /// <param name="methodName" type="String"></param>
    /// <param name="useGet" type="Boolean" optional="true"></param>
    /// <param name="params" mayBeNull="true" optional="true"></param>
    /// <param name="onSuccess" type="Function" mayBeNull="true" optional="true"></param>
    /// <param name="onFailure" type="Function" mayBeNull="true" optional="true"></param>
    /// <param name="userContext" mayBeNull="true" optional="true"></param>
    /// <param name="timeout" type="Number" optional="true"></param>
    /// <returns type="Sys.Net.WebRequest"></returns>
    var e = Function._validateParams(arguments, [
        {name: "servicePath", type: String},
        {name: "methodName", type: String},
        {name: "useGet", type: Boolean, optional: true},
        {name: "params", mayBeNull: true, optional: true},
        {name: "onSuccess", type: Function, mayBeNull: true, optional: true},
        {name: "onFailure", type: Function, mayBeNull: true, optional: true},
        {name: "userContext", mayBeNull: true, optional: true},
        {name: "timeout", type: Number, optional: true}
    ]);
    if (e) throw e;


        var request = new Sys.Net.WebRequest();
		logger.error(`invoke(servicePath=${servicePath},methodName=${methodName},useGet=${useGet},params=${params},onSuccess=${onSuccess},onFailure=${onFailure},userContext=${userContext},timeout=${timeout}): request → ${request}`, { "event.action": "invoke", "logrite.servicePath": servicePath, "logrite.methodName": methodName, "logrite.useGet": useGet, "logrite.params": params, "logrite.onSuccess": onSuccess, "logrite.onFailure": onFailure, "logrite.userContext": userContext, "logrite.timeout": timeout });

    request.get_headers()['Content-Type'] = 'application/json; charset=utf-8';
    if (!params) params = {};
    var urlParams = params;
        if (!useGet || !urlParams) urlParams = {};
    request.set_url(Sys.Net.WebRequest._createUrl(servicePath+"/"+methodName, urlParams));

    var body = null;
		logger.error(`invoke(servicePath=${servicePath},methodName=${methodName},useGet=${useGet},params=${params},onSuccess=${onSuccess},onFailure=${onFailure},userContext=${userContext},timeout=${timeout}): body → ${body}`, { "event.action": "invoke", "logrite.servicePath": servicePath, "logrite.methodName": methodName, "logrite.useGet": useGet, "logrite.params": params, "logrite.onSuccess": onSuccess, "logrite.onFailure": onFailure, "logrite.userContext": userContext, "logrite.timeout": timeout });
        if (!useGet) {
        body = Sys.Serialization.JavaScriptSerializer.serialize(params);

                if (body === "{}") body = "";
    }

        request.set_body(body);
    request.add_completed(onComplete);
    if (timeout && timeout > 0) request.set_timeout(timeout);
    request.invoke();

    function onComplete(response, eventArgs) {
		logger.error(`>>> Entering onComplete(response=${response},eventArgs=${eventArgs})`, { "event.action": "onComplete", "logrite.response": response, "logrite.eventArgs": eventArgs });
        if (response.get_responseAvailable()) {
            var statusCode = response.get_statusCode();
			logger.error(`onComplete(response=${response},eventArgs=${eventArgs}): statusCode → ${statusCode}`, { "event.action": "onComplete", "logrite.response": response, "logrite.eventArgs": eventArgs });
            var result = null;
			logger.error(`onComplete(response=${response},eventArgs=${eventArgs}): result → ${result}`, { "event.action": "onComplete", "logrite.response": response, "logrite.eventArgs": eventArgs });
			logger.debug(`onComplete(response=${response},eventArgs=${eventArgs}): result._statusCode → ${result._statusCode}`, { "event.action": "onComplete", "logrite.response": response, "logrite.eventArgs": eventArgs });

            try {
                var contentType = response.getResponseHeader("Content-Type");
				logger.error(`onComplete(response=${response},eventArgs=${eventArgs}): contentType → ${contentType}`, { "event.action": "onComplete", "logrite.response": response, "logrite.eventArgs": eventArgs });
                if (contentType.startsWith("application/json")) {
                    result = response.get_object();
                }
                else if (contentType.startsWith("text/xml")) {
                    result = response.get_xml();
                }
                                else {
                    result = response.get_responseData();
                }
            } catch (ex) {
			logger.error(`Exception in onComplete(response=${response},eventArgs=${eventArgs}): ${ex.message}`, ex, { "event.action": "onComplete", "event.category": "process", "logrite.response": response, "logrite.eventArgs": eventArgs });
            }

            var error = response.getResponseHeader("jsonerror");
            var errorObj = (error === "true");
			logger.error(`onComplete(response=${response},eventArgs=${eventArgs}): errorObj → ${errorObj}`, { "event.action": "onComplete", "logrite.response": response, "logrite.eventArgs": eventArgs });
            if (errorObj) {
                result = new Sys.Net.WebServiceError(false, result.Message, result.StackTrace, result.ExceptionType);
            }
            if (((statusCode < 200) || (statusCode >= 300)) || errorObj) {
                if (onFailure) {
                    if (!result || !errorObj) {
                        result = new Sys.Net.WebServiceError(false , String.format(Sys.Res.webServiceFailedNoMsg, methodName), "", "");
                    }
                    result._statusCode = statusCode;
                    onFailure(result, userContext, methodName);
                }
                else {
                                        var error;
                    if (result && errorObj) {
                                                error = result.get_exceptionType() + "-- " + result.get_message();
                    }
                    else {
                                                                        error = response.get_responseData();
                    }
                    alert(String.format(Sys.Res.webServiceFailed, methodName, error));
					logger.error(`onComplete(response=${response},eventArgs=${eventArgs}): error → ${error}`, { "event.action": "onComplete", "logrite.response": response, "logrite.eventArgs": eventArgs });
                }
            }
            else if (onSuccess) {
                onSuccess(result, userContext, methodName);
            }
        }
        else {
            var msg;
            if (response.get_timedOut()) {
                msg = String.format(Sys.Res.webServiceTimedOut, methodName);
            }
            else {
                msg = String.format(Sys.Res.webServiceFailedNoMsg, methodName)
            }
            if (onFailure) {
                onFailure(new Sys.Net.WebServiceError(response.get_timedOut(), msg, "", ""), userContext, methodName);
            }
            else {
                                alert(msg);
            }
        }
    }

    return request;
}

Sys.Net.WebServiceProxy._generateTypedConstructor = function Sys$Net$WebServiceProxy$_generateTypedConstructor(type) {
	logger.error(`>>> Entering _generateTypedConstructor(type=${type})`, { "event.action": "_generateTypedConstructor", "logrite.type": type });
    return function(properties) {
                if (properties) {
            for (var name in properties) {
                this[name] = properties[name];
            }
        }
        this.__type = type;
		logger.debug(`_generateTypedConstructor(type=${type}): this.__type → ${this.__type}`, { "event.action": "_generateTypedConstructor", "logrite.type": type });
    }
	logger.warn(`<<< Exiting _generateTypedConstructor(type=${type})`, { "event.action": "_generateTypedConstructor", "logrite.type": type });
}
Sys.Net.WebServiceError = function Sys$Net$WebServiceError(timedOut, message, stackTrace, exceptionType) {
	logger.error(`>>> Entering WebServiceError(timedOut=${timedOut},message=${message},stackTrace=${stackTrace},exceptionType=${exceptionType})`, { "event.action": "WebServiceError", "logrite.timedOut": timedOut, "logrite.message": message, "logrite.stackTrace": stackTrace, "logrite.exceptionType": exceptionType });
    /// <param name="timedOut" type="Boolean"></param>
    /// <param name="message" type="String" mayBeNull="true"></param>
    /// <param name="stackTrace" type="String" mayBeNull="true"></param>
    /// <param name="exceptionType" type="String" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "timedOut", type: Boolean},
        {name: "message", type: String, mayBeNull: true},
        {name: "stackTrace", type: String, mayBeNull: true},
        {name: "exceptionType", type: String, mayBeNull: true}
    ]);
    if (e) throw e;

    this._timedOut = timedOut;
    this._message = message;
	logger.debug(`WebServiceError(timedOut=${timedOut},message=${message},stackTrace=${stackTrace},exceptionType=${exceptionType}): this._timedOut → ${this._timedOut}`, { "event.action": "WebServiceError", "logrite.timedOut": timedOut, "logrite.message": message, "logrite.stackTrace": stackTrace, "logrite.exceptionType": exceptionType });
    this._stackTrace = stackTrace;
    this._exceptionType = exceptionType;
	logger.debug(`WebServiceError(timedOut=${timedOut},message=${message},stackTrace=${stackTrace},exceptionType=${exceptionType}): this._message → ${this._message}`, { "event.action": "WebServiceError", "logrite.timedOut": timedOut, "logrite.message": message, "logrite.stackTrace": stackTrace, "logrite.exceptionType": exceptionType });
    this._statusCode = -1;
	logger.debug(`WebServiceError(timedOut=${timedOut},message=${message},stackTrace=${stackTrace},exceptionType=${exceptionType}): this._statusCode → ${this._statusCode}`, { "event.action": "WebServiceError", "logrite.timedOut": timedOut, "logrite.message": message, "logrite.stackTrace": stackTrace, "logrite.exceptionType": exceptionType });
}


    function Sys$Net$WebServiceError$get_timedOut() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timedOut;
    }

    function Sys$Net$WebServiceError$get_statusCode() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._statusCode;
    }

    function Sys$Net$WebServiceError$get_message() {
		logger.error('>>> Entering get_message()', { "event.action": "get_message" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_message()', { "event.action": "get_message" });
        return this._message;
    }

    function Sys$Net$WebServiceError$get_stackTrace() {
		logger.error('>>> Entering get_stackTrace()', { "event.action": "get_stackTrace" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_stackTrace()', { "event.action": "get_stackTrace" });
        return this._stackTrace;
    }

    function Sys$Net$WebServiceError$get_exceptionType() {
		logger.error('>>> Entering get_exceptionType()', { "event.action": "get_exceptionType" });
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_exceptionType()', { "event.action": "get_exceptionType" });
        return this._exceptionType;
    }
Sys.Net.WebServiceError.prototype = {
    get_timedOut: Sys$Net$WebServiceError$get_timedOut,

    get_statusCode: Sys$Net$WebServiceError$get_statusCode,

    get_message: Sys$Net$WebServiceError$get_message,

    get_stackTrace: Sys$Net$WebServiceError$get_stackTrace,

    get_exceptionType: Sys$Net$WebServiceError$get_exceptionType
}
Sys.Net.WebServiceError.registerClass('Sys.Net.WebServiceError');

Type.registerNamespace('Sys.Services');

Sys.Services._ProfileService = function Sys$Services$_ProfileService() {
	logger.error('>>> Entering _ProfileService()', { "event.action": "_ProfileService" });
    Sys.Services._ProfileService.initializeBase(this);
    this.properties = {};
	logger.debug(`_ProfileService(): this.properties → ${this.properties}`, { "event.action": "_ProfileService" });
}
Sys.Services._ProfileService.DefaultWebServicePath = '';








    function Sys$Services$_ProfileService$get_defaultFailedCallback() {
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultFailedCallback;
    }
    function Sys$Services$_ProfileService$set_defaultFailedCallback(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;
		logger.error(`set_defaultFailedCallback(value=${value}): e → ${e}`, { "event.action": "set_defaultFailedCallback", "logrite.value": value });

        this._defaultFailedCallback = value;
		logger.debug(`set_defaultFailedCallback(value=${value}): this._defaultFailedCallback → ${this._defaultFailedCallback}`, { "event.action": "set_defaultFailedCallback", "logrite.value": value });
    }

    function Sys$Services$_ProfileService$get_defaultLoadCompletedCallback() {
		logger.error('>>> Entering get_defaultLoadCompletedCallback()', { "event.action": "get_defaultLoadCompletedCallback" });
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_defaultLoadCompletedCallback()', { "event.action": "get_defaultLoadCompletedCallback" });
        return this._defaultLoadCompletedCallback;
    }
    function Sys$Services$_ProfileService$set_defaultLoadCompletedCallback(value) {
		logger.error(`>>> Entering set_defaultLoadCompletedCallback(value=${value})`, { "event.action": "set_defaultLoadCompletedCallback", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;
		logger.error(`set_defaultLoadCompletedCallback(value=${value}): e → ${e}`, { "event.action": "set_defaultLoadCompletedCallback", "logrite.value": value });

        this._defaultLoadCompletedCallback = value;
		logger.debug(`set_defaultLoadCompletedCallback(value=${value}): this._defaultLoadCompletedCallback → ${this._defaultLoadCompletedCallback}`, { "event.action": "set_defaultLoadCompletedCallback", "logrite.value": value });
    }

    function Sys$Services$_ProfileService$get_defaultSaveCompletedCallback() {
		logger.error('>>> Entering get_defaultSaveCompletedCallback()', { "event.action": "get_defaultSaveCompletedCallback" });
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_defaultSaveCompletedCallback()', { "event.action": "get_defaultSaveCompletedCallback" });
        return this._defaultSaveCompletedCallback;
    }
    function Sys$Services$_ProfileService$set_defaultSaveCompletedCallback(value) {
		logger.error(`>>> Entering set_defaultSaveCompletedCallback(value=${value})`, { "event.action": "set_defaultSaveCompletedCallback", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;
		logger.error(`set_defaultSaveCompletedCallback(value=${value}): e → ${e}`, { "event.action": "set_defaultSaveCompletedCallback", "logrite.value": value });

        this._defaultSaveCompletedCallback = value;
		logger.debug(`set_defaultSaveCompletedCallback(value=${value}): this._defaultSaveCompletedCallback → ${this._defaultSaveCompletedCallback}`, { "event.action": "set_defaultSaveCompletedCallback", "logrite.value": value });
    }


    function Sys$Services$_ProfileService$get_path() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._path;
    }
    function Sys$Services$_ProfileService$set_path(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;
		logger.error(`set_path(value=${value}): e → ${e}`, { "event.action": "set_path", "logrite.value": value });

        if((!value) || (!value.length)) {
            value = '';
        }
        this._path = value;
    }

    function Sys$Services$_ProfileService$get_timeout() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timeout;
    }
    function Sys$Services$_ProfileService$set_timeout(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        this._timeout = value;
    }

    function Sys$Services$_ProfileService$load(propertyNames, loadCompletedCallback, failedCallback, userContext) {
		logger.error(`>>> Entering load(propertyNames=${propertyNames},loadCompletedCallback=${loadCompletedCallback},failedCallback=${failedCallback},userContext=${userContext})`, { "event.action": "load", "logrite.propertyNames": propertyNames, "logrite.loadCompletedCallback": loadCompletedCallback, "logrite.failedCallback": failedCallback, "logrite.userContext": userContext });
        /// <param name="propertyNames" type="Array" elementType="String" optional="true" elementMayBeNull="false" mayBeNull="true"></param>
        /// <param name="loadCompletedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="failedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="userContext" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "propertyNames", type: Array, mayBeNull: true, optional: true, elementType: String},
            {name: "loadCompletedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "failedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        var parameters = {};
		logger.error(`load(propertyNames=${propertyNames},loadCompletedCallback=${loadCompletedCallback},failedCallback=${failedCallback},userContext=${userContext}): parameters → ${parameters}`, { "event.action": "load", "logrite.propertyNames": propertyNames, "logrite.loadCompletedCallback": loadCompletedCallback, "logrite.failedCallback": failedCallback, "logrite.userContext": userContext });
        var methodName;
        if(!propertyNames) {
            methodName = "GetAllPropertiesForCurrentUser";
        }
        else {
            methodName = "GetPropertiesForCurrentUser";
            parameters = { properties: this._clonePropertyNames(propertyNames) };
			logger.error(`load(propertyNames=${propertyNames},loadCompletedCallback=${loadCompletedCallback},failedCallback=${failedCallback},userContext=${userContext}): methodName → ${methodName}`, { "event.action": "load", "logrite.propertyNames": propertyNames, "logrite.loadCompletedCallback": loadCompletedCallback, "logrite.failedCallback": failedCallback, "logrite.userContext": userContext });
        }
                this._invoke(this._get_path(),
                                        methodName,
                                        false,
                                        parameters,
                                        Function.createDelegate(this, this._onLoadComplete),
                                        Function.createDelegate(this, this._onLoadFailed),                                         [loadCompletedCallback, failedCallback, userContext]);
										logger.debug(`load(propertyNames=${propertyNames},loadCompletedCallback=${loadCompletedCallback},failedCallback=${failedCallback},userContext=${userContext}): methodName → ${methodName}`, { "event.action": "load", "logrite.propertyNames": propertyNames, "logrite.loadCompletedCallback": loadCompletedCallback, "logrite.failedCallback": failedCallback, "logrite.userContext": userContext });
    }

    function Sys$Services$_ProfileService$save(propertyNames, saveCompletedCallback, failedCallback, userContext) {
		logger.error(`>>> Entering save(propertyNames=${propertyNames},saveCompletedCallback=${saveCompletedCallback},failedCallback=${failedCallback},userContext=${userContext})`, { "event.action": "save", "logrite.propertyNames": propertyNames, "logrite.saveCompletedCallback": saveCompletedCallback, "logrite.failedCallback": failedCallback, "logrite.userContext": userContext });
        /// <param name="propertyNames" type="Array" elementType="String" optional="true" elementMayBeNull="false" mayBeNull="true"></param>
        /// <param name="saveCompletedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="failedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="userContext" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "propertyNames", type: Array, mayBeNull: true, optional: true, elementType: String},
            {name: "saveCompletedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "failedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        var flattenedProperties = this._flattenProperties(propertyNames, this.properties);
				logger.error(`save(propertyNames=${propertyNames},saveCompletedCallback=${saveCompletedCallback},failedCallback=${failedCallback},userContext=${userContext}): flattenedProperties → ${flattenedProperties}`, { "event.action": "save", "logrite.propertyNames": propertyNames, "logrite.saveCompletedCallback": saveCompletedCallback, "logrite.failedCallback": failedCallback, "logrite.userContext": userContext });
                this._invoke(this._get_path(),
                                        "SetPropertiesForCurrentUser",
                                        false,
                                        { values: flattenedProperties },
                                        Function.createDelegate(this, this._onSaveComplete),
                                        Function.createDelegate(this, this._onSaveFailed),
                                        [saveCompletedCallback, failedCallback, userContext]);
										logger.warn(`<<< Exiting save(propertyNames=${propertyNames},saveCompletedCallback=${saveCompletedCallback},failedCallback=${failedCallback},userContext=${userContext})`, { "event.action": "save", "logrite.propertyNames": propertyNames, "logrite.saveCompletedCallback": saveCompletedCallback, "logrite.failedCallback": failedCallback, "logrite.userContext": userContext });
    }


    function Sys$Services$_ProfileService$_clonePropertyNames(arr) {
		logger.error(`>>> Entering _clonePropertyNames(arr=${arr})`, { "event.action": "_clonePropertyNames", "logrite.arr": arr });
        var nodups = [];
        var seen = {};
        for(var i=0; i < arr.length; i++) {
            var prop = arr[i];
            if(!seen[prop]) { Array.add(nodups, prop); seen[prop]=true; };
        }
		logger.warn(`<<< Exiting _clonePropertyNames(arr=${arr})`, { "event.action": "_clonePropertyNames", "logrite.arr": arr });
        return nodups;
    }





    function Sys$Services$_ProfileService$_flattenProperties(propertyNames, properties, groupName) {
		logger.error(`>>> Entering _flattenProperties(propertyNames=${propertyNames},properties=${properties},groupName=${groupName})`, { "event.action": "_flattenProperties", "logrite.propertyNames": propertyNames, "logrite.properties": properties, "logrite.groupName": groupName });
        var flattenedProperties = {};
        var val;
        var key;
        if(propertyNames && propertyNames.length === 0) {
            return flattenedProperties;
        }

        for (var property in properties) {
            val = properties[property];
            key = groupName ? groupName + "." + property : property;
						logger.debug(`_flattenProperties(propertyNames=${propertyNames},properties=${properties},groupName=${groupName}): val → ${val}`, { "event.action": "_flattenProperties", "logrite.propertyNames": propertyNames, "logrite.properties": properties, "logrite.groupName": groupName });
                        if(Sys.Services.ProfileGroup.isInstanceOfType(val)) {
                var groupProperties = this._flattenProperties(propertyNames, val, key);
                                                                                                                for(var subKey in groupProperties) {
                    var subVal = groupProperties[subKey];
                    flattenedProperties[subKey] = subVal;
                }
            }
            else {
                                if(!propertyNames || Array.indexOf(propertyNames, key) !== -1) {
                    flattenedProperties[key] = val;
                }
            }
        }
		logger.error(`_flattenProperties(propertyNames=${propertyNames},properties=${properties},groupName=${groupName}): groupProperties → ${groupProperties}`, { "event.action": "_flattenProperties", "logrite.propertyNames": propertyNames, "logrite.properties": properties, "logrite.groupName": groupName });
        return flattenedProperties;
    }

    function Sys$Services$_ProfileService$_get_path() {
		logger.error('>>> Entering _get_path()', { "event.action": "_get_path" });
        var path = this.get_path();
        if(!path.length) {
            path = Sys.Services._ProfileService.DefaultWebServicePath;
        }
        if(!path || !path.length) {
		logger.error(`_get_path(): path → ${path}`, { "event.action": "_get_path" });
            throw Error.invalidOperation(Sys.Res.servicePathNotSet);
        }
        return path;
    }

    function Sys$Services$_ProfileService$_onLoadComplete(result, context, methodName) {
		logger.error(`>>> Entering _onLoadComplete(result=${result},context=${context},methodName=${methodName})`, { "event.action": "_onLoadComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
        if(typeof(result) !== "object") {
            throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, methodName, "Object"));
        }

        var unflattened = this._unflattenProperties(result);
		logger.error(`_onLoadComplete(result=${result},context=${context},methodName=${methodName}): unflattened → ${unflattened}`, { "event.action": "_onLoadComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
        for(var name in unflattened) {
            this.properties[name] = unflattened[name];
        }
        
        var userCallback = context[0];
        var callback = userCallback ? userCallback : this._defaultLoadCompletedCallback;
        if(callback) {
            callback(result.length, context[2], "Sys.Services.ProfileService.load");
			logger.warn(`<<< Exiting _onLoadComplete(result=${result},context=${context},methodName=${methodName})`, { "event.action": "_onLoadComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
        }
    }

    function Sys$Services$_ProfileService$_onLoadFailed(err, context, methodName) {
		logger.error(`>>> Entering _onLoadFailed(err=${err},context=${context},methodName=${methodName})`, { "event.action": "_onLoadFailed", "logrite.err": err, "logrite.context": context, "logrite.methodName": methodName });
        var userCallback = context[1];
        var callback = userCallback ? userCallback : this._defaultFailedCallback;
        if(callback) {
            callback(err, context[2], "Sys.Services.ProfileService.load");
			logger.warn(`<<< Exiting _onLoadFailed(err=${err},context=${context},methodName=${methodName})`, { "event.action": "_onLoadFailed", "logrite.err": err, "logrite.context": context, "logrite.methodName": methodName });
        }
    }

    function Sys$Services$_ProfileService$_onSaveComplete(result, context, methodName) {
		logger.error(`>>> Entering _onSaveComplete(result=${result},context=${context},methodName=${methodName})`, { "event.action": "_onSaveComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
        if(typeof(result) !== "number") {
            throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, methodName, "Number"));
        }
        
        var userCallback = context[0];
        var userContext = context[2];
        var callback = userCallback ? userCallback : this._defaultSaveCompletedCallback;
        if(callback) {
            callback(result, userContext, "Sys.Services.ProfileService.save");
			logger.warn(`<<< Exiting _onSaveComplete(result=${result},context=${context},methodName=${methodName})`, { "event.action": "_onSaveComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
        }
    }

    function Sys$Services$_ProfileService$_onSaveFailed(err, context, methodName) {
		logger.error(`>>> Entering _onSaveFailed(err=${err},context=${context},methodName=${methodName})`, { "event.action": "_onSaveFailed", "logrite.err": err, "logrite.context": context, "logrite.methodName": methodName });
        var userCallback = context[1];
        var userContext = context[2];
        var callback = userCallback ? userCallback : this._defaultFailedCallback;
        if(callback) {
            callback(err, userContext, "Sys.Services.ProfileService.save");
			logger.warn(`<<< Exiting _onSaveFailed(err=${err},context=${context},methodName=${methodName})`, { "event.action": "_onSaveFailed", "logrite.err": err, "logrite.context": context, "logrite.methodName": methodName });
        }
    }

    function Sys$Services$_ProfileService$_unflattenProperties(properties) {
		logger.error(`>>> Entering _unflattenProperties(properties=${properties})`, { "event.action": "_unflattenProperties", "logrite.properties": properties });
        var unflattenedProperties = {};
        var dotIndex;
        var val;
        var count = 0;
		logger.error(`_unflattenProperties(properties=${properties}): count → ${count}`, { "event.action": "_unflattenProperties", "logrite.properties": properties });
        for(var key in properties) {
            count++;
            val = properties[key];

			logger.debug(`_unflattenProperties(properties=${properties}): val → ${val}`, { "event.action": "_unflattenProperties", "logrite.properties": properties });
            dotIndex = key.indexOf('.');
            if(dotIndex !== -1) {
				logger.error(`_unflattenProperties(properties=${properties}): dotIndex → ${dotIndex}`, { "event.action": "_unflattenProperties", "logrite.properties": properties });
				logger.debug(`_unflattenProperties(properties=${properties}): dotIndex → ${dotIndex}`, { "event.action": "_unflattenProperties", "logrite.properties": properties });
                var groupName = key.substr(0, dotIndex);
				logger.error(`_unflattenProperties(properties=${properties}): groupName → ${groupName}`, { "event.action": "_unflattenProperties", "logrite.properties": properties });
                key = key.substr(dotIndex+1);
                var group = unflattenedProperties[groupName];
                if((!group) || (!Sys.Services.ProfileGroup.isInstanceOfType(group))) {
                    group = new Sys.Services.ProfileGroup();
					logger.error(`_unflattenProperties(properties=${properties}): group → ${group}`, { "event.action": "_unflattenProperties", "logrite.properties": properties });
                    unflattenedProperties[groupName] = group;
                }
                group[key] = val;
            }
            else {
                unflattenedProperties[key] = val;
            }
        }
        properties.length = count;
		logger.debug(`_unflattenProperties(properties=${properties}): properties.length → ${properties.length}`, { "event.action": "_unflattenProperties", "logrite.properties": properties });
		logger.warn(`<<< Exiting _unflattenProperties(properties=${properties})`, { "event.action": "_unflattenProperties", "logrite.properties": properties });
        return unflattenedProperties;
    }
Sys.Services._ProfileService.prototype = {
    _defaultFailedCallback: null,
    _defaultLoadCompletedCallback: null,
    _defaultSaveCompletedCallback: null,
    _path: '',
    _timeout: 0,

    get_defaultFailedCallback: Sys$Services$_ProfileService$get_defaultFailedCallback,
    set_defaultFailedCallback: Sys$Services$_ProfileService$set_defaultFailedCallback,

    get_defaultLoadCompletedCallback: Sys$Services$_ProfileService$get_defaultLoadCompletedCallback,
    set_defaultLoadCompletedCallback: Sys$Services$_ProfileService$set_defaultLoadCompletedCallback,

    get_defaultSaveCompletedCallback: Sys$Services$_ProfileService$get_defaultSaveCompletedCallback,
    set_defaultSaveCompletedCallback: Sys$Services$_ProfileService$set_defaultSaveCompletedCallback,
    
    
    get_path: Sys$Services$_ProfileService$get_path,
    set_path: Sys$Services$_ProfileService$set_path,
        
    get_timeout: Sys$Services$_ProfileService$get_timeout,
    set_timeout: Sys$Services$_ProfileService$set_timeout,
        
    load: Sys$Services$_ProfileService$load,

    save: Sys$Services$_ProfileService$save,
    
        _clonePropertyNames: Sys$Services$_ProfileService$_clonePropertyNames,    

                    _flattenProperties: Sys$Services$_ProfileService$_flattenProperties,
    
    _get_path: Sys$Services$_ProfileService$_get_path,    

    _onLoadComplete: Sys$Services$_ProfileService$_onLoadComplete,
    
    _onLoadFailed: Sys$Services$_ProfileService$_onLoadFailed,
    
    _onSaveComplete: Sys$Services$_ProfileService$_onSaveComplete,
    
    _onSaveFailed: Sys$Services$_ProfileService$_onSaveFailed,
    
    _unflattenProperties: Sys$Services$_ProfileService$_unflattenProperties
}
Sys.Services._ProfileService.registerClass('Sys.Services._ProfileService', Sys.Net.WebServiceProxy);
Sys.Services.ProfileService = new Sys.Services._ProfileService();

Sys.Services.ProfileGroup = function Sys$Services$ProfileGroup(properties) {
	logger.error(`>>> Entering ProfileGroup(properties=${properties})`, { "event.action": "ProfileGroup", "logrite.properties": properties });
    /// <param name="properties" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "properties", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    if(properties) {
        for(var property in properties) {
            this[property] = properties[property];
			logger.warn(`<<< Exiting ProfileGroup(properties=${properties})`, { "event.action": "ProfileGroup", "logrite.properties": properties });
        }
    }
}
Sys.Services.ProfileGroup.registerClass('Sys.Services.ProfileGroup');








Sys.Services._AuthenticationService = function Sys$Services$_AuthenticationService() {
	logger.error('>>> Entering _AuthenticationService()', { "event.action": "_AuthenticationService" });
    if (arguments.length !== 0) throw Error.parameterCount();
    Sys.Services._AuthenticationService.initializeBase(this);
	logger.warn('<<< Exiting _AuthenticationService()', { "event.action": "_AuthenticationService" });
}
Sys.Services._AuthenticationService.DefaultWebServicePath = '';








    function Sys$Services$_AuthenticationService$get_defaultFailedCallback() {
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._defaultFailedCallback;
    }
    function Sys$Services$_AuthenticationService$set_defaultFailedCallback(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;

        this._defaultFailedCallback = value;
    }

    function Sys$Services$_AuthenticationService$get_defaultLoginCompletedCallback() {
		logger.error('>>> Entering get_defaultLoginCompletedCallback()', { "event.action": "get_defaultLoginCompletedCallback" });
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_defaultLoginCompletedCallback()', { "event.action": "get_defaultLoginCompletedCallback" });
        return this._defaultLoginCompletedCallback;
    }
    function Sys$Services$_AuthenticationService$set_defaultLoginCompletedCallback(value) {
		logger.error(`>>> Entering set_defaultLoginCompletedCallback(value=${value})`, { "event.action": "set_defaultLoginCompletedCallback", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;
		logger.error(`set_defaultLoginCompletedCallback(value=${value}): e → ${e}`, { "event.action": "set_defaultLoginCompletedCallback", "logrite.value": value });

        this._defaultLoginCompletedCallback = value;
		logger.debug(`set_defaultLoginCompletedCallback(value=${value}): this._defaultLoginCompletedCallback → ${this._defaultLoginCompletedCallback}`, { "event.action": "set_defaultLoginCompletedCallback", "logrite.value": value });
    }

    function Sys$Services$_AuthenticationService$get_defaultLogoutCompletedCallback() {
		logger.error('>>> Entering get_defaultLogoutCompletedCallback()', { "event.action": "get_defaultLogoutCompletedCallback" });
        /// <value type="Function" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_defaultLogoutCompletedCallback()', { "event.action": "get_defaultLogoutCompletedCallback" });
        return this._defaultLogoutCompletedCallback;
    }
    function Sys$Services$_AuthenticationService$set_defaultLogoutCompletedCallback(value) {
		logger.error(`>>> Entering set_defaultLogoutCompletedCallback(value=${value})`, { "event.action": "set_defaultLogoutCompletedCallback", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Function, mayBeNull: true}]);
        if (e) throw e;
		logger.error(`set_defaultLogoutCompletedCallback(value=${value}): e → ${e}`, { "event.action": "set_defaultLogoutCompletedCallback", "logrite.value": value });

        this._defaultLogoutCompletedCallback = value;
		logger.debug(`set_defaultLogoutCompletedCallback(value=${value}): this._defaultLogoutCompletedCallback → ${this._defaultLogoutCompletedCallback}`, { "event.action": "set_defaultLogoutCompletedCallback", "logrite.value": value });
    }

    function Sys$Services$_AuthenticationService$get_isLoggedIn() {
		logger.error('>>> Entering get_isLoggedIn()', { "event.action": "get_isLoggedIn" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_isLoggedIn()', { "event.action": "get_isLoggedIn" });
        return this._authenticated;
    }

    function Sys$Services$_AuthenticationService$get_path() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._path;
    }
    function Sys$Services$_AuthenticationService$set_path(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        if((!value) || (!value.length)) {
            value = '';
        }
        this._path = value;
    }

    function Sys$Services$_AuthenticationService$get_timeout() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timeout;
    }
    function Sys$Services$_AuthenticationService$set_timeout(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        this._timeout = value;
    }

    function Sys$Services$_AuthenticationService$login(username, password, isPersistent, customInfo, redirectUrl, loginCompletedCallback, failedCallback, userContext) {
        /// <param name="username" type="String" mayBeNull="false"></param>
        /// <param name="password" type="String" mayBeNull="true"></param>
        /// <param name="isPersistent" type="Boolean" optional="true" mayBeNull="true"></param>
        /// <param name="customInfo" type="String" optional="true" mayBeNull="true"></param>
        /// <param name="redirectUrl" type="String" optional="true" mayBeNull="true"></param>
        /// <param name="loginCompletedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="failedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="userContext" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "username", type: String},
            {name: "password", type: String, mayBeNull: true},
            {name: "isPersistent", type: Boolean, mayBeNull: true, optional: true},
            {name: "customInfo", type: String, mayBeNull: true, optional: true},
            {name: "redirectUrl", type: String, mayBeNull: true, optional: true},
            {name: "loginCompletedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "failedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

                this._invoke(this._get_path(), "Login", false,
                                        { userName: username, password: password, createPersistentCookie: isPersistent },
                                        Function.createDelegate(this, this._onLoginComplete),
                                        Function.createDelegate(this, this._onLoginFailed),
                                        [username, password, isPersistent, redirectUrl, loginCompletedCallback, failedCallback, userContext]);
    }

    function Sys$Services$_AuthenticationService$logout(redirectUrl, logoutCompletedCallback, failedCallback, userContext) {
		logger.error(`>>> Entering logout(redirectUrl=${redirectUrl},logoutCompletedCallback=${logoutCompletedCallback},failedCallback=${failedCallback},userContext=${userContext})`, { "event.action": "logout", "logrite.redirectUrl": redirectUrl, "logrite.logoutCompletedCallback": logoutCompletedCallback, "logrite.failedCallback": failedCallback, "logrite.userContext": userContext });
        /// <param name="redirectUrl" type="String" optional="true" mayBeNull="true"></param>
        /// <param name="logoutCompletedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="failedCallback" type="Function" optional="true" mayBeNull="true"></param>
        /// <param name="userContext" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "redirectUrl", type: String, mayBeNull: true, optional: true},
            {name: "logoutCompletedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "failedCallback", type: Function, mayBeNull: true, optional: true},
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

                this._invoke(this._get_path(), "Logout", false, {}, 
                                        Function.createDelegate(this, this._onLogoutComplete),
                                        Function.createDelegate(this, this._onLogoutFailed),
                                        [redirectUrl, logoutCompletedCallback, failedCallback, userContext]);
										logger.warn(`<<< Exiting logout(redirectUrl=${redirectUrl},logoutCompletedCallback=${logoutCompletedCallback},failedCallback=${failedCallback},userContext=${userContext})`, { "event.action": "logout", "logrite.redirectUrl": redirectUrl, "logrite.logoutCompletedCallback": logoutCompletedCallback, "logrite.failedCallback": failedCallback, "logrite.userContext": userContext });
    }

    function Sys$Services$_AuthenticationService$_get_path() {
        var path = this.get_path();
        if(!path.length) {
            path = Sys.Services._AuthenticationService.DefaultWebServicePath;
        }
        if(!path || !path.length) {
            throw Error.invalidOperation(Sys.Res.servicePathNotSet);
        }
        return path;
    }

    function Sys$Services$_AuthenticationService$_onLoginComplete(result, context, methodName) {
		logger.error(`>>> Entering _onLoginComplete(result=${result},context=${context},methodName=${methodName})`, { "event.action": "_onLoginComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
        if(typeof(result) !== "boolean") {
            throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, methodName, "Boolean"));
        }
        
        var redirectUrl = context[3];
        var userCallback = context[4];
        var userContext = context[6];
        var callback = userCallback ? userCallback : this._defaultLoginCompletedCallback;
        
        if(result) {
            this._authenticated = true;

			logger.debug(`_onLoginComplete(result=${result},context=${context},methodName=${methodName}): this._authenticated → ${this._authenticated}`, { "event.action": "_onLoginComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
            if(callback) {
                callback(true, userContext, "Sys.Services.AuthenticationService.login");
            }
            
            if(typeof(redirectUrl) !== "undefined" && redirectUrl !== null) {
                                window.location.href = redirectUrl;
								logger.warn(`<<< Exiting _onLoginComplete(result=${result},context=${context},methodName=${methodName})`, { "event.action": "_onLoginComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
            }
        }
        else if (callback) {
            callback(false, userContext, "Sys.Services.AuthenticationService.login");
        }
    }

    function Sys$Services$_AuthenticationService$_onLoginFailed(err, context, methodName) {
		logger.error(`>>> Entering _onLoginFailed(err=${err},context=${context},methodName=${methodName})`, { "event.action": "_onLoginFailed", "logrite.err": err, "logrite.context": context, "logrite.methodName": methodName });
        var userCallback = context[5];
        var callback = userCallback ? userCallback : this._defaultFailedCallback;
        if(callback) {
            callback(err, context[6], "Sys.Services.AuthenticationService.login");
			logger.warn(`<<< Exiting _onLoginFailed(err=${err},context=${context},methodName=${methodName})`, { "event.action": "_onLoginFailed", "logrite.err": err, "logrite.context": context, "logrite.methodName": methodName });
        }
    }

    function Sys$Services$_AuthenticationService$_onLogoutComplete(result, context, methodName) {
		logger.error(`>>> Entering _onLogoutComplete(result=${result},context=${context},methodName=${methodName})`, { "event.action": "_onLogoutComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
        if(result !== null) {
            throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, methodName, "null"));
        }
        
        var redirectUrl = context[0];
        var userCallback = context[1];
        var userContext = context[3];
        var callback = userCallback ? userCallback : this._defaultLogoutCompletedCallback;

        this._authenticated = false;
        
		logger.debug(`_onLogoutComplete(result=${result},context=${context},methodName=${methodName}): this._authenticated → ${this._authenticated}`, { "event.action": "_onLogoutComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
        if (callback) {
            callback(null, userContext, "Sys.Services.AuthenticationService.logout");
        }
        
                if(!redirectUrl) {
            window.location.reload();
        }
        else {
            window.location.href = redirectUrl;
			logger.warn(`<<< Exiting _onLogoutComplete(result=${result},context=${context},methodName=${methodName})`, { "event.action": "_onLogoutComplete", "logrite.result": result, "logrite.context": context, "logrite.methodName": methodName });
        }
    }

    function Sys$Services$_AuthenticationService$_onLogoutFailed(err, context, methodName) {
		logger.error(`>>> Entering _onLogoutFailed(err=${err},context=${context},methodName=${methodName})`, { "event.action": "_onLogoutFailed", "logrite.err": err, "logrite.context": context, "logrite.methodName": methodName });
        var userCallback = context[2];
        var callback = userCallback ? userCallback : this._defaultFailedCallback;
        if(callback) {
            callback(err, context[3], "Sys.Services.AuthenticationService.logout");
			logger.warn(`<<< Exiting _onLogoutFailed(err=${err},context=${context},methodName=${methodName})`, { "event.action": "_onLogoutFailed", "logrite.err": err, "logrite.context": context, "logrite.methodName": methodName });
        }
    }

    function Sys$Services$_AuthenticationService$_setAuthenticated(authenticated) {
		logger.error(`>>> Entering _setAuthenticated(authenticated=${authenticated})`, { "event.action": "_setAuthenticated", "logrite.authenticated": authenticated });
        this._authenticated = authenticated;
		logger.warn(`<<< Exiting _setAuthenticated(authenticated=${authenticated})`, { "event.action": "_setAuthenticated", "logrite.authenticated": authenticated });
    }
Sys.Services._AuthenticationService.prototype = {
    _defaultFailedCallback: null,
    _defaultLoginCompletedCallback: null,
    _defaultLogoutCompletedCallback: null,
    _path: '',
    _timeout: 0,
    _authenticated: false,
    
    get_defaultFailedCallback: Sys$Services$_AuthenticationService$get_defaultFailedCallback,
    set_defaultFailedCallback: Sys$Services$_AuthenticationService$set_defaultFailedCallback,

    get_defaultLoginCompletedCallback: Sys$Services$_AuthenticationService$get_defaultLoginCompletedCallback,
    set_defaultLoginCompletedCallback: Sys$Services$_AuthenticationService$set_defaultLoginCompletedCallback,

    get_defaultLogoutCompletedCallback: Sys$Services$_AuthenticationService$get_defaultLogoutCompletedCallback,
    set_defaultLogoutCompletedCallback: Sys$Services$_AuthenticationService$set_defaultLogoutCompletedCallback,

    get_isLoggedIn: Sys$Services$_AuthenticationService$get_isLoggedIn,

    get_path: Sys$Services$_AuthenticationService$get_path,
    set_path: Sys$Services$_AuthenticationService$set_path,
    
    get_timeout: Sys$Services$_AuthenticationService$get_timeout,
    set_timeout: Sys$Services$_AuthenticationService$set_timeout,    
    
    login: Sys$Services$_AuthenticationService$login,
    
    logout: Sys$Services$_AuthenticationService$logout,
    
    _get_path: Sys$Services$_AuthenticationService$_get_path,
    
    _onLoginComplete: Sys$Services$_AuthenticationService$_onLoginComplete,
    
    _onLoginFailed: Sys$Services$_AuthenticationService$_onLoginFailed,
    
    _onLogoutComplete: Sys$Services$_AuthenticationService$_onLogoutComplete,
    
    _onLogoutFailed: Sys$Services$_AuthenticationService$_onLogoutFailed,
    
    _setAuthenticated: Sys$Services$_AuthenticationService$_setAuthenticated    
}

Sys.Services._AuthenticationService.registerClass('Sys.Services._AuthenticationService', Sys.Net.WebServiceProxy);
Sys.Services.AuthenticationService = new Sys.Services._AuthenticationService();

Type.registerNamespace('Sys.Serialization');


Sys.Serialization.JavaScriptSerializer = function Sys$Serialization$JavaScriptSerializer() {
	logger.error('>>> Entering JavaScriptSerializer()', { "event.action": "JavaScriptSerializer" });
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting JavaScriptSerializer()', { "event.action": "JavaScriptSerializer" });
}
Sys.Serialization.JavaScriptSerializer.registerClass('Sys.Serialization.JavaScriptSerializer');

Sys.Serialization.JavaScriptSerializer._stringRegEx = new RegExp('["\b\f\n\r\t\\\\\x00-\x1F]', 'i');

Sys.Serialization.JavaScriptSerializer._serializeWithBuilder = function Sys$Serialization$JavaScriptSerializer$_serializeWithBuilder(object, stringBuilder, sort) {
	logger.error(`>>> Entering _serializeWithBuilder(object=${object},stringBuilder=${stringBuilder},sort=${sort})`, { "event.action": "_serializeWithBuilder", "logrite.object": object, "logrite.stringBuilder": stringBuilder, "logrite.sort": sort });
    var i;
    switch (typeof object) {
    case 'object':
        if (object) {
                        if (Array.isInstanceOfType(object)) {
                stringBuilder.append('[');
                for (i = 0; i < object.length; ++i) {
                    if (i > 0) {
                        stringBuilder.append(',');
                    }
                    Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(object[i], stringBuilder);
                }
                stringBuilder.append(']');
            }
            else {
                                                                if (Date.isInstanceOfType(object)) {
                    stringBuilder.append('"\\/Date(');
                    stringBuilder.append(object.getTime());
                    stringBuilder.append(')\\/"');
                    break;
                }

                var properties = [];
                var propertyCount = 0;
                for (var name in object) {
                                        if (name.startsWith('$')) {
                        continue;
                    }
                    properties[propertyCount++] = name;
                }
                if (sort) properties.sort();

                stringBuilder.append('{');
                var needComma = false;
                for (i=0; i<propertyCount; i++) {
                    var value = object[properties[i]];
                    if (typeof value !== 'undefined' && typeof value !== 'function') {
                        if (needComma) {
                            stringBuilder.append(',');
                        }
                        else {
                            needComma = true;
                        }

                                                Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(properties[i], stringBuilder, sort);
                        stringBuilder.append(':');
                        Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(value, stringBuilder, sort);
                    }
                }
                stringBuilder.append('}');
            }
        }
        else {
            stringBuilder.append('null');
        }
        break;

    case 'number':
        if (isFinite(object)) {
            stringBuilder.append(String(object));
        }
        else {
            throw Error.invalidOperation(Sys.Res.cannotSerializeNonFiniteNumbers);
        }
        break;

    case 'string':
        stringBuilder.append('"');

                if (Sys.Browser.agent === Sys.Browser.Safari || Sys.Serialization.JavaScriptSerializer._stringRegEx.test(object)) {
            var length = object.length;
            for (i = 0; i < length; ++i) {
                var curChar = object.charAt(i);
								logger.error(`_serializeWithBuilder(object=${object},stringBuilder=${stringBuilder},sort=${sort}): curChar → ${curChar}`, { "event.action": "_serializeWithBuilder", "logrite.object": object, "logrite.stringBuilder": stringBuilder, "logrite.sort": sort });
                                if (curChar >= ' ') {
                                        if (curChar === '\\' || curChar === '"') {
                        stringBuilder.append('\\');
                    }
                    stringBuilder.append(curChar);
                }
                else {
                    switch (curChar) {
                        case '\b':
                            stringBuilder.append('\\b');
                            break;
                        case '\f':
                            stringBuilder.append('\\f');
                            break;
                        case '\n':
                            stringBuilder.append('\\n');
                            break;
                        case '\r':
                            stringBuilder.append('\\r');
                            break;
                        case '\t':
                            stringBuilder.append('\\t');
                            break;
                        default:
                                                        stringBuilder.append('\\u00');
                            if (curChar.charCodeAt() < 16) stringBuilder.append('0');
                            stringBuilder.append(curChar.charCodeAt().toString(16));
                    }
                }
            }
        } else {
            stringBuilder.append(object);
        }
        stringBuilder.append('"');
        break;

    case 'boolean':
        stringBuilder.append(object.toString());
        break;

    default:
        stringBuilder.append('null');
		logger.warn(`<<< Exiting _serializeWithBuilder(object=${object},stringBuilder=${stringBuilder},sort=${sort})`, { "event.action": "_serializeWithBuilder", "logrite.object": object, "logrite.stringBuilder": stringBuilder, "logrite.sort": sort });
        break;
    }
}

Sys.Serialization.JavaScriptSerializer.serialize = function Sys$Serialization$JavaScriptSerializer$serialize(object) {
	logger.error(`>>> Entering serialize(object=${object})`, { "event.action": "serialize", "logrite.object": object });
    /// <param name="object" mayBeNull="true"></param>
    /// <returns type="String"></returns>
    var e = Function._validateParams(arguments, [
        {name: "object", mayBeNull: true}
    ]);
    if (e) throw e;

    var stringBuilder = new Sys.StringBuilder();
	logger.error(`serialize(object=${object}): stringBuilder → ${stringBuilder}`, { "event.action": "serialize", "logrite.object": object });
    Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(object, stringBuilder, false);
	logger.warn(`<<< Exiting serialize(object=${object})`, { "event.action": "serialize", "logrite.object": object });
    return stringBuilder.toString();
}

Sys.Serialization.JavaScriptSerializer.deserialize = function Sys$Serialization$JavaScriptSerializer$deserialize(data) {
	logger.error(`>>> Entering deserialize(data=${data})`, { "event.action": "deserialize", "logrite.data": data });
    /// <param name="data" type="String"></param>
    /// <returns></returns>
    var e = Function._validateParams(arguments, [
        {name: "data", type: String}
    ]);
    if (e) throw e;

    if (data.length === 0) throw Error.argument('data', Sys.Res.cannotDeserializeEmptyString);
                                                            
    try {    
        var exp = data.replace(new RegExp('(^|[^\\\\])\\"\\\\/Date\\((-?[0-9]+)\\)\\\\/\\"', 'g'), "$1new Date($2)");
		logger.error(`deserialize(data=${data}): exp → ${exp}`, { "event.action": "deserialize", "logrite.data": data });
        return eval('(' + exp + ')');
    }
    catch (e) {
         throw Error.argument('data', Sys.Res.cannotDeserializeInvalidJson);
    }
}

Sys.CultureInfo = function Sys$CultureInfo(name, numberFormat, dateTimeFormat) {
	logger.error(`>>> Entering CultureInfo(name=${name},numberFormat=${numberFormat},dateTimeFormat=${dateTimeFormat})`, { "event.action": "CultureInfo", "logrite.name": name, "logrite.numberFormat": numberFormat, "logrite.dateTimeFormat": dateTimeFormat });
    /// <param name="name" type="String"></param>
    /// <param name="numberFormat" type="Object"></param>
    /// <param name="dateTimeFormat" type="Object"></param>
    var e = Function._validateParams(arguments, [
        {name: "name", type: String},
        {name: "numberFormat", type: Object},
        {name: "dateTimeFormat", type: Object}
    ]);
    if (e) throw e;

    this.name = name;
    this.numberFormat = numberFormat;
	logger.debug(`CultureInfo(name=${name},numberFormat=${numberFormat},dateTimeFormat=${dateTimeFormat}): name → ${name}`, { "event.action": "CultureInfo", "logrite.name": name, "logrite.numberFormat": numberFormat, "logrite.dateTimeFormat": dateTimeFormat });
	logger.debug(`CultureInfo(name=${name},numberFormat=${numberFormat},dateTimeFormat=${dateTimeFormat}): this.name → ${this.name}`, { "event.action": "CultureInfo", "logrite.name": name, "logrite.numberFormat": numberFormat, "logrite.dateTimeFormat": dateTimeFormat });
    this.dateTimeFormat = dateTimeFormat;
}

    function Sys$CultureInfo$_getDateTimeFormats() {
		logger.error('>>> Entering _getDateTimeFormats()', { "event.action": "_getDateTimeFormats" });
        if (! this._dateTimeFormats) {
            var dtf = this.dateTimeFormat;
            this._dateTimeFormats =
              [ dtf.MonthDayPattern,
                dtf.YearMonthPattern,
                dtf.ShortDatePattern,
                dtf.ShortTimePattern,
                dtf.LongDatePattern,
                dtf.LongTimePattern,
                dtf.FullDateTimePattern,
                dtf.RFC1123Pattern,
                dtf.SortableDateTimePattern,
                dtf.UniversalSortableDateTimePattern ];
        }
		logger.warn('<<< Exiting _getDateTimeFormats()', { "event.action": "_getDateTimeFormats" });
        return this._dateTimeFormats;
    }
    function Sys$CultureInfo$_getMonthIndex(value) {
		logger.error(`>>> Entering _getMonthIndex(value=${value})`, { "event.action": "_getMonthIndex", "logrite.value": value });
        if (!this._upperMonths) {
            this._upperMonths = this._toUpperArray(this.dateTimeFormat.MonthNames);
        }
		logger.warn(`<<< Exiting _getMonthIndex(value=${value})`, { "event.action": "_getMonthIndex", "logrite.value": value });
        return Array.indexOf(this._upperMonths, this._toUpper(value));
    }
    function Sys$CultureInfo$_getAbbrMonthIndex(value) {
		logger.error(`>>> Entering _getAbbrMonthIndex(value=${value})`, { "event.action": "_getAbbrMonthIndex", "logrite.value": value });
        if (!this._upperAbbrMonths) {
            this._upperAbbrMonths = this._toUpperArray(this.dateTimeFormat.AbbreviatedMonthNames);
        }
		logger.warn(`<<< Exiting _getAbbrMonthIndex(value=${value})`, { "event.action": "_getAbbrMonthIndex", "logrite.value": value });
        return Array.indexOf(this._upperMonths, this._toUpper(value));
    }
    function Sys$CultureInfo$_getDayIndex(value) {
		logger.error(`>>> Entering _getDayIndex(value=${value})`, { "event.action": "_getDayIndex", "logrite.value": value });
        if (!this._upperDays) {
            this._upperDays = this._toUpperArray(this.dateTimeFormat.DayNames);
        }
		logger.warn(`<<< Exiting _getDayIndex(value=${value})`, { "event.action": "_getDayIndex", "logrite.value": value });
        return Array.indexOf(this._upperDays, this._toUpper(value));
    }
    function Sys$CultureInfo$_getAbbrDayIndex(value) {
		logger.error(`>>> Entering _getAbbrDayIndex(value=${value})`, { "event.action": "_getAbbrDayIndex", "logrite.value": value });
        if (!this._upperAbbrDays) {
            this._upperAbbrDays = this._toUpperArray(this.dateTimeFormat.AbbreviatedDayNames);
        }
		logger.warn(`<<< Exiting _getAbbrDayIndex(value=${value})`, { "event.action": "_getAbbrDayIndex", "logrite.value": value });
        return Array.indexOf(this._upperAbbrDays, this._toUpper(value));
    }
    function Sys$CultureInfo$_toUpperArray(arr) {
		logger.error(`>>> Entering _toUpperArray(arr=${arr})`, { "event.action": "_toUpperArray", "logrite.arr": arr });
        var result = [];
        for (var i = 0, il = arr.length; i < il; i++) {
            result[i] = this._toUpper(arr[i]);
        }
		logger.error(`_toUpperArray(arr=${arr}): result → ${result}`, { "event.action": "_toUpperArray", "logrite.arr": arr });
        return result;
    }
    function Sys$CultureInfo$_toUpper(value) {
						logger.error(`>>> Entering _toUpper(value=${value})`, { "event.action": "_toUpper", "logrite.value": value });
						logger.warn(`<<< Exiting _toUpper(value=${value})`, { "event.action": "_toUpper", "logrite.value": value });
                        return value.split("\u00A0").join(' ').toUpperCase();
    }
Sys.CultureInfo.prototype = {
    _getDateTimeFormats: Sys$CultureInfo$_getDateTimeFormats,
    _getMonthIndex: Sys$CultureInfo$_getMonthIndex,
    _getAbbrMonthIndex: Sys$CultureInfo$_getAbbrMonthIndex,
    _getDayIndex: Sys$CultureInfo$_getDayIndex,
    _getAbbrDayIndex: Sys$CultureInfo$_getAbbrDayIndex,
    _toUpperArray: Sys$CultureInfo$_toUpperArray,
    _toUpper: Sys$CultureInfo$_toUpper
}
Sys.CultureInfo._parse = function Sys$CultureInfo$_parse(value) {
    var cultureInfo = Sys.Serialization.JavaScriptSerializer.deserialize(value);
	logger.error(`_parse(value=${value}): cultureInfo → ${cultureInfo}`, { "event.action": "_parse", "logrite.value": value });
    return new Sys.CultureInfo(cultureInfo.name, cultureInfo.numberFormat, cultureInfo.dateTimeFormat);
}
Sys.CultureInfo.registerClass('Sys.CultureInfo');

Sys.CultureInfo.InvariantCulture = Sys.CultureInfo._parse('{"name":"","numberFormat":{"CurrencyDecimalDigits":2,"CurrencyDecimalSeparator":".","IsReadOnly":true,"CurrencyGroupSizes":[3],"NumberGroupSizes":[3],"PercentGroupSizes":[3],"CurrencyGroupSeparator":",","CurrencySymbol":"\u00A4","NaNSymbol":"NaN","CurrencyNegativePattern":0,"NumberNegativePattern":1,"PercentPositivePattern":0,"PercentNegativePattern":0,"NegativeInfinitySymbol":"-Infinity","NegativeSign":"-","NumberDecimalDigits":2,"NumberDecimalSeparator":".","NumberGroupSeparator":",","CurrencyPositivePattern":0,"PositiveInfinitySymbol":"Infinity","PositiveSign":"+","PercentDecimalDigits":2,"PercentDecimalSeparator":".","PercentGroupSeparator":",","PercentSymbol":"%","PerMilleSymbol":"\u2030","NativeDigits":["0","1","2","3","4","5","6","7","8","9"],"DigitSubstitution":1},"dateTimeFormat":{"AMDesignator":"AM","Calendar":{"MinSupportedDateTime":"@-62135568000000@","MaxSupportedDateTime":"@253402300799999@","AlgorithmType":1,"CalendarType":1,"Eras":[1],"TwoDigitYearMax":2029,"IsReadOnly":true},"DateSeparator":"/","FirstDayOfWeek":0,"CalendarWeekRule":0,"FullDateTimePattern":"dddd, dd MMMM yyyy HH:mm:ss","LongDatePattern":"dddd, dd MMMM yyyy","LongTimePattern":"HH:mm:ss","MonthDayPattern":"MMMM dd","PMDesignator":"PM","RFC1123Pattern":"ddd, dd MMM yyyy HH\':\'mm\':\'ss \'GMT\'","ShortDatePattern":"MM/dd/yyyy","ShortTimePattern":"HH:mm","SortableDateTimePattern":"yyyy\'-\'MM\'-\'dd\'T\'HH\':\'mm\':\'ss","TimeSeparator":":","UniversalSortableDateTimePattern":"yyyy\'-\'MM\'-\'dd HH\':\'mm\':\'ss\'Z\'","YearMonthPattern":"yyyy MMMM","AbbreviatedDayNames":["Sun","Mon","Tue","Wed","Thu","Fri","Sat"],"ShortestDayNames":["Su","Mo","Tu","We","Th","Fr","Sa"],"DayNames":["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"],"AbbreviatedMonthNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthNames":["January","February","March","April","May","June","July","August","September","October","November","December",""],"IsReadOnly":true,"NativeCalendarName":"Gregorian Calendar","AbbreviatedMonthGenitiveNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthGenitiveNames":["January","February","March","April","May","June","July","August","September","October","November","December",""]}}');

if (typeof(__cultureInfo) === 'undefined') {
    var __cultureInfo = '{"name":"en-US","numberFormat":{"CurrencyDecimalDigits":2,"CurrencyDecimalSeparator":".","IsReadOnly":false,"CurrencyGroupSizes":[3],"NumberGroupSizes":[3],"PercentGroupSizes":[3],"CurrencyGroupSeparator":",","CurrencySymbol":"$","NaNSymbol":"NaN","CurrencyNegativePattern":0,"NumberNegativePattern":1,"PercentPositivePattern":0,"PercentNegativePattern":0,"NegativeInfinitySymbol":"-Infinity","NegativeSign":"-","NumberDecimalDigits":2,"NumberDecimalSeparator":".","NumberGroupSeparator":",","CurrencyPositivePattern":0,"PositiveInfinitySymbol":"Infinity","PositiveSign":"+","PercentDecimalDigits":2,"PercentDecimalSeparator":".","PercentGroupSeparator":",","PercentSymbol":"%","PerMilleSymbol":"\u2030","NativeDigits":["0","1","2","3","4","5","6","7","8","9"],"DigitSubstitution":1},"dateTimeFormat":{"AMDesignator":"AM","Calendar":{"MinSupportedDateTime":"@-62135568000000@","MaxSupportedDateTime":"@253402300799999@","AlgorithmType":1,"CalendarType":1,"Eras":[1],"TwoDigitYearMax":2029,"IsReadOnly":false},"DateSeparator":"/","FirstDayOfWeek":0,"CalendarWeekRule":0,"FullDateTimePattern":"dddd, MMMM dd, yyyy h:mm:ss tt","LongDatePattern":"dddd, MMMM dd, yyyy","LongTimePattern":"h:mm:ss tt","MonthDayPattern":"MMMM dd","PMDesignator":"PM","RFC1123Pattern":"ddd, dd MMM yyyy HH\':\'mm\':\'ss \'GMT\'","ShortDatePattern":"M/d/yyyy","ShortTimePattern":"h:mm tt","SortableDateTimePattern":"yyyy\'-\'MM\'-\'dd\'T\'HH\':\'mm\':\'ss","TimeSeparator":":","UniversalSortableDateTimePattern":"yyyy\'-\'MM\'-\'dd HH\':\'mm\':\'ss\'Z\'","YearMonthPattern":"MMMM, yyyy","AbbreviatedDayNames":["Sun","Mon","Tue","Wed","Thu","Fri","Sat"],"ShortestDayNames":["Su","Mo","Tu","We","Th","Fr","Sa"],"DayNames":["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"],"AbbreviatedMonthNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthNames":["January","February","March","April","May","June","July","August","September","October","November","December",""],"IsReadOnly":false,"NativeCalendarName":"Gregorian Calendar","AbbreviatedMonthGenitiveNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthGenitiveNames":["January","February","March","April","May","June","July","August","September","October","November","December",""]}}';
}
Sys.CultureInfo.CurrentCulture = Sys.CultureInfo._parse(__cultureInfo);
delete __cultureInfo;

Sys.UI.Point = function Sys$UI$Point(x, y) {
	logger.error(`>>> Entering Point(x=${x},y=${y})`, { "event.action": "Point", "logrite.x": x, "logrite.y": y });
    /// <param name="x" type="Number" integer="true"></param>
    /// <param name="y" type="Number" integer="true"></param>
    /// <field name="x" type="Number" integer="true"></field>
    /// <field name="y" type="Number" integer="true"></field>
    var e = Function._validateParams(arguments, [
        {name: "x", type: Number, integer: true},
        {name: "y", type: Number, integer: true}
    ]);
    if (e) throw e;

    this.x = x;
    this.y = y;
	logger.debug(`Point(x=${x},y=${y}): this.x → ${this.x}`, { "event.action": "Point", "logrite.x": x, "logrite.y": y });
}
Sys.UI.Point.registerClass('Sys.UI.Point');
Sys.UI.Bounds = function Sys$UI$Bounds(x, y, width, height) {
	logger.error(`>>> Entering Bounds(x=${x},y=${y},width=${width},height=${height})`, { "event.action": "Bounds", "logrite.x": x, "logrite.y": y, "logrite.width": width, "logrite.height": height });
    /// <param name="x" type="Number" integer="true"></param>
    /// <param name="y" type="Number" integer="true"></param>
    /// <param name="height" type="Number" integer="true"></param>
    /// <param name="width" type="Number" integer="true"></param>
    /// <field name="x" type="Number" integer="true"></field>
    /// <field name="y" type="Number" integer="true"></field>
    /// <field name="height" type="Number" integer="true"></field>
    /// <field name="width" type="Number" integer="true"></field>
    var e = Function._validateParams(arguments, [
        {name: "x", type: Number, integer: true},
        {name: "y", type: Number, integer: true},
        {name: "height", type: Number, integer: true},
        {name: "width", type: Number, integer: true}
    ]);
    if (e) throw e;

    this.x = x;
    this.y = y;
	logger.debug(`Bounds(x=${x},y=${y},width=${width},height=${height}): x → ${x}`, { "event.action": "Bounds", "logrite.x": x, "logrite.y": y, "logrite.width": width, "logrite.height": height });
	logger.debug(`Bounds(x=${x},y=${y},width=${width},height=${height}): this.x → ${this.x}`, { "event.action": "Bounds", "logrite.x": x, "logrite.y": y, "logrite.width": width, "logrite.height": height });
	logger.debug(`Bounds(x=${x},y=${y},width=${width},height=${height}): y → ${y}`, { "event.action": "Bounds", "logrite.x": x, "logrite.y": y, "logrite.width": width, "logrite.height": height });
    this.height = height;
    this.width = width;
}
Sys.UI.Bounds.registerClass('Sys.UI.Bounds');
Sys.UI.DomElement = function Sys$UI$DomElement() {
	logger.error('>>> Entering DomElement()', { "event.action": "DomElement" });
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting DomElement()', { "event.action": "DomElement" });
    throw Error.notImplemented();
}
Sys.UI.DomElement.registerClass('Sys.UI.DomElement');

Sys.UI.DomElement.addCssClass = function Sys$UI$DomElement$addCssClass(element, className) {
    /// <param name="element" domElement="true"></param>
    /// <param name="className" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "className", type: String}
    ]);
    if (e) throw e;

    if (!Sys.UI.DomElement.containsCssClass(element, className)) {
        if (element.className === '') {
            element.className = className;
        }
        else {
            element.className += ' ' + className;
        }
    }
}

Sys.UI.DomElement.containsCssClass = function Sys$UI$DomElement$containsCssClass(element, className) {
    /// <param name="element" domElement="true"></param>
    /// <param name="className" type="String"></param>
    /// <returns type="Boolean"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "className", type: String}
    ]);
    if (e) throw e;

    return Array.contains(element.className.split(' '), className);
}

Sys.UI.DomElement.getBounds = function Sys$UI$DomElement$getBounds(element) {
	logger.error(`>>> Entering getBounds(element=${element})`, { "event.action": "getBounds", "logrite.element": element });
    /// <param name="element" domElement="true"></param>
    /// <returns type="Sys.UI.Bounds"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    var offset = Sys.UI.DomElement.getLocation(element);
	logger.error(`getBounds(element=${element}): offset → ${offset}`, { "event.action": "getBounds", "logrite.element": element });

	logger.warn(`<<< Exiting getBounds(element=${element})`, { "event.action": "getBounds", "logrite.element": element });
    return new Sys.UI.Bounds(offset.x, offset.y, element.offsetWidth || 0, element.offsetHeight || 0);
}

var $get = Sys.UI.DomElement.getElementById = function Sys$UI$DomElement$getElementById(id, element) {
	logger.error(`>>> Entering getElementById(id=${id},element=${element})`, { "event.action": "getElementById", "logrite.id": id, "logrite.element": element });
    /// <param name="id" type="String"></param>
    /// <param name="element" domElement="true" optional="true" mayBeNull="true"></param>
    /// <returns domElement="true" mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "id", type: String},
        {name: "element", mayBeNull: true, domElement: true, optional: true}
    ]);
    if (e) throw e;

    if (!element) return document.getElementById(id);
    if (element.getElementById) return element.getElementById(id);

        var nodeQueue = [];
    var childNodes = element.childNodes;
    for (var i = 0; i < childNodes.length; i++) {
        var node = childNodes[i];
        if (node.nodeType == 1) {
            nodeQueue[nodeQueue.length] = node;
        }
    }

    while (nodeQueue.length) {
        node = nodeQueue.shift();
        if (node.id == id) {
            return node;
        }
        childNodes = node.childNodes;
        for (i = 0; i < childNodes.length; i++) {
            node = childNodes[i];
            if (node.nodeType == 1) {
                nodeQueue[nodeQueue.length] = node;
            }
        }
    }

	logger.warn(`<<< Exiting getElementById(id=${id},element=${element})`, { "event.action": "getElementById", "logrite.id": id, "logrite.element": element });
    return null;
}



switch(Sys.Browser.agent) {
    case Sys.Browser.InternetExplorer:
        Sys.UI.DomElement.getLocation = function Sys$UI$DomElement$getLocation(element) {
			logger.error(`>>> Entering getLocation(element=${element})`, { "event.action": "getLocation", "logrite.element": element });
            /// <param name="element" domElement="true"></param>
            /// <returns type="Sys.UI.Point"></returns>
            var e = Function._validateParams(arguments, [
                {name: "element", domElement: true}
            ]);
            if (e) throw e;

                        if (element.self || element.nodeType === 9) return new Sys.UI.Point(0,0);

                                                var clientRects = element.getClientRects();
												logger.error(`getLocation(element=${element}): clientRects → ${clientRects}`, { "event.action": "getLocation", "logrite.element": element });
            if (!clientRects || !clientRects.length) {
                return new Sys.UI.Point(0,0);
            }

            var w = element.ownerDocument.parentWindow;
                                                var offsetL = w.screenLeft - top.screenLeft - top.document.documentElement.scrollLeft + 2;
            var offsetT = w.screenTop - top.screenTop - top.document.documentElement.scrollTop + 2;

                                                                        var f = w.frameElement || null;
            if (f) {
                                                                                var fstyle = f.currentStyle;
                offsetL += (f.frameBorder || 1) * 2 +
                    (parseInt(fstyle.paddingLeft) || 0) +
                    (parseInt(fstyle.borderLeftWidth) || 0) -
                    element.ownerDocument.documentElement.scrollLeft;
                offsetT += (f.frameBorder || 1) * 2 +
                    (parseInt(fstyle.paddingTop) || 0) +
                    (parseInt(fstyle.borderTopWidth) || 0) -
                    element.ownerDocument.documentElement.scrollTop;
            }

            var clientRect = clientRects[0];

			logger.warn(`<<< Exiting getLocation(element=${element})`, { "event.action": "getLocation", "logrite.element": element });
            return new Sys.UI.Point(
                clientRect.left - offsetL,
                clientRect.top - offsetT);
        }
        break;
    case Sys.Browser.Safari:
        Sys.UI.DomElement.getLocation = function Sys$UI$DomElement$getLocation(element) {
            /// <param name="element" domElement="true"></param>
            /// <returns type="Sys.UI.Point"></returns>
            var e = Function._validateParams(arguments, [
                {name: "element", domElement: true}
            ]);
            if (e) throw e;

                        if ((element.window && (element.window === element)) || element.nodeType === 9) return new Sys.UI.Point(0,0);

                                                var offsetX = 0;
            var offsetY = 0;

            var previous = null;
            var previousStyle = null;
            var currentStyle;
            for (var parent = element; parent; previous = parent, previousStyle = currentStyle, parent = parent.offsetParent) {
                currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);
                var tagName = parent.tagName;

                                                                                if ((parent.offsetLeft || parent.offsetTop) &&
                    ((tagName !== "BODY") || (!previousStyle || previousStyle.position !== "absolute"))) {

                    offsetX += parent.offsetLeft;
                    offsetY += parent.offsetTop;
                }
            }

            currentStyle = Sys.UI.DomElement._getCurrentStyle(element);
            var elementPosition = currentStyle ? currentStyle.position : null;
            var elementPositioned = elementPosition && (elementPosition !== "static");
                        if (!elementPosition || (elementPosition !== "absolute")) {
                                for (var parent = element.parentNode; parent; parent = parent.parentNode) {
                    tagName = parent.tagName;

                    if ((tagName !== "BODY") && (tagName !== "HTML") && (parent.scrollLeft || parent.scrollTop)) {
                        offsetX -= (parent.scrollLeft || 0);
                        offsetY -= (parent.scrollTop || 0);
                    }
                    currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);
                    var parentPosition = currentStyle ? currentStyle.position : null;

                                        if (parentPosition && (parentPosition === "absolute")) break;
                }
            }

            return new Sys.UI.Point(offsetX, offsetY);
        }
        break;
    case Sys.Browser.Opera:
        Sys.UI.DomElement.getLocation = function Sys$UI$DomElement$getLocation(element) {
            /// <param name="element" domElement="true"></param>
            /// <returns type="Sys.UI.Point"></returns>
            var e = Function._validateParams(arguments, [
                {name: "element", domElement: true}
            ]);
            if (e) throw e;

                        if ((element.window && (element.window === element)) || element.nodeType === 9) return new Sys.UI.Point(0,0);

                                                var offsetX = 0;
            var offsetY = 0;

            var previous = null;
            for (var parent = element; parent; previous = parent, parent = parent.offsetParent) {

                var tagName = parent.tagName;

                offsetX += parent.offsetLeft || 0;
                offsetY += parent.offsetTop || 0;
            }

                        var elementPosition = element.style.position;
            var elementPositioned = elementPosition && (elementPosition !== "static");

                        for (var parent = element.parentNode; parent; parent = parent.parentNode) {
                tagName = parent.tagName;

                if ((tagName !== "BODY") && (tagName !== "HTML") && (parent.scrollLeft || parent.scrollTop) &&
                    ((elementPositioned &&
                    ((parent.style.overflow === "scroll") || (parent.style.overflow === "auto"))))) {
                                        
                    offsetX -= (parent.scrollLeft || 0);
                    offsetY -= (parent.scrollTop || 0);
                }
                var parentPosition = (parent && parent.style) ? parent.style.position : null;

                                elementPositioned = elementPositioned || (parentPosition && (parentPosition !== "static"));
            }


            return new Sys.UI.Point(offsetX, offsetY);
        }
        break;
    default:
        Sys.UI.DomElement.getLocation = function Sys$UI$DomElement$getLocation(element) {
            /// <param name="element" domElement="true"></param>
            /// <returns type="Sys.UI.Point"></returns>
            var e = Function._validateParams(arguments, [
                {name: "element", domElement: true}
            ]);
            if (e) throw e;

                        if ((element.window && (element.window === element)) || element.nodeType === 9) return new Sys.UI.Point(0,0);

            var offsetX = 0;
            var offsetY = 0;
            var previous = null;
            var previousStyle = null;
            var currentStyle = null;
            for (var parent = element; parent; previous = parent, previousStyle = currentStyle, parent = parent.offsetParent) {
                var tagName = parent.tagName;
                currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);

                                                if ((parent.offsetLeft || parent.offsetTop) &&
                    !((tagName === "BODY") &&
                    (!previousStyle || previousStyle.position !== "absolute"))) {

                    offsetX += parent.offsetLeft;
                    offsetY += parent.offsetTop;
                }

                                                if (previous !== null && currentStyle) {
                                                                                                                                            if ((tagName !== "TABLE") && (tagName !== "TD") && (tagName !== "HTML")) {
                        offsetX += parseInt(currentStyle.borderLeftWidth) || 0;
                        offsetY += parseInt(currentStyle.borderTopWidth) || 0;
                    }
                    if (tagName === "TABLE" &&
                        (currentStyle.position === "relative" || currentStyle.position === "absolute")) {
                        offsetX += parseInt(currentStyle.marginLeft) || 0;
                        offsetY += parseInt(currentStyle.marginTop) || 0;
                    }
                }
            }

            currentStyle = Sys.UI.DomElement._getCurrentStyle(element);
            var elementPosition = currentStyle ? currentStyle.position : null;
            var elementPositioned = elementPosition && (elementPosition !== "static");
                        if (!elementPosition || (elementPosition !== "absolute")) {
                                                                                for (var parent = element.parentNode; parent; parent = parent.parentNode) {
                                                                                tagName = parent.tagName;

                    if ((tagName !== "BODY") && (tagName !== "HTML") && (parent.scrollLeft || parent.scrollTop)) {

                        offsetX -= (parent.scrollLeft || 0);
                        offsetY -= (parent.scrollTop || 0);

                        currentStyle = Sys.UI.DomElement._getCurrentStyle(parent);
                        offsetX += parseInt(currentStyle.borderLeftWidth) || 0;
                        offsetY += parseInt(currentStyle.borderTopWidth) || 0;
                    }
                }
            }

            return new Sys.UI.Point(offsetX, offsetY);
        }
        break;

}


Sys.UI.DomElement.removeCssClass = function Sys$UI$DomElement$removeCssClass(element, className) {
    /// <param name="element" domElement="true"></param>
    /// <param name="className" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "className", type: String}
    ]);
    if (e) throw e;

    var currentClassName = ' ' + element.className + ' ';
    var index = currentClassName.indexOf(' ' + className + ' ');
    if (index >= 0) {
        element.className = (currentClassName.substr(0, index) + ' ' +
            currentClassName.substring(index + className.length + 1, currentClassName.length)).trim();
    }
}

Sys.UI.DomElement.setLocation = function Sys$UI$DomElement$setLocation(element, x, y) {
	logger.error(`>>> Entering setLocation(element=${element},x=${x},y=${y})`, { "event.action": "setLocation", "logrite.element": element, "logrite.x": x, "logrite.y": y });
    /// <param name="element" domElement="true"></param>
    /// <param name="x" type="Number" integer="true"></param>
    /// <param name="y" type="Number" integer="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "x", type: Number, integer: true},
        {name: "y", type: Number, integer: true}
    ]);
    if (e) throw e;

    var style = element.style;
	logger.debug(`setLocation(element=${element},x=${x},y=${y}): style.left → ${style.left}`, { "event.action": "setLocation", "logrite.element": element, "logrite.x": x, "logrite.y": y });
    style.position = 'absolute';
	logger.debug(`setLocation(element=${element},x=${x},y=${y}): style.position → ${style.position}`, { "event.action": "setLocation", "logrite.element": element, "logrite.x": x, "logrite.y": y });
    style.left = x + "px";
    style.top = y + "px";
	logger.debug(`setLocation(element=${element},x=${x},y=${y}): style.top → ${style.top}`, { "event.action": "setLocation", "logrite.element": element, "logrite.x": x, "logrite.y": y });
}

Sys.UI.DomElement.toggleCssClass = function Sys$UI$DomElement$toggleCssClass(element, className) {
    /// <param name="element" domElement="true"></param>
    /// <param name="className" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "className", type: String}
    ]);
    if (e) throw e;

    if (Sys.UI.DomElement.containsCssClass(element, className)) {
        Sys.UI.DomElement.removeCssClass(element, className);
    }
    else {
        Sys.UI.DomElement.addCssClass(element, className);
    }
}

Sys.UI.DomElement._getCurrentStyle = function Sys$UI$DomElement$_getCurrentStyle(element) {
	logger.error(`>>> Entering _getCurrentStyle(element=${element})`, { "event.action": "_getCurrentStyle", "logrite.element": element });
    var w = (element.ownerDocument ? element.ownerDocument : element.documentElement).defaultView;
	logger.error(`_getCurrentStyle(element=${element}): w → ${w}`, { "event.action": "_getCurrentStyle", "logrite.element": element });
    return ((w && (element !== w) && w.getComputedStyle) ? w.getComputedStyle(element, null) : element.style);
}
Sys.UI.Behavior = function Sys$UI$Behavior(element) {
	logger.error(`>>> Entering Behavior(element=${element})`, { "event.action": "Behavior", "logrite.element": element });
    /// <param name="element" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    Sys.UI.Behavior.initializeBase(this);

    this._element = element;

	logger.debug(`Behavior(element=${element}): this._element → ${this._element}`, { "event.action": "Behavior", "logrite.element": element });
    var behaviors = element._behaviors;
    if (!behaviors) {
        element._behaviors = [this];
		logger.debug(`Behavior(element=${element}): element._behaviors → ${element._behaviors}`, { "event.action": "Behavior", "logrite.element": element });
    }
    else {
        behaviors[behaviors.length] = this;
		logger.warn(`<<< Exiting Behavior(element=${element})`, { "event.action": "Behavior", "logrite.element": element });
    }
}


    function Sys$UI$Behavior$get_element() {
		logger.error('>>> Entering get_element()', { "event.action": "get_element" });
        /// <value domElement="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_element()', { "event.action": "get_element" });
        return this._element;
    }
    function Sys$UI$Behavior$get_id() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        var baseId = Sys.UI.Behavior.callBaseMethod(this, 'get_id');
        if (baseId) return baseId;
        if (!this._element || !this._element.id) return '';
        return this._element.id + '$' + this.get_name();
    }
    function Sys$UI$Behavior$get_name() {
		logger.error('>>> Entering get_name()', { "event.action": "get_name" });
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._name) return this._name;
        var name = Object.getTypeName(this);
		logger.error(`get_name(): name → ${name}`, { "event.action": "get_name" });
        var i = name.lastIndexOf('.');
        if (i != -1) name = name.substr(i + 1);
        if (!this.get_isInitialized()) this._name = name;
		logger.warn('<<< Exiting get_name()', { "event.action": "get_name" });
        return name;
    }
    function Sys$UI$Behavior$set_name(value) {
		logger.error(`>>> Entering set_name(value=${value})`, { "event.action": "set_name", "logrite.value": value });
        if ((value === '') || (value.charAt(0) === ' ') || (value.charAt(value.length - 1) === ' '))
            throw Error.argument('value', Sys.Res.invalidId);
        if (typeof(this._element[value]) !== 'undefined')
            throw Error.invalidOperation(String.format(Sys.Res.behaviorDuplicateName, value));
        if (this.get_isInitialized()) throw Error.invalidOperation(Sys.Res.cantSetNameAfterInit);
        this._name = value;
		logger.debug(`set_name(value=${value}): this._name → ${this._name}`, { "event.action": "set_name", "logrite.value": value });
    }
    function Sys$UI$Behavior$initialize() {
        Sys.UI.Behavior.callBaseMethod(this, 'initialize');
        var name = this.get_name();
        if (name) this._element[name] = this;
		logger.error(`initialize(): name → ${name}`, { "event.action": "initialize" });
    }
    function Sys$UI$Behavior$dispose() {
        Sys.UI.Behavior.callBaseMethod(this, 'dispose');
        if (this._element) {
            var name = this.get_name();
			logger.error(`dispose(): name → ${name}`, { "event.action": "dispose" });
            if (name) {
                this._element[name] = null;
            }
            Array.remove(this._element._behaviors, this);
            delete this._element;
        }
    }
Sys.UI.Behavior.prototype = {
    _name: null,
    get_element: Sys$UI$Behavior$get_element,
    get_id: Sys$UI$Behavior$get_id,
    get_name: Sys$UI$Behavior$get_name,
    set_name: Sys$UI$Behavior$set_name,
    initialize: Sys$UI$Behavior$initialize,
    dispose: Sys$UI$Behavior$dispose
}
Sys.UI.Behavior.registerClass('Sys.UI.Behavior', Sys.Component);

Sys.UI.Behavior.getBehaviorByName = function Sys$UI$Behavior$getBehaviorByName(element, name) {
	logger.error(`>>> Entering getBehaviorByName(element=${element},name=${name})`, { "event.action": "getBehaviorByName", "logrite.element": element, "logrite.name": name });
    /// <param name="element" domElement="true"></param>
    /// <param name="name" type="String"></param>
    /// <returns type="Sys.UI.Behavior" mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "name", type: String}
    ]);
    if (e) throw e;

    var b = element[name];
	logger.warn(`<<< Exiting getBehaviorByName(element=${element},name=${name})`, { "event.action": "getBehaviorByName", "logrite.element": element, "logrite.name": name });
    return (b && Sys.UI.Behavior.isInstanceOfType(b)) ? b : null;
}

Sys.UI.Behavior.getBehaviors = function Sys$UI$Behavior$getBehaviors(element) {
	logger.error(`>>> Entering getBehaviors(element=${element})`, { "event.action": "getBehaviors", "logrite.element": element });
    /// <param name="element" domElement="true"></param>
    /// <returns type="Array" elementType="Sys.UI.Behavior"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    if (!element._behaviors) return [];
	logger.warn(`<<< Exiting getBehaviors(element=${element})`, { "event.action": "getBehaviors", "logrite.element": element });
    return Array.clone(element._behaviors);
}

Sys.UI.Behavior.getBehaviorsByType = function Sys$UI$Behavior$getBehaviorsByType(element, type) {
	logger.error(`>>> Entering getBehaviorsByType(element=${element},type=${type})`, { "event.action": "getBehaviorsByType", "logrite.element": element, "logrite.type": type });
    /// <param name="element" domElement="true"></param>
    /// <param name="type" type="Type"></param>
    /// <returns type="Array" elementType="Sys.UI.Behavior"></returns>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true},
        {name: "type", type: Type}
    ]);
    if (e) throw e;

    var behaviors = element._behaviors;
    var results = [];
    if (behaviors) {
        for (var i = 0, l = behaviors.length; i < l; i++) {
            if (type.isInstanceOfType(behaviors[i])) {
                results[results.length] = behaviors[i];
            }
        }
    }
	logger.error(`getBehaviorsByType(element=${element},type=${type}): results → ${results}`, { "event.action": "getBehaviorsByType", "logrite.element": element, "logrite.type": type });
    return results;
}
Sys.UI.VisibilityMode = function Sys$UI$VisibilityMode() {
	logger.error('>>> Entering VisibilityMode()', { "event.action": "VisibilityMode" });
    /// <field name="hide" type="Number" integer="true" static="true"></field>
    /// <field name="collapse" type="Number" integer="true" static="true"></field>
    if (arguments.length !== 0) throw Error.parameterCount();
	logger.warn('<<< Exiting VisibilityMode()', { "event.action": "VisibilityMode" });
    throw Error.notImplemented();
}



Sys.UI.VisibilityMode.prototype = {
    hide: 0,
    collapse: 1
}
Sys.UI.VisibilityMode.registerEnum("Sys.UI.VisibilityMode");

Sys.UI.Control = function Sys$UI$Control(element) {
	logger.error(`>>> Entering Control(element=${element})`, { "event.action": "Control", "logrite.element": element });
    /// <param name="element" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    if (typeof(element.control) != 'undefined') throw Error.invalidOperation(Sys.Res.controlAlreadyDefined);
    Sys.UI.Control.initializeBase(this);

    this._element = element;
    element.control = this;
	logger.debug(`Control(element=${element}): this._element → ${this._element}`, { "event.action": "Control", "logrite.element": element });
	logger.debug(`Control(element=${element}): element.control → ${element.control}`, { "event.action": "Control", "logrite.element": element });

    this._oldDisplayMode = this._element.style.display;
    if (!this._oldDisplayMode || (this._oldDisplayMode == 'none')) {
        this._oldDisplayMode = '';
		logger.debug(`Control(element=${element}): this._oldDisplayMode → ${this._oldDisplayMode}`, { "event.action": "Control", "logrite.element": element });
		logger.warn(`<<< Exiting Control(element=${element})`, { "event.action": "Control", "logrite.element": element });
    }
}




    function Sys$UI$Control$get_element() {
        /// <value domElement="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._element;
    }
    function Sys$UI$Control$get_id() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._element) return '';
        return this._element.id;
    }
    function Sys$UI$Control$set_id(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        throw Error.invalidOperation(Sys.Res.cantSetId);
    }
    function Sys$UI$Control$get_parent() {
		logger.error('>>> Entering get_parent()', { "event.action": "get_parent" });
        /// <value type="Sys.UI.Control"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._parent) {
            return this._parent;
        }
        else {
            var parentElement = this._element.parentNode;
            while (parentElement) {
                if (parentElement.control) {
                    return parentElement.control;
                }
                parentElement = parentElement.parentNode;
            }
			logger.warn('<<< Exiting get_parent()', { "event.action": "get_parent" });
            return null;
        }
    }
    function Sys$UI$Control$set_parent(value) {
		logger.error(`>>> Entering set_parent(value=${value})`, { "event.action": "set_parent", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Sys.UI.Control}]);
        if (e) throw e;
		logger.error(`set_parent(value=${value}): e → ${e}`, { "event.action": "set_parent", "logrite.value": value });

        var parents = [this];
        var current = value;
        while (current) {
            if (Array.contains(parents, current)) throw Error.invalidOperation(Sys.Res.circularParentChain);
            parents[parents.length] = current;
            current = current.get_parent();
        }
        this._parent = value;
		logger.error(`set_parent(value=${value}): current → ${current}`, { "event.action": "set_parent", "logrite.value": value });
    }
    function Sys$UI$Control$get_visibilityMode() {
		logger.error('>>> Entering get_visibilityMode()', { "event.action": "get_visibilityMode" });
        /// <value type="Sys.UI.VisibilityMode"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_visibilityMode()', { "event.action": "get_visibilityMode" });
        return this._visibilityMode;
    }
    function Sys$UI$Control$set_visibilityMode(value) {
		logger.error(`>>> Entering set_visibilityMode(value=${value})`, { "event.action": "set_visibilityMode", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Sys.UI.VisibilityMode}]);
        if (e) throw e;
		logger.error(`set_visibilityMode(value=${value}): e → ${e}`, { "event.action": "set_visibilityMode", "logrite.value": value });

        if (this._visibilityMode !== value) {
            this._visibilityMode = value;
            if (this.get_visible() === false) {
				logger.debug(`set_visibilityMode(value=${value}): this._visibilityMode → ${this._visibilityMode}`, { "event.action": "set_visibilityMode", "logrite.value": value });
                if (this._visibilityMode === Sys.UI.VisibilityMode.hide) {
                    this._element.style.display = this._oldDisplayMode;
                }
                else {
                    this._element.style.display = 'none';
                }
            }
        }
        this._visibilityMode = value;
		logger.warn(`<<< Exiting set_visibilityMode(value=${value})`, { "event.action": "set_visibilityMode", "logrite.value": value });
    }
    function Sys$UI$Control$get_visible() {
		logger.error('>>> Entering get_visible()', { "event.action": "get_visible" });
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
		logger.warn('<<< Exiting get_visible()', { "event.action": "get_visible" });
        return (this._element.style.visibility != 'hidden');
    }
    function Sys$UI$Control$set_visible(value) {
		logger.error(`>>> Entering set_visible(value=${value})`, { "event.action": "set_visible", "logrite.value": value });
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;
		logger.error(`set_visible(value=${value}): e → ${e}`, { "event.action": "set_visible", "logrite.value": value });

        if (value != this.get_visible()) {
            this._element.style.visibility = value ? 'visible' : 'hidden';
            if (value || (this._visibilityMode === Sys.UI.VisibilityMode.hide)) {
                this._element.style.display = this._oldDisplayMode;
            }
            else {
                this._element.style.display = 'none';
				logger.warn(`<<< Exiting set_visible(value=${value})`, { "event.action": "set_visible", "logrite.value": value });
            }
        }
    }
    function Sys$UI$Control$addCssClass(className) {
        /// <param name="className" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "className", type: String}
        ]);
        if (e) throw e;

        Sys.UI.DomElement.addCssClass(this._element, className);
    }
    function Sys$UI$Control$dispose() {
        Sys.UI.Control.callBaseMethod(this, 'dispose');
        if (this._element) {
            this._element.control = undefined;
            delete this._element;
        }
    }
    function Sys$UI$Control$initialize() {
        Sys.UI.Control.callBaseMethod(this, 'initialize');
        var elt = this._element;
    }
    function Sys$UI$Control$onBubbleEvent(source, args) {
		logger.error(`>>> Entering onBubbleEvent(source=${source},args=${args})`, { "event.action": "onBubbleEvent", "logrite.source": source, "logrite.args": args });
        /// <param name="source"></param>
        /// <param name="args" type="Sys.EventArgs"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "source"},
            {name: "args", type: Sys.EventArgs}
        ]);
        if (e) throw e;

		logger.warn(`<<< Exiting onBubbleEvent(source=${source},args=${args})`, { "event.action": "onBubbleEvent", "logrite.source": source, "logrite.args": args });
        return false;
    }
    function Sys$UI$Control$raiseBubbleEvent(source, args) {
		logger.error(`>>> Entering raiseBubbleEvent(source=${source},args=${args})`, { "event.action": "raiseBubbleEvent", "logrite.source": source, "logrite.args": args });
        /// <param name="source"></param>
        /// <param name="args" type="Sys.EventArgs"></param>
        var e = Function._validateParams(arguments, [
            {name: "source"},
            {name: "args", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        var currentTarget = this.get_parent();
		logger.error(`raiseBubbleEvent(source=${source},args=${args}): currentTarget → ${currentTarget}`, { "event.action": "raiseBubbleEvent", "logrite.source": source, "logrite.args": args });
        while (currentTarget) {
            if (currentTarget.onBubbleEvent(source, args)) {
                return;
            }
            currentTarget = currentTarget.get_parent();
        }
    }
    function Sys$UI$Control$removeCssClass(className) {
        /// <param name="className" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "className", type: String}
        ]);
        if (e) throw e;

        Sys.UI.DomElement.removeCssClass(this._element, className);
    }
    function Sys$UI$Control$toggleCssClass(className) {
        /// <param name="className" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "className", type: String}
        ]);
        if (e) throw e;

        Sys.UI.DomElement.toggleCssClass(this._element, className);
    }
Sys.UI.Control.prototype = {
    _parent: null,
    _visibilityMode: Sys.UI.VisibilityMode.hide,

    get_element: Sys$UI$Control$get_element,
    get_id: Sys$UI$Control$get_id,
    set_id: Sys$UI$Control$set_id,
    get_parent: Sys$UI$Control$get_parent,
    set_parent: Sys$UI$Control$set_parent,
    get_visibilityMode: Sys$UI$Control$get_visibilityMode,
    set_visibilityMode: Sys$UI$Control$set_visibilityMode,
    get_visible: Sys$UI$Control$get_visible,
    set_visible: Sys$UI$Control$set_visible,
    addCssClass: Sys$UI$Control$addCssClass,
    dispose: Sys$UI$Control$dispose,
    initialize: Sys$UI$Control$initialize,
    onBubbleEvent: Sys$UI$Control$onBubbleEvent,
    raiseBubbleEvent: Sys$UI$Control$raiseBubbleEvent,
    removeCssClass: Sys$UI$Control$removeCssClass,
    toggleCssClass: Sys$UI$Control$toggleCssClass
}
Sys.UI.Control.registerClass('Sys.UI.Control', Sys.Component);

Type.registerNamespace('Sys');
Sys.Res={
"argumentTypeName":"Value is not the name of an existing type.",
"methodRegisteredTwice":"Method {0} has already been registered.",
"cantSetIdAfterInit":"The id property can\u0027t be set on this object after initialization.",
"componentCantSetIdAfterAddedToApp":"The id property of a component can\u0027t be set after it\u0027s been added to the Application object.",
"behaviorDuplicateName":"A behavior with name \u0027{0}\u0027 already exists or it is the name of an existing property on the target element.",
"notATypeName":"Value is not a valid type name.",
"typeShouldBeTypeOrString":"Value is not a valid type or a valid type name.",
"boolTrueOrFalse":"Value must be \u0027true\u0027 or \u0027false\u0027.",
"stringFormatInvalid":"The format string is invalid.",
"referenceNotFound":"Component \u0027{0}\u0027 was not found.",
"enumReservedName":"\u0027{0}\u0027 is a reserved name that can\u0027t be used as an enum value name.",
"eventHandlerNotFound":"Handler not found.",
"circularParentChain":"The chain of control parents can\u0027t have circular references.",
"undefinedEvent":"\u0027{0}\u0027 is not an event.",
"notAMethod":"{0} is not a method.",
"propertyUndefined":"\u0027{0}\u0027 is not a property or an existing field.",
"eventHandlerInvalid":"Handler was not added through the Sys.UI.DomEvent.addHandler method.",
"scriptLoadFailedDebug":"The script \u0027{0}\u0027 failed to load. Check for:\r\n Inaccessible path.\r\n Script errors. (IE) Enable \u0027Display a notification about every script error\u0027 under advanced settings.\r\n Missing call to Sys.Application.notifyScriptLoaded().",
"propertyNotWritable":"\u0027{0}\u0027 is not a writable property.",
"enumInvalidValueName":"\u0027{0}\u0027 is not a valid name for an enum value.",
"controlAlreadyDefined":"A control is already associated with the element.",
"namespaceContainsObject":"Object {0} already exists and is not a namespace.",
"cantAddNonFunctionhandler":"Can\u0027t add a handler that is not a function.",
"scriptLoaderAlreadyLoading":"ScriptLoader.loadScripts cannot be called while the ScriptLoader is already loading scripts.",
"invalidNameSpace":"Value is not a valid namespace identifier.",
"notAnInterface":"Value is not a valid interface.",
"eventHandlerNotFunction":"Handler must be a function.",
"propertyNotAnArray":"\u0027{0}\u0027 is not an Array property.",
"typeRegisteredTwice":"Type {0} has already been registered.",
"cantSetNameAfterInit":"The name property can\u0027t be set on this object after initialization.",
"appDuplicateComponent":"Two components with the same id \u0027{0}\u0027 can\u0027t be added to the application.",
"appComponentMustBeInitialized":"Components must be initialized before they are added to the Application object.",
"baseNotAClass":"Value is not a class.",
"methodNotFound":"No method found with name \u0027{0}\u0027.",
"arrayParseBadFormat":"Value must be a valid string representation for an array. It must start with a \u0027[\u0027 and end with a \u0027]\u0027.",
"cantSetId":"The id property can\u0027t be set on this object.",
"stringFormatBraceMismatch":"The format string contains an unmatched opening or closing brace.",
"enumValueNotInteger":"An enumeration definition can only contain integer values.",
"propertyNullOrUndefined":"Cannot set the properties of \u0027{0}\u0027 because it returned a null value.",
"componentCantSetIdTwice":"The id property of a component can\u0027t be set more than once.",
"createComponentOnDom":"Value must be null for Components that are not Controls or Behaviors.",
"createNotComponent":"{0} does not derive from Sys.Component.",
"createNoDom":"Value must not be null for Controls and Behaviors.",
"cantAddWithoutId":"Can\u0027t add a component that doesn\u0027t have an id.",
"badTypeName":"Value is not the name of the type being registered or the name is a reserved word.",
"argumentInteger":"Value must be an integer.",
"scriptLoadMultipleCallbacks":"The script \u0027{0}\u0027 contains multiple calls to Sys.Application.notifyScriptLoaded(). Only one is allowed.",
"invokeCalledTwice":"Cannot call invoke more than once.",
"webServiceFailed":"The server method \u0027{0}\u0027 failed with the following error: {1}",
"argumentType":"Object cannot be converted to the required type.",
"argumentNull":"Value cannot be null.",
"controlCantSetId":"The id property can\u0027t be set on a control.",
"formatBadFormatSpecifier":"Format specifier was invalid.",
"webServiceFailedNoMsg":"The server method \u0027{0}\u0027 failed.",
"argumentDomElement":"Value must be a DOM element.",
"invalidExecutorType":"Could not create a valid Sys.Net.WebRequestExecutor from: {0}.",
"cannotCallBeforeResponse":"Cannot call {0} when responseAvailable is false.",
"actualValue":"Actual value was {0}.",
"enumInvalidValue":"\u0027{0}\u0027 is not a valid value for enum {1}.",
"scriptLoadFailed":"The script \u0027{0}\u0027 could not be loaded.",
"parameterCount":"Parameter count mismatch.",
"cannotDeserializeEmptyString":"Cannot deserialize empty string.",
"formatInvalidString":"Input string was not in a correct format.",
"invalidTimeout":"Value must be greater than or equal to zero.",
"cannotAbortBeforeStart":"Cannot abort when executor has not started.",
"argument":"Value does not fall within the expected range.",
"cannotDeserializeInvalidJson":"Cannot deserialize. The data does not correspond to valid JSON.",
"invalidHttpVerb":"httpVerb cannot be set to an empty or null string.",
"nullWebRequest":"Cannot call executeRequest with a null webRequest.",
"eventHandlerInvalid":"Handler was not added through the Sys.UI.DomEvent.addHandler method.",
"cannotSerializeNonFiniteNumbers":"Cannot serialize non finite numbers.",
"argumentUndefined":"Value cannot be undefined.",
"webServiceInvalidReturnType":"The server method \u0027{0}\u0027 returned an invalid type. Expected type: {1}",
"servicePathNotSet":"The path to the web service has not been set.",
"argumentTypeWithTypes":"Object of type \u0027{0}\u0027 cannot be converted to type \u0027{1}\u0027.",
"cannotCallOnceStarted":"Cannot call {0} once started.",
"badBaseUrl1":"Base URL does not contain ://.",
"badBaseUrl2":"Base URL does not contain another /.",
"badBaseUrl3":"Cannot find last / in base URL.",
"setExecutorAfterActive":"Cannot set executor after it has become active.",
"paramName":"Parameter name: {0}",
"cannotCallOutsideHandler":"Cannot call {0} outside of a completed event handler.",
"format":"One of the identified items was in an invalid format.",
"assertFailedCaller":"Assertion Failed: {0}\r\nat {1}",
"argumentOutOfRange":"Specified argument was out of the range of valid values.",
"webServiceTimedOut":"The server method \u0027{0}\u0027 timed out.",
"notImplemented":"The method or operation is not implemented.",
"assertFailed":"Assertion Failed: {0}",
"invalidOperation":"Operation is not valid due to the current state of the object.",
"breakIntoDebugger":"{0}\r\n\r\nBreak into debugger?"
};

if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();