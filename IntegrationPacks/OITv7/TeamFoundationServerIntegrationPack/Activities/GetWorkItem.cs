using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    /// <summary>
    ///     Activity: Get Work Item 
    /// </summary>
    [Activity("Get Work Item", Description = "Retrieves detailed information of a work item by ID")]
    public class GetWorkItem : IActivity
    {
        private const int NumberOfCustomFields = 10;

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

            designer.AddInput(PublishedData.WorkItemId);

            designer.AddInput(PublishedData.DownloadAttachmentsFolder).WithFolderBrowser().NotRequired();
            designer.AddOutput(PublishedData.DownloadedFiles).AsString().WithFilter();

            for (int i = 0; i < NumberOfCustomFields; i++)
            {
                var key = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldName, i);
                var val = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldValue, i);

                designer.AddInput(key).NotRequired();

                designer.AddOutput(key).AsString();
                designer.AddOutput(val).AsString();
            }

            designer.AddOutput(PublishedData.FieldName).AsString().WithFilter();
            designer.AddOutput(PublishedData.FieldValue).AsString().WithFilter();

            foreach (KeyValuePair<string, string> kv in PublishedData.GetWorkItem)
            {
                switch (kv.Value)
                {
                    case PublishedData.AsDateTime:
                        designer.AddOutput(kv.Key).AsDateTime();
                        break;

                    case PublishedData.AsNumber:
                        designer.AddOutput(kv.Key).AsNumber();
                        break;

                    case PublishedData.AsString:
                        designer.AddOutput(kv.Key).AsString();
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

            int workItemId = request.Inputs[PublishedData.WorkItemId].AsInt32();

            if (workItemId <= 0)
            {
                throw new ActivityExecutionException(AppResource.WorkItemIdShouldBeGreaterThanZero);
            }

            var wiHelper = new WorkItemHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);
            var props = wiHelper.QueryByID(workItemId);

            if (props == null)
            {
                throw new ActivityExecutionException(AppResource.WorkItemNotFound);
            }

            foreach (KeyValuePair<string, object> kv in props)
            {
                var values = props[kv.Key] as List<Dictionary<string, string>>;
                if (values == null)
                {
                    response.WithFiltering().Publish(kv.Key, kv.Value);
                }
                else
                {
                    foreach (Dictionary<string, string> val in values.OrderBy(e => e[PublishedData.FieldName]))
                    {
                        response.WithFiltering().Publish(val);
                    }
                }
            }

            // Populate the custom fields
            for (int i = 0; i < NumberOfCustomFields; i++)
            {
                var key = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldName, i);
                var val = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldValue, i);

                var keyInput = request.Inputs[key].AsString();

                if (string.IsNullOrEmpty(keyInput))
                    continue;

                string varOutput = WorkItemHelper.GetCustomWorkItemProperties(props, keyInput);

                response.Publish(key, keyInput);
                response.Publish(val, varOutput);
            }

            var downloadAttachmentsFolder = request.Inputs[PublishedData.DownloadAttachmentsFolder].AsString();
            if (!string.IsNullOrEmpty(downloadAttachmentsFolder))
            {
                var workItem = wiHelper.GetWorkItem(workItemId);

                using (var webRequest = new WebClient())
                {
                    if (string.IsNullOrEmpty(ConnectionSettings.UserName))
                    {
                        webRequest.Credentials = CredentialCache.DefaultNetworkCredentials;
                    }
                    else
                    {
                        webRequest.Credentials = new NetworkCredential(
                            ConnectionSettings.UserName,
                            ConnectionSettings.Password,
                            ConnectionSettings.Domain);
                    }

                    foreach (Attachment att in workItem.Attachments)
                    {
                        var fileName = Path.Combine(downloadAttachmentsFolder, att.Name);

                        EventTracing.TraceEvent(
                            TraceEventType.Verbose,
                            (int)EventType.DebugInfo,
                            "Attempt to download '{0}' to '{1}'",
                            att.Uri,
                            fileName);

                        webRequest.DownloadFile(att.Uri, fileName);

                        var dict = new Dictionary<string, string>();
                        dict[PublishedData.DownloadedFiles] = fileName;
                        response.WithFiltering().Publish(dict);
                    }
                }
            }
        }
    }
}

