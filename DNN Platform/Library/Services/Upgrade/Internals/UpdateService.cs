// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Upgrade.Internals
{
    using System.IO;
    using System.Net;

    using DotNetNuke.Application;
    using DotNetNuke.Common;

    public class UpdateService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpdateService));
        private static string ApplicationVersion
        {
            get
            {
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return Globals.FormatVersion(version, "00", 3, string.Empty);
            }
        }

        private static string ApplicationName
        {
            get
            {
                return DotNetNukeContext.Current.Application.Name;
            }
        }

        public static StreamReader GetLanguageList()
        {
			Logger.LogDebug(">>> Entering GetLanguageList()");
            string url = DotNetNukeContext.Current.Application.UpgradeUrl + "/languages.aspx";
            url += "?core=" + ApplicationVersion;
            url += "&type=Framework";
            url += "&name=" + ApplicationName;

            StreamReader myResponseReader = GetResponseAsStream(url);
			Logger.LogDebug("GetLanguageList(): myResponseReader → {MyResponseReader}", myResponseReader);
			Logger.LogDebug("<<< Exiting GetLanguageList()");
            return myResponseReader;
        }

        public static string GetLanguageDownloadUrl(string cultureCode)
        {
			Logger.LogDebug(">>> Entering GetLanguageDownloadUrl(cultureCode={CultureCode})", cultureCode);
            string url = DotNetNukeContext.Current.Application.UpgradeUrl + "/languages.aspx";
            url += "?core=" + ApplicationVersion;
            url += "&type=Framework";
            url += "&name=" + ApplicationName;
            url += "&culture=" + cultureCode;

            StreamReader myResponseReader = GetResponseAsStream(url);
			Logger.LogDebug("GetLanguageDownloadUrl(cultureCode={CultureCode}): myResponseReader → {MyResponseReader}", cultureCode, myResponseReader);
            string downloadUrl = myResponseReader.ReadToEnd();
			Logger.LogDebug("<<< Exiting GetLanguageDownloadUrl(cultureCode={CultureCode})", cultureCode);
            return downloadUrl;
        }

        private static StreamReader GetResponseAsStream(string url)
        {
			Logger.LogDebug(">>> Entering GetResponseAsStream(url={Url})", url);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // creating the proxy for the service call using the HttpWebRequest class
            var webReq = (HttpWebRequest)WebRequest.Create(url);
			Logger.LogDebug("GetResponseAsStream(url={Url}): webReq → {WebReq}", url, webReq);

            // Set the method/action type
            webReq.Method = "GET";
			Logger.LogDebug("GetResponseAsStream(url={Url}): webReq.Method → {Arg1}", url, webReq.Method);

            // We use form contentType
            webReq.ContentType = "text/xml; charset=utf-8";
			Logger.LogDebug("GetResponseAsStream(url={Url}): webReq.ContentType → {Arg1}", url, webReq.ContentType);

            // Get the response handle, we have no true response yet!
            var webResp = (HttpWebResponse)webReq.GetResponse();
			Logger.LogDebug("GetResponseAsStream(url={Url}): webResp → {WebResp}", url, webResp);

            // Now, we read the response (the string), and output it.
            Stream myResponse = webResp.GetResponseStream();
			Logger.LogDebug("GetResponseAsStream(url={Url}): myResponse → {MyResponse}", url, myResponse);

            // read the stream into streamreader
            var myResponseReader = new StreamReader(myResponse);
			Logger.LogDebug("GetResponseAsStream(url={Url}): myResponseReader → {MyResponseReader}", url, myResponseReader);
			Logger.LogInformation("GetResponseAsStream(url)={Url}: external service call {Url1} took {Arg2} ms", url, url, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting GetResponseAsStream(url={Url})", url);
            return myResponseReader;
        }
    }
}
