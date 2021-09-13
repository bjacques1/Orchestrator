// <copyright file="TerminateWorkflow.cs" company="Microsoft">
//      Copyright (C) 2012 Microsoft Corporation.  All rights reserved.
// </copyright>

namespace WindowsWorkflowIntegrationPack
{
    using System;
    using System.Activities;
    using System.Globalization;
    using Microsoft.SystemCenter.Orchestrator.Integration;

    /// <summary>
    ///     Terminate Workflow activity
    /// </summary>
    [Activity("Terminate Workflow", Description = "Terminates a workflow execution")]
    public class TerminateWorkflow : IActivity
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
            designer.AddInput(PublishedData.Reason);
            designer.AddInput(PublishedData.TimeoutInMinutes).WithDefaultValue(1);
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

            var workflowApp = WorkflowStateStore.GetHostingHandle(id) as WorkflowApplication;
            if (workflowApp == null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentUICulture,
                    AppResources.WorkflowNotFound,
                    id));
            }

            var reason = request.Inputs[PublishedData.Reason].AsString();
            var timeout = request.Inputs[PublishedData.TimeoutInMinutes].AsInt32();

            workflowApp.Terminate(reason, TimeSpan.FromMinutes(timeout));
        }
    }
}
