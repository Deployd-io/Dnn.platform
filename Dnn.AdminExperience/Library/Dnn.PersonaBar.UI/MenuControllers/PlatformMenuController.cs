// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.MenuControllers
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Application;

    /// <summary>The <see cref="IMenuItemController"/> for the DNN Platform logout action.</summary>
    public class PlatformMenuController : IMenuItemController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PlatformMenuController));
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
            return DotNetNukeContext.Current.Application.SKU == "DNN";
        }

        /// <inheritdoc />
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
        }
    }
}
