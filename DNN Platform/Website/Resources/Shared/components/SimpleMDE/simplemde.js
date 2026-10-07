var logger = (typeof logger !== 'undefined' && logger) || console;
var isMac = /Mac/.test(navigator.platform);

var shortcuts = {
	'Cmd-B': toggleBold,
	'Cmd-I': toggleItalic,
	'Cmd-K': drawLink,
	'Cmd-H': toggleHeadingSmaller,
	'Shift-Cmd-H': toggleHeadingBigger,
	'Cmd-Alt-I': drawImage,
	"Cmd-'": toggleBlockquote,
	'Cmd-Alt-L': toggleOrderedList,
	'Cmd-L': toggleUnorderedList,
	'Cmd-Alt-C': toggleCodeBlock,
	'Cmd-P': togglePreview,
};


/**
 * Fix shortcut. Mac use Command, others use Ctrl.
 */
function fixShortcut(name) {
	logger.error(`>>> Entering fixShortcut(name=${name})`, { "event.action": "fixShortcut", "logrite.name": name });
	if(isMac) {
		name = name.replace('Ctrl', 'Cmd');
	} else {
		name = name.replace('Cmd', 'Ctrl');
	}
	logger.warn(`<<< Exiting fixShortcut(name=${name})`, { "event.action": "fixShortcut", "logrite.name": name });
	return name;
}


/**
 * Create icon element for toolbar.
 */
function createIcon(options, enableTooltips) {
	logger.error(`>>> Entering createIcon(options=${options},enableTooltips=${enableTooltips})`, { "event.action": "createIcon", "logrite.options": options, "logrite.enableTooltips": enableTooltips });
	options = options || {};
	var el = document.createElement('a');
	logger.error(`createIcon(options=${options},enableTooltips=${enableTooltips}): el → ${el}`, { "event.action": "createIcon", "logrite.options": options, "logrite.enableTooltips": enableTooltips });
	logger.debug(`createIcon(options=${options},enableTooltips=${enableTooltips}): el.title → ${el.title}`, { "event.action": "createIcon", "logrite.options": options, "logrite.enableTooltips": enableTooltips });
	logger.debug(`createIcon(options=${options},enableTooltips=${enableTooltips}): el.className → ${el.className}`, { "event.action": "createIcon", "logrite.options": options, "logrite.enableTooltips": enableTooltips });
	enableTooltips = (enableTooltips == undefined) ? true : enableTooltips;
	logger.error(`createIcon(options=${options},enableTooltips=${enableTooltips}): enableTooltips → ${enableTooltips}`, { "event.action": "createIcon", "logrite.options": options, "logrite.enableTooltips": enableTooltips });

	if(options.title && enableTooltips) {
		el.title = options.title;

		if(isMac) {
			el.title = el.title.replace('Ctrl', '⌘');
			el.title = el.title.replace('Alt', '⌥');
		}
	}

	el.className = options.className;
	logger.warn(`<<< Exiting createIcon(options=${options},enableTooltips=${enableTooltips})`, { "event.action": "createIcon", "logrite.options": options, "logrite.enableTooltips": enableTooltips });
	return el;
}

function createSep() {
	logger.error('>>> Entering createSep()', { "event.action": "createSep" });
	el = document.createElement('i');
	el.className = 'separator';
	el.innerHTML = '|';
	logger.warn('<<< Exiting createSep()', { "event.action": "createSep" });
	return el;
}


/**
 * The state of CodeMirror at the given position.
 */
function getState(cm, pos) {
	logger.error(`>>> Entering getState(cm=${cm},pos=${pos})`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });
	pos = pos || cm.getCursor('start');
	var stat = cm.getTokenAt(pos);
	logger.error(`getState(cm=${cm},pos=${pos}): pos → ${pos}`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });
	logger.error(`getState(cm=${cm},pos=${pos}): stat → ${stat}`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });
	if(!stat.type) return {};

	var types = stat.type.split(' ');
	logger.error(`getState(cm=${cm},pos=${pos}): types → ${types}`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });

	var ret = {},
		data, text;
		logger.debug(`getState(cm=${cm},pos=${pos}): ret.quote → ${ret.quote}`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });
		logger.debug(`getState(cm=${cm},pos=${pos}): ret.bold → ${ret.bold}`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });
		logger.debug(`getState(cm=${cm},pos=${pos}): ret.italic → ${ret.italic}`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });
		logger.debug(`getState(cm=${cm},pos=${pos}): ret.strikethrough → ${ret.strikethrough}`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });
		logger.debug(`getState(cm=${cm},pos=${pos}): ret.code → ${ret.code}`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });
	for(var i = 0; i < types.length; i++) {
		data = types[i];
		if(data === 'strong') {
			ret.bold = true;
		} else if(data === 'variable-2') {
			text = cm.getLine(pos.line);
			if(/^\s*\d+\.\s/.test(text)) {
				ret['ordered-list'] = true;
			} else {
				ret['unordered-list'] = true;
			}
		} else if(data === 'atom') {
			ret.quote = true;
		} else if(data === 'em') {
			ret.italic = true;
		} else if(data === 'quote') {
			ret.quote = true;
		} else if(data === 'strikethrough') {
			ret.strikethrough = true;
		} else if(data === 'comment') {
			ret.code = true;
		}
	}
	logger.warn(`<<< Exiting getState(cm=${cm},pos=${pos})`, { "event.action": "getState", "logrite.cm": cm, "logrite.pos": pos });
	return ret;
}


// Saved overflow setting
var saved_overflow = "";

/**
 * Toggle full screen of the editor.
 */
function toggleFullScreen(editor) {
	logger.error(`>>> Entering toggleFullScreen(editor=${editor})`, { "event.action": "toggleFullScreen" });
	// Set fullscreen
	var cm = editor.codemirror;
	logger.error(`toggleFullScreen(editor=${editor}): cm → ${cm}`, { "event.action": "toggleFullScreen" });
	cm.setOption("fullScreen", !cm.getOption("fullScreen"));


	// Prevent scrolling on body during fullscreen active
	if(cm.getOption("fullScreen")) {
		saved_overflow = document.body.style.overflow;
		document.body.style.overflow = "hidden";
	} else {
		document.body.style.overflow = saved_overflow;
	}


	// Update toolbar class
	var wrap = cm.getWrapperElement();
	logger.error(`toggleFullScreen(editor=${editor}): wrap → ${wrap}`, { "event.action": "toggleFullScreen" });

	if(!/fullscreen/.test(wrap.previousSibling.className)) {
		wrap.previousSibling.className += " fullscreen";
	} else {
		wrap.previousSibling.className = wrap.previousSibling.className.replace(/\s*fullscreen\b/, "");
	}


	// Update toolbar button
	var toolbarButton = editor.toolbarElements.fullscreen;

	if(!/active/.test(toolbarButton.className)) {
		toolbarButton.className += " active";
	} else {
		toolbarButton.className = toolbarButton.className.replace(/\s*active\s*/g, '');
	}


	// Hide side by side if needed
	var sidebyside = cm.getWrapperElement().nextSibling;
	if(/editor-preview-active-side/.test(sidebyside.className))
		toggleSideBySide(editor);
		logger.warn(`<<< Exiting toggleFullScreen(editor=${editor})`, { "event.action": "toggleFullScreen" });
		logger.error('>>> Entering toggleFullScreen()', { "event.action": "toggleFullScreen" });
		logger.warn('<<< Exiting toggleFullScreen()', { "event.action": "toggleFullScreen" });
}


