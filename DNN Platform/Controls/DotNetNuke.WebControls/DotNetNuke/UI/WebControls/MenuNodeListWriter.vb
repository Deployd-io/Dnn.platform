' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class MenuNodeListWriter
        Implements IMenuNodeWriter
        ' Methods
        Public Sub New()
		Logger.LogDebug(">>> Entering New()")
        End Sub

        Public Sub New(ByVal eOrientation As Orientation)
            Me.Orientation = eOrientation
        End Sub
		Logger.LogDebug("<<< Exiting New(eOrientation={EOrientation})", eOrientation)
		Logger.LogDebug("New(eOrientation={EOrientation}): Me.Orientation → {Arg1}", eOrientation, Me.Orientation)

        Private Function GetChildControlID(ByVal NodeID As String, ByVal Prefix As String) As String
			Logger.LogDebug(">>> Entering GetChildControlID(NodeID={NodeID},Prefix={Prefix})", NodeID, Prefix)
            Return $"{Me.m_Node.DNNMenu.ClientID}{Prefix}{NodeID}"
        End Function
		Logger.LogDebug("<<< Exiting GetChildControlID(NodeID={NodeID},Prefix={Prefix})", NodeID, Prefix)

        Private Function GetLink() As HyperLink
			Logger.LogDebug(">>> Entering GetLink()")
            Dim link2 As New HyperLink
            If (If((Not Me.m_Node.Enabled OrElse (Me.m_Node.ClickAction <> eClickAction.Navigate)), 0, 1) <> 0) Then
                link2.NavigateUrl = Me.m_Node.NavigateURL
				Logger.LogDebug("GetLink(): link2.NavigateUrl → {Arg0}", link2.NavigateUrl)
            End If
            Return link2
        End Function
		Logger.LogDebug("<<< Exiting GetLink()")

        Protected Sub Render(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer)
            Me.RenderOpenTag(writer)
            Dim parent As HyperLink = Me.GetLink
            Me.RenderNodeIcon(parent)
            Me.RenderNodeText(parent)
            Me.RenderNodeArrow(parent)
            parent.RenderControl(writer)
            If Me.m_Node.HasNodes Then
                writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.GetChildControlID(Me.m_Node.ID, "sub"))
                writer.RenderBeginTag(HtmlTextWriterTag.Ul)
                Me.RenderChildren(writer)
                writer.RenderEndTag
            End If
            writer.RenderEndTag
        End Sub
		Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer)

        Protected Sub RenderChildren(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderChildren(writer={Writer})", writer)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me.m_Node.MenuNodes.GetEnumerator
				Logger.LogDebug("RenderChildren(writer={Writer}): enumerator → {Enumerator}", writer, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    DirectCast(enumerator.Current, MenuNode).Render(writer)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub
		Logger.LogDebug("<<< Exiting RenderChildren(writer={Writer})", writer)

        Public Sub RenderNode(ByVal writer As HtmlTextWriter, ByVal Node As MenuNode) Implements IMenuNodeWriter.RenderNode
			Logger.LogDebug(">>> Entering RenderNode(writer={Writer},Node={Node})", writer, Node)
            Me.m_Node = Node
            Me.Render(writer)
			Logger.LogDebug("RenderNode(writer={Writer},Node={Node}): Me.m_Node → {Arg2}", writer, Node, Me.m_Node)
        End Sub
		Logger.LogDebug("<<< Exiting RenderNode(writer={Writer},Node={Node})", writer, Node)

        Protected Sub RenderNodeArrow(ByVal parent As Control)
			Logger.LogDebug(">>> Entering RenderNodeArrow(parent={Parent})", parent)
            If Me.m_Node.HasNodes Then
                Dim objA As Image = Nothing
                If (Me.m_Node.Level = 0) Then
                    If (Strings.Len(Me.m_Node.DNNMenu.RootArrowImage) > 0) Then
                        objA = New Image With { _
                            .ImageUrl = Me.m_Node.DNNMenu.RootArrowImage _
                        }
							Logger.LogDebug("RenderNodeArrow(parent={Parent}): objA → {ObjA}", parent, objA)
                    End If
                ElseIf (Strings.Len(Me.m_Node.DNNMenu.ChildArrowImage) > 0) Then
                    objA = New Image With { _
                        .ImageUrl = Me.m_Node.DNNMenu.ChildArrowImage _
                    }
                End If
                If Not Object.ReferenceEquals(objA, Nothing) Then
                    parent.Controls.Add(objA)
                End If
            End If
        End Sub
		Logger.LogDebug("<<< Exiting RenderNodeArrow(parent={Parent})", parent)

        Protected Sub RenderNodeIcon(ByVal parent As Control)
			Logger.LogDebug(">>> Entering RenderNodeIcon(parent={Parent})", parent)
            Dim cSSIcon As String = ""
            If (Strings.Len(Me.m_Node.CSSIcon) > 0) Then
                cSSIcon = Me.m_Node.CSSIcon
				Logger.LogDebug("RenderNodeIcon(parent={Parent}): cSSIcon → {CSSIcon}", parent, cSSIcon)
            ElseIf (Strings.Len(Me.m_Node.DNNMenu.DefaultIconCssClass) > 0) Then
                cSSIcon = Me.m_Node.DNNMenu.DefaultIconCssClass
            End If
            If ((Me.m_Node.ImageIndex > -1) AndAlso (Me.m_Node.ImageIndex > -1)) Then
                Dim objA As NodeImage = Me.m_Node.DNNMenu.ImageList(Me.m_Node.ImageIndex)
                If Not Object.ReferenceEquals(objA, Nothing) Then
                    Dim child As New Image With { _
                        .ImageUrl = objA.ImageUrl, _
                        .AlternateText = " ", _
                        .CssClass = cSSIcon _
                    }
                    parent.Controls.Add(child)
                End If
            End If
        End Sub
		Logger.LogDebug("<<< Exiting RenderNodeIcon(parent={Parent})", parent)

        Protected Sub RenderNodeText(ByVal parent As Control)
			Logger.LogDebug(">>> Entering RenderNodeText(parent={Parent})", parent)
            Dim child As New Label With { _
                .Text = Me.m_Node.Text _
				Logger.LogDebug("RenderNodeText(parent={Parent}): child.ToolTip → {Arg1}", parent, child.ToolTip)
				Logger.LogDebug("<<< Exiting RenderNodeText(parent={Parent})", parent)
            }
            If (Strings.Len(Me.m_Node.ToolTip) > 0) Then
                child.ToolTip = Me.m_Node.ToolTip
            End If
            parent.Controls.Add(child)
        End Sub

        Protected Sub RenderOpenTag(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderOpenTag(writer={Writer})", writer)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me.GetChildControlID(Me.m_Node.ID, "ctr"))
            writer.RenderBeginTag(HtmlTextWriterTag.Li)
        End Sub
		Logger.LogDebug("<<< Exiting RenderOpenTag(writer={Writer})", writer)


        ' Properties
        Public Property Orientation As Orientation
            Get
                Return Me.m_eOrientation
            End Get
            Set(ByVal Value As Orientation)
                Me.m_eOrientation = Value
            End Set
        End Property


        ' Fields
        Private m_Node As MenuNode
        Private m_eOrientation As Orientation
    End Class
End Namespace

