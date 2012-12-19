// Written by Asaeli Matelau for CS3500 Assignment PS10
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomNetworking;
using BB;
using System.Threading;


namespace BS
{
    class BoggleGame
    {
        private string player1Name;         // name of player 1
        private string player2Name;         // name of player 2

        private int player1Score;           // player 1's score
        private int player2Score;           // player 2's score


        private StringSocket player1;       // socket of player 1
        private StringSocket player2;       // socket of player 2

        private HashSet<String> p1Legal;    // list of legal words played by player 1

        public HashSet<String> P1Legal
        {
            get { return p1Legal; }
            
        }
        private HashSet<String> p2Legal;    // list of legal words played by player 2

        public HashSet<String> P2Legal
        {
            get { return p2Legal; }

        }
        private HashSet<String> p1Illegal;  // list of illegal words played by player 1

        public HashSet<String> P1Illegal
        {
            get { return p1Illegal; }

        }
        private HashSet<String> p2Illegal;  // list of illegal words played by player 2

        public HashSet<String> P2Illegal
        {
            get { return p2Illegal; }
        }

        private HashSet<String> common;

        public HashSet<String> Common
        {
            get { return common; }
        }

        private BoggleBoard board;          // this game's boggle board
        private int duration;               // duration of this game

        private SortedSet<string> legalWords;   // the legal words of this game
        private bool gameInSession;             // shows if this game is in session

        // used for testing
        public int Player1Score
        {
            get { return player1Score; }

        }

        // used for testing
        public int Player2Score
        {
            get { return player2Score; }

        }

        //used for testing
        public int Duration
        {
            get { return duration; }
        }


        // controls the state of the game so players cannot make submissions when time has expired and so that time does not update if the game is done
        public bool GameInSession
        {
            get { return gameInSession; }
            set { gameInSession = value; }
        }

        /// <summary>
        /// Constructs a new game of boggle.
        /// </summary>
        /// <param name="validWords"></param>
        /// <param name="gameLength"></param>
        /// <param name="gameBoard"></param>
        public BoggleGame(SortedSet<string> validWords, int gameLength, BoggleBoard gameBoard)
        {
            legalWords = validWords;
            duration = gameLength;
            board = gameBoard;
            player1Name = null;
            player2Name = null;
            player1Score = 0;
            player2Score = 0;

            p1Illegal = new HashSet<string>();
            p2Illegal = new HashSet<string>();
            p1Legal = new HashSet<string>();
            p2Legal = new HashSet<string>();
        }

        /// <summary>
        /// Returns player1's socket
        /// </summary>
        /// <returns></returns>
        public StringSocket Player1()
        {
            return player1;
        }

        /// <summary>
        /// Returns player2's socket
        /// </summary>
        /// <returns></returns>
        public StringSocket Player2()
        {
            return player2;
        }

        /// <summary>
        /// Returns the board as a sixteen character string.
        /// </summary>
        /// <returns></returns>
        public string Board()
        {
            return board.ToString();
        }

        /// <summary>
        /// Returns the name of player one.
        /// </summary>
        /// <returns></returns>
        public string Player1Name()
        {
            return player1Name;
        }

        /// <summary>
        /// resets the player name if a player disconnects
        /// </summary>
        public void resetPlayer1Name()
        {
            player1Name = null;
        }

        /// <summary>
        /// Returns the name of player two.
        /// </summary>
        /// <returns></returns>
        public string Player2Name()
        {
            return player2Name;
        }

        /// <summary>
        /// Returns true once a games has two players, false otherwise
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public bool addPlayer(string playerName, StringSocket socket, int game)
        {
            if (player1Name == null)
            {
                player1Name = playerName;
                player1 = socket;
                player1.Player = 1;
                player1.Game = game;
                return false;
            }
            else
            {
                player2Name = playerName;
                player2 = socket;
                player2.Player = 2;
                player2.Game = game;
                return true;
            }
        }

        /// <summary>
        /// Returns true if the game is over, false otherwise
        /// </summary>
        /// <returns></returns>
        public bool updateTime()
        {
            if (gameInSession)
            {
                --duration;
                if (duration == 0)
                {
                    return true;
                }
                else
                { return false; }
            }
            else
            {
                //send time remaining
                int timeRemaining = duration;
                string timeString = "TIME " + timeRemaining + "\n";
                player1.BeginSend(timeString, (d, p) => { }, player1);
                player2.BeginSend(timeString, (d, p) => { }, player2);
                return false;
            }
        }

        /// <summary>
        /// adds the word to the correct list and updates the score accordingly for both players
        /// </summary>
        /// <param name="word"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public void addWord(string word, int player)
        {
            //games must be in session
            if (gameInSession)
            {
                //Word is LEGAL and can be formed
                if (legalWords.Contains(word) && board.CanBeFormed(word))
                {
                    legalWord(word, player);

                }

                //WORD is ILLEGAL
                else
                {
                    illegalWord(word, player);
                }
            }
        }

