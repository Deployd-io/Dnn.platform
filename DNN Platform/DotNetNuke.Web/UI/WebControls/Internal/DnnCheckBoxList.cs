// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    public class DnnCheckBoxList : CheckBoxList
    {
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IClientResourceController clientResourceController;
        private string initValue;

        /// <summary>Initializes a new instance of the <see cref="DnnCheckBoxList"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnCheckBoxList()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnCheckBoxList"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnCheckBoxList(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController)
        {
			eventLogger.LogError(">>> Entering DnnCheckBoxList(appStatus={AppStatus},eventLogger={EventLogger},clientResourceController={ClientResourceController})", appStatus, eventLogger, clientResourceController);
			eventLogger.LogDebug("DnnCheckBoxList(appStatus={AppStatus},eventLogger={EventLogger},clientResourceController={ClientResourceController}): eventLogger → {EventLogger1}", appStatus, eventLogger, clientResourceController, eventLogger);
			eventLogger.LogDebug("DnnCheckBoxList(appStatus={AppStatus},eventLogger={EventLogger},clientResourceController={ClientResourceController}): clientResourceController → {ClientResourceController1}", appStatus, eventLogger, clientResourceController, clientResourceController);
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			eventLogger.LogWarning("<<< Exiting DnnCheckBoxList(appStatus={AppStatus},eventLogger={EventLogger},clientResourceController={ClientResourceController})", appStatus, eventLogger, clientResourceController);
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
        }

        /// <inheritdoc />
        public override string SelectedValue
        {
            get
            {
                return base.SelectedValue;
            }

            set
            {
                if (this.RequiresDataBinding)
                {
                    this.initValue = value;
                }
                else
                {
                    base.SelectedValue = value;
                }
            }
        }

        /// <inheritdoc />
        public override void DataBind()
        {
			eventLogger.LogError(">>> Entering DataBind()");
            if (!string.IsNullOrEmpty(this.initValue))
            {
                this.DataBind(this.initValue);
            }
            else
            {
                base.DataBind();
            }
			eventLogger.LogWarning("<<< Exiting DataBind()");
        }

        /// <summary>Adds an item to the list.</summary>
        /// <param name="text">The item text.</param>
        /// <param name="value">The item value.</param>
        public void AddItem(string text, string value)
        {
			eventLogger.LogError(">>> Entering AddItem(text={Text},value={Value})", text, value);
            this.Items.Add(new ListItem(text, value));
			eventLogger.LogWarning("<<< Exiting AddItem(text={Text},value={Value})", text, value);
        }

        /// <summary>Inserts an item into the list.</summary>
        /// <param name="index">The location in the collection to insert the item.</param>
        /// <param name="text">The item text.</param>
        /// <param name="value">The item value.</param>
        public void InsertItem(int index, string text, string value)
        {
			eventLogger.LogError(">>> Entering InsertItem(index={Index},text={Text},value={Value})", index, text, value);
            this.Items.Insert(index, new ListItem(text, value));
			eventLogger.LogWarning("<<< Exiting InsertItem(index={Index},text={Text},value={Value})", index, text, value);
        }

        /// <summary>Binds a data source to the invoked server control and all its child controls.</summary>
        /// <param name="initialValue">The initial value.</param>
        public void DataBind(string initialValue)
        {
			eventLogger.LogError(">>> Entering DataBind(initialValue={InitialValue})", initialValue);
            this.DataBind(initialValue, false);
			eventLogger.LogWarning("<<< Exiting DataBind(initialValue={InitialValue})", initialValue);
        }

        /// <summary>Binds a data source to the invoked server control and all its child controls.</summary>
        /// <param name="initial">The initial value or text.</param>
        /// <param name="findByText">Whether <paramref name="initial"/> is the text or value.</param>
        public void DataBind(string initial, bool findByText)
        {
			eventLogger.LogError(">>> Entering DataBind(initial={Initial},findByText={FindByText})", initial, findByText);
            base.DataBind();

            this.Select(initial, findByText);
			eventLogger.LogWarning("<<< Exiting DataBind(initial={Initial},findByText={FindByText})", initial, findByText);
        }

        /// <summary>Selects an item.</summary>
        /// <param name="initial">The item's value or text.</param>
        /// <param name="findByText">Whether <paramref name="initial"/> is the text or value.</param>
        public void Select(string initial, bool findByText)
        {
			eventLogger.LogError(">>> Entering Select(initial={Initial},findByText={FindByText})", initial, findByText);
            if (findByText)
            {
                if (this.FindItemByText(initial, true) != null)
                {
                    this.FindItemByText(initial, true).Selected = true;
                }
            }
            else
            {
                if (this.FindItemByValue(initial, true) != null)
                {
                    this.FindItemByValue(initial, true).Selected = true;
                }
            }
			eventLogger.LogWarning("<<< Exiting Select(initial={Initial},findByText={FindByText})", initial, findByText);
        }

        /// <summary>Finds an item by its text.</summary>
        /// <param name="text">The item's text.</param>
        /// <param name="ignoreCase">Whether to do a case-insensitive search.</param>
        /// <returns>The list item or <see langword="null"/>.</returns>
        public ListItem FindItemByText(string text, bool ignoreCase = false)
        {
			eventLogger.LogError(">>> Entering FindItemByText(text={Text},ignoreCase={IgnoreCase})", text, ignoreCase);
            eventLogger.LogWarning("<<< Exiting FindItemByText(text={Text},ignoreCase={IgnoreCase})", text, ignoreCase);
            return ignoreCase ? this.Items.FindByText(text) : this.Items.FindByTextWithIgnoreCase(text);
        }

        /// <summary>Finds an item by its value.</summary>
        /// <param name="value">The item's value.</param>
        /// <param name="ignoreCase">Whether to do a case-insensitive search.</param>
        /// <returns>The list item or <see langword="null"/>.</returns>
        public ListItem FindItemByValue(string value, bool ignoreCase = false)
        {
			eventLogger.LogError(">>> Entering FindItemByValue(value={Value},ignoreCase={IgnoreCase})", value, ignoreCase);
            eventLogger.LogWarning("<<< Exiting FindItemByValue(value={Value},ignoreCase={IgnoreCase})", value, ignoreCase);
            return ignoreCase ? this.Items.FindByValue(value) : this.Items.FindByValueWithIgnoreCase(value);
        }

        /// <summary>Finds an item's index by its value.</summary>
        /// <param name="value">The item's value.</param>
        /// <returns>The index or <c>-1</c>.</returns>
        public int FindItemIndexByValue(string value)
        {
			eventLogger.LogError(">>> Entering FindItemIndexByValue(value={Value})", value);
            eventLogger.LogWarning("<<< Exiting FindItemIndexByValue(value={Value})", value);
            return this.Items.IndexOf(this.FindItemByValue(value));
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnInit(e={E})", e);
            this.RepeatColumns = 1;
			eventLogger.LogDebug("OnInit(e={E}): this.RepeatColumns → {Arg1}", e, this.RepeatColumns);
            base.OnInit(e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnPreRender(e={E})", e);
            Utilities.ApplyControlSkin(this.clientResourceController, this, string.Empty, string.Empty);
            this.RegisterRequestResources();

            base.OnPreRender(e);
			eventLogger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        private void RegisterRequestResources()
        {
			eventLogger.LogError(">>> Entering RegisterRequestResources()");
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.DnnPlugins);

            if (this.appStatus.Status == UpgradeStatus.None)
            {
                var package = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == "Selectize");
				eventLogger.LogError("RegisterRequestResources(): package → {Package}", package);
                if (package != null)
                {
                    JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, "Selectize");

                    var libraryPath = $"~/Resources/Libraries/{package.LibraryName}/{Globals.FormatVersion(package.Version, "00", 3, "_")}/";
					eventLogger.LogError("RegisterRequestResources(): libraryPath → {LibraryPath}", libraryPath);
                    this.clientResourceController.RegisterStylesheet($"{libraryPath}selectize.css");
                    this.clientResourceController.RegisterStylesheet($"{libraryPath}selectize.default.css");

                    var initScripts = $"$('#{this.ClientID}').selectize({{}});";
					eventLogger.LogError("RegisterRequestResources(): initScripts → {InitScripts}", initScripts);

                    this.Page.ClientScript.RegisterStartupScript(this.Page.GetType(), $"{this.ClientID}Sctipts", initScripts, true);
                }
            }
			eventLogger.LogWarning("<<< Exiting RegisterRequestResources()");
        }
    }
}
