using System.Collections.Generic;

namespace TeamFoundationServerIntegrationPack
{
    abstract class PublishedData
    {
        #region Sorted list of const strings

        public const string AddAttachment = "File attachment to be added";
        public const string AreaPath = "Area Path";
        public const string AsDateTime = "AsDateTime";
        public const string AsNumber = "AsNumber";
        public const string AssignedTo = "Assigned To";
        public const string AssociatedChangesets = "Associated Changesets";
        public const string AsString = "AsString";
        public const string Attachment = "Attachment";

        public const string BuildControllerName = "Build Controller Name";
        public const string BuildControllerUri = "Build Controller URI";
        public const string BuildDefinitionName = "Build Definition Name";
        public const string BuildDefinitionUri = "Build Definition URI";
        public const string BuildDirectory = "Build Directory";
        public const string BuildEvent = "Build Event";
        public const string BuildFinished = "Build Finished";
        public const string BuildFlavor = "Build Flavor";
        public const string BuildLabelName = "Build Label Name";
        public const string BuildNumber = "Build Number";
        public const string BuildPlatform = "Build Platform";
        public const string BuildQuality = "Build Quality";
        public const string BuildQueryOrder = "Build Query Order";
        public const string BuildReason = "Reason";
        public const string BuildServerException = "Build Server Exception";
        public const string BuildStatus = "Build Status";
        public const string BuildUri = "Build URI";

        public const string ChangedBy = "Changed By";
        public const string ChangedDate = "Changed Date";
        public const string ChangedFields = "Changed Fields";
        public const string ChangedFieldDetails = "Changed Field Details";
        public const string ChangeType = "Change Type";
        public const string CheckinComment = "Check In Comment";
        public const string CheckinEvent = "Check-in Event";
        public const string CheckinNoteName = "Check-in Note Name";
        public const string CheckinNoteValue = "Check-in Note Value";
        public const string CheckoutAction = "Check Out Action";
        public const string Comment = "Comment";
        public const string Committer = "Committer";
        public const string CompilationStatus = "Compilation Status";
        public const string CompletedTests = "Number of Completed Tests";
        public const string CreatedBy = "Created By";
        public const string CreatedDate = "Created Date";
        public const string CreationDate = "Creation Date";
        public const string CustomFieldName = "Custom Field Name {0}";
        public const string CustomFieldValue = "Custom Field Value {0}";
        
        public const string DateCompleted = "Date Completed";
        public const string DateStarted = "Date Started";
        public const string DayPrecision = "Query in day precision";
        public const string DefaultDropLocation = "Default Drop Location";
        public const string Description = "Description";
        public const string DisplayName = "Display Name";
        public const string DownloadAttachmentsFolder = "Download all attachments at folder";
        public const string DownloadedFiles = "Downloaded file attachments";
        public const string DropLocation = "Drop Location";
        public const string DropLocationRoot = "Drop Location Root";

        public const string Enabled = "Enabled";
        public const string EndDate = "End Date";
        public const string ErrorMessage = "Error Message";

        public const string FailedTests = "Number of Failed Tests";
        public const string FieldName = "Field Name";
        public const string FieldValue = "Field Value";
        public const string FileName = "File Name";
        public const string FinishedAfter = "Finished After";
        public const string FinishedBefore = "Finished Before";
        public const string FinishTime = "Finish Time";
        public const string FullPath = "Full Path";

        public const string GetChangedWorkItems = "Get Changed Work Items";

        public const string History = "History";
        
        public const string Id = "Id";
        public const string InconclusiveTests = "Number of Inconclusive Tests";
        public const string InProgressTests = "Number of Inprogress Tests";
        public const string IsAutomated = "Is Automated";
        public const string ItemChangesetId = "Changset ID";
        public const string ItemCheckinDate = "Check-in Date";
        public const string ItemContentLength = "Content Length";
        public const string ItemFolder = "Item Folder";
        public const string ItemLocalPath = "Local Path";
        public const string ItemServerPath = "Server Path";
        public const string ItemType = "Item Type";
        public const string Iteration = "Iteration";
        public const string IterationPath = "Iteration Path";

        public const string KeepForever = "Keep forever";
        
        public const string LastBuildUri = "Last Build URI";
        public const string LastGoodBuildLabel = "Last Good Build Label";
        public const string LastGoodBuildUri = "Last Good Build URI";
        public const string LastUpdated = "Last Updated";
        public const string LastUpdatedBy = "Last Updated By";
        public const string ListOfTestCases = "Comma separated test cases";
        public const string ListOfTestSuites = "Comma separated test suites";
        public const string LogLocation = "Log Location";

        public const string MaxBuildsPerDefinition = "Maximum Builds Per Definition";

        public const string NumberOfConflicts = "Number Of Conflicts";
        public const string NumberOfFailures = "Number Of Failures";
        public const string NumberOfFields = "Number Of Fields";
        public const string NumberOfObjects = "Number Of Objects";
        public const string NumberOfUpdated = "Number Of Updated";
        public const string NumberOfWarnings = "Number Of Warnings";

        public const string Owner = "Owner";

        public const string ParentTestSuiteId = "Parent Test Suite ID";
        public const string PassedTests = "Number of Passed Tests";
        public const string PendingChange = "Pending Change";
        public const string PendingTests = "Number of Pending Tests";
        public const string Priority = "Priority";
        public const string Project = "Project";
        public const string PropertyName = "Property Name";
        public const string PropertyValue = "Property Value";
        
        public const string Query = "Query (WIQL)";
        public const string QueryInterval = "Query interval (second)";

