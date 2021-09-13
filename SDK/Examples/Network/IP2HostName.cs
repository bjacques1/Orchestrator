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

 

using System;
using System.Net;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Network
{
    /// <summary>
    /// An Activity that retrieve the host name from an IP Address.
    /// </summary>
    [Activity("IP To Host")]
    public class IP2HostName : IActivity
    {
        public void Design(IActivityDesigner designer)
        {
            designer.AddInput("IP Address");
            designer.AddOutput("Host Name");            
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            IPHostEntry host = Dns.GetHostEntry(request.Inputs["IP Address"].AsString());
            response.PublishRange("Host Name", host.HostName);
        }

    }
}
