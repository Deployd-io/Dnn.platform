// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Linq;

    using Cake.Common.IO;
    using Cake.Frosting;

    using Dnn.CakeUtils;

    /// <summary>A cake task to create the Symbols package.</summary>
    [IsDependentOn(typeof(PreparePackaging))]
    [IsDependentOn(typeof(OtherPackages))]
    public sealed class CreateSymbols : FrostingTask<Context>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override void Run(Context context)
        {
			Logger.LogDebug(">>> Entering Run(context={Context})", context);
            context.CreateDirectory(context.ArtifactsDir);
            var packageZip = context.ArtifactsDir + context.File($"DNN_Platform_{context.GetBuildNumber()}_Symbols.zip");
            context.Zip("./Build/Symbols/", packageZip, context.GetFiles("./Build/Symbols/*"));
			Logger.LogDebug("Run(context={Context}): packageZip → {PackageZip}", context, packageZip);

            // Fix for WebUtility symbols missing from bin folder
            context.CopyFiles(
                context.GetFiles("./DNN Platform/DotNetNuke.WebUtility/bin/DotNetNuke.WebUtility.*"),
                context.WebsiteDir + context.Directory("bin/"));
            var files = context.GetFilesByPatterns(
                context.WebsiteDir,
                context.PackagingPatterns.SymbolsInclude,
                context.PackagingPatterns.SymbolsExclude);
            var resFile = context.ZipToBytes(context.WebsiteDir, files);
            context.AddBinaryFileToZip(packageZip, resFile, "Resources.zip", true);
			Logger.LogDebug("<<< Exiting Run(context={Context})", context);
			Logger.LogDebug("Run(context={Context}): resFile → {ResFile}", context, resFile);
        }
    }
}