/**
 * Action for toggling bold.
 */
function toggleBold(editor) {
	logger.error(`>>> Entering toggleBold(editor=${editor})`, { "event.action": "toggleBold" });
	_toggleBlock(editor, 'bold', '**');
	logger.warn(`<<< Exiting toggleBold(editor=${editor})`, { "event.action": "toggleBold" });
	logger.error('>>> Entering toggleBold()', { "event.action": "toggleBold" });
	logger.warn('<<< Exiting toggleBold()', { "event.action": "toggleBold" });
}


/**
 * Action for toggling italic.
 */
function toggleItalic(editor) {
	logger.error(`>>> Entering toggleItalic(editor=${editor})`, { "event.action": "toggleItalic" });
	_toggleBlock(editor, 'italic', '*');
	logger.warn(`<<< Exiting toggleItalic(editor=${editor})`, { "event.action": "toggleItalic" });
	logger.error('>>> Entering toggleItalic()', { "event.action": "toggleItalic" });
	logger.warn('<<< Exiting toggleItalic()', { "event.action": "toggleItalic" });
}


/**
 * Action for toggling strikethrough.
 */
function toggleStrikethrough(editor) {
	logger.error(`>>> Entering toggleStrikethrough(editor=${editor})`, { "event.action": "toggleStrikethrough" });
	_toggleBlock(editor, 'strikethrough', '~~');
	logger.warn(`<<< Exiting toggleStrikethrough(editor=${editor})`, { "event.action": "toggleStrikethrough" });
	logger.error('>>> Entering toggleStrikethrough()', { "event.action": "toggleStrikethrough" });
	logger.warn('<<< Exiting toggleStrikethrough()', { "event.action": "toggleStrikethrough" });
}

/**
 * Action for toggling code block.
 */
function toggleCodeBlock(editor) {
	logger.error(`>>> Entering toggleCodeBlock(editor=${editor})`, { "event.action": "toggleCodeBlock" });
	_toggleBlock(editor, 'code', '```\r\n', '\r\n```');
	logger.warn(`<<< Exiting toggleCodeBlock(editor=${editor})`, { "event.action": "toggleCodeBlock" });
	logger.error('>>> Entering toggleCodeBlock()', { "event.action": "toggleCodeBlock" });
	logger.warn('<<< Exiting toggleCodeBlock()', { "event.action": "toggleCodeBlock" });
}

/**
 * Action for toggling blockquote.
 */
function toggleBlockquote(editor) {
	logger.error(`>>> Entering toggleBlockquote(editor=${editor})`, { "event.action": "toggleBlockquote" });
	var cm = editor.codemirror;
	_toggleLine(cm, 'quote');
	logger.warn(`<<< Exiting toggleBlockquote(editor=${editor})`, { "event.action": "toggleBlockquote" });
	logger.error('>>> Entering toggleBlockquote()', { "event.action": "toggleBlockquote" });
	logger.warn('<<< Exiting toggleBlockquote()', { "event.action": "toggleBlockquote" });
}

/**
 * Action for toggling heading size: normal -> h1 -> h2 -> h3 -> h4 -> h5 -> h6 -> normal
 */
function toggleHeadingSmaller(editor) {
	logger.error(`>>> Entering toggleHeadingSmaller(editor=${editor})`, { "event.action": "toggleHeadingSmaller" });
	var cm = editor.codemirror;
	_toggleHeading(cm, 'smaller');
	logger.warn(`<<< Exiting toggleHeadingSmaller(editor=${editor})`, { "event.action": "toggleHeadingSmaller" });
	logger.error('>>> Entering toggleHeadingSmaller()', { "event.action": "toggleHeadingSmaller" });
	logger.warn('<<< Exiting toggleHeadingSmaller()', { "event.action": "toggleHeadingSmaller" });
}

/**
 * Action for toggling heading size: normal -> h6 -> h5 -> h4 -> h3 -> h2 -> h1 -> normal
 */
function toggleHeadingBigger(editor) {
	logger.error(`>>> Entering toggleHeadingBigger(editor=${editor})`, { "event.action": "toggleHeadingBigger" });
	var cm = editor.codemirror;
	_toggleHeading(cm, 'bigger');
	logger.warn(`<<< Exiting toggleHeadingBigger(editor=${editor})`, { "event.action": "toggleHeadingBigger" });
	logger.error('>>> Entering toggleHeadingBigger()', { "event.action": "toggleHeadingBigger" });
	logger.warn('<<< Exiting toggleHeadingBigger()', { "event.action": "toggleHeadingBigger" });
}

/**
 * Action for toggling heading size 1
 */
function toggleHeading1(editor) {
	logger.error(`>>> Entering toggleHeading1(editor=${editor})`, { "event.action": "toggleHeading1" });
	var cm = editor.codemirror;
	_toggleHeading(cm, undefined, 1);
	logger.warn(`<<< Exiting toggleHeading1(editor=${editor})`, { "event.action": "toggleHeading1" });
	logger.error('>>> Entering toggleHeading1()', { "event.action": "toggleHeading1" });
	logger.warn('<<< Exiting toggleHeading1()', { "event.action": "toggleHeading1" });
}

/**
 * Action for toggling heading size 2
 */
function toggleHeading2(editor) {
	logger.error(`>>> Entering toggleHeading2(editor=${editor})`, { "event.action": "toggleHeading2" });
	var cm = editor.codemirror;
	_toggleHeading(cm, undefined, 2);
	logger.warn(`<<< Exiting toggleHeading2(editor=${editor})`, { "event.action": "toggleHeading2" });
	logger.error('>>> Entering toggleHeading2()', { "event.action": "toggleHeading2" });
	logger.warn('<<< Exiting toggleHeading2()', { "event.action": "toggleHeading2" });
}

/**
 * Action for toggling heading size 3
 */
function toggleHeading3(editor) {
	logger.error(`>>> Entering toggleHeading3(editor=${editor})`, { "event.action": "toggleHeading3" });
	var cm = editor.codemirror;
	_toggleHeading(cm, undefined, 3);
	logger.warn(`<<< Exiting toggleHeading3(editor=${editor})`, { "event.action": "toggleHeading3" });
	logger.error('>>> Entering toggleHeading3()', { "event.action": "toggleHeading3" });
	logger.warn('<<< Exiting toggleHeading3()', { "event.action": "toggleHeading3" });
}


/**
 * Action for toggling ul.
 */
function toggleUnorderedList(editor) {
	logger.error(`>>> Entering toggleUnorderedList(editor=${editor})`, { "event.action": "toggleUnorderedList" });
	var cm = editor.codemirror;
	_toggleLine(cm, 'unordered-list');
	logger.warn(`<<< Exiting toggleUnorderedList(editor=${editor})`, { "event.action": "toggleUnorderedList" });
	logger.error('>>> Entering toggleUnorderedList()', { "event.action": "toggleUnorderedList" });
	logger.warn('<<< Exiting toggleUnorderedList()', { "event.action": "toggleUnorderedList" });
}


