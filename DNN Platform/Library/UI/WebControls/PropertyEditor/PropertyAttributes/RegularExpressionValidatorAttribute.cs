// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RegularExpressionValidatorAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RegularExpressionValidatorAttribute));
        private readonly string expression;

        /// <summary>Initializes a new instance of the <see cref="RegularExpressionValidatorAttribute"/> class.</summary>
        /// <param name="expression">The regular expression pattern.</param>
        public RegularExpressionValidatorAttribute(string expression)
        {
			Logger.LogDebug(">>> Entering RegularExpressionValidatorAttribute(expression={Expression})", expression);
            this.expression = expression;
			Logger.LogDebug("RegularExpressionValidatorAttribute(expression={Expression}): this.expression → {Arg1}", expression, this.expression);
			Logger.LogDebug("<<< Exiting RegularExpressionValidatorAttribute(expression={Expression})", expression);
        }

        public string Expression
        {
            get
            {
                return this.expression;
            }
        }
    }
}
