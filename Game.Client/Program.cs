using Game.Client;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace GameClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logManager = new LogManager();

            //If both project will run at the same time so This line wait for Game Server start process 
            Thread.Sleep(2000);

            var deviceId = Guid.NewGuid();
            try
            {
                logManager.LogInfo("Application started", new { deviceId });
                //create client and connect to server
                using var clientWebSocket = new ClientWebSocket();
                var cancellationTokenSource = new CancellationTokenSource();
                var serverUri = new Uri("ws://127.0.0.1:5053/ws-game");
                var webSocketManager = new WebSocketManager(clientWebSocket, serverUri, deviceId, logManager);

                await webSocketManager.ConnectAsync(cancellationTokenSource.Token);
                Console.WriteLine("Connected to the server");
                logManager.LogInfo("Connected to the server", new { deviceId });


                //This line listening server immediately
                await webSocketManager.ListenServerAsync(cancellationTokenSource.Token);

                // This code section is for testing each message type
                while (true)
                {
                    Console.WriteLine("Please enter message type:");
                    var messageType = Console.ReadLine();
                    string message = "";
                    switch (messageType)
                    {
                        case "Login":
                            message = CreateMessage("Login", JsonConvert.SerializeObject(new { DeviceId = deviceId }));
                            break;
                        case "UpdateResources":
                            Console.WriteLine("Please choose resource type (Coins,Rolls)");
                            var updateResourceType = Console.ReadLine();

                            Console.WriteLine($"How many {updateResourceType} do you want to increase?");
                            var updateResourceValue = Console.ReadLine();

                            message = CreateMessage("UpdateResources", JsonConvert.SerializeObject(new { ResourceType = updateResourceType, ResourceValue = updateResourceValue }));
                            break;
                        case "SendGift":
                            Console.WriteLine("Friend player Id");
                            var friendPlayerId = Console.ReadLine();

                            Console.WriteLine("Please choose resource type (Coins,Rolls)");
                            var resourceType = Console.ReadLine();

                            Console.WriteLine($"How many {resourceType} do you want to send gift?");
                            var resourceValue = Console.ReadLine();

                            message = CreateMessage("SendGift", JsonConvert.SerializeObject(new { FriendPlayerId = friendPlayerId, ResourceType = resourceType, ResourceValue = resourceValue }));
                            break;
                        default:
                            Console.WriteLine("Invalid message type");
                            Console.ReadLine();
                            break;
                    }


                    while (clientWebSocket.State == WebSocketState.Open)
                    {
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            await webSocketManager.SendWebSocketMessageAsync(message, cancellationTokenSource.Token);
                            Console.WriteLine("Message sent to the server");
                        }
                        break;
                    }

                    Console.WriteLine("Press Enter to continue");
                    Console.WriteLine("Press ESC to close connection...");
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                        break;
                }

                await webSocketManager.CloseConnectionAsync();
                Console.WriteLine("Connection closed");

                logManager.LogInfo("Connection closed", new
                {
                    deviceId
                });

                cancellationTokenSource.Cancel();
                Console.ReadLine();
            }
            catch (WebSocketException exception)
            {
                Console.WriteLine($"- {exception.Message}");
                logManager.LogError($"Socket exception - {deviceId}", exception);

                Console.Read();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Something went wrong-{exception.Message}");

                logManager.LogError($"Something went wrong-{deviceId}", exception);

                Console.Read();
            }

        }

        private static string CreateMessage(string messageType, string messageBody)
        {
            var messageObject = new SocketMessage
            {
                MessageType = messageType,
                MessageBody = messageBody
            };

            return JsonConvert.SerializeObject(messageObject);
        }

        private static async Task SendWebSocketMessage(ClientWebSocket socket, string message, CancellationToken cancellationToken)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
        }

        private static async Task ReceiveMessageFromServerAsync(ClientWebSocket clientWebSocket, CancellationToken cancellationToken)
        {
            var responseBuffer = new byte[1024];
            while (true)
            {
                var byteReceived = new ArraySegment<byte>(responseBuffer);
                WebSocketReceiveResult response = await clientWebSocket.ReceiveAsync(byteReceived, cancellationToken);
                var responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, response.Count);
                if (response.EndOfMessage)
                    Console.WriteLine(responseMessage);

            }
        }
    }

    public class SocketMessage
    {
        public string MessageType { get; set; }
        public string MessageBody { get; set; }
    }
}