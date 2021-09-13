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
    /// An Activity that increments a date/tiem value.
    /// </summary>
    [Activity("Increment DateTime")]
    public class IncrementDate : IActivity
    {
        public enum IncrementWhat { Milliseconds, Seconds, Minutes, Hours, Days, Months, Years }

        public void Design(IActivityDesigner designer)
        {
            designer.AddInput("Increment What").WithEnumBrowser(typeof(IncrementWhat));
            designer.AddInput("Increment");
            designer.AddInput("Date/Time").WithDateTimeBrowser();
            designer.AddOutput("Old Date/Time").AsDateTime();
            designer.AddOutput("New Date/Time").AsDateTime();
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            DateTime oldDateTime = request.Inputs["Date/Time"].AsDateTime();
            response.Publish("Old Date/Time", oldDateTime);

            DateTime newDateTime;
            IncrementWhat what = request.Inputs["Increment What"].As<IncrementWhat>();
            IRuntimeValue increment = request.Inputs["Increment"];

            switch (what)
            {
                case IncrementWhat.Milliseconds:
                    newDateTime = oldDateTime.AddMilliseconds(increment.AsDouble());
                    break;

                case IncrementWhat.Seconds:
                    newDateTime = oldDateTime.AddSeconds(increment.AsDouble());
                    break;

                case IncrementWhat.Minutes:
                    newDateTime = oldDateTime.AddMinutes(increment.AsDouble());
                    break;

                case IncrementWhat.Hours:
                    newDateTime = oldDateTime.AddHours(increment.AsDouble());
                    break;

                case IncrementWhat.Days:
                    newDateTime = oldDateTime.AddDays(increment.AsDouble());
                    break;

                case IncrementWhat.Months:
                    newDateTime = oldDateTime.AddMonths(increment.AsInt32());
                    break;

                case IncrementWhat.Years:
                    newDateTime = oldDateTime.AddYears(increment.AsInt32());
                    break;                

                default:
                    newDateTime = oldDateTime;
                    break;
            }

            response.Publish("New Date/Time", newDateTime);
        }
    }
}
