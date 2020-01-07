using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace Dammen
{
    class BoardData
    {
        // Core data\
        public char[] playField { get; private set; }
        public string currentPlayer { get; private set; }

        private string currentOpponent;

        public MoveData currentValidMoves;
        public TieData currentTieData;

        private List<BoardData> boardDataValidMoves;

        private int scoreOfBoard;
        public List<int> bestMoveIndexForAI { get; private set; }

        public static List<string> textForDebuggingAi { get; private set; }

        public BoardData(InitialBoardStates chosenStart, string colorPlayerStart)
        {
            currentPlayer = colorPlayerStart;
            currentOpponent = "w";
            if (colorPlayerStart == "w")
            {
                currentOpponent = "b";
            }

            //Fill in the playField
            playField = new char[100];
            PlacePieces(chosenStart);

            //Find all current valid moves
            currentValidMoves = new MoveData(currentPlayer, playField);

            // Construct the DrawData
            currentTieData = new TieData();

            textForDebuggingAi = new List<string>();
        }
        private BoardData(char[] endPlayFieldPassed, BoardData boardDataPassed)
        {
            // Copy the boardData
            playField = new Char[100];
            for (int i = 0; i < 100; i++)
            {
                playField[i] = endPlayFieldPassed[i];
            }
            // The playField needs to be updated
            RemoveStrikedPieces();
            ChangeDisksIntoDammen();

            // Who is the current player and who is the opponent
            currentPlayer = boardDataPassed.currentPlayer;
            currentOpponent = boardDataPassed.currentOpponent;
            // Next Turn
            SwitchPlayer();

            //Find the current Valid Moves
            currentValidMoves = new MoveData(currentPlayer, playField);

            //Debugging Stuff 
            /*
            string writeToConsole = "";
            for (int i = 0; i < 100; i++)
            {
                if(i%10 == 0)
                {
                    writeToConsole += "\n";
                }
                writeToConsole += playField[i];
            }
            Console.WriteLine(writeToConsole);
            */

            //Copy the DrawData
            currentTieData = new TieData(boardDataPassed.currentTieData);
            UpdataTieData();
        }

        public void TrimMoveListToSelect(int selectedLocation)
        {
            currentValidMoves.Clear();
            currentValidMoves = new MoveData(selectedLocation, playField);
        }
        public void GetPossibleMoves()
        {
            currentValidMoves.Clear();
            currentValidMoves = new MoveData(currentPlayer, playField);
        }

        public void SwitchPlayer()
        {
            if (currentPlayer == "b")
            {
                currentPlayer = "w";
                currentOpponent = "b";
            }
            else
            {
                currentPlayer = "b";
                currentOpponent = "w";
            }
        }
        public void MovePiece(int oldLocation, int newLocation)
        {
            //Check if something got struck, and assign it as such
            int diffLoc = newLocation - oldLocation;
            foreach (int k in new int[] { 9, 11 })
            {
                if (diffLoc % k == 0)
                {
                    for (int i = 1; i <= Math.Abs(diffLoc / k); i++)
                    {
                        if (playField[oldLocation + i * k * (diffLoc / Math.Abs(diffLoc))].ToString().ToLower() == currentOpponent)
                        {
                            // A strike has occured
                            playField[oldLocation + i * k * (diffLoc / Math.Abs(diffLoc))] = 's';
                        }

                        // If this triggers something is wrong
                        if ((((i * k) % 9) == 0)
                            && (((i * k) % 11) == 0))
                        {
                            Console.WriteLine("Error in movePiece");
                        }
                    }
                }
            }
            //Move the piece
            playField[newLocation] = playField[oldLocation];
            playField[oldLocation] = ' ';
            return;
        }
        public void RemoveStrikedPieces()
        {
            for (int i = 0; i < 100; i++)
            {
                if (playField[i] == 's')
                {
                    playField[i] = ' ';
                }
            }
        }
        public void ChangeDisksIntoDammen()
        {
            for (int i = 1; i < 10; i += 2)
            {
                if (playField[i] == 'w')
                {
                    playField[i] = 'W';
                }
            }
            for (int i = 90; i < 99; i += 2)
            {
                if (playField[i] == 'b')
                {
                    playField[i] = 'B';
                }
            }
        }
        public bool CheckLoss()
        {
            // If there are no moves, the next player losses the game
            if (currentValidMoves.highestStrike < 0)
            {
                return true;
            }
            return false;
        }
        public bool CheckTie()
        {
            return currentTieData.isTie;
        }
        public void UpdataTieData()
        {
            currentTieData.UpdateTieData(playField, currentValidMoves, currentPlayer);
        }
        public int EvaluateTurnEndPlayFields(int depth, Stopwatch timerPassed)
        {
            //Black tries to minimize the score
            //White tries to maximize it

            boardDataValidMoves = new List<BoardData>();
            bestMoveIndexForAI = new List<int>();

            List<int> scoreOfEachMove = new List<int>();

            foreach (char[] endPlayField in currentValidMoves.endPlayFieldsList)
            {
                boardDataValidMoves.Add(new BoardData(endPlayField, this));
            }

            //Did the opponent Lose?
            for (int i = 0; i < currentValidMoves.endPlayFieldsList.Count(); i++)
            {
                if (boardDataValidMoves[i].CheckLoss())
                {
                    //If a move is found that would win the game for the AI
                    //The AI will always pick this move the funcion can then end

                    //Black minimizes and white maximizes
                    scoreOfBoard = 9001;
                    if (currentPlayer == "b")
                    {
                        scoreOfBoard = -9001;
                    }

                    bestMoveIndexForAI.Add(i);
                    return scoreOfBoard;
                }
            }

            //Is there a Tie
            for (int i = 0; i < currentValidMoves.moveList.Count(); i++)
            {
                if (boardDataValidMoves[i].CheckTie())
                {
                    bestMoveIndexForAI.Add(i);

                    if (currentPlayer == "b")
                    {
                        scoreOfEachMove.Add(20);
                    }
                    else
                    {
                        scoreOfEachMove.Add(-20);
                    }
                }
            }

            if (depth > 0)
            {
                //The algorithm will try to look further into the tree of possibilities
                //This means that some recursion is going to take place
                //to evaluate the next board states

                for (int i = 0; i < currentValidMoves.moveList.Count(); i++)
                {
                    //skip the boards that result in a tie or a win

                    if (bestMoveIndexForAI.Contains(i))
                    {
                        continue;
                    }
                    scoreOfEachMove.Add(boardDataValidMoves[i].EvaluateTurnEndPlayFields(depth - 1, timerPassed));
                    bestMoveIndexForAI.Add(i);
                }
            }
            else
            {
                //Evaluate the payField bassed on the number of pieces remainning
                for (int i = 0; i < currentValidMoves.moveList.Count(); i++)
                {
                    //skip the boards that result in a tie
                    if (bestMoveIndexForAI.Contains(i))
                    {
                        continue;
                    }
                    scoreOfEachMove.Add(
                        EvaluateBoard(boardDataValidMoves[i].playField, boardDataValidMoves[i].currentPlayer));
                    bestMoveIndexForAI.Add(i);
                }
            }

            //What is the highest or lowest score?
            scoreOfBoard = FindMaxOrMin(scoreOfEachMove, (currentPlayer == "w"));

            //Used to store Data for debugging Purposses

            //************************
            //Remove all indexes that do not  have this value
            for (int i = 0; i < scoreOfEachMove.Count(); i++)
            {
                if (scoreOfEachMove[i] != scoreOfBoard)
                {
                    scoreOfEachMove.RemoveAt(i);
                    bestMoveIndexForAI.RemoveAt(i);
                    i--;
                }
            }

            //************************

            return scoreOfBoard;

            int FindMaxOrMin(List<int> scoreListPassed, bool findMaxTrue)
            {
                if (findMaxTrue)
                {
                    int currentMax = -9000;
                    foreach (int score in scoreListPassed)
                    {
                        if (score > currentMax)
                        {
                            currentMax = score;
                        }
                    }
                    return currentMax;
                }
                else
                {
                    int currentMin = 9000;
                    foreach (int score in scoreListPassed)
                    {
                        if (score < currentMin)
                        {
                            currentMin = score;
                        }
                    }
                    return currentMin;
                }
            }
        }

        private int EvaluateBoard(char[] playFieldPassed, string playerColorPassed)
        {
            float scoreBlack = 0;
            float scoreWhite = 0;
            int result;

            foreach (char field in playFieldPassed)
            {
                switch (field)
                {
                    case 'b':
                        {
                            scoreBlack++;
                            break;
                        }
                    case 'w':
                        {
                            scoreWhite++;
                            break;
                        }
                    case 'B':
                        {
                            scoreBlack += 5;
                            break;
                        }
                    case 'W':
                        {
                            scoreWhite += 5;
                            break;
                        }
                }
            }

            result = (int)(100 * (scoreWhite - scoreBlack) / (scoreWhite + scoreBlack));

            /*
            if (playerColorPassed == "b")
            {
                result = (int) (100 * (scoreBlack / (scoreWhite + scoreBlack)));
            }
            else
            {
                result = (int) (100 * (scoreWhite / (scoreWhite + scoreBlack)));
            }*/

            return result;
        }

        private void PlacePieces(InitialBoardStates initialBoardState)
        {
            switch (initialBoardState)
            {
                case InitialBoardStates.STD:
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            playField[i] = ' ';

                            if ((((i % 20) < 10)
                            && i % 2 == 1)
                            || (((i % 20) >= 10)
                            && i % 2 == 0))
                            {
                                if (i > 60)
                                //Place white pieces
                                {
                                    playField[i] = 'w';
                                }
                                else if (i < 40)
                                {
                                    //Place black pieces
                                    playField[i] = 'b';
                                }
                            }
                        }
                        break;
                    }
                case InitialBoardStates.STRIKETEST1:
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            playField[i] = ' ';
                        }
                        playField[81] = 'w';
                        playField[54] = 'w';
                        playField[56] = 'w';
                        playField[65] = 'b';
                        playField[67] = 'b';
                        playField[47] = 'b';
                        playField[14] = 'b';
                        playField[3] = 'W';
                        //playField[70] = 'B';
                        break;
                    }
                case InitialBoardStates.STRIKETEST2:
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            playField[i] = ' ';
                        }
                        playField[1] = 'B';
                        playField[12] = 'w';
                        //playField[34] = 'w';
                        playField[58] = 'w';
                        playField[61] = 'w';
                        playField[63] = 'w';
                        playField[83] = 'w';
                        playField[27] = 'w';
                        break;
                    }
                case InitialBoardStates.STRIKETEST3:
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            playField[i] = ' ';
                        }
                        playField[45] = 'B';

                        playField[56] = 'w';
                        playField[34] = 'w';
                        playField[54] = 'w';
                        playField[36] = 'w';

                        playField[78] = 'w';
                        playField[12] = 'w';
                        playField[72] = 'w';
                        playField[18] = 'w';

                        break;
                    }
                case InitialBoardStates.TIETEST1:
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            playField[i] = ' ';
                        }
                        playField[10] = 'B';

                        playField[27] = 'W';
                        playField[18] = 'W';
                        playField[29] = 'w';
                        playField[36] = 'w';

                        //playField[78] = 'w';
                        //playField[12] = 'w';
                        //playField[72] = 'w';
                        //playField[18] = 'w';

                        break;
                    }
                case InitialBoardStates.AITEST1:
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            playField[i] = ' ';
                        }

                        playField[1] = 'b';
                        playField[3] = 'b';
                        playField[5] = 'b';
                        playField[9] = 'b';
                        playField[10] = 'b';
                        playField[12] = 'b';
                        playField[14] = 'b';
                        playField[16] = 'b';
                        playField[18] = 'b';
                        playField[21] = 'b';
                        playField[38] = 'b';
                        playField[43] = 'b';
                        playField[49] = 'b';
                        playField[52] = 'b';
                        playField[47] = 'b';

                        playField[50] = 'w';
                        playField[61] = 'w';
                        playField[65] = 'w';
                        playField[67] = 'w';
                        playField[69] = 'w';
                        playField[70] = 'w';
                        playField[72] = 'w';
                        playField[81] = 'w';
                        playField[85] = 'w';
                        playField[87] = 'w';
                        playField[89] = 'w';
                        playField[90] = 'w';
                        playField[92] = 'w';
                        playField[94] = 'w';
                        playField[96] = 'w';
                        break;
                    }
            }

        }
    }
}
