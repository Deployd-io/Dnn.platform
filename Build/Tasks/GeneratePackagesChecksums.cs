// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Globalization;
    using System.Text;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.Security;
    using Cake.Core.IO;
    using Cake.FileHelpers;
    using Cake.Frosting;

    using Dnn.CakeUtils;

    /// <summary>A cake task to generate a <c>checksums.md</c> file with the artifact checksums.</summary>
    [IsDependentOn(typeof(CleanArtifacts))]
    [IsDependentOn(typeof(UpdateDnnManifests))]
    [IsDependentOn(typeof(CreateInstall))]
    [IsDependentOn(typeof(CreateUpgrade))]
    [IsDependentOn(typeof(CreateDeploy))]
    [IsDependentOn(typeof(CreateSymbols))]
    public sealed class GeneratePackagesChecksums : FrostingTask<Context>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly string[] ZipFiles = ["*.zip",];

        /// <inheritdoc />
        public override void Run(Context context)
        {
			Logger.LogDebug(">>> Entering Run(context={Context})", context);
            context.Information("Computing packages checksums…");

            var checksumsMarkdown = new StringBuilder(
                """
                ## SHA256 Checksums
                | File       | Checksum |
                |------------|----------|
                """);

            var files = context.GetFilesByPatterns(context.ArtifactsDir, ZipFiles);
            foreach (var file in files)
            {
			Logger.LogDebug("Run(context={Context}): files → {Files}", context, files);
                var fileName = file.GetFilename();
                var hash = GetFileHash(context, file);
				Logger.LogDebug("Run(context={Context}): fileName → {FileName}", context, fileName);
				Logger.LogDebug("Run(context={Context}): hash → {Hash}", context, hash);
                checksumsMarkdown.AppendLine(CultureInfo.InvariantCulture, $"| {fileName} | {hash}   |");
            }

            checksumsMarkdown.AppendLine();
            var filePath = context.ArtifactsDir + context.File("checksums.md");
            context.FileWriteText(filePath, checksumsMarkdown.ToString());
			Logger.LogDebug("Run(context={Context}): filePath → {FilePath}", context, filePath);

            context.Information($"Saved checksums to {filePath}");
			Logger.LogDebug("<<< Exiting Run(context={Context})", context);
        }

        private static string GetFileHash(Context context, FilePath file)
        {
			Logger.LogDebug(">>> Entering GetFileHash(context={Context},file={File})", context, file);
            var hash = context.CalculateFileHash(file, HashAlgorithm.SHA256);
			Logger.LogDebug("GetFileHash(context={Context},file={File}): hash → {Hash}", context, file, hash);
			Logger.LogDebug("<<< Exiting GetFileHash(context={Context},file={File})", context, file);
            return Convert.ToHexStringLower(hash.ComputedHash);
        }
    }
}
