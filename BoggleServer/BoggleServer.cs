﻿// Written by Asaeli Matelau for CS3500 Assignment PS10
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomNetworking;
using System.Net.Sockets;
using System.Net;
using BB;
using System.IO;
using System.Timers;
using System.Threading;
using MySql.Data.MySqlClient;

namespace BS
{
    public class BoggleServer
    {
        //dictionary of current games string -> game object
        private Dictionary<int, BoggleGame> currentGames;

        //string to connect to the database
        private const string connectionString = "server=atr.eng.utah.edu;database=matelau;Truncated for Protection";

        //keepts track of number of games
        int gameNumber;

        //list of legal words for the server
        private SortedSet<string> legalWords;

        //duration of games for the server
        private int gameLength;

        //setup game used to pair-up connections
        private BoggleGame setupGame;

        //heart of the server
        private TcpListener server;

        // heart of the webserver
        private TcpListener webServer;

        //keeps track of the time
        private System.Timers.Timer time;

        // custom board
        private string customBoard;

        private Queue<Tuple<StringSocket, string>> PlayQ;
        private Queue<Tuple<StringSocket, string>> WordQ;
   

        //used for testing
        internal Dictionary<int, BoggleGame> CurrentGames
        {
            get { return currentGames; }
        }

        // used for testing
        internal BoggleGame SetupGame
        {
            get { return setupGame; }
        }

        // used for testing
        public SortedSet<string> LegalWords
        {
            get { return legalWords; }
        }

        static void Main(string[] args)
        {
            int val;
            string path = args[1];
            //get the value of the timer and check that it is first an integer and secondly greater than zero
            if (!(int.TryParse(args[0], out val) && val > 0))
            {

                Console.WriteLine("Time must be greater than zero");

            }
            else if (args.Length == 2)
            {
                new BoggleServer(val, path);
            }
            else
            {
                string personalBoggleBoard = args[2];
                if (personalBoggleBoard.Length == 16)
                {
                    new BoggleServer(val, path, personalBoggleBoard);
                }
                else
                {
                    Console.WriteLine("BoggleBoard must contain 16 characters");
                }
            }

            //for debugging
            Console.ReadLine();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gameLengthArgs"></param>
        /// <param name="path"></param>
        public BoggleServer(int gameLengthArgs, string path)
        {
            PlayQ = new Queue<Tuple<StringSocket, string>>();
            WordQ = new Queue<Tuple<StringSocket, string>>();
            server = new TcpListener(IPAddress.Any, 2000);
            server.Start();
            webServer = new TcpListener(IPAddress.Any, 2500);
            webServer.Start();

            gameLength = gameLengthArgs;
            //initialize the sorted set of legal words
            legalWords = new SortedSet<string>();
            ThreadPool.QueueUserWorkItem(e => parseFile(path));

            customBoard = null;
            currentGames = new Dictionary<int, BoggleGame>();
            gameNumber = 1;
            setupGame = new BoggleGame(legalWords, gameLength, new BoggleBoard());
            //create timer
            time = new System.Timers.Timer(1000);
            time.Elapsed += new ElapsedEventHandler(updateTimers);
            time.Start();
            Console.WriteLine("Server Running");
            server.BeginAcceptSocket(ConnectionReceived, null);
            webServer.BeginAcceptSocket(WebConnectionReceived, null);

        }

        /// <summary>
        /// Three parameter constructor allows the board to be specified 
        /// </summary>
        /// <param name="gameLengthArgs"></param>
        /// <param name="path"></param>
        /// <param name="board"></param>
        public BoggleServer(int gameLengthArgs, string path, string board)
        {
            PlayQ = new Queue<Tuple<StringSocket, string>>();
            WordQ = new Queue<Tuple<StringSocket, string>>();
            server = new TcpListener(IPAddress.Any, 2000);
            server.Start();
            webServer = new TcpListener(IPAddress.Any, 2500);
            webServer.Start();

            gameLength = gameLengthArgs;
            //initialize the sorted set of legal words
            legalWords = new SortedSet<string>();
            ThreadPool.QueueUserWorkItem(e => parseFile(path));

            customBoard = board.ToUpper();
            currentGames = new Dictionary<int, BoggleGame>();
            gameNumber = 1;
            setupGame = new BoggleGame(legalWords, gameLength, new BoggleBoard(board));
            //create timer
            time = new System.Timers.Timer(1000);
            time.Elapsed += new ElapsedEventHandler(updateTimers);
            time.Start();
            Console.WriteLine("Server Running");
            server.BeginAcceptSocket(ConnectionReceived, null);
            webServer.BeginAcceptSocket(WebConnectionReceived, null);
        }

        /// <summary>
        /// Constructor that allows the server to accept sockets on a different port for testing 
        /// </summary>
        /// <param name="gameLengthArgs"></param>
        /// <param name="path"></param>
        public BoggleServer(int gameLengthArgs, string path, int port)
        {
            PlayQ = new Queue<Tuple<StringSocket, string>>();
            WordQ = new Queue<Tuple<StringSocket, string>>();
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            webServer = new TcpListener(IPAddress.Any, 2500);
            webServer.Start();
            gameLength = gameLengthArgs;
            //initialize the sorted set of legal words
            legalWords = new SortedSet<string>();
            ThreadPool.QueueUserWorkItem(e => parseFile(path));

            customBoard = null;
            currentGames = new Dictionary<int, BoggleGame>();
            gameNumber = 1;
            setupGame = new BoggleGame(legalWords, gameLength, new BoggleBoard());
            //create timer
            time = new System.Timers.Timer(1000);
            time.Elapsed += new ElapsedEventHandler(updateTimers);
            time.Start();
            Console.WriteLine("Server Running");
            server.BeginAcceptSocket(ConnectionReceived, null);
            webServer.BeginAcceptSocket(WebConnectionReceived, null);

        }

        /// <summary>
        /// Builds a SortedSet of "LegalWords" from a properly formatted txt file
        /// </summary>
        /// <param name="path"></param>
        private void parseFile(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    // reads in the text file
                    while (sr != null)
                    {
                        string line = sr.ReadLine();
                        legalWords.Add(line);
                    }

                    // converts the words to upper case
                    SortedSet<string> upperLegal = new SortedSet<string>();
                    foreach (string s in legalWords)
                    {

                        upperLegal.Add(s.ToUpper());
                    }
                    legalWords = upperLegal;

                }
            }

            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Deals with connection requests
        /// </summary>
        private void ConnectionReceived(IAsyncResult ar)
        {

            //get the underlying socket   
            Socket socket = server.EndAcceptSocket(ar);
            //wrap the socket
            StringSocket ss = new StringSocket(socket, UTF8Encoding.Default);
            //start receiving on the stringsocket
            ss.BeginReceive(commandReceieved, ss);

            //accept another connection
            server.BeginAcceptSocket(ConnectionReceived, null);
        }

