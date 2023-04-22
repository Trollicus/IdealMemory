using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Client.Extensions;
using Client.Handlers.Models;
using Client.Handlers.OpCode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Server.Handlers.Logger;
using Server.Handlers.OpCode;
using OpCodes = Client.Handlers.OpCode.OpCodes;

namespace Client.Handlers.WS;

public class SocketClient
{
    private static readonly IHost Host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder(Environment.GetCommandLineArgs())
        .ConfigureLogging(builder =>
            builder.ClearProviders()
                .AddColorConsoleLogger(_ => { }))
        .Build();

    // ReSharper disable once InconsistentNaming
    private static readonly ILogger logger = Host.Services.GetRequiredService<ILogger<Program>>();

    private static readonly Socket Socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    private readonly IPEndPoint _endPoint = new(IPAddress.Loopback, 1337);

    public async Task ConnectAsync()
    {
        await Socket.ConnectAsync(_endPoint);
        
        logger.LogInformation(3, "Connected to the server!");
        logger.LogInformation(3, "Commands: 'Register', 'Login', 'Logout' & 'Exit'");
        
        while (true)
        {
            var command = Console.ReadLine();
            if (command != null)
            {
                OpCodes.WsMessage? message = CreateMessage(command);
                await SendMessageAsync(Socket, message);
                await ReceiveMessageAsync(Socket);
            }

            if (command == "exit")
            {
                break;
            }
        }

        Socket.Close();
        Socket.Dispose();
    }

    private static OpCodes.WsMessage? CreateMessage(string command)
    {
        return command.ToLower() switch
        {
            "register" => new OpCodes.WsMessage(OpCodes.WsOpCodes.Register, Guid.Empty, JsonSerializer.Serialize(new UserDtOs.RegisterRequest
            {
                Username = "bigtonkat",
                Password = "123456789",
                Email = "use69r@example.com"
            })),
            "login" => new OpCodes.WsMessage(OpCodes.WsOpCodes.Login, Guid.Empty, JsonSerializer.Serialize(new UserDtOs.LoginRequest
            {
                UsernameOrEmail = "bigtonkat",
                Password = "123456789"
            })),
            "logout" => new OpCodes.WsMessage(OpCodes.WsOpCodes.Logout, Guid.NewGuid(), string.Empty),
            _ => null
        };
    }

    
    private static async Task ReceiveMessageAsync(Socket socket)
    {
        byte[] buffer = new byte[512];
        int bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None);

        if (bytesRead > 0)
        {
            OpCodes.WsMessage? receivedMessage =
                JsonSerializer.Deserialize<OpCodes.WsMessage>(buffer.AsSpan(0, bytesRead));

            var output = receivedMessage.OpCode switch
            {
                OpCodes.WsOpCodes.Register => JsonSerializer.Deserialize<UserDtOs.RegisterResponse>(receivedMessage.Payload)?.SuccessMessage,
                OpCodes.WsOpCodes.Login => JsonSerializer.Deserialize<UserDtOs.LoginResponse>(receivedMessage.Payload)?.SuccessMessage,
                _ => $"Unhandled OpCode: {receivedMessage.OpCode}"
            };

            Console.WriteLine(output);
        }
    }

    private static Task SendMessageAsync(Socket socket, OpCodes.WsMessage message) =>
        socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message), SocketFlags.None);

}