' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Web.UI.Design

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNTreeDesigner
        Inherits ControlDesigner
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
			Logger.LogDebug(">>> Entering GetDesignTimeHtml()")
            Dim errorDesignTimeHtml As String = Nothing
            Try 
                errorDesignTimeHtml = "<Div>This is a PlaceHolder.<br>Additional design-time support will be <br>added in future versions</Div>"
				Logger.LogDebug("GetDesignTimeHtml(): errorDesignTimeHtml → {ErrorDesignTimeHtml}", errorDesignTimeHtml)
            Catch exception1 As Exception
                Dim ex As Exception = exception1
				Logger.LogError("Exception in GetDesignTimeHtml(): {Arg0}", ex.Message)
                ProjectData.SetProjectError(ex)
                Dim e As Exception = ex
                errorDesignTimeHtml = Me.GetErrorDesignTimeHtml(e)
                ProjectData.ClearProjectError
            End Try
            Return errorDesignTimeHtml
        End Function
		Logger.LogDebug("<<< Exiting GetDesignTimeHtml()")

    End Class
End Namespace

