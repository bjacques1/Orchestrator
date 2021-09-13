using System;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Request Build", Description = "Request a new build with optional shelveset and drop location")]
    public class RequestBuild : IActivity
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
            designer.AddInput(PublishedData.DropLocation).NotRequired();
            designer.AddInput(PublishedData.Priority).NotRequired().WithEnumBrowser(typeof(QueuePriority));
            designer.AddInput(PublishedData.Reason).NotRequired().WithEnumBrowser(typeof(BuildReason));
            designer.AddInput(PublishedData.RequestedFor).NotRequired();
            designer.AddInput(PublishedData.ShelvesetName).NotRequired();

            designer.AddOutput(PublishedData.Project).AsString();
            designer.AddOutput(PublishedData.BuildDefinitionName).AsString();
            designer.AddOutput(PublishedData.DropLocation).AsString();
            designer.AddOutput(PublishedData.Priority).AsString();
            designer.AddOutput(PublishedData.Reason).AsString();
            designer.AddOutput(PublishedData.RequestedFor).AsString();
            designer.AddOutput(PublishedData.ShelvesetName).AsString();
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

            string teamProject = request.Inputs[PublishedData.Project].AsString();
            string definitionName = request.Inputs[PublishedData.BuildDefinitionName].AsString();
            string dropLocation = request.Inputs[PublishedData.DropLocation].AsString();
            string priority = request.Inputs[PublishedData.Priority].AsString();
            string reason = request.Inputs[PublishedData.Reason].AsString();
            string requestedFor = request.Inputs[PublishedData.RequestedFor].AsString();
            string shelvesetName = request.Inputs[PublishedData.ShelvesetName].AsString();

            response.Publish(PublishedData.Project, teamProject);
            response.Publish(PublishedData.BuildDefinitionName, definitionName);
            response.Publish(PublishedData.DropLocation, dropLocation);
            response.Publish(PublishedData.Priority, priority);
            response.Publish(PublishedData.Reason, reason);
            response.Publish(PublishedData.RequestedFor, requestedFor);
            response.Publish(PublishedData.ShelvesetName, shelvesetName);

            var result = buildHelper.RequestBuild(
                teamProject,
                definitionName,
                dropLocation,
                priority,
                reason,
                requestedFor,
                shelvesetName);

            if (!result)
            {
                throw new ActivityExecutionException(AppResource.BuildDefinitionNotFound);
            }
        }
    }
}

