// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
    using System.IO;
    using System.Reflection;

    public static class FileHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static string GetAbsoluteDir(string relativePathIn)
        {
			Logger.LogDebug(">>> Entering GetAbsoluteDir(relativePathIn={RelativePathIn})", relativePathIn);
            if (!relativePathIn.StartsWith(@"\", StringComparison.Ordinal))
            {
                relativePathIn = @"\" + relativePathIn;
            }
			Logger.LogDebug("GetAbsoluteDir(relativePathIn={RelativePathIn}): relativePathIn → {RelativePathIn1}", relativePathIn, relativePathIn);

            var rootDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			Logger.LogDebug("GetAbsoluteDir(relativePathIn={RelativePathIn}): rootDirectory → {RootDirectory}", relativePathIn, rootDirectory);
            var pathOut = rootDirectory + relativePathIn;

			Logger.LogDebug("<<< Exiting GetAbsoluteDir(relativePathIn={RelativePathIn})", relativePathIn);
            return pathOut;
        }
    }
}
