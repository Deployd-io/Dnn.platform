Imports System

private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
Namespace DotNetNuke.UI.Utilities
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)> _
    Public NotInheritable Class ControlMethodClassAttribute : Inherits Attribute

        ' Fields
        Private m_FriendlyNamespace As String = ""

        ' Methods
        Public Sub New()
		Logger.LogDebug(">>> Entering New()")
        End Sub

        Public Sub New(ByVal FriendlyNamespace As String)
            m_FriendlyNamespace = FriendlyNamespace
        End Sub
		Logger.LogDebug("<<< Exiting New(FriendlyNamespace={FriendlyNamespace})", FriendlyNamespace)
		Logger.LogDebug("New(FriendlyNamespace={FriendlyNamespace}): m_FriendlyNamespace → {M_FriendlyNamespace}", FriendlyNamespace, m_FriendlyNamespace)

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

