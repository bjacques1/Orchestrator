// <copyright file="StartWorkflow.cs" company="Microsoft">
//      Copyright (C) 2012 Microsoft Corporation.  All rights reserved.
// </copyright>

namespace WindowsWorkflowIntegrationPack
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.SystemCenter.Orchestrator.Integration;

    /// <summary>
    ///     Start workflow activity
    /// </summary>
    [Activity("Start Workflow", Description = "Starts a Windows Workflow")]
    public class StartWorkflow : IActivity
    {
        /// <summary>
        ///     Gets or sets the workflow settings
        /// </summary>
        [ActivityConfiguration]
        public WorkflowSettings Settings { get; set; }
        
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

            designer.AddInput(PublishedData.WorkflowFileName).WithFileBrowser();
            designer.AddInput(PublishedData.WorkflowClassName);
            designer.AddInput(PublishedData.DatabaseConnection)
                .WithDefaultValue(@"Integrated Security=SSPI;Data Source=.\SQLEXPRESS;Initial Catalog=WF4Persistence");

            for (int i = 1; i <= this.Settings.MaxNumberOfParameters; i++)
            {
                designer.AddInput(PublishedData.InputParameter + i.ToString(CultureInfo.InvariantCulture)).NotRequired();
                designer.AddInput(PublishedData.InputValue + i.ToString(CultureInfo.InvariantCulture)).NotRequired();
            }

            for (int i = 1; i <= this.Settings.MaxNumberOfExtensions; i++)
            {
                designer.AddInput(PublishedData.ExtensionAssemblyName + i.ToString(CultureInfo.InvariantCulture))
                    .NotRequired()
                    .WithFileBrowser();
                designer.AddInput(PublishedData.ExtensionClassName + i.ToString(CultureInfo.InvariantCulture))
                    .NotRequired();
            }

            designer.AddOutput(PublishedData.WorkflowId).AsString().WithDescription(Guid.NewGuid().ToString("B"));
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

            // Loads the workflow XAML file
            var workflowFileName = request.Inputs[PublishedData.WorkflowFileName].AsString();
            var workflowClassName = request.Inputs[PublishedData.WorkflowClassName].AsString();

            // Collects all the input parameters
            var inputs = new Dictionary<string, object>();

            for (int i = 0; i < this.Settings.MaxNumberOfParameters; i++)
            {
                string parameter = PublishedData.InputParameter + i.ToString(CultureInfo.InvariantCulture);
                string value = PublishedData.InputValue + i.ToString(CultureInfo.InvariantCulture);

                if (request.Inputs.Contains(parameter))
                {
                    parameter = request.Inputs[parameter].AsString();

                    if (request.Inputs.Contains(value))
                    {
                        value = request.Inputs[value].AsString();
                    }
                    else
                    {
                        value = string.Empty;
                    }

                    inputs[parameter] = value;
                }
            }

            var connectionString = request.Inputs[PublishedData.DatabaseConnection].AsString();

            var extensions = new Dictionary<string, string>();
            for (int i = 0; i < this.Settings.MaxNumberOfExtensions; i++)
            {
                string assemblyName = PublishedData.ExtensionAssemblyName + i.ToString(CultureInfo.InvariantCulture);
                string className = PublishedData.ExtensionClassName + i.ToString(CultureInfo.InvariantCulture);

                if (request.Inputs.Contains(assemblyName) && request.Inputs.Contains(className))
                {
                    assemblyName = request.Inputs[assemblyName].AsString();
                    className = request.Inputs[className].AsString();

                    extensions[assemblyName] = className;
                }
            }

            var workflowApp = WorkflowHelper.CreateWorkflowApplication(
                workflowFileName,
                workflowClassName,
                inputs,
                connectionString,
                extensions,
                true);

            workflowApp.Run();

            WorkflowStateStore.AddWorkflow(workflowApp.Id, workflowApp);
            
            response.Publish(PublishedData.WorkflowId, workflowApp.Id.ToString("B"));
        }
    }
}
