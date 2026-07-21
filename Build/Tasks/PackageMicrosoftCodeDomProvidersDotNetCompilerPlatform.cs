// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Build.Tasks;

using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

using Dnn.CakeUtils;

/// <summary>A cake task to generate the Microsoft.CodeDom.Providers.DotNetCompilerPlatform package.</summary>
public sealed class PackageMicrosoftCodeDomProvidersDotNetCompilerPlatform()
    : PackageComponentTaskBase("Microsoft.CodeDom.Providers.DotNetCompilerPlatform")
{
	Logger.LogDebug(">>> Entering PackageMicrosoftCodeDomProvidersDotNetCompilerPlatform()");
    /// <inheritdoc />
	Logger.LogDebug("<<< Exiting PackageMicrosoftCodeDomProvidersDotNetCompilerPlatform()");
    public override async Task RunAsync(Context context)
    {
		Logger.LogDebug(">>> Entering RunAsync(context={Context})", context);
        await base.RunAsync(context);

        var binDir = GetBinDir(context);
        using var zipStream = new MemoryStream();
		Logger.LogDebug("RunAsync(context={Context}): binDir → {BinDir}", context, binDir);
		Logger.LogDebug("RunAsync(context={Context}): zipStream → {ZipStream}", context, zipStream);
        await ZipFile.CreateFromDirectoryAsync(binDir.Combine("roslyn").ToString(), zipStream);
        await zipStream.FlushAsync();
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        zipStream.Position = 0;
		Logger.LogDebug("RunAsync(context={Context}): zipStream.Position → {Arg1}", context, zipStream.Position);
        context.AddStreamToZip(this.GetPackageZipPath(context, binDir), zipStream, "roslyn.zip", append: true);
		Logger.LogDebug("<<< Exiting RunAsync(context={Context})", context);
    }
}
