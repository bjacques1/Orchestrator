using System;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Get Test Plans", Description = "Get the list of test plans in a team project")]
    public class GetTestPlans : IActivity
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
            designer.AddInput(PublishedData.Query).NotRequired().WithDefaultValue("select * from TestPlan");

            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();
            designer.AddOutput(PublishedData.TestPlanId).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.TestPlanName).AsString().WithFilter();
            designer.AddOutput(PublishedData.Description).AsString().WithFilter();
            designer.AddOutput(PublishedData.Owner).AsString().WithFilter();
            designer.AddOutput(PublishedData.AreaPath).AsString().WithFilter();
            designer.AddOutput(PublishedData.State).AsString().WithFilter();
            designer.AddOutput(PublishedData.StartDate).AsDateTime().WithFilter();
            designer.AddOutput(PublishedData.EndDate).AsDateTime().WithFilter();
            designer.AddOutput(PublishedData.Iteration).AsString().WithFilter();
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

            var plans = testManagementHelper.GetTestPlans(
                request.Inputs[PublishedData.Project].AsString(),
                queryText);

            response.Publish(PublishedData.NumberOfObjects, plans.Count);
            foreach (var plan in plans)
            {
                response.WithFiltering().Publish(plan);
            }
        }
    }
}
