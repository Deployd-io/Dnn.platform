' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Web.UI
Imports Microsoft.Extensions.Logging
Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

Namespace DotNetNuke.UI.WebControls
    Friend Interface ITreeNodeWriter
        ' Methods
        Sub RenderNode(ByVal writer As HtmlTextWriter, ByVal Node As TreeNode)
		_logger.LogDebug(">>> Entering RenderNode(writer={Writer},Node={Node})", writer, Node)
    End Interface
End Namespace

