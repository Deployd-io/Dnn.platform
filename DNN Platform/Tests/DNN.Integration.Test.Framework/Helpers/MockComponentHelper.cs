// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Services.Cache;
    using Moq;
	using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class copied from Platform Tests: DotNetNuke.Tests.Utilities.Mocks.MockComponentProvider
    ///
    /// This class helps to mock any components that have been registered using the ComponentFactory.
    /// </summary>
    public static class MockComponentHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static Mock<CachingProvider> CreateDataCacheProvider()
        {
			_logger.LogDebug(">>> Entering CreateDataCacheProvider()");
			_logger.LogDebug("<<< Exiting CreateDataCacheProvider()");
            return CreateNew<CachingProvider>();
        }

        public static Mock<T> CreateNew<T>()
            where T : class
        {
			_logger.LogDebug(">>> Entering CreateNew()");
            if (ComponentFactory.Container == null)
            {
                ComponentFactory.Container = new SimpleContainer();
            }

            // Try and get mock
            var mockComp = ComponentFactory.GetComponent<Mock<T>>();
			_logger.LogDebug("CreateNew(): mockComp → {MockComp}", mockComp);
            var objComp = ComponentFactory.GetComponent<T>();
			_logger.LogDebug("CreateNew(): objComp → {ObjComp}", objComp);

            if (mockComp == null)
            {
                mockComp = new Mock<T>();
                ComponentFactory.RegisterComponentInstance<Mock<T>>(mockComp);
            }

            if (objComp == null)
            {
                ComponentFactory.RegisterComponentInstance<T>(mockComp.Object);
            }

			_logger.LogDebug("<<< Exiting CreateNew()");
            return mockComp;
        }
    }
}