        /// <summary>
        /// Deals with web connection requests
        /// </summary>
        /// <param name="ar"></param>
        private void WebConnectionReceived(IAsyncResult ar)
        {
            // get the underlying socket
            Socket socket = webServer.EndAcceptSocket(ar);

            // wrap the socket
            StringSocket ss = new StringSocket(socket, UTF8Encoding.Default);

            // start receiving on the string socket
            ss.BeginReceive(WebCommandReceived, ss);

            // accept another connection
            webServer.BeginAcceptSocket(WebConnectionReceived, null);
        }

        /// <summary>
        /// Callback method once Connections are Received
        /// </summary>
        /// <param name="command"></param>
        /// <param name="e"></param>
        /// <param name="ss"></param>
        private void commandReceieved(string command, Exception e, object ss)
        {
            string nameTemp = command;

            lock (currentGames)
            {
                StringSocket socket = (StringSocket)ss;

                // check if socket has been terminated
                if (command == null && e == null)
                    terminated(socket);


                if (command != null)
                {
                    command = parseCommand(command);

                    switch (command.Substring(0, 4))
                    {
                        case "PLAY":
                            //the game is already in play
                            if (socket.Game > 0 || socket.Player == 1 || socket.Player == 2)
                            {
                                socket.BeginSend("IGNORING " + command, (ee, p) => { }, socket);
                            }
                            else
                                play(socket, nameTemp.Remove(0, 5));
                            break;
                        case "WORD":
                            BoggleGame game;
                            string werd = command.Remove(0, 5);
                            if (!(currentGames.TryGetValue(socket.Game, out game)) || werd.Contains(" "))
                                socket.BeginSend("IGNORING " + command, (ee, p) => { }, socket);
                            else
                                if (werd.Length > 2)
                                    word(socket, werd);
                            break;
                        case "what?":
                            break;
                        default:
                            socket.BeginSend("IGNORING " + command, (ee, p) => { }, socket);
                            break;
                    }
                }
                socket.BeginReceive(commandReceieved, socket);
            }
        }

        /// <summary>
        /// Callback method for connections received on the web server
        /// </summary>
        /// <param name="command"></param>
        /// <param name="e"></param>
        /// <param name="ss"></param>
        private void WebCommandReceived(string command, Exception e, object ss)
        {
            bool valid = false;
            Console.WriteLine(command);
            lock (connectionString)
            {
                
                StringSocket socket = (StringSocket)ss;
                if (command != null && command.Contains("GET") && command.Substring(0, 3).Equals("GET"))
                {
                    string test = command;
                    // GET /players HTTP/1.1 
                    if (command.Contains("/players"))
                    {
                        beginHTML(socket);
                        playersHTML(socket);
                        endHTML(socket);
                        valid = true;
                    }

                    // GET /games?player=Joe HTTP/1.1
                    if (command.Contains("games?player="))
                    {
                        // get the players name
                        string playerName = getProperSubstring(test);
                        beginHTML(socket);
                        playerHTML(socket, playerName);
                        endHTML(socket);
                        valid = true;
                    }

                    // GET /game?id=35 HTTP/1.1
                    if (command.Contains("game?id="))
                    {
                        // get game id
                        string gameID = getProperSubstring(command);
                        int gID;
                        int.TryParse(gameID, out gID);

                        beginHTML(socket);
                        gameHTML(socket, gID);
                        endHTML(socket);

                        valid = true;
                    }

                    //If the first line of text is anything else, the server should send back (see below) an HTML page containing an error message.

                        if (!valid)
                        {
                            string error = "The Page Requested Does Not Exist";
                            beginHTML(socket);
                            anythingElseHTML(socket, error);
                            endHTML(socket);
                        }
                    

                }
              

                
                //socket.BeginReceive(WebCommandReceived, socket);

                Thread.Sleep(1000);
                
                socket.shutdown();
                socket.close();
            }
        }

        /// <summary>
        /// Gets the proper substring from a command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string getProperSubstring(string command)
        {
            int index1 = command.IndexOf("=") + 1;
            int index2 = command.LastIndexOf(" ");
            int testlength = command.Length;
            //I know this is not ideal, but for some reason it was giving me an error to substring(index1, index2)
            command = command.Substring(0, index2);
            command = command.Remove(0, index1);
            return command;
        }


