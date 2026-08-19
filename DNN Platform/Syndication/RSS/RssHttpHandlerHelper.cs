// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Web;
    using System.Web.Security;

    /// <summary>Helper class (for <see cref="RssHttpHandlerBase{TRssChannelType,TRssItemType,TRssImageType}"/>) to pack and unpack user name and channel to from/to query string.</summary>
    public class RssHttpHandlerHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RssHttpHandlerHelper));
        private RssHttpHandlerHelper()
        {
		Logger.LogDebug(">>> Entering RssHttpHandlerHelper()");
		Logger.LogDebug("<<< Exiting RssHttpHandlerHelper()");
        }

        /// <summary>Generate the link (to the .ashx) containing the channel name and (encoded) userName.</summary>
        /// <param name="handlerPath">The handler path.</param>
        /// <param name="channelName">The channel name.</param>
        /// <param name="userName">The user name.</param>
        /// <returns>A URL to the channel.</returns>
        public static string GenerateChannelLink(string handlerPath, string channelName, string userName)
        {
			Logger.LogDebug(">>> Entering GenerateChannelLink(handlerPath={HandlerPath},channelName={ChannelName},userName={UserName})", handlerPath, channelName, userName);
            string link = VirtualPathUtility.ToAbsolute(handlerPath);

            if (string.IsNullOrEmpty(userName))
            {
                if (!string.IsNullOrEmpty(channelName))
                {
                    link += "?c=" + HttpUtility.UrlEncode(channelName);
                }
            }
            else
            {
                if (channelName == null)
                {
                    channelName = string.Empty;
                }

                userName = "." + userName; // not to confuse the encrypted string with real auth ticket for real user
				Logger.LogDebug("GenerateChannelLink(handlerPath={HandlerPath},channelName={ChannelName},userName={UserName}): userName → {UserName1}", handlerPath, channelName, userName, userName);
                DateTime ticketDate = DateTime.Now.AddDays(-100); // already expired
				Logger.LogDebug("GenerateChannelLink(handlerPath={HandlerPath},channelName={ChannelName},userName={UserName}): ticketDate → {TicketDate}", handlerPath, channelName, userName, ticketDate);

                var t = new FormsAuthenticationTicket(2, userName, ticketDate, ticketDate.AddDays(2), false, channelName, "/");
				Logger.LogDebug("GenerateChannelLink(handlerPath={HandlerPath},channelName={ChannelName},userName={UserName}): t → {T}", handlerPath, channelName, userName, t);

                link += "?t=" + FormsAuthentication.Encrypt(t);
            }

			Logger.LogDebug("<<< Exiting GenerateChannelLink(handlerPath={HandlerPath},channelName={ChannelName},userName={UserName})", handlerPath, channelName, userName);
            return link;
        }

        /// <summary>Parses the channel's query-string to extract the channel name and user name.</summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="channelName">The channel name.</param>
        /// <param name="userName">The user name or <see cref="string.Empty"/>.</param>
        internal static void ParseChannelQueryString(HttpRequest request, out string channelName, out string userName)
        {
			Logger.LogDebug(">>> Entering ParseChannelQueryString(request={Request})", request);
            string ticket = request.QueryString["t"];

            if (string.IsNullOrEmpty(ticket))
            {
                userName = string.Empty;

				Logger.LogDebug("ParseChannelQueryString(request={Request}): userName → {UserName}", request, userName);
                // optional unencrypted channel name
                channelName = request.QueryString["c"];
            }
            else
            {
                // encrypted user name and channel name
                FormsAuthenticationTicket t = FormsAuthentication.Decrypt(ticket);
				Logger.LogDebug("ParseChannelQueryString(request={Request}): t → {T}", request, t);
                userName = t.Name.Substring(1); // remove extra prepended '.'
                channelName = t.UserData;
            }
			Logger.LogDebug("<<< Exiting ParseChannelQueryString(request={Request})", request);
        }
    }
}
