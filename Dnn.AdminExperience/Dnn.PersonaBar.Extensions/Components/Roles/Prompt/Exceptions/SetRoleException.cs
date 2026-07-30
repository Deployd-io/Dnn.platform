// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components.Prompt.Exceptions
{
    using System;

    public class SetRoleException : Exception
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public SetRoleException(string message)
            : base(message)
        {
		Logger.LogDebug(">>> Entering SetRoleException(message={Message})", message);
		Logger.LogDebug("<<< Exiting SetRoleException(message={Message})", message);
        }
    }
}
