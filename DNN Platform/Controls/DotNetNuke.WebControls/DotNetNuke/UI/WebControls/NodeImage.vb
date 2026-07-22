' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    Public Class NodeImage
        Implements IStateManager
        ' Methods
        Public Sub New()
			Logger.LogDebug(">>> Entering New()")
            Me._marked = False
        End Sub
		Logger.LogDebug("<<< Exiting New()")
		Logger.LogDebug("New(): Me._marked → {Arg0}", Me._marked)

        Public Sub New(ByVal NewImageUrl As String)
            Me._marked = False
            If Object.ReferenceEquals(Me.ImageUrl, Nothing) Then
				Logger.LogDebug("New(NewImageUrl={NewImageUrl}): Me._marked → {Arg1}", NewImageUrl, Me._marked)
                Throw New ArgumentNullException(NameOf(NewImageUrl))
            End If
            DirectCast(Me, IStateManager).TrackViewState
            Me.ImageUrl = NewImageUrl
			Logger.LogDebug("New(NewImageUrl={NewImageUrl}): Me.ImageUrl → {Arg1}", NewImageUrl, Me.ImageUrl)
        End Sub

        Public Sub LoadViewState(ByVal state As Object) Implements IStateManager.LoadViewState
			Logger.LogDebug(">>> Entering LoadViewState(state={State})", state)
            If (Not state Is Nothing) Then
                DirectCast(Me.ViewState, IStateManager).LoadViewState(state)
            End If
        End Sub
		Logger.LogDebug("<<< Exiting LoadViewState(state={State})", state)

        Public Function SaveViewState() As Object Implements IStateManager.SaveViewState
			Logger.LogDebug(">>> Entering SaveViewState()")
            Dim obj2 As Object = Nothing
            If Not Object.ReferenceEquals(Me._state, Nothing) Then
                obj2 = DirectCast(Me._state, IStateManager).SaveViewState
				Logger.LogDebug("SaveViewState(): obj2 → {Obj2}", obj2)
            End If
            Return obj2
        End Function
		Logger.LogDebug("<<< Exiting SaveViewState()")

        Friend Sub SetDirty()
			Logger.LogDebug(">>> Entering SetDirty()")
            Dim flag As Boolean = Not Object.ReferenceEquals(Me._state, Nothing)
            If flag Then
                Dim enumerator As IEnumerator = Nothing
                Try 
                    enumerator = Me._state.Keys.GetEnumerator
					Logger.LogDebug("SetDirty(): enumerator → {Enumerator}", enumerator)
                    Do While True
                        flag = enumerator.MoveNext
						Logger.LogDebug("SetDirty(): flag → {Flag}", flag)
                        If Not flag Then
                            Exit Do
                        End If
                        Dim key As String = Conversions.ToString(enumerator.Current)
                        Me._state.SetItemDirty(key, True)
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
            End If
        End Sub
		Logger.LogDebug("<<< Exiting SetDirty()")

        Public Sub TrackViewState() Implements IStateManager.TrackViewState
			Logger.LogDebug(">>> Entering TrackViewState()")
            Me._marked = True
        End Sub
		Logger.LogDebug("<<< Exiting TrackViewState()")
		Logger.LogDebug("TrackViewState(): Me._marked → {Arg0}", Me._marked)


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