/**
 * Action for toggling ol.
 */
function toggleOrderedList(editor) {
	logger.error(`>>> Entering toggleOrderedList(editor=${editor})`, { "event.action": "toggleOrderedList" });
	var cm = editor.codemirror;
	_toggleLine(cm, 'ordered-list');
	logger.warn(`<<< Exiting toggleOrderedList(editor=${editor})`, { "event.action": "toggleOrderedList" });
	logger.error('>>> Entering toggleOrderedList()', { "event.action": "toggleOrderedList" });
	logger.warn('<<< Exiting toggleOrderedList()', { "event.action": "toggleOrderedList" });
}


/**
 * Action for drawing a link.
 */
function drawLink(editor) {
	logger.error(`>>> Entering drawLink(editor=${editor})`, { "event.action": "drawLink" });
	var cm = editor.codemirror;
	var stat = getState(cm);
	_replaceSelection(cm, stat.link, '[', '](http://)');
	logger.error(`drawLink(editor=${editor}): stat → ${stat}`, { "event.action": "drawLink" });
	logger.error('>>> Entering drawLink()', { "event.action": "drawLink" });
	logger.warn('<<< Exiting drawLink()', { "event.action": "drawLink" });
}


/**
 * Action for drawing an img.
 */
function drawImage(editor) {
	logger.error(`>>> Entering drawImage(editor=${editor})`, { "event.action": "drawImage" });
	var cm = editor.codemirror;
	var stat = getState(cm);
	_replaceSelection(cm, stat.image, '![](http://', ')');
	logger.error(`drawImage(editor=${editor}): stat → ${stat}`, { "event.action": "drawImage" });
	logger.error('>>> Entering drawImage()', { "event.action": "drawImage" });
	logger.warn('<<< Exiting drawImage()', { "event.action": "drawImage" });
}


/**
 * Action for drawing a horizontal rule.
 */
function drawHorizontalRule(editor) {
	logger.error(`>>> Entering drawHorizontalRule(editor=${editor})`, { "event.action": "drawHorizontalRule" });
	var cm = editor.codemirror;
	var stat = getState(cm);
	_replaceSelection(cm, stat.image, '', '\n\n-----\n\n');
	logger.error(`drawHorizontalRule(editor=${editor}): stat → ${stat}`, { "event.action": "drawHorizontalRule" });
	logger.error('>>> Entering drawHorizontalRule()', { "event.action": "drawHorizontalRule" });
	logger.warn('<<< Exiting drawHorizontalRule()', { "event.action": "drawHorizontalRule" });
}


/**
 * Undo action.
 */
function undo(editor) {
	logger.error(`>>> Entering undo(editor=${editor})`, { "event.action": "undo" });
	var cm = editor.codemirror;
	cm.undo();
	cm.focus();
	logger.warn(`<<< Exiting undo(editor=${editor})`, { "event.action": "undo" });
	logger.error('>>> Entering undo()', { "event.action": "undo" });
	logger.warn('<<< Exiting undo()', { "event.action": "undo" });
}


/**
 * Redo action.
 */
function redo(editor) {
	logger.error(`>>> Entering redo(editor=${editor})`, { "event.action": "redo" });
	var cm = editor.codemirror;
	cm.redo();
	cm.focus();
	logger.warn(`<<< Exiting redo(editor=${editor})`, { "event.action": "redo" });
	logger.error('>>> Entering redo()', { "event.action": "redo" });
	logger.warn('<<< Exiting redo()', { "event.action": "redo" });
}


/**
 * Toggle side by side preview
 */
function toggleSideBySide(editor) {
	logger.error(`>>> Entering toggleSideBySide(editor=${editor})`, { "event.action": "toggleSideBySide" });
	var cm = editor.codemirror;
	var wrapper = cm.getWrapperElement();
	logger.error(`toggleSideBySide(editor=${editor}): wrapper → ${wrapper}`, { "event.action": "toggleSideBySide" });
	var code = wrapper.firstChild;
	var preview = wrapper.nextSibling;
	var toolbarButton = editor.toolbarElements["side-by-side"];
	logger.error(`toggleSideBySide(editor=${editor}): toolbarButton → ${toolbarButton}`, { "event.action": "toggleSideBySide" });

	if(/editor-preview-active-side/.test(preview.className)) {
		preview.className = preview.className.replace(
			/\s*editor-preview-active-side\s*/g, ''
		);
		toolbarButton.className = toolbarButton.className.replace(/\s*active\s*/g, '');
		wrapper.className = wrapper.className.replace(/\s*CodeMirror-sided\s*/g, ' ');
	} else {
		logger.warn(`<<< Exiting toggleSideBySide(editor=${editor})`, { "event.action": "toggleSideBySide" });
		/* When the preview button is clicked for the first time,
		 * give some time for the transition from editor.css to fire and the view to slide from right to left,
		 * instead of just appearing.
		 */
		setTimeout(function() {
			if(!cm.getOption("fullScreen")) toggleFullScreen(editor);
			preview.className += ' editor-preview-active-side'
		}, 1);
		toolbarButton.className += ' active';
		wrapper.className += ' CodeMirror-sided';
	}

	// Hide normal preview if active
	var previewNormal = wrapper.lastChild;
	if(/editor-preview-active/.test(previewNormal.className)) {
		previewNormal.className = previewNormal.className.replace(
			/\s*editor-preview-active\s*/g, ''
		);
		var toolbar = editor.toolbarElements.preview;
		var toolbar_div = wrapper.previousSibling;
		toolbar.className = toolbar.className.replace(/\s*active\s*/g, '');
		toolbar_div.className = toolbar_div.className.replace(/\s*disabled-for-preview*/g, '');
	}

	// Start preview with the current text
	preview.innerHTML = editor.options.previewRender(editor.value(), preview);

	// Updates preview
	cm.on('update', function() {
		preview.innerHTML = editor.options.previewRender(editor.value(), preview);
	});
	logger.error('>>> Entering toggleSideBySide()', { "event.action": "toggleSideBySide" });
	logger.warn('<<< Exiting toggleSideBySide()', { "event.action": "toggleSideBySide" });
}


/**
 * Preview action.
 */
