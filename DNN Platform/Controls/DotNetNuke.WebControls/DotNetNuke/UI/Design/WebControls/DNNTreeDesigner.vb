' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Web.UI.Design
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNTreeDesigner
        Inherits ControlDesigner
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Overrides Function GetDesignTimeHtml() As String
			_logger.LogDebug(">>> Entering GetDesignTimeHtml()")
            Dim errorDesignTimeHtml As String = Nothing
            Try 
                errorDesignTimeHtml = "<Div>This is a PlaceHolder.<br>Additional design-time support will be <br>added in future versions</Div>"
				_logger.LogDebug("GetDesignTimeHtml(): errorDesignTimeHtml → {ErrorDesignTimeHtml}", errorDesignTimeHtml)
            Catch exception1 As Exception
				_logger.LogError("Exception in GetDesignTimeHtml(): {Arg0}", exception1.getMessage())
                Dim ex As Exception = exception1
                ProjectData.SetProjectError(ex)
                Dim e As Exception = ex
                errorDesignTimeHtml = Me.GetErrorDesignTimeHtml(e)
                ProjectData.ClearProjectError
            End Try
			_logger.LogDebug("<<< Exiting GetDesignTimeHtml()")
            Return errorDesignTimeHtml
        End Function

    End Class
End Namespace

