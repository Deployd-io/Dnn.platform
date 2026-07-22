' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Reflection
Imports System.Web.UI

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class NodeImageCollection
        Inherits CollectionBase
        Implements IStateManager
        ' Methods
        Public Sub New()
		Logger.LogDebug(">>> Entering New()")
        End Sub

        Public Sub New(ByVal value As NodeImageCollection)
            Me.AddRange(value)
        End Sub
		Logger.LogDebug("<<< Exiting New(value={Value})", value)

        Public Sub New(ByVal value As NodeImage())
            Me.AddRange(value)
        End Sub

        Public Function Add(ByVal value As NodeImage) As Integer
			Logger.LogDebug(">>> Entering Add(value={Value})", value)
            Dim num2 As Integer = Me.List.Add(value)
            If Me.m_isTrackingViewState Then
                value.TrackViewState
                value.SetDirty
            End If
            Return num2
        End Function
		Logger.LogDebug("<<< Exiting Add(value={Value})", value)

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
        End Sub

        Public Sub AddRange(ByVal value As NodeImageCollection)
			Logger.LogDebug(">>> Entering AddRange(value={Value})", value)
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
		Logger.LogDebug("<<< Exiting AddRange(value={Value})", value)

        Public Function Contains(ByVal value As NodeImage) As Boolean
			Logger.LogDebug(">>> Entering Contains(value={Value})", value)
            Return Not Object.ReferenceEquals(Me.GetImage(value.ImageUrl), Nothing)
        End Function
		Logger.LogDebug("<<< Exiting Contains(value={Value})", value)

        Public Function Contains(ByVal strImageUrl As String) As Boolean
            Return Not Object.ReferenceEquals(Me.GetImage(strImageUrl), Nothing)
        End Function

        Public Sub CopyTo(ByVal array As NodeImage(), ByVal index As Integer)
			Logger.LogDebug(">>> Entering CopyTo(array={Array})", array)
            Me.List.CopyTo(array, index)
        End Sub
		Logger.LogDebug("<<< Exiting CopyTo(array={Array})", array)

        Private Function GetImage(ByVal strUrl As String) As NodeImage
			Logger.LogDebug(">>> Entering GetImage(strUrl={StrUrl})", strUrl)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.List.GetEnumerator
				Logger.LogDebug("GetImage(strUrl={StrUrl}): enumerator → {Enumerator}", strUrl, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As NodeImage = DirectCast(enumerator.Current, NodeImage)
                    If (strUrl = current.ImageUrl) Then
                        Return current
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            Return Nothing
        End Function
		Logger.LogDebug("<<< Exiting GetImage(strUrl={StrUrl})", strUrl)

        Private Function GetImageIndex(ByVal strUrl As String) As Integer
			Logger.LogDebug(">>> Entering GetImageIndex(strUrl={StrUrl})", strUrl)
            Dim num3 As Integer = (Me.List.Count - 1)
            Dim num2 As Integer = 0
            Do While True
                Dim num As Integer
                Dim num4 As Integer = num3
                If (num2 > num4) Then
                    num = -1
					Logger.LogDebug("GetImageIndex(strUrl={StrUrl}): num → {Num}", strUrl, num)
                Else
                    If (strUrl <> DirectCast(Me.List(num2), NodeImage).ImageUrl) Then
                        num2 += 1
                        Continue Do
                    End If
                    num = num2
                End If
                Return num
            Loop

            Return Nothing
        End Function
		Logger.LogDebug("<<< Exiting GetImageIndex(strUrl={StrUrl})", strUrl)

        Public Function IndexOf(ByVal value As NodeImage) As Integer
			Logger.LogDebug(">>> Entering IndexOf(value={Value})", value)
            Return Me.GetImageIndex(value.ImageUrl)
        End Function
		Logger.LogDebug("<<< Exiting IndexOf(value={Value})", value)

        Public Function IndexOf(ByVal strImageUrl As String) As Integer
            Return Me.GetImageIndex(strImageUrl)
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal value As NodeImage)
			Logger.LogDebug(">>> Entering Insert(index={Index},value={Value})", index, value)
            Me.List.Insert(index, value)
        End Sub
		Logger.LogDebug("<<< Exiting Insert(index={Index},value={Value})", index, value)

        Public Sub LoadViewState(ByVal state As Object) Implements IStateManager.LoadViewState
			Logger.LogDebug(">>> Entering LoadViewState(state={State})", state)
            If (Not state Is Nothing) Then
                Dim objArray As Object() = DirectCast(state, Object())
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
        End Sub
		Logger.LogDebug("<<< Exiting LoadViewState(state={State})", state)

        Public Sub Remove(ByVal value As NodeImage)
			Logger.LogDebug(">>> Entering Remove(value={Value})", value)
            Me.List.Remove(value)
        End Sub
		Logger.LogDebug("<<< Exiting Remove(value={Value})", value)

        Public Function SaveViewState() As Object Implements IStateManager.SaveViewState
			Logger.LogDebug(">>> Entering SaveViewState()")
            Dim obj2 As Object = Nothing
            If (Me.Count = 0) Then
                obj2 = Nothing
				Logger.LogDebug("SaveViewState(): obj2 → {Obj2}", obj2)
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
		Logger.LogDebug("<<< Exiting SaveViewState()")

        Public Function ToArray() As NodeImage()
			Logger.LogDebug(">>> Entering ToArray()")
            Dim imageArray As NodeImage() = Nothing
#Disable Warning CA1861
            imageArray = DirectCast(Utils.CopyArray(DirectCast(imageArray, Array), New NodeImage(((Me.Count - 1) + 1)  - 1) {}), NodeImage())
			Logger.LogDebug("ToArray(): imageArray → {ImageArray}", imageArray)
#Enable Warning CA1861
            Me.CopyTo(imageArray, 0)
            Return imageArray
        End Function
		Logger.LogDebug("<<< Exiting ToArray()")

        Public Sub TrackViewState() Implements IStateManager.TrackViewState
			Logger.LogDebug(">>> Entering TrackViewState()")
            Me.m_isTrackingViewState = True
        End Sub
		Logger.LogDebug("<<< Exiting TrackViewState()")
		Logger.LogDebug("TrackViewState(): Me.m_isTrackingViewState → {Arg0}", Me.m_isTrackingViewState)


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

