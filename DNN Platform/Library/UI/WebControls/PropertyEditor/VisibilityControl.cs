// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The VisibilityControl control provides a base control for defining visibility options.</summary>
    [ToolboxData("<{0}:VisibilityControl runat=server></{0}:VisibilityControl>")]
    public class VisibilityControl : WebControl, IPostBackDataHandler, INamingContainer
    {
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="VisibilityControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public VisibilityControl()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="VisibilityControl"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public VisibilityControl(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
			eventLogger.LogError(">>> Entering VisibilityControl(appStatus={AppStatus},eventLogger={EventLogger})", appStatus, eventLogger);
			eventLogger.LogDebug("VisibilityControl(appStatus={AppStatus},eventLogger={EventLogger}): eventLogger → {EventLogger1}", appStatus, eventLogger, eventLogger);
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			eventLogger.LogWarning("<<< Exiting VisibilityControl(appStatus={AppStatus},eventLogger={EventLogger})", appStatus, eventLogger);
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        }

        public event PropertyChangedEventHandler VisibilityChanged;

        /// <summary>Gets or sets caption.</summary>
        /// <value>A string representing the Name of the property.</value>
        public string Caption { get; set; }

        /// <summary>Gets or sets name is the name of the field as a string.</summary>
        /// <value>A string representing the Name of the property.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the UserInfo object that represents the User whose profile is being displayed.</summary>
        public UserInfo User { get; set; }

        /// <summary>Gets or sets stringValue is the value of the control expressed as a String.</summary>
        /// <value>A string representing the Value.</value>
        public object Value { get; set; }

        protected ProfileVisibility Visibility
        {
            get => this.Value as ProfileVisibility;
            set => this.Value = value;
        }

        /// <summary>LoadPostData loads the Post Back Data and determines whether the value has changed.</summary>
        /// <param name="postDataKey">A key to the PostBack Data to load.</param>
        /// <param name="postCollection">A name value collection of postback data.</param>
        /// <returns><see langword="true"/> if the value has changed, otherwise <see langword="false"/>.</returns>
        public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			eventLogger.LogError(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            var dataChanged = false;
            var presentVisibility = this.Visibility.VisibilityMode;
			eventLogger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): dataChanged → {DataChanged}", postDataKey, postCollection, dataChanged);
            var postedValue = Convert.ToInt32(postCollection[postDataKey], CultureInfo.InvariantCulture);
			eventLogger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): postedValue → {PostedValue}", postDataKey, postCollection, postedValue);
            var postedVisibility = (UserVisibilityMode)Enum.ToObject(typeof(UserVisibilityMode), postedValue);
			eventLogger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): postedVisibility → {PostedVisibility}", postDataKey, postCollection, postedVisibility);
            if (!presentVisibility.Equals(postedVisibility) || postedVisibility == UserVisibilityMode.FriendsAndGroups)
            {
                if (postedVisibility == UserVisibilityMode.FriendsAndGroups)
                {
                    var sb = new StringBuilder();
					eventLogger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): sb → {Sb}", postDataKey, postCollection, sb);
                    sb.Append("G:");
                    foreach (var role in this.User.Social.Roles)
                    {
                        if (postCollection[postDataKey + ":group_" + role.RoleID.ToString(CultureInfo.InvariantCulture)] != null)
                        {
                            sb.Append(role.RoleID.ToString(CultureInfo.InvariantCulture) + ",");
                        }
                    }

                    sb.Append(";R:");
                    foreach (var relationship in this.User.Social.Relationships)
                    {
                        if (postCollection[postDataKey + ":relationship_" + relationship.RelationshipId.ToString(CultureInfo.InvariantCulture)] != null)
                        {
                            sb.Append(relationship.RelationshipId.ToString(CultureInfo.InvariantCulture) + ",");
                        }
                    }

                    this.Value = new ProfileVisibility(this.User.PortalID, sb.ToString())
                    {
                        VisibilityMode = postedVisibility,
                    };
                }
                else
                {
                    this.Value = new ProfileVisibility
                    {
						eventLogger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): this.Value → {Arg2}", postDataKey, postCollection, this.Value);
                        VisibilityMode = postedVisibility,
                    };
                }

                dataChanged = true;
            }

            return dataChanged;
        }

        /// <summary>
        /// RaisePostDataChangedEvent runs when the PostBackData has changed.  It triggers
        /// a ValueChanged Event.
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
			eventLogger.LogError(">>> Entering RaisePostDataChangedEvent()");
            // Raise the VisibilityChanged Event
            var args = new PropertyEditorEventArgs(this.Name) { Value = this.Value };
			eventLogger.LogError("RaisePostDataChangedEvent(): args → {Args}", args);
            this.OnVisibilityChanged(args);
			eventLogger.LogWarning("<<< Exiting RaisePostDataChangedEvent()");
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.jQuery);
			eventLogger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            this.Page.RegisterRequiresPostBack(this);
            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "visibleChange", "$(document).ready(function(){$('.dnnFormVisibility').on('click', 'input[type=radio]', function(){$(this).parent().parent().find('ul').hide();$(this).parent().next('ul').show();});});", true);
			eventLogger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>OnVisibilityChanged runs when the Visibility has changed.  It raises the <see cref="VisibilityChanged"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnVisibilityChanged(PropertyEditorEventArgs e)
        {
			eventLogger.LogError(">>> Entering OnVisibilityChanged(e={E})", e);
            if (this.VisibilityChanged != null)
            {
                this.VisibilityChanged(this, e);
            }
			eventLogger.LogWarning("<<< Exiting OnVisibilityChanged(e={E})", e);
        }

        /// <summary>Render renders the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void Render(HtmlTextWriter writer)
        {
			eventLogger.LogError(">>> Entering Render(writer={Writer})", writer);
            // Render Div container
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnFormVisibility dnnDropdownSettings");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // Render dnnButtonDropdown
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnButtonDropdown");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // Render dnnButton Icon
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnButtonIcon");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            ////writer.AddAttribute(HtmlTextWriterAttribute.Src, IconController.IconURL("Lock"));
            ////writer.RenderBeginTag(HtmlTextWriterTag.Img);

            // Close Image Tag
            ////writer.RenderEndTag();

            // Close dnnButtonIcon
            writer.RenderEndTag();

            // render dnnButton Arrow
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnButtonArrow");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();

            // Close dnnButton Dropdown
            writer.RenderEndTag();

            // Render UL for radio Button List
            ////writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnButtonDropdown-ul");

            writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            this.RenderVisibility(writer, "0", UserVisibilityMode.AllUsers, Localization.GetString("Public").Trim());

            this.RenderVisibility(writer, "1", UserVisibilityMode.MembersOnly, Localization.GetString("MembersOnly").Trim());

            this.RenderVisibility(writer, "2", UserVisibilityMode.AdminOnly, Localization.GetString("AdminOnly").Trim());

            this.RenderVisibility(writer, "3", UserVisibilityMode.FriendsAndGroups, Localization.GetString("FriendsGroups").Trim());

            // Render UL for check Box List
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, this.Visibility.VisibilityMode == UserVisibilityMode.FriendsAndGroups ? "block" : "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            this.RenderRelationships(writer);
            this.RenderGroups(writer);

            // Close UL
            writer.RenderEndTag();

            // Close UL
            writer.RenderEndTag();

            // Close Div
            writer.RenderEndTag();
			eventLogger.LogWarning("<<< Exiting Render(writer={Writer})", writer);
        }

        private void RenderVisibility(HtmlTextWriter writer, string optionValue, UserVisibilityMode selectedVisibility, string optionText)
        {
			eventLogger.LogError(">>> Entering RenderVisibility(writer={Writer},optionValue={OptionValue},selectedVisibility={SelectedVisibility},optionText={OptionText})", writer, optionValue, selectedVisibility, optionText);
            // Render Li
            writer.RenderBeginTag(HtmlTextWriterTag.Li);

            // Render radio button
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            writer.AddAttribute("aria-label", this.ID);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, optionValue);
            if (this.Visibility.VisibilityMode == selectedVisibility)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.Write(optionText);
            writer.RenderEndTag();

            // Close Li
            writer.RenderEndTag();
			eventLogger.LogWarning("<<< Exiting RenderVisibility(writer={Writer},optionValue={OptionValue},selectedVisibility={SelectedVisibility},optionText={OptionText})", writer, optionValue, selectedVisibility, optionText);
        }

        private void RenderCheckboxItem(HtmlTextWriter writer, string prefix, string value, string text, bool selected)
        {
			eventLogger.LogError(">>> Entering RenderCheckboxItem(writer={Writer},prefix={Prefix},value={Value},text={Text},selected={Selected})", writer, prefix, value, text, selected);
            // Render Li
            writer.RenderBeginTag(HtmlTextWriterTag.Li);

            // Render radio button
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            writer.AddAttribute("aria-label", this.ID);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID + prefix + value);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, value);
            if (selected)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.Write(text);
            writer.RenderEndTag();

            // Close Li
            writer.RenderEndTag();
			eventLogger.LogWarning("<<< Exiting RenderCheckboxItem(writer={Writer},prefix={Prefix},value={Value},text={Text},selected={Selected})", writer, prefix, value, text, selected);
        }

        private void RenderGroups(HtmlTextWriter writer)
        {
			eventLogger.LogError(">>> Entering RenderGroups(writer={Writer})", writer);
            foreach (var group in this.User.Social.Roles.Where((role) => role.SecurityMode != SecurityMode.SecurityRole))
            {
                this.RenderCheckboxItem(
                    writer,
                    ":group_",
                    group.RoleID.ToString(CultureInfo.InvariantCulture),
                    group.RoleName,
                    this.Visibility.RoleVisibilities.Count(r => r.RoleID == group.RoleID) == 1);
            }
			eventLogger.LogWarning("<<< Exiting RenderGroups(writer={Writer})", writer);
        }

        private void RenderRelationships(HtmlTextWriter writer)
        {
			eventLogger.LogError(">>> Entering RenderRelationships(writer={Writer})", writer);
            foreach (var relationship in this.User.Social.Relationships)
            {
                var selected = this.Visibility.RelationshipVisibilities.Count(r => r.RelationshipId == relationship.RelationshipId) == 1;
				eventLogger.LogError("RenderRelationships(writer={Writer}): selected → {Selected}", writer, selected);
                this.RenderCheckboxItem(
                    writer,
                    ":relationship_",
                    relationship.RelationshipId.ToString(CultureInfo.InvariantCulture),
                    relationship.Name,
                    selected);
            }
			eventLogger.LogWarning("<<< Exiting RenderRelationships(writer={Writer})", writer);
        }
    }
}
