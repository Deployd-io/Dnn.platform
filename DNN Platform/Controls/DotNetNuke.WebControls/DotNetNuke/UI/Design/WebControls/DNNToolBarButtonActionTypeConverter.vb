' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports DotNetNuke.UI.WebControls
Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel
Imports System.Web.UI
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Design.WebControls
    Public Class DNNToolBarButtonActionTypeConverter
        Inherits TypeConverter
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Private Function AttachedControl(ByVal context As ITypeDescriptorContext) As Control
			_logger.LogError(">>> Entering AttachedControl(context={Context})", context)
            If Object.ReferenceEquals(Me.m_objAttached, Nothing) Then
                Dim parent As DNNToolBar = DirectCast(DirectCast(context.Instance, DNNToolBarButton).Parent, DNNToolBar)
				_logger.LogError("AttachedControl(context={Context}): parent → {Parent}", context, parent)
                Me.m_objAttached = parent.AttachedControl
				_logger.LogDebug("AttachedControl(context={Context}): Me.m_objAttached → {Arg1}", context, Me.m_objAttached)
            End If
			_logger.LogWarning("<<< Exiting AttachedControl(context={Context})", context)
            Return Me.m_objAttached
        End Function

        Public Overrides Function GetStandardValues(ByVal context As ITypeDescriptorContext) As StandardValuesCollection
			_logger.LogError(">>> Entering GetStandardValues(context={Context})", context)
            Dim values As New Collection
            Dim commonActions As String() = DirectCast(context.Instance, DNNToolBarButton).CommonActions
			_logger.LogError("GetStandardValues(context={Context}): commonActions → {CommonActions}", context, commonActions)
            Dim index As Integer = 0
            Do While True
                Dim flag As Boolean = (index < commonActions.Length)
                If Not flag Then
                    If Me.SupportsActions(context) Then
                        Dim actions As String() = DirectCast(Me.AttachedControl(context), IDNNToolBarSupportedActions).Actions
						_logger.LogError("GetStandardValues(context={Context}): actions → {Actions}", context, actions)
                        Dim num2 As Integer = 0
                        Do While True
                            flag = (num2 < actions.Length)
							_logger.LogError("GetStandardValues(context={Context}): flag → {Flag}", context, flag)
							_logger.LogDebug("GetStandardValues(context={Context}): flag → {Flag}", context, flag)
                            If Not flag Then
                                Exit Do
                            End If
                            Dim str2 As String = actions(num2)
							_logger.LogError("GetStandardValues(context={Context}): str2 → {Str2}", context, str2)
                            values.Add(str2, Nothing, Nothing, Nothing)
                            num2 += 1
                        Loop
                    End If
                    Return New StandardValuesCollection(values)
                End If
                Dim item As String = commonActions(index)
				_logger.LogError("GetStandardValues(context={Context}): item → {Item}", context, item)
                values.Add(item, Nothing, Nothing, Nothing)
                index += 1
            Loop

			_logger.LogWarning("<<< Exiting GetStandardValues(context={Context})", context)
            Return Nothing
        End Function

        Public Overrides Function GetStandardValuesExclusive(ByVal context As ITypeDescriptorContext) As Boolean
			_logger.LogError(">>> Entering GetStandardValuesExclusive(context={Context})", context)
			_logger.LogWarning("<<< Exiting GetStandardValuesExclusive(context={Context})", context)
            Return False
        End Function

        Public Overrides Function GetStandardValuesSupported(ByVal context As ITypeDescriptorContext) As Boolean
			_logger.LogError(">>> Entering GetStandardValuesSupported(context={Context})", context)
			_logger.LogWarning("<<< Exiting GetStandardValuesSupported(context={Context})", context)
            Return Me.SupportsActions(context)
        End Function

        Private Function SupportsActions(ByVal context As ITypeDescriptorContext) As Boolean
			_logger.LogError(">>> Entering SupportsActions(context={Context})", context)
			_logger.LogWarning("<<< Exiting SupportsActions(context={Context})", context)
            Return ((Not Me.AttachedControl(context) Is Nothing) AndAlso TypeOf Me.AttachedControl(context) Is IDNNToolBarSupportedActions)
        End Function


        ' Fields
        Private m_objAttached As Control
    End Class
End Namespace

