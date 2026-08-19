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
        public ClientOption()
        {
		eventLogger.LogDebug(">>> Entering ClientOption()");
		eventLogger.LogDebug("<<< Exiting ClientOption()");
        }

        public ClientOption(string name, string value)
        {
			eventLogger.LogDebug(">>> Entering ClientOption(name={Name},value={Value})", name, value);
            this.Name = name;
            this.Value = value;
			eventLogger.LogDebug("ClientOption(name={Name},value={Value}): this.Name → {Arg2}", name, value, this.Name);
			eventLogger.LogDebug("<<< Exiting ClientOption(name={Name},value={Value})", name, value);
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}
