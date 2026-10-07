var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjaxDebug.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.dom.positioning.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.motion.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.controls');

dnn.extend(dnn.controls, {
    initMenu: function(ctl)
    {
        if (ctl && dnn.controls.controls[ctl.id] == null)
        {
            var menu = new dnn.controls.DNNMenu(ctl);
            menu.initialize();
            return menu;
        }
		logger.warn(`<<< Exiting initMenu(ctl=${ctl})`, { "event.action": "initMenu", "logrite.ctl": ctl });
    }
});

//------- Constructor -------//
dnn.controls.DNNMenu = function(o)
{
    dnn.controls.DNNMenu.initializeBase(this, [o]);

    //--- Data Properties ---//  
    this.rootNode = null;
    this.nodes = [];
    this._loadNodes();

    //--- Appearance Properties ---//
    this.mbcss = this.getProp('mbcss', '');
    this.mcss = this.getProp('mcss', '');

    this.css = this.getProp('css', '');
    this.cssChild = this.getProp('csschild', '');
    this.cssHover = this.getProp('csshover', '');
    this.cssSel = this.getProp('csssel', '');
    this.cssIcon = this.getProp('cssicon', '');

    this.sysImgPath = this.getProp('sysimgpath', 'images/');
    this.imagePaths = this.getProp('imagepaths', '').split(',');
    this.imageList = this.getProp('imagelist', '').split(',');
    for (var i = 0; i < this.imageList.length; i++)
    {
        var index = this.imageList[i].indexOf(']');
        if (index > -1)
            this.imageList[i] = this.imagePaths[this.imageList[i].substring(1, index)] + this.imageList[i].substring(index + 1);
    }
    this.urlList = this.getProp('urllist', '').split(',');

    this.workImg = this.getProp('workimg', 'dnnanim.gif');

    this.rootArrow = this.getProp('rarrowimg', '');
    this.childArrow = this.getProp('carrowimg', '');
    this.target = this.getProp('target', '');
    this.defaultJS = this.getProp('js', '');
    this.postBack = this.getProp('postback', '');
    this.callBack = this.getProp('callback', '');
    this.callBackStatFunc = this.getProp('callbacksf', '');
    if (this.callBackStatFunc.length > 0)
        this.add_handler('callBackStatus', eval(this.callBackStatFunc));

    this.orient = new Number(this.getProp('orient', dnn.controls.orient.horizontal));
    this.suborient = new Number(this.getProp('suborient', dnn.controls.orient.vertical));

    this.openMenus = [];
    this.moutDelay = this.getProp('moutdelay', 500);
    this.minDelay = new Number(this.getProp('mindelay', 250));

    this.renderMode = new Number(this.getProp('rmode', 0));
    //When rendermode is ul then loop each node and apply mouseover and out handlers
    this.useTables = (this.renderMode == dnn.controls.menuRenderMode.normal);
    this.enablePostbackState = (this.getProp('enablepbstate', '0') == '1');
    this.podInProgress = false;
    this.keyboardAccess = (this.getProp('kbaccess', '1') == '1');

    this.hoverMNode = null;
    this.selMNode = null;

    this.animation = new Number(this.getProp('anim', '0')); //0=None
    this.easingType = new Number(this.getProp('easeType', '3')); //3=Expo
    this.easingDir = new Number(this.getProp('easeDir', '1')); ; //1=Out
    this.animationLen = new Number(this.getProp('animLen', '1'));
    this.animationInterval = new Number(this.getProp('animInt', '10'));

    this._attachedHandlers = [];
    this._onsubmitDelegate = null;
    this._hideMenusDelegate = null;
    this._expandNodeDelegate = null;
}

