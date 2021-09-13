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

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Date
{
    /// <summary>
    /// An Activity that returns the day of week from a date.
    /// </summary>
    [Activity("Days of the week")]
    public class DayOfTheWeek
    {
        private DateTime date = DateTime.Now;

        [ActivityInput]
        public DateTime Date
        {
            set{ date = value; }
        }
        [ActivityOutput(Description = "The day of the week (Sunday = 0, Saturday = 6")]
        public DayOfWeek Day
        {
            get { return date.DayOfWeek;  }
        }                       
    }
}
