' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
Imports System.Diagnostics.CodeAnalysis
Imports Microsoft.Extensions.Logging
Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

Namespace DotNetNuke.UI.Utilities

    <SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification := "Breaking change")>
    Public Interface IClientAPICallbackEventHandler
        Function RaiseClientAPICallbackEvent(ByVal eventArgument As String) As String
		_logger.LogDebug(">>> Entering RaiseClientAPICallbackEvent(eventArgument={EventArgument})", eventArgument)
    End Interface

End Namespace
