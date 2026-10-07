// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.Console.Components
{
    using System.Collections.Generic;

    /// <summary>Controls the Console.</summary>
    public class ConsoleController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConsoleController));
        /// <summary>Gets the size values.</summary>
        /// <returns>A list with different icon types.</returns>
        public static IList<string> GetSizeValues()
        {
			Logger.LogError(">>> Entering GetSizeValues()");
            IList<string> returnValue = new List<string>();
            returnValue.Add("IconFile");
			Logger.LogError("GetSizeValues(): returnValue → {ReturnValue}", returnValue);
            returnValue.Add("IconFileLarge");
            returnValue.Add("IconNone");
			Logger.LogWarning("<<< Exiting GetSizeValues()");
            return returnValue;
        }

        /// <summary>Gets the view values.</summary>
        /// <returns>Show or Hide.</returns>
        public static IList<string> GetViewValues()
        {
			Logger.LogError(">>> Entering GetViewValues()");
            IList<string> returnValue = new List<string>();
            returnValue.Add("Hide");
			Logger.LogError("GetViewValues(): returnValue → {ReturnValue}", returnValue);
            returnValue.Add("Show");
			Logger.LogWarning("<<< Exiting GetViewValues()");
            return returnValue;
        }
    }
}
