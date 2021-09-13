using System;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Check In", Description = "Check in pending changes to the version control server")]
    public class CheckIn : IActivity
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
            designer.AddInput(PublishedData.ItemLocalPath).WithFileBrowser();
            designer.AddInput(PublishedData.CheckinComment);
            designer.AddInput(PublishedData.VersionControlRecursionType).WithEnumBrowser(typeof(RecursionType)).WithDefaultValue(RecursionType.None);

            designer.AddOutput(PublishedData.WorkspacePath);
            designer.AddOutput(PublishedData.ItemLocalPath);
            designer.AddOutput(PublishedData.CheckinComment);
            designer.AddOutput(PublishedData.VersionControlRecursionType);
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
            var localPath = request.Inputs[PublishedData.ItemLocalPath].AsString();
            var comment = request.Inputs[PublishedData.CheckinComment].AsString();
            var recursion = request.Inputs[PublishedData.VersionControlRecursionType].AsString();

            var vcHelper = new VersionControlHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);

            vcHelper.Checkin(workspacePath, localPath, recursion, comment);

            response.Publish(PublishedData.WorkspacePath, workspacePath);
            response.Publish(PublishedData.ItemLocalPath, localPath);
            response.Publish(PublishedData.CheckinComment, comment);
            response.Publish(PublishedData.VersionControlRecursionType, recursion);
        }
    }
}

