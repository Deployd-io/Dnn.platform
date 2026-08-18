// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models;

using System.Net;
using System.Text;

using DotNetNuke.Abstractions.ClientResources;

/// <summary>Represents a font resource that can be registered and rendered for client use.</summary>
public class FontResource : LinkResource, IFontResource
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FontResource));
    private readonly IClientResourceController clientResourceController;

    /// <summary>Initializes a new instance of the <see cref="FontResource"/> class.</summary>
    /// <param name="clientResourceController">The client resources controller used to manage font resources.</param>
    public FontResource(IClientResourceController clientResourceController)
    {
		Logger.LogDebug(">>> Entering FontResource(clientResourceController={ClientResourceController})", clientResourceController);
        this.clientResourceController = clientResourceController;
        this.Provider = ClientResourceProviders.DnnPageHeaderProvider;
		Logger.LogDebug("FontResource(clientResourceController={ClientResourceController}): this.clientResourceController → {Arg1}", clientResourceController, this.clientResourceController);
		Logger.LogDebug("<<< Exiting FontResource(clientResourceController={ClientResourceController})", clientResourceController);
    }

    /// <inheritdoc />
    public override void Register()
    {
		Logger.LogDebug(">>> Entering Register()");
        this.clientResourceController.AddFont(this);
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
            htmlString.Append($" rel=\"preload\" as=\"font\"");
        }
        else if (!this.Preload && !this.Attributes.ContainsKey("rel"))
        {
            htmlString.Append($" rel=\"font\"");
        }

        this.RenderType(htmlString);
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
