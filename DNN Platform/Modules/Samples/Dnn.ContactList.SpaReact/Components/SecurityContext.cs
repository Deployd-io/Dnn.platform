// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.SpaReact.Components
{
    public class SecurityContext
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool IsAdmin { get; set; }
        private UserInfo user { get; set; }
        public int UserId
        {
            get
            {
                return user.UserID;
            }
        }

        public SecurityContext(ModuleInfo objModule, UserInfo user)
        {
			_logger.LogError(">>> Entering SecurityContext(objModule={ObjModule},user={User})", objModule, user);
            this.user = user;
			_logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): this.user → {Arg2}", objModule, user, this.user);
            if (user.IsSuperUser)
            {
				_logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): user → {User1}", objModule, user, user);
                CanView = CanEdit = IsAdmin = true;
				_logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): CanView → {CanView}", objModule, user, CanView);
            }
            else
            {
				_logger.LogError("SecurityContext(objModule={ObjModule},user={User}): CanView → {CanView}", objModule, user, CanView);
                IsAdmin = user.IsAdmin;
                if (IsAdmin)
                {
					_logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): IsAdmin → {IsAdmin}", objModule, user, IsAdmin);
                    CanView = CanEdit = true;
                }
                else
                {
                    CanView = ModulePermissionController.CanViewModule(objModule);
                    CanEdit = ModulePermissionController.HasModulePermission(objModule.ModulePermissions, "EDIT");
					_logger.LogError("SecurityContext(objModule={ObjModule},user={User}): CanEdit → {CanEdit}", objModule, user, CanEdit);
                }
			_logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): CanEdit → {CanEdit}", objModule, user, CanEdit);
        _logger.LogWarning("<<< Exiting SecurityContext(objModule={ObjModule},user={User})", objModule, user);
            }
        }
    }
}
