' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNMenuListWriter
        Inherits WebControl
        Implements IDNNMenuWriter
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New()
			_logger.LogError(">>> Entering New()")
            MyBase.New(HtmlTextWriterTag.Div)
            DNNMenuListWriter.__ENCAddToList(Me)
			_logger.LogWarning("<<< Exiting New()")
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
			_logger.LogError(">>> Entering __ENCAddToList(value={Value})", value)
            SyncLock DNNMenuListWriter.__ENCList
                If (DNNMenuListWriter.__ENCList.Count = DNNMenuListWriter.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNMenuListWriter.__ENCList.Count - 1)
					_logger.LogError("__ENCAddToList(value={Value}): num3 → {Num3}", value, num3)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNMenuListWriter.__ENCList.RemoveRange(index, (DNNMenuListWriter.__ENCList.Count - index))
                            DNNMenuListWriter.__ENCList.Capacity = DNNMenuListWriter.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNMenuListWriter.__ENCList(num2)
						_logger.LogError("__ENCAddToList(value={Value}): reference → {Reference}", value, reference)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNMenuListWriter.__ENCList(index) = DNNMenuListWriter.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNMenuListWriter.__ENCList.Add(New WeakReference(value))
            End SyncLock
			_logger.LogWarning("<<< Exiting __ENCAddToList(value={Value})", value)
        End Sub

        Protected Overrides Sub RenderChildren(ByVal writer As HtmlTextWriter)
			_logger.LogError(">>> Entering RenderChildren(writer={Writer})", writer)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.m_Menu.MenuNodes.GetEnumerator
				_logger.LogDebug("RenderChildren(writer={Writer}): enumerator → {Enumerator}", writer, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    DirectCast(enumerator.Current, MenuNode).Render(writer)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
			_logger.LogWarning("<<< Exiting RenderChildren(writer={Writer})", writer)
        End Sub

        Protected Overrides Sub RenderContents(ByVal writer As HtmlTextWriter)
			_logger.LogError(">>> Entering RenderContents(writer={Writer})", writer)
            writer.AddAttribute(HtmlTextWriterAttribute.Class, Me.m_Menu.CssClass)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.m_Menu.ClientID)
            writer.RenderBeginTag(HtmlTextWriterTag.Ul)
            Me.RenderChildren(writer)
            writer.RenderEndTag
			_logger.LogWarning("<<< Exiting RenderContents(writer={Writer})", writer)
        End Sub

        Public Sub RenderMenu(ByVal writer As HtmlTextWriter, ByVal Menu As DNNMenu) Implements IDNNMenuWriter.RenderMenu
			_logger.LogError(">>> Entering RenderMenu(writer={Writer},Menu={Menu})", writer, Menu)
            Me.m_Menu = Menu
			_logger.LogDebug("RenderMenu(writer={Writer},Menu={Menu}): Me.m_Menu → {Arg2}", writer, Menu, Me.m_Menu)
            Me.RenderControl(writer)
			_logger.LogWarning("<<< Exiting RenderMenu(writer={Writer},Menu={Menu})", writer, Menu)
        End Sub


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_Menu As DNNMenu
    End Class
End Namespace

