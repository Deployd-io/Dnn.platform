// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>Emit a fallback block for a script in the same part of the page.</summary>
    public class DnnJsIncludeFallback : WebControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnJsIncludeFallback));
        /// <summary>Initializes a new instance of the <see cref="DnnJsIncludeFallback"/> class.</summary>
        /// <param name="objectName">The JS object name to verify is defined in global scope.</param>
        /// <param name="fileName">The path to load if the object is not defined.</param>
        public DnnJsIncludeFallback(string objectName, string fileName)
        {
			Logger.LogError(">>> Entering DnnJsIncludeFallback(objectName={ObjectName},fileName={FileName})", objectName, fileName);
            this.ObjectName = objectName;
			Logger.LogDebug("DnnJsIncludeFallback(objectName={ObjectName},fileName={FileName}): this.ObjectName → {Arg2}", objectName, fileName, this.ObjectName);
        Logger.LogWarning("<<< Exiting DnnJsIncludeFallback(objectName={ObjectName},fileName={FileName})", objectName, fileName);
            this.FileName = fileName;
        }

        /// <summary>Gets or sets the object name.</summary>
        public string ObjectName { get; set; }

        /// <summary>Gets or sets the file name.</summary>
        public string FileName { get; set; }

        /// <inheritdoc />
        public override void RenderControl(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderControl(writer={Writer})", writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
            writer.RenderBeginTag(HtmlTextWriterTag.Script);

            if (this.ObjectName.Contains("."))
            {
                // generate function check
                writer.Write($"if (typeof {this.ObjectName} != 'function') {{");
            }
            else
            {
                // generate object check
                writer.Write($"if (typeof {this.ObjectName} == 'undefined') {{");
            }

            writer.Write($"document.write('<script src=\"{this.FileName}\" type=\"text/javascript\"></' + 'script>');");
            writer.Write("}");
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderControl(writer={Writer})", writer);
        }
    }
}
