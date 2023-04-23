using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Extensions;
using Server.Handlers.Logger;
using Server.Handlers.DataBase.Configuration;
using Server.Handlers.DataBase.Services;
using Server.Handlers.OpCode;

namespace Server.Handlers.WS;

public class SocketServer
{
    private static readonly IHost Host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder(Environment.GetCommandLineArgs())
        .ConfigureLogging(builder =>
            builder.ClearProviders()
                .AddColorConsoleLogger(_ => { }))
        .Build();

    // ReSharper disable once InconsistentNaming
    public static readonly ILogger logger = Host.Services.GetRequiredService<ILogger<Program>>();

    private static readonly Socket Socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    private readonly IPEndPoint _endPoint = new(IPAddress.Loopback, 1337);

    private static readonly CancellationTokenSource CancellationTokenSource = new();
    
    private readonly IServiceProvider _serviceProvider;

    public SocketServer()
    {
        var services = new ServiceCollection();
        DatabaseConfiguration.ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    public async Task StartAsync()
    {
        Socket.Bind(_endPoint);
        Socket.Listen(32); //Maximum number of pending connections to queue 
        logger.LogInformation(3, "Server has started!");
        
        while (!CancellationTokenSource.IsCancellationRequested)
        {
            Socket socket = await Socket.AcceptAsync();

            var ipEndPoint = socket.RemoteEndPoint;

            logger.LogInformation(3, "New connection: {@IpEndPoint}", ipEndPoint);

            _ = await Task.Run(() => Task.FromResult(HandleClientAsync(socket, CancellationTokenSource)));
        }
    }

    private async Task HandleClientAsync(Socket socket, CancellationTokenSource token)
    {
        var cancellationToken = token.Token;
        Memory<byte> buffer = new byte[512];

        using var memoryStream = new MemoryStream();

        while (!cancellationToken.IsCancellationRequested)
        {
            int bytes = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
            memoryStream.Write(buffer.Span.Slice(0, bytes));

            logger.LogInformation(3, "{S}", Encoding.UTF8.GetString(buffer.Span.Slice(0, bytes)));

            string jsonString = Encoding.UTF8.GetString(memoryStream.ToArray());
            OpCodes.WsMessage? message = JsonSerializer.Deserialize<OpCodes.WsMessage>(jsonString);

            using var scope = _serviceProvider.CreateScope();
            var userService = scope.ServiceProvider.GetRequiredService<UserServices>();
            
            _ = WhoseOnline(userService, CancellationTokenSource);
            
            var opCodeHandler = new OpCodeHandler(userService);
            await opCodeHandler.HandleOpCodeAsync(socket, message);

            memoryStream.SetLength(0);
        }

        socket.Close();
        socket.Dispose();
    }

    private async Task WhoseOnline(UserServices userServices,CancellationTokenSource cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var loggedInUsers = userServices.GetLoggedInUsers();
            
            await 60;
            
            Console.WriteLine($"Online Users ({loggedInUsers.Count}):");

            foreach (var user in loggedInUsers)
            {
                Console.WriteLine($"Username: {user.Value}, SessionId: {user.Key}");
            }
            
        }
    }
}
