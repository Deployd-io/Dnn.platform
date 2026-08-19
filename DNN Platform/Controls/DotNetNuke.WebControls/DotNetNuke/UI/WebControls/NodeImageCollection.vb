' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Reflection
Imports System.Web.UI
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class NodeImageCollection
        Inherits CollectionBase
        Implements IStateManager
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New()
		_logger.LogDebug(">>> Entering New()")
		_logger.LogDebug(">>> Entering New(value={Value})", value)
		_logger.LogDebug("<<< Exiting New(value={Value})", value)
        End Sub

        Public Sub New(ByVal value As NodeImageCollection)
			_logger.LogDebug(">>> Entering New(value={Value})", value)
            Me.AddRange(value)
			_logger.LogDebug("<<< Exiting New(value={Value})", value)
        End Sub

        Public Sub New(ByVal value As NodeImage())
            Me.AddRange(value)
        End Sub

        Public Function Add(ByVal value As NodeImage) As Integer
			_logger.LogDebug(">>> Entering Add(value={Value})", value)
			_logger.LogDebug("Add(value={Value}): num2 → {Num2}", value, num2)
            Dim num2 As Integer = Me.List.Add(value)
            If Me.m_isTrackingViewState Then
                value.TrackViewState
                value.SetDirty
            End If
			_logger.LogDebug(">>> Entering Add(ImageUrl={ImageUrl})", ImageUrl)
			_logger.LogDebug("<<< Exiting Add(value={Value})", value)
			_logger.LogDebug("<<< Exiting Add(ImageUrl={ImageUrl})", ImageUrl)
            Return num2
        End Function

        Public Function Add(ByVal ImageUrl As String) As Integer
            Return Me.Add(New NodeImage(ImageUrl))
        End Function

        Public Sub AddRange(ByVal value As NodeImage())
            Dim num2 As Integer = (value.Length - 1)
            Dim index As Integer = 0
            Do While True
                Dim num3 As Integer = num2
                If (index > num3) Then
                    Return
                End If
                Me.Add(value(index))
                index += 1
            Loop
			_logger.LogDebug(">>> Entering AddRange(value={Value})", value)
			_logger.LogDebug("AddRange(value={Value}): num2 → {Num2}", value, num2)
			_logger.LogDebug("<<< Exiting AddRange(value={Value})", value)
        End Sub

        Public Sub AddRange(ByVal value As NodeImageCollection)
            Dim num2 As Integer = (value.Count - 1)
            Dim num As Integer = 0
            Do While True
                Dim num3 As Integer = num2
                If (num > num3) Then
                    Return
                End If
                Me.Add(DirectCast(value.List(num), NodeImage))
                num += 1
            Loop
        End Sub

        Public Function Contains(ByVal value As NodeImage) As Boolean
			_logger.LogDebug(">>> Entering Contains(value={Value})", value)
			_logger.LogDebug(">>> Entering Contains(strImageUrl={StrImageUrl})", strImageUrl)
			_logger.LogDebug("<<< Exiting Contains(value={Value})", value)
			_logger.LogDebug("<<< Exiting Contains(strImageUrl={StrImageUrl})", strImageUrl)
            Return Not Object.ReferenceEquals(Me.GetImage(value.ImageUrl), Nothing)
        End Function

        Public Function Contains(ByVal strImageUrl As String) As Boolean
            Return Not Object.ReferenceEquals(Me.GetImage(strImageUrl), Nothing)
        End Function

        Public Sub CopyTo(ByVal array As NodeImage(), ByVal index As Integer)
			_logger.LogDebug(">>> Entering CopyTo(array={Array})", array)
            Me.List.CopyTo(array, index)
			_logger.LogDebug("<<< Exiting CopyTo(array={Array})", array)
        End Sub

        Private Function GetImage(ByVal strUrl As String) As NodeImage
			_logger.LogDebug(">>> Entering GetImage(strUrl={StrUrl})", strUrl)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.List.GetEnumerator
				_logger.LogDebug("GetImage(strUrl={StrUrl}): enumerator → {Enumerator}", strUrl, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As NodeImage = DirectCast(enumerator.Current, NodeImage)
					_logger.LogDebug("GetImage(strUrl={StrUrl}): current → {Current}", strUrl, current)
                    If (strUrl = current.ImageUrl) Then
                        Return current
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
			_logger.LogDebug("<<< Exiting GetImage(strUrl={StrUrl})", strUrl)
            Return Nothing
        End Function

        Private Function GetImageIndex(ByVal strUrl As String) As Integer
			_logger.LogDebug(">>> Entering GetImageIndex(strUrl={StrUrl})", strUrl)
			_logger.LogDebug("GetImageIndex(strUrl={StrUrl}): num3 → {Num3}", strUrl, num3)
            Dim num3 As Integer = (Me.List.Count - 1)
            Dim num2 As Integer = 0
            Do While True
                Dim num As Integer
                Dim num4 As Integer = num3
                If (num2 > num4) Then
                    num = -1
					_logger.LogDebug("GetImageIndex(strUrl={StrUrl}): num → {Num}", strUrl, num)
                Else
                    If (strUrl <> DirectCast(Me.List(num2), NodeImage).ImageUrl) Then
                        num2 += 1
                        Continue Do
                    End If
                    num = num2
                End If
                Return num
            Loop

			_logger.LogDebug("<<< Exiting GetImageIndex(strUrl={StrUrl})", strUrl)
            Return Nothing
        End Function

        Public Function IndexOf(ByVal value As NodeImage) As Integer
			_logger.LogDebug(">>> Entering IndexOf(value={Value})", value)
			_logger.LogDebug(">>> Entering IndexOf(strImageUrl={StrImageUrl})", strImageUrl)
			_logger.LogDebug("<<< Exiting IndexOf(value={Value})", value)
			_logger.LogDebug("<<< Exiting IndexOf(strImageUrl={StrImageUrl})", strImageUrl)
            Return Me.GetImageIndex(value.ImageUrl)
        End Function

        Public Function IndexOf(ByVal strImageUrl As String) As Integer
            Return Me.GetImageIndex(strImageUrl)
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal value As NodeImage)
			_logger.LogDebug(">>> Entering Insert(index={Index},value={Value})", index, value)
            Me.List.Insert(index, value)
			_logger.LogDebug("<<< Exiting Insert(index={Index},value={Value})", index, value)
        End Sub

        Public Sub LoadViewState(ByVal state As Object) Implements IStateManager.LoadViewState
			_logger.LogDebug(">>> Entering LoadViewState(state={State})", state)
            If (Not state Is Nothing) Then
                Dim objArray As Object() = DirectCast(state, Object())
				_logger.LogDebug("LoadViewState(state={State}): objArray → {ObjArray}", state, objArray)
				_logger.LogDebug("LoadViewState(state={State}): num2 → {Num2}", state, num2)
                Dim num2 As Integer = (objArray.Length - 1)
                Dim index As Integer = 0
                Do While True
                    Dim num3 As Integer = num2
                    If (index > num3) Then
                        Exit Do
                    End If
                    Dim image As New NodeImage
                    Me.Add(image)
                    DirectCast(image, IStateManager).TrackViewState
                    DirectCast(image, IStateManager).LoadViewState(objArray(index))
                    index += 1
                Loop
            End If
			_logger.LogDebug("<<< Exiting LoadViewState(state={State})", state)
        End Sub

        Public Sub Remove(ByVal value As NodeImage)
			_logger.LogDebug(">>> Entering Remove(value={Value})", value)
            Me.List.Remove(value)
			_logger.LogDebug("<<< Exiting Remove(value={Value})", value)
        End Sub

        Public Function SaveViewState() As Object Implements IStateManager.SaveViewState
            Dim obj2 As Object = Nothing
            If (Me.Count = 0) Then
                obj2 = Nothing
            Else
                Dim objArray As Object() = New Object((Me.Count + 1)  - 1) {}
                Dim num2 As Integer = (Me.Count - 1)
                Dim index As Integer = 0
                Do While True
                    Dim num3 As Integer = num2
                    If (index > num3) Then
                        obj2 = objArray
                        Exit Do
                    End If
                    objArray(index) = Me(index).SaveViewState
                    index += 1
                Loop
            End If
            Return obj2
        End Function

        Public Function ToArray() As NodeImage()
            Dim imageArray As NodeImage() = Nothing
