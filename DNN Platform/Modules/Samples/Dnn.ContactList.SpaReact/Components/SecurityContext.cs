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
			_logger.LogDebug(">>> Entering SecurityContext(objModule={ObjModule},user={User})", objModule, user);
            this.user = user;
            if (user.IsSuperUser)
            {
                CanView = CanEdit = IsAdmin = true;
            }
            else
            {
                IsAdmin = user.IsAdmin;
				_logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): IsAdmin → {IsAdmin}", objModule, user, IsAdmin);
                if (IsAdmin)
                {
                    CanView = CanEdit = true;
                }
                else
                {
                    CanView = ModulePermissionController.CanViewModule(objModule);
                    CanEdit = ModulePermissionController.HasModulePermission(objModule.ModulePermissions, "EDIT");
                }
            }
			_logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): CanEdit → {CanEdit}", objModule, user, CanEdit);
			_logger.LogDebug("<<< Exiting SecurityContext(objModule={ObjModule},user={User})", objModule, user);
        }
    }
}
