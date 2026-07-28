// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;

namespace Dnn.ContactList.SpaReact.Components
{
    public class SecurityContext
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering SecurityContext(objModule={ObjModule},user={User})", objModule, user);
            this.user = user;
            if (user.IsSuperUser)
			Logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): this.user → {Arg2}", objModule, user, this.user);
			Logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): user → {User1}", objModule, user, user);
            {
                CanView = CanEdit = IsAdmin = true;
				Logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): CanView → {CanView}", objModule, user, CanView);
				Logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): CanEdit → {CanEdit}", objModule, user, CanEdit);
            }
            else
            {
                IsAdmin = user.IsAdmin;
				Logger.LogDebug("SecurityContext(objModule={ObjModule},user={User}): IsAdmin → {IsAdmin}", objModule, user, IsAdmin);
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
			Logger.LogDebug("<<< Exiting SecurityContext(objModule={ObjModule},user={User})", objModule, user);
        }
    }
}
