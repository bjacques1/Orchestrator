using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TeamFoundationServerIntegrationPack
{
    class WorkItemHelper
    {
        private WorkItemStore workItemStore;
        private const string fieldDict = "Field Dict";

        /// <summary>
        ///     Constructor.  Connects to the TFS server and retrieves the work item store.
        /// </summary>
        /// <param name="url">TFS server URL, e.g. "http://scxtfs2:8080/tfs"</param>
        public WorkItemHelper(
            string url,
            string domain,
            string userName,
            string password)
        {
            var tfs = TfsConnectionFactory.GetTeamProjectCollection(url, domain, userName, password);
            workItemStore = tfs.GetService<WorkItemStore>();

            EventTracing.TraceInfo("Connect to '{0}' for work item tracking", url);
        }

        /// <summary>
        ///     Retrieves a work item by Id
        /// </summary>
        /// <param name="id">Work item ID</param>
        /// <returns></returns>
        public WorkItem GetWorkItem(
            int id)
        {
            return workItemStore.GetWorkItem(id);
        }

        /// <summary>
        ///     Queries the properties/fields of a work item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Dictionary<string, object> QueryByID(
            int id)
        {
            var ret = WorkItemProperties(GetWorkItem(id));

            EventTracing.TraceInfo(
                "QueryByID({0}): {1}",
                id,
                EventTracing.ToString(ret));

            return ret;
        }

        /// <summary>
        ///     Gets the number of items that would be returned if the query was executed
        /// </summary>
        /// <param name="wiql"></param>
        /// <param name="dayPrecision">True to ignore time values so that DateTime objects are treated as dates; otherwise, false</param>
        /// <returns></returns>
        public int QueryCountByWiql(
            string wiql,
            bool dayPrecision)
        {
            var query = new Query(workItemStore, wiql, null, dayPrecision);
            var count = query.RunCountQuery();

            EventTracing.TraceInfo(
                "QueryCountByWiql: '{0}'",
                count);

            return count;
        }

        /// <summary>
        ///     Queries the work items using Work Item Query Language
        /// </summary>
        /// <param name="wiql"></param>
        /// <param name="dayPrecision">True to ignore time values so that DateTime objects are treated as dates; otherwise, false</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> QueryByWiq(
            string wiql,
            bool dayPrecision)
        {
            var query = new Query(workItemStore, wiql, null, dayPrecision);
            var workItems = query.RunQuery();
            var result = new List<Dictionary<string, object>>();

            EventTracing.TraceInfo(
                "QueryByWiq: '{0}'",
                wiql);

            if (workItems == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Warning,
                    (int)EventType.QueryFailed,
                    "Query failed.",
                    null);
            }
            else
            {
                foreach (WorkItem workItem in workItems)
                {
                    result.Add(WorkItemProperties(workItem, false));
                }
            }

            return result;
        }

        /// <summary>
        ///     Creates a new work item
        /// </summary>
        /// <param name="project"></param>
        /// <param name="wiType"></param>
        /// <returns></returns>
        public WorkItem CreateWorkItem(
            string project,
            string wiType)
        {
            try
            {
                var workItemProject = workItemStore.Projects[project];
                var workItemType = workItemProject.WorkItemTypes[wiType];

                return workItemType.NewWorkItem();
            }
            catch (ClientException e)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Warning,
                    (int)EventType.ExceptionCaught,
                    "ClientException: {0}\n{1}",
                    e.Message,
                    e.StackTrace);

                EventTracing.TraceEvent(
                    TraceEventType.Warning,
                    (int)EventType.ExceptionCaught,
                    "Failed to create new work item: project='{0}' type='{1}'",
                    project,
                    wiType);
            }

            return null;
        }

        /// <summary>
        ///     Retrieves the list of projects hosted by the TFS.
        /// </summary>
        /// <returns></returns>
        public List<string> GetListOfProjects()
        {
            var projs = new List<string>();
            for (int i = 0; i < workItemStore.Projects.Count; i++ )
            {
                projs.Add(workItemStore.Projects[i].Name);
            }

            EventTracing.TraceInfo(
                "GetListOfProjects: {0}",
                EventTracing.ToString(projs));

            return projs;
        }

        /// <summary>
        ///     Retrieves the list of work item types supported by all projects.
        /// </summary>
        /// <returns></returns>
        public List<string> GetListOfAllWorkItemTypes()
        {
            var types = new List<string>();

            for (int i = 0; i < workItemStore.Projects.Count; i++)
            {
                var wiTypes = workItemStore.Projects[i].WorkItemTypes;

                for (int j = 0; j < wiTypes.Count; j++)
                {
                    if (types.IndexOf(wiTypes[j].Name) < 0)
                    {
                        types.Add(wiTypes[j].Name);
                    }
                }
            }

            return types;
        }

        /// <summary>
        ///     Retrieves the list of work item types supported by the given project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public List<string> GetListOfWorkItemTypes(
            string project)
        {
            var types = new List<string>();
            var wiTypes = workItemStore.Projects[project].WorkItemTypes;
            for (int i = 0; i < wiTypes.Count; i++)
            {
                types.Add(wiTypes[i].Name);
            }

            return types;
        }

        #region Private methods

        /// <summary>
        ///     Retrieves the properties / fields in a work item and stores in a dictionary.
        /// </summary>
        /// <param name="workItem">Work item</param>
        /// <returns></returns>
        private static Dictionary<string, object> WorkItemProperties(
            WorkItem workItem,
            bool includeFields=true)
        {
            var result = new Dictionary<string, object>();

            result[PublishedData.AreaPath] = workItem.AreaPath;
            result[PublishedData.ChangedBy] = workItem.ChangedBy;
            result[PublishedData.ChangedDate] = workItem.ChangedDate;
            result[PublishedData.CreatedBy] = workItem.CreatedBy;
            result[PublishedData.CreatedDate] = workItem.CreatedDate;
            result[PublishedData.Description] = workItem.Description;

            var sb = new StringBuilder();
            foreach (Attachment attachment in workItem.Attachments)
            {
                sb.AppendLine(attachment.Name);
            }

            result[PublishedData.Attachment] = sb.ToString();

            if (includeFields)
            {
                result[PublishedData.NumberOfFields] = workItem.Fields.Count;

                // This won't be directly published, so the key doesn't go into PublishedData
                result[fieldDict] = new List<Dictionary<string, string>>();

                for (int i = 0; i < workItem.Fields.Count; i++)
                {
                    if (workItem.Fields[i].OriginalValue == null)
                    {
                        continue;
                    }
                        
                    var pair = new Dictionary<string, string>();
                    pair[PublishedData.FieldName] = workItem.Fields[i].Name;
                    pair[PublishedData.FieldValue] = workItem.Fields[i].OriginalValue.ToString();
                    
                    ((List<Dictionary<string, string>>)result[fieldDict]).Add(pair);
                }

                result[PublishedData.ChangedFields] = GetChangedFields(workItem);
                result[PublishedData.ChangedFieldDetails] = GetChangedFields(workItem, true);
            }

            result[PublishedData.History] = GetWorkItemHistory(workItem);
            result[PublishedData.Id] = workItem.Id;
            result[PublishedData.IterationPath] = workItem.IterationPath;
            result[PublishedData.Reason] = workItem.Reason;
            result[PublishedData.State] = workItem.State;
            result[PublishedData.Title] = workItem.Title;
            result[PublishedData.WorkItemType] = workItem.Type.Name;

            return result;
        }

        private static string GetWorkItemHistory(
            WorkItem workItem)
        {
            var sb = new StringBuilder();

            foreach (Revision rev in workItem.Revisions)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", rev.Index, rev.GetTagLine());
                sb.AppendLine();
                sb.AppendLine(rev.Fields[CoreField.History].OriginalValue.ToString());
            }

            sb.AppendLine(workItem.Fields[CoreField.History].OriginalValue.ToString());

            return sb.ToString();
        }

        private static string GetChangedFields(
            WorkItem workItem,
            bool withValues=false)
        {
            if (workItem.Revisions.Count < 1)
                return string.Empty;

            var sb = new StringBuilder();

            var lastFields = workItem.Revisions[workItem.Revisions.Count - 1].Fields;
            foreach (Field f in lastFields)
            {
                string curVal = f.Value == null ? "" : f.Value.ToString();
                string oriVal = f.OriginalValue == null ? "" : f.OriginalValue.ToString();

                if (f.Value != null && curVal != oriVal && !(f.Value is DateTime))
                {
                    if (!withValues)
                    {
                        sb.AppendLine(f.Name);
                    }
                    else
                    {
                        sb.AppendFormat(
                            CultureInfo.InvariantCulture,
                            "{0} : {1} => {2}",
                            f.Name,
                            oriVal,
                            curVal);
                        sb.AppendLine();
                    }
                }
            }

            return sb.ToString();
        }

        public static string GetCustomWorkItemProperties(
            Dictionary<string, object> properties,
            string key)
        {
            if (!properties.ContainsKey(fieldDict))
            {
                return string.Empty;
            }

            var fields = properties[fieldDict] as List<Dictionary<string, string>>;
            Debug.Assert(fields != null);

            var dict = fields.Find(item => item[PublishedData.FieldName] == key);
            if (dict == null)
            {
                return string.Empty;
            }

            return dict[PublishedData.FieldValue];
        }

        #endregion
    }
}
