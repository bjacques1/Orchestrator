namespace PowerShellIntegrationPack
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using PowerShellInvoke;

    internal class PowerShellClient
    {
        /// <summary>
        ///     Mapping between user account and 32-bit service proxy
        /// </summary>
        private static Dictionary<string,IPowerShellInvoke> serviceProxy32 = new Dictionary<string,IPowerShellInvoke>();

        /// <summary>
        ///     Mapping between user account and 64-bit service proxy
        /// </summary>
        private static Dictionary<string, IPowerShellInvoke> serviceProxy64 = new Dictionary<string, IPowerShellInvoke>();

        /// <summary>
        ///     True for 64bit, False for 32-bit.  Key is the runspace name with prefix of user account name.
        /// </summary>
        private static Dictionary<string, bool> runspaceIs64bit = new Dictionary<string,bool>();

        /// <summary>
        ///     Returns the normalized user name of the given credential, e.g. "REDMOND-ZHYAO".  If the given credential is empty, return the current credential.
        /// </summary>
        /// <param name="credential">Network credential</param>
        /// <returns>Normalized user name</returns>
        private static string UserName(
            NetworkCredential credential)
        {
            string name;

            if (credential == null || string.IsNullOrEmpty(credential.UserName))
                name = WindowsIdentity.GetCurrent().Name.Replace('\\', '-');
            else
                name = credential.Domain + "-" + credential.UserName;

            return name.ToUpperInvariant();
        }

        /// <summary>
        ///     Generates the TCP end point URL for the WCF service
        /// </summary>
        /// <param name="credential">Credential for connecting the WCF service, null for current user</param>
        /// <param name="is64bit">Whether connecting to 64-bit service (if true) or 32-bit service (if false)</param>
        /// <returns>End point URL for WCF TCP binding</returns>
        private static string GenerateTcpEndpoint(
            NetworkCredential credential,
            bool is64bit)
        {
            // Range of dynamic port for TCP
            const int minPort = 49152;
            const int maxPort = 65535;

            var endPoint = UserName(credential) + (is64bit? 8 : 4).ToString();
            int port = endPoint.GetHashCode();
            if (port < 0)
            {
                port = -port;
            }

            port = port % (maxPort - minPort - 1);
            port += minPort;

            return string.Format(CultureInfo.InvariantCulture, "net.tcp://localhost:{0}/PowerShellInvoke/{1}", port, endPoint);
        }

        /// <summary>
        ///     Intializes the service proxies, and starts the process using the current identity if it's not started already.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static void Initialize(
            NetworkCredential credential)
        {
            // Do not initialize twice.
            if (serviceProxy32.ContainsKey(UserName(credential)))
            {
                return;
            }

            // WCF TCP binding with Windows authentication and transport security
            var binding = new NetTcpBinding(SecurityMode.Transport);
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxBufferPoolSize = 1024 * 1024 * 128;
            binding.MaxBufferSize = 1024 * 1024 * 128;
            binding.MaxReceivedMessageSize = 1024 * 1024 * 128;
            binding.ReaderQuotas.MaxStringContentLength = 1024 * 1024 * 16;

            var serviceAddress = new EndpointAddress(GenerateTcpEndpoint(credential, false));

            serviceProxy32[UserName(credential)] = ChannelFactory<IPowerShellInvoke>.CreateChannel(
                binding,
                serviceAddress);

            // Check if the current machine is amd64 version
            var wow64 = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432");
            var proc = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            if (wow64 == "AMD64" || proc == "AMD64")
            {
                // This process is either Wow64 or native amd64.

                serviceAddress = new EndpointAddress(GenerateTcpEndpoint(credential, true));
                serviceProxy64[UserName(credential)] = ChannelFactory<IPowerShellInvoke>.CreateChannel(
                    binding,
                    serviceAddress);
            }
            else
            {
                // Only 32-bit

                serviceProxy64 = null;
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static bool OpenRunspace(
            NetworkCredential credential,
            bool is64bit,
            string runspaceName,
            string userName,
            string password,
            string domain,
            string hostName,
            int portNumber,
            bool useSsl,
            string authentication,
            string outFileName,
            string errorFileName)
        {
            bool ret;

            Initialize(credential);

            using (new Impersonation(credential.Domain, credential.UserName, credential.Password))
            {
                if (is64bit && serviceProxy64 != null)
                {
                    ret = serviceProxy64[UserName(credential)].OpenRunspace(
                        runspaceName, userName, password, domain, hostName, portNumber, useSsl, authentication, outFileName, errorFileName);

                    // Remember this runspace with this credential is 64-bit
                    if (ret)
                    {
                        runspaceIs64bit[UserName(credential) + runspaceName] = true;
                    }
                }
                else if (!is64bit && serviceProxy32 != null)
                {
                    ret = serviceProxy32[UserName(credential)].OpenRunspace(
                        runspaceName, userName, password, domain, hostName, portNumber, useSsl, authentication, outFileName, errorFileName);

                    // Remember this runspace with this credential is 32-bit
                    if (ret)
                    {
                        runspaceIs64bit[UserName(credential) + runspaceName] = false;
                    }
                }
                else
                {
                    ret = false;
                }
            }

            return ret;
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static bool CloseRunspace(
            NetworkCredential credential,
            string runspaceName)
        {
            bool ret = false;

            // Check if the runspace has been openned within this process or not
            if (!runspaceIs64bit.ContainsKey(UserName(credential) + runspaceName))
            {
                EventTracing.TraceInfo("Runspace '{0}' with {1} is not found", runspaceName, UserName(credential));

                return false;
            }

            Initialize(credential);

            using (new Impersonation(credential.Domain, credential.UserName, credential.Password))
            {
                if (runspaceIs64bit[UserName(credential) + runspaceName])
                {
                    if (serviceProxy64 != null)
                    {
                        ret = serviceProxy64[UserName(credential)].CloseRunspace(runspaceName);
                    }
                }
                else
                {
                    if (serviceProxy32 != null)
                    {
                        ret = serviceProxy32[UserName(credential)].CloseRunspace(runspaceName);
                    }
                }

                // If the runspace has been closed, cleanup the entry in runspaceIs64bit
                runspaceIs64bit.Remove(UserName(credential) + runspaceName);

                EventTracing.TraceInfo("Runspace '{0}' with {1} has been closed", runspaceName, UserName(credential));
            }

            return ret;
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static bool FlushRunspaceLog(
            NetworkCredential credential,
            string runspaceName)
        {
            // Check if the runspace has been openned within this process or not
            if (!runspaceIs64bit.ContainsKey(UserName(credential) + runspaceName))
            {
                EventTracing.TraceInfo("Runspace '{0}' with {1} is not found", runspaceName, UserName(credential));

                return false;
            }

            Initialize(credential);

            using (new Impersonation(credential.Domain, credential.UserName, credential.Password))
            {
                if (runspaceIs64bit[UserName(credential) + runspaceName])
                {
                    if (serviceProxy64 != null)
                    {
                        return serviceProxy64[UserName(credential)].FlushRunspaceLog(runspaceName);
                    }
                }
                else
                {
                    if (serviceProxy32 != null)
                    {
                        return serviceProxy32[UserName(credential)].FlushRunspaceLog(runspaceName);
                    }
                }
            }

            return false;
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        public static List<Dictionary<string, string>> RunScript(
            NetworkCredential credential,
            string runspaceName,
            List<string> scripts)
        {
            if (!runspaceIs64bit.ContainsKey(UserName(credential) + runspaceName))
            {
                var dict = new Dictionary<string, string>();
                dict[PublishedData.ExceptionMessage] = string.Format(CultureInfo.InvariantCulture, "Runspace '{0}' not found", runspaceName);
                var results = new List<Dictionary<string, string>>();
                results.Add(dict);

                EventTracing.TraceInfo("Runspace '{0}' with {1} is not found", runspaceName, UserName(credential));

                return results;
            }

            Initialize(credential);

            using (new Impersonation(credential.Domain, credential.UserName, credential.Password))
            {
                if (runspaceIs64bit[UserName(credential) + runspaceName])
                {
                    if (serviceProxy64 != null)
                        return serviceProxy64[UserName(credential)].RunScript(runspaceName, scripts);
                }
                else
                {
                    if (serviceProxy32 != null)
                        return serviceProxy32[UserName(credential)].RunScript(runspaceName, scripts);
                }
            }

            return null;
        }
    }
}

