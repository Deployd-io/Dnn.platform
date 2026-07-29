Imports System

Namespace DotNetNuke.UI.Utilities
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)> _
    Public NotInheritable Class ControlMethodClassAttribute : Inherits Attribute
	Private Shared ReadOnly Logger As ILog = LoggerSource.Instance.GetLogger(GetType(ControlMethodClassAttribute))

        ' Fields
        Private m_FriendlyNamespace As String = ""

        ' Methods
        Public Sub New()
		Logger.LogDebug(">>> Entering New()")
		Logger.LogDebug(">>> Entering New(FriendlyNamespace={FriendlyNamespace})", FriendlyNamespace)
        End Sub

        Public Sub New(ByVal FriendlyNamespace As String)
            m_FriendlyNamespace = FriendlyNamespace
			Logger.LogDebug("<<< Exiting New(FriendlyNamespace={FriendlyNamespace})", FriendlyNamespace)
			Logger.LogDebug("New(FriendlyNamespace={FriendlyNamespace}): m_FriendlyNamespace → {M_FriendlyNamespace}", FriendlyNamespace, m_FriendlyNamespace)
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

