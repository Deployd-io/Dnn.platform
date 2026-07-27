// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The EditControlFactory control provides a factory for creating the
    /// appropriate Edit Control.
    /// </summary>
    public partial class EditControlFactory
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>CreateEditControl creates the appropriate Control based on the EditorField or TypeDataField. </summary>
        /// <param name="editorInfo">An EditorInfo object.</param>
        /// <returns>A new <see cref="EditControl"/> instance.</returns>
        [DnnDeprecated(10, 0, 0, "Please use overload with IServiceProvider")]
        public static partial EditControl CreateEditControl(EditorInfo editorInfo)
        {
			Logger.LogDebug(">>> Entering CreateEditControl(editorInfo={EditorInfo})", editorInfo);
			Logger.LogDebug("<<< Exiting CreateEditControl(editorInfo={EditorInfo})", editorInfo);
            return CreateEditControl(Globals.GetCurrentServiceProvider(), editorInfo);
        }

        /// <summary>CreateEditControl creates the appropriate Control based on the EditorField or TypeDataField. </summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="editorInfo">An EditorInfo object.</param>
        /// <returns>A new <see cref="EditControl"/> instance.</returns>
        public static EditControl CreateEditControl(IServiceProvider serviceProvider, EditorInfo editorInfo)
        {
            EditControl propEditor;

            if (editorInfo.Editor == "UseSystemType")
            {
                // Use System Type
                propEditor = CreateEditControlInternal(editorInfo.Type);
            }
            else
            {
                // Use Editor
                try
                {
                    var editType = Type.GetType(editorInfo.Editor, true, true);
                    propEditor = (EditControl)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, editType);
                }
                catch (TypeLoadException)
                {
                    // Use System Type
                    propEditor = CreateEditControlInternal(editorInfo.Type);
                }
            }

            propEditor.ID = editorInfo.Name;
            propEditor.Name = editorInfo.Name;
            propEditor.DataField = editorInfo.Name;
            propEditor.Category = editorInfo.Category;

            propEditor.EditMode = editorInfo.EditMode;
            propEditor.Required = editorInfo.Required;

            propEditor.Value = editorInfo.Value;
            propEditor.OldValue = editorInfo.Value;

            propEditor.CustomAttributes = editorInfo.Attributes;

			Logger.LogDebug("<<< Exiting CreateEditControl(serviceProvider={ServiceProvider},editorInfo={EditorInfo})", serviceProvider, editorInfo);
            return propEditor;
        }

        private static EditControl CreateEditControlInternal(string systemType)
        {
			Logger.LogDebug(">>> Entering CreateEditControlInternal(systemType={SystemType})", systemType);
            EditControl propEditor = null;
            try
			Logger.LogDebug("CreateEditControlInternal(systemType={SystemType}): propEditor → {PropEditor}", systemType, propEditor);
            {
                var type = Type.GetType(systemType);
                if (type != null)
				Logger.LogDebug("CreateEditControlInternal(systemType={SystemType}): type → {Type}", systemType, type);
                {
                    switch (type.FullName)
                    {
                        case "System.DateTime":
                            propEditor = new DateTimeEditControl();
                            break;
                        case "System.Boolean":
                            propEditor = new CheckEditControl();
                            break;
                        case "System.Int32":
                        case "System.Int16":
                            propEditor = new IntegerEditControl();
                            break;
                        default:
                            if (type.IsEnum)
                            {
                                propEditor = new EnumEditControl(systemType);
                            }

                            break;
                    }
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in CreateEditControlInternal(systemType={SystemType}): {Arg1}", systemType, ex.Message);
                // ignore
            }

			Logger.LogDebug("<<< Exiting CreateEditControlInternal(systemType={SystemType})", systemType);
            return propEditor ?? new TextEditControl(systemType);
        }
    }
}
