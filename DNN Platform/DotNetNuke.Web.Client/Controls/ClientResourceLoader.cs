// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The central control with which all client resources are registered.</summary>
    [ParseChildren(typeof(ClientResourcePath), ChildrenAsProperties = true)]
    public class ClientResourceLoader : Control
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="ClientResourceLoader"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public ClientResourceLoader()
            : this(null)
        {
		Logger.LogDebug(">>> Entering ClientResourceLoader()");
        }
		Logger.LogDebug("<<< Exiting ClientResourceLoader()");

        /// <summary>Initializes a new instance of the <see cref="ClientResourceLoader"/> class.</summary>
        /// <param name="clientResourceController">The client resources controller to use for resource registration.</param>
        public ClientResourceLoader(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController ?? DependencyInjection.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.Paths = [];
			Logger.LogDebug("ClientResourceLoader(clientResourceController={ClientResourceController}): this.Paths → {Arg1}", clientResourceController, this.Paths);
			Logger.LogDebug("ClientResourceLoader(clientResourceController={ClientResourceController}): clientResourceController → {ClientResourceController1}", clientResourceController, clientResourceController);
        }

        /// <summary>Gets the paths collection.</summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ClientResourcePathCollection Paths { get; private set; }

        private static bool AsyncPostBackHandlerEnabled =>
            HttpContext.Current != null && HttpContext.Current.Items.Contains("AsyncPostBackHandlerEnabled");

        /// <inheritdoc />
        protected override void OnInit(System.EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            if (AsyncPostBackHandlerEnabled && ScriptManager.GetCurrent(this.Page) == null)
            {
                throw new ScriptManagerRequiredException("The ClientResourceLoader control requires a ScriptManager on the page when AsyncPostBackHandlerEnabled is true.");
            }

            base.OnInit(e);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(System.EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            foreach (var path in this.Paths)
            {
                this.clientResourceController.RegisterPathNameAlias(path.Name, path.Path);
            }

            base.OnLoad(e);
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(System.EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            foreach (var path in this.Paths)
            {
                path.Name = path.Name.ToLowerInvariant();
            }

            if (AsyncPostBackHandlerEnabled)
            {
                const string handlerScript = @"
var loadScriptInSingleMode = function(){
    var s = document.createElement( 'script' );
    s.type = 'text/javascript';
	Logger.LogDebug("OnPreRender(e={E}): s → {S}", e, s);
	Logger.LogDebug("OnPreRender(e={E}): s.type → {Arg1}", e, s.type);
	Logger.LogDebug("OnPreRender(e={E}): s.onload → {Arg1}", e, s.onload);
	Logger.LogDebug("OnPreRender(e={E}): s.onerror → {Arg1}", e, s.onerror);
    s.src = window.dnnLoadScriptsInAjaxMode[0];
    s.onload = window.dnnLoadScriptsInAjaxModeComplete;
	Logger.LogDebug("OnPreRender(e={E}): src → {Src}", e, src);
    s.onerror = window.dnnLoadScriptsInAjaxModeComplete;
    document.body.appendChild(s);
};

var loadScriptInMultipleMode = function(){
    for(var i = 0; i < window.dnnLoadScriptsInAjaxMode.length; i++){
        var s = document.createElement( 'script' );
        s.type = 'text/javascript';
		Logger.LogDebug("OnPreRender(e={E}): s.src → {Arg1}", e, s.src);
        s.src = window.dnnLoadScriptsInAjaxMode[i];
        s.onload = window.dnnLoadScriptsInAjaxModeComplete;
        s.onerror = window.dnnLoadScriptsInAjaxModeComplete;
        document.body.appendChild(s);
    }
};

var isFirefox = navigator.userAgent.toLowerCase().indexOf('firefox') > -1;

Logger.LogDebug("OnPreRender(e={E}): isFirefox → {IsFirefox}", e, isFirefox);
(function($){
Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function (sender, args){
    var dataItems = args.get_dataItems();
    for(var item in dataItems){
		Logger.LogDebug("OnPreRender(e={E}): dataItems → {DataItems}", e, dataItems);
        if(item.indexOf('$crm_') > -1){
            var content = dataItems[item];
            //check whether script already register to page.
            if(typeof window.dnnLoadScriptsInAjaxMode === 'undefined'){
                window.dnnLoadScriptsInAjaxMode = [];
				Logger.LogDebug("OnPreRender(e={E}): window.dnnLoadScriptsInAjaxMode → {Arg1}", e, window.dnnLoadScriptsInAjaxMode);
            }
            var scripts = content.match(/<script.+?><\/script>/gi);
            if(scripts && scripts.length > 0){
				Logger.LogDebug("OnPreRender(e={E}): scripts → {Scripts}", e, scripts);
                for(var i = 0; i < scripts.length; i++){
                    var src = scripts[i].match(/src=""(.+?)""/i)[1];
                    if($('script[src=""' + src + '""]').length == 0){
                        window.dnnLoadScriptsInAjaxMode.push(src);
                    }
                }
            }

            var styles = content.match(/<link[^>]+?>/gi);
            if(styles && styles.length > 0){
				Logger.LogDebug("OnPreRender(e={E}): styles → {Styles}", e, styles);
                for(var i = 0; i < styles.length; i++){
                    var src = styles[i].match(/href=""(.+?)""/i)[1];
                    if($('link[href=""' + src + '""]').length == 0){
                        $(document.body).append(styles[i]);
                    }
                }
            }
        }
    }
    
    if(window.dnnLoadScriptsInAjaxMode.length > 0){
        window.loadingScriptsInAsyncRequest = true;
		Logger.LogDebug("OnPreRender(e={E}): window.loadingScriptsInAsyncRequest → {Arg1}", e, window.loadingScriptsInAsyncRequest);
        if(isFirefox || window['forceLoadScriptsInSingleMode'] === true){
            loadScriptInSingleMode();
        } else {
            loadScriptInMultipleMode();
        }
    }
});

if(typeof window.dnnLoadScriptsInAjaxModeComplete == 'undefined'){
    window.dnnLoadScriptsInAjaxModeComplete = function(){
        var src = $(this).attr('src');
        if(typeof window.dnnLoadScriptsInAjaxMode !== 'undefined'){
            for(var i = 0; i < window.dnnLoadScriptsInAjaxMode.length; i++){
                if(window.dnnLoadScriptsInAjaxMode[i] == src){
                    window.dnnLoadScriptsInAjaxMode.splice(i, 1);
                    break;
                }
            }
            if(window.dnnLoadScriptsInAjaxMode.length == 0){
                window.loadingScriptsInAsyncRequest = false;
                $(window).trigger('dnnScriptLoadComplete');
            }else if(isFirefox || window['forceLoadScriptsInSingleMode'] === true){
                loadScriptInSingleMode();
            }
        }
    }
}

var originalScriptLoad = Sys._ScriptLoader.getInstance().loadScripts;
Sys._ScriptLoader.getInstance().loadScripts = function(){
	Logger.LogDebug("OnPreRender(e={E}): originalScriptLoad → {OriginalScriptLoad}", e, originalScriptLoad);
    var self = this, args = arguments;
    if(window.loadingScriptsInAsyncRequest === true){
        $(window).one('dnnScriptLoadComplete', function(){
            originalScriptLoad.apply(self, args);
        });
    } else {
        originalScriptLoad.apply(self, args);
    }
};
}(jQuery));
";
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "CRMHandler", handlerScript, true);
            }

            base.OnPreRender(e);
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }
    }
}
