// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;

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

        public static string LocalizeControl(string controlText)
        {
			Logger.LogDebug(">>> Entering LocalizeControl(controlText={ControlText})", controlText);
            string sKey = string.Empty;
            string sReplace = string.Empty;
			Logger.LogDebug("LocalizeControl(controlText={ControlText}): sReplace → {SReplace}", controlText, sReplace);
            MatchCollection matches = default(MatchCollection);
			Logger.LogDebug("LocalizeControl(controlText={ControlText}): matches → {Matches}", controlText, matches);
            matches = ResexRegex.Matches(controlText);
            foreach (Match match in matches)
            {
			Logger.LogDebug("LocalizeControl(controlText={ControlText}): sKey → {SKey}", controlText, sKey);
                sKey = match.Value;
                sReplace = GetSharedResource(sKey);

                string newValue = match.Value;
				Logger.LogDebug("LocalizeControl(controlText={ControlText}): newValue → {NewValue}", controlText, newValue);
                if (!string.IsNullOrEmpty(sReplace))
                {
                    newValue = sReplace;
                }

                controlText = controlText.Replace(sKey, newValue);
				Logger.LogDebug("LocalizeControl(controlText={ControlText}): controlText → {ControlText1}", controlText, controlText);
            }

			Logger.LogDebug("<<< Exiting LocalizeControl(controlText={ControlText})", controlText);
            return controlText;
        }

        public static string GetSharedResource(string key)
        {
			Logger.LogDebug(">>> Entering GetSharedResource(key={Key})", key);
            string sValue = key;
            sValue = DotNetNuke.Services.Localization.Localization.GetString(key, Constants.SharedResourcesPath);
			Logger.LogDebug("GetSharedResource(key={Key}): sValue → {SValue}", key, sValue);
            if (sValue == string.Empty)
            {
                return key;
            }
            else
            {
				Logger.LogDebug("<<< Exiting GetSharedResource(key={Key})", key);
                return sValue;
            }
        }

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
            sText = HttpUtility.HtmlEncode(sText);
			Logger.LogDebug("<<< Exiting RemoveHTML(sText={SText})", sText);
            return sText;
        }

        public static bool AreFriends(UserInfo profileUser, UserInfo currentUser)
        {
			Logger.LogDebug(">>> Entering AreFriends(profileUser={ProfileUser},currentUser={CurrentUser})", profileUser, currentUser);
            var friendsRelationShip = RelationshipController.Instance.GetFriendRelationship(profileUser, currentUser);
			Logger.LogDebug("AreFriends(profileUser={ProfileUser},currentUser={CurrentUser}): friendsRelationShip → {FriendsRelationShip}", profileUser, currentUser, friendsRelationShip);
			Logger.LogDebug("<<< Exiting AreFriends(profileUser={ProfileUser},currentUser={CurrentUser})", profileUser, currentUser);
            return friendsRelationShip != null && friendsRelationShip.Status == RelationshipStatus.Accepted;
        }

        internal static Bitmap GetImageFromURL(string url)
        {
			Logger.LogDebug(">>> Entering GetImageFromURL(url={Url})", url);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            string sImgName = string.Empty;
            System.Net.WebRequest myRequest = default(System.Net.WebRequest);
			Logger.LogDebug("GetImageFromURL(url={Url}): myRequest → {MyRequest}", url, myRequest);
            Bitmap bmp = null;
			Logger.LogDebug("GetImageFromURL(url={Url}): bmp → {Bmp}", url, bmp);
            try
            {
                myRequest = System.Net.WebRequest.Create(url);
                myRequest.Proxy = null;
				Logger.LogDebug("GetImageFromURL(url={Url}): myRequest.Proxy → {Arg1}", url, myRequest.Proxy);
                using (WebResponse myResponse = myRequest.GetResponse())
                {
                    using (Stream myStream = myResponse.GetResponseStream())
                    {
                        string sContentType = myResponse.ContentType;
                        string sExt = string.Empty;
						Logger.LogDebug("GetImageFromURL(url={Url}): sExt → {SExt}", url, sExt);
                        if (sContentType.Contains("png"))
                        {
                            sExt = ".png";
                        }
                        else if (sContentType.Contains("jpg"))
                        {
                            sExt = ".jpg";
                        }
                        else if (sContentType.Contains("jpeg"))
                        {
                            sExt = ".jpg";
                        }
                        else if (sContentType.Contains("gif"))
                        {
                            sExt = ".gif";
                        }

                        if (!string.IsNullOrEmpty(sExt))
                        {
                            bmp = new Bitmap(myStream);
                        }
                    }
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

        internal static string PrepareURL(string url)
        {
			Logger.LogDebug(">>> Entering PrepareURL(url={Url})", url);
            url = url.Trim();
            if (!url.StartsWith("http"))
			Logger.LogDebug("PrepareURL(url={Url}): url → {Url1}", url, url);
            {
                url = "http://" + url;
            }

            if (url.Contains("https://"))
            {
                url = url.Replace("https://", "http://");
            }

            if (url.Contains("http://http://"))
            {
                url = url.Replace("http://http://", "http://");
            }

            if (!(url.IndexOf("http://") == 0))
            {
                url = "http://" + url;
            }

            Uri objURI = null;
			Logger.LogDebug("PrepareURL(url={Url}): objURI → {ObjURI}", url, objURI);

            objURI = new Uri(url);
			Logger.LogDebug("<<< Exiting PrepareURL(url={Url})", url);
            return url;
        }

        internal static LinkInfo GetLinkData(string url)
        {
			Logger.LogDebug(">>> Entering GetLinkData(url={Url})", url);
            string sPage = GetPageFromURL(ref url, string.Empty, string.Empty);
            LinkInfo link = new LinkInfo();
			Logger.LogDebug("GetLinkData(url={Url}): link → {Link}", url, link);
            if (string.IsNullOrEmpty(sPage))
            {
                return link;
            }

            string sTitle = string.Empty;
            string sDescription = string.Empty;
            string sImage = string.Empty;
			Logger.LogDebug("GetLinkData(url={Url}): sImage → {SImage}", url, sImage);

            link.URL = url;
			Logger.LogDebug("GetLinkData(url={Url}): link.URL → {Arg1}", url, link.URL);
            link.Images = new List<ImageInfo>();
            Match m = PageRegex.Match(sPage);
			Logger.LogDebug("GetLinkData(url={Url}): m → {M}", url, m);
            if (m.Success)
            {
                link.Title = m.Groups[2].ToString().Trim();
            }

            MatchCollection matches = default(MatchCollection);
			Logger.LogDebug("GetLinkData(url={Url}): matches → {Matches}", url, matches);
            matches = MetaRegex.Matches(sPage);
            int i = 0;
            foreach (Match match in matches)
            {
                string sTempDesc = match.Groups[0].Value;
                foreach (Match subM in MetaSubRegex.Matches(sTempDesc))
                {
                    if (subM.Groups[4].Value.Equals("OG:DESCRIPTION", StringComparison.InvariantCultureIgnoreCase))
                    {
                        link.Description = subM.Groups[9].Value;
						Logger.LogDebug("GetLinkData(url={Url}): link.Description → {Arg1}", url, link.Description);
                    }
                    else if (subM.Groups[4].Value.Equals("DESCRIPTION", StringComparison.InvariantCultureIgnoreCase))
                    {
                        link.Description = subM.Groups[9].Value;
                    }

                    if (subM.Groups[4].Value.Equals("OG:TITLE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        link.Title = subM.Groups[9].Value;
						Logger.LogDebug("GetLinkData(url={Url}): link.Title → {Arg1}", url, link.Title);
                    }

                    if (subM.Groups[4].Value.Equals("OG:IMAGE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        sImage = subM.Groups[9].Value;
                        ImageInfo img = new ImageInfo();
						Logger.LogDebug("GetLinkData(url={Url}): img → {Img}", url, img);
						Logger.LogDebug("GetLinkData(url={Url}): img.URL → {Arg1}", url, img.URL);
                        img.URL = sImage;
                        link.Images.Add(img);
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

            matches = MetaSubRegex2.Matches(sPage);

            string imgList = string.Empty;
            string hostUrl = string.Empty;
			Logger.LogDebug("GetLinkData(url={Url}): hostUrl → {HostUrl}", url, hostUrl);
            if (!url.Contains("http"))
            {
                url = "http://" + url;
				Logger.LogDebug("GetLinkData(url={Url}): url → {Url1}", url, url);
            }

            Uri uri = new Uri(url);
			Logger.LogDebug("GetLinkData(url={Url}): uri → {Uri}", url, uri);
            hostUrl = uri.Host;
            if (url.Contains("https:"))
            {
                hostUrl = "https://" + hostUrl;
            }
            else
            {
                hostUrl = "http://" + hostUrl;
            }

            foreach (Match match in matches)
            {
                string sImg = match.Groups[5].Value;
				Logger.LogDebug("GetLinkData(url={Url}): sImg → {SImg}", url, sImg);
                if (string.IsNullOrEmpty(sImg))
                {
                    sImg = match.Groups[8].Value;
                }

                if (!string.IsNullOrEmpty(sImg))
                {
                    if (!sImg.Contains("http"))
                    {
                        sImg = hostUrl + sImg;
                    }

                    ImageInfo img = new ImageInfo();
                    img.URL = sImg;
                    if (!imgList.Contains(sImg))
                    {
                        Bitmap bmp = Utilities.GetImageFromURL(sImg);
						Logger.LogDebug("GetLinkData(url={Url}): bmp → {Bmp}", url, bmp);
                        if (bmp != null)
                        {
                            if (bmp.Height > 25 & bmp.Height < 500 & bmp.Width > 25 & bmp.Width < 500)
                            {
                                link.Images.Add(img);
                                imgList += sImg;
                                i += 1;
                            }
                        }
                    }

                    if (i == 10)
                    {
                        break;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetLinkData(url={Url})", url);
            return link;
        }

        internal static string GetPageFromURL(ref string url, string username, string password)
        {
			Logger.LogDebug(">>> Entering GetPageFromURL(url={Url},username={Username},password=<redacted>)", url, username);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            url = PrepareURL(url);
            HttpWebRequest objWebRequest = default(HttpWebRequest);
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): url → {Url1}", url, username, url);
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): objWebRequest → {ObjWebRequest}", url, username, objWebRequest);
            HttpWebResponse objWebResponse = default(HttpWebResponse);
            CookieContainer cookies = new CookieContainer();
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): objWebResponse → {ObjWebResponse}", url, username, objWebResponse);
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): cookies → {Cookies}", url, username, cookies);
            Uri objURI = new Uri(url);
            objWebRequest = (HttpWebRequest)HttpWebRequest.Create(objURI);
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): objURI → {ObjURI}", url, username, objURI);
            objWebRequest.KeepAlive = false;
            objWebRequest.Proxy = null;
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): objWebRequest.KeepAlive → {Arg3}", url, username, objWebRequest.KeepAlive);
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): objWebRequest.Proxy → {Arg3}", url, username, objWebRequest.Proxy);
			Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): objWebRequest.CookieContainer → {Arg3}", url, username, objWebRequest.CookieContainer);
            objWebRequest.CookieContainer = cookies;
            if (!string.IsNullOrEmpty(username) & !string.IsNullOrEmpty(password))
            {
                NetworkCredential nc = new NetworkCredential(username, password);
				Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): nc → {Nc}", url, username, nc);
				Logger.LogDebug("GetPageFromURL(url={Url},username={Username},password=<redacted>): objWebRequest.Credentials → {Arg3}", url, username, objWebRequest.Credentials);
                objWebRequest.Credentials = nc;
            }

            string sHTML = string.Empty;
            try
            {
                objWebResponse = (HttpWebResponse)objWebRequest.GetResponse();
                Encoding enc = Encoding.UTF8;

                string contentType = objWebResponse.ContentType;

                if ((objWebRequest.HaveResponse == true) & objWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    objWebResponse.Cookies = objWebRequest.CookieContainer.GetCookies(objWebRequest.RequestUri);
                    using (Stream objStream = objWebResponse.GetResponseStream())
                    using (StreamReader objStreamReader = new StreamReader(objStream, enc))
                    {
                        sHTML = objStreamReader.ReadToEnd();
                        objStreamReader.Close();
                        objStream.Close();
                    }
                }

                objWebResponse.Close();
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in GetPageFromURL(url={Url},username={Username},password=<redacted>): {Arg3}", url, username, ex.getMessage());
                Services.Exceptions.Exceptions.LogException(ex);
            }

			Logger.LogInformation("GetPageFromURL(url,username,password)={Url},{Username},{Password}: external service call {Url1} took {Arg4} ms", url, username, password, url, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting GetPageFromURL(url={Url},username={Username},password=<redacted>)", url, username);
            return sHTML;
        }
    }
}
