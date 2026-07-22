// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization
{
    using System;
    using System.Web.UI.Design;

    public class LocalizationExpressionBuilderEditor : ExpressionEditor
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider)
        {
			Logger.LogDebug(">>> Entering EvaluateExpression(expression={Expression},parseTimeData={ParseTimeData},propertyType={PropertyType},serviceProvider={ServiceProvider})", expression, parseTimeData, propertyType, serviceProvider);
			Logger.LogDebug("<<< Exiting EvaluateExpression(expression={Expression},parseTimeData={ParseTimeData},propertyType={PropertyType},serviceProvider={ServiceProvider})", expression, parseTimeData, propertyType, serviceProvider);
            return string.Concat("[dnnLoc:", expression, "]");
        }
    }
}
