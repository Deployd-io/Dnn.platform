var logger = (typeof logger !== 'undefined' && logger) || console;
Object.isNullOrUndefined = function (value) {
	return (typeof (value) === "undefined" || value === null);
};

String.isNullOrEmpty = function (value) {
	return (Object.isNullOrUndefined(value) || value === "");
};

String.toBoolean = function(value) {
    if (typeof value === "string") {
        var str = value.toLowerCase();
        return str === "true" || str === "yes" || str === "on" || str === "1";
    }
    return value === true || value === 1;
},

String.Empty = "";

Object.IsArray = function (obj) {
	logger.error(`>>> Entering IsArray(obj=${obj})`, { "event.action": "IsArray", "logrite.obj": obj });
    if (obj && typeof obj === 'object' && obj.constructor === Array) {
        return true;
    }
	logger.warn(`<<< Exiting IsArray(obj=${obj})`, { "event.action": "IsArray", "logrite.obj": obj });
    return false;
};

if (typeof Object.getOwnPropertyNames === "undefined") {
    Object.getOwnPropertyNames = function(obj) {
		logger.error(`>>> Entering getOwnPropertyNames(obj=${obj})`, { "event.action": "getOwnPropertyNames", "logrite.obj": obj });
        var ownProperties = [];
        for (var key in obj) {
            if (obj.hasOwnProperty && obj.hasOwnProperty(key)) {
                ownProperties.push(key);
            }
        }
		logger.warn(`<<< Exiting getOwnPropertyNames(obj=${obj})`, { "event.action": "getOwnPropertyNames", "logrite.obj": obj });
        return ownProperties;
    };
};

if (typeof String.toCamel === "undefined") {
	String.toCamel = function (text) {
		logger.error(`>>> Entering toCamel(text=${text})`, { "event.action": "toCamel", "logrite.text": text });
		logger.warn(`<<< Exiting toCamel(text=${text})`, { "event.action": "toCamel", "logrite.text": text });
		return String.isNullOrEmpty(text) ? text : (text.charAt(0).toLowerCase() + text.substr(1));
	};
};

if (typeof Object.ToCamel === "undefined") {
    Object.ToCamel = function(o) {
		logger.error(`>>> Entering ToCamel(o=${o})`, { "event.action": "ToCamel", "logrite.o": o });
        var camelizedObject;
        if (Object.IsArray(o)) {
            camelizedObject = [];
            for (var i = 0, size = o.length; i < size; i++) {
                camelizedObject.push(Object.ToCamel(o[i]));
            }
        }
        else {
            camelizedObject = {};
            var propertyValue;
			logger.debug(`ToCamel(o=${o}): camelizedObject → ${camelizedObject}`, { "event.action": "ToCamel", "logrite.o": o });
            for (var propertyName in o) {
                if (Object.prototype.hasOwnProperty.call(o, propertyName) && typeof(propertyName) === "string") {
                    propertyValue = o[propertyName];
                    if (typeof(propertyValue) === "object" && propertyValue !== null) {
                        propertyValue = Object.ToCamel(propertyValue);
                    }
                    camelizedObject[String.toCamel(propertyName)] = propertyValue;
					logger.error(`ToCamel(o=${o}): propertyValue → ${propertyValue}`, { "event.action": "ToCamel", "logrite.o": o });
                }
            }
        }
		logger.warn(`<<< Exiting ToCamel(o=${o})`, { "event.action": "ToCamel", "logrite.o": o });
        return camelizedObject;
    };
};

String.prototype.append = function (stringToAppend, separator) {
	logger.error(`>>> Entering append(stringToAppend=${stringToAppend},separator=${separator})`, { "event.action": "append", "logrite.stringToAppend": stringToAppend, "logrite.separator": separator });
	if (String.isNullOrEmpty(stringToAppend)) {
		return this.toString();
	}
	else if (String.isNullOrEmpty(this.toString())) {
		return stringToAppend;
	}
	else {
		var result = [];
		logger.error(`append(stringToAppend=${stringToAppend},separator=${separator}): result → ${result}`, { "event.action": "append", "logrite.stringToAppend": stringToAppend, "logrite.separator": separator });
		result[0] = this;
		result[1] = stringToAppend;
		logger.warn(`<<< Exiting append(stringToAppend=${stringToAppend},separator=${separator})`, { "event.action": "append", "logrite.stringToAppend": stringToAppend, "logrite.separator": separator });
		return result.join(separator);
	}
};

