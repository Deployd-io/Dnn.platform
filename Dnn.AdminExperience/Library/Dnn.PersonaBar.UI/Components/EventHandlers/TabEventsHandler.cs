// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components.EventHandlers
{
    using System.ComponentModel.Composition;

    using Dnn.PersonaBar.UI.Components.Controllers;
    using DotNetNuke.Entities.Tabs.Actions;

    /// <summary>The default <see cref="ITabEventHandler"/> implementation.</summary>
    [Export(typeof(ITabEventHandler))]
    public class TabEventsHandler : ITabEventHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabEventsHandler));
        /// <inheritdoc />
        public void TabCreated(object sender, TabEventArgs args)
        {
			Logger.LogError(">>> Entering TabCreated(sender={Sender},args={Args})", sender, args);
            AdminMenuController.Instance.CreateLinkMenu(args.Tab);
			Logger.LogWarning("<<< Exiting TabCreated(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void TabUpdated(object sender, TabEventArgs args)
        {
			Logger.LogError(">>> Entering TabUpdated(sender={Sender},args={Args})", sender, args);
            AdminMenuController.Instance.CreateLinkMenu(args.Tab);
			Logger.LogWarning("<<< Exiting TabUpdated(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void TabRemoved(object sender, TabEventArgs args)
        {
		Logger.LogError(">>> Entering TabRemoved(sender={Sender},args={Args})", sender, args);
        Logger.LogWarning("<<< Exiting TabRemoved(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void TabDeleted(object sender, TabEventArgs args)
        {
			Logger.LogError(">>> Entering TabDeleted(sender={Sender},args={Args})", sender, args);
            AdminMenuController.Instance.DeleteLinkMenu(args.Tab);
			Logger.LogWarning("<<< Exiting TabDeleted(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void TabRestored(object sender, TabEventArgs args)
        {
			Logger.LogError(">>> Entering TabRestored(sender={Sender},args={Args})", sender, args);
            AdminMenuController.Instance.CreateLinkMenu(args.Tab);
			Logger.LogWarning("<<< Exiting TabRestored(sender={Sender},args={Args})", sender, args);
        }

        /// <inheritdoc />
        public void TabMarkedAsPublished(object sender, TabEventArgs args)
        {
		Logger.LogError(">>> Entering TabMarkedAsPublished(sender={Sender},args={Args})", sender, args);
        Logger.LogWarning("<<< Exiting TabMarkedAsPublished(sender={Sender},args={Args})", sender, args);
        }
    }
}
