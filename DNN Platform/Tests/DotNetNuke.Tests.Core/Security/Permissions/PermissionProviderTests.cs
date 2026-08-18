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
    [Test]
    public void SaveFolderPermissions_DoesNotThrow()
    {
		mockEventLogger.LogDebug(">>> Entering SaveFolderPermissions_DoesNotThrow()");
        var permissions = new List<PermissionInfo>();
        var cache = new Dictionary<string, object> { { CachingProvider.GetCacheKey(DataCache.PermissionsCacheKey), permissions }, };
		mockEventLogger.LogDebug("SaveFolderPermissions_DoesNotThrow(): cache → {Cache}", cache);
        var fakeCachingProvider = new FakeCachingProvider(cache);
		mockEventLogger.LogDebug("SaveFolderPermissions_DoesNotThrow(): fakeCachingProvider → {FakeCachingProvider}", fakeCachingProvider);
		mockEventLogger.LogDebug("SaveFolderPermissions_DoesNotThrow(): permissions → {Permissions}", permissions);
        ComponentFactory.RegisterComponentInstance<CachingProvider>(fakeCachingProvider);

        ComponentFactory.RegisterComponentInstance<DataProvider>(new Mock<DataProvider>(Mock.Of<IApplicationStatusInfo>()).Object);
        using var serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));
		mockEventLogger.LogDebug("SaveFolderPermissions_DoesNotThrow(): serviceProvider → {ServiceProvider}", serviceProvider);

        var permissionProvider = new PermissionProvider();
		mockEventLogger.LogDebug("SaveFolderPermissions_DoesNotThrow(): permissionProvider → {PermissionProvider}", permissionProvider);

        IFolderInfo folder = new FolderInfo(initialiseEmptyPermissions: true);
		mockEventLogger.LogDebug("SaveFolderPermissions_DoesNotThrow(): folder → {Folder}", folder);
        Assert.DoesNotThrow(() => permissionProvider.SaveFolderPermissions(folder));
		mockEventLogger.LogDebug("<<< Exiting SaveFolderPermissions_DoesNotThrow()");
    }

    [Test]
    public void SaveFolderPermissions_WithPermissions_DoesNotThrow()
    {
		mockEventLogger.LogDebug(">>> Entering SaveFolderPermissions_WithPermissions_DoesNotThrow()");
        var readPermission = new PermissionInfo { PermissionCode = "SYSTEM_FOLDER", PermissionKey = "READ", PermissionID = 1, };
        var permissions = new List<PermissionInfo> { readPermission, };
		mockEventLogger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): readPermission → {ReadPermission}", readPermission);
        var cache = new Dictionary<string, object> { { CachingProvider.GetCacheKey(DataCache.PermissionsCacheKey), permissions }, };
		mockEventLogger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): cache → {Cache}", cache);
        var fakeCachingProvider = new FakeCachingProvider(cache);
		mockEventLogger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): fakeCachingProvider → {FakeCachingProvider}", fakeCachingProvider);
        ComponentFactory.RegisterComponentInstance<CachingProvider>(fakeCachingProvider);

        ComponentFactory.RegisterComponentInstance<DataProvider>(new Mock<DataProvider>(Mock.Of<IApplicationStatusInfo>()).Object);
        using var serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IHostSettings>()));
		mockEventLogger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): serviceProvider → {ServiceProvider}", serviceProvider);

        var permissionProvider = new PermissionProvider();
		mockEventLogger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): permissionProvider → {PermissionProvider}", permissionProvider);

        IFolderInfo folder = new FolderInfo(initialiseEmptyPermissions: true);
		mockEventLogger.LogDebug("SaveFolderPermissions_WithPermissions_DoesNotThrow(): folder → {Folder}", folder);
        folder.FolderPermissions.Add(new FolderPermissionInfo(readPermission) { AllowAccess = true, RoleID = -2, UserID = -1, FolderID = 2, });
        Assert.DoesNotThrow(() => permissionProvider.SaveFolderPermissions(folder));
		mockEventLogger.LogDebug("<<< Exiting SaveFolderPermissions_WithPermissions_DoesNotThrow()");
    }
}
