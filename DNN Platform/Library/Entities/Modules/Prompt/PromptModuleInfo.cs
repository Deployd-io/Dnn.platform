// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Prompt
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    public class PromptModuleInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PromptModuleInfo));
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
#pragma warning disable CS3008 // Identifier beginning with an underscore is not CLS-compliant
        public string __ModuleId { get; set; } // command link
#pragma warning restore CS3008 // Identifier beginning with an underscore is not CLS-compliant

        public int ModuleId { get; set; }

        public string Title { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
#pragma warning disable CS3008 // Identifier beginning with an underscore is not CLS-compliant
        public string __ModuleName { get; set; } // command link
#pragma warning restore CS3008 // Identifier beginning with an underscore is not CLS-compliant

        public string ModuleName { get; set; }

        public string FriendlyName { get; set; }

        public int ModuleDefId { get; set; }

        public int TabModuleId { get; set; }

        public string AddedToPages { get; set; }

        public static PromptModuleInfo FromDnnModuleInfo(ModuleInfo dnnModule, bool? deleted = null)
        {
			Logger.LogError(">>> Entering FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted})", dnnModule, deleted);
            var mim = new PromptModuleInfo
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
            mim.__ModuleName = $"list-modules --name '{mim.ModuleName}' --all";
			Logger.LogDebug("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): mim.__ModuleName → {Arg2}", dnnModule, deleted, mim.__ModuleName);

            // get a list of all pages this module is added to
            var addedTo = ModuleController.Instance.GetTabModulesByModule(mim.ModuleId);
			Logger.LogError("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): addedTo → {AddedTo}", dnnModule, deleted, addedTo);
            if (deleted.HasValue)
            {
                addedTo = addedTo.Where(x => x.IsDeleted == deleted.Value).ToList();
            }

            var sbAddedTo = new StringBuilder();
			Logger.LogError("FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted}): sbAddedTo → {SbAddedTo}", dnnModule, deleted, sbAddedTo);
            foreach (var modInfo in addedTo)
            {
                if (sbAddedTo.Length > 0)
                {
                    sbAddedTo.Append(", ");
                }

                sbAddedTo.Append(modInfo.TabID);
            }

            mim.AddedToPages = sbAddedTo.ToString();
			Logger.LogWarning("<<< Exiting FromDnnModuleInfo(dnnModule={DnnModule},deleted={Deleted})", dnnModule, deleted);
            return mim;
        }
    }
}
