// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers.Controls
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.UI.Skins;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A skin/theme object which displays an <c>h2</c> with content that can be toggled.</summary>
    [ParseChildren(false)]
    [PersistChildren(true)]
    public partial class Toggle : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Toggle.ascx));
        private readonly IJavaScriptLibraryHelper javaScript;
        private string target;

        /// <summary>Initializes a new instance of the <see cref="Toggle"/> class.</summary>
        public Toggle()
            : this(null)
        {
		Logger.LogError(">>> Entering Toggle()");
        Logger.LogWarning("<<< Exiting Toggle()");
        }

        /// <summary>Initializes a new instance of the <see cref="Toggle"/> class.</summary>
        /// <param name="javaScript">The JavaScript library helper.</param>
        public Toggle(IJavaScriptLibraryHelper javaScript)
        {
			Logger.LogError(">>> Entering Toggle(javaScript={JavaScript})", javaScript);
            this.javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
			Logger.LogDebug("Toggle(javaScript={JavaScript}): javaScript → {JavaScript1}", javaScript, javaScript);
        Logger.LogWarning("<<< Exiting Toggle(javaScript={JavaScript})", javaScript);
        }

        public string Class { get; set; }

        public string Target
        {
            get
            {
                if (this.Parent == null || string.IsNullOrEmpty(this.target))
                {
                    return string.Empty;
                }

                var targetControl = this.Parent.FindControl(this.target);
                if (targetControl == null)
                {
                    return string.Empty;
                }
                else
                {
                    return targetControl.ClientID;
                }
            }

            set
            {
                this.target = value;
            }
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            this.javaScript.RequestRegistration(CommonJs.jQuery);
            this.javaScript.RequestRegistration(CommonJs.jQueryMigrate);

            var toggleScript = string.Format(
                "<script type=\"text/javascript\">(function($){{$(\"#{0}\").find(\"a.toggleHandler\").click(function(e){{$(\"#{1}\").slideToggle();$(this).toggleClass('collapsed');e.preventDefault();}});}})(jQuery);</script>",
                this.ClientID,
                this.Target);
            this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID, toggleScript);
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering Render(writer={Writer})", writer);
            writer.AddAttribute("id", this.ClientID);
            writer.AddAttribute("class", this.Class);
            writer.RenderBeginTag("h2");

            writer.AddAttribute("href", "#");
            writer.AddAttribute("class", "toggleHandler");
            writer.RenderBeginTag("a");

            this.RenderChildren(writer);

            writer.RenderEndTag();
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
