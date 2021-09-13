// <copyright file="GetWorkflowStatus.cs" company="Microsoft">
//      Copyright (C) 2012 Microsoft Corporation.  All rights reserved.
// </copyright>

namespace WindowsWorkflowIntegrationPack
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SystemCenter.Orchestrator.Integration;

    /// <summary>
    ///     Get Workflow Status activity
    /// </summary>
    [Activity("Get Workflow Status", Description = "Gets the status of a workflow and retrieves results if completed")]
    public class GetWorkflowStatus : IActivity
    {
        /// <summary>
        ///     Adds inputs and outputs at the design time
        /// </summary>
        /// <param name="designer">designer that defines input and output</param>
        public void Design(IActivityDesigner designer)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }

            designer.AddInput(PublishedData.WorkflowId);
            designer.AddInput(PublishedData.WaitUntilFinished).WithDefaultValue(false).WithBooleanBrowser();

            designer.AddOutput(PublishedData.WorkflowStatus);
            designer.AddOutput(PublishedData.WorkflowMessage);
            designer.AddOutput(PublishedData.ResultName).WithFilter();
            designer.AddOutput(PublishedData.ResultValue).WithFilter();
        }

        /// <summary>
        ///     Execution time
        /// </summary>
        /// <param name="request">request that contains the input</param>
        /// <param name="response">response that publishes the data</param>
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

            var id = request.Inputs[PublishedData.WorkflowId].As<Guid>();

            if (request.Inputs[PublishedData.WaitUntilFinished].AsBoolean())
            {
                WorkflowStateStore.WaitWorkflow(id);
            }

            response.Publish(
                PublishedData.WorkflowStatus,
                WorkflowStateStore.GetStatus(id));

            response.Publish(
                PublishedData.WorkflowMessage,
                WorkflowStateStore.GetMessage(id));

            var results = WorkflowStateStore.GetResults(id);

            if (results != null && results.Count > 0)
            {
                foreach (var kv in results)
                {
                    var data = new Dictionary<string, object>
                    {
                        { PublishedData.ResultName, kv.Key },
                        { PublishedData.ResultValue, kv.Value.ToString() },
                    };
                    response.WithFiltering().Publish(data);
                }
            }
        }
    }
}
