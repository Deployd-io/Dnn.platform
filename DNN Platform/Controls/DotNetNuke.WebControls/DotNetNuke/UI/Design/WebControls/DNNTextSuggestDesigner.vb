' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.WebControls
Imports System
Imports System.IO
Imports System.Web.UI
Imports System.Web.UI.Design
Imports System.Web.UI.WebControls
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNTextSuggestDesigner
        Inherits ControlDesigner
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
            Dim str As String
            Dim component As DNNTextSuggest = DirectCast(Me.Component, DNNTextSuggest)
            If (component.ID.Length <= 0) Then
                str = Nothing
            Else
                Dim writer As New StringWriter
                Dim textbox As New TextBox() With {
                    .Text = component.Text
					_logger.LogDebug(">>> Entering GetDesignTimeHtml()")
					_logger.LogDebug("GetDesignTimeHtml(): component → {Component}", component)
					_logger.LogDebug("<<< Exiting GetDesignTimeHtml()")
					_logger.LogDebug("GetDesignTimeHtml(): str → {Str}", str)
                }
                textbox.RenderControl(New HtmlTextWriter(writer))
                str = writer.ToString
            End If
            Return str
        End Function


        ' Properties
        Public Overrides ReadOnly Property AllowResize As Boolean
            Get
                Return False
            End Get
        End Property

    End Class
End Namespace

