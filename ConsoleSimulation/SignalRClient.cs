using Microsoft.AspNet.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace ConsoleSimulation
{
    internal class SignalRClient
    {
        public event Action<string> StandardOutputReceived;
        public event Action RequestExit;

        public HubConnection Connection { get; private set; }
        public IHubProxy HubProxy { get; private set; }

        public SignalRClient(string origin)
        {
            Connection = new HubConnection($"{origin}/signalr")
            {
                //DeadlockErrorTimeout = TimeSpan.FromSeconds(3),
                //TransportConnectTimeout = TimeSpan.FromSeconds(3)
            };
            Connection.Closed += Connection_Closed;
            //HubProxy = Connection.CreateHubProxy("MyHub");
            ////Handle incoming event from server: use Invoke to write to console from SignalR's thread
            //HubProxy.On<string, string>("AddMessage", (name, message) => { }
            //);

            HubProxy = Connection.CreateHubProxy("MyHub");
            HubProxy.On<string>("StandardOutput", OnStandardOutput);
            HubProxy.On("RequestExit", OnRequestExitExit);
        }

        public async Task ConnectAsync()
        {
            await Connection.Start();
        }

        public async Task Send(string value)
        {
            await HubProxy.Invoke("StandardInput", value);
        }

        private void OnRequestExitExit()
        {
            RequestExit?.Invoke();
        }

        private void OnStandardOutput(string content)
        {
            StandardOutputReceived?.Invoke(content);
        }

        private void Connection_Closed()
        {
            Console.WriteLine("Connection has been disconnected by server.");
            RequestExit?.Invoke();
        }
    }
}
