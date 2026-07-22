// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.RequestFilter
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Web;

    /// <summary>Represents a rule from <c>RequestFilter.Config</c>.</summary>
    [Serializable]
    public class RequestFilterRule
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="RequestFilterRule"/> class.</summary>
        /// <param name="serverVariable">The server variable name.</param>
        /// <param name="values">The values to filter.</param>
        /// <param name="op">The filter operator.</param>
        /// <param name="action">The filter action.</param>
        /// <param name="location">The redirect location.</param>
        public RequestFilterRule(string serverVariable, string values, RequestFilterOperatorType op, RequestFilterRuleType action, string location)
        {
			Logger.LogDebug(">>> Entering RequestFilterRule(serverVariable={ServerVariable},values={Values},op={Op},action={Action},location={Location})", serverVariable, values, op, action, location);
            this.ServerVariable = serverVariable;
            this.SetValues(values, op);
			Logger.LogDebug("RequestFilterRule(serverVariable={ServerVariable},values={Values},op={Op},action={Action},location={Location}): this.ServerVariable → {Arg5}", serverVariable, values, op, action, location, this.ServerVariable);
            this.Operator = op;
			Logger.LogDebug("RequestFilterRule(serverVariable={ServerVariable},values={Values},op={Op},action={Action},location={Location}): this.Operator → {Arg5}", serverVariable, values, op, action, location, this.Operator);
            this.Action = action;
            this.Location = location;
			Logger.LogDebug("<<< Exiting RequestFilterRule(serverVariable={ServerVariable},values={Values},op={Op},action={Action},location={Location})", serverVariable, values, op, action, location);
			Logger.LogDebug("RequestFilterRule(serverVariable={ServerVariable},values={Values},op={Op},action={Action},location={Location}): this.Action → {Arg5}", serverVariable, values, op, action, location, this.Action);
			Logger.LogDebug("RequestFilterRule(serverVariable={ServerVariable},values={Values},op={Op},action={Action},location={Location}): this.Location → {Arg5}", serverVariable, values, op, action, location, this.Location);
        }

        /// <summary>Initializes a new instance of the <see cref="RequestFilterRule"/> class.</summary>
        public RequestFilterRule()
        {
        }

        /// <summary>Gets the raw value.</summary>
        public string RawValue => string.Join(" ", this.Values.ToArray());

        /// <summary>Gets or sets the server variable.</summary>
        public string ServerVariable { get; set; }

        /// <summary>Gets or sets the values.</summary>
        public List<string> Values { get; set; } = new List<string>();

        /// <summary>Gets or sets the rule action.</summary>
        public RequestFilterRuleType Action { get; set; }

        /// <summary>Gets or sets the rule operator.</summary>
        public RequestFilterOperatorType Operator { get; set; }

        /// <summary>Gets or sets the redirection location.</summary>
        public string Location { get; set; }

        /// <summary>Parses the <paramref name="values"/> and sets <see cref="Values"/>.</summary>
        /// <param name="values">The raw values.</param>
        /// <param name="op">The operator.</param>
        public void SetValues(string values, RequestFilterOperatorType op)
        {
			Logger.LogDebug(">>> Entering SetValues(values={Values},op={Op})", values, op);
            this.Values.Clear();
            if (op != RequestFilterOperatorType.Regex)
            {
                var vals = values.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in vals)
                {
				Logger.LogDebug("SetValues(values={Values},op={Op}): vals → {Vals}", values, op, vals);
                    this.Values.Add(value.ToUpperInvariant());
                }
            }
            else
            {
                this.Values.Add(values);
            }
			Logger.LogDebug("<<< Exiting SetValues(values={Values},op={Op})", values, op);
        }

        /// <summary>Determines whether the <paramref name="serverVariableValue"/> matches this rule.</summary>
        /// <param name="serverVariableValue">The value of the server variable.</param>
        /// <returns><see langword="true"/> if the values matches the rule, otherwise <see langword="false"/>.</returns>
        public bool Matches(string serverVariableValue)
        {
			Logger.LogDebug(">>> Entering Matches(serverVariableValue={ServerVariableValue})", serverVariableValue);
			Logger.LogDebug("<<< Exiting Matches(serverVariableValue={ServerVariableValue})", serverVariableValue);
            return this.Operator switch
            {
                RequestFilterOperatorType.Equal => this.Values.Contains(serverVariableValue.ToUpperInvariant()),
                RequestFilterOperatorType.NotEqual => !this.Values.Contains(serverVariableValue.ToUpperInvariant()),
                RequestFilterOperatorType.Regex => Regex.IsMatch(serverVariableValue, this.Values[0], RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
                _ => false,
            };
        }

        /// <summary>Executes the rule action on the current response.</summary>
        public void Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            HttpResponse response = HttpContext.Current.Response;
            switch (this.Action)
			Logger.LogDebug("Execute(): response → {Response}", response);
            {
                case RequestFilterRuleType.Redirect:
                    response.Redirect(this.Location, true);
                    break;
                case RequestFilterRuleType.PermanentRedirect:
                    response.StatusCode = 301;
					Logger.LogDebug("Execute(): response.StatusCode → {Arg0}", response.StatusCode);
					Logger.LogDebug("Execute(): response.Status → {Arg0}", response.Status);
					Logger.LogDebug("Execute(): response.RedirectLocation → {Arg0}", response.RedirectLocation);
                    response.Status = "301 Moved Permanently";
                    response.RedirectLocation = this.Location;
                    response.End();
                    break;
                case RequestFilterRuleType.NotFound:
                    response.StatusCode = 404;
					Logger.LogDebug("Execute(): response.SuppressContent → {Arg0}", response.SuppressContent);
                    response.SuppressContent = true;
                    response.End();
                    break;
            }
			Logger.LogDebug("<<< Exiting Execute()");
        }
    }
}
