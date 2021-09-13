using System;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Get Test Suites", Description = "Get the test suites in a team project")]
    public class GetTestSuites : IActivity
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
            designer.AddInput(PublishedData.Query).NotRequired().WithDefaultValue("select * from TestSuite");

            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();
            designer.AddOutput(PublishedData.TestSuiteId).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.Description).AsString().WithFilter();
            designer.AddOutput(PublishedData.TestSuiteTitle).AsString().WithFilter();
            designer.AddOutput(PublishedData.TestSuiteState).AsString().WithFilter();
            designer.AddOutput(PublishedData.TestPlanId).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.TestPlanName).AsString().WithFilter();
            designer.AddOutput(PublishedData.ParentTestSuiteId).AsNumber().WithFilter();
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

            var suites = testManagementHelper.GetTestSuites(
                request.Inputs[PublishedData.Project].AsString(),
                queryText);

            response.Publish(PublishedData.NumberOfObjects, suites.Count);
            foreach (var suite in suites)
            {
                response.WithFiltering().Publish(suite);
            }
        }
    }
}
