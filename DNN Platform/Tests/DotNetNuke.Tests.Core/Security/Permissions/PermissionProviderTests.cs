// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Security.Permissions;

using System.Collections.Generic;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Tests.Utilities.Fakes;

using Microsoft.Extensions.DependencyInjection;

using Moq;
using NUnit.Framework;

[TestFixture]
public class PermissionProviderTests
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    [Test]
    public void SaveFolderPermissions_DoesNotThrow()
    {
		Logger.LogDebug(">>> Entering SaveFolderPermissions_DoesNotThrow()");
        var permissions = new List<PermissionInfo>();
        var cache = new Dictionary<string, object> { { CachingProvider.GetCacheKey(DataCache.PermissionsCacheKey), permissions }, };
		Logger.LogDebug("SaveFolderPermissions_DoesNotThrow(): permissions → {Permissions}", permissions);
		Logger.LogDebug("SaveFolderPermissions_DoesNotThrow(): cache → {Cache}", cache);
        var fakeCachingProvider = new FakeCachingProvider(cache);
        ComponentFactory.RegisterComponentInstance<CachingProvider>(fakeCachingProvider);
		Logger.LogDebug("SaveFolderPermissions_DoesNotThrow(): fakeCachingProvider → {FakeCachingProvider}", fakeCachingProvider);

        ComponentFactory.RegisterComponentInstance<DataProvider>(new Mock<DataProvider>(Mock.Of<IApplicationStatusInfo>()).Object);
        using var serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));

		Logger.LogDebug("SaveFolderPermissions_DoesNotThrow(): serviceProvider → {ServiceProvider}", serviceProvider);
        var permissionProvider = new PermissionProvider();

		Logger.LogDebug("SaveFolderPermissions_DoesNotThrow(): permissionProvider → {PermissionProvider}", permissionProvider);
        IFolderInfo folder = new FolderInfo(initialiseEmptyPermissions: true);
        Assert.DoesNotThrow(() => permissionProvider.SaveFolderPermissions(folder));
		Logger.LogDebug("<<< Exiting SaveFolderPermissions_DoesNotThrow()");
		Logger.LogDebug("SaveFolderPermissions_DoesNotThrow(): folder → {Folder}", folder);
    }

    [Test]
    public void SaveFolderPermissions_WithPermissions_DoesNotThrow()
    {
		Logger.LogDebug(">>> Entering SaveFolderPermissions_WithPermissions_DoesNotThrow()");
        var readPermission = new PermissionInfo { PermissionCode = "SYSTEM_FOLDER", PermissionKey = "READ", PermissionID = 1, };
        var permissions = new List<PermissionInfo> { readPermission, };
		Logger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): readPermission → {ReadPermission}", readPermission);
        var cache = new Dictionary<string, object> { { CachingProvider.GetCacheKey(DataCache.PermissionsCacheKey), permissions }, };
        var fakeCachingProvider = new FakeCachingProvider(cache);
		Logger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): cache → {Cache}", cache);
		Logger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): fakeCachingProvider → {FakeCachingProvider}", fakeCachingProvider);
        ComponentFactory.RegisterComponentInstance<CachingProvider>(fakeCachingProvider);

        ComponentFactory.RegisterComponentInstance<DataProvider>(new Mock<DataProvider>(Mock.Of<IApplicationStatusInfo>()).Object);
        using var serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));

		Logger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): serviceProvider → {ServiceProvider}", serviceProvider);
        var permissionProvider = new PermissionProvider();

		Logger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): permissionProvider → {PermissionProvider}", permissionProvider);
        IFolderInfo folder = new FolderInfo(initialiseEmptyPermissions: true);
        folder.FolderPermissions.Add(new FolderPermissionInfo(readPermission) { AllowAccess = true, RoleID = -2, UserID = -1, FolderID = 2, });
		Logger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): folder → {Folder}", folder);
        Assert.DoesNotThrow(() => permissionProvider.SaveFolderPermissions(folder));
		Logger.LogDebug("<<< Exiting SaveFolderPermissions_WithPermissions_DoesNotThrow()");
    }
}
