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
    /// An Orchestrator Service class that performs matrix addition.
    /// </summary>
    [Activity("Matrix Addition")]
    public class MatrixAddition : IActivity
    {
        private MatrixDimensions dimensions;

        [ActivityConfiguration]
        public MatrixDimensions Dimensions
        {
            get { return dimensions;  }
            set { dimensions = value; }
        }

        public void Design(IActivityDesigner designer)
        {
            for (Byte i = 0; i < Dimensions.Rows; ++i)
            {
                for (Byte j = 0; j < Dimensions.Columns; ++j)
                {                    
                    designer.AddInput(GetCell("A", i, j));
                    designer.AddInput(GetCell("B", i, j));
                    designer.AddOutput(GetCell("C", i, j)).AsNumber();
                }
            }
        }

        public void Execute(IActivityRequest request, IActivityResponse response)
        {
            for (Byte i = 0; i < Dimensions.Rows; ++i)
            {
                for (Byte j = 0; j < Dimensions.Columns; ++j)
                {
                    int a = request.Inputs[GetCell("A", i, j)].AsByte();
                    int b = request.Inputs[GetCell("B", i, j)].AsByte();
                    response.Publish(GetCell("C", i, j), a + b);
                }
            }
        }
                
        private String GetCell(String prefix, Byte row, Byte column)
        {
            return prefix + ( row + 1 ) + ( column + 1 );
        }
    }
}
