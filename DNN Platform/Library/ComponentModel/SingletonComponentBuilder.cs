// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;

    using DotNetNuke.Framework;

    internal class SingletonComponentBuilder : IComponentBuilder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SingletonComponentBuilder));
        private readonly IServiceProvider serviceProvider;
        private readonly Type type;
        private object instance;

        /// <summary>Initializes a new instance of the <see cref="SingletonComponentBuilder"/> class.</summary>
        /// <param name="serviceProvider">The DI container scope.</param>
        /// <param name="name">The name of the component.</param>
        /// <param name="type">The type of the component.</param>
        public SingletonComponentBuilder(IServiceProvider serviceProvider, string name, Type type)
        {
			Logger.LogError(">>> Entering SingletonComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type})", serviceProvider, name, type);
			Logger.LogDebug("SingletonComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type}): serviceProvider → {ServiceProvider1}", serviceProvider, name, type, serviceProvider);
        Logger.LogWarning("<<< Exiting SingletonComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type})", serviceProvider, name, type);
            this.Name = name;
			Logger.LogDebug("SingletonComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type}): this.Name → {Arg3}", serviceProvider, name, type, this.Name);
            this.serviceProvider = serviceProvider;
            this.type = type;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public object BuildComponent()
        {
			Logger.LogError(">>> Entering BuildComponent()");
            Logger.LogWarning("<<< Exiting BuildComponent()");
            return this.instance ??= Reflection.CreateObject(this.serviceProvider, this.type);
        }
    }
}
