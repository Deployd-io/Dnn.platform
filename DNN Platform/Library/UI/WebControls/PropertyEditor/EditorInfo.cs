// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The EditorInfo class provides a helper class for the Property Editor.</summary>
    public partial class EditorInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EditorInfo));
        /// <summary>Initializes a new instance of the <see cref="EditorInfo"/> class.</summary>
        public EditorInfo()
        {
			Logger.LogError(">>> Entering EditorInfo()");
            this.Visible = true;
        }

        public object[] Attributes { get; set; }

        public string Category { get; set; }

        public Style ControlStyle { get; set; }

        public PropertyEditorMode EditMode { get; set; }

        public string Editor { get; set; }

        public LabelMode LabelMode { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }

        public string ResourceKey { get; set; }

        public string Type { get; set; }

        public UserInfo User { get; set; }

        public object Value { get; set; }

        public string ValidationExpression { get; set; }

        public bool Visible { get; set; }

        public ProfileVisibility ProfileVisibility { get; set; }

        /// <summary>GetEditor gets the appropriate Editor based on ID properties.</summary>
        /// <param name="editorType">The ID of the Editor.</param>
        /// <returns>The non-localized name of the editor.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with ListController")]
        public static partial string GetEditor(int editorType)
            => GetEditor(Globals.GetCurrentServiceProvider().GetRequiredService<ListController>(), editorType);

        /// <summary>GetEditor gets the appropriate Editor based on ID properties.</summary>
        /// <param name="listController">The list controller.</param>
        /// <param name="editorType">The ID of the Editor.</param>
        /// <returns>The non-localized name of the editor.</returns>
        public static string GetEditor(ListController listController, int editorType)
        {
			Logger.LogError(">>> Entering GetEditor(editorType={EditorType})", editorType);
            string editor = "UseSystemType";
			Logger.LogError(">>> Entering GetEditor(listController={ListController},editorType={EditorType})", listController, editorType);
            if (editorType != Null.NullInteger)
            {
                ListEntryInfo definitionEntry = listController.GetListEntryInfo("DataType", editorType);
				Logger.LogError("GetEditor(editorType={EditorType}): definitionEntry → {DefinitionEntry}", editorType, definitionEntry);
                if (definitionEntry != null)
                {
					Logger.LogError("GetEditor(listController={ListController},editorType={EditorType}): definitionEntry → {DefinitionEntry}", listController, editorType, definitionEntry);
                    editor = definitionEntry.TextNonLocalized;
					Logger.LogError("GetEditor(editorType={EditorType}): editor → {Editor}", editorType, editor);
                }
            }

			Logger.LogWarning("<<< Exiting GetEditor(listController={ListController},editorType={EditorType})", listController, editorType);
            return editor;
        }

        /// <summary>GetEditor gets the appropriate Editor based on ID properties.</summary>
        /// <param name="editorValue">The value of the editor list item.</param>
        /// <returns>The non-localized name of the editor.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with ListController")]
        public static partial string GetEditor(string editorValue)
            => GetEditor(Globals.GetCurrentServiceProvider().GetRequiredService<ListController>(), editorValue);

        /// <summary>GetEditor gets the appropriate Editor based on ID properties.</summary>
        /// <param name="listController">The list controller.</param>
        /// <param name="editorValue">The value of the editor list item.</param>
        /// <returns>The non-localized name of the editor.</returns>
        public static string GetEditor(ListController listController, string editorValue)
        {
            string editor = "UseSystemType";
            ListEntryInfo definitionEntry = listController.GetListEntryInfo("DataType", editorValue);
            if (definitionEntry != null)
            {
                editor = definitionEntry.TextNonLocalized;
            }

			Logger.LogError("GetEditor(editorValue={EditorValue}): editor → {Editor}", editorValue, editor);
        Logger.LogWarning("<<< Exiting GetEditor(listController={ListController},editorValue={EditorValue})", listController, editorValue);
            return editor;
        }
    }
}
