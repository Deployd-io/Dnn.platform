// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Assets
{
    public class SortProperties
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SortProperties));
        public string Column { get; set; }

        public bool Ascending { get; set; }

        public static SortProperties Parse(string sortExpression)
        {
			Logger.LogError(">>> Entering Parse(sortExpression={SortExpression})", sortExpression);
            var sortProperties = new SortProperties { Column = "ItemName", Ascending = true };

			Logger.LogError("Parse(sortExpression={SortExpression}): sortProperties → {SortProperties}", sortExpression, sortProperties);
            if (!string.IsNullOrEmpty(sortExpression))
            {
                var se = sortExpression.Split(' ');
				Logger.LogError("Parse(sortExpression={SortExpression}): se → {Se}", sortExpression, se);
                if (se.Length == 2)
                {
                    sortProperties.Column = se[0];
					Logger.LogDebug("Parse(sortExpression={SortExpression}): sortProperties.Column → {Arg1}", sortExpression, sortProperties.Column);
                    sortProperties.Ascending = se[1] == "ASC";
                }
            }

			Logger.LogWarning("<<< Exiting Parse(sortExpression={SortExpression})", sortExpression);
            return sortProperties;
        }
    }
}
