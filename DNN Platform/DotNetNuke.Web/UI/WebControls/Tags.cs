// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A tags control.</summary>
    /// <param name="vocabularyController">The vocabulary controller.</param>
    /// <param name="termController">The term controller.</param>
    public class Tags(IVocabularyController vocabularyController, ITermController termController)
        : WebControl, IPostBackEventHandler, IPostBackDataHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Tags));
        private readonly IVocabularyController vocabularyController = vocabularyController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IVocabularyController>();
        private readonly ITermController termController = termController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITermController>();
        private string tags;

        /// <summary>Initializes a new instance of the <see cref="Tags"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IVocabularyController. Scheduled removal in v12.0.0.")]
        protected Tags()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering Tags()");
        }

        /// <summary>An event which is triggered when the tags are updated.</summary>
        public event EventHandler<EventArgs> TagsUpdated;

        /// <summary>Gets or sets the URL of the add image.</summary>
        public string AddImageUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether to allow tagging.</summary>
        public bool AllowTagging { get; set; }

        /// <summary>Gets or sets the URL of the cancel image.</summary>
        public string CancelImageUrl { get; set; }

        /// <summary>Gets or sets the content item.</summary>
        public ContentItem ContentItem { get; set; }

        /// <summary>Gets or sets a value indicating whether it's in edit mode.</summary>
        public bool IsEditMode
        {
            get
            {
                bool isEditMode = false;
                if (this.ViewState["IsEditMode"] != null)
                {
                    isEditMode = Convert.ToBoolean(this.ViewState["IsEditMode"], CultureInfo.InvariantCulture);
                }

                return isEditMode;
            }

            set
            {
                this.ViewState["IsEditMode"] = value;
            }
        }

        /// <summary>Gets or sets the URL format string.</summary>
        public string NavigateUrlFormatString { get; set; }

        /// <summary>Gets or sets the repeat direction.</summary>
        public string RepeatDirection { get; set; } = "Horizontal";

        /// <summary>Gets or sets the URL of the save image.</summary>
        public string SaveImageUrl { get; set; }

        /// <summary>Gets or sets the separator.</summary>
        public string Separator { get; set; } = ",&nbsp;";

        /// <summary>Gets or sets a value indicating whether to show categories.</summary>
        public bool ShowCategories { get; set; }

        /// <summary>Gets or sets a value indicating whether to show tags.</summary>
        public bool ShowTags { get; set; }

        private Vocabulary TagVocabulary =>
            this.vocabularyController.GetVocabularies().SingleOrDefault(v => v.IsSystem && v.Name == "Tags");

        /// <inheritdoc />
        public override void RenderControl(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderControl(writer={Writer})", writer);
            // Render Outer Div
            writer.AddAttribute(HtmlTextWriterAttribute.Class, this.RepeatDirection.ToLowerInvariant());
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // Render Categories
            if (this.ShowCategories)
            {
                // Render UL
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "categories");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, this.LocalizeString("Category.ToolTip"));
                writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                // Render Category Links
                var categories = from cat in this.ContentItem.Terms where cat.VocabularyId != this.TagVocabulary.VocabularyId select cat;

                for (int i = 0; i <= categories.Count() - 1; i++)
                {
                    if (i == 0)
                    {
                        // First Category
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "first_tag");
                    }
                    else if (i == categories.Count() - 1)
                    {
                        // Last Category
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "last_tag");
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    this.RenderTerm(writer, categories.ToList()[i], i < categories.Count() - 1 && this.RepeatDirection.Equals("horizontal", StringComparison.OrdinalIgnoreCase));

                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
            }

            if (this.ShowTags)
            {
                // Render UL
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "tags");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, this.LocalizeString("Tag.ToolTip"));
                writer.RenderBeginTag(HtmlTextWriterTag.Ul);

                // Render Tag Links
                var tags = from cat in this.ContentItem.Terms where cat.VocabularyId == this.TagVocabulary.VocabularyId select cat;

                for (int i = 0; i <= tags.Count() - 1; i++)
                {
                    if (i == 0)
                    {
                        // First Tag
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "first_tag");
                    }
                    else if (i == tags.Count() - 1)
                    {
                        // Last Tag
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "last_tag");
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    this.RenderTerm(writer, tags.ToList()[i], i < tags.Count() - 1 && this.RepeatDirection.Equals("horizontal", StringComparison.OrdinalIgnoreCase));

                    writer.RenderEndTag();
                }

                if (this.AllowTagging)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    if (this.IsEditMode)
                    {
                        writer.Write("&nbsp;&nbsp;");

                        writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
                        writer.AddAttribute("OnKeyPress", "return disableEnterKey(event)");
                        writer.RenderBeginTag(HtmlTextWriterTag.Input);
                        writer.RenderEndTag();

                        writer.Write("&nbsp;&nbsp;");

                        // Render Save Button
                        this.RenderButton(writer, "Save", this.SaveImageUrl);

                        writer.Write("&nbsp;&nbsp;");

                        // Render Add Button
                        this.RenderButton(writer, "Cancel", this.CancelImageUrl);
                    }
                    else
                    {
                        writer.Write("&nbsp;&nbsp;");

                        // Render Add Button
                        this.RenderButton(writer, "Add", this.AddImageUrl);
                    }

                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
            }

            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderControl(writer={Writer})", writer);
        }

        /// <inheritdoc />
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogDebug(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            this.tags = postCollection[postDataKey];

			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): this.tags → {Arg2}", postDataKey, postCollection, this.tags);
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): tags → {Tags}", postDataKey, postCollection, tags);
			Logger.LogDebug("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return true;
        }

        /// <inheritdoc />
        public void RaisePostDataChangedEvent()
        {
		Logger.LogDebug(">>> Entering RaisePostDataChangedEvent()");
		Logger.LogDebug("<<< Exiting RaisePostDataChangedEvent()");
        }

        /// <inheritdoc />
        public void RaisePostBackEvent(string eventArgument)
        {
			Logger.LogDebug(">>> Entering RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
            switch (eventArgument)
            {
                case "Add":
                    this.IsEditMode = true;
					Logger.LogDebug("RaisePostBackEvent(eventArgument={EventArgument}): this.IsEditMode → {Arg1}", eventArgument, this.IsEditMode);
                    break;
                case "Cancel":
                    this.IsEditMode = false;
                    break;
                case "Save":
                    this.SaveTags();
                    break;
                default:
                    this.IsEditMode = false;
                    break;
            }
			Logger.LogDebug("<<< Exiting RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
        }

        /// <summary>A method which triggers the <see cref="TagsUpdated"/> event.</summary>
        /// <param name="e">The event args.</param>
        protected void OnTagsUpdate(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnTagsUpdate(e={E})", e);
            if (this.TagsUpdated != null)
            {
                this.TagsUpdated(this, e);
            }
			Logger.LogDebug("<<< Exiting OnTagsUpdate(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (!this.Page.ClientScript.IsClientScriptBlockRegistered(this.UniqueID))
            {
                StringBuilder sb = new StringBuilder();
				Logger.LogDebug("OnPreRender(e={E}): sb → {Sb}", e, sb);

                sb.Append("<script language='javascript' type='text/javascript' >");
                sb.Append(Environment.NewLine);
                sb.Append("function disableEnterKey(e)");
                sb.Append('{');
                sb.Append("var key;");
                sb.Append("if(window.event)");
                sb.Append("key = window.event.keyCode;");
                sb.Append("else ");
                sb.Append("key = e.which;");
                sb.Append("return (key != 13);");
                sb.Append('}');
                sb.Append("</script>");

                this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), this.UniqueID, sb.ToString());
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private string LocalizeString(string key)
        {
			Logger.LogDebug(">>> Entering LocalizeString(key={Key})", key);
            string localResourceFile = Utilities.GetLocalResourceFile(this);
            string localizedString = null;
			Logger.LogDebug("LocalizeString(key={Key}): localizedString → {LocalizedString}", key, localizedString);
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(localResourceFile))
            {
                localizedString = Localization.GetString(key, localResourceFile);
            }
            else
            {
                localizedString = Null.NullString;
            }

			Logger.LogDebug("<<< Exiting LocalizeString(key={Key})", key);
            return localizedString;
        }

        private void RenderButton(HtmlTextWriter writer, string buttonType, string imageUrl)
        {
			Logger.LogDebug(">>> Entering RenderButton(writer={Writer},buttonType={ButtonType},imageUrl={ImageUrl})", writer, buttonType, imageUrl);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, this.LocalizeString($"{buttonType}.ToolTip"));
            writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ClientScript.GetPostBackClientHyperlink(this, buttonType));
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            // Image
            if (!string.IsNullOrEmpty(imageUrl))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, this.ResolveUrl(imageUrl));
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            writer.Write(this.LocalizeString(buttonType));
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderButton(writer={Writer},buttonType={ButtonType},imageUrl={ImageUrl})", writer, buttonType, imageUrl);
        }

        private void RenderTerm(HtmlTextWriter writer, Term term, bool renderSeparator)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, string.Format(CultureInfo.InvariantCulture, this.NavigateUrlFormatString, term.Name));
            writer.AddAttribute(HtmlTextWriterAttribute.Title, term.Name);
            writer.AddAttribute(HtmlTextWriterAttribute.Rel, "tag");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(term.Name);
            writer.RenderEndTag();

            if (renderSeparator)
            {
                writer.Write(this.Separator);
            }
			Logger.LogDebug("<<< Exiting RenderTerm(writer={Writer},term={Term},renderSeparator={RenderSeparator})", writer, term, renderSeparator);
        }

        private void SaveTags()
        {
			Logger.LogDebug(">>> Entering SaveTags()");
            string tags = this.tags;

            if (!string.IsNullOrEmpty(tags))
            {
                foreach (string t in tags.Split(','))
                {
                    if (!string.IsNullOrEmpty(t))
                    {
                        string tagName = t.Trim(' ');
                        Term existingTerm = (from term in this.ContentItem.Terms.AsQueryable() where term.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase) select term).SingleOrDefault();
						Logger.LogDebug("SaveTags(): existingTerm → {ExistingTerm}", existingTerm);

                        if (existingTerm == null)
                        {
                            // Not tagged
                            Term term = this.termController.GetTermsByVocabulary(this.TagVocabulary.VocabularyId).SingleOrDefault(te => te.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
							Logger.LogDebug("SaveTags(): term → {Term}", term);
                            if (term == null)
                            {
                                // Add term
                                term = new Term(this.TagVocabulary.VocabularyId) { Name = tagName, };
                                this.termController.AddTerm(term);
                            }

                            // Add term to content
                            this.ContentItem.Terms.Add(term);
                            this.termController.AddTermToContent(term, this.ContentItem);
                        }
                    }
                }
            }

            this.IsEditMode = false;
			Logger.LogDebug("SaveTags(): this.IsEditMode → {Arg0}", this.IsEditMode);

            // Raise the Tags Updated Event
            this.OnTagsUpdate(EventArgs.Empty);
			Logger.LogDebug("<<< Exiting SaveTags()");
        }
    }
}