function togglePreview(editor) {
	logger.error(`>>> Entering togglePreview(editor=${editor})`, { "event.action": "togglePreview" });
	var cm = editor.codemirror;
	var wrapper = cm.getWrapperElement();
	logger.error(`togglePreview(editor=${editor}): wrapper → ${wrapper}`, { "event.action": "togglePreview" });
	var toolbar_div = wrapper.previousSibling;
	var toolbar = editor.toolbarElements.preview;
	var preview = wrapper.lastChild;
	logger.error(`togglePreview(editor=${editor}): preview → ${preview}`, { "event.action": "togglePreview" });
	logger.debug(`togglePreview(editor=${editor}): preview.className → ${preview.className}`, { "event.action": "togglePreview" });
	if(!preview || !/editor-preview/.test(preview.className)) {
		preview = document.createElement('div');
		preview.className = 'editor-preview';
		wrapper.appendChild(preview);
	}
	if(/editor-preview-active/.test(preview.className)) {
		preview.className = preview.className.replace(
			/\s*editor-preview-active\s*/g, ''
		);
		toolbar.className = toolbar.className.replace(/\s*active\s*/g, '');
		toolbar_div.className = toolbar_div.className.replace(/\s*disabled-for-preview*/g, '');
	} else {
		logger.warn(`<<< Exiting togglePreview(editor=${editor})`, { "event.action": "togglePreview" });
		/* When the preview button is clicked for the first time,
		 * give some time for the transition from editor.css to fire and the view to slide from right to left,
		 * instead of just appearing.
		 */
		setTimeout(function() {
			preview.className += ' editor-preview-active'
		}, 1);
		toolbar.className += ' active';
		toolbar_div.className += ' disabled-for-preview';
	}
	preview.innerHTML = editor.options.previewRender(editor.value(), preview);

	// Turn off side by side if needed
	var sidebyside = cm.getWrapperElement().nextSibling;
	if(/editor-preview-active-side/.test(sidebyside.className))
		toggleSideBySide(editor);
		logger.error('>>> Entering togglePreview()', { "event.action": "togglePreview" });
		logger.warn('<<< Exiting togglePreview()', { "event.action": "togglePreview" });
}

function _replaceSelection(cm, active, start, end) {
	logger.error(`>>> Entering _replaceSelection(cm=${cm},active=${active},start=${start},end=${end})`, { "event.action": "_replaceSelection", "logrite.cm": cm, "logrite.active": active, "logrite.start": start, "logrite.end": end });
	if(/editor-preview-active/.test(cm.getWrapperElement().lastChild.className))
		return;

	var text;
	var startPoint = cm.getCursor('start');
	logger.error(`_replaceSelection(cm=${cm},active=${active},start=${start},end=${end}): startPoint → ${startPoint}`, { "event.action": "_replaceSelection", "logrite.cm": cm, "logrite.active": active, "logrite.start": start, "logrite.end": end });
	var endPoint = cm.getCursor('end');
	logger.error(`_replaceSelection(cm=${cm},active=${active},start=${start},end=${end}): endPoint → ${endPoint}`, { "event.action": "_replaceSelection", "logrite.cm": cm, "logrite.active": active, "logrite.start": start, "logrite.end": end });
	if(active) {
		text = cm.getLine(startPoint.line);
		start = text.slice(0, startPoint.ch);
		end = text.slice(startPoint.ch);
		cm.replaceRange(start + end, {
			line: startPoint.line,
			ch: 0
		});
	} else {
		logger.error(`_replaceSelection(cm=${cm},active=${active},start=${start},end=${end}): end → ${end}`, { "event.action": "_replaceSelection", "logrite.cm": cm, "logrite.active": active, "logrite.start": start, "logrite.end": end });
		text = cm.getSelection();
		cm.replaceSelection(start + text + end);
		logger.error(`_replaceSelection(cm=${cm},active=${active},start=${start},end=${end}): text → ${text}`, { "event.action": "_replaceSelection", "logrite.cm": cm, "logrite.active": active, "logrite.start": start, "logrite.end": end });
		logger.debug(`_replaceSelection(cm=${cm},active=${active},start=${start},end=${end}): text → ${text}`, { "event.action": "_replaceSelection", "logrite.cm": cm, "logrite.active": active, "logrite.start": start, "logrite.end": end });

		startPoint.ch += start.length;
		if(startPoint !== endPoint) {
			endPoint.ch += start.length;
		}
	}
	cm.setSelection(startPoint, endPoint);
	cm.focus();
	logger.warn(`<<< Exiting _replaceSelection(cm=${cm},active=${active},start=${start},end=${end})`, { "event.action": "_replaceSelection", "logrite.cm": cm, "logrite.active": active, "logrite.start": start, "logrite.end": end });
}


