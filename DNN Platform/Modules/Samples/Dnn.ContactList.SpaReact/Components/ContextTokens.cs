// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Globalization;
using System.Web;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.Modules;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.SpaReact.Components
{
    public class ContextTokens : IPropertyAccess
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public CacheLevel Cacheability => CacheLevel.notCacheable;
        private readonly SecurityContext security;
        private readonly ModuleInstanceContext moduleContext;

        public ContextTokens(ModuleInstanceContext moduleContext, UserInfo user)
        {
			_logger.LogDebug(">>> Entering ContextTokens(moduleContext={ModuleContext},user={User})", moduleContext, user);
            this.moduleContext = moduleContext;
            this.security = new SecurityContext(moduleContext.Configuration, user);
			_logger.LogWarning("ContextTokens(moduleContext={ModuleContext},user={User}): security → {Security}", moduleContext, user, security);
			_logger.LogDebug("ContextTokens(moduleContext={ModuleContext},user={User}): this.moduleContext → {Arg2}", moduleContext, user, this.moduleContext);
			_logger.LogDebug("<<< Exiting ContextTokens(moduleContext={ModuleContext},user={User})", moduleContext, user);
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			_logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
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
					_logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
                    return string.Empty;
            }
        }
    }
}
