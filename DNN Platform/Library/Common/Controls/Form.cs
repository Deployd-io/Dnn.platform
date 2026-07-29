// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Controls
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    /// <summary>The Form will reset action to raw url instead of rewrite url.</summary>
    public class Form : HtmlForm
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        protected override void RenderAttributes(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderAttributes(writer={Writer})", writer);
            var stringWriter = new StringWriter();
            var htmlWriter = new HtmlTextWriter(stringWriter);
			Logger.LogDebug("RenderAttributes(writer={Writer}): stringWriter → {StringWriter}", writer, stringWriter);
			Logger.LogDebug("RenderAttributes(writer={Writer}): htmlWriter → {HtmlWriter}", writer, htmlWriter);
            base.RenderAttributes(htmlWriter);
            string html = stringWriter.ToString();

			Logger.LogDebug("RenderAttributes(writer={Writer}): html → {Html}", writer, html);
            // Locate and replace action attribute
            int startPoint = html.IndexOf("action=\"", StringComparison.OrdinalIgnoreCase);
            if (startPoint >= 0)
			Logger.LogDebug("RenderAttributes(writer={Writer}): startPoint → {StartPoint}", writer, startPoint);
            {
                int endPoint = html.IndexOf("\"", startPoint + 8, StringComparison.Ordinal) + 1;
                html = html.Remove(startPoint, endPoint - startPoint);
				Logger.LogDebug("RenderAttributes(writer={Writer}): endPoint → {EndPoint}", writer, endPoint);
                html = html.Insert(startPoint, "action=\"" + HttpUtility.HtmlEncode(HttpContext.Current.Request.RawUrl) + "\"");
            }

            if (this.ID != null)
            {
                // Locate and replace id attribute
                startPoint = html.IndexOf("id=\"", StringComparison.OrdinalIgnoreCase);
                if (startPoint >= 0)
                {
                    int endPoint = html.IndexOf("\"", startPoint + 4, StringComparison.Ordinal) + 1;
                    html = html.Remove(startPoint, endPoint - startPoint);
                    html = html.Insert(startPoint, "id=\"" + this.ClientID + "\"");
                }
            }

            writer.Write(html);
			Logger.LogDebug("<<< Exiting RenderAttributes(writer={Writer})", writer);
        }
    }
}
