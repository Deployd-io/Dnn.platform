' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.WebControls
Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.IO
Imports System.Web.UI
Imports System.Web.UI.Design
Imports System.Web.UI.WebControls
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNToolBarDesigner
        Inherits ControlDesigner
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
			_logger.LogError(">>> Entering GetDesignTimeHtml()")
            Dim enumerator As IEnumerator = Nothing
            Dim component As DNNToolBar = DirectCast(MyBase.Component, DNNToolBar)
			_logger.LogError("GetDesignTimeHtml(): component → {Component}", component)
            Dim writer As New StringWriter
            Dim writer2 As New HtmlTextWriter(writer)
            Dim label2 As New Label
            If (Strings.Len(component.CssClass) > 0) Then
                label2.CssClass = component.CssClass
				_logger.LogDebug("GetDesignTimeHtml(): label2.CssClass → {Arg0}", label2.CssClass)
            End If
            Try 
                enumerator = component.Buttons.GetEnumerator
				_logger.LogDebug("GetDesignTimeHtml(): enumerator → {Enumerator}", enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNToolBarButton = DirectCast(enumerator.Current, DNNToolBarButton)
					_logger.LogError("GetDesignTimeHtml(): current → {Current}", current)
                    Dim child As New Label
                    If (Strings.Len(current.CssClass) > 0) Then
                        child.CssClass = current.CssClass
						_logger.LogDebug("GetDesignTimeHtml(): child.CssClass → {Arg0}", child.CssClass)
                    End If
                    If (Strings.Len(current.Text) > 0) Then
                        child.Text = current.Text
						_logger.LogDebug("GetDesignTimeHtml(): child.Text → {Arg0}", child.Text)
                    End If
                    label2.Controls.Add(child)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            label2.Style.Add("position", "")
            label2.Style.Add("top", "0px")
            label2.Style.Add("left", "0px")
            label2.RenderControl(writer2)
			_logger.LogWarning("<<< Exiting GetDesignTimeHtml()")
            Return writer.ToString
        End Function

    End Class
End Namespace