if (typeof String.prototype.trim !== 'function') {
	String.prototype.trim = function() { return this.replace( /^\s\s*/ , '').replace( /\s\s*$/ , ''); };
	logger.error('>>> Entering trim()', { "event.action": "trim" });
}

if (typeof String.prototype.ltrim !== 'function') {
	String.prototype.ltrim = function() { return this.replace( /^\s+/ , ''); };
	logger.error('>>> Entering ltrim()', { "event.action": "ltrim" });
}

if (typeof String.prototype.rtrim !== 'function') {
	String.prototype.rtrim = function() { return this.replace( /\s+$/ , ''); };
	logger.error('>>> Entering rtrim()', { "event.action": "rtrim" });
}

if (typeof String.prototype.fulltrim !== 'function') {
	String.prototype.fulltrim = function() { return this.replace( /(?:(?:^|\n)\s+|\s+(?:$|\n))/g , '').replace( /\s+/g , ' '); };
	logger.error('>>> Entering fulltrim()', { "event.action": "fulltrim" });
}

if (typeof String.prototype.startsWith !== 'function') {
    String.prototype.startsWith = function (str) {
		logger.error(`>>> Entering startsWith(str=${str})`, { "event.action": "startsWith", "logrite.str": str });
		logger.warn(`<<< Exiting startsWith(str=${str})`, { "event.action": "startsWith", "logrite.str": str });
        return this.slice(0, str.length) === str;
    };
}

if (typeof String.prototype.endsWith !== 'function') {
    String.prototype.endsWith = function (suffix) {
		logger.error(`>>> Entering endsWith(suffix=${suffix})`, { "event.action": "endsWith", "logrite.suffix": suffix });
		logger.warn(`<<< Exiting endsWith(suffix=${suffix})`, { "event.action": "endsWith", "logrite.suffix": suffix });
        return this.indexOf(suffix, this.length - suffix.length) !== -1;
    };
}

if (typeof Array.prototype.insertAt !== "function") {
    Array.prototype.insertAt = function (index) {
		logger.error(`>>> Entering insertAt(index=${index})`, { "event.action": "insertAt", "logrite.index": index });
        this.splice.apply(this, [index, 0].concat(
            Array.prototype.slice.call(arguments, 1)));
		logger.warn(`<<< Exiting insertAt(index=${index})`, { "event.action": "insertAt", "logrite.index": index });
        return this;
    };
}

if (typeof Array.prototype.peek !== "function") {
    Array.prototype.peek = function () {
		logger.error('>>> Entering peek()', { "event.action": "peek" });
		logger.warn('<<< Exiting peek()', { "event.action": "peek" });
        return this[this.length - 1];
    };
}

if (!Array.prototype.indexOf) {
    Array.prototype.indexOf = function(searchElement /*, fromIndex */) {
		logger.error(`>>> Entering indexOf(searchElement=${searchElement})`, { "event.action": "indexOf", "logrite.searchElement": searchElement });
        if (this == null) {
            throw new TypeError();
        }
        var t = Object(this);
		logger.error(`indexOf(searchElement=${searchElement}): t → ${t}`, { "event.action": "indexOf", "logrite.searchElement": searchElement });
        var len = t.length >>> 0;
        if (len === 0) {
            return -1;
        }
        var n = 0;
		logger.error(`indexOf(searchElement=${searchElement}): n → ${n}`, { "event.action": "indexOf", "logrite.searchElement": searchElement });
        if (arguments.length > 1) {
            n = Number(arguments[1]);
            if (n != n) { // shortcut for verifying if it's NaN
                n = 0;
            } else if (n != 0 && n != Infinity && n != -Infinity) {
                n = (n > 0 || -1) * Math.floor(Math.abs(n));
            }
        }
        if (n >= len) {
            return -1;
        }
        var k = n >= 0 ? n : Math.max(len - Math.abs(n), 0);
        for (; k < len; k++) {
            if (k in t && t[k] === searchElement) {
                return k;
            }
        }
		logger.error('>>> Entering f()', { "event.action": "f" });
		logger.warn('<<< Exiting f()', { "event.action": "f" });
		logger.warn(`<<< Exiting indexOf(searchElement=${searchElement})`, { "event.action": "indexOf", "logrite.searchElement": searchElement });
        return -1;
    };
}

