// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Common.Utilities;

    public static class ExceptionExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static string Hash(this Exception exc)
        {
			Logger.LogDebug(">>> Entering Hash(exc={Exc})", exc);
            if (exc == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

			Logger.LogDebug("Hash(exc={Exc}): sb → {Sb}", exc, sb);
            AddException(sb, exc);

            if (exc.InnerException != null)
            {
                AddException(sb, exc.InnerException);
            }

			Logger.LogDebug("<<< Exiting Hash(exc={Exc})", exc);
            return sb.ToString().ToUpperInvariant().GenerateSha256Hash();
        }

        private static void AddException(StringBuilder sb, Exception ex)
        {
            if (!string.IsNullOrEmpty(ex.Message))
            {
                sb.AppendLine(ex.Message);
            }

            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                sb.AppendLine(ex.StackTrace);
            }
        }
    }
}
