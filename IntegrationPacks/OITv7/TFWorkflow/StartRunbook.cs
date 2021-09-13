using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace Orchestrator2012.Workflow
{
    /// <summary>
    ///     Support of localized description in the attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    internal sealed class MyDescriptionAttribute : DescriptionAttribute
    {
        public MyDescriptionAttribute(string description) : base(Get(description))
        {
        }

        private static string Get(string resourceName)
        {
            return AppResource.ResourceManager.GetString(resourceName, AppResource.Culture);
        }
    }

    [OrchestratorCategory, BuildActivity(HostEnvironmentOption.All), MyDescription("StartRunbookDesc")]
    public sealed class StartRunbook : CodeActivity<Guid>
    {
        [RequiredArgument, DefaultValue("http://localhost:81/Orchestrator2012/Orchestrator.svc"), MyDescription("OrchestratorUrlDesc")]
        public InArgument<string> OrchestratorUrl { get; set; }

        [RequiredArgument, DefaultValue(@"\MainTest"), MyDescription("RunbookPathDesc")]
        public InArgument<string> RunbookPath { get; set; }

        [RequiredArgument, DefaultValue(""), MyDescription("BinariesDirectoryDesc")]
        public InArgument<string> BinariesDirectory { get; set; }

        [RequiredArgument, DefaultValue(""), MyDescription("BuildConfigurationDesc")]
        public InArgument<string> BuildConfiguration { get; set; }

        [RequiredArgument, DefaultValue(""), MyDescription("CustomParameterDesc")]
        public InArgument<string> CustomParameter { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.RequireExtension<IBuildDetail>();
        }

        protected override Guid Execute(CodeActivityContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var buildDetail = context.GetExtension<IBuildDetail>();
            var url = OrchestratorUrl.Get(context);
            var runbookPath = RunbookPath.Get(context);
            var binariesDirectory = BinariesDirectory.Get(context);

            var runbookParameters = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(binariesDirectory))
            {
                if (string.IsNullOrWhiteSpace(buildDetail.DropLocation))
                {
                    context.TrackBuildError(AppResource.BothDropLocationAndBinariesDirectoryAreEmpty);
                    throw new WorkflowApplicationException(AppResource.BothDropLocationAndBinariesDirectoryAreEmpty);
                }

                runbookParameters["DropLocation"] = buildDetail.DropLocation;
            }
            else
            {
                runbookParameters["DropLocation"] = binariesDirectory;
            }

            runbookParameters["BuildNumber"] = buildDetail.BuildNumber;
            runbookParameters["TeamProject"] = buildDetail.TeamProject;
            runbookParameters["BuildDefinition"] = buildDetail.BuildDefinition.Name;
            runbookParameters["ShelveSetName"] = buildDetail.ShelvesetName;
            runbookParameters["BuildReason"] = buildDetail.Reason.ToString();
            runbookParameters["BuildUri"] = buildDetail.Uri.AbsoluteUri.ToString(CultureInfo.InvariantCulture);
            runbookParameters["RequestedFor"] = buildDetail.RequestedFor;
            runbookParameters["BuildConfiguration"] = BuildConfiguration.Get(context);
            runbookParameters["CustomParameter"] = CustomParameter.Get(context);

            var sco = new OrchestratorServiceReference.OrchestratorContext(new Uri(url));
            sco.Credentials = CredentialCache.DefaultNetworkCredentials;
            sco.MergeOption = MergeOption.OverwriteChanges;

            context.TrackBuildMessage(string.Format(CultureInfo.InvariantCulture,
                AppResource.StartRunbook,
                url, runbookPath,
                runbookParameters["DropLocation"],
                runbookParameters["BuildNumber"],
                runbookParameters["CustomParameter"]));

            var runbook = (from rb in sco.Runbooks
                           where rb.Path == runbookPath
                           select rb).FirstOrDefault();

            if (runbook == null)
            {
                var userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                var msg = string.Format(CultureInfo.InvariantCulture, AppResource.RunbookNotFound, runbookPath, userName);

                context.TrackBuildError(msg);
                throw new WorkflowApplicationException(msg);
            }

            CheckRunbookOutParameter(context, sco, runbook.Id);

            var runbookParameterIds = GetRunbookInParameters(context, sco, runbook.Id);
            
            // Create a job

            var job = new OrchestratorServiceReference.Job();
            job.RunbookId = runbook.Id;
            job.Parameters = BuildRunbookParameters(context, runbookParameters, runbookParameterIds);

            context.TrackBuildMessage(string.Format(CultureInfo.InvariantCulture, AppResource.JobParameter, job.Parameters));

            sco.AddToJobs(job);
            context.TrackBuildMessage(AppResource.PostingJob);

            var response = sco.SaveChanges();

            context.TrackBuildMessage(string.Format(CultureInfo.InvariantCulture, AppResource.JobId, job.Id.ToString("B")));

            return job.Id;
        }

        /// <summary>
        ///     Checks and returns the input parameters of the runbook
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sco"></param>
        /// <param name="runbookId"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetRunbookInParameters(
            CodeActivityContext context,
            OrchestratorServiceReference.OrchestratorContext sco,
            Guid runbookId)
        {
            var runbookParameterIds = new Dictionary<string, string>();

            var inParams = from p in sco.RunbookParameters
                           where (p.RunbookId == runbookId && p.Direction.Equals("In", StringComparison.OrdinalIgnoreCase))
                           select p;
            foreach (var p in inParams)
            {
                context.TrackBuildMessage(string.Format(CultureInfo.InvariantCulture,
                    AppResource.RunbookInputParameter,
                    p.Name, p.Id.ToString("B"), p.Type));

                runbookParameterIds[p.Name] = p.Id.ToString("B");
            }

            return runbookParameterIds;
        }

        /// <summary>
        ///     Checks if the runbook can return "TestOutcome" of string as the output
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sco"></param>
        /// <param name="runbookId"></param>
        private static void CheckRunbookOutParameter(
            CodeActivityContext context,
            OrchestratorServiceReference.OrchestratorContext sco,
            Guid runbookId)
        {
            // Check if the runbook can return "TestOutcome" of string as the output
            var testOutcomeParam = (from p in sco.RunbookParameters
                                    where (p.RunbookId == runbookId &&
                                           p.Direction.Equals("Out", StringComparison.OrdinalIgnoreCase) &&
                                           p.Name.Equals("TestOutcome", StringComparison.Ordinal))
                                    select p).FirstOrDefault();
            if (testOutcomeParam == null)
            {
                var msg = AppResource.NoTestOutcomeReturn;

                context.TrackBuildError(msg);
                throw new WorkflowApplicationException(msg);
            }

            if (!testOutcomeParam.Type.Equals("String", StringComparison.OrdinalIgnoreCase))
            {
                var msg = AppResource.TestOutcomeNotString;

                context.TrackBuildError(msg);
                throw new WorkflowApplicationException(msg);
            }
        }

        private static string BuildRunbookParameters(
            CodeActivityContext context,
            Dictionary<string, string> runbookParameters,
            Dictionary<string, string> runbookParameterIds)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<Data>");

            foreach (var kv in runbookParameters)
            {
                if (!runbookParameterIds.ContainsKey(kv.Key))
                {
                    var msg = string.Format(CultureInfo.InvariantCulture,
                        AppResource.NoInputParameter,
                        kv.Key);

                    context.TrackBuildError(msg);
                    throw new WorkflowApplicationException(msg);
                }

                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<Parameter><Name>{0}</Name><ID>{1}</ID><Value>{2}</Value></Parameter>",
                    kv.Key,
                    runbookParameterIds[kv.Key],
                    kv.Value);

                // Remove the key/value so we know it's used
                runbookParameterIds.Remove(kv.Key);
            }

            // for all parameters that are defined in the runbook but we do not use, fill in empty value
            foreach (var kv in runbookParameterIds)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<Parameter><Name>{0}</Name><ID>{1}</ID><Value>{2}</Value></Parameter>",
                    kv.Key,
                    kv.Value,
                    string.Empty);

                context.TrackBuildMessage(string.Format(CultureInfo.InvariantCulture, AppResource.ParameterSetToEmpty, kv.Key));
            }

            sb.AppendLine("</Data>");

            return sb.ToString();
        }
    }
}
