// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Internal
{
    using System;

    using DotNetNuke.Framework;

    /// <summary>Business Layer to manage Messaging. Also contains CRUD methods.</summary>
    public class InternalMessagingController : ServiceLocator<IInternalMessagingController, InternalMessagingController>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InternalMessagingController));
        /// <inheritdoc />
        protected override Func<IInternalMessagingController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new InternalMessagingControllerImpl();
        }
    }
}
