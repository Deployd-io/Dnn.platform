// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public class CssPropertyAccess : JsonPropertyAccess<StylesheetDto>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly Page page;

        /// <summary>Initializes a new instance of the <see cref="CssPropertyAccess"/> class.</summary>
        /// <param name="page">The page to which the CSS should be registered.</param>
        public CssPropertyAccess(Page page)
        {
			Logger.LogDebug(">>> Entering CssPropertyAccess(page={Page})", page);
            this.page = page;
			Logger.LogDebug("<<< Exiting CssPropertyAccess(page={Page})", page);
			Logger.LogDebug("CssPropertyAccess(page={Page}): page → {Page1}", page, page);
			Logger.LogDebug("CssPropertyAccess(page={Page}): this.page → {Arg1}", page, this.page);
        }

        /// <inheritdoc />
        protected override string ProcessToken(StylesheetDto model, UserInfo accessingUser, Scope accessLevel)
        {
			Logger.LogDebug(">>> Entering ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel})", model, accessingUser, accessLevel);
            if (string.IsNullOrEmpty(model.Path))
            {
                throw new ArgumentException("The Css token must specify a path or property.");
            }

            if (model.Priority == 0)
            {
                model.Priority = (int)FileOrder.Css.DefaultPriority;
            }

            if (string.IsNullOrEmpty(model.Provider))
            {
                ClientResourceManager.RegisterStyleSheet(this.page, model.Path, model.Priority);
            }
            else
            {
                ClientResourceManager.RegisterStyleSheet(this.page, model.Path, model.Priority, model.Provider);
            }

			Logger.LogDebug("<<< Exiting ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel})", model, accessingUser, accessLevel);
            return string.Empty;
        }
    }
}