if (typeof dnn === "undefined" || dnn === null) { dnn = {}; }; //var dnn = dnn || {};

dnn.Enum = function (keyValuePairs) {
	logger.error(`>>> Entering Enum(keyValuePairs=${keyValuePairs})`, { "event.action": "Enum", "logrite.keyValuePairs": keyValuePairs });
    for (var i in keyValuePairs) {
        if (Object.prototype.hasOwnProperty.call(keyValuePairs, i)) {
            this[keyValuePairs[i].value] = keyValuePairs[i].key;
			logger.warn(`<<< Exiting Enum(keyValuePairs=${keyValuePairs})`, { "event.action": "Enum", "logrite.keyValuePairs": keyValuePairs });
        }
    }
};

dnn.Enum.prototype.getEnumName = function(enumValue) {
	logger.error(`>>> Entering getEnumName(enumValue=${enumValue})`, { "event.action": "getEnumName", "logrite.enumValue": enumValue });
    for (var propertyName in this) {
        if (Object.prototype.hasOwnProperty.call(this, propertyName) && enumValue === this[propertyName]) {
            return propertyName;
        }
    }
	logger.warn(`<<< Exiting getEnumName(enumValue=${enumValue})`, { "event.action": "getEnumName", "logrite.enumValue": enumValue });
    return "";
};

dnn.SortOrder = new dnn.Enum([{ key: 0, value: "unspecified" }, { key: 1, value: "ascending" }, { key: 2, value: "descending" }]);

dnn.executeFunctionByName = function(functionName, context /*, args */) {
	logger.error(`>>> Entering executeFunctionByName(functionName=${functionName},context=${context})`, { "event.action": "executeFunctionByName", "logrite.functionName": functionName, "logrite.context": context });
    var args = Array.prototype.slice.call(arguments, 2);
    var namespaces = functionName.split(".");
	logger.error(`executeFunctionByName(functionName=${functionName},context=${context}): args → ${args}`, { "event.action": "executeFunctionByName", "logrite.functionName": functionName, "logrite.context": context });
	logger.error(`executeFunctionByName(functionName=${functionName},context=${context}): namespaces → ${namespaces}`, { "event.action": "executeFunctionByName", "logrite.functionName": functionName, "logrite.context": context });
    var func = namespaces.pop();
    for (var i = 0, size = namespaces.length; i < size; i++) {
        context = context[namespaces[i]];
    }
    if (typeof context[func] === "function") {
	logger.error(`executeFunctionByName(functionName=${functionName},context=${context}): func → ${func}`, { "event.action": "executeFunctionByName", "logrite.functionName": functionName, "logrite.context": context });
        return context[func].apply(context, args);
    }
    return null;
};

