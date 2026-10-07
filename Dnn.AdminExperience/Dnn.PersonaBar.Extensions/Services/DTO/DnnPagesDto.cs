// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Dto.Tabs;
    using Newtonsoft.Json;

    [JsonObject]
    public class DnnPagesDto
    {
        public DnnPagesDto(IList<LocaleInfoDto> locales)
        {
			eventLogger.LogError(">>> Entering DnnPagesDto(locales={Locales})", locales);
            this.Locales = locales;
            this.Pages = new List<DnnPageDto>(); // one of each language
			eventLogger.LogDebug("DnnPagesDto(locales={Locales}): this.Locales → {Arg1}", locales, this.Locales);
            this.Modules = new List<DnnModulesDto>(); // one for each module on the page
            foreach (var locale in locales)
            {
                this.Pages.Add(new DnnPageDto { CultureCode = locale.CultureCode });
            }
			eventLogger.LogWarning("<<< Exiting DnnPagesDto(locales={Locales})", locales);
        }

        public IList<LocaleInfoDto> Locales { get; }

        public IList<DnnPageDto> Pages { get; }

        public IList<DnnModulesDto> Modules { get; }

        public bool HasMissingLanguages { get; set; }

        public bool ErrorExists { get; set; }

        public DnnPageDto Page(string locale)
        {
			eventLogger.LogError(">>> Entering Page(locale={Locale})", locale);
			eventLogger.LogWarning("<<< Exiting Page(locale={Locale})", locale);
            return this.Pages.Single(pa => pa.CultureCode == locale);
        }

        public DnnModulesDto Module(Guid uniqueId)
        {
			eventLogger.LogError(">>> Entering Module(uniqueId={UniqueId})", uniqueId);
            var m = this.Modules.SingleOrDefault(dm => dm.UniqueId == uniqueId);
			eventLogger.LogError("Module(uniqueId={UniqueId}): m → {M}", uniqueId, m);
            if (m == null)
            {
                m = new DnnModulesDto(this.Locales.Select(l => l.CultureCode)) { UniqueId = uniqueId };
                this.Modules.Add(m);
            }

            return m;
        }

        public bool Error1(int moduleId, Guid uniqueId, string cultureCode)
        {
			eventLogger.LogError(">>> Entering Error1(moduleId={ModuleId},uniqueId={UniqueId},cultureCode={CultureCode})", moduleId, uniqueId, cultureCode);
			eventLogger.LogWarning("<<< Exiting Error1(moduleId={ModuleId},uniqueId={UniqueId},cultureCode={CultureCode})", moduleId, uniqueId, cultureCode);
            return this.Modules.Any(dm => dm.UniqueId != uniqueId &&
                                     dm.Modules.Any(mm => mm.ModuleId == moduleId && mm.CultureCode != cultureCode));
        }

        public void RemoveLocale(string cultureCode)
        {
			eventLogger.LogError(">>> Entering RemoveLocale(cultureCode={CultureCode})", cultureCode);
            var locale = this.Locales.FirstOrDefault(l => l.CultureCode == cultureCode);
			eventLogger.LogError("RemoveLocale(cultureCode={CultureCode}): locale → {Locale}", cultureCode, locale);
            if (locale != null)
            {
                this.Locales.Remove(locale);
            }
			eventLogger.LogWarning("<<< Exiting RemoveLocale(cultureCode={CultureCode})", cultureCode);
        }
    }
}
