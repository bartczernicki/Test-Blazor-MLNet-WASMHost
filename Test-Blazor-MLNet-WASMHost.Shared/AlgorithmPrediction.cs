using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Blazor_MLNet_WASMHost.Shared
{
    public class AlgorithmPrediction
    {
        public string AlgorithmName { get; set; }

        public bool Prediction { get; set; }

        public float Probability { get; set; }
    }
}
