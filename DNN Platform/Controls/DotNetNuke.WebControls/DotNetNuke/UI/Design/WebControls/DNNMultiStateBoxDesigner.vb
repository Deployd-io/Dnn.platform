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
    Public Class DNNMultiStateBoxDesigner
        Inherits ControlDesigner
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
			_logger.LogError(">>> Entering GetDesignTimeHtml()")
            Dim str As String
            Dim component As DNNMultiStateBox = DirectCast(Me.Component, DNNMultiStateBox)
			_logger.LogError("GetDesignTimeHtml(): component → {Component}", component)
            If (component.ID.Length <= 0) Then
                str = Nothing
				_logger.LogDebug("GetDesignTimeHtml(): str → {Str}", str)
            Else
                Dim writer As New StringWriter
                Dim textbox As New TextBox() With {
                    .Text = component.Text
                }
                textbox.RenderControl(New HtmlTextWriter(writer))
                str = writer.ToString
            End If
			_logger.LogWarning("<<< Exiting GetDesignTimeHtml()")
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

