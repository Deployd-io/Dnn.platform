// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ListAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly string listName;
        private readonly string parentKey;
        private readonly ListBoundField textField;
        private readonly ListBoundField valueField;

        /// <summary>Initializes a new instance of the <see cref="ListAttribute"/> class.</summary>
        /// <param name="listName">The name of the List to use for this property.</param>
        /// <param name="parentKey">The key of the parent for this List.</param>
        /// <param name="textField">Text Field.</param>
        /// <param name="valueField">Value Field.</param>
        public ListAttribute(string listName, string parentKey, ListBoundField valueField, ListBoundField textField)
        {
			Logger.LogDebug(">>> Entering ListAttribute(listName={ListName},parentKey={ParentKey},valueField={ValueField},textField={TextField})", listName, parentKey, valueField, textField);
            this.listName = listName;
            this.parentKey = parentKey;
			Logger.LogDebug("ListAttribute(listName={ListName},parentKey={ParentKey},valueField={ValueField},textField={TextField}): this.listName → {Arg4}", listName, parentKey, valueField, textField, this.listName);
			Logger.LogDebug("ListAttribute(listName={ListName},parentKey={ParentKey},valueField={ValueField},textField={TextField}): this.parentKey → {Arg4}", listName, parentKey, valueField, textField, this.parentKey);
			Logger.LogDebug("ListAttribute(listName={ListName},parentKey={ParentKey},valueField={ValueField},textField={TextField}): valueField → {ValueField1}", listName, parentKey, valueField, textField, valueField);
            this.textField = textField;
            this.valueField = valueField;
			Logger.LogDebug("<<< Exiting ListAttribute(listName={ListName},parentKey={ParentKey},valueField={ValueField},textField={TextField})", listName, parentKey, valueField, textField);
			Logger.LogDebug("ListAttribute(listName={ListName},parentKey={ParentKey},valueField={ValueField},textField={TextField}): textField → {TextField1}", listName, parentKey, valueField, textField, textField);
			Logger.LogDebug("ListAttribute(listName={ListName},parentKey={ParentKey},valueField={ValueField},textField={TextField}): this.textField → {Arg4}", listName, parentKey, valueField, textField, this.textField);
			Logger.LogDebug("ListAttribute(listName={ListName},parentKey={ParentKey},valueField={ValueField},textField={TextField}): this.valueField → {Arg4}", listName, parentKey, valueField, textField, this.valueField);
        }

        public string ListName
        {
            get
            {
                return this.listName;
            }
        }

        public string ParentKey
        {
            get
            {
                return this.parentKey;
            }
        }

        public ListBoundField TextField
        {
            get
            {
                return this.textField;
            }
        }

        public ListBoundField ValueField
        {
            get
            {
                return this.valueField;
            }
        }
    }
}
