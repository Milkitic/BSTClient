using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BSTClient
{
    class SignalRServer
    {
        public IDisposable SignalR { get; set; }
        const string ServerURI = "http://localhost:8080";
        /// <summary>
        /// Starts the server and checks for error thrown when another server is already 
        /// running. This method is called asynchronously from Button_Start.
        /// </summary>
        private void StartServer()
        {
            try
            {
                //SignalR = WebApp.Start(ServerURI);
            }
            catch (TargetInvocationException ex)
            {
                throw new Exception($"A server is already running at {ServerURI}", ex);
            }

        }
    }
}
