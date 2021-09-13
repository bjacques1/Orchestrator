using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Orchestrator2012.Workflow
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class OrchestratorCategoryAttribute : CategoryAttribute
    {
        public OrchestratorCategoryAttribute()
            : base("Orchestrator 2012 Activities")
        {
        }
    }
}
