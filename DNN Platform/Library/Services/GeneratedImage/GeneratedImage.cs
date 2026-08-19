// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class GeneratedImage : Image
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GeneratedImage));
        private const string TimestampField = "__timestamp";
        private readonly Control bindingContainer;
        private readonly HttpContextBase context;
        private string timestamp;
        private string imageHandlerUrl;

        /// <summary>Initializes a new instance of the <see cref="GeneratedImage"/> class.</summary>
        public GeneratedImage()
        {
			Logger.LogDebug(">>> Entering GeneratedImage()");
            this.Parameters = new List<ImageParameter>();
			Logger.LogDebug("<<< Exiting GeneratedImage()");
        }

        /// <summary>Initializes a new instance of the <see cref="GeneratedImage"/> class.</summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="bindingContainer">The control into which the image should be rendered.</param>
        internal GeneratedImage(HttpContextBase context, Control bindingContainer)
            : this()
        {
            this.context = context;
            this.bindingContainer = bindingContainer;
			Logger.LogDebug("GeneratedImage(context={Context},bindingContainer={BindingContainer}): this.context → {Arg2}", context, bindingContainer, this.context);
        }

        public List<ImageParameter> Parameters { get; }

        public string ImageHandlerUrl
        {
            get
            {
                return this.imageHandlerUrl ?? string.Empty;
            }

            set
            {
                this.imageHandlerUrl = value;
            }
        }

        public string Timestamp
        {
            get
            {
                return this.timestamp ?? string.Empty;
            }

            set
            {
                this.timestamp = value;
            }
        }

        private new HttpContextBase Context => this.context ?? new HttpContextWrapper(HttpContext.Current);

        private new Control BindingContainer => this.bindingContainer ?? base.BindingContainer;

        /// <inheritdoc />
        protected override void OnDataBinding(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnDataBinding(e={E})", e);
            base.OnDataBinding(e);

            Control bindingContainer = this.BindingContainer;
            foreach (var parameter in this.Parameters)
            {
                parameter.BindingContainer = bindingContainer;
				Logger.LogDebug("OnDataBinding(e={E}): parameter.BindingContainer → {Arg1}", e, parameter.BindingContainer);
                parameter.DataBind();
            }
			Logger.LogDebug("<<< Exiting OnDataBinding(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (this.DesignMode)
            {
                return;
            }

            this.ImageUrl = this.BuildImageUrl();
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private static void AddQueryStringParameter(StringBuilder stringBuilder, bool paramAlreadyAdded, string name, string value)
        {
			Logger.LogDebug(">>> Entering AddQueryStringParameter(stringBuilder={StringBuilder},paramAlreadyAdded={ParamAlreadyAdded},name={Name},value={Value})", stringBuilder, paramAlreadyAdded, name, value);
            stringBuilder.Append(paramAlreadyAdded ? '&' : '?');
            stringBuilder.Append(HttpUtility.UrlEncode(name));
            stringBuilder.Append('=');
            stringBuilder.Append(HttpUtility.UrlEncode(value));
			Logger.LogDebug("<<< Exiting AddQueryStringParameter(stringBuilder={StringBuilder},paramAlreadyAdded={ParamAlreadyAdded},name={Name},value={Value})", stringBuilder, paramAlreadyAdded, name, value);
        }

        private string BuildImageUrl()
        {
			Logger.LogDebug(">>> Entering BuildImageUrl()");
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(this.ImageHandlerUrl);
			Logger.LogDebug("BuildImageUrl(): stringBuilder → {StringBuilder}", stringBuilder);

            var paramAlreadyAdded = false;
			Logger.LogDebug("BuildImageUrl(): paramAlreadyAdded → {ParamAlreadyAdded}", paramAlreadyAdded);

            foreach (var parameter in this.Parameters)
            {
                AddQueryStringParameter(stringBuilder, paramAlreadyAdded, parameter.Name, parameter.Value);
                paramAlreadyAdded = true;
            }

            string timeStamp = this.Timestamp?.Trim();
            if (!string.IsNullOrEmpty(timeStamp))
            {
                AddQueryStringParameter(stringBuilder, paramAlreadyAdded, TimestampField, timeStamp);
            }

			Logger.LogDebug("<<< Exiting BuildImageUrl()");
            return stringBuilder.ToString();
        }
    }
}
