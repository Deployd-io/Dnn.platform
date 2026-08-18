// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    /// <summary>A group in the <see cref="DnnRibbonBar"/>.</summary>
    [ParseChildren(true)]
    public class DnnRibbonBarGroup : WebControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnRibbonBarGroup));
        private bool checkToolVisibility = true;
        private HtmlGenericControl contentContainer;

        /// <summary>Initializes a new instance of the <see cref="DnnRibbonBarGroup"/> class.</summary>
        public DnnRibbonBarGroup()
            : base("div")
        {
			Logger.LogDebug(">>> Entering DnnRibbonBarGroup()");
            this.CssClass = "dnnRibbonGroup";
			Logger.LogDebug("DnnRibbonBarGroup(): this.CssClass → {Arg0}", this.CssClass);
			Logger.LogDebug("<<< Exiting DnnRibbonBarGroup()");
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

        /// <summary>Gets or sets the footer template.</summary>
        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Footer { get; set; }

        /// <summary>Gets or sets the content template.</summary>
        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate Content { get; set; }

        /// <summary>Gets or sets a value indicating whether the check tool is visible.</summary>
        public virtual bool CheckToolVisibility
        {
            get
            {
                return this.checkToolVisibility;
            }

            set
            {
                this.checkToolVisibility = value;
            }
        }

        /// <inheritdoc />
        public override Control FindControl(string id)
        {
			Logger.LogDebug(">>> Entering FindControl(id={Id})", id);
            this.EnsureChildControls();
			Logger.LogDebug("<<< Exiting FindControl(id={Id})", id);
            return base.FindControl(id);
        }

        /// <inheritdoc />
        public override void RenderControl(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderControl(writer={Writer})", writer);
            if (this.CheckVisibility())
            {
                this.RenderBeginTag(writer);
                this.RenderChildren(writer);
                this.RenderEndTag(writer);
            }
			Logger.LogDebug("<<< Exiting RenderControl(writer={Writer})", writer);
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            this.Controls.Clear();

            HtmlGenericControl topLeft = new HtmlGenericControl("div");
			Logger.LogDebug("CreateChildControls(): topLeft → {TopLeft}", topLeft);
            topLeft.Attributes.Add("class", "topLeft");
            HtmlGenericControl topRight = new HtmlGenericControl("div");
			Logger.LogDebug("CreateChildControls(): topRight → {TopRight}", topRight);
            topRight.Attributes.Add("class", "topRight");

            HtmlGenericControl bottomLeft = new HtmlGenericControl("div");
			Logger.LogDebug("CreateChildControls(): bottomLeft → {BottomLeft}", bottomLeft);
            bottomLeft.Attributes.Add("class", "bottomLeft");
            HtmlGenericControl bottomRight = new HtmlGenericControl("div");
			Logger.LogDebug("CreateChildControls(): bottomRight → {BottomRight}", bottomRight);
            bottomRight.Attributes.Add("class", "bottomRight");

            this.contentContainer = new HtmlGenericControl("div");
			Logger.LogDebug("CreateChildControls(): contentContainer → {ContentContainer}", contentContainer);
            this.contentContainer.Attributes.Add("class", "content");

            HtmlGenericControl footerContainer = new HtmlGenericControl("div");
			Logger.LogDebug("CreateChildControls(): footerContainer → {FooterContainer}", footerContainer);
            footerContainer.Attributes.Add("class", "footer");

            this.Controls.Add(topLeft);
            this.Controls.Add(topRight);
            this.Controls.Add(this.contentContainer);
            this.Controls.Add(footerContainer);
            this.Controls.Add(bottomLeft);
            this.Controls.Add(bottomRight);

            if (this.Content != null)
            {
                this.Content.InstantiateIn(this.contentContainer);
            }

            if (this.Footer != null)
            {
                this.Footer.InstantiateIn(footerContainer);
            }
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        private static bool AreChildToolsVisible(ref ControlCollection children, ref bool foundTool)
        {
            bool returnValue = false;

            foreach (Control ctrl in children)
            {
                if (ctrl is IDnnRibbonBarTool)
                {
                    foundTool = true;
                    if (ctrl.Visible)
                    {
                        returnValue = true;
                        break;
                    }
                }
                else
                {
                    ControlCollection controls = ctrl.Controls;
                    if (AreChildToolsVisible(ref controls, ref foundTool))
                    {
                        if (foundTool)
                        {
                            returnValue = true;
                            break;
                        }
                    }
                }
            }

            if (!foundTool)
            {
                return true;
            }

            return returnValue;
        }

        private bool CheckVisibility()
        {
			Logger.LogDebug(">>> Entering CheckVisibility()");
            bool returnValue = true;
            if (this.Visible && this.CheckToolVisibility)
			Logger.LogDebug("CheckVisibility(): returnValue → {ReturnValue}", returnValue);
            {
                // Hide group if all tools are invisible
                bool foundTool = false;
                ControlCollection controls = this.contentContainer.Controls;
                returnValue = AreChildToolsVisible(ref controls, ref foundTool);
            }

			Logger.LogDebug("<<< Exiting CheckVisibility()");
            return returnValue;
        }
    }
}
