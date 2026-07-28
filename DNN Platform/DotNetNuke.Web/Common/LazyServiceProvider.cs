// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common
{
    using System;
    using System.ComponentModel;

    /// <summary>An <see cref="IServiceProvider"/> implementation which wraps a provider that can be replaced or set after initialization.</summary>
    public class LazyServiceProvider : IServiceProvider, INotifyPropertyChanged
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private IServiceProvider serviceProvider;

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
			Logger.LogDebug(">>> Entering GetService(serviceType={ServiceType})", serviceType);
			Logger.LogDebug("<<< Exiting GetService(serviceType={ServiceType})", serviceType);
            return this.serviceProvider?.GetService(serviceType);
        }

        /// <summary>Sets the service provider.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        internal void SetProvider(IServiceProvider serviceProvider)
        {
			Logger.LogDebug(">>> Entering SetProvider(serviceProvider={ServiceProvider})", serviceProvider);
            this.serviceProvider = serviceProvider;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(serviceProvider)));
			Logger.LogDebug("<<< Exiting SetProvider(serviceProvider={ServiceProvider})", serviceProvider);
			Logger.LogDebug("SetProvider(serviceProvider={ServiceProvider}): this.serviceProvider → {Arg1}", serviceProvider, this.serviceProvider);
			Logger.LogDebug("SetProvider(serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", serviceProvider, serviceProvider);
        }
    }
}
