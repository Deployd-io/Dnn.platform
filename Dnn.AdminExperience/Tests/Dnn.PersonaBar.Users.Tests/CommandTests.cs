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
	using Microsoft.Extensions.Logging;

    public abstract class CommandTests<T>
        where T : ConsoleCommandBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        protected abstract string CommandName { get; }

        protected int testPortalId { get; set; }

        protected PortalSettings portalSettings { get; set; }

        protected ConsoleErrorResultModel errorResultModel { get; set; }

        protected abstract T CreateCommand();

        protected abstract void ChildSetup();

        protected virtual void ChildTearDown()
        {
		_logger.LogDebug(">>> Entering ChildTearDown()");
		_logger.LogDebug("<<< Exiting ChildTearDown()");
        }

        [SetUp]
        protected void Setup()
        {
			_logger.LogDebug(">>> Entering Setup()");
            this.ChildSetup();
            this.errorResultModel = null;
			_logger.LogDebug("Setup(): this.errorResultModel → {Arg0}", this.errorResultModel);
            this.portalSettings = new PortalSettings();
            this.portalSettings.PortalId = this.testPortalId;
            this.testPortalId = 0;
			_logger.LogDebug("Setup(): this.testPortalId → {Arg0}", this.testPortalId);
			_logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        protected void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this.ChildTearDown();
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        protected UserInfo GetUser(int userId, bool isDeleted)
        {
			_logger.LogDebug(">>> Entering GetUser(userId={UserId},isDeleted={IsDeleted})", userId, isDeleted);
            var userInfo = new UserInfo();
            var profile = new UserProfile();
			_logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): profile → {Profile}", userId, isDeleted, profile);
			_logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): profile.FirstName → {Arg2}", userId, isDeleted, profile.FirstName);
			_logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo.UserID → {Arg2}", userId, isDeleted, userInfo.UserID);
			_logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo.Profile → {Arg2}", userId, isDeleted, userInfo.Profile);
			_logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo.IsDeleted → {Arg2}", userId, isDeleted, userInfo.IsDeleted);
			_logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo.PortalID → {Arg2}", userId, isDeleted, userInfo.PortalID);
			_logger.LogDebug("GetUser(userId={UserId},isDeleted={IsDeleted}): userInfo → {UserInfo}", userId, isDeleted, userInfo);
            profile.FirstName = "testUser";
            userInfo.UserID = userId;
            userInfo.Profile = profile;
            userInfo.IsDeleted = isDeleted;
            userInfo.PortalID = this.testPortalId;
			_logger.LogDebug("<<< Exiting GetUser(userId={UserId},isDeleted={IsDeleted})", userId, isDeleted);
            return userInfo;
        }

        protected ConsoleResultModel RunCommand(params string[] args)
        {
			_logger.LogDebug(">>> Entering RunCommand(args={Args})", args);
            var command = this.CreateCommand();
            command.Initialize(new[] { this.CommandName }.Concat(args).ToArray(), this.portalSettings, null, -1);
			_logger.LogDebug("RunCommand(args={Args}): command → {Command}", args, command);
			_logger.LogDebug("<<< Exiting RunCommand(args={Args})", args);
            return command.Run();
        }
    }
}
