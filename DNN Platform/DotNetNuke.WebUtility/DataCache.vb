' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Web
Imports Microsoft.Extensions.Logging
'Copied from DotNetNuke Core

Namespace DotNetNuke.UI.Utilities
	Public Class DataCache
	Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

		Public Shared Function GetCache(ByVal CacheKey As String) As Object
		_logger.LogDebug(">>> Entering GetCache(CacheKey={CacheKey})", CacheKey)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			_logger.LogDebug("<<< Exiting GetCache(CacheKey={CacheKey})", CacheKey)
			Return objCache(CacheKey)

		End Function

		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object)
		_logger.LogDebug(">>> Entering SetCache(CacheKey={CacheKey},objObject={ObjObject})", CacheKey, objObject)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			_logger.LogDebug("<<< Exiting SetCache(CacheKey={CacheKey},objObject={ObjObject})", CacheKey, objObject)
			objCache.Insert(CacheKey, objObject)

		_logger.LogDebug(">>> Entering SetCache(CacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency})", CacheKey, objObject, objDependency)
		_logger.LogDebug("<<< Exiting SetCache(CacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency})", CacheKey, objObject, objDependency)
		End Sub

		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal objDependency As System.Web.Caching.CacheDependency)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			_logger.LogDebug("<<< Exiting SetCache(CacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency})", CacheKey, objObject, objDependency)
			objCache.Insert(CacheKey, objObject, objDependency)

		_logger.LogDebug(">>> Entering SetCache(CacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency},AbsoluteExpiration={AbsoluteExpiration},SlidingExpiration={SlidingExpiration})", CacheKey, objObject, objDependency, AbsoluteExpiration, SlidingExpiration)
		_logger.LogDebug("<<< Exiting SetCache(CacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency},AbsoluteExpiration={AbsoluteExpiration},SlidingExpiration={SlidingExpiration})", CacheKey, objObject, objDependency, AbsoluteExpiration, SlidingExpiration)
		End Sub
		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal objDependency As System.Web.Caching.CacheDependency, ByVal AbsoluteExpiration As Date, ByVal SlidingExpiration As System.TimeSpan)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			_logger.LogDebug("<<< Exiting SetCache(CacheKey={CacheKey},objObject={ObjObject},objDependency={ObjDependency},AbsoluteExpiration={AbsoluteExpiration},SlidingExpiration={SlidingExpiration})", CacheKey, objObject, objDependency, AbsoluteExpiration, SlidingExpiration)
			objCache.Insert(CacheKey, objObject, objDependency, AbsoluteExpiration, SlidingExpiration)

		_logger.LogDebug(">>> Entering SetCache(CacheKey={CacheKey},objObject={ObjObject},SlidingExpiration={SlidingExpiration})", CacheKey, objObject, SlidingExpiration)
		End Sub


		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal SlidingExpiration As Integer)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, Nothing, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromSeconds(SlidingExpiration))

		_logger.LogDebug(">>> Entering SetCache(CacheKey={CacheKey},objObject={ObjObject},AbsoluteExpiration={AbsoluteExpiration})", CacheKey, objObject, AbsoluteExpiration)
		End Sub

		Public Shared Sub SetCache(ByVal CacheKey As String, ByVal objObject As Object, ByVal AbsoluteExpiration As Date)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			objCache.Insert(CacheKey, objObject, Nothing, AbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration)

		End Sub

		Public Shared Sub RemoveCache(ByVal CacheKey As String)
		_logger.LogDebug(">>> Entering RemoveCache(CacheKey={CacheKey})", CacheKey)

			Dim objCache As System.Web.Caching.Cache = HttpRuntime.Cache

			If Not objCache(CacheKey) Is Nothing Then
				objCache.Remove(CacheKey)
				_logger.LogDebug("<<< Exiting RemoveCache(CacheKey={CacheKey})", CacheKey)
			End If

		End Sub
	End Class
End Namespace
