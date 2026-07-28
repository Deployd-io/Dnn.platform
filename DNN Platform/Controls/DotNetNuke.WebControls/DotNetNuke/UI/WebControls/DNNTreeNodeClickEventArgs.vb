' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTreeNodeClickEventArgs
        Inherits EventArgs
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(DNNTreeNodeClickEventArgs))
        ' Methods
        Public Sub New(ByVal Node As TreeNode)
			Logger.LogDebug(">>> Entering New(Node={Node})", Node)
            Me._node = Node
			Logger.LogDebug("<<< Exiting New(Node={Node})", Node)
			Logger.LogDebug("New(Node={Node}): Me._node → {Arg1}", Node, Me._node)
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

