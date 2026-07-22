' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

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

Namespace DotNetNuke.UI.WebControls
    <DefaultMember("Item")> _
    Public Class DNNNodeCollection
        Inherits XmlCollectionBase
        ' Methods
        Public Sub New(ByVal strNamespace As String)
			Logger.LogDebug(">>> Entering New(strNamespace={StrNamespace})", strNamespace)
            MyBase.New(strNamespace)
        End Sub
		Logger.LogDebug("<<< Exiting New(strNamespace={StrNamespace})", strNamespace)

        Public Sub New(ByVal objXmlNode As XmlNode)
            MyBase.New(objXmlNode)
        End Sub

        Public Sub New(ByVal strNamespace As String, ByVal objTreeControl As DnnTree)
            MyBase.New(strNamespace, objTreeControl)
        End Sub

        Public Sub New(ByVal RootNamespace As String, ByVal JSONDict As Dictionary(Of String, Object))
            MyBase.New(RootNamespace)
            Me.LoadJSON(DirectCast(JSONDict("nodes"), IEnumerable))
        End Sub

        Public Sub New(ByVal strXML As String, ByVal strXSLFile As String)
            MyBase.New(strXML, strXSLFile)
        End Sub

        Public Sub New(ByVal objXmlNode As XmlNode, ByVal objTreeControl As DnnTree)
            MyBase.New(objXmlNode, objTreeControl)
        End Sub

        Public Function Add() As Integer
			Logger.LogDebug(">>> Entering Add()")
            Dim node As New DNNNode
            Me.XMLNode.AppendChild(Me.XMLDoc.ImportNode(node.XmlNode, False))
            node.ID = (node.ParentNameSpace & "_" & Conversions.ToString(Me.XMLNode.ChildNodes.Count))
            Return (Me.XMLNode.ChildNodes.Count - 1)
        End Function
		Logger.LogDebug("<<< Exiting Add()")

        Public Function Add(ByVal objNode As DNNNode) As Integer
            Dim newChild As XmlNode = Me.XMLDoc.ImportNode(objNode.XmlNode, True)
            Me.XMLNode.AppendChild(newChild)
            objNode.AssociateXmlNode(newChild)
            Return (Me.XMLNode.ChildNodes.Count - 1)
        End Function

        Public Function Add(ByVal strID As String, ByVal strKey As String, ByVal strText As String, ByVal strNavigateURL As String, ByVal strJSFunction As String, ByVal strTarget As String, ByVal strToolTip As String, ByVal blnEnabled As Boolean, ByVal strCSSClass As String, ByVal strCSSClassSelected As String, ByVal strCSSClassHover As String) As Integer
            Dim num2 As Integer = Me.Add
            Dim node As DNNNode = Me(num2)
            If (Strings.Len(strID) > 0) Then
                node.ID = strID
				Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.ID → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.ID)
            End If
            node.Key = strKey
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.Key → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.Key)
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.Text → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.Text)
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.NavigateURL → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.NavigateURL)
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.JSFunction → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.JSFunction)
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.Target → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.Target)
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.ToolTip → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.ToolTip)
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.Enabled → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.Enabled)
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.CSSClass → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.CSSClass)
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.CSSClassSelected → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.CSSClassSelected)
			Logger.LogDebug("Add(strID={StrID},strKey={StrKey},strText={StrText},strNavigateURL={StrNavigateURL},strJSFunction={StrJSFunction},strTarget={StrTarget},strToolTip={StrToolTip},blnEnabled={BlnEnabled},strCSSClass={StrCSSClass},strCSSClassSelected={StrCSSClassSelected},strCSSClassHover={StrCSSClassHover}): node.CSSClassHover → {Arg11}", strID, strKey, strText, strNavigateURL, strJSFunction, strTarget, strToolTip, blnEnabled, strCSSClass, strCSSClassSelected, strCSSClassHover, node.CSSClassHover)
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
			Logger.LogDebug(">>> Entering AddBreak()")
            Dim num2 As Integer = Me.Add
            Me(num2).IsBreak = True
            Return num2
        End Function
		Logger.LogDebug("<<< Exiting AddBreak()")

        Public Function Contains(ByVal value As DNNNode) As Boolean
			Logger.LogDebug(">>> Entering Contains(value={Value})", value)
            Return Not Object.ReferenceEquals(Me.FindNode(value.ID), Nothing)
        End Function
		Logger.LogDebug("<<< Exiting Contains(value={Value})", value)

        Public Sub CopyTo(ByVal myArr As Array, ByVal index As Integer)
			Logger.LogDebug(">>> Entering CopyTo(myArr={MyArr},index={Index})", myArr, index)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = MyBase.InnerXMLNode.ChildNodes.GetEnumerator
				Logger.LogDebug("CopyTo(myArr={MyArr},index={Index}): enumerator → {Enumerator}", myArr, index, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As XmlNode = DirectCast(enumerator.Current, XmlNode)
                    myArr.SetValue(current, index)
                    index += 1
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub
		Logger.LogDebug("<<< Exiting CopyTo(myArr={MyArr},index={Index})", myArr, index)

        Private Function DoTransform(ByVal XML As String, ByVal XSL As String) As String
			Logger.LogDebug(">>> Entering DoTransform(XML={XML},XSL={XSL})", XML, XSL)
            Return Me.DoTransform(XML, XSL, Nothing)
        End Function

        Private Function DoTransform(ByVal XML As String, ByVal XSL As String, ByVal Params As XsltArgumentList) As String
            Dim str As String
			Logger.LogDebug("<<< Exiting DoTransform(XML={XML},XSL={XSL})", XML, XSL)
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
				Logger.LogDebug("DoTransform(XML={XML},XSL={XSL},Params={Params}): str → {Str}", XML, XSL, Params, str)
            Catch exception1 As Exception
                Dim ex As Exception = exception1
				Logger.LogError("Exception in DoTransform(XML={XML},XSL={XSL},Params={Params}): {Arg3}", XML, XSL, Params, ex.Message)
                ProjectData.SetProjectError(ex)
                Throw ex
            End Try
            Return str
        End Function

        Friend Function FindFast(ByVal Key As String, ByVal Value As String, ByVal Parent As XmlNode, ByVal OptimizedForSmallData As Boolean) As XmlNode
			Logger.LogDebug(">>> Entering FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData})", Key, Value, Parent, OptimizedForSmallData)
            Dim node As XmlNode = Nothing
            Try 
                If Not OptimizedForSmallData Then
                    node = Parent.SelectSingleNode($".//n[@{Key}='{Value}']")
					Logger.LogDebug("FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData}): node → {Node}", Key, Value, Parent, OptimizedForSmallData, node)
                Else
                    Dim firstChild As XmlNode = Parent.FirstChild
                    Do While True
                        If Object.ReferenceEquals(firstChild, Nothing) Then
                            node = Nothing
                        Else
                            If (If(((firstChild.Attributes(Key) Is Nothing) OrElse (firstChild.Attributes(Key).Value <> Value)), 0, 1) = 0) Then
                                If Parent.HasChildNodes Then
                                    Dim objA As XmlNode = Me.FindFast(Key, Value, firstChild, True)
                                    If Not Object.ReferenceEquals(objA, Nothing) Then
                                        node = objA
                                        Exit Do
                                    End If
                                End If
                                firstChild = firstChild.NextSibling
								Logger.LogDebug("FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData}): firstChild → {FirstChild}", Key, Value, Parent, OptimizedForSmallData, firstChild)
                                Continue Do
                            End If
                            node = firstChild
                        End If
                        Exit Do
                    Loop
                End If
            Catch exception1 As Exception
                Dim ex As Exception = exception1
				Logger.LogError("Exception in FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData}): {Arg4}", Key, Value, Parent, OptimizedForSmallData, ex.Message)
                ProjectData.SetProjectError(ex)
                Throw ex
            End Try
            Return node
        End Function
		Logger.LogDebug("<<< Exiting FindFast(Key={Key},Value={Value},Parent={Parent},OptimizedForSmallData={OptimizedForSmallData})", Key, Value, Parent, OptimizedForSmallData)

        Public Function FindNode(ByVal ID As String) As DNNNode
			Logger.LogDebug(">>> Entering FindNode(ID={ID})", ID)
            Dim objA As XmlNode = Me.FindFast("id", ID, Me.XMLNode, True)
            Return If(Object.ReferenceEquals(objA, Nothing), Nothing, New DNNNode(objA))
        End Function
		Logger.LogDebug("<<< Exiting FindNode(ID={ID})", ID)

        Public Function FindNodeByKey(ByVal Key As String) As DNNNode
			Logger.LogDebug(">>> Entering FindNodeByKey(Key={Key})", Key)
            Dim objA As XmlNode = Me.FindFast("key", Key, Me.XMLNode, True)
            Return If(Object.ReferenceEquals(objA, Nothing), Nothing, New DNNNode(objA))
        End Function
		Logger.LogDebug("<<< Exiting FindNodeByKey(Key={Key})", Key)

        Public Overridable Function FindSelectedNodes() As Collection
			Logger.LogDebug(">>> Entering FindSelectedNodes()")
            Dim collection As New Collection
            Dim flag As Boolean = Not Object.ReferenceEquals(Me.XMLNode, Nothing)
            If flag Then
                Dim enumerator As IEnumerator = Nothing
                Dim list As XmlNodeList = Me.XMLNode.SelectNodes("//n[@selected='1']")
                Try 
                    enumerator = list.GetEnumerator
					Logger.LogDebug("FindSelectedNodes(): enumerator → {Enumerator}", enumerator)
                    Do While True
                        flag = enumerator.MoveNext
						Logger.LogDebug("FindSelectedNodes(): flag → {Flag}", flag)
                        If Not flag Then
                            Exit Do
                        End If
                        Dim current As XmlNode = DirectCast(enumerator.Current, XmlNode)
                        collection.Add(New DNNNode(current), Nothing, Nothing, Nothing)
                    Loop
                Finally
                    If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                        TryCast(enumerator,IDisposable).Dispose
                    End If
                End Try
            End If
            Return collection
        End Function
		Logger.LogDebug("<<< Exiting FindSelectedNodes()")

        Public Shadows Function GetEnumerator() As IEnumerator
            Return New DNNNodeEnumerator(MyBase.InnerXMLNode)
        End Function

        Public Function Import(ByVal objNode As DNNNode) As Integer
			Logger.LogDebug(">>> Entering Import(objNode={ObjNode})", objNode)
            Return Me.Import(objNode, True)
        End Function
		Logger.LogDebug("<<< Exiting Import(objNode={ObjNode})", objNode)

        Public Function Import(ByVal objNode As DNNNode, ByVal blnDeep As Boolean) As Integer
            Dim newChild As XmlNode = Me.XMLDoc.ImportNode(objNode.XmlNode, blnDeep)
            Me.XMLNode.AppendChild(newChild)
            Return (Me.XMLNode.ChildNodes.Count - 1)
        End Function

        Public Function IndexOf(ByVal value As DNNNode) As Integer
			Logger.LogDebug(">>> Entering IndexOf(value={Value})", value)
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
					Logger.LogDebug("IndexOf(value={Value}): num2 → {Num2}", value, num2)
                End If
                Return num2
            Loop

            Return Nothing
        End Function
		Logger.LogDebug("<<< Exiting IndexOf(value={Value})", value)

        Public Sub InsertAfter(ByVal index As Integer, ByVal value As DNNNode)
			Logger.LogDebug(">>> Entering InsertAfter(index={Index},value={Value})", index, value)
            Dim newChild As XmlNode = Me.XMLDoc.ImportNode(value.XmlNode, True)
            Me.XMLNode.InsertAfter(newChild, Me.XMLNode.ChildNodes(index))
        End Sub
		Logger.LogDebug("<<< Exiting InsertAfter(index={Index},value={Value})", index, value)

        Public Sub InsertBefore(ByVal index As Integer, ByVal value As DNNNode)
			Logger.LogDebug(">>> Entering InsertBefore(index={Index},value={Value})", index, value)
            Dim newChild As XmlNode = Me.XMLDoc.ImportNode(value.XmlNode, True)
            Me.XMLNode.InsertBefore(newChild, Me.XMLNode.ChildNodes(index))
        End Sub
		Logger.LogDebug("<<< Exiting InsertBefore(index={Index},value={Value})", index, value)

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
        End Sub

        Public Sub LoadJSON(ByVal JSONNodes As IEnumerable)
			Logger.LogDebug(">>> Entering LoadJSON(JSONNodes={JSONNodes})", JSONNodes)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = JSONNodes.GetEnumerator
				Logger.LogDebug("LoadJSON(JSONNodes={JSONNodes}): enumerator → {Enumerator}", JSONNodes, enumerator)
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
		Logger.LogDebug("<<< Exiting LoadJSON(JSONNodes={JSONNodes})", JSONNodes)

        Public Sub Remove(ByVal value As DNNNode)
			Logger.LogDebug(">>> Entering Remove(value={Value})", value)
            Me.XMLNode.RemoveChild(value.XmlNode)
        End Sub
		Logger.LogDebug("<<< Exiting Remove(value={Value})", value)

        Public Sub Remove(ByVal index As Integer)
            Me.XMLNode.RemoveChild(Me.XMLNode.ChildNodes(index))
        End Sub

        Public Shadows Sub RemoveAt(ByVal index As Integer)
            Me.Remove(index)
        End Sub

        Public Function ToJSON() As String
			Logger.LogDebug(">>> Entering ToJSON()")
            Dim enumerator As IEnumerator = Nothing
            Dim builder As New StringBuilder
            builder.Append("nodes:[")
            Try 
                enumerator = Me.GetEnumerator
				Logger.LogDebug("ToJSON(): enumerator → {Enumerator}", enumerator)
                Do While True
                    Dim flag As Boolean
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNNode = DirectCast(enumerator.Current, DNNNode)
                    If flag Then
                        builder.Append(","c)
                    End If
                    flag = True
					Logger.LogDebug("ToJSON(): flag → {Flag}", flag)
                    builder.Append(current.ToJSON(True))
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            builder.Append("]"c)
            Return builder.ToString
        End Function
		Logger.LogDebug("<<< Exiting ToJSON()")

        Public Function ToXml() As String
			Logger.LogDebug(">>> Entering ToXml()")
            Return Me.XMLDoc.OuterXml
        End Function
		Logger.LogDebug("<<< Exiting ToXml()")


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

