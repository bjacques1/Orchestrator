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

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Strings
{
    [Activity]
    class Replace
    {
        private string input;
        private string replacement;
        private string pattern;
        private int maxReplace = -1;
        private int startAt = 0;

        [ActivityInput]
        public string Input
        {
            set { input = value; }
        }

        [ActivityInput("Maximum Replacements", Optional=true)]
        public int MaxReplace
        {
            set { maxReplace = value; }
        }

        [ActivityInput("Start At", Optional = true)]
        public int StartAt
        {
            set { startAt = value; }
        }

        [ActivityInput]
        public string Replacement
        {
            set { replacement = value; }
        }

        [ActivityInput]
        public string Pattern
        {
            set { pattern = value; }
        }

        [ActivityOutput("New String")]
        public string NewString
        {
            get
            {
                Regex regex = new Regex(pattern);
                return regex.Replace(input, replacement, maxReplace, startAt);
            }
        }        
    }
}
