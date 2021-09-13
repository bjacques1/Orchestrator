namespace PowerShellIntegrationPack
{
    using Microsoft.SystemCenter.Orchestrator.Integration;

    /// <summary>
    ///     Connection setting of PowerShell integration pack
    /// </summary>
    [ActivityData("PowerShell Settings")]
    public class PowerShellSettings
    {
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

