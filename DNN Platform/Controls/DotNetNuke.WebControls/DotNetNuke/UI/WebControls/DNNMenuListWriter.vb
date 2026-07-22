' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNMenuListWriter
        Inherits WebControl
        Implements IDNNMenuWriter
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(DNNMenuListWriter))
        ' Methods
        Public Sub New()
			Logger.LogDebug(">>> Entering New()")
            MyBase.New(HtmlTextWriterTag.Div)
            DNNMenuListWriter.__ENCAddToList(Me)
			Logger.LogDebug("<<< Exiting New()")
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
			Logger.LogDebug(">>> Entering __ENCAddToList(value={Value})", value)
            SyncLock DNNMenuListWriter.__ENCList
                If (DNNMenuListWriter.__ENCList.Count = DNNMenuListWriter.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNMenuListWriter.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNMenuListWriter.__ENCList.RemoveRange(index, (DNNMenuListWriter.__ENCList.Count - index))
                            DNNMenuListWriter.__ENCList.Capacity = DNNMenuListWriter.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNMenuListWriter.__ENCList(num2)
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
			Logger.LogDebug("<<< Exiting __ENCAddToList(value={Value})", value)
        End Sub

        Protected Overrides Sub RenderChildren(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderChildren(writer={Writer})", writer)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.m_Menu.MenuNodes.GetEnumerator
				Logger.LogDebug("RenderChildren(writer={Writer}): enumerator → {Enumerator}", writer, enumerator)
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
			Logger.LogDebug("<<< Exiting RenderChildren(writer={Writer})", writer)
        End Sub

        Protected Overrides Sub RenderContents(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderContents(writer={Writer})", writer)
            writer.AddAttribute(HtmlTextWriterAttribute.Class, Me.m_Menu.CssClass)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.m_Menu.ClientID)
            writer.RenderBeginTag(HtmlTextWriterTag.Ul)
            Me.RenderChildren(writer)
            writer.RenderEndTag
			Logger.LogDebug("<<< Exiting RenderContents(writer={Writer})", writer)
        End Sub

        Public Sub RenderMenu(ByVal writer As HtmlTextWriter, ByVal Menu As DNNMenu) Implements IDNNMenuWriter.RenderMenu
			Logger.LogDebug(">>> Entering RenderMenu(writer={Writer},Menu={Menu})", writer, Menu)
            Me.m_Menu = Menu
            Me.RenderControl(writer)
			Logger.LogDebug("RenderMenu(writer={Writer},Menu={Menu}): Me.m_Menu → {Arg2}", writer, Menu, Me.m_Menu)
			Logger.LogDebug("<<< Exiting RenderMenu(writer={Writer},Menu={Menu})", writer, Menu)
        End Sub


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_Menu As DNNMenu
    End Class
End Namespace

