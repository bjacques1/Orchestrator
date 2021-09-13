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
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Math
{
    /// <summary>
    /// An Orchestrator service class that returns the logarithm of a 
    /// specified number in a specified base.
    /// </summary>
    [Activity]
    public class Log
    {
        private Double number = 0.0;
        private Double newBase = 0.0;

        [ActivityInput]        
        public Double Number
        {
            set{ number = value; }
        }
        
        [ActivityInput]
        public Double Base
        {
            set { newBase = value; }
        }

        [ActivityOutput(Description = "Log of number X in base base Y.")]
        public Double Result
        {
            get { return System.Math.Log(number, newBase); }
        }    
    }
}
