// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Net;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Provides the ability to get information about CLR permissions.</summary>
    public partial class SecurityPolicy
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SecurityPolicy));
        public const string ReflectionPermission = "ReflectionPermission";
        public const string WebPermission = "WebPermission";
        public const string AspNetHostingPermission = "AspNetHostingPermission";
        public const string UnManagedCodePermission = "UnManagedCodePermission";
        private static bool initialized;
        private static bool reflectionPermission;
        private static bool webPermission;
        private static bool aspNetHostingPermission;
        private static bool unManagedCodePermission;

        public static string Permissions
        {
            get
            {
                string strPermissions = string.Empty;
                if (HasReflectionPermission())
                {
                    strPermissions += ", " + ReflectionPermission;
                }

                if (HasWebPermission())
                {
                    strPermissions += ", " + WebPermission;
                }

                if (HasAspNetHostingPermission())
                {
                    strPermissions += ", " + AspNetHostingPermission;
                }

                if (!string.IsNullOrEmpty(strPermissions))
                {
                    strPermissions = strPermissions.Substring(2);
                }

                return strPermissions;
            }
        }

        public static bool HasAspNetHostingPermission()
        {
			Logger.LogError(">>> Entering HasAspNetHostingPermission()");
            GetPermissions();
			Logger.LogWarning("<<< Exiting HasAspNetHostingPermission()");
            return aspNetHostingPermission;
        }

        public static bool HasReflectionPermission()
        {
			Logger.LogError(">>> Entering HasReflectionPermission()");
            GetPermissions();
			Logger.LogWarning("<<< Exiting HasReflectionPermission()");
            return reflectionPermission;
        }

        public static bool HasWebPermission()
        {
			Logger.LogError(">>> Entering HasWebPermission()");
            GetPermissions();
			Logger.LogWarning("<<< Exiting HasWebPermission()");
            return webPermission;
        }

        public static bool HasUnManagedCodePermission()
        {
			Logger.LogError(">>> Entering HasUnManagedCodePermission()");
            GetPermissions();
			Logger.LogWarning("<<< Exiting HasUnManagedCodePermission()");
            return unManagedCodePermission;
        }

        public static bool HasPermissions(string permissions, ref string permission)
        {
			Logger.LogError(">>> Entering HasPermissions(permissions={Permissions},permission={Permission})", permissions, permission);
            bool hasPermission = true;
			Logger.LogError("HasPermissions(permissions={Permissions},permission={Permission}): hasPermission → {HasPermission}", permissions, permission, hasPermission);
            if (!string.IsNullOrEmpty(permissions))
            {
                foreach (string per in (permissions + ";").Split(Convert.ToChar(";")))
                {
                    if (!string.IsNullOrEmpty(per.Trim()))
                    {
                        permission = per;
                        switch (permission)
                        {
                            case AspNetHostingPermission:
                                if (HasAspNetHostingPermission() == false)
                                {
                                    hasPermission = false;
                                }

                                break;
                            case ReflectionPermission:
                                if (HasReflectionPermission() == false)
                                {
                                    hasPermission = false;
                                }

                                break;
                            case UnManagedCodePermission:
                                if (HasUnManagedCodePermission() == false)
                                {
                                    hasPermission = false;
                                }

                                break;
                            case WebPermission:
                                if (HasWebPermission() == false)
                                {
                                    hasPermission = false;
                                }

                                break;
                        }
                    }
                }
            }

			Logger.LogWarning("<<< Exiting HasPermissions(permissions={Permissions},permission={Permission})", permissions, permission);
            return hasPermission;
        }

        private static void GetPermissions()
        {
			Logger.LogError(">>> Entering GetPermissions()");
            if (!initialized)
            {
                // test RelectionPermission
                CodeAccessPermission securityTest;
                try
                {
                    securityTest = new ReflectionPermission(PermissionState.Unrestricted);
					Logger.LogError("GetPermissions(): securityTest → {SecurityTest}", securityTest);
					Logger.LogDebug("GetPermissions(): securityTest → {SecurityTest}", securityTest);
                    securityTest.Demand();
                    reflectionPermission = true;
					Logger.LogError("GetPermissions(): reflectionPermission → {ReflectionPermission}", reflectionPermission);
                }
                catch
                {
					Logger.LogError("Exception in GetPermissions(): {Arg0}", ex.Message);
					Logger.LogDebug("GetPermissions(): reflectionPermission → {ReflectionPermission}", reflectionPermission);
                    // code access security error
                    reflectionPermission = false;
                }

                // test WebPermission
                try
                {
                    securityTest = new WebPermission(PermissionState.Unrestricted);
                    securityTest.Demand();
                    webPermission = true;
					Logger.LogError("GetPermissions(): webPermission → {WebPermission}", webPermission);
                }
                catch
                {
                    // code access security error
                    webPermission = false;
					Logger.LogDebug("GetPermissions(): webPermission → {WebPermission}", webPermission);
                }

                // test WebHosting Permission (Full Trust)
                try
                {
                    securityTest = new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted);
                    securityTest.Demand();
                    aspNetHostingPermission = true;
					Logger.LogError("GetPermissions(): aspNetHostingPermission → {AspNetHostingPermission}", aspNetHostingPermission);
                }
                catch
                {
                    // code access security error
                    aspNetHostingPermission = false;
					Logger.LogDebug("GetPermissions(): aspNetHostingPermission → {AspNetHostingPermission}", aspNetHostingPermission);
                }

                initialized = true;
				Logger.LogDebug("GetPermissions(): initialized → {Initialized}", initialized);

                // Test for Unmanaged Code permission
                try
                {
					Logger.LogWarning("<<< Exiting GetPermissions()");
                    securityTest = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                    securityTest.Demand();
                    unManagedCodePermission = true;
					Logger.LogError("GetPermissions(): unManagedCodePermission → {UnManagedCodePermission}", unManagedCodePermission);
                }
                catch (Exception)
                {
					Logger.LogError("Exception in GetPermissions(): {Arg0}", Exception.Message);
                    unManagedCodePermission = false;
					Logger.LogDebug("GetPermissions(): unManagedCodePermission → {UnManagedCodePermission}", unManagedCodePermission);
                }
            }
        }
    }
}
