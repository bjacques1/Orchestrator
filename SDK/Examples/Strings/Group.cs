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
    /// An Orchestrator service class that returns the captured substring 
    /// from the input string using a specified regular expression.
    /// </summary>
    [Activity]
    public class Group
    {
        private String sequence;
        private String pattern;
        private Int16 group;

        [ActivityInput]        
        public String Sequence{ set{ sequence = value; } }
        
        [ActivityInput]
        public String Pattern { set{ pattern = value; } }
        
        [ActivityInput("Group to Find")]
        public Int16 GroupToFind { set{ group = value; } }
        
        [ActivityOutput(Description = "The nth group.")]
        public String Found
        {
            get
            {
                Regex regex = new Regex(pattern);
                Match match = regex.Match(sequence);
                if (match.Success && group < match.Groups.Count)
                {
                    return match.Groups[group].Value;
                }
                
                return string.Empty;
            }
        }                
    }
}
