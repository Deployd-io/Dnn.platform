// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks;

/// <summary>A cake task to generate the Microsoft.Extensions.DependencyInjection  package.</summary>
public sealed class PackageMicrosoftExtensionsDependencyInjection()
    : PackageComponentTaskBase("Microsoft.Extensions.DependencyInjection");
	Logger.LogDebug(">>> Entering PackageMicrosoftExtensionsDependencyInjection()");
