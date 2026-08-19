// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Instrumentation;

    /// <inheritdoc />
    public class ClientResourceController : IClientResourceController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ClientResourceController));
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IClientResourceSettings clientResourceSettings;
        private readonly Guid controllerId;
        private readonly int crmVersion = 1;
        private bool hasBegunRendering;

        /// <summary>Initializes a new instance of the <see cref="ClientResourceController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public ClientResourceController(IHostSettings hostSettings)
            : this(hostSettings, null, null)
        {
		Logger.LogDebug(">>> Entering ClientResourceController(hostSettings={HostSettings})", hostSettings);
        }
		Logger.LogDebug("<<< Exiting ClientResourceController(hostSettings={HostSettings})", hostSettings);

        /// <summary>Initializes a new instance of the <see cref="ClientResourceController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.3. Use overload with IClientResourceSettings. Scheduled removal in v12.0.0.")]
        public ClientResourceController(IHostSettings hostSettings, IApplicationStatusInfo appStatus)
            : this(hostSettings, appStatus, null)
        {
		Logger.LogDebug(">>> Entering ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus})", hostSettings, appStatus);
		Logger.LogDebug("<<< Exiting ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus})", hostSettings, appStatus);
		Logger.LogDebug("ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus},clientResourceSettings={ClientResourceSettings}): appStatus → {AppStatus1}", hostSettings, appStatus, clientResourceSettings, appStatus);
		Logger.LogDebug("ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus},clientResourceSettings={ClientResourceSettings}): clientResourceSettings → {ClientResourceSettings1}", hostSettings, appStatus, clientResourceSettings, clientResourceSettings);
		Logger.LogDebug("ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus},clientResourceSettings={ClientResourceSettings}): hostSettings → {HostSettings1}", hostSettings, appStatus, clientResourceSettings, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="ClientResourceController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="clientResourceSettings">The client resource settings.</param>
        public ClientResourceController(IHostSettings hostSettings, IApplicationStatusInfo appStatus, IClientResourceSettings clientResourceSettings)
        {
            this.hostSettings = hostSettings;
            this.appStatus = appStatus;
			Logger.LogDebug("ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus},clientResourceSettings={ClientResourceSettings}): this.hostSettings → {Arg3}", hostSettings, appStatus, clientResourceSettings, this.hostSettings);
			Logger.LogDebug("ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus},clientResourceSettings={ClientResourceSettings}): this.appStatus → {Arg3}", hostSettings, appStatus, clientResourceSettings, this.appStatus);
            this.clientResourceSettings = clientResourceSettings;
            this.RegisterPathNameAlias("SharedScripts", "~/Resources/Shared/Scripts/");
            this.controllerId = Guid.NewGuid();
			Logger.LogDebug("ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus},clientResourceSettings={ClientResourceSettings}): this.clientResourceSettings → {Arg3}", hostSettings, appStatus, clientResourceSettings, this.clientResourceSettings);
			Logger.LogDebug("ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus},clientResourceSettings={ClientResourceSettings}): controllerId → {ControllerId}", hostSettings, appStatus, clientResourceSettings, controllerId);
            if (clientResourceSettings != null)
            {
                this.crmVersion = clientResourceSettings.OverrideDefaultSettings ? clientResourceSettings.PortalCrmVersion : clientResourceSettings.HostCrmVersion;
				Logger.LogDebug("ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus},clientResourceSettings={ClientResourceSettings}): crmVersion → {CrmVersion}", hostSettings, appStatus, clientResourceSettings, crmVersion);
				Logger.LogDebug("ClientResourceController(hostSettings={HostSettings},appStatus={AppStatus},clientResourceSettings={ClientResourceSettings}): this.crmVersion → {Arg3}", hostSettings, appStatus, clientResourceSettings, this.crmVersion);
            }

            Logger.Debug($"ClientResourceController initialized with ID {this.controllerId}");
        }

        private List<IFontResource> Fonts { get; set; } = [];

        private List<IScriptResource> Scripts { get; set; } = [];

        private List<IStylesheetResource> Stylesheets { get; set; } = [];

        private Dictionary<string, string> PathNameAliases { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private HashSet<string> FontsToExclude { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private HashSet<string> ScriptsToExclude { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private HashSet<string> StylesheetsToExclude { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public void AddFont(IFontResource font)
        {
			Logger.LogDebug(">>> Entering AddFont(font={Font})", font);
            this.Fonts = this.AddResource(this.Fonts, font);
			Logger.LogDebug("<<< Exiting AddFont(font={Font})", font);
        }

        /// <inheritdoc />
        public void AddScript(IScriptResource script)
        {
			Logger.LogDebug(">>> Entering AddScript(script={Script})", script);
            this.Scripts = this.AddResource(this.Scripts, script);
			Logger.LogDebug("<<< Exiting AddScript(script={Script})", script);
        }

        /// <inheritdoc />
        public void AddStylesheet(IStylesheetResource stylesheet)
        {
			Logger.LogDebug(">>> Entering AddStylesheet(stylesheet={Stylesheet})", stylesheet);
            this.Stylesheets = this.AddResource(this.Stylesheets, stylesheet);
			Logger.LogDebug("<<< Exiting AddStylesheet(stylesheet={Stylesheet})", stylesheet);
        }

        /// <inheritdoc />
        public IFontResource CreateFont(string sourcePath)
        {
			Logger.LogDebug(">>> Entering CreateFont(sourcePath={SourcePath})", sourcePath);
            var font = new Models.FontResource(this);
            font.FilePath = sourcePath;
			Logger.LogDebug("CreateFont(sourcePath={SourcePath}): font.FilePath → {Arg1}", sourcePath, font.FilePath);
			Logger.LogDebug("CreateFont(sourcePath={SourcePath}): font → {Font}", sourcePath, font);

            switch (font.FilePath?.Substring(font.FilePath.LastIndexOf('.')).ToLowerInvariant())
            {
                case ".eot":
                    font.Type = "application/vnd.ms-fontobject";
					Logger.LogDebug("CreateFont(sourcePath={SourcePath}): font.Type → {Arg1}", sourcePath, font.Type);
                    break;
                case ".woff":
                    font.Type = "font/woff";
                    break;
                case ".woff2":
                    font.Type = "font/woff2";
                    break;
                case ".ttf":
                    font.Type = "font/ttf";
                    break;
                case ".svg":
                    font.Type = "image/svg+xml";
                    break;
                case ".otf":
                    font.Type = "font/otf";
                    break;
                default:
                    font.Type = "application/octet-stream";
                    break;
            }

			Logger.LogDebug("<<< Exiting CreateFont(sourcePath={SourcePath})", sourcePath);
			Logger.LogDebug("<<< Exiting CreateFont(sourcePath={SourcePath},pathNameAlias={PathNameAlias})", sourcePath, pathNameAlias);
			Logger.LogDebug("<<< Exiting CreateFont(sourcePath={SourcePath},pathNameAlias={PathNameAlias},mimeType={MimeType})", sourcePath, pathNameAlias, mimeType);
            return font;
        }

        /// <inheritdoc />
        public IFontResource CreateFont(string sourcePath, string pathNameAlias)
        {
            var font = this.CreateFont(sourcePath);
            font.PathNameAlias = pathNameAlias;
			Logger.LogDebug("CreateFont(sourcePath={SourcePath},pathNameAlias={PathNameAlias}): font → {Font}", sourcePath, pathNameAlias, font);
            return font;
        }

        /// <inheritdoc />
        public IFontResource CreateFont(string sourcePath, string pathNameAlias, string mimeType)
        {
            var font = this.CreateFont(sourcePath, pathNameAlias);
            font.Type = mimeType;
			Logger.LogDebug("CreateFont(sourcePath={SourcePath},pathNameAlias={PathNameAlias},mimeType={MimeType}): font → {Font}", sourcePath, pathNameAlias, mimeType, font);
            return font;
        }

        /// <inheritdoc />
        public IScriptResource CreateScript(string sourcePath)
        {
			Logger.LogDebug(">>> Entering CreateScript(sourcePath={SourcePath})", sourcePath);
            var script = new Models.ScriptResource(this);
            script.FilePath = sourcePath;
			Logger.LogDebug("CreateScript(sourcePath={SourcePath}): script.FilePath → {Arg1}", sourcePath, script.FilePath);
			Logger.LogDebug("CreateScript(sourcePath={SourcePath}): script → {Script}", sourcePath, script);
			Logger.LogDebug("<<< Exiting CreateScript(sourcePath={SourcePath})", sourcePath);
			Logger.LogDebug("<<< Exiting CreateScript(sourcePath={SourcePath},pathNameAlias={PathNameAlias})", sourcePath, pathNameAlias);
            return script;
        }

        /// <inheritdoc />
        public IScriptResource CreateScript(string sourcePath, string pathNameAlias)
        {
            var script = this.CreateScript(sourcePath);
            script.PathNameAlias = pathNameAlias;
			Logger.LogDebug("CreateScript(sourcePath={SourcePath},pathNameAlias={PathNameAlias}): script → {Script}", sourcePath, pathNameAlias, script);
            return script;
        }

        /// <inheritdoc />
        public IStylesheetResource CreateStylesheet(string sourcePath)
        {
			Logger.LogDebug(">>> Entering CreateStylesheet(sourcePath={SourcePath})", sourcePath);
            var stylesheet = new Models.StylesheetResource(this);
            stylesheet.FilePath = sourcePath;
			Logger.LogDebug("CreateStylesheet(sourcePath={SourcePath}): stylesheet.FilePath → {Arg1}", sourcePath, stylesheet.FilePath);
			Logger.LogDebug("CreateStylesheet(sourcePath={SourcePath}): stylesheet → {Stylesheet}", sourcePath, stylesheet);
			Logger.LogDebug("<<< Exiting CreateStylesheet(sourcePath={SourcePath})", sourcePath);
			Logger.LogDebug("<<< Exiting CreateStylesheet(sourcePath={SourcePath},pathNameAlias={PathNameAlias})", sourcePath, pathNameAlias);
            return stylesheet;
        }

        /// <inheritdoc />
        public IStylesheetResource CreateStylesheet(string sourcePath, string pathNameAlias)
        {
            var stylesheet = this.CreateStylesheet(sourcePath);
            stylesheet.PathNameAlias = pathNameAlias;
			Logger.LogDebug("CreateStylesheet(sourcePath={SourcePath},pathNameAlias={PathNameAlias}): stylesheet → {Stylesheet}", sourcePath, pathNameAlias, stylesheet);
            return stylesheet;
        }

        /// <inheritdoc />
        public void RegisterPathNameAlias(string pathNameAlias, string resolvedPath)
        {
			Logger.LogDebug(">>> Entering RegisterPathNameAlias(pathNameAlias={PathNameAlias},resolvedPath={ResolvedPath})", pathNameAlias, resolvedPath);
            this.PathNameAliases[pathNameAlias] = resolvedPath;
			Logger.LogDebug("<<< Exiting RegisterPathNameAlias(pathNameAlias={PathNameAlias},resolvedPath={ResolvedPath})", pathNameAlias, resolvedPath);
        }

        /// <inheritdoc />
        public void RemoveFontByName(string fontName)
        {
			Logger.LogDebug(">>> Entering RemoveFontByName(fontName={FontName})", fontName);
            this.FontsToExclude.Add(fontName);
			Logger.LogDebug("<<< Exiting RemoveFontByName(fontName={FontName})", fontName);
        }

        /// <inheritdoc />
        public void RemoveFontByPath(string fontPath, string pathNameAlias)
        {
			Logger.LogDebug(">>> Entering RemoveFontByPath(fontPath={FontPath},pathNameAlias={PathNameAlias})", fontPath, pathNameAlias);
            var fullPath = this.ResolvePath(fontPath, pathNameAlias);
            this.FontsToExclude.Add(fullPath);
			Logger.LogDebug("RemoveFontByPath(fontPath={FontPath},pathNameAlias={PathNameAlias}): fullPath → {FullPath}", fontPath, pathNameAlias, fullPath);
			Logger.LogDebug("<<< Exiting RemoveFontByPath(fontPath={FontPath},pathNameAlias={PathNameAlias})", fontPath, pathNameAlias);
        }

        /// <inheritdoc />
        public void RemoveScriptByName(string scriptName)
        {
			Logger.LogDebug(">>> Entering RemoveScriptByName(scriptName={ScriptName})", scriptName);
            this.ScriptsToExclude.Add(scriptName);
			Logger.LogDebug("<<< Exiting RemoveScriptByName(scriptName={ScriptName})", scriptName);
        }

        /// <inheritdoc />
        public void RemoveScriptByPath(string scriptPath, string pathNameAlias)
        {
			Logger.LogDebug(">>> Entering RemoveScriptByPath(scriptPath={ScriptPath},pathNameAlias={PathNameAlias})", scriptPath, pathNameAlias);
            var fullPath = this.ResolvePath(scriptPath, pathNameAlias);
            this.ScriptsToExclude.Add(fullPath);
			Logger.LogDebug("RemoveScriptByPath(scriptPath={ScriptPath},pathNameAlias={PathNameAlias}): fullPath → {FullPath}", scriptPath, pathNameAlias, fullPath);
			Logger.LogDebug("<<< Exiting RemoveScriptByPath(scriptPath={ScriptPath},pathNameAlias={PathNameAlias})", scriptPath, pathNameAlias);
        }

        /// <inheritdoc />
        public void RemoveStylesheetByName(string stylesheetName)
        {
			Logger.LogDebug(">>> Entering RemoveStylesheetByName(stylesheetName={StylesheetName})", stylesheetName);
            this.StylesheetsToExclude.Add(stylesheetName);
			Logger.LogDebug("<<< Exiting RemoveStylesheetByName(stylesheetName={StylesheetName})", stylesheetName);
        }

        /// <inheritdoc />
        public void RemoveStylesheetByPath(string stylesheetPath, string pathNameAlias)
        {
			Logger.LogDebug(">>> Entering RemoveStylesheetByPath(stylesheetPath={StylesheetPath},pathNameAlias={PathNameAlias})", stylesheetPath, pathNameAlias);
            var fullPath = this.ResolvePath(stylesheetPath, pathNameAlias);
            this.StylesheetsToExclude.Add(fullPath);
			Logger.LogDebug("RemoveStylesheetByPath(stylesheetPath={StylesheetPath},pathNameAlias={PathNameAlias}): fullPath → {FullPath}", stylesheetPath, pathNameAlias, fullPath);
			Logger.LogDebug("<<< Exiting RemoveStylesheetByPath(stylesheetPath={StylesheetPath},pathNameAlias={PathNameAlias})", stylesheetPath, pathNameAlias);
        }

        /// <inheritdoc />
        public string RenderDependencies(ResourceType resourceType, string provider, string applicationPath)
        {
			Logger.LogDebug(">>> Entering RenderDependencies(resourceType={ResourceType},provider={Provider},applicationPath={ApplicationPath})", resourceType, provider, applicationPath);
            this.hasBegunRendering = true;
            Logger.Debug($"Rendering dependencies for CRC id {this.controllerId} with ResourceType={resourceType}, Provider={provider}, ApplicationPath={applicationPath}. We have {this.Scripts.Count} scripts, {this.Stylesheets.Count} stylesheets and {this.Fonts.Count} fonts.");
			Logger.LogDebug("RenderDependencies(resourceType={ResourceType},provider={Provider},applicationPath={ApplicationPath}): this.hasBegunRendering → {Arg3}", resourceType, provider, applicationPath, this.hasBegunRendering);
            var sortedList = new List<IResource>();
            if (resourceType is ResourceType.Font or ResourceType.All)
            {
                sortedList.AddRange(
                    this.Fonts
                        .Where(s =>
                            (s.Provider == provider || (s.Provider == string.Empty && provider == ClientResourceProviders.DefaultCssProvider))
                            && !this.FontsToExclude.Contains(s.Name))
                        .OrderBy(l => l.Priority));
            }

            if (resourceType is ResourceType.Stylesheet or ResourceType.All)
            {
                sortedList.AddRange(
                    this.Stylesheets
                        .Where(s =>
                            (s.Provider == provider || (s.Provider == string.Empty && provider == ClientResourceProviders.DefaultCssProvider))
                            && !this.StylesheetsToExclude.Contains(s.Name))
                        .OrderBy(l => l.Priority));
            }

            if (resourceType is ResourceType.Script or ResourceType.All)
            {
                sortedList.AddRange(
                    this.Scripts
                        .Where(s =>
                            (s.Provider == provider || (s.Provider == string.Empty && provider == ClientResourceProviders.DefaultJsProvider))
                            && !this.ScriptsToExclude.Contains(s.Name))
                        .OrderBy(s => s.Priority));
            }

            if (System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
            {
                sortedList = sortedList.ConvertAll(resource =>
                {
                    if (resource.ResolvedPath.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                        (this.hostSettings.CdnEnabled && !string.IsNullOrEmpty(resource.CdnUrl)))
                    {
                        return resource;
                    }

                    var ext = Path.GetExtension(resource.ResolvedPath);
                    var rtlResolvedPath = Path.ChangeExtension(resource.ResolvedPath, ".rtl" + ext);
                    var cleanRtlResolvedPath = rtlResolvedPath.TrimStart('~').Replace("/", "\\");
                    var physicalPath = Path.Combine(
                        this.appStatus.ApplicationMapPath,
                        cleanRtlResolvedPath.TrimStart('\\'));

                    if (!File.Exists(physicalPath))
                    {
                        return resource;
                    }

                    resource.ResolvedPath = rtlResolvedPath;
					Logger.LogDebug("RenderDependencies(resourceType={ResourceType},provider={Provider},applicationPath={ApplicationPath}): resource.ResolvedPath → {Arg3}", resourceType, provider, applicationPath, resource.ResolvedPath);

                    return resource;
                });
            }

            var crmVersion = this.hostSettings.DebugMode ? DateTimeOffset.UtcNow.ToUnixTimeSeconds() : this.crmVersion;

			Logger.LogDebug("<<< Exiting RenderDependencies(resourceType={ResourceType},provider={Provider},applicationPath={ApplicationPath})", resourceType, provider, applicationPath);
            return string.Join(string.Empty, sortedList.Select(resource => resource.Render(unchecked((int)crmVersion), this.hostSettings.CdnEnabled, applicationPath)));
        }

        private List<T> AddResource<T>(List<T> resources, T resource)
            where T : IResource
        {
			Logger.LogDebug(">>> Entering AddResource(resources={Resources},resource={Resource})", resources, resource);
            resource.ResolvedPath = this.ResolvePath(resource.FilePath, resource.PathNameAlias);
            Logger.Debug($"Adding resource {resource.ResolvedPath} to CRC id {this.controllerId} which currently has {resources.Count} resources");

            if (this.hasBegunRendering)
            {
                Logger.Error($"Cannot add resource {resource.ResolvedPath} to CRC id {this.controllerId} because rendering has already begun");

                ////throw new InvalidOperationException("Cannot add resources after rendering has begun.");
            }

            resources.RemoveAll(l => string.Equals(l.ResolvedPath, resource.ResolvedPath, StringComparison.OrdinalIgnoreCase)); // remove any existing link with the same key (i.e. exactly the same resolved path)
            if (!string.IsNullOrEmpty(resource.Name))
            {
                // if a resource with the same name and force version is already present we ignore this one
                if (resources.Exists(r => string.Equals(r.Name, resource.Name, StringComparison.OrdinalIgnoreCase) && r.ForceVersion))
                {
                    return resources;
                }

                // If we are forcing the version, we need to remove any existing link with the same name
                if (resource.ForceVersion)
                {
                    resources.RemoveAll(r => string.Equals(r.Name, resource.Name, StringComparison.OrdinalIgnoreCase));
                }

                // If we have a version, we need to remove any existing link with the same name and a lower version
                if (!string.IsNullOrEmpty(resource.Version))
                {
                    resources.RemoveAll(r => string.Equals(r.Name, resource.Name, StringComparison.OrdinalIgnoreCase) && VersionIsLessThan(r.Version, resource.Version));

                    // If we have an existing link with the same name and a higher version, we do not add this link
                    if (resources.Exists(r => string.Equals(r.Name, resource.Name, StringComparison.OrdinalIgnoreCase) && !VersionIsLessThan(r.Version, resource.Version)))
                    {
						Logger.LogDebug(">>> Entering VersionIsLessThan(version={Version},otherVersion={OtherVersion})", version, otherVersion);
						Logger.LogDebug("<<< Exiting VersionIsLessThan(version={Version},otherVersion={OtherVersion})", version, otherVersion);
                        return resources;
                    }
                }
            }

            resources.Add(resource);
			Logger.LogDebug("<<< Exiting AddResource(resources={Resources},resource={Resource})", resources, resource);
            return resources;

            static bool VersionIsLessThan(string version, string otherVersion)
            {
				Logger.LogDebug(">>> Entering VersionIsLessThan(version={Version},otherVersion={OtherVersion})", version, otherVersion);
                if (Version.TryParse(version, out var parsedVersion) &&
                    Version.TryParse(otherVersion, out var otherParsedVersion))
                {
				Logger.LogDebug("<<< Exiting VersionIsLessThan(version={Version},otherVersion={OtherVersion})", version, otherVersion);
                    return parsedVersion < otherParsedVersion;
                }

                return string.Compare(version, otherVersion, StringComparison.OrdinalIgnoreCase) < 0;
            }
        }

        private string ResolvePath(string filePath, string pathNameAlias)
        {
			Logger.LogDebug(">>> Entering ResolvePath(filePath={FilePath},pathNameAlias={PathNameAlias})", filePath, pathNameAlias);
            if (string.IsNullOrEmpty(filePath))
            {
                return filePath;
            }

            if (filePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                // Path is assumed to be fully qualified
                return filePath;
            }

            if (filePath.StartsWith("//", StringComparison.OrdinalIgnoreCase))
            {
                // Path is assumed to be fully qualified
                return filePath;
            }

            // Path is either a relative path including the application path or a path starting with a tilde or a path relative to the path name alias
            filePath = filePath.Replace(@"\", "/");
            if (!string.IsNullOrEmpty(pathNameAlias))
            {
				Logger.LogDebug("ResolvePath(filePath={FilePath},pathNameAlias={PathNameAlias}): filePath → {FilePath1}", filePath, pathNameAlias, filePath);
                if (this.PathNameAliases.TryGetValue(pathNameAlias, out var alias))
                {
                    return $"{alias.TrimEnd('/')}/{filePath.TrimStart('/')}";
                }
            }

			Logger.LogDebug("<<< Exiting ResolvePath(filePath={FilePath},pathNameAlias={PathNameAlias})", filePath, pathNameAlias);
            return filePath;
        }
    }
}
