namespace PowerShellInvoke
{
    using System.Collections.Generic;
    using System.ServiceModel;

    /// <summary>
    ///     WCF service interface for hosting PowerShell script execution
    /// </summary>
    [ServiceContract]
    public interface IPowerShellInvoke
    {
        [OperationContract]
        bool OpenRunspace(
            string runspaceName,
            string userName,
            string password,
            string domain,
            string hostName,
            int portNumber,
            bool useSsl,
            string authentication,
            string outFileName,
            string errorFileName);

        [OperationContract]
        bool CloseRunspace(
            string runspaceName);

        [OperationContract]
        bool FlushRunspaceLog(
            string runspaceName);

        [OperationContract]
        List<Dictionary<string, string>> RunScript(
            string runspaceName,
            List<string> scripts);
    }
}
