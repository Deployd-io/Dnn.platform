// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;

    public sealed class StandardTabAndModuleInfoProvider : ITabAndModuleInfoProvider
    {
        private const string ModuleIdKey = "ModuleId";
        private const string TabIdKey = "TabId";
        private const string MonikerQueryKey = "Moniker";
        private const string MonikerHeaderKey = "X-DNN-MONIKER";
        private const string MonikerSettingsKey = "Moniker";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StandardTabAndModuleInfoProvider));

        /// <inheritdoc />
        public bool TryFindTabId(HttpRequestBase request, out int tabId)
        {
			Logger.LogDebug(">>> Entering TryFindTabId(request={Request})", request);
			Logger.LogDebug("<<< Exiting TryFindTabId(request={Request})", request);
			Logger.LogDebug("<<< Exiting TryFindTabId(request={Request},tryMoniker={TryMoniker})", request, tryMoniker);
            return TryFindTabId(request, out tabId, true);
        }

        /// <inheritdoc />
        public bool TryFindModuleId(HttpRequestBase request, out int moduleId)
        {
			Logger.LogDebug(">>> Entering TryFindModuleId(request={Request})", request);
			Logger.LogDebug("<<< Exiting TryFindModuleId(request={Request})", request);
			Logger.LogDebug("<<< Exiting TryFindModuleId(request={Request},tryMoniker={TryMoniker})", request, tryMoniker);
            return TryFindModuleId(request, out moduleId, true);
        }

        /// <inheritdoc />
        public bool TryFindModuleInfo(HttpRequestBase request, out ModuleInfo moduleInfo)
        {
			Logger.LogDebug(">>> Entering TryFindModuleInfo(request={Request})", request);
            int tabId, moduleId;
            if (TryFindTabId(request, out tabId, false) && TryFindModuleId(request, out moduleId, false))
            {
                moduleInfo = ModuleController.Instance.GetModule(moduleId, tabId, false);
				Logger.LogDebug("TryFindModuleInfo(request={Request}): moduleInfo → {ModuleInfo}", request, moduleInfo);
			Logger.LogDebug("<<< Exiting TryFindModuleInfo(request={Request})", request);
                return moduleInfo != null;
            }

            return TryFindByMoniker(request, out moduleInfo);
        }

        private static bool TryFindTabId(HttpRequestBase request, out int tabId, bool tryMoniker)
        {
            tabId = FindInt(request, TabIdKey);
            if (tabId > Null.NullInteger)
            {
                return true;
            }

            if (tryMoniker)
            {
                ModuleInfo moduleInfo;
                if (TryFindByMoniker(request, out moduleInfo))
                {
                    tabId = moduleInfo.TabID;
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindModuleId(HttpRequestBase request, out int moduleId, bool tryMoniker)
        {
            moduleId = FindInt(request, ModuleIdKey);
            if (moduleId > Null.NullInteger)
            {
                return true;
            }

            if (tryMoniker)
            {
                ModuleInfo moduleInfo;
                if (TryFindByMoniker(request, out moduleInfo))
                {
                    moduleId = moduleInfo.ModuleID;
                    return true;
                }
            }

            return false;
        }

        private static int FindInt(HttpRequestBase requestBase, string key)
        {
			Logger.LogDebug(">>> Entering FindInt(requestBase={RequestBase},key={Key})", requestBase, key);
            string value = null;
            if (requestBase.Headers[key] != null)
            {
                value = requestBase.Headers[key];
            }

            if (string.IsNullOrEmpty(value) && requestBase.Url != null)
            {
                var queryString = HttpUtility.ParseQueryString(requestBase.Url.Query);
				Logger.LogDebug("FindInt(requestBase={RequestBase},key={Key}): queryString → {QueryString}", requestBase, key, queryString);
                value = queryString[key];
            }

            int id;
			Logger.LogDebug("<<< Exiting FindInt(requestBase={RequestBase},key={Key})", requestBase, key);
            return int.TryParse(value, out id) ? id : Null.NullInteger;
        }

        private static bool TryFindByMoniker(HttpRequestBase requestBase, out ModuleInfo moduleInfo)
        {
			Logger.LogDebug(">>> Entering TryFindByMoniker(requestBase={RequestBase})", requestBase);
            var id = FindIntInHeader(requestBase, MonikerHeaderKey);
            if (id <= Null.NullInteger)
            {
                id = FindIntInQueryString(requestBase, MonikerQueryKey);
            }

            moduleInfo = id > Null.NullInteger ? ModuleController.Instance.GetTabModule(id) : null;
			Logger.LogDebug("TryFindByMoniker(requestBase={RequestBase}): moduleInfo → {ModuleInfo}", requestBase, moduleInfo);
			Logger.LogDebug("<<< Exiting TryFindByMoniker(requestBase={RequestBase})", requestBase);
            return moduleInfo != null;
        }

        private static int FindIntInHeader(HttpRequestBase requestBase, string key)
        {
			Logger.LogDebug(">>> Entering FindIntInHeader(requestBase={RequestBase},key={Key})", requestBase, key);
            string value = null;
            if (requestBase.Headers[key] != null)
            {
                value = requestBase.Headers[key];
            }

			Logger.LogDebug("<<< Exiting FindIntInHeader(requestBase={RequestBase},key={Key})", requestBase, key);
            return GetTabModuleInfoFromMoniker(value);
        }

        private static int FindIntInQueryString(HttpRequestBase requestBase, string key)
        {
			Logger.LogDebug(">>> Entering FindIntInQueryString(requestBase={RequestBase},key={Key})", requestBase, key);
            string value = null;
            if (requestBase.Url != null)
            {
                var queryString = HttpUtility.ParseQueryString(requestBase.Url.Query);
				Logger.LogDebug("FindIntInQueryString(requestBase={RequestBase},key={Key}): queryString → {QueryString}", requestBase, key, queryString);
                value = queryString[key];
            }

			Logger.LogDebug("<<< Exiting FindIntInQueryString(requestBase={RequestBase},key={Key})", requestBase, key);
            return GetTabModuleInfoFromMoniker(value);
        }

        private static int GetTabModuleInfoFromMoniker(string monikerValue)
        {
			Logger.LogDebug(">>> Entering GetTabModuleInfoFromMoniker(monikerValue={MonikerValue})", monikerValue);
            monikerValue = (monikerValue ?? string.Empty).Trim();
            if (monikerValue.Length > 0)
            {
                var ids = TabModulesController.Instance.GetTabModuleIdsBySetting(MonikerSettingsKey, monikerValue);
                if (ids != null && ids.Any())
                {
                    return ids.First();
                }

                if (Logger.IsWarnEnabled)
                {
                    Logger.WarnFormat(CultureInfo.InvariantCulture, "The specified moniker ({0}) is not defined in the system", monikerValue);
                }
            }

			Logger.LogDebug("<<< Exiting GetTabModuleInfoFromMoniker(monikerValue={MonikerValue})", monikerValue);
            return Null.NullInteger;
        }
    }
}
