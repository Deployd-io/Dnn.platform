' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports System

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTextSuggestEventArgs
        Inherits EventArgs
        ' Methods
        Public Sub New(ByVal nodes As DNNNodeCollection, ByVal [text] As String)
			Logger.LogDebug(">>> Entering New(nodes={Nodes},[text]={Arg})", nodes, [text])
            Me._nodes = nodes
            Me._text = [text]
			Logger.LogDebug("New(nodes={Nodes},[text]={Arg1}): Me._text → {Arg2}", nodes, [text], Me._text)
			Logger.LogDebug("New(nodes={Nodes},[text]={Arg1}): Me._nodes → {Arg2}", nodes, [text], Me._nodes)
        End Sub
		Logger.LogDebug("<<< Exiting New(nodes={Nodes},[text]={Arg})", nodes, [text])


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

