// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    public class TemplateArgument
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="TemplateArgument"/> class.</summary>
        public TemplateArgument()
        {
		Logger.LogDebug(">>> Entering TemplateArgument()");
		Logger.LogDebug("<<< Exiting TemplateArgument()");
        }

        /// <summary>Initializes a new instance of the <see cref="TemplateArgument"/> class.</summary>
        /// <param name="name">The template argument name.</param>
        /// <param name="value">The template argument value.</param>
        public TemplateArgument(string name, string value)
        {
			Logger.LogDebug(">>> Entering TemplateArgument(name={Name},value={Value})", name, value);
            this.Name = name;
            this.Value = value;
			Logger.LogDebug("<<< Exiting TemplateArgument(name={Name},value={Value})", name, value);
			Logger.LogDebug("TemplateArgument(name={Name},value={Value}): this.Name → {Arg2}", name, value, this.Name);
			Logger.LogDebug("TemplateArgument(name={Name},value={Value}): this.Value → {Arg2}", name, value, this.Value);
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
