using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    /// <summary>
    ///     Activity: create a new work item.
    /// </summary>
    [Activity("New Work Item", Description = "Create a new work item")]
    public class NewWorkItem : IActivity
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

            var wiHelper = new WorkItemHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);
            var allProjects = wiHelper.GetListOfProjects();
            var allWorkItemTypes = wiHelper.GetListOfAllWorkItemTypes();

            designer.AddInput(PublishedData.Project).WithListBrowser(allProjects);
            designer.AddInput(PublishedData.WorkItemType).WithListBrowser(allWorkItemTypes);
            designer.AddInput(PublishedData.Title);
            // State == Active
            designer.AddInput(PublishedData.AssignedTo).NotRequired();
            designer.AddInput(PublishedData.AreaPath).NotRequired();
            designer.AddInput(PublishedData.Description).NotRequired();
            designer.AddInput(PublishedData.History).NotRequired();
            designer.AddInput(PublishedData.IterationPath).NotRequired();

            designer.AddInput(PublishedData.AddAttachment).WithFileBrowser().NotRequired();

            // TODO:
            // publish Project and AssignedTo

            for (int i = 0; i < NumberOfCustomFields; i++)
            {
                var key = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldName, i);
                var val = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldValue, i);

                designer.AddInput(key).NotRequired();
                designer.AddInput(val).NotRequired();

                designer.AddOutput(key).AsString();
                designer.AddOutput(val).AsString();
            }

            designer.AddOutput(PublishedData.FieldName).AsString().WithFilter();
            designer.AddOutput(PublishedData.FieldValue).AsString().WithFilter();

            foreach (KeyValuePair<string, string> kv in PublishedData.NewWorkItem)
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

            var wiHelper = new WorkItemHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);
            
            var workItem = wiHelper.CreateWorkItem(
                request.Inputs[PublishedData.Project].AsString(),
                request.Inputs[PublishedData.WorkItemType].AsString());

            if (workItem == null)
            {
                throw new ActivityExecutionException(AppResource.FailedToCreateWorkItem);
            }

            workItem.Title = request.Inputs[PublishedData.Title].AsString();

            string wiAssignedTo = request.Inputs[PublishedData.AssignedTo].AsString();
            if (!string.IsNullOrEmpty(wiAssignedTo))
            {
                workItem.Fields["Assigned To"].Value = wiAssignedTo;
            }

            string wiArePath = request.Inputs[PublishedData.AreaPath].AsString();
            if (!string.IsNullOrEmpty(wiArePath))
            {
                workItem.AreaPath = wiArePath;
            }

            var wiDescription = request.Inputs[PublishedData.Description].AsString();
            if (!string.IsNullOrEmpty(wiDescription))
            {
                workItem.Description = wiDescription;
            }

            var wiItertionPath = request.Inputs[PublishedData.IterationPath].AsString();
            if (!string.IsNullOrEmpty(wiItertionPath))
            {
                workItem.IterationPath = wiItertionPath;
            }

            var wiAttachment = request.Inputs[PublishedData.AddAttachment].AsString();
            if (!string.IsNullOrEmpty(wiAttachment))
            {
                workItem.Attachments.Add(new Attachment(wiAttachment));
            }

            for (int i = 0; i < NumberOfCustomFields; i++)
            {
                var key = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldName, i);
                var val = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldValue, i);

                var keyInput = request.Inputs[key].AsString();
                var valInput = request.Inputs[val].AsString();

                if (string.IsNullOrEmpty(keyInput) || string.IsNullOrEmpty(valInput))
                    continue;

                if (workItem.Fields.Contains(keyInput))
                {
                    workItem.Fields[keyInput].Value = valInput;
                    response.Publish(key, keyInput);
                    response.Publish(val, valInput);
                }
                else
                {
                    throw new ActivityExecutionException(string.Format(
                        CultureInfo.CurrentCulture,
                        AppResource.FieldNotFound,
                        key, keyInput));
                }
            }

            // Validate the rules.  If it's valid, save it, otherwise show the error.
            if (workItem.IsValid())
            {
                workItem.Save();
            }
            else
            {
                var invalidFields = workItem.Validate();

                var sb = new StringBuilder(AppResource.InvalidFieldsList);

                foreach (var field in invalidFields)
                {
                    var wi = field as Field;
                    if (wi != null)
                    {
                        string val = wi.Value == null ? "(null)" : wi.Value.ToString();
                        sb.AppendFormat("'{0}'='{1}', ", wi.Name, val);
                    }
                    else
                    {
                        EventTracing.TraceEvent(TraceEventType.Verbose, 0, field.ToString(), null);
                    }
                }

                EventTracing.TraceEvent(
                    TraceEventType.Warning,
                    (int)EventType.FoundInvalidFields,
                    sb.ToString(),
                    null);

                throw new ActivityExecutionException(sb.ToString());
            }

            var props = wiHelper.QueryByID(workItem.Id);
            if (props == null)
            {
                throw new ActivityExecutionException(AppResource.WorkItemNotFound);
            }

            foreach (KeyValuePair<string, object> kv in props)
            {
                var values = props[kv.Key] as List<Dictionary<string, string>>;
                if (values == null)
                {
                    response.Publish(kv.Key, kv.Value);
                }
                else
                {
                    foreach (Dictionary<string, string> val in values.OrderBy(e => e[PublishedData.FieldName]))
                    {
                        response.WithFiltering().Publish(val);
                    }
                }
            }
        }
    }
}

