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
			Logger.LogDebug(">>> Entering OnUserCreateCompleted(e={E})", e);
            if (this.UserCreateCompleted != null)
            {
                this.UserCreateCompleted(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserCreateCompleted(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserCreated"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserCreated(UserCreatedEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserCreated(e={E})", e);
            if (this.UserCreated != null)
            {
                this.UserCreated(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserCreated(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserDeleted"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserDeleted(UserDeletedEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserDeleted(e={E})", e);
            if (this.UserDeleted != null)
            {
                this.UserDeleted(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserDeleted(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserDeleteError"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserDeleteError(UserUpdateErrorArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserDeleteError(e={E})", e);
            if (this.UserDeleteError != null)
            {
                this.UserDeleteError(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserDeleteError(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserRestored"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserRestored(UserRestoredEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserRestored(e={E})", e);
            if (this.UserRestored != null)
            {
                this.UserRestored(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserRestored(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserRestoreError"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserRestoreError(UserUpdateErrorArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserRestoreError(e={E})", e);
            if (this.UserRestoreError != null)
            {
                this.UserRestoreError(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserRestoreError(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserRemoved"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserRemoved(UserRemovedEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserRemoved(e={E})", e);
            if (this.UserRemoved != null)
            {
                this.UserRemoved(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserRemoved(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserRemoveError"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserRemoveError(UserUpdateErrorArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserRemoveError(e={E})", e);
            if (this.UserRemoveError != null)
            {
                this.UserRemoveError(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserRemoveError(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserUpdated"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserUpdated(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserUpdated(e={E})", e);
            if (this.UserUpdated != null)
            {
                this.UserUpdated(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserUpdated(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserUpdateCompleted"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserUpdateCompleted(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserUpdateCompleted(e={E})", e);
            if (this.UserUpdateCompleted != null)
            {
                this.UserUpdateCompleted(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserUpdateCompleted(e={E})", e);
        }

        /// <summary>Raises the <see cref="UserUpdateError"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnUserUpdateError(UserUpdateErrorArgs e)
        {
			Logger.LogDebug(">>> Entering OnUserUpdateError(e={E})", e);
            if (this.UserUpdateError != null)
            {
                this.UserUpdateError(this, e);
            }
			Logger.LogDebug("<<< Exiting OnUserUpdateError(e={E})", e);
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
				Logger.LogDebug(">>> Entering UserCreatedEventArgs(newUser={NewUser})", newUser);
                this.NewUser = newUser;
				Logger.LogDebug("UserCreatedEventArgs(newUser={NewUser}): this.NewUser → {Arg1}", newUser, this.NewUser);
				Logger.LogDebug("<<< Exiting UserCreatedEventArgs(newUser={NewUser})", newUser);
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
				Logger.LogDebug(">>> Entering UserDeletedEventArgs(id={Id},name={Name})", id, name);
                this.UserId = id;
                this.UserName = name;
				Logger.LogDebug("UserDeletedEventArgs(id={Id},name={Name}): this.UserId → {Arg2}", id, name, this.UserId);
				Logger.LogDebug("<<< Exiting UserDeletedEventArgs(id={Id},name={Name})", id, name);
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
				Logger.LogDebug(">>> Entering UserRestoredEventArgs(id={Id},name={Name})", id, name);
                this.UserId = id;
                this.UserName = name;
				Logger.LogDebug("UserRestoredEventArgs(id={Id},name={Name}): this.UserId → {Arg2}", id, name, this.UserId);
				Logger.LogDebug("<<< Exiting UserRestoredEventArgs(id={Id},name={Name})", id, name);
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
				Logger.LogDebug(">>> Entering UserRemovedEventArgs(id={Id},name={Name})", id, name);
                this.UserId = id;
                this.UserName = name;
				Logger.LogDebug("UserRemovedEventArgs(id={Id},name={Name}): this.UserId → {Arg2}", id, name, this.UserId);
				Logger.LogDebug("<<< Exiting UserRemovedEventArgs(id={Id},name={Name})", id, name);
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
				Logger.LogDebug(">>> Entering UserUpdateErrorArgs(id={Id},name={Name},message={Message})", id, name, message);
                this.UserId = id;
                this.UserName = name;
				Logger.LogDebug("UserUpdateErrorArgs(id={Id},name={Name},message={Message}): this.UserId → {Arg3}", id, name, message, this.UserId);
				Logger.LogDebug("UserUpdateErrorArgs(id={Id},name={Name},message={Message}): this.UserName → {Arg3}", id, name, message, this.UserName);
                this.Message = message;
				Logger.LogDebug("<<< Exiting UserUpdateErrorArgs(id={Id},name={Name},message={Message})", id, name, message);
            }

            /// <summary>Gets or sets the error message.</summary>
            public string Message { get; set; }
        }
    }
}
