namespace PowerShellIntegrationPack
{
    internal abstract class PublishedData
    {
        public const string RunspaceName = "Runspace Name";
        public const string Script = "Script";
        public const string Is64bit = "64-bit Runspace (local)";
        public const string UserName = "User name";
        public const string Password = "Password";
        public const string Domain = "Domain";
        public const string HostName = "Host Name";
        public const string PortNumber = "Port Number";
        public const string UseSsl = "Connection Use SSL";
        public const string Authentication = "Authentication";
        public const string OutFileName = "Output log file";
        public const string ErrorFileName = "Error log file";

        public const string Variable = "Variable (e.g. $var)";

        // Output data
        public const string ExceptionMessage = "Exception.Message";
        public const string ExceptionStackTrace = "Exception.StackTrace";
        public const string ExceptionSource = "Exception.Source";

        public const string Succeeded = "Succeeded";
        public const string NumberOfObjects = "Number Of Objects";
        public const string PropertyName = "Property Name";
        public const string PropertyValue = "Property Value";
    }
}
