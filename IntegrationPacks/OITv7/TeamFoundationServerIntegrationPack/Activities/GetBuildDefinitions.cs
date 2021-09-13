using System;
using System.Collections.Generic;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Get Build Definitions", Description = "Get the build definitions in a team project")]
    public class GetBuildDefinitions : IActivity
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
            designer.AddInput(PublishedData.BuildDefinitionName).NotRequired();

            designer.AddOutput(PublishedData.Project).AsString();
            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();

            foreach (KeyValuePair<string, string> kvp in PublishedData.BuildDefinitionProperties)
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

            response.Publish(PublishedData.Project, teamProject);

            var results = buildHelper.GetBuildDefinitions(
                teamProject,
                definitionName);

            if (results == null)
            {
                throw new ActivityExecutionException(AppResource.BuildDefinitionNotFound);
            }

            response.Publish(PublishedData.NumberOfObjects, results.Count);

            foreach (Dictionary<string, object> res in results)
            {
                response.WithFiltering().Publish(res);
            }
        }
    }
}

