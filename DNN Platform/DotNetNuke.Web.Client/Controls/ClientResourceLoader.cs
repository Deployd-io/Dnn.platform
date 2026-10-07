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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClientResourceLoader));
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="ClientResourceLoader"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public ClientResourceLoader()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ClientResourceLoader"/> class.</summary>
        /// <param name="clientResourceController">The client resources controller to use for resource registration.</param>
        public ClientResourceLoader(IClientResourceController clientResourceController)
        {
			Logger.LogError(">>> Entering ClientResourceLoader(clientResourceController={ClientResourceController})", clientResourceController);
            this.clientResourceController = clientResourceController ?? DependencyInjection.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
			Logger.LogWarning("<<< Exiting ClientResourceLoader(clientResourceController={ClientResourceController})", clientResourceController);
            this.Paths = [];
        }

        /// <summary>Gets the paths collection.</summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ClientResourcePathCollection Paths { get; private set; }

        private static bool AsyncPostBackHandlerEnabled =>
            HttpContext.Current != null && HttpContext.Current.Items.Contains("AsyncPostBackHandlerEnabled");

        /// <inheritdoc />
        protected override void OnInit(System.EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            if (AsyncPostBackHandlerEnabled && ScriptManager.GetCurrent(this.Page) == null)
            {
				Logger.LogError("Throwing ScriptManagerRequiredException from OnInit(e={E})", e);
                throw new ScriptManagerRequiredException("The ClientResourceLoader control requires a ScriptManager on the page when AsyncPostBackHandlerEnabled is true.");
            }

            base.OnInit(e);
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(System.EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            foreach (var path in this.Paths)
            {
                this.clientResourceController.RegisterPathNameAlias(path.Name, path.Path);
            }

            base.OnLoad(e);
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(System.EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            foreach (var path in this.Paths)
            {
                path.Name = path.Name.ToLowerInvariant();
            }

            if (AsyncPostBackHandlerEnabled)
            {
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
                const string handlerScript = @"
var loadScriptInSingleMode = function(){
    var s = document.createElement( 'script' );
	Logger.LogError("OnPreRender(e={E}): s → {S}", e, s);
	Logger.LogDebug("OnPreRender(e={E}): s.src → {Arg1}", e, s.src);
	Logger.LogDebug("OnPreRender(e={E}): s.onload → {Arg1}", e, s.onload);
	Logger.LogDebug("OnPreRender(e={E}): s.onerror → {Arg1}", e, s.onerror);
    s.type = 'text/javascript';
	Logger.LogDebug("OnPreRender(e={E}): s.type → {Arg1}", e, s.type);
    s.src = window.dnnLoadScriptsInAjaxMode[0];
    s.onload = window.dnnLoadScriptsInAjaxModeComplete;
    s.onerror = window.dnnLoadScriptsInAjaxModeComplete;
    document.body.appendChild(s);
};

var loadScriptInMultipleMode = function(){
    for(var i = 0; i < window.dnnLoadScriptsInAjaxMode.length; i++){
        var s = document.createElement( 'script' );
        s.type = 'text/javascript';
        s.src = window.dnnLoadScriptsInAjaxMode[i];
        s.onload = window.dnnLoadScriptsInAjaxModeComplete;
        s.onerror = window.dnnLoadScriptsInAjaxModeComplete;
        document.body.appendChild(s);
    }
};

var isFirefox = navigator.userAgent.toLowerCase().indexOf('firefox') > -1;
Logger.LogError("OnPreRender(e={E}): isFirefox → {IsFirefox}", e, isFirefox);

(function($){
Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function (sender, args){
    var dataItems = args.get_dataItems();
	Logger.LogError("OnPreRender(e={E}): dataItems → {DataItems}", e, dataItems);
    for(var item in dataItems){
        if(item.indexOf('$crm_') > -1){
            var content = dataItems[item];
            //check whether script already register to page.
            if(typeof window.dnnLoadScriptsInAjaxMode === 'undefined'){
                window.dnnLoadScriptsInAjaxMode = [];
				Logger.LogDebug("OnPreRender(e={E}): window.dnnLoadScriptsInAjaxMode → {Arg1}", e, window.dnnLoadScriptsInAjaxMode);
            }
            var scripts = content.match(/<script.+?><\/script>/gi);
			Logger.LogError("OnPreRender(e={E}): scripts → {Scripts}", e, scripts);
            if(scripts && scripts.length > 0){
                for(var i = 0; i < scripts.length; i++){
                    var src = scripts[i].match(/src=""(.+?)""/i)[1];
					Logger.LogError("OnPreRender(e={E}): src → {Src}", e, src);
                    if($('script[src=""' + src + '""]').length == 0){
                        window.dnnLoadScriptsInAjaxMode.push(src);
                    }
                }
            }

            var styles = content.match(/<link[^>]+?>/gi);
			Logger.LogError("OnPreRender(e={E}): styles → {Styles}", e, styles);
            if(styles && styles.length > 0){
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
Logger.LogError("OnPreRender(e={E}): originalScriptLoad → {OriginalScriptLoad}", e, originalScriptLoad);
Sys._ScriptLoader.getInstance().loadScripts = function(){
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
        }
    }
}
