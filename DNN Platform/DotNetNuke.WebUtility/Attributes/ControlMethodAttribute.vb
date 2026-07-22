Imports System

private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
Namespace DotNetNuke.UI.Utilities
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)> _
    Public NotInheritable Class ControlMethodAttribute : Inherits Attribute
        Public Sub New()
		Logger.LogDebug(">>> Entering New()")
        End Sub
    End Class

End Namespace

