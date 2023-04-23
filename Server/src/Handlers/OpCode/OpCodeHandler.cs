using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Server.Extensions;
using Server.Handlers.DataBase.Models;
using Server.Handlers.DataBase.Services;

namespace Server.Handlers.OpCode;

public class OpCodeHandler
{
    private readonly UserServices _userService;
    
    public OpCodeHandler(UserServices userService)
    {
        _userService = userService;
    }

    //TODO: Add Heartbeat
    //TODO: Add Rate limit
    public async Task HandleOpCodeAsync(Socket socket, OpCodes.WsMessage? message)
    {
        OpCodes.WsOpCodes opCode = message.OpCode;
        string payload = message.Payload;
        
        switch (opCode)
        {
            case OpCodes.WsOpCodes.Register:
                await HandleRegisterAsync(socket, payload);
                break;
            case OpCodes.WsOpCodes.Login:
                await HandleLoginAsync(socket, payload);
                break;
            case OpCodes.WsOpCodes.Logout:
                await HandleLogoutAsync(socket, payload);
                break;
            default:
                Console.WriteLine(69);
                break;
        }
    }
    
    private async Task HandleRegisterAsync(Socket socket, string? payload)
    {
        var registerRequest = JsonSerializer.Deserialize<UserDtOs.RegisterRequest>(payload);
        var registerResponse = await _userService.Register(registerRequest?.Email, registerRequest?.Username, registerRequest?.Password);
        
        await SendResponseAsync(socket, OpCodes.WsOpCodes.Register, JsonSerializer.Serialize(registerResponse));
    }

    private async Task HandleLoginAsync(Socket socket, string? payload)
    {
        var loginRequest = JsonSerializer.Deserialize<UserDtOs.LoginRequest>(payload);
        var loginResponse = await _userService.Login(loginRequest?.UsernameOrEmail, loginRequest?.Password, socket);

        await SendResponseAsync(socket, OpCodes.WsOpCodes.Login, JsonSerializer.Serialize(loginResponse));
    }

    private async Task HandleLogoutAsync(Socket socket, string? payload)
    {
        var logoutRequest = JsonSerializer.Deserialize<UserDtOs.LogoutRequest>(payload);
        Console.WriteLine($"HandleLogOutAsync: {logoutRequest.SessionId}, Username: {logoutRequest.Username}");
        var logoutResponse = await _userService.Logout(logoutRequest.SessionId, logoutRequest.Username);
        
        await SendResponseAsync(socket, OpCodes.WsOpCodes.Logout, JsonSerializer.Serialize(logoutResponse));
    }
    
    private byte[] GetMessageBytes(OpCodes.WsMessage message)
    {
        string jsonString = JsonSerializer.Serialize(message);
        return Encoding.UTF8.GetBytes(jsonString);
    }

    private async Task SendResponseAsync(Socket socket, OpCodes.WsOpCodes opCode, string message)
    {
        var responseMessage =
            new OpCodes.WsMessage(opCode, Guid.NewGuid(),message); // Use a new SessionId for the response
        byte[] messageBytes = GetMessageBytes(responseMessage);

        await socket.SendAsync(new ReadOnlyMemory<byte>(messageBytes), SocketFlags.None);
    }
}