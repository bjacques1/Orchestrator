namespace PowerShellIntegrationPack
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.SystemCenter.Orchestrator.Integration;

    [Activity("Set Credential", Description = "Create a PSCredential object and assign to a variable")]
    public class SetCredential: IActivity
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
            designer.AddInput(PublishedData.UserName);
            designer.AddInput(PublishedData.Password).PasswordProtect();
            designer.AddInput(PublishedData.Variable);

            designer.AddOutput(PublishedData.RunspaceName).AsString();
            designer.AddOutput(PublishedData.Variable).AsString();
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
            var userName = request.Inputs[PublishedData.UserName].AsString();
            var password = request.Inputs[PublishedData.Password].AsString();
            var variable = request.Inputs[PublishedData.Variable].AsString();

            var scripts = new List<string>();
            scripts.Add(
                string.Format(
                CultureInfo.InvariantCulture,
                "{0}-pw = ConvertTo-SecureString -string \"{1}\" -asPlainText -force",
                variable,
                password));
            scripts.Add(
                string.Format(
                CultureInfo.InvariantCulture,
                "{0} = New-Object System.Management.Automation.PsCredential(\"{1}\" , {0}-pw)",
                variable,
                userName));

            var ret = PowerShellClient.RunScript(
                new System.Net.NetworkCredential(
                    Settings.UserName,
                    Settings.Password,
                    Settings.Domain),
                runspaceName,
                scripts);

            if (ret == null || ret.Count < 1)
            {
                response.Publish(PublishedData.Succeeded, false.ToString());
                throw new ApplicationException();
            }
            else if (ret.Count == 1 && ret[0].ContainsKey(PublishedData.ExceptionMessage))
            {
                response.Publish(PublishedData.Succeeded, false.ToString());
                response.Publish(ret[0]);
                throw new ActivityWarning(ret[0][PublishedData.ExceptionMessage]);
            }
            else
            {
                response.Publish(PublishedData.Succeeded, true.ToString());
                response.Publish(PublishedData.RunspaceName, runspaceName);
                response.Publish(PublishedData.Variable, variable);
            }
        }
    }
}

