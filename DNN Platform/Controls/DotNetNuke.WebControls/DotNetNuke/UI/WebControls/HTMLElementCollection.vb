' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Globalization
Imports System.Reflection
Imports System.Text
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class HTMLElementCollection
        Inherits CollectionBase
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Function Add(ByVal value As HTMLElement) As Integer
			_logger.LogDebug(">>> Entering Add(value={Value})", value)
			_logger.LogDebug("<<< Exiting Add(value={Value})", value)
            Return Me.List.Add(value)
        End Function

        Public Sub AddRange(ByVal value As HTMLElement())
			_logger.LogDebug(">>> Entering AddRange(value={Value})", value)
			_logger.LogDebug("AddRange(value={Value}): num2 → {Num2}", value, num2)
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
			_logger.LogDebug("<<< Exiting AddRange(value={Value})", value)
        End Sub

        Public Function Contains(ByVal value As HTMLElement) As Boolean
			_logger.LogDebug(">>> Entering Contains(value={Value})", value)
            Dim flag As Boolean
			_logger.LogDebug("<<< Exiting Contains(value={Value})", value)
            Return flag
        End Function

        Public Sub CopyTo(ByVal array As HTMLElement(), ByVal index As Integer)
			_logger.LogDebug(">>> Entering CopyTo(array={Array})", array)
            Me.List.CopyTo(array, index)
			_logger.LogDebug("<<< Exiting CopyTo(array={Array})", array)
        End Sub

        Public Function IndexOf(ByVal value As HTMLElement) As Integer
			_logger.LogDebug(">>> Entering IndexOf(value={Value})", value)
            Dim num As Integer
			_logger.LogDebug("<<< Exiting IndexOf(value={Value})", value)
            Return num
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal value As HTMLElement)
			_logger.LogDebug(">>> Entering Insert(index={Index},value={Value})", index, value)
            Me.List.Insert(index, value)
			_logger.LogDebug("<<< Exiting Insert(index={Index},value={Value})", index, value)
        End Sub

        Public Sub Remove(ByVal value As HTMLElement)
			_logger.LogDebug(">>> Entering Remove(value={Value})", value)
            Me.List.Remove(value)
			_logger.LogDebug("<<< Exiting Remove(value={Value})", value)
        End Sub

        Public Function ToJSON() As String
			_logger.LogDebug(">>> Entering ToJSON()")
			_logger.LogDebug(">>> Entering ToJSON(KeyAttribute={KeyAttribute})", KeyAttribute)
			_logger.LogDebug("<<< Exiting ToJSON()")
			_logger.LogDebug("<<< Exiting ToJSON(KeyAttribute={KeyAttribute})", KeyAttribute)
            Return Me.ToJSON("")
        End Function

        Public Function ToJSON(ByVal KeyAttribute As String) As String
            Dim enumerator As IEnumerator = Nothing
            Dim builder As New StringBuilder
            builder.Append("{"c)
            Try 
                enumerator = Me.List.GetEnumerator
                Do While True
                    Dim str As String
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As HTMLElement = DirectCast(enumerator.Current, HTMLElement)
                    If (builder.Length > 1) Then
                        builder.Append(","c)
                    End If
                    If current.Attributes.Contains(KeyAttribute) Then
                        str = Conversions.ToString(current.Attributes(KeyAttribute))
                    Else
                        Dim num As Integer
                        num += 1
                        str = ("__" & num.ToString(CultureInfo.InvariantCulture))
                    End If
                    builder.Append((str & ":" & current.ToJSON))
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            builder.Append("}"c)
            Return builder.ToString
        End Function

        Public Overrides Function ToString() As String
			_logger.LogDebug(">>> Entering ToString()")
            Dim enumerator As IEnumerator = Nothing
            Dim builder As New StringBuilder
            Try 
                enumerator = Me.List.GetEnumerator
				_logger.LogDebug("ToString(): enumerator → {Enumerator}", enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As HTMLElement = DirectCast(enumerator.Current, HTMLElement)
					_logger.LogDebug("ToString(): current → {Current}", current)
                    builder.Append(current.Raw)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
			_logger.LogDebug("<<< Exiting ToString()")
            Return builder.ToString
        End Function


        ' Properties
        Public Default Property Item(ByVal index As Integer) As HTMLElement
            Get
                Return DirectCast(Me.List(index), HTMLElement)
            End Get
            Set(ByVal Value As HTMLElement)
                Me.List(index) = Value
            End Set
        End Property

    End Class
End Namespace

