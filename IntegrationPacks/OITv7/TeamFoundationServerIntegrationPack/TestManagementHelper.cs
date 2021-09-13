using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace TeamFoundationServerIntegrationPack
{
    // Reference:
    //      WIQL for Test: http://blogs.msdn.com/b/duat_le/archive/2010/02/25/wiql-for-test.aspx
    //      Microsoft.TeamFoundation.TestManagement.Client Namespace: http://msdn.microsoft.com/en-us/library/dd998375.aspx

    class TestManagementHelper
    {
        private TestManagementService testManagementService;
        private IIdentityManagementService idService;

        public TestManagementHelper(
            string url,
            string domain,
            string userName,
            string password)
        {
            var tfs = TfsConnectionFactory.GetTeamProjectCollection(
                url, domain, userName, password);
            testManagementService = tfs.GetService<TestManagementService>();
            idService = tfs.GetService<IIdentityManagementService>();

            EventTracing.TraceInfo("Connect to '{0}' for test management.", url);
        }

        /// <summary>
        ///     Retrieves the list of test plans in a team project
        /// </summary>
        /// <param name="teamProjectName">Name of the team project</param>
        /// <param name="queryText">WIQL, retrieves all plans if not present</param>
        /// <returns></returns>
        public List<Dictionary<string,object>> GetTestPlans(
            string teamProjectName,
            string queryText)
        {
            var project = testManagementService.GetTeamProject(teamProjectName);

            if (project == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Error,
                    (int)EventType.TeamProjectNotFound,
                    "Team project '{0}' is not found",
                    teamProjectName);
                throw new ActivityExecutionException(AppResource.TeamProjectNotFound);
            }

            if (string.IsNullOrEmpty(queryText))
            {
                queryText = "select * from TestPlan";
            }
            var plans = project.TestPlans.Query(queryText);

            var planProps = new List<Dictionary<string, object>>();

            foreach (var plan in plans)
            {
                var dict = new Dictionary<string, object>();
                dict[PublishedData.TestPlanId] = plan.Id;
                dict[PublishedData.TestPlanName] = plan.Name;
                dict[PublishedData.Description] = plan.Description;
                dict[PublishedData.Owner] = plan.Owner.DisplayName;
                dict[PublishedData.AreaPath] = plan.AreaPath;
                dict[PublishedData.State] = plan.State.ToString();
                dict[PublishedData.StartDate] = plan.StartDate;
                dict[PublishedData.EndDate] = plan.EndDate;
                dict[PublishedData.Iteration] = plan.Iteration;

                planProps.Add(dict);
            }

            return planProps;
        }

        /// <summary>
        ///     Retrieves all test configurations in a team project
        /// </summary>
        /// <param name="teamProjectName"></param>
        /// <param name="queryText"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetTestConfigurations(
            string teamProjectName,
            string queryText)
        {
            var project = testManagementService.GetTeamProject(teamProjectName);
            
            if (project == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Error,
                    (int)EventType.TeamProjectNotFound,
                    "Team project '{0}' is not found",
                    teamProjectName);
                throw new ActivityExecutionException(AppResource.TeamProjectNotFound);
            }

            if (string.IsNullOrEmpty(queryText))
            {
                queryText = "select * from TestConfiguration";
            }
            var configs = project.TestConfigurations.Query(queryText);

            var configProps = new List<Dictionary<string, object>>();

            foreach (var config in configs)
            {
                var dict = new Dictionary<string, object>();
                dict[PublishedData.Id] = config.Id;
                dict[PublishedData.AreaPath] = config.AreaPath;
                dict[PublishedData.State] = config.State.ToString();
                dict[PublishedData.Description] = config.Description;
                dict[PublishedData.TestConfigurationName] = config.Name;

                configProps.Add(dict);
            }

            return configProps;
        }

        /// <summary>
        ///     Retrieves a list of test suites in a team project
        /// </summary>
        /// <param name="teamProjectName"></param>
        /// <param name="queryText"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetTestSuites(
            string teamProjectName,
            string queryText)
        {
            var project = testManagementService.GetTeamProject(teamProjectName);
            
            if (project == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Error,
                    (int)EventType.TeamProjectNotFound,
                    "Team project '{0}' is not found",
                    teamProjectName);
                throw new ActivityExecutionException(AppResource.TeamProjectNotFound);
            }

            if (string.IsNullOrEmpty(queryText))
            {
                queryText = "select * from TestSuite";
            }
            var suites = project.TestSuites.Query(queryText);

            var suiteProps = new List<Dictionary<string, object>>();

            foreach (var suite in suites)
            {
                var dict = new Dictionary<string, object>();

                dict[PublishedData.TestSuiteId] = suite.Id;
                dict[PublishedData.Description] = suite.Description;
                dict[PublishedData.TestSuiteTitle] = suite.Title;
                dict[PublishedData.TestSuiteState] = suite.State.ToString();
                dict[PublishedData.TestPlanId] = suite.Plan.Id;
                dict[PublishedData.TestPlanName] = suite.Plan.Name;

                string parentSuite = string.Empty;
                if (suite.Parent != null)
                {
                    dict[PublishedData.ParentTestSuiteId] = suite.Parent.Id;
                }
                else
                {
                    dict[PublishedData.ParentTestSuiteId] = string.Empty;
                }

                suiteProps.Add(dict);
            }

            return suiteProps;
        }

        /// <summary>
        ///     Retrieves a list of test runs in a team project
        /// </summary>
        /// <param name="teamProjectName"></param>
        /// <param name="queryText"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetTestRuns(
            string teamProjectName,
            string queryText)
        {
            IEnumerable<ITestRun> runs;

            if (string.IsNullOrEmpty(queryText))
            {
                queryText = "select * from TestRun";
            }

            if (string.IsNullOrEmpty(teamProjectName))
            {
                runs = testManagementService.QueryTestRuns(queryText);
            }
            else
            {
                var project = testManagementService.GetTeamProject(teamProjectName);
                if (project == null)
                {
                    EventTracing.TraceEvent(
                        TraceEventType.Error,
                        (int)EventType.TeamProjectNotFound,
                        "Team project '{0}' is not found",
                        teamProjectName);
                    throw new ActivityExecutionException(AppResource.TeamProjectNotFound);
                }

                runs = project.TestRuns.Query(queryText);
            }

            var runProps = new List<Dictionary<string, object>>();

            foreach (var run in runs)
            {
                var dict = new Dictionary<string, object>();
                dict[PublishedData.TestRunId] = run.Id.ToString(CultureInfo.InvariantCulture);
                dict[PublishedData.TestPlanId] = run.TestPlanId;
                dict[PublishedData.State] = run.State.ToString();
                dict[PublishedData.Title] = run.Title;
                dict[PublishedData.Owner] = run.Owner.DisplayName;
                dict[PublishedData.IsAutomated] = run.IsAutomated.ToString();
                dict[PublishedData.DateCompleted] = run.DateCompleted;
                dict[PublishedData.DateStarted] = run.DateStarted;
                dict[PublishedData.BuildNumber] = run.BuildNumber;
                dict[PublishedData.BuildFlavor] = run.BuildFlavor;
                dict[PublishedData.BuildPlatform] = run.BuildPlatform;
                dict[PublishedData.Comment] = run.Comment;
                dict[PublishedData.ErrorMessage] = run.ErrorMessage;
                dict[PublishedData.Iteration] = run.Iteration;
                dict[PublishedData.LastUpdated] = run.LastUpdated;
                dict[PublishedData.LastUpdatedBy] = run.LastUpdatedBy.DisplayName;

                dict[PublishedData.TotalTests] = string.Empty;
                dict[PublishedData.CompletedTests] = string.Empty;
                dict[PublishedData.FailedTests] = string.Empty;
                dict[PublishedData.InconclusiveTests] = string.Empty;
                dict[PublishedData.InProgressTests] = string.Empty;
                dict[PublishedData.PassedTests] = string.Empty;
                dict[PublishedData.PendingTests] = string.Empty;

                // Just in case any statistics is not available
                try
                {
                    dict[PublishedData.TotalTests] = run.Statistics.TotalTests;
                    dict[PublishedData.CompletedTests] = run.Statistics.CompletedTests;
                    dict[PublishedData.FailedTests] = run.Statistics.FailedTests;
                    dict[PublishedData.InconclusiveTests] = run.Statistics.InconclusiveTests;
                    dict[PublishedData.InProgressTests] = run.Statistics.InProgressTests;
                    dict[PublishedData.PassedTests] = run.Statistics.PassedTests;
                    dict[PublishedData.PendingTests] = run.Statistics.PendingTests;
                }
                catch (TestManagementServerException)
                {
                }

                runProps.Add(dict);
            }

            return runProps;
        }

        /// <summary>
        ///     Retrieves the object of an individual test run
        /// </summary>
        /// <param name="testRunId"></param>
        /// <param name="testRunTitle"></param>
        /// <param name="buildNumber"></param>
        /// <returns></returns>
        public ITestRun GetTestRun(
            int testRunId,
            string testRunTitle,
            string buildNumber)
        {
            ITestRun run = null;

            if (testRunId > 0)
            {
                var runs = testManagementService.QueryTestRuns(string.Format(
                    CultureInfo.InvariantCulture,
                    "select * from TestRun where TestRunId={0}",
                    testRunId));

                if (runs.Count() > 0)
                    run = runs.First();
            }
            else if (!string.IsNullOrEmpty(testRunTitle))
            {
                var queryText = new StringBuilder();
                
                queryText.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "select * from TestRun where Title='{0}'",
                    testRunTitle.Trim());

                if (!string.IsNullOrEmpty(buildNumber))
                {
                    queryText.AppendFormat(
                        CultureInfo.InvariantCulture,
                        " and BuildNumber='{0}'",
                        buildNumber.Trim());
                }

                queryText.Append(" order by TestRunId");

                var runs = testManagementService.QueryTestRuns(queryText.ToString());

                if (runs.Count() > 0)
                    run = runs.Last();
            }

            return run;
        }

        /// <summary>
        ///     Creates a new TestRun object
        /// </summary>
        /// <param name="teamProjectName"></param>
        /// <param name="testPlanId"></param>
        /// <param name="testPlanName"></param>
        /// <param name="isAutomated"></param>
        /// <param name="testRunTitle"></param>
        /// <param name="buildDirectory"></param>
        /// <param name="buildFlavor"></param>
        /// <param name="buildNumber"></param>
        /// <param name="buildPlatform"></param>
        /// <param name="dateStarted"></param>
        /// <param name="dateCompleted"></param>
        /// <param name="owner"></param>
        /// <param name="testSuiteIds"></param>
        /// <param name="testCaseIds"></param>
        /// <returns></returns>
        public ITestRun CreateTestRun(
            string teamProjectName,
            int testPlanId,
            string testPlanName,
            bool isAutomated,
            string testRunTitle,
            string buildDirectory,
            string buildFlavor,
            string buildNumber,
            string buildPlatform,
            DateTime? dateStarted,
            DateTime? dateCompleted,
            string owner,
            IEnumerable<int> testSuiteIds,
            IEnumerable<int> testCaseIds)
        {
            var project = testManagementService.GetTeamProject(teamProjectName);
            if (project == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Error,
                    (int)EventType.TeamProjectNotFound,
                    "Team project '{0}' is not found",
                    teamProjectName);
                throw new ActivityExecutionException(AppResource.TeamProjectNotFound);
            }

            ITestPlan plan = null;

            if (testPlanId > 0)
            {
                var plans = project.TestPlans.Query(string.Format(CultureInfo.InvariantCulture, "select * from TestPlan where PlanId={0}", testPlanId));
                if (plans.Count > 0)
                    plan = plans[0];
            }
            else if (!string.IsNullOrEmpty(testPlanName))
            {
                var plans = project.TestPlans.Query(string.Format(CultureInfo.InvariantCulture, "select * from TestPlan where PlanName='{0}'", testPlanName));
                if (plans.Count > 0)
                    plan = plans[0];
            }

            if (plan == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Error,
                    (int)EventType.TestPlanNotFound,
                    "Test Plan ID='{0}' Name='{1}' is not found",
                    testPlanId,
                    testPlanName);

                throw new ActivityExecutionException(AppResource.TestPlanNotFound);
            }

            var run = plan.CreateTestRun(isAutomated);
            run.Title = testRunTitle;

            // Start filling optional fields

            TeamFoundationIdentity ownerId = null;
            if (!string.IsNullOrEmpty(owner))
                ownerId = GetIdentity(owner);

            if (!string.IsNullOrEmpty(buildDirectory))
                run.BuildDirectory = buildDirectory;

            if (!string.IsNullOrEmpty(buildFlavor))
                run.BuildFlavor = buildFlavor;
            
            if (!string.IsNullOrEmpty(buildNumber))
                run.BuildNumber = buildNumber;
            
            if (!string.IsNullOrEmpty(buildPlatform))
                run.BuildPlatform = buildPlatform;
            
            if (dateStarted != null)
                run.DateStarted = dateStarted.Value;
            
            if (dateCompleted != null)
                run.DateStarted = dateCompleted.Value;

            run.Owner = ownerId;

            // Add the test points
            foreach (var testCaseId in testCaseIds)
            {
                var testPoints = plan.QueryTestPoints(string.Format(
                    CultureInfo.InvariantCulture,
                    "select * from TestPoint where PlanId={0} and TestCaseId={1}",
                    plan.Id,
                    testCaseId));

                foreach (var testPoint in testPoints)
                {
                    if (testPoint != null)
                        run.AddTestPoint(testPoint, ownerId);
                }
            }

            foreach (var testSuiteId in testSuiteIds)
            {
                var testPoints = plan.QueryTestPoints(string.Format(
                    CultureInfo.InvariantCulture,
                    "select * from TestPoint where PlanId={0} and SuiteId={1}",
                    plan.Id,
                    testSuiteId));

                foreach (var testPoint in testPoints)
                {
                    if (testPoint != null)
                        run.AddTestPoint(testPoint, ownerId);
                }
            }

            run.Save();

            return run;
        }

        /// <summary>
        ///     Adds the result of a test case to the TestRun
        /// </summary>
        /// <param name="run"></param>
        /// <param name="testCaseId"></param>
        /// <param name="owner"></param>
        /// <param name="errorMessage"></param>
        /// <param name="outCome"></param>
        /// <param name="dateStarted"></param>
        /// <param name="dateCompleted"></param>
        /// <param name="attachment"></param>
        public void AddTestCaseResult(
            ITestRun run,
            int testCaseId,
            string owner,
            string errorMessage,
            TestOutcome outCome,
            DateTime? dateStarted,
            DateTime? dateCompleted,
            string attachment)
        {
            var testCaseResult = run.QueryResults().First(result => result.TestCaseId == testCaseId);
            if (testCaseResult == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Error,
                    (int)EventType.TestCaseNotFound,
                    "Test Case ID='{0}' not found",
                    testCaseId);
                throw new ActivityExecutionException(AppResource.TestCaseNotFound);
            }

            testCaseResult.Outcome = outCome;
            testCaseResult.State = TestResultState.Completed;
            testCaseResult.Owner = GetIdentity(owner);

            if (dateStarted != null)
                testCaseResult.DateStarted = dateStarted.Value;
            
            if (dateCompleted != null)
                testCaseResult.DateCompleted = dateCompleted.Value;
            
            if (!string.IsNullOrEmpty(errorMessage))
                testCaseResult.ErrorMessage = errorMessage;
            
            if (!string.IsNullOrEmpty(attachment))
                testCaseResult.CreateAttachment(attachment);

            // testCaseResult.AssociateWorkItem(workItem);
            
            testCaseResult.Save();
        }

        /// <summary>
        ///     Retrieves the TeamFoundationIdentity from the Display Name
        /// </summary>
        /// <param name="userName">User name as DisplayName, e.g. "Zhenhua Yao"</param>
        /// <returns></returns>
        private TeamFoundationIdentity GetIdentity(string userName)
        {
            return idService.ReadIdentity(IdentitySearchFactor.DisplayName, userName, MembershipQuery.None, ReadIdentityOptions.None);
        }
    }
}
