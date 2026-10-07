// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common
{
    /// <summary>This class handles basic elements about File Items. Is is a basic Get/Set for Value and Text.</summary>
    public class FileItem
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileItem));
        /// <summary>Initializes a new instance of the <see cref="FileItem" /> class.</summary>
        /// <param name="value">The value.</param>
        /// <param name="text">The text.</param>
        public FileItem(string value, string text)
        {
			Logger.LogError(">>> Entering FileItem(value={Value},text={Text})", value, text);
            this.Value = value;
			Logger.LogDebug("FileItem(value={Value},text={Text}): this.Value → {Arg2}", value, text, this.Value);
        Logger.LogWarning("<<< Exiting FileItem(value={Value},text={Text})", value, text);
            this.Text = text;
        }

        /// <summary>Gets the text.</summary>
        /// <value>The text.</value>
        public string Text { get; private set; }

        /// <summary>Gets the value.</summary>
        /// <value>The value.</value>
        public string Value { get; private set; }
    }
}
