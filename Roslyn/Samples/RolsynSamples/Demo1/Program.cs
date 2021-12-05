﻿using System;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Demo1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var ret1 = await Evalute1Plus1Async();
            Console.WriteLine(ret1);

            var ret2 = await Evalute1Plus1_ComplexAsync();
            Console.WriteLine(ret2);

            await RunWithExplictUsingAsync();

            await RunWithImportsAsync();

            await RunExpandoObjectAsync();

            await RunExtensionMethodsAsync();

            await RunCustomMethodsWithParamsAsync();

            await ContinueRunCustomMethodsWithParamsAsync();

            await CreateScriptThenRunCustomMethodsWithParamsAsync();
        }

        static async Task<object> Evalute1Plus1Async()
        {
            return await CSharpScript.EvaluateAsync("1+1");
        }


        static async Task<object> Evalute1Plus1_ComplexAsync()
        {
            // var a = 1+1 由于是个代码块，所以必须使用分号
            // Evaluate 代码块 并不会返回，所以最后给一个 b 作为表达式来返回
            return await CSharpScript.EvaluateAsync("var a = 1+1;var b = a+1;b");
        }

        /// <summary>
        /// 需要显示使用 using
        /// </summary>
        /// <returns></returns>
        static async Task RunWithExplictUsingAsync()
        {
            string script = @"
using System; // Console 需要这个名称空间
var coll = new string[]{}; 
foreach(var a in coll) 
{
    Console.WriteLine(a);
}";
            await CSharpScript.RunAsync(script);
        }

        /// <summary>
        /// 通过 WithImports 来使用 using
        /// </summary>
        /// <returns></returns>
        static async Task RunWithImportsAsync()
        {
            string script = @"
var coll = new string[]{}; 
    Console.WriteLine(coll);

foreach(var a in coll) 
{
    Console.WriteLine(a);
}";
            ScriptOptions scriptOptions = ScriptOptions.Default;
            //Add namespaces
            scriptOptions = scriptOptions.WithImports("System"); // WithImports 表示只使用这个 imports
            await CSharpScript.RunAsync(script, scriptOptions);
        }

        static async Task RunExpandoObjectAsync()
        {
            string scripts = @"
dynamic obj = new ExpandoObject();
obj.A = 1;
Console.WriteLine(obj.A);
";

            var expressions = typeof(System.Dynamic.ExpandoObject).Assembly; // 使用 ExpandObject 需要的 dll
            var csharp = typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly; // 使用 dynamic 需要的 dll
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddReferences(expressions, csharp)
                .AddImports("System", "System.Dynamic"); // 与 withImports 不同，这个是在已有的 imports 基础上增加其他 imports

            await CSharpScript.RunAsync(scripts, scriptOptions);
        }

        static async Task RunExtensionMethodsAsync()
        {
            string scripts = @"
dynamic obj = new ExpandoObject();
obj.A = 1;
((ExpandoObject)obj).MyConsole();
";
            var myExt = typeof(Exts).Assembly; // MyConsole 需要这个 dll
            var expressions = typeof(System.Dynamic.ExpandoObject).Assembly; // 使用 ExpandObject 需要的 dll
            var csharp = typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly; // 使用 dynamic 需要的 dll
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddReferences(expressions, csharp, myExt)
                .AddImports("System", "System.Dynamic", "Demo1"); // MyConsole 需要使用这个 namespace

            await CSharpScript.RunAsync(scripts, scriptOptions);
        }

        static async Task RunCustomMethodsWithParamsAsync()
        {
            string scripts = @"
M(""xxx"");

void M(string val) {
    Console.WriteLine(val);
}
";
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddImports("System");

            await CSharpScript.RunAsync(scripts, scriptOptions);
        }

        static async Task ContinueRunCustomMethodsWithParamsAsync()
        {
            string scripts = @"
void M(string val) {
    Console.WriteLine(val);
}
";
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddImports("System");

            var rslt = await CSharpScript.RunAsync(scripts, scriptOptions);

            string script2 = @"M(""secondScript"")";
            await rslt.ContinueWithAsync(script2);
        }

        /// <summary>
        /// 分成2个 script
        /// </summary>
        /// <returns></returns>
        static async Task CreateScriptThenRunCustomMethodsWithParamsAsync()
        {
            string scripts = @"
void M(string val) {
    Console.WriteLine(val);
}
";
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddImports("System");

            var rslt = CSharpScript.Create(scripts, scriptOptions);

            string script2 = @"M(""secondScript"")";
            rslt = rslt.ContinueWith(script2);

            await rslt.RunAsync();


        }
    }

    public static class Exts
    {
        public static void MyConsole(this object obj)
        {
            if (obj != null)
            {
                Console.WriteLine(obj);
            }
        }
    }
}
