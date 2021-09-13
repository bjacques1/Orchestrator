using System;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Get Test Runs", Description = "Get the list of test runs in a team project or all projects")]
    public class GetTestRuns : IActivity
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
            designer.AddInput(PublishedData.Query).NotRequired().WithDefaultValue("select * from TestRun");

            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();
            designer.AddOutput(PublishedData.TestRunId).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.TestPlanId).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.State).AsString().WithFilter();
            designer.AddOutput(PublishedData.Title).AsString().WithFilter();
            designer.AddOutput(PublishedData.Owner).AsString().WithFilter();
            designer.AddOutput(PublishedData.IsAutomated).AsString().WithFilter();
            designer.AddOutput(PublishedData.DateCompleted).AsDateTime().WithFilter();
            designer.AddOutput(PublishedData.DateStarted).AsDateTime().WithFilter();
            designer.AddOutput(PublishedData.BuildNumber).AsString().WithFilter();
            designer.AddOutput(PublishedData.BuildFlavor).AsString().WithFilter();
            designer.AddOutput(PublishedData.BuildPlatform).AsString().WithFilter();
            designer.AddOutput(PublishedData.Comment).AsString().WithFilter();
            designer.AddOutput(PublishedData.ErrorMessage).AsString().WithFilter();
            designer.AddOutput(PublishedData.Iteration).AsString().WithFilter();
            designer.AddOutput(PublishedData.LastUpdated).AsDateTime().WithFilter();
            designer.AddOutput(PublishedData.LastUpdatedBy).AsString().WithFilter();
            designer.AddOutput(PublishedData.TotalTests).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.CompletedTests).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.FailedTests).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.InconclusiveTests).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.InProgressTests).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.PassedTests).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.PendingTests).AsNumber().WithFilter();
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

            var runs = testManagementHelper.GetTestRuns(
                request.Inputs[PublishedData.Project].AsString(),
                queryText);

            response.Publish(PublishedData.NumberOfObjects, runs.Count);
            foreach (var run in runs)
            {
                response.WithFiltering().Publish(run);
            }
        }
    }
}
