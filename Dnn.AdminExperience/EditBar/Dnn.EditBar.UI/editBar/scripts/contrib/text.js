var logger = (typeof logger !== 'undefined' && logger) || console;
/**
 * @license RequireJS text 1.0.8 Copyright (c) 2010-2011, The Dojo Foundation All Rights Reserved.
 * Available via the MIT or new BSD license.
 * see: http://github.com/jrburke/requirejs for details
 */
/*jslint regexp: true, plusplus: true, sloppy: true */
/*global require: false, XMLHttpRequest: false, ActiveXObject: false,
  define: false, window: false, process: false, Packages: false,
  java: false, location: false */

(function () {
    var progIds = ['Msxml2.XMLHTTP', 'Microsoft.XMLHTTP', 'Msxml2.XMLHTTP.4.0'],
        xmlRegExp = /^\s*<\?xml(\s)+version=[\'\"](\d)*.(\d)*[\'\"](\s)*\?>/im,
        bodyRegExp = /<body[^>]*>\s*([\s\S]+)\s*<\/body>/im,
        hasLocation = typeof location !== 'undefined' && location.href,
        defaultProtocol = hasLocation && location.protocol && location.protocol.replace(/\:/, ''),
        defaultHostName = hasLocation && location.hostname,
        defaultPort = hasLocation && (location.port || undefined),
        buildMap = [];

    define(function () {
        var text, fs;

        text = {
            version: '1.0.8',

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
                    .replace(/[\r]/g, "\\r");
            },

            createXhr: function () {
				logger.error('>>> Entering createXhr()', { "event.action": "createXhr" });
                //Would love to dump the ActiveX crap in here. Need IE 6 to die first.
                var xhr, i, progId;
				const _start_createXhr_52 = Date.now();
                if (typeof XMLHttpRequest !== "undefined") {
                    return new XMLHttpRequest();
                } else if (typeof ActiveXObject !== "undefined") {
                    for (i = 0; i < 3; i++) {
                        progId = progIds[i];
                        try {
                            xhr = new ActiveXObject(progId);
							logger.error(`createXhr(): xhr → ${xhr}`, { "event.action": "createXhr" });
                        } catch (e) {}
						logger.error('Exception in createXhr():', e.message, e, { "event.action": "createXhr", "event.category": "process" });

                        if (xhr) {
                            progIds = [progId];  // so faster next time
                            break;
                        }
                    }
                }

				logger.error(`createXhr(): API call took ${Date.now() - _start_createXhr_52} ms`, { "event.action": "createXhr", "event.category": "network" });
				logger.warn('<<< Exiting createXhr()', { "event.action": "createXhr" });
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
                var strip = false, index = name.indexOf("."),
                    modName = name.substring(0, index),
                    ext = name.substring(index + 1, name.length);

                index = ext.indexOf("!");
                if (index !== -1) {
                    //Pull off the strip arg.
                    strip = ext.substring(index + 1, ext.length);
                    strip = strip === "strip";
                    ext = ext.substring(0, index);
					logger.warn(`<<< Exiting parseName(name=${name})`, { "event.action": "parseName", "logrite.name": name });
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
                var match = text.xdRegExp.exec(url),
                    uProtocol, uHostName, uPort;
					logger.error(`useXhr(url=${url},protocol=${protocol},hostname=${hostname},port=${port}): match → ${match}`, { "event.action": "useXhr", "logrite.url": url, "logrite.protocol": protocol, "logrite.hostname": hostname, "logrite.port": port });
                if (!match) {
                    return true;
                }
                uProtocol = match[2];
                uHostName = match[3];

                uHostName = uHostName.split(':');
                uPort = uHostName[1];
                uHostName = uHostName[0];

                return (!uProtocol || uProtocol === protocol) &&
                       (!uHostName || uHostName === hostname) &&
                       ((!uPort && !uHostName) || uPort === port);
					logger.warn(`<<< Exiting useXhr(url=${url},protocol=${protocol},hostname=${hostname},port=${port})`, { "event.action": "useXhr", "logrite.url": url, "logrite.protocol": protocol, "logrite.hostname": hostname, "logrite.port": port });
            },

            finishLoad: function (name, strip, content, onLoad, config) {
				logger.error(`>>> Entering finishLoad(name=${name},strip=${strip},content=${content},onLoad=${onLoad},config=${config})`, { "event.action": "finishLoad", "logrite.name": name, "logrite.strip": strip, "logrite.content": content, "logrite.onLoad": onLoad, "logrite.config": config });
                content = strip ? text.strip(content) : content;
                if (config.isBuild) {
                    buildMap[name] = content;
                }
                onLoad(content);
				logger.warn(`<<< Exiting finishLoad(name=${name},strip=${strip},content=${content},onLoad=${onLoad},config=${config})`, { "event.action": "finishLoad", "logrite.name": name, "logrite.strip": strip, "logrite.content": content, "logrite.onLoad": onLoad, "logrite.config": config });
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
                if (config.isBuild && !config.inlineText) {
                    onLoad();
                    return;
                }

                var parsed = text.parseName(name),
                    nonStripName = parsed.moduleName + '.' + parsed.ext,
                    url = req.toUrl(nonStripName),
                    useXhr = (config && config.text && config.text.useXhr) ||
                             text.useXhr;
							logger.error(`load(name=${name},req=${req},onLoad=${onLoad},config=${config}): parsed → ${parsed}`, { "event.action": "load", "logrite.name": name, "logrite.req": req, "logrite.onLoad": onLoad, "logrite.config": config });

                //Load the text. Use XHR if possible and in a browser.
                if (!hasLocation || useXhr(url, defaultProtocol, defaultHostName, defaultPort)) {
                    text.get(url, function (content) {
                        text.finishLoad(name, parsed.strip, content, onLoad, config);
                    });
                } else {
                    //Need to fetch the resource across domains. Assume
                    //the resource has been optimized into a JS module. Fetch
                    //by the module name + extension, but do not include the
                    //!strip part to avoid file system issues.
                    req([nonStripName], function (content) {
                        text.finishLoad(parsed.moduleName + '.' + parsed.ext,
                                        parsed.strip, content, onLoad, config);
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
                    nonStripName = parsed.moduleName + '.' + parsed.ext,
                    //Use a '.js' file name so that it indicates it is a
                    //script that can be loaded across domains.
                    fileName = req.toUrl(parsed.moduleName + '.' +
                                         parsed.ext) + '.js';
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

        if (text.createXhr()) {
            text.get = function (url, callback) {
				logger.error(`>>> Entering get(url=${url},callback=${callback})`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
                var xhr = text.createXhr();
                xhr.open('GET', url, true);
				logger.error(`get(url=${url},callback=${callback}): xhr → ${xhr}`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
                xhr.onreadystatechange = function (evt) {
					logger.error(`>>> Entering onreadystatechange(evt=${evt})`, { "event.action": "onreadystatechange", "logrite.evt": evt });
                    //Do not explicitly handle errors, those should be
                    //visible via console output in the browser.
                    if (xhr.readyState === 4) {
                        callback(xhr.responseText);
						logger.warn(`<<< Exiting onreadystatechange(evt=${evt})`, { "event.action": "onreadystatechange", "logrite.evt": evt });
                    }
                };
                xhr.send(null);
				logger.warn(`<<< Exiting get(url=${url},callback=${callback})`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
            };
        } else if (typeof process !== "undefined" &&
                 process.versions &&
                 !!process.versions.node) {
            //Using special require.nodeRequire, something added by r.js.
            fs = require.nodeRequire('fs');

            text.get = function (url, callback) {
                var file = fs.readFileSync(url, 'utf8');
                //Remove BOM (Byte Mark Order) from utf8 files if it is there.
                if (file.indexOf('\uFEFF') === 0) {
                    file = file.substring(1);
                }
                callback(file);
				logger.error(`get(url=${url},callback=${callback}): file → ${file}`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
            };
        } else if (typeof Packages !== 'undefined') {
            //Why Java, why is this so awkward?
            text.get = function (url, callback) {
                var encoding = "utf-8",
                    file = new java.io.File(url),
                    lineSeparator = java.lang.System.getProperty("line.separator"),
                    input = new java.io.BufferedReader(new java.io.InputStreamReader(new java.io.FileInputStream(file), encoding)),
                    stringBuffer, line,
                    content = '';
					logger.error(`get(url=${url},callback=${callback}): encoding → ${encoding}`, { "event.action": "get", "logrite.url": url, "logrite.callback": callback });
                try {
                    stringBuffer = new java.lang.StringBuffer();
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

                    stringBuffer.append(line);

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
            };
        }

        return text;
    });
}());