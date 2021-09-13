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
    /// An Activity that finds the length of a file.
    /// </summary>
    [Activity("File Length")]
    public class FileLength
    {
        private FileInfo fileInfo;

        [ActivityInput("File Path")]
        public FileInfo FilePath
        {
            set{ fileInfo = value; }
        }

        [ActivityOutput(Description = "Length in bytes")]
        public Int64 Length{ 
            get
            {                
                if (fileInfo.Exists)
                {
                    return fileInfo.Length;
                }

                throw new ApplicationException("File not found");
            } 
        }        
    }
}
