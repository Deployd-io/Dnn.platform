' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Web.UI
Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(IMenuNodeWriter))

Namespace DotNetNuke.UI.WebControls
    Friend Interface IMenuNodeWriter
        ' Methods
        Sub RenderNode(ByVal writer As HtmlTextWriter, ByVal Node As MenuNode)
		Logger.LogDebug(">>> Entering RenderNode(writer={Writer},Node={Node})", writer, Node)
    End Interface
End Namespace

