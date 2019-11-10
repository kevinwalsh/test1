using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LGChecked_TurtleChallenge
{
    /*  *****************************************************************************************
         README:
           Tasks required:
                - Read in a settings file
                    - This will contain grid size, starting location, starting direction, end location, 0-N mine locations
                - Read in a sequence file
                    - This will contain a finite sequence of instructions (either Move(Forward), TurnL, TurnR)
                    - We should expect multiple sequences in a single file
                - Package the exercise file as an .exe
                - Specify the game-settings & moves files as inputs at runtime, from command line
                - Determine result of move sequence

           Extras:
                - Data models
                - Enums
                - Enum parser (exception handling for "no matching enum")
                - variable board size
                - multiple mines
                - default filenames
                - Lots of Error Handling
                    - File not found
                    - No specified input file
                    - Minor errorHandling for file contents (enums not as expected, not all settings parameters found, etc;
                        - Will terminate application gracefully   
                - Movement/ Board Edges: Application will handle edges/ outOfBounds
                    - "Illegal moves" will be simply ignored and skipped
                - Info given on move count required for escape/ mine triggering
                
                - Moves sequence specifically chosen to demonstrate robustness of program
                    - Seq 1: Spin counterclockwise/ left x5;    
                                                        //  correct 360 degree movement through enums
                    - Seq 2: Spin clockwise/ right x5;    
                                                        //  correct 360 degree movement through enums
                    - Seq 3: Move up off topleft of board, move left off topleft of board;      
                                                        //  Correct positioning rules; turtle position is never set < 0 
                    - Seq 4: Move right to end of board, 
                            move down to end of board,           
                            Turn Right and go to exit;          
                                           // turtle position is never set > board length,    AND turtle escapes with 1 move left   
                    - Seq 5: Turn around, go down board, turn left, walk into mine;         
                                                        //  Mine detection success with 4 moves left        AND game terminates before exit reached


           Assumptions made:
                - Settings files will generally be in correct/expected format
                    -comma-delimited, no spaces/ newlines, all values present.
                        - Errorhandling for a final comma in settings file
                    - Recommended file/settings ErrorHandling for future iterations:
                        - Ensure all positions values specified in settings are valid (>=0)
                        - Ensure all points fit within specified grid
                        - Ensure all occupied spaces are unique (turtle shouldnt start on exit or mine)     OR handle appropriately
                - REFERENCE DIRECTIONS
                    - Storing enums for North/South/East/West
                    - Defining "Grid co-ordinates" from [0,0] in top-left corner (Northwest);
                                i.e.    y++ to go south; x++ to go east

                - SETTINGS: All positions specified as array index format, 0 to (N-1)
                    - Except Grid will be specified as size/ length (N);
                    (e.g. 4x5 grid, first position = [0,0], last position = [3,4]) 
                - Move Sequence: No fancy algorithms. Simple dumb predefined command sequence.   
                

***************************************************************************************************************  
         */



    //---------------------- DATA MODELS
             // moved to Models file


    //---------------------- MAIN FUNCTION

    class Program
    {
        public static SettingsModel settings;

        static void Main(string[] args)
        {
            Console.WriteLine(new string('*', 50));
            Console.WriteLine("\n*********\t TURTLE CHALLENGE \t *********\n");
            Console.WriteLine(new string('*', 50));

            // Use pre-defined file names UNLESS two files are specified in command line;
            var settingsFileLocation = "game-settings.txt";
            var movesFileLocation= "moves.txt";
           
            if (args.Length == 2)
            {
                settingsFileLocation = args[0];
                movesFileLocation = args[1];
            }
            else
            {
                Console.WriteLine("*****************************\n    DEBUG:      INPUT SETTINGS FILES NOT SPECIFIED");
                Console.WriteLine("\t\tusing defaults (game-settings.txt, moves.txt)    \n**************************");
            }
            var settingsFile = Helpers.ReadFileFromDebugFolder(@settingsFileLocation);
            var movesFile = Helpers.ReadFileFromDebugFolder(movesFileLocation);

            // TASK DESCRIPTION specifies to expect multiple sequences of move scenarios in single file
            var movesFile_Split =movesFile.Split(new string[] { "Sequence=" }, StringSplitOptions.RemoveEmptyEntries);
            settings = Helpers.GetSettings(settingsFile);
            

            // INITIALIZE GRID + mines using settings file
            Tile[,] grid = new Tile[settings.gridX, settings.gridY];
            grid[settings.exitX, settings.exitY] = Tile.Exit;
            foreach(Position mine in settings.mines)
            {
                grid[mine.x, mine.y] = Tile.Mine;
            }

            // RUN GAME USING EACH MOVES LIST SEQUENCE
            int sequenceNum = 0;
            foreach(string seq in movesFile_Split)
            {
                sequenceNum++;
                Console.WriteLine("\n Move Sequence #" + sequenceNum);
                var moves = Helpers.GetMovesList(seq.Replace("{", "").Replace("}", ""));
                int resultCode = RunMoveSequence(moves, grid);          // we may want to use resultCode in future iterations
            }
           
            //  END OF APPLICATION
            Helpers.KeepTerminalOpen();     //Keep console terminal open to view outputs
        }

//------------------------------------------------------------------------------------------------------------------
// -------------------------    GAME-SPECIFIC LOGIC/ functions  ----------------------------------------------------
//------------------------------------------------------------------------------------------------------------------

        static int RunMoveSequence(List<MoveType> moves, Tile[,] grid)
        {
            int movesExecuted = 0;
            Turtle turtle = ResetTurtlePosition(settings);

            foreach (MoveType m in moves)
            {
                movesExecuted++;
                ExecuteMove(turtle, grid, m);

                bool b = CheckForExitOrMine(grid[turtle.pos.x, turtle.pos.y]);
                if (b == true)
                {
                    return FinaliseGameScenario(grid[turtle.pos.x, turtle.pos.y], movesExecuted, moves.Count());      
                }
            }
            // Reached end of moves list, "still in danger""
            Console.WriteLine("0% STAMINA! (turtle still alive but hasn't escaped) (" + movesExecuted + " moves completed)");
            return 0;

        }

        static void ExecuteMove(Turtle turtle, Tile[,] grid, MoveType m)
        {
            switch (m)
            {
                case MoveType.Forward:
                    MoveTurtle(turtle, grid);
                    break;
                case MoveType.TurnL:
                    TurnTurtle(turtle, false);
                    break;
                case MoveType.TurnR:
                    TurnTurtle(turtle, true);
                    break;
            }
        }
        
        static Turtle ResetTurtlePosition(SettingsModel settings)
        {
            return new Turtle(settings.startX, settings.startY, settings.startDirection);
        }

        static void MoveTurtle(Turtle turtle, Tile[,] grid)
        {
            Position newPos = turtle.Move();
            if (newPos.x >= 0 && newPos.x < grid.GetLength(0)
                    && newPos.y >= 0 && newPos.y < grid.GetLength(1))
            {
                // Only update position when we have confirmed new Position is within grid boundaries
                turtle.pos = newPos;
            }
                // ELSE ignore movement
        }

        static void TurnTurtle(Turtle turtle, bool clockwise)
        {
            int i = (int)turtle.dir;
            i+= clockwise? 1: -1;                // change to next/prev enum based on boolean
            if (i < 0) { i = Enum.GetValues(typeof(Direction)).Length - 1; }
            else if (i > Enum.GetValues(typeof(Direction)).Length -1 ) { i = 0; }               // loop to start/end of enums
            turtle.dir = (Direction) i;
        }

        static Boolean CheckForExitOrMine(Tile tile)
        {
            return tile == Tile.Exit || tile == Tile.Mine;
        }
        
        static int FinaliseGameScenario(Tile t,int movesExecuted, int totalMoves)
        {
            switch (t)
            {
                case Tile.Exit:
                    Console.WriteLine("SUCCESS! Turtle Escaped! (on turn " + movesExecuted + " of " + totalMoves+ ")");
                    return 1;
                case Tile.Mine:
                    Console.WriteLine("FAILURE!   Mine Triggered! R.I.P. Turtle  (on turn " + movesExecuted + " of " +totalMoves + ")");
                    return 2;
                default:
                    Console.WriteLine("\n\n\nERROR: Unexpected early termination. (Turtle not detected on either Mine or Exit tile)\n\n\n");
                    Console.WriteLine(" (on turn " + movesExecuted + " of " +totalMoves + ")");
                    return -1;
            }
        }
    }
}
