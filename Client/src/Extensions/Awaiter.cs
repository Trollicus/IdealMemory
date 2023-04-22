using System.Runtime.CompilerServices;

namespace Client.Extensions;

public static class Awaiter
{
    public static TaskAwaiter GetAwaiter(this int task)
        => Task.Delay(task * 1000).GetAwaiter();
}