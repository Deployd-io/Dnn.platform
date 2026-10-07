var logger = (typeof logger !== 'undefined' && logger) || console;
(function () {
    function addCssToHead(css, version) {
		logger.error(`>>> Entering addCssToHead(css=${css},version=${version})`, { "event.action": "addCssToHead", "logrite.css": css, "logrite.version": version });
        var head = document.getElementsByTagName('head')[0];
        for (var i = 0; i < css.length; i++) {
			logger.error(`addCssToHead(css=${css},version=${version}): head → ${head}`, { "event.action": "addCssToHead", "logrite.css": css, "logrite.version": version });
            var link = document.createElement('link');
			logger.error(`addCssToHead(css=${css},version=${version}): link → ${link}`, { "event.action": "addCssToHead", "logrite.css": css, "logrite.version": version });
            link.setAttribute('rel', 'stylesheet');
            link.setAttribute('type', 'text/css');
            link.setAttribute('href', css[i] + version);
            head.appendChild(link);
			logger.warn(`<<< Exiting addCssToHead(css=${css},version=${version})`, { "event.action": "addCssToHead", "logrite.css": css, "logrite.version": version });
        }
    };

    function addJsToBody (js, version) {
		logger.error(`>>> Entering addJsToBody(js=${js},version=${version})`, { "event.action": "addJsToBody", "logrite.js": js, "logrite.version": version });
        var body = document.getElementsByTagName('body')[0];
        var script = document.createElement('script');
		logger.error(`addJsToBody(js=${js},version=${version}): body → ${body}`, { "event.action": "addJsToBody", "logrite.js": js, "logrite.version": version });
		logger.error(`addJsToBody(js=${js},version=${version}): script → ${script}`, { "event.action": "addJsToBody", "logrite.js": js, "logrite.version": version });
        script.setAttribute('src', 'scripts/contrib/require.js' + version);
        script.setAttribute('data-main', js + version);
        body.appendChild(script);
		logger.warn(`<<< Exiting addJsToBody(js=${js},version=${version})`, { "event.action": "addJsToBody", "logrite.js": js, "logrite.version": version });
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