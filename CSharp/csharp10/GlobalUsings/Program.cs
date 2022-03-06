// net6 c# 10
// 不需要声明 using System.Text.Json;
// 移到了 GlobalUsings.cs 中
var output = JsonSerializer.Serialize(new { a = 1});
Console.WriteLine(output);
