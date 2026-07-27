// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System.Xml.Serialization;

    [XmlInclude(typeof(ClientBoolean))]
    [XmlInclude(typeof(ClientNumber))]
    [XmlInclude(typeof(ClientString))]
    public class ClientOption
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ClientOption()
        {
		Logger.LogDebug(">>> Entering ClientOption()");
		Logger.LogDebug("<<< Exiting ClientOption()");
        }

        public ClientOption(string name, string value)
        {
			Logger.LogDebug(">>> Entering ClientOption(name={Name},value={Value})", name, value);
            this.Name = name;
            this.Value = value;
			Logger.LogDebug("<<< Exiting ClientOption(name={Name},value={Value})", name, value);
			Logger.LogDebug("ClientOption(name={Name},value={Value}): this.Name → {Arg2}", name, value, this.Name);
			Logger.LogDebug("ClientOption(name={Name},value={Value}): this.Value → {Arg2}", name, value, this.Value);
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
