// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.NavigationProvider
{
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.UI.WebControls;

    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class NavigationEventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NavigationEventArgs));
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ID;

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public DNNNode Node;

        /// <summary>Initializes a new instance of the <see cref="NavigationEventArgs"/> class.</summary>
        /// <param name="strID">The ID.</param>
        /// <param name="objNode">The node.</param>
        public NavigationEventArgs(string strID, DNNNode objNode)
        {
			Logger.LogError(">>> Entering NavigationEventArgs(strID={StrID},objNode={ObjNode})", strID, objNode);
			Logger.LogDebug("NavigationEventArgs(strID={StrID},objNode={ObjNode}): Node → {Node}", strID, objNode, Node);
            this.ID = strID;
            this.Node = objNode;
			Logger.LogDebug("NavigationEventArgs(strID={StrID},objNode={ObjNode}): this.ID → {Arg2}", strID, objNode, this.ID);
        Logger.LogWarning("<<< Exiting NavigationEventArgs(strID={StrID},objNode={ObjNode})", strID, objNode);
        }
    }
}
