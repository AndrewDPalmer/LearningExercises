using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RankingCalculator.Models
{
    public class Season
    {
		//Lists of teams and matches that happen in this season
        public List<Team> Teams = new List<Team>();
        public List<Match> Matches = new List<Match>();

        public Season()
        {
        }
			
        //assuming well formed input as explained in the readme
        public void AddMatch(string matchAsText)
        {
            //parse our string into something useful
            string[] splitMatch = Regex.Split(matchAsText, ", ");
            string left = splitMatch[0];
            string right = splitMatch[1];

            Team leftTeam = AddTeam(getTeamName(left));
            Team rightTeam = AddTeam(getTeamName(right));

            //create our match
            Match match = new Match(leftTeam, getScore(left), rightTeam, getScore(right));
            Matches.Add(match);

            Teams.Sort();
        }

		//gets a team name from the input
        private string getTeamName(string input)
        {
            string[] split = input.Split(' ');
            return input.Replace(" " + split[split.Length - 1], "");
        }

		//gets a score from the input
        private int getScore(string input)
        {
            string[] split = input.Split(' ');
            return int.Parse(split[split.Length - 1]);
        }

		/// <summary>
		/// Adds the team. But only if the team hasn't already been added
		/// </summary>
		/// <returns>the new Team if created or the existing team if already in the collection</returns>
		/// <param name="teamName">Team name.</param>
        private Team AddTeam(string teamName)
        {
            Team team = Teams.Find(x => x.TeamName.Equals(teamName));

            if (team != null)
            {
                return team;
            }
            else
            {
                team = new Team(teamName);
                Teams.Add(team);
                return team;
            }
        }


    }
}
