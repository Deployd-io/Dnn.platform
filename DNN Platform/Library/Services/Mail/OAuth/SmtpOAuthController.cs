// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail.OAuth
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.DependencyInjection.Extensions;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>SMTP OAuth controller.</summary>
    public class SmtpOAuthController : ISmtpOAuthController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SmtpOAuthController));
        private readonly IEnumerable<ISmtpOAuthProvider> smtpOAuthProviders;

        /// <summary>Initializes a new instance of the <see cref="SmtpOAuthController"/> class.</summary>
        /// <param name="smtpOAuthProviders">The SMTP OAuth providers.</param>
        public SmtpOAuthController(IEnumerable<ISmtpOAuthProvider> smtpOAuthProviders)
        {
			Logger.LogDebug(">>> Entering SmtpOAuthController(smtpOAuthProviders={SmtpOAuthProviders})", smtpOAuthProviders);
            this.smtpOAuthProviders = smtpOAuthProviders;
			Logger.LogDebug("SmtpOAuthController(smtpOAuthProviders={SmtpOAuthProviders}): smtpOAuthProviders → {SmtpOAuthProviders1}", smtpOAuthProviders, smtpOAuthProviders);
			Logger.LogDebug("SmtpOAuthController(smtpOAuthProviders={SmtpOAuthProviders}): this.smtpOAuthProviders → {Arg1}", smtpOAuthProviders, this.smtpOAuthProviders);
			Logger.LogDebug("<<< Exiting SmtpOAuthController(smtpOAuthProviders={SmtpOAuthProviders})", smtpOAuthProviders);
        }

        /// <summary>Registers all of the <see cref="ISmtpOAuthProvider"/> types with the <paramref name="serviceCollection"/>.</summary>
        /// <param name="serviceCollection">The services collection.</param>
        public static void RegisterOAuthProviders(IServiceCollection serviceCollection)
        {
			Logger.LogDebug(">>> Entering RegisterOAuthProviders(serviceCollection={ServiceCollection})", serviceCollection);
            var providerTypes = TypeExtensions.SafeGetTypes()
                .Types.Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ISmtpOAuthProvider).IsAssignableFrom(t));

            serviceCollection.TryAddEnumerable(providerTypes.Select(t => ServiceDescriptor.Transient(typeof(ISmtpOAuthProvider), t)));
			Logger.LogDebug("<<< Exiting RegisterOAuthProviders(serviceCollection={ServiceCollection})", serviceCollection);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<ISmtpOAuthProvider> GetOAuthProviders()
        {
			Logger.LogDebug(">>> Entering GetOAuthProviders()");
			Logger.LogDebug("<<< Exiting GetOAuthProviders()");
            return this.smtpOAuthProviders.ToList();
        }

        /// <inheritdoc />
        public ISmtpOAuthProvider GetOAuthProvider(string name)
        {
			Logger.LogDebug(">>> Entering GetOAuthProvider(name={Name})", name);
			Logger.LogDebug("<<< Exiting GetOAuthProvider(name={Name})", name);
            return this.smtpOAuthProviders.FirstOrDefault(i => i.Name == name);
        }
    }
}
