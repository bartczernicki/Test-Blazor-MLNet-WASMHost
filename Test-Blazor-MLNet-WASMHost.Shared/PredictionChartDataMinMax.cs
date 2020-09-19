using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Blazor_MLNet_WASMHost.Shared
{
    public class PredictionChartDataMinMax
    {
        public string Algorithm { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public int SeasonPlayed { get; set; }
    }
}