        /// <summary>
        /// Generates the html header and connection info
        /// </summary>
        /// <param name="socket"></param>
        private void beginHTML(StringSocket socket)
        {
            socket.BeginSend("HTTP/1.1 200 OK\r\n", (e, p) => { }, null);
            socket.BeginSend("Connection: close\r\n", (e, p) => { }, null);
            socket.BeginSend("Content-Type: text/html; charset=UTF-8\r\n", (e, p) => { }, null);
            socket.BeginSend("\r\n", (e, p) => { }, null);
            
            socket.BeginSend("<!DOCTYPE html>\r\n", (e, p) => { }, null);
            socket.BeginSend("<html>\r\n", (e, p) => { }, null);
            socket.BeginSend("<body>\r\n", (e, p) => { }, null);            
        }
        
        /// <summary>
        /// Closes the HTML body
        /// </summary>
        /// <param name="socket"></param>
        private void endHTML(StringSocket socket)
        {
            socket.BeginSend("</body>\r\n", (e, p) => { }, null);
            socket.BeginSend("</html>\r\n", (e, p) => { }, null);     
      
        }


        /// <summary>
        /// Sends an HTML page that summarizes a games information
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="gameID"></param>
        private void gameHTML(StringSocket socket, int gameID)
        {
           //The page should contain the names and scores of the two players involved, the date and time when the game was played, a 4x4 table containing the Boggle board that was used,
           //the time limit that was used for the game, and the five-part word summary.
           //If there the specified game does not exist, treat this as an "anything else" case as discussed below.

            int p1ID = 0;
            int p2ID = 0;
            int p1Score = 0;
            int p2Score = 0;
            DateTime time = new DateTime(); 
            string board = "";
            int timeLimit = 0;
            string Player1Name = "";
            string Player2Name = "";

            HashSet<string> p1Legal = new HashSet<string>();
            HashSet<string> p2Legal = new HashSet<string>();
            HashSet<string> p1illegal = new HashSet<string>();
            HashSet<string> p2illegal = new HashSet<string>();
            HashSet<string> common = new HashSet<string>();

            //used in html transmission
            string p1legals;
            string p2legals;
            string p1illegals;
            string p2illegals;
            string commons;

            try
            {

                //prep data from db
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // Open a connection
                    conn.Open();

                    MySqlCommand command;
                    // get pid 
                    command = conn.CreateCommand();
                    command.CommandText =
                       @"SELECT * from matelau.Games where idGames = @gameID";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@gameID", gameID);


                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        p1ID = (int)reader["Player1ID"];
                        p2ID = (int)reader["Player2ID"];
                        p1Score = (int)reader["Player1Score"];
                        p2Score = (int)reader["Player2Score"];
                        time = (DateTime)reader["Time"];
                        board = (string)reader["Board"];
                        timeLimit = (int)reader["TimeLimit"];
                    }

                    Player1Name = getPlayerNameFromID(p1ID);
                    Player2Name = getPlayerNameFromID(p2ID);

                    //get the common words for p1
                    command.CommandText =
                      @"SELECT * from matelau.WordsPlayed where GameId = @gameID0 and playerID = @pID0 and Legal = 1 and Common = 1";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@gameID0", gameID);
                    command.Parameters.AddWithValue("@pID0", p1ID);

                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            common.Add((string)reader["Word"]);
                        }
                    }

