' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.IO
Imports System.Xml

Namespace DotNetNuke.UI.WebControls
    Public MustInherit Class XmlCollectionBase
        Implements ICollection, IList, IEnumerable
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(XmlCollectionBase))
        ' Methods
        Protected Sub New()
		Logger.LogDebug(">>> Entering New()")
		Logger.LogDebug(">>> Entering New(objTreeControl={ObjTreeControl})", objTreeControl)
        End Sub

        Protected Sub New(ByVal objTreeControl As DnnTree)
			Logger.LogDebug(">>> Entering New(objTreeControl={ObjTreeControl})", objTreeControl)
            Me.m_objTree = objTreeControl
			Logger.LogDebug("New(objTreeControl={ObjTreeControl}): Me.m_objTree → {Arg1}", objTreeControl, Me.m_objTree)
			Logger.LogDebug(">>> Entering New(strNamespace={StrNamespace})", strNamespace)
			Logger.LogDebug("New(strNamespace={StrNamespace}): Me.InnerXMLDoc → {Arg1}", strNamespace, Me.InnerXMLDoc)
			Logger.LogDebug("<<< Exiting New(objTreeControl={ObjTreeControl})", objTreeControl)
			Logger.LogDebug("New(strNamespace={StrNamespace}): node.Value → {Arg1}", strNamespace, node.Value)
        End Sub

        Public Sub New(ByVal strNamespace As String)
            Me.InnerXMLDoc = New XmlDocument
            Me.InnerXMLNode = Me.InnerXMLDoc.CreateNode(XmlNodeType.Element, "root", "")
            Dim node As XmlAttribute = Me.InnerXMLDoc.CreateAttribute("id")
            node.Value = strNamespace
            Me.InnerXMLNode.Attributes.Append(node)
            Me.InnerXMLDoc.AppendChild(Me.InnerXMLNode)
			Logger.LogDebug("<<< Exiting New(strNamespace={StrNamespace})", strNamespace)
			Logger.LogDebug(">>> Entering New(objXmlNode={ObjXmlNode})", objXmlNode)
			Logger.LogDebug("New(objXmlNode={ObjXmlNode}): Me.InnerXMLDoc → {Arg1}", objXmlNode, Me.InnerXMLDoc)
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode)
            Me.InnerXMLNode = objXmlNode
            Me.InnerXMLDoc = Me.InnerXMLNode.OwnerDocument
			Logger.LogDebug("New(objXmlNode={ObjXmlNode}): Me.InnerXMLNode → {Arg1}", objXmlNode, Me.InnerXMLNode)
			Logger.LogDebug(">>> Entering New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl})", strNamespace, objTreeControl)
			Logger.LogDebug("New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl}): Me.InnerXMLDoc → {Arg2}", strNamespace, objTreeControl, Me.InnerXMLDoc)
			Logger.LogDebug("New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl}): Me.m_objTree → {Arg2}", strNamespace, objTreeControl, Me.m_objTree)
			Logger.LogDebug("<<< Exiting New(objXmlNode={ObjXmlNode})", objXmlNode)
			Logger.LogDebug("New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl}): node.Value → {Arg2}", strNamespace, objTreeControl, node.Value)
        End Sub

        Public Sub New(ByVal strNamespace As String, ByVal objTreeControl As DnnTree)
            Me.m_objTree = objTreeControl
            Me.InnerXMLDoc = New XmlDocument
            Me.InnerXMLNode = Me.InnerXMLDoc.CreateNode(XmlNodeType.Element, "root", "")
            Dim node As XmlAttribute = Me.InnerXMLDoc.CreateAttribute("id")
            node.Value = strNamespace
            Me.InnerXMLNode.Attributes.Append(node)
            Me.InnerXMLDoc.AppendChild(Me.InnerXMLNode)
			Logger.LogDebug("<<< Exiting New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl})", strNamespace, objTreeControl)
			Logger.LogDebug(">>> Entering New(strXML={StrXML},strXSLFile={StrXSLFile})", strXML, strXSLFile)
			Logger.LogDebug("New(strXML={StrXML},strXSLFile={StrXSLFile}): Me.InnerXMLDoc → {Arg2}", strXML, strXSLFile, Me.InnerXMLDoc)
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
			Logger.LogDebug("<<< Exiting New(strXML={StrXML},strXSLFile={StrXSLFile})", strXML, strXSLFile)
			Logger.LogDebug("New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl}): Me.InnerXMLNode → {Arg2}", objXmlNode, objTreeControl, Me.InnerXMLNode)
			Logger.LogDebug("New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl}): Me.InnerXMLDoc → {Arg2}", objXmlNode, objTreeControl, Me.InnerXMLDoc)
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode, ByVal objTreeControl As DnnTree)
            Me.m_objTree = objTreeControl
            Me.InnerXMLNode = objXmlNode
			Logger.LogDebug("New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl}): Me.m_objTree → {Arg2}", objXmlNode, objTreeControl, Me.m_objTree)
            Me.InnerXMLDoc = Me.InnerXMLNode.OwnerDocument
			Logger.LogDebug("<<< Exiting New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl})", objXmlNode, objTreeControl)
        End Sub

        Private Function Add(ByVal value As Object) As Integer Implements IList.Add
			Logger.LogDebug(">>> Entering Add(value={Value})", value)
            Dim num As Integer
			Logger.LogDebug("<<< Exiting Add(value={Value})", value)
            Return num
        End Function

        Public Sub Clear() Implements IList.Clear
			Logger.LogDebug(">>> Entering Clear()")
            Dim num As Integer = (Me.InnerXMLNode.ChildNodes.Count - 1)
            Do While True
                Dim num2 As Integer = 0
                If (num < num2) Then
                    Return
                End If
                Me.InnerXMLNode.RemoveChild(Me.InnerXMLNode.ChildNodes(num))
                num = (num + -1)
				Logger.LogDebug("Clear(): num → {Num}", num)
            Loop
			Logger.LogDebug("<<< Exiting Clear()")
        End Sub

        Private Function Contains(ByVal value As Object) As Boolean Implements IList.Contains
			Logger.LogDebug(">>> Entering Contains(value={Value})", value)
            Dim flag As Boolean
			Logger.LogDebug("<<< Exiting Contains(value={Value})", value)
            Return flag
        End Function

        Private Sub CopyTo(ByVal array As Array, ByVal index As Integer) Implements ICollection.CopyTo
		Logger.LogDebug(">>> Entering CopyTo(array={Array},index={Index})", array, index)
        End Sub

        Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
			Logger.LogDebug(">>> Entering GetEnumerator()")
			Logger.LogDebug("<<< Exiting GetEnumerator()")
            Return If(Not Object.ReferenceEquals(Me.m_objTree, Nothing), New TreeNodeEnumerator(Me.m_objXMLNode, Me.m_objTree), Me.m_objXMLNode.ChildNodes.GetEnumerator)
        End Function

        Private Function IndexOf(ByVal value As Object) As Integer Implements IList.IndexOf
			Logger.LogDebug(">>> Entering IndexOf(value={Value})", value)
            Dim num As Integer
			Logger.LogDebug("<<< Exiting IndexOf(value={Value})", value)
            Return num
        End Function

        Private Sub Insert(ByVal index As Integer, ByVal value As Object) Implements IList.Insert
		Logger.LogDebug(">>> Entering Insert(index={Index},value={Value})", index, value)
        End Sub

        Private Sub Remove(ByVal value As Object) Implements IList.Remove
		Logger.LogDebug(">>> Entering Remove(value={Value})", value)
        End Sub

        Public Sub RemoveAt(ByVal index As Integer) Implements IList.RemoveAt
			Logger.LogDebug(">>> Entering RemoveAt(index={Index})", index)
            Me.InnerXMLNode.RemoveChild(Me.InnerXMLNode.ChildNodes(index))
			Logger.LogDebug("<<< Exiting RemoveAt(index={Index})", index)
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

