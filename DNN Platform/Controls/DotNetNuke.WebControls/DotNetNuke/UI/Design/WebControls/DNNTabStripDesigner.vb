' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.WebControls
Imports System
Imports System.Web.UI.Design
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNTabStripDesigner
        Inherits ControlDesigner
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(DNNTabStripDesigner))
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
			Logger.LogDebug(">>> Entering GetDesignTimeHtml()")
            Dim designTimeHtml As String
            Dim component As DNNTabStrip = DirectCast(MyBase.Component, DNNTabStrip)
            Dim tabs As TabStripTabCollection = component.Tabs
            Dim forceDownLevel As Boolean = component.ForceDownLevel
            component.ForceDownLevel = True
			Logger.LogDebug("GetDesignTimeHtml(): component.ForceDownLevel → {Arg0}", component.ForceDownLevel)
            If (tabs.Count <> 0) Then
                designTimeHtml = MyBase.GetDesignTimeHtml
				Logger.LogDebug("GetDesignTimeHtml(): designTimeHtml → {DesignTimeHtml}", designTimeHtml)
            Else
                tabs.Add(New DNNTab("Tab 1"))
                tabs.Add(New DNNTab("Tab 2"))
                Dim child As New Panel
                Dim unit2 As New Unit("100px")
                child.Width = unit2
				Logger.LogDebug("GetDesignTimeHtml(): child.Width → {Arg0}", child.Width)
				Logger.LogDebug("GetDesignTimeHtml(): unit2 → {Unit2}", unit2)
				Logger.LogDebug("GetDesignTimeHtml(): child.Height → {Arg0}", child.Height)
                unit2 = New Unit("50px")
                child.Height = unit2
                tabs(0).Controls.Add(child)
                component.SelectedIndex = 0
				Logger.LogDebug("GetDesignTimeHtml(): component.SelectedIndex → {Arg0}", component.SelectedIndex)
                designTimeHtml = MyBase.GetDesignTimeHtml
                tabs.Clear
            End If
            component.ForceDownLevel = forceDownLevel
			Logger.LogDebug("<<< Exiting GetDesignTimeHtml()")
            Return designTimeHtml
        End Function

    End Class
End Namespace

