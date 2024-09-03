using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralBetCSVParser
{
    public struct GoalWriteRecord
    {
        [Index(0)]
        public string HomeTeam { get; set; }
        [Index(1)]
        public string AwayTeam { get; set; }
        [Index(2)]
        public int HomeGoalCount { get; set; }
        [Index(3)]
        public int AwayGoalCount { get; set; }
        [Index(4)]
        public int BTTS { get; set; }
        [Index(5)]
        public int OverUnder { get; set; }
    }
}
