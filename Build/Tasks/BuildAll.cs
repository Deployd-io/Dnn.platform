// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using Cake.Common.IO;
    using Cake.Frosting;

    /// <summary>A cake task to compile the platform and create all of the packages.</summary>
    /// <remarks>This is the task run during CI.</remarks>
    [IsDependentOn(typeof(CleanArtifacts))]
    [IsDependentOn(typeof(UpdateDnnManifests))]
    [IsDependentOn(typeof(GenerateSecurityAnalyzerChecksums))]
    [IsDependentOn(typeof(SetPackageVersions))]
    [IsDependentOn(typeof(CreateInstall))]
    [IsDependentOn(typeof(CreateUpgrade))]
    [IsDependentOn(typeof(CreateDeploy))]
    [IsDependentOn(typeof(CreateSymbols))]
    [IsDependentOn(typeof(CreateNugetPackages))]
    [IsDependentOn(typeof(GeneratePackagesChecksums))]
    public sealed class BuildAll : FrostingTask<Context>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override void Run(Context context)
        {
			Logger.LogDebug(">>> Entering Run(context={Context})", context);
            RevertSqlDataProvider(context);
			Logger.LogDebug("<<< Exiting Run(context={Context})", context);
        }

        private static void RevertSqlDataProvider(Context context)
        {
			Logger.LogDebug(">>> Entering RevertSqlDataProvider(context={Context})", context);
            var fileName = context.File($"{context.GetTwoDigitsVersionNumber()}.SqlDataProvider");
            var filePath = context.Directory("./Dnn Platform/Website/Providers/DataProviders/SqlDataProvider/") + fileName;
			Logger.LogDebug("RevertSqlDataProvider(context={Context}): fileName → {FileName}", context, fileName);
			Logger.LogDebug("RevertSqlDataProvider(context={Context}): filePath → {FilePath}", context, filePath);
            if (!context.SqlDataProviderExists && context.FileExists(filePath))
            {
                context.DeleteFile(filePath);
            }
			Logger.LogDebug("<<< Exiting RevertSqlDataProvider(context={Context})", context);
        }
    }
}
