// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Globalization;
using System.Web;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Modules;
using Newtonsoft.Json;

namespace Dnn.ContactList.SpaReact.Components
{
    public class ContextTokens : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public CacheLevel Cacheability => CacheLevel.notCacheable;
        private readonly SecurityContext security;
        private readonly ModuleInstanceContext moduleContext;

        public ContextTokens(ModuleInstanceContext moduleContext, UserInfo user)
        {
			Logger.LogDebug(">>> Entering ContextTokens(moduleContext={ModuleContext},user={User})", moduleContext, user);
            this.moduleContext = moduleContext;
            this.security = new SecurityContext(moduleContext.Configuration, user);
			Logger.LogDebug("<<< Exiting ContextTokens(moduleContext={ModuleContext},user={User})", moduleContext, user);
			Logger.LogDebug("ContextTokens(moduleContext={ModuleContext},user={User}): this.moduleContext → {Arg2}", moduleContext, user, this.moduleContext);
			Logger.LogWarning("ContextTokens(moduleContext={ModuleContext},user={User}): security → {Security}", moduleContext, user, security);
			Logger.LogDebug("ContextTokens(moduleContext={ModuleContext},user={User}): moduleContext → {ModuleContext1}", moduleContext, user, moduleContext);
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            switch (propertyName.ToUpperInvariant())
            {
                case "SECURITY":
                    return HttpUtility.HtmlAttributeEncode(JsonConvert.SerializeObject(this.security));
                case "MODULE":
                    return HttpUtility.HtmlAttributeEncode(
                        JsonConvert.SerializeObject(
                            new
                            {
                                this.moduleContext.ModuleId,
                                this.moduleContext.TabId,
                                this.moduleContext.TabModuleId,
                                this.moduleContext.PortalId,
                            }));
                default:
                    propertyNotFound = true;
					Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
                    return string.Empty;
            }
        }
    }
}
