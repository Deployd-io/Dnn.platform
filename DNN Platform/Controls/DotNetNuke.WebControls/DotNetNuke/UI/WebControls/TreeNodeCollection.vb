' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Reflection
Imports System.Web.UI
Imports System.Xml
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class TreeNodeCollection
        Inherits DNNNodeCollection
        Implements IStateManager
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal JSONDict As Dictionary(Of String, Object), ByVal Tree As DnnTree)
			_logger.LogError(">>> Entering New(JSONDict={JSONDict},Tree={Tree})", JSONDict, Tree)
            MyBase.New(Tree.ClientID, JSONDict)
            Me.m_DNNTree = Tree
			_logger.LogDebug("New(JSONDict={JSONDict},Tree={Tree}): Me.m_DNNTree → {Arg2}", JSONDict, Tree, Me.m_DNNTree)
        _logger.LogWarning("<<< Exiting New(JSONDict={JSONDict},Tree={Tree})", JSONDict, Tree)
			_logger.LogError(">>> Entering New(strNamespace={StrNamespace},objControl={ObjControl})", strNamespace, objControl)
			_logger.LogDebug("New(strNamespace={StrNamespace},objControl={ObjControl}): Me.m_DNNTree → {Arg2}", strNamespace, objControl, Me.m_DNNTree)
        _logger.LogWarning("<<< Exiting New(strNamespace={StrNamespace},objControl={ObjControl})", strNamespace, objControl)
        End Sub

        Public Sub New(ByVal strNamespace As String, ByVal objControl As DnnTree)
            MyBase.New(strNamespace, objControl)
            Me.m_DNNTree = objControl
			_logger.LogError(">>> Entering New(objXmlNode={ObjXmlNode},objControl={ObjControl})", objXmlNode, objControl)
			_logger.LogDebug("New(objXmlNode={ObjXmlNode},objControl={ObjControl}): Me.m_DNNTree → {Arg2}", objXmlNode, objControl, Me.m_DNNTree)
        _logger.LogWarning("<<< Exiting New(objXmlNode={ObjXmlNode},objControl={ObjControl})", objXmlNode, objControl)
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode, ByVal objControl As DnnTree)
            MyBase.New(objXmlNode, objControl)
            Me.m_DNNTree = objControl
			_logger.LogError(">>> Entering New(strXML={StrXML},strXSLFile={StrXSLFile},objControl={ObjControl})", strXML, strXSLFile, objControl)
			_logger.LogDebug("New(strXML={StrXML},strXSLFile={StrXSLFile},objControl={ObjControl}): Me.m_DNNTree → {Arg3}", strXML, strXSLFile, objControl, Me.m_DNNTree)
        _logger.LogWarning("<<< Exiting New(strXML={StrXML},strXSLFile={StrXSLFile},objControl={ObjControl})", strXML, strXSLFile, objControl)
        End Sub

        Public Sub New(ByVal strXML As String, ByVal strXSLFile As String, ByVal objControl As DnnTree)
            MyBase.New(strXML, strXSLFile)
            Me.m_DNNTree = objControl
        End Sub

        Public Shadows Function Add() As Integer
            Return Me.Add("")
        End Function

        Public Shadows Function Add(ByVal objNode As TreeNode) As Integer
            Dim num2 As Integer = MyBase.Add(objNode)
            objNode.SetDNNTree(Me.m_DNNTree)
            If (Strings.Len(objNode.ID) = 0) Then
                objNode.ID = (objNode.ParentNameSpace & "_" & Conversions.ToString(objNode.GetHashCode))
            End If
            Return num2
        End Function

        Public Shadows Function Add(ByVal strText As String) As Integer
            Dim objNode As New TreeNode(Me.m_DNNTree)
            Dim num2 As Integer = Me.Add(objNode)
            objNode.Text = strText
            Return num2
        End Function

        Public Shadows Function Add(ByVal strID As String, ByVal strKey As String, ByVal strText As String, ByVal strNavigateURL As String) As Integer
            Return Me.Add(strID, strKey, strText, strNavigateURL, "", "", "", True, "", "", "", False, eClickAction.Navigate, "", -1)
        End Function

        Public Shadows Function Add(ByVal strID As String, ByVal strKey As String, ByVal strText As String, ByVal strNavigateURL As String, ByVal strJSFunction As String, ByVal strTarget As String, ByVal strToolTip As String, ByVal blnEnabled As Boolean, ByVal strCSSClass As String, ByVal strCSSClassSelected As String, ByVal strCSSClassHover As String, ByVal blnSelected As Boolean, ByVal enumClickAction As eClickAction, ByVal strCssClassOver As String, ByVal intImageIndex As Integer) As Integer
            Dim num2 As Integer = Me.Add
            Dim node As TreeNode = Me(num2)
            node.ID = If((Strings.Len(strID) <= 0), (node.ParentNameSpace & "_" & Conversions.ToString(Me.XMLNode.ChildNodes.Count)), strID)
            node.Key = strKey
            node.Text = strText
            node.NavigateUrl = strNavigateURL
            node.JSFunction = strJSFunction
            node.Target = strTarget
            node.ToolTip = strToolTip
            node.Enabled = blnEnabled
            node.CssClass = strCSSClass
            node.CSSClassSelected = strCSSClassSelected
            node.CSSClassHover = strCSSClassHover
            node.Selected = blnSelected
            node.ClickAction = enumClickAction
            node.CssClassOver = strCssClassOver
            node.ImageIndex = intImageIndex
            Return num2
        End Function

        Public Shadows Sub Clear()
            Dim num As Integer = (Me.XMLNode.ChildNodes.Count - 1)
            Do While True
                Dim num2 As Integer = 0
                If (num < num2) Then
                    Return
                End If
                Me.XMLNode.RemoveChild(Me.XMLNode.ChildNodes(num))
                num = (num + -1)
            Loop
        End Sub

        Public Shadows Function Contains(ByVal value As TreeNode) As Boolean
            Return Not Object.ReferenceEquals(Me.FindNode(value.ID), Nothing)
        End Function

        Public Shadows Function FindNode(ByVal ID As String) As TreeNode
            Dim objA As XmlNode = Me.FindFast("id", ID, Me.XMLNode, True)
            Return If(Object.ReferenceEquals(objA, Nothing), Nothing, New TreeNode(objA, Me.m_DNNTree))
        End Function

        Public Shadows Function FindNodeByKey(ByVal Key As String) As TreeNode
            Dim objA As XmlNode = Me.FindFast("key", Key, Me.XMLNode, True)
            Return If(Object.ReferenceEquals(objA, Nothing), Nothing, New TreeNode(objA, Me.m_DNNTree))
        End Function

        Public Overrides Function FindSelectedNodes() As Collection
			_logger.LogError(">>> Entering FindSelectedNodes()")
            Dim collection As New Collection
            Dim flag As Boolean = Not Object.ReferenceEquals(Me.XMLNode, Nothing)
            If flag Then
                Dim enumerator As IEnumerator = Nothing
                Dim list As XmlNodeList = Me.XMLNode.SelectNodes("//n[@selected='1']")
				_logger.LogError("FindSelectedNodes(): list → {List}", list)
                Try 
                    enumerator = list.GetEnumerator
					_logger.LogDebug("FindSelectedNodes(): enumerator → {Enumerator}", enumerator)
                    Do While True
                        flag = enumerator.MoveNext
						_logger.LogError("FindSelectedNodes(): flag → {Flag}", flag)
						_logger.LogDebug("FindSelectedNodes(): flag → {Flag}", flag)
                        If Not flag Then
                            Exit Do
                        End If
                        Dim current As XmlNode = DirectCast(enumerator.Current, XmlNode)
						_logger.LogError("FindSelectedNodes(): current → {Current}", current)
                        collection.Add(New TreeNode(current, Me.m_DNNTree), Nothing, Nothing, Nothing)
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
            End If
			_logger.LogWarning("<<< Exiting FindSelectedNodes()")
            Return collection
        End Function

        Public Shadows Function GetEnumerator() As IEnumerator
            Return New TreeNodeEnumerator(Me.XMLNode, Me.m_DNNTree)
        End Function

        Public Shadows Function IndexOf(ByVal value As TreeNode) As Integer
            Dim count As Integer = Me.XMLNode.ChildNodes.Count
            Dim num As Integer = 0
            Do While True
                Dim num2 As Integer
                Dim num4 As Integer = count
                If (num <= num4) Then
                    If (New TreeNode(Me.XMLNode.ChildNodes(num), Me.m_DNNTree).ID <> value.ID) Then
                        num += 1
                        Continue Do
                    End If
                    num2 = num
                End If
                Return num2
            Loop

            Return Nothing
        End Function

        Public Sub Insert(ByVal index As Integer, ByVal value As TreeNode)
			_logger.LogError(">>> Entering Insert(index={Index},value={Value})", index, value)
            Me.XMLNode.InsertAfter(Me.XMLNode.ChildNodes(index), value.XmlNode)
			_logger.LogWarning("<<< Exiting Insert(index={Index},value={Value})", index, value)
        End Sub

        Public Sub LoadViewState(ByVal state As Object) Implements IStateManager.LoadViewState
		_logger.LogError(">>> Entering LoadViewState(state={State})", state)
        End Sub

        Public Shadows Sub Remove(ByVal value As TreeNode)
            Me.XMLNode.RemoveChild(value.XmlNode)
        End Sub

        Public Function SaveViewState() As Object Implements IStateManager.SaveViewState
			_logger.LogError(">>> Entering SaveViewState()")
            Dim obj2 As Object = Nothing
			_logger.LogWarning("<<< Exiting SaveViewState()")
            Return obj2
        End Function

        Public Sub TrackViewState() Implements IStateManager.TrackViewState
		_logger.LogError(">>> Entering TrackViewState()")
        End Sub


        ' Properties
        Public Default Shadows Property Item(ByVal index As Integer) As TreeNode
            Get
                Return New TreeNode(Me.XMLNode.ChildNodes(index), Me.m_DNNTree)
            End Get
            Set(ByVal Value As TreeNode)
                Throw New NotSupportedException("Cannot Assign Node Directly")
            End Set
        End Property

        Public ReadOnly Property IsTrackingViewState As Boolean Implements IStateManager.IsTrackingViewState
            Get
                Return False
            End Get
        End Property


        ' Fields
        Private m_DNNTree As DnnTree
    End Class
End Namespace

