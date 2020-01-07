using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dammen
{
    class TieData
    {
        public bool whiteRequestsDraw;
        public bool blackRequestsDraw;

        private int numberOfTurnsPlayed;
        private int numberOfTurns3Piece;
        private int numberOfTurns5Turn;
        private int numberOfTurnsNoStrike;
        private List<char[]> boardHistory;

        public string tieMessage { get; private set; }

        public bool isTie { get; private set; }

        public TieData()
        {
            whiteRequestsDraw = false;
            blackRequestsDraw = false;

            numberOfTurnsPlayed = 0;
            numberOfTurns3Piece = 0;
            numberOfTurns5Turn = 0;
            numberOfTurnsNoStrike = 0;
            boardHistory = new List<Char[]>();

            tieMessage = "";
            isTie = false;
        }

        public TieData(TieData DrawDataPassed)
        {
            whiteRequestsDraw = DrawDataPassed.whiteRequestsDraw;
            blackRequestsDraw = DrawDataPassed.blackRequestsDraw;

            numberOfTurnsPlayed = DrawDataPassed.numberOfTurnsPlayed;
            numberOfTurns3Piece = DrawDataPassed.numberOfTurns3Piece;
            numberOfTurns5Turn = DrawDataPassed.numberOfTurns5Turn;
            numberOfTurnsNoStrike = DrawDataPassed.numberOfTurnsNoStrike;
            boardHistory = new List<Char[]>();
            boardHistory.AddRange(DrawDataPassed.boardHistory);

            tieMessage = DrawDataPassed.tieMessage;
            isTie = DrawDataPassed.isTie;
        }

        public void UpdateAgreedDraw()
        {
            if (numberOfTurnsPlayed > 0
                && whiteRequestsDraw
                && blackRequestsDraw)
            {
                isTie = true;
                tieMessage = "Tie: Players agreed to Tie.";
                //Console.WriteLine(tieMessage);
            }
        }
        public void UpdateTieData(char[] playFieldPassed, MoveData moveDataPassed, string ColorPassed)
        {
            //If for some reason an empty list is passed, end the function
            if(moveDataPassed.moveList.Count() < 1)
            {
                return;
            }

            numberOfTurnsPlayed++;
            bool onlyOneDam = true;
            int lastLocation = moveDataPassed.moveList[0][0];
            // Is there only one Dam? There is only one piece if there is only the first location
            foreach (List<int> locationList in moveDataPassed.moveList)
            {
                if (lastLocation != locationList[0]
                    || playFieldPassed[locationList[0]] == 'b'
                    || playFieldPassed[locationList[0]] == 'w')
                {
                    onlyOneDam = false;
                    break;
                }
                lastLocation = locationList[0];
            }

            if (onlyOneDam)
            {
                int numberOfOpponentDisks = 0;
                int numberOfOpponentDam = 0;

                char opponentDisc = 'b';
                char opponentDam = 'B';
                if (ColorPassed == "b")
                {
                    opponentDisc = 'w';
                    opponentDam = 'W';
                }

                foreach (char field in playFieldPassed)
                {
                    if (opponentDisc == field)
                    {
                        numberOfOpponentDisks++;
                    }
                    if (opponentDam == field)
                    {
                        numberOfOpponentDam++;
                    }
                }

                if (numberOfOpponentDam + numberOfOpponentDisks == 3)
                {
                    numberOfTurns3Piece++;
                    //both players moved 16 times in a stand of one Dam against 3 pieces (of which there is one Dam)
                    if (numberOfTurns3Piece >= 32)
                    {
                        isTie = true;
                        tieMessage = "Tie: 1 Dam versus three pieces, no change after "
                            + numberOfTurns3Piece + " turns.";
                        Console.WriteLine(tieMessage);
                        return;
                    }
                }
                else if (numberOfOpponentDam == 2
                    && numberOfOpponentDisks == 0)
                {
                    numberOfTurns5Turn++;
                    //both players moved 5 times in a stand of one Dam against two Dammen (of which there is one Dam)
                    if (numberOfTurns5Turn >= 5)
                    {
                        isTie = true;
                        tieMessage = "Tie: 1 Dam versus two pieces, no change after "
                            + 2 * numberOfTurns5Turn + " turns.";
                        Console.WriteLine(tieMessage);
                        return;
                    }
                }
                else if (numberOfOpponentDam == 1
                    && numberOfOpponentDisks == 0)
                {
                    //Both players have only 1 Dam left and the current player cannot immediatly strike the other player
                    if (moveDataPassed.highestStrike < 1)
                    {
                        isTie = true;
                        tieMessage = "Tie: Only 1 dam left with no strike.";
                        Console.WriteLine(tieMessage);
                        return;
                    }
                }
            }


            char[] playFieldToSave = new char[100];
            Array.Copy(playFieldPassed, playFieldToSave, 100);
            boardHistory.Insert(0, playFieldToSave);
            //Array.Copy(playFieldPassed, 0, playFieldToSave);
            //boardHistory.Add();
            //boardHistory.Add(playFieldPassed);

            //The rest of the Tie conditions should not happen early game, so they are removed
            //This is to prevent out of Index problems
            if (numberOfTurnsPlayed < 2)
            {
                return;
            }

            int numberOfSameBoards = 1;
            bool sameArray;
            //After three times the same position on the board.
            for (int j = 1; j < boardHistory.Count() - 1; j++)
            {
                sameArray = true;
                for (int k = 0; k < 100; k++)
                {
                    //toShow += numberOfTurnsPlayed + " " + k.ToString() + " " + boardHistory[i][k].ToString() + " " + boardHistory[j][k].ToString() + "\n";
                    //Console.WriteLine(toShow);
                    if (boardHistory[0][k] != boardHistory[j][k])
                    {
                        sameArray = false;
                        break;
                    }
                }
                if (sameArray == true)
                {
                    numberOfSameBoards++;
                }
                //if (&boardHistory[i].Equals(&boardHistory[j]))
                //{
                //    numberOfSameBoards++;
                //}
            }
            if (numberOfSameBoards >= 3)
            {
                isTie = true;
                tieMessage = "Tie, three times the same board in the last " +
                    15 + " turns";
                Console.WriteLine(tieMessage);
                //Console.WriteLine(numberOfTurnsPlayed);
                return;
            }
            if (boardHistory.Count() > 15)
            {
                boardHistory.RemoveAt(boardHistory.Count() - 1);
            }
            /*for (int i = 0; i < boardHistory.Count() - 3; i++)
            {
                numberOfSameBoards = 0;
                sameArray = true;
                for (int j = i + 1; j < boardHistory.Count(); j++)
                {
                    for (int k = 0; k < 100; k++)
                    {
                        //toShow += numberOfTurnsPlayed + " " + k.ToString() + " " + boardHistory[i][k].ToString() + " " + boardHistory[j][k].ToString() + "\n";
                        //Console.WriteLine(toShow);
                        if (boardHistory[i][k] != boardHistory[j][k])
                        {
                            sameArray = false;
                            break;
                        }
                    }
                    if (sameArray == true)
                    {
                        numberOfSameBoards++;
                    }
                    //if (&boardHistory[i].Equals(&boardHistory[j]))
                    //{
                    //    numberOfSameBoards++;
                    //}
                }
                if (numberOfSameBoards >= 3)
                {
                    isTie = true;
                    tieMessage = "Tie, three times the same board in the last " + 
                        4 + " turns";
                    Console.WriteLine(tieMessage);
                    return;
                }
            }
            if (boardHistory.Count() > 10)
            {
                boardHistory.RemoveAt(0);
            }*/

            //both players moved 25 times and did not make a strike in that period, neither did a disk move
            char[] oldBoardNoDam = new char[100];
            Array.Copy(boardHistory[boardHistory.Count() - 2], oldBoardNoDam, 100);
            char[] currentBoardNoDam = new char[100];
            Array.Copy(boardHistory[boardHistory.Count() - 1], currentBoardNoDam, 100);

            //char[] oldBoardNoDam = (char[])boardHistory[boardHistory.Count() - 2].Clone();
            //char[] currentBoardNoDam = (char[])boardHistory[boardHistory.Count() - 1].Clone();

            int oldNumberOfBlackPieces = 0;
            int oldnumberOfWhitePieces = 0;

            int currentNumberOfBlackPieces = 0;
            int currentNumberOfWhitePieces = 0;
            for (int i = 0; i < 100; i++)
            {
                //Count the pieces
                if (oldBoardNoDam[i] == 'W'
                    || oldBoardNoDam[i] == 'w')
                {
                    oldnumberOfWhitePieces++;
                }
                if (oldBoardNoDam[i] == 'B'
                    || oldBoardNoDam[i] == 'b')
                {
                    oldNumberOfBlackPieces++;
                }
                if (currentBoardNoDam[i] == 'W'
                    || currentBoardNoDam[i] == 'w')
                {
                    currentNumberOfWhitePieces++;
                }
                if (currentBoardNoDam[i] == 'B'
                    || currentBoardNoDam[i] == 'b')
                {
                    currentNumberOfBlackPieces++;
                }

                //Remove the Dammen
                if (oldBoardNoDam[i] == 'W'
                    || oldBoardNoDam[i] == 'B')
                {
                    oldBoardNoDam[i] = ' ';
                }
                if (currentBoardNoDam[i] == 'W'
                    || currentBoardNoDam[i] == 'B')
                {
                    currentBoardNoDam[i] = ' ';
                }
            }

            //if (oldBoardNoDam.Equals(currentBoardNoDam)
            //    && currentNumberOfBlackPieces == oldNumberOfBlackPieces
            //    && currentNumberOfWhitePieces == oldnumberOfWhitePieces)
            if (IsTheSameBoard(oldBoardNoDam, currentBoardNoDam)
                && currentNumberOfBlackPieces == oldNumberOfBlackPieces
                && currentNumberOfWhitePieces == oldnumberOfWhitePieces)
            {
                numberOfTurnsNoStrike++;
            }
            else
            {
                numberOfTurnsNoStrike = 0;
            }

            if (numberOfTurnsNoStrike > 25)
            {
                isTie = true;
                tieMessage = "Tie, No strike and no disk move after "
                    + numberOfTurnsNoStrike + " turns.";
                Console.WriteLine(tieMessage);
                Console.WriteLine(numberOfTurnsPlayed);
                return;
            }

            return;

            bool IsTheSameBoard(char[] board1, char[] board2)
            {
                for (int k = 0; k < 100; k++)
                {
                    if (board1[k] != board2[k])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
