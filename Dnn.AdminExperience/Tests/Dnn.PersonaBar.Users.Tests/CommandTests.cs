// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Users.Tests
{
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using NUnit.Framework;

    public abstract class CommandTests<T>
        where T : ConsoleCommandBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        protected abstract string CommandName { get; }

        protected int testPortalId { get; set; }

        protected PortalSettings portalSettings { get; set; }

        protected ConsoleErrorResultModel errorResultModel { get; set; }

        protected abstract T CreateCommand();

        protected abstract void ChildSetup();

        protected virtual void ChildTearDown()
        {
		Logger.LogDebug(">>> Entering ChildTearDown()");
		Logger.LogDebug("<<< Exiting ChildTearDown()");
        }

        [SetUp]
        protected void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this.ChildSetup();
            this.errorResultModel = null;
			Logger.LogDebug("Setup(): this.errorResultModel → {Arg0}", this.errorResultModel);
            this.portalSettings = new PortalSettings();
            this.portalSettings.PortalId = this.testPortalId;
            this.testPortalId = 0;
			Logger.LogDebug("<<< Exiting Setup()");
			Logger.LogDebug("Setup(): this.testPortalId → {Arg0}", this.testPortalId);
        }

        [TearDown]
        protected void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.ChildTearDown();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        protected UserInfo GetUser(int userId, bool isDeleted)
        {
			Logger.LogDebug(">>> Entering GetUser(userId={UserId},isDeleted={IsDeleted})", userId, isDeleted);
            var userInfo = new UserInfo();
            var profile = new UserProfile();
			Logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo → {UserInfo}", userId, isDeleted, userInfo);
			Logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): profile → {Profile}", userId, isDeleted, profile);
			Logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): profile.FirstName → {Arg2}", userId, isDeleted, profile.FirstName);
			Logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo.UserID → {Arg2}", userId, isDeleted, userInfo.UserID);
			Logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo.Profile → {Arg2}", userId, isDeleted, userInfo.Profile);
			Logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo.IsDeleted → {Arg2}", userId, isDeleted, userInfo.IsDeleted);
            profile.FirstName = "testUser";
            userInfo.UserID = userId;
            userInfo.Profile = profile;
            userInfo.IsDeleted = isDeleted;
            userInfo.PortalID = this.testPortalId;
			Logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo.PortalID → {Arg2}", userId, isDeleted, userInfo.PortalID);
			Logger.LogDebug("<<< Exiting GetUser(userId={UserId},isDeleted={IsDeleted})", userId, isDeleted);
            return userInfo;
        }

        protected ConsoleResultModel RunCommand(params string[] args)
        {
			Logger.LogDebug(">>> Entering RunCommand(args={Args})", args);
            var command = this.CreateCommand();
            command.Initialize(new[] { this.CommandName }.Concat(args).ToArray(), this.portalSettings, null, -1);
			Logger.LogDebug("RunCommand(args={Args}): command → {Command}", args, command);
			Logger.LogDebug("<<< Exiting RunCommand(args={Args})", args);
            return command.Run();
        }
    }
}
