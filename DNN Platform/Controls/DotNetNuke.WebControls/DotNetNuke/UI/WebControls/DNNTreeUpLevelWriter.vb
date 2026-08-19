' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNTreeUpLevelWriter
        Inherits WebControl
        Implements IDNNTreeWriter
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New()
			_logger.LogDebug(">>> Entering New()")
            DNNTreeUpLevelWriter.__ENCAddToList(Me)
			_logger.LogDebug("<<< Exiting New()")
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
			_logger.LogDebug(">>> Entering __ENCAddToList(value={Value})", value)
            SyncLock DNNTreeUpLevelWriter.__ENCList
                If (DNNTreeUpLevelWriter.__ENCList.Count = DNNTreeUpLevelWriter.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNTreeUpLevelWriter.__ENCList.Count - 1)
					_logger.LogDebug("__ENCAddToList(value={Value}): num3 → {Num3}", value, num3)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNTreeUpLevelWriter.__ENCList.RemoveRange(index, (DNNTreeUpLevelWriter.__ENCList.Count - index))
                            DNNTreeUpLevelWriter.__ENCList.Capacity = DNNTreeUpLevelWriter.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNTreeUpLevelWriter.__ENCList(num2)
						_logger.LogDebug("__ENCAddToList(value={Value}): reference → {Reference}", value, reference)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNTreeUpLevelWriter.__ENCList(index) = DNNTreeUpLevelWriter.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNTreeUpLevelWriter.__ENCList.Add(New WeakReference(value))
            End SyncLock
			_logger.LogDebug("<<< Exiting __ENCAddToList(value={Value})", value)
        End Sub

        Protected Function MarshalledProperties() As Hashtable Implements IDNNTreeWriter.MarshalledProperties
			_logger.LogDebug(">>> Entering MarshalledProperties()")
            Dim hashtable2 As New Hashtable
            Dim flag As Boolean = (Me._tree.ImageList.Count > 0)
			_logger.LogDebug("MarshalledProperties(): flag → {Flag}", flag)
            If flag Then
                Dim enumerator As IEnumerator = Nothing
                Dim expression As String = ""
				_logger.LogDebug("MarshalledProperties(): expression → {Expression}", expression)
                Try 
                    enumerator = Me._tree.ImageList.GetEnumerator
					_logger.LogDebug("MarshalledProperties(): enumerator → {Enumerator}", enumerator)
                    Do While True
                        flag = enumerator.MoveNext
                        If Not flag Then
                            Exit Do
                        End If
                        Dim current As NodeImage = DirectCast(enumerator.Current, NodeImage)
						_logger.LogDebug("MarshalledProperties(): current → {Current}", current)
                        expression = (expression & Conversions.ToString(Interaction.IIf((Strings.Len(expression) > 0), ",", "")) & current.ImageUrl)
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
                hashtable2.Add("imagelist", expression)
            End If
            hashtable2.Add("postback", ClientAPI.GetPostBackEventReference(Me._tree, ("[NODEID]" & ClientAPI.COLUMN_DELIMITER & "Click")))
            If Me._tree.PopulateNodesFromClient Then
                If ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XMLHTTP) Then
                    hashtable2.Add("callback", ClientAPI.GetCallbackEventReference(Me._tree, "'[NODEXML]'", "this.callBackSuccess", "tNode", "this.callBackFail", "this.callBackStatus"))
                Else
                    hashtable2.Add("callback", ClientAPI.GetPostBackClientHyperlink(Me._tree, ("[NODEID]" & ClientAPI.COLUMN_DELIMITER & "OnDemand")))
                End If
                If (Strings.Len(Me._tree.CallbackStatusFunction) > 0) Then
                    hashtable2.Add("callbackSF", Me._tree.CallbackStatusFunction)
                End If
            End If
			_logger.LogDebug("<<< Exiting MarshalledProperties()")
            Return hashtable2
        End Function

        Protected Overrides Sub RenderChildren(ByVal writer As HtmlTextWriter)
			_logger.LogDebug(">>> Entering RenderChildren(writer={Writer})", writer)
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
			_logger.LogDebug("<<< Exiting RenderChildren(writer={Writer})", writer)
        End Sub

        Protected Overrides Sub RenderContents(ByVal writer As HtmlTextWriter)
			_logger.LogDebug(">>> Entering RenderContents(writer={Writer})", writer)
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "DNNTree")
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Me._tree.UniqueID)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me._tree.ClientID)
            writer.RenderBeginTag(HtmlTextWriterTag.Div)
            writer.RenderEndTag
			_logger.LogDebug("<<< Exiting RenderContents(writer={Writer})", writer)
        End Sub

        Public Sub RenderTree(ByVal writer As HtmlTextWriter, ByVal tree As DnnTree) Implements IDNNTreeWriter.RenderTree
			_logger.LogDebug(">>> Entering RenderTree(writer={Writer},tree={Tree})", writer, tree)
			_logger.LogDebug("RenderTree(writer={Writer},tree={Tree}): Me._tree → {Arg2}", writer, tree, Me._tree)
            Me._tree = tree
            Me.RenderControl(writer)
			_logger.LogDebug("<<< Exiting RenderTree(writer={Writer},tree={Tree})", writer, tree)
        End Sub


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private _tree As DnnTree
    End Class
End Namespace

