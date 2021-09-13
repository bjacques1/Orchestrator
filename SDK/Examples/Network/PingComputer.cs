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
    /// An Activity that can ping a computer.
    /// </summary>
    [Activity]
    public class PingComputer
    {
        private string hostNameOrAddress;
        private int timeout = 120;

        [ActivityInput("Hostname/Address")]
        public string HostNameOrAddress
        {
            set { hostNameOrAddress = value; }
        }

        [ActivityInput(Optional = true)]
        public int Timeout
        {
            set { timeout = value; }
        }

        [ActivityOutput]
        public PingReplyAdapter Reply
        {
            get
            {
                using (var pingSender = new Ping())
                {
                    return new PingReplyAdapter(pingSender.Send(hostNameOrAddress, timeout));
                }
            }
        }
    }
}
