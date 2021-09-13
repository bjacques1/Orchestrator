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

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Math
{
    /// <summary>
    /// An Activity that performs rounding.    
    /// </summary>
    /// <remarks>
    /// This class demonstrates how enumerations can be used to provided
    /// browseable options for ActivityInput properties.
    /// </remarks>
    [Activity]
    public class Round
    {
        private MidpointRounding mode;
        private int decimals;
        private Decimal value;

        [ActivityInput("Mode"), ActivityOutput("Mode")]
        public MidpointRounding Algorithm
        {
            get { return mode;  }
            set { mode = value;}
        }

        [ActivityInput, ActivityOutput]
        public int Decimals
        {
            get { return decimals; }
            set { decimals = value; }
        }

        [ActivityInput, ActivityOutput]
        public Decimal Value
        {
            get { return value; }
            set { this.value = value; }
        }
        
        [ActivityOutput("Rounded Value")]
        public Decimal RoundedValue
        {
            get { return Decimal.Round(value, decimals, mode); }    
        }
    }
}
