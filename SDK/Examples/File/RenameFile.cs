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

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.File
{
    /// <summary>
    /// An Orchestrator service class that renames a file.
    /// </summary>
    [Activity("Rename File")]
    public class RenameFile
    {
        private String oldName = "";
        private String newName = "";

        [ActivityInput("Old Name")]
        public String OldName
        {
            set { oldName = value; }
        }
        
        [ActivityInput("New Name")]
        public String NewName
        {
            set{ newName = value; }
        }
        
        [ActivityMethod]
        public void Invoke()
        {
            System.IO.File.Move(oldName, newName);    
        }        
    }
}
