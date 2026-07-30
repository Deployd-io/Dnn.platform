' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System

Namespace DotNetNuke.UI.WebControls
    Public Class DNNLabelEditEventArgs
        Inherits EventArgs
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(DNNLabelEditEventArgs))
        ' Methods
        Public Sub New(ByVal [text] As String)
			Logger.LogDebug(">>> Entering New([text]={Arg})", [text])
            Me._text = [text]
            Me._returnedText = [text]
			Logger.LogDebug("New([text]={Arg0}): Me._returnedText → {Arg1}", [text], Me._returnedText)
			Logger.LogDebug("New([text]={Arg0}): Me._text → {Arg1}", [text], Me._text)
			Logger.LogDebug("<<< Exiting New([text]={Arg})", [text])
        End Sub


        ' Properties
        Public ReadOnly Property [Text] As String
            Get
                Return Me._text
            End Get
        End Property

        Public Property ReturnedText As String
            Get
                Return Me._returnedText
            End Get
            Set(ByVal value As String)
                Me._returnedText = value
            End Set
        End Property


        ' Fields
        Private _text As String
        Private _returnedText As String
    End Class
End Namespace

