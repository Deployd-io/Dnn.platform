// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules.Definitions;
    using Newtonsoft.Json;

    [JsonObject]
    public class ModuleDefinitionDto
    {
        public ModuleDefinitionDto()
        {
        }

        public ModuleDefinitionDto(ModuleDefinitionInfo definition)
        {
			eventLogger.LogError(">>> Entering ModuleDefinitionDto(definition={Definition})", definition);
            this.Id = definition.ModuleDefID;
			eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.Id → {Arg1}", definition, this.Id);
            this.DesktopModuleId = definition.DesktopModuleID;
            this.Name = definition.DefinitionName;
			eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.DesktopModuleId → {Arg1}", definition, this.DesktopModuleId);
			eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.Name → {Arg1}", definition, this.Name);
            this.FriendlyName = definition.FriendlyName;
            this.CacheTime = definition.DefaultCacheTime;

            foreach (var moduleControlInfo in definition.ModuleControls.Values)
            {
				eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): FriendlyName → {FriendlyName}", definition, FriendlyName);
				eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.FriendlyName → {Arg1}", definition, this.FriendlyName);
                this.Controls.Add(new ModuleControlDto(moduleControlInfo));
            }
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("desktopModuleId")]
        public int DesktopModuleId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("cacheTime")]
        public int CacheTime { get; set; }

        [JsonProperty("controls")]
        public IList<ModuleControlDto> Controls { get; set; } = new List<ModuleControlDto>();

        public ModuleDefinitionInfo ToModuleDefinitionInfo()
        {
			eventLogger.LogError(">>> Entering ToModuleDefinitionInfo()");
			eventLogger.LogWarning("<<< Exiting ToModuleDefinitionInfo()");
            return new ModuleDefinitionInfo
            {
                ModuleDefID = this.Id,
                DesktopModuleID = this.DesktopModuleId,
                DefinitionName = this.Name,
                FriendlyName = this.FriendlyName,
                DefaultCacheTime = this.CacheTime,
            };
        }
    }
}
