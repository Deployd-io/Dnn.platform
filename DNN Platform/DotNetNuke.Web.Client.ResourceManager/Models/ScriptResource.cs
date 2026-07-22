// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models;

using System.Net;
using System.Text;

using DotNetNuke.Abstractions.ClientResources;

/// <summary>Represents a script resource that can be registered and rendered in the client resources system.</summary>
public class ScriptResource : ResourceBase, IScriptResource
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    private readonly IClientResourceController clientResourceController;

    /// <summary>Initializes a new instance of the <see cref="ScriptResource"/> class.</summary>
    /// <param name="clientResourceController">The client resources controller.</param>
    public ScriptResource(IClientResourceController clientResourceController)
    {
		Logger.LogDebug(">>> Entering ScriptResource(clientResourceController={ClientResourceController})", clientResourceController);
        this.clientResourceController = clientResourceController;
        this.Provider = ClientResourceProviders.DefaultJsProvider;
		Logger.LogDebug("ScriptResource(clientResourceController={ClientResourceController}): this.clientResourceController → {Arg1}", clientResourceController, this.clientResourceController);
		Logger.LogDebug("ScriptResource(clientResourceController={ClientResourceController}): this.Provider → {Arg1}", clientResourceController, this.Provider);
		Logger.LogDebug("ScriptResource(clientResourceController={ClientResourceController}): clientResourceController → {ClientResourceController1}", clientResourceController, clientResourceController);
        this.Priority = (int)FileOrder.Js.DefaultPriority;
        this.Type = "text/javascript";
		Logger.LogDebug("<<< Exiting ScriptResource(clientResourceController={ClientResourceController})", clientResourceController);
		Logger.LogDebug("ScriptResource(clientResourceController={ClientResourceController}): this.Type → {Arg1}", clientResourceController, this.Type);
    }

    /// <inheritdoc />
    public bool Async { get; set; }

    /// <inheritdoc />
    public bool Defer { get; set; }

    /// <inheritdoc />
    public bool NoModule { get; set; }

    /// <inheritdoc />
    public override void Register()
    {
		Logger.LogDebug(">>> Entering Register()");
        this.clientResourceController.AddScript(this);
		Logger.LogDebug("<<< Exiting Register()");
    }

    /// <inheritdoc />
    public override string Render(int crmVersion, bool useCdn, string applicationPath)
    {
		Logger.LogDebug(">>> Entering Render(crmVersion={CrmVersion},useCdn={UseCdn},applicationPath={ApplicationPath})", crmVersion, useCdn, applicationPath);
        var htmlString = new StringBuilder("<script");
        htmlString.Append($" src=\"{this.GetVersionedPath(crmVersion, useCdn, applicationPath)}\"");
		Logger.LogDebug("Render(crmVersion={CrmVersion},useCdn={UseCdn},applicationPath={ApplicationPath}): htmlString → {HtmlString}", crmVersion, useCdn, applicationPath, htmlString);
        if (this.Async && !this.Attributes.ContainsKey("async"))
        {
            htmlString.Append(" async");
        }

        if (this.Defer && !this.Attributes.ContainsKey("defer"))
        {
            htmlString.Append(" defer");
        }

        if (this.NoModule && !this.Attributes.ContainsKey("nomodule"))
        {
            htmlString.Append(" nomodule");
        }

        this.RenderType(htmlString);
        this.RenderBlocking(htmlString);
        this.RenderCrossOriginAttribute(htmlString);
        this.RenderFetchPriority(htmlString);
        this.RenderIntegrity(htmlString);
        this.RenderReferrerPolicy(htmlString);
        this.RenderAttributes(htmlString);
        htmlString.Append("></script>");
		Logger.LogDebug("<<< Exiting Render(crmVersion={CrmVersion},useCdn={UseCdn},applicationPath={ApplicationPath})", crmVersion, useCdn, applicationPath);
        return htmlString.ToString();
    }
}
