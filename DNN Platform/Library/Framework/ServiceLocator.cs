// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides a readily testable way to manage a Singleton.</summary>
    /// <typeparam name="TContract">The interface that the controller provides.</typeparam>
    /// <typeparam name="TSelf">The type of the controller itself, used to call the GetFactory override.</typeparam>
    public abstract class ServiceLocator<TContract, TSelf>
        where TSelf : ServiceLocator<TContract, TSelf>, new()
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServiceLocator));
        private static Lazy<TContract> instance = new Lazy<TContract>(InitInstance, true);
        private static TContract testableInstance;
        private static bool useTestable;

        /// <summary>Gets a singleton of T.</summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Breaking change")]
        public static TContract Instance
        {
            get
            {
                if (useTestable)
                {
                    return testableInstance;
                }

                return instance.Value;
            }
        }

        /// <summary>Gets or sets the service locator factory.</summary>
        protected static Func<TContract> Factory { get; set; }

        /// <summary>Registers an instance to use for the Singleton.</summary>
        /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
        /// <param name="instance">The instance to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Breaking change")]
        public static void SetTestableInstance(TContract instance)
        {
			Logger.LogError(">>> Entering SetTestableInstance(instance={Instance})", instance);
            testableInstance = instance;
            useTestable = true;
			Logger.LogDebug("SetTestableInstance(instance={Instance}): testableInstance → {TestableInstance}", instance, testableInstance);
        Logger.LogWarning("<<< Exiting SetTestableInstance(instance={Instance})", instance);
        }

        /// <summary>Clears the current instance, a new instance will be initialized when next requested.</summary>
        /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Breaking change")]
        public static void ClearInstance()
        {
			Logger.LogError(">>> Entering ClearInstance()");
            useTestable = false;
            testableInstance = default(TContract);
			Logger.LogError("ClearInstance(): testableInstance → {TestableInstance}", testableInstance);
            instance = new Lazy<TContract>(InitInstance, true);
			Logger.LogError("ClearInstance(): instance → {Instance}", instance);
        }

        /// <summary>Gets the service locator factory.</summary>
        /// <returns>A factory function.</returns>
        protected abstract Func<TContract> GetFactory();

        private static TContract InitInstance()
        {
			Logger.LogError(">>> Entering InitInstance()");
            if (Factory == null)
            {
                var controllerInstance = new TSelf();
				Logger.LogError("InitInstance(): controllerInstance → {ControllerInstance}", controllerInstance);
                Factory = controllerInstance.GetFactory();
				Logger.LogError("InitInstance(): Factory → {Factory}", Factory);
				Logger.LogDebug("InitInstance(): Factory → {Factory}", Factory);
            }

			Logger.LogWarning("<<< Exiting InitInstance()");
            return Factory();
        }
    }
}
