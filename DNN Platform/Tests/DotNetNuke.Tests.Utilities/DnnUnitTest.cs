// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities;

using System.IO;
using System.Reflection;
using System.Web;
using Microsoft.Extensions.Logging;

public class DnnUnitTest
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    public string WebsiteAppPath { get; set; }

    public string WebsitePhysicalAppPath { get; set; }

    public virtual void OneTimeSetUp()
    {
		_logger.LogDebug(">>> Entering OneTimeSetUp()");
        var uri = new System.Uri(Assembly.GetExecutingAssembly().CodeBase);
        var path = HttpUtility.UrlDecode(Path.GetFullPath(uri.AbsolutePath));
		_logger.LogDebug("OneTimeSetUp(): path → {Path}", path);
		_logger.LogDebug("OneTimeSetUp(): uri → {Uri}", uri);

        this.WebsiteAppPath = "http://localhost/DNN_Platform";
		_logger.LogDebug("OneTimeSetUp(): this.WebsiteAppPath → {Arg0}", this.WebsiteAppPath);
        var websiteRootPath = path.Substring(0, path.IndexOf("DNN Platform", System.StringComparison.Ordinal));
		_logger.LogDebug("OneTimeSetUp(): websiteRootPath → {WebsiteRootPath}", websiteRootPath);
        this.WebsitePhysicalAppPath = Path.Combine(websiteRootPath, "Website");
		_logger.LogDebug("<<< Exiting OneTimeSetUp()");
    }
}
