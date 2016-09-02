using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RankingCalculator.Models
{
    public class Match
    {
        private Team _team1;
        private int _score1;
        private Team _team2;
        private int _score2;
		private bool _arePointsAwarded = false;

        public Match(Team team1, int score1, Team team2, int score2)
        {
            _team1 = team1;
            _team1.Matches.Add(this);
            _score1 = score1;
            _team2 = team2;
            _team2.Matches.Add(this);
            _score2 = score2;

			AwardPoints ();
        }

		//these compares are safe because I implemented iComparable on class Team and based it on the score - a team is less than another if its score is lower or if its name is alphabetically earlier when there is a tie
        public bool IsTie
        {
            get
            {
                if (_score1 == _score2)
                {
                    return true;
                }
                else
                    return false;
            }
        }

		//these compares are safe because I implemented iComparable on class Team and based it on the score - a team is less than another if its score is lower
		/// <summary>
		/// Gets the winner.
		/// </summary>
		/// <value>The winner. returns null if a tie</value>
		public Team Winner
		{
			get
			{
				if (_score1 < _score2)
					return _team2;
				else if (_score1 > _score2)
					return _team1;
				else
					return null;
			}
		}

		//these compares are safe because I implemented iComparable on class Team and based it on the score - a team is less than another if its score is lower
		/// <summary>
		/// Gets the loser.
		/// </summary>
		/// <value>The loser. returns null if a tie</value>
		public Team Loser
		{
			get
			{
				if (_score1 < _score2)
					return _team1;
				else if (_score1 > _score2)
					return _team2;
				else
					return null;
			}
		}

		/// <summary>
		/// Awards the points.  returns true if successful, false if unsuccessful (i.e. already added)
		/// </summary>
		/// <returns><c>true</c>, if points were awarded, <c>false</c> otherwise.</returns>
		private bool AwardPoints()
		{
			if (_arePointsAwarded == true) {
				return false;
			}

			if (this.IsTie) 
			{
				_team1.AwardPoints (1);
				_team2.AwardPoints (1);
			} else 
			{
				Winner.AwardPoints (3);
				Loser.AwardPoints (0);
			}
			return true;
		}

    }
}
