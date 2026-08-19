// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Instrumentation;

    public static class ComponentFactory
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ComponentFactory));

        public static IContainer Container { get; set; }

        public static void InstallComponents(params IComponentInstaller[] installers)
        {
			Logger.LogDebug(">>> Entering InstallComponents(installers={Installers})", installers);
            if (installers == null)
            {
				Logger.LogWarning("Throwing ArgumentNullException from InstallComponents(installers={Installers})", installers);
                throw new ArgumentNullException(nameof(installers));
            }

            VerifyContainer();
            foreach (var installer in installers)
            {
                if (installer == null)
                {
                    throw new ArgumentNullException(nameof(installers), "Item in installers was null");
                }

                installer.InstallComponents(Container);
            }
			Logger.LogDebug("<<< Exiting InstallComponents(installers={Installers})", installers);
        }

        public static object GetComponent(string name)
        {
            VerifyContainer();
			Logger.LogDebug("<<< Exiting GetComponent()");
			Logger.LogDebug("<<< Exiting GetComponent(name={Name})", name);
            return Container.GetComponent(name);
        }

        public static TContract GetComponent<TContract>()
        {
            VerifyContainer();
            return Container.GetComponent<TContract>();
        }

        public static object GetComponent(Type contractType)
        {
            VerifyContainer();
			Logger.LogDebug("<<< Exiting GetComponent(contractType={ContractType})", contractType);
            return Container.GetComponent(contractType);
        }

        public static TContract GetComponent<TContract>(string name)
        {
            VerifyContainer();
            return Container.GetComponent<TContract>(name);
        }

        public static object GetComponent(string name, Type contractType)
        {
            VerifyContainer();
            return Container.GetComponent(name, contractType);
        }

        public static string[] GetComponentList(Type contractType)
        {
			Logger.LogDebug(">>> Entering GetComponentList(contractType={ContractType})", contractType);
            VerifyContainer();
			Logger.LogDebug("<<< Exiting GetComponentList(contractType={ContractType})", contractType);
            return Container.GetComponentList(contractType);
        }

        public static string[] GetComponentList<TContract>()
        {
            VerifyContainer();
            return Container.GetComponentList<TContract>();
        }

        public static Dictionary<string, TContract> GetComponents<TContract>()
        {
            VerifyContainer();
            var components = new Dictionary<string, TContract>();
            foreach (string componentName in GetComponentList<TContract>())
            {
                components[componentName] = GetComponent<TContract>(componentName);
            }

            return components;
        }

        public static IDictionary GetComponentSettings(string name)
        {
			Logger.LogDebug(">>> Entering GetComponentSettings(name={Name})", name);
            VerifyContainer();
			Logger.LogDebug("<<< Exiting GetComponentSettings(name={Name})", name);
			Logger.LogDebug("<<< Exiting GetComponentSettings()");
            return Container.GetComponentSettings(name);
        }

        public static IDictionary GetComponentSettings(Type component)
        {
            VerifyContainer();
            return Container.GetComponentSettings(component);
        }

        public static IDictionary GetComponentSettings<TComponent>()
        {
            VerifyContainer();
            return Container.GetComponentSettings<TComponent>();
        }

        public static void RegisterComponent<TComponent>()
            where TComponent : class
        {
            VerifyContainer();
            Container.RegisterComponent<TComponent>();
        }

        public static void RegisterComponent<TContract, TComponent>()
            where TComponent : class
        {
			Logger.LogDebug(">>> Entering RegisterComponent()");
            VerifyContainer();
            Container.RegisterComponent<TContract, TComponent>();
			Logger.LogDebug("<<< Exiting RegisterComponent()");
        }

        public static void RegisterComponent(Type componentType)
        {
			Logger.LogDebug(">>> Entering RegisterComponent(componentType={ComponentType})", componentType);
            VerifyContainer();
            Container.RegisterComponent(componentType);
			Logger.LogDebug("<<< Exiting RegisterComponent(componentType={ComponentType})", componentType);
        }

        public static void RegisterComponent(Type contractType, Type componentType)
        {
			Logger.LogDebug(">>> Entering RegisterComponent(contractType={ContractType},componentType={ComponentType})", contractType, componentType);
            VerifyContainer();
            Container.RegisterComponent(contractType, componentType);
			Logger.LogDebug("<<< Exiting RegisterComponent(contractType={ContractType},componentType={ComponentType})", contractType, componentType);
        }

        public static void RegisterComponent<TComponent>(string name)
            where TComponent : class
        {
            VerifyContainer();
            Container.RegisterComponent<TComponent>(name);
        }

        public static void RegisterComponent<TContract, TComponent>(string name)
            where TComponent : class
        {
			Logger.LogDebug(">>> Entering RegisterComponent(name={Name})", name);
            VerifyContainer();
            Container.RegisterComponent<TContract, TComponent>(name);
			Logger.LogDebug("<<< Exiting RegisterComponent(name={Name})", name);
        }

        public static void RegisterComponent(string name, Type componentType)
        {
            VerifyContainer();
            Container.RegisterComponent(name, componentType);
        }

        public static void RegisterComponent(string name, Type contractType, Type componentType)
        {
            VerifyContainer();
            Container.RegisterComponent(name, contractType, componentType);
        }

        public static void RegisterComponentInstance(string name, Type contractType, object instance)
        {
			Logger.LogDebug(">>> Entering RegisterComponentInstance(name={Name},contractType={ContractType},instance={Instance})", name, contractType, instance);
            VerifyContainer();
            Container.RegisterComponentInstance(name, contractType, instance);
			Logger.LogDebug("<<< Exiting RegisterComponentInstance(name={Name},contractType={ContractType},instance={Instance})", name, contractType, instance);
        }

        public static void RegisterComponentInstance(string name, object instance)
        {
            VerifyContainer();
            Container.RegisterComponentInstance(name, instance);
			Logger.LogDebug(">>> Entering RegisterComponentInstance(name={Name},instance={Instance})", name, instance);
			Logger.LogDebug("<<< Exiting RegisterComponentInstance(name={Name},instance={Instance})", name, instance);
        }

        public static void RegisterComponentInstance<TContract>(string name, object instance)
        {
            VerifyContainer();
            Container.RegisterComponentInstance<TContract>(name, instance);
        }

        public static void RegisterComponentInstance<TContract>(object instance)
        {
            VerifyContainer();
            Container.RegisterComponentInstance<TContract>(instance);
        }

        public static void RegisterComponentSettings(string name, IDictionary dependencies)
        {
			Logger.LogDebug(">>> Entering RegisterComponentSettings(name={Name},dependencies={Dependencies})", name, dependencies);
            VerifyContainer();
            Container.RegisterComponentSettings(name, dependencies);
			Logger.LogDebug("<<< Exiting RegisterComponentSettings(name={Name},dependencies={Dependencies})", name, dependencies);
        }

        public static void RegisterComponentSettings(Type component, IDictionary dependencies)
        {
			Logger.LogDebug(">>> Entering RegisterComponentSettings(component={Component},dependencies={Dependencies})", component, dependencies);
            VerifyContainer();
            Container.RegisterComponentSettings(component, dependencies);
			Logger.LogDebug("<<< Exiting RegisterComponentSettings(component={Component},dependencies={Dependencies})", component, dependencies);
        }

        public static void RegisterComponentSettings<TComponent>(IDictionary dependencies)
        {
            VerifyContainer();
            Container.RegisterComponentSettings<TComponent>(dependencies);
        }

        private static void VerifyContainer()
        {
			Logger.LogDebug(">>> Entering VerifyContainer()");
            if (Container == null)
            {
                Logger.Warn("Container was null, instantiating SimpleContainer");
                Container = new SimpleContainer();
            }
			Logger.LogDebug("VerifyContainer(): Container → {Container}", Container);
			Logger.LogDebug("<<< Exiting VerifyContainer()");
        }
    }
}
