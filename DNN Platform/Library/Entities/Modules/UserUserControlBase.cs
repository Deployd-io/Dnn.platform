// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;

    /// <summary>The UserUserControlBase class defines a custom base class for the User Control.</summary>
    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class UserUserControlBase : UserModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserUserControlBase));
        public delegate void UserCreatedEventHandler(object sender, UserCreatedEventArgs e);

        public delegate void UserDeletedEventHandler(object sender, UserDeletedEventArgs e);

        public delegate void UserRestoredEventHandler(object sender, UserRestoredEventArgs e);

        public delegate void UserRemovedEventHandler(object sender, UserRemovedEventArgs e);

        public delegate void UserUpdateErrorEventHandler(object sender, UserUpdateErrorArgs e);

        public event UserCreatedEventHandler UserCreated;

        public event UserCreatedEventHandler UserCreateCompleted;

        public event UserDeletedEventHandler UserDeleted;

        public event UserUpdateErrorEventHandler UserDeleteError;

        public event UserRestoredEventHandler UserRestored;

        public event UserUpdateErrorEventHandler UserRestoreError;

        public event UserRemovedEventHandler UserRemoved;

        public event UserUpdateErrorEventHandler UserRemoveError;

        public event EventHandler UserUpdated;

        public event EventHandler UserUpdateCompleted;

        public event UserUpdateErrorEventHandler UserUpdateError;

        /// <inheritdoc />
        protected override bool AddUser => !this.Request.IsAuthenticated || base.AddUser;

        /// <summary>Raises the <see cref="UserCreateCompleted"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserCreateCompleted(UserCreatedEventArgs e)
        {
			Logger.LogError(">>> Entering OnUserCreateCompleted(e={E})", e);
            if (this.UserCreateCompleted != null)
            {
                this.UserCreateCompleted(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserCreateCompleted(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserCreated"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserCreated(UserCreatedEventArgs e)
        {
			Logger.LogError(">>> Entering OnUserCreated(e={E})", e);
            if (this.UserCreated != null)
            {
                this.UserCreated(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserCreated(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserDeleted"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserDeleted(UserDeletedEventArgs e)
        {
			Logger.LogError(">>> Entering OnUserDeleted(e={E})", e);
            if (this.UserDeleted != null)
            {
                this.UserDeleted(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserDeleted(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserDeleteError"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserDeleteError(UserUpdateErrorArgs e)
        {
			Logger.LogError(">>> Entering OnUserDeleteError(e={E})", e);
            if (this.UserDeleteError != null)
            {
                this.UserDeleteError(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserDeleteError(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserRestored"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserRestored(UserRestoredEventArgs e)
        {
			Logger.LogError(">>> Entering OnUserRestored(e={E})", e);
            if (this.UserRestored != null)
            {
                this.UserRestored(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserRestored(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserRestoreError"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserRestoreError(UserUpdateErrorArgs e)
        {
			Logger.LogError(">>> Entering OnUserRestoreError(e={E})", e);
            if (this.UserRestoreError != null)
            {
                this.UserRestoreError(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserRestoreError(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserRemoved"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserRemoved(UserRemovedEventArgs e)
        {
			Logger.LogError(">>> Entering OnUserRemoved(e={E})", e);
            if (this.UserRemoved != null)
            {
                this.UserRemoved(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserRemoved(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserRemoveError"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserRemoveError(UserUpdateErrorArgs e)
        {
			Logger.LogError(">>> Entering OnUserRemoveError(e={E})", e);
            if (this.UserRemoveError != null)
            {
                this.UserRemoveError(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserRemoveError(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserUpdated"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserUpdated(EventArgs e)
        {
			Logger.LogError(">>> Entering OnUserUpdated(e={E})", e);
            if (this.UserUpdated != null)
            {
                this.UserUpdated(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserUpdated(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserUpdateCompleted"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserUpdateCompleted(EventArgs e)
        {
			Logger.LogError(">>> Entering OnUserUpdateCompleted(e={E})", e);
            if (this.UserUpdateCompleted != null)
            {
                this.UserUpdateCompleted(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserUpdateCompleted(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserUpdateError"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserUpdateError(UserUpdateErrorArgs e)
        {
			Logger.LogError(">>> Entering OnUserUpdateError(e={E})", e);
            if (this.UserUpdateError != null)
            {
                this.UserUpdateError(this, e);
            }
			Logger.LogWarning("<<< Exiting OnUserUpdateError(e={E})", e);
        }

        /// <summary>The BaseUserEventArgs class provides a base for User EventArgs classes.</summary>
        [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
        public class BaseUserEventArgs
        {
            /// <summary>Gets or sets the ID of the User.</summary>
            public int UserId { get; set; }

            /// <summary>Gets or sets the ID of the User.</summary>
            public string UserName { get; set; }
        }

        /// <summary>
        /// The UserCreatedEventArgs class provides a customised EventArgs class for
        /// the UserCreated Event.
        /// </summary>
        public class UserCreatedEventArgs
        {
            private UserCreateStatus createStatus = UserCreateStatus.Success;

            /// <summary>
            /// Initializes a new instance of the <see cref="UserCreatedEventArgs"/> class.
            /// Constructs a new UserCreatedEventArgs.
            /// </summary>
            /// <param name="newUser">The newly Created User.</param>
            public UserCreatedEventArgs(UserInfo newUser)
            {
				Logger.LogError(">>> Entering UserCreatedEventArgs(newUser={NewUser})", newUser);
                this.NewUser = newUser;
            }

            /// <summary>Gets or sets the Create Status.</summary>
            public UserCreateStatus CreateStatus
            {
                get
                {
                    return this.createStatus;
                }

                set
                {
                    this.createStatus = value;
                }
            }

            /// <summary>Gets or sets the New User.</summary>
            public UserInfo NewUser { get; set; }

            /// <summary>Gets or sets a value indicating whether to Notify the new User of the Creation.</summary>
            public bool Notify { get; set; }
        }

        /// <summary>
        /// The UserDeletedEventArgs class provides a customised EventArgs class for
        /// the UserDeleted Event.
        /// </summary>
        public class UserDeletedEventArgs : BaseUserEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserDeletedEventArgs"/> class.
            /// Constructs a new UserDeletedEventArgs.
            /// </summary>
            /// <param name="id">The Id of the User.</param>
            /// <param name="name">The user name of the User.</param>
            public UserDeletedEventArgs(int id, string name)
            {
				Logger.LogError(">>> Entering UserDeletedEventArgs(id={Id},name={Name})", id, name);
                this.UserId = id;
				Logger.LogDebug("UserDeletedEventArgs(id={Id},name={Name}): this.UserId → {Arg2}", id, name, this.UserId);
            Logger.LogWarning("<<< Exiting UserDeletedEventArgs(id={Id},name={Name})", id, name);
                this.UserName = name;
            }
        }

        /// <summary>
        /// The UserRestoredEventArgs class provides a customised EventArgs class for
        /// the UserRestored Event.
        /// </summary>
        public class UserRestoredEventArgs : BaseUserEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserRestoredEventArgs"/> class.
            /// Constructs a new UserRestoredEventArgs.
            /// </summary>
            /// <param name="id">The Id of the User.</param>
            /// <param name="name">The user name of the User.</param>
            public UserRestoredEventArgs(int id, string name)
            {
				Logger.LogError(">>> Entering UserRestoredEventArgs(id={Id},name={Name})", id, name);
                this.UserId = id;
				Logger.LogDebug("UserRestoredEventArgs(id={Id},name={Name}): this.UserId → {Arg2}", id, name, this.UserId);
            Logger.LogWarning("<<< Exiting UserRestoredEventArgs(id={Id},name={Name})", id, name);
                this.UserName = name;
            }
        }

        /// <summary>
        /// The UserRemovedEventArgs class provides a customised EventArgs class for
        /// the UserRemoved Event.
        /// </summary>
        public class UserRemovedEventArgs : BaseUserEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserRemovedEventArgs"/> class.
            /// Constructs a new UserRemovedEventArgs.
            /// </summary>
            /// <param name="id">The Id of the User.</param>
            /// <param name="name">The user name of the User.</param>
            public UserRemovedEventArgs(int id, string name)
            {
				Logger.LogError(">>> Entering UserRemovedEventArgs(id={Id},name={Name})", id, name);
                this.UserId = id;
				Logger.LogDebug("UserRemovedEventArgs(id={Id},name={Name}): this.UserId → {Arg2}", id, name, this.UserId);
            Logger.LogWarning("<<< Exiting UserRemovedEventArgs(id={Id},name={Name})", id, name);
                this.UserName = name;
            }
        }

        /// <summary>
        /// The UserUpdateErrorArgs class provides a customised EventArgs class for
        /// the UserUpdateError Event.
        /// </summary>
        public class UserUpdateErrorArgs : BaseUserEventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserUpdateErrorArgs"/> class.
            /// Constructs a new UserUpdateErrorArgs.
            /// </summary>
            /// <param name="id">The Id of the User.</param>
            /// <param name="name">The user name of the User.</param>
            /// <param name="message">The error message.</param>
            public UserUpdateErrorArgs(int id, string name, string message)
            {
				Logger.LogError(">>> Entering UserUpdateErrorArgs(id={Id},name={Name},message={Message})", id, name, message);
				Logger.LogDebug("UserUpdateErrorArgs(id={Id},name={Name},message={Message}): this.UserName → {Arg3}", id, name, message, this.UserName);
                this.UserId = id;
                this.UserName = name;
				Logger.LogDebug("UserUpdateErrorArgs(id={Id},name={Name},message={Message}): this.UserId → {Arg3}", id, name, message, this.UserId);
                this.Message = message;
            }

            /// <summary>Gets or sets the error message.</summary>
            public string Message { get; set; }
        }
    }
}
