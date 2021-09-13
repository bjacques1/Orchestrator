// <copyright file="QuickRunScript.cs" company="Microsoft">
//      Copyright (C) 2012  Microsoft.
// </copyright>

namespace PowerShellIntegrationPack
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation.Runspaces;
    using Microsoft.SystemCenter.Orchestrator.Integration;

    /// <summary>
    ///     Orchestrator activity for running PowerShell scripts in 32-bit bit without impersonation
    /// </summary>
    [Activity("Quick Run Script", Description = "Run 32-bit PowerShell script without impersonation")]
    public class QuickRunScript : IActivity
    {
        public void Design(
            IActivityDesigner designer)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }

            designer.AddInput(PublishedData.Script);
            designer.AddInput(PublishedData.OutFileName).WithFileBrowser();
            designer.AddInput(PublishedData.ErrorFileName).WithFileBrowser();
            designer.AddInput(PublishedData.UserName).NotRequired();
            designer.AddInput(PublishedData.Password).NotRequired().PasswordProtect();
            designer.AddInput(PublishedData.Domain).NotRequired();
            designer.AddInput(PublishedData.HostName).NotRequired().WithComputerBrowser();
            designer.AddInput(PublishedData.PortNumber).NotRequired().WithDefaultValue(0);
            designer.AddInput(PublishedData.UseSsl).NotRequired().WithBooleanBrowser().WithDefaultValue(false);
            designer.AddInput(PublishedData.Authentication).NotRequired().WithEnumBrowser(typeof(AuthenticationMechanism));

            designer.AddOutput(PublishedData.Script).AsString();
            designer.AddOutput(PublishedData.Succeeded).AsString();
            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();
            designer.AddCorellatedData(typeof(PsObjectProperty));
            designer.AddOutput(PublishedData.ExceptionMessage).AsString();
            designer.AddOutput(PublishedData.ExceptionStackTrace).AsString();
            designer.AddOutput(PublishedData.ExceptionSource).AsString();
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

            var script = request.Inputs[PublishedData.Script].AsString();
            var outFileName = request.Inputs[PublishedData.OutFileName].AsString();
            var errorFileName = request.Inputs[PublishedData.ErrorFileName].AsString();
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

            response.Publish(PublishedData.Script, script);

            var scripts = new List<string>();
            scripts.Add(script);

            var invoker = new PowerShellInvoke.PowerShellInvoke();
            var runspaceName = "Default";

            var openned = invoker.OpenRunspace(
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

            if (!openned)
            {
                response.Publish(PublishedData.Succeeded, false.ToString());
                throw new ActivityWarning("Failed to open the runspace");
            }

            try
            {
                var ret = invoker.RunScript(
                    runspaceName,
                    scripts);

                if (ret == null)
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
                    response.Publish(PublishedData.NumberOfObjects, ret.Count);
                    foreach (var dict in ret)
                    {
                        response.WithFiltering().PublishRange(this.GetPropertiesOfPsObject(dict));
                    }
                }
            }
            finally
            {
                invoker.CloseRunspace(runspaceName);
            }
        }

        private IEnumerable<PsObjectProperty> GetPropertiesOfPsObject(Dictionary<string, string> dict)
        {
            foreach (var kvp in dict.OrderBy(e => e.Key))
            {
                var prop = new PsObjectProperty();
                prop.PropertyName = kvp.Key;
                prop.PropertyValue = kvp.Value;

                yield return prop;
            }
        }
    }
}
