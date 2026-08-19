const logger = console;
(function () {
	logger.debug('>>> Entering function()');
	var regex = /\.mobi\.html/;
    var mobi = regex.test(location.href);
	logger.debug('<<< Exiting function()');
	logger.debug(`function(): mobi → ${mobi}`);
    
    function addCssToHead(css, version) {
		logger.debug(`>>> Entering addCssToHead(css=${css},version=${version})`);
        var head = document.getElementsByTagName('head')[0];
        for (var i = 0; i < css.length; i++) {
			logger.debug(`addCssToHead(css=${css},version=${version}): head → ${head}`);
            var link = document.createElement('link');
			logger.debug(`addCssToHead(css=${css},version=${version}): link → ${link}`);
            link.setAttribute('rel', 'stylesheet');
            link.setAttribute('type', 'text/css');
            link.setAttribute('href', css[i] + version);
            head.appendChild(link);
			logger.debug(`<<< Exiting addCssToHead(css=${css},version=${version})`);
        }
    };

    function addJsToBody (js, version) {
		logger.debug(`>>> Entering addJsToBody(js=${js},version=${version})`);
        var body = document.getElementsByTagName('body')[0];
        var script = document.createElement('script');
		logger.debug(`addJsToBody(js=${js},version=${version}): body → ${body}`);
		logger.debug(`addJsToBody(js=${js},version=${version}): script → ${script}`);
        script.setAttribute('src', 'scripts/contrib/require.js' + version);
        script.setAttribute('data-main', js + version);
        body.appendChild(script);
		logger.debug(`<<< Exiting addJsToBody(js=${js},version=${version})`);
    };

    var editBarSettings = window.parent['editBarSettings'];
    var debugMode = editBarSettings['debugMode'] === true;
    var cdv = editBarSettings['buildNumber'];
    var version = (cdv ? '?cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random(): '');
    var styles = [];
    var mainJs = mobi ? 'scripts/main.mobi.js' : 'scripts/main.js';
    var themeCss = 'css/theme.css';
    var mainCss = mobi ? 'css/main.mobi.css' : 'css/main.css';

    var hasCustomEditBarTheme = editBarSettings['editBarTheme'];
    if (hasCustomEditBarTheme){
        styles.push('../../../../Portals/_default/EditBarTheme.css');
    }
    else{
        styles.push(themeCss);
    }

    styles.push(mainCss);

    addCssToHead(styles, version);
    addJsToBody(mainJs, version);
})();