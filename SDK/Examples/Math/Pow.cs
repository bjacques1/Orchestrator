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
    /// An Orchestrator service class that returns a specified number raised 
    /// to the specified power.    
    /// </summary>
    [Activity]
    public class Pow
    {
        private Double number = 0.0;
        private Double power = 0.0;

        [ActivityInput]
        public Double Number
        {
            set { number = value; }
        }
        
        [ActivityInput]
        public Double Power
        {
            set { power = value; }
        }

        [ActivityOutput(Description = "The number raised to by a power.")]
        public Double Result
        {
            get { return System.Math.Pow(number, power); }
        }        
    }
}
