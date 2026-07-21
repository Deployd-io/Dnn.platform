// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal.Components
{
    using DotNetNuke.Services.Journal;

    public class JournalItemTokenReplace : Services.Tokens.BaseCustomTokenReplace
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public JournalItemTokenReplace(JournalItem journalItem, JournalControl journalControl)
        {
			Logger.LogDebug(">>> Entering JournalItemTokenReplace(journalItem={JournalItem},journalControl={JournalControl})", journalItem, journalControl);
            this.AddPropertySource("journalitem", journalItem);
            this.AddRawPropertySource("journalcontrol", journalControl);
            if (journalItem.ItemData != null)
            {
                this.AddPropertySource("journaldata", journalItem.ItemData);
            }

            if (journalItem.JournalAuthor != null)
            {
                this.AddPropertySource("journalauthor", journalItem.JournalAuthor);
                this.AddPropertySource("journalprofile", new ProfilePicPropertyAccess(journalItem.JournalAuthor.Id));
            }
			Logger.LogDebug("<<< Exiting JournalItemTokenReplace(journalItem={JournalItem},journalControl={JournalControl})", journalItem, journalControl);
        }

        public string ReplaceJournalItemTokens(string source)
        {
			Logger.LogDebug(">>> Entering ReplaceJournalItemTokens(source={Source})", source);
			Logger.LogDebug("<<< Exiting ReplaceJournalItemTokens(source={Source})", source);
            return this.ReplaceTokens(source);
        }
    }
}
