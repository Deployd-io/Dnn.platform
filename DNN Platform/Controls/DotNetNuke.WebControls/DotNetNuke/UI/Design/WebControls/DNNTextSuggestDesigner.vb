' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.WebControls
Imports System
Imports System.IO
Imports System.Web.UI
Imports System.Web.UI.Design
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNTextSuggestDesigner
        Inherits ControlDesigner
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(DNNTextSuggestDesigner))
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
			Logger.LogDebug(">>> Entering GetDesignTimeHtml()")
            Dim str As String
            Dim component As DNNTextSuggest = DirectCast(Me.Component, DNNTextSuggest)
            If (component.ID.Length <= 0) Then
                str = Nothing
				Logger.LogDebug("GetDesignTimeHtml(): str → {Str}", str)
            Else
                Dim writer As New StringWriter
                Dim textbox As New TextBox() With {
                    .Text = component.Text
                }
                textbox.RenderControl(New HtmlTextWriter(writer))
                str = writer.ToString
            End If
			Logger.LogDebug("<<< Exiting GetDesignTimeHtml()")
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

