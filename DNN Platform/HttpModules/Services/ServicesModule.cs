// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.Services
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common;

    /// <summary>An <see cref="IHttpModule"/> for DNN's ASP.NET Web API Services Framework.</summary>
    public class ServicesModule : IHttpModule
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServicesModule));
        /// <inheritdoc cref="Globals.ServicesFrameworkRegex"/>
        public static readonly Regex ServiceApi = Globals.ServicesFrameworkRegex;

        /// <inheritdoc />
        public void Init(HttpApplication context)
        {
			Logger.LogDebug(">>> Entering Init(context={Context})", context);
            context.BeginRequest += InitDnn;

            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
			Logger.LogDebug("<<< Exiting Init(context={Context})", context);
        }

        /// <inheritdoc />
        public void Dispose()
        {
		Logger.LogDebug(">>> Entering Dispose()");
		Logger.LogDebug("<<< Exiting Dispose()");
        }

        private static void InitDnn(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering InitDnn(sender={Sender},e={E})", sender, e);
            if (sender is HttpApplication app && ServiceApi.IsMatch(app.Context.Request.RawUrl.ToLowerInvariant()))
            {
                Initialize.Init(app);
            }
			Logger.LogDebug("<<< Exiting InitDnn(sender={Sender},e={E})", sender, e);
        }

        private static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreSendRequestHeaders(sender={Sender},e={E})", sender, e);
            if (sender is HttpApplication app)
            {
                // WEB API should not send cookies and other specific headers in response;
                // they reveal too much info and are security risk
                var headers = app.Response.Headers;
                headers.Remove("Server");

                // DNN-8325
                ////if (ServiceApi.IsMatch(app.Context.Request.RawUrl.ToLowerInvariant()))
                ////{
                ////   headers.Remove("Set-Cookie");
                ////}
            }
			Logger.LogDebug("<<< Exiting OnPreSendRequestHeaders(sender={Sender},e={E})", sender, e);
        }
    }
}
