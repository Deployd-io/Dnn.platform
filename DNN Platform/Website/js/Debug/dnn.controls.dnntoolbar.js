var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.dom.positioning.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.controls');

dnn.controls.DNNToolBar = function (ns, behaviorID) {
	logger.error(`>>> Entering DNNToolBar(ns=${ns},behaviorID=${behaviorID})`, { "event.action": "DNNToolBar", "logrite.ns": ns, "logrite.behaviorID": behaviorID });
    this.ctr = document.createElement('div');
    this.ctr.id = ns + '_tb';
    dnn.controls.DNNToolBar.initializeBase(this, [this.ctr]);
    this.initialize(behaviorID);
    //this.ns = ns;

    this.css = null;
    this.cssButton = null;
	logger.debug(`DNNToolBar(ns=${ns},behaviorID=${behaviorID}): this.css → ${this.css}`, { "event.action": "DNNToolBar", "logrite.ns": ns, "logrite.behaviorID": behaviorID });
    this.cssButtonHover = null;
    this.moutDelay = null;
	logger.debug(`DNNToolBar(ns=${ns},behaviorID=${behaviorID}): this.cssButton → ${this.cssButton}`, { "event.action": "DNNToolBar", "logrite.ns": ns, "logrite.behaviorID": behaviorID });

    this.buttons = [];
	logger.debug(`DNNToolBar(ns=${ns},behaviorID=${behaviorID}): this.cssButtonHover → ${this.cssButtonHover}`, { "event.action": "DNNToolBar", "logrite.ns": ns, "logrite.behaviorID": behaviorID });
    this.relatedCtl = null;
    this.ctr.style.position = 'relative';	//allow relative positioning without taking up space
	logger.debug(`DNNToolBar(ns=${ns},behaviorID=${behaviorID}): this.moutDelay → ${this.moutDelay}`, { "event.action": "DNNToolBar", "logrite.ns": ns, "logrite.behaviorID": behaviorID });
    this.ctl = document.createElement('div');
    this.ctr.appendChild(this.ctl);
	logger.debug(`DNNToolBar(ns=${ns},behaviorID=${behaviorID}): this.buttons → ${this.buttons}`, { "event.action": "DNNToolBar", "logrite.ns": ns, "logrite.behaviorID": behaviorID });
    this.ctl.style.display = 'none';
    this.moutDelay = 1000;
	logger.debug(`DNNToolBar(ns=${ns},behaviorID=${behaviorID}): this.relatedCtl → ${this.relatedCtl}`, { "event.action": "DNNToolBar", "logrite.ns": ns, "logrite.behaviorID": behaviorID });
    this._hideDelegate = dnn.createDelegate(this, this.hide);
    this.actionHandler = null;
	logger.debug(`DNNToolBar(ns=${ns},behaviorID=${behaviorID}): this.actionHandler → ${this.actionHandler}`, { "event.action": "DNNToolBar", "logrite.ns": ns, "logrite.behaviorID": behaviorID });
}

