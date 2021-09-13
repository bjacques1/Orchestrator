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
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Math
{
    /// <summary>
    /// An Activity class that returns random numbers.
    /// </summary>
    /// <remarks>
    /// This class demonstrates out a properties marked with ActivityOutput
    /// attribute can return a collection of items.
    /// </remarks>
    [Activity]
    public class RandomNumbers
    {
        private UInt16 howMany;
        private Int32 seed;

        [ActivityInput]
        public UInt16 HowMany
        {
            set { howMany = value; }
        }                
        
        [ActivityInput]
        public Int32 Seed
        {
            set { seed = value; }
        }

        [ActivityOutput]                    
        public IEnumerable<Int32> Result
        {
            get
            {
                Random random = new Random(seed);
                for (UInt16 n = 0; n < howMany; ++n)
                {
                    yield return random.Next();
                }                           
            }
        }
    }
}
