// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Sitemap
{
    using System;
    using System.Text;
    using System.Web;

    using DotNetNuke.Entities.Portals;

    public class SitemapHandler : IHttpHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SitemapHandler));
        /// <inheritdoc />
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc />
        public void ProcessRequest(HttpContext context)
        {
			Logger.LogError(">>> Entering ProcessRequest(context={Context})", context);
            try
            {
                HttpResponse response = context.Response;
				Logger.LogError("ProcessRequest(context={Context}): response → {Response}", context, response);

                response.ContentType = "text/xml";
				Logger.LogDebug("ProcessRequest(context={Context}): response.ContentType → {Arg1}", context, response.ContentType);
                response.ContentEncoding = Encoding.UTF8;
				Logger.LogDebug("ProcessRequest(context={Context}): response.ContentEncoding → {Arg1}", context, response.ContentEncoding);

                var builder = new SitemapBuilder(PortalSettings.Current);
				Logger.LogError("ProcessRequest(context={Context}): builder → {Builder}", context, builder);

                if (context.Request.QueryString["i"] != null)
                {
                    // This is a request for one of the files that build the sitemapindex
                    builder.GetSitemapIndexFile(context.Request.QueryString["i"], response.Output);
                }
                else
                {
                    builder.BuildSiteMap(response.Output);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in ProcessRequest(context={Context}): {Arg1}", context, exc.Message);
                Exceptions.Exceptions.LogException(exc);
            }
			Logger.LogWarning("<<< Exiting ProcessRequest(context={Context})", context);
        }
    }
}
