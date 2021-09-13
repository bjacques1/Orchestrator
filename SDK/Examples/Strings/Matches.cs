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
using System.Text.RegularExpressions;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Strings
{
    /// <summary>
    /// An Orchestrator service class that searches the specified input string 
    /// for an occurrence of the specified regular expression,
    /// </summary>
    [Activity]
    public class Matches
    {
        private String sequence;
        private String pattern;

        [ActivityInput]
        public String Sequence { set { sequence = value; } }

        [ActivityInput]
        public String Pattern { set { pattern = value; } }

        [ActivityOutput("Matches", Description = "Does the sequence match the pattern.")]
        public Boolean IsMatch
        {
            get
            {
                Regex regex = new Regex(pattern);
                return regex.Match(sequence).Success;
            }
        }
    }
}
