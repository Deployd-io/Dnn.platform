// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.ModuleCache;
    using DotNetNuke.Services.Tokens;
    using Newtonsoft.Json;

    /// <summary>ModuleInfo provides the Entity Layer for Modules.</summary>
    [XmlRoot("module", IsNullable = false)]
    [Serializable]
    public class ModuleInfo : ContentItem, IPropertyAccess
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleInfo));
        private string authorizedEditRoles;
        private string authorizedViewRoles;
        private string cultureCode;
        private Guid defaultLanguageGuid;
        private ModuleInfo defaultLanguageModule;
        private DesktopModuleInfo desktopModule;
        private Dictionary<string, ModuleInfo> localizedModules;
        private Guid localizedVersionGuid;
        private ModuleControlInfo moduleControl;
        private ModuleDefinitionInfo moduleDefinition;
        private ModulePermissionCollection modulePermissions;
        private TabInfo parentTab;
        private Hashtable moduleSettings;
        private Hashtable tabModuleSettings;

        /// <summary>Initializes a new instance of the <see cref="ModuleInfo"/> class.</summary>
        public ModuleInfo()
        {
			Logger.LogDebug(">>> Entering ModuleInfo()");
            // initialize the properties that can be null
            // in the database
            this.PortalID = Null.NullInteger;
			Logger.LogDebug("ModuleInfo(): PortalID → {PortalID}", PortalID);
			Logger.LogDebug("ModuleInfo(): this.PortalID → {Arg0}", this.PortalID);
            this.OwnerPortalID = Null.NullInteger;
            this.TabModuleID = Null.NullInteger;
			Logger.LogDebug("ModuleInfo(): OwnerPortalID → {OwnerPortalID}", OwnerPortalID);
			Logger.LogDebug("ModuleInfo(): this.OwnerPortalID → {Arg0}", this.OwnerPortalID);
			Logger.LogDebug("ModuleInfo(): TabModuleID → {TabModuleID}", TabModuleID);
			Logger.LogDebug("ModuleInfo(): this.TabModuleID → {Arg0}", this.TabModuleID);
            this.DesktopModuleID = Null.NullInteger;
            this.ModuleDefID = Null.NullInteger;
            this.ModuleTitle = Null.NullString;
            this.ModuleVersion = Null.NullInteger;
			Logger.LogDebug("ModuleInfo(): DesktopModuleID → {DesktopModuleID}", DesktopModuleID);
			Logger.LogDebug("ModuleInfo(): this.DesktopModuleID → {Arg0}", this.DesktopModuleID);
			Logger.LogDebug("ModuleInfo(): ModuleDefID → {ModuleDefID}", ModuleDefID);
			Logger.LogDebug("ModuleInfo(): this.ModuleDefID → {Arg0}", this.ModuleDefID);
			Logger.LogDebug("ModuleInfo(): ModuleTitle → {ModuleTitle}", ModuleTitle);
			Logger.LogDebug("ModuleInfo(): this.ModuleTitle → {Arg0}", this.ModuleTitle);
			Logger.LogDebug("ModuleInfo(): this.ModuleVersion → {Arg0}", this.ModuleVersion);
            this.authorizedEditRoles = Null.NullString;
            this.authorizedViewRoles = Null.NullString;
            this.Alignment = Null.NullString;
            this.Color = Null.NullString;
            this.Border = Null.NullString;
            this.IconFile = Null.NullString;
            this.Header = Null.NullString;
            this.Footer = Null.NullString;
			Logger.LogDebug("ModuleInfo(): this.authorizedEditRoles → {Arg0}", this.authorizedEditRoles);
			Logger.LogDebug("ModuleInfo(): this.authorizedViewRoles → {Arg0}", this.authorizedViewRoles);
			Logger.LogDebug("ModuleInfo(): Alignment → {Alignment}", Alignment);
			Logger.LogDebug("ModuleInfo(): this.Alignment → {Arg0}", this.Alignment);
			Logger.LogDebug("ModuleInfo(): Color → {Color}", Color);
			Logger.LogDebug("ModuleInfo(): this.Color → {Arg0}", this.Color);
			Logger.LogDebug("ModuleInfo(): Border → {Border}", Border);
			Logger.LogDebug("ModuleInfo(): this.Border → {Arg0}", this.Border);
			Logger.LogDebug("ModuleInfo(): IconFile → {IconFile}", IconFile);
			Logger.LogDebug("ModuleInfo(): this.IconFile → {Arg0}", this.IconFile);
			Logger.LogDebug("ModuleInfo(): Header → {Header}", Header);
			Logger.LogDebug("ModuleInfo(): this.Header → {Arg0}", this.Header);
			Logger.LogDebug("ModuleInfo(): Footer → {Footer}", Footer);
			Logger.LogDebug("ModuleInfo(): this.Footer → {Arg0}", this.Footer);
            this.StartDate = Null.NullDate;
            this.EndDate = Null.NullDate;
            this.ContainerSrc = Null.NullString;
            this.DisplayTitle = true;
            this.DisplayPrint = true;
            this.DisplaySyndicate = false;

            // Guid, Version Guid, and Localized Version Guid should be initialised to a new value
            this.UniqueId = Guid.NewGuid();
            this.VersionGuid = Guid.NewGuid();
            this.localizedVersionGuid = Guid.NewGuid();

            // Default Language Guid should be initialised to a null Guid
            this.defaultLanguageGuid = Null.NullGuid;
			Logger.LogDebug("<<< Exiting ModuleInfo()");
        }

        /// <summary>Gets the Associated Desktop Module.</summary>
        /// <returns>An Integer.</returns>
        [XmlIgnore]
        [JsonIgnore]
        public DesktopModuleInfo DesktopModule
        {
            get
            {
                return this.desktopModule ??
                       (this.desktopModule = this.DesktopModuleID > Null.NullInteger
                            ? DesktopModuleController.GetDesktopModule(this.DesktopModuleID, this.PortalID)
                            : new DesktopModuleInfo());
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public bool HideAdminBorder
        {
            get
            {
                object setting = this.TabModuleSettings["hideadminborder"];
                if (setting == null || string.IsNullOrEmpty(setting.ToString()))
                {
                    return false;
                }

                if (bool.TryParse(setting.ToString(), out var val))
                {
                    return val;
                }

                return false;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public bool IsShared
        {
            get { return this.OwnerPortalID != this.PortalID; }
        }

        public ModuleControlInfo ModuleControl
        {
            get
            {
                return this.moduleControl ??
                       (this.moduleControl = this.ModuleControlId > Null.NullInteger
                            ? ModuleControlController.GetModuleControl(this.ModuleControlId)
                            : new ModuleControlInfo());
            }
        }

        /// <summary>Gets the Associated Module Definition.</summary>
        /// <returns>A ModuleDefinitionInfo.</returns>
        [XmlIgnore]
        [JsonIgnore]
        public ModuleDefinitionInfo ModuleDefinition
        {
            get
            {
                return this.moduleDefinition ??
                       (this.moduleDefinition = this.ModuleDefID > Null.NullInteger
                            ? ModuleDefinitionController.GetModuleDefinitionByID(this.ModuleDefID)
                            : new ModuleDefinitionInfo());
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public Hashtable ModuleSettings
        {
            get
            {
                if (this.moduleSettings == null)
                {
                    if (this.ModuleID == Null.NullInteger)
                    {
                        this.moduleSettings = new Hashtable();
                    }
                    else
                    {
                        this.moduleSettings = new ModuleController().GetModuleSettings(this.ModuleID, this.TabID);
                    }
                }

                return this.moduleSettings;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public Hashtable TabModuleSettings
        {
            get
            {
                if (this.tabModuleSettings == null)
                {
                    if (this.TabModuleID == Null.NullInteger)
                    {
                        this.tabModuleSettings = new Hashtable();
                    }
                    else
                    {
                        this.tabModuleSettings = new ModuleController().GetTabModuleSettings(this.TabModuleID, this.TabID);
                    }
                }

                return this.tabModuleSettings;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public ModuleInfo DefaultLanguageModule
        {
            get
            {
                if (this.defaultLanguageModule == null && (!this.DefaultLanguageGuid.Equals(Null.NullGuid)) && this.ParentTab != null && this.ParentTab.DefaultLanguageTab != null &&
                    this.ParentTab.DefaultLanguageTab.ChildModules != null)
                {
                    this.defaultLanguageModule = (from kvp in this.ParentTab.DefaultLanguageTab.ChildModules where kvp.Value.UniqueId == this.DefaultLanguageGuid select kvp.Value).SingleOrDefault();
                }

                return this.defaultLanguageModule;
            }
        }

        public bool IsDefaultLanguage
        {
            get
            {
                return this.DefaultLanguageGuid == Null.NullGuid;
            }
        }

        public bool IsLocalized
        {
            get
            {
                bool isLocalized = true;
                if (this.DefaultLanguageModule != null)
                {
                    // Child language
                    isLocalized = this.ModuleID != this.DefaultLanguageModule.ModuleID;
                }

                return isLocalized;
            }
        }

        public bool IsNeutralCulture
        {
            get
            {
                return string.IsNullOrEmpty(this.CultureCode);
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public bool IsTranslated
        {
            get
            {
                bool isTranslated = true;
                if (this.DefaultLanguageModule != null)
                {
                    // Child language
                    isTranslated = this.LocalizedVersionGuid == this.DefaultLanguageModule.LocalizedVersionGuid;
                }

                return isTranslated;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public Dictionary<string, ModuleInfo> LocalizedModules
        {
            get
            {
                if (this.localizedModules == null && this.DefaultLanguageGuid.Equals(Null.NullGuid) && this.ParentTab != null && this.ParentTab.LocalizedTabs != null)
                {
                    // Cycle through all localized tabs looking for this module
                    this.localizedModules = new Dictionary<string, ModuleInfo>();
                    foreach (TabInfo t in this.ParentTab.LocalizedTabs.Values)
                    {
                        foreach (ModuleInfo m in t.ChildModules.Values)
                        {
                            ModuleInfo tempModuleInfo;
                            if (m.DefaultLanguageGuid == this.UniqueId && !m.IsDeleted && !this.localizedModules.TryGetValue(m.CultureCode, out tempModuleInfo))
                            {
                                this.localizedModules.Add(m.CultureCode, m);
                            }
                        }
                    }
                }

                return this.localizedModules;
            }
        }

        public TabInfo ParentTab
        {
            get
            {
                if (this.parentTab == null)
                {
                    if (this.PortalID == Null.NullInteger || string.IsNullOrEmpty(this.CultureCode))
                    {
                        this.parentTab = TabController.Instance.GetTab(this.TabID, this.PortalID, false);
                    }
                    else
                    {
                        Locale locale = LocaleController.Instance.GetLocale(this.CultureCode);
                        this.parentTab = TabController.Instance.GetTabByCulture(this.TabID, this.PortalID, locale);
                    }
                }

                return this.parentTab;
            }
        }

        /// <inheritdoc />
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        [XmlElement("alignment")]
        public string Alignment { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public bool AllModules { get; set; }

        [XmlElement("alltabs")]
        public bool AllTabs { get; set; }

        [XmlElement("border")]
        public string Border { get; set; }

        [XmlElement("cachemethod")]
        public string CacheMethod { get; set; }

        [XmlElement("cachetime")]
        public int CacheTime { get; set; }

        [XmlElement("color")]
        public string Color { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public string ContainerPath { get; set; }

        [XmlElement("containersrc")]
        public string ContainerSrc { get; set; }

        /// <summary>Gets or sets the ID of the Associated Desktop Module.</summary>
        /// <returns>An Integer.</returns>
        [XmlIgnore]
        [JsonIgnore]
        public int DesktopModuleID { get; set; }

        [XmlElement("displayprint")]
        public bool DisplayPrint { get; set; }

        [XmlElement("displaysyndicate")]
        public bool DisplaySyndicate { get; set; }

        [XmlElement("displaytitle")]
        public bool DisplayTitle { get; set; }

        [XmlElement("enddate")]
        public DateTime EndDate { get; set; }

        [XmlElement("footer")]
        public string Footer { get; set; }

        [XmlElement("header")]
        public string Header { get; set; }

        [XmlElement("iconfile")]
        public string IconFile { get; set; }

        [XmlElement("inheritviewpermissions")]
        public bool InheritViewPermissions { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public bool IsDefaultModule { get; set; }

        [XmlElement("isdeleted")]
        public bool IsDeleted { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public bool IsShareable { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public bool IsShareableViewOnly { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public DateTime LastContentModifiedOnDate { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public int ModuleControlId { get; set; }

        /// <summary>Gets or sets the ID of the Associated Module Definition.</summary>
        /// <returns>An Integer.</returns>
        [XmlIgnore]
        [JsonIgnore]
        public int ModuleDefID { get; set; }

        [XmlElement("moduleorder")]
        public int ModuleOrder { get; set; }

        /// <summary>
        /// Gets or sets get the ModulePermissions for the Module DO NOT USE THE SETTTER.
        /// <remarks>
        /// Since 5.0 the setter has been obsolete, directly setting the ModulePermissionCollection is likely an error, change the contenst of the collection instead.
        /// The setter still exists to preserve binary compatibility without the obsolete attribute since c# will not allow only a setter to be obsolete.
        /// </remarks>
        /// </summary>
        [XmlArray("modulepermissions")]
        [XmlArrayItem("permission")]
        public ModulePermissionCollection ModulePermissions
        {
            get
            {
                return this.modulePermissions ??
                    (this.modulePermissions = this.ModuleID > 0
                            ? new ModulePermissionCollection(ModulePermissionController.GetModulePermissions(this.ModuleID, this.TabID))
                            : new ModulePermissionCollection());
            }

            set
            {
                this.modulePermissions = value;
            }
        }

        [XmlElement("title")]
        public string ModuleTitle { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public int ModuleVersion { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public int OwnerPortalID { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public int PaneModuleCount { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public int PaneModuleIndex { get; set; }

        [XmlElement("panename")]
        public string PaneName { get; set; }

        [XmlElement("portalid")]
        public int PortalID { get; set; }

        [XmlElement("startdate")]
        public DateTime StartDate { get; set; }

        [XmlElement("tabmoduleid")]
        public int TabModuleID { get; set; }

        [XmlElement("uniqueId")]
        public Guid UniqueId { get; set; }

        [XmlElement("versionGuid")]
        public Guid VersionGuid { get; set; }

        [XmlElement("visibility")]
        public VisibilityState Visibility { get; set; }

        [XmlElement("cultureCode")]
        public string CultureCode
        {
            get
            {
                return this.cultureCode;
            }

            set
            {
                this.cultureCode = value;
            }
        }

        [XmlElement("defaultLanguageGuid")]
        public Guid DefaultLanguageGuid
        {
            get
            {
                return this.defaultLanguageGuid;
            }

            set
            {
                this.defaultLanguageGuid = value;
            }
        }

        [XmlElement("localizedVersionGuid")]
        public Guid LocalizedVersionGuid
        {
            get
            {
                return this.localizedVersionGuid;
            }

            set
            {
                this.localizedVersionGuid = value;
            }
        }

        /// <summary>Gets or sets the Key ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlIgnore]
        [JsonIgnore]
        public override int KeyID
        {
            get
            {
                return this.ModuleID;
            }

            set
            {
                this.ModuleID = value;
            }
        }

        /// <summary>Fills a ModuleInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public override void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            // Call the base classes fill method to populate base class properties
            this.FillInternal(dr);

            this.UniqueId = Null.SetNullGuid(dr["UniqueId"]);
			Logger.LogDebug("Fill(dr={Dr}): UniqueId → {UniqueId}", dr, UniqueId);
			Logger.LogDebug("Fill(dr={Dr}): DefaultLanguageGuid → {DefaultLanguageGuid}", dr, DefaultLanguageGuid);
			Logger.LogDebug("Fill(dr={Dr}): LocalizedVersionGuid → {LocalizedVersionGuid}", dr, LocalizedVersionGuid);
            this.VersionGuid = Null.SetNullGuid(dr["VersionGuid"]);
			Logger.LogDebug("Fill(dr={Dr}): VersionGuid → {VersionGuid}", dr, VersionGuid);
            this.DefaultLanguageGuid = Null.SetNullGuid(dr["DefaultLanguageGuid"]);
            this.LocalizedVersionGuid = Null.SetNullGuid(dr["LocalizedVersionGuid"]);
            this.CultureCode = Null.SetNullString(dr["CultureCode"]);
			Logger.LogDebug("Fill(dr={Dr}): CultureCode → {CultureCode}", dr, CultureCode);

            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
			Logger.LogDebug("Fill(dr={Dr}): PortalID → {PortalID}", dr, PortalID);
            if (dr.GetSchemaTable().Select("ColumnName = 'OwnerPortalID'").Length > 0)
            {
                this.OwnerPortalID = Null.SetNullInteger(dr["OwnerPortalID"]);
				Logger.LogDebug("Fill(dr={Dr}): OwnerPortalID → {OwnerPortalID}", dr, OwnerPortalID);
            }

            this.ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
			Logger.LogDebug("Fill(dr={Dr}): ModuleDefID → {ModuleDefID}", dr, ModuleDefID);
            this.ModuleTitle = Null.SetNullString(dr["ModuleTitle"]);
			Logger.LogDebug("Fill(dr={Dr}): ModuleTitle → {ModuleTitle}", dr, ModuleTitle);
            this.AllTabs = Null.SetNullBoolean(dr["AllTabs"]);
			Logger.LogDebug("Fill(dr={Dr}): AllTabs → {AllTabs}", dr, AllTabs);
            this.IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
			Logger.LogDebug("Fill(dr={Dr}): IsDeleted → {IsDeleted}", dr, IsDeleted);
            this.InheritViewPermissions = Null.SetNullBoolean(dr["InheritViewPermissions"]);
			Logger.LogDebug("Fill(dr={Dr}): InheritViewPermissions → {InheritViewPermissions}", dr, InheritViewPermissions);

            if (dr.GetSchemaTable().Select("ColumnName = 'IsShareable'").Length > 0)
            {
                this.IsShareable = Null.SetNullBoolean(dr["IsShareable"]);
				Logger.LogDebug("Fill(dr={Dr}): IsShareable → {IsShareable}", dr, IsShareable);
            }

            if (dr.GetSchemaTable().Select("ColumnName = 'IsShareableViewOnly'").Length > 0)
            {
                this.IsShareableViewOnly = Null.SetNullBoolean(dr["IsShareableViewOnly"]);
				Logger.LogDebug("Fill(dr={Dr}): IsShareableViewOnly → {IsShareableViewOnly}", dr, IsShareableViewOnly);
            }

            this.Header = Null.SetNullString(dr["Header"]);
			Logger.LogDebug("Fill(dr={Dr}): Header → {Header}", dr, Header);
			Logger.LogDebug("Fill(dr={Dr}): StartDate → {StartDate}", dr, StartDate);
			Logger.LogDebug("Fill(dr={Dr}): EndDate → {EndDate}", dr, EndDate);
            this.Footer = Null.SetNullString(dr["Footer"]);
			Logger.LogDebug("Fill(dr={Dr}): Footer → {Footer}", dr, Footer);
            this.StartDate = Null.SetNullDateTime(dr["StartDate"]);
            this.EndDate = Null.SetNullDateTime(dr["EndDate"]);
            this.LastContentModifiedOnDate = Null.SetNullDateTime(dr["LastContentModifiedOnDate"]);
            try
            {
                this.TabModuleID = Null.SetNullInteger(dr["TabModuleID"]);
				Logger.LogDebug("Fill(dr={Dr}): TabModuleID → {TabModuleID}", dr, TabModuleID);
				Logger.LogDebug("Fill(dr={Dr}): Alignment → {Alignment}", dr, Alignment);
				Logger.LogDebug("Fill(dr={Dr}): Color → {Color}", dr, Color);
				Logger.LogDebug("Fill(dr={Dr}): Border → {Border}", dr, Border);
				Logger.LogDebug("Fill(dr={Dr}): IconFile → {IconFile}", dr, IconFile);
                this.ModuleOrder = Null.SetNullInteger(dr["ModuleOrder"]);
				Logger.LogDebug("Fill(dr={Dr}): ModuleOrder → {ModuleOrder}", dr, ModuleOrder);
                this.PaneName = Null.SetNullString(dr["PaneName"]);
				Logger.LogDebug("Fill(dr={Dr}): PaneName → {PaneName}", dr, PaneName);
                this.CacheTime = Null.SetNullInteger(dr["CacheTime"]);
				Logger.LogDebug("Fill(dr={Dr}): CacheTime → {CacheTime}", dr, CacheTime);
                this.CacheMethod = Null.SetNullString(dr["CacheMethod"]);
				Logger.LogDebug("Fill(dr={Dr}): CacheMethod → {CacheMethod}", dr, CacheMethod);
                this.Alignment = Null.SetNullString(dr["Alignment"]);
                this.Color = Null.SetNullString(dr["Color"]);
                this.Border = Null.SetNullString(dr["Border"]);
                this.IconFile = Null.SetNullString(dr["IconFile"]);
                int visible = Null.SetNullInteger(dr["Visibility"]);
				Logger.LogDebug("Fill(dr={Dr}): visible → {Visible}", dr, visible);
                if (visible == Null.NullInteger)
                {
                    this.Visibility = VisibilityState.Maximized;
					Logger.LogDebug("Fill(dr={Dr}): Visibility → {Visibility}", dr, Visibility);
					Logger.LogDebug("Fill(dr={Dr}): this.Visibility → {Arg1}", dr, this.Visibility);
                }
                else
                {
                    switch (visible)
                    {
                        case 0:
                            this.Visibility = VisibilityState.Maximized;
                            break;
                        case 1:
                            this.Visibility = VisibilityState.Minimized;
                            break;
                        case 2:
                            this.Visibility = VisibilityState.None;
                            break;
                    }
                }

                this.ContainerSrc = Null.SetNullString(dr["ContainerSrc"]);
				Logger.LogDebug("Fill(dr={Dr}): ContainerSrc → {ContainerSrc}", dr, ContainerSrc);
				Logger.LogDebug("Fill(dr={Dr}): DisplayPrint → {DisplayPrint}", dr, DisplayPrint);
				Logger.LogDebug("Fill(dr={Dr}): DisplaySyndicate → {DisplaySyndicate}", dr, DisplaySyndicate);
				Logger.LogDebug("Fill(dr={Dr}): DesktopModuleID → {DesktopModuleID}", dr, DesktopModuleID);
                this.DisplayTitle = Null.SetNullBoolean(dr["DisplayTitle"]);
				Logger.LogDebug("Fill(dr={Dr}): DisplayTitle → {DisplayTitle}", dr, DisplayTitle);
                this.DisplayPrint = Null.SetNullBoolean(dr["DisplayPrint"]);
                this.DisplaySyndicate = Null.SetNullBoolean(dr["DisplaySyndicate"]);
                this.DesktopModuleID = Null.SetNullInteger(dr["DesktopModuleID"]);
                this.ModuleControlId = Null.SetNullInteger(dr["ModuleControlID"]);
				Logger.LogDebug("Fill(dr={Dr}): ModuleControlId → {ModuleControlId}", dr, ModuleControlId);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
            string outputFormat = string.Empty;
            if (format == string.Empty)
			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): outputFormat → {OutputFormat}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, outputFormat);
            {
                outputFormat = "g";
            }

            if (currentScope == Scope.NoSettings)
            {
                propertyNotFound = true;
				Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): propertyNotFound → {PropertyNotFound1}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, propertyNotFound);
                return PropertyAccess.ContentLocked;
            }

            propertyNotFound = true;
            string result = string.Empty;
			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): result → {Result}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, result);
            bool isPublic = true;
			Logger.LogDebug("GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound}): isPublic → {IsPublic}", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound, isPublic);
            switch (propertyName.ToLowerInvariant())
            {
                case "portalid":
                    propertyNotFound = false;
                    result = this.PortalID.ToString(outputFormat, formatProvider);
                    break;
                case "displayportalid":
                    propertyNotFound = false;
                    result = this.OwnerPortalID.ToString(outputFormat, formatProvider);
                    break;
                case "tabid":
                    propertyNotFound = false;
                    result = this.TabID.ToString(outputFormat, formatProvider);
                    break;
                case "tabmoduleid":
                    propertyNotFound = false;
                    result = this.TabModuleID.ToString(outputFormat, formatProvider);
                    break;
                case "moduleid":
                    propertyNotFound = false;
                    result = this.ModuleID.ToString(outputFormat, formatProvider);
                    break;
                case "moduledefid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ModuleDefID.ToString(outputFormat, formatProvider);
                    break;
                case "moduleorder":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ModuleOrder.ToString(outputFormat, formatProvider);
                    break;
                case "panename":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.PaneName, format);
                    break;
                case "moduletitle":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ModuleTitle, format);
                    break;
                case "cachetime":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.CacheTime.ToString(outputFormat, formatProvider);
                    break;
                case "cachemethod":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.CacheMethod, format);
                    break;
                case "alignment":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Alignment, format);
                    break;
                case "color":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Color, format);
                    break;
                case "border":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Border, format);
                    break;
                case "iconfile":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.IconFile, format);
                    break;
                case "alltabs":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.AllTabs, formatProvider);
                    break;
                case "isdeleted":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsDeleted, formatProvider);
                    break;
                case "header":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Header, format);
                    break;
                case "footer":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.Footer, format);
                    break;
                case "startdate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.StartDate.ToString(outputFormat, formatProvider);
                    break;
                case "enddate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.EndDate.ToString(outputFormat, formatProvider);
                    break;
                case "containersrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ContainerSrc, format);
                    break;
                case "displaytitle":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DisplayTitle, formatProvider);
                    break;
                case "displayprint":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DisplayPrint, formatProvider);
                    break;
                case "displaysyndicate":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DisplaySyndicate, formatProvider);
                    break;
                case "inheritviewpermissions":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.InheritViewPermissions, formatProvider);
                    break;
                case "isshareable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsShareable, formatProvider);
                    break;
                case "isshareableviewonly":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsShareableViewOnly, formatProvider);
                    break;
                case "desktopmoduleid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.DesktopModuleID.ToString(outputFormat, formatProvider);
                    break;
                case "friendlyname":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.FriendlyName, format);
                    break;
                case "foldername":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.FolderName, format);
                    break;
                case "description":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.Description, format);
                    break;
                case "version":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.Version, format);
                    break;
                case "ispremium":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsPremium, formatProvider);
                    break;
                case "isadmin":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsAdmin, formatProvider);
                    break;
                case "businesscontrollerclass":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.BusinessControllerClass, format);
                    break;
                case "modulename":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.ModuleName, format);
                    break;
                case "supportedfeatures":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.DesktopModule.SupportedFeatures.ToString(outputFormat, formatProvider);
                    break;
                case "compatibleversions":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.CompatibleVersions, format);
                    break;
                case "dependencies":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.Dependencies, format);
                    break;
                case "permissions":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.Permissions, format);
                    break;
                case "defaultcachetime":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ModuleDefinition.DefaultCacheTime.ToString(outputFormat, formatProvider);
                    break;
                case "modulecontrolid":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.ModuleControlId.ToString(outputFormat, formatProvider);
                    break;
                case "controlsrc":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ModuleControl.ControlSrc, format);
                    break;
                case "controltitle":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ModuleControl.ControlTitle, format);
                    break;
                case "helpurl":
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ModuleControl.HelpURL, format);
                    break;
                case "supportspartialrendering":
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.ModuleControl.SupportsPartialRendering, formatProvider);
                    break;
                case "containerpath":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.ContainerPath, format);
                    break;
                case "panemoduleindex":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.PaneModuleIndex.ToString(outputFormat, formatProvider);
                    break;
                case "panemodulecount":
                    isPublic = false;
                    propertyNotFound = false;
                    result = this.PaneModuleCount.ToString(outputFormat, formatProvider);
                    break;
                case "isdefaultmodule":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.IsDefaultModule, formatProvider);
                    break;
                case "allmodules":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.AllModules, formatProvider);
                    break;
                case "isportable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsPortable, formatProvider);
                    break;
                case "issearchable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsSearchable, formatProvider);
                    break;
                case "isupgradeable":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.Boolean2LocalizedYesNo(this.DesktopModule.IsUpgradeable, formatProvider);
                    break;
                case "adminpage":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.AdminPage, format);
                    break;
                case "hostpage":
                    isPublic = false;
                    propertyNotFound = false;
                    result = PropertyAccess.FormatString(this.DesktopModule.HostPage, format);
                    break;
            }

            if (!isPublic && currentScope != Scope.Debug)
            {
                propertyNotFound = true;
                result = PropertyAccess.ContentLocked;
            }

			Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
            return result;
        }

        public ModuleInfo Clone()
        {
			Logger.LogDebug(">>> Entering Clone()");
            var objModuleInfo = new ModuleInfo
            {
                PortalID = this.PortalID,
                OwnerPortalID = this.OwnerPortalID,
                TabID = this.TabID,
                TabModuleID = this.TabModuleID,
                ModuleID = this.ModuleID,
                ModuleOrder = this.ModuleOrder,
                PaneName = this.PaneName,
                ModuleTitle = this.ModuleTitle,
                CacheTime = this.CacheTime,
                CacheMethod = this.CacheMethod,
                Alignment = this.Alignment,
                Color = this.Color,
                Border = this.Border,
                IconFile = this.IconFile,
                AllTabs = this.AllTabs,
                Visibility = this.Visibility,
                IsDeleted = this.IsDeleted,
                Header = this.Header,
                Footer = this.Footer,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                ContainerSrc = this.ContainerSrc,
                DisplayTitle = this.DisplayTitle,
                DisplayPrint = this.DisplayPrint,
                DisplaySyndicate = this.DisplaySyndicate,
                InheritViewPermissions = this.InheritViewPermissions,
                IsShareable = this.IsShareable,
                IsShareableViewOnly = this.IsShareableViewOnly,
                DesktopModuleID = this.DesktopModuleID,
                ModuleDefID = this.ModuleDefID,
                ModuleControlId = this.ModuleControlId,
                ContainerPath = this.ContainerPath,
                PaneModuleIndex = this.PaneModuleIndex,
                PaneModuleCount = this.PaneModuleCount,
                IsDefaultModule = this.IsDefaultModule,
                AllModules = this.AllModules,
                UniqueId = Guid.NewGuid(),
                VersionGuid = Guid.NewGuid(),
                DefaultLanguageGuid = this.DefaultLanguageGuid,
                LocalizedVersionGuid = this.LocalizedVersionGuid,
                CultureCode = this.CultureCode,
            };

            // localized properties
            this.Clone(objModuleInfo, this);
			Logger.LogDebug("<<< Exiting Clone()");
            return objModuleInfo;
        }

        public string GetEffectiveCacheMethod()
        {
			Logger.LogDebug(">>> Entering GetEffectiveCacheMethod()");
            string effectiveCacheMethod;
            if (!string.IsNullOrEmpty(this.CacheMethod))
            {
                effectiveCacheMethod = this.CacheMethod;
            }
            else if (!string.IsNullOrEmpty(Host.Host.ModuleCachingMethod))
            {
                effectiveCacheMethod = Host.Host.ModuleCachingMethod;
            }
            else
            {
                var defaultModuleCache = ComponentFactory.GetComponent<ModuleCachingProvider>();
				Logger.LogDebug("GetEffectiveCacheMethod(): defaultModuleCache → {DefaultModuleCache}", defaultModuleCache);
                effectiveCacheMethod = (from provider in ModuleCachingProvider.GetProviderList() where provider.Value.Equals(defaultModuleCache) select provider.Key).SingleOrDefault();
            }

            if (string.IsNullOrEmpty(effectiveCacheMethod))
            {
				Logger.LogWarning("Throwing InvalidOperationException from GetEffectiveCacheMethod()");
			Logger.LogDebug("<<< Exiting GetEffectiveCacheMethod()");
                throw new InvalidOperationException(Localization.GetString("EXCEPTION_ModuleCacheMissing"));
            }

            return effectiveCacheMethod;
        }

        public void Initialize(int portalId)
        {
			Logger.LogDebug(">>> Entering Initialize(portalId={PortalId})", portalId);
            this.PortalID = portalId;
            this.OwnerPortalID = portalId;
			Logger.LogDebug("Initialize(portalId={PortalId}): this.PortalID → {Arg1}", portalId, this.PortalID);
			Logger.LogDebug("Initialize(portalId={PortalId}): OwnerPortalID → {OwnerPortalID}", portalId, OwnerPortalID);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.OwnerPortalID → {Arg1}", portalId, this.OwnerPortalID);
			Logger.LogDebug("Initialize(portalId={PortalId}): ModuleOrder → {ModuleOrder}", portalId, ModuleOrder);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.ModuleOrder → {Arg1}", portalId, this.ModuleOrder);
			Logger.LogDebug("Initialize(portalId={PortalId}): PaneName → {PaneName}", portalId, PaneName);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.PaneName → {Arg1}", portalId, this.PaneName);
			Logger.LogDebug("Initialize(portalId={PortalId}): ModuleTitle → {ModuleTitle}", portalId, ModuleTitle);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.ModuleTitle → {Arg1}", portalId, this.ModuleTitle);
			Logger.LogDebug("Initialize(portalId={PortalId}): CacheTime → {CacheTime}", portalId, CacheTime);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.CacheTime → {Arg1}", portalId, this.CacheTime);
			Logger.LogDebug("Initialize(portalId={PortalId}): CacheMethod → {CacheMethod}", portalId, CacheMethod);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.CacheMethod → {Arg1}", portalId, this.CacheMethod);
			Logger.LogDebug("Initialize(portalId={PortalId}): Alignment → {Alignment}", portalId, Alignment);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.Alignment → {Arg1}", portalId, this.Alignment);
			Logger.LogDebug("Initialize(portalId={PortalId}): Color → {Color}", portalId, Color);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.Color → {Arg1}", portalId, this.Color);
			Logger.LogDebug("Initialize(portalId={PortalId}): Border → {Border}", portalId, Border);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.Border → {Arg1}", portalId, this.Border);
			Logger.LogDebug("Initialize(portalId={PortalId}): IconFile → {IconFile}", portalId, IconFile);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.IconFile → {Arg1}", portalId, this.IconFile);
			Logger.LogDebug("Initialize(portalId={PortalId}): AllTabs → {AllTabs}", portalId, AllTabs);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.AllTabs → {Arg1}", portalId, this.AllTabs);
			Logger.LogDebug("Initialize(portalId={PortalId}): Visibility → {Visibility}", portalId, Visibility);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.Visibility → {Arg1}", portalId, this.Visibility);
			Logger.LogDebug("Initialize(portalId={PortalId}): IsDeleted → {IsDeleted}", portalId, IsDeleted);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.IsDeleted → {Arg1}", portalId, this.IsDeleted);
			Logger.LogDebug("Initialize(portalId={PortalId}): Header → {Header}", portalId, Header);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.Header → {Arg1}", portalId, this.Header);
			Logger.LogDebug("Initialize(portalId={PortalId}): Footer → {Footer}", portalId, Footer);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.Footer → {Arg1}", portalId, this.Footer);
			Logger.LogDebug("Initialize(portalId={PortalId}): StartDate → {StartDate}", portalId, StartDate);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.StartDate → {Arg1}", portalId, this.StartDate);
			Logger.LogDebug("Initialize(portalId={PortalId}): EndDate → {EndDate}", portalId, EndDate);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.EndDate → {Arg1}", portalId, this.EndDate);
			Logger.LogDebug("Initialize(portalId={PortalId}): DisplayTitle → {DisplayTitle}", portalId, DisplayTitle);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.DisplayTitle → {Arg1}", portalId, this.DisplayTitle);
			Logger.LogDebug("Initialize(portalId={PortalId}): DisplayPrint → {DisplayPrint}", portalId, DisplayPrint);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.DisplayPrint → {Arg1}", portalId, this.DisplayPrint);
			Logger.LogDebug("Initialize(portalId={PortalId}): DisplaySyndicate → {DisplaySyndicate}", portalId, DisplaySyndicate);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.DisplaySyndicate → {Arg1}", portalId, this.DisplaySyndicate);
			Logger.LogDebug("Initialize(portalId={PortalId}): InheritViewPermissions → {InheritViewPermissions}", portalId, InheritViewPermissions);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.InheritViewPermissions → {Arg1}", portalId, this.InheritViewPermissions);
			Logger.LogDebug("Initialize(portalId={PortalId}): IsShareable → {IsShareable}", portalId, IsShareable);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.IsShareable → {Arg1}", portalId, this.IsShareable);
			Logger.LogDebug("Initialize(portalId={PortalId}): IsShareableViewOnly → {IsShareableViewOnly}", portalId, IsShareableViewOnly);
            this.ModuleDefID = Null.NullInteger;
            this.ModuleOrder = Null.NullInteger;
            this.PaneName = Null.NullString;
			Logger.LogDebug("Initialize(portalId={PortalId}): ModuleDefID → {ModuleDefID}", portalId, ModuleDefID);
			Logger.LogDebug("Initialize(portalId={PortalId}): this.ModuleDefID → {Arg1}", portalId, this.ModuleDefID);
            this.ModuleTitle = Null.NullString;
            this.CacheTime = 0;
            this.CacheMethod = Null.NullString;
            this.Alignment = Null.NullString;
            this.Color = Null.NullString;
            this.Border = Null.NullString;
            this.IconFile = Null.NullString;
            this.AllTabs = Null.NullBoolean;
            this.Visibility = VisibilityState.Maximized;
            this.IsDeleted = Null.NullBoolean;
            this.Header = Null.NullString;
            this.Footer = Null.NullString;
            this.StartDate = Null.NullDate;
            this.EndDate = Null.NullDate;
            this.DisplayTitle = true;
            this.DisplayPrint = false;
            this.DisplaySyndicate = Null.NullBoolean;
            this.InheritViewPermissions = Null.NullBoolean;
            this.IsShareable = true;
            this.IsShareableViewOnly = true;
            this.ContainerSrc = Null.NullString;
            this.DesktopModuleID = Null.NullInteger;
            this.ModuleControlId = Null.NullInteger;
            this.ContainerPath = Null.NullString;
            this.PaneModuleIndex = 0;
            this.PaneModuleCount = 0;
            this.IsDefaultModule = Null.NullBoolean;
            this.AllModules = Null.NullBoolean;
            if (PortalSettings.Current.DefaultModuleId > Null.NullInteger && PortalSettings.Current.DefaultTabId > Null.NullInteger)
            {
                ModuleInfo objModule = ModuleController.Instance.GetModule(PortalSettings.Current.DefaultModuleId, PortalSettings.Current.DefaultTabId, true);
				Logger.LogDebug("Initialize(portalId={PortalId}): objModule → {ObjModule}", portalId, objModule);
                if (objModule != null)
                {
                    this.Alignment = objModule.Alignment;
                    this.Color = objModule.Color;
                    this.Border = objModule.Border;
                    this.IconFile = objModule.IconFile;
                    this.Visibility = objModule.Visibility;
                    this.ContainerSrc = objModule.ContainerSrc;
                    this.DisplayTitle = objModule.DisplayTitle;
                    this.DisplayPrint = objModule.DisplayPrint;
                    this.DisplaySyndicate = objModule.DisplaySyndicate;
                }
            }
			Logger.LogDebug("<<< Exiting Initialize(portalId={PortalId})", portalId);
        }
    }
}
