using System;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Get Test Configurations", Description = "Get the list of test configurations in a team project")]
    public class GetTestConfigurations : IActivity
    {
        [ActivityConfiguration]
        public TfsConnectionSettings ConnectionSettings
        {
            get;
            set;
        }

        public void Design(IActivityDesigner designer)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }

            designer.AddInput(PublishedData.Project);
            designer.AddInput(PublishedData.Query).NotRequired().WithDefaultValue("select * from TestConfiguration");

            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();
            designer.AddOutput(PublishedData.Id).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.AreaPath).AsString().WithFilter();
            designer.AddOutput(PublishedData.State).AsString().WithFilter();
            designer.AddOutput(PublishedData.Description).AsString().WithFilter();
            designer.AddOutput(PublishedData.TestConfigurationName).AsString().WithFilter();
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            var testManagementHelper = new TestManagementHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);

            string queryText = string.Empty;
            if (request.Inputs.Contains(PublishedData.Query))
            {
                queryText = request.Inputs[PublishedData.Query].AsString();
            }

            var configs = testManagementHelper.GetTestConfigurations(
                request.Inputs[PublishedData.Project].AsString(),
                queryText);

            response.Publish(PublishedData.NumberOfObjects, configs.Count);
            foreach (var config in configs)
            {
                response.WithFiltering().Publish(config);
            }
        }
    }
}
