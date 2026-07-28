' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNTreeWriter
        Inherits WebControl
        Implements IDNNTreeWriter
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(DNNTreeWriter))
        ' Methods
        Public Sub New()
			Logger.LogDebug(">>> Entering New()")
            DNNTreeWriter.__ENCAddToList(Me)
			Logger.LogDebug("<<< Exiting New()")
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
			Logger.LogDebug(">>> Entering __ENCAddToList(value={Value})", value)
            SyncLock DNNTreeWriter.__ENCList
                If (DNNTreeWriter.__ENCList.Count = DNNTreeWriter.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNTreeWriter.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNTreeWriter.__ENCList.RemoveRange(index, (DNNTreeWriter.__ENCList.Count - index))
                            DNNTreeWriter.__ENCList.Capacity = DNNTreeWriter.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNTreeWriter.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNTreeWriter.__ENCList(index) = DNNTreeWriter.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNTreeWriter.__ENCList.Add(New WeakReference(value))
            End SyncLock
			Logger.LogDebug("<<< Exiting __ENCAddToList(value={Value})", value)
        End Sub

        Protected Function MarshalledProperties() As Hashtable Implements IDNNTreeWriter.MarshalledProperties
			Logger.LogDebug(">>> Entering MarshalledProperties()")
			Logger.LogDebug("<<< Exiting MarshalledProperties()")
            Return New Hashtable
        End Function

        Protected Overrides Sub RenderChildren(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderChildren(writer={Writer})", writer)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me._tree.TreeNodes.GetEnumerator
				Logger.LogDebug("RenderChildren(writer={Writer}): enumerator → {Enumerator}", writer, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    DirectCast(enumerator.Current, TreeNode).Render(writer)
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
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "DNNTree")
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Me._tree.UniqueID)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me._tree.ClientID)
            writer.RenderBeginTag(HtmlTextWriterTag.Div)
            Me.RenderChildren(writer)
            writer.RenderEndTag
			Logger.LogDebug("<<< Exiting RenderContents(writer={Writer})", writer)
        End Sub

        Public Sub RenderTree(ByVal writer As HtmlTextWriter, ByVal tree As DnnTree) Implements IDNNTreeWriter.RenderTree
			Logger.LogDebug(">>> Entering RenderTree(writer={Writer},tree={Tree})", writer, tree)
            Me._tree = tree
            Me.RenderControl(writer)
			Logger.LogDebug("RenderTree(writer={Writer},tree={Tree}): Me._tree → {Arg2}", writer, tree, Me._tree)
			Logger.LogDebug("<<< Exiting RenderTree(writer={Writer},tree={Tree})", writer, tree)
        End Sub


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private _tree As DnnTree
    End Class
End Namespace

