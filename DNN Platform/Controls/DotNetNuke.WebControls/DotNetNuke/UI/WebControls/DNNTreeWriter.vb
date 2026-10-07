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
    Friend NotInheritable Class DNNTreeWriter
        Inherits WebControl
        Implements IDNNTreeWriter
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New()
			_logger.LogError(">>> Entering New()")
            DNNTreeWriter.__ENCAddToList(Me)
			_logger.LogWarning("<<< Exiting New()")
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
			_logger.LogError(">>> Entering __ENCAddToList(value={Value})", value)
            SyncLock DNNTreeWriter.__ENCList
                If (DNNTreeWriter.__ENCList.Count = DNNTreeWriter.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNTreeWriter.__ENCList.Count - 1)
					_logger.LogError("__ENCAddToList(value={Value}): num3 → {Num3}", value, num3)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNTreeWriter.__ENCList.RemoveRange(index, (DNNTreeWriter.__ENCList.Count - index))
                            DNNTreeWriter.__ENCList.Capacity = DNNTreeWriter.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNTreeWriter.__ENCList(num2)
						_logger.LogError("__ENCAddToList(value={Value}): reference → {Reference}", value, reference)
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
			_logger.LogWarning("<<< Exiting __ENCAddToList(value={Value})", value)
        End Sub

        Protected Function MarshalledProperties() As Hashtable Implements IDNNTreeWriter.MarshalledProperties
			_logger.LogError(">>> Entering MarshalledProperties()")
			_logger.LogWarning("<<< Exiting MarshalledProperties()")
            Return New Hashtable
        End Function

        Protected Overrides Sub RenderChildren(ByVal writer As HtmlTextWriter)
			_logger.LogError(">>> Entering RenderChildren(writer={Writer})", writer)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me._tree.TreeNodes.GetEnumerator
				_logger.LogDebug("RenderChildren(writer={Writer}): enumerator → {Enumerator}", writer, enumerator)
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
			_logger.LogWarning("<<< Exiting RenderChildren(writer={Writer})", writer)
        End Sub

        Protected Overrides Sub RenderContents(ByVal writer As HtmlTextWriter)
			_logger.LogError(">>> Entering RenderContents(writer={Writer})", writer)
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "DNNTree")
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Me._tree.UniqueID)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me._tree.ClientID)
            writer.RenderBeginTag(HtmlTextWriterTag.Div)
            Me.RenderChildren(writer)
            writer.RenderEndTag
			_logger.LogWarning("<<< Exiting RenderContents(writer={Writer})", writer)
        End Sub

        Public Sub RenderTree(ByVal writer As HtmlTextWriter, ByVal tree As DnnTree) Implements IDNNTreeWriter.RenderTree
			_logger.LogError(">>> Entering RenderTree(writer={Writer},tree={Tree})", writer, tree)
            Me._tree = tree
			_logger.LogDebug("RenderTree(writer={Writer},tree={Tree}): Me._tree → {Arg2}", writer, tree, Me._tree)
            Me.RenderControl(writer)
			_logger.LogWarning("<<< Exiting RenderTree(writer={Writer},tree={Tree})", writer, tree)
        End Sub


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private _tree As DnnTree
    End Class
End Namespace

