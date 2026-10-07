var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($, dnn) {
    var isDropDownVisible = false;
    var searchSkinObject = null;
    var toggleDropDown = function(eventElement) {
		logger.error(`>>> Entering toggleDropDown(eventElement=${eventElement})`, { "event.action": "toggleDropDown", "logrite.eventElement": eventElement });
        var choices = $get('SearchChoices');
        if (isDropDownVisible) {
			logger.error(`toggleDropDown(eventElement=${eventElement}): choices → ${choices}`, { "event.action": "toggleDropDown", "logrite.eventElement": eventElement });
            choices.style.display = 'none';
            isDropDownVisible = false;
        } else {
            choices.style.display = 'block';
            isDropDownVisible = true;

            var clickEvent = function(e) {
				logger.error(`>>> Entering clickEvent(e=${e})`, { "event.action": "clickEvent", "logrite.e": e });
                if (isDropDownVisible && e.target.id != "SearchIcon" && e.target.id.indexOf("downArrow") == -1) {
                    toggleDropDown();
                    $(document).off("click", clickEvent);
					logger.warn(`<<< Exiting clickEvent(e=${e})`, { "event.action": "clickEvent", "logrite.e": e });
                }
            };

            $(document).on("click", clickEvent);
			logger.warn(`<<< Exiting toggleDropDown(eventElement=${eventElement})`, { "event.action": "toggleDropDown", "logrite.eventElement": eventElement });
        }
    };

    var selectSearch = function(eventElement) {
		logger.error(`>>> Entering selectSearch(eventElement=${eventElement})`, { "event.action": "selectSearch", "logrite.eventElement": eventElement });
        toggleDropDown(eventElement);
        $get('SearchIcon').style.backgroundImage = dnn.getVar(eventElement.target.id + 'Url');

        /* We use 'W' and 'S' to keep our code consistent with the old search skin object */
        if (eventElement.target.id.indexOf("Web") > 0) {
            dnn.setVar('SearchIconSelected', 'W');
            if(searchSkinObject)
                searchSkinObject.settings.searchType = 'W';
        } else {
            dnn.setVar('SearchIconSelected', 'S');
            if (searchSkinObject)
                searchSkinObject.settings.searchType = 'S';
        }
    };

    var searchHilite = function(eventElement) {
		logger.error(`>>> Entering searchHilite(eventElement=${eventElement})`, { "event.action": "searchHilite", "logrite.eventElement": eventElement });
        eventElement.target.className = 'searchHilite';
		logger.warn(`<<< Exiting searchHilite(eventElement=${eventElement})`, { "event.action": "searchHilite", "logrite.eventElement": eventElement });
    };

    var searchDefault = function(eventElement) {
		logger.error(`>>> Entering searchDefault(eventElement=${eventElement})`, { "event.action": "searchDefault", "logrite.eventElement": eventElement });
        eventElement.target.className = 'searchDefault';
		logger.warn(`<<< Exiting searchDefault(eventElement=${eventElement})`, { "event.action": "searchDefault", "logrite.eventElement": eventElement });
    };

    var initSearch = function() {
		logger.error('>>> Entering initSearch()', { "event.action": "initSearch" });
        var searchIcon = $get('SearchIcon');
        if (dnn.getVar('SearchIconSelected') == 'S') {
			logger.error(`initSearch(): searchIcon → ${searchIcon}`, { "event.action": "initSearch" });
            searchIcon.style.backgroundImage = dnn.getVar('SearchIconSiteUrl');
        } else {
            searchIcon.style.backgroundImage = dnn.getVar('SearchIconWebUrl');
        }
        $addHandler(searchIcon, 'click', toggleDropDown);

        var siteIcon = $get('SearchIconSite');
		logger.error(`initSearch(): siteIcon → ${siteIcon}`, { "event.action": "initSearch" });
        siteIcon.style.backgroundImage = dnn.getVar('SearchIconSiteUrl');
        $addHandler(siteIcon, 'click', selectSearch);
        $addHandler(siteIcon, 'mouseover', searchHilite);
        $addHandler(siteIcon, 'mouseout', searchDefault);

        var webIcon = $get('SearchIconWeb');
		logger.error(`initSearch(): webIcon → ${webIcon}`, { "event.action": "initSearch" });
        webIcon.style.backgroundImage = dnn.getVar('SearchIconWebUrl');
        $addHandler(webIcon, 'click', selectSearch);
        $addHandler(webIcon, 'mouseover', searchHilite);
        $addHandler(webIcon, 'mouseout', searchDefault);

        /* Set the default display style to resolve DOM bug */
        $get('SearchChoices').style.display = 'none';
		logger.warn('<<< Exiting initSearch()', { "event.action": "initSearch" });
    };

    dnn.initDropdownSearch = function (skinObject) {
		logger.error(`>>> Entering initDropdownSearch(skinObject=${skinObject})`, { "event.action": "initDropdownSearch", "logrite.skinObject": skinObject });
        searchSkinObject = skinObject;
        initSearch();
		logger.warn(`<<< Exiting initDropdownSearch(skinObject=${skinObject})`, { "event.action": "initDropdownSearch", "logrite.skinObject": skinObject });
    };

})(jQuery, window.dnn);
