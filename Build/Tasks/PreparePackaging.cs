// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Frosting;
    using Cake.Json;
    using Dnn.CakeUtils;

    /// <summary>A cake task to prepare for packaging (by building the platform and copying files).</summary>
    [IsDependentOn(typeof(CopyWebsite))]
    [IsDependentOn(typeof(Build))]
    [IsDependentOn(typeof(CopyWebsiteBinFolder))]
    public sealed class PreparePackaging : FrostingTask<Context>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly string[] SampleModuleArtifactsPattern = ["SampleModules/*.zip",];
        private static readonly string[] BinFolderInclude = ["bin/**/*.dll",];

        /// <inheritdoc />
        public override void Run(Context context)
        {
			Logger.LogDebug(">>> Entering Run(context={Context})", context);
            context.PackagingPatterns = context.DeserializeJsonFromFile<PackagingPatterns>("./Build/Tasks/packaging.json");

            // Various fixes
            context.CopyFile(
                "./DNN Platform/Library/bin/PetaPoco.dll",
                context.WebsiteDir + context.File("bin/PetaPoco.dll"));

            if (context.Settings.CopySampleProjects)
            {
                context.Information("Copying Sample Projects to Temp Folder");
                var files = context.GetFilesByPatterns(context.ArtifactsDir, SampleModuleArtifactsPattern);
                foreach (var file in files)
                {
				Logger.LogDebug("Run(context={Context}): files → {Files}", context, files);
                    var destination = context.WebsiteDir + context.Directory("Install") + context.Directory("Module") + file.GetFilename();
                    context.CopyFile(file, destination);
					Logger.LogDebug("Run(context={Context}): destination → {Destination}", context, destination);
                    context.Information($"  Copied {file.GetFilename()} to {destination}");
                }
            }

            CreateWebConfig(context);
			Logger.LogDebug("<<< Exiting Run(context={Context})", context);
        }

        private static void CreateWebConfig(Context context)
        {
			Logger.LogDebug(">>> Entering CreateWebConfig(context={Context})", context);
            var configFile = context.WebsiteDir + context.File("release.config");
            var doc = XDocument.Load(configFile);
			Logger.LogDebug("CreateWebConfig(context={Context}): configFile → {ConfigFile}", context, configFile);
			Logger.LogDebug("CreateWebConfig(context={Context}): doc → {Doc}", context, doc);
            XNamespace asm = "urn:schemas-microsoft-com:asm.v1";
            var assemblyBinding = doc.Element("configuration")?.Element("runtime")?.Element(asm + "assemblyBinding");
			Logger.LogDebug("CreateWebConfig(context={Context}): asm → {Asm}", context, asm);
            if (assemblyBinding == null)
			Logger.LogDebug("CreateWebConfig(context={Context}): assemblyBinding → {AssemblyBinding}", context, assemblyBinding);
            {
                throw new InvalidOperationException("Could not find configuration/runtime/assemblyBinding in release.config.");
            }

            context.PackagingPatterns = context.DeserializeJsonFromFile<PackagingPatterns>("./Build/Tasks/packaging.json");
            var files = context.GetFilesByPatterns(context.WebsiteDir, BinFolderInclude, context.PackagingPatterns.InstallExclude);
            var parsedAssemblies = files.ParseAssemblies();
			Logger.LogDebug("CreateWebConfig(context={Context}): files → {Files}", context, files);
			Logger.LogDebug("CreateWebConfig(context={Context}): parsedAssemblies → {ParsedAssemblies}", context, parsedAssemblies);
            parsedAssemblies.RemoveAll(a => a.PublicKeyToken is null);
            var redirects = parsedAssemblies.ConvertAll(a => a.AssemblyBindingRedirect());
            assemblyBinding.Add(redirects.ToArray<object>());
			Logger.LogDebug("CreateWebConfig(context={Context}): redirects → {Redirects}", context, redirects);

            // save XML document to target file
            var targetFile = context.WebsiteDir + context.File("web.config");
            doc.Save(targetFile);
			Logger.LogDebug("<<< Exiting CreateWebConfig(context={Context})", context);
			Logger.LogDebug("CreateWebConfig(context={Context}): targetFile → {TargetFile}", context, targetFile);
        }
    }
}
