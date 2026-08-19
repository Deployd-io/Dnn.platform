' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.Utilities
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Public Class BaseWebControl
        Inherits WebControl
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        <DebuggerNonUserCode> _
        Public Sub New()
			_logger.LogDebug(">>> Entering New()")
            BaseWebControl.__ENCAddToList(Me)
			_logger.LogDebug("<<< Exiting New()")
        End Sub

        <DebuggerNonUserCode> _
        Private Shared Sub __ENCAddToList(ByVal value As Object)
			_logger.LogDebug(">>> Entering __ENCAddToList(value={Value})", value)
            SyncLock BaseWebControl.__ENCList
                If (BaseWebControl.__ENCList.Count = BaseWebControl.__ENCList.Capacity) Then
                    Dim index As Integer = 0
                    Dim num3 As Integer = (BaseWebControl.__ENCList.Count - 1)
					_logger.LogDebug("__ENCAddToList(value={Value}): num3 → {Num3}", value, num3)
                    Dim num2 As Integer = 0
                    Do While True
                        Dim num4 As Integer = num3
                        If (num2 > num4) Then
                            BaseWebControl.__ENCList.RemoveRange(index, (BaseWebControl.__ENCList.Count - index))
                            BaseWebControl.__ENCList.Capacity = BaseWebControl.__ENCList.Count
                            Exit Do
                        End If
                        Dim reference As WeakReference = BaseWebControl.__ENCList(num2)
						_logger.LogDebug("__ENCAddToList(value={Value}): reference → {Reference}", value, reference)
                        If reference.IsAlive Then
                            If (num2 <> index) Then
                                BaseWebControl.__ENCList(index) = BaseWebControl.__ENCList(num2)
                            End If
                            index += 1
                        End If
                        num2 += 1
                    Loop
                End If
                BaseWebControl.__ENCList.Add(New WeakReference(value))
            End SyncLock
			_logger.LogDebug("<<< Exiting __ENCAddToList(value={Value})", value)
        End Sub

        Public Shared Sub RegisterInitialize(ByVal ctl As Control, ByVal func As String, ByVal propJson As String)
			_logger.LogDebug(">>> Entering RegisterInitialize(ctl={Ctl},func={Func},propJson={PropJson})", ctl, func, propJson)
			_logger.LogDebug("RegisterInitialize(ctl={Ctl},func={Func},propJson={PropJson}): dnn → {Dnn}", ctl, func, propJson, dnn)
            _logger.LogDebug("RegisterInitialize(ctl={Ctl},func={Func},propJson={PropJson}): str → {Str}", ctl, func, propJson, str)
            Dim str As String = $"dnn.setVar('{ctl.ClientID}_p', '{ClientAPI.EscapeForJavascript(propJson)}');"
            ClientAPI.RegisterStartUpScript(ctl.Page, (ctl.ClientID & "_startup"), $"<script type=""text/javascript"">{str}dnn.controls.{func}($get('{ctl.ClientID}'));</script>")
			_logger.LogDebug("<<< Exiting RegisterInitialize(ctl={Ctl},func={Func},propJson={PropJson})", ctl, func, propJson)
        End Sub


        ' Fields
        Private Shared __ENCList As List(Of WeakReference) = New List(Of WeakReference)
    End Class
End Namespace

