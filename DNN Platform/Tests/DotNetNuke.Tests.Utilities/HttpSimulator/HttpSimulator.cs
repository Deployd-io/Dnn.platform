// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Instance.Utilities.HttpSimulator
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.SessionState;
	using Microsoft.Extensions.Logging;

    public enum HttpVerb
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        GET = 0,
        HEAD = 1,
        POST = 2,
        PUT = 3,
        DELETE = 4,
    }

    /// <summary>
    /// Useful class for simulating the HttpContext. This does not actually
    /// make an HttpRequest, it merely simulates the state that your code
    /// would be in "as if" handling a request. Thus the HttpContext.Current
    /// property is populated.
    /// </summary>
    public class HttpSimulator : IDisposable
    {
        private static readonly string WebsitePhysicalAppPath = ConfigurationManager.AppSettings["DefaultPhysicalAppPath"];
        private readonly NameValueCollection _formVars = new NameValueCollection();
        private readonly NameValueCollection _headers = new NameValueCollection();
        private StringBuilder _builder;
        private Uri _referer;

        private string _applicationPath = "/";

        private string _physicalApplicationPath = WebsitePhysicalAppPath;

        private string _physicalPath = WebsitePhysicalAppPath;

        public HttpSimulator()
            : this("/", WebsitePhysicalAppPath)
        {
		_logger.LogDebug(">>> Entering HttpSimulator()");
        }

		_logger.LogDebug("<<< Exiting HttpSimulator()");
        public HttpSimulator(string applicationPath)
            : this(applicationPath, WebsitePhysicalAppPath)
        {
		_logger.LogDebug(">>> Entering HttpSimulator(applicationPath={ApplicationPath})", applicationPath);
		_logger.LogDebug("<<< Exiting HttpSimulator(applicationPath={ApplicationPath})", applicationPath);
        }

        public HttpSimulator(string applicationPath, string physicalApplicationPath)
        {
            this.ApplicationPath = applicationPath;
            this.PhysicalApplicationPath = physicalApplicationPath;
			_logger.LogDebug("HttpSimulator(applicationPath={ApplicationPath},physicalApplicationPath={PhysicalApplicationPath}): this.ApplicationPath → {Arg2}", applicationPath, physicalApplicationPath, this.ApplicationPath);
        }

        /// <summary>Gets physical path to the requested file (used for simulation purposes).</summary>
        public string PhysicalPath
        {
            get { return this._physicalPath; }
        }

        /// <summary>Gets the text from the response to the simulated request.</summary>
        public string ResponseText
        {
            get
            {
                return (this._builder ?? new StringBuilder()).ToString();
            }
        }

        public string Host { get; private set; }

        public string LocalPath { get; private set; }

        public int Port { get; private set; }

        /// <summary>Gets portion of the URL after the application.</summary>
        public string Page { get; private set; }

        /// <summary>
        /// Gets or sets the same thing as the IIS Virtual directory. It's
        /// what gets returned by Request.ApplicationPath.
        /// </summary>
        public string ApplicationPath
        {
            get { return this._applicationPath; }

            set
            {
                this._applicationPath = value ?? "/";
                this._applicationPath = NormalizeSlashes(this._applicationPath);
            }
        }

        /// <summary>Gets or sets physical path to the application (used for simulation purposes).</summary>
        public string PhysicalApplicationPath
        {
            get { return this._physicalApplicationPath; }

            set
            {
                this._physicalApplicationPath = value ?? WebsitePhysicalAppPath;

                // strip trailing backslashes.
                this._physicalApplicationPath = StripTrailingBackSlashes(this._physicalApplicationPath) + @"\";
            }
        }

        public TextWriter ResponseWriter { get; set; }

        public SimulatedHttpRequest WorkerRequest { get; private set; }

        /// <summary>Sets up the HttpContext objects to simulate a GET request.</summary>
        /// <remarks>
        /// Simulates a request to http://localhost/.
        /// </remarks>
        /// <returns></returns>
        public HttpSimulator SimulateRequest()
        {
			_logger.LogDebug(">>> Entering SimulateRequest()");
			_logger.LogDebug("<<< Exiting SimulateRequest()");
			_logger.LogDebug("<<< Exiting SimulateRequest(url={Url})", url);
			_logger.LogDebug("<<< Exiting SimulateRequest(url={Url},httpVerb={HttpVerb})", url, httpVerb);
			_logger.LogDebug("<<< Exiting SimulateRequest(url={Url},formVariables={FormVariables})", url, formVariables);
			_logger.LogDebug("<<< Exiting SimulateRequest(url={Url},formVariables={FormVariables},headers={Headers})", url, formVariables, headers);
			_logger.LogDebug("<<< Exiting SimulateRequest(url={Url},httpVerb={HttpVerb},headers={Headers})", url, httpVerb, headers);
			_logger.LogDebug("<<< Exiting SimulateRequest(url={Url},httpVerb={HttpVerb},formVariables={FormVariables},headers={Headers})", url, httpVerb, formVariables, headers);
            return this.SimulateRequest(new Uri("http://localhost/"));
        }

        /// <summary>Sets up the HttpContext objects to simulate a GET request.</summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url)
        {
            return this.SimulateRequest(url, HttpVerb.GET);
        }

        /// <summary>Sets up the HttpContext objects to simulate a request.</summary>
        /// <param name="url"></param>
        /// <param name="httpVerb"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url, HttpVerb httpVerb)
        {
            return this.SimulateRequest(url, httpVerb, null, null);
        }

        /// <summary>Sets up the HttpContext objects to simulate a POST request.</summary>
        /// <param name="url"></param>
        /// <param name="formVariables"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url, NameValueCollection formVariables)
        {
            return this.SimulateRequest(url, HttpVerb.POST, formVariables, null);
        }

        /// <summary>Sets up the HttpContext objects to simulate a POST request.</summary>
        /// <param name="url"></param>
        /// <param name="formVariables"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url, NameValueCollection formVariables, NameValueCollection headers)
        {
            return this.SimulateRequest(url, HttpVerb.POST, formVariables, headers);
        }

        /// <summary>Sets up the HttpContext objects to simulate a request.</summary>
        /// <param name="url"></param>
        /// <param name="httpVerb"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public HttpSimulator SimulateRequest(Uri url, HttpVerb httpVerb, NameValueCollection headers)
        {
            return this.SimulateRequest(url, httpVerb, null, headers);
        }

        /// <summary>Sets the referer for the request. Uses a fluent interface.</summary>
        /// <param name="referer"></param>
        /// <returns></returns>
        public HttpSimulator SetReferer(Uri referer)
        {
			_logger.LogDebug(">>> Entering SetReferer(referer={Referer})", referer);
            if (this.WorkerRequest != null)
            {
                this.WorkerRequest.SetReferer(referer);
            }

            this._referer = referer;
			_logger.LogDebug("SetReferer(referer={Referer}): _referer → {_referer}", referer, _referer);
			_logger.LogDebug("SetReferer(referer={Referer}): this._referer → {Arg1}", referer, this._referer);
			_logger.LogDebug("<<< Exiting SetReferer(referer={Referer})", referer);
            return this;
        }

        /// <summary>Sets a form variable.</summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpSimulator SetFormVariable(string name, string value)
        {
			_logger.LogDebug(">>> Entering SetFormVariable(name={Name},value={Value})", name, value);
            if (this.WorkerRequest != null)
            {
				_logger.LogWarning("Throwing InvalidOperationException from SetFormVariable(name={Name},value={Value})", name, value);
                throw new InvalidOperationException("Cannot set form variables after calling Simulate().");
            }

            this._formVars.Add(name, value);

			_logger.LogDebug("<<< Exiting SetFormVariable(name={Name},value={Value})", name, value);
            return this;
        }

        /// <summary>Sets a header value.</summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public HttpSimulator SetHeader(string name, string value)
        {
			_logger.LogDebug(">>> Entering SetHeader(name={Name},value={Value})", name, value);
            if (this.WorkerRequest != null)
            {
				_logger.LogWarning("Throwing InvalidOperationException from SetHeader(name={Name},value={Value})", name, value);
                throw new InvalidOperationException("Cannot set headers after calling Simulate().");
            }

            this._headers.Add(name, value);

			_logger.LogDebug("<<< Exiting SetHeader(name={Name},value={Value})", name, value);
            return this;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2.</filterpriority>
        public void Dispose()
        {
			_logger.LogDebug(">>> Entering Dispose()");
			_logger.LogDebug("Dispose(): HttpContext.Current → {Arg0}", HttpContext.Current);
            HttpContext.Current = null;
			_logger.LogDebug("<<< Exiting Dispose()");
        }

        protected static HostingEnvironment GetHostingEnvironment()
        {
			_logger.LogDebug(">>> Entering GetHostingEnvironment()");
            HostingEnvironment environment;
            try
            {
                environment = new HostingEnvironment();
            }
            catch (InvalidOperationException)
            {
				_logger.LogError("Exception in GetHostingEnvironment(): {Arg0}", InvalidOperationException.getMessage());
                // Shoot, we need to grab it via reflection.
                environment = ReflectionHelper.GetStaticFieldValue<HostingEnvironment>("_theHostingEnvironment", typeof(HostingEnvironment));
            }

			_logger.LogDebug("<<< Exiting GetHostingEnvironment()");
            return environment;
        }

        protected static string NormalizeSlashes(string s)
        {
			_logger.LogDebug(">>> Entering NormalizeSlashes(s={S})", s);
            if (string.IsNullOrEmpty(s) || s == "/")
            {
                return "/";
            }

            s = s.Replace(@"\", "/");

            // Reduce multiple slashes in row to single.
            var normalized = Regex.Replace(s, "(/)/+", "$1");

            // Strip left.
            normalized = StripPrecedingSlashes(normalized);

            // Strip right.
            normalized = StripTrailingSlashes(normalized);
			_logger.LogDebug("NormalizeSlashes(s={S}): normalized → {Normalized}", s, normalized);
			_logger.LogDebug("NormalizeSlashes(s={S}): s → {S1}", s, s);
			_logger.LogDebug("<<< Exiting NormalizeSlashes(s={S})", s);
            return "/" + normalized;
        }

        protected static string StripPrecedingSlashes(string s)
        {
			_logger.LogDebug(">>> Entering StripPrecedingSlashes(s={S})", s);
			_logger.LogDebug("<<< Exiting StripPrecedingSlashes(s={S})", s);
            return Regex.Replace(s, "^/*(.*)", "$1");
        }

        protected static string StripTrailingSlashes(string s)
        {
			_logger.LogDebug(">>> Entering StripTrailingSlashes(s={S})", s);
			_logger.LogDebug("<<< Exiting StripTrailingSlashes(s={S})", s);
            return Regex.Replace(s, "(.*)/*$", "$1", RegexOptions.RightToLeft);
        }

        protected static string StripTrailingBackSlashes(string s)
        {
			_logger.LogDebug(">>> Entering StripTrailingBackSlashes(s={S})", s);
			_logger.LogDebug("<<< Exiting StripTrailingBackSlashes(s={S})", s);
            return string.IsNullOrEmpty(s) ? string.Empty : Regex.Replace(s, @"(.*)\\*$", "$1", RegexOptions.RightToLeft);
        }

        /// <summary>Sets up the HttpContext objects to simulate a request.</summary>
        /// <param name="url"></param>
        /// <param name="httpVerb"></param>
        /// <param name="formVariables"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected virtual HttpSimulator SimulateRequest(Uri url, HttpVerb httpVerb, NameValueCollection formVariables, NameValueCollection headers)
        {
            HttpContext.Current = null;

            this.ParseRequestUrl(url);

            if (this.ResponseWriter == null)
            {
                this._builder = new StringBuilder();
                this.ResponseWriter = new StringWriter(this._builder);
            }

            this.SetHttpRuntimeInternals();

            var query = ExtractQueryStringPart(url);

            if (formVariables != null)
            {
                this._formVars.Add(formVariables);
            }

            if (this._formVars.Count > 0)
            {
                httpVerb = HttpVerb.POST; // Need to enforce this.
            }

            if (headers != null)
            {
                this._headers.Add(headers);
            }

            this.WorkerRequest = new SimulatedHttpRequest(this.ApplicationPath, this.PhysicalApplicationPath, this.PhysicalPath, this.Page, query, this.ResponseWriter, this.Host, this.Port, httpVerb.ToString());

            this.WorkerRequest.Form.Add(this._formVars);
            this.WorkerRequest.Headers.Add(this._headers);

            if (this._referer != null)
            {
                this.WorkerRequest.SetReferer(this._referer);
            }

            this.InitializeSession();

            InitializeApplication();

            // Console.WriteLine("host: " + Host);
            // Console.WriteLine("virtualDir: " + _applicationPath);
            // Console.WriteLine("page: " + LocalPath);
            // Console.WriteLine("pathPartAfterApplicationPart: " + Page);
            // Console.WriteLine("appPhysicalDir: " + _physicalApplicationPath);
            // if (HttpContext.Current != null)
            // {
            //    Console.WriteLine("Request.Url.LocalPath: " + HttpContext.Current.Request.Url.LocalPath);
            //    Console.WriteLine("Request.Url.Host: " + HttpContext.Current.Request.Url.Host);
            //    Console.WriteLine("Request.FilePath: " + HttpContext.Current.Request.FilePath);
            //    Console.WriteLine("Request.Path: " + HttpContext.Current.Request.Path);
            //    Console.WriteLine("Request.RawUrl: " + HttpContext.Current.Request.RawUrl);
            //    Console.WriteLine("Request.Url: " + HttpContext.Current.Request.Url);
            //    Console.WriteLine("Request.Url.Port: " + HttpContext.Current.Request.Url.Port);
            //    Console.WriteLine("Request.ApplicationPath: " + HttpContext.Current.Request.ApplicationPath);
            //    Console.WriteLine("Request.PhysicalPath: " + HttpContext.Current.Request.PhysicalPath);
            // }
            // Console.WriteLine("HttpRuntime.AppDomainAppPath: " + HttpRuntime.AppDomainAppPath);
            // Console.WriteLine("HttpRuntime.AppDomainAppVirtualPath: " + HttpRuntime.AppDomainAppVirtualPath);
            // Console.WriteLine("HostingEnvironment.ApplicationPhysicalPath: " + HostingEnvironment.ApplicationPhysicalPath);
            // Console.WriteLine("HostingEnvironment.ApplicationVirtualPath: " + HostingEnvironment.ApplicationVirtualPath);
            return this;
        }

        private static void InitializeApplication()
        {
			_logger.LogDebug(">>> Entering InitializeApplication()");
            var appFactoryType = Type.GetType("System.Web.HttpApplicationFactory, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            var appFactory = ReflectionHelper.GetStaticFieldValue<object>("_theApplicationFactory", appFactoryType);
			_logger.LogDebug("InitializeApplication(): appFactory → {AppFactory}", appFactory);
			_logger.LogDebug("InitializeApplication(): appFactoryType → {AppFactoryType}", appFactoryType);
            ReflectionHelper.SetPrivateInstanceFieldValue("_state", appFactory, HttpContext.Current.Application);
			_logger.LogDebug("<<< Exiting InitializeApplication()");
        }

        private static string RightAfter(string original, string search)
        {
			_logger.LogDebug(">>> Entering RightAfter(original={Original},search={Search})", original, search);
            if (search.Length > original.Length || search.Length == 0)
            {
                return original;
            }

            original = original.Trim();
			_logger.LogDebug("RightAfter(original={Original},search={Search}): original → {Original1}", original, search, original);
            search = search.Trim();
			_logger.LogDebug("RightAfter(original={Original},search={Search}): search → {Search1}", original, search, search);
            var searchIndex = original.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase);
			_logger.LogDebug("RightAfter(original={Original},search={Search}): searchIndex → {SearchIndex}", original, search, searchIndex);

            if (searchIndex < 0)
            {
                return original;
            }

            // mod to add one onto the search length - otherwise strange results??
            // return original.Substring(original.IndexOf(search) + search.Length + 1);
            // return original.Substring(original.IndexOf(search) + search.Length);
            // original = original.Substring(searchIndex, original.Length - searchIndex);
            // return original.Replace(search,"");
            var regexMatch = @"(" + search + ")(?<keep>.+)";
			_logger.LogDebug("RightAfter(original={Original},search={Search}): regexMatch → {RegexMatch}", original, search, regexMatch);
            const string regexReplace = @"${keep}";
            var result = Regex.Replace(original, regexMatch, regexReplace, RegexOptions.IgnoreCase);
			_logger.LogDebug("RightAfter(original={Original},search={Search}): result → {Result}", original, search, result);
			_logger.LogDebug("<<< Exiting RightAfter(original={Original},search={Search})", original, search);
            return result;
        }

        private static string ExtractQueryStringPart(Uri url)
        {
			_logger.LogDebug(">>> Entering ExtractQueryStringPart(url={Url})", url);
            var query = url.Query;
			_logger.LogDebug("<<< Exiting ExtractQueryStringPart(url={Url})", url);
            return query.StartsWith("?", StringComparison.Ordinal) ? query.Substring(1) : query;
        }

        private void InitializeSession()
        {
			_logger.LogDebug(">>> Entering InitializeSession()");
            HttpContext.Current = new HttpContext(this.WorkerRequest);
            HttpContext.Current.Items.Clear();
            var session = (HttpSessionState)ReflectionHelper.Instantiate(typeof(HttpSessionState), new[] { typeof(IHttpSessionState) }, new FakeHttpSessionState());
			_logger.LogDebug("InitializeSession(): session → {Session}", session);

            HttpContext.Current.Items.Add("AspSession", session);
			_logger.LogDebug("<<< Exiting InitializeSession()");
        }

        private void ParseRequestUrl(Uri url)
        {
			_logger.LogDebug(">>> Entering ParseRequestUrl(url={Url})", url);
            if (url == null)
            {
                return;
            }

            this.Host = url.Host;
			_logger.LogDebug("ParseRequestUrl(url={Url}): this.Host → {Arg1}", url, this.Host);
            this.Port = url.Port;
            this.LocalPath = url.LocalPath;
			_logger.LogDebug("ParseRequestUrl(url={Url}): this.Port → {Arg1}", url, this.Port);
			_logger.LogDebug("ParseRequestUrl(url={Url}): this.LocalPath → {Arg1}", url, this.LocalPath);
            this.Page = StripPrecedingSlashes(RightAfter(url.LocalPath, this.ApplicationPath));
            this._physicalPath = Path.Combine(this._physicalApplicationPath, this.Page.Replace("/", @"\"));
        }

        private void SetHttpRuntimeInternals()
        {
			_logger.LogDebug(">>> Entering SetHttpRuntimeInternals()");
            // We cheat by using reflection.

            // get singleton property value
            var runtime = ReflectionHelper.GetStaticFieldValue<HttpRuntime>("_theRuntime", typeof(HttpRuntime));
			_logger.LogDebug("SetHttpRuntimeInternals(): runtime → {Runtime}", runtime);

            // set app path property value
            ReflectionHelper.SetPrivateInstanceFieldValue("_appDomainAppPath", runtime, this.PhysicalApplicationPath);

            // set app virtual path property value
            const string vpathTypeName = "System.Web.VirtualPath, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            var virtualPath = ReflectionHelper.Instantiate(vpathTypeName, new[] { typeof(string) }, new object[] { this.ApplicationPath });
			_logger.LogDebug("SetHttpRuntimeInternals(): virtualPath → {VirtualPath}", virtualPath);
            ReflectionHelper.SetPrivateInstanceFieldValue("_appDomainAppVPath", runtime, virtualPath);

            // set codegen dir property value
            ReflectionHelper.SetPrivateInstanceFieldValue("_codegenDir", runtime, this.PhysicalApplicationPath);

            var environment = GetHostingEnvironment();
			_logger.LogDebug("SetHttpRuntimeInternals(): environment → {Environment}", environment);
            ReflectionHelper.SetPrivateInstanceFieldValue("_appPhysicalPath", environment, this.PhysicalApplicationPath);
            ReflectionHelper.SetPrivateInstanceFieldValue("_appVirtualPath", environment, virtualPath);
            ReflectionHelper.SetPrivateInstanceFieldValue("_configMapPath", environment, new ConfigMapPath(this));
			_logger.LogDebug("<<< Exiting SetHttpRuntimeInternals()");
        }

        public class FakeHttpSessionState : NameObjectCollectionBase, IHttpSessionState
        {
            private const bool _isNewSession = true;
            private readonly string _sessionId = Guid.NewGuid().ToString();
            private readonly HttpStaticObjectsCollection _staticObjects = new HttpStaticObjectsCollection();
            private readonly object _syncRoot = new object();
            private int _timeout = 30; // minutes

            /// <summary>Gets the unique session identifier for the session.</summary>
            ///
            /// <returns>
            /// The session ID.
            /// </returns>
            ///
            public string SessionID
            {
                get { return this._sessionId; }
            }

            /// <summary>Gets a value indicating whether the session was created with the current request.</summary>
            ///
            /// <returns>
            /// true if the session was created with the current request; otherwise, false.
            /// </returns>
            ///
            public bool IsNewSession
            {
                get { return _isNewSession; }
            }

            /// <summary>Gets the current session-state mode.</summary>
            ///
            /// <returns>
            /// One of the <see cref="T:System.Web.SessionState.SessionStateMode"></see> values.
            /// </returns>
            ///
            public SessionStateMode Mode
            {
                get { return SessionStateMode.InProc; }
            }

            /// <summary>Gets a value indicating whether the session ID is embedded in the URL or stored in an HTTP cookie.</summary>
            ///
            /// <returns>
            /// true if the session is embedded in the URL; otherwise, false.
            /// </returns>
            ///
            public bool IsCookieless
            {
                get { return false; }
            }

            /// <summary>Gets a value that indicates whether the application is configured for cookieless sessions.</summary>
            ///
            /// <returns>
            /// One of the <see cref="T:System.Web.HttpCookieMode"></see> values that indicate whether the application is configured for cookieless sessions. The default is <see cref="F:System.Web.HttpCookieMode.UseCookies"></see>.
            /// </returns>
            ///
            public HttpCookieMode CookieMode
            {
                get { return HttpCookieMode.UseCookies; }
            }

            /// <summary>Gets a collection of objects declared by &lt;object Runat="Server" Scope="Session"/&gt; tags within the ASP.NET application file Global.asax.</summary>
            ///
            /// <returns>
            /// An <see cref="T:System.Web.HttpStaticObjectsCollection"></see> containing objects declared in the Global.asax file.
            /// </returns>
            ///
            public HttpStaticObjectsCollection StaticObjects
            {
                get { return this._staticObjects; }
            }

            /// <summary>Gets an object that can be used to synchronize access to the collection of session-state values.</summary>
            ///
            /// <returns>
            /// An object that can be used to synchronize access to the collection.
            /// </returns>
            ///
            public object SyncRoot
            {
                get { return this._syncRoot; }
            }

            /// <summary>Gets a value indicating whether access to the collection of session-state values is synchronized (thread safe).</summary>
            /// <returns>
            /// true if access to the collection is synchronized (thread safe); otherwise, false.
            /// </returns>
            ///
            public bool IsSynchronized
            {
                get { return true; }
            }

            /// <summary>Gets or sets the time-out period (in minutes) allowed between requests before the session-state provider terminates the session.</summary>
            ///
            /// <returns>
            /// The time-out period, in minutes.
            /// </returns>
            ///
            public int Timeout
            {
                get { return this._timeout; }
                set { this._timeout = value; }
            }

            /// <summary>Gets or sets the locale identifier (LCID) of the current session.</summary>
            ///
            /// <returns>
            /// A <see cref="T:System.Globalization.CultureInfo"></see> instance that specifies the culture of the current session.
            /// </returns>
            ///
            public int LCID { get; set; }

            /// <summary>Gets or sets the code-page identifier for the current session.</summary>
            ///
            /// <returns>
            /// The code-page identifier for the current session.
            /// </returns>
            ///
            public int CodePage { get; set; }

            /// <summary>Gets a value indicating whether the session is read-only.</summary>
            ///
            /// <returns>
            /// true if the session is read-only; otherwise, false.
            /// </returns>
            ///
            bool IHttpSessionState.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            /// <summary>Gets or sets a session-state item value by name.</summary>
            ///
            /// <returns>
            /// The session-state item value specified in the name parameter.
            /// </returns>
            ///
            /// <param name="name">The key name of the session-state item value. </param>
            public object this[string name]
            {
                get { return this.BaseGet(name); }
                set { this.BaseSet(name, value); }
            }

            /// <summary>Gets or sets a session-state item value by numerical index.</summary>
            ///
            /// <returns>
            /// The session-state item value specified in the index parameter.
            /// </returns>
            ///
            /// <param name="index">The numerical index of the session-state item value. </param>
            public object this[int index]
            {
                get { return this.BaseGet(index); }
                set { this.BaseSet(index, value); }
            }

            /// <summary>Ends the current session.</summary>
            ///
            public void Abandon()
            {
				_logger.LogDebug(">>> Entering Abandon()");
                this.BaseClear();
				_logger.LogDebug("<<< Exiting Abandon()");
            }

            /// <summary>Adds a new item to the session-state collection.</summary>
            ///
            /// <param name="name">The name of the item to add to the session-state collection. </param>
            /// <param name="value">The value of the item to add to the session-state collection. </param>
            public void Add(string name, object value)
            {
				_logger.LogDebug(">>> Entering Add(name={Name},value={Value})", name, value);
                this.BaseAdd(name, value);
				_logger.LogDebug("<<< Exiting Add(name={Name},value={Value})", name, value);
            }

            /// <summary>Deletes an item from the session-state item collection.</summary>
            ///
            /// <param name="name">The name of the item to delete from the session-state item collection. </param>
            public void Remove(string name)
            {
				_logger.LogDebug(">>> Entering Remove(name={Name})", name);
                this.BaseRemove(name);
				_logger.LogDebug("<<< Exiting Remove(name={Name})", name);
            }

            /// <summary>Deletes an item at a specified index from the session-state item collection.</summary>
            ///
            /// <param name="index">The index of the item to remove from the session-state collection. </param>
            public void RemoveAt(int index)
            {
				_logger.LogDebug(">>> Entering RemoveAt(index={Index})", index);
                this.BaseRemoveAt(index);
				_logger.LogDebug("<<< Exiting RemoveAt(index={Index})", index);
            }

            /// <summary>Clears all values from the session-state item collection.</summary>
            ///
            public void Clear()
            {
				_logger.LogDebug(">>> Entering Clear()");
                this.BaseClear();
				_logger.LogDebug("<<< Exiting Clear()");
            }

            /// <summary>Clears all values from the session-state item collection.</summary>
            ///
            public void RemoveAll()
            {
				_logger.LogDebug(">>> Entering RemoveAll()");
                this.BaseClear();
				_logger.LogDebug("<<< Exiting RemoveAll()");
            }

            /// <summary>Copies the collection of session-state item values to a one-dimensional array, starting at the specified index in the array.</summary>
            ///
            /// <param name="array">The <see cref="T:System.Array"></see> that receives the session values. </param>
            /// <param name="index">The index in array where copying starts. </param>
            public void CopyTo(Array array, int index)
            {
				_logger.LogDebug(">>> Entering CopyTo(array={Array},index={Index})", array, index);
				_logger.LogWarning("Throwing NotImplementedException from CopyTo(array={Array},index={Index})", array, index);
				_logger.LogDebug("<<< Exiting CopyTo(array={Array},index={Index})", array, index);
                throw new NotImplementedException();
            }
        }

        internal class ConfigMapPath : IConfigMapPath
        {
            private readonly HttpSimulator _requestSimulation;

            public ConfigMapPath(HttpSimulator simulation)
            {
				_logger.LogDebug(">>> Entering ConfigMapPath(simulation={Simulation})", simulation);
                this._requestSimulation = simulation;
				_logger.LogDebug("ConfigMapPath(simulation={Simulation}): _requestSimulation → {_requestSimulation}", simulation, _requestSimulation);
				_logger.LogDebug("ConfigMapPath(simulation={Simulation}): this._requestSimulation → {Arg1}", simulation, this._requestSimulation);
				_logger.LogDebug("<<< Exiting ConfigMapPath(simulation={Simulation})", simulation);
            }

            public string GetMachineConfigFilename()
            {
				_logger.LogDebug(">>> Entering GetMachineConfigFilename()");
				_logger.LogWarning("Throwing NotImplementedException from GetMachineConfigFilename()");
				_logger.LogDebug("<<< Exiting GetMachineConfigFilename()");
                throw new NotImplementedException();
            }

            public string GetRootWebConfigFilename()
            {
				_logger.LogDebug(">>> Entering GetRootWebConfigFilename()");
				_logger.LogWarning("Throwing NotImplementedException from GetRootWebConfigFilename()");
				_logger.LogDebug("<<< Exiting GetRootWebConfigFilename()");
                throw new NotImplementedException();
            }

            public void GetPathConfigFilename(string siteId, string path, out string directory, out string baseName)
            {
				_logger.LogDebug(">>> Entering GetPathConfigFilename(siteId={SiteId},path={Path})", siteId, path);
				_logger.LogWarning("Throwing NotImplementedException from GetPathConfigFilename(siteId={SiteId},path={Path})", siteId, path);
				_logger.LogDebug("<<< Exiting GetPathConfigFilename(siteId={SiteId},path={Path})", siteId, path);
                throw new NotImplementedException();
            }

            public void GetDefaultSiteNameAndID(out string siteName, out string siteId)
            {
				_logger.LogDebug(">>> Entering GetDefaultSiteNameAndID()");
				_logger.LogWarning("Throwing NotImplementedException from GetDefaultSiteNameAndID()");
				_logger.LogDebug("<<< Exiting GetDefaultSiteNameAndID()");
                throw new NotImplementedException();
            }

            public void ResolveSiteArgument(string siteArgument, out string siteName, out string siteId)
            {
				_logger.LogDebug(">>> Entering ResolveSiteArgument(siteArgument={SiteArgument})", siteArgument);
				_logger.LogWarning("Throwing NotImplementedException from ResolveSiteArgument(siteArgument={SiteArgument})", siteArgument);
				_logger.LogDebug("<<< Exiting ResolveSiteArgument(siteArgument={SiteArgument})", siteArgument);
                throw new NotImplementedException();
            }

            public string MapPath(string siteId, string path)
            {
				_logger.LogDebug(">>> Entering MapPath(siteId={SiteId},path={Path})", siteId, path);
                var page = StripPrecedingSlashes(RightAfter(path, this._requestSimulation.ApplicationPath));
                return Path.Combine(this._requestSimulation.PhysicalApplicationPath, page.Replace("/", @"\"));
            }

            public string GetAppPathForPath(string siteId, string path)
            {
				_logger.LogDebug(">>> Entering GetAppPathForPath(siteId={SiteId},path={Path})", siteId, path);
				_logger.LogDebug("<<< Exiting GetAppPathForPath(siteId={SiteId},path={Path})", siteId, path);
                return this._requestSimulation.ApplicationPath;
            }
        }
    }
}
