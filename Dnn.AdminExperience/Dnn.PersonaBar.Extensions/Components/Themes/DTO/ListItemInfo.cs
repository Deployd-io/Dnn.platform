// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.Components.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

    [DataContract]
    public class ListItemInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ListItemInfo()
        {
		Logger.LogDebug(">>> Entering ListItemInfo()");
		Logger.LogDebug("<<< Exiting ListItemInfo()");
        }

        public ListItemInfo(string text, string value)
        {
			Logger.LogDebug(">>> Entering ListItemInfo(text={Text},value={Value})", text, value);
            this.Text = text;
            this.Value = value;
			Logger.LogDebug("<<< Exiting ListItemInfo(text={Text},value={Value})", text, value);
			Logger.LogDebug("ListItemInfo(text={Text},value={Value}): this.Text → {Arg2}", text, value, this.Text);
			Logger.LogDebug("ListItemInfo(text={Text},value={Value}): this.Value → {Arg2}", text, value, this.Value);
        }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}
