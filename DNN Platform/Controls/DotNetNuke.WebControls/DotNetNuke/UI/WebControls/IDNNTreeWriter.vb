' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Web.UI
Imports Microsoft.Extensions.Logging
Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

Namespace DotNetNuke.UI.WebControls
    Friend Interface IDNNTreeWriter
        ' Methods
        Function MarshalledProperties() As Hashtable
		_logger.LogDebug(">>> Entering MarshalledProperties()")
        Sub RenderTree(ByVal writer As HtmlTextWriter, ByVal tree As DnnTree)
		_logger.LogDebug(">>> Entering RenderTree(writer={Writer},tree={Tree})", writer, tree)
    End Interface
End Namespace

