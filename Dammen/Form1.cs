using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Dammen
{
    //Possible directions in which the pieces can move
    public enum Directions
    {
        NW,
        NE,
        SW,
        SE,
    };

    public enum InitialBoardStates
    {
        STD,
        STRIKETEST1,
        STRIKETEST2,
        STRIKETEST3,
        TIETEST1,
        TIETEST2,
        AITEST1,
    };

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        readonly Color blackPiece = System.Drawing.Color.FromArgb(
                        ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        readonly Color whitePiece = System.Drawing.Color.FromArgb(
                        ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
        readonly Color blackTile = System.Drawing.Color.FromArgb(
                        ((int)(((byte)(128)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
        readonly Color whiteTile = System.Drawing.Color.FromArgb(
                        ((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
        readonly Color colorRed = System.Drawing.Color.FromArgb(
                        ((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
        readonly Color colorGreen = System.Drawing.Color.FromArgb(
                        ((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));

        // Bool to check if the game is started
        private bool isGameStarted = false;

        // For human player
        private bool isPieceSelected;
        private int humanSelectedLocation;
        //private MoveData humanValidMoves;

        // Saves data relevant to current game
        BoardData currentBoardData;

        //Used for the notation in the textbox
        private string notationText;
        private int notationTurn;
        private bool turnHasStrike;

        //Used for the AI
        private bool whiteIsAI;
        private int whiteAIDiff;
        private bool blackIsAI;
        private int blackAIDiff;

        // For debuging/testing
        private string messageString = "message:";
        private StringBuilder forDisplay = new StringBuilder();
        private int boardEvalSave;

        //Keep track of time
        private Stopwatch timerPassed;

        /*----------------EVENTS-------------------*/

        //When the start button is clicked, draw the pieces
        private void StartButton_Click(object sender, EventArgs e)
        {
            //If the game is already started, nothing happens
            if (isGameStarted)
            {
                return;
            }

            //The game can be started
            isGameStarted = true;

            //Initiate a new board, the black pieces start
            currentBoardData = new BoardData(InitialBoardStates.STD, "w");

            //Data for Human player
            isPieceSelected = false;
            humanSelectedLocation = -1;

            // Draw the Pieces
            DrawBoardPieces(currentBoardData.playField);

            //Update text on TieButton
            UpdateTieButtonText();

            //Data for notation Textbox
            notationText = "";
            notationTurn = 0;
            turnHasStrike = false;

            timerPassed = new Stopwatch();

            // Read in AI settings and disables those fields
            whiteIsAI = whiteAIradioButton.Checked;
            whiteAIDiff = (int) whiteAINumUpDown.Value;
            whitePanel.Enabled = false;

            blackIsAI = blackAIradioButton.Checked;
            blackAIDiff = (int) blackAINumUpDown.Value;
            blackPanel.Enabled = false;

            //If the white player is an AI, it will start
            if(whiteIsAI)
            {
                AIMakesMove();
            }
        }

        //The AI makes a move
        private async void AIMakesMove()
        {
            //Get the difficculty of the current player
            int turnDifficultyOfAI = whiteAIDiff;
            if (currentBoardData.currentPlayer == "b")
            {
                turnDifficultyOfAI = blackAIDiff;
            }

            // get the list of possible moves with the best score
            currentBoardData.EvaluateTurnEndPlayFields(turnDifficultyOfAI, timerPassed);

            //Get the list of positions.
            List<int> movesToDo = new List<int>();
            Random randomNumber = new Random();
            int chosenIndex;
            if(currentBoardData.bestMoveIndexForAI.Count() == 0 
                && currentBoardData.currentValidMoves.moveList.Count > 0)
            {
                //this means that there are no moves that will not result in a loss
                //The first move is picked
                movesToDo.AddRange(currentBoardData.currentValidMoves
                    .moveList[0]);
            }
            else if(currentBoardData.bestMoveIndexForAI.Count() == 1)
            {
                //Get the list of coordinates of the move that needs to be done;
                movesToDo.AddRange(currentBoardData.currentValidMoves
                    .moveList[currentBoardData.bestMoveIndexForAI[0]]);
            }
            else
            {
                //If there are multiple moves with the sme highest score, chose a move at random
                //from that list
                chosenIndex = randomNumber.Next(0, currentBoardData.bestMoveIndexForAI.Count());
                movesToDo.AddRange(currentBoardData.currentValidMoves
                    .moveList[currentBoardData.bestMoveIndexForAI[chosenIndex]]);
            }

            //Move through the List of coordinates
            for (int i = 0; i < movesToDo.Count()-1; i++)
            {
                //Console.WriteLine("what?");
                currentBoardData.MovePiece(movesToDo[i], movesToDo[i + 1]);
                DrawBoardPieces(currentBoardData.playField);
            }

            //Update the notation text
            if (movesToDo.Count() > 1)
            {
                if (currentBoardData.currentPlayer == "w")
                {
                    notationTurn++;
                    notationText += notationTurn;
                }
                notationText += "    " + movesToDo[0];
                string middleMarker = "-";
                foreach (char field in currentBoardData.playField)
                {
                    if (field == 's')
                    {
                        middleMarker = "x";
                    }
                }
                notationText += middleMarker;
                notationText += LocationIndexConverter(movesToDo[movesToDo.Count() - 1]);
                if (currentBoardData.currentPlayer == "b")
                {
                    notationText += "\n";
                }
                richTextBox1.Text = notationText;
                //scrolls the tesxtbox down
                richTextBox1.AppendText(" ");
            }

            if (whiteIsAI && blackIsAI)
            {
                await DoSomethingAsync();
            }

            // It is the turn of the next player
            NextTurn();

            async Task DoSomethingAsync()
            {
                await Task.Delay(100);
            }
        }

        //When a field(button) is clicked, stuff happens
        private void Field_Click(object sender, EventArgs e)
        {
            // If the game is not yet started, nothing happens
            if (!isGameStarted)
            {
                return;
            }

            Button clickedButton = sender as Button;
            int humanClickedLocation = clickedButton.TabIndex;

            // either a piece is already selected, or not.
            // If a piece was not yet selected, select a piece
            if (isPieceSelected == false)
            {
                // Check if the piece clicked is on the list of current possible moves
                // if this is not the case nothing happens

                bool isClickedValid = false;
                foreach (List<int> validMove in currentBoardData.currentValidMoves.moveList)
                {
                    if (validMove[0] == humanClickedLocation)
                    {
                        isClickedValid = true;
                    }
                }
                if (!isClickedValid)
                {
                    //Debugging stuff
                    //forDisplay.Clear();
                    //constructStringBuilder(currentBoardData.currentValidMoves.endPlayFieldsList);
                    //timerPassed.Start();
                    //boardEvalSave = currentBoardData.EvaluateTurnEndPlayFields(3,timerPassed);
                    //timerPassed.Stop();
                    //DisPMoveData(currentBoardData.currentValidMoves);
                    //timerPassed.Reset();
                    return;
                }

                humanSelectedLocation = humanClickedLocation;

                if (currentBoardData.currentPlayer == "w")
                {
                    notationTurn++;
                    notationText += notationTurn;
                }
                notationText += "    " + LocationIndexConverter(humanSelectedLocation);

                // Trim the the curently available moves to the piece that is selected
                currentBoardData.TrimMoveListToSelect(humanSelectedLocation);

                //Debugging stuff
                //orDisplay.Clear();
                //constructStringBuilder(currentBoardData.currentValidMoves.EndPlayFieldsList);
                //boardEvalSave = currentBoardData.EvaluateTurnEndPlayFields(3);
                //DisPMoveData(currentBoardData.currentValidMoves);

                // Show the available moves
                DrawBordercolor();
                isPieceSelected = true;
            }
            else
            {
                // The player must click a new location
                // of the piece, otherwise nothing happens
                if (!IsHumanMovePossibleForPiece(humanClickedLocation))
                {
                    return;
                }

                currentBoardData.MovePiece(humanSelectedLocation, humanClickedLocation);
                
                //Data for notationtextbox
                if (currentBoardData.currentValidMoves.highestStrike > 0)
                {
                    turnHasStrike = true;
                }

                //Continue this turn if more strikes are present
                if (currentBoardData.currentValidMoves.highestStrike > 1)
                {
                    // Trim the the curently available moves to the piece that is selected
                    currentBoardData.TrimMoveListToSelect(humanClickedLocation);
                    //DisPMoveData(currentValidMoves);

                    humanSelectedLocation = humanClickedLocation;
                    DrawBoardPieces(currentBoardData.playField);
                    ClearBorderColor();
                    DrawBordercolor();
                }
                //Else end this turn and reset stuff
                else
                {
                    //Data for notationtextbox
                    if (turnHasStrike)
                    {
                        notationText += "x";
                    }
                    else
                    {
                        notationText += "-";
                    }
                    notationText += LocationIndexConverter(humanClickedLocation);
                    if (currentBoardData.currentPlayer == "b")
                    {
                        notationText += "\n";
                    }
                    turnHasStrike = false;
                    richTextBox1.Text = notationText;
                    //scrolls the tesxtbox down
                    richTextBox1.AppendText(" ");

                    //Prepares the next Turn
                    NextTurn();

                    //Reset the variables for human
                    isPieceSelected = false;
                    humanSelectedLocation = -1;
                    ClearBorderColor();
                }
            }
            return;

            // A local method
            bool IsHumanMovePossibleForPiece(int location)
            {
                foreach (List<int> locationList in currentBoardData.currentValidMoves.moveList)
                {
                    if (location == locationList[1])
                    {
                        return true;
                    }
                }
                return false;
            }

            void DrawBordercolor()
            {
                // I wish I had a way to select the buttons based on tabindex or something.
                if (currentBoardData.currentValidMoves != null)
                {
                    foreach (Control control in tableLayoutPanel1.Controls)
                    {
                        Button tempButton = control as Button;

                        foreach (List<int> locationList in currentBoardData.currentValidMoves.moveList)
                        {
                            if (tempButton.TabIndex == locationList[1])
                            {
                                tempButton.FlatAppearance.BorderColor = colorRed;
                            }
                        }
                    }
                }
                //the clickedbutton gets a green edge
                clickedButton.FlatAppearance.BorderColor = colorGreen;
            }
        }
        private void EndGame(string endGameMessage)
        {
            whitePanel.Enabled = true;
            blackPanel.Enabled = true;
            isGameStarted = false;
            MessageBox.Show(endGameMessage);
        }
        private void NextTurn()
        {
            //Remove striked pieces
            currentBoardData.RemoveStrikedPieces();
            //Change Disks into Dammen, when possible
            currentBoardData.ChangeDisksIntoDammen();

            //Draws the board
            DrawBoardPieces(currentBoardData.playField);

            //It is the turn of the next player
            currentBoardData.SwitchPlayer();
            UpdateTieButtonText();

            // Get all possible valid moves of the next player
            currentBoardData.GetPossibleMoves();
            //DisPMoveData(currentValidMoves);

            // Check if the next player has lost
            if (currentBoardData.CheckLoss())
            {
                string winMessage = "White ";
                if (currentBoardData.currentPlayer == "w")
                {
                    winMessage = "Black ";
                }
                winMessage += " wins.";
                EndGame(winMessage);
                return;
            }

            // Is there a Tie?
            currentBoardData.UpdataTieData();
            if (currentBoardData.CheckTie())
            {
                EndGame("The game ended in a Tie\n\n"
                    + currentBoardData.currentTieData.tieMessage);
            }

            if (((whiteIsAI && currentBoardData.currentPlayer == "w")
                || (blackIsAI && currentBoardData.currentPlayer == "b"))
                && isGameStarted)
            {
                //Console.WriteLine("something");
                AIMakesMove();
            }
        }
        private int LocationIndexConverter(int locationPassed)
        {
            if(locationPassed % 20 < 10)
            {
                return (locationPassed+1)/2;
            }
            else
            {
                return (locationPassed / 2) + 1;
            }
        }
        private void UpdateTieButtonText()
        {
            if ((currentBoardData.currentPlayer == "b"
                && currentBoardData.currentTieData.blackRequestsDraw)
                || (currentBoardData.currentPlayer == "w"
                && currentBoardData.currentTieData.whiteRequestsDraw))
            {
                drawButton.Text = "You are requesting a Tie";
            }
            else
            {
                drawButton.Text = "Request a Tie";
            }

            return;
        }
        private void ClearBorderColor()
        {
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Button tempButton = control as Button;
                if (tempButton.BackColor == blackTile)
                {
                    tempButton.FlatAppearance.BorderColor = blackTile;
                }
            }
        }
        private void DrawBoardPieces(char[] playFieldPassed)
        {
            //Draws the pieces
            foreach (Control control in tableLayoutPanel1.Controls)
            {
                Button iconButton = control as Button;
                if (iconButton != null) //Not sure what this conditional check is for, got it from a tutorial
                {
                    DrawPiece(iconButton, playFieldPassed[iconButton.TabIndex]);
                }
            }
        }
        private void DrawPiece(Button fieldFromControl, char fieldFromArray)
        {
            switch (fieldFromArray)
            {
                case 'b':
                    fieldFromControl.Text = "l";
                    fieldFromControl.ForeColor = blackPiece;
                    break;
                case 'B':
                    fieldFromControl.Text = "O";
                    fieldFromControl.ForeColor = blackPiece;
                    break;
                case 'w':
                    fieldFromControl.Text = "l";
                    fieldFromControl.ForeColor = whitePiece;
                    break;
                case 'W':
                    fieldFromControl.Text = "O";
                    fieldFromControl.ForeColor = whitePiece;
                    break;
                case 's':
                    fieldFromControl.ForeColor = colorRed;
                    break;
                case ' ':
                    fieldFromControl.Text = "";
                    break;
            }
        }
        //Used for debugging
        private void DisPMoveData(MoveData moveData)
        {
            string toDisplay = "highest strike = " + moveData.highestStrike;
            toDisplay += "\n current board score: " + boardEvalSave;
            toDisplay += "\n best move AI: ";
            foreach (int i in currentBoardData.bestMoveIndexForAI)
            {
                toDisplay += i + " ";
            }
            int k = 0;
            foreach (List<int> locationList in moveData.moveList)
            {
                toDisplay += "\n" + k + " | ";
                k++;
                foreach (int location in locationList)
                {
                    toDisplay += location + " ";
                }
            }
            toDisplay += "\nTime past: " + timerPassed.ElapsedMilliseconds + " ms";
            toDisplay += "\n\n\n" + forDisplay.ToString();
            MessageBox.Show(toDisplay);
        }

        private void DrawButton_Click(object sender, EventArgs e)
        {
            if (isGameStarted == false)
            {
                return;
            }

            if (currentBoardData.currentPlayer == "b")
            {
                currentBoardData.currentTieData.blackRequestsDraw
                    = !currentBoardData.currentTieData.blackRequestsDraw;
            }

            if (currentBoardData.currentPlayer == "w")
            {
                currentBoardData.currentTieData.whiteRequestsDraw
                    = !currentBoardData.currentTieData.whiteRequestsDraw;
            }
            UpdateTieButtonText();
            currentBoardData.currentTieData.UpdateAgreedDraw();
            if (currentBoardData.currentTieData.isTie)
            {
                EndGame("The game ended in a Tie\n\n"
                    + currentBoardData.currentTieData.tieMessage);
            }
        }

        private void GiveupButton_Click(object sender, EventArgs e)
        {
            if (isGameStarted == false)
            {
                return;
            }

            string giveupMessage = "Black wins.";
            if (currentBoardData.currentPlayer == "b")
            {
                giveupMessage = "White wins.";
            }

            EndGame("You have given up.\n\n"
                + giveupMessage);
        }

        //Used for debugging/testing
        private void constructStringBuilder(List<char[]> playFieldListPassed)
        {
            foreach (char[] playFieldPassed in playFieldListPassed)
            {
                forDisplay.Append("\n\n");
                for (int i = 0; i < 100; i++)
                {
                    if (i % 10 == 0)
                    {
                        forDisplay.Append("\n");
                    }
                    forDisplay.Append(playFieldPassed[i].ToString());
                }
            }
        }
    }
}
