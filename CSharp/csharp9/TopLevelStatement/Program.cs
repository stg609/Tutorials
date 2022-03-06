// net5 c# 9
// 语句可以直接写在文件的最外层，不需要定义 class, Main。 编译器会自动加上 Main

// 注意，使用 TLS 的特性，就不能和 非 TLS 的一起使用，也就是不能再声明 namespace, class 这些

using System;
Console.WriteLine("Hello, World!");
