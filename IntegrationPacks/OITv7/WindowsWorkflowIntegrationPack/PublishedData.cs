// <copyright file="PublishedData.cs" company="Microsoft">
//      Copyright (C) 2012 Microsoft Corporation.  All rights reserved.
// </copyright>

namespace WindowsWorkflowIntegrationPack
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Input and output strings.
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Published data")]
    internal static class PublishedData
    {
        internal const string DatabaseConnection = "SQL Database Connection";

        internal const string ExtensionAssemblyName = "Extension Assembly File Name ";
        internal const string ExtensionClassName = "Extension Class Name ";

        internal const string InputParameter = "Input Parameter ";
        internal const string InputValue = "Input Value ";

        internal const string Reason = "Reason";
        internal const string ResultName = "Result Name";
        internal const string ResultValue = "Result Value";

        internal const string TimeoutInMinutes = "Timeout (minutes)";

        internal const string WaitUntilFinished = "Wait Until Finished";
        internal const string WorkflowId = "Workflow ID";
        internal const string WorkflowMessage = "Workflow Message";
        internal const string WorkflowStatus = "Workflow Status";
        internal const string WorkflowFileName = "Workflow File Name";
        internal const string WorkflowClassName = "Workflow Class Name";
    }
}
