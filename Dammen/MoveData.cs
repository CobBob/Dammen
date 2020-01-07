using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dammen
{
    class MoveData
    {
        //Because it is unwise to store both the number of strikes and the
        //list of moves in the same List<int[]>, a separate dataclass is used
        //now that I think about it, why didn't I do this before?

        public List<List<int>> moveList { get; private set; }
        public int highestStrike { get; private set; }

        public List<char[]> endPlayFieldsList { get; private set; }

        /*
        public MoveData()
        {
            moveList = new List<int[]>();
            strikeList = new int[2] { 0, 0 };
            highestStrike = -1; // no moves available
        }
        */
        public MoveData(int location, char[] playField)
        {
            moveList = new List<List<int>>();
            endPlayFieldsList = new List<char[]>();
            highestStrike = -1; // no moves available
            PossiblePieceMoves(location, playField);
        }

        private MoveData(int location, char[] playField, int highestStrikePassed)
        {
            moveList = new List<List<int>>();
            endPlayFieldsList = new List<char[]>();
            highestStrike = highestStrikePassed; // no moves available
            PossiblePieceMoves(location, playField);
        }

        public MoveData(string playerColor, char[] playField)
        {
            moveList = new List<List<int>>();
            endPlayFieldsList = new List<char[]>();
            highestStrike = -1; // no moves available
            PossibleMoves(playerColor, playField);
        }

        //private VarInitiate()
        //{
        //
        //}

        // Creates the list of available moves for a given piece
        public void PossiblePieceMoves(int location, char[] playField)
        {
            char currentCharofPiece = playField[location];//PlayFieldCharFromLocation(location, playField);
            char newCharofPiece;
            int newLocation;

            char newCharofPieceStrike;
            int newLocationStrike;
            int highestStrikeBefore = highestStrike;
            //int highestStrikeThisIteration = highestStrikeBefore;

            string colorOfPiece = currentCharofPiece.ToString().ToLower();
            string colorOfOponent = "w";
            string colorOfnewLocation;
            if (colorOfPiece == "w")
            {
                colorOfOponent = "b";
            }

            // Data used for storing stuff for striking
            char[] playFieldAfterStrike = new char[100];

            // maximum number of steps that can be taken by a piece
            //  max steps: 10 in case of dame, 1 in case of disk
            int maxSteps = 1;
            if (currentCharofPiece == 'W'
                || currentCharofPiece == 'B')
            {
                maxSteps = 10;
            }

            // If a strikeList is passed, what is the highest strike of the previous step?
            //if (strikeList != null)
            //{
            //    highestStrike = strikeList.Count() - 1;
            //    highestStrikeBefore = highestStrike;
            //}

            foreach (Directions direction in Enum.GetValues(typeof(Directions)))
            {
                for (int i = 1; i <= maxSteps; i++)
                {
                    // Fisrt is the location valid and if there is a piece in the way can it be struck?

                    //  Find coordinates of new location
                    newLocation = GetNewLocation(direction, location, i);

                    //          If New location == 0
                    //              break out of this loop, invalid location
                    if (newLocation == 0)
                    {
                        break;
                    }

                    // What is present on the new location?
                    newCharofPiece = playField[newLocation];//PlayFieldCharFromLocation(newLocation, playField);

                    // What is its color?
                    colorOfnewLocation = newCharofPiece.ToString().ToLower();

                    //          if own piece
                    //              break out of this loop, own piece is in the way
                    if (colorOfPiece == colorOfnewLocation)
                    {
                        break;
                    }
                    //          If occupied with piece of other color
                    //              can it be struck?
                    else if (colorOfnewLocation == colorOfOponent)
                    {
                        // TO DO: striking logic

                        /*It should look like this:
                         * if the field after it is empty
                         * 
                         * Somehow the piece that is struck needs to have some data stored that it
                         * is struck, I cannot do this with the playfield that is entered.
                         * The solution is simple: pass a modified playfield to 
                         * MoveData(int location, char[,] playField)
                         * But to stop it from moving, another field is needed to indicate to 
                         * not start highest strike at -1, otherwise the algorithm might move a piece instead
                         *      this can be resolved by using the length of the strikeList
                         * furthermore the previous location needs to be added or woven into the new move list.
                         *      Some kind of strikelist needs to be passed.
                         * Create MoveData(int location, char[,] playField, int[] strikeList)
                         * 
                         * it will create another movedata class
                         * its inputs need to be the playfield, and location of the pawn
                         * ideally I would use the PossiblePieceMoves function again
                         * how to keep track of the highest strike?
                         *
                         *
                         * 
                         * 
                         */
                        //Oh, wait how to handle the Dam? Simple: for loop



                        for (int j = 1; j <= maxSteps; j++)
                        {

                            newLocationStrike = GetNewLocation(direction, location, i + j);

                            //          If NewLocationStrike == 0
                            //              break out of this loop, invalid location
                            if (newLocationStrike == 0)
                            {
                                break;
                            }

                            newCharofPieceStrike = playField[newLocationStrike];

                            //If the location does not contain an empty space, break the for-loop
                            if (newCharofPieceStrike != ' ')
                            {
                                break;
                            }
                            //A strike (must) occur

                            // If the previous highest strike was below 1, then clear the movelist of regular moves
                            // Then set the highest strike before to 0;
                            if (highestStrike < 1)
                            {
                                moveList.Clear();
                                endPlayFieldsList.Clear();
                                highestStrikeBefore = 0;
                            }

                            //A strike is added to highest strike, unless it is already higher
                            if (highestStrike < (highestStrikeBefore + 1))
                            {
                                highestStrike = (highestStrikeBefore + 1);
                            }

                            //Can the piece be used to strike more pieces?

                            //Create the new playfield to pass
                            Array.Copy(playField, playFieldAfterStrike, 100);
                            playFieldAfterStrike[newLocation] = 's';
                            playFieldAfterStrike[newLocationStrike] = playFieldAfterStrike[location];
                            playFieldAfterStrike[location] = ' ';

                            // Check if there are more strikes to be found
                            MoveData NextStrikeMoveData = new MoveData(newLocationStrike, playFieldAfterStrike, highestStrikeBefore + 1);

                            //If a Higher strike is found, remove the last lists and use the new ones
                            if (NextStrikeMoveData.highestStrike > highestStrike)
                            {
                                moveList.Clear();
                                endPlayFieldsList.Clear();

                                foreach (List<int> locationList in NextStrikeMoveData.moveList)
                                {
                                    moveList.Add(new List<int>() { location });
                                    moveList[moveList.Count() - 1].AddRange(locationList);
                                }
                                AppendEndPlayField(NextStrikeMoveData.endPlayFieldsList);

                                highestStrike = NextStrikeMoveData.highestStrike;
                            }
                            else if (highestStrike == NextStrikeMoveData.highestStrike)
                            {
                                if (NextStrikeMoveData.moveList.Any())
                                {
                                    foreach (List<int> locationList in NextStrikeMoveData.moveList)
                                    {
                                        moveList.Add(new List<int>() { location });
                                        moveList[moveList.Count() - 1].AddRange(locationList);
                                    }
                                    AppendEndPlayField(NextStrikeMoveData.endPlayFieldsList);
                                }
                                else
                                {
                                    moveList.Add(new List<int> { location, newLocationStrike });
                                    AddFieldToEndPlayFieldList(playFieldAfterStrike);
                                }
                            }
                            continue;
                        }
                        break;
                    }
                    //          If empty, 
                    //              Unless it is a black disc moving Up,
                    //              Or a white piece moving down
                    //                  add to list of possible moves
                    else if (newCharofPiece == ' ')
                    {
                        // If there are possible strikes, do not bother with adding
                        // possible moves, continue with loop when there is a DAM
                        if (highestStrike > 0)
                        {
                            continue;
                        }

                        if ((currentCharofPiece == 'b'
                            && (direction == Directions.NW
                            || direction == Directions.NE))
                            || (currentCharofPiece == 'w'
                            && (direction == Directions.SW
                            || direction == Directions.SE)))
                        {
                            break;
                        }
                        highestStrike = 0;
                        moveList.Add(new List<int> { location, newLocation });
                        AddFieldToEndPlayFieldList(playField);
                        endPlayFieldsList[endPlayFieldsList.Count() - 1][newLocation] = playField[location];
                        endPlayFieldsList[endPlayFieldsList.Count() - 1][location] = ' ';

                        continue;
                    }
                    else if (newCharofPiece == 's')
                    {
                        break;
                    }
                    Console.WriteLine("uh ohh....");
                    Console.ReadLine();
                    // If the end is reached, an unexpected error occured somewhere
                }
            }
        }
        //A helper function
        private void AddFieldToEndPlayFieldList(char[] playFieldPassed)
        {
            endPlayFieldsList.Add(new char[100]);
            for (int k = 0; k < 100; k++)
            {
                endPlayFieldsList[endPlayFieldsList.Count() - 1][k] = playFieldPassed[k];
            }
        }
        private void AppendEndPlayField(List<char[]> playFieldListPassed)
        {
            foreach(char[] playFieldPassed in playFieldListPassed)
            {
                AddFieldToEndPlayFieldList(playFieldPassed);
            }
        }
        private void PossibleMoves(string playerColor, char[] playField)
        {
            // Check the possible moves of every piece on the board
            // If a piece is found with the same color as the specified color
            // check its possible moves
            char pieceOnLocation;
            for (int location = 1; location < 99; location++)
            {
                pieceOnLocation = playField[location];
                if (playerColor == pieceOnLocation.ToString().ToLower())
                {
                    MoveData movesOfPiece = new MoveData(location, playField);
                    //If the highestStrike at this location is the same add it to the list
                    if (movesOfPiece.highestStrike == highestStrike)
                    {
                        moveList.AddRange(new List<List<int>>(movesOfPiece.moveList));
                        AppendEndPlayField(movesOfPiece.endPlayFieldsList);
                    }
                    //If the highestStrike at this location is higher, clear moveData and use moveList of Piece
                    else if (movesOfPiece.highestStrike > highestStrike)
                    {
                        moveList.Clear();
                        endPlayFieldsList.Clear();
                        moveList.AddRange(new List<List<int>>(movesOfPiece.moveList));
                        AppendEndPlayField(movesOfPiece.endPlayFieldsList);
                        highestStrike = movesOfPiece.highestStrike;
                    }
                    movesOfPiece.Clear();
                    //Else do nothing and continue the loop
                }
            }
        }

        public void Clear()
        {
            moveList.Clear();
            endPlayFieldsList.Clear();
            highestStrike = -1;
        }

        private int GetNewLocation(Directions direction, int currentLocation, int steps)
        {
            //this function is used to quickly get a new location based on the direction,
            //the current location and the amount of steps in a certain direction
            //It will return a 0 if the new location is invalid

            int newLocation;

            switch (direction)
            {
                case Directions.NE:
                    {
                        newLocation = currentLocation - steps * 9;
                        if (newLocation / 10 != (currentLocation / 10 - steps)
                            || newLocation < 1)
                        {
                            return 0;
                        }
                        else
                        {
                            return newLocation;
                        }
                    }
                case Directions.NW:
                    {
                        newLocation = currentLocation - steps * 11;
                        if (newLocation / 10 != (currentLocation / 10 - steps)
                            || newLocation < 1)
                        {
                            return 0;
                        }
                        else
                        {
                            return newLocation;
                        }
                    }
                case Directions.SE:
                    {
                        newLocation = currentLocation + steps * 11;
                        if (newLocation / 10 != (currentLocation / 10 + steps)
                            || newLocation > 98)
                        {
                            return 0;
                        }
                        else
                        {
                            return newLocation;
                        }
                    }
                case Directions.SW:
                    {
                        newLocation = currentLocation + steps * 9;
                        if (newLocation / 10 != (currentLocation / 10 + steps)
                            || newLocation > 98)
                        {
                            return 0;
                        }
                        else
                        {
                            return newLocation;
                        }
                    }
            }
            return 0;
        }

        /*
        private char PlayFieldCharFromLocation(int location, char[,] playField)
        {
            // A helper function to quickl get the character of the playfield
            // using the tabindex
            return playField[location % 10, location / 10];
        }*/
    }
}
