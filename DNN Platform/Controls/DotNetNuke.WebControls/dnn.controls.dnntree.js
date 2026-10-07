var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.controls.js" assembly="DotNetNuke.WebControls" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.dom.positioning.js" assembly="DotNetNuke.WebUtility" />
/// <reference name="dnn.xmlhttp.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.controls');

dnn.extend(dnn.controls, {
    initTree: function(ctl)
    {
		logger.error(`>>> Entering initTree(ctl=${ctl})`, { "event.action": "initTree", "logrite.ctl": ctl });
        if (ctl)
        {
            var tree = new dnn.controls.DNNTree(ctl);
            tree.initialize();
			logger.error(`initTree(ctl=${ctl}): tree → ${tree}`, { "event.action": "initTree", "logrite.ctl": ctl });
			logger.warn(`<<< Exiting initTree(ctl=${ctl})`, { "event.action": "initTree", "logrite.ctl": ctl });
            return tree;
        }
    }
});

//------- Constructor -------//
dnn.controls.DNNTree = function(o)
{
    dnn.controls.DNNTree.initializeBase(this, [o]);

    //--- Data Properties ---//  
    this.rootNode = null;
    this.nodes = [];
    this._loadNodes();
    this.hoverTreeNode = null;
    this.selTreeNode = null;

    //--- Appearance Properties ---//
    this.css = this.getProp('css', '');
    this.cssChild = this.getProp('csschild', '');
    this.cssHover = this.getProp('csshover', '');
    this.cssSel = this.getProp('csssel', '');
    this.cssIcon = this.getProp('cssicon', '');

    this.sysImgPath = this.getProp('sysimgpath', 'images/');
    this.imageList = this.getProp('imagelist', '').split(',');
    this.expandImg = this.getProp('expimg', '');
    this.workImg = this.getProp('workimg', 'dnnanim.gif');
    this.animf = new Number(this.getProp('animf', '5'));

    this.collapseImg = this.getProp('colimg', '');

    this.indentWidth = new Number(this.getProp('indentw', '10'));
    if (this.indentWidth == 0)
        this.indentWidth = 10;
    this.checkBoxes = this.getProp('checkboxes', '0') == '1';
    this.checkBoxMode = new Number(this.getProp('cbm', '0'));
    this.target = this.getProp('target', '');
    this.defaultJS = this.getProp('js', '');

    this.postBack = this.getProp('postback', '');
    this.callBack = this.getProp('callback', '');
    this.callBackStatFunc = this.getProp('callbackSF', '');
    if (this.callBackStatFunc.length > 0)
        this.add_handler('callBackStatus', eval(this.callBackStatFunc));

    //obtain width of expand image
    this.expImgWidth = new Number(this.getProp('expcolimgw', '12'));

    kbCtl = this.container;
    if (this.container.tabIndex <= 0)
        this.container.tabIndex = 0;
    else
    {
        kbCtl = document.createElement('input');
        kbCtl.type = 'text';
        kbCtl.style.width = 0;
        kbCtl.style.height = 0;
        kbCtl.style.background = 'transparent';
        kbCtl.style.border = 0;
        kbCtl.style.positioning = 'absolute';
        kbCtl.style.left = '-999em';
        this.container.parentNode.appendChild(kbCtl);
    }
    this.addHandlers(kbCtl, { "keydown": this.keydownHandler,
        "focus": this.focusHandler
    }, this);

    this._onsubmitDelegate = Function.createDelegate(this, this._onsubmit);
    dnn.controls.submitComp.add_handler(this._onsubmitDelegate);

}

