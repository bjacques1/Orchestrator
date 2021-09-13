//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Microsoft Public License.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

 

using System.Net;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Network
{
    /// <summary>
    /// An Activity that retrieve the IP address from a host
    /// </summary>
    [Activity("Host To IP")]
    public class HostName2IP : IActivity
    {
        public void Design(IActivityDesigner designer)
        {
            designer.AddInput("Host Name");
            designer.AddOutput("IP Address");
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            IPHostEntry host = Dns.GetHostEntry(request.Inputs["Host Name"].AsString());
            response.PublishRange("IP Address", host.AddressList);
        }
    }
}