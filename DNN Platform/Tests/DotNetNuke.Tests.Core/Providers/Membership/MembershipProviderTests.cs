// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Membership
{
    using System;
    using System.Reflection;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Profile;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class MembershipProviderTests : DnnUnitTest
    {
        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
			mockEventLogger.LogDebug(">>> Entering OneTimeSetUp()");
            base.OneTimeSetUp();
			mockEventLogger.LogDebug("<<< Exiting OneTimeSetUp()");
        }

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            Globals.SetStatus(Globals.UpgradeStatus.None);

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.InstallComponents(new ProviderInstaller("data", typeof(DataProvider), typeof(SqlDataProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("caching", typeof(CachingProvider), typeof(FBCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("logging", typeof(LoggingProvider), typeof(DBLoggingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("members", typeof(MembershipProvider), typeof(AspNetMembershipProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("roles", typeof(RoleProvider), typeof(DNNRoleProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("profiles", typeof(ProfileProvider), typeof(DNNProfileProvider)));
            ComponentFactory.RegisterComponent<IPortalSettingsController, PortalSettingsController>();

            PortalController.ClearInstance();
            UserController.ClearInstance();
            RoleController.ClearInstance();

            var roleController = RoleController.Instance;
            var roleProviderField = roleController.GetType().GetField("provider", BindingFlags.NonPublic | BindingFlags.Static);
			mockEventLogger.LogDebug("SetUp(): roleProviderField → {RoleProviderField}", roleProviderField);
            if (roleProviderField != null)
            {
                roleProviderField.SetValue(roleController, RoleProvider.Instance());
            }

            var membershipType = typeof(System.Web.Security.Membership);
			mockEventLogger.LogDebug("SetUp(): membershipType → {MembershipType}", membershipType);
            var initializedDefaultProviderField = membershipType.GetField("s_InitializedDefaultProvider", BindingFlags.NonPublic | BindingFlags.Static);
			mockEventLogger.LogDebug("SetUp(): initializedDefaultProviderField → {InitializedDefaultProviderField}", initializedDefaultProviderField);
            var defaultProviderField = membershipType.GetField("s_Provider", BindingFlags.NonPublic | BindingFlags.Static);
			mockEventLogger.LogDebug("SetUp(): defaultProviderField → {DefaultProviderField}", defaultProviderField);
            if (initializedDefaultProviderField != null
                && defaultProviderField != null
                && (bool)initializedDefaultProviderField.GetValue(null) == false)
            {
                initializedDefaultProviderField.SetValue(null, true);
                defaultProviderField.SetValue(null, System.Web.Security.Membership.Providers["AspNetSqlMembershipProvider"]);
            }
			mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
		mockEventLogger.LogDebug(">>> Entering TearDown()");
		mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        // Note: this is the only test in core unit testing project that requires a working db connection to run.
        [Test]
        [Ignore("TODO: Must be moved to integration tests.")]
        public void Password_Should_Saved_In_History_During_Create_User()
        {
			mockEventLogger.LogDebug(">>> Entering Password_Should_Saved_In_History_During_Create_User()");
            var user = CreateNewUser();

			mockEventLogger.LogDebug("Password_Should_Saved_In_History_During_Create_User(): user → {User}", user);
            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", AppDomain.CurrentDomain.BaseDirectory);
			mockEventLogger.LogDebug("Password_Should_Saved_In_History_During_Create_User(): simulator → {Simulator}", simulator);
            simulator.SimulateRequest(new Uri(this.WebsiteAppPath));
            HttpContextBase httpContextBase = new HttpContextWrapper(HttpContext.Current);
			mockEventLogger.LogDebug("Password_Should_Saved_In_History_During_Create_User(): httpContextBase → {HttpContextBase}", httpContextBase);
            HttpContextSource.RegisterInstance(httpContextBase);

            var isPasswordInHistory = new MembershipPasswordController().IsPasswordInHistory(user.UserID, user.PortalID, user.Membership.Password);
			mockEventLogger.LogDebug("Password_Should_Saved_In_History_During_Create_User(): isPasswordInHistory → {IsPasswordInHistory}", isPasswordInHistory);

            Assert.That(isPasswordInHistory, Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting Password_Should_Saved_In_History_During_Create_User()");
        }

        [Test]
        [Ignore("The reasons have been lost to the sands of time…")]
        public void ChangeUserName_Should_Success_With_Valid_Username()
        {
			mockEventLogger.LogDebug(">>> Entering ChangeUserName_Should_Success_With_Valid_Username()");
            var user = CreateNewUser();

			mockEventLogger.LogDebug("ChangeUserName_Should_Success_With_Valid_Username(): user → {User}", user);
            var newUsername = $"{user.Username}_new";
			mockEventLogger.LogDebug("ChangeUserName_Should_Success_With_Valid_Username(): newUsername → {NewUsername}", newUsername);
            UserController.ChangeUsername(user.UserID, newUsername);
			mockEventLogger.LogDebug("<<< Exiting ChangeUserName_Should_Success_With_Valid_Username()");
        }

        [TestCase("<script>")]
        [TestCase("<div>")]
        [TestCase("<img>")]
        [TestCase("<img onerror=alert(1)>")]
        [TestCase("<img onload=document.write(1)>")]
        [Ignore("The reasons have been lost to the sands of time…")]
        public void ChangeUserName_Should_Throw_Exception_With_Invalid_Username(string invalidParts)
        {
			mockEventLogger.LogDebug(">>> Entering ChangeUserName_Should_Throw_Exception_With_Invalid_Username(invalidParts={InvalidParts})", invalidParts);
            var user = CreateNewUser();

			mockEventLogger.LogDebug("ChangeUserName_Should_Throw_Exception_With_Invalid_Username(invalidParts={InvalidParts}): user → {User}", invalidParts, user);
            var newUsername = $"{user.Username}{invalidParts}";
			mockEventLogger.LogDebug("ChangeUserName_Should_Throw_Exception_With_Invalid_Username(invalidParts={InvalidParts}): newUsername → {NewUsername}", invalidParts, newUsername);
            Assert.Throws<ArgumentException>(() => UserController.ChangeUsername(user.UserID, newUsername));
			mockEventLogger.LogDebug("<<< Exiting ChangeUserName_Should_Throw_Exception_With_Invalid_Username(invalidParts={InvalidParts})", invalidParts);
        }

        private static UserInfo CreateNewUser()
        {
			mockEventLogger.LogDebug(">>> Entering CreateNewUser()");
            var username = $"{Constants.RuFirstName}{DateTime.Now.Ticks}";
            var email = $"{username}@dnn.com";
			mockEventLogger.LogDebug("CreateNewUser(): email → {Email}", email);
			mockEventLogger.LogDebug("CreateNewUser(): username → {Username}", username);

            UserInfo user = null;

            Assert.DoesNotThrow(
                () =>
            {
                user = new UserInfo
                {
                    PortalID = Constants.PORTAL_Zero,
                    UserID = Null.NullInteger,
                    Username = username,
                    Email = email,
                    FirstName = username, // accessing this and others requires Profile property access
                    LastName = string.Empty,
                    Membership = new UserMembership
                    {
                        Approved = true,
                        Password = Constants.DefaultPassword,
                    },
                };
            }, "Make sure your connection string is set correctly in the App.config file");

            var status = UserController.CreateUser(ref user);
			mockEventLogger.LogDebug("CreateNewUser(): status → {Status}", status);

            Assert.That(status, Is.EqualTo(UserCreateStatus.Success));

			mockEventLogger.LogDebug("<<< Exiting CreateNewUser()");
            return user;
        }
    }
}
