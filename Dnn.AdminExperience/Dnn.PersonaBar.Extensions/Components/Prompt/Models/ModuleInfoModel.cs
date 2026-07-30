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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted})", dnnModule, deleted);
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
			Logger.LogDebug("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): mim.__ModuleId → {Arg2}", dnnModule, deleted, mim.__ModuleId);
			Logger.LogDebug("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): mim.__ModuleName → {Arg2}", dnnModule, deleted, mim.__ModuleName);
            mim.__ModuleName = $"list-modules --name '{mim.ModuleName}' --all";

            // get a list of all pages this module is added to
            var addedTo = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModulesByModule(mim.ModuleId);
            if (deleted.HasValue)
			Logger.LogDebug("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): addedTo → {AddedTo}", dnnModule, deleted, addedTo);
            {
                addedTo = addedTo.Where(x => x.IsDeleted == deleted.Value).ToList();
            }

            var sbAddedTo = new StringBuilder();
            foreach (var modInfo in addedTo)
            {
			Logger.LogDebug("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): sbAddedTo → {SbAddedTo}", dnnModule, deleted, sbAddedTo);
                if (sbAddedTo.Length > 0)
                {
                    sbAddedTo.Append(", ");
                }

                sbAddedTo.Append(modInfo.TabID);
            }

            mim.AddedToPages = sbAddedTo.ToString();
			Logger.LogDebug("<<< Exiting FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted})", dnnModule, deleted);
            return mim;
        }
    }
}
