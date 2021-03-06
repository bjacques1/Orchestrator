//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.261
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Orchestrator2012.Workflow {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class AppResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Orchestrator2012.Workflow.AppResource", typeof(AppResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The directory where all binaries are stored (if empty the activity will use DropLocation).
        /// </summary>
        internal static string BinariesDirectoryDesc {
            get {
                return ResourceManager.GetString("BinariesDirectoryDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Both DropLocation and BinariesDirectory are empty, no binaries can be located..
        /// </summary>
        internal static string BothDropLocationAndBinariesDirectoryAreEmpty {
            get {
                return ResourceManager.GetString("BothDropLocationAndBinariesDirectoryAreEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The configuration/flavor of the build, &quot;Debug&quot; or &quot;Release&quot;.  It could be empty..
        /// </summary>
        internal static string BuildConfigurationDesc {
            get {
                return ResourceManager.GetString("BuildConfigurationDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Extra parameter to be passed to the runbook.
        /// </summary>
        internal static string CustomParameterDesc {
            get {
                return ResourceManager.GetString("CustomParameterDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Orchestrator web service at {0} is used to get the status of job {1}.
        /// </summary>
        internal static string GetJobStatusById {
            get {
                return ResourceManager.GetString("GetJobStatusById", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instance {0}, status = {1}.
        /// </summary>
        internal static string InstanceMsg {
            get {
                return ResourceManager.GetString("InstanceMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Instance parameter {0} : {1}.
        /// </summary>
        internal static string InstanceParameter {
            get {
                return ResourceManager.GetString("InstanceParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Job ID: {0}.
        /// </summary>
        internal static string JobId {
            get {
                return ResourceManager.GetString("JobId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Job ID returned by StartRunbook activity.
        /// </summary>
        internal static string JobIdDesc {
            get {
                return ResourceManager.GetString("JobIdDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Job {0} is not found.
        /// </summary>
        internal static string JobNotFound {
            get {
                return ResourceManager.GetString("JobNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Job parameters: {0}.
        /// </summary>
        internal static string JobParameter {
            get {
                return ResourceManager.GetString("JobParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Job status == Completed.
        /// </summary>
        internal static string JobStatusCompleted {
            get {
                return ResourceManager.GetString("JobStatusCompleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Runbook does not define input parameter &apos;{0}&apos;.
        /// </summary>
        internal static string NoInputParameter {
            get {
                return ResourceManager.GetString("NoInputParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No &apos;TestOutcome&apos; parameter is found.  Set to Unknown.
        /// </summary>
        internal static string NoTestOutcomeParameter {
            get {
                return ResourceManager.GetString("NoTestOutcomeParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The runbook does not have &apos;TestOutcome&apos; (case sensitive) as the return data.
        /// </summary>
        internal static string NoTestOutcomeReturn {
            get {
                return ResourceManager.GetString("NoTestOutcomeReturn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to URL for Orchestrator web service, e.g. &quot;http://host:81/Orchestrator2012/Orchestrator.svc&quot;.
        /// </summary>
        internal static string OrchestratorUrlDesc {
            get {
                return ResourceManager.GetString("OrchestratorUrlDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parameter {0} set to empty.
        /// </summary>
        internal static string ParameterSetToEmpty {
            get {
                return ResourceManager.GetString("ParameterSetToEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Posting job to the web service.
        /// </summary>
        internal static string PostingJob {
            get {
                return ResourceManager.GetString("PostingJob", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Runbook input parameter: {0}, ID={1}, Type={2}.
        /// </summary>
        internal static string RunbookInputParameter {
            get {
                return ResourceManager.GetString("RunbookInputParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Gets the Runbook job status and retrieves return data if it is completed.
        /// </summary>
        internal static string RunbookJobStatusDesc {
            get {
                return ResourceManager.GetString("RunbookJobStatusDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Runbook {0} is not found, current user &apos;{1}&apos;.
        /// </summary>
        internal static string RunbookNotFound {
            get {
                return ResourceManager.GetString("RunbookNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Full path of the runbook to be invoked.
        /// </summary>
        internal static string RunbookPathDesc {
            get {
                return ResourceManager.GetString("RunbookPathDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dictionary of Runbook return data.
        /// </summary>
        internal static string RunbookReturnDataDesc {
            get {
                return ResourceManager.GetString("RunbookReturnDataDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Orchestrator web service at {0} is called to start the runbook {1}.
        ///  Drop Location: {2}  Build Number: {3}  Custom Parameter: {4}.
        /// </summary>
        internal static string StartRunbook {
            get {
                return ResourceManager.GetString("StartRunbook", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starts an Orchestrator 2012 Runbook via web service and gets the job GUID.
        /// </summary>
        internal static string StartRunbookDesc {
            get {
                return ResourceManager.GetString("StartRunbookDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test outcome returned by the Runbook.
        /// </summary>
        internal static string TestOutcomeDesc {
            get {
                return ResourceManager.GetString("TestOutcomeDesc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type of &apos;TestOutcome&apos; return data of the runbook should be String.
        /// </summary>
        internal static string TestOutcomeNotString {
            get {
                return ResourceManager.GetString("TestOutcomeNotString", resourceCulture);
            }
        }
    }
}
