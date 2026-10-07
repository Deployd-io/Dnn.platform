// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>Creates a control that render one item in a list ($lt;li> control).</summary>
    public class DnnUnsortedListItem : WebControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnUnsortedListItem));
        /// <summary>Initializes a new instance of the <see cref="DnnUnsortedListItem"/> class.</summary>
        public DnnUnsortedListItem()
            : base(HtmlTextWriterTag.Li)
        {
		Logger.LogError(">>> Entering DnnUnsortedListItem()");
        Logger.LogWarning("<<< Exiting DnnUnsortedListItem()");
        }

        /// <summary>Adds the controls.</summary>
        /// <param name="childControls">The controls to add.</param>
        public void AddControls(params Control[] childControls)
        {
			Logger.LogError(">>> Entering AddControls(childControls={ChildControls})", childControls);
            foreach (var childControl in childControls)
            {
                if (childControl != null)
                {
                    this.Controls.Add(childControl);
                }
            }
			Logger.LogWarning("<<< Exiting AddControls(childControls={ChildControls})", childControls);
        }

        /// <inheritdoc />
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering AddAttributesToRender(writer={Writer})", writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            }

            if (!string.IsNullOrEmpty(this.ToolTip))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, this.ToolTip);
            }
			Logger.LogWarning("<<< Exiting AddAttributesToRender(writer={Writer})", writer);
        }
    }
}
