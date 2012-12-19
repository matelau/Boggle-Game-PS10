// Written by Asaeli Matelau for CS3500 Assignment PS10
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BoggleClient
{
    public static class BoggleClient
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BoggleClientView());
        }
    }
}
