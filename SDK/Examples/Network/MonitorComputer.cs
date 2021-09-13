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
using System.Net.NetworkInformation;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Network
{
    /// <summary>
    /// An Activity that monitors the status some host
    /// </summary>
    [Activity, ActivityMonitor(Interval=15)]
    public class MonitorComputer : IActivity        
    {
        private static readonly string NAME = "Hostname/Address";
        private static readonly string ROUND_TRIP_TIME = "Round Trip Time";
        private static readonly string STATUS = "Status";
        
        public void Design(IActivityDesigner designer)
        {
            designer.AddInput(NAME).WithComputerBrowser();
            designer.AddOutput(ROUND_TRIP_TIME).AsNumber();
            designer.AddOutput(STATUS);            
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            using (var ping = new Ping())
            {
                PingReply reply = ping.Send(request.Inputs[NAME].AsString());
                response.Publish(STATUS, reply.Status);
                response.Publish(ROUND_TRIP_TIME, reply.RoundtripTime);
            }
        }
    }
}
