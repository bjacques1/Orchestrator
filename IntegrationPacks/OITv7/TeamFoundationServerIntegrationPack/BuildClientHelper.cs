using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.TeamFoundation.Build.Client;

namespace TeamFoundationServerIntegrationPack
{
    /// <summary>
    ///     Helper class for Team Foudation Build 2010.
    /// </summary>
    class BuildClientHelper
    {
        private IBuildServer buildServer;
        private Dictionary<string, object> buildEvent;
        public EventWaitHandle BuildStatusChanged = new AutoResetEvent(false);

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="url">TFS URL, e.g. "http://scxtfs2:8080/tfs"</param>
        public BuildClientHelper(
            string url,
            string domain,
            string userName,
            string password)
        {
            var tfs = TfsConnectionFactory.GetTeamProjectCollection(url, domain, userName, password);
            buildServer = tfs.GetService<IBuildServer>();

            EventTracing.TraceInfo(
                "Connect to '{0}' for build service.  Build serve version: {1}",
                url,
                buildServer.BuildServerVersion);
        }

        /// <summary>
        ///     Get all the build definitions in a team project or the details of a certain build definition.
        /// </summary>
        /// <param name="teamProject">Name of the team project, e.g. "SC_Orchestrator"</param>
        /// <param name="definitionName">Name of the build definition, e.g. "SCOMain"; empty to get all build definitions in the team project</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetBuildDefinitions(
            string teamProject,
            string definitionName)
        {
            IBuildDefinition[] buildDefs;
            var results = new List<Dictionary<string, object>>();

            // If the definition name is empty, the intention is to get the details of all build definitions in a team project.
            if (string.IsNullOrEmpty(definitionName))
            {
                buildDefs = buildServer.QueryBuildDefinitions(teamProject);

                EventTracing.TraceInfo("Number of build definitions found in team project '{0}' : {1}",
                    teamProject,
                    buildDefs.Length);
            }
            else
            {
                var buildDef = buildServer.GetBuildDefinition(teamProject, definitionName);
                buildDefs = new IBuildDefinition[]{buildDef,};

                EventTracing.TraceInfo("Team project '{0}' build definition '{1}'.",
                    teamProject,
                    definitionName);
            }

            if (buildDefs == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Warning,
                    (int)EventType.BuildNotFound,
                    "Build definition not found");
                return null;
            }

            // Convert and pack the build definition properties
            
            foreach (IBuildDefinition def in buildDefs)
            {
                var result = new Dictionary<string,object>();
 
                result[PublishedData.BuildDefinitionName] = def.Name;
                result[PublishedData.BuildDefinitionUri] = def.Uri;
                result[PublishedData.BuildControllerUri] = def.BuildControllerUri;
                result[PublishedData.DefaultDropLocation] = def.DefaultDropLocation;
                result[PublishedData.Description] = def.Description;
                result[PublishedData.Enabled] = def.Enabled.ToString();
                result[PublishedData.FullPath] = def.FullPath;
                result[PublishedData.LastBuildUri] = def.LastBuildUri;
                result[PublishedData.LastGoodBuildLabel] = def.LastGoodBuildLabel;
                result[PublishedData.LastGoodBuildUri] = def.LastGoodBuildUri;

                results.Add(result);

                EventTracing.TraceEvent(
                    TraceEventType.Verbose,
                    (int)EventType.DebugInfo,
                    "Build definition: {0}",
                    EventTracing.ToString(result));
            }

