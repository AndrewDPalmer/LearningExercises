using System;
using System.Collections.Generic;
using System.IO;
using RankingCalculator.Models;

namespace RankingCalculator
{
    public class RankingCalculator
    {
        static void Main(string[] args)
        {
            // if args are invalid print an error and exit
            if(!isArgsValid(args))
            { 
                return;
            }
            string inputFilePath = args[0];

            //open our file and read it line by line
            StreamReader fileReader;
            try
            {
                fileReader = new StreamReader(inputFilePath);
            }
            catch (Exception exc)
            {
                //file connection problem, throw error and exit
                Console.WriteLine("File connection error: " + exc.Message);
                Console.WriteLine("Please use a valid input file path as the parameter");
                return;
            }

            //set up our season
            Season thisSeason = new Season();

            //now read the file line by line
            string matchData;
            while ((matchData = fileReader.ReadLine()) != null)
            {
				if (!string.IsNullOrWhiteSpace (matchData)) 
				{
					thisSeason.AddMatch (matchData);
				}
            }

            //output to a file if we have a second argument
            int index = 0;
            if(args.Length == 2)
            {
                using (StreamWriter file = new StreamWriter(args[1], false))
                {
                    foreach (Team t in thisSeason.Teams)
                    {
                        file.WriteLine((index + 1).ToString() + ". " + t.ToString());
						index++;
                    }
                }
            }

            //output to the console
            index = 0;
            foreach (Team t in thisSeason.Teams)
            {
                Console.WriteLine((index + 1).ToString() + ". " + t.ToString());
                index++;
            }
        }

        public static bool isArgsValid(string[] args)
        {
            if(args.Length < 1 || args.Length > 2)
            {
                Console.Write(
                    "RankingCalculator expects either 1 or 2 parameters" +
                    "\nArgument 1: REQUIRED: An input file path." +
                    "\nArgument 2: OPTIONAL: An output file path" +
                    "\nIf only one argument is given it will be assumed to be an input file and output will be made to the Console"
                    );
                return false;
            }
            return true;
        }
    }
}
