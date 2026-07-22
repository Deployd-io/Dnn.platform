' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports System
Imports System.Collections
Imports System.Reflection

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class DNNMultiStateCollection
        Inherits CollectionBase
        ' Methods
        Public Sub New(ByVal ctl As DNNMultiStateBox)
			Logger.LogDebug(">>> Entering New(ctl={Ctl})", ctl)
            Me.m_ctl = ctl
        End Sub
		Logger.LogDebug("<<< Exiting New(ctl={Ctl})", ctl)
		Logger.LogDebug("New(ctl={Ctl}): Me.m_ctl → {Arg1}", ctl, Me.m_ctl)

        Public Sub Add(ByVal Item As DNNMultiState)
			Logger.LogDebug(">>> Entering Add(Item={Item})", Item)
            MyBase.InnerList.Add(Item)
        End Sub
		Logger.LogDebug("<<< Exiting Add(Item={Item})", Item)

        Public Sub AddAt(ByVal index As Integer, ByVal Item As DNNMultiState)
			Logger.LogDebug(">>> Entering AddAt(index={Index},Item={Item})", index, Item)
            MyBase.InnerList.Insert(index, Item)
        End Sub
		Logger.LogDebug("<<< Exiting AddAt(index={Index},Item={Item})", index, Item)

        Protected Overrides Sub OnInsert(ByVal index As Integer, ByVal value As Object)
			Logger.LogDebug(">>> Entering OnInsert(index={Index},value={Value})", index, value)
            TryCast(value,DNNMultiState).Parent = Me.m_ctl
            Me.m_ctl.NotifyDesigner
        End Sub
		Logger.LogDebug("<<< Exiting OnInsert(index={Index},value={Value})", index, value)


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

