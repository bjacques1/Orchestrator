using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace TeamFoundationServerIntegrationPack
{
    // This only monitors the check-ins from the local machine.
    [ActivityMonitor(Interval=30)]
    [Activity("Monitor Check-in", Description="Monitors the version control server and triggers before or upon check-in")]
    public class MonitorCheckIn : IActivity
    {
        private VersionControlHelper vcHelper = null;

        [ActivityConfiguration]
        public TfsConnectionSettings ConnectionSettings
        {
            set;
            get;
        }

        public void Design(
            IActivityDesigner designer)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }

            designer.AddOutput(PublishedData.CheckinEvent).AsString();
            designer.AddOutput(PublishedData.ItemChangesetId).AsNumber();
            designer.AddOutput(PublishedData.PendingChange);
        }

        public void Execute(
            IActivityRequest request,
            IActivityResponse response)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (vcHelper == null)
            {
                vcHelper = new VersionControlHelper(
                    ConnectionSettings.Url,
                    ConnectionSettings.Domain,
                    ConnectionSettings.UserName,
                    ConnectionSettings.Password,
                    true);
            }

            vcHelper.CheckinDetected.WaitOne();

            var data = vcHelper.GetCheckinEvent();

            response.Publish(data);
        }
    }
}

