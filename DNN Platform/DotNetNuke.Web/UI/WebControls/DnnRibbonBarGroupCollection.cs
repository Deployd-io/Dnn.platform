// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls;

using System;
using System.Web.UI;

/// <summary>A collection of <see cref="DnnRibbonBarGroup"/> controls.</summary>
public class DnnRibbonBarGroupCollection : TypedControlCollection<DnnRibbonBarGroup>
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <summary>Initializes a new instance of the <see cref="DnnRibbonBarGroupCollection"/> class.</summary>
    /// <param name="owner">The owner control.</param>
    public DnnRibbonBarGroupCollection(Control owner)
        : base(owner)
    {
	Logger.LogDebug(">>> Entering DnnRibbonBarGroupCollection(owner={Owner})", owner);
	Logger.LogDebug("<<< Exiting DnnRibbonBarGroupCollection(owner={Owner})", owner);
    }

    /// <inheritdoc />
    protected override void ThrowOnInvalidControlType()
    {
		Logger.LogDebug(">>> Entering ThrowOnInvalidControlType()");
        throw new ArgumentException("DnnRibbonBarGroupCollection must contain controls of type DnnRibbonBarGroup");
    }
}
