' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports System.Collections

Namespace DotNetNuke.UI.Utilities

    Public Class BrowserCollection

        Inherits CollectionBase
        Implements IList(Of Browser)

        ''' <inheritdoc />
        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Browser).IsReadOnly
            Get
                Return Me.InnerList.IsReadOnly
            End Get
        End Property
        
        ''' <inheritdoc />
        Private ReadOnly Property Generic_Count As Integer Implements ICollection(Of Browser).Count
            Get
                Return MyBase.Count
            End Get
        End Property

        Default Public Overridable Property Item(ByVal index As Integer) As Browser
            Get
                Return DirectCast(MyBase.List.Item(index), Browser)
            End Get
            Set(ByVal Value As Browser)
                MyBase.List.Item(index) = Value
            End Set
        End Property

        ''' <inheritdoc />
        Private Property IList_Item(index As Integer) As Browser Implements IList(Of Browser).Item
            Get
                Return Me(index)
            End Get
            Set
                Me(index) = Value
            End Set
        End Property

        ''' <inheritdoc />
        Public Function Remove(item As Browser) As Boolean Implements ICollection(Of Browser).Remove
			Logger.LogDebug(">>> Entering Remove(item={Item})", item)
            If Not Me.Contains(item) Then
                Return False
            End If

            Me.InnerList.Remove(item)
            Return True
        End Function
		Logger.LogDebug("<<< Exiting Remove(item={Item})", item)

        ''' <inheritdoc />
        Public Sub Add(ByVal b As Browser) Implements ICollection(Of Browser).Add
			Logger.LogDebug(">>> Entering Add(b={B})", b)
            Me.InnerList.Add(b)
        End Sub
		Logger.LogDebug("<<< Exiting Add(b={B})", b)

        ''' <inheritdoc />
        Public Function Contains(item As Browser) As Boolean Implements ICollection(Of Browser).Contains
			Logger.LogDebug(">>> Entering Contains(item={Item})", item)
            Return Me.InnerList.Contains(item)
        End Function
		Logger.LogDebug("<<< Exiting Contains(item={Item})", item)

        ''' <inheritdoc />
        Public Sub CopyTo(array As Browser(), arrayIndex As Integer) Implements ICollection(Of Browser).CopyTo
			Logger.LogDebug(">>> Entering CopyTo(array={Array})", array)
            Me.InnerList.CopyTo(array, arrayIndex)
        End Sub
		Logger.LogDebug("<<< Exiting CopyTo(array={Array})", array)

        ''' <inheritdoc />
        Public Function IndexOf(item As Browser) As Integer Implements IList(Of Browser).IndexOf
			Logger.LogDebug(">>> Entering IndexOf(item={Item})", item)
            Return Me.InnerList.IndexOf(item)
        End Function
		Logger.LogDebug("<<< Exiting IndexOf(item={Item})", item)

        ''' <inheritdoc />
        Public Sub Insert(index As Integer, item As Browser) Implements IList(Of Browser).Insert
			Logger.LogDebug(">>> Entering Insert(index={Index},item={Item})", index, item)
            Me.InnerList.Insert(index, item)
        End Sub
		Logger.LogDebug("<<< Exiting Insert(index={Index},item={Item})", index, item)

        ''' <inheritdoc />
        Private Sub Generic_Clear() Implements ICollection(Of Browser).Clear
			Logger.LogDebug(">>> Entering Generic_Clear()")
            MyBase.Clear()
        End Sub
		Logger.LogDebug("<<< Exiting Generic_Clear()")

        ''' <inheritdoc />
        Private Sub Generic_RemoveAt(index As Integer) Implements IList(Of Browser).RemoveAt
			Logger.LogDebug(">>> Entering Generic_RemoveAt(index={Index})", index)
            MyBase.RemoveAt(index)
        End Sub
		Logger.LogDebug("<<< Exiting Generic_RemoveAt(index={Index})", index)

        ''' <inheritdoc />
        Private Function Generic_GetEnumerator() As IEnumerator(Of Browser) Implements IEnumerable(Of Browser).GetEnumerator
			Logger.LogDebug(">>> Entering Generic_GetEnumerator()")
            Return New BrowserEnumerator(Me.GetEnumerator())
        End Function
		Logger.LogDebug("<<< Exiting Generic_GetEnumerator()")

        Private NotInheritable Class BrowserEnumerator
            Implements IEnumerator(Of Browser)

            Private ReadOnly enumerator As IEnumerator

            Public Sub New(enumerator As IEnumerator)
				Logger.LogDebug(">>> Entering New(enumerator={Enumerator})", enumerator)
                Me.enumerator = enumerator
            End Sub
			Logger.LogDebug("<<< Exiting New(enumerator={Enumerator})", enumerator)
			Logger.LogDebug("New(enumerator={Enumerator}): Me.enumerator → {Arg1}", enumerator, Me.enumerator)

            ''' <inheritdoc />
            Public Sub Dispose() Implements IDisposable.Dispose
				Logger.LogDebug(">>> Entering Dispose()")
                Dim disposable = TryCast(Me.enumerator, IDisposable)
                If disposable IsNot nothing then
                    disposable.Dispose()
                End If
            End Sub
			Logger.LogDebug("<<< Exiting Dispose()")

            ''' <inheritdoc />
            Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
				Logger.LogDebug(">>> Entering MoveNext()")
                Return Me.enumerator.MoveNext()
            End Function
			Logger.LogDebug("<<< Exiting MoveNext()")

            ''' <inheritdoc />
            Public Sub Reset() Implements IEnumerator.Reset
				Logger.LogDebug(">>> Entering Reset()")
                Me.enumerator.Reset()
            End Sub
			Logger.LogDebug("<<< Exiting Reset()")

            ''' <inheritdoc />
            Public ReadOnly Property Generic_Current As Browser Implements IEnumerator(Of Browser).Current
                Get
                    Return CType(Me.enumerator.Current, Browser)
                End Get
            End Property

            ''' <inheritdoc />
            Public ReadOnly Property Current As Object Implements IEnumerator.Current
                Get
                    Return Me.Generic_Current
                End Get
            End Property

        End Class

    End Class

End Namespace
