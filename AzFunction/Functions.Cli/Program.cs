// -------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------

namespace Functions.Runner
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Entry point for this CLI application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// For coordination with the primary thread
        /// </summary>
        private static readonly ManualResetEvent ReadyToExit = new ManualResetEvent(false);

        /// <summary>
        /// For coordination with the primary thread
        /// </summary>
        private static readonly ManualResetEvent Shutdown = new ManualResetEvent(false);

        /// <summary>
        /// Background task listening for user input
        /// </summary>
        private static BackgroundWorker backgroundWorker;

        /// <summary>
        /// Instance variable
        /// </summary>
        /// <param name="args"></param>
        private static Program program = null;

        /// <summary>
        /// Data container for app settings related to Azure Auth calls
        /// </summary>
        private readonly AzureAdOptions options = new AzureAdOptions();

        /// <summary>
        /// URI endpoint to invoke operations
        /// </summary>
        private readonly string functionEndpoint;

        /// <summary>
        /// URI endpoint to invoke operations
        /// </summary>
        private readonly string function2Endpoint;

        /// <summary>
        /// Logging object
        /// </summary>
        private readonly ILogger logger = null;

        /// <summary>
        /// Prevents a default instance of the <see cref="Program" /> class from being created.
        /// </summary>
        private Program()
        {
            // Initialize ILogger to output to Console
            logger = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            })
            .CreateLogger<Program>();

            // Load configuration from appsettings.json
            var b = new ConfigurationBuilder()
                .AddJsonFile(Constants.AppSettingsFile, optional: true, reloadOnChange: true)
                .Build();

            b.Bind(Constants.AzureAdSettingsSection, options);

            functionEndpoint = b.GetValue<string>(Constants.FunctionsEndpoint);
            function2Endpoint = b.GetValue<string>(Constants.Functions2EndPoint);
        }

        /// <summary>
        /// Program entry point
        /// </summary>
        public static void Main(/*string[] args*/)
        {
            Console.WriteLine("Welcome to Function Invocator.");
            Console.WriteLine("Initializing ...");

            try
            {
                // For console input
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += BackgroundWorker_Listen;
                backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                backgroundWorker.RunWorkerAsync();

                program = new Program();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to initialize.");
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine("Initialization complete.");
            Console.WriteLine("Press Ctrl+C anytime to exit ...\n");

            try
            {
                program.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Shutdown.Set();

            Console.WriteLine("Function Invocator has finished.");
            Console.WriteLine("Press any key to close window.");

            ReadyToExit.WaitOne(900);
            if (backgroundWorker != null)
            {
                backgroundWorker.Dispose();
            }

            Console.ReadKey(true);
        }

        /// <summary>
        /// Listen for keyboard input to shutdown. Ignore everything other than Ctrl+C.
        /// </summary>
        /// <param name="sender">parameter must not be empty</param>
        /// <param name="e">parameter is not used</param>
        private static void BackgroundWorker_Listen(object sender, DoWorkEventArgs e)
        {
            do
            {
                if (Console.KeyAvailable)
                {
                    var cki = Console.ReadKey(true);
                    if (((cki.Modifiers & ConsoleModifiers.Control) != 0) && cki.Key == ConsoleKey.C)
                    {
                        Shutdown.Set();
                        break;
                    }
                }
            }
            while (!Shutdown.WaitOne(600));
        }

        /// <summary>
        /// Used to notify main thread that backgroundworker has completed.
        /// </summary>
        /// <param name="sender">parameter must not be empty</param>
        /// <param name="e">parameter is not used</param>
        private static void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ReadyToExit.Set();
        }

        /// <summary>
        /// Invoke application logic
        /// </summary>
        private void Run()
        {
            Mutex mutex = null;
            int iteration = 0,
                maxIteration = 20;

            Console.Write("Waiting for server process .");
            while (mutex == null && iteration < maxIteration)
            {
                try
                {
                    mutex = Mutex.OpenExisting(Constants.GlobalMutexName);
                }
                catch (Exception)
                {
                    Console.Write(".");
                    iteration++;
                    Thread.Sleep(1000);
                }
            }

            if (mutex == null)
            {
                Console.WriteLine("\nTimed out looking for server process.");
                return;
            }
            else
            {
                mutex.Dispose();
            }

            Console.WriteLine("\nFound the server process. Proceeding forward.");
            Console.WriteLine("\nInvoking function 1 of 2. This should return 'Hello, Bill'.");
            var tr = new TestRunner(logger);
            tr.InvokeHttpTrigger(options, functionEndpoint);

            Console.WriteLine("\nInvoking function 2 of 2. This should get 401 error and return nothing.");
            var tr2 = new TestRunner(logger);
            tr2.InvokeHttpTrigger(options, function2Endpoint);
        }
    }
}
