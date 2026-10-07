// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Prompt.Models
{
    using System.Diagnostics.CodeAnalysis;

    public class PageModelBase
    {
        /// <summary>Initializes a new instance of the <see cref="PageModelBase"/> class.</summary>
        public PageModelBase()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PageModelBase"/> class.</summary>
        /// <param name="tab">The tab information.</param>
        public PageModelBase(DotNetNuke.Entities.Tabs.TabInfo tab)
        {
			eventLogger.LogError(">>> Entering PageModelBase(tab={Tab})", tab);
            this.Name = tab.TabName;
			eventLogger.LogWarning("<<< Exiting PageModelBase(tab={Tab})", tab);
			eventLogger.LogDebug("PageModelBase(tab={Tab}): this.Name → {Arg1}", tab, this.Name);
            this.ParentId = tab.ParentId;
            this.Path = tab.TabPath;
			eventLogger.LogDebug("PageModelBase(tab={Tab}): this.ParentId → {Arg1}", tab, this.ParentId);
			eventLogger.LogDebug("PageModelBase(tab={Tab}): this.Path → {Arg1}", tab, this.Path);
            this.TabId = tab.TabID;
            this.Skin = tab.SkinSrc;
            this.Title = tab.Title;
            this.IncludeInMenu = tab.IsVisible;
			eventLogger.LogDebug("PageModelBase(tab={Tab}): this.TabId → {Arg1}", tab, this.TabId);
            this.IsDeleted = tab.IsDeleted;
        }

        // ReSharper disable InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public string __TabId => $"get-page {this.TabId}";

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public string __ParentId => $"list-pages --parentid {this.ParentId}";

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public string __IncludeInMenu => $"list-pages --visible{(this.IncludeInMenu ? string.Empty : " false")}";

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public string __IsDeleted => $"list-pages --deleted{(this.IsDeleted ? string.Empty : " false")}";

        // ReSharper restore InconsistentNaming
        public int TabId { get; set; }

        public string Name { get; set; }

        public string Title { get; set; }

        public int ParentId { get; set; }

        public string Skin { get; set; }

        public string Path { get; set; }

        public bool IncludeInMenu { get; set; }

        public bool IsDeleted { get; set; }
    }
}
