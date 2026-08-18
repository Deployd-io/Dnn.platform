//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace DotNetNuke.Tests.UI.WatiN.Utilities;

using System;
using System.Diagnostics;
using Microsoft.Web.Administration;
using Microsoft.Extensions.Logging;

public static class IISManager
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    public static void CreateIISApplication(string appName, string physicalPath)
    {
		_logger.LogDebug(">>> Entering CreateIISApplication(appName={AppName},physicalPath={PhysicalPath})", appName, physicalPath);
        ServerManager iisManager = new ServerManager();
        Site defaultSite = iisManager.Sites["Default Web Site"];
		_logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath}): defaultSite → {DefaultSite}", appName, physicalPath, defaultSite);
        Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];
		_logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath}): app → {App}", appName, physicalPath, app);
		_logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath}): iisManager → {IisManager}", appName, physicalPath, iisManager);

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
		_logger.LogDebug("<<< Exiting CreateIISApplication(appName={AppName},physicalPath={PhysicalPath})", appName, physicalPath);
    }

    public static void CreateIISApplication(string appName, string physicalPath, string appPool)
    {
		_logger.LogDebug(">>> Entering CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool})", appName, physicalPath, appPool);
        ServerManager iisManager = new ServerManager();
        Site defaultSite = iisManager.Sites["Default Web Site"];
		_logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool}): defaultSite → {DefaultSite}", appName, physicalPath, appPool, defaultSite);
        Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];
		_logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool}): app → {App}", appName, physicalPath, appPool, app);
		_logger.LogDebug("CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool}): iisManager → {IisManager}", appName, physicalPath, appPool, iisManager);

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
		_logger.LogDebug("<<< Exiting CreateIISApplication(appName={AppName},physicalPath={PhysicalPath},appPool={AppPool})", appName, physicalPath, appPool);
    }

    public static void CreateIISWebSite(string siteName, string siteAddress, string physicalPath)
    {
		_logger.LogDebug(">>> Entering CreateIISWebSite(siteName={SiteName},siteAddress={SiteAddress},physicalPath={PhysicalPath})", siteName, siteAddress, physicalPath);
        ServerManager iisManager = new ServerManager();
        Site site = iisManager.Sites[siteName];
		_logger.LogDebug("CreateIISWebSite(siteName={SiteName},siteAddress={SiteAddress},physicalPath={PhysicalPath}): iisManager → {IisManager}", siteName, siteAddress, physicalPath, iisManager);
        if (site != null)
        {
            iisManager.Sites.Remove(iisManager.Sites[siteName]);
            iisManager.CommitChanges();
        }

        iisManager.Sites.Add(siteName, "http", string.Concat("*:80:", siteAddress), physicalPath);
        iisManager.CommitChanges();
        iisManager.Dispose();
		_logger.LogDebug("<<< Exiting CreateIISWebSite(siteName={SiteName},siteAddress={SiteAddress},physicalPath={PhysicalPath})", siteName, siteAddress, physicalPath);
    }

    public static void RecycleApplicationPool(string appName)
    {
		_logger.LogDebug(">>> Entering RecycleApplicationPool(appName={AppName})", appName);
        ////ServerManager iisManager = new ServerManager();
        ////Site defaultSite = iisManager.Sites["Default Web Site"];
        ////Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];

        ////if (app != null)
        ////{
        ////    string appPool = app.ApplicationPoolName;
        ////    ApplicationPool pool = iisManager.ApplicationPools[appPool];
        ////    pool.Recycle();
        ////}
		_logger.LogDebug("<<< Exiting RecycleApplicationPool(appName={AppName})", appName);
    }

    public static void SetApplicationPool(string appName, string appPool)
    {
		_logger.LogDebug(">>> Entering SetApplicationPool(appName={AppName},appPool={AppPool})", appName, appPool);
        ServerManager iisManager = new ServerManager();
        Site defaultSite = iisManager.Sites["Default Web Site"];
		_logger.LogDebug("SetApplicationPool(appName={AppName},appPool={AppPool}): defaultSite → {DefaultSite}", appName, appPool, defaultSite);
        Microsoft.Web.Administration.Application app = defaultSite.Applications[String.Format("/{0}", appName)];
		_logger.LogDebug("SetApplicationPool(appName={AppName},appPool={AppPool}): app → {App}", appName, appPool, app);
		_logger.LogDebug("SetApplicationPool(appName={AppName},appPool={AppPool}): iisManager → {IisManager}", appName, appPool, iisManager);

        if (app != null)
        {
            app.ApplicationPoolName = appPool;
			_logger.LogDebug("SetApplicationPool(appName={AppName},appPool={AppPool}): app.ApplicationPoolName → {Arg2}", appName, appPool, app.ApplicationPoolName);
            iisManager.CommitChanges();
            iisManager.Dispose();
        }
		_logger.LogDebug("<<< Exiting SetApplicationPool(appName={AppName},appPool={AppPool})", appName, appPool);
    }
    public static void KillW3WP()
    {
		_logger.LogDebug(">>> Entering KillW3WP()");
        foreach (Process p in Process.GetProcessesByName("w3wp"))
        {
            p.Kill();
        }
		_logger.LogDebug("<<< Exiting KillW3WP()");
    }
}
