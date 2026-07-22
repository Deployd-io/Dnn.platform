// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.MenuControllers
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Controls the themes menu.</summary>
    public class ThemeMenuController : IMenuItemController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public void UpdateParameters(MenuItem menuItem)
        {
		Logger.LogDebug(">>> Entering UpdateParameters(menuItem={MenuItem})", menuItem);
		Logger.LogDebug("<<< Exiting UpdateParameters(menuItem={MenuItem})", menuItem);
        }

        /// <inheritdoc />
        public bool Visible(MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering Visible(menuItem={MenuItem})", menuItem);
			Logger.LogDebug("<<< Exiting Visible(menuItem={MenuItem})", menuItem);
            return true;
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return new Dictionary<string, object>
            {
                { "previewUrl", Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>().NavigateURL() },
            };
        }
    }
}
