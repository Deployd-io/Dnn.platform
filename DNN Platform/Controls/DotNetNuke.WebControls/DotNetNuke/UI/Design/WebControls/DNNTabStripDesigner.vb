' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.WebControls
Imports System
Imports System.Web.UI.Design
Imports System.Web.UI.WebControls
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNTabStripDesigner
        Inherits ControlDesigner
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
			_logger.LogDebug(">>> Entering GetDesignTimeHtml()")
            Dim designTimeHtml As String
            Dim component As DNNTabStrip = DirectCast(MyBase.Component, DNNTabStrip)
			_logger.LogDebug("GetDesignTimeHtml(): component → {Component}", component)
            Dim tabs As TabStripTabCollection = component.Tabs
            Dim forceDownLevel As Boolean = component.ForceDownLevel
            component.ForceDownLevel = True
			_logger.LogDebug("GetDesignTimeHtml(): component.ForceDownLevel → {Arg0}", component.ForceDownLevel)
            If (tabs.Count <> 0) Then
                designTimeHtml = MyBase.GetDesignTimeHtml
				_logger.LogDebug("GetDesignTimeHtml(): designTimeHtml → {DesignTimeHtml}", designTimeHtml)
            Else
                tabs.Add(New DNNTab("Tab 1"))
                tabs.Add(New DNNTab("Tab 2"))
                Dim child As New Panel
                Dim unit2 As New Unit("100px")
                child.Width = unit2
				_logger.LogDebug("GetDesignTimeHtml(): child.Width → {Arg0}", child.Width)
				_logger.LogDebug("GetDesignTimeHtml(): unit2 → {Unit2}", unit2)
				_logger.LogDebug("GetDesignTimeHtml(): child.Height → {Arg0}", child.Height)
                unit2 = New Unit("50px")
                child.Height = unit2
                tabs(0).Controls.Add(child)
                component.SelectedIndex = 0
				_logger.LogDebug("GetDesignTimeHtml(): component.SelectedIndex → {Arg0}", component.SelectedIndex)
                designTimeHtml = MyBase.GetDesignTimeHtml
                tabs.Clear
            End If
            component.ForceDownLevel = forceDownLevel
			_logger.LogDebug("<<< Exiting GetDesignTimeHtml()")
            Return designTimeHtml
        End Function

    End Class
End Namespace

