using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;

namespace BoggleClient
{
    public partial class BoggleClientView : Form
    {
        private BoggleClientModel model;    // model for this client
        private bool isConnected;           // true is the result of the client being connected to a server
                                            // false otherwise.

        /// <summary>
        /// constructs the client
        /// </summary>
        public BoggleClientView()
        {
            InitializeComponent();
            model = new BoggleClientModel();
            //register event handlers
            model.IncomingStartEvent += setupGame;            
            model.IncomingScoreEvent += updateScore;
            model.IncomingTimeEvent += updateTime;
            model.IncomingStopEvent += summary;
            model.IncomingTerminatedEvent += terminate;
            model.noSuchHostEvent += badHost;
            
        }

        private void BoggleClientView_Load(object sender, EventArgs e)
        {
            isConnected = false;
        }

        /// <summary>
        /// sets up the game
        /// </summary>
        /// <param name="line"></param>
        private void setupGame(String line)
        {
            // The command is "START $ # @", where $ is the 16 characters that 
            // appear on the Boggle board being used for this game, # is the 
            // length of the game in seconds, and @ is the opponent's name.

            // prep the string
            line = line.TrimStart();

            // display the board
            string board = line.Substring(0, 16);
            displayBoard(board);

            // display the start time
            line = line.Substring(16).TrimStart();
            string time = line.Substring(0, line.IndexOf(' '));
            updateTime(time);

            // display the players
            line = line.Substring(line.IndexOf(' ')).TrimStart();
            string players = nameLabel.Text + " VS " + line;
            nameLabel.Invoke(new Action(() => nameLabel.Text = players));

            // display proper labels
            scoreLabel.Invoke(new Action(() => scoreLabel.Visible = true));
            opponentScoreLabel.Invoke(new Action(() => opponentScoreLabel.Visible = true));
            timeLabel.Invoke(new Action(() => timeLabel.Visible = true));
            counterLabel.Invoke(new Action(() => counterLabel.Visible = true));
            wordLabel.Invoke(new Action(() => wordLabel.Visible = true));
            wordTextBox.Invoke(new Action(() => wordTextBox.Visible = true));
            enterButton.Invoke(new Action(() => enterButton.Visible = true));

            // setup focus for entering words
            AcceptButton = enterButton;
            wordTextBox.Invoke(new Action(() => wordTextBox.Focus()));
        }

        /// <summary>
        /// Displays the boggle board on the client
        /// </summary
        /// <param name="board"></param>
        private void displayBoard(string board)
        {
            board00.Invoke(new Action(()=> board00.Text = "" + board[0]));
            board01.Invoke(new Action(() => board01.Text = "" + board[1]));
            board02.Invoke(new Action(() => board02.Text = "" + board[2]));
            board03.Invoke(new Action(() => board03.Text = "" + board[3]));
            board10.Invoke(new Action(() => board10.Text = "" + board[4]));
            board11.Invoke(new Action(() => board11.Text = "" + board[5]));
            board12.Invoke(new Action(() => board12.Text = "" + board[6]));
            board13.Invoke(new Action(() => board13.Text = "" + board[7]));
            board20.Invoke(new Action(() => board20.Text = "" + board[8]));
            board21.Invoke(new Action(() => board21.Text = "" + board[9]));
            board22.Invoke(new Action(() => board22.Text = "" + board[10]));
            board23.Invoke(new Action(() => board23.Text = "" + board[11]));
            board30.Invoke(new Action(() => board30.Text = "" + board[12]));
            board31.Invoke(new Action(() => board31.Text = "" + board[13]));
            board32.Invoke(new Action(() => board32.Text = "" + board[14]));
            board33.Invoke(new Action(() => board33.Text = "" + board[15]));
        }

