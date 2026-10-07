// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Dto.Tabs
{
    using Newtonsoft.Json;

    [JsonObject]
    public class LocaleInfoDto
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocaleInfoDto));
        public LocaleInfoDto(string cultureCode)
        {
			Logger.LogError(">>> Entering LocaleInfoDto(cultureCode={CultureCode})", cultureCode);
            this.CultureCode = cultureCode;
			Logger.LogDebug("LocaleInfoDto(cultureCode={CultureCode}): this.CultureCode → {Arg1}", cultureCode, this.CultureCode);
            this.Icon = string.IsNullOrEmpty(cultureCode)
                ? "/images/Flags/none.gif"
                : $"/images/Flags/{cultureCode}.gif";
        }

        public string CultureCode { get; }

        public string Icon { get; }
    }
}
