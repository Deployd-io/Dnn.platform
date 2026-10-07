' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System.Web.UI
Imports System.Web.UI.HtmlControls
Imports System.Web.UI.WebControls
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Utilities
	Public Class Globals
	Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

		''' -----------------------------------------------------------------------------
		''' <summary>
		''' Searches control hierarchy from top down to find a control matching the passed in name
		''' </summary>
		''' <param name="objParent">Root control to begin searching</param>
		''' <param name="strControlName">Name of control to look for</param>
		''' <returns></returns>
		''' <remarks>
		''' This differs from FindControlRecursive in that it looks down the control hierarchy, whereas, the 
		''' FindControlRecursive starts at the passed in control and walks the tree up.  Therefore, this function is 
		''' more a expensive task.
		''' </remarks>
		''' <history>
		''' 	[Jon Henning]	9/17/2004	Created
		'''     [Jon Henning]   12/3/2004   Now checking if the control HasControls before calling FindControl.
		'''                                 Using FindControl or accessing the controls collection on controls like
		'''                                 the DataList can cause problems with ViewState
		''' </history>
		''' -----------------------------------------------------------------------------
		Public Shared Function FindControlRecursive(ByVal objParent As Control, ByVal strControlName As String) As Control
			_logger.LogError(">>> Entering FindControlRecursive(objParent={ObjParent},strControlName={StrControlName})", objParent, strControlName)
			_logger.LogError(">>> Entering FindControlRecursive(objParent={ObjParent},strControlName={StrControlName},strClientID={StrClientID})", objParent, strControlName, strClientID)
			_logger.LogWarning("<<< Exiting FindControlRecursive(objParent={ObjParent},strControlName={StrControlName})", objParent, strControlName)
			_logger.LogWarning("<<< Exiting FindControlRecursive(objParent={ObjParent},strControlName={StrControlName},strClientID={StrClientID})", objParent, strControlName, strClientID)
			Return FindControlRecursive(objParent, strControlName, "")
        End Function

        Public Shared Function FindControlRecursive(ByVal objParent As Control, ByVal strControlName As String, ByVal strClientID As String) As Control
            Dim objCtl As Control
            Dim objChild As Control
            objCtl = objParent.FindControl(strControlName)
            If objCtl Is Nothing Then
                For Each objChild In objParent.Controls
                    If objChild.HasControls Then objCtl = FindControlRecursive(objChild, strControlName, strClientID)
                    If Not objCtl Is Nothing AndAlso Len(strClientID) > 0 AndAlso objCtl.ClientID <> strClientID Then objCtl = Nothing
                    If Not objCtl Is Nothing Then Exit For
                Next
            End If
            Return objCtl
        End Function

        Public Shared Function GetAttribute(ByVal objControl As Control, ByVal strAttr As String) As String
			_logger.LogError(">>> Entering GetAttribute(objControl={ObjControl},strAttr={StrAttr})", objControl, strAttr)
            Select Case True
                Case TypeOf objControl Is WebControl
                    Return CType(objControl, WebControl).Attributes(strAttr)
                Case TypeOf objControl Is HtmlControl
                    Return CType(objControl, HtmlControl).Attributes(strAttr)
                Case Else
                    'throw error?
                    Return Nothing
            End Select
			_logger.LogWarning("<<< Exiting GetAttribute(objControl={ObjControl},strAttr={StrAttr})", objControl, strAttr)
        End Function

        Public Shared Sub SetAttribute(ByVal objControl As Control, ByVal strAttr As String, ByVal strValue As String)
			_logger.LogError(">>> Entering SetAttribute(objControl={ObjControl},strAttr={StrAttr},strValue={StrValue})", objControl, strAttr, strValue)
			_logger.LogError("SetAttribute(objControl={ObjControl},strAttr={StrAttr},strValue={StrValue}): strOrigVal → {StrOrigVal}", objControl, strAttr, strValue, strOrigVal)
            Dim strOrigVal As String = GetAttribute(objControl, strAttr)
            If Len(strOrigVal) > 0 Then strValue = strOrigVal & strValue
            Select Case True
                Case TypeOf objControl Is WebControl
                    Dim objCtl As WebControl = CType(objControl, WebControl)
					_logger.LogError("SetAttribute(objControl={ObjControl},strAttr={StrAttr},strValue={StrValue}): objCtl → {ObjCtl}", objControl, strAttr, strValue, objCtl)
                    If objCtl.Attributes(strAttr) Is Nothing Then
                        objCtl.Attributes.Add(strAttr, strValue)
                    Else
                        objCtl.Attributes(strAttr) = strValue
                    End If
                Case TypeOf objControl Is HtmlControl
                    Dim objCtl As HtmlControl = CType(objControl, HtmlControl)
                    If objCtl.Attributes(strAttr) Is Nothing Then
                        objCtl.Attributes.Add(strAttr, strValue)
                    Else
                        objCtl.Attributes(strAttr) = strValue
                    End If
                Case Else
                    'throw error?
            End Select
			_logger.LogWarning("<<< Exiting SetAttribute(objControl={ObjControl},strAttr={StrAttr},strValue={StrValue})", objControl, strAttr, strValue)
        End Sub

        'hack... can we use a method to determine this
        Private Shared m_aryScripts As Hashtable
        Public Shared Function IsEmbeddedScript(ByVal key As String) As Boolean
			_logger.LogError(">>> Entering IsEmbeddedScript(key={Key})", key)
            If m_aryScripts Is Nothing Then
                m_aryScripts = New Hashtable
				_logger.LogDebug("IsEmbeddedScript(key={Key}): m_aryScripts → {M_aryScripts}", key, m_aryScripts)
                m_aryScripts.Add("dnn.js", "")
                m_aryScripts.Add("dnn.dom.positioning.js", "")
                m_aryScripts.Add("dnn.diagnostics.js", "")
                m_aryScripts.Add("dnn.scripts.js", "")
                m_aryScripts.Add("dnn.util.tablereorder.js", "")
                m_aryScripts.Add("dnn.xml.js", "")
                m_aryScripts.Add("dnn.xml.jsparser.js", "")
                m_aryScripts.Add("dnn.xmlhttp.js", "")
                m_aryScripts.Add("dnn.xmlhttp.jsxmlhttprequest.js", "")
                m_aryScripts.Add("dnn.motion.js", "")
            End If
			_logger.LogWarning("<<< Exiting IsEmbeddedScript(key={Key})", key)
            Return m_aryScripts.ContainsKey(key)
        End Function

    End Class
End Namespace