        /// <summary>
        /// Clears the board
        /// </summary>
        /// <param name="board"></param>
        private void clearBoard()
        {
            board00.Invoke(new Action(() => board00.Text = ""));
            board01.Invoke(new Action(() => board01.Text = ""));
            board02.Invoke(new Action(() => board02.Text = ""));
            board03.Invoke(new Action(() => board03.Text = ""));
            board10.Invoke(new Action(() => board10.Text = ""));
            board11.Invoke(new Action(() => board11.Text = ""));
            board12.Invoke(new Action(() => board12.Text = ""));
            board13.Invoke(new Action(() => board13.Text = ""));
            board20.Invoke(new Action(() => board20.Text = ""));
            board21.Invoke(new Action(() => board21.Text = ""));
            board22.Invoke(new Action(() => board22.Text = ""));
            board23.Invoke(new Action(() => board23.Text = ""));
            board30.Invoke(new Action(() => board30.Text = ""));
            board31.Invoke(new Action(() => board31.Text = ""));
            board32.Invoke(new Action(() => board32.Text = ""));
            board33.Invoke(new Action(() => board33.Text = ""));
        }

        /// <summary>
        /// updates the score 
        /// </summary>
        /// <param name="line"></param>
        private void updateScore(String line)
        {
            if (isConnected)
            {
                // "SCORE #1 #2", where #1 is the client's current score 
                // and #2 is the opponent's current score.

                // prep the string
                line = line.Trim();
                scoreLabel.Invoke(new Action(() => scoreLabel.Text = "Score:  " + line.Substring(0, line.IndexOf(' '))));

                line = line.Substring(line.IndexOf(' ')).TrimStart();
                opponentScoreLabel.Invoke(new Action(() => opponentScoreLabel.Text = "Opponent Score:  " + line));
            }
        }

        /// <summary>
        /// updates the Time
        /// </summary>
        /// <param name="line"></param>
        private void updateTime(String line)
        {
            lock (counterLabel)
            {
                if (!counterLabel.IsDisposed)
                {
                    counterLabel.Invoke(new Action(() => counterLabel.Text = line.Trim()));
                }
            }
        }


        /// <summary>
        /// game summary
        /// </summary>
        /// <param name="line"></param>
        private void summary(String line)
        {

            // the client played a legal words that weren't played by the opponent, 
            // the opponent played b legal words that weren't played by the client, 
            // both players played c legal words in common, 
            // the client played d illegal words, 
            // and the opponent played e illegal words.  

            // "STOP a #1 b #2 c #3 d #4 e #5", where a, b, c, d, and e 
            // are the counts described above and #1, #2, #3, #4, and #5 
            // are the corresponding space-separated lists of words.

            // prep the string
            line = line.Trim();
            string[] data = line.Split(' ');
            string clientLegalWordCount = data[0];
            string clientLegalWords = "";
            int index = 1;
            int temp;

            //insure time states 0
            counterLabel.Invoke(new Action(() => counterLabel.Text = "0"));

            while (index < data.Length && !Int32.TryParse(data[index], out temp))
            {
                clientLegalWords = clientLegalWords + data[index] + " ";
                index++;
            }

            string opponentLegalWordCount = data[index++];
            string opponentLegalWords = "";

            while (index < data.Length && !Int32.TryParse(data[index], out temp))
            {
                opponentLegalWords = opponentLegalWords + data[index] + " ";
                index++;
            }

            string commonWordsCount = data[index++];
            string commonWords = "";

            while (index < data.Length && !Int32.TryParse(data[index], out temp))
            {
                commonWords = commonWords + data[index] + " ";
                index++;
            }

            string clientIllegalWordCount = data[index++];
            string clientIllegalWords = "";

            while (index < data.Length && !Int32.TryParse(data[index], out temp))
            {
                clientIllegalWords = clientIllegalWords + data[index] + " ";
                index++;
            }

            string opponentIllegalWordCount = data[index++];
            string opponentIllegalWords = "";

            while (index < data.Length && !Int32.TryParse(data[index], out temp))
            {
                opponentIllegalWords = opponentIllegalWords + data[index] + " ";
                index++;
            }

            MessageBox.Show("GAME OVER\n\n"
                           + "Your Legal Words (" + clientLegalWordCount + "):  "
                           + clientLegalWords + "\n"
                           + "Opponent's Legal Words(" + opponentLegalWordCount + "):  "
                           + opponentLegalWords + "\n"
                           + "Common Words (" + commonWordsCount + "):  "
                           + commonWords + "\n"
                           + "Your Illegal Words (" + clientIllegalWordCount + "):  "
                           + clientIllegalWords + "\n"
                           + "Opponent's Illegal Words (" + opponentIllegalWordCount + "):  "
                           + opponentIllegalWords);

            // end the session
            model.Disconnect();
            disconnect();        
        }

