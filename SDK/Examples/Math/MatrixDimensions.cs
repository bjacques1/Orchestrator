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

namespace Microsoft.SystemCenter.Orchestrator.Integration.Examples.Math
{
    /// <summary>
    /// An Orchestrator Service Data class that represents the
    /// dimensions of a matrix.
    /// </summary>
    [ActivityData("Dimensions")]
    public class MatrixDimensions
    {
        private Byte rows = 0;
        private Byte columns = 0;        

        [ActivityInput, ActivityOutput]
        public Byte Rows
        {
            get { return rows; }
            set { rows = value; }
        }
        
        [ActivityInput, ActivityOutput]
        public Byte Columns
        {
            get { return columns; }
            set { columns = value; }
        }
        
    }
}
