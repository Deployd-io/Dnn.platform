Imports System
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Utilities
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)> _
    Public NotInheritable Class ControlMethodAttribute : Inherits Attribute
		Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance
        Public Sub New()
		_logger.LogError(">>> Entering New()")
        End Sub
    End Class

End Namespace

