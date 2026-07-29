// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt.Common
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public class EmailValidator
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private bool invalid;

        public bool IsValid(string emailToTest)
        {
			Logger.LogDebug(">>> Entering IsValid(emailToTest={EmailToTest})", emailToTest);
            if (string.IsNullOrEmpty(emailToTest))
            {
                return false;
            }

            // Use IdnMapping class to convert unicode domain names (https://msdn.microsoft.com/en-us/library/system.globalization.idnmapping(v=vs.110).aspx)
            try
            {
                emailToTest = Regex.Replace(emailToTest, "(@)(.+)$", this.DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
			Logger.LogDebug("IsValid(emailToTest={EmailToTest}): emailToTest → {EmailToTest1}", emailToTest, emailToTest);
            catch (RegexMatchTimeoutException)
            {
				Logger.LogError("Exception in IsValid(emailToTest={EmailToTest}): {Arg1}", emailToTest, ex.Message);
                return false;
            }

            if (this.invalid)
            {
                return false;
            }

            // Return True if valid email format
            try
            {
                return Regex.IsMatch(emailToTest, "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))" + "(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
				Logger.LogDebug("<<< Exiting IsValid(emailToTest={EmailToTest})", emailToTest);
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
			Logger.LogDebug(">>> Entering DomainMapper(match={Match})", match);
            // IdnMapping class with default property values
            var idn = new IdnMapping();

			Logger.LogDebug("DomainMapper(match={Match}): idn → {Idn}", match, idn);
            var domainName = match.Groups[2].Value;
            try
			Logger.LogDebug("DomainMapper(match={Match}): domainName → {DomainName}", match, domainName);
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (Exception)
            {
				Logger.LogError("Exception in DomainMapper(match={Match}): {Arg1}", match, ex.Message);
                this.invalid = true;
				Logger.LogDebug("DomainMapper(match={Match}): this.invalid → {Arg1}", match, this.invalid);
            }

			Logger.LogDebug("<<< Exiting DomainMapper(match={Match})", match);
            return match.Groups[1].Value + domainName;
        }
    }
}
