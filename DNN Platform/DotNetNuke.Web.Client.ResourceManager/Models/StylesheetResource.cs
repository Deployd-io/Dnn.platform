// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models;

using System.Net;
using System.Text;

using DotNetNuke.Abstractions.ClientResources;

/// <summary>Represents a stylesheet resource that can be registered and rendered in the client resources controller.</summary>
public class StylesheetResource : LinkResource, IStylesheetResource
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StylesheetResource));
    private readonly IClientResourceController clientResourceController;

    /// <summary>Initializes a new instance of the <see cref="StylesheetResource"/> class.</summary>
    /// <param name="clientResourceController">The client resources controller used to register the stylesheet.</param>
    public StylesheetResource(IClientResourceController clientResourceController)
    {
		Logger.LogDebug(">>> Entering StylesheetResource(clientResourceController={ClientResourceController})", clientResourceController);
        this.clientResourceController = clientResourceController;
        this.Provider = ClientResourceProviders.DefaultCssProvider;
		Logger.LogDebug("StylesheetResource(clientResourceController={ClientResourceController}): this.clientResourceController → {Arg1}", clientResourceController, this.clientResourceController);
		Logger.LogDebug("StylesheetResource(clientResourceController={ClientResourceController}): this.Provider → {Arg1}", clientResourceController, this.Provider);
        this.Priority = (int)FileOrder.Css.DefaultPriority;
		Logger.LogDebug("<<< Exiting StylesheetResource(clientResourceController={ClientResourceController})", clientResourceController);
    }

    /// <inheritdoc />
    public bool Disabled { get; set; }

    /// <inheritdoc />
    public override void Register()
    {
		Logger.LogDebug(">>> Entering Register()");
        this.clientResourceController.AddStylesheet(this);
		Logger.LogDebug("<<< Exiting Register()");
    }

    /// <inheritdoc />
    public override string Render(int crmVersion, bool useCdn, string applicationPath)
    {
		Logger.LogDebug(">>> Entering Render(crmVersion={CrmVersion},useCdn={UseCdn},applicationPath={ApplicationPath})", crmVersion, useCdn, applicationPath);
        var htmlString = new StringBuilder("<link");
        htmlString.Append($" href=\"{this.GetVersionedPath(crmVersion, useCdn, applicationPath)}\"");
		Logger.LogDebug("Render(crmVersion={CrmVersion},useCdn={UseCdn},applicationPath={ApplicationPath}): htmlString → {HtmlString}", crmVersion, useCdn, applicationPath, htmlString);
        if (this.Preload && !this.Attributes.ContainsKey("rel") && !this.Attributes.ContainsKey("as"))
        {
            htmlString.Append($" rel=\"preload\" as=\"style\"");
        }
        else if (!this.Preload && !this.Attributes.ContainsKey("rel"))
        {
            htmlString.Append($" rel=\"stylesheet\"");
        }

        if (this.Disabled && !this.Attributes.ContainsKey("disabled"))
        {
            htmlString.Append(" disabled");
        }

        this.RenderMedia(htmlString);
        this.RenderType(htmlString);
        this.RenderBlocking(htmlString);
        this.RenderCrossOriginAttribute(htmlString);
        this.RenderFetchPriority(htmlString);
        this.RenderIntegrity(htmlString);
        this.RenderReferrerPolicy(htmlString);
        this.RenderAttributes(htmlString);
        htmlString.Append(" />");
		Logger.LogDebug("<<< Exiting Render(crmVersion={CrmVersion},useCdn={UseCdn},applicationPath={ApplicationPath})", crmVersion, useCdn, applicationPath);
        return htmlString.ToString();
    }
}
