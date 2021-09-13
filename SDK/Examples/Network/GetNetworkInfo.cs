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
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Network
{
    /// <summary>
    /// An Activity that returns information about the network adapters currently
    /// on the machine.
    /// </summary>
    [Activity("Get Network Info")]
    public class GetNetworkInfo : IActivity
    {
        public void Design(IActivityDesigner designer)
        {
            designer.AddOutput("ID");
            designer.AddOutput("Physical Address");
            designer.AddOutput("Description");
            designer.AddOutput("Name");
            designer.AddOutput("Operational Status");
            designer.AddOutput("Interface Type");
            designer.AddOutput("Speed").AsNumber();
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                Dictionary<string, object> publishedData = new Dictionary<string, object>();
                
                publishedData.Add("ID", adapter.Id);
                publishedData.Add("Physical Address", adapter.GetPhysicalAddress());
                publishedData.Add("Description", adapter.Description);
                publishedData.Add("Name", adapter.Name);
                publishedData.Add("Operational Status", adapter.OperationalStatus);
                publishedData.Add("Interface Type", adapter.NetworkInterfaceType);
                publishedData.Add("Speed", adapter.Speed);

                response.Publish(publishedData);
            }
        }
    }
}
