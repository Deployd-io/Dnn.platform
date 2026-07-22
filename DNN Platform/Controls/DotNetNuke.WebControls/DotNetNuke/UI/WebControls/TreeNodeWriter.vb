' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports DotNetNuke.UI.Utilities
Imports Microsoft.VisualBasic
Imports System
Imports System.Collections
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    Friend NotInheritable Class TreeNodeWriter
        Implements ITreeNodeWriter
        ' Methods
        Private Function GetNodeCss(ByVal oNode As TreeNode) As String
			Logger.LogDebug(">>> Entering GetNodeCss(oNode={ONode})", oNode)
            Dim cssClass As String = oNode.DNNTree.CssClass
            If (oNode.Level > 0) Then
                cssClass = oNode.DNNTree.DefaultChildNodeCssClass
				Logger.LogDebug("GetNodeCss(oNode={ONode}): cssClass → {CssClass}", oNode, cssClass)
            End If
            If (Strings.Len(oNode.CssClass) > 0) Then
                cssClass = oNode.CssClass
            End If
            If oNode.Selected Then
                cssClass = If((Strings.Len(oNode.CSSClassSelected) <= 0), (cssClass & " " & oNode.DNNTree.DefaultNodeCssClassSelected), (cssClass & " " & oNode.CSSClassSelected))
            End If
            Return cssClass
        End Function
		Logger.LogDebug("<<< Exiting GetNodeCss(oNode={ONode})", oNode)

        Protected Sub Render(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer)
            Me.RenderContents(writer)
            If (If((Not Me._Node.HasNodes OrElse (Not Me._Node.IsExpanded AndAlso Not Me._Node.DNNTree.IsCrawler)), 0, 1) <> 0) Then
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "Child")
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                writer.RenderBeginTag(HtmlTextWriterTag.Div)
                Me.RenderChildren(writer)
                writer.RenderEndTag
            End If
        End Sub
		Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer)

        Protected Sub RenderChildren(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderChildren(writer={Writer})", writer)
            Dim enumerator As IEnumerator = Nothing
            Try 
                enumerator = Me._Node.TreeNodes.GetEnumerator
				Logger.LogDebug("RenderChildren(writer={Writer}): enumerator → {Enumerator}", writer, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    DirectCast(enumerator.Current, TreeNode).Render(writer)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
        End Sub
		Logger.LogDebug("<<< Exiting RenderChildren(writer={Writer})", writer)

        Protected Sub RenderContents(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderContents(writer={Writer})", writer)
            Me.RenderOpenTag(writer)
            If (Me._Node.DNNTree.IndentWidth = 0) Then
                Me._Node.DNNTree.IndentWidth = 9
            End If
            If (Me._Node.Level > 0) Then
                Me.RenderSpacer(writer, (Me._Node.Level * Me._Node.DNNTree.IndentWidth))
            End If
            Me.RenderExpandNodeIcon(writer)
            Me.RenderNodeCheckbox(writer)
            Me.RenderNodeIcon(writer)
            Me.RenderNodeText(writer)
            writer.RenderEndTag
        End Sub
		Logger.LogDebug("<<< Exiting RenderContents(writer={Writer})", writer)

        Protected Sub RenderExpandNodeIcon(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderExpandNodeIcon(writer={Writer})", writer)
            If Not Me._Node.HasNodes Then
                Me.RenderSpacer(writer, Me._Node.DNNTree.ExpandCollapseImageWidth)
            Else
                Dim link As New HyperLink
                Dim image As New Image
                If (Me._Node.IsExpanded OrElse Me._Node.DNNTree.IsCrawler) Then
                    link.Text = TreeNodeWriter._expcol(1)
                    If (Strings.Len(Me._Node.DNNTree.ExpandedNodeImage) > 0) Then
                        image.ImageUrl = Me._Node.DNNTree.ExpandedNodeImage
						Logger.LogDebug("RenderExpandNodeIcon(writer={Writer}): image.ImageUrl → {Arg1}", writer, image.ImageUrl)
                    End If
                Else
                    link.Text = TreeNodeWriter._expcol(0)
                    If (Strings.Len(Me._Node.DNNTree.CollapsedNodeImage) > 0) Then
                        image.ImageUrl = Me._Node.DNNTree.CollapsedNodeImage
                    End If
                End If
                link.NavigateUrl = ClientAPI.GetPostBackClientHyperlink(Me._Node.DNNTree, Me._Node.ID)
                If (Strings.Len(image.ImageUrl) <= 0) Then
                    link.RenderControl(writer)
                Else
                    link.RenderBeginTag(writer)
                    image.RenderControl(writer)
                    link.RenderEndTag(writer)
                End If
                image = Nothing
				Logger.LogDebug("RenderExpandNodeIcon(writer={Writer}): image → {Image}", writer, image)
				Logger.LogDebug("RenderExpandNodeIcon(writer={Writer}): link → {Link}", writer, link)
                link = Nothing
            End If
            writer.Write("&nbsp;")
        End Sub
		Logger.LogDebug("<<< Exiting RenderExpandNodeIcon(writer={Writer})", writer)

        Public Sub RenderNode(ByVal writer As HtmlTextWriter, ByVal Node As TreeNode) Implements ITreeNodeWriter.RenderNode
			Logger.LogDebug(">>> Entering RenderNode(writer={Writer},Node={Node})", writer, Node)
            Me._Node = Node
            Me.Render(writer)
			Logger.LogDebug("RenderNode(writer={Writer},Node={Node}): Me._Node → {Arg2}", writer, Node, Me._Node)
        End Sub
		Logger.LogDebug("<<< Exiting RenderNode(writer={Writer},Node={Node})", writer, Node)

        Protected Sub RenderNodeCheckbox(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderNodeCheckbox(writer={Writer})", writer)
            If Me._Node.DNNTree.CheckBoxes Then
                Dim box As New CheckBox With { _
                    .ID = (Me._Node.ID & TreeNode._separator & TreeNode._checkboxIDSufix), _
                    .Checked = Me._Node.Selected _
                }
                Dim expression As String = ""
                If (Strings.Len(Me._Node.JSFunction) > 0) Then
					Logger.LogDebug("RenderNodeCheckbox(writer={Writer}): String → {String}", writer, String)
                    If Not Me._Node.JSFunction.EndsWith(";", StringComparison.Ordinal) Then
                        Dim node As TreeNode = Me._Node
                        node.JSFunction = (node.JSFunction & ";")
                    End If
                    expression = (expression & Me._Node.JSFunction)
					Logger.LogDebug("RenderNodeCheckbox(writer={Writer}): expression → {Expression}", writer, expression)
                End If
                If (Strings.Len(Me._Node.DNNTree.JSFunction) > 0) Then
                    If Not Me._Node.DNNTree.JSFunction.EndsWith(";", StringComparison.Ordinal) Then
                        Dim dNNTree As DnnTree = Me._Node.DNNTree
                        dNNTree.JSFunction = (dNNTree.JSFunction & ";")
                    End If
                    expression = (expression & Me._Node.DNNTree.JSFunction)
                End If
                Dim str2 As String = (ClientAPI.GetPostBackClientHyperlink(Me._Node.DNNTree, (Me._Node.ID & ClientAPI.COLUMN_DELIMITER & "Click")).Replace("javascript:", "") & ";")
                Dim str As String = (ClientAPI.GetPostBackClientHyperlink(Me._Node.DNNTree, (Me._Node.ID & ClientAPI.COLUMN_DELIMITER & "Checked")).Replace("javascript:", "") & ";")
                expression = If(Me._Node.Selected, str, If((Strings.Len(expression) <= 0), (expression & str2), (("if (eval(""" & expression.Replace("""", """""") & """) != false) ") & str2 & " else " & str)))
                box.Attributes.Add("onclick", expression)
                box.RenderControl(writer)
                box = Nothing
				Logger.LogDebug("RenderNodeCheckbox(writer={Writer}): box → {Box}", writer, box)
                writer.Write("&nbsp;")
            End If
        End Sub
		Logger.LogDebug("<<< Exiting RenderNodeCheckbox(writer={Writer})", writer)

        Protected Sub RenderNodeIcon(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderNodeIcon(writer={Writer})", writer)
            Dim label As New Label
            If (Strings.Len(Me._Node.CSSIcon) > 0) Then
                label.CssClass = Me._Node.CSSIcon
				Logger.LogDebug("RenderNodeIcon(writer={Writer}): label.CssClass → {Arg1}", writer, label.CssClass)
            ElseIf (Strings.Len(Me._Node.DNNTree.DefaultIconCssClass) > 0) Then
                label.CssClass = Me._Node.DNNTree.DefaultIconCssClass
            End If
            label.RenderBeginTag(writer)
            If (Me._Node.ImageIndex > -1) Then
                Dim objA As NodeImage = Me._Node.DNNTree.ImageList(Me._Node.ImageIndex)
                If Not Object.ReferenceEquals(objA, Nothing) Then
                    Dim image As New Image() With { _
                        .ImageUrl = objA.ImageUrl _
                    }
                    image.RenderControl(writer)
                    writer.Write("&nbsp;")
                End If
            End If
            label.RenderEndTag(writer)
        End Sub
		Logger.LogDebug("<<< Exiting RenderNodeIcon(writer={Writer})", writer)

        Protected Sub RenderNodeText(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderNodeText(writer={Writer})", writer)
            Dim link As New HyperLink
            Dim expression As String = ""
            link.Text = Me._Node.Text
			Logger.LogDebug("RenderNodeText(writer={Writer}): link.Text → {Arg1}", writer, link.Text)
            If (Strings.Len(Me._Node.JSFunction) > 0) Then
                If Not Me._Node.JSFunction.EndsWith(";", StringComparison.Ordinal) Then
                    Dim node As TreeNode = Me._Node
                    node.JSFunction = (node.JSFunction & ";")
                End If
                expression = (expression & Me._Node.JSFunction)
				Logger.LogDebug("RenderNodeText(writer={Writer}): expression → {Expression}", writer, expression)
            ElseIf (Strings.Len(Me._Node.DNNTree.JSFunction) > 0) Then
                If Not Me._Node.DNNTree.JSFunction.EndsWith(";", StringComparison.Ordinal) Then
                    Dim dNNTree As DnnTree = Me._Node.DNNTree
                    dNNTree.JSFunction = (dNNTree.JSFunction & ";")
                End If
                expression = (expression & Me._Node.DNNTree.JSFunction)
            End If
            If Me._Node.Enabled Then
                Select Case Me._Node.ClickAction
                    Case eClickAction.PostBack, eClickAction.Expand
                        If (Strings.Len(expression) > 0) Then
                            expression = ("if (eval(""" & expression.Replace("""", """""") & """) != false) ")
                        End If
                        expression = (expression & ClientAPI.GetPostBackClientHyperlink(Me._Node.DNNTree, (Me._Node.ID & ClientAPI.COLUMN_DELIMITER & "Click")).Replace("javascript:", ""))
                        Exit Select
                    Case eClickAction.Navigate
                        If (Strings.Len(expression) > 0) Then
                            expression = ("if (eval(""" & expression.Replace("""", """""") & """) != false) ")
                        End If
                        If (Strings.Len(Me._Node.DNNTree.Target) <= 0) Then
                            expression = (expression & "window.location.href='" & Me._Node.NavigateUrl & "';")
                        Else
                            Dim strArray As String() = New String() { expression, "window.frames.", Me._Node.DNNTree.Target, ".location.href='", Me._Node.NavigateUrl, "'; void(0);" }
                            expression = String.Concat(strArray)
                        End If
                        Exit Select
                    Case Else
                        Exit Select
                End Select
                link.NavigateUrl = ("javascript:" & expression)
            End If
            If (Strings.Len(Me._Node.ToolTip) > 0) Then
                link.ToolTip = Me._Node.ToolTip
				Logger.LogDebug("RenderNodeText(writer={Writer}): link.ToolTip → {Arg1}", writer, link.ToolTip)
            End If
            Dim nodeCss As String = Me.GetNodeCss(Me._Node)
            If (Strings.Len(nodeCss) > 0) Then
                link.CssClass = nodeCss
				Logger.LogDebug("RenderNodeText(writer={Writer}): link.CssClass → {Arg1}", writer, link.CssClass)
            End If
            link.RenderControl(writer)
        End Sub
		Logger.LogDebug("<<< Exiting RenderNodeText(writer={Writer})", writer)

        Protected Sub RenderOpenTag(ByVal writer As HtmlTextWriter)
			Logger.LogDebug(">>> Entering RenderOpenTag(writer={Writer})", writer)
            writer.AddAttribute(HtmlTextWriterAttribute.Name, Me._Node.ID)
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Me._Node.ID.Replace(TreeNode._separator, "_"))
            writer.RenderBeginTag(HtmlTextWriterTag.Div)
        End Sub
		Logger.LogDebug("<<< Exiting RenderOpenTag(writer={Writer})", writer)

        Protected Sub RenderSpacer(ByVal writer As HtmlTextWriter, ByVal Width As Integer)
			Logger.LogDebug(">>> Entering RenderSpacer(writer={Writer},Width={Width})", writer, Width)
            writer.AddStyleAttribute("width", (Width.ToString(CultureInfo.InvariantCulture) & "px"))
            writer.AddStyleAttribute("height", "1px")
            writer.AddAttribute("src", (Me._Node.DNNTree.SystemImagesPath & "spacer.gif"))
            writer.RenderBeginTag(HtmlTextWriterTag.Img)
            writer.RenderEndTag
        End Sub
		Logger.LogDebug("<<< Exiting RenderSpacer(writer={Writer},Width={Width})", writer, Width)


        ' Fields
        Private Shared ReadOnly _expcol As String() = New String() { "+", "-" }
        Private _Node As TreeNode
    End Class
End Namespace

