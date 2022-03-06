// net6 c#10

// 用于输出入参的原始形式
using System.Runtime.CompilerServices;

Validate(1 == 2); // 会输出 1 == 2，而不是 false

static void Validate(bool condition, [CallerArgumentExpression("condition")] string? message = null)
{
    if (!condition)
    {
        System.Console.WriteLine($"Argument failed validation: <{message}>");
    }
}