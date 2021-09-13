using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SystemCenter.Orchestrator.Integration;
using Microsoft.TeamFoundation.Build.Client;

namespace TeamFoundationServerIntegrationPack
{
    [ActivityMonitor(Interval=60)]
    [Activity("Monitor Build", Description="Monitors queued build and triggers when the status is changing or changed")]
    public class MonitorBuild: IActivity
    {
        private BuildClientHelper buildHelper = null;
        private const int queryInterval = 10;

        [ActivityConfiguration]
        public TfsConnectionSettings ConnectionSettings
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

            designer.AddInput(PublishedData.Project);
            designer.AddInput(PublishedData.BuildDefinitionName);

            designer.AddOutput(PublishedData.BuildLabelName).WithFilter();
            designer.AddOutput(PublishedData.BuildNumber).WithFilter();
            designer.AddOutput(PublishedData.BuildEvent).WithFilter();
        }

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

            if (buildHelper == null)
            {
                buildHelper = new BuildClientHelper(
                    ConnectionSettings.Url,
                    ConnectionSettings.Domain,
                    ConnectionSettings.UserName,
                    ConnectionSettings.Password);
            }

            var teamProject = request.Inputs[PublishedData.Project].AsString();
            var definitionName = request.Inputs[PublishedData.BuildDefinitionName].AsString();

            while (!buildHelper.SetBuildEventHandlers(teamProject, definitionName))
            {
                System.Threading.Thread.Sleep(queryInterval * 1000);
            }

            buildHelper.BuildStatusChanged.WaitOne();

            var data = buildHelper.GetBuildEvent();
            response.WithFiltering().Publish(data);
        }
    }
}

