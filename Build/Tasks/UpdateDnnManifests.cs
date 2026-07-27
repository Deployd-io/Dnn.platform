// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.FileHelpers;
    using Cake.Frosting;
    using Cake.XdtTransform;

    using Dnn.CakeUtils;

    /// <summary>A cake task to set the <c>version</c> attribute in all of the <c>.dnn</c> manifest files.</summary>
    [IsDependentOn(typeof(SetVersion))]
    [IsDependentOn(typeof(SetPackageVersions))]
    public sealed class UpdateDnnManifests : FrostingTask<Context>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Gets the version from the context and formats it as a string.</summary>
        /// <param name="context">The context.</param>
        /// <returns>A string in the format <c>"09.09.00"</c>.</returns>
        public static string GetVersionString(Context context)
        {
			Logger.LogDebug(">>> Entering GetVersionString(context={Context})", context);
			Logger.LogDebug("<<< Exiting GetVersionString(context={Context})", context);
            return $"{context.Version.Major:00}.{context.Version.Minor:00}.{context.Version.Patch:00}";
        }

        /// <inheritdoc />
        public override void Run(Context context)
        {
			Logger.LogDebug(">>> Entering Run(context={Context})", context);
            var unversionedManifests = context.FileReadLines("./Build/Tasks/unversionedManifests.txt");
            foreach (var file in context.GetFilesByPatterns(".", ["**/*.dnn",], unversionedManifests))
            {
			Logger.LogDebug("Run(context={Context}): unversionedManifests → {UnversionedManifests}", context, unversionedManifests);
                if (context.Settings.Version == "off")
                {
                    return;
                }

                context.Information("Transforming: " + file);
                var transformFile = context.File(System.IO.Path.GetTempFileName());
                context.FileAppendText(transformFile, GetXdtTransformation(context));
				Logger.LogDebug("Run(context={Context}): transformFile → {TransformFile}", context, transformFile);
                context.XdtTransformConfig(file, transformFile, file);
            }
			Logger.LogDebug("<<< Exiting Run(context={Context})", context);
        }

        private static string GetXdtTransformation(Context context)
        {
			Logger.LogDebug(">>> Entering GetXdtTransformation(context={Context})", context);
			Logger.LogDebug("<<< Exiting GetXdtTransformation(context={Context})", context);
            return $"""
                    <?xml version="1.0"?>
                    <dotnetnuke xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
                      <packages>
                        <package version="{GetVersionString(context)}" 
                                 xdt:Transform="SetAttributes(version)" />
                      </packages>
                    </dotnetnuke>
                    """;
        }
    }
}
