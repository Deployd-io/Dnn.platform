Imports System

Namespace DotNetNuke.UI.Utilities
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)> _
    Public NotInheritable Class ControlMethodAttribute : Inherits Attribute
		Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(ControlMethodAttribute))
        Public Sub New()
		Logger.LogDebug(">>> Entering New()")
        End Sub
    End Class

End Namespace

