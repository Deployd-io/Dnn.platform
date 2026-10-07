// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;

    /// <summary>The SettingsEditorControl control provides an Editor to edit DotNetNuke Settings.</summary>
    [ToolboxData("<{0}:SettingsEditorControl runat=server></{0}:SettingsEditorControl>")]
    public class SettingsEditorControl : PropertyEditorControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SettingsEditorControl));
        /// <summary>Initializes a new instance of the <see cref="SettingsEditorControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public SettingsEditorControl()
            : this(Globals.GetCurrentServiceProvider())
        {
		Logger.LogError(">>> Entering SettingsEditorControl()");
        Logger.LogWarning("<<< Exiting SettingsEditorControl()");
        }

        /// <summary>Initializes a new instance of the <see cref="SettingsEditorControl"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        public SettingsEditorControl(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>Gets or sets the CustomEditors that are used by this control.</summary>
        /// <value>The CustomEditors object.</value>
        [Browsable(false)]
        public Hashtable CustomEditors { get; set; }

        /// <summary>Gets or sets the Visibility values that are used by this control.</summary>
        /// <value>The CustomEditors object.</value>
        public Hashtable Visibility { get; set; }

        /// <summary>Gets the Underlying DataSource.</summary>
        /// <value>An IEnumerable.</value>
        protected override IEnumerable UnderlyingDataSource
        {
            get
            {
                return this.GetSettings();
            }
        }

        /// <inheritdoc />
        protected override void AddEditorRow(Table table, object obj)
        {
			Logger.LogError(">>> Entering AddEditorRow(table={Table},obj={Obj})", table, obj);
            var info = (SettingInfo)obj;
			Logger.LogWarning("<<< Exiting AddEditorRow(table={Table},obj={Obj})", table, obj);
			Logger.LogError("AddEditorRow(table={Table},obj={Obj}): info → {Info}", table, obj, info);
            this.AddEditorRow(table, info.Name, new SettingsEditorInfoAdapter(this.DataSource, obj, this.ID));
        }

        /// <inheritdoc />
        protected override void AddEditorRow(Panel container, object obj)
        {
			Logger.LogError(">>> Entering AddEditorRow(container={Container},obj={Obj})", container, obj);
            var info = (SettingInfo)obj;
			Logger.LogWarning("<<< Exiting AddEditorRow(container={Container},obj={Obj})", container, obj);
			Logger.LogError("AddEditorRow(container={Container},obj={Obj}): info → {Info}", container, obj, info);
            this.AddEditorRow(container, info.Name, new SettingsEditorInfoAdapter(this.DataSource, obj, this.ID));
			Logger.LogError(">>> Entering AddEditorRow(obj={Obj})", obj);
			Logger.LogWarning("<<< Exiting AddEditorRow(obj={Obj})", obj);
			Logger.LogError("AddEditorRow(obj={Obj}): info → {Info}", obj, info);
        }

        /// <inheritdoc />
        protected override void AddEditorRow(object obj)
        {
            var info = (SettingInfo)obj;
            this.AddEditorRow(this, info.Name, new SettingsEditorInfoAdapter(this.DataSource, obj, this.ID));
        }

        /// <summary>GetRowVisibility determines the Visibility of a row in the table.</summary>
        /// <param name="obj">The property.</param>
        /// <returns><see langword="true"/> if the row is visible, otherwise <see langword="false"/>.</returns>
        protected override bool GetRowVisibility(object obj)
        {
			Logger.LogError(">>> Entering GetRowVisibility(obj={Obj})", obj);
            var info = (SettingInfo)obj;
            var isVisible = true;
			Logger.LogError("GetRowVisibility(obj={Obj}): info → {Info}", obj, info);
            if (this.Visibility?[info.Name] != null)
            {
                isVisible = Convert.ToBoolean(this.Visibility[info.Name], CultureInfo.InvariantCulture);
				Logger.LogError("GetRowVisibility(obj={Obj}): isVisible → {IsVisible}", obj, isVisible);
            }

			Logger.LogWarning("<<< Exiting GetRowVisibility(obj={Obj})", obj);
            return isVisible;
        }

        /// <summary>GetSettings converts the DataSource into an ArrayList (IEnumerable).</summary>
        private ArrayList GetSettings()
        {
			Logger.LogError(">>> Entering GetSettings()");
            var settings = (Hashtable)this.DataSource;
            var arrSettings = new ArrayList();
			Logger.LogError("GetSettings(): arrSettings → {ArrSettings}", arrSettings);
			Logger.LogError("GetSettings(): settings → {Settings}", settings);
            var settingsEnumerator = settings.GetEnumerator();
            while (settingsEnumerator.MoveNext())
            {
				Logger.LogError("GetSettings(): settingsEnumerator → {SettingsEnumerator}", settingsEnumerator);
                var info = new SettingInfo(settingsEnumerator.Key, settingsEnumerator.Value);
				Logger.LogError("GetSettings(): info → {Info}", info);
                if (this.CustomEditors?[settingsEnumerator.Key] != null)
                {
                    info.Editor = Convert.ToString(this.CustomEditors[settingsEnumerator.Key], CultureInfo.InvariantCulture);
                }

                arrSettings.Add(info);
            }

            arrSettings.Sort(new SettingNameComparer());
			Logger.LogWarning("<<< Exiting GetSettings()");
            return arrSettings;
        }
    }
}
