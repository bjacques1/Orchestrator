using System;
using System.Collections.Generic;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    /// <summary>
    ///     Activity: Query Work Items.
    /// </summary>
    [Activity("Query Work Items", Description = "Queries work items using the Work Item Query Language")]
    public class QueryWorkItems : IActivity
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

            designer.AddInput(PublishedData.WorkItemQuery);
            designer.AddInput(PublishedData.DayPrecision).WithBooleanBrowser().WithDefaultValue(true);
            designer.AddOutput(PublishedData.WorkItemQuery).AsString();

            designer.AddOutput(PublishedData.NumberOfObjects).AsNumber();

            foreach (KeyValuePair<string, string> kv in PublishedData.QueryWorkItems)
            {
                switch (kv.Value)
                {
                    case PublishedData.AsDateTime:
                        designer.AddOutput(kv.Key).AsDateTime().WithFilter();
                        break;

                    case PublishedData.AsNumber:
                        designer.AddOutput(kv.Key).AsNumber().WithFilter();
                        break;

                    case PublishedData.AsString:
                        designer.AddOutput(kv.Key).AsString().WithFilter();
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

            string wiql = request.Inputs[PublishedData.WorkItemQuery].AsString();
            bool dayPrecision = request.Inputs[PublishedData.DayPrecision].AsBoolean();

            if (string.IsNullOrEmpty(wiql))
            {
                throw new ActivityExecutionException(AppResource.WiqlShouldNotBeEmpty);
            }

            response.Publish(PublishedData.WorkItemQuery, wiql);
            response.Publish(PublishedData.DayPrecision, dayPrecision);

            var wiHelper = new WorkItemHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);
            var listOfProps = wiHelper.QueryByWiq(wiql, dayPrecision);

            if (listOfProps != null)
            {
                response.Publish(PublishedData.NumberOfObjects, listOfProps.Count);

                foreach (Dictionary<string, object> props in listOfProps)
                {
                    response.WithFiltering().Publish(props);
                }
            }
            else
            {
                throw new ActivityExecutionException(AppResource.QueryFailed);
            }
        }
    }
}

