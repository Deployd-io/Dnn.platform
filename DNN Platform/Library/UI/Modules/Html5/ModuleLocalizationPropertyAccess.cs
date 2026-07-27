// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules.Html5
{
    using System.IO;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;

    public class ModuleLocalizationPropertyAccess : JsonPropertyAccess<ModuleLocalizationDto>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ModuleInstanceContext moduleContext;
        private readonly string html5File;

        /// <summary>Initializes a new instance of the <see cref="ModuleLocalizationPropertyAccess"/> class.</summary>
        /// <param name="moduleContext">The module context.</param>
        /// <param name="html5File">The HTML file.</param>
        public ModuleLocalizationPropertyAccess(ModuleInstanceContext moduleContext, string html5File)
        {
			Logger.LogDebug(">>> Entering ModuleLocalizationPropertyAccess(moduleContext={ModuleContext},html5File={Html5File})", moduleContext, html5File);
            this.html5File = html5File;
            this.moduleContext = moduleContext;
			Logger.LogDebug("<<< Exiting ModuleLocalizationPropertyAccess(moduleContext={ModuleContext},html5File={Html5File})", moduleContext, html5File);
			Logger.LogDebug("ModuleLocalizationPropertyAccess(moduleContext={ModuleContext},html5File={Html5File}): this.html5File → {Arg2}", moduleContext, html5File, this.html5File);
			Logger.LogDebug("ModuleLocalizationPropertyAccess(moduleContext={ModuleContext},html5File={Html5File}): moduleContext → {ModuleContext1}", moduleContext, html5File, moduleContext);
			Logger.LogDebug("ModuleLocalizationPropertyAccess(moduleContext={ModuleContext},html5File={Html5File}): this.moduleContext → {Arg2}", moduleContext, html5File, this.moduleContext);
        }

        /// <inheritdoc />
        protected override string ProcessToken(ModuleLocalizationDto model, UserInfo accessingUser, Scope accessLevel)
        {
			Logger.LogDebug(">>> Entering ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel})", model, accessingUser, accessLevel);
            string returnValue = string.Empty;

			Logger.LogDebug("ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel}): returnValue → {ReturnValue}", model, accessingUser, accessLevel, returnValue);
            string resourceFile = model.LocalResourceFile;
            if (string.IsNullOrEmpty(resourceFile))
			Logger.LogDebug("ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel}): resourceFile → {ResourceFile}", model, accessingUser, accessLevel, resourceFile);
            {
                var fileName = Path.GetFileName(this.html5File);
                var path = this.html5File.Replace(fileName, string.Empty);
				Logger.LogDebug("ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel}): fileName → {FileName}", model, accessingUser, accessLevel, fileName);
				Logger.LogDebug("ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel}): path → {Path}", model, accessingUser, accessLevel, path);
                resourceFile = Path.Combine(path, Localization.LocalResourceDirectory + "/", Path.ChangeExtension(fileName, "resx"));
            }

            if (!string.IsNullOrEmpty(model.Key))
            {
                returnValue = Localization.GetString(model.Key, resourceFile);
            }

			Logger.LogDebug("<<< Exiting ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel})", model, accessingUser, accessLevel);
            return returnValue;
        }
    }
}
