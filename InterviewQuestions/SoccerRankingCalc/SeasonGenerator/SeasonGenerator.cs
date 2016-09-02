using System;
using System.Collections.Generic;
using System.IO;


namespace SeasonGenerator
{
	class MainClass
	{
		//I'm assuming 2 args and I'm not doing error checking so this prog would be easy to kill
		//arg1 = and integer you can enter anything but I limit it between 2 and 100
		//arg2 = a file path
		public static void Main (string[] args)
		{
			int teamCount = int.Parse (args [0]);
			if (teamCount < 2)
				teamCount = 2;  //if less than 2 teams I will do 2 if greater than 100 I am doing 100 teams
			else if (teamCount > 100)
				teamCount = 100;

			int minScore = -5;
			int maxScore = 100;

			List<string> teamPrefixes = new List<string>(){"Angry", "Red", "", "Blue", "Sad", "Orange", "Pink", "Green", "Crazy", "Beligerent"};
			List<string> teamSuffixes = new List<string>(){"Bears", "Birds", "Rhinos", "Octopi", "People", "Losers", "Aliens", "Robots", "Pandas", "Products of your imagination"};
			List<string> teamNames = new List<string> ();

			Random rand = new Random ();
			for (int i = 0; i < teamCount; i++) 
			{

				string prefix = teamPrefixes [rand.Next (0, teamPrefixes.Count)];
				string suffix = teamSuffixes [rand.Next(0, teamSuffixes.Count)];
				teamNames.Add((prefix + " " + suffix).Trim());
			}

			using (StreamWriter file = new StreamWriter(args[1], false))
			{
				foreach (string team in teamNames) 
				{
					foreach (string opponent in teamNames) 
					{
						if (team == opponent) 
						{
							Console.WriteLine ("Bye week!");  // this solves for the team playing itself in this loop as well as any randomly generated duplicate team names
						} 
						else 
						{
							string output = team + " " + rand.Next(minScore, maxScore).ToString() + ", " + opponent + " " + rand.Next(minScore, maxScore).ToString();
							Console.WriteLine (output);
							file.WriteLine (output);
						}
					}
				}
			}
		}
	}
}