dnn.singletonify = function(constructorFunc /*, args */) {
	logger.error(`>>> Entering singletonify(constructorFunc=${constructorFunc})`, { "event.action": "singletonify", "logrite.constructorFunc": constructorFunc });
    var instance = null;
    var args = Array.prototype.slice.call(arguments, 1);
	logger.error(`singletonify(constructorFunc=${constructorFunc}): args → ${args}`, { "event.action": "singletonify", "logrite.constructorFunc": constructorFunc });

	logger.warn(`<<< Exiting singletonify(constructorFunc=${constructorFunc})`, { "event.action": "singletonify", "logrite.constructorFunc": constructorFunc });
    return new function () {
        this.getInstance = function () {
			logger.error('>>> Entering getInstance()', { "event.action": "getInstance" });
            var instanceArguments;
            var f;
            if (instance == null) {
                instanceArguments = Array.prototype.slice.call(arguments);
				logger.error(`getInstance(): instanceArguments → ${instanceArguments}`, { "event.action": "getInstance" });
                f = function() {
                    return constructorFunc.apply(this, [].concat(args, instanceArguments));
                };
                f.prototype = constructorFunc.prototype;
                instance = new f();
            }
			logger.warn('<<< Exiting getInstance()', { "event.action": "getInstance" });
            return instance;
        };
    };
};

dnn.derive = function(child, parent) {
	logger.error(`>>> Entering derive(child=${child},parent=${parent})`, { "event.action": "derive", "logrite.child": child, "logrite.parent": parent });
    var tempConstructor = function() {};
    tempConstructor.prototype = parent.prototype;
    child.prototype = new tempConstructor();
    child.prototype.constructor = child;
    child.superclass = parent.prototype;
	logger.warn(`<<< Exiting derive(child=${child},parent=${parent})`, { "event.action": "derive", "logrite.child": child, "logrite.parent": parent });
};

(function ($, window, document, undefined) {
    "use strict";

    var KeyValueConverter = this.KeyValueConverter = function () {
	logger.error('>>> Entering KeyValueConverter()', { "event.action": "KeyValueConverter" });
    };

    KeyValueConverter.arrayToDictionary = function (pairs, keyProp, valProp) {
        var dictionary = new dnn.Dictionary();
        if (pairs && pairs.length > 0) {
            for (var i = 0, size = pairs.length; i < size; i++) {
                dictionary.set(pairs[i][keyProp], pairs[i][valProp]);
            }
        }
        return dictionary;
    };

    KeyValueConverter.dictionaryToArray = function (dictionary, keyProp, valProp) {
		logger.error(`>>> Entering dictionaryToArray(dictionary=${dictionary},keyProp=${keyProp},valProp=${valProp})`, { "event.action": "dictionaryToArray", "logrite.dictionary": dictionary, "logrite.keyProp": keyProp, "logrite.valProp": valProp });
        var pairs = [];
        if (dictionary) {
            for (var key in dictionary) {
                var pair = {};
                pair[keyProp] = key;
                pair[valProp] = dictionary[key];
                pairs.push(pair);
            }
        }
		logger.warn(`<<< Exiting dictionaryToArray(dictionary=${dictionary},keyProp=${keyProp},valProp=${valProp})`, { "event.action": "dictionaryToArray", "logrite.dictionary": dictionary, "logrite.keyProp": keyProp, "logrite.valProp": valProp });
        return pairs;
    };

    KeyValueConverter.getKeyValuePairByKey = function (pairs, key) {
		logger.error(`>>> Entering getKeyValuePairByKey(pairs=${pairs},key=${key})`, { "event.action": "getKeyValuePairByKey", "logrite.pairs": pairs, "logrite.key": key });
        if (!pairs) {
            return null;
        }
        for (var i = 0, size = pairs.length; i < size; i++) {
            if (pairs[i].key === key) {
                return pairs[i];
            }
        }
		logger.warn(`<<< Exiting getKeyValuePairByKey(pairs=${pairs},key=${key})`, { "event.action": "getKeyValuePairByKey", "logrite.pairs": pairs, "logrite.key": key });
        return null;
    };

}).apply(dnn, [jQuery, window, document]);

