using System;
using System.Collections.Generic;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Get Shelveset", Description = "Get the details of a shelveset or a list of shelvesets")]
    public class GetShelveSet : IActivity
    {
        [ActivityConfiguration]
        public TfsConnectionSettings ConnectionSettings
        {
            set;
            get;
        }

        public void Design(IActivityDesigner designer)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }

            designer.AddInput(PublishedData.ShelvesetName);
            designer.AddInput(PublishedData.Owner);

            // For the list
            designer.AddOutput(PublishedData.Comment).AsString().WithFilter();
            designer.AddOutput(PublishedData.CreationDate).AsDateTime().WithFilter();
            designer.AddOutput(PublishedData.DisplayName).AsString().WithFilter();
            designer.AddOutput(PublishedData.ShelvesetName).AsString().WithFilter();
            designer.AddOutput(PublishedData.Owner).AsString().WithFilter();

            // for individual shelveset
            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();
            designer.AddOutput(PublishedData.ItemType).AsString().WithFilter();
            designer.AddOutput(PublishedData.FileName).AsString().WithFilter();
            designer.AddOutput(PublishedData.ChangeType).AsString().WithFilter();
            designer.AddOutput(PublishedData.ItemFolder).AsString().WithFilter();
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
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

            var shelvesetName = request.Inputs[PublishedData.ShelvesetName].AsString();
            var owner = request.Inputs[PublishedData.Owner].AsString();

            List<Dictionary<string,object>> returnedObjects;

            if (string.IsNullOrEmpty(shelvesetName) || string.IsNullOrEmpty(owner))
            {
                returnedObjects = vcHelper.GetListOfShelveSets(shelvesetName, owner);
            }
            else
            {
                returnedObjects = vcHelper.GetShelveSet(shelvesetName, owner);
            }

            if (returnedObjects == null)
            {
                response.Publish(PublishedData.NumberOfObjects, 0);
            }
            else
            {
                response.Publish(PublishedData.NumberOfObjects, returnedObjects.Count);

                foreach (var obj in returnedObjects)
                {
                    response.WithFiltering().Publish(obj);
                }
            }
        }
    }
}
