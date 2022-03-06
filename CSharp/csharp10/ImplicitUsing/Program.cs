// net6 c# 10

// csproj 中需要增加  <ImplicitUsings>enable</ImplicitUsings>， 如果不想启用则需要 <ImplicitUsings>disable</ImplicitUsings>
// 隐式加载的名称空间, 对于 console 项目，可以在 obj/Debug/net6.0 中找到 projectName.GlobalUsings.g.cs 的文件，包含：
// global using global::System;
// global using global::System.Collections.Generic;
// global using global::System.IO;
// global using global::System.Linq;
// global using global::System.Net.Http;
// global using global::System.Threading;
// global using global::System.Threading.Tasks;

// 不同项目类型，默认加载的名称空间会有点不一样

Console.WriteLine("Hello, World!");
