' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.
private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));

Imports System.Web
'Copied from DotNetNuke Core

Namespace DotNetNuke.UI.Utilities
	Public Class DataCache

		Public Shared Function GetCache(ByVal CacheKey As String) As Object
		Logger.LogDebug(">>> Entering GetCache(CacheKey={CacheKey})", CacheKey)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			Return objCache(CacheKey)

		End Function
		Logger.LogDebug("<<< Exiting GetCache(CacheKey={CacheKey})", CacheKey)

		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object)
		Logger.LogDebug(">>> Entering SetCache(CacheKey={CacheKey},objObject={ObjObject})", CacheKey, objObject)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject)

		End Sub
		Logger.LogDebug("<<< Exiting SetCache(CacheKey={CacheKey},objObject={ObjObject})", CacheKey, objObject)

		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal objDependency As System.Web.Caching.CacheDependency)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, objDependency)

		End Sub
		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal objDependency As System.Web.Caching.CacheDependency, ByVal AbsoluteExpiration As Date, ByVal SlidingExpiration As System.TimeSpan)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, objDependency, AbsoluteExpiration, SlidingExpiration)

		End Sub


		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal SlidingExpiration As Integer)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, Nothing, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(SlidingExpiration))

		End Sub

		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal AbsoluteExpiration As Date)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, Nothing, AbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration)

		End Sub

		Public Shared Sub RemoveCache(ByVal CacheKey As String)
		Logger.LogDebug(">>> Entering RemoveCache(CacheKey={CacheKey})", CacheKey)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			If Not objCache(CacheKey) Is Nothing Then
				objCache.Remove(CacheKey)
			End If

		End Sub
		Logger.LogDebug("<<< Exiting RemoveCache(CacheKey={CacheKey})", CacheKey)
	End Class
End Namespace
