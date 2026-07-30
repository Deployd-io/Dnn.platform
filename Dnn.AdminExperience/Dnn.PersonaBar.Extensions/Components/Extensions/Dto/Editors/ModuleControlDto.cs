// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using Newtonsoft.Json;

    [JsonObject]
    public class ModuleControlDto
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ModuleControlDto()
        {
		Logger.LogDebug(">>> Entering ModuleControlDto()");
        }

        public ModuleControlDto(ModuleControlInfo moduleControl)
        {
			Logger.LogDebug(">>> Entering ModuleControlDto(moduleControl={ModuleControl})", moduleControl);
            this.Id = moduleControl.ModuleControlID;
            this.DefinitionId = moduleControl.ModuleDefID;
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.Id → {Arg1}", moduleControl, this.Id);
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.DefinitionId → {Arg1}", moduleControl, this.DefinitionId);
            this.Key = moduleControl.ControlKey;
            this.Title = moduleControl.ControlTitle;
            this.Source = moduleControl.ControlSrc;
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.Key → {Arg1}", moduleControl, this.Key);
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.Title → {Arg1}", moduleControl, this.Title);
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.Source → {Arg1}", moduleControl, this.Source);
            this.Type = moduleControl.ControlType;
            this.Order = moduleControl.ViewOrder;
            this.Icon = moduleControl.IconFile;
            this.HelpUrl = moduleControl.HelpURL;
            this.SupportPopups = moduleControl.SupportsPopUps;
            this.SupportPartialRendering = moduleControl.SupportsPartialRendering;
			Logger.LogDebug("<<< Exiting ModuleControlDto(moduleControl={ModuleControl})", moduleControl);
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.Type → {Arg1}", moduleControl, this.Type);
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.Order → {Arg1}", moduleControl, this.Order);
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.Icon → {Arg1}", moduleControl, this.Icon);
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.HelpUrl → {Arg1}", moduleControl, this.HelpUrl);
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.SupportPopups → {Arg1}", moduleControl, this.SupportPopups);
			Logger.LogDebug("ModuleControlDto(moduleControl={ModuleControl}): this.SupportPartialRendering → {Arg1}", moduleControl, this.SupportPartialRendering);
			Logger.LogDebug("<<< Exiting ModuleControlDto()");
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("definitionId")]
        public int DefinitionId { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("type")]
        public SecurityAccessLevel Type { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("helpUrl")]
        public string HelpUrl { get; set; }

        [JsonProperty("supportPopups")]
        public bool SupportPopups { get; set; }

        [JsonProperty("supportPartialRendering")]
        public bool SupportPartialRendering { get; set; }

        public ModuleControlInfo ToModuleControlInfo()
        {
			Logger.LogDebug(">>> Entering ToModuleControlInfo()");
			Logger.LogDebug("<<< Exiting ToModuleControlInfo()");
            return new ModuleControlInfo
            {
                ModuleControlID = this.Id,
                ModuleDefID = this.DefinitionId,
                ControlKey = this.Key,
                ControlTitle = this.Title,
                ControlSrc = this.Source,
                ControlType = this.Type,
                ViewOrder = this.Order,
                IconFile = this.Icon,
                HelpURL = this.HelpUrl,
                SupportsPartialRendering = this.SupportPartialRendering,
                SupportsPopUps = this.SupportPopups,
            };
        }
    }
}
