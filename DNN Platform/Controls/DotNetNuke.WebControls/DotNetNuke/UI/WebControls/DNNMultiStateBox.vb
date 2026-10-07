' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Design.WebControls
Imports DotNetNuke.UI.Utilities
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.ComponentModel.Design
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.Design
Imports System.Web.UI.WebControls
Imports DotNetNuke.Abstractions.ClientResources
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    <PersistChildren(False), ParseChildren(True, "States"), ToolboxData("<{0}:DNNMultiStateBox runat=server></{0}:DNNMultiStateBox>"), Designer(GetType(DNNMultiStateBoxDesigner))> _
    Public Class DNNMultiStateBox
        Inherits CheckBox
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Sub New()
			_logger.LogError(">>> Entering New()")
            AddHandler MyBase.PreRender, New EventHandler(AddressOf Me.DNNMultiStateBox_PreRender)
            DNNMultiStateBox.__ENCAddToList(Me)
            Me.m_States = Nothing
			_logger.LogDebug("New(): Me.m_States → {Arg0}", Me.m_States)
			_logger.LogDebug("New(): Me.m_ValueSet → {Arg0}", Me.m_ValueSet)
        _logger.LogWarning("<<< Exiting New()")
            Me.m_Enabled = True
			_logger.LogDebug("New(): Me.m_Enabled → {Arg0}", Me.m_Enabled)
            Me.m_ValueSet = False
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
			_logger.LogError(">>> Entering __ENCAddToList(value={Value})", value)
            SyncLock DNNMultiStateBox.__ENCList
                If (DNNMultiStateBox.__ENCList.Count = DNNMultiStateBox.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (DNNMultiStateBox.__ENCList.Count - 1)
					_logger.LogError("__ENCAddToList(value={Value}): num3 → {Num3}", value, num3)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            DNNMultiStateBox.__ENCList.RemoveRange(index, (DNNMultiStateBox.__ENCList.Count - index))
                            DNNMultiStateBox.__ENCList.Capacity = DNNMultiStateBox.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = DNNMultiStateBox.__ENCList(num2)
						_logger.LogError("__ENCAddToList(value={Value}): reference → {Reference}", value, reference)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                DNNMultiStateBox.__ENCList(index) = DNNMultiStateBox.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                DNNMultiStateBox.__ENCList.Add(New WeakReference(value))
            End SyncLock
			_logger.LogWarning("<<< Exiting __ENCAddToList(value={Value})", value)
        End Sub

        Private Sub DNNMultiStateBox_PreRender(ByVal sender As Object, ByVal e As EventArgs)
			_logger.LogError(">>> Entering DNNMultiStateBox_PreRender(sender={Sender},e={E})", sender, e)
            Me.RegisterClientScript
            If Not Object.ReferenceEquals(Me.SelectedState, Nothing) Then
                Me.Text = Me.SelectedState.Text
				_logger.LogDebug("DNNMultiStateBox_PreRender(sender={Sender},e={E}): Me.Text → {Arg2}", sender, e, Me.Text)
            End If
			_logger.LogWarning("<<< Exiting DNNMultiStateBox_PreRender(sender={Sender},e={E})", sender, e)
        End Sub

        Protected Function MarshalledProperties() As Hashtable
			_logger.LogError(">>> Entering MarshalledProperties()")
			_logger.LogWarning("<<< Exiting MarshalledProperties()")
            Return New Hashtable From { { "states", MSAJAX.Serialize(Me.States) } }
        End Function

        Friend Sub NotifyDesigner()
			_logger.LogError(">>> Entering NotifyDesigner()")
            If (If((Not MyBase.DesignMode OrElse (Me.States Is Nothing)), 0, 1) <> 0) Then
                Dim designer As ControlDesigner = TryCast(TryCast(MyBase.Site.Container,IDesignerHost).GetDesigner(Me),ControlDesigner)
				_logger.LogError("NotifyDesigner(): designer → {Designer}", designer)
                Dim member As PropertyDescriptor = Nothing
                Try 
                    member = TypeDescriptor.GetProperties(Me)("States")
					_logger.LogDebug("NotifyDesigner(): member → {Member}", member)
                Catch exception1 As Exception
					_logger.LogError("Exception in NotifyDesigner(): {Arg0}", exception1.Message)
                    Dim ex As Exception = exception1
                    ProjectData.SetProjectError(ex)
                    Dim exception As Exception = ex
                    ProjectData.ClearProjectError
                    Return
                End Try
                Dim ce As New ComponentChangedEventArgs(Me, member, Nothing, Me.States)
                designer.OnComponentChanged(Me, ce)
            End If
			_logger.LogWarning("<<< Exiting NotifyDesigner()")
        End Sub

        Public Sub RegisterClientScript()
			_logger.LogError(">>> Entering RegisterClientScript()")
            ClientAPI.RegisterClientReference(Me.Page, ClientAPI.ClientNamespaceReferences.dnn_dom)
            WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.js", DotNetNuke.Web.Client.FileOrder.Js.DnnControlsLabelEdit)
            WebControls.RegisterClientScriptBlock(Me.Page, "dnn.controls.dnnmultistatebox.js")
			_logger.LogWarning("<<< Exiting RegisterClientScript()")
        End Sub

        Protected Overrides Sub Render(ByVal writer As HtmlTextWriter)
			_logger.LogError(">>> Entering Render(writer={Writer})", writer)
            writer.AddAttribute("value", Me.SelectedStateKey)
            MyBase.Render(writer)
            Dim properties As Hashtable = Me.MarshalledProperties
            Dim marshalledPropertyJSON As String = ScriptGenerator.GetMarshalledPropertyJSON(Me, properties)
			_logger.LogError("Render(writer={Writer}): marshalledPropertyJSON → {MarshalledPropertyJSON}", writer, marshalledPropertyJSON)
            BaseWebControl.RegisterInitialize(Me, "initMultiStateBox", marshalledPropertyJSON)
			_logger.LogWarning("<<< Exiting Render(writer={Writer})", writer)
        End Sub


        ' Properties
        <DefaultValue(""), ClientProperty, ClientPropertyName("state")> _
        Public Property SelectedStateKey As String
            Get
                If (If((Me.m_ValueSet OrElse (Not Me.Page.IsPostBack OrElse (Strings.Len(Me.Page.Request.Form(Me.UniqueID)) <= 0))), 0, 1) <> 0) Then
                    Me.SelectedStateKey = Me.Page.Request.Form(Me.UniqueID)
                End If
                Return If((If(((Strings.Len(Me.Attributes("value")) <= 0) OrElse (Me.Attributes("value") = "on")), 0, 1) = 0), If((Me.States.Count <= 0), "", Me.States(0).Key), Me.Attributes("value"))
            End Get
            Set(ByVal Value As String)
                Me.m_ValueSet = True
                Me.Attributes("value") = Value
            End Set
        End Property

        Public ReadOnly Property SelectedState As DNNMultiState
            Get
                Dim selectedStateKey As String = Me.SelectedStateKey
                If Not String.IsNullOrEmpty(selectedStateKey) Then
                    Dim enumerator As IEnumerator = Nothing
                    Try 
                        enumerator = Me.States.GetEnumerator
                        Do While True
                            If Not enumerator.MoveNext Then
                                Exit Do
                            End If
                            Dim current As DNNMultiState = DirectCast(enumerator.Current, DNNMultiState)
                            If (current.Key = selectedStateKey) Then
                                Return current
                            End If
                        Loop
                    Finally
                        If Not Object.ReferenceEquals(TryCast(enumerator,IDisposable), Nothing) Then
                            TryCast(enumerator,IDisposable).Dispose
                        End If
                    End Try
                End If
                Return Nothing
            End Get
        End Property

        <DefaultValue(""), ClientProperty, ClientPropertyName("bid")> _
        Public Property BehaviorID As String
            Get
                Return Conversions.ToString(Me.ViewState("BehaviorID"))
            End Get
            Set(ByVal Value As String)
                Me.ViewState("BehaviorID") = Value
            End Set
        End Property

        <Description("Location of ClientAPI js files"), Category("Paths"), DefaultValue("")> _
        Public Property ClientAPIScriptPath As String
            Get
                Return ClientAPI.ScriptPath
            End Get
            Set(ByVal Value As String)
                ClientAPI.ScriptPath = Value
            End Set
        End Property

        <Description("Location of dnn.controls.DNNMultiStateBox.js file"), Category("Paths"), DefaultValue("")> _
        Public Property MultiStateBoxScriptPath As String
            Get
                Return If((Strings.Len(Me.ViewState("MultiStateBoxScriptPath")) <> 0), Conversions.ToString(Me.ViewState("MultiStateBoxScriptPath")), Me.ClientAPIScriptPath)
            End Get
            Set(ByVal Value As String)
                Me.ViewState("MultiStateBoxScriptPath") = Value
            End Set
        End Property

        <ClientPropertyName("imgpath"), UrlProperty, ClientProperty, DefaultValue("images/"), Description("Directory to find the images for the MultiStateBox."), Category("Paths")> _
        Public Property ImagePath As String
            Get
                Return If((Strings.Len(Me.ViewState("ImagePath")) <> 0), Conversions.ToString(Me.ViewState("ImagePath")), "images/")
            End Get
            Set(ByVal Value As String)
                Me.ViewState("ImagePath") = Value
            End Set
        End Property

        <PersistenceMode(PersistenceMode.InnerProperty)> _
        Public Overridable ReadOnly Property States As DNNMultiStateCollection
            Get
                If Object.ReferenceEquals(Me.m_States, Nothing) Then
                    Me.m_States = New DNNMultiStateCollection(Me)
                End If
                Return Me.m_States
            End Get
        End Property

        <DefaultValue(True), ClientPropertyName("enabled"), ClientProperty> _
        Public Shadows Property Enabled As Boolean
            Get
                Return Me.m_Enabled
            End Get
            Set(ByVal value As Boolean)
                Me.m_Enabled = value
            End Set
        End Property


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
        Private m_States As DNNMultiStateCollection
        Private m_Enabled As Boolean
        Private m_ValueSet As Boolean
    End Class
End Namespace

