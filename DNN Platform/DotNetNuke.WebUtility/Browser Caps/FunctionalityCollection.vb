' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Utilities

    Public Class FunctionalityCollection
	Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

        Inherits CollectionBase
        Implements IList(Of FunctionalityInfo)

        ''' <inheritdoc />
        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of FunctionalityInfo).IsReadOnly
            Get
                Return Me.InnerList.IsReadOnly
            End Get
        End Property
        
        ''' <inheritdoc />
        Private ReadOnly Property Generic_Count As Integer Implements ICollection(Of FunctionalityInfo).Count
            Get
                Return MyBase.Count
            End Get
        End Property

        Public Sub Add(ByVal f As FunctionalityInfo) Implements ICollection(Of FunctionalityInfo).Add
			_logger.LogDebug(">>> Entering Add(f={F})", f)
            Me.InnerList.Add(f)
			_logger.LogDebug("<<< Exiting Add(f={F})", f)
        End Sub

        Default Public Overridable Property Item(ByVal index As Integer) As FunctionalityInfo
            Get
                Return DirectCast(MyBase.List.Item(index), FunctionalityInfo)
            End Get
            Set(ByVal Value As FunctionalityInfo)
                MyBase.List.Item(index) = Value
            End Set
        End Property

        ''' <inheritdoc />
        Private Property IList_Item(index As Integer) As FunctionalityInfo Implements IList(Of FunctionalityInfo).Item
            Get
                Return Me(index)
            End Get
            Set
                Me(index) = Value
            End Set
        End Property

        Default Public Overridable Property Item(ByVal functionality As ClientAPI.ClientFunctionality) As FunctionalityInfo
            Get
                Dim _fInfo As FunctionalityInfo = Nothing
                For Each fInfo As FunctionalityInfo In List
                    If fInfo.Functionality = functionality Then
                        _fInfo = fInfo
                        Exit For
                    End If
                Next
                Return _fInfo
            End Get
            Set(ByVal value As FunctionalityInfo)
                Dim bFound As Boolean = False
                For Each fInfo As FunctionalityInfo In List
                    If fInfo.Functionality = functionality Then
                        fInfo = value
                        bFound = True
                        Exit For
                    End If
                Next
                If Not bFound Then
                    Throw New KeyNotFoundException("Item Not Found")
                End If
            End Set
        End Property

        ''' <inheritdoc />
        Public Function Remove(item As FunctionalityInfo) As Boolean Implements ICollection(Of FunctionalityInfo).Remove
			_logger.LogDebug(">>> Entering Remove(item={Item})", item)
            If Not Me.Contains(item) Then
                Return False
            End If

            Me.InnerList.Remove(item)
			_logger.LogDebug("<<< Exiting Remove(item={Item})", item)
            Return True
        End Function

        ''' <inheritdoc />
        Public Function Contains(item As FunctionalityInfo) As Boolean Implements ICollection(Of FunctionalityInfo).Contains
            Return Me.InnerList.Contains(item)
        End Function

        ''' <inheritdoc />
        Public Sub CopyTo(array As FunctionalityInfo(), arrayIndex As Integer) Implements ICollection(Of FunctionalityInfo).CopyTo
			_logger.LogDebug(">>> Entering CopyTo(array={Array})", array)
            Me.InnerList.CopyTo(array, arrayIndex)
			_logger.LogDebug("<<< Exiting CopyTo(array={Array})", array)
        End Sub

        ''' <inheritdoc />
        Public Function IndexOf(item As FunctionalityInfo) As Integer Implements IList(Of FunctionalityInfo).IndexOf
            Return Me.InnerList.IndexOf(item)
        End Function

        ''' <inheritdoc />
        Public Sub Insert(index As Integer, item As FunctionalityInfo) Implements IList(Of FunctionalityInfo).Insert
			_logger.LogDebug(">>> Entering Insert(index={Index},item={Item})", index, item)
            Me.InnerList.Insert(index, item)
			_logger.LogDebug("<<< Exiting Insert(index={Index},item={Item})", index, item)
        End Sub

        ''' <inheritdoc />
        Private Sub Generic_Clear() Implements ICollection(Of FunctionalityInfo).Clear
            MyBase.Clear()
        End Sub

        ''' <inheritdoc />
        Private Sub Generic_RemoveAt(index As Integer) Implements IList(Of FunctionalityInfo).RemoveAt
			_logger.LogDebug(">>> Entering Generic_RemoveAt(index={Index})", index)
            MyBase.RemoveAt(index)
			_logger.LogDebug("<<< Exiting Generic_RemoveAt(index={Index})", index)
        End Sub

        ''' <inheritdoc />
        Private Function Generic_GetEnumerator() As IEnumerator(Of FunctionalityInfo) Implements IEnumerable(Of FunctionalityInfo).GetEnumerator
            Return New FunctionalityInfoEnumerator(Me.GetEnumerator())
        End Function

        Private NotInheritable Class FunctionalityInfoEnumerator
            Implements IEnumerator(Of FunctionalityInfo)

            Private ReadOnly enumerator As IEnumerator

            Public Sub New(enumerator As IEnumerator)
                Me.enumerator = enumerator
            End Sub

            ''' <inheritdoc />
            Public Sub Dispose() Implements IDisposable.Dispose
				_logger.LogDebug(">>> Entering Dispose()")
                Dim disposable = TryCast(Me.enumerator, IDisposable)
				_logger.LogDebug("Dispose(): disposable → {Disposable}", disposable)
                If disposable IsNot nothing then
                    disposable.Dispose()
                End If
				_logger.LogDebug("<<< Exiting Dispose()")
            End Sub

            ''' <inheritdoc />
            Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
                Return Me.enumerator.MoveNext()
            End Function

            ''' <inheritdoc />
            Public Sub Reset() Implements IEnumerator.Reset
				_logger.LogDebug(">>> Entering Reset()")
                Me.enumerator.Reset()
				_logger.LogDebug("<<< Exiting Reset()")
            End Sub

            ''' <inheritdoc />
            Public ReadOnly Property Generic_Current As FunctionalityInfo Implements IEnumerator(Of FunctionalityInfo).Current
                Get
                    Return CType(Me.enumerator.Current, FunctionalityInfo)
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
