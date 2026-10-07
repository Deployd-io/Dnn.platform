' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Web.UI
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Public Class NodeImage
        Implements IStateManager
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New()
			_logger.LogError(">>> Entering New()")
			_logger.LogDebug("New(): Me._marked → {Arg0}", Me._marked)
        _logger.LogWarning("<<< Exiting New()")
            Me._marked = False
			_logger.LogError(">>> Entering New(NewImageUrl={NewImageUrl})", NewImageUrl)
			_logger.LogDebug("New(NewImageUrl={NewImageUrl}): Me._marked → {Arg1}", NewImageUrl, Me._marked)
			_logger.LogDebug("New(NewImageUrl={NewImageUrl}): Me.ImageUrl → {Arg1}", NewImageUrl, Me.ImageUrl)
        _logger.LogWarning("<<< Exiting New(NewImageUrl={NewImageUrl})", NewImageUrl)
        End Sub

        Public Sub New(ByVal NewImageUrl As String)
            Me._marked = False
            If Object.ReferenceEquals(Me.ImageUrl, Nothing) Then
                Throw New ArgumentNullException(NameOf(NewImageUrl))
            End If
            DirectCast(Me, IStateManager).TrackViewState
            Me.ImageUrl = NewImageUrl
        End Sub

        Public Sub LoadViewState(ByVal state As Object) Implements IStateManager.LoadViewState
			_logger.LogError(">>> Entering LoadViewState(state={State})", state)
            If (Not state Is Nothing) Then
                DirectCast(Me.ViewState, IStateManager).LoadViewState(state)
            End If
			_logger.LogWarning("<<< Exiting LoadViewState(state={State})", state)
        End Sub

        Public Function SaveViewState() As Object Implements IStateManager.SaveViewState
			_logger.LogError(">>> Entering SaveViewState()")
            Dim obj2 As Object = Nothing
            If Not Object.ReferenceEquals(Me._state, Nothing) Then
                obj2 = DirectCast(Me._state, IStateManager).SaveViewState
				_logger.LogDebug("SaveViewState(): obj2 → {Obj2}", obj2)
            End If
			_logger.LogWarning("<<< Exiting SaveViewState()")
            Return obj2
        End Function

        Friend Sub SetDirty()
			_logger.LogError(">>> Entering SetDirty()")
            Dim flag As Boolean = Not Object.ReferenceEquals(Me._state, Nothing)
            If flag Then
                Dim enumerator As IEnumerator = Nothing
                Try 
                    enumerator = Me._state.Keys.GetEnumerator
					_logger.LogDebug("SetDirty(): enumerator → {Enumerator}", enumerator)
                    Do While True
                        flag = enumerator.MoveNext
						_logger.LogError("SetDirty(): flag → {Flag}", flag)
						_logger.LogDebug("SetDirty(): flag → {Flag}", flag)
                        If Not flag Then
                            Exit Do
                        End If
                        Dim key As String = Conversions.ToString(enumerator.Current)
						_logger.LogError("SetDirty(): key → {Key}", key)
                        Me._state.SetItemDirty(key, True)
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
            End If
			_logger.LogWarning("<<< Exiting SetDirty()")
        End Sub

        Public Sub TrackViewState() Implements IStateManager.TrackViewState
			_logger.LogError(">>> Entering TrackViewState()")
			_logger.LogDebug("TrackViewState(): Me._marked → {Arg0}", Me._marked)
        _logger.LogWarning("<<< Exiting TrackViewState()")
            Me._marked = True
        End Sub


        ' Properties
        Public ReadOnly Property IsTrackingViewState As Boolean Implements IStateManager.IsTrackingViewState
            Get
                Return Me._marked
            End Get
        End Property

        Public Property ImageUrl As String
            Get
                Dim str As String = String.Empty
                If (Not Me.ViewState("ImageUrl") Is Nothing) Then
                    str = Conversions.ToString(Me.ViewState("ImageUrl"))
                End If
                Return str
            End Get
            Set(ByVal Value As String)
                Me.ViewState("ImageUrl") = Value
            End Set
        End Property

        Protected ReadOnly Property ViewState As StateBag
            Get
                If Object.ReferenceEquals(Me._state, Nothing) Then
                    Me._state = New StateBag(True)
                    If Me.IsTrackingViewState Then
                        DirectCast(Me._state, IStateManager).TrackViewState
                    End If
                End If
                Return Me._state
            End Get
        End Property


        ' Fields
        Private _marked As Boolean
        Private _state As StateBag
    End Class
End Namespace

