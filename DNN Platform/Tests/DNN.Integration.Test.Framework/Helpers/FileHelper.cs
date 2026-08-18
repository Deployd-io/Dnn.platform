// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
    using System.IO;
    using System.Reflection;
	using Microsoft.Extensions.Logging;

    public static class FileHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static string GetAbsoluteDir(string relativePathIn)
        {
			_logger.LogDebug(">>> Entering GetAbsoluteDir(relativePathIn={RelativePathIn})", relativePathIn);
            if (!relativePathIn.StartsWith(@"\", StringComparison.Ordinal))
            {
                relativePathIn = @"\" + relativePathIn;
				_logger.LogDebug("GetAbsoluteDir(relativePathIn={RelativePathIn}): relativePathIn → {RelativePathIn1}", relativePathIn, relativePathIn);
            }

            var rootDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var pathOut = rootDirectory + relativePathIn;

            return pathOut;
        }
    }
}
