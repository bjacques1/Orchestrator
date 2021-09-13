namespace PowerShellInvoke
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.ServiceModel;
    using PowerShellIntegrationPack;
    using System.Management.Automation.Runspaces;

    /// <summary>
    ///     Implementation of PowerShellInvoke WCF service for running PowerShell scripts locally or remotely in either 32-bit
    ///     or 64-bit process.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PowerShellInvoke : IPowerShellInvoke
    {
        public bool OpenRunspace(
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
            return MyRunspaceFactory.OpenRunspace(
                runspaceName,
                userName,
                password,
                domain,
                hostName,
                portNumber,
                useSsl,
                authentication,
                outFileName,
                errorFileName);
        }

        public bool FlushRunspaceLog(
            string runspaceName)
        {
            return MyRunspaceFactory.FlushRunspaceLog(runspaceName);
        }

        public bool CloseRunspace(
            string runspaceName)
        {
            return MyRunspaceFactory.CloseRunspace(runspaceName);
        }

        /// <summary>
        ///     Runs a list of PowerShell scripts and returns a list of PSObject that will be converted to Dictionary.
        /// </summary>
        /// <param name="runspaceName">Name of the runspace, should not be null</param>
        /// <param name="scripts">List of PowerShell scripts to run</param>
        /// <returns>List of PSObject converted to Dictionary</returns>
        public List<Dictionary<string, string>> RunScript(
            string runspaceName,
            List<string> scripts)
        {
            EventTracing.TraceInfo("RunScript: {0} - {1}", runspaceName, scripts[0]);

            if (string.IsNullOrEmpty(runspaceName) || scripts == null || scripts.Count == 0)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Error,
                    0,      // TODO: define an event ID for error
                    "ERROR: No script to run");
                return null;
            }

            var runspace = MyRunspaceFactory.GetRunspace(runspaceName);

            // Push scripts into the pipeline in order
            var pipeline = runspace.CreatePipeline();
            scripts.ForEach(
                line => pipeline.Commands.AddScript(line));

            var results = new List<Dictionary<string, string>>();

            Collection<PSObject> psobjs;

            // Run the scripts
            try
            {
                psobjs = pipeline.Invoke();
            }
            catch (RuntimeException e)
            {
                // On exception, return the detail of the exception
                var result = new Dictionary<string, string>();
                result["Exception.Message"] = e.Message;
                result["Exception.StackTrace"] = e.StackTrace;
                result["Exception.Source"] = e.Source;

                results.Add(result);

                EventTracing.TraceInfo("ERROR: {0}", e);

                return results;
            }
            catch (InvalidRunspaceStateException e)
            {
                // On exception, return the detail of the exception
                var result = new Dictionary<string, string>();
                result["Exception.Message"] = e.Message;
                result["Exception.StackTrace"] = e.StackTrace;
                result["Exception.Source"] = e.Source;

                results.Add(result);

                EventTracing.TraceInfo("ERROR: {0}", e);

                return results;
            }

            // Convert the list of PSObject to the list of Dictionary
            foreach (PSObject psobj in psobjs)
            {
                var result = new Dictionary<string, string>();

                result[".ToString"] = psobj.ToString();

                foreach (PSPropertyInfo prop in psobj.Properties)
                {
                    try
                    {
                        result[prop.Name] = prop.Value == null? "" : prop.Value.ToString();
                        
                        EventTracing.TraceEvent(TraceEventType.Verbose, 0, "{0,20} : {1}", prop.Name, prop.Value);
                    }
                    catch (GetValueInvocationException)
                    {
                        // If any property cannot be retrieved, just ignore it.
                    }
                }

                results.Add(result);
            }

            return results;
        }
    }
}
