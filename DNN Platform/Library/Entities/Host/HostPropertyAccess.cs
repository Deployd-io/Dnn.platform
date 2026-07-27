// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    public class HostPropertyAccess : DictionaryPropertyAccess
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="HostPropertyAccess"/> class.</summary>
        public HostPropertyAccess()
            : base(HostController.Instance.GetSettingsDictionary())
        {
		Logger.LogDebug(">>> Entering HostPropertyAccess()");
		Logger.LogDebug("<<< Exiting HostPropertyAccess()");
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
			Logger.LogDebug(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
            if (propertyName.Equals("hosttitle", System.StringComparison.OrdinalIgnoreCase) || currentScope == Scope.Debug)
            {
                return base.GetProperty(propertyName, format, formatProvider, accessingUser, currentScope, ref propertyNotFound);
            }
            else
            {
                propertyNotFound = true;
				Logger.LogDebug("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},currentScope={CurrentScope},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, currentScope, propertyNotFound);
                return PropertyAccess.ContentLocked;
            }
        }
    }
}
