' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports System
Imports System.Globalization
Imports System.Reflection
'Imports System.Configuration
Imports System.Web.Compilation

Namespace DotNetNuke.UI.Utilities

    Public Class Reflection

#Region "Public Shared Methods"

        Public Shared Function CreateObject(ByVal TypeName As String, ByVal CacheKey As String) As Object

            Return CreateObject(TypeName, CacheKey, True)

        End Function

        Public Shared Function CreateObject(ByVal TypeName As String, ByVal CacheKey As String, ByVal UseCache As Boolean) As Object

            ' dynamically create the object
            Return Activator.CreateInstance(CreateType(TypeName, CacheKey, UseCache))

        End Function

        Public Shared Function CreateObject(Of T)() As T
		Logger.LogDebug(">>> Entering CreateObject(Of={Of})", Of)

            ' dynamically create the object
            Return Activator.CreateInstance(Of T)()

        End Function
		Logger.LogDebug("<<< Exiting CreateObject(Of={Of})", Of)

        Public Shared Function CreateType(ByVal TypeName As String) As Type
			Logger.LogDebug(">>> Entering CreateType(TypeName={TypeName})", TypeName)
            Return CreateType(TypeName, "", True, False)
        End Function
		Logger.LogDebug("<<< Exiting CreateType(TypeName={TypeName})", TypeName)

        Public Shared Function CreateType(ByVal TypeName As String, ByVal IgnoreErrors As Boolean) As Type
            Return CreateType(TypeName, "", True, IgnoreErrors)
        End Function

        Public Shared Function CreateType(ByVal TypeName As String, ByVal CacheKey As String, ByVal UseCache As Boolean) As Type
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
				Logger.LogDebug("CreateType(TypeName={TypeName},CacheKey={CacheKey},UseCache={UseCache},IgnoreErrors={IgnoreErrors}): objType → {ObjType}", TypeName, CacheKey, UseCache, IgnoreErrors, objType)
            End If

            ' is the type in the cache?
            If objType Is Nothing Then
                Try
                    ' use reflection to get the type of the class
                    objType = BuildManager.GetType(TypeName, True, True)

                    If UseCache Then
						Dim __sw As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
						Logger.LogInformation("CreateType(TypeName={TypeName},CacheKey={CacheKey},UseCache={UseCache},IgnoreErrors={IgnoreErrors}): query {Op} took {Ms} ms", TypeName, CacheKey, UseCache, IgnoreErrors, "' insert the type into the cache", __sw.ElapsedMilliseconds)
                        ' insert the type into the cache
                        DataCache.SetCache(CacheKey, objType)
                    End If
                Catch exc As Exception
                    ' could not load the type
					Logger.LogError("Exception in CreateType(TypeName={TypeName},CacheKey={CacheKey},UseCache={UseCache},IgnoreErrors={IgnoreErrors}): {Arg4}", TypeName, CacheKey, UseCache, IgnoreErrors, ex.Message)
                    If Not IgnoreErrors Then
                        'LogException(exc)
                        Throw
                    End If
                End Try
            End If

            Return objType
        End Function

        Public Shared Function CreateInstance(ByVal Type As Type) As Object
			Logger.LogDebug(">>> Entering CreateInstance(Type={Type})", Type)
            If Not Type Is Nothing Then
                Return Type.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, Nothing, Nothing, Nothing, Nothing)
            Else
                Return Nothing
            End If
        End Function
		Logger.LogDebug("<<< Exiting CreateInstance(Type={Type})", Type)

        Public Shared Function GetProperty(ByVal Type As Type, ByVal PropertyName As String, ByVal Target As Object) As Object
			Logger.LogDebug(">>> Entering GetProperty(Type={Type},PropertyName={PropertyName},Target={Target})", Type, PropertyName, Target)
            If Not Type Is Nothing Then
                Return Type.InvokeMember(PropertyName, System.Reflection.BindingFlags.GetProperty, Nothing, Target, Nothing, CultureInfo.InvariantCulture)
            Else
                Return Nothing
            End If
        End Function
		Logger.LogDebug("<<< Exiting GetProperty(Type={Type},PropertyName={PropertyName},Target={Target})", Type, PropertyName, Target)

        Public Shared Sub SetProperty(ByVal Type As Type, ByVal PropertyName As String, ByVal Target As Object, ByVal Args() As Object)
			Logger.LogDebug(">>> Entering SetProperty(Type={Type},PropertyName={PropertyName},Target={Target},Args(={Arg})", Type, PropertyName, Target, Args()
            If Not Type Is Nothing Then
                Type.InvokeMember(PropertyName, System.Reflection.BindingFlags.SetProperty, Nothing, Target, Args, CultureInfo.InvariantCulture)
            End If
        End Sub
		Logger.LogDebug("<<< Exiting SetProperty(Type={Type},PropertyName={PropertyName},Target={Target},Args(={Arg})", Type, PropertyName, Target, Args()

        Public Shared Function InvokeMethod(ByVal Type As Type, ByVal MethodName As String, ByVal Target As Object, ByVal Args() As Object) As Object
			Logger.LogDebug(">>> Entering InvokeMethod(Type={Type},MethodName={MethodName},Target={Target},Args(={Arg})", Type, MethodName, Target, Args()
            If Not Type Is Nothing Then
                'Can't use this. in case generic method defined, params match and ambiguous match found error
                'Dim method As MethodInfo = Type.GetMethod(MethodName, Type.GetTypeArray(Args))
                Dim match As Boolean = False
                Dim method As MethodInfo = Nothing
                For Each method In Type.GetMethods()
                    If method.Name = MethodName AndAlso method.IsGenericMethod = False AndAlso method.GetParameters().Length = Args.Length Then
                        If ParamsSameType(method.GetParameters(), Args) Then
                            match = True
							Logger.LogDebug("InvokeMethod(Type={Type},MethodName={MethodName},Target={Target},Args(={Arg3}): match → {{}}", Type, MethodName, Target, Args(, match)
                            Exit For
                        End If
                    End If
                Next
                If match AndAlso Not method Is Nothing Then
                    Return method.Invoke(Target, Args)
                End If
            End If
            Return Nothing
        End Function
		Logger.LogDebug("<<< Exiting InvokeMethod(Type={Type},MethodName={MethodName},Target={Target},Args(={Arg})", Type, MethodName, Target, Args()

        Private Shared Function ParamsSameType(ByVal params() As ParameterInfo, ByVal Args() As Object) As Boolean
			Logger.LogDebug(">>> Entering ParamsSameType(params(={Arg})", params()
            Dim match As Boolean = True
            For i As Integer = 0 To Args.Length - 1
                If params(i).ParameterType.IsAssignableFrom(Args(i).GetType()) = False Then
                    match = False
					Logger.LogDebug("ParamsSameType(params(={Arg0}): match → {{}}", params(, match)
                    Exit For
                End If
            Next
            Return match
        End Function
		Logger.LogDebug("<<< Exiting ParamsSameType(params(={Arg})", params()

        Public Shared Function InvokeGenericMethod(Of T)(ByVal Type As Type, ByVal MethodName As String, ByVal Target As Object, ByVal Args() As Object) As T
			Logger.LogDebug(">>> Entering InvokeGenericMethod(Of={Of})", Of)
            If Not Type Is Nothing Then
                Dim method As MethodInfo = Nothing
                For Each method In Type.GetMembers()
                    If method.Name = MethodName AndAlso method.IsGenericMethod Then Exit For
                Next
                If Not method Is Nothing Then
                    Dim genMethod As MethodInfo = method.MakeGenericMethod(GetType(T))
                    Return CType(genMethod.Invoke(Target, Args), T)
                End If
            End If
            Return Nothing
        End Function
		Logger.LogDebug("<<< Exiting InvokeGenericMethod(Of={Of})", Of)

#End Region

    End Class

End Namespace
