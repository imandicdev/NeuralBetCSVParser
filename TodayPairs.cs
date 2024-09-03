using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralBetCSVParser
{
    public class TodayPairs
    {
        [Index(0)]
        public string HomeTeam { get; set; }
        [Index(1)]
        public string AwayTeam { get; set; }
    }
}
