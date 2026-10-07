// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;
    using System.Collections;

    public abstract class AbstractContainer : IContainer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AbstractContainer));
        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract object GetComponent(string name);

        /// <inheritdoc />
        public abstract object GetComponent(string name, Type contractType);

        /// <inheritdoc />
        public abstract object GetComponent(Type contractType);

        /// <inheritdoc />
        public virtual TContract GetComponent<TContract>()
        {
			Logger.LogError(">>> Entering GetComponent()");
            Logger.LogWarning("<<< Exiting GetComponent()");
			Logger.LogError(">>> Entering GetComponent(name={Name})", name);
            Logger.LogWarning("<<< Exiting GetComponent(name={Name})", name);
            return (TContract)this.GetComponent(typeof(TContract));
        }

        /// <inheritdoc />
        public virtual TContract GetComponent<TContract>(string name)
        {
            return (TContract)this.GetComponent(name, typeof(TContract));
        }

        /// <inheritdoc />
        public abstract string[] GetComponentList(Type contractType);

        /// <inheritdoc />
        public virtual string[] GetComponentList<TContract>()
        {
			Logger.LogError(">>> Entering GetComponentList()");
            Logger.LogWarning("<<< Exiting GetComponentList()");
            return this.GetComponentList(typeof(TContract));
        }

        /// <inheritdoc />
        public abstract IDictionary GetComponentSettings(string name);

        /// <inheritdoc />
        public virtual IDictionary GetComponentSettings(Type component)
        {
			Logger.LogError(">>> Entering GetComponentSettings(component={Component})", component);
            Logger.LogWarning("<<< Exiting GetComponentSettings(component={Component})", component);
            return this.GetComponentSettings(component.FullName);
        }

        /// <inheritdoc />
        public IDictionary GetComponentSettings<TComponent>()
        {
            return this.GetComponentSettings(typeof(TComponent).FullName);
        }

        /// <inheritdoc />
        public abstract void RegisterComponent(string name, Type contractType, Type componentType, ComponentLifeStyleType lifestyle);

        /// <inheritdoc />
        public virtual void RegisterComponent(string name, Type contractType, Type componentType)
        {
			Logger.LogError(">>> Entering RegisterComponent(name={Name},contractType={ContractType},componentType={ComponentType})", name, contractType, componentType);
            this.RegisterComponent(name, contractType, componentType, ComponentLifeStyleType.Singleton);
			Logger.LogWarning("<<< Exiting RegisterComponent(name={Name},contractType={ContractType},componentType={ComponentType})", name, contractType, componentType);
			Logger.LogError(">>> Entering RegisterComponent(name={Name},componentType={ComponentType})", name, componentType);
			Logger.LogWarning("<<< Exiting RegisterComponent(name={Name},componentType={ComponentType})", name, componentType);
        }

        /// <inheritdoc />
        public virtual void RegisterComponent(string name, Type componentType)
        {
            this.RegisterComponent(name, componentType, componentType, ComponentLifeStyleType.Singleton);
        }

        /// <inheritdoc />
        public virtual void RegisterComponent(Type contractType, Type componentType)
        {
			Logger.LogError(">>> Entering RegisterComponent(contractType={ContractType},componentType={ComponentType})", contractType, componentType);
            this.RegisterComponent(componentType.FullName, contractType, componentType, ComponentLifeStyleType.Singleton);
			Logger.LogWarning("<<< Exiting RegisterComponent(contractType={ContractType},componentType={ComponentType})", contractType, componentType);
        }

        /// <inheritdoc />
        public virtual void RegisterComponent(Type contractType, Type componentType, ComponentLifeStyleType lifestyle)
        {
			Logger.LogError(">>> Entering RegisterComponent(contractType={ContractType},componentType={ComponentType},lifestyle={Lifestyle})", contractType, componentType, lifestyle);
            this.RegisterComponent(componentType.FullName, contractType, componentType, lifestyle);
			Logger.LogWarning("<<< Exiting RegisterComponent(contractType={ContractType},componentType={ComponentType},lifestyle={Lifestyle})", contractType, componentType, lifestyle);
			Logger.LogError(">>> Entering RegisterComponent(componentType={ComponentType})", componentType);
			Logger.LogWarning("<<< Exiting RegisterComponent(componentType={ComponentType})", componentType);
        }

        /// <inheritdoc />
        public virtual void RegisterComponent(Type componentType)
        {
            this.RegisterComponent(componentType.FullName, componentType, componentType, ComponentLifeStyleType.Singleton);
        }

        /// <inheritdoc />
        public virtual void RegisterComponent<TComponent>()
            where TComponent : class
        {
            this.RegisterComponent(typeof(TComponent));
        }

        /// <inheritdoc />
        public virtual void RegisterComponent<TComponent>(string name)
            where TComponent : class
        {
            this.RegisterComponent<TComponent, TComponent>(name, ComponentLifeStyleType.Singleton);
        }

        /// <inheritdoc />
        public virtual void RegisterComponent<TComponent>(string name, ComponentLifeStyleType lifestyle)
            where TComponent : class
        {
            this.RegisterComponent<TComponent, TComponent>(name, lifestyle);
        }

        /// <inheritdoc />
        public virtual void RegisterComponent<TContract, TComponent>()
            where TComponent : class
        {
            this.RegisterComponent(typeof(TContract), typeof(TComponent));
        }

        /// <inheritdoc />
        public virtual void RegisterComponent<TContract, TComponent>(string name)
            where TComponent : class
        {
            this.RegisterComponent<TContract, TComponent>(name, ComponentLifeStyleType.Singleton);
        }

        /// <inheritdoc />
        public virtual void RegisterComponent<TContract, TComponent>(string name, ComponentLifeStyleType lifestyle)
            where TComponent : class
        {
            this.RegisterComponent(name, typeof(TContract), typeof(TComponent), lifestyle);
        }

        /// <inheritdoc />
        public abstract void RegisterComponentSettings(string name, IDictionary dependencies);

        /// <inheritdoc />
        public virtual void RegisterComponentSettings(Type component, IDictionary dependencies)
        {
			Logger.LogError(">>> Entering RegisterComponentSettings(component={Component},dependencies={Dependencies})", component, dependencies);
            this.RegisterComponentSettings(component.FullName, dependencies);
			Logger.LogWarning("<<< Exiting RegisterComponentSettings(component={Component},dependencies={Dependencies})", component, dependencies);
        }

        /// <inheritdoc />
        public virtual void RegisterComponentSettings<TComponent>(IDictionary dependencies)
        {
            this.RegisterComponentSettings(typeof(TComponent).FullName, dependencies);
        }

        /// <inheritdoc />
        public abstract void RegisterComponentInstance(string name, Type contractType, object instance);

        /// <inheritdoc />
        public void RegisterComponentInstance(string name, object instance)
        {
            this.RegisterComponentInstance(name, instance.GetType(), instance);
			Logger.LogError(">>> Entering RegisterComponentInstance(instance={Instance})", instance);
			Logger.LogWarning("<<< Exiting RegisterComponentInstance(instance={Instance})", instance);
        }

        /// <inheritdoc />
        public void RegisterComponentInstance<TContract>(object instance)
        {
            this.RegisterComponentInstance<TContract>(instance.GetType().FullName, instance);
        }

        /// <inheritdoc />
        public void RegisterComponentInstance<TContract>(string name, object instance)
        {
            this.RegisterComponentInstance(name, typeof(TContract), instance);
        }

        public virtual IDictionary GetCustomDependencies<TComponent>()
        {
			Logger.LogError(">>> Entering GetCustomDependencies()");
            Logger.LogWarning("<<< Exiting GetCustomDependencies()");
            return this.GetComponentSettings(typeof(TComponent).FullName);
        }
    }
}
