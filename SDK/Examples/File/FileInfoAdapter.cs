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

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.File
{
    /// <summary>
    /// An ActivityData class that adaptes the .NET <c>FileInfo</c> class
    /// so that it's properties can be published.
    /// </summary>
    [ActivityData]
    public class FileInfoAdapter
    {
        private readonly FileInfo info;

        public FileInfoAdapter(FileInfo info)
        {
            this.info = info;
        }

        [ActivityOutput(Description="The date/time the file was created")]
        [ActivityFilter]
        public DateTime Created
        {
            get { return info.CreationTime; }
        }

        [ActivityOutput(Description="The length of the file in bytes"), ActivityFilter]        
        public long Length
        {
            get { return info.Length; }
        }

        [ActivityOutput(Description="The date/time the file was created")]
        [ActivityFilter]
        public DateTime Modified
        {
            get { return info.LastWriteTime; }
        }

        [ActivityOutput(Description="The date/time the file was accessed"), ActivityFilter]
        public DateTime Accessed
        {
            get { return info.LastAccessTime; }
        }

        [ActivityOutput(Description="Is the file read-only"), ActivityFilter]
        public Boolean ReadOnly
        {
            get { return (info.Attributes & FileAttributes.ReadOnly) != 0; }
        }

        [ActivityOutput(Description="The name of the file"), ActivityFilter]
        public String Name
        {
            get { return info.Name; }
        }
        [ActivityOutput(Description="The full name of the file"), ActivityFilter]
        public String FullName
        {
            get { return info.FullName; }
        }
    }

}
