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

    public class TestUtil
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering AddUser(portalId={PortalId},userName={UserName},password={Password},vanityUrl={VanityUrl})", portalId, userName, password, vanityUrl);
            var user = UserController.GetUserByName(portalId, userName);
            if (user == null)
			Logger.LogDebug("AddUser(portalId={PortalId},userName={UserName},password={Password},vanityUrl={VanityUrl}): user → {User}", portalId, userName, password, vanityUrl, user);
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
				Logger.LogDebug("AddUser(portalId={PortalId},userName={UserName},password={Password},vanityUrl={VanityUrl}): user.VanityUrl → {Arg4}", portalId, userName, password, vanityUrl, user.VanityUrl);
				Logger.LogDebug("AddUser(portalId={PortalId},userName={UserName},password={Password},vanityUrl={VanityUrl}): user.IsDeleted → {Arg4}", portalId, userName, password, vanityUrl, user.IsDeleted);
                user.IsDeleted = false;
                UserController.UpdateUser(portalId, user);
            }
			Logger.LogDebug("<<< Exiting AddUser(portalId={PortalId},userName={UserName},password={Password},vanityUrl={VanityUrl})", portalId, userName, password, vanityUrl);
        }

        internal static void DeleteUser(int portalId, string userName)
        {
			Logger.LogDebug(">>> Entering DeleteUser(portalId={PortalId},userName={UserName})", portalId, userName);
            var user = UserController.GetUserByName(portalId, userName);
            if (user != null)
			Logger.LogDebug("DeleteUser(portalId={PortalId},userName={UserName}): user → {User}", portalId, userName, user);
            {
                UserController.RemoveUser(user);
            }
			Logger.LogDebug("<<< Exiting DeleteUser(portalId={PortalId},userName={UserName})", portalId, userName);
        }

        internal static string GetEmbeddedFileName(string fileName)
        {
			Logger.LogDebug(">>> Entering GetEmbeddedFileName(fileName={FileName})", fileName);
            string fullName = string.Format("{0}.{1}", EmbeddedFilePath, fileName);
            if (!fullName.ToLowerInvariant().EndsWith(".csv"))
            {
                fullName += ".csv";
            }

			Logger.LogDebug("<<< Exiting GetEmbeddedFileName(fileName={FileName})", fileName);
            return fullName;
        }

        internal static Stream GetEmbeddedFileStream(string fileName)
        {
			Logger.LogDebug(">>> Entering GetEmbeddedFileStream(fileName={FileName})", fileName);
			Logger.LogDebug("<<< Exiting GetEmbeddedFileStream(fileName={FileName})", fileName);
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(GetEmbeddedFileName(fileName));
        }

        internal static void GetReplaceCharDictionary(Dictionary<string, string> testFields, Dictionary<string, string> replaceCharacterDictionary)
        {
            string replaceCharWithChar = testFields.GetValue("ReplaceChars");

            if (!string.IsNullOrEmpty(replaceCharWithChar))
            {
                string[] pairs = replaceCharWithChar.Split(';');
                foreach (string pair in pairs)
                {
                    string[] vals = pair.Split(':');
                    string key = null;
                    string val = null;
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
        }

        internal static string ReadStream(string fileName)
        {
			Logger.LogDebug(">>> Entering ReadStream(fileName={FileName})", fileName);
			Logger.LogDebug("<<< Exiting ReadStream(fileName={FileName})", fileName);
            return Util.ReadStream(FilePath, fileName);
        }

        internal static void ReadStream(string fileName, Action<string, string> onReadLine)
        {
            Util.ReadStream(FilePath, fileName, onReadLine);
        }
    }
}
