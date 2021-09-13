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
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Microsoft.SystemCenter.Orchestrator.Integration;

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.File
{
    /// <summary>
    /// An Activity that returns information about the files in a specified directory.
    /// </summary>
    [Activity]
    public class FileList : IActivity
    {
        private readonly static string PATH = "Path";
        private readonly static string PATTERN = "Pattern";
        private readonly static string NUM_FILES = "Number of Files";
      
        public void Design(IActivityDesigner designer)
        {
            designer.AddInput(PATH).WithFileBrowser();
            designer.AddInput(PATTERN).WithDefaultValue("*.*");
            designer.AddOutput(NUM_FILES).AsNumber();
            designer.AddCorellatedData(typeof (FileInfoAdapter));
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            DirectoryInfo path = request.Inputs[PATH].As<DirectoryInfo>();
            string pattern = request.Inputs[PATTERN].AsString();
            IEnumerable files = FindFiles(path, pattern);
            int numFiles = response.WithFiltering().PublishRange(files);
            response.Publish(NUM_FILES, numFiles);               
        }

        private IEnumerable<FileInfoAdapter> FindFiles(DirectoryInfo path, string pattern)
        {
            foreach(FileInfo info in path.GetFiles(pattern))
                yield return new FileInfoAdapter(info);
        }        
    }
}
