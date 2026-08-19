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
			Logger.LogDebug(">>> Entering HasAspNetHostingPermission()");
            GetPermissions();
			Logger.LogDebug("<<< Exiting HasAspNetHostingPermission()");
            return aspNetHostingPermission;
        }

        public static bool HasReflectionPermission()
        {
			Logger.LogDebug(">>> Entering HasReflectionPermission()");
            GetPermissions();
			Logger.LogDebug("<<< Exiting HasReflectionPermission()");
            return reflectionPermission;
        }

        public static bool HasWebPermission()
        {
			Logger.LogDebug(">>> Entering HasWebPermission()");
            GetPermissions();
			Logger.LogDebug("<<< Exiting HasWebPermission()");
            return webPermission;
        }

        public static bool HasUnManagedCodePermission()
        {
			Logger.LogDebug(">>> Entering HasUnManagedCodePermission()");
            GetPermissions();
			Logger.LogDebug("<<< Exiting HasUnManagedCodePermission()");
            return unManagedCodePermission;
        }

        public static bool HasPermissions(string permissions, ref string permission)
        {
			Logger.LogDebug(">>> Entering HasPermissions(permissions={Permissions},permission={Permission})", permissions, permission);
            bool hasPermission = true;
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

			Logger.LogDebug("<<< Exiting HasPermissions(permissions={Permissions},permission={Permission})", permissions, permission);
            return hasPermission;
        }

        private static void GetPermissions()
        {
			Logger.LogDebug(">>> Entering GetPermissions()");
            if (!initialized)
            {
                // test RelectionPermission
                CodeAccessPermission securityTest;
                try
                {
                    securityTest = new ReflectionPermission(PermissionState.Unrestricted);
                    securityTest.Demand();
					Logger.LogDebug("GetPermissions(): securityTest → {SecurityTest}", securityTest);
                    reflectionPermission = true;
                }
                catch
                {
					Logger.LogError("Exception in GetPermissions(): {Arg0}", ex.Message);
                    // code access security error
                    reflectionPermission = false;
                }

                // test WebPermission
                try
                {
                    securityTest = new WebPermission(PermissionState.Unrestricted);
                    securityTest.Demand();
                    webPermission = true;
                }
                catch
                {
                    // code access security error
                    webPermission = false;
                }

                // test WebHosting Permission (Full Trust)
                try
                {
                    securityTest = new AspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted);
                    securityTest.Demand();
                    aspNetHostingPermission = true;
                }
                catch
                {
                    // code access security error
                    aspNetHostingPermission = false;
                }

                initialized = true;
				Logger.LogDebug("GetPermissions(): initialized → {Initialized}", initialized);

                // Test for Unmanaged Code permission
                try
                {
                    securityTest = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                    securityTest.Demand();
                    unManagedCodePermission = true;
                }
                catch (Exception)
                {
					Logger.LogError("Exception in GetPermissions(): {Arg0}", Exception.Message);
                    unManagedCodePermission = false;
                }
            }
			Logger.LogDebug("<<< Exiting GetPermissions()");
        }
    }
}
