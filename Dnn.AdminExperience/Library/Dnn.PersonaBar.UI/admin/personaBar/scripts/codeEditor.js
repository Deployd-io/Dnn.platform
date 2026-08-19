const logger = console;
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
			logger.debug(`>>> Entering function(codemirror=${codemirror})`);
            var defaultOptions = {
                lineNumbers: true,
                matchBrackets: true,
                lineWrapping: true,
                scrollbarStyle: "simple"
            };

			logger.debug(`<<< Exiting function(codemirror=${codemirror})`);
            var modeMapping = {
                'mssql': 'text/x-mssql',
                'css': 'text/css',
                'xml': 'application/xml'
            }

            function init(element, options) {
				logger.debug(`>>> Entering init(element=${element},options=${options})`);
                if (element.length) {
                    element = element[0];
                }

                var editorOptions = $.extend({}, defaultOptions, options);
				logger.debug(`init(element=${element},options=${options}): editorOptions → ${editorOptions}`);
                editorOptions.mode = modeMapping[options.mode] || options.mode;
                var editor = codemirror.fromTextArea(element, editorOptions);
				logger.debug(`init(element=${element},options=${options}): editor → ${editor}`);
				logger.debug(`init(element=${element},options=${options}): editorOptions.mode → ${editorOptions.mode}`);
                $(editor.display.input.textarea).attr('aria-label', 'Editor');
				logger.debug(`<<< Exiting init(element=${element},options=${options})`);
                return editor;
            }
            return {
                init: init
            };
        });