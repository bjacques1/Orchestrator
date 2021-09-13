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
using System.Text;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Strings
{
    /// <summary>
    /// An Activity that splits a string based on specified separators.
    /// </summary>
    [Activity]
    public class SplitStringy 
    {
        private string text;
        private string separators;
        private StringSplitOptions options;

        [ActivityInput]
        public string Text
        {
            set { text = value; }
        }

        [ActivityInput]
        public string Separators
        {
            set { separators = value; }
        }

        [ActivityInput("Remove Empty Entries", Default = true)]
        public bool RemoveEmpty
        {
            set { options = value ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None; }
        }

        [ActivityOutput, ActivityFilter]
        public string[] Items
        {
            get { return text.Split(separators.ToCharArray(), options); }
        }
    }
}
