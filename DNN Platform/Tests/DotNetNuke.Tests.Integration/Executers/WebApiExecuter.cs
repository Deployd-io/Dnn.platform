// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Tests.Utilities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public enum LoginAsUser
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        GivenUserName = 0,
        AnonymousUser = 1,
        RegisteredUser = 2,
        Host = 3,
    }

    public abstract class WebApiExecuter
    {
        public Func<string, string, string, IWebApiConnector> Login;

        protected Func<string, IWebApiConnector> Anonymous;

        protected List<HttpResponseMessage> Responses = new List<HttpResponseMessage>();

        private LoginAsUser _loginAs;
        private IWebApiConnector _connector;

        protected WebApiExecuter()
        {
			Logger.LogDebug(">>> Entering WebApiExecuter()");
            this.LoginAs = LoginAsUser.RegisteredUser;
            this.Login = WebApiTestHelper.LoginRegisteredUser;
			Logger.LogDebug("WebApiExecuter(): this.LoginAs → {Arg0}", this.LoginAs);
			Logger.LogDebug("WebApiExecuter(): Login → {Login}", Login);
			Logger.LogDebug("WebApiExecuter(): this.Login → {Arg0}", this.Login);
            this.Anonymous = WebApiTestHelper.GetAnnonymousConnector;

            this.UserFirstName = Constants.RuFirstName;
			Logger.LogDebug("WebApiExecuter(): Anonymous → {Anonymous}", Anonymous);
			Logger.LogDebug("WebApiExecuter(): this.Anonymous → {Arg0}", this.Anonymous);
			Logger.LogDebug("WebApiExecuter(): this.UserFirstName → {Arg0}", this.UserFirstName);
            this.UserLastName = Constants.RuLastName;
			Logger.LogDebug("<<< Exiting WebApiExecuter()");
			Logger.LogDebug("WebApiExecuter(): this.UserLastName → {Arg0}", this.UserLastName);
        }

        public string UserName
        {
            get
            {
                return this.Connector.UserName;
            }
        }

        public string DisplayName
        {
            get
            {
                return string.Join(" ", this.UserFirstName, this.UserLastName);
            }
        }

        public int UserId
        {
            get
            {
                return DatabaseHelper.ExecuteScalar<int>($"SELECT UserId FROM {{objectQualifier}}Users WHERE UserName = '{this.UserName}'");
            }
        }

        public LoginAsUser LoginAs
        {
            get
            {
                return this._loginAs;
            }

            set
            {
                this._loginAs = value;
                this._connector = null;
            }
        }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public IWebApiConnector Connector
        {
            get
            {
                if (this._connector == null)
                {
                    switch (this.LoginAs)
                    {
                        case LoginAsUser.RegisteredUser:
                            this._connector = this.Login(this.UserFirstName, this.UserLastName, null);
                            break;
                        case LoginAsUser.AnonymousUser:
                            this._connector = this.Anonymous(null);
                            break;
                        case LoginAsUser.Host:
                            this._connector = WebApiTestHelper.LoginHost();
                            break;
                        default:
                            this._connector = this.Login(this.UserFirstName, this.UserLastName, null);
                            break;
                    }
                }

                return this._connector;
            }

            set
            {
                this._connector = value;
                var userName = this._connector.UserName.Split('.');
                if (userName.Length == 2)
                {
                    this.UserFirstName = userName[0];
                    this.UserLastName = userName[1];
                }
            }
        }

        public HttpResponseMessage GetLastResponseMessage()
        {
			Logger.LogDebug(">>> Entering GetLastResponseMessage()");
			Logger.LogDebug("<<< Exiting GetLastResponseMessage()");
            return this.Responses.Last();
        }

        /// <summary>Return the last executer's response deserialized.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the executer does not have any response.</exception>
        /// <returns></returns>
        public JContainer GetLastDeserializeResponseMessage()
        {
			Logger.LogDebug(">>> Entering GetLastDeserializeResponseMessage()");
            if (!this.Responses.Any())
            {
                throw new InvalidOperationException("GetLastDeserializeResponseMessage cannot be called when the Executer does not have any Responses");
            }

            var data = this.Responses.Last().Content.ReadAsStringAsync().Result;
			Logger.LogDebug("GetLastDeserializeResponseMessage(): data → {Data}", data);
			Logger.LogDebug("<<< Exiting GetLastDeserializeResponseMessage()");
            return JsonConvert.DeserializeObject<JContainer>(data);
        }

        public IEnumerable<HttpResponseMessage> GetResponseMessages()
        {
			Logger.LogDebug(">>> Entering GetResponseMessages()");
			Logger.LogDebug("<<< Exiting GetResponseMessages()");
            return this.Responses.ToArray();
        }

        /// <summary>Return the list of the executer's responses deserialized.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the executer does not have any response.</exception>
        /// <returns></returns>
        public IEnumerable<JContainer> GetDeserializeResponseMessages()
        {
			Logger.LogDebug(">>> Entering GetDeserializeResponseMessages()");
            if (!this.Responses.Any())
            {
                throw new InvalidOperationException("GetDeserializeResponseMessages cannot be called when the Executer does not have any Responses");
            }

			Logger.LogDebug("<<< Exiting GetDeserializeResponseMessages()");
            return this.GetResponseMessages().Select(r => JsonConvert.DeserializeObject<JContainer>(r.Content.ReadAsStringAsync().Result));
        }
    }
}
