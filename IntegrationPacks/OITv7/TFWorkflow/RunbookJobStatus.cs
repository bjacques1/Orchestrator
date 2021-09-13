using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Net;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace Orchestrator2012.Workflow
{
    [OrchestratorCategory, BuildActivity(HostEnvironmentOption.All), MyDescription("RunbookJobStatusDesc")]
    public sealed class RunbookJobStatus : CodeActivity<string>
    {
        [DefaultValue("http://localhost:81/Orchestrator2012/Orchestrator.svc"), RequiredArgument, MyDescription("OrchestratorUrlDesc")]
        public InArgument<string> OrchestratorUrl { get; set; }

        [DefaultValue(""), RequiredArgument, MyDescription("JobIdDesc")]
        public InArgument<Guid> JobId { get; set; }

        [MyDescription("TestOutcomeDesc")]
        public OutArgument<string> TestOutcome { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [MyDescription("RunbookReturnDataDesc")]
        public OutArgument<Dictionary<string,string>> RunbookReturnData { get; set; }

        protected override string Execute(CodeActivityContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            } 
            
            var url = OrchestratorUrl.Get(context);
            var jobId = JobId.Get(context);

            var sco = new OrchestratorServiceReference.OrchestratorContext(new Uri(url));
            sco.Credentials = CredentialCache.DefaultNetworkCredentials;
            sco.MergeOption = MergeOption.OverwriteChanges;

            context.TrackBuildMessage(string.Format(CultureInfo.InvariantCulture,
                AppResource.GetJobStatusById,
                url, jobId));

            OrchestratorServiceReference.Job job = null;
            try
            {
                job = (from j in sco.Jobs where j.Id == jobId select j).FirstOrDefault();
            }
            catch (DataServiceQueryException e)
            {
                // If the connection is lost, return the status to the caller without throwing the exception
                context.TrackBuildWarning(e.Message);

                return "DataServiceQueryException";
            }

            if (job == null)
            {
                var msg = string.Format(CultureInfo.InvariantCulture, AppResource.JobNotFound, jobId);
                context.TrackBuildError(msg);
                throw new WorkflowApplicationException(msg);
            }

            var jobOutput = new Dictionary<string, string>();

            if (job.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                context.TrackBuildMessage(AppResource.JobStatusCompleted);

                // Get the return data
                var instances = sco.RunbookInstances.Where(ri => ri.JobId == jobId);

                // For the non-monitor runbook job, there should be only one instance.
                foreach (var instance in instances)
                {
                    context.TrackBuildMessage(string.Format(CultureInfo.InvariantCulture, 
                        AppResource.InstanceMsg,
                        instance.Id, instance.Status));

                    if (!instance.Status.Equals("Success", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var outParameters = sco.RunbookInstanceParameters.Where(
                        rip => rip.RunbookInstanceId == instance.Id && rip.Direction.Equals("Out", StringComparison.OrdinalIgnoreCase));

                    foreach (var parameter in outParameters)
                    {
                        context.TrackBuildMessage(string.Format(CultureInfo.InvariantCulture, 
                            AppResource.InstanceParameter,
                            parameter.Name, parameter.Value));

                        jobOutput[parameter.Name] = parameter.Value;
                    }
                }

                // Job is completed, set the return data in the workflow and TestOutcome
                RunbookReturnData.Set(context, jobOutput);

                if (jobOutput.ContainsKey("TestOutcome"))
                {
                    TestOutcome.Set(context, jobOutput["TestOutcome"]);
                }
                else
                {
                    context.TrackBuildWarning(AppResource.NoTestOutcomeParameter);

                    TestOutcome.Set(context, "Unknown");
                }
            }

            return job.Status;
        }
    }
}
