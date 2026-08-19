// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Services.Localization;

/// <summary>Utilities for the journal module.</summary>
public class Utilities
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Utilities));
    private static readonly Regex PageRegex = new Regex(
        "<(title)[^>]*?>((?:.|\\n)*?)</\\s*\\1\\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex MetaRegex = new Regex(
        "<meta\\s*(?:(?:\\b(\\w|-)+\\b\\s*(?:=\\s*(?:\"[^\"]*\"|'[^']*'|[^\"'<> ]+)\\s*)?)*)/?\\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex MetaSubRegex = new Regex(
        "<meta[\\s]+[^>]*?(((name|property)*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?)|(content*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?))((content*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|(name*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|>)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex MetaSubRegex2 = new Regex(
        "<img[\\s]+[^>]*?((alt*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?)|(src*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?))((src*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|(alt*?[\\s]?=[\\s\\x27\\x22]+(.*?)[\\x27\\x22]+.*?>)|>)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex ResexRegex = new Regex("(\\{resx:.+?\\})", RegexOptions.Compiled);

    private static readonly Regex HtmlTextRegex = new Regex("<(.|\\n)*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Localizes a control.</summary>
    /// <param name="controlText">The text of the control to localize.</param>
    /// <returns>The localized text.</returns>
    public static string LocalizeControl(string controlText)
    {
		Logger.LogDebug(">>> Entering LocalizeControl(controlText={ControlText})", controlText);
        var matches = ResexRegex.Matches(controlText);
        foreach (Match match in matches)
        {
		Logger.LogDebug("LocalizeControl(controlText={ControlText}): matches → {Matches}", controlText, matches);
            var key = match.Value;
			Logger.LogWarning("LocalizeControl(controlText={ControlText}): key → {Key}", controlText, key);
            var replace = GetSharedResource(key);
			Logger.LogDebug("LocalizeControl(controlText={ControlText}): replace → {Replace}", controlText, replace);

            var newValue = match.Value;
			Logger.LogDebug("LocalizeControl(controlText={ControlText}): newValue → {NewValue}", controlText, newValue);
            if (!string.IsNullOrEmpty(replace))
            {
                newValue = replace;
            }

            controlText = controlText.Replace(key, newValue);
			Logger.LogDebug("LocalizeControl(controlText={ControlText}): controlText → {ControlText1}", controlText, controlText);
        }

		Logger.LogDebug("<<< Exiting LocalizeControl(controlText={ControlText})", controlText);
        return controlText;
    }

    /// <summary>Gets a shared resource string for the specified key.</summary>
    /// <param name="key">The key of the shared resource string.</param>
    /// <returns>The shared resource value string.</returns>
    public static string GetSharedResource(string key)
    {
		Logger.LogDebug(">>> Entering GetSharedResource(key={Key})", key);
        var value = Localization.GetString(key, Constants.SharedResourcesPath);
		Logger.LogDebug("<<< Exiting GetSharedResource(key={Key})", key);
        return value == string.Empty ? key : value;
    }

    /// <summary>Removes HTML tags from the specified text.</summary>
    /// <param name="sText">The text from which to remove HTML tags.</param>
    /// <returns>The text without HTML tags.</returns>
    public static string RemoveHTML(string sText)
    {
		Logger.LogDebug(">>> Entering RemoveHTML(sText={SText})", sText);
        if (string.IsNullOrEmpty(sText))
        {
            return string.Empty;
        }

        sText = HttpUtility.HtmlDecode(sText);
		Logger.LogDebug("RemoveHTML(sText={SText}): sText → {SText1}", sText, sText);
        sText = HttpUtility.UrlDecode(sText);
        sText = sText.Trim();
        if (string.IsNullOrEmpty(sText))
        {
            return string.Empty;
        }

        sText = HtmlTextRegex.Replace(sText, string.Empty);
		Logger.LogDebug("<<< Exiting RemoveHTML(sText={SText})", sText);
        return HttpUtility.HtmlEncode(sText);
    }

    /// <summary>Determines whether two users are friends.</summary>
    /// <param name="profileUser">The profile user.</param>
    /// <param name="currentUser">The current user.</param>
    /// <returns><c>true</c> if the users are friends; otherwise, <c>false</c>.</returns>
    public static bool AreFriends(UserInfo profileUser, UserInfo currentUser)
    {
		Logger.LogDebug(">>> Entering AreFriends(profileUser={ProfileUser},currentUser={CurrentUser})", profileUser, currentUser);
        var friendsRelationShip = RelationshipController.Instance.GetFriendRelationship(profileUser, currentUser);
		Logger.LogDebug("AreFriends(profileUser={ProfileUser},currentUser={CurrentUser}): friendsRelationShip → {FriendsRelationShip}", profileUser, currentUser, friendsRelationShip);
		Logger.LogDebug("<<< Exiting AreFriends(profileUser={ProfileUser},currentUser={CurrentUser})", profileUser, currentUser);
        return friendsRelationShip is { Status: RelationshipStatus.Accepted, };
    }

    /// <summary>Gets an image from the specified URL.</summary>
    /// <param name="url">The URL of the image.</param>
    /// <returns>The image as a Bitmap, or null if the image could not be retrieved.</returns>
    internal static Bitmap GetImageFromURL(string url)
    {
		Logger.LogDebug(">>> Entering GetImageFromURL(url={Url})", url);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        Bitmap bmp = null;
        try
		Logger.LogDebug("GetImageFromURL(url={Url}): bmp → {Bmp}", url, bmp);
        {
            if (!TryGetSafeUri(url, out var imageUri))
            {
                return null;
            }

            var myRequest = WebRequest.Create(imageUri);
			Logger.LogDebug("GetImageFromURL(url={Url}): myRequest → {MyRequest}", url, myRequest);
			Logger.LogDebug("GetImageFromURL(url={Url}): myRequest.Proxy → {Arg1}", url, myRequest.Proxy);
            myRequest.Proxy = null;
            using var myResponse = myRequest.GetResponse();
			Logger.LogDebug("GetImageFromURL(url={Url}): myResponse → {MyResponse}", url, myResponse);
            using var myStream = myResponse.GetResponseStream();
			Logger.LogDebug("GetImageFromURL(url={Url}): myStream → {MyStream}", url, myStream);
            var contentType = myResponse.ContentType;
            var extension = string.Empty;
			Logger.LogDebug("GetImageFromURL(url={Url}): extension → {Extension}", url, extension);
            if (contentType.Contains("png"))
            {
                extension = ".png";
            }
            else if (contentType.Contains("jpg") || contentType.Contains("jpeg"))
            {
                extension = ".jpg";
            }
            else if (contentType.Contains("gif"))
            {
                extension = ".gif";
            }

            if (!string.IsNullOrEmpty(extension) && myStream is not null)
            {
                bmp = new Bitmap(myStream);
            }

            return bmp;
        }
        catch
        {
			Logger.LogError("Exception in GetImageFromURL(url={Url}): {Arg1}", url, ex.Message);
			Logger.LogInformation("GetImageFromURL(url)={Url}: external service call {Url1} took {Arg2} ms", url, url, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting GetImageFromURL(url={Url})", url);
            return null;
        }
    }

    /// <summary>Prepares a URL for safe use by validating it and ensuring it uses the HTTP or HTTPS scheme.</summary>
    /// <param name="url">The URL to prepare.</param>
    /// <returns>The prepared URL.</returns>
    /// <exception cref="UriFormatException">Thrown if the URL is invalid or unsafe.</exception>
    internal static string PrepareURL(string url)
    {
		Logger.LogDebug(">>> Entering PrepareURL(url={Url})", url);
		Logger.LogDebug("<<< Exiting PrepareURL(url={Url})", url);
        return !TryGetSafeUri(url, out var uri)
            ? throw new UriFormatException("Invalid or unsafe URL.")
            : uri.AbsoluteUri;
    }

    /// <summary>Gets link data from the specified URL, including title, description, and images.</summary>
    /// <param name="url">The URL of the page to retrieve link data from.</param>
    /// <returns>A <see cref="LinkInfo"/> object containing the link data.</returns>
    internal static LinkInfo GetLinkData(string url)
    {
		Logger.LogDebug(">>> Entering GetLinkData(url={Url})", url);
        var page = GetPageFromURL(ref url, string.Empty, string.Empty);
        var link = new LinkInfo();
		Logger.LogDebug("GetLinkData(url={Url}): link → {Link}", url, link);
		Logger.LogDebug("GetLinkData(url={Url}): page → {Page}", url, page);
        if (string.IsNullOrEmpty(page))
        {
            return link;
        }

        link.URL = url;
		Logger.LogDebug("GetLinkData(url={Url}): link.URL → {Arg1}", url, link.URL);
		Logger.LogDebug("GetLinkData(url={Url}): link.Images → {Arg1}", url, link.Images);
        link.Images = [];
        var m = PageRegex.Match(page);
		Logger.LogDebug("GetLinkData(url={Url}): m → {M}", url, m);
        if (m.Success)
        {
            link.Title = m.Groups[2].ToString().Trim();
        }

        var matches = MetaRegex.Matches(page);
		Logger.LogDebug("GetLinkData(url={Url}): matches → {Matches}", url, matches);
        var i = 0;
        foreach (Match match in matches)
        {
            var sTempDesc = match.Groups[0].Value;
            foreach (Match subM in MetaSubRegex.Matches(sTempDesc))
            {
                if (subM.Groups[4].Value.Equals("OG:DESCRIPTION", StringComparison.InvariantCultureIgnoreCase) || subM.Groups[4].Value.Equals("DESCRIPTION", StringComparison.InvariantCultureIgnoreCase))
                {
                    link.Description = subM.Groups[9].Value;
					Logger.LogDebug("GetLinkData(url={Url}): link.Description → {Arg1}", url, link.Description);
                }

                if (subM.Groups[4].Value.Equals("OG:TITLE", StringComparison.InvariantCultureIgnoreCase))
                {
                    link.Title = subM.Groups[9].Value;
					Logger.LogDebug("GetLinkData(url={Url}): link.Title → {Arg1}", url, link.Title);
                }

                if (subM.Groups[4].Value.Equals("OG:IMAGE", StringComparison.InvariantCultureIgnoreCase))
                {
                    var image = subM.Groups[9].Value;
                    link.Images.Add(new ImageInfo { URL = image, });
                    i += 1;
                }
            }
        }

        if (!string.IsNullOrEmpty(link.Description))
        {
            link.Description = HttpUtility.HtmlDecode(link.Description);
            link.Description = HttpUtility.UrlDecode(link.Description);
            link.Description = RemoveHTML(link.Description);
        }

        if (!string.IsNullOrEmpty(link.Title))
        {
            link.Title = link.Title.Replace("&amp;", "&");
        }

        matches = MetaSubRegex2.Matches(page);

        var imgList = string.Empty;
        if (!url.Contains("http"))
        {
            url = $"http://{url}";
			Logger.LogDebug("GetLinkData(url={Url}): url → {Url1}", url, url);
        }

        var uri = new Uri(url);
		Logger.LogDebug("GetLinkData(url={Url}): uri → {Uri}", url, uri);
        var hostUrl = uri.Host;
        hostUrl = url.Contains("https:")
            ? $"https://{hostUrl}"
            : $"http://{hostUrl}";

        foreach (Match match in matches)
        {
            var imgSrc = match.Groups[5].Value;
			Logger.LogDebug("GetLinkData(url={Url}): imgSrc → {ImgSrc}", url, imgSrc);
            if (string.IsNullOrEmpty(imgSrc))
            {
                imgSrc = match.Groups[8].Value;
            }

            if (string.IsNullOrEmpty(imgSrc))
            {
                continue;
            }

            if (!imgSrc.Contains("http"))
            {
                imgSrc = hostUrl + imgSrc;
            }

            var img = new ImageInfo { URL = imgSrc, };
            if (!imgList.Contains(imgSrc))
            {
                var bmp = GetImageFromURL(imgSrc);
				Logger.LogDebug("GetLinkData(url={Url}): bmp → {Bmp}", url, bmp);
                if (bmp != null)
                {
                    if (bmp.Height > 25 & bmp.Height < 500 & bmp.Width > 25 & bmp.Width < 500)
                    {
                        link.Images.Add(img);
                        imgList += imgSrc;
                        i += 1;
                    }
                }
            }

            if (i == 10)
            {
                break;
            }
        }

		Logger.LogDebug("<<< Exiting GetLinkData(url={Url})", url);
        return link;
    }

    /// <summary>Gets the HTML content of a page from the specified URL, optionally using provided credentials for authentication.</summary>
    /// <param name="url">The URL of the page to retrieve.</param>
    /// <param name="username">The username for authentication (optional).</param>
    /// <param name="password">The password for authentication (optional).</param>
    /// <returns>The HTML content of the page as a string.</returns>
    internal static string GetPageFromURL(ref string url, string username, string password)
    {
		Logger.LogDebug(">>> Entering GetPageFromURL(url={Url},username={Username},password=<redacted>)", url, username);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        var cookies = new CookieContainer();

		Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): cookies → {Cookies}", url, username, cookies);
        Uri pageUri;
        try
        {
            url = PrepareURL(url);
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): url → {Url1}", url, username, url);
            pageUri = new Uri(url);
        }
        catch (Exception ex)
        {
			Logger.LogError("Exception in GetPageFromURL(url={Url},username={Username},password=<redacted>): {Arg3}", url, username, ex.getMessage());
            Services.Exceptions.Exceptions.LogException(ex);
            return string.Empty;
        }

        var webRequest = (HttpWebRequest)WebRequest.Create(pageUri);
		Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): webRequest → {WebRequest}", url, username, webRequest);
		Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): webRequest.KeepAlive → {Arg3}", url, username, webRequest.KeepAlive);
		Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): webRequest.Proxy → {Arg3}", url, username, webRequest.Proxy);
		Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): webRequest.CookieContainer → {Arg3}", url, username, webRequest.CookieContainer);
        webRequest.KeepAlive = false;
        webRequest.Proxy = null;
        webRequest.CookieContainer = cookies;
        if (!string.IsNullOrEmpty(username) & !string.IsNullOrEmpty(password))
        {
            webRequest.Credentials = new NetworkCredential(username, password);
        }

        var html = string.Empty;
		Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): html → {Html}", url, username, html);
        try
        {
            var webResponse = (HttpWebResponse)webRequest.GetResponse();
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): webResponse → {WebResponse}", url, username, webResponse);
            if (webRequest.HaveResponse & webResponse.StatusCode == HttpStatusCode.OK)
            {
                webResponse.Cookies = webRequest.CookieContainer.GetCookies(webRequest.RequestUri);
                using var stream = webResponse.GetResponseStream();
				Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): stream → {Stream}", url, username, stream);
                using var streamReader = new StreamReader(stream, Encoding.UTF8);
				Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): streamReader → {StreamReader}", url, username, streamReader);
                html = streamReader.ReadToEnd();
                streamReader.Close();
                stream.Close();
            }

            webResponse.Close();
        }
        catch (Exception ex)
        {
            Services.Exceptions.Exceptions.LogException(ex);
        }

		Logger.LogInformation("GetPageFromURL(url,username,password)={Url},{Username},{Password}: external service call {Url1} took {Arg4} ms", url, username, password, url, __logStart.ElapsedMilliseconds);
		Logger.LogDebug("<<< Exiting GetPageFromURL(url={Url},username={Username},password=<redacted>)", url, username);
        return html;
    }

    private static bool TryGetSafeUri(string input, out Uri uri)
    {
		Logger.LogDebug(">>> Entering TryGetSafeUri(input={Input})", input);
        uri = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var url = input.Trim();
		Logger.LogDebug("TryGetSafeUri(input={Input}): url → {Url}", input, url);
        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = $"http://{url}";
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
        {
            return false;
        }

        if (!(uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Block loopback hostnames explicitly. Even if DNS resolution changes, localhost and
        // loopback names should never be fetched by this server-side preview endpoint.
        if (uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var addresses = Dns.GetHostAddresses(uri.DnsSafeHost);
			Logger.LogDebug("TryGetSafeUri(input={Input}): addresses → {Addresses}", input, addresses);
            if (addresses.Length == 0)
            {
                return false;
            }

            if (addresses.Any(IsBlockedAddress))
            {
                return false;
            }
        }
        catch
        {
			Logger.LogError("Exception in TryGetSafeUri(input={Input}): {Arg1}", input, ex.Message);
		Logger.LogDebug("<<< Exiting TryGetSafeUri(input={Input})", input);
            return false;
        }

        return true;
    }

    private static bool IsBlockedAddress(IPAddress address)
    {
		Logger.LogDebug(">>> Entering IsBlockedAddress(address={Address})", address);
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        switch (address.AddressFamily)
        {
            case AddressFamily.InterNetwork:
                {
                    var bytes = address.GetAddressBytes();
					Logger.LogDebug("IsBlockedAddress(address={Address}): bytes → {Bytes}", address, bytes);

                    // IPv4 ranges blocked for SSRF protection:
                    // - 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16 (RFC 1918 private networks)
                    // - 127.0.0.0/8 (loopback)
                    // - 169.254.0.0/16 (link-local/APIPA)
                    // These are non-public/internal address spaces and should not be reachable via URL preview.
                    return bytes[0] == 10
                           || bytes[0] == 127
                           || (bytes[0] == 169 && bytes[1] == 254)
                           || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                           || (bytes[0] == 192 && bytes[1] == 168);
                }

            case AddressFamily.InterNetworkV6:
                {
                    var bytes = address.GetAddressBytes();
                    var uniqueLocal = (bytes[0] & 0xFE) == 0xFC;
					Logger.LogDebug("IsBlockedAddress(address={Address}): uniqueLocal → {UniqueLocal}", address, uniqueLocal);

                    // IPv6 ranges blocked for the same reason as IPv4 internal ranges:
                    // - ::1 (loopback)
                    // - fe80::/10 (link-local)
                    // - fec0::/10 (site-local, deprecated but still non-public)
                    // - fc00::/7 (unique local addresses)
                    // These are internal/non-routable scopes and should not be fetched by server-side previews.
                    return address.Equals(IPAddress.IPv6Loopback)
                           || address.IsIPv6LinkLocal
                           || address.IsIPv6SiteLocal
                           || uniqueLocal;
                }

            default:
				Logger.LogDebug("<<< Exiting IsBlockedAddress(address={Address})", address);
                return false;
        }
    }
}
