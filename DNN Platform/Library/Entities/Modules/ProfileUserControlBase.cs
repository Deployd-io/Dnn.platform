// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;

    /// <summary>The ProfileUserControlBase class defines a custom base class for the profile Control.</summary>
    public class ProfileUserControlBase : UserModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public event EventHandler ProfileUpdated;

        public event EventHandler ProfileUpdateCompleted;

        /// <summary>Raises the <see cref="ProfileUpdateCompleted"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnProfileUpdateCompleted(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnProfileUpdateCompleted(e={E})", e);
            if (this.ProfileUpdateCompleted != null)
            {
                this.ProfileUpdateCompleted(this, e);
            }
			Logger.LogDebug("<<< Exiting OnProfileUpdateCompleted(e={E})", e);
        }

        /// <summary>Raises the <see cref="ProfileUpdated"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnProfileUpdated(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnProfileUpdated(e={E})", e);
            if (this.ProfileUpdated != null)
            {
                this.ProfileUpdated(this, e);
            }
			Logger.LogDebug("<<< Exiting OnProfileUpdated(e={E})", e);
        }
    }
}
