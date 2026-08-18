// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System.Linq;

    using DNN.Integration.Test.Framework.Helpers;
	using Microsoft.Extensions.Logging;

    public class PermissionController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static dynamic GetPermission(string code, string key)
        {
			_logger.LogDebug(">>> Entering GetPermission(code={Code},key={Key})", code, key);
            const string query =
                @"SELECT * 
                  FROM {{objectQualifier}}Permission
                  WHERE PermissionCode = '{0}' AND PermissionKey = '{1}'";
				_logger.LogDebug("GetPermission(code={Code},key={Key}): PermissionCode → {PermissionCode}", code, key, PermissionCode);
			_logger.LogDebug("<<< Exiting GetPermission(code={Code},key={Key})", code, key);
            return DatabaseHelper.ExecuteDynamicQuery(string.Format(query, code, key)).SingleOrDefault();
        }
    }
}
