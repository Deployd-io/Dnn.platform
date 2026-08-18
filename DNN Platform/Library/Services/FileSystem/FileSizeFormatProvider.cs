// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class FileSizeFormatProvider : IFormatProvider, ICustomFormatter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSizeFormatProvider));
        private const string FileSizeFormat = "fs";
        private const decimal OneKiloByte = 1024;
        private const decimal OneMegaByte = OneKiloByte * 1024;
        private const decimal OneGigaByte = OneMegaByte * 1024;

        /// <inheritdoc />
        public object GetFormat(Type formatType)
        {
			Logger.LogDebug(">>> Entering GetFormat(formatType={FormatType})", formatType);
			Logger.LogDebug(">>> Entering Format(format={Format},arg={Arg},formatProvider={FormatProvider})", format, arg, formatProvider);
			Logger.LogDebug("<<< Exiting Format(format={Format},arg={Arg},formatProvider={FormatProvider})", format, arg, formatProvider);
			Logger.LogDebug("<<< Exiting GetFormat(formatType={FormatType})", formatType);
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        /// <inheritdoc />
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (format == null || !format.StartsWith(FileSizeFormat, StringComparison.Ordinal))
            {
                return DefaultFormat(format, arg, formatProvider);
            }

            if (arg is string)
            {
                return DefaultFormat(format, arg, formatProvider);
            }

            decimal size;

            try
            {
                size = Convert.ToDecimal(arg, formatProvider);
            }
            catch (InvalidCastException)
            {
                return DefaultFormat(format, arg, formatProvider);
            }

            string suffix;
            if (size >= OneGigaByte)
            {
                size /= OneGigaByte;
                suffix = Localization.GetString("SizeGb");
            }
            else if (size >= OneMegaByte)
            {
                size /= OneMegaByte;
                suffix = Localization.GetString("SizeMb");
            }
            else if (size >= OneKiloByte)
            {
                size /= OneKiloByte;
                suffix = Localization.GetString("SizeKb");
            }
            else
            {
                suffix = Localization.GetString("SizeB");
            }

            return string.Format(formatProvider, "{0:N1} {1}", size, suffix);
        }

        private static string DefaultFormat(string format, object arg, IFormatProvider formatProvider)
        {
			Logger.LogDebug(">>> Entering DefaultFormat(format={Format},arg={Arg},formatProvider={FormatProvider})", format, arg, formatProvider);
            if (arg is IFormattable formattableArg)
            {
			Logger.LogDebug("<<< Exiting DefaultFormat(format={Format},arg={Arg},formatProvider={FormatProvider})", format, arg, formatProvider);
                return formattableArg.ToString(format, formatProvider);
            }

            return arg.ToString();
        }
    }
}
