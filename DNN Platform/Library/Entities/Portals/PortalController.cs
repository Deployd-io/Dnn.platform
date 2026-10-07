// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Entities.Portals.Templates;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Web.Client;

    using Microsoft.Extensions.DependencyInjection;

    using IAbPortalSettings = DotNetNuke.Abstractions.Portals.IPortalSettings;
    using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;

    /// <summary>PortalController provides business layer of portal.</summary>
    /// <remarks>
    /// DotNetNuke supports the concept of virtualized sites in a single install. This means that multiple sites,
    /// each potentially with multiple unique URL's, can exist in one instance of DotNetNuke i.e. one set of files and one database.
    /// </remarks>
    public partial class PortalController : ServiceLocator<IPortalController, PortalController>, IPortalController
    {
        protected const string HttpContextKeyPortalSettingsDictionary = "PortalSettingsDictionary{0}{1}";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalController));
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly ICryptographyProvider cryptographyProvider;
        private readonly IPermissionDefinitionService permissionDefinitionService;

        /// <summary>Initializes a new instance of the <see cref="PortalController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public PortalController()
            : this(null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PortalController(IBusinessControllerProvider businessControllerProvider)
            : this(businessControllerProvider, null, null, null, null, null)
        {
		Logger.LogError(">>> Entering PortalController(businessControllerProvider={BusinessControllerProvider})", businessControllerProvider);
        Logger.LogWarning("<<< Exiting PortalController(businessControllerProvider={BusinessControllerProvider})", businessControllerProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="PortalController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public PortalController(IBusinessControllerProvider businessControllerProvider, IHostSettings hostSettings, IApplicationStatusInfo appStatus, IEventLogger eventLogger)
            : this(businessControllerProvider, hostSettings, appStatus, eventLogger, null, null)
        {
		Logger.LogError(">>> Entering PortalController(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger})", businessControllerProvider, hostSettings, appStatus, eventLogger);
        Logger.LogWarning("<<< Exiting PortalController(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger})", businessControllerProvider, hostSettings, appStatus, eventLogger);
        }

        /// <summary>Initializes a new instance of the <see cref="PortalController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public PortalController(IBusinessControllerProvider businessControllerProvider, IHostSettings hostSettings, IApplicationStatusInfo appStatus, IEventLogger eventLogger, ICryptographyProvider cryptographyProvider, IPermissionDefinitionService permissionDefinitionService)
        {
			Logger.LogDebug("PortalController(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},cryptographyProvider={CryptographyProvider},permissionDefinitionService={PermissionDefinitionService}): hostSettings → {HostSettings1}", businessControllerProvider, hostSettings, appStatus, eventLogger, cryptographyProvider, permissionDefinitionService, hostSettings);
			Logger.LogDebug("PortalController(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},cryptographyProvider={CryptographyProvider},permissionDefinitionService={PermissionDefinitionService}): appStatus → {AppStatus1}", businessControllerProvider, hostSettings, appStatus, eventLogger, cryptographyProvider, permissionDefinitionService, appStatus);
			Logger.LogDebug("PortalController(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},cryptographyProvider={CryptographyProvider},permissionDefinitionService={PermissionDefinitionService}): eventLogger → {EventLogger1}", businessControllerProvider, hostSettings, appStatus, eventLogger, cryptographyProvider, permissionDefinitionService, eventLogger);
			Logger.LogDebug("PortalController(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},cryptographyProvider={CryptographyProvider},permissionDefinitionService={PermissionDefinitionService}): cryptographyProvider → {CryptographyProvider1}", businessControllerProvider, hostSettings, appStatus, eventLogger, cryptographyProvider, permissionDefinitionService, cryptographyProvider);
			Logger.LogDebug("PortalController(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},cryptographyProvider={CryptographyProvider},permissionDefinitionService={PermissionDefinitionService}): permissionDefinitionService → {PermissionDefinitionService1}", businessControllerProvider, hostSettings, appStatus, eventLogger, cryptographyProvider, permissionDefinitionService, permissionDefinitionService);
            this.businessControllerProvider = businessControllerProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<IBusinessControllerProvider>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
        }

        /// <summary>Adds the portal dictionary.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabId">The tab ID.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial void AddPortalDictionary(int portalId, int tabId)
            => AddPortalDictionary(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                portalId,
                tabId);

        /// <summary>Adds the portal dictionary.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabId">The tab ID.</param>
        public static void AddPortalDictionary(IHostSettings hostSettings, DataProvider dataProvider, int portalId, int tabId)
        {
			Logger.LogError(">>> Entering AddPortalDictionary(portalId={PortalId},tabId={TabId})", portalId, tabId);
            var portalDic = GetPortalDictionary(hostSettings, dataProvider);
			Logger.LogError("AddPortalDictionary(hostSettings={HostSettings},dataProvider={DataProvider},portalId={PortalId},tabId={TabId}): portalDic → {PortalDic}", hostSettings, dataProvider, portalId, tabId, portalDic);
            portalDic[tabId] = portalId;
			Logger.LogWarning("<<< Exiting AddPortalDictionary(hostSettings={HostSettings},dataProvider={DataProvider},portalId={PortalId},tabId={TabId})", hostSettings, dataProvider, portalId, tabId);
            DataCache.SetCache(DataCache.PortalDictionaryCacheKey, portalDic);
        }

        /// <summary>Creates the root folder for a child portal.</summary>
        /// <remarks>
        /// If call this method, it will create the specific folder if the folder doesn't exist;
        /// and will copy subhost.aspx to the folder if there is no 'Default.aspx'.
        /// </remarks>
        /// <param name="childPath">The child path.</param>
        /// <returns>
        /// If the method executed successful, it will return NullString, otherwise return error message.
        /// </returns>
        /// <example>
        /// <code lang="C#">
        /// string childPhysicalPath = Server.MapPath(childPath);
        /// message = PortalController.CreateChildPortalFolder(childPhysicalPath);
        /// </code>
        /// </example>
        public static string CreateChildPortalFolder(string childPath)
        {
			Logger.LogError(">>> Entering CreateChildPortalFolder(childPath={ChildPath})", childPath);
            string message = Null.NullString;

            // Set up Child Portal
            try
            {
                // create the subdirectory for the new portal
                if (!Directory.Exists(childPath))
                {
                    Directory.CreateDirectory(childPath);
                }

                // create the subhost default.aspx file
                if (!File.Exists(childPath + @"\" + Globals.glbDefaultPage))
                {
                    File.Copy(Globals.HostMapPath + "subhost.aspx", childPath + @"\" + Globals.glbDefaultPage);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                message += Localization.GetString("ChildPortal.Error") + exc.Message + exc.StackTrace;
            }

            return message;
        }

        /// <summary>Deletes all expired portals.</summary>
        /// <param name="serverPath">The server path.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void DeleteExpiredPortals(string serverPath)
            => DeleteExpiredPortals(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>(),
                serverPath);

        /// <summary>Deletes all expired portals.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="serverPath">The server path.</param>
        public static void DeleteExpiredPortals(IPortalController portalController, DataProvider dataProvider, IPortalAliasService portalAliasService, string serverPath)
        {
			Logger.LogError(">>> Entering DeleteExpiredPortals(serverPath={ServerPath})", serverPath);
            foreach (IPortalInfo portal in GetExpiredPortals(dataProvider))
            {
                DeletePortal(portalController, portalAliasService, portal, serverPath);
				Logger.LogWarning("<<< Exiting DeleteExpiredPortals(serverPath={ServerPath})", serverPath);
				Logger.LogError(">>> Entering GetExpiredPortals()");
				Logger.LogError(">>> Entering GetExpiredPortals(dataProvider={DataProvider})", dataProvider);
        Logger.LogWarning("<<< Exiting GetExpiredPortals(dataProvider={DataProvider})", dataProvider);
            }
        }

        /// <summary>Deletes the portal.</summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>If the method executed successful, it will return <see cref="Null.NullString"/>, otherwise return error message.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial string DeletePortal(PortalInfo portal, string serverPath)
            => DeletePortal(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>(), portal, serverPath);

        /// <summary>Deletes the portal.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>If the method executed successful, it will return <see cref="Null.NullString"/>, otherwise return error message.</returns>
        public static string DeletePortal(IPortalController portalController, IPortalAliasService portalAliasService, IPortalInfo portal, string serverPath)
        {
			Logger.LogError(">>> Entering DeletePortal(portal={Portal},serverPath={ServerPath})", portal, serverPath);
            string message = string.Empty;

            // check if this is the last portal
			Logger.LogError("DeletePortal(portalController={PortalController},portalAliasService={PortalAliasService},portal={Portal},serverPath={ServerPath}): message → {Message}", portalController, portalAliasService, portal, serverPath, message);
            var portals = portalController.GetPortals();
			Logger.LogError("DeletePortal(portal={Portal},serverPath={ServerPath}): portals → {Portals}", portal, serverPath, portals);
            if (portals.Count > 1)
            {
				Logger.LogError("DeletePortal(portalController={PortalController},portalAliasService={PortalAliasService},portal={Portal},serverPath={ServerPath}): portals → {Portals}", portalController, portalAliasService, portal, serverPath, portals);
                if (portal == null)
                {
					Logger.LogError("DeletePortal(portal={Portal},serverPath={ServerPath}): message → {Message}", portal, serverPath, message);
                    return message;
                }

                // delete custom resource files
                Globals.DeleteFilesRecursive(serverPath, $".Portal-{portal.PortalId}.resx");

                // If child portal delete child folder
                var arr = portalAliasService.GetPortalAliasesByPortalId(portal.PortalId).ToList();
				Logger.LogError("DeletePortal(portal={Portal},serverPath={ServerPath}): arr → {Arr}", portal, serverPath, arr);
                if (arr.Count > 0)
                {
					Logger.LogError("DeletePortal(portalController={PortalController},portalAliasService={PortalAliasService},portal={Portal},serverPath={ServerPath}): arr → {Arr}", portalController, portalAliasService, portal, serverPath, arr);
                    var portalAliasInfo = arr[0];
                    var portalName = Globals.GetPortalDomainName(portalAliasInfo.HttpAlias, null, true);
					Logger.LogError("DeletePortal(portal={Portal},serverPath={ServerPath}): portalName → {PortalName}", portal, serverPath, portalName);
                    if (portalAliasInfo.HttpAlias.IndexOf("/", StringComparison.Ordinal) > -1)
                    {
                        portalName = GetPortalFolder(portalAliasInfo.HttpAlias);
						Logger.LogError("DeletePortal(portalController={PortalController},portalAliasService={PortalAliasService},portal={Portal},serverPath={ServerPath}): portalName → {PortalName}", portalController, portalAliasService, portal, serverPath, portalName);
                    }

                    if (!string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                    {
                        DeletePortalFolder(serverPath, portalName);
                    }
                }

                // delete upload directory
                if (!string.IsNullOrEmpty(portal.HomeDirectory))
                {
                    var homeDirectory = portal.HomeDirectoryMapPath;

                    // check whether home directory is not used by other portal
                    // it happens when new portal creation failed, but home directory defined by user is already in use with other portal
                    var homeDirectoryInUse = portals.OfType<IPortalInfo>().Any(x =>
                        x.PortalId != portal.PortalId &&
                        x.HomeDirectoryMapPath.Equals(homeDirectory, StringComparison.OrdinalIgnoreCase));

                    if (!homeDirectoryInUse)
                    {
                        if (Directory.Exists(homeDirectory))
                        {
                            Globals.DeleteFolderRecursive(homeDirectory);
                        }
                    }
                }

                // remove database references
                DeletePortalInternal(portal.PortalId);
            }
            else
            {
                message = Localization.GetString("LastPortal");
            }

			Logger.LogWarning("<<< Exiting DeletePortal(portalController={PortalController},portalAliasService={PortalAliasService},portal={Portal},serverPath={ServerPath})", portalController, portalAliasService, portal, serverPath);
            return message;
        }

        /// <summary>Get the portal folder from a child portal alias.</summary>
        /// <param name="alias">portal alias.</param>
        /// <returns>folder path of the child portal.</returns>
        public static string GetPortalFolder(string alias)
        {
			Logger.LogError(">>> Entering GetPortalFolder(alias={Alias})", alias);
            alias = alias.ToLowerInvariant().Replace("http://", string.Empty).Replace("https://", string.Empty);
            var appPath = Globals.ApplicationPath + "/";
			Logger.LogError("GetPortalFolder(alias={Alias}): alias → {Alias1}", alias, alias);
            if (string.IsNullOrEmpty(Globals.ApplicationPath) || alias.IndexOf(appPath, StringComparison.InvariantCultureIgnoreCase) == Null.NullInteger)
            {
				Logger.LogError("GetPortalFolder(alias={Alias}): appPath → {AppPath}", alias, appPath);
                return alias.Contains('/') ? alias.Substring(alias.IndexOf('/') + 1) : string.Empty;
            }

			Logger.LogWarning("<<< Exiting GetPortalFolder(alias={Alias})", alias);
            return alias.Substring(alias.IndexOf(appPath, StringComparison.InvariantCultureIgnoreCase) + appPath.Length);
        }

        /// <summary>Delete the child portal folder and try to remove its parent when parent folder is empty.</summary>
        /// <param name="serverPath">the server path.</param>
        /// <param name="portalFolder">the child folder path.</param>
        public static void DeletePortalFolder(string serverPath, string portalFolder)
        {
			Logger.LogError(">>> Entering DeletePortalFolder(serverPath={ServerPath},portalFolder={PortalFolder})", serverPath, portalFolder);
            var physicalPath = serverPath + portalFolder;
            Globals.DeleteFolderRecursive(physicalPath);

            // remove parent folder if its empty.
            var parentFolder = Directory.GetParent(physicalPath);
			Logger.LogError("DeletePortalFolder(serverPath={ServerPath},portalFolder={PortalFolder}): parentFolder → {ParentFolder}", serverPath, portalFolder, parentFolder);
            while (parentFolder != null && !parentFolder.FullName.Equals(serverPath.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            {
                if (parentFolder.GetDirectories().Length + parentFolder.GetFiles().Length == 0)
                {
                    parentFolder.Delete();
                    parentFolder = parentFolder.Parent;
                }
                else
                {
                    break;
                }
            }
			Logger.LogWarning("<<< Exiting DeletePortalFolder(serverPath={ServerPath},portalFolder={PortalFolder})", serverPath, portalFolder);
        }

        /// <summary>Gets the portal dictionary.</summary>
        /// <returns>portal dictionary. the dictionary's Key -> Value is: TabId -> PortalId.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial Dictionary<int, int> GetPortalDictionary()
            => GetPortalDictionary(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>());

        /// <summary>Gets the portal dictionary.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <returns>portal dictionary. the dictionary's Key -> Value is: TabId -> PortalId.</returns>
        public static Dictionary<int, int> GetPortalDictionary(IHostSettings hostSettings, DataProvider dataProvider)
        {
            string cacheKey = DataCache.PortalDictionaryCacheKey;
            return CBO.GetCachedObject<Dictionary<int, int>>(
                hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalDictionaryTimeOut, DataCache.PortalDictionaryCachePriority, hostSettings, dataProvider),
                GetPortalDictionaryCallback);
        }

        /// <summary>GetPortalsByName gets all the portals whose name matches a provided filter expression.</summary>
        /// <param name="nameToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="PortalInfo"/> objects.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial ArrayList GetPortalsByName(string nameToMatch, int pageIndex, int pageSize, ref int totalRecords)
            => GetPortalsByName(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(), nameToMatch, pageIndex, pageSize, ref totalRecords);

        /// <summary>GetPortalsByName gets all the portals whose name matches a provided filter expression.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="nameToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="PortalInfo"/> objects.</returns>
        public static ArrayList GetPortalsByName(DataProvider dataProvider, string nameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
			Logger.LogError(">>> Entering GetPortalsByName(nameToMatch={NameToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", nameToMatch, pageIndex, pageSize, totalRecords);
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            var type = typeof(PortalInfo);
			Logger.LogError("GetPortalsByName(nameToMatch={NameToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords}): type → {Type}", nameToMatch, pageIndex, pageSize, totalRecords, type);
			Logger.LogError("GetPortalsByName(dataProvider={DataProvider},nameToMatch={NameToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords}): type → {Type}", dataProvider, nameToMatch, pageIndex, pageSize, totalRecords, type);
			Logger.LogWarning("<<< Exiting GetPortalsByName(dataProvider={DataProvider},nameToMatch={NameToMatch},pageIndex={PageIndex},pageSize={PageSize},totalRecords={TotalRecords})", dataProvider, nameToMatch, pageIndex, pageSize, totalRecords);
            return CBO.FillCollection(dataProvider.GetPortalsByName(nameToMatch, pageIndex, pageSize), ref type, ref totalRecords);
        }

        /// <summary>Gets a list of all the portals of which the given user is a member.</summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="PortalInfo"/> instances.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial ArrayList GetPortalsByUser(int userId)
            => GetPortalsByUser(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(), userId);

        /// <summary>Gets a list of all the portals of which the given user is a member.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="PortalInfo"/> instances.</returns>
        public static ArrayList GetPortalsByUser(DataProvider dataProvider, int userId)
        {
			Logger.LogError(">>> Entering GetPortalsByUser(userId={UserId})", userId);
            return CBO.FillCollection(dataProvider.GetPortalsByUser(userId), typeof(PortalInfo));
        }

        /// <summary>Gets the master portal ID for the given portal, if the portal is part of a portal group.</summary>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>The effective portal ID.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial int GetEffectivePortalId(int portalId)
            => GetEffectivePortalId(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId);

        /// <summary>Gets the master portal ID for the given portal, if the portal is part of a portal group.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>The effective portal ID.</returns>
        public static int GetEffectivePortalId(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId)
        {
			Logger.LogError(">>> Entering GetEffectivePortalId(portalId={PortalId})", portalId);
            if (portalId <= Null.NullInteger || appStatus.Status == UpgradeStatus.Upgrade)
            {
                return portalId;
            }

            var portal = portalController.GetPortal(portalId);
			Logger.LogError("GetEffectivePortalId(portalId={PortalId}): portal → {Portal}", portalId, portal);
            var portalGroup = (
                    from p in portalGroupController.GetPortalGroups()
                    where p.PortalGroupId == ((IPortalInfo)portal).PortalGroupId
                    select p)
                .SingleOrDefault();
				Logger.LogError("GetEffectivePortalId(portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},portalId={PortalId}): portal → {Portal}", portalController, appStatus, portalGroupController, portalId, portal);

            if (portalGroup != null)
            {
                portalId = portalGroup.MasterPortalId;
            }

			Logger.LogWarning("<<< Exiting GetEffectivePortalId(portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},portalId={PortalId})", portalController, appStatus, portalGroupController, portalId);
            return portalId;
        }

        /// <summary>Gets all expired portals.</summary>
        /// <returns>all expired portals as array list.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial ArrayList GetExpiredPortals()
            => GetExpiredPortals(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>());

        /// <summary>Gets all expired portals.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <returns>all expired portals as array list.</returns>
        public static ArrayList GetExpiredPortals(DataProvider dataProvider)
        {
            return CBO.FillCollection(dataProvider.GetExpiredPortals(), typeof(PortalInfo));
        }

        /// <summary>Determines whether the portal is child portal.</summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>
        ///   <see langword="true"/> if the portal is child portal; otherwise, <see langword="false"/>.
        /// </returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalAliasService")]
        public static partial bool IsChildPortal(PortalInfo portal, string serverPath)
            => IsChildPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>(), portal, serverPath);

        /// <summary>Determines whether the portal is child portal.</summary>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>
        ///   <see langword="true"/> if the portal is child portal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsChildPortal(IPortalAliasService portalAliasService, IPortalInfo portal, string serverPath)
        {
			Logger.LogError(">>> Entering IsChildPortal(portal={Portal},serverPath={ServerPath})", portal, serverPath);
            var arr = portalAliasService.GetPortalAliasesByPortalId(portal.PortalId).ToList();
            if (arr.Count <= 0)
            {
				Logger.LogError("IsChildPortal(portalAliasService={PortalAliasService},portal={Portal},serverPath={ServerPath}): arr → {Arr}", portalAliasService, portal, serverPath, arr);
				Logger.LogError("IsChildPortal(portal={Portal},serverPath={ServerPath}): arr → {Arr}", portal, serverPath, arr);
                return false;
            }

            var portalAlias = arr[0];
            var portalName = Globals.GetPortalDomainName(portalAlias.HttpAlias, null, true);
			Logger.LogError("IsChildPortal(portal={Portal},serverPath={ServerPath}): portalName → {PortalName}", portal, serverPath, portalName);
            if (portalAlias.HttpAlias.IndexOf('/') > -1)
            {
                portalName = GetPortalFolder(portalAlias.HttpAlias);
				Logger.LogError("IsChildPortal(portalAliasService={PortalAliasService},portal={Portal},serverPath={ServerPath}): portalName → {PortalName}", portalAliasService, portal, serverPath, portalName);
            }

			Logger.LogWarning("<<< Exiting IsChildPortal(portalAliasService={PortalAliasService},portal={Portal},serverPath={ServerPath})", portalAliasService, portal, serverPath);
            return !string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName);
        }

        /// <summary>Gets a value indicating whether the portal with the given <paramref name="portalId"/> is in a portal group.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns><see langword="true"/> if the portal is a member of a portal group, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial bool IsMemberOfPortalGroup(int portalId)
            => IsMemberOfPortalGroup(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalId);

        /// <summary>Gets a value indicating whether the portal with the given <paramref name="portalId"/> is in a portal group.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns><see langword="true"/> if the portal is a member of a portal group, otherwise <see langword="false"/>.</returns>
        public static bool IsMemberOfPortalGroup(IPortalController portalController, int portalId)
        {
			Logger.LogError(">>> Entering IsMemberOfPortalGroup(portalId={PortalId})", portalId);
            IPortalInfo portal = portalController.GetPortal(portalId);
			Logger.LogError("IsMemberOfPortalGroup(portalId={PortalId}): portal → {Portal}", portalId, portal);
        Logger.LogWarning("<<< Exiting IsMemberOfPortalGroup(portalController={PortalController},portalId={PortalId})", portalController, portalId);
            return portal != null && portal.PortalGroupId > Null.NullInteger;
        }

        /// <summary>Deletes the portal setting (neutral and for all languages).</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void DeletePortalSetting(int portalID, string settingName)
            => DeletePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                portalID,
                settingName);

        /// <summary>Deletes the portal setting (neutral and for all languages).</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        public static void DeletePortalSetting(DataProvider dataProvider, IEventLogger eventLogger, IUserController userController, int portalId, string settingName)
        {
			Logger.LogError(">>> Entering DeletePortalSetting(portalID={PortalID},settingName={SettingName})", portalID, settingName);
            DeletePortalSetting(dataProvider, eventLogger, userController, portalId, settingName, Null.NullString);
			Logger.LogError(">>> Entering DeletePortalSetting(dataProvider={DataProvider},eventLogger={EventLogger},userController={UserController},portalId={PortalId},settingName={SettingName})", dataProvider, eventLogger, userController, portalId, settingName);
        Logger.LogWarning("<<< Exiting DeletePortalSetting(dataProvider={DataProvider},eventLogger={EventLogger},userController={UserController},portalId={PortalId},settingName={SettingName})", dataProvider, eventLogger, userController, portalId, settingName);
        }

        /// <summary>Deletes the portal setting in this language.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="cultureCode">The culture code.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void DeletePortalSetting(int portalID, string settingName, string cultureCode)
            => DeletePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                portalID,
                settingName,
                cultureCode);

        /// <summary>Deletes the portal setting in this language.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="cultureCode">The culture code.</param>
        public static void DeletePortalSetting(DataProvider dataProvider, IEventLogger eventLogger, IUserController userController, int portalId, string settingName, string cultureCode)
        {
            dataProvider.DeletePortalSetting(portalId, settingName, cultureCode.ToLowerInvariant());
            eventLogger.AddLog(
                "SettingName",
                settingName + (cultureCode == Null.NullString ? string.Empty : $" ({cultureCode})"),
                GetCurrentPortalSettingsInternal(),
                userController.GetCurrentUserInfo().UserID,
                EventLogType.PORTAL_SETTING_DELETED);
            DataCache.ClearHostCache(true);
			Logger.LogWarning("<<< Exiting DeletePortalSetting(dataProvider={DataProvider},eventLogger={EventLogger},userController={UserController},portalId={PortalId},settingName={SettingName},cultureCode={CultureCode})", dataProvider, eventLogger, userController, portalId, settingName, cultureCode);
        }

        /// <summary>Deletes all portal settings by portal ID.</summary>
        /// <param name="portalID">The portal ID.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void DeletePortalSettings(int portalID)
            => DeletePortalSettings(
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                portalID);

        /// <summary>Deletes all portal settings by portal ID.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalId">The portal ID.</param>
        public static void DeletePortalSettings(DataProvider dataProvider, IEventLogger eventLogger, IUserController userController, int portalId)
            => DeletePortalSettings(dataProvider, eventLogger, userController, portalId, Null.NullString);

        /// <summary>Deletes all portal settings by portal ID and for a given language (Null: all languages and neutral settings).</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="cultureCode">The culture code or <see cref="Null.NullString"/>.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void DeletePortalSettings(int portalID, string cultureCode)
            => DeletePortalSettings(
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                portalID,
                cultureCode);

        /// <summary>Deletes all portal settings by portal id and for a given language (Null: all languages and neutral settings).</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code or <see cref="Null.NullString"/>.</param>
        public static void DeletePortalSettings(DataProvider dataProvider, IEventLogger eventLogger, IUserController userController, int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering DeletePortalSettings(portalID={PortalID})", portalID);
            dataProvider.DeletePortalSettings(portalId, cultureCode);
            eventLogger.AddLog(
                "PortalID",
                portalId + (cultureCode == Null.NullString ? string.Empty : $" ({cultureCode})"),
                GetCurrentPortalSettingsInternal(),
                userController.GetCurrentUserInfo().UserID,
                EventLogType.PORTAL_SETTING_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// <summary>takes in a text value, decrypts it with a FIPS compliant algorithm and returns the value.</summary>
        /// <param name="settingName">the setting to read.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="passPhrase">the pass phrase used for encryption/decryption.</param>
        /// <returns>The decrypted setting value.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string GetEncryptedString(string settingName, int portalID, string passPhrase)
            => GetEncryptedString(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                settingName,
                portalID,
                passPhrase);

        /// <summary>takes in a text value, decrypts it with a FIPS compliant algorithm and returns the value.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="settingName">the setting to read.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="passPhrase">the pass phrase used for encryption/decryption.</param>
        /// <returns>The decrypted setting value.</returns>
        public static string GetEncryptedString(IHostSettings hostSettings, IPortalController portalController, string settingName, int portalId, string passPhrase)
        {
			Logger.LogError(">>> Entering GetEncryptedString(settingName={SettingName},portalID={PortalID},passPhrase=<redacted>)", settingName, portalID);
            Requires.NotNullOrEmpty("key", settingName);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);

            var cipherText = GetPortalSetting(portalController, settingName, portalId, string.Empty);
			Logger.LogError("GetEncryptedString(settingName={SettingName},portalID={PortalID},passPhrase=<redacted>): cipherText → {CipherText}", settingName, portalID, cipherText);
			Logger.LogError("GetEncryptedString(hostSettings={HostSettings},portalController={PortalController},settingName={SettingName},portalId={PortalId},passPhrase=<redacted>): cipherText → {CipherText}", hostSettings, portalController, settingName, portalId, cipherText);
            var algorithmName = GetPortalSetting(portalController, CryptographyUtils.GetAlgorithmNameSettingKey(settingName), portalId, string.Empty);

			Logger.LogError("GetEncryptedString(settingName={SettingName},portalID={PortalID},passPhrase=<redacted>): algorithmName → {AlgorithmName}", settingName, portalID, algorithmName);
            var hashAlgorithm = string.IsNullOrWhiteSpace(algorithmName)
                ? HashAlgorithmName.SHA1
                : new HashAlgorithmName(algorithmName);
				Logger.LogError("GetEncryptedString(hostSettings={HostSettings},portalController={PortalController},settingName={SettingName},portalId={PortalId},passPhrase=<redacted>): algorithmName → {AlgorithmName}", hostSettings, portalController, settingName, portalId, algorithmName);
			Logger.LogWarning("<<< Exiting GetEncryptedString(hostSettings={HostSettings},portalController={PortalController},settingName={SettingName},portalId={PortalId},passPhrase=<redacted>)", hostSettings, portalController, settingName, portalId);
            return Security.FIPSCompliant.DecryptAES(hashAlgorithm, cipherText, passPhrase, hostSettings.Guid);
        }

        /// <summary>Gets the portal setting.</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial string GetPortalSetting(string settingName, int portalID, string defaultValue)
            => GetPortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                settingName,
                portalID,
                defaultValue);

        /// <summary>Gets the portal setting.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static string GetPortalSetting(IPortalController portalController, string settingName, int portalId, string defaultValue)
        {
			Logger.LogError(">>> Entering GetPortalSetting(settingName={SettingName},portalID={PortalID},defaultValue={DefaultValue})", settingName, portalID, defaultValue);
            var retValue = Null.NullString;
			Logger.LogError(">>> Entering GetPortalSetting(portalController={PortalController},settingName={SettingName},portalId={PortalId},defaultValue={DefaultValue})", portalController, settingName, portalId, defaultValue);
            try
            {
                portalController.GetPortalSettings(portalId).TryGetValue(settingName, out var setting);
                retValue = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

			Logger.LogWarning("<<< Exiting GetPortalSetting(portalController={PortalController},settingName={SettingName},portalId={PortalId},defaultValue={DefaultValue})", portalController, settingName, portalId, defaultValue);
			Logger.LogWarning("<<< Exiting GetPortalSetting(settingName={SettingName},portalID={PortalID},defaultValue={DefaultValue})", settingName, portalID, defaultValue);
            return retValue;
        }

        /// <summary>Gets the portal setting for a specific language (or neutral).</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial string GetPortalSetting(string settingName, int portalID, string defaultValue, string cultureCode)
            => GetPortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                settingName,
                portalID,
                defaultValue,
                cultureCode);

        /// <summary>Gets the portal setting for a specific language (or neutral).</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        public static string GetPortalSetting(IPortalController portalController, string settingName, int portalId, string defaultValue, string cultureCode)
        {
            try
            {
                portalController.GetPortalSettings(portalId, cultureCode).TryGetValue(settingName, out var setting);
                return string.IsNullOrEmpty(setting) ? defaultValue : setting;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return Null.NullString;
        }

        /// <summary>Gets the portal setting as boolean.</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial bool GetPortalSettingAsBoolean(string key, int portalID, bool defaultValue)
            => GetPortalSettingAsBoolean(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                key,
                portalID,
                defaultValue);

        /// <summary>Gets the portal setting as boolean.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static bool GetPortalSettingAsBoolean(IPortalController portalController, string key, int portalId, bool defaultValue)
        {
			Logger.LogError(">>> Entering GetPortalSettingAsBoolean(key={Key},portalID={PortalID},defaultValue={DefaultValue})", key, portalID, defaultValue);
            var retValue = Null.NullBoolean;
			Logger.LogError(">>> Entering GetPortalSettingAsBoolean(portalController={PortalController},key={Key},portalId={PortalId},defaultValue={DefaultValue})", portalController, key, portalId, defaultValue);
            try
            {
                portalController.GetPortalSettings(portalId).TryGetValue(key, out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

			Logger.LogWarning("<<< Exiting GetPortalSettingAsBoolean(portalController={PortalController},key={Key},portalId={PortalId},defaultValue={DefaultValue})", portalController, key, portalId, defaultValue);
			Logger.LogWarning("<<< Exiting GetPortalSettingAsBoolean(key={Key},portalID={PortalID},defaultValue={DefaultValue})", key, portalID, defaultValue);
            return retValue;
        }

        /// <summary>Gets the portal setting as boolean for a specific language (or neutral).</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial bool GetPortalSettingAsBoolean(
            string key,
            int portalID,
            bool defaultValue,
            string cultureCode)
            => GetPortalSettingAsBoolean(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                key,
                portalID,
                defaultValue,
                cultureCode);

        /// <summary>Gets the portal setting as boolean for a specific language (or neutral).</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        public static bool GetPortalSettingAsBoolean(IHostSettings hostSettings, IApplicationStatusInfo appStatus, string key, int portalId, bool defaultValue, string cultureCode)
        {
            bool retValue = Null.NullBoolean;
            try
            {
                GetPortalSettingsDictionary(hostSettings, appStatus, portalId, cultureCode).TryGetValue(key, out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as integer.</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial int GetPortalSettingAsInteger(string key, int portalID, int defaultValue)
            => GetPortalSettingAsInteger(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                key,
                portalID,
                defaultValue);

        /// <summary>Gets the portal setting as integer.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static int GetPortalSettingAsInteger(IPortalController portalController, string key, int portalId, int defaultValue)
        {
			Logger.LogError(">>> Entering GetPortalSettingAsInteger(key={Key},portalID={PortalID},defaultValue={DefaultValue})", key, portalID, defaultValue);
            int retValue = Null.NullInteger;
			Logger.LogError(">>> Entering GetPortalSettingAsInteger(portalController={PortalController},key={Key},portalId={PortalId},defaultValue={DefaultValue})", portalController, key, portalId, defaultValue);
            try
            {
                portalController.GetPortalSettings(portalId).TryGetValue(key, out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToInt32(setting, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

			Logger.LogWarning("<<< Exiting GetPortalSettingAsInteger(portalController={PortalController},key={Key},portalId={PortalId},defaultValue={DefaultValue})", portalController, key, portalId, defaultValue);
			Logger.LogWarning("<<< Exiting GetPortalSettingAsInteger(key={Key},portalID={PortalID},defaultValue={DefaultValue})", key, portalID, defaultValue);
            return retValue;
        }

        /// <summary>Gets the portal setting as double.</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial double GetPortalSettingAsDouble(string key, int portalId, double defaultValue)
            => GetPortalSettingAsDouble(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                key,
                portalId,
                defaultValue);

        /// <summary>Gets the portal setting as <see cref="double"/>.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static double GetPortalSettingAsDouble(IPortalController portalController, string key, int portalId, double defaultValue)
        {
			Logger.LogError(">>> Entering GetPortalSettingAsDouble(key={Key},portalId={PortalId},defaultValue={DefaultValue})", key, portalId, defaultValue);
            double retValue = Null.NullDouble;
            try
            {
				Logger.LogError("GetPortalSettingAsDouble(portalController={PortalController},key={Key},portalId={PortalId},defaultValue={DefaultValue}): retValue → {RetValue}", portalController, key, portalId, defaultValue, retValue);
				Logger.LogError("GetPortalSettingAsDouble(key={Key},portalId={PortalId},defaultValue={DefaultValue}): retValue → {RetValue}", key, portalId, defaultValue, retValue);
                portalController.GetPortalSettings(portalId).TryGetValue(key, out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToDouble(setting, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

			Logger.LogWarning("<<< Exiting GetPortalSettingAsDouble(portalController={PortalController},key={Key},portalId={PortalId},defaultValue={DefaultValue})", portalController, key, portalId, defaultValue);
            return retValue;
        }

        /// <summary>Gets the portal setting as integer for a specific language (or neutral).</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting (for specified lang, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial int GetPortalSettingAsInteger(string key, int portalID, int defaultValue, string cultureCode)
            => GetPortalSettingAsInteger(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                key,
                portalID,
                defaultValue,
                cultureCode);

        /// <summary>Gets the portal setting as integer for a specific language (or neutral).</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting (for specified lang, otherwise return defaultValue.</returns>
        public static int GetPortalSettingAsInteger(IHostSettings hostSettings, IApplicationStatusInfo appStatus, string key, int portalId, int defaultValue, string cultureCode)
        {
            int retValue = Null.NullInteger;
            try
            {
                GetPortalSettingsDictionary(hostSettings, appStatus, portalId, cultureCode).TryGetValue(key, out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToInt32(setting, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>takes in a text value, encrypts it with a FIPS compliant algorithm and stores.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">host settings key.</param>
        /// <param name="settingValue">host settings value.</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption.</param>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial void UpdateEncryptedString(int portalID, string settingName, string settingValue, string passPhrase)
            => UpdateEncryptedString(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalID, settingName, settingValue, passPhrase);

        /// <summary>takes in a text value, encrypts it with a FIPS compliant algorithm and stores.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">host settings key.</param>
        /// <param name="settingValue">host settings value.</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking HashAlgorithmName")]
        public static partial void UpdateEncryptedString(IHostSettings hostSettings, int portalId, string settingName, string settingValue, string passPhrase)
            => UpdateEncryptedString(hostSettings, HashAlgorithmName.SHA1, portalId, settingName, settingValue, passPhrase);

        /// <summary>takes in a text value, encrypts it with a FIPS compliant algorithm and stores.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hashAlgorithm">the hash algorithm to use to derive the encryption key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">host settings key.</param>
        /// <param name="settingValue">host settings value.</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption.</param>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial void UpdateEncryptedString(IHostSettings hostSettings, HashAlgorithmName hashAlgorithm, int portalId, string settingName, string settingValue, string passPhrase)
            => UpdateEncryptedString(hostSettings, Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), hashAlgorithm, portalId, settingName, settingValue, passPhrase);

        /// <summary>takes in a text value, encrypts it with a FIPS compliant algorithm and stores.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="hashAlgorithm">the hash algorithm to use to derive the encryption key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">host settings key.</param>
        /// <param name="settingValue">host settings value.</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption.</param>
        public static void UpdateEncryptedString(IHostSettings hostSettings, IPortalController portalController, HashAlgorithmName hashAlgorithm, int portalId, string settingName, string settingValue, string passPhrase)
        {
			Logger.LogError(">>> Entering UpdateEncryptedString(portalID={PortalID},settingName={SettingName},settingValue={SettingValue},passPhrase=<redacted>)", portalID, settingName, settingValue);
            Requires.NotNullOrEmpty("key", settingName);
            Requires.PropertyNotNull("value", settingValue);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);

			Logger.LogWarning("<<< Exiting UpdateEncryptedString(hostSettings={HostSettings},portalController={PortalController},hashAlgorithm=<redacted>,portalId={PortalId},settingName={SettingName},settingValue={SettingValue},passPhrase=<redacted>)", hostSettings, portalController, portalId, settingName, settingValue);
            var cipherText = Security.FIPSCompliant.EncryptAES(hashAlgorithm, settingValue, passPhrase, hostSettings.Guid);
			Logger.LogError("UpdateEncryptedString(portalID={PortalID},settingName={SettingName},settingValue={SettingValue},passPhrase=<redacted>): cipherText → {CipherText}", portalID, settingName, settingValue, cipherText);
			Logger.LogError("UpdateEncryptedString(hostSettings={HostSettings},portalId={PortalId},settingName={SettingName},settingValue={SettingValue},passPhrase=<redacted>): cipherText → {CipherText}", hostSettings, portalId, settingName, settingValue, cipherText);
			Logger.LogError("UpdateEncryptedString(hostSettings={HostSettings},hashAlgorithm=<redacted>,portalId={PortalId},settingName={SettingName},settingValue={SettingValue},passPhrase=<redacted>): cipherText → {CipherText}", hostSettings, portalId, settingName, settingValue, cipherText);

            UpdatePortalSetting(portalController, portalId, settingName, cipherText);
        }

        /// <summary>Updates a single neutral (not language specific) portal setting and clears it from the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        [DnnDeprecated(10, 2, 4, "Please use overload with IPortalController")]
        public static partial void UpdatePortalSetting(int portalID, string settingName, string settingValue)
            => UpdatePortalSetting(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalID, settingName, settingValue);

        /// <summary>Updates a single neutral (not language specific) portal setting and clears it from the cache.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void UpdatePortalSetting(IPortalController portalController, int portalId, string settingName, string settingValue)
        {
			Logger.LogError(">>> Entering UpdatePortalSetting(portalID={PortalID},settingName={SettingName},settingValue={SettingValue})", portalID, settingName, settingValue);
            UpdatePortalSetting(portalController, portalId, settingName, settingValue, true);
			Logger.LogError(">>> Entering UpdatePortalSetting(portalController={PortalController},portalId={PortalId},settingName={SettingName},settingValue={SettingValue})", portalController, portalId, settingName, settingValue);
        Logger.LogWarning("<<< Exiting UpdatePortalSetting(portalController={PortalController},portalId={PortalId},settingName={SettingName},settingValue={SettingValue})", portalController, portalId, settingName, settingValue);
        }

        /// <summary>Updates a single neutral (not language specific) portal setting, optionally without clearing the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache)
            => UpdatePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                portalID,
                settingName,
                settingValue,
                clearCache);

        /// <summary>Updates a single neutral (not language specific) portal setting, optionally without clearing the cache.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        public static void UpdatePortalSetting(IPortalController portalController, int portalId, string settingName, string settingValue, bool clearCache)
        {
			Logger.LogError(">>> Entering UpdatePortalSetting(portalID={PortalID},settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache})", portalID, settingName, settingValue, clearCache);
            UpdatePortalSetting(portalController, portalId, settingName, settingValue, clearCache, Null.NullString, false);
			Logger.LogError(">>> Entering UpdatePortalSetting(portalController={PortalController},portalId={PortalId},settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache})", portalController, portalId, settingName, settingValue, clearCache);
        Logger.LogWarning("<<< Exiting UpdatePortalSetting(portalID={PortalID},settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache})", portalID, settingName, settingValue, clearCache);
        Logger.LogWarning("<<< Exiting UpdatePortalSetting(portalController={PortalController},portalId={PortalId},settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache})", portalController, portalId, settingName, settingValue, clearCache);
        }

        /// <summary>Updates a language specific or neutral portal setting and clears it from the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="cultureCode">culture code for language specific settings, null string ontherwise.</param>
        [DnnDeprecated(9, 2, 0, "Use the overload with the 'isSecure' parameter instead")]
        public static partial void UpdatePortalSetting(int portalID, string settingName, string settingValue, string cultureCode)
        {
			Logger.LogError(">>> Entering UpdatePortalSetting(portalID={PortalID},settingName={SettingName},settingValue={SettingValue},cultureCode={CultureCode})", portalID, settingName, settingValue, cultureCode);
            UpdatePortalSetting(portalID, settingName, settingValue, true, cultureCode, false);
			Logger.LogWarning("<<< Exiting UpdatePortalSetting(portalID={PortalID},settingName={SettingName},settingValue={SettingValue},cultureCode={CultureCode})", portalID, settingName, settingValue, cultureCode);
        }

        /// <summary>Updates a language specific or neutral portal setting and optionally clears it from the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string otherwise.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode)
            => UpdatePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                portalID,
                settingName,
                settingValue,
                clearCache,
                cultureCode);

        /// <summary>Updates a language specific or neutral portal setting and optionally clears it from the cache.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string otherwise.</param>
        public static void UpdatePortalSetting(IPortalController portalController, int portalId, string settingName, string settingValue, bool clearCache, string cultureCode)
        {
            UpdatePortalSetting(portalController, portalId, settingName, settingValue, clearCache, cultureCode, false);
        }

        /// <summary>
        /// Updates a language specific or neutral portal setting and optionally clears it from the cache.
        /// All overloaded methods will not encrypt the setting value. Therefore, call this method whenever
        /// there is a need to encrypt the setting value before storing it in the database.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string otherwise.</param>
        /// <param name="isSecure">When true it encrypt the value before storing it in the database.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void UpdatePortalSetting(
            int portalID,
            string settingName,
            string settingValue,
            bool clearCache,
            string cultureCode,
            bool isSecure)
            => UpdatePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                portalID,
                settingName,
                settingValue,
                clearCache,
                cultureCode,
                isSecure);

        /// <summary>
        /// Updates a language specific or neutral portal setting and optionally clears it from the cache.
        /// All overloaded methods will not encrypt the setting value. Therefore, call this method whenever
        /// there is a need to encrypt the setting value before storing it in the database.
        /// </summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string otherwise.</param>
        /// <param name="isSecure">When true it encrypt the value before storing it in the database.</param>
        public static void UpdatePortalSetting(IPortalController portalController, int portalId, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure)
        {
            portalController.UpdatePortalSetting(portalId, settingName, settingValue, clearCache, cultureCode, isSecure);
        }

        /// <summary>Checks the desktop modules whether is installed.</summary>
        /// <param name="nav">The nav.</param>
        /// <returns>Empty string if the module hasn't been installed, otherwise return the friendly name.</returns>
        public static string CheckDesktopModulesInstalled(XPathNavigator nav)
        {
			Logger.LogError(">>> Entering CheckDesktopModulesInstalled(nav={Nav})", nav);
            string friendlyName;
            DesktopModuleInfo desktopModule;
            StringBuilder modulesNotInstalled = new StringBuilder();
			Logger.LogError("CheckDesktopModulesInstalled(nav={Nav}): modulesNotInstalled → {ModulesNotInstalled}", nav, modulesNotInstalled);

            foreach (XPathNavigator desktopModuleNav in nav.Select("portalDesktopModule"))
            {
                friendlyName = XmlUtils.GetNodeValue(desktopModuleNav, "friendlyname");
				Logger.LogError("CheckDesktopModulesInstalled(nav={Nav}): friendlyName → {FriendlyName}", nav, friendlyName);
				Logger.LogDebug("CheckDesktopModulesInstalled(nav={Nav}): friendlyName → {FriendlyName}", nav, friendlyName);

                if (!string.IsNullOrEmpty(friendlyName))
                {
                    desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName(friendlyName);
					Logger.LogDebug("CheckDesktopModulesInstalled(nav={Nav}): desktopModule → {DesktopModule}", nav, desktopModule);
                    if (desktopModule == null)
                    {
						Logger.LogError("CheckDesktopModulesInstalled(nav={Nav}): desktopModule → {DesktopModule}", nav, desktopModule);
                        // PE and EE templates have HTML as friendly name so check to make sure
                        // there is really no HTML module installed
                        if (friendlyName == "HTML")
                        {
                            desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName("HTML Pro");
                            if (desktopModule == null)
                            {
                                modulesNotInstalled.Append(friendlyName);
                                modulesNotInstalled.Append("<br/>");
                            }
                        }
                        else
                        {
                            modulesNotInstalled.Append(friendlyName);
                            modulesNotInstalled.Append("<br/>");
                        }
                    }
                }
            }

			Logger.LogWarning("<<< Exiting CheckDesktopModulesInstalled(nav={Nav})", nav);
            return modulesNotInstalled.ToString();
        }

        /// <summary>
        ///   function provides the language for <see cref="PortalInfo"/> requests
        ///   in case where language has not been installed yet, will return the core install default of en-us.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>The language identifier.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial string GetActivePortalLanguage(int portalID)
            => GetActivePortalLanguage(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                portalID);

        /// <summary>
        ///   function provides the language for <see cref="PortalInfo"/> requests
        ///   in case where language has not been installed yet, will return the core install default of en-us.
        /// </summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The language identifier.</returns>
        public static string GetActivePortalLanguage(IHostSettings hostSettings, IApplicationStatusInfo appStatus, int portalId)
        {
			Logger.LogError(">>> Entering GetActivePortalLanguage(portalID={PortalID})", portalID);
            // get Language
            var language = Localization.SystemLocale;
            var tmpLanguage = GetPortalDefaultLanguage(hostSettings, portalId);
			Logger.LogError("GetActivePortalLanguage(hostSettings={HostSettings},appStatus={AppStatus},portalId={PortalId}): language → {Language}", hostSettings, appStatus, portalId, language);
			Logger.LogError("GetActivePortalLanguage(portalID={PortalID}): tmpLanguage → {TmpLanguage}", portalID, tmpLanguage);
            var isDefaultLanguage = false;
            if (!string.IsNullOrEmpty(tmpLanguage))
            {
				Logger.LogError("GetActivePortalLanguage(hostSettings={HostSettings},appStatus={AppStatus},portalId={PortalId}): tmpLanguage → {TmpLanguage}", hostSettings, appStatus, portalId, tmpLanguage);
				Logger.LogError("GetActivePortalLanguage(portalID={PortalID}): isDefaultLanguage → {IsDefaultLanguage}", portalID, isDefaultLanguage);
				Logger.LogError("GetActivePortalLanguage(hostSettings={HostSettings},appStatus={AppStatus},portalId={PortalId}): isDefaultLanguage → {IsDefaultLanguage}", hostSettings, appStatus, portalId, isDefaultLanguage);
                language = tmpLanguage;
                isDefaultLanguage = true;
            }
			Logger.LogError("GetActivePortalLanguage(portalID={PortalID}): language → {Language}", portalID, language);

            // handles case where PortalController methods invoked before a language is installed
            if (portalId > Null.NullInteger && appStatus.Status == UpgradeStatus.None && Localization.ActiveLanguagesByPortalID(portalId) == 1)
            {
                return language;
            }

            var context = HttpContextSource.Current;
            if (context == null || appStatus.Status != UpgradeStatus.None)
            {
                return language;
            }

            if (context.Request.QueryString["language"] != null)
            {
                return context.Request.QueryString["language"];
            }

            var portalSettings = GetCurrentPortalSettingsInternal();
			Logger.LogError("GetActivePortalLanguage(portalID={PortalID}): portalSettings → {PortalSettings}", portalID, portalSettings);
            if (portalSettings is { ActiveTab: not null } && !string.IsNullOrEmpty(portalSettings.ActiveTab.CultureCode))
            {
				Logger.LogError("GetActivePortalLanguage(hostSettings={HostSettings},appStatus={AppStatus},portalId={PortalId}): portalSettings → {PortalSettings}", hostSettings, appStatus, portalId, portalSettings);
                return portalSettings.ActiveTab.CultureCode;
            }

            // PortalSettings IS Nothing - probably means we haven't set it yet (in Begin Request)
            // so try detecting the user's cookie
            if (context.Request["language"] != null)
            {
                language = context.Request["language"];
                isDefaultLanguage = false;
            }

            if ((!string.IsNullOrEmpty(language) && !isDefaultLanguage) || !EnableBrowserLanguageInDefault(hostSettings, appStatus, portalId))
            {
                return language;
            }

            var culture = Localization.GetBrowserCulture(portalId);
			Logger.LogError("GetActivePortalLanguage(portalID={PortalID}): culture → {Culture}", portalID, culture);
            if (culture != null)
            {
				Logger.LogError("GetActivePortalLanguage(hostSettings={HostSettings},appStatus={AppStatus},portalId={PortalId}): culture → {Culture}", hostSettings, appStatus, portalId, culture);
                language = culture.Name;
            }

			Logger.LogWarning("<<< Exiting GetActivePortalLanguage(hostSettings={HostSettings},appStatus={AppStatus},portalId={PortalId})", hostSettings, appStatus, portalId);
            return language;
        }

        /// <summary>return the current DefaultLanguage value from the Portals table for the requested <paramref name="portalID"/>.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>The language identifier.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial string GetPortalDefaultLanguage(int portalID)
            => GetPortalDefaultLanguage(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalID);

        /// <summary>return the current DefaultLanguage value from the Portals table for the requested <paramref name="portalId"/>.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The language identifier.</returns>
        public static string GetPortalDefaultLanguage(IHostSettings hostSettings, int portalId)
        {
			Logger.LogError(">>> Entering GetPortalDefaultLanguage(portalID={PortalID})", portalID);
            string cacheKey = $"PortalDefaultLanguage_{portalId}";
			Logger.LogError("GetPortalDefaultLanguage(portalID={PortalID}): cacheKey → {CacheKey}", portalID, cacheKey);
        Logger.LogError("GetPortalDefaultLanguage(hostSettings={HostSettings},portalId={PortalId}): cacheKey → {CacheKey}", hostSettings, portalId, cacheKey);
        Logger.LogWarning("<<< Exiting GetPortalDefaultLanguage(hostSettings={HostSettings},portalId={PortalId})", hostSettings, portalId);
            return CBO.GetCachedObject<string>(
                hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, portalId),
                GetPortalDefaultLanguageCallBack);
        }

        /// <summary>
        ///   set the required DefaultLanguage in the Portals table for a particular portal
        ///   saves having to update an entire PortalInfo object.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void UpdatePortalDefaultLanguage(int portalID, string cultureCode)
            => UpdatePortalDefaultLanguage(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(), portalID, cultureCode);

        /// <summary>
        ///   set the required DefaultLanguage in the Portals table for a particular portal
        ///   saves having to update an entire PortalInfo object.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        public static void UpdatePortalDefaultLanguage(DataProvider dataProvider, int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering UpdatePortalDefaultLanguage(portalID={PortalID},cultureCode={CultureCode})", portalID, cultureCode);
            dataProvider.UpdatePortalDefaultLanguage(portalId, cultureCode);

            // ensure localization record exists as new portal default language may be relying on fallback chain
            // of which it is now the final part
            dataProvider.EnsureLocalizationExists(portalId, cultureCode);
			Logger.LogWarning("<<< Exiting UpdatePortalDefaultLanguage(dataProvider={DataProvider},portalId={PortalId},cultureCode={CultureCode})", dataProvider, portalId, cultureCode);
        }

        /// <summary>Increments the Client Resource Manager version.</summary>
        /// <param name="portalID">The portal ID.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void IncrementCrmVersion(int portalID)
            => IncrementCrmVersion(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalID);

        /// <summary>Increments the Client Resource Manager version.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        public static void IncrementCrmVersion(IPortalController portalController, int portalId)
        {
			Logger.LogError(">>> Entering IncrementCrmVersion(portalID={PortalID})", portalID);
            var versionSetting = GetPortalSetting(portalController, Web.Client.ClientResourceSettings.VersionKey, portalId, "1");
            if (int.TryParse(versionSetting, out var currentVersion))
            {
				Logger.LogError("IncrementCrmVersion(portalController={PortalController},portalId={PortalId}): versionSetting → {VersionSetting}", portalController, portalId, versionSetting);
				Logger.LogError("IncrementCrmVersion(portalID={PortalID}): versionSetting → {VersionSetting}", portalID, versionSetting);
                var newVersion = currentVersion + 1;
                UpdatePortalSetting(portalController, portalId, Web.Client.ClientResourceSettings.VersionKey, newVersion.ToString(CultureInfo.InvariantCulture), true);
            }
			Logger.LogWarning("<<< Exiting IncrementCrmVersion(portalController={PortalController},portalId={PortalId})", portalController, portalId);
        }

        /// <summary>Increment the CRM version for all portals that are overriding the host CRM version.</summary>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void IncrementOverridingPortalsCrmVersion()
            => IncrementOverridingPortalsCrmVersion(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>());

        /// <summary>Increment the CRM version for all portals that are overriding the host CRM version.</summary>
        /// <param name="portalController">The portal controller.</param>
        public static void IncrementOverridingPortalsCrmVersion(IPortalController portalController)
        {
			Logger.LogError(">>> Entering IncrementOverridingPortalsCrmVersion()");
            foreach (IPortalInfo portal in portalController.GetPortals())
            {
                var setting = GetPortalSetting(portalController, Web.Client.ClientResourceSettings.OverrideDefaultSettingsKey, portal.PortalId, "False");
				Logger.LogError("IncrementOverridingPortalsCrmVersion(): setting → {Setting}", setting);
				Logger.LogError("IncrementOverridingPortalsCrmVersion(portalController={PortalController}): setting → {Setting}", portalController, setting);

                // if this portal is overriding the host level...
                if (bool.TryParse(setting, out var overriden) && overriden)
                {
                    // increment its version
                    IncrementCrmVersion(portalController, portal.PortalId);
                }
            }
			Logger.LogWarning("<<< Exiting IncrementOverridingPortalsCrmVersion(portalController={PortalController})", portalController);
        }

        /// <summary>Creates a new portal alias.</summary>
        /// <param name="portalId">ID of the portal.</param>
        /// <param name="portalAlias">Portal Alias to be created.</param>
        public void AddPortalAlias(int portalId, string portalAlias)
        {
			Logger.LogError(">>> Entering AddPortalAlias(portalId={PortalId},portalAlias={PortalAlias})", portalId, portalAlias);
            // Check if the Alias exists
            PortalAliasInfo portalAliasInfo = PortalAliasController.Instance.GetPortalAlias(portalAlias, portalId);
			Logger.LogError("AddPortalAlias(portalId={PortalId},portalAlias={PortalAlias}): portalAliasInfo → {PortalAliasInfo}", portalId, portalAlias, portalAliasInfo);

            // If alias does not exist add new
            if (portalAliasInfo == null)
            {
                portalAliasInfo = new PortalAliasInfo { PortalID = portalId, HTTPAlias = portalAlias, IsPrimary = true };
                PortalAliasController.Instance.AddPortalAlias(portalAliasInfo);
            }
			Logger.LogWarning("<<< Exiting AddPortalAlias(portalId={PortalId},portalAlias={PortalAlias})", portalId, portalAlias);
        }

        /// <summary>Copies the page template.</summary>
        /// <param name="templateFile">The template file.</param>
        /// <param name="mappedHomeDirectory">The mapped home directory.</param>
        public void CopyPageTemplate(string templateFile, string mappedHomeDirectory)
        {
			Logger.LogError(">>> Entering CopyPageTemplate(templateFile={TemplateFile},mappedHomeDirectory={MappedHomeDirectory})", templateFile, mappedHomeDirectory);
            string hostTemplateFile = $"{Globals.HostMapPath}Templates\\{templateFile}";
            if (!File.Exists(hostTemplateFile))
            {
                return;
            }

            string portalTemplateFolder = $"{mappedHomeDirectory}Templates\\";
            if (!Directory.Exists(portalTemplateFolder))
            {
                // Create Portal Templates folder
                Directory.CreateDirectory(portalTemplateFolder);
            }

            string portalTemplateFile = portalTemplateFolder + templateFile;
            if (!File.Exists(portalTemplateFile))
            {
                File.Copy(hostTemplateFile, portalTemplateFile);
            }
        }

        /// <summary>Creates the portal.</summary>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="adminUserId">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="template">The portal template.</param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <see langword="true"/> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        public int CreatePortal(string portalName, int adminUserId, string description, string keyWords, IPortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
			Logger.LogError(">>> Entering CreatePortal(portalName={PortalName},adminUserId={AdminUserId},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal})", portalName, adminUserId, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
            // Attempt to create a new portal
            int portalId = CreatePortal(this.hostSettings, portalName, homeDirectory, template.CultureCode);
			Logger.LogError("CreatePortal(portalName={PortalName},adminUserId={AdminUserId},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal}): portalId → {PortalId}", portalName, adminUserId, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, portalId);

            // Log the portal if into http context, if exception occurred in next step, we can remove the portal which is not really created.
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Add("CreatingPortalId", portalId);
            }

            string message = Null.NullString;

            if (portalId != -1)
            {
                // add administrator
                int administratorId = adminUserId;
                var adminUser = new UserInfo();
				Logger.LogError("CreatePortal(portalName={PortalName},adminUserId={AdminUserId},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal}): adminUser → {AdminUser}", portalName, adminUserId, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, adminUser);

                // add userportal record
                UserController.AddUserPortal(portalId, administratorId);

                // retrieve existing administrator
                try
                {
                    adminUser = UserController.GetUserById(portalId, administratorId);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                if (administratorId > 0)
                {
                    this.CreatePortalInternal(portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, ref message);
                }
            }
            else
            {
                message += Localization.GetString("CreatePortal.Error");
				Logger.LogError("Throwing CreatePortalException from CreatePortal(portalName={PortalName},adminUserId={AdminUserId},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal})", portalName, adminUserId, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
                throw new CreatePortalException(message);
            }

            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
				Logger.LogError("Exception in CreatePortal(portalName={PortalName},adminUserId={AdminUserId},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal}): {Arg10}", portalName, adminUserId, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, Exception.Message);
                // should be no exception, but suppress just in case
            }

            // remove the portal id from http context as there is no exception.
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains("CreatingPortalId"))
            {
                HttpContext.Current.Items.Remove("CreatingPortalId");
            }

			Logger.LogWarning("<<< Exiting CreatePortal(portalName={PortalName},adminUserId={AdminUserId},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal})", portalName, adminUserId, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
			Logger.LogWarning("<<< Exiting CreatePortal(portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal})", portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
			Logger.LogWarning("<<< Exiting CreatePortal(hostSettings={HostSettings},portalName={PortalName},homeDirectory={HomeDirectory},cultureCode={CultureCode})", hostSettings, portalName, homeDirectory, cultureCode);
            return portalId;
        }

        /// <summary>Creates the portal.</summary>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="adminUser">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="template">The portal template.</param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <see langword="true"/> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        public int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, IPortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            // Attempt to create a new portal
            int portalId = CreatePortal(this.hostSettings, portalName, homeDirectory, template.CultureCode);

            // Log the portal if into http context, if exception occurred in next step, we can remove the portal which is not really created.
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Add("CreatingPortalId", portalId);
            }

            string message = Null.NullString;

            if (portalId != -1)
            {
                // add administrator
                int administratorId = Null.NullInteger;
                adminUser.PortalID = portalId;
                try
                {
                    UserCreateStatus createStatus = UserController.CreateUser(ref adminUser);
                    if (createStatus == UserCreateStatus.Success)
                    {
                        administratorId = adminUser.UserID;

                        // reload the UserInfo as when it was first created, it had no portal id and therefore
                        // used host profile definitions
                        adminUser = UserController.GetUserById(adminUser.PortalID, adminUser.UserID);
                    }
                    else
                    {
                        message += UserController.GetUserCreateStatus(createStatus);
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    message += Localization.GetString("CreateAdminUser.Error") + exc.Message + exc.StackTrace;
                }

                if (administratorId > 0)
                {
                    this.CreatePortalInternal(portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, ref message);
                }
            }
            else
            {
                message += Localization.GetString("CreatePortal.Error");
                throw new CreatePortalException(message);
            }

            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                // should be no exception, but suppress just in case
            }

            // remove the portal id from http context as there is no exception.
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains("CreatingPortalId"))
            {
                HttpContext.Current.Items.Remove("CreatingPortalId");
            }

            return portalId;
        }

        /// <summary>Get all the available portal templates grouped by culture.</summary>
        /// <returns>List of PortalTemplateInfo objects.</returns>
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.GetPortalTemplates instead")]
        public partial IList<PortalTemplateInfo> GetAvailablePortalTemplates()
        {
			Logger.LogError(">>> Entering GetAvailablePortalTemplates()");
            var list = new List<PortalTemplateInfo>();

			Logger.LogError("GetAvailablePortalTemplates(): list → {List}", list);
            var templateFilePaths = PortalTemplateIO.Instance.EnumerateTemplates();
			Logger.LogError("GetAvailablePortalTemplates(): templateFilePaths → {TemplateFilePaths}", templateFilePaths);
            var languageFileNames = PortalTemplateIO.Instance.EnumerateLanguageFiles().Select(Path.GetFileName).ToList();
			Logger.LogError("GetAvailablePortalTemplates(): languageFileNames → {LanguageFileNames}", languageFileNames);

            foreach (string templateFilePath in templateFilePaths)
            {
                var currentFileName = Path.GetFileName(templateFilePath);
				Logger.LogError("GetAvailablePortalTemplates(): currentFileName → {CurrentFileName}", currentFileName);
                var langs = languageFileNames
                                .Where(x => GetTemplateName(x).Equals(currentFileName, StringComparison.OrdinalIgnoreCase))
                                .Select(x => GetCultureCode(x))
                                .Distinct()
                                .ToList();

                if (langs.Count != 0)
                {
                    langs.ForEach(x => list.Add(new PortalTemplateInfo(this.hostSettings, templateFilePath, x)));
                }
                else
                {
                    list.Add(new PortalTemplateInfo(this.hostSettings, templateFilePath, string.Empty));
                }
            }

			Logger.LogWarning("<<< Exiting GetAvailablePortalTemplates()");
            return list;
        }

        /// <summary>Gets information of a portal.</summary>
        /// <param name="portalId">ID of the portal.</param>
        /// <returns>PortalInfo object with portal definition.</returns>
        public PortalInfo GetPortal(int portalId)
        {
			Logger.LogError(">>> Entering GetPortal(portalId={PortalId})", portalId);
            if (portalId == -1)
            {
                return null;
            }

            string defaultLanguage = GetActivePortalLanguage(this.hostSettings, this.appStatus, portalId);
			Logger.LogError("GetPortal(portalId={PortalId}): defaultLanguage → {DefaultLanguage}", portalId, defaultLanguage);
            PortalInfo portal = this.GetPortal(portalId, defaultLanguage);
			Logger.LogError("GetPortal(portalId={PortalId}): portal → {Portal}", portalId, portal);
            if (portal == null)
            {
                // Active language may not be valid, so fallback to default language
                defaultLanguage = GetPortalDefaultLanguage(this.hostSettings, portalId);
                portal = this.GetPortal(portalId, defaultLanguage);
            }

			Logger.LogWarning("<<< Exiting GetPortal(portalId={PortalId})", portalId);
			Logger.LogWarning("<<< Exiting GetPortal(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
			Logger.LogWarning("<<< Exiting GetPortal(uniqueId={UniqueId})", uniqueId);
            return portal;
        }

        /// <summary>Gets information of a portal.</summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>The <see cref="PortalInfo"/> instance or <see langword="null"/>.</returns>
        public PortalInfo GetPortal(int portalId, string cultureCode)
        {
            if (portalId == -1)
            {
                return null;
            }

            var portal = GetPortalInternal(this, portalId, cultureCode);
            if (Localization.ActiveLanguagesByPortalID(portalId) <= 1 || portal != null)
            {
                return portal;
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetPortalDefaultLanguage(this.hostSettings, portalId);
            }

            var fallbackLanguage = string.Empty;
            var userLocale = LocaleController.Instance.GetLocale(cultureCode);
            if (userLocale != null && !string.IsNullOrEmpty(userLocale.Fallback))
            {
                fallbackLanguage = userLocale.Fallback;
            }

            if (string.IsNullOrEmpty(fallbackLanguage))
            {
                fallbackLanguage = Localization.SystemLocale;
            }

            portal = GetPortalInternal(this, portalId, fallbackLanguage);
            if (portal != null)
            {
                return portal;
            }

            // if we cannot find any fallback, it means it's a non-portal default language
            DataProvider.Instance().EnsureLocalizationExists(portalId, GetActivePortalLanguage(this.hostSettings, this.appStatus, portalId));
            DataCache.ClearHostCache(true);
            return GetPortalInternal(this, portalId, GetActivePortalLanguage(this.hostSettings, this.appStatus, portalId));
        }

        /// <summary>Gets the portal.</summary>
        /// <param name="uniqueId">The unique id.</param>
        /// <returns>Portal info.</returns>
        public PortalInfo GetPortal(Guid uniqueId)
        {
            return this.GetPortalList(Null.NullString).SingleOrDefault(p => p.GUID == uniqueId);
        }

        /// <summary>Gets information from all portals.</summary>
        /// <returns>ArrayList of PortalInfo objects.</returns>
        public ArrayList GetPortals()
        {
			Logger.LogError(">>> Entering GetPortals()");
            Logger.LogWarning("<<< Exiting GetPortals()");
            return new ArrayList(this.GetPortalList(Null.NullString));
        }

        /// <summary>Get portals in specific culture.</summary>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="PortalInfo"/> instances.</returns>
        public List<PortalInfo> GetPortalList(string cultureCode)
        {
			Logger.LogError(">>> Entering GetPortalList(cultureCode={CultureCode})", cultureCode);
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PortalCacheKey, Null.NullInteger, cultureCode);
			Logger.LogError("GetPortalList(cultureCode={CultureCode}): cacheKey → {CacheKey}", cultureCode, cacheKey);
			Logger.LogWarning("<<< Exiting GetPortalList(cultureCode={CultureCode})", cultureCode);
            return CBO.GetCachedObject<List<PortalInfo>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, cultureCode),
                c => CBO.FillCollection<PortalInfo>(DataProvider.Instance().GetPortals(cultureCode)));
        }

        /// <summary>Gets the portal settings dictionary.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>portal settings.</returns>
        public Dictionary<string, string> GetPortalSettings(int portalId)
        {
            return GetPortalSettingsDictionary(this.hostSettings, this.appStatus, portalId, string.Empty);
        }

        /// <summary>Gets the portal settings dictionary.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>portal settings.</returns>
        public Dictionary<string, string> GetPortalSettings(int portalId, string cultureCode)
        {
            return GetPortalSettingsDictionary(this.hostSettings, this.appStatus, portalId, cultureCode);
        }

        /// <summary>Load info for a portal template.</summary>
        /// <param name="templatePath">Full path to the portal template.</param>
        /// <param name="cultureCode">the culture code if any for the localization of the portal template.</param>
        /// <returns>A portal template.</returns>
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.GetPortalTemplate instead")]
        public partial PortalTemplateInfo GetPortalTemplate(string templatePath, string cultureCode)
        {
			Logger.LogError(">>> Entering GetPortalTemplate(templatePath={TemplatePath},cultureCode={CultureCode})", templatePath, cultureCode);
            var template = new PortalTemplateInfo(this.hostSettings, templatePath, cultureCode);

			Logger.LogError("GetPortalTemplate(templatePath={TemplatePath},cultureCode={CultureCode}): template → {Template}", templatePath, cultureCode, template);
            if (!string.IsNullOrEmpty(cultureCode) && template.CultureCode != cultureCode)
            {
                return null;
            }

			Logger.LogWarning("<<< Exiting GetPortalTemplate(templatePath={TemplatePath},cultureCode={CultureCode})", templatePath, cultureCode);
            return template;
        }

        /// <summary>Gets the portal space used bytes.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>Space used in bytes.</returns>
        public long GetPortalSpaceUsedBytes(int portalId)
        {
			Logger.LogError(">>> Entering GetPortalSpaceUsedBytes(portalId={PortalId})", portalId);
            long size = 0;
            IDataReader dr = DataProvider.Instance().GetPortalSpaceUsed(portalId);
			Logger.LogError("GetPortalSpaceUsedBytes(portalId={PortalId}): size → {Size}", portalId, size);
            try
            {
				Logger.LogError("GetPortalSpaceUsedBytes(portalId={PortalId}): dr → {Dr}", portalId, dr);
                if (dr.Read())
                {
                    if (dr["SpaceUsed"] != DBNull.Value)
                    {
                        size = Convert.ToInt64(dr["SpaceUsed"], CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in GetPortalSpaceUsedBytes(portalId={PortalId}): {Arg1}", portalId, ex.Message);
                Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogWarning("<<< Exiting GetPortalSpaceUsedBytes(portalId={PortalId})", portalId);
            return size;
        }

        /// <summary>Verifies if there's enough space to upload a new file on the given portal.</summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="fileSizeBytes">Size of the file being uploaded.</param>
        /// <returns>True if there's enough space available to upload the file.</returns>
        public bool HasSpaceAvailable(int portalId, long fileSizeBytes)
        {
			Logger.LogError(">>> Entering HasSpaceAvailable(portalId={PortalId},fileSizeBytes={FileSizeBytes})", portalId, fileSizeBytes);
            int hostSpace;
            if (portalId == -1)
            {
                hostSpace = 0;
				Logger.LogError("HasSpaceAvailable(portalId={PortalId},fileSizeBytes={FileSizeBytes}): hostSpace → {HostSpace}", portalId, fileSizeBytes, hostSpace);
            }
            else
            {
				Logger.LogDebug("HasSpaceAvailable(portalId={PortalId},fileSizeBytes={FileSizeBytes}): hostSpace → {HostSpace}", portalId, fileSizeBytes, hostSpace);
                PortalSettings ps = GetCurrentPortalSettingsInternal();
				Logger.LogError("HasSpaceAvailable(portalId={PortalId},fileSizeBytes={FileSizeBytes}): ps → {Ps}", portalId, fileSizeBytes, ps);
                if (ps != null && ps.PortalId == portalId)
                {
                    hostSpace = ps.HostSpace;
                }
                else
                {
                    PortalInfo portal = this.GetPortal(portalId);
					Logger.LogError("HasSpaceAvailable(portalId={PortalId},fileSizeBytes={FileSizeBytes}): portal → {Portal}", portalId, fileSizeBytes, portal);
                    hostSpace = portal.HostSpace;
                }
            }

			Logger.LogWarning("<<< Exiting HasSpaceAvailable(portalId={PortalId},fileSizeBytes={FileSizeBytes})", portalId, fileSizeBytes);
            return (((this.GetPortalSpaceUsedBytes(portalId) + fileSizeBytes) / Math.Pow(1024, 2)) <= hostSpace) || hostSpace == 0;
        }

        /// <inheritdoc />
        public void MapLocalizedSpecialPages(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            DataCache.ClearHostCache(true);
            DataProvider.Instance().EnsureLocalizationExists(portalId, cultureCode);

            PortalInfo defaultPortal = this.GetPortal(portalId, GetPortalDefaultLanguage(this.hostSettings, portalId));
			Logger.LogError("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): defaultPortal → {DefaultPortal}", portalId, cultureCode, defaultPortal);
            PortalInfo targetPortal = this.GetPortal(portalId, cultureCode);
			Logger.LogError("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal → {TargetPortal}", portalId, cultureCode, targetPortal);

            Locale targetLocale = LocaleController.Instance.GetLocale(cultureCode);
			Logger.LogError("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetLocale → {TargetLocale}", portalId, cultureCode, targetLocale);
            TabInfo tempTab;
            if (defaultPortal.HomeTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.HomeTabId, portalId, targetLocale);
				Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): tempTab → {TempTab}", portalId, cultureCode, tempTab);
                if (tempTab != null)
                {
					Logger.LogError("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): tempTab → {TempTab}", portalId, cultureCode, tempTab);
                    targetPortal.HomeTabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.HomeTabId → {Arg2}", portalId, cultureCode, targetPortal.HomeTabId);
                }
            }

            if (defaultPortal.LoginTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.LoginTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.LoginTabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.LoginTabId → {Arg2}", portalId, cultureCode, targetPortal.LoginTabId);
                }
            }

            if (defaultPortal.RegisterTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.RegisterTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.RegisterTabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.RegisterTabId → {Arg2}", portalId, cultureCode, targetPortal.RegisterTabId);
                }
            }

            if (defaultPortal.SplashTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.SplashTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.SplashTabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.SplashTabId → {Arg2}", portalId, cultureCode, targetPortal.SplashTabId);
                }
            }

            if (defaultPortal.UserTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.UserTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.UserTabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.UserTabId → {Arg2}", portalId, cultureCode, targetPortal.UserTabId);
                }
            }

            if (defaultPortal.SearchTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.SearchTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.SearchTabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.SearchTabId → {Arg2}", portalId, cultureCode, targetPortal.SearchTabId);
                }
            }

            if (defaultPortal.Custom404TabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.Custom404TabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.Custom404TabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.Custom404TabId → {Arg2}", portalId, cultureCode, targetPortal.Custom404TabId);
                }
            }

            if (defaultPortal.Custom500TabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.Custom500TabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.Custom500TabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.Custom500TabId → {Arg2}", portalId, cultureCode, targetPortal.Custom500TabId);
                }
            }

            if (defaultPortal.TermsTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.TermsTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.TermsTabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.TermsTabId → {Arg2}", portalId, cultureCode, targetPortal.TermsTabId);
                }
            }

            if (defaultPortal.PrivacyTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.PrivacyTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.PrivacyTabId = tempTab.TabID;
					Logger.LogDebug("MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode}): targetPortal.PrivacyTabId → {Arg2}", portalId, cultureCode, targetPortal.PrivacyTabId);
                }
            }

            this.UpdatePortalInternal(targetPortal, false);
			Logger.LogWarning("<<< Exiting MapLocalizedSpecialPages(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
        }

        /// <inheritdoc />
        public void RemovePortalLocalization(int portalId, string cultureCode, bool clearCache = true)
        {
			Logger.LogError(">>> Entering RemovePortalLocalization(portalId={PortalId},cultureCode={CultureCode},clearCache={ClearCache})", portalId, cultureCode, clearCache);
            DataProvider.Instance().RemovePortalLocalization(portalId, cultureCode);
            if (clearCache)
            {
                DataCache.ClearPortalCache(portalId, false);
            }
			Logger.LogWarning("<<< Exiting RemovePortalLocalization(portalId={PortalId},cultureCode={CultureCode},clearCache={ClearCache})", portalId, cultureCode, clearCache);
        }

        /// <summary>Processes a template file for the new portal.</summary>
        /// <param name="portalId">PortalId of the new portal.</param>
        /// <param name="template">The template.</param>
        /// <param name="administratorId">UserId for the portal administrator. This is used to assign roles to this user.</param>
        /// <param name="mergeTabs">Flag to determine whether Module content is merged.</param>
        /// <param name="isNewPortal">Flag to determine is the template is applied to an existing portal or a new one.</param>
        /// <remarks>
        /// The roles and settings nodes will only be processed on the portal template file.
        /// </remarks>
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.ApplyPortalTemplate instead")]
        public partial void ParseTemplate(int portalId, PortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
			Logger.LogError(">>> Entering ParseTemplate(portalId={PortalId},template={Template},administratorId={AdministratorId},mergeTabs={MergeTabs},isNewPortal={IsNewPortal})", portalId, template, administratorId, mergeTabs, isNewPortal);
            var portalTemplateImporter = new PortalTemplateImporter(
                this.permissionDefinitionService,
                this.businessControllerProvider,
                new ListController(this.eventLogger, this.hostSettings),
                this.eventLogger,
                this.hostSettings,
                this,
                this.appStatus,
                PortalGroupController.Instance,
                UserController.Instance,
                FileContentTypeManager.Instance,
                RoleProvider.Instance(),
                RoleController.Instance,
                new Templates.PortalTemplateInfo(template.TemplateFilePath, template.CultureCode));
            portalTemplateImporter.ParseTemplate(portalId, administratorId, mergeTabs.ToNewEnum(), isNewPortal);
			Logger.LogWarning("<<< Exiting ParseTemplate(portalId={PortalId},template={Template},administratorId={AdministratorId},mergeTabs={MergeTabs},isNewPortal={IsNewPortal})", portalId, template, administratorId, mergeTabs, isNewPortal);
        }

        /// <summary>Processes the resource file for the template file selected.</summary>
        /// <param name="portalPath">New portal's folder.</param>
        /// <param name="resoureceFile">full path to the resource file.</param>
        /// <remarks>
        /// The resource file is a zip file with the same name as the selected template file and with
        /// an extension of .resources (to disable this file being downloaded).
        /// For example: for template file "portal.template" a resource file "portal.template.resources" can be defined.
        /// </remarks>
        public void ProcessResourceFileExplicit(string portalPath, string resoureceFile)
        {
			Logger.LogError(">>> Entering ProcessResourceFileExplicit(portalPath={PortalPath},resoureceFile={ResoureceFile})", portalPath, resoureceFile);
            try
            {
                FileSystemUtils.UnzipResources(new ZipArchive(new FileStream(resoureceFile, FileMode.Open, FileAccess.Read)), portalPath);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
			Logger.LogWarning("<<< Exiting ProcessResourceFileExplicit(portalPath={PortalPath},resoureceFile={ResoureceFile})", portalPath, resoureceFile);
        }

        /// <summary>Updates the portal expiry.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="cultureCode">The culture code.</param>
        public void UpdatePortalExpiry(int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering UpdatePortalExpiry(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            var portal = this.GetPortal(portalId, cultureCode);

			Logger.LogError("UpdatePortalExpiry(portalId={PortalId},cultureCode={CultureCode}): portal → {Portal}", portalId, cultureCode, portal);
            if (portal.ExpiryDate == Null.NullDate)
            {
                portal.ExpiryDate = DateTime.Now;
				Logger.LogDebug("UpdatePortalExpiry(portalId={PortalId},cultureCode={CultureCode}): portal.ExpiryDate → {Arg2}", portalId, cultureCode, portal.ExpiryDate);
            }

            portal.ExpiryDate = portal.ExpiryDate.AddMonths(1);

            this.UpdatePortalInfo(portal);
			Logger.LogWarning("<<< Exiting UpdatePortalExpiry(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
        }

        /// <inheritdoc />
        public void UpdatePortalInfo(PortalInfo portal)
        {
			Logger.LogError(">>> Entering UpdatePortalInfo(portal={Portal})", portal);
            this.UpdatePortalInternal(portal, true);
			Logger.LogWarning("<<< Exiting UpdatePortalInfo(portal={Portal})", portal);
        }

        /// <inheritdoc cref="DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo" />
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo instead")]
        public partial int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, PortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            return this.CreatePortal(portalName, adminUser, description, keyWords, template.ToNewPortalTemplateInfo(), homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
        }

        /// <inheritdoc cref="DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo" />
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo instead")]
        public partial int CreatePortal(string portalName, int adminUserId, string description, string keyWords, PortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            return this.CreatePortal(portalName, adminUserId, description, keyWords, template.ToNewPortalTemplateInfo(), homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
        }

        /// <inheritdoc />
        [Obsolete("Deprecated in DotNetNuke 9.2.0. Use the overloaded one with the 'isSecure' parameter instead. Scheduled removal in v11.0.0.")]
        void IPortalController.UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode)
        {
            UpdatePortalSettingInternal(
                this,
                this.hostSettings,
                this.appStatus,
                this.eventLogger,
                portalID,
                settingName,
                settingValue,
                clearCache,
                cultureCode,
                false);
        }

        /// <inheritdoc />
        void IPortalController.UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure)
        {
            UpdatePortalSettingInternal(
                this,
                this.hostSettings,
                this.appStatus,
                this.eventLogger,
                portalID,
                settingName,
                settingValue,
                clearCache,
                cultureCode,
                isSecure);
        }

        /// <summary>Gets the current portal settings.</summary>
        /// <returns>portal settings.</returns>
        PortalSettings IPortalController.GetCurrentPortalSettings()
        {
            return GetCurrentPortalSettingsInternal();
        }

        /// <inheritdoc />
        IAbPortalSettings IPortalController.GetCurrentSettings()
        {
            return GetCurrentPortalSettingsInternal();
        }

        internal static void EnsureRequiredEventLogTypesExist()
        {
			Logger.LogError(">>> Entering EnsureRequiredEventLogTypesExist()");
            if (!DoesLogTypeExists(nameof(EventLogType.PAGE_NOT_FOUND_404)))
            {
                // Add 404 Log
                var logTypeInfo = new LogTypeInfo
                {
                    LogTypeKey = nameof(EventLogType.PAGE_NOT_FOUND_404),
                    LogTypeFriendlyName = "HTTP Error Code 404 Page Not Found",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeInfo);

                // Add LogType
                var logTypeConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = true,
                    LogTypeKey = nameof(EventLogType.PAGE_NOT_FOUND_404),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeConf);
            }

            if (!DoesLogTypeExists(nameof(EventLogType.IP_LOGIN_BANNED)))
            {
                // Add IP filter log type
                var logTypeFilterInfo = new LogTypeInfo
                {
                    LogTypeKey = nameof(EventLogType.IP_LOGIN_BANNED),
                    LogTypeFriendlyName = "HTTP Error Code Forbidden IP address rejected",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeFilterInfo);

                // Add LogType
                var logTypeFilterConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = true,
                    LogTypeKey = nameof(EventLogType.IP_LOGIN_BANNED),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeFilterConf);
            }

            if (!DoesLogTypeExists(nameof(EventLogType.TABURL_CREATED)))
            {
                var logTypeInfo = new LogTypeInfo
                {
                    LogTypeKey = nameof(EventLogType.TABURL_CREATED),
                    LogTypeFriendlyName = "TabURL created",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationSuccess",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = nameof(EventLogType.TABURL_UPDATED);
                logTypeInfo.LogTypeFriendlyName = "TabURL updated";
				Logger.LogDebug("EnsureRequiredEventLogTypesExist(): logTypeInfo.LogTypeFriendlyName → {Arg0}", logTypeInfo.LogTypeFriendlyName);
                LogController.Instance.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = nameof(EventLogType.TABURL_DELETED);
                logTypeInfo.LogTypeFriendlyName = "TabURL deleted";
                LogController.Instance.AddLogType(logTypeInfo);

                // Add LogType
                var logTypeUrlConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = false,
                    LogTypeKey = nameof(EventLogType.TABURL_CREATED),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);

                logTypeUrlConf.LogTypeKey = nameof(EventLogType.TABURL_UPDATED);
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);

                logTypeUrlConf.LogTypeKey = nameof(EventLogType.TABURL_DELETED);
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);
            }

            if (!DoesLogTypeExists(nameof(EventLogType.SCRIPT_COLLISION)))
            {
                // Add IP filter log type
                var logTypeFilterInfo = new LogTypeInfo
                {
                    LogTypeKey = nameof(EventLogType.SCRIPT_COLLISION),
                    LogTypeFriendlyName = "Javscript library registration resolved script collision",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeFilterInfo);

                // Add LogType
                var logTypeFilterConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = true,
                    LogTypeKey = nameof(EventLogType.SCRIPT_COLLISION),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeFilterConf);
            }
			Logger.LogWarning("<<< Exiting EnsureRequiredEventLogTypesExist()");
        }

        /// <inheritdoc />
        protected override Func<IPortalController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IPortalController>;
        }

        private static int CreatePortal(IHostSettings hostSettings, string portalName, string homeDirectory, string cultureCode)
        {
            // add portal
            int portalId = -1;
            try
            {
                // Use host settings as default values for these parameters
                // This can be overwritten on the portal template
                var hostController = HostController.Instance;
                var demoPeriod = TimeSpan.FromDays(hostController.GetInteger("DemoPeriod"));
                var datExpiryDate = demoPeriod > TimeSpan.Zero
                    ? Convert.ToDateTime(Globals.GetMediumDate(DateTime.Now.Add(demoPeriod).ToString(CultureInfo.InvariantCulture)), CultureInfo.CurrentCulture)
                    : Null.NullDate;

                var hostCurrency = hostController.GetString("HostCurrency");
                if (string.IsNullOrEmpty(hostCurrency))
                {
                    hostCurrency = "USD";
                }

                portalId = DataProvider.Instance().CreatePortal(
                    portalName,
                    hostCurrency,
                    datExpiryDate,
                    hostController.GetDouble("HostFee", 0),
                    hostSettings.HostSpace,
                    hostSettings.PageQuota,
                    hostSettings.UserQuota,
                    0, // site log history function has been removed.
                    homeDirectory,
                    cultureCode,
                    UserController.Instance.GetCurrentUserInfo().UserID);

                // clear portal cache
                DataCache.ClearHostCache(true);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                // should be no exception, but suppress just in case
            }

            return portalId;
        }

        private static void DeletePortalInternal(int portalId)
        {
			Logger.LogError(">>> Entering DeletePortalInternal(portalId={PortalId})", portalId);
            UserController.DeleteUsers(portalId, false, true);

            IPortalInfo portal = Instance.GetPortal(portalId);
			Logger.LogError("DeletePortalInternal(portalId={PortalId}): portal → {Portal}", portalId, portal);

            DataProvider.Instance().DeletePortalInfo(portalId);

            try
            {
                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = nameof(EventLogType.PORTAL_DELETED),
                };
                log.LogProperties.Add(new LogDetailInfo("Delete Portal:", portal.PortalName));
                log.LogProperties.Add(new LogDetailInfo("PortalID:", portal.PortalId.ToString(CultureInfo.InvariantCulture)));
                LogController.Instance.AddLog(log);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            DataCache.ClearHostCache(true);

            // queue remove portal from search index
            var document = new SearchDocumentToDelete
            {
                PortalId = portalId,
            };

            DataProvider.Instance().AddSearchDeletedItems(document);
			Logger.LogWarning("<<< Exiting DeletePortalInternal(portalId={PortalId})", portalId);
        }

        private static bool DoesLogTypeExists(string logTypeKey)
        {
			Logger.LogError(">>> Entering DoesLogTypeExists(logTypeKey={LogTypeKey})", logTypeKey);
            LogTypeInfo logType;
            Dictionary<string, LogTypeInfo> logTypeDictionary = LogController.Instance.GetLogTypeInfoDictionary();
			Logger.LogError("DoesLogTypeExists(logTypeKey={LogTypeKey}): logTypeDictionary → {LogTypeDictionary}", logTypeKey, logTypeDictionary);
            logTypeDictionary.TryGetValue(logTypeKey, out logType);
            if (logType == null)
            {
                return false;
            }

			Logger.LogWarning("<<< Exiting DoesLogTypeExists(logTypeKey={LogTypeKey})", logTypeKey);
            return true;
        }

        private static PortalSettings GetCurrentPortalSettingsInternal()
        {
			Logger.LogError(">>> Entering GetCurrentPortalSettingsInternal()");
            PortalSettings objPortalSettings = null;
			Logger.LogError("GetCurrentPortalSettingsInternal(): objPortalSettings → {ObjPortalSettings}", objPortalSettings);
            if (HttpContext.Current != null)
            {
                objPortalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

			Logger.LogWarning("<<< Exiting GetCurrentPortalSettingsInternal()");
            return objPortalSettings;
        }

        private static PortalInfo GetPortalInternal(PortalController portalController, int portalId, string cultureCode)
        {
			Logger.LogError(">>> Entering GetPortalInternal(portalController={PortalController},portalId={PortalId},cultureCode={CultureCode})", portalController, portalId, cultureCode);
			Logger.LogWarning("<<< Exiting GetPortalInternal(portalController={PortalController},portalId={PortalId},cultureCode={CultureCode})", portalController, portalId, cultureCode);
            return portalController.GetPortalList(cultureCode).SingleOrDefault(p => ((IPortalInfo)p).PortalId == portalId);
        }

        private static object GetPortalDefaultLanguageCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogError(">>> Entering GetPortalDefaultLanguageCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            int portalId = (int)cacheItemArgs.ParamList[0];
			Logger.LogError("GetPortalDefaultLanguageCallBack(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);
			Logger.LogWarning("<<< Exiting GetPortalDefaultLanguageCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return DataProvider.Instance().GetPortalDefaultLanguage(portalId);
        }

        private static object GetPortalDictionaryCallback(CacheItemArgs cacheItemArgs)
        {
			Logger.LogError(">>> Entering GetPortalDictionaryCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var hostSettings = (IHostSettings)cacheItemArgs.Params[0];
            var dataProvider = (DataProvider)cacheItemArgs.Params[1];
			Logger.LogError("GetPortalDictionaryCallback(cacheItemArgs={CacheItemArgs}): dataProvider → {DataProvider}", cacheItemArgs, dataProvider);
			Logger.LogError("GetPortalDictionaryCallback(cacheItemArgs={CacheItemArgs}): hostSettings → {HostSettings}", cacheItemArgs, hostSettings);
            var portalDic = new Dictionary<int, int>();
            if (hostSettings.PerformanceSetting == PerformanceSettings.NoCaching)
            {
				Logger.LogError("GetPortalDictionaryCallback(cacheItemArgs={CacheItemArgs}): portalDic → {PortalDic}", cacheItemArgs, portalDic);
                return portalDic;
            }

            // get all tabs
            const int intField = 0;
            using var dr = dataProvider.GetTabPaths(Null.NullInteger, Null.NullString);
			Logger.LogError("GetPortalDictionaryCallback(cacheItemArgs={CacheItemArgs}): dr → {Dr}", cacheItemArgs, dr);
            try
            {
                while (dr.Read())
                {
                    // add to dictionary
                    portalDic[Convert.ToInt32(Null.SetNull(dr["TabID"], intField), CultureInfo.InvariantCulture)] = Convert.ToInt32(Null.SetNull(dr["PortalID"], intField), CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in GetPortalDictionaryCallback(cacheItemArgs={CacheItemArgs}): {Arg1}", cacheItemArgs, exc.Message);
                Exceptions.LogException(exc);
            }

			Logger.LogWarning("<<< Exiting GetPortalDictionaryCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return portalDic;
        }

        private static object GetPortalSettingsDictionaryCallback(CacheItemArgs cacheItemArgs)
        {
			Logger.LogError(">>> Entering GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var portalId = (int)cacheItemArgs.ParamList[0];
            var dicSettings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			Logger.LogError("GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs}): dicSettings → {DicSettings}", cacheItemArgs, dicSettings);
			Logger.LogError("GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);

            if (portalId <= -1)
            {
                return dicSettings;
            }

            var cultureCode = Convert.ToString(cacheItemArgs.ParamList[1], CultureInfo.InvariantCulture);
			Logger.LogError("GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs}): cultureCode → {CultureCode}", cacheItemArgs, cultureCode);
            if (string.IsNullOrEmpty(cultureCode))
            {
                var hostSettings = (IHostSettings)cacheItemArgs.ParamList[2];
				Logger.LogError("GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs}): hostSettings → {HostSettings}", cacheItemArgs, hostSettings);
                var appStatus = (IApplicationStatusInfo)cacheItemArgs.ParamList[3];
				Logger.LogError("GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs}): appStatus → {AppStatus}", cacheItemArgs, appStatus);
                cultureCode = GetActivePortalLanguage(hostSettings, appStatus, portalId);
            }

            var dr = DataProvider.Instance().GetPortalSettings(portalId, cultureCode);
			Logger.LogError("GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs}): dr → {Dr}", cacheItemArgs, dr);
            try
            {
                while (dr.Read())
                {
                    if (dr.IsDBNull(1))
                    {
                        continue;
                    }

                    var key = dr.GetString(0);
					Logger.LogError("GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs}): key → {Key}", cacheItemArgs, key);
                    if (dicSettings.ContainsKey(key))
                    {
                        dicSettings[key] = dr.GetString(1);
                        var log = new LogInfo
                        {
                            LogTypeKey = nameof(EventLogType.ADMIN_ALERT),
                        };
                        log.AddProperty("Duplicate PortalSettings Key", key);
                        LogController.Instance.AddLog(log);
                    }
                    else
                    {
                        dicSettings.Add(key, dr.GetString(1));
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs}): {Arg1}", cacheItemArgs, exc.Message);
                Exceptions.LogException(exc);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogWarning("<<< Exiting GetPortalSettingsDictionaryCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return dicSettings;
        }

        private static void EnsureRequiredProvidersForFolderTypes()
        {
			Logger.LogError(">>> Entering EnsureRequiredProvidersForFolderTypes()");
            if (ComponentFactory.GetComponent<CryptographyProvider>() == null)
            {
                ComponentFactory.InstallComponents(new ProviderInstaller("cryptography", typeof(CryptographyProvider), typeof(FipsCompilanceCryptographyProvider)));
                ComponentFactory.RegisterComponentInstance<CryptographyProvider>(new FipsCompilanceCryptographyProvider());
            }
			Logger.LogWarning("<<< Exiting EnsureRequiredProvidersForFolderTypes()");
        }

        private static void EnsureFolderProviderRegistration<TAbstract>(FolderTypeConfig folderTypeConfig, XmlDocument webConfig)
            where TAbstract : class
        {
            var providerBusinessClassNode = webConfig.SelectSingleNode("configuration/dotnetnuke/folder/providers/add[@name='" + folderTypeConfig.Provider + "']");

            var typeClass = Type.GetType(providerBusinessClassNode.Attributes["type"].Value);
            if (typeClass != null)
            {
                ComponentFactory.RegisterComponentInstance<TAbstract>(folderTypeConfig.Provider, Activator.CreateInstance(typeClass));
            }
        }

        private static bool EnableBrowserLanguageInDefault(IHostSettings hostSettings, IApplicationStatusInfo appStatus, int portalId)
        {
			Logger.LogError(">>> Entering EnableBrowserLanguageInDefault(hostSettings={HostSettings},appStatus={AppStatus},portalId={PortalId})", hostSettings, appStatus, portalId);
            bool retValue = Null.NullBoolean;
			Logger.LogError("EnableBrowserLanguageInDefault(hostSettings={HostSettings},appStatus={AppStatus},portalId={PortalId}): retValue → {RetValue}", hostSettings, appStatus, portalId, retValue);
            try
            {
                GetPortalSettingsDictionary(hostSettings, appStatus, portalId, Localization.SystemLocale).TryGetValue("EnableBrowserLanguage", out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = hostSettings.EnableBrowserLanguage;
                }
                else
                {
                    retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

			Logger.LogWarning("<<< Exiting EnableBrowserLanguageInDefault(hostSettings={HostSettings},appStatus={AppStatus},portalId={PortalId})", hostSettings, appStatus, portalId);
            return retValue;
        }

        private static Dictionary<string, string> GetPortalSettingsDictionary(IHostSettings hostSettings, IApplicationStatusInfo appStatus, int portalId, string cultureCode)
        {
            var httpContext = HttpContext.Current;

            if (string.IsNullOrEmpty(cultureCode) && portalId > -1)
            {
                cultureCode = GetActivePortalLanguageFromHttpContext(hostSettings, appStatus, httpContext, portalId);
            }

            // Get PortalSettings from Context or from cache
            var dictionaryKey = string.Format(CultureInfo.InvariantCulture, HttpContextKeyPortalSettingsDictionary, portalId, cultureCode);
            Dictionary<string, string> dictionary = null;
            if (httpContext != null)
            {
                dictionary = httpContext.Items[dictionaryKey] as Dictionary<string, string>;
            }

            if (dictionary == null)
            {
                var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PortalSettingsCacheKey, portalId, cultureCode);
                dictionary = CBO.GetCachedObject<Dictionary<string, string>>(
                    hostSettings,
                    new CacheItemArgs(
                        cacheKey,
                        DataCache.PortalSettingsCacheTimeOut,
                        DataCache.PortalSettingsCachePriority,
                        portalId,
                        cultureCode,
                        hostSettings,
                        appStatus),
                    GetPortalSettingsDictionaryCallback,
                    true);
                if (httpContext != null)
                {
                    httpContext.Items[dictionaryKey] = dictionary;
                }
            }

            return dictionary;
        }

        private static string GetActivePortalLanguageFromHttpContext(IHostSettings hostSettings, IApplicationStatusInfo appStatus, HttpContext httpContext, int portalId)
        {
			Logger.LogError(">>> Entering GetActivePortalLanguageFromHttpContext(hostSettings={HostSettings},appStatus={AppStatus},httpContext={HttpContext},portalId={PortalId})", hostSettings, appStatus, httpContext, portalId);
            var cultureCode = string.Empty;

			Logger.LogError("GetActivePortalLanguageFromHttpContext(hostSettings={HostSettings},appStatus={AppStatus},httpContext={HttpContext},portalId={PortalId}): cultureCode → {CultureCode}", hostSettings, appStatus, httpContext, portalId, cultureCode);
            // Lookup culturecode but cache it in the HttpContext for performance
            var activeLanguageKey = $"ActivePortalLanguage{portalId}";
			Logger.LogError("GetActivePortalLanguageFromHttpContext(hostSettings={HostSettings},appStatus={AppStatus},httpContext={HttpContext},portalId={PortalId}): activeLanguageKey → {ActiveLanguageKey}", hostSettings, appStatus, httpContext, portalId, activeLanguageKey);
            if (httpContext != null)
            {
                cultureCode = (string)httpContext.Items[activeLanguageKey];
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetActivePortalLanguage(hostSettings, appStatus, portalId);
                if (httpContext != null)
                {
                    httpContext.Items[activeLanguageKey] = cultureCode;
                }
            }

			Logger.LogWarning("<<< Exiting GetActivePortalLanguageFromHttpContext(hostSettings={HostSettings},appStatus={AppStatus},httpContext={HttpContext},portalId={PortalId})", hostSettings, appStatus, httpContext, portalId);
            return cultureCode;
        }

        private static void UpdatePortalSettingInternal(IPortalController portalController, IHostSettings hostSettings, IApplicationStatusInfo appStatus, IEventLogger eventLogger, int portalId, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure)
        {
			Logger.LogError(">>> Entering UpdatePortalSettingInternal(portalController={PortalController},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},portalId={PortalId},settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache},cultureCode={CultureCode},isSecure={IsSecure})", portalController, hostSettings, appStatus, eventLogger, portalId, settingName, settingValue, clearCache, cultureCode, isSecure);
            string currentSetting = GetPortalSetting(portalController, settingName, portalId, string.Empty, cultureCode);

            if (isSecure && !string.IsNullOrEmpty(settingName) && !string.IsNullOrEmpty(settingValue))
            {
                settingValue = Security.FIPSCompliant.EncryptAES(HashAlgorithmName.SHA512, settingValue, Config.GetDecryptionkey(), hostSettings.Guid);
				Logger.LogError("UpdatePortalSettingInternal(portalController={PortalController},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},portalId={PortalId},settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache},cultureCode={CultureCode},isSecure={IsSecure}): settingValue → {SettingValue1}", portalController, hostSettings, appStatus, eventLogger, portalId, settingName, settingValue, clearCache, cultureCode, isSecure, settingValue);
                UpdatePortalSettingInternal(portalController, hostSettings, appStatus, eventLogger, portalId, CryptographyUtils.GetAlgorithmNameSettingKey(settingName), HashAlgorithmName.SHA512.Name, false, cultureCode, false);
            }

            if (currentSetting != settingValue)
            {
                DataProvider.Instance().UpdatePortalSetting(portalId, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID, cultureCode, isSecure);
                eventLogger.AddLog(settingName + ((cultureCode == Null.NullString) ? string.Empty : " (" + cultureCode + ")"), settingValue, GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogType.PORTAL_SETTING_UPDATED);
                if (clearCache)
                {
                    DataCache.ClearPortalCache(portalId, false);
                    DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey);

                    var httpContext = HttpContext.Current;
                    if (httpContext != null)
                    {
                        var cultureCodeForKey = GetActivePortalLanguageFromHttpContext(hostSettings, appStatus, httpContext, portalId);
						Logger.LogError("UpdatePortalSettingInternal(portalController={PortalController},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},portalId={PortalId},settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache},cultureCode={CultureCode},isSecure={IsSecure}): cultureCodeForKey → {CultureCodeForKey}", portalController, hostSettings, appStatus, eventLogger, portalId, settingName, settingValue, clearCache, cultureCode, isSecure, cultureCodeForKey);
                        var dictionaryKey = string.Format(CultureInfo.InvariantCulture, HttpContextKeyPortalSettingsDictionary, portalId, cultureCodeForKey);
						Logger.LogError("UpdatePortalSettingInternal(portalController={PortalController},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},portalId={PortalId},settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache},cultureCode={CultureCode},isSecure={IsSecure}): dictionaryKey → {DictionaryKey}", portalController, hostSettings, appStatus, eventLogger, portalId, settingName, settingValue, clearCache, cultureCode, isSecure, dictionaryKey);
                        httpContext.Items[dictionaryKey] = null;
                    }
                }

                EventManager.Instance.OnPortalSettingUpdated(new PortalSettingUpdatedEventArgs
                {
                    PortalId = portalId,
                    SettingName = settingName,
                    SettingValue = settingValue,
                });
            }
			Logger.LogWarning("<<< Exiting UpdatePortalSettingInternal(portalController={PortalController},hostSettings={HostSettings},appStatus={AppStatus},eventLogger={EventLogger},portalId={PortalId},settingName={SettingName},settingValue={SettingValue},clearCache={ClearCache},cultureCode={CultureCode},isSecure={IsSecure})", portalController, hostSettings, appStatus, eventLogger, portalId, settingName, settingValue, clearCache, cultureCode, isSecure);
        }

        private static string GetCultureCode(string languageFileName)
        {
			Logger.LogError(">>> Entering GetCultureCode(languageFileName={LanguageFileName})", languageFileName);
            // e.g. "default template.template.en-US.resx"
			Logger.LogWarning("<<< Exiting GetCultureCode(languageFileName={LanguageFileName})", languageFileName);
            return languageFileName.GetLocaleCodeFromFileName();
        }

        private static string GetTemplateName(string languageFileName)
        {
			Logger.LogError(">>> Entering GetTemplateName(languageFileName={LanguageFileName})", languageFileName);
            // e.g. "default template.template.en-US.resx"
			Logger.LogWarning("<<< Exiting GetTemplateName(languageFileName={LanguageFileName})", languageFileName);
            return languageFileName.GetFileNameFromLocalizedResxFile();
        }

        private void CreatePortalInternal(int portalId, string portalName, UserInfo adminUser, string description, string keyWords, IPortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal, ref string message)
        {
			Logger.LogError(">>> Entering CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message})", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message);
            // Add default workflows
            try
            {
                SystemWorkflowManager.Instance.CreateSystemWorkflows(portalId);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message}): {Arg12}", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message, ex.Message);
                Exceptions.LogException(ex);
            }

            var portalTemplateImporter = new PortalTemplateImporter(
                this.permissionDefinitionService,
                this.businessControllerProvider,
                new ListController(this.eventLogger, this.hostSettings),
                this.eventLogger,
                this.hostSettings,
                this,
                this.appStatus,
                PortalGroupController.Instance,
                UserController.Instance,
                FileContentTypeManager.Instance,
                RoleProvider.Instance(),
                RoleController.Instance,
                template);

            if (string.IsNullOrEmpty(homeDirectory))
            {
                homeDirectory = "Portals/" + portalId;
				Logger.LogError("CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message}): homeDirectory → {HomeDirectory1}", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message, homeDirectory);
            }

            string mappedHomeDirectory = $@"{this.appStatus.ApplicationMapPath}\{homeDirectory}\".Replace("/", @"\");

            if (Directory.Exists(mappedHomeDirectory))
            {
                message += string.Format(CultureInfo.CurrentCulture, Localization.GetString("CreatePortalHomeFolderExists.Error"), homeDirectory);
				Logger.LogError("Throwing CreatePortalException from CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message})", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message);
                throw new CreatePortalException(message);
            }

            message += portalTemplateImporter.CreateProfileDefinitions(portalId);

            if (string.IsNullOrEmpty(message))
            {
                try
                {
                    // the upload directory may already exist if this is a new DB working with a previously installed application
                    if (Directory.Exists(mappedHomeDirectory))
                    {
                        Globals.DeleteFolderRecursive(mappedHomeDirectory);
                    }
                }
                catch (Exception exc1)
                {
                    Logger.Error(exc1);
                    message += Localization.GetString("DeleteUploadFolder.Error") + exc1.Message + exc1.StackTrace;
                }

                // Set up Child Portal
                if (message == Null.NullString)
                {
                    if (isChildPortal)
                    {
                        message = CreateChildPortalFolder(childPath);
						Logger.LogError("CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message}): message → {Message1}", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message, message);
                    }
                }
                else
                {
                    throw new CreatePortalException(message);
                }

                if (message == Null.NullString)
                {
                    try
                    {
                        // create the upload directory for the new portal
                        Directory.CreateDirectory(mappedHomeDirectory);

                        // ensure that the Templates folder exists
                        string templateFolder = $"{mappedHomeDirectory}Templates";
                        if (!Directory.Exists(templateFolder))
                        {
                            Directory.CreateDirectory(templateFolder);
                        }

                        // ensure that the Users folder exists
                        string usersFolder = $"{mappedHomeDirectory}Users";
                        if (!Directory.Exists(usersFolder))
                        {
                            Directory.CreateDirectory(usersFolder);
                        }

                        // copy the default page template
                        this.CopyPageTemplate("Default.page.template", mappedHomeDirectory);

                        // process zip resource file if present
                        if (File.Exists(template.ResourceFilePath))
                        {
                            this.ProcessResourceFileExplicit(mappedHomeDirectory, template.ResourceFilePath);
                        }
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                        message += Localization.GetString("ChildPortal.Error") + exc1.Message + exc1.StackTrace;
                    }
                }
                else
                {
                    throw new CreatePortalException(message);
                }

                if (message == Null.NullString)
                {
                    try
                    {
                        FolderMappingController.Instance.AddDefaultFolderTypes(portalId);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                        message += Localization.GetString("DefaultFolderMappings.Error") + exc1.Message + exc1.StackTrace;
                    }
                }
                else
                {
                    throw new CreatePortalException(message);
                }

                LocaleCollection newPortalLocales = null;
                if (message == Null.NullString)
                {
                    try
                    {
                        this.CreatePredefinedFolderTypes(portalId);
                        portalTemplateImporter.ParseTemplateInternal(portalId, adminUser.UserID, PortalTemplateModuleAction.Replace.ToNewEnum(), true, out newPortalLocales);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                        message += Localization.GetString("PortalTemplate.Error") + exc1.Message + exc1.StackTrace;
                    }
                }
                else
                {
                    throw new CreatePortalException(message);
                }

                if (message == Null.NullString)
                {
                    var obsoletePortal = this.GetPortal(portalId);
					Logger.LogError("CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message}): obsoletePortal → {ObsoletePortal}", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message, obsoletePortal);
                    IPortalInfo portal = obsoletePortal;
                    portal.Description = description;
					Logger.LogDebug("CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message}): portal.Description → {Arg12}", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message, portal.Description);
                    portal.KeyWords = keyWords;
					Logger.LogDebug("CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message}): portal.KeyWords → {Arg12}", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message, portal.KeyWords);
                    portal.UserTabId = TabController.GetTabByTabPath(portal.PortalId, "//UserProfile", portal.CultureCode);
                    if (portal.UserTabId == -1)
                    {
                        portal.UserTabId = TabController.GetTabByTabPath(portal.PortalId, "//ActivityFeed", portal.CultureCode);
                    }

                    portal.SearchTabId = TabController.GetTabByTabPath(portal.PortalId, "//SearchResults", portal.CultureCode);
                    this.UpdatePortalInfo(obsoletePortal);

                    adminUser.Profile.PreferredLocale = portal.DefaultLanguage;
                    var portalSettings = new PortalSettings(obsoletePortal);
					Logger.LogError("CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message}): portalSettings → {PortalSettings}", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message, portalSettings);
                    adminUser.Profile.PreferredTimeZone = portalSettings.TimeZone;
                    UserController.UpdateUser(portal.PortalId, adminUser);

                    DesktopModuleController.AddDesktopModulesToPortal(portalId);

                    this.AddPortalAlias(portalId, portalAlias);

                    UpdatePortalSetting(portalId, "DefaultPortalAlias", portalAlias, false);

                    DataCache.ClearPortalCache(portalId, true);

                    if (newPortalLocales != null)
                    {
                        foreach (Locale newPortalLocale in newPortalLocales.AllValues)
                        {
                            Localization.AddLanguageToPortal(portalId, newPortalLocale.LanguageId, false);
                            if (portalSettings.ContentLocalizationEnabled)
                            {
                                this.MapLocalizedSpecialPages(portalId, newPortalLocale.Code);
                            }
                        }
                    }

                    try
                    {
                        RelationshipController.Instance.CreateDefaultRelationshipsForPortal(portalId);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                    }

                    // add profanity list to new portal
                    try
                    {
                        const string listName = "ProfanityFilter";
                        var listController = new ListController(this.eventLogger, this.hostSettings);
						Logger.LogError("CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message}): listController → {ListController}", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message, listController);
                        var entry = new ListEntryInfo
                        {
                            PortalID = portalId,
                            SystemList = false,
                            ListName = $"{listName}-{portalId}",
                        };
                        listController.AddListEntry(entry);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                    }

                    // add banned password list to new portal
                    try
                    {
                        const string listName = "BannedPasswords";
                        var listController = new ListController(this.eventLogger, this.hostSettings);
                        var entry = new ListEntryInfo
                        {
                            PortalID = portalId,
                            SystemList = false,
                            ListName = $"{listName}-{portalId}",
                        };
                        listController.AddListEntry(entry);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                    }

                    ServicesRoutingManager.ReRegisterServiceRoutesWhileSiteIsRunning();

                    try
                    {
                        var log = new LogInfo
                        {
                            BypassBuffering = true,
                            LogTypeKey = nameof(EventLogType.PORTAL_CREATED),
                        };
                        log.LogProperties.Add(new LogDetailInfo("Install Portal:", portalName));
                        log.LogProperties.Add(new LogDetailInfo("FirstName:", adminUser.FirstName));
                        log.LogProperties.Add(new LogDetailInfo("LastName:", adminUser.LastName));
                        log.LogProperties.Add(new LogDetailInfo("Username:", adminUser.Username));
                        log.LogProperties.Add(new LogDetailInfo("Email:", adminUser.Email));
                        log.LogProperties.Add(new LogDetailInfo("Description:", description));
                        log.LogProperties.Add(new LogDetailInfo("Keywords:", keyWords));
                        log.LogProperties.Add(new LogDetailInfo("Template:", template.TemplateFilePath));
                        log.LogProperties.Add(new LogDetailInfo("TemplateCulture:", template.CultureCode));
                        log.LogProperties.Add(new LogDetailInfo("HomeDirectory:", homeDirectory));
                        log.LogProperties.Add(new LogDetailInfo("PortalAlias:", portalAlias));
                        log.LogProperties.Add(new LogDetailInfo("ServerPath:", serverPath));
                        log.LogProperties.Add(new LogDetailInfo("ChildPath:", childPath));
                        log.LogProperties.Add(new LogDetailInfo("IsChildPortal:", isChildPortal.ToString()));
                        LogController.Instance.AddLog(log);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                    }

                    EventManager.Instance.OnPortalCreated(new PortalCreatedEventArgs { PortalId = portalId });
					Logger.LogWarning("<<< Exiting CreatePortalInternal(portalId={PortalId},portalName={PortalName},adminUser={AdminUser},description={Description},keyWords={KeyWords},template={Template},homeDirectory={HomeDirectory},portalAlias={PortalAlias},serverPath={ServerPath},childPath={ChildPath},isChildPortal={IsChildPortal},message={Message})", portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, message);
                }
                else
                {
                    throw new CreatePortalException(message);
                }
            }
            else
            {
                DeletePortalInternal(portalId);
                throw new CreatePortalException(message);
            }
        }

        private void CreatePredefinedFolderTypes(int portalId)
        {
			Logger.LogError(">>> Entering CreatePredefinedFolderTypes(portalId={PortalId})", portalId);
            try
            {
                EnsureRequiredProvidersForFolderTypes();
            }
            catch (Exception ex)
            {
                Logger.Error(Localization.GetString("CreatingConfiguredFolderMapping.Error"), ex);
            }

            var webConfig = Config.Load();
			Logger.LogError("CreatePredefinedFolderTypes(portalId={PortalId}): webConfig → {WebConfig}", portalId, webConfig);
            foreach (FolderTypeConfig folderTypeConfig in FolderMappingsConfigController.Instance.FolderTypes)
            {
                try
                {
                    EnsureFolderProviderRegistration<FolderProvider>(folderTypeConfig, webConfig);
                    FolderMappingController.Instance.AddFolderMapping(this.GetFolderMappingFromConfig(
                        folderTypeConfig,
                        portalId));
                }
                catch (Exception ex)
                {
                    Logger.Error(Localization.GetString("CreatingConfiguredFolderMapping.Error") + ": " + folderTypeConfig.Name, ex);
                }
            }
			Logger.LogWarning("<<< Exiting CreatePredefinedFolderTypes(portalId={PortalId})", portalId);
        }

        private (string SettingValue, string EncryptionAlgorithmName, string InitializationVector) EnsureSettingValue(FolderTypeSettingConfig settingNode, int portalId)
        {
			Logger.LogError(">>> Entering EnsureSettingValue(settingNode={SettingNode},portalId={PortalId})", settingNode, portalId);
            var ensuredSettingValue =
                settingNode.Value
                    .Replace("{PortalId}", (portalId != -1) ? portalId.ToString(CultureInfo.InvariantCulture) : "_default")
                    .Replace("{HostId}", this.hostSettings.Guid);
            if (settingNode.Encrypt)
            {
                return FolderProvider.EncryptValue(this.cryptographyProvider, this.hostSettings, ensuredSettingValue);
            }

			Logger.LogWarning("<<< Exiting EnsureSettingValue(settingNode={SettingNode},portalId={PortalId})", settingNode, portalId);
            return (ensuredSettingValue, null, null);
        }

        private FolderMappingInfo GetFolderMappingFromConfig(FolderTypeConfig node, int portalId)
        {
			Logger.LogError(">>> Entering GetFolderMappingFromConfig(node={Node},portalId={PortalId})", node, portalId);
            var folderMapping = new FolderMappingInfo
            {
                PortalID = portalId,
                MappingName = node.Name,
                FolderProviderType = node.Provider,
            };

            foreach (FolderTypeSettingConfig settingNode in node.Settings)
            {
                var (settingValue, algorithmName, initializationVector) = this.EnsureSettingValue(settingNode, portalId);
                folderMapping.FolderMappingSettings.Add(settingNode.Name, settingValue);
                if (!string.IsNullOrWhiteSpace(algorithmName))
                {
                    folderMapping.FolderMappingSettings.Add(FolderProvider.GetAlgorithmSettingKey(settingNode.Name), algorithmName);
                    folderMapping.FolderMappingSettings.Add(FolderProvider.GetInitializationVectorSettingKey(settingNode.Name), initializationVector);
                }
            }

			Logger.LogWarning("<<< Exiting GetFolderMappingFromConfig(node={Node},portalId={PortalId})", node, portalId);
            return folderMapping;
        }

        private void UpdatePortalInternal(PortalInfo portal, bool clearCache)
        {
			Logger.LogError(">>> Entering UpdatePortalInternal(portal={Portal},clearCache={ClearCache})", portal, clearCache);
            var processorPassword = portal.ProcessorPassword;
			Logger.LogError("UpdatePortalInternal(portal={Portal},clearCache={ClearCache}): processorPassword → {ProcessorPassword}", portal, clearCache, processorPassword);
            if (!string.IsNullOrEmpty(processorPassword))
            {
                processorPassword = Security.FIPSCompliant.EncryptAES(HashAlgorithmName.SHA1, processorPassword, Config.GetDecryptionkey(), this.hostSettings.Guid);
            }

            DataProvider.Instance().UpdatePortalInfo(
                portal.PortalID,
                portal.PortalGroupID,
                portal.PortalName,
                portal.LogoFile,
                portal.FooterText,
                portal.ExpiryDate,
                portal.UserRegistration,
                portal.BannerAdvertising,
                portal.Currency,
                portal.AdministratorId,
                portal.HostFee,
                portal.HostSpace,
                portal.PageQuota,
                portal.UserQuota,
                portal.PaymentProcessor,
                portal.ProcessorUserId,
                processorPassword,
                portal.Description,
                portal.KeyWords,
                portal.BackgroundFile,
                0, // site log history function has been removed.
                portal.SplashTabId,
                portal.HomeTabId,
                portal.LoginTabId,
                portal.RegisterTabId,
                portal.UserTabId,
                portal.SearchTabId,
                portal.Custom404TabId,
                portal.Custom500TabId,
                portal.TermsTabId,
                portal.PrivacyTabId,
                portal.DefaultLanguage,
                portal.HomeDirectory,
                UserController.Instance.GetCurrentUserInfo().UserID,
                portal.CultureCode);

            this.eventLogger.AddLog(
                "PortalId",
                portal.PortalID.ToString(CultureInfo.InvariantCulture),
                GetCurrentPortalSettingsInternal(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.PORTALINFO_UPDATED);

            // ensure a localization item exists (in case a new default language has been set)
            DataProvider.Instance().EnsureLocalizationExists(portal.PortalID, portal.DefaultLanguage);

            // clear portal cache
            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }
			Logger.LogWarning("<<< Exiting UpdatePortalInternal(portal={Portal},clearCache={ClearCache})", portal, clearCache);
        }

        /// <inheritdoc cref="DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo" />
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo instead")]
        public partial class PortalTemplateInfo
        {
            private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalController));
            private readonly IHostSettings hostSettings;
            private readonly string originalCultureCode;
            private string resourceFilePath;

            /// <summary>Initializes a new instance of the <see cref="PortalTemplateInfo"/> class.</summary>
            /// <param name="templateFilePath">The path to the template file.</param>
            /// <param name="cultureCode">The culture code.</param>
            public PortalTemplateInfo(string templateFilePath, string cultureCode)
                : this(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), templateFilePath, cultureCode)
            {
			Logger.LogError(">>> Entering PortalTemplateInfo(templateFilePath={TemplateFilePath},cultureCode={CultureCode})", templateFilePath, cultureCode);
            Logger.LogDebug("PortalTemplateInfo(hostSettings={HostSettings},templateFilePath={TemplateFilePath},cultureCode={CultureCode}): TemplateFilePath → {TemplateFilePath1}", hostSettings, templateFilePath, cultureCode, TemplateFilePath);
            Logger.LogWarning("<<< Exiting PortalTemplateInfo(templateFilePath={TemplateFilePath},cultureCode={CultureCode})", templateFilePath, cultureCode);
            }

            /// <summary>Initializes a new instance of the <see cref="PortalTemplateInfo"/> class.</summary>
            /// <param name="hostSettings">The host settings.</param>
            /// <param name="templateFilePath">The path to the template file.</param>
            /// <param name="cultureCode">The culture code.</param>
            public PortalTemplateInfo(IHostSettings hostSettings, string templateFilePath, string cultureCode)
            {
				Logger.LogError(">>> Entering PortalTemplateInfo(hostSettings={HostSettings},templateFilePath={TemplateFilePath},cultureCode={CultureCode})", hostSettings, templateFilePath, cultureCode);
				Logger.LogDebug("PortalTemplateInfo(hostSettings={HostSettings},templateFilePath={TemplateFilePath},cultureCode={CultureCode}): hostSettings → {HostSettings1}", hostSettings, templateFilePath, cultureCode, hostSettings);
				Logger.LogDebug("PortalTemplateInfo(hostSettings={HostSettings},templateFilePath={TemplateFilePath},cultureCode={CultureCode}): this.hostSettings → {Arg3}", hostSettings, templateFilePath, cultureCode, this.hostSettings);
                this.TemplateFilePath = templateFilePath;
				Logger.LogWarning("<<< Exiting PortalTemplateInfo(hostSettings={HostSettings},templateFilePath={TemplateFilePath},cultureCode={CultureCode})", hostSettings, templateFilePath, cultureCode);
				Logger.LogDebug("PortalTemplateInfo(hostSettings={HostSettings},templateFilePath={TemplateFilePath},cultureCode={CultureCode}): this.TemplateFilePath → {Arg3}", hostSettings, templateFilePath, cultureCode, this.TemplateFilePath);
                this.hostSettings = hostSettings;
                this.originalCultureCode = cultureCode;

                this.InitLocalizationFields(cultureCode);
				Logger.LogDebug("PortalTemplateInfo(hostSettings={HostSettings},templateFilePath={TemplateFilePath},cultureCode={CultureCode}): this.originalCultureCode → {Arg3}", hostSettings, templateFilePath, cultureCode, this.originalCultureCode);
                this.InitNameAndDescription();
            }

            public string ResourceFilePath
            {
                get
                {
                    if (this.resourceFilePath == null)
                    {
                        this.resourceFilePath = PortalTemplateIO.Instance.GetResourceFilePath(this.TemplateFilePath);
                    }

                    return this.resourceFilePath;
                }
            }

            public string Name { get; private set; }

            public string CultureCode { get; private set; }

            public string TemplateFilePath { get; private set; }

            public string LanguageFilePath { get; private set; }

            public string Description { get; private set; }

            internal DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo ToNewPortalTemplateInfo()
            {
				Logger.LogError(">>> Entering ToNewPortalTemplateInfo()");
                Logger.LogWarning("<<< Exiting ToNewPortalTemplateInfo()");
                return new DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo(this.TemplateFilePath, this.originalCultureCode);
            }

            private static string ReadLanguageFileValue(XDocument xmlDoc, string name)
            {
				Logger.LogError(">>> Entering ReadLanguageFileValue(xmlDoc={XmlDoc},name={Name})", xmlDoc, name);
                Logger.LogWarning("<<< Exiting ReadLanguageFileValue(xmlDoc={XmlDoc},name={Name})", xmlDoc, name);
                return (from f in xmlDoc.Descendants("data")
                        where (string)f.Attribute("name") == name
                        select (string)f.Element("value")).SingleOrDefault();
            }

            private void InitNameAndDescription()
            {
				Logger.LogError(">>> Entering InitNameAndDescription()");
                if (!string.IsNullOrEmpty(this.LanguageFilePath))
                {
                    this.LoadNameAndDescriptionFromLanguageFile();
                }

                if (string.IsNullOrEmpty(this.Name))
                {
                    this.Name = Path.GetFileNameWithoutExtension(this.TemplateFilePath);
                }

                if (string.IsNullOrEmpty(this.Description))
                {
                    this.LoadDescriptionFromTemplateFile();
                }
				Logger.LogWarning("<<< Exiting InitNameAndDescription()");
            }

            private void LoadDescriptionFromTemplateFile()
            {
				Logger.LogError(">>> Entering LoadDescriptionFromTemplateFile()");
                try
                {
                    XDocument xmlDoc;
                    using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.TemplateFilePath))
                    {
                        xmlDoc = XDocument.Load(reader);
						Logger.LogError("LoadDescriptionFromTemplateFile(): xmlDoc → {XmlDoc}", xmlDoc);
						Logger.LogDebug("LoadDescriptionFromTemplateFile(): xmlDoc → {XmlDoc}", xmlDoc);
                    }

                    this.Description = xmlDoc.Elements("portal").Elements("description").SingleOrDefault().Value;
                }
                catch (Exception e)
                {
                    Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
                }
				Logger.LogWarning("<<< Exiting LoadDescriptionFromTemplateFile()");
            }

            private void LoadNameAndDescriptionFromLanguageFile()
            {
				Logger.LogError(">>> Entering LoadNameAndDescriptionFromLanguageFile()");
                try
                {
                    using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.LanguageFilePath))
                    {
                        var xmlDoc = XDocument.Load(reader);
						Logger.LogError("LoadNameAndDescriptionFromLanguageFile(): xmlDoc → {XmlDoc}", xmlDoc);

                        this.Name = ReadLanguageFileValue(xmlDoc, "LocalizedTemplateName.Text");
                        this.Description = ReadLanguageFileValue(xmlDoc, "PortalDescription.Text");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
                }
				Logger.LogWarning("<<< Exiting LoadNameAndDescriptionFromLanguageFile()");
            }

            private void InitLocalizationFields(string cultureCode)
            {
				Logger.LogError(">>> Entering InitLocalizationFields(cultureCode={CultureCode})", cultureCode);
                this.LanguageFilePath = PortalTemplateIO.Instance.GetLanguageFilePath(this.TemplateFilePath, cultureCode);
                if (string.IsNullOrEmpty(this.LanguageFilePath))
                {
                    (cultureCode, _) = PortalTemplateIO.Instance.GetTemplateLanguages(this.TemplateFilePath);
                    if (string.IsNullOrEmpty(cultureCode))
                    {
                        var portalSettings = PortalSettings.Current;
                        cultureCode = portalSettings != null ? PortalController.GetPortalDefaultLanguage(this.hostSettings, portalSettings.PortalId) : Localization.SystemLocale;
						Logger.LogError("InitLocalizationFields(cultureCode={CultureCode}): cultureCode → {CultureCode1}", cultureCode, cultureCode);
                    }
                }
				Logger.LogWarning("<<< Exiting InitLocalizationFields(cultureCode={CultureCode})", cultureCode);

                this.CultureCode = cultureCode;
            }
        }
    }
}