        /// <summary>
        /// Helper method used to score legal words
        /// </summary>
        /// <param name="word"></param>
        /// <param name="player"></param>
        private void legalWord(string word, int player)
        {
            if (player == 1)
            {
                if (!(p2Legal.Contains(word)) && !(p1Legal.Contains(word)))
                {
                    player1Score += calculateValue(word);
                }
                p1Legal.Add(word);


            }
            else
            {
                if (!(p1Legal.Contains(word)) && !(p2Legal.Contains(word)))
                {
                    player2Score += calculateValue(word);
                }
                p2Legal.Add(word);
            }
            sendScore();

        }

        /// <summary>
        /// Helper method to update score for illegal word plays
        /// </summary>
        /// <param name="word"></param>
        /// <param name="player"></param>
        private void illegalWord(string word, int player)
        {
            if (player == 1)
            {
                if (!(p1Illegal.Contains(word)))
                {
                    player1Score--;
                    p1Illegal.Add(word);
                }
            }
            else
            {
                if (!(p2Illegal.Contains(word)))
                {
                    player2Score--;
                    p2Illegal.Add(word);

                }
            }
            sendScore();
        }

        /// <summary>
        /// Calculates the point value of a given word.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private int calculateValue(string word)
        {
            int length = word.Length;
            switch (length)
            {
                case 3:
                case 4:
                    return 1;
                case 5:
                    return 2;
                case 6:
                    return 3;
                case 7:
                    return 5;
                default:
                    return 11;
            }
        }

        /// <summary>
        /// Helper Method used to update clients score
        /// </summary>
        public void sendScore()
        {

            //Insure the sockets are connected
            if (player1.connected() && player2.connected())
            {
                player1.BeginSend(score(1), (e, p) => { }, null);
                player2.BeginSend(score(2), (e, p) => { }, null);
            }


        }

        /// <summary>
        /// Helper method to provide appropiate Score string for return
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private string score(int player)
        {
            if (player == 1)
            {
                return "SCORE " + player1Score + " " + player2Score + "\n";
            }
            else
            {
                return "SCORE " + player2Score + " " + player1Score + "\n";
            }
        }

        /// <summary>
        /// Helper Method used to send the proper summary to each client
        /// </summary>
        public void summary()
        {
            // "STOP a #1 b #2 c #3 d #4 e #5"
            // STOP legalwordUniqueCount legalwordUniquelist, opponentLegalWordUniquecount oppLegalWordUniqueList, commonCount CommonList, playersIllegalWordCount, playersIllegalWordList, opppIllWrdCnt oppIllWrdLst

            //Find the common words
            common = new HashSet<string>();
            foreach (string s in p1Legal)
            {
                if (p2Legal.Contains(s))
                {
                    common.Add(s);
                }

            }
            //IEnumerable<string> common = p1Legal.Intersect(p2Legal);


            //remove the common words
            p1Legal.SymmetricExceptWith(common);
            p2Legal.SymmetricExceptWith(common);

            //create a space separated list
            string p1Unique = string.Join(" ", p1Legal);
            string p2Unique = string.Join(" ", p2Legal);
            string commonWord = string.Join(" ", common);

            string p1IllegalWords = string.Join(" ", p1Illegal);
            string p2IllegalWords = string.Join(" ", p2Illegal);



            //player1 Summary
            string player1ReturnString = "STOP " + p1Legal.Count + " " + p1Unique + " " + p2Legal.Count + " " + p2Unique + " " + common.Count() + " " + commonWord + " " + p1Illegal.Count + " " + p1IllegalWords + " " + p2Illegal.Count + " " + p2IllegalWords + "\n";

            //player2 Summary
            string player2ReturnString = "STOP " + p2Legal.Count + " " + p2Unique + " " + p1Legal.Count + " " + p1Unique + " " + common.Count() + " " + commonWord + " " + p2Illegal.Count + " " + p2IllegalWords + " " + p1Illegal.Count + " " + p1IllegalWords + "\n";

            //send summaries
            if (player1.connected() && player2.connected())
            {

                player1.BeginSend(player1ReturnString, (e, p) => { }, null);
                player2.BeginSend(player2ReturnString, (e, p) => { }, null);

            }
        }

        /// <summary>
        /// Helper Method used to shutdown sockets
        /// </summary>
        public void shutdown()
        {
            if (gameInSession)
            {
                gameInSession = false;
                if (player1.connected() && player2.connected())
                {
                    player1.shutdown();
                    player2.shutdown();
                    player1.close();
                    player2.close();
                }
                Console.WriteLine("Shutting down game " + player1.Game);
            }


        }
    }
}
