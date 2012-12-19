using BoggleClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using BS;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;


namespace BoggleClientTest
{
    
    
    /// <summary>
    ///This is a test class for BoggleClientModelTest and is intended
    ///to contain all BoggleClientModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BoggleClientModelTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for BoggleClientModel Constructor
        ///</summary>
        [TestMethod()]
        public void BoggleClientModelConstructorTest()
        {
            BoggleClientModel target = new BoggleClientModel();
            //Not yet implemented
          
        }

        /// <summary>
        ///A test for Connect
        ///</summary>
        [TestMethod()]
        public void ConnectTest()
        {

            new ConnectTest1().run();     
      
        }


        /// <summary>
        /// Helper Class to test that a IncomingStartEvent is fired as expected
        /// </summary>
        public class ConnectTest1
        {
            
            private ManualResetEvent MRE = new ManualResetEvent(false);
            private int timeout = 1000; 
            BoggleServer server;

            public void run()
            {
               try{
                //setup server
                 server = new BoggleServer(80, "dictionary.txt");

                //setup clients
                BoggleClientModel target = new BoggleClientModel();
                string IPAddress = "127.0.0.1";
                string name = "test";

                BoggleClientModel target2 = new BoggleClientModel();
                string name2 = "test2";


                target.IncomingStartEvent += delegate(string line)
                {
                    MRE.Set();
                };

                //trigger event
                target.Connect(IPAddress, name);
                target2.Connect(IPAddress, name2);

                Assert.AreEqual(true, MRE.WaitOne(timeout), "Timed out waiting 1");
               }
                finally
               {server.close();}

            }

        }

        /// <summary>
        ///A test for No Such Host
        ///</summary>
        [TestMethod()]
        public void NoSuchHost()
        {

            new NoSuchHostTest().run();

        }

        /// <summary>
        /// Helper class to test a NoSuchHostEvent
        /// </summary>
        public class NoSuchHostTest
        {

            private ManualResetEvent MRE = new ManualResetEvent(false);
            private int timeout = 1000;
            BoggleServer server;

            public void run()
            {
                try{
                //setup server
                server  = new BoggleServer(80, "dictionary.txt");

                //setup client
                BoggleClientModel target = new BoggleClientModel();
                string IPAddress = "garbageAddress";
                string name = "test";


                target.noSuchHostEvent += delegate(string line)
                {
                    MRE.Set();
                };

                //trigger event
                target.Connect(IPAddress, name);

                Assert.AreEqual(true, MRE.WaitOne(timeout), "Timed out waiting 1");
                }
                finally
                {server.close();}

            }

        }

        /// <summary>
        ///A test for TerminateEvent
        ///</summary>
        [TestMethod()]
        public void TerminateTest()
        {
            new TerminatedEvent().run();

        }

        /// <summary>
        /// Helper class for testing an IncomingTerminatedEvent
        /// </summary>
        public class TerminatedEvent
        {

            private ManualResetEvent MRE = new ManualResetEvent(false);
            private ManualResetEvent MRE1 = new ManualResetEvent(false);
            private int timeout = 1000;
            BoggleServer server;

            /// <summary>
            /// Helper test method
            /// </summary>
            public void run()
            {
                try
                {
                    //setup server
                    server = new BoggleServer(80, "dictionary.txt");

                    //setup clients
                    BoggleClientModel target = new BoggleClientModel();
                    string IPAddress = "127.0.0.1";
                    string name = "test";

                    BoggleClientModel target2 = new BoggleClientModel();
                    string name2 = "test2";

                    target.IncomingStartEvent += delegate(string line)
                    {
                        MRE.Set();
                    };


                    target2.IncomingTerminatedEvent += delegate(string line)
                    {
                        MRE1.Set();
                    };

                    //connect the clients
                    target.Connect(IPAddress, name);
                    target2.Connect(IPAddress, name2);

                    Thread.Sleep(1000);

                    //trigger event
                    target2.terminate();

                    Assert.AreEqual(true, MRE.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual(true, MRE1.WaitOne(timeout), "Timed out waiting 2");
                }
                finally
                { server.close(); }

            }

        }

        /// <summary>
        ///A test for IncomingStopEvent
        ///</summary>
        [TestMethod()]
        public void stopEventTest()
        {
            new StopEvent1().run();

        }


