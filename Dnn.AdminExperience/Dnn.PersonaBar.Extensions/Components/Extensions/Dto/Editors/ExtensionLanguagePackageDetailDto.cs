// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using DotNetNuke.Services.Installer.Packages;
    using Newtonsoft.Json;

    [JsonObject]
    public class ExtensionLanguagePackageDetailDto : CoreLanguagePackageDetailDto
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ExtensionLanguagePackageDetailDto()
        {
		Logger.LogDebug(">>> Entering ExtensionLanguagePackageDetailDto()");
		Logger.LogDebug("<<< Exiting ExtensionLanguagePackageDetailDto()");
        }

        public ExtensionLanguagePackageDetailDto(int portalId, PackageInfo package)
            : base(portalId, package)
        {
        }

        [JsonProperty("dependentPackageId")]
        public int DependentPackageId { get; set; }
    }
}
