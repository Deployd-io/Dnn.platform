// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Internal.SourceGenerators;

using System;
using System.Diagnostics;

/// <summary>Marks a type or member as deprecated.</summary>
[Conditional("DNN_SOURCE_GENERATOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Method)]
public class DnnDeprecatedAttribute : Attribute
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <summary>Initializes a new instance of the <see cref="DnnDeprecatedAttribute"/> class.</summary>
    /// <param name="majorVersion">The major version in which the type or member was deprecated.</param>
    /// <param name="minorVersion">The minor version in which the type or member was deprecated.</param>
    /// <param name="patchVersion">The patch version in which the type or member was deprecated.</param>
    /// <param name="replacement">The suggested replacement or alternative.</param>
    public DnnDeprecatedAttribute(int majorVersion, int minorVersion, int patchVersion, string replacement)
        : this(majorVersion, minorVersion, patchVersion, replacement, false)
    {
	Logger.LogDebug(">>> Entering DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement})", majorVersion, minorVersion, patchVersion, replacement);
	Logger.LogDebug(">>> Entering DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement},error={Error})", majorVersion, minorVersion, patchVersion, replacement, error);
	Logger.LogDebug("<<< Exiting DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement},error={Error})", majorVersion, minorVersion, patchVersion, replacement, error);
    }

    /// <summary>Initializes a new instance of the <see cref="DnnDeprecatedAttribute"/> class.</summary>
    /// <param name="majorVersion">The major version in which the type or member was deprecated.</param>
    /// <param name="minorVersion">The minor version in which the type or member was deprecated.</param>
    /// <param name="patchVersion">The patch version in which the type or member was deprecated.</param>
    /// <param name="replacement">The suggested replacement or alternative.</param>
    /// <param name="error"><see langword="true"/> if the obsolete element usage generates a compiler error; <see langword="false"/> if it generates a compiler warning.</param>
    public DnnDeprecatedAttribute(int majorVersion, int minorVersion, int patchVersion, string replacement, bool error)
    {
        this.MajorVersion = majorVersion;
        this.MinorVersion = minorVersion;
		Logger.LogDebug("DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement},error={Error}): this.MajorVersion → {Arg5}", majorVersion, minorVersion, patchVersion, replacement, error, this.MajorVersion);
		Logger.LogDebug("DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement},error={Error}): this.MinorVersion → {Arg5}", majorVersion, minorVersion, patchVersion, replacement, error, this.MinorVersion);
        this.PatchVersion = patchVersion;
        this.Replacement = replacement;
        this.IsError = error;
		Logger.LogDebug("DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement},error={Error}): this.PatchVersion → {Arg5}", majorVersion, minorVersion, patchVersion, replacement, error, this.PatchVersion);
		Logger.LogDebug("DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement},error={Error}): this.Replacement → {Arg5}", majorVersion, minorVersion, patchVersion, replacement, error, this.Replacement);
		Logger.LogDebug("DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement},error={Error}): this.IsError → {Arg5}", majorVersion, minorVersion, patchVersion, replacement, error, this.IsError);
        this.RemovalVersion = majorVersion + 2;
		Logger.LogDebug("<<< Exiting DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement},error={Error})", majorVersion, minorVersion, patchVersion, replacement, error);
		Logger.LogDebug("DnnDeprecatedAttribute(majorVersion={MajorVersion},minorVersion={MinorVersion},patchVersion={PatchVersion},replacement={Replacement},error={Error}): this.RemovalVersion → {Arg5}", majorVersion, minorVersion, patchVersion, replacement, error, this.RemovalVersion);
    }

    /// <summary>Gets the major version in which the type or member was deprecated.</summary>
    public int MajorVersion { get; }

    /// <summary>Gets the minor version in which the type or member was deprecated.</summary>
    public int MinorVersion { get; }

    /// <summary>Gets the patch version in which the type or member was deprecated.</summary>
    public int PatchVersion { get; }

    /// <summary>Gets the suggested replacement or alternative.</summary>
    public string Replacement { get; }

    /// <summary>Gets or sets the major version in which the type or member will be removed.</summary>
    public int RemovalVersion { get; set; }

    /// <summary>Gets or sets a value indicating whether the compiler will treat usage of the obsolete program element as an error.</summary>
    public bool IsError { get; set; }
}
