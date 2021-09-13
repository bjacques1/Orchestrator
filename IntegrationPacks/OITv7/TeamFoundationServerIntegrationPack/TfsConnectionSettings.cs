using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    /// <summary>
    ///     URL of the TFS server, e.g. "http://scxtfs2:8080/tfs"
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tfs"), ActivityData("TFS Connection Settings")]
    public class TfsConnectionSettings
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), ActivityInput("Team Foundation Server URL")]
        public string Url
        {
            set;
            get;
        }

        [ActivityInput("Logon domain (RunAs)", Optional = true)]
        public string Domain
        {
            set;
            get;
        }

        [ActivityInput("User name (RunAs)", Optional = true)]
        public string UserName
        {
            set;
            get;
        }

        [ActivityInput("Password (RunAs)", Optional = true, PasswordProtected = true)]
        public string Password
        {
            set;
            get;
        }
    }
}

