' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Globalization
Imports System.Reflection
'Imports System.Configuration
Imports System.Web.Compilation
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Utilities

    Public Class Reflection
	Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

#Region "Public Shared Methods"

        Public Shared Function CreateObject(ByVal TypeName As String, ByVal CacheKey As String) As Object
		_logger.LogError(">>> Entering CreateObject(TypeName={TypeName},CacheKey={CacheKey})", TypeName, CacheKey)

			_logger.LogError(">>> Entering CreateObject(TypeName={TypeName},CacheKey={CacheKey},UseCache={UseCache})", TypeName, CacheKey, UseCache)
			_logger.LogWarning("<<< Exiting CreateObject(TypeName={TypeName},CacheKey={CacheKey})", TypeName, CacheKey)
			_logger.LogWarning("<<< Exiting CreateObject(TypeName={TypeName},CacheKey={CacheKey},UseCache={UseCache})", TypeName, CacheKey, UseCache)
			_logger.LogWarning("<<< Exiting CreateObject()")
            Return CreateObject(TypeName, CacheKey, True)

        End Function

        Public Shared Function CreateObject(ByVal TypeName As String, ByVal CacheKey As String, ByVal UseCache As Boolean) As Object

            ' dynamically create the object
			_logger.LogError(">>> Entering CreateObject()")
            Return Activator.CreateInstance(CreateType(TypeName, CacheKey, UseCache))

        End Function

        Public Shared Function CreateObject(Of T)() As T

            ' dynamically create the object
            Return Activator.CreateInstance(Of T)()

        End Function

        Public Shared Function CreateType(ByVal TypeName As String) As Type
			_logger.LogError(">>> Entering CreateType(TypeName={TypeName})", TypeName)
			_logger.LogError(">>> Entering CreateType(TypeName={TypeName},IgnoreErrors={IgnoreErrors})", TypeName, IgnoreErrors)
			_logger.LogWarning("<<< Exiting CreateType(TypeName={TypeName})", TypeName)
			_logger.LogWarning("<<< Exiting CreateType(TypeName={TypeName},IgnoreErrors={IgnoreErrors})", TypeName, IgnoreErrors)
			_logger.LogWarning("<<< Exiting CreateType(TypeName={TypeName},CacheKey={CacheKey},UseCache={UseCache})", TypeName, CacheKey, UseCache)
			_logger.LogWarning("<<< Exiting CreateType(TypeName={TypeName},CacheKey={CacheKey},UseCache={UseCache},IgnoreErrors={IgnoreErrors})", TypeName, CacheKey, UseCache, IgnoreErrors)
            Return CreateType(TypeName, "", True, False)
        End Function

        Public Shared Function CreateType(ByVal TypeName As String, ByVal IgnoreErrors As Boolean) As Type
			_logger.LogError(">>> Entering CreateType(TypeName={TypeName},CacheKey={CacheKey},UseCache={UseCache})", TypeName, CacheKey, UseCache)
            Return CreateType(TypeName, "", True, IgnoreErrors)
        End Function

        Public Shared Function CreateType(ByVal TypeName As String, ByVal CacheKey As String, ByVal UseCache As Boolean) As Type
			_logger.LogError(">>> Entering CreateType(TypeName={TypeName},CacheKey={CacheKey},UseCache={UseCache},IgnoreErrors={IgnoreErrors})", TypeName, CacheKey, UseCache, IgnoreErrors)
            Return CreateType(TypeName, CacheKey, UseCache, False)
        End Function

        Public Shared Function CreateType(ByVal TypeName As String, ByVal CacheKey As String, ByVal UseCache As Boolean, ByVal IgnoreErrors As Boolean) As Type

            If CacheKey = "" Then
                CacheKey = TypeName
            End If

            Dim objType As Type = Nothing

            ' use the cache for performance
            If UseCache Then
                objType = CType(DataCache.GetCache(CacheKey), Type)
            End If

            ' is the type in the cache?
            If objType Is Nothing Then
                Try
                    ' use reflection to get the type of the class
                    objType = BuildManager.GetType(TypeName, True, True)

                    If UseCache Then
                        ' insert the type into the cache
                        DataCache.SetCache(CacheKey, objType)
                    End If
                Catch exc As Exception
                    ' could not load the type
                    If Not IgnoreErrors Then
                        'LogException(exc)
                        Throw
                    End If
                End Try
            End If

            Return objType
        End Function

        Public Shared Function CreateInstance(ByVal Type As Type) As Object
			_logger.LogError(">>> Entering CreateInstance(Type={Type})", Type)
            If Not Type Is Nothing Then
                Return Type.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, Nothing, Nothing, Nothing, Nothing)
            Else
                Return Nothing
            End If
			_logger.LogWarning("<<< Exiting CreateInstance(Type={Type})", Type)
        End Function

        Public Shared Function GetProperty(ByVal Type As Type, ByVal PropertyName As String, ByVal Target As Object) As Object
			_logger.LogError(">>> Entering GetProperty(Type={Type},PropertyName={PropertyName},Target={Target})", Type, PropertyName, Target)
            If Not Type Is Nothing Then
                Return Type.InvokeMember(PropertyName, System.Reflection.BindingFlags.GetProperty, Nothing, Target, Nothing, CultureInfo.InvariantCulture)
            Else
                Return Nothing
            End If
			_logger.LogWarning("<<< Exiting GetProperty(Type={Type},PropertyName={PropertyName},Target={Target})", Type, PropertyName, Target)
        End Function

        Public Shared Sub SetProperty(ByVal Type As Type, ByVal PropertyName As String, ByVal Target As Object, ByVal Args() As Object)
			_logger.LogError(">>> Entering SetProperty(Type={Type},PropertyName={PropertyName},Target={Target},Args={Args})", Type, PropertyName, Target, Args)
            If Not Type Is Nothing Then
                Type.InvokeMember(PropertyName, System.Reflection.BindingFlags.SetProperty, Nothing, Target, Args, CultureInfo.InvariantCulture)
            End If
			_logger.LogWarning("<<< Exiting SetProperty(Type={Type},PropertyName={PropertyName},Target={Target},Args={Args})", Type, PropertyName, Target, Args)
        End Sub

        Public Shared Function InvokeMethod(ByVal Type As Type, ByVal MethodName As String, ByVal Target As Object, ByVal Args() As Object) As Object
			_logger.LogError(">>> Entering InvokeMethod(Type={Type},MethodName={MethodName},Target={Target},Args={Args})", Type, MethodName, Target, Args)
            If Not Type Is Nothing Then
                'Can't use this. in case generic method defined, params match and ambiguous match found error
                'Dim method As MethodInfo = Type.GetMethod(MethodName, Type.GetTypeArray(Args))
                Dim match As Boolean = False
                Dim method As MethodInfo = Nothing
                For Each method In Type.GetMethods()
                    If method.Name = MethodName AndAlso method.IsGenericMethod = False AndAlso method.GetParameters().Length = Args.Length Then
                        If ParamsSameType(method.GetParameters(), Args) Then
                            match = True
							_logger.LogDebug("InvokeMethod(Type={Type},MethodName={MethodName},Target={Target},Args={Args}): match → {Match}", Type, MethodName, Target, Args, match)
                            Exit For
                        End If
                    End If
                Next
                If match AndAlso Not method Is Nothing Then
                    Return method.Invoke(Target, Args)
                End If
            End If
			_logger.LogWarning("<<< Exiting InvokeMethod(Type={Type},MethodName={MethodName},Target={Target},Args={Args})", Type, MethodName, Target, Args)
            Return Nothing
        End Function

        Private Shared Function ParamsSameType(ByVal params() As ParameterInfo, ByVal Args() As Object) As Boolean
			_logger.LogError(">>> Entering ParamsSameType(params={Params},Args={Args})", params, Args)
            Dim match As Boolean = True
            For i As Integer = 0 To Args.Length - 1
                If params(i).ParameterType.IsAssignableFrom(Args(i).GetType()) = False Then
                    match = False
					_logger.LogDebug("ParamsSameType(params={Params},Args={Args}): match → {Match}", params, Args, match)
                    Exit For
                End If
            Next
			_logger.LogWarning("<<< Exiting ParamsSameType(params={Params},Args={Args})", params, Args)
            Return match
        End Function

        Public Shared Function InvokeGenericMethod(Of T)(ByVal Type As Type, ByVal MethodName As String, ByVal Target As Object, ByVal Args() As Object) As T
			_logger.LogError(">>> Entering InvokeGenericMethod(Type={Type},MethodName={MethodName},Target={Target},Args={Args})", Type, MethodName, Target, Args)
            If Not Type Is Nothing Then
                Dim method As MethodInfo = Nothing
                For Each method In Type.GetMembers()
                    If method.Name = MethodName AndAlso method.IsGenericMethod Then Exit For
                Next
                If Not method Is Nothing Then
                    Dim genMethod As MethodInfo = method.MakeGenericMethod(GetType(T))
					_logger.LogError("InvokeGenericMethod(Type={Type},MethodName={MethodName},Target={Target},Args={Args}): genMethod → {GenMethod}", Type, MethodName, Target, Args, genMethod)
                    Return CType(genMethod.Invoke(Target, Args), T)
                End If
            End If
			_logger.LogWarning("<<< Exiting InvokeGenericMethod(Type={Type},MethodName={MethodName},Target={Target},Args={Args})", Type, MethodName, Target, Args)
            Return Nothing
        End Function

#End Region

    End Class

End Namespace
