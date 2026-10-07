var logger = (typeof logger !== 'undefined' && logger) || console;
/// <reference name="MicrosoftAjax.js" />
/// <reference name="dnn.js" assembly="DotNetNuke.WebUtility" />

Type.registerNamespace('dnn.dom.positioning');

dnn.extend(dnn.dom.positioning, {
    pns: 'dnn.dom',
    ns: 'positioning',
    dragCtr: null,
    dragCtrDims: null,

    bodyScrollLeft: function()
    {
		logger.error('>>> Entering bodyScrollLeft()', { "event.action": "bodyScrollLeft" });
        if (window.pageYOffset)
            return window.pageYOffset;

        var oBody = (document.compatMode && document.compatMode != "BackCompat") ? document.documentElement : document.body;
		logger.warn('<<< Exiting bodyScrollLeft()', { "event.action": "bodyScrollLeft" });
		logger.error(`bodyScrollLeft(): oBody → ${oBody}`, { "event.action": "bodyScrollLeft" });
        return oBody.scrollLeft;
    },

    bodyScrollTop: function()
    {
		logger.error('>>> Entering bodyScrollTop()', { "event.action": "bodyScrollTop" });
        if (window.pageXOffset)
            return window.pageXOffset;

        var oBody = (document.compatMode && document.compatMode != "BackCompat") ? document.documentElement : document.body;
		logger.warn('<<< Exiting bodyScrollTop()', { "event.action": "bodyScrollTop" });
		logger.error(`bodyScrollTop(): oBody → ${oBody}`, { "event.action": "bodyScrollTop" });
        return oBody.scrollTop;
    },

    viewPortHeight: function()
    {
		logger.error('>>> Entering viewPortHeight()', { "event.action": "viewPortHeight" });
        // supported in Mozilla, Opera, and Safari
        if (window.innerHeight)
            return window.innerHeight;

        var oBody = (document.compatMode && document.compatMode != "BackCompat") ? document.documentElement : document.body;
		logger.warn('<<< Exiting viewPortHeight()', { "event.action": "viewPortHeight" });
		logger.error(`viewPortHeight(): oBody → ${oBody}`, { "event.action": "viewPortHeight" });
        return oBody.clientHeight;
    },

    viewPortWidth: function()
    {
		logger.error('>>> Entering viewPortWidth()', { "event.action": "viewPortWidth" });
        // supported in Mozilla, Opera, and Safari
        if (window.innerWidth)
            return window.innerWidth;

        var oBody = (document.compatMode && document.compatMode != "BackCompat") ? document.documentElement : document.body;
		logger.warn('<<< Exiting viewPortWidth()', { "event.action": "viewPortWidth" });
		logger.error(`viewPortWidth(): oBody → ${oBody}`, { "event.action": "viewPortWidth" });
        return oBody.clientWidth;
    },

    dragContainer: function(oCtl, e)
    {
        var iNewLeft = 0;
        var iNewTop = 0;
        //var e = dnn.dom.event.object;
        var oCont = dnn.dom.getById(oCtl.contID);
        var oTitle = dnn.dom.positioning.dragCtr;
        var iScrollTop = this.bodyScrollTop();
        var iScrollLeft = this.bodyScrollLeft();

        if (oCtl.startLeft == null)
            oCtl.startLeft = e.clientX - this.elementLeft(oCont) + iScrollLeft;

        if (oCtl.startTop == null)
            oCtl.startTop = e.clientY - this.elementTop(oCont) + iScrollTop;

        if (oCont.style.position == 'relative')
            oCont.style.position = 'absolute';

        iNewLeft = e.clientX - oCtl.startLeft + iScrollLeft;
        iNewTop = e.clientY - oCtl.startTop + iScrollTop;

        if (iNewLeft > this.elementWidth(document.forms[0]))// this.viewPortWidth() + iScrollLeft)
            iNewLeft = this.elementWidth(document.forms[0]); //this.viewPortWidth() + iScrollLeft;

        if (iNewTop > this.elementHeight(document.forms[0])) //this.viewPortHeight() + iScrollTop)
            iNewTop = this.elementHeight(document.forms[0]); //this.viewPortHeight() + iScrollTop;

        oCont.style.left = iNewLeft + 'px';
        oCont.style.top = iNewTop + 'px';

        if (oTitle != null && oTitle.dragOver != null)
            eval(oCtl.dragOver);
			logger.warn(`<<< Exiting dragContainer(oCtl=${oCtl},e=${e})`, { "event.action": "dragContainer", "logrite.oCtl": oCtl, "logrite.e": e });
    },

    elementHeight: function(eSrc)
    {
		logger.error(`>>> Entering elementHeight(eSrc=${eSrc})`, { "event.action": "elementHeight", "logrite.eSrc": eSrc });
        if (eSrc.offsetHeight == null || eSrc.offsetHeight == 0)
        {
            if (eSrc.offsetParent == null)
                return 0;
            if (eSrc.offsetParent.offsetHeight == null || eSrc.offsetParent.offsetHeight == 0)
            {
                if (eSrc.offsetParent.offsetParent != null)
                    return eSrc.offsetParent.offsetParent.offsetHeight; //needed for Konqueror
                else
                    return 0;
            }
            else
                return eSrc.offsetParent.offsetHeight;
        }
        else
            return eSrc.offsetHeight;
    },

    elementLeft: function(eSrc)
    {
		logger.error(`>>> Entering elementLeft(eSrc=${eSrc})`, { "event.action": "elementLeft", "logrite.eSrc": eSrc });
		logger.warn(`<<< Exiting elementLeft(eSrc=${eSrc})`, { "event.action": "elementLeft", "logrite.eSrc": eSrc });
        return this.elementPos(eSrc).l;
    },

    elementOverlapScore: function(oDims1, oDims2)
    {
		logger.error(`>>> Entering elementOverlapScore(oDims1=${oDims1},oDims2=${oDims2})`, { "event.action": "elementOverlapScore", "logrite.oDims1": oDims1, "logrite.oDims2": oDims2 });
        var iLeftScore = 0;
        var iTopScore = 0;
        if (oDims1.l <= oDims2.l && oDims2.l <= oDims1.r)	//if left of content fits between panel borders
            iLeftScore += (oDims1.r < oDims2.r ? oDims1.r : oDims2.r) - oDims2.l; //set score based off left of content to closest right border
        if (oDims2.l <= oDims1.l && oDims1.l <= oDims2.r)	//if left of panel fits between content borders
            iLeftScore += (oDims2.r < oDims1.r ? oDims2.r : oDims1.r) - oDims1.l; //set score based off left of panel to closest right border
        if (oDims1.t <= oDims2.t && oDims2.t <= oDims1.b)	//if top of content fits between panel borders
            iTopScore += (oDims1.b < oDims2.b ? oDims1.b : oDims2.b) - oDims2.t; //set score based off top of content to closest bottom border
        if (oDims2.t <= oDims1.t && oDims1.t <= oDims2.b)	//if top of panel fits between content borders
            iTopScore += (oDims2.b < oDims1.b ? oDims2.b : oDims1.b) - oDims1.t; //set score based off top of panel to closest bottom border

		logger.warn(`<<< Exiting elementOverlapScore(oDims1=${oDims1},oDims2=${oDims2})`, { "event.action": "elementOverlapScore", "logrite.oDims1": oDims1, "logrite.oDims2": oDims2 });
        return iLeftScore * iTopScore;
    },

    elementTop: function(eSrc)
    {
		logger.error(`>>> Entering elementTop(eSrc=${eSrc})`, { "event.action": "elementTop", "logrite.eSrc": eSrc });
		logger.warn(`<<< Exiting elementTop(eSrc=${eSrc})`, { "event.action": "elementTop", "logrite.eSrc": eSrc });
        return this.elementPos(eSrc).t;
    },

    elementPos: function(eSrc)
    {
		logger.error(`>>> Entering elementPos(eSrc=${eSrc})`, { "event.action": "elementPos", "logrite.eSrc": eSrc });
        var oPos = new Object();
        oPos.t = 0; //relative top
		logger.error(`elementPos(eSrc=${eSrc}): oPos → ${oPos}`, { "event.action": "elementPos", "logrite.eSrc": eSrc });
		logger.debug(`elementPos(eSrc=${eSrc}): oPos.t → ${oPos.t}`, { "event.action": "elementPos", "logrite.eSrc": eSrc });
		logger.debug(`elementPos(eSrc=${eSrc}): oPos.at → ${oPos.at}`, { "event.action": "elementPos", "logrite.eSrc": eSrc });
		logger.debug(`elementPos(eSrc=${eSrc}): oPos.al → ${oPos.al}`, { "event.action": "elementPos", "logrite.eSrc": eSrc });
        oPos.l = 0; //relative left
		logger.debug(`elementPos(eSrc=${eSrc}): oPos.l → ${oPos.l}`, { "event.action": "elementPos", "logrite.eSrc": eSrc });
        oPos.at = 0; //actual top
        oPos.al = 0; //actual left

        var eParent = eSrc;
        var style;
        var srcId = eSrc.id;
        if (srcId != null && srcId.length == 0)
            srcId = null;

        if (eSrc.style.position == 'absolute')
        {
            oPos.t = eParent.offsetTop;
            oPos.l = eParent.offsetLeft;
        }
        while (eParent != null)
        {
            oPos.at += eParent.offsetTop;
            oPos.al += eParent.offsetLeft;
            if (eSrc.style.position != 'absolute')
            {
                if (eParent.currentStyle)
                    style = eParent.currentStyle;
                else
                    style = Sys.UI.DomElement._getCurrentStyle(eParent);

				logger.error(`elementPos(eSrc=${eSrc}): style → ${style}`, { "event.action": "elementPos", "logrite.eSrc": eSrc });
                if (eParent.id == srcId || style.position != 'relative')
                {
                    oPos.t += eParent.offsetTop;
                    oPos.l += eParent.offsetLeft;
                }
            }

            eParent = eParent.offsetParent;
            if (eParent == null || (eParent.tagName.toUpperCase() == "BODY" && dnn.dom.browser.isType(dnn.dom.browser.Konqueror)))  //safari no longer needed here
                break;
        }

		logger.warn(`<<< Exiting elementPos(eSrc=${eSrc})`, { "event.action": "elementPos", "logrite.eSrc": eSrc });
        return oPos;
    },

    elementWidth: function(eSrc)
    {
		logger.error(`>>> Entering elementWidth(eSrc=${eSrc})`, { "event.action": "elementWidth", "logrite.eSrc": eSrc });
        if (eSrc.offsetWidth == null || eSrc.offsetWidth == 0)
        {
            if (eSrc.offsetParent == null)
                return 0;
            if (eSrc.offsetParent.offsetWidth == null || eSrc.offsetParent.offsetWidth == 0)
            {
                if (eSrc.offsetParent.offsetParent != null)
                    return eSrc.offsetParent.offsetParent.offsetWidth; //needed for Konqueror
                else
                    return 0;
            }
            else
                return eSrc.offsetParent.offsetWidth

        }
        else
            return eSrc.offsetWidth;
    },

    enableDragAndDrop: function(oContainer, oTitle, sDragCompleteEvent, sDragOverEvent)
    {
		logger.error(`>>> Entering enableDragAndDrop(oContainer=${oContainer},oTitle=${oTitle},sDragCompleteEvent=${sDragCompleteEvent},sDragOverEvent=${sDragOverEvent})`, { "event.action": "enableDragAndDrop", "logrite.oContainer": oContainer, "logrite.oTitle": oTitle, "logrite.sDragCompleteEvent": sDragCompleteEvent, "logrite.sDragOverEvent": sDragOverEvent });
        //dnn.dom.attachEvent(document.body, 'onmousemove', __dnn_bodyMouseMove);
        dnn.dom.addSafeHandler(document.body, 'onmousemove', dnn.dom.positioning, '__dnn_bodyMouseMove');
        //dnn.dom.attachEvent(document.body, 'onmouseup', __dnn_bodyMouseUp);
        dnn.dom.addSafeHandler(document.body, 'onmouseup', dnn.dom.positioning, '__dnn_bodyMouseUp');
        //dnn.dom.attachEvent(oTitle, 'onmousedown', __dnn_containerMouseDownDelay);
        dnn.dom.addSafeHandler(oTitle, 'onmousedown', dnn.dom.positioning, '__dnn_containerMouseDownDelay');

        if (dnn.dom.browser.type == dnn.dom.browser.InternetExplorer)
            oTitle.style.cursor = 'hand';
        else
            oTitle.style.cursor = 'pointer';

        if (oContainer.id.length == 0)
            oContainer.id = oTitle.id + '__dnnCtr';

		logger.debug(`enableDragAndDrop(oContainer=${oContainer},oTitle=${oTitle},sDragCompleteEvent=${sDragCompleteEvent},sDragOverEvent=${sDragOverEvent}): oContainer.id → ${oContainer.id}`, { "event.action": "enableDragAndDrop", "logrite.oContainer": oContainer, "logrite.oTitle": oTitle, "logrite.sDragCompleteEvent": sDragCompleteEvent, "logrite.sDragOverEvent": sDragOverEvent });
        oTitle.contID = oContainer.id;
		logger.debug(`enableDragAndDrop(oContainer=${oContainer},oTitle=${oTitle},sDragCompleteEvent=${sDragCompleteEvent},sDragOverEvent=${sDragOverEvent}): oTitle.contID → ${oTitle.contID}`, { "event.action": "enableDragAndDrop", "logrite.oContainer": oContainer, "logrite.oTitle": oTitle, "logrite.sDragCompleteEvent": sDragCompleteEvent, "logrite.sDragOverEvent": sDragOverEvent });
        if (sDragCompleteEvent != null)
            oTitle.dragComplete = sDragCompleteEvent;
        if (sDragOverEvent != null)
            oTitle.dragOver = sDragOverEvent;

		logger.warn(`<<< Exiting enableDragAndDrop(oContainer=${oContainer},oTitle=${oTitle},sDragCompleteEvent=${sDragCompleteEvent},sDragOverEvent=${sDragOverEvent})`, { "event.action": "enableDragAndDrop", "logrite.oContainer": oContainer, "logrite.oTitle": oTitle, "logrite.sDragCompleteEvent": sDragCompleteEvent, "logrite.sDragOverEvent": sDragOverEvent });
		logger.debug(`enableDragAndDrop(oContainer=${oContainer},oTitle=${oTitle},sDragCompleteEvent=${sDragCompleteEvent},sDragOverEvent=${sDragOverEvent}): oTitle.dragOver → ${oTitle.dragOver}`, { "event.action": "enableDragAndDrop", "logrite.oContainer": oContainer, "logrite.oTitle": oTitle, "logrite.sDragCompleteEvent": sDragCompleteEvent, "logrite.sDragOverEvent": sDragOverEvent });
        return true;
    },

    placeOnTop: function(oCont, bShow, sSrc)
    {
		logger.error(`>>> Entering placeOnTop(oCont=${oCont},bShow=${bShow},sSrc=${sSrc})`, { "event.action": "placeOnTop", "logrite.oCont": oCont, "logrite.bShow": bShow, "logrite.sSrc": sSrc });
        if (dnn.dom.browser.isType(dnn.dom.browser.Opera, dnn.dom.browser.Mozilla, dnn.dom.browser.Netscape, dnn.dom.browser.Safari) ||
	        (dnn.dom.browser.isType(dnn.dom.browser.InternetExplorer) && dnn.dom.browser.version >= 7))
            return; //not needed

        var oIFR = dnn.dom.getById('ifr' + oCont.id);

        if (oIFR == null)
        {
            var oIFR = document.createElement('iframe');
			logger.debug(`placeOnTop(oCont=${oCont},bShow=${bShow},sSrc=${sSrc}): oIFR.src → ${oIFR.src}`, { "event.action": "placeOnTop", "logrite.oCont": oCont, "logrite.bShow": bShow, "logrite.sSrc": sSrc });
			logger.debug(`placeOnTop(oCont=${oCont},bShow=${bShow},sSrc=${sSrc}): oIFR.scrolling → ${oIFR.scrolling}`, { "event.action": "placeOnTop", "logrite.oCont": oCont, "logrite.bShow": bShow, "logrite.sSrc": sSrc });
            oIFR.id = 'ifr' + oCont.id;
			logger.debug(`placeOnTop(oCont=${oCont},bShow=${bShow},sSrc=${sSrc}): oIFR.id → ${oIFR.id}`, { "event.action": "placeOnTop", "logrite.oCont": oCont, "logrite.bShow": bShow, "logrite.sSrc": sSrc });
            if (sSrc != null)
                oIFR.src = sSrc;
            oIFR.style.top = '0px';
			logger.debug(`placeOnTop(oCont=${oCont},bShow=${bShow},sSrc=${sSrc}): oIFR.frameBorder → ${oIFR.frameBorder}`, { "event.action": "placeOnTop", "logrite.oCont": oCont, "logrite.bShow": bShow, "logrite.sSrc": sSrc });
            oIFR.style.left = '0px';
            oIFR.style.filter = "progid:DXImageTransform.Microsoft.Alpha(opacity=0)";
            oIFR.scrolling = 'no';
            oIFR.frameBorder = 'no';
            oIFR.style.display = 'none';
            oIFR.style.position = 'absolute';
            oCont.parentNode.appendChild(oIFR);
        }
        var oDims = new dnn.dom.positioning.dims(oCont);

        oIFR.style.width = oDims.w;
        oIFR.style.height = oDims.h;
        oIFR.style.top = oDims.t + 'px';
        oIFR.style.left = oDims.l + 'px';

        var iIndex = dnn.dom.getCurrentStyle(oCont, 'zIndex');
        if (iIndex == null || iIndex == 0 || isNaN(iIndex))
            iIndex = 1;

        oCont.style.zIndex = iIndex;
        oIFR.style.zIndex = iIndex - 1;

        if (bShow)
            oIFR.style.display = "block";
        else if (oIFR != null)
            oIFR.style.display = 'none';
    },

    __dnn_containerMouseDown: function(oCtl)
    {
        //oCtl = dnn.dom.event.srcElement;
        while (oCtl.contID == null)
        {
            oCtl = oCtl.parentNode;
            if (oCtl.tagName.toUpperCase() == 'BODY')
                return;
        }
        dnn.dom.positioning.dragCtr = oCtl; //assumption is we can only drag one thing at a time
        oCtl.startTop = null;
		logger.debug(`__dnn_containerMouseDown(oCtl=${oCtl}): oCtl.startTop → ${oCtl.startTop}`, { "event.action": "__dnn_containerMouseDown", "logrite.oCtl": oCtl });
        oCtl.startLeft = null;
		logger.debug(`__dnn_containerMouseDown(oCtl=${oCtl}): oCtl.startLeft → ${oCtl.startLeft}`, { "event.action": "__dnn_containerMouseDown", "logrite.oCtl": oCtl });

        var oCont = dnn.dom.getById(oCtl.contID);
		logger.error(`__dnn_containerMouseDown(oCtl=${oCtl}): oCont → ${oCont}`, { "event.action": "__dnn_containerMouseDown", "logrite.oCtl": oCtl });
        if (oCont.style.position == null || oCont.style.position.length == 0)
            oCont.style.position = 'relative';

        dnn.dom.positioning.dragCtrDims = new dnn.dom.positioning.dims(oCont); //store now so we aren't continually calculating

        if (oCont.getAttribute('_b') == null)
        {
            oCont.setAttribute('_b', oCont.style.backgroundColor);
            oCont.setAttribute('_z', oCont.style.zIndex);
            oCont.setAttribute('_w', oCont.style.width);
            oCont.setAttribute('_d', oCont.style.border);
            oCont.style.zIndex = 9999;
            oCont.style.backgroundColor = DNN_HIGHLIGHT_COLOR;
            oCont.style.border = '4px outset ' + DNN_HIGHLIGHT_COLOR;
            oCont.style.width = dnn.dom.positioning.elementWidth(oCont);
            if (dnn.dom.browser.type == dnn.dom.browser.InternetExplorer)
                oCont.style.filter = 'progid:DXImageTransform.Microsoft.Alpha(opacity=80)';
        }
    },

    __dnn_containerMouseDownDelay: function(e)
    {
		logger.error(`>>> Entering __dnn_containerMouseDownDelay(e=${e})`, { "event.action": "__dnn_containerMouseDownDelay", "logrite.e": e });
        var oTitle = e.srcElement;
        if (oTitle == null)
            oTitle = e.target;
        dnn.doDelay('__dnn_dragdrop', 500, this.__dnn_containerMouseDown, oTitle);
		logger.warn(`<<< Exiting __dnn_containerMouseDownDelay(e=${e})`, { "event.action": "__dnn_containerMouseDownDelay", "logrite.e": e });
    },

    __dnn_bodyMouseUp: function()
    {
		logger.error('>>> Entering __dnn_bodyMouseUp()', { "event.action": "__dnn_bodyMouseUp" });
        dnn.cancelDelay('__dnn_dragdrop');
        var oCtl = dnn.dom.positioning.dragCtr;
        if (oCtl != null && oCtl.dragComplete != null)
        {
            eval(oCtl.dragComplete);

            var oCont = dnn.dom.getById(oCtl.contID);
			logger.error(`__dnn_bodyMouseUp(): oCont → ${oCont}`, { "event.action": "__dnn_bodyMouseUp" });

            oCont.style.backgroundColor = oCont.getAttribute('_b');
            oCont.style.zIndex = oCont.getAttribute('_z');
            oCont.style.width = oCont.getAttribute('_w');
            oCont.style.border = oCont.getAttribute('_d');
            oCont.setAttribute('_b', null);
            oCont.setAttribute('_z', null);
            if (dnn.dom.browser.type == dnn.dom.browser.InternetExplorer)
                oCont.style.filter = null;

        }

        dnn.dom.positioning.dragCtr = null;
		logger.warn('<<< Exiting __dnn_bodyMouseUp()', { "event.action": "__dnn_bodyMouseUp" });
    },

    __dnn_bodyMouseMove: function(e)
    {
		logger.error(`>>> Entering __dnn_bodyMouseMove(e=${e})`, { "event.action": "__dnn_bodyMouseMove", "logrite.e": e });
        if (this.dragCtr != null)
            this.dragContainer(this.dragCtr, e);
    }

});    

//dims object
dnn.dom.positioning.dims = function(eSrc)
{
    var bHidden = (eSrc.style.display == 'none');
	
    if (bHidden)
	    eSrc.style.display = "";
	
    this.w = dnn.dom.positioning.elementWidth(eSrc);
    this.h = dnn.dom.positioning.elementHeight(eSrc);
    var oPos = dnn.dom.positioning.elementPos(eSrc);
    this.t = oPos.t;
    this.l = oPos.l;
    this.at = oPos.at;	//actual top
    this.al = oPos.al;	//actual left
    this.rot = this.at - this.t; //relative offset top
    this.rol = this.al - this.l; //relative offset left
	
    this.r = this.l + this.w;
    this.b = this.t + this.h;
	
    if (bHidden)
	    eSrc.style.display = "none";
	
}
dnn.dom.positioning.dims.registerClass('dnn.dom.positioning.dims');


