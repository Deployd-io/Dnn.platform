' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Reflection
Imports System.Web
Imports System.Web.UI
Imports Microsoft.Extensions.Logging


Namespace DotNetNuke.UI.Utilities

    Public Class MSAJAX
	Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

#Region "Member Variables"
        Private Shared m_ScriptManagerType As Type
        Private Shared m_ScriptReferenceType As Type
        Private Shared m_ScriptReferenceCollectionType As Type
        Private Shared m_JavaScriptSerializerType As Type
        Private Shared m_AlternateJavaScriptSerializerType As Type
        Private Shared m_Installed As Integer = -1
#End Region

#Region "Private Properties"

        Private Shared ReadOnly Property ScriptManagerType() As Type
            Get
                If m_ScriptManagerType Is Nothing Then
                    m_ScriptManagerType = Reflection.CreateType("System.Web.UI.ScriptManager", False)
                End If
                Return m_ScriptManagerType
            End Get
        End Property

        Private Shared ReadOnly Property ScriptReferenceCollectionType() As Type
            Get
                If m_ScriptReferenceCollectionType Is Nothing Then
                    m_ScriptReferenceCollectionType = Reflection.CreateType("System.Web.UI.ScriptReferenceCollection", True)
                End If
                Return m_ScriptReferenceCollectionType
            End Get
        End Property

        Private Shared ReadOnly Property ScriptReferenceType() As Type
            Get
                If m_ScriptReferenceType Is Nothing Then
                    m_ScriptReferenceType = Reflection.CreateType("System.Web.UI.ScriptReference", True)
                End If
                Return m_ScriptReferenceType
            End Get
        End Property

        Private Shared ReadOnly Property JavaScriptSerializerType() As Type
            Get
                If m_JavaScriptSerializerType Is Nothing Then
                    m_JavaScriptSerializerType = Reflection.CreateType("System.Web.Script.Serialization.JavaScriptSerializer", True)
                End If
                Return m_JavaScriptSerializerType
            End Get
        End Property

        Private Shared ReadOnly Property AlternateJavaScriptSerializerType() As Type
            Get
                If m_AlternateJavaScriptSerializerType Is Nothing Then
                    m_AlternateJavaScriptSerializerType = Reflection.CreateType("Newtonsoft.Json.JavaScriptConvert", True)
                End If
                Return m_AlternateJavaScriptSerializerType
            End Get
        End Property

        Public Shared ReadOnly Property IsInstalled() As Boolean
            Get
                If m_Installed = -1 Then
                    'Dim capiPath As String = System.IO.Path.GetDirectoryName(GetType(ClientAPI.ClientFunctionality).Assembly.CodeBase)
                    'Dim msajaxPath As String = System.IO.Path.GetDirectoryName(ScriptManagerType.Assembly.CodeBase)
                    If ScriptManagerType.Assembly.GlobalAssemblyCache = False Then
                        'If capiPath = msajaxPath Then
                        'if msajax loaded from same path as clientapi, then we need to be in full trust

                        Try
                            'demand a high level permission
                            Dim perm As AspNetHostingPermission = New AspNetHostingPermission(AspNetHostingPermissionLevel.High)
                            perm.Demand()
                            m_Installed = 1
                        Catch ex As Exception
                            m_Installed = 0
                        End Try
                    Else
                        m_Installed = 1
                    End If

                End If
                Return m_Installed = 1
                'Return Not ScriptManagerType() Is Nothing
            End Get
        End Property

#End Region

