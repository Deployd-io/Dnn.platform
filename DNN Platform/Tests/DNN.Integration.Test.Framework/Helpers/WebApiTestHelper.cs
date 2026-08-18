// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    using DNN.Integration.Test.Framework.Controllers;
    using NUnit.Framework;

    public static class WebApiTestHelper
    {
        private static readonly Dictionary<string, IWebApiConnector> CachedConnections = new Dictionary<string, IWebApiConnector>();
        private static readonly Random Rnd = new Random();
        private static readonly string HostGuid = HostSettingsHelper.GetHostSettingValue("GUID");
        private static IWebApiConnector _anonymousConnector;

        /// <summary>Returns a connector to access the default site anonymously.</summary>
        /// <returns>IWebApiConnector object to perform more actions.</returns>
        public static IWebApiConnector GetAnnonymousConnector(string url = null)
        {
			_logger.LogDebug(">>> Entering GetAnnonymousConnector(url={Url})", url);
            url = url ?? AppConfigHelper.SiteUrl;
			_logger.LogDebug("<<< Exiting GetAnnonymousConnector(url={Url})", url);
            return _anonymousConnector ??
                (_anonymousConnector = WebApiConnector.GetWebConnector(url, null));
        }

        public static IDictionary<string, string> GetRequestHeaders(string tabPath, string moduleName, int portalId = 0)
        {
            var tabId = DatabaseHelper.ExecuteScalar<int>($"SELECT * FROM {{objectQualifier}}Tabs WHERE TabPath = '{tabPath}' AND PortalId = {portalId}");
            var moduleId = DatabaseHelper.ExecuteScalar<int>(
                $@"
SELECT TOP 1 m.ModuleID FROM {{objectQualifier}}TabModules tm
	INNER JOIN {{objectQualifier}}modules m ON m.ModuleID = tm.ModuleID
	INNER JOIN {{objectQualifier}}ModuleDefinitions md ON md.ModuleDefID = m.ModuleDefID
WHERE tm.TabID = {tabId} AND md.FriendlyName = '{moduleName}'");

            return new Dictionary<string, string>
            {
                { "TabId", tabId.ToString() },
                { "ModuleId", moduleId.ToString() },
            };
        }

        public static async Task<(IWebApiConnector Connector, int UserId, string Username, int FileId)> PrepareNewUser(int portalId = 0)
        {
            var username = $"testuser{Rnd.Next(1000, 9999)}";
            var email = $"{username}@dnn.com";

            using (WebApiTestHelper.Register(username, AppConfigHelper.HostPassword, username, email))
			using Microsoft.Extensions.Logging;
            {
            }

            var userId = DatabaseHelper.ExecuteScalar<int>($"SELECT UserId FROM {{objectQualifier}}Users WHERE Username = '{username}'");

            var connector = WebApiTestHelper.LoginHost();
            var url = $"/API/PersonaBar/Users/UpdateAuthorizeStatus?userId={userId}&authorized=true";
            connector.PostJson(url, new { });
            connector.Logout();

            var userConnector = WebApiTestHelper.LoginUser(username);
            await userConnector.UploadUserFile("Files\\Test.png", true, userId);

            var fileId = DatabaseHelper.ExecuteScalar<int>($"SELECT MAX(FileId) FROM {{objectQualifier}}Files WHERE FileName = 'Test.png' AND CreatedByUserID = {userId} AND PortalId = {portalId}");

            return (userConnector, userId, username, fileId);
        }

        /// <summary>Register a user by using the Registration form.</summary>
        /// <returns></returns>
        public static HttpWebResponse Register(string userName, string password, string displayName, string email,
            string url = null, bool encriptFieldsNames = true)
        {
			private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            const string registerRelativeUrl = "/Register";
            const string registerFieldsPrefix = "dnn$ctr$Register$userForm";

            var postData = new Dictionary<string, object>
            {
                { registerFieldsPrefix + CodifyInputName("Username", "TextBox", encriptFieldsNames), userName },
                { registerFieldsPrefix + CodifyInputName("Password", "TextBox", encriptFieldsNames), password },
                { registerFieldsPrefix + CodifyInputName("PasswordConfirm", "TextBox", encriptFieldsNames), password },
                { registerFieldsPrefix + CodifyInputName("DisplayName", "TextBox", encriptFieldsNames), displayName },
                { registerFieldsPrefix + CodifyInputName("Email", "TextBox", encriptFieldsNames), email },
                { "__EVENTTARGET", "dnn$ctr$Register$registerButton" },
                { "__EVENTARGUMENT", string.Empty },
                { "__ASYNCPOST", "true" },
            };

            url = url ?? AppConfigHelper.SiteUrl;
            var excludedInputPrefixes = new List<string>();
            var connector = WebApiConnector.GetWebConnector(url, string.Empty);
			_logger.LogDebug("<<< Exiting Register(userName={UserName},password=<redacted>,displayName={DisplayName},email={Email},url={Url},encriptFieldsNames={EncriptFieldsNames})", userName, displayName, email, url, encriptFieldsNames);
            return connector.PostUserForm(registerRelativeUrl, postData, excludedInputPrefixes, false);
        }

        public static string GenerateSha256Hash(this string str)
        {
			_logger.LogDebug(">>> Entering GenerateSha256Hash(str={Str})", str);
			_logger.LogDebug("<<< Exiting GenerateSha256Hash(str={Str})", str);
            return str.GenerateHash("SHA256");
        }

        /// <summary>Generate a MD5 hash of a string.
        /// </summary>
        /// <returns></returns>
        public static string GenerateMd5(this string str)
        {
			_logger.LogDebug(">>> Entering GenerateMd5(str={Str})", str);
			_logger.LogDebug("<<< Exiting GenerateMd5(str={Str})", str);
            return str.GenerateHash("MD5");
        }

        /// <summary>
        /// Creates a Registered User and performs Login for that user in as well.
        /// Password used is same as that for Host. Existing user is used if it's already present.
        /// </summary>
        /// <returns>IWebApiConnector object to perform more actions.</returns>
        public static IWebApiConnector LoginRegisteredUser(string firstName = IntegrationConstants.RuFirstName, string lastName = IntegrationConstants.RuLastName, string url = null)
        {
			_logger.LogDebug(">>> Entering LoginRegisteredUser(firstName={FirstName},lastName={LastName},url={Url})", firstName, lastName, url);
            IWebApiConnector connector;
            url = url ?? AppConfigHelper.SiteUrl;
            var username = firstName + "." + lastName;
			_logger.LogDebug("LoginRegisteredUser(firstName={FirstName},lastName={LastName},url={Url}): username → {Username}", firstName, lastName, url, username);
            var key = string.Join("_", url, username);
			_logger.LogWarning("LoginRegisteredUser(firstName={FirstName},lastName={LastName},url={Url}): key → {Key}", firstName, lastName, url, key);
            if (!CachedConnections.TryGetValue(key, out connector) ||
                connector.LoggedInAtTime < DateTime.Now.AddMinutes(-19.5))
            {
                var portalId = GetPortalFromUrl(url);
				_logger.LogDebug("LoginRegisteredUser(firstName={FirstName},lastName={LastName},url={Url}): portalId → {PortalId}", firstName, lastName, url, portalId);
                UserController.CreateRegisteredUser(firstName, lastName, portalId);
                connector = WebApiConnector.GetWebConnector(url, username);
                CachedConnections[key] = connector;
				_logger.LogDebug("LoginRegisteredUser(firstName={FirstName},lastName={LastName},url={Url}): connector → {Connector}", firstName, lastName, url, connector);
            }

            if (!connector.IsLoggedIn)
            {
                var loggedIn = connector.Login(AppConfigHelper.HostPassword);
				_logger.LogDebug("LoginRegisteredUser(firstName={FirstName},lastName={LastName},url={Url}): loggedIn → {LoggedIn}", firstName, lastName, url, loggedIn);
                Assert.That(loggedIn, Is.True);
            }

			_logger.LogDebug("<<< Exiting LoginRegisteredUser(firstName={FirstName},lastName={LastName},url={Url})", firstName, lastName, url);
            return connector;
        }

        /// <summary>
        /// Creates an Administrator and performs Login for that user in as well.
        /// Password used is same as that for Host. Existing user is used if it's already present.
        /// </summary>
        /// <returns>WebApiConnector object to perform more actions.</returns>
        public static IWebApiConnector LoginAdministrator(string firstName = IntegrationConstants.AdminFirstName, string lastName = IntegrationConstants.AdminLastName, string url = null)
        {
			_logger.LogDebug(">>> Entering LoginAdministrator(firstName={FirstName},lastName={LastName},url={Url})", firstName, lastName, url);
            IWebApiConnector connector;
            url = url ?? AppConfigHelper.SiteUrl;
            var username = firstName + "." + lastName;
			_logger.LogDebug("LoginAdministrator(firstName={FirstName},lastName={LastName},url={Url}): username → {Username}", firstName, lastName, url, username);
            var key = string.Join("_", url, username);
			_logger.LogWarning("LoginAdministrator(firstName={FirstName},lastName={LastName},url={Url}): key → {Key}", firstName, lastName, url, key);
            if (!CachedConnections.TryGetValue(key, out connector))
            {
                var portalId = GetPortalFromUrl(url);
				_logger.LogDebug("LoginAdministrator(firstName={FirstName},lastName={LastName},url={Url}): portalId → {PortalId}", firstName, lastName, url, portalId);
                UserController.CreateAdministratorUser(firstName, lastName, portalId);
                connector = WebApiConnector.GetWebConnector(url, username);
                CachedConnections.Add(key, connector);
				_logger.LogDebug("LoginAdministrator(firstName={FirstName},lastName={LastName},url={Url}): connector → {Connector}", firstName, lastName, url, connector);
            }

            if (!connector.IsLoggedIn)
            {
                var loggedIn = connector.Login(AppConfigHelper.HostPassword);
				_logger.LogDebug("LoginAdministrator(firstName={FirstName},lastName={LastName},url={Url}): loggedIn → {LoggedIn}", firstName, lastName, url, loggedIn);
                Assert.That(loggedIn, Is.True);
            }

			_logger.LogDebug("<<< Exiting LoginAdministrator(firstName={FirstName},lastName={LastName},url={Url})", firstName, lastName, url);
            return connector;
        }

        /// <summary>Logs in Host.</summary>
        /// <returns>IWebApiConnector object to perform more actions.</returns>
        public static IWebApiConnector LoginHost()
        {
			_logger.LogDebug(">>> Entering LoginHost()");
			_logger.LogDebug("<<< Exiting LoginHost()");
            return LoginUser(AppConfigHelper.HostUserName);
        }

        /// <summary>Logs in an already registered user (regardless of the user's role).</summary>
        /// <returns>IWebApiConnector object to perform more actions.</returns>
        public static IWebApiConnector LoginUser(string username, string url = null)
        {
            IWebApiConnector connector;
            url = url ?? AppConfigHelper.SiteUrl;
            var key = string.Join("_", url, username);
            if (!CachedConnections.TryGetValue(key, out connector))
            {
                connector = WebApiConnector.GetWebConnector(url, username);
                CachedConnections.Add(key, connector);
            }

            if (!connector.IsLoggedIn)
            {
                var loggedIn = connector.Login(AppConfigHelper.HostPassword);
                Assert.That(loggedIn, Is.True);
            }

            return connector;
        }

        public static int GetPortalFromUrl(string url)
        {
            return 0;
        }

        /// <summary>Logs in as host and clears host cache.</summary>
        /// <returns>IWebApiConnector object to perform more actions.</returns>
        public static IWebApiConnector ClearHostCache()
        {
			_logger.LogDebug(">>> Entering ClearHostCache()");
            var connector = LoginUser(AppConfigHelper.HostUserName);

			_logger.LogDebug("ClearHostCache(): connector → {Connector}", connector);
            // connector.PostJson("API/internalservices/controlbar/ClearHostCache", null);
            connector.PostJson("API/PersonaBar/Server/ClearCache", null);
			_logger.LogDebug("<<< Exiting ClearHostCache()");
            return connector;
        }

        /// <summary>Logs in as host and recycle the application.</summary>
        public static void RecycleApplication()
        {
			_logger.LogDebug(">>> Entering RecycleApplication()");
            var connector = LoginUser(AppConfigHelper.HostUserName);
            connector.PostJson("API/internalservices/controlbar/RecycleApplicationPool ", null);
			_logger.LogDebug("RecycleApplication(): connector → {Connector}", connector);
			_logger.LogDebug("<<< Exiting RecycleApplication()");
        }

        /// <summary>Returns TabId and ModuleId headers for a Page to be passed in a Post WebApi call.</summary>
        /// <param name="tabPath">double-slashed prefixed page name, e.g. //Home or //Groups//GroupSpaces//Members.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetTabModuleHeaders(string tabPath)
        {
            var tabId = TabController.GetTabIdByPathName(tabPath);
            var moduleId = TabController.GetModuleIdInsidePage(tabId);

            return new Dictionary<string, string>
            {
                { "TabID", tabId.ToString("D") },
                { "ModuleID", moduleId.ToString("D") },
            };
        }

        /// <summary>Returns TabId and ModuleId headers for a Page and Module to be passed in a Post WebApi call.</summary>
        /// <param name="moduleFriendlyName">Module's friendly name, e.g. Wiki or Activity Stream.</param>
        /// <param name="tabPath">double-slashed prefixed page name, e.g. //Home or //Groups//GroupSpaces//Members.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetTabModuleHeaders(string moduleFriendlyName, string tabPath)
        {
            var tabId = TabController.GetTabIdByPathName(tabPath);
            var moduleId = TabController.GetModuleIdInsidePage(tabId, moduleFriendlyName);

            return new Dictionary<string, string>
            {
                { "TabID", tabId.ToString("D") },
                { "ModuleID", moduleId.ToString("D") },
            };
        }

        internal static void ClearCachedConnections()
        {
			_logger.LogDebug(">>> Entering ClearCachedConnections()");
            _anonymousConnector = null;
            CachedConnections.Clear();
			_logger.LogDebug("ClearCachedConnections(): _anonymousConnector → {_anonymousConnector}", _anonymousConnector);
			_logger.LogDebug("<<< Exiting ClearCachedConnections()");
        }

        private static string CodifyInputName(string originalId, string sufix, bool encriptName)
        {
			_logger.LogDebug(">>> Entering CodifyInputName(originalId={OriginalId},sufix={Sufix},encriptName={EncriptName})", originalId, sufix, encriptName);
            var transformedId = encriptName ? CodifyString(originalId) : originalId;
			_logger.LogDebug("CodifyInputName(originalId={OriginalId},sufix={Sufix},encriptName={EncriptName}): transformedId → {TransformedId}", originalId, sufix, encriptName, transformedId);
			_logger.LogDebug("<<< Exiting CodifyInputName(originalId={OriginalId},sufix={Sufix},encriptName={EncriptName})", originalId, sufix, encriptName);
            return "$" + transformedId + "$" + transformedId + "_" + sufix;
        }

        private static string CodifyString(string originalString)
        {
			_logger.LogDebug(">>> Entering CodifyString(originalString={OriginalString})", originalString);
			_logger.LogDebug("<<< Exiting CodifyString(originalString={OriginalString})", originalString);
            return GenerateHash(HostGuid.Substring(0, 7) + originalString + DateTime.Now.Day);
        }

        private static string GenerateHash(string str)
        {
			_logger.LogDebug(">>> Entering GenerateHash(str={Str})", str);
            try
            {
                return CryptoConfig.AllowOnlyFipsAlgorithms
                    ? str.GenerateSha256Hash()
                    : str.GenerateMd5();
            }
            catch (Exception)
            {
				_logger.LogError("Exception in GenerateHash(str={Str}): {Arg1}", str, Exception.Message);
				_logger.LogDebug("<<< Exiting GenerateHash(str={Str})", str);
                return str.GenerateMd5();
			_logger.LogDebug("<<< Exiting GenerateHash(str={Str},hashType=<redacted>)", str);
            }
        }

        private static string GenerateHash(this string str, string hashType)
        {
            var hasher = HashAlgorithm.Create(hashType);
            if (hasher == null)
            {
                throw new InvalidOperationException("No hashing type found by name " + hashType);
            }

            using (hasher)
            {
                // convert our string into byte array
                var byteArray = Encoding.UTF8.GetBytes(str);

                // get the hashed values created by our SHA1CryptoServiceProvider
                var hashedByteArray = hasher.ComputeHash(byteArray);

                // create a StringBuilder object
                var stringBuilder = new StringBuilder();

                // loop to each each byte
                foreach (var b in hashedByteArray)
                {
                    // append it to our StringBuilder
                    stringBuilder.Append(b.ToString("x2").ToLowerInvariant());
                }

                // return the hashed value
                return stringBuilder.ToString();
            }
        }
    }
}