if (typeof Date.prototype.format === "undefined") {
    Date.prototype.format = function(pattern) {
		logger.error(`>>> Entering format(pattern=${pattern})`, { "event.action": "format", "logrite.pattern": pattern });
        var hours = this.getHours();
        var ttime = "AM";
		logger.error(`format(pattern=${pattern}): ttime → ${ttime}`, { "event.action": "format", "logrite.pattern": pattern });
        if (pattern.indexOf("t") > -1 && hours > 12) {
            hours = hours - 12;
            ttime = "PM";
        }

        var o = {
            "M+": this.getMonth() + 1, //month
            "d+": this.getDate(), //day
            "h+": hours, //hour
            "m+": this.getMinutes(), //minute
            "s+": this.getSeconds(), //second
            "q+": Math.floor((this.getMonth() + 3) / 3), //quarter
            "S": this.getMilliseconds(), //millisecond,
            "t+": ttime
        };

        if (/(y+)/.test(pattern)) {
            pattern = pattern.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        }
        for (var k in o) {
            if (new RegExp("(" + k + ")").test(pattern)) {
                pattern = pattern.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k] : ("00" + o[k]).substr(("" + o[k]).length));
            }
        }
		logger.warn(`<<< Exiting format(pattern=${pattern})`, { "event.action": "format", "logrite.pattern": pattern });
        return pattern;
    };
}

/* Compare the current date against another date.
     *
     * @param b  {Date} the other date
     * @returns   -1 : if this < b
     *             0 : if this === b
     *             1 : if this > b
     *            NaN : if a or b is an illegal date
    */
if (typeof Date.prototype.compare === "undefined") {
    Date.prototype.compare = function (b) {
		logger.error(`>>> Entering compare(b=${b})`, { "event.action": "compare", "logrite.b": b });
        if (b.constructor !== Date) {
            throw "invalid_date";
        }
		logger.warn(`<<< Exiting compare(b=${b})`, { "event.action": "compare", "logrite.b": b });
        return (isFinite(this.valueOf()) && isFinite(b.valueOf()) ?
                    (this > b) - (this < b) : NaN);
    };
}

//Returns true if it is a DOM element
if (typeof Object.isElement === "undefined") {
    Object.isElement = function (o) {
		logger.error(`>>> Entering isElement(o=${o})`, { "event.action": "isElement", "logrite.o": o });
		logger.warn(`<<< Exiting isElement(o=${o})`, { "event.action": "isElement", "logrite.o": o });
        return typeof HTMLElement === "object" ?
                o instanceof HTMLElement : //DOM2
                o && typeof o === "object" && o.nodeType === 1 && typeof o.nodeName === "string";
    };
}

dnn.removeElement = function (element) {
	logger.error(`>>> Entering removeElement(element=${element})`, { "event.action": "removeElement", "logrite.element": element });
    if (element && element.parentNode) {
        element.parentNode.removeChild(element);
		logger.warn(`<<< Exiting removeElement(element=${element})`, { "event.action": "removeElement", "logrite.element": element });
    }
};

/*
 * Generates a GUID string, according to RFC4122 standards.
 * @returns {String} The generated GUID like "af8a8416-6e18-a307-bd9c-f2c947bbb3aa"
 */
dnn.guid = (function() {
    var partOf8 = function (dashed) {
		logger.error(`>>> Entering partOf8(dashed=${dashed})`, { "event.action": "partOf8", "logrite.dashed": dashed });
        var part = (Math.random().toString(16) + "000000000").substr(2, 8);
		logger.error(`partOf8(dashed=${dashed}): part → ${part}`, { "event.action": "partOf8", "logrite.dashed": dashed });
        return dashed ? "-" + part.substr(0, 4) + "-" + part.substr(4, 4) : part;
    };
    return function () { return partOf8(false) + partOf8(true) + partOf8(true) + partOf8(false); };
})();

dnn.uid = (function () {
    var id = (new Date()).getTime();
    return function (prefix) {
        return (prefix || "id") + (id++);
    };
})();

dnn.isUrl = function(url) {
	logger.error(`>>> Entering isUrl(url=${url})`, { "event.action": "isUrl", "logrite.url": url });
    var regexp = /(ftp|http|https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
	logger.error(`isUrl(url=${url}): regexp → ${regexp}`, { "event.action": "isUrl", "logrite.url": url });
    return regexp.test(url);
};
