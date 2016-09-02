using NUnit.Framework;
using System;
using RankingCalculator.Models;

namespace RankingCalculator.Test
{
	[TestFixture ()]
	public class RankingCalcTest
	{
		[Test ()]
		public void TestTeam ()
		{
			Team team = new Team ("TeamName");
			Assert.AreEqual(team.ToString(), "TeamName, 0 pts");
			Assert.AreEqual (team.Score, 0);

			//add points and test
			team.AwardPoints (12);
			Assert.AreEqual("TeamName, 12 pts", team.ToString());
			Assert.AreEqual (12, team.Score);

			//award negative points (I expect the number not to change - negative points are not ok in a season ranking)
			team.AwardPoints (-2);
			Assert.AreEqual("TeamName, 12 pts", team.ToString());
			Assert.AreEqual (12, team.Score);
		}

		[Test ()]
		public void TestMatchTie()
		{
			//Test a Tie
			Team team1 = new Team ("Team1");
			Team team2 = new Team ("Team2");
			Match match = new Match (team1, 3, team2, 3);
			Assert.AreEqual (1, team1.Score);
			Assert.AreEqual (1, team2.Score);
			Assert.IsNull (match.Winner);
			Assert.IsNull (match.Loser);
			Assert.IsTrue (match.IsTie);
		}

		[Test ()]
		public void TestMatchTeam1win()
		{
			//Test the first team winnning
			Team team1 = new Team ("Team1");
			Team team2 = new Team ("Team2");
			Match match = new Match (team1, 6, team2, 3);
			Assert.AreEqual (3, team1.Score);
			Assert.AreEqual (0, team2.Score);
			Assert.IsNotNull (match.Winner);
			Assert.IsNotNull (match.Loser);
			Assert.IsFalse (match.IsTie);
		}

		[Test ()]
		public void TestMatchTeam2Win()
		{
			//Test the second team winning
			Team team1 = new Team ("Team1");
			Team team2 = new Team ("Team2");
			Match match = new Match (team1, 3, team2, 6);
			Assert.AreEqual (0, team1.Score);
			Assert.AreEqual (3, team2.Score);
			Assert.IsNotNull (match.Winner);
			Assert.IsNotNull (match.Loser);
			Assert.IsFalse (match.IsTie);
		}

		[Test ()]
		public void TestMatchNegativeScores()
		{
			//test with negative scores - negative game scores are ok in the match but a team can never have a negative season score
			Team team1 = new Team ("Team1");
			Team team2 = new Team ("Team2");
			Match match = new Match (team1, -3, team2, 3);
			Assert.AreEqual (0, team1.Score);
			Assert.AreEqual (3, team2.Score);
			Assert.IsNotNull (match.Winner);
			Assert.IsNotNull (match.Loser);
			Assert.IsFalse (match.IsTie);
		}

		[Test ()]
		public void TestSeason()
		{
			//I am assuming good inoput so not testing for all possible handling of bad input...

			//Test an empty Season
			Season season = new Season ();
			Assert.AreEqual (0, season.Teams.Count);
			Assert.AreEqual (0, season.Matches.Count);

			//Add a match
			season.AddMatch ("team1 3, team2 3");
			Assert.AreEqual (2, season.Teams.Count);
			Assert.AreEqual (1, season.Matches.Count);

			//add another match with an existing team and a new team
			season.AddMatch ("team1 2, team3 3");
			Assert.AreEqual (3, season.Teams.Count);
			Assert.AreEqual (2, season.Matches.Count);

			//add another match with both existing
			season.AddMatch("team1 2, team2 2");
			Assert.AreEqual (3, season.Teams.Count);
			Assert.AreEqual (3, season.Matches.Count);

		}
	}
}

