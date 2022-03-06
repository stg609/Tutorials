// net6 c# 10
// 文件级别的 ns 表示不需要再使用花括号把该 ns 范围内的东西包裹起来，这个动机是因为，C# 生态中 99.7% 的代码中一个文件只会有一个 ns

namespace X.Y.Z;

class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello World");
    }
}
