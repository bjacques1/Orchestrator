using System;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CheckOut")]
    [Activity("Check Out", Description = "Check out for adding, deleteing, or editting from the version control server")]
    public class CheckOut : IActivity
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
            designer.AddInput(PublishedData.CheckoutAction).WithEnumBrowser(typeof(CheckoutAction));
            designer.AddInput(PublishedData.VersionControlRecursionType).WithEnumBrowser(typeof(RecursionType)).WithDefaultValue(RecursionType.None);

            designer.AddOutput(PublishedData.WorkspacePath).AsString();
            designer.AddOutput(PublishedData.ItemLocalPath).AsString();
            designer.AddOutput(PublishedData.CheckoutAction).AsString();
            designer.AddOutput(PublishedData.VersionControlRecursionType).AsString();
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
            var action = request.Inputs[PublishedData.CheckoutAction].AsString();
            var recursion = request.Inputs[PublishedData.VersionControlRecursionType].AsString();

            var vcHelper = new VersionControlHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);

            vcHelper.Checkout(workspacePath, localPath, action, recursion);

            response.Publish(PublishedData.WorkspacePath, workspacePath);
            response.Publish(PublishedData.ItemLocalPath, localPath);
            response.Publish(PublishedData.CheckoutAction, action);
            response.Publish(PublishedData.VersionControlRecursionType, recursion);
        }
    }
}

