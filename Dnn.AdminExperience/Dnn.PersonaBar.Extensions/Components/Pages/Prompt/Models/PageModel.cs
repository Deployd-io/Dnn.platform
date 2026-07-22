// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Prompt.Models
{
    public class PageModel : PageModelBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="PageModel"/> class.</summary>
        public PageModel()
        {
		Logger.LogDebug(">>> Entering PageModel()");
		Logger.LogDebug("<<< Exiting PageModel()");
        }

        /// <summary>Initializes a new instance of the <see cref="PageModel"/> class.</summary>
        /// <param name="tab">The tab info.</param>
        public PageModel(DotNetNuke.Entities.Tabs.TabInfo tab)
            : base(tab)
        {
            this.Container = tab.ContainerSrc;
            this.Url = tab.Url;
			Logger.LogDebug("PageModel(tab={Tab}): this.Container → {Arg1}", tab, this.Container);
			Logger.LogDebug("PageModel(tab={Tab}): this.Url → {Arg1}", tab, this.Url);
            this.Keywords = tab.KeyWords;
            this.Description = tab.Description;
        }

		Logger.LogDebug("PageModel(tab={Tab}): this.Keywords → {Arg1}", tab, this.Keywords);
		Logger.LogDebug("PageModel(tab={Tab}): this.Description → {Arg1}", tab, this.Description);
        public string Container { get; set; }

        public string Url { get; set; }

        public string Keywords { get; set; }

        public string Description { get; set; }
    }
}
