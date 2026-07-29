// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;

    using DNN.Integration.Test.Framework.Controllers;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Common.Utilities;

    internal class WebApiConnector : IWebApiConnector, IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public const string FileFilters =
            "swf,jpg,jpeg,jpe,gif,bmp,png,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,xml," +
            "xsl,xsd,css,zip,template,htmtemplate,ico,avi,mpg,mpeg,mp3,wmv,mov,wav";

        public const string RqVerifTokenName = "__RequestVerificationToken";
        public const string RqVerifTokenNameNoUndescrores = "RequestVerificationToken";

        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.131 Safari/537.36";

        private const string LoginPath = "/Login";

        private const string LogoffPath = "/Home/ctl/Logoff";

        // private const string LogoffPath = "/Home/ctl/Logoff.aspx";
        // private const string LogoffPath = "/Logoff.aspx");
        // private const string LogoffPath = "/Home/tabid/55/ctl/LogOff/Default.aspx");
        private const string UploadFileRequestPath = "API/internalservices/fileupload/postfile";

        private const string ActivityStreamUploadFilePath = "API/DNNCorp/ActivityStream/FileUpload/UploadFile";
        private static readonly Dictionary<string, CachedWebPage> CachedPages = new Dictionary<string, CachedWebPage>();

        private static readonly Regex HtmlFormInuts = new Regex(
            @"<input .*?/>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Encoding Encoding = Encoding.UTF8;

        private string _userAgentValue;
        private int _userId;

        private CookieContainer _sessionCookiesContainer;
        private Cookie _cookieVerificationToken;
        private string _inputFieldVerificationToken;
        private string _currentTabId;

        private WebApiConnector(string siteUrl)
        {
			Logger.LogDebug(">>> Entering WebApiConnector(siteUrl={SiteUrl})", siteUrl);
            this.Timeout = TimeSpan.FromMinutes(1);
            this.Domain = new Uri(siteUrl);
			Logger.LogInformation("WebApiConnector(siteUrl={SiteUrl}): Timeout → {Timeout}", siteUrl, Timeout);
            this.IsLoggedIn = false;
			Logger.LogDebug("WebApiConnector(siteUrl={SiteUrl}): this.IsLoggedIn → {Arg1}", siteUrl, this.IsLoggedIn);
            this.ResetUserId();
            this._sessionCookiesContainer = new CookieContainer();
			Logger.LogDebug("WebApiConnector(siteUrl={SiteUrl}): _sessionCookiesContainer → {_sessionCookiesContainer}", siteUrl, _sessionCookiesContainer);
            this._cookieVerificationToken = new Cookie(RqVerifTokenName, string.Empty, "/", this.Domain.Host);
			Logger.LogDebug("WebApiConnector(siteUrl={SiteUrl}): _cookieVerificationToken → {_cookieVerificationToken}", siteUrl, _cookieVerificationToken);
            this.AvoidCaching = false;
			Logger.LogDebug("<<< Exiting WebApiConnector(siteUrl={SiteUrl})", siteUrl);
			Logger.LogDebug("WebApiConnector(siteUrl={SiteUrl}): this.AvoidCaching → {Arg1}", siteUrl, this.AvoidCaching);
        }

        /// <summary>
        /// Gets the userID will be available only if the user is logged in.
        /// After obtaining it for the first time, it will be cached until logout.
        /// </summary>
        public int UserId
        {
            get
            {
                if (this._userId <= 0 && this.IsLoggedIn)
                {
                    this._userId = UserController.GetUserId(this.UserName);
                }

                return this._userId;
            }
        }

        public Uri Domain { get; }

        public CookieContainer SessionCookies
        {
            get
            {
                this.EnsureLoggedIn();
                return this._sessionCookiesContainer;
            }
        }

        public string UserAgentValue
        {
            get { return this._userAgentValue ?? (this._userAgentValue = ConfigurationManager.AppSettings["HttpUserAgent"] ?? DefaultUserAgent); }
            set { this._userAgentValue = value; }
        }

        public bool AvoidCaching { get; set; }

        public string UserName { get; private set; }

        public bool IsLoggedIn { get; private set; }

        // ==============================================================================
        // Adapted from http://www.briangrinstead.com/blog/multipart-form-post-in-c#

        public TimeSpan Timeout { get; set; }

        public DateTime LoggedInAtTime { get; private set; }

        public static WebApiConnector GetWebConnector(string siteUrl, string userName)
        {
			Logger.LogDebug(">>> Entering GetWebConnector(siteUrl={SiteUrl},userName={UserName})", siteUrl, userName);
			Logger.LogDebug("<<< Exiting GetWebConnector(siteUrl={SiteUrl},userName={UserName})", siteUrl, userName);
            return new WebApiConnector(siteUrl)
            {
                UserName = (userName ?? string.Empty).Replace("'", string.Empty),
            };
        }

        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            this.Logout();
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        public void Logout()
        {
			Logger.LogDebug(">>> Entering Logout()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (this.IsLoggedIn)
            {
                this.LoggedInAtTime = DateTime.MinValue;
				Logger.LogDebug("Logout(): this.LoggedInAtTime → {Arg0}", this.LoggedInAtTime);
                this.IsLoggedIn = false;
                this.ResetUserId();
				Logger.LogDebug("Logout(): this.IsLoggedIn → {Arg0}", this.IsLoggedIn);
                try
                {
                    var requestUriString = CombineUrlPath(this.Domain, LogoffPath);
                    var httpWebRequest1 = (HttpWebRequest)WebRequest.Create(requestUriString);
					Logger.LogDebug("Logout(): requestUriString → {RequestUriString}", requestUriString);
					Logger.LogDebug("Logout(): httpWebRequest1 → {HttpWebRequest1}", httpWebRequest1);
					Logger.LogDebug("Logout(): httpWebRequest1.Method → {Arg0}", httpWebRequest1.Method);
					Logger.LogDebug("Logout(): httpWebRequest1.KeepAlive → {Arg0}", httpWebRequest1.KeepAlive);
					Logger.LogDebug("Logout(): httpWebRequest1.CookieContainer → {Arg0}", httpWebRequest1.CookieContainer);
					Logger.LogDebug("Logout(): httpWebRequest1.ReadWriteTimeout → {Arg0}", httpWebRequest1.ReadWriteTimeout);
					Logger.LogDebug("Logout(): httpWebRequest1.UserAgent → {Arg0}", httpWebRequest1.UserAgent);
                    httpWebRequest1.Method = "GET";
                    httpWebRequest1.KeepAlive = false;
                    httpWebRequest1.CookieContainer = this._sessionCookiesContainer;
                    httpWebRequest1.ReadWriteTimeout = 90;
                    httpWebRequest1.UserAgent = this.UserAgentValue;
                    using (httpWebRequest1.GetResponse())
                    {
                        // no need to read the response stream after we logoff
                    }
                }
                finally
                {
                    this._cookieVerificationToken = null;
					Logger.LogDebug("Logout(): _cookieVerificationToken → {_cookieVerificationToken}", _cookieVerificationToken);
					Logger.LogDebug("Logout(): this._cookieVerificationToken → {Arg0}", this._cookieVerificationToken);
                    this._sessionCookiesContainer = new CookieContainer();

					Logger.LogDebug("Logout(): _sessionCookiesContainer → {_sessionCookiesContainer}", _sessionCookiesContainer);
                    var url = CombineUrlPath(this.Domain, "/");
                    CachedWebPage cachedPage;
					Logger.LogDebug("Logout(): url → {Url}", url);
                    if (!this.AvoidCaching && CachedPages.TryGetValue(url, out cachedPage) &&
                        cachedPage.FetchDateTime < DateTime.Now.AddMinutes(-19.5))
                    {
                        this._inputFieldVerificationToken = cachedPage.VerificationToken;
						Logger.LogDebug("Logout(): this._inputFieldVerificationToken → {Arg0}", this._inputFieldVerificationToken);
                    }
                    else
                    {
                        this._inputFieldVerificationToken = null;
                    }
                }
            }
			Logger.LogDebug("<<< Exiting Logout()");
			Logger.LogInformation("Logout(): external service call {RequestUriString} took {Arg1} ms", requestUriString, __logStart.ElapsedMilliseconds);
        }

        public bool Login(string password)
        {
			Logger.LogDebug(">>> Entering Login(password={Password})", password);
            if (this.IsLoggedIn)
            {
                return true;
            }

            // This method uses multi-part parameters in the post body
            // the response is similar to this:
            // code => HTTP/1.1 302 Found
            // body => <html><head><title>Object moved</title></head><body><h2>Object moved to <a href="http://[domain]/">here</a>.</h2></body></html>
            const string fieldsPrefix = "dnn$ctr$Login$Login_DNN";
            var postData = new Dictionary<string, object>
                {
                    { fieldsPrefix + "$txtUsername", this.UserName },
                    { fieldsPrefix + "$txtPassword", password },
                    { "__EVENTTARGET", fieldsPrefix + "$cmdLogin" }, // most important field; button action
                    { "__EVENTARGUMENT", string.Empty },
                };

            var excludedInputPrefixes = new List<string>();

			Logger.LogDebug("Login(password={Password}): excludedInputPrefixes → {ExcludedInputPrefixes}", password, excludedInputPrefixes);
            // CombineUrlPath(_domain, LoginPath);
            using (var httpResponse2 = this.PostUserForm(LoginPath, postData, excludedInputPrefixes, false))
            {
                if (httpResponse2 != null && httpResponse2.StatusCode < HttpStatusCode.BadRequest) // < 400
                {
                    using (httpResponse2)
                    {
                        this.VerifyLogInCookie(httpResponse2);
                    }
                }
            }

            if (this.IsLoggedIn)
            {
                this.LoggedInAtTime = DateTime.Now;
				Logger.LogDebug("Login(password={Password}): this.LoggedInAtTime → {Arg1}", password, this.LoggedInAtTime);
            }

			Logger.LogDebug("<<< Exiting Login(password={Password})", password);
            return this.IsLoggedIn;
        }

        public async Task<HttpResponseMessage> UploadUserFile(
            string fileName,
            bool waitHttpResponse = true,
            int userId = -1)
        {
            this.EnsureLoggedIn();

            var folder = "Users";
            if (userId > Null.NullInteger)
            {
                var rootFolder = PathUtils.GetUserFolderPathElement(userId, PathUtils.UserFolderElement.Root);
                var subFolder = PathUtils.GetUserFolderPathElement(userId, PathUtils.UserFolderElement.SubFolder);
                folder = $"Users/{rootFolder}/{subFolder}/{userId}/";
            }

            return await this.UploadFile(fileName, folder, waitHttpResponse);
        }

        public HttpResponseMessage ActivityStreamUploadUserFile(IDictionary<string, string> headers, string fileName)
        {
            this.EnsureLoggedIn();
            return this.ActivityStreamUploadFile(headers, fileName);
        }

        public async Task<bool> UploadCmsFile(string fileName, string portalFolder)
        {
			Logger.LogDebug(">>> Entering UploadCmsFile(fileName={FileName},portalFolder={PortalFolder})", fileName, portalFolder);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.EnsureLoggedIn();
            var result = await this.UploadFile(fileName, portalFolder);
			Logger.LogInformation("UploadCmsFile(fileName,portalFolder)={FileName},{PortalFolder}: external service call took {Arg2} ms", fileName, portalFolder, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("UploadCmsFile(fileName={FileName},portalFolder={PortalFolder}): result → {Result}", fileName, portalFolder, result);
			Logger.LogDebug("<<< Exiting UploadCmsFile(fileName={FileName},portalFolder={PortalFolder})", fileName, portalFolder);
            return result.IsSuccessStatusCode;
        }

        public HttpResponseMessage PostJson(
            string relativeUrl,
            object content, IDictionary<string, string> contentHeaders = null, bool waitHttpResponse = true, bool ignoreLoggedIn = false)
        {
            if (!ignoreLoggedIn)
            {
                this.EnsureLoggedIn();
            }

            using (var client = this.CreateHttpClient("/", true))
            {
                var rqHeaders = client.DefaultRequestHeaders;
                rqHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                rqHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html", 0.5d));
                rqHeaders.Add("X-Requested-With", "XMLHttpRequest");
                rqHeaders.UserAgent.Clear();
                rqHeaders.UserAgent.ParseAdd(this.UserAgentValue);

                if (contentHeaders != null)
                {
                    foreach (var hdr in contentHeaders)
                    {
                        if (rqHeaders.Contains(hdr.Key))
                        {
                            rqHeaders.Remove(hdr.Key);
                        }

                        rqHeaders.Add(hdr.Key, hdr.Value);
                    }
                }

                var requestUriString = CombineUrlPath(this.Domain, relativeUrl);
                var result = client.PostAsJsonAsync(requestUriString, content).Result;
				Logger.LogDebug(">>> Entering CreateHttpClient(path={Path},autoRedirect={AutoRedirect})", path, autoRedirect);
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogDebug("<<< Exiting CreateHttpClient(path={Path},autoRedirect={AutoRedirect})", path, autoRedirect);
                return !waitHttpResponse
                    ? result
                    : EnsureSuccessResponse(result, "PostJson", requestUriString);
            }
        }

        public HttpResponseMessage PutJson(
            string relativeUrl,
            object content, IDictionary<string, string> contentHeaders = null, bool waitHttpResponse = true, bool ignoreLoggedIn = false)
        {
            if (!ignoreLoggedIn)
            {
                this.EnsureLoggedIn();
            }

            using (var client = this.CreateHttpClient("/", true))
            {
                var rqHeaders = client.DefaultRequestHeaders;
                rqHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                rqHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html", 0.5d));
                rqHeaders.Add("X-Requested-With", "XMLHttpRequest");
                rqHeaders.UserAgent.Clear();
                rqHeaders.UserAgent.ParseAdd(this.UserAgentValue);

                if (contentHeaders != null)
                {
                    foreach (var hdr in contentHeaders)
                    {
                        if (rqHeaders.Contains(hdr.Key))
                        {
                            rqHeaders.Remove(hdr.Key);
                        }

                        rqHeaders.Add(hdr.Key, hdr.Value);
                    }
                }

                var requestUriString = CombineUrlPath(this.Domain, relativeUrl);
                var result = client.PutAsJsonAsync(requestUriString, content).Result;
                return !waitHttpResponse
                    ? result
                    : EnsureSuccessResponse(result, "PutJson", requestUriString);
            }
        }

        public HttpWebResponse PostUserForm(string relativeUrl, IDictionary<string, object> formFields,
            List<string> excludedInputPrefixes, bool checkUserLoggedIn = true, bool followRedirect = false)
        {
            if (checkUserLoggedIn)
            {
                this.EnsureLoggedIn();
            }

            var clientHandler = new HttpClientHandler
            {
                CookieContainer = this._sessionCookiesContainer,
            };

            var postParameters = new Dictionary<string, object>();
            string[] inputFields;
            using (var client = new HttpClient(clientHandler)
            {
                BaseAddress = this.Domain,
                Timeout = this.Timeout,
            })
            {
                inputFields = this.GetPageInputFields(client, relativeUrl);
            }

            var firstField = formFields.First().Key;
            if (!inputFields.Any(f => f.Contains(firstField)))
            {
                // the form doesn't have the proper input fields
                Console.WriteLine(
                    @"Either User '{0}' has no rights to post to this page {1} or " +
                    @"this page does not contain correct form ", this.UserName, relativeUrl);

                // return null;
            }

            foreach (var field in inputFields)
            {
                XElement xe = null;
                try
                {
                    // fixes the error in HTML file input fields; e.g.:
                    //      <input type="file" name="postfile" multiple />
                    // but should work with XHTML file input fields; e.g.:
                    //      <input type="file" name="postfile" multiple="multiple" />
                    var text = field.Contains(" multiple") && !field.Contains(" multiple=")
                        ? field.Replace(" multiple", string.Empty)
                        : field;

                    xe = XElement.Parse(text);
                }
                catch (XmlException ex)
                {
                    Console.WriteLine(@"XmlException: cannot parse input fields: {0}. Ex: {1}", field, ex.Message);
                }

                var attrs = xe == null
                                ? new XAttribute[0]
                                : xe.Attributes().ToArray();

                var inputType = attrs.FirstOrDefault(a => a.Name == "type");
                var inputName = attrs.FirstOrDefault(a => a.Name == "name");
                var inputValue = attrs.FirstOrDefault(a => a.Name == "value");

                if (inputType != null && inputName != null)
                {
                    switch (inputType.Value)
                    {
                        case "hidden":
                            {
                                if (!postParameters.ContainsKey(inputName.Value))
                                {
                                    var value = inputValue == null ? string.Empty : inputValue.Value;
                                    if (formFields.ContainsKey(inputName.Value))
                                    {
                                        value = formFields[inputName.Value].ToString();
                                    }

                                    postParameters.Add(inputName.Value, value);
                                }
                            }

                            break;
                        case "text":
                        case "checkbox":
                        case "radio":
                            if (formFields.ContainsKey(inputName.Value) &&
                                !postParameters.ContainsKey(inputName.Value))
                            {
                                postParameters.Add(inputName.Value, formFields[inputName.Value]);
                            }

                            break;

                            // other types as "submit", etc. are ignored/discarded
                    }
                }
            }

            foreach (var field in formFields)
            {
                if (!postParameters.ContainsKey(field.Key))
                {
                    postParameters.Add(field.Key, field.Value);
                }
            }

            if (excludedInputPrefixes != null)
            {
                var keys = postParameters.Keys.ToArray();

                var filteredKeys = from prefix in excludedInputPrefixes
                                   from key in keys
                                   where key.StartsWith(prefix)
                                   select key;

                foreach (var key in filteredKeys)
                {
                    postParameters.Remove(key);
                }
            }

            if (postParameters.Count > 0)
            {
                var url = CombineUrlPath(this.Domain, relativeUrl);
                return this.MultipartFormDataPost(url, this.UserAgentValue, postParameters, null, followRedirect);
            }

            return null;
        }

        public HttpWebResponse MultipartFormDataPost(string relativeUrl, IDictionary<string, object> postParameters, IDictionary<string, string> headers = null, bool followRedirect = false)
        {
            var url = CombineUrlPath(this.Domain, relativeUrl);
            return this.MultipartFormDataPost(url, this.UserAgentValue, postParameters, headers, followRedirect);
        }

        public HttpResponseMessage GetContent(
            string relativeUrl, object parameters, Dictionary<string, string> contentHeaders = null,
            bool waitHttpResponse = true, bool autoRedirect = true)
        {
            var url = relativeUrl + "?" + QueryStringFromObject(parameters);
            return this.GetContent(url, contentHeaders, waitHttpResponse, autoRedirect);
        }

        public HttpResponseMessage GetContent(
            string relativeUrl, Dictionary<string, string> contentHeaders = null, bool waitHttpResponse = true, bool autoRedirect = true)
        {
            using (var client = this.CreateHttpClient("/", autoRedirect))
            {
                var rqHeaders = client.DefaultRequestHeaders;
                rqHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                rqHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html", 0.5d));
                rqHeaders.Add("X-Requested-With", "XMLHttpRequest");
                rqHeaders.UserAgent.ParseAdd(this.UserAgentValue);

                if (contentHeaders != null)
                {
                    foreach (var hdr in contentHeaders)
                    {
                        rqHeaders.Add(hdr.Key, hdr.Value);
                    }
                }

                var requestUriString = CombineUrlPath(this.Domain, relativeUrl);
                var uri = new Uri(requestUriString);
                var result = client.GetAsync(uri.AbsoluteUri).Result;
                return !waitHttpResponse
                    ? result
                    : EnsureSuccessResponse(result, "GetContent", uri.AbsoluteUri);
            }
        }

        private static string GetVerificationToken(string pageData)
        {
			Logger.LogDebug(">>> Entering GetVerificationToken(pageData={PageData})", pageData);
            if (!string.IsNullOrEmpty(pageData))
            {
                const string str1 = "<input name=\"" + RqVerifTokenName + "\" type=\"hidden\" value=\"";
                var startIndex1 = pageData.IndexOf(str1, StringComparison.Ordinal) + str1.Length;
				Logger.LogDebug("GetVerificationToken(pageData={PageData}): startIndex1 → {StartIndex1}", pageData, startIndex1);
                if (startIndex1 >= str1.Length)
                {
                    var num1 = pageData.IndexOf("\"", startIndex1, StringComparison.Ordinal);
                    return pageData.Substring(startIndex1, num1 - startIndex1);
                }
            }

			Logger.LogDebug("<<< Exiting GetVerificationToken(pageData={PageData})", pageData);
            return string.Empty;
        }

        private static string GetCurrentTabId(string pageData)
        {
			Logger.LogDebug(">>> Entering GetCurrentTabId(pageData={PageData})", pageData);
            if (!string.IsNullOrEmpty(pageData))
            {
                var match = Regex.Match(pageData, "`sf_tabId`:`(\\d+)`");
                if (match.Success)
				Logger.LogDebug("GetCurrentTabId(pageData={PageData}): match → {Match}", pageData, match);
                {
                    return match.Groups[1].Value;
                }
            }

			Logger.LogDebug("<<< Exiting GetCurrentTabId(pageData={PageData})", pageData);
            return string.Empty;
        }

        private static byte[] GetMultipartFormData(IEnumerable<KeyValuePair<string, object>> postParameters, string boundary)
        {
            Stream formDataStream = new MemoryStream();
            var needsClrf = false;

            foreach (var param in postParameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsClrf)
                {
                    formDataStream.Write(Encoding.GetBytes("\r\n"), 0, Encoding.GetByteCount("\r\n"));
                }

                needsClrf = true;

                var value = param.Value as FileParameter;
                if (value != null)
                {
                    var fileToUpload = value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    var header = string.Format(
                        "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(Encoding.GetBytes(header), 0, Encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    var postData = string.Format(
                        "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(Encoding.GetBytes(postData), 0, Encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            var footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(Encoding.GetBytes(footer), 0, Encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            var formData = new byte[formDataStream.Length];
            var len = formDataStream.Read(formData, 0, formData.Length);
            if (len != formDataStream.Length)
            {
                Console.WriteLine(
                    @"ERROR: not all form data was read from the stream. " +
                    @"Requested to read {0} bytes, but was read {1} bytes", formDataStream.Length, len);
            }

            formDataStream.Close();
            return formData;
        }

        private static string CombineUrlPath(Uri domain, string path)
        {
			Logger.LogDebug(">>> Entering CombineUrlPath(domain={Domain},path={Path})", domain, path);
            if (path.StartsWith("http"))
            {
                return path;
            }

            var url = domain.AbsoluteUri;
            if (!url.EndsWith("/", StringComparison.Ordinal))
            {
                url += "/";
            }

            if (string.IsNullOrEmpty(path))
            {
                path = string.Empty;
            }

            if (path.StartsWith("/", StringComparison.Ordinal))
            {
                return url + path.Substring(1);
            }

			Logger.LogDebug("<<< Exiting CombineUrlPath(domain={Domain},path={Path})", domain, path);
            return new Uri(url + path).AbsoluteUri;
        }

        private static string QueryStringFromObject(object query)
        {
			Logger.LogDebug(">>> Entering QueryStringFromObject(query={Query})");
            var properties = from p in query.GetType().GetProperties()
                             where p.GetValue(query, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(query, null).ToString());

			Logger.LogDebug("<<< Exiting QueryStringFromObject(query={Query})");
            return string.Join("&", properties.ToArray());
        }

        private static HttpResponseMessage EnsureSuccessResponse(HttpResponseMessage result, string source, string url)
        {
            var body = string.Empty;

            try
            {
                body = result.Content.ReadAsStringAsync().Result;
                return result.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(@"{0} failed for [{1}] {2}\n{3}", source, url, ex.Message, body);
                throw new WebApiException(ex, result, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"{0} failed for [{1}] {2}", source, url, ex.Message);
                throw;
            }
        }

        private void ResetUserId()
        {
			Logger.LogDebug(">>> Entering ResetUserId()");
            this._userId = -1;
			Logger.LogDebug("<<< Exiting ResetUserId()");
			Logger.LogDebug("ResetUserId(): _userId → {_userId}", _userId);
			Logger.LogDebug("ResetUserId(): this._userId → {Arg0}", this._userId);
        }

        private void EnsureLoggedIn()
        {
			Logger.LogDebug(">>> Entering EnsureLoggedIn()");
            if (!this.IsLoggedIn)
            {
                Console.WriteLine(@"User not logged in yet");
                throw new WebApiException(
                    new HttpRequestException("User not logged in yet."),
                    new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
			Logger.LogDebug("<<< Exiting EnsureLoggedIn()");
        }

        private void VerifyLogInCookie(HttpWebResponse httpResponse)
        {
			Logger.LogDebug(">>> Entering VerifyLogInCookie(httpResponse={HttpResponse})", httpResponse);
            var cookie = AppConfigHelper.LoginCookie;
            var loginCookie = httpResponse.Cookies[cookie];
            if (loginCookie != null)
            {
                this.IsLoggedIn = true;
				Logger.LogDebug("VerifyLogInCookie(httpResponse={HttpResponse}): this.IsLoggedIn → {Arg1}", httpResponse, this.IsLoggedIn);
                this.ExtractVerificationCookie(httpResponse.Headers["Set-Cookie"] ?? string.Empty);
                using (var rs = httpResponse.GetResponseStream())
                {
                    if (rs != null && httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (var sr = new StreamReader(rs, Encoding.UTF8))
                        {
                            var data = sr.ReadToEnd();
                            var token = GetVerificationToken(data);
                            if (!string.IsNullOrEmpty(token))
                            {
                                this._inputFieldVerificationToken = token;
                            }
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting VerifyLogInCookie(httpResponse={HttpResponse})", httpResponse);
        }

        private void ExtractVerificationCookie(string cookiesString)
        {
			Logger.LogDebug(">>> Entering ExtractVerificationCookie(cookiesString={CookiesString})", cookiesString);
            var parts1 = cookiesString.Split(',');
            foreach (var part1 in parts1)
            {
			Logger.LogDebug("ExtractVerificationCookie(cookiesString={CookiesString}): parts1 → {Parts1}", cookiesString, parts1);
                if (part1.Contains(RqVerifTokenName))
                {
                    var parts2 = part1.Split(';');
                    foreach (var part2 in parts2)
                    {
					Logger.LogDebug("ExtractVerificationCookie(cookiesString={CookiesString}): parts2 → {Parts2}", cookiesString, parts2);
                        if (part2.Contains(RqVerifTokenName))
                        {
                            this._cookieVerificationToken.Value = part2.Split('=')[1];
                        }
                        else if (part2.Contains("path"))
                        {
                            this._cookieVerificationToken.Path = part2.Split('=')[1];
                        }
                        else if (part2.Contains("HttpOnly"))
                        {
                            this._cookieVerificationToken.HttpOnly = true;
                        }
                    }

                    break;
                }
            }
			Logger.LogDebug("<<< Exiting ExtractVerificationCookie(cookiesString={CookiesString})", cookiesString);
        }

        private async Task<HttpResponseMessage> UploadFile(string fileName, string portalFolder, bool waitHttpResponse = true)
        {
			Logger.LogDebug(">>> Entering UploadFile(fileName={FileName},portalFolder={PortalFolder},waitHttpResponse={WaitHttpResponse})", fileName, portalFolder, waitHttpResponse);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            using (var client = this.CreateHttpClient("/", true))
            {
                var headers = client.DefaultRequestHeaders;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html", 0.8));
                headers.UserAgent.ParseAdd(this.UserAgentValue);

                if (string.IsNullOrEmpty(this._inputFieldVerificationToken))
                {
                    var resultGet = await client.GetAsync("/");
                    var data = await resultGet.Content.ReadAsStringAsync();
					Logger.LogDebug("UploadFile(fileName={FileName},portalFolder={PortalFolder},waitHttpResponse={WaitHttpResponse}): resultGet → {ResultGet}", fileName, portalFolder, waitHttpResponse, resultGet);
					Logger.LogDebug("UploadFile(fileName={FileName},portalFolder={PortalFolder},waitHttpResponse={WaitHttpResponse}): data → {Data}", fileName, portalFolder, waitHttpResponse, data);
                    this._inputFieldVerificationToken = GetVerificationToken(data);

                    if (!string.IsNullOrEmpty(this._inputFieldVerificationToken))
                    {
                        client.DefaultRequestHeaders.Add(RqVerifTokenNameNoUndescrores, this._inputFieldVerificationToken);
                    }
                    else
                    {
                        Console.WriteLine(@"Cannot find '{0}' in the page input fields (A)", RqVerifTokenName);
                    }
                }

                var content = new MultipartFormDataContent();
                var values = new[]
				Logger.LogDebug("UploadFile(fileName={FileName},portalFolder={PortalFolder},waitHttpResponse={WaitHttpResponse}): content → {Content}", fileName, portalFolder, waitHttpResponse, content);
                {
                    new KeyValuePair<string, string>("\"folder\"", portalFolder),
                    new KeyValuePair<string, string>("\"filter\"", FileFilters),
                    new KeyValuePair<string, string>("\"overwrite\"", "true"),
                };

                foreach (var keyValuePair in values)
                {
                    content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                }

                var fi = new FileInfo(fileName);
                var fileContent = new ByteArrayContent(File.ReadAllBytes(fileName));
				Logger.LogDebug("UploadFile(fileName={FileName},portalFolder={PortalFolder},waitHttpResponse={WaitHttpResponse}): fi → {Fi}", fileName, portalFolder, waitHttpResponse, fi);
				Logger.LogDebug("UploadFile(fileName={FileName},portalFolder={PortalFolder},waitHttpResponse={WaitHttpResponse}): fileContent → {FileContent}", fileName, portalFolder, waitHttpResponse, fileContent);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fi.Name,
                    Name = "\"postfile\"",
                };

                content.Add(fileContent);

                client.DefaultRequestHeaders.UserAgent.ParseAdd(this.UserAgentValue);
                var result = await client.PostAsync(UploadFileRequestPath, content);
				Logger.LogDebug("UploadFile(fileName={FileName},portalFolder={PortalFolder},waitHttpResponse={WaitHttpResponse}): result → {Result}", fileName, portalFolder, waitHttpResponse, result);
				Logger.LogInformation("UploadFile(fileName,portalFolder,waitHttpResponse)={FileName},{PortalFolder},{WaitHttpResponse}: external service call took {Arg3} ms", fileName, portalFolder, waitHttpResponse, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting UploadFile(fileName={FileName},portalFolder={PortalFolder},waitHttpResponse={WaitHttpResponse})", fileName, portalFolder, waitHttpResponse);
                return !waitHttpResponse
                    ? result
                    : EnsureSuccessResponse(result, "UploadFile", UploadFileRequestPath);
            }
        }

        private HttpResponseMessage ActivityStreamUploadFile(
            IDictionary<string, string> headers, string fileName, bool waitHttpResponse = false)
        {
            using (var clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
            })
            using (var client = new HttpClient(clientHandler))
            {
                clientHandler.CookieContainer = this._sessionCookiesContainer;
                client.BaseAddress = this.Domain;
                client.Timeout = this.Timeout;

                var rqHeaders = client.DefaultRequestHeaders;
                rqHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                rqHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html", 0.8));
                rqHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/png"));
                rqHeaders.UserAgent.ParseAdd(this.UserAgentValue);

                var resultGet = client.GetAsync("/").Result;
                var data = resultGet.Content.ReadAsStringAsync().Result;
                var requestVerificationToken = GetVerificationToken(data);
                if (!string.IsNullOrEmpty(requestVerificationToken))
                {
                    client.DefaultRequestHeaders.Add(RqVerifTokenNameNoUndescrores, requestVerificationToken);
                }
                else
                {
                    Console.WriteLine(@"Cannot find '{0}' in the page input fields (B)", RqVerifTokenName);
                }

                var content = new MultipartFormDataContent();
                var fi = new FileInfo(fileName);
                var fileContent = new ByteArrayContent(File.ReadAllBytes(fileName));
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"files[]\"",
                    FileName = "\"" + fi.Name + "\"",
                };
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");

                content.Add(fileContent);
                rqHeaders.Add("TabID", headers["TabID"]);
                rqHeaders.Add("ModuleID", headers["ModuleID"]);
                var result = client.PostAsync(ActivityStreamUploadFilePath, content).Result;
                return !waitHttpResponse
                    ? result
                    : EnsureSuccessResponse(result, "ActivityStreamUploadFile", ActivityStreamUploadFilePath);
            }
        }

        private HttpClient CreateHttpClient(string path, bool autoRedirect)
        {
            var clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = autoRedirect,
                CookieContainer = this._sessionCookiesContainer,
            };

            var url = CombineUrlPath(this.Domain, path);
            var client = new HttpClient(clientHandler)
            {
                BaseAddress = this.Domain,
                Timeout = this.Timeout,
            };

            if (string.IsNullOrEmpty(this._inputFieldVerificationToken))
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(this.UserAgentValue);
                var resultGet = client.GetAsync(url).Result;
                var data = resultGet.Content.ReadAsStringAsync().Result;
                this._inputFieldVerificationToken = GetVerificationToken(data);
                client.DefaultRequestHeaders.Add(RqVerifTokenNameNoUndescrores, this._inputFieldVerificationToken);

                this._currentTabId = GetCurrentTabId(data);
                client.DefaultRequestHeaders.Add("TabId", this._currentTabId);
            }
            else
            {
                client.DefaultRequestHeaders.Add("TabId", this._currentTabId);
                client.DefaultRequestHeaders.Add(RqVerifTokenNameNoUndescrores, this._inputFieldVerificationToken);
            }

            return client;
        }

        private string[] GetPageInputFields(HttpClient client, string path)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            CachedWebPage cachedPage = null;
            var url = CombineUrlPath(this.Domain, path);
            if (!this.IsLoggedIn || this.AvoidCaching ||
                (!CachedPages.TryGetValue(url, out cachedPage) ||
                 cachedPage.FetchDateTime < DateTime.Now.AddMinutes(-19.5)))
            {
                try
                {
                    var requestVerificationToken = string.Empty;
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(this.UserAgentValue);
                    var resultGet = client.GetAsync(url).Result;
                    var data = resultGet.Content.ReadAsStringAsync().Result;
                    const string str1 = "<input name=\"" + RqVerifTokenName + "\" type=\"hidden\" value=\"";
                    var startIndex1 = data.IndexOf(str1, StringComparison.Ordinal);
                    if (startIndex1 >= 0)
                    {
                        startIndex1 += str1.Length;
                        var num1 = data.IndexOf("\"", startIndex1, StringComparison.Ordinal);
                        requestVerificationToken = data.Substring(startIndex1, num1 - startIndex1);
                        client.DefaultRequestHeaders.Add(RqVerifTokenNameNoUndescrores, requestVerificationToken);
                    }
                    else
                    {
                        Console.WriteLine(@"Cannot find '{0}' in the page input fields. Data: {1}", RqVerifTokenName, resultGet);
                    }

                    var inputFields = HtmlFormInuts.Matches(data).Cast<Match>().Select(match => match.Groups[0].Value).ToArray();
                    cachedPage = new CachedWebPage(requestVerificationToken, inputFields);
                    CachedPages[url] = cachedPage;
                    return inputFields;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"Error accessing this path {0}.{1}{2}", path, Environment.NewLine, ex);
                }
            }
            else
            {
                client.DefaultRequestHeaders.Add(RqVerifTokenNameNoUndescrores, cachedPage.VerificationToken);
            }

            return cachedPage != null ? cachedPage.InputFields : [];
        }

        private HttpWebResponse MultipartFormDataPost(
            string postUrl, string userAgent, IDictionary<string, object> postParameters, IDictionary<string, string> headers = null, bool followRedirect = false)
        {
            var formDataBoundary = string.Format("----WebKitFormBoundary{0:X16}", DateTime.Now.Ticks);
            var contentType = "multipart/form-data; boundary=" + formDataBoundary;
            var formData = GetMultipartFormData(postParameters, formDataBoundary);
            return this.PostForm(postUrl, userAgent, contentType, headers, formData, followRedirect);
        }

        private HttpWebResponse PostForm(string postUrl, string userAgent, string contentType, IDictionary<string, string> headers, byte[] formData, bool followRedirect)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var request = WebRequest.Create(postUrl) as HttpWebRequest;

            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = this._sessionCookiesContainer;
            request.ContentLength = formData.Length;
            request.Headers.Add(RqVerifTokenNameNoUndescrores, this._inputFieldVerificationToken);
            if (headers != null)
            {
                foreach (var h in headers)
                {
                    request.Headers.Add(h.Key, h.Value);
                }
            }

            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.KeepAlive = true;
            request.ReadWriteTimeout = 90;
            request.AllowAutoRedirect = followRedirect;
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

            // You could add authentication here as well if needed:
            // request.PreAuthenticate = true;
            // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            // request.Headers.Add("Authorization", "Basic " +
            //     Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

            // Send the form data to the request.
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                return request.GetResponse() as HttpWebResponse;
            }
        }
    }
}
