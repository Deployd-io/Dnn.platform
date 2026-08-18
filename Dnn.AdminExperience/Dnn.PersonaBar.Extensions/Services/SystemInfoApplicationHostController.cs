// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Application;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class SystemInfoApplicationHostController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SystemInfoApplicationHostController));

        public static string FirstCharToUpper(string input)
            => FirstCharToUpper(input, CultureInfo.CurrentCulture);

        public static string FirstCharToUpper(string input, CultureInfo culture)
        {
			Logger.LogDebug(">>> Entering FirstCharToUpper(input={Input},culture={Culture})", input, culture);
            if (string.IsNullOrEmpty(input))
            {
			Logger.LogDebug("<<< Exiting FirstCharToUpper(input={Input})", input);
                return input;
            }

            return input.First().ToString().ToUpper(culture) + string.Join(string.Empty, input.Skip(1));
        }

        [HttpGet]
        public HttpResponseMessage GetApplicationInfo()
        {
			Logger.LogDebug(">>> Entering GetApplicationInfo()");
            try
            {
                var friendlyUrlProvider = GetProviderConfiguration("friendlyUrl");
				Logger.LogDebug("GetApplicationInfo(): friendlyUrlProvider → {FriendlyUrlProvider}", friendlyUrlProvider);
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    product = DotNetNukeContext.Current.Application.Description,
                    version = DotNetNukeContext.Current.Application.Version.ToString(3),
                    guid = DotNetNuke.Entities.Host.Host.GUID,
                    htmlEditorProvider = GetProviderConfiguration("htmlEditor"),
                    dataProvider = GetProviderConfiguration("data"),
                    cachingProvider = GetProviderConfiguration("caching"),
                    loggingProvider = GetProviderConfiguration("logging"),
                    friendlyUrlProvider,
                    friendlyUrlsEnabled = DotNetNuke.Entities.Host.Host.UseFriendlyUrls.ToString(),
                    friendlyUrlType = GetFriendlyUrlType(friendlyUrlProvider),
                    schedulerMode = DotNetNuke.Entities.Host.Host.SchedulerMode.ToString(),
                    webFarmEnabled = DotNetNuke.Services.Cache.CachingProvider.Instance().IsWebFarm().ToString(),
                    casPermissions = SecurityPolicy.Permissions,
                });
				Logger.LogDebug("GetApplicationInfo(): guid → {Guid}", guid);
				Logger.LogDebug("GetApplicationInfo(): product → {Product}", product);
				Logger.LogDebug("GetApplicationInfo(): cachingProvider → {CachingProvider}", cachingProvider);
				Logger.LogDebug("GetApplicationInfo(): htmlEditorProvider → {HtmlEditorProvider}", htmlEditorProvider);
				Logger.LogDebug("GetApplicationInfo(): friendlyUrlType → {FriendlyUrlType}", friendlyUrlType);
				Logger.LogDebug("GetApplicationInfo(): version → {Version}", version);
				Logger.LogDebug("GetApplicationInfo(): friendlyUrlsEnabled → {FriendlyUrlsEnabled}", friendlyUrlsEnabled);
				Logger.LogDebug("GetApplicationInfo(): loggingProvider → {LoggingProvider}", loggingProvider);
				Logger.LogDebug("GetApplicationInfo(): dataProvider → {DataProvider}", dataProvider);
				Logger.LogDebug("GetApplicationInfo(): webFarmEnabled → {WebFarmEnabled}", webFarmEnabled);
				Logger.LogDebug("GetApplicationInfo(): schedulerMode → {SchedulerMode}", schedulerMode);
				Logger.LogDebug("GetApplicationInfo(): casPermissions → {CasPermissions}", casPermissions);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetApplicationInfo()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static string GetFriendlyUrlType(string friendlyUrlProvider)
        {
            var urlProvider = (Provider)ProviderConfiguration.GetProviderConfiguration("friendlyUrl").Providers[friendlyUrlProvider];
            var urlFormat = urlProvider.Attributes["urlformat"];
            return string.IsNullOrWhiteSpace(urlFormat) ? "SearchFriendly" : FirstCharToUpper(urlFormat, CultureInfo.InvariantCulture);
        }

        private static string GetProviderConfiguration(string providerName)
        {
			Logger.LogDebug(">>> Entering GetProviderConfiguration(providerName={ProviderName})", providerName);
			Logger.LogDebug("<<< Exiting GetProviderConfiguration(providerName={ProviderName})", providerName);
            return ProviderConfiguration.GetProviderConfiguration(providerName).DefaultProvider;
        }
    }
}
