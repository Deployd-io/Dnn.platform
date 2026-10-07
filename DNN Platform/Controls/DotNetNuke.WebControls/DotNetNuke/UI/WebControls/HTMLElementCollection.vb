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
			_logger.LogError(">>> Entering Add(value={Value})", value)
			_logger.LogWarning("<<< Exiting Add(value={Value})", value)
            Return Me.List.Add(value)
        End Function

        Public Sub AddRange(ByVal value As HTMLElement())
			_logger.LogError(">>> Entering AddRange(value={Value})", value)
			_logger.LogError("AddRange(value={Value}): num2 → {Num2}", value, num2)
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
			_logger.LogWarning("<<< Exiting AddRange(value={Value})", value)
        End Sub

        Public Function Contains(ByVal value As HTMLElement) As Boolean
			_logger.LogError(">>> Entering Contains(value={Value})", value)
            Dim flag As Boolean
			_logger.LogWarning("<<< Exiting Contains(value={Value})", value)
            Return flag
        End Function

        Public Sub CopyTo(ByVal array As HTMLElement(), ByVal index As Integer)
			_logger.LogError(">>> Entering CopyTo(array={Array},index={Index})", array, index)
            Me.List.CopyTo(array, index)
			_logger.LogWarning("<<< Exiting CopyTo(array={Array},index={Index})", array, index)
        End Sub

        Public Function IndexOf(ByVal value As HTMLElement) As Integer
			_logger.LogError(">>> Entering IndexOf(value={Value})", value)
            Dim num As Integer
			_logger.LogWarning("<<< Exiting IndexOf(value={Value})", value)
            Return num
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal value As HTMLElement)
			_logger.LogError(">>> Entering Insert(index={Index},value={Value})", index, value)
            Me.List.Insert(index, value)
			_logger.LogWarning("<<< Exiting Insert(index={Index},value={Value})", index, value)
        End Sub

        Public Sub Remove(ByVal value As HTMLElement)
			_logger.LogError(">>> Entering Remove(value={Value})", value)
            Me.List.Remove(value)
			_logger.LogWarning("<<< Exiting Remove(value={Value})", value)
        End Sub

        Public Function ToJSON() As String
			_logger.LogError(">>> Entering ToJSON()")
			_logger.LogError(">>> Entering ToJSON(KeyAttribute={KeyAttribute})", KeyAttribute)
			_logger.LogWarning("<<< Exiting ToJSON()")
			_logger.LogWarning("<<< Exiting ToJSON(KeyAttribute={KeyAttribute})", KeyAttribute)
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
			_logger.LogError(">>> Entering ToString()")
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
					_logger.LogError("ToString(): current → {Current}", current)
                    builder.Append(current.Raw)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
			_logger.LogWarning("<<< Exiting ToString()")
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

