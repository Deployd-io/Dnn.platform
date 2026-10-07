// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    public class ClientNumber : ClientOption
    {
        public ClientNumber()
        {
        }

        public ClientNumber(string name, string value)
        {
			eventLogger.LogError(">>> Entering ClientNumber(name={Name},value={Value})", name, value);
            this.Name = name;
			eventLogger.LogWarning("<<< Exiting ClientNumber(name={Name},value={Value})", name, value);
			eventLogger.LogDebug("ClientNumber(name={Name},value={Value}): this.Name → {Arg2}", name, value, this.Name);
            this.Value = value;
        }
    }
}
