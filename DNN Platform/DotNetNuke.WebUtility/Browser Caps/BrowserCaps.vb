' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Globalization
Imports System.IO
Imports System.Web.Caching
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Xml.XPath
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Utilities

    <Serializable(), XmlRoot("capabilities")>
    Public Class BrowserCaps
	Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

#Region "Private Members"

        Private m_objFunctionality As FunctionalityCollection
        Private m_objFunctionalityDict As Hashtable
        Private Const CLIENTAPI_CACHE_KEY As String = "ClientAPICaps"

#End Region

#Region "Public Properties"

        <XmlElement("functionality")>
        Public Property Functionality() As FunctionalityCollection
            Get
                Return m_objFunctionality
            End Get
            Set(ByVal Value As FunctionalityCollection)
                m_objFunctionality = Value
            End Set
        End Property

        Public ReadOnly Property FunctionalityDictionary() As Hashtable
            Get
                If m_objFunctionalityDict Is Nothing Then
                    m_objFunctionalityDict = New Hashtable
                End If
                Return m_objFunctionalityDict
            End Get
        End Property
#End Region

#Region "Private Shared Methods"

        Private Shared Function GetBrowser(ByVal objNav As XPathNavigator) As Browser
			_logger.LogError(">>> Entering GetBrowser(objNav={ObjNav})", objNav)
            Dim objBrowser As New Browser()
            objBrowser.Contains = objNav.GetAttribute("contains", "")
            objBrowser.Name = objNav.GetAttribute("nm", "")
            Dim strMinVersion As String = objNav.GetAttribute("minversion", "")
			_logger.LogError("GetBrowser(objNav={ObjNav}): strMinVersion → {StrMinVersion}", objNav, strMinVersion)
            'If Not String.IsNullOrEmpty(strMinVersion) Then    '.NET 2.0 specific
            If Len(strMinVersion) > 0 Then
                objBrowser.MinVersion = Double.Parse(strMinVersion, CultureInfo.InvariantCulture)
            End If
			_logger.LogWarning("<<< Exiting GetBrowser(objNav={ObjNav})", objNav)
            Return objBrowser
        End Function

#End Region

#Region "Shared Methods"

        Public Shared Function GetBrowserCaps() As BrowserCaps
		_logger.LogError(">>> Entering GetBrowserCaps()")

            Dim objCaps As BrowserCaps = CType(DataCache.GetCache(CLIENTAPI_CACHE_KEY), BrowserCaps)
            Dim objFunc As FunctionalityInfo = Nothing

            If (objCaps Is Nothing) Then
                Dim strPath As String = String.Empty

                Try
                    strPath = System.Web.HttpContext.Current.Server.MapPath(ClientAPI.ScriptPath & "/ClientAPICaps.config")
					_logger.LogDebug("GetBrowserCaps(): strPath → {StrPath}", strPath)
                Catch ex As Exception
					_logger.LogError("Exception in GetBrowserCaps(): {Arg0}", ex.Message)
                    'ignore error - worried about people with reverse proxies and such...
                End Try

                Dim objDoc As XPathDocument = Nothing
                Dim fileExists As Boolean = File.Exists(strPath)
					_logger.LogError("GetBrowserCaps(): fileExists → {FileExists}", fileExists)
                    objCaps = New BrowserCaps()
					_logger.LogDebug("GetBrowserCaps(): objCaps → {ObjCaps}", objCaps)
					_logger.LogError("GetBrowserCaps(): objCaps → {ObjCaps}", objCaps)
                    objCaps.Functionality = New FunctionalityCollection()
                    If fileExists Then

                        'Create a FileStream for the Config file
                        Using objReader = New FileStream(strPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                            Using xmlReader As XmlReader = XmlReader.Create(objReader)
                                objDoc = New XPathDocument(xmlReader)
								_logger.LogDebug("GetBrowserCaps(): objDoc → {ObjDoc}", objDoc)
                            End Using
                        End Using
                    Else
                        Using tr = New StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ClientAPICaps.config"))
                            Using xmlReader As XmlReader = XmlReader.Create(tr)
                                objDoc = New XPathDocument(xmlReader)
                            End Using
                        End Using 
                    End If

                    If Not objDoc Is Nothing Then
                        For Each objNavFunc As XPathNavigator In objDoc.CreateNavigator.Select("//functionality")
                            objFunc = New FunctionalityInfo()
							_logger.LogDebug("GetBrowserCaps(): objFunc → {ObjFunc}", objFunc)
                            objFunc.Functionality = DirectCast(System.Enum.Parse(GetType(ClientAPI.ClientFunctionality), objNavFunc.GetAttribute("nm", "")), ClientAPI.ClientFunctionality)
                            objFunc.Desc = objNavFunc.GetAttribute("desc", "")

                            objFunc.Supports = New BrowserCollection()
                            For Each objNavSupports As XPathNavigator In objNavFunc.CreateNavigator.Select("supports/browser")
                                objFunc.Supports.Add(GetBrowser(objNavSupports))
                            Next

                            objFunc.Excludes = New BrowserCollection()
                            For Each objNavExcludes As XPathNavigator In objNavFunc.CreateNavigator.Select("excludes/browser")
                                objFunc.Excludes.Add(GetBrowser(objNavExcludes))
                            Next

                            objCaps.Functionality.Add(objFunc)
                        Next
                    End If

                ' Set back into Cache
                If fileExists Then
                    DataCache.SetCache(CLIENTAPI_CACHE_KEY, objCaps, New CacheDependency(strPath))
                End If
            End If

			_logger.LogWarning("<<< Exiting GetBrowserCaps()")
            Return objCaps

        End Function

#End Region

    End Class

End Namespace