                    //get the legal words for p1
                    command.CommandText =
                      @"SELECT * from matelau.WordsPlayed where GameId = @gameID2 and playerID = @pID and legal = 1";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@gameID2", gameID);
                    command.Parameters.AddWithValue("@pID", p1ID);

                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            p1Legal.Add((string)reader["Word"]);
                        }
                    }

                    //get the illegal words for p1
                    command.CommandText =
                      @"SELECT * from matelau.WordsPlayed where GameId = @gameID3 and playerID = @pID2 and legal = 0";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@gameID3", gameID);
                    command.Parameters.AddWithValue("@pID2", p1ID);

                    // Execute the comman
                    command.ExecuteNonQuery();

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            p1illegal.Add((string)reader["Word"]);
                        }
                    }

                    //~~~~~~~ player 2 ~~~~~~~~~
                    //get the legal words for p2
                    command.CommandText =
                      @"SELECT * from matelau.WordsPlayed where GameId = @gameID4 and playerID = @pID3 and legal = 1";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@gameID4", gameID);
                    command.Parameters.AddWithValue("@pID3", p2ID);

                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            p2Legal.Add((string)reader["Word"]);
                        }
                    }

                    //get the illegal words for p1
                    command.CommandText =
                      @"SELECT * from matelau.WordsPlayed where GameId = @gameID5 and playerID = @pID4 and legal = 0";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@gameID5", gameID);
                    command.Parameters.AddWithValue("@pID4", p2ID);

                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            p2illegal.Add((string)reader["Word"]);
                        }
                    }

                    //clean up p1legal
                    foreach (string s in common)
                    {
                        p1Legal.Remove(s); 
                    }
                     
                    p1legals = string.Join(", ", p1Legal);
                    p1illegals = string.Join(", ", p1illegal);
                    p2legals = string.Join(", ", p2Legal);
                    p2illegals = string.Join(", ", p2illegal);
                    commons = string.Join(", ", common);




                }


                //send headers
                socket.BeginSend("<table border=\"1\">\r\n", (e, p) => { }, null);
                socket.BeginSend("<tr>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Player 1</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Player 2</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Player1 Score</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Player2 Score</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Game Time Limit</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Date & Time</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Player 1 Legal</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Player 2 Legal</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Player 1 Illegal</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Player 2 Illegal</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("<th>Common Words</th>\r\n", (e, p) => { }, null);
                socket.BeginSend("</tr>\r\n", (e, p) => { }, null);

                //open row
                socket.BeginSend("<tr>\r\n", (e, p) => { }, null);

                //send data to be displayed
                socket.BeginSend("<td>" + Player1Name + "</td>\r\n", (e, p) => { }, null);
                socket.BeginSend("<td>" + Player2Name + "</td>\r\n", (e, p) => { }, null);

                socket.BeginSend("<td>" + p1Score + "</td>\r\n", (e, p) => { }, null);
                socket.BeginSend("<td>" + p2Score + "</td>\r\n", (e, p) => { }, null);

                socket.BeginSend("<td>" + timeLimit + "</td>\r\n", (e, p) => { }, null);
                socket.BeginSend("<td>" + time + "</td>\r\n", (e, p) => { }, null);

                //send word report
                socket.BeginSend("<td>" + p1legals + "</td>\r\n", (e, p) => { }, null);
                socket.BeginSend("<td>" + p2legals + "</td>\r\n", (e, p) => { }, null);
                socket.BeginSend("<td>" + p1illegals + "</td>\r\n", (e, p) => { }, null);
                socket.BeginSend("<td>" + p2illegals + "</td>\r\n", (e, p) => { }, null);
                socket.BeginSend("<td>" + commons + "</td>\r\n", (e, p) => { }, null);


                //close row
                socket.BeginSend("</tr>\r\n", (e, p) => { }, null);
                socket.BeginSend("</table>\r\n", (e, p) => { }, null);

                socket.BeginSend("<br>\r\n", (e, p) => { }, null);

                //display boggle board table
                socket.BeginSend("<table align = \"center\" border=\"1\">\r\n", (e, p) => { }, null);
                socket.BeginSend("<tr>\r\n", (e, p) => { }, null);

                for (int i = 0; i < 16; i++)
                {
                    socket.BeginSend("<td>" + board[i] + "</td>\r\n", (e, p) => { }, null);
                    //close rows
                    if (i == 3 || i == 7 || i == 11 || i == 15)
                    {
                        socket.BeginSend("</tr>\r\n", (e, p) => { }, null);
                    }
                }

                socket.BeginSend("</table>\r\n", (e, p) => { }, null);
            }
            catch (MySqlException)
            {
                anythingElseHTML(socket, "Error: The Game Requested Does Not Exist"); 
            }
        }


        /// <summary>
        /// Sends general error messages in HTML
        /// </summary>
        /// <param name="socker"></param>
        /// <param name="message"></param>
        private void anythingElseHTML(StringSocket socket, string message)
        {
            socket.BeginSend("<p>\r\n", (e, p) => { }, null);
            socket.BeginSend(message + " \r\n", (e, p) => { }, null);
            socket.BeginSend("</p>\r\n", (e, p) => { }, null);
        }

        /// <summary>
        /// Sends the HTML for GET /players HTTP/1.1 
        /// </summary>
        /// <param name="socket"></param>
        private void playersHTML(StringSocket socket)
        {
            // There should be one row for each player in the database and four columns.  
            // Each row should consist of the player's name, the number of games won by the player, 
            // the number of games lost by the player, and the number of games tied by the player.

            // socket.BeginSend("\r\n", (e, p) => { }, null);
            socket.BeginSend("<table border=\"1\">\r\n", (e, p) => { }, null);
            socket.BeginSend("<tr>\r\n", (e, p) => { }, null);
            socket.BeginSend("<th>Player Name</th>\r\n", (e, p) => { }, null);
            socket.BeginSend("<th>Games Won</th>\r\n", (e, p) => { }, null);
            socket.BeginSend("<th>Games Lost</th>\r\n", (e, p) => { }, null);
            socket.BeginSend("<th>Games Tied</th>\r\n", (e, p) => { }, null);
            socket.BeginSend("</tr>\r\n", (e, p) => { }, null);            

            int count = getPlayerCount();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection
                conn.Open();

                MySqlCommand command;

                for (int i = 1; i <= count; i++)
                {
                    command = conn.CreateCommand();
                    socket.BeginSend("<tr>\r\n", (e, p) => { }, null);

                    command.CommandText = @"select (Name) from Players where idPlayers = (@id)";
                    command.Prepare();
                    command.Parameters.AddWithValue("@id", i);

                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Get the name of the player
                    string name = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        name = reader.GetString(0);
                        socket.BeginSend("<td>" + name + "</td>\r\n", (e, p) => { }, null);
                    }

                    // number of wins
                    command = conn.CreateCommand();
                    command.CommandText = @"select count(*) from Games where Player1ID = (@id1) and Player1Score > Player2Score";
                    command.Prepare();
                    command.Parameters.AddWithValue("@id1", i);


                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Get the number of wins
                    int wins = 0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        wins = reader.GetInt32(0);
                    }

                    command.CommandText = @"select count(*) from Games where Player2ID = (@id2) and Player1Score < Player2Score";
                    command.Prepare();
                    command.Parameters.AddWithValue("@id2", i);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        wins += reader.GetInt32(0);
                        socket.BeginSend("<td>" + wins + "</td>\r\n", (e, p) => { }, null);
                    }

                    // number of losses
                    command = conn.CreateCommand();
                    command.CommandText = @"select count(*) from Games where Player1ID = (@id1) and Player1Score < Player2Score";
                    command.Prepare();
                    command.Parameters.AddWithValue("@id1", i);


                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Get the number of wins
                    int losses = 0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        losses = reader.GetInt32(0);

                    }

                    command.CommandText = @"select count(*) from Games where Player2ID = (@id2) and Player1Score > Player2Score";
                    command.Prepare();
                    command.Parameters.AddWithValue("@id2", i);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        losses += reader.GetInt32(0);
                        socket.BeginSend("<td>" + losses + "</td>\r\n", (e, p) => { }, null);
                    }

                    // number of ties
                    command = conn.CreateCommand();
                    command.CommandText = @"select count(*) from Games where Player1ID = (@id1) and Player1Score = Player2Score";
                    command.Prepare();
                    command.Parameters.AddWithValue("@id1", i);


                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Get the number of wins
                    int ties = 0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        ties = reader.GetInt32(0);

                    }

                    command.CommandText = @"select count(*) from Games where Player2ID = (@id2) and Player1Score = Player2Score";
                    command.Prepare();
                    command.Parameters.AddWithValue("@id2", i);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        ties += reader.GetInt32(0);
                        socket.BeginSend("<td>" + ties + "</td>\r\n", (e, p) => { }, null);
                    }

                    socket.BeginSend("</tr>\r\n", (e, p) => { }, null);
                }

                conn.Close();
            }

            socket.BeginSend("</table>\r\n", (e, p) => { }, null);                      

        }

        /// <summary>
        /// Sends an individual player's record. six columns 1- game number, 2- games played, 3- date/time, 4- opponent name, 5- player score, 6- opponent score 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="playerName"></param>
        private void playerHTML(StringSocket socket, string playerName)
        {
            //The server should send back (see below) an HTML page containing a table of information.  
            //There should be one row for each game played by the player named in the line of text (e.g., "Joe" in the example above) and six columns.  
            //Each row should consist of a number that uniquely  identifies the game (see the next paragraph for how that number will be used), 
            //the date and time when the game was played, the name of the opponent, the score for the named player, and the score for the opponent.

            

            //get the number of games "player" has particpated in 

            int count = getGameCount(playerName);
            try
            {
          
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // Open a connection
                    conn.Open();

                    MySqlCommand command;

                    command = conn.CreateCommand();

                    int pID = getPlayerID(playerName);
                    if (pID == -1)
                    {
                        throw new Exception();
                    }

                    socket.BeginSend("<table border=\"1\">\r\n", (e, p) => { }, null);
                    socket.BeginSend("<tr>\r\n", (e, p) => { }, null);
                    socket.BeginSend("<th>Game Number</th>\r\n", (e, p) => { }, null);
                    socket.BeginSend("<th>Game ID</th>\r\n", (e, p) => { }, null);
                    socket.BeginSend("<th>Date & Time</th>\r\n", (e, p) => { }, null);
                    socket.BeginSend("<th>Opponent</th>\r\n", (e, p) => { }, null);
                    socket.BeginSend("<th>Player Score</th>\r\n", (e, p) => { }, null);
                    socket.BeginSend("<th>Opponent Score</th>\r\n", (e, p) => { }, null);
                    socket.BeginSend("</tr>\r\n", (e, p) => { }, null);

                    int[] gameIDS = new int[count];
                    DateTime[] times = new DateTime[count];
                    string[] opponentNames = new string[count];
                    int[] playerScores = new int[count];
                    int[] oppScores = new int[count];

                    //get all the games where player was player 1
                    command.CommandText =
                       @"SELECT * from matelau.Games where Player1ID = @pID";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@pID", pID);

                    int index = 0;
                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //get game ids
                            gameIDS[index] = (int)reader["idGames"];
                            //gets game played times
                            times[index] = (DateTime)reader["Time"];
                            //get opponent names
                            opponentNames[index] = getPlayerNameFromID((int)reader["Player2ID"]);
                            //get player Scores
                            playerScores[index] = (int)reader["Player1Score"];
                            //get oppScores
                            oppScores[index] = (int)reader["Player2Score"];
                            index++;
                        }
                    }
                    //check to see if any games remain where player was the opponent
                    if (index != count)
                    {
                        //get all the games where player was player 2
                        command.CommandText =
                           @"SELECT * from matelau.Games where Player2ID = @pID2";

                        // Prepare the command
                        command.Prepare();
                        command.Parameters.AddWithValue("@pID2", pID);


                        // Execute the command and cycle through the DataReader object
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //get game ids
                                gameIDS[index] = (int)reader["idGames"];
                                //gets game played times
                                times[index] = (DateTime)reader["Time"];
                                //get opponent names
                                opponentNames[index] = getPlayerNameFromID((int)reader["Player1ID"]);
                                //get player Scores
                                playerScores[index] = (int)reader["Player2Score"];
                                //get oppScores
                                oppScores[index] = (int)reader["Player1Score"];
                                index++;
                            }
                        }
                    }

                    //prepare table data
                    for (int i = 1; i <= count; i++)
                    {
                        //open row
                        socket.BeginSend("<tr>\r\n", (e, p) => { }, null);
                        //number of games
                        socket.BeginSend("<td>" + i + "</td>\r\n", (e, p) => { }, null);

                        //display game Ids
                        socket.BeginSend("<td>" + gameIDS[i - 1] + "</td>\r\n", (e, p) => { }, null);

                        //display times
                        socket.BeginSend("<td>" + times[i - 1].ToString() + "</td>\r\n", (e, p) => { }, null);

                        //display opp Names
                        socket.BeginSend("<td>" + opponentNames[i - 1] + "</td>\r\n", (e, p) => { }, null);

                        //display player Scores
                        socket.BeginSend("<td>" + playerScores[i - 1] + "</td>\r\n", (e, p) => { }, null);

                        //display opp Scores
                        socket.BeginSend("<td>" + oppScores[i - 1] + "</td>\r\n", (e, p) => { }, null);

                        //close the row
                        socket.BeginSend("</tr>\r\n", (e, p) => { }, null);
                    }

                    conn.Close();
                }
                socket.BeginSend("</table>\r\n", (e, p) => { }, null);
            }
            catch (MySqlException)
            {
                anythingElseHTML(socket, "Error: The Player Requested Does Not Exist");
            }
            catch (Exception)
            {
                anythingElseHTML(socket, "Error: The Player Requested Does Not Exist");
            }

        }

        /// <summary>
        /// Gets the player name from the database from an ID, null if the players name is not found
        /// </summary>
        /// <param name="PlayerID"></param>
        /// <returns></returns>
        private string getPlayerNameFromID(int PlayerID)
        {
            string playerName = null;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // Open a connection
                    conn.Open();

                    MySqlCommand command;


                    command = conn.CreateCommand();


                    command.CommandText = @"select (Name) from Players where idPlayers = (@id)";
                    command.Prepare();
                    command.Parameters.AddWithValue("@id", PlayerID);

                    // Execute the command 
                    command.ExecuteNonQuery();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        playerName = reader.GetString(0);
                     
                    }  
                }

            }
            catch (MySqlException)
            {
                playerName = null;
            }

            return playerName;

        }

        /// <summary>
        /// Returns the Players ID from the Database if available
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        private int getPlayerID(string playerName)
        {
            try
            {
                int pID;
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    // Open a connection
                    conn.Open();

                    MySqlCommand command;
                    // get pid 
                    command = conn.CreateCommand();
                    command.CommandText =
                       @"SELECT idPlayers FROM matelau.Players where Players.Name = @PlayerName";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@PlayerName", playerName);


                    // Execute the command 
                    command.ExecuteNonQuery();

                    // Execute the command and cycle through the DataReader object
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        reader.Read();
                        pID = (int)reader["idPlayers"];
                    }
                }
                return pID;
            }
            catch (MySqlException)
            {
                return -1;                
            }
        }

        /// <summary>
        /// Returns the number of players stored in the database
        /// </summary>
        /// <returns></returns>
        private int getPlayerCount()
        {
            int count = 0;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection
                conn.Open();

                // Create a command
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = @"SELECT COUNT(*) FROM matelau.Players";

                // Prepare the command
                command.Prepare();

                // Execute the command 
                count = (int) command.ExecuteNonQuery();

                // Get the number of players in the database
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    count = reader.GetInt32(0);
                }
                
                conn.Close();
            }
            return count;
        }


        /// <summary>
        /// Returns the number of games stored in the database for a particular player
        /// </summary>
        /// <returns></returns>
        private int getGameCount(string playerName)
        {
            int count = 0;
            int pID; 

          
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection
                conn.Open();
                MySqlCommand command = conn.CreateCommand();

                // get the player's Id
                 pID = getPlayerID(playerName);

                // Create a command
                command = conn.CreateCommand();
                command.CommandText = @"SELECT COUNT(*) FROM matelau.Games where Games.Player1ID = @pID";

                // Prepare the command
                command.Prepare();
                command.Parameters.AddWithValue("@pID", pID);

                // Execute the command 
                count = (int)command.ExecuteNonQuery();

                // Get the number of games in the database where player is number 1
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    count = reader.GetInt32(0);
                }

                // Create a command
                command = conn.CreateCommand();
                command.CommandText = @"SELECT COUNT(*) FROM matelau.Games where Games.Player2ID = @pID2";

                // Prepare the command
                command.Prepare();
                command.Parameters.AddWithValue("@pID2", pID);

                // Execute the command 
                command.ExecuteNonQuery();

                // Get the number of games in the database where player is number 2
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    count = count + reader.GetInt32(0);
                }
                conn.Close();
            }
            return count;
        }

        /// <summary>
        /// parses the command, removing "\r" and setting it ToUpper
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string parseCommand(string command)
        {
            if (command.Contains("\r"))
            {
                command = command.Substring(0, command.Length - 1);
            }
            command = command.ToUpper();

            return command;
        }

        /// <summary>
        /// Notifies the remaining player that the games has been terminated
        /// </summary>
        /// <param name="ss"></param>
        private void terminated(StringSocket ss)
        {
            lock (currentGames)
            {

                int gameNumber = ss.Game;
                BoggleGame game;
                // check if the closed socket has another socket associated with it in a currentgame
                if (currentGames.TryGetValue(gameNumber, out game))
                {
                    if (ss.Player == 1 && game.Player2() != null)
                    {
                        game.Player2().BeginSend("TERMINATED\n", (e, p) => { }, null);
                        game.shutdown();
                    }

                    if (ss.Player == 2)
                    {
                        game.Player1().BeginSend("TERMINATED\n", (e, p) => { }, null);
                        game.shutdown();
                    }
                }
                //the sockets are in the setup game
                else
                {
                    if (ss != null)
                    {
                        ss.close();
                    }
                    createNewGame();
                }
            }
        }
       
        /// <summary>
        /// Adds the player to a game.  If a pair is completed starts the game
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="name"></param>
        private void play(StringSocket socket, string name)
        {
            //Queue up incoming requests
            PlayQ.Enqueue(new Tuple<StringSocket, string>(socket, name));

            //lock processing
            lock (setupGame)
            {
                //process current requests
                while (PlayQ.Count != 0)
                {
                    Tuple<StringSocket, string> playCmd = PlayQ.Dequeue();
                    StringSocket ss = playCmd.Item1;
                    string playerName = playCmd.Item2;
                    //add player to the setup game, check if two players are present
                    if (setupGame.addPlayer(playerName, ss, gameNumber))
                    {
                        currentGames.Add(gameNumber, setupGame);
                        gameNumber++;
                        // signals client to that new game has started
                        string msg = "START " + setupGame.Board() + " " + gameLength + " " + setupGame.Player2Name() + "\n";
                        setupGame.Player1().BeginSend(msg, (e, p) => { }, null);
                        msg = "START " + setupGame.Board() + " " + gameLength + " " + setupGame.Player1Name() + "\n";
                        setupGame.GameInSession = true;
                        setupGame.Player2().BeginSend(msg, (e, p) => { }, null);

                        createNewGame();
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new boggle game
        /// </summary>
        private void createNewGame()
        {
            lock (setupGame)
            {
                BoggleBoard newBoard;
                if (customBoard == null)
                    newBoard = new BoggleBoard();
                else
                    newBoard = new BoggleBoard(customBoard);

                setupGame = new BoggleGame(legalWords, gameLength, newBoard);
            }

        }

        /// <summary>
        /// Helper method to handle the word command
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="word"></param>
        private void word(StringSocket socket, string word)
        {
            //Que up requests for the TCPListener
            WordQ.Enqueue(new Tuple<StringSocket, string>(socket, word));

            //lock processing
            lock (CurrentGames)
            {
                //process all current requests
                while (WordQ.Count != 0)
                {
                    Tuple<StringSocket, string> wordRequest = WordQ.Dequeue();
                    StringSocket ss = wordRequest.Item1;
                    string submittedWord = wordRequest.Item2;
                    int gameNumber = ss.Game;
                    //check and see if the socket is in a valid game and the length of the word is of the proper length to be considered
                    if (currentGames.ContainsKey(gameNumber) && submittedWord.Length > 2)
                    {
                        // add the word for the correct player 
                        int playerNumber = ss.Player;
                        currentGames[gameNumber].addWord(submittedWord, playerNumber);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the timers of all current games, if the game is finished sends summary message and closes sockets
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void updateTimers(object source, ElapsedEventArgs e)
        {
            lock (currentGames)
            {
                // if games are currently running
                if (currentGames.Count > 0)
                {
                    foreach (BoggleGame game in currentGames.Values)
                    {
                        //returns true when the game is over
                        if (game.updateTime())
                        {
                            game.GameInSession = false;
                            //shut game down and report proper values
                            game.sendScore();
                            //send summaries
                            game.summary();
                            //close sockets
                            game.shutdown();
                            //remove the game from the list of games

                            //update db
                            updateDatabase(game); 


                        }
                        //send Time remaining
                        else
                        {
                            //send time remaining
                            int timeRemaining = game.Duration;
                            string timeString = "TIME " + timeRemaining + "\n";
                            game.Player1().BeginSend(timeString, (d, p) => { }, game.Player1());
                            game.Player2().BeginSend(timeString, (d, p) => { }, game.Player2());
                        } 
                    }

                }

            }
             
        }

        /// <summary>
        /// Updates the database at the conclusion of a game 
        /// </summary>
        /// <param name="game"></param>
        private void updateDatabase(BoggleGame game)
        {
            //add players
            addPlayer(game.Player2Name());
            addPlayer(game.Player1Name());

            //add game
            DateTime time = addGame(game);

            //add words played
            addWords(game, time);


        }

        /// <summary>
        /// Adds specific Players to the Database
        /// </summary>
        /// <param name="PlayerName"></param>
        private void addPlayer(String PlayerName)
        {
            if (!playerExists(PlayerName)) {
            // Create connection object
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {

                    // Open a connection
                    conn.Open();

                    // Create a command
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = @"insert into Players (Name) values(@PlayerName)";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@PlayerName", PlayerName);


                    // Execute the command 
                    command.ExecuteNonQuery();

                    conn.Close();

                }
            }          
        }

        /// <summary>
        /// Returns whether a player is already in the database
        /// </summary>
        /// <param name="PlayerName"></param>
        /// <returns></returns>
        private bool playerExists(String PlayerName)
        {
            
            int count = 0;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection
                conn.Open();

                // Create a command
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = @"SELECT COUNT(*) from Players where Name = (@PlayerName)";

                // Prepare the command
                command.Prepare();
                command.Parameters.AddWithValue("@PlayerName", PlayerName);

                // Execute the command 
                count = (int) command.ExecuteNonQuery();

                // Get the number of players in the database
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    count = reader.GetInt32(0);
                }
                
                conn.Close();
            }            
        
            return count == 1;
        }

        /// <summary>
        /// updates the game table of the database
        /// </summary>
        /// <param name="game"></param>
        private DateTime addGame(BoggleGame game)
        {
            int p1ID;
            int p2ID;
            String board = game.Board();
            int timeLimit = gameLength;
            int p1Score = game.Player1Score;
            int p2Score = game.Player2Score;
            DateTime time = DateTime.Now; 
            // Create connection object
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection
                conn.Open();

                // Create a command
                MySqlCommand command = conn.CreateCommand();
                command.CommandText =
                    @"SELECT idPlayers FROM matelau.Players where Players.Name = @PlayerName or Players.Name = @PlayerName2";

                // Prepare the command
                command.Prepare();
                command.Parameters.AddWithValue("@PlayerName", game.Player1Name());
                command.Parameters.AddWithValue("@PlayerName2", game.Player2Name());

                // Execute the command 
                command.ExecuteNonQuery();

                // Execute the command and cycle through the DataReader object
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    p1ID = (int) reader["idPlayers"];
                    reader.Read();
                    p2ID = (int)reader["idPlayers"]; 
                }                

                // Create a command
                command = conn.CreateCommand();
                command.CommandText =
                    @"insert into Games (Player1ID, Player2Id, Time, Board, TimeLimit, Player1Score, Player2Score) values (@Player1ID, @Player2Id, @Time, @Board, @TimeLimit, @Player1Score, @Player2Score)";

                // Prepare the command
                command.Prepare();
                command.Parameters.AddWithValue("@Player1ID", p1ID);
                command.Parameters.AddWithValue("@Player2ID", p2ID);
                command.Parameters.AddWithValue("@Time", time);
                command.Parameters.AddWithValue("@Board", board);
                command.Parameters.AddWithValue("@TimeLimit", timeLimit);
                command.Parameters.AddWithValue("@Player1Score", p1Score);
                command.Parameters.AddWithValue("@Player2Score", p2Score);

                // Execute the command 
                command.ExecuteNonQuery();

                conn.Close();
            }
            return time;

        }

        /// <summary>
        /// Adds this games words to the database
        /// </summary>
        /// <param name="game"></param>
        private void addWords(BoggleGame game, DateTime time)
        {
            int p1ID;
            int p2ID; 

            // Create connection object
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Open a connection
                conn.Open();

                //get the gameId
                int gameId;
                // Create a command
                MySqlCommand command = conn.CreateCommand();
                command.CommandText =
                    @"SELECT idGames FROM matelau.Games where Games.Time = @time and Games.Board = @Board";

                // Prepare the command
                command.Prepare();
                command.Parameters.AddWithValue("@time", time);
                command.Parameters.AddWithValue("@Board", game.Board());

                // Execute the command 
                command.ExecuteNonQuery();

                // Execute the command and cycle through the DataReader object
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    gameId = (int)reader["idGames"];
                }                

                // get the players Ids
                
                // Create a command
                command = conn.CreateCommand();
                command.CommandText =
                    @"SELECT idPlayers FROM matelau.Players where Players.Name = @PlayerName or Players.Name = @PlayerName2";

                // Prepare the command
                command.Prepare();
                command.Parameters.AddWithValue("@PlayerName", game.Player1Name());
                command.Parameters.AddWithValue("@PlayerName2", game.Player2Name());

                // Execute the command 
                command.ExecuteNonQuery();

                // Execute the command and cycle through the DataReader object
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    p1ID = (int)reader["idPlayers"];
                    reader.Read();
                    p2ID = (int)reader["idPlayers"];
                }
                
                //add words p1
                HashSet<String> p1Legal = game.P1Legal;
                foreach (String word in p1Legal)
                {
                    // Create a command
                    command = conn.CreateCommand();
                    command.CommandText =
                        @"insert into WordsPlayed (GameId, Word, Legal,PlayerID) 
                             values(@GameId, @Word, @Legal, @PlayerID)";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@GameId", gameId);
                    command.Parameters.AddWithValue("@Word", word);
                    command.Parameters.AddWithValue("@Legal", 1);
                    command.Parameters.AddWithValue("@PlayerID", p1ID );


                    // Execute the command 
                    command.ExecuteNonQuery();
                    
                }

                HashSet<String> p1Illegal = game.P1Illegal;
                foreach (String word in p1Illegal)
                {
                    // Create a command
                    command = conn.CreateCommand();
                    command.CommandText =
                        @"insert into WordsPlayed (GameId, Word, Legal,PlayerID) 
                             values(@GameId, @Word, @Legal, @PlayerID)";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@GameId", gameId);
                    command.Parameters.AddWithValue("@Word", word);
                    //illegal
                    command.Parameters.AddWithValue("@Legal", 0);
                    command.Parameters.AddWithValue("@PlayerID", p1ID);


                    // Execute the command 
                    command.ExecuteNonQuery();

                }

                HashSet<String> p2Legal = game.P2Legal;
                foreach (String word in p2Legal)
                {
                    // Create a command
                    command = conn.CreateCommand();
                    command.CommandText =
                        @"insert into WordsPlayed (GameId, Word, Legal,PlayerID) 
                             values(@GameId, @Word, @Legal, @PlayerID)";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@GameId", gameId);
                    command.Parameters.AddWithValue("@Word", word);
                    //illegal
                    command.Parameters.AddWithValue("@Legal", 1);
                    command.Parameters.AddWithValue("@PlayerID", p2ID);


                    // Execute the command 
                    command.ExecuteNonQuery();

                }
                HashSet<String> p2Illegal = game.P2Illegal;
                foreach (String word in p2Illegal)
                {
                    // Create a command
                    command = conn.CreateCommand();
                    command.CommandText =
                        @"insert into WordsPlayed (GameId, Word, Legal,PlayerID) 
                             values(@GameId, @Word, @Legal, @PlayerID)";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@GameId", gameId);
                    command.Parameters.AddWithValue("@Word", word);
                    //illegal
                    command.Parameters.AddWithValue("@Legal", 0);
                    command.Parameters.AddWithValue("@PlayerID", p2ID);


                    // Execute the command 
                    command.ExecuteNonQuery();                  
                }

                // add common words

                HashSet<String> common = game.Common;
                foreach (String word in common)
                {
                    // Create a command
                    command = conn.CreateCommand();
                    command.CommandText =
                        @"insert into WordsPlayed (GameId, Word, Legal,PlayerID, Common) 
                             values(@GameId, @Word, @Legal, @PlayerID, @Common)";

                    // Prepare the command
                    command.Prepare();
                    command.Parameters.AddWithValue("@GameId", gameId);
                    command.Parameters.AddWithValue("@Word", word);
                    //legal and common
                    command.Parameters.AddWithValue("@Legal", 1);
                    command.Parameters.AddWithValue("@Common", 1);
                    command.Parameters.AddWithValue("@PlayerID", p1ID);



                    // Execute the command 
                    command.ExecuteNonQuery();
                }

                conn.Close();
            }

        }

        /// <summary>
        /// Helper method used to close the underlying TCPListener
        /// </summary>
        public void close()
        {
            server.Stop();
            currentGames.Clear();
        }
    }
}
