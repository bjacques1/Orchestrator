namespace TeamFoundationServerIntegrationPack
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SystemCenter.Orchestrator.Integration;
    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    ///     Orchestrator activity - Create Manual Build
    /// </summary>
    [Activity("Create Manual Build", Description = "Create a manual build in TFS")]
    public class CreateManualBuild : IActivity
    {
        [ActivityConfiguration]
        public TfsConnectionSettings ConnectionSettings { get; set; }

        public void Design(IActivityDesigner designer)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }

            designer.AddInput(PublishedData.Project);
            designer.AddInput(PublishedData.BuildDefinitionName);

            designer.AddInput(PublishedData.BuildNumber);
            designer.AddInput(PublishedData.DropLocation);
            designer.AddInput(PublishedData.BuildStatus)
                .WithDefaultValue(BuildStatus.Succeeded).WithEnumBrowser(typeof(BuildStatus));
            designer.AddInput(PublishedData.BuildControllerName).NotRequired().WithComputerBrowser();
            designer.AddInput(PublishedData.RequestedFor).NotRequired();
            designer.AddInput(PublishedData.CompilationStatus).NotRequired()
                .WithEnumBrowser(typeof(BuildPhaseStatus)).WithDefaultValue(BuildPhaseStatus.Succeeded);
            designer.AddInput(PublishedData.KeepForever).NotRequired().WithBooleanBrowser().WithDefaultValue(true);
            designer.AddInput(PublishedData.BuildLabelName).NotRequired();
            designer.AddInput(PublishedData.LogLocation).NotRequired();
            designer.AddInput(PublishedData.BuildQuality).NotRequired();
            designer.AddInput(PublishedData.SourceGetVersion).NotRequired();
            designer.AddInput(PublishedData.TestStatus).NotRequired()
                .WithEnumBrowser(typeof(BuildPhaseStatus)).WithDefaultValue(BuildPhaseStatus.Succeeded);

            designer.AddOutput(PublishedData.BuildServerException);

            foreach (KeyValuePair<string, string> kvp in PublishedData.BuildProperties)
            {
                switch (kvp.Value)
                {
                    case PublishedData.AsDateTime:
                        designer.AddOutput(kvp.Key).AsDateTime().WithFilter();
                        break;

                    case PublishedData.AsNumber:
                        designer.AddOutput(kvp.Key).AsNumber().WithFilter();
                        break;

                    case PublishedData.AsString:
                        designer.AddOutput(kvp.Key).AsString().WithFilter();
                        break;
                }
            }
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
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

            var result = buildHelper.CreateManualBuild(
                request.Inputs[PublishedData.Project].AsString(),
                request.Inputs[PublishedData.BuildDefinitionName].AsString(),
                request.Inputs[PublishedData.BuildNumber].AsString(),
                request.Inputs[PublishedData.DropLocation].AsString(),
                request.Inputs[PublishedData.BuildStatus].As<BuildStatus>(),
                GetInput(request, PublishedData.BuildControllerName),
                GetInput(request, PublishedData.RequestedFor),
                GetInput(request, PublishedData.CompilationStatus),
                GetInput(request, PublishedData.KeepForever),
                GetInput(request, PublishedData.BuildLabelName),
                GetInput(request, PublishedData.LogLocation),
                GetInput(request, PublishedData.BuildQuality),
                GetInput(request, PublishedData.SourceGetVersion),
                GetInput(request, PublishedData.TestStatus));

            response.WithFiltering().Publish(result);
        }

        private static string GetInput(IActivityRequest request, string name)
        {
            string ret = string.Empty;

            if (request.Inputs.Contains(name))
            {
                ret = request.Inputs[name].AsString();
            }

            return ret;
        }
    }
}
