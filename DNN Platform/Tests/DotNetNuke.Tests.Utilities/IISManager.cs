//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace DotNetNuke.Tests.UI.WatiN.Utilities;

using System;
using System.Diagnostics;
using Microsoft.Web.Administration;

public static class IISManager
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    public static void CreateIISApplication(string appName, string physicalPath)
    {
		Logger.LogDebug(">>> Entering CreateIISApplication(appName={AppName},physicalPath={PhysicalPath})", appName, physicalPath);
        ServerManager iisManager = new ServerManager();
        Site defaultSite = iisManager.Sites["Default Web Site"];
		Logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath}): iisManager → {IisManager}", appName, physicalPath, iisManager);
		Logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath}): defaultSite → {DefaultSite}", appName, physicalPath, defaultSite);
        Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

		Logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath}): app → {App}", appName, physicalPath, app);
        // Remove application if it already exists
        if (app != null)
        {
            defaultSite.Applications.Remove(app);
            iisManager.CommitChanges();
        }

        defaultSite = iisManager.Sites["Default Web Site"];
        defaultSite.Applications.Add(String.Format("/{0}", appName), physicalPath);
        iisManager.CommitChanges();
        iisManager.Dispose();
		Logger.LogDebug("<<< Exiting CreateIISApplication(appName={AppName},physicalPath={PhysicalPath})", appName, physicalPath);
    }

    public static void CreateIISApplication(string appName, string physicalPath, string appPool)
    {
		Logger.LogDebug(">>> Entering CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool})", appName, physicalPath, appPool);
        ServerManager iisManager = new ServerManager();
		Logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool}): iisManager → {IisManager}", appName, physicalPath, appPool, iisManager);
        Site defaultSite = iisManager.Sites["Default Web Site"];
        Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];
		Logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool}): defaultSite → {DefaultSite}", appName, physicalPath, appPool, defaultSite);
		Logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool}): app → {App}", appName, physicalPath, appPool, app);

        // Remove application if it already exists
        if (app != null)
        {
            defaultSite.Applications.Remove(app);
            iisManager.CommitChanges();
        }

        defaultSite.Applications.Add(String.Format("/{0}", appName), physicalPath);
        SetApplicationPool(appName, appPool);
        iisManager.CommitChanges();
        iisManager.Dispose();
		Logger.LogDebug("<<< Exiting CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool})", appName, physicalPath, appPool);
    }

    public static void CreateIISWebSite(string siteName, string siteAddress, string physicalPath)
    {
		Logger.LogDebug(">>> Entering CreateIISWebSite(siteName={SiteName},siteAddress={SiteAddress},physicalPath={PhysicalPath})", siteName, siteAddress, physicalPath);
        ServerManager iisManager = new ServerManager();
        Site site = iisManager.Sites[siteName];
		Logger.LogDebug("CreateIISWebSite(siteName={SiteName},siteAddress={SiteAddress},physicalPath={PhysicalPath}): iisManager → {IisManager}", siteName, siteAddress, physicalPath, iisManager);
        if (site != null)
        {
            iisManager.Sites.Remove(iisManager.Sites[siteName]);
            iisManager.CommitChanges();
        }

        iisManager.Sites.Add(siteName, "http", string.Concat("*:80:", siteAddress), physicalPath);
        iisManager.CommitChanges();
        iisManager.Dispose();
		Logger.LogDebug("<<< Exiting CreateIISWebSite(siteName={SiteName},siteAddress={SiteAddress},physicalPath={PhysicalPath})", siteName, siteAddress, physicalPath);
    }

    public static void RecycleApplicationPool(string appName)
    {
		Logger.LogDebug(">>> Entering RecycleApplicationPool(appName={AppName})", appName);
        ////ServerManager iisManager = new ServerManager();
        ////Site defaultSite = iisManager.Sites["Default Web Site"];
        ////Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

        ////if (app != null)
        ////{
        ////    string appPool = app.ApplicationPoolName;
        ////    ApplicationPool pool = iisManager.ApplicationPools[appPool];
        ////    pool.Recycle();
        ////}
		Logger.LogDebug("<<< Exiting RecycleApplicationPool(appName={AppName})", appName);
    }

    public static void SetApplicationPool(string appName, string appPool)
    {
		Logger.LogDebug(">>> Entering SetApplicationPool(appName={AppName},appPool={AppPool})", appName, appPool);
        ServerManager iisManager = new ServerManager();
        Site defaultSite = iisManager.Sites["Default Web Site"];
		Logger.LogDebug("SetApplicationPool(appName={AppName},appPool={AppPool}): iisManager → {IisManager}", appName, appPool, iisManager);
		Logger.LogDebug("SetApplicationPool(appName={AppName},appPool={AppPool}): defaultSite → {DefaultSite}", appName, appPool, defaultSite);
        Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

		Logger.LogDebug("SetApplicationPool(appName={AppName},appPool={AppPool}): app → {App}", appName, appPool, app);
        if (app != null)
        {
            app.ApplicationPoolName = appPool;
			Logger.LogDebug("SetApplicationPool(appName={AppName},appPool={AppPool}): app.ApplicationPoolName → {Arg2}", appName, appPool, app.ApplicationPoolName);
            iisManager.CommitChanges();
            iisManager.Dispose();
        }
		Logger.LogDebug("<<< Exiting SetApplicationPool(appName={AppName},appPool={AppPool})", appName, appPool);
    }
    public static void KillW3WP()
    {
		Logger.LogDebug(">>> Entering KillW3WP()");
        foreach (Process p in Process.GetProcessesByName("w3wp"))
        {
            p.Kill();
        }
		Logger.LogDebug("<<< Exiting KillW3WP()");
    }
}
