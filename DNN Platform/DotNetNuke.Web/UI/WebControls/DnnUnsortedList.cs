// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>Creates a control that renders its children as a bulleted list.</summary>
    /// <remarks>
    /// Control renders an unordered list HTML control.
    /// Each child control in <see cref="DnnUnsortedList"/> is rendered as a separate list item.
    /// To obtain a control over list item style, add a <see cref="DnnUnsortedListItem" /> to a controls list,
    /// and tune this object appropriately.
    /// </remarks>
    public class DnnUnsortedList : WebControl, INamingContainer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnUnsortedList));
        private UniformControlCollection<DnnUnsortedList, DnnUnsortedListItem> listItems;

        /// <summary>Initializes a new instance of the <see cref="DnnUnsortedList"/> class.</summary>
        public DnnUnsortedList()
            : base(HtmlTextWriterTag.Ul)
        {
		Logger.LogDebug(">>> Entering DnnUnsortedList()");
		Logger.LogDebug("<<< Exiting DnnUnsortedList()");
        }

        /// <summary>Gets the list items.</summary>
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        [MergableProperty(false)]
        public virtual UniformControlCollection<DnnUnsortedList, DnnUnsortedListItem> ListItems
        {
            get
            {
                return this.listItems ?? (this.listItems = new UniformControlCollection<DnnUnsortedList, DnnUnsortedListItem>(this));
            }
        }

        /// <summary>A "macro" that adds a set of controls or control as a single list item (li).  Use ListItems.Add(UnsortedListItem) method.</summary>
        /// <remarks>All controls from the list will be rendered as children of a single list item.</remarks>
        /// <param name="listItemControls">The controls to add to a new list item.</param>
        public void AddListItem(params Control[] listItemControls)
        {
			Logger.LogDebug(">>> Entering AddListItem(listItemControls={ListItemControls})", listItemControls);
            var listItem = new DnnUnsortedListItem();
            listItem.AddControls(listItemControls);
			Logger.LogDebug("AddListItem(listItemControls={ListItemControls}): listItem → {ListItem}", listItemControls, listItem);
            this.ListItems.Add(listItem);
			Logger.LogDebug("<<< Exiting AddListItem(listItemControls={ListItemControls})", listItemControls);
        }

        /// <inheritdoc />
        protected override sealed ControlCollection CreateControlCollection()
        {
			Logger.LogDebug(">>> Entering CreateControlCollection()");
			Logger.LogDebug("<<< Exiting CreateControlCollection()");
            return new TypedControlCollection<DnnUnsortedListItem>(this);
        }

        /// <inheritdoc />
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering AddAttributesToRender(writer={Writer})", writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            }
			Logger.LogDebug("<<< Exiting AddAttributesToRender(writer={Writer})", writer);
        }
    }
}