dnn.controls.DNNMenu.prototype =
{

    initialize: function()
    {
		logger.error('>>> Entering initialize()', { "event.action": "initialize" });
        dnn.controls.DNNMenu.callBaseMethod(this, 'initialize');
        if (this.keyboardAccess)
        {
            this._setupKeyHandling();
        }
        this.generateMenuHTML();

        if (this.enablePostbackState)
        {
            this._onsubmitDelegate = Function.createDelegate(this, this._onsubmit);
            dnn.controls.submitComp.add_handler(this._onsubmitDelegate);
        }
        this._hideMenusDelegate = dnn.createDelegate(this, this.hideMenus);
        this._expandNodeDelegate = dnn.createDelegate(this, this.__expandNode);
		logger.warn('<<< Exiting initialize()', { "event.action": "initialize" });
    },

    //--- Generates menu HTML ---//

    generateMenuHTML: function()
    {
		logger.error('>>> Entering generateMenuHTML()', { "event.action": "generateMenuHTML" });
        this.container.className = this.mbcss;
        dnn.dom.disableTextSelect(this.container);

        for (var i = 0; i < this.rootNode.childNodeCount(); i++)
            this.renderNode(this.rootNode.childNodes(i), this.container);

        this.addHandlers(document.body, { "click": this._bodyClick }, this);

        dnn.setVar(this.ns + '_json', '');  //clear out xml for postbacks don't need payload, unless enablePostbackState is set, which will trigger _onSubmit
		logger.warn('<<< Exiting generateMenuHTML()', { "event.action": "generateMenuHTML" });
    },

    renderNode: function(node, ctr)
    {
        var mNode = new dnn.controls.DNNMenuNode(node);
        if (mNode.selected)
            this.selMNode = mNode;

        var menuBuilder = this._getMenuBuilder(mNode, ctr);

        if (menuBuilder.alreadyRendered == false)
        {
            if (this.isNodeVertical(mNode))
                menuBuilder.newRow();
            menuBuilder.newCont();

            if (mNode.lhtml.length > 0)
                menuBuilder.appendChild(this.renderCustomHTML(mNode.lhtml));

            var icon = this.renderIcon(mNode);
            menuBuilder.appendChild(icon); //render icon
            if (this.useTables == false || mNode.level == 0)
                icon.className = 'icn ' + (mNode.cssIcon.length > 0 ? mNode.cssIcon : this.cssIcon);
            else
                menuBuilder.subcont.className = 'icn ' + (mNode.cssIcon.length > 0 ? mNode.cssIcon : this.cssIcon); //assign style to container of icon

            if (mNode.isBreak == false)
                menuBuilder.appendChild(this.renderText(mNode), true); //render text

            menuBuilder.newCell();
            this.renderArrow(mNode, menuBuilder.subcont);

            if (mNode.rhtml.length > 0)
                menuBuilder.appendChild(this.renderCustomHTML(mNode.rhtml));

            if (mNode.toolTip.length > 0)
                menuBuilder.row.title = mNode.toolTip;
        }

        this.assignCss(mNode);

        if (mNode.enabled)
            this.addHandlers(menuBuilder.row, { "click": this._nodeTextClick }, this);

        if (this._attachedHandlers[menuBuilder.container.id] != 'mouseover')
        {
            this._attachedHandlers[menuBuilder.container.id] = 'mouseover';
            this.addHandlers(menuBuilder.container, { "mouseover": this._nodeMOver,
                "mouseout": this._nodeMOut
            }, this);
        }

        if (mNode.hasNodes || mNode.hasPendingNodes)	//if node has children render container and hide if necessary
        {
            var subMenu = this.renderSubMenu(mNode);
            this.container.appendChild(subMenu);

            dnn.dom.positioning.placeOnTop(subMenu, false, this.sysImgPath + 'spacer.gif');  //todo: preload spacer so it works on first try    -don't show first time

            for (var i = 0; i < node.childNodeCount(); i++)	//recursively call child rendering
                this.renderNode(node.childNodes(i), subMenu);
        }
		logger.warn(`<<< Exiting renderNode(node=${node},ctr=${ctr})`, { "event.action": "renderNode", "logrite.node": node, "logrite.ctr": ctr });
    },

    renderCustomHTML: function(sHTML)
    {
		logger.error(`>>> Entering renderCustomHTML(sHTML=${sHTML})`, { "event.action": "renderCustomHTML", "logrite.sHTML": sHTML });
        var ctl = dnn.dom.createElement('span');
        ctl.innerHTML = sHTML;
		logger.error(`renderCustomHTML(sHTML=${sHTML}): ctl → ${ctl}`, { "event.action": "renderCustomHTML", "logrite.sHTML": sHTML });
		logger.warn(`<<< Exiting renderCustomHTML(sHTML=${sHTML})`, { "event.action": "renderCustomHTML", "logrite.sHTML": sHTML });
        return ctl;
    },

    renderIcon: function(mNode)
    {
		logger.error(`>>> Entering renderIcon(mNode=${mNode})`, { "event.action": "renderIcon", "logrite.mNode": mNode });
        var ctl = dnn.dom.createElement('span');
		logger.error(`renderIcon(mNode=${mNode}): ctl → ${ctl}`, { "event.action": "renderIcon", "logrite.mNode": mNode });
        if (mNode.imageIndex > -1 || mNode.image != '')
        {
            var img = this.createChildControl('img', mNode.id, 'icn');
			logger.error(`renderIcon(mNode=${mNode}): img → ${img}`, { "event.action": "renderIcon", "logrite.mNode": mNode });
            img.src = (mNode.image.length > 0 ? mNode.image : this.imageList[mNode.imageIndex]);
            ctl.appendChild(img);
        }

		logger.warn(`<<< Exiting renderIcon(mNode=${mNode})`, { "event.action": "renderIcon", "logrite.mNode": mNode });
        return ctl;
    },

    renderArrow: function(mNode, ctr)
    {
        if (mNode.hasNodes || mNode.hasPendingNodes)
        {
            var imgSrc = (mNode.level == 0 ? this.rootArrow : this.childArrow);
            if (imgSrc.trim().length > 0)
            {
                if (this.useTables && mNode.level > 0)	//do not require tables to need special padding to properly show arrow, place a real image there and have browser space it appropriately
                {
                    var img = dnn.dom.createElement('img');
                    img.src = imgSrc;
                    ctr.appendChild(img);
                }
                else
                {
                    ctr.style.backgroundImage = 'url(' + imgSrc + ')';
                    ctr.style.backgroundRepeat = 'no-repeat';
                    ctr.style.backgroundPosition = 'right';
                }
            }
        }
		logger.warn(`<<< Exiting renderArrow(mNode=${mNode},ctr=${ctr})`, { "event.action": "renderArrow", "logrite.mNode": mNode, "logrite.ctr": ctr });
    },

    renderText: function(mNode)
    {
		logger.error(`>>> Entering renderText(mNode=${mNode})`, { "event.action": "renderText", "logrite.mNode": mNode });
        var ctl = this.createChildControl('span', mNode.id, 't');
        ctl.className = 'txt';
		logger.error(`renderText(mNode=${mNode}): ctl → ${ctl}`, { "event.action": "renderText", "logrite.mNode": mNode });
		logger.debug(`renderText(mNode=${mNode}): ctl.className → ${ctl.className}`, { "event.action": "renderText", "logrite.mNode": mNode });
        ctl.innerHTML = mNode.text;
		logger.debug(`renderText(mNode=${mNode}): ctl.innerHTML → ${ctl.innerHTML}`, { "event.action": "renderText", "logrite.mNode": mNode });
        ctl.style.cursor = 'pointer';
		logger.warn(`<<< Exiting renderText(mNode=${mNode})`, { "event.action": "renderText", "logrite.mNode": mNode });
        return ctl;
    },

    renderSubMenu: function(mNode)
    {
		logger.error(`>>> Entering renderSubMenu(mNode=${mNode})`, { "event.action": "renderSubMenu", "logrite.mNode": mNode });
        var menuBuilder = this._getMenuBuilder(mNode, null);
        /*if (this.suborient == dnn.controls.orient.vertical) // (this.useTables)
        menuBuilder = new dnn.controls.DNNMenuTableBuilder(this, mNode);		
        else
        menuBuilder = new dnn.controls.DNNMenuBuilder(this, mNode);
        */
        var subMenu = menuBuilder.createSubMenu();
		logger.error(`renderSubMenu(mNode=${mNode}): menuBuilder → ${menuBuilder}`, { "event.action": "renderSubMenu", "logrite.mNode": mNode });
		logger.error(`renderSubMenu(mNode=${mNode}): subMenu → ${subMenu}`, { "event.action": "renderSubMenu", "logrite.mNode": mNode });
        subMenu.style.position = 'absolute';
        subMenu.style.display = 'none';
        var css = this.mcss;
        css += ' m m' + mNode.level;
        css += ' mid' + mNode.id;

        subMenu.className = css;
		logger.debug(`renderSubMenu(mNode=${mNode}): subMenu.className → ${subMenu.className}`, { "event.action": "renderSubMenu", "logrite.mNode": mNode });
		logger.warn(`<<< Exiting renderSubMenu(mNode=${mNode})`, { "event.action": "renderSubMenu", "logrite.mNode": mNode });
        return subMenu;
    },

    //---- Methods ---//
    //todo: move
    _getMenuBuilder: function(mNode, ctr)
    {
		logger.error(`>>> Entering _getMenuBuilder(mNode=${mNode},ctr=${ctr})`, { "event.action": "_getMenuBuilder", "logrite.mNode": mNode, "logrite.ctr": ctr });
        var menuBuilder;
        if (ctr)
        {
            if (this.renderMode == dnn.controls.menuRenderMode.normal && mNode.level > 0 && this.isNodeVertical(mNode))
                menuBuilder = new dnn.controls.DNNMenuTableBuilder(this, mNode, ctr);
            else if (this.renderMode == dnn.controls.menuRenderMode.unorderdlist)
                menuBuilder = new dnn.controls.DNNMenuListBuilder(this, mNode, ctr);
            else
                menuBuilder = new dnn.controls.DNNMenuBuilder(this, mNode, ctr);
        }
        else
        {
            if (this.renderMode == dnn.controls.menuRenderMode.normal && this.suborient == dnn.controls.orient.vertical) // (this.useTables)
                menuBuilder = new dnn.controls.DNNMenuTableBuilder(this, mNode);
            else if (this.renderMode == dnn.controls.menuRenderMode.unorderdlist)
                menuBuilder = new dnn.controls.DNNMenuListBuilder(this, mNode);
            else
                menuBuilder = new dnn.controls.DNNMenuBuilder(this, mNode);
        }

		logger.warn(`<<< Exiting _getMenuBuilder(mNode=${mNode},ctr=${ctr})`, { "event.action": "_getMenuBuilder", "logrite.mNode": mNode, "logrite.ctr": ctr });
        return menuBuilder;
    },

    hoverNode: function(mNode)
    {
		logger.error(`>>> Entering hoverNode(mNode=${mNode})`, { "event.action": "hoverNode", "logrite.mNode": mNode });
        if (this.hoverMNode != null)
        {
            this.hoverMNode.hover = false;
            this.assignCss(this.hoverMNode);
        }
        if (mNode != null)
        {
            mNode.hover = true;
			logger.debug(`hoverNode(mNode=${mNode}): mNode.hover → ${mNode.hover}`, { "event.action": "hoverNode", "logrite.mNode": mNode });
            this.assignCss(mNode);
        }
        this.hoverMNode = mNode;
		logger.debug(`hoverNode(mNode=${mNode}): this.hoverMNode → ${this.hoverMNode}`, { "event.action": "hoverNode", "logrite.mNode": mNode });
    },

    __expandNode: function(ctx)
    {
		logger.error(`>>> Entering __expandNode(ctx=${ctx})`, { "event.action": "__expandNode", "logrite.ctx": ctx });
        this.expandNode(ctx, true);
		logger.warn(`<<< Exiting __expandNode(ctx=${ctx})`, { "event.action": "__expandNode", "logrite.ctx": ctx });
    },

    expandNode: function(mNode, force)
    {
		logger.error(`>>> Entering expandNode(mNode=${mNode},force=${force})`, { "event.action": "expandNode", "logrite.mNode": mNode, "logrite.force": force });
        dnn.cancelDelay(this.ns + 'min');
        if (mNode.hasPendingNodes)
        {
            if (this.podInProgress == false)
            {
                this.podInProgress = true;
                this.showWorkImage(mNode, true);
				logger.debug(`expandNode(mNode=${mNode},force=${force}): this.podInProgress → ${this.podInProgress}`, { "event.action": "expandNode", "logrite.mNode": mNode, "logrite.force": force });
                mNode.menu = this; //need to give reference back to self
				logger.debug(`expandNode(mNode=${mNode},force=${force}): mNode.menu → ${mNode.menu}`, { "event.action": "expandNode", "logrite.mNode": mNode, "logrite.force": force });

                if (this.callBack.indexOf('[NODEXML]') > -1)
                    eval(this.callBack.replace('[NODEXML]', dnn.escapeForEval(mNode.node.getXml())));
                else
                    eval(this.callBack.replace('[NODEID]', mNode.id));
            }
        }
        else
        {
            if (this.openMenus.length > 0 && this.openMenus[this.openMenus.length - 1].id == mNode.id)
                return;
            if (this.minDelay == 0 || force)
            {
                this.hideMenus(new dnn.controls.DNNMenuNode(mNode.node.parentNode())); //MinDelay???
                var subMenu = this.getChildControl(mNode.id, 'sub');
				logger.error(`expandNode(mNode=${mNode},force=${force}): subMenu → ${subMenu}`, { "event.action": "expandNode", "logrite.mNode": mNode, "logrite.force": force });
                if (subMenu != null)
                {
                    this.positionMenu(mNode, subMenu);
                    this.showSubMenu(subMenu, true, mNode);
                    this.openMenus[this.openMenus.length] = mNode;
                    mNode.expanded = true;
					logger.debug(`expandNode(mNode=${mNode},force=${force}): mNode.expanded → ${mNode.expanded}`, { "event.action": "expandNode", "logrite.mNode": mNode, "logrite.force": force });
                    mNode.update();
					logger.warn(`<<< Exiting expandNode(mNode=${mNode},force=${force})`, { "event.action": "expandNode", "logrite.mNode": mNode, "logrite.force": force });
                }
            }
            else
                dnn.doDelay(this.ns + 'min', this.minDelay, this._expandNodeDelegate, mNode);
        }
        return true;
    },

    showSubMenu: function(subMenu, show, mNode)
    {
		logger.error(`>>> Entering showSubMenu(subMenu=${subMenu},show=${show},mNode=${mNode})`, { "event.action": "showSubMenu", "logrite.subMenu": subMenu, "logrite.show": show, "logrite.mNode": mNode });
        dnn.dom.positioning.placeOnTop(subMenu, show, this.sysImgPath + 'spacer.gif');
        subMenu.style.clip = 'rect(auto,auto,auto,auto)';
        if (this.animation != 0)  //0=none
        {
            subMenu.style.display = '';
            var dir;
            if (this.isNodeVertical(mNode))
                dir = (show ? dnn.motion.animationDir.Right : dnn.motion.animationDir.Left);
            else
                dir = (show ? dnn.motion.animationDir.Down : dnn.motion.animationDir.Up);
            dnn.dom.animate(subMenu, this.animation, dir, this.easingType, this.easingDir, this.animationLen, this.animationInterval);
			logger.error(`showSubMenu(subMenu=${subMenu},show=${show},mNode=${mNode}): dir → ${dir}`, { "event.action": "showSubMenu", "logrite.subMenu": subMenu, "logrite.show": show, "logrite.mNode": mNode });
        }
        else
            subMenu.style.display = (show ? '' : 'none');

    },

    showWorkImage: function(mNode, show)
    {
		logger.error(`>>> Entering showWorkImage(mNode=${mNode},show=${show})`, { "event.action": "showWorkImage", "logrite.mNode": mNode, "logrite.show": show });
        if (this.workImg != null)
        {
            var icon = this.getChildControl(mNode.id, 'icn');
			logger.error(`showWorkImage(mNode=${mNode},show=${show}): icon → ${icon}`, { "event.action": "showWorkImage", "logrite.mNode": mNode, "logrite.show": show });
			logger.debug(`showWorkImage(mNode=${mNode},show=${show}): icon.src → ${icon.src}`, { "event.action": "showWorkImage", "logrite.mNode": mNode, "logrite.show": show });
            if (icon != null)
            {
                if (show)
                    icon.src = this.sysImgPath + this.workImg;
                else
                    icon.src = (mNode.image.length > 0 ? mNode.image : this.imageList[mNode.imageIndex]);
            }
		logger.warn(`<<< Exiting showWorkImage(mNode=${mNode},show=${show})`, { "event.action": "showWorkImage", "logrite.mNode": mNode, "logrite.show": show });
        }

    },

    isNodeVertical: function(mNode)
    {
		logger.error(`>>> Entering isNodeVertical(mNode=${mNode})`, { "event.action": "isNodeVertical", "logrite.mNode": mNode });
		logger.warn(`<<< Exiting isNodeVertical(mNode=${mNode})`, { "event.action": "isNodeVertical", "logrite.mNode": mNode });
        return ((mNode.level == 0 && this.orient == dnn.controls.orient.vertical) || (mNode.level > 0 && this.suborient == dnn.controls.orient.vertical));
    },

    hideMenus: function(mNode)
    {
		logger.error(`>>> Entering hideMenus(mNode=${mNode})`, { "event.action": "hideMenus", "logrite.mNode": mNode });
        for (var i = this.openMenus.length - 1; i >= 0; i--)
        {
            if (mNode != null && this.openMenus[i].id == mNode.id)
                break;
            this.collapseNode(this.openMenus[i]);
            this.openMenus.length = this.openMenus.length - 1;
			logger.warn(`<<< Exiting hideMenus(mNode=${mNode})`, { "event.action": "hideMenus", "logrite.mNode": mNode });
        }
    },

    collapseNode: function(mNode)
    {
        var subMenu = this.getChildControl(mNode.id, 'sub');
        if (subMenu != null)
        {
            this.positionMenu(mNode, subMenu);
            this.showSubMenu(subMenu, false, mNode);
            mNode.expanded = null;
            mNode.update();
            return true;
        }
		logger.warn(`<<< Exiting collapseNode(mNode=${mNode})`, { "event.action": "collapseNode", "logrite.mNode": mNode });
    },

    positionMenu: function(mNode, menu)
    {
        var oPCtl = this.getChildControl(mNode.id, 'ctr');
        if (dnn.dom.browser.isType(dnn.dom.browser.Safari, dnn.dom.browser.Opera))
        {
            if (oPCtl.tagName == 'TR' && oPCtl.childNodes.length > 0)
                oPCtl = oPCtl.childNodes[oPCtl.childNodes.length - 1]; //fix for Safari... use TD instead of TR
        }

        var oPDims = new dnn.dom.positioning.dims(oPCtl);
        var oMDims = new dnn.dom.positioning.dims(menu);
        var iScrollLeft = dnn.dom.positioning.bodyScrollLeft();
        var iScrollTop = dnn.dom.positioning.bodyScrollTop()
        //Max = ViewPort + Scroll - Menu's relative offset
        var iMaxTop = dnn.dom.positioning.viewPortHeight() + iScrollTop - oPDims.rot;
        var iMaxLeft = dnn.dom.positioning.viewPortWidth() + iScrollLeft - oPDims.rol;
        var iNewTop = oPDims.t;
        var iNewLeft = oPDims.l;
        var iStartTop = oPDims.t;
        var iStartLeft = oPDims.l;

        if (this.isNodeVertical(mNode))
        {
            iNewLeft = oPDims.l + oPDims.w;
            iStartTop = iMaxTop;
        }
        else
        {
            iNewTop = oPDims.t + oPDims.h;
            iStartLeft = iMaxLeft;
        }

        if (iNewTop + oMDims.h >= iMaxTop)	//if menu doesn't fit below...
        {
            if (oPDims.rot + iStartTop - oMDims.h > iScrollTop)	//see if it fits above
                iNewTop = iStartTop - oMDims.h;
            //else						//cause menu to scroll...
        }

        if (iNewLeft + oMDims.w > iMaxLeft)	//if menu doesn't fit to right
        {
            if (oPDims.rol + iStartLeft - oMDims.w > iScrollLeft)  //see if it fits to left
                iNewLeft = iStartLeft - oMDims.w;
        }

        //horizontal submenus
        if (this.suborient == dnn.controls.orient.horizontal && this.isNodeVertical(mNode) == false)
        {
            var oRDims = new dnn.dom.positioning.dims(this.container);
            iNewLeft = oRDims.l;
        }
        menu.style.top = iNewTop + 'px';
        menu.style.left = iNewLeft + 'px';
		logger.warn(`<<< Exiting positionMenu(mNode=${mNode},menu=${menu})`, { "event.action": "positionMenu", "logrite.mNode": mNode, "logrite.menu": menu });
    },

    selectNode: function(mNode)
    {
		logger.error(`>>> Entering selectNode(mNode=${mNode})`, { "event.action": "selectNode", "logrite.mNode": mNode });
        var arg = new dnn.controls.DNNNodeEventArgs(mNode);
        this.invoke_handler('click', arg);
		logger.error(`selectNode(mNode=${mNode}): arg → ${arg}`, { "event.action": "selectNode", "logrite.mNode": mNode });
        if (arg.get_cancel())
            return;

        if (this.selMNode != null)	//unselect previously selected node
        {
            this.selMNode.selected = null;
            this.selMNode.update('selected');
            this.assignCss(this.selMNode);
        }

        //always select node (we aren't supporting checkboxes yet)
        mNode.selected = true; //(mNode.selected ? null : true);
        mNode.update('selected');
        this.assignCss(mNode);

        this.selMNode = mNode;

		logger.debug(`selectNode(mNode=${mNode}): this.selMNode → ${this.selMNode}`, { "event.action": "selectNode", "logrite.mNode": mNode });
        if (mNode.hasNodes || mNode.hasPendingNodes)
            this.expandNode(mNode, true); //force display

        if (mNode.selected)
        {
            var sJS = this.defaultJS;
            if (mNode.js.length > 0)
                sJS = mNode.js;

            if (sJS.length > 0)
            {
                this.update(true);  //always save state possible posting back (module actions)
                if (eval(sJS) == false)
                    return; //don't do postback if returns false
            }

            if (mNode.clickAction == dnn.controls.action.postback)
            {
                this.update(true);  //always save state when posting back
                eval(this.postBack.replace('[NODEID]', mNode.id));
            }
            else if (mNode.clickAction == dnn.controls.action.nav)
                dnn.dom.navigate(mNode.getUrl(this), mNode.target.length > 0 ? mNode.target : this.target);
        }
        return true;
    },

    assignCss: function(mNode)
    {
		logger.error(`>>> Entering assignCss(mNode=${mNode})`, { "event.action": "assignCss", "logrite.mNode": mNode });
        var ctr = this.getChildControl(mNode.id, 'ctr');
        var css = this.css;
		logger.error(`assignCss(mNode=${mNode}): ctr → ${ctr}`, { "event.action": "assignCss", "logrite.mNode": mNode });

        if (mNode.level > 0 && this.cssChild.length > 0)
            css = this.cssChild;

        if (mNode.css.length > 0)
            css = mNode.css;

        if (mNode.hover)
            css += ' hov ' + (mNode.cssHover.length > 0 ? mNode.cssHover : this.cssHover);
        if (mNode.selected)
            css += ' sel ' + (mNode.cssSel.length > 0 ? mNode.cssSel : this.cssSel);
        if (mNode.breadcrumb)
            css += ' bc';
        if (mNode.isBreak)
            css += ' break';

        css += ' mi mi' + mNode.node.getNodePath();
        css += ' id' + mNode.id;

        if (mNode.level == 0)
            css += ' root';

        if (mNode.node.getNodeIndex() == 0)
            css += ' first';

        if (mNode.node.getNodeIndex() == mNode.node.parentNode().childNodeCount() - 1)
            css += ' last';

        if ((mNode.node.getNodeIndex() == 0) && (mNode.node.getNodeIndex() == mNode.node.parentNode().childNodeCount() - 1))
            css += ' firstlast';    //allow for both to be detected


        ctr.className = css;
		logger.debug(`assignCss(mNode=${mNode}): ctr.className → ${ctr.className}`, { "event.action": "assignCss", "logrite.mNode": mNode });
    },

    update: function()
    {
		logger.error('>>> Entering update()', { "event.action": "update" });
        dnn.setVar(this.ns + '_json', this.rootNode.getJSON());
		logger.warn('<<< Exiting update()', { "event.action": "update" });
    },

    //--- Event Handlers ---//
    _onsubmit: function()
    {
		logger.error('>>> Entering _onsubmit()', { "event.action": "_onsubmit" });
        this.update(true);
		logger.warn('<<< Exiting _onsubmit()', { "event.action": "_onsubmit" });
    },

    _bodyClick: function()
    {
		logger.error('>>> Entering _bodyClick()', { "event.action": "_bodyClick" });
        this.hideMenus();
		logger.warn('<<< Exiting _bodyClick()', { "event.action": "_bodyClick" });
    },

    focusHandler: function(e)
    {
		logger.error(`>>> Entering focusHandler(e=${e})`, { "event.action": "focusHandler", "logrite.e": e });
        var mNode = this.hoverMNode;
        if (mNode == null)
            mNode = this.selMNode;
			logger.error(`focusHandler(e=${e}): mNode → ${mNode}`, { "event.action": "focusHandler", "logrite.e": e });
        if (mNode == null)
            mNode = new dnn.controls.DNNMenuNode(this.nodes[0]);
        //mNode = new dnn.controls.DNNMenuNode(this.rootNode.childNodes(0));
        this.hoverNode(mNode);
        this.container.onfocus = null;
		logger.warn(`<<< Exiting focusHandler(e=${e})`, { "event.action": "focusHandler", "logrite.e": e });
    },

    blurHandler: function(e)
    {
		logger.error(`>>> Entering blurHandler(e=${e})`, { "event.action": "blurHandler", "logrite.e": e });
        if (this.hoverMNode != null)
            this.hoverNode(null);

        dnn.cancelDelay(this.ns + 'min');
        if (this.moutDelay > 0)
            dnn.doDelay(this.ns + 'mout', this.moutDelay, this._hideMenusDelegate);
        else
            this.hideMenus();
    },

    safariKeyHandler: function(e)
    {
		logger.error(`>>> Entering safariKeyHandler(e=${e})`, { "event.action": "safariKeyHandler", "logrite.e": e });
        if (e.charCode == KEY_RETURN)
        {
            if (this.hoverMNode != null && this.hoverMNode.enabled)
                this.selectNode(this.hoverMNode);
            return false;
        }
    },

    keyboardHandler: function(e)
    {
		logger.error(`>>> Entering keyboardHandler(e=${e})`, { "event.action": "keyboardHandler", "logrite.e": e });
        var code = e.keyCode;
        if (code == null)
            code = e.charCode;
        if (code == KEY_RETURN)
        {
            if (this.hoverMNode != null && this.hoverMNode.enabled)
                this.selectNode(this.hoverMNode);
            return false;
        }

        if (code == KEY_ESCAPE)
        {
            this.blurHandler();
            return false;
        }

        if (code >= KEY_LEFT_ARROW && code <= KEY_DOWN_ARROW)
        {
            var iDir = (code == KEY_UP_ARROW || code == KEY_LEFT_ARROW) ? -1 : 1;
			logger.error(`keyboardHandler(e=${e}): iDir → ${iDir}`, { "event.action": "keyboardHandler", "logrite.e": e });
            var sAxis = (code == KEY_UP_ARROW || code == KEY_DOWN_ARROW) ? 'y' : 'x';
			logger.error(`keyboardHandler(e=${e}): sAxis → ${sAxis}`, { "event.action": "keyboardHandler", "logrite.e": e });

            var mNode = this.hoverMNode;
			logger.error(`keyboardHandler(e=${e}): mNode → ${mNode}`, { "event.action": "keyboardHandler", "logrite.e": e });
            var oNewMNode;
            if (mNode == null)
                mNode = new dnn.controls.DNNMenuNode(this.nodes[0]);
            //mNode = new dnn.controls.DNNMenuNode(this.rootNode.childNodes(0));

            var bHor = !this.isNodeVertical(mNode);
			logger.error(`keyboardHandler(e=${e}): bHor → ${bHor}`, { "event.action": "keyboardHandler", "logrite.e": e });
            if ((sAxis == 'y' && !bHor) || (bHor && sAxis == 'x'))
            {
                this.hideMenus(new dnn.controls.DNNMenuNode(mNode.node.parentNode()));
                oNewMNode = this.__getNextNode(mNode, iDir);
            }
            else
            {
                if (iDir == -1)
                {
                    oNewMNode = new dnn.controls.DNNMenuNode(mNode.node.parentNode());
                    if (oNewMNode.level == 0 && this.orient == dnn.controls.orient.horizontal)
                        oNewMNode = this.__getNextNode(new dnn.controls.DNNMenuNode(mNode.node.parentNode()), iDir);
                    this.hideMenus(oNewMNode);

                }
                else if (iDir == 1)
                {
                    if (mNode.hasNodes || mNode.hasPendingNodes)
                    {
                        if (mNode.expanded != true)
                        {
                            this.expandNode(mNode);
                            if (this.podInProgress == false)
                                oNewMNode = new dnn.controls.DNNMenuNode(mNode.node.nodes[0]);
                            //oNewMNode = new dnn.controls.DNNMenuNode(mNode.node.childNodes(0));
                        }
                    }
                    else
                    {
                        var node = mNode.node;
						logger.error(`keyboardHandler(e=${e}): node → ${node}`, { "event.action": "keyboardHandler", "logrite.e": e });
                        while (node.parentNode().nodeName() != 'root')
                            node = node.parentNode();
                        oNewMNode = new dnn.controls.DNNMenuNode(node);
                        oNewMNode = this.__getNextNode(oNewMNode, iDir);
                        this.hideMenus(new dnn.controls.DNNMenuNode(oNewMNode.node.parentNode()));
						logger.error(`keyboardHandler(e=${e}): oNewMNode → ${oNewMNode}`, { "event.action": "keyboardHandler", "logrite.e": e });
                    }
                }
            }
            if (oNewMNode != null && oNewMNode.node.nodeName() != 'root')
                this.hoverNode(oNewMNode);

		logger.warn(`<<< Exiting keyboardHandler(e=${e})`, { "event.action": "keyboardHandler", "logrite.e": e });
            return false;
        }

    },

    dispose: function()
    {
		logger.error('>>> Entering dispose()', { "event.action": "dispose" });
        this._onsubmitDelegate = null;
        this._hideMenusDelegate = null;
		logger.debug(`dispose(): this._onsubmitDelegate → ${this._onsubmitDelegate}`, { "event.action": "dispose" });
        this._expandNodeDelegate = null;
        dnn.controls.DNNMenu.callBaseMethod(this, 'dispose');
		logger.debug(`dispose(): this._hideMenusDelegate → ${this._hideMenusDelegate}`, { "event.action": "dispose" });
    },

    __getNextNode: function(mNode, iDir)
    {
		logger.error(`>>> Entering __getNextNode(mNode=${mNode},iDir=${iDir})`, { "event.action": "__getNextNode", "logrite.mNode": mNode, "logrite.iDir": iDir });
        var node;
        var parentNode = mNode.node.parentNode();
		logger.error(`__getNextNode(mNode=${mNode},iDir=${iDir}): parentNode → ${parentNode}`, { "event.action": "__getNextNode", "logrite.mNode": mNode, "logrite.iDir": iDir });
        var nodeIndex = mNode.node.getNodeIndex('id');
		logger.error(`__getNextNode(mNode=${mNode},iDir=${iDir}): nodeIndex → ${nodeIndex}`, { "event.action": "__getNextNode", "logrite.mNode": mNode, "logrite.iDir": iDir });
        if (nodeIndex + iDir < 0)	//if first node was selected and going left, select last node
            node = parentNode.nodes[parentNode.childNodeCount() - 1]; //node = parentNode.childNodes(parentNode.childNodeCount()-1);
        else if (nodeIndex + iDir > parentNode.childNodeCount() - 1)
            node = parentNode.nodes[0]; //node = parentNode.childNodes(0);
        else
            node = parentNode.nodes[nodeIndex + iDir]; //node = parentNode.childNodes(nodeIndex + iDir);

        var oRetNode = new dnn.controls.DNNMenuNode(node);
		logger.error(`__getNextNode(mNode=${mNode},iDir=${iDir}): oRetNode → ${oRetNode}`, { "event.action": "__getNextNode", "logrite.mNode": mNode, "logrite.iDir": iDir });
        if (oRetNode.isBreak)
        {
            nodeIndex += iDir; //check next one
            if (nodeIndex + iDir < 0)
                node = parentNode.childNodes(parentNode.childNodeCount() - 1);
            else if (nodeIndex + iDir > parentNode.childNodeCount() - 1)
                node = parentNode.childNodes(0);
            else
                node = parentNode.childNodes(nodeIndex + iDir);
			logger.error(`__getNextNode(mNode=${mNode},iDir=${iDir}): node → ${node}`, { "event.action": "__getNextNode", "logrite.mNode": mNode, "logrite.iDir": iDir });
            return new dnn.controls.DNNMenuNode(node);
        }
        else
            return oRetNode;
    },

    callBackFail: function(result, ctx, req)
    {
		logger.error(`>>> Entering callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        var mNode = ctx;
        var menu = mNode.menu;
        menu.invoke_handler('callBackFail', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
		logger.warn(`<<< Exiting callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },

    callBackStatus: function(result, ctx, req)
    {
		logger.error(`>>> Entering callBackStatus(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackStatus", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        var mNode = ctx;
        var menu = mNode.menu;
        menu.invoke_compatHandler('callBackStatus', result, ctx, req);
		logger.warn(`<<< Exiting callBackStatus(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackStatus", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },

    callBackSuccess: function(result, ctx, req)
    {
		logger.error(`>>> Entering callBackSuccess(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        var mNode = ctx;
        var node = mNode.node;
		logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): mNode → ${mNode}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
		logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): node → ${node}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
		logger.debug(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): node.nodes → ${node.nodes}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
		logger.debug(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): mNode.hasPendingNodes → ${mNode.hasPendingNodes}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        var menu = mNode.menu;

        menu.showWorkImage(mNode, false);
        //node.appendXml(result);
        var json = dnn.evalJSON("{" + result + "}");
		logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): json → ${json}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
		logger.debug(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): mNode.hasNodes → ${mNode.hasNodes}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
		logger.debug(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): menu.podInProgress → ${menu.podInProgress}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        node.nodes = json.nodes;
        node.setupJSONNodes(node.rootNode(), node, node.nodes);

        var subMenu = menu.getChildControl(mNode.id, 'sub');
		logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): subMenu → ${subMenu}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        for (var i = 0; i < node.childNodeCount(); i++)
            menu.renderNode(node.childNodes(i), subMenu);

        mNode = new dnn.controls.DNNMenuNode(node);
        mNode.hasPendingNodes = false;
        mNode.hasNodes = true;

        subMenu = menu.getChildControl(mNode.id, 'sub');
        menu.expandNode(mNode);
        //menu.expandNode(new dnn.controls.DNNMenuNode(node));

        menu.callBackStatus(result, ctx, req);
        menu.podInProgress = false;

        menu.invoke_handler('callBackSuccess', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
		logger.warn(`<<< Exiting callBackSuccess(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },

    _nodeTextClick: function(evt)
    {
		logger.error(`>>> Entering _nodeTextClick(evt=${evt})`, { "event.action": "_nodeTextClick", "logrite.evt": evt });
        var node = this._findEventNode(evt);
        var mNode;
		logger.error(`_nodeTextClick(evt=${evt}): node → ${node}`, { "event.action": "_nodeTextClick", "logrite.evt": evt });

        if (node != null)
        {
            mNode = new dnn.controls.DNNMenuNode(node);
            this.selectNode(mNode);
			logger.error(`_nodeTextClick(evt=${evt}): mNode → ${mNode}`, { "event.action": "_nodeTextClick", "logrite.evt": evt });
        }
        evt.stopPropagation();
		logger.warn(`<<< Exiting _nodeTextClick(evt=${evt})`, { "event.action": "_nodeTextClick", "logrite.evt": evt });
    },

    _menuMOver: function(evt, element)
    {
		logger.error(`>>> Entering _menuMOver(evt=${evt},element=${element})`, { "event.action": "_menuMOver", "logrite.evt": evt, "logrite.element": element });
        dnn.cancelDelay(this.ns + 'mout');
		logger.warn(`<<< Exiting _menuMOver(evt=${evt},element=${element})`, { "event.action": "_menuMOver", "logrite.evt": evt, "logrite.element": element });
    },

    _menuMOut: function(evt, element)
    {
		logger.error(`>>> Entering _menuMOut(evt=${evt},element=${element})`, { "event.action": "_menuMOut", "logrite.evt": evt, "logrite.element": element });
        dnn.cancelDelay(this.ns + 'min');
        if (this.moutDelay > 0)
            dnn.doDelay(this.ns + 'mout', this.moutDelay, dnn.createDelegate(this, this.hideMenus));
        else
            this.hideMenus();

    },

    _nodeMOver: function(evt, element)
    {
		logger.error(`>>> Entering _nodeMOver(evt=${evt},element=${element})`, { "event.action": "_nodeMOver", "logrite.evt": evt, "logrite.element": element });
        var node = this._findEventNode(evt);
		logger.error(`_nodeMOver(evt=${evt},element=${element}): node → ${node}`, { "event.action": "_nodeMOver", "logrite.evt": evt, "logrite.element": element });
        if (node != null)
        {
            var mNode = new dnn.controls.DNNMenuNode(node);
			logger.error(`_nodeMOver(evt=${evt},element=${element}): mNode → ${mNode}`, { "event.action": "_nodeMOver", "logrite.evt": evt, "logrite.element": element });
            //this.hideMenus(new dnn.controls.DNNMenuNode(node.parentNode())); //MinDelay???
            mNode.hover = true;
			logger.debug(`_nodeMOver(evt=${evt},element=${element}): mNode.hover → ${mNode.hover}`, { "event.action": "_nodeMOver", "logrite.evt": evt, "logrite.element": element });
            this.assignCss(mNode);

            if (mNode.expanded != true)
                this.expandNode(mNode);
            evt.stopPropagation();
        }
        this._menuMOver(evt, element);
		logger.warn(`<<< Exiting _nodeMOver(evt=${evt},element=${element})`, { "event.action": "_nodeMOver", "logrite.evt": evt, "logrite.element": element });
    },

    _nodeMOut: function(evt, element)
    {
		logger.error(`>>> Entering _nodeMOut(evt=${evt},element=${element})`, { "event.action": "_nodeMOut", "logrite.evt": evt, "logrite.element": element });
        var node = this._findEventNode(evt);
		logger.error(`_nodeMOut(evt=${evt},element=${element}): node → ${node}`, { "event.action": "_nodeMOut", "logrite.evt": evt, "logrite.element": element });
        if (node != null)
        {
            var mNode = new dnn.controls.DNNMenuNode(node);
			logger.error(`_nodeMOut(evt=${evt},element=${element}): mNode → ${mNode}`, { "event.action": "_nodeMOut", "logrite.evt": evt, "logrite.element": element });
            this.assignCss(mNode);
            this._menuMOut(evt, element);
            evt.stopPropagation();
			logger.warn(`<<< Exiting _nodeMOut(evt=${evt},element=${element})`, { "event.action": "_nodeMOut", "logrite.evt": evt, "logrite.element": element });
        }
    },


    //OBSOLETE!
    getXml: function()
    {
		logger.error('>>> Entering getXml()', { "event.action": "getXml" });
		logger.warn('<<< Exiting getXml()', { "event.action": "getXml" });
        return this.rootNode.getXml();
    },

    _findEventNode: function(evt)
    {
		logger.error(`>>> Entering _findEventNode(evt=${evt})`, { "event.action": "_findEventNode", "logrite.evt": evt });
        if (dnn.dom.isNonTextNode(evt.target))
            return this.rootNode.findNode(this.getChildControlBaseId(evt.target));
    },

    _loadNodes: function()
    {
		logger.error('>>> Entering _loadNodes()', { "event.action": "_loadNodes" });
        var json = dnn.evalJSON(dnn.getVar(this.ns + '_json'));
		logger.error(`_loadNodes(): json → ${json}`, { "event.action": "_loadNodes" });
        if (json)
        {
            this.nodes = json.nodes;
            this.rootNode = {};
			logger.debug(`_loadNodes(): this.nodes → ${this.nodes}`, { "event.action": "_loadNodes" });
            this.rootNode.nodes = this.nodes;
            this.rootNode.id = this.ns;
            this.rootNode = new dnn.controls.JSONNode(this.rootNode, 'root', 0);
			logger.debug(`_loadNodes(): this.rootNode → ${this.rootNode}`, { "event.action": "_loadNodes" });
			logger.warn('<<< Exiting _loadNodes()', { "event.action": "_loadNodes" });
        }
    },

    _setupKeyHandling: function()
    {
		logger.error('>>> Entering _setupKeyHandling()', { "event.action": "_setupKeyHandling" });
        if (this.container.tabIndex <= 0)
        {
            this.container.tabIndex = 0;
            this.addHandlers(this.container, { "keydown": this.keyboardHandler,
                "focus": this.focusHandler,
                "blur": this.blurHandler
            }, this);
        }
        else
        {
            var txt = document.createElement('input');
			logger.error(`_setupKeyHandling(): txt → ${txt}`, { "event.action": "_setupKeyHandling" });
            txt.type = 'text';
			logger.debug(`_setupKeyHandling(): txt.type → ${txt.type}`, { "event.action": "_setupKeyHandling" });
            txt.style.width = 0;
            txt.style.height = 0;
            txt.style.background = 'transparent';
            txt.style.border = 0;
            txt.style.positioning = 'absolute';
            if (dnn.dom.browser.isType(dnn.dom.browser.Safari))
            {
                txt.style.width = '1px'; //safari doesn't like zero
                txt.style.height = '1px';
                txt.style.left = '-999em';
                this.addHandlers(txt, { "keydown": this.keyboardHandler }, this);
                this.addHandlers(this.container.parentNode, { "keypress": this.safariKeyHandler }, this);
            }
            else
                this.addHandlers(txt, { "keypress": this.keyboardHandler }, this);

            this.addHandlers(txt, { "focus": this.focusHandler,
                "blur": this.blurHandler
            }, this);

            this.container.parentNode.appendChild(txt);
			logger.warn('<<< Exiting _setupKeyHandling()', { "event.action": "_setupKeyHandling" });
        }
    }


}

dnn.controls.DNNMenu.registerClass('dnn.controls.DNNMenu', dnn.controls.control);

//DNNMenuBuilder object
dnn.controls.DNNMenuBuilder = function(menu, mNode, ctr)
{
    this.menu = menu;
    this.mNode = mNode;
    this.isVertical = menu.isNodeVertical(mNode);
    this.container = ctr;
    this.row = null;
    this.subcont = null;
    this.alreadyRendered = false;
}

//DNNMenuBuilder specific methods
dnn.controls.DNNMenuBuilder.prototype =
{
    appendChild: function(ctl, isNewCell)
    {
		logger.error(`>>> Entering appendChild(ctl=${ctl},isNewCell=${isNewCell})`, { "event.action": "appendChild", "logrite.ctl": ctl, "logrite.isNewCell": isNewCell });
        this.subcont.appendChild(ctl);
		logger.warn(`<<< Exiting appendChild(ctl=${ctl},isNewCell=${isNewCell})`, { "event.action": "appendChild", "logrite.ctl": ctl, "logrite.isNewCell": isNewCell });
    },

    newCell: function() { },

    newCont: function()
    {
		logger.error('>>> Entering newCont()', { "event.action": "newCont" });
        if (this.isVertical)
            this.row = this.menu.createChildControl('div', this.mNode.id, 'ctr'); //container for Node
        else
            this.row = this.menu.createChildControl('span', this.mNode.id, 'ctr'); //container for Node
        this.subcont = this.row;
        this.container.appendChild(this.subcont);
		logger.debug(`newCont(): this.subcont → ${this.subcont}`, { "event.action": "newCont" });
    },

    newRow: function()
    {
		logger.error('>>> Entering newRow()', { "event.action": "newRow" });
        //if (this.container.childNodes.length > 0)
		logger.warn('<<< Exiting newRow()', { "event.action": "newRow" });
        //	this.container.appendChild(document.createElement('br'));
    },

    createSubMenu: function()
    {
		logger.error('>>> Entering createSubMenu()', { "event.action": "createSubMenu" });
		logger.warn('<<< Exiting createSubMenu()', { "event.action": "createSubMenu" });
        return this.menu.createChildControl('DIV', this.mNode.id, 'sub'); //Not using SPAN due to FireFox bug...
    }
}
dnn.controls.DNNMenuBuilder.registerClass('dnn.controls.DNNMenuBuilder');


//DNNMenuTableBuilder object inherits DNNMenuBuilder
dnn.controls.DNNMenuTableBuilder = function(menu, node, cont)
{
    dnn.controls.DNNMenuTableBuilder.initializeBase(this, [menu, node, cont]);
}

//DNNMenuTableBuilder specific methods
dnn.controls.DNNMenuTableBuilder.prototype =
{
    appendChild: function(ctl, isNewCell)
    {
        if (isNewCell)
            this.newCell();
        this.subcont.appendChild(ctl);
    },

    newCont: function()
    {
        this.subcont = this.newCell(); //TD	
    },

    newCell: function()
    {
		logger.error('>>> Entering newCell()', { "event.action": "newCell" });
        var td = dnn.dom.createElement('td');
        this.row.appendChild(td);
		logger.error(`newCell(): td → ${td}`, { "event.action": "newCell" });
        this.subcont = td;
		logger.warn('<<< Exiting newCell()', { "event.action": "newCell" });
        return td;
    },

    newRow: function()
    {
        this.row = this.menu.createChildControl('tr', this.mNode.id, 'ctr'); //TR
        var tb = dnn.dom.getByTagName('TBODY', this.container);
        tb[0].appendChild(this.row);
		logger.error(`newRow(): tb → ${tb}`, { "event.action": "newRow" });
    },

    createSubMenu: function()
    {
        var subMenu = this.menu.createChildControl('table', this.mNode.id, 'sub');
        subMenu.border = 0;
		logger.error(`createSubMenu(): subMenu → ${subMenu}`, { "event.action": "createSubMenu" });
		logger.debug(`createSubMenu(): subMenu.border → ${subMenu.border}`, { "event.action": "createSubMenu" });
		logger.debug(`createSubMenu(): subMenu.cellSpacing → ${subMenu.cellSpacing}`, { "event.action": "createSubMenu" });
        subMenu.cellPadding = 0;
		logger.debug(`createSubMenu(): subMenu.cellPadding → ${subMenu.cellPadding}`, { "event.action": "createSubMenu" });
        subMenu.cellSpacing = 0;
        subMenu.appendChild(dnn.dom.createElement('tbody'));
        return subMenu;
    }
}

dnn.controls.DNNMenuTableBuilder.registerClass('dnn.controls.DNNMenuTableBuilder', dnn.controls.DNNMenuBuilder);

//DNNMenuTableBuilder object inherits DNNMenuBuilder
dnn.controls.DNNMenuListBuilder = function(menu, node, cont)
{
    dnn.controls.DNNMenuListBuilder.initializeBase(this, [menu, node, cont]);
    this.alreadyRendered = true;
    this.row = dnn.dom.getById(this.menu.getChildControlId(this.mNode.id, 'ctr'), this.menu.container);
    this.menu.registerChildControl(this.row, this.mNode.id);
    this._setStyles(this.row);
    this.subcont = this.row;

    this.subMenu = dnn.dom.getById(this.menu.getChildControlId(this.mNode.id, 'sub'), this.menu.container);
    if (this.subMenu)
    {
        this.menu.registerChildControl(this.subMenu, this.mNode.id);
        this._setStyles(this.subMenu);
    }

}

//DNNMenuListBuilder specific methods
dnn.controls.DNNMenuListBuilder.prototype =
{
    appendChild: function(ctl, isNewCell) { },
    newCont: function() { },

    createSubMenu: function()
    {
        //    this.subMenu.parentNode.removeChild(this.subMenu);
        return this.subMenu;
    },

    _setStyles: function(ctl)
    {
        if (this.menu.isNodeVertical(this.mNode) == false)
            ctl.style.display = 'inline';

        ctl.style.listStyle = 'none';
        //ctl.style.margin = 0; //WCT-9771
        //ctl.style.padding = 0; //WCT-9771
        //ctl.style.float = 'left';
        //ctl.style.clear = 'left';
		logger.warn(`<<< Exiting _setStyles(ctl=${ctl})`, { "event.action": "_setStyles", "logrite.ctl": ctl });
    }
}

dnn.controls.DNNMenuListBuilder.registerClass('dnn.controls.DNNMenuListBuilder', dnn.controls.DNNMenuBuilder);


dnn.controls.DNNMenuNode = function(node)
{
    dnn.controls.DNNMenuNode.initializeBase(this, [node]);
    this._addAbbr({ breadcrumb: 'bcrumb', clickAction: 'ca', imageIndex: 'iIdx', urlIndex: 'uIdx' });

    //menu specific attributes
    this.hover = false;
    this.expanded = null; //node.getAttribute('expanded', '0') == '1' ? true : null;
    this.selected = node.getAttribute('selected', '0') == '1' ? true : null;
    this.breadcrumb = node.getAttribute('bcrumb', '0') == '1' ? true : null;
    this.clickAction = node.getAttribute('ca', dnn.controls.action.postback);
    this.imageIndex = new Number(node.getAttribute('iIdx', '-1'));
    this.urlIndex = new Number(node.getAttribute('uIdx', '-1'));
    this.lhtml = node.getAttribute('lhtml', '');
    this.rhtml = node.getAttribute('rhtml', '');
}

//DNNMenuNode specific methods
dnn.controls.DNNMenuNode.prototype =
{
    childNodes: function(iIndex)
    {
		logger.error(`>>> Entering childNodes(iIndex=${iIndex})`, { "event.action": "childNodes", "logrite.iIndex": iIndex });
		logger.warn(`<<< Exiting childNodes(iIndex=${iIndex})`, { "event.action": "childNodes", "logrite.iIndex": iIndex });
        if (this.node.nodes(iIndex) != null)
            return new dnn.controls.DNNMenuNode(this.node.nodes(iIndex));

    },
    getUrl: function(menu)
    {
		logger.error(`>>> Entering getUrl(menu=${menu})`, { "event.action": "getUrl", "logrite.menu": menu });
        if (this.urlIndex > -1)
            return menu.urlList[this.urlIndex] + this.url;
        else
            return this.url;
    }
}
dnn.controls.DNNMenuNode.registerClass('dnn.controls.DNNMenuNode', dnn.controls.DNNNode);

Type.registerNamespace('dnn.controls');

dnn.controls.menuRenderMode = function() { };
dnn.controls.menuRenderMode.prototype = {
    normal: 0,
    notables: 1,
    unorderdlist: 2,
    downlevel: 3
}
dnn.controls.menuRenderMode.registerEnum("dnn.controls.menuRenderMode");
