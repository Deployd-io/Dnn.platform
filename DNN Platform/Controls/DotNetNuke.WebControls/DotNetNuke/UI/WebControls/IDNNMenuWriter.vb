' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Web.UI
Imports Microsoft.Extensions.Logging
Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

Namespace DotNetNuke.UI.WebControls
    Friend Interface IDNNMenuWriter
        ' Methods
        Sub RenderMenu(ByVal writer As HtmlTextWriter, ByVal menu As DNNMenu)
		_logger.LogDebug(">>> Entering RenderMenu(writer={Writer},menu={Menu})", writer, menu)
    End Interface
End Namespace

