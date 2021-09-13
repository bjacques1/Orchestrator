using System;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Download file", Description = "Download a file from the version control server")]
    public class DownloadFile : IActivity
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

            designer.AddInput(PublishedData.ItemServerPath);
            designer.AddInput(PublishedData.ItemLocalPath).WithFileBrowser();

            designer.AddOutput(PublishedData.ItemServerPath).AsString();
            designer.AddOutput(PublishedData.ItemLocalPath).AsString();
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

            var vcHelper = new VersionControlHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);

            var serverPath = request.Inputs[PublishedData.ItemServerPath].AsString();
            var localPath = request.Inputs[PublishedData.ItemLocalPath].AsString();

            vcHelper.DownloadFile(serverPath, localPath);

            response.Publish(PublishedData.ItemServerPath, serverPath);
            response.Publish(PublishedData.ItemLocalPath, localPath);
        }
    }
}

