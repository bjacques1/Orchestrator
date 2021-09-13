using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    [ActivityMonitor(Interval=60)]
    [Activity("Monitor Work Items Query", Description="Monitors the result of work items query and triggers if the number of result changes")]
    public class MonitorWorkItemsQuery : IActivity
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
            designer.AddInput(PublishedData.QueryInterval).WithDefaultValue(60);
            designer.AddInput(PublishedData.DayPrecision).WithBooleanBrowser().WithDefaultValue(true);
            designer.AddInput(PublishedData.GetChangedWorkItems).WithBooleanBrowser().WithDefaultValue(false);
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

            int pollingInterval = request.Inputs[PublishedData.QueryInterval].AsInt32();
            bool getChangedWorkItems = request.Inputs[PublishedData.GetChangedWorkItems].AsBoolean();

            var wiHelper = new WorkItemHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);

            int count = wiHelper.QueryCountByWiql(wiql, dayPrecision);
            List<Dictionary<string, object>> originalWorkItemList = null, currentWorkItemList = null;

            if (getChangedWorkItems)
            {
                originalWorkItemList = wiHelper.QueryByWiq(wiql, dayPrecision);
            }

            while (true)
            {
                var new_count = wiHelper.QueryCountByWiql(wiql, dayPrecision);

                if (new_count != count)
                {
                    count = new_count;
                    break;
                }

                count = new_count;
                System.Threading.Thread.Sleep(1000 * pollingInterval);
            }

            if (getChangedWorkItems)
            {
                currentWorkItemList = wiHelper.QueryByWiq(wiql, dayPrecision);

                var unique = FindUniqueWorkItems(originalWorkItemList, currentWorkItemList);

                response.Publish(PublishedData.NumberOfObjects, unique.Count);

                foreach (Dictionary<string, object> props in unique)
                {
                    response.WithFiltering().Publish(props);
                }
            }
            else
            {
                response.Publish(PublishedData.NumberOfObjects, count);
            }
        }

        private static List<Dictionary<string,object>> FindUniqueWorkItems(
            List<Dictionary<string, object>> originalWorkItemList,
            List<Dictionary<string, object>> currentWorkItemList)
        {
            foreach (Dictionary<string, object> wi in originalWorkItemList)
            {
                var found = currentWorkItemList.Any(item => (int)wi[PublishedData.Id] == (int)item[PublishedData.Id]);

                EventTracing.TraceInfo("WI {0} found: {1}", wi[PublishedData.Id], found);

                if (found)
                {
                    currentWorkItemList.RemoveAll(item => (int)wi[PublishedData.Id] == (int)item[PublishedData.Id]);
                }
                else
                {
                    currentWorkItemList.Add(wi);
                }
            }

            return currentWorkItemList;
        }
    }
}

