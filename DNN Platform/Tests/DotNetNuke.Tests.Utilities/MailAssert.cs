// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using NUnit.Framework;

    public static class MailAssert
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static void Base64EncodedContentLineContains(string expectedValue, string toAddress, string findByText)
        {
			Logger.LogDebug(">>> Entering Base64EncodedContentLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
            var contentLine = ConvertEmailContentFromBase64ToUnicode(GetEmailFileName(toAddress, findByText));

			Logger.LogDebug("Base64EncodedContentLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText}): contentLine → {ContentLine}", expectedValue, toAddress, findByText, contentLine);
            Assert.That(contentLine, Does.Contain(expectedValue));
			Logger.LogDebug("<<< Exiting Base64EncodedContentLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
        }

        public static void ContentLineContains(string expectedValue, string toAddress, string findByText)
        {
			Logger.LogDebug(">>> Entering ContentLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
            var contentLine = FindContentUsingRegex(expectedValue, GetEmailFileName(toAddress, findByText));

			Logger.LogDebug("ContentLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText}): contentLine → {ContentLine}", expectedValue, toAddress, findByText, contentLine);
            Assert.That(string.IsNullOrEmpty(contentLine), Is.False);
			Logger.LogDebug("<<< Exiting ContentLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
        }

        public static void FromLineContains(string expectedValue, string toAddress, string findByText)
        {
			Logger.LogDebug(">>> Entering FromLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
            string fromLine = FindEmailLineUsingRegex("From", GetEmailFileName(toAddress, findByText));

            Assert.That(fromLine, Does.Contain(expectedValue));
			Logger.LogDebug("<<< Exiting FromLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
        }

        public static void MessageSent(string expectedValue, string toAddress, string message)
        {
			Logger.LogDebug(">>> Entering MessageSent(expectedValue={ExpectedValue},toAddress={ToAddress},message={Message})", expectedValue, toAddress, message);
            GetEmailFileName(toAddress, expectedValue);
			Logger.LogDebug("<<< Exiting MessageSent(expectedValue={ExpectedValue},toAddress={ToAddress},message={Message})", expectedValue, toAddress, message);
        }

        public static void ReplyToLineContains(string expectedValue, string toAddress, string findByText)
        {
			Logger.LogDebug(">>> Entering ReplyToLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
            string replyToLine = FindEmailLineUsingRegex("Reply-To", GetEmailFileName(toAddress, findByText));

            Assert.That(replyToLine, Does.Contain(expectedValue));
			Logger.LogDebug("<<< Exiting ReplyToLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
        }

        public static void SubjectLineContains(string expectedValue, string toAddress, string findByText)
        {
			Logger.LogDebug(">>> Entering SubjectLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
            string subjectLine = FindEmailLineUsingRegex("Subject", GetEmailFileName(toAddress, findByText));

            Assert.That(subjectLine, Does.Contain(expectedValue));
			Logger.LogDebug("<<< Exiting SubjectLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
        }

        public static void ToLineContains(string expectedValue, string toAddress, string findByText)
        {
			Logger.LogDebug(">>> Entering ToLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
            string toLine = FindEmailLineUsingRegex("To", GetEmailFileName(toAddress, findByText));

            Assert.That(toLine, Does.Contain(expectedValue));
			Logger.LogDebug("<<< Exiting ToLineContains(expectedValue={ExpectedValue},toAddress={ToAddress},findByText={FindByText})", expectedValue, toAddress, findByText);
        }

        private static string ConvertEmailContentFromBase64ToUnicode(string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);

            // Separate the content from the EmailAddress information
            const string contentLineRegex = "(base64)(.*(\n)+)+";
            Match match = Regex.Match(@emailContent, contentLineRegex, RegexOptions.Singleline);
            Assert.That(match.Success, Is.True, "Could not find content Line! Looking in file: " + emailFileName);

            // Convert the EmailAddress content
            emailContent = match.Groups[2].Value.Replace("\r\n", string.Empty);

            byte[] b = Convert.FromBase64String(emailContent);
            string decodedEmailContent = Encoding.ASCII.GetString(b);
            return decodedEmailContent;
        }

        private static string FindContentUsingRegex(string content, string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);
            string contentLineRegex = @content + ".*";
            Match match = Regex.Match(@emailContent, @contentLineRegex);
            Assert.That(match.Success, Is.True, "Could not find content Line! Looking in file: " + emailFileName);
            string contentLine = match.Value;
            return contentLine;
        }

        private static string FindEmailLineUsingRegex(string linePrefix, string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);
            string lineRegex = @"^(" + linePrefix + @"): (.+)";
            Match match = Regex.Match(@emailContent, @lineRegex, RegexOptions.Multiline);
            Assert.That(match.Success, Is.True, "Could not find " + linePrefix + " Line! Looking in file: " + emailFileName);
            string line = match.Value;
            return line;
        }

        private static string FindLineUsingRegex(string linePrefix, string emailFileName)
        {
            string emailContent = File.ReadAllText(emailFileName);
            string lineRegex = linePrefix + @": .*";
            Match match = Regex.Match(@emailContent, @lineRegex);
            Assert.That(match.Success, Is.True, "Could not find " + linePrefix + " Line! Looking in file: " + emailFileName);
            string line = match.Value;
            return line;
        }

        private static string GetEmailFileName(string toAddress, string findByText)
        {
			Logger.LogDebug(">>> Entering GetEmailFileName(toAddress={ToAddress},findByText={FindByText})", toAddress, findByText);
			Logger.LogDebug("<<< Exiting GetEmailFileName(toAddress={ToAddress},findByText={FindByText})", toAddress, findByText);
            return GetEmailFileName(toAddress, findByText, "The newsletter sent to " + toAddress + " could not be found");
        }

        private static string GetEmailFileName(string toAddress, string findByText, string message)
        {
            var physicalPath = Directory.GetCurrentDirectory();
            var packagePath = physicalPath.Replace("\\Fixtures", "\\Packages");
            var emailPath = packagePath + "\\TestEmails";
            var emailFileName = string.Empty;

            foreach (var email in Directory.GetFiles(emailPath))
            {
                string emailContent = File.ReadAllText(email);
                if (emailContent.Contains(findByText) && emailContent.Contains("To: " + toAddress))
                {
                    emailFileName = email;
                    break;
                }
            }

            Assert.That(emailFileName, Is.Not.Null, message + " The test was searching in: " + emailPath);

			Logger.LogDebug("<<< Exiting GetEmailFileName(toAddress={ToAddress},findByText={FindByText},message={Message})", toAddress, findByText, message);
            return emailFileName;
        }
    }
}
