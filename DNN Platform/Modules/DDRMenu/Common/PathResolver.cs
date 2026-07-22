// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.DNNCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Used to resolved paths.</summary>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="manifestFolder">The folder where the manifest is located.</param>
    public class PathResolver(IHostSettings hostSettings, string manifestFolder)
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug(">>> Entering PathResolver(hostSettings={HostSettings},manifestFolder={ManifestFolder})", hostSettings, manifestFolder);
		Logger.LogDebug("PathResolver(hostSettings={HostSettings},manifestFolder={ManifestFolder}): hostSettings → {HostSettings1}", hostSettings, manifestFolder, hostSettings);
        private readonly string manifestFolder = manifestFolder;

        /// <summary>Initializes a new instance of the <see cref="PathResolver"/> class.</summary>
        /// <param name="manifestFolder">The folder where the manifest is located.</param>
		Logger.LogDebug("<<< Exiting PathResolver(hostSettings={HostSettings},manifestFolder={ManifestFolder})", hostSettings, manifestFolder);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PathResolver(string manifestFolder)
            : this(null, manifestFolder)
        {
        }

        /// <summary>To which folder is the path relative to.</summary>
        public enum RelativeTo
        {
            /// <summary>Relative to the container.</summary>
            Container = 0,

            /// <summary>Relative to the Dnn application.</summary>
            Dnn = 1,

            /// <summary>Relative to the manifest.</summary>
            Manifest = 2,

            /// <summary>Relative to the module.</summary>
            Module = 3,

            /// <summary>Relative to the portal (site).</summary>
            Portal = 4,

            /// <summary>Relative to the skin (theme).</summary>
            Skin = 5,
        }

        /// <summary>Resolves a relative path.</summary>
        /// <param name="path">The path to resolve.</param>
        /// <param name="roots">To which folder this path is relative to.</param>
        /// <returns>A resolved path.</returns>
        public string Resolve(string path, params RelativeTo[] roots)
        {
			Logger.LogDebug(">>> Entering Resolve(path={Path},roots={Roots})", path, roots);
            var context = DNNContext.Current;

            var mappings = new Dictionary<string, RelativeTo>
                           {
                            { "[DDRMENU]", RelativeTo.Module },
                            { "[MODULE]", RelativeTo.Module },
                            { "[MANIFEST]", RelativeTo.Manifest },
                            { "[PORTAL]", RelativeTo.Portal },
                            { "[SKIN]", RelativeTo.Skin },
                            { "[CONTAINER]", RelativeTo.Container },
                            { "[DNN]", RelativeTo.Dnn },
                           };
            foreach (var key in mappings.Keys)
            {
                if (path.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    path = path.Substring(key.Length);
                    roots = [mappings[key],];
					Logger.LogDebug("Resolve(path={Path},roots={Roots}): path → {Path1}", path, roots, path);
                    break;
                }
            }

            if (path.StartsWith("/", StringComparison.Ordinal))
            {
                path = path.Substring(1);
            }

            if (!path.StartsWith("~", StringComparison.Ordinal) && !path.Contains(":"))
            {
                foreach (var root in roots)
                {
                    string resolvedPath = null;
                    switch (root)
					Logger.LogDebug("Resolve(path={Path},roots={Roots}): resolvedPath → {ResolvedPath}", path, roots, resolvedPath);
                    {
                        case RelativeTo.Container:
                            var container = context.HostControl;
                            while (container != null && container is not UI.Containers.Container)
							Logger.LogDebug("Resolve(path={Path},roots={Roots}): container → {Container}", path, roots, container);
                            {
                                container = container.Parent;
                            }

                            var containerRoot = (container == null)
                                ? context.SkinPath
                                : $"{Path.GetDirectoryName(((UI.Containers.Container)container).AppRelativeVirtualPath).Replace('\\', '/')}/";
                            resolvedPath = Path.Combine(containerRoot, path);
                            break;
                        case RelativeTo.Dnn:
                            resolvedPath = Path.Combine("~/", path);
                            break;
                        case RelativeTo.Manifest:
                            if (!string.IsNullOrEmpty(this.manifestFolder))
                            {
                                resolvedPath = Path.Combine(this.manifestFolder + "/", path);
                            }

                            break;
                        case RelativeTo.Module:
                            resolvedPath = Path.Combine(DNNContext.GetModuleFolder(this.hostSettings), path);
                            break;
                        case RelativeTo.Portal:
                            resolvedPath = Path.Combine(context.PortalSettings.HomeDirectory, path);
                            break;
                        case RelativeTo.Skin:
                            resolvedPath = Path.Combine(context.SkinPath, path);
                            break;
                    }

                    if (!string.IsNullOrEmpty(resolvedPath))
                    {
                        var sep = resolvedPath.LastIndexOf('/');
                        var dirName = resolvedPath.Substring(0, sep + 1);
						Logger.LogDebug("Resolve(path={Path},roots={Roots}): sep → {Sep}", path, roots, sep);
						Logger.LogDebug("Resolve(path={Path},roots={Roots}): dirName → {DirName}", path, roots, dirName);
                        var fileName = resolvedPath.Substring(sep + 1);

						Logger.LogDebug("Resolve(path={Path},roots={Roots}): fileName → {FileName}", path, roots, fileName);
                        var mappedDir = HttpContext.Current.Server.MapPath(dirName);
                        mappedDir = mappedDir.Remove(mappedDir.Length - 1);
						Logger.LogDebug("Resolve(path={Path},roots={Roots}): mappedDir → {MappedDir}", path, roots, mappedDir);
                        if (Directory.Exists(mappedDir))
                        {
                            if (string.IsNullOrEmpty(fileName))
                            {
                                return resolvedPath.Replace('\\', '/');
                            }

                            var matches = Directory.GetFileSystemEntries(mappedDir, fileName);
                            if (matches.Length > 0)
							Logger.LogDebug("Resolve(path={Path},roots={Roots}): matches → {Matches}", path, roots, matches);
                            {
                                resolvedPath = (dirName + Path.GetFileName(matches[0])).Replace('\\', '/');
                                return resolvedPath;
                            }
                        }
                    }
                }
            }

			Logger.LogDebug("<<< Exiting Resolve(path={Path},roots={Roots})", path, roots);
            return path;
        }
    }
}