#Region "Public Methods"

        Private Shared Function ScriptManagerControl(ByVal objPage As Page) As Control
			_logger.LogError(">>> Entering ScriptManagerControl(objPage={ObjPage})", objPage)
			_logger.LogError("ScriptManagerControl(objPage={ObjPage}): method → {Method}", objPage, method)
            Dim method As MethodInfo = ScriptManagerType.GetMethod("GetCurrent")
			_logger.LogWarning("<<< Exiting ScriptManagerControl(objPage={ObjPage})", objPage)
            Return CType(method.Invoke(ScriptManagerType, New Object() {objPage}), Control)
        End Function

        Public Shared Sub Register(ByVal objPage As Page)
			_logger.LogError(">>> Entering Register(objPage={ObjPage})", objPage)
            If IsInstalled() Then
                If ScriptManagerControl(objPage) Is Nothing Then
                    Dim sm As Control = ScriptManagerControl(objPage)
                    If sm Is Nothing Then
                        sm = CType(Reflection.CreateInstance(ScriptManagerType), Control)
						_logger.LogError("Register(objPage={ObjPage}): sm → {Sm}", objPage, sm)
                    End If
                    If Not sm Is Nothing Then
                        sm.ID = "ScriptManager"
						_logger.LogDebug("Register(objPage={ObjPage}): sm.ID → {Arg1}", objPage, sm.ID)
                        'objPage.Form.Controls.AddAt(0, sm)
                        objPage.Form.Controls.Add(sm)
                        HttpContext.Current.Items("System.Web.UI.ScriptManager") = True 'Let DNN know we added it
                    End If
                End If
            Else
                If ClientAPI.UseExternalScripts Then 'TEST!!!
                    MSAJAX.RegisterClientScript(objPage, ClientAPI.ScriptPath & "MicrosoftAjax.js")
                    MSAJAX.RegisterClientScript(objPage, ClientAPI.ScriptPath & "MicrosoftAjaxWebForms.js")
                Else
                    MSAJAX.RegisterClientScript(objPage, "MicrosoftAjax.js", "DotNetNuke.WebUtility")
                    MSAJAX.RegisterClientScript(objPage, "MicrosoftAjaxWebForms.js", "DotNetNuke.WebUtility")
                End If
                objPage.ClientScript.RegisterStartupScript(objPage.GetType(), "MSAJAXInit", "Sys.Application.initialize();", True)
            End If
			_logger.LogWarning("<<< Exiting Register(objPage={ObjPage})", objPage)
        End Sub

        Public Shared Sub SetScriptManagerProperty(ByVal objPage As Page, ByVal PropertyName As String, ByVal Args() As Object)
			_logger.LogError(">>> Entering SetScriptManagerProperty(objPage={ObjPage},PropertyName={PropertyName},Args={Args})", objPage, PropertyName, Args)
            If IsInstalled() Then
                Register(objPage)
                If Not ScriptManagerControl(objPage) Is Nothing Then
                    Reflection.SetProperty(ScriptManagerType, PropertyName, ScriptManagerControl(objPage), Args)
                End If
            End If
			_logger.LogWarning("<<< Exiting SetScriptManagerProperty(objPage={ObjPage},PropertyName={PropertyName},Args={Args})", objPage, PropertyName, Args)
        End Sub

        Public Shared Sub RegisterClientScript(ByVal objPage As Page, ByVal Path As String)
			_logger.LogError(">>> Entering RegisterClientScript(objPage={ObjPage},Path={Path})", objPage, Path)
            If IsInstalled() Then
                Register(objPage)
                Dim ref As Object = GetScriptReference()
				_logger.LogError("RegisterClientScript(objPage={ObjPage},Path={Path}): ref → {Ref}", objPage, Path, ref)
                MSAJAX.SetScriptReferenceProperty(ref, "Path", Path)
                MSAJAX.SetScriptReferenceProperty(ref, "IgnoreScriptPath", False)
                RegisterClientScript(objPage, ref)
            Else
                If objPage.ClientScript.IsClientScriptBlockRegistered(Path) = False Then
                    objPage.ClientScript.RegisterClientScriptInclude(Path, Path)
                End If
            End If
			_logger.LogWarning("<<< Exiting RegisterClientScript(objPage={ObjPage},Path={Path})", objPage, Path)
			_logger.LogError(">>> Entering RegisterClientScript(objPage={ObjPage},Name={Name},Assembly={Assembly})", objPage, Name, Assembly)
			_logger.LogError("RegisterClientScript(objPage={ObjPage},Name={Name},Assembly={Assembly}): ref → {Ref}", objPage, Name, Assembly, ref)
			_logger.LogWarning("<<< Exiting RegisterClientScript(objPage={ObjPage},Name={Name},Assembly={Assembly})", objPage, Name, Assembly)
        End Sub

        Public Shared Sub RegisterStartupScript(ByVal objPage As Page, ByVal Key As String, ByVal Script As String)
			_logger.LogError(">>> Entering RegisterStartupScript(objPage={ObjPage},Key={Key},Script={Script})", objPage, Key, Script)
            If IsInstalled() Then
                Dim methods() As MethodInfo = ScriptManagerType.GetMethods()
                For Each method As MethodInfo In methods
                    If (method.Name = "RegisterStartupScript") Then
                        method.Invoke(Nothing, New Object() {objPage, objPage.GetType(), Key, Script, False})
                        Exit For
                    End If
                Next
            Else
                objPage.ClientScript.RegisterStartupScript(objPage.GetType(), Key, Script)
            End If
			_logger.LogWarning("<<< Exiting RegisterStartupScript(objPage={ObjPage},Key={Key},Script={Script})", objPage, Key, Script)
        End Sub

        Public Shared Sub RegisterClientScript(ByVal objPage As Page, ByVal Name As String, ByVal Assembly As String)
            If IsInstalled() Then
                Register(objPage)
                Dim ref As Object = GetScriptReference()
                MSAJAX.SetScriptReferenceProperty(ref, "Name", Name)
                MSAJAX.SetScriptReferenceProperty(ref, "Assembly", Assembly)
                RegisterClientScript(objPage, ref)
            Else
                If objPage.ClientScript.IsClientScriptBlockRegistered(Name) = False Then
                    objPage.ClientScript.RegisterClientScriptResource(Reflection.CreateType(Assembly, True), Name)
                End If
            End If
			_logger.LogError(">>> Entering RegisterClientScript(objPage={ObjPage},Ref={Ref})", objPage, Ref)
			_logger.LogError("RegisterClientScript(objPage={ObjPage},Ref={Ref}): sm → {Sm}", objPage, Ref, sm)
			_logger.LogError("RegisterClientScript(objPage={ObjPage},Ref={Ref}): scripts → {Scripts}", objPage, Ref, scripts)
			_logger.LogWarning("<<< Exiting RegisterClientScript(objPage={ObjPage},Ref={Ref})", objPage, Ref)
        End Sub

        Private Shared Sub RegisterClientScript(ByVal objPage As Page, ByVal Ref As Object)
            If IsInstalled() Then
                Register(objPage)
                Dim sm As Object = ScriptManagerControl(objPage)
                Dim scripts As Object = ScriptManagerScripts(sm)
                If CBool(Reflection.InvokeMethod(ScriptReferenceCollectionType, "Contains", scripts, New Object() {Ref})) = False Then
                    Reflection.InvokeMethod(ScriptReferenceCollectionType, "Add", ScriptManagerScripts(sm), New Object() {Ref})
                End If
            End If
        End Sub

        Private Shared Function GetScriptReference() As Object
			_logger.LogError(">>> Entering GetScriptReference()")
			_logger.LogWarning("<<< Exiting GetScriptReference()")
            Return Reflection.CreateInstance(ScriptReferenceType)
        End Function

        Private Shared Sub SetScriptReferenceProperty(ByVal Ref As Object, ByVal Name As String, ByVal Value As Object)
			_logger.LogError(">>> Entering SetScriptReferenceProperty(Ref={Ref},Name={Name},Value={Value})", Ref, Name, Value)
            Reflection.SetProperty(ScriptReferenceType, Name, Ref, New Object() {Value})
			_logger.LogWarning("<<< Exiting SetScriptReferenceProperty(Ref={Ref},Name={Name},Value={Value})", Ref, Name, Value)
        End Sub

        Private Shared Function ScriptManagerScripts(ByVal objScriptManager As Object) As Object
			_logger.LogError(">>> Entering ScriptManagerScripts(objScriptManager={ObjScriptManager})", objScriptManager)
			_logger.LogWarning("<<< Exiting ScriptManagerScripts(objScriptManager={ObjScriptManager})", objScriptManager)
            Return Reflection.GetProperty(ScriptManagerType, "Scripts", objScriptManager)
        End Function

        Public Shared Function Deserialize(Of T)(ByVal Data As String) As T
			_logger.LogError(">>> Entering Deserialize(Data={Data})", Data)
            If IsInstalled() Then
                Dim ser As Object = Reflection.CreateInstance(JavaScriptSerializerType)
				_logger.LogError("Deserialize(Data={Data}): ser → {Ser}", Data, ser)
                Return Reflection.InvokeGenericMethod(Of T)(JavaScriptSerializerType, "Deserialize", ser, New Object() {Data})
            ElseIf AlternateJavaScriptSerializerType IsNot Nothing Then
                Try
                    Return CType(Reflection.InvokeMethod(AlternateJavaScriptSerializerType, "DeserializeObject", Nothing, New Object() {Data, GetType(T)}), T)
                Catch ex As Exception
					_logger.LogError("Exception in Deserialize(Data={Data}): {Arg1}", Data, ex.Message)
                    'if initial attempt did not work, try one more time, without specifying a type
                    Return CType(Reflection.InvokeMethod(AlternateJavaScriptSerializerType, "DeserializeObject", Nothing, New Object() {Data}), T)
                End Try
			_logger.LogWarning("<<< Exiting Deserialize(Data={Data})", Data)
            End If

        End Function

        Public Shared Function DeserializeObject(ByVal Data As String) As Object
			_logger.LogError(">>> Entering DeserializeObject(Data={Data})", Data)
            If IsInstalled() Then
                Dim ser As Object = Reflection.CreateInstance(JavaScriptSerializerType)
				_logger.LogError("DeserializeObject(Data={Data}): ser → {Ser}", Data, ser)
                Return Reflection.InvokeMethod(JavaScriptSerializerType, "DeserializeObject", ser, New Object() {Data})
            ElseIf AlternateJavaScriptSerializerType IsNot Nothing Then
                Return Reflection.InvokeMethod(AlternateJavaScriptSerializerType, "DeserializeObject", Nothing, New Object() {Data})
            End If
			_logger.LogWarning("<<< Exiting DeserializeObject(Data={Data})", Data)
            Return Nothing
        End Function

        Public Shared Function Serialize(ByVal Obj As Object) As String
			_logger.LogError(">>> Entering Serialize(Obj={Obj})", Obj)
            Dim ser As Object
            If IsInstalled() Then
                ser = Reflection.CreateInstance(JavaScriptSerializerType)
				_logger.LogDebug("Serialize(Obj={Obj}): ser → {Ser}", Obj, ser)
                Return CType(Reflection.InvokeMethod(JavaScriptSerializerType, "Serialize", ser, New Object() {Obj}), String)
            ElseIf AlternateJavaScriptSerializerType IsNot Nothing Then
                Dim json As String = CType(Reflection.InvokeMethod(AlternateJavaScriptSerializerType, "SerializeObject", Nothing, New Object() {Obj}), String)
				_logger.LogError("Serialize(Obj={Obj}): json → {Json}", Obj, json)
                'HACK
                Return json.Replace("'", "\u0027") 'make same as MSAJAX - escape single quote
            End If
			_logger.LogWarning("<<< Exiting Serialize(Obj={Obj})", Obj)
            Return ""
        End Function

#End Region

    End Class
End Namespace