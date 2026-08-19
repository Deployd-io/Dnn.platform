// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Controls
{
    using System;

    using DotNetNuke.Entities.Modules;

    /// <summary>A control which displays a message.</summary>
    public partial class Message : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Message.ascx));
        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.InitializeComponent();
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        private void InitializeComponent()
        {
			Logger.LogDebug(">>> Entering InitializeComponent()");
            this.ID = "Message";
			Logger.LogDebug("InitializeComponent(): this.ID → {Arg0}", this.ID);
			Logger.LogDebug("<<< Exiting InitializeComponent()");
        }
    }
}
