// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>Renders client resources.</summary>
    public class DnnResources : Literal
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnResources));
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnResources"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnResources(IClientResourceController clientResourceController)
        {
			Logger.LogDebug(">>> Entering DnnResources(clientResourceController={ClientResourceController})", clientResourceController);
            this.clientResourceController = clientResourceController;
			Logger.LogDebug("DnnResources(clientResourceController={ClientResourceController}): clientResourceController → {ClientResourceController1}", clientResourceController, clientResourceController);
			Logger.LogDebug("DnnResources(clientResourceController={ClientResourceController}): this.clientResourceController → {Arg1}", clientResourceController, this.clientResourceController);
			Logger.LogDebug("<<< Exiting DnnResources(clientResourceController={ClientResourceController})", clientResourceController);
        }

        /// <summary>Gets or sets the application path.</summary>
        public string ApplicationPath { get; set; }

        /// <summary>Gets or sets the provider.</summary>
        public string Provider { get; set; }

        private static bool AsyncPostBackHandlerEnabled =>
            HttpContext.Current != null && HttpContext.Current.Items.Contains("AsyncPostBackHandlerEnabled");

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer);
            base.Render(writer);
            writer.Write(this.clientResourceController.RenderDependencies(ResourceType.All, this.Provider, this.ApplicationPath));

            var scriptManager = ScriptManager.GetCurrent(this.Page);
			Logger.LogDebug("Render(writer={Writer}): scriptManager → {ScriptManager}", writer, scriptManager);
            if (this.Provider == ClientResourceProviders.DnnBodyProvider && scriptManager != null && scriptManager.IsInAsyncPostBack)
            {
                var holderControl = this.Page.FindControl("BodySCRIPTS");
				Logger.LogDebug("Render(writer={Writer}): holderControl → {HolderControl}", writer, holderControl);
				Logger.LogDebug("Render(writer={Writer}): holderControl.ID → {Arg1}", writer, holderControl.ID);
                holderControl.ID = "$crm_BodySCRIPTS";

                // Get scripts to be loaded during async postback
                var js = this.clientResourceController.RenderDependencies(ResourceType.Script, ClientResourceProviders.DnnBodyProvider, this.ApplicationPath);
				Logger.LogDebug("Render(writer={Writer}): js → {Js}", writer, js);
                var css = this.clientResourceController.RenderDependencies(ResourceType.Stylesheet, ClientResourceProviders.DefaultCssProvider, this.ApplicationPath);
				Logger.LogDebug("Render(writer={Writer}): css → {Css}", writer, css);
                scriptManager.RegisterDataItem(holderControl, $"{js}{css}");
            }
			Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer);
        }

        /// <inheritdoc />
        protected override void OnInit(System.EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            if (AsyncPostBackHandlerEnabled && ScriptManager.GetCurrent(this.Page) == null)
            {
				Logger.LogWarning("Throwing ScriptManagerRequiredException from OnInit(e={E})", e);
                throw new ScriptManagerRequiredException("The ClientResourceLoader control requires a ScriptManager on the page when AsyncPostBackHandlerEnabled is true.");
            }

            base.OnInit(e);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc/>
        protected override void OnPreRender(System.EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            if (AsyncPostBackHandlerEnabled)
            {
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
                const string handlerScript = @"
var loadScriptInSingleMode = function(){
    var s = document.createElement( 'script' );
	Logger.LogDebug("OnPreRender(e={E}): s → {S}", e, s);
	Logger.LogDebug("OnPreRender(e={E}): s.type → {Arg1}", e, s.type);
	Logger.LogDebug("OnPreRender(e={E}): s.onload → {Arg1}", e, s.onload);
	Logger.LogDebug("OnPreRender(e={E}): s.onerror → {Arg1}", e, s.onerror);
    s.type = 'text/javascript';
    s.src = window.dnnLoadScriptsInAjaxMode[0];
	Logger.LogDebug("OnPreRender(e={E}): src → {Src}", e, src);
	Logger.LogDebug("OnPreRender(e={E}): s.src → {Arg1}", e, s.src);
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
Logger.LogDebug("OnPreRender(e={E}): isFirefox → {IsFirefox}", e, isFirefox);

(function($){
Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function (sender, args){
    var dataItems = args.get_dataItems();
	Logger.LogDebug("OnPreRender(e={E}): dataItems → {DataItems}", e, dataItems);
    for(var item in dataItems){
        if(item.indexOf('$crm_') > -1){
            var content = dataItems[item];
            //check whether script already register to page.
            if(typeof window.dnnLoadScriptsInAjaxMode === 'undefined'){
                window.dnnLoadScriptsInAjaxMode = [];
				Logger.LogDebug("OnPreRender(e={E}): window.dnnLoadScriptsInAjaxMode → {Arg1}", e, window.dnnLoadScriptsInAjaxMode);
            }
            var scripts = content.match(/<script.+?><\/script>/gi);
			Logger.LogDebug("OnPreRender(e={E}): scripts → {Scripts}", e, scripts);
            if(scripts && scripts.length > 0){
                for(var i = 0; i < scripts.length; i++){
                    var src = scripts[i].match(/src=""(.+?)""/i)[1];
                    if($('script[src=""' + src + '""]').length == 0){
                        window.dnnLoadScriptsInAjaxMode.push(src);
                    }
                }
            }

            var styles = content.match(/<link[^>]+?>/gi);
			Logger.LogDebug("OnPreRender(e={E}): styles → {Styles}", e, styles);
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
Logger.LogDebug("OnPreRender(e={E}): originalScriptLoad → {OriginalScriptLoad}", e, originalScriptLoad);
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
