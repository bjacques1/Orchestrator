// <copyright file="WorkflowSettings.cs" company="Microsoft">
//      Copyright (C) 2012 Microsoft Corporation.  All rights reserved.
// </copyright>

namespace WindowsWorkflowIntegrationPack
{
    using Microsoft.SystemCenter.Orchestrator.Integration;

    /// <summary>
    ///     Settings for starting windows workflow
    /// </summary>
    [ActivityData("Windows Workflow Settings")]
    public class WorkflowSettings
    {
        /// <summary>
        ///     Gets or sets Maximum number of the input parameters
        /// </summary>
        [ActivityInput("Maximum number of input parameters", Default = 10)]
        public int MaxNumberOfParameters { get; set; }

        /// <summary>
        ///     Gets or sets maximum number of the workflow extension
        /// </summary>
        [ActivityInput("Maximum number of workflow extensions", Default = 3)]
        public int MaxNumberOfExtensions { get; set; }
    }
}
