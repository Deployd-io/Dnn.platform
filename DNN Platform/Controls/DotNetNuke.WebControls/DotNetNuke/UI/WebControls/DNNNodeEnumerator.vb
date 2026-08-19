' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Xml
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class DNNNodeEnumerator
        Implements IEnumerator
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal objRoot As XmlNode)
			_logger.LogDebug(">>> Entering New(objRoot={ObjRoot})", objRoot)
			_logger.LogDebug("New(objRoot={ObjRoot}): Me.m_objXMLNode → {Arg1}", objRoot, Me.m_objXMLNode)
			_logger.LogDebug("New(objRoot={ObjRoot}): Me.m_intCursor → {Arg1}", objRoot, Me.m_intCursor)
            Me.m_objXMLNode = objRoot
            Me.m_intCursor = -1
			_logger.LogDebug("<<< Exiting New(objRoot={ObjRoot})", objRoot)
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
			_logger.LogDebug(">>> Entering MoveNext()")
            If (Me.m_intCursor < Me.m_objXMLNode.ChildNodes.Count) Then
                Me.m_intCursor += 1
            End If
			_logger.LogDebug("<<< Exiting MoveNext()")
            Return (Me.m_intCursor <> Me.m_objXMLNode.ChildNodes.Count)
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
			_logger.LogDebug(">>> Entering Reset()")
			_logger.LogDebug("Reset(): Me.m_intCursor → {Arg0}", Me.m_intCursor)
            Me.m_intCursor = -1
			_logger.LogDebug("<<< Exiting Reset()")
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

