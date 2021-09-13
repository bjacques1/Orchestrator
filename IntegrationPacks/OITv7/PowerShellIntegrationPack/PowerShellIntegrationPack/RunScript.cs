namespace PowerShellIntegrationPack
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.SystemCenter.Orchestrator.Integration;

    /// <summary>
    ///     Published data for PSObject
    /// </summary>
    [ActivityData]
    internal class PsObjectProperty
    {
        /// <summary>
        ///     Name of PSObject property
        /// </summary>
        [ActivityOutput(PublishedData.PropertyName, "Name of PsObject property")]
        [ActivityFilter(PublishedData.PropertyName)]
        public string PropertyName { get; set; }

        /// <summary>
        ///     Value of PSObject property
        /// </summary>
        [ActivityOutput(PublishedData.PropertyValue, "Value of PsObject property")]
        [ActivityFilter(PublishedData.PropertyValue)]
        public string PropertyValue { get; set; }
    }

    /// <summary>
    ///     Orchestrator activity for running PowerShell scripts
    /// </summary>
    [Activity("Run Script", Description="Run PowerShell script")]
    public class RunScript : IActivity
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
            designer.AddInput(PublishedData.Script);

            designer.AddOutput(PublishedData.RunspaceName).AsString();
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

            var runspaceName = request.Inputs[PublishedData.RunspaceName].AsString();
            var script = request.Inputs[PublishedData.Script].AsString();

            response.Publish(PublishedData.RunspaceName, runspaceName);
            response.Publish(PublishedData.Script, script);

            var scripts = new List<string>();
            scripts.Add(script);

            var ret = PowerShellClient.RunScript(
                new System.Net.NetworkCredential(
                    Settings.UserName,
                    Settings.Password,
                    Settings.Domain),
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
                    response.WithFiltering().PublishRange(GetPropertiesOfPsObject(dict));
                }
            }
        }

        private IEnumerable<PsObjectProperty> GetPropertiesOfPsObject(Dictionary<string,string> dict)
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

