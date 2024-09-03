using CsvHelper.Configuration.Attributes;


namespace NeuralBetCSVParser
{
    public struct GoalReadRecord
    {
        
        public string home_team_name { get; set; }
        
        public string away_team_name { get; set; }
        
        public int home_team_goal_count { get; set; }
        
        public int away_team_goal_count { get; set; }


    }
}
