// Written by Asaeli Matelau for CS3500 Assignment PS10
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomNetworking;
using System.Net.Sockets;
using System.Threading;

namespace BoggleClient
{
    
    public class BoggleClientModel
    {
        private StringSocket socket;
        private object critLock = new Object(); 

        // Register events.
        public event Action<String> IncomingTimeEvent;
        public event Action<String> IncomingStartEvent;
        public event Action<String> IncomingScoreEvent;
        public event Action<String> IncomingTerminatedEvent;
        public event Action<String> IncomingStopEvent;
        public event Action<String> noSuchHostEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        public BoggleClientModel()
        {
            socket = null; 
        }


        /// <summary>
        /// Connect to the server at the given hostname and port and with the give name.
        /// </summary>
        public void Connect(string IPAddress, String name)
        {           
            if (socket == null)
            {
                try
                {
                    TcpClient client = new TcpClient(IPAddress, 2000);
                    socket = new StringSocket(client.Client, UTF8Encoding.Default);
                    socket.BeginSend("PLAY " + name + "\n", (e, p) => { }, null);
                    socket.BeginReceive(LineReceived, null);
                }
                catch (Exception e)
                {
                    //no such host event
                    noSuchHostEvent(e.Message);
                }
            }
        }

        /// <summary>
        /// Send a line of text to the server.
        /// </summary>
        /// <param name="line"></param>
        public void SendMessage(String line)
        {
            if (socket != null)
            {
                socket.BeginSend(line + "\n", (e, p) => { }, null);
            }
        }

        /// <summary>
        /// Deal with an arriving line of text.
        /// </summary>
        private void LineReceived(String s, Exception e, object p)
        {
            //separate for different commands to each have their own event 

            lock (critLock)
            {
                string test = s;

                if (s != null)
                {

                    test = test.ToUpper();

                    //start
                    if (IncomingStartEvent != null && test.Contains("START") && !(test.Contains("WORD")))
                    {

                        string line = s.Remove(0, 6);
                        IncomingStartEvent(line);
                    }

                    //score
                    if (IncomingScoreEvent != null && test.Contains("SCORE") && !(test.Contains("WORD")))
                    {
                        string line = s.Remove(0, 6);
                        IncomingScoreEvent(line);
                    }

                    //terminated
                    if (test.Length > 8)
                    {
                        if (IncomingTerminatedEvent != null && test.Contains("TERMINATED"))
                        {
                            string line = test;
                            IncomingTerminatedEvent(line);
                        }
                    }
                    //time
                    if (IncomingTimeEvent != null && test.Contains("TIME") && !(test.Contains("WORD")))
                    {
                        //trim command from string
                        string line = s.Remove(0, 4);
                        IncomingTimeEvent(line);
                    }

                    //stop
                    if (IncomingStopEvent != null && test.Contains("STOP") && !(test.Contains("WORD")))
                    {
                        string line = s.Remove(0, 4);
                        IncomingStopEvent(line);
                    }

                    //else ignoring
                }
            }

           //start receiving 
            if (socket != null)
            {
                socket.BeginReceive(LineReceived, null);
            }
        }

        /// <summary>
        /// Disconnects the String Socket from the server
        /// </summary>
        public void Disconnect()
        {
            //allows for unfinished business to process before closing
            //This stall made the method more reliable
            Thread.Sleep(1000);
            if (socket != null)
            {
                socket.close();
            }

            //set to null to reset for next game
            socket = null;
        }

        /// <summary>
        /// sends a terminated command
        /// </summary>
        public void terminate()
        {
            
            if (socket != null)
            {
                socket.BeginSend("TERMINATED \n", (e, p) => { }, null);
            }

            Disconnect();
        }
    }
}


