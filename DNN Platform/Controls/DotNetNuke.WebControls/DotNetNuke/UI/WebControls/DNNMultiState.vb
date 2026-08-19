' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.ComponentModel
Imports System.Web.UI
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    <PersistChildren(True), ParseChildren(False)> _
    Public Class DNNMultiState
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New()
			_logger.LogDebug(">>> Entering New()")
			_logger.LogDebug("New(): Me.m_Key → {Arg0}", Me.m_Key)
			_logger.LogDebug("New(): Me.m_Text → {Arg0}", Me.m_Text)
			_logger.LogDebug("New(): Me.m_ImageUrl → {Arg0}", Me.m_ImageUrl)
			_logger.LogDebug("New(): Me.m_DisabledImageUrl → {Arg0}", Me.m_DisabledImageUrl)
			_logger.LogDebug("New(): Me.m_ToolTip → {Arg0}", Me.m_ToolTip)
            Me.m_Key = ""
            Me.m_Text = ""
            Me.m_ImageUrl = ""
            Me.m_DisabledImageUrl = ""
            Me.m_ToolTip = ""
			_logger.LogDebug("<<< Exiting New()")
			_logger.LogDebug(">>> Entering New(Key={Key},ImageUrl={ImageUrl},DisabledImageUrl={DisabledImageUrl},ToolTip={ToolTip})", Key, ImageUrl, DisabledImageUrl, ToolTip)
			_logger.LogDebug("New(Key={Key},ImageUrl={ImageUrl},DisabledImageUrl={DisabledImageUrl},ToolTip={ToolTip}): Me.m_Key → {Arg4}", Key, ImageUrl, DisabledImageUrl, ToolTip, Me.m_Key)
			_logger.LogDebug("New(Key={Key},ImageUrl={ImageUrl},DisabledImageUrl={DisabledImageUrl},ToolTip={ToolTip}): Me.m_Text → {Arg4}", Key, ImageUrl, DisabledImageUrl, ToolTip, Me.m_Text)
			_logger.LogDebug("New(Key={Key},ImageUrl={ImageUrl},DisabledImageUrl={DisabledImageUrl},ToolTip={ToolTip}): Me.m_ImageUrl → {Arg4}", Key, ImageUrl, DisabledImageUrl, ToolTip, Me.m_ImageUrl)
			_logger.LogDebug("New(Key={Key},ImageUrl={ImageUrl},DisabledImageUrl={DisabledImageUrl},ToolTip={ToolTip}): Me.m_DisabledImageUrl → {Arg4}", Key, ImageUrl, DisabledImageUrl, ToolTip, Me.m_DisabledImageUrl)
			_logger.LogDebug("New(Key={Key},ImageUrl={ImageUrl},DisabledImageUrl={DisabledImageUrl},ToolTip={ToolTip}): Me.m_ToolTip → {Arg4}", Key, ImageUrl, DisabledImageUrl, ToolTip, Me.m_ToolTip)
			_logger.LogDebug("<<< Exiting New(Key={Key},ImageUrl={ImageUrl},DisabledImageUrl={DisabledImageUrl},ToolTip={ToolTip})", Key, ImageUrl, DisabledImageUrl, ToolTip)
        End Sub

        Public Sub New(ByVal Key As String, ByVal [Text] As String, ByVal ImageUrl As String, ByVal DisabledImageUrl As String, ByVal ToolTip As String)
            Me.m_Key = ""
            Me.m_Text = ""
            Me.m_ImageUrl = ""
            Me.m_DisabledImageUrl = ""
            Me.m_ToolTip = ""
            Me.m_Key = Key
            Me.m_Text = [Text]
            Me.m_ImageUrl = ImageUrl
            Me.m_DisabledImageUrl = DisabledImageUrl
            Me.m_ToolTip = ToolTip
        End Sub

        Friend Sub NotifyDesigner()
			_logger.LogDebug(">>> Entering NotifyDesigner()")
            If Not Object.ReferenceEquals(Me.Parent, Nothing) Then
                Me.Parent.NotifyDesigner
            End If
			_logger.LogDebug("<<< Exiting NotifyDesigner()")
        End Sub


        ' Properties
        <NotifyParentProperty(True)> _
        Public Property Key As String
            Get
                Return Me.m_Key
            End Get
            Set(ByVal Value As String)
                Me.m_Key = Value
                Me.NotifyDesigner
            End Set
        End Property

        <DefaultValue(""), Category("Appearance"), NotifyParentProperty(True)> _
        Public Property [Text] As String
            Get
                Return Me.m_Text
            End Get
            Set(ByVal Value As String)
                Me.m_Text = Value
                Me.NotifyDesigner
            End Set
        End Property

        <DefaultValue(""), NotifyParentProperty(True), Category("Appearance")> _
        Public Property ToolTip As String
            Get
                Return Me.m_ToolTip
            End Get
            Set(ByVal Value As String)
                Me.m_ToolTip = Value
                Me.NotifyDesigner
            End Set
        End Property

        <DefaultValue(""), NotifyParentProperty(True), Category("Appearance")> _
        Public Property ImageUrl As String
            Get
                Return Me.m_ImageUrl
            End Get
            Set(ByVal Value As String)
                Me.m_ImageUrl = Value
                Me.NotifyDesigner
            End Set
        End Property

        <DefaultValue(""), Category("Appearance"), NotifyParentProperty(True)> _
        Public Property DisabledImageUrl As String
            Get
                Return Me.m_DisabledImageUrl
            End Get
            Set(ByVal Value As String)
                Me.m_DisabledImageUrl = Value
                Me.NotifyDesigner
            End Set
        End Property


        ' Fields
        Friend Parent As DNNMultiStateBox
        Private m_Key As String
        Private m_Text As String
        Private m_ImageUrl As String
        Private m_DisabledImageUrl As String
        Private m_ToolTip As String
    End Class
End Namespace