        /// <summary>
        /// Helper class used to test the IncomingStopEvent for a BoggleClientModel
        /// </summary>
        public class StopEvent1
        {

            private ManualResetEvent MRE = new ManualResetEvent(false);
            private ManualResetEvent MRE1 = new ManualResetEvent(false);
            private int timeout = 5000;
            BoggleServer server;

            public void run()
            {
                try
                {
                    //setup server
                    server = new BoggleServer(3, "dictionary.txt");

                    //setup clients
                    BoggleClientModel target = new BoggleClientModel();
                    string IPAddress = "127.0.0.1";
                    string name = "test";

                    BoggleClientModel target2 = new BoggleClientModel();
                    string name2 = "test2";

                    target.IncomingStartEvent += delegate(string line)
                    {
                        MRE.Set();
                    };


                    target2.IncomingStopEvent += delegate(string line)
                    {
                        MRE1.Set();
                    };

                    //connect the clients
                    target.Connect(IPAddress, name);
                    target2.Connect(IPAddress, name2);

                    //trigger event
                    Thread.Sleep(3000);

                    Assert.AreEqual(true, MRE.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual(true, MRE1.WaitOne(timeout), "Timed out waiting 2");
                }
                finally
                {
                    server.close();
                }
                

            }

        }



        /// <summary>
        ///A test for IncomingTimeEvent
        ///</summary>
        [TestMethod()]
        public void TimeEventTest()
        {
            new TimeEvent().run();

        }


        /// <summary>
        /// Helper class used to test the IncomingStopEvent for a BoggleClientModel
        /// </summary>
        public class TimeEvent
        {

            private ManualResetEvent MRE = new ManualResetEvent(false);
            private ManualResetEvent MRE1 = new ManualResetEvent(false);
            private int timeout = 2000;
            BoggleServer server;

            public void run()
            {
                try
                {
                    //setup server
                    server = new BoggleServer(10, "dictionary.txt");

                    //setup clients
                    BoggleClientModel target = new BoggleClientModel();
                    string IPAddress = "127.0.0.1";
                    string name = "test";

                    BoggleClientModel target2 = new BoggleClientModel();
                    string name2 = "test2";

                    target.IncomingTimeEvent += delegate(string line)
                    {
                        MRE.Set();
                    };


                    target2.IncomingTimeEvent += delegate(string line)
                    {
                        MRE1.Set();
                    };

                    //connect the clients
                    target.Connect(IPAddress, name);
                    target2.Connect(IPAddress, name2);

                    //trigger event
                    Thread.Sleep(1000);

                    Assert.AreEqual(true, MRE.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual(true, MRE1.WaitOne(timeout), "Timed out waiting 2");
                }
                finally
                {
                    server.close();
                }


            }

        }

        /// <summary>
        ///A test for IncomingTimeEvent
        ///</summary>
        [TestMethod()]
        public void ScoreEventTest()
        {
            new ScoreEvent().run();

        }


        /// <summary>
        /// Helper class used to test the IncomingStopEvent for a BoggleClientModel
        /// </summary>
        public class ScoreEvent
        {

            private ManualResetEvent MRE = new ManualResetEvent(false);
            private ManualResetEvent MRE1 = new ManualResetEvent(false);
            private int timeout = 2000;
            BoggleServer server;

            public void run()
            {
                try
                {
                    //setup server
                    server = new BoggleServer(10, "dictionary.txt", "AAAAAAAAAAAAAAAA");
                    server.LegalWords.Add("AAA");

                    //setup clients
                    BoggleClientModel target = new BoggleClientModel();
                    string IPAddress = "127.0.0.1";
                    string name = "test";

                    BoggleClientModel target2 = new BoggleClientModel();
                    string name2 = "test2";

                    target.IncomingScoreEvent += delegate(string line)
                    {
                        MRE.Set();
                    };


                    target2.IncomingScoreEvent += delegate(string line)
                    {
                        MRE1.Set();
                    };

                    //connect the clients
                    target.Connect(IPAddress, name);
                    target2.Connect(IPAddress, name2);

                    //trigger event
                    target.SendMessage("WORD AAA");

                    Assert.AreEqual(true, MRE.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual(true, MRE1.WaitOne(timeout), "Timed out waiting 2");
                }
                finally
                {
                    server.close();
                }


            }

        }

       
    }
}
