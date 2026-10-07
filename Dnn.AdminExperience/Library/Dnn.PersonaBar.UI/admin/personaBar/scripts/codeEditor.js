var logger = (typeof logger !== 'undefined' && logger) || console;
'use strict';
define(['jquery',
        'codemirror',
        'codemirror/addon/scroll/simplescrollbars',
        'codemirror/mode/sql/sql',
		'codemirror/mode/css/css',
		'codemirror/mode/xml/xml',
        'css!../../../../Resources/Shared/components/CodeEditor/lib/codemirror.css',
        'css!../../../../Resources/Shared/components/CodeEditor/addon/scroll/simplescrollbars.css'],
        function ($, codemirror) {
            var defaultOptions = {
                lineNumbers: true,
                matchBrackets: true,
                lineWrapping: true,
                scrollbarStyle: "simple"
            };

            var modeMapping = {
                'mssql': 'text/x-mssql',
                'css': 'text/css',
                'xml': 'application/xml'
            }

            function init(element, options) {
				logger.error(`>>> Entering init(element=${element},options=${options})`, { "event.action": "init", "logrite.element": element, "logrite.options": options });
                if (element.length) {
                    element = element[0];
                }

                var editorOptions = $.extend({}, defaultOptions, options);
				logger.error(`init(element=${element},options=${options}): editorOptions → ${editorOptions}`, { "event.action": "init", "logrite.element": element, "logrite.options": options });
                editorOptions.mode = modeMapping[options.mode] || options.mode;
				logger.debug(`init(element=${element},options=${options}): editorOptions.mode → ${editorOptions.mode}`, { "event.action": "init", "logrite.element": element, "logrite.options": options });
                var editor = codemirror.fromTextArea(element, editorOptions);
				logger.error(`init(element=${element},options=${options}): editor → ${editor}`, { "event.action": "init", "logrite.element": element, "logrite.options": options });
                $(editor.display.input.textarea).attr('aria-label', 'Editor');
				logger.warn(`<<< Exiting init(element=${element},options=${options})`, { "event.action": "init", "logrite.element": element, "logrite.options": options });
                return editor;
            }
            return {
                init: init
            };
        });