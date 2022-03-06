// net6 c# 10
// 之前版本常量中不支持使用插值形式，只能通过如下方式：
// private const string ApiBase = "/api";
// private const string AnotherApi = ApiBase + "/library";

class Program
{
    private const string ApiBase = "/api";
    private const string AnotherApi = $"{ApiBase}/library";

    public static void Main()
    {
        System.Console.WriteLine(AnotherApi);
    }
}