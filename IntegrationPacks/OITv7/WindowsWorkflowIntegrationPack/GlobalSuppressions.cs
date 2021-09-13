// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Design",
    "CA1014:MarkAssembliesWithClsCompliant",
    Justification = "OIT is not CLS compliant")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Design", 
    "CA2210:AssembliesShouldHaveValidStrongNames",
    Justification = "Free code no strong name no GAC")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Reliability",
    "CA2001:AvoidCallingProblematicMethods",
    MessageId = "System.Reflection.Assembly.LoadFrom",
    Scope = "member",
    Target = "WindowsWorkflowIntegrationPack.WorkflowHelper.#SetWorkflowExtension(System.Activities.WorkflowApplication,System.String,System.String)",
    Justification = "Designed to load file from specified location")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Reliability",
    "CA2001:AvoidCallingProblematicMethods",
    MessageId = "System.Reflection.Assembly.LoadFrom",
    Scope = "member",
    Target = "WindowsWorkflowIntegrationPack.WorkflowHelper.#LoadWorkflowFromClass(System.String,System.String)",
    Justification = "Designed to load file from specified location")]
