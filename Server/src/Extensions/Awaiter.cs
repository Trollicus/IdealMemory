using System.Runtime.CompilerServices;

namespace Server.Extensions;

public static class Awaiter
{
    public static TaskAwaiter GetAwaiter(this int task)
        => Task.Delay(task * 1000).GetAwaiter();
}