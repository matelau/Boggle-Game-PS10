// Written by Asaeli Matelau for CS3500 Assignment PS10
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BS;
using BoggleClient;
using System.Threading;

namespace Launcher
{
    class Launcher
    {
        /// <summary>
        /// Launches a Boggle server and two Boggle clients
        /// </summary>
        static void Main(string[] args)
        {
            new BoggleServer(80, @"..\..\dictionary.txt");
            new Thread(() => BoggleClient.BoggleClient.Main()).Start();
            new Thread(() => BoggleClient.BoggleClient.Main()).Start();
           // new Thread(() => BoggleClient.BoggleClient.Main()).Start();
           // new Thread(() => BoggleClient.BoggleClient.Main()).Start(); 
        }
    }
}