#Disable Warning CA1861
            imageArray = DirectCast(Utils.CopyArray(DirectCast(imageArray, Array), New NodeImage(((Me.Count - 1) + 1)  - 1) {}), NodeImage())
#Enable Warning CA1861
            Me.CopyTo(imageArray, 0)
            Return imageArray
        End Function

        Public Sub TrackViewState() Implements IStateManager.TrackViewState
			_logger.LogDebug(">>> Entering TrackViewState()")
			_logger.LogDebug("TrackViewState(): Me.m_isTrackingViewState → {Arg0}", Me.m_isTrackingViewState)
            Me.m_isTrackingViewState = True
			_logger.LogDebug("<<< Exiting TrackViewState()")
        End Sub


        ' Properties
        Public Default Property Item(ByVal index As Integer) As NodeImage
            Get
                Return DirectCast(Me.List(index), NodeImage)
            End Get
            Set(ByVal Value As NodeImage)
                Me.List(index) = Value
            End Set
        End Property

        Public Property IsTrackingViewState As Boolean Implements IStateManager.IsTrackingViewState
            Get
#Disable Warning BC40000
                Return Me.m_isTrackingViewState
            End Get
            Protected Set(value As Boolean)
                Me.m_isTrackingViewState = value
#Enable Warning BC40000
            End Set
        End Property


        ' Fields
#Disable Warning CA1051
        <Obsolete("Deprecated in DotNetNuke 10.2.3. Please use IsTrackingViewState property. Scheduled removal in v12.0.0.")>
        Protected m_isTrackingViewState As Boolean
#Enable Warning CA1051
    End Class
End Namespace

