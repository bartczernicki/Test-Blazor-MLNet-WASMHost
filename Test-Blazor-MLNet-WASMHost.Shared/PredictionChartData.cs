using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Blazor_MLNet_WASMHost.Shared
{
    public class PredictionChartData
    {
        public string Algorithm { get; set; }

        public double InductedToHallOfFameProbability { get; set; }

        public double OnHallOfFameBallotProbability { get; set; }

        public int SeasonPlayed { get; set; }
    }
}
