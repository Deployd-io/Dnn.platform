const logger = console;
(function () {
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

    var personaBarSettings = window.parent['personaBarSettings'];
    var debugMode = personaBarSettings['debugMode'] === true;
    var cdv = personaBarSettings['buildNumber'];
    var version = (cdv ? '?cdv=' + cdv : '') + (debugMode ? '&t=' + Math.random(): '');
    var styles = [];
    var cssVariables = personaBarSettings['cssVariablesPath'];
    var mainJs = 'scripts/main.js';
    var themeCss = 'css/theme.css';
    var mainCss = 'css/main.css';

    if (cssVariables) {
        styles.push(cssVariables);
    }

    var hasCustomPersonaBarTheme = personaBarSettings['personaBarTheme'];
    if (hasCustomPersonaBarTheme){
        styles.push('../../../../Portals/_default/PersonaBarTheme.css');
    }
    else{
        styles.push(themeCss);
    }

    styles.push(mainCss);
    styles.push('css/graph.css');

    addCssToHead(styles, version);
    addJsToBody(mainJs, version);
})();