function _toggleHeading(cm, direction, size) {
	logger.error(`>>> Entering _toggleHeading(cm=${cm},direction=${direction},size=${size})`, { "event.action": "_toggleHeading", "logrite.cm": cm, "logrite.direction": direction, "logrite.size": size });
	if(/editor-preview-active/.test(cm.getWrapperElement().lastChild.className))
		return;

	var startPoint = cm.getCursor('start');
	logger.error(`_toggleHeading(cm=${cm},direction=${direction},size=${size}): startPoint → ${startPoint}`, { "event.action": "_toggleHeading", "logrite.cm": cm, "logrite.direction": direction, "logrite.size": size });
	var endPoint = cm.getCursor('end');
	for(var i = startPoint.line; i <= endPoint.line; i++) {
		(function(i) {
			var text = cm.getLine(i);
			var currHeadingLevel = text.search(/[^#]/);

			if(direction !== undefined) {
				if(currHeadingLevel <= 0) {
					if(direction == 'bigger') {
						text = '###### ' + text;
					} else {
						text = '# ' + text;
					}
				} else if(currHeadingLevel == 6 && direction == 'smaller') {
					text = text.substr(7);
				} else if(currHeadingLevel == 1 && direction == 'bigger') {
					text = text.substr(2);
				} else {
					if(direction == 'bigger') {
						text = text.substr(1);
					} else {
						text = '#' + text;
					}
				}
			} else {
				if(size == 1) {
					if(currHeadingLevel <= 0) {
						text = '# ' + text;
					} else if(currHeadingLevel == size) {
						text = text.substr(currHeadingLevel + 1);
					} else {
						text = '# ' + text.substr(currHeadingLevel + 1);
					}
				} else if(size == 2) {
					if(currHeadingLevel <= 0) {
						text = '## ' + text;
					} else if(currHeadingLevel == size) {
						text = text.substr(currHeadingLevel + 1);
					} else {
						text = '## ' + text.substr(currHeadingLevel + 1);
					}
				} else {
					if(currHeadingLevel <= 0) {
						text = '### ' + text;
					} else if(currHeadingLevel == size) {
						text = text.substr(currHeadingLevel + 1);
					} else {
						text = '### ' + text.substr(currHeadingLevel + 1);
					}
				}
			}

			cm.replaceRange(text, {
				line: i,
				ch: 0
			}, {
				line: i,
				ch: 99999999999999
			});
		})(i);
	}
	cm.focus();
	logger.error(`_toggleHeading(cm=${cm},direction=${direction},size=${size}): endPoint → ${endPoint}`, { "event.action": "_toggleHeading", "logrite.cm": cm, "logrite.direction": direction, "logrite.size": size });
}


function _toggleLine(cm, name) {
	logger.error(`>>> Entering _toggleLine(cm=${cm},name=${name})`, { "event.action": "_toggleLine", "logrite.cm": cm, "logrite.name": name });
	if(/editor-preview-active/.test(cm.getWrapperElement().lastChild.className))
		return;

	var stat = getState(cm);
	logger.error(`_toggleLine(cm=${cm},name=${name}): stat → ${stat}`, { "event.action": "_toggleLine", "logrite.cm": cm, "logrite.name": name });
	var startPoint = cm.getCursor('start');
	logger.error(`_toggleLine(cm=${cm},name=${name}): startPoint → ${startPoint}`, { "event.action": "_toggleLine", "logrite.cm": cm, "logrite.name": name });
	var endPoint = cm.getCursor('end');
	var repl = {
		'quote': /^(\s*)\>\s+/,
		'unordered-list': /^(\s*)(\*|\-|\+)\s+/,
		'ordered-list': /^(\s*)\d+\.\s+/
	};
	var map = {
		'quote': '> ',
		'unordered-list': '* ',
		'ordered-list': '1. '
	};
	for(var i = startPoint.line; i <= endPoint.line; i++) {
		(function(i) {
			var text = cm.getLine(i);
			if(stat[name]) {
				text = text.replace(repl[name], '$1');
			} else {
				text = map[name] + text;
			}
			cm.replaceRange(text, {
				line: i,
				ch: 0
			}, {
				line: i,
				ch: 99999999999999
			});
		})(i);
	}
	cm.focus();
	logger.error(`_toggleLine(cm=${cm},name=${name}): endPoint → ${endPoint}`, { "event.action": "_toggleLine", "logrite.cm": cm, "logrite.name": name });
}

function _toggleBlock(editor, type, start_chars, end_chars) {
	logger.error(`>>> Entering _toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars})`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });
	if(/editor-preview-active/.test(editor.codemirror.getWrapperElement().lastChild.className))
		return;

	end_chars = (typeof end_chars === 'undefined') ? start_chars : end_chars;
	var cm = editor.codemirror;
	logger.error(`_toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars}): end_chars → ${end_chars}`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });
	logger.error(`_toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars}): cm → ${cm}`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });
	var stat = getState(cm);

	logger.error(`_toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars}): stat → ${stat}`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });
	var text;
	var start = start_chars;
	var end = end_chars;
	logger.error(`_toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars}): end → ${end}`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });

	var startPoint = cm.getCursor('start');
	logger.error(`_toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars}): startPoint → ${startPoint}`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });
	var endPoint = cm.getCursor('end');
	logger.error(`_toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars}): endPoint → ${endPoint}`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });
	logger.debug(`_toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars}): endPoint.ch → ${endPoint.ch}`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });

	if(stat[type]) {
		text = cm.getLine(startPoint.line);
		start = text.slice(0, startPoint.ch);
		end = text.slice(startPoint.ch);
		if(type == "bold") {
			start = start.replace(/(\*\*|__)(?![\s\S]*(\*\*|__))/, "");
			end = end.replace(/(\*\*|__)/, "");
		} else if(type == "italic") {
			start = start.replace(/(\*|_)(?![\s\S]*(\*|_))/, "");
			end = end.replace(/(\*|_)/, "");
		} else if(type == "strikethrough") {
			start = start.replace(/(\*\*|~~)(?![\s\S]*(\*\*|~~))/, "");
			end = end.replace(/(\*\*|~~)/, "");
		}
		cm.replaceRange(start + end, {
			line: startPoint.line,
			ch: 0
		}, {
			line: startPoint.line,
			ch: 99999999999999
		});

		if(type == "bold" || type == "strikethrough") {
			startPoint.ch -= 2;
			if(startPoint !== endPoint) {
				endPoint.ch -= 2;
			}
		} else if(type == "italic") {
			startPoint.ch -= 1;
			if(startPoint !== endPoint) {
				endPoint.ch -= 1;
			}
		}
	} else {
		text = cm.getSelection();
		if(type == "bold") {
			text = text.split("**").join("");
			text = text.split("__").join("");
		} else if(type == "italic") {
			text = text.split("*").join("");
			text = text.split("_").join("");
		} else if(type == "strikethrough") {
			text = text.split("~~").join("");
		}
		cm.replaceSelection(start + text + end);
		logger.error(`_toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars}): text → ${text}`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });
		logger.debug(`_toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars}): text → ${text}`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });

		startPoint.ch += start_chars.length;
		endPoint.ch = startPoint.ch + text.length;
	}

	cm.setSelection(startPoint, endPoint);
	cm.focus();
	logger.warn(`<<< Exiting _toggleBlock(editor=${editor},type=${type},start_chars=${start_chars},end_chars=${end_chars})`, { "event.action": "_toggleBlock", "logrite.editor": editor, "logrite.type": type, "logrite.start_chars": start_chars, "logrite.end_chars": end_chars });
}


/* The right word count in respect for CJK. */
function wordCount(data) {
	logger.error(`>>> Entering wordCount(data=${data})`, { "event.action": "wordCount", "logrite.data": data });
	var pattern = /[a-zA-Z0-9_\u0392-\u03c9]+|[\u4E00-\u9FFF\u3400-\u4dbf\uf900-\ufaff\u3040-\u309f\uac00-\ud7af]+/g;
	var m = data.match(pattern);
	var count = 0;
	logger.error(`wordCount(data=${data}): count → ${count}`, { "event.action": "wordCount", "logrite.data": data });
	if(m === null) return count;
	for(var i = 0; i < m.length; i++) {
		if(m[i].charCodeAt(0) >= 0x4E00) {
			count += m[i].length;
		} else {
			count += 1;
		}
	}
	logger.error(`wordCount(data=${data}): m → ${m}`, { "event.action": "wordCount", "logrite.data": data });
	return count;
}


