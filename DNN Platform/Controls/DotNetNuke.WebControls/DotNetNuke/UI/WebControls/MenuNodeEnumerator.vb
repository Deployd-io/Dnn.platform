' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports System
Imports System.Collections
Imports System.Xml

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class MenuNodeEnumerator
        Implements IEnumerator
        ' Methods
        Public Sub New(ByVal objRoot As XmlNode, ByVal objControl As DNNMenu)
			Logger.LogDebug(">>> Entering New(objRoot={ObjRoot},objControl={ObjControl})", objRoot, objControl)
            Me.m_objXMLNode = objRoot
            Me.m_objDNNMenu = objControl
			Logger.LogDebug("New(objRoot={ObjRoot},objControl={ObjControl}): Me.m_objDNNMenu → {Arg2}", objRoot, objControl, Me.m_objDNNMenu)
			Logger.LogDebug("New(objRoot={ObjRoot},objControl={ObjControl}): Me.m_intCursor → {Arg2}", objRoot, objControl, Me.m_intCursor)
			Logger.LogDebug("New(objRoot={ObjRoot},objControl={ObjControl}): Me.m_objXMLNode → {Arg2}", objRoot, objControl, Me.m_objXMLNode)
            Me.m_intCursor = -1
        End Sub
		Logger.LogDebug("<<< Exiting New(objRoot={ObjRoot},objControl={ObjControl})", objRoot, objControl)

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
			Logger.LogDebug(">>> Entering MoveNext()")
            If (Me.m_intCursor < Me.m_objXMLNode.ChildNodes.Count) Then
                Me.m_intCursor += 1
            End If
            Return (Me.m_intCursor <> Me.m_objXMLNode.ChildNodes.Count)
        End Function
		Logger.LogDebug("<<< Exiting MoveNext()")

        Public Sub Reset() Implements IEnumerator.Reset
			Logger.LogDebug(">>> Entering Reset()")
            Me.m_intCursor = -1
        End Sub
		Logger.LogDebug("<<< Exiting Reset()")
		Logger.LogDebug("Reset(): Me.m_intCursor → {Arg0}", Me.m_intCursor)


        ' Properties
        Public ReadOnly Property Current As Object Implements IEnumerator.Current
            Get
                If ((Me.m_intCursor < 0) Or (Me.m_intCursor = Me.m_objXMLNode.ChildNodes.Count)) Then
                    Throw New InvalidOperationException
                End If
                Return New MenuNode(Me.m_objXMLNode.ChildNodes(Me.m_intCursor), Me.m_objDNNMenu)
            End Get
        End Property


        ' Fields
        Private m_objXMLNode As XmlNode
        Private m_objDNNMenu As DNNMenu
        Private m_intCursor As Integer
    End Class
End Namespace

