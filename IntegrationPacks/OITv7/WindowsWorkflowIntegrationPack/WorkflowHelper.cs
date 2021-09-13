// <copyright file="WorkflowHelper.cs" company="Microsoft">
//      Copyright (C) 2012 Microsoft Corporation.  All rights reserved.
// </copyright>

namespace WindowsWorkflowIntegrationPack
{
    using System;
    using System.Activities;
    using System.Activities.DurableInstancing;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Static helper class for workflow execution
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.SpacingRules", "SA1025:CodeMustNotContainMultipleWhitespaceInARow", Justification = "Align the variables")]
    internal static class WorkflowHelper
    {
        /// <summary>
        ///     Creates a WorkflowApplication object to load the given XAML file and prepares for running/loading
        /// </summary>
        /// <param name="workflowFileName">File name of the XAML workflow or the DLL</param>
        /// <param name="workflowClassName">Class name of the workflow, if file name points to DLL</param>
        /// <param name="inputParameters">Input parameters</param>
        /// <param name="sqlConnectionString">SQL connection string for persistence</param>
        /// <param name="extensionAssembliesAndClasses">List of extensions</param>
        /// <param name="newInstance">True if running the workflow first time, False if restarting from instance store</param>
        /// <returns>WorkflowApplication object</returns>
        internal static WorkflowApplication CreateWorkflowApplication(
            string                      workflowFileName,
            string                      workflowClassName,
            Dictionary<string, object>  inputParameters,
            string                      sqlConnectionString,
            Dictionary<string, string>  extensionAssembliesAndClasses,
            bool                        newInstance)
        {
            // Confirms the file does exist
            if (!File.Exists(workflowFileName))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentUICulture,
                    AppResources.WorkflowFileNotFound,
                    workflowFileName));
            }

            Activity workflow;

            if (workflowFileName.ToUpperInvariant().EndsWith(".XAML", StringComparison.OrdinalIgnoreCase))
            {
                workflow = ActivityXamlServices.Load(workflowFileName);
            }
            else
            {
                // Loads the DLL and constructs the class
                workflow = LoadWorkflowFromClass(workflowFileName, workflowClassName);
            }

            WorkflowApplication workflowApp;

            if (newInstance)
            {
                workflowApp = new WorkflowApplication(workflow, inputParameters);
            }
            else
            {
                // Restarts from instance store, ignores the input parameters
                workflowApp = new WorkflowApplication(workflow);
            }

            if (string.IsNullOrWhiteSpace(sqlConnectionString))
            {
                workflowApp.PersistableIdle = delegate(WorkflowApplicationIdleEventArgs e)
                {
                    WorkflowStateStore.Update(e.InstanceId, "PersistableIdle", null);

                    // No SQL database connection, cannot persist
                    return PersistableIdleAction.None;
                };
            }
            else
            {
                // New instance always sets the instance owner
                SetupSqlInstanceStore(workflowApp, sqlConnectionString, newInstance);

                workflowApp.PersistableIdle = delegate(WorkflowApplicationIdleEventArgs e)
                {
                    WorkflowStateStore.Update(e.InstanceId, "PersistableIdle", null);

                    // Persist only, do not unload
                    return PersistableIdleAction.Persist;
                };
            }

            SetWorkflowDelegates(workflowApp);

            foreach (var kv in extensionAssembliesAndClasses)
            {
                SetWorkflowExtension(workflowApp, kv.Key, kv.Value);
            }

            return workflowApp;
        }

        /// <summary>
        ///     Loads a workflow from an assembly
        /// </summary>
        /// <param name="workflowFileName">File name of the assembly</param>
        /// <param name="workflowClassName">Class name, or the ending part</param>
        /// <returns>Activity being loaded</returns>
        private static Activity LoadWorkflowFromClass(string workflowFileName, string workflowClassName)
        {
            var assembly = Assembly.LoadFrom(workflowFileName);
            var classType = assembly.GetTypes().FirstOrDefault(
                type => type.FullName.EndsWith(workflowClassName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (classType == null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentUICulture,
                    AppResources.ClassNotFoundInAssembly,
                    workflowClassName,
                    workflowFileName));
            }

            var activity = classType.InvokeMember(
                ".ctor",
                BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public,
                null,
                null,
                new object[] { },
                CultureInfo.InvariantCulture) as Activity;

            return activity;
        }

        /// <summary>
        ///     Setup the SQL instance store for persistence
        /// </summary>
        /// <param name="workflowApp">WorkflowApplication object</param>
        /// <param name="sqlConnectionString">SQL connection string</param>
        /// <param name="setInstanceOwner">Whether to set DB instance owner</param>
        private static void SetupSqlInstanceStore(
            WorkflowApplication workflowApp,
            string              sqlConnectionString,
            bool                setInstanceOwner)
        {
            var store = new SqlWorkflowInstanceStore(sqlConnectionString);

            if (setInstanceOwner)
            {
                store.InstanceCompletionAction = InstanceCompletionAction.DeleteAll;

                var handle = store.CreateInstanceHandle();
                var view = store.Execute(handle, new CreateWorkflowOwnerCommand(), TimeSpan.FromSeconds(60));

                handle.Free();

                store.DefaultInstanceOwner = view.InstanceOwner;
            }

            workflowApp.InstanceStore = store;
        }

        /// <summary>
        ///     Sets the workflow delegates to store the states during the execution
        /// </summary>
        /// <param name="workflowApp">WorkflowApplication instance</param>
        private static void SetWorkflowDelegates(WorkflowApplication workflowApp)
        {
            workflowApp.Completed = delegate(WorkflowApplicationCompletedEventArgs e)
            {
                if (e.CompletionState == ActivityInstanceState.Faulted)
                {
                    var message = string.Format(
                        CultureInfo.CurrentUICulture,
                        AppResources.WorkflowTerminated,
                        e.InstanceId,
                        e.TerminationException.GetType().FullName,
                        e.TerminationException.Message);

                    WorkflowStateStore.Update(
                        e.InstanceId,
                        e.CompletionState.ToString(),
                        message);
                }
                else if (e.CompletionState == ActivityInstanceState.Canceled)
                {
                    WorkflowStateStore.Update(
                        e.InstanceId,
                        e.CompletionState.ToString(),
                        null);
                }
                else
                {
                    WorkflowStateStore.Update(
                        e.InstanceId,
                        e.CompletionState.ToString(),
                        null);

                    foreach (var kv in e.Outputs)
                    {
                        WorkflowStateStore.AddResult(e.InstanceId, kv.Key, kv.Value);
                    }
                }

                WorkflowStateStore.SetWorkflowFinished(e.InstanceId);
            };

            workflowApp.Aborted = delegate(WorkflowApplicationAbortedEventArgs e)
            {
                // Display the exception that caused the workflow to abort.
                var message = string.Format(
                    CultureInfo.CurrentUICulture,
                    AppResources.WorkflowTerminated,
                    e.InstanceId,
                    e.Reason.GetType().FullName,
                    e.Reason.Message);

                WorkflowStateStore.Update(
                    e.InstanceId,
                    "Aborted",
                    message);

                WorkflowStateStore.SetWorkflowFinished(e.InstanceId);
            };

            workflowApp.Idle = delegate(WorkflowApplicationIdleEventArgs e)
            {
                // Perform any processing that should occur
                // when a workflow goes idle. If the workflow can persist,
                // both Idle and PersistableIdle are called in that order.
                WorkflowStateStore.Update(
                    e.InstanceId,
                    "Idle",
                    null);
            };

            workflowApp.Unloaded = delegate(WorkflowApplicationEventArgs e)
            {
                WorkflowStateStore.Update(
                    e.InstanceId,
                    "Unloaded",
                    null);
            };

            workflowApp.OnUnhandledException = delegate(WorkflowApplicationUnhandledExceptionEventArgs e)
            {
                var message = string.Format(
                    CultureInfo.CurrentUICulture,
                    AppResources.WorkflowUnhandledException,
                    e.UnhandledException.Message,
                    e.ExceptionSource.DisplayName,
                    e.ExceptionSourceInstanceId);

                WorkflowStateStore.Update(
                    e.InstanceId,
                    "UnhandledException",
                    message);

                // Instruct the runtime to terminate the workflow.
                // Other choices are Abort and Cancel
                return UnhandledExceptionAction.Terminate;
            };
        }

        /// <summary>
        ///     Adds the specified extension to the WorkflowApplication instance
        /// </summary>
        /// <param name="workflowApp">WorkflowApplication instance</param>
        /// <param name="assemblyName">File name of the extension assembly</param>
        /// <param name="className">Name of the class</param>
        private static void SetWorkflowExtension(WorkflowApplication workflowApp, string assemblyName, string className)
        {
            assemblyName = assemblyName.Trim();

            if (!File.Exists(assemblyName))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentUICulture,
                    AppResources.AssemblyFileNotFound,
                    assemblyName));
            }

            var assembly = Assembly.LoadFrom(assemblyName);
            var classType = assembly.GetTypes().FirstOrDefault(
                type => type.FullName.EndsWith(className.Trim(), StringComparison.OrdinalIgnoreCase));

            if (classType == null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentUICulture,
                    AppResources.ClassNotFoundInAssembly,
                    className,
                    assemblyName));
            }

            object extentionObject;

            // Checks if there is a Create static method
            var createMethodInfo = classType.GetMethod(
                "Create",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod,
                null,
                Type.EmptyTypes,
                null);

            if (createMethodInfo != null)
            {
                // Constructs using Create method
                extentionObject = createMethodInfo.Invoke(null, null);
            }
            else
            {
                // Constructs an new instance
                extentionObject = classType.InvokeMember(
                    ".ctor",
                    BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.Public,
                    null,
                    null,
                    new object[] { },
                    CultureInfo.InvariantCulture);
            }

            workflowApp.Extensions.Add(extentionObject);
        }
    }
}
