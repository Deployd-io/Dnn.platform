Imports System
Imports Microsoft.Extensions.Logging

Namespace DotNetNuke.UI.Utilities
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)> _
    Public NotInheritable Class ControlMethodClassAttribute : Inherits Attribute
	Private Shared ReadOnly _logger As Microsoft.Extensions.Logging.ILogger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance

        ' Fields
        Private m_FriendlyNamespace As String = ""

        ' Methods
        Public Sub New()
		_logger.LogError(">>> Entering New()")
		_logger.LogError(">>> Entering New(FriendlyNamespace={FriendlyNamespace})", FriendlyNamespace)
		_logger.LogDebug("New(FriendlyNamespace={FriendlyNamespace}): m_FriendlyNamespace → {M_FriendlyNamespace}", FriendlyNamespace, m_FriendlyNamespace)
        _logger.LogWarning("<<< Exiting New(FriendlyNamespace={FriendlyNamespace})", FriendlyNamespace)
        End Sub

        Public Sub New(ByVal FriendlyNamespace As String)
            m_FriendlyNamespace = FriendlyNamespace
        End Sub

        Public Property FriendlyNamespace() As String
            Get
                Return m_FriendlyNamespace
            End Get
            Set(ByVal value As String)
                m_FriendlyNamespace = value
            End Set
        End Property

    End Class

End Namespace

