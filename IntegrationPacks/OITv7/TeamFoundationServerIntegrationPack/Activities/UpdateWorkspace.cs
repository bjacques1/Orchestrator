using System;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Update Workspace", Description = "Update the workspace to the latest version from the version control server")]
    public class UpdateWorkspace : IActivity
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

            designer.AddInput(PublishedData.WorkspacePath).WithDefaultValue("C:\\").WithFolderBrowser();

            designer.AddOutput(PublishedData.WorkspacePath).AsString();
            designer.AddOutput(PublishedData.NumberOfUpdated).AsNumber();
            designer.AddOutput(PublishedData.NumberOfConflicts).AsNumber();
            designer.AddOutput(PublishedData.NumberOfFailures).AsNumber();
            designer.AddOutput(PublishedData.NumberOfWarnings).AsNumber();
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

            var workspacePath = request.Inputs[PublishedData.WorkspacePath].AsString();
            var vcHelper = new VersionControlHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);
            var props = vcHelper.UpdateWorkspace(workspacePath);

            if (props == null)
            {
                throw new ActivityExecutionException(AppResource.FailedToUpdateWorkspace);
            }
            else
            {
                response.Publish(props);
                response.Publish(PublishedData.WorkspacePath, workspacePath);
            }
        }
    }
}

