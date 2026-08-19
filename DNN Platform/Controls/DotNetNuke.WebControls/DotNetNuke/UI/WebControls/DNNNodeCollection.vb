' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Xml
Imports System.Xml.XPath
Imports System.Xml.Xsl
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class DNNNodeCollection
        Inherits XmlCollectionBase
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal strNamespace As String)
			_logger.LogDebug(">>> Entering New(strNamespace={StrNamespace})", strNamespace)
            MyBase.New(strNamespace)
			_logger.LogDebug("<<< Exiting New(strNamespace={StrNamespace})", strNamespace)
			_logger.LogDebug(">>> Entering New(objXmlNode={ObjXmlNode})", objXmlNode)
			_logger.LogDebug("<<< Exiting New(objXmlNode={ObjXmlNode})", objXmlNode)
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode)
            MyBase.New(objXmlNode)
			_logger.LogDebug(">>> Entering New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl})", strNamespace, objTreeControl)
			_logger.LogDebug("<<< Exiting New(strNamespace={StrNamespace},objTreeControl={ObjTreeControl})", strNamespace, objTreeControl)
        End Sub

        Public Sub New(ByVal strNamespace As String, ByVal objTreeControl As DnnTree)
            MyBase.New(strNamespace, objTreeControl)
			_logger.LogDebug(">>> Entering New(RootNamespace={RootNamespace},JSONDict={JSONDict},Object={Object})", RootNamespace, JSONDict, Object)
			_logger.LogDebug("<<< Exiting New(RootNamespace={RootNamespace},JSONDict={JSONDict},Object={Object})", RootNamespace, JSONDict, Object)
        End Sub

        Public Sub New(ByVal RootNamespace As String, ByVal JSONDict As Dictionary(Of String, Object))
            MyBase.New(RootNamespace)
            Me.LoadJSON(DirectCast(JSONDict("nodes"), IEnumerable))
			_logger.LogDebug(">>> Entering New(strXML={StrXML},strXSLFile={StrXSLFile})", strXML, strXSLFile)
			_logger.LogDebug("<<< Exiting New(strXML={StrXML},strXSLFile={StrXSLFile})", strXML, strXSLFile)
        End Sub

        Public Sub New(ByVal strXML As String, ByVal strXSLFile As String)
            MyBase.New(strXML, strXSLFile)
			_logger.LogDebug(">>> Entering New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl})", objXmlNode, objTreeControl)
			_logger.LogDebug("<<< Exiting New(objXmlNode={ObjXmlNode},objTreeControl={ObjTreeControl})", objXmlNode, objTreeControl)
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode, ByVal objTreeControl As DnnTree)
            MyBase.New(objXmlNode, objTreeControl)
        End Sub

        Public Function Add() As Integer
			_logger.LogDebug(">>> Entering Add()")
            Dim node As New DNNNode
            Me.XMLNode.AppendChild(Me.XMLDoc.ImportNode(node.XmlNode, False))
            node.ID = (node.ParentNameSpace & "_" & Conversions.ToString(Me.XMLNode.ChildNodes.Count))
			_logger.LogDebug(">>> Entering Add(objNode={ObjNode})", objNode)
			_logger.LogDebug("<<< Exiting Add()")
			_logger.LogDebug("<<< Exiting Add(objNode={ObjNode})", objNode)
			_logger.LogDebug("<<< Exiting Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover})", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover)
            Return (Me.XMLNode.ChildNodes.Count - 1)
        End Function

        Public Function Add(ByVal objNode As DNNNode) As Integer
            Dim newChild As XmlNode = Me.XMLDoc.ImportNode(objNode.XmlNode, True)
            Me.XMLNode.AppendChild(newChild)
            objNode.AssociateXmlNode(newChild)
			_logger.LogDebug(">>> Entering Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover})", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover)
            Return (Me.XMLNode.ChildNodes.Count - 1)
        End Function

        Public Function Add(ByVal strID As String, ByVal strKey As String, ByVal strText As String, ByVal strNavigateURL As String, ByVal strJSFunction As String, ByVal strTarget As String, ByVal strToolTip As String, ByVal blnEnabled As Boolean, ByVal strCSSClass As String, ByVal strCSSClassSelected As String, ByVal strCSSClassHover As String) As Integer
            Dim num2 As Integer = Me.Add
            Dim node As DNNNode = Me(num2)
            If (Strings.Len(strID) > 0) Then
                node.ID = strID
            End If
            node.Key = strKey
            node.Text = strText
            node.NavigateURL = strNavigateURL
            node.JSFunction = strJSFunction
            node.Target = strTarget
            node.ToolTip = strToolTip
            node.Enabled = blnEnabled
            node.CSSClass = strCSSClass
            node.CSSClassSelected = strCSSClassSelected
            node.CSSClassHover = strCSSClassHover
            Return num2
        End Function

        Public Function AddBreak() As Integer
			_logger.LogDebug(">>> Entering AddBreak()")
            Dim num2 As Integer = Me.Add
            Me(num2).IsBreak = True
			_logger.LogDebug("<<< Exiting AddBreak()")
            Return num2
        End Function

        Public Function Contains(ByVal value As DNNNode) As Boolean
			_logger.LogDebug(">>> Entering Contains(value={Value})", value)
			_logger.LogDebug("<<< Exiting Contains(value={Value})", value)
            Return Not Object.ReferenceEquals(Me.FindNode(value.ID), Nothing)
        End Function

        Public Sub CopyTo(ByVal myArr As Array, ByVal index As Integer)
			_logger.LogDebug(">>> Entering CopyTo(myArr={MyArr},index={Index})", myArr, index)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = MyBase.InnerXMLNode.ChildNodes.GetEnumerator
				_logger.LogDebug("CopyTo(myArr={MyArr},index={Index}): enumerator → {Enumerator}", myArr, index, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As XmlNode = DirectCast(enumerator.Current, XmlNode)
					_logger.LogDebug("CopyTo(myArr={MyArr},index={Index}): current → {Current}", myArr, index, current)
                    myArr.SetValue(current, index)
                    index += 1
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
			_logger.LogDebug("<<< Exiting CopyTo(myArr={MyArr},index={Index})", myArr, index)
        End Sub

        Private Function DoTransform(ByVal XML As String, ByVal XSL As String) As String
			_logger.LogDebug(">>> Entering DoTransform(XML={XML},XSL={XSL})", XML, XSL)
			_logger.LogDebug(">>> Entering DoTransform(XML={XML},XSL={XSL},Params={Params})", XML, XSL, Params)
			_logger.LogDebug("<<< Exiting DoTransform(XML={XML},XSL={XSL})", XML, XSL)
			_logger.LogDebug("<<< Exiting DoTransform(XML={XML},XSL={XSL},Params={Params})", XML, XSL, Params)
            Return Me.DoTransform(XML, XSL, Nothing)
        End Function

        Private Function DoTransform(ByVal XML As String, ByVal XSL As String, ByVal Params As XsltArgumentList) As String
            Dim str As String
            Try 
                Dim transform As New XslCompiledTransform
                transform.Load(XSL)
                Dim sb As New StringBuilder
                Using writer As New StringWriter(sb, Nothing)
                Using xmlWriter As XmlWriter = XmlWriter.Create(writer)
                Using reader As New StringReader(XML)
                Using xmlReader As XmlReader = XmlReader.Create(reader, New XmlReaderSettings With { .XmlResolver = Nothing })
                    transform.Transform(xmlReader, Params, xmlWriter, New XmlUrlResolver)
                    writer.Close
                End Using
                End Using
                End Using
                End Using
                str = sb.ToString
            Catch exception1 As Exception
                Dim ex As Exception = exception1
                ProjectData.SetProjectError(ex)
                Throw ex
            End Try
            Return str
        End Function

        Friend Function FindFast(ByVal Key As String, ByVal Value As String, ByVal Parent As XmlNode, ByVal OptimizedForSmallData As Boolean) As XmlNode
			_logger.LogDebug(">>> Entering FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData})", Key, Value, Parent, OptimizedForSmallData)
            Dim node As XmlNode = Nothing
            Try 
                If Not OptimizedForSmallData Then
                    node = Parent.SelectSingleNode($".//n[@{Key}='{Value}']")
					_logger.LogDebug("FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData}): node → {Node}", Key, Value, Parent, OptimizedForSmallData, node)
                Else
                    Dim firstChild As XmlNode = Parent.FirstChild
                    Do While True
                        If Object.ReferenceEquals(firstChild, Nothing) Then
                            node = Nothing
                        Else
                            If (If(((firstChild.Attributes(Key) Is Nothing) OrElse (firstChild.Attributes(Key).Value <> Value)), 0, 1) = 0) Then
                                If Parent.HasChildNodes Then
                                    Dim objA As XmlNode = Me.FindFast(Key, Value, firstChild, True)
									_logger.LogDebug("FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData}): objA → {ObjA}", Key, Value, Parent, OptimizedForSmallData, objA)
                                    If Not Object.ReferenceEquals(objA, Nothing) Then
                                        node = objA
                                        Exit Do
                                    End If
                                End If
                                firstChild = firstChild.NextSibling
								_logger.LogDebug("FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData}): firstChild → {FirstChild}", Key, Value, Parent, OptimizedForSmallData, firstChild)
                                Continue Do
                            End If
                            node = firstChild
                        End If
                        Exit Do
                    Loop
                End If
            Catch exception1 As Exception
				_logger.LogError("Exception in FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData}): {Arg4}", Key, Value, Parent, OptimizedForSmallData, exception1.getMessage())
                Dim ex As Exception = exception1
                ProjectData.SetProjectError(ex)
                Throw ex
            End Try
			_logger.LogDebug("<<< Exiting FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData})", Key, Value, Parent, OptimizedForSmallData)
            Return node
        End Function

        Public Function FindNode(ByVal ID As String) As DNNNode
			_logger.LogDebug(">>> Entering FindNode(ID={ID})", ID)
			_logger.LogDebug("FindNode(ID={ID}): objA → {ObjA}", ID, objA)
            Dim objA As XmlNode = Me.FindFast("id", ID, Me.XMLNode, True)
			_logger.LogDebug("<<< Exiting FindNode(ID={ID})", ID)
            Return If(Object.ReferenceEquals(objA, Nothing), Nothing, New DNNNode(objA))
        End Function

        Public Function FindNodeByKey(ByVal Key As String) As DNNNode
			_logger.LogDebug(">>> Entering FindNodeByKey(Key={Key})", Key)
			_logger.LogDebug("FindNodeByKey(Key={Key}): objA → {ObjA}", Key, objA)
            Dim objA As XmlNode = Me.FindFast("key", Key, Me.XMLNode, True)
			_logger.LogDebug("<<< Exiting FindNodeByKey(Key={Key})", Key)
            Return If(Object.ReferenceEquals(objA, Nothing), Nothing, New DNNNode(objA))
        End Function

        Public Overridable Function FindSelectedNodes() As Collection
			_logger.LogDebug(">>> Entering FindSelectedNodes()")
            Dim collection As New Collection
            Dim flag As Boolean = Not Object.ReferenceEquals(Me.XMLNode, Nothing)
			_logger.LogDebug("FindSelectedNodes(): flag → {Flag}", flag)
            If flag Then
                Dim enumerator As IEnumerator = Nothing
                Dim list As XmlNodeList = Me.XMLNode.SelectNodes("//n[@selected='1']")
				_logger.LogDebug("FindSelectedNodes(): list → {List}", list)
                Try 
                    enumerator = list.GetEnumerator
					_logger.LogDebug("FindSelectedNodes(): enumerator → {Enumerator}", enumerator)
                    Do While True
                        flag = enumerator.MoveNext
                        If Not flag Then
                            Exit Do
                        End If
                        Dim current As XmlNode = DirectCast(enumerator.Current, XmlNode)
						_logger.LogDebug("FindSelectedNodes(): current → {Current}", current)
                        collection.Add(New DNNNode(current), Nothing, Nothing, Nothing)
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
            End If
			_logger.LogDebug("<<< Exiting FindSelectedNodes()")
            Return collection
        End Function

        Public Shadows Function GetEnumerator() As IEnumerator
            Return New DNNNodeEnumerator(MyBase.InnerXMLNode)
        End Function

        Public Function Import(ByVal objNode As DNNNode) As Integer
			_logger.LogDebug(">>> Entering Import(objNode={ObjNode})", objNode)
			_logger.LogDebug(">>> Entering Import(objNode={ObjNode},blnDeep={BlnDeep})", objNode, blnDeep)
			_logger.LogDebug("<<< Exiting Import(objNode={ObjNode})", objNode)
			_logger.LogDebug("<<< Exiting Import(objNode={ObjNode},blnDeep={BlnDeep})", objNode, blnDeep)
            Return Me.Import(objNode, True)
        End Function

        Public Function Import(ByVal objNode As DNNNode, ByVal blnDeep As Boolean) As Integer
            Dim newChild As XmlNode = Me.XMLDoc.ImportNode(objNode.XmlNode, blnDeep)
            Me.XMLNode.AppendChild(newChild)
            Return (Me.XMLNode.ChildNodes.Count - 1)
        End Function

        Public Function IndexOf(ByVal value As DNNNode) As Integer
			_logger.LogDebug(">>> Entering IndexOf(value={Value})", value)
			_logger.LogDebug("IndexOf(value={Value}): num3 → {Num3}", value, num3)
            Dim num3 As Integer = (Me.XMLNode.ChildNodes.Count - 1)
            Dim num As Integer = 0
            Do While True
                Dim num2 As Integer
                Dim num4 As Integer = num3
                If (num <= num4) Then
                    If (New DNNNode(Me.XMLNode.ChildNodes(num)).ID <> value.ID) Then
                        num += 1
                        Continue Do
                    End If
                    num2 = num
					_logger.LogDebug("IndexOf(value={Value}): num2 → {Num2}", value, num2)
                End If
                Return num2
            Loop

			_logger.LogDebug("<<< Exiting IndexOf(value={Value})", value)
            Return Nothing
        End Function

        Public Sub InsertAfter(ByVal index As Integer, ByVal value As DNNNode)
			_logger.LogDebug(">>> Entering InsertAfter(index={Index},value={Value})", index, value)
			_logger.LogDebug("InsertAfter(index={Index},value={Value}): newChild → {NewChild}", index, value, newChild)
            Dim newChild As XmlNode = Me.XMLDoc.ImportNode(value.XmlNode, True)
            Me.XMLNode.InsertAfter(newChild, Me.XMLNode.ChildNodes(index))
			_logger.LogDebug("<<< Exiting InsertAfter(index={Index},value={Value})", index, value)
        End Sub

        Public Sub InsertBefore(ByVal index As Integer, ByVal value As DNNNode)
			_logger.LogDebug(">>> Entering InsertBefore(index={Index},value={Value})", index, value)
			_logger.LogDebug("InsertBefore(index={Index},value={Value}): newChild → {NewChild}", index, value, newChild)
            Dim newChild As XmlNode = Me.XMLDoc.ImportNode(value.XmlNode, True)
            Me.XMLNode.InsertBefore(newChild, Me.XMLNode.ChildNodes(index))
			_logger.LogDebug("<<< Exiting InsertBefore(index={Index},value={Value})", index, value)
        End Sub

        <Obsolete("Use LoadJSON IEnumerable instead")> _
        Public Sub LoadJSON(ByVal JSONNodes As ArrayList)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = JSONNodes.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As Dictionary(Of String, Object) = DirectCast(enumerator.Current, Dictionary(Of String, Object))
                    Dim objNode As New DNNNode
                    Dim enumerator2 = current.Keys.GetEnumerator
                    Do While True
                        If Not enumerator2.MoveNext Then
                            Exit Do
                        End If
                        Dim str As String = enumerator2.Current
                        If (str = "nodes") Then
                            objNode.DNNNodes.LoadJSON(DirectCast(current("nodes"), IEnumerable))
                            Continue Do
                        End If
                        objNode.CustomAttribute(str) = current(str).ToString
                    Loop
                    Me.Add(objNode)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
			_logger.LogDebug(">>> Entering LoadJSON(JSONNodes={JSONNodes})", JSONNodes)
			_logger.LogDebug("LoadJSON(JSONNodes={JSONNodes}): enumerator → {Enumerator}", JSONNodes, enumerator)
			_logger.LogDebug("<<< Exiting LoadJSON(JSONNodes={JSONNodes})", JSONNodes)
        End Sub

        Public Sub LoadJSON(ByVal JSONNodes As IEnumerable)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = JSONNodes.GetEnumerator
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As Dictionary(Of String, Object) = DirectCast(enumerator.Current, Dictionary(Of String, Object))
                    Dim objNode As New DNNNode
                    Dim enumerator2 = current.Keys.GetEnumerator
                    Do While True
                        If Not enumerator2.MoveNext Then
                            Exit Do
                        End If
                        Dim str As String = enumerator2.Current
                        If (str = "nodes") Then
                            objNode.DNNNodes.LoadJSON(DirectCast(current("nodes"), IEnumerable))
                            Continue Do
                        End If
                        objNode.CustomAttribute(str) = current(str).ToString
                    Loop
                    Me.Add(objNode)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub

        Public Sub Remove(ByVal value As DNNNode)
			_logger.LogDebug(">>> Entering Remove(value={Value})", value)
            Me.XMLNode.RemoveChild(value.XmlNode)
			_logger.LogDebug("<<< Exiting Remove(value={Value})", value)
			_logger.LogDebug(">>> Entering Remove(index={Index})", index)
			_logger.LogDebug("<<< Exiting Remove(index={Index})", index)
        End Sub

        Public Sub Remove(ByVal index As Integer)
            Me.XMLNode.RemoveChild(Me.XMLNode.ChildNodes(index))
        End Sub

        Public Shadows Sub RemoveAt(ByVal index As Integer)
            Me.Remove(index)
        End Sub

        Public Function ToJSON() As String
			_logger.LogDebug(">>> Entering ToJSON()")
            Dim enumerator As IEnumerator = Nothing
            Dim builder As New StringBuilder
            builder.Append("nodes:[")
            Try 
                enumerator = Me.GetEnumerator
				_logger.LogDebug("ToJSON(): enumerator → {Enumerator}", enumerator)
                Do While True
                    Dim flag As Boolean
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNNode = DirectCast(enumerator.Current, DNNNode)
					_logger.LogDebug("ToJSON(): current → {Current}", current)
                    If flag Then
                        builder.Append(","c)
                    End If
                    flag = True
					_logger.LogDebug("ToJSON(): flag → {Flag}", flag)
                    builder.Append(current.ToJSON(True))
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            builder.Append("]"c)
			_logger.LogDebug("<<< Exiting ToJSON()")
            Return builder.ToString
        End Function

        Public Function ToXml() As String
			_logger.LogDebug(">>> Entering ToXml()")
			_logger.LogDebug("<<< Exiting ToXml()")
            Return Me.XMLDoc.OuterXml
        End Function


        ' Properties
        Public ReadOnly Property XMLNode As XmlNode
            Get
                Return MyBase.InnerXMLNode
            End Get
        End Property

        Public ReadOnly Property XMLDoc As XmlDocument
            Get
                Return MyBase.InnerXMLDoc
            End Get
        End Property

        Public Default Property Item(ByVal index As Integer) As DNNNode
            Get
                Return New DNNNode(MyBase.InnerXMLNode.ChildNodes(index))
            End Get
            Set(ByVal Value As DNNNode)
                Throw New NotSupportedException("Cannot Assign Node Directly")
            End Set
        End Property

        Public ReadOnly Shadows Property Count As Integer
            Get
                Return MyBase.InnerXMLNode.ChildNodes.Count
            End Get
        End Property

    End Class
End Namespace

