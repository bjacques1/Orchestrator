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
    ///     Set work item activity
    /// </summary>
    [Activity("Set Work Item", Description = "Change the fields of the state of a work item")]
    public class SetWorkItem : IActivity
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
            designer.AddInput(PublishedData.AreaPath).NotRequired();
            designer.AddInput(PublishedData.Description).NotRequired();
            designer.AddInput(PublishedData.AssignedTo).NotRequired();
            designer.AddInput(PublishedData.History).NotRequired();
            designer.AddInput(PublishedData.IterationPath).NotRequired();
            designer.AddInput(PublishedData.State).NotRequired();
            designer.AddInput(PublishedData.Title).NotRequired();

            designer.AddInput(PublishedData.AddAttachment).WithFileBrowser().NotRequired();
            designer.AddInput(PublishedData.RemoveAttachment).NotRequired();

            // TODO:
            // publish project and assigned to

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

            foreach (KeyValuePair<string, string> kv in PublishedData.SetWorkItem)
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

            var wi = wiHelper.GetWorkItem(workItemId);

            var wiAreaPath = request.Inputs[PublishedData.AreaPath].AsString();
            if (!string.IsNullOrEmpty(wiAreaPath))
            {
                wi.AreaPath = wiAreaPath;
            }

            var wiDescription = request.Inputs[PublishedData.Description].AsString();
            if (!string.IsNullOrEmpty(wiDescription))
            {
                wi.Description = wiDescription;
            }

            var wiState = request.Inputs[PublishedData.State].AsString();
            if (!string.IsNullOrEmpty(wiState))
            {
                wi.State = wiState;
            }

            var wiAssignedTo = request.Inputs[PublishedData.AssignedTo].AsString();
            if (!string.IsNullOrEmpty(wiAssignedTo))
            {
                wi.Fields["Assigned To"].Value = wiAssignedTo;
            }

            var wiHistory = request.Inputs[PublishedData.History].AsString();
            if (!string.IsNullOrEmpty(wiHistory))
            {
                wi.History = wiHistory;
            }

            var wiIterationPath = request.Inputs[PublishedData.IterationPath].AsString();
            if (!string.IsNullOrEmpty(wiIterationPath))
            {
                wi.IterationPath = wiIterationPath;
            }

            var wiTitle = request.Inputs[PublishedData.Title].AsString();
            if (!string.IsNullOrEmpty(wiTitle))
            {
                wi.Title = wiTitle;
            }

            var wiAttachment = request.Inputs[PublishedData.AddAttachment].AsString();
            if (!string.IsNullOrEmpty(wiAttachment))
            {
                wi.Attachments.Add(new Attachment(wiAttachment));
            }

            var wiRemoveAttachment = request.Inputs[PublishedData.RemoveAttachment].AsString();
            if (!string.IsNullOrEmpty(wiRemoveAttachment))
            {
                RemoveAttachment(wi, wiRemoveAttachment);
            }

            for (int i = 0; i < NumberOfCustomFields; i++)
            {
                var key = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldName, i);
                var val = string.Format(CultureInfo.InvariantCulture, PublishedData.CustomFieldValue, i);

                var keyInput = request.Inputs[key].AsString();
                var valInput = request.Inputs[val].AsString();

                if (string.IsNullOrEmpty(keyInput) || string.IsNullOrEmpty(valInput))
                    continue;

                if (wi.Fields.Contains(keyInput))
                {
                    wi.Fields[keyInput].Value = valInput;

                    response.Publish(key, keyInput);
                    response.Publish(val, valInput);
                }
                else
                {
                    throw new ActivityExecutionException(string.Format(
                        CultureInfo.CurrentCulture,
                        AppResource.FieldNotFound,
                        key,
                        keyInput));
                }
            }

            // Validate the rules.  If it's valid, save it, otherwise show the error.
            if (wi.IsValid())
            {
                wi.Save();
            }
            else
            {
                var invalidFields = LogInvalidFields(wi);
                throw new ActivityExecutionException(invalidFields);
            }

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

        private static void RemoveAttachment(
            WorkItem wi,
            string wiRemoveAttachment)
        {
            foreach (Attachment attachment in wi.Attachments)
            {
                var match = string.Compare(
                    wiRemoveAttachment,
                    attachment.Name,
                    true,
                    CultureInfo.CurrentCulture);

                if (match == 0)
                {
                    wi.Attachments.Remove(attachment);
                    break;
                }
            }
        }

        private static string LogInvalidFields(
            WorkItem wi)
        {
            var invalidFields = wi.Validate();

            var sb = new StringBuilder(AppResource.InvalidFieldsList);

            foreach (var obj in invalidFields)
            {
                var field = obj as Field;
                if (field != null)
                {
                    string val = field.Value == null ? "(null)" : field.Value.ToString();
                    sb.AppendFormat("'{0}'='{1}', ", field.Name, val);
                }
                else
                {
                    EventTracing.TraceEvent(TraceEventType.Verbose, 0, obj.ToString(), null);
                }
            }

            EventTracing.TraceEvent(
                TraceEventType.Warning,
                (int)EventType.FoundInvalidFields,
                sb.ToString(),
                null);

            return sb.ToString();
        }
    }
}

