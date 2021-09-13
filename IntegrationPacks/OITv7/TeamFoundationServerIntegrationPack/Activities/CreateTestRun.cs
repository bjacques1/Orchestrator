using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Create Test Run", Description = "Create a new test run in a team project")]
    public class CreateTestRun : IActivity
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
            designer.AddInput(PublishedData.TestPlanId);
            designer.AddInput(PublishedData.TestPlanName);
            designer.AddInput(PublishedData.IsAutomated).WithBooleanBrowser();
            designer.AddInput(PublishedData.TestRunTitle);
            designer.AddInput(PublishedData.BuildNumber);
            designer.AddInput(PublishedData.ListOfTestSuites);
            designer.AddInput(PublishedData.ListOfTestCases);
            designer.AddInput(PublishedData.Owner);
            designer.AddInput(PublishedData.BuildDirectory).NotRequired();
            designer.AddInput(PublishedData.BuildFlavor).NotRequired();
            designer.AddInput(PublishedData.BuildPlatform).NotRequired();
            designer.AddInput(PublishedData.DateCompleted).WithDateTimeBrowser().NotRequired();
            designer.AddInput(PublishedData.DateStarted).WithDateTimeBrowser().NotRequired();

            designer.AddOutput(PublishedData.TestRunId).AsNumber();
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

            string buildDirectory = string.Empty;
            if (request.Inputs.Contains(PublishedData.BuildDirectory))
                buildDirectory = request.Inputs[PublishedData.BuildDirectory].AsString();

            string buildFlavor = string.Empty;
            if (request.Inputs.Contains(PublishedData.BuildFlavor))
                buildFlavor = request.Inputs[PublishedData.BuildFlavor].AsString();

            string buildPlatform = string.Empty;
            if (request.Inputs.Contains(PublishedData.BuildPlatform))
                buildPlatform = request.Inputs[PublishedData.BuildPlatform].AsString();
            
            string owner = string.Empty;
            if (request.Inputs.Contains(PublishedData.Owner))
                owner = request.Inputs[PublishedData.Owner].AsString();

            DateTime? dateCompleted = null;
            if (request.Inputs.Contains(PublishedData.DateCompleted))
                dateCompleted = request.Inputs[PublishedData.DateCompleted].AsDateTime();

            DateTime? dateStarted = null;
            if (request.Inputs.Contains(PublishedData.DateStarted))
                dateStarted = request.Inputs[PublishedData.DateStarted].AsDateTime();

            var listOfSuites = ConvertNumberListToArray(request.Inputs[PublishedData.ListOfTestSuites].AsString());
            var listOfCases = ConvertNumberListToArray(request.Inputs[PublishedData.ListOfTestCases].AsString());

            if (listOfCases.Count() == 0 && listOfSuites.Count() == 0)
                throw new ActivityExecutionException(AppResource.NoTestCaseOrSuite);

            var testManagementHelper = new TestManagementHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);

            var run = testManagementHelper.CreateTestRun(
                request.Inputs[PublishedData.Project].AsString(),
                request.Inputs[PublishedData.TestPlanId].AsInt32(),
                request.Inputs[PublishedData.TestPlanName].AsString(),
                request.Inputs[PublishedData.IsAutomated].AsBoolean(),
                request.Inputs[PublishedData.TestRunTitle].AsString(),
                buildDirectory,
                buildFlavor,
                request.Inputs[PublishedData.BuildNumber].AsString(),
                buildPlatform,
                dateStarted,
                dateCompleted,
                owner,
                listOfSuites,
                listOfCases);

            response.Publish(PublishedData.TestRunId, run.Id);
        }

        private static IEnumerable<int> ConvertNumberListToArray(string numbers)
        {
            var separatedNumbers = numbers.Split(new char[] { ',', ';' });
            var convertedNumbers = from numStr in separatedNumbers
                                   where !string.IsNullOrEmpty(numStr.Trim())
                                   select int.Parse(numStr.Trim(), CultureInfo.InvariantCulture);
            return convertedNumbers.AsEnumerable();
        }
    }
}