        /// <summary>
        /// game summary
        /// </summary>
        /// <param name="line"></param>
        private void terminate(String line)
        {
            MessageBox.Show("The opponent client has unexpectedly disconnected.");
            model.terminate();
            disconnect();
        }

        /// <summary>
        /// Connects the game to the given server, else disconnects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectionButton_Click(object sender, EventArgs e)
        {
            if (!isConnected) // connects game to server
            {                             
                string server = serverTextBox.Text;
                string player = nameTextBox.Text;

                // REMOVED FOR BUTTON TESTING ONLY ********************************************************
                model.Connect(server, player);
                // REMOVED FOR BUTTON TESTING ONLY ********************************************************

                // We need to test that it is connected

                // update connection button
                connectionButton.Invoke(new Action(() => connectionButton.Text = "Disconnect"));
                isConnected = true;

                // hide unnecessary fields
                serverLabel.Invoke(new Action(() => serverLabel.Visible = false));
                serverTextBox.Invoke(new Action(() => serverTextBox.Visible = false));

                // update name label
                nameLabel.Invoke(new Action(() => nameLabel.Text = player));
                nameTextBox.Invoke(new Action(() => nameTextBox.Visible = false));

                // clear score, time, and wordbox
                wordTextBox.Invoke( new Action(() => wordTextBox.Text = ""));
                scoreLabel.Invoke( new Action(() => scoreLabel.Text ="Score: 0"));
                opponentScoreLabel.Invoke(new Action(() => opponentScoreLabel.Text ="Score: 0")); 


                // TESTING PURPOSE ONLY *******************************************************************
                //setupGame("    ABCDEFGHJKLMNOPQ    60    Player 2   ");
                //updateScore("    5   7   ");
                //terminate(null);
                //summary("3 cat dog bird 3 mouse frog horse 2 common words 2 aaa bbb 2 ccc ddd");
                // TESTING PURPOSE ONLY *******************************************************************
            }
            else  // disconnects game from the server
            {                
                
                disconnect();
                model.Disconnect();
            }
        }

        /// <summary>
        /// Returns the client to a starting state
        /// </summary>
        private void disconnect()
        {
                 // update connection button
                connectionButton.Invoke(new Action(() => connectionButton.Text = "Connect"));
                isConnected = false;

                // reset server labels
                serverLabel.Invoke(new Action(() => serverLabel.Visible = true));
                serverTextBox.Invoke(new Action(() => serverTextBox.Text = ""));
                serverTextBox.Invoke(new Action(() => serverTextBox.Visible = true));

                // set game labels and button invisible
                nameLabel.Invoke(new Action(() => nameLabel.Text = "Name:"));
                nameTextBox.Invoke(new Action(() => nameTextBox.Text = ""));
                nameTextBox.Invoke(new Action(() => nameTextBox.Visible = true));
                scoreLabel.Invoke(new Action(() => scoreLabel.Visible = false));
                opponentScoreLabel.Invoke(new Action(() => opponentScoreLabel.Visible = false));
                timeLabel.Invoke(new Action(() => timeLabel.Visible = false));
                counterLabel.Invoke(new Action(() => counterLabel.Visible = false));
                wordLabel.Invoke(new Action(() => wordLabel.Visible = false));
                wordTextBox.Invoke(new Action(() => wordTextBox.Visible = false));
                enterButton.Invoke(new Action(() => enterButton.Visible = false));
                clearBoard();

                // reset focus
                AcceptButton = connectionButton;
                nameTextBox.Invoke(new Action(() => nameTextBox.Focus()));
          
        }

        /// <summary>
        /// Sends the current word being played
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enterButton_Click(object sender, EventArgs e)
        {
            if (wordTextBox.Text != "")
                model.SendMessage("WORD " + wordTextBox.Text);
            wordTextBox.Text = "";
        }

        private void BoggleClientView_FormClosing(object sender, FormClosingEventArgs e)
        {
            model.Disconnect(); 
        }

        private void badHost(string line)
        {
            MessageBox.Show(line + ". Please click disconnect and try again");
            disconnect(); 
        }

    }
}
