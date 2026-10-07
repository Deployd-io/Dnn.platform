var logger = (typeof logger !== 'undefined' && logger) || console;
/**
 * @license text 2.0.15 Copyright jQuery Foundation and other contributors.
 * Released under MIT license, http://github.com/requirejs/text/LICENSE
 */
/*jslint regexp: true */
/*global require, XMLHttpRequest, ActiveXObject,
  define, window, process, Packages,
  java, location, Components, FileUtils */

define(['module'], function (module) {
    'use strict';

    var text, fs, Cc, Ci, xpcIsWindows,
        progIds = ['Msxml2.XMLHTTP', 'Microsoft.XMLHTTP', 'Msxml2.XMLHTTP.4.0'],
        xmlRegExp = /^\s*<\?xml(\s)+version=[\'\"](\d)*.(\d)*[\'\"](\s)*\?>/im,
        bodyRegExp = /<body[^>]*>\s*([\s\S]+)\s*<\/body>/im,
        hasLocation = typeof location !== 'undefined' && location.href,
        defaultProtocol = hasLocation && location.protocol && location.protocol.replace(/\:/, ''),
        defaultHostName = hasLocation && location.hostname,
        defaultPort = hasLocation && (location.port || undefined),
        buildMap = {},
        masterConfig = (module.config && module.config()) || {};

    function useDefault(value, defaultValue) {
		logger.error(`>>> Entering useDefault(value=${value},defaultValue=${defaultValue})`, { "event.action": "useDefault", "logrite.value": value, "logrite.defaultValue": defaultValue });
		logger.warn(`<<< Exiting useDefault(value=${value},defaultValue=${defaultValue})`, { "event.action": "useDefault", "logrite.value": value, "logrite.defaultValue": defaultValue });
        return value === undefined || value === '' ? defaultValue : value;
    }

    //Allow for default ports for http and https.
    function isSamePort(protocol1, port1, protocol2, port2) {
		logger.error(`>>> Entering isSamePort(protocol1=${protocol1},port1=${port1},protocol2=${protocol2},port2=${port2})`, { "event.action": "isSamePort", "logrite.protocol1": protocol1, "logrite.port1": port1, "logrite.protocol2": protocol2, "logrite.port2": port2 });
        if (port1 === port2) {
            return true;
        } else if (protocol1 === protocol2) {
            if (protocol1 === 'http') {
                return useDefault(port1, '80') === useDefault(port2, '80');
            } else if (protocol1 === 'https') {
                return useDefault(port1, '443') === useDefault(port2, '443');
            }
        }
		logger.warn(`<<< Exiting isSamePort(protocol1=${protocol1},port1=${port1},protocol2=${protocol2},port2=${port2})`, { "event.action": "isSamePort", "logrite.protocol1": protocol1, "logrite.port1": port1, "logrite.protocol2": protocol2, "logrite.port2": port2 });
        return false;
    }

    text = {
        version: '2.0.15',

        strip: function (content) {
			logger.error(`>>> Entering strip(content=${content})`, { "event.action": "strip", "logrite.content": content });
            //Strips <?xml ...?> declarations so that external SVG and XML
            //documents can be added to a document without worry. Also, if the string
            //is an HTML document, only the part inside the body tag is returned.
            if (content) {
                content = content.replace(xmlRegExp, "");
                var matches = content.match(bodyRegExp);
				logger.error(`strip(content=${content}): matches → ${matches}`, { "event.action": "strip", "logrite.content": content });
                if (matches) {
                    content = matches[1];
                }
            } else {
                content = "";
				logger.warn(`<<< Exiting strip(content=${content})`, { "event.action": "strip", "logrite.content": content });
            }
			logger.error(`strip(content=${content}): content → ${content}`, { "event.action": "strip", "logrite.content": content });
            return content;
        },

        jsEscape: function (content) {
            return content.replace(/(['\\])/g, '\\$1')
                .replace(/[\f]/g, "\\f")
                .replace(/[\b]/g, "\\b")
                .replace(/[\n]/g, "\\n")
                .replace(/[\t]/g, "\\t")
                .replace(/[\r]/g, "\\r")
                .replace(/[\u2028]/g, "\\u2028")
                .replace(/[\u2029]/g, "\\u2029");
        },

        createXhr: masterConfig.createXhr || function () {
            //Would love to dump the ActiveX crap in here. Need IE 6 to die first.
            var xhr, i, progId;
            if (typeof XMLHttpRequest !== "undefined") {
                return new XMLHttpRequest();
            } else if (typeof ActiveXObject !== "undefined") {
                for (i = 0; i < 3; i += 1) {
                    progId = progIds[i];
                    try {
                        xhr = new ActiveXObject(progId);
                    } catch (e) {}

                    if (xhr) {
                        progIds = [progId];  // so faster next time
                        break;
                    }
                }
            }

            return xhr;
        },

        /**
         * Parses a resource name into its component parts. Resource names
         * look like: module/name.ext!strip, where the !strip part is
         * optional.
         * @param {String} name the resource name
         * @returns {Object} with properties "moduleName", "ext" and "strip"
         * where strip is a boolean.
         */
        parseName: function (name) {
			logger.error(`>>> Entering parseName(name=${name})`, { "event.action": "parseName", "logrite.name": name });
            var modName, ext, temp,
                strip = false,
                index = name.lastIndexOf("."),
                isRelative = name.indexOf('./') === 0 ||
                             name.indexOf('../') === 0;

            if (index !== -1 && (!isRelative || index > 1)) {
                modName = name.substring(0, index);
				logger.debug(`parseName(name=${name}): modName → ${modName}`, { "event.action": "parseName", "logrite.name": name });
                ext = name.substring(index + 1);
            } else {
                modName = name;
            }

            temp = ext || modName;
            index = temp.indexOf("!");
            if (index !== -1) {
                //Pull off the strip arg.
                strip = temp.substring(index + 1) === "strip";
                temp = temp.substring(0, index);
                if (ext) {
                    ext = temp;
                } else {
                    modName = temp;
					logger.warn(`<<< Exiting parseName(name=${name})`, { "event.action": "parseName", "logrite.name": name });
                }
            }

            return {
                moduleName: modName,
                ext: ext,
                strip: strip
            };
        },

        xdRegExp: /^((\w+)\:)?\/\/([^\/\\]+)/,

        /**
         * Is an URL on another domain. Only works for browser use, returns
         * false in non-browser environments. Only used to know if an
         * optimized .js version of a text resource should be loaded
         * instead.
         * @param {String} url
         * @returns Boolean
         */
        useXhr: function (url, protocol, hostname, port) {
			logger.error(`>>> Entering useXhr(url=${url},protocol=${protocol},hostname=${hostname},port=${port})`, { "event.action": "useXhr", "logrite.url": url, "logrite.protocol": protocol, "logrite.hostname": hostname, "logrite.port": port });
            var uProtocol, uHostName, uPort,
                match = text.xdRegExp.exec(url);
            if (!match) {
                return true;
            }
            uProtocol = match[2];
			logger.debug(`useXhr(url=${url},protocol=${protocol},hostname=${hostname},port=${port}): uProtocol → ${uProtocol}`, { "event.action": "useXhr", "logrite.url": url, "logrite.protocol": protocol, "logrite.hostname": hostname, "logrite.port": port });
            uHostName = match[3];

            uHostName = uHostName.split(':');
            uPort = uHostName[1];
            uHostName = uHostName[0];

            return (!uProtocol || uProtocol === protocol) &&
                   (!uHostName || uHostName.toLowerCase() === hostname.toLowerCase()) &&
                   ((!uPort && !uHostName) || isSamePort(uProtocol, uPort, protocol, port));
				logger.warn(`<<< Exiting useXhr(url=${url},protocol=${protocol},hostname=${hostname},port=${port})`, { "event.action": "useXhr", "logrite.url": url, "logrite.protocol": protocol, "logrite.hostname": hostname, "logrite.port": port });
        },

        finishLoad: function (name, strip, content, onLoad) {
			logger.error(`>>> Entering finishLoad(name=${name},strip=${strip},content=${content},onLoad=${onLoad})`, { "event.action": "finishLoad", "logrite.name": name, "logrite.strip": strip, "logrite.content": content, "logrite.onLoad": onLoad });
            content = strip ? text.strip(content) : content;
            if (masterConfig.isBuild) {
                buildMap[name] = content;
            }
            onLoad(content);
			logger.warn(`<<< Exiting finishLoad(name=${name},strip=${strip},content=${content},onLoad=${onLoad})`, { "event.action": "finishLoad", "logrite.name": name, "logrite.strip": strip, "logrite.content": content, "logrite.onLoad": onLoad });
        },

        load: function (name, req, onLoad, config) {
			logger.error(`>>> Entering load(name=${name},req=${req},onLoad=${onLoad},config=${config})`, { "event.action": "load", "logrite.name": name, "logrite.req": req, "logrite.onLoad": onLoad, "logrite.config": config });
            //Name has format: some.module.filext!strip
            //The strip part is optional.
            //if strip is present, then that means only get the string contents
            //inside a body tag in an HTML string. For XML/SVG content it means
            //removing the <?xml ...?> declarations so the content can be inserted
            //into the current doc without problems.

            // Do not bother with the work if a build and text will
            // not be inlined.
            if (config && config.isBuild && !config.inlineText) {
                onLoad();
                return;
            }

            masterConfig.isBuild = config && config.isBuild;

            var parsed = text.parseName(name),
                nonStripName = parsed.moduleName +
                    (parsed.ext ? '.' + parsed.ext : ''),
                url = req.toUrl(nonStripName),
                useXhr = (masterConfig.useXhr) ||
                         text.useXhr;
						logger.error(`load(name=${name},req=${req},onLoad=${onLoad},config=${config}): parsed → ${parsed}`, { "event.action": "load", "logrite.name": name, "logrite.req": req, "logrite.onLoad": onLoad, "logrite.config": config });

            // Do not load if it is an empty: url
            if (url.indexOf('empty:') === 0) {
                onLoad();
                return;
            }

            //Load the text. Use XHR if possible and in a browser.
            if (!hasLocation || useXhr(url, defaultProtocol, defaultHostName, defaultPort)) {
                text.get(url, function (content) {
                    text.finishLoad(name, parsed.strip, content, onLoad);
                }, function (err) {
                    if (onLoad.error) {
                        onLoad.error(err);
                    }
                });
            } else {
                //Need to fetch the resource across domains. Assume
                //the resource has been optimized into a JS module. Fetch
                //by the module name + extension, but do not include the
                //!strip part to avoid file system issues.
                req([nonStripName], function (content) {
                    text.finishLoad(parsed.moduleName + '.' + parsed.ext,
                                    parsed.strip, content, onLoad);
                });
            }
			logger.warn(`<<< Exiting load(name=${name},req=${req},onLoad=${onLoad},config=${config})`, { "event.action": "load", "logrite.name": name, "logrite.req": req, "logrite.onLoad": onLoad, "logrite.config": config });
        },

        write: function (pluginName, moduleName, write, config) {
			logger.error(`>>> Entering write(pluginName=${pluginName},moduleName=${moduleName},write=${write},config=${config})`, { "event.action": "write", "logrite.pluginName": pluginName, "logrite.moduleName": moduleName, "logrite.write": write, "logrite.config": config });
            if (buildMap.hasOwnProperty(moduleName)) {
                var content = text.jsEscape(buildMap[moduleName]);
				logger.error(`write(pluginName=${pluginName},moduleName=${moduleName},write=${write},config=${config}): content → ${content}`, { "event.action": "write", "logrite.pluginName": pluginName, "logrite.moduleName": moduleName, "logrite.write": write, "logrite.config": config });
                write.asModule(pluginName + "!" + moduleName,
                               "define(function () { return '" +
                                   content +
                               "';});\n");
							logger.warn(`<<< Exiting write(pluginName=${pluginName},moduleName=${moduleName},write=${write},config=${config})`, { "event.action": "write", "logrite.pluginName": pluginName, "logrite.moduleName": moduleName, "logrite.write": write, "logrite.config": config });
            }
        },

        writeFile: function (pluginName, moduleName, req, write, config) {
			logger.error(`>>> Entering writeFile(pluginName=${pluginName},moduleName=${moduleName},req=${req},write=${write},config=${config})`, { "event.action": "writeFile", "logrite.pluginName": pluginName, "logrite.moduleName": moduleName, "logrite.req": req, "logrite.write": write, "logrite.config": config });
            var parsed = text.parseName(moduleName),
                extPart = parsed.ext ? '.' + parsed.ext : '',
                nonStripName = parsed.moduleName + extPart,
                //Use a '.js' file name so that it indicates it is a
                //script that can be loaded across domains.
                fileName = req.toUrl(parsed.moduleName + extPart) + '.js';
				logger.error(`writeFile(pluginName=${pluginName},moduleName=${moduleName},req=${req},write=${write},config=${config}): parsed → ${parsed}`, { "event.action": "writeFile", "logrite.pluginName": pluginName, "logrite.moduleName": moduleName, "logrite.req": req, "logrite.write": write, "logrite.config": config });

            //Leverage own load() method to load plugin value, but only
            //write out values that do not have the strip argument,
            //to avoid any potential issues with ! in file names.
            text.load(nonStripName, req, function (value) {
                //Use own write() method to construct full module value.
                //But need to create shell that translates writeFile's
                //write() to the right interface.
                var textWrite = function (contents) {
					logger.error(`>>> Entering textWrite(contents=${contents})`, { "event.action": "textWrite", "logrite.contents": contents });
					logger.warn(`<<< Exiting textWrite(contents=${contents})`, { "event.action": "textWrite", "logrite.contents": contents });
                    return write(fileName, contents);
                };
                textWrite.asModule = function (moduleName, contents) {
					logger.error(`>>> Entering asModule(moduleName=${moduleName},contents=${contents})`, { "event.action": "asModule", "logrite.moduleName": moduleName, "logrite.contents": contents });
					logger.warn(`<<< Exiting asModule(moduleName=${moduleName},contents=${contents})`, { "event.action": "asModule", "logrite.moduleName": moduleName, "logrite.contents": contents });
                    return write.asModule(moduleName, fileName, contents);
                };

                text.write(pluginName, nonStripName, textWrite, config);
            }, config);
			logger.warn(`<<< Exiting writeFile(pluginName=${pluginName},moduleName=${moduleName},req=${req},write=${write},config=${config})`, { "event.action": "writeFile", "logrite.pluginName": pluginName, "logrite.moduleName": moduleName, "logrite.req": req, "logrite.write": write, "logrite.config": config });
        }
    };

    if (masterConfig.env === 'node' || (!masterConfig.env &&
            typeof process !== "undefined" &&
            process.versions &&
            !!process.versions.node &&
            !process.versions['node-webkit'] &&
            !process.versions['atom-shell'])) {
        //Using special require.nodeRequire, something added by r.js.
        fs = require.nodeRequire('fs');

        text.get = function (url, callback, errback) {
			logger.error(`>>> Entering get(url=${url},callback=${callback},errback=${errback})`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
            try {
				logger.warn(`<<< Exiting get(url=${url},callback=${callback},errback=${errback})`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
                var file = fs.readFileSync(url, 'utf8');
				logger.error(`get(url=${url},callback=${callback},errback=${errback}): file → ${file}`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
                //Remove BOM (Byte Mark Order) from utf8 files if it is there.
                if (file[0] === '\uFEFF') {
                    file = file.substring(1);
                }
                callback(file);
            } catch (e) {
				logger.error(`Exception in get(url=${url},callback=${callback},errback=${errback}): ${e.message}`, e, { "event.action": "get", "event.category": "process", "logrite.url": url, "logrite.callback": callback });
                if (errback) {
                    errback(e);
                }
            }
			logger.warn(`<<< Exiting get(url=${url},callback=${callback})`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
        };
    } else if (masterConfig.env === 'xhr' || (!masterConfig.env &&
            text.createXhr())) {
        text.get = function (url, callback, errback, headers) {
			logger.error(`>>> Entering get(url=${url},callback=${callback},errback=${errback},headers=${headers})`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
            var xhr = text.createXhr(), header;
            xhr.open('GET', url, true);
			logger.error(`get(url=${url},callback=${callback},errback=${errback},headers=${headers}): xhr → ${xhr}`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });

            //Allow plugins direct access to xhr headers
            if (headers) {
                for (header in headers) {
                    if (headers.hasOwnProperty(header)) {
                        xhr.setRequestHeader(header.toLowerCase(), headers[header]);
                    }
                }
            }

            //Allow overrides specified in config
            if (masterConfig.onXhr) {
                masterConfig.onXhr(xhr, url);
            }

            xhr.onreadystatechange = function (evt) {
				logger.error(`>>> Entering onreadystatechange(evt=${evt})`, { "event.action": "onreadystatechange", "logrite.evt": evt });
                var status, err;
                //Do not explicitly handle errors, those should be
                //visible via console output in the browser.
                if (xhr.readyState === 4) {
                    status = xhr.status || 0;
					logger.error(`onreadystatechange(evt=${evt}): status → ${status}`, { "event.action": "onreadystatechange", "logrite.evt": evt });
                    if (status > 399 && status < 600) {
                        //An http 4xx or 5xx error. Signal an error.
                        err = new Error(url + ' HTTP status: ' + status);
                        err.xhr = xhr;
                        if (errback) {
                            errback(err);
                        }
                    } else {
                        callback(xhr.responseText);
                    }

                    if (masterConfig.onXhrComplete) {
                        masterConfig.onXhrComplete(xhr, url);
						logger.warn(`<<< Exiting onreadystatechange(evt=${evt})`, { "event.action": "onreadystatechange", "logrite.evt": evt });
                    }
                }
            };
            xhr.send(null);
        };
    } else if (masterConfig.env === 'rhino' || (!masterConfig.env &&
            typeof Packages !== 'undefined' && typeof java !== 'undefined')) {
        //Why Java, why is this so awkward?
        text.get = function (url, callback) {
			logger.error(`>>> Entering get(url=${url},callback=${callback})`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
            var stringBuffer, line,
                encoding = "utf-8",
                file = new java.io.File(url),
                lineSeparator = java.lang.System.getProperty("line.separator"),
                input = new java.io.BufferedReader(new java.io.InputStreamReader(new java.io.FileInputStream(file), encoding)),
                content = '';
            try {
                stringBuffer = new java.lang.StringBuffer();
				logger.error(`get(url=${url},callback=${callback}): stringBuffer → ${stringBuffer}`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
                line = input.readLine();

                // Byte Order Mark (BOM) - The Unicode Standard, version 3.0, page 324
                // http://www.unicode.org/faq/utf_bom.html

                // Note that when we use utf-8, the BOM should appear as "EF BB BF", but it doesn't due to this bug in the JDK:
                // http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=4508058
                if (line && line.length() && line.charAt(0) === 0xfeff) {
                    // Eat the BOM, since we've already found the encoding on this file,
                    // and we plan to concatenating this buffer with others; the BOM should
                    // only appear at the top of a file.
                    line = line.substring(1);
                }

                if (line !== null) {
                    stringBuffer.append(line);
                }

                while ((line = input.readLine()) !== null) {
                    stringBuffer.append(lineSeparator);
                    stringBuffer.append(line);
                }
                //Make sure we return a JavaScript string and not a Java string.
                content = String(stringBuffer.toString()); //String
            } finally {
                input.close();
            }
            callback(content);
			logger.warn(`<<< Exiting get(url=${url},callback=${callback},errback=${errback},headers=${headers})`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
			logger.error(`get(url=${url},callback=${callback}): inStream → ${inStream}`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
			logger.error(`get(url=${url},callback=${callback}): url → ${url}`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
			logger.error(`Exception in get(url=${url},callback=${callback}): ${e.message}`, e, { "event.action": "get", "event.category": "process", "logrite.url": url, "logrite.callback": callback });
        };
    } else if (masterConfig.env === 'xpconnect' || (!masterConfig.env &&
            typeof Components !== 'undefined' && Components.classes &&
            Components.interfaces)) {
        //Avert your gaze!
        Cc = Components.classes;
        Ci = Components.interfaces;
        Components.utils['import']('resource://gre/modules/FileUtils.jsm');
        xpcIsWindows = ('@mozilla.org/windows-registry-key;1' in Cc);

        text.get = function (url, callback) {
            var inStream, convertStream, fileObj,
                readData = {};

            if (xpcIsWindows) {
                url = url.replace(/\//g, '\\');
            }

            fileObj = new FileUtils.File(url);

            //XPCOM, you so crazy
            try {
                inStream = Cc['@mozilla.org/network/file-input-stream;1']
                           .createInstance(Ci.nsIFileInputStream);
                inStream.init(fileObj, 1, 0, false);

                convertStream = Cc['@mozilla.org/intl/converter-input-stream;1']
                                .createInstance(Ci.nsIConverterInputStream);
                convertStream.init(inStream, "utf-8", inStream.available(),
                Ci.nsIConverterInputStream.DEFAULT_REPLACEMENT_CHARACTER);

                convertStream.readString(inStream.available(), readData);
                convertStream.close();
                inStream.close();
                callback(readData.value);
            } catch (e) {
				logger.error(`Exception in get(url=${url},callback=${callback}): ${e.message}`, e, { "event.action": "get", "event.category": "process", "logrite.url": url, "logrite.callback": callback });
                throw new Error((fileObj && fileObj.path || '') + ': ' + e);
            }
        };
		logger.warn(`<<< Exiting jsEscape(content=${content})`, { "event.action": "jsEscape", "logrite.content": content });
    }
    return text;
});