// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Icons;

/// <summary>The default <see cref="IIconController"/> implementation.</summary>
internal class IconControllerWrapper : ComponentBase<IIconController, IconControllerWrapper>, IIconController
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public string IconURL(string key)
    {
		Logger.LogDebug(">>> Entering IconURL(key={Key})", key);
		Logger.LogDebug("<<< Exiting IconURL(key={Key})", key);
        return IconController.IconURL(key);
    }

    /// <inheritdoc />
    public string IconURL(string key, string size)
    {
		Logger.LogDebug("<<< Exiting IconURL(key={Key},size={Size})", key, size);
        return IconController.IconURL(key, size);
    }
}