dnn.controls.DNNTree.prototype =
{

    initialize: function()
    {
		logger.error('>>> Entering initialize()', { "event.action": "initialize" });
        dnn.controls.DNNTree.callBaseMethod(this, 'initialize');
        this.generateTreeHTML();
		logger.warn('<<< Exiting initialize()', { "event.action": "initialize" });
    },

    focusHandler: function(e)
    {
		logger.error(`>>> Entering focusHandler(e=${e})`, { "event.action": "focusHandler", "logrite.e": e });
        var tNode = this.hoverTreeNode;
        if (tNode == null)
            tNode = new dnn.controls.DNNTreeNode(this.rootNode.childNodes(0));
			logger.error(`focusHandler(e=${e}): tNode → ${tNode}`, { "event.action": "focusHandler", "logrite.e": e });
        this.hoverNode(tNode);
        this.container.onfocus = null;
		logger.warn(`<<< Exiting focusHandler(e=${e})`, { "event.action": "focusHandler", "logrite.e": e });
    },

    keydownHandler: function(e)
    {
		logger.error(`>>> Entering keydownHandler(e=${e})`, { "event.action": "keydownHandler", "logrite.e": e });
        var dir = 0;
        var axis = '';
		logger.error(`keydownHandler(e=${e}): axis → ${axis}`, { "event.action": "keydownHandler", "logrite.e": e });

        if (e.keyCode == KEY_UP_ARROW)
        {
            dir = -1;
            axis = 'y';
        }
        if (e.keyCode == KEY_DOWN_ARROW)
        {
            dir = 1;
            axis = 'y';
        }
        if (e.keyCode == KEY_LEFT_ARROW)
        {
            dir = -1;
            axis = 'x';
        }
        if (e.keyCode == KEY_RIGHT_ARROW)
        {
            dir = 1;
            axis = 'x';
        }

        if (dir != 0)
        {
            var tNode = this.hoverTreeNode;
			logger.error(`keydownHandler(e=${e}): tNode → ${tNode}`, { "event.action": "keydownHandler", "logrite.e": e });
            var node;
            if (tNode == null)
                tNode = new dnn.controls.DNNTreeNode(this.rootNode.childNodes(0));

            if (axis == 'x')
            {
                if (dir == -1)
                {
                    if (tNode.hasNodes && tNode.expanded)
                        this.collapseNode(tNode);
                    else
                        node = tNode.node.parentNode();
                }

                if (dir == 1)
                {
                    if (tNode.hasNodes || tNode.hasPendingNodes)
                    {
                        if (tNode.expanded != true)
                            this.expandNode(tNode);
                        else
                            node = tNode.node.childNodes(0);
                    }
                }
            }
            else if (axis == 'y')
            {
                var iNodeIndex = tNode.node.getNodeIndex('id');
                var parentNode = tNode.node.parentNode();
				logger.error(`keydownHandler(e=${e}): parentNode → ${parentNode}`, { "event.action": "keydownHandler", "logrite.e": e });
                if (tNode.hasNodes && tNode.expanded && dir > 0)	//if has expanded nodes and going down, select first child
                    node = tNode.node.childNodes(0);
                else if (iNodeIndex + dir < 0)	//if first node was selected and going up, select parent
                    node = parentNode;
                else if (iNodeIndex + dir < parentNode.childNodeCount())	//if navigated index less than number of nodes contained in parent
                {
                    node = parentNode.childNodes(iNodeIndex + dir); //navigate there
                    if (dir == -1)		//if going up... look for expanded sibling above (recursively)
                    {
                        var tNode2 = new dnn.controls.DNNTreeNode(node);
						logger.error(`keydownHandler(e=${e}): tNode2 → ${tNode2}`, { "event.action": "keydownHandler", "logrite.e": e });
                        while (tNode2.expanded)	//determine if parent node is expanded, if so find its last child node
                        {
                            if (tNode2.node.childNodeCount() == 0)
                                break;
                            node = tNode2.node.childNodes(tNode2.node.childNodeCount() - 1); //select last node in parent's collection
                            tNode2 = new dnn.controls.DNNTreeNode(node); //needed to check expanded property
                        }
                    }
                }
                else if (parentNode.nodeName() != 'root')	//logic for last node in collection
                {
                    var iNodeIndex = parentNode.getNodeIndex('id');
					logger.error(`keydownHandler(e=${e}): iNodeIndex → ${iNodeIndex}`, { "event.action": "keydownHandler", "logrite.e": e });
                    var tempParent = parentNode;
					logger.error(`keydownHandler(e=${e}): tempParent → ${tempParent}`, { "event.action": "keydownHandler", "logrite.e": e });
                    if (dir == 1)	//if going down... verify that parent node has sibling available to select, if not recursively look for one
                    {
                        while (tempParent.nodeName() != 'root' && iNodeIndex + dir >= tempParent.parentNode().childNodeCount())	//while index greater than node count
                        {
                            tempParent = tempParent.parentNode();
                            iNodeIndex = tempParent.getNodeIndex('id');
                        }
                    }
                    if (tempParent.nodeName() != 'root')
                        node = tempParent.parentNode().childNodes(iNodeIndex + 1);
                }
            }
            if (node != null && node.nodeName() != 'root')
                this.hoverNode(new dnn.controls.DNNTreeNode(node));

            return false;
        }

        if (e.keyCode == KEY_RETURN && this.hoverTreeNode != null)
        {
            this.selectNode(this.hoverTreeNode);
		logger.warn(`<<< Exiting keydownHandler(e=${e})`, { "event.action": "keydownHandler", "logrite.e": e });
            return false;
        }

    },

    hoverNode: function(tNode)
    {
		logger.error(`>>> Entering hoverNode(tNode=${tNode})`, { "event.action": "hoverNode", "logrite.tNode": tNode });
        if (this.hoverTreeNode != null)
        {
            this.hoverTreeNode.hover = false;
            this.assignCss(this.hoverTreeNode);
        }
        tNode.hover = true;
		logger.debug(`hoverNode(tNode=${tNode}): tNode.hover → ${tNode.hover}`, { "event.action": "hoverNode", "logrite.tNode": tNode });
        this.assignCss(tNode);
        this.hoverTreeNode = tNode;
		logger.debug(`hoverNode(tNode=${tNode}): this.hoverTreeNode → ${this.hoverTreeNode}`, { "event.action": "hoverNode", "logrite.tNode": tNode });
    },

    getXml: function()
    {
		logger.error('>>> Entering getXml()', { "event.action": "getXml" });
		logger.warn('<<< Exiting getXml()', { "event.action": "getXml" });
        return this.rootNode.getXml();
    },

    expandNode: function(tNode)
    {
		logger.error(`>>> Entering expandNode(tNode=${tNode})`, { "event.action": "expandNode", "logrite.tNode": tNode });
        var ctr = this.getChildControl(tNode.id, 'pctr');
        var expandCol = this.getChildControl(tNode.id, 'expcol');
		logger.error(`expandNode(tNode=${tNode}): ctr → ${ctr}`, { "event.action": "expandNode", "logrite.tNode": tNode });
		logger.error(`expandNode(tNode=${tNode}): expandCol → ${expandCol}`, { "event.action": "expandNode", "logrite.tNode": tNode });
		logger.debug(`expandNode(tNode=${tNode}): tNode.expanded → ${tNode.expanded}`, { "event.action": "expandNode", "logrite.tNode": tNode });

        expandCol.src = this.expandImg;
		logger.debug(`expandNode(tNode=${tNode}): expandCol.src → ${expandCol.src}`, { "event.action": "expandNode", "logrite.tNode": tNode });
        tNode.expanded = true;
        tNode.update();
        this.update();

        if (tNode.hasPendingNodes)
        {
            var sXml = tNode.node.getXml();
			logger.error(`expandNode(tNode=${tNode}): sXml → ${sXml}`, { "event.action": "expandNode", "logrite.tNode": tNode });
            tNode.tree = this; //need to give reference back to self

			logger.debug(`expandNode(tNode=${tNode}): tNode.tree → ${tNode.tree}`, { "event.action": "expandNode", "logrite.tNode": tNode });
            if (this.workImg != null)
            {
                var icon = this.getChildControl(tNode.id, 'icn');
				logger.error(`expandNode(tNode=${tNode}): icon → ${icon}`, { "event.action": "expandNode", "logrite.tNode": tNode });
				logger.debug(`expandNode(tNode=${tNode}): icon.src → ${icon.src}`, { "event.action": "expandNode", "logrite.tNode": tNode });
                if (icon)
                    icon.src = this.sysImgPath + this.workImg;
            }
            if (this.callBack.indexOf('[NODEXML]') > -1)
                eval(this.callBack.replace('[NODEXML]', dnn.escapeForEval(sXml)));
            else
                eval(this.callBack.replace('[NODEID]', tNode.id));

            tNode.hasPendingNodes = false;
			logger.debug(`expandNode(tNode=${tNode}): tNode.hasPendingNodes → ${tNode.hasPendingNodes}`, { "event.action": "expandNode", "logrite.tNode": tNode });
            tNode.hasNodes = true;
			logger.debug(`expandNode(tNode=${tNode}): tNode.hasNodes → ${tNode.hasNodes}`, { "event.action": "expandNode", "logrite.tNode": tNode });
            this.hoverTreeNode = tNode;
        }
        else
        {
            dnn.dom.expandElement(ctr, this.animf);
			logger.debug(`expandNode(tNode=${tNode}): this.hoverTreeNode → ${this.hoverTreeNode}`, { "event.action": "expandNode", "logrite.tNode": tNode });
        }

		logger.warn(`<<< Exiting expandNode(tNode=${tNode})`, { "event.action": "expandNode", "logrite.tNode": tNode });
        return true;
    },

    collapseNode: function(tNode)
    {
		logger.error(`>>> Entering collapseNode(tNode=${tNode})`, { "event.action": "collapseNode", "logrite.tNode": tNode });
        var ctr = this.getChildControl(tNode.id, 'pctr');
        var expandCol = this.getChildControl(tNode.id, 'expcol');
		logger.error(`collapseNode(tNode=${tNode}): ctr → ${ctr}`, { "event.action": "collapseNode", "logrite.tNode": tNode });
        //ctr.style.display = 'none';
        dnn.dom.collapseElement(ctr, this.animf);
		logger.error(`collapseNode(tNode=${tNode}): expandCol → ${expandCol}`, { "event.action": "collapseNode", "logrite.tNode": tNode });
		logger.debug(`collapseNode(tNode=${tNode}): tNode.expanded → ${tNode.expanded}`, { "event.action": "collapseNode", "logrite.tNode": tNode });
        expandCol.src = this.collapseImg;
		logger.debug(`collapseNode(tNode=${tNode}): expandCol.src → ${expandCol.src}`, { "event.action": "collapseNode", "logrite.tNode": tNode });
        tNode.expanded = null;
        tNode.update();
        this.update();
		logger.warn(`<<< Exiting collapseNode(tNode=${tNode})`, { "event.action": "collapseNode", "logrite.tNode": tNode });
        return true;
    },

    selectNode: function(tNode)
    {
		logger.error(`>>> Entering selectNode(tNode=${tNode})`, { "event.action": "selectNode", "logrite.tNode": tNode });
        var arg = new dnn.controls.DNNNodeEventArgs(tNode);
        this.invoke_handler('click', arg);
		logger.error(`selectNode(tNode=${tNode}): arg → ${arg}`, { "event.action": "selectNode", "logrite.tNode": tNode });
        if (arg.get_cancel())
            return;

        if (this.selTreeNode != null && this.checkBoxes == false)
        {
            this.selTreeNode.selected = null;
            this.assignCss(this.selTreeNode);
            this.selTreeNode.update('selected');
        }

        if (tNode.selected)
        {
            tNode.selected = null;
			logger.debug(`selectNode(tNode=${tNode}): tNode.selected → ${tNode.selected}`, { "event.action": "selectNode", "logrite.tNode": tNode });
            this.assignCss(tNode);
        }
        else
        {
            tNode.selected = true;
            this.hoverTreeNode = tNode;
            this.assignCss(tNode);
			logger.debug(`selectNode(tNode=${tNode}): this.hoverTreeNode → ${this.hoverTreeNode}`, { "event.action": "selectNode", "logrite.tNode": tNode });
        }
        tNode.update('selected');

        this.selTreeNode = tNode;
        this.update();
		logger.debug(`selectNode(tNode=${tNode}): this.selTreeNode → ${this.selTreeNode}`, { "event.action": "selectNode", "logrite.tNode": tNode });

        var chk = this.getChildControl(tNode.id, 'chk');
		logger.error(`selectNode(tNode=${tNode}): chk → ${chk}`, { "event.action": "selectNode", "logrite.tNode": tNode });
		logger.debug(`selectNode(tNode=${tNode}): chk.checked → ${chk.checked}`, { "event.action": "selectNode", "logrite.tNode": tNode });
        if (chk != null)
            chk.checked = tNode.selected;

        if (tNode.selected)
        {
            var js = '';
            if (this.defaultJS.length > 0)
                js = this.defaultJS;
            if (tNode.js.length > 0)
                js = tNode.js;

            if (js.length > 0)
            {
                if (eval(js) == false)
                    return; //don't do postback if returns false
            }

            if (tNode.clickAction == null || tNode.clickAction == dnn.controls.action.postback)
                eval(this.postBack.replace('[NODEID]', tNode.id));
            else if (tNode.clickAction == dnn.controls.action.nav)
                dnn.dom.navigate(tNode.url, tNode.target.length > 0 ? tNode.target : this.target);
            else if (tNode.clickAction == dnn.controls.action.expand)
            {
                if (tNode.hasNodes || tNode.hasPendingNodes)
                {
                    if (tNode.expanded)
                        this.collapseNode(tNode);
                    else
                        this.expandNode(tNode);
                }
            }
        }

		logger.warn(`<<< Exiting selectNode(tNode=${tNode})`, { "event.action": "selectNode", "logrite.tNode": tNode });
        return true;
    },

    selectAllChildren: function(tNode)
    {
		logger.error(`>>> Entering selectAllChildren(tNode=${tNode})`, { "event.action": "selectAllChildren", "logrite.tNode": tNode });
        var childTNode;
        for (var i = 0; i < tNode.childNodeCount(); i++)
        {
            childTNode = tNode.childNodes(i);
            if (childTNode.selected != tNode.selected)
                this.selectNode(childTNode);

			logger.debug(`selectAllChildren(tNode=${tNode}): childTNode → ${childTNode}`, { "event.action": "selectAllChildren", "logrite.tNode": tNode });
            this.selectAllChildren(childTNode);
			logger.warn(`<<< Exiting selectAllChildren(tNode=${tNode})`, { "event.action": "selectAllChildren", "logrite.tNode": tNode });
        }
    },

    assignCss: function(tNode)
    {
		logger.error(`>>> Entering assignCss(tNode=${tNode})`, { "event.action": "assignCss", "logrite.tNode": tNode });
        var oText = this.getChildControl(tNode.id, 't'); //, this.container);
        var sNodeCss = this.css;
		logger.error(`assignCss(tNode=${tNode}): oText → ${oText}`, { "event.action": "assignCss", "logrite.tNode": tNode });

        if (tNode.level > 0 && this.cssChild.length > 0)
            sNodeCss = this.cssChild;

        if (tNode.css.length > 0)
            sNodeCss = tNode.css;

        //tNode.hoverCss;

        if (tNode.hover)
            sNodeCss += ' ' + (tNode.cssHover.length > 0 ? tNode.cssHover : this.cssHover);
        if (tNode.selected)
            sNodeCss += ' ' + (tNode.cssSel.length > 0 ? tNode.cssSel : this.cssSel);

        oText.className = sNodeCss;
		logger.debug(`assignCss(tNode=${tNode}): oText.className → ${oText.className}`, { "event.action": "assignCss", "logrite.tNode": tNode });
    },

    update: function(force)
    {
		logger.error(`>>> Entering update(force=${force})`, { "event.action": "update", "logrite.force": force });
        if (force)
        {
            if (this.selTreeNode)
                dnn.setVar(this.ns + ':selected', this.selTreeNode.id); 	//BACKWARDS COMPAT ONLY!!!    
            dnn.setVar(this.ns + '_json', this.rootNode.getJSON());
			logger.warn(`<<< Exiting update(force=${force})`, { "event.action": "update", "logrite.force": force });
        }
        return true;
    },

    //--- Event Handlers ---//
    _onsubmit: function()
    {
		logger.error('>>> Entering _onsubmit()', { "event.action": "_onsubmit" });
        this.update(true);
		logger.warn('<<< Exiting _onsubmit()', { "event.action": "_onsubmit" });
    },

    callBackStatus: function(result, ctx, req)
    {
		logger.error(`>>> Entering callBackStatus(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackStatus", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        var tNode = ctx;
        var tree = tNode.tree;
        tree.invoke_compatHandler('callBackStatus', result, ctx, req);
		logger.warn(`<<< Exiting callBackStatus(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackStatus", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },

    callBackSuccess: function(result, ctx, req)
    {
		logger.error(`>>> Entering callBackSuccess(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        var tNode = ctx;
        var tree = tNode.tree;
        var parent = tNode.node; ;
		logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): parent → ${parent}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
		logger.debug(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): parent.nodes → ${parent.nodes}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });

        var json = dnn.evalJSON("{" + result + "}");
		logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): json → ${json}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        parent.nodes = json.nodes;
        parent.setupJSONNodes(parent.rootNode(), parent, parent.nodes);

        if (tree.workImg != null)
        {
            var icon = tree.getChildControl(tNode.id, 'icn');
			logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): icon → ${icon}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
			logger.debug(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): icon.src → ${icon.src}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
            if (tNode.image != '')
                icon.src = tNode.image;
            else if (tNode.imageIndex > -1)
                icon.src = tree.imageList[tNode.imageIndex];
        }

        //ctx.tree.generateTreeHTML();	
        //get container
        var ctr = tree.getChildControl(tNode.id, 'pctr');
		logger.error(`callBackSuccess(result=${result},ctx=${ctx},req=${req}): ctr → ${ctr}`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        tree.renderNode(tNode.node, ctr, true);

        tree.update();

        //var ctr = tree.getChildControl(tNode.id, 'pctr');
        tree.expandNode(new dnn.controls.DNNTreeNode(tNode.node));

        tree.invoke_compatHandler('callBackStatus', result, ctx, req);
        tree.invoke_handler('callBackSuccess', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
		logger.warn(`<<< Exiting callBackSuccess(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackSuccess", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },

    callBackFail: function(result, ctx, req)
    {
		logger.error(`>>> Entering callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
        var tNode = ctx;
        var tree = tNode.tree;
        tree.invoke_handler('callBackFail', new dnn.controls.DNNCallbackEventArgs(result, ctx, req));
		logger.warn(`<<< Exiting callBackFail(result=${result},ctx=${ctx},req=${req})`, { "event.action": "callBackFail", "logrite.result": result, "logrite.ctx": ctx, "logrite.req": req });
    },

    nodeExpColClick: function(evt)
    {
		logger.error(`>>> Entering nodeExpColClick(evt=${evt})`, { "event.action": "nodeExpColClick", "logrite.evt": evt });
        var node = this._findEventNode(evt);
		logger.error(`nodeExpColClick(evt=${evt}): node → ${node}`, { "event.action": "nodeExpColClick", "logrite.evt": evt });
        if (node != null)
        {
            var tNode = new dnn.controls.DNNTreeNode(node);
			logger.error(`nodeExpColClick(evt=${evt}): tNode → ${tNode}`, { "event.action": "nodeExpColClick", "logrite.evt": evt });

            var ctr = this.getChildControl(tNode.id, 'pctr');
            if (tNode.expanded)
                this.collapseNode(tNode);
            else
                this.expandNode(tNode);
        }
    },

    nodeCheck: function(evt)
    {
		logger.error(`>>> Entering nodeCheck(evt=${evt})`, { "event.action": "nodeCheck", "logrite.evt": evt });
        var node = this._findEventNode(evt);
		logger.error(`nodeCheck(evt=${evt}): node → ${node}`, { "event.action": "nodeCheck", "logrite.evt": evt });
        if (node != null)
        {
            var tNode = new dnn.controls.DNNTreeNode(node);
			logger.error(`nodeCheck(evt=${evt}): tNode → ${tNode}`, { "event.action": "nodeCheck", "logrite.evt": evt });
            if (this.checkBoxMode == 2 && this.selTreeNode != null)
                this.selectNode(this.selTreeNode);   //unselect node

            this.selectNode(tNode);

            if (this.checkBoxMode == 1)
                this.selectAllChildren(tNode);

		logger.warn(`<<< Exiting nodeCheck(evt=${evt})`, { "event.action": "nodeCheck", "logrite.evt": evt });
        }
    },

    nodeTextClick: function(evt)
    {
		logger.error(`>>> Entering nodeTextClick(evt=${evt})`, { "event.action": "nodeTextClick", "logrite.evt": evt });
        var node = this._findEventNode(evt);
		logger.error(`nodeTextClick(evt=${evt}): node → ${node}`, { "event.action": "nodeTextClick", "logrite.evt": evt });
        if (node != null)
        {
            this.selectNode(new dnn.controls.DNNTreeNode(node));
			logger.warn(`<<< Exiting nodeTextClick(evt=${evt})`, { "event.action": "nodeTextClick", "logrite.evt": evt });
        }
    },

    nodeTextMOver: function(evt)
    {
		logger.error(`>>> Entering nodeTextMOver(evt=${evt})`, { "event.action": "nodeTextMOver", "logrite.evt": evt });
        var node = this._findEventNode(evt);
        if (node != null)
            this.hoverNode(new dnn.controls.DNNTreeNode(node));
			logger.warn(`<<< Exiting nodeTextMOver(evt=${evt})`, { "event.action": "nodeTextMOver", "logrite.evt": evt });
    },

    nodeTextMOut: function(evt)
    {
		logger.error(`>>> Entering nodeTextMOut(evt=${evt})`, { "event.action": "nodeTextMOut", "logrite.evt": evt });
        var node = this._findEventNode(evt);
        if (node != null)
            this.assignCss(new dnn.controls.DNNTreeNode(node));
			logger.warn(`<<< Exiting nodeTextMOut(evt=${evt})`, { "event.action": "nodeTextMOut", "logrite.evt": evt });
    },

    dispose: function()
    {
		logger.error('>>> Entering dispose()', { "event.action": "dispose" });
        this._onsubmitDelegate = null;
        dnn.controls.DNNTree.callBaseMethod(this, 'dispose');
		logger.debug(`dispose(): this._onsubmitDelegate → ${this._onsubmitDelegate}`, { "event.action": "dispose" });
    },

    //--- Generates tree HTML through passed in XML DOM ---//
    generateTreeHTML: function()
    {
		logger.error('>>> Entering generateTreeHTML()', { "event.action": "generateTreeHTML" });
        //this.debugWrite('generateTreeHTML', 'generateTreeHTML');
        //this.rootNode = this.DOM.rootNode();
        this.renderNode(null, this.container);
		logger.warn('<<< Exiting generateTreeHTML()', { "event.action": "generateTreeHTML" });
        //this.debugWrite('generateTreeHTML', 'generateTreeHTML [END]');	
    },

    renderNode: function(node, oCont, bExists)
    {
		logger.error(`>>> Entering renderNode(node=${node},oCont=${oCont},bExists=${bExists})`, { "event.action": "renderNode", "logrite.node": node, "logrite.oCont": oCont, "logrite.bExists": bExists });
        var oChildCont = oCont;
        var tNode;
		logger.error(`renderNode(node=${node},oCont=${oCont},bExists=${bExists}): oChildCont → ${oChildCont}`, { "event.action": "renderNode", "logrite.node": node, "logrite.oCont": oCont, "logrite.bExists": bExists });

        if (bExists != true)
        {
            if (node != null)
            {
                //render node
                tNode = new dnn.controls.DNNTreeNode(node);
                var oNewContainer;
				logger.error(`renderNode(node=${node},oCont=${oCont},bExists=${bExists}): tNode → ${tNode}`, { "event.action": "renderNode", "logrite.node": node, "logrite.oCont": oCont, "logrite.bExists": bExists });
				logger.debug(`renderNode(node=${node},oCont=${oCont},bExists=${bExists}): tNode → ${tNode}`, { "event.action": "renderNode", "logrite.node": node, "logrite.oCont": oCont, "logrite.bExists": bExists });
                oNewContainer = this.createChildControl('DIV', tNode.id, 'ctr'); //container for Node
				logger.error(`renderNode(node=${node},oCont=${oCont},bExists=${bExists}): oNewContainer → ${oNewContainer}`, { "event.action": "renderNode", "logrite.node": node, "logrite.oCont": oCont, "logrite.bExists": bExists });
                oNewContainer.appendChild(this.renderSpacer((this.indentWidth * tNode.level) + ((tNode.hasNodes || tNode.hasPendingNodes) ? 0 : this.expImgWidth))); //indent node
                if (tNode.hasNodes || tNode.hasPendingNodes)	//if node has children then render expand/collapse icon
                    oNewContainer.appendChild(this.renderExpCol(tNode));

                if (this.checkBoxes)
                    oNewContainer.appendChild(this.renderCheckbox(tNode));

                var oIconCont = this.renderIconCont(tNode);
				logger.error(`renderNode(node=${node},oCont=${oCont},bExists=${bExists}): oIconCont → ${oIconCont}`, { "event.action": "renderNode", "logrite.node": node, "logrite.oCont": oCont, "logrite.bExists": bExists });
                oNewContainer.appendChild(oIconCont);
                if (tNode.imageIndex > -1 || tNode.image != '')	//if node has image 
                {
                    oIconCont.appendChild(this.renderIcon(tNode));
                    //oNewContainer.appendChild(this.renderSpacer(10));
                }
                //else
                //    oIconCont.appendChild(this.renderSpacer(this.indentWidth));

                oNewContainer.appendChild(this.renderText(tNode)); //render text
                oCont.appendChild(oNewContainer);
                this.assignCss(tNode);
            }
            else
                node = this.rootNode;

            if (tNode != null && (tNode.hasNodes || tNode.hasPendingNodes))	//if node has children render container and hide if necessary
            {
                oChildCont = this.createChildControl('DIV', tNode.id, 'pctr'); //Not using SPAN due to FireFox bug...
                if (tNode.expanded != true)
                    oChildCont.style.display = 'none';
                oCont.appendChild(oChildCont);
            }
        }

        for (var i = 0; i < node.childNodeCount(); i++)	//recursively call child rendering
            this.renderNode(node.childNodes(i), oChildCont);


    },

    renderExpCol: function(tNode)
    {
		logger.error(`>>> Entering renderExpCol(tNode=${tNode})`, { "event.action": "renderExpCol", "logrite.tNode": tNode });
        var img = this.createChildControl('IMG', tNode.id, 'expcol');
		logger.error(`renderExpCol(tNode=${tNode}): img → ${img}`, { "event.action": "renderExpCol", "logrite.tNode": tNode });
        if ((tNode.hasNodes || tNode.hasPendingNodes) && this.expandImg.length)
        {
            if (tNode.expanded)
                img.src = this.expandImg;
            else
                img.src = this.collapseImg;
        }
        //img.style.width = this.expImgWidth;	
        //img.style.cursor = 'hand';	//ie
        img.style.cursor = 'pointer';
        //dnn.dom.attachEvent(img, 'onclick', this.nodeExpColClick);
        //dnn.dom.attachEvent(img, 'onclick', dnn.dom.getObjMethRef(this, 'nodeExpColClick'));
        //img.onclick = dnn.dom.getObjMethRef(this, 'nodeExpColClick');
        this.addHandlers(img, { "click": this.nodeExpColClick }, this);

		logger.warn(`<<< Exiting renderExpCol(tNode=${tNode})`, { "event.action": "renderExpCol", "logrite.tNode": tNode });
        return img;
    },

    renderIconCont: function(tNode)
    {
		logger.error(`>>> Entering renderIconCont(tNode=${tNode})`, { "event.action": "renderIconCont", "logrite.tNode": tNode });
        var span = this.createChildControl('SPAN', tNode.id, 'icnc');
        if (tNode.cssIcon.length > 0)
            span.className = tNode.cssIcon;
        else if (this.cssIcon.length > 0)
            span.className = this.cssIcon;
			logger.debug(`renderIconCont(tNode=${tNode}): span.className → ${span.className}`, { "event.action": "renderIconCont", "logrite.tNode": tNode });

		logger.warn(`<<< Exiting renderIconCont(tNode=${tNode})`, { "event.action": "renderIconCont", "logrite.tNode": tNode });
        return span;
    },

    renderIcon: function(tNode)
    {
		logger.error(`>>> Entering renderIcon(tNode=${tNode})`, { "event.action": "renderIcon", "logrite.tNode": tNode });
        var img = this.createChildControl('IMG', tNode.id, 'icn');
        if (tNode.image != '')
            img.src = tNode.image;
        else if (tNode.imageIndex > -1)
            img.src = this.imageList[tNode.imageIndex];
        //img.style.paddingRight = 10;	//doesn't work in IE???
		logger.debug(`renderIcon(tNode=${tNode}): img.src → ${img.src}`, { "event.action": "renderIcon", "logrite.tNode": tNode });
        return img;
    },

    renderCheckbox: function(tNode)
    {
		logger.error(`>>> Entering renderCheckbox(tNode=${tNode})`, { "event.action": "renderCheckbox", "logrite.tNode": tNode });
        var chk = this.createChildControl('INPUT', tNode.id, 'chk');
        chk.type = 'checkbox';
		logger.error(`renderCheckbox(tNode=${tNode}): chk → ${chk}`, { "event.action": "renderCheckbox", "logrite.tNode": tNode });
		logger.debug(`renderCheckbox(tNode=${tNode}): chk.type → ${chk.type}`, { "event.action": "renderCheckbox", "logrite.tNode": tNode });
		logger.debug(`renderCheckbox(tNode=${tNode}): chk.checked → ${chk.checked}`, { "event.action": "renderCheckbox", "logrite.tNode": tNode });
        chk.defaultChecked = tNode.selected;
		logger.debug(`renderCheckbox(tNode=${tNode}): chk.defaultChecked → ${chk.defaultChecked}`, { "event.action": "renderCheckbox", "logrite.tNode": tNode });
        chk.checked = tNode.selected;

        this.addHandlers(chk, { "click": this.nodeCheck }, this);
		logger.warn(`<<< Exiting renderCheckbox(tNode=${tNode})`, { "event.action": "renderCheckbox", "logrite.tNode": tNode });
        return chk;
    },

    renderSpacer: function(width)
    {
		logger.error(`>>> Entering renderSpacer(width=${width})`, { "event.action": "renderSpacer", "logrite.width": width });
        var img = document.createElement('IMG');
        img.src = this.sysImgPath + 'spacer.gif';
		logger.error(`renderSpacer(width=${width}): img → ${img}`, { "event.action": "renderSpacer", "logrite.width": width });
		logger.debug(`renderSpacer(width=${width}): img.src → ${img.src}`, { "event.action": "renderSpacer", "logrite.width": width });
		logger.debug(`renderSpacer(width=${width}): img.height → ${img.height}`, { "event.action": "renderSpacer", "logrite.width": width });
        img.width = width;
		logger.debug(`renderSpacer(width=${width}): img.width → ${img.width}`, { "event.action": "renderSpacer", "logrite.width": width });
        img.height = 1;
        img.style.width = width + 'px';
        img.style.height = '1px';
		logger.warn(`<<< Exiting renderSpacer(width=${width})`, { "event.action": "renderSpacer", "logrite.width": width });
        return img;
    },

    renderText: function(tNode)
    {
		logger.error(`>>> Entering renderText(tNode=${tNode})`, { "event.action": "renderText", "logrite.tNode": tNode });
        var span = this.createChildControl('SPAN', tNode.id, 't');
        span.innerHTML = tNode.text;
		logger.error(`renderText(tNode=${tNode}): span → ${span}`, { "event.action": "renderText", "logrite.tNode": tNode });
		logger.debug(`renderText(tNode=${tNode}): span.innerHTML → ${span.innerHTML}`, { "event.action": "renderText", "logrite.tNode": tNode });
		logger.debug(`renderText(tNode=${tNode}): span.title → ${span.title}`, { "event.action": "renderText", "logrite.tNode": tNode });
        span.style.cursor = 'pointer';

        if (tNode.toolTip.length > 0)
            span.title = tNode.toolTip;

        if (tNode.enabled || tNode.clickAction == dnn.controls.action.expand)
        {
            //span.onclick = dnn.dom.getObjMethRef(this, 'nodeTextClick');
            if (this.checkBoxes)
                this.addHandlers(span, { "click": this.nodeCheck }, this);
            else
                this.addHandlers(span, { "click": this.nodeTextClick }, this);

            if (this.cssHover.length > 0)	//only do this if necessary
            {
                //span.onmouseover = dnn.dom.getObjMethRef(this, 'nodeTextMOver');
                //span.onmouseout = dnn.dom.getObjMethRef(this, 'nodeTextMOut');
                this.addHandlers(span, { "mouseover": this.nodeTextMOver,
                    "mouseout": this.nodeTextMOut
                }, this);
            }
        }

        if (tNode.selected)
        {
            this.selTreeNode = tNode;
            this.hoverTreeNode = tNode;
			logger.debug(`renderText(tNode=${tNode}): this.selTreeNode → ${this.selTreeNode}`, { "event.action": "renderText", "logrite.tNode": tNode });
        }

		logger.debug(`renderText(tNode=${tNode}): this.hoverTreeNode → ${this.hoverTreeNode}`, { "event.action": "renderText", "logrite.tNode": tNode });
		logger.warn(`<<< Exiting renderText(tNode=${tNode})`, { "event.action": "renderText", "logrite.tNode": tNode });
        return span;
    },

    _findEventNode: function(evt)
    {
		logger.error(`>>> Entering _findEventNode(evt=${evt})`, { "event.action": "_findEventNode", "logrite.evt": evt });
		logger.warn(`<<< Exiting _findEventNode(evt=${evt})`, { "event.action": "_findEventNode", "logrite.evt": evt });
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
    }


    /*get_DebugMode: function() {return this._debugMode;},
    set_DebugMode: function(value) 
    {
    this._debugMode = value;
    if (this._debugMode && this._debugControl == null)
    {
    this._debugControl = document.createElement('textarea');
    this._debugControl.rows = 20;
    this._debugControl.cols = 150;
    document.forms[0].appendChild(this._debugControl);
    }
    else if (this._debugControl)
    {
    this._debugControl.style.display = 'none';
    }
    },

_debugWrite: function()
    {
    this._debugControl.value += this._debugBuffer;
    this._debugBuffer = '';
    },

debugWrite: function(category, text)
    {
    if (this._debugMode)
    {
    if (this._debugControl == null)
    {
    this.set_DebugMode(this._debugMode);
    }
    
    var s = text;
    var d = new Date();
    if (this._debugTimes[category] && text.indexOf('[END]') > -1)
    {
    s += ' (' + ((d - this._debugTimes[category])/ 1000) + ')'; 
    }
    this._debugTimes[category] = d;
    this._debugBuffer += s + '\n';
    if (this._prevDebugTimeout)
    {
    window.clearTimeout(this._prevDebugTimeout);
    }
    this._prevDebugTimeout = window.setTimeout(Function.createDelegate(this, this._debugWrite), 1000);
    }
    }*/

} //END DNNTree
    dnn.controls.DNNTree.registerClass('dnn.controls.DNNTree', dnn.controls.control);

    dnn.controls.DNNTreeNode = function(node)
    {
        dnn.controls.DNNTreeNode.initializeBase(this, [node]);

        //tree specific attributes
        this.hover = false;
        this.expanded = node.getAttribute('expanded', '0') == '1' ? true : null;
        this.selected = node.getAttribute('selected', '0') == '1' ? true : null;
        this.clickAction = node.getAttribute('ca', dnn.controls.action.postback);
        this.imageIndex = new Number(node.getAttribute('imgIdx', '-1')); //defaulting to 0 for backwards compat!

        //this.checkBox = node.getAttribute('checkBox', '0');	//IS THIS NECESSARY?
    }

    //DNNTreeNode specific methods
    dnn.controls.DNNTreeNode.prototype =
{
    childNodes: function(iIndex)
    {
		logger.error(`>>> Entering childNodes(iIndex=${iIndex})`, { "event.action": "childNodes", "logrite.iIndex": iIndex });
        if (this.node.childNodes(iIndex) != null)
            return new dnn.controls.DNNTreeNode(this.node.childNodes(iIndex));
    }
}
    dnn.controls.DNNTreeNode.registerClass('dnn.controls.DNNTreeNode', dnn.controls.DNNNode);


    //BACKWARDS COMPAT ONLY!
    var DT_ACTION_POSTBACK = 0;
    var DT_ACTION_EXPAND = 1;
    var DT_ACTION_NONE = 2;
    var DT_ACTION_NAV = 3;

    function __dt_DNNTreeNode(ctl)
    {
		logger.error(`>>> Entering __dt_DNNTreeNode(ctl=${ctl})`, { "event.action": "__dt_DNNTreeNode", "logrite.ctl": ctl });
        var node = dnn.controls.controls[ctl.ns].rootNode.findNode(ctl.nodeid);
		logger.error(`__dt_DNNTreeNode(ctl=${ctl}): node → ${node}`, { "event.action": "__dt_DNNTreeNode", "logrite.ctl": ctl });
        if (node != null)
        {
            var tNode = new dnn.controls.DNNTreeNode(node);
			logger.error(`__dt_DNNTreeNode(ctl=${ctl}): tNode → ${tNode}`, { "event.action": "__dt_DNNTreeNode", "logrite.ctl": ctl });

            this.ctl = ctl;
            this.id = ctl.id;
			logger.debug(`__dt_DNNTreeNode(ctl=${ctl}): ctl → ${ctl}`, { "event.action": "__dt_DNNTreeNode", "logrite.ctl": ctl });
			logger.debug(`__dt_DNNTreeNode(ctl=${ctl}): this.ctl → ${this.ctl}`, { "event.action": "__dt_DNNTreeNode", "logrite.ctl": ctl });
			logger.debug(`__dt_DNNTreeNode(ctl=${ctl}): this.id → ${this.id}`, { "event.action": "__dt_DNNTreeNode", "logrite.ctl": ctl });
            this.key = tNode.key;
            this.nodeID = ctl.nodeid; //trim off t
            this.text = tNode.text;
            this.serverName = ctl.name;
			logger.debug(`__dt_DNNTreeNode(ctl=${ctl}): this.key → ${this.key}`, { "event.action": "__dt_DNNTreeNode", "logrite.ctl": ctl });
			logger.debug(`__dt_DNNTreeNode(ctl=${ctl}): this.nodeID → ${this.nodeID}`, { "event.action": "__dt_DNNTreeNode", "logrite.ctl": ctl });
        }
    }


