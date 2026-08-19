// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:GroupListControl runat=server></{0}:GroupListControl>")]
    public class GroupListControl : WebControl
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        public UserInfo currentUser;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings PortalSettings => PortalSettings.Current;

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string ItemTemplate { get; set; }

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string HeaderTemplate { get; set; }

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string FooterTemplate { get; set; }

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string RowHeaderTemplate { get; set; }

        [DefaultValue("")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public string RowFooterTemplate { get; set; }

        [DefaultValue(1)]
        public int ItemsPerRow { get; set; }

        [DefaultValue(0)]
        public int RoleGroupId { get; set; }

        [DefaultValue(20)]
        public int PageSize { get; set; }

        [DefaultValue(0)]
        public int CurrentIndex { get; set; }

        [DefaultValue(-1)]
        public int TabId { get; set; }

        [DefaultValue("")]
        public string SearchFilter { get; set; }

        [DefaultValue("rolename")]
        public string SortField { get; set; }

        [DefaultValue("asc")]
        public string SortDirection { get; set; }

        [DefaultValue(false)]
        public bool DisplayCurrentUserGroups { get; set; }

        public int GroupViewTabId { get; set; }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.currentUser = UserController.Instance.GetCurrentUserInfo();
			eventLogger.LogDebug("OnInit(e={E}): currentUser → {CurrentUser}", e, currentUser);
			eventLogger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter output)
        {
			eventLogger.LogDebug(">>> Entering Render(output={Output})", output);
            var whereCls = new List<Func<RoleInfo, bool>>
            {
                grp => grp.SecurityMode != SecurityMode.SecurityRole && grp.Status == RoleStatus.Approved,
            };

            if (this.RoleGroupId >= -1)
            {
                whereCls.Add(grp => grp.RoleGroupID == this.RoleGroupId);
            }

            if (this.DisplayCurrentUserGroups)
            {
                whereCls.Add(grp => this.currentUser.IsInRole(grp.RoleName));
            }
            else
            {
                whereCls.Add(grp => grp.IsPublic || this.currentUser.IsInRole(grp.RoleName) || this.currentUser.IsInRole(this.PortalSettings.AdministratorRoleName));
            }

            if (!string.IsNullOrEmpty(this.SearchFilter))
            {
                whereCls.Add(grp => grp.RoleName.ToLowerInvariant().Contains(this.SearchFilter.ToLowerInvariant()) || grp.Description.ToLowerInvariant().Contains(this.SearchFilter.ToLowerInvariant()));
            }

            var roles = RoleController.Instance.GetRoles(this.PortalSettings.PortalId, grp => TestPredicateGroup(whereCls, grp));
			eventLogger.LogDebug("Render(output={Output}): roles → {Roles}", output, roles);

            if (this.SortDirection.ToLowerInvariant() == "asc")
            {
                roles = roles.OrderBy(info => GetOrderByProperty(info, this.SortField)).ToList();
            }
            else
            {
                roles = roles.OrderByDescending(info => GetOrderByProperty(info, this.SortField)).ToList();
				eventLogger.LogDebug(">>> Entering GetOrderByProperty(obj={Obj},property={Property})", obj, property);
				eventLogger.LogDebug("<<< Exiting GetOrderByProperty(obj={Obj},property={Property})", obj, property);
            }

            decimal pages = (decimal)roles.Count / (decimal)this.PageSize;

            output.Write(this.HeaderTemplate);

            this.ItemTemplate = this.ItemTemplate.Replace("{resx:posts}", Localization.GetString("posts", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:members}", Localization.GetString("members", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:photos}", Localization.GetString("photos", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:documents}", Localization.GetString("documents", Constants.SharedResourcesPath));

            this.ItemTemplate = this.ItemTemplate.Replace("{resx:Join}", Localization.GetString("Join", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:Pending}", Localization.GetString("Pending", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("{resx:LeaveGroup}", Localization.GetString("LeaveGroup", Constants.SharedResourcesPath));
            this.ItemTemplate = this.ItemTemplate.Replace("[GroupViewTabId]", this.GroupViewTabId.ToString());

            if (roles.Count == 0)
            {
                output.Write(string.Format("<div class=\"dnnFormMessage dnnFormInfo\"><span>{0}</span></div>", Localization.GetString("NoGroupsFound", Constants.SharedResourcesPath)));
            }

            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["page"]))
            {
                this.CurrentIndex = Convert.ToInt32(HttpContext.Current.Request.QueryString["page"].ToString());
                this.CurrentIndex = this.CurrentIndex - 1;
            }
			eventLogger.LogDebug("Render(output={Output}): this.CurrentIndex → {Arg1}", output, this.CurrentIndex);

            int rowItem = 0;
			eventLogger.LogDebug("Render(output={Output}): rowItem → {RowItem}", output, rowItem);
            int recordStart = this.CurrentIndex * this.PageSize;
			eventLogger.LogDebug("Render(output={Output}): recordStart → {RecordStart}", output, recordStart);

            if (this.CurrentIndex == 0)
            {
                recordStart = 0;
            }

            for (int x = recordStart; x < (recordStart + this.PageSize); x++)
            {
                if (x > roles.Count - 1)
                {
                    break;
                }

                var role = roles[x];

                string rowTemplate = this.ItemTemplate;

                if (rowItem == 0)
                {
                    output.Write(this.RowHeaderTemplate);
                }

                var groupParser = new GroupViewParser(this.PortalSettings, role, this.currentUser, rowTemplate, this.GroupViewTabId);
				eventLogger.LogDebug("Render(output={Output}): groupParser → {GroupParser}", output, groupParser);
                output.Write(groupParser.ParseView());

                rowItem += 1;

                if (rowItem == this.ItemsPerRow)
                {
                    output.Write(this.RowFooterTemplate);
                    rowItem = 0;
                }
            }

            if (rowItem > 0)
            {
                output.Write(this.RowFooterTemplate);
            }

            output.Write(this.FooterTemplate);

            int totalPages = Convert.ToInt32(System.Math.Ceiling(pages));
			eventLogger.LogDebug("Render(output={Output}): totalPages → {TotalPages}", output, totalPages);

            if (totalPages == 0)
            {
                totalPages = 1;
            }

            string sUrlFormat = "<a href=\"{0}\" class=\"{1}\">{2}</a>";
            string[] currParams = new string[] { };

            StringBuilder sb = new StringBuilder();
			eventLogger.LogDebug("Render(output={Output}): sb → {Sb}", output, sb);

            if (totalPages > 1)
            {
                for (int x = 1; x <= totalPages; x++)
                {
                    string[] @params = new string[] { };

                    if (currParams.Length > 0 & x > 1)
                    {
                        @params = Utilities.AddParams("page=" + x.ToString(), currParams);
                    }
                    else if (currParams.Length > 0 & x == 1)
                    {
                        @params = currParams;
                    }
                    else if (x > 1)
                    {
                        @params = new string[] { "page=" + x.ToString() };
                    }

                    string sUrl = Utilities.NavigateUrl(this.TabId, @params);

                    string cssClass = "pagerItem";
					eventLogger.LogDebug("Render(output={Output}): cssClass → {CssClass}", output, cssClass);

                    if (x - 1 == this.CurrentIndex)
                    {
                        cssClass = "pagerItemSelected";
                    }

                    sb.AppendFormat(sUrlFormat, sUrl, cssClass, x.ToString());
                }
            }

            output.Write("<div class=\"dnnClear groupPager\">");
            output.Write(sb.ToString());
            output.Write("</div>");
			eventLogger.LogDebug("<<< Exiting Render(output={Output})", output);
        }

        private static bool TestPredicateGroup(IEnumerable<Func<RoleInfo, bool>> predicates, RoleInfo ri)
        {
			eventLogger.LogDebug(">>> Entering TestPredicateGroup(predicates={Predicates},ri={Ri})", predicates, ri);
			eventLogger.LogDebug("<<< Exiting TestPredicateGroup(predicates={Predicates},ri={Ri})", predicates, ri);
            return predicates.All(p => p(ri));
        }

        private static object GetOrderByProperty(object obj, string property)
        {
			eventLogger.LogDebug(">>> Entering GetOrderByProperty(obj={Obj},property={Property})", obj, property);
            var propertyInfo = obj.GetType().GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			eventLogger.LogDebug("GetOrderByProperty(obj={Obj},property={Property}): propertyInfo → {PropertyInfo}", obj, property, propertyInfo);
			eventLogger.LogDebug("<<< Exiting GetOrderByProperty(obj={Obj},property={Property})", obj, property);
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null);
        }
    }
}
