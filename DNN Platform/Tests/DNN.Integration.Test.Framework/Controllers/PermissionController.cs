// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System.Linq;

    using DNN.Integration.Test.Framework.Helpers;

    public class PermissionController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static dynamic GetPermission(string code, string key)
        {
			Logger.LogDebug(">>> Entering GetPermission(code={Code},key={Key})", code, key);
            const string query =
                @"SELECT * 
                  FROM {{objectQualifier}}Permission
                  WHERE PermissionCode = '{0}' AND PermissionKey = '{1}'";
			Logger.LogDebug("GetPermission(code={Code},key={Key}): PermissionCode → {PermissionCode}", code, key, PermissionCode);
			Logger.LogDebug("<<< Exiting GetPermission(code={Code},key={Key})", code, key);
            return DatabaseHelper.ExecuteDynamicQuery(string.Format(query, code, key)).SingleOrDefault();
        }
    }
}