var toolbarBuiltInButtons = {
	"bold": {
		name: "bold",
		action: toggleBold,
		className: "fa fa-bold",
		title: "Bold (Ctrl+B)",
	},
	"italic": {
		name: "italic",
		action: toggleItalic,
		className: "fa fa-italic",
		title: "Italic (Ctrl+I)",
	},
	"strikethrough": {
		name: "strikethrough",
		action: toggleStrikethrough,
		className: "fa fa-strikethrough",
		title: "Strikethrough",
	},
	"heading": {
		name: "heading",
		action: toggleHeadingSmaller,
		className: "fa fa-header",
		title: "Heading (Ctrl+H)",
	},
	"heading-smaller": {
		name: "heading-smaller",
		action: toggleHeadingSmaller,
		className: "fa fa-header fa-header-x fa-header-smaller",
		title: "Smaller Heading (Ctrl+H)",
	},
	"heading-bigger": {
		name: "heading-bigger",
		action: toggleHeadingBigger,
		className: "fa fa-header fa-header-x fa-header-bigger",
		title: "Bigger Heading (Shift+Ctrl+H)",
	},
	"heading-1": {
		name: "heading-1",
		action: toggleHeading1,
		className: "fa fa-header fa-header-x fa-header-1",
		title: "Big Heading",
	},
	"heading-2": {
		name: "heading-2",
		action: toggleHeading2,
		className: "fa fa-header fa-header-x fa-header-2",
		title: "Medium Heading",
	},
	"heading-3": {
		name: "heading-3",
		action: toggleHeading3,
		className: "fa fa-header fa-header-x fa-header-3",
		title: "Small Heading",
	},
	"code": {
		name: "code",
		action: toggleCodeBlock,
		className: "fa fa-code",
		title: "Code (Ctrl+Alt+C)",
	},
	"quote": {
		name: "quote",
		action: toggleBlockquote,
		className: "fa fa-quote-left",
		title: "Quote (Ctrl+')",
	},
	"unordered-list": {
		name: "unordered-list",
		action: toggleUnorderedList,
		className: "fa fa-list-ul",
		title: "Generic List (Ctrl+L)",
	},
	"ordered-list": {
		name: "ordered-list",
		action: toggleOrderedList,
		className: "fa fa-list-ol",
		title: "Numbered List (Ctrl+Alt+L)",
	},
	"link": {
		name: "link",
		action: drawLink,
		className: "fa fa-link",
		title: "Create Link (Ctrl+K)",
	},
	"image": {
		name: "image",
		action: drawImage,
		className: "fa fa-picture-o",
		title: "Insert Image (Ctrl+Alt+I)",
	},
	"horizontal-rule": {
		name: "horizontal-rule",
		action: drawHorizontalRule,
		className: "fa fa-minus",
		title: "Insert Horizontal Line",
	},
	"preview": {
		name: "preview",
		action: togglePreview,
		className: "fa fa-eye",
		title: "Toggle Preview (Ctrl+P)",
	},
	"side-by-side": {
		name: "side-by-side",
		action: toggleSideBySide,
		className: "fa fa-columns",
		title: "Toggle Side by Side (F9)",
	},
	"fullscreen": {
		name: "fullscreen",
		action: toggleFullScreen,
		className: "fa fa-arrows-alt",
		title: "Toggle Fullscreen (F11)",
	},
	"guide": {
		name: "guide",
		action: "http://nextstepwebs.github.io/simplemde-markdown-editor/markdown-guide",
		className: "fa fa-question-circle",
		title: "Markdown Guide",
	}
};

var toolbar = ["bold", "italic", "heading", "|", "quote", "unordered-list", "ordered-list", "|", "link", "image", "|", "preview", "side-by-side", "fullscreen", "guide"];

/**
 * Interface of SimpleMDE.
 */
function SimpleMDE(options) {
	logger.error(`>>> Entering SimpleMDE(options=${options})`, { "event.action": "SimpleMDE", "logrite.options": options });
	options = options || {};

	// Used later to refer to it's parent
	options.parent = this;
	logger.debug(`SimpleMDE(options=${options}): options.parent → ${options.parent}`, { "event.action": "SimpleMDE", "logrite.options": options });

	// Find the textarea to use
	if(options.element) {
		this.element = options.element;
	} else if(options.element === null) {
		logger.debug(`SimpleMDE(options=${options}): this.element → ${this.element}`, { "event.action": "SimpleMDE", "logrite.options": options });
		// This means that the element option was specified, but no element was found
		console.log("SimpleMDE: Error. No element was found.");
		return;
	}

	// Handle toolbar and status bar
	if(options.toolbar !== false)
		options.toolbar = options.toolbar || SimpleMDE.toolbar;

	logger.debug(`SimpleMDE(options=${options}): options.toolbar → ${options.toolbar}`, { "event.action": "SimpleMDE", "logrite.options": options });
	if(!options.hasOwnProperty('status')) {
		options.status = ['autosave', 'lines', 'words', 'cursor'];
		logger.debug(`SimpleMDE(options=${options}): options.status → ${options.status}`, { "event.action": "SimpleMDE", "logrite.options": options });
	}

	// Add default preview rendering function
	if(!options.previewRender) {
		options.previewRender = function(plainText) {
			logger.error(`>>> Entering previewRender(plainText=${plainText})`, { "event.action": "previewRender", "logrite.plainText": plainText });
			// Note: 'this' refers to the options object
			logger.warn(`<<< Exiting previewRender(plainText=${plainText})`, { "event.action": "previewRender", "logrite.plainText": plainText });
			return this.parent.markdown(plainText);
		}
	}

	// Set default options for parsing config
	options.parsingConfig = options.parsingConfig || {};
	logger.debug(`SimpleMDE(options=${options}): options.parsingConfig → ${options.parsingConfig}`, { "event.action": "SimpleMDE", "logrite.options": options });

	// Update this options
	this.options = options;

	logger.debug(`SimpleMDE(options=${options}): options → ${options}`, { "event.action": "SimpleMDE", "logrite.options": options });
	// Auto render
	this.render();
	logger.debug(`SimpleMDE(options=${options}): this.options → ${this.options}`, { "event.action": "SimpleMDE", "logrite.options": options });

	// The codemirror component is only available after rendering
	// so, the setter for the initialValue can only run after
	// the element has been rendered
	if(options.initialValue) {
		this.value(options.initialValue);
	}
	logger.warn(`<<< Exiting SimpleMDE(options=${options})`, { "event.action": "SimpleMDE", "logrite.options": options });
}

/**
 * Default toolbar elements.
 */
SimpleMDE.toolbar = toolbar;

/**
 * Default markdown render.
 */
SimpleMDE.prototype.markdown = function(text) {
	if(window.marked) {
		// Update options
		if(this.options && this.options.singleLineBreaks !== false) {
			marked.setOptions({
				breaks: true
			});
		}

		return marked(text);
	}
};

/**
 * Render editor to the given element.
 */
SimpleMDE.prototype.render = function(el) {
	if(!el) {
		el = this.element || document.getElementsByTagName('textarea')[0];
	}

	if(this._rendered && this._rendered === el) {
		// Already rendered.
		return;
	}

	this.element = el;
	var options = this.options;

	var self = this;
	var keyMaps = {};

	for(var key in shortcuts) {
		(function(key) {
			keyMaps[fixShortcut(key)] = function(cm) {
				shortcuts[key](self);
			};
		})(key);
	}

	keyMaps["Enter"] = "newlineAndIndentContinueMarkdownList";
	keyMaps["Tab"] = "tabAndIndentMarkdownList";
	keyMaps["Shift-Tab"] = "shiftTabAndUnindentMarkdownList";
	keyMaps["F11"] = function(cm) {
		toggleFullScreen(self);
	};
	keyMaps["F9"] = function(cm) {
		toggleSideBySide(self);
	};
	keyMaps["Esc"] = function(cm) {
		if(cm.getOption("fullScreen")) toggleFullScreen(self);
	};

	var mode, backdrop;
	if(options.spellChecker !== false) {
		mode = "spell-checker";
		backdrop = options.parsingConfig;
		backdrop.name = "gfm";
		backdrop.gitHubSpice = false;
	} else {
		mode = options.parsingConfig;
		mode.name = "gfm";
		mode.gitHubSpice = false;
	}

	this.codemirror = CodeMirror.fromTextArea(el, {
		mode: mode,
		backdrop: backdrop,
		theme: "paper",
		tabSize: (options.tabSize != undefined) ? options.tabSize : 2,
		indentUnit: (options.tabSize != undefined) ? options.tabSize : 2,
		indentWithTabs: (options.indentWithTabs === false) ? false : true,
		lineNumbers: false,
		autofocus: (options.autofocus === true) ? true : false,
		extraKeys: keyMaps,
		lineWrapping: (options.lineWrapping === false) ? false : true
	});

	if(options.toolbar !== false) {
		this.createToolbar();
	}
	if(options.status !== false) {
		this.createStatusbar();
	}
	if(options.autosave != undefined && options.autosave.enabled === true) {
		this.autosave();
	}

	this.createSideBySide();

	this._rendered = this.element;
};

