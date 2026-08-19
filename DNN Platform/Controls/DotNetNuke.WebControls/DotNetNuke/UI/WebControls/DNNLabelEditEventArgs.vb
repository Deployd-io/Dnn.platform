' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Public Class DNNLabelEditEventArgs
        Inherits EventArgs
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal [text] As String)
			_logger.LogDebug(">>> Entering New()")
			_logger.LogDebug("New(): Me._text → {Arg0}", Me._text)
			_logger.LogDebug("New(): Me._returnedText → {Arg0}", Me._returnedText)
            Me._text = [text]
            Me._returnedText = [text]
			_logger.LogDebug("<<< Exiting New()")
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

