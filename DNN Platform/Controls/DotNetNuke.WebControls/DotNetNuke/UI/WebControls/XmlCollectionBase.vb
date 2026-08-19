' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.IO
Imports System.Xml
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Public MustInherit Class XmlCollectionBase
        Implements ICollection, IList, IEnumerable
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Protected Sub New()
		_logger.LogDebug(">>> Entering New()")
		_logger.LogDebug(">>> Entering New(objTreeControl={ObjTreeControl})", objTreeControl)
		_logger.LogDebug("New(objTreeControl={ObjTreeControl}): Me.m_objTree → {Arg1}", objTreeControl, Me.m_objTree)
		_logger.LogDebug("<<< Exiting New(objTreeControl={ObjTreeControl})", objTreeControl)
        End Sub

        Protected Sub New(ByVal objTreeControl As DnnTree)
			_logger.LogDebug(">>> Entering New(objTreeControl={ObjTreeControl})", objTreeControl)
            Me.m_objTree = objTreeControl
			_logger.LogDebug("<<< Exiting New(objTreeControl={ObjTreeControl})", objTreeControl)
			_logger.LogDebug(">>> Entering New(strNamespace={StrNamespace})", strNamespace)
			_logger.LogDebug("New(strNamespace={StrNamespace}): Me.InnerXMLDoc → {Arg1}", strNamespace, Me.InnerXMLDoc)
			_logger.LogDebug("New(strNamespace={StrNamespace}): node → {Node}", strNamespace, node)
			_logger.LogDebug("New(strNamespace={StrNamespace}): node.Value → {Arg1}", strNamespace, node.Value)
			_logger.LogDebug("<<< Exiting New(strNamespace={StrNamespace})", strNamespace)
        End Sub

        Public Sub New(ByVal strNamespace As String)
            Me.InnerXMLDoc = New XmlDocument
            Me.InnerXMLNode = Me.InnerXMLDoc.CreateNode(XmlNodeType.Element, "root", "")
            Dim node As XmlAttribute = Me.InnerXMLDoc.CreateAttribute("id")
            node.Value = strNamespace
            Me.InnerXMLNode.Attributes.Append(node)
            Me.InnerXMLDoc.AppendChild(Me.InnerXMLNode)
			_logger.LogDebug(">>> Entering New(objXmlNode={ObjXmlNode})", objXmlNode)
			_logger.LogDebug("New(objXmlNode={ObjXmlNode}): Me.InnerXMLNode → {Arg1}", objXmlNode, Me.InnerXMLNode)
			_logger.LogDebug("New(objXmlNode={ObjXmlNode}): Me.InnerXMLDoc → {Arg1}", objXmlNode, Me.InnerXMLDoc)
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode)
            Me.InnerXMLNode = objXmlNode
            Me.InnerXMLDoc = Me.InnerXMLNode.OwnerDocument
			_logger.LogDebug("<<< Exiting New(objXmlNode={ObjXmlNode})", objXmlNode)
			_logger.LogDebug(">>> Entering New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl})", strNamespace, objTreeControl)
			_logger.LogDebug("New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl}): Me.m_objTree → {Arg2}", strNamespace, objTreeControl, Me.m_objTree)
			_logger.LogDebug("New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl}): Me.InnerXMLDoc → {Arg2}", strNamespace, objTreeControl, Me.InnerXMLDoc)
			_logger.LogDebug("New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl}): node → {Node}", strNamespace, objTreeControl, node)
			_logger.LogDebug("New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl}): node.Value → {Arg2}", strNamespace, objTreeControl, node.Value)
			_logger.LogDebug("<<< Exiting New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl})", strNamespace, objTreeControl)
        End Sub

        Public Sub New(ByVal strNamespace As String, ByVal objTreeControl As DnnTree)
            Me.m_objTree = objTreeControl
            Me.InnerXMLDoc = New XmlDocument
            Me.InnerXMLNode = Me.InnerXMLDoc.CreateNode(XmlNodeType.Element, "root", "")
            Dim node As XmlAttribute = Me.InnerXMLDoc.CreateAttribute("id")
            node.Value = strNamespace
            Me.InnerXMLNode.Attributes.Append(node)
            Me.InnerXMLDoc.AppendChild(Me.InnerXMLNode)
        End Sub

        Public Sub New(ByVal strXML As String, ByVal strXSLFile As String)
            Me.InnerXMLDoc = New XmlDocument With { .XmlResolver = Nothing }
            If (Strings.Len(strXSLFile) <= 0) Then
                Using stringReader = New StringReader(strXML)
                Using xmlReader As XmlReader = XmlReader.Create(stringReader, New XmlReaderSettings With { .XmlResolver = Nothing })
                    Me.InnerXMLDoc.Load(xmlReader)
                End Using
                End Using
            End If
            Me.InnerXMLNode = Me.InnerXMLDoc.SelectSingleNode("//root")
			_logger.LogDebug(">>> Entering New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl})", objXmlNode, objTreeControl)
			_logger.LogDebug("New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl}): Me.m_objTree → {Arg2}", objXmlNode, objTreeControl, Me.m_objTree)
			_logger.LogDebug("New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl}): Me.InnerXMLNode → {Arg2}", objXmlNode, objTreeControl, Me.InnerXMLNode)
			_logger.LogDebug("New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl}): Me.InnerXMLDoc → {Arg2}", objXmlNode, objTreeControl, Me.InnerXMLDoc)
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode, ByVal objTreeControl As DnnTree)
            Me.m_objTree = objTreeControl
            Me.InnerXMLNode = objXmlNode
            Me.InnerXMLDoc = Me.InnerXMLNode.OwnerDocument
			_logger.LogDebug("<<< Exiting New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl})", objXmlNode, objTreeControl)
        End Sub

        Private Function Add(ByVal value As Object) As Integer Implements IList.Add
			_logger.LogDebug(">>> Entering Add(value={Value})", value)
            Dim num As Integer
			_logger.LogDebug("<<< Exiting Add(value={Value})", value)
            Return num
        End Function

        Public Sub Clear() Implements IList.Clear
			_logger.LogDebug(">>> Entering Clear()")
			_logger.LogDebug("Clear(): num → {Num}", num)
            Dim num As Integer = (Me.InnerXMLNode.ChildNodes.Count - 1)
            Do While True
                Dim num2 As Integer = 0
                If (num < num2) Then
                    Return
                End If
                Me.InnerXMLNode.RemoveChild(Me.InnerXMLNode.ChildNodes(num))
                num = (num + -1)
            Loop
			_logger.LogDebug("<<< Exiting Clear()")
        End Sub

        Private Function Contains(ByVal value As Object) As Boolean Implements IList.Contains
			_logger.LogDebug(">>> Entering Contains(value={Value})", value)
            Dim flag As Boolean
			_logger.LogDebug("<<< Exiting Contains(value={Value})", value)
            Return flag
        End Function

        Private Sub CopyTo(ByVal array As Array, ByVal index As Integer) Implements ICollection.CopyTo
		_logger.LogDebug(">>> Entering CopyTo(array={Array},index={Index})", array, index)
        End Sub

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
			_logger.LogDebug(">>> Entering GetEnumerator()")
			_logger.LogDebug("<<< Exiting GetEnumerator()")
            Return If(Not Object.ReferenceEquals(Me.m_objTree, Nothing), New TreeNodeEnumerator(Me.m_objXMLNode, Me.m_objTree), Me.m_objXMLNode.ChildNodes.GetEnumerator)
        End Function

        Private Function IndexOf(ByVal value As Object) As Integer Implements IList.IndexOf
			_logger.LogDebug(">>> Entering IndexOf(value={Value})", value)
            Dim num As Integer
			_logger.LogDebug("<<< Exiting IndexOf(value={Value})", value)
            Return num
        End Function

        Private Sub Insert(ByVal index As Integer, ByVal value As Object) Implements IList.Insert
		_logger.LogDebug(">>> Entering Insert(index={Index},value={Value})", index, value)
        End Sub

        Private Sub Remove(ByVal value As Object) Implements IList.Remove
		_logger.LogDebug(">>> Entering Remove(value={Value})", value)
        End Sub

        Public Sub RemoveAt(ByVal index As Integer) Implements IList.RemoveAt
			_logger.LogDebug(">>> Entering RemoveAt(index={Index})", index)
            Me.InnerXMLNode.RemoveChild(Me.InnerXMLNode.ChildNodes(index))
			_logger.LogDebug("<<< Exiting RemoveAt(index={Index})", index)
        End Sub


        ' Properties
        Private ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized
            Get
                Dim flag As Boolean
                Return flag
            End Get
        End Property

        Private ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
            Get
                Dim obj2 As Object = Nothing
                Return obj2
            End Get
        End Property

        Private ReadOnly Property IsFixedSize As Boolean Implements IList.IsFixedSize
            Get
                Dim flag As Boolean
                Return flag
            End Get
        End Property

        Private ReadOnly Property IsReadOnly As Boolean Implements IList.IsReadOnly
            Get
                Dim flag As Boolean
                Return flag
            End Get
        End Property

        Private Property Item(ByVal index As Integer) As Object Implements IList.Item
            Get
                Dim obj2 As Object = Nothing
                Return obj2
            End Get
            Set(ByVal Value As Object)
            End Set
        End Property

        Public ReadOnly Property Count As Integer Implements System.Collections.ICollection.Count
            Get
                Return Me.InnerXMLNode.ChildNodes.Count
            End Get
        End Property

        Protected ReadOnly Property InnerList As ArrayList
            Get
                Return Nothing
            End Get
        End Property

        Protected ReadOnly Property List As IList
            Get
                Return Nothing
            End Get
        End Property

        Protected Property InnerXMLNode As XmlNode
            Get
                Return Me.m_objXMLNode
            End Get
            Set(ByVal Value As XmlNode)
                Me.m_objXMLNode = Value
            End Set
        End Property

        Protected Property InnerXMLDoc As XmlDocument
            Get
                Return Me.m_objXMLDoc
            End Get
            Set(ByVal Value As XmlDocument)
                Me.m_objXMLDoc = Value
            End Set
        End Property

        
        ' Fields
        Private m_arr As ArrayList
        Private m_objXMLNode As XmlNode
        Private m_objXMLDoc As XmlDocument
        Private m_objTree As DnnTree
    End Class
End Namespace

