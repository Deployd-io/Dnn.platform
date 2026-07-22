' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTreeEventArgs
        Inherits EventArgs
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(DNNTreeEventArgs))
        ' Methods
        Public Sub New(ByVal node As TreeNode)
			Logger.LogDebug(">>> Entering New(node={Node})", node)
            Me._node = node
			Logger.LogDebug("<<< Exiting New(node={Node})", node)
			Logger.LogDebug("New(node={Node}): Me._node → {Arg1}", node, Me._node)
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

