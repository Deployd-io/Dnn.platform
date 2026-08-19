// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Razor
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;

    /// <summary>A razor web page model.</summary>
    /// <typeparam name="TModel">The model type.</typeparam>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public abstract partial class DotNetNukeWebPage<TModel> : DotNetNukeWebPage
        where TModel : class
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private TModel model;

        public DotNetNukeWebPage()
        {
			_logger.LogDebug(">>> Entering DotNetNukeWebPage()");
            var model = Globals.GetCurrentServiceProvider().GetService<TModel>();
            this.Model = model ?? Activator.CreateInstance<TModel>();
			_logger.LogDebug("DotNetNukeWebPage(): model → {Model}", model);
			_logger.LogDebug("<<< Exiting DotNetNukeWebPage()");
        }

        public new TModel Model
        {
            get { return this.PageContext?.Model as TModel ?? this.model; }
            set { this.model = value; }
        }
    }
}
