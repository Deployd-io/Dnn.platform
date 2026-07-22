// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A literal control.</summary>
    public class DnnFormLiteralItem : DnnFormItemBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="DnnFormLiteralItem"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnFormLiteralItem()
            : this(Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>())
        {
		Logger.LogDebug(">>> Entering DnnFormLiteralItem()");
        }
		Logger.LogDebug("<<< Exiting DnnFormLiteralItem()");

        /// <summary>Initializes a new instance of the <see cref="DnnFormLiteralItem"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public DnnFormLiteralItem(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
            : base(appStatus, eventLogger)
        {
            this.ViewStateMode = ViewStateMode.Disabled;
			Logger.LogDebug("DnnFormLiteralItem(appStatus={AppStatus},eventLogger={EventLogger}): this.ViewStateMode → {Arg2}", appStatus, eventLogger, this.ViewStateMode);
        }

        /// <inheritdoc />
        protected override WebControl CreateControlInternal(Control container)
        {
			Logger.LogDebug(">>> Entering CreateControlInternal(container={Container})", container);
            var literal = new Label { ID = this.ID + "_Label", Text = Convert.ToString(this.Value, CultureInfo.InvariantCulture), };
            container.Controls.Add(literal);
			Logger.LogDebug("CreateControlInternal(container={Container}): literal → {Literal}", container, literal);
			Logger.LogDebug("<<< Exiting CreateControlInternal(container={Container})", container);
            return literal;
        }
    }
}
