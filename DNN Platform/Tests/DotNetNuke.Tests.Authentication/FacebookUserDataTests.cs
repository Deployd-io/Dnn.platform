// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Authentication
{
    using System;

    using DotNetNuke.Authentication.Facebook.Components;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Authentication.OAuth;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class FacebookUserDataTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string SampleUserJson = @"{
    ""id"": ""220439"",
    ""name"": ""Bret the Taylor"",
    ""first_name"": ""Bret"",
    ""last_name"": ""Taylor"",
    ""link"": ""https://www.facebook.com/btaylor"",
    ""username"": ""btaylor"",
    ""gender"": ""male"",
    ""locale"": ""en_US""
}";

        [Test]
        public void FacebookUserData_Populates_Inherited_Name_Properties_When_Deserialized()
        {
			_logger.LogDebug(">>> Entering FacebookUserData_Populates_Inherited_Name_Properties_When_Deserialized()");
            // Act
            UserData sampleUser = Json.Deserialize<FacebookUserData>(SampleUserJson);
			_logger.LogDebug("FacebookUserData_Populates_Inherited_Name_Properties_When_Deserialized(): sampleUser → {SampleUser}", sampleUser);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(sampleUser.FirstName, Is.EqualTo("Bret"), "Should correctly pull first name from first_name field, not by parsing name");
                Assert.That(sampleUser.LastName, Is.EqualTo("Taylor"), "Should correctly pull first name from first_name field, not by parsing name");
            }
			_logger.LogDebug("<<< Exiting FacebookUserData_Populates_Inherited_Name_Properties_When_Deserialized()");
        }

        [Test]
        public void FacebookUserData_Populates_Link_Property_When_Deserialized()
        {
			_logger.LogDebug(">>> Entering FacebookUserData_Populates_Link_Property_When_Deserialized()");
            // Act
            var sampleUser = Json.Deserialize<FacebookUserData>(SampleUserJson);
			_logger.LogDebug("FacebookUserData_Populates_Link_Property_When_Deserialized(): sampleUser → {SampleUser}", sampleUser);

            // Assert
            Assert.That(sampleUser.Link, Is.EqualTo(new Uri("https://www.facebook.com/btaylor", UriKind.Absolute)));
			_logger.LogDebug("<<< Exiting FacebookUserData_Populates_Link_Property_When_Deserialized()");
        }
    }
}
