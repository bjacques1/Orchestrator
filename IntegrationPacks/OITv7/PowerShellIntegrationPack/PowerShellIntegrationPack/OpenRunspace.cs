namespace PowerShellIntegrationPack
{
    using System;
    using System.Management.Automation.Runspaces;
    using Microsoft.SystemCenter.Orchestrator.Integration;

    [Activity("Open Runspace", Description = "Open a PowerShell runspace with the specified name")]
    public class OpenRunspace : IActivity
    {
        [ActivityConfiguration]
        public PowerShellSettings Settings
        {
            set;
            get;
        }

        public void Design(
            IActivityDesigner designer)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }

            designer.AddInput(PublishedData.Is64bit).WithBooleanBrowser().WithDefaultValue(false);
            designer.AddInput(PublishedData.RunspaceName).WithDefaultValue("Default");
            designer.AddInput(PublishedData.OutFileName).WithFileBrowser();
            designer.AddInput(PublishedData.ErrorFileName).WithFileBrowser();
            designer.AddInput(PublishedData.UserName).NotRequired();
            designer.AddInput(PublishedData.Password).NotRequired().PasswordProtect();
            designer.AddInput(PublishedData.Domain).NotRequired();
            designer.AddInput(PublishedData.HostName).NotRequired().WithComputerBrowser();
            designer.AddInput(PublishedData.PortNumber).NotRequired().WithDefaultValue(0);
            designer.AddInput(PublishedData.UseSsl).NotRequired().WithBooleanBrowser().WithDefaultValue(false);
            designer.AddInput(PublishedData.Authentication).NotRequired().WithEnumBrowser(typeof(AuthenticationMechanism));

            designer.AddOutput(PublishedData.RunspaceName).AsString();
            designer.AddOutput(PublishedData.Succeeded).AsString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public void Execute(
            IActivityRequest request,
            IActivityResponse response)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            var is64bit = request.Inputs[PublishedData.Is64bit].AsBoolean();
            var runspaceName = request.Inputs[PublishedData.RunspaceName].AsString();
            var userName = request.Inputs[PublishedData.UserName].AsString();
            var password = request.Inputs[PublishedData.Password].AsString();
            var domain = request.Inputs[PublishedData.Domain].AsString();
            var hostName = request.Inputs[PublishedData.HostName].AsString();
            
            int portNumber = 0;
            if (request.Inputs.Contains(PublishedData.PortNumber))
            {
                portNumber = request.Inputs[PublishedData.PortNumber].AsInt32();
            }

            bool useSsl = false;
            if (request.Inputs.Contains(PublishedData.UseSsl))
            {
                useSsl = request.Inputs[PublishedData.UseSsl].AsBoolean();
            }

            var authentication = request.Inputs[PublishedData.Authentication].AsString();

            // if not in Wow64 or connect to remote, ignore is64bit
            if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432") != "AMD64" ||
                !string.IsNullOrEmpty(hostName))
            {
                is64bit = false;
            }

            var ret = PowerShellClient.OpenRunspace(
                new System.Net.NetworkCredential(
                    Settings.UserName,
                    Settings.Password,
                    Settings.Domain),
                is64bit,
                runspaceName,
                userName,
                password,
                domain,
                hostName,
                portNumber,
                useSsl,
                authentication,
                request.Inputs[PublishedData.OutFileName].AsString(),
                request.Inputs[PublishedData.ErrorFileName].AsString());

            response.Publish(PublishedData.Succeeded, ret.ToString());
            response.Publish(PublishedData.RunspaceName, runspaceName);

            if (!ret)
            {
                throw new ActivityWarning("Runspace not openned successfully");
            }
        }
    }
}

