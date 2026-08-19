// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Controllers
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Business Layer to manage Search.</summary>
    [DnnDeprecated(10, 0, 0, "Please resolve ISearchController via Dependency Injection")]
    public partial class SearchController : ServiceLocator<ISearchController, SearchController>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchController));
        /// <inheritdoc />
        protected override Func<ISearchController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<ISearchController>;
        }
    }
}
