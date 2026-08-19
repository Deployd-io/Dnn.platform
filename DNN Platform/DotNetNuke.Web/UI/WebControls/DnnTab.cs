// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>A tab control.</summary>
    [ParseChildren(true)]
    public class DnnTab : WebControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnTab));
        /// <summary>Initializes a new instance of the <see cref="DnnTab"/> class.</summary>
        public DnnTab()
            : base("div")
        {
		Logger.LogDebug(">>> Entering DnnTab()");
		Logger.LogDebug("<<< Exiting DnnTab()");
        }

        /// <inheritdoc />
        public override ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        /// <summary>Gets or sets the header template.</summary>
        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Header { get; set; }

        /// <summary>Gets or sets the content template.</summary>
        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Content { get; set; }

        /// <inheritdoc />
        public override Control FindControl(string id)
        {
			Logger.LogDebug(">>> Entering FindControl(id={Id})", id);
            this.EnsureChildControls();
			Logger.LogDebug("<<< Exiting FindControl(id={Id})", id);
            return base.FindControl(id);
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            this.Controls.Clear();

            if (this.Content != null)
            {
                this.Content.InstantiateIn(this);
            }
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer);
            this.RenderBeginTag(writer);
            this.RenderChildren(writer);
            this.RenderEndTag(writer);
			Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
