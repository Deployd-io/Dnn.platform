' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Public Class DNNMenuEventArgs
        Inherits EventArgs
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal node As MenuNode)
			_logger.LogError(">>> Entering New(node={Node})", node)
			_logger.LogDebug("New(node={Node}): Me._node → {Arg1}", node, Me._node)
        _logger.LogWarning("<<< Exiting New(node={Node})", node)
            Me._node = node
        End Sub


        ' Properties
        Public ReadOnly Property Node As MenuNode
            Get
                Return Me._node
            End Get
        End Property


        ' Fields
        Private _node As MenuNode
    End Class
End Namespace

