using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Get Changeset", Description="Get the details of a changeset")]
    public class GetChangeSet : IActivity
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

            designer.AddInput(PublishedData.ItemChangesetId);

            designer.AddOutput(PublishedData.ItemChangesetId).AsNumber();
            designer.AddOutput(PublishedData.CheckinNoteName).AsString().WithFilter();
            designer.AddOutput(PublishedData.CheckinNoteValue).AsString().WithFilter();
            designer.AddOutput(PublishedData.CheckinComment).AsString();
            designer.AddOutput(PublishedData.Committer).AsString();
            designer.AddOutput(PublishedData.CreationDate).AsDateTime();
            designer.AddOutput(PublishedData.Owner).AsString();
            designer.AddOutput(PublishedData.PropertyName).AsString().WithFilter();
            designer.AddOutput(PublishedData.PropertyValue).AsString().WithFilter();
            designer.AddOutput(PublishedData.WorkItemId).AsNumber().WithFilter();
            designer.AddOutput(PublishedData.Title).AsString().WithFilter();
            designer.AddOutput(PublishedData.ChangeType).AsString().WithFilter();
            designer.AddOutput(PublishedData.ItemServerPath).AsString().WithFilter();
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
            
            var id = request.Inputs[PublishedData.ItemChangesetId].AsInt32();

            var changeset = vcHelper.GetChangeset(id);

            if (changeset == null)
            {
                throw new ActivityExecutionException(AppResource.ChangeSetNotFound);
            }

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "Changeset {0}: comment='{1}', owner='{2}'",
                id,
                changeset.Comment,
                changeset.Owner);

            response.Publish(PublishedData.ItemChangesetId, id);
            response.Publish(PublishedData.CheckinComment, changeset.Comment);
            response.Publish(PublishedData.Committer, changeset.Committer);
            response.Publish(PublishedData.CreationDate, changeset.CreationDate);
            response.Publish(PublishedData.Owner, changeset.Owner);

            foreach (var cn in changeset.CheckinNote.Values)
            {
                var dict = new Dictionary<string, string>();
                dict[PublishedData.CheckinNoteName] = cn.Name;
                dict[PublishedData.CheckinNoteValue] = cn.Value;

                response.WithFiltering().Publish(dict);
            }

            foreach (var prop in changeset.Properties)
            {
                var dict = new Dictionary<string, string>();
                dict[PublishedData.PropertyName] = prop.PropertyName;
                dict[PublishedData.PropertyValue] = prop.Value == null? "" : prop.Value.ToString();

                response.WithFiltering().Publish(dict);
            }

            foreach (var wi in changeset.WorkItems)
            {
                var dict = new Dictionary<string, object>();
                dict[PublishedData.WorkItemId] = wi.Id;
                dict[PublishedData.Title] = wi.Title;

                response.WithFiltering().Publish(dict);
            }

            foreach (var change in changeset.Changes)
            {
                var dict = new Dictionary<string, string>();
                dict[PublishedData.ItemServerPath] = change.Item.ServerItem;
                dict[PublishedData.ChangeType] = change.ChangeType.ToString();

                response.WithFiltering().Publish(dict);
            }
        }
    }
}

