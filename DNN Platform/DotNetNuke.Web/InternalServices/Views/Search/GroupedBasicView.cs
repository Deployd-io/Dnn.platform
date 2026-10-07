// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices.Views.Search
{
    using System.Collections.Generic;

    /// <summary>BasicView grouped by DocumentTypeName.</summary>
    public class GroupedBasicView
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GroupedBasicView));
        /// <summary>Initializes a new instance of the <see cref="GroupedBasicView"/> class.</summary>
        public GroupedBasicView()
        {
		Logger.LogError(">>> Entering GroupedBasicView()");
        }

        /// <summary>Initializes a new instance of the <see cref="GroupedBasicView"/> class.</summary>
        /// <param name="basic">The basic view.</param>
        public GroupedBasicView(BasicView basic)
        {
			Logger.LogError(">>> Entering GroupedBasicView(basic={Basic})", basic);
            this.DocumentTypeName = basic.DocumentTypeName;
			Logger.LogDebug("GroupedBasicView(basic={Basic}): this.DocumentTypeName → {Arg1}", basic, this.DocumentTypeName);
            this.Results = new List<BasicView>
            {
                new BasicView
                {
					Logger.LogDebug("GroupedBasicView(basic={Basic}): this.Results → {Arg1}", basic, this.Results);
                    Title = basic.Title,
                    Snippet = basic.Snippet,
                    Description = basic.Description,
                    DocumentUrl = basic.DocumentUrl,
                    Attributes = basic.Attributes,
                },
            };
        }

        /// <summary>Gets or sets type of Search Document.</summary>
        public string DocumentTypeName { get; set; }

        /// <summary>Gets or sets results of the Search.</summary>
        public List<BasicView> Results { get; set; }
    }
}