            return results;
        }

        /// <summary>
        ///     Retrieves the build details which have been requested/queued on the build server for a certain team
        ///     project and build definition.
        /// </summary>
        /// <param name="teamProject">Name of the team project, e.g. "SC_Orchestrator"</param>
        /// <param name="definitionName">Name of the build definition, e.g. "SCOMain"</param>
        /// <param name="maxBuildsPerDefinition">How many builds should be retrived for each definition</param>
        /// <param name="buildNumber">Filtering on build number; can be empty</param>
        /// <param name="status">Filtering on the build status; can be empty</param>
        /// <param name="quality">Filtering on the build quality; can be empty</param>
        /// <param name="buildQueryOrder">Which order the build should be queried, the default should be StartTimeDescending</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetBuildDetails(
            string teamProject,
            string definitionName,
            int maxBuildsPerDefinition,
            string buildNumber,
            string status,
            string quality,
            DateTime finishedAfter,
            DateTime finishedBefore,
            BuildQueryOrder buildQueryOrder)
        {
            var spec = buildServer.CreateBuildDetailSpec(teamProject, definitionName);

            if (maxBuildsPerDefinition > 0)
            {
                spec.MaxBuildsPerDefinition = maxBuildsPerDefinition;
            }

            if (!string.IsNullOrEmpty(buildNumber))
            {
                spec.BuildNumber = buildNumber;
            }

            if (!string.IsNullOrEmpty(status))
            {
                spec.Status = (BuildStatus)Enum.Parse(typeof(BuildStatus), status);
            }

            if (!string.IsNullOrEmpty(quality))
            {
                spec.Quality = quality;
            }

            spec.MinFinishTime = finishedAfter;
            spec.MaxFinishTime = finishedBefore;

            spec.QueryOrder = buildQueryOrder;

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                (int)EventType.DebugInfo,
                "GetBuildDetails: MaxBuildsPerDefinition={0} BuildNumber={1} Status={2} Quality={3} Time={4} - {5}",
                maxBuildsPerDefinition,
                buildNumber,
                status,
                quality,
                finishedAfter,
                finishedBefore);

            var builds = buildServer.QueryBuilds(spec);

            var results = new List<Dictionary<string, object>>();

            if (builds == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Warning,
                    (int)EventType.BuildNotFound,
                    "QueryBuilds failed");
                return null;
            }

            EventTracing.TraceInfo(
                "Number of build details: {0}",
                builds.Builds.Length);

            foreach (IBuildDetail detail in builds.Builds)
            {
                var result = BuildDetailProperties(detail);

                var changesets = InformationNodeConverters.GetAssociatedChangesets(detail);
                var sb = new StringBuilder();
                foreach (var changeset in changesets)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        "{0} {1}; ",
                        changeset.ChangesetId,
                        changeset.CheckedInBy);
                }

                result[PublishedData.AssociatedChangesets] = sb.ToString();

                results.Add(result);

                EventTracing.TraceEvent(
                    TraceEventType.Verbose,
                    (int)EventType.DebugInfo,
                    "Build detail: {0}",
                    EventTracing.ToString(result));
            }

            return results;
        }

        /// <summary>
        ///     Changes the build details
        /// </summary>
        /// <param name="uri">URI of the build</param>
        /// <param name="buildNumber">New build number if not empty</param>
        /// <param name="dropLocation">New drop location if not empty</param>
        /// <param name="labelName">New build label if not empty</param>
        /// <param name="logLocation">New log location if not empty</param>
        /// <param name="quality">New quality if not empty</param>
        /// <param name="status">New build status if not empty, should be string of BuildStatus enum</param>
        /// <param name="testStatus">New test status if not empty, should be string of BuildPhaseStatus enum</param>
        /// <param name="keepForever">Whether the build should be kept for ever</param>
        /// <returns></returns>
        public Dictionary<string, object> SetBuildDetails(
            string uri,
            string buildNumber,
            string dropLocation,
            string labelName,
            string logLocation,
            string quality,
            string status,
            string testStatus,
            string keepForever)
        {
            var detail = buildServer.GetBuild(new Uri(uri));
            if (detail == null)
            {
                EventTracing.TraceEvent(TraceEventType.Warning,
                    (int)EventType.BuildNotFound,
                    "Build '{0}' not found",
                    uri);

                return null;
            }

            if (!string.IsNullOrEmpty(buildNumber))
            {
                detail.BuildNumber = buildNumber;
            }

            if (!string.IsNullOrEmpty(dropLocation))
            {
                detail.DropLocation = dropLocation;
            }

            if (!string.IsNullOrEmpty(labelName))
            {
                detail.LabelName = labelName;
            }

            if (!string.IsNullOrEmpty(logLocation))
            {
                detail.LogLocation = logLocation;
            }

            if (!string.IsNullOrEmpty(quality))
            {
                detail.Quality = quality;
            }

            if (!string.IsNullOrEmpty(status))
            {
                detail.Status = (BuildStatus)Enum.Parse(typeof(BuildStatus), status);
            }

            if (!string.IsNullOrEmpty(testStatus))
            {
                detail.TestStatus = (BuildPhaseStatus)Enum.Parse(typeof(BuildPhaseStatus), testStatus);
            }

            if (!string.IsNullOrEmpty(keepForever))
            {
                detail.KeepForever = Boolean.Parse(keepForever);
            }

            detail.Save();

            var result = BuildDetailProperties(detail);

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                (int)EventType.DebugInfo,
                "SetBuildDetails: result saved. {0}",
                EventTracing.ToString(result));

            return result;
        }

        /// <summary>
        ///     Requests for a new build and queues it.
        /// </summary>
        /// <param name="teamProject">Name of the team project, e.g. "SC_Orchestrator"</param>
        /// <param name="definitionName">Name of the build definition, e.g. "SCOMain"</param>
        /// <param name="dropLocation">Desired drop location, if not empty</param>
        /// <param name="priority">Desired queue priority, if not empty</param>
        /// <param name="reason">Reason for requesting the build, if not empty</param>
        /// <param name="requestedFor">Whom the request is for, if not empty</param>
        /// <param name="shelvesetName">Name of the shelveset to be applied, if not empty</param>
        /// <returns>If the build is requested successfully</returns>
        public bool RequestBuild(
            string teamProject,
            string definitionName,
            string dropLocation,
            string priority,
            string reason,
            string requestedFor,
            string shelvesetName)
        {
            var buildDef = buildServer.GetBuildDefinition(teamProject, definitionName);

            if (buildDef == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Warning,
                    (int)EventType.BuildNotFound,
                    "RequestBuild: Build not found");

                return false;
            }

            var buildReq = buildDef.CreateBuildRequest();

            if (!string.IsNullOrEmpty(dropLocation))
            {
                buildReq.DropLocation = dropLocation;
            }

            if (!string.IsNullOrEmpty(priority))
            {
                buildReq.Priority = (QueuePriority)Enum.Parse(typeof(QueuePriority), priority);
            }

            if (!string.IsNullOrEmpty(reason))
            {
                buildReq.Reason = (BuildReason)Enum.Parse(typeof(BuildReason), reason);
            }

            if (!string.IsNullOrEmpty(requestedFor))
            {
                buildReq.RequestedFor = requestedFor;
            }

            if (!string.IsNullOrEmpty(shelvesetName))
            {
                buildReq.ShelvesetName = shelvesetName;
            }

            buildServer.QueueBuild(buildReq);

            EventTracing.TraceEvent(TraceEventType.Verbose,
                (int)EventType.DebugInfo,
                "RequestBuild: DropLocation={0} Priority={1} Reason={2} RequestedFor={3} Shelveset={4}",
                dropLocation,
                priority,
                reason,
                requestedFor,
                shelvesetName);

            return true;
        }

        public Dictionary<string,object> CreateManualBuild(
            string teamProject,
            string buildDefinitionName,
            string buildNumber,
            string dropLocation,
            BuildStatus buildStatus,
            string buildControllerName,
            string requestedFor,
            string compilationStatus,
            string keepForever,
            string buildLabelName,
            string logLocation,
            string buildQuality,
            string sourceGetVersion,
            string testStatus)
        {
            var buildDef = buildServer.GetBuildDefinition(teamProject, buildDefinitionName);

            if (buildDef == null)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Warning,
                    (int)EventType.BuildNotFound,
                    "CreateManualBuild: Build definition not found");

                throw new ActivityExecutionException("Build definition not found in the team project");
            }

            IBuildController controller = null;
            if (!string.IsNullOrEmpty(buildControllerName))
            {
                controller = buildServer.GetBuildController(buildControllerName);

                if (controller == null)
                {
                    EventTracing.TraceEvent(
                        TraceEventType.Warning,
                        (int)EventType.BuildControllerNotFound,
                        "CreateManualBuild: build controller '{0}' not found",
                        buildControllerName);
                }

                throw new ActivityExecutionException("Build controller not found on the build server");
            }

            var buildDetail = buildDef.CreateManualBuild(
                buildNumber,
                dropLocation,
                buildStatus,
                controller,
                requestedFor);

            if (!string.IsNullOrEmpty(compilationStatus))
            {
                try
                {
                    buildDetail.CompilationStatus = (BuildPhaseStatus)Enum.Parse(typeof(BuildPhaseStatus), compilationStatus);
                }
                catch (ArgumentException)
                {
                }
            }

            if (!string.IsNullOrEmpty(keepForever))
            {
                try
                {
                    buildDetail.KeepForever = bool.Parse(keepForever);
                }
                catch (ArgumentException)
                {
                }
            }

            if (!string.IsNullOrEmpty(buildLabelName))
            {
                buildDetail.LabelName = buildLabelName;
            }

            if (!string.IsNullOrEmpty(logLocation))
            {
                buildDetail.LogLocation = logLocation;
            }

            if (!string.IsNullOrEmpty(buildQuality))
            {
                buildDetail.Quality = buildQuality;
            }

            if (!string.IsNullOrEmpty(sourceGetVersion))
            {
                buildDetail.SourceGetVersion = sourceGetVersion;
            }

            if (!string.IsNullOrEmpty(testStatus))
            {
                try
                {
                    buildDetail.TestStatus = (BuildPhaseStatus)Enum.Parse(typeof(BuildPhaseStatus), testStatus);
                }
                catch (ArgumentException)
                {
                }
            }

            string serverException = string.Empty;

            try
            {
                buildDetail.Save();
            }
            catch (BuildServerException e)
            {
                EventTracing.TraceEvent(
                    TraceEventType.Warning,
                    (int)EventType.ExceptionCaught,
                    "BuildServerException: {0}",
                    e);

                serverException = e.Message;
            }

            var result = BuildDetailProperties(buildDetail);

            if (!string.IsNullOrEmpty(serverException))
            {
                result[PublishedData.BuildServerException] = serverException;
            }

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                (int)EventType.DebugInfo,
                "SetBuildDetails: result saved. {0}",
                EventTracing.ToString(result));

            return result;
        }

        /// <summary>
        ///     Retrieves and reset the build status changed event
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetBuildEvent()
        {
            Dictionary<string, object> ret;

            lock (buildEvent)
            {
                ret = new Dictionary<string, object>(buildEvent);
                buildEvent.Clear();
            }

            return ret;
        }

        /// <summary>
        ///     Sets handlers for build status changed events of a given build (IBuildDetail)
        /// </summary>
        /// <param name="uri"></param>
        public bool SetBuildEventHandlers(
            string teamProject,
            string definitionName)
        {
            var spec = buildServer.CreateBuildDetailSpec(teamProject, definitionName);
            spec.Status = BuildStatus.InProgress;

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                (int)EventType.DebugInfo,
                "Get all build in team project '{0}' build '{1}' and status=InProgress.",
                teamProject,
                definitionName);

            var builds = buildServer.QueryBuilds(spec);

            if (builds == null)
            {
                EventTracing.TraceEvent(TraceEventType.Warning,
                    (int)EventType.BuildNotFound,
                    "QueryBuilds failed");

                return false;
            }
            else if (builds.Builds.GetLength(0) == 0)
            {
                EventTracing.TraceEvent(TraceEventType.Warning,
                    (int)EventType.BuildNotFound,
                    "No build is found");

                return false;
            }

            buildEvent = new Dictionary<string, object>();

            foreach (IBuildDetail detail in builds.Builds)
            {
                EventTracing.TraceEvent(TraceEventType.Verbose,
                    (int)EventType.DebugInfo,
                    "Monitoring build Label '{0}' number '{1}'",
                    detail.LabelName, detail.BuildNumber);

                detail.StatusChanged +=
                    delegate(object sender, StatusChangedEventArgs e)
                    {
                        IBuildDetail senderDetail = sender as IBuildDetail;

                        lock (buildEvent)
                        {
                            if (senderDetail != null)
                            {
                                buildEvent[PublishedData.BuildNumber] = senderDetail.BuildNumber;
                                buildEvent[PublishedData.BuildLabelName] = senderDetail.LabelName;
                            }

                            buildEvent[PublishedData.BuildEvent] = "StatusChanged";
                        }

                        BuildStatusChanged.Set();
                    };

                detail.StatusChanging +=
                    delegate(object sender, StatusChangedEventArgs e)
                    {
                        IBuildDetail senderDetail = sender as IBuildDetail;

                        lock (buildEvent)
                        {
                            if (senderDetail != null)
                            {
                                buildEvent[PublishedData.BuildNumber] = senderDetail.BuildNumber;
                                buildEvent[PublishedData.BuildLabelName] = senderDetail.LabelName;
                            }

                            buildEvent[PublishedData.BuildEvent] = "StatusChanging";
                        }

                        BuildStatusChanged.Set();
                    };


                detail.Connect();
            }

            return true;
        }

        /// <summary>
        ///     Converts IBuildDetail object to key-value pairs.
        /// </summary>
        /// <param name="detail"></param>
        /// <returns></returns>
        private static Dictionary<string, object> BuildDetailProperties(
            IBuildDetail detail)
        {
            var result = new Dictionary<string, object>();

            result[PublishedData.BuildDefinitionName] = detail.BuildDefinition.Name;
            result[PublishedData.BuildLabelName] = detail.LabelName;
            result[PublishedData.BuildNumber] = detail.BuildNumber;
            result[PublishedData.BuildQuality] = detail.Quality;
            result[PublishedData.BuildControllerUri] = detail.BuildControllerUri;
            result[PublishedData.BuildFinished] = detail.BuildFinished.ToString();
            result[PublishedData.CompilationStatus] = detail.CompilationStatus.ToString();
            result[PublishedData.DropLocation] = detail.DropLocation;
            result[PublishedData.DropLocationRoot] = detail.DropLocationRoot;
            result[PublishedData.FinishTime] = detail.FinishTime;
            result[PublishedData.LogLocation] = detail.LogLocation;
            result[PublishedData.BuildReason] = detail.Reason.ToString();
            result[PublishedData.RequestedBy] = detail.RequestedBy;
            result[PublishedData.RequestedFor] = detail.RequestedFor;
            result[PublishedData.ShelvesetName] = detail.ShelvesetName;
            result[PublishedData.SourceGetVersion] = detail.SourceGetVersion;
            result[PublishedData.StartTime] = detail.StartTime;
            result[PublishedData.BuildStatus] = detail.Status;
            result[PublishedData.TestStatus] = detail.TestStatus.ToString();
            result[PublishedData.BuildUri] = detail.Uri;

            return result;
        }

        /*
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && BuildStatusChanged != null)
            {
                BuildStatusChanged.Set();
                BuildStatusChanged.Dispose();
                BuildStatusChanged = null;
            }
        }
        */
    }
}
