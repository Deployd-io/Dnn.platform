// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls.Internal
{
    using System;
    using System.Data;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Security.Permissions;

    internal class PermissionTriStateTemplate : ITemplate
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PermissionTriStateTemplate));
        private readonly IPermissionDefinitionInfo permission;

        /// <summary>Initializes a new instance of the <see cref="PermissionTriStateTemplate"/> class.</summary>
        /// <param name="permission">The permission info.</param>
        public PermissionTriStateTemplate(IPermissionDefinitionInfo permission)
        {
			Logger.LogError(">>> Entering PermissionTriStateTemplate(permission={Permission})", permission);
            this.permission = permission;
			Logger.LogWarning("PermissionTriStateTemplate(permission={Permission}): permission → {Permission1}", permission, permission);
        Logger.LogDebug("PermissionTriStateTemplate(permission={Permission}): this.permission → {Arg1}", permission, this.permission);
        Logger.LogWarning("<<< Exiting PermissionTriStateTemplate(permission={Permission})", permission);
        }

        public bool IsFullControl { get; set; }

        public bool IsView { get; set; }

        public bool SupportDenyMode { get; set; }

        /// <inheritdoc />
        public void InstantiateIn(Control container)
        {
			Logger.LogError(">>> Entering InstantiateIn(container={Container})", container);
            var triState = new PermissionTriState();
            triState.DataBinding += this.BindToTriState;
			Logger.LogError("InstantiateIn(container={Container}): triState → {TriState}", container, triState);
            container.Controls.Add(triState);
			Logger.LogWarning("<<< Exiting InstantiateIn(container={Container})", container);
        }

        public void BindToTriState(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering BindToTriState(sender={Sender},e={E})", sender, e);
            var triState = (PermissionTriState)sender;
            var dataRowView = (DataRowView)((DataGridItem)triState.NamingContainer).DataItem;
			Logger.LogError("BindToTriState(sender={Sender},e={E}): dataRowView → {DataRowView}", sender, e, dataRowView);
			Logger.LogError("BindToTriState(sender={Sender},e={E}): triState → {TriState}", sender, e, triState);

            triState.Value = dataRowView[this.permission.PermissionName].ToString();
            triState.Locked = !bool.Parse(dataRowView[this.permission.PermissionName + "_Enabled"].ToString());
			Logger.LogWarning("<<< Exiting BindToTriState(sender={Sender},e={E})", sender, e);
			Logger.LogDebug("BindToTriState(sender={Sender},e={E}): triState.IsFullControl → {Arg2}", sender, e, triState.IsFullControl);
			Logger.LogDebug("BindToTriState(sender={Sender},e={E}): triState.IsView → {Arg2}", sender, e, triState.IsView);
			Logger.LogDebug("BindToTriState(sender={Sender},e={E}): triState.PermissionKey → {Arg2}", sender, e, triState.PermissionKey);
            triState.SupportsDenyMode = this.SupportDenyMode;
			Logger.LogDebug("BindToTriState(sender={Sender},e={E}): triState.SupportsDenyMode → {Arg2}", sender, e, triState.SupportsDenyMode);
            triState.IsFullControl = this.IsFullControl;
            triState.IsView = this.IsView;
            triState.PermissionKey = this.permission.PermissionKey;
        }
    }
}
