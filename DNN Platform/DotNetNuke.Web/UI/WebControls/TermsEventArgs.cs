// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;

    using DotNetNuke.Entities.Content.Taxonomy;

    /// <summary>Information about an event related to a <see cref="Term"/> being selected.</summary>
    public class TermsEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TermsEventArgs));
        private readonly Term selectedTerm;

        /// <summary>Initializes a new instance of the <see cref="TermsEventArgs"/> class.</summary>
        /// <param name="selectedTerm">The selected term.</param>
        public TermsEventArgs(Term selectedTerm)
        {
			Logger.LogError(">>> Entering TermsEventArgs(selectedTerm={SelectedTerm})", selectedTerm);
            this.selectedTerm = selectedTerm;
			Logger.LogDebug("TermsEventArgs(selectedTerm={SelectedTerm}): selectedTerm → {SelectedTerm1}", selectedTerm, selectedTerm);
        Logger.LogDebug("TermsEventArgs(selectedTerm={SelectedTerm}): this.selectedTerm → {Arg1}", selectedTerm, this.selectedTerm);
        Logger.LogWarning("<<< Exiting TermsEventArgs(selectedTerm={SelectedTerm})", selectedTerm);
        }

        /// <summary>Gets the selected term.</summary>
        public Term SelectedTerm
        {
            get
            {
                return this.selectedTerm;
            }
        }
    }
}
