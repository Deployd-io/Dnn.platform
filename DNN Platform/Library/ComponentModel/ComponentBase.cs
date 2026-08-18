// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>A base class for a Dnn component.</summary>
    /// <typeparam name="TContract">The contract type.</typeparam>
    /// <typeparam name="TType">The component type.</typeparam>
    public abstract class ComponentBase<TContract, TType>
        where TType : class, TContract
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ComponentBase));
        private static TContract testableInstance;
        private static bool useTestable;

        /// <summary>Gets an instance of the Component.</summary>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Breaking change")]
        public static TContract Instance
        {
            get
            {
                if (useTestable && testableInstance != null)
                {
                    return testableInstance;
                }

                var component = ComponentFactory.GetComponent<TContract>();

                if (component == null)
                {
                    component = (TContract)Activator.CreateInstance(typeof(TType), true);
                    ComponentFactory.RegisterComponentInstance<TContract>(component);
                }

                return component;
            }
        }

        /// <summary>Registers an instance of a component.</summary>
        /// <param name="instance">The instance to register.</param>
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Breaking change")]
        public static void RegisterInstance(TContract instance)
        {
			Logger.LogDebug(">>> Entering RegisterInstance(instance={Instance})", instance);
            if (ComponentFactory.GetComponent<TContract>() == null)
            {
                ComponentFactory.RegisterComponentInstance<TContract>(instance);
            }
			Logger.LogDebug("<<< Exiting RegisterInstance(instance={Instance})", instance);
        }

        /// <summary>Registers an instance to use for the Singleton.</summary>
        /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
        /// <param name="instance">The instance to set.</param>
        internal static void SetTestableInstance(TContract instance)
        {
			Logger.LogDebug(">>> Entering SetTestableInstance(instance={Instance})", instance);
            testableInstance = instance;
            useTestable = true;
			Logger.LogDebug("SetTestableInstance(instance={Instance}): useTestable → {UseTestable}", instance, useTestable);
			Logger.LogDebug("SetTestableInstance(instance={Instance}): testableInstance → {TestableInstance}", instance, testableInstance);
			Logger.LogDebug("<<< Exiting SetTestableInstance(instance={Instance})", instance);
        }

        /// <summary>Clears the current instance, a new instance will be initialized when next requested.</summary>
        /// <remarks>Intended for unit testing purposes, not thread safe.</remarks>
        internal static void ClearInstance()
        {
			Logger.LogDebug(">>> Entering ClearInstance()");
            useTestable = false;
            testableInstance = default(TContract);
			Logger.LogDebug("ClearInstance(): testableInstance → {TestableInstance}", testableInstance);
			Logger.LogDebug("<<< Exiting ClearInstance()");
        }
    }
}
