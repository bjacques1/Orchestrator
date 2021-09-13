namespace PowerShellInvoke
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using PowerShellIntegrationPack;

    /// <summary>
    ///     Tuple for storing PowerShell runspace and logger
    /// </summary>
    internal class RunspacePair
    {
        public Runspace Space;
        public Logger Log;
    }

    /// <summary>
    ///     Factory class for managing runspaces and loggers
    /// </summary>
    internal static class MyRunspaceFactory
    {
        private const int DefaultWinRMPort = 5985;
        private static Dictionary<string, RunspacePair> s_runspaces = new Dictionary<string, RunspacePair>();

        /// <summary>
        ///     Returns the number of runspaces
        /// </summary>
        /// <returns>Number of the runspace being openned</returns>
        public static int NumberOfRunspaces()
        {
            return s_runspaces.Count;
        }

        /// <summary>
        ///     Retrieves a runspace for a given name.  Returns null if the runspace is not found.
        /// </summary>
        /// <param name="runspaceName">Runspace name, should not be null</param>
        /// <returns>runspace if found, otherwise null</returns>
        public static Runspace GetRunspace(
            string runspaceName)
        {
            if (s_runspaces.ContainsKey(runspaceName))
            {
                return s_runspaces[runspaceName].Space;
            }

            EventTracing.TraceEvent(
                TraceEventType.Error,
                0,  // TODO: define the ID for different events
                "Runspace '{0}' is not found",
                runspaceName);

            return null;
        }

        /// <summary>
        ///     Closes a given runspace.
        /// </summary>
        /// <param name="runspaceName">Name of the runspace, should not be null</param>
        /// <returns>If the runspace has been successfully closed</returns>
        public static bool CloseRunspace(
            string runspaceName)
        {
            if (s_runspaces.ContainsKey(runspaceName))
            {
                lock (s_runspaces)
                {
                    // Check again to confirm the runspace is not removed by another thread
                    if (s_runspaces.ContainsKey(runspaceName))
                    {
                        // Close the runspace and the logger
                        var pair = s_runspaces[runspaceName];
                        pair.Space.Close();
                        pair.Log.Close();

                        s_runspaces.Remove(runspaceName);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            EventTracing.TraceInfo("Runspace '{0}' closed", runspaceName);

            return true;
        }

        /// <summary>
        ///     Flushes the log for the given runspace
        /// </summary>
        /// <param name="runspaceName">Name of the runspace</param>
        /// <returns>true if the runspace log has been flushed successfully</returns>
        public static bool FlushRunspaceLog(
            string runspaceName)
        {
            if (s_runspaces.ContainsKey(runspaceName))
            {
                s_runspaces[runspaceName].Log.Flush();
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Opens a local or remote runspace
        /// </summary>
        /// <param name="runspaceName">Name of the runspace, should not be null</param>
        /// <param name="userName">User name, null if openning local runspace with the current credential</param>
        /// <param name="password">Password for the given user, can be null</param>
        /// <param name="domain">Domain of the user, can be null</param>
        /// <param name="hostName">Host name if openning a remote runspace, otherwise null</param>
        /// <param name="portNumber">Port number greater than 0, or use the default if less than zero</param>
        /// <param name="useSsl">Whether to use SSL transport</param>
        /// <param name="authentication">AuthenticationMechanism, null for "Default"</param>
        /// <param name="outFilename">File name of standard output log, null if the logging is not needed</param>
        /// <param name="errFilename">File name of standard error log, null if the logging is not needed</param>
        /// <returns>If the runspace has been openned successfully</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static bool OpenRunspace(
            string runspaceName,
            string userName,
            string password,
            string domain,
            string hostName,
            int portNumber,
            bool useSsl,
            string authentication,
            string outFilename,
            string errFilename)
        {
            // If the runspace already exists, just return true and do nothing.
            if (GetRunspace(runspaceName) != null)
            {
                EventTracing.TraceInfo("Runspace '{0}' exists", runspaceName);

                return true;
            }

            EventTracing.TraceInfo("OpenRunspace '{0}': {1}\\{2}:'{3}' ==> {4}:{5}, SSL={6} Auth={7}",
                runspaceName, domain, userName, password, hostName, portNumber, useSsl, authentication);

            lock (s_runspaces)
            {
                // Logger for the newly openned runspace
                var log = new Logger();
                log.Open(outFilename, errFilename);

                using (var securePassword = new System.Security.SecureString())
                {
                    if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(hostName))
                    {
                        // Local runspace with default credential (current user)

                        s_runspaces[runspaceName] = new RunspacePair
                        {
                            Space = RunspaceFactory.CreateRunspace(new PlainPSHost(log)),
                            Log = log,
                        };

                        EventTracing.TraceInfo("Local runspace");
                    }
                    else
                    {
                        WSManConnectionInfo conn;

                        // If host name is empty, assume remote to localhost with a different user
                        if (string.IsNullOrEmpty(hostName))
                        {
                            hostName = "localhost";
                        }

                        if (string.IsNullOrEmpty(userName))
                        {
                            // Current user, remote runspace

                            conn = new WSManConnectionInfo(
                                new Uri(string.Format(CultureInfo.InvariantCulture, "http://{0}/wsman", hostName)));

                            EventTracing.TraceInfo("Remote runspace to {0}", hostName);
                        }
                        else
                        {
                            // Another user, remote runspace
                            // Change the password to secure string
                            foreach (char c in password)
                            {
                                securePassword.AppendChar(c);
                            }

                            if (!string.IsNullOrEmpty(domain))
                            {
                                userName = domain + "\\" + userName;
                            }

                            if (portNumber > 0)
                            {
                                conn = new WSManConnectionInfo(
                                    useSsl,
                                    hostName,
                                    portNumber,
                                    "wsman",
                                    "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                                    new PSCredential(userName, securePassword));

                                EventTracing.TraceInfo("Remote runspace to {0} with port = ", hostName, portNumber);
                            }
                            else
                            {
                                conn = new WSManConnectionInfo(
                                    new Uri(string.Format(
                                        CultureInfo.InvariantCulture,
                                        "http://{0}:{1}/wsman",
                                        hostName,
                                        DefaultWinRMPort)),
                                        "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                                        new PSCredential(
                                            userName,
                                            securePassword));

                                EventTracing.TraceInfo(
                                    "Remote runspace to {0} with default port = ",
                                    hostName,
                                    DefaultWinRMPort);
                            }
                        }

                        if (string.IsNullOrEmpty(authentication))
                        {
                            authentication = "Default";
                        }

                        conn.AuthenticationMechanism = (AuthenticationMechanism)Enum.Parse(
                            typeof(AuthenticationMechanism),
                            authentication);

                        s_runspaces[runspaceName] = new RunspacePair
                        {
                            Space = RunspaceFactory.CreateRunspace(new PlainPSHost(log), conn),
                            Log = log,
                        };
                    }

                    try
                    {
                        s_runspaces[runspaceName].Space.Open();
                    }
                    catch (Exception e)
                    {
                        EventTracing.TraceEvent(TraceEventType.Error, 0, "Runspace.Open exception: {0}", e);

                        CloseRunspace(runspaceName);

                        return false;
                    }
                }

                EventTracing.TraceInfo("Runspace '{0}' opened", runspaceName);
            }

            return true;
        }
    }
}
