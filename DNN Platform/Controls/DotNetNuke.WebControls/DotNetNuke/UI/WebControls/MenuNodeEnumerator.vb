' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Xml
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class MenuNodeEnumerator
        Implements IEnumerator
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal objRoot As XmlNode, ByVal objControl As DNNMenu)
			_logger.LogError(">>> Entering New(objRoot={ObjRoot},objControl={ObjControl})", objRoot, objControl)
			_logger.LogDebug("New(objRoot={ObjRoot},objControl={ObjControl}): Me.m_objDNNMenu → {Arg2}", objRoot, objControl, Me.m_objDNNMenu)
			_logger.LogDebug("New(objRoot={ObjRoot},objControl={ObjControl}): Me.m_intCursor → {Arg2}", objRoot, objControl, Me.m_intCursor)
        _logger.LogWarning("<<< Exiting New(objRoot={ObjRoot},objControl={ObjControl})", objRoot, objControl)
            Me.m_objXMLNode = objRoot
			_logger.LogDebug("New(objRoot={ObjRoot},objControl={ObjControl}): Me.m_objXMLNode → {Arg2}", objRoot, objControl, Me.m_objXMLNode)
            Me.m_objDNNMenu = objControl
            Me.m_intCursor = -1
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
			_logger.LogError(">>> Entering MoveNext()")
            If (Me.m_intCursor < Me.m_objXMLNode.ChildNodes.Count) Then
                Me.m_intCursor += 1
            End If
			_logger.LogWarning("<<< Exiting MoveNext()")
            Return (Me.m_intCursor <> Me.m_objXMLNode.ChildNodes.Count)
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
			_logger.LogError(">>> Entering Reset()")
			_logger.LogDebug("Reset(): Me.m_intCursor → {Arg0}", Me.m_intCursor)
        _logger.LogWarning("<<< Exiting Reset()")
            Me.m_intCursor = -1
        End Sub


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