SimpleMDE.prototype.autosave = function() {
	logger.error('>>> Entering autosave()', { "event.action": "autosave" });
	var content = this.value();
	var simplemde = this;
	logger.error(`autosave(): content → ${content}`, { "event.action": "autosave" });

	if(this.options.autosave.unique_id == undefined || this.options.autosave.unique_id == "") {
		console.log("SimpleMDE: You must set a unique_id to use the autosave feature");
		return;
	}

	if(simplemde.element.form != null && simplemde.element.form != undefined) {
		simplemde.element.form.addEventListener("submit", function() {
			localStorage.setItem(simplemde.options.autosave.unique_id, "");
		});
	}

	if(this.options.autosave.loaded !== true) {
		if(typeof localStorage.getItem(this.options.autosave.unique_id) == "string" && localStorage.getItem(this.options.autosave.unique_id) != "")
			this.codemirror.setValue(localStorage.getItem(this.options.autosave.unique_id));

		this.options.autosave.loaded = true;
	}

	if(localStorage) {
		localStorage.setItem(this.options.autosave.unique_id, content);
	}

	var el = document.getElementById("autosaved");
	logger.error(`autosave(): el → ${el}`, { "event.action": "autosave" });
	logger.debug(`autosave(): el.innerHTML → ${el.innerHTML}`, { "event.action": "autosave" });
	if(el != null && el != undefined && el != "") {
		var d = new Date();
		logger.error(`autosave(): d → ${d}`, { "event.action": "autosave" });
		var hh = d.getHours();
		logger.error(`autosave(): hh → ${hh}`, { "event.action": "autosave" });
		var m = d.getMinutes();
		logger.error(`autosave(): m → ${m}`, { "event.action": "autosave" });
		var dd = "am";
		logger.error(`autosave(): dd → ${dd}`, { "event.action": "autosave" });
		var h = hh;
		if(h >= 12) {
			h = hh - 12;
			dd = "pm";
		}
		if(h == 0) {
			h = 12;
		}
		m = m < 10 ? "0" + m : m;

		el.innerHTML = "Autosaved: " + h + ":" + m + " " + dd;
	}

	setTimeout(function() {
		simplemde.autosave();
	}, this.options.autosave.delay || 10000);
	logger.warn('<<< Exiting autosave()', { "event.action": "autosave" });
};

SimpleMDE.prototype.createSideBySide = function() {
	logger.error('>>> Entering createSideBySide()', { "event.action": "createSideBySide" });
	var cm = this.codemirror;
	var wrapper = cm.getWrapperElement();
	logger.error(`createSideBySide(): wrapper → ${wrapper}`, { "event.action": "createSideBySide" });
	var preview = wrapper.nextSibling;
	logger.error(`createSideBySide(): preview → ${preview}`, { "event.action": "createSideBySide" });
	logger.debug(`createSideBySide(): preview.className → ${preview.className}`, { "event.action": "createSideBySide" });
	logger.debug(`createSideBySide(): preview.scrollTop → ${preview.scrollTop}`, { "event.action": "createSideBySide" });

	if(!preview || !/editor-preview-side/.test(preview.className)) {
		preview = document.createElement('div');
		preview.className = 'editor-preview-side';
		wrapper.parentNode.insertBefore(preview, wrapper.nextSibling);
	}

	// Syncs scroll  editor -> preview
	var cScroll = false;
	var pScroll = false;
	cm.on('scroll', function(v) {
		if(cScroll) {
			cScroll = false;
			return;
		};
		pScroll = true;
		height = v.getScrollInfo().height - v.getScrollInfo().clientHeight;
		ratio = parseFloat(v.getScrollInfo().top) / height;
		move = (preview.scrollHeight - preview.clientHeight) * ratio;
		preview.scrollTop = move;
	});

	// Syncs scroll  preview -> editor
	preview.onscroll = function(v) {
		logger.error(`>>> Entering onscroll(v=${v})`, { "event.action": "onscroll", "logrite.v": v });
		if(pScroll) {
			pScroll = false;
			return;
		};
		cScroll = true;
		height = preview.scrollHeight - preview.clientHeight;
		ratio = parseFloat(preview.scrollTop) / height;
		move = (cm.getScrollInfo().height - cm.getScrollInfo().clientHeight) * ratio;
		cm.scrollTo(0, move);
		logger.warn(`<<< Exiting onscroll(v=${v})`, { "event.action": "onscroll", "logrite.v": v });
	};
	logger.warn('<<< Exiting createSideBySide()', { "event.action": "createSideBySide" });
	return true;
}

SimpleMDE.prototype.createToolbar = function(items) {
	logger.error(`>>> Entering createToolbar(items=${items})`, { "event.action": "createToolbar", "logrite.items": items });
	items = items || this.options.toolbar;

	if(!items || items.length === 0) {
		return;
	}

	for(var i = 0; i < items.length; i++) {
		if(toolbarBuiltInButtons[items[i]] != undefined) {
			items[i] = toolbarBuiltInButtons[items[i]];
		}
	}

	var bar = document.createElement('div');
	logger.error(`createToolbar(items=${items}): bar → ${bar}`, { "event.action": "createToolbar", "logrite.items": items });
	bar.className = 'editor-toolbar';
	logger.debug(`createToolbar(items=${items}): bar.className → ${bar.className}`, { "event.action": "createToolbar", "logrite.items": items });

	var self = this;

	var el;
	var toolbar_data = {};
	logger.error(`createToolbar(items=${items}): toolbar_data → ${toolbar_data}`, { "event.action": "createToolbar", "logrite.items": items });
	self.toolbar = items;

	logger.debug(`createToolbar(items=${items}): self.toolbar → ${self.toolbar}`, { "event.action": "createToolbar", "logrite.items": items });
	for(var i = 0; i < items.length; i++) {
		if(items[i].name == "guide" && self.options.toolbarGuideIcon === false)
			continue;

		(function(item) {
			var el;
			if(item === '|') {
				el = createSep();
			} else {
				el = createIcon(item, self.options.toolbarTips);
			}

			// bind events, special for info
			if(item.action) {
				if(typeof item.action === 'function') {
					el.onclick = function(e) {
						logger.error(`>>> Entering onclick(e=${e})`, { "event.action": "onclick", "logrite.e": e });
						item.action(self);
						logger.warn(`<<< Exiting onclick(e=${e})`, { "event.action": "onclick", "logrite.e": e });
					};
				} else if(typeof item.action === 'string') {
					el.href = item.action;
					el.target = '_blank';
				}
			}
			toolbar_data[item.name || item] = el;
			bar.appendChild(el);
		})(items[i]);
	}

	self.toolbarElements = toolbar_data;
	logger.debug(`createToolbar(items=${items}): self.toolbarElements → ${self.toolbarElements}`, { "event.action": "createToolbar", "logrite.items": items });

	var cm = this.codemirror;
	cm.on('cursorActivity', function() {
		var stat = getState(cm);

		for(var key in toolbar_data) {
			(function(key) {
				var el = toolbar_data[key];
				if(stat[key]) {
					logger.debug(`createToolbar(items=${items}): el.href → ${el.href}`, { "event.action": "createToolbar", "logrite.items": items });
					logger.debug(`createToolbar(items=${items}): el.target → ${el.target}`, { "event.action": "createToolbar", "logrite.items": items });
					el.className += ' active';
				} else if(key != "fullscreen" && key != "side-by-side") {
					el.className = el.className.replace(/\s*active\s*/g, '');
				}
			})(key);
		}
	});

	var cmWrapper = cm.getWrapperElement();
	logger.error(`createToolbar(items=${items}): cmWrapper → ${cmWrapper}`, { "event.action": "createToolbar", "logrite.items": items });
	cmWrapper.parentNode.insertBefore(bar, cmWrapper);
	return bar;
};

