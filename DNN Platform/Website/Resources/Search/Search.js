const logger = console;
﻿(function ($, dnn) {
	logger.debug('>>> Entering function()');
    var isDropDownVisible = false;
    var searchSkinObject = null;
    var toggleDropDown = function(eventElement) {
		logger.debug(`>>> Entering toggleDropDown(eventElement=${eventElement})`);
        var choices = $get('SearchChoices');
        if (isDropDownVisible) {
			logger.debug(`toggleDropDown(eventElement=${eventElement}): choices → ${choices}`);
            choices.style.display = 'none';
            isDropDownVisible = false;
        } else {
            choices.style.display = 'block';
            isDropDownVisible = true;

            var clickEvent = function(e) {
				logger.debug(`>>> Entering clickEvent(e=${e})`);
                if (isDropDownVisible && e.target.id != "SearchIcon" && e.target.id.indexOf("downArrow") == -1) {
                    toggleDropDown();
                    $(document).off("click", clickEvent);
					logger.debug(`<<< Exiting clickEvent(e=${e})`);
                }
            };

            $(document).on("click", clickEvent);
			logger.debug(`<<< Exiting toggleDropDown(eventElement=${eventElement})`);
        }
    };

    var selectSearch = function(eventElement) {
		logger.debug(`>>> Entering selectSearch(eventElement=${eventElement})`);
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
				logger.debug(`<<< Exiting selectSearch(eventElement=${eventElement})`);
                searchSkinObject.settings.searchType = 'S';
        }
    };

    var searchHilite = function(eventElement) {
		logger.debug(`>>> Entering searchHilite(eventElement=${eventElement})`);
        eventElement.target.className = 'searchHilite';
		logger.debug(`<<< Exiting searchHilite(eventElement=${eventElement})`);
    };

    var searchDefault = function(eventElement) {
		logger.debug(`>>> Entering searchDefault(eventElement=${eventElement})`);
        eventElement.target.className = 'searchDefault';
		logger.debug(`<<< Exiting searchDefault(eventElement=${eventElement})`);
    };

    var initSearch = function() {
		logger.debug('>>> Entering initSearch()');
        var searchIcon = $get('SearchIcon');
        if (dnn.getVar('SearchIconSelected') == 'S') {
			logger.debug(`initSearch(): searchIcon → ${searchIcon}`);
            searchIcon.style.backgroundImage = dnn.getVar('SearchIconSiteUrl');
        } else {
            searchIcon.style.backgroundImage = dnn.getVar('SearchIconWebUrl');
        }
        $addHandler(searchIcon, 'click', toggleDropDown);

        var siteIcon = $get('SearchIconSite');
		logger.debug(`initSearch(): siteIcon → ${siteIcon}`);
        siteIcon.style.backgroundImage = dnn.getVar('SearchIconSiteUrl');
        $addHandler(siteIcon, 'click', selectSearch);
        $addHandler(siteIcon, 'mouseover', searchHilite);
        $addHandler(siteIcon, 'mouseout', searchDefault);

        var webIcon = $get('SearchIconWeb');
		logger.debug(`initSearch(): webIcon → ${webIcon}`);
        webIcon.style.backgroundImage = dnn.getVar('SearchIconWebUrl');
        $addHandler(webIcon, 'click', selectSearch);
        $addHandler(webIcon, 'mouseover', searchHilite);
        $addHandler(webIcon, 'mouseout', searchDefault);

        /* Set the default display style to resolve DOM bug */
        $get('SearchChoices').style.display = 'none';
		logger.debug('<<< Exiting initSearch()');
    };

    dnn.initDropdownSearch = function (skinObject) {
        searchSkinObject = skinObject;
        initSearch();
	logger.debug(`<<< Exiting function(skinObject=${skinObject})`);
    };

})(jQuery, window.dnn);
