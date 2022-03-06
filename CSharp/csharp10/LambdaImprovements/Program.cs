// net6 c# 10

// 1. 自动推断
// c# 9 的时候，委托不支持推断，必须明确声明委托类型：
// Action helloWorld = ()=>System.Console.WriteLine("Hello World");
// 如果想体验 9 ，可以把 csproj 中的 LangVersion 改成 9

// c# 10 支持委托推断
using System;
var helloWorld = () => System.Console.WriteLine("Hello World");

helloWorld();

// 2. 类型声明
// 我们也可以在括号前声明类型来表示返回类型
var text = string? () => null;
