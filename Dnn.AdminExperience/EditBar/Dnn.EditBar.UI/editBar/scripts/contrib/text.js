const logger = console;
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
	logger.debug(`>>> Entering function(evt=${evt})`);
    var progIds = ['Msxml2.XMLHTTP', 'Microsoft.XMLHTTP', 'Msxml2.XMLHTTP.4.0'],
        xmlRegExp = /^\s*<\?xml(\s)+version=[\'\"](\d)*.(\d)*[\'\"](\s)*\?>/im,
        bodyRegExp = /<body[^>]*>\s*([\s\S]+)\s*<\/body>/im,
        hasLocation = typeof location !== 'undefined' && location.href,
        defaultProtocol = hasLocation && location.protocol && location.protocol.replace(/\:/, ''),
        defaultHostName = hasLocation && location.hostname,
        defaultPort = hasLocation && (location.port || undefined),
        buildMap = [];

    define(function () {
		logger.debug('>>> Entering define()'); const _start = Date.now();
        var text, fs;

        text = {
            version: '1.0.8',

            strip: function (content) {
                //Strips <?xml ...?> declarations so that external SVG and XML
                //documents can be added to a document without worry. Also, if the string
                //is an HTML document, only the part inside the body tag is returned.
                if (content) {
                    content = content.replace(xmlRegExp, "");
                    var matches = content.match(bodyRegExp);
					logger.debug(`strip(content=${content}): matches → ${matches}`);
                    if (matches) {
                        content = matches[1];
                    }
                } else {
                    content = "";
					logger.debug(`<<< Exiting strip(content=${content})`);
                }
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
				logger.debug('>>> Entering createXhr()'); const _start = Date.now();
                //Would love to dump the ActiveX crap in here. Need IE 6 to die first.
                var xhr, i, progId;
                if (typeof XMLHttpRequest !== "undefined") {
                    return new XMLHttpRequest();
                } else if (typeof ActiveXObject !== "undefined") {
                    for (i = 0; i < 3; i++) {
                        progId = progIds[i];
                        try {
                            xhr = new ActiveXObject(progId);
                        } catch (e) {}
						logger.error('Exception in define():', e.message, e);

                        if (xhr) {
							logger.error('Exception in createXhr():', e.message, e);
                            progIds = [progId];  // so faster next time
                            break;
                        }
                    }
                }

				logger.info(`createXhr(): API call took ${Date.now() - _start} ms`);
				logger.debug('<<< Exiting createXhr()');
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
				logger.debug(`>>> Entering parseName(name=${name})`);
                var strip = false, index = name.indexOf("."),
                    modName = name.substring(0, index),
                    ext = name.substring(index + 1, name.length);

                index = ext.indexOf("!");
                if (index !== -1) {
                    //Pull off the strip arg.
                    strip = ext.substring(index + 1, ext.length);
                    strip = strip === "strip";
                    ext = ext.substring(0, index);
					logger.debug(`<<< Exiting parseName(name=${name})`);
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
				logger.debug(`>>> Entering useXhr(url=${url},protocol=${protocol},hostname=${hostname},port=${port})`);
                var match = text.xdRegExp.exec(url),
                    uProtocol, uHostName, uPort;
					logger.debug(`useXhr(url=${url},protocol=${protocol},hostname=${hostname},port=${port}): match → ${match}`);
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
					logger.debug(`useXhr(url=${url},protocol=${protocol},hostname=${hostname},port=${port}): uHostName → ${uHostName}`);
					logger.debug(`<<< Exiting useXhr(url=${url},protocol=${protocol},hostname=${hostname},port=${port})`);
            },

            finishLoad: function (name, strip, content, onLoad, config) {
                content = strip ? text.strip(content) : content;
				logger.info(`define(): API call took ${Date.now() - _start} ms`);
                if (config.isBuild) {
                    buildMap[name] = content;
                }
                onLoad(content);
				logger.debug(`finishLoad(name=${name},strip=${strip},content=${content},onLoad=${onLoad},config=${config}): content → ${content}`);
				logger.debug(`<<< Exiting finishLoad(name=${name},strip=${strip},content=${content},onLoad=${onLoad},config=${config})`);
            },

            load: function (name, req, onLoad, config) {
				logger.debug(`>>> Entering load(name=${name},req=${req},onLoad=${onLoad},config=${config})`);
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
							logger.debug(`load(name=${name},req=${req},onLoad=${onLoad},config=${config}): parsed → ${parsed}`);

                //Load the text. Use XHR if possible and in a browser.
                if (!hasLocation || useXhr(url, defaultProtocol, defaultHostName, defaultPort)) {
                    text.get(url, function (content) {
						logger.debug(`>>> Entering get(url=${url})`);
						logger.debug(`<<< Exiting get(url=${url})`);
                        text.finishLoad(name, parsed.strip, content, onLoad, config);
                    });
                } else {
                    //Need to fetch the resource across domains. Assume
                    //the resource has been optimized into a JS module. Fetch
                    //by the module name + extension, but do not include the
                    //!strip part to avoid file system issues.
                    req([nonStripName], function (content) {
						logger.debug(`>>> Entering req(nonStripName=${nonStripName})`);
                        text.finishLoad(parsed.moduleName + '.' + parsed.ext,
                                        parsed.strip, content, onLoad, config);
										logger.debug(`<<< Exiting req(nonStripName=${nonStripName})`);
                    });
                }
				logger.debug(`<<< Exiting load(name=${name},req=${req},onLoad=${onLoad},config=${config})`);
            },

            write: function (pluginName, moduleName, write, config) {
				logger.debug(`>>> Entering write(pluginName=${pluginName},moduleName=${moduleName},write=${write},config=${config})`);
                if (buildMap.hasOwnProperty(moduleName)) {
                    var content = text.jsEscape(buildMap[moduleName]);
					logger.debug(`write(pluginName=${pluginName},moduleName=${moduleName},write=${write},config=${config}): content → ${content}`);
                    write.asModule(pluginName + "!" + moduleName,
                                   "define(function () { return '" +
                                       content +
                                   "';});\n");
								logger.debug(`<<< Exiting write(pluginName=${pluginName},moduleName=${moduleName},write=${write},config=${config})`);
                }
            },

            writeFile: function (pluginName, moduleName, req, write, config) {
				logger.debug(`>>> Entering writeFile(pluginName=${pluginName},moduleName=${moduleName},req=${req},write=${write},config=${config})`);
                var parsed = text.parseName(moduleName),
                    nonStripName = parsed.moduleName + '.' + parsed.ext,
                    //Use a '.js' file name so that it indicates it is a
                    //script that can be loaded across domains.
                    fileName = req.toUrl(parsed.moduleName + '.' +
                                         parsed.ext) + '.js';
										logger.debug(`writeFile(pluginName=${pluginName},moduleName=${moduleName},req=${req},write=${write},config=${config}): parsed → ${parsed}`);

				logger.debug(`<<< Exiting writeFile(pluginName=${pluginName},moduleName=${moduleName},req=${req},write=${write},config=${config})`);
                //Leverage own load() method to load plugin value, but only
                //write out values that do not have the strip argument,
                //to avoid any potential issues with ! in file names.
                text.load(nonStripName, req, function (value) {
                    //Use own write() method to construct full module value.
                    //But need to create shell that translates writeFile's
                    //write() to the right interface.
                    var textWrite = function (contents) {
						logger.debug(`>>> Entering textWrite(contents=${contents})`);
						logger.debug(`<<< Exiting textWrite(contents=${contents})`);
                        return write(fileName, contents);
                    };
                    textWrite.asModule = function (moduleName, contents) {
						logger.debug(`<<< Exiting function(moduleName=${moduleName},contents=${contents})`);
                        return write.asModule(moduleName, fileName, contents);
                    };

                    text.write(pluginName, nonStripName, textWrite, config);
                }, config);
            }
        };

        if (text.createXhr()) {
            text.get = function (url, callback) {
                var xhr = text.createXhr();
                xhr.open('GET', url, true);
				logger.debug(`function(url=${url},callback=${callback}): xhr → ${xhr}`);
                xhr.onreadystatechange = function (evt) {
                    //Do not explicitly handle errors, those should be
                    //visible via console output in the browser.
                    if (xhr.readyState === 4) {
                        callback(xhr.responseText);
                    }
                };
                xhr.send(null);
            };
        } else if (typeof process !== "undefined" &&
                 process.versions &&
                 !!process.versions.node) {
            //Using special require.nodeRequire, something added by r.js.
            fs = require.nodeRequire('fs');

            text.get = function (url, callback) {
                var file = fs.readFileSync(url, 'utf8');
				logger.debug(`function(url=${url},callback=${callback}): file → ${file}`);
                //Remove BOM (Byte Mark Order) from utf8 files if it is there.
                if (file.indexOf('\uFEFF') === 0) {
                    file = file.substring(1);
                }
                callback(file);
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
					logger.debug(`function(url=${url},callback=${callback}): encoding → ${encoding}`);
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