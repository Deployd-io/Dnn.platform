// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;

    using DotNetNuke.Framework;

    internal class TransientComponentBuilder : IComponentBuilder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IServiceProvider serviceProvider;
        private readonly Type type;

        /// <summary>Initializes a new instance of the <see cref="TransientComponentBuilder"/> class.</summary>
        /// <param name="serviceProvider">The DI container scope.</param>
        /// <param name="name">The name of the component.</param>
        /// <param name="type">The type of the component.</param>
        public TransientComponentBuilder(IServiceProvider serviceProvider, string name, Type type)
        {
			Logger.LogDebug(">>> Entering TransientComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type})", serviceProvider, name, type);
            this.Name = name;
            this.serviceProvider = serviceProvider;
			Logger.LogDebug("TransientComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type}): this.Name → {Arg3}", serviceProvider, name, type, this.Name);
			Logger.LogDebug("TransientComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type}): serviceProvider → {ServiceProvider1}", serviceProvider, name, type, serviceProvider);
			Logger.LogDebug("TransientComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type}): this.serviceProvider → {Arg3}", serviceProvider, name, type, this.serviceProvider);
            this.type = type;
			Logger.LogDebug("<<< Exiting TransientComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type})", serviceProvider, name, type);
			Logger.LogDebug("TransientComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type}): type → {Type1}", serviceProvider, name, type, type);
			Logger.LogDebug("TransientComponentBuilder(serviceProvider={ServiceProvider},name={Name},type={Type}): this.type → {Arg3}", serviceProvider, name, type, this.type);
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public object BuildComponent()
        {
			Logger.LogDebug(">>> Entering BuildComponent()");
			Logger.LogDebug("<<< Exiting BuildComponent()");
            return Reflection.CreateObject(this.serviceProvider, this.type);
        }
    }
}