SimpleMDE.prototype.createStatusbar = function(status) {
	logger.error(`>>> Entering createStatusbar(status=${status})`, { "event.action": "createStatusbar", "logrite.status": status });
	status = status || this.options.status;
	options = this.options;
	logger.error(`createStatusbar(status=${status}): status → ${status}`, { "event.action": "createStatusbar", "logrite.status": status });

	if(!status || status.length === 0) return;

	var bar = document.createElement('div');
	logger.error(`createStatusbar(status=${status}): bar → ${bar}`, { "event.action": "createStatusbar", "logrite.status": status });
	bar.className = 'editor-statusbar';
	logger.debug(`createStatusbar(status=${status}): bar.className → ${bar.className}`, { "event.action": "createStatusbar", "logrite.status": status });

	var pos, cm = this.codemirror;
	for(var i = 0; i < status.length; i++) {
		(function(name) {
			var el = document.createElement('span');
			logger.debug(`createStatusbar(status=${status}): el.innerHTML → ${el.innerHTML}`, { "event.action": "createStatusbar", "logrite.status": status });
			el.className = name;
			logger.debug(`createStatusbar(status=${status}): el.className → ${el.className}`, { "event.action": "createStatusbar", "logrite.status": status });
			if(name === 'words') {
				el.innerHTML = '0';
				cm.on('update', function() {
					el.innerHTML = wordCount(cm.getValue());
				});
			} else if(name === 'lines') {
				el.innerHTML = '0';
				cm.on('update', function() {
					el.innerHTML = cm.lineCount();
				});
			} else if(name === 'cursor') {
				el.innerHTML = '0:0';
				cm.on('cursorActivity', function() {
					pos = cm.getCursor();
					logger.debug(`createStatusbar(status=${status}): pos → ${pos}`, { "event.action": "createStatusbar", "logrite.status": status });
					el.innerHTML = pos.line + ':' + pos.ch;
				});
			} else if(name === 'autosave') {
				if(options.autosave != undefined && options.autosave.enabled === true) {
					el.setAttribute("id", "autosaved");
				}
			}
			bar.appendChild(el);
		})(status[i]);
	}

	var cmWrapper = this.codemirror.getWrapperElement();
	logger.error(`createStatusbar(status=${status}): cmWrapper → ${cmWrapper}`, { "event.action": "createStatusbar", "logrite.status": status });
	cmWrapper.parentNode.insertBefore(bar, cmWrapper.nextSibling);
	return bar;
};

/**
 * Get or set the text content.
 */
SimpleMDE.prototype.value = function(val) {
	logger.error(`>>> Entering value(val=${val})`, { "event.action": "value", "logrite.val": val });
	if(val === undefined) {
		return this.codemirror.getValue();
	} else {
		this.codemirror.getDoc().setValue(val);
		logger.warn(`<<< Exiting value(val=${val})`, { "event.action": "value", "logrite.val": val });
		return this;
	}
};


/**
 * Bind static methods for exports.
 */
SimpleMDE.toggleBold = toggleBold;
SimpleMDE.toggleItalic = toggleItalic;
SimpleMDE.toggleStrikethrough = toggleStrikethrough;
SimpleMDE.toggleBlockquote = toggleBlockquote;
SimpleMDE.toggleHeadingSmaller = toggleHeadingSmaller;
SimpleMDE.toggleHeadingBigger = toggleHeadingBigger;
SimpleMDE.toggleHeading1 = toggleHeading1;
SimpleMDE.toggleHeading2 = toggleHeading2;
SimpleMDE.toggleHeading3 = toggleHeading3;
SimpleMDE.toggleCodeBlock = toggleCodeBlock;
SimpleMDE.toggleUnorderedList = toggleUnorderedList;
SimpleMDE.toggleOrderedList = toggleOrderedList;
SimpleMDE.drawLink = drawLink;
SimpleMDE.drawImage = drawImage;
SimpleMDE.drawHorizontalRule = drawHorizontalRule;
SimpleMDE.undo = undo;
SimpleMDE.redo = redo;
SimpleMDE.togglePreview = togglePreview;
SimpleMDE.toggleSideBySide = toggleSideBySide;
SimpleMDE.toggleFullScreen = toggleFullScreen;

/**
 * Bind instance methods for exports.
 */
SimpleMDE.prototype.toggleBold = function() {
	toggleBold(this);
};
SimpleMDE.prototype.toggleItalic = function() {
	toggleItalic(this);
};
SimpleMDE.prototype.toggleStrikethrough = function() {
	toggleStrikethrough(this);
};
SimpleMDE.prototype.toggleBlockquote = function() {
	toggleBlockquote(this);
};
SimpleMDE.prototype.toggleHeadingSmaller = function() {
	toggleHeadingSmaller(this);
};
SimpleMDE.prototype.toggleHeadingBigger = function() {
	toggleHeadingBigger(this);
};
SimpleMDE.prototype.toggleHeading1 = function() {
	toggleHeading1(this);
};
SimpleMDE.prototype.toggleHeading2 = function() {
	toggleHeading2(this);
};
SimpleMDE.prototype.toggleHeading3 = function() {
	toggleHeading3(this);
};
SimpleMDE.prototype.toggleCodeBlock = function() {
	toggleCodeBlock(this);
};
SimpleMDE.prototype.toggleUnorderedList = function() {
	toggleUnorderedList(this);
};
SimpleMDE.prototype.toggleOrderedList = function() {
	toggleOrderedList(this);
};
SimpleMDE.prototype.drawLink = function() {
	drawLink(this);
};
SimpleMDE.prototype.drawImage = function() {
	drawImage(this);
};
SimpleMDE.prototype.drawHorizontalRule = function() {
	drawHorizontalRule(this);
};
SimpleMDE.prototype.undo = function() {
	undo(this);
};
SimpleMDE.prototype.redo = function() {
	redo(this);
};
SimpleMDE.prototype.togglePreview = function() {
	togglePreview(this);
};
SimpleMDE.prototype.toggleSideBySide = function() {
	toggleSideBySide(this);
};
SimpleMDE.prototype.toggleFullScreen = function() {
	toggleFullScreen(this);
};
