' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports System
Imports System.Collections
Imports System.Reflection

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class DNNToolBarButtonCollection
        Implements IList, ICollection, IEnumerable
        ' Methods
        Friend Sub New(ByVal owner As DNNToolBar)
			Logger.LogDebug(">>> Entering New(owner={Owner})", owner)
            Me.owner = owner
        End Sub
		Logger.LogDebug("<<< Exiting New(owner={Owner})", owner)
		Logger.LogDebug("New(owner={Owner}): Me.owner → {Arg1}", owner, Me.owner)

        Public Function Add(ByVal row As DNNToolBarButton) As Integer
			Logger.LogDebug(">>> Entering Add(row={Row})", row)
            Me.AddAt(-1, row)
            Return (Me.owner.Controls.Count - 1)
        End Function
		Logger.LogDebug("<<< Exiting Add(row={Row})", row)

        Private Function Add(ByVal o As Object) As Integer Implements IList.Add
            Return Me.Add(DirectCast(o, DNNToolBarButton))
        End Function

        Public Sub AddAt(ByVal index As Integer, ByVal row As DNNToolBarButton)
			Logger.LogDebug(">>> Entering AddAt(index={Index},row={Row})", index, row)
            Me.owner.Controls.AddAt(index, row)
        End Sub
		Logger.LogDebug("<<< Exiting AddAt(index={Index},row={Row})", index, row)

        Public Sub AddRange(ByVal rows As DNNToolBarButton())
			Logger.LogDebug(">>> Entering AddRange(rows={Rows})", rows)
            If (rows Is Nothing) Then
                Throw New ArgumentNullException(NameOf(rows))
            End If
            Dim button As DNNToolBarButton
            For Each button In rows
                Me.Add(button)
            Next
        End Sub
		Logger.LogDebug("<<< Exiting AddRange(rows={Rows})", rows)

        Public Sub Clear() Implements IList.Clear
			Logger.LogDebug(">>> Entering Clear()")
            If Me.owner.HasControls Then
                Me.owner.Controls.Clear
            End If
        End Sub
		Logger.LogDebug("<<< Exiting Clear()")

        Private Function Contains(ByVal o As Object) As Boolean Implements IList.Contains
			Logger.LogDebug(">>> Entering Contains(o={O})", o)
            Return Me.owner.Controls.Contains(DirectCast(o, DNNToolBarButton))
        End Function
		Logger.LogDebug("<<< Exiting Contains(o={O})", o)

        Public Sub CopyTo(ByVal array As Array, ByVal index As Integer) Implements ICollection.CopyTo
			Logger.LogDebug(">>> Entering CopyTo(array={Array},index={Index})", array, index)
            Dim enumerator As IEnumerator = Me.GetEnumerator
            Do While enumerator.MoveNext
                index += 1
                array.SetValue(enumerator.Current, index)
            Loop
        End Sub
		Logger.LogDebug("<<< Exiting CopyTo(array={Array},index={Index})", array, index)

        Public Function FindTab(ByVal strId As String) As DNNToolBarButton
			Logger.LogDebug(">>> Entering FindTab(strId={StrId})", strId)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.GetEnumerator
				Logger.LogDebug("FindTab(strId={StrId}): enumerator → {Enumerator}", strId, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNToolBarButton = DirectCast(enumerator.Current, DNNToolBarButton)
                    If (current.ID = strId) Then
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
		Logger.LogDebug("<<< Exiting FindTab(strId={StrId})", strId)

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
			Logger.LogDebug(">>> Entering GetEnumerator()")
            Return Me.owner.Controls.GetEnumerator
        End Function
		Logger.LogDebug("<<< Exiting GetEnumerator()")

        Public Function GetRowIndex(ByVal row As DNNToolBarButton) As Integer
			Logger.LogDebug(">>> Entering GetRowIndex(row={Row})", row)
            Return If(Not Me.owner.HasControls, -1, Me.owner.Controls.IndexOf(row))
        End Function
		Logger.LogDebug("<<< Exiting GetRowIndex(row={Row})", row)

        Private Function IndexOf(ByVal o As Object) As Integer Implements IList.IndexOf
			Logger.LogDebug(">>> Entering IndexOf(o={O})", o)
            Return Me.owner.Controls.IndexOf(DirectCast(o, DNNToolBarButton))
        End Function
		Logger.LogDebug("<<< Exiting IndexOf(o={O})", o)

        Private Sub Insert(ByVal index As Integer, ByVal o As Object) Implements IList.Insert
			Logger.LogDebug(">>> Entering Insert(index={Index},o={O})", index, o)
            Me.AddAt(index, DirectCast(o, DNNToolBarButton))
        End Sub
		Logger.LogDebug("<<< Exiting Insert(index={Index},o={O})", index, o)

        Public Sub Remove(ByVal row As DNNToolBarButton)
			Logger.LogDebug(">>> Entering Remove(row={Row})", row)
            Me.owner.Controls.Remove(row)
        End Sub
		Logger.LogDebug("<<< Exiting Remove(row={Row})", row)

        Private Sub Remove(ByVal o As Object) Implements IList.Remove
            Me.Remove(DirectCast(o, DNNToolBarButton))
        End Sub

        Public Sub RemoveAt(ByVal index As Integer) Implements IList.RemoveAt
			Logger.LogDebug(">>> Entering RemoveAt(index={Index})", index)
            Me.owner.Controls.RemoveAt(index)
        End Sub
		Logger.LogDebug("<<< Exiting RemoveAt(index={Index})", index)


        ' Properties
        Public ReadOnly Property Count As Integer Implements ICollection.Count
            Get
                Return If(Not Me.owner.HasControls, 0, Me.owner.Controls.Count)
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements IList.IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Default Property Item(ByVal index As Integer) As DNNToolBarButton
            Get
                Return DirectCast(Me.owner.Controls(index), DNNToolBarButton)
            End Get
        End Property

        Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
            Get
                Return Me
            End Get
        End Property

        Private ReadOnly Property IsFixedSize As Boolean Implements IList.IsFixedSize
            Get
                Return False
            End Get
        End Property

        Private Property _item(ByVal index As Integer) As Object Implements IList.Item
            Get
                Return Me.owner.Controls(index)
            End Get
            Set(ByVal value As Object)
                Me.RemoveAt(index)
                Me.AddAt(index, DirectCast(value, DNNToolBarButton))
            End Set
        End Property


        ' Fields
        Private owner As DNNToolBar
    End Class
End Namespace

