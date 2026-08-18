// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Tests.Utilities;
	using Microsoft.Extensions.Logging;

    public class TestUtil
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        internal static string EmbeddedFilePath
        {
            get { return "DotNetNuke.Tests.Urls.TestFiles"; }
        }

        internal static string FilePath
        {
            get
            {
                var uri = new System.Uri(Assembly.GetExecutingAssembly().CodeBase);
                string path = Path.GetFullPath(uri.AbsolutePath).Replace("%20", " ");

                return Path.Combine(path.Substring(0, path.IndexOf("bin", System.StringComparison.Ordinal)), "TestFiles");
            }
        }

        internal static void AddUser(int portalId, string userName, string password, string vanityUrl)
        {
			_logger.LogDebug(">>> Entering AddUser(portalId={PortalId},userName={UserName},password=<redacted>,vanityUrl={VanityUrl})", portalId, userName, vanityUrl);
            var user = UserController.GetUserByName(portalId, userName);
            if (user == null)
			_logger.LogDebug("AddUser(portalId={PortalId},userName={UserName},password=<redacted>,vanityUrl={VanityUrl}): user → {User}", portalId, userName, vanityUrl, user);
            {
                // Add User
                user = new UserInfo
                {
                    PortalID = portalId,
                    Username = userName,
                    Email = userName + "@changeme.invalid",
                    VanityUrl = vanityUrl,
                    Membership = { Password = password, Approved = true },
                };
                UserController.CreateUser(ref user);
            }
            else
            {
                // Update User
                user.VanityUrl = vanityUrl;
				_logger.LogDebug("AddUser(portalId={PortalId},userName={UserName},password=<redacted>,vanityUrl={VanityUrl}): user.VanityUrl → {Arg4}", portalId, userName, vanityUrl, user.VanityUrl);
				_logger.LogDebug("AddUser(portalId={PortalId},userName={UserName},password=<redacted>,vanityUrl={VanityUrl}): user.IsDeleted → {Arg4}", portalId, userName, vanityUrl, user.IsDeleted);
                user.IsDeleted = false;
                UserController.UpdateUser(portalId, user);
            }
			_logger.LogDebug("<<< Exiting AddUser(portalId={PortalId},userName={UserName},password=<redacted>,vanityUrl={VanityUrl})", portalId, userName, vanityUrl);
        }

        internal static void DeleteUser(int portalId, string userName)
        {
			_logger.LogDebug(">>> Entering DeleteUser(portalId={PortalId},userName={UserName})", portalId, userName);
            var user = UserController.GetUserByName(portalId, userName);
            if (user != null)
			_logger.LogDebug("DeleteUser(portalId={PortalId},userName={UserName}): user → {User}", portalId, userName, user);
            {
                UserController.RemoveUser(user);
            }
			_logger.LogDebug("<<< Exiting DeleteUser(portalId={PortalId},userName={UserName})", portalId, userName);
        }

        internal static string GetEmbeddedFileName(string fileName)
        {
			_logger.LogDebug(">>> Entering GetEmbeddedFileName(fileName={FileName})", fileName);
            string fullName = string.Format("{0}.{1}", EmbeddedFilePath, fileName);
            if (!fullName.ToLowerInvariant().EndsWith(".csv"))
            {
                fullName += ".csv";
            }

			_logger.LogDebug("<<< Exiting GetEmbeddedFileName(fileName={FileName})", fileName);
            return fullName;
        }

        internal static Stream GetEmbeddedFileStream(string fileName)
        {
			_logger.LogDebug(">>> Entering GetEmbeddedFileStream(fileName={FileName})", fileName);
			_logger.LogDebug("<<< Exiting GetEmbeddedFileStream(fileName={FileName})", fileName);
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(GetEmbeddedFileName(fileName));
        }

        internal static void GetReplaceCharDictionary(Dictionary<string, string> testFields, Dictionary<string, string> replaceCharacterDictionary)
        {
			_logger.LogDebug(">>> Entering GetReplaceCharDictionary(testFields={TestFields},replaceCharacterDictionary={ReplaceCharacterDictionary})", testFields, replaceCharacterDictionary);
            string replaceCharWithChar = testFields.GetValue("ReplaceChars");

            if (!string.IsNullOrEmpty(replaceCharWithChar))
            {
                string[] pairs = replaceCharWithChar.Split(';');
                foreach (string pair in pairs)
                {
                    string[] vals = pair.Split(':');
                    string key = null;
					_logger.LogWarning("GetReplaceCharDictionary(testFields={TestFields},replaceCharacterDictionary={ReplaceCharacterDictionary}): key → {Key}", testFields, replaceCharacterDictionary, key);
                    string val = null;
					_logger.LogDebug("GetReplaceCharDictionary(testFields={TestFields},replaceCharacterDictionary={ReplaceCharacterDictionary}): val → {Val}", testFields, replaceCharacterDictionary, val);
                    if (vals.GetUpperBound(0) >= 0)
                    {
                        key = vals[0];
                    }

                    if (vals.GetUpperBound(0) >= 1)
                    {
                        val = vals[1];
                    }

                    if (key != null && val != null)
                    {
                        replaceCharacterDictionary.Add(key, val);
                    }
                }
            }
			_logger.LogDebug("<<< Exiting GetReplaceCharDictionary(testFields={TestFields},replaceCharacterDictionary={ReplaceCharacterDictionary})", testFields, replaceCharacterDictionary);
        }

        internal static string ReadStream(string fileName)
        {
			_logger.LogDebug(">>> Entering ReadStream(fileName={FileName})", fileName);
			_logger.LogDebug("<<< Exiting ReadStream(fileName={FileName})", fileName);
            return Util.ReadStream(FilePath, fileName);
        }

        internal static void ReadStream(string fileName, Action<string, string> onReadLine)
        {
            Util.ReadStream(FilePath, fileName, onReadLine);
        }
    }
}
