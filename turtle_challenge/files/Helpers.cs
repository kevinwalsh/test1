using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LGChecked_TurtleChallenge
{
    class Helpers
    {
        public static SettingsModel GetSettings(string settingsFile)
        {
            var model = new SettingsModel();

            try
            {
                model.gridX = int.Parse(GetValueForParameter(settingsFile, "GridX"));
                model.gridY = int.Parse(GetValueForParameter(settingsFile, "GridY"));
                model.startX = int.Parse(GetValueForParameter(settingsFile, "StartX"));
                model.startY = int.Parse(GetValueForParameter(settingsFile, "StartY"));
                model.exitX = int.Parse(GetValueForParameter(settingsFile, "ExitX"));
                model.exitY = int.Parse(GetValueForParameter(settingsFile, "ExitY"));
                model.startDirection = Helpers.ParseEnum<Direction>(GetValueForParameter(settingsFile, "StartDirection"));
                model.mineX = new List<int>();
                model.mineY = new List<int>();
                model.mines = new List<Position>();
                var mines = settingsFile.Split(new string[] { "MineX" }, StringSplitOptions.RemoveEmptyEntries);            
                                        // the above works but removes "mineX"; will need to implement hacky workaround below for now
                
                foreach (string mine in mines)
                {
                    if (mine.Contains("MineY"))
                    {
                        /*
                        model.mineX.Add(int.Parse(GetValueForParameter(string.Concat("MineX", mine, 1), "MineX")));
                        model.mineY.Add(int.Parse(GetValueForParameter(mine, "MineY")));
                        */
                                    //  neater to implement a "mine" object
                        model.mines.Add(new Position(
                            int.Parse(GetValueForParameter(string.Concat("MineX", mine, 1), "MineX")),
                            int.Parse(GetValueForParameter(mine, "MineY"))
                            ));
                        //model.mineX.Add(GetValueForParameter(mine.Replace("=","MineX="), "MineX"));       // even hackier original
                    }
                }
            }
            catch (Exception e)
            {
                var myMessage = "<<UNKNOWN>>   EXCEPTION OCCURED while initiating settings parameters";
                PrintException(e, myMessage);           
            }
            return model;
        }
        public static string GetValueForParameter(string file, string parameter)
        {
            int startPosition = file.IndexOf(parameter) + (parameter + "=").Length;
            int endPosition = file.Substring(startPosition).IndexOf(",") + startPosition;

            if (startPosition < endPosition)                // As expected
            {
                return file.Substring(startPosition, endPosition - startPosition);
            }
            else
            {
                var temp2 = file.Substring(startPosition);
                return file.Substring(startPosition);           // ENDOFFILE, i.e. if last value doesnt end with comma
            }
        }


        public static List<MoveType> GetMovesList(string movesFile)
        {
            List<MoveType> moves = new List<MoveType>();
            var sequenceSplit = movesFile.Split(',');
            foreach (string s in sequenceSplit)
            {
                MoveType m = Helpers.ParseEnum<MoveType>(s.Replace(",", ""));       
                                    // we split values on commas, so we also need to remove them to determine enum
                moves.Add(m);
            }
            return moves;
        }
        
//  ------------------------------------------------------ more System-level/ generic helpers
        public static T ParseEnum<T>(string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }

            catch (ArgumentException e)
            {
                var myMessage = "ENUM EXCEPTION OCCURED \n" +
                    "ArgumentException: No valid enum found from provided String:      (" + value + ") \n" +
                    "(Check settings/moves file for typos, spaces or final commas)";
                PrintException(e, myMessage);               
                return default(T);      //never reached
            }
        }
        
        public static string ReadFileFromDebugFolder(string filename)
        {
            string msg = null;
            try
            {
                msg = File.ReadAllText(filename);
                return msg;
            }
            catch (FileNotFoundException e)
            {
                var myMessage = "FILENOTFOUND EXCEPTION OCCURED \n" +
                    "Note: Please ensure the appropriate settings files are placed in the Bin\\DEBUG folder";
                PrintException(e, myMessage);
            }
            catch (Exception e)
            {
                var myMessage = "<<UNKNOWN>>   EXCEPTION OCCURED";
                PrintException(e, myMessage);
            }
            return msg;
        }
        
        public static void PrintException(Exception e, string myMessage)
        {
            Console.WriteLine(myMessage);
            //  PrintFullExceptionText(e);              // too much information for user, only use for debug purposes
            Helpers.KeepTerminalOpen();
            Environment.Exit(0);            // Exit application early
        }

        public static void PrintFullExceptionText(Exception e)
        {
            Console.WriteLine(e);
        }

        public static void KeepTerminalOpen()
        {
            Console.WriteLine("\n\n" + new string('*', 50));
            Console.WriteLine("\nEND OF APPLICATION - Press any key to exit:");
            Console.ReadKey();      // Keep console open to view output
        }
    }
}
