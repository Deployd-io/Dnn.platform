' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNMenuWriter
        Inherits WebControl
        Implements IDNNMenuWriter
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New()
			_logger.LogError(">>> Entering New()")
            DNNMenuWriter.__ENCAddToList(Me)
			_logger.LogWarning("<<< Exiting New()")
			_logger.LogError(">>> Entering New(blnForceFullMenu={BlnForceFullMenu})", blnForceFullMenu)
			_logger.LogDebug("New(blnForceFullMenu={BlnForceFullMenu}): Me.ForceFullMenu → {Arg1}", blnForceFullMenu, Me.ForceFullMenu)
        _logger.LogWarning("<<< Exiting New(blnForceFullMenu={BlnForceFullMenu})", blnForceFullMenu)
        End Sub

        Public Sub New(ByVal blnForceFullMenu As Boolean)
            DNNMenuWriter.__ENCAddToList(Me)
            Me.ForceFullMenu = blnForceFullMenu
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
			_logger.LogError(">>> Entering __ENCAddToList(value={Value})", value)
            SyncLock DNNMenuWriter.__ENCList
                If (DNNMenuWriter.__ENCList.Count = DNNMenuWriter.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNMenuWriter.__ENCList.Count - 1)
					_logger.LogError("__ENCAddToList(value={Value}): num3 → {Num3}", value, num3)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNMenuWriter.__ENCList.RemoveRange(index, (DNNMenuWriter.__ENCList.Count - index))
                            DNNMenuWriter.__ENCList.Capacity = DNNMenuWriter.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNMenuWriter.__ENCList(num2)
						_logger.LogError("__ENCAddToList(value={Value}): reference → {Reference}", value, reference)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNMenuWriter.__ENCList(index) = DNNMenuWriter.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNMenuWriter.__ENCList.Add(New WeakReference(value))
            End SyncLock
			_logger.LogWarning("<<< Exiting __ENCAddToList(value={Value})", value)
        End Sub

        Protected Overrides Sub RenderChildren(ByVal writer As HtmlTextWriter)
			_logger.LogError(">>> Entering RenderChildren(writer={Writer})", writer)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.m_objMenu.MenuNodes.GetEnumerator
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
            Dim str As String = "DNNMenu "
            If (Me.m_objMenu.SelectedMenuNodes.Count = 0) Then
                If (Strings.Len(Me.m_objMenu.MenuBarCssClass) > 0) Then
                    str = (str & Me.m_objMenu.MenuBarCssClass)
					_logger.LogError("RenderContents(writer={Writer}): str → {Str}", writer, str)
					_logger.LogDebug("RenderContents(writer={Writer}): str → {Str}", writer, str)
                End If
            ElseIf (Strings.Len(Me.m_objMenu.MenuCssClass) > 0) Then
                str = (str & Me.m_objMenu.MenuCssClass)
            End If
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            writer.AddAttribute(HtmlTextWriterAttribute.Class, str)
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Me.m_objMenu.UniqueID)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.m_objMenu.ClientID)
            writer.RenderBeginTag(HtmlTextWriterTag.Div)
            Me.RenderChildren(writer)
            writer.RenderEndTag
			_logger.LogWarning("<<< Exiting RenderContents(writer={Writer})", writer)
        End Sub

        Public Sub RenderMenu(ByVal writer As HtmlTextWriter, ByVal Menu As DNNMenu) Implements IDNNMenuWriter.RenderMenu
			_logger.LogError(">>> Entering RenderMenu(writer={Writer},Menu={Menu})", writer, Menu)
            Me.m_objMenu = Menu
			_logger.LogDebug("RenderMenu(writer={Writer},Menu={Menu}): Me.m_objMenu → {Arg2}", writer, Menu, Me.m_objMenu)
            Me.RenderControl(writer)
			_logger.LogWarning("<<< Exiting RenderMenu(writer={Writer},Menu={Menu})", writer, Menu)
        End Sub


        ' Properties
        Public Property ForceFullMenu As Boolean
            Get
                Return Me.m_blnForceFullMenu
            End Get
            Set(ByVal Value As Boolean)
                Me.m_blnForceFullMenu = Value
            End Set
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_objMenu As DNNMenu
        Private m_blnForceFullMenu As Boolean
    End Class
End Namespace

