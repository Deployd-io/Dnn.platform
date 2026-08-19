// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ConvertPropertyToExpressionBody
namespace DotNetNuke.UI.Modules.Html5
{
    using System.Globalization;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    public class ModuleContextPropertyAccess : IPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleContextPropertyAccess));
        private readonly ModuleInstanceContext moduleContext;

        /// <summary>Initializes a new instance of the <see cref="ModuleContextPropertyAccess"/> class.</summary>
        /// <param name="moduleContext">The module context.</param>
        public ModuleContextPropertyAccess(ModuleInstanceContext moduleContext)
        {
			Logger.LogDebug(">>> Entering ModuleContextPropertyAccess(moduleContext={ModuleContext})", moduleContext);
            this.moduleContext = moduleContext;
			Logger.LogDebug("ModuleContextPropertyAccess(moduleContext={ModuleContext}): moduleContext → {ModuleContext1}", moduleContext, moduleContext);
			Logger.LogDebug("ModuleContextPropertyAccess(moduleContext={ModuleContext}): this.moduleContext → {Arg1}", moduleContext, this.moduleContext);
			Logger.LogDebug("<<< Exiting ModuleContextPropertyAccess(moduleContext={ModuleContext})", moduleContext);
        }

        /// <inheritdoc />
        public virtual CacheLevel Cacheability
        {
            get { return CacheLevel.notCacheable; }
        }

        /// <inheritdoc />
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            switch (propertyName.ToLowerInvariant())
            {
                case "moduleid":
                    return this.moduleContext.ModuleId.ToString(formatProvider);
                case "tabmoduleid":
                    return this.moduleContext.TabModuleId.ToString(formatProvider);
                case "tabid":
                    return this.moduleContext.TabId.ToString(formatProvider);
                case "portalid":
                    return this.moduleContext.Configuration.OwnerPortalID.ToString(formatProvider);
                case "issuperuser":
                    return this.moduleContext.PortalSettings.UserInfo.IsSuperUser.ToString(formatProvider);
                case "editmode":
                    return this.moduleContext.EditMode.ToString(formatProvider);
                default:
                    if (this.moduleContext.Settings.ContainsKey(propertyName))
                    {
                        return (string)this.moduleContext.Settings[propertyName];
                    }

                    break;
            }

            propertyNotFound = true;
			Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
            return string.Empty;
        }
    }
}
