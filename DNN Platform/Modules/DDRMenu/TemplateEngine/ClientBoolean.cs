// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    public class ClientBoolean : ClientOption
    {
        public ClientBoolean()
        {
        }

        public ClientBoolean(string name, string value)
        {
			eventLogger.LogError(">>> Entering ClientBoolean(name={Name},value={Value})", name, value);
            this.Name = name;
			eventLogger.LogWarning("<<< Exiting ClientBoolean(name={Name},value={Value})", name, value);
			eventLogger.LogDebug("ClientBoolean(name={Name},value={Value}): this.Name → {Arg2}", name, value, this.Name);
            this.Value = value;
        }
    }
}
