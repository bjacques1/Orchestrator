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
using System.IO;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.File
{
    /// <summary>
    /// An Activity that copies a file.
    /// </summary>
    [Activity("Copy File")]
    public class CopyFile
    {
        private FileInfo source;
        private FileInfo destination;
        private bool overwrite;
        
        [ActivityInput, ActivityOutput]
        public FileInfo Source
        {
            set { source = value; }
            get { return source; }
        }

        [ActivityInput, ActivityOutput]
        public FileInfo Destination
        {
            set { destination = value; }
            get { return destination;  }
        }
        
        [ActivityInput(Default=false)]
        public Boolean Overwrite
        {
            set { overwrite = value; }
        }

        [ActivityMethod]
        public void Run()
        {
            source.CopyTo(destination.FullName, overwrite);
        }
    }
}
