// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization.Internal
{
    using System;

    using DotNetNuke.Framework;

    public class TestableLocalization : ServiceLocator<ILocalization, TestableLocalization>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TestableLocalization));
        /// <inheritdoc />
        protected override Func<ILocalization> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new LocalizationImpl();
        }
    }
}
