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
using System.Net.NetworkInformation;


namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Network
{
    /// <summary>
    /// An ActivityData class that is used to encapulsate the .NET
    /// <see cref="PingReply"/> class.
    /// </summary>
    [ActivityData]
    public class PingReplyAdapter
    {
        private readonly PingReply reply;

        internal PingReplyAdapter(PingReply reply)
        {
            this.reply = reply;
        }

        [ActivityOutput]
        public IPAddress Address
        {
            get { return reply.Address; }
        }

        [ActivityOutput("Round Trip Time")]
        public long RoundTripTime
        {
            get { return reply.RoundtripTime; }
        }
    }

}
