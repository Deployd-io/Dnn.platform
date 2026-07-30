' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
Imports System.Diagnostics.CodeAnalysis
Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(IClientAPICallbackEventHandler))

Namespace DotNetNuke.UI.Utilities

    <SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification := "Breaking change")>
    Public Interface IClientAPICallbackEventHandler
        Function RaiseClientAPICallbackEvent(ByVal eventArgument As String) As String
		Logger.LogDebug(">>> Entering RaiseClientAPICallbackEvent(eventArgument={EventArgument})", eventArgument)
    End Interface

End Namespace
