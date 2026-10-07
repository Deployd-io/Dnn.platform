// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    public class ModuleInfoModel
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        public string __ModuleId { get; set; } // command link

        public int ModuleId { get; set; }

        public string Title { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        public string __ModuleName { get; set; } // command link

        public string ModuleName { get; set; }

        public string FriendlyName { get; set; }

        public int ModuleDefId { get; set; }

        public int TabModuleId { get; set; }

        public string AddedToPages { get; set; }

        public static ModuleInfoModel FromDnnModuleInfo(DotNetNuke.Entities.Modules.ModuleInfo dnnModule, bool? deleted = null)
        {
			eventLogger.LogError(">>> Entering FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted})", dnnModule, deleted);
            var mim = new ModuleInfoModel
            {
                ModuleId = dnnModule.ModuleID,
                Title = dnnModule.ModuleTitle,
                FriendlyName = dnnModule.DesktopModule.FriendlyName,
                ModuleName = dnnModule.DesktopModule.ModuleName,
                TabModuleId = dnnModule.TabModuleID,
                ModuleDefId = dnnModule.ModuleDefID,
            };

            // assign command links
            mim.__ModuleId = $"get-module {mim.ModuleId}";
			eventLogger.LogDebug("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): mim.__ModuleId → {Arg2}", dnnModule, deleted, mim.__ModuleId);
			eventLogger.LogDebug("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): mim.__ModuleName → {Arg2}", dnnModule, deleted, mim.__ModuleName);
            mim.__ModuleName = $"list-modules --name '{mim.ModuleName}' --all";

            // get a list of all pages this module is added to
            var addedTo = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModulesByModule(mim.ModuleId);
			eventLogger.LogError("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): addedTo → {AddedTo}", dnnModule, deleted, addedTo);
            if (deleted.HasValue)
            {
                addedTo = addedTo.Where(x => x.IsDeleted == deleted.Value).ToList();
            }

            var sbAddedTo = new StringBuilder();
			eventLogger.LogError("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): sbAddedTo → {SbAddedTo}", dnnModule, deleted, sbAddedTo);
            foreach (var modInfo in addedTo)
            {
                if (sbAddedTo.Length > 0)
                {
                    sbAddedTo.Append(", ");
                }

                sbAddedTo.Append(modInfo.TabID);
            }

            mim.AddedToPages = sbAddedTo.ToString();
			eventLogger.LogWarning("<<< Exiting FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted})", dnnModule, deleted);
            return mim;
        }
    }
}
