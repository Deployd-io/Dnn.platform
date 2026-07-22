' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports System

Namespace DotNetNuke.UI.WebControls
    Public Class DNNMenuNodeClickEventArgs
        Inherits EventArgs
        ' Methods
        Public Sub New(ByVal Node As MenuNode)
			Logger.LogDebug(">>> Entering New(Node={Node})", Node)
            Me._node = Node
        End Sub
		Logger.LogDebug("<<< Exiting New(Node={Node})", Node)
		Logger.LogDebug("New(Node={Node}): Me._node → {Arg1}", Node, Me._node)


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

