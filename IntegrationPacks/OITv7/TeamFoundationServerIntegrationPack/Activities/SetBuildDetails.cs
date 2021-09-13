using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SystemCenter.Orchestrator.Integration;
using Microsoft.TeamFoundation.Build.Client;

namespace TeamFoundationServerIntegrationPack
{
    [Activity("Set Build Details", Description = "Change the build details")]
    public class SetBuildDetails : IActivity
    {
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

            designer.AddInput(PublishedData.BuildUri);
            designer.AddInput(PublishedData.BuildNumber).NotRequired();
            designer.AddInput(PublishedData.DropLocation).NotRequired();
            designer.AddInput(PublishedData.BuildLabelName).NotRequired();
            designer.AddInput(PublishedData.LogLocation).NotRequired();
            designer.AddInput(PublishedData.BuildQuality).NotRequired();
            designer.AddInput(PublishedData.BuildStatus).NotRequired().WithEnumBrowser(typeof(BuildStatus));
            designer.AddInput(PublishedData.TestStatus).NotRequired().WithEnumBrowser(typeof(BuildPhaseStatus));
            designer.AddInput(PublishedData.KeepForever).NotRequired().WithBooleanBrowser();

            foreach (KeyValuePair<string, string> kvp in PublishedData.BuildProperties)
            {
                switch (kvp.Value)
                {
                    case PublishedData.AsDateTime:
                        designer.AddOutput(kvp.Key).AsDateTime();
                        break;

                    case PublishedData.AsNumber:
                        designer.AddOutput(kvp.Key).AsNumber();
                        break;

                    case PublishedData.AsString:
                        designer.AddOutput(kvp.Key).AsString();
                        break;
                }
            }
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

            var buildHelper = new BuildClientHelper(
                ConnectionSettings.Url,
                ConnectionSettings.Domain,
                ConnectionSettings.UserName,
                ConnectionSettings.Password);

            var uri = request.Inputs[PublishedData.BuildUri].AsString();
            var buildNumber = request.Inputs[PublishedData.BuildNumber].AsString();
            var dropLocation = request.Inputs[PublishedData.DropLocation].AsString();
            var labelName = request.Inputs[PublishedData.BuildLabelName].AsString();
            var logLocation = request.Inputs[PublishedData.LogLocation].AsString();
            var quality = request.Inputs[PublishedData.BuildQuality].AsString();
            var status = request.Inputs[PublishedData.BuildStatus].AsString();
            var testStatus = request.Inputs[PublishedData.TestStatus].AsString();
            var keepForever = request.Inputs[PublishedData.KeepForever].AsString();

            var result = buildHelper.SetBuildDetails(
                uri,
                buildNumber,
                dropLocation,
                labelName,
                logLocation,
                quality,
                status,
                testStatus,
                keepForever);

            if (result == null)
            {
                throw new ActivityExecutionException(AppResource.BuildNotFound);
            }

            response.Publish(result);
        }
    }
}

