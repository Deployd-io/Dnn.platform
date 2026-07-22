// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.Security;
    using Cake.Core.IO;
    using Cake.FileHelpers;
    using Cake.Frosting;

    /// <summary>A cake task to generate the <c>Default.aspx</c> checksum for the Security Analyzer.</summary>
    [IsDependentOn(typeof(SetVersion))]
    public sealed class GenerateSecurityAnalyzerChecksums : FrostingTask<Context>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override void Run(Context context)
        {
			Logger.LogDebug(">>> Entering Run(context={Context})", context);
            context.Information("Generating default.aspx checksum…");
            var sourceFile = context.File("./Dnn Platform/Website/Default.aspx");
            var destFile = context.File("./Dnn.AdminExperience/Dnn.PersonaBar.Extensions/Components/Security/Resources/sums.resources");
			Logger.LogDebug("Run(context={Context}): sourceFile → {SourceFile}", context, sourceFile);
			Logger.LogDebug("Run(context={Context}): destFile → {DestFile}", context, destFile);
            var hash = CalculateSha(context, sourceFile);
            var content = $"""
						Logger.LogDebug("Run(context={Context}): hash → {Hash}", context, hash);
                           <checksums>
                             <sum name="Default.aspx" version="{context.Version.MajorMinorPatch}" type="Platform" sum="{hash}" />
                           </checksums>
                           """;
            context.FileWriteText(destFile, content);
			Logger.LogDebug("<<< Exiting Run(context={Context})", context);
        }

        private static string CalculateSha(Context context, FilePath file)
        {
			Logger.LogDebug(">>> Entering CalculateSha(context={Context},file={File})", context, file);
            var hash = context.CalculateFileHash(file, HashAlgorithm.SHA256);
			Logger.LogDebug("CalculateSha(context={Context},file={File}): hash → {Hash}", context, file, hash);
			Logger.LogDebug("<<< Exiting CalculateSha(context={Context},file={File})", context, file);
            return Convert.ToHexStringLower(hash.ComputedHash);
        }
    }
}
