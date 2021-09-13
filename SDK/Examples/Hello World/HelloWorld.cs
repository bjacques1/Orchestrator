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
using System.Linq;
using System.Text;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.HelloWorld
{
    [Activity("Hello World")]
    public class HelloWorldActivity
    {
        private string hwinput;

        [ActivityInput("Hello World Input")]
        public string HWInput
        {
            set { hwinput = value; }
        }

        [ActivityOutput("Hello World Output")]
        public string HWOutput
        {
            get
            {
                char[] arr = hwinput.ToCharArray();
                Array.Reverse(arr);
                return new string(arr);
            }
        }

    }
}
