// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Linq;

    using DotNetNuke.Entities.Modules.Settings;

    public class ComplexTypeSerializer : ISettingsSerializer<ComplexType>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public string Serialize(ComplexType value)
        {
			Logger.LogDebug(">>> Entering Serialize(value={Value})", value);
			Logger.LogDebug("<<< Exiting Serialize(value={Value})", value);
            return $"{value.X} | {value.Y}";
        }

        public ComplexType Deserialize(string value)
        {
			Logger.LogDebug(">>> Entering Deserialize(value={Value})", value);
            if (string.IsNullOrEmpty(value))
            {
                return new ComplexType(5, 10);
            }

            var values = value.Split(new[] { '|', }, StringSplitOptions.RemoveEmptyEntries).Select(val => val.Trim()).Select(int.Parse).ToArray();
			Logger.LogDebug("Deserialize(value={Value}): values → {Values}", value, values);
			Logger.LogDebug("<<< Exiting Deserialize(value={Value})", value);
            return new ComplexType(values[0], values[1]);
        }
    }
}
