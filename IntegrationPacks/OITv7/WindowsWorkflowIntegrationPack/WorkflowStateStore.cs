// -----------------------------------------------------------------------
// <copyright file="WorkflowStateStore.cs" company="Microsoft">
//      Copyright (C) 2012 Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace WindowsWorkflowIntegrationPack
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    /// <summary>
    ///     Store the Windows workflow state in the process level
    /// </summary>
    internal static class WorkflowStateStore
    {
        /// <summary>
        ///     Stores the states of all workflows in the current process
        /// </summary>
        private static Dictionary<Guid, WorkflowState> workflows = new Dictionary<Guid, WorkflowState>();

        /// <summary>
        ///     Adds a new workflow
        /// </summary>
        /// <param name="id">Instance ID of the workflow</param>
        /// <param name="hostingHandle">WorkflowApplication object</param>
        public static void AddWorkflow(Guid id, object hostingHandle)
        {
            lock (workflows)
            {
                workflows[id] = new WorkflowState() { HostingHandle = hostingHandle };
            }
        }

        /// <summary>
        ///     Updates the status and the message of the workflow
        /// </summary>
        /// <param name="id">Instance ID of the workflow</param>
        /// <param name="status">Current status of the workflow</param>
        /// <param name="message">Message to be appended</param>
        public static void Update(Guid id, string status, string message)
        {
            if (!workflows.ContainsKey(id))
            {
                return;
            }

            var wf = workflows[id];

            lock (wf)
            {
                if (!string.IsNullOrWhiteSpace(status))
                {
                    wf.Status = status;
                }

                if (!string.IsNullOrWhiteSpace(message))
                {
                    wf.Message.AppendLine(message);
                }
            }
        }

        /// <summary>
        ///     Adds a return results of the workflow
        /// </summary>
        /// <param name="id">Instance ID of the workflow</param>
        /// <param name="key">Key/name of the result</param>
        /// <param name="val">Value of the result</param>
        public static void AddResult(Guid id, string key, object val)
        {
            if (!workflows.ContainsKey(id))
            {
                return;
            }

            var wf = workflows[id];

            lock (wf)
            {
                wf.Results[key] = val;
            }
        }

        /// <summary>
        ///     Gets the status of the workflow
        /// </summary>
        /// <param name="id">Instance ID of the workflow</param>
        /// <returns>Status of the workflow, empty string if the workflow is not found</returns>
        public static string GetStatus(Guid id)
        {
            if (!workflows.ContainsKey(id))
            {
                return string.Empty;
            }

            return workflows[id].Status;
        }

        /// <summary>
        ///     Gets the accumulated message of the workflow execution
        /// </summary>
        /// <param name="id">Instance ID of the workflow</param>
        /// <returns>Message of the workflow execution, empty string if the workflow is not found</returns>
        public static string GetMessage(Guid id)
        {
            if (!workflows.ContainsKey(id))
            {
                return string.Empty;
            }

            return workflows[id].Message.ToString();
        }

        /// <summary>
        ///     Gets the results of the workflow execution after it's completed
        /// </summary>
        /// <param name="id">Instance ID of the workflow</param>
        /// <returns>Results in key/value pairs</returns>
        public static Dictionary<string, object> GetResults(Guid id)
        {
            if (!workflows.ContainsKey(id))
            {
                return null;
            }

            return workflows[id].Results;
        }

        /// <summary>
        ///     Gets the hosting handle of a workflow
        /// </summary>
        /// <param name="id">Instance ID of the workflow</param>
        /// <returns>WorkflowApplicaiton object</returns>
        public static object GetHostingHandle(Guid id)
        {
            if (!workflows.ContainsKey(id))
            {
                return null;
            }

            return workflows[id].HostingHandle;
        }

        /// <summary>
        ///     Sets the workflow to finished in order to unblock the wait thread
        /// </summary>
        /// <param name="id">Instance ID of the workflow</param>
        public static void SetWorkflowFinished(Guid id)
        {
            if (workflows.ContainsKey(id))
            {
                workflows[id].WaitHandle.Set();
            }
        }

        /// <summary>
        ///     Waits until the workflow is finished
        /// </summary>
        /// <param name="id">Instance ID of the workflow</param>
        public static void WaitWorkflow(Guid id)
        {
            if (workflows.ContainsKey(id))
            {
                workflows[id].WaitHandle.WaitOne();
            }
        }

        /// <summary>
        ///     Stores the state and results of a single Windows workflow
        /// </summary>
        private class WorkflowState
        {
            /// <summary>
            ///     Initializes a new instance of the WorkflowState class
            /// </summary>
            public WorkflowState()
            {
                this.Message = new StringBuilder();
                this.Results = new Dictionary<string, object>();
                this.WaitHandle = new AutoResetEvent(false);
            }

            /// <summary>
            ///     Gets or sets the current status of the workflow
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            ///     Gets the accumulative message of the workflow during the execution
            /// </summary>
            public StringBuilder Message { get; private set; }

            /// <summary>
            ///     Gets the results/output of the workflow after the execution
            /// </summary>
            public Dictionary<string, object> Results { get; private set; }

            /// <summary>
            ///     Gets the workflow finished signal
            /// </summary>
            public AutoResetEvent WaitHandle { get; private set; }

            /// <summary>
            ///     Gets or sets the hosting handle, or the WorkflowApplication object
            /// </summary>
            public object HostingHandle { get; set; }
        }
    }
}
