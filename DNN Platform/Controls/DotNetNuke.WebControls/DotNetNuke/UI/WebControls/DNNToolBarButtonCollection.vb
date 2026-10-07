' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Reflection
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class DNNToolBarButtonCollection
        Implements IList, ICollection, IEnumerable
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Friend Sub New(ByVal owner As DNNToolBar)
			_logger.LogError(">>> Entering New(owner={Owner})", owner)
			_logger.LogDebug("New(owner={Owner}): Me.owner → {Arg1}", owner, Me.owner)
        _logger.LogWarning("<<< Exiting New(owner={Owner})", owner)
            Me.owner = owner
        End Sub

        Public Function Add(ByVal row As DNNToolBarButton) As Integer
			_logger.LogError(">>> Entering Add(row={Row})", row)
            Me.AddAt(-1, row)
			_logger.LogError(">>> Entering Add(o={O})", o)
			_logger.LogWarning("<<< Exiting Add(row={Row})", row)
			_logger.LogWarning("<<< Exiting Add(o={O})", o)
            Return (Me.owner.Controls.Count - 1)
        End Function

        Private Function Add(ByVal o As Object) As Integer Implements IList.Add
            Return Me.Add(DirectCast(o, DNNToolBarButton))
        End Function

        Public Sub AddAt(ByVal index As Integer, ByVal row As DNNToolBarButton)
			_logger.LogError(">>> Entering AddAt(index={Index},row={Row})", index, row)
            Me.owner.Controls.AddAt(index, row)
			_logger.LogWarning("<<< Exiting AddAt(index={Index},row={Row})", index, row)
        End Sub

        Public Sub AddRange(ByVal rows As DNNToolBarButton())
			_logger.LogError(">>> Entering AddRange(rows={Rows})", rows)
            If (rows Is Nothing) Then
                Throw New ArgumentNullException(NameOf(rows))
            End If
            Dim button As DNNToolBarButton
            For Each button In rows
                Me.Add(button)
            Next
			_logger.LogWarning("<<< Exiting AddRange(rows={Rows})", rows)
        End Sub

        Public Sub Clear() Implements IList.Clear
			_logger.LogError(">>> Entering Clear()")
            If Me.owner.HasControls Then
                Me.owner.Controls.Clear
            End If
			_logger.LogWarning("<<< Exiting Clear()")
        End Sub

        Private Function Contains(ByVal o As Object) As Boolean Implements IList.Contains
			_logger.LogError(">>> Entering Contains(o={O})", o)
			_logger.LogWarning("<<< Exiting Contains(o={O})", o)
            Return Me.owner.Controls.Contains(DirectCast(o, DNNToolBarButton))
        End Function

        Public Sub CopyTo(ByVal array As Array, ByVal index As Integer) Implements ICollection.CopyTo
			_logger.LogError(">>> Entering CopyTo(array={Array},index={Index})", array, index)
            Dim enumerator As IEnumerator = Me.GetEnumerator
            Do While enumerator.MoveNext
                index += 1
                array.SetValue(enumerator.Current, index)
            Loop
			_logger.LogWarning("<<< Exiting CopyTo(array={Array},index={Index})", array, index)
        End Sub

        Public Function FindTab(ByVal strId As String) As DNNToolBarButton
			_logger.LogError(">>> Entering FindTab(strId={StrId})", strId)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.GetEnumerator
				_logger.LogDebug("FindTab(strId={StrId}): enumerator → {Enumerator}", strId, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNToolBarButton = DirectCast(enumerator.Current, DNNToolBarButton)
					_logger.LogError("FindTab(strId={StrId}): current → {Current}", strId, current)
                    If (current.ID = strId) Then
                        Return current
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
			_logger.LogWarning("<<< Exiting FindTab(strId={StrId})", strId)
            Return Nothing
        End Function

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
			_logger.LogError(">>> Entering GetEnumerator()")
			_logger.LogWarning("<<< Exiting GetEnumerator()")
            Return Me.owner.Controls.GetEnumerator
        End Function

        Public Function GetRowIndex(ByVal row As DNNToolBarButton) As Integer
			_logger.LogError(">>> Entering GetRowIndex(row={Row})", row)
			_logger.LogWarning("<<< Exiting GetRowIndex(row={Row})", row)
            Return If(Not Me.owner.HasControls, -1, Me.owner.Controls.IndexOf(row))
        End Function

        Private Function IndexOf(ByVal o As Object) As Integer Implements IList.IndexOf
			_logger.LogError(">>> Entering IndexOf(o={O})", o)
			_logger.LogWarning("<<< Exiting IndexOf(o={O})", o)
            Return Me.owner.Controls.IndexOf(DirectCast(o, DNNToolBarButton))
        End Function

        Private Sub Insert(ByVal index As Integer, ByVal o As Object) Implements IList.Insert
			_logger.LogError(">>> Entering Insert(index={Index},o={O})", index, o)
            Me.AddAt(index, DirectCast(o, DNNToolBarButton))
			_logger.LogWarning("<<< Exiting Insert(index={Index},o={O})", index, o)
        End Sub

        Public Sub Remove(ByVal row As DNNToolBarButton)
			_logger.LogError(">>> Entering Remove(row={Row})", row)
            Me.owner.Controls.Remove(row)
			_logger.LogWarning("<<< Exiting Remove(row={Row})", row)
			_logger.LogError(">>> Entering Remove(o={O})", o)
			_logger.LogWarning("<<< Exiting Remove(o={O})", o)
        End Sub

        Private Sub Remove(ByVal o As Object) Implements IList.Remove
            Me.Remove(DirectCast(o, DNNToolBarButton))
        End Sub

        Public Sub RemoveAt(ByVal index As Integer) Implements IList.RemoveAt
			_logger.LogError(">>> Entering RemoveAt(index={Index})", index)
            Me.owner.Controls.RemoveAt(index)
			_logger.LogWarning("<<< Exiting RemoveAt(index={Index})", index)
        End Sub


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

