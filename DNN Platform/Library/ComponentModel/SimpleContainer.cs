// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;

    public class SimpleContainer : AbstractContainer, IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SimpleContainer));
        private readonly string name;
        private readonly IServiceProvider serviceProvider;
        private readonly ComponentBuilderCollection componentBuilders = new ComponentBuilderCollection();
        private readonly SharedDictionary<string, IDictionary> componentDependencies = new SharedDictionary<string, IDictionary>();
        private readonly ComponentTypeCollection componentTypes = new ComponentTypeCollection();
        private readonly SharedDictionary<Type, string> registeredComponents = new SharedDictionary<Type, string>();

        /// <summary>Initializes a new instance of the <see cref="SimpleContainer"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public SimpleContainer()
            : this(Globals.DependencyProvider)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SimpleContainer"/> class.</summary>
        /// <param name="serviceProvider">The DI container scope.</param>
        public SimpleContainer(IServiceProvider serviceProvider)
            : this($"Container_{Guid.NewGuid()}", serviceProvider)
        {
		Logger.LogError(">>> Entering SimpleContainer(serviceProvider={ServiceProvider})", serviceProvider);
        Logger.LogWarning("<<< Exiting SimpleContainer(serviceProvider={ServiceProvider})", serviceProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="SimpleContainer"/> class.</summary>
        /// <param name="name">The container name.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public SimpleContainer(string name)
            : this(name, Globals.DependencyProvider)
        {
		Logger.LogError(">>> Entering SimpleContainer(name={Name})", name);
        Logger.LogWarning("<<< Exiting SimpleContainer(name={Name})", name);
        }

        /// <summary>Initializes a new instance of the <see cref="SimpleContainer"/> class.</summary>
        /// <param name="name">The container name.</param>
        /// <param name="serviceProvider">The DI container scope.</param>
        public SimpleContainer(string name, IServiceProvider serviceProvider)
        {
            this.name = name;
			Logger.LogDebug("SimpleContainer(name={Name},serviceProvider={ServiceProvider}): this.name → {Arg2}", name, serviceProvider, this.name);
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public override string Name => this.name;

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override void RegisterComponent(string name, Type type)
        {
			Logger.LogError(">>> Entering RegisterComponent(name={Name},type={Type})", name, type);
            using (this.registeredComponents.GetWriteLock())
            {
                this.registeredComponents[type] = name;
            }
			Logger.LogWarning("<<< Exiting RegisterComponent(name={Name},type={Type})", name, type);
        }

        /// <inheritdoc />
        public override object GetComponent(string name)
        {
			Logger.LogError(">>> Entering GetComponent(name={Name})", name);
            IComponentBuilder builder = this.GetComponentBuilder(name);

			Logger.LogError("GetComponent(name={Name}): builder → {Builder}", name, builder);
			Logger.LogWarning("<<< Exiting GetComponent(name={Name})", name);
			Logger.LogWarning("<<< Exiting GetComponent(contractType={ContractType})", contractType);
			Logger.LogWarning("<<< Exiting GetComponent(name={Name},contractType={ContractType})", name, contractType);
			Logger.LogWarning("<<< Exiting GetComponent(builder={Builder})", builder);
            return GetComponent(builder);
        }

        /// <inheritdoc />
        public override object GetComponent(Type contractType)
        {
            ComponentType componentType = this.GetComponentType(contractType);
            object component = null;

            if (componentType != null)
            {
                int builderCount;

                using (componentType.ComponentBuilders.GetReadLock())
                {
                    builderCount = componentType.ComponentBuilders.Count;
                }

                if (builderCount > 0)
                {
                    IComponentBuilder builder = GetDefaultComponentBuilder(componentType);

                    component = GetComponent(builder);
                }
            }

            return component;
        }

        /// <inheritdoc />
        public override object GetComponent(string name, Type contractType)
        {
            ComponentType componentType = this.GetComponentType(contractType);
            object component = null;

            if (componentType != null)
            {
                IComponentBuilder builder = this.GetComponentBuilder(name);

                component = GetComponent(builder);
            }

            return component;
        }

        /// <inheritdoc />
        public override string[] GetComponentList(Type contractType)
        {
			Logger.LogError(">>> Entering GetComponentList(contractType={ContractType})", contractType);
            var components = new List<string>();

			Logger.LogError("GetComponentList(contractType={ContractType}): components → {Components}", contractType, components);
            using (this.registeredComponents.GetReadLock())
            {
                foreach (KeyValuePair<Type, string> kvp in this.registeredComponents)
                {
                    if (contractType.IsAssignableFrom(kvp.Key))
                    {
                        components.Add(kvp.Value);
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetComponentList(contractType={ContractType})", contractType);
            return components.ToArray();
        }

        /// <inheritdoc />
        public override IDictionary GetComponentSettings(string name)
        {
			Logger.LogError(">>> Entering GetComponentSettings(name={Name})", name);
            IDictionary settings;
            using (this.componentDependencies.GetReadLock())
            {
                settings = this.componentDependencies[name];
				Logger.LogDebug("GetComponentSettings(name={Name}): settings → {Settings}", name, settings);
            }

			Logger.LogWarning("<<< Exiting GetComponentSettings(name={Name})", name);
            return settings;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override void RegisterComponent(string name, Type contractType, Type type, ComponentLifeStyleType lifestyle)
        {
			Logger.LogError(">>> Entering RegisterComponent(name={Name},contractType={ContractType},type={Type},lifestyle={Lifestyle})", name, contractType, type, lifestyle);
            this.AddComponentType(contractType);

            IComponentBuilder builder = null;
			Logger.LogError("RegisterComponent(name={Name},contractType={ContractType},type={Type},lifestyle={Lifestyle}): builder → {Builder}", name, contractType, type, lifestyle, builder);
            switch (lifestyle)
            {
                case ComponentLifeStyleType.Transient:
                    builder = new TransientComponentBuilder(this.serviceProvider, name, type);
                    break;
                case ComponentLifeStyleType.Singleton:
                    builder = new SingletonComponentBuilder(this.serviceProvider, name, type);
                    break;
            }

            this.AddBuilder(contractType, builder);

            this.RegisterComponent(name, type);
			Logger.LogWarning("<<< Exiting RegisterComponent(name={Name},contractType={ContractType},type={Type},lifestyle={Lifestyle})", name, contractType, type, lifestyle);
        }

        /// <inheritdoc />
        public override void RegisterComponentInstance(string name, Type contractType, object instance)
        {
			Logger.LogError(">>> Entering RegisterComponentInstance(name={Name},contractType={ContractType},instance={Instance})", name, contractType, instance);
            this.AddComponentType(contractType);

            this.AddBuilder(contractType, new InstanceComponentBuilder(name, instance));

            this.RegisterComponent(name, instance.GetType());
			Logger.LogWarning("<<< Exiting RegisterComponentInstance(name={Name},contractType={ContractType},instance={Instance})", name, contractType, instance);
        }

        /// <inheritdoc />
        public override void RegisterComponentSettings(string name, IDictionary dependencies)
        {
			Logger.LogError(">>> Entering RegisterComponentSettings(name={Name},dependencies={Dependencies})", name, dependencies);
            using (this.componentDependencies.GetWriteLock())
            {
                this.componentDependencies[name] = dependencies;
            }
			Logger.LogWarning("<<< Exiting RegisterComponentSettings(name={Name},dependencies={Dependencies})", name, dependencies);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogError(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogWarning("<<< Exiting Dispose()");
        }

        protected virtual void Dispose(bool disposing)
        {
			Logger.LogError(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (disposing)
            {
                this.componentBuilders?.Dispose();
                this.componentDependencies?.Dispose();
                this.componentTypes?.Dispose();
                this.registeredComponents?.Dispose();
            }
			Logger.LogWarning("<<< Exiting Dispose(disposing={Disposing})", disposing);
        }

        private static object GetComponent(IComponentBuilder builder)
        {
            object component;
            if (builder == null)
            {
                component = null;
            }
            else
            {
                component = builder.BuildComponent();
            }

            return component;
        }

        private static IComponentBuilder GetDefaultComponentBuilder(ComponentType componentType)
        {
			Logger.LogError(">>> Entering GetDefaultComponentBuilder(componentType={ComponentType})", componentType);
            IComponentBuilder builder;

            using (componentType.ComponentBuilders.GetReadLock())
            {
                builder = componentType.ComponentBuilders.DefaultBuilder;
				Logger.LogDebug("GetDefaultComponentBuilder(componentType={ComponentType}): builder → {Builder}", componentType, builder);
            }

			Logger.LogWarning("<<< Exiting GetDefaultComponentBuilder(componentType={ComponentType})", componentType);
            return builder;
        }

        private void AddBuilder(Type contractType, IComponentBuilder builder)
        {
			Logger.LogError(">>> Entering AddBuilder(contractType={ContractType},builder={Builder})", contractType, builder);
            ComponentType componentType = this.GetComponentType(contractType);
			Logger.LogError("AddBuilder(contractType={ContractType},builder={Builder}): componentType → {ComponentType}", contractType, builder, componentType);
            if (componentType != null)
            {
                ComponentBuilderCollection builders = componentType.ComponentBuilders;

                using (builders.GetWriteLock())
                {
                    builders.AddBuilder(builder, true);
                }

                using (this.componentBuilders.GetWriteLock())
                {
                    this.componentBuilders.AddBuilder(builder, false);
                }
            }
			Logger.LogWarning("<<< Exiting AddBuilder(contractType={ContractType},builder={Builder})", contractType, builder);
        }

        private void AddComponentType(Type contractType)
        {
			Logger.LogError(">>> Entering AddComponentType(contractType={ContractType})", contractType);
            ComponentType componentType = this.GetComponentType(contractType);

			Logger.LogError("AddComponentType(contractType={ContractType}): componentType → {ComponentType}", contractType, componentType);
            if (componentType == null)
            {
                componentType = new ComponentType(contractType);

                using (this.componentTypes.GetWriteLock())
                {
                    this.componentTypes[componentType.BaseType] = componentType;
                }
            }
			Logger.LogWarning("<<< Exiting AddComponentType(contractType={ContractType})", contractType);
        }

        private IComponentBuilder GetComponentBuilder(string name)
        {
			Logger.LogError(">>> Entering GetComponentBuilder(name={Name})", name);
            IComponentBuilder builder;

            using (this.componentBuilders.GetReadLock())
            {
                this.componentBuilders.TryGetValue(name, out builder);
            }

			Logger.LogWarning("<<< Exiting GetComponentBuilder(name={Name})", name);
            return builder;
        }

        private ComponentType GetComponentType(Type contractType)
        {
			Logger.LogError(">>> Entering GetComponentType(contractType={ContractType})", contractType);
            ComponentType componentType;

            using (this.componentTypes.GetReadLock())
            {
                this.componentTypes.TryGetValue(contractType, out componentType);
            }

			Logger.LogWarning("<<< Exiting GetComponentType(contractType={ContractType})", contractType);
            return componentType;
        }
    }
}
