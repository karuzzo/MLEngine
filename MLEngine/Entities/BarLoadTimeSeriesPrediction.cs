using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLEngine.Entities
{
    class BarLoadTimeSeriesPrediction
    {
        public float[] Forecasted { get; set; }

        public float[] LowerBound{ get; set; }

        public float[] UpperBound{ get; set; }
    }
}
