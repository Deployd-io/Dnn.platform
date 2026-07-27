// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities;

using System.IO;
using System.Reflection;
using System.Web;

public class DnnUnitTest
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    public string WebsiteAppPath { get; set; }

    public string WebsitePhysicalAppPath { get; set; }

    public virtual void OneTimeSetUp()
    {
		Logger.LogDebug(">>> Entering OneTimeSetUp()");
        var uri = new System.Uri(Assembly.GetExecutingAssembly().CodeBase);
        var path = HttpUtility.UrlDecode(Path.GetFullPath(uri.AbsolutePath));
		Logger.LogDebug("OneTimeSetUp(): uri → {Uri}", uri);
		Logger.LogDebug("OneTimeSetUp(): path → {Path}", path);

        this.WebsiteAppPath = "http://localhost/DNN_Platform";
		Logger.LogDebug("OneTimeSetUp(): this.WebsiteAppPath → {Arg0}", this.WebsiteAppPath);
        var websiteRootPath = path.Substring(0, path.IndexOf("DNN Platform", System.StringComparison.Ordinal));
        this.WebsitePhysicalAppPath = Path.Combine(websiteRootPath, "Website");
		Logger.LogDebug("<<< Exiting OneTimeSetUp()");
		Logger.LogDebug("OneTimeSetUp(): websiteRootPath → {WebsiteRootPath}", websiteRootPath);
    }
}
