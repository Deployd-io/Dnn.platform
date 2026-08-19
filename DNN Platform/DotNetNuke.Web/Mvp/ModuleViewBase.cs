// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvp
{
    using System;
    using System.Globalization;

    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins.Controls;
    using WebFormsMvp.Web;

    /// <summary>Represents a class that is a view for a module in a Web Forms Model-View-Presenter application.</summary>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public abstract partial class ModuleViewBase : ModuleUserControlBase, IModuleViewBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleViewBase));
        /// <summary>Initializes a new instance of the <see cref="ModuleViewBase"/> class.</summary>
        protected ModuleViewBase()
        {
			Logger.LogDebug(">>> Entering ModuleViewBase()");
            this.AutoDataBind = true;
			Logger.LogDebug("ModuleViewBase(): this.AutoDataBind → {Arg0}", this.AutoDataBind);
			Logger.LogDebug("<<< Exiting ModuleViewBase()");
        }

        /// <inheritdoc />
        public event EventHandler Initialize;

        /// <inheritdoc />
        public new event EventHandler Load;

        /// <inheritdoc />
        public bool ThrowExceptionIfNoPresenterBound
        {
            get { return true; }
        }

        /// <inheritdoc />
        public bool AutoDataBind { get; set; }

        /// <inheritdoc />
        public void ProcessModuleLoadException(Exception ex)
        {
			Logger.LogDebug(">>> Entering ProcessModuleLoadException(ex={Ex})", ex);
            Exceptions.ProcessModuleLoadException(this, ex);
			Logger.LogDebug("<<< Exiting ProcessModuleLoadException(ex={Ex})", ex);
        }

        /// <inheritdoc />
        public void ShowMessage(string messageHeader, string message, ModuleMessage.ModuleMessageType messageType)
        {
			Logger.LogDebug(">>> Entering ShowMessage(messageHeader={MessageHeader},message={Message},messageType={MessageType})", messageHeader, message, messageType);
            DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, messageHeader, message, messageType);
			Logger.LogDebug("<<< Exiting ShowMessage(messageHeader={MessageHeader},message={Message},messageType={MessageType})", messageHeader, message, messageType);
        }

        /// <summary>Gets the data item.</summary>
        /// <typeparam name="T">The type of item.</typeparam>
        /// <returns>The item or a new <typeparamref name="T"/>.</returns>
        protected T DataItem<T>()
            where T : class, new()
        {
			Logger.LogDebug(">>> Entering DataItem()");
            var t = this.Page.GetDataItem() as T ?? new T();
			Logger.LogDebug("<<< Exiting DataItem()");
			Logger.LogDebug("DataItem(): t → {T}", t);
            return t;
        }

        /// <summary>Gets the data value.</summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The value.</returns>
        protected T DataValue<T>()
        {
			Logger.LogDebug(">>> Entering DataValue()");
			Logger.LogDebug("<<< Exiting DataValue()");
			Logger.LogDebug("<<< Exiting DataValue(format={Format})", format);
            return (T)this.Page.GetDataItem();
        }

        /// <summary>Gets the formatted value.</summary>
        /// <param name="format">The format string.</param>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <returns>The formatted value.</returns>
        protected string DataValue<T>(string format)
        {
            return string.Format(CultureInfo.CurrentCulture, format, this.DataValue<T>());
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            PageViewHost.Register(this, this.Context, false);

            base.OnInit(e);

            this.Page.InitComplete += this.PageInitComplete;
            this.Page.PreRenderComplete += this.PagePreRenderComplete;
            this.Page.Load += this.PageLoad;
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        private void PageInitComplete(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering PageInitComplete(sender={Sender},e={E})", sender, e);
            if (this.Initialize != null)
            {
                this.Initialize(this, EventArgs.Empty);
            }
			Logger.LogDebug("<<< Exiting PageInitComplete(sender={Sender},e={E})", sender, e);
        }

        private void PageLoad(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering PageLoad(sender={Sender},e={E})", sender, e);
            if (this.Load != null)
            {
                this.Load(this, e);
            }
			Logger.LogDebug("<<< Exiting PageLoad(sender={Sender},e={E})", sender, e);
        }

        private void PagePreRenderComplete(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering PagePreRenderComplete(sender={Sender},e={E})", sender, e);
            // This event is raised after any async page tasks have completed, so it
            // is safe to data-bind
            if (this.AutoDataBind)
            {
                this.DataBind();
            }
			Logger.LogDebug("<<< Exiting PagePreRenderComplete(sender={Sender},e={E})", sender, e);
        }
    }
}
