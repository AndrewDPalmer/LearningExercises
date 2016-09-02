using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RankingCalculator.Models
{
    public class Team : IComparable<Team>
    {
		//a list of all matches this team played in - not really used in the current implementation 
        public List<Match> Matches = new List<Match>();

		//the Teams Score for the season
		private int _score = 0;
		public int Score
		{
			get
			{
				return _score;
			}
		}

		//the teams Name
        private string _teamName;
        public string TeamName { get { return _teamName; } }


        public Team(string teamName)
        {
            _teamName = teamName;
        }

		//used for the list sort (C# handles sorting of lists<Team> as long as iComparable is implemented on the class)
        public int CompareTo(Team compare)
        {
            int result = this.Score.CompareTo(compare.Score) * -1;
            if (result != 0)
                return result;
            else
                return this.TeamName.CompareTo(compare.TeamName);
        }

		//Output our team and score according to prescribed formatting
        public override string ToString()
        {
            return _teamName + ", " + Score.ToString() + " pts";
        }

		//gives a team points for a game
		public void AwardPoints(int pointsScored)
		{
			if (pointsScored > 0) {
				_score += pointsScored;
			}
		}
    }
}
