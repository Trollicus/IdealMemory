using System.Text.Json;

namespace Client.Handlers.OpCode;

public class OpCodes
{
    public enum WsOpCodes
    {
        Register = 0,
        Login = 1,
        Logout = 2,
        Heartbeat = 3,
    }
    
    public record WsMessage(WsOpCodes OpCode, Guid SessionId, string Payload)
    {
        public static WsMessage? CreateRegisterMessage(object registerModel) =>
            new WsMessage(WsOpCodes.Register, Guid.Empty, JsonSerializer.Serialize(registerModel));

        public static WsMessage? CreateLoginMessage(object loginModel) =>
            new WsMessage(WsOpCodes.Login, Guid.Empty, JsonSerializer.Serialize(loginModel));

        public static WsMessage? CreateLogoutMessage(Guid sessionId) =>
            new WsMessage(WsOpCodes.Logout, sessionId, "");
    }
}