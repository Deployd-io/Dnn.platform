' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System

Namespace DotNetNuke.UI.WebControls
    <AttributeUsage(AttributeTargets.Property)>
    Public Class ClientPropertyNameAttribute
        Inherits Attribute
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(ClientPropertyNameAttribute))
        ' Methods
        Public Sub New(ByVal name As String)
			Logger.LogDebug(">>> Entering New(name={Name})", name)
            Me.m_Name = name
			Logger.LogDebug("<<< Exiting New(name={Name})", name)
			Logger.LogDebug("New(name={Name}): Me.m_Name → {Arg1}", name, Me.m_Name)
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

