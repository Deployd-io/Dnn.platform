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
		eventLogger.LogDebug(">>> Entering ModuleDefinitionDto()");
		eventLogger.LogDebug("<<< Exiting ModuleDefinitionDto()");
        }

        public ModuleDefinitionDto(ModuleDefinitionInfo definition)
        {
			eventLogger.LogDebug(">>> Entering ModuleDefinitionDto(definition={Definition})", definition);
            this.Id = definition.ModuleDefID;
            this.DesktopModuleId = definition.DesktopModuleID;
			eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.Id → {Arg1}", definition, this.Id);
			eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.DesktopModuleId → {Arg1}", definition, this.DesktopModuleId);
            this.Name = definition.DefinitionName;
            this.FriendlyName = definition.FriendlyName;
            this.CacheTime = definition.DefaultCacheTime;
			eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.Name → {Arg1}", definition, this.Name);
			eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): FriendlyName → {FriendlyName}", definition, FriendlyName);
			eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.FriendlyName → {Arg1}", definition, this.FriendlyName);
			eventLogger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.CacheTime → {Arg1}", definition, this.CacheTime);

            foreach (var moduleControlInfo in definition.ModuleControls.Values)
            {
                this.Controls.Add(new ModuleControlDto(moduleControlInfo));
            }
			eventLogger.LogDebug("<<< Exiting ModuleDefinitionDto(definition={Definition})", definition);
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
			eventLogger.LogDebug(">>> Entering ToModuleDefinitionInfo()");
			eventLogger.LogDebug("<<< Exiting ToModuleDefinitionInfo()");
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
