namespace PowerShellIntegrationPack
{
    using System;
    using Microsoft.SystemCenter.Orchestrator.Integration;

    [Activity("Flush Runspace Log", Description = "Flush the output and error logs of the PowerShell runspace by the name")]
    public class FlushRunspaceLog : IActivity
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

            designer.AddInput(PublishedData.RunspaceName).WithDefaultValue("Default");
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

            var runspaceName = request.Inputs[PublishedData.RunspaceName].AsString();

            var ret = PowerShellClient.FlushRunspaceLog(
                new System.Net.NetworkCredential(
                    Settings.UserName,
                    Settings.Password,
                    Settings.Domain),
                runspaceName);

            response.Publish(PublishedData.Succeeded, ret.ToString());
            response.Publish(PublishedData.RunspaceName, runspaceName);

            if (!ret)
            {
                throw new ActivityWarning("Runspace is not found");
            }
        }
    }
}
