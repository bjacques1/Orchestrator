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
    /// An Orchestrator service class that returns the larger of 
    /// two double-precision floating-point numbers.
    /// </summary>    
    [Activity("Maximum")]
    public class Maximum
    {
        private Double val1 = 0.0;
        private Double val2 = 0.0;        

        [ActivityInput("Value 1")]
        public Double Val1
        {
            set { val1 = value; }
        }

        [ActivityInput("Value 2")]
        public Double Val2
        {
            set { val2 = value; }
        }
        
        [ActivityOutput(Description = "The maximum of two numbers")]
        public Double Result
        {
            get { return System.Math.Max(val1, val2); }
        }

    }
}