dnn.controls.DNNToolBar.prototype =
{

    loadDefinition: function (toolbarID, nsPrefix, relatedCtl, parent, insertBeforeCtl, actionHandler) {
		logger.error(`>>> Entering loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler})`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
        var def = dnn.controls.toolbars[nsPrefix + ':' + toolbarID];
        if (def == null)	//try and load with xhtmlconformance off
            def = dnn.controls.toolbars[nsPrefix + '$' + toolbarID];
        if (def == null)	//try and load global toolbar
            def = dnn.controls.toolbars[toolbarID];

        if (def) {
            this.relatedCtl = relatedCtl;
            this.css = def.css;
			logger.debug(`loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler}): relatedCtl → ${relatedCtl}`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
			logger.debug(`loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler}): this.relatedCtl → ${this.relatedCtl}`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
			logger.debug(`loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler}): this.css → ${this.css}`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
            this.cssButton = def.cssb;
            this.cssButtonHover = def.cssbh;
            this.actionHandler = actionHandler;
            for (var i = 0; i < def.btns.length; i++) {
				logger.debug(`loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler}): this.cssButton → ${this.cssButton}`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
                var btn = def.btns[i];
                this.addButton(btn.key, btn.ca, btn.css, btn.cssh, btn.img, btn.txt, btn.alt, btn.js, btn.url, true);
				logger.debug(`loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler}): this.cssButtonHover → ${this.cssButtonHover}`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
            }

			logger.debug(`loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler}): actionHandler → ${actionHandler}`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
			logger.debug(`loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler}): this.actionHandler → ${this.actionHandler}`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
            if (def.mod)
                this.moutDelay = def.mod;

			logger.debug(`loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler}): this.moutDelay → ${this.moutDelay}`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
            if (insertBeforeCtl) {
                //mini-hack to work around issue with &nbsp; just before ctl which causes "bumping"
                var sibling = dnn.dom.getSibling(insertBeforeCtl, -1);
				logger.error(`loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler}): sibling → ${sibling}`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
                if (sibling != null && sibling.nodeType == 3)
                    insertBeforeCtl = sibling;
                parent.insertBefore(this.ctr, insertBeforeCtl);
            }
            else
                parent.appendChild(this.ctr);
		logger.warn(`<<< Exiting loadDefinition(toolbarID=${toolbarID},nsPrefix=${nsPrefix},relatedCtl=${relatedCtl},parent=${parent},insertBeforeCtl=${insertBeforeCtl},actionHandler=${actionHandler})`, { "event.action": "loadDefinition", "logrite.toolbarID": toolbarID, "logrite.nsPrefix": nsPrefix, "logrite.relatedCtl": relatedCtl, "logrite.parent": parent, "logrite.insertBeforeCtl": insertBeforeCtl, "logrite.actionHandler": actionHandler });
        }

    },

    addButton: function (key, clickAct, css, hoverCss, img, text, toolTip, js, url, visible) {
		logger.error(`>>> Entering addButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible})`, { "event.action": "addButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible });
        if (key == null)
            key = this.buttons.length;
        if (this.cssButton)
            css = this.cssButton + ' ' + css;
        if (this.cssButtonHover)
            hoverCss = this.cssButtonHover + ' ' + hoverCss;

		logger.error(`addButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible}): hoverCss → ${hoverCss}`, { "event.action": "addButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible });
        this.buttons[key] = new dnn.controls.DNNToolBarButton(key, clickAct, css, hoverCss, img, text, toolTip, js, url, visible, this);
		logger.warn(`<<< Exiting addButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible})`, { "event.action": "addButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible });
    },

    refresh: function () {
		logger.error('>>> Entering refresh()', { "event.action": "refresh" });
        this.ctl.className = this.css;
        for (var key in this.buttons) {
            var btn = this.buttons[key];
            if (typeof btn == 'function')	//safeguard against other js frameworks
                continue;

            if (btn.ctl == null) {
                btn.render();
                this.ctl.appendChild(btn.ctl);
            }
            btn.ctl.style.display = (btn.visible ? '' : 'none');
			logger.warn('<<< Exiting refresh()', { "event.action": "refresh" });
        }
    },

    show: function (hide) {
		logger.error(`>>> Entering show(hide=${hide})`, { "event.action": "show", "logrite.hide": hide });
		const __log_start = Date.now();
        dnn.cancelDelay(this.ns + 'mout');
        if (this.ctl.style.display != '') {
            this.refresh();
			logger.error(`show(hide)=${hide}: database operation took ${Date.now() - __log_start} ms`, { "event.action": "show", "event.category": "database", "logrite.hide": hide });
            this.ctl.style.display = '';
            this.ctr.className = this.ctr.className + " visible";
        }
        if (hide)
            this.beginHide();
    },

    beginHide: function () {
		logger.error('>>> Entering beginHide()', { "event.action": "beginHide" });
        if (this.moutDelay > 0)
            dnn.doDelay(this.ns + 'mout', this.moutDelay, this._hideDelegate);

    },

    hide: function () {
		logger.error('>>> Entering hide()', { "event.action": "hide" });
        this.ctl.style.display = 'none';
        this.ctr.className = this.ctr.className.replace(" visible", "");
		logger.warn('<<< Exiting hide()', { "event.action": "hide" });
    },

    buttonClick: function (e) {
		logger.error(`>>> Entering buttonClick(e=${e})`, { "event.action": "buttonClick", "logrite.e": e });
        var tb = this.tb;
        var arg = new dnn.controls.DNNToolBarEventArgs(this);
		logger.error(`buttonClick(e=${e}): arg → ${arg}`, { "event.action": "buttonClick", "logrite.e": e });
        tb.invoke_handler('click', arg);
        if (arg.get_cancel())
            return;
        this.click();

        if (e && e.preventDefault) {
            e.stopPropagation(); // DOM style (return false doesn't always work in FF)
            e.preventDefault();
        }
        else {
            window.event.cancelBubble = true;//IE stopPropagation
			logger.warn(`<<< Exiting buttonClick(e=${e})`, { "event.action": "buttonClick", "logrite.e": e });
        }
        return false;
    },

    buttonMouseOver: function (evt) {
		logger.error(`>>> Entering buttonMouseOver(evt=${evt})`, { "event.action": "buttonMouseOver", "logrite.evt": evt });
        this.tb.show(false);
        this.hover();
		logger.warn(`<<< Exiting buttonMouseOver(evt=${evt})`, { "event.action": "buttonMouseOver", "logrite.evt": evt });
    },

    buttonMouseOut: function (evt) {
		logger.error(`>>> Entering buttonMouseOut(evt=${evt})`, { "event.action": "buttonMouseOut", "logrite.evt": evt });
        this.tb.beginHide();
        this.unhover();
		logger.warn(`<<< Exiting buttonMouseOut(evt=${evt})`, { "event.action": "buttonMouseOut", "logrite.evt": evt });
    },

    dispose: function () {
		logger.error('>>> Entering dispose()', { "event.action": "dispose" });
        this._hideDelegate = null;
        this.actionHandler = null;
        dnn.controls.DNNToolBar.callBaseMethod(this, 'dispose');
		logger.debug(`dispose(): this._hideDelegate → ${this._hideDelegate}`, { "event.action": "dispose" });
    }


}
dnn.controls.DNNToolBar.registerClass('dnn.controls.DNNToolBar', dnn.controls.control);


dnn.controls.DNNToolBarButton = function (key, clickAct, css, hoverCss, img, text, toolTip, js, url, visible, oToolbar) {
	logger.error(`>>> Entering DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar})`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
    this.ctl = null;
    this.key = key;
    this.clickAction = clickAct;
	logger.debug(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): this.ctl → ${this.ctl}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
	logger.warn(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): key → ${key}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
	logger.debug(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): this.key → ${this.key}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
	logger.debug(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): css → ${css}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
	logger.debug(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): this.clickAction → ${this.clickAction}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
	logger.debug(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): hoverCss → ${hoverCss}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
	logger.debug(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): img → ${img}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
	logger.debug(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): this.css → ${this.css}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
	logger.debug(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): this.visible → ${this.visible}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
    this.tb = oToolbar;
	logger.debug(`DNNToolBarButton(key=${key},clickAct=${clickAct},css=${css},hoverCss=${hoverCss},img=${img},text=${text},toolTip=${toolTip},js=${js},url=${url},visible=${visible},oToolbar=${oToolbar}): this.tb → ${this.tb}`, { "event.action": "DNNToolBarButton", "logrite.key": key, "logrite.clickAct": clickAct, "logrite.css": css, "logrite.hoverCss": hoverCss, "logrite.img": img, "logrite.text": text, "logrite.toolTip": toolTip, "logrite.js": js, "logrite.url": url, "logrite.visible": visible, "logrite.oToolbar": oToolbar });
    this.css = css;
    this.hoverCss = hoverCss;
    this.img = img;
    this.tooltip = toolTip;
    this.txt = text;
    this.js = js;
    this.url = url;
    this.visible = visible;
}

dnn.controls.DNNToolBarButton.prototype =
{

    render: function () {
		logger.error('>>> Entering render()', { "event.action": "render" });
        if (!this.ctl) {
            //this.ctl = document.createElement('span');
            var bar = this.tb;
            this.ctl = bar.createChildControl('div', this.key, '');
            this.ctl.className = this.css;
            if (this.tooltip)
                this.ctl.title = this.tooltip;
            //this.ctl.key = this.key;
            if (this.img) {
                var img = document.createElement('img');
				logger.error(`render(): img → ${img}`, { "event.action": "render" });
                img.src = this.img;
				logger.debug(`render(): img.src → ${img.src}`, { "event.action": "render" });
                this.ctl.appendChild(img);
            }
            if (this.txt) {
                var span = document.createElement('span');
				logger.error(`render(): span → ${span}`, { "event.action": "render" });
                span.innerHTML = this.txt;
				logger.debug(`render(): span.innerHTML → ${span.innerHTML}`, { "event.action": "render" });
                this.ctl.appendChild(span);
            }


            /*
            dnn.dom.addSafeHandler(this.ctl, 'onmouseover', this, 'mouseOver');
            dnn.dom.addSafeHandler(this.ctl, 'onmouseout', this, 'mouseOut');
            if (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer))  //ie has issue if contenteditable looses focus to a span (img are ok).  this works for both
                dnn.dom.addSafeHandler(this.ctl, 'onmousedown', this, 'click');
            else
                dnn.dom.addSafeHandler(this.ctl, 'onclick', this, 'click');
            */
            var clickEvent = 'mousedown';

			logger.error(`render(): clickEvent → ${clickEvent}`, { "event.action": "render" });
            bar.addHandlers(this.ctl, bar.getDynamicEventObject(clickEvent, bar.buttonClick), this);
			logger.warn('<<< Exiting render()', { "event.action": "render" });
            bar.addHandlers(this.ctl, {
                'mouseover': bar.buttonMouseOver,
                'mouseout': bar.buttonMouseOut
            }, this);
        }
    },

    hover: function () {
		logger.error('>>> Entering hover()', { "event.action": "hover" });
        if (this.hoverCss)
            this.ctl.className = this.css + ' ' + this.hoverCss;
			logger.warn('<<< Exiting hover()', { "event.action": "hover" });
    },

    unhover: function () {
		logger.error('>>> Entering unhover()', { "event.action": "unhover" });
        if (this.hoverCss)
            this.ctl.className = this.css;
			logger.warn('<<< Exiting unhover()', { "event.action": "unhover" });
    },

    click: function () {
		logger.error('>>> Entering click()', { "event.action": "click" });
        if (this.clickAction == 'js')
            eval(this.js);
        else if (this.clickAction == 'navigate')
            dnn.dom.navigate(this.url);
        else if (this.tb.actionHandler != null)
            this.tb.actionHandler(this, this.tb.relatedCtl);

    },
    /*
    mouseOver: function () 
    {
        this.tb.show(false);
        if (this.hoverCss)
            this.ctl.className = this.css + ' ' + this.hoverCss;
    },
    
    mouseOut: function () 
    {
        this.tb.beginHide();
        if (this.hoverCss)
            this.ctl.className = this.css;
    },*/

    getVal: function (sVal, sDef) {
		logger.error(`>>> Entering getVal(sVal=${sVal},sDef=${sDef})`, { "event.action": "getVal", "logrite.sVal": sVal, "logrite.sDef": sDef });
		logger.warn(`<<< Exiting getVal(sVal=${sVal},sDef=${sDef})`, { "event.action": "getVal", "logrite.sVal": sVal, "logrite.sDef": sDef });
        return (sVal ? sVal : sDef);
    }

}
dnn.controls.DNNToolBarButton.registerClass('dnn.controls.DNNToolBarButton');

