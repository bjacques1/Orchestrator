using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.SystemCenter.Orchestrator.Integration;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Add Test Case Result", Description = "Add a test case result to a test run")]
    public class AddTestCaseResult : IActivity
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

            designer.AddInput(PublishedData.TestRunId).WithDefaultValue(0);
            designer.AddInput(PublishedData.TestRunTitle).NotRequired();
            designer.AddInput(PublishedData.BuildNumber).NotRequired();
            designer.AddInput(PublishedData.TestCaseId);
            designer.AddInput(PublishedData.Owner);
            designer.AddInput(PublishedData.ErrorMessage).NotRequired();
            designer.AddInput(PublishedData.TestOutcome).WithEnumBrowser(typeof(TestOutcome));
            designer.AddInput(PublishedData.DateCompleted).WithDateTimeBrowser().NotRequired();
            designer.AddInput(PublishedData.DateStarted).WithDateTimeBrowser().NotRequired();
            designer.AddInput(PublishedData.Attachment).WithFileBrowser().NotRequired();
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

            int testRunId = request.Inputs[PublishedData.TestRunId].AsInt32();
            
            string testRunTitle = string.Empty;
            if (request.Inputs.Contains(PublishedData.TestRunTitle))
                testRunTitle = request.Inputs[PublishedData.TestRunTitle].AsString();

            string buildNumber = string.Empty;
            if (request.Inputs.Contains(PublishedData.BuildNumber))
                buildNumber = request.Inputs[PublishedData.BuildNumber].AsString();

            var owner = request.Inputs[PublishedData.Owner].AsString();

            string errorMessage = string.Empty;
            if (request.Inputs.Contains(PublishedData.ErrorMessage))
                errorMessage = request.Inputs[PublishedData.ErrorMessage].AsString();

            string attachment = string.Empty;
            if (request.Inputs.Contains(PublishedData.Attachment))
                attachment = request.Inputs[PublishedData.Attachment].AsString();

            DateTime? dateCompleted = null;
            if (request.Inputs.Contains(PublishedData.DateCompleted))
                dateCompleted = request.Inputs[PublishedData.DateCompleted].AsDateTime();

            DateTime? dateStarted = null;
            if (request.Inputs.Contains(PublishedData.DateStarted))
                dateStarted = request.Inputs[PublishedData.DateStarted].AsDateTime();

            if (testRunId <= 0 && string.IsNullOrEmpty(testRunTitle))
            {
                throw new ActivityExecutionException(AppResource.MissingTestRunIdOrTitle);
            }

            var run = testManagementHelper.GetTestRun(testRunId, testRunTitle, buildNumber);
            if (run == null)
            {
                throw new ActivityExecutionException(AppResource.NoTestRun);
            }

            testManagementHelper.AddTestCaseResult(
                run,
                request.Inputs[PublishedData.TestCaseId].AsInt32(),
                owner,
                errorMessage,
                request.Inputs[PublishedData.TestOutcome].As<TestOutcome>(),
                dateStarted,
                dateCompleted,
                attachment);

            run.Save();
        }
    }
}
