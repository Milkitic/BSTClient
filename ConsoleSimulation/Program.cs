using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSimulation
{
    internal class Program
    {
        private static SignalRClient _signalRClient;
        private static bool _canExit = false;
        private static int Main(string[] args)
        {
            string origin = null;

            try
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    switch (arg)
                    {
                        case "--origin":
                            origin = CommandHelper.ReadValue<string>(args, ref i);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }

            if (origin == null)
            {
                Console.Error.WriteLine("usage: ." + Path.DirectorySeparatorChar +
                                        Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) +
                                        " --origin [server_origin]");
                return 1;
            }


            _signalRClient = new SignalRClient(origin);
            _signalRClient.RequestExit += SignalRClient_RequestExit;
            try
            {
                _signalRClient.ConnectAsync().Wait();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.InnerException?.Message ?? e.Message);
                return 1;
            }

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Task.Run(async () =>
            {
                while (true)
                {
                    var value = Console.ReadLine();
                    await _signalRClient.Send(value);
                }
            });

            while (!_canExit) Thread.Sleep(100);
            return 0;
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _signalRClient.Connection?.StopAsync().Wait();
            _signalRClient.Connection?.DisposeAsync().Wait();
        }

        private static void SignalRClient_RequestExit()
        {
            _canExit = true;
        }
    }
}
