' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Design.WebControls
Imports DotNetNuke.UI.Utilities
Imports DotNetNuke.Web.Client
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Diagnostics
Imports System.Text
Imports System.Web.UI
Imports System.Web.UI.Design
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls

Namespace DotNetNuke.UI.WebControls
    <ProvideProperty("Toolbar", GetType(Control)), DefaultProperty("Buttons"), ParseChildren(True, "Buttons"), Designer(GetType(DNNToolBarDesigner)), PersistChildren(False)> _
    Public Class DNNToolBar
        Inherits Control
        Implements IExtenderProvider
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(DNNToolBar))
        ' Methods
        Public Sub New()
			Logger.LogDebug(">>> Entering New()")
            AddHandler MyBase.PreRender, New EventHandler(AddressOf Me.DNNToolBar_PreRender)
            DNNToolBar.__ENCAddToList(Me)
            Me.m_bMarked = False
			Logger.LogDebug("New(): Me.m_bMarked → {Arg0}", Me.m_bMarked)
			Logger.LogDebug("<<< Exiting New()")
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
			Logger.LogDebug(">>> Entering __ENCAddToList(value={Value})", value)
            SyncLock DNNToolBar.__ENCList
                If (DNNToolBar.__ENCList.Count = DNNToolBar.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNToolBar.__ENCList.Count - 1)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNToolBar.__ENCList.RemoveRange(index, (DNNToolBar.__ENCList.Count - index))
                            DNNToolBar.__ENCList.Capacity = DNNToolBar.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNToolBar.__ENCList(num2)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNToolBar.__ENCList(index) = DNNToolBar.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNToolBar.__ENCList.Add(New WeakReference(value))
            End SyncLock
			Logger.LogDebug("<<< Exiting __ENCAddToList(value={Value})", value)
        End Sub

        Public Function CanExtend(ByVal extendee As Object) As Boolean Implements IExtenderProvider.CanExtend
			Logger.LogDebug(">>> Entering CanExtend(extendee={Extendee})", extendee)
			Logger.LogDebug("<<< Exiting CanExtend(extendee={Extendee})", extendee)
            Return ((True = TypeOf extendee Is IDNNToolBar) AndAlso (DirectCast(extendee, IDNNToolBar).ToolBarId = Me.ID))
        End Function

        Private Sub DNNToolBar_PreRender(ByVal sender As Object, ByVal e As EventArgs)
			Logger.LogDebug(">>> Entering DNNToolBar_PreRender(sender={Sender},e={E})", sender, e)
            If Me.Visible Then
                Me.RegisterToolbarScript
            End If
			Logger.LogDebug("<<< Exiting DNNToolBar_PreRender(sender={Sender},e={E})", sender, e)
        End Sub

        Private Function FindAttachedControl(ByVal objControl As Control) As Control
			Logger.LogDebug(">>> Entering FindAttachedControl(objControl={ObjControl})", objControl)
            Dim enumerator As IEnumerator = Nothing
            If (TypeOf objControl Is IDNNToolBar AndAlso (DirectCast(objControl, IDNNToolBar).ToolBarId = Me.ID)) Then
                Return objControl
            End If
            Try 
                enumerator = objControl.Controls.GetEnumerator
				Logger.LogDebug("FindAttachedControl(objControl={ObjControl}): enumerator → {Enumerator}", objControl, enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As Control = DirectCast(enumerator.Current, Control)
                    Dim objA As Control = Me.FindAttachedControl(current)
                    If Not Object.ReferenceEquals(objA, Nothing) Then
                        Return objA
                    End If
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
			Logger.LogDebug("<<< Exiting FindAttachedControl(objControl={ObjControl})", objControl)
            Return Nothing
        End Function

        Public Function GetToolbar(ByVal objControl As Control) As DNNToolBar
			Logger.LogDebug(">>> Entering GetToolbar(objControl={ObjControl})", objControl)
            Dim bar2 As IDNNToolBar = DirectCast(objControl, IDNNToolBar)
            Dim control As Control = Nothing
            If (Strings.Len(bar2.ToolBarId) > 0) Then
                control = objControl.Page.FindControl(bar2.ToolBarId)
				Logger.LogDebug("GetToolbar(objControl={ObjControl}): control → {Control}", objControl, control)
            End If
			Logger.LogDebug("<<< Exiting GetToolbar(objControl={ObjControl})", objControl)
            Return DirectCast(control, DNNToolBar)
        End Function

        Friend Sub NotifyDesigner()
			Logger.LogDebug(">>> Entering NotifyDesigner()")
            If (If((Not WebControls.IsDesignMode OrElse (MyBase.Site Is Nothing)), 0, 1) <> 0) Then
                Dim member As PropertyDescriptor = TypeDescriptor.GetProperties(Me)("Toolbar")
                Dim ce As New ComponentChangedEventArgs(Me, member, Nothing, Me)
                DirectCast(DirectCast(MyBase.Site.Container, IDesignerHost).GetDesigner(Me), ControlDesigner).OnComponentChanged(Me, ce)
            End If
			Logger.LogDebug("<<< Exiting NotifyDesigner()")
        End Sub

        Public Function RegisterToolBar(ByVal objAssociatedControl As Control, ByVal strShowEventName As String, ByVal strHideEventName As String, ByVal strToolBarActionHandler As String) As Boolean
			Logger.LogDebug(">>> Entering RegisterToolBar(objAssociatedControl={ObjAssociatedControl},strShowEventName={StrShowEventName},strHideEventName={StrHideEventName},strToolBarActionHandler={StrToolBarActionHandler})", objAssociatedControl, strShowEventName, strHideEventName, strToolBarActionHandler)
			Logger.LogDebug(">>> Entering RegisterToolBar(objAssociatedControl={ObjAssociatedControl},strShowEventName={StrShowEventName},strHideEventName={StrHideEventName},strToolBarActionHandler={StrToolBarActionHandler},ToolBarScriptPath={ToolBarScriptPath})", objAssociatedControl, strShowEventName, strHideEventName, strToolBarActionHandler, ToolBarScriptPath)
			Logger.LogDebug("<<< Exiting RegisterToolBar(objAssociatedControl={ObjAssociatedControl},strShowEventName={StrShowEventName},strHideEventName={StrHideEventName},strToolBarActionHandler={StrToolBarActionHandler})", objAssociatedControl, strShowEventName, strHideEventName, strToolBarActionHandler)
			Logger.LogDebug("<<< Exiting RegisterToolBar(objAssociatedControl={ObjAssociatedControl},strShowEventName={StrShowEventName},strHideEventName={StrHideEventName},strToolBarActionHandler={StrToolBarActionHandler},ToolBarScriptPath={ToolBarScriptPath})", objAssociatedControl, strShowEventName, strHideEventName, strToolBarActionHandler, ToolBarScriptPath)
            Return Me.RegisterToolBar(objAssociatedControl, strShowEventName, strHideEventName, strToolBarActionHandler, ClientAPI.ScriptPath)
        End Function

        Public Function RegisterToolBar(ByVal objAssociatedControl As Control, ByVal strShowEventName As String, ByVal strHideEventName As String, ByVal strToolBarActionHandler As String, ByVal ToolBarScriptPath As String) As Boolean
            Dim flag As Boolean
            If Not ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML) Then
                flag = False
            Else
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.js", FileOrder.Js.DnnControlsLabelEdit)
                WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.dnntoolbarstub.js")
                Dim str As String = $"__dnn_toolbarHandler('{Me.UniqueID}','{objAssociatedControl.ClientID}','{Me.BehaviorID}','{Me.NamingContainer.UniqueID}',{strToolBarActionHandler},'{strShowEventName}','{strHideEventName}')"
                If TypeOf objAssociatedControl Is WebControl Then
                    DirectCast(objAssociatedControl, WebControl).Attributes.Add(strShowEventName, str)
                ElseIf TypeOf objAssociatedControl Is HtmlControl Then
                    DirectCast(objAssociatedControl, HtmlControl).Attributes.Add(strShowEventName, str)
                End If
                flag = True
            End If
            Return flag
        End Function

        Private Function RegisterToolbarScript() As String
			Logger.LogDebug(">>> Entering RegisterToolbarScript()")
            Dim str As String = Nothing
            Dim uniqueID As String = Me.UniqueID
            If Me.ReuseToolBar Then
                uniqueID = If(Not String.IsNullOrEmpty(Me.BehaviorID), Me.BehaviorID, Me.ID)
				Logger.LogDebug("RegisterToolbarScript(): uniqueID → {UniqueID}", uniqueID)
            End If
            If (Not ClientAPI.IsClientScriptBlockRegistered(Me.Page, (uniqueID & "_toolbar")) AndAlso ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML)) Then
                ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn)
                ClientAPI.RegisterStartUpScript(Me.Page, (uniqueID & "_toolbar"), $"<script type=""text/javascript"">dnn.controls.toolbars['{uniqueID}']={Me.ToJSON};</script>")
            End If
			Logger.LogDebug("RegisterToolbarScript(): type → {Type}", type)
            If Not ClientAPI.UseExternalScripts Then
                ClientAPI.RegisterEmbeddedResource(Me.Page, "dnn.controls.dnntoolbar.js", Me.GetType)
                ClientAPI.RegisterEmbeddedResource(Me.Page, "dnn.controls.dnntoolbarstub.js", Me.GetType)
            End If
			Logger.LogDebug("<<< Exiting RegisterToolbarScript()")
            Return str
        End Function

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
		Logger.LogDebug(">>> Entering Render(writer={Writer})", writer)
        End Sub

        Public Sub SetToolbar()
		Logger.LogDebug(">>> Entering SetToolbar()")
        End Sub

        Friend Function ToJSON() As String
			Logger.LogDebug(">>> Entering ToJSON()")
            Dim enumerator As IEnumerator = Nothing
            Dim enumerator2 As IEnumerator = Nothing
            Dim hashtable As New Hashtable
            If (Strings.Len(Me.CssClass) > 0) Then
                hashtable.Add("css", ("'" & Me.CssClass & "'"))
            End If
            If (Strings.Len(Me.DefaultButtonCssClass) > 0) Then
                hashtable.Add("cssb", ("'" & Me.DefaultButtonCssClass & "'"))
            End If
            If (Strings.Len(Me.DefaultButtonHoverCssClass) > 0) Then
                hashtable.Add("cssbh", ("'" & Me.DefaultButtonHoverCssClass & "'"))
            End If
            If (Me.MouseOutDelay <> 250) Then
                hashtable.Add("mod", Me.MouseOutDelay)
            End If
            If (Strings.Len(Me.Visible) > 0) Then
                hashtable.Add("vis", If(Me.Visible, 1, 0))
            End If
            Dim builder As New StringBuilder
            builder.Append("["c)
            Try 
                enumerator = Me.Buttons.GetEnumerator
				Logger.LogDebug("ToJSON(): enumerator → {Enumerator}", enumerator)
                Do While True
                    If Not enumerator.MoveNext Then
                        Exit Do
                    End If
                    Dim current As DNNToolBarButton = DirectCast(enumerator.Current, DNNToolBarButton)
                    If (builder.Length > 1) Then
                        builder.Append(","c)
                    End If
                    builder.Append(current.ToJSON)
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                    TryCast(enumerator,IDisposable).Dispose
                End If
            End Try
            builder.Append("]"c)
            hashtable.Add("btns", builder.ToString)
            builder = New StringBuilder
			Logger.LogDebug("ToJSON(): builder → {Builder}", builder)
            builder.Append("{"c)
            Try 
                enumerator2 = hashtable.Keys.GetEnumerator
				Logger.LogDebug("ToJSON(): enumerator2 → {Enumerator2}", enumerator2)
                Do While True
                    If Not enumerator2.MoveNext Then
                        Exit Do
                    End If
                    Dim str2 As String = Conversions.ToString(enumerator2.Current)
                    If (builder.Length > 1) Then
                        builder.Append(","c)
                    End If
                    builder.Append((str2 & ":" & Conversions.ToString(hashtable(str2))))
                Loop
            Finally
                If Not Object.ReferenceEquals(TryCast(enumerator2,IDisposable), Nothing) Then
                    TryCast(enumerator2,IDisposable).Dispose
                End If
            End Try
            builder.Append("}"c)
			Logger.LogDebug("<<< Exiting ToJSON()")
            Return builder.ToString
        End Function


        ' Properties
        <Bindable(True), ClientPropertyName("bid"), ClientProperty, DefaultValue("")> _
        Public Property BehaviorID As String
            Get
                Return Conversions.ToString(Me.ViewState("BehaviorID"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("BehaviorID") = Value
            End Set
        End Property

        <Category("Appearance")> _
        Public Property CssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("CssClass"))
            End Get
            Set(ByVal value As String)
                Me.ViewState("CssClass") = value
                If WebControls.IsDesignMode Then
                    Me.NotifyDesigner
                End If
            End Set
        End Property

        <Category("Appearance")> _
        Public Property DefaultButtonCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultButtonCssClass"))
            End Get
            Set(ByVal value As String)
                Me.ViewState("DefaultButtonCssClass") = value
                If WebControls.IsDesignMode Then
                    Me.NotifyDesigner
                End If
            End Set
        End Property

        <Category("Appearance")> _
        Public Property DefaultButtonHoverCssClass As String
            Get
                Return Conversions.ToString(Me.ViewState("DefaultButtonHoverCssClass"))
            End Get
            Set(ByVal value As String)
                Me.ViewState("DefaultButtonHoverCssClass") = value
                If WebControls.IsDesignMode Then
                    Me.NotifyDesigner
                End If
            End Set
        End Property

        <PersistenceMode(PersistenceMode.Attribute), Category("Behavior"), Description("Number of milliseconds to wait befor hiding sub-menu on mouse out"), DefaultValue(250)> _
        Public Property MouseOutDelay As Integer
            Get
                Return If((Strings.Len(Conversions.ToString(Me.ViewState("MouseOutDelay"))) <> 0), Conversions.ToInteger(Me.ViewState("MouseOutDelay")), 250)
            End Get
            Set(ByVal Value As Integer)
                Me.ViewState("MouseOutDelay") = Value
                If WebControls.IsDesignMode Then
                    Me.NotifyDesigner
                End If
            End Set
        End Property

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerDefaultProperty)> _
        Public Overridable ReadOnly Property Buttons As DNNToolBarButtonCollection
            Get
                If Object.ReferenceEquals(Me.m_objButtons, Nothing) Then
                    Me.m_objButtons = New DNNToolBarButtonCollection(Me)
                End If
                Return Me.m_objButtons
            End Get
        End Property

        <PersistenceMode(PersistenceMode.Attribute), DefaultValue(False), Description("Allows toolbar structure to be reused by others sharing the same id"), Category("Behavior")> _
        Public Property ReuseToolBar As Boolean
            Get
                Return If((Strings.Len(Conversions.ToString(Me.ViewState("ReuseToolBar"))) <> 0), Conversions.ToBoolean(Me.ViewState("ReuseToolBar")), False)
            End Get
            Set(ByVal Value As Boolean)
                Me.ViewState("ReuseToolBar") = Value
                If WebControls.IsDesignMode Then
                    Me.NotifyDesigner
                End If
            End Set
        End Property

        <Browsable(False), Bindable(False)> _
        Public Shadows Property Visible As Boolean
            Get
                Return MyBase.Visible
            End Get
            Set(ByVal Value As Boolean)
                MyBase.Visible = Value
            End Set
        End Property

        <Browsable(False)> _
        Friend ReadOnly Property AttachedControl As Control
            Get
                Return Me.FindAttachedControl(Me.Page)
            End Get
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_bMarked As Boolean
        Private m_objState As StateBag
        Private m_objButtons As DNNToolBarButtonCollection
    End Class
End Namespace

