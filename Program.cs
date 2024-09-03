using CsvHelper;
using CsvHelper.Configuration;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralBetCSVParser
{
    internal class Program
    {

        static void Main(string[] args)
        {
            //string argument = args[0];
            //string filePath = string.Empty; 
            //switch (argument)
            //{
            //    case "colombia":
            //        filePath = @"colombia-categoria-primera-a-matches-2022-to-2022-stats.csv";
            //        break;
            //    default:
            //        Console.WriteLine("Incorrect input or fixture not found");
            //        break;
            //}

            float margin = 2.5f;
            string filePath = @"england-national-league-matches-2023-to-2024-stats.csv";
            string pairsPath = @"Pairs.csv";
            IEnumerable<GoalReadRecord> goalRecords = null;
            IEnumerable<TodayPairs> pairsRecords = null;
            List<GoalWriteRecord> goalWriteRecords = new List<GoalWriteRecord>();
            List<Pairs> writePairs = new List<Pairs>();
            List<TodayPairs> writeTodayPairs = new List<TodayPairs>();
            List<KeyValuePair<int, int>> goalResults = new List<KeyValuePair<int, int>>();
            List<KeyValuePair<string, string>> teamResults = new List<KeyValuePair<string, string>>();
            List<int> BTTS = new List<int>();
            List<int> UnderOver = new List<int>();
            List<string> CorrectHomeTeams = new List<string>();
            List<string> CorrectAwayTeams = new List<string>();

            using (var reader = File.OpenText(filePath))
            {
                var csvParser = new CsvParser(reader, CultureInfo.InvariantCulture);
                CsvReader r = new CsvReader(csvParser);
                goalRecords = r.GetRecords<GoalReadRecord>().ToArray();

            }



            foreach (var goalRecord in goalRecords)
            {
                goalResults.Add(new KeyValuePair<int, int>(goalRecord.home_team_goal_count, goalRecord.away_team_goal_count));
                teamResults.Add(new KeyValuePair<string, string>(goalRecord.home_team_name, goalRecord.away_team_name));
            }

            foreach (var value in goalResults)
            {
                if (value.Key > 0 && value.Value > 0)
                {
                    BTTS.Add(1);
                }
                else
                {
                    BTTS.Add(0);
                }
                if (value.Key + value.Value > margin)
                {
                    UnderOver.Add(1);
                }
                else
                {
                    UnderOver.Add(0);
                }
            }

            var configPersons = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };

            using (var writer = new StreamWriter("Matches3.csv"))
            using (var csv = new CsvWriter(writer, configPersons))
            {
                int index = 0;
                foreach (var goalRecord in goalRecords)
                {
                    goalWriteRecords.Add(new GoalWriteRecord
                    {
                        HomeTeam = goalRecord.home_team_name,
                        AwayTeam = goalRecord.away_team_name,
                        HomeGoalCount = goalRecord.home_team_goal_count,
                        AwayGoalCount = goalRecord.away_team_goal_count,
                        BTTS = BTTS[index],
                        OverUnder = UnderOver[index]
                    });
                    //var writeRecord = new List<GoalWriteRecord>()
                    //{
                    //    new GoalWriteRecord { HomeTeam = goalRecord.home_team_name, AwayTeam = goalRecord.away_team_name, HomeGoalCount = goalRecord.home_team_goal_count, AwayGoalCount = goalRecord.away_team_goal_count, BTTS = BTTS[index] }

                    //};
                    index++;

                }
                csv.WriteRecords(goalWriteRecords);
            }

            var items = new List<object>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load("https://www.futbol24.com/national/England/National-League/2023-2024/fixtures/");
            //var homeTeams = document.DocumentNode.SelectNodes("//team4");
            string HomeTeam = "team2";
            string AwayTeam = "team3";
            string timeZoneText = "green timezonebar";
            string dateTimeNode = "data dymek timezone";
            string loadingContainer = "table loadingContainer";
            List<string> HomeTeams = new List<string>();
            List<string> AwayTeams = new List<string>();




            int teamsIndex = 0;

            foreach (HtmlNode node in document.DocumentNode.SelectNodes("//div[@class='" + loadingContainer + "']"))
            {
                foreach (var titleNode in node.SelectNodes(".//td[@class='" + dateTimeNode + "']"))
                {
                    string date = DateTime.Now.ToString("dd.MM.yyyy");

                    //string date = "25.05.2023";




                    var title = titleNode.GetAttributeValue("title", "");
                    string todayDate = title.Substring(0, title.LastIndexOf(date) + 10);
                    if (todayDate != date)
                    {
                        break;
                    }

                    var tdHomeNodes = node.SelectNodes("//td[@class='" + HomeTeam + "']");
                    var tdAwayNodes = node.SelectNodes("//td[@class='" + AwayTeam + "']");
                    string home = tdHomeNodes[teamsIndex].InnerText;
                    string away = tdAwayNodes[teamsIndex].InnerText;
                    HomeTeams.Add(home);
                    AwayTeams.Add(away);
                    teamsIndex++;
                }

            }



            var dictionary = HomeTeams.Zip(AwayTeams, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

            var configPairs = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };





            using (var writer = new StreamWriter("Pairs.csv"))
            using (var csv = new CsvWriter(writer, configPairs))
            {
                foreach (var dic in dictionary)
                {

                    writePairs.Add(new Pairs
                    {
                        HomeTeam = dic.Key,
                        AwayTeam = dic.Value

                    });




                }


                csv.WriteRecords(writePairs);
            }


            if (File.Exists(pairsPath))
            {
                using (var reader = File.OpenText(pairsPath))
                {
                    var csvParser = new CsvParser(reader, CultureInfo.InvariantCulture);
                    CsvReader r = new CsvReader(csvParser);
                    pairsRecords = r.GetRecords<TodayPairs>().ToArray();

                }
            }

            //TODO: Possible null dereference. Consider inspecting 'pairsRecords'.
            foreach (var rec in pairsRecords) 
            {
                var homePair = rec.HomeTeam;
                var awayPair = rec.AwayTeam;

                homePair = getCorrectTeam(homePair);
                awayPair = getCorrectTeam(awayPair);
                if (!string.IsNullOrEmpty(homePair) && !string.IsNullOrEmpty(awayPair))
                {
                    if (!CorrectHomeTeams.Contains(homePair) && !CorrectAwayTeams.Contains(awayPair))
                    {
                        CorrectHomeTeams.Add(homePair);
                        CorrectAwayTeams.Add(awayPair);
                    }
                }

            }

            //CorrectHomeTeams.RemoveAll(string.IsNullOrEmpty);
            //CorrectAwayTeams.RemoveAll(string.IsNullOrEmpty);

            var newDictionary = CorrectHomeTeams.Zip(CorrectAwayTeams, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);


          

            var configTodayPairs = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
            };



            using (var writer = new StreamWriter("TodayPairs.csv"))
            using (var csv = new CsvWriter(writer, configTodayPairs))
            {
                foreach (var dic in newDictionary)
                {

                    writeTodayPairs.Add(new TodayPairs
                    {
                        HomeTeam = dic.Key,
                        AwayTeam = dic.Value

                    });




                }


                csv.WriteRecords(writeTodayPairs);
            }

            //string getHomeTeam(string team)
            //{
            //    foreach (var homeTeam in teamResults)
            //    {
            //        if (homeTeam.Key.Contains($"{team}"))
            //        {
            //            double similarity = Math.Round(findSimilarity(homeTeam.Key, team), 2);
            //            if (similarity >= -0.09)
            //            {
            //                team = homeTeam.Key;
            //                break;
            //            }

            //        }
            //    }

            //    return team;
            //}

            //string getAwayTeam(string team)
            //{
            //    foreach (var awayTeam in teamResults)
            //    {
            //        if (awayTeam.Value.Contains($"{team}"))
            //        {
            //            double similarity = Math.Round(findSimilarity(awayTeam.Value, team), 2);
            //            if (similarity >= -0.09)
            //            {
            //                team = awayTeam.Key;
            //                break;
            //            }

            //        }
            //    }

            //    return team;
            //}

            string getCorrectTeam(string team)
            {
                foreach (var correctTeam in teamResults)
                {
                    if (correctTeam.Key.Contains(team.Substring(0, Math.Min(team.Length, 6))) ||
                        correctTeam.Value.Contains(team.Substring(0, Math.Min(team.Length, 6))))
                    {
                        double homeSimilarity = Math.Round(findSimilarity(correctTeam.Key, team), 2);
                        double awaySimilarity = Math.Round(findSimilarity(correctTeam.Value, team), 2);

                        if (homeSimilarity > awaySimilarity)
                        {
                            team = correctTeam.Key;
                            break;
                        }
                        else
                        {
                            team = correctTeam.Value;
                            break;
                        }
                    }
                }
                return team;
            }

        }






        //public static string GetHomeTeam(IEnumerable<GoalReadRecord> Teams, string team)
        //{
        //    string correctTeam = string.Empty;

        //    foreach (var homeTeam in Teams)
        //    {
        //        double similarity = Math.Round(findSimilarity(homeTeam.home_team_name, team), 2);
        //        if (similarity >= -0.09)
        //        {
        //            correctTeam = homeTeam.home_team_name;
        //            break;

        //        }
        //    }

        //    return correctTeam;
        //}

        //public static string GetAwayTeam(IEnumerable<GoalReadRecord> Teams, string team)
        //{
        //    string correctTeam = string.Empty;
        //    foreach (var awayTeam in Teams)
        //    {
        //        double similarity = Math.Round(findSimilarity(awayTeam.away_team_name, team), 2);
        //        if (similarity >= -0.09)
        //        {
        //            correctTeam = awayTeam.away_team_name;
        //            break;

        //        }

        //    }
        //    return correctTeam;
        //}

        public static int getEditDistance(string X, string Y)
        {
            int m = X.Length;
            int n = Y.Length;

            int[][] T = new int[m + 1][];
            for (int i = 0; i < m + 1; ++i)
            {
                T[i] = new int[n + 1];
            }

            for (int i = 1; i <= m; i++)
            {
                T[i][0] = i;
            }
            for (int j = 1; j <= n; j++)
            {
                T[0][j] = j;
            }

            int cost;
            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    cost = X[i - 1] == Y[j - 1] ? 0 : 1;
                    T[i][j] = Math.Min(Math.Min(T[i - 1][j] + 1, T[i][j - 1] + 1),
                            T[i - 1][j - 1] + cost);
                }
            }

            return T[m][n];
        }

        public static double findSimilarity(string x, string y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentException("Strings must not be null");
            }

            double maxLength = Math.Max(x.Length, y.Length);
            if (maxLength > 0)
            {
                // optionally ignore case if needed
                return (maxLength - minDistance(x, y)) / maxLength;
            }
            return 1.0;
        }

        public static double findSimilarityMultiLevenschtein(string x, string y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentException("Strings must not be null");
            }

            double maxLength = Math.Max(x.Length, y.Length);
            if (maxLength > 0)
            {
                // optionally ignore case if needed
                return (maxLength - minDistance(x, y)) / maxLength;
            }
            return 1.0;

        }

        public static int minDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[][] dp = new int[n + 1][];
            for (int i = 0; i <= n; i++)
            {
                dp[i] = new int[m + 1];
                for (int j = 0; j <= m; j++)
                {
                    dp[i][j] = 0;
                    if (i == 0)
                    {
                        dp[i][j] = j;
                    }
                    else if (j == 0)
                    {
                        dp[i][j] = i;
                    }
                }
            }
            s = string.Empty + s;
            t = string.Empty + t;

            int cost;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {

                    cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    //dp[i][j] = Math.Min(Math.Min(dp[i - 1][j] + 1, dp[i][j - 1] + 1), dp[i - 1][j - 1] + 1);
                    dp[i][j] = 1 + Math.Min(Math.Min(dp[i - 1][j] + 1, dp[i][j - 1]),
                            dp[i - 1][j - 1] + cost);

                }

            }


            return dp[n][m];
        }

        public static int MultipleLevenshteinDistance(string x, string y)
        {
            // ako su nizovi x ili y null, baciti izuzetak
            if (x == null || y == null)
            {
                throw new ArgumentException("Strings must not be null");
            }

            // dužine nizova x i y
            int n = x.Length;
            int m = y.Length;

            // ako je jedan od nizova prazan, vratiti drugi niz
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }

            // inicijalizovati matricu dp sa dimenzijama n + 1 x m + 1
            int[][] dp = new int[n + 1][];
            for (int i = 0; i <= n; i++)
            {
                dp[i] = new int[m + 1];
            }

            // inicijalizovati prvi red i prvi stupac matrice dp
            for (int i = 0; i <= n; i++)
            {
                dp[i][0] = i;
            }
            for (int j = 0; j <= m; j++)
            {
                dp[0][j] = j;
            }

            // računati višestruku Levenshteinovu udaljenost između nizova x i y
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    // cijena zamjene znakova x[i - 1] i y[j - 1]
                    int cost = x[i - 1] == y[j - 1] ? 0 : 1;

                    // minimizovati trošak između troška zamjene, troška brisanja i troška dodavanja
                    dp[i][j] = Math.Min(Math.Min(dp[i - 1][j] + 1, dp[i][j - 1] + 1), dp[i - 1][j - 1] + cost);
                }
            }

            // vratiti vrijednost iz posljednjeg elementa matrice dp
            return dp[n][m];
        }

        public static double CosineSimilarity(string x, string y)
        {
            // ako su nizovi x ili y null, baciti izuzetak
            if (x == null || y == null)
            {
                throw new ArgumentException("Strings must not be null");
            }

            // razdvojiti nizove x i y u riječi
            string[] xWords = x.Split(' ');
            string[] yWords = y.Split(' ');

            // ako su nizovi xWords ili yWords prazni, vratiti 0
            if (xWords.Length == 0 || yWords.Length == 0)
            {
                return 0;
            }

            // inicijalizirati dictionary za pojedinačne riječi u nizovima xWords i yWords
            var xDict = new Dictionary<string, int>();
            var yDict = new Dictionary<string, int>();

            // za svaku riječ u nizovima xWords i yWords,
            // ažurirati broj pojavljivanja u dictionaryju
            foreach (string word in xWords)
            {
                if (xDict.ContainsKey(word))
                {
                    xDict[word]++;
                }
                else
                {
                    xDict[word] = 1;
                }
            }
            foreach (string word in yWords)
            {
                if (yDict.ContainsKey(word))
                {
                    yDict[word]++;
                }
                else
                {
                    yDict[word] = 1;
                }
            }

            // inicijalizirati skalarne produkte i norme vektora x i y
            double xDotProduct = 0;
            double yDotProduct = 0;
            double xNorm = 0;
            double yNorm = 0;

            // za svaku riječ u oba dictionaryja,
            // ažurirati skalarne produkte i norme vektora x i y
            foreach (string word in xDict.Keys)
            {
                if (yDict.ContainsKey(word))
                {
                    xDotProduct += xDict[word] * yDict[word];
                }
                xNorm += xDict[word] * xDict[word];
            }
            foreach (string word in yDict.Keys)
            {
                yNorm += yDict[word] * yDict[word];
            }

            // ako su norme vektora x i y jednake 0, vratiti 0
            if (xNorm == 0 || yNorm == 0)
            {
                return 0;
            }

            // vratiti kosinus sličnosti između vektora x i y
            return xDotProduct / (Math.Sqrt(xNorm) * Math.Sqrt(yNorm));
        }
    }
}
            

