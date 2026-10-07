' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTextSuggestEventArgs
        Inherits EventArgs
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal nodes As DNNNodeCollection, ByVal [text] As String)
			_logger.LogError(">>> Entering New(nodes={Nodes})", nodes)
			_logger.LogDebug("New(nodes={Nodes}): Me._text → {Arg1}", nodes, Me._text)
        _logger.LogWarning("<<< Exiting New(nodes={Nodes})", nodes)
            Me._nodes = nodes
			_logger.LogDebug("New(nodes={Nodes}): Me._nodes → {Arg1}", nodes, Me._nodes)
            Me._text = [text]
        End Sub


        ' Properties
        Public ReadOnly Property Nodes As DNNNodeCollection
            Get
                Return Me._nodes
            End Get
        End Property

        Public ReadOnly Property [Text] As String
            Get
                Return Me._text
            End Get
        End Property


        ' Fields
        Private _nodes As DNNNodeCollection
        Private _text As String
    End Class
End Namespace

