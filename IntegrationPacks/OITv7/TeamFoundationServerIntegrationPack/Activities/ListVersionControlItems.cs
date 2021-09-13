using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("List Version Control Items", Description = "Lists files and directories at a version control server")]
    public class ListVersionControlItems : IActivity
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

            designer.AddInput(PublishedData.VersionControlServerPath).WithDefaultValue("$/*");
            designer.AddInput(PublishedData.VersionControlRecursionType).WithEnumBrowser(typeof(RecursionType)).WithDefaultValue("Full");

            designer.AddOutput(PublishedData.VersionControlServerPath).AsString();
            designer.AddOutput(PublishedData.VersionControlRecursionType).AsString();

            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();
            designer.AddOutput(PublishedData.ItemType).AsString().WithFilter();
            designer.AddOutput(PublishedData.ItemServerPath).AsString().WithFilter();
            designer.AddOutput(PublishedData.ItemContentLength).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.ItemChangesetId).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.ItemCheckinDate).AsDateTime().WithFilter();
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

            var path = request.Inputs[PublishedData.VersionControlServerPath].AsString();

            if (string.IsNullOrEmpty(path))
            {
                throw new ActivityExecutionException(AppResource.VersionControlServerPathShouldNotBeEmpty);
            }

            var recursion = request.Inputs[PublishedData.VersionControlRecursionType].AsString();

            var vcHelper = new VersionControlHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);
            var results = vcHelper.GetItems(path, recursion);
            if (results == null)
            {
                throw new ActivityExecutionException(AppResource.FailedToGetItems);
            }

            response.Publish(PublishedData.NumberOfObjects, results.Count);

            foreach (Dictionary<string, object> item in results)
            {
                response.WithFiltering().Publish(item);
            }

            response.Publish(PublishedData.VersionControlServerPath, path);
            response.Publish(PublishedData.VersionControlRecursionType, recursion);
        }
    }
}

