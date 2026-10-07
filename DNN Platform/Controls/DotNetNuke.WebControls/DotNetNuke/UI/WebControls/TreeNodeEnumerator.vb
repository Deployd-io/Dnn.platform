' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Xml
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class TreeNodeEnumerator
        Implements IEnumerator
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New(ByVal objRoot As XmlNode, ByVal objControl As DnnTree)
			_logger.LogError(">>> Entering New(objRoot={ObjRoot},objControl={ObjControl})", objRoot, objControl)
			_logger.LogDebug("New(objRoot={ObjRoot},objControl={ObjControl}): Me.m_DNNTree → {Arg2}", objRoot, objControl, Me.m_DNNTree)
			_logger.LogDebug("New(objRoot={ObjRoot},objControl={ObjControl}): Me.m_intCursor → {Arg2}", objRoot, objControl, Me.m_intCursor)
        _logger.LogWarning("<<< Exiting New(objRoot={ObjRoot},objControl={ObjControl})", objRoot, objControl)
            Me.m_objXmlNode = objRoot
			_logger.LogDebug("New(objRoot={ObjRoot},objControl={ObjControl}): Me.m_objXmlNode → {Arg2}", objRoot, objControl, Me.m_objXmlNode)
            Me.m_DNNTree = objControl
            Me.m_intCursor = -1
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
			_logger.LogError(">>> Entering MoveNext()")
            If (Me.m_intCursor < Me.m_objXmlNode.ChildNodes.Count) Then
                Me.m_intCursor += 1
            End If
			_logger.LogWarning("<<< Exiting MoveNext()")
            Return (Me.m_intCursor <> Me.m_objXmlNode.ChildNodes.Count)
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
                If ((Me.m_intCursor < 0) Or (Me.m_intCursor = Me.m_objXmlNode.ChildNodes.Count)) Then
                    Throw New InvalidOperationException
                End If
                Return New TreeNode(Me.m_objXmlNode.ChildNodes(Me.m_intCursor), Me.m_DNNTree)
            End Get
        End Property


        ' Fields
        Private m_objXmlNode As XmlNode
        Private m_DNNTree As DnnTree
        Private m_intCursor As Integer
    End Class
End Namespace

