// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Dependencies
{
    using System.Xml.XPath;

    using DotNetNuke.Framework;

    /// <summary>The TypeDependency determines whether the dependent type is installed.</summary>
    public class TypeDependency : DependencyBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private string missingDependentType = string.Empty;
        private string dependentTypes;

        /// <inheritdoc />
        public override string ErrorMessage
        {
            get
            {
                return Util.INSTALL_Namespace + " - " + this.missingDependentType;
            }
        }

        /// <inheritdoc />
        public override bool IsValid
        {
            get
            {
                bool isValid = true;
                if (!string.IsNullOrEmpty(this.dependentTypes))
                {
                    foreach (string dependentType in (this.dependentTypes + ";").Split(';'))
                    {
                        if (!string.IsNullOrEmpty(dependentType.Trim()))
                        {
                            if (Reflection.CreateType(dependentType, true) == null)
                            {
                                this.missingDependentType = dependentType;
                                isValid = false;
                                break;
                            }
                        }
                    }
                }

                return isValid;
            }
        }

        /// <inheritdoc />
        public override void ReadManifest(XPathNavigator dependencyNav)
        {
			Logger.LogDebug(">>> Entering ReadManifest(dependencyNav={DependencyNav})", dependencyNav);
            this.dependentTypes = dependencyNav.Value;
			Logger.LogDebug("<<< Exiting ReadManifest(dependencyNav={DependencyNav})", dependencyNav);
			Logger.LogDebug("ReadManifest(dependencyNav={DependencyNav}): this.dependentTypes → {Arg1}", dependencyNav, this.dependentTypes);
        }
    }
}
