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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ModuleDefinitionDto()
        {
		Logger.LogDebug(">>> Entering ModuleDefinitionDto()");
        }

        public ModuleDefinitionDto(ModuleDefinitionInfo definition)
        {
			Logger.LogDebug(">>> Entering ModuleDefinitionDto(definition={Definition})", definition);
            this.Id = definition.ModuleDefID;
            this.DesktopModuleId = definition.DesktopModuleID;
			Logger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.Id → {Arg1}", definition, this.Id);
			Logger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.DesktopModuleId → {Arg1}", definition, this.DesktopModuleId);
            this.Name = definition.DefinitionName;
            this.FriendlyName = definition.FriendlyName;
            this.CacheTime = definition.DefaultCacheTime;
			Logger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.Name → {Arg1}", definition, this.Name);
			Logger.LogDebug("ModuleDefinitionDto(definition={Definition}): FriendlyName → {FriendlyName}", definition, FriendlyName);
			Logger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.FriendlyName → {Arg1}", definition, this.FriendlyName);
			Logger.LogDebug("ModuleDefinitionDto(definition={Definition}): this.CacheTime → {Arg1}", definition, this.CacheTime);

            foreach (var moduleControlInfo in definition.ModuleControls.Values)
            {
                this.Controls.Add(new ModuleControlDto(moduleControlInfo));
            }
			Logger.LogDebug("<<< Exiting ModuleDefinitionDto(definition={Definition})", definition);
			Logger.LogDebug("<<< Exiting ModuleDefinitionDto()");
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
			Logger.LogDebug(">>> Entering ToModuleDefinitionInfo()");
			Logger.LogDebug("<<< Exiting ToModuleDefinitionInfo()");
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
