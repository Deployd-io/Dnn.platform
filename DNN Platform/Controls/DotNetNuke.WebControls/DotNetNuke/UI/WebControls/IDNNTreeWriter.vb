' Copyright (c) .NET Foundation. All rights reserved.
' Licensed under the MIT License. See LICENSE file in the project root for full license information.

Imports System
Imports System.Collections
Imports System.Web.UI
Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(IDNNTreeWriter))

Namespace DotNetNuke.UI.WebControls
    Friend Interface IDNNTreeWriter
        ' Methods
        Function MarshalledProperties() As Hashtable
		Logger.LogDebug(">>> Entering MarshalledProperties()")
        Sub RenderTree(ByVal writer As HtmlTextWriter, ByVal tree As DnnTree)
		Logger.LogDebug(">>> Entering RenderTree(writer={Writer},tree={Tree})", writer, tree)
    End Interface
End Namespace

