' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Reflection
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class DNNMultiStateCollection
        Inherits CollectionBase
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal ctl As DNNMultiStateBox)
			_logger.LogDebug(">>> Entering New(ctl={Ctl})", ctl)
			_logger.LogDebug("New(ctl={Ctl}): Me.m_ctl → {Arg1}", ctl, Me.m_ctl)
            Me.m_ctl = ctl
			_logger.LogDebug("<<< Exiting New(ctl={Ctl})", ctl)
        End Sub

        Public Sub Add(ByVal Item As DNNMultiState)
			_logger.LogDebug(">>> Entering Add(Item={Item})", Item)
            MyBase.InnerList.Add(Item)
			_logger.LogDebug("<<< Exiting Add(Item={Item})", Item)
        End Sub

        Public Sub AddAt(ByVal index As Integer, ByVal Item As DNNMultiState)
			_logger.LogDebug(">>> Entering AddAt(index={Index},Item={Item})", index, Item)
            MyBase.InnerList.Insert(index, Item)
			_logger.LogDebug("<<< Exiting AddAt(index={Index},Item={Item})", index, Item)
        End Sub

        Protected Overrides Sub OnInsert(ByVal index As Integer, ByVal value As Object)
			_logger.LogDebug(">>> Entering OnInsert(index={Index},value={Value})", index, value)
            TryCast(value,DNNMultiState).Parent = Me.m_ctl
            Me.m_ctl.NotifyDesigner
			_logger.LogDebug("<<< Exiting OnInsert(index={Index},value={Value})", index, value)
        End Sub


        ' Properties
        Public Default Property Item(ByVal index As Integer) As DNNMultiState
            Get
                Return TryCast(MyBase.InnerList(index),DNNMultiState)
            End Get
            Set(ByVal value As DNNMultiState)
                MyBase.InnerList(index) = value
            End Set
        End Property


        ' Fields
        Private m_ctl As DNNMultiStateBox
    End Class
End Namespace

