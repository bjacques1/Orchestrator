using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TeamFoundationServerIntegrationPack
{
    enum CheckoutAction
    {
        Edit,
        Add,
        Delete,
    }

    /// <summary>
    ///     Helper class for TFS version control
    /// </summary>
    class VersionControlHelper
    {
        private VersionControlServer vcServer;
        private Dictionary<string, object> checkinEvent;
        public EventWaitHandle CheckinDetected = new AutoResetEvent(false);

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="url">TFS URL, e.g. "http://scxtfs2:8080/tfs"</param>
        public VersionControlHelper(
            string url,
            string domain,
            string userName,
            string password,
            bool handleEvents = false)
        {
            var tfs = TfsConnectionFactory.GetTeamProjectCollection(url, domain, userName, password);
            vcServer = tfs.GetService<VersionControlServer>();

            EventTracing.TraceInfo(
                "Connect to '{0}' for version control",
                url);

            if (handleEvents)
            {
                checkinEvent = new Dictionary<string, object>();
                SetCheckinEventHandlers();
            }
        }

        /// <summary>
        ///     Lists the items on a server path.  No workspace is needed.
        /// </summary>
        /// <param name="path">Path on the server, e.g. "$/zzSandbox/zhyao/src/ProductStudioIntegrationPack"</param>
        /// <param name="recursion">string of RecursionType, e.g. "Full", "None", "OneLevel"</param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetItems(
            string path,
            string recursion)
        {
            var results = new List<Dictionary<string, object>>();
            
            var recursionType = (RecursionType)Enum.Parse(typeof(RecursionType), recursion);

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "GetItems: path='{0}' recursion='{1}'=>'{2}'",
                path,
                recursion,
                recursionType);

            var items = vcServer.GetItems(path, recursionType);

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "GetItems: {0} items returned",
                items.Items.GetLength(0));

            foreach (Item item in items.Items)
            {
                var prop = new Dictionary<string, object>();

                prop[PublishedData.ItemType] = item.ItemType.ToString();
                prop[PublishedData.ItemServerPath] = item.ServerItem;
                prop[PublishedData.ItemContentLength] = item.ContentLength;
                prop[PublishedData.ItemChangesetId] = item.ChangesetId;
                prop[PublishedData.ItemCheckinDate] = item.CheckinDate;

                results.Add(prop);
            }

            return results;
        }

        /// <summary>
        ///     Downloads a file from the server to the local machine without a workspace.
        /// </summary>
        /// <param name="serverPath"></param>
        /// <param name="localFileName"></param>
        public void DownloadFile(
            string serverPath,
            string localFileName)
        {
            // Note: if the serverPath is incorrect, VersionControlException will be raised which will be caught by
            // QIK.  If the local file is not accessible, no exception will be raised.
            vcServer.DownloadFile(serverPath, localFileName);

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "DownloadFile: serverPath='{0}' localFileName='{1}'",
                serverPath,
                localFileName);
        }

        /// <summary>
        ///     Updates the local workspace to the latest version.
        /// </summary>
        /// <param name="workspacePath"></param>
        /// <returns></returns>
        public Dictionary<string, object> UpdateWorkspace(
            string workspacePath)
        {
            var ws = GetWorkspace(workspacePath);
            var status = ws.Get();

            var dict = new Dictionary<string, object>();

            dict[PublishedData.NumberOfUpdated] = status.NumUpdated;
            dict[PublishedData.NumberOfConflicts] = status.NumConflicts;
            dict[PublishedData.NumberOfFailures] = status.NumFailures;
            dict[PublishedData.NumberOfWarnings] = status.NumWarnings;

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "GetWrokspace: workspacePath='{0}'",
                workspacePath);

            return dict;
        }

        /// <summary>
        ///     Checks out one or more files in a local workspace.
        /// </summary>
        /// <param name="workspacePath"></param>
        /// <param name="localFilePath"></param>
        /// <param name="action"></param>
        /// <param name="recursion"></param>
        public void Checkout(
            string workspacePath,
            string localFilePath,
            string action,
            string recursion)
        {
            var recursionType = (RecursionType)Enum.Parse(typeof(RecursionType), recursion);
            var checkoutAction = (CheckoutAction)Enum.Parse(typeof(CheckoutAction), action);

            var ws = GetWorkspace(workspacePath);

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "Checkout: workspacePath='{0}' localFilePath='{1}' action='{2}'=>'{3}' recursion='{4}'=>'{5}'",
                workspacePath,
                localFilePath,
                action, checkoutAction.ToString(),
                recursion, recursionType.ToString());

            int num = 0;

            switch (checkoutAction)
            {
                case CheckoutAction.Add:
                    EventTracing.TraceInfo("KNOWN BUG: Workspace.PendAdd() does not work in session 0");
                    num = ws.PendAdd(localFilePath, recursionType == RecursionType.Full);
                    break;

                case CheckoutAction.Delete:
                    num = ws.PendDelete(localFilePath, recursionType);
                    break;

                case CheckoutAction.Edit:
                    num = ws.PendEdit(localFilePath, recursionType);
                    break;
            }

            EventTracing.TraceEvent(TraceEventType.Verbose, 0, "Checkout: {0} items checked out", num);
        }

        /// <summary>
        ///     Checks in the pending changes in the local workspace.
        /// </summary>
        /// <param name="workspacePath"></param>
        /// <param name="localFilePath"></param>
        /// <param name="recursion"></param>
        /// <param name="comment"></param>
        public void Checkin(
            string workspacePath,
            string localFilePath,
            string recursion,
            string comment)
        {
            var recursionType = (RecursionType)Enum.Parse(typeof(RecursionType), recursion);
            var ws = GetWorkspace(workspacePath);

            PendingChange[] changes = null;

            if (string.IsNullOrEmpty(localFilePath))
            {
                changes = ws.GetPendingChanges();
            }
            else
            {
                changes = ws.GetPendingChanges(localFilePath, recursionType);
            }

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "Checkin: workspacePath='{0}' localFilePath='{1}' recursion='{2}'=>'{3}' comment={4}",
                workspacePath,
                localFilePath,
                recursion, recursionType.ToString(),
                comment);

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "Number of pending changes: {0}",
                changes.GetLength(0));

            ws.CheckIn(changes, comment);
        }

        /// <summary>
        ///     Retrieves and resets the check-in event
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetCheckinEvent()
        {
            Dictionary<string, object> ret;

            lock (checkinEvent)
            {
                ret = new Dictionary<string, object>(checkinEvent);
                checkinEvent.Clear();
            }

            return ret;
        }

        /// <summary>
        ///     Sets handlers for check-in events
        /// </summary>
        private void SetCheckinEventHandlers()
        {
            EventTracing.TraceInfo(
                "Handlers for check-in event set.");

            vcServer.BeforeCheckinPendingChange +=
                delegate(object sender, ProcessingChangeEventArgs e)
                {
                    EventTracing.TraceInfo("BeforeCheckinPendingChange");

                    lock (checkinEvent)
                    {
                        checkinEvent[PublishedData.CheckinEvent] = "BeforeCheckinPendingChange";
                        checkinEvent[PublishedData.PendingChange] = PendingChangeToString(e.PendingChange);
                    }

                    CheckinDetected.Set();
                };

            vcServer.CommitCheckin +=
                delegate(object sender, CommitCheckinEventArgs e)
                {
                    EventTracing.TraceInfo("BeforeCheckinPendingChange");
                    
                    lock (checkinEvent)
                    {
                        checkinEvent[PublishedData.CheckinEvent] = "CommitCheckin";
                        checkinEvent[PublishedData.ItemChangesetId] = e.ChangesetId;

                        var sb = new StringBuilder();
                        foreach (PendingChange pendingChange in e.Changes)
                        {
                            sb.AppendLine(PendingChangeToString(pendingChange));
                        }
                        checkinEvent[PublishedData.PendingChange] = sb.ToString();
                    }

                    CheckinDetected.Set();
                };
        }

        private static string PendingChangeToString(
            PendingChange pendingChange)
        {
            return pendingChange.ChangeType.ToString() + " : " + pendingChange.FileName;
        }

        public Changeset GetChangeset(int id)
        {
            var changeset = vcServer.GetChangeset(id, true, true, true);

            EventTracing.TraceEvent(
                TraceEventType.Verbose,
                0,
                "GetChangeset: id={0}",
                id);

            return changeset;
        }

        /// <summary>
        ///     Returns a list of shelvesets and their properties for a given shelveset name or owner name
        /// </summary>
        /// <param name="shelveSetName"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetListOfShelveSets(
                string shelveSetName,
                string owner)
        {
            var shelveSets = vcServer.QueryShelvesets(shelveSetName, owner);

            var svProps = new List<Dictionary<string, object>>();
            foreach (var sv in shelveSets)
            {
                var prop = new Dictionary<string, object>();

                prop[PublishedData.Comment] = sv.Comment;
                prop[PublishedData.CreationDate] = sv.CreationDate;
                prop[PublishedData.DisplayName] = sv.DisplayName;
                prop[PublishedData.ShelvesetName] = sv.Name;
                prop[PublishedData.Owner] = sv.OwnerName;

                svProps.Add(prop);
            }

            return svProps;
        }

        /// <summary>
        ///     Returns the list of files in the shelveset for the given shelveset name and owner alias (DOMAIN\user)
        /// </summary>
        /// <param name="shelveSetName"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetShelveSet(
            string shelveSetName,
            string owner)
        {
            if (string.IsNullOrEmpty(shelveSetName) || string.IsNullOrEmpty(owner))
                return null;

            var shelveSets = vcServer.QueryShelvesets(shelveSetName, owner);
            if (shelveSets == null || shelveSets.Length == 0)
            {
                EventTracing.TraceInfo("shelveset not found");
                return null;
            }

            var svProps = new List<Dictionary<string, object>>();

            var pendingSets = vcServer.QueryShelvedChanges(shelveSets[0]);
            if (pendingSets == null)
            {
                EventTracing.TraceInfo("QueryShelvedChanges returned null");
                return null;
            }

            foreach (var ps in pendingSets)
            {
                var pendingChanges = ps.PendingChanges;

                foreach (var pc in pendingChanges)
                {
                    var prop = new Dictionary<string, object>();

                    prop[PublishedData.ItemType] = pc.ItemType.ToString();
                    prop[PublishedData.FileName] = pc.FileName;
                    prop[PublishedData.ChangeType] = pc.ChangeType.ToString();
                    prop[PublishedData.ItemFolder] = pc.LocalOrServerFolder;
                    prop[PublishedData.CreationDate] = pc.CreationDate;

                    svProps.Add(prop);
                }
            }

            return svProps;
        }

        /// <summary>
        ///     Queries the server for the workspace that matches the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private Workspace GetWorkspace(string path)
        {
            var absolutePath = System.IO.Path.GetFullPath(path);

            var workspaces = vcServer.QueryWorkspaces(null, vcServer.AuthorizedUser, Environment.MachineName);
            foreach (var ws in workspaces)
            {
                foreach (var folder in ws.Folders)
                {
                    if (folder.LocalItem == absolutePath)
                        return ws;
                }
            }

            EventTracing.TraceEvent(
                TraceEventType.Error,
                0,
                "GetWorkspace: path={0} not found in all workspaces in the local computer",
                absolutePath);

            throw new ActivityExecutionException("Workspace not found");
        }

        /*
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && CheckinDetected != null)
            {
                CheckinDetected.Set();
                CheckinDetected.Dispose();
                CheckinDetected = null;
            }
        }
        */
    }
}
