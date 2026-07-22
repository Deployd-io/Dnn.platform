// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Services.Localization;

    [ToolboxData("<{0}:PagingControl runat=server></{0}:PagingControl>")]
    public class PagingControl : WebControl, IPostBackEventHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected Repeater PageNumbers;

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected TableCell cellDisplayLinks;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected TableCell cellDisplayStatus;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Table tablePageNumbers;
        private int totalPages = -1;
        private string cssClassLinkActive;
        private string cssClassLinkInactive;
        private string cssClassPagingStatus;
        private PagingControlMode mode = PagingControlMode.URL;

        public event EventHandler PageChanged;

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string CSSClassLinkActive
        {
            get
            {
                return string.IsNullOrEmpty(this.cssClassLinkActive) ? string.Empty : this.cssClassLinkActive;
            }

            set
            {
                this.cssClassLinkActive = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string CSSClassLinkInactive
        {
            get
            {
                return string.IsNullOrEmpty(this.cssClassLinkInactive) ? string.Empty : this.cssClassLinkInactive;
            }

            set
            {
                this.cssClassLinkInactive = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string CSSClassPagingStatus
        {
            get
            {
                return string.IsNullOrEmpty(this.cssClassPagingStatus) ? string.Empty : this.cssClassPagingStatus;
            }

            set
            {
                this.cssClassPagingStatus = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("1")]
        public int CurrentPage { get; set; }

        public PagingControlMode Mode
        {
            get
            {
                return this.mode;
            }

            set
            {
                this.mode = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("10")]
        public int PageSize { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        public string QuerystringParams { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("-1")]
        public int TabID { get; set; }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("0")]
        public int TotalRecords { get; set; }

        /// <inheritdoc />
        public void RaisePostBackEvent(string eventArgument)
        {
			Logger.LogDebug(">>> Entering RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
            this.CurrentPage = int.Parse(eventArgument.Replace("Page_", string.Empty), CultureInfo.InvariantCulture);
            this.OnPageChanged(EventArgs.Empty);
			Logger.LogDebug("<<< Exiting RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            this.tablePageNumbers = new Table();
            this.cellDisplayStatus = new TableCell();
			Logger.LogDebug("CreateChildControls(): cellDisplayStatus → {CellDisplayStatus}", cellDisplayStatus);
			Logger.LogDebug("CreateChildControls(): tablePageNumbers → {TablePageNumbers}", tablePageNumbers);
            this.cellDisplayLinks = new TableCell();
			Logger.LogDebug("CreateChildControls(): cellDisplayLinks → {CellDisplayLinks}", cellDisplayLinks);

            ////cellDisplayStatus.CssClass = "Normal";
            ////cellDisplayLinks.CssClass = "Normal";
            this.tablePageNumbers.CssClass = string.IsNullOrEmpty(this.CssClass) ? "PagingTable" : this.CssClass;
            var intRowIndex = this.tablePageNumbers.Rows.Add(new TableRow());
            this.PageNumbers = new Repeater();
			Logger.LogDebug("CreateChildControls(): intRowIndex → {IntRowIndex}", intRowIndex);
			Logger.LogDebug("CreateChildControls(): PageNumbers → {PageNumbers}", PageNumbers);
            var i = new PageNumberLinkTemplate(this);
            this.PageNumbers.ItemTemplate = i;
            this.BindPageNumbers(this.TotalRecords, this.PageSize);
            this.cellDisplayStatus.HorizontalAlign = HorizontalAlign.Left;

            ////cellDisplayStatus.Width = new Unit("50%");
            this.cellDisplayLinks.HorizontalAlign = HorizontalAlign.Right;

            ////cellDisplayLinks.Width = new Unit("50%");
            var intTotalPages = this.totalPages;
            if (intTotalPages == 0)
			Logger.LogDebug("CreateChildControls(): intTotalPages → {IntTotalPages}", intTotalPages);
            {
                intTotalPages = 1;
            }

            var str = string.Format(CultureInfo.CurrentCulture, Localization.GetString("Pages"), this.CurrentPage, intTotalPages);
            var lit = new LiteralControl(str);
			Logger.LogDebug("CreateChildControls(): str → {Str}", str);
			Logger.LogDebug("CreateChildControls(): lit → {Lit}", lit);
            this.cellDisplayStatus.Controls.Add(lit);
            this.tablePageNumbers.Rows[intRowIndex].Cells.Add(this.cellDisplayStatus);
            this.tablePageNumbers.Rows[intRowIndex].Cells.Add(this.cellDisplayLinks);
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        protected void OnPageChanged(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPageChanged(e={E})", e);
            this.PageChanged?.Invoke(this, e);
			Logger.LogDebug("<<< Exiting OnPageChanged(e={E})", e);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        protected override void Render(HtmlTextWriter output)
        {
			Logger.LogDebug(">>> Entering Render(output={Output})", output);
            if (this.PageNumbers == null)
            {
                this.CreateChildControls();
            }

            var str = new StringBuilder();
            str.Append(this.GetFirstLink() + "&nbsp;&nbsp;&nbsp;");
			Logger.LogDebug("Render(output={Output}): str → {Str}", output, str);
            str.Append(this.GetPreviousLink() + "&nbsp;&nbsp;&nbsp;");
            var result = new StringBuilder(1024);
            this.PageNumbers.RenderControl(new HtmlTextWriter(new StringWriter(result)));
			Logger.LogDebug("Render(output={Output}): result → {Result}", output, result);
            str.Append(result.ToString());
            str.Append(this.GetNextLink() + "&nbsp;&nbsp;&nbsp;");
            str.Append(this.GetLastLink() + "&nbsp;&nbsp;&nbsp;");
            this.cellDisplayLinks.Controls.Add(new LiteralControl(str.ToString()));
            this.tablePageNumbers.RenderControl(output);
			Logger.LogDebug("<<< Exiting Render(output={Output})", output);
        }

        private void BindPageNumbers(int totalRecords, int recordsPerPage)
        {
			Logger.LogDebug(">>> Entering BindPageNumbers(totalRecords={TotalRecords},recordsPerPage={RecordsPerPage})", totalRecords, recordsPerPage);
            const int pageLinksPerPage = 10;
            if (totalRecords < 1 || recordsPerPage < 1)
            {
                this.totalPages = 1;
				Logger.LogDebug("BindPageNumbers(totalRecords={TotalRecords},recordsPerPage={RecordsPerPage}): totalPages → {TotalPages}", totalRecords, recordsPerPage, totalPages);
				Logger.LogDebug("BindPageNumbers(totalRecords={TotalRecords},recordsPerPage={RecordsPerPage}): this.totalPages → {Arg2}", totalRecords, recordsPerPage, this.totalPages);
                return;
            }

            this.totalPages = totalRecords / recordsPerPage >= 1 ? Convert.ToInt32(Math.Ceiling(Convert.ToDouble(totalRecords) / recordsPerPage)) : 0;
            if (this.totalPages > 0)
            {
                var ht = new DataTable();
                ht.Columns.Add("PageNum");
				Logger.LogDebug("BindPageNumbers(totalRecords={TotalRecords},recordsPerPage={RecordsPerPage}): ht → {Ht}", totalRecords, recordsPerPage, ht);
                DataRow tmpRow;
                var lowNum = 1;
                var highNum = Convert.ToInt32(this.totalPages);
				Logger.LogDebug("BindPageNumbers(totalRecords={TotalRecords},recordsPerPage={RecordsPerPage}): lowNum → {LowNum}", totalRecords, recordsPerPage, lowNum);
				Logger.LogDebug("BindPageNumbers(totalRecords={TotalRecords},recordsPerPage={RecordsPerPage}): highNum → {HighNum}", totalRecords, recordsPerPage, highNum);
                double tmpNum;
                tmpNum = this.CurrentPage - (pageLinksPerPage / 2);
				Logger.LogDebug("BindPageNumbers(totalRecords={TotalRecords},recordsPerPage={RecordsPerPage}): tmpNum → {TmpNum}", totalRecords, recordsPerPage, tmpNum);
                if (tmpNum < 1)
                {
                    tmpNum = 1;
                }

                if (this.CurrentPage > (pageLinksPerPage / 2))
                {
                    lowNum = Convert.ToInt32(Math.Floor(tmpNum));
                }

                if (Convert.ToInt32(this.totalPages) <= pageLinksPerPage)
                {
                    highNum = Convert.ToInt32(this.totalPages);
                }
                else
                {
                    highNum = lowNum + pageLinksPerPage - 1;
                }

                if (highNum > Convert.ToInt32(this.totalPages))
                {
                    highNum = Convert.ToInt32(this.totalPages);
                    if (highNum - lowNum < pageLinksPerPage)
                    {
                        lowNum = highNum - pageLinksPerPage + 1;
                    }
                }

                if (highNum > Convert.ToInt32(this.totalPages))
                {
                    highNum = Convert.ToInt32(this.totalPages);
                }

                if (lowNum < 1)
                {
                    lowNum = 1;
                }

                int i;
                for (i = lowNum; i <= highNum; i++)
                {
                    tmpRow = ht.NewRow();
					Logger.LogDebug("BindPageNumbers(totalRecords={TotalRecords},recordsPerPage={RecordsPerPage}): tmpRow → {TmpRow}", totalRecords, recordsPerPage, tmpRow);
                    tmpRow["PageNum"] = i;
                    ht.Rows.Add(tmpRow);
                }

                this.PageNumbers.DataSource = ht;
                this.PageNumbers.DataBind();
            }
			Logger.LogDebug("<<< Exiting BindPageNumbers(totalRecords={TotalRecords},recordsPerPage={RecordsPerPage})", totalRecords, recordsPerPage);
        }

        private string CreateUrl(string currentPage)
        {
			Logger.LogDebug(">>> Entering CreateUrl(currentPage={CurrentPage})", currentPage);
            switch (this.Mode)
            {
                case PagingControlMode.URL:
                    return !string.IsNullOrEmpty(this.QuerystringParams)
                               ? (!string.IsNullOrEmpty(currentPage) ? TestableGlobals.Instance.NavigateURL(this.TabID, string.Empty, this.QuerystringParams, "currentpage=" + currentPage) : TestableGlobals.Instance.NavigateURL(this.TabID, string.Empty, this.QuerystringParams))
                               : (!string.IsNullOrEmpty(currentPage) ? TestableGlobals.Instance.NavigateURL(this.TabID, string.Empty, "currentpage=" + currentPage) : TestableGlobals.Instance.NavigateURL(this.TabID));
                default:
					Logger.LogDebug("<<< Exiting CreateUrl(currentPage={CurrentPage})", currentPage);
                    return this.Page.ClientScript.GetPostBackClientHyperlink(this, "Page_" + currentPage, false);
            }
        }

        /// <summary>GetLink returns the page number links for paging.</summary>
        private string GetLink(int pageNum)
        {
			Logger.LogDebug(">>> Entering GetLink(pageNum={PageNum})", pageNum);
            if (pageNum == this.CurrentPage)
            {
                return this.CSSClassLinkInactive.Trim().Length > 0
                    ? $"<span class=\"{this.CSSClassLinkInactive}\">[{pageNum}]</span>"
                    : $"<span>[{pageNum}]</span>";
            }

            var url = this.CreateUrl(pageNum.ToString(CultureInfo.InvariantCulture));
			Logger.LogDebug("GetLink(pageNum={PageNum}): url → {Url}", pageNum, url);
			Logger.LogDebug("<<< Exiting GetLink(pageNum={PageNum})", pageNum);
            return this.CSSClassLinkActive.Trim().Length > 0
                       ? $"<a href=\"{url}\" class=\"{this.CSSClassLinkActive}\">{pageNum}</a>"
                       : $"<a href=\"{url}\">{pageNum}</a>";
        }

        /// <summary>GetPreviousLink returns the link for the Previous page for paging.</summary>
        private string GetPreviousLink()
        {
			Logger.LogDebug(">>> Entering GetPreviousLink()");
            var url = this.CreateUrl((this.CurrentPage - 1).ToString(CultureInfo.InvariantCulture));
            var previousText = Localization.GetString("Previous", Localization.SharedResourceFile);
			Logger.LogDebug("GetPreviousLink(): url → {Url}", url);
			Logger.LogDebug("GetPreviousLink(): previousText → {PreviousText}", previousText);
			Logger.LogDebug("<<< Exiting GetPreviousLink()");
            return this.CurrentPage > 1 && this.totalPages > 0
                       ? (this.CSSClassLinkActive.Trim().Length > 0
                              ? $"<a href=\"{url}\" class=\"{this.CSSClassLinkActive}\">{previousText}</a>"
                              : $"<a href=\"{url}\">{previousText}</a>")
                       : (this.CSSClassLinkInactive.Trim().Length > 0
                              ? $"<span class=\"{this.CSSClassLinkInactive}\">{previousText}</span>"
                              : $"<span>{previousText}</span>");
        }

        /// <summary>GetNextLink returns the link for the Next Page for paging.</summary>
        private string GetNextLink()
        {
			Logger.LogDebug(">>> Entering GetNextLink()");
            var url = this.CreateUrl((this.CurrentPage + 1).ToString(CultureInfo.InvariantCulture));
            var nextText = Localization.GetString("Next", Localization.SharedResourceFile);
			Logger.LogDebug("GetNextLink(): url → {Url}", url);
			Logger.LogDebug("GetNextLink(): nextText → {NextText}", nextText);
			Logger.LogDebug("<<< Exiting GetNextLink()");
            return this.CurrentPage != this.totalPages && this.totalPages > 0
                       ? (this.CSSClassLinkActive.Trim().Length > 0
                              ? $"<a href=\"{url}\" class=\"{this.CSSClassLinkActive}\">{nextText}</a>"
                              : $"<a href=\"{url}\">{nextText}</a>")
                       : (this.CSSClassLinkInactive.Trim().Length > 0
                              ? $"<span class=\"{this.CSSClassLinkInactive}\">{nextText}</span>"
                              : $"<span>{nextText}</span>");
        }

        /// <summary>GetFirstLink returns the First Page link for paging.</summary>
        private string GetFirstLink()
        {
			Logger.LogDebug(">>> Entering GetFirstLink()");
            var firstText = Localization.GetString("First", Localization.SharedResourceFile);
            if (this.CurrentPage > 1 && this.totalPages > 0)
			Logger.LogDebug("GetFirstLink(): firstText → {FirstText}", firstText);
            {
                var url = this.CreateUrl("1");
                return this.CSSClassLinkActive.Trim().Length > 0
				Logger.LogDebug("GetFirstLink(): url → {Url}", url);
                           ? $"<a href=\"{url}\" class=\"{this.CSSClassLinkActive}\">{firstText}</a>"
                           : $"<a href=\"{url}\">{firstText}</a>";
            }

			Logger.LogDebug("<<< Exiting GetFirstLink()");
            return this.CSSClassLinkInactive.Trim().Length > 0
                       ? $"<span class=\"{this.CSSClassLinkInactive}\">{firstText}</span>"
                       : $"<span>{firstText}</span>";
        }

        /// <summary>GetLastLink returns the Last Page link for paging.</summary>
        private string GetLastLink()
        {
			Logger.LogDebug(">>> Entering GetLastLink()");
            var lastText = Localization.GetString("Last", Localization.SharedResourceFile);
            if (this.CurrentPage != this.totalPages && this.totalPages > 0)
			Logger.LogDebug("GetLastLink(): lastText → {LastText}", lastText);
            {
                var url = this.CreateUrl(this.totalPages.ToString(CultureInfo.InvariantCulture));
                return this.CSSClassLinkActive.Trim().Length > 0
				Logger.LogDebug("GetLastLink(): url → {Url}", url);
                           ? $"<a href=\"{url}\" class=\"{this.CSSClassLinkActive}\">{lastText}</a>"
                           : $"<a href=\"{url}\">{lastText}</a>";
            }

			Logger.LogDebug("<<< Exiting GetLastLink()");
            return this.CSSClassLinkInactive.Trim().Length > 0
                       ? $"<span class=\"{this.CSSClassLinkInactive}\">{lastText}</span>"
                       : $"<span>{lastText}</span>";
        }

        public class PageNumberLinkTemplate : ITemplate
        {
            private readonly PagingControl pagingControl;

            /// <summary>Initializes a new instance of the <see cref="PageNumberLinkTemplate"/> class.</summary>
            /// <param name="ctlPagingControl">The paging control.</param>
            public PageNumberLinkTemplate(PagingControl ctlPagingControl)
            {
				Logger.LogDebug(">>> Entering PageNumberLinkTemplate(ctlPagingControl={CtlPagingControl})", ctlPagingControl);
                this.pagingControl = ctlPagingControl;
				Logger.LogDebug("<<< Exiting PageNumberLinkTemplate(ctlPagingControl={CtlPagingControl})", ctlPagingControl);
				Logger.LogDebug("PageNumberLinkTemplate(ctlPagingControl={CtlPagingControl}): pagingControl → {PagingControl}", ctlPagingControl, pagingControl);
				Logger.LogDebug("PageNumberLinkTemplate(ctlPagingControl={CtlPagingControl}): this.pagingControl → {Arg1}", ctlPagingControl, this.pagingControl);
            }

            /// <inheritdoc />
            void ITemplate.InstantiateIn(Control container)
            {
                var l = new Literal();
                l.DataBinding += this.BindData;
                container.Controls.Add(l);
            }

            private void BindData(object sender, EventArgs e)
            {
				Logger.LogDebug(">>> Entering BindData(sender={Sender},e={E})", sender, e);
                var lc = (Literal)sender;
                var container = (RepeaterItem)lc.NamingContainer;
				Logger.LogDebug("BindData(sender={Sender},e={E}): lc → {Lc}", sender, e, lc);
				Logger.LogDebug("BindData(sender={Sender},e={E}): container → {Container}", sender, e, container);
                lc.Text = this.pagingControl.GetLink(Convert.ToInt32(DataBinder.Eval(container.DataItem, "PageNum"), CultureInfo.InvariantCulture)) + "&nbsp;&nbsp;";
				Logger.LogDebug("<<< Exiting BindData(sender={Sender},e={E})", sender, e);
            }
        }
    }
}
