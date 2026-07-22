// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    public class ClientString : ClientOption
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ClientString()
        {
		Logger.LogDebug(">>> Entering ClientString()");
		Logger.LogDebug("<<< Exiting ClientString()");
        }

        public ClientString(string name, string value)
        {
			Logger.LogDebug(">>> Entering ClientString(name={Name},value={Value})", name, value);
            this.Name = name;
            this.Value = value;
			Logger.LogDebug("<<< Exiting ClientString(name={Name},value={Value})", name, value);
			Logger.LogDebug("ClientString(name={Name},value={Value}): this.Name → {Arg2}", name, value, this.Name);
        }
    }
}
