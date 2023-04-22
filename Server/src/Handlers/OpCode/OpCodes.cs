using System.Text.Json.Serialization;

namespace Server.Handlers.OpCode;

public class OpCodes
{
    public enum WsOpCodes
    {
        Register = 0,
        Login = 1,
        Logout = 2,
        Heartbeat = 3,
    }

    public record WsMessage(
        [property: JsonPropertyName("OpCode")] WsOpCodes OpCode,
        [property: JsonPropertyName("SessionId")] Guid SessionId,
        [property: JsonPropertyName("Payload")] string Payload);
}