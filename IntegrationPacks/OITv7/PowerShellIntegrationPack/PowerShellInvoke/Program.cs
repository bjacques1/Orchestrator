namespace PowerShellInvoke
{
    using System;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Globalization;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceProcess;
    using PowerShellIntegrationPack;

    /// <summary>
    ///     Windows Service for hosting the WCF service
    /// </summary>
    public class InvokeServer : ServiceBase
    {
        /// <summary>
        ///     URI of the WCF service
        /// </summary>
        private static Uri serviceUri;

        /// <summary>
        ///     WCF service host
        /// </summary>
        private static ServiceHost host = null;

        public InvokeServer()
            : base()
        {
            this.CanStop = true;
            this.CanPauseAndContinue = false;
            this.ServiceName = "PowerShell Invoke " + GenerateNormalizedName();
        }

        /// <summary>
        ///     Returns the normalized user name, e.g. "REDMOND-ZHYAO4"
        /// </summary>
        /// <returns>Normalized user name</returns>
        private static string GenerateNormalizedName()
        {
            return WindowsIdentity.GetCurrent().Name.Replace('\\', '-').ToUpperInvariant()
                + IntPtr.Size.ToString();
        }

        /// <summary>
        ///     Returns the TCP base address based on the user name being running the service
        /// </summary>
        /// <returns>TCP address as URL</returns>
        private static string GenerateTcpBaseAddress()
        {
            // The range of dynamic TCP port according to ICANN
            const int minPort = 49152;
            const int maxPort = 65535;

            int port = GenerateNormalizedName().GetHashCode();
            if (port < 0)
            {
                port = -port;
            }

            port = port % (maxPort - minPort - 1);
            port += minPort;

            return string.Format(CultureInfo.InvariantCulture, "net.tcp://localhost:{0}/PowerShellInvoke", port);
        }

        /// <summary>
        ///     Starts the WCF service with TCP binding using Windows authentication and transport security
        /// </summary>
        /// <returns>true if successfully started, false if otherwise</returns>
        protected override void OnStart(string[] args)
        {
            serviceUri = new Uri(GenerateTcpBaseAddress());

            var endPoint = GenerateNormalizedName();

            EventTracing.TraceInfo("Start WCF server at {0} with end point '{1}'", serviceUri.OriginalString, endPoint);

            try
            {
                host = new ServiceHost(new PowerShellInvoke(), serviceUri);

                var binding = new NetTcpBinding(SecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                binding.TransferMode = TransferMode.Streamed;

                binding.MaxBufferPoolSize = 1024 * 1024 * 128;
                binding.MaxBufferSize = 1024 * 1024 * 128;
                binding.MaxReceivedMessageSize = 1024 * 1024 * 128;
                binding.ReaderQuotas.MaxStringContentLength = 1024 * 1024 * 16;

                host.AddServiceEndpoint(typeof(IPowerShellInvoke), binding, endPoint);
                host.Open();

                EventTracing.TraceInfo("OK : service started");
            }
            catch (InvalidOperationException e)
            {
                EventTracing.TraceInfo("Exception: {0}", e);

                return;
            }
        }

        /// <summary>
        ///     Stops the running WCF service
        /// </summary>
        protected override void OnStop()
        {
            if (host != null && host.State != CommunicationState.Closed)
            {
                host.Close();
                host = null;

                EventTracing.TraceInfo("Service stopped");
            }
        }
    }

    /// <summary>
    ///     Main program for working with InstallUtil.exe
    /// </summary>
    [RunInstaller(true)]
    public class Program : Installer
    {
        public Program()
        {
            // Generate the service postfix based on the user name passed to InstallUtil.exe.  It will be the same as the
            // normalize name in InvokeServer, e.g. "REMOND-ZHYAO4".
            var cmdline = Environment.GetCommandLineArgs();
            string servicePostfix = string.Empty;

            foreach (var arg in cmdline)
            {
                if (arg.StartsWith("/username", true, CultureInfo.InvariantCulture))
                {
                    servicePostfix = arg.Split(new char[] { '=', ':' })[1];
                    servicePostfix = servicePostfix.Replace('\\', '-');
                    servicePostfix = servicePostfix.ToUpperInvariant() + IntPtr.Size.ToString();
                }
            }

            // If user name is not provided in the command line, don't install the service because we cannot set a unique
            // name for the Windows service.
            if (string.IsNullOrEmpty(servicePostfix))
            {
                Console.WriteLine("/username must be provided");
                return;
            }

            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            // Service will be started automatically with the credential of the service account, the user name and the password
            // have been provided to InstallUtil.exe already.
            processInstaller.Account = ServiceAccount.User;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = "PowerShell Invoke " + servicePostfix;
            serviceInstaller.Description = "PowerShell integration pack - Invoker service running as " + servicePostfix;

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }

        /// <summary>
        ///     Entry point for the Windows Service.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            ServiceBase.Run(new InvokeServer());
        }
    }
}
