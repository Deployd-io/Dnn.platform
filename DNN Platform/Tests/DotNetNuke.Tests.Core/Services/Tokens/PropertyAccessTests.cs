// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.Tokens
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PropertyAccessTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IPortalController>()));
			Logger.LogDebug("<<< Exiting SetUp()");
			Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        [TestCase("now", false)]
        [TestCase("today", false)]
        [TestCase("ticksperday", false)]
        [TestCase("anything", true)]
        [TestCase("", true)]
        public void TicksPropertyAccess_GetProperty_Sets_PropertyNotFound(string propertyName, bool expected)
        {
            // Arrange
            var ticksPropertyAccess = new TicksPropertyAccess();
            var accessingUser = new UserInfo();

            // Act
            bool propertyNotFound = false;
            string propertyValue = ticksPropertyAccess.GetProperty(propertyName, string.Empty, CultureInfo.InvariantCulture, accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
            Assert.That(propertyNotFound, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("today")]
        [TestCase("ticksperday")]
        public void TicksPropertyAccess_GetProperty_Returns_Correct_String(string propertyName)
        {
			Logger.LogDebug(">>> Entering TicksPropertyAccess_GetProperty_Returns_Correct_String(propertyName={PropertyName})", propertyName);
            // Arrange
            var ticksPropertyAccess = new TicksPropertyAccess();
            var accessingUser = new UserInfo();
			Logger.LogDebug("TicksPropertyAccess_GetProperty_Returns_Correct_String(propertyName={PropertyName}): ticksPropertyAccess → {TicksPropertyAccess}", propertyName, ticksPropertyAccess);
			Logger.LogDebug("TicksPropertyAccess_GetProperty_Returns_Correct_String(propertyName={PropertyName}): accessingUser → {AccessingUser}", propertyName, accessingUser);

            long expected = DateTime.MinValue.Ticks;
            switch (propertyName)
			Logger.LogDebug("TicksPropertyAccess_GetProperty_Returns_Correct_String(propertyName={PropertyName}): expected → {Expected}", propertyName, expected);
            {
                case "now":
                    expected = DateTime.Now.Ticks;
                    break;
                case "today":
                    expected = DateTime.Today.Ticks;
                    break;
                case "ticksperday":
                    expected = TimeSpan.TicksPerDay;
                    break;
            }

            // Act
            bool propertyNotFound = false;
            string propertyValue = ticksPropertyAccess.GetProperty(propertyName, string.Empty, CultureInfo.InvariantCulture, accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
            Assert.That(propertyValue, Is.EqualTo(expected.ToString(CultureInfo.InvariantCulture)));
			Logger.LogDebug("<<< Exiting TicksPropertyAccess_GetProperty_Returns_Correct_String(propertyName={PropertyName})", propertyName);
        }

        [Test]
        [TestCase("current", false)]
        [TestCase("now", false)]
        [TestCase("system", false)]
        [TestCase("utc", false)]
        [TestCase("anything", true)]
        [TestCase("", true)]
        public void DateTimePropertyAccess_GetProperty_Sets_PropertyNotFound(string propertyName, bool expected)
        {
            // Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var accessingUser = new UserInfo { Profile = new UserProfile { PreferredTimeZone = TimeZoneInfo.Local } };

            // Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, string.Empty, CultureInfo.InvariantCulture, accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
            Assert.That(propertyNotFound, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("current", "")]
        [TestCase("now", "")]
        [TestCase("system", "")]
        [TestCase("utc", "")]
        [TestCase("current", "fr")]
        [TestCase("now", "de")]
        [TestCase("system", "en")]
        [TestCase("utc", "it")]
        public void DateTimePropertyAccess_GetProperty_Returns_Correct_String_For_Culture(string propertyName, string cultureName)
        {
			Logger.LogDebug(">>> Entering DateTimePropertyAccess_GetProperty_Returns_Correct_String_For_Culture(propertyName={PropertyName},cultureName={CultureName})", propertyName, cultureName);
            // Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var accessingUser = new UserInfo { Profile = new UserProfile { PreferredTimeZone = TimeZoneInfo.Local } };
			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Returns_Correct_String_For_Culture(propertyName={PropertyName},cultureName={CultureName}): dtPropertyAccess → {DtPropertyAccess}", propertyName, cultureName, dtPropertyAccess);
            var culture = new CultureInfo(cultureName);

			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Returns_Correct_String_For_Culture(propertyName={PropertyName},cultureName={CultureName}): culture → {Culture}", propertyName, cultureName, culture);
            string expected = string.Empty;

			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Returns_Correct_String_For_Culture(propertyName={PropertyName},cultureName={CultureName}): expected → {Expected}", propertyName, cultureName, expected);
            switch (propertyName)
            {
                case "current":
                    expected = DateTime.Now.ToString("D", culture);
                    break;
                case "now":
                    expected = DateTime.Now.ToString("g", culture);
                    break;
                case "system":
                    expected = DateTime.Now.ToString("g", culture);
                    break;
                case "utc":
                    expected = DateTime.Now.ToUniversalTime().ToString("g", culture);
                    break;
            }

            // Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, string.Empty, culture, accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
            Assert.That(propertyValue, Is.EqualTo(expected));
			Logger.LogDebug("<<< Exiting DateTimePropertyAccess_GetProperty_Returns_Correct_String_For_Culture(propertyName={PropertyName},cultureName={CultureName})", propertyName, cultureName);
        }

        [Test]
        [TestCase("current", "", "D")]
        [TestCase("now", "", "g")]
        [TestCase("system", "", "g")]
        [TestCase("utc", "", "g")]
        [TestCase("current", "fr", "g")]
        [TestCase("now", "de", "mmm yyyy")]
        [TestCase("system", "en", "dd/mm/yy")]
        [TestCase("utc", "it", "mmm dd, yyyy")]
        public void DateTimePropertyAccess_GetProperty_Returns_Correct_String_Given_Format_And_Culture(string propertyName, string cultureName, string format)
        {
			Logger.LogDebug(">>> Entering DateTimePropertyAccess_GetProperty_Returns_Correct_String_Given_Format_And_Culture(propertyName={PropertyName},cultureName={CultureName},format={Format})", propertyName, cultureName, format);
            // Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var accessingUser = new UserInfo { Profile = new UserProfile { PreferredTimeZone = TimeZoneInfo.Local } };
			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Returns_Correct_String_Given_Format_And_Culture(propertyName={PropertyName},cultureName={CultureName},format={Format}): dtPropertyAccess → {DtPropertyAccess}", propertyName, cultureName, format, dtPropertyAccess);
            var culture = new CultureInfo(cultureName);

			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Returns_Correct_String_Given_Format_And_Culture(propertyName={PropertyName},cultureName={CultureName},format={Format}): culture → {Culture}", propertyName, cultureName, format, culture);
            string expected = string.Empty;

			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Returns_Correct_String_Given_Format_And_Culture(propertyName={PropertyName},cultureName={CultureName},format={Format}): expected → {Expected}", propertyName, cultureName, format, expected);
            switch (propertyName)
            {
                case "current":
                    expected = DateTime.Now.ToString(format, culture);
                    break;
                case "now":
                    expected = DateTime.Now.ToString(format, culture);
                    break;
                case "system":
                    expected = DateTime.Now.ToString(format, culture);
                    break;
                case "utc":
                    expected = DateTime.Now.ToUniversalTime().ToString(format, culture);
                    break;
            }

            // Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, format, culture, accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
            Assert.That(propertyValue, Is.EqualTo(expected));
			Logger.LogDebug("<<< Exiting DateTimePropertyAccess_GetProperty_Returns_Correct_String_Given_Format_And_Culture(propertyName={PropertyName},cultureName={CultureName},format={Format})", propertyName, cultureName, format);
        }

        [Test]
        [TestCase("current", "Tokyo Standard Time")]
        [TestCase("now", "Azores Standard Time")]
        public void DateTimePropertyAccess_GetProperty_Adjusts_For_TimeZone(string propertyName, string timeZoneId)
        {
			Logger.LogDebug(">>> Entering DateTimePropertyAccess_GetProperty_Adjusts_For_TimeZone(propertyName={PropertyName},timeZoneId={TimeZoneId})", propertyName, timeZoneId);
            // Arrange
            var dtPropertyAccess = new DateTimePropertyAccess();
            var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Adjusts_For_TimeZone(propertyName={PropertyName},timeZoneId={TimeZoneId}): dtPropertyAccess → {DtPropertyAccess}", propertyName, timeZoneId, dtPropertyAccess);
			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Adjusts_For_TimeZone(propertyName={PropertyName},timeZoneId={TimeZoneId}): userTimeZone → {UserTimeZone}", propertyName, timeZoneId, userTimeZone);
            var timeZoneProfileProperty = new ProfilePropertyDefinition(Constants.PORTAL_Zero)
            {
                PropertyName = "PreferredTimeZone",
                PropertyValue = timeZoneId,
            };
            var userProfile = new UserProfile();
            userProfile.ProfileProperties.Add(timeZoneProfileProperty);
			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Adjusts_For_TimeZone(propertyName={PropertyName},timeZoneId={TimeZoneId}): userProfile → {UserProfile}", propertyName, timeZoneId, userProfile);

            var accessingUser = new UserInfo { Profile = userProfile };
            var culture = CultureInfo.InvariantCulture;

            string expected = string.Empty;

			Logger.LogDebug("DateTimePropertyAccess_GetProperty_Adjusts_For_TimeZone(propertyName={PropertyName},timeZoneId={TimeZoneId}): expected → {Expected}", propertyName, timeZoneId, expected);
            switch (propertyName)
            {
                case "current":
                    expected = TimeZoneInfo.ConvertTime(DateTime.Now, userTimeZone).ToString("D", culture);
                    break;
                case "now":
                    expected = TimeZoneInfo.ConvertTime(DateTime.Now, userTimeZone).ToString("g", culture);
                    break;
                case "system":
                    expected = DateTime.Now.ToString("g", culture);
                    break;
                case "utc":
                    expected = DateTime.Now.ToUniversalTime().ToString("g", culture);
                    break;
            }

            // Act
            bool propertyNotFound = false;
            string propertyValue = dtPropertyAccess.GetProperty(propertyName, string.Empty, culture, accessingUser, Scope.DefaultSettings, ref propertyNotFound);

            // Assert
            Assert.That(propertyValue, Is.EqualTo(expected));
			Logger.LogDebug("<<< Exiting DateTimePropertyAccess_GetProperty_Adjusts_For_TimeZone(propertyName={PropertyName},timeZoneId={TimeZoneId})", propertyName, timeZoneId);
        }
    }
}
