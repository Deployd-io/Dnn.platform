// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Common
{
    using System.Web.Routing;

    public static class TypeHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Given an object of anonymous type, add each property as a key and associated with its value to a dictionary.
        ///
        /// This helper will cache accessors and types, and is intended when the anonymous object is accessed multiple
        /// times throughout the lifetime of the web application.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>The route value dictionary containing each property from <paramref name="value"/>.</returns>
        public static RouteValueDictionary ObjectToDictionary(object value)
        {
			Logger.LogDebug(">>> Entering ObjectToDictionary(value={Value})", value);
            RouteValueDictionary dictionary = new RouteValueDictionary();

			Logger.LogDebug("ObjectToDictionary(value={Value}): dictionary → {Dictionary}", value, dictionary);
            if (value != null)
            {
                foreach (PropertyHelper helper in PropertyHelper.GetProperties(value))
                {
                    dictionary.Add(helper.Name, helper.GetValue(value));
                }
            }

			Logger.LogDebug("<<< Exiting ObjectToDictionary(value={Value})", value);
            return dictionary;
        }
    }
}