        public const string Reason = "Reason";
        public const string RemoveAttachment = "File attachment to be removed";
        public const string RequestedBy = "Requested By";
        public const string RequestedFor = "Requested For";
        
        public const string ShelvesetName = "Shelveset Name";
        public const string SourceGetVersion = "Source Get Version";
        public const string StartDate = "Start Date";
        public const string StartTime = "Start Time";
        public const string State = "State";

        public const string TestCaseId = "Test Case ID";
        public const string TestConfigurationId = "Test Configuration ID";
        public const string TestConfigurationName = "Test Configuration Name";
        public const string TestOutcome = "Test Outcome";
        public const string TestPlanId = "Test Plan ID";
        public const string TestPlanName = "Test Plan Name";
        public const string TestRunId = "Test Run ID";
        public const string TestRunTitle = "Test Run Title";
        public const string TestStatus = "Test Status";
        public const string TestSuiteId = "Test Suite ID";
        public const string TestSuiteState = "Test Suite State";
        public const string TestSuiteTitle = "Test Suite Title";
        public const string Title = "Title";
        public const string TotalTests = "Number of Total Tests";
        
        public const string VersionControlRecursionType = "Recursion Type";
        public const string VersionControlServerPath = "Version Control Server Path";

        public const string WorkItemId = "Work Item ID";
        public const string WorkItemQuery = "Work Item Query";
        public const string WorkItemType = "Type";
        public const string WorkspacePath = "Local Workspace Path";

        #endregion

        public static readonly Dictionary<string, string> GetWorkItem = new Dictionary<string,string>();
        public static readonly Dictionary<string, string> SetWorkItem = GetWorkItem;
        public static readonly Dictionary<string, string> NewWorkItem = GetWorkItem;
        public static readonly Dictionary<string, string> QueryWorkItems = new Dictionary<string, string>();

        public static readonly Dictionary<string, string> BuildProperties = new Dictionary<string, string>();

        public static readonly Dictionary<string, string> BuildDefinitionProperties = new Dictionary<string, string>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static PublishedData()
        {
            GetWorkItem[AreaPath] = AsString;
            // "Attached File Count",
            // "Attachments",
            GetWorkItem[ChangedBy] = AsString;
            GetWorkItem[ChangedDate] = AsDateTime;
            GetWorkItem[CreatedBy] = AsString;
            GetWorkItem[CreatedDate] = AsDateTime;
            GetWorkItem[Description] = AsString;
            GetWorkItem[NumberOfFields] = AsNumber;
            GetWorkItem[History] = AsString;
            GetWorkItem[Id] = AsNumber;
            GetWorkItem[IterationPath] = AsString;
            // "Links",
            GetWorkItem[Reason] = AsString;
            // Rev
            // Revision
            GetWorkItem[State] = AsString;
            GetWorkItem[Title] = AsString;
            GetWorkItem[WorkItemType] = AsString;
            GetWorkItem[ChangedFields] = AsString;
            GetWorkItem[ChangedFieldDetails] = AsString;
            GetWorkItem[Attachment] = AsString;

            QueryWorkItems[AreaPath] = AsString;
            QueryWorkItems[ChangedBy] = AsString;
            QueryWorkItems[ChangedDate] = AsDateTime;
            QueryWorkItems[CreatedBy] = AsString;
            QueryWorkItems[CreatedDate] = AsDateTime;
            QueryWorkItems[Description] = AsString;
            QueryWorkItems[History] = AsString;
            QueryWorkItems[Id] = AsNumber;
            QueryWorkItems[IterationPath] = AsString;
            QueryWorkItems[Reason] = AsString;
            QueryWorkItems[State] = AsString;
            QueryWorkItems[Title] = AsString;
            QueryWorkItems[WorkItemType] = AsString;

            BuildProperties[BuildDefinitionName] = AsString;
            BuildProperties[BuildLabelName] = AsString;
            BuildProperties[BuildNumber] = AsString;
            BuildProperties[BuildQuality] = AsString;
            BuildProperties[BuildControllerUri] = AsString;
            BuildProperties[BuildFinished] = AsString;
            BuildProperties[CompilationStatus] = AsString;
            BuildProperties[DropLocation] = AsString;
            BuildProperties[DropLocationRoot] = AsString;
            BuildProperties[FinishTime] = AsDateTime;
            BuildProperties[LogLocation] = AsString;
            BuildProperties[BuildReason] = AsString;
            BuildProperties[RequestedBy] = AsString;
            BuildProperties[RequestedFor] = AsString;
            BuildProperties[ShelvesetName] = AsString;
            BuildProperties[SourceGetVersion] = AsString;
            BuildProperties[StartTime] = AsDateTime;
            BuildProperties[BuildStatus] = AsString;
            BuildProperties[TestStatus] = AsString;
            BuildProperties[BuildUri] = AsString;
            BuildProperties[AssociatedChangesets] = AsString;

            BuildDefinitionProperties[BuildDefinitionName] = AsString;
            BuildDefinitionProperties[BuildDefinitionUri] = AsString;
            BuildDefinitionProperties[BuildControllerUri] = AsString;
            BuildDefinitionProperties[DefaultDropLocation] = AsString;
            BuildDefinitionProperties[Description] = AsString;
            BuildDefinitionProperties[Enabled] = AsString;
            BuildDefinitionProperties[FullPath] = AsString;
            BuildDefinitionProperties[LastBuildUri] = AsString;
            BuildDefinitionProperties[LastGoodBuildLabel] = AsString;
            BuildDefinitionProperties[LastGoodBuildUri] = AsString;
        }
    }
}
