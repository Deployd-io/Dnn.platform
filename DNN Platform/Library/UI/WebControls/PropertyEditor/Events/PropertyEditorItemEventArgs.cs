// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    /// <summary>
    /// The PropertyEditorItemEventArgs class is a cusom EventArgs class for
    /// handling Event Args.
    /// </summary>
    public class PropertyEditorItemEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEditorItemEventArgs"/> class.
        /// Constructs a new PropertyEditorItemEventArgs.
        /// </summary>
        /// <param name="editor">The editor created.</param>
        public PropertyEditorItemEventArgs(EditorInfo editor)
        {
			Logger.LogDebug(">>> Entering PropertyEditorItemEventArgs(editor={Editor})", editor);
            this.Editor = editor;
			Logger.LogDebug("<<< Exiting PropertyEditorItemEventArgs(editor={Editor})", editor);
			Logger.LogDebug("PropertyEditorItemEventArgs(editor={Editor}): this.Editor → {Arg1}", editor, this.Editor);
        }

        /// <summary>Gets or sets whether the proeprty has changed.</summary>
        /// <value>A String.</value>
        public EditorInfo Editor { get; set; }
    }
}
