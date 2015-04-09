﻿using System.Collections.Generic;

namespace CodeMetricsLoader.Data
{
    public class DimTarget
    {
        public int TargetId { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public virtual List<DimModule> Modules { get; set; }

        public DimTarget()
        {
            Modules = new List<DimModule>();
        }
    }
}