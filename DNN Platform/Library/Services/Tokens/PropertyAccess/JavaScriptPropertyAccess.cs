// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.Services.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>Property Access implementation for javascript registration.</summary>
    public class JavaScriptPropertyAccess : JsonPropertyAccess<JavaScriptDto>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly Page page;

        /// <summary>Initializes a new instance of the <see cref="JavaScriptPropertyAccess"/> class.</summary>
        /// <param name="page">The current page.</param>
        public JavaScriptPropertyAccess(Page page)
        {
			Logger.LogDebug(">>> Entering JavaScriptPropertyAccess(page={Page})", page);
            this.page = page;
			Logger.LogDebug("<<< Exiting JavaScriptPropertyAccess(page={Page})", page);
			Logger.LogDebug("JavaScriptPropertyAccess(page={Page}): page → {Page1}", page, page);
			Logger.LogDebug("JavaScriptPropertyAccess(page={Page}): this.page → {Arg1}", page, this.page);
        }

        /// <inheritdoc />
        protected override string ProcessToken(JavaScriptDto model, UserInfo accessingUser, Scope accessLevel)
        {
			Logger.LogDebug(">>> Entering ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel})", model, accessingUser, accessLevel);
            if (string.IsNullOrEmpty(model.JsName) && string.IsNullOrEmpty(model.Path))
            {
                throw new ArgumentException("If the jsname property is not specified then the JavaScript token must specify a path.");
            }

            if (model.Priority == 0)
            {
                model.Priority = (int)FileOrder.Js.DefaultPriority;
            }

            if (string.IsNullOrEmpty(model.Path))
            {
                RegisterInstalledLibrary(model);
            }
            else
            {
                ClientResourceManager.RegisterScript(
                    this.page,
                    model.Path,
                    model.Priority,
                    model.Provider ?? string.Empty,
                    model.JsName ?? string.Empty,
                    model.Version ?? string.Empty,
                    model.HtmlAttributes);
            }

			Logger.LogDebug("<<< Exiting ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel})", model, accessingUser, accessLevel);
            return string.Empty;
        }

        private static void RegisterInstalledLibrary(JavaScriptDto model)
        {
			Logger.LogDebug(">>> Entering RegisterInstalledLibrary(model={Model})", model);
            Version version = null;
            var specific = SpecificVersion.Latest;
			Logger.LogDebug("RegisterInstalledLibrary(model={Model}): version → {Version}", model, version);
			Logger.LogDebug("RegisterInstalledLibrary(model={Model}): specific → {Specific}", model, specific);
            if (!string.IsNullOrEmpty(model.Version))
            {
                version = new Version(model.Version);

                if (!string.IsNullOrEmpty(model.Specific))
                {
                    switch (model.Specific)
                    {
                        case "Exact":
                            specific = SpecificVersion.Exact;
                            break;
                        case "LatestMajor":
                            specific = SpecificVersion.LatestMajor;
                            break;
                        case "LatestMinor":
                            specific = SpecificVersion.LatestMinor;
                            break;
                        default:
                            specific = SpecificVersion.Latest;
                            break;
                    }
                }
            }

            JavaScript.RequestRegistration(model.JsName, version, specific);
			Logger.LogDebug("<<< Exiting RegisterInstalledLibrary(model={Model})", model);
        }
    }
}
