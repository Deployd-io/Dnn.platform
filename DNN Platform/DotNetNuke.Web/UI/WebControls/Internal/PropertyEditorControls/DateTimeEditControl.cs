// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal.PropertyEditorControls
{
    using System;
    using System.Collections.Specialized;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The DateEditControl control provides a standard UI component for editing date properties.</summary>
    /// <remarks>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</remarks>
    [ToolboxData("<{0}:DateTimeEditControl runat=server></{0}:DateTimeEditControl>")]
    public class DateTimeEditControl : EditControl
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DateTimeEditControl));
        private TextBox dateControl;

        /// <inheritdoc />
        public override string ID
        {
            get => base.ID + "_control";
            set => base.ID = value;
        }

        /// <summary>Gets dateValue returns the Date representation of the Value.</summary>
        /// <value>A Date representing the Value.</value>
        protected DateTime DateValue
        {
            get
            {
                DateTime dteValue = Null.NullDate;
                try
                {
                    var dteString = Convert.ToString(this.Value, CultureInfo.InvariantCulture);
                    DateTime.TryParse(dteString, CultureInfo.InvariantCulture, DateTimeStyles.None, out dteValue);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                return dteValue;
            }
        }

        /// <summary>Gets a string that will be used to format the date in the absence of a FormatAttribute.</summary>
        /// <value>A String representing the default format to use to render the date.</value>
        /// <returns>A Format String.</returns>
        protected virtual string DefaultFormat => "g";

        /// <summary>Gets format is a string that will be used to format the date in View mode.</summary>
        /// <value>A String representing the format to use to render the date.</value>
        /// <returns>A Format String.</returns>
        protected virtual string Format
        {
            get
            {
                string format = this.DefaultFormat;
                if (this.CustomAttributes != null)
                {
                    foreach (Attribute attribute in this.CustomAttributes)
                    {
                        if (attribute is FormatAttribute formatAttribute)
                        {
                            format = formatAttribute.Format;
                            break;
                        }
                    }
                }

                return format;
            }
        }

        /// <summary>Gets oldDateValue returns the Date representation of the OldValue.</summary>
        /// <value>A Date representing the OldValue.</value>
        protected DateTime OldDateValue
        {
            get
            {
                DateTime dteValue = Null.NullDate;
                try
                {
                    // Try and cast the value to an DateTime
                    var dteString = this.OldValue as string;
                    if (!string.IsNullOrEmpty(dteString))
                    {
                        dteValue = DateTime.Parse(dteString, CultureInfo.InvariantCulture);
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                return dteValue;
            }
        }

        /// <summary>Gets or sets the Value expressed as a String.</summary>
        protected override string StringValue
        {
            get
            {
                string stringValue = Null.NullString;
                if (this.DateValue.ToUniversalTime().Date != (DateTime)SqlDateTime.MinValue && this.DateValue != Null.NullDate)
                {
                    stringValue = this.DateValue.ToString(this.Format, CultureInfo.InvariantCulture);
                }

                return stringValue;
            }

            set
            {
                this.Value = DateTime.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private TextBox DateControl => this.dateControl ??= new TextBox { TextMode = TextBoxMode.DateTimeLocal, };

        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogError(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            this.EnsureChildControls();
            bool dataChanged = false;
			Logger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): dataChanged → {DataChanged}", postDataKey, postCollection, dataChanged);
            string presentValue = this.StringValue;
            string postedValue = postCollection[postDataKey + "_control"];
            if (!presentValue.Equals(postedValue, StringComparison.Ordinal))
            {
                if (string.IsNullOrEmpty(postedValue))
                {
                    this.Value = Null.NullDate;
					Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): Value → {Value}", postDataKey, postCollection, Value);
                    dataChanged = true;
                }
                else
                {
					Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): this.Value → {Arg2}", postDataKey, postCollection, this.Value);
                    if (DateTime.TryParseExact(postedValue, "yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
                    {
                        this.Value = value;
                        dataChanged = true;
                    }
                }
            }

            this.LoadDateControls();
			Logger.LogWarning("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return dataChanged;
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogError(">>> Entering CreateChildControls()");
            base.CreateChildControls();

            this.DateControl.ControlStyle.CopyFrom(this.ControlStyle);
            this.DateControl.ID = base.ID + "_control";

            this.Controls.Add(this.DateControl);
			Logger.LogWarning("<<< Exiting CreateChildControls()");
        }

        /// <summary>Loads the date controls.</summary>
        protected virtual void LoadDateControls()
        {
			Logger.LogError(">>> Entering LoadDateControls()");
            if (this.DateValue != Null.NullDate)
            {
                this.DateControl.Text = this.DateValue.ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture);
            }
			Logger.LogWarning("<<< Exiting LoadDateControls()");
        }

        /// <summary>OnDataChanged is called by the PostBack Handler when the Data has changed.</summary>
        /// <param name="e">An EventArgs object.</param>
        protected override void OnDataChanged(EventArgs e)
        {
			Logger.LogError(">>> Entering OnDataChanged(e={E})", e);
            var args = new PropertyEditorEventArgs(this.Name)
            {
                Value = this.DateValue,
                OldValue = this.OldDateValue,
                StringValue = this.DateValue.ToString(CultureInfo.InvariantCulture),
            };
            this.OnValueChanged(args);
			Logger.LogWarning("<<< Exiting OnDataChanged(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            this.LoadDateControls();

            if (this.Page != null && this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>RenderEditMode is called by the base control to render the control in Edit Mode.</summary>
        /// <param name="writer">The writer to which to render the HTML.</param>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderEditMode(writer={Writer})", writer);
            this.RenderChildren(writer);
			Logger.LogWarning("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }

        /// <summary>RenderViewMode renders the View (readonly) mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderViewMode(writer={Writer})", writer);
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(this.StringValue);
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderViewMode(writer={Writer})", writer);
        }
    }
}
