using System;
using System.Collections.Generic;
using Microsoft.SystemCenter.Orchestrator.Integration;
using Microsoft.TeamFoundation.Build.Client;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Get Build Details", Description = "Get the build details")]
    public class GetBuildDetails : IActivity
    {
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
            designer.AddInput(PublishedData.MaxBuildsPerDefinition).WithDefaultValue(5).NotRequired();
            designer.AddInput(PublishedData.BuildNumber).NotRequired();
            designer.AddInput(PublishedData.BuildQuality).NotRequired();
            designer.AddInput(PublishedData.BuildStatus).NotRequired().WithEnumBrowser(typeof(BuildStatus));
            designer.AddInput(PublishedData.FinishedAfter).NotRequired().WithDateTimeBrowser();
            designer.AddInput(PublishedData.FinishedBefore).NotRequired().WithDateTimeBrowser();
            designer.AddInput(PublishedData.BuildQueryOrder).NotRequired()
                .WithEnumBrowser(typeof(BuildQueryOrder)).WithDefaultValue(BuildQueryOrder.StartTimeDescending);

            designer.AddOutput(PublishedData.Project).AsString();
            designer.AddOutput(PublishedData.MaxBuildsPerDefinition).AsNumber();
            designer.AddOutput(PublishedData.FinishedAfter);
            designer.AddOutput(PublishedData.FinishedBefore);
            designer.AddOutput(PublishedData.BuildQueryOrder);

            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();

            foreach (KeyValuePair<string, string> kvp in PublishedData.BuildProperties)
            {
                switch (kvp.Value)
                {
                    case PublishedData.AsDateTime:
                        designer.AddOutput(kvp.Key).AsDateTime().WithFilter();
                        break;

                    case PublishedData.AsNumber:
                        designer.AddOutput(kvp.Key).AsNumber().WithFilter();
                        break;

                    case PublishedData.AsString:
                        designer.AddOutput(kvp.Key).AsString().WithFilter();
                        break;
                }
            }
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

            var buildHelper = new BuildClientHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);

            var teamProject = request.Inputs[PublishedData.Project].AsString();
            var definitionName = request.Inputs[PublishedData.BuildDefinitionName].AsString();

            int maxBuildsPerDefinition = 0;
            if (request.Inputs.Contains(PublishedData.MaxBuildsPerDefinition))
            {
                maxBuildsPerDefinition = request.Inputs[PublishedData.MaxBuildsPerDefinition].AsInt32();
                response.Publish(PublishedData.MaxBuildsPerDefinition, maxBuildsPerDefinition);
            }
            
            var buildNumber = request.Inputs[PublishedData.BuildNumber].AsString();
            var status = request.Inputs[PublishedData.BuildStatus].AsString();
            var quality = request.Inputs[PublishedData.BuildQuality].AsString();

            DateTime finishedAfter;
            if (request.Inputs.Contains(PublishedData.FinishedAfter))
            {
                finishedAfter = request.Inputs[PublishedData.FinishedAfter].AsDateTime();
                response.Publish(PublishedData.FinishedAfter, finishedAfter);
            }
            else
            {
                finishedAfter = new DateTime(1900, 1, 1);
            }

            DateTime finishedBefore;
            if (request.Inputs.Contains(PublishedData.FinishedBefore))
            {
                finishedBefore = request.Inputs[PublishedData.FinishedBefore].AsDateTime();
                response.Publish(PublishedData.FinishedBefore, finishedBefore);
            }
            else
            {
                finishedBefore = new DateTime(9999, 1, 1);
            }

            var buildQueryOrder = BuildQueryOrder.StartTimeDescending;
            if (request.Inputs.Contains(PublishedData.BuildQueryOrder))
            {
                buildQueryOrder = request.Inputs[PublishedData.BuildQueryOrder].As<BuildQueryOrder>();
                response.Publish(PublishedData.BuildQueryOrder, buildQueryOrder.ToString());
            }

            response.Publish(PublishedData.Project, teamProject);

            var results = buildHelper.GetBuildDetails(
                teamProject,
                definitionName,
                maxBuildsPerDefinition,
                buildNumber,
                status,
                quality,
                finishedAfter,
                finishedBefore,
                buildQueryOrder);

            if (results == null)
            {
                throw new ActivityExecutionException(AppResource.BuildNotFound);
            }

            response.Publish(PublishedData.NumberOfObjects, results.Count);

            foreach (Dictionary<string, object> res in results)
            {
                response.WithFiltering().Publish(res);
            }
        }
    }
}

