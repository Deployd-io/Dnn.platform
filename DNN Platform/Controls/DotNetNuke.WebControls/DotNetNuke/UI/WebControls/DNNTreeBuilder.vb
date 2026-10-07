' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Web.UI
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.WebControls
    Public Class DNNTreeBuilder
        Inherits ControlBuilder
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        ' Methods
        Public Overrides Function GetChildControlType(ByVal tagName As String, ByVal attribs As IDictionary) As Type
			_logger.LogError(">>> Entering GetChildControlType(tagName={TagName},attribs={Attribs})", tagName, attribs)
			_logger.LogWarning("<<< Exiting GetChildControlType(tagName={TagName},attribs={Attribs})", tagName, attribs)
            Return If(Not tagName.EndsWith("TreeNode", StringComparison.OrdinalIgnoreCase), Nothing, GetType(TreeNode))
        End Function

    End Class
End Namespace

