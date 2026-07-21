// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.EventListeners
{
    using System;

    /// <summary>SkinEventArgs provides a custom EventARgs class for Skin Events.</summary>
    public class SkinEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly Skin skin;

        /// <summary>Initializes a new instance of the <see cref="SkinEventArgs"/> class.</summary>
        /// <param name="skin">The skin.</param>
        public SkinEventArgs(Skin skin)
        {
			Logger.LogDebug(">>> Entering SkinEventArgs(skin={Skin})", skin);
            this.skin = skin;
			Logger.LogDebug("<<< Exiting SkinEventArgs(skin={Skin})", skin);
			Logger.LogDebug("SkinEventArgs(skin={Skin}): skin → {Skin1}", skin, skin);
			Logger.LogDebug("SkinEventArgs(skin={Skin}): this.skin → {Arg1}", skin, this.skin);
        }

        public Skin Skin
        {
            get
            {
                return this.skin;
            }
        }
    }
}
