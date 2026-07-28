// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.UI.Modules;

    /// <summary>The PackageEditorBase class provides a Base Classs for Package Editors.</summary>
    public class PackageEditorBase : ModuleUserControlBase, IPackageEditor
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private bool isWizard = Null.NullBoolean;
        private PackageInfo package;
        private int packageID = Null.NullInteger;

        /// <summary>Gets or sets the Package ID.</summary>
        /// <value>An Integer.</value>
        public int PackageID
        {
            get
            {
                return this.packageID;
            }

            set
            {
                this.packageID = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the Editor is in the Wizard.</summary>
        /// <value>An Boolean.</value>
        public bool IsWizard
        {
            get
            {
                return this.isWizard;
            }

            set
            {
                this.isWizard = value;
            }
        }

        protected string DisplayMode => (this.Request.QueryString["Display"] ?? string.Empty).ToLowerInvariant();

        protected virtual string EditorID
        {
            get
            {
                return Null.NullString;
            }
        }

        protected bool IsSuperTab
        {
            get
            {
                return this.ModuleContext.PortalSettings.ActiveTab.IsSuperTab;
            }
        }

        protected PackageInfo Package
        {
            get
            {
                if (this.package == null)
                {
                    this.package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, (p) => p.PackageID == this.PackageID);
                }

                return this.package;
            }
        }

        /// <summary>Used to Initialize the Control.</summary>
        public virtual void Initialize()
        {
		Logger.LogDebug(">>> Entering Initialize()");
		Logger.LogDebug("<<< Exiting Initialize()");
        }

        /// <summary>Used to Update the Package.</summary>
        public virtual void UpdatePackage()
        {
		Logger.LogDebug(">>> Entering UpdatePackage()");
		Logger.LogDebug("<<< Exiting UpdatePackage()");
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            this.ID = this.EditorID;
            base.OnInit(e);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
			Logger.LogDebug("OnInit(e={E}): this.ID → {Arg1}", e, this.ID);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }
    }
}
