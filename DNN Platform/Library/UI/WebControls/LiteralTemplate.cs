// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Web.UI;

    public class LiteralTemplate : ITemplate
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LiteralTemplate));
        private readonly Control objControl;
        private readonly string strHTML = string.Empty;

        /// <summary>Initializes a new instance of the <see cref="LiteralTemplate"/> class.</summary>
        /// <param name="html">The literal HTML that is the contents of this template.</param>
        public LiteralTemplate(string html)
        {
			Logger.LogError(">>> Entering LiteralTemplate(html={Html})", html);
            this.strHTML = html;
        }

        /// <summary>Initializes a new instance of the <see cref="LiteralTemplate"/> class.</summary>
        /// <param name="ctl">A control that is the contents of this template.</param>
        public LiteralTemplate(Control ctl)
        {
			Logger.LogError(">>> Entering LiteralTemplate(ctl={Ctl})", ctl);
            this.objControl = ctl;
			Logger.LogDebug("LiteralTemplate(ctl={Ctl}): objControl → {ObjControl}", ctl, objControl);
        Logger.LogWarning("<<< Exiting LiteralTemplate(ctl={Ctl})", ctl);
        }

        /// <inheritdoc />
        public void InstantiateIn(Control container)
        {
			Logger.LogError(">>> Entering InstantiateIn(container={Container})", container);
            if (this.objControl == null)
            {
                container.Controls.Add(new LiteralControl(this.strHTML));
            }
            else
            {
                container.Controls.Add(this.objControl);
            }
			Logger.LogWarning("<<< Exiting InstantiateIn(container={Container})", container);
        }
    }
}
