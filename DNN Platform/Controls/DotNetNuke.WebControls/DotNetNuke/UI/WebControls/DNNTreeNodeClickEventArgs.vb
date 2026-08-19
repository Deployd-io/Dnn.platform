' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTreeNodeClickEventArgs
        Inherits EventArgs
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal Node As TreeNode)
			_logger.LogDebug(">>> Entering New(Node={Node})", Node)
			_logger.LogDebug("New(Node={Node}): Me._node → {Arg1}", Node, Me._node)
            Me._node = Node
			_logger.LogDebug("<<< Exiting New(Node={Node})", Node)
        End Sub


        ' Properties
        Public ReadOnly Property Node As TreeNode
            Get
                Return Me._node
            End Get
        End Property


        ' Fields
        Private _node As TreeNode
    End Class
End Namespace

