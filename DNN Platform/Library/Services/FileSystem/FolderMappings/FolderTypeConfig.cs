// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    using System.Collections.Generic;

    /// <summary>HostSettingConfig - A class that represents Install/DotNetNuke.Install.Config/Settings.</summary>
    public class FolderTypeConfig
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="FolderTypeConfig"/> class.</summary>
        public FolderTypeConfig()
        {
			Logger.LogDebug(">>> Entering FolderTypeConfig()");
            this.Settings = new List<FolderTypeSettingConfig>();
			Logger.LogDebug("<<< Exiting FolderTypeConfig()");
        }

        public string Name { get; set; }

        public string Provider { get; set; }

        public IList<FolderTypeSettingConfig> Settings { get; set; }
    }
}
