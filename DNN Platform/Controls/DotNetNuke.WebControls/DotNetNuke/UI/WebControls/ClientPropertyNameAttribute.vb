' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    <AttributeUsage(AttributeTargets.Property)>
    Public Class ClientPropertyNameAttribute
        Inherits Attribute
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal name As String)
			_logger.LogDebug(">>> Entering New(name={Name})", name)
			_logger.LogDebug("New(name={Name}): Me.m_Name → {Arg1}", name, Me.m_Name)
            Me.m_Name = name
			_logger.LogDebug("<<< Exiting New(name={Name})", name)
        End Sub


        ' Properties
        Public ReadOnly Property Name As String
            Get
                Return Me.m_Name
            End Get
        End Property


        ' Fields
        Private m_Name As String = ""
    End Class
End Namespace

