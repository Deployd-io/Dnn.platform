' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Xml

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNNodeEnumerator
        Implements IEnumerator
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(DNNNodeEnumerator))
        ' Methods
        Public Sub New(ByVal objRoot As XmlNode)
			Logger.LogDebug(">>> Entering New(objRoot={ObjRoot})", objRoot)
            Me.m_objXMLNode = objRoot
            Me.m_intCursor = -1
			Logger.LogDebug("New(objRoot={ObjRoot}): Me.m_intCursor → {Arg1}", objRoot, Me.m_intCursor)
			Logger.LogDebug("New(objRoot={ObjRoot}): Me.m_objXMLNode → {Arg1}", objRoot, Me.m_objXMLNode)
			Logger.LogDebug("<<< Exiting New(objRoot={ObjRoot})", objRoot)
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
			Logger.LogDebug(">>> Entering MoveNext()")
            If (Me.m_intCursor < Me.m_objXMLNode.ChildNodes.Count) Then
                Me.m_intCursor += 1
            End If
			Logger.LogDebug("<<< Exiting MoveNext()")
            Return (Me.m_intCursor <> Me.m_objXMLNode.ChildNodes.Count)
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
			Logger.LogDebug(">>> Entering Reset()")
            Me.m_intCursor = -1
			Logger.LogDebug("<<< Exiting Reset()")
			Logger.LogDebug("Reset(): Me.m_intCursor → {Arg0}", Me.m_intCursor)
        End Sub


        ' Properties
        Public ReadOnly Property Current As Object Implements IEnumerator.Current
            Get
                If ((Me.m_intCursor < 0) Or (Me.m_intCursor = Me.m_objXMLNode.ChildNodes.Count)) Then
                    Throw New InvalidOperationException
                End If
                Return New DNNNode(Me.m_objXMLNode.ChildNodes(Me.m_intCursor))
            End Get
        End Property


        ' Fields
        Private m_objXMLNode As XmlNode
        Private m_intCursor As Integer
    End Class
End Namespace

